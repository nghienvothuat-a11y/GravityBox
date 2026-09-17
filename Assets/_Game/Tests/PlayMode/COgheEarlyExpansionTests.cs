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

        private IEnumerator Load(int number)
        {
            void Loaded(Scene scene,LoadSceneMode mode)
            {
                game=Object.FindFirstObjectByType<VenomCampaign>();
                if(game==null)return;
                game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;
            }
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync($"VenomOrigin{number:00}");}
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
        {
            yield return Load(12);
            var monitor=Object.FindFirstObjectByType<COgheSlideLaunchMonitor>();Assert.NotNull(monitor);
            var first=System.Array.Find(monitor.SlideSurfaces,p=>p.name=="Slippery trough 1");Assert.NotNull(first);
            Vector3 staging=game.Root.TransformPoint(new Vector3(-.115f,.168f,0));
            game.Motion.Move(0,staging);yield return Until(28,()=>Vector3.Distance(game.Motion.Centre(0),staging)<.045f);
            Evidence("12-staging");
            Assert.Less(Vector3.Distance(game.Motion.Centre(0),staging),.055f,"The separate gripping climb must reach the launch platform. "+State);
            game.MoveTo(first.Closest(game.Root.TransformPoint(new Vector3(-.066f,.137f,0))),first);
            yield return Until(8,()=>monitor.EnteredSlide);if(!monitor.EnteredSlide)Evidence("12-missed-entry");Assert.IsTrue(monitor.EnteredSlide,State);
            yield return Until(8,()=>monitor.Launched);Evidence("12-launched");if(!monitor.Launched)Debug.Log("L12 LAUNCH BLOCKERS\n"+RouteDiagnostics(game.Root.TransformPoint(new Vector3(.24f,-.09f,0))));
            Assert.IsTrue(monitor.Launched,$"Every particle must physically clear the lip collider during flight. airborne={monitor.AirborneParticleCount}, actualContacts={monitor.ActualContactCount}, slideContacts={monitor.SlideContactCount}, supports={monitor.SupportContactCount}, speed={monitor.CurrentSpeed:F3}, peakSlide={monitor.PeakSlideSpeed:F3}, airFrames={monitor.AirborneFrames}; {State}");
            Assert.IsFalse(game.Owner.Completed,"Leaving the lip is not a win; tissue must physically catch and cross the real aperture.");
            yield return Until(8,()=>monitor.Caught);Evidence("12-caught");if(!monitor.Caught)Debug.Log("L12 CATCH BLOCKERS\n"+RouteDiagnostics(game.Owner.Outlet.position-game.Owner.Outlet.forward*.022f));Assert.IsTrue(monitor.Caught,"At least one real gripping contact must reach the catch wall. "+State);
            Assert.Greater(monitor.PeakAirSpeed,.12f,"The launch must carry measurable physical momentum.");
            game.Motion.Move(0,game.Owner.Outlet.position-game.Owner.Outlet.forward*.022f,false,true);
            yield return Until(25,()=>game.Owner.Completed);Evidence("12-solved");Assert.IsTrue(game.Owner.Completed,State);
        }

        [UnityTest] public IEnumerator Level13_LeverThenMeasuredButtonOpenTheWindingTubeAndResetRelatchesBoth()
        {
            yield return Load(13);
            var sequence=Object.FindFirstObjectByType<COgheLatchedAccessSequence>();
            var tube=Object.FindFirstObjectByType<COgheTubeNetwork>();
            Assert.NotNull(sequence);Assert.NotNull(tube);Assert.IsFalse(game.FinalExitAvailable);Assert.IsFalse(tube.IsEntryOpen(0));

            var lever=sequence.Lever.GetComponent<VenomMovableProp>();Assert.NotNull(lever);
            game.SelectProp(lever);yield return Until(16,()=>game.Attached);Assert.IsTrue(game.Attached,"The creature must reach and grasp lever A before it can move.");
            Vector3 pulled=game.Root.TransformPoint(new Vector3(-.275f,-.215f,-.17f));
            for(int i=0;i<1200&&!sequence.DoorLatched;i++)
            {game.SetPropTarget(pulled);Steps(1);if(i%240==0)yield return null;}
            Assert.IsTrue(sequence.DoorLatched,"Only actual hinge travel beyond the detent may latch door A. "+State);
            game.ReleaseProp();
            yield return Until(5,()=>sequence.DoorOpening>sequence.DoorTravel*.82f);
            Evidence("13-door-open");Assert.Greater(sequence.DoorOpening,sequence.DoorTravel*.82f,$"door={sequence.DoorOpening:F4}/{sequence.DoorTravel:F4}; "+State);

            Vector3 button=sequence.Button.transform.position+game.Root.up*.023f;
            game.Motion.Move(0,button,true);yield return Until(28,()=>sequence.TubeLatched);
            Assert.IsTrue(sequence.Button.Pressed,"Button B must measure actual tissue mass. "+State);
            Assert.IsTrue(sequence.TubeLatched,State);yield return Until(5,()=>tube.IsEntryOpen(0));Evidence("13-button-and-lid");
            Assert.IsTrue(tube.IsEntryOpen(0),$"lid={sequence.LidOpening:F4}/{sequence.LidTravel:F4}; blocker={tube.EntryBlocker?.bounds}; "+State);

            Vector3 inlet=game.Root.TransformPoint(tube.Nodes[0].LocalPosition);
            game.Motion.Move(0,inlet-game.Root.forward*.024f);yield return Until(24,()=>Vector3.Distance(game.Motion.Centre(0),inlet)<.10f);
            Assert.IsTrue(tube.TryChoose(0,0),"The whole connected body must physically reach the clear inlet.");
            yield return Until(30,()=>game.Owner.Completed);Evidence("13-solved");
            if(!game.Owner.Completed)Debug.Log($"L13 TUBE FAILURE {tube.DebugState(0)}\n{tube.DebugEntryState(0,0)}\n{tube.DebugExitRoster()}\n{RouteDiagnostics(inlet)}");
            Assert.IsTrue(game.Owner.Completed,"The continuous winding path must deliver all tissue through the final opening. "+State);

            game.ResetLevel();Steps(30);
            Assert.IsFalse(sequence.DoorLatched);Assert.IsFalse(sequence.TubeLatched);Assert.IsFalse(tube.ExitReached);Assert.IsFalse(game.FinalExitAvailable);
        }

        [UnityTest] public IEnumerator Level14_OppositeGravityDirectionsSeatTheBridgeThenOpenTheRailGate()
        {
            yield return Load(14);
            var assembly=Object.FindFirstObjectByType<COgheGravityBridgeAssembly>();Assert.NotNull(assembly);
            var climb=System.Array.Find(game.Surfaces,p=>p.name=="Safe climb");Assert.NotNull(climb);
            Vector3 aroundLeft=game.Root.TransformPoint(new Vector3(-.18f,-.265f,-.06f));
            game.Motion.Move(0,aroundLeft);yield return Until(18,()=>Vector3.Distance(game.Motion.Centre(0),aroundLeft)<.050f);
            Assert.Less(Vector3.Distance(game.Motion.Centre(0),aroundLeft),.060f,"The floor route must pass around the solid pedestal's left edge. "+State);
            Vector3 climbBottom=climb.Closest(game.Root.TransformPoint(new Vector3(-.10f,-.278f,-.08f)))+climb.Normal*.019f;
            game.MoveTo(climbBottom-climb.Normal*.019f,climb);yield return Until(14,()=>Vector3.Distance(game.Motion.Centre(0),climbBottom)<.055f);
            Assert.Less(Vector3.Distance(game.Motion.Centre(0),climbBottom),.065f,"The strip must make real contact with the floor route. "+State);
            Vector3 climbTop=climb.Closest(game.Root.TransformPoint(new Vector3(-.08f,.035f,-.22f)))+climb.Normal*.019f;
            game.MoveTo(climbTop-climb.Normal*.019f,climb);yield return Until(24,()=>Vector3.Distance(game.Motion.Centre(0),climbTop)<.055f);
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
