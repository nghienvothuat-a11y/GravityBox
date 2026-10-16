#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace GravityBox.Venom
{
    /// <summary>Opt-in native input/visual check. In-memory progress and intro state, no player save writes.</summary>
    public sealed class COgheFirstPlayReview : MonoBehaviour
    {
        private string output;
        private Mouse mouse;
        private VenomCampaign game;
        private COgheProductUI ui;
        private int commandBefore;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-firstplay-review")<0)return;
            VenomCampaignSave.PersistenceEnabled=false;COgheProductMode.OverrideForTests=true;
            COgheProductMode.SessionStarted=false;COgheIntro.SeenOverrideForTests=false;
            COgheShop.ResetForTests(new COgheShop.State{Migrated=true,PacingRevision=1});
            var root=new GameObject("First play review");DontDestroyOnLoad(root);root.AddComponent<COgheFirstPlayReview>();
        }
        IEnumerator Start()
        {
            output=Path.GetFullPath("Artifacts/FirstPlayControls/Native");Directory.CreateDirectory(output);
            Application.runInBackground=true;Screen.SetResolution(720,1280,FullScreenMode.Windowed);
            mouse=InputSystem.AddDevice<Mouse>();
            yield return new WaitForSecondsRealtime(2);
            game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            if(ui.Page!=COgheProductPage.Intro){Fail("First launch did not start intro");yield break;}
            yield return Shot("01-first-launch-intro");
            var intro=FindFirstObjectByType<COgheIntro>();intro.Skip();
            if(ui.Page!=COgheProductPage.Game||game.Definition.Order!=1||game.Owner.Paused){Fail("Skip did not hand over");yield break;}
            yield return new WaitForSecondsRealtime(.17f);yield return Shot("02-hint-fading-in");
            yield return new WaitForSecondsRealtime(.35f);yield return Shot("03-level-one-guidance");
            var p=(Vector2)game.Owner.View.WorldToScreenPoint(game.Root.TransformPoint(new Vector3(0,-.3f,-.15f)));
            commandBefore=game.Feedback.CommandCount;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=1});yield return null;
            yield return new WaitForSecondsRealtime(.12f);yield return Shot("04-first-touch-fade-out");
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;
            yield return new WaitForSecondsRealtime(.4f);
            if(ui.HintShown||game.Feedback.CommandCount<=commandBefore){Fail("Hint swallowed the first command");yield break;}
            yield return Shot("05-guidance-dismissed");
            COgheIntro.SeenOverrideForTests=true;ui.LoadForTest(5);
            yield return new WaitForSecondsRealtime(2);game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            var c=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>().Single(t=>t.Label=="C");
            p=game.Owner.View.WorldToScreenPoint(c.Rail.Body.position+game.Root.TransformDirection(new Vector3(.023f,.013f,.020f)));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=1});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;yield return null;
            if(!c.Busy){Fail("Native casing tap missed");yield break;}
            yield return Shot("06-tap-ivory-casing");
            float deadline=Time.realtimeSinceStartup+30;
            while(c.CompletedJourneys==0&&Time.realtimeSinceStartup<deadline)yield return null;
            if(c.CompletedJourneys==0){Fail("Casing command did not finish");yield break;}
            yield return new WaitForSecondsRealtime(.5f);yield return Shot("07-control-operated");
            File.WriteAllText(Path.Combine(output,"result.json"),"{\"passed\":true,\"firstLaunchIntro\":true,\"skipToLevel1\":true,\"firstTouchCommand\":true,\"casingCommandCompleted\":true,\"writesPlayerSave\":false}");
            Application.Quit();
        }
        IEnumerator Shot(string name)
        {yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));yield return null;}
        void Fail(string why){File.WriteAllText(Path.Combine(output,"failure.txt"),why);Debug.LogError(why);Application.Quit(1);}
        void OnDestroy(){if(mouse!=null)InputSystem.RemoveDevice(mouse);COgheIntro.SeenOverrideForTests=null;}
    }
}
#endif
