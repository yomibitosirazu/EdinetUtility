﻿# 変更履歴
***
## 0.2.209.1  (2019-5-31)

**バグ修正：**
- 取り下げ書類がリストにあるとエラーで落ちる
    - docTypeCodeがnullの場合書類タイプを日本語名に変更しようとしてnull参照をしていた。
***
## 0.2.208.1  (2019-5-18)

**変更**
- EDINETウェブページの詳細検索からダウンロードした書類をインポート可能にした
    - 右クリックメニューに「ウェブページからダウンロードした書類をインポート」を追加
    - zipファイルまたは解答済みのフォルダ内にある「XbrlSearchDlInfo.csv」をファイルダイアログで選択する
    - 書類ごとにzipアーカイブでファイル名{docid}_.zipに変更して保存
    - 日付のメタデータが未取得の場合はメタデータータイプ２を取得してデータベース更新

**バグ修正：**
- コード検索結果がテーブルに表示できなくなっていたため修正
- 四半期報告書または有価証券報告書をダウブルクリックして表示されるReportViewerを閉じた後に再表示できないことがある
    - フォームを閉じた直後はnull参照にならないようなので、IsDisposedのチェックを追加した 

***
## 0.2.207.1  (2019-5-16)

**変更**
- 書類一覧リストで有価証券報告書（120）と四半期報告書（140）の行をダブルクリックすることでXBRLを解析して直接テーブル表示
    - 新たにモードレスでフォームが開く
    - サマリーフィールドへの出力ではなくデータグリッドにした
    - 決算期間ごとに行を変更して項目を表示
    - モードレスにしたことでインラインhtmlと比較確認できる 
    - 右クリックメニューからコピーが可能
- Server Timeout に対するリトライを可能にした
    - ごくまれにタイムアウトエラー（デフォルト100秒）が発生する場合がある
    - リトライ回数を設定ダイアログで指定
    - タイムアウトはめったにないので動作確認が十分ではない
- メタデータのタイプ１とタイプ２の取得コードを多少わかりやすいように変更

***
## 0.2.206.1  (2019-5-12)

**変更**
- Disclosuresテーブルにサマリーカラムを追加
    - 大量保有報告書の概要を出力
    - 書類のダウンロード時に概要をテーブルに格納して、リストテーブルで確認を容易にした
    - 過去の書類リストを表示した場合にダウンロード済みのxbrlを読みサマリー更新
    - メニューからデータベース内のサマリーの更新も可能
- xbrlのパーシングを変更
    - XmlDocumentのGetElementsByTagName("*")ですべてのエレメントを読み込んで解析する方法に変更
    - childを再帰的に読み込むよりも時間が早い
    - ZipEntryを読み込むZipクラスを追加
- その他


***
## 0.2.205.1  (2019-5-8)
**バグ修正：**
- 設定ダイアログで書類タイプを指定していても自動ダウンロードで反映せずにすべてダウンロードしていたのを修正
- リストテーブルのSplitterDistanceが保持できるように工夫

**変更**
- プログレスバーの進行度が不正確だった
    - 多少時間がかかるがテーブル表示一覧または日付ごとのデータ一覧から最初にダウンロード予定書類をリストアップする方法に変更
- リストテーブルの書類をダウンロード（当日分以外）方法を変更
    1. 各書類の日付をチェックしてメタデータリスト取得24時間以上の場合日付リスト再取得
    2. これを繰り返して、リストテーブルのステータスを更新
    3. テーブルリストから対象外、縦覧終了、ダウンロード済みなどを除外した一覧を作成（tableDownload）
    4. このリストをもとに書類をダウンロード
    5. プログレスバーも日付ごとではなくダウンロード予定の書類総数とした 
***
## 0.2.204.2  (2019-5-7)
**バグ修正：**
- タイマーによる当日の書類ダウンロードエラーの修正
    - dicMetadataのnullチェックを追加した
