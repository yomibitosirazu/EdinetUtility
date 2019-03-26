using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using Disclosures;

namespace EdinetViewer {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private string dirCache;
        private string toppage = "http://disclosure.edinet-fsa.go.jp/";
        private Edinet edinet;
        private FileSystemWatcher fileSystemWatcher;
        private bool IsReading;

        private void Form1_Shown(object sender, EventArgs e) {
            this.Form1_Resize(null, null);
            DatePicker.MinDate = DateTime.Now.Date.AddYears(-5);
            DatePicker.MaxDate = DateTime.Now.AddDays(1);
            DatePicker.Value = DateTime.Now;
            Env env = new Env();
            string key = "DocumentDirectory";
            if (env.Settings.Count == 0 || !env.Settings.ContainsKey(key)) {
                FolderBrowserDialog dlg = new FolderBrowserDialog() {
                    Description = "閲覧した書類をキャッシュとして保存するディレクトリを選択してください。\r\nディレクトリ内にTaxonomy等を保存するデーターベースedinet.sqliteがなければ作成されます。\r\n閲覧書類はディレクトリのdocumentsフォルダにdocidごとに保存されます。"
                };
                if (dlg.ShowDialog() == DialogResult.OK)
                    env.Update(key, dlg.SelectedPath);
                else {
                    this.Close();
                    return;
                }
            }
            dirCache = env.Settings[key];
            key = "version";
            if (!env.Settings.ContainsKey(key))
                env.Update(key, "v1");
            TbVersion.Text = env.Settings["version"];
            if (!Directory.Exists(dirCache)) {
                Directory.CreateDirectory(dirCache);
            }

            splitContainer1.Panel1Collapsed = true;
            splitContainer3.Panel2Collapsed = true;

            edinet = new Edinet(dirCache, TbVersion.Text);
            if (edinet.Xbrl.Taxonomy.DicTaxonomy.Count == 0) {
                browser.DocumentText = "<body><h1>EDINET Disclosure Viewer  <span style=\"font - size:small\">alpha version</span></h1>" + 
                    "EDINETのダウンロードページから" +
                    "<ul><li>最新タクソノミー（00 . 全様式一括  ）</ul>をダウンロードする必要があります。<br>" +
                    "EDINETのトップページを下までスクロールすると【EDINETタクソノミ及びコードリスト 】のリンクがあります。" +
                    "リンクページのEDINETタクソノミの【00 . 全様式一括  】を展開して最新タクソノミを" +
                    string.Format("{0}にダウンロードまたはコピーしてください。",dirCache) +
                    "<br><br>フォルダにダウンロードしたzipアーカイブが作成されると自動的にデータベースを構築します。<br>" +
                    "それまではAPIを利用できますが書類のタクソノミのラベル解析はできません。" +
                    "<br><br>「OK」ボタンをクリックするとEDINETページに移動します。</body>";
                string message = "XBRLの解析にはタクソノミをデータベースにインポートする必要があります。\r\n" +
                    "EDINETのトップページ下方のダウンロードリンク【EDINETタクソノミ及びコードリスト 】から【00 . 全様式一括  】のうち最新タクソノミをダウンロードして" +
                    string.Format("フォルダ{0}にコピーしてください。\r\n",dirCache) +
                    "コピーが完了すると自動的にデータベースを構築しますが数分時間がかかります。" +
                    "それまではAPIを利用できますが書類のタクソノミのラベル解析はできません。";


                MessageBox.Show(message, "はじめに");
                page = Page.First;
                browser.Navigate(toppage);
                fileSystemWatcher = new FileSystemWatcher(dirCache) {
                    NotifyFilter = NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };
                fileSystemWatcher.Created += FileSystemWatcher_Created;
                fileSystemWatcher.Changed += FileSystemWatcher_Created;
            }
            LoadTaxonomy();

            DatePicker.CloseUp += DatePicker_CloseUp;
        }

        private void LoadTaxonomy() {
            StatusLabel1.Text = "";

            DatePicker.Enabled = true;
            //dv = new DataView(edinet.Api.TableDocumentList, "", "seqNumber DESC", DataViewRowState.CurrentRows);
            dgvList.DataSource = edinet.DvDocuments;
            dgvContents.DataSource = edinet.DvContents;
            dgvXbrl.DataSource = edinet.TableElements;
            FormatDatagridview();
            if (browser.Url == null)
                browser.Navigate(toppage);
        }

