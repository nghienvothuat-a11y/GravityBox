using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GravityBox.Editor
{
    /// <summary>
    /// Read-only review of the 50 Spatial scenes against Mrk's chapter-1 rules (05/10/2026): no letters on controls,
    /// nothing dark or metallic, every lock shown. Lists, per level, visible text, dark parts, controls with their
    /// labels and links, and lock pins. Writes Artifacts/COgheSpatialAudit/presentation.txt; changes nothing.
    /// </summary>
    public static class COgheSpatialPresentationAudit
    {
        [MenuItem("Gravity Box/COghe/Spatial/Audit presentation (letters, dark parts, locks)")]
        public static void Run()
        {
            var lines = new List<string>();
            foreach (var path in VenomCampaignBuilder.SpatialScenePaths().Where(File.Exists))
            {
                EditorSceneManager.OpenScene(path);
                var game = Object.FindFirstObjectByType<VenomCampaign>();
                var owner = game.GetComponent<VenomLevelController>();
                lines.Add($"== {game.Definition.Order:00} {Path.GetFileNameWithoutExtension(path)} {game.Definition.Title}");
                foreach (var t in game.GetComponentsInChildren<TextMesh>())
                {
                    var r = t.GetComponent<Renderer>();
                    if (r != null && r.enabled && t.gameObject.activeInHierarchy && !string.IsNullOrWhiteSpace(t.text))
                        lines.Add($"  TEXT '{t.text}' on {Where(t.transform, owner.transform)}");
                }
                foreach (var task in owner.Apparatus.GetComponentsInChildren<COgheTapRail>())
                    lines.Add($"  CONTROL {task.Label} {task.name} requires={(task.RequiredRail != null ? task.RequiredRail.name : "-")}{(task.AlsoRequired != null && task.AlsoRequired.Length > 0 ? "+" + string.Join("+", task.AlsoRequired.Where(x => x != null).Select(x => x.name)) : "")} pin={(task.InterlockPin != null ? task.InterlockPin.name : "-")} grip={(task.RequiredGrip != null ? task.RequiredGrip.Label : "-")} load={(task.RequiredLoad != null ? task.RequiredLoad.name : "-")}");
                foreach (var link in owner.Apparatus.GetComponentsInChildren<COgheViewMechanism>())
                    lines.Add($"  LINK {(link.Input != null ? link.Input.name : "-")} -> {(link.Output != null ? link.Output.name : "-")}{(link.FinalGate ? " (exit)" : "")}");
                foreach (var m in owner.Apparatus.GetComponentsInChildren<COgheMechanism>())
                    if (!(m is COgheTapRail) && !(m is COgheRailSlider) && !(m is COgheViewMechanism))
                        lines.Add($"  MECH {m.GetType().Name} {m.name}");
                var dark = new Dictionary<string, int>();
                foreach (var r in game.GetComponentsInChildren<Renderer>())
                {
                    if (!r.enabled || !r.gameObject.activeInHierarchy || r.sharedMaterial == null || !r.sharedMaterial.HasProperty("_BaseColor")) continue;
                    if (r is LineRenderer && r.GetComponentInParent<COgheTubeNetwork>() != null) continue;
                    var m = r.sharedMaterial; var c = m.GetColor("_BaseColor");
                    float lum = .3f * c.r + .59f * c.g + .11f * c.b, metallic = m.HasProperty("_Metallic") ? m.GetFloat("_Metallic") : 0;
                    if (m.name.StartsWith("Circuit ") || m.name.StartsWith("Spatial printed") || m.name == "Chamber shadow" || m.name == "Label ink" || m.name == "Brushed aluminium") continue;
                    if (c.a < .6f) continue;
                    if (lum < .45f || metallic > .4f)
                    {
                        string key = $"{r.name} mat={m.name} lum={lum:0.00} metal={metallic:0.00}";
                        dark[key] = dark.TryGetValue(key, out int n) ? n + 1 : 1;
                    }
                }
                foreach (var kv in dark) lines.Add($"  DARK x{kv.Value} {kv.Key}");
            }
            Directory.CreateDirectory("Artifacts/COgheSpatialAudit");
            File.WriteAllLines("Artifacts/COgheSpatialAudit/presentation.txt", lines);
            Debug.Log("SPATIAL PRESENTATION AUDIT written: " + lines.Count + " lines");
        }

        private static string Where(Transform t, Transform root)
        {
            var parts = new List<string>();
            for (var p = t; p != null && p != root; p = p.parent) parts.Add(p.name);
            parts.Reverse();
            return string.Join("/", parts.Skip(Mathf.Max(0, parts.Count - 3)));
        }
    }
}
