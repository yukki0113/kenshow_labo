using System;
using System.Globalization;
using System.IO;

namespace ExportResults
{
    internal sealed class ExportOptions
    {
        public string ConnectionString { get; private set; }
        public int Year { get; private set; }
        public string OutRoot { get; private set; }
        public DateTime? DateFrom { get; private set; }
        public DateTime? DateTo { get; private set; }

        private ExportOptions()
        {
            this.ConnectionString = string.Empty;
            this.Year = 0;
            this.OutRoot = Path.Combine("..", "..", "..", "..", "docs", "data");
            this.DateFrom = null;
            this.DateTo = null;
        }

        /// <summary>
        /// CLI引数を解釈します。
        /// </summary>
        public static ExportOptions Parse(string[] args)
        {
            ExportOptions opt = new ExportOptions();

            // 引数を走査する
            for (int i = 0; i < args.Length; i++)
            {
                string key = args[i];

                if (string.Equals(key, "--conn", StringComparison.OrdinalIgnoreCase))
                {
                    opt.ConnectionString = ReadValue(args, ref i);
                }
                else if (string.Equals(key, "--year", StringComparison.OrdinalIgnoreCase))
                {
                    string v = ReadValue(args, ref i);
                    opt.Year = int.Parse(v, CultureInfo.InvariantCulture);
                }
                else if (string.Equals(key, "--out-root", StringComparison.OrdinalIgnoreCase))
                {
                    opt.OutRoot = ReadValue(args, ref i);
                }
                else if (string.Equals(key, "--date-from", StringComparison.OrdinalIgnoreCase))
                {
                    string v = ReadValue(args, ref i);
                    opt.DateFrom = DateTime.ParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                else if (string.Equals(key, "--date-to", StringComparison.OrdinalIgnoreCase))
                {
                    string v = ReadValue(args, ref i);
                    opt.DateTo = DateTime.ParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
            }

            // 必須チェック
            if (string.IsNullOrWhiteSpace(opt.ConnectionString))
            {
                // appsettings(.local).json から読む
                Microsoft.Extensions.Configuration.IConfigurationRoot config = AppConfigLoader.Load();
                opt.ConnectionString = AppConfigLoader.GetConnectionString(config);
            }

            if (string.IsNullOrWhiteSpace(opt.ConnectionString))
            {
                throw new ArgumentException(
                    "DB接続文字列が見つかりません。--conn で渡すか、ExportResults/appsettings.local.json に Db:ConnectionString を設定してください。");
            }
            return opt;
        }

        /// <summary>
        /// 次のトークンを値として読み出します。
        /// </summary>
        private static string ReadValue(string[] args, ref int i)
        {
            if (i + 1 >= args.Length)
            {
                throw new ArgumentException("Missing value for " + args[i]);
            }

            i = i + 1;
            return args[i];
        }
    }
}
