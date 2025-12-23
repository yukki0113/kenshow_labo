using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using KenshowLabo.Tools.Models;

namespace ImportRaceEntries.Netkeiba {
    /// <summary>
    /// netkeiba保存HTMLから出馬表情報を抽出します。
    /// </summary>
    public sealed class NetkeibaRaceParser {
        private static readonly Regex TitleRegex =
            new Regex(@"(?<y>\d{4})年(?<m>\d{1,2})月(?<d>\d{1,2})日\s*(?<track>札幌|函館|福島|新潟|中山|東京|中京|京都|阪神|小倉)\s*(?<rno>\d{1,2})R",
                RegexOptions.Compiled);

        /// <summary>
        /// HTML文字列を解析し、IF用の行リストを返します。
        /// </summary>
        public List<IfRaceEntryRow> ParseToIfRows(string html) {
            if (string.IsNullOrWhiteSpace(html)) {
                throw new ArgumentException("html is empty.");
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            RaceHeader header = ParseRaceHeader(doc);

            // 出馬表ページ優先
            HtmlNode shutubaTable = doc.DocumentNode.SelectSingleNode("//table[contains(@class,'Shutuba_Table')]");
            if (shutubaTable != null) {
                return ParseShutuba(header, shutubaTable);
            }

            // 結果ページ
            HtmlNode resultTable = doc.DocumentNode.SelectSingleNode("//table[contains(@class,'ResultRefund')]");
            if (resultTable != null) {
                return ParseResult(header, resultTable);
            }

            throw new InvalidOperationException("出馬表/結果のテーブルが見つかりませんでした。");
        }

        private RaceHeader ParseRaceHeader(HtmlDocument doc) {
            // titleから 日付/場/R を取る（あなたの2サンプルはどちらも取れました）
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//title");
            string title = string.Empty;
            if (titleNode != null) {
                title = (titleNode.InnerText ?? string.Empty).Trim();
            }

            Match m = TitleRegex.Match(title);
            if (!m.Success) {
                throw new InvalidOperationException("title から日付/場/R を取得できません: " + title);
            }

            int y = int.Parse(m.Groups["y"].Value, CultureInfo.InvariantCulture);
            int mo = int.Parse(m.Groups["m"].Value, CultureInfo.InvariantCulture);
            int d = int.Parse(m.Groups["d"].Value, CultureInfo.InvariantCulture);
            DateTime raceDate = new DateTime(y, mo, d);

            string trackName = m.Groups["track"].Value;
            int raceNo = int.Parse(m.Groups["rno"].Value, CultureInfo.InvariantCulture);

            // レース名
            HtmlNode raceNameNode = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'RaceName')]");
            string raceName = string.Empty;
            if (raceNameNode != null) {
                raceName = (raceNameNode.InnerText ?? string.Empty).Trim();
            }

