using System;
using System.Collections.Generic;
using System.IO;
using KenshowLabo.Tools.Config;
using KenshowLabo.Tools.Models;

namespace ImportRaceEntries {
    internal static class RaceEntryParser {
        /// <summary>
        /// HTMLを解析して RaceEntryRow に変換します。
        /// 現時点は雛形のため、後でHTML仕様に合わせて実装します。
        /// </summary>
        public static List<RaceEntryRow> Parse(string html, string sourceFile) {
            // TODO: HTML仕様確定後に実装
            // ここでは「通し稼働」のため空で返します。
            List<RaceEntryRow> rows = new List<RaceEntryRow>();

            // 解析結果が0件でも、アプリ全体は落ちないようにしておく
            return rows;
        }

        /// <summary>
        /// 1ファイルを読み込み、解析します。
        /// </summary>
        public static List<RaceEntryRow> ParseFile(string filePath) {
            string html = File.ReadAllText(filePath);
            return Parse(html, filePath);
        }
    }
}
