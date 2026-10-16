using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    public sealed partial class COgheProductUI
    {
        public const int StyleUnlockLevel = 8;
        public bool StyleUnlocked => Earned(StyleUnlockLevel);
        private bool NeedsInkGuide => StyleUnlocked && COgheShop.Current.InkGuideStep < 4;
        private bool styleFromMenu, inkGuide;
        private Text inkGuideText;
        private RectTransform inkGuideCard;

        /// <summary>The coloring stage is independent of the furnished Home unlocked by Boss 10.</summary>
        public void OpenStyleFromMenu()
        {
            if (!StyleUnlocked) { Notify("Style opens after Level 8."); return; }
            if (bonusChapter > 0) EndBonusSession();
            Popup = COgheProductPopup.None; SetPaused(false);
            Game.EnterShowcase(); Game.SetHabitatPresentation(false); MenuShadows(false);
            Page = COgheProductPage.Home; homeSize = -1; homeZoom = false;
            OpenStyle(); styleFromMenu = true;
            inkGuide = NeedsInkGuide;
            victoryNextRequested = false;
            if (inkGuide)
            {
                if (COgheShop.Current.InkGuideStep == 0) COgheShop.SetInkGuideStep(1);
                styleInk = "INK_OCEAN"; styleScroll = 1;
                COgheAnalytics.Log("ink_guide_start", "step", COgheShop.Current.InkGuideStep);
            }
            Rebuild();
        }

        private void InkGuideView()
        {
            if (!inkGuide) { inkGuideText = null; return; }
            // Reuse the mix strip for a single, readable instruction. The body and syringe stay unobstructed.
            if (styleMix != null) styleMix.gameObject.SetActive(false);
            var card = art.Box(pageRoot, "Ink lesson", new Rect(16, styleStageBottom + 3, width - 32, 61), COgheUIArt.Mint);
            inkGuideText = art.Label(card.transform, "Instruction", "", new Rect(12, 2, width - 56, 57), 12, COgheUIArt.Ink);
            COgheGuide.CreateUI(pageRoot, InkGuideTarget);
            RefreshInkGuide();
        }

        private Vector2? InkGuideTarget()
        {
            if (!inkGuide || Page != COgheProductPage.Style || Popup != COgheProductPopup.None) return null;
            int step = COgheShop.Current.InkGuideStep;
            RectTransform target = step < 2 ? inkGuideCard : step >= 3 ? styleDoneGroup.transform as RectTransform : null;
            if (target != null)
                return RectTransformUtility.WorldToScreenPoint(null, target.TransformPoint(target.rect.center));
            if (step == 2 && Surface != null)
            {
                var points = Surface.DrawnParticles;
                if (points == null || points.Length == 0) return null;
                Vector3 centre = Vector3.zero; foreach (var p in points) centre += p;
                return Game.Owner.View.WorldToScreenPoint(centre / points.Length);
            }
            return null;
        }

        private void RefreshInkGuide()
        {
            if (!inkGuide || inkGuideText == null) return;
            int step = COgheShop.Current.InkGuideStep;
            if (step < 2)
            {
                styleCaption.text = "Choose a syringe below.";
                styleDetail.text = "Free · Tap Ocean to begin.";
            }
            else if (step >= 3 && !injecting) styleCaption.text = "Your first color. Make it yours!";
            inkGuideText.text = step < 2 ? "1 / 3 · Choose your ink\nTap the Ocean syringe below. This color is free." :
                step < 3 ? (injecting && held >= .6f ? "2 / 3 · Now lift your finger\nThe ink spreads through COghe and saves automatically." :
                    "2 / 3 · Press and hold on COghe\nHold to inject. Move your finger to color another spot.") :
                "3 / 3 · Ready to play\nTap Done to save your color and continue.";
        }

        private void FinishInkGuide()
        {
            if (!inkGuide) { LeaveStyle(); return; }
            if (COgheShop.Current.InkGuideStep < 3)
            { Notify("Choose Ocean, then hold on COghe and release."); return; }
            // The final step may be reached while the liquid is still settling: persist the actual look now.
            SaveLook(); COgheShop.SetInkGuideStep(4); inkGuide = false;
            COgheAnalytics.Log("ink_guide_complete");
            CloseStyle(); ShowMenu(); Play();
        }
    }
}
