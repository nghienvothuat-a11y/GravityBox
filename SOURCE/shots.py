"""COghe App Store screenshots: a headline band on top, a real game frame below (cropped around COghe).

Usage: python -I shots.py <spec.json> <out dir> [W H]   (default 1320 2868, the iPhone 6.9" portrait size)
Spec: {"shots": [{"name", "src" (PNG path), "focus" [x, y] or "track" (with "take" + "frame"), "zoom", "title", "sub",
"bg" [r, g, b], "ink" [r, g, b], "badge" (optional small label)}]}. Output: opaque RGB PNGs without metadata.
"""
import os, sys, json, math
from PIL import Image, ImageDraw, ImageFont, ImageFilter

HOME = '/Users/tommynguyen/.buzz'
FONT_B = HOME + '/REPOS/GravityBox-spatial20/Assets/_Game/Venom/Resources/COgheUI/ManropeBold.ttf'
FONT_R = HOME + '/REPOS/GravityBox-spatial20/Assets/_Game/Venom/Resources/COgheUI/Manrope.ttf'

def font(p, s): return ImageFont.truetype(p, s)

def track_at(take_dir, frame, size):
    pts = {}
    for line in open(os.path.join(take_dir, 'track.txt')):
        v = line.split()
        if len(v) >= 3: pts[int(v[0])] = (float(v[1]), size[1] - float(v[2]))
    k = min(pts, key=lambda f: abs(f - frame)); return pts[k]

def wrap(d, text, f, width):
    if '\n' in text: return text.split('\n')
    lines, cur = [], ''
    for word in text.split():
        t = (cur + ' ' + word).strip()
        if d.textlength(t, font=f) <= width or not cur: cur = t
        else: lines.append(cur); cur = word
    lines.append(cur); return lines

