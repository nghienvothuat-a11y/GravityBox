using System;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;
namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        private const string DepthFolder="Assets/_Game/Venom/Art/GlassDepth";
        private static Material DepthMaterial(string name,Material source,Shader shader=null)
        {
            string path=DepthFolder+"/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=source!=null?new Material(source):new Material(shader);AssetDatabase.CreateAsset(m,path);}
            else if(source!=null)m.CopyPropertiesFromMaterial(source);
            if(shader!=null)m.shader=shader; m.name=name;EditorUtility.SetDirty(m);return m;
        }
        private static void ConfigureGlassDepthStudy(VenomCampaign game,VenomSurfacePatch[] surfaces=null)
        {
            Directory.CreateDirectory(DepthFolder);AssetDatabase.Refresh();
            var study=game.GetComponent<COgheGlassDepthStudy>();if(study==null)study=game.gameObject.AddComponent<COgheGlassDepthStudy>();
            var shader=Shader.Find("COghe/Depth Study Glass");
            if(shader==null)throw new Exception("Depth glass shader missing");
            var clear=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Clear optical glass.mat");
            var floor=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Expansion inspection floor.mat");
            var cube=AssetDatabase.LoadAssetAtPath<Cubemap>(Folder+"/Day studio reflection.asset");
            study.ClearLit=DepthMaterial("A glass",clear,shader);study.FloorLit=DepthMaterial("A floor",floor,shader);
            study.ClearReflected=DepthMaterial("B glass",clear,shader);study.FloorReflected=DepthMaterial("B floor",floor,shader);
            foreach(var m in new[]{study.ClearLit,study.FloorLit,study.ClearReflected,study.FloorReflected})
            {m.SetTexture("_StudioCube",cube);m.SetFloat("_ContactAlpha",0);m.SetFloat("_ReflectionStrength",m==study.ClearReflected?.45f:m==study.FloorReflected?.12f:0);m.SetFloat("_Floor",m==study.FloorLit||m==study.FloorReflected?1:0);}
            var owner=game.GetComponent<VenomLevelController>();
            study.Surfaces=(surfaces??game.Surfaces).Select(p=>p.GetComponent<Renderer>()).ToArray();
            study.Frame=owner.Rotation.transform.Find("Glass preview frame").GetComponentsInChildren<Renderer>();
            study.RefinedFrame=new Material[study.Frame.Length];
            for(int i=0;i<study.Frame.Length;i++)
            {
                var source=study.Frame[i].sharedMaterial;var m=DepthMaterial("B "+source.name,source);
                bool metal=source.name.Contains("aluminium");m.SetFloat("_Smoothness",metal?.50f:.28f);m.SetFloat("_Metallic",metal?.55f:.04f);study.RefinedFrame[i]=m;
            }
            study.Creature=DepthMaterial("B creature",owner.MatterProfile.Skin);study.Creature.SetFloat("_Smoothness",.68f);study.Creature.SetFloat("_Metallic",.20f);
            var studio=owner.transform.Find("Day Lab studio");
            study.Bench=studio.GetComponentsInChildren<Renderer>().First(r=>r.bounds.size.x>8);
            study.LabBackground=DepthMaterial("C lab background",null,Shader.Find("COghe/Depth Study Lab"));
            study.Key=studio.GetComponentsInChildren<Light>().First(l=>l.shadows!=LightShadows.None);
            study.Fill=studio.GetComponentsInChildren<Light>().First(l=>l.shadows==LightShadows.None);
            EditorUtility.SetDirty(study);
        }
        [MenuItem("Gravity Box/COghe/Glass preview/Build depth comparison Mac")]
        public static void BuildGlassDepthMac()
        {
            RebuildGlassPreview();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=VenomCampaignBuilder.ViewCampaignScenePaths(),target=BuildTarget.StandaloneOSX,locationPathName="Builds/GlassDepth/macOS/COghe.app",options=BuildOptions.None,extraScriptingDefines=new[]{"COGHE_MOBILE_BENCHMARK"}});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Depth Mac build failed");
        }
    }
}
