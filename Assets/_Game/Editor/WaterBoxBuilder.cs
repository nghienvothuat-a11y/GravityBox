using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    public static class WaterBoxBuilder
    {
        public static void AddWater(GameObject root)
        {
            const string profilePath = PhysicsLabBuilder.Folder + "/Profiles/Room temperature water.asset";
            WaterProfile profile = AssetDatabase.LoadAssetAtPath<WaterProfile>(profilePath);
            if (profile == null) { profile = ScriptableObject.CreateInstance<WaterProfile>(); AssetDatabase.CreateAsset(profile, profilePath); }
            WaterVolume water = root.AddComponent<WaterVolume>(); water.Profile = profile;
            WaterVisuals visuals = root.AddComponent<WaterVisuals>();
            Material clear = Material("Water enclosure glass", "GravityBox/Retained Water");
            clear.SetColor("_Tint", new Color(.5f, .75f, .8f, .45f)); EditorUtility.SetDirty(clear);
            root.transform.Find("Clear side walls").GetComponent<Renderer>().sharedMaterial = clear;
            root.transform.Find("Clear top cover").GetComponent<Renderer>().sharedMaterial = clear;
            visuals.FloorRenderer = root.transform.Find("Floor with circular cut").GetComponent<Renderer>();
            visuals.FloorRenderer.sharedMaterial = Material("Water illuminated floor", "GravityBox/Underwater Caustics");
            GameObject volume = GameObject.CreatePrimitive(PrimitiveType.Cube); volume.name = "Retained full water volume";
            Object.DestroyImmediate(volume.GetComponent<Collider>());
            volume.transform.SetParent(root.transform, false); volume.transform.localScale = water.HalfSize * 2;
            visuals.VolumeRenderer = volume.GetComponent<Renderer>();
            visuals.VolumeRenderer.sharedMaterial = Material("Full water optical volume", "GravityBox/Retained Water");
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
