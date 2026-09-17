using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        public static void ApplyExpansionLevel(VenomCampaign game)
        {
            int number=game.Definition.Order;
            meshDirectory=$"Meshes/Level{number:00}";serial=0;
            Directory.CreateDirectory(Folder+"/"+meshDirectory);AssetDatabase.Refresh();
            var owner=game.GetComponent<VenomLevelController>();var root=owner.Rotation.transform;
            Remove(root,ArtRoot);Remove(owner.transform,"Day Lab studio");InitializeMaterials(owner);
            var clear=Glass("Clear optical glass",.025f,0,false);
            var coating=Glass("Ice satin coating",.24f,1,true);
            // A fully coated chamber can put several panes in front of COghe.
            // Keep its lavender grain readable without obscuring the action.
            var shellCoating=Glass("Expansion satin shell",.05f,.28f,true);
            var divider=Glass("Divider glass",.36f,0,false);
            divider.SetColor("_BaseColor",new Color(.91f,.56f,.20f,.36f));
            var tube=Glass("Transfer bore glass",.32f,0,false);
            tube.SetColor("_BaseColor",new Color(.16f,.55f,.67f,.32f));
            var steel=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Brushed cutting steel.mat")??alloy;
            var floorGlass=Glass("Expansion inspection floor",.48f,0,false);
            floorGlass.SetColor("_BaseColor",new Color(.77f,.83f,.84f,.48f));EditorUtility.SetDirty(floorGlass);
            var panes=new List<VenomSurfacePatch>();var floors=new List<Renderer>();
            var bounds=new Bounds(Vector3.zero,Vector3.zero);bool bounded=false;VenomSurfacePatch sphere=null;
            foreach(var patch in game.Surfaces)
            {
                var renderer=patch.GetComponent<MeshRenderer>();if(renderer==null)continue;
                var prop=patch.GetComponentInParent<VenomMovableProp>();
                bool moving=prop!=null;string name=patch.name.ToLowerInvariant();
                bool isFloor=!moving&&Vector3.Dot(patch.Normal,Vector3.up)>.98f&&patch.Size.x>.25f;
                bool blade=renderer.sharedMaterial!=null&&renderer.sharedMaterial.name=="Blade";
                bool dividerFace=!moving&&(name.Contains("divider")||name.Contains("partition")||name.Contains("low wall"));
                renderer.enabled=true;renderer.shadowCastingMode=ShadowCastingMode.Off;
                renderer.sharedMaterial=blade?steel:patch.Slippery||patch.HasSlipRegion?coating:moving?amber:dividerFace?divider:isFloor?floorGlass:clear;
                if(!moving&&patch.Slippery&&!patch.HasSlipRegion&&patch.Size.x>.4f&&patch.Size.y>.4f)
                    renderer.sharedMaterial=shellCoating;
                var oldOverlay=patch.transform.Find("Visible slippery coating");
                if(oldOverlay!=null)oldOverlay.GetComponent<Renderer>().enabled=false;
                // The dry approach must read as a separate climbable route beside the lavender slide.
                if(number==12&&(name=="gripping climb"||name=="launch platform"))renderer.sharedMaterial=amber;
                if(patch.SphereRadius>0){sphere=patch;renderer.sharedMaterial=Glass("Satin sphere",.075f,.7f,false);}
                if(renderer.sharedMaterial.shader.name=="COghe/Lab Glass")panes.Add(patch);
                if(isFloor)floors.Add(renderer);
                if(!moving)
                {
                    foreach(float x in new[]{-.5f,.5f})foreach(float y in new[]{-.5f,.5f})
                    {
                        Vector3 p=root.InverseTransformPoint(patch.transform.TransformPoint(new Vector3(patch.Size.x*x,patch.Size.y*y,0)));
                        if(!bounded){bounds=new Bounds(p,Vector3.zero);bounded=true;}else bounds.Encapsulate(p);
                    }
                }
            }
            var art=Child(root,ArtRoot);
            if(number==12)
            {
                // Render the exact continuous collision shape. Separate little
                // planes/boxes overlap on bends and hide the creature in stripes.
                foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())
                    if(renderer.name.StartsWith("Trough wall ")||renderer.name.StartsWith("Slippery trough "))renderer.enabled=false;
                var proxy=root.GetComponentInChildren<COgheSurfacePickProxy>();
                if(proxy!=null)
                {
                    var source=proxy.GetComponent<MeshCollider>().sharedMesh;
                    var floorIndices=new List<int>();var wallIndices=new List<int>();var indices=source.triangles;
                    for(int t=0;t<indices.Length/3;t++)
                    {
                        var dest=t%6<2?floorIndices:wallIndices;
                        for(int v=0;v<3;v++)dest.Add(indices[t*3+v]);
                    }
                    void TroughVisual(string name,List<int> triangles,Material material)
                    {
                        var mesh=new Mesh{name=name};mesh.vertices=source.vertices;mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
                        var go=Child(art,name);go.position=proxy.transform.position;go.rotation=proxy.transform.rotation;
                        go.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                        var renderer=go.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;
                    }
                    TroughVisual("Continuous lavender slide",floorIndices,Glass("Slide satin coating",.24f,1,false));
                    TroughVisual("Clear cyan slide walls",wallIndices,tube);
                }
            }
            if(sphere!=null)
            {
                float r=sphere.SphereRadius+.003f;
                Ring(art,"Sphere diagonal seam",sphere.transform.localPosition,new Vector3(1,1,1).normalized,r,.0012f,alloy);
                bounds=new Bounds(sphere.transform.localPosition,Vector3.one*r*2);
            }
            else if(bounded)
            {
                Vector3 min=bounds.min-Vector3.one*.006f,max=bounds.max+Vector3.one*.006f;
                foreach(float y in new[]{min.y,max.y})foreach(float z in new[]{min.z,max.z})
                    Box(art,"Outer rail X",new Vector3(bounds.center.x,y,z),new Vector3(max.x-min.x,.009f,.009f),.002f,alloy);
                foreach(float x in new[]{min.x,max.x})foreach(float z in new[]{min.z,max.z})
                    Box(art,"Outer rail Y",new Vector3(x,bounds.center.y,z),new Vector3(.009f,max.y-min.y,.009f),.002f,alloy);
                foreach(float x in new[]{min.x,max.x})foreach(float y in new[]{min.y,max.y})
                    Box(art,"Outer rail Z",new Vector3(x,y,bounds.center.z),new Vector3(.009f,.009f,max.z-min.z),.002f,alloy);
                foreach(float x in new[]{min.x,max.x})foreach(float y in new[]{min.y,max.y})foreach(float z in new[]{min.z,max.z})
                    Box(art,"Porcelain corner",new Vector3(x,y,z),Vector3.one*.022f,.005f,ivory);
            }
            foreach(var patch in game.Surfaces)
            {
                if(patch.HasSlipRegion)PatchOutline(art,patch,patch.SlipRegion,amber,.0013f);
                if(patch.RingGrip)Ring(art,"Grip island",patch.transform.TransformPoint(new Vector3(patch.HoleCentre.x,patch.HoleCentre.y,.001f)),patch.Normal,patch.GripRadius,.0008f,mint);
            }
            foreach(var network in root.GetComponentsInChildren<COgheTubeNetwork>())
                foreach(var renderer in network.GetComponentsInChildren<MeshRenderer>())
                    renderer.sharedMaterial=tube;
            foreach(var sensor in root.GetComponentsInChildren<COgheTissueSensor>())
            {
                string key=sensor.name.Substring(0,1);
                Label(sensor.transform,key,new Vector3(0,.007f,0),Quaternion.Euler(90,0,0),.019f,ink);
            }
            foreach(var plate in owner.Apparatus.GetComponentsInChildren<VenomPressurePlate>())
                if(plate.name.EndsWith(" B"))Label(plate.transform,"B",new Vector3(0,.009f,0),Quaternion.Euler(90,0,0),.019f,ink);
            foreach(var prop in game.Props)
            {
                if(!prop.Manipulable||number==16&&prop.ManipulationHandleOnly)continue;
                string[] words=prop.name.Split(' ');string key=words[0];
                if(key.Length!=1)key=words[words.Length-1];if(key.Length!=1)continue;
                // Props can use six separate contact faces instead of a root
                // box. Keep the identifier attached to their actual front face.
                var colliders=prop.GetComponentsInChildren<BoxCollider>();if(colliders.Length==0)continue;
                Bounds propBounds=new Bounds();bool first=true;
                foreach(var collider in colliders)
                    for(int corner=0;corner<8;corner++)
                    {
                        Vector3 offset=Vector3.Scale(collider.size*.5f,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1));
                        Vector3 point=prop.transform.InverseTransformPoint(collider.transform.TransformPoint(collider.center+offset));
                        if(first){propBounds=new Bounds(point,Vector3.zero);first=false;}else propBounds.Encapsulate(point);
                    }
                Label(prop.transform,key,new Vector3(propBounds.center.x,propBounds.center.y,propBounds.min.z-.002f),Quaternion.identity,.018f,ink);
            }
            foreach(var blade in root.GetComponentsInChildren<COgheGuillotine>())
                foreach(var renderer in blade.Rail.GetComponentsInChildren<MeshRenderer>())renderer.sharedMaterial=steel;
            if(sphere==null)
                Label(art,number.ToString("00"),bounds.min+new Vector3(.055f,bounds.size.y-.047f,-.008f),Quaternion.identity,.019f,ink);
            foreach(var rim in owner.Outlet.GetComponentsInChildren<LineRenderer>())rim.sharedMaterial=mint;
            if(number==13)BuildAccessSequenceArt(game,art);
            if(number==15)BuildPipeMazeArt(game,art);
            if(number==16)BuildAssemblyBridgeArt(game,art);
            if(number==19)BuildCooperationArt(game,art);
            var studio=Child(owner.transform,"Day Lab studio");
            float deskY=game.Definition.CanRotate?-Mathf.Max(.70f,bounds.extents.magnitude+.13f):bounds.min.y-.11f;
            Box(studio,"Laboratory bench",new Vector3(0,deskY-.015f,0),new Vector3(12,.03f,12),.004f,ivory);
            SoftShadow(studio,"Chamber shadow",new Vector3(.025f,deskY+.001f,.025f),Mathf.Max(.9f,bounds.size.x*1.25f),.13f,.50f);
            Lighting(owner,studio);
            var presentation=owner.GetComponent<COgheDayLabPresentation>()??owner.gameObject.AddComponent<COgheDayLabPresentation>();
            ConfigureWorldLabels(presentation);presentation.FadingFloors=floors.ToArray();presentation.GlassSurfaces=panes.ToArray();
            CombineByMaterial(art);CombineByMaterial(studio);presentation.FocusOccluders=art.GetComponentsInChildren<Renderer>();
            EditorUtility.SetDirty(presentation);EditorUtility.SetDirty(owner);
        }
    }
}
