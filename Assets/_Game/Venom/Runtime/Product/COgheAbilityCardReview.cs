#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Opt-in native visual fixture; isolated in-memory progress, never writes the player's save.</summary>
    public sealed class COgheAbilityCardReview : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-coghe-card-review") < 0) return;
            VenomCampaignSave.PersistenceEnabled = false;
            COgheProductMode.OverrideForTests = true;
            COgheIntro.SeenOverrideForTests = true;
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true, PacingRevision = 1 });
            var go = new GameObject("Ability card review"); DontDestroyOnLoad(go); go.AddComponent<COgheAbilityCardReview>();
        }
        IEnumerator Start()
        {
            Application.runInBackground = true;
            var args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-coghe-card-review");
            string output = at + 1 < args.Length ? Path.GetFullPath(args[at + 1]) : Path.GetFullPath("Artifacts/COgheAbilityCards/Native");
            Directory.CreateDirectory(output);
            Screen.SetResolution(720, 1280, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(2);
            var game = FindFirstObjectByType<VenomCampaign>(); var ui = game.ProductUI;
            game.Progress.HomeUnlocked = true; COgheHomeRoom.UnlockedLevelOverride = 10;
            ui.ShowMenu(); ui.OpenHome(); yield return new WaitForSecondsRealtime(1);
            foreach (var size in new[] { new Vector2Int(720, 1280), new Vector2Int(720, 1560), new Vector2Int(960, 1280) })
            {
                Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.6f);
                COgheShop.ResetForTests(new COgheShop.State { Migrated = true, Emotion = 3, Agility = 2 });
                COgheShop.Understand(COgheBonus.Key(1));
                ui.ShowPopup(COgheProductPopup.AbilityStats); yield return Shot("stats");
                Press("Choose card"); yield return Shot("pick");
                foreach (string a in new[] { "Emotion", "Agility", "Intelligence" })
                { Press(a + " card"); yield return Shot("selected-" + a); }
                Press("Choose ability"); yield return Shot("learned-Clever");
                if (COgheAbilities.Level(COgheAbility.Intelligence) != 1) throw new Exception("Card was not awarded");
                Press("Ability continue");
                COgheShop.Current.Emotion = 10; COgheShop.Understand(COgheBonus.Key(2));
                ui.ShowPopup(COgheProductPopup.AbilityStats); Press("Choose card"); yield return Shot("max-Playful");
                ui.Resume();
                IEnumerator Shot(string state)
                {
                    yield return new WaitForSecondsRealtime(.2f); yield return new WaitForEndOfFrame();
                    ScreenCapture.CaptureScreenshot(Path.Combine(output, size.x + "x" + size.y + "-" + state + ".png"));
                    yield return null;
                }
                void Press(string name)
                {
                    var b = ui.GetComponentsInChildren<Button>().Single(x => x.name == name);
                    if (!b.interactable) throw new Exception("Disabled " + name);
                    b.onClick.Invoke();
                }
            }
            File.WriteAllText(Path.Combine(output, "result.json"), "{\"passed\":true,\"screens\":21,\"saveWrites\":false,\"kind\":\"native Mac UI fixture; not child comprehension or phone performance testing\"}");
            Application.Quit();
        }
    }
}
#endif
