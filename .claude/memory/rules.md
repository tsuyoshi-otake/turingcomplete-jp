# rules.md — distilled, verified learnings for turingcomplete-jp

- 翻訳ハッシュはゲーム版によって変わる。2.1.334（build 25294874）以降はソースファイル単位の文脈依存で、同じ英文でもセクションが違えば別ハッシュになる。旧訳はハッシュではなく英文と文脈で照合する。（#1）
- 旧訳の再利用と再翻訳は `tools/translation/` のパイプライン（prep → 翻訳 → validate → build → roundtrip）で行う。ゲーム同梱の公式 Japanese.txt は、MOD導入中は `.jp-mod-backup/Japanese.txt` にある。prep.py はそちらを優先して読む。（#1）
- ゲームの翻訳ファイル: 複数行のエントリは `$hash*` を単独行に書き、本文は次の行から始める。`# ` で始まる行が `$` 行の直前にあれば、それは開発者のコメント（文脈）。（#1）
- インストーラーのCAログは `%TEMP%	uringcomplete-jp-ca.log`。言語設定は progress.dat（SQLite、`progress(key,value)` で主キーなし）の `setting_language`。CA から書き込むときは Windows 標準の `winsqlite3.dll` を P/Invoke で呼ぶ（x86 は StdCall）。System.Data.SQLite は、ネイティブの SQLite.Interop.dll が DTF の CA パッケージに入らず 0x8007007E で失敗する。（#5）
- MajorUpgrade では旧版の UninstallMod がバックアップを戻し、その後に新版の InstallMod がバックアップを取り直す。InstallMod はバックアップを上書きしてよいか `DecideBackup` で判定する。条件は (a) 公式ファイル（`===== MISSING/OUTDATED/FINISHED TRANSLATION =====` の見出しがある）、または (b) バックアップがまだない、のどちらか。現在のファイルがこの MOD と同一の場合や、正体不明でバックアップが既にある場合は上書きしない。（#5）
- 起動直後のゲームウィンドウが最小化状態（rect -32000）のときに ShowWindow で強制的に復元すると、imgui の `scale > 0.0f` アサーションが出る（翻訳とは無関係。「無視」で続行できる）。（#1）
