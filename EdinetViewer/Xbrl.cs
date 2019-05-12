using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;

using System.Data;



namespace Edinet {

    public class Xml {
        protected XmlDocument doc;
        public XmlNodeList NodeList;
        public Xml() {
            doc = new XmlDocument();
        }
        public void Load(string source) {
            doc.LoadXml(source);
            NodeList = doc.GetElementsByTagName("*");
        }
    }

    class Xbrl : Xml {
        public Taxonomy Taxonomy { get; set; }

        public Xbrl() { }
        public Xbrl(Dictionary<string, string> dicTaxonomy, List<string> listXml) : base() {
            Taxonomy = new Taxonomy() {
                DicTaxonomy = dicTaxonomy,
                ListXml = listXml
            };
        }

        private DataTable CreateTable() {
            DataTable table = new DataTable();
            table.Columns.Add("no", typeof(int));
            table.Columns.Add("tag", typeof(string));
            table.Columns.Add("ラベル", typeof(string));
            table.Columns.Add("prefix", typeof(string));
            table.Columns.Add("element", typeof(string));
            table.Columns.Add("contextRef", typeof(string));
            table.Columns.Add("sign", typeof(string));
            table.Columns.Add("value", typeof(string));
            table.Columns.Add("unitRef", typeof(string));
            table.Columns.Add("decimals", typeof(string));
            table.Columns.Add("nil", typeof(string));
            table.Columns.Add("attributes", typeof(string));
            return table;
        }
        public DataTable ToTable() {
            string[] fields = "name,contextRef,sign,unitRef,decimals,nil".Split(',');
            string[] tags = "nonNumeric,nonFraction".Split(',');
            DataTable table = CreateTable();
            if (NodeList != null) {

                int i = 0;
                foreach (XmlNode node in NodeList) {
                    string[] names = node.Name.Split(':');
                    if (names.Length > 1) {
                        i++;
                        DataRow r = table.NewRow();
                        r["no"] = i;
                        if (Array.IndexOf(tags, names[1]) > -1)
                            r["tag"] = names[1];

                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        StringBuilder attributes = new StringBuilder();
                        foreach (XmlAttribute attribute in node.Attributes) {

                            if (Array.IndexOf(fields, attribute.Name) > -1) {
                                if (attribute.Name == "name")
                                    names = attribute.InnerText.Split(':');
                                else
                                    r[attribute.Name] = attribute.InnerText;
                            } else {
                                attributes.AppendFormat("{0}={1}", attribute.Name, attribute.InnerText);
                                dic.Add(attribute.Name, attribute.InnerText);

                            }
                        }
                        r["prefix"] = names[0];
                        string value = node.InnerText.Replace("\n", "").Replace("\r", "");
                        r["value"] = value;
                        r["attributes"] = attributes;
                        r["element"] = names[1];
                        string parentLabel = Taxonomy.DicTaxonomy.ContainsKey(names[1]) ? Taxonomy.DicTaxonomy[names[1]] : "";
                        r["ラベル"] = parentLabel;

                        table.Rows.Add(r);
                    }
                }
            }
            return table;
        }


