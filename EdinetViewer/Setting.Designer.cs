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
            this.MenuDocType = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuCheckAll = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuCheckOffAll = new System.Windows.Forms.ToolStripMenuItem();
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
            this.label2 = new System.Windows.Forms.Label();
            this.checkOrderYear5 = new System.Windows.Forms.CheckBox();
            this.checkOrderList = new System.Windows.Forms.CheckBox();
            this.checkOrderToday = new System.Windows.Forms.CheckBox();
            this.labelUserAgent = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.numericRetry = new System.Windows.Forms.NumericUpDown();
            this.MenuDocType.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.MenuTbHoliday.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericWait1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericWait2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRetry)).BeginInit();
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
            this.buttonOK.Location = new System.Drawing.Point(411, 384);
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
            this.buttonCancel.Location = new System.Drawing.Point(411, 413);
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
            this.label1.Location = new System.Drawing.Point(138, 113);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "自動でダウンロードする書類様式";
            // 
            // checkTimer
            // 
            this.checkTimer.AutoSize = true;
            this.checkTimer.Checked = true;
            this.checkTimer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkTimer.Location = new System.Drawing.Point(12, 50);
            this.checkTimer.Name = "checkTimer";
            this.checkTimer.Size = new System.Drawing.Size(208, 16);
            this.checkTimer.TabIndex = 6;
            this.checkTimer.Text = "現在日のAPIをタイマーで自動リクエスト";
            this.checkTimer.UseVisualStyleBackColor = true;
            // 
            // checkDownload
            // 
            this.checkDownload.AutoSize = true;
            this.checkDownload.Checked = true;
            this.checkDownload.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkDownload.Location = new System.Drawing.Point(37, 93);
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
            this.listDocType.Location = new System.Drawing.Point(135, 130);
            this.listDocType.Name = "listDocType";
            this.listDocType.Size = new System.Drawing.Size(232, 172);
            this.listDocType.TabIndex = 9;
            // 
            // MenuDocType
            // 
            this.MenuDocType.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuCheckAll,
            this.MenuCheckOffAll});
            this.MenuDocType.Name = "MenuDocType";
            this.MenuDocType.Size = new System.Drawing.Size(137, 48);
            // 
            // MenuCheckAll
            // 
            this.MenuCheckAll.Name = "MenuCheckAll";
            this.MenuCheckAll.Size = new System.Drawing.Size(136, 22);
            this.MenuCheckAll.Text = "すべてチェック";
            this.MenuCheckAll.Click += new System.EventHandler(this.MenuCheck_Click);
            // 
            // MenuCheckOffAll
            // 
            this.MenuCheckOffAll.Name = "MenuCheckOffAll";
            this.MenuCheckOffAll.Size = new System.Drawing.Size(136, 22);
            this.MenuCheckOffAll.Text = "すべてオフ";
            this.MenuCheckOffAll.Click += new System.EventHandler(this.MenuCheck_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkEng);
            this.groupBox1.Controls.Add(this.checkAttach);
            this.groupBox1.Controls.Add(this.checkPdf);
            this.groupBox1.Controls.Add(this.checkXbrl);
            this.groupBox1.Location = new System.Drawing.Point(37, 115);
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
            this.checkPdf.Checked = true;
            this.checkPdf.CheckState = System.Windows.Forms.CheckState.Checked;
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
            this.checkXbrl.Checked = true;
            this.checkXbrl.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkXbrl.Location = new System.Drawing.Point(6, 15);
            this.checkXbrl.Name = "checkXbrl";
            this.checkXbrl.Size = new System.Drawing.Size(53, 16);
            this.checkXbrl.TabIndex = 0;
            this.checkXbrl.Text = "XBRL";
            this.checkXbrl.UseVisualStyleBackColor = true;
            // 
            // tbWatching
            // 
            this.tbWatching.Location = new System.Drawing.Point(373, 133);
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
            this.label3.Location = new System.Drawing.Point(35, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(181, 12);
            this.label3.TabIndex = 13;
            this.label3.Text = "平日間隔（8:30 - 17:15）           分";
            // 
            // tbHolidayCsv
            // 
            this.tbHolidayCsv.ContextMenuStrip = this.MenuTbHoliday;
            this.tbHolidayCsv.Location = new System.Drawing.Point(10, 555);
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
            this.linkCabinetCsv.Location = new System.Drawing.Point(10, 619);
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
            this.label4.Location = new System.Drawing.Point(14, 368);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 17;
            this.label4.Text = "祝日一覧";
            // 
            // buttonHolidayConvert
            // 
            this.buttonHolidayConvert.Location = new System.Drawing.Point(118, 526);
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
            this.label5.Location = new System.Drawing.Point(237, 71);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(179, 12);
            this.label5.TabIndex = 22;
            this.label5.Text = "連続リクエストウェイト（秒）  　　　　～";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(305, 526);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(203, 120);
            this.label6.TabIndex = 23;
            this.label6.Text = "1. 左下の内閣府公表のcsvのリンクをクリックすると下のテキストボックスにコピーされる\r\n　または、日付,祝日名形式の日付一覧をコピペしてもよい\r\n2. ↑矢印" +
    "ボタンを押すと日付と認識できる列を祝日一覧にコピーして設定ファイルに保存";
            // 
            // numericInterval
            // 
            this.numericInterval.Location = new System.Drawing.Point(162, 68);
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
            1,
            0,
            0,
            65536});
            this.numericWait1.Location = new System.Drawing.Point(365, 68);
            this.numericWait1.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericWait1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericWait1.Name = "numericWait1";
            this.numericWait1.Size = new System.Drawing.Size(35, 19);
            this.numericWait1.TabIndex = 25;
            this.numericWait1.Value = new decimal(new int[] {
            12,
            0,
            0,
            65536});
            // 
            // numericWait2
            // 
            this.numericWait2.DecimalPlaces = 1;
            this.numericWait2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericWait2.Location = new System.Drawing.Point(415, 69);
            this.numericWait2.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericWait2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericWait2.Name = "numericWait2";
            this.numericWait2.Size = new System.Drawing.Size(35, 19);
            this.numericWait2.TabIndex = 26;
            this.numericWait2.Value = new decimal(new int[] {
            16,
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
            this.listHoliday.Location = new System.Drawing.Point(12, 384);
            this.listHoliday.MultiSelect = false;
            this.listHoliday.Name = "listHoliday";
            this.listHoliday.Size = new System.Drawing.Size(274, 136);
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
            this.label7.Location = new System.Drawing.Point(371, 113);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 29;
            this.label7.Text = "監視銘柄";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 309);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 36);
            this.label2.TabIndex = 30;
            this.label2.Text = "書類の自動ダウンロード順\r\n　　チェックすると日付昇順\r\n　　オフで日付降順";
            // 
            // checkOrderYear5
            // 
            this.checkOrderYear5.AutoSize = true;
            this.checkOrderYear5.Checked = true;
            this.checkOrderYear5.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkOrderYear5.Location = new System.Drawing.Point(161, 331);
            this.checkOrderYear5.Name = "checkOrderYear5";
            this.checkOrderYear5.Size = new System.Drawing.Size(138, 16);
            this.checkOrderYear5.TabIndex = 31;
            this.checkOrderYear5.Text = "過去５年間の書類取得";
            this.checkOrderYear5.UseVisualStyleBackColor = true;
            // 
            // checkOrderList
            // 
            this.checkOrderList.AutoSize = true;
            this.checkOrderList.Location = new System.Drawing.Point(161, 351);
            this.checkOrderList.Name = "checkOrderList";
            this.checkOrderList.Size = new System.Drawing.Size(152, 16);
            this.checkOrderList.TabIndex = 32;
            this.checkOrderList.Text = "リストに表示されている書類";
            this.checkOrderList.UseVisualStyleBackColor = true;
            // 
            // checkOrderToday
            // 
            this.checkOrderToday.AutoSize = true;
            this.checkOrderToday.Location = new System.Drawing.Point(161, 309);
            this.checkOrderToday.Name = "checkOrderToday";
            this.checkOrderToday.Size = new System.Drawing.Size(94, 16);
            this.checkOrderToday.TabIndex = 33;
            this.checkOrderToday.Text = "当日分の書類";
            this.checkOrderToday.UseVisualStyleBackColor = true;
            // 
            // labelUserAgent
            // 
            this.labelUserAgent.AutoSize = true;
            this.labelUserAgent.Location = new System.Drawing.Point(36, 31);
            this.labelUserAgent.Name = "labelUserAgent";
            this.labelUserAgent.Size = new System.Drawing.Size(59, 12);
            this.labelUserAgent.TabIndex = 34;
            this.labelUserAgent.Text = "UserAgent";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(280, 46);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(136, 12);
            this.label8.TabIndex = 35;
            this.label8.Text = "タイムアウト時のリトライ回数";
            // 
            // numericRetry
            // 
            this.numericRetry.Location = new System.Drawing.Point(419, 44);
            this.numericRetry.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericRetry.Name = "numericRetry";
            this.numericRetry.Size = new System.Drawing.Size(28, 19);
            this.numericRetry.TabIndex = 36;
            this.numericRetry.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // SettingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 655);
            this.Controls.Add(this.numericRetry);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.labelUserAgent);
            this.Controls.Add(this.checkOrderToday);
            this.Controls.Add(this.checkOrderList);
            this.Controls.Add(this.checkOrderYear5);
            this.Controls.Add(this.label2);
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
            this.MenuDocType.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.MenuTbHoliday.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericWait1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericWait2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRetry)).EndInit();
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkOrderYear5;
        private System.Windows.Forms.CheckBox checkOrderList;
        private System.Windows.Forms.CheckBox checkOrderToday;
        private System.Windows.Forms.Label labelUserAgent;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericRetry;
    }
}