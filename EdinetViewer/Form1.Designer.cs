namespace EdinetViewer {
    partial class Form1 {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.dgvList = new System.Windows.Forms.DataGridView();
            this.dgvContents = new System.Windows.Forms.DataGridView();
            this.dgvXbrl = new System.Windows.Forms.DataGridView();
            this.browser = new System.Windows.Forms.WebBrowser();
            this.MenuToolbar = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuSetting = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuEdinet = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuBackground = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuPastList = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuDownload = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuImportTaxonomy = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.ProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.ProgressLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.splitUpper = new System.Windows.Forms.SplitContainer();
            this.splitLower = new System.Windows.Forms.SplitContainer();
            this.DatePicker = new System.Windows.Forms.DateTimePicker();
            this.panel1 = new System.Windows.Forms.Panel();
            this.comboFilter = new System.Windows.Forms.ComboBox();
            this.TbCode = new System.Windows.Forms.TextBox();
            this.LabelVersion = new System.Windows.Forms.Label();
            this.TbVersion = new System.Windows.Forms.TextBox();
            this.LabelMetadata = new System.Windows.Forms.Label();
            this.splitForm = new System.Windows.Forms.SplitContainer();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.MenuIEdinetCodeImport = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dgvList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvXbrl)).BeginInit();
            this.MenuToolbar.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitUpper)).BeginInit();
            this.splitUpper.Panel1.SuspendLayout();
            this.splitUpper.Panel2.SuspendLayout();
            this.splitUpper.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLower)).BeginInit();
            this.splitLower.Panel1.SuspendLayout();
            this.splitLower.Panel2.SuspendLayout();
            this.splitLower.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitForm)).BeginInit();
            this.splitForm.Panel1.SuspendLayout();
            this.splitForm.Panel2.SuspendLayout();
            this.splitForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvList
            // 
            this.dgvList.AllowUserToAddRows = false;
            this.dgvList.AllowUserToDeleteRows = false;
            this.dgvList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvList.Location = new System.Drawing.Point(0, 0);
            this.dgvList.MultiSelect = false;
            this.dgvList.Name = "dgvList";
            this.dgvList.ReadOnly = true;
            this.dgvList.RowHeadersWidth = 10;
            this.dgvList.RowTemplate.Height = 21;
            this.dgvList.Size = new System.Drawing.Size(501, 181);
            this.dgvList.TabIndex = 0;
            this.dgvList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvList_CellContentClick);
            this.dgvList.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DataGridView_CellFormatting);
            this.dgvList.CurrentCellChanged += new System.EventHandler(this.DgvList_CurrentCellChanged);
            this.dgvList.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridView_DataError);
            // 
            // dgvContents
            // 
            this.dgvContents.AllowUserToAddRows = false;
            this.dgvContents.AllowUserToDeleteRows = false;
            this.dgvContents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvContents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvContents.Location = new System.Drawing.Point(0, 0);
            this.dgvContents.MultiSelect = false;
            this.dgvContents.Name = "dgvContents";
            this.dgvContents.ReadOnly = true;
            this.dgvContents.RowHeadersWidth = 20;
            this.dgvContents.RowTemplate.Height = 21;
            this.dgvContents.Size = new System.Drawing.Size(279, 181);
            this.dgvContents.TabIndex = 1;
            this.dgvContents.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DataGridView_CellFormatting);
            this.dgvContents.CurrentCellChanged += new System.EventHandler(this.DgvContents_CurrentCellChanged);
            this.dgvContents.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridView_DataError);
            // 
            // dgvXbrl
            // 
            this.dgvXbrl.AllowUserToAddRows = false;
            this.dgvXbrl.AllowUserToDeleteRows = false;
            this.dgvXbrl.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvXbrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvXbrl.Location = new System.Drawing.Point(0, 0);
            this.dgvXbrl.MultiSelect = false;
            this.dgvXbrl.Name = "dgvXbrl";
            this.dgvXbrl.ReadOnly = true;
            this.dgvXbrl.RowHeadersWidth = 20;
            this.dgvXbrl.RowTemplate.Height = 21;
            this.dgvXbrl.Size = new System.Drawing.Size(360, 365);
            this.dgvXbrl.TabIndex = 0;
            this.dgvXbrl.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DataGridView_CellFormatting);
            this.dgvXbrl.CurrentCellChanged += new System.EventHandler(this.DgvXbrl_CurrentCellChanged);
            this.dgvXbrl.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridView_DataError);
            // 
            // browser
            // 
            this.browser.ContextMenuStrip = this.MenuToolbar;
            this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browser.Location = new System.Drawing.Point(0, 0);
            this.browser.MinimumSize = new System.Drawing.Size(20, 20);
            this.browser.Name = "browser";
            this.browser.ScriptErrorsSuppressed = true;
            this.browser.Size = new System.Drawing.Size(420, 365);
            this.browser.TabIndex = 1;
            this.browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.Browser_DocumentCompleted);
            // 
            // MenuToolbar
            // 
            this.MenuToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuSetting,
            this.MenuEdinet,
            this.MenuBackground});
            this.MenuToolbar.Name = "contextMenuStrip1";
            this.MenuToolbar.Size = new System.Drawing.Size(194, 92);
            // 
            // MenuSetting
            // 
            this.MenuSetting.Name = "MenuSetting";
            this.MenuSetting.Size = new System.Drawing.Size(193, 22);
            this.MenuSetting.Text = "設定";
            this.MenuSetting.Click += new System.EventHandler(this.Menu_Click);
            // 
            // MenuEdinet
            // 
            this.MenuEdinet.Name = "MenuEdinet";
            this.MenuEdinet.Size = new System.Drawing.Size(193, 22);
            this.MenuEdinet.Text = "Edinetトップページを表示";
            this.MenuEdinet.Click += new System.EventHandler(this.Menu_Click);
            // 
            // MenuBackground
            // 
            this.MenuBackground.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuPastList,
            this.MenuDownload,
            this.MenuImportTaxonomy,
            this.MenuIEdinetCodeImport});
            this.MenuBackground.Name = "MenuBackground";
            this.MenuBackground.Size = new System.Drawing.Size(193, 22);
            this.MenuBackground.Text = "バックグラウンドで実行";
            // 
            // MenuPastList
            // 
            this.MenuPastList.CheckOnClick = true;
            this.MenuPastList.Name = "MenuPastList";
            this.MenuPastList.Size = new System.Drawing.Size(214, 22);
            this.MenuPastList.Text = "過去5年間の書類一覧取得";
            this.MenuPastList.ToolTipText = "5年前から日付ごとに順次書類一覧を取得します。\r\n設定ダイアログでチェックされた書類タイプのアーカイブは合わせてダウンロードします。\r\n書類一覧および書類アーカイ" +
    "ブの取得とダウンロードのリクエストごとに、設定ダイアログで指定したウェイトをかけます。\r\nメニューのチェックのオンオフで再開中断を切り替えます。";
            this.MenuPastList.CheckedChanged += new System.EventHandler(this.MenuBackground_CheckedChanged);
            // 
            // MenuDownload
            // 
            this.MenuDownload.CheckOnClick = true;
            this.MenuDownload.Name = "MenuDownload";
            this.MenuDownload.Size = new System.Drawing.Size(214, 22);
            this.MenuDownload.Text = "過去5年間の書類ダウンロード";
            this.MenuDownload.ToolTipText = "設定ダイアログでチェックされている書類を5年前から現在に向かってダウンロードします。\r\n実行中はチェックがオンになり、オフオンで中断再開可能です。";
            this.MenuDownload.CheckedChanged += new System.EventHandler(this.MenuBackground_CheckedChanged);
            // 
            // MenuImportTaxonomy
            // 
            this.MenuImportTaxonomy.Name = "MenuImportTaxonomy";
            this.MenuImportTaxonomy.Size = new System.Drawing.Size(214, 22);
            this.MenuImportTaxonomy.Text = "タクソノミのインポート";
            this.MenuImportTaxonomy.ToolTipText = "最新タクソノミで更新したい場合に実行してください。";
            this.MenuImportTaxonomy.Click += new System.EventHandler(this.MenuBackground_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ProgressBar1,
            this.ProgressLabel1,
            this.StatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 579);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(784, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // ProgressBar1
            // 
            this.ProgressBar1.AutoSize = false;
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // ProgressLabel1
            // 
            this.ProgressLabel1.Name = "ProgressLabel1";
            this.ProgressLabel1.Size = new System.Drawing.Size(13, 17);
            this.ProgressLabel1.Text = "  ";
            // 
            // StatusLabel1
            // 
            this.StatusLabel1.Margin = new System.Windows.Forms.Padding(10, 3, 0, 2);
            this.StatusLabel1.Name = "StatusLabel1";
            this.StatusLabel1.Size = new System.Drawing.Size(19, 17);
            this.StatusLabel1.Text = "    ";
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.splitUpper);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.splitLower);
            this.splitMain.Size = new System.Drawing.Size(784, 550);
            this.splitMain.SplitterDistance = 181;
            this.splitMain.TabIndex = 3;
            // 
            // splitUpper
            // 
            this.splitUpper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitUpper.Location = new System.Drawing.Point(0, 0);
            this.splitUpper.Name = "splitUpper";
            // 
            // splitUpper.Panel1
            // 
            this.splitUpper.Panel1.Controls.Add(this.dgvList);
            // 
            // splitUpper.Panel2
            // 
            this.splitUpper.Panel2.Controls.Add(this.dgvContents);
            this.splitUpper.Size = new System.Drawing.Size(784, 181);
            this.splitUpper.SplitterDistance = 501;
            this.splitUpper.TabIndex = 0;
            // 
            // splitLower
            // 
            this.splitLower.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLower.Location = new System.Drawing.Point(0, 0);
            this.splitLower.Name = "splitLower";
            // 
            // splitLower.Panel1
            // 
            this.splitLower.Panel1.Controls.Add(this.browser);
            // 
            // splitLower.Panel2
            // 
            this.splitLower.Panel2.Controls.Add(this.dgvXbrl);
            this.splitLower.Size = new System.Drawing.Size(784, 365);
            this.splitLower.SplitterDistance = 420;
            this.splitLower.TabIndex = 0;
            // 
            // DatePicker
            // 
            this.DatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.DatePicker.Location = new System.Drawing.Point(3, 3);
            this.DatePicker.Name = "DatePicker";
            this.DatePicker.Size = new System.Drawing.Size(154, 19);
            this.DatePicker.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.ContextMenuStrip = this.MenuToolbar;
            this.panel1.Controls.Add(this.comboFilter);
            this.panel1.Controls.Add(this.TbCode);
            this.panel1.Controls.Add(this.LabelVersion);
            this.panel1.Controls.Add(this.TbVersion);
            this.panel1.Controls.Add(this.LabelMetadata);
            this.panel1.Controls.Add(this.DatePicker);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(784, 25);
            this.panel1.TabIndex = 5;
            // 
            // comboFilter
            // 
            this.comboFilter.FormattingEnabled = true;
            this.comboFilter.Location = new System.Drawing.Point(162, 3);
            this.comboFilter.Name = "comboFilter";
            this.comboFilter.Size = new System.Drawing.Size(121, 20);
            this.comboFilter.TabIndex = 1;
            this.comboFilter.SelectedIndexChanged += new System.EventHandler(this.ComboFilter_SelectedIndexChanged);
            // 
            // TbCode
            // 
            this.TbCode.Location = new System.Drawing.Point(292, 4);
            this.TbCode.Name = "TbCode";
            this.TbCode.Size = new System.Drawing.Size(34, 19);
            this.TbCode.TabIndex = 8;
            this.TbCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.TbCode, "4桁コードを入力するとデータベースに保存された銘柄を絞り込み表示します");
            this.TbCode.TextChanged += new System.EventHandler(this.TbCode_TextChanged);
            // 
            // LabelVersion
            // 
            this.LabelVersion.AutoSize = true;
            this.LabelVersion.Location = new System.Drawing.Point(693, 8);
            this.LabelVersion.Name = "LabelVersion";
            this.LabelVersion.Size = new System.Drawing.Size(66, 12);
            this.LabelVersion.TabIndex = 7;
            this.LabelVersion.Text = "API Version";
            this.toolTip1.SetToolTip(this.LabelVersion, "ダブルクリックで編集可能");
            this.LabelVersion.DoubleClick += new System.EventHandler(this.LabelVersion_DoubleClick);
            // 
            // TbVersion
            // 
            this.TbVersion.Enabled = false;
            this.TbVersion.Location = new System.Drawing.Point(760, 4);
            this.TbVersion.Name = "TbVersion";
            this.TbVersion.Size = new System.Drawing.Size(25, 19);
            this.TbVersion.TabIndex = 2;
            this.TbVersion.Leave += new System.EventHandler(this.TbVersion_Leave);
            // 
            // LabelMetadata
            // 
            this.LabelMetadata.AutoSize = true;
            this.LabelMetadata.Location = new System.Drawing.Point(334, 8);
            this.LabelMetadata.Name = "LabelMetadata";
            this.LabelMetadata.Size = new System.Drawing.Size(45, 12);
            this.LabelMetadata.TabIndex = 6;
            this.LabelMetadata.Text = "          ";
            this.LabelMetadata.DoubleClick += new System.EventHandler(this.LabelMetadata_DoubleClick);
            // 
            // splitForm
            // 
            this.splitForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitForm.Location = new System.Drawing.Point(0, 0);
            this.splitForm.Name = "splitForm";
            this.splitForm.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitForm.Panel1
            // 
            this.splitForm.Panel1.Controls.Add(this.panel1);
            // 
            // splitForm.Panel2
            // 
            this.splitForm.Panel2.Controls.Add(this.splitMain);
            this.splitForm.Size = new System.Drawing.Size(784, 579);
            this.splitForm.SplitterDistance = 25;
            this.splitForm.TabIndex = 6;
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 50;
            this.toolTip1.ReshowDelay = 30;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer_Tick);
            // 
            // MenuIEdinetCodeImport
            // 
            this.MenuIEdinetCodeImport.Name = "MenuIEdinetCodeImport";
            this.MenuIEdinetCodeImport.Size = new System.Drawing.Size(214, 22);
            this.MenuIEdinetCodeImport.Text = "Edinetコードリストのインポート";
            this.MenuIEdinetCodeImport.ToolTipText = "ダウンロード済みのEdinetコードリストまたはファンドコードリストをデータベースにインポートします。";
            this.MenuIEdinetCodeImport.Click += new System.EventHandler(this.MenuBackground_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 601);
            this.Controls.Add(this.splitForm);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Text = "DisclosureViewer EdinetAPI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.dgvList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContents)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvXbrl)).EndInit();
            this.MenuToolbar.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.splitUpper.Panel1.ResumeLayout(false);
            this.splitUpper.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitUpper)).EndInit();
            this.splitUpper.ResumeLayout(false);
            this.splitLower.Panel1.ResumeLayout(false);
            this.splitLower.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLower)).EndInit();
            this.splitLower.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitForm.Panel1.ResumeLayout(false);
            this.splitForm.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitForm)).EndInit();
            this.splitForm.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvList;
        private System.Windows.Forms.DataGridView dgvContents;
        private System.Windows.Forms.DataGridView dgvXbrl;
        private System.Windows.Forms.WebBrowser browser;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel1;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.SplitContainer splitUpper;
        private System.Windows.Forms.SplitContainer splitLower;
        private System.Windows.Forms.DateTimePicker DatePicker;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitForm;
        private System.Windows.Forms.Label LabelVersion;
        private System.Windows.Forms.TextBox TbVersion;
        private System.Windows.Forms.Label LabelMetadata;
        private System.Windows.Forms.ContextMenuStrip MenuToolbar;
        private System.Windows.Forms.TextBox TbCode;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem MenuEdinet;
        private System.Windows.Forms.ToolStripMenuItem MenuSetting;
        private System.Windows.Forms.ComboBox comboFilter;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem MenuBackground;
        private System.Windows.Forms.ToolStripMenuItem MenuPastList;
        private System.Windows.Forms.ToolStripMenuItem MenuDownload;
        private System.Windows.Forms.ToolStripMenuItem MenuImportTaxonomy;
        private System.Windows.Forms.ToolStripStatusLabel ProgressLabel1;
        private System.Windows.Forms.ToolStripProgressBar ProgressBar1;
        private System.Windows.Forms.ToolStripMenuItem MenuIEdinetCodeImport;
    }
}

