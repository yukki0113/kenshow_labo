import requests
from bs4 import BeautifulSoup
import time
import argparse
import re
from DBUtil import execute_bulk_insert, execute_stored_procedure, execute_sql_query

# ユーザーエージェント（実際のブラウザの UA）
headers = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36"
}

def truncate_table():
    # テーブルをTRUNCATEする
    sql_query = "TRUNCATE TABLE if_raceresult_csv"
    execute_sql_query(sql_query)


def insert_race_data_to_database(race_data):
    # データベースにレースデータを挿入する
    sql_query = """
        INSERT INTO if_raceresult_csv (
            race_id, horse_id, [馬名], [騎手], [馬番], [走破時計], [オッズ], [通過順], [着順], [馬体重], [馬体重変動]
            , [性], [齢], [斤量], [上がり], [人気], [レース名], [日付], [開催], [クラス], [芝_ダート]
            , [距離], [回り], [馬場], [天気], track_id, [場名])
        VALUES (
            %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s
        )
    """
    # バルクインサートのためのデータリストを作成
    values = [tuple(entry) for entry in race_data[1:]]  # ヘッダー行を除外し、タプルのリストに変換

    # 各レースのデータをデータベースにバルクインサート
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

    取得できない場合は空文字のまま返却する。
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
        # ページのすべてのテキストを 1 本の文字列にまとめる
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


