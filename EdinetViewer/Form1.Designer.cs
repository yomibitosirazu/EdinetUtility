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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.DatePicker = new System.Windows.Forms.DateTimePicker();
            this.panel1 = new System.Windows.Forms.Panel();
            this.MenuToolbar = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuEdinet = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuIDownloadPast = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuImportTaxonomy = new System.Windows.Forms.ToolStripMenuItem();
            this.TbCode = new System.Windows.Forms.TextBox();
            this.LabelVersion = new System.Windows.Forms.Label();
            this.TbVersion = new System.Windows.Forms.TextBox();
            this.LabelMetadata = new System.Windows.Forms.Label();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgvList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvXbrl)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.MenuToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
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
            this.dgvList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvList_CellContentClick);
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
            this.dgvContents.CurrentCellChanged += new System.EventHandler(this.dgvContents_CurrentCellChanged);
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
            this.dgvXbrl.CurrentCellChanged += new System.EventHandler(this.dgvXbrl_CurrentCellChanged);
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
            this.browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.browser_DocumentCompleted);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 579);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(784, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // StatusLabel1
            // 
            this.StatusLabel1.Margin = new System.Windows.Forms.Padding(10, 3, 0, 2);
            this.StatusLabel1.Name = "StatusLabel1";
            this.StatusLabel1.Size = new System.Drawing.Size(19, 17);
            this.StatusLabel1.Text = "    ";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(784, 550);
            this.splitContainer1.SplitterDistance = 181;
            this.splitContainer1.TabIndex = 3;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dgvList);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dgvContents);
            this.splitContainer2.Size = new System.Drawing.Size(784, 181);
            this.splitContainer2.SplitterDistance = 501;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.browser);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.dgvXbrl);
            this.splitContainer3.Size = new System.Drawing.Size(784, 365);
            this.splitContainer3.SplitterDistance = 420;
            this.splitContainer3.TabIndex = 0;
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
            // MenuToolbar
            // 
            this.MenuToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuEdinet,
            this.MenuIDownloadPast,
            this.MenuImportTaxonomy});
            this.MenuToolbar.Name = "contextMenuStrip1";
            this.MenuToolbar.Size = new System.Drawing.Size(211, 70);
            // 
            // MenuEdinet
            // 
            this.MenuEdinet.Name = "MenuEdinet";
            this.MenuEdinet.Size = new System.Drawing.Size(210, 22);
            this.MenuEdinet.Text = "Edinetトップページを表示";
            this.MenuEdinet.Click += new System.EventHandler(this.Menu_Click);
            // 
            // MenuIDownloadPast
            // 
            this.MenuIDownloadPast.Name = "MenuIDownloadPast";
            this.MenuIDownloadPast.Size = new System.Drawing.Size(210, 22);
            this.MenuIDownloadPast.Text = "過去5年間の書類一覧取得";
            this.MenuIDownloadPast.Click += new System.EventHandler(this.Menu_Click);
            // 
            // MenuImportTaxonomy
            // 
            this.MenuImportTaxonomy.Name = "MenuImportTaxonomy";
            this.MenuImportTaxonomy.Size = new System.Drawing.Size(210, 22);
            this.MenuImportTaxonomy.Text = "タクソノミのインポート";
            this.MenuImportTaxonomy.Click += new System.EventHandler(this.Menu_Click);
            // 
            // TbCode
            // 
            this.TbCode.Location = new System.Drawing.Point(163, 4);
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
            this.LabelMetadata.Location = new System.Drawing.Point(205, 8);
            this.LabelMetadata.Name = "LabelMetadata";
            this.LabelMetadata.Size = new System.Drawing.Size(45, 12);
            this.LabelMetadata.TabIndex = 6;
            this.LabelMetadata.Text = "          ";
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer4.Size = new System.Drawing.Size(784, 579);
            this.splitContainer4.SplitterDistance = 25;
            this.splitContainer4.TabIndex = 6;
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 50;
            this.toolTip1.ReshowDelay = 30;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 601);
            this.Controls.Add(this.splitContainer4);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Text = "DisclosureViewer EdinetAPI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.dgvList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContents)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvXbrl)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.MenuToolbar.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
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
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.DateTimePicker DatePicker;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.Label LabelVersion;
        private System.Windows.Forms.TextBox TbVersion;
        private System.Windows.Forms.Label LabelMetadata;
        private System.Windows.Forms.ContextMenuStrip MenuToolbar;
        private System.Windows.Forms.ToolStripMenuItem MenuIDownloadPast;
        private System.Windows.Forms.ToolStripMenuItem MenuImportTaxonomy;
        private System.Windows.Forms.TextBox TbCode;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem MenuEdinet;
    }
}

