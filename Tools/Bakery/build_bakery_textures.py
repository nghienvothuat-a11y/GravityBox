import json
from pathlib import Path

import bpy
import numpy as np


ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "Assets/_Game/Venom/Art/Bakery"
SIZE = 512
random = np.random.default_rng(4907)
grid_y, grid_x = np.mgrid[0:SIZE, 0:SIZE] / SIZE


def save_png(name, pixels):
    texture = bpy.data.images.new(name, width=SIZE, height=SIZE, alpha=True)
    texture.colorspace_settings.name = "Non-Color"
    texture.pixels.foreach_set(np.asarray(pixels, dtype=np.float32).ravel())
    texture.filepath_raw = str(OUTPUT / name)
    texture.file_format = "PNG"
    texture.save()
    bpy.data.images.remove(texture)


manifest = json.loads((OUTPUT / "materials.json").read_text())
for name, frequency, depth in (("Biscuit", 260, 0.24), ("Wafer", 170, 0.16)):
    height = np.zeros((SIZE, SIZE))
    for index in range(frequency):
        centre_x, centre_y = random.random(2)
        radius = random.uniform(0.0025, 0.008)
        distance_x = np.minimum(abs(grid_x - centre_x), 1 - abs(grid_x - centre_x))
        distance_y = np.minimum(abs(grid_y - centre_y), 1 - abs(grid_y - centre_y))
        height -= random.uniform(0.4, 1) * np.exp(-(distance_x ** 2 + distance_y ** 2) / (2 * radius ** 2))
    grain = random.normal(0, 0.009, (SIZE, SIZE))
    shade = np.clip(0.95 + height * depth + grain, 0.45, 1)
    color = np.ones((SIZE, SIZE, 4))
    color[:, :, :3] = shade[:, :, None]
    save_png(name + "_BaseColor.png", color)
    gradient_x = (np.roll(height, -1, axis=1) - np.roll(height, 1, axis=1)) * 0.8
    gradient_y = (np.roll(height, -1, axis=0) - np.roll(height, 1, axis=0)) * 0.8
    normal = np.stack((-gradient_x, -gradient_y, np.ones_like(height)), axis=2)
    normal /= np.linalg.norm(normal, axis=2)[:, :, None]
    normal_pixels = np.ones((SIZE, SIZE, 4))
    normal_pixels[:, :, :3] = normal * 0.5 + 0.5
    save_png(name + "_Normal.png", normal_pixels)
    smoothness = next(entry["smoothness"] for entry in manifest["materials"] if entry["name"] == name)
    mask = np.zeros((SIZE, SIZE, 4))
    mask[:, :, 3] = np.clip(smoothness + height * 0.08 + grain, 0, 1)
    save_png(name + "_Mask.png", mask)
    entry = next(entry for entry in manifest["materials"] if entry["name"] == name)
    entry.update({"baseColorTexture": name + "_BaseColor.png", "normalTexture": name + "_Normal.png",
                  "maskTexture": name + "_Mask.png", "normalScale": 0.35,
                  "textureSize": SIZE, "wrap": "Repeat", "normalConvention": "OpenGL",
                  "maskChannels": "R=metallic(0), A=smoothness"})
(OUTPUT / "materials.json").write_text(json.dumps(manifest, indent=2))
print("Generated 6 deterministic 512px textures; BaseColor multiplies the manifest color.")
