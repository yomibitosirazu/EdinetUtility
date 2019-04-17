using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System.Data;
using System.Threading.Tasks;
using System.Reflection;


namespace Edinet {

    class JsonContent {
        public DataTable Table { get; set; }
        public Json.Metadata Metadata { get; private set; }
        public Json.StatusCode StatusCode { get; private set; }
        public Exception Exception { get; private set; }
        public string OutputMessage { get; private set; }
        public int PrevCount { get; private set; }
        public JsonContent(DataTable table, Json.ApiResponse apiresponse, string outputMessage, int prevcount = 0) {
            Table = table;
            if (apiresponse != null) {
                Metadata = apiresponse.MetaData;
                StatusCode = apiresponse.Status;
            }
            OutputMessage = outputMessage;
            PrevCount = prevcount;
        }
        public JsonContent(Exception exception) {
            Exception = exception;
            OutputMessage = exception.Message;
            if (exception.InnerException != null)
                OutputMessage += "\t" + exception.InnerException.Message;
        }
    }

    class JsonReader {

        protected readonly RequestDocument apiDocument;
        public JsonReader(string dir, string version = "v1") {
            apiDocument = new RequestDocument(dir, version);
        }

        //public async Task<JsonContent> ApiRequest(DateTime target, RequestDocument.RequestType requestType) {
        //    if (target.Date > DateTime.Now | target.Date < DateTime.Now.Date.AddYears(-5))
        //        return null;
        //    JsonResponse response = await apiDocument.Request(target, requestType);
        //    if (response.Exception != null)
        //        return new JsonContent(response.Exception);
        //    else {
        //        DataTable table = JsonToTable(response.Json);
        //        return new JsonContent(table, response.Json);
        //        //return JsonToTable(response.Json);
        //    }

        //}



        //public JsonContent ReadJsonfile(string filepath) {
        //    using (Stream stream = File.OpenRead(filepath)) {
        //        JsonDeserializer json = new JsonDeserializer(stream);
        //        if (json.Response == null)
        //            return null;
        //        else {
        //            DataTable table = JsonToTable(json.Response);
        //            return new JsonContent(table, json);
        //        }
        //    }
        //}

        public DataTable JsonToTable(Json.ApiResponse json) {
            DataTable table = new DataTable();
            if (json.Documents != null) {
                for (int i = 0; i < json.Documents.Length; i++) {
                    PropertyInfo[] properties = json.Documents[i].GetType().GetProperties();
                    List<object> list = new List<object>();
                    if (i == 0) {
                        table = new DataTable();
                        foreach (PropertyInfo property in properties) {
                            Type type = property.PropertyType;
                            if (type.FullName.Contains("Nullable")) {
                                if (type.FullName.Contains("Int32"))
                                    type = typeof(int);
                                else if (type.FullName.Contains("DateTime"))
                                    type = typeof(DateTime);
                                else {

                                }
                            }
                            DataColumn column = new DataColumn(property.Name, type);
                            table.Columns.Add(column);
                        }
                        table.Rows.Clear();
                    }
                    foreach (PropertyInfo property in properties) {
                        list.Add(property.GetValue(json.Documents[i], null));
                    }
                    table.Rows.Add(list.ToArray());
                }
            }
            return table;
        }
        public DataTable JsonToTable(JsonDeserializer json) {
            DataTable table = new DataTable();
            if (json.Response.Documents != null) {
                for (int i = 0; i < json.Response.Documents.Length; i++) {
                    PropertyInfo[] properties = json.Response.Documents[i].GetType().GetProperties();
                    List<object> list = new List<object>();
                    if (i == 0) {
                        table = new DataTable();
                        foreach (PropertyInfo property in properties) {
                            Type type = property.PropertyType;
                            if (type.FullName.Contains("Nullable")) {
                                if (type.FullName.Contains("Int32"))
                                    type = typeof(int);
                                else if (type.FullName.Contains("DateTime"))
                                    type = typeof(DateTime);
                                else {

                                }
                            }
                            DataColumn column = new DataColumn(property.Name, type);
                            table.Columns.Add(column);
                        }
                        table.Rows.Clear();
                    }
                    foreach (PropertyInfo property in properties) {
                        list.Add(property.GetValue(json.Response.Documents[i], null));
                    }
                    table.Rows.Add(list.ToArray());
                }
            }
            return table;
        }

