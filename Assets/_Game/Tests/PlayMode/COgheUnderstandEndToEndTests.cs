using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GravityBox.Tests
{
    // Understand COghe played through as a player would (Mrk 18/10/2026: finish the task): every question answered right,
    // the toy game played, the thank-you and its prize, the ability card, and the toy in the Items collection.
    public partial class COgheProductUITests
    {
        // Chapter 2 in the regular suite: the first toy game, end to end. Every chapter on request.
        [UnityTest, Timeout(300000)] public IEnumerator ChapterTwoIsUnderstoodEndToEnd() { yield return UnderstandChapter(2); }
        [UnityTest, Explicit, Timeout(3600000)] public IEnumerator EveryChapterIsUnderstoodEndToEnd()
        {
            for (int c = 1; c <= 6; c++) yield return UnderstandChapter(c);
            for (int c = 7; c <= 9; c++) yield return UnderstandGenerated(c);
        }

        private IEnumerator UnderstandChapter(int c)
        {
            var completed = game.Progress.Completed.ToList(); bool home = game.Progress.HomeUnlocked;
            try
            {
                FreshShop(false); game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
                // the bosses up to chapter c are beaten; the chapters before are understood, their cards taken, their toys won
                for (int i = 0, bosses = 0; i < ui.Catalog.Levels.Length && bosses < c; i++)
                { game.Progress.Completed.Add(ui.Catalog.Levels[i].Id); if (ui.Catalog.Levels[i].Boss) bosses++; }
                for (int k = 1; k < c; k++) { COgheShop.Understand(COgheBonus.Key(k)); COgheShop.Current.CardsPicked.Add(k); if (COgheToys.OfChapter(k) != null) COgheToys.Give(COgheToys.OfChapter(k)); }
                for (int k = 1; k <= c; k++) { int practice = COgheBonus.PracticeId(k); COgheShop.Understand(COgheBonus.Key(practice)); COgheShop.Current.CardsPicked.Add(practice); }
                Assert.AreEqual(c, COgheBonus.Waiting(ui.Catalog, game.Progress), "Chapter " + c + " waits");
                yield return Click("Home"); yield return new WaitForSecondsRealtime(.6f);
                yield return Click("Bonus");
                var p = game.Personality; Assert.AreEqual(c, ui.BonusChapter); Assert.IsTrue(p.InBonus);
                int start = COgheShop.Drops;
                yield return PlayUnderstand(p, "Chapter " + c);
                yield return AwaitBonus(() => ui.Popup == COgheProductPopup.BonusDone, 40, "Chapter " + c + ": the thank-you and the prize");
                Assert.IsTrue(COgheBonus.Done(c));
                Assert.AreEqual(start + COgheBonus.RoundReward(1) + COgheBonus.RoundReward(2) + COgheBonus.RoundReward(3) + COgheBonus.Reward, COgheShop.Drops, "Chapter " + c + ": every question, then the prize");
                string toy = COgheToys.OfChapter(c);
                if (toy != null) Assert.IsTrue(COgheToys.Owns(toy), "Chapter " + c + " gives " + toy);
                yield return Click("Bonus continue");
                Assert.AreEqual(COgheProductPopup.AbilityPick, ui.Popup, "Chapter " + c + " ends with the card choice"); Assert.AreEqual(c, ui.AbilityChapter);
                yield return Click("Emotion card"); yield return Click("Choose ability"); yield return Click("Ability continue");
                Assert.AreEqual(COgheProductPopup.None, ui.Popup); Assert.AreEqual(0, COgheAbilities.Pending(), "One card per chapter");
                Assert.IsFalse(p.InBonus); Assert.AreEqual(0, COgheBonus.Waiting(ui.Catalog, game.Progress), "Nothing waits");
                int owned = COgheToys.Catalog.Count(t => COgheToys.Owns(t.id));
                Assert.AreEqual(Mathf.Clamp(c - 1, 0, COgheToys.Catalog.Length), owned, "Chapter " + c + ": a toy for every chapter from 2");
                yield return Click("Items");
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text == "Toys " + owned + "/" + COgheToys.Catalog.Length), "The collection counts it");
                if (toy != null)
                {
                    yield return Click("Toy " + toy);
                    var item = game.HomeRoom.Find(toy);
                    yield return AwaitBonus(() => p.Playing == item, 10, "Chapter " + c + ": its toy plays from the collection");
                }
            }
            finally { game.Personality?.EndBonus(); game.Progress.Completed.Clear(); game.Progress.Completed.AddRange(completed); game.Progress.HomeUnlocked = home; EndShop(); ui.ShowMenu(); }
        }

        // Chapters from 7 come after bosses the 65-level catalog does not have yet: played straight in Home.
        private IEnumerator UnderstandGenerated(int c)
        {
            var completed = game.Progress.Completed.ToList(); bool home = game.Progress.HomeUnlocked;
            try
            {
                // a player this far has beaten all 65 levels (the gift ball is out) and understood chapters 1–6
                FreshShop(false); game.Progress.HomeUnlocked = true; game.Progress.Completed.Clear();
                foreach (var level in ui.Catalog.Levels) game.Progress.Completed.Add(level.Id);
                for (int k = 1; k <= 6; k++) { COgheShop.Understand(COgheBonus.Key(k)); COgheShop.Current.CardsPicked.Add(k); }
                foreach (var t in COgheToys.Catalog) COgheToys.Give(t.id);
                yield return Click("Home"); yield return new WaitForSecondsRealtime(.6f);
                var ball = game.HomeRoom.Find("BALL"); Assert.IsTrue(ball != null && ball.Root.activeInHierarchy, "The gift ball is in the room");
                var p = game.Personality; p.BeginBonus(COgheBonus.Rounds(c));
                yield return PlayUnderstand(p, "Chapter " + c);
                yield return AwaitBonus(() => p.BonusFinished, 30, "Chapter " + c + ": the thank-you");
            }
            finally { game.Personality?.EndBonus(); game.Progress.Completed.Clear(); game.Progress.Completed.AddRange(completed); game.Progress.HomeUnlocked = home; EndShop(); ui.ShowMenu(); }
        }

        private static string BonusState(COghePersonality p) =>
            $"round={p.BonusRound} words={p.BonusWordsDone} counted={p.BonusCounted} ask={p.BonusAsk?.Answer}/{p.BonusAsk?.Item}x{p.BonusAsk?.Count} " +
            $"asking={p.BonusAsking} busy={p.BonusBusy} toy={p.ToyId} listening={p.ToyListening} jam={p.ToyJamming} gag={p.Gag} misses={p.BonusMisses}";

        // Answers each word as a player would (a touch, Feed, a tap on the item), as many times as COghe shows, and plays the
        // toy game; every answer must be taken as right.
        private IEnumerator PlayUnderstand(COghePersonality p, string what)
        {
            float end = Time.realtimeSinceStartup + 300;
            while (!p.BonusThanking && !p.BonusFinished)
            {
                Assert.Less(Time.realtimeSinceStartup, end, what + " is stuck: " + BonusState(p));
                if (p.InToyGame) { yield return PlayToyGame(p, what); continue; }
                if (!p.BonusAsking || p.BonusAsk == null) { yield return null; continue; }
                var ask = p.BonusAsk.Value;
                if (ask.Count > 1 && p.BonusCounted >= ask.Count) { yield return null; continue; }   // enough: COghe checks the count
                int round = p.BonusRound, words = p.BonusWordsDone, counted = p.BonusCounted, misses = p.BonusMisses;
                if (ask.Answer == COgheBonusAnswer.Feed) yield return Click("Feed");
                else
                {
                    Vector3 at = ask.Answer == COgheBonusAnswer.Touch ? p.SkinCentre + Vector3.up * .04f : game.HomeRoom.BoundsOf(game.HomeRoom.Find(ask.Item)).center;
                    game.TouchPoint(ScreenOf(at)); yield return null;
                }
                yield return AwaitBonus(() => p.BonusRound != round || p.BonusWordsDone != words || p.BonusCounted != counted || p.BonusMisses != misses
                    || p.BonusThanking || p.BonusFinished, 25, what + ": the answer lands (" + ask.Answer + " " + ask.Item + ")");
                Assert.AreEqual(misses, p.BonusMisses, what + ": a right answer is never a miss; " + BonusState(p));
            }
        }

        private IEnumerator PlayToyGame(COghePersonality p, string what)
        {
            string id = p.ToyId; int misses = p.BonusMisses; float end = Time.realtimeSinceStartup + 180;
            while (p.InToyGame && !p.ToyJamming && !p.BonusThanking && !p.BonusFinished)
            {
                Assert.Less(Time.realtimeSinceStartup, end, what + ": the " + id + " game is stuck: " + BonusState(p));
                if (!p.ToyWaitingForPlayer || p.Gag != COgheGag.None || id == COgheToys.Top && !p.ToyWobbling) { yield return null; continue; }
                if (id == COgheToys.Horn) foreach (int key in p.ToyTune) { game.TouchPoint(ScreenOf(p.ToyItem.Part("Key" + key).position)); for (int k = 0; k < 8; k++) yield return null; }
                else for (int i = 0, n = id == COgheToys.Top ? 1 : p.ToyBeats; i < n; i++) { game.TouchPoint(ScreenOf(p.ToyTapPoint)); for (int k = 0; k < 10; k++) yield return null; }
                yield return AwaitBonus(() => !p.ToyListening || p.Gag != COgheGag.None, 8, what + ": the " + id + " is played back");
                Assert.AreEqual(misses, p.BonusMisses, what + ": played right, the " + id + " game has no miss; " + BonusState(p));
            }
            if (p.ToyJamming) { game.TouchPoint(ScreenOf(p.ToyTapPoint)); yield return null; }
            yield return AwaitBonus(() => !p.InToyGame || p.BonusThanking || p.BonusFinished, 30, what + ": the " + id + " game is won");
            if (COgheToys.Is(id)) Assert.IsTrue(COgheToys.Owns(id), what + ": the " + id + " is the player's");
        }
    }
}
