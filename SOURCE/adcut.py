"""COghe ad editor: real Unity takes → one ad in several aspect ratios (9:16, 1:1, 16:9), music + effects, end card.

Usage: python -I adcut.py <spec.json> <out dir> <ffmpeg> [formats, e.g. 9x16,1x1,16x9]

A spec lists shots (take folder, frame in/out, speed, zoom, focus), captions, effect cues and the end card. Takes are
PNG folders written by the Explicit recorders (30 fps). If a take has track.txt (frame, x, y from the bottom-left), the
camera can follow COghe ("focus": "track"); otherwise "focus" is a fixed [x, y] in source pixels from the top-left.
"""
import os, sys, json, glob, math, shutil, subprocess
import numpy as np, soundfile as sf
from PIL import Image, ImageDraw, ImageFont, ImageFilter

FPS = 30
HOME = '/Users/tommynguyen/.buzz'
FONT_B = HOME + '/REPOS/GravityBox-spatial20/Assets/_Game/Venom/Resources/COgheUI/ManropeBold.ttf'
FONT_R = HOME + '/REPOS/GravityBox-spatial20/Assets/_Game/Venom/Resources/COgheUI/Manrope.ttf'
SFX = HOME + '/OUTBOX/COGHE_AD_30S_2026_10_03/AUDIO/SFX_ONE_SHOTS/'
MUSIC = HOME + '/OUTBOX/COGHE_AD_30S_2026_10_03/AUDIO/COGHE_AD_MUSIC_120BPM_30S.wav'
ICON = HOME + '/OUTBOX/COGHE_APP_ICON_2026_10_07/COGHE_APP_ICON_1024.png'
INK, TEAL, PAPER, MUTED, CORAL = (48, 78, 86), (53, 109, 109), (250, 249, 238), (98, 119, 123), (232, 116, 92)
SIZES = {'9x16': (1080, 1920), '1x1': (1080, 1080), '16x9': (1920, 1080), '886x1920': (886, 1920)}

fonts = {}
def font(path, size):
    if (path, size) not in fonts: fonts[(path, size)] = ImageFont.truetype(path, size)
    return fonts[(path, size)]

def run(cmd): subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

cache = {}
def load(path):
    if path not in cache:
        if len(cache) > 24: cache.clear()
        cache[path] = Image.open(path).convert('RGB')
    return cache[path]

class Take:
    def __init__(self, folder):
        self.dir = folder
        self.files = sorted(glob.glob(os.path.join(folder, '[0-9]*.png'))) or sorted(glob.glob(os.path.join(folder, 'frame_[0-9]*.png')))
        assert self.files, 'no frames in ' + folder
        self.size = Image.open(self.files[0]).size
        self.track = {}
        p = os.path.join(folder, 'track.txt')
        if os.path.exists(p):
            for line in open(p):
                v = line.split()
                if len(v) >= 3: self.track[int(v[0])] = (float(v[1]), self.size[1] - float(v[2]))
        self.taps = []
        p = os.path.join(folder, 'taps.txt')
        if not os.path.exists(p): p = os.path.join(os.path.dirname(folder.rstrip('/')), 'taps.txt')
        if os.path.exists(p):
            for line in open(p):
                f, x, y = line.split()[:3]; self.taps.append((int(f), float(x), self.size[1] - float(y)))

    def frame(self, f): return self.files[max(0, min(len(self.files) - 1, int(round(f))))]

    def follow(self, f, a, b, smooth=18):
        """COghe's position at frame f, averaged over a window so the camera glides (clamped to the shot)."""
        keys = [k for k in range(int(f) - smooth, int(f) + smooth + 1) if a <= k <= b and k in self.track]
        if not keys: return None
        w = [math.exp(-((k - f) / (smooth / 2)) ** 2) for k in keys]
        return (sum(self.track[k][0] * wi for k, wi in zip(keys, w)) / sum(w), sum(self.track[k][1] * wi for k, wi in zip(keys, w)) / sum(w))

