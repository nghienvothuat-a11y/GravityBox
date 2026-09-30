using System;
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
        private const string SpecimenRoot="COghe specimen number";
        public static void ApplySpecimenPlate(VenomCampaign game,string catalog=null)
        {
            if(!game.Definition.ViewOnly)return;
            var owner=game.GetComponent<VenomLevelController>();var root=owner.Rotation.transform;
            Remove(root,SpecimenRoot);
            Bounds bounds=default;bool first=true;
            foreach(var p in game.Surfaces)
            {
                if(!p.ExteriorGlass||p.GetComponentInParent<VenomMovableProp>()!=null)continue;
                foreach(float x in new[]{-.5f,.5f})foreach(float y in new[]{-.5f,.5f})
                {
                    var v=root.InverseTransformPoint(p.transform.TransformPoint(new Vector3(p.Size.x*x,p.Size.y*y,0)));
                    if(first){bounds=new Bounds(v,Vector3.zero);first=false;}else bounds.Encapsulate(v);
                }
            }
            if(first)throw new InvalidOperationException("No outer chamber for specimen number");
            string previousDirectory=meshDirectory;int previousSerial=serial;
            try
            {
                meshDirectory=$"Meshes/SpecimenPlates/{(catalog==null?"":catalog+"/")}{(catalog=="Spatial"?SpatialArtKey(game):$"Level{game.Definition.Order:00}")}";serial=0;
                Directory.CreateDirectory(Folder+"/"+meshDirectory);AssetDatabase.Refresh();
                var plastic=DepthMaterial("Specimen ivory resin",AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Warm porcelain.mat"));
                plastic.SetFloat("_Metallic",0);plastic.SetFloat("_Smoothness",.30f);
                var labelInk=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Label ink.mat");
                worldLabels=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/World labels.mat");
                var plate=Child(root,SpecimenRoot);
                // Adhesive-backed specimen plaque, flush to the OUTSIDE of the front glass.
                // Keep its top below the rail, as in the original Day Lab concept.
                var view=game.GetComponent<COgheViewPresentation>();
                float outerDepth=view!=null&&view.Panes.Length>0?.034f:.008f;
                plate.localPosition=new Vector3(bounds.min.x+.085f,bounds.max.y-.044f,bounds.min.z-outerDepth-.002f);
                var hardware=Child(plate,"Glass-mounted plaque");
                Box(hardware,"Ivory plastic plaque",Vector3.zero,new Vector3(.082f,.068f,.004f),.004f,plastic);
                Box(hardware,"Printed specimen underline",new Vector3(0,-.023f,-.0024f),new Vector3(.034f,.0014f,.0003f),.0001f,labelInk);
                CombineByMaterial(hardware);
                Label(plate,game.Definition.Order.ToString("00"),new Vector3(0,.006f,-.0028f),Quaternion.identity,.016f,labelInk);
                var label=plate.GetComponentInChildren<TextMesh>();label.fontStyle=FontStyle.Normal;
                var presentation=game.GetComponent<COgheDayLabPresentation>();
                presentation.WorldTextMaterial=worldLabels;presentation.WorldTextFont=label.font;
                EditorUtility.SetDirty(presentation);
                if(plate.GetComponentsInChildren<Collider>().Length!=0)throw new Exception("Number plate must be decoration only");
            }
            finally{meshDirectory=previousDirectory;serial=previousSerial;}
        }
        [MenuItem("Gravity Box/COghe/Glass preview/Apply approved C and specimen numbers")]
        public static void RebuildSpecimenNumbers()
        {
            const string output="Artifacts/COgheSpecimenPlates";Directory.CreateDirectory(output);
            string before=COgheViewArtVerification.CapturePhysics();
            foreach(var path in VenomCampaignBuilder.ViewCampaignScenePaths())
            {
                var scene=EditorSceneManager.OpenScene(path);var game=Object.FindFirstObjectByType<VenomCampaign>();
                if(game.Definition.Order==1)ApplyGlassPreview(game);else ApplySpecimenPlate(game);
                var study=game.GetComponent<COgheGlassDepthStudy>();if(study!=null){study.ShowComparison=false;EditorUtility.SetDirty(study);}
                var plate=game.GetComponent<VenomLevelController>().Rotation.transform.Find(SpecimenRoot);
                if(plate==null||plate.GetComponentInChildren<TextMesh>().text!=game.Definition.Order.ToString("00"))throw new Exception("Wrong specimen number: "+path);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
            string after=COgheViewArtVerification.CapturePhysics();
            File.WriteAllText(output+"/physics-before.txt",before);File.WriteAllText(output+"/physics-after.txt",after);
            if(before!=after)throw new Exception("Specimen numbers changed physics");
            Debug.Log("COGHE SPECIMEN: 30 numbered scenes; physics identical; C selected for glass pilot.");
        }
        [MenuItem("Gravity Box/COghe/Glass preview/Build approved C and numbered levels Mac")]
        public static void BuildSpecimenMac()
        {
            RebuildSpecimenNumbers();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=VenomCampaignBuilder.ViewCampaignScenePaths(),target=BuildTarget.StandaloneOSX,locationPathName="Builds/GlassLab/macOS/COghe.app",options=BuildOptions.None,extraScriptingDefines=new[]{"COGHE_MOBILE_BENCHMARK"}});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Specimen Mac build failed");
        }
    }
}
