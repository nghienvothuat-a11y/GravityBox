using System.Collections.Generic;
using GravityBox.Foundation;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using SessionState = GravityBox.Foundation.SessionState;

namespace GravityBox.Tests
{
    public sealed class DesignContractTests
    {
        private sealed class ResetProbe : IResettable
        {
            public int Captures, Resets;
            public void CaptureInitialState() => Captures++;
            public void ResetState() => Resets++;
        }

        [Test]
        public void Registry_DeduplicatesAndCapturesOnlyOnce()
        {
            var registry = new ResetRegistry(); var item = new ResetProbe();
            registry.Register(item); registry.Register(item);
            for (int i = 0; i < 100; i++) registry.RestoreAll();
            Assert.That(registry.Count, Is.EqualTo(1));
            Assert.That(item.Captures, Is.EqualTo(1));
            Assert.That(item.Resets, Is.EqualTo(100));
            registry.Clear(); registry.RestoreAll();
            Assert.That(item.Resets, Is.EqualTo(100));
        }

        [Test]
        public void TerminalState_FiresExactlyOnceUntilReset()
        {
            var session = new GameSession(); int completions = 0;
            session.Changed += state => { if (state == SessionState.Completing) completions++; };
            Assert.That(session.TryComplete(), Is.False);
            session.Activate();
            Assert.That(session.TryComplete(), Is.True);
            Assert.That(session.TryComplete(), Is.False);
            Assert.That(session.TryFail(), Is.False);
            Assert.That(completions, Is.EqualTo(1));
            session.Activate();
            Assert.That(session.TryComplete(), Is.True);
            Assert.That(completions, Is.EqualTo(2));
        }

        [Test]
        public void PausedSession_RejectsCompletionAndFailure()
        {
            var session = new GameSession(); session.Activate(); session.TogglePause();
            Assert.That(session.State, Is.EqualTo(SessionState.Paused));
            Assert.That(session.TryComplete(), Is.False);
            Assert.That(session.TryFail(), Is.False);
            session.TogglePause(); Assert.That(session.State, Is.EqualTo(SessionState.Active));
        }

        [Test]
        public void Profiles_ResolveWorldAcceleration()
        {
            var catalog = Catalog();
            foreach (LevelDefinition level in catalog.Levels)
            {
                Assert.That(level.Environment.Acceleration, Is.EqualTo(Vector3.down * 9.81f));
                Assert.That(level.InitialLocalVelocity, Is.EqualTo(Vector3.zero));
            }
        }

        [Test]
        public void Snapping_UsesAll24DistinctCubeOrientations()
        {
            var orientations = new List<Quaternion>();
            Vector3[] axes = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            foreach (Vector3 forward in axes)
                foreach (Vector3 up in axes)
                {
                    if (Mathf.Abs(Vector3.Dot(forward, up)) > 0.1f) continue;
                    Quaternion expected = Quaternion.LookRotation(forward, up);
                    Quaternion near = Quaternion.AngleAxis(4, Vector3.one.normalized) * expected;
                    Quaternion snapped = BoxRotationController.NearestCanonical(near);
                    Assert.That(Quaternion.Angle(expected, snapped), Is.LessThan(0.05f));
                    foreach (Quaternion other in orientations) Assert.That(Quaternion.Angle(other, snapped), Is.GreaterThan(1));
                    orientations.Add(snapped);
                }
            Assert.That(orientations.Count, Is.EqualTo(24));
        }

        [Test]
        public void SignalBus_IsScopedToEachLevel()
        {
            var a = new MechanismSignals(); var b = new MechanismSignals(); int count = 0;
            a.Changed += (_, __) => count++;
            a.Set("gate-a", true); a.Set("gate-a", true);
            Assert.That(b.Read("gate-a"), Is.False);
            Assert.That(count, Is.EqualTo(1));
            a.Clear(); Assert.That(a.Read("gate-a"), Is.False);
        }

