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
        public void Bridge_GravityFoldsIntoAContactLatchAndResetReleasesIt()
        {
            Load(16); var hinge = FindMechanicalHinge(); var latch = hinge.GetComponent<ContactSeatLatch>();
            // Isolate the mechanism: the ball stays in its authored safe chamber.
            levels.Ball.Capture(levels.Ball.Body.position);
            Assert.That(hinge.Joint.useMotor || hinge.Joint.useSpring,Is.False);
            SetBridgeBalanceTilt(new Vector2(4,0));
            for(int tick=0;tick<720&&!latch.Latched;tick++)Steps(1);
            Assert.That(latch.Latched,Is.True,$"Bridge did not reach its receiver; angle={hinge.Angle:F2}");
            Assert.That(hinge.Angle,Is.EqualTo(-90).Within(4));
            float seated = hinge.Angle;
            SetBridgeBalanceTilt(new Vector2(-4,0));Steps(360);
            Assert.That(hinge.Angle,Is.EqualTo(seated).Within(1),"A seated pawl retains the physical bridge against reverse gravity.");
            levels.ResetLevel();
            Assert.That(latch.Latched,Is.False);
            Assert.That(hinge.Joint.limits.max-hinge.Joint.limits.min,Is.GreaterThan(85));
        }

        [Test]
        public void Bridge_FromSpawnParksFoldsCrossesAndEscapesUsingRotationOnly()
        {
            Load(16);var latch=FindMechanicalHinge().GetComponent<ContactSeatLatch>();
            MechanicalMove(levels.Ball,new Vector2(-.215f,.080f));
            SetBridgeBalanceTilt(new Vector2(4,0));
            for(int tick=0;tick<720&&!latch.Latched;tick++)Steps(1);
            Assert.That(latch.Latched,Is.True,"The parked ball leaves the bridge free to fall into its pawl.");
            Assert.That(MechanicalLocal(levels.Ball).x,Is.LessThan(-.16f),"The parking cheek holds the ball while the bridge folds.");
            foreach(Vector2 goal in new[]{new Vector2(-.215f,-.07f),new Vector2(-.075f,-.07f),
                new Vector2(-.075f,0),new Vector2(.17f,0)})MechanicalMove(levels.Ball,goal);
            MechanicalMove(levels.Ball,new Vector2(.245f,0),true);
            Assert.That(levels.Current.Exit.HasExited,Is.True);
        }

        [Test]
        public void Balance_EqualSteelMassesCreateDifferentMomentsAndRetainTheLift()
        {
            Load(17);var hinge=FindMechanicalHinge();var latch=hinge.GetComponent<ContactSeatLatch>();
            Assert.That(levels.Balls.Count,Is.EqualTo(2));
            Assert.That(levels.Balls[0].Body.mass,Is.EqualTo(levels.Balls[1].Body.mass).Within(.000001f));
            Assert.That(hinge.Joint.useMotor || hinge.Joint.useSpring,Is.False);
            Steps(240);Assert.That(latch.Latched,Is.False);
            Assert.That(hinge.Angle,Is.LessThan(5),"B's longer initial arm holds its end down.");
            // Component experiment, not a claimed gameplay solution: relocate A
            // to the far arm, then let only gravity and contacts carry the load.
            var a=levels.Balls[0];var carrier=hinge.transform.Find("Balanced carrier frame");
            a.Body.position=carrier.TransformPoint(new Vector3(-.170f,.020f,-.055f));
            a.Body.linearVelocity=Vector3.zero;a.Body.angularVelocity=Vector3.zero;UnityEngine.Physics.SyncTransforms();
            for(int tick=0;tick<1200&&!latch.Latched;tick++){BalanceHoldFarArm(hinge,a);Steps(1);}
            Assert.That(latch.Latched,Is.True,$"Far-arm load failed to lift B; angle={hinge.Angle:F2}");
            Assert.That(hinge.Angle,Is.EqualTo(25).Within(4));
            float seated=hinge.Angle;SetBridgeBalanceTilt(new Vector2(5.5f,0));Steps(360);
            Assert.That(hinge.Angle,Is.EqualTo(seated).Within(1));
            levels.ResetLevel();Assert.That(latch.Latched,Is.False);
            Assert.That(hinge.Joint.limits.max-hinge.Joint.limits.min,Is.GreaterThan(20));
        }

        [Test]
        public void Balance_FromBothSpawnsLiftsPartnerAndBothEscapeUsingRotationOnly()
        {
            Load(17);var latch=FindMechanicalHinge().GetComponent<ContactSeatLatch>();
            var a=levels.Balls[0];var b=levels.Balls[1];
            var hinge=FindMechanicalHinge();
            for(int tick=0;tick<2400&&!latch.Latched;tick++){BalanceHoldFarArm(hinge,a);Steps(1);}
            Assert.That(latch.Latched,Is.True,$"A={MechanicalLocal(a):F4}; B={MechanicalLocal(b):F4}; angle={FindMechanicalHinge().Angle:F2}");
            MechanicalMove(a,new Vector2(-.283f,-.055f));
            MechanicalMove(a,new Vector2(-.283f,-.184f));
            MechanicalMove(a,new Vector2(0,-.184f),true);
            Assert.That(levels.Current.Exit.HasBallExited(a),Is.True);
            for(int tick=0;tick<900&&!levels.Current.Exit.HasBallExited(b)&&MechanicalLocal(b).x<.20f;tick++)
            {SetBridgeBalanceTilt(new Vector2(7,0));Steps(1);}
            MechanicalMove(b,new Vector2(.235f,.055f));
            MechanicalMove(b,new Vector2(.235f,-.184f));
            MechanicalMove(b,new Vector2(0,-.184f),true);
            Assert.That(levels.Current.Exit.HasExited,Is.True);
        }

        [Test]
        public void BridgeBalance_SeatedPartsRemainStableThroughFullPlayerInversions()
        {
            foreach(int index in new[]{16,17})
            {
                Load(index);var hinge=FindMechanicalHinge();var latch=hinge.GetComponent<ContactSeatLatch>();
                if(index==16)MechanicalMove(levels.Ball,new Vector2(-.215f,.080f));
                for(int tick=0;tick<2400&&!latch.Latched;tick++)
                {
                    if(index==16)SetBridgeBalanceTilt(new Vector2(4,0));
                    else BalanceHoldFarArm(hinge,levels.Balls[0]);
                    Steps(1);
                }
                Assert.That(latch.Latched,Is.True);
                float seated=hinge.Angle;
                foreach(Vector3 angles in new[]{new Vector3(70,0,20),new Vector3(180,45,0),new Vector3(-60,-70,100),Vector3.zero})
                {
                    levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(angles));Steps(360);
                    Assert.That(float.IsNaN(hinge.Angle),Is.False);
                    Assert.That(Mathf.Abs(Mathf.DeltaAngle(hinge.Angle,seated)),Is.LessThan(1));
                    foreach(var ball in levels.Balls)
                        if(!levels.Current.Exit.HasBallExited(ball))Assert.That(levels.Current.IsOutside(ball.Body.position),Is.False);
                }
                levels.ResetLevel();Assert.That(latch.Latched,Is.False);
            }
        }

        private PhysicalHinge FindMechanicalHinge()
        {
            foreach(PhysicalProp prop in levels.Current.Props)
            {var hinge=prop.GetComponent<PhysicalHinge>();if(hinge!=null)return hinge;}
            Assert.Fail("The authored mechanism must retain its physical hinge after detachment.");return null;
        }
        private Vector3 MechanicalLocal(BallController ball)
        {var box=levels.Current.GetComponent<Rigidbody>();return Quaternion.Inverse(box.rotation)*(ball.Body.position-box.position);}
        private Vector3 MechanicalVelocity(BallController ball)
        {var box=levels.Current.GetComponent<Rigidbody>();return Quaternion.Inverse(box.rotation)*(ball.Body.linearVelocity-box.GetPointVelocity(ball.Body.position));}
        private void SetBridgeBalanceTilt(Vector2 acceleration) => levels.Current.Rotation.SetTargetOrientation(
            Quaternion.FromToRotation(new Vector3(acceleration.x,-9.81f,acceleration.y),Vector3.down));
        private void BalanceHoldFarArm(PhysicalHinge hinge,BallController ball)
        {
            Transform carrier=hinge.transform.Find("Balanced carrier frame");
            Vector3 point=carrier.InverseTransformPoint(ball.Body.position);
            Vector3 velocity=carrier.InverseTransformDirection(ball.Body.linearVelocity-hinge.Body.GetPointVelocity(ball.Body.position));
            float acceleration=Mathf.Clamp((-.180f-point.x)*10-velocity.x*6,-1.5f,1.5f);
            Quaternion relative=Quaternion.Inverse(levels.Current.GetComponent<Rigidbody>().rotation)*carrier.rotation;
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(relative*new Vector3(acceleration,-9.81f,0),Vector3.down));
        }
        private void MechanicalMove(BallController ball,Vector2 goal,bool exit=false)
        {
            if(levels.Current.Exit.HasBallExited(ball))return;
            for(int tick=0;tick<2400;tick++)
            {
                Vector3 p=MechanicalLocal(ball),v=MechanicalVelocity(ball);
                Vector2 error=goal-new Vector2(p.x,p.z);
                SetBridgeBalanceTilt(Vector2.ClampMagnitude(error*9-new Vector2(v.x,v.z)*5,1.3f));Steps(1);
                if(levels.Current.Exit.HasBallExited(ball)||(!exit&&error.magnitude<.007f&&v.magnitude<.06f))return;
            }
            Assert.Fail($"Mechanical route blocked: {ball.name} to {goal}, at {MechanicalLocal(ball):F4}");
        }
    }
}
#endif
