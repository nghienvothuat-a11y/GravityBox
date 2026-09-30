using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>
    /// A tiny flat-shaded low-poly kit for the Home furniture: every item is built from code at runtime (lathes, lofts,
    /// boxes, rods, ellipses, icospheres) in a shared colour palette, so the furniture costs no model or texture assets.
    /// Faces carry their own normals (the faceted look of the concepts); each colour is a submesh on a shared material.
    /// </summary>
    public sealed class COgheLowPoly
    {
        private readonly List<Vector3> vertices = new List<Vector3>(512), normals = new List<Vector3>(512);
        private readonly List<Color> paints = new List<Color>(8);
        private readonly List<List<int>> triangles = new List<List<int>>(8);
        private Matrix4x4 place = Matrix4x4.identity;
        private int paint;

        public COgheLowPoly Paint(Color color)
        {
            int i = paints.IndexOf(color);
            if (i < 0) { paints.Add(color); triangles.Add(new List<int>(96)); i = paints.Count - 1; }
            paint = i; return this;
        }
        public COgheLowPoly Paint(string hex) => Paint(Hex(hex));
        public COgheLowPoly At(Vector3 position, Quaternion rotation, Vector3 scale) { place = Matrix4x4.TRS(position, rotation, scale); return this; }
        public COgheLowPoly At(Vector3 position) => At(position, Quaternion.identity, Vector3.one);
        public COgheLowPoly At(Vector3 position, Quaternion rotation) => At(position, rotation, Vector3.one);

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex.StartsWith("#") ? hex : "#" + hex, out var c); return c;
        }

        // Faces ---------------------------------------------------------------------------------------------------------
        public void Tri(Vector3 a, Vector3 b, Vector3 c)
        {
            a = place.MultiplyPoint3x4(a); b = place.MultiplyPoint3x4(b); c = place.MultiplyPoint3x4(c);
            Vector3 normal = Vector3.Cross(b - a, c - a);
            if (normal.sqrMagnitude < 1e-14f) return;
            normal.Normalize();
            int i = vertices.Count; var list = triangles[paint];
            vertices.Add(a); vertices.Add(b); vertices.Add(c); normals.Add(normal); normals.Add(normal); normals.Add(normal);
            list.Add(i); list.Add(i + 1); list.Add(i + 2);
        }
        /// <summary>A quad a-b-c-d, wound clockwise as seen from the side it faces (Unity's front faces).</summary>
        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d) { Tri(a, b, c); Tri(a, c, d); }
        private void Fan(Vector3 centre, IList<Vector3> ring, bool flip)
        {
            for (int i = 0; i < ring.Count; i++)
            {
                var a = ring[i]; var b = ring[(i + 1) % ring.Count];
                if (flip) Tri(centre, b, a); else Tri(centre, a, b);
            }
        }

        // Shapes --------------------------------------------------------------------------------------------------------
        /// <summary>Revolves a profile of (radius, height) points around +Y; radius 0 closes a cap. Optional x/z squash
        /// makes it elliptical.</summary>
        public void Lathe(Vector2[] profile, int segments, float scaleX = 1, float scaleZ = 1, float phase = 0)
        {
            for (int s = 0; s < segments; s++)
            {
                float a0 = (s + phase) * Mathf.PI * 2 / segments, a1 = (s + 1 + phase) * Mathf.PI * 2 / segments;
                for (int p = 0; p < profile.Length - 1; p++)
                {
                    Vector3 P(Vector2 q, float a) => new Vector3(Mathf.Cos(a) * q.x * scaleX, q.y, Mathf.Sin(a) * q.x * scaleZ);
                    Vector3 a = P(profile[p], a0), b = P(profile[p], a1), c = P(profile[p + 1], a1), d = P(profile[p + 1], a0);
                    // outward faces (Unity's front faces wind clockwise as seen by the viewer)
                    if (profile[p].x < 1e-5f) Tri(a, d, c);
                    else if (profile[p + 1].x < 1e-5f) Tri(a, c, b);
                    else Quad(a, d, c, b);
                }
            }
        }
        /// <summary>A closed cylinder along +Y from y0 to y1 (elliptical radii), or a frustum with two radii.</summary>
        public void Cylinder(float rx, float rz, float y0, float y1, int segments, float topScale = 1, float phase = .5f)
            => Lathe(new[] { new Vector2(0, y0), new Vector2(1, y0), new Vector2(topScale, y1), new Vector2(0, y1) }, segments, rx, rz, phase);
        /// <summary>A disc-like cylinder with a chamfered top edge.</summary>
        public void Puck(float rx, float rz, float y0, float y1, int segments, float chamfer)
        {
            float c = Mathf.Min(chamfer, (y1 - y0) * .5f);
            Lathe(new[] { new Vector2(0, y0), new Vector2(1, y0), new Vector2(1, y1 - c), new Vector2(1 - c / Mathf.Max(rx, rz), y1), new Vector2(0, y1) }, segments, rx, rz, .5f);
        }
        /// <summary>A box with its edges chamfered by <paramref name="bevel"/>.</summary>
        public void Box(Vector3 centre, Vector3 size, float bevel = 0)
        {
            Vector3 h = size * .5f; float b = Mathf.Min(bevel, Mathf.Min(h.x, Mathf.Min(h.y, h.z)) * .9f);
            if (b <= 1e-5f)
            {
                Vector3 V(float x, float y, float z) => centre + new Vector3(x * h.x, y * h.y, z * h.z);
                Quad(V(-1, -1, -1), V(-1, 1, -1), V(1, 1, -1), V(1, -1, -1));   // front (-z)
                Quad(V(1, -1, 1), V(1, 1, 1), V(-1, 1, 1), V(-1, -1, 1));       // back
                Quad(V(-1, 1, -1), V(-1, 1, 1), V(1, 1, 1), V(1, 1, -1));       // top
                Quad(V(-1, -1, 1), V(-1, -1, -1), V(1, -1, -1), V(1, -1, 1));   // bottom
                Quad(V(-1, -1, 1), V(-1, 1, 1), V(-1, 1, -1), V(-1, -1, -1));   // left
                Quad(V(1, -1, -1), V(1, 1, -1), V(1, 1, 1), V(1, -1, 1));       // right
                return;
            }
            // a chamfered box as a lathe-free construction: the convex hull of three inset boxes
            Vector3 X = new Vector3(h.x, h.y - b, h.z - b), Y = new Vector3(h.x - b, h.y, h.z - b), Z = new Vector3(h.x - b, h.y - b, h.z);
            Vector3 P(Vector3 e, int sx, int sy, int sz) => centre + new Vector3(sx * e.x, sy * e.y, sz * e.z);
            for (int sx = -1; sx <= 1; sx += 2) for (int sy = -1; sy <= 1; sy += 2) for (int sz = -1; sz <= 1; sz += 2)
            {
                // corner triangle
                Vector3 a = P(X, sx, sy, sz), c = P(Y, sx, sy, sz), d = P(Z, sx, sy, sz);
                if (sx * sy * sz > 0) Tri(a, c, d); else Tri(a, d, c);
            }
            // faces
            Quad(P(Z, -1, -1, -1), P(Z, -1, 1, -1), P(Z, 1, 1, -1), P(Z, 1, -1, -1));
            Quad(P(Z, 1, -1, 1), P(Z, 1, 1, 1), P(Z, -1, 1, 1), P(Z, -1, -1, 1));
            Quad(P(Y, -1, 1, -1), P(Y, -1, 1, 1), P(Y, 1, 1, 1), P(Y, 1, 1, -1));
            Quad(P(Y, -1, -1, 1), P(Y, -1, -1, -1), P(Y, 1, -1, -1), P(Y, 1, -1, 1));
            Quad(P(X, -1, -1, 1), P(X, -1, 1, 1), P(X, -1, 1, -1), P(X, -1, -1, -1));
            Quad(P(X, 1, -1, -1), P(X, 1, 1, -1), P(X, 1, 1, 1), P(X, 1, -1, 1));
            // edge bevels
            for (int s1 = -1; s1 <= 1; s1 += 2) for (int s2 = -1; s2 <= 1; s2 += 2)
            {
                // edges along x (y=s1, z=s2)
                Vector3 a = P(Y, -1, s1, s2), bq = P(Y, 1, s1, s2), c = P(Z, 1, s1, s2), d = P(Z, -1, s1, s2);
                if (s1 * s2 > 0) Quad(a, bq, c, d); else Quad(a, d, c, bq);
                // edges along y (x=s1, z=s2)
                a = P(X, s1, -1, s2); bq = P(X, s1, 1, s2); c = P(Z, s1, 1, s2); d = P(Z, s1, -1, s2);
                if (s1 * s2 < 0) Quad(a, bq, c, d); else Quad(a, d, c, bq);
                // edges along z (x=s1, y=s2)
                a = P(X, s1, s2, -1); bq = P(X, s1, s2, 1); c = P(Y, s1, s2, 1); d = P(Y, s1, s2, -1);
                if (s1 * s2 > 0) Quad(a, bq, c, d); else Quad(a, d, c, bq);
            }
        }
        /// <summary>A cylinder rod from a to b.</summary>
        public void Rod(Vector3 a, Vector3 b, float radius, int segments = 8, float endRadius = -1)
        {
            Vector3 axis = b - a; float length = axis.magnitude; if (length < 1e-6f) return;
            var saved = place;
            place = saved * Matrix4x4.TRS(a, Quaternion.FromToRotation(Vector3.up, axis / length), Vector3.one);
            Cylinder(radius, radius, 0, length, segments, endRadius < 0 ? 1 : endRadius / radius);
            place = saved;
        }
        /// <summary>A flat ellipse (both faces) in the XY plane facing -Z, centred at <paramref name="centre"/>.</summary>
        public void Ellipse(Vector3 centre, float rx, float ry, int segments, bool twoSided = false)
        {
            var ring = new Vector3[segments];
            for (int i = 0; i < segments; i++) { float a = i * Mathf.PI * 2 / segments; ring[i] = centre + new Vector3(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry, 0); }
            Fan(centre, ring, true);
            if (twoSided) Fan(centre, ring, false);
        }
        /// <summary>Sweeps a closed 2D section (x across, y up relative to the path) along a path.</summary>
        public void Loft(Vector3[] path, bool closedPath, Vector2[] section, Vector3 up, bool capEnds = true)
        {
            int n = path.Length, m = section.Length, rings = closedPath ? n : n;
            var frames = new Vector3[n * m];
            for (int i = 0; i < n; i++)
            {
                Vector3 prev = path[closedPath ? (i - 1 + n) % n : Mathf.Max(0, i - 1)], next = path[closedPath ? (i + 1) % n : Mathf.Min(n - 1, i + 1)];
                Vector3 t = (next - prev).normalized, side = Vector3.Cross(up, t).normalized;
                if (side.sqrMagnitude < 1e-6f) side = Vector3.Cross(Vector3.forward, t).normalized;
                Vector3 u = Vector3.Cross(t, side).normalized;
                for (int j = 0; j < m; j++) frames[i * m + j] = path[i] + side * section[j].x + u * section[j].y;
            }
            int segs = closedPath ? n : n - 1;
            for (int i = 0; i < segs; i++)
            {
                int i1 = (i + 1) % n;
                for (int j = 0; j < m; j++)
                {
                    int j1 = (j + 1) % m;
                    Quad(frames[i * m + j], frames[i * m + j1], frames[i1 * m + j1], frames[i1 * m + j]);
                }
            }
            if (!closedPath && capEnds)
            {
                var start = new Vector3[m]; var end = new Vector3[m]; Vector3 cs = Vector3.zero, ce = Vector3.zero;
                for (int j = 0; j < m; j++) { start[j] = frames[j]; end[j] = frames[(n - 1) * m + j]; cs += start[j] / m; ce += end[j] / m; }
                Fan(cs, start, true); Fan(ce, end, false);
            }
        }
        /// <summary>An icosphere (0 or 1 subdivision) with ellipsoid radii; <paramref name="paintOf"/> may colour faces.</summary>
        public void Ico(Vector3 centre, Vector3 radii, int subdivisions, Func<Vector3, Color?> paintOf = null)
        {
            float t = (1 + Mathf.Sqrt(5)) / 2;
            var p = new List<Vector3> { new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0), new Vector3(0, -1, t), new Vector3(0, 1, t),
                new Vector3(0, -1, -t), new Vector3(0, 1, -t), new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1) };
            for (int i = 0; i < p.Count; i++) p[i] = p[i].normalized;
            var f = new List<int> { 0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11, 1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8, 3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9, 4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1 };
            for (int s = 0; s < subdivisions; s++)
            {
                var next = new List<int>(f.Count * 4); var mids = new Dictionary<long, int>();
                int Mid(int a, int b) { long key = a < b ? (long)a << 32 | (uint)b : (long)b << 32 | (uint)a; if (mids.TryGetValue(key, out int m)) return m; p.Add(((p[a] + p[b]) * .5f).normalized); mids[key] = p.Count - 1; return p.Count - 1; }
                for (int i = 0; i < f.Count; i += 3)
                {
                    int a = f[i], b = f[i + 1], c = f[i + 2], ab = Mid(a, b), bc = Mid(b, c), ca = Mid(c, a);
                    next.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
                }
                f = next;
            }
            int saved = paint;
            for (int i = 0; i < f.Count; i += 3)
            {
                Vector3 a = p[f[i]], b = p[f[i + 1]], c = p[f[i + 2]];
                if (paintOf != null) { var col = paintOf((a + b + c) / 3); if (col.HasValue) Paint(col.Value); }
                Tri(centre + Vector3.Scale(a, radii), centre + Vector3.Scale(c, radii), centre + Vector3.Scale(b, radii));
                paint = saved;
            }
        }

        public static Vector3[] EllipsePath(float rx, float rz, int segments, float y = 0, float phase = .5f)
        {
            var path = new Vector3[segments];
            for (int i = 0; i < segments; i++) { float a = (i + phase) * Mathf.PI * 2 / segments; path[i] = new Vector3(Mathf.Cos(a) * rx, y, Mathf.Sin(a) * rz); }
            return path;
        }
        public static Vector2[] Rect(float w, float h) => new[] { new Vector2(-w / 2, -h / 2), new Vector2(w / 2, -h / 2), new Vector2(w / 2, h / 2), new Vector2(-w / 2, h / 2) };

        /// <summary>Bakes what was drawn into a mesh on <paramref name="target"/> (one submesh per colour).</summary>
        public GameObject Bake(string name, Transform parent, COgheHomeMaterials materials, bool transparent = false)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            var mesh = new Mesh { name = name };
            if (vertices.Count > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices); mesh.SetNormals(normals);
            int used = 0; for (int i = 0; i < triangles.Count; i++) if (triangles[i].Count > 0) used++;
            mesh.subMeshCount = used; var mats = new Material[used]; int k = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                if (triangles[i].Count == 0) continue;
                mesh.SetTriangles(triangles[i], k); mats[k++] = materials.For(paints[i], transparent);
            }
            mesh.RecalculateBounds(); mesh.UploadMeshData(true);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.GetComponent<MeshRenderer>(); r.sharedMaterials = mats; r.shadowCastingMode = transparent ? ShadowCastingMode.Off : ShadowCastingMode.On;
            materials.Track(mesh);
            vertices.Clear(); normals.Clear(); foreach (var t in triangles) t.Clear(); place = Matrix4x4.identity;
            return go;
        }
    }

    /// <summary>The shared furniture palette: one lit material per colour (cloned from the creature's lit material), a
    /// quiet glass, and the ghost used for locked previews. Everything made here is destroyed with the room.</summary>
    public sealed class COgheHomeMaterials : IDisposable
    {
        private readonly Material opaqueTemplate, glassTemplate;
        private readonly Dictionary<Color, Material> opaque = new Dictionary<Color, Material>(), glass = new Dictionary<Color, Material>();
        private readonly List<Mesh> meshes = new List<Mesh>(64);
        public Material Ghost { get; }
        public COgheHomeMaterials(Material lit, Material transparent)
        {
            opaqueTemplate = lit; glassTemplate = transparent;
            Ghost = new Material(transparent) { name = "Home ghost" };
            Ghost.SetColor("_BaseColor", new Color(.86f, .93f, .92f, .62f));
        }
        public Material For(Color color, bool transparent)
        {
            var map = transparent ? glass : opaque;
            if (map.TryGetValue(color, out var m)) return m;
            m = new Material(transparent ? glassTemplate : opaqueTemplate) { name = "Home " + ColorUtility.ToHtmlStringRGB(color) };
            m.SetColor("_BaseColor", transparent ? new Color(color.r, color.g, color.b, color.a < 1 ? color.a : .22f) : color);
            if (!transparent) { m.SetFloat("_Metallic", 0); m.SetFloat("_Smoothness", .28f); m.SetColor("_EmissionColor", Color.black); m.DisableKeyword("_EMISSION"); }
            map[color] = m; return m;
        }
        public void Track(Mesh mesh) => meshes.Add(mesh);
        public void Dispose()
        {
            foreach (var m in opaque.Values) UnityEngine.Object.Destroy(m);
            foreach (var m in glass.Values) UnityEngine.Object.Destroy(m);
            foreach (var m in meshes) UnityEngine.Object.Destroy(m);
            UnityEngine.Object.Destroy(Ghost);
            opaque.Clear(); glass.Clear(); meshes.Clear();
        }
    }
}
