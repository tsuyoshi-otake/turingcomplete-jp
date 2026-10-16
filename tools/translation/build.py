"""Assemble mod Japanese.txt for the new game version from reuse map + translated batches."""
import sys,os,json,glob,re;sys.path.insert(0,os.path.dirname(os.path.abspath(__file__)))
from parse import parse
W=sys.argv[1]; OUT=sys.argv[2]
G=os.environ.get('TC_GAME_DIR','C:/Program Files (x86)/Steam/steamapps/common/Turing Complete')+'/translations/'
ide=parse(G+'_ids_and_english.txt',True)
tr=json.load(open(W+'/reuse_by_hash.json',encoding='utf-8'))
for f in sorted(glob.glob(W+'/out/batch_*.json')): tr.update(json.load(open(f,encoding='utf-8')))
jp=lambda t: bool(re.search(r'[\u3040-\u30ff\u4e00-\u9fff]',t))
lines=[];sec=None;miss=0;ja=0;n=0
for e in ide:
    if e['hash']=='0': continue
    n+=1
    if e['sec']!=sec:
        sec=e['sec']; lines+=['','',f'=== {sec} ===','']
    t=tr.get(e['hash'])
    if t is None: miss+=1; t=e['text']
    ja+=jp(t)
    if e['comments']: lines+=['']+e['comments']
    lines.append(f"${e['hash']}*\n{t}" if e['own_line'] or '\n' in t else f"${e['hash']}* {t}")
open(OUT,'w',encoding='utf-8',newline='\n').write('\n'.join(lines).lstrip('\n')+'\n')
print(f'entries {n}, translated(have JP) {ja} ({ja/n:.1%}), no translation {miss}')
