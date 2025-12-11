import argparse
import re

from DBUtil import get_database_connection, execute_bulk_insert
from common import (
    SleepController,
    fetch_soup,
    build_race_url,
    log_info,
    log_warn,
    log_error,
)
BETTYPE_TANSHO = "単勝"
BETTYPE_FUKUSHO = "複勝"


def fetch_target_race_ids(max_count: int):
    """
    払戻未取得の race_id 一覧を取得する。

    条件:
      ・TR_raceresult に存在する race_id
      ・TR_Payout に 「式別=単勝 or 複勝」が 1件も無い race_id
    """
    sql = """
        SELECT DISTINCT
            r.race_id
        FROM TR_raceresult r
        WHERE NOT EXISTS (
            SELECT 1
            FROM TR_Payout p
            WHERE p.race_id = r.race_id
              AND p.式別 IN (N'単勝', N'複勝')
        )
        ORDER BY r.race_id
    """

    connection = get_database_connection()
    try:
        race_ids = []

        # TR_raceresult の race_id は CHAR(12) なので文字列のまま返す
        with connection.cursor(as_dict=True) as cursor:
            cursor.execute(sql)
            for row in cursor:
                race_id = row["race_id"]
                if race_id is not None:
                    race_ids.append(str(race_id))

        if max_count is not None and max_count > 0 and len(race_ids) > max_count:
            race_ids = race_ids[:max_count]

        return race_ids

    finally:
        connection.close()


def fetch_race_page(race_id: str, sleeper=None):
    """
    1レースぶんの結果ページを取得し、BeautifulSoup オブジェクトを返す。

    ・接続エラーやステータスコード異常時は None を返す
    """
    url = build_race_url(race_id)
    soup = fetch_soup(url, sleeper=sleeper)
    if soup is None:
        log_error(f"race_id={race_id}: ページ取得に失敗しました。")
    return soup



def _parse_int_from_text(text: str):
    """
    '240円', '3人気', ' 1 ' などから数値部分だけを取り出して int に変換する。
    数値が見つからなければ None を返す。
    """
    if text is None:
        return None

    cleaned = re.sub(r"[^\d]", "", text)
    if cleaned == "":
        return None

    try:
        return int(cleaned)
    except ValueError:
        return None


def _split_digit_tokens(text: str):
    """
    与えられた文字列を空白で分割し、
    「数字を1つ以上含む」トークンだけを残して返す。

    例:
      "8 16 1"              -> ["8", "16", "1"]
      "320 540 210"         -> ["320", "540", "210"]
      "5人気 10人気 2人気"  -> ["5人気", "10人気", "2人気"]
    """
    if text is None:
        return []

    text = text.strip()
    if text == "":
        return []

    raw_tokens = re.split(r"\s+", text)
    tokens = []

    for token in raw_tokens:
        if re.search(r"\d", token):
            tokens.append(token)

    return tokens


def parse_payouts_single_race(soup: BeautifulSoup):
    """
    レース結果ページ内の「払戻情報」テーブルから
    単勝・複勝の情報だけを抜き出し、リストで返す。

    戻り値のリスト要素イメージ:
    {
        "式別": "単勝" or "複勝",
        "馬番1": 1,
        "馬番2": None,
        "馬番3": None,
        "払戻金": 240,
        "人気": 1,
    }
    """
    results = []

    # 1) 払戻テーブルを特定する
    payout_table = None
    for table in soup.find_all("table"):
        text = table.get_text(separator=" ", strip=True)
        if "単勝" in text and "複勝" in text:
            payout_table = table
            break

    if payout_table is None:
        log_warn("払戻情報テーブルが見つかりませんでした。")
        return results

    current_bet_type = None

    # 2) 行ごとに処理
    for row in payout_table.find_all("tr"):
        cells = row.find_all(["th", "td"])
        if not cells:
            continue

        # 先頭セルが式別（単勝 / 複勝 など）
        bet_label = cells[0].get_text(separator=" ", strip=True)

        # 空行（複勝の2行目など）は前行の式別を引き継ぐ
        if bet_label != "":
            current_bet_type = bet_label

        # 単勝・複勝以外の行はスキップ
        if current_bet_type not in (BETTYPE_TANSHO, BETTYPE_FUKUSHO):
            continue

        data_cells = cells[1:]
        if not data_cells:
            continue

        # 通常「馬番」「払戻金」「人気」の3列構成
        if len(data_cells) < 2:
            continue

        horse_cell = data_cells[0]
        payout_cell = data_cells[1]
        popularity_cell = data_cells[2] if len(data_cells) >= 3 else None

        horse_text = horse_cell.get_text(separator=" ", strip=True)
        payout_text = payout_cell.get_text(separator=" ", strip=True)
        popularity_text = (
            popularity_cell.get_text(separator=" ", strip=True)
            if popularity_cell is not None
            else None
        )

        horse_tokens = _split_digit_tokens(horse_text)
        payout_tokens = _split_digit_tokens(payout_text)
        pop_tokens = _split_digit_tokens(popularity_text) if popularity_text else []

        if not horse_tokens or not payout_tokens:
            continue

        count = min(len(horse_tokens), len(payout_tokens))

        for idx in range(count):
            horse_number = _parse_int_from_text(horse_tokens[idx])
            payout = _parse_int_from_text(payout_tokens[idx])

            popularity = None
            if idx < len(pop_tokens):
                popularity = _parse_int_from_text(pop_tokens[idx])

            if horse_number is None or payout is None:
                continue

            record = {
                "式別": current_bet_type,
                "馬番1": horse_number,
                "馬番2": None,
                "馬番3": None,
                "払戻金": payout,
                "人気": popularity,
            }
            results.append(record)

    return results


