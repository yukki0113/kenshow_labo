using System;

namespace KenshowLabo.ScrapingRunner {
    /// <summary>
    /// settings.json の内容を表す設定モデルです。
    /// </summary>
    public sealed class AppSettings {
        /// <summary>1リクエストごとの待機秒数です。</summary>
        public double PerRequestSec { get; set; } = 0.5;

        /// <summary>バッチ単位の件数です。</summary>
        public int BatchSize { get; set; } = 100;

        /// <summary>バッチ間の休憩分数です。</summary>
        public int BatchWaitMinutes { get; set; } = 30;

        /// <summary>WIN5の処理をスキップします。</summary>
        public bool SkipWin5 { get; set; } = false;

        /// <summary>血統の処理をスキップします。</summary>
        public bool SkipPedigree { get; set; } = false;

        /// <summary>払戻の処理をスキップします。</summary>
        public bool SkipPayout { get; set; } = false;

        /// <summary>血統バッチの最大処理件数です。</summary>
        public int PedigreeMaxCount { get; set; } = 100;

        /// <summary>払戻バッチの最大処理件数です。</summary>
        public int PayoutMaxCount { get; set; } = 100;
    }
}
