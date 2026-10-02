using System;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>An ad network / mediation SDK (Mrk integrates it; it plugs in through <see cref="COgheAds.Provider"/>).</summary>
    public interface ICOgheAdProvider
    {
        bool InterstitialReady { get; }
        void ShowInterstitial(string placement, Action done);
        bool RewardedReady { get; }
        /// <summary>done(true) once the reward was earned, done(false) if it was closed early or failed.</summary>
        void ShowRewarded(string placement, Action<bool> done);
        /// <summary>The anchored banner at the bottom of the safe area (shown or hidden).</summary>
        void SetBanner(bool show);
        /// <summary>The banner's height in screen pixels while shown (adaptive: it comes from the SDK), else 0.</summary>
        float BannerHeight { get; }
    }

    /// <summary>
    /// Every ad goes through here (Mrk 02/10, PLANS/COGHE_MONETIZATION_PLAN.md §6): the banner only on the main menu, Home and
    /// the victory screen; the full-screen ad only between levels after a win, paced (from the 6th win, 2 wins and 90 s
    /// apart, capped per session and day, never right after a rewarded ad); rewarded ads only when the player asks. No
    /// ads at all with No Ads or Plus; Plus gets rewards without an ad. When an ad is not ready the game simply goes on.
    /// </summary>
    public static class COgheAds
    {
        /// <summary>The real SDK, once integrated. Without it: test ads in development/test builds, otherwise none.</summary>
        public static ICOgheAdProvider Provider;
        /// <summary>Test ads (a grey card that "plays" for 2 s, a grey banner): on by default in development/test builds outside
        /// tests and proofs; a switch in Pause turns them off.</summary>
        public static bool TestAds = COgheTestTools.LevelSelect && !Application.isBatchMode;
        private static ICOgheAdProvider none, fake;
        private static ICOgheAdProvider Active
        {
            get
            {
                if (Provider != null) return Provider;
                if (TestAds && COgheProductMode.OverrideForTests != true) return fake ??= new COgheTestAds();
                return none ??= new COgheNoAds();
            }
        }
        /// <summary>A full-screen ad is up: the game waits (no auto-advance, no input).</summary>
        public static bool Showing { get; private set; }
        public static event Action BannerChanged;
        private static bool bannerWanted; private static ICOgheAdProvider bannerOn;
        private static float lastInterstitial = -1e6f, lastRewarded = -1e6f;
        private static int winsSinceInterstitial, sessionInterstitials;

        public static float Now => Time.realtimeSinceStartup;

        // Banner -------------------------------------------------------------------------------------------------------------
        /// <summary>Whether the current screen wants the banner (the product UI asks on every page change).</summary>
        public static void Banner(bool show)
        {
            bool on = show && !COgheEntitlements.AdsRemoved;
            if (on == bannerWanted && Active == bannerOn) return;
            if (bannerOn != null && bannerOn != Active) bannerOn.SetBanner(false);   // the SDK (or test ads) changed
            bannerWanted = on; bannerOn = Active; Active.SetBanner(on); BannerChanged?.Invoke();
        }
        /// <summary>The SDK calls this when the banner's size changes (an adaptive banner loads late): screens make room.</summary>
        public static void NotifyBannerChanged() => BannerChanged?.Invoke();
        /// <summary>Screen pixels the banner takes at the bottom (0 when there is none).</summary>
        public static float BannerHeight => bannerWanted && !COgheEntitlements.AdsRemoved ? Active.BannerHeight : 0;

        // Rewarded ----------------------------------------------------------------------------------------------------------
        /// <summary>Can a reward be offered now (Plus gets it without an ad)?</summary>
        public static bool RewardedAvailable => COgheEntitlements.InstantRewards || Active.RewardedReady;
        public static void Rewarded(string placement, Action<bool> done)
        {
            if (COgheEntitlements.InstantRewards) { COgheAnalytics.Log("ad_rewarded_instant", "placement", placement); done?.Invoke(true); return; }
            if (Showing || !Active.RewardedReady) { COgheAnalytics.Log("ad_fail", "type", "rewarded", "placement", placement); done?.Invoke(false); return; }
            Showing = true; COgheAnalytics.Log("ad_show", "type", "rewarded", "placement", placement);
            bool finished = false;
            Active.ShowRewarded(placement, ok =>
            {
                if (finished) return; finished = true;   // a callback that comes twice pays once
                Showing = false; lastRewarded = Now;
                COgheAnalytics.Log(ok ? "ad_complete" : "ad_closed", "type", "rewarded", "placement", placement);
                done?.Invoke(ok);
            });
        }

        // Full-screen, between levels ------------------------------------------------------------------------------------
        public static void NoteWin() { winsSinceInterstitial++; }
        /// <summary>Is a full-screen ad due after this win (<paramref name="totalWins"/> levels won so far)?</summary>
        public static bool InterstitialDue(int totalWins)
        {
            if (COgheEntitlements.AdsRemoved) return false;
            if (totalWins < COgheEconomy.InterstitialFromWin) return false;
            if (winsSinceInterstitial < COgheEconomy.InterstitialMinWins) return false;
            if (Now - lastInterstitial < COgheEconomy.InterstitialMinSeconds) return false;
            if (Now - lastRewarded < COgheEconomy.RewardedQuietSeconds) return false;
            if (sessionInterstitials >= COgheEconomy.InterstitialSessionCap) return false;
            return COgheShop.InterstitialsToday < COgheEconomy.InterstitialDailyCap;
        }
        /// <summary>Show the full-screen ad if it is due and ready; false (and nothing happens) otherwise.</summary>
        public static bool TryInterstitial(string placement, int totalWins, Action done)
        {
            if (Showing || !InterstitialDue(totalWins)) return false;
            if (!Active.InterstitialReady) { COgheAnalytics.Log("ad_fail", "type", "interstitial", "placement", placement); return false; }
            Showing = true; winsSinceInterstitial = 0; sessionInterstitials++; lastInterstitial = Now; COgheShop.NoteInterstitial();
            COgheAnalytics.Log("ad_show", "type", "interstitial", "placement", placement);
            bool finished = false;
            Active.ShowInterstitial(placement, () =>
            {
                if (finished) return; finished = true;
                Showing = false; COgheAnalytics.Log("ad_complete", "type", "interstitial", "placement", placement);
                done?.Invoke();
            });
            return true;
        }

        public static void ResetForTests()
        {
            Provider = null; Showing = false; bannerWanted = false; bannerOn = null; lastInterstitial = lastRewarded = -1e6f;
            winsSinceInterstitial = sessionInterstitials = 0; TestAds = false;
        }
    }

    /// <summary>No SDK yet (a store build before integration): no ads, nothing to wait for.</summary>
    public sealed class COgheNoAds : ICOgheAdProvider
    {
        public bool InterstitialReady => false;
        public void ShowInterstitial(string placement, Action done) => done();
        public bool RewardedReady => false;
        public void ShowRewarded(string placement, Action<bool> done) => done(false);
        public void SetBanner(bool show) { }
        public float BannerHeight => 0;
    }

    /// <summary>
    /// Test ads for development/test builds: a grey card over everything that "plays" for 2 s and then can be closed, and a
    /// grey 50 dp banner, so placement, pacing and rewards can be checked on a phone before the SDK exists.
    /// </summary>
    public sealed class COgheTestAds : ICOgheAdProvider
    {
        private Canvas canvas; private RectTransform banner, card; private Text cardText; private Button close;
        private float bannerHeight;
        public bool InterstitialReady => true;
        public bool RewardedReady => true;
        public float BannerHeight => banner != null && banner.gameObject.activeSelf ? bannerHeight : 0;

        private void Ensure()
        {
            if (canvas != null) return;
            var go = new GameObject("COghe test ads", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            UnityEngine.Object.DontDestroyOnLoad(go);
            canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 500;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            banner = Panel(go.transform, "Test banner", new Color(.62f, .64f, .63f), font, "Test banner · 320×50", out _);
            card = Panel(go.transform, "Test ad", new Color(.16f, .19f, .2f, .97f), font, "", out cardText);
            card.anchorMin = Vector2.zero; card.anchorMax = Vector2.one; card.offsetMin = card.offsetMax = Vector2.zero;
            var closeRect = Panel(card, "Close test ad", new Color(.9f, .9f, .88f), font, "Close", out var closeText);
            closeText.color = new Color(.19f, .3f, .34f);
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(.5f, .25f); closeRect.sizeDelta = new Vector2(360, 120);
            close = closeRect.gameObject.AddComponent<Button>();
            banner.gameObject.SetActive(false); card.gameObject.SetActive(false);
        }
        private static RectTransform Panel(Transform parent, string name, Color color, Font font, string label, out Text text)
        {
            var r = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>(); r.SetParent(parent, false);
            r.GetComponent<Image>().color = color;
            text = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>(); text.rectTransform.SetParent(r, false);
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one; text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            text.font = font; text.text = label; text.alignment = TextAnchor.MiddleCenter; text.fontSize = 34; text.color = Color.white; text.raycastTarget = false;
            return r;
        }

        public void SetBanner(bool show)
        {
            Ensure();
            float dp = Screen.dpi > 0 ? Screen.dpi / 160f : Mathf.Max(1, Screen.height / 800f);
            bannerHeight = Mathf.Round(50 * dp);
            var safe = Screen.safeArea;
            banner.anchorMin = banner.anchorMax = Vector2.zero; banner.pivot = Vector2.zero;
            banner.anchoredPosition = new Vector2(safe.x, safe.y); banner.sizeDelta = new Vector2(safe.width, bannerHeight);
            banner.GetComponentInChildren<Text>().fontSize = Mathf.RoundToInt(bannerHeight * .32f);
            banner.gameObject.SetActive(show);
        }
        public void ShowInterstitial(string placement, Action done) => Play("Test ad (full screen) · " + placement, false, ok => done());
        public void ShowRewarded(string placement, Action<bool> done) => Play("Test rewarded ad · " + placement, true, done);

        private void Play(string title, bool rewarded, Action<bool> done)
        {
            Ensure();
            card.gameObject.SetActive(true); card.SetAsLastSibling();
            var runner = card.GetComponent<Runner>(); if (runner == null) runner = card.gameObject.AddComponent<Runner>();
            runner.Begin(title, rewarded, cardText, close, ok => { card.gameObject.SetActive(false); done(ok); });
        }

        private sealed class Runner : MonoBehaviour
        {
            private float end; private string title; private bool rewarded; private Text text; private Button close; private Action<bool> done;
            public void Begin(string t, bool r, Text label, Button button, Action<bool> finish)
            {
                title = t; rewarded = r; text = label; close = button; done = finish; end = Time.realtimeSinceStartup + 2;
                close.onClick.RemoveAllListeners();
                close.onClick.AddListener(() => { bool earned = Time.realtimeSinceStartup >= end; var d = done; done = null; d?.Invoke(!rewarded || earned); });
            }
            private void Update()
            {
                if (done == null) return;
                float left = end - Time.realtimeSinceStartup;
                text.text = title + "\n\n" + (left > 0 ? (rewarded ? "Reward in " : "") + Mathf.CeilToInt(left) + " s" : rewarded ? "Reward earned" : "");
            }
        }
    }
}
