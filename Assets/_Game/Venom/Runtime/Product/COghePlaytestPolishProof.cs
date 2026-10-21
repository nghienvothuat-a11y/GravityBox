#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Opt-in native visual fixture, no player saves. Solves are covered separately by PlayMode tests.</summary>
    public sealed class COghePlaytestPolishProof : MonoBehaviour
    {
        private string output;
        private VenomCampaign game;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-coghe-playtest-review") < 0) return;
            VenomCampaignSave.PersistenceEnabled = false; COgheProductMode.OverrideForTests = true;
            COgheIntro.SeenOverrideForTests = true;
            var go = new GameObject("Playtest polish review"); DontDestroyOnLoad(go);
            go.AddComponent<COghePlaytestPolishProof>();
        }
        IEnumerator Load(int n)
        {
            yield return SceneManager.LoadSceneAsync($"COgheSpatial{n:00}");
            yield return null; yield return null;
            game = FindFirstObjectByType<VenomCampaign>();
            if (game.ProductUI.Page != COgheProductPage.Game) game.ProductUI.Play();
            game.ProductUI.OnGameplayInteraction();
            yield return new WaitForSeconds(.25f);
        }
        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs(); int flag = Array.IndexOf(args, "-coghe-playtest-review");
            output = Path.GetFullPath(flag + 1 < args.Length && !args[flag + 1].StartsWith("-")
                ? args[flag + 1] : "Artifacts/COghePlaytestPolish/Native");
            Directory.CreateDirectory(output);
            Application.runInBackground = true; Screen.SetResolution(720, 960, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(2);
            for (int ability = 0; ability < 3; ability++)
            {
                COgheShop.ResetForTests(new COgheShop.State { Migrated = true, InkGiftRevision = 1,
                    LastAbility = ability, Emotion = ability == 0 ? 1 : 0, Agility = ability == 1 ? 1 : 0,
                    Intelligence = ability == 2 ? 1 : 0 });
                yield return Load(1);
                if (ability == 1) game.Motion.Move(0, game.Root.TransformPoint(new Vector3(.30f, -.30f, .18f)));
                yield return new WaitForSeconds(2.8f);
                yield return Shot($"ability-{ability}");
                var fx = game.ProductUI.GetComponentInChildren<COgheAbilityFx>();
                Debug.Log($"POLISH FX {ability}: bursts={fx.Bursts} motes={fx.LiveCount}");
            }
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true, InkGiftRevision = 1 });
            yield return Load(2);
            for (int i = 0; i < 8; i++)
            {
                game.CameraRig.Orbit(90, 720);
                yield return new WaitForSeconds(.3f);
                yield return Shot($"level-02-angle-{i}");
            }
            yield return Load(1);
            game.ProductUI.ReplayIntro();
            var intro = FindFirstObjectByType<COgheIntro>();
            while (!intro.Started) yield return null;
            intro.Speed = 0;
            foreach (float time in new[] { 16.3f, 16.8f, 17.5f, 19.8f })
            {
                intro.Clock = time; yield return Shot("intro-" + time.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
            }
            intro.Skip(); yield return Load(1);
            game.Progress.HomeUnlocked = true; game.ProductUI.ShowMenu();
            game.ProductUI.OpenHome(); game.ProductUI.ToggleHomeZoom();
            yield return new WaitForSeconds(2);
            game.Personality.Force(COgheAct.Shape, COgheShape.Apple);
            yield return new WaitForSeconds(1.6f); yield return Shot("food-apple");
            var ui = game.ProductUI;
            for (int i = 0; i < 8; i++) game.Progress.Completed.Add(ui.Catalog.Levels[i].Id);
            ui.OpenStyleFromMenu(); yield return new WaitForSeconds(1); yield return Shot("mint-choose");
            ui.ChooseStyleItem("INK_MINT"); yield return Shot("mint-hold");
            var points = game.Matter.GetComponent<VenomSurface>().DrawnParticles;
            Vector3 centre = Vector3.zero; foreach (var p in points) centre += p; centre /= points.Length;
            if (!ui.BeginStyleHold(game.Owner.View.WorldToScreenPoint(centre), false))
            { Debug.LogError("Polish proof: cannot inject Mint"); Application.Quit(1); yield break; }
            yield return new WaitForSeconds(1);
            ui.EndStyleHold(); yield return new WaitForSeconds(1); yield return Shot("mint-done");
            bool guided = COgheShop.Current.InkGuideStep == 3 &&
                !ui.GetComponentsInChildren<Button>().Any(b => b.name == "Back");
            Debug.Log("POLISH REVIEW DONE: Mint injected, mandatory Done=" + guided);
            Application.Quit(guided ? 0 : 1);
        }
        IEnumerator Shot(string name)
        {
            ScreenCapture.CaptureScreenshot(Path.Combine(output, name + ".png"));
            yield return new WaitForEndOfFrame(); yield return null;
        }
    }
}
#endif
