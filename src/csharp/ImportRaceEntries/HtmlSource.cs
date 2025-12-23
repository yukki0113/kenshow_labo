using System.Collections.Generic;
using System.IO;

namespace ImportRaceEntries {
    internal static class HtmlSource {
        /// <summary>
        /// 入力パスがファイルなら1件、フォルダならpatternで列挙します。
        /// </summary>
        public static List<string> EnumerateHtmlFiles(string inputPath, string pattern) {
            List<string> files = new List<string>();

            if (File.Exists(inputPath)) {
                files.Add(inputPath);
                return files;
            }

            if (Directory.Exists(inputPath)) {
                string[] arr = Directory.GetFiles(inputPath, pattern, SearchOption.TopDirectoryOnly);

                foreach (string f in arr) {
                    files.Add(f);
                }

                return files;
            }

            return files;
        }
    }
}
