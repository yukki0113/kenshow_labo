using System;
using System.Collections.Generic;
using KenshowLabo.Tools.Models;
using KenshowLabo.Tools.Config;

namespace ImportRaceEntries {
    internal static class Program {
        /// <summary>
        /// 出馬表HTMLを読み込み、DBへ投入します。
        /// </summary>
        public static int Main(string[] args) {
            ImportOptions options = ImportOptions.Parse(args);

            // 設定読込（appsettings.local.json）
            Microsoft.Extensions.Configuration.IConfigurationRoot config = AppConfigLoader.Load();
            string connectionString = AppConfigLoader.GetConnectionString(config);

            if (string.IsNullOrWhiteSpace(connectionString)) {
                throw new ArgumentException("Db:ConnectionString is empty. appsettings.local.json を確認してください。");
            }

            // 入力HTMLを列挙
            List<string> files = HtmlSource.EnumerateHtmlFiles(options.InputPath, options.Pattern);

            int parsedRowsTotal = 0;

            foreach (string file in files) {
                // 解析
                List<RaceEntryRow> rows = RaceEntryParser.ParseFile(file);
                parsedRowsTotal = parsedRowsTotal + rows.Count;

                // DB投入（0件ならスキップ）
                if (rows.Count > 0) {
                    IfRaceEntryRepository.UpsertMany(connectionString, rows);
                }

                Console.WriteLine("Processed: " + file + " rows=" + rows.Count);
            }

            Console.WriteLine("Done. totalRows=" + parsedRowsTotal);
            return 0;
        }
    }
}
