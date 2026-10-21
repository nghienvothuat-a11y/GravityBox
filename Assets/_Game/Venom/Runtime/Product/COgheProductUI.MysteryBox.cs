using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// Mystery gift box (Mrk 21/10/2026): on display level 3 and every 5 levels after (a boss passes its turn to the
    /// next level), a small bright box drops near the exit. When the merged body comes near, COghe walks to it on its
    /// own, the camera closes in, and the player may open it with a rewarded ad without knowing what is inside: Drops
    /// or an item (the first box: the Bee hat). Skip and it flies away. Each level's box opens once (key box:&lt;level&gt;).
    /// The box is presentation only (<see cref="COgheMysteryBox"/>); the puzzle, its exit and its physics are unchanged.
    /// </summary>
    public sealed partial class COgheProductUI
    {
        public enum MysteryBoxPhase { None, Waiting, Walking, Offer, Reveal, Done }
        public MysteryBoxPhase BoxPhase { get; private set; }
        public COgheMysteryBox MysteryBox => mysteryBox;
        /// <summary>What the opened box gave (tests, reveal): an item id, or null with <see cref="BoxDrops"/>.</summary>
        public string BoxItem { get; private set; }
        public int BoxDrops { get; private set; }
        private COgheMysteryBox mysteryBox;
        private float boxClock, boxFxAt;
        // The prize rising out of the opened box (UI, over the level), then flying into the reveal panel.
        private RectTransform boxPrize; private CanvasGroup boxPrizeAlpha, boxPrizeName;
        private Vector2 boxPrizeFrom, boxPrizeTop, boxPrizeTo; private float boxPrizeEnd; private bool boxPanelShown;
        private const float BoxTriggerRadius = .14f, BoxExitRadius = .18f, BoxWalkLimit = 3.5f, BoxDropDelay = 1f;
        public const string BoxPlacement = "mystery_box";
        public const string FirstBoxItem = "HAT_BEE";

        /// <summary>Where each level's box rests (level root space, on the floor COghe crosses on its way out) and the
        /// camera yaw that sees it in a view-only box. A level without a place here has no box until one is chosen and
        /// tested.</summary>
        public static readonly Dictionary<string, (Vector3 spot, float yaw)> BoxSpots = new Dictionary<string, (Vector3, float)>
        {
            // 03: behind the observation wall, beside the floor route to the back-glass exit; seen from the right (yaw -72).
            { "coghe.spatial.pilot.03", (new Vector3(.10f, -.30f, .17f), -72) },
            // 08 (G6): on the floor before the climb up the right glass to the exit (routes.txt, 21/10/2026).
            { "coghe.spatial.plus.g6", (new Vector3(.24f, -.29f, .20f), 0) },
            // 13 (G2): on the floor beside the walk along x .32 to the floor exit behind shutter B.
            { "coghe.spatial.plus.g2", (new Vector3(.26f, -.29f, .07f), 0) },
            // 18–63: on a flat stretch of the recorded way out (routes.txt), before any last climb to the exit.
            { "coghe.spatial.plus.n18", (new Vector3(.27f, -.29f, -.14f), 0) },
            { "coghe.spatial.plus.n15", (new Vector3(.172f, -.265f, -.07f), 0) },
            { "coghe.spatial.plus.n44", (new Vector3(.186f, -.205f, .127f), 0) },
            { "coghe.spatial.next.18", (new Vector3(.238f, -.19f, .12f), 0) },
            { "coghe.spatial.plus.n48", (new Vector3(.217f, -.11f, .201f), 0) },
            { "coghe.spatial.plus.k05", (new Vector3(-.13f, -.29f, -.101f), 0) },
            { "coghe.spatial.plus.n31", (new Vector3(.292f, -.29f, .025f), 0) },
            { "coghe.spatial.plus.n43", (new Vector3(.184f, -.085f, .119f), 0) },
            { "coghe.spatial.plus.n32", (new Vector3(.184f, -.20f, .166f), 0) },
            // 63 (G11): the way out after the swing is the moving landing deck and the right glass, so the box waits on the
            // left shelf, in its back corner, away from the swing's grip.
            { "coghe.spatial.plus.g11", (new Vector3(-.35f, -.08f, .14f), 0) },
        };

        /// <summary>Display 3, 8, 13 …: every fifth level from 3; a boss passes its turn to the level after it.</summary>
        public static bool BoxScheduled(int order, System.Func<int, bool> boss)
        {
            bool Slot(int n) => n >= 3 && (n - 3) % 5 == 0;
            if (order < 3 || boss(order)) return false;
            return Slot(order) || Slot(order - 1) && boss(order - 1);
        }
        private bool IsBoss(int order) => Catalog != null && order >= 1 && order <= Catalog.Levels.Length && Catalog.Levels[order - 1] != null && Catalog.Levels[order - 1].Boss;
        private string BoxKey => "box:" + LevelId;
        public bool MysteryBoxHere => Game != null && Game.Definition != null && BoxSpots.ContainsKey(LevelId) &&
            BoxScheduled(Game.Definition.Order, IsBoss) && !COgheShop.Paid(BoxKey);
        private bool BoxWalking => BoxPhase == MysteryBoxPhase.Walking;

        private void TickMysteryBox()
        {
            if (Page != COgheProductPage.Game) return;
            if (BoxPhase == MysteryBoxPhase.Reveal)
            {
                AnimateBoxPrize();
                if (!boxPanelShown && Time.unscaledTime >= boxFxAt + COgheMysteryBox.ShowTime) { boxPanelShown = true; Rebuild(); }
            }
            if (BoxPhase == MysteryBoxPhase.None)
            {
                if (!MysteryBoxHere || Popup != COgheProductPopup.None || Time.timeSinceLevelLoad < BoxDropDelay) return;
                DropMysteryBox(); return;
            }
            if (BoxPhase is MysteryBoxPhase.Walking or MysteryBoxPhase.Offer or MysteryBoxPhase.Reveal)
                Game.CameraRig.TurnTo(BoxSpots[LevelId].yaw, Time.unscaledDeltaTime);   // the close-up sees round walls
            if (mysteryBox == null || Popup != COgheProductPopup.None) return;
            if (BoxPhase == MysteryBoxPhase.Waiting)
            {
                if (!mysteryBox.Landed || Game.Owner.Completed || Game.Owner.Lost || Game.Matter.FragmentCount != 1) return;
                int part = Game.Motion.Selected;
                if (Game.MechanismSuppressesMotion(part) || Game.IsFlowing(part)) return;   // never pulled out of a tube or a ride
                Vector3 centre = Game.Motion.Centre(Game.Motion.Selected);
                bool near = Vector3.Distance(centre, mysteryBox.Rest) < BoxTriggerRadius ||
                            Game.Owner.Outlet != null && Vector3.Distance(centre, Game.Owner.Outlet.position) < BoxExitRadius;
                if (near) WalkToMysteryBox(centre);
            }
            else if (BoxPhase == MysteryBoxPhase.Walking)
            {
                boxClock += Time.deltaTime;
                if (Game.Motion.Get(Game.Motion.Selected) == null || boxClock > BoxWalkLimit) { BoxPhase = MysteryBoxPhase.Offer; ShowPopup(COgheProductPopup.MysteryBox); }
            }
        }

        private void DropMysteryBox()
        {
            var root = Game.Root; Vector3 up = root.up;
            // Land on whatever is under the authored spot (floor, shelf, deck); no surface there, no box.
            if (!SurfaceBelow(root.TransformPoint(BoxSpots[LevelId].spot), up, out var rest)) { BoxPhase = MysteryBoxPhase.Done; return; }
            var go = new GameObject("Mystery gift box"); go.transform.SetParent(root, true);
            mysteryBox = go.AddComponent<COgheMysteryBox>();
            Vector3 heading = Game.Definition.CameraEuler; heading.y += BoxSpots[LevelId].yaw;   // the close-up's view
            mysteryBox.Initialize(Game.Owner.MatterProfile.Skin, rest, up, -(Quaternion.Euler(heading) * Vector3.forward));
            BoxPhase = MysteryBoxPhase.Waiting;
            COgheAnalytics.Log("mystery_box_drop", "level_id", LevelId);
        }

        private void WalkToMysteryBox(Vector3 centre)
        {
            // Stand beside the box as the close-up camera sees it (not in front of it), on the side COghe comes from.
            Vector3 up = Game.Root.up, heading = Game.Definition.CameraEuler; heading.y += BoxSpots[LevelId].yaw;
            Vector3 across = Vector3.ProjectOnPlane(Quaternion.Euler(heading) * Vector3.right, up).normalized;
            if (Vector3.Dot(centre - mysteryBox.Rest, across) < 0) across = -across;
            // 9 cm aside and 3 cm further from the camera, so the 8 cm body never covers the 4 cm box; the close-up
            // centres between them.
            Vector3 deeper = Vector3.ProjectOnPlane(Quaternion.Euler(heading) * Vector3.forward, up).normalized;
            // On the box's own surface: a shelf may end before 9 cm; then the other side, then the side COghe came from.
            Vector3 stand = mysteryBox.Rest + Vector3.ProjectOnPlane(centre - mysteryBox.Rest, up).normalized * .07f;
            foreach (var side in new[] { across, -across })
                if (SurfaceBelow(mysteryBox.Rest + side * .09f + deeper * .03f, up, out var hit) && Mathf.Abs(Vector3.Dot(hit - mysteryBox.Rest, up)) < .01f)
                { stand = hit; across = side; break; }
            stand += up * .019f; mysteryBox.LiftAwayFrom(stand);
            Game.Motion.Move(Game.Motion.Selected, stand);
            Game.CameraRig.Spotlight = mysteryBox.Rest + across * .035f + up * .035f;
            BoxPhase = MysteryBoxPhase.Walking; boxClock = 0; HideHint();
            COgheAnalytics.Log("mystery_box_reach", "level_id", LevelId);
        }

        /// <summary>The first solid (non-creature, non-trigger) surface point within 12 cm below <paramref name="point"/>.</summary>
        private bool SurfaceBelow(Vector3 point, Vector3 up, out Vector3 hitPoint)
        {
            hitPoint = point; float best = float.MaxValue; bool found = false;
            foreach (var hit in Physics.RaycastAll(point + up * .05f, -up, .17f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.rigidbody != null && System.Array.IndexOf(Game.Matter.Bodies, hit.rigidbody) >= 0) continue;
                if (hit.distance < best) { best = hit.distance; hitPoint = hit.point; found = true; }
            }
            return found;
        }

        /// <summary>The prize, decided when the box opens (so it is never something already owned) and the same for a
        /// given level and save: the first box is the Bee hat; later ones are Drops (30/50/80) or an item not owned yet,
        /// up to ten levels ahead of its unlock.</summary>
        public static COgheReward MysteryPrize(string levelId, int order)
        {
            if (COgheEconomy.Find(FirstBoxItem) != null && !COgheShop.Owns(FirstBoxItem) && order <= 3) return COgheReward.Mystery(levelId, 0, FirstBoxItem);
            var rng = new System.Random(StableSeed(levelId));
            if (rng.NextDouble() < .45)
            {
                var pool = new List<string>();
                foreach (var item in COgheEconomy.Items)
                    if (!COgheEconomy.IsGift(item.Id) && !COgheShop.Owns(item.Id) && item.Level <= order + 10) pool.Add(item.Id);
                if (pool.Count > 0) return COgheReward.Mystery(levelId, 0, pool[rng.Next(pool.Count)]);
            }
            double roll = rng.NextDouble();
            return COgheReward.Mystery(levelId, roll < .5 ? 30 : roll < .85 ? 50 : 80, null);
        }
        private static int StableSeed(string s) { unchecked { int h = 23; foreach (char c in s ?? "") h = h * 31 + c; return h; } }

        public void OpenMysteryBox()
        {
            if (BoxPhase != MysteryBoxPhase.Offer) return;
            var reward = MysteryPrize(LevelId, Game.Definition.Order);
            void Reveal()
            {
                if (this == null) return;
                BoxItem = reward.ItemId; BoxDrops = reward.Amount; BoxPhase = MysteryBoxPhase.Reveal; boxFxAt = Time.unscaledTime; boxPanelShown = false;
                if (BoxItem != null && COgheWardrobe.Find(BoxItem) is { Inside: false })
                { var look = COgheStyle.Current; look.Hat = BoxItem; look.Save(); look.ApplyTo(Game); }   // worn at once
                mysteryBox?.Open(); COgheAudio.Happy();
                COgheAnalytics.Log("mystery_box_open", "level_id", LevelId, "prize", BoxItem ?? "drops", "amount", BoxDrops);
                Popup = COgheProductPopup.MysteryReveal; Rebuild();
            }
            if (COgheAds.RewardedAvailable)
                COgheAds.Rewarded(BoxPlacement, reward, ok =>
                {
                    if (this == null) return;
                    if (ok) Reveal(); else Notify("No ad right now. Try again in a moment.");
                });
            else { COgheShop.ApplyReward(reward); Reveal(); }   // never locked behind an ad that cannot load (as hints)
        }

        public void SkipMysteryBox()
        {
            if (BoxPhase != MysteryBoxPhase.Offer) return;
            mysteryBox?.FlyAway(); COgheAnalytics.Log("mystery_box_skip", "level_id", LevelId);
            CloseMysteryBox();
        }

        /// <summary>Back to the level after the reveal (Great!) or a skip.</summary>
        public void CloseMysteryBox()
        {
            BoxPhase = MysteryBoxPhase.Done; Game.CameraRig.Spotlight = null; Resume();
        }

        private bool MysteryBoxPopup()
        {
            if (Popup != COgheProductPopup.MysteryBox && Popup != COgheProductPopup.MysteryReveal) return false;
            if (Popup == COgheProductPopup.MysteryReveal && Time.unscaledTime < boxFxAt + COgheMysteryBox.ShowTime)
            {
                // First the lid lifts and the prize rises out of the box over the level (Codex's storyboard); the
                // panel follows at ShowTime. The clear shield still holds the level's input.
                popupRoot.Find("Input shield").GetComponent<Image>().color = Color.clear;
                BoxPrize(); return true;
            }
            float w = Mathf.Min(312, width - 40), h = Popup == COgheProductPopup.MysteryBox ? 214 : 268;
            // Low on the screen: the box stays in view above it.
            var panel = art.Box(popupRoot, "Panel", new Rect((width - w) * .5f, height - h - 28, w, h), COgheUIArt.Paper, true).rectTransform;
            if (Popup == COgheProductPopup.MysteryBox)
            {
                PopupTitle(panel, w, "A mystery gift!");
                art.Label(panel, "Message", "What's inside? Open it to find out.", new Rect(24, 70, w - 48, 24), 14, COgheUIArt.Muted);
                var open = LabelButton(panel, "Open mystery box", new Rect(24, h - 116, w - 48, 54), COgheIcon.Play, "Watch an ad to open", OpenMysteryBox, true);
                open.interactable = !COgheAds.Showing;
                LabelButton(panel, "Skip mystery box", new Rect(24, h - 56, w - 48, 40), COgheIcon.Close, "Skip", SkipMysteryBox);
            }
            else
            {
                COgheRewardFx.Fireworks(panel, boxFxAt + COgheMysteryBox.ShowTime);
                PopupTitle(panel, w, "Surprise!");
                var item = BoxItem != null ? COgheEconomy.Find(BoxItem) : null;
                if (item != null)
                {
                    ItemPicture(panel, item.Id, new Rect((w - 88) * .5f, 70, 88, 88));
                    var name = art.Label(panel, "Prize", item.Name, new Rect(24, 162, w - 48, 26), 18, COgheUIArt.Ink); name.font = art.BoldFont;
                    if (item.Group == COgheShopGroup.Hat) art.Label(panel, "Worn", "COghe is wearing it!", new Rect(24, 188, w - 48, 20), 12, COgheUIArt.Teal);
                }
                else
                {
                    var reward = art.Rect(panel, "Prize", new Rect(w * .5f - 84, 96, 168, 48));
                    DropIcon(reward, new Rect(0, 6, 36, 36));
                    var amount = art.Label(reward, "Amount", "+" + BoxDrops + " Drops", new Rect(42, 0, 130, 48), 26, COgheUIArt.Teal, TextAnchor.MiddleLeft);
                    amount.font = art.BoldFont; COgheCountUp.Run(amount, BoxDrops, "+", " Drops");
                }
                LabelButton(panel, "Mystery box continue", new Rect(24, h - 70, w - 48, 54), COgheIcon.Forward, "Great!", CloseMysteryBox, true);
                BoxPrize();   // on top of the panel: it flies into the panel's picture
            }
            return true;
        }

        /// <summary>The prize over the opened box: an ivory card with the real icon, rising from the box mouth, then its
        /// name; after ShowTime it flies into the reveal panel's picture and fades. Rebuilt from <see cref="boxFxAt"/>,
        /// so a rebuild never restarts it.</summary>
        private void BoxPrize()
        {
            boxPrize = null;
            if (mysteryBox == null || Time.unscaledTime >= boxFxAt + COgheMysteryBox.OpenTime) return;
            var item = BoxItem != null ? COgheEconomy.Find(BoxItem) : null;
            Vector3 up = mysteryBox.transform.up, side = Game.Owner.View.transform.right;
            float gift = (BonusScreen(mysteryBox.Rest + side * COgheMysteryBox.Size) - BonusScreen(mysteryBox.Rest)).magnitude;
            float s = Mathf.Clamp(gift * 1.15f, 64, 112);
            boxPrizeFrom = BonusScreen(mysteryBox.Mouth + up * .1f * COgheMysteryBox.Size);
            boxPrizeTop = boxPrizeFrom + Vector2.up * Mathf.Max((BonusScreen(mysteryBox.Mouth + up * 1.25f * COgheMysteryBox.Size) - boxPrizeFrom).magnitude, s * .9f);
            // The reveal panel (MysteryBoxPopup, 268 high, 28 above the bottom): the item picture, or the drop icon.
            float panel = 268;
            boxPrizeTo = item != null ? new Vector2(width * .5f, panel - 86) : new Vector2(width * .5f - 66, panel - 92);
            boxPrizeEnd = item != null ? 88 / s : 36 / (s * .6f);
            boxPrize = art.Rect(popupRoot, "Box prize", new Rect(0, 0, s, s));
            boxPrize.anchorMin = boxPrize.anchorMax = Vector2.zero; boxPrize.pivot = new Vector2(.5f, .5f);
            boxPrizeAlpha = boxPrize.gameObject.AddComponent<CanvasGroup>(); boxPrizeAlpha.blocksRaycasts = false;
            art.Box(boxPrize, "Card", new Rect(0, 0, s, s), COgheUIArt.Paper).pixelsPerUnitMultiplier = 1.2f;
            if (item != null) ItemPicture(boxPrize, item.Id, new Rect(s * .1f, s * .1f, s * .8f, s * .8f));
            else DropIcon(boxPrize, new Rect(s * .2f, s * .2f, s * .6f, s * .6f));
            var tag = art.Box(boxPrize, "Name", new Rect((s - 180) * .5f, s + 6, 180, 30), COgheUIArt.Paper);
            boxPrizeName = tag.gameObject.AddComponent<CanvasGroup>();
            art.Label(tag.transform, "Text", item != null ? item.Name : "+" + BoxDrops + " Drops", new Rect(0, 0, 180, 30), 16, item != null ? COgheUIArt.Ink : COgheUIArt.Teal).font = art.BoldFont;
            AnimateBoxPrize();
        }

        /// <summary>Codex's timing (unscaled, from the reward): hidden to .40 s; rises .40–.90 with a 1.08 overshoot; holds
        /// with its name to ShowTime; then into the panel by OpenTime.</summary>
        private void AnimateBoxPrize()
        {
            if (boxPrize == null) return;
            float t = Time.unscaledTime - boxFxAt, scale, alpha = 1; Vector2 at;
            if (t < .4f) { at = boxPrizeFrom; scale = 0; }
            else if (t < .9f)
            {
                float u = (t - .4f) / .5f, rise = 1 - (1 - u) * (1 - u) * (1 - u);
                at = Vector2.LerpUnclamped(boxPrizeFrom, boxPrizeTop, rise);
                scale = u < .7f ? Mathf.Lerp(.25f, 1.08f, Mathf.Sin(u / .7f * Mathf.PI * .5f)) : Mathf.Lerp(1.08f, 1, Mathf.SmoothStep(0, 1, (u - .7f) / .3f));
            }
            else if (t < COgheMysteryBox.ShowTime) { at = boxPrizeTop; scale = 1; }
            else
            {
                float v = Mathf.SmoothStep(0, 1, (t - COgheMysteryBox.ShowTime) / (COgheMysteryBox.OpenTime - COgheMysteryBox.ShowTime));
                at = Vector2.Lerp(boxPrizeTop, boxPrizeTo, v); scale = Mathf.Lerp(1, boxPrizeEnd, v); alpha = 1 - v;
            }
            boxPrize.anchoredPosition = at; boxPrize.localScale = Vector3.one * scale; boxPrizeAlpha.alpha = alpha;
            boxPrizeName.alpha = t < COgheMysteryBox.ShowTime ? Mathf.Clamp01((t - .9f) / .15f) : 0;
            if (t >= COgheMysteryBox.OpenTime) { Destroy(boxPrize.gameObject); boxPrize = null; }
        }
    }
}
