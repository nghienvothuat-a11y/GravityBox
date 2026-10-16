#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Opt-in review reel of the ability cards (-coghe-ability-reel &lt;dir&gt;, Mrk 16/10/2026): Home with a card
    /// waiting, the Ability bars, the choice of Intelligence and its one-time tip, then a level where COghe, after a pause,
    /// walks to the device it remembers, waits for the player's tap, and wonders when it remembers no further. One PNG per
    /// frame at 30 fps and a ring where each tap lands. Development builds only; writes no save.</summary>
    public sealed class COgheAbilityReel : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-coghe-ability-reel") < 0) return;
            VenomCampaignSave.PersistenceEnabled = false; COgheProductMode.OverrideForTests = true;
            var go = new GameObject("Ability reel"); DontDestroyOnLoad(go); go.AddComponent<COgheAbilityReel>();
        }

        private string output; private int frame; private bool capturing;
        private RectTransform ring; private Image ringImage; private float ringAge = 99;

        private IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-coghe-ability-reel");
            output = at + 1 < args.Length && !args[at + 1].StartsWith("-") ? Path.GetFullPath(args[at + 1]) : Path.Combine(Application.persistentDataPath, "AbilityReel");
            Directory.CreateDirectory(output);
            Screen.SetResolution(472, 1022, FullScreenMode.Windowed); Application.runInBackground = true; COgheIntro.Seen = true;
            yield return new WaitForSecondsRealtime(1.5f);
            var game = FindFirstObjectByType<VenomCampaign>();
            if (game == null || game.ProductUI == null) { Fail("no menu"); yield break; }
            var ui = game.ProductUI; var marks = new System.Text.StringBuilder();
            // a player who has just understood COghe after the first Boss, with a few cards already
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheShop.TestsOwnUnlocked = false;
            COgheHomeRoom.UnlockedLevelOverride = 10;
            game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
            for (int i = 0; i < 10; i++) game.Progress.Completed.Add(ui.Catalog.Levels[i].Id);
            COgheShop.Current.Emotion = 3; COgheShop.Current.Agility = 2;
            COgheShop.Understand(COgheBonus.Key(1));
            BuildRing(); ui.ShowMenu(); yield return new WaitForSecondsRealtime(.6f);
            ui.OpenHome(); yield return new WaitForSecondsRealtime(3f);   // furniture and hello, unrecorded
            Time.captureFramerate = 30; capturing = true; StartCoroutine(CaptureFrames());
            marks.AppendLine($"{frame} home"); yield return Frames(45);
            yield return Press(ui, "Ability"); marks.AppendLine($"{frame} stats"); yield return Frames(75);
            yield return Press(ui, "Choose card"); marks.AppendLine($"{frame} pick"); yield return Frames(50);
            yield return Press(ui, "Intelligence card"); yield return Frames(30);
            yield return Press(ui, "Choose ability"); marks.AppendLine($"{frame} chosen"); yield return Frames(80);
            yield return Press(ui, "Ability continue"); yield return Frames(25);
            capturing = false; Time.captureFramerate = 0;
            // the level: G2 (rail A, then rail B); COghe remembers one step
            int g2 = Array.FindIndex(ui.Catalog.Levels, l => l != null && l.Id == "coghe.spatial.plus.g2") + 1;
            ui.LoadForTest(g2);
            for (int i = 0; i < 600 && (game == null || game.Definition.Order != g2 || game.ProductUI == null || game.ProductUI.Page != COgheProductPage.Game); i++)
            { yield return new WaitForSecondsRealtime(.05f); game = FindFirstObjectByType<VenomCampaign>(); }
            if (game == null || game.Definition.Order != g2 || game.Intelligence == null) { Fail("G2 did not open with Intelligence"); yield break; }
            yield return new WaitForSecondsRealtime(1.5f);
            Time.captureFramerate = 30; capturing = true; StartCoroutine(CaptureFrames());
            marks.AppendLine($"{frame} level");
            var intel = game.Intelligence;
            yield return Until(() => intel.GuidedStep == 0 && game.Motion.Get(game.Motion.Selected) == null, 30 * 25);
            marks.AppendLine($"{frame} beside A"); yield return Frames(20);
            var a = game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>().First(r => r.Label == "A");
            yield return Touch(game, a.HandPoint); marks.AppendLine($"{frame} tap A");
            yield return Until(() => intel.Progress >= 1, 30 * 30);
            yield return Until(() => intel.Puzzled > 0, 30 * 12); marks.AppendLine($"{frame} wonders");
            yield return Frames(90);
            capturing = false; Time.captureFramerate = 0;
            File.WriteAllText(Path.Combine(output, "marks.txt"), marks.ToString());
            File.WriteAllText(Path.Combine(output, "native-result.json"), "{\"result\":\"passed\",\"frames\":" + frame + "}");
            Application.Quit(0);
        }

        private void Fail(string why) { Debug.LogError("Ability reel: " + why); File.WriteAllText(Path.Combine(output, "native-result.json"), "{\"result\":\"failed\",\"reason\":\"" + why + "\"}"); Application.Quit(1); }
        private IEnumerator CaptureFrames()
        {
            while (capturing)
            {
                yield return new WaitForEndOfFrame();
                if (!capturing) yield break;
                ScreenCapture.CaptureScreenshot(Path.Combine(output, $"frame_{frame:00000}.png")); frame++;
            }
        }
        private static IEnumerator Frames(int n) { for (int i = 0; i < n; i++) yield return null; }
        private IEnumerator Until(Func<bool> done, int limit)
        {
            for (int i = 0; i < limit && !done(); i++) yield return null;
            if (!done()) Fail("timed out");
        }
        private IEnumerator Press(COgheProductUI ui, string button)
        {
            Canvas.ForceUpdateCanvases();
            var b = ui.GetComponentsInChildren<Button>().FirstOrDefault(x => x.name == button);
            if (b == null) { Fail("no button " + button); yield break; }
            var rect = (RectTransform)b.transform;
            ShowRing(RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)));
            yield return Frames(6); COgheAudio.UiTap(); b.onClick.Invoke(); yield return Frames(4);
        }
        private IEnumerator Touch(VenomCampaign game, Vector3 world)
        {
            Vector2 screen = game.Owner.View.WorldToScreenPoint(world);
            ShowRing(screen); yield return Frames(6); game.TouchPoint(screen); yield return Frames(4);
        }
        private void BuildRing()
        {
            var canvas = new GameObject("Tap ring canvas", typeof(Canvas)).GetComponent<Canvas>(); DontDestroyOnLoad(canvas.gameObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 1000;
            const int n = 64; var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                float d = new Vector2(x + .5f - n * .5f, y + .5f - n * .5f).magnitude / (n * .5f);
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(1 - Mathf.Abs(d - .8f) / .12f) * .9f + (d < .3f ? .5f : 0)));
            }
            tex.Apply();
            ringImage = new GameObject("Tap ring", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            ringImage.sprite = Sprite.Create(tex, new Rect(0, 0, n, n), Vector2.one * .5f); ringImage.raycastTarget = false;
            ring = ringImage.rectTransform; ring.SetParent(canvas.transform, false); ring.anchorMin = ring.anchorMax = Vector2.zero;
            ringImage.enabled = false;
        }
        private void ShowRing(Vector2 screen) { ring.anchoredPosition = screen; ringAge = 0; ringImage.enabled = true; }
        private void Update()
        {
            if (ringImage == null || !ringImage.enabled) return;
            ringAge += Time.deltaTime;
            float k = Mathf.Clamp01(ringAge / .5f);
            ring.sizeDelta = Vector2.one * Mathf.Lerp(46, 70, k);
            ringImage.color = new Color(.21f, .43f, .43f, 1 - k * k);
            if (k >= 1) ringImage.enabled = false;
        }
    }
}
#endif
