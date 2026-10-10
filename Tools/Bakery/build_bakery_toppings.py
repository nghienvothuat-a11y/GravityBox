import json
import math
from pathlib import Path

import bmesh
import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "Assets/_Game/Venom/Art/Bakery"
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
manifest = json.loads((OUTPUT / "materials.json").read_text())
additions = [
    {"name": "MintLeaf", "color": {"r": .32, "g": .52, "b": .21, "a": 1}, "smoothness": .32},
    {"name": "Grape jelly", "color": {"r": .58, "g": .43, "b": .78, "a": 1}, "smoothness": .76},
]
for addition in additions:
    manifest["materials"] = [entry for entry in manifest["materials"] if entry["name"] != addition["name"]]
    manifest["materials"].append(addition)
materials = {}
for entry in manifest["materials"]:
    material = bpy.data.materials.new(entry["name"])
    color = entry["color"]
    material.diffuse_color = (color["r"], color["g"], color["b"], 1)
    material.use_nodes = True
    surface = material.node_tree.nodes.get("Principled BSDF")
    surface.inputs["Base Color"].default_value = material.diffuse_color
    surface.inputs["Roughness"].default_value = 1 - entry["smoothness"]
    materials[entry["name"]] = material


def mesh_object(name, vertices, faces, material):
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(materials[material])
    return obj


def chip():
    profile = [(0, 0), (.006, 0), (.0068, .0015), (.0055, .0038), (.003, .0068), (.0012, .009), (0, .010)]
    vertices = []
    segments = 16
    for radius, height in profile:
        for index in range(segments):
            angle = index * math.tau / segments
            vertices.append((radius * math.cos(angle) + .0015 * (height / .01) ** 2,
                             radius * math.sin(angle), height))
    faces = []
    for ring in range(len(profile) - 1):
        for index in range(segments):
            following = (index + 1) % segments
            faces.append((ring * segments + index, ring * segments + following,
                          (ring + 1) * segments + following, (ring + 1) * segments + index))
    return mesh_object("ChocolateChip", vertices, faces, "Chocolate")


def sprinkle():
    bpy.ops.mesh.primitive_uv_sphere_add(segments=12, ring_count=8, location=(0, 0, .0013))
    obj = bpy.context.object
    obj.name = "Sprinkle"
    obj.scale = (.0045, .0013, .0013)
    obj.data.materials.append(materials["Strawberry"])
    return obj


def leaf():
    vertices = []
    for layer in (-1, 1):
        for along in range(13):
            fraction = along / 12
            width = .006 * math.sin(math.pi * fraction) ** .9
            for across in range(5):
                side = across / 2 - 1
                arch = .0012 * math.sin(math.pi * fraction)
                height = .0006 + arch + layer * .00055 * math.sin(math.pi * fraction) * (1 - abs(side))
                vertices.append(((fraction - .5) * .027, side * width, height))
    faces = []
    for layer in range(2):
        offset = layer * 65
        for along in range(12):
            for across in range(4):
                start = offset + along * 5 + across
                faces.append((start, start + 1, start + 6, start + 5))
    for along in range(12):
        for across in (0, 4):
            start = along * 5 + across
            faces.append((start, start + 5, start + 70, start + 65))
    return mesh_object("MintLeaf", vertices, faces, "MintLeaf")


audit = []
for name, builder in (("Sprinkle", sprinkle), ("ChocolateChip", chip), ("MintLeaf", leaf)):
    bpy.ops.object.select_all(action="DESELECT")
    obj = builder()
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    editable = bmesh.new()
    editable.from_mesh(obj.data)
    bmesh.ops.remove_doubles(editable, verts=list(editable.verts), dist=1e-8)
    bmesh.ops.recalc_face_normals(editable, faces=list(editable.faces))
    bmesh.ops.triangulate(editable, faces=list(editable.faces))
    assert all(edge.is_manifold for edge in editable.edges), (name, "not closed")
    assert editable.calc_volume(signed=True) > 0, (name, "inverted")
    editable.to_mesh(obj.data)
    editable.free()
    for polygon in obj.data.polygons:
        polygon.use_smooth = True
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(island_margin=.02)
    bpy.ops.object.mode_set(mode="OBJECT")
    count = len(obj.data.polygons)
    audit.append({"name": name, "triangles": count})
    bpy.ops.export_scene.fbx(filepath=str(OUTPUT / (name + ".fbx")), use_selection=True,
        object_types={"MESH"}, global_scale=1, apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z", axis_up="Y", bake_space_transform=True, mesh_smooth_type="OFF",
        add_leaf_bones=False, bake_anim=False)
    bpy.data.objects.remove(obj, do_unlink=True)
assert sum(entry["triangles"] for entry in audit) < 3000
for entry in audit:
    bpy.ops.import_scene.fbx(filepath=str(OUTPUT / (entry["name"] + ".fbx")))
    imported = list(bpy.context.selected_objects)
    assert sum(len(obj.data.polygons) for obj in imported) == entry["triangles"]
    for obj in imported:
        assert obj.data.uv_layers.active is not None
        assert all(len(polygon.vertices) == 3 for polygon in obj.data.polygons)
        editable = bmesh.new()
        editable.from_mesh(obj.data)
        bmesh.ops.remove_doubles(editable, verts=list(editable.verts), dist=1e-8)
        assert all(edge.is_manifold for edge in editable.edges)
        assert editable.calc_volume(signed=True) > 0
        editable.free()
        bpy.data.objects.remove(obj, do_unlink=True)
    entry["fbx_roundtrip"] = "PASS"
(OUTPUT / "materials.json").write_text(json.dumps(manifest, indent=2))
(ROOT / "Tools/Bakery/TOPPINGS_VERIFICATION.json").write_text(json.dumps(audit, indent=2))
print(json.dumps(audit, indent=2))
for index, entry in enumerate(audit):
    bpy.ops.import_scene.fbx(filepath=str(OUTPUT / (entry["name"] + ".fbx")))
    for obj in bpy.context.selected_objects:
        obj.scale *= 20
        obj.location.x += (index - 1) * .65
bpy.ops.mesh.primitive_plane_add(size=200, location=(0, 0, -.002))
floor_material = materials["Cream"]
bpy.context.object.data.materials.append(floor_material)
bpy.ops.object.camera_add(location=(.8, -2, 1.8))
camera = bpy.context.object
camera.rotation_euler = (Vector((0, 0, .05)) - camera.location).to_track_quat("-Z", "Y").to_euler()
camera.data.type = "ORTHO"
camera.data.ortho_scale = 2.4
scene = bpy.context.scene
scene.camera = camera
bpy.ops.object.light_add(type="AREA", location=(-1, -2, 4))
bpy.context.object.data.energy = 250
bpy.context.object.data.size = 4
scene.world.color = (.3, .3, .3)
scene.render.engine = "CYCLES"
scene.cycles.samples = 24
scene.render.resolution_x = 1200
scene.render.resolution_y = 600
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.filepath = str(ROOT / "Tools/Bakery/TOPPINGS_PREVIEW.png")
bpy.ops.render.render(write_still=True)
