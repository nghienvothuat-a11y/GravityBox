# Filmstrip of a take segment cropped around COghe (track.txt): strip.py <take dir> <out> <a> <b> <n> <zoom>
import sys, os, glob
from PIL import Image, ImageDraw
d, out, a, b, n, z = sys.argv[1], sys.argv[2], int(sys.argv[3]), int(sys.argv[4]), int(sys.argv[5]), float(sys.argv[6])
files = sorted(glob.glob(os.path.join(d, '[0-9]*.png'))); W0, H0 = Image.open(files[0]).size
tr = {}
for line in open(os.path.join(d, 'track.txt')):
    v = line.split()
    if len(v) >= 3: tr[int(v[0])] = (float(v[1]), H0 - float(v[2]))
w, h = 200, 356; sheet = Image.new('RGB', (n * w, h + 16), 'white'); dr = ImageDraw.Draw(sheet)
for i in range(n):
    f = a + round(i * (b - a) / max(1, n - 1)); im = Image.open(files[min(f, len(files) - 1)]).convert('RGB')
    cx, cy = tr.get(f, (W0 / 2, H0 / 2)); cw = W0 / z; ch = cw * 16 / 9
    x0 = min(max(cx - cw / 2, 0), W0 - cw); y0 = min(max(cy - ch / 2, 0), H0 - ch)
    sheet.paste(im.resize((w, h), Image.LANCZOS, box=(x0, y0, x0 + cw, y0 + ch)), (i * w, 16)); dr.text((i * w + 3, 2), str(f), fill='black')
sheet.save(out, quality=85)
