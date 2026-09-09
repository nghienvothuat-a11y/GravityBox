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
                Require(level.Prefab.InteriorDepth > catalog.BallProfile.Radius * 2 + .006f,
                    level.Id + ": shell depth must contain the complete ball.");
                ValidateSpatialMaze(level, catalog.BallProfile.Radius);
                WaterVolume water = level.Prefab.GetComponent<WaterVolume>();
                if (level.Shape == ContainerShape.WaterBox)
                {
                    Require(water != null && water.Profile != null, level.Id + ": water experiment needs a fluid profile.");
                    Require(water.Profile.Density > 990 && water.Profile.Density < 1010 && water.Profile.DynamicViscosity > 0,
                        level.Id + ": invalid fresh water properties.");
                    Require(water.HalfSize == new Vector3(.16f, .042f, .16f), level.Id + ": water must fill the square interior.");
                    var visuals = level.Prefab.GetComponent<GravityBox.Presentation.WaterVisuals>();
                    Require(visuals != null && visuals.VolumeRenderer != null && visuals.FloorRenderer != null && visuals.TracerMaterial != null,
                        level.Id + ": missing water presentation.");
                    Require(visuals.VolumeRenderer.GetComponent<Collider>() == null, level.Id + ": optical volume must not block the ball.");
                }
                else Require(water == null, level.Id + ": dry experiment unexpectedly has water forces.");
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
                foreach (GravitySliderGuide slider in level.Prefab.GetComponentsInChildren<GravitySliderGuide>(true))
                {
                    ConfigurableJoint joint = slider.GetComponent<ConfigurableJoint>();
                    Require(joint != null && joint.connectedBody == level.Prefab.GetComponent<Rigidbody>(),
                        level.Id + ": slider rail must be connected to its rotating box.");
                    Require(joint.xMotion == ConfigurableJointMotion.Limited &&
                        joint.yMotion == ConfigurableJointMotion.Locked && joint.zMotion == ConfigurableJointMotion.Locked &&
                        joint.angularXMotion == ConfigurableJointMotion.Locked && joint.angularYMotion == ConfigurableJointMotion.Locked &&
                        joint.angularZMotion == ConfigurableJointMotion.Locked, level.Id + ": slider must have one constrained travel axis.");
                    Require(joint.xDrive.positionSpring == 0 && joint.xDrive.positionDamper == 0 &&
                        joint.linearLimitSpring.spring == 0 && joint.linearLimitSpring.damper == 0,
                        level.Id + ": gravity slider must not be motor or spring driven.");
                }
                LayeredMaze layered = level.Prefab.GetComponent<LayeredMaze>();
                if (layered != null)
                {
                    Require(layered.Decks != null && layered.Decks.Length >= 2, level.Id + ": layered maze requires multiple decks.");
                    Require(layered.TransferPorts != null && layered.TransferPorts.Length == layered.Decks.Length - 1,
                        level.Id + ": each internal deck needs a real transfer opening.");
                    for (int i = 0; i < layered.Decks.Length; i++)
                    {
                        Require(layered.Decks[i].FloorCollider != null, level.Id + ": a deck is missing its physical floor.");
                        if (i > 0) Require(layered.Decks[i - 1].FloorHeight - layered.Decks[i].FloorHeight > catalog.BallProfile.Radius * 2 + .006f,
                            level.Id + ": decks must be ordered from top to bottom with clearance for the sphere.");
                    }
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
            // A sphere is defined by its radial shell, not an extruded XZ contour.
            if (level.GetComponent<SpatialMaze>() != null) return;
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

        private static void ValidateSpatialMaze(LevelDefinition definition, float radius)
        {
            SpatialMaze maze = definition.Prefab.GetComponent<SpatialMaze>();
            if (definition.Shape != ContainerShape.SphereMaze)
            {
                Require(maze == null, definition.Id + ": spatial metadata requires a spherical container.");
                return;
            }
            Require(maze != null && maze.ShellCollider != null && maze.ShellCollider.sharedMesh != null,
                definition.Id + ": spherical maze requires its real hollow shell.");
            Require(!maze.ShellCollider.convex && maze.InnerRadius > radius * 4 && maze.ShellThickness > 0,
                definition.Id + ": invalid spherical shell.");
            Require(maze.InnerRadius + maze.ShellThickness + radius < definition.Prefab.BoundsHalfExtent,
                definition.Id + ": spherical shell exceeds framing/failure bounds.");
            Require(maze.ClearWidth > radius * 2 + .006f && maze.SightGap < radius * 2 - .004f,
                definition.Id + ": the route must fit the ball while sight gaps retain it.");
            Require(maze.NodesLocal != null && maze.NodesLocal.Length > 2 && maze.Edges != null &&
                maze.Edges.Length == maze.NodesLocal.Length - 1, definition.Id + ": connected spatial maze must be a tree.");
            Require(maze.SpawnNode >= 0 && maze.SpawnNode < maze.NodesLocal.Length &&
                maze.ExitNode >= 0 && maze.ExitNode < maze.NodesLocal.Length && maze.SpawnNode != maze.ExitNode,
                definition.Id + ": invalid maze endpoints.");
            Require(maze.MainPath != null && maze.MainPath.Length > 2 && maze.MainPath[0] == maze.SpawnNode &&
                maze.MainPath[maze.MainPath.Length - 1] == maze.ExitNode, definition.Id + ": invalid authored route endpoints.");
            var sections = new HashSet<MeshCollider>();
            var links = new HashSet<int>();
            var neighbours = new List<int>[maze.NodesLocal.Length];
            for (int n = 0; n < neighbours.Length; n++) neighbours[n] = new List<int>();
            foreach (SpatialMazeEdge edge in maze.Edges)
            {
                Require(edge.A >= 0 && edge.A < neighbours.Length && edge.B >= 0 && edge.B < neighbours.Length && edge.A != edge.B,
                    definition.Id + ": invalid connected section.");
                int key = Mathf.Min(edge.A, edge.B) * neighbours.Length + Mathf.Max(edge.A, edge.B);
                Require(links.Add(key), definition.Id + ": duplicate connection.");
                Vector3 delta = maze.NodesLocal[edge.B] - maze.NodesLocal[edge.A];
                Require(edge.Axis >= 0 && edge.Axis <= 2 && Mathf.Abs(delta[edge.Axis]) > .01f &&
                    Mathf.Abs(delta[(edge.Axis + 1) % 3]) < .00001f && Mathf.Abs(delta[(edge.Axis + 2) % 3]) < .00001f,
                    definition.Id + ": sections must meet along their authored axis.");
                neighbours[edge.A].Add(edge.B); neighbours[edge.B].Add(edge.A);
                ValidateSection(edge.Collider);
            }
            Require(maze.JunctionColliders != null && maze.JunctionColliders.Length == maze.NodesLocal.Length - 1,
                definition.Id + ": missing physical junctions.");
            foreach (MeshCollider junction in maze.JunctionColliders) ValidateSection(junction);
            var reached = new HashSet<int> { maze.SpawnNode };
            var pending = new Queue<int>(); pending.Enqueue(maze.SpawnNode);
            while (pending.Count > 0)
                foreach (int neighbour in neighbours[pending.Dequeue()])
                    if (reached.Add(neighbour)) pending.Enqueue(neighbour);
            Require(reached.Count == neighbours.Length, definition.Id + ": disconnected maze branch.");
            var pathNodes = new HashSet<int>();
            for (int i = 0; i < maze.MainPath.Length; i++)
            {
                int node = maze.MainPath[i];
                Require(node >= 0 && node < neighbours.Length && pathNodes.Add(node), definition.Id + ": invalid route node.");
                if (i > 0) Require(neighbours[maze.MainPath[i - 1]].Contains(node), definition.Id + ": route crosses a closed junction.");
            }
            Require(maze.Planks != null && maze.Planks.Length > 0, definition.Id + ": missing physical plank geometry.");
            foreach (SpatialMazePlank plank in maze.Planks)
            {
                Require(plank.SectionCollider != null && sections.Contains(plank.SectionCollider),
                    definition.Id + ": plank does not belong to a physical section.");
                Require(plank.Size.x > 0 && plank.Size.y > 0 && plank.Size.z > 0, definition.Id + ": invalid plank dimensions.");
                // Small overlapping joins are intentional: connected planks form
                // the maze boundary. The collision mesh uses these same boxes.
                for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 point = plank.CentreLocal + plank.RotationLocal * Vector3.Scale(plank.Size * .5f, new Vector3(x, y, z));
                    Require(point.magnitude <= maze.InnerRadius + maze.ShellThickness + .001f,
                        definition.Id + ": a connected plank protrudes beyond the spherical shell.");
                }
            }
            void ValidateSection(MeshCollider collider)
            {
                Require(collider != null && collider.enabled && !collider.isTrigger && !collider.convex && collider.sharedMesh != null && sections.Add(collider),
                    definition.Id + ": missing or duplicate physical maze section.");
            }
            Vector3 spawn = definition.Prefab.transform.InverseTransformPoint(definition.Prefab.BallSpawn.position);
            Require(spawn.magnitude + radius < maze.InnerRadius, definition.Id + ": spawn does not fit inside the sphere.");
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
