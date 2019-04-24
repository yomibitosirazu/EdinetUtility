using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;

using System.IO;

namespace Edinet {
    public class HttpRequest {
        protected readonly string baseUrl = "https://disclosure.edinet-fsa.go.jp/";
        protected static HttpClient client;
        public HttpRequest() {
            if (client == null) {
                client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
                client.DefaultRequestHeaders.Add("Accept-Language", "ja-JP");
                client.BaseAddress = new Uri(baseUrl);
            }
        }
    }

    public class HttpResponse {
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
        }
    }
    public class ApiResponse : HttpResponse {
        public Json.StatusCode EdinetStatusCode { get; protected set; }
        public void Update(Json.StatusCode edinetstatuscode, Nullable<HttpStatusCode> status, MediaTypeHeaderValue contentType) {
            base.Update(status, contentType);
            EdinetStatusCode = edinetstatuscode;
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
        public RequestDocument(string dir, string version) : base() {
            Version = version;
            directory = dir;
            //response = new HttpResponse();

        }

        public void Dispose() {
            if (client != null)
                client.Dispose();
        }

        public async Task<JsonResponse> Request(DateTime date, RequestType type) {
            JsonResponse response = new JsonResponse();
            //JsonDeserializer json = null;
            string url = string.Format("/api/{0}/documents.json?date={1:yyyy-MM-dd}{2}", Version, date, type == RequestType.List ? "&type=2" : "");
            try {
                using (HttpResponseMessage res = await client.GetAsync(url)) {
                    Stream stream = await res.Content.ReadAsStreamAsync();
                    JsonDeserializer json = new JsonDeserializer(stream);
                    response.Update(json.Response, res.StatusCode, res.Content.Headers.ContentType);
                    stream.Dispose();
                }
            } catch (Exception ex) {
                response.Update(ex);
            }
            SaveLog(GetLog(response, type, date));
            return response;
        }

        public async Task<ArchiveResponse> DownloadArchive(string docid, DocumentType type) {
            ArchiveResponse response = new ArchiveResponse();
            string url = string.Format("/api/{0}/documents/{1}?type={2}", Version, docid, (int)type);
            try {
                using (HttpResponseMessage res = await client.GetAsync(url)) {
                    using (Stream stream = await res.Content.ReadAsStreamAsync()) {
                        using (MemoryStream ms = new MemoryStream()) {
                            string filename = res.Content.Headers.ContentDisposition.FileName.Replace("\"", "");
                            stream.CopyTo(ms);
                            byte[] buffer = ms.ToArray();
                            stream.Flush();
                            System.Net.Http.Headers.MediaTypeHeaderValue contenttype = res.Content.Headers.ContentType;
                            //ContentType content = (ContentType)Enum.ToObject(typeof(ContentType), Array.IndexOf(ContentTypes, contenttype.ToString()));
                            //ArchiveResponse result = new ApiArchiveResult(contenttype.ToString(), res.StatusCode, buffer, filename);
                            response.Update(buffer, res.StatusCode, filename, contenttype);
                        }
                    }
                }
            } catch (Exception ex) {
                response.Update(ex);
            }
            try {
                SaveLog(GetLog(response));

            } catch (Exception ex) {

                Console.WriteLine($"SaveLog(DownloadArchive)\r\n{ex.Message}");
            }
            return response;
        }


        private void SaveLog(string log) {
            string logfile = Path.Combine(directory, "EdinetApi.log");
            File.AppendAllText(logfile, log + "\r\n");
        }
        private string GetLog(JsonResponse response, RequestType type, DateTime target) {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetLog(response, type));
            if (response.Exception == null) {
                sb.AppendFormat("\t{0}", target.ToString("yyyy-MM-dd"));
                if (response != null && response.Json != null && response.Json.MetaData.Resultset != null)
                    sb.AppendFormat("\tcount:{0}", response.Json.MetaData.Resultset.Count);
            }
            return sb.ToString();
            //string logfile = Path.Combine(directory, "EdinetApi.log");
            //File.AppendAllText(logfile, sb.ToString());
        }
        private string GetLog(ArchiveResponse response) {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetLog(response, RequestType.Archive));
            if (response.Exception == null) {
                sb.AppendFormat("\t{0}", response.Filename);
            }
            return sb.ToString();
            //string logfile = Path.Combine(directory, "EdinetApi.log");
            //File.AppendAllText(logfile, sb.ToString());
        }
        private string GetLog(ApiResponse response, RequestType type) {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.f} {1}\t", DateTime.Now, type.ToString());
            if (response.HttpStatusCode != null)
                sb.AppendFormat("{0}[{1}]", (int)response.HttpStatusCode, response.HttpStatusCode.ToString());
            else
                sb.Append("\t");
            if (response.EdinetStatusCode != null)
                sb.AppendFormat("\t{0}[{1}]", response.EdinetStatusCode.Status, response.EdinetStatusCode.Message);
            else
                sb.Append("\t" + response.HeaderContentType.MediaType ?? "");
            if (response.Exception != null) {
                sb.AppendFormat("\t{0}", response.Exception.Message);
                if (response.Exception.InnerException != null)
                    sb.AppendFormat(" {0}", response.Exception.InnerException.Message);
            }
            return sb.ToString();
        }



    }


}









