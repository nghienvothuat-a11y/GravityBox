using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
    /// <summary>Repeatable, isolated pilot; never rewrites the sixty production scenes or definitions.</summary>
    public static class COgheOnboardingBuilder
    {
        public const string Folder="Assets/_Game/Venom/OnboardingPilot";
        public const string SaveKey="coghe.onboarding.pilot.v1";
        public static readonly int[] SourceSlots={1,2,41,8,14,18,9,16,42,10};
        public static string[] ScenePaths()
        {
            var paths=new string[SourceSlots.Length];
            for(int i=0;i<paths.Length;i++)paths[i]=$"{Folder}/COgheLearn{i+1:00}.unity";
            return paths;
        }
        [MenuItem("Gravity Box/COghe/Onboarding/Generate isolated ten-level pilot")]
        public static void Generate()
        {
            foreach(int slot in SourceSlots)
                if(!File.Exists(Source(slot)))throw new FileNotFoundException("Pilot source is required",Source(slot));
            Directory.CreateDirectory(Folder+"/Definitions");AssetDatabase.Refresh();
            var paths=ScenePaths();var sequence=Array.ConvertAll(paths,Path.GetFileNameWithoutExtension);
            for(int i=0;i<SourceSlots.Length;i++)
            {
                var scene=EditorSceneManager.OpenScene(Source(SourceSlots[i]),OpenSceneMode.Single);
                var game=Object.FindFirstObjectByType<VenomCampaign>();
                if(game==null)throw new InvalidOperationException("Missing campaign in "+Source(SourceSlots[i]));
                var original=game.Definition;
                string path=$"{Folder}/Definitions/Lesson{i+1:00}.asset";
                var definition=AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(path);
                if(definition==null){definition=ScriptableObject.CreateInstance<VenomCampaignDefinition>();AssetDatabase.CreateAsset(definition,path);}
                EditorUtility.CopySerialized(original,definition);
                definition.name=$"Lesson{i+1:00}";definition.Order=i+1;
                int titleStart=original.Title.IndexOf(" · ",StringComparison.Ordinal);
                definition.Title=$"{i+1:00} · "+(titleStart>=0?original.Title.Substring(titleStart+3):original.Title);
                definition.SceneSequence=(string[])sequence.Clone();definition.ProgressKey=SaveKey;
                // The first two lessons have verified tap-only paths. Rotation is taught in the following chapter.
                if(i<2)definition.CanRotate=false;
                game.Definition=definition;game.name=$"COghe learn {i+1:00} · {definition.Id}";
                var lesson=game.gameObject.AddComponent<COgheOnboarding>();
                Configure(lesson,game,SourceSlots[i]);
                foreach(var label in game.GetComponentsInChildren<TextMesh>(true))
                    if(label.text==SourceSlots[i].ToString("00"))label.text=(i+1).ToString("00");
                EditorUtility.SetDirty(definition);EditorUtility.SetDirty(game);EditorUtility.SetDirty(lesson);
                if(!EditorSceneManager.SaveScene(scene,paths[i],true))throw new IOException("Could not save "+paths[i]);
                Debug.Log($"COGHE PILOT {i+1:00}: source {SourceSlots[i]:00}, stable ID {definition.Id}");
            }
            var scenes=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach(string path in paths)
                if(!scenes.Exists(s=>s.path==path))scenes.Add(new EditorBuildSettingsScene(path,true));
            EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
            // Save-as-copy leaves the source scene modified in memory. End in a pilot scene
            // so a subsequent manual Save cannot accidentally turn a production source into the pilot.
            EditorSceneManager.OpenScene(paths[0],OpenSceneMode.Single);
            Debug.Log("COGHE PILOT GENERATED: ten isolated scenes; production catalog/save preserved.");
        }
        private static string Source(int slot)=>$"{VenomCampaignBuilder.Campaign30Folder}/COgheOrigin{slot:00}.unity";
        private static void Configure(COgheOnboarding lesson,VenomCampaign game,int slot)
        {
            switch(slot)
            {
                case 1:lesson.Lesson=COgheOnboarding.LessonKind.Exit;break;
                // Keep the teaching point below the selectable roof's projected edge.
                case 2:lesson.Lesson=COgheOnboarding.LessonKind.Climb;lesson.LocalDestination=new Vector3(-.16f,-.06f,.3f);break;
                case 41:case 42:
                    lesson.Lesson=COgheOnboarding.LessonKind.Tap;
                    foreach(var tap in game.GetComponentsInChildren<COgheTapRail>())
                    {if(tap.Label=="A")lesson.TapA=tap;else if(tap.Label=="B")lesson.TapB=tap;}
                    if(lesson.TapA==null||slot==42&&lesson.TapB==null)throw new InvalidOperationException("Missing tap lesson handles");
                    break;
                case 8:
                    lesson.Lesson=COgheOnboarding.LessonKind.Crate;lesson.Primary=game.Props[0];
                    // Avoid aiming exactly along the radial triangle seam below the circular hole.
                    lesson.LocalDestination=new Vector3(.3f,-.025f,.008f);break;
                case 16:
                    lesson.Lesson=COgheOnboarding.LessonKind.Sequence;lesson.Sequence=game.GetComponentInChildren<COgheSequentialWinch>();
                    lesson.PrimaryRail=lesson.Sequence.Access;lesson.SecondaryRail=lesson.Sequence.Handle;
                    lesson.Primary=lesson.PrimaryRail.GetComponent<VenomMovableProp>();lesson.Secondary=lesson.SecondaryRail.GetComponent<VenomMovableProp>();break;
                case 18:
                    lesson.Lesson=COgheOnboarding.LessonKind.Bridge;lesson.Bridge=game.GetComponentInChildren<COgheDockedBridgeDeck>();
                    lesson.PrimaryRail=lesson.Bridge.Rail;lesson.Primary=lesson.PrimaryRail.GetComponent<VenomMovableProp>();break;
                case 14:
                    lesson.Lesson=COgheOnboarding.LessonKind.Rail;lesson.PrimaryRail=game.GetComponentInChildren<COgheExitRailLock>().Rail;
                    lesson.Primary=lesson.PrimaryRail.GetComponent<VenomMovableProp>();break;
                case 9:
                    lesson.Lesson=COgheOnboarding.LessonKind.Rail;
                    lesson.Primary=Array.Find(game.Props,p=>p.name=="G sliding gear");lesson.PrimaryRail=lesson.Primary.GetComponent<COgheRailSlider>();break;
                case 10:lesson.Lesson=COgheOnboarding.LessonKind.Boss;break;
                default:throw new ArgumentOutOfRangeException(nameof(slot));
            }
        }
        [MenuItem("Gravity Box/COghe/Onboarding/Build pilot macOS")]
        public static void BuildMac()
        {
            var paths=ScenePaths();foreach(string path in paths)if(!File.Exists(path))throw new FileNotFoundException("Generate pilot first",path);
            const string output="Builds/COgheOnboarding/macOS/COghe Learn.app";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            string previous=PlayerSettings.productName;
            try
            {
                PlayerSettings.productName="Venom";
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=paths,target=BuildTarget.StandaloneOSX,locationPathName=output,options=BuildOptions.Development});
                if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Pilot build failed: "+report.summary.result);
                Debug.Log("COGHE PILOT BUILD SUCCESS: "+output);
            }
            finally{PlayerSettings.productName=previous;}
        }
    }
}