def crop_box(take, aspect, zoom, centre):
    sw, sh = take.size
    w = sw / zoom; h = w / aspect
    if h > sh: h = sh; w = h * aspect
    cx = min(max(centre[0], w / 2), sw - w / 2); cy = min(max(centre[1], h / 2), sh - h / 2)
    return (cx - w / 2, cy - h / 2, cx + w / 2, cy + h / 2)

# ---- overlays ---------------------------------------------------------------------------------------------------------
def pill_text(layer, text, sub, W, H, y, alpha, scale, rise, colour=PAPER, ink=INK):
    d = ImageDraw.Draw(layer)
    f = font(FONT_B, int(64 * scale)); fs = font(FONT_R, int(38 * scale))
    tw = d.textlength(text, font=f); sw = d.textlength(sub, font=fs) if sub else 0
    pw = max(tw, sw) + 92 * scale; ph = (124 + (54 if sub else 0)) * scale
    x0 = (W - pw) / 2; y0 = y + rise
    shadow = Image.new('RGBA', (W, H), (0, 0, 0, 0)); ds = ImageDraw.Draw(shadow)
    ds.rounded_rectangle((x0, y0 + 10 * scale, x0 + pw, y0 + ph + 10 * scale), 46 * scale, fill=(20, 40, 40, int(70 * alpha)))
    layer.alpha_composite(shadow.filter(ImageFilter.GaussianBlur(14 * scale)))
    d.rounded_rectangle((x0, y0, x0 + pw, y0 + ph), 46 * scale, fill=colour + (int(244 * alpha),))
    d.text(((W - tw) / 2, y0 + 26 * scale), text, font=f, fill=ink + (int(255 * alpha),))
    if sub: d.text(((W - sw) / 2, y0 + 116 * scale), sub, font=fs, fill=MUTED + (int(255 * alpha),))

def caption(img, cap, t, fmt):
    W, H = img.size; c0, c1 = cap['t']
    if not (c0 <= t < c1): return
    into, left = t - c0, c1 - t; alpha = min(1, into / .18, left / .14); rise = int(16 * (1 - min(1, into / .22)))
    scale = {'9x16': 1.25, '886x1920': 1.12, '1x1': .95, '16x9': .95}[fmt]
    pos = cap.get('pos', 'top')
    y = {'top': {'9x16': 190, '886x1920': 190, '1x1': 70, '16x9': 64}, 'low': {'9x16': 1420, '886x1920': 1420, '1x1': 760, '16x9': 760}}[pos][fmt]
    layer = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    pill_text(layer, cap['text'], cap.get('sub'), W, H, y, alpha, scale, rise, ink=CORAL if cap.get('accent') else INK)
    img.alpha_composite(layer)

def ripple(img, x, y, age, scale):
    layer = Image.new('RGBA', img.size, (0, 0, 0, 0)); d = ImageDraw.Draw(layer); k = age / 18
    if age < 10:
        a = 1 - age / 10; r0 = 34 * scale
        d.ellipse((x - r0, y - r0, x + r0, y + r0), fill=(255, 255, 255, int(170 * a)), outline=TEAL + (int(230 * a),), width=max(2, int(6 * scale)))
    r = (30 + 70 * k) * scale; a = (1 - k) ** 1.5
    d.ellipse((x - r, y - r, x + r, y + r), outline=(255, 255, 255, int(235 * a)), width=max(2, int(8 * scale)))
    img.alpha_composite(layer)

