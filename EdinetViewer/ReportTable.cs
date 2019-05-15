using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace EdinetViewer {
    public partial class ReportTable : Form {

        private Dictionary<string, string> contents;
        private DataView dv;
        public ReportTable() {
            InitializeComponent();
        }

        public void ChangeSource(DataTable table, Dictionary<string, string> columns, Dictionary<string, string> dic) {
            contents = dic;
            dv = new DataView(table, "", "", DataViewRowState.CurrentRows);
            dataGridView1.DataSource = dv;
            foreach (DataGridViewColumn col in dataGridView1.Columns) {
                if (columns.ContainsKey(col.Name)) {
                    col.HeaderText = columns[col.Name];
                    col.ToolTipText = col.Name;
                }
            }
            for (int i = 2; i < dataGridView1.ColumnCount; i++)
                dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            for (int i = 0; i < dataGridView1.RowCount; i++)
                if (Edinet.Const.Context.ContainsKey(dataGridView1.Rows[i].Cells[0].Value.ToString()))
                    dataGridView1.Rows[i].Cells[0].ToolTipText = Edinet.Const.Context[dataGridView1.Rows[i].Cells[0].Value.ToString()];
            StringBuilder sb = new StringBuilder();
            if (dic.ContainsKey("SecurityCodeDEI"))
                sb.Append(dic["SecurityCodeDEI"]);
            if (dic.ContainsKey("CompanyNameCoverPage"))
                sb.Append(" " + dic["CompanyNameCoverPage"]);
            if (dic.ContainsKey("DocumentTitleCoverPage"))
                sb.Append(" " + dic["DocumentTitleCoverPage"]);
            if (dic.ContainsKey("QuarterlyAccountingPeriodCoverPage"))
                sb.Append(" " + dic["QuarterlyAccountingPeriodCoverPage"]);
            this.Text = sb.ToString();
            this.BringToFront();
        }

        private void Menu_Click(object sender, EventArgs e) {
            switch((sender as ToolStripMenuItem).Name) {
                case "MenuCopy":
                    dataGridView1.SelectAll();
                    dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
                    Clipboard.SetDataObject(dataGridView1.GetClipboardContent());
                    break;
                case "MenuInfo":
                    StringBuilder sb = new StringBuilder();
                    foreach(var kv in contents) {
                        sb.AppendLine($"{kv.Key} {kv.Value}");
                    }
                    MessageBox.Show(sb.ToString());
                    break;
            }
        }
    }
}
