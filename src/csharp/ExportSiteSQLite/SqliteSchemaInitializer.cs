using Microsoft.Data.Sqlite;

namespace ExportSiteSQLite
{
    /// <summary>
    /// SQLiteスキーマ作成。
    /// </summary>
    public sealed class SqliteSchemaInitializer
    {
        /// <summary>
        /// スキーマを作成する。
        /// </summary>
        public void CreateSchema(SqliteConnection conn)
        {
            /* 初版は中央VIEWのみを対象に、PWAの条件/集計/前走比較に必要な列だけを保持する。
               horse_name は初版不要のため持たず、class_name と race_no は表示/検証補助として保持する。 */
            Execute(conn, @"
                CREATE TABLE fact_condition_stats_base (
                    race_id           TEXT NOT NULL,
                    umaban            INTEGER NOT NULL,
                    horse_id          TEXT NULL,
                    race_date         TEXT NOT NULL,
                    jyo_cd            TEXT NOT NULL,
                    race_no           INTEGER NOT NULL,
                    race_name         TEXT NULL,
                    class_name        TEXT NULL,
                    grade_cd          TEXT NULL,
                    win5_flg          INTEGER NOT NULL DEFAULT 0,
                    surface           TEXT NULL,
                    distance_m        INTEGER NULL,
                    baba_text         TEXT NULL,
                    wakuban           INTEGER NULL,
                    jockey_name       TEXT NULL,
                    sire_name         TEXT NULL,
                    running_style     TEXT NULL,
                    finish_pos        INTEGER NULL,
                    sex               TEXT NULL,
                    age               INTEGER NULL,
                    popularity        INTEGER NULL,
                    weight_carried    REAL NULL,
                    horse_weight      INTEGER NULL,
                    payout_win_yen    INTEGER NULL,
                    payout_place_yen  INTEGER NULL,
                    prev_class        TEXT NULL,
                    prev_distance_m   INTEGER NULL,
                    distance_change   TEXT NULL,
                    PRIMARY KEY (race_id, umaban)
            );");
            
            Execute(conn, "CREATE INDEX idx_fcsb_filter ON fact_condition_stats_base (race_date, jyo_cd, surface, distance_m, grade_cd, win5_flg);");
            Execute(conn, "CREATE INDEX idx_fcsb_race_name ON fact_condition_stats_base (race_name);");
            Execute(conn, "CREATE INDEX idx_fcsb_jockey ON fact_condition_stats_base (jockey_name);");
            Execute(conn, "CREATE INDEX idx_fcsb_sire ON fact_condition_stats_base (sire_name);");

            /* old */
            // /* メタ */
            // Execute(conn, @"
            //     CREATE TABLE meta_snapshot (
            //         snapshot_id    TEXT PRIMARY KEY,
            //         created_at_utc TEXT NOT NULL,
            //         scope_note     TEXT NOT NULL,
            //         source_version TEXT NULL
            // );");

            // /* 検索ハブ */
            // Execute(conn, @"
            //     CREATE TABLE dim_race (
            //         race_id      TEXT PRIMARY KEY,
            //         race_date    TEXT NOT NULL,
            //         jyo_cd       TEXT NOT NULL,
            //         race_no      INTEGER NOT NULL,
            //         race_name    TEXT NULL,
            //         track_name   TEXT NULL,
            //         surface_type TEXT NULL,
            //         distance_m   INTEGER NULL,
            //         class_simple TEXT NULL,
            //         win5_flg     INTEGER NOT NULL DEFAULT 0,
            //         has_result   INTEGER NOT NULL DEFAULT 0,
            //         has_entry    INTEGER NOT NULL DEFAULT 0,
            //         data_group   TEXT NULL
            // );");

            // /* 過去：結果 */
            // Execute(conn, @"
            //     CREATE TABLE fact_race_result (
            //         race_id        TEXT NOT NULL,
            //         horse_id       TEXT NOT NULL,
            //         horse_name     TEXT NOT NULL,
            //         umaban         INTEGER NULL,
            //         wakuban        INTEGER NULL,
            //         finish_pos     INTEGER NULL,
            //         time_text      TEXT NULL,
            //         margin_text    TEXT NULL,
            //         jockey_name    TEXT NULL,
            //         weight_carried REAL NULL,
            //         odds           REAL NULL,
            //         popularity     INTEGER NULL,
            //         PRIMARY KEY (race_id, horse_id)
            // );");

            // /* 過去：払戻（MVP:単複） */
            // Execute(conn, @"
            //     CREATE TABLE fact_payout (
            //         race_id     TEXT NOT NULL,
            //         bet_type    TEXT NOT NULL,
            //         combo_text  TEXT NOT NULL,
            //         payout_yen  INTEGER NOT NULL,
            //         popularity  INTEGER NULL,
            //         PRIMARY KEY (race_id, bet_type, combo_text)
            // );");

            // /* 週末：出馬 */
            // Execute(conn, @"
            //     CREATE TABLE fact_race_entry (
            //         race_id        TEXT NOT NULL,
            //         horse_id       TEXT NOT NULL,
            //         horse_name     TEXT NOT NULL,
            //         umaban         INTEGER NULL,
            //         wakuban        INTEGER NULL,
            //         jockey_name    TEXT NULL,
            //         trainer_name   TEXT NULL,
            //         weight_carried REAL NULL,
            //         PRIMARY KEY (race_id, horse_id)
            // );");

            // /* 週末：指数（PKは race_id + umaban で安全に） */
            // Execute(conn, @"
            //     CREATE TABLE fact_race_expectation (
            //         race_id       TEXT NOT NULL,
            //         umaban        INTEGER NOT NULL,
            //         horse_id      TEXT NULL,
            //         horse_name    TEXT NULL,
            //         ability       REAL NULL,
            //         mult_course   REAL NULL,
            //         mult_style    REAL NULL,
            //         mult_frame    REAL NULL,
            //         mult_blood    REAL NULL,
            //         mult_jockey   REAL NULL,
            //         expected_raw  REAL NULL,
            //         expected_100  REAL NULL,
            //         PRIMARY KEY (race_id, umaban)
            // );");

            // /* 血統（週末出走馬限定で軽量） */
            // Execute(conn, @"
            //     CREATE TABLE dim_horse_pedigree (
            //         horse_id TEXT PRIMARY KEY,
            //         sire     TEXT NULL,
            //         dam      TEXT NULL,
            //         siresire TEXT NULL
            // );");

            // /* 新聞ビュー */
            // Execute(conn, @"
            //     CREATE VIEW vw_newspaper_rows AS
            //     SELECT
            //         e.race_id,
            //         e.wakuban,
            //         e.umaban,
            //         e.horse_id,
            //         e.horse_name,
            //         e.jockey_name,
            //         e.trainer_name,
            //         e.weight_carried,
            //         x.ability,
            //         x.mult_course,
            //         x.mult_style,
            //         x.mult_frame,
            //         x.mult_blood,
            //         x.mult_jockey,
            //         x.expected_raw,
            //         x.expected_100,
            //         p.sire,
            //         p.dam,
            //         p.siresire
            //     FROM fact_race_entry e
            //     LEFT JOIN fact_race_expectation x
            //         ON x.race_id = e.race_id AND x.umaban = e.umaban
            //     LEFT JOIN dim_horse_pedigree p
            //         ON p.horse_id = e.horse_id;
            // ");

            // /* 索引（体感用） */
            // Execute(conn, "CREATE INDEX idx_dim_race_search ON dim_race (race_date, jyo_cd, surface_type, distance_m, class_simple, win5_flg);");
            // Execute(conn, "CREATE INDEX idx_dim_race_name ON dim_race (race_name);");
            // Execute(conn, "CREATE INDEX idx_result_race ON fact_race_result (race_id, finish_pos);");
            // Execute(conn, "CREATE INDEX idx_payout_race ON fact_payout (race_id);");
            // Execute(conn, "CREATE INDEX idx_entry_race ON fact_race_entry (race_id, umaban);");
            // Execute(conn, "CREATE INDEX idx_expect_race ON fact_race_expectation (race_id);");
        }

        /// <summary>
        /// 非クエリSQLを実行する。
        /// </summary>
        private static void Execute(SqliteConnection conn, string sql)
        {
            using (SqliteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
