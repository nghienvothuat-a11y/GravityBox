# Option 1 mesh fix (Mrk, 10/10/2026: "Tripo3D tạo model rồi dùng Claude sửa lưới"): Codex's clean_tripo_models.py adapted by
# Claude to the five PA1 references (OUTBOX/COGHE_PA1_REFERENCES_2026_10_10): decimate, cap holes, remove degenerate faces,
# rebake 1024 maps, bottom-centre pivot, FBX round-trip audit. Floor and plate keep 4000 triangles (largest on screen).
import hashlib
import json
import math
from pathlib import Path

import bmesh
import bpy
import numpy as np
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
SOURCES = ROOT.parents[1] / ".scratch/tripo/pa1"
OUTPUT = ROOT / "Assets/_Game/Venom/Art/Bakery/TripoPA1"
EVIDENCE = ROOT / "Tools/Bakery/TripoPA1"
ACCEPTED = {"floor": "PA1Floor", "rail": "PA1Rail", "goal": "PA1Goal", "table": "PA1Table", "plate": "PA1Plate"}
TARGET = {"PA1Floor": 4000, "PA1Plate": 4000}


def meshes():
    return [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]


def bounds():
    points = [obj.matrix_world @ vertex.co for obj in meshes() for vertex in obj.data.vertices]
    return (Vector([min(point[axis] for point in points) for axis in range(3)]),
            Vector([max(point[axis] for point in points) for axis in range(3)]))


def inspect():
    minimum, maximum = bounds()
    report = {"bounds_min_blender": list(minimum), "bounds_max_blender": list(maximum),
              "dimensions_blender": list(maximum - minimum), "meshes": [], "triangles": 0}
    for obj in meshes():
        mesh = obj.data
        mesh.calc_loop_triangles()
        editable = bmesh.new()
        editable.from_mesh(mesh)
        details = {"name": obj.name, "vertices": len(mesh.vertices),
                   "triangles": len(mesh.loop_triangles), "uv_layers": len(mesh.uv_layers),
                   "material_slots": len(obj.material_slots),
                   "boundary_edges": sum(edge.is_boundary for edge in editable.edges),
                   "nonmanifold_edges": sum(not edge.is_manifold for edge in editable.edges),
                   "degenerate_triangles": sum(face.area < 1e-14 for face in mesh.loop_triangles),
                   "finite_vertices": all(math.isfinite(value) for vertex in mesh.vertices for value in vertex.co),
                   "signed_volume": editable.calc_volume(signed=True)}
        editable.free()
        report["meshes"].append(details)
        report["triangles"] += details["triangles"]
    referenced = {node.image for obj in meshes() for slot in obj.material_slots
                  if slot.material and slot.material.node_tree for node in slot.material.node_tree.nodes
                  if node.type == "TEX_IMAGE" and node.image}
    report["images"] = [{"name": img.name, "size": list(img.size), "color_space": img.colorspace_settings.name}
                        for img in sorted(referenced, key=lambda image: image.name)]
    return report


def normalize():
    minimum, maximum = bounds()
    center = (minimum + maximum) / 2
    offset = Vector((center.x, center.y, minimum.z))
    scale = 0.1 / (maximum.x - minimum.x)
    for obj in meshes():
        transform = obj.matrix_world.copy()
        for vertex in obj.data.vertices:
            vertex.co = (transform @ vertex.co - offset) * scale
        obj.matrix_world.identity()
    return scale


