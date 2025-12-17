using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KenshowLabo.ScrapingRunner {
    /// <summary>
    /// run_all.py のプロセス実行・出力受信・停止制御を担当します。
    /// </summary>
    public sealed class RunAllProcessRunner : IDisposable {
        private Process? _process;
        private CancellationTokenSource? _ctsRead;

        private readonly object _lockObj;

        /// <summary>
        /// stdout の行を受け取るイベントです。
        /// </summary>
        public event Action<string>? StdOutLineReceived;

        /// <summary>
        /// stderr の行を受け取るイベントです。
        /// </summary>
        public event Action<string>? StdErrLineReceived;

        /// <summary>
        /// プロセス終了時のイベントです（exitCode を渡します）。
        /// </summary>
        public event Action<int>? Exited;

        /// <summary>
        /// 実行中かどうかです。
        /// </summary>
        public bool IsRunning {
            get {
                lock (_lockObj) {
                    if (_process == null) {
                        return false;
                    }

                    return !_process.HasExited;
                }
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public RunAllProcessRunner() {
            _lockObj = new object();
        }

        /// <summary>
        /// run_all.py を起動します。
        /// </summary>
        public void Start(string pythonExePath, string runAllPyPath, string arguments, string workingDirectory) {
            lock (_lockObj) {
                // 二重起動防止
                if (_process != null && !_process.HasExited) {
                    throw new InvalidOperationException("既に実行中です。");
                }

                // ProcessStartInfo 構築
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = pythonExePath;
                psi.Arguments = QuoteIfNeeded(runAllPyPath) + " " + arguments;
                psi.WorkingDirectory = workingDirectory;

                // 標準出力/標準エラーを受け取る
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.RedirectStandardInput = false;

                // コンソールウィンドウは出さない（必要なら false に）
                psi.CreateNoWindow = true;

                Process proc = new Process();
                proc.StartInfo = psi;
                proc.EnableRaisingEvents = true;
                proc.Exited += OnProcessExited;

                // 起動
                proc.Start();

                _process = proc;

                // 出力読み取り開始
                _ctsRead = new CancellationTokenSource();
                CancellationToken token = _ctsRead.Token;

                Task.Run(() => ReadStdOutLoop(proc, token), token);
                Task.Run(() => ReadStdErrLoop(proc, token), token);
            }
        }

        /// <summary>
        /// 二段階停止を行います。
        /// </summary>
        public async Task StopAsync(TimeSpan softWait, TimeSpan hardWait) {
            Process? proc = null;

            lock (_lockObj) {
                proc = _process;
            }

            if (proc == null) {
                return;
            }

            if (proc.HasExited) {
                return;
            }

            // -------------------------
            // 1) ソフト停止（効く場合のみ）
            // -------------------------
            try {
                // Python はメインウィンドウを持たないことが多く、ここが効かないケースが多いです。
                // ただし、将来コンソール表示で起動した場合などは CloseMainWindow が有効になる場合があります。
                proc.CloseMainWindow();
            }
            catch {
                // 失敗しても次段へ
            }

            // ソフト停止待機
            bool softStopped = await WaitForExitAsync(proc, softWait).ConfigureAwait(false);
            if (softStopped) {
                return;
            }

            // -------------------------
            // 2) 強制停止（プロセスツリー）
            // -------------------------
            try {
                proc.Kill(true);
            }
            catch {
                // 既に死んでいる等は無視
            }

            // 強制停止の完了待ち（念のため）
            await WaitForExitAsync(proc, hardWait).ConfigureAwait(false);
        }

        /// <summary>
        /// 破棄処理です。
        /// </summary>
        public void Dispose() {
            lock (_lockObj) {
                if (_ctsRead != null) {
                    _ctsRead.Cancel();
                    _ctsRead.Dispose();
                    _ctsRead = null;
                }

                if (_process != null) {
                    try {
                        if (!_process.HasExited) {
                            _process.Kill(true);
                        }
                    }
                    catch {
                        // 無視
                    }

                    _process.Dispose();
                    _process = null;
                }
            }
        }

        /// <summary>
        /// stdout を行単位で読み取ります。
        /// </summary>
        private void ReadStdOutLoop(Process proc, CancellationToken token) {
            try {
                while (!token.IsCancellationRequested && !proc.HasExited) {
                    string? line = proc.StandardOutput.ReadLine();
                    if (line == null) {
                        break;
                    }

                    StdOutLineReceived?.Invoke(line);
                }
            }
            catch {
                // 読み取り例外は無視（停止時に発生しがち）
            }
        }

        /// <summary>
        /// stderr を行単位で読み取ります。
        /// </summary>
        private void ReadStdErrLoop(Process proc, CancellationToken token) {
            try {
                while (!token.IsCancellationRequested && !proc.HasExited) {
                    string? line = proc.StandardError.ReadLine();
                    if (line == null) {
                        break;
                    }

                    StdErrLineReceived?.Invoke(line);
                }
            }
            catch {
                // 読み取り例外は無視
            }
        }

        /// <summary>
        /// プロセス終了イベントです。
        /// </summary>
        private void OnProcessExited(object? sender, EventArgs e) {
            int code = 0;

            lock (_lockObj) {
                if (_process != null) {
                    try {
                        code = _process.ExitCode;
                    }
                    catch {
                        code = -1;
                    }
                }

                if (_ctsRead != null) {
                    _ctsRead.Cancel();
                }
            }

            Exited?.Invoke(code);
        }

        /// <summary>
        /// 指定時間待機してプロセス終了を待ちます。
        /// </summary>
        private static async Task<bool> WaitForExitAsync(Process proc, TimeSpan timeout) {
            DateTime start = DateTime.Now;

            while (!proc.HasExited) {
                await Task.Delay(200).ConfigureAwait(false);

                TimeSpan elapsed = DateTime.Now - start;
                if (elapsed > timeout) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 空白を含むパスをクォートします。
        /// </summary>
        private static string QuoteIfNeeded(string path) {
            if (path.Contains(" ")) {
                return "\"" + path + "\"";
            }

            return path;
        }
    }
}
