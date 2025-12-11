import argparse
import time
import requests
from bs4 import BeautifulSoup
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

from DBUtil import get_database_connection, execute_sql_query


# ユーザーエージェント（ScrapeRaceToDB.py と同等のものを使用）
HEADERS = {
    "User-Agent": (
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
        "AppleWebKit/537.36 (KHTML, like Gecko) "
        "Chrome/143.0.0.0 Safari/537.36"
    )
}


def fetch_tr_horse_ids():
    """
    TR_raceresult から horse_id と 馬名 の一覧を取得する。

    ・horse_id が NULL のものは除外
    ・戻り値: [(horse_id, 馬名), ...] のリスト
    """
    sql = """
        SELECT DISTINCT
            horse_id,
            [馬名]
        FROM TR_raceresult
        WHERE horse_id IS NOT NULL
    """

    connection = get_database_connection()
    try:
        results = []

        with connection.cursor(as_dict=True) as cursor:
            cursor.execute(sql)
            for row in cursor:
                horse_id = str(row["horse_id"])
                horse_name = row["馬名"]
                results.append((horse_id, horse_name))

        return results

    finally:
        connection.close()


def fetch_pedigree_horse_ids():
    """
    MT_HorsePedigree から既に登録済みの horse_id を取得する。

    ・戻り値: set([horse_id, ...])
    """
    sql = """
        SELECT horse_id
        FROM MT_HorsePedigree
    """

    connection = get_database_connection()
    try:
        horse_ids = set()

        with connection.cursor(as_dict=True) as cursor:
            cursor.execute(sql)
            for row in cursor:
                horse_id = str(row["horse_id"])
                horse_ids.add(horse_id)

        return horse_ids

    finally:
        connection.close()


def build_horse_url(horse_id: str) -> str:
    """
    horse_id から netkeiba の馬ページURLを組み立てる。

    例: horse_id = '2019103422'
        → 'https://db.netkeiba.com/horse/2019103422/'
    """
    return f"https://db.netkeiba.com/horse/{horse_id}/"


def fetch_horse_page(horse_id: str):
    """
    1頭ぶんの馬ページを取得し、BeautifulSoup オブジェクトを返す。

    ・接続に失敗した場合は None を返す
    """
    url = build_horse_url(horse_id)
    print(f"[INFO] Fetching: {url}")

    try:
        response = requests.get(url, headers=HEADERS, timeout=10)
    except requests.exceptions.RequestException as e:
        print(f"[ERROR] Request failed: {e}")
        return None

    # netkeiba は EUC-JP ベースのため decode してからパース
    html = response.content.decode("euc-jp", "ignore")
    soup = BeautifulSoup(html, "html.parser")
    return soup


def fetch_horse_page_selenium(horse_id: str):
    """
    Selenium を使って馬ページを開き、JavaScript 実行後の HTML を取得する。

    ・血統タブ（table.blood_table）が出現するまで待機してから page_source を返す。
    ・戻り値: BeautifulSoup オブジェクト（失敗時は None）
    """
    url = build_horse_url(horse_id)
    print(f"[INFO][SELENIUM] Fetching: {url}")

    # Chrome をヘッドレスモード（画面を出さないモード）で起動
    options = Options()
    options.add_argument("--headless=new")
    options.add_argument("--disable-gpu")
    options.add_argument("--no-sandbox")

    driver = webdriver.Chrome(options=options)

    try:
        driver.get(url)

        # 血統タブの table.blood_table が現れるまで最大10秒待機
        try:
            WebDriverWait(driver, 10).until(
                EC.presence_of_element_located(
                    (By.CSS_SELECTOR, "table.blood_table")
                )
            )
        except Exception:
            print("[WARN] blood_table が見つかりませんでした（タイムアウト）。")

        html = driver.page_source
        soup = BeautifulSoup(html, "html.parser")
        return soup

    finally:
        driver.quit()


