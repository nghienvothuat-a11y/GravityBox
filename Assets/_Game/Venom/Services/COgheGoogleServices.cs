using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Services
{
    /// <summary>Android SDK lifetime; game rules depend only on the existing ads / analytics interfaces.</summary>
    public sealed class COgheGoogleServices : MonoBehaviour
    {
        public const string AndroidPackage = "com.gravityboxlab.venom";
        public const string AdMobApp = "ca-app-pub-7806638519709442~4983717950";
        public static bool Ready { get; private set; }
        private readonly Queue<KeyValuePair<string, IReadOnlyDictionary<string, object>>> queue = new();
        private FirebaseApp firebase;
        private COgheAdMob ads;
        private bool adsInitializing;
        private float consentRetryDelay = 15;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Application.identifier != AndroidPackage || Application.isBatchMode) return;
            var host = new GameObject("COghe Google services");
            DontDestroyOnLoad(host); host.AddComponent<COgheGoogleServices>();
#endif
        }

        private void Awake()
        {
            // UMP completes before MobileAds.Initialize. Its callbacks already need a Unity dispatcher.
            MobileAdsEventExecutor.Initialize();
            COgheAds.TestAds = false;
            COgheAds.Provider = new COgheNoAds();
            COgheAnalytics.Sink = Send;
            COgheEconomy.LoadCached();
            Debug.Log($"[COghe services] Active economy: {COgheEconomy.Revision}/{COgheEconomy.Variant}");
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (this == null) return;
                if (task.IsFaulted || task.IsCanceled || task.Result != DependencyStatus.Available)
                { queue.Clear(); Debug.LogWarning("[COghe services] Firebase unavailable; gameplay continues."); return; }
                firebase = FirebaseApp.DefaultInstance;
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                FirebaseAnalytics.SetUserProperty("build_channel", COgheTestTools.LevelSelect ? "test" : "store");
                FirebaseAnalytics.SetUserProperty("economy_revision", COgheEconomy.Revision);
                FirebaseAnalytics.SetUserProperty("economy_variant", COgheEconomy.Variant);
                Ready = true;
                while (queue.Count > 0) { var e = queue.Dequeue(); Send(e.Key, e.Value); }
                Debug.Log("[COghe services] Firebase ready: " + firebase.Options.ProjectId);
                COgheRemoteConfig.Fetch();
            });
            COgheAds.PrivacyRequired = () => ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
            COgheAds.OpenPrivacy = () => ConsentForm.ShowPrivacyOptionsForm(error => Main(() =>
            {
                ads?.Suspend();
                if (error != null) Debug.LogWarning("[COghe services] Privacy options: " + error.ErrorCode);
                EnableAds();
            }));
            RequestConsent();
        }

        private void RequestConsent()
        {
            CancelInvoke(nameof(RetryConsent));
            ConsentInformation.Update(new ConsentRequestParameters(), error => Main(() =>
            {
                if (this == null) return;
                if (error != null) { ConsentError(error); EnableAds(); return; }
                ConsentForm.LoadAndShowConsentFormIfRequired(formError => Main(() =>
                {
                    if (this == null) return;
                    if (formError != null) ConsentError(formError);
                    else consentRetryDelay = 15;
                    EnableAds();
                }));
            }));
        }

        private void ConsentError(FormError error)
        {
            Debug.LogWarning("[COghe services] Consent: " + error.ErrorCode + " " + error.Message);
            // A fresh install can launch offline. Recover in the same session when the network returns.
            // Invalid publisher configuration (3) requires a dashboard fix, not repeated requests.
            if (error.ErrorCode == 3) return;
            Invoke(nameof(RetryConsent), consentRetryDelay);
            consentRetryDelay = Mathf.Min(60, consentRetryDelay * 2);
        }

        private void RetryConsent()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                Invoke(nameof(RetryConsent), 15);
            else RequestConsent();
        }

        public static void Main(Action action) => MobileAdsEventExecutor.ExecuteInUpdate(action);

        private void EnableAds()
        {
            if (!ConsentInformation.CanRequestAds()) return;
            if (ads != null) { ads.Resume(); return; }
            if (adsInitializing) return;
            adsInitializing = true;
            MobileAds.Initialize(status => Main(() =>
            {
                adsInitializing = false;
                if (this == null || status == null || !ConsentInformation.CanRequestAds()) return;
                ads = gameObject.AddComponent<COgheAdMob>();
                COgheAds.Provider = ads;
                COgheAds.RefreshBanner();
                Debug.Log("[COghe services] AdMob ready; " + (COgheTestTools.LevelSelect ? "Google test units" : "production units"));
            }));
        }

        private void Send(string name, IReadOnlyDictionary<string, object> values)
        {
            if (!Ready)
            {
                if (queue.Count == 128) queue.Dequeue();
                queue.Enqueue(new KeyValuePair<string, IReadOnlyDictionary<string, object>>(name, values)); return;
            }
            var parameters = new List<Parameter>(values.Count + 4);
            foreach (var pair in values)
            {
                if (pair.Value is bool b) parameters.Add(new Parameter(pair.Key, b ? 1L : 0L));
                else if (pair.Value is int || pair.Value is long) parameters.Add(new Parameter(pair.Key, Convert.ToInt64(pair.Value)));
                else if (pair.Value is float || pair.Value is double) parameters.Add(new Parameter(pair.Key, Convert.ToDouble(pair.Value)));
                else parameters.Add(new Parameter(pair.Key, (pair.Value?.ToString() ?? "").Substring(0, Math.Min(100, pair.Value?.ToString().Length ?? 0))));
            }
            parameters.Add(new Parameter("build_channel", COgheTestTools.LevelSelect ? "test" : "store"));
            parameters.Add(new Parameter("event_schema", 1L));
            parameters.Add(new Parameter("economy_revision", COgheEconomy.Revision));
            parameters.Add(new Parameter("economy_variant", COgheEconomy.Variant));
            FirebaseAnalytics.LogEvent(name, parameters.ToArray());
        }

        private void OnDestroy()
        {
            COgheAnalytics.Sink = null; COgheAds.PrivacyRequired = null; COgheAds.OpenPrivacy = null; Ready = false;
        }
    }
}
