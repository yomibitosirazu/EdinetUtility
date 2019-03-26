using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Reflection;

namespace EdinetViewer {
    class Env {
        private readonly string settingfile = string.Format("{0}_{1}.xml", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, Environment.MachineName);
        public Dictionary<string, string> Settings { get; set; }
        public Env() {
            Load();
        }
        public bool Save() {
            //要素の先頭は数値不可、全角不可 スペース、セミコロン不可
            string name = Assembly.GetExecutingAssembly().GetName().Name;
            XElement root = new XElement(name,
                    from keyValue in Settings
                    select new XElement(keyValue.Key, keyValue.Value));
            root.Save(settingfile);
            return true;
        }
        public void Update(string key, string value) {
            if (Settings.ContainsKey(key))
                Settings[key] = value;
            else
                Settings.Add(key, value);
            Save();
        }
        public void Load() {
            string name = Assembly.GetExecutingAssembly().GetName().Name;
            Settings = new Dictionary<string, string>();
            if (!File.Exists(settingfile))
                return;
            XDocument doc = XDocument.Load(settingfile);
            XElement root = doc.Element(name);
            foreach (XElement el in root.Elements())
                Settings.Add(el.Name.LocalName, el.Value);
        }
    }
}
