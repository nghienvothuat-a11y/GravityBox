"""Blender-authored mesh kit. Export Unity local coordinates explicitly to avoid FBX axis ambiguity.
FBX and .blend retain an editable library; meshpack is the deterministic Unity interchange.
Run: Blender -b --python ArtSource/COghe/NewGraphic/build_kit.py -- /absolute/repo
"""
import bpy, json, math, sys, hashlib
from pathlib import Path
from mathutils import Vector
root=Path(sys.argv[sys.argv.index('--')+1]); source=root/'ArtSource/COghe/NewGraphic'
layout=json.loads((source/'layout.json').read_text())
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
colors={'Grout':(.64,.64,.58,1),'Floor':(.94,.91,.83,1),'Ivory':(.91,.86,.75,1),'Blue':(.55,.72,.79,1),'Amber':(.96,.55,.13,1),'Lavender':(.57,.47,.78,1),'Metal':(.43,.58,.64,1),'Graphite':(.065,.09,.11,1),'Mint':(.29,.73,.56,1)}
mats={}
for name,col in colors.items():
 m=bpy.data.materials.new(name);m.diffuse_color=col;m.use_nodes=True
 p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=col;p.inputs['Roughness'].default_value=.32;p.inputs['Metallic'].default_value=.5 if name=='Metal' else .03
 mats[name]=m

def u(v):
 if isinstance(v,dict):return Vector((v['x'],-v['z'],v['y']))
 return Vector((v[0],-v[2],v[1]))
def out(v):return {'x':v.x,'y':v.z,'z':-v.y}
current=[]
def finish(obj,mat,bevel=0):
 obj.data.materials.append(mats[mat]);current.append(obj)
 if bevel:
  mod=obj.modifiers.new('Manufactured edge radius','BEVEL');mod.width=bevel;mod.segments=3
  mod=obj.modifiers.new('Weighted studio normals','WEIGHTED_NORMAL');mod.keep_sharp=True;mod.weight=50
 for p in obj.data.polygons:p.use_smooth=bool(bevel)
 return obj

def box(name,centre,size,mat,bevel):
 bpy.ops.mesh.primitive_cube_add(size=1,location=u(centre));o=bpy.context.object;o.name=name;o.dimensions=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 return finish(o,mat,min(bevel,min(size)*.4))
def disk(name,centre,radius,depth,mat,normal=(0,0,-1)):
 bpy.ops.mesh.primitive_cylinder_add(vertices=32,radius=radius,depth=depth,location=u(centre));o=bpy.context.object;o.name=name;o.rotation_mode='QUATERNION';o.rotation_quaternion=Vector((0,0,1)).rotation_difference(u(normal));return finish(o,mat,min(depth*.25,.001))
def raw(part):
 mesh=bpy.data.meshes.new(part['id']);vs=[u(v) for v in part['vertices']];ix=part['triangles'];faces=[(ix[i],ix[i+1],ix[i+2]) for i in range(0,len(ix),3)]
 mesh.from_pydata(vs,[],faces);mesh.update()
 layer=mesh.uv_layers.new(name='Panel UV')
 for poly in mesh.polygons:
  for li in poly.loop_indices:
   v=mesh.vertices[mesh.loops[li].vertex_index].co
   layer.data[li].uv=(v.x/max(.01,part['size']['x'])+.5,v.z/max(.01,part['size']['y'])+.5)
 o=bpy.data.objects.new(part['id'],mesh);bpy.context.collection.objects.link(o)
 return finish(o,part['material'],0)

def frame(name,width,height,border,depth,mat,z=0):
 # Four rounded corners with matching sample counts for each contour.
 def contour(w,h,r):
  points=[]
  for cx,cy,start in [(w/2-r,h/2-r,0),(-w/2+r,h/2-r,90),(-w/2+r,-h/2+r,180),(w/2-r,-h/2+r,270)]:
   for j in range(9):
    a=math.radians(start+j*90/8)
    points.append((cx+math.cos(a)*r,cy+math.sin(a)*r))
  return points
 # Inner contour covers only the outside 0.8mm edge of the collision plane.
 inner=contour(width+.0016,height+.0016,.003)
 outer=contour(width+border*2,height+border*2,border+.003)
 vs=[]
 for zz in [z+.009,z-depth]:
  for loop in [outer,inner]:vs.extend([u((x,y,zz)) for x,y in loop])
 n=len(inner);faces=[]
 for i in range(n):
  j=(i+1)%n
  faces.extend([(i,j,n+j,n+i),(2*n+i,3*n+i,3*n+j,2*n+j),(i,2*n+i,2*n+j,j),(n+i,n+j,3*n+j,3*n+i)])
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(vs,[],faces);mesh.update()
 o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o)
 return finish(o,mat,.003)

