#if UNITY_EDITOR
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed partial class PhysicsLifecycleTests
    {
        [Test]
        public void Water_SameSquareAndSteelWithBuoyancyMatchingDisplacedWater()
        {
            Load(12);
            WaterVolume water = levels.Current.GetComponent<WaterVolume>();
            Assert.That(water, Is.Not.Null);
            CollectionAssert.AreEqual(levels.Catalog.Levels[1].Prefab.Footprint, levels.Current.Footprint);
            Assert.That(levels.Ball.Body.mass, Is.EqualTo(.11097676f).Within(.000001f));
            Assert.That(levels.Ball.Body.linearDamping + levels.Ball.Body.angularDamping, Is.Zero);
            Assert.That(water.GetComponent<WaterVisuals>().VolumeRenderer.GetComponent<Collider>(), Is.Null);
            // Begin fully immersed, clear of every surface. Observe the first actual
            // physics tick: steel sinks, but with buoyancy opposing Earth's gravity.
            levels.Ball.Body.position = new Vector3(-.1f, 0, 0);
            levels.Ball.Body.linearVelocity = Vector3.zero;
            UnityEngine.Physics.SyncTransforms();
            Steps(1);
            float displacedMass = 998.2f * 4f / 3f * Mathf.PI * Mathf.Pow(Radius, 3);
            float expectedBuoyancy = displacedMass * 9.81f;
            Assert.That(water.SubmergedFraction, Is.EqualTo(1).Within(.00001f));
            Assert.That(water.BuoyancyForce.y, Is.EqualTo(expectedBuoyancy).Within(.00001f));
            Assert.That(water.DragForce.magnitude, Is.LessThan(.000001f));
            Assert.That(levels.Ball.Body.linearVelocity.y / Dt,
                Is.EqualTo(-9.81f + expectedBuoyancy / levels.Ball.Body.mass).Within(.003f));
            Assert.That(levels.Ball.Body.linearVelocity.y, Is.LessThan(0));
            TestContext.WriteLine($"Water: buoyancy {expectedBuoyancy:F6} N; initial sinking acceleration {levels.Ball.Body.linearVelocity.y / Dt:F5} m/s².");
        }

        [Test]
        public void Water_DragOpposesRelativeMotionAndMatchesQuadraticCoastingReference()
        {
            Load(12);
            WaterVolume water = levels.Current.GetComponent<WaterVolume>();
            EnvironmentProfile zeroG = Object.Instantiate(levels.Definition.Environment);
            try
            {
                zeroG.GravityScale = 0;
                // Calibration tank without boundaries, using the shipped force path.
                foreach (Collider collider in levels.Current.GetComponentsInChildren<Collider>()) collider.enabled = false;
                water.HalfSize = Vector3.one * 10;
                forces.Configure(levels.Ball, zeroG); water.Bind(levels.Ball, zeroG); forces.AddProvider(water);
                levels.Ball.Body.position = Vector3.zero; levels.Ball.Body.linearVelocity = Vector3.right;
                float k = .5f * 998.2f * .44f * Mathf.PI * Radius * Radius / levels.Ball.Body.mass;
                float previous = 1;
                for (int tick = 0; tick < 120; tick++)
                {
                    Steps(1);
                    Assert.That(Vector3.Dot(water.DragForce, water.RelativeVelocity), Is.LessThanOrEqualTo(0));
                    Assert.That(levels.Ball.Body.linearVelocity.x, Is.InRange(0, previous));
                    previous = levels.Ball.Body.linearVelocity.x;
                }
                float expected = 1 / (1 + k);
                Assert.That(previous, Is.EqualTo(expected).Within(.004f));
                Assert.That(levels.Ball.Body.linearVelocity.y, Is.EqualTo(0).Within(.00001f));
                TestContext.WriteLine($"Water coasting: 1.000 → {previous:F5} m/s in 1 s; analytic quadratic drag {expected:F5} m/s.");
                Vector3 a = WaterVolume.SphereDrag(Vector3.right * .2f, Radius, 998.2f, .001002f);
                Vector3 b = WaterVolume.SphereDrag(Vector3.right * .4f, Radius, 998.2f, .001002f);
                Assert.That(b.magnitude / a.magnitude, Is.EqualTo(4).Within(.0001f));
                Vector3 slow = WaterVolume.SphereDrag(Vector3.right * .0000002f, Radius, 998.2f, .001002f);
                float stokes = 6 * Mathf.PI * .001002f * Radius * .0000002f;
                Assert.That(-slow.x, Is.EqualTo(stokes).Within(stokes * .01f));
            }
            finally { Object.DestroyImmediate(zeroG); }
        }

        [Test]
        public void Water_RestLoadIsReducedAndStillStableWithoutPulsing()
        {
            Load(12); Steps(300);
            WaterVolume water = levels.Current.GetComponent<WaterVolume>();
            float expectedLoad = levels.Ball.Body.mass * 9.81f - water.BuoyancyForce.y;
            float minLoad = float.PositiveInfinity, maxLoad = 0, maxSpeed = 0;
            int impacts = 0; levels.Ball.Impact += _ => impacts++;
            for (int tick = 0; tick < 600; tick++)
            {
                Steps(1);
                minLoad = Mathf.Min(minLoad, levels.Ball.ContactLoad); maxLoad = Mathf.Max(maxLoad, levels.Ball.ContactLoad);
                maxSpeed = Mathf.Max(maxSpeed, levels.Ball.Body.linearVelocity.magnitude);
            }
            Assert.That(minLoad, Is.GreaterThan(expectedLoad * .95f));
            Assert.That(maxLoad, Is.LessThan(expectedLoad * 1.05f));
            Assert.That(maxSpeed, Is.LessThan(.002f)); Assert.That(impacts, Is.Zero);
            TestContext.WriteLine($"Water rest: load {minLoad:F5}–{maxLoad:F5} N; predicted {expectedLoad:F5} N; peak speed {maxSpeed:F6} m/s.");
        }

        [Test]
        public void Water_ImmersionFadesAtExitAndDryBallRecoversEarthFreeFall()
        {
            Load(12); WaterVolume water = levels.Current.GetComponent<WaterVolume>();
            Vector3 exit = levels.Current.Exit.transform.position;
            float previous = 1;
            for (int i = 0; i <= 20; i++)
            {
                Vector3 p = new Vector3(exit.x, -water.HalfSize.y + Radius - i * (2 * Radius / 20), exit.z);
                float fraction = water.Immersion(p, Radius);
                Assert.That(fraction, Is.InRange(-.00001f, previous + .00001f)); previous = fraction;
                if (i == 10) Assert.That(fraction, Is.EqualTo(.5f).Within(.00001f));
            }
            Assert.That(previous, Is.LessThan(.00001f));
            LaunchThroughExit();
            Assert.That(levels.Current.Exit.HasExited, Is.True);
            Assert.That(levels.Ball.Body.isKinematic, Is.False);
            float before = levels.Ball.Body.linearVelocity.y; Steps(12);
            Assert.That(water.BuoyancyForce + water.DragForce, Is.EqualTo(Vector3.zero));
            Assert.That(levels.Ball.Body.linearVelocity.y - before, Is.EqualTo(-9.81f * 12 * Dt).Within(.003f));
            Assert.That(water.GetComponent<WaterVisuals>().VolumeRenderer.enabled, Is.True);
        }

        [Test]
        public void Water_FromSpawnGoesAroundCubeAndEscapesByTiltingOnly()
        {
            Load(12); int wins = 0; levels.Current.Exit.Exited += () => wins++;
            DriveWaterTo(new Vector2(-.10f, -.105f), false);
            DriveWaterTo(new Vector2(.105f, -.105f), true);
            Assert.That(wins, Is.EqualTo(1)); Assert.That(levels.Ball.Body.isKinematic, Is.False);
            Assert.That(levels.Current.GetComponent<WaterVolume>().SubmergedFraction, Is.LessThan(.001f));
        }

        private void DriveWaterTo(Vector2 goal, bool escape)
        {
            Rigidbody root = levels.Current.GetComponent<Rigidbody>();
            for (int tick = 0; tick < 2400; tick++)
            {
                Quaternion inv = Quaternion.Inverse(root.rotation);
                Vector3 p = inv * (levels.Ball.Body.position - root.position);
                Vector3 v = inv * (levels.Ball.Body.linearVelocity - root.GetPointVelocity(levels.Ball.Body.position));
                Vector2 error = goal - new Vector2(p.x, p.z);
                Vector2 a = Vector2.ClampMagnitude(error * 12 - new Vector2(v.x, v.z) * 5, 1.8f);
                levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(a.x, -9.81f, a.y), Vector3.down));
                Steps(1);
                if (escape ? levels.Current.Exit.HasExited : error.magnitude < .012f && v.magnitude < .15f)
                { TestContext.WriteLine($"Water route {goal}: {(tick + 1) * Dt:F3}s, exited={levels.Current.Exit.HasExited}."); return; }
                Assert.That(levels.Current.IsOutside(levels.Ball.Body.position), Is.False);
            }
            Assert.Fail("Water route did not reach " + goal);
        }

        [Test]
        public void Water_FlowWakePauseResetAndUnloadRemainBoundedAndIndependentOfDryLevels()
        {
            Load(12); WaterVolume water = levels.Current.GetComponent<WaterVolume>();
            WaterVisuals visual = water.GetComponent<WaterVisuals>(); visual.Initialize(water);
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(20, 35, 0));
            for (int tick = 0; tick < 65; tick++) { Steps(1); visual.Advance(Dt); }
            Assert.That(water.FluidAngularVelocity.magnitude, Is.GreaterThan(.01f));
            Assert.That(visual.LiveWakeCount, Is.GreaterThan(0));
            Assert.That(visual.GetComponentsInChildren<Collider>().Length,
                Is.EqualTo(levels.Catalog.Levels[1].Prefab.GetComponentsInChildren<Collider>().Length));
            for (int axis = 0; axis < 3; axis++)
            {
                Vector3 p = new Vector3(.06f, .01f, .07f); p[axis] = water.HalfSize[axis];
                Assert.That(water.LocalFlowAt(p)[axis], Is.EqualTo(0).Within(.00001f));
            }
            float clock = visual.VisualClock; visual.Advance(0);
            Assert.That(visual.VisualClock, Is.EqualTo(clock));
            levels.ResetLevel();
            Assert.That(water.FluidAngularVelocity, Is.EqualTo(Vector3.zero));
            Assert.That(visual.LiveWakeCount, Is.Zero); Assert.That(visual.VisualClock, Is.Zero);
            Assert.That(visual.GetComponentsInChildren<MeshFilter>().Length,
                Is.EqualTo(levels.Catalog.Levels[12].Prefab.GetComponentsInChildren<MeshFilter>().Length + 1));
            Load(1);
            Assert.That(levels.Current.GetComponent<WaterVolume>(), Is.Null);
            levels.Ball.Body.position = Vector3.up; levels.Ball.Body.linearVelocity = Vector3.zero; Steps(1);
            Assert.That(levels.Ball.Body.linearVelocity.y / Dt, Is.EqualTo(-9.81f).Within(.003f));
        }
    }
}
#endif