            // RaceData01（芝/ダ/距離、天候/馬場が含まれる）
            HtmlNode data01Node = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'RaceData01')]");
            string data01 = string.Empty;
            if (data01Node != null) {
                data01 = HtmlEntity.DeEntitize(data01Node.InnerText ?? string.Empty).Trim();
            }

            // RaceData02（開催など）
            HtmlNode data02Node = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'RaceData02')]");
            string data02 = string.Empty;
            if (data02Node != null) {
                data02 = HtmlEntity.DeEntitize(data02Node.InnerText ?? string.Empty).Trim();
            }

            RaceHeader header = new RaceHeader();
            header.RaceDate = raceDate;
            header.RaceNo = raceNo;
            header.TrackName = trackName;
            header.RaceName = raceName;

            // surface/distance/turn/weather/going/meeting を最低限埋める（取れなければ空/0で保持）
            ApplyRaceData01(header, data01);
            ApplyRaceData02(header, data02);

            return header;
        }

        private void ApplyRaceData01(RaceHeader header, string data01) {
            // 例）"15:40発走 / 芝2500m (右 A) / 天候:晴 / 馬場:良"
            if (string.IsNullOrWhiteSpace(data01)) {
                return;
            }

            // 芝/ダ/障 + 距離
            Match md = Regex.Match(data01, @"(芝|ダート|ダ|障害|障)\s*(\d+)m");
            if (md.Success) {
                header.SurfaceType = md.Groups[1].Value;
                header.DistanceM = int.Parse(md.Groups[2].Value, CultureInfo.InvariantCulture);
            }

            // 回り（右/左）と内外（内/外）
            // 例: "(右 内)" "(左 外)" など
            Match mi = Regex.Match(data01, @"\((?<turn>右|左)\s*(?<inout>内|外)");
            if (mi.Success) {
                header.Turn = mi.Groups["turn"].Value;

                // 内外は存在するレースだけ（なければ空のまま）
                header.CourseInOut = mi.Groups["inout"].Value;
            }

            // 天候/馬場
            Match mw = Regex.Match(data01, @"天候\s*:\s*([^\s/]+)");
            if (mw.Success) {
                header.Weather = mw.Groups[1].Value;
            }

            Match mg = Regex.Match(data01, @"馬場\s*:\s*([^\s/]+)");
            if (mg.Success) {
                header.Going = mg.Groups[1].Value;
            }
        }

        private void ApplyRaceData02(RaceHeader header, string data02) {
            // 例）"5回 中山 8日目 サラ系..."
            if (string.IsNullOrWhiteSpace(data02)) {
                return;
            }

            Match mm = Regex.Match(data02, @"^\s*(\d+回\s*[^\s]+\s*\d+日目)");
            if (mm.Success) {
                header.Meeting = mm.Groups[1].Value.Trim();
            }
        }

        private List<IfRaceEntryRow> ParseShutuba(RaceHeader header, HtmlNode table) {
            // Shutuba_Table：枠/馬番が空の場合がある
            List<IfRaceEntryRow> list = new List<IfRaceEntryRow>();

            HtmlNodeCollection rows = table.SelectNodes(".//tr[td]");
            if (rows == null) {
                return list;
            }

            int provisionalNo = 101;

            foreach (HtmlNode tr in rows) {
                HtmlNodeCollection tds = tr.SelectNodes("./td");
                if (tds == null) {
                    continue;
                }

                // 想定列（サンプルから）
                // 1:枠 2:馬番 4:馬名 5:性齢 6:斤量 7:騎手
                string frameText = GetCellText(tds, 1);
                string horseNoText = GetCellText(tds, 2);

                int? frameNo = TryParseInt(frameText);
                int horseNo;
                int? parsedHorseNo = TryParseInt(horseNoText);
                if (parsedHorseNo.HasValue) {
                    horseNo = parsedHorseNo.Value;
                }
                else {
                    horseNo = provisionalNo;
                    provisionalNo++;
                }

                HtmlNode horseAnchor = GetHorseAnchor(tds, 4);
                if (horseAnchor == null) {
                    continue;
                }

                long horseIdNk = ParseHorseIdFromUrl(horseAnchor.GetAttributeValue("href", string.Empty));
                string horseName = (horseAnchor.InnerText ?? string.Empty).Trim();

                string sexAge = GetCellText(tds, 5);
                string sex = ParseSex(sexAge);
                int? age = ParseAge(sexAge);

                decimal? carriedWeight = TryParseDecimal(GetCellText(tds, 6));
                string jockey = GetCellText(tds, 7);

                IfRaceEntryRow row = CreateIfRowBase(header);
                row.FrameNo = frameNo;
                row.HorseNo = horseNo;
                row.HorseId = horseIdNk; // ※いまはnk由来のhorse_idを入れる（TR展開時にJVへ変換）
                row.HorseName = horseName;
                row.Sex = sex;
                row.Age = age;
                row.JockeyName = jockey;
                row.CarriedWeight = carriedWeight;

                list.Add(row);
            }

            return list;
        }

        private List<IfRaceEntryRow> ParseResult(RaceHeader header, HtmlNode table) {
            // ResultRefund：枠/馬番は確定値
            List<IfRaceEntryRow> list = new List<IfRaceEntryRow>();

            HtmlNodeCollection rows = table.SelectNodes(".//tr[td]");
            if (rows == null) {
                return list;
            }

            foreach (HtmlNode tr in rows) {
                HtmlNodeCollection tds = tr.SelectNodes("./td");
                if (tds == null) {
                    continue;
                }

                // 想定列（サンプルから）
                // 2:枠 3:馬番 4:馬名 5:性齢 6:斤量 7:騎手
                int? frameNo = TryParseInt(GetCellText(tds, 2));
                int horseNo = ParseRequiredInt(GetCellText(tds, 3), "horse_no");

                HtmlNode horseAnchor = GetHorseAnchor(tds, 4);
                if (horseAnchor == null) {
                    continue;
                }

                long horseIdNk = ParseHorseIdFromUrl(horseAnchor.GetAttributeValue("href", string.Empty));
                string horseName = (horseAnchor.InnerText ?? string.Empty).Trim();

                string sexAge = GetCellText(tds, 5);
                string sex = ParseSex(sexAge);
                int? age = ParseAge(sexAge);

                decimal? carriedWeight = TryParseDecimal(GetCellText(tds, 6));
                string jockey = GetCellText(tds, 7);

                IfRaceEntryRow row = CreateIfRowBase(header);
                row.FrameNo = frameNo;
                row.HorseNo = horseNo;
                row.HorseId = horseIdNk;
                row.HorseName = horseName;
                row.Sex = sex;
                row.Age = age;
                row.JockeyName = jockey;
                row.CarriedWeight = carriedWeight;

                list.Add(row);
            }

            return list;
        }

        private IfRaceEntryRow CreateIfRowBase(RaceHeader header) {
            IfRaceEntryRow row = new IfRaceEntryRow();
            row.RaceDate = header.RaceDate;
            row.RaceName = header.RaceName;
            row.RaceClass = header.RaceClass;
            row.ClassSimple = header.ClassSimple;
            row.TrackName = header.TrackName;
            row.SurfaceType = header.SurfaceType;
            row.DistanceM = header.DistanceM;
            row.Meeting = header.Meeting;
            row.Turn = header.Turn;
            row.CourseInOut = header.CourseInOut;
            row.Going = header.Going;
            row.Weather = header.Weather;
            row.ScrapedAt = DateTime.Now;
            return row;
        }

        private static string GetCellText(HtmlNodeCollection tds, int oneBasedIndex) {
            int idx = oneBasedIndex - 1;
            if (idx < 0 || idx >= tds.Count) {
                return string.Empty;
            }

            string text = HtmlEntity.DeEntitize(tds[idx].InnerText ?? string.Empty);
            return text.Trim();
        }

        private static HtmlNode GetHorseAnchor(HtmlNodeCollection tds, int oneBasedIndex) {
            int idx = oneBasedIndex - 1;
            if (idx < 0 || idx >= tds.Count) {
                return null;
            }

            HtmlNode a = tds[idx].SelectSingleNode(".//a");
            return a;
        }

        private static int? TryParseInt(string s) {
            if (string.IsNullOrWhiteSpace(s)) {
                return null;
            }

            int v;
            if (int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out v)) {
                return v;
            }

            return null;
        }

        private static decimal? TryParseDecimal(string s) {
            if (string.IsNullOrWhiteSpace(s)) {
                return null;
            }

            decimal v;
            if (decimal.TryParse(s.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out v)) {
                return v;
            }

            return null;
        }

        private static int ParseRequiredInt(string s, string fieldName) {
            int v;
            if (!int.TryParse((s ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out v)) {
                throw new InvalidOperationException(fieldName + " is not int: " + s);
            }

            return v;
        }

        private static long ParseHorseIdFromUrl(string url) {
            // 例: https://db.netkeiba.com/horse/2021105369
            Match m = Regex.Match(url ?? string.Empty, @"/horse/(\d+)");
            if (!m.Success) {
                throw new InvalidOperationException("horse_id をURLから取得できません: " + url);
            }

            return long.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        private static string ParseSex(string sexAge) {
            if (string.IsNullOrWhiteSpace(sexAge)) {
                return string.Empty;
            }

            return sexAge.Trim().Substring(0, 1);
        }

        private static int? ParseAge(string sexAge) {
            if (string.IsNullOrWhiteSpace(sexAge)) {
                return null;
            }

            string s = sexAge.Trim();
            if (s.Length <= 1) {
                return null;
            }

            string agePart = s.Substring(1);

            int age;
            if (int.TryParse(agePart, NumberStyles.Integer, CultureInfo.InvariantCulture, out age)) {
                return age;
            }

            return null;
        }

        private sealed class RaceHeader {
            public DateTime RaceDate;
            public int RaceNo;
            public string TrackName;
            public string RaceName;

            public string RaceClass;
            public string ClassSimple;

            public string SurfaceType;
            public int DistanceM;
            public string Turn;
            public string CourseInOut;

            public string Meeting;
            public string Going;
            public string Weather;

            public RaceHeader() {
                this.TrackName = string.Empty;
                this.RaceName = string.Empty;

                this.RaceClass = string.Empty;
                this.ClassSimple = string.Empty;

                this.SurfaceType = string.Empty;
                this.Turn = string.Empty;
                this.CourseInOut = string.Empty;

                this.Meeting = string.Empty;
                this.Going = string.Empty;
                this.Weather = string.Empty;
            }
        }
    }
}
