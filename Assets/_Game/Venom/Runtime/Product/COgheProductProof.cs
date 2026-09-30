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
            deadline=Time.realtimeSinceStartup+180;Application.runInBackground=true;
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
            game.AutoAdvance=false;yield return Capture("07-victory");
            game.AutoAdvance=true;yield return new WaitForSecondsRealtime(5);
            game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            if(game.Definition.Order!=2){Fail("Auto advance did not load level 2");yield break;}
            yield return Capture("08-next-puzzle");
            ui.ShowMenu();game.Progress.HomeUnlocked=true;ui.OpenHome();yield return Capture("09-home");
            ui.ShowPopup(COgheProductPopup.Collection);yield return Capture("10-collection");ui.Resume();ui.ShowMenu();
            ui.ReplayIntro();yield return new WaitForSecondsRealtime(4);
            game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            yield return Capture("11-intro-replay");var replay=FindFirstObjectByType<COgheIntro>();replay.Speed=30;replay.Skip();while(COgheIntro.Playing)yield return null;
            ui.Play();if(COgheIntro.Playing){replay=FindFirstObjectByType<COgheIntro>();while(!replay.Started)yield return null;replay.Speed=30;replay.Skip();while(COgheIntro.Playing)yield return null;}
            game.Owner.enabled=false;var half=new bool[32];for(int i=0;i<16;i++)half[i]=true;game.Matter.Partition(0,half);
            yield return Capture("12-fragments-ui-fixture");game.Owner.enabled=true;ui.ShowMenu();
            Screen.SetResolution(720,1612,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(1);
            yield return Capture("13-tall-menu");ui.Play();if(COgheIntro.Playing){replay=FindFirstObjectByType<COgheIntro>();while(!replay.Started)yield return null;replay.Speed=30;replay.Skip();while(COgheIntro.Playing)yield return null;}
            yield return Capture("14-tall-game");ui.ShowPopup(COgheProductPopup.Pause);yield return Capture("15-tall-pause");
            File.WriteAllText(Path.Combine(output,"native-result.json"),"{\"result\":\"passed\",\"nativeFirstLevelSolved\":true,\"autoAdvancedToLevel\":2,\"savedProgressWritten\":false,\"captures\":15}");
            Debug.Log("COGHE PRODUCT NATIVE PROOF PASSED");Application.Quit(0);
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
