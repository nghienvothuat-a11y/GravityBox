#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Opt-in native UI review fixture, never a claim of solving eight puzzles. Writes no player saves.</summary>
    public sealed class COgheChapterProgressionProof : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-chapter-review")<0)return;
            VenomCampaignSave.PersistenceEnabled=false;COgheProductMode.OverrideForTests=true;
            var go=new GameObject("Chapter progression review");DontDestroyOnLoad(go);go.AddComponent<COgheChapterProgressionProof>();
        }
        IEnumerator Start()
        {
            Application.runInBackground=true;Screen.SetResolution(720,1280,FullScreenMode.Windowed);
            string output=Path.GetFullPath("Artifacts/ChapterProgression20261014/UI");Directory.CreateDirectory(output);
            yield return new WaitForSecondsRealtime(2);
            var game=FindFirstObjectByType<VenomCampaign>();var ui=game.ProductUI;
            COgheShop.ResetForTests(new COgheShop.State{Migrated=true});COgheStyle.ResetForTests(new COgheStyle());
            for(int i=0;i<8;i++)game.Progress.Completed.Add(ui.Catalog.Levels[i].Id);
            ui.ShowMenu();yield return Shot(output,"menu-style-unlocked");
            ui.Play();yield return new WaitForSecondsRealtime(2);yield return Shot(output,"ink-1-choose");
            ui.ChooseStyleItem("INK_MINT");yield return Shot(output,"ink-2-hold");
            var points=game.Matter.GetComponent<VenomSurface>().DrawnParticles;
            Vector3 centre=Vector3.zero;foreach(var p in points)centre+=p;centre/=points.Length;
            bool held=ui.BeginStyleHold(game.Owner.View.WorldToScreenPoint(centre),false);
            if(!held){Debug.LogError("Ink review could not touch the creature");Application.Quit(1);yield break;}
            yield return new WaitForSecondsRealtime(.8f);yield return Shot(output,"ink-injecting");
            ui.EndStyleHold();yield return new WaitForSecondsRealtime(1);yield return Shot(output,"ink-3-saved");
            bool colored=COgheStyle.Current.Amount.Any(a=>a.x>.01f);
            COgheShop.SetInkGuideStep(4);ui.ShowMenu();game.Progress.HomeUnlocked=true;
            game.Progress.Completed.Add(ui.Catalog.Levels[8].Id);game.Progress.Completed.Add(ui.Catalog.Levels[9].Id);
            ui.Play();yield return new WaitForSecondsRealtime(4);yield return Shot(output,"required-understand-coghe");
            File.WriteAllText(Path.Combine(output,"result.json"),"{\"uiFixture\":true,\"inkInjected\":"+colored.ToString().ToLowerInvariant()+",\"bonusChapter\":"+ui.BonusChapter+",\"writesPlayerSave\":false}");
            Application.Quit(colored&&ui.BonusChapter==1?0:1);
        }
        IEnumerator Shot(string path,string name)
        {yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(path,name+".png"));yield return null;}
    }
}
#endif
