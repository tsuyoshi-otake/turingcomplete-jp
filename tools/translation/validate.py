"""Usage: python -I validate.py <batch number, e.g. 03>   (checks out/batch_NN.json against in/batch_NN.json)"""
import sys,json,re,os,collections
W=os.path.dirname(os.path.abspath(__file__)); n=sys.argv[1]
src=json.load(open(f'{W}/in/batch_{n}.json',encoding='utf-8'))
try: out=json.load(open(f'{W}/out/batch_{n}.json',encoding='utf-8'))
except Exception as ex: print('CANNOT LOAD OUTPUT:',ex); sys.exit(1)
tag=lambda t: collections.Counter(re.findall(r'\[/?[a-z_]+(?:=[^\]]*)?\]',t))
ph=lambda t: collections.Counter(re.findall(r'\{[^{}\n]*\}|%[a-zA-Z0-9_]+',t))
jp=lambda t: bool(re.search(r'[\u3040-\u30ff\u4e00-\u9fff]',t))
errs=[];eng=0
for d in src:
    h=d['hash']
    if h not in out: errs.append(f'{h}: missing'); continue
    t=out[h]
    if not isinstance(t,str) or not t.strip(): errs.append(f'{h}: empty'); continue
    if tag(t)!=tag(d['en']): errs.append(f'{h}: markup tags differ {dict(tag(d["en"])-tag(t))} vs {dict(tag(t)-tag(d["en"]))}')
    if ph(t)!=ph(d['en']): errs.append(f'{h}: placeholders differ {dict(ph(d["en"])-ph(t))} vs {dict(ph(t)-ph(d["en"]))}')
    if not jp(t): eng+=1
extra=set(out)-{d['hash'] for d in src}
if extra: errs.append(f'unknown hashes: {sorted(extra)[:5]}')
print(f'{len(src)} entries, {len(errs)} problems, {eng} left without Japanese (OK only for code/symbols/proper nouns)')
for e in errs[:60]: print(' ',e)
sys.exit(1 if errs else 0)
