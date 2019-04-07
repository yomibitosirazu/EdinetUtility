using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;


namespace Disclosures {
    public class Xbrl {
        public struct Element {
            public string Name;
            public string Prefix;
            public string Label;
            public string Value;
            public string Sign;
            public string ContextRef;
            public string UnitRef;
            public string Decimals;
            public string Nil;
            public string Attributes;
            public Dictionary<string, string> DicAttribute;
            public string Tag;
        }


        private bool inline;
        public bool Inline { get { return inline; } }
        private List<Element> elements = new List<Element>();
        public List<Element> Elements { get { return elements; } }
        public Taxonomy Taxonomy { get; private set; }

        public Xbrl() {
            Taxonomy = new Taxonomy();
        }
        public void IntializeTaxonomy(Dictionary<string,string> dicTaxonomy, List<string> listXml) {
            Taxonomy.DicTaxonomy = dicTaxonomy;
            Taxonomy.ListXml = listXml;
        }
        public void Load(string source, bool inline) {
            elements.Clear();
            try {
                if (!inline)
                    XbrlFile(source);
                else
                    InlineXbrl(source);
            } catch (Exception) {

                //throw;
            }

        }
        private void InlineXbrl(string source) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            inline = true;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(source);

            XmlNodeList listNonnumeric = doc.DocumentElement.GetElementsByTagName("ix:nonNumeric");
            XmlNodeList listNonfraction = doc.DocumentElement.GetElementsByTagName("ix:nonFraction");
            for (int i = 0; i < listNonnumeric.Count; i++) {
                XmlNode node = listNonnumeric.Item(i);
                Element element = new Element {
                    Prefix = node.Attributes["name"].InnerText.Split(':')[0],
                    Name = node.Attributes["name"].InnerText.Split(':')[1]
                };
                if (Taxonomy.DicTaxonomy.ContainsKey(element.Name))
                    element.Label = Taxonomy.DicTaxonomy[element.Name];
                element.ContextRef = node.Attributes["contextRef"].InnerText;
                if (node.Attributes["unitRef"] != null)
                    element.UnitRef = node.Attributes["unitRef"].InnerText;
                if (node.Attributes["decimals"] != null)
                    element.Decimals = node.Attributes["decimals"].InnerText;
                if (node.Attributes["xsi:nil"] != null)
                    element.Nil = node.Attributes["xsi:nil"].InnerText;
                if (node.Attributes["sign"] != null)
                    element.Sign = node.Attributes["sign"].InnerText;
                element.DicAttribute = new Dictionary<string, string>();
                StringBuilder attributes = new StringBuilder();
                for (int j = 0; j < node.Attributes.Count; j++) {
                    if (!"name,contextRef,unitRef,decimals,xsi:nil,sign".Contains(node.Attributes.Item(j).Name)) {
                        attributes.AppendFormat("{0}={1}", node.Attributes.Item(j).Name, node.Attributes.Item(j).InnerText);
                        element.DicAttribute.Add(node.Attributes.Item(j).Name, node.Attributes.Item(j).InnerText);
                    }
                }
                element.Attributes = attributes.ToString();
                element.Value = node.InnerText;
                element.Tag = "nonNumeric";
                elements.Add(element);
            }
            for (int i = 0; i < listNonfraction.Count; i++) {
                XmlNode node = listNonfraction.Item(i);
                Element element = new Element {
                    Prefix = node.Attributes["name"].InnerText.Split(':')[0],
                    Name = node.Attributes["name"].InnerText.Split(':')[1]
                };
                if (Taxonomy.DicTaxonomy.ContainsKey(element.Name))
                    element.Label = Taxonomy.DicTaxonomy[element.Name];
                //element.Label = Database.Sqlite.SearchTaxonomy(element.Name);
                element.ContextRef = node.Attributes["contextRef"].InnerText;
                if (node.Attributes["unitRef"] != null)
                    element.UnitRef = node.Attributes["unitRef"].InnerText;
                if (node.Attributes["decimals"] != null)
                    element.Decimals = node.Attributes["decimals"].InnerText;
                if (node.Attributes["xsi:nil"] != null)
                    element.Nil = node.Attributes["xsi:nil"].InnerText;
                if(node.Attributes["sign"] !=null) 
                    element.Sign = node.Attributes["sign"].InnerText;
                StringBuilder attributes = new StringBuilder();
                element.DicAttribute = new Dictionary<string, string>();
                for (int j = 0; j < node.Attributes.Count; j++) {
                    if (!"name,contextRef,unitRef,decimals,xsi:nil,sign".Contains(node.Attributes.Item(j).Name)) {
                        attributes.AppendFormat("{0}={1} ", node.Attributes[j].Name, node.Attributes[j].InnerText);
                        element.DicAttribute.Add(node.Attributes.Item(j).Name, node.Attributes.Item(j).InnerText);
                    }
                }
                element.Attributes = attributes.ToString();
                element.Value = node.InnerText;
                element.Tag = "nonFraction";
                elements.Add(element);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);

        }


