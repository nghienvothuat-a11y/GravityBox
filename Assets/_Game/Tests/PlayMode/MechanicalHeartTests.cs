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
        public void Heart_UnpressedReleaseCannotRetainBridgeAndResetRestoresBothBallsAndProps()
        {
            Load(22);var heart=levels.Current.GetComponent<MechanicalHeart>();
            Assert.That(levels.Balls.Count,Is.EqualTo(2));Assert.That(levels.Current.Props.Length,Is.EqualTo(3));
            Assert.That(heart.BridgeCatch.Armed,Is.False);
            SetBridgeBalanceTilt(new Vector2(-7,0));Steps(480);
            Assert.That(heart.Released,Is.False);Assert.That(heart.BridgeCatch.Latched,Is.False);
            levels.ResetLevel();
            Assert.That(heart.Released || heart.BridgeCatch.Latched || heart.BridgeCatch.Armed,Is.False);
            Assert.That(levels.Current.Exit.EscapedCount,Is.Zero);
            foreach(var ball in levels.Balls)Assert.That(ball.Body.linearVelocity,Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Heart_FromBothSpawnsSecuresBridgeTransfersCageAndEscapes()
        {
            Load(22);var heart=levels.Current.GetComponent<MechanicalHeart>();
            var a=levels.Balls[0];var b=levels.Balls[1];
            heartInsideSeen=new bool[2];heartMouthSeen=new bool[2];heartAirSeen=new bool[2];heartMouthCentreSeen=new bool[2];
            heartPreviousCagePoint=new Vector3[2];heartMouthClearTick=new int[2];heartTick=0;
            // Only shared rotation intent is commanded after Load.
            for(int t=0;t<2400;t++)
            {
                HeartHoldFarArm(heart.BridgeCatch.Hinge,a);HeartStep(heart);
                if(heart.BridgeCatch.Hinge.Angle>24.3f&&HeartCarrierPoint(heart,a).x<-.197f&&MechanicalVelocity(a).magnitude<.04f)break;
            }
            TestContext.WriteLine($"Heart lift A={MechanicalLocal(a):F4},B={MechanicalLocal(b):F4},angle={heart.BridgeCatch.Hinge.Angle:F2}");
            Assert.That(heart.BridgeCatch.Hinge.Angle,Is.GreaterThan(24));
            Assert.That(HeartCarrierPoint(heart,a).x,Is.LessThan(-.197f),"A must actually settle into the far recess before handing control to B.");
            SetBridgeBalanceTilt(new Vector2(7,0));
            for(int t=0;t<1200 && MechanicalLocal(b).x<.175f;t++)HeartStep(heart);
            Assert.That(MechanicalLocal(a).x,Is.LessThan(-.14f),"The recessed pocket must keep A on the long arm while B climbs out.");
            HeartMoveUpper(heart,b,new Vector2(.178f,.010f));
            HeartMoveUpper(heart,b,new Vector2(.245f,.010f));
            HeartMoveUpper(heart,b,new Vector2(.245f,.055f));
            SetBridgeBalanceTilt(new Vector2(0,1.5f));
            for(int t=0;t<900&&!heart.BridgeCatch.Latched;t++)HeartStep(heart);
            Assert.That(heart.Released,Is.True,$"B failed to press: {MechanicalLocal(b):F4}, travel={heart.ReleasePlunger.Guide.Displacement:F4}");
            Assert.That(heart.BridgeCatch.Latched,Is.True);
            Assert.That(heart.ReleaseOperator,Is.SameAs(b));
            // Retrieving A requires tipping far enough to climb the pocket lip; B is sheltered by its release cup.
            SetBridgeBalanceTilt(new Vector2(-25,0));
            for(int t=0;t<1200 && MechanicalLocal(a).x>-.267f;t++)HeartStep(heart);
            Assert.That(MechanicalLocal(a).x,Is.LessThan(-.267f),$"A could not climb out after the pawl engaged: {MechanicalLocal(a):F4}");
            levels.Current.Rotation.SetTargetOrientation(Quaternion.identity);
            for(int t=0;t<180;t++)HeartStep(heart);
            HeartMoveUpper(heart,a,new Vector2(-.283f,-.184f));
            HeartMoveUpper(heart,a,new Vector2(0,-.184f),true);
            HeartMoveUpper(heart,b,new Vector2(.235f,-.184f));
            HeartMoveUpper(heart,b,new Vector2(0,-.184f),true);
            for(int t=0;t<300;t++)HeartStep(heart);
            TestContext.WriteLine($"Heart cage loaded A={MechanicalLocal(a):F4}, B={MechanicalLocal(b):F4}, angle={heart.Cage.Angle:F2}");
            // A moderate stop-driven release aims both unsupported drops into the physical catcher.
            levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(35,0,0));
            for(int t=0;t<1200&&(!heartMouthSeen[0]||!heartMouthSeen[1]);t++)HeartStep(heart);
            levels.Current.Rotation.SetTargetOrientation(Quaternion.identity);
            for(int t=0;t<240;t++)HeartStep(heart);
            for(int t=0;t<3600&&!levels.Current.Exit.HasExited;t++)
            {
                var ball=levels.Current.Exit.HasBallExited(a)?b:a;
                Vector3 p=MechanicalLocal(ball),v=MechanicalVelocity(ball);
                SetBridgeBalanceTilt(Vector2.ClampMagnitude(-new Vector2(p.x,p.z)*8-new Vector2(v.x,v.z)*5,1));
                HeartStep(heart);
            }
            TestContext.WriteLine($"Heart final A={MechanicalLocal(a):F4},B={MechanicalLocal(b):F4}; cage={heartInsideSeen[0]}/{heartInsideSeen[1]}, mouth={heartMouthSeen[0]}/{heartMouthSeen[1]}, flight={heartAirSeen[0]}/{heartAirSeen[1]}");
            Assert.That(heartInsideSeen,Is.All.True,"Both balls must enter the suspended cage on the intended route.");
            Assert.That(heartMouthSeen,Is.All.True,"Both balls must leave through its side mouth into unsupported flight.");
            Assert.That(heartAirSeen,Is.All.True,"Each ball must have a measured unsupported interval between cage clearance and the catcher.");
            Assert.That(levels.Current.Exit.HasExited,Is.True,"Both steel balls must physically escape the sphere.");
        }

        private bool[] heartInsideSeen,heartMouthSeen,heartAirSeen,heartMouthCentreSeen;
        private Vector3[] heartPreviousCagePoint;
        private int[] heartMouthClearTick;
        private int heartTick;
        private readonly Collider[] heartContactBuffer=new Collider[32];
        private Vector3 HeartCarrierPoint(MechanicalHeart heart,BallController ball)
            => heart.BridgeCatch.Hinge.transform.Find("Balanced carrier frame").InverseTransformPoint(ball.Body.position);
        private void HeartHoldFarArm(PhysicalHinge hinge,BallController ball)
        {
            Transform carrier=hinge.transform.Find("Balanced carrier frame");
            Vector3 point=carrier.InverseTransformPoint(ball.Body.position);
            Vector3 velocity=carrier.InverseTransformDirection(ball.Body.linearVelocity-hinge.Body.GetPointVelocity(ball.Body.position));
            float acceleration=Mathf.Clamp((-.205f-point.x)*10-velocity.x*6,-1.5f,1.5f);
            Quaternion relative=Quaternion.Inverse(levels.Current.GetComponent<Rigidbody>().rotation)*carrier.rotation;
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(relative*new Vector3(acceleration,-9.81f,0),Vector3.down));
        }
        private void HeartStep(MechanicalHeart heart)
        {
            Steps(1);heartTick++;
            for(int i=0;i<2;i++)
            {
                BallController ball=levels.Balls[i];
                Vector3 p=Quaternion.Inverse(heart.Cage.Body.rotation)*(ball.Body.position-heart.Cage.Body.position),previous=heartPreviousCagePoint[i];
                if(!heartInsideSeen[i]&&Mathf.Abs(p.x)<.075f&&p.y>-.035f&&p.y<.032f&&Mathf.Abs(p.z)<.092f)
                {heartInsideSeen[i]=true;TestContext.WriteLine($"Heart ball{i+1} entered cage at {p:F5}; t={heartTick*Dt:F3}.");}
                if(heartInsideSeen[i]&&!heartMouthSeen[i]&&previous.z<=.113f&&p.z>.113f)
                {
                    Vector3 crossing=Vector3.Lerp(previous,p,(.113f-previous.z)/(p.z-previous.z));
                    TestContext.WriteLine($"Heart ball{i+1} crossed mouth plane {crossing:F5}, next={p:F5}; t={heartTick*Dt:F3}.");
                    // Measure the actual mouth plane before the rolling sphere drops below its floor edge.
                    if(Mathf.Abs(crossing.x)<=.020f&&crossing.y>=-.034f&&crossing.y<=.034f)heartMouthCentreSeen[i]=true;
                }
                if(p.z<.10f&&!heartMouthSeen[i])heartMouthCentreSeen[i]=false;
                if(heartMouthCentreSeen[i]&&!heartMouthSeen[i]&&p.z>.129f)
                {heartMouthSeen[i]=true;heartMouthClearTick[i]=heartTick;TestContext.WriteLine($"Heart ball{i+1} wholly cleared mouth at {p:F5}; t={heartTick*Dt:F3}.");}
                if(heartInsideSeen[i]&&!heartMouthSeen[i]&&previous.y>=-.065f&&p.y<-.065f)
                    TestContext.WriteLine($"Heart ball{i+1} below cage floor at {p:F5}, prior={previous:F5}; t={heartTick*Dt:F3}.");
                if(heartMouthSeen[i]&&!heartAirSeen[i]&&heartTick-heartMouthClearTick[i]<60&&!levels.Current.Exit.HasBallExited(ball))
                {
                    int count=UnityEngine.Physics.OverlapSphereNonAlloc(ball.Body.position,ball.Profile.Radius+.0005f,heartContactBuffer,~0,QueryTriggerInteraction.Ignore);
                    bool touching=false;
                    for(int c=0;c<count;c++)if(heartContactBuffer[c].attachedRigidbody!=ball.Body){touching=true;break;}
                    if(!touching){heartAirSeen[i]=true;TestContext.WriteLine($"Heart ball{i+1} unsupported after mouth, local={MechanicalLocal(ball):F5}; t={heartTick*Dt:F3}.");}
                }
                heartPreviousCagePoint[i]=p;
            }
        }
        private void HeartMoveUpper(MechanicalHeart heart,BallController ball,Vector2 goal,bool bore=false)
        {
            if(MechanicalLocal(ball).y<-.185f)return;
            for(int tick=0;tick<3000;tick++)
            {
                Vector3 p=MechanicalLocal(ball),v=MechanicalVelocity(ball);
                if(bore&&p.y<-.185f)return;
                Vector2 error=goal-new Vector2(p.x,p.z);
                SetBridgeBalanceTilt(Vector2.ClampMagnitude(error*9-new Vector2(v.x,v.z)*5,1.3f));HeartStep(heart);
                if(!bore&&error.magnitude<.007f&&v.magnitude<.06f)return;
            }
            Assert.Fail($"Heart upper route blocked: {ball.name} to {goal}, at {MechanicalLocal(ball):F4}.");
        }
    }
}
#endif
