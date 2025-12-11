import requests
from bs4 import BeautifulSoup
import time
import argparse
import re

from DBUtil import execute_bulk_insert, execute_stored_procedure, execute_sql_query

# ユーザーエージェント（他バッチと共通の UA）
HEADERS = {
    "User-Agent": (
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
        "AppleWebKit/537.36 (KHTML, like Gecko) "
        "Chrome/143.0.0.0 Safari/537.36"
    )
}


def truncate_table():
    """
    IF_RaceResult_CSV を TRUNCATE する。
    年をまたいで取得する前提なので、実行ごとに全削除。
    """
    sql_query = "TRUNCATE TABLE if_raceresult_csv"
    execute_sql_query(sql_query)


def insert_race_data_to_database(race_data):
    """
    race_data（ヘッダ行＋明細行）のリストを IF_RaceResult_CSV に一括INSERTする。
    """
    sql_query = """
        INSERT INTO if_raceresult_csv (
            race_id, horse_id, [馬名], [騎手], [馬番], [走破時計], [オッズ], [通過順], [着順], [馬体重], [馬体重変動]
            , [性], [齢], [斤量], [上がり], [人気], [レース名], [日付], [開催], [クラス], [芝_ダート]
            , [距離], [回り], [馬場], [天気], track_id, [場名])
        VALUES (
            %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s
        )
    """
    # race_data[0] はヘッダ行なのでスキップ
    values = [tuple(entry) for entry in race_data[1:]]

    execute_bulk_insert(sql_query, values)


def parse_race_condition_from_spans(soup_span, url):
    """
    span の配列から
    - sur: 芝/ダ など
    - rou: 右/左/直 など
    - dis: 距離（数値文字列）
    - con: 馬場状態（良/稍/重/不 など、先頭1文字を想定）
    - wed: 天候（晴/雨など、先頭1文字を想定）
    を、可能な範囲で安全に取り出す。
    """

    sur = ""
    rou = ""
    dis = ""
    con = ""
    wed = ""

    # 例: "ダ左1400m / 天候 : 晴 / ダート : 良 / 発走 : 10:05"
    # のようなテキストを持つ span を探し、そこからまとめて抜き出す。
    for span in soup_span:
        text = span.get_text(" ", strip=True)
        if not text:
            continue

        # 距離と芝/ダート表記が両方含まれている span だけを候補にする
        if "m" not in text:
            continue
        if ("芝" not in text) and ("ダ" not in text) and ("ダート" not in text) and ("障" not in text):
            continue

        try:
            # --- 芝/ダ/障 ---
            m_sur = re.search(r"(芝|ダート|ダ|障害)", text)
            if m_sur:
                s = m_sur.group(1)
                if s.startswith("芝"):
                    sur = "芝"
                elif "ダ" in s:
                    sur = "ダ"
                else:
                    sur = s[0]  # 障害など

            # --- 距離（数値 + m） ---
            m_dis = re.search(r"(\d+)\s*m", text)
            if m_dis:
                dis = m_dis.group(1)

            # --- 回り（右/左/直） ---
            if "右" in text:
                rou = "右"
            elif "左" in text:
                rou = "左"
            elif "直線" in text or "直" in text:
                rou = "直"

            # --- 天候: 天候 : 晴 の "晴" を取得 ---
            m_w = re.search(r"天候\s*[:：]\s*([^\s\u3000/&<])", text)
            if m_w:
                wed = m_w.group(1)

            # --- 馬場状態: ダート : 良 / 芝 : 稍重 などの 1文字目を取得 ---
            # 例: "... / ダート : 良  / 発走 ..." の "良"
            m_c = re.search(r"(芝|ダート|ダ)\s*[:：]\s*([^\s\u3000/&<])", text)
            if m_c:
                con = m_c.group(2)

            # 何かしら取れていればここで確定
            if sur or dis or wed or con:
                break

        except Exception:
            # 解析に失敗した場合は次の span へ
            continue

    if not (sur or dis or con or wed):
        print(f"[WARN] レース条件情報の解析に失敗: {url}")

    return sur, rou, dis, con, wed


