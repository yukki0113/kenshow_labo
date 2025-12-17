using KenshowLabo.ScrapingRunner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScrapingRunner {
    public partial class SettingsForm : Form {
        private readonly AppSettings _original;

        /// <summary>
        /// 保存結果の設定です。DialogResult.OK のときのみ有効です。
        /// </summary>
        public AppSettings ResultSettings { get; private set; }

        public SettingsForm(AppSettings current) {
            InitializeComponent();

            _original = current;

            // 初期表示
            txtPerRequestSec.Text = current.PerRequestSec.ToString(CultureInfo.InvariantCulture);
            txtBatchSize.Text = current.BatchSize.ToString(CultureInfo.InvariantCulture);
            txtBatchWaitMinutes.Text = current.BatchWaitMinutes.ToString(CultureInfo.InvariantCulture);

            chkSkipWin5.Checked = current.SkipWin5;
            chkSkipPedigree.Checked = current.SkipPedigree;
            chkSkipPayout.Checked = current.SkipPayout;

            txtPedigreeMaxCount.Text = current.PedigreeMaxCount.ToString(CultureInfo.InvariantCulture);
            txtPayoutMaxCount.Text = current.PayoutMaxCount.ToString(CultureInfo.InvariantCulture);

            // 初期値
            ResultSettings = current;
        }

        private void btnSave_Click(object sender, EventArgs e) {
            // -------------------------
            // バリデーション
            // -------------------------
            double perRequestSec = 0.0;
            int batchSize = 0;
            int batchWaitMinutes = 0;
            int pedigreeMaxCount = 0;
            int payoutMaxCount = 0;

            if (!Double.TryParse(txtPerRequestSec.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out perRequestSec)) {
                MessageBox.Show("per_request_sec は数値で入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (perRequestSec < 0.0 || perRequestSec > 60.0) {
                MessageBox.Show("per_request_sec は 0.0～60.0 の範囲で入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Int32.TryParse(txtBatchSize.Text, out batchSize)) {
                MessageBox.Show("batch_size は整数で入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (batchSize < 1 || batchSize > 100000) {
                MessageBox.Show("batch_size は 1～100000 の範囲で入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Int32.TryParse(txtBatchWaitMinutes.Text, out batchWaitMinutes)) {
                MessageBox.Show("batch_wait_minutes は整数で入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (batchWaitMinutes < 0 || batchWaitMinutes > 1440) {
                MessageBox.Show("batch_wait_minutes は 0～1440 の範囲で入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Int32.TryParse(txtPedigreeMaxCount.Text, out pedigreeMaxCount)) {
                MessageBox.Show("pedigree_max_count は整数で入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (pedigreeMaxCount < 0 || pedigreeMaxCount > 1000000) {
                MessageBox.Show("pedigree_max_count は 0～1000000 の範囲で入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Int32.TryParse(txtPayoutMaxCount.Text, out payoutMaxCount)) {
                MessageBox.Show("payout_max_count は整数で入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (payoutMaxCount < 0 || payoutMaxCount > 1000000) {
                MessageBox.Show("payout_max_count は 0～1000000 の範囲で入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // -------------------------
            // 反映（新しいインスタンスを生成）
            // -------------------------
            AppSettings next = new AppSettings();
            next.PerRequestSec = perRequestSec;
            next.BatchSize = batchSize;
            next.BatchWaitMinutes = batchWaitMinutes;

            next.SkipWin5 = chkSkipWin5.Checked;
            next.SkipPedigree = chkSkipPedigree.Checked;
            next.SkipPayout = chkSkipPayout.Checked;

            next.PedigreeMaxCount = pedigreeMaxCount;
            next.PayoutMaxCount = payoutMaxCount;

            ResultSettings = next;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
