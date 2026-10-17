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
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text == "Choose Speedy"));
                Assert.IsFalse(ui.GetComponentsInChildren<Text>().Any(t => t.text.Contains("%")), "Cards say what grows, not by how much");
                yield return Click("Choose ability");
                Assert.AreEqual(1, COgheAbilities.Level(COgheAbility.Agility));
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text == "Speedy · Lv 1"));
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
                Assert.AreEqual(3, ui.GetComponentsInChildren<Image>().Count(i => i.name.StartsWith("Emotion bar ") && i.color.r < .8f && i.color.g < .5f), "Emotion: three segments filled");
                yield return Click("Choose card");
                Assert.AreEqual(COgheProductPopup.AbilityPick, ui.Popup);
                yield return Click("Intelligence card"); yield return Click("Choose ability");
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text.StartsWith("Wait 5 seconds.")), "Intelligence explains itself once");
                yield return Click("Ability continue");
                Assert.AreEqual(1, COgheAbilities.Level(COgheAbility.Intelligence));
                Assert.IsFalse(ui.GetComponentsInChildren<Image>().Any(i => i.name == "Ability ready"), "Nothing waits any more");
            }
            finally { game.Progress.Completed.Clear(); game.Progress.Completed.AddRange(completed); game.Progress.HomeUnlocked = home; EndShop(); ui.ShowMenu(); }
        }
        [UnityTest] public IEnumerator AbilityCardsCanBeChangedBeforeConfirmAndMaxCannotBePicked()
        {
            try
            {
                FreshShop(false); COgheHomeRoom.UnlockedLevelOverride = 10; game.Progress.HomeUnlocked = true;
                COgheShop.Understand(COgheBonus.Key(1));
                ui.OpenHome(); yield return new WaitForSecondsRealtime(.3f);
                ui.ShowPopup(COgheProductPopup.AbilityStats); yield return Click("Choose card");
                int commands = game.Feedback.CommandCount;
                yield return Click("Emotion card"); yield return Click("Agility card"); yield return Click("Intelligence card");
                Assert.AreEqual(0, COgheShop.Current.CardsPicked.Count, "Previewing never spends a card");
                Assert.AreEqual(1, ui.GetComponentsInChildren<Image>().Count(i => i.name == "Selected check"));
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text == "Choose Clever"));
                yield return Click("Choose ability");
                Assert.AreEqual(0, COgheAbilities.Level(COgheAbility.Emotion));
                Assert.AreEqual(0, COgheAbilities.Level(COgheAbility.Agility));
                Assert.AreEqual(1, COgheAbilities.Level(COgheAbility.Intelligence));
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text == "Then tap it to use it."));
                yield return Click("Ability continue");
                Assert.AreEqual(commands, game.Feedback.CommandCount, "Card touches never command the creature");
                COgheShop.Current.Emotion = 10; COgheShop.Understand(COgheBonus.Key(2));
                ui.ShowPopup(COgheProductPopup.AbilityStats); yield return Click("Choose card");
                var full = ui.GetComponentsInChildren<Button>().Single(b => b.name == "Emotion card");
                Assert.IsFalse(full.interactable);
                yield return Click("Emotion card");
                Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b => b.name == "Choose ability"));
                yield return Click("Agility card"); yield return Click("Choose ability"); yield return Click("Ability continue");
                Assert.AreEqual(1, COgheAbilities.Level(COgheAbility.Agility));
                Assert.AreEqual(2, COgheShop.Current.CardsPicked.Count);
            }
            finally { EndShop(); ui.ShowMenu(); }
        }

        [UnityTest] public IEnumerator AbilityCardsFitPhonesAndTabletsWithoutOverlaps()
        {
            var report = new System.Text.StringBuilder(); int problems = 0;
            try
            {
                FreshShop(false); COgheHomeRoom.UnlockedLevelOverride = 10; game.Progress.HomeUnlocked = true;
                ui.OpenHome(); yield return new WaitForSecondsRealtime(.3f);
                foreach (var screen in new[] { new Vector2(640, 1136), new Vector2(720, 1280), new Vector2(1080, 2340), new Vector2(1536, 2048) })
                {
                    COgheProductUI.LayoutAreaForTests = new Rect(0, 0, screen.x, screen.y);
                    COgheShop.ResetForTests(new COgheShop.State { Migrated = true, Emotion = 3, Agility = 2 });
                    COgheShop.Understand(COgheBonus.Key(1));
                    ui.ShowPopup(COgheProductPopup.AbilityStats); ui.RelayoutForTests(); yield return null;
                    Audit("Ability stats", screen, report, ref problems);
                    Press("Choose card"); yield return null; Audit("Ability pick", screen, report, ref problems);
                    Assert.IsFalse(ui.GetComponentsInChildren<Image>().Any(x => x.name == "Selected check"), "No card is recommended or preselected");
                    var cards = new[] { "Emotion", "Agility", "Intelligence" }.Select(name =>
                        (RectTransform)ui.GetComponentsInChildren<Button>().Single(b => b.name == name + " card").transform).ToArray();
                    var initialBounds = cards.Select(WorldRect).ToArray();
                    for (int i = 0; i < cards.Length; i++)
                    {
                        Assert.GreaterOrEqual(cards[i].rect.width, 48, "The entire card is a generous touch target");
                        Assert.AreEqual(cards[0].rect.size, cards[i].rect.size, "All three choices have identical size");
                        Assert.AreEqual(initialBounds[0].y, initialBounds[i].y, .01f, "All cards share one horizontal row");
                        if (i > 0) Assert.Greater(initialBounds[i].xMin, initialBounds[i - 1].xMax, "Cards have a visible gap");
                    }
                    foreach (string name in new[] { "Emotion", "Agility", "Intelligence" })
                    {
                        Press(name + " card"); yield return null; Audit("Selected " + name, screen, report, ref problems);
                        var b = ui.GetComponentsInChildren<Button>().Single(x => x.name == name + " card");
                        Assert.GreaterOrEqual(((RectTransform)b.transform).rect.height, 48);
                        int index = System.Array.IndexOf(new[] { "Emotion", "Agility", "Intelligence" }, name);
                        Assert.AreEqual(initialBounds[index], WorldRect((RectTransform)b.transform), "Selection never enlarges or moves a card");
                        // Own button content is exempt in the shared audit: check its labels and illustration separately.
                        var labels = b.GetComponentsInChildren<Text>();
                        var picture = b.GetComponentsInChildren<Image>().Single(x => x.name == "COghe illustration");
                        foreach (var text in labels)
                        {
                            Assert.LessOrEqual(text.preferredHeight, text.rectTransform.rect.height + 1, text.text + " fits its box");
                            Assert.IsFalse(WorldRect(picture.rectTransform).Overlaps(WorldRect(text.rectTransform)), text.text + " never covers COghe");
                        }
                    }
                    Press("Choose ability"); yield return null; Audit("Learned Clever", screen, report, ref problems);
                    Press("Ability continue"); yield return null;
                    COgheShop.Current.Emotion = 10; COgheShop.Understand(COgheBonus.Key(2));
                    ui.ShowPopup(COgheProductPopup.AbilityStats); Press("Choose card"); yield return null;
                    Audit("Max Playful", screen, report, ref problems); ui.Resume();
                }
            }
            finally
            {
                COgheProductUI.LayoutAreaForTests = null; EndShop(); ui.ShowMenu(); ui.RelayoutForTests();
                System.IO.Directory.CreateDirectory("Artifacts/COgheAbilityCards");
                System.IO.File.WriteAllText("Artifacts/COgheAbilityCards/layout-audit.txt", report.ToString());
            }
            Assert.AreEqual(0, problems, report.ToString());
        }
    }
}
