using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExportResults
{
    internal sealed class ResultsIndexJson
    {
        [JsonPropertyName("lastUpdated")]
        public string LastUpdated { get; set; }

        [JsonPropertyName("years")]
        public List<ResultsYearItemJson> Years { get; set; }

        public ResultsIndexJson()
        {
            this.LastUpdated = string.Empty;
            this.Years = new List<ResultsYearItemJson>();
        }
    }

    internal sealed class ResultsYearItemJson
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("file")]
        public string File { get; set; }

        [JsonPropertyName("raceCount")]
        public int RaceCount { get; set; }

        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }

        [JsonPropertyName("lastUpdated")]
        public string LastUpdated { get; set; }

        public ResultsYearItemJson()
        {
            this.File = string.Empty;
            this.LastUpdated = string.Empty;
        }
    }
}