        //書類一覧APIを呼び出す　カレンダー日付変更時に呼び出される
        private void Request() {
            StatusLabel1.Text = "";
            splitContainer1.Panel1Collapsed = false;
            splitContainer3.Panel2Collapsed = false;
            //FormatDatagridview();
            DateTime target = DatePicker.Value.Date;
            IsReading = true;
            bool access = edinet.GetDisclosureList(target, true);
            browser.DocumentText = edinet.Source.Replace("\n", "<br>").Replace(" ", "&nbsp;");
            StatusLabel1.ForeColor = Color.Black;
            if (access) {
                if (edinet.apiList.StatusCode != System.Net.HttpStatusCode.OK) {
                    StatusLabel1.ForeColor = Color.Red;
                    StatusLabel1.Text = string.Format("{3:HH:mm:ss} 書類一覧APIエラー {0} {1} {2}", edinet.Json.Root.metadata.title, edinet.Json.Root.metadata.status, edinet.Json.Root.metadata.message,DateTime.Now);
                } else {
                    LabelMetadata.Text = string.Format("status:{0} {1} count:{2} {3} type:{4}", edinet.Json.Root.metadata.message, edinet.Json.Root.metadata.processDateTime,
                            edinet.Json.Root.metadata.resultset.count, edinet.Json.Root.metadata.title, edinet.Json.Root.metadata.parameter.type);
                    StatusLabel1.Text = string.Format("{1:HH:mm:ss} 書類一覧API[{0}]", edinet.apiList.StatusCode.ToString(), DateTime.Now);
                }
            } else {
                StatusLabel1.Text = DateTime.Now.ToString("HH:mm:ss") + " 書類一覧キャッシュ ";
                LabelMetadata.Text = string.Format("{0} {1} count:{2} {3} type:{4}", "cache", edinet.Json.Root.metadata.processDateTime,
                        edinet.Json.Root.metadata.resultset.count, edinet.Json.Root.metadata.title, edinet.Json.Root.metadata.parameter.type);
            }
            for (int i = 0; i < dgvList.RowCount; i++) {
                for (int j = 0; j < dgvList.ColumnCount; j++) {
                    string keyForm = edinet.DvDocuments[i]["ordinanceCode"].ToString() + "-" + edinet.DvDocuments[i]["formCode"].ToString();
                    switch (dgvList.Columns[j].Name) {
                        case "formCode":
                            if (Disclosures.Const.FormCode.ContainsKey(keyForm))
                                dgvList.Rows[i].Cells[j].ToolTipText = Disclosures.Const.FormCode[keyForm];
                            break;
                        case "edinetCode":
                            //dgvList.Rows[i].Cells[j].ToolTipText = Disclosures.Const.FormCode
                            break;
                        case "docTypeCode":
                            if (Disclosures.Const.DocTypeCode.ContainsKey(dgvList.Rows[i].Cells[j].ToString()))
                                dgvList.Rows[i].Cells[j].ToolTipText = Disclosures.Const.DocTypeCode[dgvList.Rows[i].Cells[j].ToString()];
                            break;
                    }
                }
            }
            currentRow1 = -1;
            IsReading = false;
        }

        private void ButtonRequest_Click(object sender, EventArgs e) {
            FormatDatagridview();
        }


