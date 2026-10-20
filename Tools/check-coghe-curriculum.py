#!/usr/bin/env python3
"""Static authoring audit only; real solves/wander tests remain mandatory."""
import json
import re
from pathlib import Path

root = Path(__file__).resolve().parents[1]
data = json.loads((root / "Docs/LevelDesign/COghe/Difficulty65/curriculum.json").read_text())
source = (root / "Assets/_Game/Editor/COgheSpatialCampaignBuilder.cs").read_text()
# Prototype levels appended at the end (a "// Prototypes" comment) are not in the curriculum until placed.
order = re.findall(r'"([A-Z0-9]+)"', source.split("SpatialOrder={", 1)[1].split("};", 1)[0].split("// Prototypes", 1)[0])
assert len(order) == len(set(order)) == 68  # 65 + the physics levels 66–68 (Mrk 20/10/2026)
assert order == [row["key"] for row in data["levels"]], "Dossier and builder disagree"
assert [row["position"] for row in data["levels"]] == list(range(1, 69))
assert [order[n-1] for n in data["bossPositions"]] == ["10", "B1", "G5", "20", "N40", "N50"]
assert order[:7] == ["01", "02", "03", "04", "05", "K01", "06"], "Glass-rule opening contract"
for chain in data["chains"]:
    intro = order.index(chain["intro"])
    practice = [order.index(k) for k in chain["practice"]]
    assert practice and all(p > intro for p in practice), chain["skill"]
    for key in chain["combine"]:
        assert any(p < order.index(key) for p in practice), (chain["skill"], key, "missing prior practice")
print(f"{len(order)} unique IDs, 6 fixed bosses, {len(data['chains'])} learning chains validated (static only).")
