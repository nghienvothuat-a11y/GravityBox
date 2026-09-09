#if UNITY_EDITOR
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using SessionState = GravityBox.Foundation.SessionState;

namespace GravityBox.Tests
{
    public sealed class CampaignLifecycleTests
    {
        private sealed class MemoryStorage : ICampaignProgressStorage
        {
            private readonly Dictionary<string,string> values = new Dictionary<string,string>();
            public string Read(string key) => values.TryGetValue(key,out string value) ? value : "";
            public void Write(string key,string value) => values[key] = value;
        }
        private GameObject services;
        private LevelManager levels;
        private EnvironmentForceSystem forces;
        private SimulationMode oldMode;
        private Vector3 oldGravity;
        private float oldFixed, oldMaximum, oldContact, oldBounce;
        private int oldSolver, oldVelocity;
        private const float Dt = 1f / 120f;
        public static IEnumerable<int> EveryLevel => Enumerable.Range(1,100);

        [SetUp]
        public void Setup()
        {
            oldMode = Physics.simulationMode; oldGravity = Physics.gravity;
            oldFixed = Time.fixedDeltaTime; oldMaximum = Time.maximumDeltaTime;
            oldContact = Physics.defaultContactOffset; oldBounce = Physics.bounceThreshold;
            oldSolver = Physics.defaultSolverIterations; oldVelocity = Physics.defaultSolverVelocityIterations;
            PhysicsTiming.Apply(); Physics.simulationMode = SimulationMode.Script; Physics.gravity = Vector3.zero;
            services = new GameObject("Campaign lifecycle services");
            forces = services.AddComponent<EnvironmentForceSystem>(); forces.enabled = false;
            levels = services.AddComponent<LevelManager>(); levels.enabled = false;
            LevelCatalog campaign = AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/_Game/Campaign/CampaignCatalog.asset");
            Assert.That(campaign, Is.Not.Null, "Generate the campaign content before running these tests.");
            levels.Initialize(campaign, forces, AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/_Game/ScriptableObjects/LevelCatalog.asset"), false, new MemoryStorage());
            levels.Current.Rotation.enabled = false;
        }
        [TearDown]
        public void Teardown()
        {
            if (levels != null)
            {
                var root = levels.Current; var balls = levels.Balls.ToArray();
                var props = root != null ? root.Props : System.Array.Empty<PhysicalProp>();
                Object.DestroyImmediate(services);
                foreach (var prop in props) if (prop != null) Object.DestroyImmediate(prop.gameObject);
                if (root != null) Object.DestroyImmediate(root.gameObject);
                foreach (var ball in balls) if (ball != null) Object.DestroyImmediate(ball.gameObject);
            }
            Physics.simulationMode = oldMode; Physics.gravity = oldGravity;
            Time.fixedDeltaTime = oldFixed; Time.maximumDeltaTime = oldMaximum;
            Physics.defaultContactOffset = oldContact; Physics.bounceThreshold = oldBounce;
            Physics.defaultSolverIterations = oldSolver; Physics.defaultSolverVelocityIterations = oldVelocity;
            Time.timeScale = 1;
        }
        private void Load(int number) { levels.Load(number - 1); levels.Current.Rotation.enabled = false; }
        private void Steps(int count)
        {
            for (int i = 0; i < count; i++)
            {
                levels.Current.Rotation.Step(Dt); forces.Step(); Physics.Simulate(Dt); levels.Current.Exit.EvaluateTraversal();
            }
        }

        [TestCaseSource(nameof(EveryLevel))]
        public void AuthoredLevelHasStableSpawnAndReversiblePhysicsLifecycle(int number)
        {
            Load(number);
            Vector3[] starts = levels.Balls.Select(b => b.Body.position).ToArray();
            Quaternion orientation = levels.Current.Rotation.Orientation;
            var solids = levels.Current.GetComponentsInChildren<Collider>().Concat(levels.Current.Props.SelectMany(p => p.GetComponentsInChildren<Collider>())).ToArray();
            foreach (var ball in levels.Balls)
                foreach (Collider collider in solids)
                    if (collider.enabled && !collider.isTrigger && Physics.ComputePenetration(ball.GetComponent<SphereCollider>(), ball.Body.position, ball.Body.rotation,
                            collider, collider.transform.position, collider.transform.rotation, out _, out float depth))
                        Assert.That(depth, Is.LessThan(.0011f), $"C{number:000} spawn overlaps {collider.name}");
            Steps(90);
            foreach (var ball in levels.Balls)
            {
                Assert.That(float.IsNaN(ball.Body.position.sqrMagnitude), Is.False);
                if (!levels.Current.Exit.HasBallExited(ball)) Assert.That(levels.Current.IsOutside(ball.Body.position), Is.False, $"C{number:000} drops outside at rest");
            }
            int resettableCount = levels.Current.Resets.Count;
            for (int trial = 0; trial < 3; trial++)
            {
                levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(15 + trial * 11, 24, -18)); Steps(45);
                levels.ResetLevel();
                for (int i = 0; i < levels.Balls.Count; i++)
                {
                    Assert.That(Vector3.Distance(levels.Balls[i].Body.position, starts[i]), Is.LessThan(.00001f));
                    Assert.That(levels.Balls[i].Body.linearVelocity, Is.EqualTo(Vector3.zero));
                    Assert.That(levels.Balls[i].Body.angularVelocity, Is.EqualTo(Vector3.zero));
                }
                Assert.That(Quaternion.Angle(levels.Current.Rotation.Orientation, orientation), Is.LessThan(.001f));
                Assert.That(levels.Current.Exit.HasExited, Is.False);
                Assert.That(levels.Current.Resets.Count, Is.EqualTo(resettableCount));
                Assert.That(forces.TargetCount, Is.EqualTo(levels.Balls.Count + levels.Current.Props.Length));
                Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active));
            }
        }

        [TestCaseSource(nameof(EveryLevel))]
        public void FinalApertureAdmitsEachWholeBallAndNeverCompletesEarly(int number)
        {
            Load(number); ExitSocket exit = levels.Current.Exit;
            // Isolates the final aperture contract; does not claim a route from spawn.
            for (int i = 0; i < levels.Balls.Count; i++)
            {
                BallController ball = levels.Balls[i];
                ball.Body.position = exit.transform.TransformPoint(new Vector3(0, 0, -.025f));
                ball.Body.linearVelocity = Vector3.zero; ball.Body.angularVelocity = Vector3.zero;
                Physics.SyncTransforms(); exit.BeginTracking();
                for (int tick = 0; tick < 480 && !exit.HasBallExited(ball); tick++) Steps(1);
                Assert.That(exit.HasBallExited(ball), Is.True, $"C{number:000} ball {i + 1} cannot leave the actual aperture");
                Assert.That(ball.Body.isKinematic, Is.False);
                Assert.That(levels.Session.State, Is.EqualTo(i == levels.Balls.Count - 1 ? SessionState.Completing : SessionState.Active));
            }
        }

        [Test]
        public void BossReplayUsesVisualCopiesAndResetRestoresSimulation()
        {
            Load(10);
            var replay = services.AddComponent<BossReplay>(); replay.Initialize(levels, null);
            for (int i = 0; i < 12; i++) { Steps(1); replay.SendMessage("FixedUpdate"); }
            // An exit fixture isolates replay lifecycle; this is not a puzzle route test.
            var ball = levels.Ball; var exit = levels.Current.Exit;
            ball.Body.position = exit.transform.TransformPoint(new Vector3(0, 0, -.025f));
            ball.Body.linearVelocity = Vector3.zero; Physics.SyncTransforms(); exit.BeginTracking();
            for (int i = 0; i < 480 && !exit.HasExited; i++) Steps(1);
            Assert.That(replay.Available, Is.True);
            var bodies = Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
            var positions = bodies.Select(b => b.position).ToArray();
            var renderers = levels.Current.GetComponentsInChildren<Renderer>();
            var priorVisibility = renderers.Select(r => r.forceRenderingOff).ToArray();
            replay.Play();
            Assert.That(replay.IsPlaying, Is.True); Assert.That(Time.timeScale, Is.Zero);
            Assert.That(Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Length, Is.EqualTo(bodies.Length));
            var ghost = GameObject.Find("Observed boss replay — visuals only");
            Assert.That(ghost, Is.Not.Null);
            Assert.That(ghost.GetComponentsInChildren<Collider>().Length, Is.Zero);
            Assert.That(ghost.GetComponentsInChildren<MeshRenderer>().Length, Is.GreaterThan(0));
            for (int i = 0; i < bodies.Length; i++) Assert.That(bodies[i].position, Is.EqualTo(positions[i]));
            levels.ResetLevel();
            Assert.That(replay.IsPlaying, Is.False); Assert.That(replay.Available, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1)); Assert.That(ghost.activeSelf, Is.False);
            for (int i = 0; i < renderers.Length; i++) Assert.That(renderers[i].forceRenderingOff, Is.EqualTo(priorVisibility[i]));
        }

        [UnityTest]
        public IEnumerator CampaignHudLoadsChapterBoundariesAndLabWithoutRuntimeErrors()
        {
            var hud = services.AddComponent<GameHud>(); hud.Initialize(levels);
            foreach (int number in new[] { 1, 10, 40, 50, 70, 80, 100 })
            {
                Load(number);
                yield return null;
                Assert.That(hud.GetComponentsInChildren<Text>().Any(t => t.text == levels.Definition.DisplayName), Is.True);
                var floor = levels.Current.transform.Find("Floor with circular cut");
                if (floor != null && levels.Current.GetComponent<WaterVolume>() == null)
                    Assert.That(levels.Current.GetComponent<ContainerInspectionView>(), Is.Not.Null);
            }
            levels.SwitchCatalog(); yield return null;
            Assert.That(levels.Catalog.IsCampaign, Is.False);
            Assert.That(hud.GetComponentsInChildren<Text>().Any(t => t.text == levels.Definition.DisplayName), Is.True);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void SwitchingCampaignAndLabCleansPropsAndRestoresStableIdProgress()
        {
            Load(100); var root = levels.Current; var props = root.Props.ToArray();
            levels.SwitchCatalog();
            Assert.That(levels.Catalog.IsCampaign, Is.False); Assert.That(levels.Catalog.Levels.Length, Is.EqualTo(23));
            Assert.That(root.gameObject.activeSelf, Is.False);
            foreach (var prop in props) Assert.That(prop.gameObject.activeSelf, Is.False);
            Assert.That(forces.TargetCount, Is.EqualTo(1));
            levels.SwitchCatalog(); Assert.That(levels.Definition.Id, Is.EqualTo("campaign-100"));
            levels.Next(); Assert.That(levels.Index, Is.EqualTo(99), "The final campaign level must not wrap back to onboarding.");
        }
    }
}
#endif
