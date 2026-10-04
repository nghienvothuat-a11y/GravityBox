#if UNITY_ANDROID && (DEVELOPMENT_BUILD || COGHE_TEST_TOOLS)
using System.Collections;
using System.IO;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Services
{
    /// <summary>Explicit device SDK check. Uses a memory-only save; absent from store builds.
    /// Android launch extra: --ez coghe_services_proof true. Rewards are never persisted.</summary>
    public sealed class COgheServicesProof : MonoBehaviour
    {
        private string output;
        private VenomCampaign game;
        private COgheProductUI ui;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var intent = activity.Call<AndroidJavaObject>("getIntent");
            if (!intent.Call<bool>("getBooleanExtra", "coghe_services_proof", false)) return;
            VenomCampaignSave.PersistenceEnabled = false;
            COgheProductMode.OverrideForTests = true; COgheIntro.Seen = true;
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true });
            COgheEntitlements.ResetForTests();
            var host = new GameObject("Opt-in Google services proof"); DontDestroyOnLoad(host);
            host.AddComponent<COgheServicesProof>();
        }
        private IEnumerator Start()
        {
            output = Path.Combine(Application.persistentDataPath, "ServicesProof"); Directory.CreateDirectory(output);
            yield return new WaitForSecondsRealtime(3);
            game = FindFirstObjectByType<VenomCampaign>(); ui = game.ProductUI;
            ui.ShowMenu();
            float until = Time.realtimeSinceStartup + 65;
            while ((!COgheGoogleServices.Ready || COgheAds.BannerHeight <= 0) && Time.realtimeSinceStartup < until) yield return null;
            Debug.Log($"COGHE SERVICES PROOF ready={COgheGoogleServices.Ready} banner={COgheAds.BannerHeight}");
            yield return Shot("01-menu");
            ui.Play(); game.AutoAdvance = false; yield return new WaitForSecondsRealtime(.8f);
            // Real crawl/exit: no completion or position overrides.
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            until = Time.realtimeSinceStartup + 45;
            while (!game.Owner.Completed && Time.realtimeSinceStartup < until) yield return null;
            yield return new WaitForSecondsRealtime(1);
            yield return Shot("02-win");
            Debug.Log("COGHE SERVICES PROOF actualWin=" + game.Owner.Completed);
            ui.ShowPopup(COgheProductPopup.Restart); ui.Confirm();
            yield return new WaitForSecondsRealtime(1);
            // Known loss fixture checks the production failure route, not puzzle solvability.
            game.Fail(VenomCampaign.MergeFailure); yield return new WaitForSecondsRealtime(.5f);
            yield return Shot("03-loss"); ui.Confirm(); yield return new WaitForSecondsRealtime(.5f);
            ui.ShowMenu();
            File.WriteAllLines(Path.Combine(output, "events.txt"), COgheAnalytics.Recent);
            Debug.Log("COGHE SERVICES PROOF gameplay events saved; awaiting test ad interaction");
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var intent = activity.Call<AndroidJavaObject>("getIntent");
            if (!intent.Call<bool>("getBooleanExtra", "coghe_ads_proof", false)) yield break;
            // SDK presentation fixture only: pacing itself has PlayMode coverage. No saved wins are changed.
            until = Time.realtimeSinceStartup + 45;
            while (COgheAds.Provider?.InterstitialReady != true && Time.realtimeSinceStartup < until) yield return null;
            COgheAds.NoteWin(); COgheAds.NoteWin();
            bool closed = false;
            bool shown = COgheAds.TryInterstitial("services_proof", 6, () => closed = true);
            Debug.Log("COGHE SERVICES PROOF interstitialShown=" + shown);
            until = Time.realtimeSinceStartup + 120;
            while (shown && !closed && Time.realtimeSinceStartup < until) yield return null;
            Debug.Log("COGHE SERVICES PROOF interstitialClosed=" + closed);
            if (COgheAds.Showing) yield break;
            COgheEntitlements.Grant(COgheEntitlements.NoAds); ui.ShowMenu();
            until = Time.realtimeSinceStartup + 60;
            while (!COgheAds.RewardedAvailable && Time.realtimeSinceStartup < until) yield return null;
            Debug.Log($"COGHE SERVICES PROOF NoAds forcedAllowed={COgheAds.ForcedAdsAllowed} banner={COgheAds.BannerHeight} rewardedReady={COgheAds.RewardedAvailable}");
            int before = COgheShop.Drops; bool earned = false; closed = false;
            var reward = COgheReward.Shop();
            COgheAds.Rewarded("no_ads_proof", reward, ok => { earned = ok; closed = true; });
            until = Time.realtimeSinceStartup + 120;
            while (!closed && Time.realtimeSinceStartup < until) yield return null;
            Debug.Log($"COGHE SERVICES PROOF NoAds reward={earned} delta={COgheShop.Drops - before} expected={reward.Amount} paid={COgheShop.Paid(reward.Key)} cap={COgheShop.Current.AdsForDropsToday}");
            if (COgheAds.Showing) yield break;
            yield return Shot("04-no-ads-reward");
            COgheEntitlements.Grant(COgheEntitlements.Plus); before = COgheShop.Drops;
            COgheAds.Rewarded("plus_proof", COgheReward.Shop(), ok => earned = ok);
            Debug.Log($"COGHE SERVICES PROOF Plus reward={earned} delta={COgheShop.Drops - before} showing={COgheAds.Showing}");
            File.WriteAllLines(Path.Combine(output, "events.txt"), COgheAnalytics.Recent);
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame(); var texture = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(output, name + ".png"), texture.EncodeToPNG()); Destroy(texture);
        }
    }
}
#endif