icon_cache = {}
def end_card(img, t, t0, card, fmt):
    W, H = img.size; k = min(1, max(0, (t - t0) / .45)); e = 1 - (1 - k) ** 3
    layer = Image.new('RGBA', (W, H), PAPER + (int(255 * e),)); d = ImageDraw.Draw(layer)
    s = min(W / 1080, H / 1080) if fmt not in ('9x16', '886x1920') else W / 1080 * 1.2
    isz = int(300 * s)
    if isz not in icon_cache:
        ic = Image.open(ICON).convert('RGB').resize((isz, isz), Image.LANCZOS)
        m = Image.new('L', (isz, isz), 0); ImageDraw.Draw(m).rounded_rectangle((0, 0, isz - 1, isz - 1), int(isz * .225), fill=255)
        icon_cache[isz] = (ic, m)
    ic, m = icon_cache[isz]
    if fmt == '16x9':
        cx, top = W * .30, H / 2 - isz / 2 - 20 * s
        ix, iy = int(cx - isz / 2), int(top + 30 * (1 - e))
        tx0 = W * .50
    else:
        cy = H * (.36 if fmt in ('9x16', '886x1920') else .30)
        ix, iy = int(W / 2 - isz / 2), int(cy - isz / 2 + 30 * (1 - e))
    sh = Image.new('RGBA', (W, H), (0, 0, 0, 0)); ImageDraw.Draw(sh).rounded_rectangle((ix, iy + 18 * s, ix + isz, iy + isz + 18 * s), int(isz * .225), fill=(20, 40, 40, int(90 * e)))
    layer.alpha_composite(sh.filter(ImageFilter.GaussianBlur(22 * s)))
    icm = m.point(lambda v: int(v * e)); layer.paste(ic, (ix, iy), icm)
    f = font(FONT_B, int(150 * s)); parts = [('C', INK), ('O', TEAL), ('ghe', INK)]
    total = sum(d.textlength(p, font=f) for p, _ in parts)
    tag = card.get('tagline', 'Physics puzzles with a liquid pet'); ts = font(FONT_R, int(46 * s)); tw = d.textlength(tag, font=ts)
    cta = card.get('cta', 'PLAY FREE'); cf = font(FONT_B, int(54 * s)); cw = d.textlength(cta, font=cf); bw, bh = cw + 120 * s, 118 * s
    if fmt == '16x9':
        x, y = tx0, H / 2 - 230 * s
        for p, c in parts: d.text((x, y), p, font=f, fill=c + (int(255 * e),)); x += d.textlength(p, font=f)
        d.text((tx0 + 6 * s, y + 200 * s), tag, font=ts, fill=MUTED + (int(255 * e),))
        bx, by = tx0, y + 300 * s
    else:
        x, y = (W - total) / 2, iy + isz + 40 * s
        for p, c in parts: d.text((x, y), p, font=f, fill=c + (int(255 * e),)); x += d.textlength(p, font=f)
        d.text(((W - tw) / 2, y + 196 * s), tag, font=ts, fill=MUTED + (int(255 * e),))
        bx, by = (W - bw) / 2, y + 300 * s
    pulse = 1 + .04 * math.sin(max(0, t - t0 - .5) * 2 * math.pi * 1.6)
    cx, cyy = bx + bw / 2, by + bh / 2; bw2, bh2 = bw * pulse, bh * pulse
    d.rounded_rectangle((cx - bw2 / 2, cyy - bh2 / 2, cx + bw2 / 2, cyy + bh2 / 2), bh2 / 2, fill=CORAL + (int(255 * e),))
    d.text((cx - cw / 2, cyy - bh2 / 2 + 26 * s), cta, font=cf, fill=(255, 255, 255, int(255 * e)))
    img.alpha_composite(layer)

