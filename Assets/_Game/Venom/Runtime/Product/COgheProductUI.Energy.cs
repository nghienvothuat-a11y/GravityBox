using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// COghe's energy on screen (COgheEnergy, Mrk 21/10/2026): a bolt and a bar at Home, under Back, across from the
    /// Drops (green when fed, orange when hungry, red when exhausted; a tap opens the Food tray). A win that leaves COghe
    /// hungry offers "Hungry · Feed": the usual after-win steps (unlocks, the bonus, an ad) run, then Home opens on the
    /// Food tray instead of the next level. A toy turned down by a hungry COghe says why.
    /// </summary>
    public sealed partial class COgheProductUI
    {
        public const string EnergyMeterName = "Energy", FeedAfterVictoryButton = "Feed at Home";
        private RectTransform energyFill; private Image energyBolt; private float energyTrack; private int energyShown = -1;
        private bool feedAfterVictory;

        public static Color EnergyTint(COgheHunger state) =>
            state == COgheHunger.Exhausted ? new Color(.84f, .30f, .26f) : state == COgheHunger.Hungry ? new Color(.93f, .58f, .18f) : new Color(.30f, .66f, .42f);

        private void EnergyMeter(Transform parent, Rect r, System.Action tap)
        {
            var box = art.Box(parent, EnergyMeterName, r, COgheUIArt.Paper, true); box.pixelsPerUnitMultiplier = 2.2f;
            if (tap != null)
            {
                var b = box.gameObject.AddComponent<Button>(); b.targetGraphic = box;
                b.onClick.AddListener(() => { COgheAudio.UiTap(); tap(); });
            }
            energyBolt = art.Icon(box.transform, COgheIcon.Zap, new Rect(8, (r.height - 24) * .5f, 24, 24));
            energyTrack = r.width - 50;
            var track = art.Box(box.transform, "Track", new Rect(38, (r.height - 10) * .5f, energyTrack, 10), new Color(.86f, .87f, .83f));
            track.pixelsPerUnitMultiplier = 6; track.raycastTarget = false;
            var fill = art.Box(track.transform, "Fill", new Rect(0, 0, energyTrack, 10), Color.white);
            fill.pixelsPerUnitMultiplier = 6; fill.raycastTarget = false;
            energyFill = fill.rectTransform; energyShown = -1; TickEnergy();
        }

        /// <summary>Every frame: the bar follows COghe's energy (a meal fills it ball by ball).</summary>
        private void TickEnergy()
        {
            if (energyFill == null || energyShown == COgheEnergy.Value) return;
            energyShown = COgheEnergy.Value;
            var tint = EnergyTint(COgheEnergy.State);
            energyFill.sizeDelta = new Vector2(Mathf.Max(10, energyTrack * energyShown / COgheEnergy.Max), energyFill.sizeDelta.y);
            energyFill.GetComponent<Image>().color = tint;
            if (energyBolt != null) energyBolt.color = tint;
        }

        /// <summary>Victory, Home open and COghe hungry: the way to its food, top left (the Drops are top right).</summary>
        private void HungryVictoryButton()
        {
            if (!Game.Progress.HomeUnlocked || !COgheEnergy.Hungry) return;
            var b = LabelButton(pageRoot, FeedAfterVictoryButton, new Rect(24, 25, Mathf.Min(156, width - 172), 44), COgheIcon.Food, "Hungry · Feed", FeedAfterVictory, false, 13);
            b.interactable = !victoryNextRequested;
            var glyph = b.transform.Find(COgheIcon.Food.ToString())?.GetComponent<Image>();
            if (glyph != null) glyph.color = EnergyTint(COgheEnergy.State);
        }

        /// <summary>The after-win steps as for Next, then Home with the Food tray open.</summary>
        private void FeedAfterVictory()
        {
            if (victoryNextRequested) return;
            feedAfterVictory = true; NextLevel();
            COgheAnalytics.Log("energy_feed_prompt", "level", victoryLevel, "value", COgheEnergy.Value);
        }

        /// <summary>Where the victory goes once its steps are done: Home on the Food tray, if the player asked for it.</summary>
        private bool FeedAfterVictoryReady()
        {
            if (!feedAfterVictory) return false;
            feedAfterVictory = false;
            EnterHome(); ShowPopup(COgheProductPopup.Food);
            return true;
        }
    }
}
