using System;
using System.IO;
using System.Text;

namespace ImportRaceEntries.Netkeiba {
    /// <summary>
    /// 保存HTMLを安全に文字列へ変換します（UTF-8優先、崩れたらEUC-JPへフォールバック）。
    /// </summary>
    public static class HtmlFileReader {
        /// <summary>
        /// HTMLファイルを読み込み、文字列として返します。
        /// </summary>
        public static string ReadAllText(string path) {
            if (string.IsNullOrWhiteSpace(path)) {
                throw new ArgumentException("path is empty.");
            }

            // EUC-JP等を使うためにProvider登録（複数回呼んでも問題ありません）
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            byte[] bytes = File.ReadAllBytes(path);

            // 1) まずUTF-8で試す（正しい場合はそのまま）
            string utf8Text = DecodeUtf8(bytes);
            if (LooksMojibake(utf8Text)) {
                // 2) EUC-JPにフォールバック
                Encoding euc = Encoding.GetEncoding("euc-jp");
                string eucText = euc.GetString(bytes);
                return eucText;
            }

            return utf8Text;
        }

        private static string DecodeUtf8(byte[] bytes) {
            // strictで失敗したら置換で読む
            try {
                UTF8Encoding strict = new UTF8Encoding(false, true);
                return strict.GetString(bytes);
            }
            catch {
                UTF8Encoding relaxed = new UTF8Encoding(false, false);
                return relaxed.GetString(bytes);
            }
        }

        /// <summary>
        /// 文字化けっぽいかを簡易判定します（置換文字が多い等）。
        /// </summary>
        private static bool LooksMojibake(string text) {
            if (text == null) {
                return true;
            }

            int replacementCount = 0;
            for (int i = 0; i < text.Length; i++) {
                if (text[i] == '�') {
                    replacementCount++;
                }
            }

            // 置換文字が多い場合は文字化けとみなす
            if (replacementCount >= 100) {
                return true;
            }

            return false;
        }
    }
}
