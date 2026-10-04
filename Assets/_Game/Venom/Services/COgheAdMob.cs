using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Services
{
    public sealed class COgheAdMob : MonoBehaviour, ICOgheAdProvider
    {
        public const string BannerId = "ca-app-pub-7806638519709442/2373866483";
        public const string InterstitialId = "ca-app-pub-7806638519709442/7274839373";
        public const string RewardedId = "ca-app-pub-7806638519709442/1245541087";
        private const string TestBanner = "ca-app-pub-3940256099942544/6300978111";
        private const string TestInterstitial = "ca-app-pub-3940256099942544/1033173712";
        private const string TestRewarded = "ca-app-pub-3940256099942544/5224354917";
        private BannerView banner;
        private InterstitialAd interstitial;
        private RewardedAd rewarded;
        private bool wanted, bannerLoaded, loadingI, loadingR, suspended;
        private float nextI, nextR, nextB, loadedI, loadedR, tick;
        private int failuresI, failuresR, failuresB, generation, bannerWidth;
        private Action<bool> finishInterstitial, finishRewarded;
        private static string Unit(string real, string test) => COgheTestTools.LevelSelect || Debug.isDebugBuild ? test : real;
        private bool ConsentAllowed => !suspended && ConsentInformation.CanRequestAds();
        private bool ForcedAllowed => ConsentAllowed && COgheAds.ForcedAdsAllowed;
        private bool RewardAllowed => ConsentAllowed && COgheAds.RewardAdsAllowed;
        public bool InterstitialReady => ForcedAllowed && interstitial != null && interstitial.CanShowAd();
        public bool RewardedReady => RewardAllowed && rewarded != null && rewarded.CanShowAd();
        public float BannerHeight => wanted && ForcedAllowed && bannerLoaded && banner != null ? banner.GetHeightInPixels() : 0;
        private static void Main(Action action) => COgheGoogleServices.Main(action);

        private void Update()
        {
            if (Time.realtimeSinceStartup < tick) return;
            tick = Time.realtimeSinceStartup + 1;
            if (!ForcedAllowed)
            { if (banner != null) DestroyBanner(); interstitial?.Destroy(); interstitial = null; }
            if (!RewardAllowed) { rewarded?.Destroy(); rewarded = null; }
            if (!ConsentAllowed || COgheAds.Showing) return;
            if (interstitial != null && tick - loadedI > 3500) { interstitial.Destroy(); interstitial = null; }
            if (rewarded != null && tick - loadedR > 3500) { rewarded.Destroy(); rewarded = null; }
            if (ForcedAllowed && interstitial == null && !loadingI && tick >= nextI) LoadInterstitial();
            if (RewardAllowed && rewarded == null && !loadingR && tick >= nextR) LoadRewarded();
            if (wanted && bannerWidth != Screen.width) DestroyBanner();
            if (ForcedAllowed && wanted && banner == null && tick >= nextB) LoadBanner();
        }
        private static float Backoff(ref int count) => Mathf.Min(60, 5 * Mathf.Pow(2, Math.Min(count++, 4)));
        private void LoadInterstitial()
        {
            loadingI = true; int ticket = generation;
            InterstitialAd.Load(Unit(InterstitialId, TestInterstitial), new AdRequest(), (ad, error) => Main(() =>
            {
                if (this == null || ticket != generation) { ad?.Destroy(); return; }
                loadingI = false;
                if (!ForcedAllowed) { ad?.Destroy(); return; }
                if (error != null || ad == null) { nextI = Time.realtimeSinceStartup + Backoff(ref failuresI); LoadError("interstitial", error); return; }
                interstitial = ad; failuresI = 0; loadedI = Time.realtimeSinceStartup;
            }));
        }
        private void LoadRewarded()
        {
            loadingR = true; int ticket = generation;
            RewardedAd.Load(Unit(RewardedId, TestRewarded), new AdRequest(), (ad, error) => Main(() =>
            {
                if (this == null || ticket != generation) { ad?.Destroy(); return; }
                loadingR = false;
                if (!RewardAllowed) { ad?.Destroy(); return; }
                if (error != null || ad == null) { nextR = Time.realtimeSinceStartup + Backoff(ref failuresR); LoadError("rewarded", error); return; }
                rewarded = ad; failuresR = 0; loadedR = Time.realtimeSinceStartup;
                COgheAds.NotifyAvailabilityChanged();
            }));
        }
        private static void LoadError(string format, LoadAdError error) =>
            COgheAnalytics.Log("ad_load_fail", "type", format, "code", error == null ? -1 : error.GetCode());
        private static void Paid(AdValue value, string format, string placement, string unit, ResponseInfo response)
        {
            // Separate diagnostic event: Firebase/AdMob may auto-collect ad_impression; never duplicate it.
            COgheAnalytics.Log("ad_revenue", "type", format, "placement", placement, "ad_unit_id", unit,
                "value", value.Value / 1000000.0, "currency", value.CurrencyCode,
                "precision", value.Precision.ToString(), "ad_source", response?.GetMediationAdapterClassName() ?? "unknown");
        }
        public void ShowInterstitial(string placement, Action shown, Action<bool> closed)
        {
            if (!InterstitialReady) { closed?.Invoke(false); return; }
            var ad = interstitial; var response = ad.GetResponseInfo(); interstitial = null; bool finished = false, opened = false;
            void Finish(bool ok)
            {
                if (finished) return; finished = true; ad.Destroy(); finishInterstitial = null; closed?.Invoke(ok && opened);
            }
            finishInterstitial = Finish;
            ad.OnAdFullScreenContentOpened += () => Main(() => { if (finished || opened) return; opened = true; shown?.Invoke(); });
            ad.OnAdFullScreenContentClosed += () => Main(() => Finish(true));
            ad.OnAdFullScreenContentFailed += error => Main(() => Finish(false));
            ad.OnAdPaid += value => Main(() => Paid(value, "interstitial", placement, Unit(InterstitialId, TestInterstitial), response));
            try { ad.Show(); } catch (Exception) { Finish(false); }
        }
        public void ShowRewarded(string placement, Action shown, Action earned, Action<bool> closed)
        {
            if (!RewardedReady) { closed?.Invoke(false); return; }
            var ad = rewarded; var response = ad.GetResponseInfo(); rewarded = null; bool finished = false, opened = false, paid = false;
            void Finish(bool ok)
            {
                if (finished) return; finished = true; ad.Destroy(); finishRewarded = null; closed?.Invoke(ok && opened);
            }
            finishRewarded = Finish;
            ad.OnAdFullScreenContentOpened += () => Main(() => { if (finished || opened) return; opened = true; shown?.Invoke(); });
            ad.OnAdFullScreenContentClosed += () => Main(() => Finish(true));
            ad.OnAdFullScreenContentFailed += error => Main(() => Finish(false));
            ad.OnAdPaid += value => Main(() => Paid(value, "rewarded", placement, Unit(RewardedId, TestRewarded), response));
            try { ad.Show(reward => Main(() => { if (paid) return; paid = true; earned?.Invoke(); })); } catch (Exception) { Finish(false); }
        }
        public void SetBanner(bool show)
        {
            wanted = show;
            if (!show || !ForcedAllowed) { DestroyBanner(); return; }
            if (banner == null && Time.realtimeSinceStartup >= nextB) LoadBanner();
            else if (bannerLoaded) banner.Show();
            COgheAds.NotifyBannerChanged();
        }
        private void LoadBanner()
        {
            bannerWidth = Screen.width;
            var size = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            var current = new BannerView(Unit(BannerId, TestBanner), size, AdPosition.Bottom); banner = current;
            current.Hide();
            current.OnBannerAdLoaded += () => Main(() =>
            {
                if (this == null || banner != current) return;
                bannerLoaded = true; failuresB = 0;
                if (wanted && ForcedAllowed) current.Show(); else current.Hide();
                COgheAds.NotifyBannerChanged();
            });
            current.OnBannerAdLoadFailed += error => Main(() =>
            {
                if (this == null || banner != current) return;
                nextB = Time.realtimeSinceStartup + Backoff(ref failuresB); DestroyBanner(); LoadError("banner", error);
            });
            current.OnAdPaid += value =>
            {
                string page = COgheAds.BannerPlacement; var response = current.GetResponseInfo();
                Main(() => Paid(value, "banner", page, Unit(BannerId, TestBanner), response));
            };
            current.OnAdImpressionRecorded += () =>
            { string page = COgheAds.BannerPlacement; Main(() => COgheAnalytics.Log("ad_show", "type", "banner", "placement", page)); };
            current.OnAdClicked += () =>
            { string page = COgheAds.BannerPlacement; Main(() => COgheAnalytics.Log("ad_click", "type", "banner", "placement", page)); };
            current.LoadAd(new AdRequest());
        }
        private void DestroyBanner() { banner?.Destroy(); banner = null; bannerLoaded = false; COgheAds.NotifyBannerChanged(); }
        public void Suspend()
        {
            suspended = true; generation++; loadingI = loadingR = false;
            DestroyBanner(); interstitial?.Destroy(); interstitial = null; rewarded?.Destroy(); rewarded = null;
        }
        public void Resume() { suspended = false; nextI = nextR = nextB = 0; }
        private void OnDestroy() { Suspend(); finishInterstitial?.Invoke(false); finishRewarded?.Invoke(false); }
    }
}
