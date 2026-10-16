using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GravityBox.Tests
{
    // Ability cards (Mrk 16/10/2026): the card choice after Understand COghe, and Home's Ability button with the three bars.
    public partial class COgheProductUITests
    {
        [UnityTest] public IEnumerator UnderstandingCOgheEndsWithOneAbilityCard()
        {
            var completed = game.Progress.Completed.ToList(); bool home = game.Progress.HomeUnlocked;
            try
            {
                FreshShop(false); COgheHomeRoom.UnlockedLevelOverride = 10; game.Progress.HomeUnlocked = true;
                game.Progress.Completed.Add(ui.Catalog.Levels[9].Id);
                COgheShop.Understand(COgheBonus.Key(1));   // chapter 1's Understand COghe, just finished
                yield return Click("Home"); yield return new WaitForSecondsRealtime(.6f);
                ui.ShowPopup(COgheProductPopup.BonusDone); yield return null;
                yield return Click("Bonus continue");
                Assert.AreEqual(COgheProductPopup.AbilityPick, ui.Popup, "The bonus ends with the card choice");
                Assert.AreEqual(1, ui.AbilityChapter);
                Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b => b.name == "Choose ability"), "Nothing chosen until a card is tapped");
                yield return Click("Agility card");
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text == "Choose Agility"));
                Assert.IsFalse(ui.GetComponentsInChildren<Text>().Any(t => t.text.Contains("%")), "Cards say what grows, not by how much");
                yield return Click("Choose ability");
                Assert.AreEqual(1, COgheAbilities.Level(COgheAbility.Agility));
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text == "Agility Lv 1!"));
                yield return Click("Ability continue");
                Assert.AreEqual(COgheProductPopup.None, ui.Popup);
                Assert.AreEqual(0, COgheAbilities.Pending(), "One card per Understand COghe");
            }
            finally { game.Progress.Completed.Clear(); game.Progress.Completed.AddRange(completed); game.Progress.HomeUnlocked = home; EndShop(); ui.ShowMenu(); }
        }

        [UnityTest] public IEnumerator HomeAbilityShowsBarsAndAWaitingCard()
        {
            var completed = game.Progress.Completed.ToList(); bool home = game.Progress.HomeUnlocked;
            try
            {
                FreshShop(false); COgheHomeRoom.UnlockedLevelOverride = 10; game.Progress.HomeUnlocked = true;
                game.Progress.Completed.Add(ui.Catalog.Levels[9].Id);
                COgheShop.Current.Emotion = 3; COgheShop.Current.Agility = 2;
                COgheShop.Understand(COgheBonus.Key(1));   // a card left waiting (the player left during the choice)
                yield return Click("Home"); yield return new WaitForSecondsRealtime(.6f);
                Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b => b.name == "Play"), "Ability replaces Home's Play");
                Assert.IsTrue(ui.GetComponentsInChildren<Image>().Any(i => i.name == "Ability ready"), "A red dot: a card waits");
                yield return Click("Ability");
                Assert.AreEqual(COgheProductPopup.AbilityStats, ui.Popup);
                var texts = ui.GetComponentsInChildren<Text>().Select(t => t.text).ToList();
                CollectionAssert.Contains(texts, "Lv 3/10"); CollectionAssert.Contains(texts, "Lv 2/10"); CollectionAssert.Contains(texts, "Lv 0/10");
                Assert.AreEqual(3, ui.GetComponentsInChildren<Image>().Count(i => i.name.StartsWith("Emotion bar ") && i.color.r > .7f && i.color.g < .5f), "Emotion: three segments filled");
                yield return Click("Choose card");
                Assert.AreEqual(COgheProductPopup.AbilityPick, ui.Popup);
                yield return Click("Intelligence card"); yield return Click("Choose ability");
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text.StartsWith("Leave COghe alone for 5 seconds")), "Intelligence explains itself once");
                yield return Click("Ability continue");
                Assert.AreEqual(1, COgheAbilities.Level(COgheAbility.Intelligence));
                Assert.IsFalse(ui.GetComponentsInChildren<Image>().Any(i => i.name == "Ability ready"), "Nothing waits any more");
            }
            finally { game.Progress.Completed.Clear(); game.Progress.Completed.AddRange(completed); game.Progress.HomeUnlocked = home; EndShop(); ui.ShowMenu(); }
        }
    }
}
