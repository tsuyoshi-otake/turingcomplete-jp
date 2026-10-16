# 翻訳更新パイプライン

ゲーム本体のアップデートで文字列ハッシュが変わったときに `mod/translations/Japanese.txt` を作り直すためのスクリプト群。
2026-10（Steam ビルド 25294874, ゲーム 2.1.334）の更新で使用した。

新版ではハッシュが文脈（ソースファイル）依存になり、旧MODのハッシュとはほぼ一致しない（2,027件中184件のみ一致）。
そのため「ハッシュ一致は旧訳を流用、それ以外は旧MODを用語集として再翻訳」という手順を取る。

## 手順

1. `python -I prep.py <work>` — ゲームの `_ids_and_english.txt` / `Japanese.txt` と現在の MOD を読み、
   `<work>/in/batch_NN.json`（要翻訳）、`reuse_by_hash.json`（流用）、`old_mod_tm.tsv`（用語集）、`validate.py`、`INSTRUCTIONS.md` を作る。
2. 各バッチを翻訳者（エージェント）に渡す。指示は `INSTRUCTIONS.md`。出力は `<work>/out/batch_NN.json`。
3. `python -I <work>/validate.py NN` — マークアップタグ・プレースホルダの一致を検査（全バッチ problems 0 にする）。
4. 必要なら `normalize.py`（work 内で実行）で表記ゆれを統一。表は今回の更新用なので見直してから使う。
5. `python -I build.py <work> ../../mod/translations/Japanese.txt` で組み立て。
6. `python -I roundtrip.py <work>` — 組み立て結果を読み戻し、全ハッシュ・セクションが一致することを確認。

ゲームディレクトリが既定と違う場合は環境変数 `TC_GAME_DIR` を設定する。
