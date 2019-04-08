# Edinet API を利用したユーティリティー
VisualStudio2017 C#を利用して作成した EDINET API を利用するユーティリティーです。
## [EdinetViewer](https://github.com/yomibitosirazu/EdinetUtility/tree/master/EdinetViewer)
EDINETの開示書類を表示するだけのC#Windowsフォームアプリケーション  
EDINET API を利用して日付ごとの開示書類のリスト一覧を取得  
一覧から選択したXBRLの開示書類をAPIでダウンロードしてエレメントを解析してテーブル表示

### バージョンについて
変更点についてはEdinetViewerフォルダ内の[CHANGELOG.md](EdinetViewer/CHANGELOG.md)を参照してください。
#### v0.2.101.0 マスターブランチ（現在のブランチ）
- APIの取得を非同期に変更
- 多少の並列処理を可能
- 開発途中で不安定
- SqliteCoreのアンインストールとインストールが必要
    - Githubにプッシュする段階でSqliteをアンインストールしていましたが忘れることが多いです
    - NugetパッケージマネージャでSqliteCoreがインストール済みであれば、一度アンインストールして再度「参照」でsqliteを検索して再度インストールしてください
#### v0.1.00 ブランチv0100 初期公開バージョン
- APIの取得をHttpClientの同期処理を利用

<img src="https://github.com/yomibitosirazu/EdinetUtility/blob/master/EdinetViewer/images/DisclosureViewer.png">

## 開発環境
Microsoft Visual Studio Community 2017   
Windows10 Pro 64bit  
Windows10 Home 32bit (Pararells Desktop 13 for Mac)
