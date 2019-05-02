using System;
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
                try {
                    this.Invoke((MethodInvoker)(() => {
                        if (initialize) {
                            ProgressBar1.Value = 0;
                            ProgressLabel1.Text = "";
                        }
                        ProgressBar1.Visible = show;
                        ProgressLabel1.Visible = show;
                    }));
                } catch (Exception ex) {
                    Console.WriteLine($"in InvokeVisible {ex.Message.Replace("\r\n", "\t")}");
                }

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
                try {
                    this.Invoke((MethodInvoker)(() => {
                        if (value >= 0)
                            ProgressBar1.Value = value;
                        if (text != null)
                            ProgressLabel1.Text = text;
                    }));

                } catch (Exception ex) {
                    Console.WriteLine($"in InvokeProgressLabel {ex.Message.Replace("\r\n", "\t")}");
                }
            } else {
                if (value >= 0 & ProgressBar1 != null)
                    ProgressBar1.Value = value;
                if (text != null & ProgressLabel1 != null)
                    ProgressLabel1.Text = text;
            }
        }
        private void InvokeLabel(string text) {
            if (this.InvokeRequired) {
                try {
                    this.Invoke((MethodInvoker)(() => {
                        if (text != null)
                            ProgressLabel1.Text = text;
                    }));

                } catch (Exception ex) {
                    Console.WriteLine($"in InvokeLabel {ex.Message.Replace("\r\n", "\t")}");
                }
            } else {
                if (text != null)
                    ProgressLabel1.Text = text;
            }
        }
        private void InvokeProgress(int value) {
            if (this.InvokeRequired) {
                try {
                    this.Invoke((MethodInvoker)(() => {
                        ProgressBar1.Value = value;
                    }));

                } catch (Exception ex) {
                    Console.WriteLine($"in InvokeProgress {ex.Message.Replace("\r\n", "\t")}");
                }
            } else {
                ProgressBar1.Value = value;
            }
        }
        private void InvokeMenuCheck(string name, bool check = false) {
            if (this.InvokeRequired) {
                try {
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
                } catch (Exception ex) {
                    Console.WriteLine($"in InvokeMenuCheck {ex.Message.Replace("\r\n", "\t")}");
                }

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
            try {
                InvokeLabel(ex.Message + ex.InnerException != null ? (" " + ex.InnerException.Message) : "");

            } catch (Exception ex1) {

                Console.WriteLine($"{ex.Message}\r\n*****{ex1.Message}******");
            }
            await Task.Delay(10000);
            if (menu != null)
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
            foreach (DateTime target in list) {
                if (token.IsCancellationRequested) {
                    InvokeProgressLabel(0, "Canceled");
                    await Task.Delay(2000);
                    InvokeVisible(false);
                    return;
                }
                JsonContent content = await disclosures.ReadDocuments(target);
                if (await CheckException(content.Exception, "MenuPastList"))
                    return;
                if (content.StatusCode != null) {
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
                    if (error != null)
                        error += "　終了します";
                    InvokeProgressLabel((int)(i / list.Count * 100), error ?? output);
                    if (error != null) {
                        return;
                    }
                    stack.Push(int.Parse(content.StatusCode.Status));
                    int wait = random.Next((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000));
                    Console.Write("  wait[{0}]", wait);
                    await Task.Delay(wait);
                    Console.WriteLine();
                }
                i++;
            }
            sw.Stop();
            
            await Task.Delay(2000);
            InvokeMenuCheck("MenuPastList");
            InvokeVisible(false);
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
            InvokeVisible(true);
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            StringBuilder sb = new StringBuilder();
            if (setting.Xbrl)
                sb.Append("(xbrlFlag = '1' and xbrl is null)");
            if (setting.Pdf)
                sb.Append((sb.Length > 0 ? " or  " : "") + "(pdfFlag = '1' and pdf is null)");
            if (setting.Attach)
                sb.Append((sb.Length > 0 ? " or  " : "") + "(attachDocFlag = '1' and attach is null)");
            if (setting.English)
                sb.Append((sb.Length > 0 ? " or  " : "") + "(englishDocFlag = '1' and english is null)");
            if (sb.Length > 0) {
                sb.Insert(0, "and (");
                sb.Append(")");
            }
            if (setting.DocumentTypes.Length > 0 & setting.Watching.Length > 0)
                sb.Append(" and (");
            else
                sb.Append(" and");
            if (setting.DocumentTypes.Length > 0) {
                sb.Append(" docTypeCode in (");
                for (int j = 0; j < setting.DocumentTypes.Length; j++) {
                    if (j > 0)
                        sb.Append(",");
                    sb.AppendFormat(" '{0}'", setting.DocumentTypes[j]);
                }
                sb.Append(")");
            }
            if (setting.Watching != null && setting.Watching.Length > 0) {
                sb.Append(" or secCode in (");
                for (int j = 0; j < setting.Watching.Length; j++) {
                    if (j > 0)
                        sb.Append(",");
                    sb.AppendFormat(" '{0}0'", setting.Watching[j]);
                }
                sb.Append(")");
            }
            if (setting.DocumentTypes.Length > 0 & setting.Watching.Length > 0)
                sb.Append(")");

            List<DateTime> listMetadata = null;
            //List<DateTime> listAccess = null;
            List<DateTime> list = new List<DateTime>();
            Random random = new Random();
            if (!today) {
                listMetadata = disclosures.Database.MetadataList();
                await Task.Run(() => {
                    string query = string.Format("select distinct date(`date`) from disclosures where date(`date`) >= '{0:yyyy-MM-dd}' and (`status` is null or `status` != '縦覧終了') {1} order by id;", DateTime.Now.AddYears(-5).Date, sb.ToString());
                    DataTable table = disclosures.Database.ReadQuery(query);
                    foreach (DataRow r in table.Rows)
                        list.Add(DateTime.Parse(r[0].ToString()));
                });

                if (list.Count == 0) {
                    InvokeProgressLabel(0, "ダウンロードする書類はありません");
                    await Task.Delay(1000);
                    InvokeMenuCheck("MenuDownload");
                    InvokeVisible(false);
                    Console.WriteLine("canceled background download archive");
                    return;
                }

            } else
                list.Add(DateTime.Now.Date);

            string[] fields = new string[] { "xbrl", "pdf", "attach", "english" };
            DateTime target = today ? DateTime.Now.Date : DateTime.Now.AddYears(-5).Date;
            DateTime end = today ? DateTime.Now.Date : DateTime.Now.AddDays(-1).Date;

            do {

                if (listMetadata != null && (listMetadata.Contains(target) & !list.Contains(target))) {
                    //Console.WriteLine($"skip {target:yyyy-MM-dd}");
                    target = target.AddDays(1);
                    continue;
                }

                //TimeSpan wait = CalcWait(sw.Elapsed);
                //if (wait.Ticks > 0)
                //    await Task.Delay(wait);
                JsonContent content = await disclosures.ReadDocuments(target,false);
                if(content.StatusCode!=null)
                InvokeLabel($"metadata API {target:yyyy-MM-dd (ddd)}");
                if (content.Metadata.Resultset.Count == 0)
                    continue;

                sw.Restart();
                DataView dv = new DataView(content.Table, "", today ? "id" : "id", DataViewRowState.CurrentRows);
                //int count = dv.Count;
                int count = 0;
                for (int j = 0; j < dv.Count; j++) {

                    if (token.IsCancellationRequested) {
                        InvokeProgressLabel(0, "Canceled");
                        await Task.Delay(1000);
                        InvokeVisible(false);
                        return;
                    }

                    DataRowView r = dv[j];
                    if (r["status"] != DBNull.Value) {
                        if (r["status"].ToString() == "縦覧終了") {
                            continue;
                        }
                    }
                    if (r["docID"] == DBNull.Value)
                        continue;
                    string docid = r["docID"].ToString();
                    bool[] check = new bool[] { setting.Xbrl, setting.Pdf, setting.Attach, setting.English };
                    bool[] flag = new bool[] { r["xbrlFlag"] != DBNull.Value && r["xbrlFlag"].ToString() == "1",
                        r["pdfFlag"] != DBNull.Value && r["pdfFlag"].ToString() == "1",
                        r["attachDocFlag"] != DBNull.Value &&        r["attachDocFlag"].ToString() == "1",
                        r["englishDocFlag"] != DBNull.Value && r["englishDocFlag"].ToString() == "1" };
                    bool[] downloaded = new bool[] {
                                r["xbrl"] != DBNull.Value && r["xbrl"].ToString().Contains(docid) ? true:false,
                                r["pdf"] != DBNull.Value && r["pdf"].ToString().Contains(docid) ? true:false,
                                r["attach"] != DBNull.Value && r["attach"].ToString().Contains(docid) ? true:false,
                                r["english"] != DBNull.Value && r["english"].ToString().Contains(docid) ? true:false
                    };
                    if (flag[0] == downloaded[0] & flag[1] == downloaded[1] & flag[2] == downloaded[2] & flag[3] == downloaded[3])
                        continue;
                    bool flagWatching = r["secCode"] != DBNull.Value && int.TryParse(r["secCode"].ToString(), out int code) && Array.IndexOf(setting.Watching, code / 10) > -1;

                    bool[] flag404 = new bool[] {
                                r["xbrl"] != DBNull.Value && r["xbrl"].ToString() == "404" ? true:false,
                                r["pdf"] != DBNull.Value && r["pdf"].ToString() == "404" ? true:false,
                                r["attach"] != DBNull.Value && r["attach"].ToString() == "404" ? true:false,
                                r["english"] != DBNull.Value && r["english"].ToString() == "404" ? true:false
                            };

                    int id = int.Parse(r["id"].ToString());//dbはlong
                    int no2 = int.Parse(id.ToString().Substring(6, 4));

                    bool notFound = false;
                    Console.Write("{0} {1}", id, docid);
                    for (int i = 0; i < fields.Length; i++) {
                        if (token.IsCancellationRequested) {
                            InvokeProgressLabel(0, "Canceled");
                            await Task.Delay(1000);
                            InvokeVisible(false);
                            return;
                        }
                        if (!flag[i] | r[fields[i]].ToString().Contains(docid))
                            continue;
                        if (flagWatching | check[i]) {
                            int year = 20 * 100 + id / 100000000;
                            string filepath = string.Format(@"{0}\Documents\{1}\{2}_{3}.{4}", setting.Directory, year, docid, i + 1, i == 1 ? "pdf" : "zip");
                            bool exists = File.Exists(filepath);
                            if (!exists) {
                                if (notFound) {
                                    disclosures.UpdateArchiveStatus(id, (RequestDocument.DocumentType)Enum.ToObject(typeof(RequestDocument.DocumentType), i + 1), "404");
                                    //continue;
                                } else {
                                    //sw.Stop();
                                    //wait = CalcWait(sw.Elapsed);
                                    //if (wait.Ticks > 0) {
                                    //    sw.Restart();
                                    //    await Task.Delay(wait);
                                    //    sw.Stop();
                                    //    Console.Write($" {(sw.ElapsedTicks / 10000):0.0}ms");
                                    //}
                                    await Wait((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000));
                                    if (today || count % 5 == 0) {
                                        //5回ごとにレスポンスチェック
                                        ArchiveResponse response = await disclosures.DownloadArchive(id, docid, (RequestDocument.DocumentType)Enum.ToObject(typeof(RequestDocument.DocumentType), i + 1));
                                        //sw.Restart();
                                        if (response.EdinetStatusCode != null && response.EdinetStatusCode.Status == "404")
                                            notFound = true;
                                        string output = string.Format("ダウンロード {0:#,##0}/{1:#,##0} no:{2}{3}[{4}] status[{5}] ", j, dv.Count, no2, today ? "" : string.Format("({0:yyyy-MM-dd})", target), fields[i], response.HttpStatusCode.ToString());
                                        if (await CheckException(response.Exception, "MenuDownload"))
                                            return;
                                        string filename = $"{docid}_{i + 1}";
                                        if (!today)
                                            output += string.Format("{0:m':'ss}経過", sw.Elapsed);
                                        InvokeProgressLabel((int)(j * 100 / dv.Count), output);
                                        Console.Write(" {0}[{1}]", fields[i], filename);
                                    } else {

                                        // * これが一番早い
                                        await disclosures.DownloadArchiveNoAwait(id, docid, (RequestDocument.DocumentType)Enum.ToObject(typeof(RequestDocument.DocumentType), i + 1));
                                        //sw.Restart();
                                        string output = string.Format("ダウ ンロード {0:#,##0}/{1:#,##0} no:{2}{3}[{4}] status[{5}] ", j, dv.Count, no2, today ? "" : string.Format("({0:yyyy-MM-dd})", target), fields[i], "");
                                        string filename = $"{docid}_{i + 1}";
                                        if (!today)
                                            output += string.Format("{0:m':'ss}経過", sw.Elapsed);
                                        InvokeProgressLabel((int)(j * 100 / dv.Count), output);
                                        Console.Write(" {0}[{1}]", fields[i], filename);

                                        // //*statuscodeまで待つと多少時間かかる
                                        //System.Net.Http.HttpResponseMessage resMessage = await disclosures.RequestDownload(docid, (RequestDocument.DocumentType)Enum.ToObject(typeof(RequestDocument.DocumentType), i + 1));
                                        //sw.Restart();
                                        //string output = string.Format("ダウ ンロード {0:#,##0}/{1:#,##0} no:{2}{3}[{4}] status[{5}] ", j, count, no2, today ? "" : string.Format("({0:yyyy-MM-dd})", target), fields[i], resMessage.StatusCode.ToString());
                                        //disclosures.Download(resMessage, id, fields[i]);
                                        //string filename = $"{docid}_{i + 1}";
                                        //InvokeProgressLabel((int)(j * 100 / count), output);
                                        //Console.Write(" {0}[{1}]", fields[i], filename);
                                    }
                                    count++;
                                }
                            }
                        }
                    }
                    Console.WriteLine();
                }
                target = target.AddDays(1);
                if (target == end)
                    break;
                //日付変更のウェイト
                await Wait(2400, 4800);
                Console.WriteLine();

            } while (target <= end);




            sw.Stop();
            Console.WriteLine("finish background download archive");
            await Task.Delay(1000);
            InvokeVisible(false);
            InvokeMenuCheck("MenuDownload");

        }

        private async Task Wait(int wait1, int wait2) {
            int waitMin = Math.Min(wait1, wait2);
            int waitMax = Math.Max(wait1, wait2);
            Random random = new Random();
            int wait = random.Next(waitMin, waitMax);
            Console.Write(" delay{0}ms", wait);
            await Task.Delay(wait);
        }

        //private TimeSpan CalcWait(TimeSpan elapsed) {
        //    Random random = new Random();
        //    int waitMin = (int)(Math.Min(setting.Wait[0], setting.Wait[1]) * 1000);
        //    int waitMax = (int)(Math.Max(setting.Wait[0], setting.Wait[1]) * 1000);
        //    TimeSpan ts = new TimeSpan(0);
        //    if (elapsed < new TimeSpan(waitMax * 10000)) {
        //        TimeSpan wait = new TimeSpan(random.Next(waitMin, waitMax) * 10000);
        //        if (elapsed < wait) {
        //            //Console.WriteLine($"delay {wait - elapsed}");
        //            ts = wait - elapsed;
        //        }
        //    }
        //    Console.Write(" delay{0}ms({1:0.00}ms経過)", ts.Milliseconds, elapsed.Ticks / 10000);
        //    return ts;
        //}


    }


}