def insert_payout_records(race_id: str, payout_list):
    """
    1レースぶんの払戻情報を TR_Payout に一括INSERTする。
    """
    if not payout_list:
        return

    sql = """
        INSERT INTO TR_Payout
            (race_id, 式別, 馬番1, 馬番2, 馬番3, 払戻金, 人気)
        VALUES (%s, %s, %s, %s, %s, %s, %s)
    """

    values = []
    for p in payout_list:
        values.append(
            (
                race_id,
                p["式別"],
                p.get("馬番1"),
                p.get("馬番2"),
                p.get("馬番3"),
                p["払戻金"],
                p.get("人気"),
            )
        )

    try:
        execute_bulk_insert(sql, values)
        print(f"[INFO] INSERT 完了: race_id={race_id}, 件数={len(values)}")
    except Exception as e:
        print(f"[ERROR] INSERT 失敗: race_id={race_id}, error={e}")


def process_payouts(
    max_count: int,
    per_request_sec: float = 0.5,
    batch_size: int = 50,
    wait_minutes: int = 30,
):
    """
    払戻未取得レースを対象に、単勝・複勝の払戻を取得して TR_Payout にINSERTする。

    max_count      : 今回処理する最大レース数
    per_request_sec: 1レースごとのリクエスト間隔（秒）
    batch_size     : 何レースごとにインターバルを入れるか
    wait_minutes   : インターバル（分）
    """
    log_info("払戻未取得レースを検索中...")
    race_ids = fetch_target_race_ids(max_count=max_count)
    log_info(f"対象 race_id 件数: {len(race_ids)}")

    if not race_ids:
        log_info("処理対象レースはありません。終了します。")
        return

    sleeper = SleepController(
        per_request_sec=per_request_sec,
        batch_size=batch_size,
        batch_interval_sec=wait_minutes * 60,
    )

    processed = 0

    for race_id in race_ids:
        soup = fetch_race_page(race_id, sleeper=sleeper)
        if soup is None:
            log_error(f"race_id={race_id}: ページ取得に失敗したためスキップします。")
            continue

        payouts = parse_payouts_single_race(soup)
        if not payouts:
            log_warn(f"race_id={race_id}: 単勝・複勝の払戻が見つかりませんでした。")
            continue

        insert_payout_records(race_id, payouts)
        processed += 1

    log_info(f"今回の処理完了。処理レース数 = {processed}")


def test_single_race(race_id: str):
    """
    開発・調査用：指定した1レースの払戻情報をパースして標準出力にダンプする。
    DBへのINSERTは行わない。
    """
    soup = fetch_race_page(race_id)
    if soup is None:
        log_error("ページ取得に失敗しました。")
        return

    payouts = parse_payouts_single_race(soup)
    print("=== parsed payouts (単勝・複勝) ===")
    for p in payouts:
        print(
            f"式別={p['式別']}, 馬番1={p['馬番1']}, 払戻金={p['払戻金']}, 人気={p['人気']}"
        )
    log_info(f"レコード数: {len(payouts)}")


def main():
    """
    コマンドライン引数に応じて動作を切り替えるエントリポイント。

    - --test-race-id が指定された場合:
        固定 race_id で 1件だけ取得して内容をダンプ（開発・調査用）。
    - 指定されていない場合:
        TR_raceresult / TR_Payout を突合して、
        払戻未取得 race_id を先頭から max_count 件だけ取得し、DBにINSERT。
    """
    parser = argparse.ArgumentParser(description="レース払戻情報取得バッチ")
    parser.add_argument(
        "--test-race-id",
        type=str,
        required=False,
        help="テスト用に 1レースだけ指定して取得する race_id（例: 202406050811）",
    )
    parser.add_argument(
        "--max-count",
        type=int,
        default=100,
        help="今回の実行で処理する最大レース数（デフォルト100件）",
    )
    parser.add_argument(
        "--per-request-sec",
        type=float,
        default=0.5,
        help="1レースごとのリクエスト間隔（秒）（デフォルト 0.5秒）",
    )
    parser.add_argument(
        "--batch-size",
        type=int,
        default=50,
        help="何レースごとにインターバルを入れるか（デフォルト50レース）",
    )
    parser.add_argument(
        "--wait-minutes",
        type=int,
        default=30,
        help="インターバルの長さ（分）（デフォルト30分）",
    )

    args = parser.parse_args()

    if args.test_race_id:
        log_info("固定 race_id でテスト実行します。")
        test_single_race(args.test_race_id)
        log_info("テスト終了。")
        return

    process_payouts(
        max_count=args.max_count,
        per_request_sec=args.per_request_sec,
        batch_size=args.batch_size,
        wait_minutes=args.wait_minutes,
    )


if __name__ == "__main__":
    log_info("payout.py: script started")
    main()
