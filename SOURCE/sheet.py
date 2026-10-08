# Contact sheet of a take: N evenly spaced frames (full frame, small), frame numbers printed. Usage: sheet.py <take dir> <out.jpg> [n] [first] [last]
import sys, os, glob
from PIL import Image, ImageDraw
d, out = sys.argv[1], sys.argv[2]; n = int(sys.argv[3]) if len(sys.argv) > 3 else 16
frames = sorted(glob.glob(os.path.join(d, '[0-9]*.png')))
a = int(sys.argv[4]) if len(sys.argv) > 4 else 0; b = int(sys.argv[5]) if len(sys.argv) > 5 else len(frames) - 1
pick = [frames[a + round(i * (b - a) / max(1, n - 1))] for i in range(n)]
w, h = 220, 391; cols = 8; rows = (n + cols - 1) // cols
sheet = Image.new('RGB', (cols * w, rows * (h + 18)), 'white'); dr = ImageDraw.Draw(sheet)
for i, f in enumerate(pick):
    im = Image.open(f).convert('RGB'); im.thumbnail((w, h)); x, y = (i % cols) * w, (i // cols) * (h + 18)
    sheet.paste(im, (x, y + 18)); dr.text((x + 4, y + 3), os.path.basename(f)[:-4], fill='black')
sheet.save(out, quality=85)
