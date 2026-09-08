using System.Collections;
using System.Collections.Generic;
using GravityBox.Foundation;
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using GravityBox.Gameplay;
using UnityEditor;
#endif

namespace GravityBox.Tests
{
    // Independent SI-unit calibration, using the same controller and force path as the playable boxes.
    // These tests do not depend on level layouts, goals, presentation, or a prerecorded solution.
    public sealed class SteelBallPhysicsTests
    {
        private const float Dt = 1f / 120;
        private const float Radius = 0.015f;
        private readonly List<Object> owned = new List<Object>();
        private Scene scene;
        private PhysicsScene physics;
        private BallController ball;
        private BallPhysicsProfile profile;
        private EnvironmentProfile earth;
        private EnvironmentForceSystem forces;
        private PhysicsMaterial material;
        private float previousFixedDelta, previousMaximumDelta, previousContactOffset, previousBounceThreshold;
        private int previousSolverIterations, previousSolverVelocityIterations;
        private Vector3 previousGravity;

        [SetUp]
        public void Setup()
        {
            previousFixedDelta = Time.fixedDeltaTime;
            previousMaximumDelta = Time.maximumDeltaTime;
            previousContactOffset = UnityEngine.Physics.defaultContactOffset;
            previousBounceThreshold = UnityEngine.Physics.bounceThreshold;
            previousSolverIterations = UnityEngine.Physics.defaultSolverIterations;
            previousSolverVelocityIterations = UnityEngine.Physics.defaultSolverVelocityIterations;
            previousGravity = UnityEngine.Physics.gravity;
            PhysicsTiming.Apply();
            Assert.That(Time.fixedDeltaTime, Is.EqualTo(Dt));
            scene = SceneManager.CreateScene("Steel sphere calibration", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            physics = scene.GetPhysicsScene();
            material = Own(new PhysicsMaterial("Steel calibration contact")
            {
                staticFriction = 0.5f, dynamicFriction = 0.4f, bounciness = 0.28f,
                frictionCombine = PhysicsMaterialCombine.Average, bounceCombine = PhysicsMaterialCombine.Average
            });
            profile = Own(ScriptableObject.CreateInstance<BallPhysicsProfile>());
            profile.Radius = Radius;
            profile.Mass = 4f / 3f * Mathf.PI * Radius * Radius * Radius * 7850f;
            profile.ContactMaterial = material;
            earth = Own(ScriptableObject.CreateInstance<EnvironmentProfile>());
            earth.LinearDamping = earth.AngularDamping = 0;
            earth.MaxLinearSpeed = 6;
            // Intentionally use the old cap to prove registration raises it to permit v = omega r.
            earth.MaxAngularSpeed = 35;
            ball = Node("Steel sphere").AddComponent<BallController>();
            ball.Configure(profile, Vector3.zero, false);
            forces = Node("Environment").AddComponent<EnvironmentForceSystem>();
            forces.enabled = false;
            forces.Configure(ball, earth);
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) Object.DestroyImmediate(owned[i]);
            owned.Clear();
            Time.fixedDeltaTime = previousFixedDelta;
            Time.maximumDeltaTime = previousMaximumDelta;
            UnityEngine.Physics.defaultContactOffset = previousContactOffset;
            UnityEngine.Physics.bounceThreshold = previousBounceThreshold;
            UnityEngine.Physics.defaultSolverIterations = previousSolverIterations;
            UnityEngine.Physics.defaultSolverVelocityIterations = previousSolverVelocityIterations;
            UnityEngine.Physics.gravity = previousGravity;
            if (scene.IsValid()) yield return SceneManager.UnloadSceneAsync(scene);
        }

        private T Own<T>(T value) where T : Object { owned.Add(value); return value; }
        private GameObject Node(string name)
        {
            var result = Own(new GameObject(name));
            SceneManager.MoveGameObjectToScene(result, scene);
            return result;
        }

        private BoxCollider Box(string name, Vector3 position, Vector3 size, Quaternion rotation)
        {
            GameObject node = Node(name);
            node.transform.SetPositionAndRotation(position, rotation);
            BoxCollider collider = node.AddComponent<BoxCollider>();
            collider.size = size;
            collider.contactOffset = profile.ContactOffset;
            collider.sharedMaterial = material;
            return collider;
        }

        private void Place(Vector3 position)
        {
            ball.Body.position = position;
            ball.CaptureInitialState();
            ball.ResetState();
            UnityEngine.Physics.SyncTransforms();
        }

        private void Steps(int count)
        {
            for (int i = 0; i < count; i++)
            {
                forces.Step();
                physics.Simulate(Dt);
            }
        }

