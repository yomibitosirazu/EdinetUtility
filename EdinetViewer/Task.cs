﻿using System;
using System.Collections.Generic;

using System.Data;

using System.IO;
using System.IO.Compression;
using System.Threading;

using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;

namespace Edinet {

    //dummy デザイナー表示しないため
    public class MyTask { }

    partial class Form1 {

        public enum TaskType { List, Archive, TodayArchive, Taxonomy, EdinetCode, Start, VersionUp, test };

        //非同期を３分類（⓪書類一覧取得、①アーカイブ連続ダウンロード、②その他）して同時実行可能にする
        private Task[] backgroundTask;
        private CancellationTokenSource[] backgroundCancel;
        private async Task BackGroundStart(TaskType type, object parameter = null) {
            if(backgroundTask == null) {
                backgroundTask = new Task[3];
                backgroundCancel = new CancellationTokenSource[3];
            }
            int typeNo = (int)type < 2 ? (int)type : ((int)type == 2 ? 1 : 2);
            //if ((int)type > 2)
            //    typeNo = 2;
            if (backgroundTask[typeNo] == null) {
                backgroundCancel[typeNo] = new CancellationTokenSource();

                switch (type) {
                    case TaskType.List:
                            backgroundTask[0] = DownloadLists(backgroundCancel[0].Token);
                        break;
                    case TaskType.Archive:
                            backgroundTask[1] = DownloadArchives(backgroundCancel[1].Token);
                        break;
                    case TaskType.TodayArchive:
                        backgroundTask[1] = DownloadArchives(backgroundCancel[1].Token, true);
                        break;
                    case TaskType.Taxonomy:
                        backgroundTask[2] = ImportTaxonomy(parameter.ToString());
                        break;
                    case TaskType.EdinetCode:
                        backgroundTask[2] = ImportEdinetCode(parameter.ToString());
                        break;
                    case TaskType.VersionUp:
                        backgroundTask[2] = VersionUp(parameter.ToString());
                        break;
                    case TaskType.Start:
                        break;
                    case TaskType.test:
                        backgroundTask[2] = ReadMetadataList(backgroundCancel[2].Token);
                        break;
                }
                await backgroundTask[typeNo];
                backgroundTask[typeNo] = null;
            } else {
                StatusLabel1.Text = string.Format("{0:HH:mm:ss} バックグラウンドで実行中のため自動ダウンロードはスキップしました",DateTime.Now);
            }
        }
   
        private void InvokeVisible(bool show, bool initialize = true) {
            if (this.InvokeRequired) {
                this.Invoke((MethodInvoker)(() => {
                    if (initialize) {
                        ProgressBar1.Value = 0;
                        ProgressLabel1.Text = "";
                    }
                    ProgressBar1.Visible = show;
                    ProgressLabel1.Visible = show;
                }));
            } else {
                if (initialize) {
                    ProgressBar1.Value = 0;
                    ProgressLabel1.Text = "";
                }
                ProgressBar1.Visible = show;
                ProgressLabel1.Visible = show;
            }
        }
        private void InvokeProgressLabel(int value, string text) {
            if (this.InvokeRequired) {
                this.Invoke((MethodInvoker)(() => {
                    if (value >= 0)
                        ProgressBar1.Value = value;
                    if (text != null)
                        ProgressLabel1.Text = text;
                }));
            } else {
                if (value >= 0)
                    ProgressBar1.Value = value;
                if (text != null)
                    ProgressLabel1.Text = text;
            }
        }
        private void InvokeLabel(string text) {
            if (this.InvokeRequired) {
                this.Invoke((MethodInvoker)(() => {
                    if (text != null)
                        ProgressLabel1.Text = text;
                }));
            } else {
                if (text != null)
                    ProgressLabel1.Text = text;
            }
        }
        private void InvokeProgress(int value) {
            if (this.InvokeRequired) {
                this.Invoke((MethodInvoker)(() => {
                    ProgressBar1.Value = value;
                }));
            } else {
                ProgressBar1.Value = value;
            }
        }
        private void InvokeMenuCheck(string name, bool check = false) {
            if (this.InvokeRequired) {
                this.Invoke((MethodInvoker)(() => {
                    switch (name) {
                        case "MenuPastList":
                            MenuPastList.Checked = check;
                            break;
                        case "MenuDownload":
                            MenuDownload.Checked = check;
                            break;
                    }
                }));
            } else {
                switch (name) {
                    case "MenuPastList":
                        MenuPastList.Checked = check;
                        break;
                    case "MenuDownload":
                        MenuDownload.Checked = check;
                        break;
                }
            }
        }
  


