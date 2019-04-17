using System;
using System.Text;

using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace Edinet {
    public class JsonDeserializer {
        public Json.ApiResponse Response { get; private set; }
        //public Json.ApiError Error { get; private set; }

        public JsonDeserializer(string source) {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(source), false)) {
                Deserialize(stream);
            }
        }
        public JsonDeserializer(Stream stream) {
            Deserialize(stream);
        }
        private void Deserialize(Stream stream) {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Json.ApiResponse));
            try {

                Response = serializer.ReadObject(stream) as Json.ApiResponse;
                Response.Status = new Json.StatusCode(Response.MetaData.Title, Response.MetaData.Status, Response.MetaData.Message);
                if (Response.Documents != null) {
                    for (int i = 0; i < Response.Documents.Length; i++) {
                        Response.Documents[i].Date = Response.MetaData.Parameter.Date;
                        DateTime date = DateTime.Parse(Response.MetaData.Parameter.Date);
                        Response.Documents[i].Id = int.Parse(date.ToString("yyMMdd")) * 10000 + Response.Documents[i].SeqNumber;
                        Response.Documents[i].Status = GetStatus(Response.Documents[i]);
                        if (Response.Documents[i].SecCode != null && Response.Documents[i].SecCode.Length > 3 && int.TryParse(Response.Documents[i].SecCode.Substring(0, 4), out int code))
                            Response.Documents[i].Code = code;
                        else
                            Response.Documents[i].Code = null;

                    }

                }
            } catch (Exception) {

                //JSON以外のテキストファイルを読み込んだ場合のエラー回避
            }

        }

        private string GetStatus(Json.Document doc) {
            /*
             * 縦覧の終了　　　"edinetCode": null,"withdrawalStatus": "0",
             * 
             * 書類の取下げ
             * 取り下げ提出日　　"withdrawalStatus": "1",　"submitDateTime": "2019-05-01 09:30",　submitは取り下げた日時
             * 元書類　　　　　　"withdrawalStatus": "2","edinetCode": null,　　　　他もnull
             * 途中に訂正があった場合　　訂正も"withdrawalStatus": "2","edinetCode": null,　ただし　"parentDocID": "S1000001",
             * 
             * 財務局職員による書類情報修正
             * 修正発生日　　　"docInfoEditStatus": "1",　"opeDateTime": "2019-06-11 09:30",
             * 元書類　　　　　"docInfoEditStatus": "2",
             * 提出書類は修正されない　修正はフィールドのみ
             * 
             * disclosureStatus 財務局職員による書類の不開示
             * 不開示開始日　　"disclosureStatus": "1","opeDateTime": "2019-05-01 19:30",
             * 不開示期間は元書類の日付は　　"disclosureStatus": "1",となる
             * 解除日　　　"disclosureStatus": "3","opeDateTime": "2019-06-01 17:30",
             * 
             */
            if (doc.EdinetCode == null & doc.WithdrawalStatus == "0")
                return "縦覧終了";
            else if (doc.WithdrawalStatus == "1")
                return "取下日";
            else if (doc.WithdrawalStatus == "2" & doc.EdinetCode == null) {
                if (doc.ParentDocID != null)
                    return "取下子";
                else
                    return "取下";
            } else if (doc.DocInfoEditStatus == "1")
                return "修正発生日";
            else if (doc.DocInfoEditStatus == "2")
                return "修正";
            else if (doc.DisclosureStatus == "1")
                return "不開示開始日";
            else if (doc.DisclosureStatus == "2")
                return "不開示";
            else if (doc.DisclosureStatus == "3")
                return "不開示解除日";
            return null;
        }


    }



}


namespace Edinet.Json {


    [DataContract]
    public class ApiResponse {
        [DataMember(Name = "metadata")]
        public Metadata MetaData { get; set; }
        [DataMember(Name = "results")]
        public Document[] Documents { get; set; }
        public StatusCode Status;
    }


    [DataContract]
    public class StatusCode {
        [DataMember(Name = "title")]
        public string Title { get; set; }
        [DataMember(Name = "status")]
        public string Status { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }

        public StatusCode() { }
        public StatusCode(string title, string status, string message) {
            Title = title;
            Status = status;
            Message = message;
        }
    }

    [DataContract]
    public class Metadata : StatusCode {
        [DataMember(Name = "parameter")]
        public Parameter Parameter { get; set; }
        [DataMember(Name = "resultset")]
        public Resultset Resultset { get; set; }
        [DataMember(Name = "processDateTime")]
        public string ProcessDateTime { get; set; }

    }

    [DataContract]
    public class Parameter {
        [DataMember(Name = "date")]
        public string Date { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; }
    }

    [DataContract]
    public class Resultset {
        [DataMember(Name = "count")]
        public int Count { get; set; }
    }

    [DataContract]
    public class Document {
        [DataMember(Name = "seqNumber")]
        public int SeqNumber { get; set; }
        [DataMember(Name = "docID")]
        public string DocID { get; set; }
        [DataMember(Name = "edinetCode")]
        public string EdinetCode { get; set; }
        [DataMember(Name = "secCode")]
        public string SecCode { get; set; }
        [DataMember(Name = "JCN")]
        public string Jcn { get; set; }
        [DataMember(Name = "filerName")]
        public string FilerName { get; set; }
        [DataMember(Name = "fundCode")]
        public string FundCode { get; set; }
        [DataMember(Name = "OrdinanceCode")]
        public string OrdinanceCode { get; set; }
        [DataMember(Name = "formCode")]
        public string FormCode { get; set; }
        [DataMember(Name = "docTypeCode")]
        public string DocTypeCode { get; set; }
        [DataMember(Name = "periodStart")]
        public string PeriodStart { get; set; }
        [DataMember(Name = "periodEnd")]
        public string PeriodEnd { get; set; }
        [DataMember(Name = "submitDateTime")]
        public string SubmitDateTime { get; set; }
        [DataMember(Name = "docDescription")]
        public string DocDescription { get; set; }
        [DataMember(Name = "issuerEdinetCode")]
        public string IssuerEdinetCode { get; set; }
        [DataMember(Name = "subjectEdinetCode")]
        public string SubjectEdinetCode { get; set; }
        [DataMember(Name = " subsidiaryEdinetCode")]
        public string SubsidiaryEdinetCode { get; set; }
        [DataMember(Name = "currentReportReason")]
        public string CurrentReportReason { get; set; }
        [DataMember(Name = "parentDocID")]
        public string ParentDocID { get; set; }
        [DataMember(Name = "opeDateTime")]
        public string OpeDateTime { get; set; }
        [DataMember(Name = "withdrawalStatus")]
        public string WithdrawalStatus { get; set; }
        [DataMember(Name = "docInfoEditStatus")]
        public string DocInfoEditStatus { get; set; }
        [DataMember(Name = "disclosureStatus")]
        public string DisclosureStatus { get; set; }
        [DataMember(Name = "xbrlFlag")]
        public string XbrlFlag { get; set; }
        [DataMember(Name = "pdfFlag")]
        public string PdfFlag { get; set; }
        [DataMember(Name = "attachDocFlag")]
        public string AttachDocFlag { get; set; }
        [DataMember(Name = "englishDocFlag")]
        public string EnglishDocFlag { get; set; }

        public int Id { get; set; }
        public string Date { get; set; }
        public Nullable<int> Code { get; set; }
        public string Status { get; set; }
    }



}

