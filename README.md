# Edinet API を利用したユーティリティー
VisualStudio2017 C#を利用して作成した EDINET API を利用するユーティリティーです。
***

## [JsonDeserializer](https://github.com/yomibitosirazu/EdinetUtility/tree/master/JsonDeserializer/JsonDeserializer)
EdinetViewerのJSONクラス設計変更で、このクラスのデシリアライズが機能しているかを確認するための小物ツール（Windowsフォームアプリ）。
***

## [EdinetViewer](https://github.com/yomibitosirazu/EdinetUtility/tree/master/EdinetViewer)
EDINETの開示書類を表示するだけのC#Windowsフォームアプリケーション  
EDINET API を利用して日付ごとの開示書類のリスト一覧を取得  
一覧から選択したXBRLの開示書類をAPIでダウンロードしてエレメントを解析してテーブル表示

***
### 直近の変更    
#### 2019-4-17 0.2.201.1
**機能追加・変更:**
- Jsonクラスを大幅に変更
- Disclosuresクラスも修正、Apiクラスを別のApi.csファイルに変更
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

＊仕様変更に対するエラートラップが不十分のため、状況によってエラーで落ちる可能性があります。


#### SqliteCoreのアンインストールとインストールが必要
- Githubにプッシュする段階でSqliteをアンインストールしていましたが忘れることが多いです
- NugetパッケージマネージャでSqliteCoreがインストール済みであれば、一度アンインストールして再度「参照」でsqliteを検索して再度インストールしてください
***

### バージョンについて
変更点についてはEdinetViewerフォルダ内の[CHANGELOG.md](EdinetViewer/CHANGELOG.md)を参照してください。
#### v0.2.1-- マスターブランチ（現在のブランチ）
- APIの取得を非同期に変更
- 多少の並列処理を可能
- 開発途中で不安定
-#### v0.1.00 ブランチv0100 初期公開バージョン
- APIの取得をHttpClientの同期処理を利用

<img src="https://github.com/yomibitosirazu/EdinetUtility/blob/master/EdinetViewer/images/DisclosureViewer.png">

## 開発環境
Microsoft Visual Studio Community 2017   
Windows10 Pro 64bit  
Windows10 Home 32bit (Pararells Desktop 13 for Mac)
