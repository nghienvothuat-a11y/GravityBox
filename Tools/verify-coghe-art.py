#!/usr/bin/env python3
"""Compare campaign physics with a Git baseline after a presentation-only rebuild."""
import argparse
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CAMPAIGN = "Assets/_Game/Venom/Campaign"


def previous(ref, path):
    return subprocess.check_output(["git", "show", f"{ref}:{path}"], cwd=ROOT, text=True)


def blocks(text, kinds):
    return {ident: body for kind, ident, body in re.findall(
        r"--- !u!(\d+) &(\d+)\n(.*?)(?=\n--- !u!|\Z)", text, re.S) if kind in kinds}


def pose(body):
    return "\n".join(line for line in body.splitlines() if line.startswith((
        "  m_LocalRotation:", "  m_LocalPosition:", "  m_LocalScale:", "  m_Father:")))


def verify(ref, levels=range(1, 11)):
    guid = re.search(r"guid: (\w+)", (ROOT / "Assets/_Game/Venom/Runtime/VenomSurfacePatch.cs.meta").read_text())[1]
    report = {"baseline": ref, "levels": []}
    for n in levels:
        path = f"{CAMPAIGN}/VenomOrigin{n:02}.unity"
        before, after = previous(ref, path), (ROOT / path).read_text()
        physics_types = {"54", "59", "64", "65", "135", "136", "153"}
        old, new = blocks(before, physics_types), blocks(after, physics_types)
        assert old == new, f"Level {n}: collider, Rigidbody or joint data changed"
        a, b = blocks(before, {"4"}), blocks(after, {"4"})
        for component in old.values():
            go = re.search(r"m_GameObject: \{fileID: (\d+)\}", component)[1]
            for ident, transform in a.items():
                if f"m_GameObject: {{fileID: {go}}}" in transform:
                    assert pose(transform) == pose(b[ident]), f"Level {n}: physics pose changed"
        def patches(text):
            # Unity moved this field in serialization order; older flat scenes
            # also omit its zero default. Keep nonzero sphere radii exact.
            def canonical(body):
                lines = body.splitlines()
                radius = next((line for line in lines if line.startswith("  SphereRadius:")), "  SphereRadius: 0")
                return "\n".join([line for line in lines if not line.startswith("  SphereRadius:")] + [radius])
            return {ident: canonical(body) for ident, body in blocks(text, {"114"}).items() if f"guid: {guid}" in body}
        assert patches(before) == patches(after), f"Level {n}: navigation/grip surfaces changed"
        definition = f"{CAMPAIGN}/Definitions/Level{n:02}.asset"
        assert previous(ref, definition) == (ROOT / definition).read_text(), f"Level {n}: definition changed"
        report["levels"].append({"level": n, "physics_components_unchanged": len(old),
                                 "surface_patches_unchanged": len(patches(before)),
                                 "poses_parents_and_definition_unchanged": True})
    for asset in (ROOT / CAMPAIGN).glob("*.physicMaterial"):
        assert previous(ref, str(asset.relative_to(ROOT))) == asset.read_text(), f"Changed contact material: {asset}"
    def matter_settings(text):
        return "\n".join(line for line in text.splitlines() if line.startswith("  ") and not line.startswith(("  Skin:", "  m_Name:")))
    assert matter_settings((ROOT / CAMPAIGN / "Matter.asset").read_text()) == matter_settings(
        (ROOT / "Assets/_Game/Venom/Art/DayLab/Day Lab matter.asset").read_text()), "Matter parameters changed"
    report["result"] = "Passed"
    report["contact_materials_and_matter_parameters_unchanged"] = True
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--baseline", default="8718849")
    parser.add_argument("--levels", nargs="+", type=int, choices=range(1, 11), default=list(range(1, 11)),
                        help="Compare specified levels when other levels have intentional gameplay changes")
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    result = json.dumps(verify(args.baseline, args.levels), ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(result)
    print(result)
