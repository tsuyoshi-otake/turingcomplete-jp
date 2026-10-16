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
