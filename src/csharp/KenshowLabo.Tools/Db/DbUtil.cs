using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace KenshowLabo.Tools.Db
{
    /// <summary>
    /// SQL Server 実行の共通ユーティリティです。
    /// ※責務は「SQL実行」「トランザクション」に限定します。
    /// </summary>
    public static class DbUtil
    {
        /// <summary>
        /// SQL Serverに接続し、クエリを実行して結果を List&lt;T&gt; に詰めて返します。
        /// </summary>
        public static List<T> QueryToList<T>(
            string connectionString,
            string sql,
            Action<SqlParameterCollection> addParameters,
            Func<SqlDataReader, T> mapRow,
            int commandTimeoutSeconds = 30)
        {
            // 結果格納
            List<T> list = new List<T>();

            // 接続を開く
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // コマンドを準備する
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    // タイムアウト設定（重い集計・取込で必要になる）
                    cmd.CommandTimeout = commandTimeoutSeconds;

                    // パラメータを追加する
                    if (addParameters != null)
                    {
                        addParameters(cmd.Parameters);
                    }

                    // 実行して読み取る
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 1行 → 1要素に変換して追加する
                            T item = mapRow(reader);
                            list.Add(item);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// SQL Serverに接続し、非クエリ（INSERT/UPDATE/DELETE）を実行します。
        /// </summary>
        public static int ExecuteNonQuery(
            string connectionString,
            string sql,
            Action<SqlParameterCollection> addParameters,
            int commandTimeoutSeconds = 30)
        {
            int affectedRows = 0;

            // 接続を開く
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // コマンドを準備する
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    // タイムアウト設定
                    cmd.CommandTimeout = commandTimeoutSeconds;

                    // パラメータを追加する
                    if (addParameters != null)
                    {
                        addParameters(cmd.Parameters);
                    }

                    // 実行
                    affectedRows = cmd.ExecuteNonQuery();
                }
            }

            return affectedRows;
        }

        /// <summary>
        /// SQL Serverに接続し、スカラー値を取得します（DBNull/NULL の場合は default を返します）。
        /// </summary>
        public static T? ExecuteScalar<T>(
            string connectionString,
            string sql,
            Action<SqlParameterCollection> addParameters,
            int commandTimeoutSeconds = 30)
        {
            object result;

            // 接続を開く
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // コマンドを準備する
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    // タイムアウト設定
                    cmd.CommandTimeout = commandTimeoutSeconds;

                    // パラメータを追加する
                    if (addParameters != null)
                    {
                        addParameters(cmd.Parameters);
                    }

                    // 実行
                    result = cmd.ExecuteScalar();
                }
            }

            // NULL/DBNull の場合
            if (result == null || result == DBNull.Value)
            {
                return default(T);
            }

            // 既に型が一致している場合はそのまま返す
            if (result is T)
            {
                return (T)result;
            }

            // Nullable<T> を考慮して変換する
            Type targetType = typeof(T);
            Type? underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
            {
                targetType = underlyingType;
            }

            object converted = Convert.ChangeType(result, targetType);
            return (T)converted;
        }

        /// <summary>
        /// トランザクション内で処理を実行します（例：取込の一括INSERT/UPDATE）。
        /// </summary>
        public static void ExecuteInTransaction(
            string connectionString,
            Action<SqlConnection, SqlTransaction> action)
        {
            // 接続を開く
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // トランザクション開始
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 呼び出し側処理を実行
                        action(conn, tx);

                        // コミット
                        tx.Commit();
                    }
                    catch
                    {
                        // ロールバック（失敗しても原例外を優先）
                        try
                        {
                            tx.Rollback();
                        }
                        catch
                        {
                            // rollback失敗は握りつぶし
                        }

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// SqlParameter を作成します（NULL は DBNull.Value に置換します）。
        /// </summary>
        public static SqlParameter CreateParameter(string name, SqlDbType type, object value)
        {
            SqlParameter p = new SqlParameter(name, type);

            // NULL をそのまま渡すと例外や意図しない挙動になりやすいため変換する
            if (value == null)
            {
                p.Value = DBNull.Value;
            }
            else
            {
                p.Value = value;
            }

            return p;
        }
    }
}
