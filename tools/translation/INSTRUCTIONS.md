# Turing Complete 日本語翻訳タスク

ゲーム Turing Complete（論理回路→CPU→アセンブリを作るパズルゲーム）の新バージョン用に、英語文字列を日本語に翻訳する。
作業ディレクトリ: このファイルのあるディレクトリ（以下 W）。

## 入力
- `W/in/batch_NN.json` : 担当バッチ。各要素 `{hash, sec, note, en, official_ja?, official_status?}`
  - `sec` = ゲーム内のファイル/文脈（例 `campaign/xor_gate/hint_0.txt` は XOR ゲートのレベルのヒント、`model/board/prototype_list.nim` は部品名・部品説明）
  - `note` = 開発者のコメント（文脈）
  - `official_ja` = ゲーム同梱のコミュニティ訳（FINISHED=最新英文に対応 / OUTDATED=古い英文への訳）。参考にしてよいが、誤訳・不自然なら直す。
- `W/old_mod_tm.tsv` : **旧バージョン向けに我々が作った日本語MOD**（1行1エントリ: `旧セクション\t旧hash\t訳文`、改行は `\n`）。旧ハッシュは新版と一致しないが、同じ英文の訳が大量に入っている。**用語・言い回しの一貫性のため、必ず Grep で関連語を検索して既存訳を再利用すること**（例: レベル名 `xor`, 部品名 `Register`, UI語 `schematic` など。旧セクション名は `levels/<level>`, `components/<name>`, `trans/...`, `nim/...` など）。英文が同じなら旧訳をそのまま使う。全体を Read せず Grep で引くこと。

## 出力
`W/out/batch_NN.json` に `{ "<hash>": "<日本語訳>", ... }` の JSON オブジェクト（UTF-8, ensure_ascii 不要）を、入力の全 hash について書く。Python で書き出すと安全（`json.dump(..., ensure_ascii=False, indent=1)`）。

## 翻訳ルール
1. マークアップ `[color=#xxxxxx]...[/color]`, `[b]`, `[large]`, `[box]`, `[center]`, `[left]`, `[highlight]`, `[image=...]`, `[link=...]`, `[tip]`, `[sp]` などはタグ文字列をそのまま保持（属性値も変えない）。囲む中身は訳してよい。
2. プレースホルダ `{nr}`, `{reg_i}`, `{value}` や `%a` `%b` などは一字一句そのまま残す（個数も同じ）。
3. 改行構造（段落区切り・リスト）はおおむね保つ。先頭/末尾の改行も英文に合わせる。
4. コード片（アセンブリ `.asm`, ISA 定義 `.isa` の構文、`mov r1, 4` のような命令、真理値表の `[T] [F]`）、ニーモニック、レジスタ名は訳さない。`.asm`/`.isa` 系の値は説明文・コメント部分だけ訳し、コード部分は原文のまま。
5. ゲーム固有の固有名詞（Overture, Symphony, LEG などのアーキテクチャ名、キャラクター名）は旧MODの表記に合わせる。旧MODに無ければカタカナまたは原文のまま。
6. 文体: 旧MODに合わせる（説明文は「〜する。」「〜します」どちらかは旧MODの同種文を見て合わせる。UIラベルは体言止めの短い語）。
7. ハッシュ値 `$123*` のような行頭書式は出力に含めない（訳文のみ）。
8. どうしても訳すべきでない文字列（記号のみ・コードのみ・`{nr}` だけ等）は英文をそのまま入れる。

## 検証（必須）
書き終えたら `python -I W/validate.py NN` を実行し、problems が 0 になるまで直す。「left without Japanese」はコード/記号のみの項目なら可。

## 報告
最後に1〜3行で: エントリ数、problems 数、英語のまま残した hash とその理由（あれば）。訳文全体を報告に貼らないこと。
