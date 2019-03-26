using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System.Data;
using System.ComponentModel;
//System.Runtime.Serialization 参照追加が必要
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;


namespace Disclosures {

    /*
     * EDINET WebAPIレスポンスのJSONをデシリアライズするためのクラス（標準のSystem.Runtime.Serialization.Jsonを利用）
     *書類取得エラーのレスポンスはJsonListでデシリアライズ可能だが、APIクラス同様にJsonDocumentErrorに分けてみた
     * プロパティの先頭が小文字のため名前指定の規則違反の警告がでるが、
     * System.Runtime.Serialization.Jsonの仕様を理解していない
     * とりあえず動くので将来修正の予定
    */
    public class JsonList {
        //refer to JSONをコピペしてC#のクラス生成  //https://blog.beachside.dev/entry/2016/04/15/193000
        [DataContract]
        public class Rootobject {
            [DataMember]
            public Metadata metadata { get; set; }
            [DataMember]
            public Result[] results { get; set; }
        }

        [DataContract]
        public class Metadata {
            [DataMember]
            public string title { get; set; }
            [DataMember]
            public Parameter parameter { get; set; }
            [DataMember]
            public Resultset resultset { get; set; }
            [DataMember]
            public string processDateTime { get; set; }
            [DataMember]
            public string status { get; set; }
            [DataMember]
            public string message { get; set; }
        }

        [DataContract]
        public class Parameter {
            [DataMember]
            public string date { get; set; }
            [DataMember]
            public string type { get; set; }
        }

        [DataContract]
        public class Resultset {
            [DataMember]
            public int count { get; set; }
        }

        [DataContract]
        public class Result {
            [DataMember]
            public int seqNumber { get; set; }
            [DataMember]
            public string docID { get; set; }
            [DataMember]
            public string edinetCode { get; set; }
            [DataMember]
            public string secCode { get; set; }
            [DataMember]
            public string JCN { get; set; }
            [DataMember]
            public string filerName { get; set; }
            [DataMember]
            public string fundCode { get; set; }
            [DataMember]
            public string ordinanceCode { get; set; }
            [DataMember]
            public string formCode { get; set; }
            [DataMember]
            public string docTypeCode { get; set; }
            [DataMember]
            public string periodStart { get; set; }
            [DataMember]
            public string periodEnd { get; set; }
            [DataMember]
            public string submitDateTime { get; set; }
            [DataMember]
            public string docDescription { get; set; }
            [DataMember]
            public string issuerEdinetCode { get; set; }
            [DataMember]
            public string subjectEdinetCode { get; set; }
            [DataMember]
            public string subsidiaryEdinetCode { get; set; }
            [DataMember]
            public string currentReportReason { get; set; }
            [DataMember]
            public string parentDocID { get; set; }
            [DataMember]
            public string opeDateTime { get; set; }
            [DataMember]
            public string withdrawalStatus { get; set; }
            [DataMember]
            public string docInfoEditStatus { get; set; }
            [DataMember]
            public string disclosureStatus { get; set; }
            [DataMember]
            public string xbrlFlag { get; set; }
            [DataMember]
            public string pdfFlag { get; set; }
            [DataMember]
            public string attachDocFlag { get; set; }
            [DataMember]
            public string englishDocFlag { get; set; }
        }

