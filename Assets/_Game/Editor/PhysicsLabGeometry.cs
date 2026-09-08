using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GravityBox.Editor
{
    // Closed baked meshes, in metres. Collider and visible surface share every edge.
    internal static class PhysicsLabGeometry
    {
        private const float CoordinateTolerance = 0.0000001f;

        public static Mesh Panel(string name, Vector2[] outline, float y, float halfThickness,
            Vector2 hole, float holeRadius = 0, Vector2[][] voids = null)
        {
            // Preserve the established triangulation of the original three convex containers.
            if ((voids == null || voids.Length == 0) && IsConvex(outline)
                && Contains(outline, holeRadius > 0 ? hole : Vector2.zero))
                return ConvexPanel(name, outline, y, halfThickness, hole, holeRadius);
            if (halfThickness <= 0) throw new ArgumentOutOfRangeException(nameof(halfThickness));
            var contours = new List<Vector2[]> { OrientedContour(outline, true) };
            if (voids != null)
                foreach (Vector2[] contour in voids) contours.Add(OrientedContour(contour, false));
            if (holeRadius > 0)
            {
                var aperture = new Vector2[96];
                for (int i = 0; i < aperture.Length; i++) aperture[i] = hole + Dir(-i * Mathf.PI * 2 / aperture.Length) * holeRadius;
                contours.Add(aperture);
            }
            return GeneralPanel(name, contours, y, halfThickness);
        }

        private static Mesh ConvexPanel(string name, Vector2[] outline, float y, float halfThickness,
            Vector2 hole, float holeRadius)
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
            if (width <= 0 || top <= bottom) throw new ArgumentException("A border requires positive width and height.");
            ValidateContour(inner, name);
            var mesh = new Surface();
            var outer = new Vector2[inner.Length];
            for (int i = 0; i < inner.Length; i++)
            {
                Vector2 before = (inner[i] - inner[(i + inner.Length - 1) % inner.Length]).normalized;
                Vector2 after = (inner[(i + 1) % inner.Length] - inner[i]).normalized;
                Vector2 rightBefore = new Vector2(before.y, -before.x), rightAfter = new Vector2(after.y, -after.x);
                float denominator = 1 + Vector2.Dot(rightBefore, rightAfter);
                if (denominator < 0.00001f) throw new ArgumentException(name + " has a reversing edge; a constant-width miter is undefined.");
                // Offset each supporting line by the same perpendicular distance, then intersect.
                // CCW outer boundaries grow outwards; CW void boundaries grow into the void.
                outer[i] = inner[i] + (rightBefore + rightAfter) * (width / denominator);
            }
            ValidateContour(outer, name + " offset");
            if (SignedArea(inner) * SignedArea(outer) <= 0)
                throw new ArgumentException(name + " is too narrow for its requested border width.");
            for (int i = 0; i < inner.Length; i++)
            {
                Vector2 a = inner[i], b = inner[(i + 1) % inner.Length], oa = outer[i], ob = outer[(i + 1) % inner.Length];
                mesh.Quad(P(a, top), P(b, top), P(ob, top), P(oa, top));
                mesh.Quad(P(oa, bottom), P(ob, bottom), P(b, bottom), P(a, bottom));
                mesh.Quad(P(a, bottom), P(b, bottom), P(b, top), P(a, top));
                mesh.Quad(P(oa, top), P(ob, top), P(ob, bottom), P(oa, bottom));
            }
            return mesh.Save(name);
        }

        private struct Crossing
        {
            public Vector2 A, B;
            public double MiddleX;
        }

        private static Mesh GeneralPanel(string name, List<Vector2[]> contours, float y, float halfThickness)
        {
            // Splitting at every vertex height makes each even-odd filled span a convex trapezoid.
            // Concave notches and internal voids are never filled by a fan from an outside origin.
            var heights = new List<float>();
            foreach (Vector2[] contour in contours)
                foreach (Vector2 vertex in contour) heights.Add(vertex.y);
            SortUnique(heights);
            // Trigonometric circle vertices at the same mathematical height may differ by a few ULPs.
            // Snap only within 0.1 micrometre so face and bore edges share exactly the same cut planes.
            foreach (Vector2[] contour in contours)
                for (int i = 0; i < contour.Length; i++) contour[i].y = Canonical(heights, contour[i].y);
            ValidateDomain(contours, name);

            var xsAtHeight = new List<float>[heights.Count];
            for (int h = 0; h < heights.Count; h++)
            {
                var xs = new List<float>();
                foreach (Vector2[] contour in contours)
                    for (int i = 0; i < contour.Length; i++)
                    {
                        Vector2 a = contour[i], b = contour[(i + 1) % contour.Length];
                        if (heights[h] < Mathf.Min(a.y, b.y) || heights[h] > Mathf.Max(a.y, b.y)) continue;
                        if (a.y == b.y) { xs.Add(a.x); xs.Add(b.x); }
                        else xs.Add(EdgeX(a, b, heights[h]));
                    }
                SortUnique(xs);
                xsAtHeight[h] = xs;
            }

            var surface = new Surface();
            var crossings = new List<Crossing>();
            var polygon = new List<Vector2>();
            double area = 0;
            for (int h = 0; h < heights.Count - 1; h++)
            {
                float low = heights[h], high = heights[h + 1];
                double middle = ((double)low + high) * 0.5;
                crossings.Clear();
                foreach (Vector2[] contour in contours)
                    for (int i = 0; i < contour.Length; i++)
                    {
                        Vector2 a = contour[i], b = contour[(i + 1) % contour.Length];
                        if (middle <= Math.Min(a.y, b.y) || middle >= Math.Max(a.y, b.y)) continue;
                        crossings.Add(new Crossing { A = a, B = b, MiddleX = a.x + (b.x - a.x) * (middle - a.y) / (b.y - a.y) });
                    }
                crossings.Sort((a, b) => a.MiddleX.CompareTo(b.MiddleX));
                if (crossings.Count % 2 != 0) throw new InvalidOperationException(name + " has an unpaired contour crossing.");
                for (int c = 0; c < crossings.Count; c += 2)
                {
                    Crossing left = crossings[c], right = crossings[c + 1];
                    polygon.Clear();
                    AppendHorizontal(polygon, xsAtHeight[h], low, EdgeX(left.A, left.B, low), EdgeX(right.A, right.B, low), false);
                    AppendHorizontal(polygon, xsAtHeight[h + 1], high, EdgeX(left.A, left.B, high), EdgeX(right.A, right.B, high), true);
                    if (polygon.Count < 3) continue;
                    double patchArea = SignedArea(polygon);
                    if (patchArea < -1e-12) throw new InvalidOperationException(name + " has an inverted panel span.");
                    if (patchArea <= 1e-14) continue;
                    area += patchArea;
                    Vector2 center = Vector2.zero;
                    foreach (Vector2 point in polygon) center += point;
                    center /= polygon.Count;
                    for (int i = 0; i < polygon.Count; i++)
                    {
                        Vector2 a = polygon[i], b = polygon[(i + 1) % polygon.Count];
                        surface.Triangle(P(center, y + halfThickness), P(b, y + halfThickness), P(a, y + halfThickness));
                        surface.Triangle(P(center, y - halfThickness), P(a, y - halfThickness), P(b, y - halfThickness));
                    }
                }
            }
            double expectedArea = SignedArea(contours[0]);
            for (int i = 1; i < contours.Count; i++) expectedArea += SignedArea(contours[i]);
            if (Math.Abs(area - expectedArea) > Math.Max(1e-9, expectedArea * 0.00001))
                throw new InvalidOperationException($"{name} panel area differs from its contours: {area:G9} versus {expectedArea:G9} m².");

            // Split side faces at the same cut vertices as the caps, including horizontal notch edges.
            // This eliminates T-junctions along every outer boundary and every through-hole bore.
            var edgePoints = new List<Vector2>();
            foreach (Vector2[] contour in contours)
                for (int i = 0; i < contour.Length; i++)
                {
                    Vector2 a = contour[i], b = contour[(i + 1) % contour.Length];
                    edgePoints.Clear();
                    if (a.y == b.y)
                    {
                        int h = heights.IndexOf(a.y);
                        AppendHorizontal(edgePoints, xsAtHeight[h], a.y, Mathf.Min(a.x, b.x), Mathf.Max(a.x, b.x), a.x > b.x);
                    }
                    else
                    {
                        for (int h = 0; h < heights.Count; h++)
                            if (heights[h] >= Mathf.Min(a.y, b.y) && heights[h] <= Mathf.Max(a.y, b.y))
                                edgePoints.Add(new Vector2(Canonical(xsAtHeight[h], EdgeX(a, b, heights[h])), heights[h]));
                        if (a.y > b.y) edgePoints.Reverse();
                    }
                    for (int j = 0; j < edgePoints.Count - 1; j++)
                    {
                        Vector2 p = edgePoints[j], q = edgePoints[j + 1];
                        surface.Quad(P(p, y - halfThickness), P(p, y + halfThickness), P(q, y + halfThickness), P(q, y - halfThickness));
                    }
                }
            return surface.Save(name);
        }

        private static void AppendHorizontal(List<Vector2> result, List<float> xs, float y, float left, float right, bool reverse)
        {
            left = Canonical(xs, left); right = Canonical(xs, right);
            for (int i = 0; i < xs.Count; i++)
            {
                int index = reverse ? xs.Count - 1 - i : i;
                if (xs[index] >= left && xs[index] <= right) result.Add(new Vector2(xs[index], y));
            }
        }

        private static float EdgeX(Vector2 a, Vector2 b, float y)
        {
            if (y == a.y) return a.x;
            if (y == b.y) return b.x;
            return (float)(a.x + ((double)b.x - a.x) * (y - a.y) / ((double)b.y - a.y));
        }

        private static void SortUnique(List<float> values)
        {
            values.Sort();
            int count = 0;
            for (int i = 0; i < values.Count; i++)
                if (count == 0 || values[i] - values[count - 1] >= CoordinateTolerance) values[count++] = values[i];
            if (count < values.Count) values.RemoveRange(count, values.Count - count);
        }

        private static float Canonical(List<float> values, float value)
        {
            foreach (float candidate in values)
                if (Mathf.Abs(candidate - value) < CoordinateTolerance) return candidate;
            throw new InvalidOperationException("A panel edge could not be matched to its cut plane.");
        }

        private static Vector2[] OrientedContour(Vector2[] points, bool counterClockwise)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            var copy = (Vector2[])points.Clone();
            if ((SignedArea(copy) > 0) != counterClockwise) Array.Reverse(copy);
            return copy;
        }

        private static bool IsConvex(Vector2[] contour)
        {
            if (contour == null || contour.Length < 3) return false;
            float direction = 0;
            for (int i = 0; i < contour.Length; i++)
            {
                float turn = Cross(contour[(i + 1) % contour.Length] - contour[i], contour[(i + 2) % contour.Length] - contour[(i + 1) % contour.Length]);
                if (Mathf.Abs(turn) < 1e-10f) continue;
                if (direction == 0) direction = Mathf.Sign(turn);
                else if (Mathf.Sign(turn) != direction) return false;
            }
            return direction > 0;
        }

        private static double SignedArea(IList<Vector2> points)
        {
            double twiceArea = 0;
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 a = points[i], b = points[(i + 1) % points.Count];
                twiceArea += (double)a.x * b.y - (double)a.y * b.x;
            }
            return twiceArea * 0.5;
        }

        private static void ValidateDomain(List<Vector2[]> contours, string name)
        {
            foreach (Vector2[] contour in contours) ValidateContour(contour, name);
            for (int i = 1; i < contours.Count; i++)
            {
                if (!Contains(contours[0], contours[i][0])) throw new ArgumentException(name + " has a void outside its outer boundary.");
                for (int j = 0; j < i; j++)
                {
                    if (j > 0 && (Contains(contours[j], contours[i][0]) || Contains(contours[i], contours[j][0])))
                        throw new ArgumentException(name + " has overlapping or nested voids.");
                    for (int a = 0; a < contours[i].Length; a++)
                        for (int b = 0; b < contours[j].Length; b++)
                            if (SegmentsMeet(contours[i][a], contours[i][(a + 1) % contours[i].Length], contours[j][b], contours[j][(b + 1) % contours[j].Length]))
                                throw new ArgumentException(name + " has touching or intersecting contours.");
                }
            }
        }

        private static void ValidateContour(Vector2[] contour, string name)
        {
            if (contour == null || contour.Length < 3 || Math.Abs(SignedArea(contour)) < 1e-10)
                throw new ArgumentException(name + " requires a non-degenerate closed contour.");
            for (int i = 0; i < contour.Length; i++)
            {
                if (float.IsNaN(contour[i].x) || float.IsInfinity(contour[i].x) || float.IsNaN(contour[i].y) || float.IsInfinity(contour[i].y))
                    throw new ArgumentException(name + " contains a non-finite vertex.");
                if ((contour[i] - contour[(i + 1) % contour.Length]).sqrMagnitude < 1e-16f)
                    throw new ArgumentException(name + " contains a zero-length edge.");
                for (int j = i + 2; j < contour.Length; j++)
                {
                    if (i == 0 && j == contour.Length - 1) continue;
                    if (SegmentsMeet(contour[i], contour[(i + 1) % contour.Length], contour[j], contour[(j + 1) % contour.Length]))
                        throw new ArgumentException(name + " contains a self-intersecting contour.");
                }
            }
        }

        private static bool Contains(Vector2[] contour, Vector2 point)
        {
            bool inside = false;
            for (int i = 0, j = contour.Length - 1; i < contour.Length; j = i++)
                if ((contour[i].y > point.y) != (contour[j].y > point.y)
                    && point.x < (double)(contour[j].x - contour[i].x) * (point.y - contour[i].y) / (contour[j].y - contour[i].y) + contour[i].x)
                    inside = !inside;
            return inside;
        }

        private static bool SegmentsMeet(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            if (Mathf.Max(a.x, b.x) < Mathf.Min(c.x, d.x) - CoordinateTolerance || Mathf.Max(c.x, d.x) < Mathf.Min(a.x, b.x) - CoordinateTolerance
                || Mathf.Max(a.y, b.y) < Mathf.Min(c.y, d.y) - CoordinateTolerance || Mathf.Max(c.y, d.y) < Mathf.Min(a.y, b.y) - CoordinateTolerance) return false;
            double abC = (double)(b.x - a.x) * (c.y - a.y) - (double)(b.y - a.y) * (c.x - a.x);
            double abD = (double)(b.x - a.x) * (d.y - a.y) - (double)(b.y - a.y) * (d.x - a.x);
            double cdA = (double)(d.x - c.x) * (a.y - c.y) - (double)(d.y - c.y) * (a.x - c.x);
            double cdB = (double)(d.x - c.x) * (b.y - c.y) - (double)(d.y - c.y) * (b.x - c.x);
            return abC * abD <= 1e-20 && cdA * cdB <= 1e-20;
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
