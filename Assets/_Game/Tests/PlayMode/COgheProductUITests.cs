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
    public partial class COgheProductUITests
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
            COgheShop.ResetForTests(new COgheShop.State{Migrated=true});COgheAds.ResetForTests();COgheEntitlements.ResetForTests();COgheEconomy.ResetForTests();
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
        // Mrk: after the intro COghe stood mid-box (its menu spot), then jumped to the level start. The level is reset while
        // paused under the comic, so the reset pose must already be what is drawn, before physics steps again.
        [UnityTest] public IEnumerator FirstRunIntroDrawsCOgheAtTheLevelStart()
        {
            var menuSpot=Drawn();
            COgheIntro.Seen=false;ui.Play();var intro=Object.FindFirstObjectByType<COgheIntro>();
            while(!intro.Started)yield return null;
            for(int i=0;i<5;i++)yield return null;
            Assert.IsTrue(game.Owner.Paused,"The level waits under the comic");
            Assert.Greater(Vector3.Distance(menuSpot,game.Motion.Centre(0)),.03f,"Fixture: the menu spot is not the level start");
            foreach(var body in game.Matter.Bodies)
                Assert.Less(Vector3.Distance(body.transform.position,body.position),.002f,"Drawn where the level starts: "+body.name);
            var paused=Drawn();intro.Speed=20;intro.Skip();
            while(COgheIntro.Playing)yield return null;
            yield return null;yield return null;
            Assert.Less(Vector3.Distance(paused,Drawn()),.02f,"No jump when the level starts running");
        }
        private float TissueTop(){float top=0;foreach(var body in game.Matter.Bodies)top=Mathf.Max(top,game.Owner.View.WorldToViewportPoint(body.transform.position).y);return top;}
        private float SkinTop()
        {
            var skin=GameObject.Find("Continuous wet skin").GetComponent<MeshFilter>();float top=0;
            foreach(var v in skin.sharedMesh.vertices)top=Mathf.Max(top,game.Owner.View.WorldToViewportPoint(skin.transform.TransformPoint(v)).y);
            return top;
        }
        private Vector3 Drawn(){var c=Vector3.zero;foreach(var body in game.Matter.Bodies)c+=body.transform.position;return c/game.Matter.Bodies.Length;}
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
        [UnityTest] public IEnumerator GuidingThroughExitWaitsForNextLevelTap()
        {
            COgheAnalytics.Recent.Clear();
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
            Assert.AreEqual(1, COgheAnalytics.Recent.Count(e => e.StartsWith("level_complete ")));
            Assert.AreEqual(1, COgheAnalytics.Recent.Count(e => e.StartsWith("level_end ")));
            Assert.AreEqual(0, COgheAnalytics.Recent.Count(e => e.StartsWith("level_fail ")));
            Assert.IsTrue(game.Progress.Completed.Contains(game.Definition.Id));
            Assert.AreEqual(COgheEconomy.FirstWin,ui.VictoryDrops,"A first win pays Drops");Assert.AreEqual(COgheEconomy.FirstWin,COgheShop.Drops);
            // Mrk: the confetti pops just over COghe's head in the settled victory shot, then falls past it
            // (the tissue's top over the settled shot, not the skin: the random pose may raise tendrils above the head)
            while(game.Owner.Celebration.Elapsed<1.2f)yield return null;
            float head=0,skin=0,pop=VenomCelebration.SettledViewport(.07f,Screen.width,Screen.height,ui.VictoryStageScreenRect).y;
            while(game.Owner.Celebration.Elapsed<2f){head=Mathf.Max(head,TissueTop());skin=Mathf.Max(skin,SkinTop());yield return null;}
            Debug.Log($"Confetti origin {pop:F3}, tissue top {head:F3}, skin top {skin:F3} (viewport), pose {game.Owner.Celebration.Variant}");
            Assert.That(pop-head,Is.InRange(0f,.18f),$"Confetti origin {pop:F3} just over the tissue {head:F3}");
            yield return new WaitForSecondsRealtime(8);
            Assert.IsTrue(game.AutoAdvance,"Legacy scene flag must not bypass the Product victory button");
            Assert.AreEqual(1,game.Definition.Order);Assert.AreEqual(COgheProductPage.Victory,ui.Page);
            Assert.AreEqual(COgheProductPopup.None,ui.Popup);
            Assert.AreEqual(COgheEconomy.FirstWin,COgheShop.Drops,"Waiting must not pay the win twice");
            var next=ui.GetComponentsInChildren<Button>().Single(b=>b.name=="Next level");
            Assert.AreEqual("→  Level 2",next.GetComponent<Text>().text,"Keep the existing next-level line, not a new pill button");
            Assert.IsNull(next.GetComponent<Image>());
            Assert.GreaterOrEqual(((RectTransform)next.transform).rect.height,48);
            int commands=game.Feedback.CommandCount;
            yield return Click("Next level");
            if(game!=null)Assert.AreEqual(commands,game.Feedback.CommandCount,"Next Level must not issue a world command");
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
        // Level guides (Mrk): level 1 an arrow on the exit, level 3 "Hold to rotate" and the exit, level 4 the handle to pull.
        private IEnumerator Level(int n)
        {
            yield return SceneManager.LoadSceneAsync(ui.Catalog.Levels[0].SceneSequence[n-1]);yield return null;
            game=Object.FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            Assert.AreEqual(n,game.Definition.Order);Assert.AreEqual(COgheProductPage.Game,ui.Page);
        }
        private Vector2 OnSafe(Vector3 world)
        {
            var root=(RectTransform)ui.Guide.transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root,game.Owner.View.WorldToScreenPoint(world),null,out var local);
            return local-root.rect.min;
        }
        private void AssertPointsAt(Vector3 world,string what)
        {
            var guide=ui.Guide;Assert.IsTrue(guide.ArrowShown,what);Assert.IsTrue(guide.TargetOnScreen,what);
            Assert.Less(Vector2.Distance(guide.TargetPoint,OnSafe(world)),2f,"Aimed at "+what);
            Assert.AreEqual(guide.TargetPoint.x,guide.ArrowTip.x,.5f,what);
            Assert.That(guide.ArrowTip.y-guide.TargetPoint.y,Is.InRange(6f,30f),"Just above "+what+", pointing down");
        }
        [UnityTest] public IEnumerator Level1ArrowPointsAtTheExitUntilCOgheHeadsOut()
        {
            ui.Play();Assert.AreEqual(COgheProductPage.Game,ui.Page);
            var guide=ui.Guide;Assert.IsNotNull(guide,"Level 1 teaches the exit");
            yield return new WaitForSecondsRealtime(.4f);Assert.IsFalse(guide.ArrowShown,"It fades in after a moment");
            yield return new WaitForSecondsRealtime(1.4f);
            Assert.AreEqual(COgheGuide.Target.Exit,guide.Pointing);AssertPointsAt(game.Owner.Outlet.position,"the exit");
            float y=guide.ArrowTip.y;bool nudged=false;
            for(int f=0;f<40;f++){yield return null;nudged|=Mathf.Abs(guide.ArrowTip.y-y)>2;}
            Assert.IsTrue(nudged,"The arrow keeps nudging toward the hole");
            ui.ShowPopup(COgheProductPopup.Pause);yield return new WaitForSecondsRealtime(.6f);Assert.IsFalse(guide.ArrowShown,"Not over Pause");
            ui.Resume();yield return new WaitForSecondsRealtime(.6f);Assert.IsTrue(guide.ArrowShown);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));yield return new WaitForSecondsRealtime(.6f);
            Assert.AreEqual(COgheGuide.Target.None,guide.Pointing,"COghe heads out: the arrow steps aside");Assert.IsFalse(guide.ArrowShown);
        }
        [UnityTest] public IEnumerator Level3ShowsHoldToRotateUntilTheViewTurns()
        {
            yield return Level(3);
            var guide=ui.Guide;Assert.IsNotNull(guide);yield return new WaitForSecondsRealtime(1.8f);
            Assert.IsTrue(guide.RotateHintShown,"Hold to rotate");Assert.AreEqual(COgheGuide.Target.Exit,guide.Pointing,"and the way out");Assert.IsTrue(guide.ArrowShown);
            Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t=>t.text=="Hold to rotate"));
            // a real held drag across the box turns the view
            Vector2 a=new Vector2(Screen.width*.3f,Screen.height*.5f),b=new Vector2(Screen.width*.75f,Screen.height*.5f);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=a});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=a,buttons=1});yield return null;
            for(int i=1;i<=12;i++){InputSystem.QueueStateEvent(mouse,new MouseState{position=Vector2.Lerp(a,b,i/12f),buttons=1});yield return null;}
            InputSystem.QueueStateEvent(mouse,new MouseState{position=b});yield return null;
            Assert.Greater(Mathf.Abs(game.CameraRig.OrbitYaw),25f,"The drag turned the view");
            yield return new WaitForSecondsRealtime(.6f);
            Assert.IsTrue(guide.RotateLearned);Assert.IsFalse(guide.RotateHintShown,"Learned: the hint goes");
            Assert.IsTrue(guide.ArrowShown,"The exit arrow stays (at the screen edge if the exit turned away)");
        }
        [UnityTest] public IEnumerator Level4ArrowPointsAtTheHandleThenTheExit()
        {
            yield return Level(4);
            var guide=ui.Guide;Assert.IsNotNull(guide);yield return new WaitForSecondsRealtime(1.8f);
            var handle=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>().Single(r=>r.Label=="A");
            Assert.AreEqual(COgheGuide.Target.Handle,guide.Pointing,"The mechanism to pull first");AssertPointsAt(handle.HandPoint,"the handle");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(handle.HandPoint+Vector3.up*.004f));yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(COgheGuide.Target.None,guide.Pointing,"On its way to pull: no arrow");
            float end=Time.realtimeSinceStartup+40;
            while(!(handle.CompletedJourneys>0&&game.FinalExitAvailable)&&Time.realtimeSinceStartup<end)yield return null;
            Assert.IsTrue(game.FinalExitAvailable,"Pulled: the way is open");
            yield return new WaitForSecondsRealtime(.8f);
            Assert.AreEqual(COgheGuide.Target.Exit,guide.Pointing,"Then the exit");Assert.IsTrue(guide.ArrowShown);
        }
        [UnityTest] public IEnumerator OtherLevelsHaveNoGuide()
        {
            yield return Level(2);Assert.IsNull(ui.Guide);
            yield return Level(5);Assert.IsNull(ui.Guide);
        }
        [UnityTest] public IEnumerator IntroSkipSitsLowAndCentredForTheThumb()
        {
            COgheIntro.Seen=false;ui.Play();var intro=Object.FindFirstObjectByType<COgheIntro>();
            while(!intro.Started)yield return null;
            var safe=Screen.safeArea;float unit=Mathf.Min(safe.width/360f,safe.height/640f);var r=COgheIntro.SkipButton(unit,safe);
            Assert.AreEqual(safe.center.x,r.center.x,1f,"Centred");
            Assert.Greater(r.yMax,Screen.height-safe.yMin-40*unit,"In the lower thumb zone");Assert.LessOrEqual(r.yMax,Screen.height-safe.yMin,"Inside the safe area");
            Assert.GreaterOrEqual(r.height,48*unit-.01f,"Big enough to hit");
        }

        // Home view (Mrk): drag turns the room like a level; Zoom in follows COghe.
        [UnityTest] public IEnumerator HomeTurnsWithADragAndZoomFollowsCOghe()
        {
            game.Progress.HomeUnlocked=true;COgheHomeRoom.UnlockedLevelOverride=26;
            try
            {
                yield return Click("Home");Assert.AreEqual(COgheProductPage.Home,ui.Page);yield return new WaitForSecondsRealtime(.5f);
                var view=game.Owner.View;Assert.AreEqual(-6f,Mathf.DeltaAngle(0,view.transform.eulerAngles.y),.5f,"Starts from the front");
                Vector2 a=new Vector2(Screen.width*.4f,Screen.height*.45f),b=new Vector2(Screen.width*.6f,Screen.height*.45f);   // ~48°
                InputSystem.QueueStateEvent(mouse,new MouseState{position=a});yield return null;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=a,buttons=1});yield return null;
                for(int i=1;i<=12;i++){InputSystem.QueueStateEvent(mouse,new MouseState{position=Vector2.Lerp(a,b,i/12f),buttons=1});yield return null;}
                InputSystem.QueueStateEvent(mouse,new MouseState{position=b});yield return null;yield return null;
                Assert.Greater(Mathf.Abs(ui.HomeYaw),25f,"A drag turns the room");Assert.Less(Mathf.Abs(ui.HomeYaw),80f);
                Assert.AreEqual(-6f+ui.HomeYaw,Mathf.DeltaAngle(0,view.transform.eulerAngles.y),.5f);
                var room=game.HomeRoom;Assert.IsTrue(room.BackWallVisible);
                ui.OrbitHome(-(180-ui.HomeYaw)/240*Screen.width);yield return null;yield return null;
                Assert.AreEqual(180f,Mathf.Abs(ui.HomeYaw),2f);Assert.IsFalse(room.BackWallVisible,"From behind, the back wall steps out of the way");
                // taps still reach the floor through the invisible guards that keep the food balls in
                var floor=room.Root.TransformPoint(new Vector3(.05f,0,-.2f));game.Motion.StopAll();
                game.TouchPoint(view.WorldToScreenPoint(floor));Assert.IsNotNull(game.Motion.Get(0),"A floor tap moves COghe, the room turned round");
                // the whole room still fits: every floor corner on screen
                yield return new WaitForSecondsRealtime(1f);
                for(int i=0;i<4;i++)
                {
                    var corner=room.Root.TransformPoint(new Vector3(i%2==0?-COgheHomeRoom.HalfWidth:COgheHomeRoom.HalfWidth,0,i<2?-COgheHomeRoom.HalfDepth:COgheHomeRoom.HalfDepth));
                    var v=view.WorldToViewportPoint(corner);Assert.That(v.x,Is.InRange(0f,1f),"corner "+i);Assert.That(v.y,Is.InRange(0f,1f),"corner "+i);
                }
                float overview=view.orthographicSize;
                yield return Click("Zoom in");Assert.IsTrue(ui.HomeZoom);yield return new WaitForSecondsRealtime(2f);
                Assert.Less(view.orthographicSize,overview*.4f,"Close in");
                var at=view.WorldToViewportPoint(game.Personality.SkinCentre);Assert.That(at.x,Is.InRange(.3f,.7f),"COghe in the middle");Assert.That(at.y,Is.InRange(.25f,.75f));
                game.Motion.Move(0,room.Root.TransformPoint(new Vector3(.1f,COgheHomeRoom.BodyHeight,-.4f)));yield return new WaitForSecondsRealtime(4f);
                at=view.WorldToViewportPoint(game.Personality.SkinCentre);Assert.That(at.x,Is.InRange(.25f,.75f),"and follows it");Assert.That(at.y,Is.InRange(.2f,.8f));
                yield return Click("Zoom out");Assert.IsFalse(ui.HomeZoom);yield return new WaitForSecondsRealtime(2f);
                Assert.Greater(view.orthographicSize,overview*.8f,"Back to the whole room");
            }
            finally{COgheHomeRoom.UnlockedLevelOverride=null;}
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