def make(s, W, H):
    band = int(H * s.get('band', .205))
    bg = tuple(s.get('bg', (53, 109, 109))); ink = tuple(s.get('ink', (255, 255, 255)))
    canvas = Image.new('RGB', (W, H), bg)
    # backdrop: a soft vertical gradient of the band colour
    grad = Image.new('RGB', (1, 256))
    for y in range(256):
        k = y / 255; grad.putpixel((0, y), tuple(int(c * (1.08 - .16 * k)) if c * 1.08 < 255 else c for c in bg))
    canvas.paste(grad.resize((W, H)), (0, 0))
    if s.get('tiles'):   # one large tile on top, two below (e.g. three looks of COghe)
        gw, gh = W, H - band + 60; top = H - gh; g = int(18 * W / 1320)
        boxes = [(0, top, W, top + gh // 2 - g // 2), (0, top + gh // 2 + g // 2, W // 2 - g // 2, H), (W // 2 + g // 2, top + gh // 2 + g // 2, W, H)]
        for t, (x0, y0, x1, y1) in zip(s['tiles'], boxes):
            im = Image.open(t['src']).convert('RGB'); tw, th = x1 - x0, y1 - y0; sw2, sh2 = im.size
            w = sw2 / t.get('zoom', 1.0); h = w * th / tw
            if h > sh2: h = sh2; w = h * tw / th
            cx, cy = t.get('focus', [sw2 / 2, sh2 / 2])
            bx = min(max(cx - w / 2, 0), sw2 - w); by = min(max(cy - h / 2, 0), sh2 - h)
            canvas.paste(im.resize((tw, th), Image.LANCZOS, box=(bx, by, bx + w, by + h)), (x0, y0))
        src = None
    else:
        src = Image.open(s['src']).convert('RGB'); sw, sh = src.size
    if src is not None:
        if s.get('focus') == 'track': cx, cy = track_at(os.path.dirname(s['src']), s['frame'], src.size)
        elif isinstance(s.get('focus'), list): cx, cy = s['focus']
        else: cx, cy = sw / 2, sh / 2
        cx += s.get('nudge', [0, 0])[0]; cy += s.get('nudge', [0, 0])[1]
        gw, gh = W, H - band + 60            # the game picture slides 60 px under the band's rounded edge
        z = s.get('zoom_wide', s.get('zoom', 1.0)) if W / H > .6 else s.get('zoom', 1.0); w = sw / z; h = w * gh / gw
        if h > sh: h = sh; w = h * gw / gh
        x0 = min(max(cx - w / 2, 0), sw - w); y0 = min(max(cy - h / 2, 0), sh - h)
        pic = src.resize((gw, gh), Image.LANCZOS, box=(x0, y0, x0 + w, y0 + h))
        canvas.paste(pic, (0, H - gh))
        if s.get('inset'):   # a round close-up of COghe from the same frame
            ins = s['inset']; kx, ky = W / 1320, H / 2868; k = (kx * ky) ** .5; dd = int(ins.get('d', 560) * k)
            if ins.get('at', 'track') == 'track': px, py = track_at(os.path.dirname(s['src']), s['frame'], src.size)
            else: px, py = ins['at']
            px += ins.get('nudge', [0, 0])[0]; py += ins.get('nudge', [0, 0])[1]
            r = sw / ins.get('zoom', 3.0) / 2
            face = src.resize((dd, dd), Image.LANCZOS, box=(px - r, py - r, px + r, py + r))
            m = Image.new('L', (dd * 4, dd * 4), 0); ImageDraw.Draw(m).ellipse((0, 0, dd * 4 - 1, dd * 4 - 1), fill=255); m = m.resize((dd, dd), Image.LANCZOS)
            ix, iy = int(ins['pos'][0] * kx - dd / 2), int(ins['pos'][1] * ky - dd / 2)
            sh = Image.new('RGBA', (W, H), (0, 0, 0, 0)); ImageDraw.Draw(sh).ellipse((ix - 6, iy + 14, ix + dd + 6, iy + dd + 26), fill=(10, 30, 30, 110))
            base = canvas.convert('RGBA'); base.alpha_composite(sh.filter(ImageFilter.GaussianBlur(22)))
            ring = int(14 * k); ImageDraw.Draw(base).ellipse((ix - ring, iy - ring, ix + dd + ring, iy + dd + ring), fill=(255, 255, 255, 255))
            base.paste(face, (ix, iy), m); canvas = base.convert('RGB')
    # the band, with a rounded lower edge and a soft shadow over the picture
    layer = Image.new('RGBA', (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(layer)
    shadow = Image.new('RGBA', (W, H), (0, 0, 0, 0)); ImageDraw.Draw(shadow).rounded_rectangle((-80, -200, W + 80, band + 18), 90, fill=(10, 30, 30, 90))
    layer.alpha_composite(shadow.filter(ImageFilter.GaussianBlur(26)))
    top = Image.new('RGBA', (W, H), (0, 0, 0, 0)); td = ImageDraw.Draw(top)
    td.rounded_rectangle((-80, -200, W + 80, band), 90, fill=bg + (255,))
    layer.alpha_composite(top)
    k = ((W / 1320) * (H / 2868)) ** .5   # type scales with the canvas area, so an iPad canvas keeps the phone's proportions
    f = font(FONT_B, int(s.get('size', 116) * k)); fs = font(FONT_R, int(54 * k))
    lines = wrap(d, s['title'], f, W - 150 * k)
    lh = f.size * 1.08; sub_h = (fs.size * 1.5) if s.get('sub') else 0
    block = lh * len(lines) + sub_h; y = (band - block) / 2 + 10 * k
    for line in lines:
        tw = d.textlength(line, font=f); d.text(((W - tw) / 2, y), line, font=f, fill=ink + (255,)); y += lh
    if s.get('sub'):
        tw = d.textlength(s['sub'], font=fs); d.text(((W - tw) / 2, y + fs.size * .25), s['sub'], font=fs, fill=ink + (215,))
    if s.get('badge'):
        bf = font(FONT_B, int(44 * k)); bw = d.textlength(s['badge'], font=bf) + 64 * k; bx, by = (W - bw) / 2, H - 150 * k
        d.rounded_rectangle((bx, by, bx + bw, by + 84 * k), 42 * k, fill=(255, 255, 255, 235))
        d.text((bx + 32 * k, by + 14 * k), s['badge'], font=bf, fill=tuple(s.get('bg', (53, 109, 109))) + (255,))
    out = canvas.convert('RGBA'); out.alpha_composite(layer); return out.convert('RGB')

def main():
    spec = json.load(open(sys.argv[1])); out = sys.argv[2]; os.makedirs(out, exist_ok=True)
    W, H = (int(sys.argv[3]), int(sys.argv[4])) if len(sys.argv) > 4 else (1320, 2868)
    for i, s in enumerate(spec['shots'], 1):
        im = make(s, W, H); p = os.path.join(out, '%02d_%s_%dx%d.png' % (i, s['name'], W, H))
        im.save(p, optimize=True); print('wrote', p)

if __name__ == '__main__': main()
