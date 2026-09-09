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
        public void NestedCage_GravityKeepsInnerFloorIndependentUntilRealJointStop()
        {
            Load(18);
            var cage = levels.Current.GetComponent<NestedCagePuzzle>().Cage;
            Assert.That(cage.transform.parent, Is.Null);
            Assert.That(cage.Joint.useMotor || cage.Joint.useSpring, Is.False);
            Assert.That(cage.Joint.enableCollision, Is.True);
            Steps(120);
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(-20, 0, 0));
            Steps(360);
            Assert.That(Quaternion.Angle(cage.Body.rotation, levels.Current.Rotation.Orientation), Is.GreaterThan(8),
                "A suspended inner cage must not simply rotate with the outer shell.");
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(-80, 0, 0));
            Steps(360);
            Assert.That(Mathf.Abs(cage.Angle), Is.InRange(28, 34), "The authored mechanical stop must eventually tip the cage.");
            Assert.That(Vector3.Distance(cage.Body.position, CagePendulumAnchor(cage)), Is.LessThan(.003f));
        }

        [Test]
        public void NestedCage_RotationAloneTransfersBallThroughMouthAndCompletes()
        {
            Load(18);
            var puzzle = levels.Current.GetComponent<NestedCagePuzzle>();
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(-76, 0, 0));
            bool transferred = false;
            for (int tick = 0; tick < 1200; tick++)
            {
                Steps(1);
                Vector3 p = Quaternion.Inverse(puzzle.Cage.Body.rotation) * (levels.Ball.Body.position - puzzle.Cage.Body.position);
                if (p.z < -.155f) { transferred = true; break; }
            }
            Assert.That(transferred, Is.True, "Tilting past the stop must let the ball leave the physical mouth.");
            levels.Current.Rotation.SetTargetOrientation(Quaternion.identity); Steps(420);
            CagePendulumMoveToExit(new Vector2(.235f, -.235f));
            Assert.That(levels.Current.Exit.HasExited, Is.True);
        }

        [Test]
        public void PendulumGate_ClosedBobBlocksBallAndSidewaysGravityOpensIt()
        {
            Load(19);
            var puzzle = levels.Current.GetComponent<PendulumGatePuzzle>();
            Assert.That(puzzle.Pendulum.Joint.useMotor || puzzle.Pendulum.Joint.useSpring, Is.False);
            // Forward tilt gives the ball a real collision with the closed bob, parallel to the hinge axis.
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(-20, 0, 0)); Steps(480);
            Assert.That(CagePendulumLocal().z, Is.GreaterThan(-.012f), "The ball bypassed a closed pendulum.");
            Assert.That(Mathf.Abs(puzzle.Pendulum.Angle), Is.LessThan(8));
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(-25, 0, -40)); Steps(480);
            Assert.That(Mathf.Abs(puzzle.Pendulum.Angle), Is.GreaterThan(24));
            Assert.That(CagePendulumLocal().z, Is.LessThan(-.035f), "The clear opening must allow physical traversal.");
            Assert.That(puzzle.Bob.enabled, Is.True);
        }

        [Test]
        public void PendulumGate_RotationAloneCrossesAndCompletesFromAuthoredSpawn()
        {
            Load(19);
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(-25, 0, -40));
            bool crossed = false;
            for (int tick = 0; tick < 1200; tick++)
            {
                Steps(1);
                if (CagePendulumLocal().z < -.10f) { crossed = true; break; }
            }
            Assert.That(crossed, Is.True, "The waiting guides should keep the rolling ball aligned with the opened aperture.");
            levels.Current.Rotation.SetTargetOrientation(Quaternion.identity); Steps(240);
            CagePendulumMoveToExit(new Vector2(.205f, -.23f));
            Assert.That(levels.Current.Exit.HasExited, Is.True);
        }

        [Test]
        public void CageAndPendulum_ResetRestoresDynamicBodiesAndWideRotationsKeepBearingsAttached()
        {
            foreach (int index in new[] { 18, 19 })
            {
                Load(index); PhysicalProp prop = levels.Current.Props[0];
                Vector3 initial = prop.Body.position; Quaternion rotation = prop.Body.rotation;
                var hinge = prop.GetComponent<PhysicalHinge>();
                foreach (Vector3 angles in new[] { new Vector3(80, 0, 20), new Vector3(170, 30, 0), new Vector3(-70, 0, -75) })
                {
                    levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(angles)); Steps(360);
                    Assert.That(Vector3.Distance(prop.Body.position, CagePendulumAnchor(hinge)), Is.LessThan(.006f));
                    Assert.That(float.IsNaN(prop.Body.rotation.x), Is.False);
                }
                levels.ResetLevel();
                Assert.That(Vector3.Distance(prop.Body.position, initial), Is.LessThan(.0001f));
                Assert.That(Quaternion.Angle(prop.Body.rotation, rotation), Is.LessThan(.01f));
                Assert.That(prop.Body.isKinematic, Is.False);
                Assert.That(prop.Body.linearVelocity, Is.EqualTo(Vector3.zero));
            }
        }

        private Vector3 CagePendulumLocal()
        {
            Rigidbody box = levels.Current.GetComponent<Rigidbody>();
            return Quaternion.Inverse(box.rotation) * (levels.Ball.Body.position - box.position);
        }

        private Vector3 CagePendulumAnchor(PhysicalHinge hinge)
        {
            Rigidbody box = levels.Current.GetComponent<Rigidbody>();
            return box.position + box.rotation * hinge.Joint.connectedAnchor;
        }

        private void CagePendulumMoveToExit(Vector2 goal)
        {
            for (int tick = 0; tick < 3000 && !levels.Current.Exit.HasExited; tick++)
            {
                Rigidbody box = levels.Current.GetComponent<Rigidbody>(); Vector3 p = CagePendulumLocal();
                Vector3 v = Quaternion.Inverse(box.rotation) * (levels.Ball.Body.linearVelocity - box.GetPointVelocity(levels.Ball.Body.position));
                Vector2 acceleration = Vector2.ClampMagnitude((goal - new Vector2(p.x, p.z)) * 8 - new Vector2(v.x, v.z) * 5, 1);
                levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(acceleration.x, -9.81f, acceleration.y), Vector3.down));
                Steps(1);
            }
            Assert.That(levels.Current.Exit.HasExited, Is.True, $"Exit route blocked at {CagePendulumLocal():F4}.");
        }
    }
}
#endif
