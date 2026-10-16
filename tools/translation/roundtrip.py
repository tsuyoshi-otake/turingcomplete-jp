import sys,os,json,glob;sys.path.insert(0,os.path.dirname(os.path.abspath(__file__)))
from parse import parse
W=sys.argv[1]
G=os.environ.get('TC_GAME_DIR','C:/Program Files (x86)/Steam/steamapps/common/Turing Complete')+'/translations/'
ide={e['hash']:e for e in parse(G+'_ids_and_english.txt',True) if e['hash']!='0'}
tr=json.load(open(W+'/reuse_by_hash.json',encoding='utf-8'))
for f in sorted(glob.glob(W+'/out/batch_*.json')): tr.update(json.load(open(f,encoding='utf-8')))
got={e['hash']:e for e in parse(os.path.join(os.path.dirname(os.path.abspath(__file__)),'..','..','mod','translations','Japanese.txt'))}
bad=[h for h in ide if h not in got or got[h]['text']!=tr.get(h,ide[h]['text'])]
print('expected',len(ide),'parsed',len(got),'mismatch',len(bad),bad[:5])
print('sections match',all(got[h]['sec']==ide[h]['sec'] for h in ide))
