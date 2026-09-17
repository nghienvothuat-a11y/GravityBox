using System.Collections;
using System.Collections.Generic;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed class COgheMechanismExpansionTests
    {
        private const float Dt=1f/120;
        private SimulationMode previousMode;
        private readonly List<GameObject> fixtures=new List<GameObject>();
        private VenomCampaign game;

        [SetUp] public void Before()
        {previousMode=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;VenomCampaignSave.PersistenceEnabled=false;}
        [TearDown] public void After()
        {foreach(var go in fixtures)if(go!=null)Object.DestroyImmediate(go);fixtures.Clear();Physics.simulationMode=previousMode;VenomCampaignSave.PersistenceEnabled=true;Time.timeScale=1;}

        private Transform Frame()
        {
            var go=new GameObject("Isolated rail test frame",typeof(Rigidbody));fixtures.Add(go);go.transform.position=new Vector3(3,0,0);go.GetComponent<Rigidbody>().isKinematic=true;return go.transform;
        }
        private COgheRailSlider Rail(Transform frame,Vector3 axis,float mass,float friction,float initial=0,bool gravity=false)
        {
            var go=new GameObject("Test carriage",typeof(Rigidbody),typeof(ConfigurableJoint),typeof(COgheRailSlider));go.transform.SetParent(frame,false);
            var rb=go.GetComponent<Rigidbody>();rb.mass=mass;rb.useGravity=false;rb.solverIterations=20;rb.solverVelocityIterations=8;
            var rail=go.GetComponent<COgheRailSlider>();rail.Body=rb;rail.Frame=frame;rail.Axis=axis;rail.Travel=.12f;rail.InitialTravel=initial;rail.Resistance=friction;rail.Gravity=gravity;
            var joint=go.GetComponent<ConfigurableJoint>();rail.Joint=joint;joint.connectedBody=frame.GetComponent<Rigidbody>();joint.autoConfigureConnectedAnchor=false;
            joint.axis=axis;joint.secondaryAxis=axis==Vector3.up?Vector3.forward:Vector3.up;joint.xMotion=ConfigurableJointMotion.Limited;
            joint.yMotion=joint.zMotion=joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Locked;joint.linearLimit=new SoftJointLimit{limit=.06f};
            rail.ResetMechanism(null);return rail;
        }
        private static void Drive(COgheRailSlider rail,float effort,int ticks)
        {for(int i=0;i<ticks;i++){rail.ApplyEffort(rail.WorldAxis*effort);rail.StepMechanism(null,Dt);Physics.Simulate(Dt);}}

        [Test] public void HeavyCarriageRequiresLargeFragmentForceAndCanBeLoweredAgain()
        {
            var rail=Rail(Frame(),Vector3.up,.032f,.055f,0,true);rail.LatchAtEnd=true;
            Drive(rail, .224f,240);Assert.Less(rail.Position,.002f,"One third of full tissue force must not lift G");
            Drive(rail, .448f,240);Assert.IsTrue(rail.AtEnd,"Two thirds of the .672 N full body force must raise G");
            Drive(rail,0,120);Assert.IsTrue(rail.Latched);Assert.IsTrue(rail.AtEnd,"End catch carries the gravity load");
            Drive(rail,-.20f,120);Assert.Less(rail.Position,.02f,"Reverse pull releases the catch, allowing a fresh alignment");
            rail.ResetMechanism(null);Assert.IsFalse(rail.Latched);Assert.That(rail.Position,Is.EqualTo(0).Within(.0001f));
        }

        [Test] public void FinalCoverUsesContinuousResistanceAndPhysicalPin()
        {
            var cap=Rail(Frame(),Vector3.right,.20f,.57f);cap.LatchAtEnd=true;cap.Locked=true;
            Drive(cap,.672f,180);Assert.Less(cap.Position,.002f,"Full tissue cannot bypass an engaged pin");
            cap.Locked=false;Drive(cap,.56f,180);Assert.Less(cap.Position,.002f,"A small missing amount of mass matters continuously");
            Drive(cap,.64f,300);Assert.IsTrue(cap.AtEnd,"A sufficiently large uneven fragment may move H without checking fragment count");
            Drive(cap,0,120);Assert.IsTrue(cap.Latched);
            cap.LatchAtStart=true;
            Drive(cap,-.64f,300);Assert.Less(cap.Position,.002f);Drive(cap,0,30);Assert.IsTrue(cap.Latched,"Lower bearing catches retain alignment too");
            Drive(cap,.64f,300);Assert.IsTrue(cap.AtEnd,"A reverse pull releases the lower catch for recovery");
        }

        [Test] public void RailsFollowWorldGravityAcrossAnOrientationRegionAndSeparateAgain()
        {
            var frame=Frame();var rails=new[]{Rail(frame,Vector3.right,.10f,.012f,.12f,true),Rail(frame,Vector3.forward,.10f,.012f,.12f,true),Rail(frame,Vector3.up,.10f,.012f,.12f,true)};
            foreach(float offset in new[]{-8f,0f,8f})
            {
                frame.rotation=Quaternion.Euler(offset,0,0)*Quaternion.FromToRotation(new Vector3(-1,-1,-1).normalized,Vector3.down);
                foreach(var rail in rails)rail.ResetMechanism(null);Physics.SyncTransforms();
                for(int i=0;i<240;i++){foreach(var rail in rails)rail.StepMechanism(null,Dt);Physics.Simulate(Dt);}
                foreach(var rail in rails)Assert.Less(rail.Position,.002f,"Each orthogonal rail must settle at its lower stop across a broad tilt region");
            }
            frame.rotation=Quaternion.FromToRotation(Vector3.one.normalized,Vector3.down);Physics.SyncTransforms();
            for(int i=0;i<240;i++){foreach(var rail in rails)rail.StepMechanism(null,Dt);Physics.Simulate(Dt);}
            foreach(var rail in rails){Assert.IsTrue(rail.AtEnd);Assert.IsFalse(rail.Latched,"Bridge pieces never weld together");}
        }

        [Test] public void GearTrainRejectsGapsAndAxialMisalignmentAndUsesAlternatingRatios()
        {
            var frame=Frame();var train=frame.gameObject.AddComponent<COgheGearTrain>();train.Wheels=new Transform[4];train.PitchRadii=new[]{.05f,.05f,.05f,.05f};
            for(int i=0;i<4;i++){train.Wheels[i]=new GameObject("Gear "+i).transform;train.Wheels[i].SetParent(frame,false);train.Wheels[i].localPosition=Vector3.right*(i*.10f);}
            train.InitializeMechanism(null);train.ResetMechanism(null);
            train.Wheels[2].localPosition+=Vector3.up*.06f;train.StepMechanism(null,Dt);
            Assert.IsFalse(train.Meshed);Assert.Less(train.AngularSpeeds[0],0);Assert.Greater(train.AngularSpeeds[1],0);Assert.AreEqual(0,train.AngularSpeeds[2]);Assert.AreEqual(0,train.AngularSpeeds[3]);
            train.Wheels[2].localPosition=new Vector3(.20f,0,.01f);train.StepMechanism(null,Dt);Assert.IsFalse(train.Meshed,"Close screen projection is insufficient if axles are in separate planes");
            train.Wheels[2].localPosition=new Vector3(.20f,0,0);train.StepMechanism(null,Dt);Assert.IsTrue(train.Meshed);
            Assert.Greater(train.AngularSpeeds[3],0);Assert.That(train.AngularSpeeds[3],Is.EqualTo(-train.AngularSpeeds[0]).Within(.0001f));
            train.Wheels[1].localPosition+=Vector3.down*.04f;train.StepMechanism(null,Dt);Assert.AreEqual(0,train.AngularSpeeds[3],"Breaking any contact stops the downstream train");
        }

        private IEnumerator Load(int number)
        {
            void Loaded(Scene scene,LoadSceneMode mode){game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;}
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync($"VenomOrigin{number:00}");}finally{SceneManager.sceneLoaded-=Loaded;}
            Assert.NotNull(game);yield return null;
        }
        private void TissueOn(COgheTissueSensor pad,int start,int count)
        {
            for(int i=start;i<start+count;i++)
            {var body=game.Matter.Bodies[i];body.position=pad.transform.TransformPoint(new Vector3((i%3-1)*.018f,.010f,((i/3)%3-1)*.018f));if(!body.isKinematic)body.linearVelocity=Vector3.zero;}
            pad.StepMechanism(game,Dt);
        }

        private string State
        {
            get
            {
                var parts=new List<string>();foreach(int a in Anchors())parts.Add($"{a}:{Count(a)} at {game.Root.InverseTransformPoint(game.Motion.Centre(a)):F3} route={game.Motion.Get(a)?.Cursor}/{game.Motion.Get(a)?.Path.Count}");
                var mechanisms=new List<string>();foreach(var rail in game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())mechanisms.Add($"{rail.name}={rail.Position:F4}/{rail.Travel:F3},locked={rail.Locked},force={rail.Effort:F3}");
                foreach(var pad in game.Owner.Apparatus.GetComponentsInChildren<COgheTissueSensor>())mechanisms.Add($"{pad.name}={pad.Load:F3}/{pad.Threshold:F3}");
                foreach(var knife in game.Owner.Apparatus.GetComponentsInChildren<COgheGuillotine>())mechanisms.Add($"{knife.name} phase={knife.Phase} blade={game.Root.InverseTransformPoint(knife.Rail.Body.position):F4}");
                return $"{game.Failure}; activity={game.Activity}; groups={string.Join(" | ",parts)}; escaped={game.Matter.EscapedCount}; {string.Join("; ",mechanisms)}";
            }
        }
        private List<int> Anchors()
        {
            var result=new List<int>();var seen=new HashSet<int>();
            for(int i=0;i<32;i++)if(!game.Matter.Escaped[i]&&seen.Add(game.Matter.Groups[i]))result.Add(i);
            return result;
        }
        private int Count(int anchor)
        {int count=0;for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[anchor])count++;return count;}
        private void Tick()
        {game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);Physics.Simulate(Dt);}
        private IEnumerator WaitFor(float seconds,System.Func<bool> condition,string reason,System.Action refresh=null,System.Func<string> diagnostics=null)
        {
            for(int i=0;i<Mathf.CeilToInt(seconds/Dt)&&!condition()&&!game.Owner.Lost;i++)
            {if(i%120==0)refresh?.Invoke();Tick();if(i%240==0)yield return null;}
            if(!condition())COgheExpansionIntegrationTests.Capture(game,$"{game.Definition.Order:00}-failed-{NUnit.Framework.TestContext.CurrentContext.Test.Name}");
            Assert.IsTrue(condition(),reason+"; "+State+(condition()||diagnostics==null?"":"; "+diagnostics()));
        }
        private IEnumerator Walk(int anchor,Vector3 point,float seconds=25,float tolerance=.035f)
        {
            game.SelectFragment(anchor);game.Motion.Move(anchor,point,true);
            yield return WaitFor(seconds,()=>Vector3.Distance(game.Motion.Centre(anchor),point)<tolerance,"Walk to "+game.Root.InverseTransformPoint(point));
        }
        private IEnumerator Pull(int anchor,COgheRailSlider rail,float direction,System.Func<bool> finished,float seconds=24)
        {
            game.SelectFragment(anchor);game.SelectProp(rail.GetComponent<VenomMovableProp>());
            yield return WaitFor(25,()=>game.Attached,"Approach and grasp "+rail.name);
            Vector3 target=rail.Frame.TransformPoint(rail.Start+rail.Axis*(direction>0?rail.Travel+.12f:-.12f));
            game.SetPropTarget(target);
            yield return WaitFor(seconds,finished,"Drive "+rail.name+" with "+Count(anchor)+" particles",()=>game.SetPropTarget(target));
            COgheExpansionIntegrationTests.Capture(game,$"{game.Definition.Order:00}-operated-{rail.name}");
            game.ReleaseProp();
        }
        private IEnumerator HoldPad(int anchor,COgheTissueSensor pad)
        {
            yield return Walk(anchor,pad.transform.position+pad.transform.up*.018f);
            yield return WaitFor(10,()=>pad.Active,"Actual supported tissue load on "+pad.name);
            COgheExpansionIntegrationTests.Capture(game,$"{game.Definition.Order:00}-held-{pad.name}");
        }
        private IEnumerator ThroughPipe(int anchor,COgheTubeNetwork pipe)
        {
            Vector3 entry=game.Root.TransformPoint(pipe.Nodes[0].LocalPosition);
            yield return Walk(anchor,entry+game.Root.TransformDirection(pipe.Nodes[0].LocalOutward)*.025f,35,.025f);
            Assert.IsTrue(pipe.TryChoose(anchor,0),"A physically reached mouth accepts its adjacent edge; "+State);
            yield return WaitFor(18,()=>pipe.LastReachedNode==1&&!pipe.IsParticleInside(anchor),"Bidirectional transfer reaches the other room",diagnostics:()=>
            {
                var particles=new List<string>();
                for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[anchor])
                {
                    var body=game.Matter.Bodies[i];var sphere=body.GetComponent<SphereCollider>();
                    particles.Add($"p{i} local={game.Root.InverseTransformPoint(body.position):F5} velocity={game.Root.InverseTransformDirection(body.linearVelocity):F4} radius={sphere.radius:F5} scale={sphere.transform.lossyScale:F3}");
                }
                return pipe.DebugState(anchor)+" bore="+pipe.Radius+" particles="+string.Join(" | ",particles);
            });
            // Descend the receiving pedestal's continuous outer grip face
            // before ordering a long move through the next compartment.
            Vector3 landing=pipe.Nodes[1].LocalPosition+pipe.Nodes[1].LocalOutward*.064f;landing.y=-.274f;
            yield return Walk(anchor,game.Root.TransformPoint(landing),25,.032f);
        }
        private IEnumerator Reunion(Vector3 world)
        {
            foreach(int anchor in Anchors())game.Motion.Move(anchor,world,true);
            yield return WaitFor(40,()=>game.Matter.TotalFragmentCount==1,"All physical parts reunite through the opened gates");
        }
        private IEnumerator Leave(int anchor)
        {
            game.SelectFragment(anchor);game.Motion.Move(anchor,game.Owner.Outlet.position-game.Owner.Outlet.forward*.024f,false,true);
            yield return WaitFor(40,()=>game.Owner.Completed,"The complete merged body traverses the real exit");
            COgheExpansionIntegrationTests.Capture(game,$"{game.Definition.Order:00}-completed");
            Assert.AreEqual(32,game.Matter.EscapedCount);
        }

        [UnityTest] public IEnumerator Level17FullSolutionThroughBearingCommandsAndActualExit()
        {
            yield return Load(17);var rails=game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>();
            COgheRailSlider a=null,b=null;foreach(var rail in rails){if(rail.name.StartsWith("A "))a=rail;if(rail.name.StartsWith("B "))b=rail;}
            var train=game.Owner.Apparatus.GetComponentInChildren<COgheGearTrain>();
            yield return Walk(0,game.Root.TransformPoint(new Vector3(-.129f,a.Start.y,.068f)));
            yield return Pull(0,a,1,()=>a.AtEnd);
            Assert.IsFalse(train.ExitUnlocked,"One adjusted bearing does not open the outlet");
            yield return Walk(0,game.Root.TransformPoint(new Vector3(-.129f,-.277f,.068f)));
            yield return Walk(0,game.Root.TransformPoint(new Vector3(-.129f,-.277f,-.11f)));
            yield return Walk(0,game.Root.TransformPoint(new Vector3(.129f,-.277f,-.11f)));
            yield return Walk(0,game.Root.TransformPoint(new Vector3(.129f,b.Start.y+b.InitialTravel,.068f)));
            yield return Pull(0,b,-1,()=>b.Position<.002f);
            yield return WaitFor(15,()=>train.ExitUnlocked,"A continuous physical transmission lifts its rack");
            yield return Leave(0);
        }

        [UnityTest] public IEnumerator Level18FullSolutionByGravityDockingAndWalkingTheBridge()
        {
            yield return Load(18);var rails=game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>();
            game.Motion.Move(0,game.Root.TransformPoint(new Vector3(-.255f,-.098f,0)),true);
            Quaternion pose=Quaternion.FromToRotation(new Vector3(-1,-1,-1).normalized,Vector3.down);game.Owner.Rotation.SetTargetOrientation(pose);
            yield return WaitFor(12,()=>{foreach(var rail in rails)if(rail.Position>.002f)return false;return true;},"All three colliding bridge bars dock under the same world gravity");
            foreach(float x in new[]{-.14f,0f,.14f,.255f})yield return Walk(0,game.Root.TransformPoint(new Vector3(x,-.098f,0)),18,.032f);
            yield return Leave(0);
        }

        [UnityTest] public IEnumerator Level19FullSolutionCutsHoldsPullsReunitesAndExits()
        {
            yield return Load(19);var knife=game.Owner.Apparatus.GetComponentInChildren<COgheGuillotine>();var winch=game.Owner.Apparatus.GetComponentInChildren<COgheCooperativeWinch>();
            game.Motion.Move(0,knife.Sensor.position,true);
            yield return WaitFor(22,()=>game.Matter.TotalFragmentCount>1,"Actual gravity-blade intersection creates independently controllable parts");
            var parts=Anchors();parts.Sort((a,b)=>game.Motion.Centre(a).x.CompareTo(game.Motion.Centre(b).x));
            int left=parts[0],right=parts[parts.Count-1];
            yield return HoldPad(left,winch.Input);
            yield return Pull(right,winch.Handle,1,()=>winch.Complete,28);
            yield return Reunion(game.Root.TransformPoint(new Vector3(.24f,-.274f,.16f)));
            yield return Leave(Anchors()[0]);
        }

        private IEnumerator PrepareBossFirstCut(COgheGuillotine knife)
        {
            // Physical cuts are biased by moving the body relative to the fixed blade, never by assigning groups.
            foreach(float offset in new[]{.009f,-.009f,.016f,-.016f,0f})
            {
                if(game.Matter.TotalFragmentCount>1)yield return Reunion(game.Root.TransformPoint(new Vector3(-.49f,-.274f,.04f)));
                int anchor=Anchors()[0];
                yield return Walk(anchor,game.Root.TransformPoint(new Vector3(-.50f,-.274f,.04f)));
                yield return WaitFor(8,()=>knife.Phase==VenomCampaign.BladePhase.Ready,"Knife clears and rearms before a new physical cut");
                int before=game.Matter.CutCount;game.Motion.Move(anchor,knife.Sensor.position+game.Root.right*offset,true);
                yield return WaitFor(20,()=>game.Matter.CutCount>before,"Biased first cut occurs");
                var parts=Anchors();if(parts.Count!=2)continue;
                int largest=Count(parts[0])>Count(parts[1])?parts[0]:parts[1];int small=largest==parts[0]?parts[1]:parts[0];
                if(Count(largest)>=19&&Count(small)>=7)yield break;
            }
            Assert.Fail("Physical first-cut retries could not produce a usable load allocation; "+State);
        }

        [UnityTest] public IEnumerator Level20FullSolutionUsesMassOrderThreeRolesAndMergedEscape()
        {
            yield return Load(20);var winch=game.Owner.Apparatus.GetComponentInChildren<COgheCooperativeWinch>();
            var knives=game.Owner.Apparatus.GetComponentsInChildren<COgheGuillotine>();System.Array.Sort(knives,(a,b)=>a.Sensor.position.x.CompareTo(b.Sensor.position.x));
            var pipes=game.Owner.Apparatus.GetComponentsInChildren<COgheTubeNetwork>();System.Array.Sort(pipes,(a,b)=>a.Nodes[0].LocalPosition.x.CompareTo(b.Nodes[0].LocalPosition.x));
            yield return PrepareBossFirstCut(knives[0]);var parts=Anchors();parts.Sort((a,b)=>Count(a).CompareTo(Count(b)));int holdA=parts[0],worker=parts[1];
            yield return HoldPad(holdA,winch.Input);yield return ThroughPipe(worker,pipes[0]);
            yield return Walk(worker,game.Root.TransformPoint(new Vector3(0,winch.GearCarriage.Start.y,.015f)),30);
            yield return Pull(worker,winch.GearCarriage,1,()=>winch.GearCarriage.AtEnd,28);
            int oldGroup=game.Matter.Groups[worker];var workerParticles=new HashSet<int>();for(int i=0;i<32;i++)if(game.Matter.Groups[i]==oldGroup)workerParticles.Add(i);
            int oldCuts=game.Matter.CutCount;game.SelectFragment(worker);game.Motion.Move(worker,knives[1].Sensor.position,true);
            yield return WaitFor(25,()=>game.Matter.CutCount>oldCuts&&game.Matter.TotalFragmentCount>=3,"Second physical cut creates the B and C operators");
            Assert.IsTrue(winch.Input.Active,"The scoped second cut must leave the A holder in place");
            var candidates=new List<int>();foreach(int a in Anchors())if(workerParticles.Contains(a))candidates.Add(a);candidates.Sort((a,b)=>Count(b).CompareTo(Count(a)));
            Assert.GreaterOrEqual(candidates.Count,2);int holdB=candidates[0],pullC=candidates[1];
            yield return HoldPad(holdB,winch.Output);yield return ThroughPipe(pullC,pipes[1]);
            yield return Pull(pullC,winch.Handle,1,()=>winch.Complete,30);
            Assert.IsFalse(winch.ExitUnlocked,"The coordinated mechanism only retracts the heavy cover pin");
            yield return Reunion(game.Root.TransformPoint(new Vector3(.42f,-.274f,.12f)));
            int merged=Anchors()[0];yield return Pull(merged,winch.FinalCap,1,()=>winch.FinalCap.AtEnd,30);yield return Leave(merged);
        }

        [UnityTest] public IEnumerator TwoRoomWinchBrakesOnLoadLossAndRequiresBothActualDoors()
        {
            yield return Load(19);var winch=game.Owner.Apparatus.GetComponentInChildren<COgheCooperativeWinch>();
            foreach(var body in game.Matter.Bodies)body.isKinematic=true;
            TissueOn(winch.Input,0,8);Assert.IsTrue(winch.Input.Active);winch.StepMechanism(game,Dt);Assert.IsFalse(winch.Handle.Locked);
            winch.Handle.Body.position=winch.Handle.Frame.TransformPoint(winch.Handle.Start+winch.Handle.Axis*.03f);
            winch.Handle.ApplyEffort(winch.Handle.WorldAxis*.15f);winch.Handle.StepMechanism(game,Dt);winch.StepMechanism(game,Dt);Assert.IsTrue(winch.Engaged);
            foreach(var door in winch.Doors)Assert.IsFalse(door.Locked);
            for(int i=0;i<8;i++)game.Matter.Bodies[i].position=game.Root.TransformPoint(new Vector3(-.35f,-.27f,-.20f));
            winch.Input.StepMechanism(game,Dt);winch.StepMechanism(game,Dt);Assert.IsFalse(winch.Engaged);foreach(var door in winch.Doors)Assert.IsTrue(door.Locked,"A loss applies anti-drop brakes immediately");
            winch.Doors[0].Body.position=winch.Doors[0].Frame.TransformPoint(winch.Doors[0].Start+winch.Doors[0].Axis*winch.Doors[0].Travel);
            winch.StepMechanism(game,Dt);Assert.IsFalse(winch.Complete,"One jammed door prevents the terminal latch");
            winch.Doors[1].Body.position=winch.Doors[1].Frame.TransformPoint(winch.Doors[1].Start+winch.Doors[1].Axis*winch.Doors[1].Travel);
            winch.StepMechanism(game,Dt);Assert.IsTrue(winch.Complete);Assert.IsTrue(winch.ExitUnlocked);
            foreach(var body in game.Matter.Bodies)body.isKinematic=false;
            game.ResetLevel();Assert.IsFalse(winch.Complete);Assert.IsFalse(winch.ExitUnlocked);foreach(var door in winch.Doors)Assert.Less(door.Position,.002f);
        }

        [UnityTest] public IEnumerator BossPreservesThreeIndependentRolesAndNeverAutoOpensHeavyCover()
        {
            yield return Load(20);var winch=game.Owner.Apparatus.GetComponentInChildren<COgheCooperativeWinch>();
            Assert.AreEqual(2,game.Owner.Apparatus.GetComponentsInChildren<COgheGuillotine>().Length);
            Assert.AreEqual(2,game.Owner.Apparatus.GetComponentsInChildren<COgheTubeNetwork>().Length);
            foreach(var body in game.Matter.Bodies)body.isKinematic=true;
            TissueOn(winch.Input,0,7);TissueOn(winch.Output,7,6);
            winch.GearCarriage.Body.position=winch.GearCarriage.Frame.TransformPoint(winch.GearCarriage.Start+winch.GearCarriage.Axis*winch.GearCarriage.Travel);
            // This isolated dependency test intentionally places a fixture without a physics tick.
            // Rigidbody's immediate solver pose is distinct from its child-render transform until simulation.
            winch.GearCarriage.transform.position=winch.GearCarriage.Body.position;
            Physics.SyncTransforms();
            winch.Transmission.StepMechanism(game,Dt);Assert.IsTrue(winch.Transmission.Powered,
                $"Prepared gear train: A={winch.Input.Active}/{winch.Input.Load}, meshed={winch.Transmission.Meshed}, G={winch.GearCarriage.Position}, centres={winch.Transmission.Wheels[0].position}/{winch.Transmission.Wheels[1].position}/{winch.Transmission.Wheels[2].position}");
            winch.StepMechanism(game,Dt);Assert.IsFalse(winch.Engaged,"Pads and prepared gears cannot replace the C operator");
            winch.Handle.Body.position=winch.Handle.Frame.TransformPoint(winch.Handle.Start+winch.Handle.Axis*.03f);winch.Handle.ApplyEffort(winch.Handle.WorldAxis*.12f);winch.Handle.StepMechanism(game,Dt);winch.StepMechanism(game,Dt);Assert.IsTrue(winch.Engaged);
            for(int i=0;i<7;i++)game.Matter.Bodies[i].position=game.Root.TransformPoint(new Vector3(-.46f,-.27f,-.2f));
            winch.Input.StepMechanism(game,Dt);winch.Transmission.StepMechanism(game,Dt);winch.StepMechanism(game,Dt);Assert.IsFalse(winch.Engaged,"A still controls input clutch after G catches");
            foreach(var door in winch.Doors)door.Body.position=door.Frame.TransformPoint(door.Start+door.Axis*door.Travel);
            winch.StepMechanism(game,Dt);Assert.IsTrue(winch.Complete);Assert.IsFalse(winch.FinalCap.Locked);Assert.Less(winch.FinalCap.Position,.002f,"Winch retracts H pin but supplies no force to open H");Assert.IsFalse(winch.ExitUnlocked);
            foreach(var body in game.Matter.Bodies)body.isKinematic=false;
            game.ResetLevel();Assert.IsTrue(winch.FinalCap.Locked);Assert.IsFalse(winch.Complete);
        }

        [UnityTest] public IEnumerator BossFixedViewAcceptsActualHandleAndPadScreenTaps()
        {
            yield return Load(20);
            Assert.IsEmpty(game.Definition.Lesson,"Boss must not disclose its solution as a tutorial");
            COgheExpansionIntegrationTests.Capture(game,"20-fixed-view-controls");
            foreach(var rail in game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())
            {
                var prop=rail.GetComponent<VenomMovableProp>();if(prop==null||!prop.Manipulable)continue;
                game.ReleaseProp();game.Feedback.ResetFeedback();
                game.TouchPoint(game.Owner.View.WorldToScreenPoint(prop.Body.position));
                Assert.AreSame(prop,game.Feedback.CommandSurface?.GetComponentInParent<VenomMovableProp>(),
                    $"Fixed-view tap must select {prop.name}; picked {game.Feedback.CommandSurface?.name}");
            }
            game.ReleaseProp();
            foreach(var pad in game.Owner.Apparatus.GetComponentsInChildren<COgheTissueSensor>())
            {
                game.Feedback.ResetFeedback();game.TouchPoint(game.Owner.View.WorldToScreenPoint(pad.transform.position));
                Assert.IsNotNull(game.Feedback.CommandSurface,$"{pad.name} must be touchable from the fixed view");
                StringAssert.Contains("sensing surface",game.Feedback.CommandSurface.name,$"{pad.name} was obscured by {game.Feedback.CommandSurface.name}");
                Assert.Less(Vector3.Distance(game.Feedback.CommandPoint,pad.transform.position),.025f);
            }
        }
    }
}
