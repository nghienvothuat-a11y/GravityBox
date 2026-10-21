using System.IO;
using UnityEditor;
using UnityEngine;
namespace GravityBox.Editor
{
    public static class COghePlaytestRefinementBuilder
    {
        public static void BuildNative()
        {
            VenomCampaignBuilder.GenerateSpatialPlus();
            BuildSavedNative();
        }
        public static void BuildSavedNative()
        {
            COgheProductUIBuilder.Prepare();
            var report=UnityEditor.BuildPipeline.BuildPlayer(new UnityEditor.BuildPlayerOptions{
                scenes=VenomCampaignBuilder.SpatialScenePaths(),target=UnityEditor.BuildTarget.StandaloneOSX,
                locationPathName="Builds/COgheProduct/macOS/COghe.app",options=UnityEditor.BuildOptions.None,
                extraScriptingDefines=new[]{"COGHE_MOBILE_BENCHMARK"}});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Refinement Mac build failed");
            Debug.Log("REFINEMENT MAC BUILD SUCCESS");
        }
        // Run with -coghe-spatial-levels 13,22 -coghe-plus-levels B1.
        public static void Apply()
        {
            VenomCampaignBuilder.GenerateSpatialNext();
            VenomCampaignBuilder.GenerateSpatialPlus();
            COgheSoftEdges.RepairDockedArtFromCommandLine();
            VenomCampaignBuilder.RenumberSpatialPlaques();
            COgheProductUIBuilder.Prepare();
            AssetDatabase.SaveAssets();
            Debug.Log("PLAYTEST REFINEMENT SAVED");
        }
    }
}
