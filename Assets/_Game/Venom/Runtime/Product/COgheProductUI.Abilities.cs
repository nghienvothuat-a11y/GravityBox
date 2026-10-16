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

        private static Color AbilityColor(COgheAbility a) =>
            a == COgheAbility.Emotion ? new Color(.737f, .467f, .439f) : a == COgheAbility.Agility ? new Color(.765f, .573f, .227f) : new Color(.247f, .486f, .549f);
        private static Color AbilityTint(COgheAbility a) =>
            a == COgheAbility.Emotion ? new Color(.957f, .871f, .855f) : a == COgheAbility.Agility ? new Color(.965f, .918f, .796f) : new Color(.851f, .910f, .933f);
        private static COgheIcon AbilityIcon(COgheAbility a) => a == COgheAbility.Emotion ? COgheIcon.Smile : a == COgheAbility.Agility ? COgheIcon.Zap : COgheIcon.Brain;
        private static string AbilityGain(COgheAbility a) =>
            a == COgheAbility.Emotion ? "2 new tricks" : a == COgheAbility.Agility ? "Moves faster" : "Remembers 1 more step";
        private static string AbilityHint(COgheAbility a) =>
            a == COgheAbility.Emotion ? "Find them at Home" : a == COgheAbility.Agility ? "Floors, walls and roofs" : "Finds the next device";
        private static string AbilityNow(COgheAbility a)
        {
            int level = COgheAbilities.Level(a);
            if (a == COgheAbility.Emotion) return level == 0 ? "Tricks still to discover" : "New tricks to discover at Home";
            if (a == COgheAbility.Agility) return level == 0 ? "Its usual pace" : "Faster on every surface";
            return level == 0 ? "Not yet: COghe waits for you" : "Leave COghe for 5 s: it finds the next device";
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

        private void AbilityPickPanel()
        {
            float w = Mathf.Min(372, width - 24), h = abilityChosen ? 424 : 470, top = Mathf.Max(16, (height - h) * .5f);
            var panel = art.Box(popupRoot, "Panel", new Rect((width - w) * .5f, top, w, h), COgheUIArt.Paper, true).rectTransform;
            if (abilityChosen) { AbilityChosenPanel(panel, w, h); return; }
            PopupTitle(panel, w, "Choose an ability");
            art.Label(panel, "Message", "One card for each Understand COghe", new Rect(24, 64, w - 48, 22), 13, COgheUIArt.Muted);
            float gap = 8, cardW = (w - 32 - gap * 2) / 3, cardH = 246;
            for (int i = 0; i < 3; i++) AbilityCard(panel, i, 16 + i * (cardW + gap), 104, cardW, cardH);
            if (abilitySelected >= 0)
                LabelButton(panel, "Choose ability", new Rect(24, h - 78, w - 48, 54), AbilityIcon(AbilityOrder[abilitySelected]), "Choose " + AbilityOrder[abilitySelected], () => ConfirmAbility(), true);
            else art.Label(panel, "Choose hint", "Tap a card", new Rect(24, h - 78, w - 48, 54), 14, COgheUIArt.Muted);
        }

        private void AbilityCard(Transform panel, int index, float x, float y, float w, float h)
        {
            var ability = AbilityOrder[index]; bool full = COgheAbilities.Full(ability), selected = index == abilitySelected;
            Color color = AbilityColor(ability); int level = COgheAbilities.Level(ability);
            if (selected) { y -= 10; art.Box(panel, ability + " ring", new Rect(x - 3, y - 3, w + 6, h + 6), color); }
            art.Box(panel, ability + " shadow", new Rect(x, y + 4, w, h), new Color(.86f, .89f, .86f));
            var card = art.Box(panel, ability + " card", new Rect(x, y, w, h), Color.white, true);
            var button = card.gameObject.AddComponent<Button>(); button.targetGraphic = card;
            button.onClick.AddListener(() => { COgheAudio.UiTap(); SelectAbility(index); });
            var t = card.transform;
            art.Box(t, "Art", new Rect(6, 6, w - 12, 112), full ? new Color(.92f, .92f, .9f) : AbilityTint(ability));
            var sprite = AbilityArt(ability);
            if (sprite != null)
            {
                var im = art.Rect(t, "COghe", new Rect(8, 12, w - 16, 104)).gameObject.AddComponent<Image>();
                im.sprite = sprite; im.preserveAspect = true; im.raycastTarget = false; if (full) im.color = new Color(1, 1, 1, .45f);
            }
            art.Box(t, "Badge", new Rect(12, 12, 28, 28), color); art.Icon(t, AbilityIcon(ability), new Rect(16, 16, 20, 20), Color.white);
            var name = art.Label(t, "Name", ability.ToString(), new Rect(2, 124, w - 4, 22), 14, color); name.font = art.BoldFont;
            art.Label(t, "Level", full ? "MAX" : "Lv " + level + "  →  " + (level + 1), new Rect(2, 146, w - 4, 18), 12, full ? COgheUIArt.Muted : COgheUIArt.Ink).font = art.BoldFont;
            float pip = Mathf.Min(6, (w - 24) / 10 - 2), row = pip * 10 + 2 * 9;
            for (int i = 0; i < COgheAbilities.Max; i++)
            {
                bool on = i < level, next = !full && i == level;
                art.Box(t, "Pip " + (i + 1), new Rect((w - row) * .5f + i * (pip + 2), 170, pip, pip),
                    on ? color : next ? new Color(color.r, color.g, color.b, .35f) : new Color(.9f, .91f, .89f)).pixelsPerUnitMultiplier = 8;
            }
            art.Label(t, "Gain", full ? "At its best" : AbilityGain(ability), new Rect(4, 184, w - 8, 34), 12).font = art.BoldFont;
            art.Label(t, "Hint", full ? "" : AbilityHint(ability), new Rect(4, 216, w - 8, 28), 11, COgheUIArt.Muted);
        }

        private void AbilityChosenPanel(RectTransform panel, float w, float h)
        {
            var ability = AbilityOrder[abilitySelected]; Color color = AbilityColor(ability); int level = COgheAbilities.Level(ability);
            PopupTitle(panel, w, ability + " Lv " + level + "!");
            art.Box(panel, "Art", new Rect((w - 180) * .5f, 76, 180, 150), AbilityTint(ability));
            var sprite = AbilityArt(ability);
            if (sprite != null)
            {
                var im = art.Rect(panel, "COghe", new Rect((w - 170) * .5f, 84, 170, 142)).gameObject.AddComponent<Image>();
                im.sprite = sprite; im.preserveAspect = true; im.raycastTarget = false;
            }
            art.Box(panel, "Badge", new Rect((w - 180) * .5f + 8, 84, 32, 32), color); art.Icon(panel, AbilityIcon(ability), new Rect((w - 180) * .5f + 13, 89, 22, 22), Color.white);
            string message = ability == COgheAbility.Emotion ? "COghe learned 2 new tricks.\nWatch for them at Home."
                : ability == COgheAbility.Agility ? "COghe moves faster now." : "COghe remembers one more step.";
            art.Label(panel, "Message", message, new Rect(24, 236, w - 48, 48), 14, COgheUIArt.Ink);
            if (abilityFirstIntelligence)
            {
                art.Box(panel, "Tip", new Rect(24, 290, w - 48, 54), COgheUIArt.Mint);
                art.Label(panel, "Tip text", "Leave COghe alone for 5 seconds:\nit walks to the next device by itself.", new Rect(32, 290, w - 64, 54), 13, COgheUIArt.Teal);
            }
            LabelButton(panel, "Ability continue", new Rect(24, h - 78, w - 48, 54), COgheIcon.Forward, "Continue", FinishAbilityPick, true);
        }

        private void AbilityStatsPanel()
        {
            int pending = COgheAbilities.Pending();
            float w = Mathf.Min(340, width - 32), rowH = 92, h = 112 + rowH * 3 + 70, top = Mathf.Max(16, (height - h) * .5f);
            var panel = art.Box(popupRoot, "Panel", new Rect((width - w) * .5f, top, w, h), COgheUIArt.Paper, true).rectTransform;
            PopupTitle(panel, w, "COghe's abilities"); ClosePopup(panel, w, Resume);
            art.Label(panel, "Message", "A new card after each Understand COghe", new Rect(24, 64, w - 48, 22), 13, COgheUIArt.Muted);
            for (int i = 0; i < 3; i++)
            {
                var ability = AbilityOrder[i]; Color color = AbilityColor(ability); int level = COgheAbilities.Level(ability);
                float y = 98 + i * rowH;
                art.Box(panel, ability + " row", new Rect(16, y, w - 32, rowH - 10), Color.white);
                art.Box(panel, ability + " icon box", new Rect(28, y + 12, 38, 38), color); art.Icon(panel, AbilityIcon(ability), new Rect(35, y + 19, 24, 24), Color.white);
                art.Label(panel, ability + " name", ability.ToString(), new Rect(78, y + 8, w - 170, 24), 15, COgheUIArt.Ink, TextAnchor.MiddleLeft).font = art.BoldFont;
                art.Label(panel, ability + " level", "Lv " + level + "/" + COgheAbilities.Max, new Rect(w - 108, y + 8, 80, 24), 14, COgheUIArt.Ink, TextAnchor.MiddleRight).font = art.BoldFont;
                float barW = w - 32 - 78 - 18, seg = (barW - 3 * 9) / 10;
                for (int s = 0; s < COgheAbilities.Max; s++)
                    art.Box(panel, ability + " bar " + (s + 1), new Rect(78 + s * (seg + 3), y + 38, seg, 11), s < level ? color : new Color(.9f, .91f, .89f)).pixelsPerUnitMultiplier = 8;
                art.Label(panel, ability + " now", AbilityNow(ability), new Rect(78, y + 52, barW + 10, 26), 11, COgheUIArt.Muted, TextAnchor.MiddleLeft);
            }
            if (pending > 0) LabelButton(panel, "Choose card", new Rect(24, h - 66, w - 48, 50), COgheIcon.Cards, "Choose your card", () => OpenAbilityPick(pending, false), true);
            else art.Label(panel, "Next card", COgheAbilities.AllFull ? "Every ability is at its best" : "Next card: after the next Boss", new Rect(24, h - 62, w - 48, 40), 13, COgheUIArt.Teal);
        }
    }
}
