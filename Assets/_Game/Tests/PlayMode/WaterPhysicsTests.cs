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
            Assert.That(water.AddedMass, Is.EqualTo(displacedMass * .5f).Within(.0000001f));
            Assert.That(levels.Ball.Body.mass, Is.EqualTo(levels.Ball.Profile.Mass + displacedMass * .5f).Within(.0000001f));
            Assert.That(levels.Ball.Body.inertiaTensor.x, Is.EqualTo(levels.Ball.Profile.SolidSphereInertia).Within(1e-9f));
            Assert.That(levels.Ball.Body.linearVelocity.y / Dt,
                Is.EqualTo((-9.81f * levels.Ball.Profile.Mass + expectedBuoyancy) / levels.Ball.Body.mass).Within(.003f));
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
                float effectiveMass = levels.Ball.Profile.Mass + .5f * 998.2f * 4f / 3f * Mathf.PI * Mathf.Pow(Radius, 3);
                float k = .5f * 998.2f * .44f * Mathf.PI * Radius * Radius / effectiveMass;
                float previous = 1;
                for (int tick = 0; tick < 120; tick++)
                {
                    Steps(1);
                    Assert.That(Vector3.Dot(water.DragForce, water.RelativeVelocity), Is.LessThanOrEqualTo(0));
                    Assert.That(levels.Ball.Body.linearVelocity.x, Is.InRange(0, previous));
                    previous = levels.Ball.Body.linearVelocity.x;
                }
                float expected = 1 / (1 + k);
                Assert.That(previous, Is.EqualTo(expected).Within(.0001f));
                Assert.That(water.WallDragForce, Is.EqualTo(Vector3.zero));
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
            float expectedLoad = levels.Ball.Profile.Mass * 9.81f - water.BuoyancyForce.y;
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
        public void Water_RollingCorrelationMatchesPublishedRegimesAndStaysFinite()
        {
            float[] reynolds = { 70, 100, 150, 5000 };
            float[] referenceCd = { 4.216741f, 3.249600f, 2.470332f, 1.042160f };
            for (int i = 0; i < reynolds.Length; i++)
            {
                float speed = reynolds[i] * .001002f / (998.2f * 2 * Radius);
                float force = WaterHydrodynamics.RollingSphereResistance(speed, Radius, 998.2f, .001002f, .000003f);
                float cd = force / (.5f * 998.2f * Mathf.PI * Radius * Radius * speed * speed);
                Assert.That(cd, Is.EqualTo(referenceCd[i]).Within(.00001f));
            }
            // Check the two joins and limiting behaviour independently of timestep.
            float previous = 0;
            for (int i = -7; i <= 5; i++)
            {
                float speed = Mathf.Pow(10, i);
                float force = WaterHydrodynamics.RollingSphereResistance(speed, Radius, 998.2f, .001002f, .000003f);
                Assert.That(float.IsNaN(force) || float.IsInfinity(force), Is.False);
                Assert.That(force, Is.GreaterThan(previous)); previous = force;
            }
            foreach (float re in new[] { 5f, 300f, 1000f })
            {
                float speed = re * .001002f / (998.2f * 2 * Radius);
                float a = WaterHydrodynamics.RollingSphereResistance(speed * .99999f, Radius, 998.2f, .001002f, .000003f);
                float b = WaterHydrodynamics.RollingSphereResistance(speed * 1.00001f, Radius, 998.2f, .001002f, .000003f);
                Assert.That(b / a, Is.InRange(1, 1.0001f));
            }
        }

        [TestCase(60)]
        [TestCase(120)]
        [TestCase(240)]
        public void Water_RollingCoastMatchesAnalyticReferenceAndRefinesWithTimestep(int hz)
        {
            Load(12);
            WaterVolume water = levels.Current.GetComponent<WaterVolume>();
            BallPhysicsProfile steel = Object.Instantiate(levels.Ball.Profile);
            try
            {
                // A long, level calibration plate. Keep actual gravity, buoyancy,
                // contacts and solid-sphere inertia; omit dry deformation resistance
                // only in this fixture to isolate the hydrodynamic rolling equation.
                foreach (Collider collider in levels.Current.GetComponentsInChildren<Collider>()) collider.enabled = false;
                var plate = new GameObject("Water calibration plate");
                plate.transform.SetParent(levels.Current.transform, false);
                plate.transform.localPosition = new Vector3(0, -Radius - .01f, 0);
                BoxCollider plane = plate.AddComponent<BoxCollider>();
                plane.size = new Vector3(10, .02f, 10); plane.sharedMaterial = steel.ContactMaterial;
                plane.contactOffset = steel.ContactOffset;
                steel.RollingResistanceCoefficient = 0;
                levels.Ball.Configure(steel, Vector3.zero, false);
                water.HalfSize = Vector3.one * 10;
                water.Bind(levels.Ball, levels.Definition.Environment);
                levels.Ball.Body.position = Vector3.zero;
                Time.fixedDeltaTime = 1f / hz; UnityEngine.Physics.SyncTransforms();
                for (int tick = 0; tick < hz; tick++) { forces.Step(); UnityEngine.Physics.Simulate(Time.fixedDeltaTime); }
                Rigidbody rb = levels.Ball.Body;
                rb.linearVelocity = Vector3.right * .3f;
                rb.angularVelocity = Vector3.back * (.3f / Radius);
                float previousEnergy = .5f * rb.mass * rb.linearVelocity.sqrMagnitude
                    + .5f * steel.SolidSphereInertia * rb.angularVelocity.sqrMagnitude;
                float minWeight = 1;
                for (int tick = 0; tick < hz; tick++)
                {
                    forces.Step(); UnityEngine.Physics.Simulate(Time.fixedDeltaTime);
                    float energy = .5f * rb.mass * rb.linearVelocity.sqrMagnitude
                        + .5f * steel.SolidSphereInertia * rb.angularVelocity.sqrMagnitude;
                    Assert.That(energy, Is.LessThanOrEqualTo(previousEnergy + 1e-7f)); previousEnergy = energy;
                    minWeight = Mathf.Min(minWeight, water.WallRollingWeight);
                }
                // Independent closed-form benchmark for v' = -b*v - k*v², using
                // the published plateau Cd=1 and G/D=1e-4, with inertia m+ma+I/r².
                const float reference = .17941799f;
                Assert.That(rb.linearVelocity.x, Is.EqualTo(reference).Within(.0015f));
                Assert.That(minWeight, Is.GreaterThan(.96f));
                Assert.That(levels.Ball.ContactSlipSpeed, Is.LessThan(.001f));
                TestContext.WriteLine($"Water rolling {hz} Hz: 0.300 → {rb.linearVelocity.x:F6} m/s after 1s; reference {reference:F6}; min rolling weight {minWeight:F5}.");
            }
            finally { Object.DestroyImmediate(steel); Time.fixedDeltaTime = Dt; }
        }

        [Test]
        public void Water_WallResistanceUsesPhysicalFloorAndCubeButDoesNotSealBore()
        {
            Load(12); WaterVolume water = levels.Current.GetComponent<WaterVolume>();
            Rigidbody rb = levels.Ball.Body;
            rb.position = new Vector3(-.1f, -.027f, 0);
            rb.linearVelocity = Vector3.right * .2f; rb.angularVelocity = Vector3.back * (.2f / Radius);
            UnityEngine.Physics.SyncTransforms(); water.PrepareStep(Dt);
            Assert.That(water.WallRollingWeight, Is.GreaterThan(.99f));
            Assert.That(water.DragForce.magnitude, Is.InRange(.0143f, .0148f));
            TestContext.WriteLine($"Water actual floor at 0.2 m/s: drag {water.DragForce.magnitude:F6} N (old isolated sphere 0.006209 N).");
            rb.position = new Vector3(-.047f, -.01f, 0);
            rb.linearVelocity = Vector3.forward * .2f; rb.angularVelocity = Vector3.up * (.2f / Radius);
            UnityEngine.Physics.SyncTransforms(); water.PrepareStep(Dt);
            Assert.That(water.WallRollingWeight, Is.GreaterThan(.99f), "Cube side must supply wall resistance too.");
            rb.position = new Vector3(.105f, -.027f, -.105f);
            rb.linearVelocity = Vector3.right * .2f; rb.angularVelocity = Vector3.back * (.2f / Radius);
            UnityEngine.Physics.SyncTransforms(); water.PrepareStep(Dt);
            Assert.That(water.WallRollingWeight, Is.Zero, "The open bore must not be treated as an infinite floor.");
            Assert.That(water.WallDragForce, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Water_MovingFluidPressureUsesMaterialAccelerationAndDisableRestoresMass()
        {
            Load(12); WaterVolume water = levels.Current.GetComponent<WaterVolume>();
            Rigidbody root = levels.Current.GetComponent<Rigidbody>();
            Rigidbody rb = levels.Ball.Body;
            rb.position = new Vector3(-.1f, 0, 0); water.PrepareStep(Dt);
            rb.position = new Vector3(.1f, 0, 0); water.PrepareStep(Dt);
            Assert.That(water.FluidAccelerationForce, Is.EqualTo(Vector3.zero), "Moving a sample through still water cannot accelerate water.");
            root.position += Vector3.right * Dt; water.PrepareStep(Dt);
            Assert.That(water.FluidAccelerationForce.x, Is.EqualTo(.02116758f / Dt).Within(.001f));
            root.position += Vector3.right * Dt; water.PrepareStep(Dt);
            Assert.That(water.FluidAccelerationForce.magnitude, Is.LessThan(.0001f), "Constant uniform flow has zero material acceleration.");
            Vector3 before = rb.linearVelocity;
            water.enabled = false;
            Assert.That(rb.mass, Is.EqualTo(levels.Ball.Profile.Mass).Within(1e-7f));
            Assert.That(rb.linearVelocity, Is.EqualTo(before));
            Assert.That(water.GetAcceleration(levels.Ball, levels.Definition.Environment), Is.EqualTo(Vector3.zero));
            water.enabled = true;
            Assert.That(water.FluidAccelerationForce, Is.EqualTo(Vector3.zero));
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
            Assert.That(water.AddedMass, Is.Zero);
            Assert.That(levels.Ball.Body.mass, Is.EqualTo(levels.Ball.Profile.Mass).Within(1e-7f));
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
            Assert.That(water.FluidAccelerationForce, Is.EqualTo(Vector3.zero));
            Assert.That(water.AddedMass, Is.Zero);
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
