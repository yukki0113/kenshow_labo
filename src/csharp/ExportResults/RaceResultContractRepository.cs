using System.Collections.Generic;
using System.Data;
using System.Text;
using KenshowLabo.Tools.Db;
using KenshowLabo.Tools.Models;
using Microsoft.Data.SqlClient;

namespace ExportResults
{
    internal static class RaceResultContractRepository
    {
        /// <summary>
        /// dbo.VW_RaceResultContract_v1 から指定年（＋任意期間）を取得します。
        /// </summary>
        public static List<RaceResultContractRow> FetchRows(ExportOptions options)
        {
            // SQLを組み立てる
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT");
            sb.AppendLine("  race_id,");
            sb.AppendLine("  日付,");
            sb.AppendLine("  レース名,");
            sb.AppendLine("  クラス,");
            sb.AppendLine("  classSimple,");
            sb.AppendLine("  WIN5_flg,");
            sb.AppendLine("  場名,");
            sb.AppendLine("  芝_ダート,");
            sb.AppendLine("  距離,");
            sb.AppendLine("  枠番,");
            sb.AppendLine("  馬番,");
            sb.AppendLine("  horse_id,");
            sb.AppendLine("  馬名,");
            sb.AppendLine("  性,");
            sb.AppendLine("  齢,");
            sb.AppendLine("  騎手,");
            sb.AppendLine("  斤量,");
            sb.AppendLine("  人気,");
            sb.AppendLine("  オッズ,");
            sb.AppendLine("  着順,");
            sb.AppendLine("  走破時計,");
            sb.AppendLine("  馬体重,");
            sb.AppendLine("  馬体重変動,");
            sb.AppendLine("  通過順_1角,");
            sb.AppendLine("  通過順_2角,");
            sb.AppendLine("  通過順_3角,");
            sb.AppendLine("  通過順_4角,");
            sb.AppendLine("  上がり,");
            sb.AppendLine("  開催,");
            sb.AppendLine("  回り,");
            sb.AppendLine("  馬場,");
            sb.AppendLine("  天気,");
            sb.AppendLine("  track_id,");
            sb.AppendLine("  血統馬名,");
            sb.AppendLine("  父,");
            sb.AppendLine("  母,");
            sb.AppendLine("  母父,");
            sb.AppendLine("  母母,");
            sb.AppendLine("  父父,");
            sb.AppendLine("  父母,");
            sb.AppendLine("  頭数,");
            sb.AppendLine("  脚質,");
            sb.AppendLine("  単勝払戻,");
            sb.AppendLine("  単勝人気,");
            sb.AppendLine("  複勝払戻,");
            sb.AppendLine("  複勝人気");
            sb.AppendLine("FROM dbo.VW_RaceResultContract_v1");
            sb.AppendLine("WHERE YEAR(日付) = @Year");

            // 任意：期間フィルタ
            if (options.DateFrom.HasValue)
            {
                sb.AppendLine("  AND 日付 >= @DateFrom");
            }

            if (options.DateTo.HasValue)
            {
                sb.AppendLine("  AND 日付 <= @DateTo");
            }

            // 安定した取得順（最終ソートはC#側で確定）
            sb.AppendLine("ORDER BY 日付 ASC, race_id ASC, 馬番 ASC");

            string sql = sb.ToString();

            // DBから読み取る
            List<RaceResultContractRow> rows = DbUtil.QueryToList(
                options.ConnectionString,
                sql,
                (SqlParameterCollection p) =>
                {
                    // 必須パラメータ
                    p.Add(DbUtil.CreateParameter("@Year", SqlDbType.Int, options.Year));

                    // 任意パラメータ
                    if (options.DateFrom.HasValue)
                    {
                        p.Add(DbUtil.CreateParameter("@DateFrom", SqlDbType.Date, options.DateFrom.Value.Date));
                    }

                    if (options.DateTo.HasValue)
                    {
                        p.Add(DbUtil.CreateParameter("@DateTo", SqlDbType.Date, options.DateTo.Value.Date));
                    }
                },
                (SqlDataReader r) =>
                {
                    // 1行をRow DTOへマッピングする
                    RaceResultContractRow row = new RaceResultContractRow();

                    row.RaceId = DbUtil.ReadString(r, "race_id");
                    row.RaceDate = DbUtil.ReadDateTime(r, "日付");
                    row.RaceName = DbUtil.ReadString(r, "レース名");
                    row.ClassName = DbUtil.ReadString(r, "クラス");
                    row.ClassSimple = DbUtil.ReadString(r, "classSimple");
                    row.Win5Flg = DbUtil.ReadBool(r, "WIN5_flg");

                    row.Track = DbUtil.ReadString(r, "場名");
                    row.Surface = DbUtil.ReadString(r, "芝_ダート");
                    row.Distance = DbUtil.ReadInt(r, "距離");
                    row.Turn = DbUtil.ReadString(r, "回り");

                    row.Meeting = DbUtil.ReadString(r, "開催");
                    row.Going = DbUtil.ReadString(r, "馬場");
                    row.Weather = DbUtil.ReadString(r, "天気");
                    row.TrackId = DbUtil.ReadString(r, "track_id");

                    row.FrameNo = DbUtil.ReadInt(r, "枠番");
                    row.HorseNo = DbUtil.ReadInt(r, "馬番");
                    row.HorseId = DbUtil.ReadString(r, "horse_id");
                    row.HorseName = DbUtil.ReadString(r, "馬名");
                    row.Sex = DbUtil.ReadString(r, "性");
                    row.Age = DbUtil.ReadInt(r, "齢");
                    row.Jockey = DbUtil.ReadString(r, "騎手");
                    row.CarriedWeight = DbUtil.ReadDecimal(r, "斤量");
                    row.Popularity = DbUtil.ReadInt(r, "人気");
                    row.Odds = DbUtil.ReadDecimal(r, "オッズ");
                    row.Finish = DbUtil.ReadNullableInt(r, "着順");
                    row.Time = DbUtil.ReadString(r, "走破時計");
                    row.BodyWeight = DbUtil.ReadInt(r, "馬体重");
                    row.BodyWeightDiff = DbUtil.ReadInt(r, "馬体重変動");

                    row.Corner1 = DbUtil.ReadNullableInt(r, "通過順_1角");
                    row.Corner2 = DbUtil.ReadNullableInt(r, "通過順_2角");
                    row.Corner3 = DbUtil.ReadNullableInt(r, "通過順_3角");
                    row.Corner4 = DbUtil.ReadNullableInt(r, "通過順_4角");
                    row.Last3f = DbUtil.ReadDecimal(r, "上がり");

                    row.PedigreeHorseName = DbUtil.ReadString(r, "血統馬名");
                    row.Sire = DbUtil.ReadString(r, "父");
                    row.Dam = DbUtil.ReadString(r, "母");
                    row.DamSire = DbUtil.ReadString(r, "母父");
                    row.DamDam = DbUtil.ReadString(r, "母母");
                    row.SireSire = DbUtil.ReadString(r, "父父");
                    row.SireDam = DbUtil.ReadString(r, "父母");

                    row.FieldSize = DbUtil.ReadInt(r, "頭数");
                    row.RunningStyle = DbUtil.ReadString(r, "脚質");

                    row.WinPayout = DbUtil.ReadNullableInt(r, "単勝払戻");
                    row.WinPopularity = DbUtil.ReadNullableInt(r, "単勝人気");
                    row.PlacePayout = DbUtil.ReadNullableInt(r, "複勝払戻");
                    row.PlacePopularity = DbUtil.ReadNullableInt(r, "複勝人気");

                    return row;
                });

            return rows;
        }
    }
}
