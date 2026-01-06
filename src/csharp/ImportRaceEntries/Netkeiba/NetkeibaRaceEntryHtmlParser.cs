using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ImportRaceEntries.Netkeiba.Models;

namespace ImportRaceEntries.Netkeiba {
    public sealed class NetkeibaRaceEntryHtmlParser {
        private static readonly Regex RaceIdRegex = new Regex(@"/race/(\d{12})", RegexOptions.Compiled);
        private static readonly Regex NkHorseIdRegex = new Regex(@"/horse/(\d+)", RegexOptions.Compiled);

        /// <summary>
        /// netkeiba出馬表HTMLを解析してIF投入用のrawデータを生成します。
        /// 結果表（ResultRefund）等の場合は例外ではなくfalse返却でスキップ可能にします。
        /// </summary>
        public bool TryParse(string html, Guid importBatchId, DateTime scrapedAt, string? sourceFile, out NetkeibaRaceHeaderRaw header, out List<NetkeibaRaceEntryRowRaw> rows) {
            header = new NetkeibaRaceHeaderRaw();
            rows = new List<NetkeibaRaceEntryRowRaw>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            // ==============================
            // 1) レースIDの抽出
            // ==============================
            string? raceId = ExtractRaceId(doc);
            if (string.IsNullOrWhiteSpace(raceId)) {
                return false;
            }

            // ==============================
            // 2) 結果表（回顧ページ）除外
            // ==============================
            // RaceTable01 は出馬表も結果表も共通なので、classで判定してスキップ
            HtmlNode? table = doc.DocumentNode.SelectSingleNode("//table[contains(@class,'RaceTable01')]");
            if (table == null) {
                return false;
            }

            string tableClass = table.GetAttributeValue("class", string.Empty);
            if (tableClass.IndexOf("ResultRefund", StringComparison.OrdinalIgnoreCase) >= 0) {
                return false;
            }

            // ==============================
            // 3) ヘッダraw抽出（まずは文字列でOK）
            // ==============================
            string? raceName = InnerTextOrNull(doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'RaceName')]"));
            string? raceData01 = InnerTextOrNull(doc.DocumentNode.SelectSingleNode("//p[contains(@class,'RaceData01')]"));

            // RaceData01 から「日付/場名/R/芝ダ距離」等が取れることが多いので、rawとして保持
            // 例: "12月28日(日) 中山11R 芝2500m"
            string? raceDateRaw = null;
            string? trackNameRaw = null;
            string? raceNoRaw = null;
            string? surfaceDistanceRaw = null;

            if (!string.IsNullOrWhiteSpace(raceData01)) {
                // かなり揺れるので、ここでは過剰にパースせず「取れたら取る」方針
                // 1) raceNo（"11R"）抽出
                Match mNo = Regex.Match(raceData01, @"(\d{1,2}\s*[RＲ])");
                if (mNo.Success) {
                    raceNoRaw = mNo.Groups[1].Value.Replace(" ", string.Empty);
                }

                // 2) 距離（芝/ダ + 数字 + m）抽出
                Match mDist = Regex.Match(raceData01, @"(芝|ダ|ダート|障)\s*(\d{3,4})\s*m", RegexOptions.IgnoreCase);
                if (mDist.Success) {
                    string surface = mDist.Groups[1].Value;
                    string dist = mDist.Groups[2].Value;
                    surfaceDistanceRaw = surface + dist + "m";
                }
                else {
                    // "芝2500m" のような密結合も拾う
                    Match mDist2 = Regex.Match(raceData01, @"(芝|ダ)(\d{3,4})m", RegexOptions.IgnoreCase);
                    if (mDist2.Success) {
                        surfaceDistanceRaw = mDist2.Groups[1].Value + mDist2.Groups[2].Value + "m";
                    }
                }

                // 3) trackNameは「Rの手前の漢字」などで雑に拾えるが誤検知しやすいので、
                //    現段階はraw優先：C#で厳密化しない
                //    ただし、スペース区切りがあるケースは拾う
                string[] tokens = raceData01.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length >= 2) {
                    raceDateRaw = tokens[0];
                    trackNameRaw = tokens[1];
                }
                else {
                    raceDateRaw = raceData01;
                }
            }

            header = new NetkeibaRaceHeaderRaw {
                ImportBatchId = importBatchId,
                RaceId = raceId,
                RaceNoRaw = raceNoRaw,
                RaceDateRaw = raceDateRaw,
                RaceNameRaw = raceName,
                TrackNameRaw = trackNameRaw,
                SurfaceDistanceRaw = surfaceDistanceRaw,
                ScrapedAt = scrapedAt,
                SourceUrl = ExtractCanonicalUrl(doc),
                SourceFile = sourceFile
            };

            // ==============================
            // 4) 明細行の列Indexをヘッダ<th>から構成
            // ==============================
            Dictionary<string, int> colIndex = BuildColumnIndex(table);

            // 期待列（netkeiba表記の揺れがあるので候補を複数）
            int idxWaku = GetIndex(colIndex, new[] { "枠", "枠番" });
            int idxUma = GetIndex(colIndex, new[] { "馬", "馬番" });
            int idxHorse = GetIndex(colIndex, new[] { "馬名" });
            int idxSexAge = GetIndex(colIndex, new[] { "性齢", "性令" });
            int idxJockey = GetIndex(colIndex, new[] { "騎手" });
            int idxWeight = GetIndex(colIndex, new[] { "斤量" });

