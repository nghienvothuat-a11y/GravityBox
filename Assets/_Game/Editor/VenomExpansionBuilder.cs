using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Simulation;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        private sealed class ExpansionContext
        {
            public int Number;
            public VenomLevelController Owner;
            public VenomCampaign Game;
            public VenomCampaignDefinition Definition;
            public Transform Root;
            public readonly List<VenomSurfacePatch> Surfaces = new List<VenomSurfacePatch>();
            public readonly List<VenomMovableProp> Props = new List<VenomMovableProp>();
            public Vector3 Spawn = new Vector3(-.17f,-.255f,-.16f);
            public Vector3 Exit = new Vector3(.3f,.06f,0);
            public Vector3 Outward = Vector3.right;
        }

        public static string[] CampaignScenePaths()
        {
            var expanded=Campaign40ScenePaths();
            bool expandedComplete=true;foreach(string path in expanded)expandedComplete&=File.Exists(path);
            if(expandedComplete)return expanded;
            var integrated=Campaign30ScenePaths();
            bool complete=true;foreach(string path in integrated)complete&=File.Exists(path);
            if(complete)return integrated;
            return LegacyCampaignScenePaths();
        }

        public static string[] LegacyCampaignScenePaths()
        {
            var paths = new List<string>();
            for (int n=1;n<=LegacyLevelCount;n++)
            {
                string path=Folder+$"/VenomOrigin{n:00}.unity";
                if (!File.Exists(path)) throw new FileNotFoundException("Generate the complete campaign before building",path);
                paths.Add(path);
            }
            return paths.ToArray();
        }

        [MenuItem("Gravity Box/COghe/Generate Expansion 11–20")]
        public static void GenerateExpansion()=>GenerateExpansionRange(11,LegacyLevelCount);

        [MenuItem("Gravity Box/COghe/Rebuild Slide Level 12")]
        public static void GenerateSlideLevel12()=>GenerateExpansionRange(12,12);

        [MenuItem("Gravity Box/COghe/Rebuild Access Level 13")]
        public static void GenerateAccessLevel13()=>GenerateExpansionRange(13,13);

        [MenuItem("Gravity Box/COghe/Rebuild Assembly Bridge Level 16")]
        public static void GenerateAssemblyBridgeLevel16()=>GenerateExpansionRange(16,16);

        [MenuItem("Gravity Box/COghe/Rebuild Cooperation Level 19")]
        public static void GenerateCooperationLevel19()=>GenerateExpansionRange(19,19);

        private static void GenerateExpansionRange(int firstLevel,int lastLevel)
        {
            Directory.CreateDirectory(Folder+"/Meshes");
            Directory.CreateDirectory(Folder+"/Definitions");
            AssetDatabase.Refresh();
            glass=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Glass.mat");
            stone=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Floor.mat");
            mint=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Mint.mat");
            slip=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Slippery.mat");
            metal=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Blade.mat");
            plastic=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Warm resin.mat");
            skin=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Living obsidian.mat");
            contact=AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Folder+"/Contact.physicMaterial");
            slick=AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Folder+"/Slippery.physicMaterial");
            stepContact=AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Folder+"/Weighted step.physicMaterial");
            var profile=AssetDatabase.LoadAssetAtPath<VenomProfile>(Folder+"/Matter.asset");
            if (profile==null||glass==null||contact==null) throw new InvalidOperationException("Origin campaign assets are required");
            for (int n=firstLevel;n<=lastLevel;n++)
            {
                meshSerial=n*1000;
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var owner=new GameObject($"COghe Origin {n:00}").AddComponent<VenomLevelController>();
                owner.MatterProfile=profile;owner.ControlMode=VenomControlMode.TouchSurface;
                owner.RotationProfile=AssetDatabase.LoadAssetAtPath<RotationSettings>("Assets/_Game/PhysicsLab/Profiles/Hand rotation.asset");
                owner.IndicatorMaterial=mint;
                var game=owner.gameObject.AddComponent<VenomCampaign>();
                var definition=Asset<VenomCampaignDefinition>($"Definitions/Level{n:00}.asset",()=>ScriptableObject.CreateInstance<VenomCampaignDefinition>());
                definition.Id=$"venom.origin.{n:00}";definition.Order=n;definition.Title=$"{n:00}";
                definition.Lesson="";definition.CanRotate=false;definition.Boss=n==20;definition.Passive=false;
                definition.CameraEuler=new Vector3(28,25,0);definition.ViewRadius=.48f;
                game.Definition=definition;
                owner.Apparatus=new GameObject("Apparatus").transform;owner.Apparatus.SetParent(owner.transform,false);
                var pivot=new GameObject("Box pivot",typeof(Rigidbody),typeof(BoxRotationController));pivot.transform.SetParent(owner.Apparatus,false);
                var body=pivot.GetComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;
                owner.Rotation=pivot.GetComponent<BoxRotationController>();owner.ApertureRadius=.041f;
                var c=new ExpansionContext{Number=n,Owner=owner,Game=game,Definition=definition,Root=pivot.transform};
                bool built=BuildExpansion11To14(n,c)||BuildExpansion15To16(n,c)||BuildExpansion17To20(n,c);
                if(!built||c.Surfaces.Count==0)throw new InvalidOperationException($"No authored geometry for {n}");
                owner.Spawn=new GameObject("Spawn").transform;owner.Spawn.SetParent(c.Root,false);owner.Spawn.localPosition=c.Spawn;
                owner.Outlet=new GameObject("Final exit").transform;owner.Outlet.SetParent(c.Root,false);owner.Outlet.localPosition=c.Exit;
                owner.Outlet.localRotation=Quaternion.LookRotation(c.Outward,Mathf.Abs(c.Outward.y)>.9f?Vector3.forward:Vector3.up);
                Ring(owner.Outlet,Vector2.zero,owner.ApertureRadius,.0013f,mint);
                game.Surfaces=c.Surfaces.ToArray();game.Props=c.Props.ToArray();
                owner.CrawlFaces=new Collider[6];for(int i=0;i<6;i++)owner.CrawlFaces[i]=game.Surfaces[0].Shape;
                foreach(var prop in c.Props)prop.transform.SetParent(owner.Apparatus,true);
                var camera=new GameObject("Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();
                camera.transform.SetParent(owner.transform,false);camera.tag="MainCamera";
                camera.orthographic=true;camera.orthographicSize=1;camera.nearClipPlane=.005f;camera.farClipPlane=15;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.93f,.93f,.89f);
                camera.transform.rotation=Quaternion.Euler(definition.CameraEuler);camera.transform.position=-camera.transform.forward*3;
                owner.View=camera;
                Lighting();COgheDayLabBuilder.ApplyExpansionLevel(game);
                EditorUtility.SetDirty(definition);
                EditorSceneManager.SaveScene(scene,Folder+$"/VenomOrigin{n:00}.unity");
                Debug.Log($"COGHE EXPANSION AUTHORED {n:00}: {definition.Title}");
            }
            var scenes=new List<EditorBuildSettingsScene>();
            foreach(string path in CampaignScenePaths())scenes.Add(new EditorBuildSettingsScene(path,true));
            EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
            Debug.Log($"COGHE EXPANSION GENERATED: {firstLevel}–{lastLevel}; other scenes retained.");
        }
    }
}
