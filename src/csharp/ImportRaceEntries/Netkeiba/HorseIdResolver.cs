using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using KenshowLabo.Tools.Db;

namespace ImportRaceEntries.Netkeiba {
    /// <summary>
    /// netkeiba馬ID（nk）→ JV馬ID（jv）を解決します。
    /// 優先順位：MP_HorseId → 馬名でMT_HorsePedigree検索 → MP_HorseId作成
    /// </summary>
    public sealed class HorseIdResolver {
        private readonly string _connectionString;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public HorseIdResolver(string connectionString) {
            _connectionString = connectionString;
        }

        /// <summary>
        /// nk_horse_id と馬名から jv_horse_id を解決します。
        /// 解決できない場合は null を返します（呼び出し側で警告ログ）。
        /// </summary>
        public string? ResolveJvHorseId(string? nkHorseId, string? horseName) {
            // 入力チェック
            if (string.IsNullOrWhiteSpace(nkHorseId)) {
                return null;
            }

            // 1) 既存マッピング
            string? mapped = FindFromMapping(nkHorseId);
            if (!string.IsNullOrWhiteSpace(mapped)) {
                return mapped;
            }

            // 2) 馬名が無ければここで終了
            if (string.IsNullOrWhiteSpace(horseName)) {
                return null;
            }

            // 3) 馬名でJV血統マスタ検索（完全一致）
            List<string> hits = FindJvHorseIdsByHorseName(horseName);

            // 単一一致のみ自動採用（同名馬対策）
            if (hits.Count == 1) {
                string jvHorseId = hits[0];

                // 4) 今後のためにマッピングを作成/更新
                UpsertMapping(nkHorseId, jvHorseId, "NAME", 80);

                return jvHorseId;
            }

            // 0件 or 複数件は未解決
            return null;
        }

        /// <summary>
        /// MP_HorseId から既存の対応を取得します。
        /// </summary>
        private string? FindFromMapping(string nkHorseId) {
            const string sql = @"
SELECT m.jv_horse_id
FROM dbo.MP_HorseId AS m
WHERE m.nk_horse_id = @nk_horse_id;
";

            string? result = DbUtil.ExecuteScalar<string>(
                _connectionString,
                sql,
                (SqlParameterCollection p) => {
                    p.Add(DbUtil.CreateParameter("@nk_horse_id", SqlDbType.Char, nkHorseId));
                }
            );

            if (string.IsNullOrWhiteSpace(result)) {
                return null;
            }

            return result;
        }

        /// <summary>
        /// 馬名でMT_HorsePedigreeを検索し、該当する horse_id を返します。
        /// </summary>
        private List<string> FindJvHorseIdsByHorseName(string horseName) {
            const string sql = @"
SELECT p.horse_id
FROM dbo.MT_HorsePedigree AS p
WHERE p.horse_name = @horse_name;
";

            List<string> list = DbUtil.QueryToList<string>(
                _connectionString,
                sql,
                (SqlParameterCollection p) => {
                    p.Add(DbUtil.CreateParameter("@horse_name", SqlDbType.NVarChar, horseName));
                },
                (SqlDataReader r) => {
                    // 1列目がhorse_id想定
                    return r.GetString(0);
                }
            );

            return list;
        }

        /// <summary>
        /// MP_HorseId をUPSERTします。
        /// </summary>
        private void UpsertMapping(string nkHorseId, string jvHorseId, string source, int confidence) {
            const string sql = @"
MERGE dbo.MP_HorseId AS tgt
USING (SELECT @nk_horse_id AS nk_horse_id, @jv_horse_id AS jv_horse_id) AS src
    ON tgt.nk_horse_id = src.nk_horse_id
WHEN MATCHED THEN
    UPDATE SET
          tgt.jv_horse_id = src.jv_horse_id
        , tgt.source      = @source
        , tgt.confidence  = @confidence
        , tgt.updated_at  = SYSDATETIME()
WHEN NOT MATCHED THEN
    INSERT (nk_horse_id, jv_horse_id, source, confidence, created_at, updated_at)
    VALUES (src.nk_horse_id, src.jv_horse_id, @source, @confidence, SYSDATETIME(), SYSDATETIME());
";

            DbUtil.ExecuteNonQuery(
                _connectionString,
                sql,
                (SqlParameterCollection p) => {
                    p.Add(DbUtil.CreateParameter("@nk_horse_id", SqlDbType.Char, nkHorseId));
                    p.Add(DbUtil.CreateParameter("@jv_horse_id", SqlDbType.Char, jvHorseId));
                    p.Add(DbUtil.CreateParameter("@source", SqlDbType.NVarChar, source));
                    p.Add(DbUtil.CreateParameter("@confidence", SqlDbType.TinyInt, confidence));
                }
            );
        }
    }
}
