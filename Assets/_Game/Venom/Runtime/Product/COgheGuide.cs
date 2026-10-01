using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// First-level guides (Mrk): on level 1 an arrow fades in over the exit hole and keeps nudging toward it; level 3 adds
    /// "Hold to rotate" with a finger showing the drag until the player has turned the view, and still points at the exit;
    /// level 4 points exactly at the handle that has to be pulled, then at the exit once the way is open. The arrow steps
    /// aside while COghe is already doing the right thing, and follows its target to the screen edge when the view turns
    /// away. Presentation only, over the product canvas (under the HUD and popups).
    /// </summary>
    public sealed class COgheGuide : MonoBehaviour
    {
        public enum Target { None, Exit, Handle }
        private const float Delay = .8f, Fade = 2.5f, RotateLearnedDegrees = 25;
        private static readonly Color Amber = new Color(.875f, .729f, .439f);
        private static Sprite arrowSprite, ringSprite;

        private VenomCampaign game;
        private COgheProductUI ui;
        private RectTransform root, arrow, ring, hint, finger;
        private CanvasGroup arrowGroup, hintGroup;
        private Image ringImage;
        private COgheTapRail handle;
        private bool rotateLesson;
        private float clock;
        private Target shown = Target.Exit;

        public Target Pointing { get; private set; }
        public bool RotateLearned { get; private set; }
        /// <summary>Tests: what is on screen (alpha above one half) and where (safe-area units from the lower left).</summary>
        public bool ArrowShown => arrowGroup.alpha > .5f;
        public bool RotateHintShown => hint != null && hintGroup.alpha > .5f;
        public Vector2 ArrowTip => arrow.anchoredPosition;
        public Vector2 TargetPoint { get; private set; }
        public bool TargetOnScreen { get; private set; }

        /// <summary>The guide for this level, or null when it has none (only levels 1, 3 and 4 teach).</summary>
        public static COgheGuide Create(COgheProductUI ui, VenomCampaign game, RectTransform safe, COgheUIArt art)
        {
            int level = game.Definition != null ? game.Definition.Order : 0;
            if (level != 1 && level != 3 && level != 4) return null;
            var go = new GameObject("Level guide", typeof(RectTransform), typeof(CanvasGroup));
            var guide = go.AddComponent<COgheGuide>();
            guide.Build(ui, game, safe, art, level);
            return guide;
        }

        private void Build(COgheProductUI owner, VenomCampaign level, RectTransform safe, COgheUIArt art, int number)
        {
            ui = owner; game = level;
            root = (RectTransform)transform; root.SetParent(safe, false); root.SetAsFirstSibling();   // under the HUD and popups
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.pivot = Vector2.zero; root.offsetMin = root.offsetMax = Vector2.zero;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            MakeSprites();

            var arrowLayer = Layer("Arrow layer"); arrowGroup = arrowLayer.gameObject.AddComponent<CanvasGroup>(); arrowGroup.alpha = 0;
            ring = Child(arrowLayer, "Target ring", new Vector2(44, 44), new Vector2(.5f, .5f));
            ringImage = ring.gameObject.AddComponent<Image>(); ringImage.sprite = ringSprite; ringImage.color = Amber; ringImage.raycastTarget = false;
            arrow = Child(arrowLayer, "Guide arrow", new Vector2(30, 45), new Vector2(.5f, 0));   // pivot on the tip
            var im = arrow.gameObject.AddComponent<Image>(); im.sprite = arrowSprite; im.raycastTarget = false;

            if (number == 4)
                foreach (var rail in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())
                    if (handle == null || rail.Label == "A") handle = rail;
            rotateLesson = number == 3 && game.Definition.ViewOnly;
            if (rotateLesson)
            {
                hint = Child(root, "Rotate hint", new Vector2(196, 96), new Vector2(.5f, .5f));
                hintGroup = hint.gameObject.AddComponent<CanvasGroup>(); hintGroup.alpha = 0;
                art.Box(hint, "Panel", new Rect(0, 0, 196, 96), new Color(COgheUIArt.Paper.r, COgheUIArt.Paper.g, COgheUIArt.Paper.b, .94f)).pixelsPerUnitMultiplier = 2f;
                art.Icon(hint, COgheIcon.Left, new Rect(34, 16, 20, 20), COgheUIArt.Muted);
                art.Icon(hint, COgheIcon.Right, new Rect(142, 16, 20, 20), COgheUIArt.Muted);
                finger = art.Icon(hint, COgheIcon.Tap, new Rect(80, 6, 36, 36), COgheUIArt.Teal).rectTransform;
                finger.pivot = new Vector2(.5f, .5f); finger.anchoredPosition = new Vector2(98, -24);
                art.Label(hint, "Label", "Hold to rotate", new Rect(0, 52, 196, 30), 16).font = art.BoldFont;
            }
        }

        private RectTransform Layer(string name) => Child(root, name, Vector2.zero, Vector2.zero, true);
        private static RectTransform Child(RectTransform parent, string name, Vector2 size, Vector2 pivot, bool stretch = false)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
            if (stretch) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; r.pivot = Vector2.zero; return r; }
            r.anchorMin = r.anchorMax = Vector2.zero; r.pivot = pivot; r.sizeDelta = size; return r;
        }

        private void LateUpdate()
        {
            if (game == null || game.Owner == null || game.Matter == null) return;
            bool live = ui.Page == COgheProductPage.Game && ui.Popup == COgheProductPopup.None && !game.Owner.Paused;
            float dt = Mathf.Min(game.Owner.Paused ? Time.unscaledDeltaTime : Time.deltaTime, .05f);   // game time; fades still run under Pause
            if (live) clock += dt;
            Pointing = live && clock > Delay ? Choose() : Target.None;
            if (rotateLesson && !RotateLearned && Mathf.Abs(game.CameraRig.OrbitYaw) >= RotateLearnedDegrees) RotateLearned = true;

            var size = root.rect.size;
            arrowGroup.alpha = Mathf.MoveTowards(arrowGroup.alpha, Pointing != Target.None ? 1 : 0, dt * Fade);
            if (Pointing != Target.None) shown = Pointing;   // a fading arrow stays where it was
            if (arrowGroup.alpha > 0) Place(shown, size);
            if (hint != null)
            {
                hintGroup.alpha = Mathf.MoveTowards(hintGroup.alpha, live && clock > Delay + .4f && !RotateLearned ? 1 : 0, dt * Fade);
                hint.anchoredPosition = new Vector2(size.x * .5f, 168);
                // the finger presses, slides right and left (the view follows a held drag), lifts, and starts again
                float c = Mathf.Repeat(clock, 2.4f);
                float press = Mathf.SmoothStep(0, 1, c / .25f) * (1 - Mathf.SmoothStep(0, 1, (c - 1.85f) / .25f));
                float slide = c < .25f || c > 1.85f ? 0 : Mathf.Sin((c - .25f) / 1.6f * Mathf.PI * 2);
                finger.anchoredPosition = new Vector2(98 + 34 * slide, -24 + 3 * press);
                finger.localScale = Vector3.one * (1 - .14f * press);
            }
        }

        private Target Choose()
        {
            if (game.Owner.Completed || game.Owner.Lost || game.Matter.EscapedCount > 0) return Target.None;   // already leaving
            if (handle != null && handle.CompletedJourneys == 0 && !game.FinalExitAvailable)
                return handle.Phase == COgheTapRail.TaskPhase.Idle ? Target.Handle : Target.None;               // on its way to pull
            var order = game.Motion.Get(game.Motion.Selected);
            return order != null && order.Exit ? Target.None : Target.Exit;                                       // heading out already
        }

        private void Place(Target target, Vector2 size)
        {
            var view = game.Owner.View;
            Vector3 world = target == Target.Handle && handle != null ? handle.HandPoint : game.Owner.Outlet.position;
            Vector3 screen = view.WorldToScreenPoint(world);
            bool behind = screen.z < 0;
            if (behind) screen = new Vector3(Screen.width - screen.x, Screen.height - screen.y, 0);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out var local);
            var p = local - root.rect.min; TargetPoint = p;
            // the HUD keeps the top bar and the bottom row; the arrow stays clear of both
            float left = 26, right = size.x - 26, bottom = 70, top = size.y - 92;
            TargetOnScreen = !behind && p.x >= left && p.x <= right && p.y >= bottom && p.y <= top;
            float bob = 7 + 9 * Mathf.Abs(Mathf.Sin(clock * Mathf.PI / .7f));
            if (TargetOnScreen)
            {
                bool below = p.y + bob + 60 > top;   // no room above: point up from underneath
                arrow.anchoredPosition = p + new Vector2(0, below ? -bob : bob);
                arrow.localRotation = Quaternion.Euler(0, 0, below ? 180 : 0);
                ring.gameObject.SetActive(true); ring.anchoredPosition = p;
                float phase = Mathf.Repeat(clock, 1.3f) / 1.3f;
                ring.localScale = Vector3.one * (.45f + 1.1f * phase);
                var c = Amber; c.a = .95f * (1 - phase); ringImage.color = c;
            }
            else
            {
                // off screen (the view turned away): wait at the nearest edge, pointing the way
                var edge = new Vector2(Mathf.Clamp(p.x, left, right), Mathf.Clamp(p.y, bottom, top));
                var toward = p - edge; if (behind || toward.sqrMagnitude < 1) toward = p - size * .5f;
                toward.Normalize();
                arrow.anchoredPosition = edge - toward * (bob - 7);
                arrow.localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.down, toward));
                ring.gameObject.SetActive(false);
            }
        }

        // Two small generated sprites: a chunky arrow (amber, ink outline) pointing down, and a thin ring.
        private static void MakeSprites()
        {
            if (arrowSprite != null) return;
            Vector2[] shape = { new Vector2(32, 3), new Vector2(61, 43), new Vector2(43, 43), new Vector2(43, 93), new Vector2(21, 93), new Vector2(21, 43), new Vector2(3, 43) };
            var ink = COgheUIArt.Ink;
            var a = new Texture2D(64, 96, TextureFormat.RGBA32, false) { name = "Guide arrow", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontUnloadUnusedAsset };
            var px = new Color[64 * 96];
            for (int y = 0; y < 96; y++)
                for (int x = 0; x < 64; x++)
                {
                    float d = SignedDistance(new Vector2(x + .5f, y + .5f), shape);
                    var c = Color.Lerp(Amber, ink, Mathf.Clamp01(d + 4.5f)); c.a = Mathf.Clamp01(.5f - d);
                    px[y * 64 + x] = c;
                }
            a.SetPixels(px); a.Apply(false, true);
            arrowSprite = Sprite.Create(a, new Rect(0, 0, 64, 96), new Vector2(.5f, 0), 100);
            arrowSprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
            var r = new Texture2D(64, 64, TextureFormat.RGBA32, false) { name = "Guide ring", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontUnloadUnusedAsset };
            px = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float d = Mathf.Abs(new Vector2(x + .5f - 32, y + .5f - 32).magnitude - 27);
                    px[y * 64 + x] = new Color(1, 1, 1, Mathf.Clamp01(2.6f - d));
                }
            r.SetPixels(px); r.Apply(false, true);
            ringSprite = Sprite.Create(r, new Rect(0, 0, 64, 64), Vector2.one * .5f, 100);
            ringSprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        private static float SignedDistance(Vector2 p, Vector2[] poly)
        {
            float best = float.MaxValue; bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                Vector2 a = poly[j], b = poly[i], ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
                best = Mathf.Min(best, (a + ab * t - p).magnitude);
                if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }
            return inside ? -best : best;
        }
    }
}