# ---- render -------------------------------------------------------------------------------------------------------------
def render(spec, fmt, out_dir, ff, clips_root):
    W, H = SIZES[fmt]; aspect = W / H
    takes = {}
    def take(name):
        if name not in takes: takes[name] = Take(os.path.join(clips_root, name) if not name.startswith('/') else name)
        return takes[name]
    timeline = []
    for i, s in enumerate(spec['shots']):
        n = round((s['out'] - s['in']) / s.get('speed', 1)) if 'frames' not in s else s['frames']
        for k in range(n): timeline.append((i, k, n))
    dur = spec['duration']; total = round(dur * FPS)
    assert len(timeline) >= total - round(spec.get('card', {}).get('hold', 0) * FPS), (len(timeline), total)
    work = os.path.join(out_dir, '_frames_' + fmt); shutil.rmtree(work, ignore_errors=True); os.makedirs(work)
    last = None
    for f in range(total):
        t = f / FPS
        if f < len(timeline):
            i, k, n = timeline[f]; s = spec['shots'][i]; tk = take(s['take'])
            src = s['in'] + k * s.get('speed', 1)
            img = load(tk.frame(src))
            focus = s.get('focus', 'track')
            centre = None
            if focus == 'track': centre = tk.follow(src, s['in'], s['out'], s.get('smooth', 18))
            elif isinstance(focus, list): centre = focus
            if centre is None: centre = (tk.size[0] / 2, tk.size[1] / 2)
            if fmt in s.get('offset', {}): centre = (centre[0] + s['offset'][fmt][0], centre[1] + s['offset'][fmt][1])
            zd = s.get('zoom'); z = (zd.get(fmt) or zd.get('9x16' if fmt == '886x1920' else '*') or zd.get('*', 1.0)) if isinstance(zd, dict) else (zd or 1.0)
            z *= 1 + s.get('push', .05) * (k / max(1, n - 1))
            box = crop_box(tk, aspect, z, centre); scale = W / (box[2] - box[0])
            panel = s.get('panel', {}).get(fmt)
            if panel:   # a portrait take in a wide format: the shot in a rounded panel over a soft, blurred copy of itself
                k = W / 1920; m = int(44 * k); ph = H - 2 * m; pw = int(ph * panel['aspect'])
                pbox = crop_box(tk, panel['aspect'], panel.get('zoom', 1.0), centre)
                pimg = img.resize((pw, ph), Image.LANCZOS, box=pbox)
                bg = img.resize((W // 8, H // 8), Image.BILINEAR, box=box).filter(ImageFilter.GaussianBlur(6)).resize((W, H), Image.BICUBIC)
                frame = Image.blend(bg, Image.new('RGB', (W, H), PAPER), .45).convert('RGBA')
                sh = Image.new('RGBA', (W, H), (0, 0, 0, 0)); x0 = (W - pw) // 2
                ImageDraw.Draw(sh).rounded_rectangle((x0, m + 14 * k, x0 + pw, m + ph + 14 * k), 40 * k, fill=(20, 40, 40, 90))
                frame.alpha_composite(sh.filter(ImageFilter.GaussianBlur(18 * k)))
                mask = Image.new('L', (pw, ph), 0); ImageDraw.Draw(mask).rounded_rectangle((0, 0, pw - 1, ph - 1), int(40 * k), fill=255)
                frame.paste(pimg, (x0, m), mask)
                box = (0, 0, 1, 1); scale = 0   # no tap ripples in panel mode
            else:
                frame = img.resize((W, H), Image.LANCZOS, box=box).convert('RGBA')
            for tf, x, y in (tk.taps if scale else []):
                age = (src - tf) / s.get('speed', 1)
                if 0 <= age < 18 and s['in'] <= tf <= s['out']: ripple(frame, (x - box[0]) * scale, (y - box[1]) * scale, age, W / 1080)
            last = frame
        else:
            frame = last.copy()
        for cap in spec.get('captions', []): caption(frame, cap, t, fmt)
        card = spec.get('card')
        if card and card.get('show', True) and t >= card['t']: end_card(frame, t, card['t'], card, fmt)
        frame.convert('RGB').save(os.path.join(work, '%05d.png' % f), compress_level=1)
    return work, total

def mix(spec, path):
    music, sr = sf.read(MUSIC); dur = spec['duration']
    start = spec.get('music_from', 0.0)
    music = music[int(start * sr): int((start + dur) * sr)] * spec.get('music_gain', .62)
    if len(music) < int(dur * sr): music = np.concatenate([music, np.zeros((int(dur * sr) - len(music), music.shape[1]))])
    fade = int(.6 * sr); music[-fade:] *= np.linspace(1, 0, fade)[:, None]
    fi = int(.03 * sr); music[:fi] *= np.linspace(0, 1, fi)[:, None]
    for ts, name, gain in spec.get('cues', []):
        x, xsr = sf.read(SFX + name + '.wav')
        if x.ndim == 1: x = np.stack([x, x], 1)
        if xsr != sr: x = np.interp(np.arange(0, len(x), xsr / sr), np.arange(len(x)), x[:, 0])[:, None].repeat(2, 1)
        s = int(ts * sr); e = min(len(music), s + len(x))
        if s < len(music): music[s:e] += x[: e - s] * gain
    music *= 10 ** (-1 / 20) / max(1e-9, np.abs(music).max())
    sf.write(path, music, sr, subtype='PCM_16')

def encode(ff, frames, wav, out, total, preview=False):
    # App Store app previews: H.264 High profile level 4.0 at 10–12 Mbps, AAC 256 kbps stereo (Apple's preview specification)
    rate = ['-b:v', '11M', '-maxrate', '12M', '-bufsize', '24M', '-profile:v', 'high', '-level', '4.0'] if preview else ['-crf', '18']
    run([ff, '-y', '-framerate', str(FPS), '-i', frames + '/%05d.png', '-i', wav, '-frames:v', str(total), '-c:v', 'libx264', '-preset', 'slow',
         *rate, '-pix_fmt', 'yuv420p', '-r', str(FPS), '-c:a', 'aac', '-b:a', '256k' if preview else '192k', '-ar', '48000', '-shortest', '-map_metadata', '-1',
         '-map_chapters', '-1', '-metadata:s:v', 'handler_name=', '-metadata:s:a', 'handler_name=', '-fflags', '+bitexact', '-flags:v', '+bitexact',
         '-flags:a', '+bitexact', '-movflags', '+faststart', out])

def expand(spec):
    """Shot captions ("cap") become timed captions; the end card starts when the shots end unless given."""
    t = 0.0; caps = list(spec.get('captions', []))
    for s in spec['shots']:
        n = round((s['out'] - s['in']) / s.get('speed', 1)); t0, t = t, t + n / FPS
        c = s.get('cap')
        if c:
            if caps and caps[-1].get('_auto') and caps[-1]['text'] == c['text'] and abs(caps[-1]['t'][1] - (t0 - .05)) < 1e-6:
                caps[-1]['t'][1] = t - .05
            else: caps.append(dict(c, t=[t0 + .05, t - .05], _auto=True))
    spec['captions'] = caps
    card = spec.setdefault('card', {}); card.setdefault('t', round(t, 3))
    spec.setdefault('duration', round(card['t'] + card.get('hold', 2.0), 3))
    return spec

def main():
    spec_path, out_dir, ff = sys.argv[1], sys.argv[2], sys.argv[3]
    formats = sys.argv[4].split(',') if len(sys.argv) > 4 else ['9x16', '1x1', '16x9']
    spec = expand(json.load(open(spec_path))); os.makedirs(out_dir, exist_ok=True)
    print(spec['name'], 'duration', spec['duration'], 'card at', spec['card']['t'])
    clips_root = spec.get('clips', HOME + '/REPOS/GravityBox-spatial20/Artifacts/Clips')
    wav = os.path.join(out_dir, '_mix_%s.wav' % spec['name']); mix(spec, wav)
    for fmt in formats:
        frames, total = render(spec, fmt, out_dir, ff, clips_root)
        W, H = SIZES[fmt]
        out = os.path.join(out_dir, '%s_%s_%dX%d.mp4' % (spec['name'], fmt.upper(), W, H))
        encode(ff, frames, wav, out, total, spec.get('preview', False)); shutil.rmtree(frames); print('wrote', out)
    os.remove(wav)

if __name__ == '__main__': main()
