# JsonDeserializer for EDINET API

ローカルに保存したWDINET APIレスポンスJSONファイルをデシリアライズしてメタデータ、書類一覧を出力するWindowsFormアプリ。

EDINET API用のJSONクラスの構造とデシリアライズを大きく変更したので、
正常に機能するかを確認するための小物ツール。

書類一覧取得のみで、書類アーカイブの取得は実装していない。


<img src="https://github.com/yomibitosirazu/EdinetUtility/blob/master/JsonDeserializer/JsonDeserializer/images/Deserializer.png">  

- ①ファイルボタン
    - ボタンをクリックするとファイルダイアログでJSONファイルを選択できる
- ②日付入力ボックス
    - 過去５年間の有効な日付を入力するとEDINETにリクエスト送信してレスポンスを出力する
    - ③チェックを外すとタイプ２をリクエストして書類一覧も取得
- ④HttpClientのHttpResponseMessageでエラーがあるとここに出力される


## 開発環境
Microsoft Visual Studio Community 2019
.NET FrameWork 4.7.2    
Windows10 Pro 64bit  
Windows10 Home 32bit (Pararells Desktop 13 for Mac)
