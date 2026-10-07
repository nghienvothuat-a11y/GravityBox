#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Opt-in review reel of the bonus "Hiểu ra" of chapter 1 as a player sees it (UI included, -coghe-bonus-reel
    /// &lt;dir&gt;): a save that has just beaten the first boss, the offer, then COghe's three questions in Home (one wrong answer
    /// on purpose), its thank-you and the reward. One PNG per frame at a fixed 30 fps, a ring where each tap lands, and the
    /// sounds it made. Development builds only; writes no campaign save.</summary>
    public sealed class COgheBonusReel : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            var args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-coghe-bonus-reel") < 0 && Array.IndexOf(args, "-coghe-bonus-reel2") < 0) return;
            VenomCampaignSave.PersistenceEnabled = false; COgheProductMode.OverrideForTests = true;
            var go = new GameObject("Bonus reel"); DontDestroyOnLoad(go); go.AddComponent<COgheBonusReel>();
        }

        private string output; private int frame; private bool capturing;
        private RectTransform ring; private Image ringImage; private float ringAge = 99;

        private IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs(); bool second = Array.IndexOf(args, "-coghe-bonus-reel2") >= 0;
            int at = Array.IndexOf(args, second ? "-coghe-bonus-reel2" : "-coghe-bonus-reel");
            output = at + 1 < args.Length && !args[at + 1].StartsWith("-") ? Path.GetFullPath(args[at + 1]) : Path.Combine(Application.persistentDataPath, "BonusReel");
            Directory.CreateDirectory(output);
            Screen.SetResolution(472, 1022, FullScreenMode.Windowed); Application.runInBackground = true;
            COgheIntro.Seen = true;
            var sounds = new System.Text.StringBuilder();
            Action<string, float> heard = (clip, volume) => { if (capturing) sounds.AppendLine($"{frame / 30f:F3} {clip} {volume:F2}"); };
            yield return new WaitForSecondsRealtime(1.5f);
            var game = FindFirstObjectByType<VenomCampaign>();
            if (game == null || game.ProductUI == null) { Debug.LogError("Bonus reel: no menu"); Application.Quit(1); yield break; }
            if (second) { yield return Second(game, heard, sounds); yield break; }
            // level 12, the first boss (its tour plays first, unrecorded)
            game.ProductUI.LoadForTest(12);
            for (int i = 0; i < 600 && (game == null || game.Definition.Order != 12 || game.ProductUI == null || game.ProductUI.Page != COgheProductPage.Game); i++)
            { yield return new WaitForSecondsRealtime(.05f); game = FindFirstObjectByType<VenomCampaign>(); }
            if (game == null || game.Definition.Order != 12) { Debug.LogError("Bonus reel: level 12 did not open"); Application.Quit(1); yield break; }
            var ui = game.ProductUI;
            // a player who has just beaten it
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheShop.TestsOwnUnlocked = false;
            game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
            for (int i = 0; i < 12; i++) game.Progress.Completed.Add(ui.Catalog.Levels[i].Id);
            BuildRing(); yield return new WaitForSecondsRealtime(.5f);
            COgheAudio.Heard += heard; Time.captureFramerate = 30; capturing = true;
            StartCoroutine(CaptureFrames());

            ui.ShowVictoryForTests(ui.Catalog.Levels[11].Id, 12);
            yield return Frames(30);
            ui.VictoryStepForTests(); yield return Frames(40);                     // what level 12 unlocked
            yield return Press(ui, "Continue");
            ui.VictoryStepForTests(); yield return Frames(80);                     // the bonus offer
            yield return Press(ui, "Play bonus");
            var p = game.Personality;
            yield return Until(() => p.BonusAsking, 300); yield return Frames(70); // a heart: "a cuddle?"
            yield return Press(ui, "Feed");                                        // wrong on purpose: a gentle no
            yield return Until(() => p.BonusAsking, 90); yield return Frames(40);
            yield return Touch(game, p.SkinCentre + Vector3.up * .04f);            // right: a touch on the heart
            yield return Until(() => p.BonusRound == 1 && p.BonusAsking, 300); yield return Frames(70);   // a bouncing ball
            var ball = game.HomeRoom.Find("BALL");
            yield return Touch(game, game.HomeRoom.BoundsOf(ball).center);
            yield return Until(() => p.BonusRound == 2 && p.BonusAsking, 900); yield return Frames(60);   // a mushroom: food
            yield return Press(ui, "Feed");
            yield return Until(() => ui.Popup == COgheProductPopup.BonusDone, 1200); yield return Frames(80);
            yield return Press(ui, "Bonus continue");
            yield return Frames(60);

            capturing = false; Time.captureFramerate = 0; COgheAudio.Heard -= heard;
            File.WriteAllText(Path.Combine(output, "sounds.txt"), sounds.ToString());
            File.WriteAllText(Path.Combine(output, "frames.txt"), frame.ToString());
            Debug.Log("COGHE BONUS REEL DONE " + frame);
            Application.Quit(0);
        }

        /// <summary>The second reel (-coghe-bonus-reel2): COghe's affection at Home (hello, gentle touches up to a hug, a
        /// thank-you after a game), then the first question of the bonus of chapters 2–5 (with a wrong order and one too many).</summary>
        private IEnumerator Second(VenomCampaign game, Action<string, float> heard, System.Text.StringBuilder sounds)
        {
            var ui = game.ProductUI;
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheShop.TestsOwnUnlocked = false;
            game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
            foreach (var level in ui.Catalog.Levels) if (level != null) game.Progress.Completed.Add(level.Id);
            BuildRing(); ui.ShowMenu(); yield return new WaitForSecondsRealtime(.5f);
            COgheAudio.Heard += heard; Time.captureFramerate = 30; capturing = true;
            StartCoroutine(CaptureFrames());
            ui.OpenHome();
            var p = game.Personality;
            yield return Until(() => p.Act == COgheAct.Wave, 600); yield return Frames(90);           // hello
            for (int i = 0; i < 3; i++) { yield return Touch(game, p.SkinCentre + Vector3.up * .02f); yield return Frames(i < 2 ? 32 : 80); }   // warmer and warmer: a hug
            yield return Frames(30);
            yield return Touch(game, game.HomeRoom.BoundsOf(game.HomeRoom.Find("BALL")).center);  // a game together
            yield return Until(() => p.Playing != null, 300); yield return Until(() => p.Playing == null, 600); yield return Frames(60);   // its thank-you
            // chapter 2: two things, any order (food first)
            ui.StartBonus(2, false); yield return Until(() => p.BonusAsking, 300); yield return Frames(110);
            yield return Press(ui, "Feed"); yield return Until(() => p.BonusAsking && p.BonusWordsDone == 1, 300); yield return Frames(40);
            yield return Touch(game, p.SkinCentre + Vector3.up * .04f); yield return Until(() => p.BonusRound == 1, 300); yield return Frames(10);
            yield return Press(ui, "Bonus later"); yield return Frames(20);
            // chapter 3: in order (a cuddle first is wrong: food, then a cuddle)
            ui.StartBonus(3, false); yield return Until(() => p.BonusAsking, 300); yield return Frames(110);
            yield return Touch(game, p.SkinCentre + Vector3.up * .04f); yield return Until(() => p.BonusAsking && p.BonusMisses == 1, 120); yield return Frames(30);
            yield return Press(ui, "Feed"); yield return Until(() => p.BonusAsking && p.BonusWordsDone == 1, 300); yield return Frames(30);
            yield return Touch(game, p.SkinCentre + Vector3.up * .04f); yield return Until(() => p.BonusRound == 1, 300); yield return Frames(10);
            yield return Press(ui, "Bonus later"); yield return Frames(20);
            // chapter 4: exactly two (three is one too many: a shake, again)
            ui.StartBonus(4, false); yield return Until(() => p.BonusAsking, 300); yield return Frames(100);
            yield return Press(ui, "Feed"); yield return Frames(8); yield return Press(ui, "Feed"); yield return Frames(8); yield return Press(ui, "Feed");
            yield return Until(() => p.BonusAsking, 300); yield return Frames(40);
            yield return Press(ui, "Feed"); yield return Frames(8); yield return Press(ui, "Feed");
            yield return Until(() => p.BonusRound == 1, 400); yield return Frames(10);
            yield return Press(ui, "Bonus later"); yield return Frames(20);
            // chapter 5: two cuddles, then the ball
            ui.StartBonus(5, false); yield return Until(() => p.BonusAsking, 300); yield return Frames(120);
            yield return Touch(game, p.SkinCentre + Vector3.up * .04f); yield return Frames(12); yield return Touch(game, p.SkinCentre + Vector3.up * .04f);
            yield return Until(() => p.BonusAsking && p.BonusWordsDone == 1, 300); yield return Frames(40);
            yield return Touch(game, game.HomeRoom.BoundsOf(game.HomeRoom.Find("BALL")).center);
            yield return Until(() => p.BonusRound == 1, 900); yield return Frames(30);
            capturing = false; Time.captureFramerate = 0; COgheAudio.Heard -= heard;
            File.WriteAllText(Path.Combine(output, "sounds.txt"), sounds.ToString());
            File.WriteAllText(Path.Combine(output, "frames.txt"), frame.ToString());
            Debug.Log("COGHE BONUS REEL DONE " + frame);
            Application.Quit(0);
        }

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
        private static IEnumerator Until(Func<bool> done, int limit)
        {
            for (int i = 0; i < limit && !done(); i++) yield return null;
            if (!done()) { Debug.LogError("Bonus reel: timed out"); Application.Quit(1); }
        }

        // taps, shown as a ring where the finger lands
        private IEnumerator Press(COgheProductUI ui, string button)
        {
            Canvas.ForceUpdateCanvases();
            var b = ui.GetComponentsInChildren<Button>().FirstOrDefault(x => x.name == button);
            if (b == null) { Debug.LogError("Bonus reel: no button " + button); Application.Quit(1); yield break; }
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
