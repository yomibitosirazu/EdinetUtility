using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Http;
using System.IO;
using System.Xml.Linq;
using System.Reflection;

namespace Edinet {



    public partial class SettingDialog : Form {
        //private DialogSetting setting;

        public SettingDialog(Setting setting) {
            Setting = setting;
            InitializeComponent();
            numericWait1.Minimum = Setting.Min1;
            numericWait2.Minimum = Setting.Min2;
        }

        private readonly Dictionary<Type, int> dicPosition = new Dictionary<Type, int>() {
                {typeof(TextBox), 2 },
                {typeof(CheckBox), 5 },
                {typeof(NumericUpDown) , 7}
            };

        public Setting Setting { get; set; }
        private void Button_Click(object sender, EventArgs e) {
            switch ((sender as Button).Name) {
                case "buttonOK":
                    if (File.Exists(Path.Combine(Setting.Directory, "edinet.db")) & !Directory.Exists(tbDocumentDirectory.Text) || !File.Exists(Path.Combine(tbDocumentDirectory.Text, "edinet.db"))) {
                        DialogResult result = MessageBox.Show("データーベースは引き継がれませんがいいですか?", "データベースが存在しないフォルダ", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes) {
                            if (!Directory.Exists(tbDocumentDirectory.Text)) {
                                Directory.CreateDirectory(tbDocumentDirectory.Text);
                            }
                        } else {
                            tbDocumentDirectory.Text = Setting.Directory;
                        }
                    }

                    foreach (Control control in this.Controls) {
                        Type type = control.GetType();
                        if (control is TextBox ) {
                            Setting.Values[control.Name.Substring(dicPosition[type])] = this.Controls[control.Name].Text.Replace("\r\n","\t");
                        } else if (control is CheckBox) {
                            Setting.Values[control.Name.Substring(dicPosition[type])] = (control as CheckBox).Checked.ToString();
                        }else if(control is NumericUpDown) {
                            Setting.Values[control.Name.Substring(dicPosition[type])] = (control as NumericUpDown).Value.ToString();
                        }
                    }
                    foreach (Control control in groupBox1.Controls) {
                        if (control is CheckBox) {
                            Setting.Values[control.Name.Substring(5)] = (control as CheckBox).Checked.ToString();
                        }
                    }
                    List<string> list = new List<string>();
                    foreach (int index in listDocType.CheckedIndices) {
                        list.Add(Const.DocTypeCode.ElementAt(index).Key);
                    }
                    Setting.Values["Type"] = string.Join(",", list);
                    StringBuilder sb = new StringBuilder();
                    foreach (ListViewItem item in listHoliday.Items) {
                        sb.AppendFormat("{0},{1}\t", item.SubItems[0].Text, item.SubItems[1].Text);
                    }
                    Setting.Values["Holiday"] = sb.ToString().Trim();
                    this.Close();
                    break;
                case "buttonCancel":
                    this.Close();
                    break;
                case "btnCacheFolda":
                    FolderBrowserDialog dlg = new FolderBrowserDialog() {
                        Description = "閲覧した書類をキャッシュとして保存するディレクトリを選択してください。"
                    };
                    if (dlg.ShowDialog() == DialogResult.OK)
                        tbDocumentDirectory.Text = dlg.SelectedPath;
                    break;
            }
        }

        private void DialogSetting_Shown(object sender, EventArgs e) {
            listDocType.Items.AddRange(Const.DocTypeCode.Values.ToArray());


            foreach (Control control in this.Controls) {
                Type type = control.GetType();
                if (dicPosition.ContainsKey(type)) {
                    string name = control.Name.Substring(dicPosition[type]);
                    if (Setting.Values.ContainsKey(name)) {
                        if (control is TextBox)
                            control.Text = Setting.Values[control.Name.Substring(dicPosition[type])].Replace("\t", "\r\n");
                        else if (control is NumericUpDown & decimal.TryParse(Setting.Values[control.Name.Substring(dicPosition[type])], out decimal value))
                            (control as NumericUpDown).Value = value;
                        else if (control is CheckBox & bool.TryParse(Setting.Values[control.Name.Substring(dicPosition[type])], out bool valbool))
                            (control as CheckBox).Checked = valbool;
                    }
                } else if (control == groupBox1) {
                    foreach (Control sub in control.Controls) {
                        if (sub is CheckBox && Setting.Values.ContainsKey(sub.Name.Substring(5))) {
                            (sub as CheckBox).Checked = Setting.Values[sub.Name.Substring(5)].ToLower() == "true";
                        }
                    }
                } else if (control == listDocType & Setting.Values.ContainsKey("Type")) {
                    string[] types = Setting.Values["Type"].Split(',');
                    foreach (string type1 in types) {
                        if (type1 != "") {
                            int index = Array.IndexOf(Const.DocTypeCode.Keys.ToArray(), type1);
                            listDocType.SetItemCheckState(index, CheckState.Checked);
                        }
                    }
                }else if(control == listHoliday & Setting.Values.ContainsKey("Holiday")) {
                    string[] lines = Setting.Values["Holiday"].Replace("\r\n", "\t").Split('\t');
                    foreach (string line in lines) {
                        string[] cols = line.Split(',');
                        if (cols.Length > 1)
                            listHoliday.Items.Add(new ListViewItem(cols));
                            //listHoliday.Items.Add(cols[0]).SubItems.Add(cols[1]);
                    }
                }

            }
            toolTip1.SetToolTip(tbHolidayCsv, "内閣府の祝日一覧csvまたは類似した祝日の日付と名前一覧のcsvを貼り付けて矢印ボタンを押してください");
            toolTip1.SetToolTip(linkCabinetCsv, "これをクリックすると内閣府の祝日一覧csvを貼り付けます");
        }

