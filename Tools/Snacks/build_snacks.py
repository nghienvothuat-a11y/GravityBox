"""Original procedural snack meshes. Blender background, no external assets."""
import bpy, math, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'ArtSource/COghe/Snacks'
DEST=ROOT/'Assets/_Game/Venom/Resources/COgheSnacks'
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
def material(label, hexcode):
    m=bpy.data.materials.new(f'Snack_{label}_{hexcode}')
    rgb=[int(hexcode[i:i+2],16)/255 for i in (0,2,4)]
    linear=[v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4 for v in rgb]
    m.diffuse_color=(*linear,1); m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*linear,1); p.inputs['Roughness'].default_value=.32
    return m
crust=material('Crust','E8B04B'); cream=material('Cream','FFF1D6'); red=material('Berry','ED526F'); green=material('Leaf','69BC79'); yellow=material('Lemon','FFD85C'); filling=material('Apple','C77739'); pink=material('Scoop','F7A1B4')
def mesh(name,verts,faces,mat):
    me=bpy.data.meshes.new(name); me.from_pydata(verts,[],faces); me.update()
    o=bpy.data.objects.new(name,me); bpy.context.collection.objects.link(o); me.materials.append(mat); return o
def lathe(name,rings,mat,n=10,loc=(0,0,0),scale=(1,1,1)):
    v=[(loc[0]+r*math.cos(i*2*math.pi/n)*scale[0],loc[1]+r*math.sin(i*2*math.pi/n)*scale[1],loc[2]+z*scale[2]) for r,z in rings for i in range(n)]
    f=[tuple(reversed(range(n))),tuple(range((len(rings)-1)*n,len(rings)*n))]
    for j in range(len(rings)-1):
        for i in range(n): f.append((j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i))
    return mesh(name,v,f,mat)
def box(name,loc,size,mat,angle=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc); o=bpy.context.object; o.name=name; o.dimensions=size; o.rotation_euler.z=angle
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True); o.data.materials.append(mat); return o
def wedge(name,z0,z1,mat,scale=1):
    # Triangular slice with rounded back, tip facing camera (-Y).
    p=[(0,-.018),(.014,.007),(.010,.013),(0,.016),(-.010,.013),(-.014,.007)]
    v=[(x*scale,y*scale,z) for z in (z0,z1) for x,y in p]; n=len(p)
    f=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    return mesh(name,v,f,mat)
def leaf(loc,length=.010,width=.004,angle=0):
    x,y,z=loc
    pts=[(0,0,0),(width,.45*length,.001),(0,length,0),(-width,.45*length,.001),(0,.45*length,.002)]
    v=[(x+a*math.cos(angle)-b*math.sin(angle),y+a*math.sin(angle)+b*math.cos(angle),z+c) for a,b,c in pts]
    return mesh('Leaf',v,[(0,1,4),(1,2,4),(2,3,4),(3,0,4),(3,2,1,0)],green)
def join(name,parts):
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts:o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]; bpy.ops.object.join(); o=parts[0]; o.name=name
    bpy.context.scene.cursor.location=(0,0,0); bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    bounds=[(min(v.co[i] for v in o.data.vertices),max(v.co[i] for v in o.data.vertices)) for i in range(3)]
    offset=Vector(((bounds[0][0]+bounds[0][1])/2,(bounds[1][0]+bounds[1][1])/2,bounds[2][0]))
    for v in o.data.vertices:v.co-=offset
    # Recalculate connected component normals consistently, retain deliberate facets.
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT'); bpy.ops.mesh.normals_make_consistent(inside=False); bpy.ops.object.mode_set(mode='OBJECT')
    return o
assets=[]
# Apple pie: honey crust, caramel filling, crossed pastry strips and apple-red garnish.
p=[wedge('Base',0,.003,crust),wedge('Apple filling',.003,.010,filling,.96),wedge('Top',.010,.012,crust)]
for y,w in [(-.006,.010),(.001,.019),(.008,.022)]:p.append(box('Pastry stripe',(0,y,.013),(w,.0024,.002),cream))
p.append(box('Pastry centre',(0,.001,.014),(.0024,.026,.002),crust))
p.append(lathe('Apple garnish',[(.002,0),(.004,.0025),(.003,.005),(.0015,.006)],red,8,loc=(.004,.007,.014)))
p.append(leaf((.004,.007,.020),.007,.0025,-.6))
assets.append(join('SNACK_APPLE_PIE',p))
# Chilli candy: red wrapped bonbon, gold waist and flared green wrappers.
p=[]
body=lathe('Candy',[(.002,-.011),(.006,-.008),(.008,-.003),(.008,.003),(.006,.008),(.002,.011)],red,8)
body.rotation_euler.y=math.pi/2; body.location.z=.008;p.append(body)
for sign in [-1,1]:
    o=lathe('Twist wrapper',[(.002,.010),(.005,.014),(.0045,.017)],green,6)
    o.rotation_euler.y=sign*math.pi/2;o.location.z=.008;p.append(o)
