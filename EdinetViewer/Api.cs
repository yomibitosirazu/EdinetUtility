﻿using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Edinet {
    public class HttpRequest {
        protected readonly string baseUrl = "https://disclosure.edinet-fsa.go.jp/";
        protected static HttpClient client;
        //private static CookieContainer cc;
        //private static HttpClientHandler handler;
        public HttpRequest(string useragent) {
            if (client == null) {
                client = new HttpClient();
                //cc = new CookieContainer();
                //handler = new HttpClientHandler();
                //client = new HttpClient(handler);
                //handler.CookieContainer = cc;
                //client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
                //string useragent = $"EdinetViewer CSharp";
                //if ((Environment.MachineName == "H270M" | Environment.MachineName == "PD-1712") && File.Exists("contact.txt")) {
                //    string contact = File.ReadAllText("contact.txt");
                //    useragent += $"({contact})";
                //}
                //client.DefaultRequestHeaders.Add("User-Agent", useragent);
                //refer to https://kagasu.hatenablog.com/entry/2017/08/10/050726
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", useragent);
                client.DefaultRequestHeaders.Add("Accept-Language", "ja-JP");
                client.BaseAddress = new Uri(baseUrl);
            }
        }
        public void Dispose() {
            if (client != null)
                client.Dispose();
            //if (handler != null)
            //    handler.Dispose();
        }
    }

    public enum ResponseResult { Invalid, Success = 200, Zero = 201, SameProcess = 202, SameCount = 203, Timeout = 504, BadRequest = 400, NotFound = 404, ServerError = 500, Exception = 1001 };

    public class HttpResponse {
        public ResponseResult ReturnResult { get; set; }
        public MediaTypeHeaderValue HeaderContentType { get; protected set; }
        public Nullable<HttpStatusCode> HttpStatusCode { get; protected set; }
        public Exception Exception { get; protected set; }


        public void Update(Nullable<HttpStatusCode> status, MediaTypeHeaderValue contentType) {
            HttpStatusCode = status;
            HeaderContentType = contentType;
            Exception = null;
        }
        public void Update(Exception ex) {
            Exception = ex;
            Debug.WriteLine($"Response Exception {DateTime.Now.TimeOfDay}  {ex.Message}");
            ReturnResult = ResponseResult.Exception;
        }
        public void Update(TaskCanceledException ex) {
            Exception = ex;
            HttpStatusCode = System.Net.HttpStatusCode.RequestTimeout;
            Debug.WriteLine($"{DateTime.Now.TimeOfDay} Timeout {ex.Message}");
            ReturnResult = ResponseResult.Timeout;
        }
    }

    public class ApiResponse : HttpResponse {
        public Json.StatusCode EdinetStatusCode { get; protected set; }
        public void Update(Json.StatusCode edinetstatuscode, Nullable<HttpStatusCode> status, MediaTypeHeaderValue contentType) {
            base.Update(status, contentType);
            EdinetStatusCode = edinetstatuscode;
            //if (edinetstatuscode.Status != "200")
                this.ReturnResult = (ResponseResult)Enum.ToObject(typeof(ResponseResult), int.Parse(edinetstatuscode.Status));
        }
    }
    public class JsonResponse : ApiResponse {
        public Json.ApiResponse Json { get; private set; }
        public void Update(Json.ApiResponse json, Nullable<HttpStatusCode> status, MediaTypeHeaderValue contentType) {
            
            base.Update(json.Status, status, contentType);
            Json = json;
        }
    }
    public class ArchiveResponse : ApiResponse {

        public enum ContentType { Zip, Pdf, Fail };
        public static string[] ContentTypes { get { return new string[] { "application/octet-stream", "application/pdf", "application/json; charset=utf-8" }; } }//Zip,Pdf,Fail
        public byte[] Buffer { get; set; }
        public string Filename { get; set; }
        public ContentType Type { get; private set; }

        public void Update(byte[] buffer, Nullable<HttpStatusCode> status, string filename, MediaTypeHeaderValue contentType) {
            Buffer = buffer;
            Filename = filename;
            Type = (ContentType)Enum.ToObject(typeof(ContentType), Array.IndexOf(ContentTypes, contentType.ToString()));
            if (Type == ContentType.Fail) {
                string source = Encoding.ASCII.GetString(buffer);
                JsonDeserializer deserializer = new JsonDeserializer(source);
                base.Update(deserializer.Response.Status, status, contentType);
            } else
                base.Update(status, contentType);

        }
    }
    public class RequestDocument : HttpRequest {
        public enum RequestType { Metadata = 1, List = 2, Archive = 3 }
        public enum DocumentType { Xbrl = 1, Pdf = 2, Attach = 3, English = 4 }

        //private readonly HttpResponse response;
        //private JsonDeserializer json;
        //public JsonDeserializer Json { get; private set; }
        public string Version { get; set; }

        private readonly string directory;
        public RequestDocument(string dir, string version, string useragent) : base(useragent) {
            Version = version;
            directory = dir;
            //response = new HttpResponse();

        }


        public async Task<JsonResponse> Request(DateTime date, RequestType type, int retry) {
            JsonResponse response = new JsonResponse();
            //JsonDeserializer json = null;
            string url = string.Format("/api/{0}/documents.json?date={1:yyyy-MM-dd}{2}", Version, date, type == RequestType.List ? "&type=2" : "");
            int i = 0;
            do {
                if (i > 0) {
                    SaveLog($"  retry[{i}] {date} {type}");
                    Debug.Write($"retry Request[{i}] ");
                    await Task.Delay(2000);
                }
                try {
                    debug.ProgramCodeInfo.SetDebugQueue();
                    using (HttpResponseMessage res = await client.GetAsync(url)) {
                        debug.ProgramCodeInfo.SetDebugQueue();
                        Stream stream = await res.Content.ReadAsStreamAsync();
                        JsonDeserializer json = new JsonDeserializer(stream);
                        response.Update(json.Response, res.StatusCode, res.Content.Headers.ContentType);
#pragma warning disable CS4014
                        SaveLog(GetLog(response, type, date));
#pragma warning restore CS4014
                        stream.Dispose();
                        return response;
                    }
                } catch (TaskCanceledException ex) {
                    //ServerTimeout
                    response.Update(ex);
#pragma warning disable CS4014
                    SaveLog(GetLog(response, type, date));
#pragma warning restore CS4014
                    debug.ProgramCodeInfo.SetDebugQueue();
                } catch (Exception ex) {
                    response.Update(ex);
#pragma warning disable CS4014
                    SaveLog(GetLog(response, type, date));
#pragma warning restore CS4014
                    debug.ProgramCodeInfo.SetDebugQueue();
                    return response;
                }
                debug.ProgramCodeInfo.SetDebugQueue();
                i++;
            } while (i<=retry);

            return response;
        }

        public async Task<ArchiveResponse> DownloadArchive(string docid, DocumentType type, int retry) {
            ArchiveResponse response = new ArchiveResponse();
            string url = string.Format("/api/{0}/documents/{1}?type={2}", Version, docid, (int)type);
            int i = 0;
            do {
                if (i > 0) {
                    SaveLog($"  retry[{i}] {docid} {type}");
                    Debug.Write($"retry Download[{i}] ");
                    await Task.Delay(2000);
                }
                try {
                    debug.ProgramCodeInfo.SetDebugQueue();
                    using (HttpResponseMessage res = await client.GetAsync(url)) {
                        debug.ProgramCodeInfo.SetDebugQueue();
                        string filename = res.Content.Headers.ContentDisposition.FileName.Replace("\"", "");
                        //string filename = $"{docid}_{(int)type}";
                        System.Net.Http.Headers.MediaTypeHeaderValue contenttype = res.Content.Headers.ContentType;
                        if (filename == "404.json") {
                            filename = url;
                        }
#pragma warning disable CS4014
                        SaveLog(GetLog(res.StatusCode, RequestType.Archive, contenttype, filename));
#pragma warning restore CS4014
                        using (Stream stream = await res.Content.ReadAsStreamAsync()) {
                            using (MemoryStream ms = new MemoryStream()) {
                                stream.CopyTo(ms);
                                byte[] buffer = ms.ToArray();
                                stream.Flush();
                                response.Update(buffer, res.StatusCode, filename, contenttype);
                                debug.ProgramCodeInfo.SetDebugQueue();
                                return response;
                            }
                        }
                    }
                } catch (TaskCanceledException ex) {
                    response.Update(ex);
#pragma warning disable CS4014
                    SaveLog(GetLog(response));
#pragma warning restore CS4014
                    debug.ProgramCodeInfo.SetDebugQueue();
                } catch (Exception ex) {
                    response.Update(ex);
#pragma warning disable CS4014
                    SaveLog(GetLog(response));
#pragma warning restore CS4014
                    debug.ProgramCodeInfo.SetDebugQueue();
                    return response;
                }
                i++;
            } while (i <= retry);
            return response;
        }

        //Responseを返さないのでファイルセーブまで待たないはず
        public async Task DownloadAsync(string docid, DocumentType type, int id, Database.Sqlite db, int retry) {
            string url = string.Format("/api/{0}/documents/{1}?type={2}", Version, docid, (int)type);
            int i = 0;
            do {
                if (i > 0) {
                    Debug.Write($"retry notawait Download[{i}] ");
                    await Task.Delay(2000);
                }
                try {
                    debug.ProgramCodeInfo.SetDebugQueue();
                    using (HttpResponseMessage res = await client.GetAsync(url)) {
                        debug.ProgramCodeInfo.SetDebugQueue();
                        string filename = res.Content.Headers.ContentDisposition.FileName.Replace("\"", "");
                        //string filename = $"{docid}_{(int)type}";
                        System.Net.Http.Headers.MediaTypeHeaderValue contenttype = res.Content.Headers.ContentType;
#pragma warning disable CS4014
                        SaveLog(GetLog(res.StatusCode, RequestType.Archive, contenttype, filename, id));
#pragma warning restore CS4014
                        using (Stream stream = await res.Content.ReadAsStreamAsync()) {
                            using (MemoryStream ms = new MemoryStream()) {
                                //SaveLog(GetLog(res.StatusCode, RequestType.Archive, contenttype));
                                stream.CopyTo(ms);
                                byte[] buffer = ms.ToArray();
                                stream.Flush();
                                if (filename == "404.json") {
                                    filename = url;
                                } else {
                                    int year = 20 * 100 + id / 100000000;
                                    SaveFile(buffer, filename, year);
                                    db.UpdateFilenameOfDisclosure(id, type.ToString(), filename);
                                }
                                return;
                            }
                        }
                    }
                } catch (TaskCanceledException ex) {
                    ArchiveResponse response = new ArchiveResponse();
                    response.Update(ex);
#pragma warning disable CS4014
                    SaveLog(GetLog(response));
#pragma warning restore CS4014
                    debug.ProgramCodeInfo.SetDebugQueue();
                } catch (Exception ex) {
                    ArchiveResponse response = new ArchiveResponse();
                    response.Update(ex);
#pragma warning disable CS4014
                    SaveLog(GetLog(response));
#pragma warning restore CS4014
                    debug.ProgramCodeInfo.SetDebugQueue();
                    return;
                }
                debug.ProgramCodeInfo.SetDebugQueue();
                i++;
            } while (i <= retry);

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
        public async Task<HttpResponseMessage> RequestDownload(string docid, DocumentType type) {
            string url = string.Format("/api/{0}/documents/{1}?type={2}", Version, docid, (int)type);
            try {
                return await client.GetAsync(url);
            } catch (Exception ex) {
                ArchiveResponse response = new ArchiveResponse();
                response.Update(ex);
#pragma warning disable CS4014
                SaveLog(GetLog(response));
#pragma warning restore CS4014
                return null;
            }
        }



        public async Task Download(HttpResponseMessage httpResponseMessage, int id, string field, Database.Sqlite db) {
            string filename = httpResponseMessage.Content.Headers.ContentDisposition.FileName.Replace("\"", "");
            System.Net.Http.Headers.MediaTypeHeaderValue contenttype = httpResponseMessage.Content.Headers.ContentType;
            using (Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync()) {
                using (MemoryStream ms = new MemoryStream()) {
                    //string filename = $"{docid}_{(int)type}";
                    stream.CopyTo(ms);
                    byte[] buffer = ms.ToArray();
                    stream.Flush();
                    if (filename == "404.json") {
                        filename = httpResponseMessage.RequestMessage.RequestUri.ToString();
                    }
                     else {
                        int year = 20 * 100 + id / 100000000;
                        SaveFile(buffer, filename, year);
                        db.UpdateFilenameOfDisclosure(id, field, filename);
                    }
#pragma warning disable CS4014
                    SaveLog(GetLog(httpResponseMessage.StatusCode, RequestType.Archive,contenttype,filename));
#pragma warning restore CS4014
                }
            }

            httpResponseMessage.Dispose();
        }

        private async Task SaveLog(string log) {
            string logfile = Path.Combine(directory, "EdinetApi.log");
            //File.AppendAllLines(logfile, new string[] { log });
            try {
                using (var sw = new StreamWriter(logfile, true)) {
                    await sw.WriteLineAsync(log);
                    sw.Flush();
                }
            } catch (Exception ex) {
                Console.WriteLine("at SaveLog   " + ex.Message);
            }


        }
        private string GetLog(JsonResponse response, RequestType type, DateTime target) {
            StringBuilder sb = new StringBuilder();
            try {
                sb.Append(GetLog(response.HttpStatusCode, type, null, "", 0, response.Exception, response.EdinetStatusCode));
                if (response.Exception == null) {
                    sb.AppendFormat("\t{0}", target.ToString("yyyy-MM-dd"));
                    if (response != null && response.Json != null && response.Json.MetaData.Resultset != null)
                        sb.AppendFormat("\tcount:{0}", response.Json.MetaData.Resultset.Count);
                }

            } catch (Exception ex) {

                throw(ex);
            }
            return sb.ToString();
        }
        private string GetLog(ArchiveResponse response) {
            StringBuilder sb = new StringBuilder();
            try {
                if (response.Exception != null) {
                    return response.Exception.Message;
                }
                sb.Append(GetLog(response.HttpStatusCode, RequestType.Archive, response.HeaderContentType, response.Filename, 0, response.Exception));
                if (response.Exception == null) {
                    sb.AppendFormat("\t{0}", response.Filename);
                }

            } catch (Exception ex) {

                throw(ex);
            }
            return sb.ToString();
        }
        //private string GetLog(ApiResponse response, RequestType type) {
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff} {1}\t", DateTime.Now, type.ToString());
        //    if (response.HttpStatusCode != null)
        //        sb.AppendFormat("{0}[{1}]", (int)response.HttpStatusCode, response.HttpStatusCode.ToString());
        //    else
        //        sb.Append("\t");
        //    if (response.EdinetStatusCode != null)
        //        sb.AppendFormat("\t{0}[{1}]", response.EdinetStatusCode.Status, response.EdinetStatusCode.Message);
        //    else
        //        sb.Append("\t" + response.HeaderContentType.MediaType ?? "");
        //    if (response.Exception != null) {
        //        sb.AppendFormat("\t{0}", response.Exception.Message);
        //        if (response.Exception.InnerException != null)
        //            sb.AppendFormat(" {0}", response.Exception.InnerException.Message);
        //    }
        //    return sb.ToString();
        //}

        private string GetLog(Nullable<HttpStatusCode> statusCode, RequestType type, MediaTypeHeaderValue mediaType, string filename = "", int id = 0, Exception exception = null, Json.StatusCode edinetStatusCode = null) {
            StringBuilder sb = new StringBuilder();
            try {
                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}\t{1}\t", DateTime.Now, type.ToString());
                if (statusCode != null)
                    sb.AppendFormat("{0}[{1}]", (int)statusCode, statusCode.ToString());
                else
                    sb.Append("\t");
                if (edinetStatusCode != null)
                    sb.AppendFormat("\t{0}[{1}]", edinetStatusCode.Status, edinetStatusCode.Message);
                else
                    sb.Append("\t" + mediaType ?? "");
                if (exception != null) {
                    sb.AppendFormat("\t{0}", exception.Message);
                    if (exception.InnerException != null)
                        sb.AppendFormat(" {0}", exception.InnerException.Message);
                }
                sb.AppendFormat("\t{0}", filename);
                if (id > 0)
                    sb.AppendFormat("\t{0}", id);
            } catch (Exception ex) {

                throw(ex);
            }

            return sb.ToString();
        }


    }


}









