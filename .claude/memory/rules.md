# rules.md — distilled, verified learnings for turingcomplete-jp

- 翻訳ハッシュはゲーム版によって変わる。2.1.334（build 25294874）以降はソースファイル単位の文脈依存で、同じ英文でもセクションが違えば別ハッシュになる。旧訳はハッシュではなく英文と文脈で照合する。（#1）
- 旧訳の再利用と再翻訳は `tools/translation/` のパイプライン（prep → 翻訳 → validate → build → roundtrip）で行う。ゲーム同梱の公式 Japanese.txt は、MOD導入中は `.jp-mod-backup/Japanese.txt` にある。prep.py はそちらを優先して読む。（#1）
- ゲームの翻訳ファイル: 複数行のエントリは `$hash*` を単独行に書き、本文は次の行から始める。`# ` で始まる行が `$` 行の直前にあれば、それは開発者のコメント（文脈）。（#1）
- インストーラーのCAログは `%TEMP%\turingcomplete-jp-ca.log`。2026-10-16 時点で UpdateLanguageSetting が `SQLite.Interop.dll` を読み込めずに失敗する（言語の自動設定は効いていない）。（#1）
- MajorUpgrade では旧版の UninstallMod がバックアップを戻し、その後に新版の InstallMod がバックアップを取り直す。そのため、一度MODが入ったファイルがバックアップされると「公式ファイル」が失われる。（#1）
- 起動直後のゲームウィンドウが最小化状態（rect -32000）のときに ShowWindow で強制的に復元すると、imgui の `scale > 0.0f` アサーションが出る（翻訳とは無関係。「無視」で続行できる）。（#1）
