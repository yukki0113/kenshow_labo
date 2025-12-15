using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExportResults
{
    internal sealed class RaceJson
    {
        [JsonPropertyName("raceId")]
        public string RaceId { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("raceName")]
        public string RaceName { get; set; }

        [JsonPropertyName("seriesName")]
        public string? SeriesName { get; set; }

        [JsonPropertyName("holdingNo")]
        public int? HoldingNo { get; set; }

        [JsonPropertyName("course")]
        public CourseJson Course { get; set; }

        [JsonPropertyName("grade")]
        public string? Grade { get; set; }

        [JsonPropertyName("class")]
        public string Class { get; set; }

        [JsonPropertyName("classSimple")]
        public string ClassSimple { get; set; }

        [JsonPropertyName("win5Target")]
        public bool Win5Target { get; set; }

        [JsonPropertyName("weather")]
        public string Weather { get; set; }

        [JsonPropertyName("going")]
        public string Going { get; set; }

        [JsonPropertyName("meeting")]
        public string Meeting { get; set; }

        [JsonPropertyName("trackId")]
        public string TrackId { get; set; }

        [JsonPropertyName("entries")]
        public List<EntryJson> Entries { get; set; }

        public RaceJson()
        {
            this.RaceId = string.Empty;
            this.Date = string.Empty;
            this.RaceName = string.Empty;

            this.SeriesName = null;
            this.HoldingNo = null;

            this.Course = new CourseJson();
            this.Grade = null;

            this.Class = string.Empty;
            this.ClassSimple = string.Empty;
            this.Weather = string.Empty;
            this.Going = string.Empty;
            this.Meeting = string.Empty;
            this.TrackId = string.Empty;

            this.Entries = new List<EntryJson>();
        }
    }

    internal sealed class CourseJson
    {
        [JsonPropertyName("track")]
        public string Track { get; set; }

        [JsonPropertyName("surface")]
        public string Surface { get; set; }

        [JsonPropertyName("distance")]
        public int Distance { get; set; }

        [JsonPropertyName("turn")]
        public string Turn { get; set; }

        public CourseJson()
        {
            this.Track = string.Empty;
            this.Surface = string.Empty;
            this.Distance = 0;
            this.Turn = string.Empty;
        }
    }

    internal sealed class EntryJson
    {
        [JsonPropertyName("frameNo")]
        public int FrameNo { get; set; }

        [JsonPropertyName("horseNo")]
        public int HorseNo { get; set; }

        [JsonPropertyName("horseId")]
        public string HorseId { get; set; }

        [JsonPropertyName("horseName")]
        public string HorseName { get; set; }

        [JsonPropertyName("sex")]
        public string Sex { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("jockey")]
        public string Jockey { get; set; }

        [JsonPropertyName("carriedWeight")]
        public decimal CarriedWeight { get; set; }

        [JsonPropertyName("popularity")]
        public int Popularity { get; set; }

        [JsonPropertyName("odds")]
        public decimal Odds { get; set; }

        [JsonPropertyName("finish")]
        public int? Finish { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("bodyWeight")]
        public int BodyWeight { get; set; }

        [JsonPropertyName("bodyWeightDiff")]
        public int BodyWeightDiff { get; set; }

        [JsonPropertyName("runningStyle")]
        public string RunningStyle { get; set; }

        [JsonPropertyName("passing")]
        public PassingJson Passing { get; set; }

        [JsonPropertyName("last3f")]
        public decimal Last3f { get; set; }

        public EntryJson()
        {
            this.HorseId = string.Empty;
            this.HorseName = string.Empty;
            this.Sex = string.Empty;
            this.Jockey = string.Empty;
            this.Time = string.Empty;
            this.RunningStyle = string.Empty;
            this.Passing = new PassingJson();
        }
    }

    internal sealed class PassingJson
    {
        [JsonPropertyName("corner1")]
        public int Corner1 { get; set; }

        [JsonPropertyName("corner2")]
        public int Corner2 { get; set; }

        [JsonPropertyName("corner3")]
        public int Corner3 { get; set; }

        [JsonPropertyName("corner4")]
        public int Corner4 { get; set; }
    }
}
