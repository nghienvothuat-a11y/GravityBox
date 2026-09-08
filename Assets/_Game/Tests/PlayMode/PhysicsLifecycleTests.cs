#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using SessionState = GravityBox.Foundation.SessionState;

namespace GravityBox.Tests
{
    // These tests exercise the shipped geometry and lifecycle. The separate
    // SteelBallPhysicsTests fixture measures material, acceleration and rolling laws.
    public sealed class PhysicsLifecycleTests
    {
        private LevelManager levels;
        private EnvironmentForceSystem forces;
        private GameObject services;
        private SimulationMode previousMode;
        private Vector3 previousGravity;
        private float previousFixedDelta, previousMaximumDelta, previousContactOffset, previousBounceThreshold;
        private int previousSolverIterations, previousSolverVelocityIterations;
        private const float Dt = 1f / 120f;
        private float Radius => levels.Ball.Profile.Radius;

        [SetUp]
        public void Setup()
        {
            previousMode = UnityEngine.Physics.simulationMode;
            previousGravity = UnityEngine.Physics.gravity;
            previousFixedDelta = Time.fixedDeltaTime;
            previousMaximumDelta = Time.maximumDeltaTime;
            previousContactOffset = UnityEngine.Physics.defaultContactOffset;
            previousBounceThreshold = UnityEngine.Physics.bounceThreshold;
            previousSolverIterations = UnityEngine.Physics.defaultSolverIterations;
            previousSolverVelocityIterations = UnityEngine.Physics.defaultSolverVelocityIterations;
            PhysicsTiming.Apply();
            Assert.That(Time.fixedDeltaTime, Is.EqualTo(Dt));
            Time.timeScale = 1;
            UnityEngine.Physics.simulationMode = SimulationMode.Script;
            UnityEngine.Physics.gravity = Vector3.zero;
            services = new GameObject("Catalog physics test services");
            forces = services.AddComponent<EnvironmentForceSystem>(); forces.enabled = false;
            levels = services.AddComponent<LevelManager>(); levels.enabled = false;
            levels.Initialize(AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/_Game/ScriptableObjects/LevelCatalog.asset"), forces);
            levels.Current.Rotation.enabled = false;
        }

        [TearDown]
        public void Teardown()
        {
            GameObject root = levels.Current != null ? levels.Current.gameObject : null;
            GameObject ball = levels.Ball != null ? levels.Ball.gameObject : null;
            Object.DestroyImmediate(services);
            if (root != null) Object.DestroyImmediate(root);
            if (ball != null) Object.DestroyImmediate(ball);
            UnityEngine.Physics.simulationMode = previousMode;
            UnityEngine.Physics.gravity = previousGravity;
            Time.fixedDeltaTime = previousFixedDelta;
            Time.maximumDeltaTime = previousMaximumDelta;
            UnityEngine.Physics.defaultContactOffset = previousContactOffset;
            UnityEngine.Physics.bounceThreshold = previousBounceThreshold;
            UnityEngine.Physics.defaultSolverIterations = previousSolverIterations;
            UnityEngine.Physics.defaultSolverVelocityIterations = previousSolverVelocityIterations;
            Time.timeScale = 1;
        }

        private void Load(int index)
        {
            levels.Load(index);
            levels.Current.Rotation.enabled = false;
        }

        private void Steps(int count)
        {
            for (int i = 0; i < count; i++)
            {
                levels.Current.Rotation.Step(Dt);
                forces.Step();
                UnityEngine.Physics.Simulate(Dt);
                levels.Current.Exit.EvaluateTraversal();
            }
        }

        private void PlaceAtExit(Vector3 localPosition, bool begin = false)
        {
            levels.Ball.Body.position = levels.Current.Exit.transform.TransformPoint(localPosition);
            UnityEngine.Physics.SyncTransforms();
            if (begin) levels.Current.Exit.BeginTracking();
            else levels.Current.Exit.EvaluateTraversal();
        }

        private void BeginExitApproach(Vector2 tangent = default)
        {
            PlaceAtExit(new Vector3(tangent.x, tangent.y, -levels.Current.Exit.WallHalfDepth - Radius * 1.2f), true);
        }

        private void LaunchThroughExit()
        {
            BeginExitApproach();
            levels.Ball.Body.linearVelocity = levels.Current.Exit.transform.forward * 1.5f;
            Steps(12);
        }

        [Test]
        public void EveryBox_LoadsOneWorldSpaceBallWithClearSpawnAndRestoresItsState()
        {
            for (int index = 0; index < levels.Catalog.Levels.Length; index++)
            {
                Load(index);
                Assert.That(forces.Environment, Is.SameAs(levels.Definition.Environment));
                Assert.That(forces.TargetCount, Is.EqualTo(1));
                Assert.That(levels.Ball.transform.parent, Is.Null);
                Assert.That(levels.Current.IsOutside(levels.Ball.Body.position), Is.False);
                SphereCollider sphere = levels.Ball.GetComponent<SphereCollider>();
                foreach (Collider solid in levels.Current.GetComponentsInChildren<Collider>())
                {
                    if (!solid.enabled || solid.isTrigger) continue;
                    bool overlap = UnityEngine.Physics.ComputePenetration(sphere, levels.Ball.Body.position, levels.Ball.Body.rotation,
                        solid, solid.transform.position, solid.transform.rotation, out _, out float depth);
                    Assert.That(!overlap || depth < Radius * 0.01f, Is.True, levels.Definition.Id + " spawn overlaps " + solid.name);
                }
                Steps(24);
                levels.ResetLevel();
                Assert.That(Vector3.Distance(levels.Ball.Body.position, levels.Current.BallSpawn.position), Is.LessThan(0.00001f));
                Assert.That(levels.Ball.Body.linearVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(levels.Ball.Body.angularVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(levels.Current.Resets.Count, Is.GreaterThanOrEqualTo(3));
            }
        }

        [Test]
        public void Reset100Times_RestoresBallRootAndExitWithoutAccumulation()
        {
            for (int index = 0; index < levels.Catalog.Levels.Length; index++)
            {
                Load(index);
                Vector3 position = levels.Ball.Body.position;
                Quaternion orientation = levels.Current.Rotation.Orientation;
                int registryCount = levels.Current.Resets.Count;
                for (int reset = 0; reset < 100; reset++)
                {
                    levels.Ball.Body.position += Vector3.one * Radius * 3;
                    levels.Ball.Body.linearVelocity = Vector3.one;
                    levels.Ball.Body.angularVelocity = Vector3.one * 20;
                    levels.Current.GetComponent<Rigidbody>().rotation = Quaternion.Euler(reset, reset * 3, 35);
                    levels.ResetLevel();
                    Assert.That(Vector3.Distance(levels.Ball.Body.position, position), Is.LessThan(0.00001f));
                    Assert.That(levels.Ball.Body.linearVelocity, Is.EqualTo(Vector3.zero));
                    Assert.That(levels.Ball.Body.angularVelocity, Is.EqualTo(Vector3.zero));
                    Assert.That(Quaternion.Angle(levels.Current.Rotation.Orientation, orientation), Is.LessThan(0.001f));
                    Assert.That(levels.Current.Exit.HasExited, Is.False);
                    Assert.That(levels.Current.Resets.Count, Is.EqualTo(registryCount));
                    Assert.That(forces.TargetCount, Is.EqualTo(1));
                    Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active));
                }
            }
        }

        [Test]
        public void Exit_RequiresWholeBallOutsideThenKeepsRealTimeMomentumAndEmitsOnce()
        {
            int count = 0;
            levels.GameplayEvent += e => { if (e == "level_complete") count++; };
            BeginExitApproach();
            PlaceAtExit(Vector3.zero);
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active));
            float clear = levels.Current.Exit.WallHalfDepth + Radius;
            PlaceAtExit(new Vector3(0, 0, clear - Radius * 0.1f));
            Assert.That(count, Is.Zero, "The back of the sphere is still inside the shell.");
            levels.Ball.Body.linearVelocity = levels.Current.Exit.transform.forward * 0.5f;
            Vector3 velocity = levels.Ball.Body.linearVelocity;
            PlaceAtExit(new Vector3(0, 0, clear + Radius * 0.25f));
            Vector3 escapedAt = levels.Ball.Body.position;
            Assert.That(count, Is.EqualTo(1));
            Assert.That(levels.Ball.IsCaptured, Is.False);
            Assert.That(levels.Ball.Body.isKinematic, Is.False);
            Assert.That(levels.Ball.Body.linearVelocity, Is.EqualTo(velocity));
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Completing));
            Assert.That(Time.timeScale, Is.EqualTo(1), "The experiment must stay at real speed after escape.");
            Steps(6);
            Assert.That(Vector3.Distance(escapedAt, levels.Ball.Body.position), Is.GreaterThan(Radius));
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void Exit_RejectsInwardTravelAndCrossingSolidWallElsewhere()
        {
            float outside = levels.Current.Exit.WallHalfDepth + Radius * 2;
            PlaceAtExit(new Vector3(0, 0, outside), true);
            PlaceAtExit(Vector3.zero);
            PlaceAtExit(new Vector3(0, 0, outside));
            Assert.That(levels.Current.Exit.HasExited, Is.False, "Must approach the opening from inside.");
            levels.ResetLevel();
            float offset = levels.Current.Exit.ApertureRadius + Radius;
            BeginExitApproach(new Vector2(offset, 0));
            PlaceAtExit(new Vector3(offset, 0, outside));
            Assert.That(levels.Current.Exit.HasExited, Is.False, "Crossing the wall away from the aperture must not count.");
        }

        [Test]
        public void Exit_SweptCheckDetectsOneStepEscapeAndResetClearsPartialPassage()
        {
            float outside = levels.Current.Exit.WallHalfDepth + Radius * 2;
            BeginExitApproach();
            PlaceAtExit(new Vector3(0, 0, outside));
            Assert.That(levels.Current.Exit.HasExited, Is.True);
            levels.ResetLevel();
            BeginExitApproach();
            PlaceAtExit(Vector3.zero);
            levels.ResetLevel();
            PlaceAtExit(Vector3.zero, true);
            PlaceAtExit(new Vector3(0, 0, outside));
            Assert.That(levels.Current.Exit.HasExited, Is.False, "Reset must discard the preceding passage.");
        }

        [Test]
        public void CircularExit_RejectsCornerOverlapButAcceptsRadialClearance()
        {
            float clearance = levels.Current.Exit.ApertureRadius - Radius;
            float outside = levels.Current.Exit.WallHalfDepth + Radius * 2;
            float corner = clearance * 0.85f;
            BeginExitApproach(new Vector2(corner, corner));
            PlaceAtExit(new Vector3(corner, corner, outside));
            Assert.That(levels.Current.Exit.HasExited, Is.False, "A sphere overlapping the circular rim is not clear.");
            levels.ResetLevel();
            float inside = clearance * 0.5f;
            BeginExitApproach(new Vector2(inside, inside));
            PlaceAtExit(new Vector3(inside, inside, outside));
            Assert.That(levels.Current.Exit.HasExited, Is.True);
        }

        [Test]
        public void EveryBox_HasARealClearApertureAndAColliderFreeLightInlay()
        {
            for (int index = 0; index < levels.Catalog.Levels.Length; index++)
            {
                Load(index);
                foreach (var renderer in levels.Current.Exit.GetComponentsInChildren<MeshRenderer>())
                    Assert.That(renderer.GetComponent<Collider>(), Is.Null, "The thin light ring must not create a lip.");
                LaunchThroughExit();
                Assert.That(levels.Current.Exit.HasExited, Is.True, levels.Definition.Id + " blocks the aperture.");
                Assert.That(levels.Ball.Body.isKinematic, Is.False);
                Vector3 local = levels.Current.Exit.transform.InverseTransformPoint(levels.Ball.Body.position);
                Assert.That(local.z, Is.GreaterThan(levels.Current.Exit.WallHalfDepth + Radius));
            }
        }

        [Test]
        public void EveryBox_HasUnbrokenSideWallsAroundItsActualFootprint()
        {
            // Approach each edge from the playable side; concave shapes and an
            // annulus do not necessarily contain the root origin.
            for (int index = 0; index < levels.Catalog.Levels.Length; index++)
            {
                Load(index);
                foreach (Vector2[] contour in Contours(levels.Current))
                {
                    bool hole = contour != levels.Current.Footprint;
                    float orientation = Mathf.Sign(SignedArea(contour)) * (hole ? -1 : 1);
                    for (int edgeIndex = 0; edgeIndex < contour.Length; edgeIndex++)
                    {
                        Vector2 a = contour[edgeIndex], b = contour[(edgeIndex + 1) % contour.Length];
                        Vector2 edge = b - a;
                        Vector2 intoDomain = new Vector2(-edge.y, edge.x).normalized * orientation;
                        Vector2 point = (a + b) * 0.5f;
                        Vector2 start = point + intoDomain * 0.006f;
                        Assert.That(InDomain(start, levels.Current), Is.True, levels.Definition.Id + " has insufficient wall-side clearance.");
                        Vector3 origin = levels.Current.transform.TransformPoint(new Vector3(start.x, 0.036f, start.y));
                        Vector3 direction = levels.Current.transform.TransformDirection(new Vector3(-intoDomain.x, 0, -intoDomain.y));
                        float nearest = float.PositiveInfinity;
                        Vector3 surface = Vector3.zero;
                        foreach (RaycastHit hit in UnityEngine.Physics.SphereCastAll(origin, Radius * 0.05f, direction, 0.012f, ~0, QueryTriggerInteraction.Ignore))
                            if (hit.collider.transform.IsChildOf(levels.Current.transform) && hit.distance < nearest)
                            {
                                nearest = hit.distance;
                                surface = levels.Current.transform.InverseTransformPoint(hit.point);
                            }
                        Assert.That(nearest, Is.InRange(0.004f, 0.007f), levels.Definition.Id + " missing contour edge " + edgeIndex);
                        Assert.That(DistanceToSegment(new Vector2(surface.x, surface.z), a, b), Is.LessThan(0.001f));
                    }
                }
            }
        }

        [Test]
        public void EveryBox_FloorAndCoverFollowConcavitiesAndLeaveVoidRegionsEmpty()
        {
            for (int index = 0; index < levels.Catalog.Levels.Length; index++)
            {
                Load(index);
                Collider floor = levels.Current.transform.Find("Floor with circular cut").GetComponent<Collider>();
                Collider cover = levels.Current.transform.Find("Clear top cover").GetComponent<Collider>();
                Assert.That(floor, Is.Not.Null);
                Assert.That(cover, Is.Not.Null);
                Vector3 outlet = levels.Current.transform.InverseTransformPoint(levels.Current.Exit.transform.position);
                float bound = levels.Current.BoundsHalfExtent;
                int supported = 0, empty = 0;
                for (float x = -bound + 0.011f; x < bound; x += Radius * 1.5f)
                for (float z = -bound + 0.007f; z < bound; z += Radius * 1.5f)
                {
                    Vector2 point = new Vector2(x, z);
                    if (DistanceToBoundary(point, levels.Current) < 0.003f) continue;
                    bool domain = InDomain(point, levels.Current);
                    float apertureDistance = Vector2.Distance(point, new Vector2(outlet.x, outlet.z));
                    bool nearRim = Mathf.Abs(apertureDistance - levels.Current.Exit.ApertureRadius) < 0.002f;
                    Vector3 origin = levels.Current.transform.TransformPoint(new Vector3(x, 0, z));
                    bool floorHit = floor.Raycast(new Ray(origin, -levels.Current.transform.up), out _, 0.1f);
                    bool coverHit = cover.Raycast(new Ray(origin, levels.Current.transform.up), out _, 0.1f);
                    Assert.That(coverHit, Is.EqualTo(domain), levels.Definition.Id + " cover at " + point);
                    if (!nearRim)
                        Assert.That(floorHit, Is.EqualTo(domain && apertureDistance > levels.Current.Exit.ApertureRadius), levels.Definition.Id + " floor at " + point);
                    if (domain) supported++; else empty++;
                }
                Assert.That(supported, Is.GreaterThan(20));
                Assert.That(empty, Is.GreaterThan(20));
            }
        }

        private static IEnumerable<Vector2[]> Contours(LevelRuntime level)
        {
            yield return level.Footprint;
            foreach (var hole in level.FootprintVoids) yield return hole.Points;
        }

        private static bool InDomain(Vector2 point, LevelRuntime level)
        {
            if (!InPolygon(point, level.Footprint)) return false;
            foreach (var hole in level.FootprintVoids) if (InPolygon(point, hole.Points)) return false;
            return true;
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

        private static float SignedArea(Vector2[] polygon)
        {
            float area = 0;
            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 a = polygon[i], b = polygon[(i + 1) % polygon.Length];
                area += a.x * b.y - b.x * a.y;
            }
            return area * 0.5f;
        }

        private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            float t = Mathf.Clamp01(Vector2.Dot(point - a, b - a) / (b - a).sqrMagnitude);
            return Vector2.Distance(point, Vector2.Lerp(a, b, t));
        }

        private static float DistanceToBoundary(Vector2 point, LevelRuntime level)
        {
            float distance = float.PositiveInfinity;
            foreach (Vector2[] contour in Contours(level))
                for (int i = 0; i < contour.Length; i++)
                    distance = Mathf.Min(distance, DistanceToSegment(point, contour[i], contour[(i + 1) % contour.Length]));
            return distance;
        }

        [Test]
        public void SquareFixedCube_ProducesARealCollisionAndRebound()
        {
            Load(1);
            BoxCollider cube = null;
            foreach (BoxCollider candidate in levels.Current.GetComponentsInChildren<BoxCollider>())
                if (candidate.name == "Fixed cube") cube = candidate;
            Assert.That(cube, Is.Not.Null);
            Assert.That(cube.attachedRigidbody, Is.SameAs(levels.Current.GetComponent<Rigidbody>()));
            Assert.That(cube.attachedRigidbody.isKinematic, Is.True);
            Vector3 size = Vector3.Scale(cube.size, cube.transform.lossyScale);
            Assert.That(size.x, Is.EqualTo(size.y).Within(0.0001f));
            Assert.That(size.z, Is.EqualTo(size.y).Within(0.0001f));
            Vector3 center = cube.transform.TransformPoint(cube.center);
            Vector3 direction = cube.transform.right;
            levels.Ball.Body.position = center - direction * (size.x * 0.5f + Radius * 1.2f);
            levels.Ball.Body.linearVelocity = direction * 0.65f;
            float strongestImpact = 0;
            levels.Ball.Impact += impulse => strongestImpact = Mathf.Max(strongestImpact, impulse);
            UnityEngine.Physics.SyncTransforms();
            float lowestNormalVelocity = 0;
            for (int tick = 0; tick < 12; tick++)
            {
                Steps(1);
                lowestNormalVelocity = Mathf.Min(lowestNormalVelocity, Vector3.Dot(levels.Ball.Body.linearVelocity, direction));
                Assert.That(Vector3.Dot(levels.Ball.Body.position - center, direction), Is.LessThan(-size.x * 0.5f));
            }
            Assert.That(strongestImpact, Is.GreaterThan(0.01f), "The cube must create contact impulse.");
            Assert.That(lowestNormalVelocity, Is.LessThan(-0.02f), "The ball must rebound from the cube surface.");
        }

        [Test]
        public void EveryBox_RetainsBallDuringModerateRotationsUnlessItUsesTheExit()
        {
            Quaternion[] orientations = {
                Quaternion.Euler(15, 0, 0), Quaternion.Euler(-15, 0, 18),
                Quaternion.Euler(12, 0, -18), Quaternion.identity
            };
            for (int index = 0; index < levels.Catalog.Levels.Length; index++)
            {
                Load(index);
                foreach (Quaternion orientation in orientations)
                {
                    levels.Current.Rotation.SetTargetOrientation(orientation);
                    for (int tick = 0; tick < 180; tick++)
                    {
                        Steps(1);
                        if (levels.Current.Exit.HasExited) break;
                        Assert.That(levels.Current.IsOutside(levels.Ball.Body.position), Is.False, levels.Definition.Id + " leaked through a wall.");
                    }
                    if (levels.Current.Exit.HasExited) break;
                }
            }
        }

        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        public void NewShape_FullSphereCanFollowItsBendsAndNecksByTiltingOnly(int index)
        {
            Load(index);
            Vector2[] route = PassabilityRoute(index);
            Vector3 outlet = levels.Current.transform.InverseTransformPoint(levels.Current.Exit.transform.position);
            float height = outlet.y + levels.Current.Exit.WallHalfDepth + Radius + 0.001f;
            // This fixture tests a controlled passage, not the player's solution.
            // It sets one initial state, then only changes box rotation intent.
            for (int segment = 1; segment < route.Length; segment++)
            {
                Vector2 a = route[segment - 1], b = route[segment];
                int samples = Mathf.CeilToInt(Vector2.Distance(a, b) / (Radius * 0.5f));
                for (int sample = 0; sample <= samples; sample++)
                {
                    Vector2 point = Vector2.Lerp(a, b, sample / (float)samples);
                    Assert.That(InDomain(point, levels.Current), Is.True, "Route must stay on the actual floor domain.");
                    Assert.That(DistanceToBoundary(point, levels.Current), Is.GreaterThan(Radius + 0.001f), "The whole sphere needs passage clearance.");
                }
                Vector3 start = levels.Current.transform.TransformPoint(new Vector3(a.x, height, a.y));
                Vector3 end = levels.Current.transform.TransformPoint(new Vector3(b.x, height, b.y));
                foreach (RaycastHit hit in UnityEngine.Physics.SphereCastAll(start, Radius, (end - start).normalized, Vector3.Distance(start, end), ~0, QueryTriggerInteraction.Ignore))
                    Assert.That(hit.collider.transform.IsChildOf(levels.Current.transform), Is.False, "Passage is obstructed by " + hit.collider.name);
            }
            levels.Ball.Body.position = levels.Current.transform.TransformPoint(new Vector3(route[0].x, height, route[0].y));
            levels.Ball.Body.linearVelocity = levels.Ball.Body.angularVelocity = Vector3.zero;
            UnityEngine.Physics.SyncTransforms();
            levels.Current.Exit.BeginTracking();
            Rigidbody box = levels.Current.GetComponent<Rigidbody>();
            for (int waypoint = 1; waypoint < route.Length; waypoint++)
            {
                bool reached = false;
                float closest = float.PositiveInfinity;
                int elapsedTicks = 0;
                for (int tick = 0; tick < 1200; tick++)
                {
                    Transform root = levels.Current.transform;
                    Vector3 local = root.InverseTransformPoint(levels.Ball.Body.position);
                    Vector3 relativeVelocity = root.InverseTransformDirection(levels.Ball.Body.linearVelocity - box.GetPointVelocity(levels.Ball.Body.position));
                    Vector2 error = route[waypoint] - new Vector2(local.x, local.z);
                    Vector2 velocity = new Vector2(relativeVelocity.x, relativeVelocity.z);
                    closest = Mathf.Min(closest, error.magnitude);
                    // A slow tilt policy leaves time for the bounded hand controller
                    // to brake and reverse before the ball reaches a bend.
                    Vector2 acceleration = Vector2.ClampMagnitude(error * 8f - velocity * 5f, 1f);
                    Vector3 desiredLocalGravity = new Vector3(acceleration.x, -9.81f, acceleration.y);
                    levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(desiredLocalGravity, Vector3.down));
                    Steps(1);
                    elapsedTicks = tick + 1;
                    if (levels.Current.Exit.HasExited)
                    {
                        Assert.That(waypoint, Is.EqualTo(route.Length - 1), "Must traverse every authored bend before exiting.");
                        reached = true;
                        break;
                    }
                    Assert.That(levels.Current.IsOutside(levels.Ball.Body.position), Is.False, "Ball escaped through a wall along the passage.");
                    if (error.magnitude < Radius * 1.1f && velocity.magnitude < 0.16f)
                    {
                        reached = true;
                        break;
                    }
                }
                Vector3 finalLocal = levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position);
                TestContext.WriteLine($"PASSAGE {levels.Definition.Id}, waypoint {waypoint}: reached={reached}, {elapsedTicks * Dt:F3} s, closest distance {closest:F5} m, final local {finalLocal:F5}.");
                Assert.That(reached, Is.True, levels.Definition.Id + " cannot reach passage waypoint " + waypoint + "; local " + finalLocal.ToString("F5") + "; closest distance " + closest.ToString("F5"));
            }
        }

        private static Vector2[] PassabilityRoute(int index)
        {
            switch (index)
            {
                case 3: return new[] { new Vector2(-0.155f, 0.155f), new Vector2(-0.155f, -0.15f), new Vector2(0.185f, -0.155f) };
                case 4: return new[] { new Vector2(-0.165f, 0.155f), new Vector2(-0.165f, -0.15f), new Vector2(0.165f, -0.15f), new Vector2(0.165f, 0.155f) };
                case 5:
                    var arc = new Vector2[5];
                    for (int i = 0; i < arc.Length; i++)
                    {
                        float angle = (180f - i * 45f) * Mathf.Deg2Rad;
                        arc[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.1825f;
                    }
                    return arc;
                case 6: return new[] { new Vector2(-0.225f, 0), Vector2.zero, new Vector2(0.235f, 0) };
                case 7: return new[] { new Vector2(-0.115f, -0.14f), Vector2.zero, new Vector2(0, 0.207f) };
                default: throw new System.ArgumentOutOfRangeException(nameof(index));
            }
        }

        [UnityTest]
        public IEnumerator CompletionWaitsForManualNextAndResetKeepsCurrentExperiment()
        {
            levels.enabled = true;
            LaunchThroughExit();
            Assert.That(levels.Current.Exit.HasExited, Is.True);
            yield return new WaitForSecondsRealtime(levels.Catalog.CompletionDelay + 0.1f);
            Assert.That(levels.Index, Is.Zero, "An experiment must never advance on a timer.");
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Completing));
            Assert.That(Time.timeScale, Is.EqualTo(1));
            levels.ResetLevel();
            Assert.That(levels.Current.Exit.HasExited, Is.False);
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active));
            levels.Next();
            Assert.That(levels.Index, Is.EqualTo(1));
            Assert.That(forces.TargetCount, Is.EqualTo(1));
        }
    }
}
#endif
