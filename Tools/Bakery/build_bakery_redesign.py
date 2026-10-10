import json
import math
from pathlib import Path

import bmesh
import bpy
import numpy as np
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "Assets/_Game/Venom/Art/Bakery"
REPORT = ROOT / "Tools/Bakery"
SIZE = 512
manifest = json.loads((OUTPUT / "materials.json").read_text())
additions = [
    {"name": "Glaze lilac", "color": {"r": .62, "g": .43, "b": .79, "a": 1}, "smoothness": .82},
    {"name": "Piped cream", "color": {"r": 1, "g": .91, "b": .76, "a": 1}, "smoothness": .32},
    {"name": "Sponge", "color": {"r": .94, "g": .66, "b": .32, "a": 1}, "smoothness": .2,
     "baseColorTexture": "Sponge_BaseColor.png", "normalTexture": "Sponge_Normal.png",
     "maskTexture": "Sponge_Mask.png", "normalScale": .65, "textureSize": SIZE,
     "wrap": "Repeat", "normalConvention": "OpenGL", "maskChannels": "R=metallic(0), A=smoothness"},
]
for addition in additions:
    manifest["materials"] = [entry for entry in manifest["materials"] if entry["name"] != addition["name"]]
    manifest["materials"].append(addition)
materials = {}
for entry in additions:
    material = bpy.data.materials.new(entry["name"])
    color = entry["color"]
    material.diffuse_color = (color["r"], color["g"], color["b"], 1)
    material.use_nodes = True
    surface = material.node_tree.nodes.get("Principled BSDF")
    surface.inputs["Base Color"].default_value = material.diffuse_color
    surface.inputs["Roughness"].default_value = 1 - entry["smoothness"]
    materials[entry["name"]] = material


def save_png(name, pixels):
    texture = bpy.data.images.new(name, width=SIZE, height=SIZE, alpha=True)
    texture.colorspace_settings.name = "Non-Color"
    texture.pixels.foreach_set(np.asarray(pixels, dtype=np.float32).ravel())
    texture.filepath_raw = str(OUTPUT / name)
    texture.file_format = "PNG"
    texture.save()
    bpy.data.images.remove(texture)


random = np.random.default_rng(4910)
grid_y, grid_x = np.mgrid[0:SIZE, 0:SIZE] / SIZE
height = np.zeros((SIZE, SIZE))
for index in range(630):
    centre_x, centre_y = random.random(2)
    radius_x = random.uniform(.003, .014)
    radius_y = radius_x * random.uniform(.55, 1.7)
    distance_x = np.minimum(abs(grid_x - centre_x), 1 - abs(grid_x - centre_x))
    distance_y = np.minimum(abs(grid_y - centre_y), 1 - abs(grid_y - centre_y))
    distance = (distance_x / radius_x) ** 2 + (distance_y / radius_y) ** 2
    height -= random.uniform(.3, 1) * np.exp(-distance * 1.8)
grain = random.normal(0, .015, height.shape)
shade = np.clip(.99 + height * .32 + grain, .35, 1)
pixels = np.ones((SIZE, SIZE, 4))
pixels[:, :, :3] = shade[:, :, None]
save_png("Sponge_BaseColor.png", pixels)
gradient_x = (np.roll(height, -1, axis=1) - np.roll(height, 1, axis=1)) * 1.3
gradient_y = (np.roll(height, -1, axis=0) - np.roll(height, 1, axis=0)) * 1.3
normal = np.stack((-gradient_x, -gradient_y, np.ones_like(height)), axis=2)
normal /= np.linalg.norm(normal, axis=2)[:, :, None]
pixels[:, :, :3] = normal * .5 + .5
save_png("Sponge_Normal.png", pixels)
pixels[:, :, :3] = 0
pixels[:, :, 3] = np.clip(.2 + height * .08, .06, .22)
save_png("Sponge_Mask.png", pixels)


def ribbon(name, material, piped):
    vertices = []
    sections, sides = (60, 12) if piped else (56, 12)
    for along in range(sections + 1):
        fraction = along / sections
        position_x = .1 * (fraction - .5)
        if piped:
            pulse = .5 - .5 * math.cos(fraction * math.tau * 5)
            radius_y = .0025 + .0035 * pulse
            radius_z = .0025 + .0035 * pulse
            centre_z = .006
        else:
            drip = (.5 - .5 * math.cos(fraction * math.tau * 3)) ** 3
            bottom = -.013 - .018 * drip
            centre_z = bottom / 2
            radius_z = -bottom / 2
            radius_y = .0035
        for around in range(sides):
            angle = around * math.tau / sides
            flute = 1 + .13 * math.cos(angle * 6) if piped else 1
            vertices.append((position_x, math.cos(angle) * radius_y * flute,
                             centre_z + math.sin(angle) * radius_z * flute))
    faces = []
    for along in range(sections):
        for around in range(sides):
            following = (around + 1) % sides
            faces.append((along * sides + around, along * sides + following,
                          (along + 1) * sides + following, (along + 1) * sides + around))
    faces.append(tuple(reversed(range(sides))))
    faces.append(tuple(sections * sides + around for around in range(sides)))
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(materials[material])
    return obj


