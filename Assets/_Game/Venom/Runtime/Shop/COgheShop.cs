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
            public bool CommerceVisited;
            public List<string> Understood = new List<string>();
            // 0/1: choose a free ink; 2: hold on the body; 3: inspect the saved result; 4: finished.
            public int InkGuideStep;
            public int InkGiftRevision;
            public List<string> Retried = new List<string>();
            // Preserve unlock milestones from the orders before 15/10 and 19/10; completed puzzles still use stable IDs.
            public int PacingRevision;
            public List<int> ReachedBeforePacing = new List<int>();
            // Ability cards (Mrk 16/10/2026): one card per "Hiểu ra", each stat 0–10; the chapters whose card is taken.
            public int Emotion, Agility, Intelligence;
            public int LastAbility = -1;
            public List<int> CardsPicked = new List<int>();
            public int UnderstandPacingRevision;
            public List<int> SkippedPractices = new List<int>();
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
                state.Retried ??= new List<string>(); state.Understood ??= new List<string>();
                state.ReachedBeforePacing ??= new List<int>();
                state.CardsPicked ??= new List<int>();
                state.SkippedPractices ??= new List<int>();
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

        /// <summary>Mint replaces the free Ocean lesson without removing an existing player's Ocean.</summary>
        public static void MigrateInkGift(bool styleUnlocked)
        {
            if (Current.InkGiftRevision >= 1) return;
            if (styleUnlocked || Current.InkGuideStep > 0)
            {
                if (!Current.Owned.Contains("INK_OCEAN")) Current.Owned.Add("INK_OCEAN");
                if (Current.InkGuideStep > 0 && Current.InkGuideStep < 4) Current.InkGuideStep = 1;
            }
            Current.InkGiftRevision = 1; Save();
        }

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
        public static bool Understood(string key) => Paid(key) || Current.Understood.Contains(key);
        public static void Understand(string key)
        { if (!Current.Understood.Contains(key)) { Current.Understood.Add(key); Save(); } }
        /// <summary>Introducing the six mid-chapter practices never forces a backlog on an existing player.
        /// Skipped is separate from understood: no retroactive cards, toys or Drops. Boss checkpoints are untouched.</summary>
        public static void MigrateUnderstand(COgheProductCatalog catalog, VenomCampaignSave progress)
        {
            if (catalog == null || progress == null || Current.UnderstandPacingRevision >= 1) return;
            foreach (var m in COgheBonus.Milestones(catalog))
                if (COgheBonus.IsPractice(m.id) && progress.Completed.Contains(catalog.Levels[m.order - 1].Id)
                    && !COgheBonus.Done(m.id) && COgheBonus.CompletedRounds(m.id) == 0 && !Current.SkippedPractices.Contains(m.id))
                    Current.SkippedPractices.Add(m.id);
            Current.UnderstandPacingRevision = 1; Save();
        }
        /// <summary>Takes chapter <paramref name="chapter"/>'s ability card: +1 to <paramref name="ability"/> (to
        /// <see cref="COgheAbilities.Max"/>), once per chapter. False if that card was taken or the ability is full.</summary>
        public static bool PickAbility(int chapter, COgheAbility ability)
        {
            if (Current.CardsPicked.Contains(chapter) || COgheAbilities.Level(ability) >= COgheAbilities.Max) return false;
            Current.CardsPicked.Add(chapter);
            if (ability == COgheAbility.Emotion) Current.Emotion++;
            else if (ability == COgheAbility.Agility) Current.Agility++;
            else Current.Intelligence++;
            Current.LastAbility = (int)ability;
            Save();
            COgheAnalytics.Log("ability_pick", "chapter", chapter, "ability", ability.ToString(), "level", COgheAbilities.Level(ability));
            return true;
        }
        public static void SetInkGuideStep(int step)
        { if (Current.InkGuideStep != step) { Current.InkGuideStep = Mathf.Clamp(step, 0, 4); Save(); } }

        public static void VisitCommerce() { if (Current.CommerceVisited) return; Current.CommerceVisited = true; Save(); }
        public static void NoteRetry(string levelId) { if (Current.Retried.Contains(levelId)) return; Current.Retried.Add(levelId); Save(); }
        public static int WinReward(string levelId) => (COgheEconomy.FirstWin +
            (Current.Retried.Contains(levelId) ? 0 : COgheEconomy.CleanWin)) * COgheEntitlements.DropsMultiplier;

        /// <summary>Commit the entire earned reward in one save, immediately at the SDK earned callback.
        /// UI dismissal only refreshes presentation. Repeated callbacks/reloaded state cannot pay it twice.</summary>
        public static bool ApplyReward(COgheReward reward)
        {
            if (reward == null || Paid(reward.Key)) return false;
            if (reward.Type == COgheReward.Kind.Item)
            {
                if (COgheEconomy.Find(reward.ItemId) == null) return false;
                if (!Current.Owned.Contains(reward.ItemId)) Current.Owned.Add(reward.ItemId);
            }
            else if (reward.Type == COgheReward.Kind.Daily)
            {
                if (string.CompareOrdinal(Current.GiftDay, reward.Day) < 0) Current.GiftDay = reward.Day;
            }
            else if (reward.Type == COgheReward.Kind.Shop)
            {
                // An ad spanning midnight is charged to its offer day; it does not consume tomorrow's cap.
                if (string.CompareOrdinal(Current.AdDay, reward.Day) < 0)
                { Current.AdDay = reward.Day; Current.AdsForDropsToday = 0; Current.InterstitialsToday = 0; }
                if (Current.AdDay == reward.Day) Current.AdsForDropsToday++;
            }
            Current.Paid.Add(reward.Key); Current.Drops += reward.Amount; Save();
            if (reward.Type == COgheReward.Kind.Hint) { COgheAnalytics.Log("hint_unlock", "level_id", reward.Key.Substring(5)); return true; }
            if (reward.Type == COgheReward.Kind.Item) COgheAnalytics.Log("item_grant", "item", reward.ItemId, "source", "rewarded");
            else COgheAnalytics.Log("drops_earn", "source", reward.Source, "amount", reward.Amount, "balance", Current.Drops);
            if (reward.Type == COgheReward.Kind.Daily) COgheAnalytics.Log("daily_gift", "amount", reward.Amount, "rewarded", 1);
            return true;
        }

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
            if (game == null || game.Progress == null) return;
            if (Current.PacingRevision < COgheProgressionMilestones.LatestRevision)
            {
                for (int revision = Current.PacingRevision + 1; revision <= COgheProgressionMilestones.LatestRevision; revision++)
                    COgheProgressionMilestones.Capture(game, Current.ReachedBeforePacing, revision);
                Current.PacingRevision = COgheProgressionMilestones.LatestRevision; Save();
            }
            if (Current.Migrated) return;
            Current.Migrated = true;
            if (game.Progress.Completed.Count > 0 && !COgheHomeRoom.UnlockedLevelOverride.HasValue)
                foreach (var item in COgheEconomy.Items)
                    if (COgheHomeRoom.Reached(game, item.Level) && !Current.Owned.Contains(item.Id)) Current.Owned.Add(item.Id);
            Save();
        }
    }
}
