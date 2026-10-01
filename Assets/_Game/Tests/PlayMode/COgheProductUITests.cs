using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    public class COgheProductUITests
    {
        private VenomCampaign game;
        private COgheProductUI ui;
        private bool persistence,seen,music,sound;
        private Mouse mouse;
        private InputSettings.BackgroundBehavior background;
        private InputSettings.EditorInputBehaviorInPlayMode editorInput;
        [UnitySetUp] public IEnumerator Before()
        {
            persistence=VenomCampaignSave.PersistenceEnabled;seen=COgheIntro.Seen;music=COgheAudio.MusicOn;sound=COgheAudio.EffectsOn;
            editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            background=InputSystem.settings.backgroundBehavior;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            VenomCampaignSave.PersistenceEnabled=false;COgheProductMode.OverrideForTests=true;COgheIntro.Seen=true;
            mouse=InputSystem.AddDevice<Mouse>();
            yield return SceneManager.LoadSceneAsync("COgheSpatial01");yield return null;
            game=Object.FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            Assert.IsNotNull(ui);ui.ShowMenu();yield return new WaitForSecondsRealtime(.1f);
        }
        [UnityTearDown] public IEnumerator After()
        {
            if(COgheIntro.Playing){var intro=Object.FindFirstObjectByType<COgheIntro>();intro.Speed=100;intro.Skip();while(COgheIntro.Playing)yield return null;}
            if(ui!=null){ui.Resume();ui.enabled=false;}
            COgheProductMode.OverrideForTests=false;
            yield return SceneManager.LoadSceneAsync("COgheSpatial01");
            COgheProductMode.OverrideForTests=null;VenomCampaignSave.PersistenceEnabled=persistence;
            COgheIntro.Seen=seen;COgheAudio.MusicOn=music;COgheAudio.EffectsOn=sound;
            InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;InputSystem.settings.backgroundBehavior=background;if(mouse!=null)InputSystem.RemoveDevice(mouse);Time.timeScale=1;
        }
        private IEnumerator Click(string name)
        {
            Canvas.ForceUpdateCanvases();var target=ui.GetComponentsInChildren<Button>().Single(b=>b.name==name);
            Vector2 p=RectTransformUtility.WorldToScreenPoint(null,target.transform.TransformPoint(((RectTransform)target.transform).rect.center));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=1});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;yield return null;
        }
        [UnityTest] public IEnumerator RealMenuCreatureDoesNotUnlockOrCompletePuzzles()
        {
            Assert.AreEqual(COgheProductPage.MainMenu,ui.Page);Assert.IsTrue(game.Home);
            Assert.Greater(game.Matter.GetComponent<VenomSurface>().VertexCount,100);
            float before=game.Matter.SimulationTime;yield return new WaitForSecondsRealtime(.2f);
            Assert.Greater(game.Matter.SimulationTime,before,"Menu tissue is live, not a screenshot");
            yield return new WaitForSecondsRealtime(3.1f);Assert.AreEqual(0,game.Greeting);
            yield return Click("Greet COghe");Assert.Greater(game.Greeting,0);
            Assert.AreEqual(0,game.Progress.Completed.Count);Assert.IsFalse(game.Progress.HomeUnlocked);
            Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b=>b.name.Contains("Level ")));
        }
        [UnityTest] public IEnumerator PixelPauseStopsSimulationAndNeverIssuesWorldCommand()
        {
            yield return Click("Play");Assert.AreEqual(COgheProductPage.Game,ui.Page);Assert.IsFalse(game.Home);
            int commands=game.Feedback.CommandCount;yield return Click("Pause");
            Assert.AreEqual(COgheProductPopup.Pause,ui.Popup);Assert.IsTrue(game.Owner.Paused);
            float time=game.Matter.SimulationTime;var positions=game.Matter.Bodies.Select(b=>b.position).ToArray();
            yield return new WaitForSecondsRealtime(.2f);Assert.AreEqual(time,game.Matter.SimulationTime);
            for(int i=0;i<32;i++)Assert.AreEqual(positions[i],game.Matter.Bodies[i].position);
            yield return Click("How to play");Assert.AreEqual(COgheProductPopup.Help,ui.Popup);
            yield return Click("Got it");yield return Click("Resume");
            Assert.IsFalse(game.Owner.Paused);Assert.AreEqual(commands,game.Feedback.CommandCount,"UI clicks never reach the glass");
        }
        [UnityTest] public IEnumerator TouchscreenPauseAndResumeConsumeTheEntireTouch()
        {
            ui.Play();yield return null;var touch=InputSystem.AddDevice<Touchscreen>();
            try
            {
                int commands=game.Feedback.CommandCount;
                foreach(string name in new[]{"Pause","Resume"})
                {
                    Canvas.ForceUpdateCanvases();var target=ui.GetComponentsInChildren<Button>().Single(b=>b.name==name);
                    var rect=(RectTransform)target.transform;Vector2 p=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
                    InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Began,position=p});yield return null;
                    InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=p});yield return null;yield return null;
                    Assert.AreEqual(name=="Pause",game.Owner.Paused,name);
                }
                Assert.AreEqual(commands,game.Feedback.CommandCount,"Touchscreen UI never guides the creature");
            }
            finally{InputSystem.RemoveDevice(touch);}
        }
        [UnityTest] public IEnumerator AudioSwitchesIndependentAndRestartNeedsConfirmation()
        {
            ui.Play();ui.ShowPopup(COgheProductPopup.Pause);
            bool m=COgheAudio.MusicOn,s=COgheAudio.EffectsOn;
            yield return Click("Music");Assert.AreEqual(!m,COgheAudio.MusicOn);Assert.AreEqual(s,COgheAudio.EffectsOn);
            yield return Click("Sound");Assert.AreEqual(!s,COgheAudio.EffectsOn);Assert.AreEqual(!m,COgheAudio.MusicOn);
            yield return Click("Restart");Assert.AreEqual(COgheProductPopup.Restart,ui.Popup);
            yield return Click("Cancel");Assert.AreEqual(COgheProductPopup.Pause,ui.Popup);
            yield return Click("Restart");yield return Click("Confirm");
            Assert.AreEqual(COgheProductPage.Game,ui.Page);Assert.AreEqual(COgheProductPopup.None,ui.Popup);Assert.IsFalse(game.Owner.Paused);
            Assert.AreEqual(0,game.Matter.EscapedCount);Assert.AreEqual(0,game.Progress.Completed.Count);
        }
        [UnityTest] public IEnumerator IntroReplayReturnsToMenuAndFirstRunHandsOverToPuzzle()
        {
            yield return Click("Intro");Assert.AreEqual(COgheProductPage.Intro,ui.Page);
            var intro=Object.FindFirstObjectByType<COgheIntro>();
            while(!intro.Started)yield return null;intro.Speed=20;intro.Skip();
            while(COgheIntro.Playing)yield return null;
            Assert.AreEqual(COgheProductPage.MainMenu,ui.Page);Assert.IsFalse(game.Owner.Paused);
            COgheIntro.Seen=false;ui.Play();intro=Object.FindFirstObjectByType<COgheIntro>();
            while(!intro.Started)yield return null;intro.Speed=20;intro.Skip();
            while(COgheIntro.Playing)yield return null;
            Assert.AreEqual(COgheProductPage.Game,ui.Page);Assert.IsFalse(game.Owner.Paused);Assert.IsFalse(game.Home);
        }
        [UnityTest] public IEnumerator HomeLockAndMenuRoundTripLeavePuzzlePhysicsIntact()
        {
            var patches=game.Surfaces;yield return Click("Home");Assert.AreEqual(COgheProductPopup.Locked,ui.Popup);
            yield return Click("Got it");Assert.AreEqual(COgheProductPage.MainMenu,ui.Page);
            game.Progress.HomeUnlocked=true; // fixture: UI unlock state, not a claim that Boss 10 was solved
            yield return Click("Home");Assert.AreEqual(COgheProductPage.Home,ui.Page);
            yield return Click("Feed");yield return Click("Play");Assert.Greater(game.Greeting,0);
            yield return Click("Back");yield return Click("Play");
            Assert.IsFalse(game.Home);Assert.IsTrue(game.Owner.Apparatus.gameObject.activeSelf);Assert.AreEqual(32,game.Matter.Bodies.Length);
            foreach(var collider in game.Matter.Bodies.Select(b=>b.GetComponent<Collider>()))Assert.IsTrue(collider.enabled);
            Assert.AreEqual(0,game.Progress.Completed.Count);
        }
        [UnityTest] public IEnumerator GuidingThroughExitCelebratesAndAutomaticallyLoadsNextPuzzle()
        {
            yield return Click("Play");yield return new WaitForSecondsRealtime(.25f);
            Vector2 p=game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position);
            Assert.IsTrue(ui.AllowsWorldPointer(p));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=1});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;
            float end=Time.realtimeSinceStartup+40;
            while(!game.Owner.Completed&&Time.realtimeSinceStartup<end)yield return null;
            Assert.IsTrue(game.Owner.Completed,"Actual crawl and exit must complete, without teleporting or forcing Win");
            yield return null;Assert.AreEqual(COgheProductPage.Victory,ui.Page);
            Assert.IsTrue(game.Progress.Completed.Contains(game.Definition.Id));
            end=Time.realtimeSinceStartup+8;
            while(game!=null&&game.Definition.Order==1&&Time.realtimeSinceStartup<end)yield return null;
            yield return null;game=Object.FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            Assert.AreEqual(2,game.Definition.Order);Assert.AreEqual(COgheProductPage.Game,ui.Page);
            Assert.IsFalse(game.Owner.Paused);Assert.IsNotNull(UnityEngine.EventSystems.EventSystem.current);
            yield return Click("Pause");Assert.AreEqual(COgheProductPopup.Pause,ui.Popup);
        }
        [UnityTest] public IEnumerator FragmentButtonsSelectPartsWithoutIssuingAMove()
        {
            ui.Play();game.Owner.enabled=false;
            // UI fixture only; splitter physics is covered by the existing campaign tests.
            var half=new bool[32];for(int i=0;i<16;i++)half[i]=true;game.Matter.Partition(0,half);
            yield return new WaitForSecondsRealtime(.15f);
            int before=game.Feedback.CommandCount;yield return Click("Fragment 2");
            Assert.AreEqual(16,game.Motion.Selected);Assert.AreEqual(before,game.Feedback.CommandCount);
            yield return Click("Fragment 1");Assert.AreEqual(0,game.Motion.Selected);
        }
        [UnityTest] public IEnumerator AllFiftyScenesExposeProductControlsInsideTheSafeArea()
        {
            string[] scenes=ui.Catalog.Levels[0].SceneSequence;
            Assert.AreEqual(50,scenes.Length);
            for(int i=0;i<scenes.Length;i++)
            {
                yield return SceneManager.LoadSceneAsync(scenes[i]);yield return null;
                game=Object.FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
                if(game.Definition.Boss)
                {
                    // Boss levels open with their tour and warning first, the level paused underneath
                    var boss=COgheBossIntro.Current;Assert.IsNotNull(boss,scenes[i]+" opens with its tour");Assert.AreEqual(COgheProductPage.Intro,ui.Page,scenes[i]);
                    boss.Speed=30;float until=Time.realtimeSinceStartup+6;
                    while(COgheBossIntro.Current!=null&&Time.realtimeSinceStartup<until)yield return null;
                    Assert.IsNull(COgheBossIntro.Current,scenes[i]+" tour ends");Assert.IsFalse(game.Owner.Paused,scenes[i]);
                }
                Assert.IsNotNull(ui,scenes[i]);Assert.AreEqual(COgheProductPage.Game,ui.Page,scenes[i]);
                Assert.AreEqual(i+1,game.Definition.Order);Canvas.ForceUpdateCanvases();
                foreach(var button in ui.GetComponentsInChildren<Button>())
                {
                    var rect=(RectTransform)button.transform;
                    Assert.GreaterOrEqual(rect.rect.width,48,scenes[i]+" "+button.name);
                    Assert.GreaterOrEqual(rect.rect.height,48,scenes[i]+" "+button.name);
                    Vector2 point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
                    Assert.IsTrue(RectTransformUtility.RectangleContainsScreenPoint(ui.SafeRoot,point),scenes[i]+" "+button.name);
                    Assert.IsFalse(ui.AllowsWorldPointer(point),scenes[i]+" UI must consume "+button.name);
                }
                int commands=game.Feedback.CommandCount;yield return Click("Pause");
                Assert.IsTrue(game.Owner.Paused,scenes[i]);yield return Click("Resume");
                Assert.AreEqual(commands,game.Feedback.CommandCount,scenes[i]);
            }
        }
        [UnityTest] public IEnumerator ProgressUsesStableIdsAndNeverSkipsAnUnfinishedLevel()
        {
            var catalog=ui.Catalog;var save=new VenomCampaignSave();
            Assert.AreEqual(game.PlayableLevelCount,catalog.Levels.Length);Assert.AreEqual(1,catalog.NextIncomplete(save));
            save.Completed.Add(catalog.Levels[4].Id);Assert.AreEqual(1,catalog.NextIncomplete(save));
            save.Completed.Add(catalog.Levels[0].Id);Assert.AreEqual(2,catalog.NextIncomplete(save));
            save.Completed.Add("unknown.old.level");Assert.AreEqual(2,catalog.NextIncomplete(save));
            foreach(var level in catalog.Levels)if(!save.Completed.Contains(level.Id))save.Completed.Add(level.Id);
            Assert.AreEqual(0,catalog.NextIncomplete(save));Assert.IsTrue(catalog.AllComplete(save));
            string key="coghe.product-ui.test.migration";
            try
            {
                VenomCampaignSave.PersistenceEnabled=true;PlayerPrefs.SetString(key,JsonUtility.ToJson(save));
                var restored=VenomCampaignSave.Read(key);CollectionAssert.AreEqual(save.Completed,restored.Completed);
                Assert.AreEqual(0,catalog.NextIncomplete(restored));
            }
            finally{PlayerPrefs.DeleteKey(key);VenomCampaignSave.PersistenceEnabled=false;}
            yield return null;
        }
    }
}
