using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    // Bake once in the Editor. The visual wall and collision use the same closed mesh.
    public static class CircularExitGeometry
    {
        private const int Segments = 64;
        private const string Folder = "Assets/_Game/Meshes/Exits";
        public const float RimWidth = 0.018f;

        public static Mesh Wall(string name, Vector2 center, float radius, float halfDepth)
        {
            const float extent = 3.05f;
            var angles = new List<float>();
            for (int i = 0; i < Segments; i++) angles.Add(i * Mathf.PI * 2 / Segments);
            // Include exact corners so the outer contour is a complete square, even for an offset hole.
            foreach (float x in new[] { -extent, extent })
                foreach (float y in new[] { -extent, extent })
                    angles.Add(Mathf.Repeat(Mathf.Atan2(y - center.y, x - center.x), Mathf.PI * 2));
            angles.Sort();
            for (int i = angles.Count - 1; i > 0; i--)
                if (angles[i] - angles[i - 1] < 0.00001f) angles.RemoveAt(i);
            var outer = new List<Vector2>();
            var inner = new List<Vector2>();
            foreach (float angle in angles)
            {
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float tx = Mathf.Abs(direction.x) < 0.00001f ? float.PositiveInfinity
                    : ((direction.x > 0 ? extent : -extent) - center.x) / direction.x;
                float ty = Mathf.Abs(direction.y) < 0.00001f ? float.PositiveInfinity
                    : ((direction.y > 0 ? extent : -extent) - center.y) / direction.y;
                outer.Add(center + direction * Mathf.Min(tx, ty));
                inner.Add(center + direction * radius);
            }
            var builder = new Surface();
            for (int i = 0; i < angles.Count; i++)
            {
                int j = (i + 1) % angles.Count;
                builder.Quad(Point(outer[i], halfDepth), Point(outer[j], halfDepth), Point(inner[j], halfDepth), Point(inner[i], halfDepth));
                builder.Quad(Point(inner[i], -halfDepth), Point(inner[j], -halfDepth), Point(outer[j], -halfDepth), Point(outer[i], -halfDepth));
                builder.Quad(Point(inner[i], halfDepth), Point(inner[j], halfDepth), Point(inner[j], -halfDepth), Point(inner[i], -halfDepth));
                builder.Quad(Point(outer[i], -halfDepth), Point(outer[j], -halfDepth), Point(outer[j], halfDepth), Point(outer[i], halfDepth));
            }
            return builder.Save(name);
        }

        public static Mesh Rim(float radius, float halfDepth)
        {
            var builder = new Surface();
            // Flat inlays on the two skins; a tiny render-only offset prevents depth-buffer fighting at grazing angles.
            foreach (int side in new[] { -1, 1 })
                for (int i = 0; i < Segments; i++)
                {
                    Vector2 a = Direction(i), b = Direction(i + 1);
                    float z = side * (halfDepth + 0.003f);
                    if (side > 0)
                        builder.Quad(Point(a * (radius + RimWidth), z), Point(b * (radius + RimWidth), z), Point(b * radius, z), Point(a * radius, z));
                    else
                        builder.Quad(Point(a * radius, z), Point(b * radius, z), Point(b * (radius + RimWidth), z), Point(a * (radius + RimWidth), z));
                }
            return builder.Save("Circular light inlay");
        }

        public static Mesh Shutter(float radius, float halfDepth)
        {
            var builder = new Surface();
            for (int i = 0; i < Segments; i++)
            {
                Vector2 a = Direction(i) * radius, b = Direction(i + 1) * radius;
                builder.Triangle(new Vector3(0, 0, halfDepth), Point(a, halfDepth), Point(b, halfDepth));
                builder.Triangle(new Vector3(0, 0, -halfDepth), Point(b, -halfDepth), Point(a, -halfDepth));
                builder.Quad(Point(a, -halfDepth), Point(b, -halfDepth), Point(b, halfDepth), Point(a, halfDepth));
            }
            return builder.Save("Recessed circular shutter");
        }

        public static GameObject Visual(string name, Transform parent, Mesh mesh, Material material)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return go;
        }

        private static Vector2 Direction(int i) => new Vector2(Mathf.Cos(i * Mathf.PI * 2 / Segments), Mathf.Sin(i * Mathf.PI * 2 / Segments));
        private static Vector3 Point(Vector2 p, float z) => new Vector3(p.x, p.y, z);

        private sealed class Surface
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<int> triangles = new List<int>();
            public void Triangle(Vector3 a, Vector3 b, Vector3 c)
            {
                int n = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c);
                triangles.Add(n); triangles.Add(n + 1); triangles.Add(n + 2);
            }
            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                Triangle(a, b, c); Triangle(a, c, d);
            }
            public Mesh Save(string name)
            {
                if (!Directory.Exists(Folder)) { Directory.CreateDirectory(Folder); AssetDatabase.Refresh(); }
                string path = Folder + "/" + name + ".asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null) { mesh = new Mesh { name = name }; AssetDatabase.CreateAsset(mesh, path); }
                mesh.Clear();
                mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals(); mesh.RecalculateBounds();
                EditorUtility.SetDirty(mesh);
                return mesh;
            }
        }
    }
}
