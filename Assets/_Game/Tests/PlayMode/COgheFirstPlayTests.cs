using System.Collections;
using System.Linq;
using System.Reflection;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GravityBox.Tests
{
    public partial class COgheProductUITests
    {
        private IEnumerator ColdStart(bool introSeen, bool existingProgress = false)
        {
            COgheIntro.Seen = introSeen;
            typeof(COgheProductMode).GetField("SessionStarted", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, false);
            void Loaded(Scene scene, LoadSceneMode mode)
            {
                game = Object.FindFirstObjectByType<VenomCampaign>();
                if(existingProgress)game.Progress.Completed.Add(game.Definition.Id); // legacy-save fixture
            }
            SceneManager.sceneLoaded += Loaded;
            try { yield return SceneManager.LoadSceneAsync("COgheSpatial01"); }
            finally { SceneManager.sceneLoaded -= Loaded; }
            yield return null; game = Object.FindFirstObjectByType<VenomCampaign>(); ui = game.ProductUI;
        }

        [UnityTest] public IEnumerator FreshLaunchStartsIntroWithoutPlayAndSkipEntersLevelOneImmediately()
        {
            yield return ColdStart(false);
            Assert.AreEqual(COgheProductPage.Intro, ui.Page);
            Assert.IsFalse(game.Home); Assert.AreEqual(0, game.Progress.Completed.Count);
            var intro = Object.FindFirstObjectByType<COgheIntro>();
            while(!intro.Started)yield return null;
            var p=(Vector2)game.Owner.View.WorldToScreenPoint(game.Root.TransformPoint(new Vector3(0,-.3f,-.15f)));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=1});yield return null;
            int commands=game.Feedback.CommandCount;
            intro.Skip();
            Assert.IsFalse(COgheIntro.Playing);Assert.AreEqual(COgheProductPage.Game,ui.Page);
            Assert.AreEqual(1,game.Definition.Order);Assert.IsFalse(game.Owner.Paused);Assert.IsFalse(game.Home);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;yield return null;
            Assert.AreEqual(commands,game.Feedback.CommandCount,"The skip press/release must not command the level");
        }

        [UnityTest] public IEnumerator FreshLaunchFinishesIntroIntoLevelOneWithoutAnotherButton()
        {
            yield return ColdStart(false);
            Assert.AreEqual(COgheProductPage.Intro,ui.Page);
            var intro=Object.FindFirstObjectByType<COgheIntro>();while(!intro.Started)yield return null;
            intro.Speed=30;float end=Time.realtimeSinceStartup+10;
            while(COgheIntro.Playing&&Time.realtimeSinceStartup<end)yield return null;
            Assert.IsFalse(COgheIntro.Playing);Assert.AreEqual(COgheProductPage.Game,ui.Page);
            Assert.IsFalse(game.Owner.Paused);Assert.AreEqual(1,game.Definition.Order);
        }

        [UnityTest] public IEnumerator ReturningLaunchKeepsMenuAndLegacyProgressIsNotRestarted()
        {
            yield return ColdStart(true);
            Assert.AreEqual(COgheProductPage.MainMenu,ui.Page);Assert.IsFalse(COgheIntro.Playing);
            yield return ColdStart(false,true);
            Assert.AreEqual(COgheProductPage.MainMenu,ui.Page);Assert.IsFalse(COgheIntro.Playing);
            Assert.IsTrue(game.Progress.Completed.Contains(game.Definition.Id));Assert.AreEqual(2,ui.ResumeLevel);
        }

        [UnityTest] public IEnumerator OpeningHintFadesAndFirstTouchStillIssuesItsCommand()
        {
            ui.Play();ui.RequestHint();Assert.AreEqual(0,ui.HintAlpha);
            yield return new WaitForSecondsRealtime(.16f);
            Assert.That(ui.HintAlpha,Is.InRange(.05f,.9f),"Visible intermediate fade-in");
            yield return new WaitForSecondsRealtime(.4f);Assert.AreEqual(1,ui.HintAlpha,.001f);
            var p=(Vector2)game.Owner.View.WorldToScreenPoint(game.Root.TransformPoint(new Vector3(0,-.3f,-.15f)));
            Assert.IsTrue(ui.AllowsWorldPointer(p));int before=game.Feedback.CommandCount;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=1});yield return null;
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(ui.HintAlpha,Is.InRange(.05f,.95f),"The first press fades, without waiting for a release");
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;yield return null;
            Assert.Greater(game.Feedback.CommandCount,before,"No second tap needed after dismissing the hint");
            yield return new WaitForSecondsRealtime(.35f);Assert.IsFalse(ui.HintShown);
        }

        [UnityTest] public IEnumerator HintCloseAndPauseFadeOnUiTimeAndRelayoutDoesNotRestartIt()
        {
            ui.Play();ui.RequestHint();yield return new WaitForSecondsRealtime(.5f);
            yield return Click("Close hint");
            Assert.IsTrue(ui.HintShown,"Close begins a fade rather than destroying the panel");
            yield return new WaitForSecondsRealtime(.1f);float alpha=ui.HintAlpha;
            ui.RelayoutForTests();Assert.AreEqual(alpha,ui.HintAlpha,.001f);
            yield return new WaitForSecondsRealtime(.35f);Assert.IsFalse(ui.HintShown);
            ui.RequestHint();yield return new WaitForSecondsRealtime(.5f);
            ui.ShowPopup(COgheProductPopup.Pause);Assert.IsTrue(game.Owner.Paused);
            yield return new WaitForSecondsRealtime(.4f);Assert.IsFalse(ui.HintShown,"Fade finishes while game time is paused");
        }
    }

    public sealed partial class COgheSpatialCampaignTests
    {
        private Vector3 CasingTop(COgheTapRail task) => task.Rail.Body.position+game.Root.TransformDirection(new Vector3(.023f,.013f,.020f));

        [UnityTest] public IEnumerator WholeCasingOpensLevelFiveAndStillRespectsTheLock()
        {
            yield return Load(5);
            var tasks=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>();
            var a=tasks.Single(t=>t.Label=="A");var c=tasks.Single(t=>t.Label=="C");
            Assert.IsTrue(a.TouchControlBody&&c.TouchControlBody);
            yield return Tap(CasingTop(a));
            Assert.IsFalse(a.Busy);Assert.AreEqual(COgheMechanism.Refusal.Locked,a.LastRefusal);
            yield return Tap(CasingTop(c));Assert.IsTrue(c.Busy,"Ivory casing, away from the coloured handle");
            yield return Until(25,()=>c.CompletedJourneys==1,"COghe operates the floor control from a casing tap");
            yield return Until(8,()=>a.InterlockOpen,"Cover opens physically");
            yield return Tap(CasingTop(a));Assert.IsTrue(a.Busy,"Wall casing also works");
            yield return Until(25,()=>a.CompletedJourneys==1,"Wall handle is pulled");
            yield return Until(8,()=>game.FinalExitAvailable,"Exit opened by actual controls");
            yield return Tap(game.Owner.Outlet.position);
            yield return Until(35,()=>game.Owner.Completed,"Whole-casing route solves level 5");
            Assert.AreEqual(32,game.Matter.EscapedCount);Assert.AreEqual(1,game.Matter.TotalFragmentCount);
        }

        [UnityTest] public IEnumerator CasingCannotBeSelectedThroughAnOpaqueObjectOrOutsideItsBody()
        {
            yield return Load(5);
            var c=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>().Single(t=>t.Label=="C");
            Vector3 target=CasingTop(c);game.CameraRig.Frame(720,1280,0,true);
            Vector2 screen=game.Owner.View.WorldToScreenPoint(target);var ray=game.Owner.View.ScreenPointToRay(screen);
            var block=GameObject.CreatePrimitive(PrimitiveType.Cube);block.name="Occlusion test fixture";
            block.transform.position=target-ray.direction*.075f;block.transform.localScale=Vector3.one*.045f;
            Physics.SyncTransforms();
            game.TouchPoint(screen);Assert.IsFalse(c.Busy,"An opaque object owns the foreground hit");
            Object.DestroyImmediate(block);Physics.SyncTransforms();
            yield return Tap(c.Rail.Body.position+game.Root.TransformDirection(new Vector3(.065f,0,.035f)));
            Assert.IsFalse(c.Busy,"Adjacent glass stays a walk target");
            yield return Tap(target);Assert.IsTrue(c.Busy,"Removing the occluder exposes the same casing");
        }
    }
}
