using System;
using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    // The bonus "Hiểu ra" (Mrk 07/10/2026): offered after a chapter's boss, skippable, played in Home in body shapes.
    public partial class COgheProductUITests
    {
        private static IEnumerator AwaitBonus(Func<bool> done, float seconds, string what)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (!done() && Time.realtimeSinceStartup < end) yield return null;
            Assert.IsTrue(done(), what);
        }
        private Vector2 ScreenOf(Vector3 world) => game.Owner.View.WorldToScreenPoint(world);

        [UnityTest] public IEnumerator ABossWinOffersTheBonusAndSkipKeepsItForHome()
        {
            var completed = game.Progress.Completed.ToList(); bool home = game.Progress.HomeUnlocked;
            try
            {
                FreshShop(false); game.Progress.HomeUnlocked = true;
                var boss = ui.Catalog.Levels[11];
                Assert.IsTrue(boss.Boss, "Level 12 closes chapter 1");
                Assert.AreEqual(0, COgheBonus.After(ui.Catalog, 11), "No bonus after an ordinary level");
                ui.ShowVictoryForTests(boss.Id, 12); yield return null;
                Assert.AreEqual(1, ui.VictoryBonus, "Chapter 1's bonus is offered");
                Assert.IsFalse(ui.VictoryStepForTests()); Assert.AreEqual(COgheProductPopup.Unlocks, ui.Popup, "What the win unlocked comes first");
                yield return Click("Continue");
                Assert.IsFalse(ui.VictoryStepForTests()); Assert.AreEqual(COgheProductPopup.BonusOffer, ui.Popup, "then the bonus");
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text == "+" + COgheBonus.Reward + " Drops"), "with its reward");
                yield return Click("Skip bonus");
                Assert.AreEqual(COgheProductPopup.None, ui.Popup); Assert.AreEqual(0, ui.VictoryBonus);
                Assert.IsTrue(ui.VictoryStepForTests(), "Skip goes straight on to the next chapter");
                Assert.IsFalse(COgheBonus.Done(1), "Skipping pays nothing");
                game.Progress.Completed.Add(boss.Id);
                Assert.AreEqual(1, COgheBonus.Waiting(ui.Catalog, game.Progress), "The skipped bonus waits in Home");
            }
            finally { game.Progress.Completed.Clear(); game.Progress.Completed.AddRange(completed); game.Progress.HomeUnlocked = home; EndShop(); ui.ShowMenu(); }
        }

        [UnityTest] public IEnumerator TheBonusIsUnderstoodInHomeAndPaysOnce()
        {
            var completed = game.Progress.Completed.ToList(); bool home = game.Progress.HomeUnlocked;
            try
            {
                FreshShop(false); COgheHomeRoom.UnlockedLevelOverride = 12; game.Progress.HomeUnlocked = true;
                game.Progress.Completed.Add(ui.Catalog.Levels[11].Id);
                yield return Click("Home"); yield return new WaitForSecondsRealtime(.6f);
                Assert.AreEqual(COgheProductPage.Home, ui.Page);
                yield return Click("Bonus");
                var p = game.Personality; Assert.AreEqual(1, ui.BonusChapter); Assert.IsTrue(p.InBonus);
                int start = COgheShop.Drops;
                yield return AwaitBonus(() => p.BonusAsking, 10, "COghe walks to the middle of the room and asks");
                Assert.AreEqual(COgheShape.Heart, p.BonusAsk.Value.Shape);

                // a tap on the floor is not an answer, and does not send it walking
                game.TouchPoint(ScreenOf(game.HomeRoom.Root.TransformPoint(new Vector3(.2f, 0, -.4f)))); yield return null;
                Assert.IsNull(game.Motion.Get(0), "Floor taps do not move it during the bonus"); Assert.IsTrue(p.BonusAsking);

                // a wrong answer: a gentle no (and no food thrown), then the same question again
                yield return Click("Feed");
                Assert.AreEqual(1, p.BonusMisses); Assert.AreEqual(0, p.BonusRound); Assert.IsFalse(game.HomeFeedBalls.Active, "A wrong Feed throws nothing");
                yield return AwaitBonus(() => p.BonusAsking, 3, "It asks again");
                yield return Click("Feed");
                yield return AwaitBonus(() => p.BonusAsking, 3, "and again");
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text == "It wants a cuddle: tap COghe."), "Two misses: a hint");

                // the heart it holds up: a touch on it
                game.TouchPoint(ScreenOf(p.SkinCentre + Vector3.up * .04f));
                yield return AwaitBonus(() => COgheShop.Drops == start + COgheBonus.RoundReward(1), 1.5f, "A right answer pays at once (Mrk 07/10)");
                yield return AwaitBonus(() => p.BonusRound == 1 && p.BonusAsking, 10, "A cuddle was right; the next question");
                Assert.AreEqual(COgheShape.Ball, p.BonusAsk.Value.Shape);

                // a bouncing ball: the ball
                var ball = game.HomeRoom.Find("BALL");
                game.TouchPoint(ScreenOf(game.HomeRoom.BoundsOf(ball).center));
                yield return AwaitBonus(() => p.Playing == ball, 10, "It runs off to play ball");
                yield return AwaitBonus(() => p.BonusRound == 2 && p.BonusAsking, 25, "then asks the last question");
                Assert.AreEqual(COgheShape.Mushroom, p.BonusAsk.Value.Shape);

                // a mushroom: food
                int eaten = p.BallsEaten;
                yield return Click("Feed");
                yield return AwaitBonus(() => p.BallsEaten > eaten, 8, "It catches the food and eats it");
                yield return AwaitBonus(() => ui.Popup == COgheProductPopup.BonusDone, 30, "It thanks the player; the reward");
                int rounds = COgheBonus.RoundReward(1) + COgheBonus.RoundReward(2) + COgheBonus.RoundReward(3);
                Assert.Greater(COgheBonus.Reward, COgheBonus.RoundReward(3), "The whole bonus pays the most");
                Assert.AreEqual(start + rounds + COgheBonus.Reward, COgheShop.Drops, "Every round, then the prize"); Assert.IsTrue(COgheBonus.Done(1));
                yield return Click("Bonus continue");
                Assert.AreEqual(COgheProductPage.Home, ui.Page); Assert.AreEqual(0, ui.BonusChapter); Assert.IsFalse(p.InBonus);
                Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b => b.name == "Bonus"), "Done: no bonus waits in Home");
                Assert.AreEqual(0, COgheShop.Earn(COgheBonus.Key(1), COgheBonus.Reward, "bonus"), "It pays once");
            }
            finally { game.Progress.Completed.Clear(); game.Progress.Completed.AddRange(completed); game.Progress.HomeUnlocked = home; EndShop(); ui.ShowMenu(); }
        }

        [UnityTest] public IEnumerator PlayingTheBonusFromTheVictoryAndLeavingGoesOnToTheNextChapter()
        {
            bool home = game.Progress.HomeUnlocked;
            try
            {
                FreshShop(false); COgheHomeRoom.UnlockedLevelOverride = 12; game.Progress.HomeUnlocked = true;
                ui.ShowVictoryForTests(ui.Catalog.Levels[11].Id, 12); yield return null;
                ui.VictoryStepForTests(); yield return Click("Continue");
                ui.VictoryStepForTests(); Assert.AreEqual(COgheProductPopup.BonusOffer, ui.Popup);
                yield return Click("Play bonus");
                Assert.AreEqual(COgheProductPage.Home, ui.Page); Assert.AreEqual(1, ui.BonusChapter); Assert.IsTrue(game.Personality.InBonus);
                yield return new WaitForSecondsRealtime(.5f);
                yield return Click("Bonus later");
                float end = Time.realtimeSinceStartup + 8;
                while (game != null && game.Definition.Order != 13 && Time.realtimeSinceStartup < end) { yield return null; game = Object.FindFirstObjectByType<VenomCampaign>(); }
                yield return null; game = Object.FindFirstObjectByType<VenomCampaign>(); ui = game.ProductUI;
                Assert.AreEqual(13, game.Definition.Order, "Later: on to chapter 2");
                Assert.IsFalse(COgheBonus.Done(1), "and the bonus still waits");
            }
            finally { if (game != null) game.Progress.HomeUnlocked = home; EndShop(); }
        }
    }
}
