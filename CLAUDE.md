# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Turing Complete（Steam版パズルゲーム、App ID: 1444480）の日本語翻訳を補完するMOD。ゲーム本体の翻訳は約16%しかカバーされていないため、不足分を補完し、翻訳品質を改善する。

## Build

```bash
cd installer
dotnet build MsiPackage/MsiPackage.wixproj -c Release
```

出力: `installer/MsiPackage/bin/Release/ja-JP/MsiPackage.msi`

必要ツール: .NET SDK, WiX Toolset v6 dotnet tool (`dotnet tool install -g wix`), WixToolset.Dtf.CustomAction NuGet パッケージ

## Architecture

### `installer/CustomAction/` (C# net472 クラスライブラリ)
WiX カスタムアクション DLL。MSI のインストール/アンインストール時に実行されるロジック:
- **FindGameDirectory** (immediate CA): Steam の `libraryfolders.vdf` を解析してゲームディレクトリを自動検出し、`GAMEDIR` プロパティを設定
- **InstallMod** (immediate CA): ゲームの `translations/Japanese.txt` をバックアップし、埋め込みリソースから翻訳補完済みの Japanese.txt を展開・コピー
- **UninstallMod** (immediate CA): バックアップから `Japanese.txt` を復元

MODファイルは `<EmbeddedResource>` としてDLLに同梱。`Assembly.GetManifestResourceStream()` で実行時に抽出。

### `installer/MsiPackage/` (WiX v6 SDK プロジェクト)
MSI パッケージ定義:
- `Package.wxs` — パッケージ定義、カスタムアクションのスケジューリング、WixUI_Minimal
- `Package.ja-JP.wxl` — UI 文字列の日本語ローカライゼーション
- `License.rtf` — 使用許諾
- perUser スコープ、Codepage 65001（パッケージ）/ 932（ローカライゼーション）

### `mod/` ディレクトリ
ゲームの `translations/` に上書きされる翻訳リソース:
- `translations/Japanese.txt` — 翻訳補完済みの日本語翻訳ファイル

### 翻訳ファイルの形式
Turing Complete の翻訳ファイルはハッシュベースの文字列プールシステム:
```
=== <section_identifier> ===

$<numeric_hash>* <translated_text>
```
- 各翻訳文字列にユニークな数値ハッシュID（例: `$59663530139004*`）
- セクション区切りは `=== section/name ===`
- 複数行テキスト対応
- リッチテキストマークアップ対応（`[center]`, `[color=#e49f44]`, `[url=...]`）

参照ファイル（ゲームの `translations/` ディレクトリ内）:
- `English.txt` — 英語原文（2,161エントリ、990セクション）
- `_ids_and_english.txt` — ハッシュID + 数値ID + 英語原文のマスターリファレンス

## Development Notes

- CustomAction は net472 必須（WiX DTF の制約）。`Microsoft.NETFramework.ReferenceAssemblies` NuGet パッケージで .NET Framework SDK 不要でビルド可能
- `CustomAction.config` で .NET 4.0 ランタイムを指定
- ゲームディレクトリ例: `C:\Program Files (x86)\Steam\steamapps\common\Turing Complete`
- ゲーム実行ファイル: `Turing Complete.exe`
- 翻訳ファイルパス: `translations/Japanese.txt`
