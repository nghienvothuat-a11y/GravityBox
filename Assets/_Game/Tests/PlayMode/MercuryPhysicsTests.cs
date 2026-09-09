#if UNITY_EDITOR
using GravityBox.Presentation;
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed partial class PhysicsLifecycleTests
    {
        [Test]
        public void Mercury_SameSteelAndSquareRisesWithMeasuredMaterialProperties()
        {
            Load(13); WaterVolume liquid = levels.Current.GetComponent<WaterVolume>();
            WaterVolume reference = levels.Catalog.Levels[12].Prefab.GetComponent<WaterVolume>();
            Assert.That(liquid.Profile, Is.Not.SameAs(reference.Profile));
            Assert.That(reference.Profile.Density, Is.EqualTo(998.2f));
            Assert.That(liquid.Profile.Density, Is.EqualTo(13546));
            Assert.That(liquid.Profile.DynamicViscosity, Is.EqualTo(.001567367f).Within(1e-9f));
            CollectionAssert.AreEqual(levels.Catalog.Levels[1].Prefab.Footprint, levels.Current.Footprint);
            Assert.That(levels.Current.GetComponentsInChildren<Collider>().Length,
                Is.EqualTo(levels.Catalog.Levels[1].Prefab.GetComponentsInChildren<Collider>().Length));
            Assert.That(levels.Ball.Profile.Mass, Is.EqualTo(.11097676f).Within(1e-7f));
            levels.Ball.Body.position = new Vector3(-.1f, 0, 0);
            levels.Ball.Body.linearVelocity = Vector3.zero; UnityEngine.Physics.SyncTransforms(); Steps(1);
            Assert.That(liquid.BuoyancyForce.y, Is.EqualTo(1.8786352f).Within(.00001f));
            Assert.That(liquid.AddedMass, Is.EqualTo(.09575103f).Within(.000001f));
            Assert.That(levels.Ball.Body.linearVelocity.y / Dt, Is.EqualTo(3.821224f).Within(.003f));
            Assert.That(levels.Ball.Body.inertiaTensor.x, Is.EqualTo(levels.Ball.Profile.SolidSphereInertia).Within(1e-9f));
            TestContext.WriteLine($"Mercury: buoyancy {liquid.BuoyancyForce.y:F6} N; added mass {liquid.AddedMass:F8} kg; initial rise {levels.Ball.Body.linearVelocity.y / Dt:F6} m/s².");
        }

        [Test]
        public void Mercury_RisesToCeilingAndRestsUnderUpwardLoadWithoutPulsing()
        {
            Load(13); Steps(360);
            Assert.That(levels.Ball.Body.position.y, Is.EqualTo(.027f).Within(.0006f));
            float min = float.PositiveInfinity, max = 0, speed = 0;
            int impacts = 0; levels.Ball.Impact += _ => impacts++;
            for (int tick = 0; tick < 600; tick++)
            {
                Steps(1); min = Mathf.Min(min, levels.Ball.ContactLoad); max = Mathf.Max(max, levels.Ball.ContactLoad);
                speed = Mathf.Max(speed, levels.Ball.Body.linearVelocity.magnitude);
            }
            Assert.That(min, Is.GreaterThan(.7899532f * .95f)); Assert.That(max, Is.LessThan(.7899532f * 1.05f));
            Assert.That(speed, Is.LessThan(.002f)); Assert.That(impacts, Is.Zero);
            TestContext.WriteLine($"Mercury ceiling: y={levels.Ball.Body.position.y:F6} m; load {min:F6}–{max:F6} N; peak speed {speed:F6} m/s.");
        }

        [Test]
        public void Mercury_FreeCoastMatchesReferenceWithHighFluidToSteelMassRatio()
        {
            Load(13); WaterVolume liquid = levels.Current.GetComponent<WaterVolume>();
            EnvironmentProfile zeroG = Object.Instantiate(levels.Definition.Environment);
            try
            {
                zeroG.GravityScale = 0;
                foreach (Collider surface in levels.Current.GetComponentsInChildren<Collider>()) surface.enabled = false;
                liquid.HalfSize = Vector3.one * 10;
                forces.Configure(levels.Ball, zeroG); liquid.Bind(levels.Ball, zeroG); forces.AddProvider(liquid);
                levels.Ball.Body.position = Vector3.zero; levels.Ball.Body.linearVelocity = Vector3.right * .2f;
                float previous = .2f;
                for (int tick = 0; tick < 120; tick++)
                {
                    Steps(1);
                    Assert.That(Vector3.Dot(liquid.DragForce, liquid.RelativeVelocity), Is.LessThanOrEqualTo(0));
                    Assert.That(levels.Ball.Body.linearVelocity.x, Is.InRange(0, previous));
                    previous = levels.Ball.Body.linearVelocity.x;
                }
                Assert.That(previous, Is.EqualTo(.06583349f).Within(.0001f));
                TestContext.WriteLine($"Mercury free coasting: 0.200 → {previous:F6} m/s in 1s; reference 0.065833 m/s.");
            }
            finally { Object.DestroyImmediate(zeroG); }
        }

        [Test]
        public void Mercury_PartlyFloatingInOpeningDoesNotWinButFullPhysicalExitDoes()
        {
            Load(13); WaterVolume liquid = levels.Current.GetComponent<WaterVolume>();
            Rigidbody root = levels.Current.GetComponent<Rigidbody>();
            root.rotation = Quaternion.Euler(0, 0, 180);
            liquid.ResetState();
            // Hydrostatic equilibrium at the upward-facing mouth. Fixture placement
            // isolates the partial-immersion win contract, not a gameplay shortcut.
            float low = -.057f, high = -.027f;
            for (int i = 0; i < 32; i++)
            {
                float middle = (low + high) * .5f;
                float fraction = liquid.Immersion(root.rotation * new Vector3(.105f, middle, -.105f), Radius);
                if (fraction < 7850f / 13546) low = middle; else high = middle;
            }
            levels.Ball.Body.position = root.rotation * new Vector3(.105f, (low + high) * .5f, -.105f);
            levels.Ball.Body.linearVelocity = Vector3.zero; UnityEngine.Physics.SyncTransforms();
            levels.Current.Exit.BeginTracking();
            for (int tick = 0; tick < 120; tick++)
            { forces.Step(); UnityEngine.Physics.Simulate(Dt); levels.Current.Exit.EvaluateTraversal(); }
            Assert.That(liquid.SubmergedFraction, Is.EqualTo(7850f / 13546).Within(.001f));
            Assert.That(levels.Current.Exit.HasExited, Is.False);
            Assert.That(levels.Ball.Body.linearVelocity.magnitude, Is.LessThan(.001f));
            // Separately launch from inside the bore, exercising swept full-sphere
            // clearance and return to normal steel inertia outside the retained liquid.
            BeginExitApproach(); levels.Ball.Body.linearVelocity = Vector3.up * 1.5f;
            for (int tick = 0; tick < 24; tick++)
            { forces.Step(); UnityEngine.Physics.Simulate(Dt); levels.Current.Exit.EvaluateTraversal(); }
            Assert.That(levels.Current.Exit.HasExited, Is.True); Assert.That(levels.Ball.Body.isKinematic, Is.False);
            Assert.That(liquid.AddedMass, Is.Zero);
            Assert.That(levels.Ball.Body.mass, Is.EqualTo(levels.Ball.Profile.Mass).Within(1e-7f));
        }

        [Test]
        public void Mercury_SilverViewResetAndSwitchBackToWaterDoNotChangeSharedAssets()
        {
            Load(13); WaterVolume liquid = levels.Current.GetComponent<WaterVolume>();
            WaterVisuals visual = liquid.GetComponent<WaterVisuals>(); visual.Initialize(liquid);
            Assert.That(visual.MercuryCutaway, Is.True);
            Assert.That(visual.VolumeRenderer.sharedMaterial.shader.name, Is.EqualTo("GravityBox/Mercury Cutaway"));
            Assert.That(visual.FloorRenderer.sharedMaterial.shader.name, Is.Not.EqualTo("GravityBox/Underwater Caustics"));
            Assert.That(visual.VolumeRenderer.GetComponent<Collider>(), Is.Null);
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(40, 20, 30));
            for (int tick = 0; tick < 120; tick++) { Steps(1); visual.Advance(Dt); }
            Assert.That(float.IsNaN(levels.Ball.Body.linearVelocity.magnitude), Is.False);
            levels.ResetLevel();
            Assert.That(liquid.AddedMass, Is.Zero); Assert.That(liquid.FluidAngularVelocity, Is.EqualTo(Vector3.zero));
            Assert.That(visual.VisualClock, Is.Zero); Assert.That(visual.LiveWakeCount, Is.Zero);
            Load(12);
            Assert.That(levels.Current.GetComponent<WaterVisuals>().MercuryCutaway, Is.False);
            levels.Ball.Body.position = new Vector3(-.1f, 0, 0); levels.Ball.Body.linearVelocity = Vector3.zero;
            UnityEngine.Physics.SyncTransforms(); Steps(1);
            Assert.That(levels.Ball.Body.linearVelocity.y / Dt, Is.EqualTo(-8.050707f).Within(.003f));
        }

        [Test]
        public void Mercury_FromSpawnReachesOpeningAndEscapesUsingOnlyBoxRotation()
        {
            Load(13); Rigidbody root = levels.Current.GetComponent<Rigidbody>();
            int wins = 0; levels.Current.Exit.Exited += () => wins++;
            Quaternion upsideDown = Quaternion.Euler(0, 0, 180);
            levels.Current.Rotation.SetTargetOrientation(upsideDown); Steps(420);
            DriveTo(new Vector2(-.1f, -.105f), false);
            DriveTo(new Vector2(.105f, -.105f), true);
            if (!levels.Current.Exit.HasExited)
            {
                // Tip the real moving rim away from the partly emerged sphere.
                // This is test input only; there is no runtime release animation/force.
                levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(0, 0, 90));
                int ticks = 0;
                while (ticks < 240 && !levels.Current.Exit.HasExited) { Steps(1); ticks++; }
                TestContext.WriteLine($"Mercury turn-to-release: {ticks * Dt:F3}s; exited={levels.Current.Exit.HasExited}; local={Quaternion.Inverse(root.rotation) * levels.Ball.Body.position}.");
            }
            Assert.That(levels.Current.Exit.HasExited, Is.True);
            Assert.That(wins, Is.EqualTo(1)); Assert.That(levels.Ball.Body.isKinematic, Is.False);
            Steps(6);
            Assert.That(levels.Current.GetComponent<WaterVolume>().SubmergedFraction, Is.Zero);

            void DriveTo(Vector2 goal, bool exit)
            {
                for (int tick = 0; tick < 2400; tick++)
                {
                    Quaternion inv = Quaternion.Inverse(root.rotation);
                    Vector3 p = inv * (levels.Ball.Body.position - root.position);
                    Vector3 v = inv * (levels.Ball.Body.linearVelocity - root.GetPointVelocity(levels.Ball.Body.position));
                    Vector2 error = goal - new Vector2(p.x, p.z);
                    Vector2 a = Vector2.ClampMagnitude(error * 12 - new Vector2(v.x, v.z) * 5, 1.8f);
                    levels.Current.Rotation.SetTargetOrientation(upsideDown * Quaternion.FromToRotation(new Vector3(a.x, -3.82f, a.y), Vector3.down));
                    Steps(1);
                    if (levels.Current.Exit.HasExited || (!exit && error.magnitude < .009f && v.magnitude < .12f)
                        || (exit && error.magnitude < .005f && p.y < -.034f))
                    { TestContext.WriteLine($"Mercury route {goal}: {(tick + 1) * Dt:F3}s; exited={levels.Current.Exit.HasExited}."); return; }
                    Assert.That(levels.Current.IsOutside(levels.Ball.Body.position), Is.False);
                }
                TestContext.WriteLine($"Mercury route final local {Quaternion.Inverse(root.rotation) * levels.Ball.Body.position}, v {levels.Ball.Body.linearVelocity}, fraction {levels.Current.GetComponent<WaterVolume>().SubmergedFraction}.");
                Assert.Fail("Mercury route did not reach " + goal);
            }
        }
    }
}
#endif
