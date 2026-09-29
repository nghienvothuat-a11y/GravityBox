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
        [MenuItem("Gravity Box/COghe/Glass preview/Rebuild level 01")]
        public static void RebuildGlassPreview()
        {
            Directory.CreateDirectory("Artifacts/COgheGlassPreview");
            string before=COgheViewArtVerification.CapturePhysics();
            var scene=EditorSceneManager.OpenScene("Assets/_Game/Venom/ViewCampaign/COgheView01.unity");
            ApplyGlassPreview(Object.FindFirstObjectByType<VenomCampaign>());
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            string after=COgheViewArtVerification.CapturePhysics();
            File.WriteAllText("Artifacts/COgheGlassPreview/physics-before.txt",before);
            File.WriteAllText("Artifacts/COgheGlassPreview/physics-after.txt",after);
            if(before!=after)throw new Exception("Glass preview changed physics");
            Debug.Log("COGHE GLASS: level 01 restored; all 30 scene physics snapshots identical.");
        }
        public static void ApplyGlassPreview(VenomCampaign game)
        {
            if(game.Definition.Order!=1||!game.Definition.ViewOnly)throw new InvalidOperationException("Glass pilot is V2 01 only");
            meshDirectory="Meshes/GlassPreview01";serial=0;
            Directory.CreateDirectory(Folder+"/"+meshDirectory);AssetDatabase.Refresh();
            var owner=game.GetComponent<VenomLevelController>();var root=owner.Rotation.transform;
            var profile=game.GetComponent<COgheGraphicProfile>();
            if(profile!=null)
            {
                foreach(var go in profile.NewRoots)if(go!=null)Object.DestroyImmediate(go);
                Object.DestroyImmediate(profile);
            }
            var view=game.GetComponent<COgheViewPresentation>();
            if(view!=null)
            {
                foreach(var t in view.PaneVisuals)if(t!=null&&t.GetComponent<VenomSurfacePatch>()==null)Object.DestroyImmediate(t.gameObject);
                // Optical glass uses the existing angle-dependent glass shader, not opaque-wall cutaway.
                view.Panes=Array.Empty<VenomSurfacePatch>();view.PaneVisuals=Array.Empty<Transform>();
                view.FadeSources=Array.Empty<Material>();view.FadeVariants=Array.Empty<Material>();
            }
            Remove(root,"V2 porcelain platform");Remove(root,"Glass preview frame");
            ivory=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Warm porcelain.mat");
            alloy=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Brushed aluminium.mat");
            graphite=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Graphite fittings.mat");
            var clear=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Clear optical glass.mat");
            var tray=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Expansion inspection floor.mat");
            var floors=new System.Collections.Generic.List<Renderer>();
            Bounds bounds=new Bounds();bool first=true;
            foreach(var p in game.Surfaces)
            {
                var r=p.GetComponent<MeshRenderer>();r.enabled=true;
                bool isFloor=p.Normal.y>.98f;r.sharedMaterial=isFloor?tray:clear;
                r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=isFloor;
                if(isFloor)floors.Add(r);
                foreach(float x in new[]{-.5f,.5f})foreach(float y in new[]{-.5f,.5f})
                {
                    Vector3 v=root.InverseTransformPoint(p.transform.TransformPoint(new Vector3(p.Size.x*x,p.Size.y*y,0)));
                    if(first){bounds=new Bounds(v,Vector3.zero);first=false;}else bounds.Encapsulate(v);
                }
            }
            var art=Child(root,"Glass preview frame");
            Vector3 min=bounds.min-Vector3.one*.006f,max=bounds.max+Vector3.one*.006f;
            foreach(float y in new[]{min.y,max.y})foreach(float z in new[]{min.z,max.z})
                Box(art,"Slim aluminium X",new Vector3(bounds.center.x,y,z),new Vector3(max.x-min.x,.007f,.007f),.002f,alloy);
            foreach(float x in new[]{min.x,max.x})foreach(float z in new[]{min.z,max.z})
                Box(art,"Slim aluminium upright",new Vector3(x,bounds.center.y,z),new Vector3(.007f,max.y-min.y,.007f),.002f,alloy);
            foreach(float x in new[]{min.x,max.x})foreach(float y in new[]{min.y,max.y})
                Box(art,"Slim aluminium Z",new Vector3(x,y,bounds.center.z),new Vector3(.007f,.007f,max.z-min.z),.002f,alloy);
            foreach(float x in new[]{min.x,max.x})foreach(float y in new[]{min.y,max.y})foreach(float z in new[]{min.z,max.z})
                Box(art,"Porcelain corner",new Vector3(x,y,z),Vector3.one*.024f,.005f,ivory);
            Box(art,"Thin porcelain tray",new Vector3(bounds.center.x,min.y-.014f,bounds.center.z),new Vector3(bounds.size.x+.032f,.022f,bounds.size.z+.032f),.006f,ivory);
            CombineByMaterial(art);
            var presentation=game.GetComponent<COgheDayLabPresentation>();
            presentation.GlassSurfaces=game.Surfaces;presentation.FadingFloors=floors.ToArray();presentation.FocusOccluders=Array.Empty<Renderer>();
            COgheDayLabPresentation.ConfigureExitOutline(owner);
            var studio=owner.transform.Find("Day Lab studio");
            foreach(var light in studio.GetComponentsInChildren<Light>())
            {
                if(light.shadows!=LightShadows.None){light.color=new Color(1,.97f,.91f);light.intensity=1.1f;light.shadowStrength=.20f;light.shadowBias=.015f;light.shadowNormalBias=.10f;}
                else{light.color=new Color(.81f,.90f,1);light.intensity=.45f;}
            }
            ConfigureGlassDepthStudy(game);
            ApplySpecimenPlate(game);
            EditorUtility.SetDirty(presentation);EditorUtility.SetDirty(owner);
        }
        public static void BuildGlassPreviewMac()
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=VenomCampaignBuilder.ViewCampaignScenePaths(),target=BuildTarget.StandaloneOSX,locationPathName="Builds/GlassPreview/macOS/COghe.app",options=BuildOptions.None,extraScriptingDefines=new[]{"COGHE_MOBILE_BENCHMARK"}});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Glass Mac build failed");
        }
        public static void RebuildAndBuildGlassPreview(){RebuildGlassPreview();BuildGlassPreviewMac();}
    }
}