- 当日のメタデータ取得でタイプ1取得をスキップしていたのを修正

**変更**
- タイマーオンの時間内で起動した場合にはタイマー間隔の時間を待たずに、起動直後にメタデータを取得して当日分の書類のダウンロードもするように変更
***
## 0.2.204.1  (2019-5-6)
**変更**
- メタデータ未確定の日付一覧をメッセージボックスで出力する右クリックメニュー追加
    - 当日分は除き、非取得と翌日以降にアクセスせずに確定していない日付一覧を出力
- 書類検索ダイアログを表示する右クリックメニュー追加
    - データベースに保存されたメタデータ（タイプ２）リストから検索
    - 日付にアクセスしていないものは検索できない
    - SQL文のwhere句をテキストボックスに入力して検索
    - いくつかの例文を表示可能　これをダブルクリックすると検索テキストボックスにコピーして検索実行できる
    - テーブル表示ボタンをクリックするとリストテーブルに出力される
- リストテーブルに表示されている書類一覧だけを書類の連続ダウンロード対象とする右クリックメニューを「バックグラウンドで実行」に追加
   - ダウンロードする書類は設定ダイアログで指定した書類　ただし、監視銘柄はすべてダウンロード
- リスト一覧の連続ダウンロード追加に伴い、連続ダウンロードロジックを一部変更
    - 連続ダウンロードの順番を日付＋番号の昇順と降順を設定ダイアログから事前に変更できるようにした。
    - デバッグが十分とは言えず、オブジェクトのインスタンスエラーが出ることがあるかもしれない

##### 書類連続ダウンロードのロジックについて
1. 未取得または確定していないメタデータ日付一覧取得
    1. メタデータテーブルから確定した日付と書類件数を取得
    2. 前日から５年前までループを回してi.リストにない日付一覧を取得
2. disclosuresテーブルから設定ダイアログで指定した未ダウンロード書類を抽出
3. 1.及び2.のリストを昇順または逆順で日付を処理
4. 対象の日付が1.のi.リストに存在して件数がある場合は増加することはないため直接タイプ２の書類一覧メタデータを読む（現時点での縦覧終了などを確認するため）
5. ただし、 processDateTimeが24時間未満の場合は4をスキップしてデータベースのデーターをもとに判断する
6. 未確定日の場合はタイプ１のメタデータを読み、0件はスキップ
7. 確定リストの件数が増えていなかった場合も縦覧終了チェックのためタイプ２も読みに行く
6. 対象日の書類一覧をループ処理
    1. 対象書類が縦覧終了の場合、ローカルファイルをチェックしてダウンロード済みの場合はスキップ
    2. 未ダウンロードであればダウンロード実行
*  リストテーブルの書類をダウンロードする場合は、上記の1.をスキップし2.をリストテーブルのデータに変更し、テーブルに存在する設定ファイルで指定された書類のみダウンロードする


***
## 0.2.203.1  (2019-5-2)
**変更**
- 過去書類の連続ダウンロードのロジック変更
    - リクエストでNotFoundが極力返らないように、閲覧終了書類等確認するためメタデータを再取得してからダウンロードする方法に変更
    - 対象日付変更 > メタデータ（タイプ１）リクエスト > 書類あればメタデータ（タイプ２）リクエスト > 書類ダウンロード
- 連続で書類をダウンロードする場合,HttpResponseMessageの処理等で大幅に時間がかかっていたためロジック変更
    - 連続ダウンロードの場合は非同期処理をawaitしないように変更
    - ただし、ダウンロードエラーなどがメインスレッドで検知できなくなるため、５回に１回はawaitして重大なエラーがあれば中断するようにした。
- ウェイト時間の短縮 
    - 設定ダイアログのウェイトのincrementを0.1秒間隔に変更
    - 日付変更時に多めにウェイトをとるようにしている
