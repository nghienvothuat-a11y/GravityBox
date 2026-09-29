"""Bake a quiet laboratory background from editable Blender geometry; no runtime DOF."""
import bpy, math, sys
from mathutils import Vector
from pathlib import Path
root=Path(sys.argv[sys.argv.index('--')+1]);out=root/'Assets/_Game/Venom/Art/NewGraphic/Materials';out.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def material(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(*color,1);m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.55;return m
ivory=material('Warm laboratory housing',(.81,.79,.71));blue=material('Powder blue instrumentation',(.43,.54,.57));mint=material('Quiet mint display',(.61,.75,.71))
def box(name,p,s,m,bevel=.1):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=name;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(m)
 mod=o.modifiers.new('Soft casing','BEVEL');mod.width=bevel;mod.segments=4;o.modifiers.new('Surface normals','WEIGHTED_NORMAL');return o
def cylinder(name,p,r,d,m):
 bpy.ops.mesh.primitive_cylinder_add(vertices=40,radius=r,depth=d,location=p);o=bpy.context.object;o.name=name;o.data.materials.append(m);mod=o.modifiers.new('Soft edge','BEVEL');mod.width=.06;mod.segments=3;o.modifiers.new('Normals','WEIGHTED_NORMAL');return o
# Spare observation vessels on the left, an instrument station on the right.
box('Left lab shelf',(-2.35,.1,-.08),(2.0,.8,.16),ivory,.06)
for x,r,h in [(-2.8,.20,.9),(-2.25,.33,.62)]:
 cylinder('Sample vessel',(x,.1,h/2),r,h,blue);cylinder('Sample lid',(x,.1,h+.035),r+.035,.07,ivory)
box('Monitor foot',(2.35,0,.05),(1.2,.65,.14),ivory)
box('Monitor pedestal',(2.35,.10,.43),(.23,.24,.8),blue,.04)
box('Research monitor',(2.35,.13,1.10),(1.8,.23,1.15),ivory,.15)
box('Blue screen',(2.35,-.008,1.11),(1.5,.03,.85),blue,.1)
for i,w in enumerate([.72,1.0,.55]):box('Quiet waveform',(2.28,-.033,.91+i*.17),(w,.018,.035),mint,.008)
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True
scene.world.color=(.8,.8,.8)
bpy.ops.object.light_add(type='AREA',location=(-3,-4,6));bpy.context.object.data.energy=550;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=5
bpy.ops.object.camera_add(location=(0,-8,3.8));camera=bpy.context.object;camera.rotation_euler=(Vector((0,0,.65))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=7.4;scene.camera=camera
scene.render.resolution_x=768;scene.render.resolution_y=320;scene.render.resolution_percentage=100;scene.render.film_transparent=True
scene.view_settings.view_transform='Standard';scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA';scene.render.filepath=str(out/'LaboratoryBackdrop.png')
# Blur baked into the image using the compositor, not an image editor or phone effect.
scene.use_nodes=True
nodes=bpy.data.node_groups.new('Soft distant lab','CompositorNodeTree');nodes.interface.new_socket(name='Image',in_out='OUTPUT',socket_type='NodeSocketColor');scene.compositing_node_group=nodes
render=nodes.nodes.new('CompositorNodeRLayers');blur=nodes.nodes.new('CompositorNodeBlur');blur.inputs['Size'].default_value=(11,11)
output=nodes.nodes.new('NodeGroupOutput');nodes.links.new(render.outputs['Image'],blur.inputs['Image']);nodes.links.new(blur.outputs['Image'],output.inputs['Image'])
bpy.ops.wm.save_as_mainfile(filepath=str(root/'ArtSource/COghe/NewGraphic/Laboratory_Backdrop.blend'));bpy.ops.render.render(write_still=True)
