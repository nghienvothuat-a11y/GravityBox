using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Simulation;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        public const string ViewFolder="Assets/_Game/Venom/ViewCampaign";
        private static readonly string[] ViewNames={"Chạm để đi","Chạm mặt khác","Kéo để nhìn","Nhìn gần hơn","Chạm cơ quan","Tự làm lại","Đưa trở về","Bắc một nhịp","Hai bước nhìn thấy","Cỗ máy nhỏ"};
        private static readonly string[] ViewLessons={"Chạm vòng xanh để COghe tới lối ra.","Chạm bậc rộng để leo, rồi tới lối ra.","Kéo ngang để nhìn quanh vách. Hộp luôn đứng yên.","Tách hai ngón để nhìn gần. Toàn cảnh đưa góc nhìn về xa.","Chạm tay nắm A. COghe tự tới, đẩy và dừng ở nấc.","Tìm tay nắm và quan sát cửa thay đổi.","Vào khoang rồi đưa cửa trở về để mở lối bên kia.","Chạm tay nắm để nối cầu, rồi đi qua.","Quan sát nắp che và tay nắm bên dưới.",""};
        public static string[] ViewCampaignScenePaths()
        {var paths=new string[10];for(int i=0;i<10;i++)paths[i]=$"{ViewFolder}/COgheView{i+1:00}.unity";return paths;}

        [MenuItem("Gravity Box/COghe/V2/Generate ten view-only levels")]
        public static void GenerateViewCampaign()
        {
            PrepareCampaign30Assets();Directory.CreateDirectory(ViewFolder+"/Definitions");Directory.CreateDirectory(ViewFolder+"/Meshes");AssetDatabase.Refresh();
            string previousFolder=authoredMeshFolder;
            try{authoredMeshFolder=ViewFolder+"/Meshes";for(int n=1;n<=10;n++)BuildViewLevel(n);}
            finally{authoredMeshFolder=previousFolder;}
            var scenes=new List<EditorBuildSettingsScene>();
            foreach(string path in ViewCampaignScenePaths())scenes.Add(new EditorBuildSettingsScene(path,true));
            // Archived scenes remain available to the full regression suite. Player builders use only the V2 catalog.
            foreach(var s in EditorBuildSettings.scenes)if(!scenes.Exists(x=>x.path==s.path))scenes.Add(s);
            EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
            Debug.Log("COGHE V2 GENERATED: 10 new levels, default player catalog; old content and progress retained.");
        }

        public static void GenerateViewAndBuildMac(){GenerateViewCampaign();BuildMac();}

        private static void BuildViewLevel(int n)
        {
            meshSerial=n*1000;
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var owner=new GameObject("COghe V2 · "+ViewNames[n-1]).AddComponent<VenomLevelController>();
            Undo.RegisterCreatedObjectUndo(owner.gameObject,"Author COghe V2");
            owner.MatterProfile=AssetDatabase.LoadAssetAtPath<VenomProfile>(Folder+"/Matter.asset");owner.ControlMode=VenomControlMode.TouchSurface;
            owner.RotationProfile=AssetDatabase.LoadAssetAtPath<RotationSettings>("Assets/_Game/PhysicsLab/Profiles/Hand rotation.asset");owner.IndicatorMaterial=mint;owner.ApertureRadius=.046f;
            var game=owner.gameObject.AddComponent<VenomCampaign>();
            string path=$"{ViewFolder}/Definitions/View{n:00}.asset";
            var def=AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(path);
            if(def==null){def=ScriptableObject.CreateInstance<VenomCampaignDefinition>();AssetDatabase.CreateAsset(def,path);}
            def.Id=$"coghe.view.v2.{n:00}";def.Order=n;def.Title=$"{n:00} · {ViewNames[n-1]}";def.Lesson=ViewLessons[n-1];
            def.ViewOnly=true;def.CanRotate=false;def.Passive=false;def.Boss=n==10;def.ProgressKey="";
            def.CameraEuler=new Vector3(48,20,0);def.CameraZones=Array.Empty<VenomCameraZone>();def.InitialCameraZone=-1;
            def.SceneSequence=Array.ConvertAll(ViewCampaignScenePaths(),Path.GetFileNameWithoutExtension);game.Definition=def;
            owner.Apparatus=new GameObject("Apparatus").transform;owner.Apparatus.SetParent(owner.transform,false);
            var pivot=new GameObject("Fixed chamber",typeof(Rigidbody),typeof(BoxRotationController));pivot.transform.SetParent(owner.Apparatus,false);
            pivot.GetComponent<Rigidbody>().isKinematic=true;pivot.GetComponent<Rigidbody>().useGravity=false;owner.Rotation=pivot.GetComponent<BoxRotationController>();
            var c=new ExpansionContext{Number=n,Owner=owner,Game=game,Definition=def,Root=pivot.transform,Spawn=new Vector3(-.25f,-.250f,-.17f),Exit=new Vector3(.12f,-.225f,.30f),Outward=Vector3.forward};
            if(n==2||n==4)c.Exit=new Vector3(.16f,-.125f,.30f);
            if(n==6){c.Exit.x=-.15f;c.Spawn.x=.25f;}
            if(n==7){c.Exit=new Vector3(.22f,-.30f,.21f);c.Outward=Vector3.down;}
            bool bridge=n==8||n==10;
            if(bridge){c.Exit=new Vector3(.32f,-.30f,.16f);c.Outward=Vector3.down;c.Spawn=new Vector3(-.28f,-.250f,-.23f);}
            ViewShell(c,bridge);
            var floor=c.Surfaces[0];
            if(n==2||n==4)
            {
                Panel(c.Root,"Broad step top",new Vector3(0,-.20f,.24f),Vector3.up,new Vector2(.8f,.12f),stone,false,Vector2.zero,0,c.Surfaces);
                Panel(c.Root,"Broad inclined step",new Vector3(0,-.25f,.13f),new Vector3(0,1,-1).normalized,new Vector2(.8f,.1414214f),stone,false,Vector2.zero,0,c.Surfaces);
            }
            if(n==3)
            {
                ViewBlock(c,"Observation wall",new Vector3(.12f,-.15f,.10f),new Vector3(.28f,.30f,.016f));
                ViewBlock(c,"Observation return",new Vector3(-.02f,-.15f,.16f),new Vector3(.016f,.30f,.12f));
                def.CameraEuler=new Vector3(38,0,0);
            }
            if(n==5||n==6)
            {
                float axis=n==6?-1:1;
                var task=ViewTask(c,"A",new Vector3(c.Exit.x,-.277f,.12f),Vector3.right*axis,.16f,floor);
                var gate=ViewGate(c,"Exit door",new Vector3(c.Exit.x,-.225f,.281f),Vector3.right*axis,.16f,new Vector3(.13f,.14f,.018f));
                ViewLink(c,task.Rail,gate,true,ViewExitSurface(c));
            }
            if(n==7)ViewReversibleRoom(c,floor);
            COgheTapRail bridgeTask=null;
            if(bridge)bridgeTask=ViewBridge(c,floor);
            if(n==9||n==10)
            {
                var target=n==9?ViewTask(c,"B",new Vector3(.10f,-.277f,.08f),Vector3.right,.16f,floor):bridgeTask;
                var a=ViewTask(c,"A",n==10?new Vector3(-.30f,-.277f,.225f):new Vector3(-.27f,-.277f,.20f),Vector3.right,n==10?.08f:.11f,floor);
                Vector3 handle=target.Handle.position;
                var cover=ViewGate(c,"Opaque handle cover",new Vector3(handle.x,-.235f,handle.z),Vector3.forward,.15f,new Vector3(.115f,.018f,.115f));
                ViewLink(c,a.Rail,cover,false,null);
                target.RequiredRail=cover;target.RequiredEnd=true;
                // The lid and its short support hide both the handle and its input envelope.
                if(n==9)
                {
                    var gate=ViewGate(c,"Exit door",new Vector3(c.Exit.x,-.225f,.281f),Vector3.right,.16f,new Vector3(.13f,.14f,.018f));
                    ViewLink(c,target.Rail,gate,true,ViewExitSurface(c));
                }
                else
                {
                    var right=c.Surfaces.Find(s=>s.name=="Receiving bank");
                    var exit=ViewTask(c,"C",new Vector3(.22f,-.277f,.26f),Vector3.left,.08f,right);
                    var gate=ViewGate(c,"Exit floor shutter",c.Exit+Vector3.up*.012f,Vector3.back,.13f,new Vector3(.13f,.018f,.13f));
                    ViewLink(c,exit.Rail,gate,true,ViewExitSurface(c));
                }
            }
            owner.Spawn=new GameObject("Spawn").transform;owner.Spawn.SetParent(c.Root,false);owner.Spawn.localPosition=c.Spawn;
            owner.Outlet=new GameObject("Final exit").transform;owner.Outlet.SetParent(c.Root,false);owner.Outlet.localPosition=c.Exit;
            owner.Outlet.localRotation=Quaternion.LookRotation(c.Outward,Mathf.Abs(c.Outward.y)>.9f?Vector3.forward:Vector3.up);Ring(owner.Outlet,Vector2.zero,owner.ApertureRadius,.0032f,mint);
            game.Surfaces=c.Surfaces.ToArray();game.Props=c.Props.ToArray();owner.CrawlFaces=new Collider[6];for(int i=0;i<6;i++)owner.CrawlFaces[i]=floor.Shape;
            foreach(var prop in c.Props)prop.transform.SetParent(owner.Apparatus,true);
            var camera=new GameObject("Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.transform.SetParent(owner.transform,false);camera.tag="MainCamera";
            camera.orthographic=true;camera.nearClipPlane=.005f;camera.farClipPlane=15;camera.clearFlags=CameraClearFlags.SolidColor;camera.transform.rotation=Quaternion.Euler(def.CameraEuler);camera.transform.position=-camera.transform.forward*3;owner.View=camera;
            Lighting();COgheDayLabBuilder.ApplyViewLevel(game);
            EditorUtility.SetDirty(def);EditorSceneManager.SaveScene(scene,ViewCampaignScenePaths()[n-1]);
        }

        private static VenomSurfacePatch ViewExitSurface(ExpansionContext c)=>c.Surfaces.Find(s=>s.Hole&&Vector3.Distance(s.transform.TransformPoint(new Vector3(s.HoleCentre.x,s.HoleCentre.y,0)),c.Exit)<.01f);
        private static void ViewShell(ExpansionContext c,bool bridge)
        {
            if(!bridge)
            {
                bool step=c.Number==2||c.Number==4;
                Panel(c.Root,"Laboratory floor",new Vector3(0,-.30f,step?-.11f:0),Vector3.up,new Vector2(.80f,step?.38f:.60f),stone,c.Number==7,new Vector2(-c.Exit.x,c.Exit.z),c.Owner.ApertureRadius,c.Surfaces);
            }
            else
            {
                Panel(c.Root,"Departure bank",new Vector3(-.2525f,-.30f,0),Vector3.up,new Vector2(.295f,.60f),stone,false,Vector2.zero,0,c.Surfaces);
                var receiving=Panel(c.Root,"Receiving bank",new Vector3(.2525f,-.30f,.18f),Vector3.up,new Vector2(.295f,.24f),stone,true,new Vector2(.2525f-c.Exit.x,c.Exit.z-.18f),c.Owner.ApertureRadius,c.Surfaces);
                var basin=Panel(c.Root,"Recovery basin",new Vector3(0,-.40f,0),Vector3.up,new Vector2(.80f,.60f),stone,false,Vector2.zero,0,c.Surfaces);basin.Slippery=true;
                Panel(c.Root,"Recovery climb",new Vector3(-.105f,-.35f,0),Vector3.right,new Vector2(.60f,.10f),stone,false,Vector2.zero,0,c.Surfaces);
                var receivingEdge=Panel(c.Root,"Receiving cliff",new Vector3(.105f,-.35f,0),Vector3.left,new Vector2(.60f,.10f),stone,false,Vector2.zero,0,c.Surfaces);receivingEdge.Slippery=true;
            }
            var positions=new[]{new Vector3(0,.04f,0),new Vector3(0,-.13f,-.30f),new Vector3(0,-.13f,.30f),new Vector3(-.4f,-.13f,0),new Vector3(.4f,-.13f,0)};
            var normals=new[]{Vector3.down,Vector3.forward,Vector3.back,Vector3.right,Vector3.left};
            for(int i=0;i<5;i++)
            {
                bool hole=!bridge&&c.Number!=7&&i==2;Vector2 size=i==0?new Vector2(.8f,.60f):new Vector2(i<3?.8f:.60f,.34f);
                Vector3 local=Quaternion.Inverse(Quaternion.LookRotation(normals[i],i==0?Vector3.forward:Vector3.up))*(c.Exit-positions[i]);
                var p=Panel(c.Root,"Outer pane "+i,positions[i],normals[i],size,glass,hole,new Vector2(local.x,local.y),hole?c.Owner.ApertureRadius:0,c.Surfaces);
                p.ExteriorGlass=true;p.Selectable=i!=0;p.Slippery=bridge;
            }
        }
        private static void ViewBlock(ExpansionContext c,string name,Vector3 centre,Vector3 size)
        {
            foreach(var n in new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back})
            {
                float d=Mathf.Abs(n.x)*size.x+Mathf.Abs(n.y)*size.y+Mathf.Abs(n.z)*size.z;
                Vector2 s=Mathf.Abs(n.y)>.9f?new Vector2(size.x,size.z):Mathf.Abs(n.x)>.9f?new Vector2(size.z,size.y):new Vector2(size.x,size.y);
                Panel(c.Root,name,centre+n*d*.5f,n,s,stone,false,Vector2.zero,0,c.Surfaces);
            }
        }
        private static COgheTapRail ViewTask(ExpansionContext c,string key,Vector3 start,Vector3 axis,float travel,VenomSurfacePatch floor)
        {
            var task=TapRail(c,key,start,axis,travel,floor);task.PickHandleOnly=true;task.TrackStandPoint=true;task.TouchSize=new Vector3(.085f,.060f,.070f);task.StandOffset=new Vector3(0,0,-.05f);return task;
        }
        private static COgheRailSlider ViewGate(ExpansionContext c,string name,Vector3 start,Vector3 axis,float travel,Vector3 size)
        {
            var rail=ExpansionRail(c,name,start,axis,travel,0,size,.022f,.004f,false,false);rail.CatchTolerance=.004f;
            if(size.y<.04f)
            {
                // Flat shutters need their decorative guides beside the plate, never over its aperture.
                Vector3 side=Vector3.Cross(axis,Vector3.up).normalized;
                float offset=Mathf.Abs(side.x)*size.x*.5f+Mathf.Abs(side.z)*size.z*.5f+.014f;int guide=0;
                foreach(Transform child in c.Root)
                {
                    if(child.name==name+" rail stop")child.localPosition+=side*offset;
                    if(child.name==name+" fixed guide")child.localPosition=start+axis*travel*.5f+side*(guide++==0?-offset:offset);
                }
            }
            return rail;
        }
        private static void ViewLink(ExpansionContext c,COgheRailSlider input,COgheRailSlider output,bool final,VenomSurfacePatch aperture)
        {
            var link=new GameObject("Mechanical linkage",typeof(COgheViewMechanism)).GetComponent<COgheViewMechanism>();link.transform.SetParent(c.Root,false);
            link.Input=input;link.Output=output;link.FinalGate=final;link.Aperture=aperture;
            TapLink(c.Root,"Visible linkage housing",input.Start+Vector3.down*.01f,output.Start+Vector3.down*.01f);
        }
        private static void ViewReversibleRoom(ExpansionContext c,VenomSurfacePatch floor)
        {
            // A full-height partition with a genuine aperture; the roof prevents bypassing it.
            var entrance=Panel(c.Root,"Interior partition",new Vector3(0,-.13f,.04f),Vector3.back,new Vector2(.8f,.34f),stone,true,new Vector2(.12f,-.080f),.088f,c.Surfaces);
            var reverseFace=Panel(c.Root,"Interior partition reverse",new Vector3(0,-.13f,.049f),Vector3.forward,new Vector2(.8f,.34f),stone,true,new Vector2(-.12f,-.080f),.088f,c.Surfaces);
            var task=ViewTask(c,"A",new Vector3(-.33f,-.277f,-.05f),Vector3.right,.06f,floor);task.TwoSided=true;
            var inGate=ViewGate(c,"Entry shutter",new Vector3(-.12f,-.200f,.025f),Vector3.right,.24f,new Vector3(.19f,.19f,.016f));
            // Handle is on the top lip, reachable on either side of the partition.
            task.Handle.localPosition=new Vector3(0,.012f,-.035f);task.TouchSize=new Vector3(.085f,.065f,.065f);
            task.AlternateHandle=MechanismVisual(task.transform,"Inside handle",new Vector3(0,.012f,.16f),new Vector3(.065f,.018f,.026f),plastic);
            TapLink(task.transform,"Through-wall control spindle",task.Handle.localPosition,task.AlternateHandle.localPosition);
            var exitGate=ViewGate(c,"Return exit shutter",new Vector3(.34f,-.287f,.21f),Vector3.left,.12f,new Vector3(.11f,.018f,.11f));
            ViewLink(c,task.Rail,inGate,false,entrance);
            var room=new GameObject("Return linkage",typeof(COgheViewMechanism)).GetComponent<COgheViewMechanism>();room.transform.SetParent(c.Root,false);
            room.Input=task.Rail;room.Output=exitGate;room.Reverse=false;room.FinalGate=false;
            var exitRule=exitGate.gameObject.AddComponent<COgheViewReturnExit>();exitRule.Rail=exitGate;exitRule.Aperture=ViewExitSurface(c);exitRule.EntryReverse=reverseFace;exitRule.EntryRail=inGate;
        }
        private static COgheTapRail ViewBridge(ExpansionContext c,VenomSurfacePatch floor)
        {
            // The moving deck sits in the gap, with 6 mm seams to the two banks.
            // It slides along the aisle to meet the receiving bank; no overlapping floor colliders.
            var rail=ExpansionRail(c,"Bridge",new Vector3(0,-.313f,-.17f),Vector3.forward,.30f,0,new Vector3(.198f,.026f,.14f),.035f,.008f,false,true);
            rail.LatchAtStart=rail.LatchAtEnd=true;rail.GetComponent<VenomMovableProp>().Manipulable=false;
            var task=rail.gameObject.AddComponent<COgheTapRail>();task.Rail=rail;task.Handle=rail.GetComponent<VenomMovableProp>().ManipulationGrip;task.WorkingSurface=floor;
            task.Handle.localPosition=new Vector3(-.17f,.025f,0);task.Label=c.Number==10?"B":"A";task.StandOffset=new Vector3(-.07f,0,0);task.PickHandleOnly=true;task.TrackStandPoint=true;task.TouchSize=new Vector3(.080f,.060f,.060f);
            TapLink(rail.transform,"Handle arm",new Vector3(-.08f,.025f,0),task.Handle.localPosition);TapLabel(task.Handle,task.Label,new Vector3(0,.015f,0));
            var docked=Panel(c.Root,"Docked bridge top",new Vector3(0,-.30f,.13f),Vector3.up,new Vector2(.23f,.14f),plastic,false,Vector2.zero,0,c.Surfaces);docked.gameObject.SetActive(false);
            var deck=rail.gameObject.AddComponent<COgheDockedBridgeDeck>();deck.Rail=rail;deck.MovingSurfaces=rail.GetComponentsInChildren<VenomSurfacePatch>();deck.DockedTop=docked;
            // Docked collision keeps the full deck thickness, with only its upper face as a gripping route.
            Object.DestroyImmediate(docked.Shape);var dockCollider=docked.gameObject.AddComponent<BoxCollider>();dockCollider.center=Vector3.back*.013f;dockCollider.size=new Vector3(.23f,.14f,.026f);dockCollider.sharedMaterial=contact;dockCollider.contactOffset=.0003f;docked.Shape=dockCollider;
            // The two docking flanges cover the narrow bank seams only while latched.
            foreach(float side in new[]{-1f,1f})MechanismVisual(docked.transform,"Docking flange",new Vector3(side*.107f,0,0),new Vector3(.020f,.14f,.007f),plastic);
            return task;
        }
    }
}
