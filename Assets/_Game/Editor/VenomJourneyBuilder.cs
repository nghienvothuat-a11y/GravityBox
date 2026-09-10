using System;
using System.IO;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class VenomPrototypeBuilder
    {
        public static readonly string[] JourneyScenes={Folder+"/VenomJourney01.unity",Folder+"/VenomJourney02.unity",Folder+"/VenomJourney03.unity",Folder+"/VenomJourney04.unity",Folder+"/VenomJourney05.unity"};
        [MenuItem("Gravity Box/Venom/Generate Guided Journey 01–05")]
        public static void GenerateJourney()
        {
            contact=AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Folder+"/Materials/Soft contact.physicMaterial");
            glow=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/Mint inlay.mat");
            glass=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/Climbing crystal.mat");
            var amber=Material("Journey warm signals",new Color(.95f,.62f,.24f),.2f,.5f);
            var coverMaterial=Material("Journey jade mechanisms",new Color(.13f,.30f,.28f),.4f,.7f);
            var motion=AssetDatabase.LoadAssetAtPath<VenomLocomotionProfile>(Folder+"/Guided locomotion.asset");
            for(int chapter=1;chapter<=5;chapter++)
            {
                var scene=EditorSceneManager.OpenScene(Scenes[3],OpenSceneMode.Single);
                var owner=Object.FindFirstObjectByType<VenomLevelController>();
                owner.name=$"Venom Journey {chapter:00} — a growing companion";
                owner.LevelNumber=chapter;owner.ControlMode=VenomControlMode.TouchSurface;owner.LocomotionProfile=motion;
                var root=owner.Rotation.transform;
                owner.Guidance=owner.gameObject.AddComponent<VenomGuidance>();owner.Guidance.RequiresButton=chapter>=3;
                owner.Journey=owner.gameObject.AddComponent<VenomJourney>();var journey=owner.Journey;
                owner.Spawn.localPosition=new Vector3(-.12f,-.205f,-.16f);
                if(chapter==1)
                {
                    SetJourneyMesh(owner.CrawlFaces[0],FloorMesh(Vector2.zero,.035f,-.253f,.003f,.25f,"Journey starting floor"));
                    var solid=SolidClimbingFloor();SetJourneyMesh(owner.CrawlFaces[5],solid);
                    owner.CrawlFaces[5].transform.localPosition=Vector3.up*.506f;
                    owner.Outlet.localPosition=new Vector3(0,-.253f,0);owner.Outlet.localRotation=Quaternion.LookRotation(Vector3.down,Vector3.forward);
                    foreach(var line in root.GetComponentsInChildren<LineRenderer>())if(line.name=="Faint flush exit inlay")Object.DestroyImmediate(line.gameObject);
                    Ring(root,new Vector3(0,-.2497f,0),.035f,.0012f);
                }
                if(chapter>=3)
                {
                    journey.PadA=JourneyPad(root,journey.PadALocal,"A",glow);
                    journey.ExitCover=JourneyMotor(root,"Exit shutter",journey.ExitCoverRest,new Vector3(.10f,.006f,.10f),coverMaterial);
                    Block("A to exit inlay",root,new Vector3(-.135f,-.249f,.10f),new Vector3(.005f,.001f,.08f),glow,false);
                }
                if(chapter>=4)
                {
                    journey.PadB=JourneyPad(root,journey.PadBLocal,"B",amber);
                    journey.ButtonCover=JourneyMotor(root,"A opens the cover over B",journey.ButtonCoverRest,new Vector3(.128f,.008f,.128f),coverMaterial);
                    foreach(float x in new[]{.061f,.209f})Block("B cover guide",root,new Vector3(x,-.17f,.175f),new Vector3(.0015f,.16f,.0015f),glow,false);
                    journey.Knife=JourneyMotor(root,"Powered division blade",journey.KnifeLocal+Vector3.up*.09f,new Vector3(.003f,.092f,.13f),AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/Surgical blade.mat"));
                    Block("Warm cutting edge",journey.Knife.transform,new Vector3(0,-.044f,0),new Vector3(.004f,.002f,.13f),amber,false);
                    foreach(float z in new[]{-.194f,-.036f})Block("Blade guide",root,new Vector3(0,-.178f,z),new Vector3(.008f,.144f,.006f),coverMaterial);
                    Block("Cutting approach mark",root,new Vector3(0,-.249f,-.115f),new Vector3(.005f,.001f,.13f),amber,false);
                    Ring(root,new Vector3(0,-.2487f,-.115f),.063f,.001f);
                }
                VenomCameraFraming.Frame(owner,540,960);EditorSceneManager.SaveScene(scene,JourneyScenes[chapter-1]);
            }
            var all=new string[JourneyScenes.Length+Scenes.Length];JourneyScenes.CopyTo(all,0);Scenes.CopyTo(all,JourneyScenes.Length);
            EditorBuildSettings.scenes=Array.ConvertAll(all,p=>new EditorBuildSettingsScene(p,true));
            AssetDatabase.SaveAssets();Debug.Log("JOURNEY GENERATED: 5 guided lessons, persistent skills, independent orders and real contact gates.");
        }
        private static void SetJourneyMesh(Collider collider,Mesh mesh)
        {
            collider.GetComponent<MeshFilter>().sharedMesh=mesh;((MeshCollider)collider).sharedMesh=mesh;
        }
        private static VenomPressurePlate JourneyPad(Transform root,Vector3 p,string name,Material material)
        {
            Vector3 rest=p-Vector3.up*.001f;
            var body=Slider("Contact station "+name,root,rest,new Vector3(.116f,.004f,.116f),material,.008f,Vector3.up,.0003f);
            var pad=body.gameObject.AddComponent<VenomPressurePlate>();pad.Body=body;pad.RestLocal=rest;pad.Light=body.GetComponentInChildren<Renderer>();
            Ring(root,p+Vector3.up*.0015f,.071f,.0012f);
            var label=new GameObject("Station label "+name,typeof(TextMesh));label.transform.SetParent(root,false);
            label.transform.localPosition=p+new Vector3(0,.002f,.047f);label.transform.localRotation=Quaternion.Euler(90,0,0);
            var text=label.GetComponent<TextMesh>();text.text=name;text.fontSize=72;text.characterSize=.012f;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.85f,1,.94f);
            return pad;
        }
        private static Rigidbody JourneyMotor(Transform root,string name,Vector3 p,Vector3 size,Material material)
        {
            var go=new GameObject(name,typeof(Rigidbody));go.transform.SetParent(root,false);go.transform.localPosition=p;
            var body=go.GetComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
            Block("Moving contact surface",go.transform,Vector3.zero,size,material);return body;
        }
        [MenuItem("Gravity Box/Venom/Build Guided Journey macOS")]
        public static void BuildJourneyMac()
        {
            const string path="Builds/Venom/macOS/Venom.app";Directory.CreateDirectory(Path.GetDirectoryName(path));string previous=PlayerSettings.productName;
            try
            {
                PlayerSettings.productName="Venom";
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=JourneyScenes,target=BuildTarget.StandaloneOSX,locationPathName=path,options=BuildOptions.Development});
                if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Journey build failed: "+report.summary.result);
                Debug.Log("JOURNEY BUILD SUCCESS: "+path+" ("+report.summary.totalSize+" bytes)");
            }
            finally{PlayerSettings.productName=previous;}
        }
    }
}
