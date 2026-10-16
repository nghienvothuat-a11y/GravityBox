using UnityEngine;

namespace GravityBox.Venom
{
    public sealed partial class COgheProductUI
    {
        private GameObject hintPanel;
        private CanvasGroup hintGroup;
        private RectTransform hintLabel, hintClose;
        private float hintUntil, hintFadeAt, hintFadeFrom;
        private bool hintClosing;
        public const float HintFadeIn = .4f, HintFadeOut = .3f;
        private string operationShownFor;
        private GameObject plaque;

        public bool HintShown => hintPanel != null;
        public float HintAlpha => hintGroup != null ? hintGroup.alpha : 0;
        public string HintText { get; private set; }
        private string LevelId => Game.Definition != null ? Game.Definition.Id : null;
        /// <summary>The Hint button: a teaching level shows how to work its new mechanism; any other level reveals its
        /// answer once unlocked by a rewarded ad (free with Plus, or when no ad can be shown). Bosses have none.</summary>
        public bool HasHint => !Game.Definition.Boss && (COgheHints.SolutionFor(LevelId) != null || COgheHints.OperationFor(LevelId) != null);
        public static bool HintUnlocked(string levelId) => COgheShop.Paid("hint:" + levelId);

        private void HintButton()
        {
            if (!HasHint) return;
            art.Button(pageRoot, "Hint", new Rect(width - 136, 25, 52, 52), COgheIcon.Help, RequestHint);
            // A teaching level says how its new mechanism works, once, as the level opens.
            string operation = COgheHints.OperationFor(LevelId);
            if (operation != null && operationShownFor != LevelId) { operationShownFor = LevelId; ShowHint(operation, 7); }
        }

        public void RequestHint()
        {
            string id = LevelId, solution = COgheHints.SolutionFor(id), operation = COgheHints.OperationFor(id);
            if (solution == null) { if (operation != null) ShowHint(operation, 8); return; }
            if (HintUnlocked(id)) { ShowHint(solution, 10); return; }
            var reward = COgheReward.Hint(id);
            if (COgheAds.RewardedAvailable)
                COgheAds.Rewarded("hint", reward, ok =>
                {
                    if (this == null) return;
                    if (ok) ShowHint(solution, 10); else Notify("No hint this time. Try again in a moment.");
                });
            else { COgheShop.ApplyReward(reward); ShowHint(solution, 10); }   // never stuck behind an ad that cannot load
        }

        private void ShowHint(string text, float seconds)
        {
            ClearHint();
            float y = 200;   // below the toast row
            var box = art.Box(safe, "Hint", new Rect(24, y, width - 48, 78), COgheUIArt.Paper);
            hintPanel = box.gameObject; HintText = text;
            hintGroup = hintPanel.AddComponent<CanvasGroup>();hintGroup.alpha = 0;
            hintFadeAt = Time.unscaledTime;hintClosing = false;
            art.Icon(box.transform, COgheIcon.Help, new Rect(12, 23, 30, 30), COgheUIArt.Teal);
            hintLabel = art.Label(box.transform, "Hint text", text, new Rect(52, 4, width - 156, 70), 13, COgheUIArt.Ink, TextAnchor.MiddleLeft).rectTransform;
            hintClose = (RectTransform)art.Button(box.transform, "Close hint", new Rect(width - 98, 15, 48, 48), COgheIcon.Close, HideHint).transform;
            hintUntil = Time.unscaledTime + seconds;
        }

        private void HideHint()
        {
            if(hintPanel==null||hintClosing)return;
            hintClosing=true;hintFadeAt=Time.unscaledTime;hintFadeFrom=hintGroup.alpha;
            hintGroup.blocksRaycasts=false;hintGroup.interactable=false;
        }
        private void ClearHint()
        {if(hintPanel!=null){hintPanel.SetActive(false);Destroy(hintPanel);}hintPanel=null;hintGroup=null;hintLabel=hintClose=null;}

        /// <summary>A real play gesture starts the fade without consuming that gesture or cancelling its command.</summary>
        public void OnGameplayInteraction()
        {if(Page==COgheProductPage.Game&&Popup==COgheProductPopup.None)HideHint();}

        private void LayoutHint()
        {
            if(hintPanel==null)return;
            ((RectTransform)hintPanel.transform).sizeDelta=new Vector2(width-48,78);
            hintLabel.sizeDelta=new Vector2(width-156,70);
            hintClose.anchoredPosition=new Vector2(width-98,-15);
            if(Page!=COgheProductPage.Game||Popup!=COgheProductPopup.None)HideHint();
        }

        /// <summary>Called every frame: hint timeout, and the level plaque leaves the play view (the HUD carries the
        /// number) so it no longer hides handles and rings (Mrk, 05/10/2026).</summary>
        private void TickHints()
        {
            if (hintPanel != null)
            {
                if(Time.unscaledTime>hintUntil||Page!=COgheProductPage.Game||Popup!=COgheProductPopup.None)HideHint();
                float t=(Time.unscaledTime-hintFadeAt)/(hintClosing?HintFadeOut:HintFadeIn);
                hintGroup.alpha=hintClosing?Mathf.Lerp(hintFadeFrom,0,Mathf.SmoothStep(0,1,t)):Mathf.SmoothStep(0,1,t);
                if(hintClosing&&t>=1)ClearHint();
            }
            if (plaque == null && Game.Owner != null && Game.Owner.Rotation != null)
            { var t = Game.Owner.Rotation.transform.Find("COghe specimen number"); if (t != null) plaque = t.gameObject; }
            if (plaque != null) { bool show = Page != COgheProductPage.Game; if (plaque.activeSelf != show) plaque.SetActive(show); }
        }
    }
}
