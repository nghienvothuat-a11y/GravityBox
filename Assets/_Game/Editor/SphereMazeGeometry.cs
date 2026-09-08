using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    internal static class SphereMazeGeometry
    {
        private const int LongitudeSegments = 96, LatitudeSegments = 64;

        public static Mesh RibbonSection(string name, IList<Bounds> ribbons)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            foreach (Bounds ribbon in ribbons)
                for (int axis = 0; axis < 3; axis++)
                    foreach (int sign in new[] { -1, 1 })
                    {
                        int u = (axis + 1) % 3, v = (axis + 2) % 3;
                        Vector3 normal = Vector3.zero; normal[axis] = sign;
                        Vector3 middle = ribbon.center; middle[axis] += sign * ribbon.extents[axis];
                        int n = vertices.Count;
                        foreach (Vector2 corner in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, 1) })
                        {
                            Vector3 point = middle;
                            point[u] += corner.x * ribbon.extents[u]; point[v] += corner.y * ribbon.extents[v];
                            vertices.Add(point);
                        }
                        bool forward = Vector3.Dot(Vector3.Cross(vertices[n + 1] - vertices[n], vertices[n + 2] - vertices[n]), normal) > 0;
                        if (forward) triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
                        else triangles.AddRange(new[] { n, n + 2, n + 1, n, n + 3, n + 2 });
                    }
            return Save(name, vertices, triangles);
        }

        // Thin visual inlays follow the actual plank edges. They have no collider
        // and share one draw mesh, so they cannot fence off the open space.
        public static Mesh PlankEdges(string name, IList<Matrix4x4> poses, IList<Vector3> sizes)
        {
            const float width = .00035f;
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int plank = 0; plank < poses.Count; plank++)
            {
                int lengthAxis = 0, thicknessAxis = 0;
                for (int axis = 1; axis < 3; axis++)
                {
                    if (sizes[plank][axis] > sizes[plank][lengthAxis]) lengthAxis = axis;
                    if (sizes[plank][axis] < sizes[plank][thicknessAxis]) thicknessAxis = axis;
                }
                int crossAxis = 3 - lengthAxis - thicknessAxis;
                // Two fine long-edge accents make each ribbon readable without
                // drawing a bright wire cage around every short component.
                foreach (int sign in new[] { -1, 1 })
                {
                    Vector3 size = Vector3.one * width, centre = Vector3.zero;
                    size[lengthAxis] = sizes[plank][lengthAxis];
                    centre[crossAxis] = sign * (sizes[plank][crossAxis] - width) * .5f;
                    centre[thicknessAxis] = (sizes[plank][thicknessAxis] - width) * .5f;
                    AddBox(poses[plank], centre, size);
                }
            }
            return Save(name, vertices, triangles);

            void AddBox(Matrix4x4 pose, Vector3 centre, Vector3 size)
            {
                for (int axis = 0; axis < 3; axis++)
                    foreach (int sign in new[] { -1, 1 })
                    {
                        int u = (axis + 1) % 3, v = (axis + 2) % 3;
                        Vector3 normal = Vector3.zero; normal[axis] = sign;
                        Vector3 middle = centre; middle[axis] += sign * size[axis] * .5f;
                        int n = vertices.Count;
                        foreach (Vector2 corner in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, 1) })
                        {
                            Vector3 point = middle;
                            point[u] += corner.x * size[u] * .5f; point[v] += corner.y * size[v] * .5f;
                            vertices.Add(pose.MultiplyPoint3x4(point));
                        }
                        bool forward = Vector3.Dot(Vector3.Cross(vertices[n + 1] - vertices[n], vertices[n + 2] - vertices[n]), pose.MultiplyVector(normal)) > 0;
                        if (forward) triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
                        else triangles.AddRange(new[] { n, n + 2, n + 1, n, n + 3, n + 2 });
                    }
            }
        }

        // The inner/outer skins terminate at the same cylindrical bore radius.
        // Separate bore vertices preserve its inward-facing cylinder normals.
        public static Mesh Shell(float innerRadius, float thickness, float aperture)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            int innerRing = AddSkin(innerRadius, -1);
            int outerRing = AddSkin(innerRadius + thickness, 1);
            int bore = vertices.Count;
            for (int i = 0; i < LongitudeSegments; i++)
            {
                Vector3 inward = new Vector3(-Mathf.Cos(i * Mathf.PI * 2 / LongitudeSegments), 0,
                    -Mathf.Sin(i * Mathf.PI * 2 / LongitudeSegments));
                vertices.Add(vertices[innerRing + i]); normals.Add(inward);
                vertices.Add(vertices[outerRing + i]); normals.Add(inward);
            }
            for (int i = 0; i < LongitudeSegments; i++)
            {
                int a = bore + 2 * i, b = bore + 2 * ((i + 1) % LongitudeSegments);
                Vector3 normal = (normals[a] + normals[b]).normalized;
                Triangle(a, b, b + 1, normal); Triangle(a, b + 1, a + 1, normal);
            }
            return Save("SphereMaze hollow spherical shell", vertices, triangles, normals);

            int AddSkin(float radius, int orientation)
            {
                int pole = vertices.Count;
                vertices.Add(Vector3.up * radius); normals.Add(Vector3.up * orientation);
                float end = Mathf.PI - Mathf.Asin(aperture / radius);
                for (int latitude = 1; latitude <= LatitudeSegments; latitude++)
                {
                    float theta = end * latitude / LatitudeSegments;
                    float y = Mathf.Cos(theta) * radius;
                    float radial = latitude == LatitudeSegments ? aperture : Mathf.Sin(theta) * radius;
                    for (int longitude = 0; longitude < LongitudeSegments; longitude++)
                    {
                        float phi = longitude * Mathf.PI * 2 / LongitudeSegments;
                        Vector3 point = new Vector3(radial * Mathf.Cos(phi), y, radial * Mathf.Sin(phi));
                        vertices.Add(point); normals.Add(point.normalized * orientation);
                    }
                }
                for (int longitude = 0; longitude < LongitudeSegments; longitude++)
                    Triangle(pole, pole + 1 + longitude, pole + 1 + (longitude + 1) % LongitudeSegments,
                        Vector3.up * orientation);
                for (int latitude = 0; latitude < LatitudeSegments - 1; latitude++)
                    for (int longitude = 0; longitude < LongitudeSegments; longitude++)
                    {
                        int next = (longitude + 1) % LongitudeSegments;
                        int a = pole + 1 + latitude * LongitudeSegments + longitude;
                        int b = pole + 1 + latitude * LongitudeSegments + next;
                        int c = b + LongitudeSegments, d = a + LongitudeSegments;
                        Vector3 normal = (vertices[a] + vertices[b] + vertices[c] + vertices[d]).normalized * orientation;
                        Triangle(a, b, c, normal); Triangle(a, c, d, normal);
                    }
                return pole + 1 + (LatitudeSegments - 1) * LongitudeSegments;
            }

            void Triangle(int a, int b, int c, Vector3 desiredNormal)
            {
                triangles.Add(a);
                if (Vector3.Dot(Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]), desiredNormal) >= 0)
                { triangles.Add(b); triangles.Add(c); }
                else { triangles.Add(c); triangles.Add(b); }
            }
        }

        public static Mesh Ring(string name, float radius, float width, float halfDepth)
        {
            var vertices = new List<Vector3>(); var indices = new List<int>();
            foreach (float sign in new[] { -1f, 1f })
                for (int i = 0; i < LongitudeSegments; i++)
                {
                    float a = i * Mathf.PI * 2 / LongitudeSegments, b = (i + 1) * Mathf.PI * 2 / LongitudeSegments;
                    int n = vertices.Count;
                    float y = sign * (halfDepth + .00005f);
                    vertices.Add(new Vector3(Mathf.Cos(a) * radius, y, Mathf.Sin(a) * radius));
                    vertices.Add(new Vector3(Mathf.Cos(b) * radius, y, Mathf.Sin(b) * radius));
                    vertices.Add(new Vector3(Mathf.Cos(b) * (radius + width), y, Mathf.Sin(b) * (radius + width)));
                    vertices.Add(new Vector3(Mathf.Cos(a) * (radius + width), y, Mathf.Sin(a) * (radius + width)));
                    if (sign > 0) indices.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
                    else indices.AddRange(new[] { n, n + 2, n + 1, n, n + 3, n + 2 });
                }
            return Save(name, vertices, indices);
        }

        private static Mesh Save(string name, List<Vector3> vertices, List<int> triangles, List<Vector3> normals = null)
        {
            string path = PhysicsLabBuilder.Folder + "/Meshes/" + name + ".asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) { mesh = new Mesh { name = name }; AssetDatabase.CreateAsset(mesh, path); }
            mesh.Clear(); mesh.indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
            if (normals == null) mesh.RecalculateNormals(); else mesh.SetNormals(normals);
            mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh);
            return mesh;
        }
    }
}
