using System;
using System.Windows.Forms;

namespace Edinet {
    static class Program {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try {
                Application.Run(new Form1());
            } catch (Exception ex) {
                debug.ProgramCodeInfo.OutputLog(ex);
            }
        }
    }
}
