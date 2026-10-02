using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// The player's Drops and what they own (Mrk 02/10): Home items and Style items are bought with Drops once their level is
    /// reached; the ball and the first item of each Style group are gifts. Every reward has a key so it is paid once (a level
    /// won again, a callback repeated or the app closed half way never pays twice). Saved on the device (PlayerPrefs
    /// <c>coghe.shop.v1</c>), works offline; nothing here touches physics, puzzles or the look COghe wears.
    /// </summary>
    public static class COgheShop
    {
        [Serializable]
        public sealed class State
        {
            public int Version = 1;
            public int Drops;
            public List<string> Owned = new List<string>();
            /// <summary>Keys of rewards already paid (win:LEVEL_ID, triple:LEVEL_ID, gift:DAY…).</summary>
            public List<string> Paid = new List<string>();
            public string GiftDay = "", AdDay = "";
            public int AdsForDropsToday, InterstitialsToday;
            public bool Migrated;
        }

        private const string Key = "coghe.shop.v1";
        private static State state;
        /// <summary>Drops or ownership changed (counters and catalogs refresh).</summary>
        public static event Action Changed;
        /// <summary>Tests and previews that set <see cref="COgheHomeRoom.UnlockedLevelOverride"/> own what they unlock unless a
        /// shop test turns this off.</summary>
        public static bool TestsOwnUnlocked = true;
        /// <summary>Tests: today's date ("yyyy-MM-dd"), so the daily gift can be stepped.</summary>
        public static string TodayForTests;
        public static string Today => TodayForTests ?? DateTime.Now.ToString("yyyy-MM-dd");

        public static State Current
        {
            get
            {
                if (state != null) return state;
                state = new State();
                if (VenomCampaignSave.PersistenceEnabled)
                {
                    try { var loaded = JsonUtility.FromJson<State>(PlayerPrefs.GetString(Key, "")); if (loaded != null) state = loaded; }
                    catch (Exception e) { Debug.LogWarning("COghe shop save unreadable, starting fresh: " + e.Message); }
                }
                state.Owned ??= new List<string>(); state.Paid ??= new List<string>(); state.GiftDay ??= ""; state.AdDay ??= "";
                return state;
            }
        }
        public static void ResetForTests(State s = null) { state = s; TestsOwnUnlocked = true; TodayForTests = null; }
        private static void Save()
        {
            if (VenomCampaignSave.PersistenceEnabled) { PlayerPrefs.SetString(Key, JsonUtility.ToJson(Current)); PlayerPrefs.Save(); }
            Changed?.Invoke();
        }

        public static int Drops => Current.Drops;

        /// <summary>Owned: bought, a gift, part of COghe Plus, or (tests) unlocked.</summary>
        public static bool Owns(string id)
        {
            if (string.IsNullOrEmpty(id)) return true;
            if (COgheEconomy.IsGift(id)) return true;
            if (TestsOwnUnlocked && COgheHomeRoom.UnlockedLevelOverride.HasValue) return true;
            return Current.Owned.Contains(id);
        }

        /// <summary>Pay a reward once per <paramref name="key"/> (null: every time). Returns what was paid (0 if already).</summary>
        public static int Earn(string key, int amount, string source)
        {
            if (amount <= 0) return 0;
            if (key != null) { if (Current.Paid.Contains(key)) return 0; Current.Paid.Add(key); }
            Current.Drops += amount; Save();
            COgheAnalytics.Log("drops_earn", "source", source, "amount", amount, "balance", Current.Drops);
            return amount;
        }
        public static bool Paid(string key) => Current.Paid.Contains(key);

        /// <summary>Buy with Drops: false (nothing changes) if already owned or not enough.</summary>
        public static bool Buy(string id)
        {
            if (Owns(id)) return false;
            int price = COgheEconomy.Price(id);
            if (Current.Drops < price) return false;
            Current.Drops -= price; Current.Owned.Add(id); Save();
            COgheAnalytics.Log("item_buy", "item", id, "price", price, "currency", "drops", "balance", Current.Drops);
            COgheAnalytics.Log("drops_spend", "item", id, "amount", price, "balance", Current.Drops);
            return true;
        }
        /// <summary>Owned without paying Drops (a rewarded ad, a migration).</summary>
        public static void Grant(string id, string source)
        {
            if (Current.Owned.Contains(id)) return;
            Current.Owned.Add(id); Save();
            COgheAnalytics.Log("item_grant", "item", id, "source", source);
        }

        // The daily gift (Home) -------------------------------------------------------------------------------------------
        public static bool GiftReady => Current.GiftDay != Today;
        /// <summary>Claim today's gift (×<paramref name="multiplier"/>): what was paid, 0 if it was already claimed today.</summary>
        public static int ClaimGift(int multiplier)
        {
            if (!GiftReady) return 0;
            Current.GiftDay = Today;
            int paid = Earn("gift:" + Today, COgheEconomy.DailyGift * Mathf.Max(1, multiplier), "daily_gift");
            COgheAnalytics.Log("daily_gift", "multiplier", multiplier);
            return paid;
        }

        // Daily ad counters ------------------------------------------------------------------------------------------------
        private static void RollDay() { if (Current.AdDay != Today) { Current.AdDay = Today; Current.AdsForDropsToday = 0; Current.InterstitialsToday = 0; } }
        public static bool AdForDropsAvailable { get { RollDay(); return Current.AdsForDropsToday < COgheEconomy.AdForDropsDailyCap; } }
        public static int RewardAdForDrops()
        {
            RollDay(); if (Current.AdsForDropsToday >= COgheEconomy.AdForDropsDailyCap) return 0;
            Current.AdsForDropsToday++;
            return Earn(null, COgheEconomy.AdForDrops, "ad_shop");
        }
        public static int InterstitialsToday { get { RollDay(); return Current.InterstitialsToday; } }
        public static void NoteInterstitial() { RollDay(); Current.InterstitialsToday++; Save(); }

        /// <summary>
        /// A player from before the shop keeps everything their levels had already given them (nobody loses an item).
        /// Runs once, the first time the shop is used on a device with campaign progress.
        /// </summary>
        public static void Migrate(VenomCampaign game)
        {
            if (Current.Migrated || game == null || game.Progress == null) return;
            Current.Migrated = true;
            if (game.Progress.Completed.Count > 0 && !COgheHomeRoom.UnlockedLevelOverride.HasValue)
                foreach (var item in COgheEconomy.Items)
                    if (COgheHomeRoom.Reached(game, item.Level) && !Current.Owned.Contains(item.Id)) Current.Owned.Add(item.Id);
            Save();
        }
    }
}