def construct(p):
 k=p['kind'];s=p['size'];s=(s['x'],s['y'],s['z']);c=p['centre'];c=(c['x'],c['y'],c['z']);mat=p['material']
 if k=='pane':
  if p['hole']:raw(p)
  else:box('Powder blue panel',(0,0,-.020),(s[0],s[1],.04),'Blue',.010)
  # Continuous, rounded porcelain surround sits outside the playable panel.
  # A single swept ring avoids the scaffold look of intersecting trim cubes.
  frame('Porcelain instrument surround',s[0],s[1],.038,.056,'Ivory')
  # The outer blue panel remains the secondary shell; no overlapping second frame.
  for a in [-1,1]:
   for b in [-1,1]:
    disk('Recessed fastener',(a*(s[0]/2+.017),b*(s[1]/2+.017),.007),.004,.0009,'Metal',normal=(0,0,1))
  if not p['hole']:
   for f in [-1/6,1/6]:
    box('Manufactured panel seam',(s[0]*f,0,.00015),(.0009,s[1]-.016,.00025),'Metal',0)
  box('Lower porcelain skirting',(0,-s[1]/2+.006,-.001),(max(.02,s[0]-.02),.012,.002),'Ivory',.0007)
 elif k=='exit':
  # A flush annulus on the inner face, never a collider or raised obstacle.
  radius=s[0];vs=[];faces=[]
  for i in range(65):
   a=i*math.tau/64
   for r in [radius+.0003,radius+.0063]:vs.append(u((math.cos(a)*r,math.sin(a)*r,-.002)))
  for i in range(64):
   j=i*2;faces.append((j,j+2,j+3,j+1))
  mesh=bpy.data.meshes.new('Flush mint aperture');mesh.from_pydata(vs,[],faces);mesh.update()
  o=bpy.data.objects.new('Flush mint aperture',mesh);bpy.context.collection.objects.link(o);finish(o,'Mint')
  for poly in mesh.polygons:poly.use_smooth=False
 elif k=='surface':
  raw(p)
  if mat=='Floor' and not p['hole']:
   for fraction in [-.25,0,.25]:box('Porcelain panel joint',(s[0]*fraction,0,.0008),(.0013,s[1]-.012,.0004),'Grout',0)
   for fraction in [-1/6,1/6]:box('Porcelain panel joint',(0,s[1]*fraction,.0008),(s[0]-.012,.0013,.0004),'Grout',0)
 elif k=='prop':
  box('Satin amber body',c,s,'Amber',.008)
  # Framed shutter face: a real metal inset, porcelain shoulder and fasteners.
  if s[1]>.05 and s[2]<.065:
   box('Inset shutter face',(c[0],c[1],c[2]-s[2]/2-.0007),(s[0]*.74,s[1]*.76,.001),'Ivory',.0003)
   for a in [-1,1]:
    disk('Shutter fastener',(c[0]+a*s[0]*.38,c[1]+s[1]*.36,c[2]-s[2]/2-.0015),.003,.001,'Metal')
  # Inset manufactured strip: on top, within the collision outline.
  if s[0]>.05 and s[2]>.025:
   box('Porcelain insert',(c[0],c[1]+s[1]/2+.0002,c[2]),(s[0]*.64,.0006,s[2]*.56),'Ivory',.0002)
 elif k=='bearing':
  box('Porcelain rail bearing',(c[0],c[1]-.003,c[2]),s,'Ivory',.005)
 elif k=='handle':
  box('Ceramic carriage',(0,-.005,.005),(.074,.012,.05),'Ivory',.005)
  box('Amber grip housing',(0,.012,.003),(.067,.04,.038),'Amber',.010)
  disk('Metal rim',(0,.014,-.018),.024,.0025,'Metal')
  disk('Inset thumb pad',(0,.014,-.020),.0205,.003,'Amber')
 elif k=='base':
  box('One piece ceramic chassis',c,s,'Ivory',.024)
  box('Recessed blue plinth',(c[0],c[1]-s[1]/2+.004,c[2]),(s[0]-.032,.012,s[2]-.032),'Blue',.005)
  for x in [-1,1]:
   for z in [-1,1]:box('Rubber foot',(x*.35,c[1]-.042,z*.25),(.055,.012,.055),'Graphite',.005)
 elif k=='openbase':
  for sign in [-1,1]:
   box('Open chassis side',(sign*.424,c[1],0),(.040,.075,.68),'Ivory',.016)
   box('Open chassis end',(0,c[1],sign*.324),(.84,.075,.04),'Ivory',.016)

