using KenshowLabo.ScrapingRunner;
using System.Globalization;

namespace ScrapingRunner {
    public partial class MainForm : Form {

        private readonly SettingsRepository _settingsRepository;
        private AppSettings _settings;

        private readonly RunAllProcessRunner _runner;
        private readonly ErrorLogWriter _errorLogWriter;

        private readonly Queue<string> _uiLines;
        private readonly int _uiLineCapacity;

        public MainForm() {
            InitializeComponent();
            // -------------------------
            // 基本初期化
            // -------------------------
            string baseDir = AppContext.BaseDirectory;

            string settingsPath = Path.Combine(baseDir, "AppData", "settings.json");
            _settingsRepository = new SettingsRepository(settingsPath);
            _settings = _settingsRepository.LoadOrCreate();

            _runner = new RunAllProcessRunner();
            _runner.StdOutLineReceived += OnStdOutLine;
            _runner.StdErrLineReceived += OnStdErrLine;
            _runner.Exited += OnProcessExited;

            _errorLogWriter = new ErrorLogWriter(baseDir, 200);

            _uiLineCapacity = 400;
            _uiLines = new Queue<string>();

            // -------------------------
            // 画面初期値
            // -------------------------
            InitializeDefaultInputs();
            UpdateUiState(isRunning: false);
        }

        private void InitializeDefaultInputs() {
            // 年のデフォルト：To=今年、From=今年-5
            int year = DateTime.Now.Year;

            // NumericUpDown へ反映
            nudYearTo.Minimum = 1960;
            nudYearFrom.Maximum = DateTime.Now.Year;
            nudYearTo.Minimum = 1960;
            nudYearFrom.Maximum = DateTime.Now.Year;
            nudYearTo.Value = ClampYear(year);
            nudYearFrom.Value = ClampYear(year - 5);

            // 場コードは全ON
            SetAllPlaces(checkedValue: true);

            // ボタン表示
            btnRunStop.Text = "実行";

            // ステータス初期化
            SetStatusText("待機中");

            rtbOutput.Clear();
        }

        /// <summary>
        /// 年の範囲を NumericUpDown の範囲に丸めます。
        /// </summary>
        private decimal ClampYear(int year) {
            if (year < 1960) {
                return 1960;
            }

            if (year >= DateTime.Now.Year) {
                return DateTime.Now.Year;
            }

            return year;
        }

        private void btnAll_Click(object sender, EventArgs e) {
            // 全ONなら全OFF、そうでなければ全ON
            bool allOn = AreAllPlacesChecked();
            SetAllPlaces(checkedValue: !allOn);
        }

