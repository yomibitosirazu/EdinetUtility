using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

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

        public MediaTypeHeaderValue HeaderContentType { get; private set; }
        public Nullable<HttpStatusCode> StatusCode { get; private set; }
        public Exception Exception { get; private set; }

        //public HttpResponse() {
        //}
        public void UpdateStatus(Nullable<HttpStatusCode> status, MediaTypeHeaderValue contentType) {
            StatusCode = status;
            HeaderContentType = contentType;
            Exception = null;
        }
        public void UpdateError(Exception ex) {
            //StatusCode = null;
            //HeaderContentType = null;
            Exception = ex;
        }
        public void Initialize() {
            StatusCode = null;
            HeaderContentType = null;
            Exception = null;
        }
    }


    public class RequestDocument : HttpRequest {
        public enum RequestType { Metadata = 1, List = 2 }


        private readonly HttpResponse response;
        //private JsonDeserializer json;
        public JsonDeserializer Json { get; private set; }
        public string Version { get; set; }
        public RequestDocument(string version) : base() {
            Version = version;
            response = new HttpResponse();
        }

        public void Dispose() {
            if (client != null)
                client.Dispose();
        }

        public async Task<HttpResponse> Request(DateTime date, RequestType type) {
            response.Initialize();
            string url = string.Format("/api/{0}/documents.json?date={1:yyyy-MM-dd}{2}", Version, date, type == RequestType.List ? "&type=2" : "");
            try {
                using (HttpResponseMessage res = await client.GetAsync(url)) {
                    response.UpdateStatus(res.StatusCode, res.Content.Headers.ContentType);
                    Stream stream = await res.Content.ReadAsStreamAsync();
                    Json = new JsonDeserializer(stream);
                    stream.Dispose();
                }
            } catch (Exception ex) {
                response.UpdateError(ex);
            }
            return response;
        }

        //    private async Task<Stream> ReadStream(DateTime date, RequestType type = RequestType.List) {
        //        response.Initialize();
        //        string url = string.Format("/api/{0}/documents.json?date={1:yyyy-MM-dd}{2}", Version, date, type == RequestType.List ? "&type=2" : "");
        //        try {
        //            using (HttpResponseMessage res = await client.GetAsync(url)) {
        //                response.UpdateStatus(res.StatusCode, res.Content.Headers.ContentType);
        //                return await res.Content.ReadAsStreamAsync();
        //            }
        //        } catch (Exception ex) {
        //            response.UpdateError(ex);
        //        }
        //        return null;
        //    }
        //    private async Task<string> Read(DateTime date, RequestType type = RequestType.List) {
        //        response.Initialize();
        //        string url = string.Format("/api/{0}/documents.json?date={1:yyyy-MM-dd}{2}", Version, date, type == RequestType.List ? "&type=2" : "");
        //        try {
        //            using (var res = await client.GetAsync(url)) {
        //                response.UpdateStatus(res.StatusCode, res.Content.Headers.ContentType);
        //                return await res.Content.ReadAsStringAsync();
        //            }
        //        } catch (Exception ex) {
        //            response.UpdateError(ex);
        //        }
        //        return null;
        //    }
    }

    public class RequestArchive : HttpRequest {
        public enum ContentType { Zip, Pdf, Fail };
        public static string[] ContentTypes { get { return new string[] { "application/octet-stream", "application/pdf", "application/json; charset=utf-8" }; } }//Zip,Pdf,Fail
        public Exception Exception { get; set; }
        public RequestArchive() : base() {
        }
    }

}
