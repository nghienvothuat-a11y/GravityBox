using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>
    /// What COghe wears (Mrk 01/10): one hat on its crest and up to two kinds of little things floating inside its body.
    /// Presentation only: nothing here touches particles, forces or puzzle state. It reads where the skin was drawn after
    /// every rebuild (<see cref="VenomSurface.DrawnParticles"/>, <see cref="VenomSurface.SkinTop"/>), so acts, poses and
    /// splits carry it. Hats pop off small pieces, tubes and the exit and back on when COghe is whole. Inside things are
    /// drawn under the skin: a clear ink shows them, COghe's own dark liquid hides them.
    /// </summary>
    public sealed class COgheAccessories : MonoBehaviour
    {
        private const string Ivory = "#e8e0cf", Teal = "#356d6d", Coral = "#bc7770", Amber = "#dfba70", Rose = "#e3a0a6", Leaf = "#6f9a6a", Slate = "#304e56", Gold = "#e6b850";
        private VenomCampaign game;
        private VenomSurface surface;
        private COgheHomeMaterials materials;
        private Material lit;
        private readonly List<Material> fx = new List<Material>();
        public string Hat { get; private set; } = "";
        public readonly List<string> Inside = new List<string>(COgheWardrobe.MaxInside);
        private bool dirty = true;
        /// <summary>Everything worn or floating hangs under this (no colliders, no rigidbodies).</summary>
        public Transform Holder { get; private set; }

        // the hat: on the crest of the biggest piece, with a little inertial wobble
        private Transform hat, spinner; private float sink;
        private Vector3 hatPosition, hatTarget, jiggle, jiggleVelocity; private Quaternion hatRotation = Quaternion.identity;
        private float hatShown; private bool hatPlaced;
        public bool HatWorn => hat != null && hatShown > .5f;
        public Vector3 HatPosition => hatPosition;

        // inside: each rides a blend of three central particles of its piece, re-anchoring now and then
        private enum Motion { Drift, Swim, Rise, Pulse, Turn }
        private sealed class Floater
        {
            public Transform T; public Motion Kind; public bool Star; public int A, B, C; public float WA, WB, Phase, Next, Spin, Size = 1;
            public Vector3 Position, Velocity, Heading = Vector3.right; public bool Placed;
        }
        private readonly List<Floater> floaters = new List<Floater>();
        private readonly System.Random rnd = new System.Random(901);
        public int FloaterCount => floaters.Count;
        public Vector3 FloaterPosition(int i) => floaters[i].Position;

        public static COgheAccessories Attach(VenomCampaign game)
        {
            var surface = game.Matter.GetComponent<VenomSurface>();
            var a = surface.GetComponent<COgheAccessories>(); if (a == null) a = surface.gameObject.AddComponent<COgheAccessories>();
            a.game = game; a.surface = surface;
            surface.Rebuilt -= a.OnRebuilt; surface.Rebuilt += a.OnRebuilt;
            return a;
        }

        /// <summary>Wear <paramref name="hatId"/> (empty: none) and the inside things listed (at most two).</summary>
        public void Dress(string hatId, IList<string> inside)
        {
            hatId = hatId ?? "";
            bool same = hatId == Hat && inside.Count == Inside.Count;
            for (int i = 0; same && i < inside.Count; i++) same = inside[i] == Inside[i];
            if (same) return;
            Hat = hatId; Inside.Clear();
            for (int i = 0; i < inside.Count && Inside.Count < COgheWardrobe.MaxInside; i++) if (!Inside.Contains(inside[i])) Inside.Add(inside[i]);
            dirty = true;
        }

        private void OnDestroy()
        {
            if (surface != null) surface.Rebuilt -= OnRebuilt;
            Clear(); materials?.Dispose();
            if (lit != null) Destroy(lit);
            foreach (var m in fx) if (m != null) Destroy(m);
            if (Holder != null) Destroy(Holder.gameObject);
        }

        private void OnRebuilt(VenomSurface s)
        {
            if (dirty) Build();
            float dt = Mathf.Min(Time.deltaTime, .05f);
            if (hat != null) StepHat(dt);
            if (floaters.Count > 0) StepFloaters(dt);
        }

        // ---- building ---------------------------------------------------------------------------------------------------------
        private void Clear()
        {
            if (hat != null) Destroy(hat.gameObject);
            hat = spinner = null; hatPlaced = false; hatShown = 0;
            foreach (var f in floaters) if (f.T != null) Destroy(f.T.gameObject);
            floaters.Clear();
            // the previous outfit's materials and meshes go with it (changing clothes on the Style screen must not pile them up)
            foreach (var m in fx) if (m != null) Destroy(m);
            fx.Clear(); materials?.Dispose(); materials = null;
        }

        // inside things draw before the skin (a clear skin tints them); worn glass after it
        private const int UnderSkin = (int)RenderQueue.Transparent - 10, OverSkin = (int)RenderQueue.Transparent + 20;
        private Material Fx(Color color, float rim, float rimAlpha, bool additive, int queue)
        {
            var m = new Material(Resources.Load<Shader>("COgheInk/AccessoryFx")) { name = "Accessory FX" };
            m.SetColor("_Color", color); m.SetFloat("_Rim", rim); m.SetFloat("_RimAlpha", rimAlpha);
            m.SetFloat("_SrcBlend", (float)BlendMode.One); m.SetFloat("_DstBlend", additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
            m.renderQueue = queue; fx.Add(m); return m;
        }

        private void Build()
        {
            dirty = false; Clear();
            if (Holder == null) { Holder = new GameObject("COghe accessories").transform; Holder.SetParent(transform, false); }
            if (lit == null)
            {
                lit = new Material(game.Matter.Profile.Skin) { name = "Accessory lit" };
                lit.SetFloat("_Metallic", 0); lit.SetFloat("_Smoothness", .35f);
            }
            materials = new COgheHomeMaterials(lit, lit);
            if (!string.IsNullOrEmpty(Hat)) BuildHat(Hat);
            foreach (var id in Inside) BuildInside(id);
        }

        private void BuildHat(string id)
        {
            var k = new COgheLowPoly(); Material glassFx = null; COgheLowPoly glass = null, blades = null;
            switch (id)
            {
                case "HAT_BEANIE":
                    k.Paint(Teal).Lathe(new[] { new Vector2(.026f, .004f), new Vector2(.025f, .011f), new Vector2(.022f, .018f), new Vector2(.015f, .024f), new Vector2(.007f, .027f), new Vector2(0, .028f) }, 14);
                    k.Paint(Coral).Cylinder(.0275f, .0275f, -.002f, .007f, 14);
                    k.Paint(Ivory).Ico(new Vector3(0, .032f, 0), Vector3.one * .007f, 1);
                    sink = .012f; break;
                case "HAT_PARTY":
                    k.Paint(Amber).Lathe(new[] { new Vector2(.019f, 0), new Vector2(.013f, .015f) }, 12);
                    k.Paint(Coral).Lathe(new[] { new Vector2(.013f, .015f), new Vector2(.0065f, .03f) }, 12);
                    k.Paint(Amber).Lathe(new[] { new Vector2(.0065f, .03f), new Vector2(0, .046f) }, 12);
                    k.Paint(Teal).Cylinder(.0195f, .0195f, -.001f, .003f, 12);
                    k.Paint(Ivory).Ico(new Vector3(0, .047f, 0), Vector3.one * .006f, 1);
                    sink = .005f; break;
                case "HAT_FLOWER":
                    for (int i = 0; i < 7; i++)
                    {
                        float a = i * Mathf.PI * 2 / 7; var c = new Vector3(Mathf.Cos(a) * .024f, .004f, Mathf.Sin(a) * .024f);
                        k.Paint(i % 3 == 0 ? Coral : i % 3 == 1 ? Rose : Ivory);
                        for (int p = 0; p < 5; p++) { float b = p * Mathf.PI * 2 / 5; k.Ico(c + new Vector3(Mathf.Cos(b), .3f, Mathf.Sin(b)) * .0042f, Vector3.one * .0036f, 0); }
                        k.Paint(Amber).Ico(c + Vector3.up * .002f, Vector3.one * .0025f, 0);
                        var n = new Vector3(Mathf.Cos(a + Mathf.PI / 7) * .025f, .001f, Mathf.Sin(a + Mathf.PI / 7) * .025f);
                        k.Paint(Leaf).Ico(n, new Vector3(.005f, .0015f, .0028f), 0);
                    }
                    sink = .008f; break;
                case "HAT_CAP":
                    k.Paint(Coral).Lathe(new[] { new Vector2(.025f, .002f), new Vector2(.024f, .009f), new Vector2(.02f, .016f), new Vector2(.012f, .021f), new Vector2(0, .023f) }, 14);
                    k.Paint(Ivory).Ico(new Vector3(0, .023f, 0), Vector3.one * .0035f, 0);
                    k.Paint(Slate).Box(new Vector3(0, .003f, .027f), new Vector3(.032f, .0028f, .021f), .001f);   // the visor faces the player
                    sink = .01f; break;
                case "HAT_STRAW":
                    k.Paint(Amber).Cylinder(.046f, .046f, 0, .0032f, 18);
                    k.Cylinder(.022f, .022f, .003f, .022f, 14, .88f);
                    k.Paint(Coral).Cylinder(.0228f, .0228f, .003f, .009f, 14);
                    sink = .008f; break;
                case "HAT_PROPELLER":
                    k.Paint(Teal).Lathe(new[] { new Vector2(.025f, .003f), new Vector2(.023f, .012f), new Vector2(.017f, .019f), new Vector2(.008f, .023f), new Vector2(0, .024f) }, 14);
                    k.Paint(Amber).Cylinder(.0255f, .0255f, -.001f, .006f, 14);
                    k.Paint(Slate).Rod(new Vector3(0, .023f, 0), new Vector3(0, .031f, 0), .0014f, 6);
                    blades = new COgheLowPoly();
                    blades.Paint(Coral).Box(new Vector3(.011f, 0, 0), new Vector3(.02f, .0015f, .006f), .0007f);
                    blades.Paint(Amber).Box(new Vector3(-.011f, 0, 0), new Vector3(.02f, .0015f, .006f), .0007f);
                    blades.Paint(Ivory).Ico(Vector3.zero, Vector3.one * .0026f, 0);
                    sink = .011f; break;
                case "HAT_CROWN":
                    k.Paint(Gold).Cylinder(.022f, .022f, 0, .011f, 15);
                    for (int i = 0; i < 5; i++)
                    {
                        float a = i * Mathf.PI * 2 / 5, w = .3f;
                        Vector3 l = new Vector3(Mathf.Cos(a - w) * .0222f, .011f, Mathf.Sin(a - w) * .0222f), r = new Vector3(Mathf.Cos(a + w) * .0222f, .011f, Mathf.Sin(a + w) * .0222f);
                        Vector3 tip = new Vector3(Mathf.Cos(a) * .0222f, .024f, Mathf.Sin(a) * .0222f);
                        k.Paint(Gold).Tri(l, tip, r); k.Tri(l, r, tip);
                        k.Paint(i % 2 == 0 ? Coral : Teal).Ico(new Vector3(Mathf.Cos(a) * .0232f, .006f, Mathf.Sin(a) * .0232f), Vector3.one * .0026f, 0);
                        k.Paint(Ivory).Ico(tip + Vector3.up * .0015f, Vector3.one * .0018f, 0);
                    }
                    sink = .006f; break;
                case "HAT_ASTRO":
                    k.Paint(Ivory).Cylinder(.031f, .031f, -.002f, .006f, 16);
                    k.Paint(Slate).Rod(new Vector3(.012f, .03f, 0), new Vector3(.016f, .045f, 0), .0011f, 6);
                    k.Paint(Coral).Ico(new Vector3(.0165f, .047f, 0), Vector3.one * .0032f, 0);
                    glass = new COgheLowPoly(); glass.Paint(Color.white).Lathe(new[] { new Vector2(.03f, .004f), new Vector2(.029f, .015f), new Vector2(.024f, .026f), new Vector2(.014f, .033f), new Vector2(0, .036f) }, 18);
                    glassFx = Fx(new Color(.75f, .88f, .95f, .12f), 1.2f, .45f, false, OverSkin);
                    sink = .014f; break;
                default: return;
            }
            hat = k.Bake("COghe " + id, Holder, materials).transform;
            if (glass != null) { var g = glass.Bake("Helmet glass", hat, materials); foreach (var r in g.GetComponentsInChildren<Renderer>()) r.sharedMaterial = glassFx; }
            if (blades != null) { spinner = blades.Bake("Propeller", hat, materials).transform; spinner.localPosition = new Vector3(0, .032f, 0); }
            foreach (var r in hat.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
            hat.gameObject.SetActive(false);
        }

        private void BuildInside(string id)
        {
            switch (id)
            {
                case "FLOAT_STARS":
                {
                    var glow = Fx(new Color(1f, .82f, .45f, 1), 0, 0, true, UnderSkin);
                    for (int i = 0; i < 7; i++)
                    {
                        var k = new COgheLowPoly(); k.Paint(Color.white);
                        for (int p = 0; p < 4; p++)
                        {
                            float a = p * Mathf.PI / 2; Vector3 tip = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * .0068f;
                            Vector3 l = new Vector3(Mathf.Cos(a + .8f), Mathf.Sin(a + .8f), 0) * .002f, r = new Vector3(Mathf.Cos(a - .8f), Mathf.Sin(a - .8f), 0) * .002f;
                            k.Tri(Vector3.zero, r, tip); k.Tri(Vector3.zero, tip, r); k.Tri(Vector3.zero, tip, l); k.Tri(Vector3.zero, l, tip);
                        }
                        Add(k, "Star bit", glow, Motion.Turn, 1).Star = true;
                    }
                    break;
                }
                case "FLOAT_FISH":
                    for (int i = 0; i < 2; i++)
                    {
                        var k = new COgheLowPoly();
                        k.Paint(i == 0 ? Coral : Amber).Ico(Vector3.zero, new Vector3(.011f, .006f, .0045f), 1);
                        k.Tri(new Vector3(-.008f, 0, 0), new Vector3(-.017f, .006f, 0), new Vector3(-.017f, -.006f, 0));
                        k.Tri(new Vector3(-.008f, 0, 0), new Vector3(-.017f, -.006f, 0), new Vector3(-.017f, .006f, 0));
                        k.Paint(Ivory).Ico(new Vector3(.0045f, .0012f, .0026f), Vector3.one * .0011f, 0); k.Ico(new Vector3(.0045f, .0012f, -.0026f), Vector3.one * .0011f, 0);
                        Add(k, "Little fish", null, Motion.Swim, 1);
                    }
                    break;
                case "FLOAT_BUBBLES":
                {
                    var bubble = Fx(new Color(.85f, .95f, 1f, .1f), 1.6f, .7f, false, UnderSkin);
                    for (int i = 0; i < 6; i++) { var k = new COgheLowPoly(); k.Paint(Color.white).Ico(Vector3.zero, Vector3.one * .005f, 1); Add(k, "Bubble", bubble, Motion.Rise, .7f + (float)rnd.NextDouble() * .6f); }
                    break;
                }
                case "FLOAT_JELLY":
                {
                    var jelly = Fx(new Color(.85f, .55f, 1f, .45f), 1.1f, .4f, false, UnderSkin);
                    for (int i = 0; i < 2; i++)
                    {
                        var k = new COgheLowPoly(); k.Paint(Color.white);
                        k.Lathe(new[] { new Vector2(.0075f, 0), new Vector2(.007f, .003f), new Vector2(.0045f, .0065f), new Vector2(0, .0075f) }, 10);
                        for (int t = 0; t < 4; t++) { float a = t * Mathf.PI / 2 + .4f; var top = new Vector3(Mathf.Cos(a) * .004f, 0, Mathf.Sin(a) * .004f); k.Rod(top, top + new Vector3(Mathf.Cos(a) * .0015f, -.011f, Mathf.Sin(a) * .0015f), .0007f, 4, .0002f); }
                        Add(k, "Baby jellyfish", jelly, Motion.Pulse, 1);
                    }
                    break;
                }
                case "FLOAT_PEARLS":
                    for (int i = 0; i < 5; i++) { var k = new COgheLowPoly(); k.Paint(i % 2 == 0 ? Ivory : "#f2dcd8").Ico(Vector3.zero, Vector3.one * .0042f, 1); Add(k, "Tiny pearl", null, Motion.Drift, .8f + (float)rnd.NextDouble() * .4f); }
                    break;
                case "FLOAT_PLANET":
                {
                    var k = new COgheLowPoly();
                    k.Paint(Teal).Ico(Vector3.zero, Vector3.one * .0095f, 1);
                    k.Paint(Amber).At(Vector3.zero, Quaternion.Euler(0, 0, 20)).Cylinder(.016f, .016f, -.0006f, .0006f, 18);
                    k.At(Vector3.zero);
                    Add(k, "Tiny planet", null, Motion.Turn, 1);
                    break;
                }
            }
        }

        private Floater Add(COgheLowPoly k, string name, Material material, Motion kind, float size)
        {
            var go = k.Bake(name, Holder, materials);
            foreach (var r in go.GetComponentsInChildren<Renderer>()) { r.shadowCastingMode = ShadowCastingMode.Off; if (material != null) r.sharedMaterial = material; }
            go.transform.localScale = Vector3.one * size;
            var f = new Floater { T = go.transform, Kind = kind, Size = size, Phase = (float)rnd.NextDouble() * 10, Spin = (float)(rnd.NextDouble() * 2 - 1) * 90 };
            floaters.Add(f); return f;
        }

        // ---- the hat ----------------------------------------------------------------------------------------------------------
        private bool Biggest(out int group, out int count, out bool leaving)
        {
            var m = game.Matter; group = -1; count = 0; leaving = false;
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
            {
                int g = m.Groups[i], n = 0;
                for (int j = 0; j < CohesiveOrganism.ParticleCount; j++) if (m.Groups[j] == g) n++;
                if (n > count) { count = n; group = g; }
            }
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++) if (m.Groups[i] == group && m.Escaped[i]) leaving = true;
            return group >= 0;
        }

        private void StepHat(float dt)
        {
            if (!Biggest(out int group, out int count, out bool leaving)) return;
            var m = game.Matter; Vector3 up = Vector3.up, crest = Vector3.zero; float best = float.NegativeInfinity;
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                if (m.Groups[i] == group) { float h = Vector3.Dot(surface.DrawnParticles[i], up); if (h > best) { best = h; crest = surface.DrawnParticles[i]; } }
            bool found = surface.SkinTop(group, crest, up, .035f, out var top, out var normal);
            // too small a piece, or on its way out / through a tube: the hat pops off and back on when COghe is whole again
            bool wear = found && count >= 12 && !leaving && !game.InTube;
            hatShown = Mathf.MoveTowards(hatShown, wear ? 1 : 0, dt * (wear ? 3.5f : 6));
            Vector3 lean = Vector3.Slerp(up, normal, .45f);
            Vector3 target = top - lean * sink;
            if (!hatPlaced || (target - hatTarget).sqrMagnitude > .15f * .15f) { hatTarget = target; jiggle = jiggleVelocity = Vector3.zero; hatPlaced = true; }
            // attached to the skin; only a little inertia on top (it wobbles when the body hops or squashes, never trails)
            jiggle -= (target - hatTarget) * .35f; hatTarget = target;
            jiggleVelocity += (-jiggle * 420 - jiggleVelocity * 18) * dt; jiggle += jiggleVelocity * dt;
            jiggle = Vector3.ClampMagnitude(jiggle, .012f);
            hatPosition = target + jiggle;
            var view = game.Owner.View;
            Vector3 face = Vector3.ProjectOnPlane(view != null ? -view.transform.forward : Vector3.back, up);
            var want = Quaternion.FromToRotation(Vector3.up, lean) * Quaternion.LookRotation(face.sqrMagnitude > 1e-6f ? face.normalized : Vector3.back, Vector3.up);
            hatRotation = Quaternion.Slerp(hatRotation, want, 1 - Mathf.Exp(-dt * 14));
            float pop = hatShown <= 0 ? 0 : 1 + Mathf.Sin(hatShown * Mathf.PI) * .25f * (1 - hatShown);
            hat.gameObject.SetActive(hatShown > 0);
            hat.SetPositionAndRotation(hatPosition, hatRotation); hat.localScale = Vector3.one * (Mathf.SmoothStep(0, 1, hatShown) * pop);
            if (spinner != null) spinner.localRotation = Quaternion.Euler(0, Time.time * 540, 0);
        }

        // ---- inside -----------------------------------------------------------------------------------------------------------
        private void StepFloaters(float dt)
        {
            var m = game.Matter; float now = Time.time;
            foreach (var f in floaters)
            {
                int g = m.Groups[f.A];
                bool split = m.Groups[f.B] != g || m.Groups[f.C] != g;
                if (!f.Placed || split || now > f.Next) Anchor(f, g);
                Vector3 target = surface.DrawnParticles[f.A] * f.WA + surface.DrawnParticles[f.B] * f.WB + surface.DrawnParticles[f.C] * (1 - f.WA - f.WB);
                target += new Vector3(Mathf.Sin(now * .9f + f.Phase), Mathf.Sin(now * 1.3f + f.Phase * 1.7f), Mathf.Cos(now * .7f + f.Phase)) * .0035f;
                if (f.Kind == Motion.Rise) target += Vector3.up * (Mathf.Repeat(now * .25f + f.Phase, 1) - .5f) * .012f;   // bubbles drift up and start again
                if (!f.Placed) { f.Position = target; f.Placed = true; }
                Vector3 before = f.Position;
                f.Position = Vector3.Lerp(f.Position, target, 1 - Mathf.Exp(-dt * 18));
                f.Position = target + Vector3.ClampMagnitude(f.Position - target, .005f);   // carried by the liquid, never outside it
                f.Velocity = (f.Position - before) / Mathf.Max(1e-4f, dt);
                if (f.Velocity.sqrMagnitude > 1e-6f) f.Heading = Vector3.Slerp(f.Heading, f.Velocity.normalized, 1 - Mathf.Exp(-dt * 4));
                f.T.position = f.Position;
                switch (f.Kind)
                {
                    case Motion.Swim:
                        var side = Vector3.Cross(f.Heading, Vector3.up);
                        f.T.rotation = Quaternion.LookRotation(side.sqrMagnitude > 1e-4f ? side : Vector3.forward, Vector3.up) * Quaternion.Euler(0, Mathf.Sin(now * 9 + f.Phase) * 18, 0);
                        break;
                    case Motion.Turn:
                        f.T.rotation = Quaternion.Euler(0, now * f.Spin, now * f.Spin * .6f);
                        if (f.Star) f.T.localScale = Vector3.one * f.Size * (.75f + .35f * Mathf.Abs(Mathf.Sin(now * 2.3f + f.Phase)));
                        break;
                    case Motion.Pulse:
                        float beat = Mathf.Sin(now * 3 + f.Phase);
                        f.T.rotation = Quaternion.Euler(Mathf.Sin(now * .8f + f.Phase) * 12, 0, 0);
                        f.T.localScale = new Vector3(1 + .1f * beat, 1 - .12f * beat, 1 + .1f * beat) * f.Size;
                        break;
                }
            }
        }

        private void Anchor(Floater f, int group)
        {
            // three of the piece's more central particles, random weights: the floater stays well inside the skin
            var m = game.Matter; var members = new List<int>(32); Vector3 centre = Vector3.zero;
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++) if (m.Groups[i] == group) { members.Add(i); centre += surface.DrawnParticles[i]; }
            centre /= Mathf.Max(1, members.Count);
            members.Sort((a, b) => (surface.DrawnParticles[a] - centre).sqrMagnitude.CompareTo((surface.DrawnParticles[b] - centre).sqrMagnitude));
            int pool = Mathf.Max(1, Mathf.Min(members.Count, Mathf.Max(3, members.Count * 2 / 3)));
            f.A = members[rnd.Next(pool)]; f.B = members[rnd.Next(pool)]; f.C = members[rnd.Next(pool)];
            float a = (float)rnd.NextDouble(), b = (float)rnd.NextDouble() * (1 - a); f.WA = a; f.WB = b;
            f.Next = Time.time + 2.5f + (float)rnd.NextDouble() * 2.5f;
        }
    }
}
