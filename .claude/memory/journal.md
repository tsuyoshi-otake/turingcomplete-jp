# journal.md — append-only

## 2026-10-17 ゲーム 2.1.334 への追従と v1.1.0（#1）
- symptom: ゲーム更新（Steam build 25294874）後、v1.0.0 のMODを入れてもほぼ英語のまま。公式訳は 167/2,027（8%）。
- root cause: 翻訳ハッシュの採番が文脈依存に変わり、旧MODの 2,161 件中、新版と一致するのは 184 件だけ。セクション名もファイルパス形式に変わった。
- fix: ハッシュが一致する 184 件は流用。残りの 1,843 件は 11 バッチに分け、sonnet エージェントで再翻訳（旧MODを用語集として Grep 参照）。validate.py でタグとプレースホルダの一致を検査し、normalize.py で表記ゆれ 39 件を統一。build.py で組み立て。パイプラインは tools/translation/ に保存。
- verification: 全11バッチで problems 0。roundtrip で 2,027 件・セクションが完全一致。日本語を含む項目 1,960/2,027（96.7%、残りは略語・記号）。MSI 1.1.0 をビルドし、この PC にサイレントインストールしてゲーム内（メインメニュー、NANDゲートのレベル説明）で日本語表示を確認。現在のMODから prep→build し直すとバイト単位で同一。
- learning: rules.md 参照。インストーラーの SQLite.Interop.dll 読み込み失敗（言語の自動設定が無効）と、MajorUpgrade 時のバックアップ上書きは未修正（別Issueで扱う）。