def preview(path):
    minimum, maximum = bounds()
    center = (minimum + maximum) / 2
    extent = max(maximum - minimum)
    camera = bpy.data.objects.new("ReviewCamera", bpy.data.cameras.new("ReviewCamera"))
    bpy.context.scene.collection.objects.link(camera)
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = extent * 1.65
    camera.location = center + Vector((1, -1.2, 0.9)).normalized() * extent * 4
    camera.rotation_euler = (center - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.clip_start = 0.001
    bpy.context.scene.camera = camera
    light = bpy.data.objects.new("ReviewKey", bpy.data.lights.new("ReviewKey", "SUN"))
    bpy.context.scene.collection.objects.link(light)
    light.data.energy = 2
    light.data.angle = 0.15
    light.rotation_euler = (0.8, 0.2, 0.6)
    world = bpy.data.worlds.new("ReviewWorld")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (0.8, 0.75, 0.7, 1)
    world.node_tree.nodes["Background"].inputs[1].default_value = 0.8
    scene = bpy.context.scene
    scene.world = world
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 24
    scene.cycles.use_denoising = True
    scene.render.resolution_x = 512
    scene.render.resolution_y = 512
    scene.render.resolution_percentage = 100
    scene.render.filepath = str(path)
    bpy.ops.render.render(write_still=True)
    bpy.data.objects.remove(camera, do_unlink=True)
    bpy.data.objects.remove(light, do_unlink=True)


def textures(name, high, low):
    bpy.context.scene.collection.objects.link(high)
    bpy.ops.object.select_all(action="DESELECT")
    low.select_set(True)
    bpy.context.view_layer.objects.active = low
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=1.151917, island_margin=0.02)
    bpy.ops.object.mode_set(mode="OBJECT")
    material = bpy.data.materials.new(name)
    material.use_nodes = True
    low.data.materials.clear()
    low.data.materials.append(material)
    nodes = material.node_tree.nodes
    shader = nodes.get("Principled BSDF")
    scene = bpy.context.scene
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 16
    scene.render.bake.use_selected_to_active = True
    scene.render.bake.cage_extrusion = 0.002
    scene.render.bake.max_ray_distance = 0.004
    scene.render.bake.margin = 12
    scene.render.bake.use_pass_direct = False
    scene.render.bake.use_pass_indirect = False
    scene.render.bake.use_pass_color = True
    images = {}
    high.select_set(True)
    for role, bake_type in (("basecolor", "DIFFUSE"), ("normal", "NORMAL"), ("roughness", "ROUGHNESS"), ("metallic", "EMIT")):
        img = bpy.data.images.new(f"{name}_{role}", width=1024, height=1024, alpha=True)
        img.colorspace_settings.name = "sRGB" if role == "basecolor" else "Non-Color"
        node = nodes.new("ShaderNodeTexImage")
        node.image = img
        nodes.active = node
        high_material = high.data.materials[0]
        high_output = high_material.node_tree.nodes.get("Material Output")
        high_shader = high_material.node_tree.nodes.get("Principled BSDF")
        if role == "metallic":
            source_socket = high_shader.inputs["Metallic"].links[0].from_socket
            emission = high_material.node_tree.nodes.new("ShaderNodeEmission")
            high_material.node_tree.links.new(source_socket, emission.inputs["Color"])
            high_material.node_tree.links.new(emission.outputs[0], high_output.inputs["Surface"])
        bpy.ops.object.bake(type=bake_type)
        if role == "metallic":
            high_material.node_tree.links.new(high_shader.outputs[0], high_output.inputs["Surface"])
            high_material.node_tree.nodes.remove(emission)
        pixels = np.empty(1024 * 1024 * 4, dtype=np.float32)
        img.pixels.foreach_get(pixels)
        pixels = pixels.reshape((1024, 1024, 4))
        valid = pixels[:, :, 3] > 0.999
        for iteration in range(24):
            total = np.zeros((1024, 1024, 3), dtype=np.float32)
            weight = np.zeros((1024, 1024), dtype=np.float32)
            for axis, direction in ((0, 1), (0, -1), (1, 1), (1, -1)):
                neighbor_valid = np.roll(valid, direction, axis=axis)
                neighbor_color = np.roll(pixels[:, :, :3], direction, axis=axis)
                if axis == 0:
                    neighbor_valid[0 if direction == 1 else -1, :] = False
                else:
                    neighbor_valid[:, 0 if direction == 1 else -1] = False
                total += neighbor_color * neighbor_valid[:, :, None]
                weight += neighbor_valid
            fill = ~valid & (weight > 0)
            pixels[fill, :3] = total[fill] / weight[fill, None]
            valid |= fill
        pixels[:, :, 3] = 1
        img.pixels.foreach_set(pixels.ravel())
        if role == "normal":
            pixels = np.empty(1024 * 1024 * 4, dtype=np.float32)
            img.pixels.foreach_get(pixels)
            pixels = pixels.reshape((-1, 4))
            normals = pixels[:, :3] * 2 - 1
            normals /= np.maximum(np.linalg.norm(normals, axis=1, keepdims=True), 1e-8)
            pixels[:, :3] = normals * 0.5 + 0.5
            img.pixels.foreach_set(pixels.ravel())
        img.filepath_raw = str(OUTPUT / f"{name}_{role}.png")
        img.file_format = "PNG"
        img.save()
        images[role] = img
    for node in list(nodes):
        if node.type != "TEX_IMAGE":
            continue
        role = node.image.name.rsplit("_", 1)[-1]
        if role == "normal":
            normal = nodes.new("ShaderNodeNormalMap")
            material.node_tree.links.new(node.outputs["Color"], normal.inputs["Color"])
            material.node_tree.links.new(normal.outputs[0], shader.inputs["Normal"])
        else:
            socket = {"basecolor": "Base Color", "roughness": "Roughness", "metallic": "Metallic"}[role]
            material.node_tree.links.new(node.outputs["Color"], shader.inputs[socket])
    bpy.data.objects.remove(high, do_unlink=True)
    roughness = np.empty(1024 * 1024 * 4, dtype=np.float32)
    metallic = np.empty_like(roughness)
    images["roughness"].pixels.foreach_get(roughness)
    images["metallic"].pixels.foreach_get(metallic)
    mask_pixels = np.ones((1024 * 1024, 4), dtype=np.float32)
    mask_pixels[:, 0] = metallic.reshape((-1, 4))[:, 0]
    mask_pixels[:, 3] = 1 - roughness.reshape((-1, 4))[:, 0]
    mask = bpy.data.images.new(f"{name}_Mask", width=1024, height=1024, alpha=True)
    mask.colorspace_settings.name = "Non-Color"
    mask.pixels.foreach_set(mask_pixels.ravel())
    mask.filepath_raw = str(OUTPUT / f"{name}_Mask.png")
    mask.file_format = "PNG"
    mask.save()