def audit_mesh(obj):
    mesh = obj.data
    assert mesh.uv_layers.active is not None
    assert all(len(polygon.vertices) == 3 for polygon in mesh.polygons)
    editable = bmesh.new()
    editable.from_mesh(mesh)
    bmesh.ops.remove_doubles(editable, verts=list(editable.verts), dist=1e-8)
    assert all(edge.is_manifold for edge in editable.edges)
    assert editable.calc_volume(signed=True) > 0
    editable.free()
    mesh.calc_loop_triangles()
    assert all(triangle.area > 1e-13 for triangle in mesh.loop_triangles)
    return len(mesh.polygons)


bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
audit = []
for name, material, piped in (("GlazeDrip", "Glaze lilac", False), ("PipedCream", "Piped cream", True)):
    obj = ribbon(name, material, piped)
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    editable = bmesh.new()
    editable.from_mesh(obj.data)
    bmesh.ops.recalc_face_normals(editable, faces=list(editable.faces))
    bmesh.ops.triangulate(editable, faces=list(editable.faces))
    editable.to_mesh(obj.data)
    editable.free()
    for polygon in obj.data.polygons:
        polygon.use_smooth = True
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(island_margin=.02)
    bpy.ops.object.mode_set(mode="OBJECT")
    count = audit_mesh(obj)
    assert count <= 1500
    expected = np.array([tuple(vertex.co) for vertex in obj.data.vertices])
    bpy.ops.export_scene.fbx(filepath=str(OUTPUT / (name + ".fbx")), use_selection=True,
        object_types={"MESH"}, global_scale=1, apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z", axis_up="Y", bake_space_transform=True, mesh_smooth_type="OFF",
        add_leaf_bones=False, bake_anim=False)
    bpy.data.objects.remove(obj, do_unlink=True)
    bpy.ops.import_scene.fbx(filepath=str(OUTPUT / (name + ".fbx")))
    imported = list(bpy.context.selected_objects)
    assert len(imported) == 1
    obj = imported[0]
    assert audit_mesh(obj) == count
    actual = np.array([tuple(obj.matrix_world @ vertex.co) for vertex in obj.data.vertices])
    assert np.allclose(actual.min(axis=0), expected.min(axis=0), atol=2e-6)
    assert np.allclose(actual.max(axis=0), expected.max(axis=0), atol=2e-6)
    audit.append({"name": name, "triangles": count, "fbx_roundtrip": "PASS",
                  "unity_bounds_min": expected.min(axis=0)[[0, 2, 1]].tolist(),
                  "unity_bounds_max": expected.max(axis=0)[[0, 2, 1]].tolist()})
    bpy.data.objects.remove(obj, do_unlink=True)
(OUTPUT / "materials.json").write_text(json.dumps(manifest, indent=2))
(REPORT / "REDESIGN_VERIFICATION.json").write_text(json.dumps({"props": audit,
    "checks": "FBX roundtrip bounds, UV0, triangles, manifold, outward volume, nondegenerate faces",
    "unity_import": "Pending integration", "textures": "3 deterministic tileable 512px PNGs"}, indent=2))
print(json.dumps(audit, indent=2))
for index, entry in enumerate(audit):
    bpy.ops.import_scene.fbx(filepath=str(OUTPUT / (entry["name"] + ".fbx")))
    for obj in bpy.context.selected_objects:
        obj.location.z += .05
        obj.location.y += index * .06
bpy.ops.object.camera_add(location=(.15, -.3, .23))
camera = bpy.context.object
camera.rotation_euler = (Vector((0, .03, .04)) - camera.location).to_track_quat("-Z", "Y").to_euler()
camera.data.type = "ORTHO"
camera.data.ortho_scale = .22
scene = bpy.context.scene
scene.camera = camera
bpy.ops.object.light_add(type="AREA", location=(-.15, -.2, .4))
bpy.context.object.data.energy = 12
bpy.context.object.data.size = .3
scene.world.color = (.3, .3, .3)
scene.render.engine = "CYCLES"
scene.cycles.samples = 32
scene.render.resolution_x = 1200
scene.render.resolution_y = 800
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.filepath = str(REPORT / "REDESIGN_PROPS_PREVIEW.png")
bpy.ops.render.render(write_still=True)
