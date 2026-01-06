using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using ImportRaceEntries.Netkeiba;
using ImportRaceEntries.Netkeiba.Models;

namespace ImportRaceEntries {
    internal static class Program {
        public static int Main(string[] args) {
            // ==============================
            // 1) 引数（最小）
            // args[0] = HTMLフォルダ
            // args[1] = SQL接続文字列
            // ==============================
            if (args.Length < 2) {
                Console.WriteLine("Usage: ImportRaceEntries <htmlDir> <connectionString>");
                return 1;
            }

            string htmlDir = args[0];
            string connectionString = args[1];

            if (!Directory.Exists(htmlDir)) {
                Console.WriteLine("Directory not found: " + htmlDir);
                return 1;
            }

            NetkeibaRaceEntryHtmlParser parser = new NetkeibaRaceEntryHtmlParser();
            HorseIdResolver resolver = new HorseIdResolver(connectionString);
            IfNetkeibaRepository repo = new IfNetkeibaRepository(connectionString);

            // jv_horse_id 解決（未解決は警告ログで継続）
            foreach (NetkeibaRaceEntryRowRaw r in rows) {
                string? jv = resolver.ResolveJvHorseId(r.NkHorseId, r.HorseNameRaw);
                if (string.IsNullOrWhiteSpace(jv)) {
                    Console.WriteLine("[WARN] jv_horse_id unresolved: race_id=" + header.RaceId
                                      + " nk_horse_id=" + (r.NkHorseId ?? "null")
                                      + " horse_name=" + (r.HorseNameRaw ?? "null"));
                }
                else {
                    r.JvHorseId = jv;
                }
            }

            // IF投入（Header+Rows）
            repo.InsertIf(header, rows);

            // 必要なら IF→TR 展開（未解決行が残るとDB側でSKIP）
            repo.ApplyToTr(header.ImportBatchId);
            
            Console.WriteLine("[OK] inserted IF: race_id=" + header.RaceId
                                      + " rows=" + rows.Count
                                      + " file=" + Path.GetFileName(file));
                }
                catch (Exception ex) {
                    Console.WriteLine("[ERROR] file=" + Path.GetFileName(file));
                    Console.WriteLine(ex.ToString());
                }
            }

            return 0;
        }
    }
}