        private void XbrlFile(string source) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            inline = false;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(source);
            ReadChild(doc.ChildNodes, "");
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }
        private void ReadChild(XmlNodeList list, string folda) {
            string[] folders = folda.Split('|');

            string element = folders[folders.Length - 1];
            string name = element.Contains(":") ? element.Split(':')[1] : element;
            string parentLabel = Taxonomy.DicTaxonomy.ContainsKey(name) ? Taxonomy.DicTaxonomy[name] : "";

            if (parentLabel == "") {
            }
            int j = 0;
            foreach (XmlNode node in list) {
                if (node.Name == "#text") {
                    string contextRef = null;
                    string unitRef = null;
                    string decimals = null;
                    string nil = null;
                    string sign = null;
                    StringBuilder attributes = new StringBuilder();
                    if (node.ParentNode.Attributes != null && node.ParentNode.Attributes.Count > 0) {
                        for (int i = 0; i < node.ParentNode.Attributes.Count; i++) {
                            switch (node.ParentNode.Attributes[i].Name) {
                                case "contextRef":
                                    contextRef = node.ParentNode.Attributes[i].InnerText;
                                    break;
                                case "unitRef":
                                    unitRef = node.ParentNode.Attributes[i].InnerText;
                                    break;
                                case "decimals":
                                    decimals = node.ParentNode.Attributes[i].InnerText;
                                    break;
                                case "xsi:nil":
                                    nil = node.ParentNode.Attributes[i].InnerText;
                                    break;
                                case "sign":
                                    sign = node.ParentNode.Attributes[i].InnerText;
                                    break;
                                default:
                                    if (attributes.Length > 0)
                                        attributes.Append(" ");
                                    attributes.AppendFormat("{0}={1}", node.ParentNode.Attributes[i].Name, node.ParentNode.Attributes[i].InnerText);
                                    break;
                            }
                            //Console.Write("\t{0}={1}", node.ParentNode.Attributes[i].Name, node.ParentNode.Attributes[i].InnerText);
                        }

                    }
                    string value = node.InnerText.Replace("\n", "").Replace("\r", "");
                    Element xbrl = new Element {
                        Label = parentLabel,
                        Name = element.Split(':')[1],
                        Prefix = element.Split(':')[0],
                        ContextRef = contextRef,   //xbrli
                        Decimals = decimals,
                        UnitRef = unitRef,
                        Value = value,
                        Sign = sign,
                        Nil = nil,
                        Attributes = attributes.ToString()
                    };
                    elements.Add(xbrl);
                    if (element == "OtherNetPPE") {
                    }
                    j++;
                }
                if (node.ChildNodes.Count > 0)
                    ReadChild(node.ChildNodes, folda + "|" + node.Name);
            }
        }
    }
}