        private async void LinkCabinetCsv_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
                client.DefaultRequestHeaders.Add("Accept-Language", "ja-JP");
                client.BaseAddress = new Uri("https://www8.cao.go.jp/");
                using(HttpResponseMessage response = await client.GetAsync("/chosei/shukujitsu/syukujitsu.csv")) {
                    using (Stream stream = await response.Content.ReadAsStreamAsync()) {
                        using (var reader = (new StreamReader(stream, Encoding.GetEncoding("shift_jis"), true)) as TextReader) {
                            string source = await reader.ReadToEndAsync();
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                tbHolidayCsv.Text = source;
                            else
                                tbHolidayCsv.Text = string.Format("{0} {1}", (int)response.StatusCode, response.StatusCode.ToString());
                        }
                    }
                }
            }
        }

        private void ButtonHolidayConvert_Click(object sender, EventArgs e) {
            listHoliday.Items.Clear();
            string[] lines = tbHolidayCsv.Text.Replace("\r", "").Split('\n');
            Dictionary<DateTime, string> dic = new Dictionary<DateTime, string>();
            foreach(string line in lines) {
                string[] cols = line.Replace(",", " ").Replace("\t", " ").Split();
                for(int i = 0; i < cols.Length; i++) {
                    if(DateTime.TryParse(cols[i], out DateTime dt)) {
                        if(dt >= DateTime.Now.Date) {
                            string holiday = "";
                            if (cols.Length > 1)
                                holiday = i < cols.Length - 1 ? cols[i + 1] : cols[i - 1];
                            dic.Add(dt, holiday);
                            listHoliday.Items.Add(new ListViewItem(new string[] { dt.ToString("yyyy-MM-dd"), holiday }));
                        }
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            foreach (var kv in dic) {
                //tbHoliday.AppendText(string.Format("{0:yyyy/MM/dd},{1}\r\n", kv.Key, kv.Value));
                sb.AppendFormat("{0:yyyy/MM/dd}:{1}\r\n", kv.Key, kv.Value);
            }
            if (Setting.Values.ContainsKey("Holiday"))
                Setting.Values["Holiday"] = sb.ToString().Replace("\r\n", "\t");
            else
                Setting.Values.Add("Holiday", sb.ToString().Replace("\r\n", "\t"));
        }

        private void MenuPate_Click(object sender, EventArgs e) {
            if (Clipboard.ContainsText())
                tbHolidayCsv.Text = Clipboard.GetText();
        }

        private void MenuTbHoliday_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Clipboard.ContainsText())
                MenuTbHoliday.Enabled = true;
            else
                MenuTbHoliday.Enabled = false;
        }

        private void MenuCheck_Click(object sender, EventArgs e) {
            if((sender as ToolStripMenuItem).Name == "MenuCheckAll") {
                for (int i=0;i<listDocType.Items.Count;i++)
                    listDocType.SetItemChecked(i, true);
            } else {
                for (int i = 0; i < listDocType.Items.Count; i++)
                    listDocType.SetItemChecked(i, false);
            }
        }

    }


    public class SettingBase {
        public string FilePath { get; set; }
        public Dictionary<string, string> Values { get; set; }
        public static decimal Min1 { get {if (Environment.MachineName == "H270M" | Environment.MachineName == "PD-1712") return 0.2m; else return 0.8m;}}
        public static decimal Min2 { get {if (Environment.MachineName == "H270M" | Environment.MachineName == "PD-1712") return 0.2m; else return 1.0m;}}
        public SettingBase() {
            FilePath = string.Format("{0}_{1}.xml", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, Environment.MachineName);
            Values = new Dictionary<string, string>();
            if (!File.Exists(FilePath))
                Save();
        }

        public void Update(string key, string value) {
            if (Values.ContainsKey(key))
                Values[key] = value;
            else
                Values.Add(key, value);
            Save();
        }

        public void Load() {
            string name = Assembly.GetExecutingAssembly().GetName().Name;
            Values.Clear();
            XDocument doc = XDocument.Load(FilePath);
            XElement root = doc.Element(name);
            foreach (XElement el in root.Elements())
                Values.Add(el.Name.LocalName, el.Value);
        }

        public bool Save() {
            //要素の先頭は数値不可、全角不可 スペース、セミコロン不可
            string name = Assembly.GetExecutingAssembly().GetName().Name;
            XElement root = new XElement(name, from keyValue in Values select new XElement(keyValue.Key, keyValue.Value));
            root.Save(FilePath);
            return true;
        }

    }

    public class Setting:SettingBase {
        public Dictionary<DateTime, string> Holiday {
            get {
                Dictionary<DateTime, string> dic = new Dictionary<DateTime, string>();
                if (Values.ContainsKey("Holiday")) {
                    foreach (string s in Values["Holiday"].Split('\t')) {
                        string[] ss = s.Split(',');
                        if (DateTime.TryParse(ss[0], out DateTime dt) && !dic.ContainsKey(dt))
                            dic.Add(dt, ss[1]);
                    }
                }
                return dic;
            }
        }
        public string Version { get { return Values["Version"]; } }
        public string VersionPrev { get; private set; }
        public bool VersionUp { get; private set; }

        public string ApiVersion { get { return Values["ApiVersion"]; } }
        public string Directory { get { return Values["DocumentDirectory"]; } }
        public bool Timer { get { return bool.TryParse(Values["Timer"], out bool flag) ? flag : false; } }
        public bool Download { get { return bool.TryParse(Values["Download"], out bool flag) ? flag : false; } }
        public bool Xbrl { get { return bool.TryParse(Values["Xbrl"], out bool flag) ? flag : false; } }
        public bool Pdf { get { return bool.TryParse(Values["Pdf"], out bool flag) ? flag : false; } }
        public bool Attach { get { return bool.TryParse(Values["Attach"], out bool flag) ? flag : false; } }
        public bool English { get { return bool.TryParse(Values["Eng"], out bool flag) ? flag : false; } }
        public decimal Interval { get { return Values.ContainsKey("Interval") && decimal.TryParse(Values["Interval"], out decimal value) && value > 1 ? value : 1; } }
        public decimal[] Wait { get {
                //decimal min1 = 0.4m;
                //decimal min2 = 0.4m;
                //if (Environment.MachineName == "H270M" | Environment.MachineName == "PD-1712")
                //    min1 = 0.2m;
                return new decimal[] {
                    Values.ContainsKey("Wait1") && decimal.TryParse(Values["Wait1"], out decimal value) && value > Setting.Min1 ? value : Setting.Min1,
                    Values.ContainsKey("Wait2") && decimal.TryParse(Values["Wait2"], out decimal value2) && value2 > Setting.Min2 ? value2 : Setting.Min2
            };
            } }
        public string[] DocumentTypes { get { return Values.ContainsKey("Type") && Values["Type"].Trim() != "" ? Values["Type"].Split(',') : null; } }
        public int[] Watching { get {
                if (Values.ContainsKey("Watching") && Values["Watching"].Trim() != "") {
                    List<int> list = new List<int>();
                    foreach (string scode in Values["Watching"].Replace("\r\n", ",").Replace("\t", ",").Split(','))
                        if (int.TryParse(scode, out int code))
                            list.Add(code);
                    return list.ToArray();
                } else
                    return null;
                    } }
        public int Left { get { return Values.ContainsKey("FormLeft") && int.TryParse(Values["FormLeft"], out int value) && value >= 0 ? value : 0; } }
        public int Top { get { return Values.ContainsKey("FormTop") && int.TryParse(Values["FormTop"], out int value) && value >= 0 ? value : 0; } }
        public int Width { get { return Values.ContainsKey("FormWidth") && int.TryParse(Values["FormWidth"], out int value) && value >= 0 ? value : 800; } }
        public int Height { get { return Values.ContainsKey("FormHeight") && int.TryParse(Values["FormHeight"], out int value) && value >= 0 ? value : 600; } }
        public int MainDistance { get { return Values.ContainsKey("MainDistance") && int.TryParse(Values["MainDistance"], out int value) && value >= 0 ? value : 180; } }
        public int UpperDistance { get { return Values.ContainsKey("UpperDistance") && int.TryParse(Values["UpperDistance"], out int value) && value >= 0 ? value : 400; } }
        public int LowerDistance { get { return Values.ContainsKey("LowerDistance") && int.TryParse(Values["LowerDistance"], out int value) && value >= 0 ? value : 420; } }
        public DateTime LastVacuum { get { return Values.ContainsKey("Vacuum") && DateTime.TryParse(Values["Vacuum"], out DateTime value) ? value :DateTime.Now.AddDays(-10); } }
        public Setting():base() {
            Load();
        }
        public new void Load() {
            base.Load();
            Initialize();
        }
        private void Initialize() {
            string key = "DocumentDirectory";
            if (Values.Count == 0 || !Values.ContainsKey(key)) {
                FolderBrowserDialog dlg = new FolderBrowserDialog() {
                    Description = "閲覧した書類をキャッシュとして保存するディレクトリを選択してください。\r\nディレクトリ内にTaxonomy等を保存するデーターベースedinet.sqliteがなければ作成されます。\r\n閲覧書類はディレクトリのdocumentsフォルダにdocidごとに保存されます。"
                };
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Edinet");
                if (dlg.ShowDialog() == DialogResult.OK)
                    dir = dlg.SelectedPath;
                Update(key, dir);
            }
            key = "ApiVersion";
            if (!Values.ContainsKey(key)) {
                if (Values.ContainsKey("version")) {
                    Update(key, Values["version"]);
                    Values.Remove("version");
                }else
                    Update(key, "v1");
            }
            if (Values.ContainsKey("version")) 
                Values.Remove("version");
            //string prevVersion = null;
            if (Values.ContainsKey("Version")) {
                VersionPrev = Values["Version"];
            }
            if (Application.ProductVersion != VersionPrev)
                VersionUp = true;

            Values["Version"] = Application.ProductVersion;

            if (!Values.ContainsKey("Timer"))
                Values["Timer"] = false.ToString();
            if (!Values.ContainsKey("Download"))
                Values["Download"] = false.ToString();
            if (!Values.ContainsKey("Xbrl"))
                Values["Xbrl"] = false.ToString();
            if (!Values.ContainsKey("Pdf"))
                Values["Pdf"] = false.ToString();
            if (!Values.ContainsKey("Attach"))
                Values["Attach"] = false.ToString();
            if (!Values.ContainsKey("Eng"))
                Values["Eng"] = false.ToString();
            if (!Values.ContainsKey("Interval"))
                Values["Interval"] = "10";
            if (!Values.ContainsKey("Wait1"))
                Values["Wait1"] = "1.5";
            if (!Values.ContainsKey("Wait2"))
                Values["Wait2"] = "3.0";
            if (!Values.ContainsKey("Type"))
                Values["Type"] = "120,130,140,150,160,170";
            if (!Values.ContainsKey("Holiday")) {
                Values["HolidayCsv"] = "国民の祝日・休日月日,国民の祝日・休日名称	2019/1/1,元日	2019/1/14,成人の日	2019/2/11,建国記念の日	2019/3/21,春分の日	2019/4/29,昭和の日	2019/4/30,休日	2019/5/1,休日（祝日扱い）	2019/5/2,休日	2019/5/3,憲法記念日	2019/5/4,みどりの日	2019/5/5,こどもの日	2019/5/6,休日	2019/7/15,海の日	2019/8/11,山の日	2019/8/12,休日	2019/9/16,敬老の日	2019/9/23,秋分の日	2019/10/14,体育の日（スポーツの日）	2019/10/22,休日（祝日扱い）	2019/11/3,文化の日	2019/11/4,休日	2019/11/23,勤労感謝の日	2020/1/1,元日	2020/1/13,成人の日	2020/2/11,建国記念の日	2020/2/23,天皇誕生日	2020/2/24,休日	2020/3/20,春分の日	2020/4/29,昭和の日	2020/5/3,憲法記念日	2020/5/4,みどりの日	2020/5/5,こどもの日	2020/5/6,休日	2020/7/23,海の日	2020/7/24,スポーツの日	2020/8/10,山の日	2020/9/21,敬老の日	2020/9/22,秋分の日	2020/11/3,文化の日	2020/11/23,勤労感謝の日";
                StringBuilder sb = new StringBuilder();
                foreach (string line in Values["HolidayCsv"].Split('\t')) {
                    string[] cols = line.Split(',');
                    if (DateTime.TryParse(cols[0], out DateTime date) && date >= DateTime.Now.Date)
                        sb.AppendFormat("{0:yyyy-MM-dd},{1}\t", date, cols[1]);
                }
                Values["Holiday"] = sb.ToString().Trim();
            }
        }

        public void Save(int left, int top, int width, int height, int main, int upper, int lower, bool vacuum = false) {
            Values["FormLeft"] = left.ToString();
            Values["FormTop"] = top.ToString();
            Values["FormWidth"] = width.ToString();
            Values["FormHeight"] = height.ToString();
            Values["MainDistance"] = main.ToString();
            Values["UpperDistance"] = upper.ToString();
            Values["LowerDistance"] = lower.ToString();
            if (vacuum)
                Values["Vacuum"] = DateTime.Now.ToString();
            base.Save();
        }
        


    }


}
