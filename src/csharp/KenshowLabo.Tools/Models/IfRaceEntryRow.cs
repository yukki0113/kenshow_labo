using System;

namespace KenshowLabo.Tools.Models {
    /// <summary>
    /// dbo.IF_RaceEntry の1行（1出走馬）を表す Row DTO です。
    /// IFは「取込直近のパース済み生データ」を想定します。
    /// </summary>
    public sealed class IfRaceEntryRow {
        // ----------------------------
        // キー（現行DDLは (race_id, horse_no) だが horse_no NULL 許容ならPKにできないため注意）
        // ----------------------------

        /// <summary>
        /// race_id（CHAR(12)）
        /// </summary>
        public string RaceId { get; set; }

        /// <summary>
        /// 馬番（未確定なら null）
        /// </summary>
        public int? HorseNo { get; set; }

        /// <summary>
        /// 枠番（未確定なら null）
        /// </summary>
        public int? FrameNo { get; set; }

        // ----------------------------
        // レース基本（取れない可能性を考え、NULL許容寄り）
        // ----------------------------

        public DateTime? RaceDate { get; set; }
        public string RaceName { get; set; }
        public string RaceClass { get; set; }
        public string ClassSimple { get; set; }
        public string TrackName { get; set; }
        public string SurfaceType { get; set; }
        public int? DistanceM { get; set; }
        public string Meeting { get; set; }
        public string Turn { get; set; }
        public string Going { get; set; }
        public string Weather { get; set; }
        public int? TrackId { get; set; }

        // ----------------------------
        // 出走馬
        // ----------------------------

        /// <summary>
        /// horse_id（IF段階では netkeiba由来が入る想定。JVに解決できるなら別工程で置換）
        /// </summary>
        public long? HorseId { get; set; }

        public string HorseName { get; set; }
        public string Sex { get; set; }
        public int? Age { get; set; }
        public string JockeyName { get; set; }
        public decimal? CarriedWeight { get; set; }

        // ----------------------------
        // 管理
        // ----------------------------

        public string SourceUrl { get; set; }
        public DateTime ScrapedAt { get; set; }

        /// <summary>
        /// コンストラクタです（文字列は空文字に寄せます）。
        /// </summary>
        public IfRaceEntryRow() {
            // 文字列は null を避け、空文字で初期化します
            this.RaceId = string.Empty;

            this.RaceName = string.Empty;
            this.RaceClass = string.Empty;
            this.ClassSimple = string.Empty;
            this.TrackName = string.Empty;
            this.SurfaceType = string.Empty;
            this.Meeting = string.Empty;
            this.Turn = string.Empty;
            this.Going = string.Empty;
            this.Weather = string.Empty;

            this.HorseName = string.Empty;
            this.Sex = string.Empty;
            this.JockeyName = string.Empty;

            this.SourceUrl = string.Empty;

            // 日時はとりあえず最小値（投入前に上書き）
            this.ScrapedAt = DateTime.MinValue;
        }
    }
}
