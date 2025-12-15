using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KenshowLabo.Tools.Json
{
    public static class JsonOptionsFactory
    {
        /// <summary>
        /// 本プロジェクトの標準 JSON オプションを作成します。
        /// </summary>
        public static JsonSerializerOptions CreateDefault()
        {
            JsonSerializerOptions opt = new JsonSerializerOptions();

            // インデントして差分を読みやすくする
            opt.WriteIndented = true;

            // 日本語を含む文字列をそのまま出す（必要に応じて変更可）
            opt.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            // null を省略しない（seriesName/holdingNo を null で出したい）
            opt.DefaultIgnoreCondition = JsonIgnoreCondition.Never;

            return opt;
        }
    }
}
