using System;
using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    public static class SphereMazeBuilder
    {
        public const float InnerRadius = .36f, Thickness = .006f, ExitRadius = .023f;
        private const float Spacing = .174f, HalfWidth = .025f, PlankWidth = .010f, PlankThickness = .003f;
        private const float JointOverlap = .001f, RibbonOverlap = .0008f, MaximumRibbonLength = .045f;
        private static readonly int[] Route =
        {
            20, 11, 2, 5, 8, 17, 14, 23, 26, 25, 22, 21,
            24, 15, 16, 7, 6, 3, 12, 9, 0, 1, 4, 13, 10, 27
        };
        private static readonly int[,] Branches = { { 18, 21 }, { 19, 10 } };

        // Return an unsaved root so the shared catalog builder owns prefab persistence.
        public static LevelRuntime Build(Material glass, Material frame, Material rim, PhysicsMaterial contact)
        {
            if (glass == null || frame == null || rim == null || contact == null)
                throw new ArgumentNullException("A sphere maze requires its glass, edge, exit and contact materials.");
            var root = new GameObject("SphereMaze box", typeof(Rigidbody), typeof(BoxRotationController), typeof(LevelRuntime), typeof(SpatialMaze));
            Rigidbody body = root.GetComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
            LevelRuntime level = root.GetComponent<LevelRuntime>();
            level.Rotation = root.GetComponent<BoxRotationController>();
            level.BoundsHalfExtent = .43f; level.InteriorDepth = 2 * (InnerRadius + Thickness);
            level.Footprint = new Vector2[96];
            for (int i = 0; i < level.Footprint.Length; i++)
            {
                float angle = i * Mathf.PI * 2 / level.Footprint.Length;
                level.Footprint[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * InnerRadius;
            }
            SpatialMaze maze = root.GetComponent<SpatialMaze>();
            maze.InnerRadius = InnerRadius; maze.ShellThickness = Thickness;
            maze.ClearWidth = 2 * HalfWidth; maze.PlankWidth = PlankWidth; maze.PlankThickness = PlankThickness;
            maze.SightGap = HalfWidth - PlankWidth; maze.SpawnNode = Route[0]; maze.ExitNode = 27;
            maze.MainPath = (int[])Route.Clone(); maze.NodesLocal = new Vector3[28];
            for (int node = 0; node < 27; node++) maze.NodesLocal[node] = Centre(node);
            float innerCut = Mathf.Sqrt(InnerRadius * InnerRadius - ExitRadius * ExitRadius);
            float outerRadius = InnerRadius + Thickness;
            float outerCut = Mathf.Sqrt(outerRadius * outerRadius - ExitRadius * ExitRadius);
            maze.NodesLocal[27] = Vector3.down * (outerCut + .035f);

            Material shellGlass = Transparent("SphereMaze clear spherical glass", glass, new Color(.66f, .83f, .88f, .022f), true);
            Material plankGlass = Transparent("SphereMaze assembled glass ribbons", glass, new Color(.48f, .76f, .83f, .055f), false);
            Material edgeInlay = Transparent("SphereMaze assembled ribbon edges", glass, new Color(.52f, .79f, .86f, .18f), false);
            Material exitInlay = Transparent("SphereMaze faint exit inlay", glass, new Color(.45f, .72f, .49f, .55f), false);
            exitInlay.EnableKeyword("_EMISSION"); exitInlay.SetColor("_EmissionColor", new Color(.018f, .035f, .02f));
            EditorUtility.SetDirty(exitInlay);
            maze.ShellCollider = MeshObject("Continuous glass sphere with round cut", root.transform,
                SphereMazeGeometry.Shell(InnerRadius, Thickness, ExitRadius), shellGlass, contact).GetComponent<MeshCollider>();
            var field = new GameObject("Assembled glass ribbon maze"); field.transform.SetParent(root.transform, false);
            HashSet<int> graph = Graph();
            var edges = new List<SpatialMazeEdge>(); var planks = new List<SpatialMazePlank>();
            var edgePoses = new List<Matrix4x4>(); var edgeSizes = new List<Vector3>();
            maze.JunctionColliders = new MeshCollider[27];
            for (int node = 0; node < 27; node++)
            {
                Vector3 centre = Centre(node); var parts = new List<Bounds>();
                for (int axis = 0; axis < 3; axis++)
                    foreach (int sign in new[] { -1, 1 })
                    {
                        int coordinate = axis == 0 ? node % 3 : axis == 1 ? node / 3 % 3 : node / 9;
                        int neighbour = node + sign * (axis == 0 ? 1 : axis == 1 ? 3 : 9);
                        bool valid = coordinate + sign >= 0 && coordinate + sign < 3;
                        bool open = valid && graph.Contains(Edge(node, neighbour));
                        if (node == 10 && axis == 1 && sign == -1) open = true;
                        if (!open)
                        {
                            // A real small flat cap supports the stationary steel ball.
                            if (node == maze.SpawnNode && axis == 1 && sign == -1)
                                parts.Add(new Bounds(centre + Vector3.down * (HalfWidth + PlankThickness * .5f),
                                    new Vector3(.052f, PlankThickness, .052f)));
                            else ClosedFace(parts, centre, axis, sign);
                        }
                        if (sign > 0 && valid && graph.Contains(Edge(node, neighbour)))
                            AddCorridor(node, neighbour, axis);
                    }
                maze.JunctionColliders[node] = Section("SphereMaze assembled junction " + node.ToString("00"), parts);
            }
            AddCorridor(10, 27, 1);
            maze.Edges = edges.ToArray(); maze.Planks = planks.ToArray();
            MeshObject("Quiet ribbon edge inlays", root.transform,
                SphereMazeGeometry.PlankEdges("SphereMaze assembled ribbon edge inlays", edgePoses, edgeSizes), edgeInlay, null);

            var spawn = new GameObject("BallSpawn"); spawn.transform.SetParent(root.transform, false);
            spawn.transform.localPosition = Centre(maze.SpawnNode) + Vector3.up * (-HalfWidth + .015f + .003f);
            level.BallSpawn = spawn.transform;
            var exit = new GameObject("Flush round exit", typeof(ExitSocket)); exit.transform.SetParent(root.transform, false);
            exit.transform.localPosition = Vector3.down * ((innerCut + outerCut) * .5f);
            exit.transform.localRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            level.Exit = exit.GetComponent<ExitSocket>();
            level.Exit.ApertureRadius = ExitRadius; level.Exit.WallHalfDepth = (outerCut - innerCut) * .5f;
            Mesh ring = SphereMazeGeometry.Ring("SphereMaze flush round exit inlay", ExitRadius, .0007f, level.Exit.WallHalfDepth);
            GameObject exitVisual = MeshObject("Faint flush exit rim", exit.transform, ring, exitInlay, null);
            exitVisual.transform.localRotation = Quaternion.Euler(90, 0, 0);
            return level;

            MeshCollider Section(string name, List<Bounds> parts)
            {
                Mesh mesh = SphereMazeGeometry.RibbonSection(name, parts);
                MeshCollider collider = MeshObject(name, field.transform, mesh, plankGlass, contact).GetComponent<MeshCollider>();
                foreach (Bounds part in parts)
                {
                    planks.Add(new SpatialMazePlank { CentreLocal = part.center, Size = part.size,
                        RotationLocal = Quaternion.identity, SectionCollider = collider });
                    edgePoses.Add(Matrix4x4.TRS(part.center, Quaternion.identity, Vector3.one)); edgeSizes.Add(part.size);
                }
                return collider;
            }

            void AddCorridor(int a, int b, int axis)
            {
                Vector3 from = Centre(a), to = maze.NodesLocal[b];
                Vector3 direction = (to - from).normalized;
                Vector3 start = from + direction * (HalfWidth - JointOverlap);
                Vector3 end;
                if (b == maze.ExitNode)
                {
                    // Every slat enters the shell, but its outermost corner stays
                    // at radius .363m. Nothing protrudes outside the glass skin.
                    float crossRadiusSquared = Mathf.Pow(HalfWidth + PlankWidth * .5f, 2)
                        + Mathf.Pow(HalfWidth + PlankThickness, 2);
                    float depth = Mathf.Sqrt(Mathf.Pow(InnerRadius + Thickness * .5f, 2) - crossRadiusSquared);
                    end = Vector3.down * depth;
                }
                else end = to - direction * (HalfWidth - JointOverlap);
                var parts = new List<Bounds>(); SideRibbons(parts, start, end, axis);
                string name = b == maze.ExitNode ? "SphereMaze assembled exit run"
                    : "SphereMaze assembled run " + a.ToString("00") + "-" + b.ToString("00");
                edges.Add(new SpatialMazeEdge { A = a, B = b, Axis = axis, Collider = Section(name, parts) });
            }
        }

        private static void SideRibbons(List<Bounds> parts, Vector3 from, Vector3 to, int travelAxis)
        {
            float length = Mathf.Abs(to[travelAxis] - from[travelAxis]);
            int pieces = Mathf.CeilToInt(length / MaximumRibbonLength);
            for (int piece = 0; piece < pieces; piece++)
            {
                float low = piece == 0 ? 0 : length * piece / pieces - RibbonOverlap * .5f;
                float high = piece == pieces - 1 ? length : length * (piece + 1) / pieces + RibbonOverlap * .5f;
                Vector3 centre = Vector3.Lerp(from, to, (low + high) * .5f / length);
                for (int normalAxis = 0; normalAxis < 3; normalAxis++)
                {
                    if (normalAxis == travelAxis) continue;
                    int crossAxis = 3 - normalAxis - travelAxis;
                    foreach (int sign in new[] { -1, 1 })
                        foreach (int strip in new[] { -1, 0, 1 })
                        {
                            Vector3 position = centre, size = Vector3.zero;
                            position[normalAxis] += sign * (HalfWidth + PlankThickness * .5f);
                            position[crossAxis] += strip * HalfWidth;
                            size[normalAxis] = PlankThickness; size[crossAxis] = PlankWidth; size[travelAxis] = high - low;
                            parts.Add(new Bounds(position, size));
                        }
                }
            }
        }

        private static void ClosedFace(List<Bounds> parts, Vector3 centre, int normalAxis, int sign)
        {
            int widthAxis = (normalAxis + 1) % 3, lengthAxis = (normalAxis + 2) % 3;
            foreach (int strip in new[] { -1, 0, 1 })
            {
                Vector3 position = centre, size = Vector3.zero;
                position[normalAxis] += sign * (HalfWidth + PlankThickness * .5f);
                position[widthAxis] += strip * HalfWidth;
                size[normalAxis] = PlankThickness; size[widthAxis] = PlankWidth;
                size[lengthAxis] = 2 * HalfWidth + PlankWidth;
                parts.Add(new Bounds(position, size));
            }
        }

        private static HashSet<int> Graph()
        {
            var graph = new HashSet<int>();
            for (int i = 1; i < Route.Length - 1; i++) Add(Route[i - 1], Route[i]);
            for (int i = 0; i < Branches.GetLength(0); i++) Add(Branches[i, 0], Branches[i, 1]);
            var visited = new HashSet<int> { 0 }; var queue = new Queue<int>(); queue.Enqueue(0);
            while (queue.Count > 0)
            {
                int a = queue.Dequeue();
                for (int b = 0; b < 27; b++) if (graph.Contains(Edge(a, b)) && visited.Add(b)) queue.Enqueue(b);
            }
            if (graph.Count != 26 || visited.Count != 27)
                throw new InvalidOperationException("The assembled maze must connect all 27 junctions without a shortcut loop.");
            return graph;

            void Add(int a, int b)
            {
                if (Mathf.Abs((Centre(a) - Centre(b)).magnitude - Spacing) > .00001f || !graph.Add(Edge(a, b)))
                    throw new InvalidOperationException("An assembled maze connection must join two distinct adjacent junctions.");
            }
        }

        private static Vector3 Centre(int node) => new Vector3(node % 3 - 1, node / 3 % 3 - 1, node / 9 - 1) * Spacing;
        private static int Edge(int a, int b) => Mathf.Min(a, b) * 28 + Mathf.Max(a, b);

        private static GameObject MeshObject(string name, Transform parent, Mesh mesh, Material material, PhysicsMaterial contact)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            Renderer renderer = go.GetComponent<Renderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            if (contact != null)
            {
                MeshCollider collider = go.AddComponent<MeshCollider>(); collider.sharedMesh = mesh;
                collider.sharedMaterial = contact; collider.contactOffset = .0005f;
            }
            return go;
        }

        private static Material Transparent(string name, Material source, Color color, bool reflection)
        {
            string path = PhysicsLabBuilder.Folder + "/Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(source) { name = name }; AssetDatabase.CreateAsset(material, path); }
            else material.CopyPropertiesFromMaterial(source);
            material.SetColor("_BaseColor", color); material.SetFloat("_Metallic", 0);
            material.SetFloat("_Smoothness", reflection ? .85f : .30f);
            material.SetFloat("_Surface", 1); material.SetFloat("_Blend", 0);
            material.SetFloat("_BlendModePreserveSpecular", 0);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0); material.SetFloat("_Cull", 0);
            material.SetFloat("_SpecularHighlights", reflection ? 1 : 0);
            material.SetFloat("_EnvironmentReflections", reflection ? 1 : 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            if (reflection) { material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF"); material.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF"); }
            else { material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF"); material.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF"); }
            material.DisableKeyword("_EMISSION"); material.SetShaderPassEnabled("ShadowCaster", false);
            material.renderQueue = (int)RenderQueue.Transparent; EditorUtility.SetDirty(material);
            return material;
        }
    }
}
