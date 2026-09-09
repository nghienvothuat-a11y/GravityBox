#if UNITY_EDITOR
using System.Collections.Generic;
using GravityBox.Foundation;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed partial class PhysicsLifecycleTests
    {
        [TestCase(0)] [TestCase(1)]
        public void Cooperative_FirstEscapeKeepsPartnerPlayableAndResetRestoresBoth(int first)
        {
            Load(15); var exit=levels.Current.Exit;
            Assert.That(levels.Balls.Count,Is.EqualTo(2)); Assert.That(forces.TargetCount,Is.EqualTo(6));
            var spawns=new[]{levels.Balls[0].Body.position,levels.Balls[1].Body.position};
            int wins=0;exit.Exited+=()=>wins++;
            foreach(int index in new[]{first,1-first})
            {
                BallController ball=levels.Balls[index];
                ball.Body.position=exit.transform.TransformPoint(new Vector3(.01f,0,-.023f));ball.Body.linearVelocity=Vector3.zero;
                UnityEngine.Physics.SyncTransforms();exit.BeginTracking();
                for(int t=0;t<240&&!exit.HasBallExited(ball);t++)Steps(1);
                Assert.That(exit.HasBallExited(ball),Is.True);Assert.That(ball.Body.isKinematic,Is.False);
                Assert.That(wins,Is.EqualTo(index==first?0:1));
                Assert.That(levels.Session.State,Is.EqualTo(index==first?SessionState.Active:SessionState.Completing));
                for(int n=0;n<10;n++)exit.EvaluateTraversal();
                Assert.That(exit.EscapedCount,Is.EqualTo(index==first?1:2));
            }
            levels.ResetLevel();
            Assert.That(exit.EscapedCount,Is.Zero);Assert.That(exit.HasExited,Is.False);
            for(int i=0;i<2;i++)
            { Assert.That(levels.Balls[i].Body.position,Is.EqualTo(spawns[i]));Assert.That(levels.Balls[i].Body.linearVelocity,Is.EqualTo(Vector3.zero)); }
            Assert.That(levels.Current.GetComponent<CooperativeRelay>().Released,Is.False);
            levels.Load(0);Assert.That(levels.Balls.Count,Is.EqualTo(1));Assert.That(forces.TargetCount,Is.EqualTo(1));
        }

        [Test]
        public void Cooperative_SwitchRequiresContactAndAnUnpressedPartnerCannotCross()
        {
            Load(15);var relay=levels.Current.GetComponent<CooperativeRelay>();
            // Broad orientations cannot activate a plunger by its own weight.
            levels.Balls[0].Capture(levels.Balls[0].Body.position);levels.Balls[1].Capture(levels.Balls[1].Body.position);
            foreach(float angle in new[]{0f,70f,-70f,180f})
            {
                levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(angle,0,0));Steps(240);
                Assert.That(relay.Holding,Is.False);Assert.That(relay.Released,Is.False);
                Assert.That(relay.LeftClear,Is.False);Assert.That(relay.RightClear,Is.False);
            }
            levels.ResetLevel();levels.Balls[0].Capture(levels.Balls[0].Body.position);
            BallController b=levels.Balls[1];b.Body.position=new Vector3(.22f,-.027f,.09f);b.Body.linearVelocity=Vector3.zero;
            SetCoopTilt(new Vector2(0,-.8f));Steps(480);
            Assert.That(CooperativeLocal(b).z,Is.GreaterThan(-.005f));
            Assert.That(relay.RightClear,Is.False);Assert.That(relay.Released,Is.False);
        }

        [Test]
        public void Cooperative_BothBodiesExchangeMomentumOnContact()
        {
            Load(15);var a=levels.Balls[0];var b=levels.Balls[1];
            a.Body.position=new Vector3(-.29f,-.027f,.28f);b.Body.position=new Vector3(-.215f,-.027f,.28f);
            a.Body.linearVelocity=Vector3.right*.4f;b.Body.linearVelocity=Vector3.zero;
            levels.Current.Exit.AssistEnabled=false;UnityEngine.Physics.SyncTransforms();Steps(20);
            Assert.That(b.Body.linearVelocity.x,Is.GreaterThan(.08f));
            Assert.That(a.Body.linearVelocity.x,Is.LessThan(.3f));
            Assert.That(Vector3.Distance(a.Body.position,b.Body.position),Is.GreaterThan(.029f));
        }

        [Test]
        public void Cooperative_FromBothSpawnsUsesTwoOperatorsAndEscapesThroughOneRealHole()
        {
            Load(15);var relay=levels.Current.GetComponent<CooperativeRelay>();
            var a=levels.Balls[0];var b=levels.Balls[1];
            SetCoopTilt(new Vector2(0,-.9f));
            for(int t=0;t<600&&!relay.Holding;t++)Steps(1);
            Assert.That(relay.Holding,Is.True,"A must physically compress the holding plunger.");
            TestContext.WriteLine($"Holding: A={CooperativeLocal(a):F4}, B={CooperativeLocal(b):F4}.");
            // Keep a southward load on A while steering B sideways into its gate.
            for(int t=0;t<1800&&CooperativeLocal(b).z>-.060f;t++)
            {
                Vector3 p=CooperativeLocal(b),v=CooperativeVelocity(b);
                SetCoopTilt(new Vector2(Mathf.Clamp((.22f-p.x)*8-v.x*5,-.65f,.65f),-.9f));Steps(1);
            }
            Assert.That(CooperativeLocal(b).z,Is.LessThan(-.060f),"B must cross while A supports it.");
            Assert.That(relay.FirstOperator,Is.SameAs(a));
            CoopMove(b,new Vector2(.10f,-.09f));
            SetCoopTilt(new Vector2(0,-.9f));
            for(int t=0;t<600&&!relay.Released;t++)Steps(1);
            Assert.That(relay.Released,Is.True,$"B failed to reach release plunger: {CooperativeLocal(b):F4}");
            Assert.That(relay.SecondOperator,Is.SameAs(b));
            TestContext.WriteLine($"Released: A={CooperativeLocal(a):F4}, B={CooperativeLocal(b):F4}.");
            CoopMove(a,new Vector2(CooperativeLocal(a).x,.24f));
            foreach(Vector2 goal in new[]{new Vector2(-.282f,.24f),new Vector2(-.282f,.035f),
                new Vector2(-.20f,.035f),new Vector2(-.20f,-.09f),new Vector2(-.20f,-.278f),new Vector2(0,-.278f)}) CoopMove(a,goal);
            TestContext.WriteLine($"A out={levels.Current.Exit.HasBallExited(a)}, B={CooperativeLocal(b):F4}.");
            foreach(Vector2 goal in new[]{new Vector2(.22f,.06f),new Vector2(.22f,-.09f),new Vector2(.26f,-.09f),new Vector2(.26f,-.278f),new Vector2(0,-.278f)}) CoopMove(b,goal);
            Assert.That(levels.Current.Exit.HasExited,Is.True);Assert.That(levels.EscapedCount,Is.EqualTo(2));
            Assert.That(a.Body.isKinematic||b.Body.isKinematic,Is.False);
        }

        [Test]
        public void Cooperative_PartialEscapePauseResetAndBothGateStatesAreCleared()
        {
            Load(15); var exit = levels.Current.Exit; var a = levels.Balls[0];
            a.Body.position = exit.transform.TransformPoint(new Vector3(0,0,-.022f));
            UnityEngine.Physics.SyncTransforms(); exit.BeginTracking(); Steps(50);
            Assert.That(exit.EscapedCount, Is.EqualTo(1));
            levels.TogglePause(); Assert.That(levels.Session.State, Is.EqualTo(SessionState.Paused));
            levels.ResetLevel(); Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(exit.EscapedCount, Is.Zero); Assert.That(exit.AssistActive, Is.False);
            Steps(120); var relay = levels.Current.GetComponent<CooperativeRelay>();
            Assert.That(relay.Holding || relay.Released || relay.LeftClear || relay.RightClear, Is.False);
            foreach (BallController ball in levels.Balls)
            { Assert.That(ball.IsCaptured || ball.Body.isKinematic, Is.False); Assert.That(ball.Body.linearVelocity.magnitude, Is.LessThan(.01f)); }
        }

        [Test]
        public void MultiBall_LegacyPlateWaitsForLastOccupantAndOneWayGateKeepsSeparateClearance()
        {
            Load(15); var root = new GameObject("Multi-ball legacy mechanism fixture");
            try
            {
                var plate = root.AddComponent<PressurePlate>(); plate.Latch = false;
                plate.Bind(levels.Current.Signals, levels.Balls);
                Collider a = levels.Balls[0].GetComponent<Collider>(), b = levels.Balls[1].GetComponent<Collider>();
                plate.SendMessage("OnTriggerEnter",a); plate.SendMessage("OnTriggerEnter",b);
                plate.SendMessage("OnTriggerExit",a); Assert.That(plate.IsActive, Is.True);
                plate.SendMessage("OnTriggerExit",b); Assert.That(plate.IsActive, Is.False);
                var gate = root.AddComponent<OneWayGate>(); gate.enabled = false;
                gate.Blocker = root.AddComponent<BoxCollider>(); gate.Clearance = .05f; gate.Bind(levels.Balls);
                levels.Balls[0].Body.position = Vector3.left*.1f; levels.Balls[1].Body.position = Vector3.right*.1f;
                gate.Step(); Assert.That(UnityEngine.Physics.GetIgnoreCollision(a,gate.Blocker), Is.True);
                Assert.That(UnityEngine.Physics.GetIgnoreCollision(b,gate.Blocker), Is.False);
                gate.ResetState(); Assert.That(UnityEngine.Physics.GetIgnoreCollision(a,gate.Blocker), Is.False);
                var pad = root.AddComponent<ImpulsePad>(); pad.Bind(levels.Balls);
                Assert.That(pad.TryFire(levels.Balls[0]), Is.True); Assert.That(pad.TryFire(levels.Balls[1]), Is.True);
                Assert.That(pad.TryFire(levels.Balls[0]), Is.False);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void Cooperative_WidePlayerRotationsKeepBothBallsAndGuidedBodiesStable()
        {
            Load(15);
            foreach (Vector3 angles in new[] { new Vector3(70,0,20), new Vector3(180,45,0), new Vector3(-60,-70,100), Vector3.zero })
            {
                levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(angles));
                for (int tick=0;tick<360;tick++)
                {
                    Steps(1);
                    foreach (BallController ball in levels.Balls)
                        if (!levels.Current.Exit.HasBallExited(ball)) Assert.That(levels.Current.IsOutside(ball.Body.position), Is.False);
                    foreach (PhysicalProp prop in levels.Current.Props)
                    {
                        Assert.That(float.IsNaN(prop.Body.position.sqrMagnitude), Is.False);
                        Assert.That(prop.Body.position.magnitude, Is.LessThan(.55f), "A guided body escaped its housing.");
                    }
                }
            }
        }

        private Vector3 CooperativeLocal(BallController ball)
        {var root=levels.Current.GetComponent<Rigidbody>();return Quaternion.Inverse(root.rotation)*(ball.Body.position-root.position);}
        private Vector3 CooperativeVelocity(BallController ball)
        {var root=levels.Current.GetComponent<Rigidbody>();return Quaternion.Inverse(root.rotation)*(ball.Body.linearVelocity-root.GetPointVelocity(ball.Body.position));}
        private void SetCoopTilt(Vector2 acceleration) => levels.Current.Rotation.SetTargetOrientation(
            Quaternion.FromToRotation(new Vector3(acceleration.x,-9.81f,acceleration.y),Vector3.down));
        private void CoopMove(BallController ball,Vector2 goal)
        {
            if(levels.Current.Exit.HasBallExited(ball))return;
            for(int t=0;t<2400;t++)
            {
                Vector3 p=CooperativeLocal(ball),v=CooperativeVelocity(ball);Vector2 error=goal-new Vector2(p.x,p.z);
                SetCoopTilt(Vector2.ClampMagnitude(error*8-new Vector2(v.x,v.z)*5,1));Steps(1);
                foreach(var target in levels.Balls)
                    if(!levels.Current.Exit.HasBallExited(target))Assert.That(levels.Current.IsOutside(target.Body.position),Is.False);
                bool final=goal==new Vector2(0,-.278f);
                if(levels.Current.Exit.HasBallExited(ball)||(!final&&error.magnitude<.008f&&v.magnitude<.06f))
                {TestContext.WriteLine($"Ball {levels.Balls[0]==ball} -> {goal}: {t*Dt:F2}s, other={CooperativeLocal(levels.Balls[0]==ball?levels.Balls[1]:levels.Balls[0]):F4}");return;}
            }
            Assert.Fail($"Coop route blocked: ball {ball.name}, target {goal}, at {CooperativeLocal(ball):F4}.");
        }
    }
}
#endif
