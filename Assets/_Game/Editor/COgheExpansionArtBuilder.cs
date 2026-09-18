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
            int content=number;
            string id=game.Definition.Id??string.Empty;int split=id.LastIndexOf('.');
            if(split>=0)int.TryParse(id.Substring(split+1),out content);
            if(content<=0)content=number;
            meshDirectory=content==number?$"Meshes/Level{number:00}":$"Meshes/Campaign30/Content{content:00}Slot{number:00}";serial=0;
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
                if(content==12&&(name=="gripping climb"||name=="launch platform"))renderer.sharedMaterial=amber;
                if(patch.SphereRadius>0){sphere=patch;renderer.sharedMaterial=Glass("Satin sphere",.075f,.7f,false);}
                if(renderer.sharedMaterial.shader.name=="COghe/Lab Glass")panes.Add(patch);
                if(isFloor&&!(content==22&&(name=="climb deck"||name=="receiving body deck"))&&!(content==26&&(name.Contains("raised bank")||name=="docked seamless bridge deck")))floors.Add(renderer);
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
            Remove(root,"Learning slide presentation");
            if(content==21)BuildLearningSlideArt(game,root);
            if(content==22)BuildCatchAndFlowArt(game,root);
            if(content>=24&&content<=26)BuildReviewedMechanismArt(game,root,content);
            if(content==12)
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
            else if(bounded&&content!=22)
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
                if(!prop.Manipulable||content==16&&prop.ManipulationHandleOnly)continue;
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
                if(content==24)Label(prop.transform,key,new Vector3(-.021f,.030f,0),Quaternion.Euler(0,90,0),.023f,ink);
                else if(content==20&&key=="H")Label(prop.transform,key,new Vector3(propBounds.min.x-.002f,0,0),Quaternion.Euler(0,90,0),.026f,ink);
                else Label(prop.transform,key,new Vector3(propBounds.center.x,propBounds.center.y,propBounds.min.z-.002f),Quaternion.identity,content==20?(key=="G"?.016f:.020f):content==25?.023f:.018f,ink);
            }
            foreach(var blade in root.GetComponentsInChildren<COgheGuillotine>())
                foreach(var renderer in blade.Rail.GetComponentsInChildren<MeshRenderer>())renderer.sharedMaterial=steel;
            if(sphere==null)
                Label(art,number.ToString("00"),bounds.min+new Vector3(.055f,bounds.size.y-.047f,-.008f),Quaternion.identity,.019f,ink);
            COgheDayLabPresentation.ConfigureExitOutline(owner);
            if(content==13)BuildAccessSequenceArt(game,art);
            if(content==15)BuildPipeMazeArt(game,art);
            if(content==16)BuildAssemblyBridgeArt(game,art);
            if(content==19)BuildCooperationArt(game,art);
            if(content==20)BuildBossReadabilityArt(game,art);
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

        private static void BuildReviewedMechanismArt(VenomCampaign game,Transform root,int content)
        {
            var art=Child(root,"Reviewed mechanism presentation");
            var lavender=Lit("Solid satin slide",new Color(.43f,.37f,.76f),.08f,.57f);
            foreach(var patch in game.Surfaces)
            {
                var renderer=patch.GetComponent<MeshRenderer>();string n=patch.name;
                if(n.Contains("raised bank"))renderer.sharedMaterial=Vector3.Dot(patch.Normal,Vector3.up)>.9f?mint:ivory;
                if(n.StartsWith("Handle alcove"))renderer.sharedMaterial=ivory;
                if(n=="Docked seamless bridge deck")renderer.sharedMaterial=amber;
                if(content==26&&n=="Laboratory floor")
                {
                    // A colored coating only over the actual slippery rectangle.
                    renderer.sharedMaterial=clearFloorMaterial();
                    Box(art,"Lavender recovery basin",new Vector3(.0075f,-.299f,0),new Vector3(.275f,.001f,.60f),.0002f,lavender);
                }
            }
            foreach(var renderer in game.GetComponent<VenomLevelController>().Apparatus.GetComponentsInChildren<MeshRenderer>())
            {
                string n=renderer.name;
                if(n.Contains("fixed guide")||n.Contains("rail stop")||n.Contains("handle")&&!n.Contains("surface")||n.Contains("pocket lip")||n.Contains("pocket end"))renderer.sharedMaterial=alloy;
                if(n=="Cover receiving pocket back")renderer.sharedMaterial=graphite;
                if(n=="Visible cable drum")renderer.sharedMaterial=amber;
            }
            if(content==26)Ring(art,"Aligned partition bore",new Vector3(.142f,-.10f,.08f),Vector3.left,.075f,.002f,mint);
            if(content==24)
            {
                // Frame the receiver as part of the wall, leaving its mouth open.
                Label(art,"←",new Vector3(.257f,-.18f,.06f),Quaternion.Euler(0,90,0),.025f,ivory);
            }
            if(content==25)
            {
                var sequence=root.GetComponentInChildren<COgheSequentialWinch>();
                sequence.GetComponent<COgheWinchPresentation>().Material=alloy;
                var old=root.Find("Winch cable");if(old!=null)Object.DestroyImmediate(old.gameObject);
                Ring(art,"Drum flange",sequence.Drum.localPosition+Vector3.back*.015f,Vector3.back,.029f,.003f,alloy);
            }
            CombineByMaterial(art);
        }

        private static Material clearFloorMaterial() => AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Expansion inspection floor.mat");

        private static void BuildCatchAndFlowArt(VenomCampaign game,Transform root)
        {
            Remove(root,"Catch and flow presentation");
            var art=Child(root,"Catch and flow presentation");
            var lavender=Lit("Solid satin slide",new Color(.43f,.37f,.76f),.08f,.57f);
            var tray=Lit("Gripping tray mint",new Color(.48f,.72f,.65f),.08f,.38f);
            var pipe=Glass("Transfer bore glass",.32f,0,false);
            pipe.SetColor("_BaseColor",new Color(.16f,.55f,.67f,.32f));EditorUtility.SetDirty(pipe);
            foreach(var patch in game.Surfaces)
            {
                var renderer=patch.GetComponent<MeshRenderer>();
                switch(patch.name)
                {
                    case "Climb deck": case "Recovery climb": case "Recovery climb side": renderer.sharedMaterial=amber;break;
                    case "Short slippery departure": renderer.sharedMaterial=lavender;break;
                    case "Receiving body deck": case "Receiving recovery climb": renderer.sharedMaterial=tray;break;
                }
            }
            // Two distinct cages and separated floors make the transfer clear.
            foreach(float x in new[]{-.36f,.36f})
            {
                float min=x-.226f,max=x+.226f;
                foreach(float y in new[]{-.226f,.226f})foreach(float z in new[]{-.226f,.226f})
                    Box(art,"Chamber horizontal frame",new Vector3(x,y,z),new Vector3(.452f,.007f,.007f),.002f,alloy);
                foreach(float end in new[]{min,max})foreach(float z in new[]{-.226f,.226f})
                    Box(art,"Chamber upright",new Vector3(end,0,z),new Vector3(.007f,.452f,.007f),.002f,alloy);
                foreach(float end in new[]{min,max})foreach(float y in new[]{-.226f,.226f})
                    Box(art,"Chamber side frame",new Vector3(end,y,0),new Vector3(.007f,.007f,.452f),.002f,alloy);
                foreach(float end in new[]{min,max})foreach(float y in new[]{-.226f,.226f})foreach(float z in new[]{-.226f,.226f})
                    Box(art,"Chamber corner",new Vector3(end,y,z),Vector3.one*.017f,.004f,ivory);
            }
            // The coloured annulus is exactly flush with the real gripping wall.
            var entry=game.Tube.Entrance;
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<=64;i++)
            {
                float a=i*Mathf.PI*2/64;
                foreach(float r in new[]{entry.HoleRadius,entry.GripRadius})
                    vertices.Add(root.InverseTransformPoint(entry.transform.TransformPoint(new Vector3(
                        entry.HoleCentre.x+Mathf.Cos(a)*r,entry.HoleCentre.y+Mathf.Sin(a)*r,.0007f))));
                if(i<64){int k=i*2;triangles.AddRange(new[]{k,k+1,k+3,k,k+3,k+2});}
            }
            var mesh=new Mesh{name="Flush physical catch annulus"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var ring=Child(art,"Amber grip annulus");ring.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            ring.gameObject.AddComponent<MeshRenderer>().sharedMaterial=amber;
            foreach(var renderer in game.Tube.GetComponentsInChildren<MeshRenderer>())renderer.sharedMaterial=pipe;
            foreach(float along in new[]{.006f,game.Tube.Length-.006f})
            {
                Vector3 p=game.Tube.transform.TransformPoint(Vector3.forward*along);
                Ring(art,"Porcelain pipe collar",p,game.Tube.transform.forward,game.Tube.Radius+.004f,.006f,ivory);
                Ring(art,"Cyan coupling",p+game.Tube.transform.forward*.007f,game.Tube.transform.forward,game.Tube.Radius+.001f,.002f,alloy);
            }
            // Porcelain edges explain the deck thickness, while the open gap
            // below the purple departure remains unobstructed.
            Box(art,"Deck front edge",new Vector3(-.3925f,.135f,-.183f),new Vector3(.375f,.01f,.006f),.002f,ivory);
            Box(art,"Receiving deck front edge",new Vector3(.365f,-.094f,-.193f),new Vector3(.43f,.01f,.006f),.002f,ivory);
            foreach(float side in new[]{-1f,1f})
                Box(art,"Fall guide edge",new Vector3(-.267f,.025f,-.08f+side*.105f),new Vector3(.004f,.20f,.004f),.001f,alloy);
            CombineByMaterial(art);
        }

        private static void BuildLearningSlideArt(VenomCampaign game,Transform root)
        {
            // Functional equipment remains visible in creature-follow mode.
            // All render geometry follows the real collider; no extra collision.
            var art=Child(root,"Learning slide presentation");
            var lavender=Lit("Solid satin slide",new Color(.43f,.37f,.76f),.08f,.57f);
            var grain=SaveAsset("Slide satin grain.asset",()=>new Texture2D(64,64,TextureFormat.RGBA32,true));
            var pixels=new Color[64*64];
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
            {
                float value=.94f+.06f*((x*17+y*7+x*y*3)%31)/30f;
                pixels[y*64+x]=new Color(value,value,value,1);
            }
            grain.SetPixels(pixels);grain.wrapMode=TextureWrapMode.Repeat;grain.Apply(true);EditorUtility.SetDirty(grain);
            lavender.SetTexture("_BaseMap",grain);EditorUtility.SetDirty(lavender);
            var tray=Lit("Gripping tray mint",new Color(.48f,.72f,.65f),.08f,.38f);
            foreach(var patch in game.Surfaces)
            {
                var renderer=patch.GetComponent<MeshRenderer>();
                if(patch.name.StartsWith("Slippery trough "))renderer.enabled=false;
                else if(patch.name=="Grippy start cradle"||patch.name=="Start cradle recovery climb")renderer.sharedMaterial=amber;
                else if(patch.name=="Grippy receiving cradle")renderer.sharedMaterial=tray;
                else if(patch.name.EndsWith("cradle cheek"))renderer.sharedMaterial=ivory;
            }
            var proxy=root.GetComponentInChildren<COgheSurfacePickProxy>();
            var source=proxy.GetComponent<MeshCollider>().sharedMesh.vertices;
            int count=source.Length/4;
            Mesh Ribbon(Vector3[] a,Vector3[] b,Vector3[] c,Vector3[] d)
            {
                var vertices=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
                void Strip(Vector3[] left,Vector3[] right)
                {
                    int start=vertices.Count;
                    float distance=0;
                    for(int i=0;i<count;i++)
                    {
                        if(i>0)distance+=Vector3.Distance(left[i],left[i-1]);
                        vertices.Add(left[i]);vertices.Add(right[i]);uv.Add(new Vector2(0,distance*12));uv.Add(new Vector2(1,distance*12));
                    }
                    for(int i=0;i<count-1;i++)
                    {int p=start+i*2;triangles.AddRange(new[]{p,p+1,p+3,p,p+3,p+2});}
                }
                Strip(a,b);Strip(b,c);Strip(c,d);Strip(d,a);
                void Cap(int i,bool reverse)
                {
                    int p=vertices.Count;vertices.AddRange(new[]{a[i],b[i],c[i],d[i]});
                    uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up});
                    triangles.AddRange(reverse?new[]{p,p+2,p+1,p,p+3,p+2}:new[]{p,p+1,p+2,p,p+2,p+3});
                }
                Cap(0,true);Cap(count-1,false);
                var mesh=new Mesh{name="Continuous solid slide ribbon"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
            }
            void Render(string name,Mesh mesh,Material material)
            {
                var go=Child(art,name);go.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                go.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
            }
            var left=new Vector3[count];var right=new Vector3[count];var underLeft=new Vector3[count];var underRight=new Vector3[count];
            for(int i=0;i<count;i++)
            {
                left[i]=source[i*4];right[i]=source[i*4+1];Vector3 normal=(source[i*4+2]-left[i]).normalized;
                underLeft[i]=left[i]-normal*.008f;underRight[i]=right[i]-normal*.008f;
            }
            Render("Lavender continuous sliding bed",Ribbon(left,right,underRight,underLeft),lavender);
            foreach(int side in new[]{0,1})
            {
                var innerBottom=new Vector3[count];var innerTop=new Vector3[count];var outerBottom=new Vector3[count];var outerTop=new Vector3[count];
                for(int i=0;i<count;i++)
                {
                    innerBottom[i]=source[i*4+side];innerTop[i]=source[i*4+side+2];
                    Vector3 outward=Vector3.forward*(side==0?-.008f:.008f);
                    outerBottom[i]=innerBottom[i]+outward;outerTop[i]=innerTop[i]+outward;
                }
                Render("Porcelain continuous slide guard",Ribbon(innerBottom,innerTop,outerTop,outerBottom),ivory);
            }
            // Small mounts under each platform explain how it attaches to the
            // glass frame without concealing the path, catch tray or aperture.
            Box(art,"Start platform mount",new Vector3(-.432f,.085f,0),new Vector3(.028f,.04f,.20f),.004f,alloy);
            Box(art,"Catch platform mount",new Vector3(.432f,-.21f,0),new Vector3(.028f,.04f,.22f),.004f,alloy);
            CombineByMaterial(art);
        }
    }
}
