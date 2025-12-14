import argparse
import datetime
import re
from typing import List, Tuple

from bs4 import BeautifulSoup

from DBUtil import execute_sql_query, execute_bulk_insert
from common import fetch_soup, SleepController, log_info, log_warn, log_error


WIN5_BASE_URL = "https://race.netkeiba.com/top/win5.html?date={date}"
WIN5_RESULTS_URL = (
    "https://race.netkeiba.com/top/win5_results.html?select=win5_results&year={year}"
)


def build_win5_url(date_str: str) -> str:
    """
    YYYYMMDD 形式の文字列から WIN5 対象レースページの URL を組み立てる。
    """
    return WIN5_BASE_URL.format(date=date_str)


def build_win5_results_url(year: int) -> str:
    """
    年を指定して「過去のWIN5」一覧ページの URL を組み立てる。
    """
    return WIN5_RESULTS_URL.format(year=year)


def parse_win5_page(
    soup: BeautifulSoup, date_str: str, debug: bool = False
) -> List[Tuple[str, int]]:
    """
    WIN5 ページ (win5.html?date=YYYYMMDD) の HTML から
    race_id と leg_no(1〜5) を抽出する。

    戻り値:
        [(race_id, leg_no), ...]   leg_no は 1〜5
        レースが存在しない場合は空リスト。
    """
    pattern = re.compile(r"/race/result\.html\?race_id=(\d+)")
    race_ids: List[str] = []

    # ページ内のすべての a タグから race/result リンクを探す
    for a in soup.find_all("a", href=True):
        href = a["href"]
        m = pattern.search(href)
        if not m:
            continue

        rid = m.group(1)
        if rid not in race_ids:
            race_ids.append(rid)

    if debug:
        log_info(f"[DEBUG] {date_str} のページから抽出した race_id 一覧: {race_ids}")

    if len(race_ids) == 0:
        # この日付は WIN5 が行われていない可能性が高い
        log_info(f"{date_str}: WIN5 対象レースは検出されませんでした。")
        return []

    if len(race_ids) < 5:
        # 一応警告を出しつつ、取得できた分だけ返す
        log_warn(
            f"{date_str}: race_id が {len(race_ids)} 件しか見つかりませんでした。想定は 5 件です。"
        )

    # 上から順番に leg_no 1〜5 を付与（6件以上あれば先頭5件だけ利用）
    result: List[Tuple[str, int]] = []
    for idx, rid in enumerate(race_ids[:5]):
        leg_no = idx + 1
        result.append((rid, leg_no))

    return result


def fetch_win5_dates_for_year(
    year: int, sleeper: SleepController, debug: bool = False
) -> List[datetime.date]:
    """
    「過去のWIN5」ページ (win5_results.html?year=YYYY) から、
    その年に WIN5 が実施された日付の一覧を取得する。

    戻り値:
        datetime.date のリスト（昇順）。
    """
    url = build_win5_results_url(year)
    log_info(f"{year}: WIN5結果一覧ページ取得開始 → {url}")

    soup = fetch_soup(url, sleeper=sleeper, decode="utf-8")
    if soup is None:
        log_error(f"{year}: WIN5結果一覧ページの取得に失敗しました。")
        return []

    # href 内の "win5.html?date=YYYYMMDD" を全て拾う
    pattern = re.compile(r"win5\.html\?date=(\d{8})")
    date_str_set = set()

    for a in soup.find_all("a", href=True):
        href = a["href"]
        m = pattern.search(href)
        if not m:
            continue
        date_str = m.group(1)
        date_str_set.add(date_str)

    if debug:
        log_info(
            f"[DEBUG] {year} 年の WIN5 実施日候補 (YYYYMMDD): {sorted(date_str_set)}"
        )

    dates: List[datetime.date] = []
    for date_str in sorted(date_str_set):
        try:
            d = datetime.datetime.strptime(date_str, "%Y%m%d").date()
            dates.append(d)
        except ValueError:
            log_warn(f"{year}: 日付文字列の解析に失敗しました: {date_str}")

    log_info(f"{year}: WIN5 実施日として {len(dates)} 日を検出しました。")
    return dates


def delete_existing_for_date(win5_date: datetime.date) -> None:
    """
    特定日付の MT_Win5Target レコードを一度削除する。
    （再取得時の重複エラー防止）
    """
    sql = "DELETE FROM MT_Win5Target WHERE win5_date = %s"
    execute_sql_query(sql, (win5_date,))
    log_info(f"{win5_date}: 既存 MT_Win5Target レコードを削除しました。")


def insert_win5_records(
    win5_date: datetime.date, rows: List[Tuple[str, int]]
) -> None:
    """
    MT_Win5Target にレコードを INSERT する。
    rows: [(race_id, leg_no), ...]
    """
    if not rows:
        return

    sql = """
        INSERT INTO MT_Win5Target (race_id, win5_date, leg_no)
        VALUES (%s, %s, %s)
    """

    values = []
    for race_id, leg_no in rows:
        values.append((race_id, win5_date, leg_no))

    execute_bulk_insert(sql, values)
    log_info(f"{win5_date}: MT_Win5Target に {len(values)} 件 INSERT しました。")


