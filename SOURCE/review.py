# Grid of frames from a video at given times: review.py <video> <out.jpg> <t1,t2,...> [height]
import sys, subprocess, os, tempfile
from PIL import Image, ImageDraw
v, out, ts = sys.argv[1], sys.argv[2], [float(t) for t in sys.argv[3].split(',')]
H = int(sys.argv[4]) if len(sys.argv) > 4 else 400
tiles = []
with tempfile.TemporaryDirectory() as d:
    for i, t in enumerate(ts):
        p = os.path.join(d, f'{i}.png')
        subprocess.run(['/opt/homebrew/bin/ffmpeg', '-v', 'error', '-y', '-ss', str(t), '-i', v, '-frames:v', '1', p], check=True)
        im = Image.open(p).convert('RGB'); im = im.resize((int(im.width * H / im.height), H), Image.LANCZOS)
        ImageDraw.Draw(im).text((4, 4), f'{t:.1f}s', fill='red'); tiles.append(im)
cols = min(len(tiles), max(1, 2000 // tiles[0].width)); rows = (len(tiles) + cols - 1) // cols
sheet = Image.new('RGB', (cols * (tiles[0].width + 6), rows * (H + 6)), 'white')
for i, im in enumerate(tiles): sheet.paste(im, ((i % cols) * (tiles[0].width + 6), (i // cols) * (H + 6)))
sheet.save(out, quality=85)
