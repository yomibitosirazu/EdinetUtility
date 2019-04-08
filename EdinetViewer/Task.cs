using System;
using System.Collections.Generic;

using System.Data;

using System.IO;
using System.IO.Compression;
using System.Threading;

using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;

namespace EdinetViewer {

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
                //for (int i = 0; i < backgroundTask.Length; i++) {
                //    backgroundCancel[i] = new CancellationTokenSource(); 
                //}
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
            }
        }
   
        private void InvokeVisible(bool show) {
            if (this.InvokeRequired) {
                this.Invoke((MethodInvoker)(() => {
                    ProgressBar1.Value = 0;
                    ProgressLabel1.Text = "";
                    ProgressBar1.Visible = show;
                    ProgressLabel1.Visible = show;
                }));
            } else {
                ProgressBar1.Visible = show;
                ProgressLabel1.Visible = show;
            }
        }
        private void InvokeProgressLabel(int max, int value, string text) {
            if (this.InvokeRequired) {
                this.Invoke((MethodInvoker)(() => {
                    if (max > 0)
                        ProgressBar1.Maximum = max;
                    if (value > 0)
                        ProgressBar1.Value = value;
                    if (text != null)
                        ProgressLabel1.Text = text;
                }));
            } else {
                if (max > 0)
                    ProgressBar1.Maximum = max;
                if (value > 0)
                    ProgressBar1.Value = value;
                if (text != null)
                    ProgressLabel1.Text = text;
            }
        }
        private void InvokeProgress(int value, int max = 0) {
            if (this.InvokeRequired) {
                this.Invoke((MethodInvoker)(() => {
                    if (max > 0)
                        ProgressBar1.Maximum = max;
                    ProgressBar1.Value = value;
                }));
            } else {
                ProgressBar1.Maximum = max;
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
            List<DateTime> list = edinet.Database.MetadataList();
            InvokeProgressLabel(list.Count, 0, "");
            int i = 0;
            foreach (DateTime dt in list) {
                if (token.IsCancellationRequested) {
                    InvokeProgressLabel(0, 0, "Canceled");
                    await Task.Delay(2000);
                    InvokeVisible(false);
                    return;
                }
                i++;
                InvokeProgressLabel(0, i, dt.ToString("yyyy-MM-dd"));
                await Task.Delay(1);
            }
            await Task.Delay(500);
            InvokeProgressLabel(0, 0, "");
            await Task.Delay(2000);
            InvokeVisible(false);
        }

        private async Task ImportEdinetCode(string archivefile) {
            InvokeVisible(true);
            Disclosures.Database.Sqlite.Delegate delegateMethod = InvokeProgress;
            await Task<Dictionary<string, int>>.Run(() => {
                edinet.Database.UpdateEdinetCodelist(archivefile, delegateMethod, out Dictionary<string, int> dic);
                edinet.DicEdinetCode = dic;
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
                InvokeProgressLabel(count, 0, "");
                foreach (ZipArchiveEntry entry in archive.Entries) {
                    string filename = entry.FullName;
                    if (filename.IndexOf("taxonomy") == 0 & Path.GetExtension(filename) == ".xml") {
                        i++;
                        using (Stream stream = entry.Open()) {
                            using (StreamReader reader = new StreamReader(stream)) {
                                string source = reader.ReadToEnd();
                                await Task.Run(()=> edinet.Database.ImportTaxonomy(filename, source, archivename));
                            }
                        }
                        InvokeProgressLabel(0, i, string.Format("{0}/{1} {2}", i, count, entry.Name));
                    }
                }
            }
            edinet.ImportTaxonomy();
            InvokeProgressLabel(0, 0, "タクソノミを構築しました");
            await Task.Delay(2000);
            InvokeProgressLabel(0, 0, "");
            InvokeVisible(false);

        }

        private async Task DownloadLists(CancellationToken token) {
            token.ThrowIfCancellationRequested();
            List<DateTime> saved = edinet.Database.MetadataList();
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
                InvokeProgressLabel(100, 100, "更新が必要な日付はありません");
                await Task.Delay(2000);
                InvokeMenuCheck("MenuPastList");
                InvokeVisible(false);
                return;
            }
            InvokeProgressLabel(list.Count, 0, "");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Random random = new Random();
            Stack<int> stack = new Stack<int>();
            int i = 0;
            foreach(DateTime target in list) { 
                if (token.IsCancellationRequested) {
                    InvokeProgressLabel(0, 0, "Canceled");
                    await Task.Delay(2000);
                    InvokeVisible(false);
                    return;
                }
                Disclosures.ApiListResult result = await edinet.GetDisclosureList(target, true);
                Console.Write("{0:mm':'ss\\.f} {1} {2} count:{3}", sw.Elapsed, result.Json.Root.metadata.parameter.date, result.Json.Root.metadata.message, result.Json.Root.metadata.resultset.count);
                string output = string.Format("{0:mm':'ss\\.f}経過 {1}/{2} {3:yyyy-MM-dd} status[{4}]", sw.Elapsed, i + 1, list.Count, target, result.StatusCode.ToString());
                string error = null;
                if (result.StatusCode != 200) {
                    if (result.StatusCode == 404) {
                        if (stack.Count > 2 && stack.Peek() == 404) {
                            stack.Pop();
                            if (stack.Peek() == 404) {
                                //3連続で404 Not Foundはおかしいだろう
                                error = "Error 404[Not Found] 3連続";
                            } else
                                stack.Push(404);
                        }
                    } else {
                        error = string.Format("Error {0}[{1}]", result.StatusCode, result.StatusCode == 400 ? "Bad Request" : "Internal Server Error");
                    }
                }
                i++;
                if (error != null)
                    error += "　終了します";
                InvokeProgressLabel(0, i, error ?? output);
                if (error != null) {
                    return;
                }
                stack.Push(result.StatusCode);
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
            if (setting.DocumentTypes.Length > 0) {
                sb.Append( " and docTypeCode in (");
                for (int j = 0; j < setting.DocumentTypes.Length; j++) {
                    if (j > 0)
                        sb.Append(",");
                    sb.AppendFormat(" '{0}'", setting.DocumentTypes[j]);
                }
                sb.Append(")");
            }
            DateTime start = today ? DateTime.Now.Date : DateTime.Now.AddYears(-5).Date;
            string query = string.Format("select id, secCode, filerName, docDescription, docid, docTypeCode, edinetCode, withdrawalStatus, xbrlFlag, pdfFlag, attachDocFlag, englishDocFlag, xbrl, pdf, attach, english from disclosures where date(`date`) >= '{0:yyyy-MM-dd}' {2} order by id{1};", start, today ? " desc" : "", sb.ToString());
            edinet.Database.ReadQuery(query, out DataTable table);
            int i = 0;
            InvokeVisible(true);
            InvokeProgress(0, table.Rows.Count);
            if (table.Rows.Count == 0) {
                InvokeProgressLabel(0, 0, "ダウンロードする銘柄はありません");
                await Task.Delay(5000);
                InvokeMenuCheck("MenuDownload");
                InvokeVisible(false);
                Console.WriteLine("canceled background download archive");
                return;
            }
            InvokeProgressLabel(table.Rows.Count, 0, "");
            DateTime last = DateTime.Now;
            string[] fields = new string[] { "xbrl", "pdf", "attach", "english" };
            bool[] auto = new bool[] { setting.Xbrl, setting.Pdf, setting.Attach, setting.English };

            foreach (DataRow r in table.Rows) {
                if (token.IsCancellationRequested) {
                    InvokeProgressLabel(0, 0, "Canceled");
                    await Task.Delay(5000);
                    InvokeVisible(false);
                    return;
                }
                //long lid = (Int64)r["id"];
                int id = int.Parse(r["id"].ToString());//dbはlong
                string docid = r["docID"].ToString();
                string withdrawalStatus = r["withdrawalStatus"].ToString();
                bool isnullEdinetCode = r.IsNull("edinetCode");
                if (isnullEdinetCode && withdrawalStatus == "0") {
                    //縦覧終了
                    InvokeProgressLabel(0, 0, string.Format("{0:HH:mm:ss} {1}縦覧終了{0} {1} {2}",DateTime.Now, docid, id));
                    Console.WriteLine("{0:HH:mm:ss} {1}縦覧終了{0} {1} {2}", DateTime.Now, docid, id);
                    continue;
                }

                DateTime date = DateTime.ParseExact(id.ToString().Substring(0, 6), "yyMMdd", null);
                //if (date < start) {
                //    Console.WriteLine("skip " + id.ToString());
                //}
                //if (date >= start) {
                Console.Write("{0}", id);
                string docTypeCode = r["docTypeCode"].ToString();
                string secCode = r["secCode"] == DBNull.Value ? "0" : r["secCode"].ToString();
                int code = secCode.Length > 3 ? int.Parse(secCode.Substring(0, 4)) : 0;
                //監視銘柄はすべてダウンロード
                bool all = code > 1300 && setting.Watching != null && setting.Watching.Length > 0 && Array.IndexOf(setting.Watching, code) > -1;
                if (!all & Array.IndexOf(setting.DocumentTypes, docTypeCode) < 0) {
                    Console.WriteLine(" skip type[{0}]", docTypeCode);
                    continue;
                }

                //bool xbrlFlag = r["xbrlFlag"].ToString() == "1";
                //bool pdfFlag = r["pdfFlag"].ToString() == "1";
                //bool attachFlag = r["attachDocFlag"].ToString() == "1";
                //bool englishFlag = r["englishDocFlag"].ToString() == "1";
                string xbrl = r["xbrl"] == DBNull.Value ? null : r["xbrl"].ToString();
                string pdf = r["pdf"] == DBNull.Value ? null : r["pdf"].ToString();
                string attach = r["attach"] == DBNull.Value ? null : r["attach"].ToString();
                string english = r["english"] == DBNull.Value ? null : r["english"].ToString();
                bool[] flag = new bool[] { r["xbrlFlag"].ToString() == "1", r["pdfFlag"].ToString() == "1",
                        r["attachDocFlag"].ToString() == "1", r["englishDocFlag"].ToString() == "1" };
                bool[] check = new bool[] { setting.Xbrl, setting.Pdf, setting.Attach, setting.English };
                //string[] saved = new string[] { xbrl, pdf, attach, english };
                bool[] notsaved = new bool[] {r["xbrl"] == DBNull.Value || r["xbrl"].ToString().Trim() =="",
                        r["pdf"] == DBNull.Value || r["pdf"].ToString().Trim() =="",
                        r["attach"] == DBNull.Value || r["attach"].ToString().Trim() =="",
                        r["english"] == DBNull.Value || r["english"].ToString().Trim() ==""
                    };
                bool access = false;
                for (int j = 0; j < fields.Length; j++) {
                    if (flag[j]) {
                        if (all | check[j]) {
                            int year = 20 * 100 + id / 100000000;
                            string filepath = string.Format(@"{0}\Documents\{1}\{2}_{3}.{4}", setting.Directory, year, docid, j+1, j == 1 ? "pdf" : "zip");
                            bool exists = File.Exists(filepath);
                            if (!exists) {
                                Console.Write("(delay[{1:s\\.fff}]{0} )", fields[j], DateTime.Now - last);
                                Disclosures.ApiArchiveResult result = await edinet.DownloadArchive(id, docid, j + 1);
                                last = DateTime.Now;
                                access = true;
                                InvokeProgressLabel(0, 0, string.Format("バックグラウンドダウンロード {0}/{1} {2:mm':'ss}経過 {3:yyyy-MM-dd} {4}[{5}] status[{6}]", i, table.Rows.Count, sw.Elapsed, date, result.Name, fields[j], result.StatusCode.ToString()));

                                int wait = random.Next((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000));
                                await Task.Delay(wait);
                                Console.Write(" wait[{0}]", wait);
                            }else
                                Console.Write(" ex[{0}]", r[fields[j]]);
                        } else {
                            
                        }
                    }
                }
                if (access)
                    Console.WriteLine("{0:HH:mm:ss} {1} {2}\t{3}\t{4}", DateTime.Now, docid, code > 0 ? code.ToString():"", r["filerName"], r["docDescription"]);
                else
                    Console.WriteLine("skip");
                i++;
                if (!access)
                    InvokeProgressLabel(0, i, string.Format("バックグラウンドダウンロード {0}/{1} {2:mm':'ss}経過 {3:yyyy-MM-dd} {4} skip", i, table.Rows.Count, sw.Elapsed, date, docid));

                //}
            }
            InvokeProgressLabel(0, 0, "バックグラウンド終了しました");
            Console.WriteLine("finish background download archive");
            await Task.Delay(1000);
            InvokeVisible(false);
            InvokeMenuCheck("MenuDownload");

        }





    }


}