def extract_pedigree_2gen(soup):
    """
    血統テーブル (table.blood_table) から
      父 / 母 / 父父 / 父母 / 母父 / 母母
    を抜き出して返す。

    取得できなかったものは空文字を返す。
    """
    father = mother = ""
    father_father = father_mother = ""
    mother_father = mother_mother = ""

    blood_table = soup.find("table", class_="blood_table")
    if blood_table is None:
        return father, mother, father_father, father_mother, mother_father, mother_mother

    rows = blood_table.find_all("tr")

    # 1行目: 父, 父父
    if len(rows) > 0:
        first_row = rows[0]
        cells = first_row.find_all("td")

        # 父（左側、class=b_ml, rowspan=2）
        father_cell = first_row.find("td", class_="b_ml")
        if father_cell:
            link = father_cell.find("a")
            father = (link.get_text(strip=True) if link else father_cell.get_text(strip=True))

        # 父父（右側のセル）
        if len(cells) > 1:
            ff_cell = cells[1]
            link = ff_cell.find("a")
            father_father = (link.get_text(strip=True) if link else ff_cell.get_text(strip=True))

    # 2行目: 父母
    if len(rows) > 1:
        second_row = rows[1]
        fm_cell = second_row.find("td", class_="b_fml")
        if fm_cell:
            link = fm_cell.find("a")
            father_mother = (link.get_text(strip=True) if link else fm_cell.get_text(strip=True))

    # 3行目: 母, 母父
    if len(rows) > 2:
        third_row = rows[2]
        cells = third_row.find_all("td")

        # 母（左側、class=b_fml, rowspan=2）
        mother_cell = third_row.find("td", class_="b_fml")
        if mother_cell:
            link = mother_cell.find("a")
            mother = (link.get_text(strip=True) if link else mother_cell.get_text(strip=True))

        # 母父（右側のセル）
        if len(cells) > 1:
            mf_cell = cells[1]
            link = mf_cell.find("a")
            mother_father = (link.get_text(strip=True) if link else mf_cell.get_text(strip=True))

    # 4行目: 母母
    if len(rows) > 3:
        fourth_row = rows[3]
        mm_cell = fourth_row.find("td", class_="b_fml")
        if mm_cell:
            link = mm_cell.find("a")
            mother_mother = (link.get_text(strip=True) if link else mm_cell.get_text(strip=True))

    return father, mother, father_father, father_mother, mother_father, mother_mother


def process_unregistered_horses(max_count: int, batch_size: int = 100, wait_minutes: int = 30):
    """
    TR_raceresult / MT_HorsePedigree を突合し、
    「TR にいて MT にいない horse_id」を対象に、
    血統情報(父・母・祖父母)を取得して MT_HorsePedigree にINSERTする。

    max_count    : 今回の実行で何頭まで処理するか（安全のため上限）
    batch_size   : 何頭ごとにインターバルを入れるか
    wait_minutes : インターバルの長さ（分）
    """
    print("[INFO] TR_raceresult から horse_id 一覧を取得中...")
    tr_horses = fetch_tr_horse_ids()
    print(f"[INFO] TR_raceresult 側 horse_id 件数: {len(tr_horses)}")

    print("[INFO] MT_HorsePedigree から既存 horse_id を取得中...")
    mt_ids = fetch_pedigree_horse_ids()
    print(f"[INFO] MT_HorsePedigree 側 horse_id 件数: {len(mt_ids)}")

    # 差分抽出
    target_horses = []
    for horse_id, horse_name in tr_horses:
        if horse_id is None:
            continue
        if horse_id not in mt_ids:
            target_horses.append((horse_id, horse_name))

    print(f"[INFO] 未登録（TR にいて MT にいない）horse_id 件数: {len(target_horses)}")

    if not target_horses:
        print("[INFO] 新規に取得すべき horse_id はありません。処理を終了します。")
        return

    processed = 0

    for horse_id, horse_name in target_horses:
        if processed >= max_count:
            print(f"[INFO] max_count={max_count} に達したため処理を終了します。")
            break

        print("======================================")
        print(f"[INFO] 処理対象 horse_id = {horse_id}, 馬名 = {horse_name}")

        # 馬ページ取得（Selenium）
        soup = fetch_horse_page_selenium(horse_id)
        if soup is None:
            print("[WARN] soup が取得できなかったためスキップします。")
            continue

        # 血統抽出
        father, mother, ff, fm, mf, mm = extract_pedigree_2gen(soup)

        # 何も取れなかった場合はスキップ（念のため）
        if not father and not mother and not ff and not fm and not mf and not mm:
            print("[WARN] 血統情報が取得できなかったためスキップします。")
            continue

        # INSERT
        insert_pedigree_record(
            horse_id=horse_id,
            horse_name=horse_name,
            father=father,
            father_father=ff,
            father_mother=fm,
            mother=mother,
            mother_father=mf,
            mother_mother=mm,
        )

        processed += 1

        # 負荷対策：batch_size 頭ごとにインターバル
        if processed % batch_size == 0:
            print(f"[INFO] {processed}頭処理したので {wait_minutes}分休止します...")
            time.sleep(wait_minutes * 60)

    print(f"[INFO] 今回の処理完了。処理頭数 = {processed}")


