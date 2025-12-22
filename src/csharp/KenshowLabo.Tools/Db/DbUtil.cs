using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace KenshowLabo.Tools.Db
{
    public static class DbUtil
    {
        /// <summary>
        /// SQL Serverに接続し、クエリを実行して結果を List&lt;T&gt; に詰めて返します。
        /// </summary>
        public static List<T> QueryToList<T>(
            string connectionString,
            string sql,
            Action<SqlParameterCollection>? addParameters,
            Func<SqlDataReader, T> mapRow)
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
                            T item = mapRow(reader);
                            list.Add(item);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// SqlParameter を作成します（よく使うので共通化）。
        /// </summary>
        public static SqlParameter CreateParameter(string name, SqlDbType type, object value)
        {
            SqlParameter p = new SqlParameter(name, type);
            p.Value = value;
            return p;
        }

        /// <summary>
        /// SqlValueReader を外部から使えるように公開ラッパーを提供します。
        /// </summary>
        public static string ReadString(SqlDataReader reader, string columnName)
        {
            return SqlValueReader.ReadString(reader, columnName);
        }

        /// <summary>
        /// SqlValueReader を外部から使えるように公開ラッパーを提供します。
        /// </summary>
        public static DateTime ReadDateTime(SqlDataReader reader, string columnName)
        {
            return SqlValueReader.ReadDateTime(reader, columnName);
        }

        /// <summary>
        /// SqlValueReader を外部から使えるように公開ラッパーを提供します。
        /// </summary>
        public static int ReadInt(SqlDataReader reader, string columnName)
        {
            return SqlValueReader.ReadInt(reader, columnName);
        }

        /// <summary>
        /// SqlValueReader を外部から使えるように公開ラッパーを提供します。
        /// </summary>
        public static int? ReadNullableInt(SqlDataReader reader, string columnName)
        {
            return SqlValueReader.ReadNullableInt(reader, columnName);
        }

        /// <summary>
        /// SqlValueReader を外部から使えるように公開ラッパーを提供します。
        /// </summary>
        public static decimal ReadDecimal(SqlDataReader reader, string columnName)
        {
            return SqlValueReader.ReadDecimal(reader, columnName);
        }

        /// <summary>
        /// SqlValueReader を外部から使えるように公開ラッパーを提供します。
        /// </summary>
        public static bool ReadBool(SqlDataReader reader, string columnName)
        {
            return SqlValueReader.ReadBool(reader, columnName);
        }
        
        /// <summary>
        /// SQL Serverに接続し、非クエリ（INSERT/UPDATE/DELETE）を実行します。
        /// </summary>
        public static int ExecuteNonQuery(
            string connectionString,
            string sql,
            Action<SqlParameterCollection>? addParameters)
        {
            int affectedRows = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    if (addParameters != null)
                    {
                        addParameters(cmd.Parameters);
                    }

                    affectedRows = cmd.ExecuteNonQuery();
                }
            }

            return affectedRows;
        }

        /// <summary>
        /// SQL Serverに接続し、スカラー値を取得します（NULLの場合は default(T) を返します）。
        /// </summary>
        public static T? ExecuteScalar<T>(
            string connectionString,
            string sql,
            Action<SqlParameterCollection>? addParameters)
        {
            object? result;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    if (addParameters != null)
                    {
                        addParameters(cmd.Parameters);
                    }

                    result = cmd.ExecuteScalar();
                }
            }

            if (result == null)
            {
                return default(T);
            }

            if (result == DBNull.Value)
            {
                return default(T);
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }

        /// <summary>
        /// トランザクション内で処理を実行します（例：取込の一括INSERT/UPDATE）。
        /// </summary>
        public static void ExecuteInTransaction(
            string connectionString,
            Action<SqlConnection, SqlTransaction> action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        action(conn, tx);

                        tx.Commit();
                    }
                    catch
                    {
                        try
                        {
                            tx.Rollback();
                        }
                        catch
                        {
                            // rollback失敗は握りつぶし（原例外を優先）
                        }

                        throw;
                    }
                }
            }
        }
    }
}