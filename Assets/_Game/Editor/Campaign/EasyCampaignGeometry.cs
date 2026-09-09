using System;
using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Editor
{
    /// <summary>Small, editor-only vocabulary used to author the easy campaign variants.</summary>
    internal static class EasyCampaignGeometry
    {
        internal const float BallRadius = .015f;

        internal static Vector2 V(float x, float z) => new Vector2(x, z);

        internal static Vector2[] Rectangle(float halfX, float halfZ) => new[]
        {
            V(-halfX,-halfZ), V(halfX,-halfZ), V(halfX,halfZ), V(-halfX,halfZ)
        };

        internal static Vector2[] Circle(float radius, int segments = 64, bool clockwise = false)
        {
            var points = new Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                int j = clockwise ? segments - 1 - i : i;
                float a = j * Mathf.PI * 2 / segments;
                points[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            }
            return points;
        }

        internal static Vector2[] Star(float outer, float inner, int points, float phase = Mathf.PI * .5f)
        {
            var result = new Vector2[points * 2];
            for (int i = 0; i < result.Length; i++)
            {
                float radius = (i & 1) == 0 ? outer : inner;
                float a = phase + i * Mathf.PI / points;
                result[i] = V(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius);
            }
            Array.Reverse(result); // PhysicsLabGeometry expects the outer contour counter-clockwise in XZ.
            return result;
        }

        internal static MechanicalAuthoring Author(LevelRuntime level, CampaignBuildContext c)
            => new MechanicalAuthoring(level, c.Glass, c.Frame, c.Rim, c.Contact);

        internal static GameObject Wall(MechanicalAuthoring a, string name, Vector2 from, Vector2 to,
            float depth = .09f, float thickness = .010f, Material material = null)
        {
            Vector2 d = to - from;
            GameObject wall = a.Block(name,
                new Vector3((from.x + to.x) * .5f, 0, (from.y + to.y) * .5f),
                new Vector3(d.magnitude, depth - .006f, thickness), material != null ? material : a.Glass);
            wall.transform.localRotation = Quaternion.Euler(0, -Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, 0);
            return wall;
        }

        internal static void Pocket(MechanicalAuthoring a, string name, Vector2 centre, Vector2 mouth,
            float depth = .09f, float halfWidth = .045f, float length = .065f)
        {
            Vector2 forward = (mouth - centre).normalized;
            Vector2 side = new Vector2(-forward.y, forward.x);
            Vector2 back = centre - forward * length * .5f;
            Wall(a, name + " back", back - side * halfWidth, back + side * halfWidth, depth);
            Wall(a, name + " left cheek", back - side * halfWidth, mouth - side * halfWidth, depth);
            Wall(a, name + " right cheek", back + side * halfWidth, mouth + side * halfWidth, depth);
        }

        internal static void AddSecondBall(LevelRuntime level, Vector3 localPosition, string name = "BallSpawn B")
        {
            var spawn = new GameObject(name).transform;
            spawn.SetParent(level.transform, false);
            spawn.localPosition = localPosition;
            var spawns = new List<Transform>(level.AdditionalBallSpawns) { spawn };
            level.AdditionalBallSpawns = spawns.ToArray();
        }

        internal static void Remove(LevelRuntime level, params string[] exactNames)
        {
            var names = new HashSet<string>(exactNames);
            foreach (Transform child in level.GetComponentsInChildren<Transform>(true))
                if (child != level.transform && names.Contains(child.name)) UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        internal static void RemoveContaining(LevelRuntime level, params string[] fragments)
        {
            var remove = new List<GameObject>();
            foreach (Transform child in level.GetComponentsInChildren<Transform>(true))
            {
                if (child == level.transform) continue;
                foreach (string fragment in fragments)
                    if (child.name.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        remove.Add(child.gameObject);
                        break;
                    }
            }
            if (remove.Count == 0)
                throw new InvalidOperationException($"Campaign clone '{level.name}' has no transform containing [{string.Join(", ",fragments)}].");
            remove.Sort((a,b) => TransformDepth(b.transform).CompareTo(TransformDepth(a.transform)));
            foreach (GameObject target in remove)
                if (target != null) UnityEngine.Object.DestroyImmediate(target);
        }

        internal static void RemoveContainingUnder(LevelRuntime level, string parentName, string fragment)
        {
            Transform parent = Find(level, parentName);
            var remove = new List<GameObject>();
            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
                if (child != parent && child.name.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                    remove.Add(child.gameObject);
            if (remove.Count == 0)
                throw new InvalidOperationException($"Campaign clone '{level.name}' has no '{fragment}' below '{parentName}'.");
            // Delete deepest objects first so named decorative children do not leave invalid iteration state.
            remove.Sort((a,b) => TransformDepth(b.transform).CompareTo(TransformDepth(a.transform)));
            foreach (GameObject target in remove)
                if (target != null) UnityEngine.Object.DestroyImmediate(target);
        }

        private static int TransformDepth(Transform transform)
        {
            int depth=0;
            while(transform!=null) { depth++; transform=transform.parent; }
            return depth;
        }

        internal static Transform Find(LevelRuntime level, string exactName)
        {
            foreach (Transform child in level.GetComponentsInChildren<Transform>(true))
                if (child.name == exactName) return child;
            throw new InvalidOperationException($"Campaign clone '{level.name}' is missing required transform '{exactName}'.");
        }

        internal static void Move(LevelRuntime level, string exactName, Vector3 localPosition)
        {
            Transform target = Find(level, exactName);
            if (target != null) target.localPosition = localPosition;
        }

        internal static void Scale(LevelRuntime level, string exactName, Vector3 localScale)
        {
            Transform target = Find(level, exactName);
            if (target != null) target.localScale = localScale;
        }

        internal static void AddLiquid(LevelRuntime level, bool mercury, Vector3 halfSize)
        {
            if (mercury) WaterBoxBuilder.AddMercury(level.gameObject);
            else WaterBoxBuilder.AddWater(level.gameObject);
            WaterVolume volume = level.GetComponent<WaterVolume>();
            volume.HalfSize = halfSize;
            volume.Obstacle = new Bounds(Vector3.one * 20, Vector3.zero);
            WaterVisuals visuals = level.GetComponent<WaterVisuals>();
            if (visuals != null && visuals.VolumeRenderer != null)
                visuals.VolumeRenderer.transform.localScale = halfSize * 2;
        }

        internal static void FixedBaffle(MechanicalAuthoring a, string name, Vector2 from, Vector2 to,
            float depth = .09f, float thickness = .014f)
            => Wall(a, name, from, to, depth, thickness, a.Frame);

        internal static void TwoDeckTransfer(MechanicalAuthoring a, string prefix, bool turnRight)
        {
            // Two 6 mm plates at different heights. Their open ends overlap a broad 90 mm catch court.
            float direction = turnRight ? 1 : -1;
            a.Block(prefix + " upper deck", new Vector3(-.105f, .036f, -.050f * direction),
                new Vector3(.310f, .006f, .180f), a.Glass);
            a.Block(prefix + " lower catch deck", new Vector3(.105f, -.036f, .050f * direction),
                new Vector3(.310f, .006f, .230f), a.Glass);
            Wall(a, prefix + " upper guide", V(-.26f, .045f * direction), V(.035f, .045f * direction), .09f, .010f);
            Wall(a, prefix + " catch guide", V(-.035f, -.075f * direction), V(.26f, -.075f * direction), .09f, .010f);
        }
    }
}