def scrape_and_insert_race_data(year_start, year_end, place_codes):

    # テーブルをTRUNCATE
    truncate_table()

    for year in range(year_start, year_end + 1):

        race_data_all = []
        #取得するデータのヘッダー情報を先に追加しておく
        race_data_all.append(['race_id','horse_id','馬','騎手','馬番','走破時間','オッズ','通過順','着順','体重','体重変化','性','齢','斤量','上がり','人気','レース名','日付','開催','クラス','芝・ダート','距離','回り','馬場','天気','場id','場名'])

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

            #開催回数分ループ（6回）
            for z in range(3):
            #for z in range(7):
                
                continueCounter = 0  # 'continue'が実行された回数をカウントするためのカウンターを追加
                
                #開催日数分ループ（12日）
                for y in range(13):
                    
                    race_id = ''
                    if y<9:
                        race_id = str(year)+place_code+"0"+str(z+1)+"0"+str(y+1)
                        url1="https://db.netkeiba.com/race/"+race_id
                    else:
                        race_id = str(year)+place_code+"0"+str(z+1)+str(y+1)
                        url1="https://db.netkeiba.com/race/"+race_id
                    
                    #yの更新をbreakするためのカウンター
                    yBreakCounter = 0
                    
                    #レース数分ループ（12R）
                    for x in range(12):
                        if x<9:
                            url=url1+str("0")+str(x+1)
                            current_race_id = race_id+str("0")+str(x+1)
                        else:
                            url=url1+str(x+1)
                            current_race_id = race_id+str(x+1)
                        
                        try:
                            # ユーザーエージェントとタイムアウトを付与してリクエスト
                            r = requests.get(url, headers=headers, timeout=10)
                                                    
                        #リクエストを投げすぎるとエラーになることがあるため
                        #失敗したら10秒待機してリトライする
                        except requests.exceptions.RequestException as e:
                            print(f"Error: {e}")
                            print("Retrying in 10 seconds...")
                            time.sleep(10)  # 10秒待機
                            r = requests.get(url, headers=headers, timeout=10)
                        
                        #バグ対策でdecode
                        soup = BeautifulSoup(r.content.decode("euc-jp", "ignore"), "html.parser")
                        soup_span = soup.find_all("span")
                        
                        # テーブルを指定
                        main_table = soup.find("table", {"class": "race_table_01 nk_tb_common"})

                        # テーブル内の全ての行を取得
                        try:
                            main_rows = main_table.find_all("tr")
                        except:
                            print('continue: ' + url)
                            continueCounter += 1  # 'continue'が実行された回数をカウントアップ
                            if continueCounter == 2:  # 'continue'が2回連続で実行されたらループを抜ける
                                continueCounter = 0
                                break
                            continue

                        race_data = []
                        for i, row in enumerate(main_rows[1:], start=1):# ヘッダ行をスキップ
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
                                # 何かあってもスクレイピング全体は止めず、空文字のままにしておく
                                horse_id = ""
                            #走破時間
                            runtime=''
                            try:
                                runtime= cols[7].text.strip()
                            except IndexError:
                                runtime = ''
                            soup_nowrap = soup.find_all("td",nowrap="nowrap",class_=None)
                            #通過順
                            pas = ''
                            try:
                                pas = str(cols[10].text.strip())
                            except:
                                pas = ''
                            weight = 0
                            weight_dif = 0
                            #体重
                            var = cols[14].text.strip()
                            try:
                                weight = int(var.split("(")[0])
                                weight_dif = int(var.split("(")[1][0:-1])
                            except ValueError:
                                weight = 0
                                weight_dif = 0
                            weight = weight
                            weight_dif = weight_dif
                            #上がり
                            last = ''
                            try:
                                last = cols[11].text.strip()
                            except IndexError:
                                last = ''
                            #人気
                            pop = ''
                            try:
                                pop = cols[13].text.strip()
                            except IndexError:
                                pop = ''
                            
                            # レースの情報（まずは span ベースで）
                            sur, rou, dis, con, wed = parse_race_condition_from_spans(soup_span, url)

                            # まだ埋まっていない天候・馬場を、ページ全体のテキストから補完
                            con, wed = fill_condition_from_page_text(soup, con, wed, url)
                            
                            soup_smalltxt = soup.find_all("p",class_="smalltxt")
                            detail=str(soup_smalltxt).split(">")[1].split(" ")[1]
                            date=str(soup_smalltxt).split(">")[1].split(" ")[0]
                            clas=str(soup_smalltxt).split(">")[1].split(" ")[2].replace(u'\xa0', u' ').split(" ")[0]
                            title=str(soup.find_all("h1")[1]).split(">")[1].split("<")[0]

                            race_data = [
                                current_race_id,
                                horse_id,            #馬ID
                                cols[3].text.strip(),#馬の名前
                                cols[6].text.strip(),#騎手の名前
                                cols[2].text.strip(),#馬番
                                runtime,#走破時間
                                cols[12].text.strip(),#オッズ,
                                pas,#通過順
                                cols[0].text.strip(),#着順
                                weight,#体重
                                weight_dif,#体重変化
                                cols[4].text.strip()[0],#性
                                cols[4].text.strip()[1],#齢
                                cols[5].text.strip(),#斤量
                                last,#上がり
                                pop,#人気,
                                title,#レース名
                                date,#日付
                                detail,
                                clas,#クラス
                                sur,#芝かダートか
                                dis,#距離
                                rou,#回り
                                con,#馬場状態
                                wed,#天気
                                w,#場
                                place]
                            race_data_all.append(race_data)
                        
                        print(detail+str(x+1)+"R")#進捗を表示
                            
                    if yBreakCounter == 12:#12レース全部ない日が検出されたら、その開催中の最後の開催日と考える
                        break
        
        # データベースに挿入
        print("DBにInsert...")
        insert_race_data_to_database(race_data_all)



if __name__ == "__main__":

    # コマンドライン引数の定義
    parser = argparse.ArgumentParser(description="netkeiba レース結果スクレイピング")
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
    elapsed_time_seconds = end_time - start_time
    elapsed_time_hours = int(elapsed_time_seconds // 3600)
    elapsed_time_minutes = int((elapsed_time_seconds % 3600) // 60)
    elapsed_time_seconds = int(elapsed_time_seconds % 60)

    # 総処理時間を表示
    print(f"Total processing time: {elapsed_time_hours} 時間, {elapsed_time_minutes} 分, {elapsed_time_seconds} 秒")
    
    print("終了")
