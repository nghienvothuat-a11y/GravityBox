using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// Style (Mrk 01/10; screen designed by Codex, OUTBOX/COGHE_CUSTOMIZE_UI_2026_10_01): COghe on a little stage in Home.
    /// Colors: pick a syringe and hold on COghe's skin to inject it; up to four inks, a fifth asks which one it replaces.
    /// Accessories: one hat, up to two little things floating inside. Every change saves itself; Undo steps back one whole
    /// hold, rinse or wardrobe change. The stage owns the pointer from press to release, so nothing reaches the room.
    /// </summary>
    public sealed partial class COgheProductUI
    {
        private const int StyleHistory = 20;
        private const float NeedsClarity = .1f;   // below this the body hides what floats inside (Ocean everywhere: .3)
        private static string styleInk = "INK_OCEAN";   // the syringe in hand, kept for the session
        private int styleTab, styleSub, styleReplaceSlot = -1;
        private string stylePending;   // what a dialog is about: the locked item, the ink or inside thing waiting for a place
        private readonly List<COgheStyle> styleUndo = new List<COgheStyle>(StyleHistory);
        private bool styleOpen, styleSaving, styleDirty;
        private COgheStyleStage styleStage;
        private COgheSyringe syringe;
        private Text styleCaption, styleStatus, styleDetail;
        private RectTransform styleMix;
        private CanvasGroup styleUndoGroup, styleRinseGroup;
        private float styleStageTop = 84, styleStageBottom = 300, styleScroll = 1;
        // the hold: where it went in (a particle and an offset, so it rides the body), for how long, and the look before it
        private bool injecting, onBody, holdChanged, holdTouch;
        private string shownMix;   // the inks "Your mix" and the cards were built with
        private int anchor = -1; private Vector3 anchorOffset;
        private float held, syringeAway = 1, slideClock;
        private COgheStyle beforeHold;
        // the stage camera, eased in from the room and handed back to it
        private Vector3 styleFocus; private float styleSize = -1; private Quaternion styleRotation = Quaternion.identity;
        private float homeEase;

        public bool StyleHolding => injecting;
        public int StyleUndoCount => styleUndo.Count;
        public string StyleInk => styleInk;
        public int StyleTab => styleTab;
        public int StyleReplaceSlot => styleReplaceSlot;
        private VenomSurface Surface => Game.Matter.GetComponent<VenomSurface>();
        private COgheInking Inking => Game.Matter.GetComponent<COgheInking>();
        private bool InHome => Page == COgheProductPage.Home || Page == COgheProductPage.Style;
        private bool Earned(int level) => COgheHomeRoom.Reached(Game, level);
        // the shop (Mrk 02/10): items whose level is reached can be tried on COghe; only an owned look is kept
        private COgheStyle styleCommitted; private string styleTrying;
        private readonly List<string> unownedInLook = new List<string>(6);
        /// <summary>Items in the look COghe wears now that are not owned (inks only if some of them is in the liquid).</summary>
        public List<string> UnownedInLook()
        {
            SyncLook(); unownedInLook.Clear(); var s = COgheStyle.Current;
            for (int slot = 0; slot < COgheInking.Slots; slot++)
            {
                string id = s.Inks[slot]; if (string.IsNullOrEmpty(id) || COgheShop.Owns(id) || unownedInLook.Contains(id)) continue;
                foreach (var a in s.Amount) if (a[slot] > .002f) { unownedInLook.Add(id); break; }
            }
            if (!COgheShop.Owns(s.Hat)) unownedInLook.Add(s.Hat);
            foreach (var id in s.Inside) if (!COgheShop.Owns(id) && !unownedInLook.Contains(id)) unownedInLook.Add(id);
            return unownedInLook;
        }
        private bool LookOwned() => UnownedInLook().Count == 0;
        /// <summary>Back to the last look that was all owned (trying on ends without buying).</summary>
        private void RevertLook()
        {
            if (styleCommitted == null) return;
            COgheStyle.Current.CopyFrom(styleCommitted); COgheStyle.Current.ApplyTo(Game);
            styleTrying = null; styleUndo.Clear(); styleSaving = false;
        }
        /// <summary>The item on trial right now and not owned: the selected ink, or the hat / inside thing last put on.</summary>
        private string TriedForSale()
        {
            if (styleTab == 0) return COgheShop.Owns(styleInk) ? null : styleInk;
            if (styleTrying == null || COgheShop.Owns(styleTrying)) return null;
            var wear = COgheWardrobe.Find(styleTrying); return wear != null && wear.Inside == (styleSub == 1) ? styleTrying : null;
        }
        private void BuyTried(string id) => OfferItem(id, () => { if (styleTrying == id) styleTrying = null; SaveLook(); Rebuild(); });

        // Entering and leaving ----------------------------------------------------------------------------------------------
        public void OpenStyle()
        {
            if (Page != COgheProductPage.Home) return;
            Page = COgheProductPage.Style; Popup = COgheProductPopup.None; styleOpen = true;
            styleTab = styleSub = 0; styleReplaceSlot = -1; stylePending = null; styleUndo.Clear(); styleScroll = 1;
            var chosen = COgheInks.Find(styleInk); if (chosen == null || !Earned(chosen.Level) || !COgheShop.Owns(styleInk)) styleInk = COgheInks.Catalog[0].Id;
            styleCommitted = COgheStyle.Current.Clone(); styleTrying = null;
            var room = Game.HomeRoom;
            Game.Personality?.TakeStage(room != null ? room.StageSpot() : Game.Motion.Centre(0));   // a clear spot of the floor
            styleFocus = homeFocus; styleSize = homeSize; styleRotation = Game.Owner.View.transform.rotation;
            Rebuild();
        }

        public void LeaveStyle()
        {
            if (Page != COgheProductPage.Style) return;
            if (styleStage != null) styleStage.Release();
            if (!LookOwned()) { ShowStylePopup(COgheProductPopup.StyleTryOn); return; }   // keep it (buy) or take it off
            CloseStyle();
            Page = COgheProductPage.Home; Popup = COgheProductPopup.None;
            if (styleSize > 0) { homeFocus = styleFocus; homeSize = styleSize; homeEase = 1.2f; }   // the room camera eases back
            Rebuild();
        }

        /// <summary>Ends everything the Style screen started (whichever way the screen is left).</summary>
        private void CloseStyle()
        {
            if (!styleOpen) return;
            styleOpen = false;
            if (styleStage != null) styleStage.Release();
            EndStyleHold();
            if (syringe != null) { Destroy(syringe.gameObject); syringe = null; }
            syringeAway = 1; styleReplaceSlot = -1; stylePending = null; styleUndo.Clear();
            Game.Personality?.LeaveStage(); Game.HomeRoom?.SetStage(false);
            if (LookOwned()) SaveLook();   // a hold still settling is stored again when it settles
            else RevertLook();             // left another way while trying something on: it was not bought
        }

        /// <summary>Every frame: finish saving a settling look; while holding, inject and move the syringe.</summary>
        private void StyleTick()
        {
            if (styleSaving)
            {
                var ink = Inking;
                if (ink == null || ink.Settled)
                {
                    styleSaving = false; SaveLook(); RefreshStyle();
                    if (shownMix != MixSignature()) styleDirty = true;   // a rinse emptied the mix: show it
                }
            }
            if (Page != COgheProductPage.Style) return;
            if (styleDirty && !injecting && Popup == COgheProductPopup.None) { styleDirty = false; Rebuild(); }   // the mix changed: its cards too
            float dt = Time.deltaTime;
            if (injecting && onBody) InjectStep(dt);
            PoseSyringe(dt);
        }

        // Saving and Undo ---------------------------------------------------------------------------------------------------
        /// <summary>Copy the inks on COghe into the saved look (nothing is written to disk).</summary>
        private void SyncLook() { var ink = Inking; if (ink != null) { ink.FinishRinse(); ink.Store(COgheStyle.Current); } }
        /// <summary>App to the background: write the look now (a settling hold would otherwise wait for the app to come back).</summary>
        private void OnApplicationPause(bool paused) { if (paused && (styleOpen || styleSaving) && Game != null && Game.Matter != null) SaveLook(); }
        /// <summary>Keep the look on the device, if everything in it is owned (a look being tried on is never kept).</summary>
        private void SaveLook()
        {
            if (!LookOwned()) return;
            COgheStyle.Current.Save(); styleCommitted = COgheStyle.Current.Clone();
        }
        private void PushUndo(COgheStyle before)
        {
            if (styleUndo.Count == StyleHistory) styleUndo.RemoveAt(0);
            styleUndo.Add(before);
        }
        /// <summary>One wardrobe change: undoable, saved and worn at once.</summary>
        private void Change(System.Action edit)
        {
            SyncLook(); PushUndo(COgheStyle.Current.Clone());
            edit(); SaveLook(); Dress();
        }
        private void Dress()
        {
            var s = COgheStyle.Current; var wear = Surface.GetComponent<COgheAccessories>();
            if (s.HasWear) { if (wear == null) wear = COgheAccessories.Attach(Game); wear.Dress(s.Hat, s.Inside); }
            else if (wear != null) Destroy(wear);
        }

        public void StyleUndo()
        {
            if (injecting || styleUndo.Count == 0) return;
            var last = styleUndo[styleUndo.Count - 1]; styleUndo.RemoveAt(styleUndo.Count - 1);
            styleSaving = false; styleReplaceSlot = -1;
            COgheStyle.Current.CopyFrom(last); COgheStyle.Current.ApplyTo(Game); SaveLook();
            Rebuild();
        }

        private void RinseColors()
        {
            var ink = Inking; Popup = COgheProductPopup.None;
            if (ink != null)
            {
                SyncLook(); PushUndo(COgheStyle.Current.Clone());
                ink.Rinse(); styleSaving = true; styleReplaceSlot = -1; COgheAnalytics.Log("style_rinse");
            }
            Rebuild(); Notify("Colors rinsed · Undo available");
        }

        // The hold ----------------------------------------------------------------------------------------------------------
        /// <summary>A press on the stage: starts injecting only on COghe's skin, with an ink that has a place in the mix.</summary>
        public bool BeginStyleHold(Vector2 screen, bool touch)
        {
            if (Page != COgheProductPage.Style || Popup != COgheProductPopup.None || styleTab != 0 || injecting) return false;
            if (!Pick(screen, touch)) return false;
            { var washing = Inking; if (washing != null) washing.FinishRinse(); }
            if (styleReplaceSlot < 0 && !HasRoomFor(styleInk)) { stylePending = styleInk; ShowStylePopup(COgheProductPopup.StyleReplaceInk); return false; }
            SyncLook(); beforeHold = COgheStyle.Current.Clone();
            injecting = onBody = true; holdChanged = false; holdTouch = touch; held = slideClock = 0;
            COgheAudio.Instance?.Play("glass_tok", .4f, 0, .1f);
            RefreshStyle();
            return true;
        }
        /// <summary>The finger moved: keep adding where it is, or stop adding while it is off COghe.</summary>
        public void MoveStyleHold(Vector2 screen)
        {
            if (!injecting) return;
            bool was = onBody; onBody = Pick(screen, holdTouch);   // aimed as when it began (just above a fingertip)
            if (was != onBody) RefreshStyle();
        }
        /// <summary>Released, cancelled or interrupted: no more ink. The liquid settles, then the look saves.</summary>
        public void EndStyleHold()
        {
            if (!injecting) return;
            injecting = onBody = false;
            if (holdChanged)
            {
                PushUndo(beforeHold); styleSaving = true; COgheAnalytics.Log("style_inject", "ink", styleInk, "seconds", held);
                for (int s = 0; s < COgheInking.Slots; s++) if (beforeHold.Inks[s] != MixInks()[s]) styleDirty = true;
                COgheAudio.Instance?.Play("creature_happy", .35f, 0, .3f);
            }
            beforeHold = null; RefreshStyle();
        }

        private bool Pick(Vector2 screen, bool touch)
        {
            // a finger hides the contact: aim just above the fingertip while that is still COghe
            if (touch && PickAt(screen + Vector2.up * 26 * canvas.scaleFactor)) return true;
            return PickAt(screen);
        }
        /// <summary>The front-most particle the ray passes within skin reach of: the anchor, and where the ray enters.</summary>
        private bool PickAt(Vector2 screen)
        {
            var ray = Game.Owner.View.ScreenPointToRay(screen); var drawn = Surface.DrawnParticles; var escaped = Game.Matter.Escaped;
            float reach = Game.Matter.Profile.ParticleRadius * 1.5f, nearest = float.MaxValue; int best = -1;
            for (int i = 0; i < drawn.Length; i++)
            {
                if (escaped[i]) continue;
                var v = drawn[i] - ray.origin; float t = Vector3.Dot(v, ray.direction), off = (v - ray.direction * t).magnitude;
                if (off > reach) continue;
                float front = t - Mathf.Sqrt(reach * reach - off * off);
                if (front < nearest) { nearest = front; best = i; }
            }
            if (best < 0) return false;
            anchor = best; anchorOffset = ray.origin + ray.direction * nearest - drawn[best];
            return true;
        }

        private void InjectStep(float dt)
        {
            if (anchor < 0) return;
            var ink = Inking; if (ink == null) ink = COgheInking.Attach(Game, COgheStyle.Current.Seed);
            bool known = System.Array.IndexOf(ink.Inks, styleInk) >= 0;
            if (styleReplaceSlot >= 0) { ink.SlotFor(styleInk, styleReplaceSlot); styleReplaceSlot = -1; known = false; }
            if (!ink.Inject(Surface.DrawnParticles[anchor] + anchorOffset, styleInk, dt, held))
            {
                // the mix filled up some other way (Undo): ask, as on a press
                if (styleStage != null) styleStage.Release();
                stylePending = styleInk; ShowStylePopup(COgheProductPopup.StyleReplaceInk); return;
            }
            held += dt; holdChanged = true;
            if (!known) BuildMix();   // a new colour joined the mix
            slideClock += dt; if (slideClock > .3f) { slideClock = 0; COgheAudio.Instance?.Play("creature_slide", .18f, 0, .2f); }
        }

        private void PoseSyringe(float dt)
        {
            bool show = injecting && onBody && anchor >= 0;
            syringeAway = Mathf.MoveTowards(syringeAway, show ? 0 : 1, dt * (show ? 6 : 2.5f));
            if (syringeAway >= 1 || anchor < 0) { if (syringe != null && syringe.gameObject.activeSelf) syringe.gameObject.SetActive(false); return; }
            if (syringe == null) syringe = COgheSyringe.Create(Game.transform, Game.Matter.Profile.Skin);
            if (!syringe.gameObject.activeSelf) syringe.gameObject.SetActive(true);
            var ink = COgheInks.Find(styleInk); if (ink != null) syringe.SetInk(ink);
            var drawn = Surface.DrawnParticles; var view = Game.Owner.View.transform;
            Vector3 centre = Vector3.zero; for (int i = 0; i < drawn.Length; i++) centre += drawn[i]; centre /= drawn.Length;
            var tip = drawn[anchor] + anchorOffset;
            // standing up out of the skin, a little toward the camera: the barrel stays in sight above the finger
            var outward = (tip - centre).normalized * .5f + view.up * .8f - view.forward * .15f + view.right * .25f;
            syringe.Pose(tip, outward, Mathf.Lerp(-.004f, .05f, Mathf.SmoothStep(0, 1, syringeAway)), 1 - .75f * Mathf.Clamp01(held / 3));
        }

        private string[] MixInks() { var ink = Inking; return ink != null ? ink.Inks : COgheStyle.Current.Inks; }
        private string MixSignature() => string.Join(",", MixInks());
        private bool HasRoomFor(string id)
        {
            foreach (var s in MixInks()) if (s == null || s == id) return true;
            return false;
        }
        private bool HasColors { get { foreach (var s in MixInks()) if (s != null) return true; return false; } }

        // Choosing ----------------------------------------------------------------------------------------------------------
        public void SetStyleTab(int tab, int sub)
        {
            if (tab != styleTab || sub != styleSub) styleScroll = 1;
            if (tab != styleTab) styleReplaceSlot = -1;   // a waiting replacement is for this hold only
            styleTab = tab; styleSub = sub; Rebuild();
        }

        /// <summary>A catalog card (an ink, a hat, "" for no hat, or an inside thing).</summary>
        public void ChooseStyleItem(string id)
        {
            if (injecting) return;
            var ink = COgheInks.Find(id);
            if (ink != null)
            {
                if (!Earned(ink.Level)) { stylePending = id; ShowStylePopup(COgheProductPopup.StyleLocked); return; }
                if (!COgheShop.Owns(id)) COgheAnalytics.Log("item_preview", "item", id);
                if (styleInk != id) styleReplaceSlot = -1;
                { var washing = Inking; if (washing != null) washing.FinishRinse(); }
                styleInk = id;   // choosing a syringe does not change COghe
                if (styleReplaceSlot < 0 && !HasRoomFor(id)) { stylePending = id; ShowStylePopup(COgheProductPopup.StyleReplaceInk); return; }
                Rebuild(); return;
            }
            var s = COgheStyle.Current;
            if (id == "") { if (s.Hat != "") Change(() => s.Hat = ""); Rebuild(); return; }
            var wear = COgheWardrobe.Find(id); if (wear == null) return;
            if (!Earned(wear.Level)) { stylePending = id; ShowStylePopup(COgheProductPopup.StyleLocked); return; }
            if (!COgheShop.Owns(id)) { styleTrying = id; COgheAnalytics.Log("item_preview", "item", id); }
            if (!wear.Inside) { if (s.Hat != id) Change(() => s.Hat = id); Rebuild(); return; }
            if (s.Inside.Contains(id)) { Change(() => s.Inside.Remove(id)); Rebuild(); return; }
            if (s.Inside.Count >= COgheWardrobe.MaxInside) { stylePending = id; ShowStylePopup(COgheProductPopup.StyleReplaceInside); return; }
            Change(() => s.Inside.Add(id)); AfterInside();
        }

        private void ArmReplace(int slot) { styleReplaceSlot = slot; stylePending = null; Popup = COgheProductPopup.None; Rebuild(); }
        private void SwapInside(string old)
        {
            var s = COgheStyle.Current; string id = stylePending; stylePending = null; Popup = COgheProductPopup.None;
            int i = s.Inside.IndexOf(old); if (i >= 0 && id != null) Change(() => s.Inside[i] = id);
            AfterInside();
        }
        /// <summary>An inside thing went on: say so if COghe's colours hide it (the choice stays; nothing is recoloured).</summary>
        private void AfterInside()
        {
            var ink = Inking;
            if ((ink != null ? ink.Clarity : 0) < NeedsClarity) ShowStylePopup(COgheProductPopup.StyleNeedsClear); else Rebuild();
        }

        private void ShowStylePopup(COgheProductPopup popup)
        {
            if (styleStage != null) styleStage.Release();
            Popup = popup; Rebuild();
        }
        private void CloseStylePopup() { Popup = COgheProductPopup.None; stylePending = null; Rebuild(); }

        // The screen --------------------------------------------------------------------------------------------------------
        private void StyleView()
        {
            var s = COgheStyle.Current;
            art.Button(pageRoot, "Back", new Rect(24, 25, 52, 52), COgheIcon.Back, LeaveStyle);
            art.Label(pageRoot, "Style title", "Style", new Rect(85, 20, width - 210, 40), 19);
            styleStatus = art.Label(pageRoot, "Status", "", new Rect(85, 56, width - 210, 18), 11, COgheUIArt.Muted);
            DropsCounter(pageRoot, new Rect(width - 124, 29, 100, 44));

            // the stage gets what the catalog does not need: one row and a bit on short screens, two rows on tall ones
            float catalogHeight = Mathf.Clamp(height - 348 - 168, 124, 196), stageHeight = height - 348 - catalogHeight;
            styleStageTop = 84; styleStageBottom = 84 + stageHeight;
            var stage = art.Box(pageRoot, "Stage", new Rect(0, styleStageTop, width, stageHeight), Color.clear, true);
            styleStage = stage.gameObject.AddComponent<COgheStyleStage>();
            styleStage.Down = (p, touch) => BeginStyleHold(p, touch); styleStage.Move = MoveStyleHold; styleStage.Up = EndStyleHold;
            styleCaption = art.Label(pageRoot, "Caption", "", new Rect(24, styleStageBottom - 24, width - 48, 20), 12, COgheUIArt.Muted);

            float y = styleStageBottom + 4;
            if (styleTab == 0) { styleMix = art.Rect(pageRoot, "Your mix", new Rect(0, y, width, 58)); BuildMix(); }
            else
            {
                styleMix = null; float half = (width - 54) * .5f;
                Segment(pageRoot, "Hats", new Rect(24, y + 8, half, 44), "Hats", styleSub == 0, () => SetStyleTab(1, 0));
                Segment(pageRoot, "Inside", new Rect(30 + half, y + 8, half, 44), "Inside  " + s.Inside.Count + " / " + COgheWardrobe.MaxInside, styleSub == 1, () => SetStyleTab(1, 1));
            }
            y += 64;

            float pw = width - 24;
            var panel = art.Box(pageRoot, "Style panel", new Rect(12, y, pw, 112 + catalogHeight), COgheUIArt.Paper, true).rectTransform;
            // Colors | Accessories: one track, the chosen half filled
            float tab = (pw - 24) * .5f;
            art.Box(panel, "Tab track", new Rect(10, 6, pw - 20, 50), new Color(.9f, .925f, .9f)).pixelsPerUnitMultiplier = 2.2f;
            Tab(panel, "Colors", new Rect(12, 8, tab, 44), styleTab == 0, () => SetStyleTab(0, styleSub));
            Tab(panel, "Accessories", new Rect(12 + tab, 8, tab, 44), styleTab == 1, () => SetStyleTab(1, styleSub));
            art.Label(panel, "Selected", "", new Rect(16, 60, pw - 72, 22), 15, null, TextAnchor.MiddleLeft).font = art.BoldFont;
            styleDetail = art.Label(panel, "Detail", "", new Rect(16, 82, pw - 72, 18), 11, COgheUIArt.Muted, TextAnchor.MiddleLeft);
            string sale = TriedForSale();
            if (sale == null) art.Icon(panel, styleTab == 0 ? COgheIcon.Tap : COgheIcon.Sparkles, new Rect(pw - 48, 66, 28, 28), COgheUIArt.Teal);
            else
            {
                // on trial: buy it right here
                var buy = art.Box(panel, "Buy tried", new Rect(pw - 112, 60, 100, 38), COgheUIArt.Teal, true); buy.pixelsPerUnitMultiplier = 2f;
                var button = buy.gameObject.AddComponent<Button>(); button.targetGraphic = buy; string id = sale;
                button.onClick.AddListener(() => { COgheAudio.UiTap(); BuyTried(id); });
                DropIcon(buy.transform, new Rect(10, 8, 22, 22));
                art.Label(buy.transform, "Price", COgheEconomy.Price(sale).ToString(), new Rect(34, 0, 60, 38), 15, COgheUIArt.Paper).font = art.BoldFont;
            }
            StyleCatalog(panel, new Rect(8, 104, pw - 16, catalogHeight));

            float aw = (width - 64) / 3, ay = height - 76;
            styleUndoGroup = StyleAction(new Rect(24, ay, aw, 52), COgheIcon.Restart, "Undo", StyleUndo, false);
            styleRinseGroup = StyleAction(new Rect(32 + aw, ay, aw, 52), COgheIcon.Sparkles, "Rinse", () => ShowStylePopup(COgheProductPopup.StyleRinse), false);
            StyleAction(new Rect(40 + 2 * aw, ay, aw, 52), COgheIcon.Check, "Done", LeaveStyle, true);
            RefreshStyle();
        }

        /// <summary>The words and buttons that change while holding (never the whole screen: the catalog stays put).</summary>
        private void RefreshStyle()
        {
            if (Page != COgheProductPage.Style || pageRoot == null || styleCaption == null) return;
            var s = COgheStyle.Current; var ink = COgheInks.Find(styleInk); var inks = MixInks();
            string replacing = styleReplaceSlot >= 0 ? COgheInks.Find(inks[styleReplaceSlot])?.Name : null;
            float clarity = Inking != null ? Inking.Clarity : 0;
            styleStatus.text = styleSaving || injecting ? "Saving…" : "Saved";
            string title, detail, caption;
            if (styleTab == 0)
            {
                title = ink.Name;
                detail = replacing != null ? "Hold to replace " + replacing : !COgheShop.Owns(ink.Id) ? "Try it on COghe · buy it to keep it" : ink.Material + " · Hold on COghe to add";
                caption = injecting ? onBody ? "Adding " + ink.Name + "…" : "Back on COghe to keep adding" : replacing != null ? "Hold on COghe to replace " + replacing : "Hold on COghe. Release to keep.";
            }
            else if (TriedForSale() is string trying) { title = COgheEconomy.Find(trying)?.Name ?? ""; detail = "Trying it on · buy it to keep it"; caption = "Looks good? Buy it to keep it."; }
            else if (styleSub == 0) { title = "A little personality"; detail = "Pick one hat. Tap None to take it off."; caption = "One hat, all yours."; }
            else
            {
                title = "Little things inside"; detail = "Pick up to two. Best with clear colors.";
                caption = s.Inside.Count == 0 ? "They float inside COghe." : clarity >= NeedsClarity ? "Your decorations are ready." : "Clear ink makes these visible.";
            }
            pageRoot.Find("Style panel/Selected").GetComponent<Text>().text = title;
            styleDetail.text = detail; styleCaption.text = caption;
            SetEnabled(styleUndoGroup, styleUndo.Count > 0 && !injecting);
            SetEnabled(styleRinseGroup, HasColors && !injecting);
        }
        private static void SetEnabled(CanvasGroup g, bool on) { if (g == null) return; g.alpha = on ? 1 : .45f; g.interactable = on; }

        /// <summary>Your mix: the four inks by name (empty places stay neutral), the one waiting to be replaced marked.</summary>
        private void BuildMix()
        {
            if (styleMix == null) return;
            for (int i = styleMix.childCount - 1; i >= 0; i--) { var c = styleMix.GetChild(i).gameObject; c.SetActive(false); Destroy(c); }
            var inks = MixInks(); int count = 0; foreach (var id in inks) if (id != null) count++;
            shownMix = MixSignature();
            art.Label(styleMix, "Mix title", "Your mix", new Rect(24, 0, 120, 16), 11, COgheUIArt.Muted, TextAnchor.MiddleLeft);
            art.Label(styleMix, "Mix count", count + " / " + COgheInking.Slots + " colors", new Rect(width - 144, 0, 120, 16), 11, COgheUIArt.Muted, TextAnchor.MiddleRight);
            float w = (width - 66) / COgheInking.Slots;
            for (int slot = 0; slot < COgheInking.Slots; slot++)
            {
                var ink = COgheInks.Find(inks[slot]); var r = new Rect(24 + slot * (w + 6), 20, w, 36);
                if (slot == styleReplaceSlot) art.Box(styleMix, "Replacing", new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), COgheUIArt.Teal).pixelsPerUnitMultiplier = 2.4f;
                var chip = art.Box(styleMix, "Slot " + (slot + 1), r, ink != null ? COgheUIArt.Paper : new Color(.9f, .92f, .89f, .9f));
                chip.pixelsPerUnitMultiplier = 2.6f;
                if (ink == null) { art.Box(chip.transform, "Empty", new Rect(w * .5f - 7, 11, 14, 14), new Color(.78f, .83f, .79f)).pixelsPerUnitMultiplier = 4.5f; continue; }
                art.Box(chip.transform, "Swatch", new Rect(8, 9, 18, 18), ink.Color).pixelsPerUnitMultiplier = 3.6f;
                var name = art.Label(chip.transform, "Name", ink.Name, new Rect(30, 0, w - 33, 36), 11, null, TextAnchor.MiddleLeft);
                name.resizeTextForBestFit = true; name.resizeTextMinSize = 8; name.resizeTextMaxSize = 11; name.verticalOverflow = VerticalWrapMode.Truncate;
            }
        }

        private void StyleCatalog(RectTransform panel, Rect r)
        {
            var viewport = art.Rect(panel, "Catalog", r); viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear;   // drags between the cards scroll too
            var s = COgheStyle.Current;
            int count = styleTab == 0 ? COgheInks.Catalog.Length : styleSub == 0 ? COgheWardrobe.Hats.Length + 1 : COgheWardrobe.InsideItems.Length;
            const int columns = 4; const float cardHeight = 94;
            float cell = r.width / columns; int rows = (count + columns - 1) / columns;
            var content = art.Rect(viewport, "Content", new Rect(0, 0, r.width, rows * cardHeight + 6));
            for (int i = 0; i < count; i++)
            {
                var cardRect = new Rect((i % columns) * cell + 3, (i / columns) * cardHeight + 4, cell - 6, cardHeight - 6);
                if (styleTab == 0)
                {
                    var ink = COgheInks.Catalog[i]; bool mixed = System.Array.IndexOf(MixInks(), ink.Id) >= 0;
                    StyleCard(content, cardRect, ink.Id, ink.Name, Earned(ink.Level), ink.Level, styleInk == ink.Id, styleInk == ink.Id ? "Selected" : mixed ? "In mix" : null, COgheShop.Owns(ink.Id));
                }
                else if (styleSub == 0 && i == 0) StyleCard(content, cardRect, "", "None", true, 0, s.Hat == "", "Remove hat");
                else
                {
                    var wear = styleSub == 0 ? COgheWardrobe.Hats[i - 1] : COgheWardrobe.InsideItems[i];
                    bool on = styleSub == 0 ? s.Hat == wear.Id : s.Inside.Contains(wear.Id);
                    StyleCard(content, cardRect, wear.Id, wear.Name, Earned(wear.Level), wear.Level, on, on ? "Equipped" : null, COgheShop.Owns(wear.Id));
                }
            }
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content;
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.verticalNormalizedPosition = styleScroll;
            scroll.onValueChanged.AddListener(v => styleScroll = v.y);
        }

        private void StyleCard(Transform content, Rect r, string id, string name, bool open, int level, bool on, string meta, bool owned = true)
        {
            if (on) art.Box(content, "Outline " + id, new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), COgheUIArt.Teal).pixelsPerUnitMultiplier = 2.2f;
            var box = art.Box(content, "Card " + (id == "" ? "NONE" : id), r, on ? COgheUIArt.Mint : open ? new Color(.995f, .995f, .972f) : new Color(.93f, .94f, .91f), true);
            box.pixelsPerUnitMultiplier = 2.4f;
            var button = box.gameObject.AddComponent<Button>(); button.targetGraphic = box;
            button.onClick.AddListener(() => { COgheAudio.UiTap(); ChooseStyleItem(id); });
            float size = Mathf.Min(54, r.width - 18);
            if (id == "") art.Icon(box.transform, COgheIcon.Close, new Rect((r.width - 30) * .5f, 4 + (size - 30) * .5f, 30, 30), COgheUIArt.Muted);
            else
            {
                var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                var ir = icon.rectTransform; ir.SetParent(box.transform, false); ir.anchorMin = ir.anchorMax = new Vector2(0, 1); ir.pivot = new Vector2(0, 1);
                ir.anchoredPosition = new Vector2((r.width - size) * .5f, -4); ir.sizeDelta = new Vector2(size, size);
                icon.sprite = ItemIcon(id, "COgheStyle/Icons/"); icon.preserveAspect = true; icon.raycastTarget = false;
                icon.color = open ? Color.white : new Color(1, 1, 1, .45f);
            }
            var label = art.Label(box.transform, "Name", name, new Rect(3, size + 5, r.width - 6, 16), 11);
            label.resizeTextForBestFit = true; label.resizeTextMinSize = 8; label.resizeTextMaxSize = 11; label.verticalOverflow = VerticalWrapMode.Truncate;
            if (!open)
            {
                var state = art.Label(box.transform, "State", "Level " + level, new Rect(14, size + 21, r.width - 17, 15), 10, COgheUIArt.Muted);
                float wide = state.preferredWidth; art.Icon(box.transform, COgheIcon.Lock, new Rect((r.width - wide) * .5f - 5, size + 22, 12, 12), COgheUIArt.Muted);
            }
            else if (!owned)
            {
                // for sale: its price in Drops (it can still be tried on)
                var price = art.Label(box.transform, "State", COgheEconomy.Price(id).ToString(), new Rect(14, size + 20, r.width - 17, 16), 11, COgheUIArt.Teal);
                price.font = art.BoldFont; DropIcon(box.transform, new Rect((r.width - price.preferredWidth) * .5f - 8, size + 21, 13, 13));
            }
            else if (meta != null) art.Label(box.transform, "State", meta, new Rect(3, size + 21, r.width - 6, 15), 10, on ? COgheUIArt.Teal : COgheUIArt.Muted);
            if (on) art.Icon(box.transform, COgheIcon.Check, new Rect(r.width - 20, 4, 16, 16), COgheUIArt.Teal);
        }

        /// <summary>A tab: a label, no icon; the chosen one is mint.</summary>
        private Button Segment(Transform parent, string name, Rect r, string label, bool on, System.Action click)
        {
            var b = art.Button(parent, name, r, COgheIcon.Circle, click, null, false, on);
            b.transform.Find(nameof(COgheIcon.Circle)).gameObject.SetActive(false);
            art.Label(b.transform, "Label", label, new Rect(0, 0, r.width, r.height), 14, on ? COgheUIArt.Teal : COgheUIArt.Ink).font = art.BoldFont;
            return b;
        }
        private void Tab(Transform parent, string name, Rect r, bool on, System.Action click)
        {
            Button b;
            if (on) { b = art.Button(parent, name, r, COgheIcon.Circle, click, null, true); b.transform.Find(nameof(COgheIcon.Circle)).gameObject.SetActive(false); }
            else
            {
                var box = art.Box(parent, name, r, Color.clear, true);
                b = box.gameObject.AddComponent<Button>(); b.targetGraphic = box; b.onClick.AddListener(() => { COgheAudio.UiTap(); click(); });
            }
            art.Label(b.transform, "Label", name, new Rect(0, 0, r.width, r.height), 15, on ? COgheUIArt.Paper : COgheUIArt.Ink).font = art.BoldFont;
        }
        /// <summary>A button with its icon and words centred together (art.Button lays them out for wide buttons only).</summary>
        private Button LabelButton(Transform parent, string name, Rect r, COgheIcon icon, string label, System.Action click, bool primary = false)
        {
            var b = art.Button(parent, name, r, icon, click, null, primary);
            var glyph = (RectTransform)b.transform.Find(icon.ToString());
            var text = art.Label(b.transform, "Label", label, new Rect(0, 0, r.width, r.height), 14, primary ? COgheUIArt.Paper : COgheUIArt.Ink, TextAnchor.MiddleLeft);
            text.font = art.BoldFont;
            float wide = Mathf.Min(text.preferredWidth, r.width - 44), x = (r.width - 22 - 6 - wide) * .5f;
            glyph.anchoredPosition = new Vector2(x, -(r.height - 22) * .5f); glyph.sizeDelta = new Vector2(22, 22);
            text.rectTransform.anchoredPosition = new Vector2(x + 28, 0); text.rectTransform.sizeDelta = new Vector2(wide + 4, r.height);
            return b;
        }
        private CanvasGroup StyleAction(Rect r, COgheIcon icon, string label, System.Action click, bool primary)
        {
            var holder = art.Rect(pageRoot, label + " action", r);
            LabelButton(holder, label, new Rect(0, 0, r.width, r.height), icon, label, click, primary);
            return holder.gameObject.AddComponent<CanvasGroup>();
        }

        // Dialogs -----------------------------------------------------------------------------------------------------------
        private bool StylePopup()
        {
            if (Popup < COgheProductPopup.StyleReplaceInk || Popup > COgheProductPopup.StyleTryOn) return false;
            float w = Mathf.Min(312, width - 40), h = Popup == COgheProductPopup.StyleRinse || Popup == COgheProductPopup.StyleNeedsClear ? 290 : Popup == COgheProductPopup.StyleTryOn ? 360 : 330;
            var panel = art.Box(popupRoot, "Panel", new Rect((width - w) * .5f, (height - h) * .5f, w, h), COgheUIArt.Paper, true).rectTransform;
            float half = (w - 60) * .5f;
            switch (Popup)
            {
                case COgheProductPopup.StyleReplaceInk:
                {
                    PopupTitle(panel, w, "Replace which color?");
                    var incoming = COgheInks.Find(stylePending);
                    art.Label(panel, "Message", (incoming != null ? incoming.Name : "The new color") + " takes its place.", new Rect(24, 70, w - 48, 22), 13, COgheUIArt.Muted);
                    var inks = MixInks();
                    for (int slot = 0; slot < COgheInking.Slots; slot++)
                    {
                        var ink = COgheInks.Find(inks[slot]); if (ink == null) continue; int chosen = slot;
                        var r = new Rect(24 + (slot % 2) * (half + 12), 104 + (slot / 2) * 66, half, 54);
                        var b = art.Button(panel, "Replace " + ink.Id, r, COgheIcon.Circle, () => ArmReplace(chosen));
                        b.transform.Find(nameof(COgheIcon.Circle)).gameObject.SetActive(false);
                        art.Box(b.transform, "Swatch", new Rect(14, 17, 20, 20), ink.Color).pixelsPerUnitMultiplier = 3.4f;
                        art.Label(b.transform, "Name", ink.Name, new Rect(42, 0, r.width - 48, r.height), 14, null, TextAnchor.MiddleLeft).font = art.BoldFont;
                    }
                    LabelButton(panel, "Cancel", new Rect(24, h - 80, w - 48, 56), COgheIcon.Close, "Cancel", CloseStylePopup);
                    break;
                }
                case COgheProductPopup.StyleRinse:
                    PopupTitle(panel, w, "Rinse the colors?");
                    art.Label(panel, "Message", "COghe returns to its original black. Your hat and inside decorations stay equipped. You can undo this.", new Rect(24, 74, w - 48, 100), 13, COgheUIArt.Muted);
                    LabelButton(panel, "Cancel", new Rect(24, h - 80, half, 56), COgheIcon.Close, "Cancel", CloseStylePopup);
                    LabelButton(panel, "Rinse colors", new Rect(w * .5f + 6, h - 80, half, 56), COgheIcon.Sparkles, "Rinse", RinseColors, true);
                    break;
                case COgheProductPopup.StyleReplaceInside:
                {
                    PopupTitle(panel, w, "Replace which one?");
                    var incoming = COgheWardrobe.Find(stylePending);
                    art.Label(panel, "Message", (incoming != null ? incoming.Name : "It") + " takes its place.", new Rect(24, 70, w - 48, 22), 13, COgheUIArt.Muted);
                    var inside = COgheStyle.Current.Inside;
                    for (int i = 0; i < inside.Count && i < 2; i++)
                    {
                        var wear = COgheWardrobe.Find(inside[i]); if (wear == null) continue; string old = wear.Id;
                        var r = new Rect(24 + i * (half + 12), 104, half, 120);
                        var b = art.Button(panel, "Replace " + wear.Id, r, COgheIcon.Circle, () => SwapInside(old));
                        b.transform.Find(nameof(COgheIcon.Circle)).gameObject.SetActive(false);
                        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                        var ir = icon.rectTransform; ir.SetParent(b.transform, false); ir.anchorMin = ir.anchorMax = new Vector2(0, 1); ir.pivot = new Vector2(0, 1);
                        ir.anchoredPosition = new Vector2((half - 72) * .5f, -10); ir.sizeDelta = new Vector2(72, 72);
                        icon.sprite = ItemIcon(wear.Id, "COgheStyle/Icons/"); icon.preserveAspect = true; icon.raycastTarget = false;
                        art.Label(b.transform, "Name", wear.Name, new Rect(4, 84, half - 8, 26), 13).font = art.BoldFont;
                    }
                    LabelButton(panel, "Cancel", new Rect(24, h - 80, w - 48, 56), COgheIcon.Close, "Cancel", CloseStylePopup);
                    break;
                }
                case COgheProductPopup.StyleLocked:
                {
                    var ink = COgheInks.Find(stylePending); var wear = COgheWardrobe.Find(stylePending);
                    string name = ink != null ? ink.Name : wear != null ? wear.Name : "This"; int level = ink != null ? ink.Level : wear != null ? wear.Level : 0;
                    PopupTitle(panel, w, name);
                    var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                    var ir = icon.rectTransform; ir.SetParent(panel, false); ir.anchorMin = ir.anchorMax = new Vector2(0, 1); ir.pivot = new Vector2(0, 1);
                    ir.anchoredPosition = new Vector2((w - 104) * .5f, -72); ir.sizeDelta = new Vector2(104, 104);
                    icon.sprite = stylePending != null ? ItemIcon(stylePending, "COgheStyle/Icons/") : null; icon.preserveAspect = true; icon.raycastTarget = false; icon.color = new Color(1, 1, 1, .6f);
                    art.Icon(panel, COgheIcon.Lock, new Rect(w * .5f - 10, 182, 20, 20), COgheUIArt.Muted);   // under the picture, not on it
                    art.Label(panel, "Message", "Complete level " + level + " to unlock " + name + ".", new Rect(24, 204, w - 48, 40), 13, COgheUIArt.Muted);
                    LabelButton(panel, "Close preview", new Rect(24, h - 80, w - 48, 56), COgheIcon.Back, "Back", CloseStylePopup, true);
                    break;
                }
                case COgheProductPopup.StyleTryOn:
                {
                    // leaving with something tried on and not bought: keep it (buy) or take it off
                    var unowned = UnownedInLook(); int total = 0; foreach (var id in unowned) total += COgheEconomy.Price(id);
                    PopupTitle(panel, w, "Keep this look?");
                    var names = new System.Text.StringBuilder();
                    foreach (var id in unowned) { if (names.Length > 0) names.Append(", "); names.Append(COgheEconomy.Find(id)?.Name ?? id); }
                    art.Label(panel, "Message", names + " " + (unowned.Count == 1 ? "is" : "are") + " only tried on.", new Rect(24, 70, w - 48, 40), 13, COgheUIArt.Muted);
                    bool can = COgheShop.Drops >= total;
                    var buyAll = DropButton(panel, "Buy tried on", new Rect(24, 120, w - 48, 54), "Buy · " + total, () =>
                    {
                        foreach (var id in UnownedInLook().ToArray()) COgheShop.Buy(id);
                        if (LookOwned()) { COgheAudio.Happy(); SaveLook(); Popup = COgheProductPopup.None; LeaveStyle(); }
                    });
                    if (!can) { buyAll.interactable = false; buyAll.GetComponent<Image>().color = new Color(.62f, .7f, .7f); art.Label(panel, "Short", "You have " + COgheShop.Drops + " Drops", new Rect(24, 176, w - 48, 18), 12, COgheUIArt.Muted); }
                    LabelButton(panel, "Take it off", new Rect(24, h - 140, w - 48, 52), COgheIcon.Close, "Take it off", () => { RevertLook(); Popup = COgheProductPopup.None; LeaveStyle(); });
                    LabelButton(panel, "Keep trying", new Rect(24, h - 80, w - 48, 52), COgheIcon.Back, "Keep trying", CloseStylePopup);
                    break;
                }
                case COgheProductPopup.StyleNeedsClear:
                    PopupTitle(panel, w, "Needs clear ink");
                    art.Label(panel, "Message", "Your decoration is equipped. Add a clear color such as Ocean to see it inside COghe.", new Rect(24, 74, w - 48, 100), 13, COgheUIArt.Muted);
                    LabelButton(panel, "Keep this look", new Rect(24, h - 80, half + 18, 56), COgheIcon.Check, "Keep this look", CloseStylePopup);
                    LabelButton(panel, "Go to colors", new Rect(w * .5f + 24, h - 80, half - 18, 56), COgheIcon.Sparkles, "Colors", () => { Popup = COgheProductPopup.None; SetStyleTab(0, styleSub); }, true);
                    break;
            }
            return true;
        }

        // The stage camera --------------------------------------------------------------------------------------------------
        /// <summary>COghe (and its hat) in the middle of the stage, a little below eye level, filling most of it; the room's
        /// furniture steps out of the picture once the camera is close, leaving COghe on the plain floor.</summary>
        private bool FrameStyle(Camera camera)
        {
            camera.orthographic = true; camera.aspect = (float)Screen.width / Screen.height;
            var rotation = Quaternion.Euler(25, -8, 0);
            Vector3 right = rotation * Vector3.right, up = rotation * Vector3.up;
            // how far the drawn body reaches across and up the view, plus the skin around the particles and the hat
            var drawn = Surface.DrawnParticles; Vector3 mid = Vector3.zero; for (int i = 0; i < drawn.Length; i++) mid += drawn[i]; mid /= drawn.Length;
            float l = 0, r = 0, b = 0, t = 0;
            for (int i = 0; i < drawn.Length; i++) { var d = drawn[i] - mid; float x = Vector3.Dot(d, right), y = Vector3.Dot(d, up); l = Mathf.Min(l, x); r = Mathf.Max(r, x); b = Mathf.Min(b, y); t = Mathf.Max(t, y); }
            float skin = Game.Matter.Profile.ParticleRadius * 1.6f, hat = !string.IsNullOrEmpty(COgheStyle.Current.Hat) ? .03f : .008f;
            l -= skin; r += skin; b -= skin; t += skin + hat;
            var target = mid + right * ((l + r) * .5f) + up * ((b + t) * .5f);
            float stage = Mathf.Max(120, styleStageBottom - styleStageTop), screenHeight = Screen.height / canvas.scaleFactor;
            float size = Mathf.Max((t - b) / (.74f * stage), (r - l) / (.62f * width)) * screenHeight * .5f;
            float blend = styleSize < 0 ? 1 : 1 - Mathf.Exp(-Time.unscaledDeltaTime * 4.5f);
            styleFocus = Vector3.Lerp(styleSize < 0 ? target : styleFocus, target, blend);
            styleSize = Mathf.Lerp(styleSize < 0 ? size : styleSize, size, blend);
            styleRotation = Quaternion.Slerp(styleRotation, rotation, blend);
            camera.transform.rotation = styleRotation; camera.orthographicSize = styleSize;
            float logicalY = (styleStageTop + styleStageBottom) * .5f;
            float pixelY = Screen.safeArea.yMax - logicalY * canvas.scaleFactor, centre = pixelY / Screen.height;
            camera.transform.position = styleFocus - camera.transform.forward * 1.2f - camera.transform.up * ((centre - .5f) * 2 * styleSize);
            camera.nearClipPlane = .01f; camera.farClipPlane = 30;
            if (Game.HomeRoom != null && !Game.HomeRoom.Staged && styleSize < size * 1.25f) Game.HomeRoom.SetStage(true);
            return true;
        }
    }
}