        private async Task ReadMetadataList(CancellationToken token) {
            token.ThrowIfCancellationRequested();
            List<DateTime> list = disclosures.Database.MetadataList();
            InvokeVisible(true);
            InvokeProgressLabel(0, "");
            int i = 0;
            foreach (DateTime dt in list) {
                if (token.IsCancellationRequested) {
                    InvokeProgressLabel(0, "Canceled");
                    await Task.Delay(2000);
                    InvokeVisible(false);
                    return;
                }
                i++;
                InvokeProgressLabel((int)(i * 100 / list.Count), dt.ToString("yyyy-MM-dd"));
                await Task.Delay(1);
            }
            await Task.Delay(500);
            InvokeProgressLabel(0, "");
            await Task.Delay(2000);
            InvokeVisible(false);
        }

        private async Task ImportEdinetCode(string archivefile) {
            InvokeVisible(true);
            Database.Sqlite.Delegate delegateMethod = InvokeProgress;
            await Task<Dictionary<string, int>>.Run(() => {
                disclosures.Database.UpdateEdinetCodelist(archivefile, delegateMethod, out Dictionary<string, int> dic);
                disclosures.DicEdinetCode = dic;
                });
            await Task.Delay(2000);
            InvokeVisible(false);

        }

        private async Task ImportTaxonomy(string archivefile) {
            if (archivefile == null || !File.Exists(archivefile))
                return;
            InvokeVisible(true);

            int count = 0;
            int i = 0;
            string archivename = Path.GetFileName(archivefile);
            using (ZipArchive archive = ZipFile.Open(archivefile, ZipArchiveMode.Read)) {
                foreach (ZipArchiveEntry entry in archive.Entries) {
                    string filename = entry.FullName;
                    if (filename.IndexOf("taxonomy") == 0 & Path.GetExtension(filename) == ".xml") {
                        count++;
                    }
                }
                InvokeProgressLabel(0, "");
                i = 0;
                foreach (ZipArchiveEntry entry in archive.Entries) {
                    string filename = entry.FullName;
                    if (filename.IndexOf("taxonomy") == 0 & Path.GetExtension(filename) == ".xml") {
                        i++;
                        using (Stream stream = entry.Open()) {
                            using (StreamReader reader = new StreamReader(stream)) {
                                string source = reader.ReadToEnd();
                                await Task.Run(()=> disclosures.Database.ImportTaxonomy(filename, source, archivename));
                            }
                        }
                        InvokeProgressLabel((int)(i/count*100), string.Format("{0}/{1} {2}", i, count, entry.Name));
                    }
                }
            }
            disclosures.ImportTaxonomy();
            InvokeProgressLabel(0, "タクソノミを構築しました");
            await Task.Delay(2000);
            InvokeProgressLabel(0, "");
            InvokeVisible(false);

        }