def fill_condition_from_page_text(soup, con, wed, url):
    """
    BeautifulSoup 全体からページテキストを取得し、
    - wed: 天候（天候:晴 など）
    - con: 馬場状態（馬場:良 など）
    を、まだ入っていなければ補完する。
    """

    try:
        page_text = soup.get_text(" ", strip=True)
    except Exception:
        return con, wed

    # --- 天候がまだ空なら、"天候:晴" などを検索 ---
    if not wed:
        m_w = re.search(r"天候\s*[:：]\s*([^\s\u3000/&<])", page_text)
        if m_w:
            wed = m_w.group(1)

    # --- 馬場がまだ空なら、"馬場:良" などを検索 ---
    if not con:
        m_c = re.search(r"馬場\s*[:：]\s*([^\s\u3000/&<])", page_text)
        if m_c:
            con = m_c.group(1)

    return con, wed


def build_race_url(race_id: str) -> str:
    """
    race_id から netkeiba のレース結果ページURLを組み立てる。
    例: 202405030811 → https://db.netkeiba.com/race/202405030811/
    """
    return f"https://db.netkeiba.com/race/{race_id}/"


def debug_single_race(race_id: str):
    """
    開発・調査用:
    - 指定した1レースのページを取得
    - 距離/回り/馬場/天気
    - 馬ごとの 馬名 / horse_id / 馬番
    を標準出力にダンプする。
    DBへのINSERTは行わない。
    """
    url = build_race_url(race_id)
    print(f"[INFO] Debug fetch: {url}")

    try:
        response = requests.get(url, headers=HEADERS, timeout=10)
    except requests.exceptions.RequestException as e:
        print(f"[ERROR] HTTP リクエスト失敗: {e}")
        return

    soup = BeautifulSoup(response.content.decode("euc-jp", "ignore"), "html.parser")

    # レース条件
    soup_span = soup.find_all("span")
    sur, rou, dis, con, wed = parse_race_condition_from_spans(soup_span, url)
    con, wed = fill_condition_from_page_text(soup, con, wed, url)

    print("=== Race condition ===")
    print(f"距離   : {dis}m")
    print(f"芝/ダ : {sur}")
    print(f"回り   : {rou}")
    print(f"馬場   : {con}")
    print(f"天候   : {wed}")
    print()

    # レース結果テーブル
    main_table = soup.find("table", {"class": "race_table_01 nk_tb_common"})
    if main_table is None:
        print("[WARN] race_table_01 が見つかりませんでした。")
        return

    main_rows = main_table.find_all("tr")
    print("=== Horses in this race ===")
    for i, row in enumerate(main_rows[1:], start=1):
        cols = row.find_all("td")
        if not cols:
            continue

        # 馬番
        uma_no = cols[2].text.strip() if len(cols) > 2 else ""

        # 馬名
        horse_name = cols[3].text.strip() if len(cols) > 3 else ""

        # horse_id
        horse_id = ""
        try:
            horse_link_tag = cols[3].find("a")
            if horse_link_tag is not None:
                href = horse_link_tag.get("href", "")
                m = re.search(r"/horse/([^/]+)/", href)
                if m:
                    horse_id = m.group(1)
        except Exception:
            horse_id = ""

        print(f"{i:2d}: 馬番={uma_no}, 馬名={horse_name}, horse_id={horse_id}")

    print("[INFO] debug_single_race finished.")


