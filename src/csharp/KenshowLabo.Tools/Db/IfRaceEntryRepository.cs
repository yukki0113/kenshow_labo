using System;
using System.Collections.Generic;
using System.Data;
using KenshowLabo.Tools.Models;
using Microsoft.Data.SqlClient;

namespace KenshowLabo.Tools.Db
{
    /// <summary>
    /// dbo.IF_RaceEntry への書き込み処理を提供します。
    /// </summary>
    public static class IfRaceEntryRepository
    {
        /// <summary>
        /// race_id 単位で IF_RaceEntry を全置換（DELETE→INSERT）します。
        /// </summary>
        /// <param name="connectionString">接続文字列</param>
        /// <param name="raceId">race_id</param>
        /// <param name="rows">INSERTする行</param>
        public static void ReplaceByRaceId(string connectionString, long raceId, List<IfRaceEntryRow> rows)
        {
            // 入力チェック
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("connectionString is empty.");
            }

            if (raceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(raceId));
            }

            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            // DB接続
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // トランザクション開始
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) 対象 race_id の既存行を削除
                        DeleteByRaceId(conn, tx, raceId);

                        // 2) 新規行を INSERT
                        InsertRows(conn, tx, raceId, rows);

                        // コミット
                        tx.Commit();
                    }
                    catch
                    {
                        // ロールバック（例外は握りつぶさない）
                        try
                        {
                            tx.Rollback();
                        }
                        catch
                        {
                            // rollback失敗は無視（元の例外を優先）
                        }

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// race_id に一致する行を削除します。
        /// </summary>
        private static void DeleteByRaceId(SqlConnection conn, SqlTransaction tx, long raceId)
        {
            // DELETE SQL
            string sql = "DELETE FROM dbo.IF_RaceEntry WHERE race_id = @race_id;";

            // 実行
            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.CommandType = CommandType.Text;

                // パラメータ
                SqlParameter pRaceId = DbUtil.CreateParameter("@race_id", SqlDbType.BigInt, raceId);
                cmd.Parameters.Add(pRaceId);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 行リストを INSERT します。
        /// </summary>
        private static void InsertRows(SqlConnection conn, SqlTransaction tx, long raceId, List<IfRaceEntryRow> rows)
        {
            // INSERT SQL（IF_RaceEntry の定義に完全一致）
            string sql =
                "INSERT INTO dbo.IF_RaceEntry (" +
                "  race_id, horse_id," +
                "  race_no, race_date, race_name, race_class, classSimple, track_name, surface_type, distance_m," +
                "  meeting, turn, course_inout, going, weather," +
                "  frame_no, horse_no, horse_name, sex, age, jockey_name, carried_weight," +
                "  scraped_at, source_url" +
                ") VALUES (" +
                "  @race_id, @horse_id," +
                "  @race_no, @race_date, @race_name, @race_class, @classSimple, @track_name, @surface_type, @distance_m," +
                "  @meeting, @turn, @course_inout, @going, @weather," +
                "  @frame_no, @horse_no, @horse_name, @sex, @age, @jockey_name, @carried_weight," +
                "  @scraped_at, @source_url" +
                ");";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.CommandType = CommandType.Text;

                // ----------------------------
                // パラメータ生成（CreateParameter 形式に合わせる）
                // ----------------------------

                SqlParameter pRaceId = DbUtil.CreateParameter("@race_id", SqlDbType.BigInt, 0L);
                cmd.Parameters.Add(pRaceId);

                SqlParameter pHorseId = DbUtil.CreateParameter("@horse_id", SqlDbType.BigInt, 0L);
                cmd.Parameters.Add(pHorseId);

                SqlParameter pRaceNo = DbUtil.CreateParameter("@race_no", SqlDbType.TinyInt, (byte)0);
                cmd.Parameters.Add(pRaceNo);

                SqlParameter pRaceDate = DbUtil.CreateParameter("@race_date", SqlDbType.Date, DateTime.MinValue);
                cmd.Parameters.Add(pRaceDate);

                SqlParameter pRaceName = DbUtil.CreateParameter("@race_name", SqlDbType.NVarChar, string.Empty);
                pRaceName.Size = 50;
                cmd.Parameters.Add(pRaceName);

                SqlParameter pRaceClass = DbUtil.CreateParameter("@race_class", SqlDbType.NVarChar, DBNull.Value);
                pRaceClass.Size = 20;
                cmd.Parameters.Add(pRaceClass);

                SqlParameter pClassSimple = DbUtil.CreateParameter("@classSimple", SqlDbType.NVarChar, DBNull.Value);
                pClassSimple.Size = 20;
                cmd.Parameters.Add(pClassSimple);

                SqlParameter pTrackName = DbUtil.CreateParameter("@track_name", SqlDbType.NVarChar, string.Empty);
                pTrackName.Size = 3;
                cmd.Parameters.Add(pTrackName);

                SqlParameter pSurfaceType = DbUtil.CreateParameter("@surface_type", SqlDbType.NVarChar, string.Empty);
                pSurfaceType.Size = 10;
                cmd.Parameters.Add(pSurfaceType);

                SqlParameter pDistanceM = DbUtil.CreateParameter("@distance_m", SqlDbType.Int, 0);
                cmd.Parameters.Add(pDistanceM);

                SqlParameter pMeeting = DbUtil.CreateParameter("@meeting", SqlDbType.NVarChar, DBNull.Value);
                pMeeting.Size = 12;
                cmd.Parameters.Add(pMeeting);

                SqlParameter pTurn = DbUtil.CreateParameter("@turn", SqlDbType.NVarChar, DBNull.Value);
                pTurn.Size = 1;
                cmd.Parameters.Add(pTurn);

                SqlParameter pCourseInOut = DbUtil.CreateParameter("@course_inout", SqlDbType.NVarChar, DBNull.Value);
                pCourseInOut.Size = 1;
                cmd.Parameters.Add(pCourseInOut);

                SqlParameter pGoing = DbUtil.CreateParameter("@going", SqlDbType.NVarChar, DBNull.Value);
                pGoing.Size = 1;
                cmd.Parameters.Add(pGoing);

                SqlParameter pWeather = DbUtil.CreateParameter("@weather", SqlDbType.NVarChar, DBNull.Value);
                pWeather.Size = 1;
                cmd.Parameters.Add(pWeather);

                SqlParameter pFrameNo = DbUtil.CreateParameter("@frame_no", SqlDbType.Int, DBNull.Value);
                cmd.Parameters.Add(pFrameNo);

                SqlParameter pHorseNo = DbUtil.CreateParameter("@horse_no", SqlDbType.Int, 0);
                cmd.Parameters.Add(pHorseNo);

                SqlParameter pHorseName = DbUtil.CreateParameter("@horse_name", SqlDbType.NVarChar, string.Empty);
                pHorseName.Size = 30;
                cmd.Parameters.Add(pHorseName);

                SqlParameter pSex = DbUtil.CreateParameter("@sex", SqlDbType.NVarChar, DBNull.Value);
                pSex.Size = 1;
                cmd.Parameters.Add(pSex);

                SqlParameter pAge = DbUtil.CreateParameter("@age", SqlDbType.Int, DBNull.Value);
                cmd.Parameters.Add(pAge);

                SqlParameter pJockeyName = DbUtil.CreateParameter("@jockey_name", SqlDbType.NVarChar, DBNull.Value);
                pJockeyName.Size = 10;
                cmd.Parameters.Add(pJockeyName);

                // DECIMAL(3,1) は Precision/Scale を明示
                SqlParameter pCarriedWeight = DbUtil.CreateParameter("@carried_weight", SqlDbType.Decimal, DBNull.Value);
                pCarriedWeight.Precision = 3;
                pCarriedWeight.Scale = 1;
                cmd.Parameters.Add(pCarriedWeight);

                SqlParameter pScrapedAt = DbUtil.CreateParameter("@scraped_at", SqlDbType.DateTime2, DateTime.Now);
                cmd.Parameters.Add(pScrapedAt);

                SqlParameter pSourceUrl = DbUtil.CreateParameter("@source_url", SqlDbType.NVarChar, DBNull.Value);
                pSourceUrl.Size = 500;
                cmd.Parameters.Add(pSourceUrl);

                // ----------------------------
                // 行ごとに値をセットして実行
                // ----------------------------

                foreach (IfRaceEntryRow row in rows)
                {
                    // race_id は引数で固定（揺れ防止）
                    pRaceId.Value = raceId;

                    // 主キー相当
                    pHorseId.Value = row.HorseId;

                    // レース情報
                    pRaceNo.Value = row.RaceNo;
                    pRaceDate.Value = row.RaceDate.Date;
                    pRaceName.Value = row.RaceName;

                    pRaceClass.Value = ToDbNullIfBlank(row.RaceClass);
                    pClassSimple.Value = ToDbNullIfBlank(row.ClassSimple);

                    pTrackName.Value = row.TrackName;
                    pSurfaceType.Value = row.SurfaceType;
                    pDistanceM.Value = row.DistanceM;

                    pMeeting.Value = ToDbNullIfBlank(row.Meeting);
                    pTurn.Value = ToDbNullIfBlank(row.Turn);
                    pCourseInOut.Value = ToDbNullIfBlank(row.CourseInOut);
                    pGoing.Value = ToDbNullIfBlank(row.Going);
                    pWeather.Value = ToDbNullIfBlank(row.Weather);

                    // 出走馬情報
                    if (row.FrameNo.HasValue)
                    {
                        pFrameNo.Value = row.FrameNo.Value;
                    }
                    else
                    {
                        pFrameNo.Value = DBNull.Value;
                    }

                    // horse_no は NOT NULL（未確定は 101～）
                    pHorseNo.Value = row.HorseNo;

                    pHorseName.Value = row.HorseName;
                    pSex.Value = ToDbNullIfBlank(row.Sex);

                    if (row.Age.HasValue)
                    {
                        pAge.Value = row.Age.Value;
                    }
                    else
                    {
                        pAge.Value = DBNull.Value;
                    }

                    pJockeyName.Value = ToDbNullIfBlank(row.JockeyName);

                    if (row.CarriedWeight.HasValue)
                    {
                        pCarriedWeight.Value = row.CarriedWeight.Value;
                    }
                    else
                    {
                        pCarriedWeight.Value = DBNull.Value;
                    }

                    // 管理情報
                    pScrapedAt.Value = row.ScrapedAt;
                    pSourceUrl.Value = ToDbNullIfBlank(row.SourceUrl);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 空文字・空白は DBNull に変換します。
        /// </summary>
        private static object ToDbNullIfBlank(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DBNull.Value;
            }

            return value;
        }
    }
}