            // <tbody>の<tr>を取得（ヘッダ行は除外）
            HtmlNodeCollection? trNodes = table.SelectNodes(".//tr[td]");
            if (trNodes == null || trNodes.Count == 0) {
                return false;
            }

            int rowNo = 0;

            foreach (HtmlNode tr in trNodes) {
                HtmlNodeCollection? tds = tr.SelectNodes("./td");
                if (tds == null) {
                    continue;
                }

                // 馬名列が取れない行は捨てる（ヘッダっぽい行や空行対策）
                string? horseNameCell = GetCellText(tds, idxHorse);
                if (string.IsNullOrWhiteSpace(horseNameCell)) {
                    continue;
                }

                rowNo++;

                string? nkHorseId = ExtractNkHorseIdFromRow(tr);

                NetkeibaRaceEntryRowRaw row = new NetkeibaRaceEntryRowRaw {
                    ImportBatchId = importBatchId,
                    RowNo = rowNo,
                    NkHorseId = nkHorseId,
                    JvHorseId = null, // 後段で解決
                    FrameNoRaw = GetCellText(tds, idxWaku),
                    HorseNoRaw = GetCellText(tds, idxUma),
                    HorseNameRaw = NormalizeSpace(horseNameCell),
                    SexAgeRaw = GetCellText(tds, idxSexAge),
                    JockeyNameRaw = GetCellText(tds, idxJockey),
                    CarriedWeightRaw = GetCellText(tds, idxWeight)
                };

                rows.Add(row);
            }

            // 最低限：馬行が1件も取れないならスキップ
            if (rows.Count == 0) {
                return false;
            }

            return true;
        }

        private static string? ExtractRaceId(HtmlDocument doc) {
            // canonical から優先
            string? canonical = ExtractCanonicalUrl(doc);
            if (!string.IsNullOrWhiteSpace(canonical)) {
                Match m = RaceIdRegex.Match(canonical);
                if (m.Success) {
                    return m.Groups[1].Value;
                }
            }

            // ページ内の /race/12桁 を探索
            string html = doc.DocumentNode.OuterHtml;
            Match m2 = RaceIdRegex.Match(html);
            if (m2.Success) {
                return m2.Groups[1].Value;
            }

            return null;
        }

        private static string? ExtractCanonicalUrl(HtmlDocument doc) {
            HtmlNode? node = doc.DocumentNode.SelectSingleNode("//link[@rel='canonical']");
            if (node == null) {
                return null;
            }
            string href = node.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrWhiteSpace(href)) {
                return null;
            }
            return href;
        }

        private static Dictionary<string, int> BuildColumnIndex(HtmlNode table) {
            Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // thテキストを順番に取得（複数ヘッダ行がある場合もあるので最初の thead を優先）
            HtmlNodeCollection? ths = table.SelectNodes(".//thead//th");
            if (ths == null) {
                ths = table.SelectNodes(".//tr/th");
            }

            if (ths == null) {
                return map;
            }

            int i = 0;
            foreach (HtmlNode th in ths) {
                string key = NormalizeSpace(th.InnerText ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(key)) {
                    if (!map.ContainsKey(key)) {
                        map.Add(key, i);
                    }
                }
                i++;
            }

            return map;
        }

        private static int GetIndex(Dictionary<string, int> map, string[] candidates) {
            foreach (string c in candidates) {
                int idx;
                if (map.TryGetValue(c, out idx)) {
                    return idx;
                }
            }
            return -1;
        }

        private static string? GetCellText(HtmlNodeCollection tds, int idx) {
            if (idx < 0) {
                return null;
            }
            if (idx >= tds.Count) {
                return null;
            }

            string text = NormalizeSpace(tds[idx].InnerText ?? string.Empty);
            if (string.IsNullOrWhiteSpace(text)) {
                return null;
            }
            return text;
        }

        private static string? ExtractNkHorseIdFromRow(HtmlNode tr) {
            HtmlNode? a = tr.SelectSingleNode(".//a[contains(@href,'/horse/')]");
            if (a == null) {
                return null;
            }

            string href = a.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrWhiteSpace(href)) {
                return null;
            }

            Match m = NkHorseIdRegex.Match(href);
            if (!m.Success) {
                return null;
            }

            // 10桁0埋めに寄せる（DB側がCHAR(10)のため）
            string digits = m.Groups[1].Value;
            string padded = digits.PadLeft(10, '0');
            return padded;
        }

        private static string? InnerTextOrNull(HtmlNode? node) {
            if (node == null) {
                return null;
            }
            string text = NormalizeSpace(node.InnerText ?? string.Empty);
            if (string.IsNullOrWhiteSpace(text)) {
                return null;
            }
            return text;
        }

        private static string NormalizeSpace(string s) {
            if (s == null) {
                return string.Empty;
            }
            string t = s.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
            t = Regex.Replace(t, @"\s+", " ").Trim();
            return t;
        }
    }
}