        public void Dispose() {
            apiDocument.Dispose();
        }
    }




    class Disclosures : JsonReader {
        private string directory;
        public byte[] Buffer { get; private set; }
        public Database.Sqlite Database { get; set; }
        public Xbrl Xbrl { get; private set; }
        //private readonly string[] doctype = new string[] { "xbrl", "pdf", "attach", "english" };
        public DataTable TableDocuments { get; private set; }
        public DataView DvDocuments { get; private set; }
        public DataTable TableContents { get; private set; }
        public DataView DvContents { get; private set; }
        public DataTable TableElements { get; private set; }
        public string[] Types { get; private set; }
        public Dictionary<string, int> DicEdinetCode { get; set; }

        public bool IsCacheList { get; private set; }
        public bool IsCacheArchive { get; private set; }
        public string ApiVersion { get; private set; }


        public Disclosures(string dir, string version = "v1") : base(dir, version) {
            Database = new Database.Sqlite(Path.Combine(dir, "edinet.db"));
            directory = dir;
            Database.LoadTaxonomy(out Dictionary<string, string> dic, out List<string> list);
            Xbrl = new Xbrl();
            Xbrl.IntializeTaxonomy(dic, list);
            InitializeTables();
            Database.ReadEdinetCodelist(out Dictionary<string, int> dicCode);
            DicEdinetCode = dicCode;
        }

        public async Task<JsonContent> ReadDocuments(DateTime target, bool show = true) {
#pragma warning disable IDE0059
            DataTable table = null;
#pragma warning restore IDE0059
            Json.Metadata prevmMtadata = Database.ReadMetadata(target);
            int count = prevmMtadata != null && prevmMtadata.Resultset != null ? prevmMtadata.Resultset.Count : 0;
            if (count > 0) {
                table = Database.ReadDisclosure(target);
                if (prevmMtadata.Status == "200" & DateTime.Parse(prevmMtadata.ProcessDateTime).Date > target.Date) {
                    //翌日以降アクセスで確定
                    if (show)
                        UpdateDocumentsTable(ref table);
                    return new JsonContent(table, null, "書類一覧キャッシュ");
                }
            } else {
                table = Database.GetTableClone("disclosures");
            }
            DataView dv = new DataView(table, "", "id", DataViewRowState.CurrentRows);
            //まずメタデータのみ取得
            JsonResponse resMetadata = await apiDocument.Request(target, RequestDocument.RequestType.Metadata);
            if (resMetadata.Exception != null)
                return new JsonContent(resMetadata.Exception);
            else {
                if (resMetadata.Json.MetaData.Resultset.Count > count) {
                    await Task.Delay(50);
                    JsonResponse resList = await apiDocument.Request(target, RequestDocument.RequestType.List);
                    if (resList.Exception != null)
                        return new JsonContent(resList.Exception);
                    else {
                        AddJson(resList.Json, ref dv);
                        Database.UpdateDisclosures(dv, resList.Json.MetaData);
                        if (show)
                            UpdateDocumentsTable(ref table);
                        string message = string.Format("status:{0} 新規[{3}]/計[{2}]({1})",
                            resList.EdinetStatusCode.Message, resList.Json.MetaData.ProcessDateTime,
                                resList.Json.MetaData.Resultset.Count, resList.Json.MetaData.Resultset.Count - count);
                        return new JsonContent(dv.Table, resList.Json, message, count);
                    }
                } else {
                    if (DateTime.TryParse(resMetadata.Json.MetaData.ProcessDateTime, out DateTime processDate) && processDate.Date > target.Date) {
                        Database.UpdateMetadata(resMetadata.Json.MetaData);
                    }
                    if (show)
                        UpdateDocumentsTable(ref table);
                    string message = string.Format("status:{0} 新規[なし]/計[{2}]({1})",
                        resMetadata.EdinetStatusCode.Message, resMetadata.Json.MetaData.ProcessDateTime,
                            resMetadata.Json.MetaData.Resultset.Count);
                    return new JsonContent(table, resMetadata.Json, message, count);
                }
            }

        }

