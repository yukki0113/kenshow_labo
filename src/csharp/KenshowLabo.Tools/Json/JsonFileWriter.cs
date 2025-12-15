using System.IO;
using System.Text;
using System.Text.Json;

namespace KenshowLabo.Tools.Json
{
    public static class JsonFileWriter
    {
        /// <summary>
        /// JSONを決定的に出力します。既存ファイルと内容が同一なら書き換えません。
        /// </summary>
        public static void WriteIfChanged<T>(string path, T obj, JsonSerializerOptions options)
        {
            // JSONを生成する
            string json = JsonSerializer.Serialize(obj, options);
            json = json + System.Environment.NewLine;

            // 既存ファイルと比較する（差分がなければ書き込まない）
            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path, Encoding.UTF8);
                if (string.Equals(existing, json, System.StringComparison.Ordinal))
                {
                    return;
                }
            }

            // UTF-8 (BOMなし) で書き出す
            File.WriteAllText(path, json, new UTF8Encoding(false));
        }

        /// <summary>
        /// 標準オプションで書き出します。
        /// </summary>
        public static void WriteIfChanged<T>(string path, T obj)
        {
            JsonSerializerOptions options = JsonOptionsFactory.CreateDefault();
            WriteIfChanged(path, obj, options);
        }
    }
}
