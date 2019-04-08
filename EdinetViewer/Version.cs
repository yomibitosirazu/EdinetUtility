using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Data.SQLite;

namespace EdinetViewer {

    //dummy デザイナー表示しないため
    public class Version { }

    partial class Form1 {

        private async Task VersionUp(string version) {
            InvokeVisible(true);
            switch (version) {
                case "0.2.101.0":
                case "0.2.101.1":
                case "0.2.101.2":
                    await Task.Run(() => Update02101());
                    break;
                default:
                    await Task.Run(() => test());
                    break;
            }

        }

        private void test() {
            List<DateTime> list = edinet.Database.MetadataList();
            InvokeProgressLabel(list.Count, 0, null);
            int i = 0;
            foreach (DateTime dt in list) {
                InvokeProgressLabel(0, i, dt.ToString("yyyy-MM-dd"));
                i++;
            }
            //Task.Delay(3000);
            InvokeProgressLabel(0, 0, "");
            //Task.Delay(2000);
            System.Threading.Thread.Sleep(1000);
            InvokeVisible(false);

        }

        private void Update02101() {
            InvokeProgressLabel(0, 0, "バージョンアップ　データベース更新中");
            string[] queries = new string[] {
                                "alter table `Disclosures` add column `date` text DEFAULT NULL;",
                                "update `Disclosures` set `date` = '20'||substr(id,1,2)||'-'||substr(id,3,2)||'-'||substr(id,5,2);",
                                "alter table `Disclosures` add column  `code` integer DEFAULT NULL;",
                                "drop index IF EXISTS `DisclosuresDate`;",
                                "CREATE INDEX IF NOT EXISTS `DisclosuresDate` on Disclosures(`date`)",
                                "drop index IF EXISTS `DisclosuresNumber`;",
                                "drop index IF EXISTS `DisclosuresXbrl`;",
                                "alter table `Disclosures` add column `status` text DEFAULT NULL;",
                                "update `Disclosures` set `status`='縦覧終了' where withdrawalStatus='0' and edinetCode is null;",
                                "update `Disclosures` set `status`='取下日' where withdrawalStatus='1';",
                                "update `Disclosures` set `status`='取下' where withdrawalStatus='2' and parentDocID is null;",
                                "update `Disclosures` set `status`='取下子' where withdrawalStatus='2' and parentDocID is not null;",
                                "update `Disclosures` set `status`='修正発生日' where docInfoEditStatus='1';",
                                "update `Disclosures` set `status`='修正' where docInfoEditStatus='2';",
                                "update `Disclosures` set `status`='不開示開始日' where disclosureStatus='1';",
                                "update `Disclosures` set `status`='不開示' where disclosureStatus='2';",
                                "update `Disclosures` set `status`='不開示解除日' where disclosureStatus='3';"

                            };
            int n = Disclosures.Const.DocTypeCode.Count + queries.Length;
            InvokeProgressLabel(n, 0, null);

            //Setting setting = new Setting();
            //string dbpath = Path.Combine(setting.Values["DocumentDirectory"], "edinet.db");
            string dbpath = Path.Combine(setting.Directory, "edinet.db");
            //File.WriteAllText("v0201.log", DateTime.Now.ToString() + "\r\n");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0}", dbpath))) {
                using (SQLiteCommand command = new SQLiteCommand(conn)) {

                    bool existDate = false;
                    bool existStatus = false;
                    bool existCode = false;
                    command.CommandText = "pragma table_info(`Disclosures`);";
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            while (reader.Read()) {
                                string name = reader.GetString(1);
                                if (name == "date")
                                    existDate = true;
                                else if (name == "status")
                                    existStatus = true;
                                else if (name == "code")
                                    existCode = true;
                            }
                        }
                    }
                    command.Connection.Close();
                    //int j = 0;
                    if (!existDate) {
                        for (int i = 0; i < 2; i++) {
                            command.CommandText = queries[i];
                            command.Connection.Open();
                            command.ExecuteNonQuery();
                            command.Connection.Close();
                            InvokeProgressLabel(0, i, null);
                        }
                        //j = 2;
                    }
                    if (!existCode) {
                            command.CommandText = queries[2];
                            command.Connection.Open();
                            command.ExecuteNonQuery();
                            command.Connection.Close();
                            InvokeProgressLabel(0, 2, null);
                            //j++;
                    }
                    for (int i = 3; i < 7; i++) {
                        command.CommandText = queries[i];
                        command.Connection.Open();
                        command.ExecuteNonQuery();
                        command.Connection.Close();
                        InvokeProgressLabel(0, i, null);
                        //j = i;
                    }
                    if (!existStatus) {
                        for (int i = 7; i < queries.Length; i++) {
                            command.CommandText = queries[i];
                            command.Connection.Open();
                            command.ExecuteNonQuery();
                            command.Connection.Close();
                            InvokeProgressLabel(0, i, null);
                            //j = i;
                        }
                    }

