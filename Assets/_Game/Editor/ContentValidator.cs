using System;
using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravityBox.Editor
{
    public static class ContentValidator
    {
        [MenuItem("Gravity Box/Validate Content")]
        public static void Validate()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(PrototypeBuilder.CatalogPath);
            Require(catalog != null && catalog.Levels != null && catalog.Levels.Length > 0, "Missing catalog or levels.");
            Require(catalog.BallPrefab != null && catalog.BallProfile != null && catalog.Rotation != null, "Missing shared configuration.");
            var ids = new HashSet<string>();
            foreach (LevelDefinition level in catalog.Levels)
            {
                Require(level != null, "Null level entry.");
                Require(!string.IsNullOrWhiteSpace(level.Id) && ids.Add(level.Id), "Empty/duplicate level ID: " + level.Id);
                Require(level.Environment != null && level.Prefab != null, level.Id + ": missing profile/prefab.");
                Require(level.Prefab.Exit != null && level.Prefab.BallSpawn != null && level.Prefab.Rotation != null, level.Id + ": missing scene contract.");
                Require(level.Prefab.transform.localScale == Vector3.one, level.Id + ": root scale must be one.");
                ValidateFootprint(level, catalog.BallProfile.Radius);
                var channels = new HashSet<string>();
                foreach (PressurePlate plate in level.Prefab.GetComponentsInChildren<PressurePlate>(true)) channels.Add(plate.Channel);
                foreach (SignalDoor door in level.Prefab.GetComponentsInChildren<SignalDoor>(true))
                    Require(door.Blocker != null && channels.Contains(door.Channel), level.Id + ": door missing blocker or plate channel.");
                var outlet = level.Prefab.Exit;
                Require(outlet.ApertureRadius > catalog.BallProfile.Radius * 1.1f, level.Id + ": aperture too narrow for ball.");
                Require(outlet.WallHalfDepth > 0 && outlet.WallHalfDepth < catalog.BallProfile.Radius, level.Id + ": invalid aperture wall thickness.");
                foreach (var renderer in outlet.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (renderer.GetComponentInParent<SignalDoor>(true) != null || renderer.GetComponentInParent<PhysicalProp>(true) != null) continue;
                    Require(renderer.GetComponent<Collider>() == null, level.Id + ": light inlay must not obstruct the ball: " + renderer.name + ".");
                }
                Vector3 escaped = outlet.transform.position + outlet.transform.forward * (outlet.WallHalfDepth + catalog.BallProfile.Radius * 1.05f);
                Require(!level.Prefab.IsOutside(escaped), level.Id + ": escape threshold must precede failure bounds.");
                string required = level.Prefab.Exit.RequiredChannel;
                Require(string.IsNullOrEmpty(required) || channels.Contains(required), level.Id + ": exit requires an unknown channel.");
                foreach (PhysicalProp prop in level.Prefab.GetComponentsInChildren<PhysicalProp>(true))
                {
                    Require(!prop.Body.isKinematic && prop.Body.mass > 0, level.Id + ": prop must be a free body with mass.");
                    Require(prop.transform.lossyScale == Vector3.one, level.Id + ": physical prop scale must be one.");
                    foreach (MeshCollider collider in prop.GetComponentsInChildren<MeshCollider>(true))
                        Require(collider.convex && collider.sharedMesh != null, level.Id + ": free prop requires convex collision meshes.");
                }
                ValidateSpawn(level, catalog.BallProfile.Radius);
            }
            Debug.Log("GRAVITY BOX CONTENT VALID: " + catalog.Levels.Length + " unique levels, spawn clearance and references checked.");
        }

        private static void ValidateSpawn(LevelDefinition definition, float radius)
        {
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                LevelRuntime level = UnityEngine.Object.Instantiate(definition.Prefab);
                SceneManager.MoveGameObjectToScene(level.gameObject, preview);
                var ball = new GameObject("Spawn clearance probe", typeof(SphereCollider));
                SceneManager.MoveGameObjectToScene(ball, preview);
                SphereCollider sphere = ball.GetComponent<SphereCollider>(); sphere.radius = radius;
                Vector3 spawn = level.BallSpawn.position;
                Require(!level.IsOutside(spawn), definition.Id + ": spawn outside bounds.");
                foreach (Collider solid in level.GetComponentsInChildren<Collider>(true))
                {
                    if (solid.isTrigger || !solid.enabled) continue;
                    bool overlap = UnityEngine.Physics.ComputePenetration(sphere, spawn, Quaternion.identity, solid,
                        solid.transform.position, solid.transform.rotation, out _, out float depth);
                    Require(!overlap || depth < radius * 0.01f, definition.Id + ": spawn overlaps " + solid.name);
                    if (solid.GetComponentInParent<SignalDoor>(true) != null || solid.GetComponentInParent<PhysicalProp>(true) != null) continue;
                    for (int sample = 0; sample <= 6; sample++)
                    {
                        float reach = radius * 1.1f + level.Exit.WallHalfDepth;
                        Vector3 point = level.Exit.transform.TransformPoint(new Vector3(0, 0, Mathf.Lerp(-reach, reach, sample / 6f)));
                        bool blocked = UnityEngine.Physics.ComputePenetration(sphere, point, Quaternion.identity, solid,
                            solid.transform.position, solid.transform.rotation, out _, out float obstruction);
                        Require(!blocked || obstruction < radius * 0.01f, definition.Id + ": aperture obstructed by " + solid.name);
                    }
                }
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }

        private static void ValidateFootprint(LevelDefinition definition, float radius)
        {
            LevelRuntime level = definition.Prefab;
            Require(level.Footprint != null && level.Footprint.Length >= 3, definition.Id + ": missing authored footprint.");
            foreach (Vector2 point in level.Footprint)
                Require(!float.IsNaN(point.x) && !float.IsNaN(point.y) && point.magnitude + radius < level.BoundsHalfExtent,
                    definition.Id + ": footprint exceeds framing/failure bounds.");
            Vector3 spawn = level.transform.InverseTransformPoint(level.BallSpawn.position);
            Vector3 exit = level.transform.InverseTransformPoint(level.Exit.transform.position);
            ValidateInterior(new Vector2(spawn.x, spawn.z), radius, "spawn");
            ValidateInterior(new Vector2(exit.x, exit.z), level.Exit.ApertureRadius, "exit");

            void ValidateInterior(Vector2 point, float clearance, string label)
            {
                Require(InPolygon(point, level.Footprint) && EdgeDistance(point, level.Footprint) > clearance,
                    definition.Id + ": " + label + " does not fit inside the outer contour.");
                foreach (Vector2Contour hole in level.FootprintVoids)
                {
                    Require(hole.Points != null && hole.Points.Length >= 3, definition.Id + ": invalid interior void.");
                    Require(!InPolygon(point, hole.Points) && EdgeDistance(point, hole.Points) > clearance,
                        definition.Id + ": " + label + " overlaps an interior void.");
                }
            }
        }

        private static bool InPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                Vector2 a = polygon[i], b = polygon[j];
                if ((a.y > point.y) != (b.y > point.y) && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                    inside = !inside;
            }
            return inside;
        }

        private static float EdgeDistance(Vector2 point, Vector2[] polygon)
        {
            float distance = float.PositiveInfinity;
            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 a = polygon[i], edge = polygon[(i + 1) % polygon.Length] - a;
                float t = Mathf.Clamp01(Vector2.Dot(point - a, edge) / Mathf.Max(edge.sqrMagnitude, 1e-12f));
                distance = Mathf.Min(distance, Vector2.Distance(point, a + t * edge));
            }
            return distance;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Gravity Box validation: " + message);
        }
    }
}
