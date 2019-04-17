namespace Edinet {
    partial class SettingDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.tbDocumentDirectory = new System.Windows.Forms.TextBox();
            this.btnCacheFolda = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.checkTimer = new System.Windows.Forms.CheckBox();
            this.checkDownload = new System.Windows.Forms.CheckBox();
            this.listDocType = new System.Windows.Forms.CheckedListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkEng = new System.Windows.Forms.CheckBox();
            this.checkAttach = new System.Windows.Forms.CheckBox();
            this.checkPdf = new System.Windows.Forms.CheckBox();
            this.checkXbrl = new System.Windows.Forms.CheckBox();
            this.tbWatching = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbHolidayCsv = new System.Windows.Forms.TextBox();
            this.MenuTbHoliday = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuPate = new System.Windows.Forms.ToolStripMenuItem();
            this.linkCabinetCsv = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonHolidayConvert = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numericInterval = new System.Windows.Forms.NumericUpDown();
            this.numericWait1 = new System.Windows.Forms.NumericUpDown();
            this.numericWait2 = new System.Windows.Forms.NumericUpDown();
            this.listHoliday = new System.Windows.Forms.ListView();
            this.date = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label7 = new System.Windows.Forms.Label();
            this.MenuDocType = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuCheckAll = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuCheckOffAll = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.MenuTbHoliday.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericWait1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericWait2)).BeginInit();
            this.MenuDocType.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbDocumentDirectory
            // 
            this.tbDocumentDirectory.Location = new System.Drawing.Point(118, 3);
            this.tbDocumentDirectory.Name = "tbDocumentDirectory";
            this.tbDocumentDirectory.Size = new System.Drawing.Size(342, 19);
            this.tbDocumentDirectory.TabIndex = 1;
            // 
            // btnCacheFolda
            // 
            this.btnCacheFolda.Location = new System.Drawing.Point(2, 1);
            this.btnCacheFolda.Name = "btnCacheFolda";
            this.btnCacheFolda.Size = new System.Drawing.Size(98, 23);
            this.btnCacheFolda.TabIndex = 2;
            this.btnCacheFolda.Text = "キャッシュフォルダ";
            this.btnCacheFolda.UseVisualStyleBackColor = true;
            this.btnCacheFolda.Click += new System.EventHandler(this.Button_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(415, 360);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.Button_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(415, 389);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cnacel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.Button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(138, 95);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "自動でダウンロードする書類様式";
            // 
            // checkTimer
            // 
            this.checkTimer.AutoSize = true;
            this.checkTimer.Location = new System.Drawing.Point(12, 32);
            this.checkTimer.Name = "checkTimer";
            this.checkTimer.Size = new System.Drawing.Size(208, 16);
            this.checkTimer.TabIndex = 6;
            this.checkTimer.Text = "現在日のAPIをタイマーで自動リクエスト";
            this.checkTimer.UseVisualStyleBackColor = true;
            // 
            // checkDownload
            // 
            this.checkDownload.AutoSize = true;
            this.checkDownload.Location = new System.Drawing.Point(37, 75);
            this.checkDownload.Name = "checkDownload";
            this.checkDownload.Size = new System.Drawing.Size(145, 16);
            this.checkDownload.TabIndex = 7;
            this.checkDownload.Text = "書類も同時にダウンロード";
            this.checkDownload.UseVisualStyleBackColor = true;
            // 
            // listDocType
            // 
            this.listDocType.ContextMenuStrip = this.MenuDocType;
            this.listDocType.FormattingEnabled = true;
            this.listDocType.Location = new System.Drawing.Point(135, 112);
            this.listDocType.Name = "listDocType";
            this.listDocType.Size = new System.Drawing.Size(232, 172);
            this.listDocType.TabIndex = 9;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkEng);
            this.groupBox1.Controls.Add(this.checkAttach);
            this.groupBox1.Controls.Add(this.checkPdf);
            this.groupBox1.Controls.Add(this.checkXbrl);
            this.groupBox1.Location = new System.Drawing.Point(37, 97);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(77, 100);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "書類形式";
            // 
            // checkEng
            // 
            this.checkEng.AutoSize = true;
            this.checkEng.Location = new System.Drawing.Point(6, 76);
            this.checkEng.Name = "checkEng";
            this.checkEng.Size = new System.Drawing.Size(48, 16);
            this.checkEng.TabIndex = 3;
            this.checkEng.Text = "英文";
            this.checkEng.UseVisualStyleBackColor = true;
            // 
            // checkAttach
            // 
            this.checkAttach.AutoSize = true;
            this.checkAttach.Location = new System.Drawing.Point(6, 55);
            this.checkAttach.Name = "checkAttach";
            this.checkAttach.Size = new System.Drawing.Size(60, 16);
            this.checkAttach.TabIndex = 2;
            this.checkAttach.Text = "添付等";
            this.checkAttach.UseVisualStyleBackColor = true;
            // 
            // checkPdf
            // 
            this.checkPdf.AutoSize = true;
            this.checkPdf.Location = new System.Drawing.Point(6, 34);
            this.checkPdf.Name = "checkPdf";
            this.checkPdf.Size = new System.Drawing.Size(46, 16);
            this.checkPdf.TabIndex = 1;
            this.checkPdf.Text = "PDF";
            this.checkPdf.UseVisualStyleBackColor = true;
            // 
            // checkXbrl
            // 
            this.checkXbrl.AutoSize = true;
            this.checkXbrl.Location = new System.Drawing.Point(6, 15);
            this.checkXbrl.Name = "checkXbrl";
            this.checkXbrl.Size = new System.Drawing.Size(53, 16);
            this.checkXbrl.TabIndex = 0;
            this.checkXbrl.Text = "XBRL";
            this.checkXbrl.UseVisualStyleBackColor = true;
            // 
            // tbWatching
            // 
            this.tbWatching.Location = new System.Drawing.Point(373, 115);
            this.tbWatching.Multiline = true;
            this.tbWatching.Name = "tbWatching";
            this.tbWatching.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbWatching.Size = new System.Drawing.Size(79, 169);
            this.tbWatching.TabIndex = 11;
            this.toolTip1.SetToolTip(this.tbWatching, "銘柄はすべての書類様式を自動ダウンロード\r\n改行カンマまたはスペース区切り");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(181, 12);
            this.label3.TabIndex = 13;
            this.label3.Text = "平日間隔（8:30 - 17:15）           分";
            // 
            // tbHolidayCsv
            // 
            this.tbHolidayCsv.ContextMenuStrip = this.MenuTbHoliday;
            this.tbHolidayCsv.Location = new System.Drawing.Point(10, 537);
            this.tbHolidayCsv.Multiline = true;
            this.tbHolidayCsv.Name = "tbHolidayCsv";
            this.tbHolidayCsv.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbHolidayCsv.Size = new System.Drawing.Size(289, 60);
            this.tbHolidayCsv.TabIndex = 15;
            // 
            // MenuTbHoliday
            // 
            this.MenuTbHoliday.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuPate});
            this.MenuTbHoliday.Name = "MenuTbHoliday";
            this.MenuTbHoliday.Size = new System.Drawing.Size(116, 26);
            this.MenuTbHoliday.Opening += new System.ComponentModel.CancelEventHandler(this.MenuTbHoliday_Opening);
            // 
            // MenuPate
            // 
            this.MenuPate.Name = "MenuPate";
            this.MenuPate.Size = new System.Drawing.Size(115, 22);
            this.MenuPate.Text = "貼り付け";
            this.MenuPate.Click += new System.EventHandler(this.MenuPate_Click);
            // 
            // linkCabinetCsv
            // 
            this.linkCabinetCsv.AutoSize = true;
            this.linkCabinetCsv.Location = new System.Drawing.Point(10, 611);
            this.linkCabinetCsv.Name = "linkCabinetCsv";
            this.linkCabinetCsv.Size = new System.Drawing.Size(295, 12);
            this.linkCabinetCsv.TabIndex = 16;
            this.linkCabinetCsv.TabStop = true;
            this.linkCabinetCsv.Text = "https://www8.cao.go.jp/chosei/shukujitsu/syukujitsu.csv";
            this.linkCabinetCsv.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkCabinetCsv_LinkClicked);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 287);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 17;
            this.label4.Text = "祝日一覧";
            // 
            // buttonHolidayConvert
            // 
            this.buttonHolidayConvert.Location = new System.Drawing.Point(118, 508);
            this.buttonHolidayConvert.Name = "buttonHolidayConvert";
            this.buttonHolidayConvert.Size = new System.Drawing.Size(30, 23);
            this.buttonHolidayConvert.TabIndex = 19;
            this.buttonHolidayConvert.Text = "↑";
            this.buttonHolidayConvert.UseVisualStyleBackColor = true;
            this.buttonHolidayConvert.Click += new System.EventHandler(this.ButtonHolidayConvert_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(237, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(179, 12);
            this.label5.TabIndex = 22;
            this.label5.Text = "連続リクエストウェイト（秒）  　　　　～";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(305, 508);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(203, 120);
            this.label6.TabIndex = 23;
            this.label6.Text = "1. 左下の内閣府公表のcsvのリンクをクリックすると下のテキストボックスにコピーされる\r\n　または、日付,祝日名形式の日付一覧をコピペしてもよい\r\n2. ↑矢印" +
    "ボタンを押すと日付と認識できる列を祝日一覧にコピーして設定ファイルに保存";
            // 
            // numericInterval
            // 
            this.numericInterval.Location = new System.Drawing.Point(162, 50);
            this.numericInterval.Maximum = new decimal(new int[] {
            480,
            0,
            0,
            0});
            this.numericInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericInterval.Name = "numericInterval";
            this.numericInterval.Size = new System.Drawing.Size(35, 19);
            this.numericInterval.TabIndex = 24;
            this.numericInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericInterval.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // numericWait1
            // 
            this.numericWait1.DecimalPlaces = 1;
            this.numericWait1.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.numericWait1.Location = new System.Drawing.Point(365, 50);
            this.numericWait1.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericWait1.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numericWait1.Name = "numericWait1";
            this.numericWait1.Size = new System.Drawing.Size(35, 19);
            this.numericWait1.TabIndex = 25;
            this.numericWait1.Value = new decimal(new int[] {
            16,
            0,
            0,
            65536});
            // 
            // numericWait2
            // 
            this.numericWait2.DecimalPlaces = 1;
            this.numericWait2.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.numericWait2.Location = new System.Drawing.Point(415, 51);
            this.numericWait2.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericWait2.Minimum = new decimal(new int[] {
            6,
            0,
            0,
            65536});
            this.numericWait2.Name = "numericWait2";
            this.numericWait2.Size = new System.Drawing.Size(35, 19);
            this.numericWait2.TabIndex = 26;
            this.numericWait2.Value = new decimal(new int[] {
            42,
            0,
            0,
            65536});
            // 
            // listHoliday
            // 
            this.listHoliday.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.date,
            this.name});
            this.listHoliday.Cursor = System.Windows.Forms.Cursors.Default;
            this.listHoliday.Location = new System.Drawing.Point(12, 304);
            this.listHoliday.MultiSelect = false;
            this.listHoliday.Name = "listHoliday";
            this.listHoliday.Size = new System.Drawing.Size(274, 198);
            this.listHoliday.TabIndex = 28;
            this.listHoliday.UseCompatibleStateImageBehavior = false;
            this.listHoliday.View = System.Windows.Forms.View.Details;
            // 
            // date
            // 
            this.date.Text = "日付";
            this.date.Width = 92;
            // 
            // name
            // 
            this.name.Text = "祝日";
            this.name.Width = 176;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(371, 95);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 29;
            this.label7.Text = "監視銘柄";
            // 
            // MenuDocType
            // 
            this.MenuDocType.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuCheckAll,
            this.MenuCheckOffAll});
            this.MenuDocType.Name = "MenuDocType";
            this.MenuDocType.Size = new System.Drawing.Size(181, 70);
            // 
            // MenuCheckAll
            // 
            this.MenuCheckAll.Name = "MenuCheckAll";
            this.MenuCheckAll.Size = new System.Drawing.Size(180, 22);
            this.MenuCheckAll.Text = "すべてチェック";
            this.MenuCheckAll.Click += new System.EventHandler(this.MenuCheck_Click);
            // 
            // MenuCheckOffAll
            // 
            this.MenuCheckOffAll.Name = "MenuCheckOffAll";
            this.MenuCheckOffAll.Size = new System.Drawing.Size(180, 22);
            this.MenuCheckOffAll.Text = "すべてオフ";
            this.MenuCheckOffAll.Click += new System.EventHandler(this.MenuCheck_Click);
            // 
            // SettingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 655);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.listHoliday);
            this.Controls.Add(this.numericWait2);
            this.Controls.Add(this.numericWait1);
            this.Controls.Add(this.numericInterval);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.buttonHolidayConvert);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.linkCabinetCsv);
            this.Controls.Add(this.tbHolidayCsv);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbWatching);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listDocType);
            this.Controls.Add(this.checkDownload);
            this.Controls.Add(this.checkTimer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.btnCacheFolda);
            this.Controls.Add(this.tbDocumentDirectory);
            this.Controls.Add(this.label5);
            this.Name = "SettingDialog";
            this.Text = "設定";
            this.Shown += new System.EventHandler(this.DialogSetting_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.MenuTbHoliday.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericWait1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericWait2)).EndInit();
            this.MenuDocType.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox tbDocumentDirectory;
        private System.Windows.Forms.Button btnCacheFolda;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkTimer;
        private System.Windows.Forms.CheckBox checkDownload;
        private System.Windows.Forms.CheckedListBox listDocType;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkEng;
        private System.Windows.Forms.CheckBox checkAttach;
        private System.Windows.Forms.CheckBox checkPdf;
        private System.Windows.Forms.CheckBox checkXbrl;
        private System.Windows.Forms.TextBox tbWatching;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbHolidayCsv;
        private System.Windows.Forms.LinkLabel linkCabinetCsv;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonHolidayConvert;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip MenuTbHoliday;
        private System.Windows.Forms.ToolStripMenuItem MenuPate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericInterval;
        private System.Windows.Forms.NumericUpDown numericWait1;
        private System.Windows.Forms.NumericUpDown numericWait2;
        private System.Windows.Forms.ListView listHoliday;
        private System.Windows.Forms.ColumnHeader date;
        private System.Windows.Forms.ColumnHeader name;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ContextMenuStrip MenuDocType;
        private System.Windows.Forms.ToolStripMenuItem MenuCheckAll;
        private System.Windows.Forms.ToolStripMenuItem MenuCheckOffAll;
    }
}