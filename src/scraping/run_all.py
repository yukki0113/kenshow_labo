"""
run_all.py

レース結果 → TR反映＆枠番更新 → 血統 → 払戻
までを一括実行するためのエントリスクリプト。
"""

import time
import argparse

from DBUtil import execute_stored_procedure
from common import log_info, log_error
from race_result import scrape_and_insert_race_data
from pedigree import process_unregistered_horses
from payout import process_payouts


def run_all(
    year_from: int,
    year_to: int,
    place_codes,
    per_request_sec: float = 0.5,       # S
    batch_size: int = 100,             # ◯
    batch_wait_minutes: int = 30,      # M
    pedigree_max_count: int = 100,
    payout_max_count: int = 100,
    skip_pedigree: bool = False,
    skip_payout: bool = False,
):
    """
    一連の処理をまとめて実行する。

    year_from, year_to:
        レース結果スクレイピングの対象年（西暦）
    place_codes:
        対象場コードのリスト（例: ["05", "06"]）
    race_per_request_sec:
        レース結果取得時の 1 リクエストごとの待機秒数
    race_batch_size:
        レース結果取得時、何リクエストごとにバッチ休憩を入れるか（0 なら休憩なし）
    race_batch_interval_sec:
        レース結果取得時のバッチ休憩秒数
    pedigree_max_count:
        血統取得で今回処理する最大頭数
    pedigree_batch_size:
        血統取得で何頭ごとにインターバルを入れるか
    pedigree_wait_minutes:
        血統取得のインターバル（分）
    payout_max_count:
        払戻取得で今回処理する最大レース数
    payout_batch_size:
        払戻取得で何レースごとにインターバルを入れるか
    payout_wait_minutes:
        払戻取得のインターバル（分）
    skip_pedigree:
        True の場合、血統取得ステップをスキップ
    skip_payout:
        True の場合、払戻取得ステップをスキップ
    """

    total_start = time.time()
    log_info("run_all: レース結果 → 血統 → 払戻 の一括処理を開始します。")

    # ------------------------------
    # Step 1: レース結果スクレイピング
    # ------------------------------
    log_info("Step 1: レース結果スクレイピングを開始します。")

    scrape_and_insert_race_data(
        year_start=year_from,
        year_end=year_to,
        place_codes=place_codes,
        per_request_sec=per_request_sec,
        batch_size=batch_size,
        batch_interval_sec=batch_wait_minutes * 60,
    )

    # IF → TR への展開
    log_info("Step 1-2: TRテーブルへ展開（Insert_Into_TRData）...")
    try:
        execute_stored_procedure("Insert_Into_TRData")
    except Exception as e:
        log_error(f"Insert_Into_TRData 実行中にエラーが発生しました: {e}")
        return

    # 枠番更新
    log_info("Step 1-3: 枠番更新（UpdateFrameNumbers）...")
    try:
        execute_stored_procedure("UpdateFrameNumbers")
    except Exception as e:
        log_error(f"UpdateFrameNumbers 実行中にエラーが発生しました: {e}")
        return

    # ------------------------------
    # Step 2: 血統情報の取得
    # ------------------------------
    if not skip_pedigree:
        process_unregistered_horses(
            max_count=pedigree_max_count,
            per_request_sec=per_request_sec,
            batch_size=batch_size,
            wait_minutes=batch_wait_minutes,
        )
    else:
        log_info("Step 2: 血統情報の取得は skip_pedigree=True のためスキップします。")

    # ------------------------------
    # Step 3: 払戻情報の取得
    # ------------------------------
    if not skip_payout:
        process_payouts(
            max_count=payout_max_count,
            per_request_sec=per_request_sec,
            batch_size=batch_size,
            wait_minutes=batch_wait_minutes,
        )
    else:
        log_info("Step 3: 払戻情報の取得は skip_payout=True のためスキップします。")

    # ------------------------------
    # 全体の処理時間
    # ------------------------------
    total_end = time.time()
    elapsed = int(total_end - total_start)
    h = elapsed // 3600
    m = (elapsed % 3600) // 60
    s = elapsed % 60

    log_info(f"run_all: 全処理完了（経過時間: {h} 時間 {m} 分 {s} 秒）")


def main():
    """
    コマンドライン引数を受け取り、run_all(...) を呼び出すエントリポイント。

    例:
        python run_all.py --year-from 2024 --year-to 2024 --places 05,06
    """
    parser = argparse.ArgumentParser(description="レース結果→血統→払戻 一括実行バッチ")

    # 必須: 年度
    parser.add_argument(
        "--year-from",
        type=int,
        required=True,
        help="取得開始年（西暦）",
    )
    parser.add_argument(
        "--year-to",
        type=int,
        required=True,
        help="取得終了年（西暦・含む）",
    )

    # 任意: 場コード
    parser.add_argument(
        "--places",
        type=str,
        required=False,
        default="01,02,03,04,05,06,07,08,09,10",
        help="取得対象の場コードをカンマ区切りで指定（例: 05,06）。省略時は全場。",
    )
    # 任意: 待機秒数
    parser.add_argument(
        "--per-request-sec",
        type=float,
        default=0.5,
        help="全バッチ共通の1リクエストごとの待機秒数",
    )
    # 全バッチ共通のバッチサイズ
    parser.add_argument(
        "--batch-size",
        type=int,
        default=100,
        help="全バッチ共通のバッチサイズ",
    )
    # 全バッチ共通のバッチ休憩時間
    parser.add_argument(
        "--batch-wait-minutes",
        type=int,
        default=30,
        help="全バッチ共通のバッチ休憩時間（分）",
    )
    parser.add_argument(
        "--race-batch-interval-sec",
        type=int,
        required=False,
        default=0,
        help="レース結果取得時のバッチ休憩秒数（デフォルト 0秒）",
    )
    # 任意: 血統バッチのパラメータ
    parser.add_argument(
        "--pedigree-max-count",
        type=int,
        required=False,
        default=100,
        help="血統取得で今回処理する最大頭数（デフォルト 100）",
    )
    # 任意: 払戻バッチのパラメータ
    parser.add_argument(
        "--payout-max-count",
        type=int,
        required=False,
        default=100,
        help="払戻取得で今回処理する最大レース数（デフォルト 100）",
    )
    # ステップ個別スキップ用
    parser.add_argument(
        "--skip-pedigree",
        action="store_true",
        help="指定した場合、血統取得ステップをスキップする",
    )
    parser.add_argument(
        "--skip-payout",
        action="store_true",
        help="指定した場合、払戻取得ステップをスキップする",
    )

    args = parser.parse_args()

    # 場コード文字列 "05,06" → ["05", "06"]
    place_codes = [code.strip() for code in args.places.split(",") if code.strip()]

    run_all(
        year_from=args.year_from,
        year_to=args.year_to,
        place_codes=place_codes,
        race_per_request_sec=args.race_per_request_sec,
        race_batch_size=args.race_batch_size,
        race_batch_interval_sec=args.race_batch_interval_sec,
        pedigree_max_count=args.pedigree_max_count,
        pedigree_batch_size=args.pedigree_batch_size,
        pedigree_wait_minutes=args.pedigree_wait_minutes,
        payout_max_count=args.payout_max_count,
        payout_batch_size=args.payout_batch_size,
        payout_wait_minutes=args.payout_wait_minutes,
        skip_pedigree=args.skip_pedigree,
        skip_payout=args.skip_payout,
    )


if __name__ == "__main__":
    log_info("run_all.py: script started")
    main()
