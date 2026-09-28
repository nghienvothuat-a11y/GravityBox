using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        public static void ConfigureViewFade(COgheViewPresentation view)
        {
            var sources = new List<Material>(); var variants = new List<Material>();
            foreach (var pane in view.PaneVisuals) foreach (var r in pane.GetComponentsInChildren<Renderer>())
                foreach (var source in r.sharedMaterials)
                {
                    if (sources.Contains(source)) continue;
                    string path = Folder + "/" + source.name + " cutaway fade.mat";
                    var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (material == null) { material = new Material(source); AssetDatabase.CreateAsset(material, path); }
                    material.CopyPropertiesFromMaterial(source); material.name = source.name + " cutaway fade";
                    material.SetFloat("_Surface", 1); material.SetFloat("_Blend", 0); material.SetFloat("_AlphaClip", 0);
                    material.SetFloat("_BlendModePreserveSpecular", 0);
                    material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One); material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_ZWrite", 0); material.SetOverrideTag("RenderType", "Transparent"); material.renderQueue = (int)RenderQueue.Transparent;
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON"); material.DisableKeyword("_ALPHATEST_ON");
                    material.SetShaderPassEnabled("DepthOnly", false); material.SetShaderPassEnabled("DepthNormals", false); material.SetShaderPassEnabled("ShadowCaster", false);
                    sources.Add(source); variants.Add(material); EditorUtility.SetDirty(material);
                }
            view.FadeSources = sources.ToArray(); view.FadeVariants = variants.ToArray(); EditorUtility.SetDirty(view);
        }
    }
    public static partial class VenomCampaignBuilder
    {
        [MenuItem("Gravity Box/COghe/V2/Update zoom lesson and smooth cutaway")]
        public static void RebuildViewFeedbackAndZoomMac()
        {
            PrepareCampaign30Assets(); string previous = authoredMeshFolder;
            try { authoredMeshFolder = ViewFolder + "/Meshes"; BuildViewLevel(4); }
            finally { authoredMeshFolder = previous; }
            string before = COgheViewArtVerification.CapturePhysics();
            foreach (string path in ViewCampaignScenePaths())
            {
                var scene = EditorSceneManager.OpenScene(path);
                COgheDayLabBuilder.ConfigureViewFade(Object.FindFirstObjectByType<COgheViewPresentation>());
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
            string after = COgheViewArtVerification.CapturePhysics();
            const string folder = "Artifacts/COgheViewExpansion"; Directory.CreateDirectory(folder);
            File.WriteAllText(folder + "/fade-physics-before.txt", before); File.WriteAllText(folder + "/fade-physics-after.txt", after);
            if (before != after) throw new System.InvalidOperationException("Cutaway setup changed physics/input serialization");
            Debug.Log("COGHE FADE PHYSICS VERIFIED: all 30 scenes unchanged by presentation setup.");
            BuildMac();
        }
    }
}
