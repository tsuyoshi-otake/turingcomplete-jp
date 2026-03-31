# Turing Complete 日本語化MOD

Steam版 [Turing Complete](https://store.steampowered.com/app/1444480/) の日本語翻訳を補完するMODです。

ゲーム本体に同梱されている日本語翻訳は約16%（336/2,161エントリ）しかカバーされていません。本MODはこれを補完し、より多くのUI・レベル説明・ストーリーテキストを日本語で表示できるようにします。

## ゲーム本体との違い

| | ゲーム本体の日本語 | このMOD |
|---|---|---|
| インストール | 不要（同梱済み） | MSI インストーラーで自動 |
| アンインストール | − | 「プログラムの追加と削除」から自動復元 |
| 翻訳カバレッジ | 約16%（336エントリ） | **約84%（1,817エントリ）** |
| 翻訳品質 | コミュニティ翻訳 | 誤訳・未翻訳を修正 |
| 言語設定 | ゲーム内設定に「Japanese」が表示されない場合あり | インストーラーが自動設定 |

## 使い方

### 必要なもの

- Steam版 Turing Complete がインストール済み
- Windows 10/11

### ダウンロード

最新版: **[turingcomplete-jp.msi をダウンロード](https://github.com/tsuyoshi-otake/turingcomplete-jp/releases/latest/download/turingcomplete-jp.msi)**

全バージョン: [Releases ページ](https://github.com/tsuyoshi-otake/turingcomplete-jp/releases)

### インストール

1. 上のリンクから `turingcomplete-jp.msi` をダウンロード
2. **ゲームを終了した状態で** ダウンロードした MSI をダブルクリック
3. 「Windows によって PC が保護されました」と表示された場合は、「詳細情報」をクリックし、「実行」をクリック
4. 使用許諾に同意し、「インストール」をクリック
5. セットアップ完了画面が表示されたら「完了」をクリック

インストーラーが自動的に以下を行います:
- Steam のゲームディレクトリを自動検出
- オリジナルの Japanese.txt をバックアップ
- 翻訳補完済みの Japanese.txt をコピー
- ゲームの言語設定を「Japanese」に自動変更

> **Note:** ゲーム内の設定画面で言語を英語などに変更してしまった場合は、MSI を再実行すれば日本語に戻ります。

### アンインストール

1. 「設定」→「アプリ」→ 検索で「Turing」と入力
2. 「Turing Complete 日本語化MOD」の「...」→「アンインストール」をクリック
3. ウィザードに従って削除

または、インストールに使った MSI ファイルをダブルクリック → 「Remove」を選択

アンインストール時にオリジナルの Japanese.txt と言語設定が復元されます。

> **Note:** Steamのアップデートで翻訳ファイルが上書きされた場合、再度インストーラーを実行してください。

### 手動インストール

MSI を使わない場合:

1. `mod/translations/Japanese.txt` を以下にコピー:
   ```
   <Steamライブラリ>\steamapps\common\Turing Complete\translations\Japanese.txt
   ```
2. ゲームの言語設定を変更:
   `%APPDATA%\godot\app_userdata\Turing Complete\progress.dat` (SQLite) の `setting_language` を `Japanese` に設定

## MSI のビルド方法

[.NET SDK](https://dotnet.microsoft.com/) と [WiX Toolset v6](https://wixtoolset.org/) (dotnet tool) が必要です。

```bash
cd installer
dotnet build MsiPackage/MsiPackage.wixproj -c Release
```

出力: `installer/MsiPackage/bin/Release/ja-JP/MsiPackage.msi`

## 注意事項

- 本ファイル導入によるゲームの安定性・安全性を保証できません。自己責任で使用してください。
- 日本語化コミュニティを取り巻く状況は常に変化しています。権利者様の態度や法律の変化によってこのファイルは予告なく変更・削除される可能性があります。

## リンク

- Turing Complete 公式: https://turingcomplete.game/
- Steam: https://store.steampowered.com/app/1444480/
