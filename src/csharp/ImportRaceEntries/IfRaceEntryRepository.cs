using System;
using System.Collections.Generic;
using System.Data;
using KenshowLabo.Tools.Db;
using KenshowLabo.Tools.Models;
using Microsoft.Data.SqlClient;

namespace ImportRaceEntries {
    internal static class IfRaceEntryRepository {
        /// <summary>
        /// IF_RaceEntry へ UPSERT します（race_id + horse_no を一意想定）。
        /// </summary>
        public static void UpsertMany(string connectionString, List<RaceEntryRow> rows) {
            // 取込は基本トランザクションでまとめる
            DbUtil.ExecuteInTransaction(connectionString, (SqlConnection conn, SqlTransaction tx) => {
                // TODO: 実テーブル定義に合わせてSQLを確定する
                // ここでは雛形として、SqlCommand使い回しの型だけ作る

                string sql = ""
                    + "/* TODO: IF_RaceEntry の定義に合わせてMERGE/UPDATE+INSERTを確定 */"
                    + "SELECT 1;";

                using (SqlCommand cmd = new SqlCommand(sql, conn, tx)) {
                    cmd.CommandType = CommandType.Text;

                    // NOTE: 本実装時にパラメータを定義し、rowsループで値を差し替える
                    foreach (RaceEntryRow row in rows) {
                        // TODO: cmd.Parameters[...] に値をセットして cmd.ExecuteNonQuery()
                        // 現時点では何もしない
                    }
                }
            });
        }
    }
}
