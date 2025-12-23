using System;
using System.Globalization;
using System.IO;

namespace ImportRaceEntries {
    internal sealed class ImportOptions {
        public string OutMode { get; private set; }           // "if" 固定運用など
        public string InputPath { get; private set; }         // フォルダ or ファイル
        public string Pattern { get; private set; }           // *.html など
        public DateTime? DateFrom { get; private set; }
        public DateTime? DateTo { get; private set; }

        public ImportOptions() {
            this.OutMode = "if";
            this.InputPath = string.Empty;
            this.Pattern = "*.html";
            this.DateFrom = null;
            this.DateTo = null;
        }

        /// <summary>
        /// CLI引数を解釈します。
        /// </summary>
        public static ImportOptions Parse(string[] args) {
            ImportOptions opt = new ImportOptions();

            for (int i = 0; i < args.Length; i++) {
                string key = args[i];

                if (string.Equals(key, "--input", StringComparison.OrdinalIgnoreCase)) {
                    opt.InputPath = ReadValue(args, ref i);
                }
                else if (string.Equals(key, "--pattern", StringComparison.OrdinalIgnoreCase)) {
                    opt.Pattern = ReadValue(args, ref i);
                }
                else if (string.Equals(key, "--date-from", StringComparison.OrdinalIgnoreCase)) {
                    string v = ReadValue(args, ref i);
                    opt.DateFrom = DateTime.ParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                else if (string.Equals(key, "--date-to", StringComparison.OrdinalIgnoreCase)) {
                    string v = ReadValue(args, ref i);
                    opt.DateTo = DateTime.ParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
            }

            if (string.IsNullOrWhiteSpace(opt.InputPath)) {
                throw new ArgumentException("Missing --input");
            }

            return opt;
        }

        private static string ReadValue(string[] args, ref int i) {
            if (i + 1 >= args.Length) {
                throw new ArgumentException("Missing value for " + args[i]);
            }

            i = i + 1;
            return args[i];
        }
    }
}
