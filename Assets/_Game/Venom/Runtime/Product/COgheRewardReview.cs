#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    // Opt-in presentation fixture. Never unlocks items or advances saves in a normal player session.
    public sealed class COgheRewardReview : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-coghe-reward-review") < 0) return;
            VenomCampaignSave.PersistenceEnabled = false; COgheProductMode.OverrideForTests = true;
            var go = new GameObject("Reward review"); DontDestroyOnLoad(go); go.AddComponent<COgheRewardReview>();
        }
        IEnumerator Start()
        {
            string dir = Path.GetFullPath("Artifacts/RewardPolish/UI"); Directory.CreateDirectory(dir);
            Application.runInBackground = true; Screen.SetResolution(720,1280,FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(2);
            var game = FindFirstObjectByType<VenomCampaign>(); var ui = game.ProductUI;
            ui.LoadForTest(8);
            float deadline = Time.realtimeSinceStartup + 20;
            do
            {
                yield return null;
                game = FindFirstObjectByType<VenomCampaign>();
            } while ((game == null || game.Definition.Order != 8 || game.ProductUI == null) && Time.realtimeSinceStartup < deadline);
            if (game == null || game.Definition.Order != 8) { Application.Quit(1); yield break; }
            ui = game.ProductUI; yield return null; ui.ShowMenu();
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true, PacingRevision = COgheProgressionMilestones.LatestRevision }); COgheStyle.ResetForTests(new COgheStyle());
            COgheShop.TestsOwnUnlocked = false;
            game.Progress.Completed.Clear(); game.Progress.HomeUnlocked = false;
            for (int i=0; i<8; i++) game.Progress.Completed.Add(ui.Catalog.Levels[i].Id);
            ui.ShowVictoryForTests(ui.Catalog.Levels[7].Id,8);
            yield return Shot(dir,"01-drop-reward",.55f);
            ui.NextLevel(); yield return Shot(dir,"02-unlock-fireworks",.75f);
            Click(ui,"Continue"); yield return Shot(dir,"03-choose-ocean",2);
            ui.ChooseStyleItem("INK_OCEAN"); yield return Shot(dir,"04-hold-on-coghe",.6f);
            var points = game.Matter.GetComponent<VenomSurface>().DrawnParticles;
            var centre = Vector3.zero; foreach(var p in points) centre += p;
            if (!ui.BeginStyleHold(game.Owner.View.WorldToScreenPoint(centre/points.Length),false))
            { Debug.LogError("Reward review: hold missed body"); Application.Quit(1); yield break; }
            yield return Shot(dir,"05-release-finger",.85f);
            ui.EndStyleHold(); yield return Shot(dir,"06-done-arrow",1);
            Click(ui,"Done"); yield return Shot(dir,"07-continue-level-nine",3);
            var next = FindFirstObjectByType<VenomCampaign>();
            bool ok = COgheShop.Current.InkGuideStep == 4 && next.Definition.Order == 9;
            File.WriteAllText(Path.Combine(dir,"result.json"),"{\"presentationFixture\":true,\"continuedTo9\":"+ok.ToString().ToLowerInvariant()+",\"writesPlayerSave\":false}");
            Application.Quit(ok?0:1);
        }
        static void Click(COgheProductUI ui,string name) => ui.GetComponentsInChildren<Button>().Single(b=>b.name==name).onClick.Invoke();
        static IEnumerator Shot(string dir,string name,float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png")); yield return null;
        }
    }
}
#endif
