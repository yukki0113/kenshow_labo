using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KenshowLabo.ScrapingRunner {
    /// <summary>
    /// エラーログの出力（WARN/ERRORのみ）と、直前INFOのリングバッファ保持を行います。
    /// </summary>
    public sealed class ErrorLogWriter {
        private readonly object _lockObj;
        private readonly Queue<string> _recentInfo;
        private readonly int _recentInfoCapacity;

        private readonly string _logDirPath;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public ErrorLogWriter(string baseDirectory, int recentInfoCapacity) {
            _lockObj = new object();
            _recentInfo = new Queue<string>();
            _recentInfoCapacity = recentInfoCapacity;

            _logDirPath = Path.Combine(baseDirectory, "errLog");
        }

        /// <summary>
        /// INFO行をリングバッファに積みます（ファイル出力はしません）。
        /// </summary>
        public void AddInfo(string line) {
            lock (_lockObj) {
                // 直前INFOをリングバッファに保持
                _recentInfo.Enqueue(line);
                while (_recentInfo.Count > _recentInfoCapacity) {
                    _recentInfo.Dequeue();
                }
            }
        }

        /// <summary>
        /// WARN/ERROR をファイルへ出力します（直前INFOも添付）。
        /// </summary>
        public void WriteWarnOrError(string level, string line) {
            string filePath = CreateLogFilePath();

            // ディレクトリ作成
            if (!Directory.Exists(_logDirPath)) {
                Directory.CreateDirectory(_logDirPath);
            }

            // ログ本文構築
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("=== WARN/ERROR DETECTED ===");
            sb.AppendLine("Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("Level: " + level);
            sb.AppendLine("Message:");
            sb.AppendLine(line);
            sb.AppendLine();

            sb.AppendLine("=== Recent INFO (Ring Buffer) ===");
            lock (_lockObj) {
                foreach (string info in _recentInfo) {
                    sb.AppendLine(info);
                }
            }

            sb.AppendLine();
            sb.AppendLine("=== End ===");

            // 追記
            File.AppendAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// 生成するログファイルパスを返します（日時付き）。
        /// </summary>
        private string CreateLogFilePath() {
            string fileName = "log_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            string path = Path.Combine(_logDirPath, fileName);
            return path;
        }
    }
}
