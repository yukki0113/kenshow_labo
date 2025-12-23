using System;
using System.Collections.Generic;
using System.Data;
using KenshowLabo.Tools.Db;
using Microsoft.Data.SqlClient;

namespace KenshowLabo.Tools.Db {
    /// <summary>
    /// 競馬場名からJV準拠の場CD（2桁）を解決します。
    /// </summary>
    public sealed class TrackCodeResolver {
        private readonly string _connectionString;
        private readonly Dictionary<string, string> _cache;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public TrackCodeResolver(string connectionString) {
            this._connectionString = connectionString;
            this._cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 競馬場名から場CD（2桁）を取得します。
        /// </summary>
        public string ResolveTrackCode(string trackName) {
            if (string.IsNullOrWhiteSpace(trackName)) {
                throw new ArgumentException("trackName is empty.");
            }

            string normalized = trackName.Trim();

            string cached;
            if (this._cache.TryGetValue(normalized, out cached)) {
                return cached;
            }

            string sql =
                "SELECT track_cd " +
                "FROM dbo.MT_Track " +
                "WHERE track_name = @track_name;";

            List<string> list = DbUtil.QueryToList(
                this._connectionString,
                sql,
                p => p.Add(DbUtil.CreateParameter("@track_name", SqlDbType.NVarChar, 10, normalized)),
                reader => SqlValueReader.ReadString(reader, "track_cd")
            );

            if (list.Count == 0) {
                throw new InvalidOperationException("MT_Track に競馬場名が見つかりません: " + normalized);
            }

            string trackCd = list[0];
            this._cache[normalized] = trackCd;

            return trackCd;
        }
    }
}
