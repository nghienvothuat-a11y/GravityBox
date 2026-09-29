#!/usr/bin/env python3
"""Check document integrity and authored mass traces, NOT Unity solvability."""
import collections, hashlib, itertools, json, pathlib, re
from html.parser import HTMLParser
ROOT=pathlib.Path(__file__).resolve().parent
levels=json.loads((ROOT/'levels.json').read_text())
errors=[]
def check(ok,msg):
    if not ok:errors.append(msg)
def split_ok(a,b):
    for i,x in enumerate(a):
        if sorted(a[:i]+a[i+1:]+[x/2,x/2])==sorted(b):return True
    return False
def merge_ok(a,b):
    if sorted(a)==sorted(b):return True
    if len(a)<=len(b):return False
    # Exhaustively permit only exact addition of actual parts, including multi-pair merges.
    for i,j in itertools.combinations(range(len(a)),2):
        c=[x for k,x in enumerate(a) if k not in (i,j)]+[a[i]+a[j]]
        if merge_ok(c,b):return True
    return False
check([x['n'] for x in levels]==list(range(11,31)),'Catalog must contain exactly11–30 in order')
check(len({x['id'] for x in levels})==20,'Duplicate stable ID')
check([x['n'] for x in levels if x['boss']]==[20,30],'Boss positions20/30')
image_checks=[]
for l in levels:
    n=l['n']; key=f'{n:02}'
    check(l['status']=='design-only',f'{n}: design status')
    check(l['rotation']=='camera-only',f'{n}: camera semantics')
    check(not l['boss'] or l['hints']=='None',f'{n}: boss hints')
    check(l['mass_trace'][0]==[100] and l['mass_trace'][-1]==[100],f'{n}: initial/final mass')
    check(max(map(len,l['mass_trace']))==l['fragments'],f'{n}: peak fragments')
    for state in l['mass_trace']:check(sum(state)==100 and min(state)>0,f'{n}: mass conservation {state}')
    for a,b in zip(l['mass_trace'],l['mass_trace'][1:]):check(split_ok(a,b) or merge_ok(a,b),f'{n}: illegal mass transition {a}->{b}')
    check(('quantum' in l['mechanics'])==(l['fragments']>1),f'{n}: Q/trace mismatch')
    check(l['fragments']==1 or l['release_before_merge'],f'{n}: missing release planning')
    check(bool(l['recovery'] and l['geometry'] and l['specific_test']),f'{n}: missing authored recovery/geometry/test')
    seen=set()
    for node in l['milestones']:
        check(set(node['requires'])<=seen,f'{n}: forward dependency/cycle in authored trace')
        check(node['id'] not in seen,f'{n}: duplicate milestone');seen.add(node['id'])
    dossier=ROOT/f'Level{key}'/'README.md'
    check(dossier.exists(),f'{n}: missing dossier')
    if dossier.exists():
        text=dossier.read_text();check(re.findall(r'^## (\d+)\.',text,re.M)==[str(x) for x in range(1,11)],f'{n}: template sections')
    for relative in [f'Illustrations/{key}.png',f'Routes/{key}.svg',f'Prompts/{key}.txt']:
        path=ROOT/relative;check(path.exists(),f'{n}: missing {relative}')
        if path.suffix=='.png' and path.exists():
            blob=path.read_bytes();check(blob.startswith(b'\x89PNG\r\n\x1a\n'),f'{n}: invalid PNG')
            image_checks.append({'level':n,'sha256':hashlib.sha256(blob).hexdigest(),'bytes':len(blob)})
# Static local link validation (no browser navigation required).
class Links(HTMLParser):
    def __init__(self):super().__init__();self.refs=[]
    def handle_starttag(self,tag,attrs):
        self.refs.extend(v for k,v in attrs if k in ('src','href') and v)
refs=[]
for p in [*ROOT.glob('*.md'),*ROOT.glob('Level*/README.md')]:
    for target in re.findall(r'\]\(([^)]+)\)',p.read_text()):refs.append((p,target))
p=ROOT/'review.html'; parser=Links();parser.feed(p.read_text());refs.extend((p,x) for x in parser.refs)
for p,t in refs:
    if t.startswith(('http:','https:','#','data:','mailto:')):continue
    t=t.split('#')[0].split('?')[0]
    check((p.parent/t).exists(),f'Broken link {p.relative_to(ROOT)} -> {t}')
report={'scope':'Document structure and exact authored mass transitions only; no Unity, player, physics or performance tests', 'levels':len(levels),'images':len(image_checks),'local_links_checked':len(refs),'errors':errors,'images_sha256':image_checks}
(ROOT/'design-audit.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps({k:v for k,v in report.items() if k!='images_sha256'},ensure_ascii=False,indent=2))
raise SystemExit(bool(errors))
