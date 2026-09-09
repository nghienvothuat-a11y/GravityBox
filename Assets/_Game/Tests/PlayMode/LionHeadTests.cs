#if UNITY_EDITOR
using GravityBox.Gameplay;
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed partial class PhysicsLifecycleTests
    {
        [Test]
        public void LionHead_UsesEarthGravityAndRealFacialReliefInsideItsMane()
        {
            Load(14);
            Assert.That(levels.Definition.Shape, Is.EqualTo(ContainerShape.LionHead));
            Assert.That(levels.Definition.Environment, Is.SameAs(levels.Catalog.Levels[1].Environment));
            Assert.That(levels.Definition.Environment.Acceleration, Is.EqualTo(Vector3.down * 9.81f));
            Assert.That(levels.Current.GetComponent<WaterVolume>(), Is.Null);
            Assert.That(levels.Current.Footprint.Length, Is.GreaterThanOrEqualTo(40));
            Assert.That(levels.Ball.Profile.Mass, Is.EqualTo(.11097676f).Within(.000001f));
            Assert.That(levels.Current.Exit.AssistEnabled, Is.True);
            foreach (string name in new[] { "Left eye", "Right eye", "Nose", "Left muzzle", "Right muzzle" })
            {
                Transform feature = levels.Current.transform.Find(name + " - fixed relief");
                Assert.That(feature, Is.Not.Null);
                var collider = feature.GetComponent<MeshCollider>();
                Assert.That(collider, Is.Not.Null); Assert.That(collider.convex, Is.True);
                Assert.That(collider.sharedMesh, Is.SameAs(feature.GetComponent<MeshFilter>().sharedMesh));
                Assert.That(collider.attachedRigidbody, Is.SameAs(levels.Current.GetComponent<Rigidbody>()));
                Assert.That(collider.sharedMaterial, Is.SameAs(levels.Ball.Profile.ContactMaterial));
            }
            foreach (string name in new[] { "Lion face inlay", "Mane floor facets", "Left ear inset", "Right ear inset" })
                Assert.That(levels.Current.transform.Find(name).GetComponent<Collider>(), Is.Null);
        }

        [Test]
        public void LionHead_NosePhysicallyRedirectsAnIncomingSteelBall()
        {
            Load(14);
            levels.Ball.Body.position = new Vector3(0,-.027f,.075f);
            levels.Ball.Body.linearVelocity = Vector3.back * .45f;
            UnityEngine.Physics.SyncTransforms(); levels.Current.Exit.BeginTracking();
            bool rebounded = false; float closest = float.PositiveInfinity;
            for (int tick=0;tick<100;tick++)
            {
                Steps(1); closest = Mathf.Min(closest,levels.Ball.Body.position.z);
                rebounded |= levels.Ball.Body.linearVelocity.z > .04f;
            }
            Assert.That(closest, Is.GreaterThan(.023f), "The nose must retain the sphere on its front face.");
            Assert.That(rebounded, Is.True, "The actual bevelled nose must redirect the incoming velocity.");
            Assert.That(levels.Current.Exit.AssistActive, Is.False);
            TestContext.WriteLine($"Lion nose impact: closest centre z={closest:F5} m; rebound={rebounded}.");
        }

        [TestCase(-1)] [TestCase(1)]
        public void LionHead_FromAuthoredSpawnAroundEitherCheekToFullExit(int side)
        {
            Load(14); Rigidbody root = levels.Current.GetComponent<Rigidbody>();
            Vector3 spawn = levels.Ball.Body.position;
            Assert.That(spawn, Is.EqualTo(levels.Current.BallSpawn.position));
            Vector2[] route = { new Vector2(-.18f,.22f), new Vector2(0,.20f), new Vector2(0,.05f),
                new Vector2(side*.18f,.04f), new Vector2(side*.185f,-.17f), new Vector2(0,-.215f) };
            int wins = 0; levels.Current.Exit.Exited += () => wins++;
            Steps(120);
            for (int waypoint = 0; waypoint < route.Length; waypoint++)
            {
                bool reached = false; int tick;
                for (tick = 0; tick < 1800; tick++)
                {
                    Quaternion inverse = Quaternion.Inverse(root.rotation);
                    Vector3 local = inverse * (levels.Ball.Body.position - root.position);
                    Vector3 v = inverse * (levels.Ball.Body.linearVelocity - root.GetPointVelocity(levels.Ball.Body.position));
                    Vector2 error = route[waypoint] - new Vector2(local.x,local.z);
                    Vector2 acceleration = Vector2.ClampMagnitude(error*8 - new Vector2(v.x,v.z)*5, 1);
                    levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(acceleration.x,-9.81f,acceleration.y),Vector3.down));
                    Steps(1);
                    if (levels.Current.Exit.HasExited)
                    {
                        Assert.That(waypoint, Is.EqualTo(route.Length-1)); reached = true; break;
                    }
                    Assert.That(levels.Current.IsOutside(levels.Ball.Body.position), Is.False);
                    if (waypoint < route.Length-1 && error.magnitude < .012f && v.magnitude < .08f)
                    { reached = true; break; }
                }
                TestContext.WriteLine($"Lion side {side}, waypoint {waypoint}: {tick*Dt:F3}s; local {root.transform.InverseTransformPoint(levels.Ball.Body.position):F4}.");
                Assert.That(reached, Is.True, "Blocked route at " + waypoint);
            }
            Assert.That(wins, Is.EqualTo(1)); Assert.That(levels.Ball.Body.isKinematic, Is.False);
            levels.ResetLevel(); Steps(1);
            Assert.That(levels.Current.Exit.HasExited, Is.False); Assert.That(levels.Current.Exit.AssistActive, Is.False);
            Assert.That(Vector3.Distance(levels.Ball.Body.position,spawn), Is.LessThan(.003f));
        }
    }
}
#endif