        #region Dgv_Format_Ecents
        private void FormatDatagridview() {
            dgvXbrl.RowHeadersWidth = 30;
            int[] width = new int[] { 30, 70, 120, 60, 130, 80, 30, 100, 40, 20, 20 };
            int[] ordinary = new int[] { 0, 8, 1, 5, 4, 6, 2, 3, 7, 8, 9, 10, 11 };
            dgvXbrl.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            for (int j = 0; j < edinet.TableElements.Columns.Count; j++) {
                if (j < width.Length)
                    dgvXbrl.Columns[j].Width = width[j];
                dgvXbrl.Columns[j].DisplayIndex = ordinary[j];
            }
            dgvContents.Columns[0].Width = 30;
            dgvContents.Columns[1].Width = 60;
            dgvContents.Columns[2].Width = 100;
            dgvContents.Columns[3].Width = 200;
            foreach (var kv in Disclosures.Const.FieldHeader) {
                dgvList.Columns[kv.Key].HeaderText = kv.Value;
            }

            width = new int[] { 30, 70, 45, 40, 74, 120, 45, 25, 45, 90, 68, 68, 100, 140, 60, 60, 60, 80, 80, 80, 18, 18, 18, 24, 24, 24, 24 };
            ordinary = new int[] { 0, 14, 7, 8, 19, 9, 20, 15, 16, 5, 17, 18, 6, 10, 21, 22, 23, 24, 25, 26, 11, 12, 13, 1, 2, 3, 4 };
            for (int i = 0; i < width.Length; i++) {
                dgvList.Columns[i].Width = width[i];
                dgvList.Columns[i].DisplayIndex = ordinary[i];
                if (i > 22)
                    dgvList.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (i == 0 | i == 12)
                    dgvList.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            dgvList.Columns["withdrawalStatus"].ToolTipText = "取下書は\"1\"、取り下げられた書類 は\"2\"、それ以外は\"0\"が出力されま す。\r\n";
            dgvList.Columns["docInfoEditStatus"].ToolTipText = "財務局職員が書類を修正した情報 は\"1\"、修正された書類は\"2\"、それ 以外は\"0\"が出力されます。";
            dgvList.Columns["disclosureStatus"].ToolTipText = "財務局職員によって書類の不開示を 開始した情報は\"1\"、不開示とされて いる書類は\"2\"、財務局職員によっ て書類の不開示を解除した情報は \"3\"、それ以外は\"0\"が出力されま す。";
        }
        private int currentRow1;
        private void DgvList_CurrentCellChanged(object sender, EventArgs e) {
            StatusLabel1.Text = "";
            edinet.TableContents.Clear();
            currentRow2 = -1;
            if (!browser.DocumentText.Contains("metadata"))
                browser.DocumentText = "";
            if (!IsReading & dgvList.CurrentCell !=null) {
                if (dgvList.CurrentCell.RowIndex != currentRow1)
                    currentRow1 = dgvList.CurrentCell.RowIndex;
                edinet.SelectDocument(dgvList.CurrentCell.RowIndex, out string filepath);
                //string docid = dgvList.Rows[row].Cells["docID"].Value.ToString();
                if (edinet.IsCacheArchive)
                    StatusLabel1.Text = DateTime.Now.ToString("HH:mm:ss") + " ダウンロード済み書類";
                else
                    StatusLabel1.Text = string.Format("{1:HH:mm:ss} 書類取得API[{0}]ダウンロード", edinet.apiDocument.StatusCode.ToString(), DateTime.Now);
                int type = 0;
                if (dgvList.Rows[dgvList.CurrentCell.RowIndex].Cells["xbrlFlag"].Value.ToString() == "1")
                    type = 1;
                else if (dgvList.Rows[dgvList.CurrentCell.RowIndex].Cells["pdfFlag"].Value.ToString() == "1")
                    type = 2;
                //if (type>0) {
                //    bool isCache = edinet.GetDocument(docid, type,DateTime.Parse(dgvList.Rows[row].Cells["submitDateTime"].Value.ToString()).Date, (int)dgvList.Rows[row].Cells["seqNumber"].Value, out string downloadfile);
                //    if (isCache)
                //        StatusLabel1.Text = "キャッシュアーカイブを読み込みました";
                //    else if (edinet.Api.Status != System.Net.HttpStatusCode.OK) 
                //        StatusLabel1.Text = string.Format("APIエラー ", edinet.Json.Root.metadata.title, edinet.Json.Root.metadata.status, edinet.Json.Root.metadata.message);
                //    else
                //        StatusLabel1.Text = string.Format("APIダウンロード成功 {0}", docid);
                //    dgvContents.Rows.Clear();
                //if (type == 1) {
                //    foreach (string filepath in edinet.Api.Files) {
                //        System.IO.FileInfo inf = new System.IO.FileInfo(filepath);
                //        string folda = null;
                //        if (inf.FullName.Contains("PublicDoc")) {
                //            folda = "PublicDoc";
                //        } else if (inf.FullName.Contains("AuditDoc"))
                //            folda = "AuditDoc";
                //        else if (inf.FullName.Contains("Summary")) {
                //            folda = "Summary";
                //        } else if (inf.FullName.Contains("Attachment"))
                //            folda = "Attachment";
                //        dgvContents.Rows.Add(new string[] { inf.Extension, folda, inf.Name, filepath });
                //    }
                //} else {
                if (type == 2 & filepath != null) { 
                        string url = string.Format("file://{0}#toolbar=0&navpanes=0", filepath.Replace("\\","/"));
                        browser.Navigate(url);
                    }
                //}
            }
        }

        private int currentRow2;
        private void dgvContents_CurrentCellChanged(object sender, EventArgs e) {
            if (dgvContents.CurrentCell == null)
                return;
            if (dgvContents.CurrentCell.RowIndex != currentRow2) {
                browser.DocumentText = "";
                currentRow2 = dgvContents.CurrentCell.RowIndex;
                edinet.SelectContent(dgvContents.CurrentCell.RowIndex, out string source);
                string fullpath = dgvContents.Rows[dgvContents.CurrentCell.RowIndex].Cells["fullpath"].Value.ToString();
                if (".png .jpg .jpeg .gif .svg .tif .tiff .esp .pict .bmp".Contains(Path.GetExtension(fullpath).ToLower())) {
                    string filepath = edinet.SaveImage(fullpath);
                    browser.Navigate(string.Format("file://{0}", filepath.Replace("\\", "/")));
                } else if (source != null) { 
                    browser.DocumentText = source;
                } else {
                    browser.DocumentText = "";
                }
            }
        }

        private void dgvXbrl_CurrentCellChanged(object sender, EventArgs e) {

        }


        private void dgvList_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            if ("pdfFlag,attachDocFlag,englishDocFlag".Contains((sender as DataGridView).Columns[e.ColumnIndex].Name)) {
                object value = (sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                if (value != null && value.ToString() == "1") {

                    MessageBox.Show(e.RowIndex.ToString() +
                        "行のボタンがクリックされました。");
                }
            }
        }


        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            DataGridView dgv = (DataGridView)sender;
            switch (dgv.Name) {
                case "dgvList":
                    switch (e.ColumnIndex) {
                        case 23:
                        case 24:
                        case 25:
                        case 26:
                            if (e.Value.ToString() != "1") {
                                e.CellStyle.BackColor = Color.WhiteSmoke;
                                e.CellStyle.ForeColor = Color.WhiteSmoke;
                            }
                            break;
                    }
                    break;
                case "dgvContents":
                    switch (e.ColumnIndex) {
                        case 0:
                            if (e.Value.ToString() == ".xbrl")
                                e.CellStyle.BackColor = Color.Yellow;
                            break;
                        case 1:
                            if (e.Value.ToString() == "AuditDoc")
                                e.CellStyle.BackColor = Color.LightGray;
                            break;
                        case 2:
                            if (e.Value.ToString().ToLower().Contains("ixbrl") & dgvContents.Rows[e.RowIndex].Cells[0].Value.ToString() == ".htm")
                                dgvContents.Rows[e.RowIndex].Cells[0].Style.BackColor = Color.Aqua;
                            break;
                    }
                    break;
                case "dgvXbrl":
                    if(e.ColumnIndex == 7 && decimal.TryParse(e.Value.ToString(), out decimal value)) {
                        e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                    break;
            }
        }
        #endregion

        private void DatePicker_CloseUp(object sender, EventArgs e) {
            if (!IsReading)
                Request();
        }

        private void Form1_Resize(object sender, EventArgs e) {
            int offset = 5;
            splitContainer4.SplitterDistance = 25;
            TbVersion.Left = panel1.Right - TbVersion.Width - offset;
            LabelVersion.Left = TbVersion.Left - LabelVersion.Width;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (process != null && process.Busy)
                process.CancelAsync();
            if (edinet != null)
                edinet.apiDocument.Dispose();
        }

        private enum Page { Default, First, Top, EdinetCode, FundCode, Taxonomy };
        private Page page;
        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            switch (page) {
                case Page.First:
                    browser.Document.Body.All[browser.Document.Body.All.Count - 1].ScrollIntoView(false);
                    page = Page.Default;
                    break;
                case Page.Top:
                    break;
                case Page.EdinetCode:
                    break;
                case Page.FundCode:
                    break;
                case Page.Taxonomy:
                    break;
            }
            if (toppage.Contains(browser.Url.Host))
                MenuEdinet.Text = "Edinetトップページを表示";
        }

