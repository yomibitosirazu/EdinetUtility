using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;

namespace Web {

    //HttpClient同期アクセスの汎用ベースクラス
    public class Client {
        private static HttpClient client;
        private readonly string errorLog;
        public string BaseUrl { get; protected set; }
        public string Source { get; protected set; }
        public byte[] Buffer { get; protected set; }
        //他のクラスで同じBaseUrlでアクセスする場合にこのインスタンスを渡す
        public Client Instance { get { return this; } }
        public Client(string baseUrl, string logpath = null) {
            BaseUrl = baseUrl;
            if (logpath == null)
                errorLog = "HttpClientError.log";
            else
                errorLog = logpath;
            client = new HttpClient() ;
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
            client.DefaultRequestHeaders.Add("Accept-Language", "ja-JP");
            client.BaseAddress = new Uri(baseUrl);
        }

        //同期処理のみ
        public HttpResponseMessage GetResponse(string url) {
            //InitializeClient();
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.StatusCode != HttpStatusCode.OK) {
                File.AppendAllText(errorLog, string.Format("{0},{1},{2},{3}\r\n", DateTime.Now, (int)response.StatusCode, response.StatusCode, url));
                Console.WriteLine("### {0} {1} ### エラーログを確認してください###", (int)response.StatusCode, response.StatusCode);
            }
            return response;
        }
        public void Dispose() {
            if (client != null)
                client.Dispose();
        }
        public HttpStatusCode ReadSource(string url) {
            Source = null;
            using (HttpResponseMessage res = GetResponse(url)) {
                try {
                    Source = res.Content.ReadAsStringAsync().Result;
                } catch (System.Exception ex) {
                    System.Diagnostics.Debug.Print(ex.InnerException.InnerException.Message);
                    //'Windows-31J' はサポートされたエンコード名ではありません。
                    if (ex.InnerException.InnerException.Message.Contains("Windows-31J")) {
                        Source = ReadResponse(res, Encoding.GetEncoding("shift_jis"));
                        File.AppendAllText(errorLog, string.Format("{0},,shift_jisに変換,{1}\r\n", DateTime.Now, url));
                    }
                }
                //念のためステータスコードOK以外は後で確認できるようにソースを保存しておく
                if (res.StatusCode != HttpStatusCode.OK & Source != null) {
                    string dir = "HttpError";
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0} {1} {2}", DateTime.Now, (int)res.StatusCode, res.StatusCode);
                    sb.AppendLine();
                    sb.AppendLine(url);
                    sb.AppendLine(Source);
                    //万が一同一時刻（秒）あれば追記
                    File.AppendAllText(string.Format("{0}\\{1:yyyyMMddHHmmss}.html", dir, DateTime.Now), sb.ToString());
                }
                //Console.WriteLine(res.Content.Headers.ContentType.ToString());
                return res.StatusCode;
            }
        }
        public HttpStatusCode ReadSource(string url, Encoding enc) {
            using (HttpResponseMessage res = GetResponse(url)) {
                Source = ReadResponse(res, enc);
                return res.StatusCode;
            }
        }
        public HttpStatusCode DownloadBuffer(string url, out string filename) {
            using (HttpResponseMessage res = GetResponse(url)) {
                using (Stream stream = res.Content.ReadAsStreamAsync().Result) {
                    using (MemoryStream ms = new MemoryStream()) {
                        filename = res.Content.Headers.ContentDisposition.FileName;
                        stream.CopyTo(ms);
                        Buffer = ms.ToArray();
                        stream.Flush();
                        return res.StatusCode;
                    }
                }
            }
        }
        //public HttpStatusCode DownloadBuffer(string url, out string filename, out string message) {
        //    using (HttpResponseMessage res = GetResponse(url)) {
        //        using (Stream stream = res.Content.ReadAsStreamAsync().Result) {
        //            using (MemoryStream ms = new MemoryStream()) {
        //                filename = res.Content.Headers.ContentDisposition.FileName;
        //                stream.CopyTo(ms);
        //                Buffer = ms.ToArray();
        //                stream.Flush();
        //                System.Net.Http.Headers.MediaTypeHeaderValue contenttype = res.Content.Headers.ContentType;
        //                message = contenttype.ToString();
        //                return res.StatusCode;
        //            }
        //        }
        //    }
        //}
        private string ReadResponse(HttpResponseMessage response, Encoding enc) {
            using (Stream stream = response.Content.ReadAsStreamAsync().Result) {
                using (var reader = (new StreamReader(stream, enc, true)) as TextReader) {
                    return reader.ReadToEndAsync().Result;
                }
            }
        }

        public HttpStatusCode Download(string url, string filepath) {
            using (HttpResponseMessage res = GetResponse(url)) {
                using (FileStream filestream = File.Create(filepath)) {
                    using (System.IO.Stream stream = res.Content.ReadAsStreamAsync().Result) {
                        stream.CopyTo(filestream);
                        filestream.Flush();
                        return res.StatusCode;
                    }
                }
            }
        }

        //これはエラーログを残さない
        public HttpStatusCode ReadStream(string url, out Stream stream) {
            using (HttpResponseMessage res = GetResponse(url)) {
                stream = res.Content.ReadAsStreamAsync().Result;
                return res.StatusCode;
            }
        }


        public Stream ReadStream(string url) {
            return client.GetStreamAsync(url).Result;
        }

    }
}
