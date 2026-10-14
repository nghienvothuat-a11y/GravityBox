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
                yield return Card("INK_OCEAN");Assert.AreEqual(2,COgheShop.Current.InkGuideStep);
                yield return Hold(OnCOghe(),1.1f);yield return null;
                Assert.AreEqual(3,COgheShop.Current.InkGuideStep);
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

        [UnityTest] public IEnumerator OldRewardsRemainCompletedAndLaterBossesAlsoRequireUnderstanding()
        {
            FreshShop(false); game.Progress.HomeUnlocked=true;
            COgheShop.Earn(COgheBonus.Key(1),100,"existing_save");
            foreach(var level in ui.Catalog.Levels.Take(21))game.Progress.Completed.Add(level.Id);
            Assert.IsTrue(COgheBonus.Done(1));Assert.AreEqual(2,COgheBonus.Waiting(ui.Catalog,game.Progress));
            int count=game.Progress.Completed.Count;
            yield return Click("Play");Assert.AreEqual(2,ui.BonusChapter);
            Assert.AreEqual(count,game.Progress.Completed.Count,"Existing solved levels are not erased");
            yield return Click("Bonus back");EndShop();
        }
    }
}
