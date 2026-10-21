import bpy, math, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'ArtSource/COghe/MysteryGift'
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
def mat(name, rgb, metallic, rough):
 m=bpy.data.materials.new(name); m.diffuse_color=(*rgb,1); m.use_nodes=True
 p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*rgb,1); p.inputs['Metallic'].default_value=metallic; p.inputs['Roughness'].default_value=rough
 return m
coral=mat('Gift_Coral',(0.8,.105,.17),.05,.27)
peach=mat('Gift_Peach',(.99,.3,.34),.03,.24)
gold=mat('Gift_Champagne',(.98,.62,.16),.28,.25)
def mesh(name, v,f, material):
 me=bpy.data.meshes.new(name); me.from_pydata(v,[],f); me.update(); o=bpy.data.objects.new(name,me); bpy.context.collection.objects.link(o); o.data.materials.append(material); return o
def rounded_ring(w,r,z):
 pts=[]
 for cx,cy,base in [(w-r,w-r,0),(-w+r,w-r,90),(-w+r,-w+r,180),(w-r,-w+r,270)]:
  for i in range(3):
   a=math.radians(base+i*90/2); pts.append((cx+r*math.cos(a),cy+r*math.sin(a),z))
 return pts
# Hollow vessel, real inner walls, open mouth and closed base.
rings=[rounded_ring(.024,.004,.001),rounded_ring(.026,.005,.004),rounded_ring(.026,.005,.040),rounded_ring(.0235,.0035,.040),rounded_ring(.0235,.0035,.004)]
v=sum(rings,[]); n=12; f=[]
for j in range(4):
 for i in range(n): f.append((j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i))
f += [tuple(reversed(range(n))),tuple(range(4*n,5*n))]
body=mesh('Gift_Body',v,f,coral)
def cube(name, loc, size, material, bevel):
 bpy.ops.mesh.primitive_cube_add(size=1, location=loc); o=bpy.context.object; o.name=name; o.dimensions=size; bpy.ops.object.transform_apply(location=False,rotation=False,scale=True); o.data.materials.append(material)
 mod=o.modifiers.new('Soft edges','BEVEL'); mod.width=bevel; mod.segments=1; bpy.context.view_layer.objects.active=o; bpy.ops.object.modifier_apply(modifier=mod.name)
 return o
lid=cube('Gift_Lid',(0,0,.043),(.057,.057,.009),peach,.003)
# Set the lid pivot at bottom centre.
bpy.context.scene.cursor.location=(0,0,.0385); bpy.context.view_layer.objects.active=lid; bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
# Ribbon strips have a rectangular cross-section, rounded longitudinal path and visible loop openings.
def strip(name, points, width, thick, material, width_axis=(0,1,0)):
 v=[]; axis=Vector(width_axis)
 for i,p in enumerate(points):
  tangent=Vector(points[min(i+1,len(points)-1)])-Vector(points[max(0,i-1)])
  normal=tangent.normalized().cross(axis).normalized()
  for a,b in [(-1,-1),(1,-1),(1,1),(-1,1)]: v.append(Vector(p)+axis*a*width/2+normal*b*thick/2)
 f=[]
 for i in range(len(points)-1):
  for j in [0,2]: f.append((4*i+j,4*i+(j+1)%4,4*(i+1)+(j+1)%4,4*(i+1)+j))
 return mesh(name,v,f,material)
parts=[]
for side in [-1,1]:
 pts=[]
 for i in range(19):
  t=2*math.pi*i/18
  pts.append((side*(.003+.011*(1-math.cos(t))), .001*math.sin(t), .053+.010*math.sin(t)+.005*(1-math.cos(t))))
 parts.append(strip('BowLoop',pts,.009,.0012,gold))
# Two graceful dangling ends, joined to the knot, raised above the cap then draping at front.
for side in [-1,1]:
 pts=[]
 for i in range(7):
  t=i/6; pts.append((side*(.003+.010*t),-.003-.029*t,.052-.003*t-.012*max(0,(t-.8)/.2)**2))
 parts.append(strip('BowTail',pts,.008,.0009,gold,(1,0,0)))