                    command.Connection.Open();
                    using (SQLiteTransaction ts = command.Connection.BeginTransaction()) {
                        int i = queries.Length;
                        foreach (var kv in Disclosures.Const.DocTypeCode) {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("update Disclosures set `docTypeCode` = '{0}' where `docTypeCode` = '{1}';", kv.Key, kv.Value);
                            command.CommandText = sb.ToString();
                            int count = command.ExecuteNonQuery();
                            //File.AppendAllText("v0201.log", string.Format("update doctype[{0}]:{1}\r\n", kv.Key, count));
                            InvokeProgressLabel(0, i, kv.Value);
                            i++;
                        }
                        ts.Commit();
                    }
                    command.Connection.Close();
                    //InvokeProgressLabel(0, 0, "バージョンアップ　データベースVACUUM");
                    //command.CommandText = "VACUUM;";
                    //command.Connection.Open();
                    //command.ExecuteNonQuery();
                    //command.Connection.Close();
                    //FormClosingでVacuumするように予約
                    setting.Values["Vacuum"] = DateTime.Now.AddDays(-10).ToString();
                }
            }
            Console.WriteLine("v201 db update {0:ss':'fff}", sw.Elapsed);
            InvokeProgressLabel(0, 0, "");
            InvokeVisible(false);
        }




        #region FirstTimeExecute
        private void SetTaxonomyDownloadEvent() {
            string[] files = Directory.GetFiles(setting.Directory, "ALL_*.zip");
            if (files.Length > 0) {
                Menu_Click(MenuImportTaxonomy, null);
            } else {
                browser.DocumentText = "<body><h1>EDINET Disclosure Viewer  <span style=\"font - size:small\">alpha version</span></h1>" +
                    "EDINETのダウンロードページから" +
                    "<ul><li>最新タクソノミー（00 . 全様式一括  ）</ul>をダウンロードする必要があります。<br>" +
                    "EDINETのトップページを下までスクロールすると【EDINETタクソノミ及びコードリスト 】のリンクがあります。" +
                    "リンクページのEDINETタクソノミの【00 . 全様式一括  】を展開して最新タクソノミを" +
                    string.Format("{0}にダウンロードまたはコピーしてください。", setting.Directory) +
                    "<br><br>フォルダにダウンロードしたzipアーカイブが作成されると自動的にデータベースを構築します。<br>" +
                    "それまではAPIを利用できますが書類のタクソノミのラベル解析はできません。" +
                    "<br><br>「OK」ボタンをクリックするとEDINETページに移動します。</body>";
                string message = "XBRLの解析にはタクソノミをデータベースにインポートする必要があります。\r\n" +
                    "EDINETのトップページ下方のダウンロードリンク【EDINETタクソノミ及びコードリスト 】から【00 . 全様式一括  】のうち最新タクソノミをダウンロードして" +
                    string.Format("フォルダ{0}にコピーしてください。\r\n", setting.Directory) +
                    "コピーが完了すると自動的にデータベースを構築しますが数分時間がかかります。" +
                    "それまではAPIを利用できますが書類のタクソノミのラベル解析はできません。";


                MessageBox.Show(message, "はじめに");
                page = Page.First;
                browser.Navigate(toppage);
                fileSystemWatcher = new FileSystemWatcher(setting.Directory) {
                    NotifyFilter = NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };
                fileSystemWatcher.Created += FileSystemWatcher_Created;
                //fileSystemWatcher.Changed += FileSystemWatcher_Created;
            }
        }


        private async void FileSystemWatcher_Created(object sender, FileSystemEventArgs e) {
            string[] files = Directory.GetFiles(setting.Directory, "ALL_*.zip");
            if (files.Length == 0)
                return;
            Array.Reverse(files);
            if (fileSystemWatcher != null) {
                fileSystemWatcher.EnableRaisingEvents = false;
                fileSystemWatcher.Created -= FileSystemWatcher_Created;
                fileSystemWatcher.Dispose();
                fileSystemWatcher = null;

                await BackGroundStart(TaskType.Taxonomy, files[0]);
                //backgroundTask[2] =
                //taskBackground = StartTask(TaskType.Taxonomy, files[0]);
                //await taskBackground;
                ////taskBackground = null;
            }
        }

        #endregion


    }



}
