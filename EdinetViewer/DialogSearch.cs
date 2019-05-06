using System;
using System.Data;
using System.Windows.Forms;

namespace EdinetViewer {
    public partial class DialogSearch : Form {
        public DialogSearch(Database.Sqlite database) {
            InitializeComponent();
            db = database;
            buttonOk.Enabled = false;
        }
        private readonly Database.Sqlite db;
        public  DataTable Table;
        private void Button_Click(object sender, EventArgs e) {
            switch ((sender as Button).Name) {
                case "buttonSearch":
                    Table = db.Search(textBoxSql.Text);
                    if(Table.Rows.Count == 0)
                        labelResult.Text = "見つかりませんでした";
                    else if (Table.Rows.Count == 1 & Table.Rows[0]["id"].ToString() == "0") {
                        labelResult.Text = $"{Table.Rows[0]["docID"]}\r\n{Table.Rows[0]["status"]}";
                    } else {
                        labelResult.Text = $"{Table.Rows.Count}件見つかりました";
                        if (Table.Rows.Count > 0)
                            buttonOk.Enabled = true;
                    }
                    break;
                case "buttomOk":
                    break;
                case "buttonCancel":
                    break;
            }

        }

        private void DialogSearch_Shown(object sender, EventArgs e) {
            comboBoxExample.Items.Add("提出者名で検索");
            comboBoxExample.Items.Add("期間で検索");
            comboBoxExample.Items.Add("書類タイプで検索");
            comboBoxExample.Items.Add("複数コードで検索");
            comboBoxExample.Items.Add("ステータスで検索");
            comboBoxExample.Items.Add("書類フラグで検索");
            comboBoxExample.Items.Add("複数条件で検索");
            
            comboBoxExample.SelectedIndexChanged += ComboBoxExample_SelectedIndexChanged;
            comboBoxExample.SelectedIndex = 0;
            listViewExample.View = View.Details;
            listView1.View = View.Details;
            
            //listHoliday.Items.Add(new ListViewItem(cols));
            listView1.Items.Add(new ListViewItem("id, 数値, 年（下2桁）+ 4桁年月 + 4桁seqNo".Split(',')));
            foreach (var kv in Edinet.Const.FieldName) {
                string fieldtype = kv.Key == "seqNo" ? "数値" : "文字列";
                //listView1.Items.Add($"{kv.Key}, {fieldtype}, {kv.Value}");
                listView1.Items.Add(new ListViewItem(new string[] { kv.Key, fieldtype, kv.Value }));
            }
            //listBoxFields.Items.AddRange(
            //    new string[] {
            //        "xbrl, 文字列, ダウンロード済みでは[docid]_1",
            //        "pdf, 文字列, ダウンロード済みでは[docid]_2",
            //        "attach, 文字列, ダウンロード済みでは[docid]_3",
            //        "english, 文字列, ダウンロード済みでは[docid]_4",
            //        "date, 文字列, 日付として比較検索する場合は date(`date`) とする",
            //        "status, 文字列, 縦覧終了などの公開状態",
            //        "code, 数値, 4桁銘柄コード"
            //    }
            //    );
            //listBoxFields.Items.AddRange(
            //    new string[] {
            //        "id, 数値, 年（下2桁）+ 4桁年月 + 4桁seqNo" ,
            //        "seqNo,数値,日付ごとの連番" ,
            //        "docID,文字列," ,
            //        "edinetCode,文字列," ,
            //        "secCode,文字列,銘柄コード5桁（0追加）" ,
            //        "JCN,文字列," ,
            //        "filerName,文字列," ,
            //        ",文字列," ,
            //});
        }

        private void ComboBoxExample_SelectedIndexChanged(object sender, EventArgs e) {
            listViewExample.Items.Clear();
            switch (comboBoxExample.SelectedItem.ToString()) {
                case "提出者名で検索":
                    listViewExample.Items.Add(new ListViewItem(new string[] { "filerName = 'ソフトバンクグループ株式会社'", "" }));
                    listViewExample.Items.Add(new ListViewItem(new string[] { "filerName  like 'レオス%'", "" }));
                    listViewExample.Items.Add(new ListViewItem(new string[] { "filerName like '%五味%'", "" }));
                    listViewExample.Items.Add(new ListViewItem(new string[] { "filerName like '片山%晃'", "" }));
                    //listViewExample.Items.Add(new ListViewItem(new string[] { "", "" }));
                    break;
                case "期間で検索":
                    listViewExample.Items.Add(new ListViewItem(new string[] { "date(`date`) > '2019-04-01'", "4/1以降" }));
                    listViewExample.Items.Add(new ListViewItem(new string[] { "date(`date`) >= '2019-03-15' and date(`date`) <= '2019-04-01'", "3/15から/1の間" }));
                    break;
                case "書類タイプで検索":
                    listViewExample.Items.Add(new ListViewItem(new string[] { "docTypeCode = '120'", "有価証券報告書" }));
                    listViewExample.Items.Add(new ListViewItem(new string[] { "docTypeCode = '140'", "四半期報告書" }));
                    break;
                case "複数コードで検索":
                    listViewExample.Items.Add(new ListViewItem(new string[] { "secCode in ('67580', '65010')", "ソニーと日立" }));
                    listViewExample.Items.Add(new ListViewItem(new string[] { "secCode is not null and cast(secCode as int) > 30000 and cast(secCode as int) < 40000", "3000番台" }));
                    break;
                case "ステータスで検索":
                    listViewExample.Items.Add(new ListViewItem(new string[] { "status is not null", "null以外" }));
                    listViewExample.Items.Add(new ListViewItem(new string[] { "status like '修正%'", "null以外" }));
                    break;
                case "書類フラグで検索":
                    listViewExample.Items.Add(new ListViewItem(new string[] { "xbrlFlag = '1'", "xbrl有" }));
                    listViewExample.Items.Add(new ListViewItem(new string[] { "xbrlFlag != '1' and pdfFlag = '1'", "xbrl無しodf有" }));
                    listViewExample.Items.Add(new ListViewItem(new string[] { "attachDocFlag = '1'", "sttsch有" }));
                    break;
                case "複数条件で検索":
                    listViewExample.Items.Add(new ListViewItem(new string[] { "secCode is not null and xbrlFlag = '1' and date(`date`) > '2019-01-01'", "2019/1/1以降でxbrl書類があるコードを有する書類"}));
                    break;
        }
            listViewExample.Columns[0].Width = 300;
            listViewExample.Columns[1].Width = 200;

        }

        private void ListViewExample_DoubleClick(object sender, EventArgs e) {
            //Console.WriteLine(listViewExample.SelectedItems[0].SubItems[0].Text);
            textBoxSql.Text = listViewExample.SelectedItems[0].SubItems[0].Text;
            Button_Click(buttonSearch, null);
            tabControl1.SelectedIndex = 0;
        }
    }
}
