using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System.Data;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace Edinet {

    class JsonContent {
        public DataTable Table { get; set; }
        public Json.Metadata Metadata { get; private set; }
        public Json.StatusCode StatusCode { get; private set; }
        public Exception Exception { get; private set; }
        public string OutputMessage { get; set; }
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
        public JsonReader(string dir, string useragent, string version) {
            apiDocument = new RequestDocument(dir, version, useragent);
        }



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
        //public byte[] Buffer { get; private set; }
        public Database.Sqlite Database { get; set; }
        public Xbrl Xbrl { get; private set; }
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


        private readonly Archive.Zip zip;

        public Disclosures(string dir, string useragent, string version) : base(dir, useragent, version) {
            directory = dir;
            CheckLogSize();
            Database = new Database.Sqlite(Path.Combine(dir, "edinet.db"));
            Database.LoadTaxonomy(out Dictionary<string, string> dic, out List<string> list);
            Xbrl = new Xbrl(dic, list);
            zip = new Archive.Zip(dir);
            InitializeTables();
            Database.ReadEdinetCodelist(out Dictionary<string, int> dicCode);
            DicEdinetCode = dicCode;
        }

        private void InitializeTables() {

            TableDocuments = Database.GetTableClone("Disclosures");
            TableDocuments.Columns.Add("タイプ", typeof(string));
            DvDocuments = new DataView(TableDocuments, "", "id desc", DataViewRowState.CurrentRows);

            TableContents = zip.Table;
            DvContents = new DataView(TableContents, "", "no", DataViewRowState.CurrentRows);

            TableElements = Xbrl.ToTable();

        }

        public void ImportTaxonomy() {
            Database.LoadTaxonomy(out Dictionary<string, string> dic, out List<string> list);
            if (dic.Count > 0) {
                //Xbrl = new Xbrl();
                //Xbrl.IntializeTaxonomy(dic, list);
                Xbrl = new Xbrl(dic, list);
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


        private void CheckLogSize() {
            string logfile = Path.Combine(directory, "EdinetApi.log");
            if (File.Exists(logfile)) {
                long size = (new FileInfo(logfile)).Length;
                double mb = size / 1024f / 1024f;
                if (mb > 1) {
                    string[] files = Directory.GetFiles(directory, "EdinetApi*.log");
                    string backupname = $"EdinetApi{files.Length:00}.log";
                    File.Move(logfile, Path.Combine(directory, backupname));
                }
            }
        }

        //private JsonContent UpdateListView(DataTable table, Json.ApiResponse jsonResponse, string message, int count, bool show) {
        //    if (show) {
        //        TableDocuments = table;
        //        DvDocuments = new DataView(TableDocuments, "", "id desc", DataViewRowState.CurrentRows);
        //    }
        //    return new JsonContent(table, jsonResponse, message, count);
        //}

        //private JsonResponse CheckJson(JsonResponse response, bool diff) {
        //    if (response.ReturnResult == ResponseResult.Success) {
        //        DateTime processT = DateTime.Parse(response.Json.MetaData.ProcessDateTime);
        //        int count = response.Json.MetaData.Resultset.Count;
        //        DateTime target = DateTime.Parse(response.Json.MetaData.Parameter.Date);
        //        if (diff) {
        //            Json.Metadata prevMetadata = Database.ReadMetadata(target);
        //            if (prevMetadata != null && prevMetadata.Status == "200") {
        //                DateTime processPrev = DateTime.Parse(prevMetadata.ProcessDateTime);
        //                int countPrev = prevMetadata.Resultset.Count;
        //                if (processPrev != null && processT == processPrev)
        //                    response.ReturnResult = ResponseResult.SameProcess;
        //                else if (count == countPrev)
        //                    response.ReturnResult = ResponseResult.SameCount;
        //            }
        //        }
        //        if (count == 0)
        //            response.ReturnResult = ResponseResult.Zero;
        //    }
        //    return response;
        //}
        private JsonResponse CheckJsonDiff(JsonResponse response, Json.Metadata prevMetadata) {
            if (response.ReturnResult == ResponseResult.Success) {
                DateTime processT = DateTime.Parse(response.Json.MetaData.ProcessDateTime);
                int count = response.Json.MetaData.Resultset.Count;
                DateTime target = DateTime.Parse(response.Json.MetaData.Parameter.Date);
                if (prevMetadata == null)
                    prevMetadata = Database.ReadMetadata(target);
                if (prevMetadata != null && prevMetadata.Status == "200") {
                    DateTime processPrev = DateTime.Parse(prevMetadata.ProcessDateTime);
                    int countPrev = prevMetadata.Resultset.Count;
                    if (processPrev != null && processT == processPrev)
                        response.ReturnResult = ResponseResult.SameProcess;
                    else if (count == countPrev)
                        response.ReturnResult = ResponseResult.SameCount;
                } else {

                }

                if (count == 0)
                    response.ReturnResult = ResponseResult.Zero;
            }
            return response;
        }


        //private async Task<JsonResponse> ReadMetadataType1(DateTime target) {
        //    JsonResponse response = await apiDocument.Request(target, RequestDocument.RequestType.Metadata);
        //    if (response.ReturnResult == ResponseResult.Success) {
        //        response = CheckJson(response, true);
        //    }
        //    return response;
        //}
        private async Task<JsonResponse> ReadMetadataType1(DateTime target, Json.Metadata prevMetadata, int retry) {
            JsonResponse response = await apiDocument.Request(target, RequestDocument.RequestType.Metadata, retry);
            if (response.ReturnResult == ResponseResult.Success) {
                response = CheckJsonDiff(response, prevMetadata);
            }
            return response;
        }
        private async Task<JsonContent> ReadMetadataType2(DateTime target, int retry) {
            DataTable table = Database.ReadDisclosure(target);
            DataView dv = new DataView(table, "", "id", DataViewRowState.CurrentRows);
            JsonResponse resList = await apiDocument.Request(target, RequestDocument.RequestType.List, retry);

            Debug.Write($"{DateTime.Now.TimeOfDay} metadatalist({target:yyyy-MM-dd}) readed");
            switch (resList.ReturnResult) {
                case ResponseResult.Success:
                    AddJson(resList.Json, ref dv);
                    Database.UpdateDisclosures(dv, resList.Json.MetaData);
                    //return new JsonContent(dv.Table, resList.Json, resList.Json.MetaData.Message, resList.Json.MetaData.Resultset.Count);

                    break;
                case ResponseResult.SameProcess:
                case ResponseResult.SameCount:
                case ResponseResult.Zero:
                    //return new JsonContent(dv.Table, resList.Json, resList.Json.MetaData.Message, resList.Json.MetaData.Resultset.Count);
                    break;
                case ResponseResult.NotFound:
                    return new JsonContent(dv.Table, resList.Json, resList.Json.MetaData.Message, 0);
                case ResponseResult.Timeout:
                    break;
                case ResponseResult.Invalid://error
                case ResponseResult.BadRequest://BadRequest
                case ResponseResult.ServerError://InternalServerError
                case ResponseResult.Exception:
                    return null;
                    //case int (int) n when n >= 400 & n < 500:
                    //break;
            }

            return new JsonContent(dv.Table, resList.Json, resList.Json.MetaData.Message, resList.Json.MetaData.Resultset.Count);
        }

        private Json.Metadata CheckKakuteiAndSkip(DateTime target, bool showTable) {
            Json.Metadata prevMetadata = Database.ReadMetadata(target);
            int count = 0;
            if (prevMetadata != null && prevMetadata.Status == "200") {
                DateTime processTime = DateTime.Parse(prevMetadata.ProcessDateTime);
                //翌日以降アクセスで確定
                bool kakutei = processTime.Date > target.Date;
                if (prevMetadata.Resultset != null)
                    count = prevMetadata.Resultset.Count;
                if (kakutei) {
                    //確定でcount0件は追加されることはない　確定24hr以内はスキップ
                    if (showTable | DateTime.Now < processTime.AddHours(24) | count == 0)
                        return prevMetadata;
                }
            }
            //skipはnull
            return null;
        }
        public async Task<JsonContent> ReadDocuments(DateTime target, int retry, bool skipFirst = false, bool show = true) {
            debug.ProgramCodeInfo.SetDebugQueue();
            Json.Metadata prev = CheckKakuteiAndSkip(target, show);
            DataTable table = Database.ReadDisclosure(target);
            Json.ApiResponse apiResponse = new Json.ApiResponse() { MetaData = prev };
            if (prev != null) {

                if (show) {
                    TableDocuments = table;
                    DvDocuments = new DataView(TableDocuments, "", "id desc", DataViewRowState.CurrentRows);
                }
                return new JsonContent(table, apiResponse, "書類一覧キャッシュ", prev.Resultset.Count);
            }
            bool access = skipFirst;
            JsonResponse response = null;
            if (!skipFirst) {
                response = await ReadMetadataType1(target, prev, retry);
                if (response.ReturnResult == ResponseResult.Exception) {
                    JsonContent content = new JsonContent(response.Exception);
                    if (show) {
                        TableDocuments = table;
                        DvDocuments = new DataView(TableDocuments, "", "id desc", DataViewRowState.CurrentRows);
                    }
                    return content;
                } else if (response.ReturnResult == ResponseResult.Success) {
                    access = true;
                } else
                    access = false;

            }
            if (access) {
                JsonContent content = await ReadMetadataType2(target, retry);
                //DataView dv = new DataView(content.Table, "", "id", DataViewRowState.CurrentRows);
                //Database.UpdateDisclosures(dv, content.Metadata);
                string message = string.Format("status:{0} 新規[{3}]/計[{2}]({1})",
                    content.StatusCode.Message, content.Metadata.ProcessDateTime,
                       content.Metadata.Resultset.Count, content.Metadata.Resultset.Count - table.Rows.Count);
                content.OutputMessage = message;
                Debug.Write($" count:{content.Metadata.Resultset.Count}({content.Metadata.Resultset.Count - table.Rows.Count:+0;-0;0})");
                if (show) {
                    TableDocuments = content.Table;
                    DvDocuments = new DataView(TableDocuments, "", "id desc", DataViewRowState.CurrentRows);
                }
                return content;
            } else {
                Json.ApiResponse res = new Json.ApiResponse() {
                    MetaData = response.Json.MetaData,
                    Documents = response.Json.Documents
                };
                if (show) {
                    TableDocuments = table;
                    DvDocuments = new DataView(TableDocuments, "", "id desc", DataViewRowState.CurrentRows);
                }
                return new JsonContent(table, response != null ? response.Json : res, "Metadata only", table.Rows.Count);
            }
        }

        //        public async Task<JsonContent> ReadDocumentsOrg(DateTime target, bool skipFirst = false, bool show = true) {
        //#pragma warning disable IDE0059
        //            DataTable table = null;
        //#pragma warning restore IDE0059
        //            Json.Metadata prevMetadata = Database.ReadMetadata(target);
        //            bool skip = false;
        //            int count = 0;
        //            if (prevMetadata != null && prevMetadata.Status == "200") {
        //                DateTime processTime = DateTime.Parse(prevMetadata.ProcessDateTime);
        //                //翌日以降アクセスで確定
        //                bool kakutei = processTime.Date > target.Date;
        //                if (prevMetadata.Resultset != null)
        //                    count = prevMetadata.Resultset.Count;
        //                if (count > 0)
        //                    table = Database.ReadDisclosure(target);
        //                if (kakutei) {
        //                    //確定でcount0件は追加されることはない　確定24hr以内はスキップ
        //                    if (show | DateTime.Now < processTime.AddHours(24) | count == 0)
        //                        skip = true;
        //                }

        //            }
        //            if (table == null)
        //                table = Database.ReadDisclosure(null);
        //                //table = Database.GetTableClone("disclosures");
        //            if (skip) {
        //                //if (show)
        //                //    TableDocuments = table;
        //                    //    UpdateDocumentsTable(ref table);
        //                    Json.ApiResponse apiResponse = new Json.ApiResponse() {
        //                    MetaData = prevMetadata
        //                };
        //                //apiResponse.Status = null;
        //                //return new JsonContent(table, apiResponse, "書類一覧キャッシュ");
        //                return UpdateListView(table, apiResponse, "書類一覧キャッシュ", count, show);
        //            }

        //            DataView dv = new DataView(table, "", "id", DataViewRowState.CurrentRows);
        //            JsonResponse resMetadata = null;
        //            if (!skipFirst) {
        //                resMetadata = await apiDocument.Request(target, RequestDocument.RequestType.Metadata,0);
        //                Debug.Write($"{DateTime.Now.TimeOfDay} metadata readed");
        //            }
        //            if (resMetadata != null && resMetadata.Exception != null)
        //                return new JsonContent(resMetadata.Exception);
        //            else {
        //                if (resMetadata != null && resMetadata.Json.Status.Status != "200") {
        //                    Debug.WriteLine($" {resMetadata.Json.Status.Status}");
        //                    return new JsonContent(dv.Table, resMetadata.Json, resMetadata.Json.Status.Status);
        //                    //return UpdateListView(dv.Table, resMetadata.Json, resMetadata.Json.Status.Status, 0, show);
        //                }
        //                if (resMetadata != null && DateTime.Parse(resMetadata.Json.MetaData.ProcessDateTime).Date > target & resMetadata.Json.MetaData.Resultset.Count == 0) {
        //                    Debug.WriteLine($" count:{resMetadata.Json.MetaData.Resultset.Count}");
        //                    Database.UpdateMetadata(resMetadata.Json.MetaData);
        //                    return new JsonContent(dv.Table, resMetadata.Json, "0");
        //                    //return UpdateListView(dv.Table, resMetadata.Json, "0", 0, show);
        //                }
        //                Debug.WriteLine("");
        //                if (resMetadata == null || target < DateTime.Now.Date | resMetadata.Json.MetaData.Resultset.Count > count) {
        //                    //await Task.Delay(50);
        //                    JsonResponse resList = await apiDocument.Request(target, RequestDocument.RequestType.List,0);
        //                    Debug.Write($"{DateTime.Now.TimeOfDay} metadatalist({target:yyyy-MM-dd}) readed");
        //                    if (resList.Exception != null) {
        //                        Debug.WriteLine($" exception:{resList.Exception}");
        //                        return new JsonContent(resList.Exception);
        //                    } else {
        //                        if(resList.Json.MetaData.Status == "404") {
        //                            return new JsonContent(dv.Table, resList.Json, resList.Json.MetaData.Message, 0);
        //                        }
        //                        AddJson(resList.Json, ref dv);
        //                        Database.UpdateDisclosures(dv, resList.Json.MetaData);
        //                        //if (show)
        //                        //    TableDocuments = table;
        //                        //    UpdateDocumentsTable(ref table);
        //                        string message = string.Format("status:{0} 新規[{3}]/計[{2}]({1})",
        //                            resList.EdinetStatusCode.Message, resList.Json.MetaData.ProcessDateTime,
        //                                resList.Json.MetaData.Resultset.Count, resList.Json.MetaData.Resultset.Count - count);
        //                        Debug.Write($" count:{resList.Json.MetaData.Resultset.Count}({resList.Json.MetaData.Resultset.Count - count:+0;-0;0})");
        //                        //return new JsonContent(dv.Table, resList.Json, message, count);
        //                        //return UpdateListView(table, resList.Json, message, count, show);
        //                        if (show) {
        //                            TableDocuments = table;
        //                            DvDocuments = new DataView(TableDocuments, "", "id desc", DataViewRowState.CurrentRows);
        //                        }
        //                        return new JsonContent(table, resList.Json, message, count);
        //                    }
        //                } else {
        //                    if (DateTime.TryParse(resMetadata.Json.MetaData.ProcessDateTime, out DateTime processDate) && processDate.Date > target.Date) {
        //                        Database.UpdateMetadata(resMetadata.Json.MetaData);
        //                    }
        //                    //if (show)
        //                    //    TableDocuments = table;
        //                    //    UpdateDocumentsTable(ref table);
        //                    string message = string.Format("status:{0} 新規[なし]/計[{2}]({1})",
        //                        resMetadata.EdinetStatusCode.Message, resMetadata.Json.MetaData.ProcessDateTime,
        //                            resMetadata.Json.MetaData.Resultset.Count);
        //                    Debug.Write($"metadata {message}");
        //                    //return UpdateListView(table, resMetadata.Json, message, count,show);
        //                    //return new JsonContent(table, resMetadata.Json, message, count);
        //                    if (show) {
        //                        TableDocuments = table;
        //                        DvDocuments = new DataView(TableDocuments, "", "id desc", DataViewRowState.CurrentRows);
        //                    }
        //                    return new JsonContent(table, resMetadata.Json, message, count);
        //                }
        //            }

        //        }

        private void AddJson(Json.ApiResponse json, ref DataView dv) {
            int maxsavedId = dv.Count > 0 ? int.Parse(dv[dv.Count - 1]["id"].ToString()) : 0;
            dv.Table.Columns.Add("edit", typeof(string));
            if (json.Documents != null) {
                for (int i = 0; i < json.Documents.Length; i++) {
                    Console.WriteLine($"{json.Documents[i].SeqNumber}  {json.Documents[i].DocDescription}");
                    PropertyInfo[] properties = json.Documents[i].GetType().GetProperties();
                    List<string> fields = new List<string>();
                    foreach (PropertyInfo property in properties)
                        fields.Add(property.Name.ToLower());
                    int id = json.Documents[i].Id;
                    if (id > maxsavedId) {
                        DataRowView r = dv.AddNew();
                        foreach (DataColumn column in dv.Table.Columns) {
                            try {
                                int index = fields.IndexOf(column.ColumnName.ToLower());
                                if (index > -1) {
                                    object value = properties[index].GetValue(json.Documents[i], null);
                                    if (value == null)
                                        r[column.ColumnName] = DBNull.Value;
                                    else
                                        r[column.ColumnName] = value;
                                    if (column.ColumnName == "docTypeCode" && value != null)
                                        r["タイプ"] = Const.DocTypeCode[value.ToString()];
                                }
                            } catch (Exception ex) {

                                Console.WriteLine(ex.ToString());
                            }
                        }
                        r["edit"] = "new";
                        r.EndEdit();
                    } else {
                        int index = dv.Find(id);
                        if (index < 0) {
                            DataRowView r = dv.AddNew();
                            System.Diagnostics.Debug.Write($"AddJson not in disclosures {id}\t");
                            foreach (DataColumn column in dv.Table.Columns) {
                                index = fields.IndexOf(column.ColumnName.ToLower());
                                if (index > -1) {
                                    object value = properties[index].GetValue(json.Documents[i], null);
                                    if (value == null)
                                        r[column.ColumnName] = DBNull.Value;
                                    else {
                                        r[column.ColumnName] = value;
                                        if (column.ColumnName == "docTypeCode" && Const.DocTypeCode.ContainsKey(value.ToString()))
                                            r["タイプ"] = Const.DocTypeCode[value.ToString()];
                                    }
                                    System.Diagnostics.Debug.Write($"{value ?? ""}\t");
                                }
                            }
                            System.Diagnostics.Debug.WriteLine("");
                            r["edit"] = "new";
                            r.EndEdit();
                        } else {
                            string status = dv[index]["Status"].ToString();
                            if (status == "")
                                status = null;
                            if (status != json.Documents[i].Status) {
                                dv[index].BeginEdit();
                                dv[index]["status"] = json.Documents[i].Status;
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
        }

        public async Task<HttpResponseMessage> RequestDownload(string docid, RequestDocument.DocumentType doctype) {
            return await apiDocument.RequestDownload(docid, doctype);
        }

#pragma warning disable CS1998
        public async Task DownloadArchiveNoAwait(int id, string docid, RequestDocument.DocumentType type, int retry) {
#pragma warning disable CS4014
            apiDocument.DownloadAsync(docid, type, id, Database, retry);
        }

        public async Task Download(HttpResponseMessage httpResponseMessage, int id, string type) {
            apiDocument.Download(httpResponseMessage, id, type, Database);
#pragma warning restore CS4014
        }
#pragma warning restore CS1998

        public async Task<ArchiveResponse> DownloadArchive(int id, string docid, RequestDocument.DocumentType type, int retry) {
            //int retry = 1;
            ArchiveResponse response = await apiDocument.DownloadArchive(docid, type, retry);
            if (response.Exception != null) {
                return response;
            } else if (response.EdinetStatusCode != null && response.EdinetStatusCode.Status != "200") {
                Database.UpdateFilenameOfDisclosure(id, type.ToString(), response.EdinetStatusCode.Status);
            } else {
                int year = 20 * 100 + id / 100000000;
                try {
                    SaveFile(response.Buffer, response.Filename, year);
                    Database.UpdateFilenameOfDisclosure(id, type.ToString(), response.Filename);

                } catch (Exception ex) {

                    throw (ex);
                }

            }
            return response;
        }
        public void UpdateArchiveStatus(int id, RequestDocument.DocumentType type, string status) {
            Database.UpdateFilenameOfDisclosure(id, type.ToString(), status);
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

        //private void SaveToArchive(string filedir, string name, int year) {
        //    //using (MemoryStream stream = new MemoryStream(buffer)) {
        //        string dir = Path.Combine(directory, "Documents", year.ToString());
        //        if (!Directory.Exists(dir))
        //            Directory.CreateDirectory(dir);
        //        string filepath = string.Format(@"{0}\{1}", dir, name);
        //        //using (FileStream fs = new FileStream(filepath, FileMode.Create)) {
        //        //    stream.Position = 0;
        //        //    stream.CopyTo(fs);
        //        //}
        //    //}
        //}

        public async Task<ArchiveResponse> ChangeDocumentAsync(int id, string docid, RequestDocument.DocumentType type, int retry) {
            ArchiveResponse response = null;
            bool exist = await zip.LoadAsync(id, docid, (int)type);
            if (!exist) {
                response = await this.DownloadArchive(id, docid, type, retry);
                zip.Load(response.Buffer, (int)type);
            }
            TableContents = zip.Table;
            DvContents = new DataView(TableContents, "", "", DataViewRowState.CurrentRows);
            return response;
        }


        public void SetDocumentTable(DataTable table) {
            //List<string> list = new List<string>() { "" };
            //foreach (DataRow r in table.Rows) {
            //    string docTypeCode = r["docTypeCode"].ToString();
            //    //if (Const.DocTypeCode.ContainsKey(docTypeCode))
            //    //    r["タイプ"] = Const.DocTypeCode[docTypeCode];
            //    if (docTypeCode != "" && !list.Contains(Const.DocTypeCode[docTypeCode]))
            //        list.Add(Const.DocTypeCode[docTypeCode]);
            //}
            //Types = list.ToArray();
            TableDocuments = table;
            DvDocuments = new DataView(TableDocuments, "", "id desc", DataViewRowState.CurrentRows);
        }

        public string SelectContent(int row, out string source) {
            TableElements.Rows.Clear();
            string fullpath = DvContents[row]["fullpath"].ToString();
            try {
                //source = ReadEntry(Buffer, fullpath);
                source = zip.Read(fullpath);
                if (Path.GetExtension(fullpath) == ".xbrl" | Path.GetFileName(fullpath).Contains("ixbrl")) {
                    Xbrl.Load(source);
                    if (Xbrl.NodeList.Count > 0) {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        TableElements = Xbrl.ToTable();
                        sw.Stop();
                        Console.WriteLine(sw.ElapsedMilliseconds);
                    }
                    return source;
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                throw;
            }
            return null;
        }

        public int SearchBrand(int code) {
            int count = Database.GetDocumentsCount(code);
            if (count > 0) {
                TableDocuments = Database.SearchBrand(code);
                DvDocuments = new DataView(TableDocuments, "", "id desc", DataViewRowState.CurrentRows);
            }
            return count;
        }



        public async Task<string> UpdateSummary(int id, bool updatedb = true) {
            DataView dv = new DataView(TableDocuments, "", "id", DataViewRowState.CurrentRows);
            //if (DvDocuments.Sort.ToLower().IndexOf("id") != 0)
            //    DvDocuments.Sort = "id desc";
            int index = dv.Find(id);
            if (index > -1 | updatedb) {
                string filepath = zip.Exists(id, dv[index]["docID"].ToString(), 1);
                //Archive.Zip zip = new Archive.Zip(directory);
                string source = await Archive.Zip.ReadXbrlSourceAsync(filepath);
                Xbrl xbrl = new Xbrl();
                xbrl.Load(source);
                string summary = xbrl.GetSummaryLargeVolume();
                if (index > -1) {
                    dv[index].BeginEdit();
                    dv[index]["summary"] = summary;
                    dv[index].EndEdit();
                }
                if (updatedb) {
                    Database.UpdateFieldOfDisclosure("summary", new Dictionary<int, string>() { { id, summary } });
                }
                return summary;
            } else
                return null;
        }


        public async Task<string> UpdateQuarterAsync(int id, bool updatedb = true) {
            DataView dv = new DataView(TableDocuments, "", "id", DataViewRowState.CurrentRows);
            int index = dv.Find(id);
            if (index > -1 | updatedb) {
                string filepath = zip.Exists(id, dv[index]["docID"].ToString(), 1);
                string source = await Archive.Zip.ReadXbrlSourceAsync(filepath);
                Xbrl xbrl = new Xbrl();
                xbrl.Load(source);
                string summary = xbrl.GetSummaryQuaterResult();
                //if (index > -1) {
                //    dv[index].BeginEdit();
                //    dv[index]["summary"] = summary;
                //    dv[index].EndEdit();
                //}
                //if (updatedb) {
                //    //Database.UpdateFieldOfDisclosure("summary", new Dictionary<int, string>() { { id, summary } });
                //}
                return summary;
            } else
                return null;
        }
        public string UpdateQuarter(int id, bool updatedb = true) {
            DataView dv = new DataView(TableDocuments, "", "id", DataViewRowState.CurrentRows);
            int index = dv.Find(id);
            if (index > -1 | updatedb) {
                string filepath = zip.Exists(id, dv[index]["docID"].ToString(), 1);
                string source = Archive.Zip.ReadXbrlSource(filepath);
                //Xbrl xbrl = new Xbrl();
                //xbrl.Load(source);
                Xbrl.Load(source);
                string summary = Xbrl.GetSummaryQuaterResult();
                return summary;
            } else
                return null;
        }







        public Dictionary<string, DateTime> ImportArchives(string filepath) {
            Dictionary<string, DateTime> dicAccessDocidDate = new Dictionary<string, DateTime>();
            string extension = Path.GetExtension(filepath).ToLower();
            //string dir = "temp";
            //if (!Directory.Exists(dir))
            //    Directory.CreateDirectory(dir);
            Dictionary<int, string> dicUpdate = new Dictionary<int, string>();
            Console.WriteLine(filepath);
            if (extension == ".zip") {
                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read)) {
                    using (MemoryStream stream = new MemoryStream()) {
                        fs.CopyTo(stream);
                        using (ZipArchive archive = new ZipArchive(stream)) {
                            Dictionary<string, List<ZipArchiveEntry>> dic = new Dictionary<string, List<ZipArchiveEntry>>();
                            foreach (ZipArchiveEntry entry in archive.Entries) {
                                //Console.WriteLine(entry.FullName);
                                string docid = entry.FullName.Split('/')[0];
                                if (docid.ToLower().Contains(".csv"))
                                    continue;
                                if (dic.ContainsKey(docid))
                                    dic[docid].Add(entry);
                                else
                                    dic[docid] = new List<ZipArchiveEntry>() { entry };
                            }
                            string filter = $"docid in ('{string.Join("', '", dic.Keys)}')";
                            DataTable table = Database.ReadDisclosure(filter);
                            DataView dv = new DataView(table, "", "docid", DataViewRowState.CurrentRows);

                            foreach (var kv in dic) {
                                string docid = kv.Key;
                                string sdate = "";
                                foreach (ZipArchiveEntry entry in kv.Value) {
                                    if (entry.Name.ToLower().EndsWith(".xbrl")) {
                                        string[] ss = entry.Name.Replace(".xbrl", "").Split('_');
                                        sdate = ss[ss.Length - 1];
                                    }
                                }
                                if (sdate != "" && DateTime.TryParse(sdate, out DateTime date)) {
                                    string dest = $"{directory}\\Documents\\{date.Year}\\{docid}_1.zip";
                                    if (!File.Exists(dest)) {
                                        int index = dv.Find(docid);
                                        if (index > -1) {
                                            dicUpdate.Add(int.Parse(dv[index]["id"].ToString()), $"{docid}_1.zip");
                                        } else {
                                            if (date >= DateTime.Now.Date.AddYears(-5))
                                                dicAccessDocidDate.Add(docid, date);
                                        }
                                        using (MemoryStream ms = new MemoryStream()) {
                                            using (ZipArchive zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true)) {
                                                foreach (ZipArchiveEntry entry in kv.Value) {
                                                    using (Stream st = entry.Open()) {
                                                        byte[] buffer;
                                                        using (var ms1 = new MemoryStream()) {
                                                            st.CopyTo(ms1);
                                                            buffer = ms1.ToArray();
                                                        }

                                                        string[] ss = entry.FullName.Split('/');
                                                        string path = string.Join("/", ss, 1, ss.Length - 1);
                                                        ZipArchiveEntry e = zipArchive.CreateEntry(path);

                                                        using (var es = e.Open()) {
                                                            es.Write(buffer, 0, buffer.Length);
                                                        }
                                                    }
                                                }
                                            }
                                            using (FileStream fstream = new FileStream(dest, FileMode.Create)) {
                                                ms.Position = 0;
                                                ms.CopyTo(fstream);
                                            }
                                        }
                                        Console.Write($"{docid} ");
                                    }
                                } else {

                                }
                            }
                        }
                    }
                }
            } else if (extension == ".csv") {
                string dir = Directory.GetParent(filepath).FullName;
                string[] subdirs = Directory.GetDirectories(dir);
                List<string> list = new List<string>();
                foreach (string subdir in subdirs) {
                    string docid = Path.GetFileName(subdir);
                    list.Add(docid);
                }
                string filter = $"docid in ('{string.Join("', '", list)}')";
                DataTable table = Database.ReadDisclosure(filter);
                DataView dv = new DataView(table, "", "docid", DataViewRowState.CurrentRows);
                foreach (string docid in list) {
                    string dest = "";
                    string subdir = Path.Combine(dir, docid);
                    string[] files = Directory.GetFiles(subdir, "*.xbrl", SearchOption.AllDirectories);
                    foreach (string file in files) {
                        if (file.Contains("PublicDoc")) {
                            using (StreamReader reader = new StreamReader(file)) {
                                string source = reader.ReadToEnd();
                                Edinet.Xbrl xbrl = new Edinet.Xbrl();
                                xbrl.Load(source);
                                string fn = Path.GetFileName(file);
                                string releasedate = xbrl.GetValue("FilingDateCoverPage");
                                if (releasedate != "" && DateTime.TryParse(releasedate, out DateTime date)) {
                                    dest = $"{directory}\\Documents\\{date.Year}\\{docid}_1.zip";
                                    if (File.Exists(dest))
                                        dest = "";
                                    else {
                                        int index = dv.Find(docid);
                                        if (index > -1) {
                                            dicUpdate.Add(int.Parse(dv[index]["id"].ToString()), $"{docid}_1.zip");
                                        } else {
                                            if (date >= DateTime.Now.Date.AddYears(-5))
                                                dicAccessDocidDate.Add(docid, date);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }

                    if (dest != null) {
                        string folda = $"{dir}\\{docid}";
                        ZipFile.CreateFromDirectory(folda, dest);

                    }
                }

            }
            if (dicUpdate.Count > 0) {
                Database.UpdateFieldOfDisclosure("xbrl", dicUpdate);
            }
            if (dicUpdate.Count + dicAccessDocidDate.Count > 0)
                Console.WriteLine("import complete");
            return dicAccessDocidDate;

        }

        public async Task ReadMetadataAndUpdateDownloaded(Dictionary<string, DateTime> dic, Setting setting) {
            if (dic.Count > 0) {
                List<DateTime> dates = new List<DateTime>();
                foreach (var kv in dic)
                    if (!dates.Contains(kv.Value))
                        dates.Add(kv.Value);
                int i = 0;
                Random random = new Random();
                foreach (DateTime date in dates) {
                    if (i > 0) {
                        int wait = random.Next(Math.Min((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000)),
                                Math.Max((int)(setting.Wait[0] * 1000), (int)(setting.Wait[1] * 1000)));
                        await Task.Delay(wait);
                    }
                    await ReadMetadataType2(date, setting.Retry);
                    i++;
                }
                string filter = $"docid in ('{string.Join("', '", dic.Keys)}')";
                DataTable table = Database.ReadDisclosure(filter);
                Dictionary<int, string> dicUpdate = new Dictionary<int, string>();
                foreach(DataRow r in table.Rows) {
                    int id = int.Parse(r["id"].ToString());
                    dicUpdate[id] = $"{r["docid"].ToString()}_1.zip";
                }
                Database.UpdateFieldOfDisclosure("xbrl", dicUpdate);
            }

        }

    }

}


