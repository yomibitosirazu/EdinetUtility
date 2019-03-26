using System.Runtime.CompilerServices;
using System.IO;

namespace Debug {
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
    }
}