def main():
    OUTPUT.mkdir(parents=True, exist_ok=False)
    EVIDENCE.mkdir(parents=True, exist_ok=False)
    report = {"blender": bpy.app.version_string, "models": [], "unity_import": "Claude integration, option 1 (Mrk 10/10/2026)"}
    for source_name in ACCEPTED:
        source = next((SOURCES / source_name).glob("tripo-out/*/model.fbx"))
        bpy.ops.wm.read_factory_settings(use_empty=True)
        bpy.ops.import_scene.fbx(filepath=str(source))
        task = json.loads(source.with_name("task.json").read_text())
        entry = {"source": str(source), "sha256": hashlib.sha256(source.read_bytes()).hexdigest(),
                 "task": task, "original": inspect()}
        report["models"].append(entry)
        if source_name not in ACCEPTED:
            entry["disposition"] = "Excluded from cleanup: wrong authored silhouette/content; retain as failed generation sample"
            continue
        name = ACCEPTED[source_name]
        entry["uniform_scale"] = normalize()
        preview(EVIDENCE / f"{name}_original.png")
        assert len(meshes()) == 1
        low = meshes()[0]
        high = low.copy()
        high.data = low.data.copy()
        for obj in meshes():
            bpy.context.view_layer.objects.active = obj
            obj.data.calc_loop_triangles()
            modifier = obj.modifiers.new("MobileReduction", "DECIMATE")
            modifier.ratio = TARGET.get(name, 2700) / len(obj.data.loop_triangles)
            modifier.use_collapse_triangulate = True
            bpy.ops.object.modifier_apply(modifier=modifier.name)
            editable = bmesh.new()
            editable.from_mesh(obj.data)
            bmesh.ops.holes_fill(editable, edges=[edge for edge in editable.edges if edge.is_boundary], sides=0)
            bmesh.ops.triangulate(editable, faces=list(editable.faces))
            bmesh.ops.dissolve_degenerate(editable, dist=0.000001, edges=list(editable.edges))
            bmesh.ops.triangulate(editable, faces=list(editable.faces))
            invalid = [face for face in editable.faces if face.calc_area() < 1e-14]
            if invalid:
                bmesh.ops.delete(editable, geom=invalid, context="FACES_ONLY")
            bmesh.ops.recalc_face_normals(editable, faces=list(editable.faces))
            editable.to_mesh(obj.data)
            editable.free()
            obj.name = name
            obj.data.name = name
            if not obj.data.uv_layers:
                obj.data.uv_layers.new(name="UVMap")
        minimum, maximum = bounds()
        offset = Vector(((minimum.x + maximum.x) / 2, (minimum.y + maximum.y) / 2, minimum.z))
        for obj in (low, high):
            for vertex in obj.data.vertices:
                vertex.co -= offset
        textures(name, high, low)
        entry["cleaned"] = inspect()
        assert 1500 <= entry["cleaned"]["triangles"] <= 4500
        assert all(mesh["uv_layers"] > 0 and mesh["finite_vertices"] and mesh["degenerate_triangles"] == 0
                   for mesh in entry["cleaned"]["meshes"])
        preview(EVIDENCE / f"{name}_cleaned.png")
        bpy.ops.object.select_all(action="DESELECT")
        for obj in meshes():
            obj.select_set(True)
        destination = OUTPUT / f"{name}.fbx"
        bpy.ops.export_scene.fbx(filepath=str(destination), use_selection=True, object_types={"MESH"},
                                 axis_forward="-Z", axis_up="Y", apply_unit_scale=True,
                                 use_mesh_modifiers=True, use_triangles=True, bake_anim=False,
                                 add_leaf_bones=False, path_mode="RELATIVE", embed_textures=False)
        bpy.ops.wm.read_factory_settings(use_empty=True)
        bpy.ops.import_scene.fbx(filepath=str(destination))
        entry["roundtrip"] = inspect()
        entry["roundtrip_triangle_delta"] = entry["roundtrip"]["triangles"] - entry["cleaned"]["triangles"]
        assert 1500 <= entry["roundtrip"]["triangles"] <= 4500
        assert all(mesh["uv_layers"] > 0 and mesh["finite_vertices"] and mesh["degenerate_triangles"] == 0
                   for mesh in entry["roundtrip"]["meshes"])
        assert max(abs(entry["roundtrip"]["bounds_min_blender"][axis] - entry["cleaned"]["bounds_min_blender"][axis])
                   for axis in range(3)) < 1e-6
        assert max(abs(entry["roundtrip"]["bounds_max_blender"][axis] - entry["cleaned"]["bounds_max_blender"][axis])
                   for axis in range(3)) < 1e-6
        assert all(max(img["size"]) <= 1024 and min(img["size"]) > 0 for img in entry["roundtrip"]["images"])
    (EVIDENCE / "audit.json").write_text(json.dumps(report, indent=2))
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