o=lathe('Candy gold band',[(.0081,-.0016),(.0081,.0016)],yellow,8);o.rotation_euler.y=math.pi/2;o.location.z=.008;p.append(o)
assets.append(join('SNACK_CHILLI_CANDY',p))
# Ice cream: broad pink scoop on a golden tapered cone, cream collar.
p=[lathe('Cone',[(.001,0),(.010,.022)],crust,10),lathe('Cone rim',[(.010,.020),(.011,.021),(.011,.023)],cream,10)]
p.append(lathe('Strawberry scoop',[(.006,.021),(.013,.024),(.015,.029),(.012,.034),(.006,.037),(.001,.038)],pink,10))
assets.append(join('SNACK_ICE_CREAM',p))
# Lemon drop: bright flattened lozenge with pale bevel and readable leaf.
p=[lathe('Lemon sweet',[(.009,0),(.014,.003),(.016,.007),(.014,.011),(.009,.014)],yellow,10,scale=(1,.70,1))]
p.append(lathe('Candy face',[(.009,.014),(.007,.015)],cream,10,scale=(1,.70,1)))
p.append(leaf((.007,.001,.014),.012,.004,-.7))
assets.append(join('SNACK_LEMON_DROP',p))
# Strawberry cake: visible sponge/cream/jam layers and a sculpted strawberry.
p=[wedge('Sponge base',0,.005,crust),wedge('Cream layer',.005,.009,cream),wedge('Berry jam',.009,.012,red),wedge('Sponge top',.012,.017,crust),wedge('Icing',.017,.020,cream)]
p.append(lathe('Strawberry',[(.001,0),(.004,.003),(.006,.007),(.005,.010),(.002,.012)],red,8,loc=(0,.004,.020)))
for a in [0,2.094,4.188]:p.append(leaf((0,.004,.031),.006,.002,a))
assets.append(join('SNACK_STRAWBERRY_CAKE',p))
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
report={}
for o in assets:
    bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
    o.data.calc_loop_triangles();tris=len(o.data.loop_triangles)
    assert tris<300,(o.name,tris)
    assert min(v.co.z for v in o.data.vertices)>=-1e-7
    assert max(v.co.z for v in o.data.vertices)<=.040001
    assert all(t.area>1e-12 for t in o.data.loop_triangles)
    report[o.name]={'triangles':tris,'dimensions_m':list(o.dimensions),'materials':[m.name for m in o.data.materials]}
    bpy.ops.export_scene.fbx(filepath=str(DEST/(o.name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False,use_mesh_modifiers=True)
(OUT/'mesh_audit.json').write_text(json.dumps(report,indent=2))
# Reimport exported bytes, checking topology, dimensions, pivots and material names.
for o in assets:o.hide_set(True);o.hide_render=True
roundtrip={}
for name,expected in report.items():
    bpy.ops.object.select_all(action='DESELECT');bpy.ops.import_scene.fbx(filepath=str(DEST/(name+'.fbx')))
    objs=list(bpy.context.selected_objects); meshes=[o for o in objs if o.type=='MESH']; assert len(meshes)==1
    o=meshes[0];o.data.calc_loop_triangles(); assert len(o.data.loop_triangles)==expected['triangles']
    assert o.matrix_world.translation.length<1e-7
    coords=[o.matrix_world@v.co for v in o.data.vertices]
    dims=[max(v[i] for v in coords)-min(v[i] for v in coords) for i in range(3)]
    assert all(abs(a-b)<1e-6 for a,b in zip(dims,expected['dimensions_m']))
    assert min(v.z for v in coords)>-1e-6
    assert all(t.area>1e-12 for t in o.data.loop_triangles)
    assert all(all(math.isfinite(c) for c in v) for v in coords)
    names=[m.name.rsplit('.',1)[0] if m.name[-3:].isdigit() else m.name for m in o.data.materials]
    assert names==expected['materials'],names
    roundtrip[name]={'triangles':len(o.data.loop_triangles),'dimensions_m':dims,'pivot_at_origin':True,'finite_nondegenerate':True,'material_names_preserved':True}
    for item in objs:bpy.data.objects.remove(item,do_unlink=True)
(OUT/'fbx_roundtrip_audit.json').write_text(json.dumps(roundtrip,indent=2))
# One studio render per snack at the same scale; no preview objects exported.
groundmat=material('Preview','CCDCD8');box('Preview ground',(0,0,-.003),(2,2,.005),groundmat)
scene.render.engine='CYCLES';scene.cycles.samples=32;scene.world.color=(.35,.35,.35)
for loc,power,size in [((.05,-.08,.13),.18,.10),((-.06,-.02,.08),.10,.08),((.03,.08,.12),.20,.07)]:
    bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.data.energy=power;o.data.size=size;o.rotation_euler=(Vector((0,0,.015))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add(location=(.065,-.09,.075));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,.017))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=.060;scene.camera=cam
scene.view_settings.view_transform='AgX';scene.render.resolution_x=512;scene.render.resolution_y=512;scene.render.resolution_percentage=100
for o in assets:
    o.hide_set(False);o.hide_render=False
    scene.render.filepath=str(OUT/(o.name+'.png'));bpy.ops.render.render(write_still=True)
    o.hide_render=True
for i,o in enumerate(assets):o.hide_set(False);o.hide_render=False;o.location.x=(i-2)*.055
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'COghe_Snacks.blend'))
print('SNACKS_VALIDATED',json.dumps(report))
