using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using KenshowLabo.Tools.Json;

namespace ExportResults
{
    internal static class ResultsIndexBuilder
    {
        /// <summary>
        /// results フォルダ内の results_YYYY.json を走査して index を生成します。
        /// </summary>
        public static ResultsIndexJson BuildFromResultsDir(string resultsDir)
        {
            ResultsIndexJson index = new ResultsIndexJson();
            index.LastUpdated = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

            // 対象ファイルを列挙する
            string[] files = Directory.GetFiles(resultsDir, "results_*.json", SearchOption.TopDirectoryOnly);

            List<ResultsYearItemJson> items = new List<ResultsYearItemJson>();

            foreach (string path in files)
            {
                string fileName = Path.GetFileName(path);

                // 年を抜き出す
                int year = TryParseYearFromFileName(fileName);
                if (year <= 0)
                {
                    continue;
                }

                // ファイルを読み、raceCount/rowCount を算出する
                string json = File.ReadAllText(path);

                JsonSerializerOptions options = JsonOptionsFactory.CreateDefault();
                List<RaceJson>? races = JsonSerializer.Deserialize<List<RaceJson>>(json, options);

                int raceCount = 0;
                int rowCount = 0;

                if (races != null)
                {
                    raceCount = races.Count;

                    foreach (RaceJson r in races)
                    {
                        if (r.Entries != null)
                        {
                            rowCount = rowCount + r.Entries.Count;
                        }
                    }
                }

                DateTimeOffset lastWrite = File.GetLastWriteTime(path);

                ResultsYearItemJson item = new ResultsYearItemJson();
                item.Year = year;
                item.File = "results/" + fileName;
                item.RaceCount = raceCount;
                item.RowCount = rowCount;
                item.LastUpdated = lastWrite.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

                items.Add(item);
            }

            // 年で昇順にして決定的に
            items.Sort((ResultsYearItemJson a, ResultsYearItemJson b) => a.Year.CompareTo(b.Year));
            index.Years = items;

            return index;
        }

        /// <summary>
        /// results_YYYY.json から年を取得します。
        /// </summary>
        private static int TryParseYearFromFileName(string fileName)
        {
            if (!fileName.StartsWith("results_", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            string core = fileName.Substring("results_".Length);

            if (core.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                core = core.Substring(0, core.Length - ".json".Length);
            }

            int year;
            if (int.TryParse(core, NumberStyles.Integer, CultureInfo.InvariantCulture, out year))
            {
                return year;
            }

            return 0;
        }
    }
}
