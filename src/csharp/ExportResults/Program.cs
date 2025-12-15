using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using KenshowLabo.Tools.Json;
using KenshowLabo.Tools.Models;

namespace ExportResults
{
    internal static class Program
    {
        /// <summary>
        /// DB→年別JSON→index生成までを実行します。
        /// </summary>
        public static int Main(string[] args)
        {
            // 引数を解釈する
            ExportOptions options = ExportOptions.Parse(args);

            // 出力先フォルダを解決する
            string resultsDir = Path.Combine(options.OutRoot, "results");
            string indexDir = Path.Combine(options.OutRoot, "index");

            // フォルダが無ければ作成する
            Directory.CreateDirectory(resultsDir);
            Directory.CreateDirectory(indexDir);

            // DBからフラット行を取得する（TR_RaceResult 1行＝1頭）
            List<RaceResultRow> rows = RaceResultRepository.FetchRows(options);

            // 行を raceId 単位にまとめ、レース配列（entries付き）へ変換する
            List<RaceJson> races = RaceJsonBuilder.Build(rows);

            // 決定的な順序に整列する（date → raceId）
            races = races
                .OrderBy(r => r.Date, StringComparer.Ordinal)
                .ThenBy(r => r.RaceId, StringComparer.Ordinal)
                .ToList();

            // 年ファイルを書き出す（同一内容なら上書きしない）
            string yearFileName = "results_" + options.Year.ToString(CultureInfo.InvariantCulture) + ".json";
            string yearFilePath = Path.Combine(resultsDir, yearFileName);
            JsonFileWriter.WriteIfChanged(yearFilePath, races);

            // index を生成する（results フォルダを走査）
            ResultsIndexJson index = ResultsIndexBuilder.BuildFromResultsDir(resultsDir);

            // index を書き出す（同一内容なら上書きしない）
            string indexFilePath = Path.Combine(indexDir, "results_index.json");
            JsonFileWriter.WriteIfChanged(indexFilePath, index);

            // 実行ログ
            Console.WriteLine("Export done.");
            Console.WriteLine("Year file : " + yearFilePath);
            Console.WriteLine("Index file: " + indexFilePath);

            return 0;
        }
    }
}
