import pymssql
import json
from pathlib import Path

def _load_db_config():
    """
    DB 接続情報を dbconfig.json から読み込む。
    実ファイルは git にコミットせず、dbconfig.sample.json のみコミット対象とする。
    """
    config_path = Path(__file__).with_name("dbconfig.json")

    if not config_path.exists():
        # 初回起動時などに気付きやすいよう、はっきりしたメッセージを出す
        raise RuntimeError(
            f"DB 接続情報ファイルが見つかりません: {config_path}\n"
            "dbconfig.sample.json をコピーして dbconfig.json を作成し、接続情報を設定してください。"
        )

    with config_path.open(encoding="utf-8") as f:
        config = json.load(f)

    # 想定キーが揃っているか軽くチェック
    for key in ("server", "user", "password", "database"):
        if key not in config:
            raise RuntimeError(f"dbconfig.json に '{key}' がありません。")

    return config


def get_database_connection():
    # データベースに接続して connection オブジェクトを返す
    config = _load_db_config()

    connection = pymssql.connect(
        server=config["server"],
        user=config["user"],
        password=config["password"],
        database=config["database"]
    )
    return connection

def execute_sql_query(sql_query, args=None):
    # SQLクエリを実行する
    connection = get_database_connection()
    try:
        with connection.cursor(as_dict=True) as cursor:
            if args:
                cursor.execute(sql_query, args)
            else:
                cursor.execute(sql_query)
        # データベースにコミット
        connection.commit()
    except Exception as e:
        # エラーが発生した場合の処理
        print(f"An error occurred: {e}")
        # ロールバックを実行し、変更を取り消す
        connection.rollback()
    finally:
        # 接続を閉じる
        connection.close()
        

def execute_stored_procedure(proc_name, args=None):
    # ストアドプロシージャを実行する
    connection = get_database_connection()
    try:
        with connection.cursor(as_dict=True) as cursor:
            if args:
                cursor.callproc(proc_name, args)
            else:
                cursor.callproc(proc_name)
        # データベースにコミット
        connection.commit()
    except Exception as e:
        # エラーが発生した場合の処理
        print(f"An error occurred: {e}")
        # ロールバックを実行し、変更を取り消す
        connection.rollback()
    finally:
        # 接続を閉じる
        connection.close()
        

def execute_bulk_insert(sql_query, values):
    # バルクインサートを実行する
    connection = get_database_connection()
    try:
        with connection.cursor(as_dict=True) as cursor:
            cursor.executemany(sql_query, values)
        # データベースにコミット
        connection.commit()
    except Exception as e:
        # エラーが発生した場合の処理
        print(f"An error occurred: {e}")
        # ロールバックを実行し、変更を取り消す
        connection.rollback()
    finally:
        # 接続を閉じる
        connection.close()
