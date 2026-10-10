using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class BakeryImportCheck
{
    [Serializable] public class Palette { public string name; public Color color; public float smoothness; public string baseColorTexture, normalTexture, maskTexture; public float normalScale; }
    [Serializable] public class Manifest { public Palette[] materials; }
    public static void Run()
    {
        PlayerSettings.colorSpace = ColorSpace.Linear;
        var pipeline = UniversalRenderPipelineAsset.Create();
        pipeline.msaaSampleCount = 4;
        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/Bakery/materials.json"));
        var materials = new Dictionary<string, Material>();
        foreach (var entry in manifest.materials)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", entry.color);
            material.SetFloat("_Smoothness", entry.smoothness);
            if (!string.IsNullOrEmpty(entry.baseColorTexture))
            {
                foreach (string file in new[] {entry.baseColorTexture, entry.normalTexture, entry.maskTexture})
                {
                    var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Bakery/" + file);
                    importer.textureType = file == entry.normalTexture ? TextureImporterType.NormalMap : TextureImporterType.Default;
                    importer.sRGBTexture = file == entry.baseColorTexture;
                    importer.maxTextureSize = 512;
                    importer.mipmapEnabled = true;
                    importer.wrapMode = TextureWrapMode.Repeat;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }
                material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Bakery/" + entry.baseColorTexture));
                material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Bakery/" + entry.normalTexture));
                material.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Bakery/" + entry.maskTexture));
                material.SetFloat("_BumpScale", entry.normalScale);
                material.SetFloat("_Smoothness", 1);
                material.EnableKeyword("_NORMALMAP");
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            materials.Add(entry.name, material);
        }
        var report = new List<string> {"Unity " + Application.unityVersion + "; URP 17.3; isolated asset check, not level integration."};
        string[] names = {"Cherry", "Plate", "PressurePad", "CandyHandle", "GummyLamp", "CreamDrip", "Sprinkle", "ChocolateChip", "MintLeaf"};
        float[] scales = {8, 1, 10, 14, 14, 14, 20, 20, 20};
        int total = 0;
        for (int index = 0; index < names.Length; index++)
        {
            string path = "Assets/Bakery/" + names[index] + ".fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.importAnimation = false;
            importer.addCollider = false;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.isReadable = true;
            importer.SaveAndReimport();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new Exception("Missing imported model: " + path);
            var root = UnityEngine.Object.Instantiate(prefab);
            var bounds = new Bounds();
            bool initialized = false;
            int triangles = 0;
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>())
            {
                if (!initialized) { bounds = renderer.bounds; initialized = true; }
                else bounds.Encapsulate(renderer.bounds);
                var assigned = renderer.sharedMaterials;
                for (int slot = 0; slot < assigned.Length; slot++)
                    assigned[slot] = materials[assigned[slot].name];
                renderer.sharedMaterials = assigned;
            }
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = filter.sharedMesh;
                if (mesh.uv.Length != mesh.vertexCount || mesh.normals.Length != mesh.vertexCount || mesh.tangents.Length != mesh.vertexCount)
                    throw new Exception(names[index] + " missing mesh attributes");
                triangles += mesh.triangles.Length / 3;
            }
            if (triangles > 1500 || root.GetComponentsInChildren<Collider>().Length != 0)
                throw new Exception(names[index] + " exceeds triangle or collider contract");
            total += triangles;
            report.Add(names[index] + ": " + triangles + " triangles; bounds=" + bounds.size.ToString("F6") + "; PASS");
            root.transform.localScale = Vector3.one * scales[index];
            root.transform.position = new Vector3((index - 2.5f) * 1.05f, 0, 0);
        }
        report.Add("TOTAL " + total);
        File.WriteAllLines("UNITY_IMPORT_REPORT.txt", report);
        Debug.Log(string.Join("\n", report));
        EditorApplication.Exit(0);
    }
}
