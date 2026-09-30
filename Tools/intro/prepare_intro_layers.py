#!/usr/bin/env python3
"""COghe intro: turn the painted layers into runtime textures.

The painted layers (Codex, OUTBOX/COGHE_INTRO_ASSETS_2026_09_30/LAYERS) come at the image model's native size, in the
aspect of the canvas each one was briefed on (LAYER_MANIFEST.md). This script:
  - fits each layer to its manifest canvas (backgrounds 1080x2340; characters bottom-anchored on their canvas);
  - cleans the alpha (the model leaves the body at 252-253 and speckles of 1) and trims transparent margins;
  - registers the lab plate S7_BG_LAB_MATCH onto the real level-1 frame (REFERENCES/level01_start_1080x2340.png): an
    ECC homography on the edge maps, so the drawn glass box sits on the rendered one before the ink dissolve, and pulls
    its colours toward the render so the dissolve does not shift hue;
  - finds where COghe is in the layers that hold it (the placing hands, the close-up, the seated sprite) so the
    animation anchors on the creature instead of on canvas coordinates;
  - writes textures sized for the budget (opaque backgrounds as JPG, layers as PNG) and layout.txt (rect of each trimmed texture in its canvas, plus the COghe anchor).

Usage: python3 Tools/intro/prepare_intro_layers.py <layers_dir> <level01_1080x2340.png> [out_dir] [--sheet sheet.png]
Default out_dir: Assets/_Game/Venom/Resources/COgheIntro        (needs numpy, pillow, opencv-python-headless)
"""
import os, sys
import numpy as np
import cv2
from PIL import Image

ARGS = [a for a in sys.argv[1:] if not a.startswith("--")]
SHEET = sys.argv[sys.argv.index("--sheet") + 1] if "--sheet" in sys.argv else None
if SHEET in ARGS: ARGS.remove(SHEET)
if len(ARGS) < 2: sys.exit(__doc__)
LAYERS, REFERENCE = ARGS[0], ARGS[1]
OUT = ARGS[2] if len(ARGS) > 2 else os.path.join(os.path.dirname(__file__), "../../Assets/_Game/Venom/Resources/COgheIntro")

PANEL = (1080, 2340)
BG_MAX = 1536          # backgrounds: longest side (ASTC 8x8 on device)
# name: (canvas w, canvas h, output pixels per canvas pixel). Backgrounds are the full panel.
SPEC = {
    "S1_BG_SKY": (*PANEL, None), "S2_BG_CRATER": (*PANEL, None), "S3_BG_RIM": (*PANEL, None),
    "S4_BG_FLOOR": (*PANEL, None), "S6_BG_SITE": (*PANEL, None), "S6_CLOSEUP": (*PANEL, None),
    "S7_BG_LAB_MATCH": (*PANEL, None),
    "S1_FX_METEOR": (1024, 1024, .5), "S2_FX_BLAST": (1024, 1024, .5),
    "S3_SOLDIERS": (1080, 900, .8), "S3_SCIENTIST": (800, 1600, .9),
    "S4_SPHERE_CLOSED": (900, 900, .7), "S4_SPHERE_OPEN": (900, 900, .7), "S5_COGHE_RISE": (900, 900, .7),
    "S5_SOLDIERS_REACT": (1080, 900, .8), "S6_SCIENTIST_KNEEL": (1000, 1800, .8),
    "S7_HANDS_PLACE": (1080, 1300, .8), "S7_COGHE_SEATED": (400, 400, .6),
}
# layers holding COghe: the anchor is the creature's body (tendrils removed), not the canvas
COGHE_IN = {"S7_HANDS_PLACE", "S6_CLOSEUP", "S7_COGHE_SEATED", "S5_COGHE_RISE"}
# where to look for COghe when the layer has other dark masses (x0 y0 x1 y1, 0..1): the close-up's palm
SEARCH = {"S6_CLOSEUP": (.35, .35, .95, .65)}


def load(name):
    for ext in (".png", ".PNG"):
        p = os.path.join(LAYERS, name + ext)
        if os.path.exists(p): return Image.open(p)
    return None


