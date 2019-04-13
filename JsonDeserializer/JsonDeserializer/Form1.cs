using System;
using System.Windows.Forms;

namespace Edinet {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private Disclosures disclosures;
        private string directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        private void Form1_Shown(object sender, EventArgs e) {
            disclosures = new Disclosures();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            disclosures.Dispose();
        }

        private void ButtonJson_Click(object sender, EventArgs e) {
            using (OpenFileDialog dlg = new OpenFileDialog() {
                Title = "Edinet Json ファイルを選択してください",
                InitialDirectory = directory,
                Filter = "JSONファイル(*.json;*.txt)|*.json;*.txt",
                Multiselect = false
            }) {
                DialogResult result = dlg.ShowDialog();
                if(result == DialogResult.OK) {
                    directory = System.IO.Path.GetDirectoryName(dlg.FileName);
                    JsonContent content = disclosures.ReadJsonfile(dlg.FileName);
                    if (content != null) {
                            labelException.Text = "";
                            labelTitle.Text = content.Metadata.Title;
                            labelDate.Text = content.Metadata.Parameter.Date;
                            labelType.Text = content.Metadata.Parameter.Type;
                            labelCount.Text = content.Metadata.Resultset.Count.ToString();
                            labelProcess.Text = content.Metadata.ProcessDateTime;
                            labelStatus.Text = content.Metadata.Status;
                            labelMessage.Text = content.Metadata.Message;
                            dataGridView1.DataSource = content.Table;
                        
                    }
                }
            }
        }

        private void Form1_Resize(object sender, EventArgs e) {
            splitContainer1.SplitterDistance = 150;
        }

        private async void TextBoxDate_TextChanged(object sender, EventArgs e) {
            try {
                if (this.ActiveControl.Text == (sender as ToolStripTextBox).Text) {
                    if (this.ActiveControl.Text.Length > 7 && DateTime.TryParse(this.ActiveControl.Text, out DateTime date)) {
                        JsonContent content = await disclosures.ApiRequest(date, checkBox1.Checked ? RequestDocument.RequestType.Metadata : RequestDocument.RequestType.List);
                        if (content != null) {
                            if (content.Exception != null) {
                                labelException.Text = content.Exception.Message;
                                if (content.Exception.InnerException != null)
                                    labelException.Text += "\r\n" + content.Exception.InnerException.Message;
                                labelTitle.Text = "";
                                labelDate.Text = "";
                                labelType.Text = "";
                                labelCount.Text = "";
                                labelProcess.Text = "";
                                labelStatus.Text = "";
                                labelMessage.Text = "";
                                dataGridView1.DataSource = "";
                            } else {
                                labelException.Text = "";
                                labelTitle.Text = content.Metadata.Title;
                                if (content.Metadata.Parameter != null) {
                                    labelDate.Text = content.Metadata.Parameter.Date;
                                    labelType.Text = content.Metadata.Parameter.Type;
                                } else {
                                    labelDate.Text = "";
                                    labelType.Text = "";
                                }
                                if (content.Metadata.Resultset != null)
                                    labelCount.Text = content.Metadata.Resultset.Count.ToString();
                                else
                                    labelCount.Text = "";
                                if (content.Metadata.ProcessDateTime != null)
                                    labelProcess.Text = content.Metadata.ProcessDateTime;
                                else
                                    labelProcess.Text = "";
                                labelStatus.Text = content.Metadata.Status;
                                labelMessage.Text = content.Metadata.Message;
                                dataGridView1.DataSource = content.Table;
                            }
                        } else {
                            textBoxDate.Text = "";
                            labelException.Text = "過去５年間の日付以外は無効です";
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                
            }

        }

    }
}
