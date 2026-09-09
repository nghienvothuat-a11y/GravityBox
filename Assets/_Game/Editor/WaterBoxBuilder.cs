using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    public static class WaterBoxBuilder
    {
        public static void AddWater(GameObject root) => AddLiquid(root, false);
        public static void AddMercury(GameObject root) => AddLiquid(root, true);

        private static void AddLiquid(GameObject root, bool mercury)
        {
            string profilePath = PhysicsLabBuilder.Folder + (mercury ? "/Profiles/Room temperature mercury.asset" : "/Profiles/Room temperature water.asset");
            WaterProfile profile = AssetDatabase.LoadAssetAtPath<WaterProfile>(profilePath);
            if (profile == null) { profile = ScriptableObject.CreateInstance<WaterProfile>(); AssetDatabase.CreateAsset(profile, profilePath); }
            if (mercury)
            {
                profile.Density = 13546;
                // Assael et al. 2012, eq. 2/table 5, at 293.15 K, converted mPa.s -> Pa.s.
                profile.DynamicViscosity = .001f * Mathf.Pow(10, -.2561f + 132.29f / 293.15f);
                profile.EffectiveRoughness = .000003f;
                profile.EntrainmentSeconds = .4f; // Shared approximate flow response, not calibrated mercury spin-up.
                EditorUtility.SetDirty(profile);
            }
            WaterVolume water = root.AddComponent<WaterVolume>(); water.Profile = profile;
            WaterVisuals visuals = root.AddComponent<WaterVisuals>();
            visuals.MercuryCutaway = mercury;
            Material clear = Material(mercury ? "Mercury enclosure glass" : "Water enclosure glass", "GravityBox/Retained Water");
            clear.SetColor("_Tint", mercury ? new Color(.74f, .77f, .8f, .35f) : new Color(.5f, .75f, .8f, .45f)); EditorUtility.SetDirty(clear);
            root.transform.Find("Clear side walls").GetComponent<Renderer>().sharedMaterial = clear;
            root.transform.Find("Clear top cover").GetComponent<Renderer>().sharedMaterial = clear;
            visuals.FloorRenderer = root.transform.Find("Floor with circular cut").GetComponent<Renderer>();
            if (mercury)
            {
                Material floor = Material("Mercury cutaway floor", "Universal Render Pipeline/Lit");
                floor.SetColor("_BaseColor", new Color(.24f, .27f, .3f));
                floor.SetFloat("_Metallic", .7f); floor.SetFloat("_Smoothness", .7f); EditorUtility.SetDirty(floor);
                visuals.FloorRenderer.sharedMaterial = floor;
            }
            else visuals.FloorRenderer.sharedMaterial = Material("Water illuminated floor", "GravityBox/Underwater Caustics");
            GameObject volume = GameObject.CreatePrimitive(PrimitiveType.Cube);
            volume.name = mercury ? "Retained mercury - see-through visualization" : "Retained full water volume";
            Object.DestroyImmediate(volume.GetComponent<Collider>());
            volume.transform.SetParent(root.transform, false); volume.transform.localScale = water.HalfSize * 2;
            visuals.VolumeRenderer = volume.GetComponent<Renderer>();
            visuals.VolumeRenderer.sharedMaterial = mercury
                ? Material("Mercury see-through volume", "GravityBox/Mercury Cutaway")
                : Material("Full water optical volume", "GravityBox/Retained Water");
            visuals.VolumeRenderer.shadowCastingMode = ShadowCastingMode.Off; visuals.VolumeRenderer.receiveShadows = false;
            visuals.TracerMaterial = Material("Water suspended optical tracers", "GravityBox/Water Tracers");
        }

        private static Material Material(string name, string shader)
        {
            string path = PhysicsLabBuilder.Folder + "/Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find(shader)); AssetDatabase.CreateAsset(material, path); }
            return material;
        }
    }
}
