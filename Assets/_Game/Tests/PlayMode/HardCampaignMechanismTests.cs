#if UNITY_EDITOR
using GravityBox.Gameplay;
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed partial class PhysicsLifecycleTests
    {
        private void LoadHardCampaign(int number)
        {
            var catalog=AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/_Game/Campaign/CampaignCatalog.asset");
            Assert.That(catalog,Is.Not.Null);levels.Initialize(catalog,forces,storage:new VolatileCampaignStorage());Load(number-1);
        }

        [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void HardCampaign_RatchetMaximumRetainsUsefulOpeningAfterFurtherRackReturnsAndReset(int teeth)
        {
            if(teeth==3)Load(21);else LoadHardCampaign(teeth==1?91:93);
            var memory=levels.Current.GetComponentInChildren<MemoryRatchetAssembly>();
            Assert.That(memory.MaximumTeeth,Is.EqualTo(teeth));
            Quaternion start=memory.Cam.Body.rotation;
            // Isolated escapement-state fixture. This does not claim that a
            // player has driven the cam here: the route fixture is separate.
            for(int tooth=1;tooth<=teeth;tooth++)
            {
                memory.Cam.Body.rotation=start*Quaternion.AngleAxis(tooth*30+.3f,Vector3.down);
                Physics.SyncTransforms();memory.PrepareStep(Dt);
                Assert.That(memory.RetainedTeeth,Is.EqualTo(tooth));
            }
            for(int retry=0;retry<10;retry++)memory.PrepareStep(Dt);
            Assert.That(memory.Cam.Joint.limits.max,Is.EqualTo(teeth*30+1.5f).Within(.001f));
            Assert.That(memory.Cam.Joint.limits.min,Is.EqualTo(teeth*30-.8f).Within(.001f));
            Assert.That(memory.PassageAligned,Is.True);
            Assert.That(memory.Cam.Joint.useMotor,Is.False);
            levels.ResetLevel();
            Assert.That(memory.RetainedTeeth,Is.Zero);
            Assert.That(memory.PassageAligned,Is.False);
            Assert.That(memory.Cam.Joint.limits.max,Is.EqualTo(31.5f).Within(.001f));
            Assert.That(Quaternion.Angle(memory.Cam.Body.rotation,start),Is.LessThan(.001f));
        }

        [TestCase(91,30)] [TestCase(93,60)]
        public void HardCampaign_UsefulCamPositionHasContinuousThirtyMillimetreSphereClearance(int number,float angle)
        {
            LoadHardCampaign(number);var memory=levels.Current.GetComponentInChildren<MemoryRatchetAssembly>();
            Vector3 pivot=memory.Cam.Body.position;
            Vector3 from=pivot+new Vector3(0,-.032f,-.14f);
            Assert.That(Physics.SphereCast(from,.015f,Vector3.forward,out _,.28f),Is.True,"Closed rotor should block the real passage.");
            memory.Cam.Body.rotation*=Quaternion.AngleAxis(angle,Vector3.down);Physics.SyncTransforms();
            Assert.That(Physics.SphereCast(from,.015f,Vector3.forward,out RaycastHit hit,.28f),Is.False,
                "Indexed sphere passage obstructed by "+(hit.collider!=null?hit.collider.name:"unknown"));
        }

        [TestCase(45)] [TestCase(46)]
        public void HardCampaign_SharedCamIntermediateStateOpensNextCourtAndKeepsReverseRouteAtFinalState(int number)
        {
            LoadHardCampaign(number);var states=levels.Current.GetComponentsInChildren<MemoryRatchetAssembly>();
            Assert.That(states.Length,Is.EqualTo(2));
            Quaternion[] initial={states[0].Cam.Body.rotation,states[1].Cam.Body.rotation};
            for(int state=1;state<=2;state++)
            {
                for(int i=0;i<2;i++)states[i].Cam.Body.rotation=initial[i]*Quaternion.AngleAxis(state*30,Vector3.down);
                Physics.SyncTransforms();
                for(int i=0;i<2;i++)
                {
                    Vector3 from=states[i].Cam.Body.position+initial[i]*new Vector3(0,-.032f,-.14f);
                    bool blocked=Physics.SphereCast(from,.015f,initial[i]*Vector3.forward,out RaycastHit hit,.28f);
                    Assert.That(blocked,Is.EqualTo(i==1&&state==1),$"C{number} rotor{i+1}, state{state}, hit{(hit.collider!=null?hit.collider.name:"none")}");
                }
            }
        }

        [TestCase(67)] [TestCase(68)] [TestCase(77)] [TestCase(78)]
        public void HardCampaign_LiquidTopologyUsesOneBallStaticGeometryAndMatchingRectangularBounds(int number)
        {
            LoadHardCampaign(number);var water=levels.Current.GetComponent<WaterVolume>();
            Assert.That(water,Is.Not.Null);Assert.That(levels.Balls.Count,Is.EqualTo(1));Assert.That(levels.Current.Props,Is.Empty);
            Assert.That(water.HalfSize,Is.EqualTo(new Vector3(.37f,.357f,.37f)));
            Assert.That(water.Profile.Density,number<70?Is.LessThan(1100):Is.GreaterThan(13000));
            Assert.That(levels.Current.Exit.RequiredChannel,Is.Null.Or.Empty);
            Assert.That(levels.Current.Exit.ApertureRadius,Is.EqualTo(.023f));
        }

        [Test]
        public void HardCampaign_FirstBridgeFromSpawnCanSeatBySharedTiltWithoutMovingTheBall()
        {
            LoadHardCampaign(15);ContactSeatLatch latch=null;
            // PhysicalProp detaches articulated bodies at initialization. Query
            // the level's owned prop registry, not its original prefab hierarchy.
            foreach(var prop in levels.Current.Props)
                if(prop.GetComponent<ContactSeatLatch>()!=null)latch=prop.GetComponent<ContactSeatLatch>();
            Assert.That(latch,Is.Not.Null);
            Vector3 start=levels.Ball.Body.position;
            TestContext.WriteLine($"Initial bridge angle={latch.Hinge.Angle:F3}, native={latch.Hinge.Joint.angle:F3}, COM={latch.Hinge.Body.centerOfMass:F5}, anchor={latch.Hinge.Joint.connectedAnchor:F5}");
            // Ball starts at the south-west approach. North refuge is physically
            // separate; this fixture deliberately tests passive bridge seating.
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(2.0f,-9.81f,0),Vector3.down));
            for(int tick=0;tick<1800&&!latch.Latched;tick++)
            {
                Steps(1);
                Assert.That(levels.Current.IsOutside(levels.Ball.Body.position),Is.False);
            }
            TestContext.WriteLine($"Final bridge angle={latch.Hinge.Angle:F3}, native={latch.Hinge.Joint.angle:F3}, speed={latch.Hinge.AngularSpeed:F3}, COM={latch.Hinge.Body.centerOfMass:F5}, position={latch.Hinge.Body.position:F5}, seat={latch.Receiver.bounds}");
            Assert.That(latch.Latched,Is.True,"Bridge must actually rest on its receiver before contact retention.");
            levels.ResetLevel();
            Assert.That(latch.Latched,Is.False);Assert.That(Vector3.Distance(start,levels.Ball.Body.position),Is.LessThan(.00001f));
        }

        [TestCase(27)] [TestCase(28)] [TestCase(67)] [TestCase(68)]
        [TestCase(77)] [TestCase(78)] [TestCase(85)] [TestCase(87)] [TestCase(88)]
        public void HardCampaign_ThreeDimensionalPortsHaveFullSphereClearanceAndReverseRecovery(int number)
        {
            LoadHardCampaign(number);Physics.SyncTransforms();
            Vector3Int[] corners=HardRouteCorners(number);
            Vector3 offset=number==28?new Vector3(0,0,-.06f):Vector3.zero;
            for(int segment=1;segment<corners.Length;segment++)
            {
                Vector3Int at=corners[segment-1],delta=corners[segment]-at;
                Vector3Int step=new Vector3Int(System.Math.Sign(delta.x),System.Math.Sign(delta.y),System.Math.Sign(delta.z));
                while(at!=corners[segment])
                {
                    Vector3 from=(Vector3)at*.12f+offset,to=(Vector3)(at+step)*.12f+offset;
                    Check(from,to);Check(to,from);at+=step;
                }
            }
            Assert.That(levels.Current.Exit.RequiredChannel,Is.Null.Or.Empty);
            void Check(Vector3 from,Vector3 to)
            {
                Vector3 d=to-from;
                foreach(var hit in Physics.SphereCastAll(from,.015f,d.normalized,d.magnitude))
                    if(hit.collider.attachedRigidbody==levels.Current.GetComponent<Rigidbody>())
                        Assert.Fail($"C{number} physical centreline {from:F3}→{to:F3} hits {hit.collider.name}.");
            }
        }
        private static Vector3Int[] HardRouteCorners(int number)
        {
            Vector3Int V(int x,int y,int z)=>new Vector3Int(x,y,z);
            switch(number)
            {
                case 27:return new[]{V(-2,1,-1),V(0,1,-1),V(0,1,1),V(0,0,1),V(2,0,1),V(2,-1,1),V(2,-1,-1)};
                case 28:return new[]{V(-2,1,-2),V(0,1,-2),V(0,1,0),V(0,0,0),V(2,0,0),V(2,0,2),V(2,-1,2),V(0,-1,2),V(0,-1,3)};
                case 67:return new[]{V(-2,1,1),V(-2,1,-1),V(1,1,-1),V(1,0,-1),V(1,0,1),V(2,0,1)};
                case 68:return new[]{V(-2,1,-1),V(0,1,-1),V(0,1,1),V(1,1,1),V(1,0,1),V(1,0,2),V(-1,0,2)};
                case 77:return new[]{V(-2,-1,-1),V(-1,-1,-1),V(-1,-1,0),V(0,-1,0),V(0,0,0),V(0,1,0),V(1,1,0),V(1,1,1),V(2,1,1)};
                case 78:return new[]{V(-2,-1,-1),V(0,-1,-1),V(0,-1,1),V(0,0,1),V(1,0,1),V(1,1,1),V(1,1,-1),V(2,1,-1)};
                case 85:return new[]{V(-2,-1,-1),V(-1,-1,-1),V(-1,0,-1),V(-1,0,0),V(0,0,0),V(1,0,0),V(1,1,0),V(1,1,1),V(2,1,1)};
                case 87:return new[]{V(-2,0,0),V(2,0,0),V(2,-1,0)};
                case 88:return new[]{V(-2,1,-2),V(0,1,-2),V(0,1,0),V(0,0,0),V(2,0,0),V(2,0,2),V(0,0,2),V(0,-1,2),V(-2,-1,2)};
                default:throw new System.ArgumentOutOfRangeException(nameof(number));
            }
        }

        [Test]
        public void HardCampaign_OrthogonalChambersHaveAnActualXNeckAndClosedWrongFaces()
        {
            LoadHardCampaign(85);Physics.SyncTransforms();
            Vector3 west=new Vector3(-.12f,0,0),east=Vector3.zero;
            Assert.That(Physics.SphereCast(west,.015f,Vector3.right,out _,.12f),Is.False,"The true X neck must admit the whole ball in both directions.");
            Assert.That(Physics.SphereCast(east,.015f,Vector3.left,out _,.12f),Is.False);
            Assert.That(Physics.SphereCast(west+Vector3.up*.032f,.015f,Vector3.right,out RaycastHit collar,.12f),Is.True,
                "An off-centre transfer must meet the 76 mm neck instead of an unused floor decoration.");
            Assert.That(collar.collider.name,Does.StartWith("Transfer neck collar"));
            Assert.That(Physics.SphereCast(east,.015f,Vector3.down,out _,.12f),Is.True,"There is no lower bypass at the middle rest node.");
            Assert.That(Physics.SphereCast(east,.015f,Vector3.forward,out _,.12f),Is.True,"The wrong depth face stays physically closed.");
        }

        [TestCase(15)] [TestCase(16)] [TestCase(17)] [TestCase(18)]
        [TestCase(20)] [TestCase(47)] [TestCase(50)] [TestCase(98)]
        public void HardCampaign_BridgeRecoveryJoinsOpenLandingEndWithFullBallClearance(int number)
        {
            LoadHardCampaign(number);Physics.SyncTransforms();ContactSeatLatch latch=null;
            foreach(var prop in levels.Current.Props)
                if(prop.GetComponent<ContactSeatLatch>()!=null)latch=prop.GetComponent<ContactSeatLatch>();
            Assert.That(latch,Is.Not.Null);
            var box=levels.Current.GetComponent<Rigidbody>();
            Vector3 origin=latch.Hinge.Joint.connectedAnchor;
            Vector3 from=levels.Current.transform.TransformPoint(origin+new Vector3(.245f,.043f,0));
            Vector3 to=levels.Current.transform.TransformPoint(origin+new Vector3(.215f,.023f,0));
            Vector3 delta=to-from;
            foreach(var hit in Physics.SphereCastAll(from,.015f,delta.normalized,delta.magnitude))
                if(hit.collider.attachedRigidbody==box)
                    Assert.Fail($"C{number} recovery-to-landing joint hits {hit.collider.name}.");
            var ramp=levels.Current.transform.Find("Bridge graded recovery return");
            Assert.That(ramp,Is.Not.Null);
            // Both physical ends, including a full ball's clearance, must stay
            // within the shell's footprint and its floor/cover height.
            float halfLength=ramp.localScale.x*.5f;
            foreach(int side in new[]{-1,1})
            {
                Vector3 point=ramp.localPosition+ramp.localRotation*(Vector3.right*(side*halfLength));
                Assert.That(point.y,Is.GreaterThanOrEqualTo(-levels.Current.InteriorDepth*.5f-.006f));
                Assert.That(point.y+.03f,Is.LessThan(levels.Current.InteriorDepth*.5f));
                Assert.That(InsideHardFootprint(new Vector2(point.x,point.z),levels.Current.Footprint),Is.True,$"C{number} recovery end lies beyond the shell.");
                for(int sample=0;sample<8;sample++)
                {
                    float angle=sample*Mathf.PI*.25f;
                    Vector2 extent=new Vector2(point.x,point.z)+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*.015f;
                    Assert.That(InsideHardFootprint(extent,levels.Current.Footprint),Is.True,$"C{number} recovery end lacks full-ball shell clearance.");
                }
            }
        }
        private static bool InsideHardFootprint(Vector2 p,Vector2[] polygon)
        {
            bool inside=false;
            for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++)
                if((polygon[i].y>p.y)!=(polygon[j].y>p.y)&&p.x<(polygon[j].x-polygon[i].x)*(p.y-polygon[i].y)/(polygon[j].y-polygon[i].y)+polygon[i].x)inside=!inside;
            return inside;
        }
    }
}
#endif