        private float KineticEnergy() => 0.5f * profile.Mass * ball.Body.linearVelocity.sqrMagnitude
                                      + 0.5f * profile.SolidSphereInertia * ball.Body.angularVelocity.sqrMagnitude;

        [Test]
        public void SolidSteelSphere_HasDensityDerivedMassInertiaAndSufficientSpinLimit()
        {
            Assert.That(ball.Body.mass, Is.EqualTo(0.11097676f).Within(0.000001f));
            float expected = 2f / 5f * ball.Body.mass * Radius * Radius;
            Assert.That(ball.Body.inertiaTensor.x, Is.EqualTo(expected).Within(expected * 0.0001f));
            Assert.That(ball.Body.inertiaTensor.y, Is.EqualTo(expected).Within(expected * 0.0001f));
            Assert.That(ball.Body.inertiaTensor.z, Is.EqualTo(expected).Within(expected * 0.0001f));
            Assert.That(ball.Body.maxAngularVelocity, Is.GreaterThanOrEqualTo(earth.MaxLinearSpeed / Radius));
            Assert.That(ball.Body.linearDamping + ball.Body.angularDamping, Is.Zero);
        }

        [Test]
        public void FreeFall_AcceleratesAt981RegardlessOfSteelMassAndRetainsAirborneSpin()
        {
            Place(Vector3.up);
            Vector3 spin = new Vector3(20, 30, 40);
            ball.Body.angularVelocity = spin;
            Steps(24);
            Assert.That(ball.Body.linearVelocity.y, Is.EqualTo(-9.81f * 24 * Dt).Within(0.003f));
            Assert.That(ball.Body.position.y, Is.EqualTo(1 - 0.5f * 9.81f * 0.2f * 0.2f).Within(0.01f));
            Assert.That(Vector3.Distance(ball.Body.angularVelocity, spin), Is.LessThan(0.001f));
            Assert.That(ball.HasContact, Is.False);
            Assert.That(ball.ContactSpeed + ball.ContactLoad, Is.Zero);
        }

        [Test]
        public void TenDegreeIncline_RollsWithSolidSphereAccelerationAndNoSlip()
        {
            profile.RollingResistanceCoefficient = 0;
            material.bounciness = 0;
            Quaternion slope = Quaternion.Euler(0, 0, -10);
            Vector3 normal = slope * Vector3.up;
            Vector3 downhill = slope * Vector3.right;
            Box("Inclined contact surface", -normal * 0.01f, new Vector3(0.34f, 0.02f, 0.2f), slope);
            Place(normal * (Radius + 0.00005f));
            Steps(8);
            float before = Vector3.Dot(ball.Body.linearVelocity, downhill);
            Steps(30);
            float speed = Vector3.Dot(ball.Body.linearVelocity, downhill);
            float measuredAcceleration = (speed - before) / (30 * Dt);
            float idealAcceleration = 5f / 7f * 9.81f * Mathf.Sin(10 * Mathf.Deg2Rad);
            Assert.That(measuredAcceleration, Is.EqualTo(idealAcceleration).Within(idealAcceleration * 0.12f));
            Assert.That(ball.Body.angularVelocity.magnitude * Radius / speed, Is.InRange(0.88f, 1.12f));
            Assert.That(ball.ContactSlipSpeed, Is.LessThan(0.035f));
            Assert.That(ball.ContactLoad, Is.EqualTo(profile.Mass * 9.81f * Mathf.Cos(10 * Mathf.Deg2Rad)).Within(0.18f));
        }

        [Test]
        public void RollingResistance_DissipatesContactEnergyWithoutDampingFreeFlight()
        {
            material.bounciness = 0;
            Box("Level plate", Vector3.down * 0.01f, new Vector3(0.34f, 0.02f, 0.2f), Quaternion.identity);
            Place(Vector3.up * (Radius + 0.00005f));
            Steps(12);
            ball.Body.linearVelocity = Vector3.right * 0.3f;
            ball.Body.angularVelocity = Vector3.back * (0.3f / Radius);
            float initial = KineticEnergy();
            Steps(30);
            TestContext.WriteLine($"Level rolling over 0.25 s: 0.30000 → {ball.Body.linearVelocity.x:F5} m/s; kinetic energy {initial:F7} → {KineticEnergy():F7} J; normal load {ball.ContactLoad:F5} N.");
            Assert.That(KineticEnergy(), Is.InRange(initial * 0.5f, initial * 0.98f));
            Assert.That(ball.Body.linearVelocity.x, Is.GreaterThan(0));
            Assert.That(ball.HasContact, Is.True);
            Assert.That(ball.ContactSpeed, Is.GreaterThan(0.2f));
            Place(Vector3.up);
            ball.Body.angularVelocity = Vector3.back * 20;
            Steps(24);
            Assert.That(ball.Body.angularVelocity.z, Is.EqualTo(-20).Within(0.001f));
            Assert.That(ball.HasContact, Is.False);
        }