- APIアクセスログの日時をミリ秒単位に変更
- APIアクセスログが1MB以上になった場合連番を付けてバックアップ
- APIリクエストのUser-Agentをアプリ名に変更
    -  エクセル形式のEDINET API サンプルプログラムの説明書に従った

** 平日での確認が必要

***
## 0.2.202.1  (2019-4-24)
**バグ修正：**
- 書類連続ダウンロード（バックグラウンド）中にフォームを閉じると「破棄されたオブジェクトにアクセスできません」のエラーを起こしていたのを修正。
- 書類連続ダウンロード（バックグラウンド）のメニューチェックオフ時のキャンセル処理の修正。

***
## 0.2.201.1  (2019-4-17)

**機能追加・変更:**
- Jsonクラスを大幅に変更
- この変更に伴い、Disclosuresクラスも大幅に修正、Apiクラスを別のApi.csファイルに変更
- データベースからのテーブル読み込み変更
    - Disclosuresテーブルスキーマでクローンを作成してこれにデータを読み込む
- API仕様書に従って（サーバー負担軽減のため）、
    1. タイプ1でMetadataの取得　データ増加の確認
    2. Countに変更なければ以前までの書類一覧をデータベースから取得して表示
    3. 増加あればタイプ2をリクエスト　この間のウエイトは極端に減らしている
    4. DisclosuresクラスのReadDocumentsを参照
- ダウンロード済の書類（アーカイブ）の背景色を変更
- JSONファイルのローカル保存はせずにStreamで読み込むだけにした
- その他


***
## 0.2.111.0

**バグ修正：**

**機能追加・変更:**
- 書類アーカイブ取得時に縦覧終了などで404(Not Found)が返されたとき、Disclosureテーブルのxbrl、pdf等のカラムを404で更新して次回以降はダウンロード対象外から外す
- JsonクラスにName属性を追加してファイルを別にした

***
## 0.2.101.7
**バグ修正：**
- リストのカラムヘッダーをクリックした場合にエラーが出て並び替えができなかった

***
## 0.2.101.6
**バグ修正：**
- 書類一覧を取得したときに最新リストが追加されていなかったので修正

**機能追加・変更:**
- インターネット接続がない状況でエラーで落ちないようにした
- 書類一覧でダウンロード済みの書類アーカイブはセルを強調
- 書類一覧で書類の縦覧終了、取り下げ、不開示、修正などの書類の背景色を変更した
- pdf、添付書類、英文が「1」の場合クリックでダウンロードと内容確認できるようにした

***
## 0.2.101.5
**バグ修正：**
- 書類一覧のデータベースinsertにおけるid重複エラーを修正（sqliteの日付カラムの問題）

**機能追加・変更:**
- 過去の書類アーカイブの連続取得を変更
    - 日付ごとに未ダウンロードアーカイブ数をプログレスバーで表示
	- 準備に時間がかかるため、準備中もプログレスバーに進行状態表示
- バージョンチェック方法の変更
***
## 0.2.101.4
**バグ修正：**
- 書類一覧APIでcountが0の時のエラーを修正
- プログレスバー値計算のミスによるバー表示の異常を修正
- 過去の日付を選択した場合のエラーを一部修正
***
## 0.2.101.3
**バグ修正：**
- 書類一覧を連続取得する場合にDataGridViewを参照することで出るエラーを修復
- アーカイブダウンロードのプログレスバーの進行度を修正
- プログレスバーが複数のタスクで参照されるとMaximumが変更されてエラーが出るため、Maximumを100に固定
- データベース等バージョンアップロジックの修正

**機能追加・変更:**
- メニューからAPIリクエスト履歴をブラウザに表示可能にした
- リストの連続取得と書類アーカイブの連続取得中にクローズできるように変更

***
## 0.2.101.2
**バグ修正：**
- カレンダー選択しても日付が変更できなかった
- データベースのUpdateコマンドのエラー修正

