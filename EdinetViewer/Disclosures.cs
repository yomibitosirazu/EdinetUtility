using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System.Data;
//using System.ComponentModel;
using System.Threading.Tasks;
//System.Runtime.Serialization 参照追加が必要
//using System.Runtime.Serialization.Json;
//using System.Runtime.Serialization;


namespace Disclosures {


    public class ApiListResult {
        public string Source { get; set; }
        public string Content { get; set; }
        public int StatusCode { get; set; }
        public string StatusText { get; set; }
        public Json.JsonList Json { get; set; }
        public Exception Exception { get; set; }
        public ApiListResult(string source, string content, HttpStatusCode statuscode) {
            Exception = null;
            Source = source;
            Content = content;
            StatusCode = (int)statuscode;
            StatusText = statuscode.ToString();
            if (Json == null)
                Json = new Json.JsonList();
            Json.Deserialize(source);
        }
        public ApiListResult(Exception ex) {
            Exception = ex;
        }
        public void Clear() {
            Exception = null;
            Source = null;
            Content = null;
            StatusCode = 0;
            StatusText = null;
            Json.Clear();
        }
    }
    public class ApiArchiveResult {
        public byte[] Buffer;
        public string Name;
        public string Content;
        public int StatusCode;
        public string StatusText { get; set; }
        public Api.ContentType ContentType { get; set; }
        public Json.JsonDocumentError Error { get; private set; }
        public Exception Exception { get; set; }
        public ApiArchiveResult(string content, HttpStatusCode? statuscode, byte[] buffer, string filename) {
            Clear();
            Content = content;
            if (statuscode != null) {
                StatusCode = (int)statuscode;
                StatusText = statuscode.ToString();
            }
            Buffer = buffer;
            Name = filename;
            ContentType = (Api.ContentType)Enum.ToObject(typeof(Api.ContentType), Array.IndexOf(Api.ContentTypes, content));
            if (ContentType == Api.ContentType.Fail) {
                string source = Encoding.ASCII.GetString(buffer);
                if (Error == null)
                    Error = new Json.JsonDocumentError();
                Error.Deserialize(source);
            }
        }
        public ApiArchiveResult(Exception ex) {
            Exception = ex;
        }
        public void Clear() {
            Content = null;
            StatusCode = 0;
            StatusText = null;
            Buffer = null;
            Name = null;
            if (Error != null)
                Error.Clear();
        }
    }

    //Apiクラスを1つにまとめた  リクエストとレスポンスだけに変更
    public class Api {
        public enum ContentType { Zip, Pdf, Fail };
        public static string[] ContentTypes { get { return new string[] { "application/octet-stream", "application/pdf", "application/json; charset=utf-8" }; } }//Zip,Pdf,Fail

        private readonly string baseUrl = "https://disclosure.edinet-fsa.go.jp/";
        private static HttpClient client;
        public string Version { get; set; }
        public Api(string version) {
            Version = version;
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
            client.DefaultRequestHeaders.Add("Accept-Language", "ja-JP");
            client.BaseAddress = new Uri(baseUrl);
        }
        protected async Task<ApiListResult> GetDocumentsList(DateTime date, int type = 2) {
            string url = string.Format("/api/{0}/documents.json?date={1:yyyy-MM-dd}{2}", Version, date, type == 2 ? "&type=2" : "");
            try {
                using (var response = await client.GetAsync(url)) {
                    System.Net.Http.Headers.MediaTypeHeaderValue contenttype = response.Content.Headers.ContentType;
                    string source = await response.Content.ReadAsStringAsync();
                    ApiListResult result = new ApiListResult(source, contenttype.ToString(), response.StatusCode);
                    return result;
                }
            //} catch (HttpRequestException ex) {
            //    Console.WriteLine(ex.Message);
            //    if (ex.Message.Contains("リモート名を解決できませんでした") | ex.InnerException != null && ex.InnerException.Message.Contains("リモート名を解決できませんでした"))
            //        return null;
            //    else
            //        throw;
            } catch (Exception ex) {
                ApiListResult result = new ApiListResult(ex);
                return result;
                //Console.WriteLine(ex.Message);
                //throw;
            }

        }
        protected async Task<ApiArchiveResult> DownloadArchive(string docid, int type) {
            string url = string.Format("/api/{0}/documents/{1}?type={2}", Version, docid, type);
            try {
                using (HttpResponseMessage res = await client.GetAsync(url)) {
                    using (Stream stream = await res.Content.ReadAsStreamAsync()) {
                        using (MemoryStream ms = new MemoryStream()) {
                            string filename = res.Content.Headers.ContentDisposition.FileName.Replace("\"", "");
                            stream.CopyTo(ms);
                            byte[] buffer = ms.ToArray();
                            stream.Flush();
                            System.Net.Http.Headers.MediaTypeHeaderValue contenttype = res.Content.Headers.ContentType;
                            ContentType content = (ContentType)Enum.ToObject(typeof(ContentType), Array.IndexOf(ContentTypes, contenttype.ToString()));
                            ApiArchiveResult result = new ApiArchiveResult(contenttype.ToString(), res.StatusCode, buffer, filename);
                            return result;
                        }
                    }
                }
            } catch (Exception ex) {
                return new ApiArchiveResult(ex);
            }
        }
        public void Dispose() {
            client.Dispose();
        }
    }

