using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GravityBox.Tests
{
    public partial class COgheProductUITests
    {
        [UnityTest] public IEnumerator ReorderedCatalogKeepsOldInkUnlockAndDoesNotGrantPuzzleWins()
        {
            try
            {
                FreshShop(false);
                foreach (var level in ui.Catalog.Levels.Take(7)) game.Progress.Completed.Add(level.Id);
                // content 07 (display 8 before 15/10) left the order on 19/10: its stable ID still counts
                game.Progress.Completed.Add("coghe.spatial.pilot.07");
                COgheShop.Current.InkGuideStep = 2;
                int wins = game.Progress.Completed.Count, drops = COgheShop.Drops;
                COgheShop.Migrate(game);
                Assert.IsTrue(ui.StyleUnlocked, "Old display level 8 still unlocks Style after reordering");
                Assert.IsFalse(game.Progress.Completed.Contains(ui.Catalog.Levels[7].Id), "Migration never solves the new display level 8");
                Assert.AreEqual(wins, game.Progress.Completed.Count); Assert.AreEqual(drops, COgheShop.Drops);
                ui.OpenStyleFromMenu(); yield return null;
                Assert.AreEqual(COgheProductPage.Style, ui.Page); Assert.AreEqual(2, COgheShop.Current.InkGuideStep);
                var saved = JsonUtility.ToJson(COgheShop.Current);
                COgheShop.ResetForTests(JsonUtility.FromJson<COgheShop.State>(saved));
                COgheShop.Migrate(game);
                // 1–8 from before 15/10, and 25, where 07 stood before 19/10 (the old catalog already counted it reached)
                Assert.AreEqual(9, COgheShop.Current.ReachedBeforePacing.Count, "Migration is idempotent");
                Assert.IsFalse(COgheHomeRoom.Reached(game, 10), "No future unlocks are granted");
            }
            finally { if (ui != null) ui.ShowMenu(); EndShop(); }
        }

        [UnityTest] public IEnumerator InkGuideFitsPhoneAndTabletWithoutOverlappingControls()
        {
            var report=new System.Text.StringBuilder();int problems=0;
            try
            {
                FreshShop(false);COgheHomeRoom.UnlockedLevelOverride=8;COgheStyle.ResetForTests(new COgheStyle());
                foreach(var screen in AuditScreens)
                {
                    COgheProductUI.LayoutAreaForTests=new Rect(0,0,screen.x,screen.y);
                    foreach(int step in new[]{1,2,3})
                    {
                        ui.ShowMenu();COgheShop.SetInkGuideStep(step);ui.OpenStyleFromMenu();ui.RelayoutForTests();yield return null;
                        Audit("Ink guide "+step,screen,report,ref problems);
                    }
                }
            }
            finally {COgheProductUI.LayoutAreaForTests=null;ui.ShowMenu();ui.RelayoutForTests();EndShop();}
            Assert.AreEqual(0,problems,report.ToString());
        }

        [UnityTest] public IEnumerator LevelEightOpensFreeInkLessonWithoutOpeningHome()
        {
            try
            {
                FreshShop(false); COgheStyle.ResetForTests(new COgheStyle());
                COgheShop.Understand(COgheBonus.Key(1001)); COgheShop.PickAbility(1001,COgheAbility.Emotion); // prior mandatory practice
                for(int i=0;i<7;i++)game.Progress.Completed.Add(ui.Catalog.Levels[i].Id);
                Assert.IsFalse(ui.StyleUnlocked);
                game.Progress.Completed.Add(ui.Catalog.Levels[7].Id);
                Assert.IsTrue(ui.StyleUnlocked); Assert.IsFalse(game.Progress.HomeUnlocked);
                ui.ShowVictoryForTests(ui.Catalog.Levels[7].Id,8);yield return null;
                CollectionAssert.IsSubsetOf(new[]{"INK_OCEAN","INK_MINT"},ui.VictoryUnlocks.ToArray());
                // Victory remains until Next is chosen; starting this step does not depend on a purchase or ad.
                yield return new WaitForSecondsRealtime(.3f);Assert.AreEqual(COgheProductPage.Victory,ui.Page);
                ui.NextLevel();yield return null;yield return Click("Continue");yield return null;
                Assert.AreEqual(COgheProductPage.Style,ui.Page);Assert.IsFalse(game.Progress.HomeUnlocked);
                Assert.AreEqual(1,COgheShop.Current.InkGuideStep);Assert.IsTrue(COgheShop.Owns("INK_OCEAN"));
                yield return new WaitForSecondsRealtime(1.5f);
                Assert.IsFalse(ui.BeginStyleHold(OnCOghe(),false),"The first step teaches choosing a syringe");
                AssertInkArrowAt("Icon", "Card INK_OCEAN");
                yield return Card("INK_OCEAN");Assert.AreEqual(2,COgheShop.Current.InkGuideStep);
                yield return new WaitForSecondsRealtime(.5f);
                AssertInkArrowScreen(OnCOghe());
                yield return Hold(OnCOghe(),1.1f);yield return null;
                Assert.AreEqual(3,COgheShop.Current.InkGuideStep);
                yield return new WaitForSecondsRealtime(.5f);
                AssertInkArrowAt("Done action");
                Assert.IsTrue(COgheStyle.Current.Amount.Any(a=>a.x>.01f),"Actual tissue received the free ink");
                Assert.AreEqual(COgheEconomy.FirstWin,COgheShop.Drops,"No Drops spent during the lesson");
                Assert.AreEqual(1,ui.StyleUndoCount);
                yield return Click("Undo");Assert.IsTrue(COgheStyle.Current.Amount.All(a=>a.x<.001f));
                yield return Hold(OnCOghe(),1);yield return null;
                yield return Click("Back");Assert.AreEqual(COgheProductPage.MainMenu,ui.Page);
                var save=JsonUtility.ToJson(COgheShop.Current);
                COgheShop.ResetForTests(JsonUtility.FromJson<COgheShop.State>(save));
                yield return Click("Play");Assert.AreEqual(COgheProductPage.Style,ui.Page);
                Assert.AreEqual(3,COgheShop.Current.InkGuideStep,"Return to the saved final step");
                yield return Click("Done");
                Assert.AreEqual(4,COgheShop.Current.InkGuideStep);
                yield return AwaitBonus(()=>Object.FindFirstObjectByType<VenomCampaign>().Definition.Order==9,12,"Continue to display level 9");
                game=Object.FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
                Assert.IsFalse(game.Progress.HomeUnlocked);
            }
            finally {if(ui!=null)ui.ShowMenu();EndShop();}
        }

        private void AssertInkArrowAt(string name, string parent = null)
        {
            var target = ui.GetComponentsInChildren<RectTransform>().Single(r => r.name == name && (parent == null || r.parent.name == parent));
            AssertInkArrowScreen(RectTransformUtility.WorldToScreenPoint(null, target.TransformPoint(target.rect.center)));
        }
        private void AssertInkArrowScreen(Vector2 screen)
        {
            var arrow = ui.GetComponentsInChildren<COgheGuide>().Single(g => g.name == "Ink guide arrow");
            var r = (RectTransform)arrow.transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(r, screen, null, out var p);
            Assert.IsTrue(arrow.ArrowShown); Assert.IsTrue(arrow.TargetOnScreen);
            Assert.Less(Vector2.Distance(p - r.rect.min, arrow.TargetPoint), 8, "Guide follows the actual tappable target");
            Assert.IsFalse(arrow.GetComponent<CanvasGroup>().blocksRaycasts);
            Assert.IsTrue(arrow.GetComponentsInChildren<Graphic>().All(g => !g.raycastTarget));
        }

        [UnityTest] public IEnumerator RewardFxRemainDecorativeExpireWhilePausedAndDoNotReplayOnRelayout()
        {
            try
            {
                FreshShop(false); COgheHomeRoom.UnlockedLevelOverride = 8;
                ui.ShowVictoryForTests(ui.Catalog.Levels[7].Id, 8); yield return null;
                int paid = COgheShop.Drops;
                Assert.IsTrue(ui.GetComponentsInChildren<COgheRewardFx>().Any(f => f.name == "Drop shine"));
                ui.NextLevel(); yield return null;
                Assert.AreEqual(COgheProductPopup.Unlocks, ui.Popup);
                Assert.AreEqual(1, ui.GetComponentsInChildren<COgheRewardFx>().Count(f => f.name == "Unlock fireworks"));
                ui.RelayoutForTests(); yield return null;
                Assert.AreEqual(1, ui.GetComponentsInChildren<COgheRewardFx>().Count(f => f.name == "Unlock fireworks"));
                Assert.IsTrue(ui.GetComponentsInChildren<COgheRewardFx>().All(f => !f.raycastTarget));
                Time.timeScale = 0;
                yield return new WaitForSecondsRealtime(COgheRewardFx.FireworkLife + .2f);
                Assert.IsEmpty(ui.GetComponentsInChildren<COgheRewardFx>());
                ui.RelayoutForTests(); yield return null;
                Assert.IsEmpty(ui.GetComponentsInChildren<COgheRewardFx>(), "Layout changes do not restart rewards");
                Assert.AreEqual(paid, COgheShop.Drops, "VFX never pay or duplicate rewards");
                Assert.AreEqual(COgheProductPage.Victory, ui.Page, "Effects never advance the puzzle");
            }
            finally { Time.timeScale = 1; if (ui != null) ui.ShowMenu(); EndShop(); }
        }

        [UnityTest] public IEnumerator OldRewardsRemainCompletedAndLaterBossesAlsoRequireUnderstanding()
        {
            FreshShop(false); game.Progress.HomeUnlocked=true;
            COgheShop.Earn(COgheBonus.Key(1),100,"existing_save");
            foreach(var level in ui.Catalog.Levels.Take(21))game.Progress.Completed.Add(level.Id);
            COgheShop.MigrateUnderstand(ui.Catalog,game.Progress); // pre-curriculum save: do not force past practices
            COgheShop.PickAbility(1,COgheAbility.Emotion); // its earlier earned card has been claimed
            Assert.IsTrue(COgheBonus.Done(1));Assert.AreEqual(2,COgheBonus.Waiting(ui.Catalog,game.Progress));
            int count=game.Progress.Completed.Count;
            yield return Click("Play");Assert.AreEqual(2,ui.BonusChapter);
            Assert.AreEqual(count,game.Progress.Completed.Count,"Existing solved levels are not erased");
            yield return Click("Bonus back");EndShop();
        }
    }
}
