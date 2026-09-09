#if UNITY_EDITOR
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed partial class PhysicsLifecycleTests
    {
        private void LoadBoss(int campaignNumber)
        {
            var catalog=AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/_Game/Campaign/CampaignCatalog.asset");
            Assert.That(catalog,Is.Not.Null,"Generate campaign prefabs before boss route fixtures.");
            levels.Initialize(catalog,forces,storage:new VolatileCampaignStorage());Load(campaignNumber-1);
        }

        [TestCase(-1)] [TestCase(1)]
        public void CampaignBossLion_FromSpawnEitherCheekEscapesWithSharedTilt(int side)
        {
            LoadBoss(10);var root=levels.Current.GetComponent<Rigidbody>();
            Vector2[] route={new Vector2(side*.21f,.19f),new Vector2(side*.21f,-.12f),new Vector2(side*.12f,-.22f),new Vector2(0,-.25f)};
            Steps(120);
            for(int stage=0;stage<route.Length;stage++)
            {
                bool reached=false;
                for(int tick=0;tick<2400;tick++)
                {
                    Quaternion inv=Quaternion.Inverse(root.rotation);
                    Vector3 local=inv*(levels.Ball.Body.position-root.position);
                    Vector3 v=inv*(levels.Ball.Body.linearVelocity-root.GetPointVelocity(levels.Ball.Body.position));
                    Vector2 error=route[stage]-new Vector2(local.x,local.z);
                    Vector2 tilt=Vector2.ClampMagnitude(error*8-new Vector2(v.x,v.z)*5,1);
                    levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(tilt.x,-9.81f,tilt.y),Vector3.down));
                    Steps(1);
                    if(levels.Current.Exit.HasExited){reached=true;break;}
                    if(stage<route.Length-1&&error.magnitude<.015f&&v.magnitude<.08f){reached=true;break;}
                    Assert.That(levels.Current.IsOutside(levels.Ball.Body.position),Is.False);
                }
                Assert.That(reached,Is.True,"C10 physical branch "+side+" blocked at "+stage);
            }
            Assert.That(levels.Current.Exit.HasExited,Is.True);Assert.That(levels.Ball.Body.isKinematic,Is.False);
            levels.ResetLevel();Assert.That(levels.Current.Exit.HasExited,Is.False);
        }

        [Test]
        public void CampaignBossGarden_OpposedDoorsAndBridgeWorkUnderRealTiltWithLiveBall()
        {
            LoadBoss(20);
            GravitySliderGuide first=null,second=null;ContactSeatLatch bridge=null;
            foreach(var prop in levels.Current.Props)
            {
                var slider=prop.GetComponent<GravitySliderGuide>();
                if(slider!=null){if(slider.Joint.connectedAnchor.x<0)first=slider;else second=slider;}
                var seat=prop.GetComponent<ContactSeatLatch>();if(seat!=null)bridge=seat;
            }
            Assert.That(first,Is.Not.Null);Assert.That(second,Is.Not.Null);Assert.That(bridge,Is.Not.Null);
            // The live ball starts in its authored court. Only ordinary root tilt is applied.
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(0,-8,5),Vector3.down));Steps(480);
            Assert.That(first.Displacement,Is.GreaterThan(.09f),"First gravity gate must actually retract.");
            Assert.That(second.Displacement,Is.LessThan(.015f),"Opposite gate must remain on its closed stop.");
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(0,-8,-5),Vector3.down));Steps(480);
            Assert.That(second.Displacement,Is.GreaterThan(.09f),"Opposite gravity gate must actually retract.");
            Assert.That(first.Displacement,Is.LessThan(.015f));
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(4,-9.81f,0),Vector3.down));
            for(int tick=0;tick<900&&!bridge.Latched;tick++)Steps(1);
            if(!bridge.Latched)
            {
                TestContext.WriteLine($"GARDEN bridge angle {bridge.Hinge.Angle:F4}, native {bridge.Hinge.Joint.angle:F4}, COM {bridge.Hinge.Body.centerOfMass:F5}, local {levels.Current.transform.InverseTransformPoint(bridge.Hinge.Body.position):F5}.");
                foreach(var moving in bridge.GetComponentsInChildren<Collider>())
                foreach(var solid in levels.Current.GetComponentsInChildren<Collider>())
                    if(UnityEngine.Physics.ComputePenetration(moving,moving.transform.position,moving.transform.rotation,
                        solid,solid.transform.position,solid.transform.rotation,out Vector3 normal,out float depth)&&depth>.00001f)
                        TestContext.WriteLine($"GARDEN penetration {moving.name}/{solid.name}: depth {depth:F6}, normal {normal:F4}.");
            }
            Assert.That(bridge.Latched,Is.True,$"Garden bridge blocked in its actual housing at {bridge.Hinge.Angle:F3} degrees.");
            float seated=bridge.Hinge.Angle;
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(-4,-9.81f,0),Vector3.down));Steps(360);
            Assert.That(bridge.Hinge.Angle,Is.EqualTo(seated).Within(1));
            Assert.That(levels.Ball.Body.isKinematic,Is.False);
            levels.ResetLevel();Assert.That(bridge.Latched,Is.False);
        }

        [Test]
        public void CampaignBossLotus_FirstCamThreeContactStrokesAndPassageFromAuthoredSpawn()
        {
            LoadBoss(50);MemoryRatchetAssembly first=null;
            foreach(var memory in levels.Current.GetComponentsInChildren<MemoryRatchetAssembly>())
                if(memory.Cam.Joint.connectedAnchor.x<0)first=memory;
            Assert.That(first,Is.Not.Null);
            Assert.That(FlightMemorySteer(new Vector2(-.69f,-.145f)),Is.True,"Reach the first rack approach from the authored lotus spawn.");
            Assert.That(FlightMemorySteer(new Vector2(-.592f,-.145f)),Is.True);
            for(int stroke=0;stroke<3;stroke++)
            {
                Assert.That(FlightMemorySteer(new Vector2(-.592f,-.145f)),Is.True,"Re-approach the rack after each real return stroke.");
                levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(7,-7,0),Vector3.down));Steps(420);
                TestContext.WriteLine($"LOTUS stroke {stroke}: rack {first.Rack.Displacement:F4}, cam {first.Cam.Angle:F2}, retained {first.RetainedTeeth}, ball {levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position):F4}.");
                Assert.That(first.RetainedTeeth,Is.EqualTo(stroke+1),"A full tooth must come from actual ball/rack/finger contact.");
                Assert.That(FlightMemorySteer(new Vector2(-.68f,-.145f)),Is.True,"Retreat and brake within the rack approach court.");
                levels.Current.Rotation.SetTargetOrientation(Quaternion.identity);Steps(120);
                TestContext.WriteLine($"LOTUS return {stroke}: rack {first.Rack.Displacement:F4}, cam {first.Cam.Angle:F2}, ball {levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position):F4}.");
                Assert.That(first.Rack.Displacement,Is.LessThan(.010f));
                Assert.That(first.Cam.Angle,Is.GreaterThan((stroke+1)*30-1.8f));
            }
            Assert.That(first.PassageAligned,Is.True);
            Assert.That(FlightMemorySteer(new Vector2(-.67f,-.24f)),Is.True);
            Assert.That(FlightMemorySteer(new Vector2(-.42f,-.24f)),Is.True);
            Assert.That(FlightMemorySteer(new Vector2(-.42f,.18f)),Is.True,"The operating ball must physically traverse the opened first cam into court two.");
            Assert.That(levels.Ball.Body.isKinematic,Is.False);
            levels.ResetLevel();Assert.That(first.RetainedTeeth,Is.Zero);
        }

        [TestCase(30)] [TestCase(90)]
        public void CampaignBossConstellation_CentrelineHasSphereClearanceAndClosedWrongFaces(int boss)
        {
            LoadBoss(boss);UnityEngine.Physics.SyncTransforms();
            Vector3Int[] cells={new Vector3Int(0,0,0),new Vector3Int(1,0,0),new Vector3Int(1,0,1),new Vector3Int(1,-1,1),new Vector3Int(2,-1,1),new Vector3Int(2,-1,0),new Vector3Int(2,-2,0),new Vector3Int(1,-2,0),new Vector3Int(1,-2,-1),new Vector3Int(0,-2,-1),new Vector3Int(0,-3,-1),new Vector3Int(0,-4,-1),new Vector3Int(0,-5,-1),new Vector3Int(0,-6,-1)};
            Vector3 offset=new Vector3(0,.18f,.12f);
            for(int i=1;i<cells.Length;i++)
            {
                Vector3 from=offset+(Vector3)cells[i-1]*.12f,to=offset+(Vector3)cells[i]*.12f;
                Vector3 delta=to-from;
                Assert.That(UnityEngine.Physics.SphereCast(from,.015f,delta.normalized,out RaycastHit hit,delta.magnitude),Is.False,
                    "Room portal blocked "+cells[i-1]+" to "+cells[i]+" by "+(hit.collider!=null?hit.collider.name:"unknown"));
                Assert.That(UnityEngine.Physics.SphereCast(to,.015f,-delta.normalized,out _,delta.magnitude),Is.False,"Local reverse recovery route must remain open.");
            }
            Vector3 last=offset+(Vector3)cells[cells.Length-1]*.12f;
            Assert.That(UnityEngine.Physics.SphereCast(last,.015f,Vector3.right,out _, .12f),Is.True,"Last polar room must physically contain a sideways miss.");
            Assert.That(levels.Current.Exit.RequiredChannel,Is.Null.Or.Empty,"Room observation must not be a hidden win condition.");
        }

        [TestCase(40)] [TestCase(100)]
        public void CampaignBossCooperation_HasBroadPhysicalRefugeAndContactRetainedProgress(int boss)
        {
            LoadBoss(boss);var heart=levels.Current.GetComponent<MechanicalHeart>();
            Assert.That(levels.Balls.Count,Is.EqualTo(2));Assert.That(heart,Is.Not.Null);
            Assert.That(heart.ReleasePlunger,Is.Not.Null);Assert.That(heart.Cage,Is.Not.Null);
            Assert.That(heart.BridgeCatch.Armed,Is.False);Assert.That(heart.BridgeCatch.Receiver,Is.Not.Null);
            Transform carrier=heart.BridgeCatch.Hinge.transform.Find("Enlarged balanced carrier");
            Assert.That(carrier.Find("A 65mm recessed holding floor").localScale.x,Is.EqualTo(.065f).Within(.0001f));
            Assert.That(carrier.Find("B broad inclined cradle/B 90mm floor").localScale.z,Is.EqualTo(.09f).Within(.0001f));
            var initialA=levels.Balls[0].Body.position;var initialB=levels.Balls[1].Body.position;
            Steps(240);levels.ResetLevel();
            Assert.That(Vector3.Distance(levels.Balls[0].Body.position,initialA),Is.LessThan(.00001f));
            Assert.That(Vector3.Distance(levels.Balls[1].Body.position,initialB),Is.LessThan(.00001f));
            Assert.That(heart.Released,Is.False);Assert.That(heart.BridgeCatch.Latched,Is.False);
            Assert.That(levels.Current.Exit.RequiredChannel,Is.Null.Or.Empty);
        }

        [TestCase(70,1000f)] [TestCase(80,13546f)]
        public void CampaignBossLiquid_UsesExactRectangularDomainAndOnlyStaticObstacles(int boss,float density)
        {
            LoadBoss(boss);var water=levels.Current.GetComponent<WaterVolume>();
            Assert.That(levels.Balls.Count,Is.EqualTo(1));Assert.That(levels.Current.Props.Length,Is.Zero);
            Assert.That(water.HalfSize,Is.EqualTo(new Vector3(.16f,.042f,.16f)));
            Assert.That(water.Profile.Density,Is.EqualTo(density).Within(5));
            Assert.That(levels.Current.GetComponent<BossPresentation>(),Is.Not.Null);
            foreach(var col in levels.Current.GetComponentsInChildren<Collider>())Assert.That(col.attachedRigidbody,Is.EqualTo(levels.Current.GetComponent<Rigidbody>()));
            Steps(120);Assert.That(water.SubmergedFraction,Is.GreaterThan(.99f));
            Assert.That(levels.Ball.Profile.Radius,Is.EqualTo(.015f));
        }
    }
}
#endif