    //子クラスとしてXbrl（Taxonomy含む）、Database（Sqlite3）
    //このインスタンスを介してこれらにアクセスする
    public class Edinet : Api {

        public Database.Sqlite Database { get; set; }
        public Xbrl Xbrl { get; private set; }
        private readonly string[] doctype = new string[] { "xbrl", "pdf", "attach", "english" };
        private string cachedirectory;
        public DataTable TableDocuments;
        public DataView DvDocuments;
        public DataTable TableContents;
        public DataView DvContents;
        public DataTable TableElements;
        public ApiListResult ListResult { get; private set; }
        public ApiArchiveResult ArchiveResult { get; private set; }
        //public Json.JsonList Json { get; private set; }
        public string[] Types { get; private set; }
        public Dictionary<string, int> DicEdinetCode { get; set; }

        public bool IsCacheList { get; private set; }
        public bool IsCacheArchive { get; private set; }
        public string ApiVersion { get; private set; }
        public Edinet(string directory, string apiversion = "v1") : base(apiversion) {
            cachedirectory = directory;
            ApiVersion = apiversion;
            Database = new Database.Sqlite(Path.Combine(directory, "edinet.db"));
            Database.LoadTaxonomy(out Dictionary<string, string> dic, out List<string> list);
            Xbrl = new Xbrl();
            Xbrl.IntializeTaxonomy(dic, list);
            InitializeTables();
            Database.ReadEdinetCodelist(out Dictionary<string, int> dicCode);
            DicEdinetCode = dicCode;

        }
        private void InitializeTables() {
            TableDocuments = new DataTable();
            TableDocuments.Columns.Add(Disclosures.Const.FieldName.ElementAt(0).Key, typeof(int));
            for (int i = 1; i < Disclosures.Const.FieldName.Count; i++)
                TableDocuments.Columns.Add(Disclosures.Const.FieldName.ElementAt(i).Key, typeof(string));
            DataColumn colId = new DataColumn("id", typeof(int));
            TableDocuments.Columns.Add(colId);
            TableDocuments.Columns.Add("xbrl", typeof(string));
            TableDocuments.Columns.Add("pdf", typeof(string));
            TableDocuments.Columns.Add("attach", typeof(string));
            TableDocuments.Columns.Add("english", typeof(string));
            TableDocuments.Columns.Add("date", typeof(DateTime));
            TableDocuments.Columns.Add("status", typeof(string));
            TableDocuments.Columns.Add("タイプ", typeof(string));
            //TableDocuments.Columns.Add("new", typeof(string));
            TableDocuments.Columns.Add("code", typeof(int));
            TableDocuments.PrimaryKey = new DataColumn[] { colId };
            DvDocuments = new DataView(TableDocuments, "", "id desc", DataViewRowState.CurrentRows);
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
        public bool ChangeCacheDirectory(string dir) {
            bool exists = false;
            if (dir != cachedirectory) {
                cachedirectory = dir;
                string dbpath = Path.Combine(dir, "edinet.db");
                exists = File.Exists(dbpath);
                Database.ChangeDirectory(dbpath);
            }
            return exists;
        }

        private void UpdateDocumentsTable(ref DataTable table) {
            TableDocuments.Rows.Clear();
            for (int i = 0; i < table.Rows.Count; i++) {
                DataRow r = TableDocuments.NewRow();
                for (int j = 0; j < TableDocuments.Columns.Count; j++) {
                    if (table.Columns.Contains(TableDocuments.Columns[j].ColumnName)) {
                        r[TableDocuments.Columns[j].ColumnName] = table.Rows[i][TableDocuments.Columns[j].ColumnName];
                        if (TableDocuments.Columns[j].ColumnName == "docTypeCode") {
                            string docTypeCode = table.Rows[i][TableDocuments.Columns[j].ColumnName].ToString();
                            if (Const.DocTypeCode.ContainsKey(docTypeCode))
                                r["タイプ"] = Const.DocTypeCode[docTypeCode];
                        }
                    }
                }
                TableDocuments.Rows.Add(r);
            }
        }
        private void LoadDocuments(DateTime target) {
            string query = string.Format("select * from Disclosures where date(`date`) = '{0:yyyy-MM-dd}';", target);
            DataTable table = Database.ReadQuery(query);
            UpdateDocumentsTable(ref table);
            //TableDocuments.Rows.Clear();
            //for (int i = 0; i < table.Rows.Count; i++) {
            //    DataRow r = TableDocuments.NewRow();
            //    for (int j = 0; j < TableDocuments.Columns.Count; j++) {
            //        if (table.Columns.Contains(TableDocuments.Columns[j].ColumnName)) {
            //            r[TableDocuments.Columns[j].ColumnName] = table.Rows[i][TableDocuments.Columns[j].ColumnName];
            //            if (TableDocuments.Columns[j].ColumnName == "docTypeCode") {
            //                string docTypeCode = table.Rows[i][TableDocuments.Columns[j].ColumnName].ToString();
            //                if (Const.DocTypeCode.ContainsKey(docTypeCode))
            //                    r["タイプ"] = Const.DocTypeCode[docTypeCode];
            //            }
            //        }
            //    }
            //    TableDocuments.Rows.Add(r);
            //}
        }

        private bool CheckException(Exception ex) {
            if (ex != null) {
                string message = ex.Message;
                if (ex.GetType() == typeof(HttpRequestException)) {
                    if (ex.InnerException != null)
                        message += " " + ex.InnerException.Message;
                    //if (ex.Message.Contains("リモート名を解決できませんでした") |
                    //    ex.InnerException != null &&
                    //    ex.InnerException.Message.Contains("リモート名を解決できませんでした")) {
                    //    //リモート名を解決できませんでした インターネット接続なし
                    //    //throw new Exception("インターネット接続を確認してください");
                    //    //LoadDocuments(target);
                    //    //return result;
                    //    message = "リモート名を解決できませんでした";
                    //} else {

                    //}
                }
                string log = string.Format("{0:yyyy-MM-dd HH:mm:ss.f} DocumentListAPI:{1} {2}\r\n", DateTime.Now,
                    "", message);
                SaveLog(log);
                return true;
            } else
                return false;
        }
        /*閲覧終了した書類は全てnullになるので、
        ①過去データをデータベースから読み込み
        ②APIでJSONを取得
        */
        public async Task<ApiListResult> GetDisclosureList(DateTime target, bool background = false) {
            ApiListResult result = null;
            int count = 0;
            if (background) {
                result = await GetDocumentsList(target);
                bool error = CheckException(result.Exception);
                if (error) {
                    return result;
                }
                Json.JsonList json = new Json.JsonList();
                json.Deserialize(result.Source);
                count = json.Root.MetaData.Resultset.Count;
                //DataTable table = TableDocuments.Clone();
                //ListToTable(ref table, json);
                Database.UpdateDisclosures(target, json);
                //request = true;
            } else {
                object[] meta = Database.GetMetadata(target);
                bool request = meta == null || ((DateTime)meta[0]).Date == target.Date ? true : false;
                TableContents.Rows.Clear();
                TableElements.Rows.Clear();
                if (ListResult != null)
                    ListResult.Clear();
                if (!request) {
                    LoadDocuments(target);
                }

                if (request) {
                    result = await GetDocumentsList(target);
                    //bool error = CheckException(result.Exception);
                    if (CheckException(result.Exception)) {
                        LoadDocuments(target);
                        return result;
                    }
                    result.Json.Deserialize(result.Source);
                    if (DicEdinetCode != null) {
                        for (int i = 0; i < result.Json.Root.Results.Length; i++) {
                            if (result.Json.Root.Results[i].EdinetCode != null &&
                            DicEdinetCode.ContainsKey(result.Json.Root.Results[i].EdinetCode))
                                result.Json.Root.Results[i].Code = DicEdinetCode[result.Json.Root.Results[i].EdinetCode];
                        }
                    }
                    count = result.Json.Root.MetaData.Resultset.Count;
                    ListResult = result;
                    DataTable table = Database.UpdateDisclosures(target, ListResult.Json);
                    UpdateDocumentsTable(ref table);

                }
                List<string> list = new List<string>();
                foreach (DataRow r in TableDocuments.Rows) {
                    string doctype = r["docTypeCode"].ToString();
                    if (doctype != "") {
                        if (!list.Contains(Disclosures.Const.DocTypeCode[doctype]))
                            list.Add(Disclosures.Const.DocTypeCode[doctype]);
                    }
                }
                List<string> list2 = new List<string>() { "" };
                foreach (string s in Disclosures.Const.DocTypeCode.Values)
                    if (list.Contains(s))
                        list2.Add(s);
                Types = list2.ToArray();
            }
            if (result != null) {
                string log = string.Format("{0:yyyy-MM-dd HH:mm:ss.f} DocumentListAPI:{1} {2}\tcount:{3}\r\n", DateTime.Now,
                    result.StatusCode, target.ToString("yyyy-MM-dd"), count);
                SaveLog(log);
                SaveCache(target, ref result);
            }
            
            return result;


        }

        //書類一覧APIレスポンスのJSONはファイルに上書き保存
        private void SaveCache(DateTime target, ref ApiListResult result) {
            if (result.Json.Root.MetaData.Status == "200") {
                if (!Directory.Exists(Path.Combine(cachedirectory, "Json")))
                    Directory.CreateDirectory(Path.Combine(cachedirectory, "Json"));
                string dirJson = Path.Combine(cachedirectory, "Json", target.Year.ToString());
                if (!Directory.Exists(dirJson))
                    Directory.CreateDirectory(dirJson);
                string filepath = Path.Combine(dirJson, target.ToString("yyyyMMdd") + ".json");
                File.WriteAllText(filepath, result.Source);
            }
        }


        public async Task<ApiArchiveResult> DownloadArchive(int id, string docid, int type) {
            ApiArchiveResult result = await DownloadArchive(docid, type);
            if (CheckException(result.Exception))
                return result;
            int year = 20 * 100 + id / 100000000;
            //string filepath = string.Format(@"{0}\Documents\{1}\{2}", cachedirectory, year, result.Name);
            ContentType content = (ContentType)Enum.ToObject(typeof(ContentType), Array.IndexOf(ContentTypes, result.Content));
            if (content != ContentType.Fail) {
                SaveFile(result.Buffer, result.Name, year);
                Database.UpdateFilenameOfDisclosure(id, doctype[type - 1], result.Name);
            } else {
                if (result.Error != null && result.Error.Root != null) {
                    Database.UpdateFilenameOfDisclosure(id, doctype[type - 1], result.Error.Root.MetaData.Status);
                }
            }
            string log = string.Format("{0:yyyy-MM-dd HH:mm:ss.f} ArchiveAPI status:{1} {2} {3}\tContent-Type:{4}[{5}]\r\n", DateTime.Now, result.StatusCode, id, result.Name, content.ToString(), result.Content);
            SaveLog(log);
            return result;
        }
        public async Task<bool> ChangeDocument(int id, string docid, int type) {
            TableContents.Rows.Clear();
            int year = 20 * 100 + id / 100000000;
            string filepath = string.Format(@"{0}\Documents\{1}\{2}_{3}.{4}", cachedirectory, year, docid, type, type == 2 ? "pdf" : "zip");
            bool exists = File.Exists(filepath);
            if (exists) {
                LoadCache(filepath);
            } else {
                
                ArchiveResult = await this.DownloadArchive(id, docid, type);
            }

            if (ArchiveResult.Buffer != null && type != 2) {
                using (MemoryStream stream = new MemoryStream(ArchiveResult.Buffer)) {
                    using (ZipArchive archive = new ZipArchive(stream)) {
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

            return !exists;
        }
        //ローカルに保存
        private void SaveFile(byte[] buffer, string name, int year) {
            using (MemoryStream stream = new MemoryStream(buffer)) {
                string dir = Path.Combine(cachedirectory, "Documents", year.ToString());
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string filepath = string.Format(@"{0}\{1}", dir, name);
                using (FileStream fs = new FileStream(filepath, FileMode.Create)) {
                    stream.Position = 0;
                    stream.CopyTo(fs);
                }
            }
        }

        private void SaveLog(string log) {
            string logfile = Path.Combine(cachedirectory, "EdinetApi.log");
            File.AppendAllText(logfile, log);
        }

        //private void CheckUpdate() {

        //}
        //private bool Equals(Json.JsonList.Result result1, Json.JsonList.Result result2) {
        //    bool equal = true;
        //    equal = equal & result1.attachDocFlag == result2.attachDocFlag;
        //    equal = equal & result1.currentReportReason == result2.currentReportReason;
        //    equal = equal & result1.disclosureStatus == result2.disclosureStatus;
        //    equal = equal & result1.docDescription == result2.docDescription;
        //    equal = equal & result1.docID == result2.docID;
        //    equal = equal & result1.docInfoEditStatus == result2.docInfoEditStatus;
        //    equal = equal & result1.docTypeCode == result2.docTypeCode;
        //    equal = equal & result1.edinetCode == result2.edinetCode;
        //    equal = equal & result1.englishDocFlag == result2.englishDocFlag;
        //    equal = equal & result1.filerName == result2.filerName;
        //    equal = equal & result1.formCode == result2.formCode;
        //    equal = equal & result1.fundCode == result2.fundCode;
        //    equal = equal & result1.issuerEdinetCode == result2.issuerEdinetCode;
        //    equal = equal & result1.JCN == result2.JCN;
        //    equal = equal & result1.opeDateTime == result2.opeDateTime;
        //    equal = equal & result1.ordinanceCode == result2.ordinanceCode;
        //    equal = equal & result1.parentDocID == result2.parentDocID;
        //    equal = equal & result1.pdfFlag == result2.pdfFlag;
        //    equal = equal & result1.periodEnd == result2.periodEnd;
        //    equal = equal & result1.periodStart == result2.periodStart;
        //    equal = equal & result1.secCode == result2.secCode;
        //    equal = equal & result1.seqNumber == result2.seqNumber;
        //    equal = equal & result1.subjectEdinetCode == result2.subjectEdinetCode;
        //    equal = equal & result1.submitDateTime == result2.submitDateTime;
        //    equal = equal & result1.subsidiaryEdinetCode == result2.subsidiaryEdinetCode;
        //    equal = equal & result1.withdrawalStatus == result2.withdrawalStatus;
        //    equal = equal & result1.xbrlFlag == result2.xbrlFlag;
        //    return equal;
        //}



        //private void ListToTable(ref DataTable table, Json.JsonList json) {
        //    DataView dv = new DataView(table, "", "id", DataViewRowState.CurrentRows);
        //    DateTime target = DateTime.Parse(json.Root.metadata.parameter.date);
        //    //EdinetCode=null withdrawalStatus='0' 縦覧期間終了
        //    List<string> list = new List<string>();
        //    for (int i = 0; i < json.Root.results.Length; i++) {
        //        //for (int i = json.Root.results.Length - 1; i >= 0; i--) {
        //        int id = int.Parse(target.ToString("yyMMdd")) * 10000 + json.Root.results[i].seqNumber;
        //        string status = GetStatus(json.Root.results[i]);
        //        int index = dv.Find(id);
        //        if (index > -1) {
        //            if (status != null) {
        //                dv[index].BeginEdit();
        //                dv[index]["status"] = status;
        //                dv[index]["new"] = "change";
        //                dv[index].EndEdit();
        //            }
        //            continue;
        //        }
        //        DataRowView r = dv.AddNew();  
        //        r[0] = json.Root.results[i].seqNumber;
        //        r[1] = json.Root.results[i].docID;
        //        r[2] = json.Root.results[i].edinetCode;
        //        r[3] = json.Root.results[i].secCode;
        //        r[4] = json.Root.results[i].JCN;
        //        r[5] = json.Root.results[i].filerName;
        //        r[6] = json.Root.results[i].fundCode;
        //        r[7] = json.Root.results[i].ordinanceCode;
        //        r[8] = json.Root.results[i].formCode;
        //        r[9] = json.Root.results[i].docTypeCode;
        //        if (json.Root.results[i].docTypeCode != null && !list.Contains(Disclosures.Const.DocTypeCode[json.Root.results[i].docTypeCode]))  
        //            list.Add(Disclosures.Const.DocTypeCode[json.Root.results[i].docTypeCode]);
        //        r[10] = json.Root.results[i].periodStart;
        //        r[11] = json.Root.results[i].periodEnd;
        //        r[12] = json.Root.results[i].submitDateTime;
        //        r[13] = json.Root.results[i].docDescription;
        //        r[14] = json.Root.results[i].issuerEdinetCode;
        //        r[15] = json.Root.results[i].subjectEdinetCode;
        //        r[16] = json.Root.results[i].subsidiaryEdinetCode;
        //        r[17] = json.Root.results[i].currentReportReason;
        //        r[18] = json.Root.results[i].parentDocID;
        //        r[19] = json.Root.results[i].opeDateTime;
        //        r[20] = json.Root.results[i].withdrawalStatus;
        //        r[21] = json.Root.results[i].docInfoEditStatus;
        //        r[22] = json.Root.results[i].disclosureStatus;
        //        if(json.Root.results[i].xbrlFlag=="1")
        //        r[23] = json.Root.results[i].xbrlFlag;
        //        if (json.Root.results[i].pdfFlag == "1")
        //            r[24] = json.Root.results[i].pdfFlag;
        //        if (json.Root.results[i].attachDocFlag == "1")
        //            r[25] = json.Root.results[i].attachDocFlag;
        //        if (json.Root.results[i].edinetCode == "1")
        //            r[26] = json.Root.results[i].englishDocFlag;
        //        if (json.Root.results[i].docTypeCode != null)
        //            r["タイプ"] = Disclosures.Const.DocTypeCode[json.Root.results[i].docTypeCode];
        //        else
        //            r["タイプ"] = null;
        //        r["date"] = DateTime.Parse(json.Root.metadata.parameter.date);
        //        r["id"] = id;
        //        if (status != null)
        //            r["status"] = status;
        //        r["new"] = "new";
        //        if (DicEdinetCode!=null & json.Root.results[i].edinetCode != null && DicEdinetCode.ContainsKey(json.Root.results[i].edinetCode))
        //            r["code"] = DicEdinetCode[json.Root.results[i].edinetCode];
        //        r.EndEdit();
        //        //table.Rows.Add(r);
        //    }
        //    List<string> list2 = new List<string>() { "" };
        //    foreach (string s in Disclosures.Const.DocTypeCode.Values)
        //        if (list.Contains(s))
        //            list2.Add(s);
        //    Types = list2.ToArray();
        //}
        //private string GetStatus(Json.JsonList.Result result) {
        //    if (result.edinetCode == null & result.withdrawalStatus == "0")
        //        return "縦覧終了";
        //    else if (result.withdrawalStatus == "1")
        //        return "取下日";
        //    else if (result.withdrawalStatus == "2" & result.edinetCode == null) {
        //        if (result.parentDocID != null)
        //            return "取下子";
        //        else
        //            return "取下";
        //    } else if (result.docInfoEditStatus == "1")
        //        return "修正発生日";
        //    else if (result.docInfoEditStatus == "2")
        //        return "修正";
        //    else if (result.disclosureStatus == "1")
        //        return "不開示開始日";
        //    else if (result.disclosureStatus == "2")
        //        return "不開示";
        //    else if (result.disclosureStatus == "3")
        //        return "不開示解除日";
        //    return null;
        //}

        public int SearchBrand(int code) {
            int count = Database.GetDocumentsCount(code);
            if (count > 0) {
                TableDocuments.Rows.Clear();
                Database.SearchBrand(code, ref TableDocuments);
                List<string> list = new List<string>() { "" };
                foreach (DataRow r in TableDocuments.Rows) {
                    if (r["docTypeCode"] != null && !list.Contains(Disclosures.Const.DocTypeCode[r["docTypeCode"].ToString()]))
                        list.Add(Disclosures.Const.DocTypeCode[r["docTypeCode"].ToString()]);

                }
                Types = list.ToArray();

            }
            return count;
        }


        private void LoadCache(string filepath) {
            if (ArchiveResult != null)
                ArchiveResult.Clear();
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read)) {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();
                ArchiveResult = new ApiArchiveResult(null, null, buffer, filepath);
            }
        }

        public void SelectContent(int row, out string source) {
            TableElements.Rows.Clear();
            string fullpath = DvContents[row]["fullpath"].ToString();
            try {
                source = ReadEntry(ArchiveResult.Buffer, fullpath);
                if (Path.GetExtension(fullpath) == ".xbrl" | Path.GetFileName(fullpath).Contains("ixbrl")) {
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
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

        public string ReadEntry(byte[] buffer, string fullpath) {
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
        //public string SaveImage(byte[] buffer, string entryFullName) {
        //    using (Stream st = new MemoryStream(buffer)) {
        //        using (var archive = new ZipArchive(st)) {
        //            foreach (ZipArchiveEntry entry in archive.Entries) {
        //                if (entry.FullName == entryFullName) {
        //                    using (Stream stream = entry.Open()) {


        //                        using (MemoryStream ms = new MemoryStream()) {
        //                            stream.CopyTo(ms);
        //                            using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms)) {
        //                                string extension = Path.GetExtension(entryFullName);
        //                                string dir = Path.Combine(cachedirectory, "temp");
        //                                if (!Directory.Exists(dir))
        //                                    Directory.CreateDirectory(dir);
        //                                string imagefile = Path.Combine(dir, "image" + extension);
        //                                image.Save(imagefile);
        //                                return imagefile;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}
        public string ExtractImageInArchive(string entryFullName, string dest) {
            using (Stream st = new MemoryStream(ArchiveResult.Buffer)) {
                using (var archive = new ZipArchive(st)) {
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        if (entry.FullName == entryFullName) {
                            using (Stream stream = entry.Open()) {
                                using (MemoryStream ms = new MemoryStream()) {
                                    stream.CopyTo(ms);
                                    using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms)) {
                                        string extension = Path.GetExtension(entryFullName);
                                        //string dir = Path.Combine(cachedirectory, "temp");
                                        //if (!Directory.Exists(dir))
                                        //    Directory.CreateDirectory(dir);
                                        string imagefile = Path.Combine(dest, "image" + extension);
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

        public string ExtractPdfInArchive(string entryFullName, string dest) {
            using (Stream st = new MemoryStream(ArchiveResult.Buffer)) {
                using (var archive = new ZipArchive(st)) {
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        if (entry.FullName == entryFullName) {
                            string filepath = string.Format(@"{0}\{1}",dest, entry.Name);
                            entry.ExtractToFile(filepath,true);
                            return filepath;
                        }
                    }
                }
            }
            return null;
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
