using System;

namespace GravityBox.Venom
{
    /// <summary>Immutable offer captured before an ad. Its transaction survives scene/UI changes and config refreshes.</summary>
    public sealed class COgheReward
    {
        public enum Kind { Victory, Item, Daily, Shop }
        public Kind Type { get; }
        public string Key { get; }
        public string ItemId { get; }
        public string Day { get; }
        public int Amount { get; }
        public string Source => Type == Kind.Victory ? "victory_triple" : Type == Kind.Daily ? "daily_gift" : "ad_shop";
        private COgheReward(Kind type, string key, int amount, string item = null)
        { Type = type; Key = key; Amount = amount; ItemId = item; Day = COgheShop.Today; }

        public static COgheReward Victory(string levelId) => new(Kind.Victory, "triple:" + levelId, COgheEconomy.TripleExtra);
        public static COgheReward Item(string id) => new(Kind.Item, "reward:item:" + id, 0, id);
        // Plus doubles the base gift, not the fixed rewarded bonus: 30 + 15 = 45.
        public static COgheReward Daily() => new(Kind.Daily, "gift:" + COgheShop.Today,
            COgheEconomy.DailyGift * COgheEntitlements.DropsMultiplier + COgheEconomy.DailyGift);
        public static COgheReward Shop() => new(Kind.Shop, "reward:shop:" + Guid.NewGuid().ToString("N"), COgheEconomy.AdForDrops);
        public bool Available => !COgheShop.Paid(Key) && (Type != Kind.Shop || COgheShop.AdForDropsAvailable) &&
            (Type != Kind.Daily || COgheShop.GiftReady) &&
            (Type != Kind.Item || (COgheEconomy.Find(ItemId) != null && !COgheShop.Owns(ItemId) && COgheEconomy.Price(ItemId) <= COgheEconomy.AdItemMaxPrice));
    }
}
