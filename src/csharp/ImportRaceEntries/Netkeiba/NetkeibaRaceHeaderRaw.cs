using System;
using System.Collections.Generic;

namespace ImportRaceEntries.Netkeiba.Models {
    public sealed class NetkeibaRaceHeaderRaw {
        public Guid ImportBatchId { get; set; }
        public string RaceId { get; set; } = string.Empty;

        public string? RaceNoRaw { get; set; }
        public string? RaceDateRaw { get; set; }
        public string? RaceNameRaw { get; set; }
        public string? TrackNameRaw { get; set; }
        public string? SurfaceDistanceRaw { get; set; }

        public DateTime ScrapedAt { get; set; }
        public string? SourceUrl { get; set; }
        public string? SourceFile { get; set; }
    }

    public sealed class NetkeibaRaceEntryRowRaw {
        public Guid ImportBatchId { get; set; }
        public int RowNo { get; set; }

        public string? NkHorseId { get; set; }
        public string? JvHorseId { get; set; }

        public string? FrameNoRaw { get; set; }
        public string? HorseNoRaw { get; set; }
        public string? HorseNameRaw { get; set; }
        public string? SexAgeRaw { get; set; }
        public string? JockeyNameRaw { get; set; }
        public string? CarriedWeightRaw { get; set; }
    }
}
