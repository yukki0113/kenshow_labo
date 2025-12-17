namespace ScrapingRunner {
    partial class MainForm {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            lblYearFrom = new Label();
            lblYearTo = new Label();
            chkPlace01 = new CheckBox();
            chkPlace02 = new CheckBox();
            chkPlace03 = new CheckBox();
            chkPlace04 = new CheckBox();
            chkPlace05 = new CheckBox();
            chkPlace06 = new CheckBox();
            chkPlace07 = new CheckBox();
            chkPlace08 = new CheckBox();
            chkPlace09 = new CheckBox();
            chkPlace10 = new CheckBox();
            lblPlace = new Label();
            lblWaveBorder = new Label();
            btnAll = new Button();
            btnSettings = new Button();
            btnRunStop = new Button();
            rtbOutput = new RichTextBox();
            nudYearFrom = new NumericUpDown();
            nudYearTo = new NumericUpDown();
            statusStripMain = new StatusStrip();
            tsslStatus = new ToolStripStatusLabel();
            rootTable = new TableLayoutPanel();
            pAction = new Panel();
            tblAction = new TableLayoutPanel();
            pLog = new Panel();
            tblLog = new TableLayoutPanel();
            pVenue = new Panel();
            tblVenue = new TableLayoutPanel();
            tblVenueHeader = new TableLayoutPanel();
            tblVenueGrid = new TableLayoutPanel();
            pYear = new Panel();
            tblYear = new TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)nudYearFrom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudYearTo).BeginInit();
            statusStripMain.SuspendLayout();
            rootTable.SuspendLayout();
            pAction.SuspendLayout();
            tblAction.SuspendLayout();
            pLog.SuspendLayout();
            tblLog.SuspendLayout();
            pVenue.SuspendLayout();
            tblVenue.SuspendLayout();
            tblVenueHeader.SuspendLayout();
            tblVenueGrid.SuspendLayout();
            pYear.SuspendLayout();
            tblYear.SuspendLayout();
            SuspendLayout();
            // 
            // lblYearFrom
            // 
            lblYearFrom.Anchor = AnchorStyles.Left;
            lblYearFrom.AutoSize = true;
            lblYearFrom.Font = new Font("MS UI Gothic", 9.75F, FontStyle.Bold);
            lblYearFrom.Location = new Point(0, 28);
            lblYearFrom.Margin = new Padding(0, 0, 0, 8);
            lblYearFrom.Name = "lblYearFrom";
            lblYearFrom.Size = new Size(49, 13);
            lblYearFrom.TabIndex = 2;
            lblYearFrom.Text = "開始年";
            // 
            // lblYearTo
            // 
            lblYearTo.Anchor = AnchorStyles.Left;
            lblYearTo.AutoSize = true;
            lblYearTo.Font = new Font("MS UI Gothic", 9.75F, FontStyle.Bold);
            lblYearTo.Location = new Point(185, 28);
            lblYearTo.Margin = new Padding(0, 0, 0, 8);
            lblYearTo.Name = "lblYearTo";
            lblYearTo.Size = new Size(49, 13);
            lblYearTo.TabIndex = 3;
            lblYearTo.Text = "終了年";
            // 
            // chkPlace01
            // 
            chkPlace01.Anchor = AnchorStyles.Left;
            chkPlace01.AutoSize = true;
            chkPlace01.Font = new Font("Segoe UI", 8F);
            chkPlace01.Location = new Point(3, 3);
            chkPlace01.Name = "chkPlace01";
            chkPlace01.Size = new Size(52, 17);
            chkPlace01.TabIndex = 4;
            chkPlace01.Text = "札幌";
            chkPlace01.UseVisualStyleBackColor = true;
            // 
            // chkPlace02
            // 
            chkPlace02.Anchor = AnchorStyles.Left;
            chkPlace02.AutoSize = true;
            chkPlace02.Font = new Font("Segoe UI", 8F);
            chkPlace02.Location = new Point(61, 3);
            chkPlace02.Name = "chkPlace02";
            chkPlace02.Size = new Size(52, 17);
            chkPlace02.TabIndex = 5;
            chkPlace02.Text = "函館";
            chkPlace02.UseVisualStyleBackColor = true;
            // 
            // chkPlace03
            // 
            chkPlace03.Anchor = AnchorStyles.Left;
            chkPlace03.AutoSize = true;
            chkPlace03.Font = new Font("Segoe UI", 8F);
            chkPlace03.Location = new Point(119, 3);
            chkPlace03.Name = "chkPlace03";
            chkPlace03.Size = new Size(52, 17);
            chkPlace03.TabIndex = 6;
            chkPlace03.Text = "福島";
            chkPlace03.UseVisualStyleBackColor = true;
            // 
            // chkPlace04
            // 
            chkPlace04.Anchor = AnchorStyles.Left;
            chkPlace04.AutoSize = true;
            chkPlace04.Font = new Font("Segoe UI", 8F);
            chkPlace04.Location = new Point(177, 3);
            chkPlace04.Name = "chkPlace04";
            chkPlace04.Size = new Size(52, 17);
            chkPlace04.TabIndex = 7;
            chkPlace04.Text = "新潟";
            chkPlace04.UseVisualStyleBackColor = true;
            // 
            // chkPlace05
            // 
            chkPlace05.Anchor = AnchorStyles.Left;
            chkPlace05.AutoSize = true;
            chkPlace05.Font = new Font("Segoe UI", 8F);
            chkPlace05.Location = new Point(235, 3);
            chkPlace05.Name = "chkPlace05";
            chkPlace05.Size = new Size(52, 17);
            chkPlace05.TabIndex = 8;
            chkPlace05.Text = "東京";
            chkPlace05.UseVisualStyleBackColor = true;
            // 
            // chkPlace06
            // 
            chkPlace06.Anchor = AnchorStyles.Left;
            chkPlace06.AutoSize = true;
            chkPlace06.Font = new Font("Segoe UI", 8F);
            chkPlace06.Location = new Point(3, 26);
            chkPlace06.Name = "chkPlace06";
            chkPlace06.Size = new Size(52, 17);
            chkPlace06.TabIndex = 9;
            chkPlace06.Text = "中山";
            chkPlace06.UseVisualStyleBackColor = true;
            // 
            // chkPlace07
            // 
            chkPlace07.Anchor = AnchorStyles.Left;
            chkPlace07.AutoSize = true;
            chkPlace07.Font = new Font("Segoe UI", 8F);
            chkPlace07.Location = new Point(61, 26);
            chkPlace07.Name = "chkPlace07";
            chkPlace07.Size = new Size(52, 17);
            chkPlace07.TabIndex = 10;
            chkPlace07.Text = "中京";
            chkPlace07.UseVisualStyleBackColor = true;
            // 
            // chkPlace08
            // 
            chkPlace08.Anchor = AnchorStyles.Left;
            chkPlace08.AutoSize = true;
            chkPlace08.Font = new Font("Segoe UI", 8F);
            chkPlace08.Location = new Point(119, 26);
            chkPlace08.Name = "chkPlace08";
            chkPlace08.Size = new Size(52, 17);
            chkPlace08.TabIndex = 11;
            chkPlace08.Text = "京都";
            chkPlace08.UseVisualStyleBackColor = true;
            // 
            // chkPlace09
            // 
            chkPlace09.Anchor = AnchorStyles.Left;
            chkPlace09.AutoSize = true;
            chkPlace09.Font = new Font("Segoe UI", 8F);
            chkPlace09.Location = new Point(177, 26);
            chkPlace09.Name = "chkPlace09";
            chkPlace09.Size = new Size(52, 17);
            chkPlace09.TabIndex = 12;
            chkPlace09.Text = "阪神";
            chkPlace09.UseVisualStyleBackColor = true;
            // 
            // chkPlace10
            // 
            chkPlace10.Anchor = AnchorStyles.Left;
            chkPlace10.AutoSize = true;
            chkPlace10.Font = new Font("Segoe UI", 8F);
            chkPlace10.Location = new Point(235, 26);
            chkPlace10.Name = "chkPlace10";
            chkPlace10.Size = new Size(52, 17);
            chkPlace10.TabIndex = 13;
            chkPlace10.Text = "小倉";
            chkPlace10.UseVisualStyleBackColor = true;
            // 
            // lblPlace
            // 
            lblPlace.Anchor = AnchorStyles.Left;
            lblPlace.AutoSize = true;
            lblPlace.Font = new Font("MS UI Gothic", 9.75F, FontStyle.Bold);
            lblPlace.Location = new Point(3, 6);
            lblPlace.Name = "lblPlace";
            lblPlace.Size = new Size(63, 13);
            lblPlace.TabIndex = 14;
            lblPlace.Text = "取得会場";
            // 
            // lblWaveBorder
            // 
            lblWaveBorder.Anchor = AnchorStyles.None;
            lblWaveBorder.AutoSize = true;
            lblWaveBorder.Font = new Font("Segoe UI", 9F);
            lblWaveBorder.Location = new Point(165, 53);
            lblWaveBorder.Margin = new Padding(0, 0, 0, 8);
            lblWaveBorder.Name = "lblWaveBorder";
            lblWaveBorder.Size = new Size(20, 15);
            lblWaveBorder.TabIndex = 15;
            lblWaveBorder.Text = "～";
            // 
            // btnAll
            // 
            btnAll.Anchor = AnchorStyles.Right;
            btnAll.Font = new Font("Segoe UI", 6.75F);
            btnAll.Location = new Point(296, 3);
            btnAll.Name = "btnAll";
            btnAll.Size = new Size(45, 20);
            btnAll.TabIndex = 16;
            btnAll.Text = "全選択";
            btnAll.UseVisualStyleBackColor = true;
            btnAll.Click += btnAll_Click;
            // 
            // btnSettings
            // 
            btnSettings.Anchor = AnchorStyles.Right;
            btnSettings.AutoSize = true;
            btnSettings.Font = new Font("Segoe UI", 6F);
            btnSettings.Location = new Point(302, 3);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(45, 22);
            btnSettings.TabIndex = 17;
            btnSettings.Text = "設定";
            btnSettings.UseVisualStyleBackColor = true;
            btnSettings.Click += btnSettings_Click;
            // 
            // btnRunStop
            // 
            btnRunStop.Anchor = AnchorStyles.None;
            btnRunStop.AutoSize = true;
            btnRunStop.Font = new Font("Segoe UI", 8.25F);
            btnRunStop.Location = new Point(142, 3);
            btnRunStop.Name = "btnRunStop";
            btnRunStop.Padding = new Padding(5);
            btnRunStop.Size = new Size(66, 33);
            btnRunStop.TabIndex = 18;
            btnRunStop.Text = "実行";
            btnRunStop.UseVisualStyleBackColor = true;
            btnRunStop.Click += btnRunStop_Click;
            // 
            // rtbOutput
            // 
            rtbOutput.BorderStyle = BorderStyle.FixedSingle;
            rtbOutput.Dock = DockStyle.Fill;
            rtbOutput.Font = new Font("Segoe UI", 9F);
            rtbOutput.Location = new Point(3, 3);
            rtbOutput.Name = "rtbOutput";
            rtbOutput.ReadOnly = true;
            rtbOutput.Size = new Size(344, 5);
            rtbOutput.TabIndex = 19;
            rtbOutput.Text = "";
            // 
            // nudYearFrom
            // 
            nudYearFrom.Dock = DockStyle.Fill;
            nudYearFrom.Font = new Font("Segoe UI Symbol", 9.75F);
            nudYearFrom.Location = new Point(3, 52);
            nudYearFrom.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            nudYearFrom.Name = "nudYearFrom";
            nudYearFrom.Size = new Size(159, 25);
            nudYearFrom.TabIndex = 21;
            nudYearFrom.TextAlign = HorizontalAlignment.Right;
            // 
            // nudYearTo
            // 
            nudYearTo.Dock = DockStyle.Fill;
            nudYearTo.Font = new Font("Segoe UI Symbol", 9.75F);
            nudYearTo.Location = new Point(188, 52);
            nudYearTo.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            nudYearTo.Name = "nudYearTo";
            nudYearTo.Size = new Size(159, 25);
            nudYearTo.TabIndex = 22;
            nudYearTo.TextAlign = HorizontalAlignment.Right;
            // 
            // statusStripMain
            // 
            statusStripMain.BackColor = Color.WhiteSmoke;
            statusStripMain.Items.AddRange(new ToolStripItem[] { tsslStatus });
            statusStripMain.Location = new Point(0, 388);
            statusStripMain.Name = "statusStripMain";
            statusStripMain.Size = new Size(408, 22);
            statusStripMain.TabIndex = 23;
            statusStripMain.Text = "statusStrip1";
            // 
            // tsslStatus
            // 
            tsslStatus.Name = "tsslStatus";
            tsslStatus.Size = new Size(52, 17);
            tsslStatus.Text = "待機中...";
            // 
            // rootTable
            // 
            rootTable.BackColor = Color.WhiteSmoke;
            rootTable.ColumnCount = 1;
            rootTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootTable.Controls.Add(pAction, 0, 4);
            rootTable.Controls.Add(pLog, 0, 3);
            rootTable.Controls.Add(pVenue, 0, 2);
            rootTable.Controls.Add(pYear, 0, 1);
            rootTable.Dock = DockStyle.Fill;
            rootTable.Location = new Point(0, 0);
            rootTable.Name = "rootTable";
            rootTable.Padding = new Padding(16);
            rootTable.RowCount = 5;
            rootTable.RowStyles.Add(new RowStyle());
            rootTable.RowStyles.Add(new RowStyle());
            rootTable.RowStyles.Add(new RowStyle());
            rootTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootTable.RowStyles.Add(new RowStyle());
            rootTable.Size = new Size(408, 388);
            rootTable.TabIndex = 24;
            // 
            // pAction
            // 
            pAction.AutoSize = true;
            pAction.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pAction.BackColor = Color.White;
            pAction.BorderStyle = BorderStyle.FixedSingle;
            pAction.Controls.Add(tblAction);
            pAction.Dock = DockStyle.Top;
            pAction.Location = new Point(16, 307);
            pAction.Margin = new Padding(0);
            pAction.Name = "pAction";
            pAction.Padding = new Padding(12);
            pAction.Size = new Size(376, 65);
            pAction.TabIndex = 6;
            // 
            // tblAction
            // 
            tblAction.AutoSize = true;
            tblAction.ColumnCount = 1;
            tblAction.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblAction.Controls.Add(btnRunStop, 0, 0);
            tblAction.Dock = DockStyle.Fill;
            tblAction.Location = new Point(12, 12);
            tblAction.Margin = new Padding(0);
            tblAction.Name = "tblAction";
            tblAction.RowCount = 1;
            tblAction.RowStyles.Add(new RowStyle());
            tblAction.Size = new Size(350, 39);
            tblAction.TabIndex = 0;
            // 
            // pLog
            // 
            pLog.AutoSize = true;
            pLog.BackColor = Color.White;
            pLog.BorderStyle = BorderStyle.FixedSingle;
            pLog.Controls.Add(tblLog);
            pLog.Dock = DockStyle.Fill;
            pLog.Location = new Point(16, 258);
            pLog.Margin = new Padding(0, 0, 0, 12);
            pLog.Name = "pLog";
            pLog.Padding = new Padding(12);
            pLog.Size = new Size(376, 37);
            pLog.TabIndex = 5;
            // 
            // tblLog
            // 
            tblLog.AutoSize = true;
            tblLog.ColumnCount = 1;
            tblLog.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblLog.Controls.Add(rtbOutput, 0, 0);
            tblLog.Dock = DockStyle.Fill;
            tblLog.Location = new Point(12, 12);
            tblLog.Name = "tblLog";
            tblLog.RowCount = 1;
            tblLog.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblLog.Size = new Size(350, 11);
            tblLog.TabIndex = 0;
            // 
            // pVenue
            // 
            pVenue.AutoSize = true;
            pVenue.BackColor = Color.White;
            pVenue.BorderStyle = BorderStyle.FixedSingle;
            pVenue.Controls.Add(tblVenue);
            pVenue.Dock = DockStyle.Fill;
            pVenue.Location = new Point(16, 134);
            pVenue.Margin = new Padding(0, 0, 0, 12);
            pVenue.Name = "pVenue";
            pVenue.Padding = new Padding(12);
            pVenue.Size = new Size(376, 112);
            pVenue.TabIndex = 4;
            // 
            // tblVenue
            // 
            tblVenue.AutoSize = true;
            tblVenue.ColumnCount = 1;
            tblVenue.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblVenue.Controls.Add(tblVenueHeader, 0, 0);
            tblVenue.Controls.Add(tblVenueGrid, 0, 1);
            tblVenue.Dock = DockStyle.Top;
            tblVenue.Location = new Point(12, 12);
            tblVenue.Margin = new Padding(0);
            tblVenue.Name = "tblVenue";
            tblVenue.RowCount = 2;
            tblVenue.RowStyles.Add(new RowStyle());
            tblVenue.RowStyles.Add(new RowStyle());
            tblVenue.Size = new Size(350, 86);
            tblVenue.TabIndex = 1;
            // 
            // tblVenueHeader
            // 
            tblVenueHeader.AutoSize = true;
            tblVenueHeader.ColumnCount = 2;
            tblVenueHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblVenueHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblVenueHeader.Controls.Add(btnAll, 1, 0);
            tblVenueHeader.Controls.Add(lblPlace, 0, 0);
            tblVenueHeader.Dock = DockStyle.Top;
            tblVenueHeader.Location = new Point(3, 3);
            tblVenueHeader.Name = "tblVenueHeader";
            tblVenueHeader.RowCount = 1;
            tblVenueHeader.RowStyles.Add(new RowStyle());
            tblVenueHeader.Size = new Size(344, 26);
            tblVenueHeader.TabIndex = 0;
            // 
            // tblVenueGrid
            // 
            tblVenueGrid.Anchor = AnchorStyles.Top;
            tblVenueGrid.AutoSize = true;
            tblVenueGrid.ColumnCount = 5;
            tblVenueGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tblVenueGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tblVenueGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tblVenueGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tblVenueGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tblVenueGrid.Controls.Add(chkPlace04, 3, 0);
            tblVenueGrid.Controls.Add(chkPlace03, 2, 0);
            tblVenueGrid.Controls.Add(chkPlace02, 1, 0);
            tblVenueGrid.Controls.Add(chkPlace01, 0, 0);
            tblVenueGrid.Controls.Add(chkPlace05, 4, 0);
            tblVenueGrid.Controls.Add(chkPlace10, 4, 1);
            tblVenueGrid.Controls.Add(chkPlace06, 0, 1);
            tblVenueGrid.Controls.Add(chkPlace09, 3, 1);
            tblVenueGrid.Controls.Add(chkPlace07, 1, 1);
            tblVenueGrid.Controls.Add(chkPlace08, 2, 1);
            tblVenueGrid.Location = new Point(24, 36);
            tblVenueGrid.Margin = new Padding(0, 4, 12, 4);
            tblVenueGrid.Name = "tblVenueGrid";
            tblVenueGrid.RowCount = 2;
            tblVenueGrid.RowStyles.Add(new RowStyle());
            tblVenueGrid.RowStyles.Add(new RowStyle());
            tblVenueGrid.Size = new Size(290, 46);
            tblVenueGrid.TabIndex = 1;
            // 
            // pYear
            // 
            pYear.AutoSize = true;
            pYear.BackColor = Color.White;
            pYear.BorderStyle = BorderStyle.FixedSingle;
            pYear.Controls.Add(tblYear);
            pYear.Dock = DockStyle.Fill;
            pYear.Location = new Point(16, 16);
            pYear.Margin = new Padding(0, 0, 0, 12);
            pYear.Name = "pYear";
            pYear.Padding = new Padding(12);
            pYear.Size = new Size(376, 106);
            pYear.TabIndex = 3;
            pYear.Paint += pYear_Paint;
            // 
            // tblYear
            // 
            tblYear.AutoSize = true;
            tblYear.ColumnCount = 3;
            tblYear.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblYear.ColumnStyles.Add(new ColumnStyle());
            tblYear.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblYear.Controls.Add(btnSettings, 2, 0);
            tblYear.Controls.Add(nudYearTo, 2, 2);
            tblYear.Controls.Add(nudYearFrom, 0, 2);
            tblYear.Controls.Add(lblYearTo, 2, 1);
            tblYear.Controls.Add(lblYearFrom, 0, 1);
            tblYear.Controls.Add(lblWaveBorder, 1, 2);
            tblYear.Dock = DockStyle.Fill;
            tblYear.Location = new Point(12, 12);
            tblYear.Margin = new Padding(0);
            tblYear.Name = "tblYear";
            tblYear.RowCount = 3;
            tblYear.RowStyles.Add(new RowStyle());
            tblYear.RowStyles.Add(new RowStyle());
            tblYear.RowStyles.Add(new RowStyle());
            tblYear.Size = new Size(350, 80);
            tblYear.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = SystemColors.Control;
            ClientSize = new Size(408, 410);
            Controls.Add(rootTable);
            Controls.Add(statusStripMain);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "MainForm";
            Text = "ScrapingRunner";
            ((System.ComponentModel.ISupportInitialize)nudYearFrom).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudYearTo).EndInit();
            statusStripMain.ResumeLayout(false);
            statusStripMain.PerformLayout();
            rootTable.ResumeLayout(false);
            rootTable.PerformLayout();
            pAction.ResumeLayout(false);
            pAction.PerformLayout();
            tblAction.ResumeLayout(false);
            tblAction.PerformLayout();
            pLog.ResumeLayout(false);
            pLog.PerformLayout();
            tblLog.ResumeLayout(false);
            pVenue.ResumeLayout(false);
            pVenue.PerformLayout();
            tblVenue.ResumeLayout(false);
            tblVenue.PerformLayout();
            tblVenueHeader.ResumeLayout(false);
            tblVenueHeader.PerformLayout();
            tblVenueGrid.ResumeLayout(false);
            tblVenueGrid.PerformLayout();
            pYear.ResumeLayout(false);
            pYear.PerformLayout();
            tblYear.ResumeLayout(false);
            tblYear.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblYearFrom;
        private Label lblYearTo;
        private CheckBox chkPlace01;
        private CheckBox chkPlace02;
        private CheckBox chkPlace03;
        private CheckBox chkPlace04;
        private CheckBox chkPlace05;
        private CheckBox chkPlace06;
        private CheckBox chkPlace07;
        private CheckBox chkPlace08;
        private CheckBox chkPlace09;
        private CheckBox chkPlace10;
        private Label lblPlace;
        private Label lblWaveBorder;
        private Button btnAll;
        private Button btnSettings;
        private Button btnRunStop;
        private RichTextBox rtbOutput;
        private NumericUpDown nudYearFrom;
        private NumericUpDown nudYearTo;
        private StatusStrip statusStripMain;
        private ToolStripStatusLabel tsslStatus;
        private TableLayoutPanel rootTable;
        private Panel pYear;
        private Panel pAction;
        private Panel pLog;
        private Panel pVenue;
        private TableLayoutPanel tblYear;
        private TableLayoutPanel tblVenue;
        private TableLayoutPanel tblVenueHeader;
        private TableLayoutPanel tblVenueGrid;
        private TableLayoutPanel tblLog;
        private TableLayoutPanel tblAction;
    }
}