**機能追加・変更:**
- タイマーをツールバーのボタンでオンオフ可能にした
- タイマーで一覧取得後に設定で指定したxbrl等アーカイブの自動ダウンロード
- ステータスバーのProgressBarの位置を変更
- Xbrlのvalueフィールドにhtmlが含まれる場合、右クリックメニューまたはダブルクリックで内容をブラウザ表示
***
## 0.2.101.1
(2019-04-08)

**バグ修正：**
- 書類の修正、不開示などステータスに変更があった場合の更新処理エラー修正
- 設定ダイアログでタイマーオンオフ、インターバルの変更をした場合に終了しなければ反映されなかったものを修正

**変更等：**
- タイマーで一覧を取得した場合にステータスバーに表示

***
## 0.2.101.0
(2019-04-07)

**機能追加:**
- 書類一覧のフィルタ
    - コンボボックスで様式コードを選択
    - ブランクですべて
- 設定ダイアログの追加
    - キャッシュディレクトリなどを変更可能にした
    - ウインドウサイズ、位置、スプリッターの位置をClose時に設定ファイルに追加して再起動時に反映
    - 連続リクエストの２つの数値の間で生成した乱数の間ウェイト
    - EDINETのサンプルプログラムは100msecの様だが、安全のため最小は0.5秒としている
- 平日8:30から17:15までの間はタイマーで自動ダウンロード
    - 設定ダイアログでオンオフ可能
    - 設定ダイアログで祝日の追加が可能
    - デフォルトは10分間隔で変更可能（1分以上）
    - 設定ダイアログで書類バイナリの同時ダウンロードも可能
        - 書類も同時にダウンロードをチェック
        - バイナリファイルの書類形式をチェック（複数可）
        - 自動でダウンロードする書類様式をチェック（複数可）


**変更:**
- 書類一覧テーブルのソートを変更submitDateTimeからid（6桁日付＋4桁seqNumber）に変更
    - 書類情報修正があるとsubmitDateTimeが過去の書類もリストアップされseqNumberソートが崩れるため
    - 同一日付でid逆順にすることで連番になる
    - 銘柄検索した場合も日付、連番の逆順で表示される
- バックグラウンドプロセスをBackgroundWorkerからTaskによる非同期処理に変更
    - 同様にHttpClientによるAPI取得などもasync非同期処理に変更
    - 非同期の導入で大幅にDisclosuresクラスを変更
    - 非同期の例外処理が困難で<font color="Fuchsia">エラーで落ちるなどのバグが残っている可能性が高い</font>
- 右クリックメニューのグループ化
    - フォアグラウンド系と非同期タスクメニューに分離
    - 過去のデータを連続取得する非同期系メニューはチェックボックスのオンオフに変更
    - オフによりタスクをキャンセル 
- 設定ダイアログ追加にあたり設定クラス名と構造を見直し変更
- データベーステーブルの一部変更
    - 書類一覧テーブルにdate、satate（縦覧終了、修正等の情報）などを追加
    - １週間以上経過した場合Vacuumを実行する

予定：データベース接続を一つにして使いまわし。
連続の場合はこの間を接続しっぱなし、Satateをチェックしてwaiting

**未知の可能性があるバグなど**
- 同時にSqliteの書き込みが起こる可能性がある
    - データベースのロックが原因でエラーが出現する可能性がある
- 非同期処理のバグつぶしが不完全である可能性
    -  オブジェクト参照エラーなどのエラーが出現する可能性がある
- その他

**Fixed bugs:**
- 過去リストの取得と過去書類のダウンロードを実装して右クリックメニューのチェックで実行オンオフ。
- バックグラウンドプロセス実行中にバックグラウンド関係のメニューが実行できないように修正。
- 書類一覧テーブル変更時にCurrentCellChangedイベントで時間がかかりすぎるのを修正。
***
## 0.1.000
(2019-03-27)

初公開