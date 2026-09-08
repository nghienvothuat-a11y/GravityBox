using System;
using System.Collections.Generic;
using GravityBox.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    public static class LayeredMazeBuilder
    {
        public const float TransferRadius = 0.038f;
        private const float HalfWidth = 0.30f, HalfDepth = 0.25f;
        private const float CellWidth = 0.15f, CellDepth = 0.125f;
        private const float Thickness = 0.006f, DeckSpacing = 0.09f;
        private static readonly float[] Heights = { 0.045f, -0.045f, -0.135f };
        private static readonly Vector2[] Entries = { new Vector2(-0.23f, -0.18f), new Vector2(0.22f, 0.17f), new Vector2(-0.22f, -0.17f) };
        private static readonly Vector2[] Targets = { new Vector2(0.22f, 0.17f), new Vector2(-0.22f, -0.17f), new Vector2(0.23f, 0.18f) };
        // Cell = row * 4 + column, measured from the south-west corner.
        private static readonly int[][] MainRoutes =
        {
            new[] { 0, 1, 5, 4, 8, 9, 10, 6, 7, 11, 15 },
            new[] { 15, 14, 10, 11, 7, 6, 5, 9, 8, 4, 0 },
            new[] { 0, 4, 5, 1, 2, 6, 7, 11, 10, 9, 13, 14, 15 }
        };
        private static readonly int[][][] Branches =
        {
            new[] { new[] { 1, 2, 3 }, new[] { 8, 12, 13, 14 } },
            new[] { new[] { 6, 2, 3 }, new[] { 2, 1 }, new[] { 9, 13, 12 } },
            new[] { new[] { 7, 3 }, new[] { 9, 8, 12 } }
        };

        public static void Build(Transform root, Material glass, Material frame, Material floor, Material marking, PhysicsMaterial contact)
        {
            if (root == null || glass == null || frame == null || floor == null || marking == null || contact == null)
                throw new ArgumentNullException("The layered maze requires its box, render materials and physical contact material.");
            Transform bottomFloor = root.Find("Floor with circular cut");
            if (bottomFloor == null || bottomFloor.GetComponent<Collider>() == null)
                throw new InvalidOperationException("Build the outer shell and its bottom exit before the layered maze interior.");
            var metadata = root.gameObject.AddComponent<LayeredMaze>();
            metadata.FloorHeights = (float[])Heights.Clone();
            metadata.Decks = new LayeredMazeDeck[3];
            metadata.TransferPorts = new Transform[2];
            metadata.TransferRadius = TransferRadius;
            var outline = new[] { new Vector2(-HalfWidth, -HalfDepth), new Vector2(HalfWidth, -HalfDepth),
                new Vector2(HalfWidth, HalfDepth), new Vector2(-HalfWidth, HalfDepth) };
            Color[] colors = { new Color(.36f, .76f, .86f), new Color(.92f, .65f, .34f), new Color(.46f, .78f, .53f) };
            string[] names = { "Upper maze", "Middle maze", "Lower maze" };
            Mesh portRing = TransferRing();

            for (int layer = 0; layer < 3; layer++)
            {
                var deckRoot = new GameObject(names[layer]);
                deckRoot.transform.SetParent(root, false);
                var renderers = new List<Renderer>();
                Color tint = colors[layer];
                Material deckGlass = TintedMaterial(names[layer] + " glass", glass, new Color(tint.r, tint.g, tint.b, .075f));
                Material edge = TintedMaterial(names[layer] + " edges", marking, tint);
                float floorHeight = Heights[layer];
                Collider floorCollider;
                if (layer < 2)
                {
                    Mesh panel = PhysicsLabGeometry.Panel(names[layer] + " transfer floor", outline,
                        floorHeight, Thickness * .5f, Targets[layer], TransferRadius);
                    var plate = new GameObject(names[layer] + " floor", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                    plate.transform.SetParent(deckRoot.transform, false);
                    plate.GetComponent<MeshFilter>().sharedMesh = panel;
                    Renderer renderer = plate.GetComponent<MeshRenderer>();
                    renderer.sharedMaterial = deckGlass;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    var collider = plate.GetComponent<MeshCollider>();
                    collider.sharedMesh = panel; collider.sharedMaterial = contact; collider.contactOffset = .0005f;
                    floorCollider = collider;
                    renderers.Add(renderer);

                    var port = new GameObject("Transfer port " + (layer + 1), typeof(MeshFilter), typeof(MeshRenderer));
                    port.transform.SetParent(deckRoot.transform, false);
                    port.transform.localPosition = new Vector3(Targets[layer].x, floorHeight, Targets[layer].y);
                    port.transform.localRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
                    port.GetComponent<MeshFilter>().sharedMesh = portRing;
                    Renderer portRenderer = port.GetComponent<MeshRenderer>();
                    portRenderer.sharedMaterial = edge; portRenderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderers.Add(portRenderer);
                    metadata.TransferPorts[layer] = port.transform;
                }
                else
                {
                    floorCollider = bottomFloor.GetComponent<Collider>();
                    Renderer renderer = bottomFloor.GetComponent<Renderer>();
                    renderer.sharedMaterial = deckGlass;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderers.Add(renderer);
                }

                HashSet<int> openings = Openings(layer);
                float wallHeight = DeckSpacing - Thickness;
                float wallY = floorHeight + DeckSpacing * .5f;
                for (int row = 0; row < 4; row++)
                    for (int column = 0; column < 4; column++)
                    {
                        int cell = row * 4 + column;
                        if (column < 3 && !openings.Contains(Edge(cell, cell + 1)))
                            Wall("Column wall " + column + "/" + row,
                                new Vector3(-HalfWidth + (column + 1) * CellWidth, wallY, -HalfDepth + (row + .5f) * CellDepth),
                                new Vector3(Thickness, wallHeight, CellDepth + Thickness));
                        if (row < 3 && !openings.Contains(Edge(cell, cell + 4)))
                            Wall("Row wall " + column + "/" + row,
                                new Vector3(-HalfWidth + (column + .5f) * CellWidth, wallY, -HalfDepth + (row + 1) * CellDepth),
                                new Vector3(CellWidth + Thickness, wallHeight, Thickness));
                    }

                // Thin perimeter lines make the three separate floor elevations readable.
                float bandY = floorHeight + Thickness * .5f;
                Line("North floor edge", new Vector3(0, bandY, HalfDepth), new Vector3(HalfWidth * 2, .001f, .0015f));
                Line("South floor edge", new Vector3(0, bandY, -HalfDepth), new Vector3(HalfWidth * 2, .001f, .0015f));
                Line("West floor edge", new Vector3(-HalfWidth, bandY, 0), new Vector3(.0015f, .001f, HalfDepth * 2));
                Line("East floor edge", new Vector3(HalfWidth, bandY, 0), new Vector3(.0015f, .001f, HalfDepth * 2));
                metadata.Decks[layer] = new LayeredMazeDeck
                {
                    Name = names[layer], FloorHeight = floorHeight, FloorCollider = floorCollider,
                    Renderers = renderers.ToArray(), RouteLocalPoints = Route(layer)
                };

                void Wall(string name, Vector3 position, Vector3 size)
                {
                    var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.name = name; wall.transform.SetParent(deckRoot.transform, false);
                    wall.transform.localPosition = position; wall.transform.localScale = size;
                    Renderer renderer = wall.GetComponent<Renderer>();
                    renderer.sharedMaterial = deckGlass; renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderers.Add(renderer);
                    Collider collider = wall.GetComponent<Collider>();
                    collider.sharedMaterial = contact; collider.contactOffset = .0005f;
                    Vector3 cap = new Vector3(size.x, .0015f, size.z);
                    Line(name + " upper edge", position + Vector3.up * (size.y * .5f - cap.y * .5f), cap);
                    Line(name + " lower edge", position - Vector3.up * (size.y * .5f - cap.y * .5f), cap);
                }

                void Line(string name, Vector3 position, Vector3 size)
                {
                    var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    line.name = name; line.transform.SetParent(deckRoot.transform, false);
                    line.transform.localPosition = position; line.transform.localScale = size;
                    UnityEngine.Object.DestroyImmediate(line.GetComponent<Collider>());
                    Renderer renderer = line.GetComponent<Renderer>();
                    renderer.sharedMaterial = edge; renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderers.Add(renderer);
                }
            }
        }

        private static HashSet<int> Openings(int layer)
        {
            var edges = new HashSet<int>();
            Add(MainRoutes[layer]);
            foreach (int[] branch in Branches[layer]) Add(branch);
            var visited = new HashSet<int> { 0 };
            var pending = new Queue<int>(); pending.Enqueue(0);
            while (pending.Count > 0)
            {
                int from = pending.Dequeue();
                for (int to = 0; to < 16; to++)
                    if (edges.Contains(Edge(from, to)) && visited.Add(to)) pending.Enqueue(to);
            }
            if (edges.Count != 15 || visited.Count != 16)
                throw new InvalidOperationException("Each maze deck must connect all sixteen cells without an unintended loop.");
            return edges;

            void Add(int[] path)
            {
                for (int i = 1; i < path.Length; i++)
                {
                    int a = path[i - 1], b = path[i];
                    if (Math.Abs(a % 4 - b % 4) + Math.Abs(a / 4 - b / 4) != 1)
                        throw new InvalidOperationException("Maze routes may only connect neighbouring cells.");
                    edges.Add(Edge(a, b));
                }
            }
        }

        private static int Edge(int a, int b) => Mathf.Min(a, b) * 16 + Mathf.Max(a, b);

        private static Vector3[] Route(int layer)
        {
            float y = Heights[layer] + Thickness * .5f + .015f;
            var route = new List<Vector3> { new Vector3(Entries[layer].x, y, Entries[layer].y) };
            // The last cell centre can already overlap the real hole. Aim straight
            // at the port from the preceding cell rather than expecting a stop over air.
            for (int i = 0; i < MainRoutes[layer].Length - 1; i++)
            {
                int cell = MainRoutes[layer][i];
                route.Add(new Vector3(-HalfWidth + (cell % 4 + .5f) * CellWidth, y,
                    -HalfDepth + (cell / 4 + .5f) * CellDepth));
            }
            route.Add(new Vector3(Targets[layer].x, y, Targets[layer].y));
            return route.ToArray();
        }

        private static Material TintedMaterial(string name, Material source, Color color)
        {
            string path = PhysicsLabBuilder.Folder + "/Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(source); AssetDatabase.CreateAsset(material, path); }
            else material.CopyPropertiesFromMaterial(source);
            material.name = name; material.SetColor("_BaseColor", color);
            material.SetColor("_EmissionColor", Color.black); material.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(material); return material;
        }

        private static Mesh TransferRing()
        {
            const int segments = 96;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments, b = (i + 1) * Mathf.PI * 2 / segments;
                foreach (int sign in new[] { -1, 1 })
                {
                    float z = sign * (Thickness * .5f + .00005f);
                    int n = vertices.Count;
                    vertices.Add(new Vector3(Mathf.Cos(a) * TransferRadius, Mathf.Sin(a) * TransferRadius, z));
                    vertices.Add(new Vector3(Mathf.Cos(b) * TransferRadius, Mathf.Sin(b) * TransferRadius, z));
                    vertices.Add(new Vector3(Mathf.Cos(b) * (TransferRadius + .0013f), Mathf.Sin(b) * (TransferRadius + .0013f), z));
                    vertices.Add(new Vector3(Mathf.Cos(a) * (TransferRadius + .0013f), Mathf.Sin(a) * (TransferRadius + .0013f), z));
                    if (sign < 0) triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
                    else triangles.AddRange(new[] { n, n + 2, n + 1, n, n + 3, n + 2 });
                }
            }
            string path = PhysicsLabBuilder.Folder + "/Meshes/Layered maze transfer ring.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) { mesh = new Mesh { name = "Layered maze transfer ring" }; AssetDatabase.CreateAsset(mesh, path); }
            mesh.Clear(); mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh);
            return mesh;
        }
    }
}
