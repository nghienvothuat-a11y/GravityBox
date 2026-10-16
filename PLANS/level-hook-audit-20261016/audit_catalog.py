#!/usr/bin/env python3
"""Read-only game audit; writes this report's CSV/JSON/Markdown, never scenes or gameplay.
Serialized component presence is not reachability, perceived difficulty, or retention.
"""
from pathlib import Path
import collections,csv,hashlib,json,re,subprocess
ROOT=Path(__file__).resolve().parents[2]
OUT=Path(__file__).resolve().parent
source=ROOT/'Assets/_Game/Editor/COgheSpatialCampaignBuilder.cs'
order=re.findall(r'"([A-Z0-9]+)"',source.read_text().split('SpatialOrder={',1)[1].split('};',1)[0])
assert len(order)==len(set(order))==65
annotations=list(csv.DictReader((OUT/'annotations.tsv').open(),delimiter='\t'))
assert [int(r['position']) for r in annotations]==list(range(1,66))
script={}
for p in (ROOT/'Assets/_Game/Venom/Runtime').rglob('*.cs.meta'):
    m=re.search(r'^guid: (\w+)',p.read_text(),re.M)
    if m:script[m[1]]=p.name[:-8]
metric_source='Docs/Verification/COgheDifficulty65/levels.jsonl'
old={r['key']:r for r in map(json.loads,(ROOT/metric_source).read_text().splitlines())}
rows=[]
for pos,(key,note) in enumerate(zip(order,annotations),1):
    scene=ROOT/'Assets/_Game/Venom/SpatialCampaign'/('COgheSpatial'+('' if key.isdigit() else 'Plus')+key+'.unity')
    content=scene.read_text()
    counts=collections.Counter(script.get(g,'unknown') for g in re.findall(r'm_Script: \{fileID: 11500000, guid: (\w+)',content))
    max_windows=[]
    for block in re.split(r'(?m)^--- !u!',content):
        m=re.search(r'm_Script: \{fileID: 11500000, guid: (\w+)',block)
        if m and script.get(m[1])=='COgheTissueSensor':
            v=re.search(r'(?m)^  MaxLoad: ([\d.eE+-]+)',block)
            if v and float(v[1])>0:max_windows.append(float(v[1]))
    metric=old[key]
    # K01 was corrected after the all-level replay: use its newer native proof separately, not mixed timing.
    evidence=metric_source
    taps=metric['taps']; seconds=metric['seconds']; timing='historical author replay, simulated seconds'
    if key=='K01':
        evidence='Docs/Verification/COgheCrateReveal6/native-summary.json'
        m=json.loads((ROOT/evidence).read_text())['levels'][0];taps=m['taps'];seconds=m['seconds'];timing='15 Oct corrected layout, native Mac seconds'
    rows.append(dict(note,position=pos,key=key,title=('06 · Lỗ dưới thùng' if key=='K01' else metric['title']),
        scene=str(scene.relative_to(ROOT)),scene_sha256=hashlib.sha256(scene.read_bytes()).hexdigest(),
        serialized_components={k:v for k,v in sorted(counts.items()) if k.startswith('COghe')},
        scale_windows=max_windows,reference_taps=taps,reference_seconds=seconds,
        reference_parts=metric['maxParts'],reference_source=evidence,timing=timing))
features=['COgheQuantumSplitter','COgheTubeNetwork','COghePassengerLift','COgheGearTrain','COgheSwingTransfer','COgheBalanceLift','COgheSeesawBridge','COgheTapRail']
summary={f:{'first':next((r['position'] for r in rows if r['serialized_components'].get(f)),None),
             'levels':[r['position'] for r in rows if r['serialized_components'].get(f)]} for f in features}
summary['upper_limit_scale_levels']=[r['position'] for r in rows if r['scale_windows']]
summary['crate_positions']=[r['position'] for r in rows if r['key'].startswith('K')]
data={'date':'2026-10-16','head':subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip(),
 'scope':'Current local working tree, including uncommitted work. Saved scenes + authoring + existing proof; no new gameplay run and no live cohort data.',
 'method':'Counts include serialized components, including inactive ones. Stage/priority/insight/risk/action are qualitative author judgments. Replay taps include deliberate failed experiments/retries; not decision counts. Timing sources are not comparable.',
 'summary':summary,'levels':rows}
(OUT/'audit.json').write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n')
fields=['position','key','title','stage','priority','insight','risk','action','reference_taps','reference_seconds','timing','reference_source','scene','scene_sha256']
with (OUT/'levels.csv').open('w',newline='',encoding='utf-8-sig') as f:
    w=csv.DictWriter(f,fieldnames=fields,extrasaction='ignore');w.writeheader();w.writerows(rows)
lines=['# Rà soát từng màn COghe ngày 16 tháng 10 năm 2026','',
'Đọc cùng [báo cáo](../COGHE_GAMEPLAY_HOOK_AUDIT_20261016.md). Số màn là vị trí hiện tại; content ID giữ nguyên. Đây là nhận định thiết kế từ source/scene, không phải điểm khó đã đo trên người chơi.','',
'| Màn | Content | Vai trò đề xuất để đánh giá | Phát hiện cần tạo | Rủi ro | Hành động |','|---:|---|---|---|---|---|']
for r in rows:lines.append('| '+ ' | '.join(str(r[k]).replace('|','/') for k in ['position','key','stage','insight','risk','action'])+' |')
(OUT/'LEVELS_01_65.md').write_text('\n'.join(lines)+'\n')
print(json.dumps(summary,ensure_ascii=False,indent=2))
