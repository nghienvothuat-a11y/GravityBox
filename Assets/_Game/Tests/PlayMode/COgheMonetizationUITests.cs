using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine;

namespace GravityBox.Tests
{
    public partial class COgheProductUITests
    {
        private sealed class VictoryDeferredAds : ICOgheAdProvider
        {
            public int Interstitials;
            public System.Action Earned;
            public System.Action<bool> Closed;
            public bool InterstitialReady => true;
            public bool RewardedReady => true;
            public float BannerHeight => 0;
            public void SetBanner(bool show) { }
            public void ShowInterstitial(string placement, System.Action shown, System.Action<bool> closed)
            { Interstitials++; Closed = closed; shown(); }
            public void ShowRewarded(string placement, System.Action shown, System.Action earned, System.Action<bool> closed)
            { Earned = earned; Closed = closed; shown(); }
        }

        [UnityTest] public IEnumerator VictoryRewardsAndAdClosureNeverAdvanceWithoutNextTap()
        {
            try
            {
                FreshShop(false); var ads = new VictoryDeferredAds(); COgheAds.Provider = ads;
                // UI fixture: make an interstitial eligible; this does not claim six puzzles were solved.
                for (int i = 1; i <= 6; i++) game.Progress.Completed.Add("victory-ui-" + i);
                COgheAds.NoteWin();
                ui.ShowVictoryForTests("manual-next", 1); yield return null;
                yield return Click("Triple drops");
                ui.NextLevel(); // Native rewarded ad is still open: this tap must not be queued.
                ads.Earned(); ads.Closed(true);
                yield return new WaitForSecondsRealtime(5);
                Assert.AreEqual(COgheProductPage.Victory, ui.Page);
                Assert.AreEqual(1, game.Definition.Order); Assert.AreEqual(0, ads.Interstitials);
                Assert.AreEqual(COgheEconomy.FirstWin + COgheEconomy.TripleExtra, COgheShop.Drops);
                // Reset only pacing to exercise the eligible interstitial independently of rewarded quiet time.
                COgheAds.ResetForTests(); COgheAds.Provider = ads; COgheAds.NoteWin(); COgheAds.NoteWin();
                yield return Click("Next level"); ui.NextLevel();
                Assert.AreEqual(1, ads.Interstitials); Assert.AreEqual(1, game.Definition.Order);
                ads.Closed(false); // A failed ad must still release the requested transition.
                float end = Time.realtimeSinceStartup + 8;
                while (game != null && game.Definition.Order == 1 && Time.realtimeSinceStartup < end) yield return null;
                yield return null; game = Object.FindFirstObjectByType<VenomCampaign>(); ui = game.ProductUI;
                Assert.AreEqual(2, game.Definition.Order); Assert.AreEqual(1, ads.Interstitials);
            }
            finally { EndShop(); }
        }

        [UnityTest] public IEnumerator VictoryUnlocksWaitForNextTapAndContinueTheRequestedTransition()
        {
            try
            {
                FreshShop(false);
                ui.ShowVictoryForTests("manual-unlocks", 10); yield return new WaitForSecondsRealtime(5);
                Assert.AreEqual(COgheProductPopup.None, ui.Popup);
                Assert.IsNotEmpty(ui.VictoryUnlocks); Assert.AreEqual(1, game.Definition.Order);
                yield return Click("Next level"); Assert.AreEqual(COgheProductPopup.Unlocks, ui.Popup);
                yield return Click("Continue");
                float end = Time.realtimeSinceStartup + 8;
                while (game != null && game.Definition.Order == 1 && Time.realtimeSinceStartup < end) yield return null;
                yield return null; game = Object.FindFirstObjectByType<VenomCampaign>(); ui = game.ProductUI;
                Assert.AreEqual(2, game.Definition.Order);
            }
            finally { EndShop(); }
        }

        [UnityTest] public IEnumerator BannerHidesForModalAndReturnsWhenClosed()
        {
            try
            {
                FreshShop(false); var ads = new CountingAds(); COgheAds.Provider = ads;
                ui.ShowMenu(); yield return null; Assert.IsTrue(ads.Shown);
                string visiblePage = COgheAds.BannerPlacement;
                COgheAds.Banner(false, "game"); Assert.AreEqual(visiblePage, COgheAds.BannerPlacement);
                ui.ShowMenu();
                ui.ShowShopPopup(COgheProductPopup.Buy); yield return null; Assert.IsFalse(ads.Shown);
                ui.Resume(); yield return null; Assert.IsTrue(ads.Shown);
                COgheEconomy.Set("banner_menu", 0); ui.ShowMenu(); Assert.IsFalse(ads.Shown);
            }
            finally { EndShop(); }
        }
        [UnityTest] public IEnumerator PlusOfferWaitsForHomeAndVictoryAdvertisesExactReward()
        {
            try
            {
                FreshShop(false); COgheAds.Provider = new CountingAds();
                ui.ShowMenu(); yield return null;
                Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(x => x.name == "Plus"));
                ui.ShowShopPopup(COgheProductPopup.Buy); yield return null;
                Assert.IsFalse(ui.GetComponentsInChildren<Button>().Any(x => x.name == "Plus offer"));
                ui.Resume();
                COgheShop.VisitCommerce(); ui.ShowMenu(); yield return null;
                Assert.IsTrue(ui.GetComponentsInChildren<Button>().Any(x => x.name == "Plus"));
                COgheEntitlements.Grant(COgheEntitlements.Plus);
                ui.ShowVictoryForTests("plus-ui", 1); yield return null;
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(x => x.text == "+20 Drops · Plus"));
                yield return Click("Triple drops");
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(x => x.text == "+40 Drops"));
                Assert.AreEqual(40, COgheShop.Drops);
            }
            finally { EndShop(); ui.ShowMenu(); }
        }
    }
}
