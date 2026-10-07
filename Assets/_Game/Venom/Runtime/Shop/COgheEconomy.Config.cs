using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace GravityBox.Venom
{
    public static partial class COgheEconomy
    {
        private const string CacheKey = "coghe.economy.config.v1";
        private static readonly HashSet<string> extraGifts = new();
        public static string Revision { get; private set; } = "defaults";
        public static string Variant { get; private set; } = "control";
        public static bool PlusOfferEnabled => Value("plus_offer_enabled", 1) != 0;
        public static bool BannerEnabled(string page) => Value("banner_" + (page == "MainMenu" ? "menu" : page.ToLowerInvariant()), 1) != 0;
        private static readonly Dictionary<string, (int value, int min, int max)> schema = new()
        {
            ["drops_first_win"] = (10, 1, 1000), ["drops_clean_win"] = (0, 0, 100),
            ["drops_triple_ad"] = (20, 1, 1000), ["drops_daily"] = (15, 1, 1000), ["drops_bonus"] = (100, 1, 1000), ["drops_bonus_round"] = (10, 1, 1000),
            ["drops_ad_shop"] = (15, 1, 1000), ["ad_shop_daily_cap"] = (3, 0, 10),
            ["ad_item_max_price"] = (60, 0, 200), ["interstitial_from_win"] = (6, 6, 1000),
            ["interstitial_min_wins"] = (2, 2, 100), ["interstitial_min_seconds"] = (90, 90, 86400),
            ["interstitial_session_cap"] = (4, 0, 4), ["interstitial_daily_cap"] = (10, 0, 10),
            ["rewarded_quiet_seconds"] = (90, 90, 86400), ["victory_triple_ms"] = (3500, 3000, 10000),
            ["banner_menu"] = (1, 0, 1), ["banner_home"] = (1, 0, 1), ["banner_victory"] = (1, 0, 1)
        };
        public static Dictionary<string, object> RemoteDefaults()
        {
            var defaults = new Dictionary<string, object>();
            foreach (var pair in schema) defaults[pair.Key] = pair.Value.value;
            foreach (var item in Items) if (Array.IndexOf(Gifts, item.Id) < 0) defaults["price_" + item.Id] = DefaultPrice(item.Id);
            defaults["gift_items"] = string.Join(",", Gifts);
            defaults["plus_offer_after"] = "home_visit";
            defaults["economy_revision"] = "2026-10-04"; defaults["economy_variant"] = "control";
            return defaults;
        }
        [Serializable] private sealed class Pair { public string key, value; }
        [Serializable] private sealed class Snapshot { public List<Pair> values = new(); }

        public static bool ValidateRemote(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || value == null) return false;
            if (schema.TryGetValue(key, out var rule))
                return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) && n >= rule.min && n <= rule.max;
            if (key.StartsWith("price_", StringComparison.Ordinal))
                return Find(key.Substring(6)) != null && Array.IndexOf(Gifts, key.Substring(6)) < 0 && int.TryParse(value, out int price) && price >= 5 && price <= 2000;
            if (key == "plus_offer_after") return value == "home_visit" || value == "never";
            if (key == "gift_items")
            {
                foreach (var id in value.Split(',')) if (Find(id.Trim()) == null) return false;
                return true;
            }
            if (key == "economy_revision" || key == "economy_variant")
            {
                if (value.Length == 0 || value.Length > 40) return false;
                foreach (char c in value) if (!char.IsLetterOrDigit(c) && c != '-' && c != '_' && c != '.') return false;
                return true;
            }
            return false;
        }
        /// <summary>Reject a malformed snapshot as a whole. Never change displayed offers in a running session.</summary>
        public static bool StageRemote(IReadOnlyDictionary<string, string> values)
        {
            var snapshot = new Snapshot();
            foreach (var pair in values)
            {
                if (!ValidateRemote(pair.Key, pair.Value)) return false;
                snapshot.values.Add(new Pair { key = pair.Key, value = pair.Value });
            }
            if (snapshot.values.Count == 0) return false;
            if (VenomCampaignSave.PersistenceEnabled)
            { PlayerPrefs.SetString(CacheKey, JsonUtility.ToJson(snapshot)); PlayerPrefs.Save(); }
            return true;
        }
        public static void LoadCached()
        {
            if (!VenomCampaignSave.PersistenceEnabled) return;
            try
            {
                var snapshot = JsonUtility.FromJson<Snapshot>(PlayerPrefs.GetString(CacheKey, ""));
                if (snapshot?.values == null) return;
                foreach (var pair in snapshot.values) if (!ValidateRemote(pair.key, pair.value)) return;
                foreach (var pair in snapshot.values)
                {
                    if (pair.key == "gift_items") foreach (var id in pair.value.Split(',')) { extraGifts.Add(id.Trim()); COgheShop.Grant(id.Trim(), "config_gift"); }
                    else if (pair.key == "plus_offer_after") overrides["plus_offer_enabled"] = pair.value == "never" ? 0 : 1;
                    else if (pair.key == "economy_revision") Revision = pair.value;
                    else if (pair.key == "economy_variant") Variant = pair.value;
                    else overrides[pair.key] = int.Parse(pair.value, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception e) { Debug.LogWarning("[COghe economy] Cache ignored: " + e.GetType().Name); }
        }
    }
}
