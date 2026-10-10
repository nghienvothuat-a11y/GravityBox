import json
import math
from pathlib import Path

import bmesh
import bpy


ROOT = Path(__file__).resolve().parents[2]
audit = json.loads((ROOT / "Tools/Bakery/mesh_audit.json").read_text())
results = []
for expected in audit["props"]:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.fbx(filepath=str(ROOT / "Assets/_Game/Venom/Art/Bakery" / (expected["name"] + ".fbx")))
    positions = []
    count = 0
    for obj in bpy.context.scene.objects:
        assert obj.type == "MESH", (expected["name"], "unexpected object type", obj.type)
        mesh = obj.data
        assert mesh.uv_layers.active is not None, (obj.name, "missing UV0")
        assert all(len(polygon.vertices) == 3 for polygon in mesh.polygons), (obj.name, "not triangulated")
        mesh.calc_loop_triangles()
        count += len(mesh.loop_triangles)
        positions.extend(obj.matrix_world @ vertex.co for vertex in mesh.vertices)
        editable = bmesh.new()
        editable.from_mesh(mesh)
        bmesh.ops.remove_doubles(editable, verts=list(editable.verts), dist=0.0000001)
        assert all(edge.is_manifold for edge in editable.edges), (obj.name, "open or nonmanifold edge")
        assert editable.calc_volume(signed=True) > 0, (obj.name, "inward volume")
        editable.free()
        assert all(triangle.area > 1e-13 for triangle in mesh.loop_triangles), (obj.name, "degenerate triangle")
        assert all(math.isfinite(value) for vertex in mesh.vertices for value in vertex.co)
    assert count == expected["triangles"] <= 1500, (expected["name"], count)
    for axis, component in (("x", 0), ("y", 2), ("z", 1)):
        minimum = min(point[component] for point in positions)
        maximum = max(point[component] for point in positions)
        assert abs(minimum - expected["bounds_min"][axis]) < 0.000002, (expected["name"], axis, minimum)
        assert abs(maximum - expected["bounds_max"][axis]) < 0.000002, (expected["name"], axis, maximum)
    results.append({"name": expected["name"], "triangles": count, "result": "PASS"})
report = {"source_base": "c7aa1d22", "blender": bpy.app.version_string,
          "checks": "FBX round-trip dimensions, triangles, UV0, finite vertices, closed manifold, outward volume, nondegenerate faces",
          "results": results, "unity_import": "Not tested here; integration owned by Claude"}
(ROOT / "Tools/Bakery/FBX_VERIFICATION.json").write_text(json.dumps(report, indent=2))
print(json.dumps(report, indent=2))