def scrape_and_insert_race_data(year_start, year_end, place_codes):
    """
    指定された年・場コードの範囲で netkeiba をスクレイピングし、
    IF_RaceResult_CSV → TR_RaceResult まで流し込むための
    IF 側データを作成する。
    """

    # テーブルをTRUNCATE
    truncate_table()

    for year in range(year_start, year_end + 1):

        race_data_all = []
        # 取得するデータのヘッダー情報を先に追加しておく
        race_data_all.append([
            'race_id', 'horse_id', '馬', '騎手', '馬番', '走破時間', 'オッズ', '通過順', '着順',
            '体重', '体重変化', '性', '齢', '斤量', '上がり', '人気', 'レース名',
            '日付', '開催', 'クラス', '芝・ダート', '距離', '回り', '馬場', '天気',
            '場id', '場名'
        ])

        for w in range(len(place_codes)):
            place_code = place_codes[w]
            place = ""
            if place_code == "01":
                place = "札幌"
            elif place_code == "02":
                place = "函館"
            elif place_code == "03":
                place = "福島"
            elif place_code == "04":
                place = "新潟"
            elif place_code == "05":
                place = "東京"
            elif place_code == "06":
                place = "中山"
            elif place_code == "07":
                place = "中京"
            elif place_code == "08":
                place = "京都"
            elif place_code == "09":
                place = "阪神"
            elif place_code == "10":
                place = "小倉"

            # 開催回数分ループ（最大6回想定 → range(7)）
            for z in range(7):

                continueCounter = 0  # 'continue' が実行された回数カウンタ

                # 開催日数分ループ（最大12日想定 → range(13)）
                for y in range(13):

                    if y < 9:
                        race_id_base = f"{year}{place_code}0{z + 1}0{y + 1}"
                    else:
                        race_id_base = f"{year}{place_code}0{z + 1}{y + 1}"

                    # y の更新を break するためのカウンタ（元ロジックを踏襲）
                    yBreakCounter = 0

                    # レース数分ループ（12R）
                    for x in range(12):
                        if x < 9:
                            current_race_id = race_id_base + "0" + str(x + 1)
                        else:
                            current_race_id = race_id_base + str(x + 1)

                        url = build_race_url(current_race_id)

                        try:
                            # ユーザーエージェントとタイムアウトを付与してリクエスト
                            r = requests.get(url, headers=HEADERS, timeout=10)

                        # リクエストを投げすぎるとエラーになることがあるため
                        # 失敗したら10秒待機してリトライする
                        except requests.exceptions.RequestException as e:
                            print(f"Error: {e}")
                            print("Retrying in 10 seconds...")
                            time.sleep(10)
                            r = requests.get(url, headers=HEADERS, timeout=10)

                        # バグ対策で decode
                        soup = BeautifulSoup(r.content.decode("euc-jp", "ignore"), "html.parser")
                        soup_span = soup.find_all("span")

                        # テーブルを指定
                        main_table = soup.find("table", {"class": "race_table_01 nk_tb_common"})

                        # テーブル内の全ての行を取得
                        try:
                            main_rows = main_table.find_all("tr")
                        except Exception:
                            print('continue: ' + url)
                            continueCounter += 1
                            if continueCounter == 2:
                                continueCounter = 0
                                break
                            continue

                        for i, row in enumerate(main_rows[1:], start=1):  # ヘッダ行をスキップ
                            cols = row.find_all("td")

                            # --- 馬ID（/horse/XXXX/ の XXXX 部分）を取得 ---
                            horse_id = ""
                            try:
                                horse_link_tag = cols[3].find("a")  # 馬名セル内の <a> タグ
                                if horse_link_tag is not None:
                                    href = horse_link_tag.get("href", "")
                                    # 例: "/horse/2019101234/" から "2019101234" を抜き出す
                                    match = re.search(r"/horse/([^/]+)/", href)
                                    if match:
                                        horse_id = match.group(1)
                            except Exception:
                                horse_id = ""

                            # 走破時間
                            try:
                                runtime = cols[7].text.strip()
                            except IndexError:
                                runtime = ""

                            # 通過順
                            try:
                                pas = cols[10].text.strip()
                            except Exception:
                                pas = ""

                            # 体重
                            var = cols[14].text.strip()
                            try:
                                weight = int(var.split("(")[0])
                                weight_dif = int(var.split("(")[1][0:-1])
                            except ValueError:
                                weight = 0
                                weight_dif = 0

                            # 上がり
                            try:
                                last = cols[11].text.strip()
                            except IndexError:
                                last = ""

                            # 人気
                            try:
                                pop = cols[13].text.strip()
                            except IndexError:
                                pop = ""

                            # レースの情報（まずは span ベースで）
                            sur, rou, dis, con, wed = parse_race_condition_from_spans(soup_span, url)

                            # まだ埋まっていない天候・馬場を、ページ全体のテキストから補完
                            con, wed = fill_condition_from_page_text(soup, con, wed, url)

                            # 開催情報など
                            soup_smalltxt = soup.find_all("p", class_="smalltxt")
                            detail = str(soup_smalltxt).split(">")[1].split(" ")[1]
                            date = str(soup_smalltxt).split(">")[1].split(" ")[0]
                            clas = str(soup_smalltxt).split(">")[1].split(" ")[2].replace(u'\xa0', u' ').split(" ")[0]
                            title = str(soup.find_all("h1")[1]).split(">")[1].split("<")[0]

                            race_data = [
                                current_race_id,
                                horse_id,              # 馬ID
                                cols[3].text.strip(),  # 馬の名前
                                cols[6].text.strip(),  # 騎手の名前
                                cols[2].text.strip(),  # 馬番
                                runtime,               # 走破時間
                                cols[12].text.strip(), # オッズ
                                pas,                   # 通過順
                                cols[0].text.strip(),  # 着順
                                weight,                # 体重
                                weight_dif,            # 体重変化
                                cols[4].text.strip()[0],  # 性
                                cols[4].text.strip()[1],  # 齢
                                cols[5].text.strip(),     # 斤量
                                last,                 # 上がり
                                pop,                  # 人気
                                title,                # レース名
                                date,                 # 日付
                                detail,
                                clas,                 # クラス
                                sur,                  # 芝かダートか
                                dis,                  # 距離
                                rou,                  # 回り
                                con,                  # 馬場状態
                                wed,                  # 天気
                                w,                    # 場ID
                                place                 # 場名
                            ]
                            race_data_all.append(race_data)

                        # 進捗を表示
                        print(detail + str(x + 1) + "R")

                    if yBreakCounter == 12:
                        break

        # データベースに挿入
        print("DBにInsert...")
        insert_race_data_to_database(race_data_all)


