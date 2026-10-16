import re
def parse(path, ids_format=False):
    """Parse a Turing Complete translation file -> list of dict(sec,hash,text,block,comments)."""
    lines=open(path,encoding='utf-8-sig').read().replace('\r\n','\n').split('\n')
    n=len(lines)
    def is_entry(i): return i<n and re.match(r'^\$\d+\*',lines[i])
    def is_comment(i):
        # comment line: '# ' line followed (through other comment lines) by an entry line
        j=i
        while j<n and lines[j].startswith('#'): j+=1
        return lines[i].startswith('#') and is_entry(j)
    out=[];sec=None;block=None;cur=None;pend=[]
    for i,line in enumerate(lines):
        m=re.match(r'^=+ ([A-Z ]+?) =+$',line)
        if m and not line.startswith('=== '):
            cur=None;block=m.group(1).strip();continue
        if re.fullmatch(r'=+',line): cur=None;continue
        m=re.match(r'^=== (.*) ===$',line)
        if m: cur=None;sec=m.group(1);continue
        if is_comment(i): pend.append(line);continue
        m=re.match(r'^\$(\d+)\*(?: (.*))?$',line)
        if m:
            t=m.group(2)
            cur=dict(own_line=t is None,sec=sec,hash=m.group(1),lines=[t] if t is not None else [],block=block,comments=pend);pend=[]
            out.append(cur);continue
        if cur is not None: cur['lines'].append(line)
    for e in out:
        L=e.pop('lines')
        while L and L[-1]=='': L.pop()
        t='\n'.join(L)
        if ids_format: t=re.sub(r'^\d+ ','',t,count=1)
        e['text']=t
    return out