def process_single_date(
    win5_date: datetime.date,
    sleeper: SleepController,
    dry_run: bool = False,
    debug: bool = False,
) -> None:
    """
    単一日付について WIN5 対象レースを取得し、MT_Win5Target を更新する。

    dry_run=True の場合は DB 更新せずログ出力のみ。
    """
    date_str = win5_date.strftime("%Y%m%d")
    url = build_win5_url(date_str)
    log_info(f"{date_str}: WIN5 ページ取得開始 → {url}")

    soup = fetch_soup(url, sleeper=sleeper, decode="utf-8")
    if soup is None:
        log_error(f"{date_str}: ページ取得に失敗しました。")
        return

    rows = parse_win5_page(soup, date_str, debug=debug)
    if not rows:
        # この日付は WIN5 実施なしとみなす
        return

    if dry_run:
        log_info(f"{date_str}: dry-run のため DB 更新は行いません。取得レース: {rows}")
        return

    # 既存を削除してから INSERT
    delete_existing_for_date(win5_date)
    insert_win5_records(win5_date, rows)


# ★ run_all から呼びやすいようにまとめた関数
def update_win5_master(
    year_from: int,
    year_to: int,
    per_request_sec: float = 0.5,
    batch_size: int = 20,
    batch_wait_minutes: int = 30,
    dry_run: bool = False,
    debug: bool = False,
) -> None:
    """
    指定年範囲の WIN5 実施日を一覧ページから取得し、
    各日の WIN5 対象レース (race_id, leg_no) を MT_Win5Target に反映する。

    ・per_request_sec: 1URL取得ごとのスリープ秒数（S）
    ・batch_size     : 何URLごとにバッチ休憩を入れるか（◯）
    ・batch_wait_minutes:
                       : バッチ休憩の長さ（分）（M）
    """
    sleeper = SleepController(
        per_request_sec=per_request_sec,
        batch_size=batch_size,
        batch_interval_sec=batch_wait_minutes * 60,
    )

    all_dates: List[datetime.date] = []

    for year in range(year_from, year_to + 1):
        dates_for_year = fetch_win5_dates_for_year(year, sleeper=sleeper, debug=debug)
        all_dates.extend(dates_for_year)

    # 年範囲全体で重複を除去し、昇順に並べる
    all_dates = sorted(set(all_dates))
    log_info(
        f"{year_from}〜{year_to} 年の WIN5 実施日として合計 {len(all_dates)} 日を検出しました。"
    )

    for win5_date in all_dates:
        process_single_date(
            win5_date,
            sleeper=sleeper,
            dry_run=dry_run,
            debug=debug,
        )

    log_info("WIN5 マスタ取得処理を終了しました。")


def main():
    parser = argparse.ArgumentParser(description="WIN5 対象レース（MT_Win5Target）取得バッチ")

    parser.add_argument(
        "--year-from",
        type=int,
        help="取得開始年（例: 2011）",
    )
    parser.add_argument(
        "--year-to",
        type=int,
        help="取得終了年（省略時は year-from と同じ）",
    )
    parser.add_argument(
        "--per-request-sec",
        type=float,
        default=0.5,
        help="1リクエストごとの待機秒数（デフォルト 0.5秒）",
    )
    parser.add_argument(
        "--batch-size",
        type=int,
        default=20,
        help="何リクエストごとにバッチ休憩を入れるか（デフォルト 20）",
    )
    parser.add_argument(
        "--batch-wait-minutes",
        type=int,
        default=30,
        help="バッチ休憩の長さ（分）（デフォルト 30分）",
    )
    parser.add_argument(
        "--test-date",
        type=str,
        help="単一日付のみ処理するテストモード（YYYYMMDD）。DB更新あり / なしは --dry-run で制御。",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="DB 更新を行わず、取得結果のログ出力だけ行う。",
    )
    parser.add_argument(
        "--debug",
        action="store_true",
        help="パース結果などの追加デバッグログを出力する。",
    )

    args = parser.parse_args()

    # --- テストモード（単一日付） ---
    if args.test_date:
        try:
            win5_date = datetime.datetime.strptime(args.test_date, "%Y%m%d").date()
        except ValueError:
            log_error(
                f"--test-date の形式が不正です: {args.test_date}（YYYYMMDD で指定してください）"
            )
            return

        sleeper = SleepController(
            per_request_sec=args.per_request_sec,
            batch_size=args.batch_size,
            batch_interval_sec=args.batch_wait_minutes * 60,
        )

        process_single_date(
            win5_date,
            sleeper=sleeper,
            dry_run=args.dry_run,
            debug=args.debug,
        )
        log_info("WIN5 テスト処理を終了しました。")
        return

    # --- 通常モード（year-from / year-to 指定） ---
    year_from = args.year_from
    year_to = args.year_to if args.year_to is not None else args.year_from

    if year_from is None:
        log_error("通常モードでは --year-from の指定が必須です。")
        return

    if year_to is None:
        year_to = year_from

    update_win5_master(
        year_from=year_from,
        year_to=year_to,
        per_request_sec=args.per_request_sec,
        batch_size=args.batch_size,
        batch_wait_minutes=args.batch_wait_minutes,
        dry_run=args.dry_run,
        debug=args.debug,
    )


if __name__ == "__main__":
    main()
