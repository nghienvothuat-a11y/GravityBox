import json
import math
from pathlib import Path

import bpy
import bmesh
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "Assets/_Game/Venom/Art/Bakery"
OUTPUT.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)

PALETTE = {
    "Strawberry": ((0.91, 0.39, 0.46), 0.46),
    "Cream": ((1.0, 0.89, 0.69), 0.4),
    "Biscuit": ((0.73, 0.43, 0.19), 0.66),
    "Wafer": ((0.96, 0.70, 0.38), 0.57),
    "Chocolate": ((0.23, 0.075, 0.036), 0.3),
    "Cherry": ((0.68, 0.028, 0.055), 0.22),
    "Stem": ((0.26, 0.36, 0.10), 0.53),
    "Porcelain": ((0.96, 0.91, 0.81), 0.24),
}
MATERIALS = {}
for label, (color, roughness) in PALETTE.items():
    material = bpy.data.materials.new(label)
    material.diffuse_color = (*color, 1)
    material.use_nodes = True
    surface = material.node_tree.nodes.get("Principled BSDF")
    surface.inputs["Base Color"].default_value = (*color, 1)
    surface.inputs["Roughness"].default_value = roughness
    MATERIALS[label] = material


def finish(obj, label, material):
    obj.name = label
    obj.data.materials.append(MATERIALS[material])
    for polygon in obj.data.polygons:
        polygon.use_smooth = True
    return obj


def sphere(label, location, scale, material, segments=24, rings=12):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=location)
    obj = bpy.context.object
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish(obj, label, material)


def box(label, location, dimensions, radius, material):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location)
    obj = bpy.context.object
    obj.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bevel = obj.modifiers.new("Soft baked edges", "BEVEL")
    bevel.width = radius
    bevel.segments = 3
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    normals = obj.modifiers.new("Face normals", "WEIGHTED_NORMAL")
    normals.keep_sharp = True
    bpy.ops.object.modifier_apply(modifier=normals.name)
    return finish(obj, label, material)


def tube(label, points, radius, material, sides=6):
    vertices = []
    for index, point in enumerate(points):
        previous = Vector(points[max(0, index - 1)])
        following = Vector(points[min(len(points) - 1, index + 1)])
        tangent = (following - previous).normalized()
        reference = Vector((0, 0, 1)) if abs(tangent.z) < 0.95 else Vector((0, 1, 0))
        first = tangent.cross(reference).normalized()
        second = tangent.cross(first).normalized()
        for side in range(sides):
            angle = side * math.tau / sides
            vertices.append(Vector(point) + radius * (math.cos(angle) * first + math.sin(angle) * second))
    faces = []
    for index in range(len(points) - 1):
        for side in range(sides):
            following = (side + 1) % sides
            faces.append((index * sides + side, index * sides + following,
                          (index + 1) * sides + following, (index + 1) * sides + side))
    faces.append(tuple(reversed(range(sides))))
    faces.append(tuple((len(points) - 1) * sides + side for side in range(sides)))
    mesh = bpy.data.meshes.new(label)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(label, mesh)
    bpy.context.collection.objects.link(obj)
    return finish(obj, label, material)


def ring(label, height, radius, thickness, material, waves=0):
    points = []
    for index in range(65):
        angle = math.tau * index / 64
        offset = 0.008 * math.sin(angle * waves) if waves else 0
        points.append(((radius + offset) * math.cos(angle),
                       (radius + offset) * math.sin(angle), height + offset * 0.5))
    return tube(label, points, thickness, material)


def macaron():
    sphere("Lower shell", (0, 0, 0.13), (0.47, 0.47, 0.13), "Strawberry", 32, 12)
    sphere("Vanilla filling", (0, 0, 0.255), (0.446, 0.446, 0.085), "Cream", 32, 8)
    sphere("Upper shell", (0, 0, 0.365), (0.47, 0.47, 0.14), "Strawberry", 32, 12)
    ring("Lower ruffled foot", 0.203, 0.438, 0.035, "Strawberry", 23)
    ring("Upper ruffled foot", 0.308, 0.443, 0.035, "Strawberry", 19)


def lollipop():
    tube("Vanilla stick", [(0, 0, 0), (0, 0, 0.87)], 0.032, "Cream", 8)
    sphere("Candy disc", (0, 0, 1.03), (0.33, 0.085, 0.33), "Cream", 20, 10)
    for face in (-1, 1):
        points = []
        for index in range(41):
            fraction = index / 40
            angle = fraction * math.tau * 2.35
            radius = 0.018 + fraction * 0.275
            points.append((radius * math.cos(angle), face * (0.09 * math.sqrt(1 - (radius / 0.33) ** 2)),
                           1.03 + radius * math.sin(angle)))
        tube("Strawberry spiral", points, 0.024, "Strawberry", 6)


