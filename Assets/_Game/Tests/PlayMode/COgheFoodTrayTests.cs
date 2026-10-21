using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    // The Food tray (Mrk 21/10/2026): Home's Feed opens it; the steel balls stay free, a snack is paid in Drops and eaten at
    // once; short of Drops, the ways to get more.
    public partial class COgheProductUITests
    {
        [UnityTest] public IEnumerator FeedOpensTheFoodTrayBallsStayFreeAndSnacksCostDrops()
        {
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true, Drops = 25 });
            game.Progress.HomeUnlocked = true;   // fixture: UI unlock state, not a claim that Boss 10 was solved
            yield return Click("Home"); Assert.AreEqual(COgheProductPage.Home, ui.Page);
            yield return new WaitForSecondsRealtime(3f);   // the furniture pops in

            yield return Click("Feed"); Assert.AreEqual(COgheProductPopup.Food, ui.Popup, "Feed opens the Food tray");
            yield return Click(COgheProductUI.FeedBallsButton);
            Assert.AreEqual(COgheProductPopup.None, ui.Popup); Assert.IsTrue(game.HomeFeeding, "Steel balls, free as before");
            Assert.AreEqual(25, COgheShop.Drops);
            float end = Time.realtimeSinceStartup + 40;
            while (game.HomeFeeding && Time.realtimeSinceStartup < end) yield return null;
            Assert.IsFalse(game.HomeFeeding, "The balls are eaten");

            var pie = COgheSnacks.Find("SNACK_APPLE_PIE"); var cake = COgheSnacks.Find("SNACK_STRAWBERRY_CAKE");
            yield return Click("Feed"); yield return Click(COgheProductUI.SnackButton(pie.Id));
            Assert.AreEqual(COgheProductPopup.None, ui.Popup); Assert.AreEqual(25 - pie.Price, COgheShop.Drops, "Paid once");
            Assert.AreEqual(pie, game.Personality.Snack, "Eaten at once");
            end = Time.realtimeSinceStartup + 10;
            while (game.Personality.Snack != null && Time.realtimeSinceStartup < end) yield return null;
            Assert.IsNull(game.Personality.Snack); Assert.AreEqual(1, game.Personality.SnacksEaten);

            yield return Click("Feed"); yield return Click(COgheProductUI.SnackButton(cake.Id));   // 30 with 15 left
            Assert.AreEqual(COgheProductPopup.Buy, ui.Popup, "Short of Drops: the ways to get more");
            Assert.AreEqual(25 - pie.Price, COgheShop.Drops, "Nothing paid"); Assert.IsNull(game.Personality.Snack);
            yield return Click("Close"); Assert.AreEqual(COgheProductPopup.None, ui.Popup);
        }

        // COghe's energy (Mrk 21/10/2026): each win with Home open uses 10; hungry under 50, it walks at 85% (70% under 25)
        // and the win offers "Hungry · Feed", which ends at Home on the Food tray; balls fill to 60 only, a snack beyond.
        [UnityTest] public IEnumerator WinsUseEnergyAHungryCOgheWalksSlowerAndTheWinLeadsToItsFood()
        {
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true, Drops = 100, InkGuideStep = 4 });
            Assert.AreEqual(100, COgheEnergy.Value, "An old save starts full");
            // a plain level: no boss, no bonus after it, nothing unlocked by its first win
            int order = 0;
            for (int o = 11; o < ui.Catalog.Levels.Length && order == 0; o++)
            {
                var level = ui.Catalog.Levels[o - 1]; bool unlocks = false;
                foreach (var item in COgheEconomy.Items) unlocks |= item.Level == o;
                if (level != null && !level.Boss && !ui.Catalog.Levels[o - 2].Boss && !unlocks) order = o;
            }
            Assert.Greater(order, 0);
            ui.ShowVictoryForTests(ui.Catalog.Levels[order - 1].Id, order);
            Assert.AreEqual(100, COgheEnergy.Value, "Nothing is used before Home opens: COghe cannot be fed yet");

            game.Progress.HomeUnlocked = true;   // fixture: UI unlock state, not a claim that Boss 10 was solved
            for (int i = 0; i < 5; i++) ui.ShowVictoryForTests(ui.Catalog.Levels[order - 1].Id, order);
            Assert.AreEqual(50, COgheEnergy.Value, "10 a win"); Assert.AreEqual(COgheHunger.Fed, COgheEnergy.State);
            Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b => b.name == COgheProductUI.FeedAfterVictoryButton), "Fed: no prompt");
            game.ResetLevel(); Assert.AreEqual(1f, game.Motion.SpeedScale, .0001f, "Fed: the usual pace");
            Assert.AreEqual(0f, game.Motion.RestWalk, "Fed: no stops for breath");

            ui.ShowVictoryForTests(ui.Catalog.Levels[order - 1].Id, order);
            Assert.AreEqual(40, COgheEnergy.Value); Assert.AreEqual(COgheHunger.Hungry, COgheEnergy.State);
            game.ResetLevel(); Assert.AreEqual(1f, game.Motion.SpeedScale, .0001f, "Hungry: the usual pace between stops");
            Assert.Greater(game.Personality.Droop, 0f, "and droops");
            Assert.AreEqual(COgheEnergy.HungryWalk, game.Motion.RestWalk, "Hungry: stops for breath");
            Assert.AreEqual(COgheEnergy.HungryPause, game.Motion.RestPause);
            COgheShop.Current.Agility = 2; game.ResetLevel();
            Assert.AreEqual(1.2f, game.Motion.SpeedScale, .0001f, "Agility counts as usual");
            COgheShop.Current.Agility = 0;

            yield return Click(COgheProductUI.FeedAfterVictoryButton);
            float end = Time.realtimeSinceStartup + 10;
            while (ui.Page != COgheProductPage.Home && Time.realtimeSinceStartup < end) yield return null;
            Assert.AreEqual(COgheProductPage.Home, ui.Page, "The win's steps, then Home");
            Assert.AreEqual(COgheProductPopup.Food, ui.Popup, "on the Food tray");
            yield return new WaitForSecondsRealtime(3f);   // the furniture pops in

            // balls: +4 each, three a serving, never past 60
            int drops = COgheShop.Drops;
            yield return Click(COgheProductUI.FeedBallsButton);
            end = Time.realtimeSinceStartup + 40;
            while (game.HomeFeeding && Time.realtimeSinceStartup < end) yield return null;
            Assert.AreEqual(40 + 3 * COgheEnergy.PerBall, COgheEnergy.Value, "Three balls");
            Assert.AreEqual(drops, COgheShop.Drops, "Free");
            yield return Click("Feed"); yield return Click(COgheProductUI.FeedBallsButton);
            end = Time.realtimeSinceStartup + 40;
            while (game.HomeFeeding && Time.realtimeSinceStartup < end) yield return null;
            Assert.AreEqual(COgheEnergy.BallCap, COgheEnergy.Value, "Balls stop at 60");

            // a snack goes beyond
            var cake = COgheSnacks.Find("SNACK_STRAWBERRY_CAKE");
            yield return Click("Feed"); yield return Click(COgheProductUI.SnackButton(cake.Id));
            end = Time.realtimeSinceStartup + 10;
            while (game.Personality.Snack != null && Time.realtimeSinceStartup < end) yield return null;
            Assert.AreEqual(COgheEnergy.Max, COgheEnergy.Value, "60 + 75, full at 100");
            game.ResetLevel(); Assert.AreEqual(1f, game.Motion.SpeedScale, .0001f, "Fed again: the usual pace");
            Assert.AreEqual(0f, game.Personality.Droop, "Fed: no droop");
            Assert.AreEqual(0f, game.Motion.RestWalk, "Fed again: no stops");

            COgheShop.SetEnergyUsed(80); game.ResetLevel();
            Assert.AreEqual(COgheHunger.Exhausted, COgheEnergy.State); Assert.AreEqual(1f, game.Motion.SpeedScale, .0001f, "Exhausted: still the same walk");
            Assert.Greater(game.Personality.Droop, COghePersonality.HungryDroop + COghePersonality.HungryHeave, "Exhausted: droops more");
            Assert.AreEqual(COgheEnergy.ExhaustedWalk, game.Motion.RestWalk, "Exhausted: stops more often");
            Assert.AreEqual(COgheEnergy.ExhaustedPause, game.Motion.RestPause, "and longer");
        }
    }
}
