using System;
using System.Collections;
using System.Collections.Generic;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    public sealed class COgheCampaign40RouteTests
    {
        private const float Dt=1f/120f;
        private VenomCampaign game;
        private SimulationMode previous;
        private bool persistence;
        [UnitySetUp] public IEnumerator Before()
        {previous=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;persistence=VenomCampaignSave.PersistenceEnabled;VenomCampaignSave.PersistenceEnabled=false;yield return null;}
        [UnityTearDown] public IEnumerator After()
        {Time.timeScale=1;Physics.simulationMode=previous;VenomCampaignSave.PersistenceEnabled=persistence;yield return null;}
        private IEnumerator Load(int level)
        {
            void Loaded(Scene s,LoadSceneMode m)
            {game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;}
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync($"COgheOrigin{level:00}");}
            finally{SceneManager.sceneLoaded-=Loaded;}
            yield return null;
        }
        private void Tick()
        {
            game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);Physics.Simulate(Dt);
            Assert.IsFalse(game.Owner.Lost,"Unexpected failure: "+game.Failure);
            foreach(var body in game.Matter.Bodies)
                Assert.IsFalse(float.IsNaN(body.position.x)||float.IsInfinity(body.position.x),"Finite physical tissue state");
        }
        private string State()
        {
            var rails=new List<string>();
            foreach(var rail in game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())rails.Add($"{rail.name}={rail.Position:F4}/{rail.Travel:F3} locked={rail.Locked} F={rail.Effort:F4}");
            return $"body={game.Motion.Centre(0):F3}; attached={game.Attached}; groups={game.Matter.TotalFragmentCount}; escaped={game.Matter.EscapedCount}; "+string.Join("; ",rails);
        }
        private IEnumerator Wait(float seconds,Func<bool> done,string reason,Action refresh=null)
        {
            for(int i=0;i<seconds/Dt&&!done();i++)
            {if(i%120==0)refresh?.Invoke();Tick();if(i%240==0)yield return null;}
            if(!done())
            {
                COgheExpansionIntegrationTests.Capture(game,$"chapter40-{game.Definition.Order}-failed");
                foreach(var net in game.Root.GetComponentsInChildren<COgheTubeNetwork>())
                    if(net.AnyTravelling||net.AnyApproaching)Debug.Log("CHAPTER_PIPE_STALLED "+net.name+" "+net.DebugState(0));
                foreach(var rail in game.Root.GetComponentsInChildren<COgheRailSlider>())
                foreach(var a in rail.GetComponentsInChildren<Collider>())
                foreach(var b in game.Root.GetComponentsInChildren<Collider>())
                {
                    if(!a.enabled||!b.enabled||a.attachedRigidbody==b.attachedRigidbody||!a.bounds.Intersects(b.bounds))continue;
                    if(Physics.ComputePenetration(a,a.transform.position,a.transform.rotation,b,b.transform.position,b.transform.rotation,out var push,out float depth)&&depth>.001f)
                        Debug.Log($"CHAPTER_CONTACT {rail.name}/{a.name} vs {b.name}: {depth:F4} along {push:F3}");
                }
            }
            Assert.IsTrue(done(),reason+"; "+State());
        }
        private COgheRailSlider Rail(string name)=>Array.Find(game.Props,p=>p.name==name).GetComponent<COgheRailSlider>();
        private IEnumerator Pull(COgheRailSlider rail,float direction,Func<bool> done,bool screen=false)
        {
            var prop=rail.GetComponent<VenomMovableProp>();
            game.SelectFragment(0);
            if(screen){game.CameraRig.Frame(720,1280,0,true);game.TouchPoint(game.Owner.View.WorldToScreenPoint(prop.ManipulationGrip.position));}
            else game.SelectProp(prop);
            yield return Wait(30,()=>game.Attached,"Reach "+rail.name);
            Vector3 end=rail.Frame.TransformPoint(rail.Start+rail.Axis*(direction>0?rail.Travel+.12f:-.12f));
            game.SetPropTarget(end);
            yield return Wait(18,done,"One grasp operates "+rail.name,()=>game.SetPropTarget(end));
            Assert.IsTrue(game.Attached,"No repeated regrasp to compensate for a bad route: "+State());
            game.ReleaseProp();
        }
        private IEnumerator Walk(Vector3 point,float tolerance=.04f)
        {game.Motion.Move(0,game.Root.TransformPoint(point),true);yield return Wait(35,()=>Vector3.Distance(game.Motion.Centre(0),game.Root.TransformPoint(point))<tolerance,"Walk "+point);}
        private IEnumerator Leave()
        {
            Assert.IsTrue(game.FinalExitAvailable,"Mechanisms must open before the exit command");
            Assert.AreEqual(1,game.Matter.TotalFragmentCount);
            COgheExpansionIntegrationTests.Capture(game,$"chapter40-{game.Definition.Order}-open");
            game.Motion.Move(0,game.Owner.Outlet.position-game.Owner.Outlet.forward*.024f,false,true);
            yield return Wait(45,()=>game.Owner.Completed,"All matter passes the real exit");
            Assert.AreEqual(32,game.Matter.EscapedCount);
            COgheExpansionIntegrationTests.Capture(game,$"chapter40-{game.Definition.Order}-won");
            Debug.Log($"CHAPTER40 VERIFIED WIN {game.Definition.Order} escaped={game.Matter.EscapedCount}");
            game.ResetLevel();for(int i=0;i<120;i++)Tick();
            Assert.IsFalse(game.FinalExitAvailable);Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.AreEqual(0,game.Matter.EscapedCount);
            foreach(var rail in game.Root.GetComponentsInChildren<COgheRailSlider>())
                Assert.That(rail.Position,Is.EqualTo(rail.InitialTravel).Within(.005f),"Retry restores "+rail.name);
        }

        [UnityTest] public IEnumerator Level31RetractsExposesPinThenMeshesAndWins()
        {
            yield return Load(31);
            var g=Rail("G access carriage");var p=Rail("P retracting pin");
            Assert.IsFalse(game.FinalExitAvailable);
            yield return Pull(g,-1,()=>g.Position<.004f);
            yield return Pull(p,1,()=>p.AtEnd);
            yield return Pull(g,1,()=>g.AtEnd);
            yield return Wait(10,()=>game.FinalExitAvailable,"Meshed train raises E");
            yield return Leave();
            game.ResetLevel();for(int i=0;i<120;i++)Tick();
            Assert.IsFalse(game.FinalExitAvailable);Assert.Less(p.Position,.004f);Assert.That(g.Position,Is.EqualTo(g.InitialTravel).Within(.005f));
        }

        [UnityTest] public IEnumerator Level32UsesAThenReturnsToMoveTheSameBridgeToBAndWins()
        {
            yield return Load(32);var bridge=Rail("B reusable bridge");
            yield return Pull(bridge,1,()=>bridge.AtEnd);
            yield return Walk(new Vector3(.24f,-.14f,-.16f));
            var sequence=Object.FindFirstObjectByType<COgheSequentialWinch>();
            yield return Pull(sequence.Handle,1,()=>sequence.Complete);
            yield return Walk(new Vector3(-.26f,-.14f,-.16f));
            yield return Pull(bridge,-1,()=>bridge.Position<.003f);
            yield return Walk(new Vector3(.25f,-.14f,.16f));
            yield return Leave();
        }

        private IEnumerator Transfer(COgheTubeNetwork net,Vector3 destination)
        {
            Vector3 local=game.Root.InverseTransformPoint(game.Motion.Centre(0));
            int entry=Vector3.Distance(local,net.Nodes[0].LocalPosition)<Vector3.Distance(local,net.Nodes[1].LocalPosition)?0:1;
            yield return Walk(net.Nodes[entry].LocalPosition+net.Nodes[entry].LocalOutward*.025f,.035f);
            Assert.IsTrue(net.TryChoose(0,0),"Choose a physically adjacent transfer pipe");
            yield return Wait(30,()=>net.AnyTravelling,"Enter pipe; "+net.DebugEntryState(0,0));
            yield return Wait(30,()=>!net.AnyTravelling&&!net.AnyApproaching,"Exit pipe; "+net.DebugState(0));
            Assert.AreEqual(1-entry,net.LastReachedNode,"Transfer must reach its opposite mouth; "+State()+"; "+net.DebugState(0));
            Debug.Log($"CHAPTER_PIPE_ARRIVAL {net.name} node={net.LastReachedNode} body={game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3}");
            yield return Walk(destination);
        }
        [UnityTest] public IEnumerator Level33ChoosesAOpensEReturnsToSAndChoosesBToWin()
        {
            yield return Load(33);var selector=Rail("T route selector");
            var nets=game.Root.GetComponentsInChildren<COgheTubeNetwork>();
            var a=Array.Find(nets,n=>n.name.StartsWith("A "));var b=Array.Find(nets,n=>n.name.StartsWith("B "));
            Assert.IsFalse(a.IsEntryOpen(0));Assert.IsTrue(b.IsEntryOpen(0));
            yield return Pull(selector,-1,()=>selector.Position<.003f);
            Assert.IsTrue(a.IsEntryOpen(0));Assert.IsFalse(b.IsEntryOpen(0));
            yield return Transfer(a,new Vector3(-.30f,.045f,.08f));
            var sequence=Object.FindFirstObjectByType<COgheSequentialWinch>();
            yield return Pull(sequence.Handle,1,()=>sequence.Complete);
            yield return Transfer(a,new Vector3(-.25f,-.277f,.15f));
            yield return Pull(selector,1,()=>selector.AtEnd);
            yield return Transfer(b,new Vector3(.30f,.045f,.12f));
            yield return Leave();
        }

        [UnityTest] public IEnumerator Level34RetractsBParksXPassesGReturnsXRestoresBAndWins()
        {
            yield return Load(34);var b=Rail("B bay bridge");var x=Rail("X crossing block");var g=Rail("G crossing carriage");
            yield return Pull(b,-1,()=>b.Position<.004f);
            yield return Pull(x,1,()=>x.AtEnd);
            yield return Pull(g,1,()=>g.AtEnd);
            yield return Wait(10,()=>game.FinalExitAvailable,"Gear train raises E");
            Vector3 near=game.Root.TransformPoint(new Vector3(.25f,-.08f,.14f));
            bool blocked=false;
            foreach(var hit in Physics.SphereCastAll(near,.009f,game.Root.right,.09f))
                blocked|=hit.collider.transform.IsChildOf(b.transform);
            Assert.IsTrue(blocked,"Retracted B must still physically cover the exit island aperture");
            yield return Pull(x,-1,()=>x.Position<.004f);
            yield return Pull(b,1,()=>b.AtEnd);
            yield return Walk(new Vector3(.36f,-.14f,.14f));
            yield return Leave();
        }

        [UnityTest] public IEnumerator ChapterScenesHaveStableIdsIdleSafelyAndResetAllMatter()
        {
            for(int level=31;level<=40;level++)
            {
                yield return Load(level);
                Assert.AreEqual($"venom.origin.{level}",game.Definition.Id);Assert.AreEqual(level,game.Definition.Order);
                Assert.IsFalse(game.Definition.CanRotate);Assert.AreEqual(level==40,game.Definition.Boss);
                if(level==40)Assert.IsEmpty(game.Definition.Lesson,"Boss gives no solution hint");
                for(int i=0;i<240;i++)Tick();
                Assert.IsFalse(game.Owner.Completed);Assert.AreEqual(32,game.Matter.Bodies.Length);
                game.ResetLevel();for(int i=0;i<120;i++)Tick();
                Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.AreEqual(0,game.Matter.EscapedCount);
                Assert.IsFalse(game.FinalExitAvailable);
                COgheExpansionIntegrationTests.Capture(game,$"chapter40-{level}-start");
            }
        }
    }
}