        [TestCase(1f)]
        [TestCase(3f)]
        public void SteelSphere_StrikesFixedCubeReboundsAndLosesEnergyWithoutTunnelling(float launchSpeed)
        {
            earth.GravityScale = 0;
            Box("Fixed sixty millimetre cube", new Vector3(0, 0.03f, 0), Vector3.one * 0.06f, Quaternion.identity);
            Place(new Vector3(-0.13f, 0.03f, 0));
            ball.Body.linearVelocity = Vector3.right * launchSpeed;
            float before = KineticEnergy();
            float measuredImpulse = 0;
            ball.Impact += impulse => measuredImpulse += impulse;
            int steps = 0;
            while (ball.Body.linearVelocity.x >= 0 && steps++ < 30) Steps(1);
            Assert.That(ball.Body.position.x, Is.LessThan(-0.03f));
            Assert.That(ball.Body.linearVelocity.x, Is.InRange(-launchSpeed * 0.4f, -launchSpeed * 0.18f));
            Assert.That(KineticEnergy(), Is.LessThan(before * 0.2f));
            Assert.That(measuredImpulse, Is.EqualTo(profile.Mass * (launchSpeed - ball.Body.linearVelocity.x)).Within(0.005f));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LeavingOneOfTwoContacts_KeepsFloorContactAndResetClearsBothReadoutsAndMotion(bool compoundBox)
        {
            material.bounciness = 0;
            BoxCollider floor = Box("Floor", Vector3.down * 0.01f, new Vector3(0.34f, 0.02f, 0.2f), Quaternion.identity);
            BoxCollider wall = Box("Right wall", new Vector3(0.06f, 0.06f, 0), new Vector3(0.02f, 0.12f, 0.2f), Quaternion.identity);
            if (compoundBox)
            {
                Rigidbody boxBody = Node("Compound box").AddComponent<Rigidbody>();
                boxBody.isKinematic = true;
                boxBody.useGravity = false;
                floor.transform.SetParent(boxBody.transform, true);
                wall.transform.SetParent(boxBody.transform, true);
            }
            earth.WorldGravityDirection = new Vector3(1, -1, 0);
            Place(new Vector3(0.035f, Radius, 0));
            Steps(30);
            Assert.That(ball.ContactCount, Is.EqualTo(2));
            wall.enabled = false;
            Steps(2);
            Assert.That(ball.ContactCount, Is.EqualTo(1));
            Assert.That(ball.HasContact, Is.True);
            Assert.That(ball.ContactLoad, Is.GreaterThan(0));
            for (int i = 0; i < 25; i++)
            {
                ball.ResetState();
                Assert.That(ball.Body.linearVelocity.sqrMagnitude + ball.Body.angularVelocity.sqrMagnitude, Is.Zero);
                Assert.That(ball.HasContact, Is.False);
                Assert.That(ball.ContactSpeed + ball.ContactSlipSpeed + ball.ContactLoad, Is.Zero);
                Steps(2);
                Assert.That(float.IsNaN(ball.Body.position.y), Is.False);
                Assert.That(ball.Body.position.y, Is.GreaterThan(Radius * 0.9f));
            }
        }

        [Test]
        public void HandControlledBox_StartsAndReversesWithinAngularAccelerationAndSpeedBounds()
        {
            GameObject root = Node("Hand controlled box");
            RotationSettings settings = Own(ScriptableObject.CreateInstance<RotationSettings>());
            BoxRotationController rotation = root.AddComponent<BoxRotationController>();
            rotation.enabled = false;
            rotation.Configure(settings, RotationMode.Free);
            rotation.CaptureInitialState();
            rotation.SetTargetOrientation(Quaternion.Euler(0, 0, 150));
            Vector3 previous = Vector3.zero;
            for (int i = 0; i < 48; i++)
            {
                if (i == 24) rotation.SetTargetOrientation(Quaternion.Euler(0, 0, -150));
                rotation.Step(Dt);
                Assert.That(rotation.CommandedAngularVelocity.magnitude, Is.LessThanOrEqualTo(settings.MaxDegreesPerSecond + 0.001f));
                Assert.That(Vector3.Distance(rotation.CommandedAngularVelocity, previous),
                    Is.LessThanOrEqualTo(settings.MaxDegreesPerSecondSquared * Dt + 0.001f));
                previous = rotation.CommandedAngularVelocity;
                physics.Simulate(Dt);
            }
            rotation.ResetState();
            Assert.That(rotation.CommandedAngularVelocity, Is.EqualTo(Vector3.zero));
            Assert.That(Quaternion.Angle(rotation.Orientation, Quaternion.identity), Is.LessThan(0.001f));
        }

#if UNITY_EDITOR
        private LevelRuntime LoadShippedContainer(int index)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/_Game/ScriptableObjects/LevelCatalog.asset");
            Assert.That(catalog, Is.Not.Null);
            profile = Own(Object.Instantiate(catalog.BallProfile));
            earth = Own(Object.Instantiate(catalog.Levels[index].Environment));
            material = profile.ContactMaterial;
            ball.Configure(profile, Vector3.zero, false);
            forces.Configure(ball, earth);
            GameObject container = Own(Object.Instantiate(catalog.Levels[index].Prefab.gameObject));
            SceneManager.MoveGameObjectToScene(container, scene);
            LevelRuntime level = container.GetComponent<LevelRuntime>();
            level.Rotation.enabled = false;
            return level;
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        public void ShippedSteelBall_SettlesAtRestWithoutRecurringBouncesOrPulsingLoad(int index)
        {
            LevelRuntime level = LoadShippedContainer(index);
            Place(level.BallSpawn.position);
            Steps(240);
            float minHeight = float.PositiveInfinity, maxHeight = float.NegativeInfinity;
            float minLoad = float.PositiveInfinity, maxLoad = 0, maxSpeed = 0, maxSpin = 0;
            int newImpacts = 0;
            ball.Impact += _ => newImpacts++;
            for (int i = 0; i < 600; i++)
            {
                Steps(1);
                minHeight = Mathf.Min(minHeight, ball.Body.position.y);
                maxHeight = Mathf.Max(maxHeight, ball.Body.position.y);
                minLoad = Mathf.Min(minLoad, ball.ContactLoad);
                maxLoad = Mathf.Max(maxLoad, ball.ContactLoad);
                maxSpeed = Mathf.Max(maxSpeed, ball.Body.linearVelocity.magnitude);
                maxSpin = Mathf.Max(maxSpin, ball.Body.angularVelocity.magnitude);
            }
            float weight = profile.Mass * earth.Acceleration.magnitude;
            TestContext.WriteLine($"Shipped box {index}: five seconds at rest, peak speed {maxSpeed:F6} m/s, height range {maxHeight - minHeight:F7} m, contact load {minLoad:F5}–{maxLoad:F5} N, recurrent impacts {newImpacts}.");
            Assert.That(maxSpeed, Is.LessThan(0.002f), "A resting sphere must not retain the gravity-step micro-bounce.");
            Assert.That(maxSpin, Is.LessThan(0.05f));
            Assert.That(maxHeight - minHeight, Is.LessThan(0.0001f));
            Assert.That(minLoad, Is.GreaterThan(weight * 0.95f));
            Assert.That(maxLoad, Is.LessThan(weight * 1.05f));
            Assert.That(newImpacts, Is.Zero);
        }

        [TestCase(0.35f)]
        [TestCase(1f)]
        [TestCase(3f)]
        public void ShippedSteelMaterial_PreservesMeaningfulCubeReboundsAboveRestTolerance(float launchSpeed)
        {
            LevelRuntime level = LoadShippedContainer(1);
            earth.GravityScale = 0;
            Transform cube = level.transform.Find("Fixed cube");
            Assert.That(cube, Is.Not.Null);
            Place(cube.position + Vector3.left * 0.13f);
            ball.Body.linearVelocity = Vector3.right * launchSpeed;
            float initialEnergy = KineticEnergy();
            int steps = 0;
            while (ball.Body.linearVelocity.x >= 0 && steps++ < 90) Steps(1);
            float restitution = -ball.Body.linearVelocity.x / launchSpeed;
            TestContext.WriteLine($"Shipped cube collision at {launchSpeed:F2} m/s: rebound {ball.Body.linearVelocity.x:F5} m/s, restitution {restitution:F4}.");
            Assert.That(ball.Body.position.x, Is.LessThan(cube.position.x));
            Assert.That(restitution, Is.EqualTo(material.bounciness).Within(0.04f));
            Assert.That(KineticEnergy(), Is.LessThan(initialEnergy * 0.25f));
        }
#endif
    }
}
