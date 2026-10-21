using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// The Food tray (Mrk 21/10/2026: snacks "phải bố trí UI hợp lý"): Home's Feed opens a tray low on the screen with
    /// COghe's energy bar, the steel balls (free, as before) and the five snacks, each with its picture, name, taste, the
    /// energy it gives and its price in Drops, so the player sees what it does before paying. A snack is paid and eaten at
    /// once; short of Drops, the ways to get more open instead. The bottom row keeps its four buttons (a fifth does not
    /// fit a 360-wide phone).
    /// </summary>
    public sealed partial class COgheProductUI
    {
        public const string FeedBallsButton = "Feed balls";
        public static string SnackButton(string id) => "Snack " + id;

        private bool FoodPopup()
        {
            if (Popup != COgheProductPopup.Food) return false;
            float w = Mathf.Min(336, width - 24), cw = (w - 48 - 16) / 3, ch = 116, top = 112, h = top + ch * 2 + 8 + 22;
            var panel = art.Box(popupRoot, "Panel", new Rect((width - w) * .5f, height - h - 28, w, h), COgheUIArt.Paper, true).rectTransform;
            PopupTitle(panel, w, "Food");
            ClosePopup(panel, w, Resume);
            // COghe's energy, with what it means right now
            EnergyMeter(panel, new Rect(24, 70, 120, 34), null);
            string mood = COgheEnergy.State == COgheHunger.Exhausted ? "Exhausted: worn out" : COgheEnergy.Hungry ? "Hungry: no toys" : COgheEnergy.Value >= COgheEnergy.Max ? "Full of energy" : "Not hungry";
            art.Label(panel, "Energy note", mood, new Rect(152, 70, w - 176, 34), 11, COgheUIArt.Muted, TextAnchor.MiddleLeft);
            for (int i = 0; i <= COgheSnacks.Catalog.Length; i++)
            {
                var r = new Rect(24 + (i % 3) * (cw + 8), top + (i / 3) * (ch + 8), cw, ch);
                if (i == 0) FoodCard(panel, r, FeedBallsButton, null, "Steel balls", "Up to " + COgheEnergy.BallCap, COgheEnergy.PerBall * COgheFeedBalls.PerFeed, 0, FeedBalls);
                else
                {
                    var snack = COgheSnacks.Catalog[i - 1];
                    FoodCard(panel, r, SnackButton(snack.Id), ItemIcon(snack.Id, "COgheHome/Icons/"), snack.Name, snack.Taste, snack.Energy, snack.Price, () => BuySnack(snack));
                }
            }
            return true;
        }

        private void FoodCard(RectTransform panel, Rect r, string name, Sprite icon, string title, string note, int energy, int price, System.Action tap)
        {
            var card = art.Box(panel, name, r, new Color(.995f, .995f, .972f), true); card.pixelsPerUnitMultiplier = 2;
            var button = card.gameObject.AddComponent<Button>(); button.targetGraphic = card;
            button.onClick.AddListener(() => { COgheAudio.UiTap(); tap(); });
            var at = new Rect((r.width - 52) * .5f, 6, 52, 52);
            if (price == 0) art.Icon(card.transform, COgheIcon.Food, new Rect(at.x + 8, at.y + 8, 36, 36), COgheUIArt.Teal);   // the steel balls
            else
            {
                var picture = art.Rect(card.transform, "Picture", at).gameObject.AddComponent<Image>();
                picture.sprite = icon; picture.preserveAspect = true; picture.raycastTarget = false; if (icon == null) picture.color = COgheUIArt.Mint;
            }
            art.Label(card.transform, "Name", title, new Rect(4, 58, r.width - 8, 20), 11, COgheUIArt.Ink).font = art.BoldFont;
            art.Label(card.transform, "Taste", note, new Rect(4, 76, r.width - 8, 16), 10, COgheUIArt.Muted);
            // what it gives (energy) on the left, what it costs on the right
            art.Icon(card.transform, COgheIcon.Zap, new Rect(5, 95, 14, 14), EnergyTint(COgheHunger.Fed));
            art.Label(card.transform, "Energy", "+" + energy, new Rect(19, 92, r.width * .5f - 19, 20), 11, COgheUIArt.Ink, TextAnchor.MiddleLeft).font = art.BoldFont;
            if (price == 0) { art.Label(card.transform, "Price", "Free", new Rect(r.width * .5f, 92, r.width * .5f - 6, 20), 11, COgheUIArt.Teal, TextAnchor.MiddleRight).font = art.BoldFont; return; }
            DropIcon(card.transform, new Rect(r.width - 36, 95, 14, 14));
            art.Label(card.transform, "Price", price.ToString(), new Rect(r.width - 22, 92, 20, 20), 11, COgheUIArt.Ink, TextAnchor.MiddleLeft).font = art.BoldFont;
        }

        private void FeedBalls()
        {
            Resume(); Game.FeedHome(); COgheAudio.Happy();
            if (COgheEnergy.Value >= COgheEnergy.BallCap) Notify("Steel balls fill COghe up to " + COgheEnergy.BallCap + ". Snacks give more!");
        }

        /// <summary>Pay and eat at once; short of Drops, the ways to get more; COghe busy (the bonus, the wardrobe): not now.</summary>
        private void BuySnack(COgheSnacks.Snack snack)
        {
            var p = Game.Personality;
            if (p == null || !p.CanEatSnack) { Notify("COghe is busy. Try again in a moment."); return; }
            if (!COgheShop.Spend(snack.Price, "snack", snack.Id)) { OfferItem(null, null); return; }   // ways to get Drops
            Resume(); p.EatSnack(snack);
            COgheAnalytics.Log("snack_eat", "snack", snack.Id, "price", snack.Price, "energy", COgheEnergy.Value, "balance", COgheShop.Drops);
        }
    }
}
