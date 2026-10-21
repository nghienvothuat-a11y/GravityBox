import bpy,json,math
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[2]/'ArtSource/COghe/MysteryGift'
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(root/'COghe_Gift.fbx'))
report={};total=0
for o in bpy.context.scene.objects:
 if o.type!='MESH':continue
 o.data.calc_loop_triangles();n=len(o.data.loop_triangles);total+=n
 assert all(math.isfinite(c) for v in o.data.vertices for c in v.co)
 assert all(t.area>1e-12 for t in o.data.loop_triangles)
 pts=[o.matrix_world@Vector(p) for p in o.bound_box]
 report[o.name]={'triangles':n,'materials':[m.name for m in o.data.materials], 'world_min':[min(p[i] for p in pts) for i in range(3)],'world_max':[max(p[i] for p in pts) for i in range(3)],'parent':o.parent.name if o.parent else None}
assert total<500
assert set(report)=={'Gift_Body','Gift_Lid','Gift_Bow'}
assert bpy.data.objects['Gift_Bow'].parent==bpy.data.objects['Gift_Lid']
print('BODY_DIMENSIONS',tuple(bpy.data.objects['Gift_Body'].dimensions));print(json.dumps(report,indent=2))
assert .050 < bpy.data.objects['Gift_Body'].dimensions.x < .055
report['total_triangles']=total
(root/'fbx_roundtrip_audit.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
