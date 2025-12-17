namespace ScrapingRunner
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            rootTable = new TableLayoutPanel();
            pSettings = new Panel();
            tblSettingsRoot = new TableLayoutPanel();
            lblSettingsTitle = new Label();
            tblSettings = new TableLayoutPanel();
            lblPerRequestSec = new Label();
            txtPerRequestSec = new TextBox();
            lblBatchSize = new Label();
            txtBatchSize = new TextBox();
            lblBatchWaitMinutes = new Label();
            txtBatchWaitMinutes = new TextBox();
            lblPedigreeMaxCount = new Label();
            txtPedigreeMaxCount = new TextBox();
            lblPayoutMaxCount = new Label();
            txtPayoutMaxCount = new TextBox();
            pSkip = new Panel();
            tblSkipRoot = new TableLayoutPanel();
            lblSkipTitle = new Label();
            flpSkip = new FlowLayoutPanel();
            chkSkipWin5 = new CheckBox();
            chkSkipPedigree = new CheckBox();
            chkSkipPayout = new CheckBox();
            pAction = new Panel();
            tblAction = new TableLayoutPanel();
            flpButtons = new FlowLayoutPanel();
            btnSave = new Button();
            btnCancel = new Button();
            rootTable.SuspendLayout();
            pSettings.SuspendLayout();
            tblSettingsRoot.SuspendLayout();
            tblSettings.SuspendLayout();
            pSkip.SuspendLayout();
            tblSkipRoot.SuspendLayout();
            flpSkip.SuspendLayout();
            pAction.SuspendLayout();
            tblAction.SuspendLayout();
            flpButtons.SuspendLayout();
            SuspendLayout();
            // 
            // rootTable
            // 
            rootTable.BackColor = Color.FromArgb(245, 246, 248);
            rootTable.ColumnCount = 1;
            rootTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootTable.Controls.Add(pSettings, 0, 0);
            rootTable.Controls.Add(pSkip, 0, 1);
            rootTable.Controls.Add(pAction, 0, 2);
            rootTable.Dock = DockStyle.Fill;
            rootTable.Location = new Point(0, 0);
            rootTable.Margin = new Padding(0);
            rootTable.Name = "rootTable";
            rootTable.Padding = new Padding(16);
            rootTable.RowCount = 3;
            rootTable.RowStyles.Add(new RowStyle());
            rootTable.RowStyles.Add(new RowStyle());
            rootTable.RowStyles.Add(new RowStyle());
            rootTable.Size = new Size(356, 410);
            rootTable.TabIndex = 0;
            // 
            // pSettings
            // 
            pSettings.BackColor = Color.White;
            pSettings.BorderStyle = BorderStyle.FixedSingle;
            pSettings.Controls.Add(tblSettingsRoot);
            pSettings.Dock = DockStyle.Top;
            pSettings.Location = new Point(16, 16);
            pSettings.Margin = new Padding(0, 0, 0, 12);
            pSettings.Name = "pSettings";
            pSettings.Padding = new Padding(12);
            pSettings.Size = new Size(324, 206);
            pSettings.TabIndex = 0;
            // 
            // tblSettingsRoot
            // 
            tblSettingsRoot.ColumnCount = 1;
            tblSettingsRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblSettingsRoot.Controls.Add(lblSettingsTitle, 0, 0);
            tblSettingsRoot.Controls.Add(tblSettings, 0, 1);
            tblSettingsRoot.Dock = DockStyle.Fill;
            tblSettingsRoot.Location = new Point(12, 12);
            tblSettingsRoot.Margin = new Padding(0);
            tblSettingsRoot.Name = "tblSettingsRoot";
            tblSettingsRoot.RowCount = 2;
            tblSettingsRoot.RowStyles.Add(new RowStyle());
            tblSettingsRoot.RowStyles.Add(new RowStyle());
            tblSettingsRoot.Size = new Size(298, 180);
            tblSettingsRoot.TabIndex = 0;
            // 
            // lblSettingsTitle
            // 
            lblSettingsTitle.AutoSize = true;
            lblSettingsTitle.Font = new Font("Segoe UI Semibold", 11F);
            lblSettingsTitle.Location = new Point(0, 0);
            lblSettingsTitle.Margin = new Padding(0, 0, 0, 8);
            lblSettingsTitle.Name = "lblSettingsTitle";
            lblSettingsTitle.Size = new Size(73, 20);
            lblSettingsTitle.TabIndex = 0;
            lblSettingsTitle.Text = "取得設定";
            // 
            // tblSettings
            // 
            tblSettings.AutoSize = true;
            tblSettings.ColumnCount = 2;
            tblSettings.ColumnStyles.Add(new ColumnStyle());
            tblSettings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblSettings.Controls.Add(lblPerRequestSec, 0, 0);
            tblSettings.Controls.Add(txtPerRequestSec, 1, 0);
            tblSettings.Controls.Add(lblBatchSize, 0, 1);
            tblSettings.Controls.Add(txtBatchSize, 1, 1);
            tblSettings.Controls.Add(lblBatchWaitMinutes, 0, 2);
            tblSettings.Controls.Add(txtBatchWaitMinutes, 1, 2);
            tblSettings.Controls.Add(lblPedigreeMaxCount, 0, 3);
            tblSettings.Controls.Add(txtPedigreeMaxCount, 1, 3);
            tblSettings.Controls.Add(lblPayoutMaxCount, 0, 4);
            tblSettings.Controls.Add(txtPayoutMaxCount, 1, 4);
            tblSettings.Dock = DockStyle.Top;
            tblSettings.Location = new Point(0, 28);
            tblSettings.Margin = new Padding(0);
            tblSettings.Name = "tblSettings";
            tblSettings.RowCount = 5;
            tblSettings.RowStyles.Add(new RowStyle());
            tblSettings.RowStyles.Add(new RowStyle());
            tblSettings.RowStyles.Add(new RowStyle());
            tblSettings.RowStyles.Add(new RowStyle());
            tblSettings.RowStyles.Add(new RowStyle());
            tblSettings.Size = new Size(298, 145);
            tblSettings.TabIndex = 1;
            // 
            // lblPerRequestSec
            // 
            lblPerRequestSec.AutoSize = true;
            lblPerRequestSec.Location = new Point(0, 6);
            lblPerRequestSec.Margin = new Padding(0, 6, 12, 6);
            lblPerRequestSec.Name = "lblPerRequestSec";
            lblPerRequestSec.Size = new Size(118, 15);
            lblPerRequestSec.TabIndex = 0;
            lblPerRequestSec.Text = "リクエスト間隔（秒）";
            // 
            // txtPerRequestSec
            // 
            txtPerRequestSec.Dock = DockStyle.Fill;
            txtPerRequestSec.Location = new Point(144, 3);
            txtPerRequestSec.Margin = new Padding(0, 3, 0, 3);
            txtPerRequestSec.Name = "txtPerRequestSec";
            txtPerRequestSec.Size = new Size(154, 23);
            txtPerRequestSec.TabIndex = 0;
            // 
            // lblBatchSize
            // 
            lblBatchSize.AutoSize = true;
            lblBatchSize.Location = new Point(0, 35);
            lblBatchSize.Margin = new Padding(0, 6, 12, 6);
            lblBatchSize.Name = "lblBatchSize";
            lblBatchSize.Size = new Size(67, 15);
            lblBatchSize.TabIndex = 2;
            lblBatchSize.Text = "バッチサイズ";
            // 
            // txtBatchSize
            // 
            txtBatchSize.Dock = DockStyle.Fill;
            txtBatchSize.Location = new Point(144, 32);
            txtBatchSize.Margin = new Padding(0, 3, 0, 3);
            txtBatchSize.Name = "txtBatchSize";
            txtBatchSize.Size = new Size(154, 23);
            txtBatchSize.TabIndex = 1;
            // 
            // lblBatchWaitMinutes
            // 
            lblBatchWaitMinutes.AutoSize = true;
            lblBatchWaitMinutes.Location = new Point(0, 64);
            lblBatchWaitMinutes.Margin = new Padding(0, 6, 12, 6);
            lblBatchWaitMinutes.Name = "lblBatchWaitMinutes";
            lblBatchWaitMinutes.Size = new Size(132, 15);
            lblBatchWaitMinutes.TabIndex = 4;
            lblBatchWaitMinutes.Text = "待機時間（分）/バッチ";
            // 
            // txtBatchWaitMinutes
            // 
            txtBatchWaitMinutes.Dock = DockStyle.Fill;
            txtBatchWaitMinutes.Location = new Point(144, 61);
            txtBatchWaitMinutes.Margin = new Padding(0, 3, 0, 3);
            txtBatchWaitMinutes.Name = "txtBatchWaitMinutes";
            txtBatchWaitMinutes.Size = new Size(154, 23);
            txtBatchWaitMinutes.TabIndex = 2;
            // 
            // lblPedigreeMaxCount
            // 
            lblPedigreeMaxCount.AutoSize = true;
            lblPedigreeMaxCount.Location = new Point(0, 93);
            lblPedigreeMaxCount.Margin = new Padding(0, 6, 12, 6);
            lblPedigreeMaxCount.Name = "lblPedigreeMaxCount";
            lblPedigreeMaxCount.Size = new Size(101, 15);
            lblPedigreeMaxCount.TabIndex = 6;
            lblPedigreeMaxCount.Text = "血統 最大取得数";
            // 
            // txtPedigreeMaxCount
            // 
            txtPedigreeMaxCount.Dock = DockStyle.Fill;
            txtPedigreeMaxCount.Location = new Point(144, 90);
            txtPedigreeMaxCount.Margin = new Padding(0, 3, 0, 3);
            txtPedigreeMaxCount.Name = "txtPedigreeMaxCount";
            txtPedigreeMaxCount.Size = new Size(154, 23);
            txtPedigreeMaxCount.TabIndex = 3;
            // 
            // lblPayoutMaxCount
            // 
            lblPayoutMaxCount.AutoSize = true;
            lblPayoutMaxCount.Location = new Point(0, 122);
            lblPayoutMaxCount.Margin = new Padding(0, 6, 12, 6);
            lblPayoutMaxCount.Name = "lblPayoutMaxCount";
            lblPayoutMaxCount.Size = new Size(101, 15);
            lblPayoutMaxCount.TabIndex = 8;
            lblPayoutMaxCount.Text = "払戻 最大取得数";
            // 
            // txtPayoutMaxCount
            // 
            txtPayoutMaxCount.Dock = DockStyle.Fill;
            txtPayoutMaxCount.Location = new Point(144, 119);
            txtPayoutMaxCount.Margin = new Padding(0, 3, 0, 3);
            txtPayoutMaxCount.Name = "txtPayoutMaxCount";
            txtPayoutMaxCount.Size = new Size(154, 23);
            txtPayoutMaxCount.TabIndex = 4;
            // 
            // pSkip
            // 
            pSkip.BackColor = Color.White;
            pSkip.BorderStyle = BorderStyle.FixedSingle;
            pSkip.Controls.Add(tblSkipRoot);
            pSkip.Dock = DockStyle.Top;
            pSkip.Location = new Point(16, 234);
            pSkip.Margin = new Padding(0, 0, 0, 12);
            pSkip.Name = "pSkip";
            pSkip.Padding = new Padding(12);
            pSkip.Size = new Size(324, 86);
            pSkip.TabIndex = 1;
            // 
            // tblSkipRoot
            // 
            tblSkipRoot.ColumnCount = 1;
            tblSkipRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblSkipRoot.Controls.Add(lblSkipTitle, 0, 0);
            tblSkipRoot.Controls.Add(flpSkip, 0, 1);
            tblSkipRoot.Dock = DockStyle.Fill;
            tblSkipRoot.Location = new Point(12, 12);
            tblSkipRoot.Margin = new Padding(0);
            tblSkipRoot.Name = "tblSkipRoot";
            tblSkipRoot.RowCount = 2;
            tblSkipRoot.RowStyles.Add(new RowStyle());
            tblSkipRoot.RowStyles.Add(new RowStyle());
            tblSkipRoot.Size = new Size(298, 60);
            tblSkipRoot.TabIndex = 0;
            // 
            // lblSkipTitle
            // 
            lblSkipTitle.AutoSize = true;
            lblSkipTitle.Font = new Font("Segoe UI Semibold", 11F);
            lblSkipTitle.Location = new Point(0, 0);
            lblSkipTitle.Margin = new Padding(0, 0, 0, 8);
            lblSkipTitle.Name = "lblSkipTitle";
            lblSkipTitle.Size = new Size(87, 20);
            lblSkipTitle.TabIndex = 0;
            lblSkipTitle.Text = "取得スキップ";
            // 
            // flpSkip
            // 
            flpSkip.AutoSize = true;
            flpSkip.Controls.Add(chkSkipWin5);
            flpSkip.Controls.Add(chkSkipPedigree);
            flpSkip.Controls.Add(chkSkipPayout);
            flpSkip.Dock = DockStyle.Top;
            flpSkip.Location = new Point(0, 28);
            flpSkip.Margin = new Padding(0);
            flpSkip.Name = "flpSkip";
            flpSkip.Size = new Size(298, 25);
            flpSkip.TabIndex = 1;
            // 
            // chkSkipWin5
            // 
            chkSkipWin5.AutoSize = true;
            chkSkipWin5.Location = new Point(0, 3);
            chkSkipWin5.Margin = new Padding(0, 3, 12, 3);
            chkSkipWin5.Name = "chkSkipWin5";
            chkSkipWin5.Size = new Size(55, 19);
            chkSkipWin5.TabIndex = 5;
            chkSkipWin5.Text = "WIN5";
            chkSkipWin5.UseVisualStyleBackColor = true;
            // 
            // chkSkipPedigree
            // 
            chkSkipPedigree.AutoSize = true;
            chkSkipPedigree.Location = new Point(67, 3);
            chkSkipPedigree.Margin = new Padding(0, 3, 12, 3);
            chkSkipPedigree.Name = "chkSkipPedigree";
            chkSkipPedigree.Size = new Size(52, 19);
            chkSkipPedigree.TabIndex = 6;
            chkSkipPedigree.Text = "血統";
            chkSkipPedigree.UseVisualStyleBackColor = true;
            // 
            // chkSkipPayout
            // 
            chkSkipPayout.AutoSize = true;
            chkSkipPayout.Location = new Point(131, 3);
            chkSkipPayout.Margin = new Padding(0, 3, 12, 3);
            chkSkipPayout.Name = "chkSkipPayout";
            chkSkipPayout.Size = new Size(52, 19);
            chkSkipPayout.TabIndex = 7;
            chkSkipPayout.Text = "払戻";
            chkSkipPayout.UseVisualStyleBackColor = true;
            // 
            // pAction
            // 
            pAction.AutoSize = true;
            pAction.BackColor = Color.White;
            pAction.BorderStyle = BorderStyle.FixedSingle;
            pAction.Controls.Add(tblAction);
            pAction.Dock = DockStyle.Top;
            pAction.Location = new Point(16, 332);
            pAction.Margin = new Padding(0);
            pAction.Name = "pAction";
            pAction.Padding = new Padding(12);
            pAction.Size = new Size(324, 76);
            pAction.TabIndex = 2;
            // 
            // tblAction
            // 
            tblAction.AutoSize = true;
            tblAction.ColumnCount = 1;
            tblAction.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblAction.Controls.Add(flpButtons, 0, 0);
            tblAction.Dock = DockStyle.Fill;
            tblAction.Location = new Point(12, 12);
            tblAction.Margin = new Padding(0);
            tblAction.Name = "tblAction";
            tblAction.RowCount = 1;
            tblAction.RowStyles.Add(new RowStyle());
            tblAction.Size = new Size(298, 50);
            tblAction.TabIndex = 0;
            // 
            // flpButtons
            // 
            flpButtons.AutoSize = true;
            flpButtons.Controls.Add(btnSave);
            flpButtons.Controls.Add(btnCancel);
            flpButtons.Dock = DockStyle.Right;
            flpButtons.FlowDirection = FlowDirection.RightToLeft;
            flpButtons.Location = new Point(140, 0);
            flpButtons.Margin = new Padding(0);
            flpButtons.Name = "flpButtons";
            flpButtons.Size = new Size(158, 50);
            flpButtons.TabIndex = 0;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Left;
            btnSave.AutoSize = true;
            btnSave.Location = new Point(75, 0);
            btnSave.Margin = new Padding(0, 0, 8, 0);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 25);
            btnSave.TabIndex = 8;
            btnSave.Text = "保存";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Left;
            btnCancel.AutoSize = true;
            btnCancel.Location = new Point(0, 0);
            btnCancel.Margin = new Padding(0);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 25);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "キャンセル";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // SettingsForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(356, 410);
            Controls.Add(rootTable);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "設定";
            rootTable.ResumeLayout(false);
            rootTable.PerformLayout();
            pSettings.ResumeLayout(false);
            tblSettingsRoot.ResumeLayout(false);
            tblSettingsRoot.PerformLayout();
            tblSettings.ResumeLayout(false);
            tblSettings.PerformLayout();
            pSkip.ResumeLayout(false);
            tblSkipRoot.ResumeLayout(false);
            tblSkipRoot.PerformLayout();
            flpSkip.ResumeLayout(false);
            flpSkip.PerformLayout();
            pAction.ResumeLayout(false);
            pAction.PerformLayout();
            tblAction.ResumeLayout(false);
            tblAction.PerformLayout();
            flpButtons.ResumeLayout(false);
            flpButtons.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel rootTable;
        private System.Windows.Forms.Panel pSettings;
        private System.Windows.Forms.TableLayoutPanel tblSettingsRoot;
        private System.Windows.Forms.Label lblSettingsTitle;
        private System.Windows.Forms.TableLayoutPanel tblSettings;
        private System.Windows.Forms.TextBox txtPerRequestSec;
        private System.Windows.Forms.TextBox txtBatchSize;
        private System.Windows.Forms.TextBox txtBatchWaitMinutes;
        private System.Windows.Forms.TextBox txtPedigreeMaxCount;
        private System.Windows.Forms.TextBox txtPayoutMaxCount;
        private System.Windows.Forms.Label lblPerRequestSec;
        private System.Windows.Forms.Label lblBatchSize;
        private System.Windows.Forms.Label lblBatchWaitMinutes;
        private System.Windows.Forms.Label lblPedigreeMaxCount;
        private System.Windows.Forms.Label lblPayoutMaxCount;
        private System.Windows.Forms.Panel pSkip;
        private System.Windows.Forms.TableLayoutPanel tblSkipRoot;
        private System.Windows.Forms.Label lblSkipTitle;
        private System.Windows.Forms.FlowLayoutPanel flpSkip;
        private System.Windows.Forms.CheckBox chkSkipWin5;
        private System.Windows.Forms.CheckBox chkSkipPedigree;
        private System.Windows.Forms.CheckBox chkSkipPayout;
        private System.Windows.Forms.Panel pAction;
        private System.Windows.Forms.TableLayoutPanel tblAction;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
