using System;
using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEditor;
using UnityEngine;

namespace GravityBox.Editor
{
    public static class CampaignValidator
    {
        [MenuItem("Gravity Box/Campaign/Validate 100 Levels")]
        public static void Validate()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CampaignBuilder.CatalogPath);
            var lab = AssetDatabase.LoadAssetAtPath<LevelCatalog>(PrototypeBuilder.CatalogPath);
            Require(catalog != null && catalog.IsCampaign && catalog.Levels?.Length == 100, "Expected a 100-level campaign.");
            Require(lab != null && lab.Levels.Length == 23, "The Physics Lab must remain intact.");
            Require(catalog.BallProfile == lab.BallProfile && catalog.BallPrefab == lab.BallPrefab && catalog.Rotation == lab.Rotation,
                "Campaign must use the same steel ball, contacts and hand rotation.");
            var ids = new HashSet<string>(); var prefabs = new HashSet<LevelRuntime>(); var learned = new HashSet<string>();
            int bosses = 0, sol = 0, astra = 0;
            for (int i = 0; i < catalog.Levels.Length; i++)
            {
                LevelDefinition level = catalog.Levels[i];
                Require(level != null && !string.IsNullOrWhiteSpace(level.Id) && ids.Add(level.Id), "Missing or duplicate level ID at " + (i + 1));
                Require(level.DisplayIndex == i + 1 && level.Prefab != null && prefabs.Add(level.Prefab), level.Id + ": unique authored prefab/index required.");
                LevelDesignProfile profile = level.Design;
                Require(profile != null && profile.Chapter == i / 10 + 1, level.Id + ": missing chapter design.");
                Require(profile.IsBoss == ((i + 1) % 10 == 0) && (profile.Role == CampaignRole.Boss) == profile.IsBoss, level.Id + ": boss slot mismatch.");
                Require(profile.IntroducedSkills.Length <= (profile.IsBoss ? 0 : 1), level.Id + ": too many new skills.");
                foreach (string skill in profile.RequiredSkills) Require(learned.Contains(skill), level.Id + ": untaught prerequisite " + skill);
                foreach (string skill in profile.IntroducedSkills) learned.Add(skill);
                if (profile.IsBoss) bosses++;
                if (profile.ImplementedByModel == "gpt-5.6-sol") sol++;
                else if (profile.ImplementedByModel == "gpt-6-astra") astra++;
                else Require(false, level.Id + ": missing requested author model.");
                Require(!profile.IsBoss || profile.ImplementedByModel == "gpt-6-astra", level.Id + ": bosses require Astra.");
                Require(profile.DifficultyBudget >= 0 && profile.DifficultyBudget <= 100, level.Id + ": invalid difficulty budget.");
                Require(!string.IsNullOrWhiteSpace(profile.RecoveryPlan) && !string.IsNullOrWhiteSpace(level.TeachingHint), level.Id + ": missing recovery/teaching intent.");
                ValidatePhysics(level, catalog);
            }
            Require(bosses == 10 && sol == 55 && astra == 45, "Expected 10 bosses, 55 Sol and 45 Astra authored levels.");
            Debug.Log("CAMPAIGN VALIDATION PASS: 100 unique prefabs, 10 bosses, curriculum prerequisites and shared physics contracts.");
        }

        private static void ValidatePhysics(LevelDefinition definition, LevelCatalog catalog)
        {
            LevelRuntime level = definition.Prefab;
            string id = definition.Id;
            Require(level.Exit != null && level.Rotation != null && level.BallSpawn != null, id + ": missing runtime contract.");
            Require(level.transform.localScale == Vector3.one, id + ": root must remain metre scale.");
            Require(level.GetComponent<Rigidbody>().isKinematic, id + ": box must be kinematic.");
            Require(definition.Environment != null && definition.Environment.Acceleration == Vector3.down * 9.81f &&
                definition.InitialLocalVelocity == Vector3.zero, id + ": changed Earth physics/spawn impulse.");
            // A curved shell's radial bore is slightly longer than the planar 6 mm skin.
            Require(Mathf.Abs(level.Exit.ApertureRadius - .023f) < .000001f && Mathf.Abs(level.Exit.WallHalfDepth - .003f) < .0001f && string.IsNullOrEmpty(level.Exit.RequiredChannel), id + ": exit contract changed.");
            Require(level.BallCount >= 1 && level.BallCount <= 2, id + ": roster should contain one or two balls.");
            for (int b = 0; b < level.BallCount; b++)
            {
                Require(level.GetBallSpawn(b) != null && !level.IsOutside(level.GetBallSpawn(b).position), id + ": invalid spawn " + b);
                for (int other = 0; other < b; other++)
                    Require(Vector3.Distance(level.GetBallSpawn(b).position, level.GetBallSpawn(other).position) > catalog.BallProfile.Radius * 2, id + ": spawns overlap.");
            }
            Require(level.GetComponentsInChildren<ImpulsePad>(true).Length == 0 && level.GetComponentsInChildren<SignalDoor>(true).Length == 0 &&
                level.GetComponentsInChildren<OneWayGate>(true).Length == 0 && level.GetComponentsInChildren<KillVolume>(true).Length == 0,
                id + ": unsupported scripted mechanism in physical campaign.");
            var bodies = new HashSet<Rigidbody>(level.GetComponentsInChildren<Rigidbody>(true));
            foreach (PhysicalProp prop in level.GetComponentsInChildren<PhysicalProp>(true))
                Require(prop.Body != null && !prop.Body.isKinematic && prop.Body.mass > 0 && prop.transform.lossyScale == Vector3.one, id + ": invalid physical prop " + prop.name);
            foreach (Joint joint in level.GetComponentsInChildren<Joint>(true))
                Require(joint.connectedBody != null && bodies.Contains(joint.connectedBody), id + ": joint references another level " + joint.name);
            foreach (MeshCollider collider in level.GetComponentsInChildren<MeshCollider>(true))
            {
                Require(collider.sharedMesh != null, id + ": missing collision mesh " + collider.name);
                Require(AssetDatabase.Contains(collider.sharedMesh), id + ": unsaved collision mesh " + collider.name);
                if (collider.GetComponentInParent<PhysicalProp>() != null)
                    Require(collider.convex, id + ": free bodies need convex meshes.");
            }
            WaterVolume fluid = level.GetComponent<WaterVolume>();
            if (fluid != null)
            {
                Require(level.BallCount == 1 && level.GetComponentsInChildren<PhysicalProp>(true).Length == 0, id + ": fluid chapter must use one ball and static obstacles.");
                Require(fluid.Profile != null && fluid.HalfSize.x > 0 && fluid.HalfSize.y > 0 && fluid.HalfSize.z > 0, id + ": invalid retained fluid bounds.");
                Require(Mathf.Abs(fluid.HalfSize.y - (level.InteriorDepth / 2 - .003f)) < .0001f, id + ": fluid depth does not match the shell.");
            }
        }

        private static void Require(bool condition, string error)
        {
            if (!condition) throw new InvalidOperationException("Campaign validation: " + error);
        }
    }
}
