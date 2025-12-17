using System;
using System.IO;
using System.Text.Json;

namespace KenshowLabo.ScrapingRunner {
    /// <summary>
    /// settings.json の読み書きを担当します。
    /// </summary>
    public sealed class SettingsRepository {
        private readonly string _settingsPath;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public SettingsRepository(string settingsPath) {
            _settingsPath = settingsPath;
        }

        /// <summary>
        /// settings.json を読み込みます。存在しない場合はデフォルトを生成して返します。
        /// </summary>
        public AppSettings LoadOrCreate() {
            // 初回起動：ファイルが無ければデフォルト生成
            if (!File.Exists(_settingsPath)) {
                AppSettings created = new AppSettings();
                Save(created);
                return created;
            }

            // ファイル読み込み
            string json = File.ReadAllText(_settingsPath);

            // デシリアライズ（失敗時はデフォルトにフォールバック）
            AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);
            if (settings == null) {
                AppSettings fallback = new AppSettings();
                Save(fallback);
                return fallback;
            }

            return settings;
        }

        /// <summary>
        /// settings.json を保存します。
        /// </summary>
        public void Save(AppSettings settings) {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;

            string json = JsonSerializer.Serialize(settings, options);

            string? dir = Path.GetDirectoryName(_settingsPath);
            if (!String.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(_settingsPath, json);
        }
    }
}