        [Test]
        public void Catalog_ContainsTwentyThreeDistinctExperimentsWithSharedSteelPhysics()
        {
            var catalog = Catalog(); var ids = new HashSet<string>();
            var shapes = new HashSet<ContainerShape>();
            Assert.That(catalog.Levels.Length, Is.EqualTo(23));
            Assert.That(catalog.BallPrefab, Is.Not.Null);
            Assert.That(catalog.Rotation, Is.Not.Null);
            for (int i = 0; i < catalog.Levels.Length; i++)
            {
                LevelDefinition level = catalog.Levels[i];
                Assert.That(ids.Add(level.Id), Is.True, "Duplicate level id");
                Assert.That(shapes.Add(level.Shape), Is.True, "Each experiment needs a distinct container shape.");
                Assert.That(level.RotationMode, Is.EqualTo(RotationMode.Free));
                Assert.That(level.DisplayIndex, Is.EqualTo(i + 1));
                Assert.That(level.Prefab, Is.Not.Null);
                Assert.That(level.Prefab.Exit, Is.Not.Null);
                Assert.That(level.Prefab.BallSpawn, Is.Not.Null);
                Assert.That(level.Prefab.Footprint, Has.Length.GreaterThanOrEqualTo(3));
                Assert.That(level.Prefab.FootprintVoids, Is.Not.Null);
                Assert.That(level.Prefab.FootprintVoids.Length, Is.EqualTo(level.Shape == ContainerShape.Annulus ? 1 : 0));
                Assert.That(level.Environment.IsZeroGravity, Is.False);
                Assert.That(level.Prefab.GetComponentsInChildren<PressurePlate>(true), Is.Empty);
                Assert.That(level.Prefab.GetComponentsInChildren<SignalDoor>(true), Is.Empty);
                Assert.That(level.Prefab.GetComponentsInChildren<OneWayGate>(true), Is.Empty);
                Assert.That(level.Prefab.GetComponentsInChildren<ImpulsePad>(true), Is.Empty);
                Assert.That(level.Prefab.GetComponentsInChildren<KillVolume>(true), Is.Empty);
                int propCount=level.Prefab.GetComponentsInChildren<PhysicalProp>(true).Length;
                if(level.Shape < ContainerShape.GravityBridge)
                    Assert.That(propCount, Is.EqualTo(level.Shape == ContainerShape.GravityLock ? 1 : level.Shape == ContainerShape.MechanicalMaze ? 2 : level.Shape == ContainerShape.CooperativeBox ? 4 : 0));
                else Assert.That(propCount,Is.EqualTo(level.Shape==ContainerShape.FlightCatch ? 0 : level.Shape==ContainerShape.MechanicalMemory || level.Shape==ContainerShape.MechanicalHeart ? 3 : 1));
                if (level.Shape == ContainerShape.SphereMaze)
                {
                    SpatialMaze sphere = level.Prefab.GetComponent<SpatialMaze>();
                    Assert.That(sphere, Is.Not.Null);
                    Assert.That(sphere.ShellCollider, Is.Not.Null);
                    Assert.That(level.Prefab.InteriorDepth, Is.EqualTo(2 * (sphere.InnerRadius + sphere.ShellThickness)).Within(0.0001f));
                    Assert.That(sphere.MainPath.Length, Is.GreaterThanOrEqualTo(26));
                    Assert.That(sphere.Edges.Length, Is.EqualTo(sphere.NodesLocal.Length - 1));
                    Assert.That(sphere.ClearWidth, Is.GreaterThan(catalog.BallProfile.Radius * 2));
                    Assert.That(sphere.SightGap, Is.LessThan(catalog.BallProfile.Radius * 2));
                }
                else if(level.Shape < ContainerShape.GravityBridge)
                    Assert.That(level.Prefab.InteriorDepth, Is.EqualTo(level.Shape == ContainerShape.LayeredMaze ? 0.27f : 0.09f).Within(0.0001f));
                else Assert.That(level.Prefab.InteriorDepth,Is.GreaterThan(catalog.BallProfile.Radius*2+.006f));
                Assert.That(level.Prefab.Exit.RequiredChannel, Is.Null.Or.Empty);
                Assert.That(level.TeachingHint, Is.Not.Empty);
                Assert.That(level.DesignerSolution, Is.Not.Empty);
            }
            CollectionAssert.AreEquivalent(new[] { ContainerShape.Circle, ContainerShape.Square, ContainerShape.Triangle,
                ContainerShape.LShape, ContainerShape.UShape, ContainerShape.Annulus, ContainerShape.Dumbbell, ContainerShape.Star,
                ContainerShape.GravityLock, ContainerShape.MechanicalMaze, ContainerShape.LayeredMaze, ContainerShape.SphereMaze, ContainerShape.WaterBox, ContainerShape.MercuryBox, ContainerShape.LionHead, ContainerShape.CooperativeBox,
                ContainerShape.GravityBridge,ContainerShape.BalanceMachine,ContainerShape.NestedCage,ContainerShape.PendulumGate,ContainerShape.FlightCatch,ContainerShape.MechanicalMemory,ContainerShape.MechanicalHeart }, shapes);
        }

        [Test]
        public void SharedBall_HasTabletopSteelSphereDimensionsAndDensity()
        {
            BallPhysicsProfile profile = Catalog().BallProfile;
            Assert.That(profile.Radius, Is.EqualTo(0.015f).Within(0.00001f));
            float volume = 4f / 3f * Mathf.PI * Mathf.Pow(profile.Radius, 3);
            Assert.That(profile.Mass / volume, Is.InRange(7600f, 8100f), "Mass must match a solid steel sphere at the chosen scale.");
            Assert.That(profile.ContactMaterial, Is.Not.Null);
        }

        private static LevelCatalog Catalog() => AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/_Game/ScriptableObjects/LevelCatalog.asset");
    }
}
