#if UNITY_EDITOR
using System.Collections;
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
            // Sweep above the fixed cube, below the lid. A finite probe avoids
            // ambiguous zero-width rays exactly on adjacent mesh triangle edges.
            for (int index = 0; index < levels.Catalog.Levels.Length; index++)
            {
                Load(index);
                Vector3 origin = levels.Current.transform.TransformPoint(new Vector3(0, 0.036f, 0));
                for (int sample = 0; sample < 48; sample++)
                {
                    float angle = sample * Mathf.PI * 2 / 48;
                    Vector3 direction = levels.Current.transform.TransformDirection(new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)));
                    float nearest = float.PositiveInfinity;
                    Vector3 surface = Vector3.zero;
                    foreach (RaycastHit hit in UnityEngine.Physics.SphereCastAll(origin, Radius * 0.05f, direction, 0.3f, ~0, QueryTriggerInteraction.Ignore))
                        if (hit.collider.transform.IsChildOf(levels.Current.transform) && hit.distance < nearest)
                        {
                            nearest = hit.distance;
                            surface = levels.Current.transform.InverseTransformPoint(hit.point);
                        }
                    Assert.That(nearest, Is.InRange(0.065f, 0.24f), levels.Definition.Id + " has a missing side wall at direction " + sample);
                    AssertFootprint(new Vector2(surface.x, surface.z), levels.Definition.Shape);
                }
            }
        }

        private static void AssertFootprint(Vector2 point, ContainerShape shape)
        {
            if (shape == ContainerShape.Circle)
                Assert.That(point.magnitude, Is.EqualTo(0.17f).Within(0.001f));
            else if (shape == ContainerShape.Square)
                Assert.That(Mathf.Max(Mathf.Abs(point.x), Mathf.Abs(point.y)), Is.EqualTo(0.16f).Within(0.001f));
            else
            {
                Vector2[] vertices = { new Vector2(-0.17f, -0.09815f), new Vector2(0.17f, -0.09815f), new Vector2(0, 0.19630f) };
                float nearestEdge = float.PositiveInfinity;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector2 edge = vertices[(i + 1) % vertices.Length] - vertices[i];
                    Vector2 outward = new Vector2(edge.y, -edge.x).normalized;
                    float distance = Vector2.Dot(point - vertices[i], outward);
                    Assert.That(distance, Is.LessThan(0.001f), "Triangle wall must stay on its authored outline.");
                    nearestEdge = Mathf.Min(nearestEdge, Mathf.Abs(distance));
                }
                Assert.That(nearestEdge, Is.LessThan(0.001f));
            }
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
