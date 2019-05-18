using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Reflection;

namespace Edinet {


    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private readonly string toppage = "http://disclosure.edinet-fsa.go.jp/";
        private Disclosures disclosures;
        private FileSystemWatcher fileSystemWatcher;
        private Setting setting;

        private bool IsReading;

        private void SettingLoad() {
            setting = new Setting();
            TbVersion.Text = setting.ApiVersion;
            this.Left = setting.Left;
            this.Top = setting.Top;
            this.Width = setting.Width;
            this.Height = setting.Height;
            this.splitMain.SplitterDistance = setting.MainDistance;
            this.splitUpper.SplitterDistance = setting.UpperDistance;
            this.splitLower.SplitterDistance = setting.LowerDistance;
        }

        private async void Form1_Shown(object sender, EventArgs e) {
            debug.ProgramCodeInfo.SetDebugQueue();
            //<--refer to https://dobon.net/vb/dotnet/control/doublebuffered.html (Copyright(C) DOBON! MIT) から引用しています
            (typeof(DataGridView)).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
               null, dgvList, new object[] { true });
            //-->

            this.Form1_Resize(null, null);
            DatePicker.MinDate = DateTime.Now.Date.AddYears(-5);
            DatePicker.MaxDate = DateTime.Now.AddDays(1);
            DatePicker.Value = DateTime.Now;

