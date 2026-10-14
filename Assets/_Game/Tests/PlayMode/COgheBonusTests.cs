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

        [UnityTest] public IEnumerator BossInterludeIsRequiredAndMenuCannotSkipIt()
        {
            var completed = game.Progress.Completed.ToList(); bool home = game.Progress.HomeUnlocked;
            try
            {
                FreshShop(false); game.Progress.HomeUnlocked = true;
                for(int i=0;i<10;i++)game.Progress.Completed.Add(ui.Catalog.Levels[i].Id);
                var boss = ui.Catalog.Levels[9];
                Assert.AreEqual(0, COgheBonus.After(ui.Catalog, 9));
                ui.ShowVictoryForTests(boss.Id, 10); yield return null;
                Assert.IsFalse(ui.VictoryStepForTests()); Assert.AreEqual(COgheProductPopup.Unlocks, ui.Popup);
                yield return Click("Continue");
                Assert.IsFalse(ui.VictoryStepForTests());
                Assert.AreEqual(COgheProductPage.Home, ui.Page); Assert.AreEqual(1, ui.BonusChapter);
                Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b=>b.name=="Skip bonus"));
                yield return Click("Bonus back"); Assert.AreEqual(COgheProductPage.MainMenu,ui.Page);
                yield return Click("Play"); Assert.AreEqual(1,ui.BonusChapter);
                Assert.AreEqual(1,game.Definition.Order,"No next scene loaded when leaving the interlude");
                Assert.IsFalse(COgheBonus.Done(1));
                CollectionAssert.AreEqual(ui.Catalog.Levels.Take(10).Select(l=>l.Id),game.Progress.Completed);
            }
            finally { ui.ShowMenu(); game.Progress.Completed.Clear();game.Progress.Completed.AddRange(completed);game.Progress.HomeUnlocked=home;EndShop(); }
        }

        [UnityTest] public IEnumerator TheBonusIsUnderstoodInHomeAndPaysOnce()
        {
            var completed = game.Progress.Completed.ToList(); bool home = game.Progress.HomeUnlocked;
            try
            {
                FreshShop(false); COgheHomeRoom.UnlockedLevelOverride = 10; game.Progress.HomeUnlocked = true;
                game.Progress.Completed.Add(ui.Catalog.Levels[9].Id);
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

        [UnityTest] public IEnumerator BonusResumesCompletedQuestionsEvenWithZeroDropRewards()
        {
            try
            {
                FreshShop(false); game.Progress.HomeUnlocked=true;
                game.Progress.Completed.Add(ui.Catalog.Levels[9].Id);
                COgheEconomy.Set("drops_bonus_round",0); COgheEconomy.Set("drops_bonus",0);
                ui.Play();yield return null;
                var p=game.Personality;
                yield return AwaitBonus(()=>p.BonusAsking,10,"First question");
                game.TouchPoint(ScreenOf(p.SkinCentre+Vector3.up*.04f));
                yield return AwaitBonus(()=>COgheBonus.CompletedRounds(1)==1,3,"Checkpoint independent of reward amount");
                yield return Click("Bonus back");
                // Serialize/reload precisely the same shop data used across application restarts.
                var saved=JsonUtility.ToJson(COgheShop.Current);
                COgheShop.ResetForTests(JsonUtility.FromJson<COgheShop.State>(saved));
                yield return Click("Play");
                Assert.AreEqual(1,game.Personality.BonusRound); Assert.AreEqual(0,COgheShop.Drops);
                Assert.IsFalse(COgheBonus.Done(1));
                // Saved final-question checkpoints with a zero reward must still reach the real thank-you and unlock.
                COgheShop.Understand(COgheBonus.RoundKey(1,2));COgheShop.Understand(COgheBonus.RoundKey(1,3));
                ui.ShowMenu();ui.Play();
                yield return AwaitBonus(()=>ui.Popup==COgheProductPopup.BonusDone,30,"Resume the thank-you after the final question");
                Assert.IsTrue(COgheBonus.Done(1));Assert.AreEqual(0,COgheShop.Drops);
                Assert.AreEqual(0,COgheBonus.Waiting(ui.Catalog,game.Progress));
            }
            finally {ui.ShowMenu();EndShop();}
        }
    }
}
