using System;
using System.Globalization;

namespace KenshowLabo.Tools.Domain {
    /// <summary>
    /// JV準拠の race_id（yyyyMMdd + 場CD(2) + R(2)）を生成します。
    /// </summary>
    public static class RaceIdBuilder {
        /// <summary>
        /// race_id（12桁）を生成します。
        /// </summary>
        public static long Build(DateTime raceDate, string trackCd, int raceNo) {
            if (string.IsNullOrWhiteSpace(trackCd)) {
                throw new ArgumentException("trackCd is empty.");
            }

            if (trackCd.Length != 2) {
                throw new ArgumentException("trackCd must be 2 chars.");
            }

            if (raceNo < 1 || raceNo > 12) {
                throw new ArgumentOutOfRangeException(nameof(raceNo), "raceNo must be 1..12.");
            }

            string ymd = raceDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string r2 = raceNo.ToString("00", CultureInfo.InvariantCulture);

            string s = ymd + trackCd + r2;

            long id;
            if (!long.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out id)) {
                throw new InvalidOperationException("race_id の生成に失敗しました: " + s);
            }

            return id;
        }
    }
}
