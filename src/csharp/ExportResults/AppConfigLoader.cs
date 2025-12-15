using System;
using Microsoft.Extensions.Configuration;

namespace ExportResults
{
    internal static class AppConfigLoader
    {
        /// <summary>
        /// appsettings.json / appsettings.local.json を読み込み、設定を返します。
        /// </summary>
        public static IConfigurationRoot Load()
        {
            string baseDir = AppContext.BaseDirectory;

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(baseDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables(prefix: "KENSHOW_");

            IConfigurationRoot config = builder.Build();
            return config;
        }

        /// <summary>
        /// Db:ConnectionString から接続文字列を取得します（無い場合は空文字）。
        /// </summary>
        public static string GetConnectionString(IConfiguration config)
        {
            // NULL の可能性があるため、まず nullable で受ける
            string? cs = config["Db:ConnectionString"];

            // null/空白は空文字に寄せる
            if (string.IsNullOrWhiteSpace(cs))
            {
                return string.Empty;
            }

            return cs;
        }
    }
}
