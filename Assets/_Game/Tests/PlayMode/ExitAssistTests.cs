#if UNITY_EDITOR
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed partial class PhysicsLifecycleTests
    {
        [TestCase(0)] [TestCase(90)] [TestCase(180)]
        public void ExitAssist_AllCatalogLevelsPullThroughTheRealHoleAtDifferentOrientations(float angle)
        {
            for (int index = 0; index < levels.Catalog.Levels.Length; index++)
            {
                Load(index);
                Quaternion pose = Quaternion.Euler(0, 0, angle);
                levels.Current.GetComponent<Rigidbody>().rotation = pose;
                levels.Current.transform.rotation = pose;
                levels.Current.Rotation.SetTargetOrientation(pose);
                // This fixture changes pose instantaneously: move the detached bodies too.
                // Gameplay instead rotates through the controller's bounded motion.
                foreach (var prop in levels.Current.Props)
                { prop.Body.position = pose * prop.Body.position; prop.Body.rotation = pose * prop.Body.rotation; }
                foreach (var target in levels.Balls) target.Body.position = pose * target.Body.position;
                UnityEngine.Physics.SyncTransforms();
                levels.Current.GetComponent<WaterVolume>()?.ResetState();
                var exit = levels.Current.Exit;
                foreach (var ball in levels.Balls)
                {
                // Each aperture fixture starts after the previous sphere has physically left the bore.
                if (exit.EscapedCount > 0) Steps(60);
                ball.Body.position = exit.transform.TransformPoint(new Vector3(0,0,-exit.WallHalfDepth-ball.Profile.Radius*1.2f));
                ball.Body.linearVelocity = ball.Body.angularVelocity = Vector3.zero; UnityEngine.Physics.SyncTransforms(); exit.BeginTracking();
                int exits = 0; exit.BallExited += target => { if (target == ball) exits++; };
                Steps(1);
                // A departing partner is a real collider: assistance waits for a clear bore.
                for (int wait = 0; wait < 120 && !exit.AssistActive && !exit.HasBallExited(ball); wait++) Steps(1);
                Assert.That(levels.Current.Exit.AssistActive, Is.True, levels.Definition.Id);
                Assert.That(exit.HasBallExited(ball), Is.False, "Entering assistance is not a win.");
                int ticks = 1;
                while (ticks < 360 && !exit.HasBallExited(ball)) { Steps(1); ticks++; }
                Assert.That(exit.HasBallExited(ball), Is.True, levels.Definition.Id + " at " + angle
                    + " local=" + levels.Current.Exit.transform.InverseTransformPoint(ball.Body.position)
                    + " assist=" + levels.Current.Exit.AssistActive);
                Assert.That(exits, Is.EqualTo(1)); Assert.That(ball.Body.isKinematic, Is.False);
                float depth = levels.Current.Exit.transform.InverseTransformPoint(ball.Body.position).z;
                Assert.That(depth, Is.GreaterThanOrEqualTo(levels.Current.Exit.WallHalfDepth + Radius));
                Steps(1);
                Assert.That(levels.Current.Exit.AssistActive, Is.False);
                Assert.That(levels.Current.Exit.AssistAcceleration, Is.EqualTo(Vector3.zero));
                TestContext.WriteLine($"Assist {index+1:00}, box Z={angle}: fully out in {ticks*Dt:F3}s.");
                }
                Assert.That(exit.HasExited, Is.True);
            }
        }

        [TestCase(1)] [TestCase(12)] [TestCase(13)]
        public void ExitAssist_CentresAnOffsetBallBeforePullingThrough(int index)
        {
            Load(index);
            var pose = Quaternion.Euler(0, 0, 180);
            levels.Current.GetComponent<Rigidbody>().rotation = pose;
            levels.Current.transform.rotation = pose;
            levels.Current.Rotation.SetTargetOrientation(pose);
            UnityEngine.Physics.SyncTransforms();
            levels.Current.GetComponent<WaterVolume>()?.ResetState();
            BeginExitApproach(new Vector2(.029f, 0)); levels.Ball.Body.linearVelocity = Vector3.zero;
            Steps(1); Assert.That(levels.Current.Exit.AssistActive, Is.True);
            for (int tick = 0; tick < 360 && !levels.Current.Exit.HasExited; tick++) Steps(1);
            Assert.That(levels.Current.Exit.HasExited, Is.True);
            Assert.That(levels.Ball.Body.isKinematic, Is.False);
        }

        [Test]
        public void ExitAssist_MercuryRestingAtUpwardMouthLeavesWithoutAnotherTilt()
        {
            Load(13); var exit = levels.Current.Exit; var liquid = levels.Current.GetComponent<WaterVolume>();
            Quaternion pose = Quaternion.Euler(0, 0, 180);
            levels.Current.GetComponent<Rigidbody>().rotation = pose;
            levels.Current.Rotation.SetTargetOrientation(pose); liquid.ResetState();
            exit.AssistEnabled = false;
            levels.Ball.Body.position = pose * new Vector3(.105f, -.0404f, -.105f);
            levels.Ball.Body.linearVelocity = Vector3.zero;
            UnityEngine.Physics.SyncTransforms(); exit.BeginTracking(); Steps(360);
            Assert.That(exit.HasExited, Is.False);
            Assert.That(liquid.SubmergedFraction, Is.EqualTo(7850f/13546).Within(.001f));
            exit.AssistEnabled = true;
            for (int tick = 0; tick < 360 && !exit.HasExited; tick++) Steps(1);
            Assert.That(exit.HasExited, Is.True); Assert.That(levels.Ball.Body.isKinematic, Is.False);
            Assert.That(Quaternion.Angle(pose, levels.Current.Rotation.Orientation), Is.LessThan(.01f));
            Steps(60); Assert.That(liquid.SubmergedFraction, Is.Zero, "Departure must not drop straight back into mercury.");
        }

        [Test]
        public void ExitAssist_RejectsFarOutsideLockedAndBlockedApproachesAndClearsOnReset()
        {
            Load(1); var exit = levels.Current.Exit;
            PlaceAtExit(new Vector3(0, 0, -exit.AssistRadius - .001f), true);
            exit.PrepareStep(Dt); Assert.That(exit.AssistActive, Is.False);
            PlaceAtExit(new Vector3(0, 0, exit.WallHalfDepth + Radius), true);
            exit.PrepareStep(Dt); Assert.That(exit.AssistActive, Is.False);
            BeginExitApproach(); exit.RequiredChannel = "closed-test";
            exit.PrepareStep(Dt); Assert.That(exit.AssistActive, Is.False);
            exit.RequiredChannel = null;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                blocker.transform.SetParent(exit.transform, false);
                blocker.transform.localPosition = Vector3.zero;
                blocker.transform.localScale = new Vector3(.08f, .08f, .006f);
                UnityEngine.Physics.SyncTransforms();
                exit.PrepareStep(Dt); Assert.That(exit.AssistActive, Is.False, "A real shutter must block assistance.");
            }
            finally { Object.DestroyImmediate(blocker); }
            UnityEngine.Physics.SyncTransforms();
            exit.PrepareStep(Dt); Assert.That(exit.AssistActive, Is.True);
            exit.Accepting = false; exit.PrepareStep(Dt);
            Assert.That(exit.AssistActive, Is.False); Assert.That(exit.AssistAcceleration, Is.EqualTo(Vector3.zero));
            exit.Accepting = true; exit.PrepareStep(Dt); Assert.That(exit.AssistActive, Is.True);
            levels.ResetLevel(); Assert.That(exit.AssistActive, Is.False);
            Assert.That(exit.AssistAcceleration, Is.EqualTo(Vector3.zero));
            Load(12); Assert.That(levels.Current.Exit.AssistActive, Is.False);
            Steps(1); Assert.That(levels.Current.Exit.AssistAcceleration, Is.EqualTo(Vector3.zero));
        }
    }
}
#endif