        public string GetSummaryLargeVolume() {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (XmlNode element in NodeList) {
                string[] names = element.Name.Split(':');
                string name = names[names.Length - 1];
                if (element.InnerText != "") {
                    switch (name) {
                        case "SecurityCodeOfIssuer": //証券コード
                            dic["code"] = element.InnerText;
                            break;
                        case "NameOfIssuer": // 発行者の名称（銘柄名）
                            dic["name"] = element.InnerText;
                            break;
                        case "NameOfEmployer": // 勤務先名称
                            dic["勤務先"] = element.InnerText;
                            break;
                        case "PurposeOfHolding": // 保有目的
                            dic["保有目的"] = element.InnerText;
                            break;
                        case "ActOfMakingImportantProposalEtcNA": // 重要提案行為等
                            dic["提案"] = element.InnerText;
                            break;
                        case "BaseDate": // 基準日
                            dic["基準日"] = element.InnerText;
                            break;
                        case "TotalNumberOfStocksEtcHeld": // 保有証券総数
                            dic["保有"] = element.InnerText;
                            break;
                        case "TotalNumberOfOutstandingStocksEtc": // 発行済株式総数
                            dic["発行"] = element.InnerText;
                            break;
                        case "HoldingRatioOfShareCertificatesEtc": // 保有割合
                            dic["割合"] = element.InnerText;
                            break;
                        case "NumberOfSubmissionDEI":
                            dic["回数"] = element.InnerText;
                            break;
                        case "DateWhenFilingRequirementAroseCoverPage":
                            dic["報告義務発生日"] = element.InnerText;
                            break;
                        case "FilingDateCoverPage":
                            dic["提出日"] = element.InnerText;
                            break;
                        case "ReasonForFilingChangeReportCoverPage":
                            dic["事由"] = element.InnerText;
                            break;
                            //case "AmendmentFlagDEI"://true：訂正提出時、false：当初提出時
                            //    dic[""] = element.InnerText;
                            //    break;
                            //case "IdentificationOfDocumentSubjectToAmendmentDEI"://該当ある場合、訂正対象の当初提出書類の書類管理番号（EDINET提出時にEDINETにより付与される番号。）を記載する。
                            //    dic[""] = element.InnerText;
                            //    break;
                            //case "ReportAmendmentFlagDEI"://true：記載事項を訂正する場合（添付書類のみの訂正及びXBRLを同時に訂正する場合を含む）、false：それ以外
                            //    dic[""] = element.InnerText;
                            //    break;
                            //case "XBRLAmendmentFlagDEI"://true：記載事項を訂正せずXBRLのみを訂正する場合、false：それ以外
                            //    dic[""] = element.InnerText;
                            //    break;
                    }
                }
            }

            //FilerNameInJapaneseDEI 氏名
            //foreach(var kv in dic) {
            //    Console.WriteLine($"{kv.Key}\t{kv.Value}");
            //}
            StringBuilder sb = new StringBuilder();
            if (dic.ContainsKey("code"))
                sb.Append($"{dic["code"]} ");
            if (dic.ContainsKey("name"))
                sb.Append($"{dic["name"]} ");
            if (dic.ContainsKey("割合")) {
                decimal ratio = decimal.Parse(dic["割合"]);
                sb.Append($"{ratio:0.0%}");
            }
            if (dic.ContainsKey("保有") & dic.ContainsKey("発行"))
                sb.Append($"({dic["保有"]}/{dic["発行"]}) ");
            if (dic.ContainsKey("基準日"))
                sb.Append($"{dic["基準日"]} ");
            if (dic.ContainsKey("保有目的"))
                sb.Append($"{dic["保有目的"]} ");

            if (dic.ContainsKey("回数"))
                sb.Append($"[{dic["回数"]}] ");
            //if (dic.ContainsKey("報告義務発生日"))
            //    sb.Append($"{dic["報告義務発生日"]} ");
            //if (dic.ContainsKey("提出日"))
            //    sb.Append($"{dic["提出日"]} ");
            if (dic.ContainsKey("事由"))
                sb.Append($"{dic["事由"]} ");
            return sb.ToString();
        }

    }

}


namespace Archive {
    using System.IO;
    using System.IO.Compression;
    using System.Threading.Tasks;

    public class Zip {
        private readonly string rootdir;
        private readonly string tempdir;
        private byte[] buffer;
        public DataTable Table { get; private set; }
        public Zip(string dir) {
            rootdir = dir;
            tempdir = Path.Combine(dir, "temp");
            if (!Directory.Exists(tempdir))
                Directory.CreateDirectory(tempdir);
            Table = new DataTable();
            Table.Columns.Add("type", typeof(string));
            Table.Columns.Add("folda", typeof(string));
            Table.Columns.Add("name", typeof(string));
            Table.Columns.Add("fullpath", typeof(string));
            Table.Columns.Add("no", typeof(int));
        }

        private void LoadContents() {
            using (MemoryStream stream = new MemoryStream(buffer)) {
                using (ZipArchive archive = new ZipArchive(stream)) {
                    int i = 0;
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        i++;
                        FileInfo inf = new FileInfo(entry.FullName);
                        string folda = null;
                        if (inf.FullName.Contains("PublicDoc")) {
                            folda = "PublicDoc";
                        } else if (inf.FullName.Contains("AuditDoc"))
                            folda = "AuditDoc";
                        else if (inf.FullName.Contains("Summary")) {
                            folda = "Summary";
                        } else if (inf.FullName.Contains("Attachment"))
                            folda = "Attachment";
                        DataRow r = Table.NewRow();
                        r["type"] = inf.Extension;
                        r["folda"] = folda;
                        r["name"] = entry.Name;
                        r["fullpath"] = entry.FullName;
                        r["no"] = i;
                        Table.Rows.Add(r);
                    }
                }
            }
        }

