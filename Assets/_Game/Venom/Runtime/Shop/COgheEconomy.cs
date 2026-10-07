using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>What a piece of COghe's world is: furniture for Home, an ink, a hat, a thing floating inside.</summary>
    public enum COgheShopGroup { Home, Ink, Hat, Inside }

    public sealed class COgheShopItem
    {
        public string Id, Name; public int Level; public COgheShopGroup Group;
    }

    /// <summary>
    /// The numbers of COghe's economy (Mrk 02/10, PLANS/COGHE_MONETIZATION_PLAN.md): what a win, an ad and the daily gift
    /// pay in Drops, what each item costs, and how often the full-screen ad may come. Every value has a default here and can
    /// be replaced from Remote Config (<see cref="Set"/>) once the tracking SDK is in, without a new build.
    /// </summary>
    public static partial class COgheEconomy
    {
        // Gifts (Mrk 02/10): the ball when Home opens, and the first item of each Style group.
        public static readonly string[] Gifts = { "BALL", "INK_OCEAN", "HAT_BEANIE", "FLOAT_STARS" };
        private static readonly Dictionary<string, int> overrides = new Dictionary<string, int>();

        public static int CleanWin => Value("drops_clean_win", 0);
        public static int FirstWin => Value("drops_first_win", 10);
        public static int TripleExtra => Value("drops_triple_ad", 20);
        public static int DailyGift => Value("drops_daily", 15);
        /// <summary>The bonus "Hiểu ra" after a chapter's boss (Mrk 07/10: a large reward, paid once).</summary>
        public static int BonusDrops => Value("drops_bonus", 100);
        /// <summary>Each right answer in the bonus pays at once (Mrk 07/10): this × the question's number (10, 20, 30).</summary>
        public static int BonusRoundDrops => Value("drops_bonus_round", 10);
        public static int AdForDrops => Value("drops_ad_shop", 15);
        public static int AdForDropsDailyCap => Value("ad_shop_daily_cap", 3);
        /// <summary>Items up to this price can also be taken for one rewarded ad.</summary>
        public static int AdItemMaxPrice => Value("ad_item_max_price", 60);
        public static int InterstitialFromWin => Value("interstitial_from_win", 6);
        public static int InterstitialMinWins => Value("interstitial_min_wins", 2);
        public static int InterstitialMinSeconds => Value("interstitial_min_seconds", 90);
        public static int InterstitialSessionCap => Value("interstitial_session_cap", 4);
        public static int InterstitialDailyCap => Value("interstitial_daily_cap", 10);
        /// <summary>No full-screen ad this soon after a rewarded one.</summary>
        public static int RewardedQuietSeconds => Value("rewarded_quiet_seconds", 90);
        /// <summary>Seconds the ×3 button waits on the victory screen.</summary>
        public static float TripleWindow => Value("victory_triple_ms", 3500) / 1000f;

        /// <summary>Remote Config: replace a value (key names as in the plan, prices as price_ITEM_ID).</summary>
        public static void Set(string key, int value) { overrides[key] = value; }
        public static void ResetForTests() { overrides.Clear(); extraGifts.Clear(); Revision = "defaults"; Variant = "control"; }
        private static int Value(string key, int fallback) => overrides.TryGetValue(key, out int v) ? v : fallback;

        private static List<COgheShopItem> items;
        /// <summary>Everything that can be owned: 14 Home items, 12 inks, 8 hats, 6 inside things.</summary>
        public static IReadOnlyList<COgheShopItem> Items
        {
            get
            {
                if (items != null) return items;
                items = new List<COgheShopItem>(40);
                foreach (var e in COgheHomeItems.Catalog) items.Add(new COgheShopItem { Id = e.id, Name = e.name, Level = e.level, Group = COgheShopGroup.Home });
                foreach (var ink in COgheInks.Catalog) items.Add(new COgheShopItem { Id = ink.Id, Name = ink.Name, Level = ink.Level, Group = COgheShopGroup.Ink });
                foreach (var w in COgheWardrobe.Hats) items.Add(new COgheShopItem { Id = w.Id, Name = w.Name, Level = w.Level, Group = COgheShopGroup.Hat });
                foreach (var w in COgheWardrobe.InsideItems) items.Add(new COgheShopItem { Id = w.Id, Name = w.Name, Level = w.Level, Group = COgheShopGroup.Inside });
                return items;
            }
        }
        public static COgheShopItem Find(string id) { foreach (var i in Items) if (i.Id == id) return i; return null; }
        public static bool IsGift(string id) => System.Array.IndexOf(Gifts, id) >= 0 || extraGifts.Contains(id);

        /// <summary>The price in Drops: dearer the later it unlocks (Home and inks 40–120, hats 50–110, inside 60–120).</summary>
        public static int Price(string id)
        {
            if (IsGift(id)) return 0;
            if (overrides.TryGetValue("price_" + id, out int set)) return set;
            return DefaultPrice(id);
        }
        private static int DefaultPrice(string id)
        {
            var item = Find(id); if (item == null || System.Array.IndexOf(Gifts, id) >= 0) return 0;
            int low = 40, high = 120;
            if (item.Group == COgheShopGroup.Hat) { low = 50; high = 110; }
            if (item.Group == COgheShopGroup.Inside) { low = 60; high = 120; }
            float k = Mathf.InverseLerp(12, 60, item.Level);   // unlock levels ×1.2 for 60 levels (Mrk 06/10): same prices
            return Mathf.RoundToInt(Mathf.Lerp(low, high, k) / 5f) * 5;
        }
    }
}
