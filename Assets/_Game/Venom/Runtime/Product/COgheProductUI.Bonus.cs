using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// The bonus "Hiểu ra" (Mrk 07/10/2026, <see cref="COgheBonus"/>). After a chapter's boss, between the unlocks and the
    /// next level, the player is offered it: Play goes to Home, where COghe asks in shapes; Skip goes on to the next level and
    /// the bonus waits in Home (its button there). Understanding every round pays the reward once, after COghe's thank-you.
    /// </summary>
    public sealed partial class COgheProductUI
    {
        private int victoryOrder, victoryBonus, bonusChapter, bonusNext, bonusDrops, bonusShownRound = -1, bonusShownMisses = -1;
        private bool bonusFromVictory, bonusPaid, bonusThanksShown, bonusPopupShown;
        private int bonusEarned;   // everything this bonus paid: the rounds and the prize
        /// <summary>The bonus being played in Home (its chapter), or 0.</summary>
        public int BonusChapter => bonusChapter;
        /// <summary>The bonus this victory will offer after the unlocks (its chapter), or 0.</summary>
        public int VictoryBonus => victoryBonus;

        private void OfferBonusAfter(int order)
        {
            victoryOrder = order; victoryBonus = COgheBonus.After(Catalog, order);
            if (victoryBonus > 0) COgheAnalytics.Log("bonus_offer", "chapter", victoryBonus);
        }

        /// <summary>Starts chapter <paramref name="chapter"/>'s bonus in Home (from the victory offer, or Home's button).</summary>
        public void StartBonus(int chapter, bool fromVictory)
        {
            var rounds = COgheBonus.Rounds(chapter); if (rounds == null || !Game.Progress.HomeUnlocked) return;
            bonusFromVictory = fromVictory; bonusNext = fromVictory ? victoryOrder + 1 : 0;
            victoryBonus = 0; victoryNextRequested = false;
            if (Page != COgheProductPage.Home) EnterHome();
            Popup = COgheProductPopup.None; homeZoom = false;
            bonusChapter = chapter; bonusPaid = bonusThanksShown = bonusPopupShown = false; bonusShownRound = bonusShownMisses = -1; bonusEarned = 0;
            Game.Personality?.BeginBonus(rounds);
            COgheAnalytics.Log("bonus_start", "chapter", chapter, "from", fromVictory ? "victory" : "home");
            Rebuild();
        }

        private void BonusTick()
        {
            var p = Game.Personality;
            if (p == null || !p.InBonus) { if (!bonusPaid) { bonusChapter = 0; Rebuild(); } return; }
            if (p.BonusRound != bonusShownRound || p.BonusMisses != bonusShownMisses || p.BonusThanking != bonusThanksShown) Rebuild();
            // each right answer pays at once (Mrk 07/10), at the top of COghe's jump of joy: little hearts, and the Drops fly
            // out of it into the counter
            int joy = p.TakeBonusJoy();
            if (joy > 0)
            {
                int paid = COgheShop.Earn(COgheBonus.RoundKey(bonusChapter, joy), COgheBonus.RoundReward(joy), "bonus_round");
                bonusEarned += paid;
                var at = BonusScreen(p.SkinCentre + Vector3.up * .05f);
                COgheHearts.Burst(safe, at, bonusChapter * 31 + joy, 9, .6f);
                if (paid > 0) FlyDrops(at, 4 + joy, paid, joy);
                COgheAnalytics.Log("bonus_round", "chapter", bonusChapter, "round", joy, "drops", paid);
            }
            if (p.TakeBonusHearts()) COgheHearts.Burst(safe, BonusScreen(p.SkinCentre + Vector3.up * .05f), bonusChapter * 31 + 7);
            if (p.TakeBonusConfetti()) COgheConfetti.Burst(safe, height, BonusScreen(p.SkinCentre + Vector3.up * .07f), bonusChapter * 53 + 11);
            if (p.TakeBonusPrize()) PayBonusPrize(true);
            if (p.BonusFinished && !bonusPopupShown)
            {
                if (!bonusPaid) PayBonusPrize(false);
                bonusPopupShown = true; Popup = COgheProductPopup.BonusDone; Rebuild();
            }
        }

        /// <summary>The prize for the whole bonus, the largest; it flies out of COghe as it dances.</summary>
        private void PayBonusPrize(bool show)
        {
            bonusPaid = true;
            bonusDrops = COgheShop.Earn(COgheBonus.Key(bonusChapter), COgheBonus.Reward, "bonus");
            bonusEarned += bonusDrops;
            COgheAnalytics.Log("bonus_complete", "chapter", bonusChapter, "drops", bonusDrops, "total", bonusEarned);
            if (show && bonusDrops > 0 && Game.Personality != null) FlyDrops(BonusScreen(Game.Personality.SkinCentre + Vector3.up * .04f), 16, bonusDrops, 99);
        }
        /// <summary>A world point in the safe area's logical units from its lower-left corner (where effects are placed).</summary>
        private Vector2 BonusScreen(Vector3 world)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(safe, Game.Owner.View.WorldToScreenPoint(world), null, out var local);
            return local - safe.rect.min;
        }
        /// <summary>Drops seen arriving: the counter holds back what is still in the air and counts up as each one lands.</summary>
        private void FlyDrops(Vector2 from, int icons, int amount, int seed)
        {
            dropsPending += amount;
            COgheDropsFly.Launch(safe, Drop, art.BoldFont, from, () => new Vector2(width - 103, height - 108), icons, amount, share =>
            {
                dropsPending = Mathf.Max(0, dropsPending - share); dropsBumpAt = Time.time;
                COgheAudio.Instance?.Play("metal_clink", .14f, 0, .03f);
            }, bonusChapter * 97 + seed);
        }

        /// <summary>"Later" in the bonus: it waits in Home; from a victory the campaign goes on.</summary>
        private void LeaveBonus()
        {
            COgheAnalytics.Log("bonus_skip", "chapter", bonusChapter, "where", "home");
            bool fromVictory = bonusFromVictory; EndBonusSession();
            if (fromVictory) ContinueAfterBonus(); else Rebuild();
        }
        private void FinishBonus()
        {
            Popup = COgheProductPopup.None;
            bool fromVictory = bonusFromVictory; EndBonusSession();
            if (fromVictory) ContinueAfterBonus(); else Rebuild();
        }
        private void EndBonusSession() { Game.Personality?.EndBonus(); bonusChapter = 0; bonusFromVictory = false; }
        private void ContinueAfterBonus()
        {
            int next = bonusNext; bonusNext = 0;
            if (next >= 1 && next <= Game.PlayableLevelCount) Load(next); else ShowMenu();
        }

        // Home during the bonus: COghe's question, the rounds so far, Feed (one of the answers) and Later ---------------------
        private void BonusHomeView()
        {
            var p = Game.Personality;
            bonusShownRound = p != null ? p.BonusRound : 0; bonusShownMisses = p != null ? p.BonusMisses : 0; bonusThanksShown = p != null && p.BonusThanking;
            art.Button(pageRoot, "Bonus later", new Rect(24, 25, 52, 52), COgheIcon.Back, LeaveBonus);
            art.Label(pageRoot, "Bonus title", "Understand COghe", new Rect(85, 25, width - 170, 52), 19);
            art.Button(pageRoot, "Pause", new Rect(width - 76, 25, 52, 52), COgheIcon.Pause, () => ShowPopup(COgheProductPopup.Pause));
            int rounds = p != null ? p.BonusRounds : 0;
            for (int i = 0; i < rounds; i++)
            {
                bool done = i < bonusShownRound || bonusThanksShown || p.BonusFinished, now = i == bonusShownRound && !done;
                float x = width * .5f - rounds * 13 + i * 26 + 4;
                art.Box(pageRoot, "Round " + (i + 1), new Rect(x, 86, 18, 18), done ? COgheUIArt.Teal : now ? COgheUIArt.Mint : new Color(.9f, .9f, .87f)).pixelsPerUnitMultiplier = 5;
            }
            DropsCounter(pageRoot, new Rect(width - 124, 88, 100, 40));   // the rewards fly in here
            string line = bonusThanksShown || p == null || p.BonusFinished ? "" : "What does COghe want?";
            var ask = p?.BonusAsk;
            if (ask.HasValue && bonusShownMisses >= 2 && !bonusThanksShown)
                line = ask.Value.Answer == COgheBonusAnswer.Touch ? "It wants a cuddle: tap COghe." :
                       ask.Value.Answer == COgheBonusAnswer.Feed ? "It is hungry: tap Feed." : "It wants to play: tap the ball.";
            art.Label(pageRoot, "Bonus ask", line, new Rect(24, 136, width - 48, 26), 13, COgheUIArt.Muted);
            float y = height - 152;
            MenuEntry(width * .5f - 28, y, "Feed", COgheIcon.Food, () => { Game.FeedHome(); COgheAudio.UiTap(); });
        }

        // Popups --------------------------------------------------------------------------------------------------------------
        private bool BonusPopup()
        {
            if (Popup != COgheProductPopup.BonusOffer && Popup != COgheProductPopup.BonusDone) return false;
            float w = Mathf.Min(312, width - 40), h = Popup == COgheProductPopup.BonusOffer ? 330 : 336;
            float top = (height - h) * .5f;
            if (Page == COgheProductPage.Victory) top = Mathf.Clamp(height * .13f + 136, top, height - h - 16);   // under "+10 Drops"
            var panel = art.Box(popupRoot, "Panel", new Rect((width - w) * .5f, top, w, h), COgheUIArt.Paper, true).rectTransform;
            if (Popup == COgheProductPopup.BonusOffer)
            {
                int chapter = victoryBonus;
                PopupTitle(panel, w, "Understand COghe");
                art.Icon(panel, COgheIcon.Sparkles, new Rect((w - 44) * .5f, 74, 44, 44), COgheUIArt.Teal);
                art.Label(panel, "Message", "Bonus: COghe wants to tell you\nsomething, without words.", new Rect(24, 124, w - 48, 44), 14, COgheUIArt.Muted);
                var reward = art.Rect(panel, "Bonus reward", new Rect(w * .5f - 66, 172, 132, 30));
                DropIcon(reward, new Rect(0, 2, 26, 26));
                art.Label(reward, "Amount", "+" + COgheBonus.Reward + " Drops", new Rect(32, 0, 100, 30), 17, COgheUIArt.Teal, TextAnchor.MiddleLeft).font = art.BoldFont;
                LabelButton(panel, "Play bonus", new Rect(24, h - 120, w - 48, 54), COgheIcon.Play, "Play", () => StartBonus(chapter, true), true);
                LabelButton(panel, "Skip bonus", new Rect(24, h - 58, w - 48, 44), COgheIcon.Skip, "Skip (play it later in Home)", () =>
                {
                    COgheAnalytics.Log("bonus_skip", "chapter", chapter, "where", "offer");
                    victoryBonus = 0; Resume();
                });
            }
            else
            {
                PopupTitle(panel, w, "You understood COghe!");
                var heart = new GameObject("Heart", typeof(RectTransform), typeof(Image));
                var r = heart.GetComponent<RectTransform>(); r.SetParent(panel, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f, 1);
                r.anchoredPosition = new Vector2(0, -76); r.sizeDelta = new Vector2(54, 54);
                var im = heart.GetComponent<Image>(); im.sprite = COgheHearts.Heart; im.color = new Color(.86f, .36f, .3f); im.raycastTarget = false;
                art.Label(panel, "Message", "COghe is so happy you got it.", new Rect(24, 136, w - 48, 24), 14, COgheUIArt.Muted);
                var reward = art.Rect(panel, "Bonus reward", new Rect(w * .5f - 84, 166, 168, 40));
                DropIcon(reward, new Rect(0, 4, 32, 32));
                var amount = art.Label(reward, "Amount", bonusDrops > 0 ? "+" + bonusDrops + " Drops" : "Already earned", new Rect(38, 0, 130, 40), 24, COgheUIArt.Teal, TextAnchor.MiddleLeft);
                amount.font = art.BoldFont;
                if (bonusDrops > 0) COgheCountUp.Run(amount, bonusDrops, "+", " Drops");   // the biggest reward, counted up
                if (bonusEarned > bonusDrops) art.Label(panel, "Bonus total", "This bonus: +" + bonusEarned + " Drops in all", new Rect(24, 210, w - 48, 22), 12, COgheUIArt.Muted);
                LabelButton(panel, "Bonus continue", new Rect(24, h - 78, w - 48, 54), COgheIcon.Forward, "Continue", FinishBonus, true);
            }
            return true;
        }
    }
}
