using System;
using System.Runtime.CompilerServices;
using System.IO;
using System.Collections.Generic;

namespace debug {
    public class Info {
        public int Line { get; set; }
        public string Method { get; set; }
        public string File { get; set; }
        public TimeSpan Time { get; set; }
    }

    class ProgramCodeInfo {


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
        public static int GetLineNo([CallerLineNumber]int line = 0) {
            return line;
        }

        public static Queue<debug.Info> QueueDebugInfo { get; private set; }
        public static void SetDebugQueue(debug.Info info) {
            if (QueueDebugInfo == null)
                QueueDebugInfo = new Queue<Info>();
            if (QueueDebugInfo.Count > 10) {
                QueueDebugInfo.Dequeue();
            }
            QueueDebugInfo.Enqueue(info);
        }

    }
}