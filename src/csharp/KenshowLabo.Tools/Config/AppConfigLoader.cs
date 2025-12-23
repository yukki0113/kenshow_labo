using System;
using Microsoft.Extensions.Configuration;

namespace KenshowLabo.Tools.Config {
    public static class AppConfigLoader {
        /// <summary>
        /// appsettings.json / appsettings.local.json を読み込みます。
        /// 実行ファイルのあるフォルダ（AppContext.BaseDirectory）を基準に探索します。
        /// </summary>
        public static IConfigurationRoot Load() {
            string baseDir = AppContext.BaseDirectory;

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(baseDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false);

            IConfigurationRoot config = builder.Build();
            return config;
        }

        /// <summary>
        /// Db:ConnectionString を取得します。未設定の場合は空文字を返します。
        /// </summary>
        public static string GetConnectionString(IConfiguration config) {
            string? cs = config["Db:ConnectionString"];

            if (string.IsNullOrWhiteSpace(cs)) {
                return string.Empty;
            }

            return cs;
        }
    }
}
