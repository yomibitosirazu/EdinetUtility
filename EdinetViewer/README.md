# EdinetViewer

WebAPIを利用してEdinetの開示書類を閲覧するアプリ。
[実行画面](https://github.com/yomibitosirazu/EdinetUtility/blob/master/EdinetViewer/images/DisclosureViewer.png)
## Description
1. EdinetのWebAPIを介して、指定した日付の開示書類リストをJSONレスポンスで取得。
2. JSON生データはWebBrowserに出力、リストはテーブル表示。
3. リスト選択してxbrlを含んでいればzipアーカイブをダウンロードして内部ファイルリスト一覧をテーブル表示。
4. 内部ファイルリストからファイルを選択すると内容をWebBrowserで表示。
5. このファイルがxbrlまたはインラインxbrlであれば、抽出エレメントをタクソノミと照合しラベル変換して一覧表示。
6. 過去の日付の書類一覧をバックグラウンドでダウンロードできる。
7. 書類一覧リストはSqlite3データベースに保存するため、コードを指定して銘柄の開示書類一覧をテーブル表示可能。
8. XBRLに関しては選択書類の閲覧だけで、売上等を時系列で比較するなどの機能はなし（これをもとに拡張可能と考える）。


## 準備
1. ダウンロードした書類を保存するキャッシュフォルダを選択 
2. 最新のタクソノミ（EDINETタクソノミ - 最新 - 00 . 全様式一括）をzipファイルとしてダウンロードしてキャッシュフォルダにコピー  
　　1.に関しては、初回起動時にフォルダダイアログが表示されて選択することで設定ファイルに保存される。  
　　2.に関しても、初回起動のブラウザからタクソノミのリンクをたどり1.のフォルダに保存するとフォルダ監視により自動的にタクソノミを解析してデーターベースに追加される。
3. デバッグまたはビルドでSqliteのエラーが出る場合には、Nugetでインストールが必要。  
    「ツール」「NuGetパッケージマネージャー」「ソリューションのNuGetパッケージの管理」 

## 使い方
[スタート画面](https://github.com/yomibitosirazu/EdinetUtility/blob/master/EdinetViewer/images/001start.png)
初回起動時に作業ディレクトリ（キャッシュファイル保存ディレクトリ）をファイルダイアログで選択します。

[右クリックメニュー](https://github.com/yomibitosirazu/EdinetUtility/blob/master/EdinetViewer/images/002.png)  
[タクソノミダウンロード](https://github.com/yomibitosirazu/EdinetUtility/blob/master/EdinetViewer/images/003.png)  
[画面](https://github.com/yomibitosirazu/EdinetUtility/blob/master/EdinetViewer/images/004.png)
1. ステータスバー　タクソノミのダウンロード後にバックグラウンドでデータベースを構築している
2. 日付を選択するDateTimePicker 選択した日付の書類一覧リストをAPIリクエスト
　　丸一日以上経過して確定したキャッシュがある場合はAPIを投げずにこれを読み込む
　　当日分もクリックすることで取得する（当日分は0時を過ぎてprocessDateTimeが翌日に確定するまではクリックするごとにAPIリクエスト）
3. リクエストに対するレスポンスを要約して表示
4. レスポンスを受け取るとその時点でブラウザに表示
　　リクエストはタイプ2
5. レスポンスのResult書類一覧をsubmitDateTime逆順でテーブル表示
6. ５の任意のセルをクリックするとその行の書類取得APIリクエストを投げる
　　xbrlFlag = "1" の場合はXbrlをzipアーカイブでダウンロード
　　pdfFlag = "1" の場合はPDFをダウンロードしてブラウザに表示
　　代替書面・添付文書、英文ファイルに関しては未実装
　　Xbrlをダウンロードするとアーカイブを読み込み⑥のテーブルに一覧表示
7. ⑥に表示された任意のセルをクリックするとXbrlとインラインXbrlに関しては⑦にエレメント一覧を出力
　　同時に④のブラウザにも出力
　　Xbrl以外も④に表示されるが、xmlの一部は表示されないことがある（右クリックからソースを開いて確認可能）
8. ４桁銘柄コードを入力してデーターベースに銘柄があれば提出日の逆順で一覧表示
　　過去の書類一覧を取得していなければヒットしない
9. 将来APIバージョンが変わった場合ラベルをダブルクリックすることでテキストボックス編集が可能
　　バージョン情報はディレクトリ情報とともに、ビルド先のフォルダにマシン名のxmlファイルで保存
[画面](https://github.com/yomibitosirazu/EdinetUtility/blob/master/EdinetViewer/images/005.png)  
[画面](https://github.com/yomibitosirazu/EdinetUtility/blob/master/EdinetViewer/images/006.png)  


## 注記

1. xmlファイルによってはWebBrowserで内容が表示されないものがありますが、この場合は右クリックから「ソースの表示」で確認してください。
2. ツールバー右クリックメニューの「過去5年間の書類一覧取得」はバックグラウンドスレッドで実行します。  
　　ダウンロード実行中に日付を選択して保存済みリストの表示なければ新たにリクエストを投げます。  
　　また、リストを選択することでxbrlの書類もダウンロードします。  
　　十分に検証していませんが<span style="background-color: #ffff00;">HttpClientが同時に別のリクエストを投げることでエラーを起こす場合があるかもしれません。</span>  
　　<span style="background-color: #ffff00;">短時間の大量アクセスは禁止事項</span>に挙げられているため、これに抵触する可能性も考えられるため過去データの自動収集はウェイトを多めにとっています。  
3. タクソノミについては構造とルールを理解している若ではありません。  
    独自の解釈で間違っている可能性もあり、Xmlエレメントとの照合が正しいとは限りません。
