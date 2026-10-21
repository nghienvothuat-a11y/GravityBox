using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>How COghe reacts once a snack is down (Mrk 21/10/2026: "ngon, cay, thích quá…").</summary>
    public enum COgheTaste { Yummy, Spicy, Cold, Sour, Delight }

    /// <summary>
    /// Snacks (Mrk 21/10/2026, five to test): bought with Drops at Home and eaten at once, each with its own way of being
    /// eaten and its own reaction, and the energy it gives back (COgheEnergy: more than the free steel balls, which stop at
    /// 60). Kept out of COgheEconomy.Items (the one-time catalog, its unlocks and Remote Config), as COgheToys.
    /// Models: Resources/COgheSnacks/&lt;id&gt; (Codex) when present, else a low-poly stand-in built here.
    /// </summary>
    public static class COgheSnacks
    {
        public sealed class Snack
        {
            public readonly string Id, Name, Taste; public readonly int Price, Energy; public readonly COgheTaste Kind;
            public Snack(string id, string name, int price, int energy, COgheTaste kind, string taste) { Id = id; Name = name; Price = price; Energy = energy; Kind = kind; Taste = taste; }
        }

        public static readonly Snack[] Catalog =
        {
            // id, name, price in Drops, energy (COgheEnergy), reaction
            new Snack("SNACK_APPLE_PIE", "Apple pie", 10, 20, COgheTaste.Yummy, "Yummy"),
            new Snack("SNACK_CHILLI_CANDY", "Chilli candy", 15, 30, COgheTaste.Spicy, "Spicy"),
            new Snack("SNACK_ICE_CREAM", "Ice cream", 20, 45, COgheTaste.Cold, "Icy"),
            new Snack("SNACK_LEMON_DROP", "Lemon drop", 20, 45, COgheTaste.Sour, "Sour"),
            new Snack("SNACK_STRAWBERRY_CAKE", "Strawberry cake", 30, 75, COgheTaste.Delight, "Lovely"),
        };

        public static Snack Find(string id) { foreach (var s in Catalog) if (s.Id == id) return s; return null; }

        /// <summary>The snack's model under <paramref name="parent"/>, bottom centre at the parent's origin, ~3 cm across.</summary>
        public static GameObject Build(Snack snack, Transform parent, COgheHomeMaterials materials)
        {
            GameObject go;
            var model = Resources.Load<GameObject>("COgheSnacks/" + snack.Id);
            if (model != null)
            {
                go = Object.Instantiate(model, parent, false); go.name = snack.Id;
                foreach (var r in go.GetComponentsInChildren<MeshRenderer>(true))
                {
                    // Codex's materials are named Snack_<Part>_<sRGB hex>: the colour goes onto the room's lit material
                    var slots = r.sharedMaterials;
                    for (int i = 0; i < slots.Length; i++) slots[i] = materials.For(ColourOf(slots[i] != null ? slots[i].name : ""), false);
                    r.sharedMaterials = slots;
                }
            }
            else go = StandIn(snack, parent, materials);
            foreach (var r in go.GetComponentsInChildren<Renderer>(true)) { r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false; }
            return go;
        }

        static Color ColourOf(string materialName)
        {
            int cut = materialName.LastIndexOf('_');
            if (cut >= 0 && ColorUtility.TryParseHtmlString("#" + materialName.Substring(cut + 1, Mathf.Min(6, materialName.Length - cut - 1)), out var c)) return c;
            return new Color(.9f, .55f, .35f);
        }

        /// <summary>A quick low-poly stand-in (until Codex's model is in): readable shape and colours, under 300 triangles.</summary>
        static GameObject StandIn(Snack snack, Transform parent, COgheHomeMaterials materials)
        {
            var k = new COgheLowPoly();
            switch (snack.Kind)
            {
                case COgheTaste.Yummy:      // a round little pie: golden crust, lattice, apple peeking through
                    k.Paint("#E8B04B").Puck(.016f, .016f, 0, .011f, 12, .003f);
                    k.Paint("#C98A2E");
                    foreach (float o in new[] { -.006f, .006f })
                    {
                        k.Box(new Vector3(o, .0115f, 0), new Vector3(.003f, .0015f, .028f));
                        k.Box(new Vector3(0, .012f, o), new Vector3(.028f, .0015f, .003f));
                    }
                    k.Paint("#D9443A").Ico(new Vector3(0, .0115f, 0), new Vector3(.004f, .002f, .004f), 0);
                    break;
                case COgheTaste.Spicy:      // a fat red chilli with a green stem
                    k.Paint("#E53935").Rod(new Vector3(-.013f, .007f, 0), new Vector3(.011f, .011f, 0), .007f, 8, .002f);
                    k.Paint("#43A047").Rod(new Vector3(-.013f, .007f, 0), new Vector3(-.019f, .012f, 0), .0022f, 6);
                    break;
                case COgheTaste.Cold:       // a cone with two scoops
                    k.Paint("#D9A35B").Lathe(new[] { new Vector2(.0005f, 0), new Vector2(.009f, .02f), new Vector2(.0095f, .021f) }, 10);
                    k.Paint("#F8BBD0").Ico(new Vector3(0, .025f, 0), new Vector3(.0105f, .009f, .0105f), 1);
                    k.Paint("#FFF3D6").Ico(new Vector3(0, .033f, 0), new Vector3(.008f, .007f, .008f), 1);
                    break;
                case COgheTaste.Sour:       // a lemon drop in its twisted wrapper
                    k.Paint("#FFE135").Ico(new Vector3(0, .009f, 0), new Vector3(.011f, .009f, .009f), 1);
                    k.Paint("#FFF59D");
                    foreach (float side in new[] { -1f, 1f }) k.Rod(new Vector3(side * .009f, .009f, 0), new Vector3(side * .018f, .009f, 0), .0025f, 6, .006f);
                    break;
                default:                    // a strawberry cake slice: sponge, cream, pink icing, a berry on top
                    k.Paint("#F5D6A8").Box(new Vector3(0, .005f, 0), new Vector3(.026f, .010f, .02f), .0015f);
                    k.Paint("#FFFFFF").Box(new Vector3(0, .0115f, 0), new Vector3(.026f, .003f, .02f), .001f);
                    k.Paint("#F48FB1").Box(new Vector3(0, .0145f, 0), new Vector3(.027f, .003f, .021f), .0012f);
                    k.Paint("#E53935").Ico(new Vector3(0, .02f, 0), new Vector3(.0055f, .0065f, .0055f), 1);
                    k.Paint("#43A047").Ico(new Vector3(0, .026f, 0), new Vector3(.003f, .001f, .003f), 0);
                    break;
            }
            return k.Bake(snack.Id, parent, materials);
        }
    }
}
