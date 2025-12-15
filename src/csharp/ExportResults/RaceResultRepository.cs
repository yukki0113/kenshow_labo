using System.Collections.Generic;
using System.Data;
using System.Text;
using KenshowLabo.Tools.Db;
using KenshowLabo.Tools.Models;
using Microsoft.Data.SqlClient;

namespace ExportResults
{
    internal static class RaceResultRepository
    {
        /// <summary>
        /// TR_RaceResult から指定年（＋任意の期間）を取得します。
        /// ※列名は実DBに合わせて SELECT ... AS を調整してください。
        /// </summary>
        public static List<RaceResultRow> FetchRows(ExportOptions options)
        {
            // SQLを組み立てる
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT");
            sb.AppendLine("  race_id          AS RaceId,");
            sb.AppendLine("  [日付]           AS RaceDate,");
            sb.AppendLine("  [レース名]       AS RaceName,");
            sb.AppendLine("  [クラス]         AS ClassName,");
            sb.AppendLine("  classSimple      AS ClassSimple,");
            sb.AppendLine("  win5Target       AS Win5Target,");
            sb.AppendLine("  [場名]           AS Track,");
            sb.AppendLine("  [芝_ダート]      AS Surface,");
            sb.AppendLine("  [距離]           AS Distance,");
            sb.AppendLine("  [回り]           AS Turn,");
            sb.AppendLine("  [天気]           AS Weather,");
            sb.AppendLine("  [馬場]           AS Going,");
            sb.AppendLine("  [開催]           AS Meeting,");
            sb.AppendLine("  track_id         AS TrackId,");
            sb.AppendLine("  [枠番]           AS FrameNo,");
            sb.AppendLine("  [馬番]           AS HorseNo,");
            sb.AppendLine("  horse_id         AS HorseId,");
            sb.AppendLine("  [馬名]           AS HorseName,");
            sb.AppendLine("  [性]             AS Sex,");
            sb.AppendLine("  [齢]             AS Age,");
            sb.AppendLine("  [騎手]           AS Jockey,");
            sb.AppendLine("  [斤量]           AS CarriedWeight,");
            sb.AppendLine("  [人気]           AS Popularity,");
            sb.AppendLine("  [オッズ]         AS Odds,");
            sb.AppendLine("  [着順]           AS Finish,");
            sb.AppendLine("  [走破時計]       AS Time,");
            sb.AppendLine("  [馬体重]         AS BodyWeight,");
            sb.AppendLine("  [馬体重変動]     AS BodyWeightDiff,");
            sb.AppendLine("  [脚質ラベル]     AS RunningStyle,");
            sb.AppendLine("  [通過順_1角]     AS Corner1,");
            sb.AppendLine("  [通過順_2角]     AS Corner2,");
            sb.AppendLine("  [通過順_3角]     AS Corner3,");
            sb.AppendLine("  [通過順_4角]     AS Corner4,");
            sb.AppendLine("  [上がり]         AS Last3f");
            sb.AppendLine("FROM TR_RaceResult");
            sb.AppendLine("WHERE YEAR([日付]) = @Year");

            if (options.DateFrom.HasValue)
            {
                sb.AppendLine("  AND [日付] >= @DateFrom");
            }

            if (options.DateTo.HasValue)
            {
                sb.AppendLine("  AND [日付] <= @DateTo");
            }

            // 安定した読み取りのため、DB側でも一応整列（最終整列はC#で確定）
            sb.AppendLine("ORDER BY [日付] ASC, race_id ASC, [馬番] ASC");

            string sql = sb.ToString();

            // ToolsのDbUtilで読み取る
            List<RaceResultRow> rows = DbUtil.QueryToList(
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
                    // 1行を DTO にマッピングする
                    RaceResultRow row = new RaceResultRow();

                    row.RaceId = DbUtil.ReadString(r, "RaceId");
                    row.RaceDate = DbUtil.ReadDateTime(r, "RaceDate");
                    row.RaceName = DbUtil.ReadString(r, "RaceName");
                    row.ClassName = DbUtil.ReadString(r, "ClassName");
                    row.ClassSimple = DbUtil.ReadString(r, "ClassSimple");
                    row.Win5Target = DbUtil.ReadBool(r, "Win5Target");

                    row.Track = DbUtil.ReadString(r, "Track");
                    row.Surface = DbUtil.ReadString(r, "Surface");
                    row.Distance = DbUtil.ReadInt(r, "Distance");
                    row.Turn = DbUtil.ReadString(r, "Turn");

                    row.Weather = DbUtil.ReadString(r, "Weather");
                    row.Going = DbUtil.ReadString(r, "Going");
                    row.Meeting = DbUtil.ReadString(r, "Meeting");
                    row.TrackId = DbUtil.ReadString(r, "TrackId");

                    row.FrameNo = DbUtil.ReadInt(r, "FrameNo");
                    row.HorseNo = DbUtil.ReadInt(r, "HorseNo");
                    row.HorseId = DbUtil.ReadString(r, "HorseId");
                    row.HorseName = DbUtil.ReadString(r, "HorseName");
                    row.Sex = DbUtil.ReadString(r, "Sex");
                    row.Age = DbUtil.ReadInt(r, "Age");
                    row.Jockey = DbUtil.ReadString(r, "Jockey");
                    row.CarriedWeight = DbUtil.ReadDecimal(r, "CarriedWeight");
                    row.Popularity = DbUtil.ReadInt(r, "Popularity");
                    row.Odds = DbUtil.ReadDecimal(r, "Odds");
                    row.Finish = DbUtil.ReadNullableInt(r, "Finish");
                    row.Time = DbUtil.ReadString(r, "Time");
                    row.BodyWeight = DbUtil.ReadInt(r, "BodyWeight");
                    row.BodyWeightDiff = DbUtil.ReadInt(r, "BodyWeightDiff");
                    row.RunningStyle = DbUtil.ReadString(r, "RunningStyle");

                    row.Corner1 = DbUtil.ReadInt(r, "Corner1");
                    row.Corner2 = DbUtil.ReadInt(r, "Corner2");
                    row.Corner3 = DbUtil.ReadInt(r, "Corner3");
                    row.Corner4 = DbUtil.ReadInt(r, "Corner4");
                    row.Last3f = DbUtil.ReadDecimal(r, "Last3f");

                    return row;
                });

            return rows;
        }
    }
}
