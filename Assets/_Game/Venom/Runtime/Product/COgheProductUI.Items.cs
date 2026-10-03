using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// Home: the item menu (every piece of furniture, earned or "Level N" with a lock; choosing a locked one shows its
    /// ghost in its place, choosing an earned one sends COghe to play with it) and the room camera, which frames the whole
    /// room and leans in on whatever COghe is playing with or the item being previewed.
    /// </summary>
    public sealed partial class COgheProductUI
    {
        private readonly Dictionary<string, Sprite> itemIcons = new Dictionary<string, Sprite>();
        private Vector3 homeFocus; private float homeSize = -1;

        private Sprite ItemIcon(string id, string folder = "COgheHome/Icons/")
        {
            if (itemIcons.TryGetValue(id, out var s)) return s;
            var tex = Resources.Load<Texture2D>(folder + id);
            s = tex != null ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * .5f, 100) : null;
            itemIcons[id] = s; return s;
        }

        private void ItemsPopup(RectTransform panel, float w, float h)
        {
            PopupTitle(panel, w, "Home items"); ClosePopup(panel, w, Resume);
            var room = Game.HomeRoom;
            const int columns = 3;
            float cell = (w - 28) / columns, cellHeight = cell + 30;
            int rows = (COgheHomeItems.Catalog.Length + columns - 1) / columns;
            var viewport = art.Rect(panel, "Viewport", new Rect(14, 74, w - 28, h - 88)); viewport.gameObject.AddComponent<RectMask2D>();
            var content = art.Rect(viewport, "Content", new Rect(0, 0, w - 28, rows * cellHeight + 8));
            var scroll = panel.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content;
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
            for (int i = 0; i < COgheHomeItems.Catalog.Length; i++)
            {
                var entry = COgheHomeItems.Catalog[i];
                var item = room?.Find(entry.id);
                bool open = room != null && item != null && room.Present(item), sale = room != null && item != null && room.ForSale(item);
                float x = (i % columns) * cell, y = (i / columns) * cellHeight;
                var box = art.Box(content, "Item " + entry.id, new Rect(x + 4, y + 4, cell - 8, cellHeight - 8), open || sale ? new Color(.995f, .995f, .972f) : new Color(.93f, .94f, .91f), true);
                box.pixelsPerUnitMultiplier = 2f;
                var button = box.gameObject.AddComponent<Button>(); button.targetGraphic = box;
                string id = entry.id; button.onClick.AddListener(() => { COgheAudio.UiTap(); ChooseItem(id); });
                var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                var ir = icon.rectTransform; ir.SetParent(box.transform, false); ir.anchorMin = ir.anchorMax = new Vector2(0, 1); ir.pivot = new Vector2(0, 1);
                float size = cell - 30; ir.anchoredPosition = new Vector2((cell - 8 - size) * .5f, -4); ir.sizeDelta = new Vector2(size, size);
                icon.sprite = ItemIcon(entry.id); icon.preserveAspect = true; icon.raycastTarget = false;
                icon.color = open || sale ? Color.white : new Color(1, 1, 1, .45f);
                art.Label(box.transform, "Name", entry.name, new Rect(2, size - 2, cell - 12, 18), 11);
                if (open) art.Label(box.transform, "State", "Play", new Rect(2, size + 13, cell - 12, 16), 10, COgheUIArt.Teal);
                else if (sale)
                {
                    // for sale (Mrk 02/10): bought with Drops, then it appears in the room
                    var price = art.Label(box.transform, "State", COgheEconomy.Price(entry.id).ToString(), new Rect(14, size + 13, cell - 26, 16), 11, COgheUIArt.Teal);
                    price.font = art.BoldFont; DropIcon(box.transform, new Rect((cell - 8) * .5f - 22, size + 13, 14, 14));
                }
                else
                {
                    art.Icon(box.transform, COgheIcon.Lock, new Rect(cell - 34, 8, 18, 18), COgheUIArt.Muted);
                    art.Label(box.transform, "State", "Level " + entry.level, new Rect(2, size + 13, cell - 12, 16), 10, COgheUIArt.Muted);
                }
            }
        }

        private void ChooseItem(string id)
        {
            var room = Game.HomeRoom; var item = room?.Find(id);
            if (item == null) { Resume(); return; }
            if (room.Present(item)) { Resume(); Game.Personality?.PlayWith(item); }
            else if (room.ForSale(item))
            {
                // buy it: it pops into the room and COghe runs to it
                room.ShowGhost(item, 6);
                OfferItem(id, () => { room.HideGhost(); Game.Personality?.RevealNew(); });
            }
            else { Resume(); room.ShowGhost(item); Notify(item.Name + " unlocks at level " + item.UnlockLevel); }
        }

        /// <summary>Home view rotation (Mrk: turn the room like a level): degrees around the room, from a drag.</summary>
        public float HomeYaw => homeYaw;
        /// <summary>Home "Zoom in": the camera stays close on COghe and follows it.</summary>
        public bool HomeZoom => homeZoom;
        private float homeYaw; private bool homeZoom;
        public void OrbitHome(float pixels) { homeYaw = Mathf.Repeat(homeYaw - pixels / Mathf.Max(1, Screen.width) * 240 + 180, 360) - 180; }
        public void ToggleHomeZoom() { homeZoom = !homeZoom; Rebuild(); }

        /// <summary>The Home camera: the whole room seen from the player's chosen side, leaning in on the item in play or
        /// the one being previewed, or close on COghe while zoomed in.</summary>
        private bool FrameHome(Camera camera)
        {
            var room = Game.HomeRoom; if (room == null) return false;
            camera.orthographic = true; camera.aspect = (float)Screen.width / Screen.height;
            var view = Quaternion.Euler(42, -6 + homeYaw, 0);
            // back from the Style stage: turn smoothly to the room view (a drag turns the room at once)
            if (homeEase > 0) { homeEase -= Time.unscaledDeltaTime; view = Quaternion.Slerp(camera.transform.rotation, view, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 4)); }
            camera.transform.rotation = view;
            Vector3 overview = room.Root.position + Vector3.up * .05f + Vector3.forward * .02f;
            // phones are narrow: the width decides; on wider screens the room's depth (1.36 m on screen) plus the UI bars does.
            // Turned, the room's outline on screen changes: scale both fits by how far its corners now reach.
            Extents(room, camera.transform.rotation, out float across, out float upward);
            Extents(room, Quaternion.Euler(42, -6, 0), out float acrossFront, out float upwardFront);
            float fitWidth = (COgheHomeRoom.HalfWidth * 2 + .22f) * .5f / camera.aspect * across / acrossFront, fitDepth = 1.08f * upward / upwardFront;
            float size = Mathf.Max(fitWidth, fitDepth), targetSize = size; Vector3 target = overview;
            float rate = 2.2f;
            var focus = room.Ghost ?? (homeZoom ? null : Game.Personality?.Playing);
            if (focus != null)
            {
                var b = room.BoundsOf(focus);
                target = Vector3.Lerp(b.center, Game.Motion.Centre(0), .35f); targetSize = Mathf.Max(.3f, size * .45f);
            }
            else if (homeZoom)
            {
                target = (Game.Personality != null ? Game.Personality.SkinCentre : Game.Motion.Centre(0)) + Vector3.up * .02f;
                targetSize = .18f; rate = 3.5f;
            }
            float blend = homeSize < 0 ? 1 : 1 - Mathf.Exp(-Time.unscaledDeltaTime * rate);
            homeFocus = Vector3.Lerp(homeSize < 0 ? target : homeFocus, target, blend);
            homeSize = Mathf.Lerp(homeSize < 0 ? targetSize : homeSize, targetSize, blend);
            camera.orthographicSize = homeSize;
            float logicalY = (84 + height - 160) * .5f;   // the middle of the room's free area: under the top bar, over the buttons
            float pixelY = Screen.safeArea.yMax - logicalY * canvas.scaleFactor, centre = pixelY / Screen.height;
            camera.transform.position = homeFocus - camera.transform.forward * 1.2f - camera.transform.up * ((centre - .5f) * 2 * camera.orthographicSize);
            camera.nearClipPlane = .01f; camera.farClipPlane = 30;
            // seen from behind, the back wall would stand between the camera and the room
            room.SetBackWallVisible(room.Root.InverseTransformDirection(camera.transform.forward).z > .2f);
            return true;
        }

        /// <summary>How far the room's box reaches from its centre along the view's right and up axes.</summary>
        private static void Extents(COgheHomeRoom room, Quaternion view, out float across, out float upward)
        {
            Vector3 right = view * Vector3.right, up = view * Vector3.up, centre = room.Root.TransformPoint(new Vector3(0, COgheHomeRoom.WallHeight * .5f, 0));
            across = upward = 0;
            for (int i = 0; i < 8; i++)
            {
                var corner = room.Root.TransformPoint(new Vector3(i % 2 == 0 ? -COgheHomeRoom.HalfWidth : COgheHomeRoom.HalfWidth,
                    (i / 2) % 2 == 0 ? 0 : COgheHomeRoom.WallHeight, i / 4 == 0 ? -COgheHomeRoom.HalfDepth : COgheHomeRoom.HalfDepth)) - centre;
                across = Mathf.Max(across, Mathf.Abs(Vector3.Dot(corner, right))); upward = Mathf.Max(upward, Mathf.Abs(Vector3.Dot(corner, up)));
            }
        }

        private void DisposeItemIcons()
        {
            foreach (var s in itemIcons.Values) if (s != null) Destroy(s);
            itemIcons.Clear();
        }
    }
}