def wafer():
    for index in range(5):
        box("Wafer layer", (0, 0, 0.03 + index * 0.036), (1.0, 0.45, 0.027), 0.012,
            "Wafer" if index % 2 == 0 else "Chocolate")
    for column in range(8):
        for row in range(3):
            box("Raised wafer grid", (-0.427 + column * 0.122, -0.143 + row * 0.143, 0.196),
                (0.104, 0.12, 0.038), 0.018, "Wafer")


def cherry():
    obj = sphere("Heart cherry", (0, 0, 0.25), (0.26, 0.245, 0.25), "Cherry", 24, 14)
    for vertex in obj.data.vertices:
        normalized = vertex.co.z / 0.25
        if normalized > 0.48:
            vertex.co.z -= 0.048 * ((normalized - 0.48) / 0.52) ** 3
    points = []
    for index in range(13):
        fraction = index / 12
        points.append((0.12 * fraction * fraction, 0, 0.453 + 0.34 * fraction))
    tube("Curved stem", points, 0.014, "Stem", 6)
    leaf = sphere("Leaf", (0.16, 0, 0.72), (0.12, 0.045, 0.012), "Stem", 16, 8)
    leaf.rotation_euler[1] = -0.45


def lathe(label, profile, material, segments=32, oval=1, scallop=0, rectangular=False):
    vertices = []
    for radius, height in profile:
        for index in range(segments):
            angle = index * math.tau / segments
            wave = scallop * math.cos(angle * 12) * max(0, (radius - 0.40) / 0.09)
            cosine, sine = math.cos(angle), math.sin(angle)
            if rectangular:
                cosine = math.copysign(abs(cosine) ** 0.5, cosine)
                sine = math.copysign(abs(sine) ** 0.5, sine)
            vertices.append(((radius + wave) * cosine, (radius + wave) * sine * oval, height))
    faces = []
    for ring_index in range(len(profile) - 1):
        for index in range(segments):
            following = (index + 1) % segments
            faces.append((ring_index * segments + index, (ring_index + 1) * segments + index,
                          (ring_index + 1) * segments + following, ring_index * segments + following))
    mesh = bpy.data.meshes.new(label)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(label, mesh)
    bpy.context.collection.objects.link(obj)
    return finish(obj, label, material)


def plate():
    lathe("Continuous porcelain dish", [(0, 0.008), (0.411, 0.008), (0.436, 0.016),
          (0.450, 0.040), (0.455, 0.068), (0.468, 0.075), (0.483, 0.070),
          (0.483, 0.054), (0.458, 0.005), (0.425, 0), (0, 0)],
          "Porcelain", 64, 0.78 / 0.98, 0.003, True)


def pressure_pad():
    lathe("Biscuit base", [(0, 0), (0.046, 0), (0.05, 0.002), (0.049, 0.004), (0, 0.004)], "Biscuit")
    lathe("Strawberry filling", [(0, 0.003), (0.047, 0.003), (0.049, 0.005),
          (0.047, 0.007), (0, 0.007)], "Strawberry")
    lathe("Flat cookie cap", [(0, 0.006), (0.047, 0.006), (0.05, 0.009),
          (0.047, 0.0115), (0, 0.0115)], "Wafer")


def gummy():
    lathe("Candy status lamp", [(0, 0), (0.013, 0), (0.015, 0.003),
          (0.014, 0.012), (0.01, 0.016), (0, 0.018)], "Strawberry", 24)


def cream_drip():
    vertices = []
    for index in range(33):
        fraction = index / 32
        height = 0.012 - 0.009 * math.sin(fraction * math.tau * 2) ** 6
        vertices.extend(((fraction * 0.10 - 0.05, -0.002, 0.025),
                         (fraction * 0.10 - 0.05, -0.003, height),
                         (fraction * 0.10 - 0.05, 0.002, height),
                         (fraction * 0.10 - 0.05, 0.002, 0.025)))
    faces = []
    for index in range(32):
        for side in range(4):
            following = (side + 1) % 4
            faces.append((index * 4 + side, index * 4 + following,
                          (index + 1) * 4 + following, (index + 1) * 4 + side))
    faces.extend(((3, 2, 1, 0), (128, 129, 130, 131)))
    mesh = bpy.data.meshes.new("Repeatable icing")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new("Repeatable icing", mesh)
    bpy.context.collection.objects.link(obj)
    finish(obj, obj.name, "Cream")


def vector_json(vector):
    return {"x": round(vector.x, 6), "y": round(vector.z, 6), "z": round(vector.y, 6)}


