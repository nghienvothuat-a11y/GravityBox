using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>One piece of Home furniture: when it unlocks, where COghe stands to use it, and its built model.</summary>
    public sealed class COgheHomeItem
    {
        public string Id, Name;
        public int UnlockLevel;
        /// <summary>Local approach point (W units, +Z back; the item faces -Z).</summary>
        public Vector3 Approach, SecondApproach;
        public bool HasSecondApproach;
        public GameObject Root;
        /// <summary>Named moving groups (pivots already at their joints).</summary>
        public readonly Dictionary<string, Transform> Parts = new Dictionary<string, Transform>();
        public Transform Part(string name) => Parts.TryGetValue(name, out var t) ? t : null;
        public Vector3 World(Vector3 localW) => Root.transform.TransformPoint(localW);
    }

    /// <summary>
    /// The 14 Home items (Codex, OUTBOX/COGHE_HOME_ITEMS_2026_09_30/ITEMS.json), built in W units (COghe's width, 10 cm):
    /// +Y up, front -Z, origin at the floor centre. Familiar things unlock first; the ball opens the Home (Mrk: the most
    /// playful toy first, at level 10; the bed moves to 14).
    /// </summary>
    public static class COgheHomeItems
    {
        public const float W = .1f;   // COghe's relaxed width at Home (measured on the live skin: ~10-12 cm, not the 7 cm first briefed)
        public static readonly (string id, string name, int level)[] Catalog =
        {
            ("BALL", "Ball", 10), ("DUMBBELL", "Dumbbell", 12), ("BED", "Bed", 14), ("MIRROR", "Mirror", 16),
            ("SWING", "Swing", 18), ("SLIDE", "Slide", 20), ("TV", "TV", 23), ("XYLOPHONE", "Xylophone", 26),
            ("TRAMPOLINE", "Trampoline", 30), ("HAMMOCK", "Hammock", 34), ("WHEEL", "Running wheel", 38),
            ("AQUARIUM", "Aquarium", 42), ("SHADOW_LAMP", "Shadow lamp", 46), ("TROPHY", "Trophy", 50),
        };
        private const string Ivory = "#e8e0cf", Slate = "#304e56", Teal = "#356d6d", Mint = "#d8e7dd", Coral = "#bc7770", Amber = "#dfba70", Blue = "#afcbd5";

        public static COgheHomeItem Build(string id, Transform parent, COgheHomeMaterials materials)
        {
            var item = new COgheHomeItem { Id = id };
            foreach (var c in Catalog) if (c.id == id) { item.Name = c.name; item.UnlockLevel = c.level; }
            item.Root = new GameObject("Home item " + id);
            item.Root.transform.SetParent(parent, false);
            item.Root.transform.localScale = Vector3.one * W;
            var k = new COgheLowPoly();
            Transform Group(string name, Vector3 pivot)
            {
                var g = new GameObject(name).transform; g.SetParent(item.Root.transform, false); g.localPosition = pivot;
                item.Parts[name] = g; return g;
            }
            void Bake(string name, Transform into = null) => k.Bake(name, into ?? item.Root.transform, materials);
            switch (id)
            {
                case "BED":
                    k.Paint(Slate).Cylinder(1.1f, .8f, 0, .11f, 12);
                    k.Paint(Ivory).Cylinder(1.08f, .78f, .055f, .195f, 12);
                    Bake("Bed frame");
                    var cushion = Group("Cushion", new Vector3(0, .14f, 0));
                    k.At(new Vector3(0, -.14f, 0)).Paint(Coral).Puck(.8f, .5f, .13f, .25f, 12, .04f);
                    k.Loft(COgheLowPoly.EllipsePath(.93f, .63f, 12, .29f), true, new[] { new Vector2(-.14f, -.11f), new Vector2(.14f, -.11f), new Vector2(.1f, .11f), new Vector2(-.1f, .11f) }, Vector3.up);
                    Bake("Cushion", cushion);
                    item.Approach = new Vector3(0, 0, -1.45f);
                    break;
                case "DUMBBELL":
                    var weights = Group("Weights", new Vector3(0, .3f, 0));
                    k.Paint(Slate).Rod(new Vector3(-.55f, 0, 0), new Vector3(.55f, 0, 0), .08f, 12);
                    k.Paint(Coral).Rod(new Vector3(-.7f, 0, 0), new Vector3(-.44f, 0, 0), .3f, 6); k.Rod(new Vector3(.44f, 0, 0), new Vector3(.7f, 0, 0), .3f, 6);
                    k.Paint(Ivory).Rod(new Vector3(-.435f, 0, 0), new Vector3(-.365f, 0, 0), .11f, 12); k.Rod(new Vector3(.365f, 0, 0), new Vector3(.435f, 0, 0), .11f, 12);
                    Bake("Dumbbell", weights);
                    item.Approach = new Vector3(0, 0, -1.05f);
                    break;
                case "BALL":
                    var ball = Group("Ball", new Vector3(0, .45f, 0));
                    Color teal = COgheLowPoly.Hex(Teal), ivory = COgheLowPoly.Hex(Ivory), amber = COgheLowPoly.Hex(Amber);
                    k.Paint(teal).Ico(Vector3.zero, Vector3.one * .45f, 1, n =>
                    {
                        float a = Mathf.Atan2(n.z, n.x) * Mathf.Rad2Deg + Mathf.Asin(Mathf.Clamp(n.y, -1, 1)) * 35;
                        a = Mathf.Repeat(a, 360);
                        return a < 150 ? teal : a < 230 ? ivory : amber;
                    });
                    Bake("Ball", ball);
                    item.Approach = new Vector3(0, 0, -1.1f);
                    break;
                case "MIRROR":
                    k.Paint(Teal).Cylinder(.525f, .375f, 0, .14f, 12);
                    k.Box(new Vector3(0, .37f, .06f), new Vector3(.2f, .6f, .2f), .03f);
                    k.Loft(new[] { new Vector3(-.86f, 1.45f, .05f), new Vector3(-.86f, .66f, .05f), new Vector3(-.62f, .44f, .05f), new Vector3(0, .4f, .05f),
                        new Vector3(.62f, .44f, .05f), new Vector3(.86f, .66f, .05f), new Vector3(.86f, 1.45f, .05f) }, false, COgheLowPoly.Rect(.1f, .13f), Vector3.back);
                    Bake("Mirror stand");
                    var glass = Group("Mirror", new Vector3(0, 1.43f, 0));
                    k.Paint(Ivory).Loft(OvalXY(.735f, .905f, 16), true, COgheLowPoly.Rect(.13f, .16f), Vector3.forward);
                    k.Paint(Blue).Ellipse(new Vector3(0, 0, -.085f), .67f, .84f, 16);
                    k.Paint("#e6f1f3").Quad(new Vector3(-.42f, -.1f, -.09f), new Vector3(-.3f, .46f, -.09f), new Vector3(-.12f, .46f, -.09f), new Vector3(-.24f, -.1f, -.09f));
                    k.Quad(new Vector3(-.1f, -.35f, -.09f), new Vector3(.02f, .2f, -.09f), new Vector3(.1f, .2f, -.09f), new Vector3(-.02f, -.35f, -.09f));
                    Bake("Mirror glass", glass);
                    item.Approach = new Vector3(0, 0, -1.15f);
                    break;
                case "SWING":
                    k.Paint(Ivory);
                    foreach (float x in new[] { -1.12f, 1.12f }) { k.Rod(new Vector3(x, 0, -.65f), new Vector3(x, 2.84f, 0), .07f, 8); k.Rod(new Vector3(x, 0, .65f), new Vector3(x, 2.84f, 0), .07f, 8); }
                    k.Paint(Slate); foreach (float x in new[] { -1.12f, 1.12f }) foreach (float z in new[] { -.65f, .65f }) k.At(new Vector3(x, 0, z)).Cylinder(.1f, .1f, 0, .06f, 8);
                    k.At(Vector3.zero).Paint(Teal).Rod(new Vector3(-1.25f, 2.88f, 0), new Vector3(1.25f, 2.88f, 0), .1f, 12);
                    Bake("Swing frame");
                    var seat = Group("Seat", new Vector3(0, 2.78f, 0));
                    k.Paint(Slate).Rod(new Vector3(-.62f, 0, 0), new Vector3(-.62f, -2f, 0), .0275f, 6); k.Rod(new Vector3(.62f, 0, 0), new Vector3(.62f, -2f, 0), .0275f, 6);
                    k.Paint(Coral).Box(new Vector3(0, -2.08f, 0), new Vector3(1.5f, .16f, .75f), .04f);
                    Bake("Swing seat", seat);
                    item.Approach = new Vector3(0, 0, -1.45f);
                    break;
                case "SLIDE":
                {
                    var line = new[] { new Vector3(0, 2.42f, 1.5f), new Vector3(0, 2.3f, 1.02f), new Vector3(0, 1.75f, .3f), new Vector3(0, .9f, -.6f), new Vector3(0, .18f, -1.55f), new Vector3(0, .12f, -2f) };
                    k.Paint(Coral).Loft(line, false, COgheLowPoly.Rect(1.02f, .12f), Vector3.up);
                    var left = new Vector3[line.Length]; var right = new Vector3[line.Length];
                    for (int i = 0; i < line.Length; i++) { left[i] = line[i] + new Vector3(-.57f, .1f, 0); right[i] = line[i] + new Vector3(.57f, .1f, 0); }
                    k.Paint(Ivory).Loft(left, false, COgheLowPoly.Rect(.12f, .16f), Vector3.up); k.Loft(right, false, COgheLowPoly.Rect(.12f, .16f), Vector3.up);
                    k.Paint(Teal);
                    foreach (float x in new[] { -.45f, .45f }) k.Box(new Vector3(x, 1.2f, 1.62f), new Vector3(.13f, 2.4f, .13f), .02f);
                    foreach (float y in new[] { .45f, .95f, 1.45f, 1.95f }) k.Rod(new Vector3(-.45f, y, 1.62f), new Vector3(.45f, y, 1.62f), .045f, 6);
                    k.Paint(Slate); foreach (float x in new[] { -.6f, .6f }) foreach (float z in new[] { -1.8f, 1.8f }) k.At(new Vector3(x, 0, z)).Cylinder(.08f, .08f, 0, .14f, 8);
                    k.At(Vector3.zero); Bake("Slide");
                    item.Approach = new Vector3(0, 0, 2.45f); item.SecondApproach = new Vector3(0, 0, -2.65f); item.HasSecondApproach = true;
                    break;
                }
                case "TV":
                    k.Paint(Ivory).Box(new Vector3(0, .15f, 0), new Vector3(2.5f, .18f, .8f), .03f);
                    k.Paint(Slate).Box(new Vector3(-.9f, .08f, 0), new Vector3(.2f, .16f, .6f)); k.Box(new Vector3(.9f, .08f, 0), new Vector3(.2f, .16f, .6f));
                    k.Paint(Ivory).Box(new Vector3(0, 1.02f, 0), new Vector3(2.3f, 1.5f, .48f), .08f);
                    k.Paint(Teal).Box(new Vector3(0, 1.03f, -.248f), new Vector3(1.9f, 1.13f, .025f), .01f);
                    k.Paint(Coral).Rod(new Vector3(1.03f, .5f, -.25f), new Vector3(1.03f, .5f, -.3f), .06f, 8);
                    Bake("TV");
                    var a = Group("ShapeA", new Vector3(-.3f, 1.1f, -.265f)); k.Paint(Blue).Ellipse(Vector3.zero, .36f, .28f, 10); Bake("Screen shape A", a);
                    var b = Group("ShapeB", new Vector3(.38f, .88f, -.266f)); k.Paint(Amber).Ellipse(Vector3.zero, .22f, .18f, 10); Bake("Screen shape B", b);
                    item.Approach = new Vector3(0, 0, -1.35f);
                    break;
                case "XYLOPHONE":
                {
                    k.Paint(Ivory).Box(new Vector3(0, .12f, -.3f), new Vector3(2.2f, .12f, .1f)); k.Box(new Vector3(0, .12f, .3f), new Vector3(2.2f, .12f, .1f));
                    k.Paint(Slate); foreach (float x in new[] { -1f, 1f }) foreach (float z in new[] { -.32f, .32f }) k.Box(new Vector3(x, .045f, z), new Vector3(.16f, .09f, .16f));
                    Bake("Xylophone frame");
                    float[] xs = { -.875f, -.525f, -.175f, .175f, .525f, .875f }, lengths = { 1, .92f, .84f, .76f, .68f, .6f };
                    string[] colours = { Coral, Amber, Mint, Teal, Blue, Coral };
                    for (int i = 0; i < 6; i++)
                    {
                        var key = Group("Key" + i, new Vector3(xs[i], .24f, 0));
                        k.Paint(colours[i]).Box(Vector3.zero, new Vector3(.28f, .13f, lengths[i]), .03f); Bake("Key " + i, key);
                    }
                    for (int m = 0; m < 2; m++)
                    {
                        var mallet = Group(m == 0 ? "MalletL" : "MalletR", new Vector3(m == 0 ? -.45f : .45f, .1f, -.5f));
                        k.Paint(Slate).Rod(new Vector3(-.3f, 0, 0), new Vector3(.3f, 0, 0), .025f, 6);
                        k.Paint(Amber).Ico(new Vector3(m == 0 ? .34f : -.34f, 0, 0), Vector3.one * .08f, 0);
                        Bake("Mallet", mallet);
                    }
                    item.Approach = new Vector3(0, 0, -1.15f);
                    break;
                }
                case "TRAMPOLINE":
                    k.Paint(Teal).Loft(COgheLowPoly.EllipsePath(1.09f, 1.09f, 12, .56f), true, COgheLowPoly.Rect(.225f, .18f), Vector3.up);
                    k.Paint(Ivory);
                    for (int i = 0; i < 6; i++) { float a6 = i * Mathf.PI / 3 + .26f; var p = new Vector3(Mathf.Cos(a6) * 1.02f, 0, Mathf.Sin(a6) * 1.02f); k.Rod(p, p + Vector3.up * .5f, .065f, 8); }
                    k.Paint(Slate);
                    for (int i = 0; i < 6; i++) { float a6 = i * Mathf.PI / 3 + .26f; k.At(new Vector3(Mathf.Cos(a6) * 1.02f, 0, Mathf.Sin(a6) * 1.02f)).Cylinder(.085f, .085f, 0, .09f, 8); }
                    k.At(Vector3.zero); Bake("Trampoline frame");
                    var mat = Group("Mat", new Vector3(0, .54f, 0));
                    // a very shallow cone: scaling the group's Y sinks the centre
                    k.Paint(Slate).Lathe(new[] { new Vector2(0, -.012f), new Vector2(.98f, 0) }, 12);
                    Bake("Trampoline mat", mat);
                    item.Approach = new Vector3(0, 0, -1.85f);
                    break;
                case "HAMMOCK":
                    k.Paint(Ivory).Rod(new Vector3(-1.75f, 0, 0), new Vector3(-1.75f, 1.6f, 0), .09f, 8); k.Rod(new Vector3(1.75f, 0, 0), new Vector3(1.75f, 1.6f, 0), .09f, 8);
                    k.Paint(Slate).Box(new Vector3(-1.75f, .07f, 0), new Vector3(.5f, .14f, 1.4f), .03f); k.Box(new Vector3(1.75f, .07f, 0), new Vector3(.5f, .14f, 1.4f), .03f);
                    k.Paint(Teal).Box(new Vector3(0, .16f, 0), new Vector3(3.5f, .14f, .16f));
                    Bake("Hammock posts");
                    var sling = Group("Sling", new Vector3(0, 1.38f, 0));
                    k.At(new Vector3(0, -1.38f, 0)).Paint(Coral).Loft(new[] { new Vector3(-1.55f, 1.35f, 0), new Vector3(-.8f, .83f, 0), new Vector3(0, .64f, 0), new Vector3(.8f, .83f, 0), new Vector3(1.55f, 1.35f, 0) },
                        false, COgheLowPoly.Rect(1.05f, .05f), Vector3.up);
                    k.Paint(Amber).Rod(new Vector3(-1.72f, 1.38f, 0), new Vector3(-1.55f, 1.38f, 0), .075f, 8); k.Rod(new Vector3(1.55f, 1.38f, 0), new Vector3(1.72f, 1.38f, 0), .075f, 8);
                    Bake("Hammock sling", sling);
                    item.Approach = new Vector3(0, 0, -1.25f);
                    break;
                case "WHEEL":
                    k.Paint(Ivory).Box(new Vector3(0, .73f, .63f), new Vector3(.22f, 1.46f, .18f), .03f);
                    k.Paint(Slate).Box(new Vector3(0, .08f, .15f), new Vector3(1.9f, .16f, 1.25f), .03f);
                    k.Paint(Coral).Rod(new Vector3(0, 1.4f, .5f), new Vector3(0, 1.4f, .66f), .13f, 12);
                    Bake("Wheel stand");
                    var drum = Group("Drum", new Vector3(0, 1.4f, 0));
                    k.Paint(Teal).Loft(OvalXY(1.105f, 1.105f, 16), true, COgheLowPoly.Rect(.19f, 1.05f), Vector3.forward);
                    k.Paint(Ivory).Loft(OvalXY(1.0f, 1.0f, 16), true, COgheLowPoly.Rect(.02f, .98f), Vector3.forward);
                    for (int i = 0; i < 8; i++) { float a8 = i * Mathf.PI / 4; var p = new Vector3(Mathf.Cos(a8) * 1.105f, Mathf.Sin(a8) * 1.105f, -.535f); k.At(p, Quaternion.Euler(0, 0, a8 * Mathf.Rad2Deg)).Box(Vector3.zero, new Vector3(.19f, .07f, .02f)); }
                    k.At(Vector3.zero); Bake("Wheel drum", drum);
                    item.Approach = new Vector3(0, 0, -1.2f);
                    break;
                case "AQUARIUM":
                {
                    k.Paint(Ivory).Box(new Vector3(0, .12f, 0), new Vector3(2.5f, .24f, 1.4f), .04f);
                    k.Box(new Vector3(0, 1.54f, -.655f), new Vector3(2.5f, .12f, .09f)); k.Box(new Vector3(0, 1.54f, .655f), new Vector3(2.5f, .12f, .09f));
                    k.Box(new Vector3(-1.205f, 1.54f, 0), new Vector3(.09f, .12f, 1.4f)); k.Box(new Vector3(1.205f, 1.54f, 0), new Vector3(.09f, .12f, 1.4f));
                    k.Paint(Slate).Ico(new Vector3(-.7f, .3f, .2f), new Vector3(.14f, .08f, .12f), 0); k.Ico(new Vector3(-.5f, .29f, .05f), new Vector3(.11f, .07f, .1f), 0); k.Ico(new Vector3(-.35f, .29f, .25f), new Vector3(.12f, .07f, .1f), 0);
                    k.Paint(Teal).Quad(new Vector3(.62f, .24f, .3f), new Vector3(.56f, .7f, .3f), new Vector3(.72f, .78f, .3f), new Vector3(.74f, .24f, .3f));
                    k.Quad(new Vector3(.74f, .24f, .3f), new Vector3(.72f, .78f, .3f), new Vector3(.56f, .7f, .3f), new Vector3(.62f, .24f, .3f));
                    k.Quad(new Vector3(.76f, .24f, .32f), new Vector3(.86f, .62f, .32f), new Vector3(.95f, .6f, .32f), new Vector3(.84f, .24f, .32f));
                    k.Quad(new Vector3(.84f, .24f, .32f), new Vector3(.95f, .6f, .32f), new Vector3(.86f, .62f, .32f), new Vector3(.76f, .24f, .32f));
                    Bake("Aquarium");
                    var fish = Group("Fish", new Vector3(0, .88f, 0));
                    k.Paint(Amber).Ico(Vector3.zero, new Vector3(.26f, .125f, .08f), 0);
                    k.Tri(new Vector3(.22f, 0, 0), new Vector3(.4f, .12f, 0), new Vector3(.4f, -.12f, 0)); k.Tri(new Vector3(.22f, 0, 0), new Vector3(.4f, -.12f, 0), new Vector3(.4f, .12f, 0));
                    Bake("Fish", fish);
                    // quiet glass last (transparent): four walls and the water surface
                    var panes = new COgheLowPoly();
                    panes.Paint(new Color(.69f, .8f, .84f, .16f));
                    panes.Quad(new Vector3(-1.175f, .24f, -.625f), new Vector3(-1.175f, 1.5f, -.625f), new Vector3(1.175f, 1.5f, -.625f), new Vector3(1.175f, .24f, -.625f));
                    panes.Quad(new Vector3(1.175f, .24f, .625f), new Vector3(1.175f, 1.5f, .625f), new Vector3(-1.175f, 1.5f, .625f), new Vector3(-1.175f, .24f, .625f));
                    panes.Quad(new Vector3(-1.175f, .24f, .625f), new Vector3(-1.175f, 1.5f, .625f), new Vector3(-1.175f, 1.5f, -.625f), new Vector3(-1.175f, .24f, -.625f));
                    panes.Quad(new Vector3(1.175f, .24f, -.625f), new Vector3(1.175f, 1.5f, -.625f), new Vector3(1.175f, 1.5f, .625f), new Vector3(1.175f, .24f, .625f));
                    panes.Paint(new Color(.69f, .8f, .84f, .2f)).Quad(new Vector3(-1.15f, 1.4f, -.6f), new Vector3(-1.15f, 1.4f, .6f), new Vector3(1.15f, 1.4f, .6f), new Vector3(1.15f, 1.4f, -.6f));
                    panes.Bake("Aquarium glass", item.Root.transform, materials, true);
                    item.Approach = new Vector3(0, 0, -1.45f);
                    break;
                }
                case "SHADOW_LAMP":
                    k.Paint(Slate).Cylinder(.35f, .35f, 0, .16f, 12);
                    k.Paint(Ivory).Rod(new Vector3(0, .12f, .05f), new Vector3(0, .84f, -.02f), .065f, 8);
                    k.Paint(Teal).Rod(new Vector3(.2f, .86f, 0), new Vector3(.3f, .86f, 0), .1f, 8);
                    Bake("Lamp stand");
                    var head = Group("Head", new Vector3(0, .86f, 0));
                    var aim = Quaternion.Euler(25, 0, 0);   // tipped up toward the wall it lights
                    k.At(aim * new Vector3(0, .1f, -.08f), aim * Quaternion.Euler(-90, 0, 0)).Paint(Ivory).Cylinder(.175f, .175f, -.275f, .275f, 12, .31f / .175f);
                    k.At(aim * new Vector3(0, .1f, -.365f), aim).Paint(Amber).Ellipse(Vector3.zero, .25f, .25f, 12);
                    k.At(Vector3.zero); Bake("Lamp head", head);
                    item.Approach = new Vector3(0, 0, -1.1f);
                    break;
                case "TROPHY":
                    k.Paint(Slate).Box(new Vector3(0, .11f, 0), new Vector3(.72f, .22f, .6f), .03f);
                    k.Paint(Ivory).Cylinder(.09f, .09f, .22f, .66f, 8);
                    k.Paint(Amber).Lathe(new[] { new Vector2(0, .6f), new Vector2(.12f, .6f), new Vector2(.3f, .8f), new Vector2(.36f, 1.05f), new Vector2(.36f, 1.2f),
                        new Vector2(.295f, 1.2f), new Vector2(.295f, 1.06f), new Vector2(.24f, .84f), new Vector2(0, .72f) }, 12);
                    foreach (float s in new[] { -1f, 1f })
                        k.Loft(new[] { new Vector3(s * .33f, 1.08f, 0), new Vector3(s * .5f, 1.04f, 0), new Vector3(s * .54f, .9f, 0), new Vector3(s * .44f, .76f, 0), new Vector3(s * .27f, .72f, 0) },
                            false, COgheLowPoly.Rect(.08f, .08f), Vector3.forward);
                    Bake("Trophy");
                    item.Approach = new Vector3(0, 0, -1.05f);
                    break;
            }
            return item;
        }

        /// <summary>An oval path in the XY plane (facing -Z), for frames and drums.</summary>
        private static Vector3[] OvalXY(float rx, float ry, int segments)
        {
            var p = new Vector3[segments];
            for (int i = 0; i < segments; i++) { float a = (i + .5f) * Mathf.PI * 2 / segments; p[i] = new Vector3(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry, 0); }
            return p;
        }
    }
}
