using System;
using System.Collections.Generic;

namespace Edinet {
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