        private void AddJson(Json.ApiResponse json, ref DataView dv) {
            int maxsavedId = dv.Count > 0 ? int.Parse(dv[dv.Count - 1]["id"].ToString()) : 0;
            dv.Table.Columns.Add("edit", typeof(string));
            if (json.Documents != null) {
                for (int i = 0; i < json.Documents.Length; i++) {
                    PropertyInfo[] properties = json.Documents[i].GetType().GetProperties();
                    List<string> fields = new List<string>();
                    foreach (PropertyInfo property in properties)
                        fields.Add(property.Name.ToLower());
                    //int seqNo = (int)properties[fields.IndexOf("seqnumber")].GetValue(json.Documents[i], null);
                    int id = json.Documents[i].Id;
                    if (id > maxsavedId) {
                        DataRowView r = dv.AddNew();
                        foreach (DataColumn column in dv.Table.Columns) {
                            int index = fields.IndexOf(column.ColumnName.ToLower());
                            if (index > -1) {
                                object value = properties[index].GetValue(json.Documents[i], null);
                                if (value == null)
                                    r[column.ColumnName] = DBNull.Value;
                                else
                                    r[column.ColumnName] = value;
                                if (column.ColumnName == "date" | (column.ColumnName == "code" & value != null) | (column.ColumnName == "status" & value != null)) {
                                    Console.Write("{0} {1} {2}", index, column.ColumnName, r[column.ColumnName]);
                                }
                            } else {
                                //Console.WriteLine("{0}", column.ColumnName);
                                //xbrl pdf attach english
                            }
                        }
                        r["edit"] = "new";
                        r.EndEdit();
                        Console.WriteLine();
                    } else {
                        int index = dv.Find(id);
                        string status = dv[index]["Status"].ToString();
                        if (status == "")
                            status = null;
                        if (status != json.Documents[i].Status) {
                            dv[index].BeginEdit();
                            dv[index]["status"] = json.Documents[i].Status;
                            //dv[index]["@edinetCode"] = json.Documents[i].EdinetCode;
                            if (json.Documents[i].WithdrawalStatus == null)
                                dv[index]["withdrawalStatus"] = DBNull.Value;
                            else
                                dv[index]["withdrawalStatus"] = json.Documents[i].WithdrawalStatus;
                            if (json.Documents[i].DocInfoEditStatus == null)
                                dv[index]["docInfoEditStatus"] = DBNull.Value;
                            else
                                dv[index]["docInfoEditStatus"] = json.Documents[i].DocInfoEditStatus;
                            if (json.Documents[i].DisclosureStatus == null)
                                dv[index]["disclosureStatus"] = DBNull.Value;
                            else
                                dv[index]["disclosureStatus"] = json.Documents[i].DisclosureStatus;
                            if (json.Documents[i].SubmitDateTime == null)
                                dv[index]["submitDateTime"] = DBNull.Value;
                            else
                                dv[index]["submitDateTime"] = json.Documents[i].SubmitDateTime;
                            if (json.Documents[i].OpeDateTime == null)
                                dv[index]["opeDateTime"] = DBNull.Value;
                            else
                                dv[index]["opeDateTime"] = json.Documents[i].OpeDateTime;
                            dv[index]["edit"] = "update";
                            dv[index].EndEdit();
                        }


                    }
                }
            }
        }


        public async Task<ArchiveResponse> DownloadArchive(int id, string docid, RequestDocument.DocumentType type) {
            ArchiveResponse response = await apiDocument.DownloadArchive(docid, type);
            if (response.Exception != null) {

            } else if (response.EdinetStatusCode != null && response.EdinetStatusCode.Status != "200") {
                Database.UpdateFilenameOfDisclosure(id, type.ToString(), response.EdinetStatusCode.Status);
            } else {
                int year = 20 * 100 + id / 100000000;
                SaveFile(response.Buffer, response.Filename, year);
                Database.UpdateFilenameOfDisclosure(id, type.ToString(), response.Filename);

            }
            return response;
        }
        private void SaveFile(byte[] buffer, string name, int year) {
            using (MemoryStream stream = new MemoryStream(buffer)) {
                string dir = Path.Combine(directory, "Documents", year.ToString());
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string filepath = string.Format(@"{0}\{1}", dir, name);
                using (FileStream fs = new FileStream(filepath, FileMode.Create)) {
                    stream.Position = 0;
                    stream.CopyTo(fs);
                }
            }
        }


