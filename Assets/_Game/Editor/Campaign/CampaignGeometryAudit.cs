using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEditor;
using UnityEngine;

namespace GravityBox.Editor
{
    public static class CampaignGeometryAudit
    {
        [Serializable] private sealed class Audit
        {
            public string Scope = "Initial spawn separation and static exit clearance; not a complete gameplay solution proof.";
            public List<string> Errors = new List<string>();
            public int LevelsChecked;
        }
        [MenuItem("Gravity Box/Campaign/Audit Spawn and Exit Geometry")]
        public static void Run()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CampaignBuilder.CatalogPath);
            if (catalog == null) throw new InvalidOperationException("Generate campaign first.");
            var audit = new Audit();
            GameObject probeObject = new GameObject("Campaign radius audit", typeof(SphereCollider));
            SphereCollider probe = probeObject.GetComponent<SphereCollider>(); probe.radius = catalog.BallProfile.Radius;
            try
            {
                foreach (LevelDefinition definition in catalog.Levels)
                {
                    LevelRuntime root = UnityEngine.Object.Instantiate(definition.Prefab);
                    try
                    {
                        var solids = root.GetComponentsInChildren<Collider>(true);
                        for (int b = 0; b < root.BallCount; b++)
                        {
                            Vector3 position = root.GetBallSpawn(b).position;
                            foreach (Collider solid in solids)
                            {
                                if (!solid.enabled || solid.isTrigger) continue;
                                if (Physics.ComputePenetration(probe, position, Quaternion.identity, solid,
                                        solid.transform.position, solid.transform.rotation, out _, out float depth) && depth > .001f)
                                    audit.Errors.Add($"C{definition.DisplayIndex:000} spawn {b + 1} penetrates {solid.name} by {depth:F5} m");
                            }
                        }
                        for (int sample = 0; sample <= 12; sample++)
                        {
                            Vector3 position = root.Exit.transform.TransformPoint(new Vector3(0, 0, -.024f + sample * .004f));
                            foreach (Collider solid in solids)
                            {
                                if (!solid.enabled || solid.isTrigger || solid.GetComponentInParent<PhysicalProp>() != null) continue;
                                if (Physics.ComputePenetration(probe, position, Quaternion.identity, solid,
                                        solid.transform.position, solid.transform.rotation, out _, out float depth) && depth > .0008f)
                                {
                                    string error = $"C{definition.DisplayIndex:000} static exit bore blocked by {solid.name}";
                                    if (!audit.Errors.Contains(error)) audit.Errors.Add(error);
                                }
                            }
                        }
                        audit.LevelsChecked++;
                    }
                    finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(probeObject); }
            Directory.CreateDirectory("Docs/Verification/Campaign100");
            File.WriteAllText("Docs/Verification/Campaign100/geometry-audit.json", JsonUtility.ToJson(audit, true));
            foreach (string error in audit.Errors) Debug.LogError(error);
            if (audit.Errors.Count > 0) throw new InvalidOperationException($"Campaign geometry audit: {audit.Errors.Count} issues in {audit.LevelsChecked} levels.");
            Debug.Log("CAMPAIGN GEOMETRY AUDIT PASS: all 100 spawns and static exit bores clear.");
        }
    }
}
