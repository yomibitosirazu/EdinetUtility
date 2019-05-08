using System;
using System.Collections.Generic;

using System.Data;

using System.IO;
using System.IO.Compression;
using System.Threading;

using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;

namespace Edinet {

    //dummy デザイナー表示しないため
    public class MyTask { }

    partial class Form1 {


        public enum TaskType { List, Archive, TodayArchive, Taxonomy, EdinetCode, Start, VersionUp, test, DgvArchive };

        //非同期を３分類（⓪書類一覧取得、①アーカイブ連続ダウンロード、②その他）して同時実行可能にする
        private Task[] backgroundTask;
        private CancellationTokenSource[] backgroundCancel;
        private async Task BackGroundStart(TaskType type, object parameter = null) {
            if (backgroundTask == null) {
                backgroundTask = new Task[3];
                backgroundCancel = new CancellationTokenSource[3];
            }
            //int typeNo = (int)type < 2 ? (int)type : ((int)type == 2 ? 1 : 2);
#pragma warning disable IDE0059
            int typeNo = 2;
#pragma warning restore IDE0059

            switch (type) {
                case TaskType.List:
                    typeNo = 0;
                    break;
                case TaskType.DgvArchive:
                case TaskType.TodayArchive:
                case TaskType.Archive:
                    typeNo = 1;
                    break;
                default:
                    typeNo = 2;
                    break;
            }
            if (backgroundTask[typeNo] == null) {
                backgroundCancel[typeNo] = new CancellationTokenSource();

                switch (type) {
                    case TaskType.List:
                        backgroundTask[0] = DownloadLists(backgroundCancel[0].Token);
                        break;
                    case TaskType.Archive:
                        //backgroundTask[1] = DownloadArchives(backgroundCancel[1].Token);
                        backgroundTask[1] = DownloadArchives(backgroundCancel[1].Token, TaskType.Archive);
                        break;
                    case TaskType.TodayArchive:
                        //backgroundTask[1] = DownloadArchives(backgroundCancel[1].Token, true);
                        backgroundTask[1] = DownloadArchives(backgroundCancel[1].Token, TaskType.TodayArchive);
                        break;
                    case TaskType.DgvArchive:
                        backgroundTask[1] = DownloadArchives(backgroundCancel[1].Token, TaskType.DgvArchive);
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
                if (MenuDownload.Checked)
                    MenuDownload.Checked = false;
                else if (MenuPastList.Checked)
                    MenuPastList.Checked = false;
                else if (MenuDownloadDgvList.Checked)
                    MenuDownloadDgvList.Checked = false;
                await Task.Delay(5000);
                InvokeVisible(false);
            } else {
                StatusLabel1.Text = string.Format("{0:HH:mm:ss} バックグラウンドで実行中のため自動ダウンロードはスキップしました", DateTime.Now);
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
                try {
                    if (initialize) {
                        ProgressBar1.Value = 0;
                        ProgressLabel1.Text = "";
                    }
                    ProgressBar1.Visible = show;
                    ProgressLabel1.Visible = show;

                } catch (Exception ex) {
                    Console.WriteLine($"in InvokeVisible {ex.Message.Replace("\r\n", "\t")}");
                }
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

        private void InvokeRefresh() {
            if (this.InvokeRequired) {
                try {
                    this.Invoke((MethodInvoker)(() => {
                        dgvList.Refresh();
                    }));
                } catch (Exception ex) {
                    Console.WriteLine($"in InvokeRefresh {ex.Message.Replace("\r\n", "\t")}");
                }

            } else {
                try {
                    dgvList.Refresh();

                } catch (Exception ex) {
                    Console.WriteLine($"in InvokeRefresh {ex.Message.Replace("\r\n", "\t")}");
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
                                await Task.Run(() => disclosures.Database.ImportTaxonomy(filename, source, archivename));
                            }
                        }
                        InvokeProgressLabel((int)(i / count * 100), string.Format("{0}/{1} {2}", i, count, entry.Name));
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
                //InvokeLabel(ex.Message + ex.InnerException != null ? (" " + ex.InnerException.Message) : "");
                InvokeLabel(ex.Message);

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
            while (d.Date < DateTime.Now.Date) {
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
                    //await Task.Delay(2000);
                    //InvokeVisible(false);
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





        //private bool CheckForDownload(DataRowView r) {
        //    if ((r["status"] != DBNull.Value && r["status"].ToString() == "縦覧終了") | r["docID"] == DBNull.Value)
        //        return false;
        //    string[] fields = new string[] { "xbrl", "pdf", "attach", "english" };
        //    string docid = r["docID"].ToString();
        //    for (int i = 0; i < fields.Length; i++) {
        //        if (r[fields[i] + (i > 1 ? "Doc" : "") + "Flag"].ToString() == "1") {
        //            int id = int.Parse(r["id"].ToString());//dbはlong
        //            int year = 20 * 100 + id / 100000000;
        //            string filepath = string.Format(@"{0}\Documents\{1}\{2}_{3}.{4}", setting.Directory, year, docid, i + 1, i == 1 ? "pdf" : "zip");
        //            //一つでも未ダウンロードあればtrueとする
        //            if (!File.Exists(filepath)) {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}


        //private async Task<bool> UpdateListTable() {
        //    Random random = new Random();
        //    //Dictionary<DateTime,int> dicMetadata = disclosures.Database.GetFinalMetalist();
        //    List<DateTime> list = new List<DateTime>();
        //    foreach (DataRowView r in disclosures.DvDocuments) {
        //        DateTime target = (DateTime)r["date"];
        //        if (!list.Contains(target)) {
        //            list.Add(target);
        //            Edinet.Json.Metadata metadata = disclosures.Database.ReadMetadata(target);
        //            if (metadata != null) {
        //                DateTime process = DateTime.Parse(metadata.ProcessDateTime);
        //                if (process.AddDays(1) > DateTime.Now)
        //                    continue;
        //                //件数0はありえない
        //                //if(process.Date > target.Date && metadata.Resultset.Count == 0) { }
        //            }
        //            //件数が確定している場合はMetadataタイプ１はスキップ
        //            debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
        //            JsonContent content = await disclosures.ReadDocuments(target, true, false);
        //            debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
        //            if (content == null) {
        //                debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
        //                InvokeLabel("メタデータ取得unknownエラー");
        //                Debug.WriteLine("メタデータ取得unknownエラー");
        //                await Task.Delay(60 * 1000);
        //                return false;
        //            }
        //            if (content.Exception != null) {
        //                debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
        //                InvokeLabel($"HttpClientエラー {content.Exception.Message}");
        //                Debug.WriteLine($"HttpClientエラー {content.Exception.Message}");
        //                await Task.Delay(60 * 1000);
        //                return false;
        //            }
        //            if (content.StatusCode != null)
        //                InvokeLabel($"metadata API {target:yyyy-MM-dd (ddd)}");
        //            if (content.StatusCode != null && content.StatusCode.Status != "200") {
        //                //メタデータ取得エラー
        //                InvokeLabel($"{content.StatusCode.Status} {content.StatusCode.Message}");
        //                await Task.Delay(5000);
        //                Console.Write($"{content.StatusCode.Status} {content.StatusCode.Message} on {target:yyyy-MM-dd}");
        //                //if (dicMetadata != null && dicMetadata.ContainsKey(target) && dicMetadata[target] > 0)
        //                //    Console.WriteLine(" タイプ2");
        //                //else
        //                //    Console.WriteLine();
        //                return false;
        //            }
        //            if(disclosures.DvDocuments.Sort.ToLower().IndexOf("id") < 0) {

        //            }
        //            for (int i = 0; i < content.Table.Rows.Count; i++) {
        //                int id = int.Parse(content.Table.Rows[i]["id"].ToString());
        //                int index = disclosures.DvDocuments.Find(id);
        //                if (index > -1) {
        //                    disclosures.DvDocuments[index]["status"] = content.Table.Rows[i]["status"];
        //                    disclosures.DvDocuments[index]["withdrawalStatus"] = content.Table.Rows[i]["withdrawalStatus"];
        //                    disclosures.DvDocuments[index]["docInfoEditStatus"] = content.Table.Rows[i]["docInfoEditStatus"];
        //                    disclosures.DvDocuments[index]["disclosureStatus"] = content.Table.Rows[i]["disclosureStatus"];
        //                }
        //            }
        //            int wait0 = random.Next(Math.Min((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000)),
        //                    Math.Max((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000)));
        //            await Task.Delay(wait0);
        //            Debug.WriteLine($"metadata {target:yyyy-MM-dd} {content.Metadata.Resultset.Count}件 delay{wait0}");
        //            //if (content.Metadata.Resultset.Count == 0)
        //            //    continue;
        //        }
        //    }
        //    return true;
        //}


        private async Task<bool> DownloadTableList(CancellationToken token, DataView dv, bool isArchive) {
            //日付ごとにダウンロードリストを保持するためのテーブル
            DataTable tableDownload = new DataTable();
            tableDownload.Columns.Add("id", typeof(int));
            tableDownload.Columns.Add("docid", typeof(string));
            tableDownload.Columns.Add("type", typeof(int));
            tableDownload.Columns.Add("pk", typeof(Int64));
            tableDownload.Columns.Add("info", typeof(string));

            string[] fields = new string[] { "xbrl", "pdf", "attach", "english" };

            //日付ごとにダウンロード予定件数を事前にチェックすることにした 
            //docID + "_1"など
            //List<string> listArchive = new List<string>();
            //System.Collections.Specialized.NameValueCollection nvc = new System.Collections.Specialized.NameValueCollection();
            TimeSpan last = DateTime.Now.TimeOfDay;
            tableDownload.Rows.Clear();
            ////ダウンロード済みはまとめてdbフィールド更新する
            Dictionary<int, string>[] dicDownloaded = new Dictionary<int, string>[4];
            for (int i = 0; i < 4; i++)
                dicDownloaded[i] = new Dictionary<int, string>();
            foreach (DataRowView r in dv) {
                if (r["status"] != DBNull.Value && r["status"].ToString() == "縦覧終了")
                    continue;
                if (r["docID"] == DBNull.Value)
                    continue;
                string docid = r["docID"].ToString();
                bool[] check = new bool[] { setting.Xbrl, setting.Pdf, setting.Attach, setting.English };
                bool[] flag = new bool[] { r["xbrlFlag"] != DBNull.Value && r["xbrlFlag"].ToString() == "1",
                        r["pdfFlag"] != DBNull.Value && r["pdfFlag"].ToString() == "1",
                        r["attachDocFlag"] != DBNull.Value &&        r["attachDocFlag"].ToString() == "1",
                        r["englishDocFlag"] != DBNull.Value && r["englishDocFlag"].ToString() == "1" };
                bool[] dbdownloaded = new bool[] {
                                r["xbrl"] != DBNull.Value && r["xbrl"].ToString().Contains(docid) ? true:false,
                                r["pdf"] != DBNull.Value && r["pdf"].ToString().Contains(docid) ? true:false,
                                r["attach"] != DBNull.Value && r["attach"].ToString().Contains(docid) ? true:false,
                                r["english"] != DBNull.Value && r["english"].ToString().Contains(docid) ? true:false
                    };
                int id = int.Parse(r["id"].ToString());//dbはlong
                int year = 20 * 100 + id / 100000000;
                bool[] downloaded = new bool[4];
                for (int i = 0; i < 4; i++) {
                    string filepath = string.Format(@"{0}\Documents\{1}\{2}_{3}.{4}", setting.Directory, year, docid, i + 1, i == 1 ? "pdf" : "zip");
                    string flagField = fields[i].ToString() + (i > 1 ? "Doc" : "") + "Flag";
                    if (r[flagField] != DBNull.Value && r[flagField].ToString() == "1") {
                        if (File.Exists(filepath)) {
                            downloaded[i] = true;
                            if (!dbdownloaded[i]) {
                                dicDownloaded[i][id] = $"{docid}_{i + 1}";
                            }
                        }
                    }
                }
                bool flagWatching = r["secCode"] != DBNull.Value && int.TryParse(r["secCode"].ToString(), out int code) && Array.IndexOf(setting.Watching, code / 10) > -1;
                string docType = r["docTypeCode"].ToString();
                if (Array.IndexOf(setting.DocumentTypes, docType) < 0) {
                    continue;
                }
                int secno = int.Parse(id.ToString().Substring(6, 4));

                debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                string scode = r["secCode"] == DBNull.Value ? new string(' ', 5) : r["secCode"].ToString();
                string filer = r["filerName"] == DBNull.Value ? new string('　', 12) : (r["filerName"].ToString().Length > 12 ? r["filerName"].ToString().Substring(0, 12) : r["filerName"].ToString() + new string('　', 12 - r["filerName"].ToString().Length));
                string info = $"{id} {docid} {scode} {filer}";
                for (int i = 0; i < fields.Length; i++) {
                    //フラグが"1"以外またはダウンロード済みはスキップ
                    if (!flag[i] | downloaded[i])
                        continue;
                    if (flagWatching | check[i]) {
                        debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                        DataRow rdownload = tableDownload.NewRow();
                        rdownload["id"] = id;
                        rdownload["docid"] = docid;
                        rdownload["type"] = i + 1;
                        rdownload["pk"] = id * 10 + i + 1;
                        rdownload["info"] = info;
                        tableDownload.Rows.Add(rdownload);
                        //nvc.Add(docid, $"{id}\t{i + 1}\t{info}");
                        info = "";

                    }
                    debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                    //InvokeRefresh();
                }
            }
            Debug.Write($" tableDownloads準備完了 [{DateTime.Now.TimeOfDay - last:s\\.fff}sec]");
            last = DateTime.Now.TimeOfDay;
            Debug.Write($" ダウンロード済み");
            int countdl = 0;
            for (int i = 0; i < 4; i++) {
                countdl += dicDownloaded[i].Count;
                Debug.Write($"{dicDownloaded[i].Count} ");
            }
            if (countdl > 0)
                disclosures.Database.UpdateFilenameOfDisclosure(dicDownloaded);
            Debug.WriteLine($" dbフィールド更新 [{DateTime.Now.TimeOfDay - last:s\\.fff}sec]");
            TimeSpan start = DateTime.Now.TimeOfDay;

            Random random = new Random();
            //ここからダウンロード
            for (int i = 0; i < tableDownload.Rows.Count; i++) {
                if (token.IsCancellationRequested) {
                    InvokeProgressLabel(0, "Canceled");
                    return true;
                }
                DataRow r = tableDownload.Rows[i];
                int itype = (int)r["type"];
                int id = (int)r["id"];
                string info = r["info"].ToString();
                if (info != "") {
                    Debug.WriteLine("");
                    Debug.Write(info);
                }
                string docid = r["docid"].ToString();

                int year = 20 * 100 + id / 100000000;
                string filepath = string.Format(@"{0}\Documents\{1}\{2}_{3}.{4}", setting.Directory, year, docid, itype, itype == 2 ? "pdf" : "zip");
                //string flagField = fields[itype - 1].ToString() + (itype > 2 ? "Doc" : "") + "Flag";
                if (File.Exists(filepath)) {
                    Debug.Write($" {fields[itype - 1]} downloaded skip ");
#pragma warning disable CS4014
                    disclosures.Database.UpdateFilenameOfDisclosureAsync(id, fields[itype - 1], $"{docid}_{itype}");
#pragma warning restore CS4014

                    continue;
                }

                //string output = "";
                int wait = random.Next(Math.Min((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000)),
                    Math.Max((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000)));
                debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());

                await Task.Delay(wait);
                Debug.Write($" {fields[itype - 1]}[{DateTime.Now.TimeOfDay - last:s\\.fff}(delay:{wait})]");
                last = DateTime.Now.TimeOfDay;
                string strstatus = "";
                string filename = $"{docid}_{itype}";

                //if (taskType == TaskType.TodayArchive || totalcount % 5 == 0) {
                if (i % 5 == 0) {
                        System.Diagnostics.Debug.Write("await");
                    //5回ごとにレスポンスチェック
                    debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                    ArchiveResponse response = await disclosures.DownloadArchive(id, docid, (RequestDocument.DocumentType)Enum.ToObject(typeof(RequestDocument.DocumentType), itype));
                    debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                    if (response == null) {
                        debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                        InvokeLabel("メタデータ取得unknownエラー");
                        await Task.Delay(60 * 1000);
                        return true;
                    }
                    if (response.Exception != null) {
                        debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                        InvokeLabel($"HttpClientエラー {response.Exception.Message}");
                        await Task.Delay(60 * 1000);
                        return true;
                    }
                    if (response.HttpStatusCode != null)
                        strstatus = $"status[{ response.HttpStatusCode.ToString()}] ";
                    else {

                    }
                    if (response.EdinetStatusCode != null && response.EdinetStatusCode.Status == "404")
                        break;
                    if (response.Exception != null) {
                        InvokeLabel(response.Exception.Message);
                        Debug.Print($"in download 1/5 {response.Exception.Message}");
                        return true;
                    }
                    string field = fields[itype - 1];
                    //string archive = $"{docid}_{itype}";
                    disclosures.Database.UpdateFilenameOfDisclosure(id, field, filename);
                    debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                } else {
                    // * これが一番早い
                    debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                    await disclosures.DownloadArchiveNoAwait(id, docid, (RequestDocument.DocumentType)Enum.ToObject(typeof(RequestDocument.DocumentType), itype));
                    debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                }

                string strcount = $"{i + 1:#,##0}/{tableDownload.Rows.Count:#,##0} ";
                string sdate = "20" + $"{id.ToString().Substring(0, 6).Insert(4, "-").Insert(2, "-")}";
                int seqNo = int.Parse(id.ToString().Substring(6, id.ToString().Length - 6));
                string output = $"ダウンロード {strcount} [{fields[itype - 1]:0,-5}] {sdate}({seqNo:0,4}) ";

                if (isArchive)
                    output += string.Format("{0:m':'ss}経過", DateTime.Now.TimeOfDay - start);
                InvokeProgressLabel((int)((i + 1) * 100 / tableDownload.Rows.Count), output + strstatus);
                debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                if (!isArchive)
                    InvokeRefresh();
            }
            return false;
        }

        private bool CheckMetadataError(JsonContent content, DateTime target) {
            if (content == null) {
                debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                InvokeLabel("メタデータ取得unknownエラー");
                Debug.WriteLine($"メタデータ取得unknownエラー  on {target:yyyy-MM-dd}");
                //await Task.Delay(60 * 1000);
                return true;
            }
            if (content.Exception != null) {
                debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                InvokeLabel($"HttpClientエラー {content.Exception.Message}");
                Debug.WriteLine($"HttpClientエラー {content.Exception.Message} on {target:yyyy-MM-dd}");
                //await Task.Delay(60 * 1000);
                return true;
            }
            if (content.StatusCode != null && content.StatusCode.Status != "200") {
                //メタデータ取得エラー
                InvokeLabel($"{content.StatusCode.Status} {content.StatusCode.Message}");
                //await Task.Delay(5000);
                Debug.WriteLine($"{content.StatusCode.Status} {content.StatusCode.Message} on {target:yyyy-MM-dd}");
                return true;
            }
            return false;
        }
        private async Task DownloadArchives(CancellationToken token, TaskType taskType) {
            debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
            if (taskType == TaskType.DgvArchive && disclosures.DvDocuments.Count == 0) {
                return;
            }
            token.ThrowIfCancellationRequested();
            if (token.IsCancellationRequested) {
                InvokeProgressLabel(0, "Canceled");
                return;
            }
            switch (taskType) {
                case TaskType.Archive:
                case TaskType.TodayArchive:
                    InvokeMenuCheck("MenuDownload", true); break;
                case TaskType.DgvArchive:
                    InvokeMenuCheck("MenuDownloadDgvList", true); break;
            }

            InvokeVisible(true);

            string[] fields = new string[] { "xbrl", "pdf", "attach", "english" };

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
            Random random = new Random();

            List<DateTime> list = new List<DateTime>();
            //確定されていないMetadataの日付を追加
            Dictionary<DateTime, int> dicMetadata = null;
            
            if (taskType == TaskType.DgvArchive) {

                foreach (DataRowView r in disclosures.DvDocuments) {
                    DateTime target = DateTime.Parse(r["date"].ToString());
                    if (target < DateTime.Now.AddYears(-5).Date)
                        continue;
                    if (token.IsCancellationRequested) {
                        InvokeProgressLabel(0, "Canceled");
                        return;
                    }


                    if (r["status"] != DBNull.Value && r["status"].ToString() == "縦覧終了" | r["docID"] == DBNull.Value)
                        continue;
                    string docid = r["docID"].ToString();
                    bool[] check = new bool[] { setting.Xbrl, setting.Pdf, setting.Attach, setting.English };
                    bool[] flag = new bool[] { r["xbrlFlag"] != DBNull.Value && r["xbrlFlag"].ToString() == "1",
                        r["pdfFlag"] != DBNull.Value && r["pdfFlag"].ToString() == "1",
                        r["attachDocFlag"] != DBNull.Value &&        r["attachDocFlag"].ToString() == "1",
                        r["englishDocFlag"] != DBNull.Value && r["englishDocFlag"].ToString() == "1" };
                    bool[] dbdownloaded = new bool[] {
                                r["xbrl"] != DBNull.Value && r["xbrl"].ToString().Contains(docid) ? true:false,
                                r["pdf"] != DBNull.Value && r["pdf"].ToString().Contains(docid) ? true:false,
                                r["attach"] != DBNull.Value && r["attach"].ToString().Contains(docid) ? true:false,
                                r["english"] != DBNull.Value && r["english"].ToString().Contains(docid) ? true:false
                    };
                    int id = int.Parse(r["id"].ToString());//dbはlong
                    int year = 20 * 100 + id / 100000000;
                    bool[] downloaded = new bool[4];
                    bool notexist = false;
                    for (int i = 0; i < 4; i++) {
                        string filepath = string.Format(@"{0}\Documents\{1}\{2}_{3}.{4}", setting.Directory, year, docid, i + 1, i == 1 ? "pdf" : "zip");
                        string flagField = fields[i].ToString() + (i > 1 ? "Doc" : "") + "Flag";
                        if (r[flagField] != DBNull.Value && r[flagField].ToString() == "1") {
                            if (File.Exists(filepath)) {
                                downloaded[i] = true;
                            } else {
                                if (flag[i] & check[i])
                                    notexist = true;
                            }
                        }
                    }
                    if (!notexist)
                        continue;
                    bool flagWatching = r["secCode"] != DBNull.Value && int.TryParse(r["secCode"].ToString(), out int code) && Array.IndexOf(setting.Watching, code / 10) > -1;
                    string docType = r["docTypeCode"].ToString();
                    if (!flagWatching & Array.IndexOf(setting.DocumentTypes, docType) < 0)
                        continue;

                    if (!list.Contains(target)) {

                        list.Add(target);
                        debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                        Edinet.Json.Metadata metadata = disclosures.Database.ReadMetadata(target);
                        debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                        if (metadata != null) {
                            DateTime process = DateTime.Parse(metadata.ProcessDateTime);
                            if (process.AddDays(1) > DateTime.Now)
                                continue;
                            //件数0はありえない
                            //if(process.Date > target.Date && metadata.Resultset.Count == 0) { }
                        }
                        //件数が確定している場合はMetadataタイプ１はスキップ
                        debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                        JsonContent content = await disclosures.ReadDocuments(target, true, false);
                        debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                        bool error = CheckMetadataError(content, target);
                        if (error)
                            return;
                        if (content.StatusCode != null)
                            InvokeLabel($"metadata API {target:yyyy-MM-dd (ddd)}");
                        if (disclosures.DvDocuments.Sort.ToLower().IndexOf("id") < 0) {

                        }
                        debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                        for (int i = 0; i < content.Table.Rows.Count; i++) {
                            id = int.Parse(content.Table.Rows[i]["id"].ToString());
                            int index = disclosures.DvDocuments.Find(id);
                            if (index > -1) {
                                disclosures.DvDocuments[index].BeginEdit();
                                disclosures.DvDocuments[index]["status"] = content.Table.Rows[i]["status"];
                                disclosures.DvDocuments[index]["withdrawalStatus"] = content.Table.Rows[i]["withdrawalStatus"];
                                disclosures.DvDocuments[index]["docInfoEditStatus"] = content.Table.Rows[i]["docInfoEditStatus"];
                                disclosures.DvDocuments[index]["disclosureStatus"] = content.Table.Rows[i]["disclosureStatus"];
                                disclosures.DvDocuments[index].EndEdit();
                            }
                        }
                        int wait0 = random.Next(Math.Min((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000)),
                                Math.Max((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000)));
                        await Task.Delay(wait0);
                        //Debug.WriteLine($"metadata {target:yyyy-MM-dd} {content.Metadata.Resultset.Count}件 delay{wait0}");
                        Debug.WriteLine($" delay({wait0:#,##0})");
                        debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                    }
                }


            } else if (taskType == TaskType.Archive) {
                await Task.Run(() => {
                    dicMetadata = disclosures.Database.GetFinalMetalist();
                    string query = string.Format("select distinct date(`date`) from disclosures where date(`date`) >= '{0:yyyy-MM-dd}' and (`status` is null or `status` != '縦覧終了') {1} order by id;", DateTime.Now.AddYears(-5).Date, sb.ToString());
                    DataTable table = disclosures.Database.ReadQuery(query);
                    List<DateTime> targets = new List<DateTime>();
                    foreach (DataRow r in table.Rows)
                        targets.Add(DateTime.Parse(r[0].ToString()));

                    DateTime date = DateTime.Now.Date;
                    do {
                        date = date.AddDays(-1);
                        if (!dicMetadata.ContainsKey(date) | targets.Contains(date))
                            list.Add(date);
                    } while (date >= DateTime.Now.AddYears(-5).Date);
                });
                if (list.Count == 0) {
                    InvokeProgressLabel(0, "ダウンロードする書類はありません");
                    Console.WriteLine("no list background download archive");
                    return;
                }

            }

            list.Sort();
            if ((taskType == TaskType.Archive & !setting.OrderAscendingYear5) | 
                (taskType == TaskType.DgvArchive & !setting.OrderAscendingList))
                list.Reverse();

            string sort = "id";
            bool cancel = false;
            if (taskType != TaskType.Archive) {
                if (taskType == TaskType.TodayArchive & !setting.OrderAscendingToday)
                    sort = "id desc";
                else if (taskType == TaskType.DgvArchive & !setting.OrderAscendingList)
                    sort = "id desc";
                DataView dv = new DataView(disclosures.TableDocuments, "", sort, DataViewRowState.CurrentRows);
                cancel = await DownloadTableList(token, dv, false);
                if (cancel)
                    return;
            } else {
                foreach (DateTime target in list) {

                    if (token.IsCancellationRequested) {
                        InvokeProgressLabel(0, "Canceled");
                        return;
                    }
                    debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());

                    //確定でカウント0は必要なし
                    if (dicMetadata != null && dicMetadata.ContainsKey(target) && dicMetadata[target] == 0)
                        continue;

                    TimeSpan last = DateTime.Now.TimeOfDay;
                    if (dicMetadata != null && dicMetadata.ContainsKey(target) && dicMetadata[target] > 0)
                        Debug.Write($"Request Metadata {target:yyyy-MM-dd ddd} skip type1");
                    else
                        Debug.Write($"Request Metadata {target:yyyy-MM-dd ddd}");

                    DataTable table = null;
                    if (taskType != TaskType.TodayArchive) {
                        debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());

                        //件数が確定している場合はMetadataタイプ１はスキップ
                        JsonContent content = await disclosures.ReadDocuments(target, (dicMetadata != null && dicMetadata.ContainsKey(target) && dicMetadata[target] > 0) ? true : false, false);
                        debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                        bool error = CheckMetadataError(content, target);
                        if (error)
                            return;
                        if (content.StatusCode != null)
                            InvokeLabel($"metadata API {target:yyyy-MM-dd (ddd)}");
                        int wait0 = random.Next(Math.Min((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000)),
                                Math.Max((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000)));
                        await Task.Delay(wait0);
                        Debug.WriteLine($" {content.Metadata.Resultset.Count}件 delay{wait0}");

                        if (content.Metadata.Resultset.Count == 0)
                            continue;
                        table = content.Table;
                    } else {
                        table = disclosures.TableDocuments;
                        //table = disclosures.Database.ReadDisclosure(DateTime.Now.Date);
                    }
                    debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());
                    if (taskType == TaskType.Archive & !setting.OrderAscendingYear5)
                        sort = "id desc";
                    DataView dv = new DataView(table, "", sort, DataViewRowState.CurrentRows);
                    cancel = await DownloadTableList(token, dv, true);
                    if (cancel)
                        return;


                    debug.ProgramCodeInfo.SetDebugQueue(debug.ProgramCodeInfo.GetInfo());


                    //日付変更のウェイト
                    if (taskType == TaskType.Archive) {
                        int wait = random.Next(3600, 5200);
                        await Task.Delay(wait);
                    }
                    Debug.WriteLine("");


                }
            }
            Console.WriteLine("finish background download archive");
        }




    }
}