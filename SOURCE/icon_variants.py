# Recolour only the backdrop of the approved COghe icon. The creature's silhouette is everything that is not mint/teal
# (holes for eyes and highlights filled); everything outside is gradient-mapped by luminance into a new colour family, so the
# backdrop's shading and the contact shadow keep their shape. Edge pixels blend by how teal they are.
import sys, numpy as np
from PIL import Image
from scipy import ndimage as nd
src, out_dir = sys.argv[1], sys.argv[2]
im = np.asarray(Image.open(src).convert('RGB')).astype(np.float32) / 255
mx, mn = im.max(2), im.min(2); d = mx - mn + 1e-6
s = d / (mx + 1e-6); v = mx
r, g, b = im[..., 0], im[..., 1], im[..., 2]
h = np.where(mx == r, ((g - b) / d) % 6, np.where(mx == g, (b - r) / d + 2, (r - g) / d + 4)) * 60
# backdrop pixels are mint/teal (hue 135–192); the body's rim lights are blue (≈210) or white, its skin near-black.
# Far from the body every pixel is backdrop. On and near the body (its underside reflects the floor) the weight is how
# teal the pixel is, with soft ramps, so the reflected floor changes colour smoothly and no hard edge appears.
backdrop = (h > 135) & (h < 192) & (s > 0.12) & (v > 0.15)
body = nd.binary_opening(~backdrop, iterations=2)
body = nd.binary_fill_holes(nd.binary_closing(body, iterations=3))
lab, n = nd.label(body); sizes = nd.sum(body, lab, range(1, n + 1))
body = lab == (1 + int(np.argmax(sizes)))          # the creature only
far = nd.distance_transform_edt(~body) > 5
teal = np.clip(1 - np.abs(h - 165) / 30, 0, 1) ** 0.5 * np.clip((s - 0.06) / 0.20, 0, 1) * np.clip((v - 0.02) / 0.08, 0, 1)
w = np.where(far, 1.0, teal)
w = nd.gaussian_filter(w, 0.7)[..., None]
core = far
L = 0.2126 * r + 0.7152 * g + 0.0722 * b
lo, hi = np.percentile(L[core], 1), np.percentile(L[core], 99.7)
t = np.clip((L - lo) / (hi - lo), 0, 1)[..., None]
families = {   # (deep, mid, light) in sRGB
    'CORAL':    ((206, 78, 64),  (244, 132, 104), (255, 226, 210)),
    'LAVENDER': ((98, 74, 190),  (150, 126, 232), (236, 230, 255)),
    'SUN':      ((232, 140, 24), (252, 192, 60),  (255, 246, 206)),
}
for name, (deep, mid, light) in families.items():
    deep, mid, light = (np.array(c, np.float32) / 255 for c in (deep, mid, light))
    rec = np.where(t < 0.5, deep + (mid - deep) * (t / 0.5), mid + (light - mid) * ((t - 0.5) / 0.5))
    rec = rec * np.clip(L / lo, 0, 1)[..., None]    # shadows darker than the open backdrop (contact shadow) stay dark
    out = im * (1 - w) + rec * w
    Image.fromarray((np.clip(out, 0, 1) * 255 + 0.5).astype(np.uint8), 'RGB').save(f'{out_dir}/COGHE_ICON_{name}_1024.png', optimize=True)
Image.fromarray((w[..., 0] * 255).astype(np.uint8)).save(f'{out_dir}/mask.png')
print('backdrop share', float(w.mean()))
