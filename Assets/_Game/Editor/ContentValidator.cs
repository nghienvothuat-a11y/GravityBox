using System;
using System.Collections.Generic;
using GravityBox.Gameplay;
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
                var channels = new HashSet<string>();
                foreach (PressurePlate plate in level.Prefab.GetComponentsInChildren<PressurePlate>(true)) channels.Add(plate.Channel);
                foreach (SignalDoor door in level.Prefab.GetComponentsInChildren<SignalDoor>(true))
                    Require(door.Blocker != null && channels.Contains(door.Channel), level.Id + ": door missing blocker or plate channel.");
                string required = level.Prefab.Exit.RequiredChannel;
                Require(string.IsNullOrEmpty(required) || channels.Contains(required), level.Id + ": exit requires an unknown channel.");
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
                    Require(!overlap || depth < 0.005f, definition.Id + ": spawn overlaps " + solid.name);
                }
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Gravity Box validation: " + message);
        }
    }
}