def export_prop(label, build, scale):
    previous = set(bpy.context.scene.objects)
    build()
    objects = sorted(set(bpy.context.scene.objects) - previous, key=lambda obj: obj.name)
    for obj in objects:
        obj.location *= scale
        obj.scale *= scale
        bpy.ops.object.select_all(action="DESELECT")
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        editable = bmesh.new()
        editable.from_mesh(obj.data)
        bmesh.ops.remove_doubles(editable, verts=list(editable.verts), dist=0.0000001)
        bmesh.ops.recalc_face_normals(editable, faces=list(editable.faces))
        editable.to_mesh(obj.data)
        editable.free()
        obj.data.update()
    chunks = {}
    for obj in objects:
        bpy.ops.object.select_all(action="DESELECT")
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.smart_project(angle_limit=1.15, island_margin=0.025)
        bpy.ops.object.mode_set(mode="OBJECT")
        mesh = obj.data
        mesh.calc_loop_triangles()
        normals = mesh.corner_normals
        normal_matrix = obj.matrix_world.to_3x3().inverted().transposed()
        material = mesh.materials[0].name
        chunk = chunks.setdefault(material, {"material": material, "vertices": [], "normals": [], "uv": [], "triangles": []})
        for triangle in mesh.loop_triangles:
            first = len(chunk["vertices"])
            for loop_index in triangle.loops:
                position = obj.matrix_world @ mesh.vertices[mesh.loops[loop_index].vertex_index].co
                normal = (normal_matrix @ normals[loop_index].vector).normalized()
                coords = mesh.uv_layers.active.data[loop_index].uv
                chunk["vertices"].append(vector_json(position))
                chunk["normals"].append(vector_json(normal))
                chunk["uv"].append({"x": round(coords.x, 6), "y": round(coords.y, 6)})
            chunk["triangles"].extend((first, first + 2, first + 1))
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
        modifier = obj.modifiers.new("Export triangles", "TRIANGULATE")
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    bpy.ops.export_scene.fbx(filepath=str(OUTPUT / (label + ".fbx")), use_selection=True,
        object_types={"MESH"}, global_scale=1, apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z", axis_up="Y", bake_space_transform=True, use_mesh_modifiers=True,
        mesh_smooth_type="OFF", add_leaf_bones=False, bake_anim=False, path_mode="AUTO")
    return {"name": label, "chunks": list(chunks.values())}, objects


pack = {"materials": [{"name": label, "color": {"r": color[0], "g": color[1], "b": color[2], "a": 1},
                        "smoothness": 1 - roughness} for label, (color, roughness) in PALETTE.items()], "props": []}
groups = []
for label, build, scale in (("Cherry", cherry, 0.045 / 0.807), ("Plate", plate, 1),
                            ("PressurePad", pressure_pad, 1), ("CandyHandle", lollipop, 0.02 / 0.66),
                            ("GummyLamp", gummy, 1), ("CreamDrip", cream_drip, 1)):
    prop, objects = export_prop(label, build, scale)
    pack["props"].append(prop)
    groups.append(objects)
    count = sum(len(chunk["triangles"]) // 3 for chunk in prop["chunks"])
    print(label, count, "triangles")
    if count > 1500:
        raise ValueError(label + " exceeds the agreed 1500 triangle budget")
(OUTPUT / "materials.json").write_text(json.dumps({"materials": pack["materials"]}, indent=2))
(ROOT / "Tools/Bakery/mesh_audit.json").write_text(json.dumps({"props": [
    {"name": prop["name"], "triangles": sum(len(chunk["triangles"]) // 3 for chunk in prop["chunks"]),
     "bounds_min": {axis: min(vertex[axis] for chunk in prop["chunks"] for vertex in chunk["vertices"]) for axis in ("x", "y", "z")},
     "bounds_max": {axis: max(vertex[axis] for chunk in prop["chunks"] for vertex in chunk["vertices"]) for axis in ("x", "y", "z")}}
    for prop in pack["props"]]}, indent=2))

for index, objects in enumerate(groups):
    for obj in objects:
        if index == 0:
            obj.scale *= 8
        elif index > 1:
            obj.scale *= 10 if index == 2 else 14
        obj.location.x += (index - 2.5) * 1.05
box("Preview ground", (0, 0, -0.08), (200, 200, 0.1), 0.01, "Cream")
bpy.ops.object.camera_add(location=(3.6, -7, 4.6))
camera = bpy.context.object
camera.rotation_euler = (Vector((0, 0, 0.25)) - camera.location).to_track_quat("-Z", "Y").to_euler()
camera.data.type = "ORTHO"
camera.data.ortho_scale = 7
bpy.context.scene.camera = camera
bpy.ops.object.light_add(type="AREA", location=(-2, -3, 6))
bpy.context.object.data.energy = 450
bpy.context.object.data.shape = "DISK"
bpy.context.object.data.size = 5
scene = bpy.context.scene
scene.world.color = (0.25, 0.25, 0.25)
scene.render.engine = "CYCLES"
scene.cycles.samples = 24
scene.render.resolution_x = 1500
scene.render.resolution_y = 750
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.filepath = str(ROOT / "Tools/Bakery/PROP_PREVIEW.png")
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT / "Tools/Bakery/BakeryProps.blend"))
bpy.ops.render.render(write_still=True)
