#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Opt-in native-player visual verification; never runs for a normal player or writes campaign saves.</summary>
    public sealed class COgheProductProof : MonoBehaviour
    {
        private VenomCampaign game;
        private COgheProductUI ui;
        private string output;
        private float deadline;
        private bool failed;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-product-proof")<0)return;
            VenomCampaignSave.PersistenceEnabled=false;COgheProductMode.OverrideForTests=true;
            var go=new GameObject("Product UI native proof");DontDestroyOnLoad(go);go.AddComponent<COgheProductProof>();
        }
        private IEnumerator Start()
        {
            deadline=Time.realtimeSinceStartup+320;Application.runInBackground=true;
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-coghe-product-proof-output");
            output=at>=0&&at+1<args.Length?Path.GetFullPath(args[at+1]):Path.Combine(Application.persistentDataPath,"ProductUIProof");Directory.CreateDirectory(output);
            Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(1);
            game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            if(ui==null){Fail("Product UI missing");yield break;}
            yield return new WaitForSecondsRealtime(3);yield return Capture("01-main-menu");
            ui.OpenHome();yield return Capture("02-home-locked");ui.Resume();
            ui.Play();if(COgheIntro.Playing){var intro=FindFirstObjectByType<COgheIntro>();while(!intro.Started)yield return null;intro.Speed=30;intro.Skip();while(COgheIntro.Playing)yield return null;}
            yield return Capture("03-game");ui.ShowPopup(COgheProductPopup.Pause);yield return Capture("04-pause");
            ui.ShowPopup(COgheProductPopup.Help);yield return Capture("05-help");
            ui.ShowPopup(COgheProductPopup.Restart);yield return Capture("06-restart");ui.Resume();
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            float end=Time.realtimeSinceStartup+45;
            while(!game.Owner.Completed&&Time.realtimeSinceStartup<end)yield return null;
            if(!game.Owner.Completed){Fail("Native first puzzle could not be solved");yield break;}
            game.AutoAdvance=false;yield return new WaitForSecondsRealtime(1f);yield return Capture("07-victory");   // the confetti pops as the shot settles
            game.AutoAdvance=true;yield return new WaitForSecondsRealtime(5);
            game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            if(game.Definition.Order!=2){Fail("Auto advance did not load level 2");yield break;}
            yield return Capture("08-next-puzzle");
            // Home with the furniture earned up to level 26 (a fixture: no progress is written)
            COgheHomeRoom.UnlockedLevelOverride=26;
            ui.ShowMenu();game.Progress.HomeUnlocked=true;ui.OpenHome();yield return new WaitForSecondsRealtime(5);yield return Capture("09-home");
            ui.ShowPopup(COgheProductPopup.Collection);yield return Capture("10-collection");ui.Resume();
            var room=game.HomeRoom;
            if(room!=null)
            {
                room.ShowGhost(room.Find("TRAMPOLINE"),8);ui.Notify("Trampoline unlocks at level 30");yield return new WaitForSecondsRealtime(1.5f);yield return Capture("16-home-ghost");room.HideGhost();
                game.Personality.PlayNow(room.Find("SWING"));yield return new WaitForSecondsRealtime(2.5f);yield return Capture("17-home-swing");
                game.Personality.PlayNow(room.Find("DUMBBELL"));yield return new WaitForSecondsRealtime(2.2f);yield return Capture("18-home-dumbbell");
            }
            COgheHomeRoom.UnlockedLevelOverride=null;ui.ShowMenu();
            ui.ReplayIntro();yield return new WaitForSecondsRealtime(4);
            game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            yield return Capture("11-intro-replay");var replay=FindFirstObjectByType<COgheIntro>();replay.Speed=30;replay.Skip();while(COgheIntro.Playing)yield return null;
            ui.Play();if(COgheIntro.Playing){replay=FindFirstObjectByType<COgheIntro>();while(!replay.Started)yield return null;replay.Speed=30;replay.Skip();while(COgheIntro.Playing)yield return null;}
            game.Owner.enabled=false;var half=new bool[32];for(int i=0;i<16;i++)half[i]=true;game.Matter.Partition(0,half);
            yield return Capture("12-fragments-ui-fixture");game.Owner.enabled=true;ui.ShowMenu();
            Screen.SetResolution(720,1612,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(1);
            yield return Capture("13-tall-menu");ui.Play();if(COgheIntro.Playing){replay=FindFirstObjectByType<COgheIntro>();while(!replay.Started)yield return null;replay.Speed=30;replay.Skip();while(COgheIntro.Playing)yield return null;}
            yield return Capture("14-tall-game");ui.ShowPopup(COgheProductPopup.Pause);yield return Capture("15-tall-pause");
            // test-build level picker, then a Boss level's opening tour and warning
            ui.ShowPopup(COgheProductPopup.Levels);yield return Capture("19-test-levels");
            ui.LoadForTest(10);yield return new WaitForSecondsRealtime(3f);
            game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            if(COgheBossIntro.Current==null){Fail("Boss level opened without its tour");yield break;}
            yield return Capture("20-boss-tour");
            var boss=COgheBossIntro.Current;boss.Seek(boss.TourLength+COgheBossIntro.PullBack+1.1f);yield return Capture("21-boss-warning");
            float bossEnd=Time.realtimeSinceStartup+10;while(COgheBossIntro.Current!=null&&Time.realtimeSinceStartup<bossEnd)yield return null;
            if(COgheBossIntro.Current!=null||game.Owner.Paused){Fail("Boss tour did not hand over");yield break;}
            yield return Capture("22-boss-level");
            // first-level guides: the exit arrow (1), "Hold to rotate" (3), the handle to pull (4)
            int shot=23;
            foreach(int n in new[]{1,3,4})
            {
                ui.LoadForTest(n);yield return new WaitForSecondsRealtime(2.5f);
                game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
                if(COgheIntro.Playing){var intro=FindFirstObjectByType<COgheIntro>();while(!intro.Started)yield return null;intro.Speed=30;intro.Skip();while(COgheIntro.Playing)yield return null;}
                yield return new WaitForSecondsRealtime(1.6f);
                if(ui.Guide==null||!ui.Guide.ArrowShown||(n==3&&!ui.Guide.RotateHintShown)){Fail("Level "+n+" guide missing");yield break;}
                yield return Capture((shot++)+"-guide-level"+n);
            }
            // Home: turned round, zoomed in on COghe, feeding (steel balls), the ball game
            COgheHomeRoom.UnlockedLevelOverride=26;
            ui.ShowMenu();game.Progress.HomeUnlocked=true;ui.OpenHome();yield return new WaitForSecondsRealtime(5);
            if(game.HomeRoom==null){Fail("Furnished Home missing");yield break;}
            ui.OrbitHome(-Screen.width*.5f);yield return new WaitForSecondsRealtime(1.5f);yield return Capture("26-home-turned");
            ui.OrbitHome(Screen.width*.5f);ui.ToggleHomeZoom();yield return new WaitForSecondsRealtime(2.5f);yield return Capture("27-home-zoom");ui.ToggleHomeZoom();
            yield return new WaitForSecondsRealtime(1.5f);game.FeedHome();yield return new WaitForSecondsRealtime(2.2f);yield return Capture("28-home-feed");
            if(game.HomeFeedBalls==null||game.HomeFeedBalls.Thrown<3){Fail("Feed threw no balls");yield break;}
            float fed=Time.realtimeSinceStartup+30;while(game.HomeFeeding&&Time.realtimeSinceStartup<fed)yield return null;
            if(game.HomeFeeding){Fail("COghe did not eat the balls");yield break;}
            game.Personality.PlayNow(game.HomeRoom.Find("BALL"));yield return new WaitForSecondsRealtime(3.45f);yield return Capture("29-home-ball-toss");
            // Style: inks held into COghe, the mix, a hat, things inside, a locked item, a fifth color's question, back home
            COgheHomeRoom.UnlockedLevelOverride=30;COgheStyle.ResetForTests(new COgheStyle{Seed=4});COgheStyle.Current.ApplyTo(game);
            ui.OpenStyle();yield return new WaitForSecondsRealtime(1.5f);
            if(ui.Page!=COgheProductPage.Style){Fail("Style did not open");yield break;}
            yield return Capture("30-style");
            yield return StyleHold("INK_OCEAN",0,.4f,2.6f,"31-style-hold");if(failed)yield break;
            yield return StyleHold("INK_CORAL",.8f,-.2f,1.2f,null);if(failed)yield break;
            yield return new WaitForSecondsRealtime(1.8f);yield return Capture("32-style-mix");
            ui.SetStyleTab(1,0);ui.ChooseStyleItem("HAT_STRAW");yield return new WaitForSecondsRealtime(1.2f);yield return Capture("33-style-hat");
            ui.SetStyleTab(1,1);ui.ChooseStyleItem("FLOAT_FISH");ui.Resume();ui.ChooseStyleItem("FLOAT_STARS");ui.Resume();
            yield return new WaitForSecondsRealtime(1.6f);yield return Capture("34-style-inside");
            ui.SetStyleTab(1,0);ui.ChooseStyleItem("HAT_ASTRO");yield return Capture("35-style-locked");ui.Resume();
            ui.SetStyleTab(0,0);yield return StyleHold("INK_GOLD",-.8f,-.3f,.8f,null);yield return StyleHold("INK_FIREFLY",.3f,-.6f,.8f,null);if(failed)yield break;
            ui.ChooseStyleItem("INK_SAKURA");if(ui.Popup!=COgheProductPopup.StyleReplaceInk){Fail("A fifth color did not ask");yield break;}
            yield return Capture("36-style-replace");ui.Resume();
            ui.LeaveStyle();yield return new WaitForSecondsRealtime(2.5f);yield return Capture("37-home-styled");
            COgheStyle.ResetForTests();
            COgheHomeRoom.UnlockedLevelOverride=null;
            File.WriteAllText(Path.Combine(output,"native-result.json"),"{\"result\":\"passed\",\"nativeFirstLevelSolved\":true,\"autoAdvancedToLevel\":2,\"savedProgressWritten\":false,\"captures\":37}");
            Debug.Log("COGHE PRODUCT NATIVE PROOF PASSED");Application.Quit(0);
        }
        /// <summary>Hold a syringe on COghe (x, y: across the body from its middle, in units of 2.5 cm) like a finger would.</summary>
        private IEnumerator StyleHold(string ink,float x,float y,float seconds,string capture)
        {
            ui.ChooseStyleItem(ink);
            var drawn=game.Matter.GetComponent<VenomSurface>().DrawnParticles;var c=Vector3.zero;foreach(var d in drawn)c+=d;c/=drawn.Length;
            var view=game.Owner.View.transform;
            if(!ui.BeginStyleHold(game.Owner.View.WorldToScreenPoint(c+view.right*x*.025f+view.up*y*.025f),false)){Fail("A hold on COghe did not start ("+ink+")");yield break;}
            if(capture!=null){yield return new WaitForSecondsRealtime(seconds*.6f);yield return Capture(capture);yield return new WaitForSecondsRealtime(seconds*.4f);}
            else yield return new WaitForSecondsRealtime(seconds);
            ui.EndStyleHold();
        }
        private IEnumerator Capture(string name)
        {
            yield return new WaitForSecondsRealtime(.35f);yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));
            Debug.Log("PRODUCT CAPTURE "+name);yield return null;
        }
        private void Update(){if(deadline>0&&Time.realtimeSinceStartup>deadline)Fail("Native proof timed out");}
        private void Fail(string reason){if(failed)return;failed=true;Debug.LogError(reason);File.WriteAllText(Path.Combine(output,"native-result.json"),"{\"result\":\"failed\",\"reason\":\""+reason+"\"}");Application.Quit(1);}
    }
}
#endif