        #region BackGroundProcess

        private Edinet.BackgroundProcess process;
        ToolStripProgressBar ProgressBar;
        ToolStripStatusLabel ProgressLabel;
        private void AddProgress() {
            if (ProgressBar == null) {
                ProgressBar = new ToolStripProgressBar();
                ProgressLabel = new ToolStripStatusLabel();
                ProgressLabel.ForeColor = Color.DarkBlue;
                statusStrip1.Items.Insert(0, ProgressLabel);
                statusStrip1.Items.Insert(0, ProgressBar);
                //ProgressBar.ToolTipText = "ステータスラベルをダブルクリックすると非表示";
                ProgressLabel.DoubleClick += (sender, e) => {
                    //ProgressBar.Value = 0;
                    ProgressBar.Visible = !ProgressBar.Visible;
                    //ProgressLabel.ToolTipText = "";
                    //ProgressLabel.Text = "";
                };
            } else {
                ProgressBar.Visible = true;
                ProgressLabel.Visible = true;
            }
        }

        private void Menu_Click(object sender, EventArgs e) {
            switch((sender as ToolStripMenuItem).Name) {
                case "MenuEdinet":
                    if(MenuEdinet.Text == "Edinetトップページを表示") {
                        if (browser.Url.Host == "" | !toppage.Contains(browser.Url.Host))
                            browser.Navigate(toppage);
                        splitContainer1.Panel1Collapsed = true;
                        splitContainer3.Panel2Collapsed = true;
                        (sender as ToolStripMenuItem).Text = "書類一覧テーブルを表示";
                    } else {
                        (sender as ToolStripMenuItem).Text = "Edinetトップページを表示";
                        splitContainer1.Panel1Collapsed = false;
                        splitContainer3.Panel2Collapsed = false;
                    }
                    //if (toppage.Contains(browser.Url.Host)) {
                    //} else {
                    //}
                    break;
                case "MenuIDownloadPast":
                    if (process == null) {
                        AddProgress();
                        //edinet.InitializeBackGround();
                        process = new Edinet.BackgroundProcess(ref edinet, ref ProgressBar, ref ProgressLabel);
                    }
                    if (process.Busy) {
                        process.CancelAsync();
                        (sender as ToolStripMenuItem).Text = "過去5年間の書類一覧取得";
                        //ProgressBar.Visible = false;
                        //ProgressLabel.Visible = false;
                    } else {
                        (sender as ToolStripMenuItem).Text += " 中断";
                        MenuImportTaxonomy.Enabled = false;
                        process.StartAsync(Edinet.BackgroundProcess.ProcessType.PastList, null);
                    }
                    break;
                case "MenuImportTaxonomy":
                    if (process == null) {
                        AddProgress();
                        process = new Edinet.BackgroundProcess(ref edinet, ref ProgressBar, ref ProgressLabel);
                    }
                    if (process.Busy) {
                        process.CancelAsync();
                        ProgressBar.Visible = false;
                        ProgressLabel.Visible = false;
                    } else {
                        using (OpenFileDialog dialog = new OpenFileDialog()) {
                            dialog.InitialDirectory = dirCache;
                            dialog.Multiselect = false;
                            dialog.Filter = "ZIP Files (.ZIP)|*.zip";
                            DialogResult result = dialog.ShowDialog();
                            if (result == DialogResult.OK)
                                process.StartAsync(Edinet.BackgroundProcess.ProcessType.ImportTaxonomy, dialog.FileName);
                        }

                    }
                    break;
            }
        }
        #endregion