        private void InitializeTables() {

            TableDocuments = Database.GetTableClone("Disclosures");
            TableDocuments.Columns.Add("タイプ", typeof(string));
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
            if (dir != directory) {
                directory = dir;
                string dbpath = Path.Combine(dir, "edinet.db");
                exists = File.Exists(dbpath);
                Database.ChangeDirectory(dbpath);
            }
            return exists;
        }

        private void UpdateDocumentsTable(ref DataTable table) {
            TableDocuments.Rows.Clear();
            List<string> list = new List<string>() { "" };
            for (int i = 0; i < table.Rows.Count; i++) {
                DataRow r = TableDocuments.NewRow();
                for (int j = 0; j < TableDocuments.Columns.Count; j++) {
                    if (table.Columns.Contains(TableDocuments.Columns[j].ColumnName)) {
                        r[TableDocuments.Columns[j].ColumnName] = table.Rows[i][TableDocuments.Columns[j].ColumnName];
                        if (TableDocuments.Columns[j].ColumnName == "docTypeCode") {
                            string docTypeCode = table.Rows[i][TableDocuments.Columns[j].ColumnName].ToString();
                            if (Const.DocTypeCode.ContainsKey(docTypeCode))
                                r["タイプ"] = Const.DocTypeCode[docTypeCode];
                            if (docTypeCode != "" && !list.Contains(Const.DocTypeCode[docTypeCode]))
                                list.Add(Const.DocTypeCode[docTypeCode]);
                        }
                    }
                }
                TableDocuments.Rows.Add(r);
            }
            if (list.Count > 0)
                Types = list.ToArray();
        }







        public async Task<ArchiveResponse> ChangeDocument(int id, string docid, RequestDocument.DocumentType type) {
            ArchiveResponse response = null;
            TableContents.Rows.Clear();
            int year = 20 * 100 + id / 100000000;
            string filepath = string.Format(@"{0}\Documents\{1}\{2}_{3}.{4}", directory, year, docid, (int)type, type ==  RequestDocument.DocumentType.Pdf ? "pdf" : "zip");
            bool exists = File.Exists(filepath);
            //byte[] buffer = null;
            if (exists) {
                Buffer = LoadCache(filepath);
                //response = new ArchiveResponse();
                //response.Update()
            } else {

                response = await this.DownloadArchive(id, docid, type);
                Buffer = response.Buffer;
            }

            if (Buffer != null && type !=  RequestDocument.DocumentType.Pdf) {
                using (MemoryStream stream = new MemoryStream(Buffer)) {
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

            return response;
        }



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




        public int SearchBrand(int code) {
            int count = Database.GetDocumentsCount(code);
            if (count > 0) {
                TableDocuments.Rows.Clear();
                DataTable table = Database.SearchBrand(code);
                UpdateDocumentsTable(ref table);
                List<string> list = new List<string>() { "" };
                foreach (DataRow r in TableDocuments.Rows) {
                    if (r["docTypeCode"] != null && !list.Contains(Const.DocTypeCode[r["docTypeCode"].ToString()]))
                        list.Add(Const.DocTypeCode[r["docTypeCode"].ToString()]);

                }
                Types = list.ToArray();

            }
            return count;
        }


        private byte[] LoadCache(string filepath) {
            byte[] buffer = null;
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read)) {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();
                //ArchiveResult = new ApiArchiveResult(null, null, buffer, filepath);
            }
            return buffer;
        }

        public void SelectContent(int row, out string source) {
            TableElements.Rows.Clear();
            string fullpath = DvContents[row]["fullpath"].ToString();
            try {
                source = ReadEntry(Buffer, fullpath);
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

        public string ExtractImageInArchive(string entryFullName, string dest) {
            using (Stream st = new MemoryStream(Buffer)) {
                using (var archive = new ZipArchive(st)) {
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        if (entry.FullName == entryFullName) {
                            using (Stream stream = entry.Open()) {
                                using (MemoryStream ms = new MemoryStream()) {
                                    stream.CopyTo(ms);
                                    using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms)) {
                                        string extension = Path.GetExtension(entryFullName);
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
            using (Stream st = new MemoryStream(Buffer)) {
                using (var archive = new ZipArchive(st)) {
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        if (entry.FullName == entryFullName) {
                            string filepath = string.Format(@"{0}\{1}", dest, entry.Name);
                            entry.ExtractToFile(filepath, true);
                            return filepath;
                        }
                    }
                }
            }
            return null;
        }
    }







}

