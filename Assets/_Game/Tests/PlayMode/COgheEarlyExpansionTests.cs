using System.Collections;
using System.Text;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed class COgheEarlyExpansionTests
    {
        private const float Dt=1f/120f;
        private VenomCampaign game;
        private SimulationMode previousSimulation;

        [UnitySetUp] public IEnumerator Before()
        {
            VenomCampaignSave.PersistenceEnabled=false;
            previousSimulation=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            yield return null;
        }

        [UnityTearDown] public IEnumerator After()
        {
            Time.timeScale=1;Physics.simulationMode=previousSimulation;VenomCampaignSave.PersistenceEnabled=true;
            yield return null;
        }

        private IEnumerator Load(int number,bool campaign=false)
        {
            void Loaded(Scene scene,LoadSceneMode mode)
            {
                game=Object.FindFirstObjectByType<VenomCampaign>();
                if(game==null)return;
                game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;
            }
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync($"{(campaign?"COgheOrigin":"VenomOrigin")}{number:00}");}
            finally{SceneManager.sceneLoaded-=Loaded;}
            Assert.NotNull(game);yield return null;Steps(60);
        }

        private void Steps(int count)
        {for(int i=0;i<count;i++){game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);Physics.Simulate(Dt);}}

        private string State
        {
            get
            {
                var order=game.Motion.Get(0);
                return $"centre={game.Motion.Centre(0):F4}, command={(order==null?"none":order.Target.ToString("F4"))}, cursor={order?.Cursor}, " +
                    $"root={game.Root.rotation.eulerAngles:F2}, completed={game.Owner.Completed}, lost={game.Owner.Lost}, failure={game.Failure}";
            }
        }

        private void Evidence(string name)
        {Debug.Log($"COGHE EARLY {name}: {State}");COgheExpansionIntegrationTests.Capture(game,name);}

        private string RouteDiagnostics(Vector3 target)
        {
            var report=new StringBuilder($"targetLocal={game.Root.InverseTransformPoint(target):F4}");
            int group=game.Matter.Groups[0];float radius=game.Matter.Profile.ParticleRadius*.82f;
            for(int i=0;i<CohesiveOrganism.ParticleCount;i++)
            {
                if(game.Matter.Groups[i]!=group||game.Matter.Escaped[i])continue;
                Rigidbody body=game.Matter.Bodies[i];string support="none";
                if(game.Motion.Support(i,out Collider shape,out _,out _))
                {
                    var patch=System.Array.Find(game.Surfaces,p=>p!=null&&p.Shape==shape);
                    support=patch!=null?$"{patch.name}/grip={game.Motion.HasGrip(i)}":shape.name;
                }
                Vector3 delta=target-body.position;string blocker="clear";
                if(delta.sqrMagnitude>.000001f)
                {
                    var hits=Physics.SphereCastAll(body.position,radius,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore);
                    System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
                    foreach(var hit in hits)
                    {
                        Rigidbody hitBody=hit.collider.attachedRigidbody;bool own=false;
                        for(int j=0;j<CohesiveOrganism.ParticleCount;j++)if(game.Matter.Groups[j]==group&&game.Matter.Bodies[j]==hitBody){own=true;break;}
                        if(own)continue;blocker=$"{hit.collider.name}@{hit.distance:F3}";break;
                    }
                }
                report.Append($"\n p{i}={game.Root.InverseTransformPoint(body.position):F3} v={game.Root.InverseTransformDirection(body.linearVelocity):F3} support={support} sweep={blocker}");
            }
            return report.ToString();
        }

        private IEnumerator Until(float seconds,System.Func<bool> condition)
        {
            int ticks=Mathf.CeilToInt(seconds/Dt);
            for(int i=0;i<ticks&&!condition()&&!game.Owner.Lost;i++)
            {Steps(1);if(i%240==0)yield return null;}
        }

        private IEnumerator Rotate(Quaternion orientation)
        {
            game.Owner.Rotation.SetTargetOrientation(orientation);
            yield return Until(8,()=>Quaternion.Angle(game.Root.rotation,orientation)<.35f);
            Assert.Less(Quaternion.Angle(game.Root.rotation,orientation),.35f);
        }

        private float MinimumLocalCoordinate(int axis)
        {
            float minimum=float.PositiveInfinity;int group=game.Matter.Groups[0];
            for(int i=0;i<CohesiveOrganism.ParticleCount;i++)if(game.Matter.Groups[i]==group&&!game.Matter.Escaped[i])
                minimum=Mathf.Min(minimum,game.Root.InverseTransformPoint(game.Matter.Bodies[i].position)[axis]);
            return minimum;
        }

        [UnityTest] public IEnumerator Level11_CratePhysicallyReachesTheWallAndBridgesTheWholeSlipperyBand()
        {
            yield return Load(11);
            game.Motion.Move(0,game.Owner.Outlet.position-game.Owner.Outlet.forward*.024f,false,true);
            yield return Until(7,()=>false);
            Assert.IsFalse(game.Owner.Completed,"The uninterrupted slippery band must prevent the floor-to-exit shortcut. "+State);

            var crate=System.Array.Find(game.Props,p=>p.name=="Amber climbing crate");Assert.NotNull(crate);
            game.SelectProp(crate);yield return Until(18,()=>game.Attached);Assert.IsTrue(game.Attached,"COghe must physically approach and grasp the crate.");
            Vector3 dock=game.Root.TransformPoint(new Vector3(.190f,-.205f,.03f));
            for(int i=0;i<1800&&game.Root.InverseTransformPoint(crate.Body.position).x<.184f;i++)
            {game.SetPropTarget(dock);Steps(1);if(i%240==0)yield return null;}
            Vector3 local=game.Root.InverseTransformPoint(crate.Body.position);
            Assert.Greater(local.x,.18f,"The creature's contact forces must move the free crate to the exit wall. "+State);
            Assert.Greater(local.y+.095f,-.13f,"The real crate top must clear the authored slippery band.");
            Assert.Greater(Vector3.Dot(crate.transform.up,game.Root.up),.90f,"The weighted free crate must remain climbable after the push.");
            game.ReleaseProp();
            var top=System.Array.Find(crate.GetComponentsInChildren<VenomSurfacePatch>(),p=>Vector3.Dot(p.Normal,game.Root.up)>.9f);Assert.NotNull(top);
            Vector3 topTarget=top.Closest(crate.Body.position+game.Root.up)+game.Root.up*.019f;
            game.MoveTo(topTarget-game.Root.up*.019f,top);
            yield return Until(22,()=>Vector3.Distance(game.Motion.Centre(0),topTarget)<.050f);
            Evidence("11-on-crate");
            Assert.Less(Vector3.Distance(game.Motion.Centre(0),topTarget),.060f,"The commanded body must climb the real crate top. "+State);
            game.Motion.Move(0,game.Owner.Outlet.position-game.Owner.Outlet.forward*.024f,false,true);
            yield return Until(38,()=>game.Owner.Completed);Evidence("11-solved");
            Assert.IsTrue(game.Owner.Completed,"Docking must lead through crate climb, upper glass and the real aperture. "+State);
        }

        [UnityTest] public IEnumerator Level12_GravityCreatesSlideFlightThenActualWallContactPrecedesExit()
        {foreach(float lateral in new[]{0f,-.025f,.025f})yield return SlideRoute(lateral);}

        [UnityTest] public IEnumerator Campaign22_GravityCreatesSlideFlightThenActualWallContactPrecedesExit()
        {yield return SlideRoute(0,true);}

        [UnityTest] public IEnumerator Campaign22_BroadDeckTapsClimbFromFloorAndAllowRetry()
        {
            yield return Load(22,true);
            var deck=System.Array.Find(game.Surfaces,p=>p.name=="Launch platform");
            Assert.NotNull(deck);
            foreach(var local in new[]{new Vector3(-.23f,.145f,-.065f),new Vector3(-.23f,.145f,.065f),
                new Vector3(-.115f,.145f,-.065f),new Vector3(-.115f,.145f,.065f),new Vector3(-.1715f,.145f,0)})
            {
                game.ResetLevel();Steps(60);
                Vector3 target=game.Root.TransformPoint(local);
                game.TouchPoint(game.Owner.View.WorldToScreenPoint(target));
                Assert.AreEqual(deck,game.Feedback.CommandSurface,$"Visible deck tap {local} must select its real top.");
                yield return Until(10,()=>game.Root.InverseTransformPoint(game.Motion.Centre(0)).y>.155f&&Vector3.Distance(game.Motion.Centre(0),target)<.040f);
                Evidence("22-deck-tap");
                Assert.Greater(game.Root.InverseTransformPoint(game.Motion.Centre(0)).y,.155f,$"Climb to {local} must clear the physical top in one command. "+State);
                Assert.Less(Vector3.Distance(game.Motion.Centre(0),target),.040f,State);
            }
            // A missed slide can leave the creature behind the climbing board.
            // Use ordinary movement to reach that floor and then retry the top.
            var floor=System.Array.Find(game.Surfaces,p=>p.name=="Glass face 0");
            game.MoveTo(game.Root.TransformPoint(new Vector3(-.16f,-.30f,.23f)),floor);
            yield return Until(12,()=>game.Root.InverseTransformPoint(game.Motion.Centre(0)).y<-.24f);
            Assert.Less(game.Root.InverseTransformPoint(game.Motion.Centre(0)).y,-.24f,State);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(deck.transform.position));
            yield return Until(12,()=>game.Root.InverseTransformPoint(game.Motion.Centre(0)).y>.155f);
            Assert.Greater(game.Root.InverseTransformPoint(game.Motion.Centre(0)).y,.155f,"Return from the rear floor must not need Retry. "+State);
        }

        private IEnumerator SlideRoute(float lateral,bool campaign=false)
        {
            yield return Load(campaign?22:12,campaign);
            var monitor=Object.FindFirstObjectByType<COgheSlideLaunchMonitor>();Assert.NotNull(monitor);
            // Aim visibly down the entry slope. A point only millimetres past
            // the dry lip means stop with the tail still planted on the deck.
            var first=System.Array.Find(monitor.SlideSurfaces,p=>p.name=="Slippery trough 4");Assert.NotNull(first);
            Vector3 staging=game.Root.TransformPoint(new Vector3(-.115f,.168f,lateral));
            Evidence("12-before-climb");
            foreach(float x in new[]{-.31f,.31f})foreach(float y in new[]{-.31f,.31f})foreach(float z in new[]{-.31f,.31f})
            {
                Vector3 screen=game.Owner.View.WorldToViewportPoint(game.Root.TransformPoint(new Vector3(x,y,z)));
                Assert.That(screen.x,Is.InRange(.025f,.975f));Assert.That(screen.y,Is.InRange(.18f,.81f));
            }
            var deck=System.Array.Find(game.Surfaces,p=>p.name=="Launch platform");Assert.NotNull(deck);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(deck.Closest(staging)));
            Assert.AreEqual(deck,game.Feedback.CommandSurface,"A tap on the visible launch deck must select the deck, not a transparent wall/roof.");
            yield return Until(28,()=>Vector3.Distance(game.Motion.Centre(0),staging)<.045f);
            Evidence("12-staging");
            Assert.Less(Vector3.Distance(game.Motion.Centre(0),staging),.055f,"The separate gripping climb must reach the launch platform. "+State);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(first.Closest(first.transform.position+game.Root.forward*lateral)));
            CollectionAssert.Contains(monitor.SlideSurfaces,game.Feedback.CommandSurface,"The visible curved trough must accept a tap, including its real sidewall.");
            Assert.LessOrEqual(System.Array.IndexOf(monitor.SlideSurfaces,game.Feedback.CommandSurface),5,"The entry tap must select an upper segment, including its sidewall.");
            yield return Until(8,()=>monitor.EnteredSlide);if(!monitor.EnteredSlide)Evidence("12-missed-entry");Assert.IsTrue(monitor.EnteredSlide,State);
            yield return Until(8,()=>monitor.Launched);Evidence("12-launched");if(!monitor.Launched)Debug.Log("L12 LAUNCH BLOCKERS\n"+RouteDiagnostics(game.Root.TransformPoint(new Vector3(.24f,-.09f,0))));
            Assert.IsTrue(monitor.Launched,$"Every particle must physically clear the lip collider during flight. airborne={monitor.AirborneParticleCount}, actualContacts={monitor.ActualContactCount}, slideContacts={monitor.SlideContactCount}, supports={monitor.SupportContactCount}, speed={monitor.CurrentSpeed:F3}, peakSlide={monitor.PeakSlideSpeed:F3}, airFrames={monitor.AirborneFrames}; {State}");
            Assert.IsFalse(game.Owner.Completed,"Leaving the lip is not a win; tissue must physically catch and cross the real aperture.");
            yield return Until(8,()=>monitor.Caught);Evidence("12-caught");if(!monitor.Caught)Debug.Log("L12 CATCH BLOCKERS\n"+RouteDiagnostics(game.Owner.Outlet.position-game.Owner.Outlet.forward*.022f));Assert.IsTrue(monitor.Caught,"At least one real gripping contact must reach the catch wall. "+State);
            Assert.Greater(monitor.PeakAirSpeed,.12f,"The launch must carry measurable physical momentum.");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            Assert.IsTrue(game.Motion.Get(0)?.Exit??false,"The catch-wall exit must accept a screen tap.");
            yield return Until(25,()=>game.Owner.Completed);Evidence("12-solved");Assert.IsTrue(game.Owner.Completed,State);
        }

        [UnityTest] public IEnumerator Level13_ClosedLidVisitCanBeCancelledToReturnToLever()
        {
            yield return Load(13);
            var sequence=Object.FindFirstObjectByType<COgheLatchedAccessSequence>();
            var lid=sequence.TubeLid;
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(lid.position));
            Steps(2400);yield return null;Evidence("13-closed-lid-visit");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(sequence.Lever.position));
            yield return Until(25,()=>game.Attached);
            Evidence("13-closed-lid-return");
            if(!game.Attached)Debug.Log("L13 RETURN "+RouteDiagnostics(sequence.Lever.position));
            Assert.IsTrue(game.Attached,"A visit to closed cover B must not trap the creature or prevent returning to lever A. "+State);
            Assert.IsFalse(sequence.DoorLatched);Assert.IsFalse(sequence.TubeLatched);
        }

        [UnityTest] public IEnumerator Level13_ExteriorBelowClosedInletCanReturnToLever()
        {
            yield return Load(13);
            var sequence=Object.FindFirstObjectByType<COgheLatchedAccessSequence>();
            // Reach the same rear wall where the reported body became stranded,
            // using normal locomotion rather than placing tissue in the collider.
            Vector3 rear=game.Root.TransformPoint(new Vector3(.095f,-.230f,.204f));
            game.Motion.Move(0,rear);
            yield return Until(18,()=>Vector3.Distance(game.Motion.Centre(0),rear)<.027f);
            Assert.Less(Vector3.Distance(game.Motion.Centre(0),rear),.04f,"Reach the rear of the closed inlet: "+State);
            Evidence("13-rear-wall");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(sequence.Lever.position));
            yield return Until(25,()=>game.Attached);
            if(!game.Attached)Debug.Log("L13 REAR RETURN "+State+"\n"+RouteDiagnostics(sequence.Lever.position));
            Assert.IsTrue(game.Attached,"The exterior of the small chamber must provide a route back to lever A. "+State);
            Assert.IsFalse(sequence.DoorLatched);Assert.IsFalse(sequence.TubeLatched);
        }

        [UnityTest] public IEnumerator Level13_IdleKeepsLeverAndBothShuttersClosed()
        {
            yield return Load(13);
            var sequence=Object.FindFirstObjectByType<COgheLatchedAccessSequence>();
            yield return Until(15,()=>sequence.DoorLatched);
            Evidence("13-idle");
            Assert.IsFalse(sequence.DoorLatched,"Lever A must not fall open under its own weight before the player pulls it.");
            Assert.Less(sequence.DoorOpening,.012f);Assert.IsFalse(sequence.TubeLatched);
            Assert.Greater(Vector3.Distance(game.Motion.Centre(0),sequence.Lever.position),.30f,"Spawn must be visibly far from the lever.");
        }

        [UnityTest] public IEnumerator Level13_LeverThenMeasuredButtonOpenTheWindingTubeAndResetRelatchesBoth()
        {yield return WindingTubeSolution(false);}

        [UnityTest] public IEnumerator Campaign17_WindingTubeRemainsContinuousThroughTheExit()
        {yield return WindingTubeSolution(true);}

        [UnityTest] public IEnumerator Campaign17_WindingTubeAcceptsSettledAndDirectButtonApproaches()
        {
            yield return WindingTubeSolution(true,3,true);
            yield return WindingTubeSolution(true,10,false);
        }

        [UnityTest] public IEnumerator Campaign17_WindingTubeFinishesWithNativeFixedUpdate()
        {yield return WindingTubeSolution(true,3,true,true);}

        [UnityTest] public IEnumerator Campaign17_TemporaryTailSnagFixtureHoldsTheHeadAndRecovers()
        {yield return WindingTubeSolution(true,0,false,false,true);}

        private IEnumerator WindingTubeSolution(bool campaign,float settleSeconds=0,bool directButton=false,bool nativePhysics=false,bool snagFixture=false)
        {
            yield return Load(campaign?17:13,campaign);
            var sequence=Object.FindFirstObjectByType<COgheLatchedAccessSequence>();
            var tube=Object.FindFirstObjectByType<COgheTubeNetwork>();
            Assert.NotNull(sequence);Assert.NotNull(tube);Assert.IsFalse(game.FinalExitAvailable);Assert.IsFalse(tube.IsEntryOpen(0));

            var lever=sequence.Lever.GetComponent<VenomMovableProp>();Assert.NotNull(lever);
            Evidence("13-start");
            Assert.IsFalse(sequence.DoorLatched);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(lever.Body.position));yield return Until(16,()=>game.Attached);Assert.IsTrue(game.Attached,"The creature must reach and grasp lever A before it can move.");
            Assert.IsFalse(sequence.DoorLatched,"Approaching the lever must not open the door before the pull command.");
            Vector3 pulled=game.Root.TransformPoint(new Vector3(-.275f,-.299f,-.225f));
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(pulled));
            yield return Until(2.9f,()=>sequence.DoorLatched);
            Assert.IsTrue(sequence.DoorLatched,"Only actual hinge travel beyond the detent may latch door A. "+State);
            game.ReleaseProp();
            yield return Until(5,()=>sequence.DoorOpening>sequence.DoorTravel*.82f);
            Evidence("13-door-open");Assert.Greater(sequence.DoorOpening,sequence.DoorTravel*.82f,$"door={sequence.DoorOpening:F4}/{sequence.DoorTravel:F4}; "+State);

            // Guide through the visible doorway before selecting the interior pad.
            Vector3 doorway=game.Root.TransformPoint(new Vector3(.082f,-.299f,-.21f));
            if(!directButton)
            {
                game.TouchPoint(game.Owner.View.WorldToScreenPoint(doorway));
                yield return Until(22,()=>Vector3.Distance(game.Motion.Centre(0),doorway+game.Root.up*.019f)<.04f);
                Assert.Less(Vector3.Distance(game.Motion.Centre(0),doorway+game.Root.up*.019f),.05f,"Reach the open doorway: "+State);
            }
            Vector3 button=sequence.Button.transform.position+game.Root.up*.023f;
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(sequence.Button.transform.position+game.Root.up*.006f));
            yield return Until(28,()=>sequence.TubeLatched);
            Assert.IsTrue(sequence.Button.Pressed,"Button B must measure actual tissue mass. "+State);
            Assert.IsTrue(sequence.TubeLatched,State);yield return Until(5,()=>tube.IsEntryOpen(0));Evidence("13-button-and-lid");
            Assert.IsTrue(tube.IsEntryOpen(0),$"lid={sequence.LidOpening:F4}/{sequence.LidTravel:F4}; blocker={tube.EntryBlocker?.bounds}; "+State);
            for(int tick=0;tick<settleSeconds*120;tick++){Steps(1);if(tick%24==0)yield return null;}

            Vector3 inlet=game.Root.TransformPoint(tube.Nodes[0].LocalPosition);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(inlet));
            yield return Until(24,()=>tube.IsParticleInside(0));
            Assert.IsTrue(tube.IsParticleInside(0),"A screen tap must guide the creature to and into the clear inlet. "+State);
            float worstGap=0;int lastEscaped=-1,maxSkinPieces=0,maxFragments=0;
            bool disabledTransportCollider=false;float mass=game.Matter.TotalMass;
            // Explicit robustness fixture, not a player solution: temporarily
            // anchor the rearmost particle at its actual intake contact. No
            // positions or puzzle state are assigned. The head must wait, and
            // the complete body must exit after the contact releases.
            FixedJoint snag=null;
            if(snagFixture)
            {
                int tail=0;float rearmost=float.PositiveInfinity;
                Vector3 axis=game.Root.TransformDirection((tube.Edges[0].ControlPoints[1]-tube.Edges[0].ControlPoints[0]).normalized);
                for(int i=0;i<32;i++)
                {
                    float depth=Vector3.Dot(game.Matter.Bodies[i].position-inlet,axis);
                    if(depth<rearmost){rearmost=depth;tail=i;}
                }
                snag=game.Matter.Bodies[tail].gameObject.AddComponent<FixedJoint>();
                snag.connectedBody=game.Root.GetComponent<Rigidbody>();snag.enableCollision=true;
            }
            if(nativePhysics)
            {Physics.simulationMode=SimulationMode.FixedUpdate;game.Owner.enabled=true;game.Owner.Rotation.enabled=true;}
            const int snagReleaseTick=8*120;
            for(int tick=0;tick<30*120&&!game.Owner.Completed&&!game.Owner.Lost;tick++)
            {
                if(tick==snagReleaseTick&&snag!=null)Object.DestroyImmediate(snag);
                if(nativePhysics)yield return new WaitForFixedUpdate();else Steps(1);
                float gap=LargestTissueGap();
                // The infinite-strength snag is stronger than gameplay grip.
                // While pinned, require bounded transport (no head reaching the
                // exit). Once released, require the normal continuous skin too.
                bool measureSkin=!snagFixture||tick>=snagReleaseTick+240;
                if(snagFixture&&tick<snagReleaseTick)
                {
                    Assert.AreEqual(0,game.Matter.EscapedCount,"The head must wait for the physically snagged tail.");
                    for(int i=0;i<32;i++)Assert.Less(Vector3.Distance(game.Matter.Bodies[i].position,inlet),.18f,
                        "Do not scatter a connected body along the length of the winding pipe while its tail is blocked.");
                }
                if(measureSkin)worstGap=Mathf.Max(worstGap,gap);
                maxFragments=Mathf.Max(maxFragments,game.Matter.TotalFragmentCount);
                for(int i=0;i<32;i++)
                    disabledTransportCollider|=tube.IsParticleInside(i)&&!game.Matter.Bodies[i].GetComponent<Collider>().enabled;
                if(game.Matter.EscapedCount>0&&lastEscaped<0)
                {
                    Evidence(campaign?"17-first-emergence":"13-first-emergence");lastEscaped=game.Matter.EscapedCount;
                    // A stray touch on the room must not tear the body away
                    // from its ongoing transfer or hand off only its head.
                    if(campaign)game.TouchPoint(game.Owner.View.WorldToScreenPoint(doorway));
                }
                if(tick%12==0&&measureSkin)
                {
                    game.Matter.GetComponent<VenomSurface>().Rebuild(false);
                    int pieces=VisibleSkinPieces();
                    maxSkinPieces=Mathf.Max(maxSkinPieces,pieces);
                    if(pieces>1)Evidence($"17-skin-failure-{pieces}");
                }
                if(lastEscaped>=0&&lastEscaped<24&&game.Matter.EscapedCount>=24)
                {Evidence(campaign?"17-tail-emergence":"13-tail-emergence");lastEscaped=game.Matter.EscapedCount;}
                if(tick%24==0)yield return null;
            }
            if(nativePhysics)
            {game.Owner.enabled=false;game.Owner.Rotation.enabled=false;Physics.simulationMode=SimulationMode.Script;}
            Debug.Log($"WINDING MAX GAP {worstGap:F4} SKIN {maxSkinPieces}");Evidence(campaign?"17-solved":"13-solved");
            if(!game.Owner.Completed)Debug.Log($"L13 TUBE FAILURE {tube.DebugState(0)}\n{tube.DebugEntryState(0,0)}\n{tube.DebugExitRoster()}\n{RouteDiagnostics(inlet)}");
            Assert.IsTrue(game.Owner.Completed,"The continuous winding path must deliver all tissue through the final opening. "+State);
            Assert.AreEqual(32,game.Matter.EscapedCount);Assert.AreEqual(mass,game.Matter.TotalMass);
            Assert.AreEqual(0,game.Matter.CutCount);Assert.AreEqual(1,maxFragments);
            Assert.IsFalse(disabledTransportCollider,"Tissue must retain physical collision while the pipe still owns its movement.");
            Assert.Less(worstGap,.024f,"The head must not stretch away from the tail at the exit handoff.");
            Assert.AreEqual(1,maxSkinPieces,"The rendered flow must remain one substantial connected skin, not separate droplets.");

            game.ResetLevel();Steps(30);
            Assert.IsFalse(sequence.DoorLatched);Assert.IsFalse(sequence.TubeLatched);Assert.IsFalse(tube.ExitReached);Assert.IsFalse(game.FinalExitAvailable);
        }

        // Longest edge of a minimum spanning tree of the physical tissue. The
        // bond graph alone can remain connected by a very long, invisible spring.
        private float LargestTissueGap()
        {
            var reached=new bool[32];var distance=new float[32];
            for(int i=1;i<32;i++)distance[i]=float.PositiveInfinity;
            float largest=0;
            for(int count=0;count<32;count++)
            {
                int next=-1;
                for(int i=0;i<32;i++)if(!reached[i]&&(next<0||distance[i]<distance[next]))next=i;
                reached[next]=true;largest=Mathf.Max(largest,distance[next]);
                for(int i=0;i<32;i++)if(!reached[i])distance[i]=Mathf.Min(distance[i],Vector3.Distance(game.Matter.Bodies[next].position,game.Matter.Bodies[i].position));
            }
            return largest;
        }

        private int VisibleSkinPieces()
        {
            var mesh=game.Matter.transform.Find("Continuous wet skin").GetComponent<MeshFilter>().sharedMesh;
            var vertices=mesh.vertices;var triangles=mesh.triangles;
            var welded=new System.Collections.Generic.Dictionary<Vector3Int,int>();
            var ids=new int[vertices.Length];var parent=new int[vertices.Length];
            int Find(int id){while(parent[id]!=id){parent[id]=parent[parent[id]];id=parent[id];}return id;}
            for(int i=0;i<vertices.Length;i++)
            {
                Vector3 v=vertices[i]*100000;
                var key=new Vector3Int(Mathf.RoundToInt(v.x),Mathf.RoundToInt(v.y),Mathf.RoundToInt(v.z));
                if(!welded.TryGetValue(key,out int id)){id=i;welded.Add(key,id);parent[i]=i;}
                ids[i]=id;
            }
            for(int i=0;i<triangles.Length;i+=3)
            {
                int a=Find(ids[triangles[i]]);
                parent[Find(ids[triangles[i+1]])]=a;parent[Find(ids[triangles[i+2]])]=a;
            }
            var area=new float[vertices.Length];float total=0;
            for(int i=0;i<triangles.Length;i+=3)
            {
                float a=Vector3.Cross(vertices[triangles[i+1]]-vertices[triangles[i]],vertices[triangles[i+2]]-vertices[triangles[i]]).magnitude*.5f;
                area[Find(ids[triangles[i]])]+=a;total+=a;
            }
            int count=0;foreach(float a in area)if(a>total*.02f)count++;
            return count;
        }

        [UnityTest] public IEnumerator Level14_OppositeGravityDirectionsSeatTheBridgeThenOpenTheRailGate()
        {
            yield return Load(14);
            var assembly=Object.FindFirstObjectByType<COgheGravityBridgeAssembly>();Assert.NotNull(assembly);
            var climb=System.Array.Find(game.Surfaces,p=>p.name=="Safe climb");Assert.NotNull(climb);
            Vector3 aroundLeft=game.Root.TransformPoint(new Vector3(-.18f,-.265f,-.06f));
            game.Motion.Move(0,aroundLeft);yield return Until(18,()=>Vector3.Distance(game.Motion.Centre(0),aroundLeft)<.050f);
            Assert.Less(Vector3.Distance(game.Motion.Centre(0),aroundLeft),.060f,"The floor route must pass around the solid pedestal's left edge. "+State);
            Vector3 climbBottom=climb.Closest(game.Root.TransformPoint(new Vector3(-.04f,-.278f,-.08f)))+climb.Normal*.019f;
            game.MoveTo(climbBottom-climb.Normal*.019f,climb);yield return Until(14,()=>Vector3.Distance(game.Motion.Centre(0),climbBottom)<.055f);
            Assert.Less(Vector3.Distance(game.Motion.Centre(0),climbBottom),.065f,"The strip must make real contact with the floor route. "+State);
            // The hanging bridge covers the upper part of the pedestal's front.
            // Command its exposed front, not a point inside/behind its solid slab.
            var bridgeFront=System.Array.Find(assembly.Bridge.GetComponentsInChildren<VenomSurfacePatch>(),p=>Vector3.Dot(p.Normal,game.Root.forward)>.9f);
            Vector3 climbTop=bridgeFront.Closest(game.Root.TransformPoint(new Vector3(-.04f,.018f,-.06f)))+bridgeFront.Normal*.019f;
            game.MoveTo(climbTop-bridgeFront.Normal*.019f,bridgeFront);yield return Until(24,()=>Vector3.Distance(game.Motion.Centre(0),climbTop)<.055f);
            Evidence("14-climb-top");Assert.Less(Vector3.Distance(game.Motion.Centre(0),climbTop),.065f,"The body must physically crawl to the top of the gripping strip. "+State);
            var departurePatch=System.Array.Find(game.Surfaces,p=>p.name=="Bridge departure perch");Assert.NotNull(departurePatch);
            Vector3 cornerSurface=game.Root.TransformPoint(new Vector3(-.08f,.04f,-.150f));
            Vector3 corner=cornerSurface+departurePatch.Normal*.019f;
            game.MoveTo(cornerSurface,departurePatch);yield return Until(18,()=>Vector3.Distance(game.Motion.Centre(0),corner)<.050f);
            Assert.Less(Vector3.Distance(game.Motion.Centre(0),corner),.060f,"The body must turn physically from the wall onto the departure perch. "+State);
            Vector3 departureSurface=game.Root.TransformPoint(new Vector3(-.04f,.04f,-.115f));
            Vector3 departure=departureSurface+departurePatch.Normal*.019f;
            game.MoveTo(departureSurface,departurePatch);yield return Until(32,()=>Vector3.Distance(game.Motion.Centre(0),departure)<.055f&&MinimumLocalCoordinate(1)>.033f&&MinimumLocalCoordinate(2)>-.235f);
            Evidence("14-departure");if(Vector3.Distance(game.Motion.Centre(0),departure)>=.065f)Debug.Log("L14 DEPARTURE BLOCKERS\n"+RouteDiagnostics(departure));Assert.Less(Vector3.Distance(game.Motion.Centre(0),departure),.065f,"The gripping climb must place the body at bridge A. "+State);
            Assert.Greater(MinimumLocalCoordinate(1),.033f,"Every particle must clear the perch underside before gravity rotates. "+State);
            Assert.Greater(MinimumLocalCoordinate(2),-.235f,"No particle may remain in the climbing-plane pocket before gravity rotates. "+State);
            yield return Rotate(Quaternion.Euler(90,0,0));
            yield return Until(12,()=>assembly.BridgeSeated);Evidence("14-bridge-seat");Assert.IsTrue(assembly.BridgeSeated,$"World gravity must swing the free hinge to its receiver; travel={assembly.BridgeTravel:F2}. "+State);
            Assert.IsFalse(assembly.GateClear,"The same orientation must keep gate B over the exit.");

            var bridgeTop=System.Array.Find(assembly.Bridge.GetComponentsInChildren<VenomSurfacePatch>(),p=>Vector3.Dot(p.Normal,game.Root.up)>.85f);Assert.NotNull(bridgeTop);
            Vector3 crossing=bridgeTop.Closest(assembly.Bridge.position+game.Root.up)+game.Root.up*.019f;
            game.MoveTo(crossing-game.Root.up*.019f,bridgeTop);yield return Until(20,()=>Vector3.Distance(game.Motion.Centre(0),crossing)<.060f);
            Evidence("14-on-bridge");Assert.Less(Vector3.Distance(game.Motion.Centre(0),crossing),.072f,"The body must use the seated rigid bridge surface. "+State);
            var safePatch=System.Array.Find(game.Surfaces,p=>p.name=="Safe resting perch");Assert.NotNull(safePatch);
            Vector3 perchSurface=game.Root.TransformPoint(new Vector3(.04f,.04f,.145f));
            Vector3 perch=perchSurface+safePatch.Normal*.019f;
            game.MoveTo(perchSurface,safePatch);yield return Until(32,()=>Vector3.Distance(game.Motion.Centre(0),perch)<.055f&&MinimumLocalCoordinate(1)>.033f&&MinimumLocalCoordinate(2)>.065f);
            Evidence("14-safe-perch");Assert.Less(Vector3.Distance(game.Motion.Centre(0),perch),.065f,"The complete body must cross the physical bridge onto the safe perch. "+State);
            Assert.Greater(MinimumLocalCoordinate(1),.033f,"Every particle must remain above the safe perch face before reverse gravity. "+State);
            Assert.Greater(MinimumLocalCoordinate(2),.065f,"Every particle must clear bridge A before reverse gravity folds it. "+State);

            yield return Rotate(Quaternion.Euler(-65,0,0));
            yield return Until(12,()=>assembly.GateClear);Evidence("14-gate-open");Assert.IsTrue(assembly.GateClear,$"Reverse gravity must slide the constrained gate past its real clearance; displacement={assembly.GateDisplacement:F4}. "+State);
            Assert.Less(assembly.BridgeTravel,25f,"Opening gate B must also withdraw bridge A behind the resting body.");
            game.Motion.Move(0,game.Owner.Outlet.position-game.Owner.Outlet.forward*.022f,false,true);
            yield return Until(30,()=>game.Owner.Completed);Evidence("14-solved");Assert.IsTrue(game.Owner.Completed,State);
        }
    }
}
