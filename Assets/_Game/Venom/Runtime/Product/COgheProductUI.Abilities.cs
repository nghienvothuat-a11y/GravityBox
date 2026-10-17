using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// Ability cards (Mrk 16/10/2026, <see cref="COgheAbilities"/>). After each "Understand COghe" the player takes one of
    /// three cards; Home's Ability button (where Play, a cuddle, used to be) shows the three abilities as bars. Cards say
    /// what grows, not by how much (Mrk: "rõ hướng, giấu chi tiết"): Emotion's tricks are found at Home, Agility is felt,
    /// and Intelligence says once how it works (leave COghe alone for 5 s).
    /// </summary>
    public sealed partial class COgheProductUI
    {
        private int abilityChapter, abilitySelected = -1;
        private bool abilityChosen, abilityContinue, abilityFirstIntelligence;
        private static readonly COgheAbility[] AbilityOrder = { COgheAbility.Emotion, COgheAbility.Agility, COgheAbility.Intelligence };
        private static readonly Sprite[] abilityArt = new Sprite[3];

        // Friendly display names only: serialized ability IDs and progression remain unchanged.
        private static string AbilityName(COgheAbility a) =>
            a == COgheAbility.Emotion ? "Playful" : a == COgheAbility.Agility ? "Speedy" : "Clever";
        private static Color AbilityColor(COgheAbility a) =>
            a == COgheAbility.Emotion ? new Color(.69f, .34f, .32f) : a == COgheAbility.Agility ? new Color(.56f, .39f, .13f) : new Color(.20f, .43f, .48f);
        private static Color AbilityTint(COgheAbility a) =>
            a == COgheAbility.Emotion ? new Color(.98f, .89f, .86f) : a == COgheAbility.Agility ? new Color(.98f, .93f, .80f) : new Color(.85f, .93f, .92f);
        private static COgheIcon AbilityIcon(COgheAbility a) => a == COgheAbility.Emotion ? COgheIcon.Heart : a == COgheAbility.Agility ? COgheIcon.Zap : COgheIcon.Brain;
        private static string AbilityGain(COgheAbility a) =>
            a == COgheAbility.Emotion ? "Learn new tricks" : a == COgheAbility.Agility ? "Move faster" : "Remember more";
        private static string AbilityHint(COgheAbility a) =>
            a == COgheAbility.Emotion ? "Play together at Home" : a == COgheAbility.Agility ? "Crawl and climb faster" : "Find the next device";
        private static string AbilityNow(COgheAbility a)
        {
            int level = COgheAbilities.Level(a);
            if (a == COgheAbility.Emotion) return level == 0 ? "New tricks to learn" : "Discover tricks together";
            if (a == COgheAbility.Agility) return level == 0 ? "COghe's usual speed" : "Faster crawling and climbing";
            return level == 0 ? "COghe waits for your tap" : "Remembers where to go next";
        }
        private static Sprite AbilityArt(COgheAbility a)
        {
            int i = (int)a;
            if (abilityArt[i] == null)
            {
                var tex = Resources.Load<Texture2D>("COgheUI/Abilities/" + a);
                if (tex != null) abilityArt[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, 0), 100);
            }
            return abilityArt[i];
        }

        /// <summary>Ability cards: the chapter whose card is being chosen (0: none).</summary>
        public int AbilityChapter => Popup == COgheProductPopup.AbilityPick ? abilityChapter : 0;
        /// <summary>Tests: tap card <paramref name="index"/> (Emotion, Agility, Intelligence) and confirm it.</summary>
        public bool ChooseAbilityForTests(int index) { SelectAbility(index); return ConfirmAbility(); }

        /// <summary>Opens the card choice for <paramref name="chapter"/>; <paramref name="continueAfter"/>: go on to the next
        /// level afterwards (straight from the victory's bonus).</summary>
        private void OpenAbilityPick(int chapter, bool continueAfter)
        {
            abilityChapter = chapter; abilityContinue = continueAfter; abilitySelected = -1; abilityChosen = false;
            COgheAnalytics.Log("ability_offer", "chapter", chapter);
            ShowPopup(COgheProductPopup.AbilityPick);
        }
        private void SelectAbility(int index)
        {
            if (abilityChosen || index < 0 || index > 2 || COgheAbilities.Full(AbilityOrder[index])) return;
            abilitySelected = index; Rebuild();
        }
        private bool ConfirmAbility()
        {
            if (abilityChosen || abilitySelected < 0) return false;
            var ability = AbilityOrder[abilitySelected];
            if (!COgheShop.PickAbility(abilityChapter, ability)) return false;
            abilityChosen = true;
            abilityFirstIntelligence = ability == COgheAbility.Intelligence && COgheAbilities.Level(ability) == 1;
            if (ability == COgheAbility.Agility) Game.Motion.SpeedScale = COgheAbilities.SpeedScale;
            Game.Personality?.Force(COgheAct.Shape, ability == COgheAbility.Emotion ? COgheShape.Heart : ability == COgheAbility.Agility ? COgheShape.Rocket : COgheShape.Star);
            COgheAudio.Happy();
            Rebuild();
            return true;
        }
        private void FinishAbilityPick()
        {
            bool continueAfter = abilityContinue; abilityChapter = 0; abilitySelected = -1; abilityChosen = false; abilityContinue = false;
            Popup = COgheProductPopup.None; SetPaused(false);
            int next = COgheAbilities.Pending();
            if (next > 0) { OpenAbilityPick(next, continueAfter); return; }   // an older card left waiting
            if (continueAfter) ContinueAfterBonus(); else Rebuild();
        }

        private bool AbilityPopup()
        {
            if (Popup == COgheProductPopup.AbilityPick) { AbilityPickPanel(); return true; }
            if (Popup == COgheProductPopup.AbilityStats) { AbilityStatsPanel(); return true; }
            return false;
        }

        private RectTransform AbilityPanel(float w, float h)
        {
            var panel = art.Box(popupRoot, "Panel", new Rect((width - w) * .5f, (height - h) * .5f, w, h), COgheUIArt.Paper, true);
            panel.pixelsPerUnitMultiplier = 1.5f;
            return panel.rectTransform;
        }

        private void AbilityPickPanel()
        {
            float w = Mathf.Min(abilityChosen ? 400 : 480, width - 24), h = abilityChosen ? 538 : 544;
            var panel = AbilityPanel(w, h);
            if (abilityChosen) { AbilityChosenPanel(panel, w, h); return; }
            art.Label(panel, "Title", "Help COghe grow!", new Rect(18, 22, w - 36, 34), 24);
            art.Label(panel, "Message", "Pick one new power", new Rect(18, 58, w - 36, 24), 15, COgheUIArt.Muted);
            // Equal columns stay visible together, including on the narrowest supported phone.
            // No default choice, featured centre card, or size/position change on selection.
            float cardWidth = (w - 44) / 3;
            for (int i = 0; i < AbilityOrder.Length; i++) AbilityCard(panel, i, new Rect(14 + i * (cardWidth + 8), 98, cardWidth, 286));
            art.Label(panel, "Card detail", abilitySelected >= 0 ? AbilityHint(AbilityOrder[abilitySelected]) : "Pick a card, then confirm.",
                new Rect(20, 402, w - 40, 48), 16, COgheUIArt.Muted);
            if (abilitySelected >= 0)
            {
                var a = AbilityOrder[abilitySelected];
                LabelButton(panel, "Choose ability", new Rect(20, 470, w - 40, 54), COgheIcon.Check,
                    "Choose " + AbilityName(a), () => ConfirmAbility(), true, 17);
            }
            else
            {
                art.Box(panel, "Choose hint box", new Rect(20, 470, w - 40, 54), COgheUIArt.Mint);
                art.Icon(panel, COgheIcon.Tap, new Rect(w * .5f - 82, 485, 24, 24), COgheUIArt.Teal);
                art.Label(panel, "Choose hint", "Tap your favorite", new Rect(w * .5f - 50, 470, 148, 54), 16, COgheUIArt.Teal, TextAnchor.MiddleLeft);
            }
        }

        private void AbilityPicture(Transform parent, COgheAbility ability, Rect bounds, bool full = false)
        {
            var sprite = AbilityArt(ability);
            if (sprite == null) { art.Icon(parent, AbilityIcon(ability), bounds, AbilityColor(ability)); return; }
            var im = art.Rect(parent, "COghe illustration", bounds).gameObject.AddComponent<Image>();
            im.sprite = sprite; im.preserveAspect = true; im.raycastTarget = false;
            if (full) im.color = new Color(1, 1, 1, .65f);
        }

        private void AbilityCard(Transform panel, int index, Rect r)
        {
            var a = AbilityOrder[index]; bool full = COgheAbilities.Full(a), selected = index == abilitySelected;
            Color color = AbilityColor(a); int level = COgheAbilities.Level(a);
            // Selection does not move the hit target under a child's finger. A tick + outline work without colour.
            art.Box(panel, a + " shadow", new Rect(r.x, r.y + 3, r.width, r.height), new Color(.83f, .86f, .82f)).pixelsPerUnitMultiplier = 2;
            if (selected) art.Box(panel, a + " ring", new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), color).pixelsPerUnitMultiplier = 2;
            var card = art.Box(panel, a + " card", r, Color.white, true); card.pixelsPerUnitMultiplier = 2;
            var button = card.gameObject.AddComponent<Button>(); button.targetGraphic = card; button.interactable = !full;
            button.onClick.AddListener(() => { COgheAudio.UiTap(); SelectAbility(index); });
            var t = card.transform;
            art.Box(t, "Art", new Rect(6, 6, r.width - 12, 120), AbilityTint(a)).pixelsPerUnitMultiplier = 2.5f;
            float pictureSize = Mathf.Min(r.width - 12, 116);
            AbilityPicture(t, a, new Rect((r.width - pictureSize) * .5f, 66 - pictureSize * .5f, pictureSize, pictureSize), full);
            art.Icon(t, selected ? COgheIcon.Check : full ? COgheIcon.Check : AbilityIcon(a), new Rect(r.width * .5f - 11, 136, 22, 22), color).name = selected ? "Selected check" : full ? "Max check" : "Ability symbol";
            art.Label(t, "Name", AbilityName(a), new Rect(5, 163, r.width - 10, 27), 17, COgheUIArt.Ink).font = art.BoldFont;
            art.Label(t, "Gain", full ? "All learned!" : AbilityGain(a), new Rect(7, 195, r.width - 14, 46), 14, COgheUIArt.Ink);
            art.Label(t, "Level", full ? "MAX · Lv 10" : "Lv " + level + " → " + (level + 1), new Rect(5, 252, r.width - 10, 22), 13, color).font = art.BoldFont;
        }

        private void AbilityChosenPanel(RectTransform panel, float w, float h)
        {
            var a = AbilityOrder[abilitySelected]; Color color = AbilityColor(a); int level = COgheAbilities.Level(a);
            art.Label(panel, "Title", "Nice choice!", new Rect(20, 22, w - 40, 36), 25);
            art.Label(panel, "Ability level", AbilityName(a) + " · Lv " + level, new Rect(20, 61, w - 40, 25), 18, color).font = art.BoldFont;
            art.Box(panel, "Art", new Rect((w - 210) * .5f, 98, 210, 192), AbilityTint(a)).pixelsPerUnitMultiplier = 1.6f;
            AbilityPicture(panel, a, new Rect((w - 190) * .5f, 100, 190, 188));
            art.Box(panel, "Learned badge", new Rect(w * .5f + 69, 263, 36, 36), color).pixelsPerUnitMultiplier = 2;
            art.Icon(panel, COgheIcon.Check, new Rect(w * .5f + 75, 269, 24, 24), Color.white);
            string message = a == COgheAbility.Emotion ? "2 new tricks to discover!" : a == COgheAbility.Agility ? "COghe moves faster now!" : "COghe remembers one more step!";
            art.Label(panel, "Message", message, new Rect(20, 310, w - 40, 50), 18).font = art.BoldFont;
            if (abilityFirstIntelligence)
            {
                // Show both parts of the rule; never imply that memory operates the device or solves the puzzle.
                art.Box(panel, "Tip", new Rect(16, 370, w - 32, 78), AbilityTint(a)).pixelsPerUnitMultiplier = 2;
                art.Icon(panel, COgheIcon.Brain, new Rect(28, 379, 24, 24), color);
                art.Label(panel, "Tip text", "Wait 5 seconds. COghe finds the next device.", new Rect(62, 375, w - 88, 36), 14, COgheUIArt.Ink, TextAnchor.MiddleLeft);
                art.Icon(panel, COgheIcon.Tap, new Rect(28, 416, 24, 24), color);
                art.Label(panel, "Your turn", "Then tap it to use it.", new Rect(62, 414, w - 88, 28), 14, COgheUIArt.Ink, TextAnchor.MiddleLeft);
            }
            else art.Label(panel, "Hint", a == COgheAbility.Emotion ? "Play together at Home." : AbilityHint(a), new Rect(24, 376, w - 48, 44), 16, COgheUIArt.Muted);
            LabelButton(panel, "Ability continue", new Rect(20, h - 72, w - 40, 54), COgheIcon.Forward, "Let's go!", FinishAbilityPick, true, 18);
        }

        private void AbilityStatsPanel()
        {
            int pending = COgheAbilities.Pending();
            float w = Mathf.Min(400, width - 24), h = 570;
            var panel = AbilityPanel(w, h);
            art.Label(panel, "Title", "COghe's powers", new Rect(20, 22, w - 82, 36), 23, COgheUIArt.Ink, TextAnchor.MiddleLeft);
            ClosePopup(panel, w, Resume);
            art.Label(panel, "Message", "Look how COghe has grown!", new Rect(20, 65, w - 40, 24), 15, COgheUIArt.Muted);
            for (int i = 0; i < AbilityOrder.Length; i++)
            {
                var a = AbilityOrder[i]; Color color = AbilityColor(a); int level = COgheAbilities.Level(a);
                float y = 105 + i * 126;
                art.Box(panel, a + " row", new Rect(14, y, w - 28, 114), Color.white).pixelsPerUnitMultiplier = 2;
                art.Box(panel, a + " art well", new Rect(20, y + 6, 90, 102), AbilityTint(a)).pixelsPerUnitMultiplier = 2;
                AbilityPicture(panel, a, new Rect(21, y + 8, 88, 98));
                float x = 122, bw = w - x - 26, seg = (bw - 3 * 9) / 10;
                art.Label(panel, a + " name", AbilityName(a), new Rect(x, y + 7, bw, 26), 20, COgheUIArt.Ink, TextAnchor.MiddleLeft);
                art.Label(panel, a + " level", "Lv " + level + "/" + COgheAbilities.Max, new Rect(x, y + 36, bw, 21), 14, color, TextAnchor.MiddleLeft).font = art.BoldFont;
                for (int j = 0; j < COgheAbilities.Max; j++)
                    art.Box(panel, a + " bar " + (j + 1), new Rect(x + j * (seg + 3), y + 63, seg, 9), j < level ? color : new Color(.9f, .91f, .89f)).pixelsPerUnitMultiplier = 8;
                art.Label(panel, a + " now", AbilityNow(a), new Rect(x, y + 80, bw, 29), 13, COgheUIArt.Muted, TextAnchor.UpperLeft);
            }
            if (pending > 0)
            {
                LabelButton(panel, "Choose card", new Rect(20, h - 70, w - 40, 52), COgheIcon.Cards, "Choose a new power", () => OpenAbilityPick(pending, false), true, 16);
            }
            else art.Label(panel, "Next card", COgheAbilities.AllFull ? "All powers learned!" : "More cards after Understand COghe", new Rect(24, h - 70, w - 48, 48), 14, COgheUIArt.Teal);
        }
    }
}
