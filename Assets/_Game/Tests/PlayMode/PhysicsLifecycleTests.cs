#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GravityBox.Foundation;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using SessionState = GravityBox.Foundation.SessionState;

namespace GravityBox.Tests
{
    public sealed class PhysicsLifecycleTests
    {
        private LevelManager levels;
        private EnvironmentForceSystem forces;
        private GameObject services;
        private SimulationMode previousMode;
        private float previousFixedDelta;
        private float previousMaximumDelta;
        private const float Dt = 1f / 60;

        [SetUp]
        public void Setup()
        {
            previousMode = UnityEngine.Physics.simulationMode;
            previousFixedDelta = Time.fixedDeltaTime;
            previousMaximumDelta = Time.maximumDeltaTime;
            // MoveRotation/contact prediction must use the same clock as Physics.Simulate.
            // Do not depend on an earlier scene/input test having run GameBootstrap.Awake.
            Time.fixedDeltaTime = Dt;
            Time.maximumDeltaTime = 0.1f;
            UnityEngine.Physics.simulationMode = SimulationMode.Script;
            UnityEngine.Physics.gravity = Vector3.zero;
            services = new GameObject("Test services");
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
            Time.fixedDeltaTime = previousFixedDelta;
            Time.maximumDeltaTime = previousMaximumDelta;
            Time.timeScale = 1;
        }

        private void Load(int i)
        {
            levels.Load(i); levels.Current.Rotation.enabled = false;
            foreach (OneWayGate gate in levels.Current.GetComponentsInChildren<OneWayGate>()) gate.enabled = false;
        }

        private void Steps(int count)
        {
            for (int i = 0; i < count; i++)
            {
                levels.Current.Rotation.Step(Dt);
                foreach (OneWayGate gate in levels.Current.GetComponentsInChildren<OneWayGate>()) gate.Step();
                forces.Step();
                UnityEngine.Physics.Simulate(Dt);
                levels.Current.Exit.EvaluateTraversal();
            }
        }

        [Test]
        public void Gravity_AcceleratesAnUnobstructedBallDownward()
        {
            levels.Ball.Body.position = new Vector3(0, 1.5f, 0);
            levels.Ball.Body.linearVelocity = Vector3.zero;
            UnityEngine.Physics.SyncTransforms(); Steps(6);
            Assert.That(levels.Ball.Body.linearVelocity.y, Is.InRange(-1.0f, -0.95f));
            Assert.That(Mathf.Abs(levels.Ball.Body.linearVelocity.x), Is.LessThan(0.001f));
        }

        [Test]
        public void ZeroG_RetainsMomentumThrough120Steps()
        {
            Load(10);
            Vector3 initial = new Vector3(0.3f, 0.12f, 0.08f);
            levels.Ball.Body.position = Vector3.zero; levels.Ball.Body.linearVelocity = initial;
            UnityEngine.Physics.SyncTransforms(); Steps(120);
            Assert.That(Vector3.Distance(levels.Ball.Body.linearVelocity, initial), Is.LessThan(0.001f));
            Assert.That(Vector3.Distance(levels.Ball.Body.position, initial * 2), Is.LessThan(0.002f));
        }

        [Test]
        public void ZeroG_RotatingRootDoesNotRotateUntouchedBallVelocity()
        {
            Load(10);
            Vector3 initial = new Vector3(0.12f, 0.05f, 0.1f);
            levels.Ball.Body.position = Vector3.zero; levels.Ball.Body.linearVelocity = initial;
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(70, 50, 25));
            UnityEngine.Physics.SyncTransforms(); Steps(90);
            Assert.That(levels.Ball.transform.parent, Is.Null);
            Assert.That(Vector3.Distance(levels.Ball.Body.linearVelocity, initial), Is.LessThan(0.001f));
            Assert.That(Quaternion.Angle(levels.Current.Rotation.Orientation, Quaternion.identity), Is.GreaterThan(30));
        }

