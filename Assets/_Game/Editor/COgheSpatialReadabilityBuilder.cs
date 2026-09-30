using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        private const string ReadabilityRoot = "Spatial tube presentation";
        private const string ReadabilityMeshes = Folder + "/Meshes/SpatialReadability";

        [MenuItem("Gravity Box/COghe/Spatial pilot/Repair surface rendering and clarify tubes")]
        public static void RebuildSpatialReadability()
        {
            const string report = "Artifacts/COgheSurfaceReadability";
            Directory.CreateDirectory(report);
            var paths = VenomCampaignBuilder.SpatialScenePaths();
            string before = COgheViewArtVerification.CapturePhysics(paths);
            File.WriteAllText(report + "/physics-before.txt", before);
            var definitions = Directory.GetFiles(VenomCampaignBuilder.SpatialFolder + "/Definitions", "*.asset").OrderBy(p => p).ToArray();
            var definitionText = definitions.Select(File.ReadAllText).ToArray();
            var rows = new List<string> { "position,scene,repairedSurfaces,removedTriangles,tubeNetworks,entryMouths" };
            foreach (string path in paths)
            {
                var scene = EditorSceneManager.OpenScene(path);
                var game = Object.FindFirstObjectByType<VenomCampaign>();
                var counts = RefineSpatialReadability(game);
                rows.Add($"{game.Definition.Order},{Path.GetFileNameWithoutExtension(path)},{counts.faces},{counts.triangles},{counts.networks},{counts.entries}");
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
            string after = COgheViewArtVerification.CapturePhysics(paths);
            File.WriteAllText(report + "/physics-after.txt", after);
            File.WriteAllLines(report + "/audit.csv", rows);
            if (before != after || !definitionText.SequenceEqual(definitions.Select(File.ReadAllText)))
                throw new InvalidOperationException("Readability pass changed physical/input data or definitions.");
            Debug.Log($"SPATIAL READABILITY VERIFIED: {paths.Length} scenes; physics and definitions identical.");
        }

        // Editor-only: collider meshes, transforms and navigation patches remain untouched.
        // A prop is six 8 mm slabs. Each slab's narrow closing faces coincide with
        // adjacent slabs' front faces. Drawing both caused lavender/ivory z-fighting.
        public static (int faces, int triangles, int networks, int entries) RefineSpatialReadability(VenomCampaign game)
        {
            Directory.CreateDirectory(ReadabilityMeshes);
            AssetDatabase.Refresh();
            int faces = 0, removed = 0, mouths = 0;
            var patches = game.Surfaces.Where(p => p != null && !p.ExteriorGlass && !p.Hole && p.Curved == null && p.SphereRadius <= 0).ToArray();
            foreach (var patch in patches)
            {
                var filter = patch.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;
                var source = filter.sharedMesh;
                var vertices = source.vertices;
                var indices = source.triangles;
                var kept = new List<int>(indices.Length);
                foreach (var tri in Enumerable.Range(0, indices.Length / 3))
                {
                    int a = indices[tri * 3], b = indices[tri * 3 + 1], c = indices[tri * 3 + 2];
                    Vector3 normal = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]).normalized;
                    // Never remove the authored contact face or its back. Only an
                    // edge completely covered by a sibling's front skin is redundant.
                    bool covered = Mathf.Abs(normal.z) < .01f && patches.Any(other =>
                        other != patch && other.transform.parent == patch.transform.parent && other.name == patch.name &&
                        other.gameObject.activeSelf == patch.gameObject.activeSelf &&
                        CoveredByFace(other, patch.transform.TransformPoint(vertices[a])) &&
                        CoveredByFace(other, patch.transform.TransformPoint(vertices[b])) &&
                        CoveredByFace(other, patch.transform.TransformPoint(vertices[c])));
                    if (!covered) { kept.Add(a); kept.Add(b); kept.Add(c); }
                }
                if (kept.Count == indices.Length) continue;
                removed += (indices.Length - kept.Count) / 3; faces++;
                var mesh = Object.Instantiate(source);
                mesh.name = "Non-overlapping panel skin";
                mesh.SetTriangles(kept, 0);mesh.RecalculateBounds();
                // Reuse geometry across scenes and opposite faces of equal size.
                string signature = string.Join(";", vertices.Select(v => v.ToString("R"))) + "|" + string.Join(",", kept);
                string path = ReadabilityMeshes + "/Surface-" + Hash128.Compute(signature) + ".asset";
                var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (saved == null) { AssetDatabase.CreateAsset(mesh, path); saved = mesh; }
                else Object.DestroyImmediate(mesh);
                filter.sharedMesh = saved;
            }

            var owner = game.GetComponent<VenomLevelController>();
            var networks = owner.Apparatus.GetComponentsInChildren<COgheTubeNetwork>(true);
            if (networks.Length == 0) return (faces, removed, 0, 0);
            string previousDirectory = meshDirectory;int previousSerial = serial;
            meshDirectory = "Meshes/SpatialReadability/" + SpatialArtKey(game);serial = 0;
            Directory.CreateDirectory(Folder + "/" + meshDirectory);AssetDatabase.Refresh();
            var pearl = Lit("Spatial tube porcelain", new Color(.89f, .89f, .80f), .08f, .38f);
            var teal = Lit("Spatial tube teal fittings", new Color(.16f, .40f, .45f), .22f, .40f);
            var bore = Glass("Spatial readable cyan bore", .27f, 0, false);
            bore.SetColor("_BaseColor", new Color(.13f, .46f, .54f, .27f));EditorUtility.SetDirty(bore);
            foreach (var network in networks)
            {
                // Static artwork follows this network, not a world-space visual shortcut.
                Remove(network.transform, ReadabilityRoot);
                foreach (var renderer in network.GetComponentsInChildren<MeshRenderer>(true)) renderer.sharedMaterial = bore;
                foreach (var line in network.GetComponentsInChildren<LineRenderer>(true))
                    if (line.name == "Fine rim") { line.sharedMaterial = teal;line.startWidth = line.endWidth = .0012f; }
                var art = Child(network.transform, ReadabilityRoot);
                foreach (var edge in network.Edges)
                {
                    if (edge.Flexible || edge.Filter == null) continue;
                    var source = edge.Filter.sharedMesh.vertices;
                    const int sides = 16;
                    int rings = source.Length / sides;
                    if (rings < 2) continue;
                    var centres = new Vector3[rings];
                    for (int r = 0; r < rings; r++) for (int s = 0; s < sides; s++) centres[r] += source[r * sides + s] / sides;
                    Vector3 Local(Vector3 p) => art.InverseTransformPoint(edge.Filter.transform.TransformPoint(p));
                    // Two fine longitudinal seams give the transparent bore a silhouette,
                    // while leaving most of its circumference clear to see tissue flow.
                    var v = new List<Vector3>();var t = new List<int>();
                    for (int rib = 0; rib < 2; rib++)
                    {
                        int start = v.Count;
                        for (int r = 0; r < rings; r++)
                        {
                            Vector3 radial = (source[r * sides + rib * 8] - centres[r]).normalized;
                            Vector3 tangent = (centres[Mathf.Min(r + 1, rings - 1)] - centres[Mathf.Max(0, r - 1)]).normalized;
                            Vector3 across = Vector3.Cross(tangent, radial).normalized * .0007f;
                            Vector3 at = centres[r] + radial * (network.Radius + .0012f);
                            v.Add(Local(at - across));v.Add(Local(at + across));
                            if (r > 0) { int k = start + (r - 1) * 2;t.AddRange(new[] { k, k + 2, k + 1, k + 1, k + 2, k + 3 }); }
                        }
                    }
                    PipeArtMesh(art, "Cyan bore seams", v, t, teal);
                }
                for (int n = 0; n < network.Nodes.Length; n++)
                {
                    var node = network.Nodes[n];
                    if (node.Terminal != COgheTubeNetwork.TerminalKind.Entry) continue;
                    var edge = network.Edges.FirstOrDefault(e => e.A == n || e.B == n);
                    if (edge == null || edge.Flexible) continue;
                    var points = COgheTubeNetwork.SampleCurve(edge.ControlPoints, 6);
                    Vector3 outward = edge.A == n ? points[0] - points[1] : points[points.Length - 1] - points[points.Length - 2];
                    var chamber = owner.Rotation.transform;
                    Vector3 at = art.InverseTransformPoint(chamber.TransformPoint(node.LocalPosition));
                    Vector3 axis = art.InverseTransformDirection(chamber.TransformDirection(outward.normalized));
                    // Inner radius stays outside the actual bore: no painted disk or lip
                    // obscures the entry; collars never get a collider or a pick target.
                    PipeCollar(art, at, axis, network.Radius + .0045f, .0035f, pearl);
                    PipeCollar(art, at + axis * .004f, axis, network.Radius + .0022f, .0014f, teal);
                    mouths++;
                }
                CombineByMaterial(art);
                foreach (var r in art.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
            }
            meshDirectory = previousDirectory;serial = previousSerial;
            return (faces, removed, networks.Length, mouths);
        }

        private static bool CoveredByFace(VenomSurfacePatch face, Vector3 world)
        {
            Vector3 p = face.transform.InverseTransformPoint(world);
            const float epsilon = .00001f;
            return Mathf.Abs(p.z) <= epsilon && Mathf.Abs(p.x) <= face.Size.x * .5f + epsilon && Mathf.Abs(p.y) <= face.Size.y * .5f + epsilon;
        }
    }
}
