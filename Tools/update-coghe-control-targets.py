#!/usr/bin/env python3
"""Upgrade saved compact TapRail controls without regenerating geometry or puzzles.

TapRail() authors '<label> carriage' switches; NextCrate/ViewBridge have distinct
bodies and must keep their walk/handle semantics. Only active catalog scenes change.
The builder sets the same opt-in for future regeneration.
"""
from pathlib import Path
import json
import re

ROOT = Path(__file__).resolve().parents[1]
VENOM = ROOT / 'Assets/_Game/Venom'
guid = re.search(r'guid: (\w+)', (VENOM / 'Runtime/COgheTapRail.cs.meta').read_text())[1]
definition = (VENOM / 'SpatialCampaign/Definitions/Spatial01.asset').read_text()
names = re.findall(r'^  - (COgheSpatial\w+)\s*$', definition, re.M)
assert len(names) == len(set(names)) == 65
report = []
for name in names:
    path = VENOM / 'SpatialCampaign' / (name + '.unity')
    before = path.read_text()
    blocks = {m[2]: m[3] for m in re.finditer(r'^--- !u!(\d+) &(\d+)\n(.*?)(?=^--- !u!|\Z)', before, re.M | re.S)}
    after = before
    for fid, block in blocks.items():
        if 'guid: ' + guid not in block:
            continue
        rail = re.search(r'^  Rail: \{fileID: (\d+)\}', block, re.M)[1]
        go = re.search(r'  m_GameObject: \{fileID: (\d+)\}', blocks[rail])[1]
        label = re.search(r'^  m_Name: (.*)', blocks[go], re.M)[1]
        if not label.endswith(' carriage') or '  CrateFaces: 1' in block:
            continue
        if '  TouchControlBody:' in block:
            replacement = re.sub(r'^  TouchControlBody: \d', '  TouchControlBody: 1', block, flags=re.M)
        else:
            replacement = re.sub(r'(^  PickHandleOnly: \d\n)', r'\1  TouchControlBody: 1\n', block, flags=re.M)
        assert replacement != block or '  TouchControlBody: 1' in block
        after = after.replace('--- !u!114 &' + fid + '\n' + block, '--- !u!114 &' + fid + '\n' + replacement, 1)
        report.append({'scene': name, 'control': label})
    # No scene field other than the input opt-in may change.
    normalize = lambda s: re.sub(r'^  TouchControlBody: \d\n', '', s, flags=re.M)
    assert normalize(before) == normalize(after)
    if before != after:
        path.write_text(after)
out = ROOT / 'Artifacts/FirstPlayControls'
out.mkdir(parents=True, exist_ok=True)
(out / 'control-targets.json').write_text(json.dumps(report, indent=2) + '\n')
print(f'{len(report)} compact controls in {len(set(r["scene"] for r in report))} active scenes; only input opt-ins changed.')
