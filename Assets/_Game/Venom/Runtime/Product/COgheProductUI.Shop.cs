using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// Making money (Mrk 02/10, PLANS/COGHE_MONETIZATION_PLAN.md): the Drops counter, buying an item, the gifts and items a
    /// win unlocks, the daily gift, COghe Plus / No Ads, room for the banner on the menu, Home and the victory screen, and what
    /// happens after a win (+Drops, ×3 for an ad, the unlock popup, then the paced full-screen ad).
    /// </summary>
    public sealed partial class COgheProductUI
    {
        private Sprite dropSprite;
        private Text dropsLabel; private int dropsShown = -1;
        private string offerId; private System.Action offerBought;
        private float fullHeight; private bool bannerDirty;
        // the victory screen
        private readonly List<string> victoryUnlocks = new List<string>(6);
        private bool victoryHome, victoryAdChecked, tripleTaken; private float tripleUntil; private int victoryDrops;

        public int VictoryDrops => victoryDrops;
        public IReadOnlyList<string> VictoryUnlocks => victoryUnlocks;
        private bool BannerPage => Page == COgheProductPage.MainMenu || Page == COgheProductPage.Home || Page == COgheProductPage.Victory;

        private Sprite Drop
        {
            get
            {
                if (dropSprite != null) return dropSprite;
                var tex = Resources.Load<Texture2D>("COgheUI/Drop");
                if (tex != null) dropSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * .5f, 100);
                return dropSprite;
            }
        }
        private Image DropIcon(Transform parent, Rect r)
        {
            var im = new GameObject("Drop", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            var t = im.rectTransform; t.SetParent(parent, false); t.anchorMin = t.anchorMax = new Vector2(0, 1); t.pivot = new Vector2(0, 1);
            t.anchoredPosition = new Vector2(r.x, -r.y); t.sizeDelta = r.size; im.sprite = Drop; im.preserveAspect = true; im.raycastTarget = false;
            if (im.sprite == null) im.color = COgheUIArt.Ink;
            return im;
        }

        /// <summary>The Drops counter: a droplet and the balance; tapping it shows how to get more.</summary>
        private void DropsCounter(Transform parent, Rect r)
        {
            var box = art.Box(parent, "Drops", r, COgheUIArt.Paper, true); box.pixelsPerUnitMultiplier = 2.2f;
            var b = box.gameObject.AddComponent<Button>(); b.targetGraphic = box;
            b.onClick.AddListener(() => { COgheAudio.UiTap(); offerId = null; Popup = COgheProductPopup.Buy; Rebuild(); });
            DropIcon(box.transform, new Rect(8, (r.height - 26) * .5f, 26, 26));
            dropsLabel = art.Label(box.transform, "Balance", COgheShop.Drops.ToString(), new Rect(38, 0, r.width - 44, r.height), 15, null, TextAnchor.MiddleLeft);
            dropsLabel.font = art.BoldFont; dropsShown = COgheShop.Drops;
        }
        private void TickShop()
        {
            if (dropsLabel != null && dropsShown != COgheShop.Drops) { dropsShown = COgheShop.Drops; dropsLabel.text = dropsShown.ToString(); }
            if (bannerDirty && !injecting) { bannerDirty = false; Rebuild(); }
        }
        private bool placingBanner;
        private void OnBannerChanged() { if (!placingBanner) bannerDirty = true; }

        /// <summary>Called by Rebuild: the banner only on the menu, Home and the victory screen; their content stays clear of it.</summary>
        private void PlaceBanner()
        {
            placingBanner = true; COgheAds.Banner(BannerPage && Page != COgheProductPage.Intro); placingBanner = false;
            float inset = BannerPage ? COgheAds.BannerHeight / Mathf.Max(.01f, canvas.scaleFactor) : 0;
            height = fullHeight - (inset > 0 ? inset + 8 : 0);
        }

        // Buying ---------------------------------------------------------------------------------------------------------------
        /// <summary>Offer an item (Home, ink, hat, inside thing); <paramref name="bought"/> runs once it is owned.</summary>
        public void OfferItem(string id, System.Action bought)
        {
            offerId = id; offerBought = bought; Popup = COgheProductPopup.Buy; Rebuild();
        }
        private void FinishOffer(string id)
        {
            var then = offerBought; offerId = null; offerBought = null;
            Resume(); then?.Invoke();   // the Items menu had paused the room
        }
        /// <summary>A button whose icon is the droplet (prices in Drops).</summary>
        private Button DropButton(Transform parent, string name, Rect r, string label, System.Action click, bool primary = true)
        {
            var b = LabelButton(parent, name, r, COgheIcon.Check, label, click, primary);
            var glyph = b.transform.Find(nameof(COgheIcon.Check)).GetComponent<Image>();
            if (Drop != null) { glyph.sprite = Drop; glyph.color = Color.white; glyph.preserveAspect = true; }
            return b;
        }
        private Sprite IconOf(string id)
        {
            var item = COgheEconomy.Find(id);
            return item == null ? null : ItemIcon(id, item.Group == COgheShopGroup.Home ? "COgheHome/Icons/" : "COgheStyle/Icons/");
        }
        private Image ItemPicture(Transform parent, string id, Rect r, float alpha = 1)
        {
            var im = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            var t = im.rectTransform; t.SetParent(parent, false); t.anchorMin = t.anchorMax = new Vector2(0, 1); t.pivot = new Vector2(0, 1);
            t.anchoredPosition = new Vector2(r.x, -r.y); t.sizeDelta = r.size; im.sprite = IconOf(id); im.preserveAspect = true; im.raycastTarget = false;
            im.color = new Color(1, 1, 1, alpha); return im;
        }
        private static string GroupName(COgheShopGroup g) => g == COgheShopGroup.Home ? "For your Home" : g == COgheShopGroup.Ink ? "Ink" : g == COgheShopGroup.Hat ? "Hat" : "Floats inside";

        /// <summary>Buy an item, or (no item) the ways to get more Drops.</summary>
        private void BuyPopup(RectTransform panel, float w, float h)
        {
            ClosePopup(panel, w, () => { offerId = null; offerBought = null; Resume(); });
            var item = offerId != null ? COgheEconomy.Find(offerId) : null;
            float y = 24;
            if (item != null)
            {
                int price = COgheEconomy.Price(item.Id);
                ItemPicture(panel, item.Id, new Rect((w - 96) * .5f, y, 96, 96)); y += 100;
                art.Label(panel, "Title", item.Name, new Rect(24, y, w - 48, 30), 20).font = art.BoldFont; y += 30;
                art.Label(panel, "Group", GroupName(item.Group), new Rect(24, y, w - 48, 18), 12, COgheUIArt.Muted); y += 26;
                bool can = COgheShop.Drops >= price; string id = item.Id;
                var buy = DropButton(panel, "Buy item", new Rect(24, y, w - 48, 54), "Buy · " + price, () =>
                {
                    if (COgheShop.Buy(id)) { COgheAudio.Happy(); FinishOffer(id); }
                });
                if (!can) { buy.interactable = false; buy.GetComponent<Image>().color = new Color(.62f, .7f, .7f); }
                y += 62;
                if (!can) { art.Label(panel, "Short", "You have " + COgheShop.Drops + " · need " + (price - COgheShop.Drops) + " more", new Rect(24, y, w - 48, 18), 12, COgheUIArt.Muted); y += 24; }
                if (price <= COgheEconomy.AdItemMaxPrice && COgheAds.RewardedAvailable)
                {
                    LabelButton(panel, "Free with ad", new Rect(24, y, w - 48, 48), COgheIcon.Play, COgheEntitlements.InstantRewards ? "Free with Plus" : "Free · watch an ad", () =>
                        COgheAds.Rewarded("item", ok => { if (!ok) return; COgheShop.Grant(id, "rewarded"); COgheAudio.Happy(); FinishOffer(id); }));
                    y += 56;
                }
            }
            else { art.Label(panel, "Title", "Drops", new Rect(24, y + 4, w - 48, 34), 22).font = art.BoldFont; DropIcon(panel, new Rect(w * .5f - 70, y + 8, 26, 26)); y += 50;
                   art.Label(panel, "Balance", "You have " + COgheShop.Drops, new Rect(24, y, w - 48, 20), 14, COgheUIArt.Muted); y += 26;
                   art.Label(panel, "How", "Win levels for Drops. Each new level gives " + COgheEconomy.FirstWin * COgheEntitlements.DropsMultiplier + ".", new Rect(24, y, w - 48, 36), 12, COgheUIArt.Muted); y += 44; }
            if (COgheAds.RewardedAvailable && COgheShop.AdForDropsAvailable)
            {
                LabelButton(panel, "Drops for ad", new Rect(24, y, w - 48, 48), COgheIcon.Play, "+" + COgheEconomy.AdForDrops + " Drops · watch an ad", () =>
                    COgheAds.Rewarded("drops", ok => { if (ok) COgheShop.RewardAdForDrops(); Rebuild(); }));
                y += 56;
            }
            if (COgheEntitlements.StoreAvailable && !COgheEntitlements.HasPlus)
                LabelButton(panel, "Plus offer", new Rect(24, y, w - 48, 44), COgheIcon.Sparkles, "COghe Plus · ×2 Drops", () => ShowShopPopup(COgheProductPopup.Plus));
        }
        private float BuyPopupHeight()
        {
            var item = offerId != null ? COgheEconomy.Find(offerId) : null;
            float h = item != null ? 24 + 100 + 30 + 26 + 62 + 24 : 24 + 50 + 26 + 44 + 24;
            if (item != null && COgheShop.Drops < COgheEconomy.Price(item.Id)) h += 24;
            if (item != null && COgheEconomy.Price(item.Id) <= COgheEconomy.AdItemMaxPrice && COgheAds.RewardedAvailable) h += 56;
            if (COgheAds.RewardedAvailable && COgheShop.AdForDropsAvailable) h += 56;
            if (COgheEntitlements.StoreAvailable && !COgheEntitlements.HasPlus) h += 52;
            return h;
        }
        /// <summary>Open a shop popup (Buy without an item: ways to get Drops; Gift; Plus).</summary>
        public void ShowShopPopup(COgheProductPopup popup) { Popup = popup; Rebuild(); }

        private bool ShopPopup()
        {
            if (Popup != COgheProductPopup.Buy && Popup != COgheProductPopup.Unlocks && Popup != COgheProductPopup.Gift && Popup != COgheProductPopup.Plus) return false;
            float w = Mathf.Min(312, width - 40);
            float h = Popup == COgheProductPopup.Buy ? BuyPopupHeight() : Popup == COgheProductPopup.Unlocks ? UnlocksPopupHeight() : Popup == COgheProductPopup.Gift ? 310 : 380;
            h = Mathf.Min(h, height - 30);
            var panel = art.Box(popupRoot, "Panel", new Rect((width - w) * .5f, (height - h) * .5f, w, h), COgheUIArt.Paper, true).rectTransform;
            switch (Popup)
            {
                case COgheProductPopup.Buy: BuyPopup(panel, w, h); break;
                case COgheProductPopup.Unlocks: UnlocksPopup(panel, w, h); break;
                case COgheProductPopup.Gift: GiftPopup(panel, w, h); break;
                case COgheProductPopup.Plus: PlusPopup(panel, w, h); break;
            }
            return true;
        }

        // After a win ----------------------------------------------------------------------------------------------------------
        private string victoryLevel;
        /// <summary>Tests: the victory screen of level <paramref name="order"/> without playing it (pays, lists what it unlocks).</summary>
        public void ShowVictoryForTests(string levelId, int order) { Page = COgheProductPage.Victory; Popup = COgheProductPopup.None; BeginVictory(levelId, order); Rebuild(); }
        /// <summary>Tests: one step of what follows a win (the ×3 wait, the unlock popup, the paced ad).</summary>
        public bool VictoryStepForTests() => VictoryReady();
        private void BeginVictory() => BeginVictory(Game.Definition.Id, Game.Definition.Order);
        private void BeginVictory(string levelId, int order)
        {
            victoryLevel = levelId;
            string key = "win:" + levelId; bool first = !COgheShop.Paid(key);
            victoryDrops = COgheShop.Earn(key, COgheEconomy.FirstWin * COgheEntitlements.DropsMultiplier, "level_win");
            COgheAds.NoteWin();
            COgheAnalytics.Log("level_complete", "level", order, "first", first);
            victoryUnlocks.Clear(); victoryHome = false; victoryAdChecked = false; tripleTaken = false;
            if (first)
            {
                foreach (var item in COgheEconomy.Items) if (item.Level == order) { victoryUnlocks.Add(item.Id); COgheAnalytics.Log("item_unlock", "item", item.Id); }
                victoryHome = Game.Progress.RevealHome;
                if (victoryHome) COgheAnalytics.Log("feature_unlock", "feature", "home_style");
            }
            tripleUntil = victoryDrops > 0 && COgheAds.RewardedAvailable ? Time.unscaledTime + COgheEconomy.TripleWindow : 0;
        }
        /// <summary>Victory, every frame: wait for the ×3 offer and the unlock popup, then the paced ad, then the next level.</summary>
        private bool VictoryReady()
        {
            if (COgheAds.Showing || Time.unscaledTime < tripleUntil) return false;
            if (victoryUnlocks.Count > 0 || victoryHome) { Popup = COgheProductPopup.Unlocks; Rebuild(); return false; }
            if (!victoryAdChecked)
            {
                victoryAdChecked = true;
                if (COgheAds.TryInterstitial("level_end", Game.Progress.Completed.Count, null)) return false;
            }
            return true;
        }
        private void TripleButton()
        {
            if (victoryDrops <= 0 || !COgheAds.RewardedAvailable) return;
            float w = Mathf.Min(260, width - 80), y = height * .13f + 140;
            if (tripleTaken) { art.Label(pageRoot, "Tripled", "+" + (victoryDrops + COgheEconomy.TripleExtra) + " Drops", new Rect(20, y, width - 40, 30), 17, COgheUIArt.Teal); return; }
            if (Time.unscaledTime >= tripleUntil) return;
            var b = LabelButton(pageRoot, "Triple drops", new Rect((width - w) * .5f, y, w, 52), COgheIcon.Play, COgheEntitlements.InstantRewards ? "×3 Drops · Plus" : "×3 Drops · watch an ad", () =>
            {
                tripleUntil = Time.unscaledTime + 60;   // hold the screen while the ad plays
                COgheAds.Rewarded("victory_triple", ok =>
                {
                    if (ok && COgheShop.Earn("triple:" + victoryLevel, COgheEconomy.TripleExtra, "victory_triple") > 0) { tripleTaken = true; COgheAudio.Happy(); }
                    tripleUntil = 0; Rebuild();
                });
            }, true);
        }

        /// <summary>What this win unlocked: gifts are already yours, the rest can be bought (or taken for an ad) now or later.</summary>
        private void UnlocksPopup(RectTransform panel, float w, float h)
        {
            PopupTitle(panel, w, "New for COghe!");
            float y = 70;
            if (victoryHome)
            {
                art.Label(panel, "Home", "Home and Style are open: a room for COghe, and its own colors and hats.", new Rect(24, y, w - 48, 40), 13, COgheUIArt.Muted);
                y += 46;
            }
            int shown = Mathf.Min(4, victoryUnlocks.Count); float cell = (w - 28) / Mathf.Max(1, shown);
            for (int i = 0; i < shown; i++)
            {
                string id = victoryUnlocks[i]; var item = COgheEconomy.Find(id); if (item == null) continue;
                var r = new Rect(14 + i * cell + 3, y, cell - 6, 150);
                var box = art.Box(panel, "Unlock " + id, r, new Color(.995f, .995f, .972f)); box.pixelsPerUnitMultiplier = 2.4f;
                float size = Mathf.Min(64, cell - 18);
                ItemPicture(box.transform, id, new Rect((r.width - size) * .5f, 6, size, size));
                var name = art.Label(box.transform, "Name", item.Name, new Rect(3, size + 8, r.width - 6, 18), 12); name.font = art.BoldFont;
                name.resizeTextForBestFit = true; name.resizeTextMinSize = 8; name.resizeTextMaxSize = 12; name.verticalOverflow = VerticalWrapMode.Truncate;
                if (COgheShop.Owns(id)) art.Label(box.transform, "State", COgheEconomy.IsGift(id) ? "Gift ✓" : "Yours ✓", new Rect(3, size + 30, r.width - 6, 18), 12, COgheUIArt.Teal);
                else
                {
                    int price = COgheEconomy.Price(id);
                    var buy = art.Box(box.transform, "Buy " + id, new Rect(6, size + 32, r.width - 12, 40), COgheUIArt.Teal, true); buy.pixelsPerUnitMultiplier = 2f;
                    var button = buy.gameObject.AddComponent<Button>(); button.targetGraphic = buy;
                    button.onClick.AddListener(() => { COgheAudio.UiTap(); OfferItem(id, () => { Popup = COgheProductPopup.Unlocks; Rebuild(); }); });
                    DropIcon(buy.transform, new Rect(8, 9, 22, 22));
                    art.Label(buy.transform, "Price", price.ToString(), new Rect(30, 0, r.width - 46, 40), 15, COgheUIArt.Paper).font = art.BoldFont;
                }
            }
            float by = h - 78;
            if (victoryHome) { LabelButton(panel, "Visit Home now", new Rect(24, by - 60, w - 48, 52), COgheIcon.Home, "Visit Home", () => { victoryUnlocks.Clear(); victoryHome = false; Popup = COgheProductPopup.None; EnterHome(); }); }
            LabelButton(panel, "Continue", new Rect(24, by, w - 48, 54), COgheIcon.Forward, "Continue", () => { victoryUnlocks.Clear(); victoryHome = false; Resume(); }, true);
        }
        private float UnlocksPopupHeight() => 70 + (victoryHome ? 46 : 0) + (victoryUnlocks.Count > 0 ? 160 : 0) + (victoryHome ? 60 : 0) + 90;

        // Daily gift (Home) ----------------------------------------------------------------------------------------------------
        private void GiftButton(float y)
        {
            var b = art.Button(pageRoot, "Daily gift", new Rect(24, y, 48, 48), COgheIcon.Heart, () => ShowShopPopup(COgheProductPopup.Gift));
            b.transform.Find(nameof(COgheIcon.Heart)).gameObject.SetActive(false);
            DropIcon(b.transform, new Rect(10, 10, 28, 28));
            if (COgheShop.GiftReady) art.Box(b.transform, "Ready", new Rect(34, 2, 12, 12), new Color(.86f, .36f, .3f)).pixelsPerUnitMultiplier = 6;
        }
        private void GiftPopup(RectTransform panel, float w, float h)
        {
            PopupTitle(panel, w, "Daily gift"); ClosePopup(panel, w, Resume);
            DropIcon(panel, new Rect((w - 64) * .5f, 72, 64, 64));
            int amount = COgheEconomy.DailyGift * COgheEntitlements.DropsMultiplier;
            if (!COgheShop.GiftReady) { art.Label(panel, "Done", "Come back tomorrow for more Drops.", new Rect(24, 146, w - 48, 40), 14, COgheUIArt.Muted); return; }
            art.Label(panel, "Amount", "+" + amount + " Drops", new Rect(24, 142, w - 48, 30), 20).font = art.BoldFont;
            LabelButton(panel, "Claim gift", new Rect(24, 186, w - 48, 52), COgheIcon.Check, "Claim", () => { COgheShop.ClaimGift(COgheEntitlements.DropsMultiplier); COgheAudio.Happy(); Resume(); }, true);
            if (COgheAds.RewardedAvailable)
                LabelButton(panel, "Claim double", new Rect(24, 246, w - 48, 48), COgheIcon.Play, COgheEntitlements.InstantRewards ? "Claim ×2 · Plus" : "Claim ×2 · watch an ad",
                    () => COgheAds.Rewarded("daily_gift", ok => { COgheShop.ClaimGift(COgheEntitlements.DropsMultiplier * (ok ? 2 : 1)); COgheAudio.Happy(); Resume(); }));
        }

        // COghe Plus / No Ads --------------------------------------------------------------------------------------------------
        private void PlusPopup(RectTransform panel, float w, float h)
        {
            PopupTitle(panel, w, "COghe Plus"); ClosePopup(panel, w, Resume);
            var store = COgheEntitlements.Store; float y = 74;
            string[] perks = { "No ads, anywhere", "Rewards without watching ads", "×2 Drops from levels and gifts", "Call COghe's monster act at Home" };
            foreach (var perk in perks) { art.Icon(panel, COgheIcon.Check, new Rect(26, y + 2, 18, 18), COgheUIArt.Teal); art.Label(panel, "Perk", perk, new Rect(52, y, w - 76, 22), 13, null, TextAnchor.MiddleLeft); y += 28; }
            y += 8;
            if (COgheEntitlements.HasPlus) art.Label(panel, "Owned", "You have COghe Plus. Thank you!", new Rect(24, y, w - 48, 40), 15, COgheUIArt.Teal);
            else
            {
                LabelButton(panel, "Buy Plus", new Rect(24, y, w - 48, 54), COgheIcon.Sparkles, "Plus · " + store.Price(COgheEntitlements.Plus), () => COgheEntitlements.Buy(COgheEntitlements.Plus, ok => { if (ok) COgheAudio.Happy(); Rebuild(); }), true);
                y += 62;
                if (!COgheEntitlements.HasNoAds)
                    LabelButton(panel, "Buy No Ads", new Rect(24, y, w - 48, 48), COgheIcon.Close, "Only remove ads · " + store.Price(COgheEntitlements.NoAds), () => COgheEntitlements.Buy(COgheEntitlements.NoAds, ok => Rebuild()));
                else art.Label(panel, "NoAds", "Ads are off (No Ads).", new Rect(24, y, w - 48, 40), 13, COgheUIArt.Muted);
            }
            var restore = art.Label(panel, "Restore label", "Restore purchases", new Rect(24, h - 44, w - 48, 30), 12, COgheUIArt.Teal);
            var hit = art.Box(panel, "Restore purchases", new Rect(24, h - 48, w - 48, 40), Color.clear, true).gameObject.AddComponent<Button>();
            hit.onClick.AddListener(() => { COgheAudio.UiTap(); COgheEntitlements.Restore(ok => { Notify(ok ? "Purchases restored." : "Nothing to restore."); }); });
            restore.transform.SetAsLastSibling();
        }
        private void PlusButton()
        {
            if (!COgheEntitlements.StoreAvailable) return;
            LabelButton(pageRoot, "Plus", new Rect(24, 25, 92, 44), COgheIcon.Sparkles, COgheEntitlements.HasPlus ? "Plus ✓" : "Plus", () => ShowShopPopup(COgheProductPopup.Plus));
        }

        // Test builds: shop tools in Pause → Levels (test) ---------------------------------------------------------------------
        private float ShopTestTools(RectTransform panel, float w, float y)
        {
            float bw = (w - 48 - 12) / 3;
            LabelButton(panel, "Test ads", new Rect(24, y, bw, 40), COgheIcon.Play, COgheAds.TestAds ? "Ads on" : "Ads off", () => { COgheAds.TestAds = !COgheAds.TestAds; COgheAds.Banner(false); Rebuild(); });
            LabelButton(panel, "Test drops", new Rect(30 + bw, y, bw, 40), COgheIcon.Sparkles, "+500", () => { COgheShop.Earn(null, 500, "test"); Rebuild(); });
            LabelButton(panel, "Test reset shop", new Rect(36 + 2 * bw, y, bw, 40), COgheIcon.Restart, "Reset", () =>
            {
                if (VenomCampaignSave.PersistenceEnabled) { PlayerPrefs.DeleteKey("coghe.shop.v1"); PlayerPrefs.DeleteKey("coghe.entitlements.v1"); }
                COgheShop.ResetForTests(new COgheShop.State { Migrated = true }); COgheEntitlements.ResetForTests(); Rebuild();
            });
            return y + 48;
        }
    }
}
