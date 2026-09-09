using System;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    [Serializable]
    internal sealed class CampaignLevelSpec
    {
        public int index, chapter;
        public string title, role, skill, recovery;
        public bool boss;
        public int[] source_prototypes;
        public float target_d;
    }

    // Authoring context only. Runtime never generates or changes the collision route.
    internal sealed class CampaignBuildContext
    {
        public Material Floor, Glass, Frame, Rim, Amber;
        public PhysicsMaterial Contact;
        public LevelCatalog Lab;

        public CampaignBuildContext(LevelCatalog lab)
        {
            Lab = lab;
            string folder = PhysicsLabBuilder.Folder + "/Materials/";
            Floor = AssetDatabase.LoadAssetAtPath<Material>(folder + "Satin aluminium.mat");
            Glass = AssetDatabase.LoadAssetAtPath<Material>(folder + "Mechanism clear glass.mat");
            Frame = AssetDatabase.LoadAssetAtPath<Material>(folder + "Machined edges.mat");
            Rim = AssetDatabase.LoadAssetAtPath<Material>(folder + "Faint exit inlay.mat");
            Amber = AssetDatabase.LoadAssetAtPath<Material>(folder + "Amber gravity slider.mat");
            Contact = lab.BallProfile.ContactMaterial;
            if (Floor == null || Glass == null || Frame == null || Rim == null || Amber == null)
                throw new InvalidOperationException("Generate the Physics Lab materials before campaign authoring.");
            Floor = CampaignMaterial("Campaign satin graphite", Floor, new Color(.14f,.15f,.15f), .35f, .2f);
            Frame = CampaignMaterial("Campaign brushed brass", Frame, new Color(.38f,.25f,.13f), .65f, .3f);
            Glass = CampaignMaterial("Campaign inspection glass", Glass, Color.white, 0, 0);
            Glass.shader = Shader.Find("GravityBox/Inspection Glass");
            Glass.SetColor("_Tint", new Color(.78f,.78f,.74f,1));
            Glass.SetFloat("_Opacity", .008f); Glass.SetFloat("_EdgeOpacity", .085f);
            Glass.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(Glass);
        }

        private static Material CampaignMaterial(string name, Material source, Color color, float metallic, float smoothness)
        {
            string path = CampaignBuilder.Folder + "/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(source); AssetDatabase.CreateAsset(material, path); }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material); return material;
        }

        public void ApplyCampaignMaterials(LevelRuntime level)
        {
            bool liquid = level.GetComponent<WaterVolume>() != null;
            foreach (Renderer renderer in level.GetComponentsInChildren<Renderer>(true))
            {
                var list = renderer.sharedMaterials;
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i] == null) continue;
                    string name = list[i].name;
                    if (name == "Mechanism clear glass" || name == "Clear cover") list[i] = Glass;
                    else if (name == "Machined edges") list[i] = Frame;
                    else if (name == "Satin aluminium") list[i] = Floor;
                    // Interior liquid baffles need a readable solid silhouette through
                    // caustics/cutaway shading. The shell remains optically clear.
                    if (liquid && level.InteriorDepth < .15f && list[i] == Glass && renderer.GetComponent<BoxCollider>() != null) list[i] = Frame;
                }
                renderer.sharedMaterials = list;
                if (Array.Exists(list, material => material == Glass)) renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        public LevelRuntime ClonePrototype(int oneBased, string name)
        {
            if (oneBased < 1 || oneBased > Lab.Levels.Length) throw new ArgumentOutOfRangeException(nameof(oneBased));
            var level = UnityEngine.Object.Instantiate(Lab.Levels[oneBased - 1].Prefab);
            level.name = name;
            level.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return level;
        }

        public LevelRuntime Planar(string name, Vector2[] outline, Vector2 spawn, Vector2 exit,
            float depth = .09f, Vector2[][] voids = null, float? spawnY = null)
        {
            if (outline == null || outline.Length < 3) throw new ArgumentException("A campaign shell needs an outline.");
            outline = PhysicsLabGeometry.OrientedContour(outline, true);
            voids = voids ?? Array.Empty<Vector2[]>();
            var orientedVoids = new Vector2[voids.Length][];
            for (int i = 0; i < voids.Length; i++) orientedVoids[i] = PhysicsLabGeometry.OrientedContour(voids[i], false);
            voids = orientedVoids;
            var go = new GameObject(name, typeof(Rigidbody), typeof(BoxRotationController), typeof(LevelRuntime));
            Rigidbody body = go.GetComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
            var level = go.GetComponent<LevelRuntime>(); level.Rotation = go.GetComponent<BoxRotationController>();
            level.Footprint = (Vector2[])outline.Clone(); level.InteriorDepth = depth;
            float radius = 0;
            foreach (Vector2 point in outline) radius = Mathf.Max(radius, point.magnitude);
            level.BoundsHalfExtent = Mathf.Sqrt(radius * radius + depth * depth * .25f) + .10f;
            level.FootprintVoids = new Vector2Contour[voids.Length];
            for (int i = 0; i < voids.Length; i++) level.FootprintVoids[i] = new Vector2Contour { Points = (Vector2[])voids[i].Clone() };
            MeshNode("Floor with circular cut", PhysicsLabGeometry.Panel(name + " floor", outline, -depth / 2, .003f, exit, .023f, voids), Floor, go.transform, true);
            MeshNode("Clear top cover", PhysicsLabGeometry.Panel(name + " cover", outline, depth / 2, .003f, Vector2.zero, 0, voids), Glass, go.transform, true);
            MeshNode("Clear side walls", PhysicsLabGeometry.Border(name + " walls", outline, .006f, -depth / 2, depth / 2), Glass, go.transform, true);
            MeshNode("Lower machined edge", PhysicsLabGeometry.Border(name + " lower edge", outline, .007f, -depth / 2 - .003f, -depth / 2 + .001f), Frame, go.transform, false);
            MeshNode("Upper machined edge", PhysicsLabGeometry.Border(name + " upper edge", outline, .007f, depth / 2 - .001f, depth / 2 + .003f), Frame, go.transform, false);
            for (int i = 0; i < voids.Length; i++)
                MeshNode("Inner wall " + i, PhysicsLabGeometry.Border(name + " inner wall " + i, voids[i], .006f, -depth / 2, depth / 2), Glass, go.transform, true);
            var start = new GameObject("BallSpawn"); start.transform.SetParent(go.transform, false);
            start.transform.localPosition = new Vector3(spawn.x, spawnY ?? (-depth / 2 + .003f + .015f + .001f), spawn.y);
            level.BallSpawn = start.transform;
            var outlet = new GameObject("Flush round exit", typeof(ExitSocket)); outlet.transform.SetParent(go.transform, false);
            outlet.transform.localPosition = new Vector3(exit.x, -depth / 2, exit.y);
            outlet.transform.localRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            level.Exit = outlet.GetComponent<ExitSocket>(); level.Exit.ApertureRadius = .023f; level.Exit.WallHalfDepth = .003f;
            MeshNode("Subtle light inlay", PhysicsLabGeometry.Inlay(.023f, .003f), Rim, outlet.transform, false);
            return level;
        }

        private void MeshNode(string name, Mesh mesh, Material material, Transform parent, bool collision)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = material == Glass ? ShadowCastingMode.Off : ShadowCastingMode.On;
            if (!collision) return;
            var collider = go.AddComponent<MeshCollider>(); collider.sharedMesh = mesh;
            collider.sharedMaterial = Contact; collider.contactOffset = .0005f;
        }
    }
}
