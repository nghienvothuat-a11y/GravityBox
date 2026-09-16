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
            Remove(root,ArtRoot);Remove(owner.transform,"Day Lab studio");
            foreach(var p in game.Props)Remove(p.transform,"COghe mechanism art");
            foreach(var rb in new[]{game.Knife,game.ButtonCover,game.ExitCover})if(rb!=null)Remove(rb.transform,"COghe mechanism art");
            InitializeMaterials(owner);
            var clear=Glass("Clear optical glass",.025f,0,false);
            var coat=Glass("Ice satin coating",.24f,1,true);
            var top=Glass("Ceiling satin coating",.085f,.65f,false);
            var coverGlass=Glass("Loose cover glass",.13f,0,false);
            var dividerGlass=Glass("Divider glass",.12f,0,false);
            var tubeGlass=Glass("Transfer bore glass",.09f,0,false);
            var sphereGlass=Glass("Satin sphere",.055f,.48f,false);
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
                if(p.name.StartsWith("Low wall")&&p.Size.x>.1f)PatchOutline(art,p,new Rect(-p.Size.x*.5f,-p.Size.y*.5f,p.Size.x,p.Size.y),alloy,.0017f);
            }
            if(game.Tube!=null)
            {
                game.Tube.GetComponentInChildren<MeshRenderer>().sharedMaterial=tubeGlass;
                foreach(float z in new[]{0f,game.Tube.Length})
                    Ring(art,"Pipe coupling",game.Tube.transform.TransformPoint(Vector3.forward*z),game.Tube.transform.forward,game.Tube.Radius+.003f,.003f,ivory);
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
            // The actual open-bottom lid keeps all five existing surface patches.
            foreach(float a in new[]{-1f,1f})
            {
                foreach(float y in new[]{-.048f,.048f})
                {
                    Box(art,"Lid lip X",new Vector3(0,y,a*.075f),new Vector3(.15f,.0028f,.0028f),.001f,amber);
                    Box(art,"Lid lip Z",new Vector3(a*.075f,y,0),new Vector3(.0028f,.0028f,.15f),.001f,amber);
                }
                foreach(float b in new[]{-1f,1f})Box(art,"Lid corner",new Vector3(a*.075f,0,b*.075f),new Vector3(.0025f,.10f,.0025f),.001f,alloy);
            }
            CombineByMaterial(art);
        }

        private static void BossMechanisms(VenomCampaign game,Transform art)
        {
            foreach(var pad in new[]{game.PadA,game.PadB})
            {
                Box(art,"Sensor porcelain bezel",pad.localPosition-Vector3.up*.0028f,new Vector3(.095f,.004f,.095f),.005f,ivory);
                Label(art,pad==game.PadA?"A":"B",pad.localPosition+Vector3.up*.0008f,Quaternion.Euler(90,0,0),.014f,ink);
            }
            void Cover(Rigidbody body,Vector3 size,string name)
            {
                body.GetComponent<Renderer>().enabled=false;
                var kit=Child(body.transform,"COghe mechanism art");var s=body.transform.localScale;kit.localScale=new Vector3(1/s.x,1/s.y,1/s.z);
                Box(kit,name,Vector3.zero,size,.003f,ivory);
                Box(kit,"Amber moving edge",new Vector3(0,size.y*.5f+.0002f,0),new Vector3(size.x*.62f,.0005f,size.z*.12f),.0002f,amber);
                CombineByMaterial(kit);
            }
            Cover(game.ButtonCover,new Vector3(.108f,.05f,.108f),"Lift cover");
            Cover(game.ExitCover,new Vector3(.105f,.009f,.105f),"Exit shutter");
            game.Knife.GetComponent<Renderer>().enabled=true;
            game.Knife.GetComponent<Renderer>().sharedMaterial=Lit("Brushed cutting steel",new Color(.29f,.39f,.44f),.86f,.68f);
            var knifeArt=Child(game.Knife.transform,"COghe mechanism art");
            Box(knifeArt,"Blade amber spine",new Vector3(0,.053f,0),new Vector3(.008f,.014f,.128f),.002f,amber);
            CombineByMaterial(knifeArt);
            foreach(float side in new[]{-1f,1f})
            {
                Box(art,"Blade guide",new Vector3(0,-.15f,-.13f+side*.073f),new Vector3(.012f,.245f,.010f),.003f,alloy);
                Box(art,"Guide foot",new Vector3(0,-.285f,-.13f+side*.073f),new Vector3(.028f,.022f,.022f),.004f,ivory);
            }
            Box(art,"Blade bridge",new Vector3(0,-.025f,-.13f),new Vector3(.024f,.014f,.167f),.004f,ivory);
        }
    }
}
