using System;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GravityBox.Editor
{
    public sealed class COgheProductUIBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder=>0;
        public void OnPreprocessBuild(BuildReport report)=>Prepare();
        [MenuItem("Gravity Box/COghe/Product UI/Prepare resources")]
        public static void Prepare()
        {
            const string folder="Assets/_Game/Venom/Resources/COgheUI";
            AssetDatabase.Refresh();
            foreach(string name in new[]{"Icons","Panel"})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(folder+"/"+name+".png");
                importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
                importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            string path=folder+"/Catalog.asset";
            var catalog=AssetDatabase.LoadAssetAtPath<COgheProductCatalog>(path);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<COgheProductCatalog>();AssetDatabase.CreateAsset(catalog,path);}
            var definitions=AssetDatabase.FindAssets("t:VenomCampaignDefinition",new[]{VenomCampaignBuilder.SpatialFolder+"/Definitions"})
                .Select(guid=>AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(d=>d.ProgressKey==COgheProductCatalog.ProgressKey).ToArray();
            var names=VenomCampaignBuilder.SpatialSceneNames();
            catalog.Levels=new VenomCampaignDefinition[names.Length];
            for(int i=0;i<names.Length;i++)
            {
                int order=i+1;var matches=definitions.Where(d=>d.Order==order&&d.SceneSequence.Length==names.Length&&d.SceneSequence[i]==names[i]).ToArray();
                if(matches.Length!=1)throw new InvalidOperationException("Product catalog requires exactly one definition at "+order+", found "+matches.Length);
                catalog.Levels[i]=matches[0];
            }
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
            Debug.Log("COGHE PRODUCT UI resources ready: "+catalog.Levels.Length+" stable level IDs");
        }
        [MenuItem("Gravity Box/COghe/Product UI/Build Mac")]
        public static void BuildMac()
        {
            Prepare();Directory.CreateDirectory("Builds/COgheProduct/macOS");
            // Preserve the bundle/save identity; only the presentation and player entry flow change.
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=VenomCampaignBuilder.SpatialScenePaths(),target=BuildTarget.StandaloneOSX,
                locationPathName="Builds/COgheProduct/macOS/COghe.app",options=BuildOptions.Development,
                extraScriptingDefines=new[]{"COGHE_MOBILE_BENCHMARK"}});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Product UI build failed: "+report.summary.result);
            Debug.Log("COGHE PRODUCT MAC BUILD SUCCESS");
        }
    }
}