pack={'levels':[]};library=[];cache={}
for level in layout['levels']:
 res={'number':level['number'],'parts':[]}
 for p in level['parts']:
  signature=hashlib.sha256(('fidelity5'+json.dumps({k:v for k,v in p.items() if k not in ['id','parent','anchor']},sort_keys=True)).encode()).hexdigest()[:16]
  if signature not in cache:
   current=[];construct(p);meshes=[]
   for material in colors:
    vertices=[];normals=[];uv=[];triangles=[]
    for o in current:
     if o.data.materials[0].name!=material:continue
     dep=bpy.context.evaluated_depsgraph_get();evaluated=o.evaluated_get(dep);mesh=evaluated.to_mesh();mesh.calc_loop_triangles()
     matrix=o.matrix_world;normalmatrix=matrix.to_3x3().inverted().transposed()
     for tri in mesh.loop_triangles:
      base=len(vertices)
      for li in tri.loops:
       loop=mesh.loops[li];v=matrix@mesh.vertices[loop.vertex_index].co
       vertices.append(out(v));normals.append(out((normalmatrix@mesh.corner_normals[li].vector).normalized()))
       uv.append({'x':v.x/max(.01,p['size']['x'])+.5,'y':v.z/max(.01,p['size']['y'])+.5} if p['kind'] in ['surface','pane'] else {'x':v.x/.8+.5,'y':-v.y/.6+.5})
      triangles.extend([base,base+1,base+2])
     evaluated.to_mesh_clear()
    if vertices:meshes.append({'material':material,'vertices':vertices,'normals':normals,'uv':uv,'triangles':triangles})
   cache[signature]=meshes
   col=bpy.data.collections.new('Kit_'+signature);bpy.context.scene.collection.children.link(col)
   index=len(library);offset=Vector(((index%8)*1.25,(index//8)*1.05,0))
   for o in current:
    for old in list(o.users_collection):old.objects.unlink(o)
    col.objects.link(o);o.location+=offset
   library.extend([col])
  res['parts'].append({'id':p['id'],'parent':p['parent'],'anchor':p['anchor'],'pane':p['pane'],'kind':p['kind'],'key':signature})
 pack['levels'].append(res)
pack['meshes']=[{'key':key,'chunks':value} for key,value in cache.items()]
(source/'meshpack.json').write_text(json.dumps(pack,separators=(',',':')))
# This is an editable modular library, not a re-authored puzzle layout.
bpy.context.scene.unit_settings.system='METRIC'
bpy.ops.wm.save_as_mainfile(filepath=str(source/'COghe_Lab_Kit.blend'))
bpy.ops.export_scene.fbx(filepath=str(source/'COghe_Lab_Kit.fbx'),use_selection=False,object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True,bake_anim=False)
(source/'manifest.json').write_text(json.dumps({'blender':bpy.app.version_string,'modules':len(cache),'levels':10,'trianglesUnique':sum(len(m['triangles'])//3 for a in cache.values() for m in a),'coordinateSystem':'meshpack: Unity local metres; Blender source: X,-Z,Y; winding preserved','interchange':'Blender evaluated geometry -> meshpack -> Unity Mesh assets. FBX supplied as portable library.'},indent=2))
print('COGHE_BLENDER_KIT_DONE',len(cache))