def clean_alpha(rgba):
    a = rgba[..., 3].astype(np.float32)
    a = np.clip((a - 3) / (248 - 3), 0, 1)          # body 252-253 -> opaque; model speckle (1-2) -> clear
    rgba[..., 3] = np.round(a * 255).astype(np.uint8)
    rgba[rgba[..., 3] == 0, :3] = 0
    return rgba


def fit_canvas(img, cw, ch):
    """Contain-fit on the canvas, bottom-centre anchored (characters stand on the canvas bottom)."""
    s = min(cw / img.width, ch / img.height)
    w, h = round(img.width * s), round(img.height * s)
    canvas = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    canvas.paste(img.resize((w, h), Image.LANCZOS), ((cw - w) // 2, ch - h))
    return canvas


def coghe_anchor(rgba, region=None):
    """Centre, width and bottom of the biggest glossy-black body (COghe), thin tendrils opened away."""
    rgb, a = rgba[..., :3].astype(np.int32), rgba[..., 3]
    dark = ((rgb.max(axis=2) < 48) & (a > 128)).astype(np.uint8)
    if region:
        h, w = dark.shape; keep = np.zeros_like(dark)
        keep[int(region[1] * h):int(region[3] * h), int(region[0] * w):int(region[2] * w)] = 1; dark &= keep
    k = max(5, int(min(rgba.shape[:2]) * .02)) | 1
    dark = cv2.morphologyEx(dark, cv2.MORPH_OPEN, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (k, k)))
    n, lab, stats, cent = cv2.connectedComponentsWithStats(dark)
    if n < 2: return None
    i = 1 + int(np.argmax(stats[1:, cv2.CC_STAT_AREA]))
    return float(cent[i][0]), float(cent[i][1]), float(stats[i, cv2.CC_STAT_WIDTH]), float(stats[i, cv2.CC_STAT_TOP] + stats[i, cv2.CC_STAT_HEIGHT])


def match_colour(rgb, ref_rgb, strength=.7):
    """Pull the plate's colours toward the rendered level (mean/spread in Lab), so the dissolve does not shift hue."""
    a = cv2.cvtColor(rgb, cv2.COLOR_RGB2LAB).astype(np.float32); b = cv2.cvtColor(ref_rgb, cv2.COLOR_RGB2LAB).astype(np.float32)
    ma, sa = a.reshape(-1, 3).mean(0), a.reshape(-1, 3).std(0) + 1e-3
    mb, sb = b.reshape(-1, 3).mean(0), b.reshape(-1, 3).std(0)
    moved = (a - ma) * (sb / sa) + mb
    out = a + (moved - a) * strength
    return cv2.cvtColor(np.clip(out, 0, 255).astype(np.uint8), cv2.COLOR_LAB2RGB)


def register_to_reference(plate_rgb, ref_rgb):
    """Homography that puts the drawn box on the rendered one (ECC on blurred edges, masked to the box area)."""
    def edges(img):
        g = cv2.cvtColor(img, cv2.COLOR_RGB2GRAY)
        e = cv2.Canny(cv2.GaussianBlur(g, (5, 5), 0), 20, 60).astype(np.float32)
        return cv2.GaussianBlur(e, (0, 0), 6)
    a, b = edges(ref_rgb), edges(plate_rgb)
    mask = np.zeros(a.shape, np.uint8); mask[600:1700, :] = 255      # the glass box and its base
    warp = np.eye(3, dtype=np.float32)
    crit = (cv2.TERM_CRITERIA_EPS | cv2.TERM_CRITERIA_COUNT, 400, 1e-7)
    for scale in (.25, .5, 1.0):                                  # coarse to fine
        sa = cv2.resize(a, None, fx=scale, fy=scale); sb = cv2.resize(b, None, fx=scale, fy=scale)
        sm = cv2.resize(mask, None, fx=scale, fy=scale, interpolation=cv2.INTER_NEAREST)
        S = np.diag([scale, scale, 1]).astype(np.float32)
        w = S @ warp @ np.linalg.inv(S)
        _, w = cv2.findTransformECC(sa, sb, w.astype(np.float32), cv2.MOTION_HOMOGRAPHY, crit, sm, 5)
        warp = np.linalg.inv(S) @ w @ S
    return warp


def main():
    os.makedirs(OUT, exist_ok=True)
    ref = np.array(Image.open(REFERENCE).convert("RGB"))
    lines, sheet = [], []
    for name, (cw, ch, scale) in SPEC.items():
        img = load(name)
        if img is None: print(f"  {name}: not painted yet"); continue
        if scale is None:
            rgb = np.array(img.convert("RGB").resize((cw, ch), Image.LANCZOS))
            if name == "S7_BG_LAB_MATCH":
                H = register_to_reference(rgb, ref)
                moved = cv2.perspectiveTransform(np.float32([[[266, 1549]], [[1019, 1388]], [[61, 1218]], [[814, 1057]]]), H)
                print("  S7_BG_LAB_MATCH registration, floor corners moved by (px):",
                      np.round(np.linalg.norm(moved[:, 0] - [[266, 1549], [1019, 1388], [61, 1218], [814, 1057]], axis=1), 1).tolist())
                rgb = cv2.warpPerspective(rgb, H, (cw, ch), flags=cv2.INTER_LANCZOS4 | cv2.WARP_INVERSE_MAP, borderMode=cv2.BORDER_REPLICATE)
                rgb = match_colour(rgb, ref)
            anchor = coghe_anchor(np.dstack([rgb, np.full(rgb.shape[:2], 255, np.uint8)]), SEARCH.get(name)) if name in COGHE_IN else None
            s = BG_MAX / max(cw, ch)
            out = Image.fromarray(rgb).resize((round(cw * s), round(ch * s)), Image.LANCZOS)
            rect = (0, 0, cw, ch)
        else:
            canvas = np.array(fit_canvas(Image.fromarray(clean_alpha(np.array(img.convert("RGBA")))), cw, ch))
            anchor = coghe_anchor(canvas, SEARCH.get(name)) if name in COGHE_IN else None
            ys, xs = np.nonzero(canvas[..., 3])
            x0, y0 = max(0, xs.min() - 2), max(0, ys.min() - 2)
            x1, y1 = min(cw, xs.max() + 3), min(ch, ys.max() + 3)
            crop = Image.fromarray(canvas[y0:y1, x0:x1])
            out = crop.resize((max(4, round(crop.width * scale)), max(4, round(crop.height * scale))), Image.LANCZOS)
            rect = (x0, y0, x1 - x0, y1 - y0)
        for ext in (".png", ".jpg"):                      # one file per name (Resources loads by name)
            if os.path.exists(os.path.join(OUT, name + ext)): os.remove(os.path.join(OUT, name + ext))
        if scale is None: out.save(os.path.join(OUT, name + ".jpg"), quality=93, subsampling=0)   # opaque: jpg keeps the repo light
        else: out.save(os.path.join(OUT, name + ".png"), optimize=True)
        line = f"{name} {rect[0] / cw:.5f} {rect[1] / ch:.5f} {rect[2] / cw:.5f} {rect[3] / ch:.5f}"
        if anchor: line += f" {anchor[0] / cw:.5f} {anchor[1] / ch:.5f} {anchor[2] / cw:.5f} {anchor[3] / ch:.5f}"
        lines.append(line)
        print(f"  {name}: {img.width}x{img.height} -> {out.width}x{out.height}" + (f", COghe at {anchor[0]:.0f},{anchor[1]:.0f} w {anchor[2]:.0f}" if anchor else ""))
        sheet.append((name, out))
    with open(os.path.join(OUT, "layout.txt"), "w") as f:
        f.write("# name  x y w h (trimmed texture in its canvas, 0..1 from top-left)  [COghe body centre x y, width, bottom]\n")
        f.write("\n".join(lines) + "\n")
    if SHEET and sheet:
        cell = 300
        s = Image.new("RGB", (cell * 6, cell * ((len(sheet) + 5) // 6) * 2), (128, 128, 128))
        for i, (n, im) in enumerate(sheet):
            t = im.copy(); t.thumbnail((cell - 8, cell * 2 - 8))
            bg = Image.new("RGBA", t.size, (128, 128, 128, 255)); bg.alpha_composite(t.convert("RGBA"))
            s.paste(bg.convert("RGB"), ((i % 6) * cell + 4, (i // 6) * cell * 2 + 4))
        s.save(SHEET)


if __name__ == "__main__":
    main()
