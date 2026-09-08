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
        private const float Dt = 1f / 60;

        [SetUp]
        public void Setup()
        {
            previousMode = UnityEngine.Physics.simulationMode;
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
            Load(5);
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

        [Test]
        public void ExitTrigger_CapturesExactlyOnceAndLocksInput()
        {
            int count = 0;
            levels.GameplayEvent += e => { if (e == "level_complete") count++; };
            levels.Ball.Body.position = levels.Current.Exit.transform.position;
            UnityEngine.Physics.SyncTransforms(); Steps(3);
            Assert.That(count, Is.EqualTo(1));
            Assert.That(levels.Ball.IsCaptured, Is.True);
            Assert.That(levels.Ball.Body.isKinematic, Is.True);
            Assert.That(levels.Current.Rotation.InputEnabled, Is.False);
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Completing));
            levels.ResetLevel();
            Assert.That(levels.Ball.IsCaptured, Is.False);
            Assert.That(levels.Current.Exit.HasCaptured, Is.False);
        }

        [Test]
        public void PlateDoorPrerequisite_PreventsBypassingSequence()
        {
            Load(5);
            Assert.That(levels.Current.Exit.TryCapture(levels.Ball), Is.False);
            levels.Ball.Body.position = levels.Current.Plates[0].transform.position;
            UnityEngine.Physics.SyncTransforms(); Steps(1);
            Assert.That(levels.Current.Plates[0].IsActive, Is.True);
            Assert.That(levels.Current.GetComponentInChildren<SignalDoor>().Blocker.enabled, Is.False);
            Assert.That(levels.Current.Exit.TryCapture(levels.Ball), Is.True);
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
            levels.Current.Exit.TryCapture(levels.Ball);
            levels.ResetLevel();
            yield return new WaitForSeconds(1f);
            Assert.That(levels.Index, Is.Zero);
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active));
        }

        [System.Serializable]
        public sealed class RouteStep { public float z; public int ticks; }
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
                if (previous != null)
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
                var actual = new RouteStep { z = segment.z, ticks = 0 };
                proof.steps.Add(actual);
                for (int tick = 0; tick < segment.ticks; tick++)
                {
                    levels.Current.Rotation.Step(Dt);
                    foreach (OneWayGate gate in gates) gate.Step();
                    forces.Step();
                    UnityEngine.Physics.Simulate(Dt);
                    actual.ticks++;
                    if (levels.Session.State == SessionState.Completing) return true;
                    if (levels.Session.State == SessionState.Failed || levels.Current.IsOutside(levels.Ball.Body.position)) return false;
                }
            }
            return false;
        }
    }
}

#endif
