using System;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>The app stores' purchases (Unity IAP plugs in here once the products exist on both stores).</summary>
    public interface ICOgheStore
    {
        bool Available { get; }
        /// <summary>The store's localized price ("$5.99").</summary>
        string Price(string product);
        void Purchase(string product, Action<bool> done);
        /// <summary>Restore purchases (Apple requires it); calls <see cref="COgheEntitlements.Grant"/> for each one found.</summary>
        void Restore(Action<bool> done);
    }

    /// <summary>
    /// COghe Plus ($5.99) and No Ads ($2.99), one-time purchases (Mrk 02/10). Plus: no banner or full-screen ads, rewards
    /// without watching an ad, Drops ×2. No Ads: only the ads go. The flags are kept on the device so the game works
    /// offline; the store integration re-grants them from receipts and Restore.
    /// </summary>
    public static class COgheEntitlements
    {
        public const string Plus = "coghe_plus", NoAds = "coghe_no_ads";
        private const string Key = "coghe.entitlements.v1";
        private static ICOgheStore store;
        /// <summary>The store: Unity IAP once integrated; a test store in development/test builds; none in a store build until then.</summary>
        public static ICOgheStore Store { get => store ??= COgheTestTools.LevelSelect ? new COgheTestStore() : null; set => store = value; }
        public static event Action Changed;
        private static int flags = -1;
        private static int Flags
        {
            get { if (flags < 0) flags = VenomCampaignSave.PersistenceEnabled ? PlayerPrefs.GetInt(Key, 0) : 0; return flags; }
            set { flags = value; if (VenomCampaignSave.PersistenceEnabled) { PlayerPrefs.SetInt(Key, value); PlayerPrefs.Save(); } Changed?.Invoke(); }
        }
        public static bool HasPlus => (Flags & 1) != 0;
        public static bool HasNoAds => (Flags & 2) != 0;
        public static bool AdsRemoved => HasPlus || HasNoAds;
        public static bool InstantRewards => HasPlus;
        public static int DropsMultiplier => HasPlus ? 2 : 1;
        public static bool StoreAvailable => Store != null && Store.Available;

        public static void Grant(string product)
        {
            int bit = product == Plus ? 1 : product == NoAds ? 2 : 0;
            if (bit == 0 || (Flags & bit) != 0) return;
            Flags |= bit;
            COgheAnalytics.Log("iap_entitlement", "product", product);
        }
        public static void Buy(string product, Action<bool> done)
        {
            if (!StoreAvailable) { done?.Invoke(false); return; }
            COgheAnalytics.Log("iap_view", "product", product);
            Store.Purchase(product, ok =>
            {
                if (ok) { Grant(product); COgheAnalytics.Log("iap_purchase", "product", product, "price", Store.Price(product)); }
                done?.Invoke(ok);
            });
        }
        public static void Restore(Action<bool> done)
        {
            if (!StoreAvailable) { done?.Invoke(false); return; }
            Store.Restore(ok => { COgheAnalytics.Log("iap_restore", "ok", ok); done?.Invoke(ok); });
        }
        public static void ResetForTests(int set = 0) { flags = set; store = null; }
    }

    /// <summary>Development and test builds: purchases succeed at once (no money), so every flow can be tried on a phone.</summary>
    public sealed class COgheTestStore : ICOgheStore
    {
        public bool Available => true;
        public string Price(string product) => product == COgheEntitlements.Plus ? "$5.99" : "$2.99";
        public void Purchase(string product, Action<bool> done) => done(true);
        public void Restore(Action<bool> done) => done(true);
    }
}