def insert_pedigree_record(
    horse_id: str,
    horse_name: str,
    father: str,
    father_father: str,
    father_mother: str,
    mother: str,
    mother_father: str,
    mother_mother: str,
):
    """
    MT_HorsePedigree に1件INSERTする。
    DBUtil.execute_sql_query を利用。
    """
    sql = """
        INSERT INTO MT_HorsePedigree
            (horse_id, 馬名, 父, 母, 母父, 母母, 父父, 父母)
        VALUES (%s, %s, %s, %s, %s, %s, %s, %s)
    """
    args = (
        horse_id,
        horse_name,
        father,
        mother,
        mother_father,
        mother_mother,
        father_father,
        father_mother,
    )

    try:
        execute_sql_query(sql, args)
        print(f"[INFO] INSERT 完了: horse_id={horse_id}, 馬名={horse_name}")
    except Exception as e:
        print(f"[ERROR] INSERT 失敗: horse_id={horse_id}, error={e}")


def dump_horse_page_for_debug(horse_id: str):
    """
    デバッグ用（Selenium 版）:
    ・馬ページを取得し、
      - タイトル
      - 血統テーブル (table.blood_table)
      - 父 / 母 / 祖父母 の名前
      - ページテキストの冒頭
      を print する。
    """
    soup = fetch_horse_page_selenium(horse_id)
    if soup is None:
        print("[WARN] soup が取得できませんでした。")
        return

    # ページタイトル
    title_tag = soup.find("title")
    if title_tag is not None:
        print("=== <title> ===")
        print(title_tag.get_text(strip=True))
        print()

    # 血統テーブルのHTMLダンプ（今までどおり）
    # blood_table = soup.find("table", class_="blood_table")
    # print("=== blood_table (血統テーブル) ===")
    # if blood_table is not None:
    #     print(blood_table.prettify())
    # else:
    #     print("blood_table が見つかりませんでした。")
    # print()

    # ここから追加：父・母・祖父母の抽出結果を表示
    father, mother, ff, fm, mf, mm = extract_pedigree_2gen(soup)
    print("=== parsed pedigree (2 generations) ===")
    print(f"父       : {father}")
    print(f"父父     : {ff}")
    print(f"父母     : {fm}")
    print(f"母       : {mother}")
    print(f"母父     : {mf}")
    print(f"母母     : {mm}")
    print()

    # ページ全体テキストの冒頭一部
    # print("=== page_text (先頭 500 文字) ===")
    # page_text = soup.get_text(" ", strip=True)
    # print(page_text[:500])
    print()


def main():
    """
    コマンドライン引数に応じて動作を切り替えるエントリポイント。
    - --test-horse-id が指定された場合:
        固定 horse_id で 1件だけ取得して内容をダンプする（開発・調査用）。
    - 指定されていない場合:
        TR_raceresult / MT_HorsePedigree を突合して、
        未登録 horse_id を先頭から max_count 件だけ取得し、DBにINSERTする。
    """
    parser = argparse.ArgumentParser(description="競走馬血統ページ取得バッチ")
    parser.add_argument(
        "--test-horse-id",
        type=str,
        required=False,
        help="テスト用に 1頭だけ指定して取得する horse_id（例: 2019103422）",
    )
    parser.add_argument(
        "--max-count",
        type=int,
        default=1,
        help="TRにいてMTにいないhorse_idのうち、何件まで処理するか（デフォルト1件）",
    )
    parser.add_argument(
        "--batch-size",
        type=int,
        default=100,
        help="何頭ごとにインターバルを入れるか（デフォルト100頭）",
    )
    parser.add_argument(
        "--wait-minutes",
        type=int,
        default=30,
        help="インターバルの長さ（分）（デフォルト30分）",
    )

    args = parser.parse_args()

    # 1) 固定 horse_id で 1件だけ試すモード（開発・調査用）
    if args.test_horse_id:
        print("[INFO] 固定 horse_id でテスト実行します。")
        dump_horse_page_for_debug(args.test_horse_id)
        print("[INFO] テスト終了。")
        return

    # 2) バッチモード（差分を回してINSERT）
    process_unregistered_horses(
        max_count=args.max_count,
        batch_size=args.batch_size,
        wait_minutes=args.wait_minutes,
    )


if __name__ == "__main__":
    print("ScrapePedigreeFromDB: script started")
    main()
