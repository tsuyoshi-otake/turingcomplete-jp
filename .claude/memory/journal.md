# journal.md — append-only

## 2026-10-17 ゲーム 2.1.334 への追従と v1.1.0（#1）
- symptom: ゲーム更新（Steam build 25294874）後、v1.0.0 のMODを入れてもほぼ英語のまま。公式訳は 167/2,027（8%）。
- root cause: 翻訳ハッシュの採番が文脈依存に変わり、旧MODの 2,161 件中、新版と一致するのは 184 件だけ。セクション名もファイルパス形式に変わった。
- fix: ハッシュが一致する 184 件は流用。残りの 1,843 件は 11 バッチに分け、sonnet エージェントで再翻訳（旧MODを用語集として Grep 参照）。validate.py でタグとプレースホルダの一致を検査し、normalize.py で表記ゆれ 39 件を統一。build.py で組み立て。パイプラインは tools/translation/ に保存。
- verification: 全11バッチで problems 0。roundtrip で 2,027 件・セクションが完全一致。日本語を含む項目 1,960/2,027（96.7%、残りは略語・記号）。MSI 1.1.0 をビルドし、この PC にサイレントインストールしてゲーム内（メインメニュー、NANDゲートのレベル説明）で日本語表示を確認。現在のMODから prep→build し直すとバイト単位で同一。
- learning: rules.md 参照。インストーラーの SQLite.Interop.dll 読み込み失敗（言語の自動設定が無効）と、MajorUpgrade 時のバックアップ上書きは未修正（別Issueで扱う）。

## 2026-10-17 v1.1.0 リリース（#1, #2, #3）
- symptom: マージ後の main から MSI をビルドしたところ、`core.autocrlf` の影響で Japanese.txt が CRLF でチェックアウトされ、実機確認した LF 版とバイト内容がずれた。
- root cause: リポジトリに `.gitattributes` がなかった。CA の DLL は checkout 済みファイルをそのまま埋め込む。
- fix: `.gitattributes` で `mod/**` と `tools/translation/**` を `eol=lf` に固定（384c5d7, #3）。再翻訳本体は 8d30092（#2）。
- verification: 再ビルドした CustomAction.dll に、実機確認したファイルとバイト単位で同一の内容が含まれている。v1.1.0 タグを付けてリリースし、`releases/latest/download/turingcomplete-jp.msi` の sha256（554af90f…e1e2）がビルド成果物と一致。Issue #1 は #2 のマージでクローズ。
- learning: リリース用 MSI は「マージ後の main をクリーンにチェックアウトした状態」からビルドし、埋め込まれたファイルが検証済みのものと同一か確認してからアップロードする。

## 2026-10-17 インストーラーの言語自動設定とバックアップ保護（#5, 5e28642）
- symptom: インストールしてもゲームの言語が Japanese に切り替わらない。CA ログに `SQLite.Interop.dll を読み込めません (0x8007007E)` が出る。MajorUpgrade や修復のあと、`.jp-mod-backup/Japanese.txt` が MOD 入りのファイルで上書きされることがあり、アンインストールしても公式ファイルに戻らない。
- root cause: System.Data.SQLite のネイティブ DLL が DTF の CustomAction.CA.dll に同梱されない。InstallMod が、現在のファイルの中身を確認せずに毎回バックアップしていた。
- fix: Windows 標準の winsqlite3.dll を P/Invoke で呼ぶ GameLanguageSetting を追加（UPDATE し、0 行なら INSERT）。InstallMod に DecideBackup を追加し、上書きは「公式の見出しがある」または「バックアップがない」場合だけにした。
- verification: progress.dat のコピーに対して Write と DecideBackup を 64bit と 32bit の PowerShell から実行して確認。実機で msiexec /qn を 4 回実行（1.1.0 からのアップグレード → 修復 → アンインストール → 再インストール）。CA ログに `Set to 'Japanese' (1 rows)`、`KeepCurrentIsThisMod`、`Set to '' (1 rows)` が出て、アンインストールで公式ファイル（217,279 バイト）に戻った。最終状態は Japanese で、バックアップは元の公式ファイルとバイト単位で同一。
- learning: Git Bash から `msiexec //i ... //l*v` を実行すると rc=103 で何も起きない。msiexec は PowerShell の Start-Process から実行する。