        private async void btnRunStop_Click(object sender, EventArgs e) {
            // 実行中：停止
            if (_runner.IsRunning) {
                DialogResult dr = MessageBox.Show(
                    "停止します。よろしいですか。",
                    "確認",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (dr != DialogResult.Yes) {
                    return;
                }

                UpdateUiState(isRunning: true); // 停止中も入力は無効のまま

                // 二段階停止（ソフト→強制）
                await _runner.StopAsync(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(10));

                return;
            }

            // 実行開始
            int yearFrom = Decimal.ToInt32(nudYearFrom.Value);
            int yearTo = Decimal.ToInt32(nudYearTo.Value);

            if (yearFrom > yearTo) {
                MessageBox.Show("年の範囲が不正です（From <= To）。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (yearFrom > yearTo) {
                MessageBox.Show("年の範囲が不正です（From <= To）。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // places
            List<string> places = GetSelectedPlaces();
            if (places.Count == 0) {
                MessageBox.Show("開催場は最低1つ選択してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // run_all.py のパス解決
            string? repoRoot = TryResolveRepoRoot();
            string? runAllPath = TryResolveRunAllPath(repoRoot);

            if (String.IsNullOrEmpty(runAllPath) || !File.Exists(runAllPath)) {
                MessageBox.Show("run_all.py が見つかりませんでした。実行環境（配置/パス）を確認してください。", "起動エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // python.exe は一旦 PATH 前提
            string pythonExe = "python";

            // arguments 組み立て
            string placesCsv = String.Join(",", places);

            string args =
                "--year-from " + yearFrom.ToString(CultureInfo.InvariantCulture) + " " +
                "--year-to " + yearTo.ToString(CultureInfo.InvariantCulture) + " " +
                "--places " + placesCsv + " " +
                "--per-request-sec " + _settings.PerRequestSec.ToString(CultureInfo.InvariantCulture) + " " +
                "--batch-size " + _settings.BatchSize.ToString(CultureInfo.InvariantCulture) + " " +
                "--batch-wait-minutes " + _settings.BatchWaitMinutes.ToString(CultureInfo.InvariantCulture) + " " +
                "--pedigree-max-count " + _settings.PedigreeMaxCount.ToString(CultureInfo.InvariantCulture) + " " +
                "--payout-max-count " + _settings.PayoutMaxCount.ToString(CultureInfo.InvariantCulture);

            if (_settings.SkipWin5) {
                args += " --skip-win5";
            }
            if (_settings.SkipPedigree) {
                args += " --skip-pedigree";
            }
            if (_settings.SkipPayout) {
                args += " --skip-payout";
            }

            // UIを初期化して実行
            rtbOutput.Clear();
            _uiLines.Clear();

            UpdateUiState(isRunning: true);
            AppendUiLine("[INFO] Start: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AppendUiLine("[INFO] " + pythonExe + " " + runAllPath + " " + args);

            // 起動（WorkingDirectoryは repo root 推奨）
            string workDir = repoRoot ?? Path.GetDirectoryName(runAllPath)!;
            _runner.Start(pythonExe, runAllPath, args, workDir);
        }

        private void btnSettings_Click(object sender, EventArgs e) {
            if (_runner.IsRunning) {
                return;
            }

            SettingsForm form = new SettingsForm(_settings);
            DialogResult dr = form.ShowDialog(this);

            if (dr == DialogResult.OK) {
                // 保存された設定を反映
                _settings = form.ResultSettings;
                _settingsRepository.Save(_settings);

                AppendUiLine("[INFO] settings.json を保存しました。");
            }
        }

        private void OnStdOutLine(string line) {
            // UIスレッドへマーシャリング
            if (InvokeRequired) {
                BeginInvoke(new Action<string>(OnStdOutLine), line);
                return;
            }

            // INFOとしてリングに積む（画面表示もする）
            _errorLogWriter.AddInfo(line);

            AppendUiLine(line);

            // WARN/ERRORっぽい行はファイルへ
            string level = DetectLevel(line);
            if (level == "WARN" || level == "ERROR") {
                _errorLogWriter.WriteWarnOrError(level, line);
            }
        }

        private void OnStdErrLine(string line) {
            // UIスレッドへマーシャリング
            if (InvokeRequired) {
                BeginInvoke(new Action<string>(OnStdErrLine), line);
                return;
            }

            // stderr は基本WARN/ERROR扱いでファイルへ
            AppendUiLine("[STDERR] " + line);
            _errorLogWriter.WriteWarnOrError("ERROR", line);
        }

        private void OnProcessExited(int exitCode) {
            // UIスレッドへ
            if (InvokeRequired) {
                BeginInvoke(new Action<int>(OnProcessExited), exitCode);
                return;
            }

            AppendUiLine("[INFO] Exited. code=" + exitCode.ToString(CultureInfo.InvariantCulture));

            UpdateUiState(isRunning: false);

            // 終了ステータス表示
            SetStatusText("終了 code=" + exitCode.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        private void UpdateUiState(bool isRunning) {
            // 入力類の有効/無効
            nudYearFrom.Enabled = !isRunning;
            nudYearTo.Enabled = !isRunning;

            SetPlaceControlsEnabled(!isRunning);

            btnAll.Enabled = !isRunning;
            btnSettings.Enabled = !isRunning;

            // 実行/停止ボタン
            btnRunStop.Text = isRunning ? "停止" : "実行";

            // ステータス表示
            if (isRunning) {
                SetStatusText("実行中...");
            }
            else {
                SetStatusText("待機中...");
            }
        }

        private void AppendUiLine(string line) {
            // UI表示は「最新中心」で溜めすぎない（リング）
            _uiLines.Enqueue(line);
            while (_uiLines.Count > _uiLineCapacity) {
                _uiLines.Dequeue();
            }

            rtbOutput.Lines = _uiLines.ToArray();
            rtbOutput.SelectionStart = rtbOutput.TextLength;
            rtbOutput.ScrollToCaret();
        }

        private bool AreAllPlacesChecked() {
            List<CheckBox> boxes = GetPlaceCheckBoxes();
            foreach (CheckBox cb in boxes) {
                if (!cb.Checked) {
                    return false;
                }
            }

            return true;
        }

        private void SetAllPlaces(bool checkedValue) {
            List<CheckBox> boxes = GetPlaceCheckBoxes();
            foreach (CheckBox cb in boxes) {
                cb.Checked = checkedValue;
            }
        }

        private void SetPlaceControlsEnabled(bool enabled) {
            List<CheckBox> boxes = GetPlaceCheckBoxes();
            foreach (CheckBox cb in boxes) {
                cb.Enabled = enabled;
            }
        }

        private List<string> GetSelectedPlaces() {
            List<string> places = new List<string>();

            // チェック状態から "01"..."10" を集める
            Dictionary<CheckBox, string> map = new Dictionary<CheckBox, string>();
            map.Add(chkPlace01, "01");
            map.Add(chkPlace02, "02");
            map.Add(chkPlace03, "03");
            map.Add(chkPlace04, "04");
            map.Add(chkPlace05, "05");
            map.Add(chkPlace06, "06");
            map.Add(chkPlace07, "07");
            map.Add(chkPlace08, "08");
            map.Add(chkPlace09, "09");
            map.Add(chkPlace10, "10");

            foreach (KeyValuePair<CheckBox, string> kv in map) {
                if (kv.Key.Checked) {
                    places.Add(kv.Value);
                }
            }

            return places;
        }

        private List<CheckBox> GetPlaceCheckBoxes() {
            List<CheckBox> list = new List<CheckBox>();
            list.Add(chkPlace01);
            list.Add(chkPlace02);
            list.Add(chkPlace03);
            list.Add(chkPlace04);
            list.Add(chkPlace05);
            list.Add(chkPlace06);
            list.Add(chkPlace07);
            list.Add(chkPlace08);
            list.Add(chkPlace09);
            list.Add(chkPlace10);
            return list;
        }

        private string DetectLevel(string line) {
            // 雑に "warn"/"error" を含むかで判定（必要なら後で厳密化）
            string s = line.ToUpperInvariant();

            if (s.Contains("ERROR")) {
                return "ERROR";
            }
            if (s.Contains("WARN")) {
                return "WARN";
            }

            return "INFO";
        }

        private string? TryResolveRepoRoot() {
            // 環境変数で指定できるようにする（任意）
            string? env = Environment.GetEnvironmentVariable("KENSHOWLABO_REPO_ROOT");
            if (!String.IsNullOrEmpty(env) && Directory.Exists(env)) {
                return env;
            }

            // AppContext.BaseDirectory から上へ辿って "src" を含む構成を探す
            string current = AppContext.BaseDirectory;

            for (int i = 0; i < 10; i++) {
                string srcPath = Path.Combine(current, "src");
                if (Directory.Exists(srcPath)) {
                    return current;
                }

                DirectoryInfo? parent = Directory.GetParent(current);
                if (parent == null) {
                    break;
                }

                current = parent.FullName;
            }

            return null;
        }

        private string? TryResolveRunAllPath(string? repoRoot) {
            // repoRoot が取れたらそこから相対
            if (!String.IsNullOrEmpty(repoRoot)) {
                string p = Path.Combine(repoRoot, "src", "scraping", "run_all.py");
                return p;
            }

            // 取れなければカレントから
            string fallback = Path.Combine("src", "scraping", "run_all.py");
            return fallback;
        }

        /// <summary>
        /// ステータスバーへ状態文字列を表示します。
        /// </summary>
        private void SetStatusText(string text) {
            // コントロール未配置の事故対策
            if (tsslStatus == null) {
                return;
            }

            tsslStatus.Text = text;
        }

        private void pYear_Paint(object sender, PaintEventArgs e) {

        }
    }
}