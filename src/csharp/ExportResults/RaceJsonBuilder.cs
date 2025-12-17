using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KenshowLabo.Tools.Models;

namespace ExportResults
{
    internal static class RaceJsonBuilder
    {
        /// <summary>
        /// 契約VIEWのフラット行を race_id 単位にまとめて RaceJson 配列に変換します。
        /// </summary>
        public static List<RaceJson> Build(List<RaceResultContractRow> rows)
        {
            // race_id でグルーピングする
            List<IGrouping<string, RaceResultContractRow>> groups = rows
                .GroupBy(r => r.RaceId, System.StringComparer.Ordinal)
                .ToList();

            List<RaceJson> races = new List<RaceJson>();

            foreach (IGrouping<string, RaceResultContractRow> g in groups)
            {
                // レース共通情報は先頭行から採用する
                RaceResultContractRow head = g.First();

                // レースを構築する
                RaceJson race = new RaceJson();
                race.RaceId = head.RaceId;
                race.Date = head.RaceDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                race.RaceName = head.RaceName;

                // 現時点は null 固定（後で整形ロジックを追加）
                race.SeriesName = null;
                race.HoldingNo = null;
                race.Grade = null;

                // course を正規化して格納する
                race.Course = new CourseJson();
                race.Course.Track = head.Track;
                race.Course.Surface = head.Surface;
                race.Course.Distance = head.Distance;
                race.Course.Turn = head.Turn;

                race.Class = head.ClassName;
                race.ClassSimple = head.ClassSimple;
                race.Win5Target = head.Win5Flg;

                race.Weather = head.Weather;
                race.Going = head.Going;
                race.Meeting = head.Meeting;
                race.TrackId = head.TrackId;

                // entries を構築する
                List<EntryJson> entries = new List<EntryJson>();

                foreach (RaceResultContractRow row in g)
                {
                    EntryJson e = new EntryJson();

                    e.FrameNo = row.FrameNo;
                    e.HorseNo = row.HorseNo;
                    e.HorseId = row.HorseId;
                    e.HorseName = row.HorseName;
                    e.Sex = row.Sex;
                    e.Age = row.Age;
                    e.Jockey = row.Jockey;
                    e.CarriedWeight = row.CarriedWeight;
                    e.Popularity = row.Popularity;
                    e.Odds = row.Odds;
                    e.Finish = row.Finish;
                    e.Time = row.Time;
                    e.BodyWeight = row.BodyWeight;
                    e.BodyWeightDiff = row.BodyWeightDiff;

                    // VIEW算出の脚質ラベルをそのまま採用
                    e.RunningStyle = row.RunningStyle;

                    e.Passing = new PassingJson();
                    e.Passing.Corner1 = ConvertNullableToZero(row.Corner1);
                    e.Passing.Corner2 = ConvertNullableToZero(row.Corner2);
                    e.Passing.Corner3 = ConvertNullableToZero(row.Corner3);
                    e.Passing.Corner4 = ConvertNullableToZero(row.Corner4);

                    e.Last3f = row.Last3f;

                    entries.Add(e);
                }

                // entries を決定的な順序に整列する（finish（未確定は最後）→ horseNo）
                entries = entries
                    .OrderBy(e => NormalizeFinishForSort(e.Finish))
                    .ThenBy(e => e.HorseNo)
                    .ToList();

                race.Entries = entries;
                races.Add(race);
            }

            return races;
        }

        /// <summary>
        /// finish のソートキーを作ります（未確定/欠損は最後に寄せる）。
        /// </summary>
        private static int NormalizeFinishForSort(int? finish)
        {
            if (!finish.HasValue)
            {
                return int.MaxValue;
            }

            if (finish.Value <= 0)
            {
                return int.MaxValue;
            }

            return finish.Value;
        }

        /// <summary>
        /// passing の JSON は数値必須のため、NULLは0へ寄せます。
        /// </summary>
        private static int ConvertNullableToZero(int? value)
        {
            if (!value.HasValue)
            {
                return 0;
            }

            return value.Value;
        }
    }
}
