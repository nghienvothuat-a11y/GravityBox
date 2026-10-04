using System;
using System.Collections.Generic;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed class COgheMonetizationTests
    {
        private bool persistence;
        private sealed class DeferredAds : ICOgheAdProvider
        {
            public Action Shown, Earned; public Action<bool> Closed;
            public bool InterstitialReady => true;
            public bool RewardedReady => true;
            public float BannerHeight => 0;
            public void SetBanner(bool show) { }
            public void ShowInterstitial(string placement, Action shown, Action<bool> closed) { Shown = shown; Closed = closed; }
            public void ShowRewarded(string placement, Action shown, Action earned, Action<bool> closed) { Shown = shown; Earned = earned; Closed = closed; }
        }
        [SetUp] public void Before()
        {
            persistence = VenomCampaignSave.PersistenceEnabled; VenomCampaignSave.PersistenceEnabled = false;
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheShop.TestsOwnUnlocked = false;
            COgheEntitlements.ResetForTests(); COgheAds.ResetForTests(); COgheEconomy.ResetForTests();
            COgheAnalytics.Recent.Clear();
        }
        [TearDown] public void After()
        {
            COgheShop.ResetForTests(); COgheEntitlements.ResetForTests(); COgheAds.ResetForTests(); COgheEconomy.ResetForTests();
            VenomCampaignSave.PersistenceEnabled = persistence;
        }
        [Test] public void NoAdsKeepsOptionalRewardsButPlusNeverRequestsThem()
        {
            COgheEntitlements.Grant(COgheEntitlements.NoAds);
            Assert.IsFalse(COgheAds.ForcedAdsAllowed); Assert.IsTrue(COgheAds.RewardAdsAllowed);
            var ads = new DeferredAds(); COgheAds.Provider = ads;
            bool done = false; COgheAds.Rewarded("drops", COgheReward.Shop(), ok => done = ok);
            ads.Shown(); ads.Earned(); ads.Closed(true);
            Assert.IsTrue(done); Assert.AreEqual(15, COgheShop.Drops);
            COgheEntitlements.Grant(COgheEntitlements.Plus);
            Assert.IsFalse(COgheAds.RewardAdsAllowed);
            ads.Earned = null; COgheAds.Rewarded("drops", COgheReward.Shop(), ok => done = ok);
            Assert.IsNull(ads.Earned); Assert.AreEqual(30, COgheShop.Drops);
        }
        [Test] public void EarnedRewardIsSavedBeforeCloseAndSurvivesReloadWithoutDoubleCredit()
        {
            const string key = "coghe.shop.v1"; bool existed = PlayerPrefs.HasKey(key); string backup = PlayerPrefs.GetString(key);
            try
            {
                VenomCampaignSave.PersistenceEnabled = true;
                var ads = new DeferredAds(); COgheAds.Provider = ads;
                var reward = COgheReward.Shop(); int closes = 0;
                COgheAds.Rewarded("drops", reward, ok => closes++); ads.Shown(); ads.Earned();
                Assert.AreEqual(0, closes); Assert.IsTrue(COgheAds.Showing);
                COgheShop.ResetForTests(); // Drop all in-memory wallet state while the native ad remains open.
                Assert.AreEqual(15, COgheShop.Drops); Assert.IsTrue(COgheShop.Paid(reward.Key));
                Assert.AreEqual(1, COgheShop.Current.AdsForDropsToday);
                ads.Earned(); ads.Closed(true); ads.Closed(true);
                Assert.AreEqual(15, COgheShop.Drops); Assert.AreEqual(1, closes);
                Assert.IsFalse(COgheShop.ApplyReward(reward));
            }
            finally { if (existed) PlayerPrefs.SetString(key, backup); else PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); }
        }
        [Test] public void CloseEarlyOrFailNeverAwardsOrConsumesDailyRewardCap()
        {
            var ads = new DeferredAds(); COgheAds.Provider = ads;
            bool paid = true; COgheAds.Rewarded("drops", COgheReward.Shop(), ok => paid = ok);
            ads.Shown(); ads.Closed(true);
            Assert.IsFalse(paid); Assert.AreEqual(0, COgheShop.Drops); Assert.AreEqual(0, COgheShop.Current.AdsForDropsToday);
            COgheAds.Rewarded("daily_gift", COgheReward.Daily(), ok => paid = ok); ads.Closed(false);
            Assert.IsTrue(COgheShop.GiftReady); Assert.IsFalse(paid);
        }
        [Test] public void FailedInterstitialDoesNotSpendCapsOrReportComplete()
        {
            var ads = new DeferredAds(); COgheAds.Provider = ads; COgheAds.NoteWin(); COgheAds.NoteWin();
            Assert.IsTrue(COgheAds.TryInterstitial("level_end", 6, null)); ads.Closed(false);
            Assert.AreEqual(0, COgheShop.InterstitialsToday); Assert.IsTrue(COgheAds.InterstitialDue(6));
            Assert.IsFalse(COgheAnalytics.Recent.Any(x => x.StartsWith("ad_complete ")));
            Assert.IsTrue(COgheAds.TryInterstitial("level_end", 6, null)); ads.Shown(); ads.Shown(); ads.Closed(true); ads.Closed(true);
            Assert.AreEqual(1, COgheShop.InterstitialsToday);
            Assert.AreEqual(1, COgheAnalytics.Recent.Count(x => x.StartsWith("ad_show ")));
            Assert.AreEqual(1, COgheAnalytics.Recent.Count(x => x.StartsWith("ad_complete ")));
        }
        [Test] public void PlusDoublesOnlyBaseRewardsAndAdvertisedBonusIsFixed()
        {
            COgheEntitlements.Grant(COgheEntitlements.Plus);
            Assert.AreEqual(20, COgheShop.WinReward("level"));
            COgheShop.Earn("win:level", COgheShop.WinReward("level"), "level_win");
            COgheAds.Rewarded("victory_triple", COgheReward.Victory("level"), null);
            Assert.AreEqual(40, COgheShop.Drops);
            COgheAds.Rewarded("daily_gift", COgheReward.Daily(), null);
            Assert.AreEqual(85, COgheShop.Drops); // +30 base +15 fixed ad bonus, not 60.
            Assert.IsFalse(COgheShop.GiftReady);
        }
        [Test] public void ThreeShopRewardsPerDayAndMidnightUsesTheOfferDay()
        {
            COgheShop.TodayForTests = "2026-10-04";
            COgheEntitlements.Grant(COgheEntitlements.Plus);
            for (int i = 0; i < 5; i++) COgheAds.Rewarded("drops", COgheReward.Shop(), null);
            Assert.AreEqual(45, COgheShop.Drops); Assert.IsFalse(COgheShop.AdForDropsAvailable);
            COgheShop.TodayForTests = "2026-10-05"; var delayed = COgheReward.Shop();
            COgheShop.TodayForTests = "2026-10-06"; Assert.IsTrue(COgheShop.AdForDropsAvailable);
            COgheShop.ApplyReward(delayed); Assert.AreEqual(0, COgheShop.Current.AdsForDropsToday);
        }
        [Test] public void ConfigValidatesSafetyAndAppliesOnlyOnNextLaunchWithOfflineCache()
        {
            const string key = "coghe.economy.config.v1"; bool existed = PlayerPrefs.HasKey(key); string backup = PlayerPrefs.GetString(key);
            try
            {
                VenomCampaignSave.PersistenceEnabled = true;
                var values = new Dictionary<string, string> { ["drops_first_win"] = "12", ["interstitial_min_seconds"] = "180", ["economy_variant"] = "conservative" };
                Assert.IsTrue(COgheEconomy.StageRemote(values)); Assert.AreEqual(10, COgheEconomy.FirstWin);
                values["interstitial_min_seconds"] = "0"; Assert.IsFalse(COgheEconomy.StageRemote(values));
                COgheEconomy.LoadCached(); Assert.AreEqual(12, COgheEconomy.FirstWin); Assert.AreEqual(180, COgheEconomy.InterstitialMinSeconds);
                Assert.AreEqual("conservative", COgheEconomy.Variant);
                Assert.IsFalse(COgheEconomy.ValidateRemote("price_INK_MINT", "-1"));
                Assert.IsFalse(COgheEconomy.ValidateRemote("price_unknown", "40"));
                Assert.IsFalse(COgheEconomy.ValidateRemote("gift_items", "MISSING"));
                Assert.IsFalse(COgheEconomy.ValidateRemote("interstitial_daily_cap", "999"));
                Assert.AreEqual(10, COgheEconomy.RemoteDefaults()["drops_first_win"], "SDK defaults are independent of last session overrides");
            }
            finally { if (existed) PlayerPrefs.SetString(key, backup); else PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); }
        }
        [Test] public void OptionalCleanBonusDoesNotPunishRetryByDefault()
        {
            COgheShop.NoteRetry("retried"); Assert.AreEqual(10, COgheShop.WinReward("retried"));
            COgheEconomy.Set("drops_clean_win", 5);
            Assert.AreEqual(15, COgheShop.WinReward("fresh")); Assert.AreEqual(10, COgheShop.WinReward("retried"));
        }
    }
}
