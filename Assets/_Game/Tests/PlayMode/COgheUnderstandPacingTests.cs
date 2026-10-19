using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GravityBox.Tests
{
    public sealed class COgheUnderstandPacingTests
    {
        private bool persistence;
        private COgheProductCatalog catalog;
        [SetUp] public void Before()
        {
            persistence = VenomCampaignSave.PersistenceEnabled; VenomCampaignSave.PersistenceEnabled = false;
            COgheShop.ResetForTests(new COgheShop.State()); COgheEconomy.ResetForTests(); COgheEntitlements.ResetForTests();
            catalog = Resources.Load<COgheProductCatalog>("COgheUI/Catalog");
        }
        [TearDown] public void After()
        { COgheShop.ResetForTests(); COgheEconomy.ResetForTests(); COgheEntitlements.ResetForTests(); VenomCampaignSave.PersistenceEnabled = persistence; }

        [Test] public void TwelveMilestonesGiveTwelveDistinctCardsAndNeverPayAgain()
        {
            var milestones = COgheBonus.Milestones(catalog).ToArray();
            CollectionAssert.AreEqual(new[] { 6, 10, 16, 21, 27, 32, 38, 43, 49, 54, 60, 65 }, milestones.Select(m => m.order));
            var save = new VenomCampaignSave(); COgheShop.MigrateUnderstand(catalog, save);
            int cards = 0, expectedDrops = 0;
            for (int order = 1; order <= catalog.Levels.Length; order++)
            {
                Assert.AreEqual(0, COgheBonus.Waiting(catalog, save));
                save.Win(catalog.Levels[order - 1]);
                int id = COgheBonus.After(catalog, order);
                if (id == 0) continue;
                Assert.AreEqual(id, COgheBonus.Waiting(catalog, save));
                for (int round = 1; round <= COgheBonus.Rounds(id).Length; round++)
                {
                    expectedDrops += COgheShop.Earn(COgheBonus.RoundKey(id, round), COgheBonus.RoundReward(round), "test");
                    COgheShop.Understand(COgheBonus.RoundKey(id, round));
                }
                expectedDrops += COgheShop.Earn(COgheBonus.Key(id), COgheBonus.CompletionReward(id), "test");
                COgheShop.Understand(COgheBonus.Key(id));
                Assert.AreEqual(id, COgheAbilities.Pending());
                Assert.IsTrue(COgheShop.PickAbility(id, (COgheAbility)(cards++ % 3)));
                Assert.IsFalse(COgheShop.PickAbility(id, COgheAbility.Emotion));
                Assert.AreEqual(0, COgheBonus.After(catalog, order));
                Assert.AreEqual(0, COgheShop.Earn(COgheBonus.Key(id), COgheBonus.CompletionReward(id), "test"));
            }
            Assert.AreEqual(12, cards); Assert.AreEqual(expectedDrops, COgheShop.Drops);
            Assert.AreEqual(4, COgheAbilities.Level(COgheAbility.Emotion));
            Assert.AreEqual(4, COgheAbilities.Level(COgheAbility.Agility));
            Assert.AreEqual(4, COgheAbilities.Level(COgheAbility.Intelligence));
            Assert.AreEqual(0, COgheAbilities.Pending());
        }

        [Test] public void LessonsUseOwnedWordsAndIntroduceOneSentenceRuleAtATime()
        {
            foreach (var m in COgheBonus.Milestones(catalog))
            {
                var rounds = COgheBonus.Rounds(m.id);
                Assert.AreEqual(COgheBonus.IsPractice(m.id) ? 1 : 3, rounds.Length);
                foreach (var r in rounds)
                {
                    if (r.IsGame) { Assert.AreEqual(0, r.Level); continue; }
                    Assert.That(r.Words.Length, Is.InRange(1, 3));
                    Assert.LessOrEqual(r.Words.Count(w => w.Count > 1), 1);
                    foreach (var w in r.Words)
                    {
                        Assert.That(w.Count, Is.InRange(1, 2));
                        if (w.Answer == COgheBonusAnswer.Item && w.Item != "BALL")
                            Assert.Less(COgheToys.Catalog.Single(t => t.id == w.Item).chapter, COgheBonus.Chapter(m.id));
                    }
                }
            }
            // First contact has no reading, inventory or counting prerequisite.
            var first = COgheBonus.Rounds(COgheBonus.PracticeId(1))[0].Words.Single();
            Assert.AreEqual(COgheBonusAnswer.Touch, first.Answer); Assert.AreEqual(1, first.Count);
            Assert.IsFalse(COgheBonus.Rounds(COgheBonus.PracticeId(2))[0].Ordered);
            Assert.IsTrue(COgheBonus.Rounds(COgheBonus.PracticeId(3))[0].Ordered);
            Assert.AreEqual(2, COgheBonus.Rounds(COgheBonus.PracticeId(4))[0].Words.Single().Count);
            Assert.IsNull(COgheBonus.Rounds(1007), "Unauthored future practices must not gate play");
        }

        [Test] public void MigrationSkipsPastNewLessonsButPreservesBossProgressCardsAndToys()
        {
            var save = new VenomCampaignSave();
            foreach (var l in catalog.Levels.Take(35)) save.Win(l);
            COgheShop.Earn("bonus:1", 100, "legacy"); COgheShop.PickAbility(1, COgheAbility.Agility);
            COgheShop.Understand("bonus:2:1"); COgheShop.Grant(COgheToys.Drum, "legacy");
            string before = JsonUtility.ToJson(save);
            COgheShop.MigrateUnderstand(catalog, save);
            CollectionAssert.AreEqual(new[] { 1001, 1002, 1003 }, COgheShop.Current.SkippedPractices);
            Assert.AreEqual(2, COgheBonus.Waiting(catalog, save)); Assert.AreEqual(1, COgheBonus.CompletedRounds(2));
            Assert.AreEqual(0, COgheAbilities.Pending()); Assert.AreEqual(100, COgheShop.Drops);
            Assert.IsTrue(COgheToys.Owns(COgheToys.Drum)); Assert.AreEqual(1.1f, COgheAbilities.SpeedScale, .0001f);
            Assert.AreEqual(before, JsonUtility.ToJson(save));
            COgheShop.ResetForTests(JsonUtility.FromJson<COgheShop.State>(JsonUtility.ToJson(COgheShop.Current)));
            foreach (var l in catalog.Levels.Take(38)) save.Win(l);
            COgheShop.MigrateUnderstand(catalog, save);
            Assert.AreEqual(1004, COgheBonus.After(catalog, 38), "New milestones after migration still appear");
            Assert.IsFalse(COgheBonus.Done(1001)); Assert.IsFalse(COgheAbilities.Picked(1001));
        }

        [Test] public void FreshSaveGetsLevelSixAndEarnedCardSurvivesReloadWithoutRepeatingLesson()
        {
            var save = new VenomCampaignSave(); COgheShop.MigrateUnderstand(catalog, save);
            foreach (var l in catalog.Levels.Take(6)) save.Win(l);
            COgheShop.MigrateUnderstand(catalog, save);
            Assert.AreEqual(1001, COgheBonus.Waiting(catalog, save)); Assert.IsFalse(save.HomeUnlocked);
            COgheShop.Understand(COgheBonus.Key(1001));
            COgheShop.ResetForTests(JsonUtility.FromJson<COgheShop.State>(JsonUtility.ToJson(COgheShop.Current)));
            Assert.AreEqual(0, COgheBonus.Waiting(catalog, save)); Assert.AreEqual(1001, COgheAbilities.Pending());
            Assert.IsTrue(COgheShop.PickAbility(1001, COgheAbility.Intelligence));
            Assert.IsFalse(COgheShop.PickAbility(1001, COgheAbility.Agility));
            Assert.IsFalse(save.HomeUnlocked); Assert.IsEmpty(COgheShop.Current.SkippedPractices);
        }
    }

    public partial class COgheProductUITests
    {
        [UnityTest, Timeout(180000)] public IEnumerator FirstPracticeBeforeHomeUnlockRecoversFromMissesAndResumesItsCard()
        {
            try
            {
                FreshShop(false); game.Progress.HomeUnlocked = false; game.Progress.Completed.Clear();
                COgheShop.MigrateUnderstand(ui.Catalog, game.Progress);
                foreach (var l in ui.Catalog.Levels.Take(6)) game.Progress.Win(l);
                ui.ShowVictoryForTests(ui.Catalog.Levels[5].Id, 6); yield return null;
                Assert.AreEqual(1001, ui.VictoryBonus);
                ui.NextLevel(); yield return null;
                if (ui.Popup == COgheProductPopup.Unlocks) yield return Click("Continue");
                yield return AwaitBonus(() => ui.BonusChapter == 1001, 10, "Level 6 victory leads to the practice");
                Assert.AreEqual(1001, ui.BonusChapter); Assert.IsFalse(game.Progress.HomeUnlocked);
                Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(b => b.name == "Items" || b.name == "Style"));
                var p = game.Personality;
                yield return AwaitBonus(() => p.BonusAsking, 12, "The early room can ask without unlocking Home");
                int initialDrops = COgheShop.Drops;
                yield return Click("Pause"); yield return new WaitForSecondsRealtime(.3f);
                Assert.AreEqual(0, p.BonusWordsDone); Assert.IsFalse(COgheBonus.Done(1001));
                yield return Click("Resume");
                for (int i = 0; i < 2; i++) { yield return Click("Feed"); yield return AwaitBonus(() => p.BonusAsking, 8, "A miss gently retries"); }
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text == "It wants a cuddle: tap COghe."));
                yield return PlayUnderstand(p, "First practice");
                yield return AwaitBonus(() => ui.Popup == COgheProductPopup.BonusDone, 30, "The one-question practice completes");
                int drops = COgheShop.Drops;
                Assert.AreEqual(initialDrops + COgheBonus.RoundReward(1) + COgheBonus.CompletionReward(1001), drops);
                yield return Click("Bonus continue"); Assert.AreEqual(1001, ui.AbilityChapter);
                ui.ShowMenu();
                COgheShop.ResetForTests(JsonUtility.FromJson<COgheShop.State>(JsonUtility.ToJson(COgheShop.Current)));
                yield return Click("Play");
                Assert.AreEqual(1001, ui.AbilityChapter); Assert.AreEqual(0, ui.BonusChapter);
                Assert.AreEqual(drops, COgheShop.Drops);
                yield return Click("Intelligence card"); yield return Click("Choose ability");
                Assert.AreEqual(1, COgheAbilities.StepsRemembered); Assert.IsFalse(game.Progress.HomeUnlocked);
                Assert.IsFalse(COgheShop.Current.CommerceVisited); Assert.AreEqual(0, COgheShop.Current.InkGuideStep);
                yield return Click("Ability continue");
                yield return AwaitBonus(() => Object.FindFirstObjectByType<VenomCampaign>()?.Definition.Order == 7, 20, "The reward continues to level 7");
            }
            finally { if (ui != null) ui.ShowMenu(); EndShop(); }
        }

        [UnityTest, Explicit, Timeout(3600000)] public IEnumerator AllTwelveUnderstandLessonsPlayThroughWithTheirCards()
        {
            for (int chapter = 1; chapter <= 6; chapter++)
            {
                yield return UnderstandPractice(chapter);
                yield return UnderstandChapter(chapter);
            }
        }
        private IEnumerator UnderstandPractice(int chapter)
        {
            try
            {
                FreshShop(false); game.Progress.Completed.Clear(); game.Progress.HomeUnlocked = chapter > 1;
                int id = COgheBonus.PracticeId(chapter);
                int order = COgheBonus.Milestones(ui.Catalog).Single(m => m.id == id).order;
                foreach (var l in ui.Catalog.Levels.Take(order)) game.Progress.Completed.Add(l.Id);
                foreach (var m in COgheBonus.Milestones(ui.Catalog).Where(m => m.order < order))
                { COgheShop.Understand(COgheBonus.Key(m.id)); COgheShop.Current.CardsPicked.Add(m.id); }
                COgheToys.GrantEarned();
                ui.StartBonus(id, false); yield return null;
                Assert.AreEqual(id, ui.BonusChapter);
                float started = Time.realtimeSinceStartup;
                yield return PlayUnderstand(game.Personality, "Practice " + chapter);
                yield return AwaitBonus(() => ui.Popup == COgheProductPopup.BonusDone, 40, "Practice thank-you");
                Debug.Log($"UNDERSTAND practice {chapter} after {order}: {Time.realtimeSinceStartup - started:F1}s");
                Assert.IsTrue(COgheBonus.Done(id)); Assert.AreEqual(COgheBonus.RoundReward(1) + COgheBonus.CompletionReward(id), COgheShop.Drops);
                yield return Click("Bonus continue"); Assert.AreEqual(id, ui.AbilityChapter);
                yield return Click("Emotion card"); yield return Click("Choose ability"); yield return Click("Ability continue");
                Assert.AreEqual(0, COgheAbilities.Pending()); Assert.AreEqual(0, COgheBonus.Waiting(ui.Catalog, game.Progress));
                Assert.AreEqual(chapter > 1, game.Progress.HomeUnlocked);
            }
            finally { ui.ShowMenu(); game.Progress.Completed.Clear(); game.Progress.HomeUnlocked = false; EndShop(); }
        }
    }

    public sealed partial class COgheSpatialCampaignTests
    {
        [UnityTest] public IEnumerator PracticeKeepsCorrectWordsWhenACountOrLaterAnswerIsWrong()
        {
            try
            {
                yield return StartBonusInHome(COgheBonus.PracticeId(6));
                var p = game.Personality;
                p.PlayWith(game.HomeRoom.Find("BALL"));
                yield return BonusUntil(() => p.BonusAsking && p.BonusWordsDone == 1, 30, "Ball complete, now two meals");
                p.BonusFeed(); p.BonusFeed(); p.BonusFeed();
                yield return BonusUntil(() => p.BonusAsking, 12, "Overcount retries only the meal");
                Assert.AreEqual(1, p.BonusWordsDone); Assert.AreEqual(0, p.BonusCounted);
                Assert.AreEqual(COgheBonusAnswer.Feed, p.BonusAsk.Value.Answer);
                p.BonusFeed(); p.BonusFeed();
                yield return BonusUntil(() => p.BonusAsking && p.BonusWordsDone == 2, 12, "Two meals done, now the cuddle");
                p.BonusFeed();
                yield return BonusUntil(() => p.BonusAsking, 8, "Wrong last action keeps both earlier wishes");
                Assert.AreEqual(2, p.BonusWordsDone); Assert.AreEqual(COgheBonusAnswer.Touch, p.BonusAsk.Value.Answer);
                yield return Frames(300);
                Assert.IsTrue(p.BonusAsking, "Taking time to think never fails the question");
                Assert.AreEqual(2, p.BonusWordsDone);
                p.TouchedInHome(p.SkinCentre);
                yield return BonusUntil(() => p.BonusFinished, 20, "Recovered without restarting the lesson");
            }
            finally { game.Personality?.EndBonus(); COgheHomeRoom.UnlockedLevelOverride = null; COgheShop.ResetForTests(); }
        }
    }
}