        private async Task<bool> CheckException(Exception ex, string menu = null) {
            if (ex == null)
                return false;
            InvokeLabel(ex.Message + ex.InnerException != null ? (" " + ex.InnerException.Message) : "");
            await Task.Delay(10000);
            if(menu!=null)
            InvokeMenuCheck(menu);
            InvokeVisible(false);
            return true;
        }
        private async Task DownloadLists(CancellationToken token) {
            token.ThrowIfCancellationRequested();
            List<DateTime> saved = disclosures.Database.MetadataList();
            DateTime min = DateTime.Now.AddYears(-5).Date;
            List<DateTime> list = new List<DateTime>();
            DateTime d = min;
            while(d.Date < DateTime.Now.Date) {
                if (!saved.Contains(d.Date))
                    list.Add(d.Date);
                d = d.AddDays(1);
            }
            list.Reverse();
            InvokeVisible(true);
            if (list.Count == 0) {
                InvokeLabel("更新が必要な日付はありません");
                await Task.Delay(2000);
                InvokeMenuCheck("MenuPastList");
                InvokeVisible(false);
                return;
            }
            InvokeProgressLabel(0, "");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Random random = new Random();
            Stack<int> stack = new Stack<int>();
            int i = 0;
            foreach(DateTime target in list) { 
                if (token.IsCancellationRequested) {
                    InvokeProgressLabel(0, "Canceled");
                    await Task.Delay(2000);
                    InvokeVisible(false);
                    return;
                }
                //ApiListResult result = await disclosures.GetDisclosureList(target, true);
                JsonContent content = await disclosures.ReadDocuments(target);
                //if(result.Exception != null) {
                //    InvokeLabel(result.Exception.Message + result.Exception.InnerException != null ? (" " + result.Exception.InnerException.Message) : "");
                //    await Task.Delay(5000);
                //    InvokeMenuCheck("MenuPastList");
                //    InvokeVisible(false);
                //    return;
                //}
                if (await CheckException(content.Exception, "MenuPastList"))
                    return;
                Console.Write("{0:mm':'ss\\.f} {1} {2} count:{3}", sw.Elapsed, content.Metadata.Parameter.Date, content.Metadata.Message, content.Metadata.Resultset.Count);
                string output = string.Format("{1:#,##0}/{2:#,##0} ({3:yyyy-MM-dd}) status[{4}]  {0:m'min'ss\\.f'sec'}経過", sw.Elapsed, i + 1, list.Count, target, content.StatusCode.Message);
                string error = null;
                if (content.StatusCode.Status != "200") {
                    if (content.StatusCode.Status == "404") {
                        if (stack.Count > 2 && stack.Peek() == 404) {
                            stack.Pop();
                            if (stack.Peek() == 404) {
                                //3連続で404 Not Foundはおかしいだろう
                                error = "Error 404[Not Found] 3連続";
                            } else
                                stack.Push(404);
                        }
                    } else {
                        error = string.Format("Error {0}[{1}]", content.StatusCode.Status, content.StatusCode.Status == "400" ? "Bad Request" : "Internal Server Error");
                    }
                }
                i++;
                if (error != null)
                    error += "　終了します";
                InvokeProgressLabel((int)(i/list.Count*100), error ?? output);
                if (error != null) {
                    return;
                }
                stack.Push(int.Parse(content.StatusCode.Status));
                int wait = random.Next((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000));
                Console.Write("  wait[{0}]", wait);
                await Task.Delay(wait);
                Console.WriteLine();
            }
            sw.Stop();
            
            await Task.Delay(2000);
            InvokeMenuCheck("MenuPastList");
            InvokeVisible(false);
            //return "finish";
        }
        private async Task DownloadArchives(CancellationToken token, bool today = false) {
            token.ThrowIfCancellationRequested();
            if (token.IsCancellationRequested) {
                InvokeProgressLabel(0, "Canceled");
                await Task.Delay(1000);
                InvokeVisible(false);
                return;
            }
            InvokeMenuCheck("MenuDownload", true);
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Random random = new Random();
            StringBuilder sb = new StringBuilder();
            if (setting.Xbrl)
                sb.Append("xbrlFlag = '1'");
            if (setting.Pdf)
                sb.Append((sb.Length > 0 ? " or  " : "") + "pdfFlag = '1'");
            if (setting.Attach)
                sb.Append((sb.Length > 0 ? " or  " : "") + "attachDocFlag = '1'");
            if (setting.English)
                sb.Append((sb.Length > 0 ? " or  " : "") + "englishDocFlag = '1'");
            if (sb.Length > 0) {
                sb.Insert(0, "and (");
                sb.Append(")");
            }
            //if (sb.Length > 0)
            //    sb.Append(" and xbrl != '404'");
            if (setting.DocumentTypes.Length > 0) {
                sb.Append(" and docTypeCode in (");
                for (int j = 0; j < setting.DocumentTypes.Length; j++) {
                    if (j > 0)
                        sb.Append(",");
                    sb.AppendFormat(" '{0}'", setting.DocumentTypes[j]);
                }
                sb.Append(")");
            }
            DateTime start = today ? DateTime.Now.Date : DateTime.Now.AddYears(-5).Date;

            string query = string.Format("select id, `date`, secCode, docid, docTypeCode, withdrawalStatus, edinetCode, xbrlFlag, pdfFlag, attachDocFlag, englishDocFlag, xbrl, pdf, attach, english from disclosures where date(`date`) >= '{0:yyyy-MM-dd}' {2} order by id{1};", start, today ? " desc" : "", sb.ToString());
            DataTable table = disclosures.Database.ReadQuery(query);
            int i = 0;
            InvokeVisible(true);
            if (table.Rows.Count == 0) {
                InvokeProgressLabel(0, "ダウンロードする書類はありません");
                await Task.Delay(1000);
                InvokeMenuCheck("MenuDownload");
                InvokeVisible(false);
                Console.WriteLine("canceled background download archive");
                return;
            }
            string[] fields = new string[] { "xbrl", "pdf", "attach", "english" };
            bool[] auto = new bool[] { setting.Xbrl, setting.Pdf, setting.Attach, setting.English };

            DataTable table2 = new DataTable();
            table2.Columns.Add("id", typeof(int));
            table2.Columns.Add("date", typeof(string));
            table2.Columns.Add("docID", typeof(string));
            table2.Columns.Add("type", typeof(int));
            table2.Columns.Add("no", typeof(int));
            List<DateTime> list = new List<DateTime>();
            int no = 0;

            await Task.Run(() => {

                foreach (DataRow r in table.Rows) {
                    if (token.IsCancellationRequested) {
                        InvokeProgressLabel(0, "Canceled");
                        InvokeVisible(false);
                        return;
                    }

                    int id = int.Parse(r["id"].ToString());//dbはlong
                    string docid = r["docID"].ToString();
                    string withdrawalStatus = r["withdrawalStatus"].ToString();
                    bool isnullEdinetCode = r.IsNull("edinetCode");
                    if (isnullEdinetCode && withdrawalStatus == "0") {
                        //縦覧終了
                        Console.WriteLine("{0} {1} 縦覧終了", id, docid);
                        continue;
                    }

                    DateTime date = DateTime.ParseExact(id.ToString().Substring(0, 6), "yyMMdd", null);
                    string docTypeCode = r["docTypeCode"].ToString();
                    string secCode = r["secCode"] == DBNull.Value ? "0" : r["secCode"].ToString();
                    int code = secCode.Length > 3 ? int.Parse(secCode.Substring(0, 4)) : 0;
                    //監視銘柄はすべてダウンロード
                    bool all = code > 1300 && setting.Watching != null && setting.Watching.Length > 0 && Array.IndexOf(setting.Watching, code) > -1;
                    if (!all & Array.IndexOf(setting.DocumentTypes, docTypeCode) < 0) {
                        //Console.WriteLine(" skip type[{0}]", docTypeCode);
                        continue;
                    }

                    //string xbrl = r["xbrl"] == DBNull.Value ? null : r["xbrl"].ToString();
                    //string pdf = r["pdf"] == DBNull.Value ? null : r["pdf"].ToString();
                    //string attach = r["attach"] == DBNull.Value ? null : r["attach"].ToString();
                    //string english = r["english"] == DBNull.Value ? null : r["english"].ToString();
                    bool[] flag404 = new bool[] {
                        r["xbrl"] != DBNull.Value && r["xbrl"].ToString() == "404" ? true:false,
                        r["pdf"] != DBNull.Value && r["pdf"].ToString() == "404" ? true:false,
                        r["attach"] != DBNull.Value && r["attach"].ToString() == "404" ? true:false,
                        r["english"] != DBNull.Value && r["english"].ToString() == "404" ? true:false
                    };
                    bool[] flag = new bool[] { r["xbrlFlag"].ToString() == "1", r["pdfFlag"].ToString() == "1",
                        r["attachDocFlag"].ToString() == "1", r["englishDocFlag"].ToString() == "1" };
                    bool[] check = new bool[] { setting.Xbrl, setting.Pdf, setting.Attach, setting.English };
                    for (int j = 0; j < fields.Length; j++) {
                        if (flag[j] & !flag404[j]) {
                            if (all | check[j]) {
                                int year = 20 * 100 + id / 100000000;
                                string filepath = string.Format(@"{0}\Documents\{1}\{2}_{3}.{4}", setting.Directory, year, docid, j + 1, j == 1 ? "pdf" : "zip");
                                bool exists = File.Exists(filepath);
                                if (!exists) {
                                    //list.Add(new object[] { id, docid, j + 1 });
                                    DataRow r2 = table2.NewRow();
                                    r2.BeginEdit();
                                    r2["id"] = id;
                                    r2["date"] = date.ToString("yyyy-MM-dd");
                                    r2["docID"] = docid;
                                    r2["type"] = j + 1;
                                    r2["no"] = no;
                                    r2.EndEdit();
                                    table2.Rows.Add(r2);
                                    no++;
                                    //Console.WriteLine(r["flag"].ToString());
                                    if (!list.Contains(date))
                                        list.Add(date);
                                }
                            }
                        }
                    }
                    i++;
                    if (!today) {
                        InvokeProgressLabel(i * 100 / table.Rows.Count, string.Format("\t\tダウンロード済みファイルをチェックしています  {0:yyyy-MM-dd}", date));
                    }
                }
            }
                        );


            DataView dv = new DataView(table2, "", today ? "id desc" : "id", DataViewRowState.CurrentRows);
            list.Sort();
            foreach (DateTime target in list) {
                dv.RowFilter = string.Format("date = '{0:yyyy-MM-dd}'", target);
                i = 0;
                int previd = 0;
                bool flag404 = false;
                for (int j = 0; j < dv.Count; j++) {

                    if (token.IsCancellationRequested) {
                        InvokeProgressLabel(0, "Canceled");
                        await Task.Delay(1000);
                        InvokeVisible(false);
                        return;
                    }
                    dgvList.Refresh();
                    int id = int.Parse(dv[j]["id"].ToString());//dbはlong
                    string docid = dv[j]["docID"].ToString();
                    int type = (int)dv[j]["type"];
                    int no2 = int.Parse(id.ToString().Substring(6, 4));
                    if (id != previd) {
                        Console.WriteLine();
                        Console.Write("{0} {1}", id, docid);
                        flag404 = false;
                    }
                    if (flag404)
                        continue;
                    ArchiveResponse response = await disclosures.DownloadArchive(id, docid,   (RequestDocument.DocumentType)Enum.ToObject(typeof(RequestDocument.DocumentType), type));
                    //string statuscode = response. result.StatusText;
                    //if (result.Error != null){
                    //    statuscode = result.Error.Root.MetaData.Status;
                        if (response.EdinetStatusCode!=null && response.EdinetStatusCode.Status == "404")
                        flag404 = true;
                    //}
                    string output = string.Format("ダウンロード {0:#,##0}/{1:#,##0} no:{2}{3}[{4}] status[{5}] ", i, dv.Count, no2, today ? "" : string.Format("({0:yyyy-MM-dd})", target), fields[type - 1], response.HttpStatusCode.ToString());
                    if (await CheckException(response.Exception, "MenuDownload"))
                        return;
                    i++;
                    if (!today)
                        output += string.Format("{0:m':'ss}経過", sw.Elapsed);

                    InvokeProgressLabel((int)(i * 100 / dv.Count), output);
                    Console.Write(" {0:m':'ss'.'f} {1}[{2}]", sw.Elapsed, fields[type - 1], response.Filename);

                    int wait = random.Next((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000));
                    await Task.Delay(wait);
                    Console.Write(" wait[{0}]", wait);
                    previd = id;
                }
            }


            sw.Stop();
            Console.WriteLine("finish background download archive");
            await Task.Delay(1000);
            InvokeVisible(false);
            InvokeMenuCheck("MenuDownload");

        }



    }


}