            splitMain.Panel1Collapsed = true;
            splitUpper.Panel2Collapsed = true;
            splitLower.Panel2Collapsed = true;
            ProgressBar1.Visible = false;
            ProgressLabel1.Visible = false;
            SettingLoad();
            StatusLabel1.Text = "";
            this.Text = Application.ProductName + " " + Application.ProductVersion;
            if (browser.Url == null)
                browser.Navigate(toppage);
            disclosures = new Disclosures(setting.Directory, setting.UserAgent, TbVersion.Text);
            if (disclosures.Xbrl.Taxonomy.DicTaxonomy.Count == 0) {
                await SetTaxonomyDownloadEvent();
            }
            dgvList.DataSource = disclosures.DvDocuments;
            dgvContents.DataSource = disclosures.DvContents;
            dgvXbrl.DataSource = disclosures.TableElements;
            FormatDatagridview();
            if (setting.VersionUp) {
                    await BackGroundStart(TaskType.VersionUp, Application.ProductVersion + "\t" + setting.VersionPrev);
            }
            this.Refresh();
            DatePicker.Enabled = true;
            DatePicker.CloseUp += DatePicker_CloseUp;
            timer1.Interval = (int)(setting.Interval * 60 * 1000);
            TimerCheck();
            if (setting.Timer) {
                //タイマーが有効であれば起動直後に当日分メタデータを一度取得
                if (TimerCheck())
                    DatePicker_CloseUp(null, null);
            }
            //この前で上のSplitterDistanceが変更されてしまう　原因不明
            this.splitUpper.SplitterDistance = setting.UpperDistance;
            timer1.Enabled = true;
            debug.ProgramCodeInfo.SetDebugQueue();
        }


        #region Dgv_Format_Events

        private void DgvList_DataSourceChanged(object sender, EventArgs e) {
            string[] display = "seqNumber,xbrlFlag,pdfFlag,attachDocFlag,englishDocFlag,code,filerName,docDescription,summary,タイプ,submitDateTime,status,docID,docInfoEditStatus,disclosureStatus,withdrawalStatus,xbrl,pdf,attach,english,formCode,periodStart,periodEnd,currentReportReason,parentDocID,opeDateTime,edinetCode,fundCode,secCode,JCN,issuerEdinetCode,subjectEdinetCode,subsidiaryEdinetCode,id,docTypeCode,date,ordinanceCode".Split(',');
            for (int i = 0; i < display.Length; i++)
                if (dgvList.Columns.Contains(display[i]) && i < dgvList.Columns.Count)
                    dgvList.Columns[display[i]].DisplayIndex = i;
            dgvList.Columns["code"].Width = 38;
            dgvList.Columns["status"].Width = 60;
            comboFilter.Items.Clear();
            comboFilter.Items.Add("");
            for (int i = 0; i < disclosures.DvDocuments.Count; i++) {
                string type = disclosures.DvDocuments[i]["タイプ"].ToString();
                if (!comboFilter.Items.Contains(type))
                    comboFilter.Items.Add(type);
            }
        }

        private void FormatDatagridview() {
            dgvXbrl.RowHeadersWidth = 30;
            int[] width = new int[] { 30, 70, 120, 60, 130, 80, 30, 100, 40, 20, 20 };
            int[] ordinary = new int[] { 0, 8, 1, 5, 4, 6, 2, 3, 7, 8, 9, 10, 11 };
            dgvXbrl.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            for (int j = 0; j < disclosures.TableElements.Columns.Count; j++) {
                if (j < width.Length)
                    dgvXbrl.Columns[j].Width = width[j];
                dgvXbrl.Columns[j].DisplayIndex = ordinary[j];
            }
            dgvContents.Columns[0].Width = 30;
            dgvContents.Columns[1].Width = 60;
            dgvContents.Columns[2].Width = 240;
            dgvContents.Columns[3].Width = 200;
            dgvContents.Columns[4].Width = 20;
            dgvContents.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            foreach (var kv in Const.FieldHeader) {
                dgvList.Columns[kv.Key].HeaderText = kv.Value;
            }
            width = new int[] { 30, 70, 45, 40, 74, 120, 45, 25, 45, 90, 68, 68, 100, 140, 60, 60, 60, 80, 80, 80, 18, 18, 18, 24, 24, 24, 24, 68 };
            for (int i = 0; i < dgvList.ColumnCount; i++) {
                if (i < width.Length)
                    dgvList.Columns[i].Width = width[i];
                if (i > 22 & i<27)
                    dgvList.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (i == 0 | i == 12)
                    dgvList.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            dgvList.Columns["withdrawalStatus"].ToolTipText = "取下書は\"1\"、取り下げられた書類 は\"2\"、それ以外は\"0\"が出力されま す。\r\n";
            dgvList.Columns["docInfoEditStatus"].ToolTipText = "財務局職員が書類を修正した情報 は\"1\"、修正された書類は\"2\"、それ 以外は\"0\"が出力されます。";
            dgvList.Columns["disclosureStatus"].ToolTipText = "財務局職員によって書類の不開示を 開始した情報は\"1\"、不開示とされて いる書類は\"2\"、財務局職員によっ て書類の不開示を解除した情報は \"3\"、それ以外は\"0\"が出力されま す。";
        }



        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            DataGridView dgv = (DataGridView)sender;
            switch (dgv.Name) {
                case "dgvList":
                    //await System.Threading.Tasks.Task.Run(() => {

                        int row = e.RowIndex;
                        int col = e.ColumnIndex;
                    if (e.RowIndex >= disclosures.DvDocuments.Count)
                        return;
                        DateTime target = DateTime.Parse(disclosures.DvDocuments[row]["date"].ToString());
                        string dir = Path.Combine(setting.Directory, "Documents", target.Year.ToString());
                        string docid = disclosures.DvDocuments[row]["docID"].ToString();

                        string[] fields = new string[] { "xbrlFlag", "pdfFlag", "attachDocFlag", "englishDocFlag" };
                        int index = Array.IndexOf(fields, dgvList.Columns[col].Name);
                        if (index > -1) {
                            if (dgvList.Rows[row].Cells[col].Value.ToString() == "1")
                                dgvList.Rows[row].Cells[col].Style.ForeColor = Color.Black;
                            else
                                dgvList.Rows[row].Cells[col].Style.ForeColor = Color.White;
                            string filepath = string.Format(@"{0}\{1}_{2}.{3}", dir, docid, index + 1, index == 1 ? "pdf" : "zip");
                            //check file ex
                            
                            if (File.Exists(filepath)) {
                                dgvList.Rows[row].Cells[col].Style.BackColor = Color.LightCyan;
                            }
                        } else if (dgvList.Columns[col].Name == "docDescription") {
                            if (dgvList.Rows[row].Cells["status"].Value.ToString().Contains("修正")) {
                                dgvList.Rows[row].Cells[col].Style.BackColor = Color.LightYellow;
                                dgvList.Rows[row].Cells["docID"].Style.BackColor = Color.LightYellow;
                                dgvList.Rows[row].Cells["edinetCode"].Style.BackColor = Color.LightYellow;
                                dgvList.Rows[row].Cells["タイプ"].Style.BackColor = Color.LightYellow;
                            }
                            else if (dgvList.Rows[row].Cells["status"].Value.ToString() != "") {
                                dgvList.Rows[row].Cells[col].Style.BackColor = Color.LightGray;
                                dgvList.Rows[row].Cells["docID"].Style.BackColor = Color.LightGray;
                                dgvList.Rows[row].Cells["edinetCode"].Style.BackColor = Color.LightGray;
                                dgvList.Rows[row].Cells["タイプ"].Style.BackColor = Color.LightGray;
                            }
                        }

                    //});
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
#pragma warning disable IDE0059
                    if (e.ColumnIndex == 7 && double.TryParse(e.Value.ToString(), out double value)) {
                        e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
#pragma warning restore IDE0059
                    break;
            }
        }
        #endregion

        #region DataGridView_Events_CurrentRowChanged
        private async void DatePicker_CloseUp(object sender, EventArgs e) {
            debug.ProgramCodeInfo.SetDebugQueue();
            TbCode.Text = "";
            if (comboFilter.SelectedIndex > 0)
                comboFilter.SelectedIndex = 0;
            if (!IsReading) {
                if (splitMain.Panel1Collapsed) {
                    splitMain.Panel1Collapsed = false;
                    splitUpper.Panel2Collapsed = false;
                    splitLower.Panel2Collapsed = false;
                    this.splitUpper.SplitterDistance = setting.UpperDistance;
                }
                DateTime target = DatePicker.Value.Date;
                IsReading = true;
                JsonContent content = await disclosures.ReadDocuments(target, setting.Retry);
                if (content.Exception != null) {
                    StatusLabel1.Text = content.Exception.Message;
                    if (content.Exception.InnerException != null)
                        StatusLabel1.Text += $" {content.Exception.InnerException.Message}";
                    timer1.Enabled = false;
                    checkTimer.Checked = false;
                    checkTimer.BackColor = Control.DefaultBackColor;
                    StatusLabel1.Text.Insert(0, "タイマーオフ");
                    return;
                }
                dgvList.DataSource = disclosures.DvDocuments;
                StatusLabel1.Text = string.Format("{0:HH:mm:ss} List {1}", DateTime.Now, content.OutputMessage);
                if(content.Table != null && content.Table.Rows.Count > 0) {
                    comboFilter.DataSource = disclosures.Types;
                    if (dgvList.Rows.Count > 0)
                        dgvList.Rows[0].Cells[0].Selected = true;
                }
                currentRow1 = -1;
                IsReading = false;
                if (content.Exception == null && content.Metadata != null && content.StatusCode != null) {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("<body><h1>{0}</h1>", content.Metadata.Parameter.Date);
                    sb.AppendFormat("<table><tr><td>StatusCode</td><td>{0}[{1}]</td></tr>", content.StatusCode.Status, content.StatusCode.Message);
                    sb.AppendFormat("<tr><td>Title</td><td>{0}</td></tr>", content.Metadata.Title);
                    sb.AppendFormat("<tr><td>Type</td><td>{0}</td></tr>", content.Metadata.Parameter.Type);
                    sb.AppendFormat("<tr><td>ProcessDateTime</td><td>{0}</td></tr>", content.Metadata.ProcessDateTime);
                    sb.AppendFormat("<tr><td>Count</td><td>{0}</td></tr></table></body>", content.Metadata.Resultset.Count);
                    browser.DocumentText = sb.ToString();
                    LabelMetadata.Text = content.OutputMessage + (sender == null ? " on timer" : "");
                    //dgvList.Refresh();
                    if (content.Metadata.Resultset.Count > 0) {
                        //timerの場合続いて書類のダウンロード
                        if (sender == null && setting.Download && (setting.Xbrl | setting.Pdf | setting.Attach | setting.English) & setting.DocumentTypes.Length > 0) {
                            await BackGroundStart(TaskType.TodayArchive);
                        } else if (sender == null) {
                            StatusLabel1.Text = "書類の自動ダウンロードはオフです";
                        } else if (setting.Watching != null && setting.Watching.Length > 0) {
                            //自動ダウンロードオフであっても監視銘柄はタイマーオンオフにかかわらずすべてダウンロードする 
                        }
                        await UpdateSummaryAsync();
                    }
                }

            }
            StatusLabel1.Text = "";

        }


        private void DgvXbrl_CurrentCellChanged(object sender, EventArgs e) {

        }


        private async void DgvList_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            if ("pdfFlag,attachDocFlag,englishDocFlag".Contains((sender as DataGridView).Columns[e.ColumnIndex].Name)) {
                object value = (sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                if (value != null && value.ToString() == "1") {
                    DateTime date = DateTime.Parse(dgvList.Rows[e.RowIndex].Cells["date"].Value.ToString());
                    int id = int.Parse(disclosures.DvDocuments[e.RowIndex]["id"].ToString());
                    string docid = disclosures.DvDocuments[e.RowIndex]["docID"].ToString();
                    string[] fields = new string[] { "xbrlFlag", "pdfFlag", "attachDocFlag", "englishDocFlag" };
                    int index = Array.IndexOf(fields, dgvList.Columns[e.ColumnIndex].Name);
                    string filepath = string.Format(@"{0}\Documents\{1}\{2}_{3}.{4}", setting.Directory, date.Year, docid, index + 1, index == 1 ? "pdf" : "zip");
                    string field = fields[index].Replace("Doc", "").Replace("Flag", "");
                    string download = dgvList.CurrentRow.Cells[field].Value.ToString();
                    if (download == "404") {
                        StatusLabel1.Text = string.Format("{0:HH:mm:ss} {1}[{2}] 404[Not Found] in table", DateTime.Now, docid, field);
                        return;
                    }
                    //int retry = 1;
                    ArchiveResponse response = await disclosures.ChangeDocumentAsync(id, docid, (RequestDocument.DocumentType)Enum.ToObject(typeof(RequestDocument.DocumentType), index + 1), setting.Retry);
                    dgvList.Refresh();
                    if (response != null) {
                        StatusLabel1.Text = string.Format("{1:HH:mm:ss} 書類取得API status[{0}] {2}ダウンロード {3}", response.EdinetStatusCode.Message, DateTime.Now, index == 1 ? "pdf" : "xbrl", response.Filename);
                        //filepath = string.Format(@"{0}\Documents\{1}\{2}", setting.Directory, year, disclosures.ArchiveResult.Name);
                    } else {
                        StatusLabel1.Text = DateTime.Now.ToString("HH:mm:ss") + " ダウンロード済み書類 ";
                        //filepath = disclosures.ArchiveResult.Name;
                    }
                    //MessageBox.Show(e.RowIndex.ToString() + "行 " + e.ColumnIndex + "列 " + dgvList.Columns[e.ColumnIndex].Name);
                    if (dgvList.Columns[e.ColumnIndex].Name == "pdfFlag") {
                        if (File.Exists(filepath)) {
                            string url = string.Format("file://{0}#toolbar=0&navpanes=0", filepath.Replace("\\", "/"));
                            browser.Navigate(url);
                        }
                    } else {
                    }
                }
            }
        }

        private int currentRow1;
        private async void DgvList_CurrentCellChanged(object sender, EventArgs e) {
            if (dgvList.CurrentCell == null || IsReading | disclosures.DvDocuments.Count == 0 || disclosures.DvDocuments[dgvList.CurrentCell.RowIndex]["id"].ToString() == "")
                return;
            StatusLabel1.Text = "";
            disclosures.TableContents.Clear();
            currentRow2 = -1;
            if (!browser.DocumentText.Contains("metadata"))
                browser.DocumentText = "";
            if (!IsReading & dgvList.CurrentCell != null) {
                if (dgvList.CurrentCell.RowIndex != currentRow1) {
                    currentRow1 = dgvList.CurrentCell.RowIndex;
                    if(disclosures.DvContents.Count > 0)
                        disclosures.DvContents.Table.Rows.Clear(); ;
                    if (disclosures.TableElements.Rows.Count > 0)
                        disclosures.TableElements.Rows.Clear(); ;
                }
                string docid = dgvList.CurrentRow.Cells["docID"].Value.ToString();
                int id = int.Parse(dgvList.CurrentRow.Cells["id"].Value.ToString());
                Nullable< RequestDocument.DocumentType> type =  null;
                if (dgvList.Rows[dgvList.CurrentCell.RowIndex].Cells["xbrlFlag"].Value.ToString() == "1")
                    type =  RequestDocument.DocumentType.Xbrl;
                else if (dgvList.Rows[dgvList.CurrentCell.RowIndex].Cells["pdfFlag"].Value.ToString() == "1")
                    type =  RequestDocument.DocumentType.Pdf;
                if (type != null) {
                    int year = 20 * 100 + id / 100000000;
                    //string filepath = null;
                    string download = dgvList.CurrentRow.Cells[type ==  RequestDocument.DocumentType.Xbrl ? "xbrl" : "pdf"].Value.ToString();
                    if (download == "404") {
                        StatusLabel1.Text = string.Format("{0:HH:mm:ss} {1}[{2}] 404[Not Found] in table", DateTime.Now, docid, type ==  RequestDocument.DocumentType.Xbrl ? "xbrl" : "pdf");
                        return;
                    }

                    //int retry = 1;
                    ArchiveResponse response = await disclosures.ChangeDocumentAsync(id, docid,  type.Value, setting.Retry);
                    dgvContents.DataSource = disclosures.DvContents;
                    dgvList.Refresh();
                    if(response == null) {
                        StatusLabel1.Text = DateTime.Now.ToString("HH:mm:ss") + " ダウンロード済み書類";
                    } else if (response.Exception != null) {
                        StatusLabel1.Text = string.Format("{0:HH:mm:ss} ダウンロードできません {1}", DateTime.Now, response.Exception.Message + response.Exception.InnerException != null ? response.Exception.InnerException.Message : "");
                        return;
                    }
                    if (response != null) {
                        if(response.EdinetStatusCode!= null)
                        StatusLabel1.Text = string.Format("{1:HH:mm:ss} 書類取得API status[{0}] {2}ダウンロード {3}", response.EdinetStatusCode.Status, DateTime.Now, type ==  RequestDocument.DocumentType.Pdf ? "pdf" : "xbrl", response.Filename);
                        else
                            StatusLabel1.Text = string.Format("{1:HH:mm:ss} 書類取得API status[{0}] {2}ダウンロード {3}", response.HttpStatusCode.ToString(), DateTime.Now, type == RequestDocument.DocumentType.Pdf ? "pdf" : "xbrl", response.Filename);
                        if (dgvList.CurrentRow.Cells["docTypeCode"].Value.ToString() == "350" | dgvList.CurrentRow.Cells["docTypeCode"].Value.ToString() == "360") {
                            if (dgvList.CurrentRow.Cells["summary"].Value.ToString() == "") {
                                await disclosures.UpdateSummary(id, true);
                                dgvList.Refresh();
                            }
                        }
                    } else {
                        //filepath = disclosures.ArchiveResult.Name;
                    }
                    if (type ==  RequestDocument.DocumentType.Pdf) {
                        string filepath = string.Format(@"{0}\Documents\{1}\{2}_2.pdf", setting.Directory, year, docid);
                        if (File.Exists(filepath)) {
                            string url = string.Format("file://{0}#toolbar=0&navpanes=0", filepath.Replace("\\", "/"));
                            browser.Navigate(url);
                        }
                        //browser.DocumentStream = new MemoryStream(response.Buffer);

                    }
                }
            }
        }

        private int currentRow2;
        private void DgvContents_CurrentCellChanged(object sender, EventArgs e) {
            if (dgvContents.CurrentCell == null)
                return;
            if (dgvContents.CurrentCell.RowIndex != currentRow2) {
                browser.DocumentText = "";
                currentRow2 = dgvContents.CurrentCell.RowIndex;

                disclosures.SelectContent(dgvContents.CurrentCell.RowIndex, out string source);
                dgvXbrl.DataSource = disclosures.TableElements;
                string fullpath = dgvContents.Rows[dgvContents.CurrentCell.RowIndex].Cells["fullpath"].Value.ToString();
                if (".png .jpg .jpeg .gif .svg .tif .tiff .esp .pict .bmp".Contains(Path.GetExtension(fullpath).ToLower())) {
                    browser.Navigate(string.Format("file://{0}", source.Replace("\\", "/")));
                } else if (Path.GetExtension(fullpath) == ".pdf") {
                    browser.Navigate(string.Format("file://{0}#toolbar=0&navpanes=0", source.Replace("\\", "/")));
                } else if (source != null) {
                    browser.DocumentText = source;
                }

            }
        }



        private void DgvXbrl_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (dgvXbrl.Columns[e.ColumnIndex].Name == "value") {
                string source = dgvXbrl.Rows[e.RowIndex].Cells["value"].Value.ToString();
                browser.DocumentText = source;
            }
        }
        private void DataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e) {
            DataGridView dgv = sender as DataGridView;
            Console.WriteLine("{0} row:{1} col:{2}", dgv.Name, e.RowIndex, e.ColumnIndex);
            MessageBox.Show(e.Exception.Message);
        }
        #endregion

        private void Form1_Resize(object sender, EventArgs e) {
            int offset = 5;
            splitForm.SplitterDistance = 25;
            TbVersion.Left = panel1.Right - TbVersion.Width - offset;
            LabelVersion.Left = TbVersion.Left - LabelVersion.Width;
            checkTimer.Left = LabelVersion.Left - checkTimer.Width - 5;
            ProgressLabel1.Width = 350;
            StatusLabel1.Width = this.Width - ProgressBar1.Width - ProgressLabel1.Width - 36;
        }

        private enum Page { Default, First, Top, EdinetCode, FundCode, Taxonomy };
        private Page page;
        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
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



        //フォアグラウンドメニュー
        private async void Menu_Click(object sender, EventArgs e) {
            switch ((sender as ToolStripMenuItem).Name) {
                case "MenuEdinet":
                    if (MenuEdinet.Text == "Edinetトップページを表示") {
                        if (browser.Url.Host == "" | !toppage.Contains(browser.Url.Host))
                            browser.Navigate(toppage);
                        splitMain.Panel1Collapsed = true;
                        splitUpper.Panel2Collapsed = true;
                        splitLower.Panel2Collapsed = true;
                        (sender as ToolStripMenuItem).Text = "書類一覧テーブルを表示";
                    } else {
                        (sender as ToolStripMenuItem).Text = "Edinetトップページを表示";
                        splitMain.Panel1Collapsed = false;
                        splitUpper.Panel2Collapsed = false;
                        splitLower.Panel2Collapsed = false;
                    }
                    break;
                case "MenuSetting": {
                        SettingDialog dialog = new SettingDialog(setting) {
                            Owner = this,
                        };
                        string dir = setting.Directory;
                        DialogResult result = dialog.ShowDialog();
                        if (result == DialogResult.OK) {
                            timer1.Enabled = setting.Timer;
                            timer1.Interval = (int)(setting.Interval * 1000 * 60);
                            TimerCheck();
                            if (dir != setting.Directory) {
                                string dbfile = $"{dialog.Setting.Directory}\\edinet.db";
                                if (!File.Exists(dbfile)) {
                                    await disclosures.Database.TaxonomyTableToMemoryAsync(this.InvokeProgress);
                                    //bool exists = disclosures.ChangeCacheDirectory(dialog.Setting.Directory);
                                    //if (!exists) {
                                    //    string[] files = Directory.GetFiles(dir, "ALL_*.zip");
                                    //    if (files.Length > 0) {
                                    //        Array.Reverse(files);
                                    //        File.Copy(files[0], Path.Combine(setting.Directory, Path.GetFileName(files[0])));
                                    //    }
                                    //    await SetTaxonomyDownloadEvent();
                                    //}
                                    disclosures.Database.ChangeDirectory(dbfile);
                                    disclosures.Database.MemoryToDatabase($"{dialog.Setting.Directory}\\edinet.db");
                                }
                            }
                            setting = dialog.Setting;
                            setting.Save();
                            disclosures.ChangeCacheDirectory(setting.Directory);
                        }
                    }
                    break;
                case "MenuApiHistory":
                    string logfile = Path.Combine(setting.Directory, "EdinetApi.log");
                    if (!File.Exists(logfile)) {
                        StatusLabel1.Text = "ログファイルがありません " + logfile;
                        return;
                    }
                    string[] lines = File.ReadAllLines(logfile); 
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<body><table>");
                    for(int i=lines.Length - 1; i >= 0; i--) {
                        string[] cols = lines[i].Replace("DocumentListAPI:", "DocumentListAPI status:").Split();
                        sb.AppendFormat("<tr><td nowrap>{0} {1}</td>", cols[0], cols[1]);
                        sb.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}</td></tr>", cols[2].Replace("Document", ""), cols[3], string.Join(" ", cols, 4, cols.Length - 4));
                    }
                    sb.Append("</table></body>");
                    splitMain.Panel1Collapsed = true;
                    splitUpper.Panel2Collapsed = true;
                    splitLower.Panel2Collapsed = true;
                    browser.DocumentText = sb.ToString();
                    break;
                case "MenuSearch":
                    using (EdinetViewer.DialogSearch search = new EdinetViewer.DialogSearch(disclosures.Database)) {
                        DialogResult dialogResult = search.ShowDialog();
                        if(dialogResult == DialogResult.OK) {
                            if (search.Table != null && search.Table.Rows.Count > 0 & search.Table.Rows[0]["id"].ToString() != "0") {
                                disclosures.SetDocumentTable(search.Table);
                                comboFilter.DataSource = disclosures.Types;
                                currentRow1 = -1;
                                if (splitMain.Panel1Collapsed) {
                                    splitMain.Panel1Collapsed = false;
                                    splitUpper.Panel2Collapsed = false;
                                    splitLower.Panel2Collapsed = false;
                                    this.splitUpper.SplitterDistance = setting.UpperDistance;
                                }
                                dgvList.DataSource = disclosures.DvDocuments;
                            }

                            search.Close();
                        }
                    }
                        break;
                case "MenuNotFinalList":
                    Dictionary<DateTime, int> dicMetadata = disclosures.Database.GetFinalMetalist();
                    List<DateTime> list = new List<DateTime>();
                    DateTime date = DateTime.Now.Date;
                    do {
                        date = date.AddDays(-1);
                        if (!dicMetadata.ContainsKey(date)) {
                            list.Add(date);
                        }
                    } while (date >= DateTime.Now.AddYears(-5).Date);
                    list.Sort();
                    list.Reverse();
                    StringBuilder sb1 = new StringBuilder();
                    foreach(DateTime target in list) {
                        sb1.AppendLine($"{target:yyyy-MM-dd ddd}");
                    }
                    if (sb1.Length == 0)
                        sb1.Append("未確定の日付は見つかりませんでした");
                    MessageBox.Show(sb1.ToString(), "メタデータ非取得または未確定の日付一覧");
                    break;
                case "MenuImprotDownloaded": {
                        using (OpenFileDialog dialog = new OpenFileDialog()) {
                            dialog.InitialDirectory = setting.Directory;
                            dialog.Multiselect = false;
                            dialog.Filter = "ZIP Files (.ZIP)|*.zip|検索結果csv|XbrlSearchDlInfo.csv";
                            DialogResult result = dialog.ShowDialog();
                            if (result == DialogResult.OK) {
                                //Archive.Downloaded downloaded = new Archive.Downloaded(dialog.FileName);
                                //downloaded.Import(disclosures.Database);
                                Dictionary<string, DateTime> dic = disclosures.ImportArchives(dialog.FileName);
                                if (dic.Count > 0)
                                    await disclosures.ReadMetadataAndUpdateDownloaded(dic, setting);
                            }
                        }
                    }
                    break;
            }
        }



        private void MenuShowBrowser_Click(object sender, EventArgs e) {
            string source = dgvXbrl.Rows[dgvXbrl.CurrentRow.Index].Cells["value"].Value.ToString();
            browser.DocumentText = source;

        }

        #region Task
        private async void MenuBackground_Click(object sender, EventArgs e) {
            switch ((sender as ToolStripMenuItem).Name) {
                case "MenuImportTaxonomy":
                    using (OpenFileDialog dialog = new OpenFileDialog()) {
                        dialog.InitialDirectory = setting.Directory;
                        dialog.Multiselect = false;
                        dialog.Filter = "ZIP Files (.ZIP)|*.zip";
                        DialogResult result = dialog.ShowDialog();
                        if (result == DialogResult.OK) {
                            await BackGroundStart(TaskType.Taxonomy, dialog.FileName);
                        }
                    }
                    break;
                case "MenuIEdinetCodeImport":
                    using (OpenFileDialog dialog = new OpenFileDialog()) {
                        dialog.InitialDirectory = setting.Directory;
                        dialog.Multiselect = false;
                        dialog.Filter = "ZIP Files (.ZIP)|*.zip";
                        DialogResult result = dialog.ShowDialog();
                        if (result == DialogResult.OK) {
                            await BackGroundStart(TaskType.EdinetCode, dialog.FileName);
                        }
                    }

                    break;
            }
        }

        //バックグラウンドメニュー

        private async void MenuCheckBackground_Click(object sender, EventArgs e) {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if (menu == MenuPastList) {
                if ((sender as ToolStripMenuItem).Checked) {
                    if (backgroundTask == null || backgroundTask[0] == null) {
                        await BackGroundStart(TaskType.List);
                    }
                } else {
                    backgroundCancel[0].Cancel();
                }

            } else if (menu == MenuDownload) {
                if ((sender as ToolStripMenuItem).Checked) {
                    if (!(setting.Xbrl | setting.Pdf | setting.Attach | setting.English) | setting.DocumentTypes.Length == 0) {
                        MessageBox.Show("設定ダイアログを表示して、「書類形式」と「自動でダウンロードする書類様式」を選択して再度メニューをチェックしてください");
                        menu.Checked = false;
                        return;
                    }
                    if (backgroundTask == null || backgroundTask[1] == null) {
                        ProgressBar1.Visible = true;
                        ProgressLabel1.Visible = true;
                        ProgressLabel1.Text = "バックグラウンドダウンロード準備中";
                        this.Refresh();
                        await BackGroundStart(TaskType.Archive);
                    }
                } else {
                    backgroundCancel[1].Cancel();
                }

            }
            else if(menu == MenuDownloadDgvList) {
                if ((sender as ToolStripMenuItem).Checked) {
                    if (backgroundTask == null || backgroundTask[1] == null) {
                        await BackGroundStart(TaskType.DgvArchive);
                    }
                } else {
                    backgroundCancel[1].Cancel();
                }
            } else if (menu == MenuSummary) {
                if ((sender as ToolStripMenuItem).Checked) {
                    if (backgroundTask == null || backgroundTask[1] == null) {
                        await BackGroundStart(TaskType.Summary);
                    }
                } else {
                    backgroundCancel[1].Cancel();
                }
            }
        }

        private async void MenuBackground_CheckedChanged(object sender, EventArgs e) {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if (menu == MenuPastList) {
                if ((sender as ToolStripMenuItem).Checked) {
                    if (backgroundTask == null || backgroundTask[0]==null) {
                        await BackGroundStart(TaskType.List);
                    }
                } else {
                    backgroundCancel[0].Cancel();
                }

            } else if (menu == MenuDownload) {
                if ((sender as ToolStripMenuItem).Checked) {
                    if (backgroundTask == null || backgroundTask[1] == null) {
                        ProgressBar1.Visible = true;
                        ProgressLabel1.Visible = true;
                        ProgressLabel1.Text = "バックグラウンドダウンロード準備中";
                        this.Refresh();
                        await BackGroundStart(TaskType.Archive);
                    }
                } else {
                    backgroundCancel[1].Cancel();
                }

            }
        }

        //refer to http://bbs.wankuma.com/index.cgi?mode=al2&namber=85389&KLOG=146
        private async void Form1_FormClosing(object sender, FormClosingEventArgs e) {

            if (backgroundTask != null) {
                bool[] flag = new bool[] { false, false, true };
                for (int i = 0; i < flag.Length; i++) {
                    if (flag[i]) {
                        while (backgroundTask[i] != null) {
                            StatusLabel1.Text = "バックグラウンドタスクが終了後にウィンドウが閉じます";
                            statusStrip1.ForeColor = Color.Red;
                            e.Cancel = true;
                            await backgroundTask[i];
                        }
                    } else
                        if (backgroundCancel[i] != null)
                        backgroundCancel[i].Cancel();
                }
                if (e.Cancel) Close();
            }
            bool vacuum = false;
            Console.WriteLine("last vacuum {0}", setting.LastVacuum);
            if (DateTime.Now > setting.LastVacuum.AddDays(7)) {
                StatusLabel1.Text = "データーベースVACUUM実行中";
                this.Refresh();
                disclosures.Database.Vacuum();
                StatusLabel1.Text = "";
                vacuum = true;
            }
            setting.Save(this.Left, this.Top, this.Width, this.Height, 
                this.splitMain.SplitterDistance, this.splitUpper.SplitterDistance, 
                this.splitLower.SplitterDistance, vacuum);

        }


        #endregion




        private void TbCode_TextChanged(object sender, EventArgs e) {
            if (LabelMetadata.Text.Contains("見つかりませんでした"))
                LabelMetadata.Text = "";
            if (TbCode.Text.Length == 4 && int.TryParse(TbCode.Text, out int code)) {
                IsReading = true;
                int count = disclosures.SearchBrand(code);
                if (count > 0) {
                    LabelMetadata.Text = string.Format("コード{0}　{1}件見つかりました", code, disclosures.TableDocuments.Rows.Count);
                    dgvList.DataSource = disclosures.DvDocuments;
                    comboFilter.DataSource = disclosures.Types;
                    currentRow1 = -1;
                    if (splitMain.Panel1Collapsed) {
                        splitMain.Panel1Collapsed = false;
                        splitUpper.Panel2Collapsed = false;
                        splitLower.Panel2Collapsed = false;
                        this.splitUpper.SplitterDistance = setting.UpperDistance;

                    }
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
            disclosures = new Disclosures(setting.Directory, setting.UserAgent, TbVersion.Text);
        }





        private void ComboFilter_SelectedIndexChanged(object sender, EventArgs e) {
            ComboBox combo = (ComboBox)sender;
            string filter = " ";
            if (combo.SelectedItem.ToString() != "")
                filter = string.Format("タイプ='{0}'", combo.SelectedItem);
            disclosures.DvDocuments.RowFilter = filter;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {

                if (disclosures != null)
                    disclosures.Dispose();
        }

        private void LabelMetadata_DoubleClick(object sender, EventArgs e) {
            Console.WriteLine("sic {0}", disclosures.DicEdinetCode.Count);
            FormatDatagridview();
        }


        private int timercount = 0;
        private void Timer_Tick(object sender, EventArgs e) {
            //タイマーによる書類一覧取得はフォアグラウンドで実行する
            if (TimerCheck()) {
                timercount++;
                StatusLabel1.Text = string.Format("timer tick {0} {1:HH:mm:ss}", timercount, DateTime.Now);
                Console.WriteLine("timer {0} {1:HH:mm:ss}", timercount, DateTime.Now);
                DatePicker.Value = DateTime.Now.Date;
                this.Refresh();
                //この中で終了後にバックグラウンドで書類アーカイブのダウンロード
                    DatePicker_CloseUp(null, null);
            }
        }

        private void CheckTimer_Click(object sender, EventArgs e) {
            setting.Values["Timer"] = checkTimer.Checked.ToString();
            TimerCheck();
        }
        private bool TimerCheck() {
            //bool enable = setting.Timer;
            if (setting.Timer) {
                checkTimer.BackColor = Color.Yellow;
                checkTimer.Text = string.Format("{0} min", setting.Interval);
            } else {
                checkTimer.BackColor = Control.DefaultBackColor;
                checkTimer.Text = "Off";
            }

            if ((DateTime.Now.DayOfWeek == DayOfWeek.Sunday | DateTime.Now.DayOfWeek == DayOfWeek.Saturday) |
                    (DateTime.Now.Month == 12 & DateTime.Now.Day > 29) |
                    (DateTime.Now.Month == 1 & DateTime.Now.Day < 4) |
                    (setting.Holiday.ContainsKey(DateTime.Now.Date))) {
                toolTip1.SetToolTip(checkTimer, "市場は休みです");
                checkTimer.BackColor = Control.DefaultBackColor;
                return false;
            } else if (DateTime.Now.AddMinutes(30).Hour < 9 | DateTime.Now.AddMinutes(-15).Hour >= 17) {
                toolTip1.SetToolTip(checkTimer, "時間外です");
                checkTimer.BackColor = Control.DefaultBackColor;
                return false;
            }

            return setting.Timer;

        }


        private void DgvList_DoubleClick(object sender, EventArgs e) {

            int type = int.Parse(dgvList.Rows[dgvList.CurrentCell.RowIndex].Cells["docTypeCode"].Value.ToString());
            if(type >= 120 & type <= 150) {
                int id = int.Parse(dgvList.Rows[dgvList.CurrentCell.RowIndex].Cells["id"].Value.ToString());
                disclosures.UpdateQuarter(id);
                //browser.DocumentText = summary;
            }
        }
    }
}