        [Test]
        public void Rotation_RespectsAngularSpeedLimit()
        {
            Quaternion initial = levels.Current.Rotation.Orientation;
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(0, 0, 175)); Steps(1);
            Assert.That(Quaternion.Angle(initial, levels.Current.Rotation.Orientation), Is.LessThanOrEqualTo(levels.Catalog.Rotation.MaxDegreesPerSecond * Dt + 0.02f));
        }

        [Test]
        public void Reset100Times_RestoresBallRootSignalsAndDoorWithoutAccumulation()
        {
            Load(8);
            Vector3 initialPosition = levels.Ball.Body.position;
            int registryCount = levels.Current.Resets.Count;
            PressurePlate plate = levels.Current.Plates[0];
            SignalDoor door = levels.Current.GetComponentInChildren<SignalDoor>();
            for (int i = 0; i < 100; i++)
            {
                levels.Ball.Body.position = new Vector3(0.1f, i % 3, 0.3f);
                levels.Ball.Body.linearVelocity = Vector3.one * 8;
                levels.Ball.Body.angularVelocity = Vector3.one * 4;
                levels.Current.GetComponent<Rigidbody>().rotation = Quaternion.Euler(i, i * 3, 35);
                plate.SetActive(true);
                Assert.That(door.IsActive, Is.True);
                levels.ResetLevel();
                Assert.That(Vector3.Distance(levels.Ball.Body.position, initialPosition), Is.LessThan(0.00001f));
                Assert.That(levels.Ball.Body.linearVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(levels.Ball.Body.angularVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(Quaternion.Angle(levels.Current.Rotation.Orientation, Quaternion.identity), Is.LessThan(0.001f));
                Assert.That(plate.IsActive || door.IsActive, Is.False);
                Assert.That(door.Blocker.enabled, Is.True);
                Assert.That(levels.Current.Signals.Read(plate.Channel), Is.False);
                Assert.That(levels.Current.Resets.Count, Is.EqualTo(registryCount));
                Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active));
            }
        }

        [Test]
        public void Reset_RestoresZeroGLaunchVelocity()
        {
            Load(10); levels.Ball.Body.linearVelocity = Vector3.one * -3; levels.ResetLevel();
            Assert.That(levels.Ball.Body.linearVelocity, Is.EqualTo(levels.Definition.InitialLocalVelocity));
        }

        private void PlaceAtExit(Vector3 localPosition, bool begin = false)
        {
            levels.Ball.Body.position = levels.Current.Exit.transform.TransformPoint(localPosition);
            UnityEngine.Physics.SyncTransforms();
            if (begin) levels.Current.Exit.BeginTracking();
            else levels.Current.Exit.EvaluateTraversal();
        }

        private void LaunchThroughExit()
        {
            PlaceAtExit(new Vector3(0, 0, -0.7f), true);
            levels.Ball.Body.linearVelocity = levels.Current.Exit.transform.forward * 6;
            Steps(18);
        }

        [Test]
        public void Exit_RequiresWholeBallOutsideThenKeepsMomentumAndEmitsOnce()
        {
            Load(10);
            int count = 0;
            levels.GameplayEvent += e => { if (e == "level_complete") count++; };
            PlaceAtExit(new Vector3(0, 0, -0.7f), true);
            PlaceAtExit(Vector3.zero);
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active), "Touching the opening is not victory.");
            float clear = levels.Current.Exit.WallHalfDepth + levels.Ball.Profile.Radius;
            PlaceAtExit(new Vector3(0, 0, clear - 0.01f));
            Assert.That(count, Is.Zero, "The rear of the sphere still overlaps the outlet.");
            levels.Ball.Body.linearVelocity = levels.Current.Exit.transform.forward * 2;
            Vector3 velocity = levels.Ball.Body.linearVelocity;
            PlaceAtExit(new Vector3(0, 0, clear + 0.03f));
            Vector3 position = levels.Ball.Body.position;
            Assert.That(count, Is.EqualTo(1));
            Assert.That(levels.Ball.IsCaptured, Is.False);
            Assert.That(levels.Ball.Body.isKinematic, Is.False);
            Assert.That(levels.Ball.Body.linearVelocity, Is.EqualTo(velocity));
            Assert.That(levels.Current.Rotation.InputEnabled, Is.False);
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Completing));
            Steps(6);
            Assert.That(Vector3.Distance(position, levels.Ball.Body.position), Is.GreaterThan(0.15f));
            Assert.That(count, Is.EqualTo(1));
            levels.ResetLevel();
            Assert.That(levels.Current.Exit.HasExited, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }

        [Test]
        public void Exit_RejectsOutsideInwardTravelAndMissedAperture()
        {
            Load(10);
            PlaceAtExit(new Vector3(0, 0, 1), true);
            PlaceAtExit(Vector3.zero);
            PlaceAtExit(new Vector3(0, 0, 1));
            Assert.That(levels.Current.Exit.HasExited, Is.False, "Must first approach from inside.");
            levels.ResetLevel();
            PlaceAtExit(new Vector3(1.2f, 0, -1), true);
            PlaceAtExit(new Vector3(1.2f, 0, 1));
            Assert.That(levels.Current.Exit.HasExited, Is.False, "Crossing the wall elsewhere is not a win.");
        }

        [Test]
        public void Exit_SweptCheckDetectsOneStepTraversalAndResetClearsPartialPassage()
        {
            Load(10);
            PlaceAtExit(new Vector3(0, 0, -1), true);
            PlaceAtExit(new Vector3(0, 0, 1));
            Assert.That(levels.Current.Exit.HasExited, Is.True);
            levels.ResetLevel();
            PlaceAtExit(new Vector3(0, 0, -1), true);
            PlaceAtExit(Vector3.zero);
            levels.ResetLevel();
            PlaceAtExit(Vector3.zero, true);
            PlaceAtExit(new Vector3(0, 0, 1));
            Assert.That(levels.Current.Exit.HasExited, Is.False, "Reset cannot retain the old passage.");
        }

        [Test]
        public void CircularExit_RejectsSquareCornersButAcceptsRadialClearance()
        {
            Load(10);
            PlaceAtExit(new Vector3(0.45f, 0.45f, -0.7f), true);
            PlaceAtExit(new Vector3(0.45f, 0.45f, 0.7f));
            Assert.That(levels.Current.Exit.HasExited, Is.False, "Old square corners are solid wall now.");
            levels.ResetLevel();
            PlaceAtExit(new Vector3(0.35f, 0.35f, -0.7f), true);
            PlaceAtExit(new Vector3(0.35f, 0.35f, 0.7f));
            Assert.That(levels.Current.Exit.HasExited, Is.True);
        }

        [Test]
        public void FlushExit_SlowRollingBallLeavesFloorWithoutClimbingALip()
        {
            Load(0);
            var exit = levels.Current.Exit;
            // Start on the actual inner floor beside the opening, not inside it.
            float height = -exit.WallHalfDepth - levels.Ball.Profile.Radius - 0.003f;
            PlaceAtExit(new Vector3(1.15f, 0, height), true);
            levels.Ball.Body.linearVelocity = exit.transform.TransformDirection(Vector3.left * 1.2f);
            Steps(120);
            Assert.That(exit.HasExited, Is.True, "A gentle roll must fall through the cut without an upward impulse.");
            Assert.That(levels.Ball.Body.isKinematic, Is.False);
        }

        [Test]
        public void EveryLevel_HasPhysicalApertureBallCanPassWithoutTeleporting()
        {
            for (int i = 0; i < 16; i++)
            {
                Load(i);
                foreach (var plate in levels.Current.Plates) plate.SetActive(true);
                // Isolate the aperture here. Actual lid release is verified separately and in solve routes.
                foreach (var prop in levels.Current.Props) prop.Body.position = new Vector3(20, 0, 0);
                LaunchThroughExit();
                Assert.That(levels.Current.Exit.HasExited, Is.True, levels.Definition.Id);
                Assert.That(levels.Ball.Body.isKinematic, Is.False, levels.Definition.Id);
                Vector3 local = levels.Current.Exit.transform.InverseTransformPoint(levels.Ball.Body.position);
                Assert.That(local.z, Is.GreaterThan(levels.Current.Exit.WallHalfDepth + levels.Ball.Profile.Radius));
            }
        }

        [Test]
        public void PlateDoorPrerequisite_BlocksPhysicalExitUntilSwitchIsPressed()
        {
            Load(8);
            Assert.That(levels.Current.Exit.IsUnlocked, Is.False);
            LaunchThroughExit();
            Assert.That(levels.Current.Exit.HasExited, Is.False);
            Assert.That(levels.Current.Exit.transform.InverseTransformPoint(levels.Ball.Body.position).z, Is.LessThan(0));
            levels.ResetLevel();
            levels.Ball.Body.position = levels.Current.Plates[0].transform.position;
            UnityEngine.Physics.SyncTransforms(); Steps(1);
            Assert.That(levels.Current.Plates[0].IsActive, Is.True);
            Assert.That(levels.Current.Exit.IsUnlocked, Is.True);
            LaunchThroughExit();
            Assert.That(levels.Current.Exit.HasExited, Is.True);
        }

        [Test]
        public void LooseLid_RestsOnInnerSeatAndBlocksBallWithoutSignalLogic()
        {
            Load(5);
            Assert.That(levels.Current.Plates, Is.Empty);
            Assert.That(levels.Current.GetComponentsInChildren<SignalDoor>(), Is.Empty);
            Assert.That(levels.Current.Exit.RequiredChannel, Is.Empty);
            Assert.That(levels.Current.Exit.IsUnlocked, Is.True, "Only solid contact closes this hole.");
            var lid = levels.Current.Props[0];
            Assert.That(lid.Body.isKinematic, Is.False);
            Assert.That(lid.transform.parent, Is.Null);
            levels.Ball.Capture(levels.Ball.Body.position);
            Steps(180);
            Vector3 seated = levels.Current.Exit.transform.InverseTransformPoint(lid.Body.position);
            Assert.That(Mathf.Abs(seated.z), Is.LessThan(0.06f));
            Assert.That(new Vector2(seated.x, seated.y).magnitude, Is.LessThan(0.06f));
            levels.ResetLevel();
            LaunchThroughExit();
            Assert.That(levels.Current.Exit.HasExited, Is.False);
            Assert.That(levels.Current.Exit.transform.InverseTransformPoint(levels.Ball.Body.position).z, Is.LessThan(-levels.Current.Exit.WallHalfDepth));
        }

        [Test]
        public void LooseLid_FallsIntoBoxWhenOpeningFacesUpWithoutAnUnlockEvent()
        {
            Load(5);
            levels.Ball.Capture(levels.Ball.Body.position);
            PhysicalProp lid = levels.Current.Props[0];
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(0, 0, 180));
            Steps(180);
            Assert.That(Vector3.Dot(levels.Current.Exit.transform.forward, Vector3.up), Is.GreaterThan(0.98f));
            Assert.That(levels.Current.Exit.transform.InverseTransformPoint(lid.Body.position).z, Is.LessThan(-0.5f));
            Assert.That(lid.Body.isKinematic, Is.False);
            Assert.That(levels.Current.IsOutside(lid.Body.position), Is.False, "The loose lid must collide with the opposite wall.");
            Assert.That(lid.GetComponentsInChildren<Collider>()[0].enabled, Is.True, "Fallen lid remains physical.");
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active), "A fallen lid alone does not win the level.");
        }

        [Test]
        public void LooseLid_RemainsInsideDuringRepeatedLargeRotations()
        {
            Load(5);
            levels.Ball.Capture(levels.Ball.Body.position);
            PhysicalProp lid = levels.Current.Props[0];
            foreach (float angle in new[] { 180f, -25f, 90f, -90f, 135f, -135f, 0f })
            {
                levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(0, 0, angle));
                for (int tick = 0; tick < 210; tick++)
                {
                    Steps(1);
                    Assert.That(levels.Current.IsOutside(lid.Body.position), Is.False, "A solid lid must not tunnel through a wall.");
                }
            }
        }

        [Test]
        public void FreeProp_ReceivesEarthGravityOnceAndDoesNotInheritBoxRotation()
        {
            Load(5);
            levels.Ball.Capture(levels.Ball.Body.position);
            PhysicalProp lid = levels.Current.Props[0];
            lid.Body.position = new Vector3(0, 1.5f, 0);
            lid.Body.linearVelocity = Vector3.zero;
            lid.Body.angularVelocity = Vector3.zero;
            Quaternion initial = lid.Body.rotation;
            forces.Register(lid);
            forces.Register(lid);
            Assert.That(forces.TargetCount, Is.EqualTo(2));
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(0, 0, 50));
            UnityEngine.Physics.SyncTransforms(); Steps(6);
            Assert.That(lid.Body.linearVelocity.y, Is.InRange(-1.0f, -0.95f));
            Assert.That(Mathf.Abs(lid.Body.linearVelocity.x) + Mathf.Abs(lid.Body.linearVelocity.z), Is.LessThan(0.001f));
            Assert.That(Quaternion.Angle(initial, lid.Body.rotation), Is.LessThan(0.001f));
        }

        [Test]
        public void FreeProp_UsesZeroGProfileWithoutInventingAReleaseForce()
        {
            Load(5);
            levels.Ball.Capture(levels.Ball.Body.position);
            PhysicalProp lid = levels.Current.Props[0];
            forces.Configure(levels.Ball, levels.Catalog.Levels[10].Environment);
            forces.Register(lid);
            lid.Body.position = new Vector3(0, 1.5f, 0);
            Vector3 velocity = new Vector3(0.1f, 0.12f, 0.08f);
            lid.Body.linearVelocity = velocity;
            UnityEngine.Physics.SyncTransforms(); Steps(6);
            Assert.That(Vector3.Distance(lid.Body.linearVelocity, velocity), Is.LessThan(0.001f));
        }

        [Test]
        public void LooseLid_Reset100TimesRestoresBothBodiesAndUnloadRemovesForceTarget()
        {
            Load(5);
            PhysicalProp lid = levels.Current.Props[0];
            Vector3 start = lid.Body.position;
            Quaternion rotation = lid.Body.rotation;
            int registryCount = levels.Current.Resets.Count;
            for (int i = 0; i < 100; i++)
            {
                lid.Body.position = Vector3.one * 8;
                lid.Body.rotation = Quaternion.Euler(i, i * 2, 90);
                lid.Body.linearVelocity = Vector3.one * 5;
                lid.Body.angularVelocity = Vector3.one * 3;
                levels.Current.GetComponent<Rigidbody>().rotation = Quaternion.Euler(0, 0, i);
                levels.ResetLevel();
                Assert.That(Vector3.Distance(lid.Body.position, start), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(lid.Body.rotation, rotation), Is.LessThan(0.001f));
                Assert.That(lid.Body.linearVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(lid.Body.angularVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(forces.TargetCount, Is.EqualTo(2));
                Assert.That(levels.Current.Resets.Count, Is.EqualTo(registryCount));
            }
            GameObject oldLid = lid.gameObject;
            Load(0);
            Assert.That(forces.TargetCount, Is.EqualTo(1));
            Assert.That(oldLid.activeSelf, Is.False);
        }

        [Test]
        public void KillVolume_FailsAndResetReturnsToActive()
        {
            Load(3); int count = 0;
            levels.GameplayEvent += e => { if (e == "level_fail") count++; };
            levels.Ball.Body.position = levels.Current.Hazards[0].transform.position;
            UnityEngine.Physics.SyncTransforms(); Steps(2);
            Assert.That(count, Is.EqualTo(1));
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Failed));
            levels.ResetLevel();
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active));
            Assert.That(levels.Ball.IsCaptured, Is.False);
        }

        [Test]
        public void OneWayGate_PermitsForwardAndBlocksReturnAndResetsIgnorePair()
        {
            Load(6); OneWayGate gate = levels.Current.GetComponentInChildren<OneWayGate>();
            Collider sphere = levels.Ball.GetComponent<Collider>();
            levels.Ball.Body.position = gate.transform.position + Vector3.left;
            gate.Step(); Assert.That(UnityEngine.Physics.GetIgnoreCollision(sphere, gate.Blocker), Is.True);
            levels.Ball.Body.position = gate.transform.position + Vector3.right;
            gate.Step(); Assert.That(UnityEngine.Physics.GetIgnoreCollision(sphere, gate.Blocker), Is.False);
            gate.SetActive(true); levels.ResetLevel();
            Assert.That(UnityEngine.Physics.GetIgnoreCollision(sphere, gate.Blocker), Is.False);
        }

        [Test]
        public void Bumper_AppliesAnAuthoredImpulseAndResetsCooldown()
        {
            Load(7); ImpulsePad pad = levels.Current.Pads[0];
            levels.Ball.Body.position = new Vector3(0, 0, 0); levels.Ball.Body.linearVelocity = Vector3.zero;
            Assert.That(pad.TryFire(levels.Ball), Is.True);
            UnityEngine.Physics.Simulate(Dt);
            Assert.That(levels.Ball.Body.linearVelocity.magnitude, Is.GreaterThan(7));
            Assert.That(pad.TryFire(levels.Ball), Is.False);
            levels.ResetLevel();
            Assert.That(pad.TryFire(levels.Ball), Is.True);
        }

        [Test]
        public void EveryLevel_LoadsCorrectPrefabEnvironmentAndResets()
        {
            for (int i = 0; i < 16; i++)
            {
                Load(i);
                Assert.That(forces.Environment, Is.SameAs(levels.Definition.Environment));
                Assert.That(levels.Current.name, Does.Contain(levels.Definition.DisplayName));
                Assert.That(levels.Current.IsOutside(levels.Ball.Body.position), Is.False);
                Steps(10); levels.ResetLevel();
                Assert.That(Vector3.Distance(levels.Ball.Body.position, levels.Current.BallSpawn.position), Is.LessThan(0.0001f));
                Assert.That(levels.Current.Resets.Count, Is.GreaterThanOrEqualTo(3));
            }
        }

        [UnityTest]
        public IEnumerator ResetDuringCompletion_CancelsPendingAdvance()
        {
            levels.enabled = true;
            LaunchThroughExit();
            Assert.That(levels.Current.Exit.HasExited, Is.True);
            levels.ResetLevel();
            yield return new WaitForSecondsRealtime(levels.Catalog.CompletionDelay + 0.1f);
            Assert.That(levels.Index, Is.Zero);
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active));
        }

        [System.Serializable]
        public sealed class RouteStep { public float z; public bool aimAtExit; public int ticks; }
        [System.Serializable]
        public sealed class Route { public string id; public List<RouteStep> steps = new List<RouteStep>(); }
        [System.Serializable]
        public sealed class RouteSet { public string editor; public float fixedDelta; public List<Route> routes = new List<Route>(); }

        [Test, Category("Content")]
        public void All16Levels_HaveReproduciblePhysicsOnlySolveRoutes()
        {
            // No ball positioning or forces are injected here. Only normal root rotation
            // and the configured environmental/mechanism forces can reach the exit.
            string path = Path.Combine(Application.dataPath, "../Docs/VERIFIED_ROUTES.json");
            RouteSet previous = File.Exists(path) ? JsonUtility.FromJson<RouteSet>(File.ReadAllText(path)) : null;
            bool record = System.Environment.GetEnvironmentVariable("GRAVITYBOX_RECORD_SOLUTIONS") == "1";
            var proof = new RouteSet { editor = Application.unityVersion, fixedDelta = Dt };
            float[][] authored = {
                new[] {-25f}, new[] {-30f, 30f, -35f}, new[] {-30f, 35f, -35f}, new[] {-25f}, new[] {-45f},
                new[] {35f, -35f}, new[] {-35f}, new[] {0f, -30f, -60f}, new[] {40f, -40f}, new[] {-35f, 40f, -40f},
                new[] {0f}, new[] {0f}, new[] {0f}, new[] {15f, -35f, 45f}, new[] {0f, 90f, -90f}, new[] {0f, 45f, -90f}
            };
            float[] targets = {-140, -90, -55, -35, -20, 0, 20, 35, 55, 90, 140, 180};
            var failed = new List<string>();
            for (int index = 0; index < 16; index++)
            {
                Load(index);
                var gates = levels.Current.GetComponentsInChildren<OneWayGate>();
                Route solved = null;
                if (record && index == 5)
                {
                    // Let both bodies settle, lift the opening, and park the loose lid
                    // on another wall before returning toward the exit.
                    foreach (float parkAngle in new[] { 90f, -90f, 55f, -55f, 135f, -135f })
                    {
                        foreach (int settleTicks in new[] { 240, 360 })
                        {
                            var simpleLidRoute = new Route { id = levels.Definition.Id };
                            simpleLidRoute.steps.Add(new RouteStep { z = 0, ticks = 120 });
                            simpleLidRoute.steps.Add(new RouteStep { z = 180, ticks = 240 });
                            simpleLidRoute.steps.Add(new RouteStep { z = parkAngle, ticks = settleTicks });
                            simpleLidRoute.steps.Add(new RouteStep { aimAtExit = true, ticks = 1200 });
                            bool repeatable = true;
                            for (int repeat = 0; repeat < 3; repeat++)
                            {
                                Load(index);
                                if (!TryRoute(simpleLidRoute, gates, out _)) { repeatable = false; break; }
                            }
                            if (repeatable) { solved = simpleLidRoute; break; }
                        }
                        if (solved != null) break;
                    }
                }
                if (solved == null && previous != null)
                {
                    Route recorded = previous.routes.Find(x => x.id == levels.Definition.Id);
                    if (recorded != null && TryRoute(recorded, gates, out Route replayed)) solved = replayed;
                }
                if (solved == null && !record)
                {
                    failed.Add(levels.Definition.Id);
                    continue;
                }
                if (solved == null)
                {
                    var route = new Route { id = levels.Definition.Id };
                    foreach (float z in authored[index]) route.steps.Add(new RouteStep { z = z, ticks = authored[index].Length == 1 ? 1200 : 420 });
                    if (TryRoute(route, gates, out Route candidate)) solved = candidate;
                }
                // Bounded deterministic search is only an initial authoring aid. Saved routes
                // run first, so normal regression tests replay a short proof instead of searching.
                for (int trial = 0; trial < 120 && solved == null; trial++)
                {
                    var random = new System.Random(1371 + index * 1000 + trial);
                    var route = new Route { id = levels.Definition.Id };
                    for (int j = 0; j < 8; j++) route.steps.Add(new RouteStep { z = targets[random.Next(targets.Length)], ticks = random.Next(90, 270) });
                    if (TryRoute(route, gates, out Route candidate)) solved = candidate;
                }
                if (solved != null)
                {
                    // The final hold is a bounded input budget, not a promise of bitwise
                    // identical contact timing across PhysX runs. Intermediate holds stay exact.
                    if (record) solved.steps[solved.steps.Count - 1].ticks += 60;
                    proof.routes.Add(solved);
                    Debug.Log($"SOLVE PROOF {levels.Definition.Id}: {solved.steps.Count} rotation segments");
                }
                else { failed.Add(levels.Definition.Id); Debug.LogWarning("NO SOLVE PROOF: " + levels.Definition.Id); }
                if (record) File.WriteAllText(path, JsonUtility.ToJson(proof, true));
            }
            Assert.That(failed, Is.Empty, "No verified route for: " + string.Join(", ", failed));
        }

        private bool TryRoute(Route route, OneWayGate[] gates, out Route proof)
        {
            proof = new Route { id = route.id };
            levels.ResetLevel();
            foreach (RouteStep segment in route.steps)
            {
                levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(0, 0, segment.z));
                var actual = new RouteStep { z = segment.z, aimAtExit = segment.aimAtExit, ticks = 0 };
                proof.steps.Add(actual);
                Vector3 previousBall = levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position);
                for (int tick = 0; tick < segment.ticks; tick++)
                {
                    if (segment.aimAtExit)
                    {
                        // Test-only player policy: observe the ball and tilt on BOTH axes.
                        // A free lid can deflect the ball in depth, which a Z-only route cannot correct.
                        // This changes rotation intent only; it never moves either free body or adds force.
                        Transform box = levels.Current.transform;
                        Vector3 ball = box.InverseTransformPoint(levels.Ball.Body.position);
                        Vector3 velocity = (ball - previousBall) / Dt;
                        previousBall = ball;
                        Vector3 outward = box.InverseTransformDirection(levels.Current.Exit.transform.forward);
                        Vector3 error = box.InverseTransformPoint(levels.Current.Exit.transform.position) - ball;
                        Vector3 tangent = Vector3.ProjectOnPlane(error * 2f - velocity * 2f, outward);
                        Vector3 gravityDirection = outward * 9.81f + Vector3.ClampMagnitude(tangent, 3f);
                        levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(gravityDirection, Vector3.down));
                    }
                    levels.Current.Rotation.Step(Dt);
                    foreach (OneWayGate gate in gates) gate.Step();
                    forces.Step();
                    UnityEngine.Physics.Simulate(Dt);
                    levels.Current.Exit.EvaluateTraversal();
                    actual.ticks++;
                    if (levels.Session.State == SessionState.Completing) return true;
                    if (levels.Session.State == SessionState.Failed || levels.Current.IsOutside(levels.Ball.Body.position)) return false;
                }
            }
            if (route.id == "gb-06")
                Debug.Log($"LID ROUTE HELD: ball {levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position)}, lid {levels.Current.transform.InverseTransformPoint(levels.Current.Props[0].Body.position)}");
            return false;
        }
    }
}

#endif