def main():
    """
    コマンドライン引数に応じて動作を切り替えるエントリポイント。

    - --test-race-id が指定された場合:
        固定 race_id で 1件だけ取得して内容をダンプする（開発・調査用）。
    - 指定されていない場合:
        従来どおり year-from/year-to/places に応じてバッチ実行し、
        IF → TR 展開、枠番更新まで行う。
    """
    parser = argparse.ArgumentParser(description="netkeiba レース結果スクレイピング")
    parser.add_argument(
        "--test-race-id",
        type=str,
        required=False,
        help="テスト用に 1レースだけ指定して取得する race_id（例: 202405010101）",
    )
    parser.add_argument(
        "--year-from",
        type=int,
        required=False,
        help="取得開始年（西暦）"
    )
    parser.add_argument(
        "--year-to",
        type=int,
        required=False,
        help="取得終了年（西暦・含む）"
    )
    parser.add_argument(
        "--places",
        type=str,
        required=False,
        default="01,02,03,04,05,06,07,08,09,10",
        help="取得対象の場コードをカンマ区切りで指定（例: 05,06）"
    )

    args = parser.parse_args()

    # 1) 固定 race_id で 1件だけ試すモード（開発・調査用）
    if args.test_race_id:
        debug_single_race(args.test_race_id)
        return

    # 2) バッチモード（従来どおり）
    if args.year_from is None or args.year_to is None:
        parser.error("--year-from と --year-to は、--test-race-id を指定しない場合は必須です。")

    # 場コード文字列 "05,06" → ["05", "06"] に変換
    place_codes = [code.strip() for code in args.places.split(",") if code.strip() != ""]

    # 処理の開始時刻を記録
    start_time = time.time()

    # 本処理
    scrape_and_insert_race_data(args.year_from, args.year_to, place_codes)

    # IF → TR
    print("TRテーブルに展開...")
    execute_stored_procedure("Insert_Into_TRData")

    # 枠番更新
    print("枠番更新...")
    execute_stored_procedure("UpdateFrameNumbers")

    # 処理の終了時刻を記録
    end_time = time.time()

    # 総処理時間を計算
    elapsed_seconds = int(end_time - start_time)
    elapsed_hours = elapsed_seconds // 3600
    elapsed_minutes = (elapsed_seconds % 3600) // 60
    elapsed_remain = elapsed_seconds % 60

    print(f"Total processing time: {elapsed_hours} 時間, {elapsed_minutes} 分, {elapsed_remain} 秒")
    print("終了")


if __name__ == "__main__":
    print("ScrapeRaceToDB: script started")
    main()
