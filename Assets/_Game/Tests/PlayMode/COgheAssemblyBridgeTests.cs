using System.Collections;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed class COgheAssemblyBridgeTests
    {
        const float Dt=1f/120;
        VenomCampaign game;COgheAssemblyBridge bridge;SimulationMode previous;
        [UnitySetUp] public IEnumerator Before()
        {
            previous=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;VenomCampaignSave.PersistenceEnabled=false;
            void Loaded(Scene scene,LoadSceneMode mode)
            {game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;}
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync("VenomOrigin16");}finally{SceneManager.sceneLoaded-=Loaded;}
            bridge=Object.FindFirstObjectByType<COgheAssemblyBridge>();Assert.NotNull(bridge);Steps(120);
        }
        [UnityTearDown] public IEnumerator After()
        {Physics.simulationMode=previous;VenomCampaignSave.PersistenceEnabled=true;Time.timeScale=1;yield return null;}
        void Steps(int n){for(int i=0;i<n;i++){game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);Physics.Simulate(Dt);}}
        string State=>$"centre={game.Motion.Centre(0):F3}; attached={game.Attached}; order={game.Motion.Get(0)?.Cursor}/{game.Motion.Get(0)?.Path.Count}; loss={game.Failure}";
        IEnumerator Wait(float seconds,System.Func<bool> done,string reason,System.Action refresh=null)
        {
            for(int i=0;i<seconds/Dt&&!done()&&!game.Owner.Lost;i++)
            {if(i%120==0)refresh?.Invoke();Steps(1);if(i%240==0)yield return null;}
            if(!done())COgheExpansionIntegrationTests.Capture(game,"16-failed-"+TestContext.CurrentContext.Test.Name);
            Assert.IsTrue(done(),reason+"; "+State);
        }
        void Tap(Vector3 point){game.TouchPoint(game.Owner.View.WorldToScreenPoint(point));}
        IEnumerator Seat(int i,bool screenInput=true)
        {
            var rail=bridge.Rails[i];var prop=rail.GetComponent<VenomMovableProp>();
            if(screenInput)Tap(prop.ManipulationGrip.position);else game.SelectProp(prop);
            yield return Wait(18,()=>game.Attached,"Approach handle "+i);
            // A real floor command in front of the socket drives the Z slider.
            Vector3 target=new Vector3(rail.Start.x,-.10f,.05f);
            if(screenInput)Tap(game.Root.TransformPoint(target));else game.SetPropTarget(game.Root.TransformPoint(target));
            yield return Wait(12,()=>rail.AtEnd&&rail.Latched,"Seat "+i+" position="+rail.Position,()=>
            {if(screenInput)Tap(game.Root.TransformPoint(target));else game.SetPropTarget(game.Root.TransformPoint(target));});
            game.ReleaseProp();Steps(60);
        }
        [UnityTest] public IEnumerator IdleCannotAssembleAndDirectExitCannotBypassTheGap()
        {
            Assert.IsFalse(game.Definition.CanRotate);Assert.IsFalse(game.Definition.Passive);
            Assert.IsNull(Object.FindFirstObjectByType<COgheTubeNetwork>());
            Steps(600);Assert.AreEqual(0,bridge.SeatedCount);Assert.IsFalse(game.Owner.Completed);Assert.IsFalse(game.Owner.Lost);
            Tap(game.Owner.Outlet.position);Steps(2400);
            Assert.IsFalse(game.Owner.Completed,"The slick wall must not bypass the elevated bridge");Assert.IsFalse(game.Owner.Lost);
            COgheExpansionIntegrationTests.Capture(game,"16-unassembled");yield return null;
        }
        [UnityTest] public IEnumerator ScreenCommandsAssembleThreePiecesThenPhysicallyCrossAndExit()
        {
            for(int i=0;i<3;i++)yield return Seat(i);
            Assert.IsTrue(bridge.Ready);
            COgheExpansionIntegrationTests.Capture(game,"16-assembled");
            foreach(var point in new[]{new Vector3(.14f,-.278f,-.22f),new Vector3(-.255f,-.278f,-.22f),
                new Vector3(-.255f,-.19f,-.202f),new Vector3(-.255f,-.078f,-.12f),new Vector3(-.255f,-.078f,0)})
            {
                game.Motion.Move(0,point);
                yield return Wait(15,()=>Vector3.Distance(game.Motion.Centre(0),point)<.035f,"Walk / climb "+point);
            }
            Tap(game.Owner.Outlet.position);
            yield return Wait(30,()=>game.Owner.Completed,"Traverse the real deck and escape");
            Assert.AreEqual(32,game.Matter.EscapedCount);Assert.IsFalse(game.Owner.Lost);
            COgheExpansionIntegrationTests.Capture(game,"16-complete");
            game.ResetLevel();Assert.IsFalse(bridge.Ready);Assert.AreEqual(0,game.Matter.EscapedCount);
            foreach(var rail in bridge.Rails){Assert.Less(rail.Position,.0001f);Assert.IsFalse(rail.Latched);}
            Steps(120);foreach(var rail in bridge.Rails)Assert.Less(rail.Position,.004f,"Spawn must not push the reset bridge.");
        }
        [UnityTest] public IEnumerator ReversePullReleasesSeatAndIdleLetsGo()
        {
            yield return Seat(0);
            var rail=bridge.Rails[0];var prop=rail.GetComponent<VenomMovableProp>();
            Tap(prop.ManipulationGrip.position);yield return Wait(15,()=>game.Attached,"Regrasp seated A");
            Vector3 target=new Vector3(rail.Start.x,-.10f,-.285f);
            Tap(target);yield return Wait(12,()=>rail.Position<.035f,"Pull A away from socket",()=>Tap(target));
            Assert.IsFalse(rail.Latched);Steps(400);Assert.IsFalse(game.Attached,"Three idle seconds release the handle");
            Assert.IsFalse(game.Owner.Lost);game.ResetLevel();Steps(30);Assert.AreEqual(0,bridge.SeatedCount);
        }
    }
}