        public Rootobject Root { get; private set; }
        private DataContractJsonSerializer serializer;
        public JsonList() {
            serializer = new DataContractJsonSerializer(typeof(JsonList.Rootobject));
        }
        public void Deserialize(string source) {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(source), false)) {
                Root = serializer.ReadObject(stream) as JsonList.Rootobject;
            }
        }
    }

    public class JsonDocumentError {
        [DataContract]
        public class Rootobject {
            [DataMember]
            public Metadata metadata { get; set; }
        }
        [DataContract]
        public class Metadata {
            [DataMember]
            public string title { get; set; }
            [DataMember]
            public string status { get; set; }
            [DataMember]
            public string message { get; set; }
        }
        public Rootobject Root { get; private set; }
        private DataContractJsonSerializer serializer;
        public JsonDocumentError() {
            serializer = new DataContractJsonSerializer(typeof(JsonDocumentError.Rootobject));
        }
        public void Deserialize(string source) {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(source), false)) {
                Root = serializer.ReadObject(stream) as JsonDocumentError.Rootobject;
            }
        }
    }

    //子クラスとしてWebApi（JSON）、Xbrl（Taxonomy含む）、Database（Sqlite3）
    //このインスタンスを介してこれらにアクセスする
    public class Edinet {

        //書類一覧を取得するAPIクラス
        public class ApiList {
            public HttpStatusCode StatusCode { get; private set; }
            public string CacheDirectory { get; set; }
            public string Version { get; set; }
            public JsonList Json { get; private set; }
            public string Source { get; private set; }
            private Web.Client client;
            public ApiList(string directory, Web.Client webclient, string version = "v1") {
                CacheDirectory = directory;
                Version = version;
                Json = new JsonList();
                client = webclient;
            }
            public void Request(DateTime target) {
                string url = string.Format("/api/{0}/documents.json?date={1:yyyy-MM-dd}&type=2", Version, target);
                /* 【APIリクエスト】　書類一覧 */
                StatusCode = client.ReadSource(url);
                Source = client.Source;
                Json.Deserialize(Source);
                string log = string.Format("{0} DocumentListAPI:{1}[{2}]\tcount:{3}\t{4}\r\n", DateTime.Now, (int)StatusCode, StatusCode.ToString(), Json.Root.metadata.resultset.count, url);
                SaveLog(log);
                SaveCache(target);
            }
            private void SaveLog(string log) {
                string logfile = Path.Combine(CacheDirectory, "EdinetApi.log");
                File.AppendAllText(logfile, log);
            }
            //書類一覧APIレスポンスのJSONはファイルに上書き保存
            private void SaveCache(DateTime target) {
                if (Json.Root.metadata.status == "200") {
                    if (!Directory.Exists(Path.Combine(CacheDirectory, "Json")))
                        Directory.CreateDirectory(Path.Combine(CacheDirectory, "Json"));
                    string dirJson = Path.Combine(CacheDirectory, "Json", target.Year.ToString());
                    if (!Directory.Exists(dirJson))
                        Directory.CreateDirectory(dirJson);
                    File.WriteAllText(Path.Combine(dirJson, target.ToString("yyyyMMdd") + ".json"), Source);
                }
            }
        }

            //Xbrl、pdf等の書類バイナリーファイルをダウンロードするAPI
            //先にこちらのインスタンスを作成して、Client（baseのインスタンス）をApiListに渡すこと
        public class ApiDocument : Web.Client {
            public Web.Client Client { get { return base.Instance; } }
            public HttpStatusCode StatusCode { get; private set; }
            public string CacheDirectory { get; set; }
            public string Version { get; set; }
            public new byte[] Buffer { get; private set; }
            public JsonDocumentError Json { get; private set; }
            public string Filename { get; private set; }
            public enum ContentType { Zip, Pdf, Fail };
            public ContentType Content { get; private set; }
            public new string Source { get; private set; }
            public string DocID { get; private set; }
            public int Type { get; private set; }
            public DateTime Submit { get; private set; }
            public ApiDocument(string directory, string version = "v1", string logfile = null) : base("https://disclosure.edinet-fsa.go.jp/", logfile) {
                CacheDirectory = directory;
                Version = version;
                Json = new JsonDocumentError();
            }
            public new void Dispose() {
                base.Dispose();
            }
            public void Request(string docid, int type, DateTime submit) {
                Source = null;
                Json = null;
                Buffer = null;
                DocID = docid;
                Type = type;
                Submit = submit;
                string url = string.Format("/api/{0}/documents/{1}?type={2}", Version, docid, type);
                string[] types = new string[] { "application/octet-stream", "application/pdf", "application/json; charset=utf-8" };//Zip,Pdf,Fail
                /* 【APIリクエスト】　書類バイナリーダウンロード */
                using (HttpResponseMessage res = GetResponse(url)) {
                    using (Stream stream = res.Content.ReadAsStreamAsync().Result) {
                        using (MemoryStream ms = new MemoryStream()) {
                            Filename = res.Content.Headers.ContentDisposition.FileName.Replace("\"","");
                            stream.CopyTo(ms);
                            Buffer = ms.ToArray();
                            stream.Flush();
                            System.Net.Http.Headers.MediaTypeHeaderValue contenttype = res.Content.Headers.ContentType;
                            Content = (ContentType)Enum.ToObject(typeof(ContentType), Array.IndexOf(types, contenttype.ToString()));
                            if (Content == ContentType.Fail) {
                                Source = Encoding.ASCII.GetString(Buffer);
                                Json.Deserialize(Source);
                            } else {
                                SaveFile(type, submit.Year);
                            }
                            string log = string.Format("{0} ArchiveAPI:{1}[{2}]\tContent-Type:{3}\t{4}\r\n", DateTime.Now, (int)res.StatusCode, res.StatusCode.ToString(), contenttype, url);
                            SaveLog(log);
                            StatusCode = res.StatusCode;
                        }
                    }
                }
            }
            //ローカルに保存
            private void SaveFile(int type, int year) {
                using (MemoryStream stream = new MemoryStream(Buffer)) {
                    string dir = Path.Combine(CacheDirectory, "Documents", year.ToString());
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    string filepath = string.Format(@"{0}\{1}", dir, Filename);
                    using (FileStream fs = new FileStream(filepath, FileMode.Create)) {
                        stream.Position = 0;
                        stream.CopyTo(fs);
                    }
                }
            }

                private void SaveLog(string log) {
                string logfile = Path.Combine(CacheDirectory, "EdinetApi.log");
                File.AppendAllText(logfile, log);
            }
        }


        //ここからEdinetクラス本体の定義
        public Database.Sqlite Database { get; set; }
        public Xbrl Xbrl { get; private set; }
        private readonly string[] doctype = new string[] { "xbrl", "pdf", "attach", "english" };
        private readonly string cachedirectory;
        public ApiList apiList { get; private set; }
        public ApiDocument apiDocument { get; private set; }
        public DataTable TableDocuments;
        public DataView DvDocuments;
        public DataTable TableContents;
        public DataView DvContents;
        public DataTable TableElements;
        public string Source { get; private set; }
        public JsonList Json { get; private set; }
        //public Dictionary<string,object> Metadata { get; private set; }
        private byte[] buffer;
        public bool IsCacheList { get; private set; }
        public bool IsCacheArchive { get; private set; }
        public Edinet(string directory, string apiversion = "v1") {
            cachedirectory = directory;
            Database = new Database.Sqlite(Path.Combine(directory, "edinet.db"));
            Json = new JsonList();
            //Api = new WebApi(directory, apiversion);
            apiDocument = new ApiDocument(directory, apiversion, Path.Combine(directory, "HttpClientError.log"));
            apiList = new ApiList(directory, apiDocument.Client, apiversion);
            Database.LoadTaxonomy(out Dictionary<string, string> dic, out List<string> list);
            Xbrl = new Xbrl();
            Xbrl.IntializeTaxonomy(dic, list);
            InitializeTables();
        }
        private void InitializeTables() {
            TableDocuments = new DataTable();
            TableDocuments.Columns.Add(Disclosures.Const.FieldName.ElementAt(0).Key, typeof(int));
            for (int i = 1; i < Disclosures.Const.FieldName.Count; i++)
                TableDocuments.Columns.Add(Disclosures.Const.FieldName.ElementAt(i).Key, typeof(string));
            DvDocuments = new DataView(TableDocuments, "", "submitDateTime desc", DataViewRowState.CurrentRows);
            TableContents = new DataTable();
            TableContents.Columns.Add("type", typeof(string));
            TableContents.Columns.Add("folda", typeof(string));
            TableContents.Columns.Add("name", typeof(string));
            TableContents.Columns.Add("fullpath", typeof(string));
            TableContents.Columns.Add("no", typeof(int));
            TableElements = new DataTable();
            TableElements.Columns.Add("no", typeof(int));
            TableElements.Columns.Add("tag", typeof(string));
            TableElements.Columns.Add("ラベル", typeof(string));
            TableElements.Columns.Add("prefix", typeof(string));
            TableElements.Columns.Add("element", typeof(string));
            TableElements.Columns.Add("contextRef", typeof(string));
            TableElements.Columns.Add("sign", typeof(string));
            TableElements.Columns.Add("value", typeof(string));
            TableElements.Columns.Add("unitRef", typeof(string));
            TableElements.Columns.Add("decimals", typeof(string));
            TableElements.Columns.Add("nil", typeof(string));
            TableElements.Columns.Add("attributes", typeof(string));
            DvContents = new DataView(TableContents, "", "no", DataViewRowState.CurrentRows);
        }
        public void ImportTaxonomy() {
            Database.LoadTaxonomy(out Dictionary<string, string> dic, out List<string> list);
            if (dic.Count > 0) {
                Xbrl = new Xbrl();
                Xbrl.IntializeTaxonomy(dic, list);
            }
        }
        //APIを利用したらtrue cacheを読み込んだらfalse
        public bool GetDisclosureList(DateTime target, bool checkProcessDateTime = false) {
            TableContents.Rows.Clear();
            TableElements.Rows.Clear();
            bool request = true;
            if (checkProcessDateTime) {
                Nullable<DateTime> saved = Database.GetMetadataProcessDateTime(target);
                if (saved != null && saved.Value.Date > target.Date) {
                    string filejson = Path.Combine(cachedirectory, "Json", target.Year.ToString(), target.ToString("yyyyMMdd") + ".json");
                    if (File.Exists(filejson)) {
                        Source = File.ReadAllText(filejson);
                        Json.Deserialize(Source);
                        //Api.LoadJson(json);
                        request = false;
                    }
                }
            }
            if (request) {
                apiList.Request(target);
                Source = apiList.Source;
                Json = apiList.Json;
            }
            ListToTable(ref TableDocuments, Json);
            int savedSecNumber = Database.GetMaxSecNumber(target);
            if (request | Json.Root.metadata.resultset.count > savedSecNumber)
                Database.UpdateInsertDisclosures(Json);
            IsCacheList = !request;
            return request;
        }


        private void ListToTable(ref DataTable table, JsonList json) {
            table.Rows.Clear();
            for (int i = json.Root.results.Length - 1; i >= 0; i--) {
                DataRow r = table.NewRow();
                r[0] = json.Root.results[i].seqNumber;
                r[1] = json.Root.results[i].docID;
                r[2] = json.Root.results[i].edinetCode;
                r[3] = json.Root.results[i].secCode;
                r[4] = json.Root.results[i].JCN;
                r[5] = json.Root.results[i].filerName;
                r[6] = json.Root.results[i].fundCode;
                r[7] = json.Root.results[i].ordinanceCode;
                r[8] = json.Root.results[i].formCode;
                if (json.Root.results[i].docTypeCode != null)
                    r[9] = Disclosures.Const.DocTypeCode[json.Root.results[i].docTypeCode];
                else
                    r[9] = null;
                r[10] = json.Root.results[i].periodStart;
                r[11] = json.Root.results[i].periodEnd;
                r[12] = json.Root.results[i].submitDateTime;
                r[13] = json.Root.results[i].docDescription;
                r[14] = json.Root.results[i].issuerEdinetCode;
                r[15] = json.Root.results[i].subjectEdinetCode;
                r[16] = json.Root.results[i].subsidiaryEdinetCode;
                r[17] = json.Root.results[i].currentReportReason;
                r[18] = json.Root.results[i].parentDocID;
                r[19] = json.Root.results[i].opeDateTime;
                r[20] = json.Root.results[i].withdrawalStatus;
                r[21] = json.Root.results[i].docInfoEditStatus;
                r[22] = json.Root.results[i].disclosureStatus;
                r[23] = json.Root.results[i].xbrlFlag;
                r[24] = json.Root.results[i].pdfFlag;
                r[25] = json.Root.results[i].attachDocFlag;
                r[26] = json.Root.results[i].englishDocFlag;
                table.Rows.Add(r);
            }
        }
        public int SearchBrand(int code) {
            int count = Database.GetDocumentsCount(code);
            if (count > 0) {
                TableDocuments.Rows.Clear();
                Database.SearchBrand(code, ref TableDocuments);
            }
            return count;
        }

        public void SelectDocument(int row, out string filepath) {
            TableContents.Rows.Clear();
            buffer = null;
            DateTime submit = DateTime.Parse(DvDocuments[row]["submitDateTime"].ToString());
            int seqnumber = (int)DvDocuments[row]["seqNumber"];
            int id = int.Parse(string.Format("{0:yyMMdd}{1:0000}", submit, seqnumber));
            int type = 0;
            if (DvDocuments[row]["xbrlFlag"].ToString() == "1")
                type = 1;
            else if (DvDocuments[row]["pdfFlag"].ToString() == "1")
                type = 2;
            string name = Database.GetFilename(id, doctype[type - 1]);
            filepath = null;
            if(name !=null && name != "")
            filepath = string.Format(@"{0}\Documents\{1}\{2}", cachedirectory, submit.Year, name);
            if (name != null && name != "" & File.Exists(filepath)) {
                if (type != 2)
                    LoadCache(filepath);
                IsCacheArchive = true;
            } else {
                string docid = DvDocuments[row]["docID"].ToString();
                apiDocument.Request(docid, type, submit);
                buffer = apiDocument.Buffer;
                Database.UpdateFilenameOfDisclosure(id, doctype[type - 1], apiDocument.Filename);
                IsCacheArchive = false;
            }
            if (buffer != null && type != 2) {
                using (MemoryStream stream = new MemoryStream(buffer)) {
                    using (ZipArchive archive = new ZipArchive(stream))  {
                        int i = 0;
                        foreach (ZipArchiveEntry entry in archive.Entries) {
                            i++;
                            System.IO.FileInfo inf = new System.IO.FileInfo(entry.FullName);
                            string folda = null;
                            if (inf.FullName.Contains("PublicDoc")) {
                                folda = "PublicDoc";
                            } else if (inf.FullName.Contains("AuditDoc"))
                                folda = "AuditDoc";
                            else if (inf.FullName.Contains("Summary")) {
                                folda = "Summary";
                            } else if (inf.FullName.Contains("Attachment"))
                                folda = "Attachment";
                            DataRow r = TableContents.NewRow();
                            r["type"] = inf.Extension;
                            r["folda"] = folda;
                            r["name"] = entry.Name;
                            r["fullpath"] = entry.FullName;
                            r["no"] = i;
                            TableContents.Rows.Add(r);
                        }
                    }
                }
            }
        }
        private void LoadCache(string filepath) {
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read)) {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();
            }
        }

        public void SelectContent(int row, out string source) {
            TableElements.Rows.Clear();
            string fullpath = DvContents[row]["fullpath"].ToString();
            source = ReadEntry(fullpath);
            if(Path.GetExtension(fullpath) == ".xbrl" | Path.GetFileName(fullpath).Contains("ixbrl")) {
                Xbrl.Load(source, Path.GetFileName(fullpath).Contains("ixbrl"));
                if (Xbrl.Elements.Count > 0) {
                    int i = 1;
                    foreach (var element in Xbrl.Elements) {
                        DataRow r = TableElements.NewRow();
                        r["no"] = i;
                        r["tag"] = element.Tag;
                        r["ラベル"] = element.Label;
                        r["prefix"] = element.Prefix;
                        r["element"] = element.Name;
                        r["contextRef"] = element.ContextRef;
                        r["value"] = element.Value;
                        r["sign"] = element.Sign;
                        r["unitRef"] = element.UnitRef;
                        r["decimals"] = element.Decimals;
                        r["nil"] = element.Nil;
                        r["attributes"] = element.Attributes;
                        TableElements.Rows.Add(r);
                        i++;
                    }
                }
            }
        }

        public string ReadEntry(string fullpath) {
            using (MemoryStream stream = new MemoryStream(buffer)) {
                using (ZipArchive archive = new ZipArchive(stream)) {
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        if (entry.FullName == fullpath) {
                            return ReadEntry(entry);
                        }
                    }
                }
            }
            return null;
        }
        public string ReadEntry(ZipArchiveEntry entry) {
            Encoding enc = Encoding.UTF8;
            if (entry.Name.EndsWith(".txt", false, System.Globalization.CultureInfo.CurrentCulture)
                | entry.Name.EndsWith(".csv", false, System.Globalization.CultureInfo.CurrentCulture))
                enc = Encoding.GetEncoding("shift_jis");
            using (Stream stream = entry.Open()) {
                using (StreamReader reader = new StreamReader(stream, enc)) {
                    return reader.ReadToEnd();
                }

            }
        }
        //アーカイブ内の画像をWebBrowserで表示するためtempフォルダに一時保存
        public string SaveImage(string entryFullName) {
            using (Stream st = new MemoryStream(buffer)) {
                using (var archive = new ZipArchive(st)) {
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        if (entry.FullName == entryFullName) {
                            using (Stream stream = entry.Open()) {
                                using (MemoryStream ms = new MemoryStream()) {
                                    stream.CopyTo(ms);
                                    using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms)) {
                                        string extension = Path.GetExtension(entryFullName);
                                        string dir = Path.Combine(cachedirectory, "temp");
                                        if (!Directory.Exists(dir))
                                            Directory.CreateDirectory(dir);
                                        string imagefile = Path.Combine(dir, "image" + extension);
                                        image.Save(imagefile);
                                        return imagefile;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        //同一のWebApiクラスのインスタンスを使用するとフォームから日付データを取得しようとした場合TableDocumentListの同時書き込みでDataTableが破損するためインスタンスを別にする
        //バックグラウンドとフォアグランドで同じHttpClientを利用するため同時にアクセスした場合の挙動は不明　もしかするとエラー起こすかも
        public ApiList ApiBackground { get; private set; }
        public void BackgroundGetDisclosureList(DateTime target) {
            if (ApiBackground == null)
                ApiBackground = new ApiList(cachedirectory, apiDocument, apiDocument.Version);

            /* 【APIリクエスト】　書類一覧 */
            ApiBackground.Request(target);
            JsonList json = ApiBackground.Json;
            Database.UpdateInsertDisclosures(json);
        }
        public void DisposeBackgroundApi() {
            //apiBackground.Dispose(); Dispose不要
            ApiBackground = null;
        }

        #region BackgroundProcess in EdinetClass
        //refer to : https://docs.microsoft.com/ja-jp/dotnet/framework/winforms/controls/how-to-implement-a-form-that-uses-a-background-operation   modified 
        public class BackgroundProcess {

            public enum ProcessType { PastList, ImportTaxonomy };
            private Edinet edinet;
            private readonly System.Windows.Forms.ToolStripProgressBar progressBar;
            private System.Windows.Forms.ToolStripStatusLabel progressLabel;
            private System.ComponentModel.BackgroundWorker worker;

            private ProcessType processType;
            public bool Busy { get; private set; }
            public BackgroundProcess(ref Edinet instanceedinet, ref System.Windows.Forms.ToolStripProgressBar progressbar, ref System.Windows.Forms.ToolStripStatusLabel label) {
                edinet = instanceedinet;
                
                progressBar = progressbar;
                progressLabel = label;
                this.worker = new System.ComponentModel.BackgroundWorker() {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(BackgroundWorker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
                worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(BackgroundWorker_ProgressChanged);
            }
            public void Dispose() {
                worker.Dispose();
            }

            public void StartAsync(ProcessType type, object param) {
                processType = type;
                progressLabel.Text = String.Empty;
                worker.RunWorkerAsync(param);
            }

            public void CancelAsync() {
                this.worker.CancelAsync();
            }

            private void BackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
                switch (processType) {
                    case ProcessType.PastList:
                        e.Result = RequestApi(sender as System.ComponentModel.BackgroundWorker, e);
                        break;
                    case ProcessType.ImportTaxonomy:
                        e.Result = UpdateTaxonomyFromArchive(e.Argument.ToString(), sender as System.ComponentModel.BackgroundWorker, e);
                        break;
                }
            }

            private void BackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) {
                if (e.Error != null) {
                    System.Windows.Forms.MessageBox.Show(e.Error.Message);
                } else if (e.Cancelled) {
                    if(progressLabel.Text.Contains("Error"))
                    progressLabel.Text = "Canceled";
                    Busy = false;
                } else {
                    progressLabel.Text = "finish " + processType.ToString();
                }

            }

            private void BackgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e) {
                progressBar.Value = e.ProgressPercentage;
            }

            private int RequestApi(System.ComponentModel.BackgroundWorker worker, System.ComponentModel.DoWorkEventArgs e) {
                Busy = true;
                List<DateTime> list = edinet.Database.MetadataList();
                DateTime min = DateTime.Now.AddYears(-5).Date;
                int count = (DateTime.Now.Date - min).Days;
                int offset = 1;
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                Random random = new Random();
                Stack<int> stack = new Stack<int>();
                while (DateTime.Now.Date.AddDays(-offset) >= DateTime.Now.AddYears(-5)) {
                    if (worker.CancellationPending) {
                        e.Cancel = true;
                        break;
                    }
                    if (list.Contains(DateTime.Now.AddDays(-offset).Date)) {
                        offset++;
                        continue;
                    }
                    int percentComplete = (offset + 1) * 100 / count;
                    if (percentComplete > 100) {
                        System.Windows.Forms.MessageBox.Show("ProgressBarのmaxumumを超えました");
                        break;
                    }
                    worker.ReportProgress(percentComplete);
                    DateTime target = DateTime.Now.AddDays(-offset).Date;
                    edinet.BackgroundGetDisclosureList(target);
                    progressLabel.Text = string.Format("{0:mm':'ss}経過 {1:yyyy-MM-dd} status[{2}]", sw.Elapsed, target,edinet.ApiBackground.StatusCode.ToString());
                    progressLabel.ForeColor = System.Drawing.Color.DarkBlue;
                    if ((int)edinet.ApiBackground.StatusCode != 200) {
                        if((int)edinet.ApiBackground.StatusCode == 404) {
                            if (stack.Count > 2 && stack.Peek() == 404) {
                                stack.Pop();
                                if (stack.Peek() == 404) {
                                    //3連続で404 Not Foundはおかしいだろう
                                    progressLabel.Text = "Error 404[Not Found] 3連続";
                                    //progressLabel.ToolTipText = Disclosures.Const.DescriptionStatusCode[(int)edinet.apiBackground.StatusCode];
                                    this.worker.CancelAsync();
                                } else
                                    stack.Push(404);
                            }
                        } else {
                            progressLabel.Text = string.Format("Error {0}[{1}]", (int)edinet.ApiBackground.StatusCode, (int)edinet.ApiBackground.StatusCode == 400 ? "Bad Request" : "Internal Server Error");
                            //progressLabel.ToolTipText = Disclosures.Const.DescriptionStatusCode[(int)edinet.apiBackground.StatusCode];
                            this.worker.CancelAsync();
                        }
                    }
                    stack.Push((int)edinet.ApiBackground.StatusCode);
                    offset++;
                    int wait = random.Next(1300, 2200);
                    System.Threading.Thread.Sleep(wait);
                    progressLabel.ForeColor = System.Drawing.Color.Black;
                }
                sw.Stop();
                edinet.DisposeBackgroundApi();
                Busy = false;
                return offset;
            }

            private int UpdateTaxonomyFromArchive(string archivefile, System.ComponentModel.BackgroundWorker worker, System.ComponentModel.DoWorkEventArgs e) {
                Busy = true;
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
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        string filename = entry.FullName;
                        if (filename.IndexOf("taxonomy") == 0 & Path.GetExtension(filename) == ".xml") {
                            if (worker.CancellationPending) {
                                e.Cancel = true;
                                break;
                            }
                            i++;
                            int percentComplete = (i) * 100 / count;
                            worker.ReportProgress(percentComplete);
                            progressLabel.Text = string.Format("{0}/{1} {2}", i, count, entry.Name);
                            string source = edinet.Database.Read(entry);
                            edinet.Database.ImportTaxonomy(filename, source, archivename);
                        }
                    }
                }
                progressLabel.Text = "finish " + processType.ToString();
                Busy = false;
                return i;
            }
            #endregion
        }

    }



    //仕様書からコード関係
    public static class Const {
        //府令コード
        private static Dictionary<string, string> ordinanceCode;
        public static Dictionary<string, string> OrdinanceCode {
            get {
                if (ordinanceCode == null) {
                    ordinanceCode = new Dictionary<string, string>() {
                    { "010", "企業内容等の開示に関する内閣府令" },
                    { "015", "財務計算に関する書類その他の情報の適正性を確保するための体制に関する内閣府令" },
                    { "020", "外国債等の発行者の開示に関する内閣府令" },
                    { "030", "特定有価証券の内容等の開示に関する内閣府令" },
                    { "040", "発行者以外の者による株券等の公開買付けの開示に関する内閣府令" },
                    { "050", "発行者による上場株券等の公開買付けの開示に関する内閣府令" },
                    { "060", "株券等の大量保有の状況の開示に関する内閣府令" }
                };
                }
                return ordinanceCode;
            }
        }
        //様式コード
        private static Dictionary<string, string> docTypeCode;
        public static Dictionary<string, string> DocTypeCode {
            get {
                if (docTypeCode == null) {
                    docTypeCode = new Dictionary<string, string>() {
                        { "010", "有価証券通知書"},
                        { "020", "変更通知書(有価証券通知書)"},
                        { "030", "有価証券届出書"},
                        { "040", "訂正有価証券届出書"},
                        { "050", "届出の取下げ願い"},
                        { "060", "発行登録通知書"},
                        { "070", "変更通知書(発行登録通知書)"},
                        { "080", "発行登録書"},
                        { "090", "訂正発行登録書"},
                        { "100", "発行登録追補書類"},
                        { "110", "発行登録取下届出書"},
                        { "120", "有価証券報告書"},
                        { "130", "訂正有価証券報告書"},
                        { "135", "確認書"},
                        { "136", "訂正確認書"},
                        { "140", "四半期報告書"},
                        { "150", "訂正四半期報告書"},
                        { "160", "半期報告書"},
                        { "170", "訂正半期報告書"},
                        { "180", "臨時報告書 " },
                        { "190", "訂正臨時報告書 " },
                        { "200", "親会社等状況報告書 " },
                        { "210", "訂正親会社等状況報告書 " },
                        { "220", "自己株券買付状況報告書 " },
                        { "230", "訂正自己株券買付状況報告書" },
                        { "235", "内部統制報告書 " },
                        { "236", "訂正内部統制報告書 " },
                        { "240", "公開買付届出書 " },
                        { "250", "訂正公開買付届出書 " },
                        { "260", "公開買付撤回届出書 " },
                        { "270", "公開買付報告書 " },
                        { "280", "訂正公開買付報告書 " },
                        { "290", "意見表明報告書 " },
                        { "300", "訂正意見表明報告書 " },
                        { "310", "対質問回答報告書 " },
                        { "320", "訂正対質問回答報告書 " },
                        { "330", "別途買付け禁止の特例を受けるための申出書" },
                        { "340", "訂正別途買付け禁止の特例を受けるための申出書" },
                        { "350", "大量保有報告書 " },
                        { "360", "訂正大量保有報告書 " },
                        { "370", "基準日の届出書 " },
                        { "380", "変更の届出書 " }
                    };
                }
                return docTypeCode;
            }
        }

        private static Dictionary<string, string> fieldName;
        public static Dictionary<string, string> FieldName {
            get {
                if (fieldName == null) {
                    fieldName = new Dictionary<string, string>() {
                        { "seqNumber",          "連番" },
                        { "docID",              "提出書類番号" },
                        { "edinetCode",         "提出者EDINETコード" },
                        { "secCode",            "提出者証券コード" },
                        { "JCN",                "提出者法人番号" },
                        { "filerName",          "提出者名" },
                        { "fundCode",           "ファンドコード" },
                        { "ordinanceCode",      "府令コード" },
                        { "formCode",           "様式コード" },
                        { "docTypeCode",        "書類識別コード" },
                        { "periodStart",        "期間（自）" },
                        { "periodEnd",          "期間（至）" },
                        { "submitDateTime",     "提出日時" },
                        { "docDescription",     "提出書類概要" },
                        { "issuerEdinetCode",   "発行会社EDINETコード" },
                        { "subjectEdinetCode",  "対象EDINETコード" },
                        { "subsidiaryEdinetCode", "子会社EDINETコード" },
                        { "currentReportReason", "臨報提出事由" },
                        { "parentDocID",        "親書類管理番号" },
                        { "opeDateTime",        "操作日時" },
                        { "withdrawalStatus",   "取下区分" },
                        { "docInfoEditStatus",  "書類情報修正区分" },
                        { "disclosureStatus",   "開示不開示区分" },
                        { "xbrlFlag",           "XBRL有無フラグ" },
                        { "pdfFlag",            "PDF有無フラグ" },
                        { "attachDocFlag",      "代替書面・添付文書有無フラグ" },
                        { "englishDocFlag",     "英文ファイル有無フラグ" }
                    };
                }
                return fieldName;
            }
        }
        private static Dictionary<string, string> fieldHeader;
        public static Dictionary<string, string> FieldHeader {
            get {
                if (fieldHeader == null) {
                    fieldHeader = new Dictionary<string, string>() {
                        { "seqNumber",          "連番" },
                        { "docID",              "DocID" },
                        { "edinetCode",         "EDINETcode" },
                        { "secCode",            "SecCode" },
                        { "JCN",                "法人番号" },
                        { "filerName",          "提出者名" },
                        { "fundCode",           "FundCode" },
                        { "ordinanceCode",      "府令" },
                        { "formCode",           "様式" },
                        { "docTypeCode",        "識別" },
                        { "periodStart",        "自" },
                        { "periodEnd",          "至" },
                        { "submitDateTime",     "提出" },
                        { "docDescription",     "概要" },
                        { "issuerEdinetCode",   "発行" },
                        { "subjectEdinetCode",  "対象" },
                        { "subsidiaryEdinetCode", "子会社" },
                        { "currentReportReason", "事由" },
                        { "parentDocID",        "親" },
                        { "opeDateTime",        "操作" },
                        { "withdrawalStatus",   "下" },
                        { "docInfoEditStatus",  "修" },
                        { "disclosureStatus",   "不" },
                        { "xbrlFlag",           "XBRL" },
                        { "pdfFlag",            "PDF" },
                        { "attachDocFlag",      "代替" },
                        { "englishDocFlag",     "英文" }
                    };
                }
                return fieldHeader;
            }
        }
        private static Dictionary<int, string> descriptionStatusCode;
        public static Dictionary<int, string> DescriptionStatusCode {
            get {
                if (descriptionStatusCode == null) {
                    descriptionStatusCode = new Dictionary<int, string>() {
                        { 200, "OK" },
                        { 400, "リクエスト内容が誤っています。\r\nリクエストの内容（エンドポイント、パラメータの形式等）を見直してください。"},
                        { 404, "データが取得できません。パラメータの設定値を見直してください。\r\n書類取得API の場合、対象の書類が非開示となっている可能性があります。"},
                        { 500, "EDINET のトップページ又は金融庁ウェブサイトの各種情報検索サービスにてメンテナンス等の情報を確認してください。"}
                    };
                }
                return descriptionStatusCode;
            }
        }

        private static Dictionary<string, string> formCode;
        public static Dictionary<string, string> FormCode {
            get {
                if (formCode == null) {
                    Database.Sqlite.LoadFormCodes(out Dictionary<string, string> dic, @"Resources\FormCodes.txt");
                    formCode = dic;
                }
                return formCode;
            }
        }


    }

}
