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
            if (Array.IndexOf(args, "-coghe-bonus-reel") < 0 && Array.IndexOf(args, "-coghe-bonus-reel2") < 0 && Array.IndexOf(args, "-coghe-trailer-reel") < 0 && Array.IndexOf(args, "-coghe-toy-reel") < 0 && Array.IndexOf(args, "-coghe-toy-reel2") < 0 && Array.IndexOf(args, "-coghe-toy-reel3") < 0 && Array.IndexOf(args, "-coghe-toy-reel4") < 0 && Array.IndexOf(args, "-coghe-chapter7-reel") < 0 && Array.IndexOf(args, "-coghe-collection-reel") < 0) return;
            VenomCampaignSave.PersistenceEnabled = false; COgheProductMode.OverrideForTests = true;
            var go = new GameObject("Bonus reel"); DontDestroyOnLoad(go); go.AddComponent<COgheBonusReel>();
        }

        private string output; private int frame, superSize = 1; private bool capturing;
        private const int MaxFrames = 9000;   // 5 minutes at 30 fps
        private RectTransform ring; private Image ringImage; private float ringAge = 99;

        private IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs(); bool second = Array.IndexOf(args, "-coghe-bonus-reel2") >= 0, trailer = Array.IndexOf(args, "-coghe-trailer-reel") >= 0, toys = Array.IndexOf(args, "-coghe-toy-reel") >= 0, toys2 = Array.IndexOf(args, "-coghe-toy-reel2") >= 0, toys3 = Array.IndexOf(args, "-coghe-toy-reel3") >= 0, toys4 = Array.IndexOf(args, "-coghe-toy-reel4") >= 0, later = Array.IndexOf(args, "-coghe-chapter7-reel") >= 0, collection = Array.IndexOf(args, "-coghe-collection-reel") >= 0;
            int at = Array.IndexOf(args, collection ? "-coghe-collection-reel" : later ? "-coghe-chapter7-reel" : toys4 ? "-coghe-toy-reel4" : toys3 ? "-coghe-toy-reel3" : toys2 ? "-coghe-toy-reel2" : toys ? "-coghe-toy-reel" : trailer ? "-coghe-trailer-reel" : second ? "-coghe-bonus-reel2" : "-coghe-bonus-reel");
            output = at + 1 < args.Length && !args[at + 1].StartsWith("-") ? Path.GetFullPath(args[at + 1]) : Path.Combine(Application.persistentDataPath, "BonusReel");
            Directory.CreateDirectory(output);
            // the trailer wants 1080×1920: a 540×960 window (it fits any screen) captured at twice its size
            if (trailer) { Screen.SetResolution(540, 960, FullScreenMode.Windowed); superSize = 2; } else Screen.SetResolution(472, 1022, FullScreenMode.Windowed);
            Application.runInBackground = true;
            COgheIntro.Seen = true;
            var sounds = new System.Text.StringBuilder();
            Action<string, float> heard = (clip, volume) => { if (capturing) sounds.AppendLine($"{frame / 30f:F3} {clip} {volume:F2}"); };
            yield return new WaitForSecondsRealtime(1.5f);
            var game = FindFirstObjectByType<VenomCampaign>();
            if (game == null || game.ProductUI == null) { Debug.LogError("Bonus reel: no menu"); Application.Quit(1); yield break; }
            if (second) { yield return Second(game, heard, sounds); yield break; }
            if (trailer) { yield return Trailer(game, heard, sounds); yield break; }
            if (toys) { yield return Toys(game, heard, sounds); yield break; }
            if (toys2) { yield return Toys2(game, heard, sounds); yield break; }
            if (toys3) { yield return Toys3(game, heard, sounds); yield break; }
            if (toys4) { yield return Toys4(game, heard, sounds); yield break; }
            if (later) { yield return Chapter7(game, heard, sounds); yield break; }
            if (collection) { yield return Collection(game, heard, sounds); yield break; }
            // the first boss (its warning plays first, unrecorded)
            int boss = FirstBoss(game.ProductUI);
            game.ProductUI.LoadForTest(boss);
            for (int i = 0; i < 600 && (game == null || game.Definition.Order != boss || game.ProductUI == null || game.ProductUI.Page != COgheProductPage.Game); i++)
            { yield return new WaitForSecondsRealtime(.05f); game = FindFirstObjectByType<VenomCampaign>(); }
            if (game == null || game.Definition.Order != boss) { Debug.LogError($"Bonus reel: level {boss} did not open"); Application.Quit(1); yield break; }
            var ui = game.ProductUI;
            // a player who has just beaten it
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheShop.TestsOwnUnlocked = false;
            game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
            for (int i = 0; i < boss; i++) game.Progress.Completed.Add(ui.Catalog.Levels[i].Id);
            BuildRing(); yield return new WaitForSecondsRealtime(.5f);
            COgheAudio.Heard += heard; Time.captureFramerate = 30; capturing = true;
            StartCoroutine(CaptureFrames());

            ui.ShowVictoryForTests(ui.Catalog.Levels[boss - 1].Id, boss);
            yield return Frames(30);
            ui.VictoryStepForTests(); yield return Frames(40);                     // what the first boss unlocked
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

        /// <summary>The catalog position of the first boss (1-based): it opens Home and the first bonus.</summary>
        private static int FirstBoss(COgheProductUI ui) => System.Array.FindIndex(ui.Catalog.Levels, l => l != null && l.Boss) + 1;

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

        /// <summary>Home footage for the how-to-play clip (-coghe-trailer-reel, 1080×1920): a furnished Home, Feed and its meal,
        /// three gentle touches up to a hug with hearts (close), and one bonus question with its reward. Marks in marks.txt.</summary>
        private IEnumerator Trailer(VenomCampaign game, Action<string, float> heard, System.Text.StringBuilder sounds)
        {
            var ui = game.ProductUI; var marks = new System.Text.StringBuilder();
            void Mark(string what) => marks.AppendLine($"{frame} {what}");
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheHomeRoom.UnlockedLevelOverride = ui.Catalog.Levels.Length;   // every piece of furniture
            game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
            for (int i = 0, boss = FirstBoss(ui); i < boss; i++) game.Progress.Completed.Add(ui.Catalog.Levels[i].Id);
            BuildRing(); ui.ShowMenu(); yield return new WaitForSecondsRealtime(.8f);
            ui.OpenHome(); var p = game.Personality;
            yield return Until(() => p.Act == COgheAct.Wave, 900); yield return new WaitForSecondsRealtime(.5f);   // furniture popped in, hello said: unrecorded
            COgheAudio.Heard += heard; Time.captureFramerate = 30; capturing = true;
            StartCoroutine(CaptureFrames());
            yield return Frames(40);
            Mark("feed"); yield return Press(ui, "Feed");
            yield return Until(() => p.BallsEaten >= 3, 900); yield return Frames(20); Mark("fed");
            ui.ToggleHomeZoom(); yield return Frames(45); Mark("touches");
            for (int i = 0; i < 3; i++) { yield return Touch(game, p.SkinCentre + Vector3.up * .02f); yield return Frames(i < 2 ? 34 : 75); }
            Mark("hugged"); ui.ToggleHomeZoom(); yield return Frames(20);
            Mark("bonus"); ui.StartBonus(1, false); yield return Until(() => p.BonusAsking, 300); yield return Frames(55);
            Mark("bonus-touch"); yield return Touch(game, p.SkinCentre + Vector3.up * .04f);
            yield return Frames(75); Mark("bonus-joy-done");
            capturing = false; Time.captureFramerate = 0; COgheAudio.Heard -= heard;
            File.WriteAllText(Path.Combine(output, "sounds.txt"), sounds.ToString());
            File.WriteAllText(Path.Combine(output, "marks.txt"), marks.ToString());
            File.WriteAllText(Path.Combine(output, "frames.txt"), frame.ToString());
            Debug.Log("COGHE BONUS REEL DONE " + frame);
            Application.Quit(0);
        }

        /// <summary>The toy reel (-coghe-toy-reel, Mrk 17/10/2026): chapter 2's last question, the drum game (a touch instead of
        /// the drum, a beat played back, one strike too many, the jam, the thank-you, then the drum played at Home), and the gags
        /// for wrong answers in chapters 1, 3, 4 and 5. Marks in marks.txt.</summary>
        private IEnumerator Toys(VenomCampaign game, Action<string, float> heard, System.Text.StringBuilder sounds)
        {
            var ui = game.ProductUI; var marks = new System.Text.StringBuilder();
            void Mark(string what) => marks.AppendLine($"{frame} {what}");
            // a player who has beaten the second boss: the furniture of those levels bought, no card waiting
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheShop.TestsOwnUnlocked = false;
            game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
            int bosses = 0, reached = 0;
            for (int i = 0; i < ui.Catalog.Levels.Length; i++) { game.Progress.Completed.Add(ui.Catalog.Levels[i].Id); if (ui.Catalog.Levels[i].Boss && ++bosses == 2) { reached = i + 1; break; } }
            foreach (var e in COgheHomeItems.Catalog) if (e.level <= reached) COgheShop.Grant(e.id, "reel");
            COgheShop.Current.CardsPicked.Add(1); COgheShop.Current.CardsPicked.Add(2);
            COgheShop.Understand(COgheBonus.Key(1)); COgheShop.Understand(COgheBonus.RoundKey(2, 1)); COgheShop.Understand(COgheBonus.RoundKey(2, 2));   // straight to the game
            BuildRing(); ui.ShowMenu(); yield return new WaitForSecondsRealtime(.8f);
            ui.OpenHome(); var p = game.Personality;
            yield return Until(() => p.Act == COgheAct.Wave, 900); yield return new WaitForSecondsRealtime(.5f);   // furniture popped in, hello said: unrecorded
            COgheAudio.Heard += heard; Time.captureFramerate = 30; capturing = true;
            StartCoroutine(CaptureFrames());
            IEnumerator Drum(int n, int gap)
            {
                for (int i = 0; i < n; i++) { var b = game.HomeRoom.BoundsOf(game.HomeRoom.Find(COgheToys.Drum)); yield return Touch(game, b.center + Vector3.up * .015f); yield return Frames(gap); }
            }
            // the drum game
            Mark("game"); ui.StartBonus(2, false);
            yield return Until(() => p.ToyShowing, 300); Mark("intro");
            yield return Until(() => p.ToyListening, 600); Mark("turn"); yield return Frames(25);
            Mark("touch-wrong"); yield return Touch(game, p.SkinCentre + Vector3.up * .01f);
            yield return Until(() => p.ToyListening, 300); Mark("turn"); yield return Frames(20);
            Mark("back2"); yield return Drum(2, 8);
            yield return Until(() => p.ToyListening && p.ToyPattern == 1, 300); Mark("turn3"); yield return Frames(20);
            Mark("too-many"); yield return Drum(4, 4);
            yield return Until(() => p.ToyListening, 300); Mark("turn3"); yield return Frames(20);
            Mark("back3"); yield return Drum(3, 8);
            yield return Until(() => p.ToyListening && p.ToyPattern == 2, 300); Mark("turn4"); yield return Frames(20);
            Mark("back4"); yield return Drum(4, 8);
            yield return Until(() => p.ToyJamming, 300); Mark("jam");
            for (int i = 0; i < 7; i++) { yield return Frames(12); yield return Drum(1, 0); }
            yield return Until(() => ui.Popup == COgheProductPopup.BonusDone, 900); Mark("done"); yield return Frames(70);
            yield return Press(ui, "Bonus continue"); yield return Frames(40);
            Mark("home-drum"); yield return Drum(1, 0);
            yield return Until(() => p.Playing != null, 400); yield return Until(() => p.Playing == null, 600); yield return Frames(20);
            // the gags: chapter 1 (a heart: food, then the ball, instead), then the ball wanted and a touch instead
            p.EndBonus(); Mark("ch1"); ui.StartBonus(1, false);
            yield return Until(() => p.BonusAsking, 400); yield return Frames(45);
            Mark("gag-spit"); yield return Press(ui, "Feed"); yield return Until(() => p.BonusAsking, 300); yield return Frames(20);
            Mark("gag-bonk"); yield return Touch(game, game.HomeRoom.BoundsOf(game.HomeRoom.Find("BALL")).center);
            yield return Until(() => p.BonusAsking, 300); yield return Frames(15);
            Mark("right"); yield return Touch(game, p.SkinCentre + Vector3.up * .04f);
            yield return Until(() => p.BonusRound == 1 && p.BonusAsking, 400); Mark("ball-wanted"); yield return Frames(45);
            Mark("gag-point"); yield return Touch(game, p.SkinCentre + Vector3.up * .01f);
            yield return Until(() => p.BonusAsking, 300); yield return Frames(20);
            // chapter 3 (food first): a touch instead
            p.EndBonus(); Mark("ch3"); ui.StartBonus(3, false);
            yield return Until(() => p.BonusAsking, 400); yield return Frames(80);
            Mark("gag-rumble"); yield return Touch(game, p.SkinCentre + Vector3.up * .01f);
            yield return Until(() => p.BonusAsking, 300); yield return Frames(20);
            // chapter 4 (exactly two food): three
            p.EndBonus(); Mark("ch4"); ui.StartBonus(4, false);
            yield return Until(() => p.BonusAsking, 400); yield return Frames(70);
            Mark("gag-hiccup"); yield return Press(ui, "Feed"); yield return Frames(6); yield return Press(ui, "Feed"); yield return Frames(6); yield return Press(ui, "Feed");
            yield return Until(() => p.BonusAsking, 400); yield return Frames(20);
            // chapter 5 (two cuddles): three
            p.EndBonus(); Mark("ch5"); ui.StartBonus(5, false);
            yield return Until(() => p.BonusAsking, 400); yield return Frames(80);
            Mark("gag-roll"); for (int i = 0; i < 3; i++) yield return Touch(game, p.SkinCentre + Vector3.up * .02f);
            yield return Until(() => p.BonusAsking, 300); yield return Frames(30);
            Mark("end");
            capturing = false; Time.captureFramerate = 0; COgheAudio.Heard -= heard;
            File.WriteAllText(Path.Combine(output, "sounds.txt"), sounds.ToString());
            File.WriteAllText(Path.Combine(output, "marks.txt"), marks.ToString());
            File.WriteAllText(Path.Combine(output, "frames.txt"), frame.ToString());
            Debug.Log("COGHE BONUS REEL DONE " + frame);
            Application.Quit(0);
        }

        /// <summary>The second toy reel (-coghe-toy-reel2): the drum as a word, chapter 3 (out of turn, then in order) and
        /// chapter 4 (three strikes). Marks in marks.txt.</summary>
        private IEnumerator Toys2(VenomCampaign game, Action<string, float> heard, System.Text.StringBuilder sounds)
        {
            var ui = game.ProductUI; var marks = new System.Text.StringBuilder();
            void Mark(string what) => marks.AppendLine($"{frame} {what}");
            // a player who has beaten the third boss and won the drum: the furniture of those levels bought, no card waiting
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheShop.TestsOwnUnlocked = false;
            game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
            int bosses = 0, reached = 0;
            for (int i = 0; i < ui.Catalog.Levels.Length; i++) { game.Progress.Completed.Add(ui.Catalog.Levels[i].Id); if (ui.Catalog.Levels[i].Boss && ++bosses == 3) { reached = i + 1; break; } }
            foreach (var e in COgheHomeItems.Catalog) if (e.level <= reached) COgheShop.Grant(e.id, "reel");
            COgheShop.Grant(COgheToys.Drum, "reel");
            for (int c = 1; c <= 4; c++) COgheShop.Current.CardsPicked.Add(c);
            COgheShop.Understand(COgheBonus.RoundKey(3, 1)); COgheShop.Understand(COgheBonus.RoundKey(4, 1));   // chapters 3, 4: the drum questions
            BuildRing(); ui.ShowMenu(); yield return new WaitForSecondsRealtime(.8f);
            ui.OpenHome(); var p = game.Personality;
            yield return Until(() => p.Act == COgheAct.Wave, 900); yield return new WaitForSecondsRealtime(.5f);
            COgheAudio.Heard += heard; Time.captureFramerate = 30; capturing = true;
            StartCoroutine(CaptureFrames());
            // the room is rebuilt when Home opens again: look the drum up each time
            Vector3 DrumAt() => game.HomeRoom.BoundsOf(game.HomeRoom.Find(COgheToys.Drum)).center + Vector3.up * .015f;
            // chapter 3, question 2: a cuddle, the drum, food (the drum out of turn first)
            Mark("drum3"); ui.StartBonus(3, false);
            yield return Until(() => p.BonusAsking, 400); yield return Frames(90);
            Mark("drum-startle"); yield return Touch(game, DrumAt());
            yield return Until(() => p.BonusAsking, 300); yield return Frames(15);
            Mark("drum3-touch"); yield return Touch(game, p.SkinCentre + Vector3.up * .04f);
            yield return Until(() => p.BonusAsking && p.BonusWordsDone == 1, 300); Mark("drum-word"); yield return Frames(75);
            Mark("drum-tap"); yield return Touch(game, DrumAt());
            yield return Until(() => p.BonusAsking && p.BonusWordsDone == 2, 600); Mark("drum3-food"); yield return Frames(30);
            yield return Press(ui, "Feed");
            yield return Until(() => p.BonusRound == 2, 400); yield return Frames(20);
            // chapter 4, question 2: exactly three strikes
            p.EndBonus(); Mark("drum4"); ui.StartBonus(4, false);
            yield return Until(() => p.BonusAsking, 400); yield return Frames(100);
            Mark("drum4-taps"); for (int i = 0; i < 3; i++) { yield return Touch(game, DrumAt()); yield return Frames(6); }
            yield return Until(() => p.BonusRound == 2, 400); yield return Frames(20);
            Mark("end");
            capturing = false; Time.captureFramerate = 0; COgheAudio.Heard -= heard;
            File.WriteAllText(Path.Combine(output, "sounds.txt"), sounds.ToString());
            File.WriteAllText(Path.Combine(output, "marks.txt"), marks.ToString());
            File.WriteAllText(Path.Combine(output, "frames.txt"), frame.ToString());
            Debug.Log("COGHE BONUS REEL DONE " + frame);
            Application.Quit(0);
        }

        /// <summary>The third toy reel (-coghe-toy-reel3): chapter 3's horn (a wrong button first), chapter 4's balloons (one
        /// puff too many first), then both played at Home. Marks in marks.txt.</summary>
        private IEnumerator Toys3(VenomCampaign game, Action<string, float> heard, System.Text.StringBuilder sounds)
        {
            var ui = game.ProductUI; var marks = new System.Text.StringBuilder();
            void Mark(string what) => marks.AppendLine($"{frame} {what}");
            // a player who has beaten the fourth boss and won the drum: the furniture of those levels bought, no card waiting
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheShop.TestsOwnUnlocked = false;
            game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
            int bosses = 0, reached = 0;
            for (int i = 0; i < ui.Catalog.Levels.Length; i++) { game.Progress.Completed.Add(ui.Catalog.Levels[i].Id); if (ui.Catalog.Levels[i].Boss && ++bosses == 4) { reached = i + 1; break; } }
            foreach (var e in COgheHomeItems.Catalog) if (e.level <= reached) COgheShop.Grant(e.id, "reel");
            COgheShop.Grant(COgheToys.Drum, "reel");
            for (int c = 1; c <= 4; c++) COgheShop.Current.CardsPicked.Add(c);
            for (int r = 1; r <= 2; r++) { COgheShop.Understand(COgheBonus.RoundKey(3, r)); COgheShop.Understand(COgheBonus.RoundKey(4, r)); }   // straight to the games
            BuildRing(); ui.ShowMenu(); yield return new WaitForSecondsRealtime(.8f);
            ui.OpenHome(); var p = game.Personality;
            yield return Until(() => p.Act == COgheAct.Wave, 900); yield return new WaitForSecondsRealtime(.5f);
            COgheAudio.Heard += heard; Time.captureFramerate = 30; capturing = true;
            StartCoroutine(CaptureFrames());
            Vector3 Key(int k) => game.HomeRoom.Find(COgheToys.Horn).Part("Key" + k).position;
            // chapter 3: the horn
            Mark("horn"); ui.StartBonus(3, false);
            yield return Until(() => p.ToyShowing, 300); Mark("horn-intro");
            yield return Until(() => p.ToyWaitingForPlayer, 600); Mark("horn-turn"); yield return Frames(20);
            Mark("horn-wrong"); yield return Touch(game, Key((p.ToyTune[0] + 1) % 3));
            for (int tune = 0; tune < 3; tune++)
            {
                yield return Until(() => p.ToyWaitingForPlayer && p.Gag == COgheGag.None, 600); yield return Frames(15);
                Mark("horn-back" + tune); foreach (int k in p.ToyTune) { yield return Touch(game, Key(k)); yield return Frames(6); }
                yield return Until(() => !p.ToyListening, 300);
            }
            yield return Until(() => p.ToyJamming, 300); Mark("horn-jam");
            for (int i = 0; i < 5; i++) { yield return Frames(12); yield return Touch(game, Key(i % 3)); }
            yield return Until(() => p.BonusThanking, 600); Mark("horn-done"); yield return Frames(30);
            // chapter 4: the balloons
            p.EndBonus(); Mark("balloon"); ui.StartBonus(4, false);
            yield return Until(() => p.ToyShowing, 300); Mark("balloon-intro");
            yield return Until(() => p.ToyWaitingForPlayer, 600); Mark("balloon-turn"); yield return Frames(15);
            Mark("balloon-pop"); for (int i = 0; i < 3; i++) { yield return Touch(game, p.ToyTapPoint); yield return Frames(8); }
            for (int b = 0; b < 3; b++)
            {
                yield return Until(() => p.ToyWaitingForPlayer && p.Gag == COgheGag.None, 600); yield return Frames(10);
                Mark("balloon-" + b); int n = p.ToyBeats; for (int i = 0; i < n; i++) { yield return Touch(game, p.ToyTapPoint); yield return Frames(10); }
                yield return Until(() => !p.ToyListening, 300);
            }
            yield return Until(() => p.ToyJamming, 300); Mark("balloon-fly");
            for (int i = 0; i < 4; i++) { yield return Frames(20); yield return Touch(game, p.ToyTapPoint); }
            yield return Until(() => p.BonusThanking, 600); Mark("balloon-done"); yield return Frames(30);
            // at Home: a tap on each
            p.EndBonus(); yield return Frames(30);
            Mark("home-horn"); yield return Touch(game, game.HomeRoom.BoundsOf(game.HomeRoom.Find(COgheToys.Horn)).center);
            yield return Until(() => p.Playing != null, 400); yield return Until(() => p.Playing == null, 600); yield return Frames(20);
            Mark("home-balloon"); yield return Touch(game, game.HomeRoom.Find(COgheToys.Balloon).Part("Balloon1").position);
            yield return Until(() => p.Playing != null, 400); yield return Until(() => p.Playing == null, 600); yield return Frames(30);
            Mark("end");
            capturing = false; Time.captureFramerate = 0; COgheAudio.Heard -= heard;
            File.WriteAllText(Path.Combine(output, "sounds.txt"), sounds.ToString());
            File.WriteAllText(Path.Combine(output, "marks.txt"), marks.ToString());
            File.WriteAllText(Path.Combine(output, "frames.txt"), frame.ToString());
            Debug.Log("COGHE BONUS REEL DONE " + frame);
            Application.Quit(0);
        }

        /// <summary>The fourth toy reel (-coghe-toy-reel4): chapter 5's spinning top (left to fall once first), chapter 6's
        /// music box (a touch on COghe first), then both played at Home. Marks in marks.txt.</summary>
        private IEnumerator Toys4(VenomCampaign game, Action<string, float> heard, System.Text.StringBuilder sounds)
        {
            var ui = game.ProductUI; var marks = new System.Text.StringBuilder();
            void Mark(string what) => marks.AppendLine($"{frame} {what}");
            // a player who has beaten the sixth boss and won the earlier toys: the furniture bought, no card waiting
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheShop.TestsOwnUnlocked = false;
            game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
            int bosses = 0, reached = 0;
            for (int i = 0; i < ui.Catalog.Levels.Length; i++) { game.Progress.Completed.Add(ui.Catalog.Levels[i].Id); if (ui.Catalog.Levels[i].Boss && ++bosses == 6) { reached = i + 1; break; } }
            foreach (var e in COgheHomeItems.Catalog) if (e.level <= reached) COgheShop.Grant(e.id, "reel");
            foreach (var t in new[] { COgheToys.Drum, COgheToys.Horn, COgheToys.Balloon }) COgheShop.Grant(t, "reel");
            for (int c = 1; c <= 6; c++) COgheShop.Current.CardsPicked.Add(c);
            for (int r = 1; r <= 2; r++) { COgheShop.Understand(COgheBonus.RoundKey(5, r)); COgheShop.Understand(COgheBonus.RoundKey(6, r)); }   // straight to the games
            BuildRing(); ui.ShowMenu(); yield return new WaitForSecondsRealtime(.8f);
            ui.OpenHome(); var p = game.Personality;
            yield return Until(() => p.Act == COgheAct.Wave, 3000);   // many new things pop in first, and it visits the newest yield return new WaitForSecondsRealtime(.5f);
            COgheAudio.Heard += heard; Time.captureFramerate = 30; capturing = true;
            StartCoroutine(CaptureFrames());
            // chapter 5: the spinning top
            Mark("top"); ui.StartBonus(5, false);
            yield return Until(() => p.ToyShowing, 300); Mark("top-intro");
            yield return Until(() => p.ToyWaitingForPlayer, 600); Mark("top-wobble");
            yield return Until(() => p.Gag == COgheGag.TopFall, 600); Mark("top-fall");
            for (int r = 0; r < COghePersonality.TopRescues; r++)
            {
                yield return Until(() => p.ToyWaitingForPlayer && p.ToyWobbling && p.Gag == COgheGag.None, 900); yield return Frames(10 + 8 * r);
                Mark("top-save" + r); yield return Touch(game, p.ToyTapPoint);
                yield return Until(() => !p.ToyListening, 300);
            }
            yield return Until(() => p.ToyJamming, 300); Mark("top-jam");
            for (int i = 0; i < 4; i++) { yield return Frames(20); yield return Touch(game, p.ToyTapPoint); }
            yield return Until(() => p.BonusThanking, 600); Mark("top-done"); yield return Frames(30);
            // chapter 6: the music box
            p.EndBonus(); Mark("box"); ui.StartBonus(6, false);
            yield return Until(() => p.ToyShowing, 300); Mark("box-intro");
            yield return Until(() => p.ToyWaitingForPlayer, 600); Mark("box-turn"); yield return Frames(15);
            Mark("box-wrong"); yield return Touch(game, p.SkinCentre + Vector3.up * .01f);
            for (int tune = 0; tune < 3; tune++)
            {
                yield return Until(() => p.ToyWaitingForPlayer && p.Gag == COgheGag.None, 600); yield return Frames(10);
                Mark("box-tune" + tune); for (int i = 0; i < 3; i++) { yield return Touch(game, p.ToyTapPoint); yield return Frames(8); }
                yield return Until(() => !p.ToyListening, 300);
            }
            yield return Until(() => p.ToyJamming, 300); Mark("box-dance");
            for (int i = 0; i < 4; i++) { yield return Frames(22); yield return Touch(game, p.ToyTapPoint); }
            yield return Until(() => p.BonusThanking, 600); Mark("box-done"); yield return Frames(30);
            // at Home: a tap on each
            p.EndBonus(); yield return Frames(30);
            Mark("home-top"); yield return Touch(game, game.HomeRoom.BoundsOf(game.HomeRoom.Find(COgheToys.Top)).center);
            yield return Until(() => p.Playing != null, 400); yield return Until(() => p.Playing == null, 600); yield return Frames(20);
            Mark("home-box"); yield return Touch(game, game.HomeRoom.BoundsOf(game.HomeRoom.Find(COgheToys.MusicBox)).center);
            yield return Until(() => p.Playing != null, 400); yield return Until(() => p.Playing == null, 600); yield return Frames(30);
            Mark("end");
            capturing = false; Time.captureFramerate = 0; COgheAudio.Heard -= heard;
            File.WriteAllText(Path.Combine(output, "sounds.txt"), sounds.ToString());
            File.WriteAllText(Path.Combine(output, "marks.txt"), marks.ToString());
            File.WriteAllText(Path.Combine(output, "frames.txt"), frame.ToString());
            Debug.Log("COGHE BONUS REEL DONE " + frame);
            Application.Quit(0);
        }

        /// <summary>The generated chapters (-coghe-chapter7-reel): chapter 7's "Hiểu ra" played through as a player would,
        /// answering whatever COghe asks (one toy tapped out of turn first), then its harder drum game. Each question's words go
        /// to marks.txt ("q1 HORNx1,feedx2,..."), for the captions.</summary>
        private IEnumerator Chapter7(VenomCampaign game, Action<string, float> heard, System.Text.StringBuilder sounds)
        {
            var ui = game.ProductUI; var marks = new System.Text.StringBuilder();
            void Mark(string what) => marks.AppendLine($"{frame} {what}");
            // a player who has beaten every boss and owns every toy; no card waiting
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheShop.TestsOwnUnlocked = false;
            game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
            foreach (var level in ui.Catalog.Levels) if (level != null) game.Progress.Completed.Add(level.Id);
            foreach (var e in COgheHomeItems.Catalog) COgheShop.Grant(e.id, "reel");
            foreach (var t in COgheToys.Catalog) COgheShop.Grant(t.id, "reel");
            for (int c = 1; c <= 7; c++) COgheShop.Current.CardsPicked.Add(c);
            BuildRing(); ui.ShowMenu(); yield return new WaitForSecondsRealtime(.8f);
            ui.OpenHome(); var p = game.Personality;
            yield return Until(() => p.Act == COgheAct.Wave, 3000); yield return new WaitForSecondsRealtime(.5f);
            COgheAudio.Heard += heard; Time.captureFramerate = 30; capturing = true;
            StartCoroutine(CaptureFrames());
            Vector3 ItemAt(string id) => id == "BALL" ? game.HomeRoom.BoundsOf(game.HomeRoom.Find("BALL")).center : game.HomeRoom.BoundsOf(game.HomeRoom.Find(id)).center + Vector3.up * .01f;
            string Describe(COgheBonusRound round) => string.Join(",", round.Words.Select(w => (w.Answer == COgheBonusAnswer.Item ? w.Item : w.Answer.ToString()) + "x" + w.Count));
            var rounds = COgheBonus.Rounds(7);
            Mark("ch7"); ui.StartBonus(7, false);
            for (int q = 0; q < 2; q++)
            {
                int round = q;
                yield return Until(() => p.BonusAsking && p.BonusRound == round, 1200); Mark("q" + (q + 1) + " " + Describe(rounds[q]));
                if (q == 0)
                {   // a toy out of turn first: a fright
                    string wrong = COgheToys.Catalog.Select(t => t.id).First(id => id != rounds[0].Words[0].Item);
                    yield return Frames(90); Mark("startle"); yield return Touch(game, ItemAt(wrong));
                    yield return Until(() => p.BonusAsking, 300);
                }
                while (p.BonusRound == round)
                {
                    yield return Until(() => p.BonusAsking || p.BonusRound != round, 1500);
                    if (p.BonusRound != round) break;
                    yield return Frames(45);
                    var w = p.BonusAsk; if (!w.HasValue) break;
                    int done = p.BonusWordsDone;
                    for (int i = 0; i < w.Value.Count; i++)
                    {
                        if (w.Value.Answer == COgheBonusAnswer.Touch) yield return Touch(game, p.SkinCentre + Vector3.up * .04f);
                        else if (w.Value.Answer == COgheBonusAnswer.Feed) yield return Press(ui, "Feed");
                        else yield return Touch(game, ItemAt(w.Value.Item));
                        yield return Frames(12);
                    }
                    yield return Until(() => p.BonusRound != round || p.BonusAsking && p.BonusWordsDone > done, 1500);
                }
            }
            // the drum game, level 1: longer beats
            yield return Until(() => p.ToyWaitingForPlayer, 900); Mark("game");
            Vector3 Drum() => game.HomeRoom.BoundsOf(game.HomeRoom.Find(COgheToys.Drum)).center + Vector3.up * .015f;
            for (int beat = 0; beat < 3; beat++)
            {
                int n = beat;
                yield return Until(() => p.ToyWaitingForPlayer && p.ToyPattern == n && p.Gag == COgheGag.None, 900); yield return Frames(10);
                Mark("beat" + p.ToyBeats); int strikes = p.ToyBeats; for (int i = 0; i < strikes; i++) { yield return Touch(game, Drum()); yield return Frames(4); }
                yield return Until(() => !p.ToyListening, 300);
            }
            yield return Until(() => p.ToyJamming, 300); Mark("jam");
            for (int i = 0; i < 5; i++) { yield return Frames(14); yield return Touch(game, Drum()); }
            yield return Until(() => p.BonusThanking, 600); Mark("done"); yield return Frames(90);
            Mark("end");
            capturing = false; Time.captureFramerate = 0; COgheAudio.Heard -= heard;
            File.WriteAllText(Path.Combine(output, "sounds.txt"), sounds.ToString());
            File.WriteAllText(Path.Combine(output, "marks.txt"), marks.ToString());
            File.WriteAllText(Path.Combine(output, "frames.txt"), frame.ToString());
            Debug.Log("COGHE BONUS REEL DONE " + frame);
            Application.Quit(0);
        }

        /// <summary>The toy collection (-coghe-collection-reel): three toys won, two still to come; the Items menu, a tap on one
        /// still to come (where it is won), then on the horn (COghe plays it). Marks in marks.txt.</summary>
        private IEnumerator Collection(VenomCampaign game, Action<string, float> heard, System.Text.StringBuilder sounds)
        {
            var ui = game.ProductUI; var marks = new System.Text.StringBuilder();
            void Mark(string what) => marks.AppendLine($"{frame} {what}");
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheShop.TestsOwnUnlocked = false;
            game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
            int bosses = 0, reached = 0;
            for (int i = 0; i < ui.Catalog.Levels.Length; i++) { game.Progress.Completed.Add(ui.Catalog.Levels[i].Id); if (ui.Catalog.Levels[i].Boss && ++bosses == 4) { reached = i + 1; break; } }
            foreach (var e in COgheHomeItems.Catalog) if (e.level <= reached) COgheShop.Grant(e.id, "reel");
            foreach (var t in new[] { COgheToys.Drum, COgheToys.Horn, COgheToys.Balloon }) COgheShop.Grant(t, "reel");
            for (int c = 1; c <= 4; c++) COgheShop.Current.CardsPicked.Add(c);
            BuildRing(); ui.ShowMenu(); yield return new WaitForSecondsRealtime(.8f);
            ui.OpenHome(); var p = game.Personality;
            yield return Until(() => p.Act == COgheAct.Wave, 3000); yield return new WaitForSecondsRealtime(2.5f);
            COgheAudio.Heard += heard; Time.captureFramerate = 30; capturing = true;
            StartCoroutine(CaptureFrames());
            yield return Frames(30);
            Mark("items"); yield return Press(ui, "Items"); yield return Frames(90);
            Mark("missing"); yield return Press(ui, "Toy " + COgheToys.Top); yield return Frames(80);
            Mark("play"); yield return Press(ui, "Toy " + COgheToys.Horn);
            yield return Until(() => p.Playing != null, 400); yield return Until(() => p.Playing == null, 600); yield return Frames(40);
            Mark("end");
            capturing = false; Time.captureFramerate = 0; COgheAudio.Heard -= heard;
            File.WriteAllText(Path.Combine(output, "sounds.txt"), sounds.ToString());
            File.WriteAllText(Path.Combine(output, "marks.txt"), marks.ToString());
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
                // a reel that broke off (an exception in its script) must not fill the disk with frames
                if (frame >= MaxFrames) { Debug.LogError($"Bonus reel: over {MaxFrames} frames, stopping"); capturing = false; Application.Quit(1); yield break; }
                ScreenCapture.CaptureScreenshot(Path.Combine(output, $"frame_{frame:00000}.png"), superSize); frame++;
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
