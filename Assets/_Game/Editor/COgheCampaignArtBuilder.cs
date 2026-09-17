using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        [MenuItem("Gravity Box/COghe/Rebuild Day Lab · Boss 10")]
        public static void RebuildBossPresentation()
        {
            var scene=EditorSceneManager.OpenScene(VenomCampaignBuilder.Folder+"/VenomOrigin10.unity");
            ApplyCampaignLevel(Object.FindFirstObjectByType<VenomCampaign>());
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("COGHE BOSS ART SUCCESS: presentation rebuilt; puzzle geometry unchanged.");
        }

        [MenuItem("Gravity Box/COghe/Apply Day Lab · All 10 Origin levels")]
        public static void RebuildCampaign()
        {
            for(int n=1;n<=10;n++)
            {
                var scene=EditorSceneManager.OpenScene(VenomCampaignBuilder.Folder+$"/VenomOrigin{n:00}.unity");
                ApplyCampaignLevel(Object.FindFirstObjectByType<VenomCampaign>());
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"COGHE ART {n:00}: presentation applied.");
            }
            AssetDatabase.SaveAssets();
            Debug.Log("COGHE CAMPAIGN ART SUCCESS: ten existing scenes, unchanged puzzle geometry.");
        }

        public static void ApplyCampaignLevel(VenomCampaign game)
        {
            if(game==null)throw new ArgumentNullException(nameof(game));
            if(game.Definition.Order==7){Apply(game);return;}
            int number=game.Definition.Order;
            meshDirectory=$"Meshes/Level{number:00}";serial=0;
            Directory.CreateDirectory(Folder+"/"+meshDirectory);AssetDatabase.Refresh();
            var owner=game.GetComponent<VenomLevelController>();var root=owner.Rotation.transform;
            Remove(root,ArtRoot);Remove(root,"COghe mechanism indicators");Remove(owner.transform,"Day Lab studio");
            foreach(var p in game.Props)Remove(p.transform,"COghe mechanism art");
            foreach(var rb in new[]{game.Knife,game.ButtonCover,game.ExitCover})if(rb!=null)Remove(rb.transform,"COghe mechanism art");
            InitializeMaterials(owner);
            var clear=Glass("Clear optical glass",.025f,0,false);
            var coat=Glass("Ice satin coating",.24f,1,true);
            var top=Glass("Ceiling satin coating",.16f,.8f,false);
            var coverGlass=Glass("Loose cover glass",.13f,0,false);
            var dividerGlass=Glass("Divider glass",.36f,0,false);
            dividerGlass.SetColor("_BaseColor",new Color(.91f,.56f,.20f,.36f));
            var tubeGlass=Glass("Transfer bore glass",.32f,0,false);
            tubeGlass.SetColor("_BaseColor",new Color(.16f,.55f,.67f,.32f));
            var sphereGlass=Glass("Satin sphere",.075f,.70f,false);
            var floorGlass=Lit("Pearl inspection floor",new Color(.77f,.83f,.84f,.93f),.08f,.38f);
            floorGlass.SetFloat("_Surface",1);floorGlass.SetFloat("_SrcBlend",(int)BlendMode.SrcAlpha);floorGlass.SetFloat("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);floorGlass.SetFloat("_ZWrite",0);
            floorGlass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");floorGlass.SetFloat("_BlendModePreserveSpecular",0);floorGlass.renderQueue=2990;EditorUtility.SetDirty(floorGlass);
            var floors=new List<Renderer>();var panes=new List<VenomSurfacePatch>();
            foreach(var p in game.Surfaces)
            {
                var r=p.GetComponent<MeshRenderer>();r.enabled=true;r.shadowCastingMode=ShadowCastingMode.Off;
                bool dynamic=p.GetComponentInParent<VenomMovableProp>()!=null;
                bool pad=p.transform==game.PadA||p.transform==game.PadB;
                bool isFloor=!dynamic&&!pad&&p.Size.x>.3f&&Vector3.Dot(p.Normal,Vector3.up)>.99f;
                bool divider=p.name.StartsWith("Low wall");
                r.sharedMaterial=pad?mint:dynamic?coverGlass:divider?dividerGlass:p.SphereRadius>0?sphereGlass:p.Slippery?(p.RingGrip?coat:top):p.HasSlipRegion?coat:isFloor?(game.Definition.CanRotate?floorGlass:floor):clear;
                if(isFloor&&game.Definition.CanRotate)floors.Add(r);
                if(r.sharedMaterial.shader.name=="COghe/Lab Glass")panes.Add(p);
                foreach(var line in p.GetComponentsInChildren<LineRenderer>())line.enabled=false;
                var overlay=p.transform.Find("Visible slippery coating");if(overlay!=null)overlay.GetComponent<Renderer>().enabled=false;
            }
            foreach(var line in owner.Outlet.GetComponentsInChildren<LineRenderer>()){line.enabled=true;line.sharedMaterial=mint;}
            var art=Child(root,ArtRoot);
            if(number==6)SphereFrame(art,owner);
            else if(number==8)
            {
                ChamberFrame(art,new Vector3(-.34f,0,0),.23f,"08",true);
                ChamberFrame(art,new Vector3(.34f,0,0),.23f,"B",true);
            }
            else ChamberFrame(art,Vector3.zero,.3f,number.ToString("00"),false);
            Ring(art,"Flush exit lip",owner.Outlet.position-owner.Outlet.forward*.0008f,owner.Outlet.forward,owner.ApertureRadius+.0018f,.0014f,alloy);
            foreach(var p in game.Surfaces)
            {
                if(p.RingGrip)
                    Ring(art,"Clear grip boundary",p.transform.TransformPoint(new Vector3(p.HoleCentre.x,p.HoleCentre.y,.001f)),p.Normal,p.GripRadius,.0007f,mint);
                if(p.HasSlipRegion)PatchOutline(art,p,p.SlipRegion,alloy,.0012f);
                if(p.name.StartsWith("Low wall")&&p.Size.x>.1f)PatchOutline(art,p,new Rect(-p.Size.x*.5f,-p.Size.y*.5f,p.Size.x,p.Size.y),amber,.0028f);
            }
            if(game.Tube!=null)
            {
                var pipeTrim=Lit("Pipe anodised fittings",new Color(.23f,.45f,.52f),.58f,.52f);
                game.Tube.GetComponentInChildren<MeshRenderer>().sharedMaterial=tubeGlass;
                foreach(float z in new[]{0f,game.Tube.Length})
                {
                    Vector3 point=game.Tube.transform.TransformPoint(Vector3.forward*z);
                    Ring(art,"Porcelain pipe collar",point,game.Tube.transform.forward,game.Tube.Radius+.007f,.006f,ivory);
                    Ring(art,"Pipe gasket",point,game.Tube.transform.forward,game.Tube.Radius+.003f,.0025f,pipeTrim);
                }
                Ring(art,"Pipe centre band",game.Tube.transform.TransformPoint(Vector3.forward*game.Tube.Length*.5f),game.Tube.transform.forward,game.Tube.Radius+.0017f,.0013f,alloy);
                foreach(float side in new[]{-1f,1f})
                {
                    var line=Child(art,"Pipe longitudinal seam").gameObject.AddComponent<LineRenderer>();
                    line.sharedMaterial=pipeTrim;line.useWorldSpace=true;line.positionCount=2;line.startWidth=line.endWidth=.0025f;
                    line.SetPosition(0,game.Tube.transform.TransformPoint(new Vector3(side*(game.Tube.Radius+.001f),0,0)));
                    line.SetPosition(1,game.Tube.transform.TransformPoint(new Vector3(side*(game.Tube.Radius+.001f),0,game.Tube.Length)));
                    line.shadowCastingMode=ShadowCastingMode.Off;
                }
            }
            foreach(var p in game.Props)CoverFrame(p);
            if(game.Definition.Boss)BossMechanisms(game,art);

            var studio=Child(owner.transform,"Day Lab studio");
            float deskTop=number==8?-.306f:number==6?-.47f:-.68f;
            var desk=Lit("Warm lab bench",new Color(.90f,.89f,.855f),0,.2f);
            Box(studio,"Seamless laboratory bench",new Vector3(0,deskTop-.009f,0),new Vector3(12,.018f,12),.006f,desk);
            SoftShadow(studio,"Soft suspended chamber shadow",new Vector3(.025f,deskTop+.001f,.025f),number==8?1.40f:.90f,.13f,.53f);
            Lighting(owner,studio);
            var presentation=owner.GetComponent<COgheDayLabPresentation>();if(presentation==null)presentation=owner.gameObject.AddComponent<COgheDayLabPresentation>();
            ConfigureWorldLabels(presentation);
            presentation.FadingFloors=floors.ToArray();presentation.GlassSurfaces=panes.ToArray();presentation.Step=presentation.StepShadow=null;
            CombineByMaterial(art);CombineByMaterial(studio);
            // No scenery disables a collider; close inspection only hides trim.
            presentation.FocusOccluders=art.GetComponentsInChildren<Renderer>();
            EditorUtility.SetDirty(presentation);EditorUtility.SetDirty(owner);
            foreach(var guid in AssetDatabase.FindAssets("t:Mesh",new[]{Folder+"/"+meshDirectory}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid),name=Path.GetFileNameWithoutExtension(path);
                if(name.StartsWith("LabMesh")&&int.TryParse(name.Substring(7),out int i)&&i>=serial)AssetDatabase.DeleteAsset(path);
            }
        }

        private static void ChamberFrame(Transform parent,Vector3 center,float half,string tag,bool fixedTray)
        {
            var frame=Child(parent,"Chamber "+tag);frame.localPosition=center;frame.localScale=Vector3.one*(half/.3f);
            if(fixedTray)
                Box(frame,"Porcelain tray chassis",new Vector3(0,-.326f,0),new Vector3(.652f,.048f,.652f),.010f,ivory);
            foreach(float a in new[]{-1f,1f})
            {
                foreach(float y in new[]{-.308f,.307f})
                {
                    Box(frame,"Horizontal frame X",new Vector3(0,y,a*.309f),new Vector3(.62f,.011f,.014f),.004f,alloy);
                    Box(frame,"Horizontal frame Z",new Vector3(a*.309f,y,0),new Vector3(.014f,.011f,.62f),.004f,alloy);
                }
                foreach(float b in new[]{-1f,1f})
                {
                    Box(frame,"Glass joint",new Vector3(a*.309f,0,b*.309f),new Vector3(.009f,.606f,.009f),.003f,alloy);
                    foreach(float y in new[]{-.294f,.29f})
                    {
                        Box(frame,"Porcelain corner mount",new Vector3(a*.313f,y,b*.313f),new Vector3(.045f,.059f,.045f),.008f,ivory);
                        var pos=new Vector3(a*.313f,y,b*.337f);
                        Disk(frame,"Recessed socket",pos,Vector3.forward*b,.009f,.0015f,graphite);
                        Disk(frame,"Steel fastener",pos+Vector3.forward*b*.001f,Vector3.forward*b,.004f,.001f,alloy);
                    }
                }
            }
            Box(frame,"Level badge",new Vector3(-.18f,.242f,-.318f),new Vector3(.11f,.063f,.013f),.006f,ivory);
            Label(frame,tag,new Vector3(-.18f,.242f,-.326f),Quaternion.identity,.019f,ink);
            Box(frame,"COghe badge",new Vector3(.14f,-.308f,-.319f),new Vector3(.125f,.025f,.006f),.003f,ivory);
            Label(frame,"COghe",new Vector3(.14f,-.308f,-.323f),Quaternion.identity,.008f,ink);
        }

        private static void SphereFrame(Transform art,VenomLevelController owner)
        {
            // Seams follow the outside of the real sphere, never bridge the bore.
            Ring(art,"Sphere equatorial joint",Vector3.zero,Vector3.up,.302f,.0015f,alloy);
            var seam=Child(art,"Sphere meridian seam").gameObject.AddComponent<LineRenderer>();
            seam.useWorldSpace=false;seam.sharedMaterial=alloy;seam.startWidth=seam.endWidth=.0013f;seam.positionCount=97;
            float start=Mathf.Asin((owner.ApertureRadius+.009f)/.303f);
            for(int i=0;i<=96;i++){float a=Mathf.Lerp(start,Mathf.PI*2-start,i/96f);seam.SetPosition(i,new Vector3(Mathf.Sin(a)*.303f,Mathf.Cos(a)*.303f,0));}
            seam.shadowCastingMode=ShadowCastingMode.Off;
            var badge=Child(art,"Sphere identification");badge.localPosition=new Vector3(-.12f,-.18f,-.213f);
            badge.localRotation=Quaternion.LookRotation(-badge.localPosition.normalized,Vector3.up);
            Box(badge,"Porcelain badge",Vector3.zero,new Vector3(.064f,.037f,.008f),.006f,ivory);
            Label(badge,"06",Vector3.back*.005f,Quaternion.identity,.012f,ink);
        }

        private static void PatchOutline(Transform parent,VenomSurfacePatch p,Rect area,Material mat,float width)
        {
            var line=Child(parent,"Surface boundary").gameObject.AddComponent<LineRenderer>();line.sharedMaterial=mat;line.useWorldSpace=false;line.positionCount=5;line.startWidth=line.endWidth=width;line.shadowCastingMode=ShadowCastingMode.Off;
            Vector2[] corners={new Vector2(area.xMin,area.yMin),new Vector2(area.xMax,area.yMin),new Vector2(area.xMax,area.yMax),new Vector2(area.xMin,area.yMax),new Vector2(area.xMin,area.yMin)};
            for(int i=0;i<5;i++)line.SetPosition(i,parent.InverseTransformPoint(p.transform.TransformPoint(new Vector3(corners[i].x,corners[i].y,.0008f))));
        }

        private static void CoverFrame(VenomMovableProp prop)
        {
            var art=Child(prop.transform,"COghe mechanism art");
            if(prop.GetComponent<VenomPropContainment>()!=null)
            {
                var shell=Glass("Reinforced cover resin",.24f,0,false);
                shell.SetColor("_BaseColor",new Color(.82f,.69f,.45f,.24f));EditorUtility.SetDirty(shell);
                foreach(var face in prop.GetComponentsInChildren<VenomSurfacePatch>())
                {
                    var box=face.Shape as BoxCollider;if(box==null)continue;
                    face.GetComponent<Renderer>().enabled=false;
                    var panel=Child(art,"Solid resin wall");
                    panel.position=face.transform.TransformPoint(box.center);panel.rotation=face.transform.rotation;
                    Box(panel,"14 mm resin section",Vector3.zero,box.size,.001f,shell);
                }
            }
            // The actual open-bottom lid keeps all five existing surface patches.
            foreach(float a in new[]{-1f,1f})
            {
                foreach(float y in new[]{-.048f,.048f})
                {
                    Box(art,"Lid lip X",new Vector3(0,y,a*.071f),new Vector3(.15f,.004f,.008f),.001f,amber);
                    Box(art,"Lid lip Z",new Vector3(a*.071f,y,0),new Vector3(.008f,.004f,.15f),.001f,amber);
                }
                foreach(float b in new[]{-1f,1f})Box(art,"Lid corner",new Vector3(a*.072f,0,b*.072f),new Vector3(.006f,.10f,.006f),.001f,alloy);
            }
            CombineByMaterial(art);
        }

        private static void BossMechanisms(VenomCampaign game,Transform art)
        {
            var feedback=game.GetComponent<COgheBossPresentation>();
            if(feedback==null)feedback=game.gameObject.AddComponent<COgheBossPresentation>();
            feedback.PadCaps=new Transform[2];feedback.PadLights=new Renderer[2];
            var live=Child(game.GetComponent<VenomLevelController>().Rotation.transform,"COghe mechanism indicators");
            var socket=Lit("Pale sensor housing",new Color(.69f,.77f,.79f),.25f,.48f);
            int index=0;
            foreach(var pad in new[]{game.PadA,game.PadB})
            {
                pad.GetComponent<Renderer>().enabled=false;
                Remove(pad,"COghe mechanism art");
                var assembly=Child(pad,"COghe mechanism art");
                Box(assembly,"Recessed sensor socket",Vector3.back*.002f,new Vector3(.094f,.094f,.004f),.009f,socket);
                Ring(assembly,"Porcelain sensor bezel",pad.position,pad.forward,.043f,.005f,ivory);
                var cap=Child(assembly,"Moving pressure cap");cap.localPosition=Vector3.forward*.007f;
                Disk(cap,"Amber pressure face",Vector3.zero,Vector3.forward,.036f,.004f,amber);
                Label(cap,index==0?"A":"B",Vector3.forward*.0021f,Quaternion.Euler(0,180,0),.024f,ink);
                CombineByMaterial(cap);
                feedback.PadCaps[index]=cap;
                var indicator=Child(live,"Pad status "+index);indicator.position=pad.TransformPoint(new Vector3(0,-.049f,.001f));indicator.rotation=pad.rotation;
                Box(indicator,"Pressure indicator",Vector3.zero,new Vector3(.039f,.006f,.003f),.001f,mint);
                CombineByMaterial(indicator);feedback.PadLights[index]=indicator.GetComponentInChildren<Renderer>();
                // Batch the fixed socket separately; the top must stay movable.
                CombineByMaterial(assembly,cap);
                index++;
            }
            void Cover(Rigidbody body,Vector3 size,string name,string badge)
            {
                body.GetComponent<Renderer>().enabled=false;
                var kit=Child(body.transform,"COghe mechanism art");var scale=body.transform.localScale;kit.localScale=new Vector3(1/scale.x,1/scale.y,1/scale.z);
                Box(kit,name,Vector3.zero,size,.003f,ivory);
                Box(kit,"Amber moving edge",new Vector3(0,size.y*.5f+.0003f,0),new Vector3(size.x*.72f,.001f,size.z*.16f),.0003f,amber);
                Label(kit,badge,new Vector3(0,size.y*.5f+.001f,-.013f),Quaternion.Euler(90,0,0),.018f,ink);
                CombineByMaterial(kit);
            }
            Cover(game.ButtonCover,new Vector3(.108f,.05f,.108f),"Lift cover","B");
            Cover(game.ExitCover,new Vector3(.105f,.009f,.105f),"Exit shutter","•");
            var steel=Lit("Brushed cutting steel",new Color(.88f,.91f,.94f),.58f,.78f);
            steel.SetTexture("_BaseMap",BrushedSteelTexture());
            var edge=Lit("Polished cutting edge",new Color(.95f,.97f,1f),.66f,.88f);
            game.Knife.GetComponent<Renderer>().enabled=true;game.Knife.GetComponent<Renderer>().sharedMaterials=new[]{steel,edge};
            game.Knife.GetComponent<MeshFilter>().sharedMesh=BladeMesh();
            var knifeArt=Child(game.Knife.transform,"COghe mechanism art");
            Box(knifeArt,"Blade clamp",new Vector3(0,.051f,0),new Vector3(.015f,.018f,.143f),.002f,graphite);
            Box(knifeArt,"Honed cutting edge",new Vector3(0,-.058f,0),new Vector3(.0018f,.003f,.13f),.0005f,edge);
            CombineByMaterial(knifeArt);
            foreach(float side in new[]{-1f,1f})
            {
                Box(art,"Blade guide",new Vector3(0,-.132f,-.13f+side*.078f),new Vector3(.014f,.326f,.012f),.003f,alloy);
                Box(art,"Guide foot",new Vector3(0,-.289f,-.13f+side*.078f),new Vector3(.031f,.021f,.029f),.004f,ivory);
            }
            Box(art,"Blade bridge",new Vector3(0,.032f,-.13f),new Vector3(.029f,.021f,.182f),.004f,ivory);
            feedback.WarningLights=new Renderer[4];
            for(int i=0;i<4;i++)
            {
                var lamp=Child(live,"Countdown lamp "+i);
                Box(lamp,"Sensor countdown",new Vector3(-.016f,.034f,-.17f+i*.027f),new Vector3(.003f,.009f,.013f),.002f,mint);
                CombineByMaterial(lamp);feedback.WarningLights[i]=lamp.GetComponentInChildren<Renderer>();
            }
            var zone=Child(live,"Cut sensor boundary").gameObject.AddComponent<LineRenderer>();
            zone.useWorldSpace=false;zone.sharedMaterial=mint;zone.startWidth=zone.endWidth=.002f;zone.positionCount=5;
            zone.SetPositions(new[]{new Vector3(-.025f,-.299f,-.198f),new Vector3(.025f,-.299f,-.198f),new Vector3(.025f,-.299f,-.062f),new Vector3(-.025f,-.299f,-.062f),new Vector3(-.025f,-.299f,-.198f)});
            zone.shadowCastingMode=ShadowCastingMode.Off;feedback.SensorBoundary=zone;
            // Visible equipment wiring conveys relationships without a tutorial.
            var wire=Child(art,"Sensor control conduit").gameObject.AddComponent<LineRenderer>();
            wire.useWorldSpace=false;wire.sharedMaterial=alloy;wire.startWidth=wire.endWidth=.002f;wire.positionCount=4;
            wire.SetPositions(new[]{new Vector3(-.135f,-.299f,.181f),new Vector3(-.135f,-.299f,.235f),new Vector3(.135f,-.299f,.235f),new Vector3(.135f,-.299f,.186f)});wire.shadowCastingMode=ShadowCastingMode.Off;
            EditorUtility.SetDirty(feedback);
        }
        private static Mesh BladeMesh()
        {
            var v=new[]{new Vector3(-.004f,.06f,-.065f),new Vector3(.004f,.06f,-.065f),new Vector3(.004f,.06f,.065f),new Vector3(-.004f,.06f,.065f),
                new Vector3(-.004f,-.043f,-.065f),new Vector3(.004f,-.043f,-.065f),new Vector3(.004f,-.043f,.065f),new Vector3(-.004f,-.043f,.065f),
                new Vector3(-.0007f,-.06f,-.065f),new Vector3(.0007f,-.06f,-.065f),new Vector3(.0007f,-.06f,.065f),new Vector3(-.0007f,-.06f,.065f)};
            // Independent face vertices keep the broad steel faces flat. Sharing
            // all eight corners averaged in the thin edges and darkened the blade.
            int[] faces={0,3,2,1,8,9,10,11,0,1,5,4,4,5,9,8,3,7,6,2,7,11,10,6,0,4,7,3,1,2,6,5,4,8,11,7,5,6,10,9};
            var vertices=new Vector3[40];var uv=new Vector2[40];var bodyTriangles=new List<int>();var edgeTriangles=new List<int>();
            for(int face=0;face<10;face++)
            {
                int start=face*4;
                for(int j=0;j<4;j++)
                {
                    var p=v[faces[start+j]];vertices[start+j]=p;
                    uv[start+j]=face>=6?new Vector2((p.z+.065f)/.13f,(p.y+.06f)/.12f):new Vector2(j==1||j==2?1:0,j>=2?1:0);
                }
                var triangles=face>=8||face==1?edgeTriangles:bodyTriangles;
                triangles.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
            }
            var mesh=new Mesh{name="Tapered steel blade",vertices=vertices,uv=uv,subMeshCount=2};
            mesh.SetTriangles(bodyTriangles,0);mesh.SetTriangles(edgeTriangles,1);
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return SavedMesh(mesh);
        }

        private static Texture2D BrushedSteelTexture()
        {
            const int size=128;
            var texture=SaveAsset("Brushed steel grain.asset",()=>new Texture2D(size,size,TextureFormat.RGBA32,true));
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                // Restrained horizontal polishing grain; one small mipmapped
                // albedo map, with no extra lights or runtime reflection capture.
                float grain=.95f+.027f*Mathf.Sin(y*2.37f)+.014f*Mathf.Sin(y*.63f+x*.035f);
                pixels[y*size+x]=new Color(grain,grain,grain,1);
            }
            texture.SetPixels(pixels);texture.wrapMode=TextureWrapMode.Repeat;
            texture.filterMode=FilterMode.Trilinear;texture.Apply(true);
            EditorUtility.SetDirty(texture);return texture;
        }
    }
}
