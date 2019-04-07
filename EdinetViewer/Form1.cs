using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Disclosures;

namespace EdinetViewer {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private string toppage = "http://disclosure.edinet-fsa.go.jp/";
        private Edinet edinet;
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
            ProgressBar1.Visible = false;
            ProgressLabel1.Visible = false;
            this.Form1_Resize(null, null);
            DatePicker.MinDate = DateTime.Now.Date.AddYears(-5);
            DatePicker.MaxDate = DateTime.Now.AddDays(1);
            DatePicker.Value = DateTime.Now;

            splitMain.Panel1Collapsed = true;
            splitUpper.Panel2Collapsed = true;
            splitLower.Panel2Collapsed = true;
            SettingLoad();
            edinet = new Edinet(setting.Directory, TbVersion.Text);
            if (edinet.Xbrl.Taxonomy.DicTaxonomy.Count == 0) {
                SetTaxonomyDownloadEvent();
            }
            StatusLabel1.Text = "";
            if (browser.Url == null)
                browser.Navigate(toppage);
            this.Text = Application.ProductName + " " + Application.ProductVersion;
            this.Refresh();
            dgvList.DataSource = edinet.DvDocuments;
            dgvContents.DataSource = edinet.DvContents;
            dgvXbrl.DataSource = edinet.TableElements;
            FormatDatagridview();
            DatePicker.Enabled = true;
            DatePicker.CloseUp += DatePicker_CloseUp;
            timer1.Interval = (int)(setting.Interval * 60 * 1000);
            timer1.Enabled = true;
            if (setting.VersionUp) {
                await BackGroundStart(TaskType.VersionUp, Application.ProductVersion);
            }
            //test
            //await BackGroundStart(TaskType.test);
        }


        #region Dgv_Format_Events
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
            dgvContents.Columns[2].Width = 240;
            dgvContents.Columns[3].Width = 200;
            dgvContents.Columns[4].Width = 20;
            dgvContents.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            foreach (var kv in Disclosures.Const.FieldHeader) {
                dgvList.Columns[kv.Key].HeaderText = kv.Value;
            }

            width = new int[] { 30, 70, 45, 40, 74, 120, 45, 25, 45, 90, 68, 68, 100, 140, 60, 60, 60, 80, 80, 80, 18, 18, 18, 24, 24, 24, 24, 68 };
            ordinary = new int[] { 0, 14, 7, 8, 19, 9, 20, 15, 16, 28, 17, 18, 6, 10, 21, 22, 23, 24, 25, 26, 11, 12, 13, 1, 2, 3, 4, 27, 29, 30, 31, 32, 33, 34, 5 };
            for (int i = 0; i < dgvList.ColumnCount; i++) {
                if (i < width.Length)
                    dgvList.Columns[i].Width = width[i];
                if (i < ordinary.Length)
                    dgvList.Columns[i].DisplayIndex = ordinary[i];
                if (i > 22 & i<27)
                    dgvList.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (i == 0 | i == 12)
                    dgvList.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            dgvList.Columns["withdrawalStatus"].ToolTipText = "取下書は\"1\"、取り下げられた書類 は\"2\"、それ以外は\"0\"が出力されま す。\r\n";
            dgvList.Columns["docInfoEditStatus"].ToolTipText = "財務局職員が書類を修正した情報 は\"1\"、修正された書類は\"2\"、それ 以外は\"0\"が出力されます。";
            dgvList.Columns["disclosureStatus"].ToolTipText = "財務局職員によって書類の不開示を 開始した情報は\"1\"、不開示とされて いる書類は\"2\"、財務局職員によっ て書類の不開示を解除した情報は \"3\"、それ以外は\"0\"が出力されま す。";
        }


        private void DgvXbrl_CurrentCellChanged(object sender, EventArgs e) {

        }


        private void DgvList_CellContentClick(object sender, DataGridViewCellEventArgs e) {
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

#region DataGridView_Events_CurrentRowChanged
        private async void DatePicker_CloseUp(object sender, EventArgs e) {
            if (!IsReading) {
                StatusLabel1.Text = "";
                splitMain.Panel1Collapsed = false;
                splitUpper.Panel2Collapsed = false;
                splitLower.Panel2Collapsed = false;
                DateTime target = DatePicker.Value.Date;
                IsReading = true;
                ApiListResult result = await edinet.GetDisclosureList(target);
                StatusLabel1.ForeColor = Color.Black;
                if (result != null) {
                    browser.DocumentText = edinet.ListResult.Source.Replace("\n", "<br>").Replace(" ", "&nbsp;");
                    if (edinet.ListResult.StatusCode != (int)System.Net.HttpStatusCode.OK) {
                        StatusLabel1.ForeColor = Color.Red;
                        StatusLabel1.Text = string.Format("{3:HH:mm:ss} 書類一覧APIエラー {0} {1} {2}", edinet.ListResult.Json.Root.metadata.title, edinet.ListResult.Json.Root.metadata.status, edinet.ListResult.Json.Root.metadata.message, DateTime.Now);
                    } else {
                        LabelMetadata.Text = string.Format("status:{0} {1} count:{2} {3} type:{4}", edinet.ListResult.Json.Root.metadata.message, edinet.ListResult.Json.Root.metadata.processDateTime,
                                edinet.ListResult.Json.Root.metadata.resultset.count, edinet.ListResult.Json.Root.metadata.title, edinet.ListResult.Json.Root.metadata.parameter.type);
                        StatusLabel1.Text = string.Format("{2:HH:mm:ss} 書類一覧API status:{0}[{1}]", edinet.ListResult.StatusCode , edinet.ListResult.StatusText, DateTime.Now);
                    }
                } else {
                    StatusLabel1.Text = DateTime.Now.ToString("HH:mm:ss") + " 書類一覧キャッシュ ";
                    DateTime? process = edinet.Database.GetMetadataProcessDateTime(target);
                    LabelMetadata.Text = string.Format("{0} {1:yyyy-MM-dd} count:{2}", "cache", process ?? target,
                            edinet.TableDocuments.Rows.Count);
                }
                comboFilter.DataSource = edinet.Types;
                currentRow1 = -1;
                IsReading = false;
            }
        }

        private int currentRow1;
        private async void DgvList_CurrentCellChanged(object sender, EventArgs e) {
            if (IsReading)
                return;
            StatusLabel1.Text = "";
            edinet.TableContents.Clear();
            currentRow2 = -1;
            if (!browser.DocumentText.Contains("metadata"))
                browser.DocumentText = "";
            if (!IsReading & dgvList.CurrentCell != null) {
                if (dgvList.CurrentCell.RowIndex != currentRow1)
                    currentRow1 = dgvList.CurrentCell.RowIndex;
                string docid = dgvList.CurrentRow.Cells["docID"].Value.ToString();
                int id = (int)dgvList.CurrentRow.Cells["id"].Value;
                int type = 0;
                if (dgvList.Rows[dgvList.CurrentCell.RowIndex].Cells["xbrlFlag"].Value.ToString() == "1")
                    type = 1;
                else if (dgvList.Rows[dgvList.CurrentCell.RowIndex].Cells["pdfFlag"].Value.ToString() == "1")
                    type = 2;
                else
                    return;
                int year = 20 * 100 + id / 100000000;
                string filepath = null;
                //string name = dgvList.Rows[dgvList.CurrentCell.RowIndex].Cells[type == 2 ? "pdf" : "xbrl"].Value.ToString().Trim();
                bool isApi = await edinet.ChangeDocument(id, docid, type);
                //bool isCache = dgvList.Rows[dgvList.CurrentCell.RowIndex].Cells[type == 2 ? "pdf" : "xbrl"].Value.ToString().Trim() != "";
                if (isApi) {
                    StatusLabel1.Text = string.Format("{1:HH:mm:ss} 書類取得API status[{0}] {2}ダウンロード {3}", edinet.ArchiveResult.StatusCode, DateTime.Now, type == 2 ? "pdf" : "xbrl", edinet.ArchiveResult.Name);
                    filepath = string.Format(@"{0}\Documents\{1}\{2}", setting.Directory, year, edinet.ArchiveResult.Name);
                } else {
                    StatusLabel1.Text = DateTime.Now.ToString("HH:mm:ss") + " ダウンロード済み書類";
                    filepath = edinet.ArchiveResult.Name;
                }
                if (type == 2) {
                    string url = string.Format("file://{0}#toolbar=0&navpanes=0", filepath.Replace("\\", "/"));
                    browser.Navigate(url);
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
                edinet.SelectContent(dgvContents.CurrentCell.RowIndex, out string source);
                string fullpath = dgvContents.Rows[dgvContents.CurrentCell.RowIndex].Cells["fullpath"].Value.ToString();
                if (".png .jpg .jpeg .gif .svg .tif .tiff .esp .pict .bmp".Contains(Path.GetExtension(fullpath).ToLower())) {
                    string filepath = edinet.SaveImage(edinet.ArchiveResult.Buffer, fullpath);
                    browser.Navigate(string.Format("file://{0}", filepath.Replace("\\", "/")));
                } else if (source != null) {
                    browser.DocumentText = source;
                } else {
                    browser.DocumentText = "";
                }
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
            StatusLabel1.Width = statusStrip1.Width - ProgressBar1.Width - ProgressLabel1.Width;
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
        private void Menu_Click(object sender, EventArgs e) {
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
                case "MenuSetting":
                    SettingDialog dialog = new SettingDialog(setting) {
                        Owner = this,
                    };
                    DialogResult result = dialog.ShowDialog();
                    if (result == DialogResult.OK) {
                        if(dialog.Setting.Directory != setting.Directory)
                            edinet.ChangeCacheDirectory(dialog.Setting.Directory);
                        if (dialog.Setting.Interval != setting.Interval)
                            timer1.Interval = (int)(dialog.Setting.Interval * 60 * 1000);
                        setting = dialog.Setting;
                        setting.Save();
                    }
                    break;
            }
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
                if (backgroundTask[1] != null)
                    backgroundCancel[1].Cancel();
                while (backgroundTask[0] != null| backgroundTask[1] != null| backgroundTask[2] != null) {
                    StatusLabel1.Text = "バックグラウンドタスクが終了後にウィンドウが閉じます";
                    statusStrip1.ForeColor = Color.Red;
                    e.Cancel = true;
                    for (int i = 0; i < 3; i++) {
                        if (backgroundTask[i] != null)
                            await backgroundTask[i];
                    }
                }
                if (e.Cancel) Close();
            }
            bool vacuum = false;
            if (DateTime.Now > setting.LastVacuum.AddDays(7)) {
                StatusLabel1.Text = "データーベースVACUUM実行中";
                this.Refresh();
                edinet.Database.Vacuum();
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
                int count = edinet.SearchBrand(code);
                if (count > 0) {
                    LabelMetadata.Text = string.Format("コード{0}　{1}件見つかりました", code, edinet.TableDocuments.Rows.Count);
                    dgvList.DataSource = edinet.DvDocuments;
                    comboFilter.DataSource = edinet.Types;
                    currentRow1 = -1;
                    splitMain.Panel1Collapsed = false;
                    splitUpper.Panel2Collapsed = false;
                    splitLower.Panel2Collapsed = false;
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
            edinet = new Edinet(Path.Combine(setting.Directory, "documents"), TbVersion.Text);
        }





        private void ComboFilter_SelectedIndexChanged(object sender, EventArgs e) {
            ComboBox combo = (ComboBox)sender;
            string filter = " ";
            if (combo.SelectedItem.ToString() != "")
                filter = string.Format("タイプ='{0}'", combo.SelectedItem);
            edinet.DvDocuments.RowFilter = filter;
        }
        private int timercount = 0;
        private void Timer_Tick(object sender, EventArgs e) {
            if (DateTime.Now.AddMinutes(30).Hour < 9)
                return;
            if (DateTime.Now.AddMinutes(45).Hour > 18)
                return;
            bool holiday = false;
            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday | DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                holiday = true;
            if (DateTime.Now.Month == 12 & DateTime.Now.Day > 29)
                holiday = true;
            if (DateTime.Now.Month == 1 & DateTime.Now.Day < 4)
                holiday = true;
            if (setting.Holiday.ContainsKey(DateTime.Now.Date))
                holiday = true;
            //タイマーによる書類一覧取得はフォアグラウンドで実行する
            if(!holiday & setting.Timer){
                timercount++;
                StatusLabel1.Text = string.Format("timer {0} {1:HH:mm:ss}", timercount , DateTime.Now);
                //taskBackground = StartTask(TaskType.List);
                //await taskBackground;
                DatePicker.Value = DateTime.Now.Date;
                this.Refresh();
                DatePicker_CloseUp(null, null);
                StatusLabel1.Text += string.Format(" timer {0} {1:HH:mm:ss}", timercount, DateTime.Now);

            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {

                if (edinet != null)
                    edinet.Dispose();
        }

        private void LabelMetadata_DoubleClick(object sender, EventArgs e) {
            Console.WriteLine("sic {0}", edinet.DicEdinetCode.Count);
            FormatDatagridview();
        }
    }
}
