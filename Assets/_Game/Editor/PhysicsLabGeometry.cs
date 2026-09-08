using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GravityBox.Editor
{
    // Closed baked meshes, in metres. Collider and visible surface share every edge.
    internal static class PhysicsLabGeometry
    {
        public static Mesh Panel(string name, Vector2[] outline, float y, float halfThickness,
            Vector2 hole, float holeRadius = 0)
        {
            var mesh = new Surface();
            if (holeRadius <= 0)
            {
                for (int i = 0; i < outline.Length; i++)
                {
                    Vector2 a = outline[i], b = outline[(i + 1) % outline.Length];
                    mesh.Triangle(P(Vector2.zero, y + halfThickness), P(b, y + halfThickness), P(a, y + halfThickness));
                    mesh.Triangle(P(Vector2.zero, y - halfThickness), P(a, y - halfThickness), P(b, y - halfThickness));
                    mesh.Quad(P(a, y - halfThickness), P(a, y + halfThickness), P(b, y + halfThickness), P(b, y - halfThickness));
                }
            }
            else
            {
                var angles = new List<float>();
                for (int i = 0; i < 96; i++) angles.Add(i * Mathf.PI * 2 / 96);
                foreach (Vector2 vertex in outline)
                    angles.Add(Mathf.Repeat(Mathf.Atan2(vertex.y - hole.y, vertex.x - hole.x), Mathf.PI * 2));
                angles.Sort();
                for (int i = angles.Count - 1; i > 0; i--)
                    if (angles[i] - angles[i - 1] < 0.00001f) angles.RemoveAt(i);
                for (int i = 0; i < angles.Count; i++)
                {
                    Vector2 da = Dir(angles[i]), db = Dir(angles[(i + 1) % angles.Count]);
                    Vector2 a = RayEdge(hole, da, outline), b = RayEdge(hole, db, outline);
                    Vector2 ia = hole + da * holeRadius, ib = hole + db * holeRadius;
                    mesh.Quad(P(b, y + halfThickness), P(a, y + halfThickness), P(ia, y + halfThickness), P(ib, y + halfThickness));
                    mesh.Quad(P(a, y - halfThickness), P(b, y - halfThickness), P(ib, y - halfThickness), P(ia, y - halfThickness));
                    mesh.Quad(P(ia, y + halfThickness), P(ia, y - halfThickness), P(ib, y - halfThickness), P(ib, y + halfThickness));
                    mesh.Quad(P(a, y - halfThickness), P(a, y + halfThickness), P(b, y + halfThickness), P(b, y - halfThickness));
                }
            }
            return mesh.Save(name);
        }

        public static Mesh Border(string name, Vector2[] inner, float width, float bottom, float top)
        {
            var mesh = new Surface();
            // All lab contours are regular polygons centred at the origin.
            float inradius = Vector2.Dot(inner[0], (inner[0] + inner[1]).normalized);
            float factor = 1 + width / inradius;
            for (int i = 0; i < inner.Length; i++)
            {
                Vector2 a = inner[i], b = inner[(i + 1) % inner.Length], oa = a * factor, ob = b * factor;
                mesh.Quad(P(a, top), P(b, top), P(ob, top), P(oa, top));
                mesh.Quad(P(oa, bottom), P(ob, bottom), P(b, bottom), P(a, bottom));
                mesh.Quad(P(a, bottom), P(b, bottom), P(b, top), P(a, top));
                mesh.Quad(P(oa, top), P(ob, top), P(ob, bottom), P(oa, bottom));
            }
            return mesh.Save(name);
        }

        public static Mesh Inlay(float radius, float halfDepth)
        {
            var mesh = new Surface();
            for (int i = 0; i < 96; i++)
            {
                Vector2 a = Dir(i * Mathf.PI * 2 / 96), b = Dir((i + 1) * Mathf.PI * 2 / 96);
                foreach (int sign in new[] { -1, 1 })
                {
                    float z = sign * (halfDepth + 0.00005f);
                    Vector3 p = new Vector3(a.x * radius, a.y * radius, z);
                    Vector3 q = new Vector3(b.x * radius, b.y * radius, z);
                    Vector3 r = new Vector3(b.x * (radius + 0.0007f), b.y * (radius + 0.0007f), z);
                    Vector3 s = new Vector3(a.x * (radius + 0.0007f), a.y * (radius + 0.0007f), z);
                    if (sign < 0) mesh.Quad(p, q, r, s); else mesh.Quad(s, r, q, p);
                }
            }
            return mesh.Save("Flush aperture inlay");
        }

        private static Vector2 RayEdge(Vector2 origin, Vector2 direction, Vector2[] polygon)
        {
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 a = polygon[i], edge = polygon[(i + 1) % polygon.Length] - a;
                float denominator = Cross(direction, edge);
                if (Mathf.Abs(denominator) < 0.000001f) continue;
                float t = Cross(a - origin, edge) / denominator;
                float u = Cross(a - origin, direction) / denominator;
                if (t >= 0 && u >= -0.00001f && u <= 1.00001f) nearest = Mathf.Min(nearest, t);
            }
            return origin + direction * nearest;
        }
        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
        private static Vector2 Dir(float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        private static Vector3 P(Vector2 p, float y) => new Vector3(p.x, y, p.y);

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
            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d) { Triangle(a, b, c); Triangle(a, c, d); }
            public Mesh Save(string name)
            {
                string path = PhysicsLabBuilder.Folder + "/Meshes/" + name + ".asset";
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null) { mesh = new Mesh { name = name }; AssetDatabase.CreateAsset(mesh, path); }
                mesh.Clear(); mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
                var uv = new List<Vector2>();
                foreach (Vector3 vertex in vertices) uv.Add(new Vector2(vertex.x, vertex.z) * 3);
                mesh.SetUVs(0, uv); mesh.RecalculateNormals(); mesh.RecalculateBounds();
                EditorUtility.SetDirty(mesh);
                return mesh;
            }
        }
    }
}
