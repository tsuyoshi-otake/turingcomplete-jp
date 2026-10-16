import sys,json,re,os;sys.path.insert(0,os.path.dirname(os.path.abspath(__file__)))
from parse import parse
W=sys.argv[1]; os.makedirs(W+'/in',exist_ok=True); os.makedirs(W+'/out',exist_ok=True)
G=os.environ.get('TC_GAME_DIR','C:/Program Files (x86)/Steam/steamapps/common/Turing Complete')+'/translations/'
# official (unmodded) Japanese.txt: the installer keeps it in .jp-mod-backup while the MOD is installed
_bk=os.path.join(G,'..','.jp-mod-backup','Japanese.txt')
ja=parse(_bk if os.path.exists(_bk) else G+'Japanese.txt'); ide=parse(G+'_ids_and_english.txt',True); mod=parse(os.path.join(os.path.dirname(os.path.abspath(__file__)),'..','..','mod','translations','Japanese.txt'))
jp=lambda t: bool(re.search(r'[\u3040-\u30ff\u4e00-\u9fff]',t))
jah={e['hash']:e for e in ja}; modh={e['hash']:e for e in mod}
# old-mod TM, grep-friendly
with open(W+'/old_mod_tm.tsv','w',encoding='utf-8') as f:
    for e in mod: f.write(f"{e['sec']}\t{e['hash']}\t{e['text'].replace(chr(10),'\n')}\n")
entries=[];reuse={}
for e in ide:
    if e['hash']=='0': continue
    h=e['hash']; g=jah.get(h)
    d=dict(hash=h,sec=e['sec'],note=' / '.join(c[1:].strip() for c in e['comments']) or None,en=e['text'])
    if g and g['block'] and not g['block'].startswith('MISSING'): d['official_ja']=g['text']; d['official_status']=g['block'].split()[0]
    m=modh.get(h)
    if m and (jp(m['text']) or m['text']!=e['text']): reuse[h]=m['text']; continue
    entries.append(d)
json.dump(reuse,open(W+'/reuse_by_hash.json','w',encoding='utf-8'),ensure_ascii=False,indent=0)
# batch by section order, ~16KB english each
entries.sort(key=lambda d:d["sec"])
batches=[];cur=[];size=0;prev=None
for d in entries:
    if (size>16000 or len(cur)>=220) and d['sec']!=prev: batches.append(cur);cur=[];size=0
    cur.append(d);size+=len(d['en'])+20;prev=d['sec']
batches.append(cur)
for i,b in enumerate(batches):
    json.dump(b,open(f'{W}/in/batch_{i:02d}.json','w',encoding='utf-8'),ensure_ascii=False,indent=1)
    print(i,len(b),sum(len(d['en']) for d in b),b[0]['sec'],'..',b[-1]['sec'])
import shutil
for f in ('validate.py','INSTRUCTIONS.md'): shutil.copy(os.path.join(os.path.dirname(os.path.abspath(__file__)),f),W)
print('reuse',len(reuse),'to translate',len(entries))