        //InvokeRequired
        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e) {
            string[] files = Directory.GetFiles(dirCache, "ALL_*.zip");
            if (files.Length > 0) {
                Array.Reverse(files);
                fileSystemWatcher.EnableRaisingEvents = false;
                fileSystemWatcher.Created -= FileSystemWatcher_Created;
                fileSystemWatcher.Dispose();
                fileSystemWatcher = null;
                ToolStripProgressBar progressBar = new ToolStripProgressBar();
                ToolStripStatusLabel label = new ToolStripStatusLabel();
                AddStatus(progressBar, label);
                if (process == null)
                    process = new Edinet.BackgroundProcess(ref edinet, ref progressBar, ref label);
                process.StartAsync(Edinet.BackgroundProcess.ProcessType.ImportTaxonomy, files[0]);
            }
        }

        #region InvokeRequired

        private void TbCode_TextChanged(object sender, EventArgs e) {
            if (LabelMetadata.Text.Contains("見つかりませんでした"))
                LabelMetadata.Text = "";
            if (TbCode.Text.Length == 4 && int.TryParse(TbCode.Text, out int code)) {
                IsReading = true;
                int count = edinet.SearchBrand(code);
                //edinet.Database.SearchBrand(code, out DataTable table);
                //if (table.Rows.Count > 0) {
                //dgvList.DataSource = new DataView(table, "", "submitDateTime DESC", DataViewRowState.CurrentRows);
                if (count > 0) {
                    LabelMetadata.Text = string.Format("コード{0}　{1}件見つかりました", code, edinet.TableDocuments.Rows.Count);
                    dgvList.DataSource = edinet.DvDocuments;
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer3.Panel2Collapsed = false;
                } else {
                    TbCode.Clear();
                    LabelMetadata.Text = string.Format("コード{0}の銘柄は見つかりませんでした", code);
                }
                IsReading = false;
            } else if (TbCode.TextLength > 4) {
                TbCode.Text = TbCode.Text.Substring(TbCode.Text.Length - 1, 1);
            }
        }

        private void LabelVersion_DoubleClick(object sender, EventArgs e) {
            if (MessageBox.Show("EDINET APIバージョンを変更しますか？\r\nEDINETと一致しない場合不正リクエストになります", "Edit API", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                TbVersion.Enabled = true;
            }
        }

        private void TbVersion_Leave(object sender, EventArgs e) {
            TbVersion.Enabled = false;
            edinet = new Edinet(Path.Combine(dirCache, "documents"), TbVersion.Text);
        }


        //<-- FileSystemWatcherプロセス用のプログレスバー
        delegate void ProgressDeligate(ToolStripProgressBar progressBar, int percentage);
        delegate void LabelDelegate(ToolStripStatusLabel label, string text);
        private void SetProgress(ToolStripProgressBar progressBar, int value) {
            if (InvokeRequired) {
                ProgressDeligate delig = new ProgressDeligate(SetProgress);
                Invoke(delig, new object[] { progressBar, value });
            } else {
                progressBar.Value = value;
            }
        }
        private void SetLabel(ToolStripStatusLabel label, string value) {
            if (InvokeRequired) {
                LabelDelegate delig = new LabelDelegate(SetLabel);
                Invoke(delig, new object[] { value });
            } else
                label.Text = value;
        }

        delegate void StatusDelegate(ToolStripProgressBar progressBar, ToolStripStatusLabel label);
        private void AddStatus(ToolStripProgressBar progressBar, ToolStripStatusLabel label) {
            if (InvokeRequired) {
                StatusDelegate delig = new StatusDelegate(AddStatus);
                Invoke(delig, new object[] { progressBar, label });
            } else {
                //statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { progressBar, label });
                //statusStrip1.Items.Insert(0, progressBar);
                //statusStrip1.Items.Insert(1, label);

                statusStrip1.Items.Add(progressBar);
                statusStrip1.Items.Add(label);
                label.TextChanged += (sender, e) => {
                    if (label.Text.Contains("finish")) {
                        label.Text = "データベースにタクソノミをインポートします";
                        edinet.ImportTaxonomy();
                        label.Text = "タクソノミを構築しました";
                        System.Threading.Thread.Sleep(10000);
                        SetProgress(progressBar, 0);
                        DeleteStatus(progressBar, label);
                    } else {
                        //delegate でProgressBarが動かないので無理やり
                        string[] cols = label.Text.Replace("/", " ").Split(' ');
                        if (int.TryParse(cols[0], out int count) && int.TryParse(cols[1], out int total)) {
                            int percentage = count * 100 / total;
                            SetProgress(progressBar, percentage);
                        }
                    }
                };
            }
        }
        delegate void DeleteDelegate(ToolStripProgressBar progressBar, ToolStripStatusLabel label);
        private void DeleteStatus(ToolStripProgressBar progressBar, ToolStripStatusLabel label) {
            if (InvokeRequired) {
                StatusDelegate delig = new StatusDelegate(DeleteStatus);
                Invoke(delig, new object[] { progressBar, label });
            } else {
                statusStrip1.Items.Remove(progressBar);
                statusStrip1.Items.Remove(label);
                progressBar.Dispose();
                label.Dispose();
            }
        }
        //-->
        #endregion

        private void DataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e) {
            DataGridView dgv = sender as DataGridView;
            Console.WriteLine("{0} row:{1} col:{2}", dgv.Name, e.RowIndex, e.ColumnIndex);
            MessageBox.Show(e.Exception.Message);
        }
    }
}
