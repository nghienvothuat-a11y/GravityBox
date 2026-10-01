using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// COghe's home: a small glass-walled room (0.9 × 1.68 m) with the furniture it has earned. Every item has a fixed
    /// place; the ones on the sides face the central aisle, so COghe walks up and down the middle to reach them. Locked
    /// items stay hidden until the player previews one from the item menu (a ghost in its place).
    /// Built entirely from code (<see cref="COgheHomeItems"/>); destroyed with the room.
    /// </summary>
    public sealed class COgheHomeRoom : IDisposable
    {
        public const float HalfWidth = .45f, HalfDepth = .84f, WallHeight = .32f;
        /// <summary>Tests and previews: treat every item up to this catalog level as earned.</summary>
        public static int? UnlockedLevelOverride;
        private const string SeenKey = "coghe.home.items.seen";
        // where each item stands: x, z (room, metres) and the way its front faces (yaw; 0 = toward the camera)
        // Tall pieces stand at the back and short ones at the front, so the camera (42° down) always sees COghe playing.
        private static readonly Dictionary<string, (float x, float z, float yaw)> Layout = new Dictionary<string, (float, float, float)>
        {
            // back row, facing the camera
            { "SWING", (-.3f, .72f, 0) }, { "TV", (0, .78f, 0) }, { "WHEEL", (.3f, .72f, 0) },
            // left column
            { "MIRROR", (-.3f, .36f, 0) }, { "SHADOW_LAMP", (-.22f, .05f, 90) }, { "HAMMOCK", (-.32f, -.33f, -90) }, { "BED", (-.3f, -.7f, -90) },
            // right column (the slide runs toward the camera; the rest face the aisle)
            { "SLIDE", (.33f, .2f, 0) }, { "AQUARIUM", (.3f, -.3f, 90) }, { "TRAMPOLINE", (.3f, -.57f, 90) }, { "TROPHY", (.34f, -.78f, 90) },
            // the aisle
            { "BALL", (.02f, .28f, 0) }, { "DUMBBELL", (0, -.28f, 0) }, { "XYLOPHONE", (0, -.66f, 0) },
        };

        public readonly Transform Root;
        public readonly List<COgheHomeItem> Items = new List<COgheHomeItem>();
        private readonly VenomCampaign game;
        private readonly COgheHomeMaterials materials;
        private readonly Dictionary<COgheHomeItem, Bounds> bounds = new Dictionary<COgheHomeItem, Bounds>();
        private readonly Dictionary<Renderer, Material[]> realMaterials = new Dictionary<Renderer, Material[]>();
        private COgheHomeItem ghost;
        private float ghostUntil;
        private Renderer backWall;
        private static COgheProductCatalog catalog;

        public COgheHomeRoom(VenomCampaign owner, Transform parent, Material lit, Material glass, Material wall)
        {
            game = owner;
            Root = new GameObject("COghe home room").transform; Root.SetParent(parent, false); Root.localPosition = new Vector3(0, -.3f, 0);
            materials = new COgheHomeMaterials(lit, glass);
            BuildShell(wall);
            foreach (var entry in COgheHomeItems.Catalog)
            {
                var item = COgheHomeItems.Build(entry.id, Root, materials);
                var place = Layout[entry.id];
                item.Root.transform.localPosition = new Vector3(place.x, 0, place.z);
                item.Root.transform.localRotation = Quaternion.Euler(0, place.yaw, 0);
                Items.Add(item);
                var b = new Bounds(item.Root.transform.position, Vector3.zero);
                foreach (var r in item.Root.GetComponentsInChildren<Renderer>()) { b.Encapsulate(r.bounds); realMaterials[r] = r.sharedMaterials; }
                bounds[item] = b;
                item.Root.SetActive(Unlocked(item));
            }
        }

        private void BuildShell(Material wall)
        {
            // floor slab, a back wall and low glass sides: the room reads as a bigger, friendlier version of a puzzle box
            var k = new COgheLowPoly();
            k.Paint("#ece7da").Box(new Vector3(0, -.006f, 0), new Vector3(HalfWidth * 2, .012f, HalfDepth * 2), .003f);
            // the back wall is its own piece: hidden while the player looks at the room from behind it
            var back = new COgheLowPoly();
            back.Paint("#d8e7dd").Box(new Vector3(0, WallHeight * .5f, HalfDepth + .006f), new Vector3(HalfWidth * 2, WallHeight, .012f));
            back.Paint("#c9ddd3").Box(new Vector3(0, .006f, HalfDepth - .004f), new Vector3(HalfWidth * 2, .012f, .008f));
            backWall = back.Bake("Room back wall", Root, materials).GetComponent<Renderer>();
            k.Paint("#e8e0cf");
            foreach (float x in new[] { -HalfWidth, HalfWidth }) foreach (float z in new[] { -HalfDepth, HalfDepth })
                k.Box(new Vector3(x, WallHeight * .35f, z), new Vector3(.012f, WallHeight * .7f, .012f), .002f);
            k.Box(new Vector3(-HalfWidth, .004f, 0), new Vector3(.012f, .008f, HalfDepth * 2)); k.Box(new Vector3(HalfWidth, .004f, 0), new Vector3(.012f, .008f, HalfDepth * 2));
            k.Box(new Vector3(0, .004f, -HalfDepth), new Vector3(HalfWidth * 2, .008f, .012f));
            k.Bake("Room shell", Root, materials);
            var panes = new COgheLowPoly();
            panes.Paint(new Color(.64f, .78f, .82f, .08f));
            float h = WallHeight * .7f;
            panes.Quad(new Vector3(-HalfWidth, 0, HalfDepth), new Vector3(-HalfWidth, h, HalfDepth), new Vector3(-HalfWidth, h, -HalfDepth), new Vector3(-HalfWidth, 0, -HalfDepth));
            panes.Quad(new Vector3(HalfWidth, 0, -HalfDepth), new Vector3(HalfWidth, h, -HalfDepth), new Vector3(HalfWidth, h, HalfDepth), new Vector3(HalfWidth, 0, HalfDepth));
            panes.Bake("Room glass", Root, materials, true);
            if (wall != null) foreach (var r in Root.GetComponentsInChildren<Renderer>()) if (r.name == "Room glass") r.sharedMaterial = wall;
        }

        /// <summary>The camera turned around behind the back wall (Home view rotation): hide it, show it again in front.</summary>
        public void SetBackWallVisible(bool visible) { if (backWall != null && backWall.enabled != visible) backWall.enabled = visible; }
        public bool BackWallVisible => backWall != null && backWall.enabled;

        // Unlocks -----------------------------------------------------------------------------------------------------------
        public bool Unlocked(COgheHomeItem item) => LevelReached(item.UnlockLevel);
        public bool LevelReached(int level)
        {
            if (UnlockedLevelOverride.HasValue) return level <= UnlockedLevelOverride.Value;
            var progress = game.Progress; if (progress == null) return false;
            if (catalog == null) catalog = Resources.Load<COgheProductCatalog>("COgheUI/Catalog");
            if (catalog != null && level <= catalog.Levels.Length && catalog.Levels[level - 1] != null) return progress.Completed.Contains(catalog.Levels[level - 1].Id);
            return progress.Completed.Count >= level;
        }
        /// <summary>Items earned since the last visit (the room reveals them once).</summary>
        public List<COgheHomeItem> Newly()
        {
            var seen = PlayerPrefs.GetString(SeenKey, "");
            var list = new List<COgheHomeItem>();
            foreach (var item in Items) if (Unlocked(item) && !(("," + seen + ",").Contains("," + item.Id + ","))) list.Add(item);
            return list;
        }
        public void MarkSeen(COgheHomeItem item)
        {
            if (!VenomCampaignSave.PersistenceEnabled) return;
            var seen = PlayerPrefs.GetString(SeenKey, "");
            if (("," + seen + ",").Contains("," + item.Id + ",")) return;
            PlayerPrefs.SetString(SeenKey, seen.Length == 0 ? item.Id : seen + "," + item.Id); PlayerPrefs.Save();
        }
        public COgheHomeItem Find(string id) { foreach (var i in Items) if (i.Id == id) return i; return null; }

        // Preview of a locked item: the same model, as a ghost, in its place --------------------------------------------------
        public void ShowGhost(COgheHomeItem item, float seconds = 4.5f)
        {
            HideGhost();
            if (item == null || Unlocked(item)) return;
            ghost = item; ghostUntil = Time.unscaledTime + seconds;
            item.Root.SetActive(true);
            foreach (var r in item.Root.GetComponentsInChildren<Renderer>())
            {
                var mats = new Material[r.sharedMaterials.Length]; for (int i = 0; i < mats.Length; i++) mats[i] = materials.Ghost;
                r.sharedMaterials = mats; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
        public void HideGhost()
        {
            if (ghost == null) return;
            foreach (var r in ghost.Root.GetComponentsInChildren<Renderer>())
                if (realMaterials.TryGetValue(r, out var mats)) { r.sharedMaterials = mats; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; }
            ghost.Root.SetActive(Unlocked(ghost)); ghost = null;
        }
        public COgheHomeItem Ghost => ghost;
        public void Step() { if (ghost != null && Time.unscaledTime > ghostUntil) HideGhost(); }

        // Geometry ------------------------------------------------------------------------------------------------------------
        /// <summary>Where COghe's centre stands to use an item (a move completes within 2.3 cm of its target, so targets
        /// sit at body height, like the Feed snack).</summary>
        public Vector3 ApproachPoint(COgheHomeItem item, bool second = false) => item.World(second && item.HasSecondApproach ? item.SecondApproach : item.Approach) + Root.up * BodyHeight;
        public const float BodyHeight = .021f;
        public Bounds BoundsOf(COgheHomeItem item) => bounds[item];
        /// <summary>The earned item under a screen ray, if any.</summary>
        public COgheHomeItem Pick(Ray ray)
        {
            COgheHomeItem best = null; float nearest = float.MaxValue;
            foreach (var item in Items)
            {
                if (!Unlocked(item) || !item.Root.activeSelf) continue;
                var b = bounds[item]; b.Expand(.01f);
                if (b.IntersectRay(ray, out float d) && d < nearest) { nearest = d; best = item; }
            }
            return best;
        }
        /// <summary>A random free spot in the central aisle for wandering.</summary>
        public Vector3 WanderPoint(System.Random rnd)
        {
            float x = Mathf.Lerp(-.12f, .12f, (float)rnd.NextDouble()), z = Mathf.Lerp(-.55f, .5f, (float)rnd.NextDouble());
            return Root.TransformPoint(new Vector3(x, BodyHeight, z));
        }
        public Vector3 Corner(int i) => Root.TransformPoint(new Vector3(i % 2 == 0 ? -.12f : .12f, BodyHeight, i < 2 ? -.56f : .52f));

        public void Dispose()
        {
            if (Root != null) UnityEngine.Object.Destroy(Root.gameObject);
            materials.Dispose();
        }
    }
}
