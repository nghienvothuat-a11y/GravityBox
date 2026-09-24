using System;
using System.Collections;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    public sealed class COgheOnboardingTests
    {
        private const float Dt=1f/120;
        private VenomCampaign game;
        private SimulationMode previousMode;
        private bool persistence;
        private int portraitHeight=1280;
        [UnitySetUp] public IEnumerator Before()
        {portraitHeight=1280;previousMode=Physics.simulationMode;persistence=VenomCampaignSave.PersistenceEnabled;Physics.simulationMode=SimulationMode.Script;VenomCampaignSave.PersistenceEnabled=false;yield return null;}
        [UnityTearDown] public IEnumerator After()
        {Time.timeScale=1;Physics.simulationMode=previousMode;VenomCampaignSave.PersistenceEnabled=persistence;yield return null;}
        private IEnumerator Load(int slot)
        {
            void Loaded(Scene scene,LoadSceneMode mode)
            {game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;}
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync($"COgheLearn{slot:00}");}
            finally{SceneManager.sceneLoaded-=Loaded;}
            yield return Delay(1);game.CameraRig.Frame(720,portraitHeight,0,true);game.Onboarding.Refresh();
        }
        private void Tick()
        {game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);if(!game.Owner.Paused)Physics.Simulate(Dt);game.Onboarding.Refresh();}
        private IEnumerator Delay(float seconds)
        {for(int i=0;i<seconds/Dt;i++){Tick();if(i%240==0)yield return null;}}
        private IEnumerator Until(float seconds,Func<bool> done,string reason)
        {
            for(int i=0;i<seconds/Dt&&!done()&&!game.Owner.Lost;i++){Tick();if(i%240==0)yield return null;}
            Assert.IsTrue(done(),reason+$"; level={game.Definition.Order} stage={game.Onboarding.Stage} activity={game.Activity} centre={game.Motion.Centre(0)} failure={game.Failure}");
        }
        private IEnumerator Tap(Vector3 point)
        {game.CameraRig.Frame(720,portraitHeight,0,true);game.TouchPoint(game.Owner.View.WorldToScreenPoint(point));game.Onboarding.Refresh();yield return null;}
        private IEnumerator Release(){game.ReleaseProp();game.Onboarding.Refresh();yield return null;}
        private IEnumerator Solve(int slot)
        {
            yield return Load(slot);
            var initialRotation=game.Root.rotation;
            yield return new COgheOnboardingScenario(game,Tap,Until,Delay,Release).Solve();
            Assert.AreEqual(32,game.Matter.EscapedCount);Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.IsFalse(game.Owner.Lost);
            Assert.Less(Quaternion.Angle(initialRotation,game.Root.rotation),.01f,"Pilot is solvable entirely through taps");
            Assert.IsEmpty(game.Onboarding.Hint);Assert.IsFalse(game.Onboarding.CueVisible);
        }
        [UnityTest] public IEnumerator Pilot01TapExit(){yield return Solve(1);}
        [UnityTest] public IEnumerator Pilot02ClimbThenExit(){yield return Solve(2);}
        [UnityTest] public IEnumerator Pilot03OneTapGear(){yield return Solve(3);}
        [UnityTest] public IEnumerator Pilot04CratePushAndClimb(){yield return Solve(4);}
        [UnityTest] public IEnumerator Pilot04CratePushAndClimbTall(){portraitHeight=1612;yield return Solve(4);}
        [UnityTest] public IEnumerator Pilot05PullCover(){yield return Solve(5);}
        [UnityTest] public IEnumerator Pilot06AssembleBridge(){yield return Solve(6);}
        [UnityTest] public IEnumerator Pilot07MeshGear(){yield return Solve(7);}
        [UnityTest] public IEnumerator Pilot08TwoStageMechanism(){yield return Solve(8);}
        [UnityTest] public IEnumerator Pilot09ReverseTapGear(){yield return Solve(9);}
        [UnityTest] public IEnumerator Pilot10BossWithoutSolutionHints(){yield return Solve(10);}

        [UnityTest] public IEnumerator CrateDestinationAcceptsTapAtBothPortraitRatios()
        {
            foreach(int height in new[]{1280,1612})
            {
                portraitHeight=height;yield return Load(4);yield return Tap(game.Onboarding.Primary.Body.position);
                yield return Until(20,()=>game.Attached,"real crate grasp");Assert.IsFalse(game.HasPropTarget);
                game.CameraRig.Frame(720,height,0,true,new Rect(0,0,720,height));
                Vector3 cue=game.Root.TransformPoint(game.Onboarding.LocalDestination);
                game.TouchPoint(game.Owner.View.WorldToScreenPoint(cue));
                Assert.IsTrue(game.HasPropTarget,$"At {height}, the indicated pane must accept the target tap");
                Vector3 accepted=game.Root.InverseTransformPoint(game.Feedback.CommandPoint);
                Assert.AreEqual(.3f,accepted.x,.002f);Assert.AreEqual(.008f,accepted.z,.002f);
            }
        }

        [UnityTest] public IEnumerator ClimbCueHitsItsActualPaneAtBothPortraitRatios()
        {
            yield return Load(2);
            foreach(int height in new[]{1280,1612})
            {
                game.ResetLevel();game.CameraRig.Frame(720,height,0,true,new Rect(0,28,720,height-72));
                game.Onboarding.Refresh();Vector3 cue=game.Onboarding.CuePoint;
                game.TouchPoint(game.Owner.View.WorldToScreenPoint(cue));
                Assert.NotNull(game.Feedback.CommandSurface);
                Assert.Less(Vector3.Distance(game.Feedback.CommandPoint,cue),.002f,$"At {height}: pictured {cue}, received {game.Feedback.CommandPoint} on {game.Feedback.CommandSurface.name}");
                Assert.Greater(Vector3.Dot(game.Feedback.CommandSurface.Normal,Vector3.back),.99f,"Teach on the visible rear pane");
            }
        }

        [UnityTest] public IEnumerator PilotNavigationPreservesContentIdsAndBounds()
        {
            string[] ids={"venom.origin.01","venom.origin.02","coghe.tap.v1.01","venom.origin.07","venom.origin.24","venom.origin.26","venom.origin.23","venom.origin.25","coghe.tap.v1.02","venom.origin.30"};
            for(int slot=1;slot<=10;slot++)
            {
                yield return Load(slot);
                Assert.AreEqual(ids[slot-1],game.Definition.Id);Assert.AreEqual(slot,game.Definition.Order);
                Assert.AreEqual(10,game.PlayableLevelCount);Assert.AreEqual(slot==10,game.Definition.Boss);
                Assert.AreEqual("coghe.onboarding.pilot.v1",game.Definition.ProgressKey);Assert.IsFalse(game.Definition.CanRotate);
                game.Load(slot==10?1:slot+1);yield return null;
                Assert.AreEqual($"COgheLearn{(slot==10?1:slot+1):00}",SceneManager.GetActiveScene().name);
                game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;
                var current=SceneManager.GetActiveScene().name;game.Load(11);game.Load(0);yield return null;Assert.AreEqual(current,SceneManager.GetActiveScene().name);
            }
        }
        [UnityTest] public IEnumerator LessonWaitsForSuccessAndSkipReplayPauseRetryDoNotIssueCommands()
        {
            yield return Load(1);Assert.AreEqual("exit",game.Onboarding.Stage);Assert.IsTrue(game.Onboarding.CueVisible);
            yield return Delay(12);Assert.AreEqual("exit",game.Onboarding.Stage,"Waiting cannot complete a lesson");
            var positions=new Vector3[32];for(int i=0;i<32;i++)positions[i]=game.Matter.Bodies[i].position;
            game.Onboarding.Toggle();Assert.IsEmpty(game.Onboarding.Hint);Assert.IsFalse(game.Onboarding.CueVisible);
            game.Onboarding.Toggle();Assert.IsTrue(game.Onboarding.CueVisible);Assert.IsNull(game.Motion.Get(0));
            for(int i=0;i<32;i++)Assert.AreEqual(positions[i],game.Matter.Bodies[i].position);
            game.Owner.TogglePause();game.Onboarding.Refresh();Assert.IsEmpty(game.Onboarding.Hint);Assert.IsFalse(game.Onboarding.CueVisible);
            game.Owner.TogglePause();game.ResetLevel();game.Onboarding.Refresh();Assert.AreEqual("exit",game.Onboarding.Stage);
            yield return Tap(game.Owner.Outlet.position);Assert.AreEqual("exit-moving",game.Onboarding.Stage);Assert.AreEqual(COgheOnboarding.CueKind.None,game.Onboarding.Cue);
            yield return Until(35,()=>game.Owner.Completed,"accepted exit command solves without tutorial intervention");
            game.ResetLevel();Assert.IsEmpty(game.Onboarding.Hint,"Completed content does not repeat guidance automatically");
        }
        [UnityTest] public IEnumerator RailLessonRequiresRealGraspAndRealDock()
        {
            yield return Load(5);var lesson=game.Onboarding;
            Assert.AreEqual("move-rail-grasp",lesson.Stage);
            yield return Tap(lesson.CuePoint);Assert.AreEqual("move-rail-grasp",lesson.Stage,"A click alone is not a successful grasp");
            yield return Until(25,()=>game.Attached,"physical grasp");Assert.AreEqual("move-rail-target",lesson.Stage);
            Assert.AreEqual(COgheOnboarding.CueKind.Destination,lesson.Cue);
            yield return Delay(1);Assert.IsTrue(game.Attached);Assert.IsFalse(game.HasPropTarget);
            Assert.AreEqual(COgheOnboarding.CueKind.Destination,lesson.Cue,"Grasp jitter must not hide the second tap instruction");
            yield return new COgheOnboardingScenario(game,Tap,Until,Delay,Release).Solve();
        }
        [UnityTest] public IEnumerator BossReplayPauseAndWaitingNeverRevealSolution()
        {
            yield return Load(10);game.Onboarding.Toggle();yield return Delay(12);
            Assert.IsEmpty(game.Onboarding.Hint);Assert.IsFalse(game.Onboarding.CueVisible);
            game.Owner.TogglePause();game.Onboarding.Refresh();Assert.IsEmpty(game.Onboarding.Hint);
        }
        [Test] public void SaveInstancesRetainTheirNamespaceAndStableIds()
        {
            string prefix="coghe.test.onboarding."+Guid.NewGuid().ToString("N");
            string before=PlayerPrefs.GetString(VenomCampaignSave.Key,"");
            bool old=VenomCampaignSave.PersistenceEnabled;var definition=ScriptableObject.CreateInstance<VenomCampaignDefinition>();
            try
            {
                VenomCampaignSave.PersistenceEnabled=true;
                var productionFixture=VenomCampaignSave.Read(prefix+".old");var pilot=VenomCampaignSave.Read(prefix+".pilot");
                definition.Id="venom.origin.30";definition.Boss=true;definition.Order=10;
                productionFixture.Win(definition);definition.Order=60;productionFixture.Win(definition);
                Assert.AreEqual(1,VenomCampaignSave.Read(prefix+".old").Completed.Count);Assert.IsTrue(VenomCampaignSave.Read(prefix+".old").HomeUnlocked);
                definition.Id="venom.origin.01";definition.Boss=false;pilot.Win(definition);
                Assert.IsFalse(VenomCampaignSave.Read(prefix+".pilot").HomeUnlocked);Assert.AreEqual(new[]{"venom.origin.01"},VenomCampaignSave.Read(prefix+".pilot").Completed);
                Assert.AreEqual(before,PlayerPrefs.GetString(VenomCampaignSave.Key,""),"Experiments never write the real production key");
            }
            finally
            {PlayerPrefs.DeleteKey(prefix+".old");PlayerPrefs.DeleteKey(prefix+".pilot");PlayerPrefs.Save();Object.DestroyImmediate(definition);VenomCampaignSave.PersistenceEnabled=old;}
        }
    }
}
