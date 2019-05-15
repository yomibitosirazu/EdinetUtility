using System;
using System.Runtime.CompilerServices;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace debug {

    public class CodeInfo {
        public int Line { get; set; }
        public string Method { get; set; }
        public string Class { get; set; }
        public string File { get; set; }
    }
    public class Info {
        public int Line { get; set; }
        public string Method { get; set; }
        public string File { get; set; }
        public TimeSpan Time { get; set; }
    }

    public class DebugInfo {
        public CodeInfo Position { get; set; }
        public CodeInfo Caller { get; set; }
        public TimeSpan Time { get; set; }
    }

    public class Utility {
        public Queue<DebugInfo> QueueInfo { get; private set; }
        private readonly int count;
        public Utility(int queuCount = 10) {
            count = queuCount;
            QueueInfo = new Queue<DebugInfo>();
        }
        public void SetQueue([CallerLineNumber]int line = 0,
                             [CallerMemberName]string name = "",
                             [CallerFilePath]string path = "",
                             [CallerMemberName] string callername = "",
                             [CallerFilePath] string callerpath = "",
                             [CallerLineNumber] int callerline = 0
                             ) {
            CodeInfo target = new CodeInfo() {
                Line = line,
                Method = name,
                File = path,
            };
            CodeInfo caller = new CodeInfo() {
                Line = callerline,
                Method = callername,
                File = callerpath,
            };
            DebugInfo info = new DebugInfo() {
                Time = DateTime.Now.TimeOfDay,
                Position = target,
                Caller = caller
            };
            if (QueueInfo.Count > count) {
                QueueInfo.Dequeue();
            }
            QueueInfo.Enqueue(info);
        }
    }


    public class ProgramCodeInfo {


        public string Message { get; private set; } //コンストラクターに何も渡さなければ　ファイル名+メソッド名+行番号が格納される
        public string MethodName { get; private set; }  //呼び出したメソッド名
        public string FilePath { get; private set; }    //フルパス
        public string FileName { get; private set; }    //呼び出しファイル名
        public int LineNumber { get; private set; } //呼び出し行番号
        public ProgramCodeInfo(string message = "",
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) {

            MethodName = memberName;
            FilePath = sourceFilePath;
            FileName = Path.GetFileName(FilePath);
            LineNumber = sourceLineNumber;
            if (message == "")
                Message = $"file:{FileName} method:{MethodName} line:{LineNumber}";
            else
                Message = message;
        }


        public static Info GetInfo([CallerLineNumber]int line = 0,
                             [CallerMemberName]string name = "",
                             [CallerFilePath]string path = "") {
            Info info = new Info() {
                Line = line,
                Method = name,
                File = path,
                Time = DateTime.Now.TimeOfDay
            };
            return info;
        }
        //public static int GetLineNo([CallerLineNumber]int line = 0) {
        //    return line;
        //}

        public static Queue<DebugInfo> QueueDebugInfo { get; private set; }
        public static void SetDebugQueue(DebugInfo info) {
            if (QueueDebugInfo == null)
                QueueDebugInfo = new Queue<DebugInfo>();
            if (QueueDebugInfo.Count > 10) {
                QueueDebugInfo.Dequeue();
            }
            QueueDebugInfo.Enqueue(info);
        }

        public static void SetDebugQueue([CallerLineNumber]int line = 0,
                             [CallerMemberName]string name = "",
                             [CallerFilePath]string path = "",
                             [CallerMemberName] string callername = "",
                             [CallerFilePath] string callerpath = "",
                             [CallerLineNumber] int callerline = 0) {
            if (QueueDebugInfo == null)
                QueueDebugInfo = new Queue<DebugInfo>();
            if (QueueDebugInfo.Count > 10) {
                QueueDebugInfo.Dequeue();
            }
            CodeInfo target = new CodeInfo() {
                Line = line,
                Method = name,
                File = Path.GetFileName(path),
            };
            CodeInfo caller = new CodeInfo() {
                Line = callerline,
                Method = callername,
                File = Path.GetFileName(callerpath),
            };
            DebugInfo info = new DebugInfo() {
                Time = DateTime.Now.TimeOfDay,
                Position = target,
                Caller = caller
            };
            QueueDebugInfo.Enqueue(info);
        }


        public static void OutputLog(Exception ex) {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
            //エラーファイルに追記
            string errorfile = "error.log";
            string html = "error.html";

            StreamWriter sw = new StreamWriter(errorfile, true);
            sw.WriteLine(DateTime.Now.ToString() + " : " + ex.Message);
            sw.WriteLine(ex.StackTrace);
            sw.Close();
            sw = new StreamWriter(html, false);
            sw.WriteLine("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"/></head><body>");
            sw.WriteLine($"{DateTime.Now}:{ex.Message}<br>");
            sw.WriteLine(ex.StackTrace.Replace("\n", "\n<br>"));
            if(QueueDebugInfo != null) {
                sw.WriteLine("<table><th>time</th>><th>line</th>><th>method</th>><th>file</th>><th>cline</th>><th>cmethod</th>><th>cfile</th></tr>");
                foreach(var info in QueueDebugInfo) {
                    sw.Write("<tr>");
                    sw.Write($"<td>{info.Time}</td>");
                    sw.Write($"<td>{info.Position.Line}</td><td>{info.Position.Method}</td><td>{info.Position.File}</td>");
                    sw.Write($"<td>{info.Caller.Line}</td><td>{info.Caller.Method}</td><td>{info.Caller.File}</td>");
                    sw.WriteLine("</tr>");
                }
                sw.WriteLine("</table>");
            }
            sw.WriteLine("</body></html>");
            sw.Close();
            //Process.Start("notepad.exe", errorfile);
            Process.Start(html);
            //MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}");
        }



    }
    //public static class ErrorUtil {
    //    public static void OutputLog(Exception ex) {
    //        Debug.WriteLine(ex.Message);
    //        Debug.WriteLine(ex.StackTrace);
    //        //エラーファイルに追記
    //        string errorfile = "error.log";
    //        string html = "error.html";

    //        StreamWriter sw = new StreamWriter(errorfile, true);
    //        sw.WriteLine(DateTime.Now.ToString() + " : " + ex.Message);
    //        sw.WriteLine(ex.StackTrace);
    //        sw.Close();
    //        sw = new StreamWriter(html, false);
    //        sw.WriteLine("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"/></head><body>");
    //        sw.WriteLine($"{DateTime.Now}:{ex.Message}<br>");
    //        sw.WriteLine(ex.StackTrace.Replace("\n","\n<br>"));
    //        sw.WriteLine("</body></html>");
    //        sw.Close();
    //        //Process.Start("notepad.exe", errorfile);
    //        Process.Start(html);
    //        //MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}");
    //    }
    //}

}