#if UNITY_EDITOR
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed partial class PhysicsLifecycleTests
    {
        [Test]
        public void FlightCatch_RollsIntoUnforcedFreeFlightAndAMissReturnsToTheRecoveryFloor()
        {
            Load(20);
            Assert.That(levels.Current.Props,Is.Empty);
            Vector3 initialBallPosition=levels.Ball.Body.position;
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(1.2f,-9.81f,0),Vector3.down));
            int airborne=0,gravitySamples=0;float maxSpeed=0;
            for(int tick=0;tick<600;tick++)
            {
                Vector3 before=levels.Ball.Body.linearVelocity;
                Vector3 local=levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position);
                bool free=local.x>.008f && local.x<.285f && local.y>-.24f && local.y<.11f && !levels.Ball.HasContact;
                Steps(1);
                maxSpeed=Mathf.Max(maxSpeed,levels.Ball.Body.linearVelocity.magnitude);
                if(free && !levels.Ball.HasContact)
                {
                    airborne++;
                    Vector3 acceleration=(levels.Ball.Body.linearVelocity-before)/Dt;
                    if((acceleration-Vector3.down*9.81f).magnitude<.03f) gravitySamples++;
                }
                Assert.That(levels.Current.IsOutside(levels.Ball.Body.position),Is.False,"A missed catch must stay inside the recovery shell.");
            }
            Assert.That(maxSpeed,Is.GreaterThan(.5f),"The curved descent must provide observable gravitational acceleration.");
            Assert.That(airborne,Is.GreaterThanOrEqualTo(6),"The transfer must contain an actual airborne span, not a hidden supporting bridge.");
            Assert.That(gravitySamples,Is.GreaterThanOrEqualTo(4),"Free flight must use world gravity without a launcher or steering force.");
            Assert.That(levels.Current.Exit.HasExited,Is.False,"The offset receiving court must not accept a straight, stationary-box miss.");
            Assert.That(levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position).y,Is.LessThan(-.23f));
            levels.ResetLevel();
            Assert.That(Vector3.Distance(levels.Ball.Body.position,initialBallPosition),Is.LessThan(.00001f));
        }

        [Test]
        public void MemoryCam_IsAContactDrivenSlottedRotorWithAPassiveReturnAndARealClearPassage()
        {
            Load(21);
            var memory=levels.Current.GetComponent<MemoryRatchetAssembly>();
            Assert.That(memory,Is.Not.Null);
            Assert.That(levels.Current.Props,Has.Length.EqualTo(3));
            Assert.That(memory.Cam.Body.isKinematic,Is.False);
            Assert.That(memory.Rack.Body.isKinematic,Is.False);
            Assert.That(memory.DrivePawl.Body.isKinematic,Is.False);
            Assert.That(memory.Cam.Joint.useMotor,Is.False);
            Assert.That(memory.Cam.Joint.useSpring,Is.False);
            Assert.That(memory.Rack.Joint.xDrive.maximumForce,Is.Zero);
            Assert.That(memory.Rack.Travel,Is.EqualTo(.062f).Within(.00001f));
            Assert.That(memory.ReturnSpring,Is.EqualTo(12f));
            Assert.That(MemoryPassageHasCam(),Is.True,"The initial east/west slot must block north/south travel.");
            // A geometric fixture, not a solution: inspect the real rotor at its
            // final indexed pose without changing or disabling any collider.
            memory.Cam.Body.rotation=Quaternion.AngleAxis(90,Vector3.down);
            UnityEngine.Physics.SyncTransforms();
            Assert.That(MemoryPassageHasCam(),Is.False,"At 90° the complete 30 mm sphere needs a continuous open corridor.");
            foreach(Collider c in memory.Cam.GetComponentsInChildren<Collider>())
            { Assert.That(c.enabled,Is.True);Assert.That(c.isTrigger,Is.False); }
            levels.ResetLevel();
            Assert.That(MemoryPassageHasCam(),Is.True);
        }

        [Test]
        public void MemoryCam_BallContactAdvancesTeethAndRetainsProgressAfterTheRackReturns()
        {
            Load(21);
            var memory=levels.Current.GetComponent<MemoryRatchetAssembly>();
            // From the shipped spawn; no pose or velocity writes after loading.
            Assert.That(FlightMemorySteer(new Vector2(-.27f,-.100f)),Is.True);
            Assert.That(FlightMemorySteer(new Vector2(-.172f,-.100f)),Is.True);
            int previous=0;
            for(int stroke=0;stroke<3;stroke++)
            {
                levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(7,-7,0),Vector3.down));
                Steps(420);
                TestContext.WriteLine($"MEMORY stroke {stroke}: rack {memory.Rack.Displacement:F4}, cam {memory.Cam.Angle:F2}, native {memory.Cam.Joint.angle:F2}, rotation {(Quaternion.Inverse(levels.Current.GetComponent<Rigidbody>().rotation)*memory.Cam.Body.rotation).eulerAngles:F2}, teeth {memory.RetainedTeeth}, finger {memory.DrivePawl.Angle:F2}, ball {levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position):F4}.");
                if(memory.RetainedTeeth<=previous)
                    foreach(PhysicalProp prop in levels.Current.Props)
                    foreach(Collider moving in prop.GetComponentsInChildren<Collider>())
                    foreach(Collider solid in levels.Current.GetComponentsInChildren<Collider>())
                        if(UnityEngine.Physics.ComputePenetration(moving,moving.transform.position,moving.transform.rotation,
                            solid,solid.transform.position,solid.transform.rotation,out Vector3 normal,out float depth) && depth>.0001f)
                            TestContext.WriteLine($"MEMORY contact {moving.name} / {solid.name}: {depth:F6} m, normal {normal:F3}.");
                Assert.That(memory.RetainedTeeth,Is.EqualTo(previous+1),"The escapement must admit exactly one physical tooth per ball-driven stroke.");
                previous=memory.RetainedTeeth;
                levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(-5,-8,0),Vector3.down));
                Steps(360);
                Assert.That(memory.RetainedTeeth,Is.EqualTo(previous));
                Assert.That(memory.Cam.Angle,Is.GreaterThan(previous*30-1.8f),"The cam must retain its actual angle after the ball leaves the rack.");
                Assert.That(memory.Rack.Displacement,Is.LessThan(.010f),"The return spring and folding drive finger must reset the rack for another stroke.");
            }
            Assert.That(memory.PassageAligned,Is.True);
            Assert.That(MemoryPassageHasCam(),Is.False);
            Assert.That(FlightMemorySteer(new Vector2(-.24f,-.20f)),Is.True);
            Assert.That(FlightMemorySteer(new Vector2(0,-.20f)),Is.True);
            Assert.That(FlightMemorySteer(new Vector2(0,.18f)),Is.True);
            Assert.That(FlightMemorySteer(new Vector2(.22f,.25f),true),Is.True);
            Assert.That(levels.Current.Exit.HasExited,Is.True,"The ball that operated the mechanism must also leave through the actual round bore.");
            levels.ResetLevel();
            Assert.That(memory.RetainedTeeth,Is.Zero);
            Assert.That(Mathf.Abs(memory.Rack.Displacement),Is.LessThan(.0001f));
            Assert.That(Mathf.Abs(memory.Cam.Angle),Is.LessThan(.05f));
        }

        [Test]
        public void FlightCatch_AFromSpawnRotationPolicyCanCatchAndExitTheRequiredBall()
        {
            Load(20);
            // A bounded search among explicit hand motions. Each attempt begins
            // at the authored spawn and obeys the shipped rotation controller.
            foreach(float triggerX in new[]{-.20f,-.14f,-.08f})
            foreach(float yaw in new[]{60f,90f,-60f})
            {
                levels.ResetLevel();bool turn=false,caught=false;
                Quaternion launch=Quaternion.FromToRotation(new Vector3(1.2f,-9.81f,0),Vector3.down);
                for(int tick=0;tick<420;tick++)
                {
                    Vector3 p=levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position);
                    if(p.x>triggerX) turn=true;
                    levels.Current.Rotation.SetTargetOrientation((turn?Quaternion.Euler(0,yaw,0):Quaternion.identity)*launch);
                    Steps(1);
                    p=levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position);
                    if(p.y<-.075f && p.x>.092f && p.x<.288f && p.z>.006f && p.z<.187f)
                    { caught=true;break; }
                    if(p.y<-.250f) break;
                }
                if(!caught) continue;
                if(FlightMemorySteer(new Vector2(.225f,.120f),true, false,yaw))
                {
                    TestContext.WriteLine($"FLIGHT verified from-spawn solution: begin yaw {yaw:F0}° at local x>{triggerX:F3} m; gravity-fed flight, receiver then physical exit.");
                    Assert.That(levels.Ball.Body.isKinematic,Is.False);return;
                }
            }
            Assert.Fail("No authored hand-motion candidate reached the receiver and exited; tune the catch geometry/control window before shipping.");
        }

        [Test]
        public void FlightCatch_AMissedBallCanReturnToTheLaunchCupViaTheClearCover()
        {
            Load(20);
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(1.2f,-9.81f,0),Vector3.down));
            Steps(480);
            Assert.That(levels.Current.Exit.HasExited,Is.False);
            levels.Current.Rotation.SetTargetOrientation(Quaternion.AngleAxis(180,Vector3.forward));
            Steps(360);
            Assert.That(FlightMemorySteer(new Vector2(0,.18f),false,true),Is.True);
            Steps(90);
            Assert.That(FlightMemorySteer(new Vector2(0,-.075f),false,true),Is.True);
            Assert.That(FlightMemorySteer(new Vector2(-.301f,-.075f),false,true),Is.True);
            TestContext.WriteLine($"FLIGHT recovery over cup {levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position):F5}.");
            // Choose the return half-turn explicitly: through +90° the local
            // downhill direction holds the ball against the pocket's back wall.
            levels.Current.Rotation.SetTargetOrientation(Quaternion.AngleAxis(90,Vector3.forward));
            Steps(240);
            TestContext.WriteLine($"FLIGHT recovery against back at Z90 {levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position):F5}.");
            // Keep a small backwards slope: returning exactly level leaves the
            // recovered ball free to roll into the takeoff ramp again.
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(-1.2f,-9.81f,0),Vector3.down));
            Steps(360);
            Vector3 recovered=levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position);
            TestContext.WriteLine($"FLIGHT recovered from a real miss via the cover: {recovered:F5}.");
            Assert.That(recovered.x,Is.InRange(-.319f,-.276f));
            Assert.That(recovered.y,Is.InRange(.223f,.247f));
            Assert.That(recovered.z,Is.InRange(-.090f,-.060f));
        }

        private bool FlightMemorySteer(Vector2 goal,bool requireExit=false,bool inverted=false,float yaw=0)
        {
            Rigidbody box=levels.Current.GetComponent<Rigidbody>();
            for(int tick=0;tick<1500;tick++)
            {
                Transform root=levels.Current.transform;
                Vector3 p=root.InverseTransformPoint(levels.Ball.Body.position);
                Vector3 velocity=root.InverseTransformDirection(levels.Ball.Body.linearVelocity-box.GetPointVelocity(levels.Ball.Body.position));
                Vector2 e=goal-new Vector2(p.x,p.z),v=new Vector2(velocity.x,velocity.z);
                Vector2 acceleration=Vector2.ClampMagnitude(e*8-v*5,1.2f);
                Vector3 g=new Vector3(acceleration.x,inverted?9.81f:-9.81f,acceleration.y);
                Quaternion reference=inverted?Quaternion.AngleAxis(180,Vector3.forward):Quaternion.Euler(0,yaw,0);
                levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(reference*g,Vector3.down)*reference);
                Steps(1);
                if(requireExit && levels.Current.Exit.HasExited) return true;
                if(!requireExit && e.magnitude<.014f && v.magnitude<.10f) return true;
                if(levels.Current.IsOutside(levels.Ball.Body.position)) return false;
            }
            TestContext.WriteLine($"FLIGHT/MEMORY unreachable waypoint {goal:F4}; local {levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position):F5}.");
            return false;
        }

        private bool MemoryPassageHasCam()
        {
            var memory=levels.Current.GetComponent<MemoryRatchetAssembly>();
            Transform root=levels.Current.transform;
            Vector3 from=root.TransformPoint(new Vector3(0,-.038f,-.100f));
            foreach(RaycastHit hit in UnityEngine.Physics.SphereCastAll(from,Radius,root.forward,.300f,~0,QueryTriggerInteraction.Ignore))
                if(hit.rigidbody==memory.Cam.Body) return true;
            return false;
        }
    }
}
#endif
