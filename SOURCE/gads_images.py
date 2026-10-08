"""Google Ads image assets for the App campaign: real game frames, a small wordmark pill (overlay well under 20% of the area).

Usage: python -I gads_images.py <spec.json> <out dir>
Spec: {"images": [{"name", "src", "focus" [x, y], "zoom", "label" (short text)}]}; each is written at 1200×1200, 1200×628 and 1200×1500.
"""
import os, sys, json
from PIL import Image, ImageDraw, ImageFont, ImageFilter

HOME = '/Users/tommynguyen/.buzz'
FONT_B = HOME + '/REPOS/GravityBox-spatial20/Assets/_Game/Venom/Resources/COgheUI/ManropeBold.ttf'
ICON = HOME + '/OUTBOX/COGHE_APP_ICON_2026_10_07/COGHE_APP_ICON_1024.png'
INK, TEAL, PAPER = (48, 78, 86), (53, 109, 109), (250, 249, 238)
SIZES = [(1200, 1200), (1200, 628), (1200, 1500)]

def crop(src, W, H, focus, zoom):
    sw, sh = src.size; a = W / H
    w = sw / zoom; h = w / a
    if h > sh: h = sh; w = h * a
    cx = min(max(focus[0], w / 2), sw - w / 2); cy = min(max(focus[1], h / 2), sh - h / 2)
    return src.resize((W, H), Image.LANCZOS, box=(cx - w / 2, cy - h / 2, cx + w / 2, cy + h / 2))

def make(spec, W, H):
    src = Image.open(spec['src']).convert('RGB')
    z = spec.get('zoom', {}); z = z.get('%dx%d' % (W, H), z.get('*', 1.0)) if isinstance(z, dict) else z
    focus = spec.get('focus', [src.width / 2, src.height / 2]); pa = spec.get('panel', {}).get('%dx%d' % (W, H))
    if pa:   # a wide image from a tall take: the shot in a rounded panel over a soft, blurred copy of itself
        bg = crop(src, W // 8, H // 8, focus, 1.0).filter(ImageFilter.GaussianBlur(5)).resize((W, H), Image.BICUBIC)
        img = Image.blend(bg, Image.new('RGB', (W, H), PAPER), .5).convert('RGBA')
        k0 = H / 628; mg = int(26 * k0); ph = H - 2 * mg; pw = int(ph * pa); x0 = W - pw - mg
        pic = crop(src, pw, ph, focus, z)
        sh = Image.new('RGBA', (W, H), (0, 0, 0, 0)); ImageDraw.Draw(sh).rounded_rectangle((x0, mg + 10, x0 + pw, mg + ph + 10), 30 * k0, fill=(20, 40, 40, 80))
        img.alpha_composite(sh.filter(ImageFilter.GaussianBlur(14)))
        m = Image.new('L', (pw, ph), 0); ImageDraw.Draw(m).rounded_rectangle((0, 0, pw - 1, ph - 1), int(30 * k0), fill=255); img.paste(pic, (x0, mg), m)
    else:
        img = crop(src, W, H, focus, z).convert('RGBA')
    if pa:   # brand block centred in the free area on the left: icon, name, label
        free = x0 - mg; k = H / 628; isz = int(150 * k)
        ic = Image.open(ICON).convert('RGB').resize((isz, isz), Image.LANCZOS)
        m = Image.new('L', (isz, isz), 0); ImageDraw.Draw(m).rounded_rectangle((0, 0, isz - 1, isz - 1), int(isz * .225), fill=255)
        f = ImageFont.truetype(FONT_B, int(84 * k)); fl = ImageFont.truetype(FONT_B, int(40 * k)); d = ImageDraw.Draw(img)
        tw = d.textlength('COghe', font=f); lw = d.textlength(spec.get('label', ''), font=fl)
        bh = isz + 24 * k + 96 * k + 56 * k; y = (H - bh) / 2; cx = mg + free / 2
        sh = Image.new('RGBA', img.size, (0, 0, 0, 0)); ImageDraw.Draw(sh).rounded_rectangle((cx - isz / 2, y + 10 * k, cx + isz / 2, y + isz + 10 * k), isz * .225, fill=(20, 40, 40, 80))
        img.alpha_composite(sh.filter(ImageFilter.GaussianBlur(12 * k))); d = ImageDraw.Draw(img)
        img.paste(ic, (int(cx - isz / 2), int(y)), m); y += isz + 24 * k
        x = cx - tw / 2
        for part, col in (('C', INK), ('O', TEAL), ('ghe', INK)): d.text((x, y), part, font=f, fill=col + (255,)); x += d.textlength(part, font=f)
        d.text((cx - lw / 2, y + 100 * k), spec.get('label', ''), font=fl, fill=TEAL + (255,))
        overlay = (max(isz, tw, lw) * bh) / (W * H)
        return img.convert('RGB'), overlay
    k = min(W, H) / 1000
    # pill: icon + "COghe" (+ label), bottom-left
    isz = int(84 * k); ic = Image.open(ICON).convert('RGB').resize((isz, isz), Image.LANCZOS)
    m = Image.new('L', (isz, isz), 0); ImageDraw.Draw(m).rounded_rectangle((0, 0, isz - 1, isz - 1), int(isz * .225), fill=255)
    f = ImageFont.truetype(FONT_B, int(46 * k)); fl = ImageFont.truetype(FONT_B, int(34 * k))
    d = ImageDraw.Draw(img); tw = d.textlength('COghe', font=f); lw = d.textlength(spec.get('label', ''), font=fl) if spec.get('label') else 0
    pw = int(isz + 30 * k + max(tw, lw) + 44 * k); ph = int(isz + 28 * k); x0, y0 = int(28 * k), int(H - ph - 28 * k)
    sh = Image.new('RGBA', img.size, (0, 0, 0, 0)); ImageDraw.Draw(sh).rounded_rectangle((x0, y0 + 8 * k, x0 + pw, y0 + ph + 8 * k), ph / 2, fill=(20, 40, 40, 70))
    img.alpha_composite(sh.filter(ImageFilter.GaussianBlur(12 * k)))
    d = ImageDraw.Draw(img); d.rounded_rectangle((x0, y0, x0 + pw, y0 + ph), ph / 2, fill=PAPER + (245,))
    img.paste(ic, (int(x0 + 14 * k), int(y0 + 14 * k)), m)
    tx = x0 + 14 * k + isz + 18 * k
    if spec.get('label'):
        d.text((tx, y0 + 12 * k), 'COghe', font=f, fill=INK + (255,)); d.text((tx, y0 + 12 * k + 52 * k), spec['label'], font=fl, fill=TEAL + (255,))
    else:
        d.text((tx, y0 + (ph - 56 * k) / 2), 'COghe', font=f, fill=INK + (255,))
    overlay = pw * ph / (W * H)
    return img.convert('RGB'), overlay

def main():
    spec = json.load(open(sys.argv[1])); out = sys.argv[2]; os.makedirs(out, exist_ok=True)
    for s in spec['images']:
        for W, H in SIZES:
            im, ov = make(s, W, H); p = os.path.join(out, 'GADS_%s_%dx%d.jpg' % (s['name'], W, H))
            im.save(p, quality=90); print('%s overlay %.1f%%' % (p, ov * 100))

if __name__ == '__main__': main()
