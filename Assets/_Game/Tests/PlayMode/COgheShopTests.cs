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
    // The shop (Mrk 02/10, PLANS/COGHE_MONETIZATION_PLAN.md): Drops, buying Home and Style items, trying on, what a win pays and
    // unlocks, the daily gift, Plus / No Ads, the paced full-screen ad and room for the banner.
    public partial class COgheProductUITests
    {
        /// <summary>An ad SDK stand-in that shows nothing and counts.</summary>
        private sealed class CountingAds : ICOgheAdProvider
        {
            public int Interstitials, Rewardeds; public bool Shown; public float Height = 120; public bool Earn = true;
            public bool InterstitialReady => true;
            public void ShowInterstitial(string placement, Action done) { Interstitials++; done(); }
            public bool RewardedReady => true;
            public void ShowRewarded(string placement, Action<bool> done) { Rewardeds++; done(Earn); }
            public void SetBanner(bool show) { Shown = show; }
            public float BannerHeight => Shown ? Height : 0;
        }
        private void FreshShop(bool testsOwnUnlocked)
        {
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheShop.TestsOwnUnlocked = testsOwnUnlocked;
            COgheAds.ResetForTests(); COgheEntitlements.ResetForTests(); COgheEconomy.ResetForTests();
        }
        private void EndShop()
        {
            COgheShop.ResetForTests(); COgheAds.ResetForTests(); COgheEntitlements.ResetForTests(); COgheEconomy.ResetForTests();
            COgheHomeRoom.UnlockedLevelOverride = null; COgheStyle.ResetForTests();
        }

        [UnityTest] public IEnumerator HomeItemsAppearOnlyOnceBought()
        {
            try
            {
                FreshShop(false); COgheHomeRoom.UnlockedLevelOverride = 20; game.Progress.HomeUnlocked = true;
                yield return Click("Home"); yield return new WaitForSecondsRealtime(.6f);
                var room = game.HomeRoom; var swing = room.Find("SWING"); var ball = room.Find("BALL");
                Assert.IsTrue(room.Present(ball), "The ball is a gift"); Assert.IsTrue(ball.Root.activeSelf);
                Assert.IsTrue(room.ForSale(swing), "The swing's level is reached: it is for sale"); Assert.IsFalse(swing.Root.activeSelf, "and not in the room yet");
                Assert.IsFalse(room.Available(room.Find("TV")), "The TV (level 23) is still locked");
                COgheShop.Earn(null, 200, "test"); int price = COgheEconomy.Price("SWING");
                yield return Click("Items"); Assert.AreEqual(COgheProductPopup.Collection, ui.Popup);
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.name == "State" && t.text == price.ToString()), "Its price on the card");
                yield return Click("Item SWING"); Assert.AreEqual(COgheProductPopup.Buy, ui.Popup);
                yield return Click("Buy item"); Assert.AreEqual(COgheProductPopup.None, ui.Popup);
                Assert.IsTrue(COgheShop.Owns("SWING")); Assert.AreEqual(200 - price, COgheShop.Drops);
                yield return new WaitForSecondsRealtime(1.5f);
                Assert.IsTrue(swing.Root.activeSelf, "Bought: it pops into the room"); Assert.AreEqual(COgheHomeItems.W, swing.Root.transform.localScale.x, .001f);
                Assert.IsFalse(COgheShop.Buy("SWING"), "Bought once");
            }
            finally { EndShop(); }
        }

        [UnityTest] public IEnumerator StyleTryOnIsKeptOnlyWhenBought()
        {
            try
            {
                FreshShop(false);
                yield return EnterStyle(new COgheStyle(), 30);
                Assert.IsTrue(COgheShop.Owns("INK_OCEAN") && COgheShop.Owns("HAT_BEANIE") && COgheShop.Owns("FLOAT_STARS"), "First of each group: gifts");
                Assert.IsFalse(COgheShop.Owns("INK_MINT"));
                // try a hat on without Drops, then take it off on the way out
                yield return Click("Accessories"); yield return Card("HAT_STRAW");
                Assert.AreEqual("HAT_STRAW", COgheStyle.Current.Hat, "Tried on COghe");
                Assert.IsTrue(ui.GetComponentsInChildren<Button>().Any(b => b.name == "Buy tried"), "with its Buy button");
                yield return Click("Done"); Assert.AreEqual(COgheProductPopup.StyleTryOn, ui.Popup, "Leaving asks: keep it or take it off");
                yield return Click("Take it off");
                Assert.AreEqual(COgheProductPage.Home, ui.Page); Assert.AreEqual("", COgheStyle.Current.Hat, "Not bought: back to the look before");
                // now with Drops: buy it while trying it on, and it stays
                COgheShop.Earn(null, 300, "test"); int price = COgheEconomy.Price("HAT_STRAW");
                yield return Click("Style"); yield return new WaitForSecondsRealtime(.8f);
                yield return Click("Accessories"); yield return Card("HAT_STRAW");
                yield return Click("Buy tried"); Assert.AreEqual(COgheProductPopup.Buy, ui.Popup);
                yield return Click("Buy item"); Assert.IsTrue(COgheShop.Owns("HAT_STRAW")); Assert.AreEqual(300 - price, COgheShop.Drops);
                yield return Click("Done"); Assert.AreEqual(COgheProductPage.Home, ui.Page, "Owned: no question on the way out");
                Assert.AreEqual("HAT_STRAW", COgheStyle.Current.Hat);
            }
            finally { EndShop(); }
        }

        [UnityTest] public IEnumerator AWinPaysOnceAndShowsWhatItUnlocked()
        {
            try
            {
                FreshShop(false); yield return null;
                var level10 = ui.Catalog.Levels[9];
                ui.ShowVictoryForTests(level10.Id, 10); yield return null;
                Assert.AreEqual(COgheEconomy.FirstWin, ui.VictoryDrops); Assert.AreEqual(COgheEconomy.FirstWin, COgheShop.Drops);
                CollectionAssert.IsSubsetOf(new[] { "BALL", "INK_OCEAN", "INK_MINT" }, ui.VictoryUnlocks.ToArray(), "Level 10: the ball, Ocean (gifts) and Mint");
                ui.VictoryStepForTests(); yield return null;
                Assert.AreEqual(COgheProductPopup.Unlocks, ui.Popup, "What it unlocked, before the next level");
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text == "Gift ✓"), "gifts marked");
                Assert.IsTrue(ui.GetComponentsInChildren<Button>().Any(b => b.name == "Buy INK_MINT"), "the rest for sale");
                yield return Click("Continue"); Assert.AreEqual(COgheProductPopup.None, ui.Popup); Assert.IsEmpty(ui.VictoryUnlocks);
                ui.ShowVictoryForTests(level10.Id, 10); yield return null;
                Assert.AreEqual(0, ui.VictoryDrops, "A level pays once"); Assert.AreEqual(COgheEconomy.FirstWin, COgheShop.Drops); Assert.IsEmpty(ui.VictoryUnlocks);
            }
            finally { EndShop(); ui.ShowMenu(); }
        }

        [UnityTest] public IEnumerator FullScreenAdsArePacedAndPlusRemovesThem()
        {
            try
            {
                FreshShop(true); var ads = new CountingAds(); COgheAds.Provider = ads;
                COgheEconomy.Set("interstitial_min_seconds", 0); COgheEconomy.Set("rewarded_quiet_seconds", 0);
                COgheAds.NoteWin(); COgheAds.NoteWin();
                Assert.IsFalse(COgheAds.TryInterstitial("level_end", 5, null), "Not before the 6th win");
                Assert.IsTrue(COgheAds.TryInterstitial("level_end", 6, null)); Assert.AreEqual(1, ads.Interstitials);
                COgheAds.NoteWin(); Assert.IsFalse(COgheAds.TryInterstitial("level_end", 7, null), "Two wins apart");
                COgheAds.NoteWin(); Assert.IsTrue(COgheAds.TryInterstitial("level_end", 8, null));
                for (int i = 0; i < 10; i++) { COgheAds.NoteWin(); COgheAds.NoteWin(); COgheAds.TryInterstitial("level_end", 10 + i * 2, null); }
                Assert.AreEqual(COgheEconomy.InterstitialSessionCap, ads.Interstitials, "Capped per session");
                COgheAds.ResetForTests(); COgheAds.Provider = ads; ads.Interstitials = 0;
                COgheEconomy.Set("interstitial_min_seconds", 90);
                COgheAds.NoteWin(); COgheAds.NoteWin(); Assert.IsTrue(COgheAds.TryInterstitial("level_end", 6, null));
                COgheAds.NoteWin(); COgheAds.NoteWin(); Assert.IsFalse(COgheAds.TryInterstitial("level_end", 8, null), "90 s apart");
                // Plus: no full-screen ads, no banner, rewards without an ad, Drops ×2
                COgheEntitlements.Store = new COgheTestStore(); bool bought = false;
                COgheEntitlements.Buy(COgheEntitlements.Plus, ok => bought = ok);
                Assert.IsTrue(bought && COgheEntitlements.HasPlus && COgheEntitlements.AdsRemoved);
                COgheEconomy.Set("interstitial_min_seconds", 0); COgheAds.NoteWin(); COgheAds.NoteWin();
                Assert.IsFalse(COgheAds.InterstitialDue(30));
                COgheAds.Banner(true); Assert.AreEqual(0, COgheAds.BannerHeight);
                int before = ads.Rewardeds; bool rewarded = false; COgheAds.Rewarded("test", ok => rewarded = ok);
                Assert.IsTrue(rewarded); Assert.AreEqual(before, ads.Rewardeds, "Plus: rewarded without watching");
                Assert.AreEqual(2, COgheEntitlements.DropsMultiplier);
                yield return null;
            }
            finally { EndShop(); }
        }

        [UnityTest] public IEnumerator TheBannerKeepsClearOfMenuHomeAndVictoryButtons()
        {
            try
            {
                FreshShop(true); var ads = new CountingAds(); COgheAds.Provider = ads;
                ui.ShowMenu(); yield return null; yield return null;
                Assert.IsTrue(ads.Shown, "Banner on the menu");
                float Bottom(string name)
                {
                    Canvas.ForceUpdateCanvases(); var rect = (RectTransform)ui.GetComponentsInChildren<Button>().First(b => b.name == name).transform;
                    var corners = new Vector3[4]; rect.GetWorldCorners(corners); return corners[0].y;
                }
                Assert.Greater(Bottom("Home"), ads.Height, "Menu buttons above the banner");
                game.Progress.HomeUnlocked = true; yield return Click("Home"); yield return null;
                Assert.IsTrue(ads.Shown); Assert.Greater(Bottom("Feed"), ads.Height, "Home buttons above the banner");
                yield return Click("Style"); yield return null;
                Assert.IsFalse(ads.Shown, "No banner in Style");
                yield return Click("Done"); yield return Click("Back"); yield return Click("Play"); yield return null;
                Assert.AreEqual(COgheProductPage.Game, ui.Page); Assert.IsFalse(ads.Shown, "No banner while playing");
            }
            finally { EndShop(); }
        }

        [UnityTest] public IEnumerator EarlierPlayersKeepWhatTheyEarnedAndTheGiftComesDaily()
        {
            var completed = game.Progress.Completed.ToList();
            try
            {
                COgheShop.ResetForTests(new COgheShop.State()); COgheShop.TestsOwnUnlocked = false;
                game.Progress.Completed.Clear(); for (int i = 0; i < 20; i++) game.Progress.Completed.Add(ui.Catalog.Levels[i].Id);
                COgheShop.Migrate(game);
                Assert.IsTrue(COgheShop.Owns("SWING") && COgheShop.Owns("SLIDE") && COgheShop.Owns("INK_GOLD") && COgheShop.Owns("HAT_FLOWER"), "Everything their levels gave them");
                Assert.IsFalse(COgheShop.Owns("TV"), "nothing beyond");
                COgheShop.TodayForTests = "2026-10-02";
                Assert.AreEqual(COgheEconomy.DailyGift, COgheShop.ClaimGift(1)); Assert.AreEqual(0, COgheShop.ClaimGift(1), "Once a day");
                COgheShop.TodayForTests = "2026-10-03"; Assert.AreEqual(COgheEconomy.DailyGift * 2, COgheShop.ClaimGift(2), "the next day, doubled by an ad");
                yield return null;
            }
            finally { game.Progress.Completed.Clear(); game.Progress.Completed.AddRange(completed); EndShop(); }
        }
    }
}
