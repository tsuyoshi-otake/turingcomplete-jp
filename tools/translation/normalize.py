"""Normalize terminology across translated batches. Run inside the work dir. The table is specific to the 2026-10 (build 25294874) pass; adjust before reuse."""
"""Terminology normalization across batches (same English -> same Japanese)."""
import json,glob,re
files=sorted(glob.glob('out/*.json')); outs={f:json.load(open(f,encoding='utf-8')) for f in files}
src={}
for f in sorted(glob.glob('in/*.json')):
    for d in json.load(open(f,encoding='utf-8')): src[d['hash']]=d
oddtitle=next(o[h] for o in outs.values() for h in o if src[h]['en']=='Odd Number of Signals')
exact={'Save':'保存','Load':'読込','Value':'値','Invert':'反転','Input':'入力',
 'Carry in':'キャリー入力','Carry out':'キャリー出力',
 'Persistent Memory':'永続メモリ','Main Memory':'メインメモリ','Register File':'レジスタファイル',
 '[color=#E49F44][b]Save [/b][/color]':'[color=#E49F44][b]保存 [/b][/color]',
 '[color=#E49F44][b]Load [/b][/color]':'[color=#E49F44][b]読込 [/b][/color]',
 '[color=#E49F44][b]   Value   [/b][/color]':'[color=#E49F44][b]   値   [/b][/color]'}
sub=[('Main Memory','メインメモリ'),('Register File','レジスタファイル'),('Persistent Memory','永続メモリ'),('Odd Number of Signals',oddtitle)]
n=0
for f,o in outs.items():
    for h,t in o.items():
        e=src[h]['en']; new=t
        if e in exact: new=exact[e]
        m=re.fullmatch(r'(Input|Output) (\d+)',e)
        if m: new=('入力' if m.group(1)=='Input' else '出力')+' '+m.group(2)
        if e not in exact:
            for a,b in sub:
                if a in e and a in new: new=new.replace(a,b)
        if new!=t: o[h]=new; n+=1
    json.dump(o,open(f,'w',encoding='utf-8'),ensure_ascii=False,indent=1)
print('changed',n)
