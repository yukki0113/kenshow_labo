"""
scraping 共通モジュール

・UA/timeout付き HTTP リクエスト → BeautifulSoup 取得
・簡易ログ出力（[INFO]/[WARN]/[ERROR]）
・レース/馬 URL の組み立て
・horse_id 抽出
・アクセス間隔制御（SleepController）
"""

import time
from typing import Optional

import requests
from bs4 import BeautifulSoup


# ===== HTTP / UA 設定 =====

# 3本のバッチで共通利用するユーザーエージェント
HEADERS = {
    "User-Agent": (
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
        "AppleWebKit/537.36 (KHTML, like Gecko) "
        "Chrome/143.0.0.0 Safari/537.36"
    )
}

DEFAULT_TIMEOUT = 10  # 秒


# ===== ログ出力 =====

def _now_str() -> str:
    """タイムスタンプ文字列（YYYY-MM-DD HH:MM:SS）を返す。"""
    return time.strftime("%Y-%m-%d %H:%M:%S")


def log_info(message: str) -> None:
    print(f"[INFO ] {_now_str()} {message}")


def log_warn(message: str) -> None:
    print(f"[WARN ] {_now_str()} {message}")


def log_error(message: str) -> None:
    print(f"[ERROR] {_now_str()} {message}")


# ===== アクセス制御（sleep） =====

class SleepController:
    """
    アクセス間隔を制御するためのクラス。

    ・per_request_sec: 1リクエストごとに待つ秒数
    ・batch_size:      何回リクエストしたらバッチ休憩を入れるか（0なら無効）
    ・batch_interval_sec: バッチ休憩の秒数
    """

    def __init__(
        self,
        per_request_sec: float = 0.0,
        batch_size: int = 0,
        batch_interval_sec: float = 0.0,
    ) -> None:
        self.per_request_sec: float = per_request_sec
        self.batch_size: int = batch_size
        self.batch_interval_sec: float = batch_interval_sec
        self._request_count: int = 0

    def before_request(self) -> None:
        """
        HTTP リクエストを投げる「直前」に呼び出す。
        必要に応じて sleep を入れる。
        """
        # 1リクエストごとの小休憩
        if self.per_request_sec > 0:
            time.sleep(self.per_request_sec)

        # バッチごとの休憩
        if self.batch_size > 0:
            self._request_count += 1
            if self._request_count % self.batch_size == 0:
                log_info(
                    f"{self._request_count}件リクエストを送信しました。"
                    f"{self.batch_interval_sec}秒休止します。"
                )
                if self.batch_interval_sec > 0:
                    time.sleep(self.batch_interval_sec)


# ===== HTTP → BeautifulSoup ヘルパー =====

def fetch_soup(
    url: str,
    *,
    timeout: int = DEFAULT_TIMEOUT,
    max_retries: int = 1,
    sleeper: Optional[SleepController] = None,
    headers: Optional[dict] = None,
    decode: str = "euc-jp",
) -> Optional[BeautifulSoup]:
    """
    指定URLからHTTP GETを行い、BeautifulSoup を返す共通関数。

    ・UA, timeout, retry を一元管理
    ・失敗時は None を返す
    """
    if headers is None:
        headers = HEADERS

    # アクセス間隔制御
    if sleeper is not None:
        sleeper.before_request()

    for attempt in range(max_retries + 1):
        try:
            log_info(f"Fetching: {url}")
            response = requests.get(url, headers=headers, timeout=timeout)
            response.raise_for_status()

            # netkeiba は euc-jp が多いのでデフォルトは euc-jp
            content = response.content.decode(decode, "ignore")
            return BeautifulSoup(content, "html.parser")

        except requests.exceptions.RequestException as e:
            log_error(f"Request failed (attempt {attempt + 1}): {e}")

            if attempt >= max_retries:
                log_error("Max retries exceeded. Giving up.")
                return None

            # リトライ前の待機
            log_info("Retrying in 10 seconds...")
            time.sleep(10)

    return None  # 通常はここまで来ない想定


# ===== URL/ID 関連 =====

def build_race_url(race_id: str) -> str:
    """
    race_id から netkeiba のレース結果ページURLを組み立てる。

    例: race_id = '202405030811'
        → 'https://db.netkeiba.com/race/202405030811/'
    """
    return f"https://db.netkeiba.com/race/{race_id}/"


def build_horse_url(horse_id: str) -> str:
    """
    horse_id から netkeiba の馬ページURLを組み立てる。

    例: horse_id = '2019103422'
        → 'https://db.netkeiba.com/horse/2019103422/'
    """
    return f"https://db.netkeiba.com/horse/{horse_id}/"


def extract_horse_id_from_href(href: str) -> str:
    """
    /horse/XXXX/ 形式の href から horse_id 部分を取り出す。
    取得できなかった場合は空文字を返す。
    """
    import re

    if not href:
        return ""

    m = re.search(r"/horse/([^/]+)/", href)
    if m:
        return m.group(1)
    return ""
