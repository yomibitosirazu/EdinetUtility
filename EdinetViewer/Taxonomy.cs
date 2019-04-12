using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;


namespace Disclosures {
    public class Taxonomy {
        public Dictionary<string, string> DicTaxonomy { get; set; }
        public List<string> ListXml { get; set; }
        public Taxonomy() { }
        public Taxonomy(Dictionary<string,string> dicTaxonomy, List<string> listXml) {
            DicTaxonomy = dicTaxonomy;
            ListXml = listXml;
        }

        public void ReadXsd(string source, ref Database.Sqlite database) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(source);
            XmlNodeList listLink = doc.GetElementsByTagName("link:linkbaseRef");
            foreach (XmlNode node in listLink) {
                string link = node.Attributes["xlink:href"].InnerText;
                if (ListXml == null)
                    ListXml = new List<string>();
                string[] path = link.Split('/');
                //ListXmlに取得済みのタクソノミxmlファイル一覧を追加し　一覧にない場合にxmlをダウンロードして読み込み
                if (link.Substring(0, 4) == "http" && !ListXml.Contains(path[path.Length - 1])) {
                    ListXml.Add(path[path.Length - 1]);
                    ImportTaxonomy(link, ref database);
                }
            }

        }

        public void ImportTaxonomy(string url, ref Database.Sqlite database) {
            string source = null;
            //string lastmodified = null;

            using (System.Net.WebClient wc = new System.Net.WebClient()) {
                using (Stream st = wc.OpenRead(url)) {
                    using (StreamReader sr = new StreamReader(st, Encoding.UTF8)) {
                        source = sr.ReadToEnd();
                        //if (wc.ResponseHeaders["Last-Modified"] != null) {
                        //    lastmodified = wc.ResponseHeaders["Last-Modified"];
                        //}
                    }
                }
            }

            string[] path = url.Split('/');
            //string fn = path[path.Length - 1];
            Console.WriteLine("  reading Taxonomy..  {0}", path[2]);
            XmlDocument document = new XmlDocument();
            document.LoadXml(source.Replace("<gen:arc", "<link:labelArc")
                             .Replace("<label:label", "<link:label").Replace("</label:label>", "</link:label>"));
            XmlNodeList locs = document.GetElementsByTagName("link:loc");
            XmlNodeList labels = document.GetElementsByTagName("link:label");
            XmlNodeList arcs = document.GetElementsByTagName("link:labelArc");
            if (locs.Count == 0)
                locs = document.GetElementsByTagName("loc");
            if (labels.Count == 0)
                labels = document.GetElementsByTagName("label");
            if (arcs.Count == 0)
                arcs = document.GetElementsByTagName("labelArc");
            if (arcs.Count == 0)
                arcs = document.GetElementsByTagName("link:definitionArc");
            if (locs.Count == 0 | labels.Count == 0 | arcs.Count == 0) {
                return;
            }
            //Dictionary<string, string> dicArc = new Dictionary<string, string>();
            Dictionary<string, XmlNode> dicLoc = new Dictionary<string, XmlNode>();
            Dictionary<string, string[]> dicLabel = new Dictionary<string, string[]>();

            for (int i = 0; i < locs.Count; i++) {
                dicLoc.Add(locs[i].Attributes["xlink:label"].InnerText, locs[i]);
            }
            for (int i = 0; i < labels.Count; i++) {
                dicLabel.Add(labels[i].Attributes["xlink:label"].InnerText
                             , new string[] { labels[i].InnerText, labels[i].Attributes["xml:lang"].InnerText });
            }

            database.SaveTaxonomy(DicTaxonomy, url, arcs, dicLoc, dicLabel);
        }
    }
}