        public string Exists(int id, string docid, int type) {
            int year = 20 * 100 + id / 100000000;
            string filepath = $@"{rootdir}\Documents\{year}\{docid}_{type}.zip";
            if (File.Exists(filepath))
                return filepath;
            else
                return null;
        }
        public bool Load(int id, string docid, int type) {
            Table.Rows.Clear();
            int year = 20 * 100 + id / 100000000;
            string filepath = $@"{rootdir}\Documents\{year}\{docid}_{type}.zip";
            if (File.Exists(filepath)) {
                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read)) {
                    buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                }
                if (buffer != null && type != 2)
                    LoadContents();
                return true;
            } else
                return false;
        }
        public async Task<bool> LoadAsync(int id, string docid, int type) {
            Table.Rows.Clear();
            int year = 20 * 100 + id / 100000000;
            string filepath = $@"{rootdir}\Documents\{year}\{docid}_{type}.zip";
            if (File.Exists(filepath)) {
                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read,
                        FileShare.Read, bufferSize: 4096, useAsync: true)) {
                    buffer = new byte[fs.Length];
                    await fs.ReadAsync(buffer, 0, buffer.Length);
                }
                if (buffer != null && type != 2)
                    LoadContents();
                return true;
            } else
                return false;
        }

        public void Load(byte[] buf, int type) {
            buffer = buf;
            if (buffer != null && type != 2)
                LoadContents();
        }

        public string Read(string fullname) {
            using (MemoryStream stream = new MemoryStream(buffer)) {
                using (ZipArchive archive = new ZipArchive(stream)) {
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        if (entry.FullName == fullname) {
                            if (".png .jpg .jpeg .gif .svg .tif .tiff .esp .pict .bmp".Contains(Path.GetExtension(fullname).ToLower())) {
                                using (Stream stream2 = entry.Open()) {
                                    using (MemoryStream ms = new MemoryStream()) {
                                        stream2.CopyTo(ms);
                                        using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms)) {
                                            string extension = Path.GetExtension(fullname);
                                            string imagefile = Path.Combine(tempdir, "image" + extension);
                                            image.Save(imagefile);
                                            return imagefile;
                                        }
                                    }
                                }

                            } else if (Path.GetExtension(fullname) == ".pdf") {
                                string filepath = string.Format(@"{0}\{1}", tempdir, entry.Name);
                                entry.ExtractToFile(filepath, true);
                                return filepath;
                            } else {

                                Encoding enc = Encoding.UTF8;
                                if (entry.Name.EndsWith(".txt", false, System.Globalization.CultureInfo.CurrentCulture)
                                    | entry.Name.EndsWith(".csv", false, System.Globalization.CultureInfo.CurrentCulture))
                                    enc = Encoding.GetEncoding("shift_jis");
                                using (Stream stream2 = entry.Open()) {
                                    using (StreamReader reader = new StreamReader(stream2, enc)) {
                                        return reader.ReadToEnd();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static string ReadXbrlSource(string filepath) {
            if (File.Exists(filepath)) {
                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read)) {
                    using (MemoryStream stream = new MemoryStream()) {
                        fs.CopyTo(stream);
                        using (ZipArchive archive = new ZipArchive(stream)) {
                            foreach (ZipArchiveEntry entry in archive.Entries) {
                                FileInfo inf = new FileInfo(entry.FullName);
                                if (inf.FullName.Contains("PublicDoc") && inf.Extension == ".xbrl") {
                                    using (Stream stream2 = entry.Open()) {
                                        using (StreamReader reader = new StreamReader(stream2, Encoding.UTF8)) {
                                            return reader.ReadToEnd();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }


        public static async Task<string> ReadXbrlSourceAsync(string filepath) {
            if (File.Exists(filepath)) {
                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read,
                    FileShare.Read, bufferSize: 4096, useAsync: true)) {
                    using (MemoryStream stream = new MemoryStream()) {
                        await fs.CopyToAsync(stream);
                        using (ZipArchive archive = new ZipArchive(stream)) {
                            foreach (ZipArchiveEntry entry in archive.Entries) {
                                FileInfo inf = new FileInfo(entry.FullName);
                                if (inf.FullName.Contains("PublicDoc") && inf.Extension == ".xbrl") {
                                    Encoding enc = Encoding.UTF8;
                                    using (Stream stream2 = entry.Open()) {
                                        using (StreamReader reader = new StreamReader(stream2, enc)) {
                                            return reader.ReadToEnd();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
















//public class Xbrl {
//    public struct Element {
//        public string Name;
//        public string Prefix;
//        public string Label;
//        public string Value;
//        public string Sign;
//        public string ContextRef;
//        public string UnitRef;
//        public string Decimals;
//        public string Nil;
//        public string Attributes;
//        public Dictionary<string, string> DicAttribute;
//        public string Tag;
//    }

//    private XmlDocument doc;
//    public XmlNodeList NodeList { get; private set; }


//    private bool inline;
//    public bool Inline { get { return inline; } }
//    private List<Element> elements = new List<Element>();
//    public List<Element> Elements { get { return elements; } }
//    public Taxonomy Taxonomy { get; private set; }

//    public Xbrl() {
//        Taxonomy = new Taxonomy();
//    }
//    public void IntializeTaxonomy(Dictionary<string,string> dicTaxonomy, List<string> listXml) {
//        Taxonomy.DicTaxonomy = dicTaxonomy;
//        Taxonomy.ListXml = listXml;
//    }
//    public void Load(string source, bool inline) {
//        elements.Clear();
//        try {
//            if (!inline)
//                XbrlFile(source);
//            else
//                InlineXbrl(source);
//        } catch (Exception) {

//            //throw;
//        }

//    }
//    private void InlineXbrl(string source) {
//        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//        //sw.Start();
//        inline = true;
//        doc = new XmlDocument();
//        doc.LoadXml(source);

//        NodeList = doc.GetElementsByTagName("*");
//        XmlNodeList listNonnumeric = doc.DocumentElement.GetElementsByTagName("ix:nonNumeric");
//        XmlNodeList listNonfraction = doc.DocumentElement.GetElementsByTagName("ix:nonFraction");
//        for (int i = 0; i < listNonnumeric.Count; i++) {
//            XmlNode node = listNonnumeric.Item(i);
//            Element element = new Element {
//                Prefix = node.Attributes["name"].InnerText.Split(':')[0],
//                Name = node.Attributes["name"].InnerText.Split(':')[1]
//            };
//            if (Taxonomy.DicTaxonomy.ContainsKey(element.Name))
//                element.Label = Taxonomy.DicTaxonomy[element.Name];
//            element.ContextRef = node.Attributes["contextRef"].InnerText;
//            if (node.Attributes["unitRef"] != null)
//                element.UnitRef = node.Attributes["unitRef"].InnerText;
//            if (node.Attributes["decimals"] != null)
//                element.Decimals = node.Attributes["decimals"].InnerText;
//            if (node.Attributes["xsi:nil"] != null)
//                element.Nil = node.Attributes["xsi:nil"].InnerText;
//            if (node.Attributes["sign"] != null)
//                element.Sign = node.Attributes["sign"].InnerText;
//            element.DicAttribute = new Dictionary<string, string>();
//            StringBuilder attributes = new StringBuilder();
//            for (int j = 0; j < node.Attributes.Count; j++) {
//                if (!"name,contextRef,unitRef,decimals,xsi:nil,sign".Contains(node.Attributes.Item(j).Name)) {
//                    attributes.AppendFormat("{0}={1}", node.Attributes.Item(j).Name, node.Attributes.Item(j).InnerText);
//                    element.DicAttribute.Add(node.Attributes.Item(j).Name, node.Attributes.Item(j).InnerText);
//                }
//            }
//            element.Attributes = attributes.ToString();
//            element.Value = node.InnerText;
//            element.Tag = "nonNumeric";
//            elements.Add(element);
//        }
//        for (int i = 0; i < listNonfraction.Count; i++) {
//            XmlNode node = listNonfraction.Item(i);
//            Element element = new Element {
//                Prefix = node.Attributes["name"].InnerText.Split(':')[0],
//                Name = node.Attributes["name"].InnerText.Split(':')[1]
//            };
//            if (Taxonomy.DicTaxonomy.ContainsKey(element.Name))
//                element.Label = Taxonomy.DicTaxonomy[element.Name];
//            //element.Label = Database.Sqlite.SearchTaxonomy(element.Name);
//            element.ContextRef = node.Attributes["contextRef"].InnerText;
//            if (node.Attributes["unitRef"] != null)
//                element.UnitRef = node.Attributes["unitRef"].InnerText;
//            if (node.Attributes["decimals"] != null)
//                element.Decimals = node.Attributes["decimals"].InnerText;
//            if (node.Attributes["xsi:nil"] != null)
//                element.Nil = node.Attributes["xsi:nil"].InnerText;
//            if(node.Attributes["sign"] !=null) 
//                element.Sign = node.Attributes["sign"].InnerText;
//            StringBuilder attributes = new StringBuilder();
//            element.DicAttribute = new Dictionary<string, string>();
//            for (int j = 0; j < node.Attributes.Count; j++) {
//                if (!"name,contextRef,unitRef,decimals,xsi:nil,sign".Contains(node.Attributes.Item(j).Name)) {
//                    attributes.AppendFormat("{0}={1} ", node.Attributes[j].Name, node.Attributes[j].InnerText);
//                    element.DicAttribute.Add(node.Attributes.Item(j).Name, node.Attributes.Item(j).InnerText);
//                }
//            }
//            element.Attributes = attributes.ToString();
//            element.Value = node.InnerText;
//            element.Tag = "nonFraction";
//            elements.Add(element);
//        }
//        //sw.Stop();
//        //Console.WriteLine(sw.Elapsed);

//    }


//    private void XbrlFile(string source) {
//        inline = false;
//        doc = new XmlDocument();
//        doc.LoadXml(source);



//        ////XmlElement element = doc.GetElementById("NameOfIssuer");
//        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//        //sw.Start();
//        NodeList = doc.GetElementsByTagName("*");
//        //sw.Stop();
//        //Console.WriteLine(sw.Elapsed.Milliseconds);
//        //sw.Restart();
//        //ReadChild(doc.ChildNodes, "");
//        //sw.Stop();
//        //Console.WriteLine(sw.Elapsed.Milliseconds);
//    }
//    public void ReadChild() {
//        ReadChild(doc.ChildNodes, "");
//    }
//    private void ReadChild(XmlNodeList list, string folda) {
//        string[] folders = folda.Split('|');

//        string element = folders[folders.Length - 1];
//        string name = element.Contains(":") ? element.Split(':')[1] : element;
//        string parentLabel = Taxonomy.DicTaxonomy.ContainsKey(name) ? Taxonomy.DicTaxonomy[name] : "";

//        if (parentLabel == "") {
//        }
//        int j = 0;
//        foreach (XmlNode node in list) {
//            if (node.Name == "#text") {
//                string contextRef = null;
//                string unitRef = null;
//                string decimals = null;
//                string nil = null;
//                string sign = null;
//                StringBuilder attributes = new StringBuilder();
//                if (node.ParentNode.Attributes != null && node.ParentNode.Attributes.Count > 0) {
//                    for (int i = 0; i < node.ParentNode.Attributes.Count; i++) {
//                        switch (node.ParentNode.Attributes[i].Name) {
//                            case "contextRef":
//                                contextRef = node.ParentNode.Attributes[i].InnerText;
//                                break;
//                            case "unitRef":
//                                unitRef = node.ParentNode.Attributes[i].InnerText;
//                                break;
//                            case "decimals":
//                                decimals = node.ParentNode.Attributes[i].InnerText;
//                                break;
//                            case "xsi:nil":
//                                nil = node.ParentNode.Attributes[i].InnerText;
//                                break;
//                            case "sign":
//                                sign = node.ParentNode.Attributes[i].InnerText;
//                                break;
//                            default:
//                                if (attributes.Length > 0)
//                                    attributes.Append(" ");
//                                attributes.AppendFormat("{0}={1}", node.ParentNode.Attributes[i].Name, node.ParentNode.Attributes[i].InnerText);
//                                break;
//                        }
//                        //Console.Write("\t{0}={1}", node.ParentNode.Attributes[i].Name, node.ParentNode.Attributes[i].InnerText);
//                    }

//                }
//                string value = node.InnerText.Replace("\n", "").Replace("\r", "");
//                Element xbrl = new Element {
//                    Label = parentLabel,
//                    Name = element.Split(':')[1],
//                    Prefix = element.Split(':')[0],
//                    ContextRef = contextRef,   //xbrli
//                    Decimals = decimals,
//                    UnitRef = unitRef,
//                    Value = value,
//                    Sign = sign,
//                    Nil = nil,
//                    Attributes = attributes.ToString()
//                };
//                elements.Add(xbrl);
//                if (element == "OtherNetPPE") {
//                }
//                j++;
//            }
//            if (node.ChildNodes.Count > 0)
//                ReadChild(node.ChildNodes, folda + "|" + node.Name);
//        }
//    }
//}