parts.append(cube('Knot',(0,0,.053),(.011,.011,.008),gold,.003))
# Wrapping bands stop at the lid seam so the lid can open cleanly.
for rotate in [False,True]:
 pts=[(0,-.0266,.004),(0,-.0266,.039),(0,-.026,.041),(0,.026,.041),(0,.0266,.039),(0,.0266,.004)]
 if rotate: pts=[(y,x,z) for x,y,z in pts]
 band=strip('BodyRibbon',pts,.009,.0007,gold,(0,1,0) if rotate else (1,0,0))
 # Keep upper bridge off the mouth: remove top-spanning faces, leaving independent side bands.
 # rebuild side strips instead of crossing the hollow opening.
 bpy.data.objects.remove(band,do_unlink=True)
 for side in [-1,1]:
  p=[(0,side*.0267,.004),(0,side*.0267,.038)]
  if rotate: p=[(y,x,z) for x,y,z in p]
  band=strip('BodyRibbon',p,.009,.0007,gold,(0,1,0) if rotate else (1,0,0)); band.parent=body
# Lid cross ribbons: laid along the rounded top with curved shoulder.
for rotate in [False,True]:
 pts=[(0,-.0287,.041),(0,-.0287,.044),(0,-.027,.0478),(0,-.024,.0482),(0,.024,.0482),(0,.027,.0478),(0,.0287,.044),(0,.0287,.041)]
 if rotate: pts=[(y,x,z) for x,y,z in pts]
 parts.append(strip('LidRibbon',pts,.009,.0007,gold,(0,1,0) if rotate else (1,0,0)))
# Join body ribbons into body, bow+ribbons into one lid child.
def join(objects,active):
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=active; bpy.ops.object.join(); return active
join([body]+list(body.children),body)
bow=join(parts,parts[-1]); bow.name='Gift_Bow'; bow.parent=lid; bow.matrix_parent_inverse=lid.matrix_world.inverted()
for o in [body,lid,bow]:
 for p in o.data.polygons:p.use_smooth=True
 mod=o.modifiers.new('Weighted normals','WEIGHTED_NORMAL'); mod.keep_sharp=True; bpy.context.view_layer.objects.active=o; bpy.ops.object.modifier_apply(modifier=mod.name)
# Export only asset. FBX axis conversion supplies Unity Y-up.
objects=[body,lid,bow]; bpy.ops.object.select_all(action='DESELECT')
for o in objects:o.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'COghe_Gift.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False,use_mesh_modifiers=True)
report={o.name:sum(len(p.vertices)-2 for p in o.data.polygons) for o in objects}
report['total_triangles']=sum(report.values()); assert report['total_triangles']<500,report
(OUT/'mesh_audit.json').write_text(json.dumps(report,indent=2))
# Preview uses studio lighting only, not a claim of Unity appearance.
cube('Preview ground',(0,0,-.004),(2,2,.006),mat('Preview ivory',(.65,.74,.74),0,.7),.001)
scene=bpy.context.scene; scene.render.engine='CYCLES'; scene.cycles.samples=48
scene.world.color=(.35,.35,.35)
for loc,power,size in [((.05,-.12,.19),.4,.15),((-.12,-.01,.09),.2,.12),((.06,.08,.15),.5,.10)]:
 bpy.ops.object.light_add(type='AREA',location=loc); l=bpy.context.object;l.data.energy=power;l.data.shape='DISK';l.data.size=size;l.rotation_euler=(Vector((0,0,.03))-l.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add(location=(.115,-.17,.12)); cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,.033))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=.13;scene.camera=cam
scene.view_settings.view_transform='AgX';scene.render.resolution_x=1000;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'COghe_Gift.blend'))
scene.render.filepath=str(OUT/'gift_closed.png');bpy.ops.render.render(write_still=True)
lid.location.z+=.035;lid.rotation_euler[1]=math.radians(-18)
cam.data.ortho_scale=.17;cam.rotation_euler=(Vector((0,0,.05))-cam.location).to_track_quat('-Z','Y').to_euler()
scene.render.filepath=str(OUT/'gift_open.png');bpy.ops.render.render(write_still=True)
print('GIFT_AUDIT',report)
