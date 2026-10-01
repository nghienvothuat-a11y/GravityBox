using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    public enum COgheAccessory { None, Beanie, PartyHat, FlowerCrown, Inclusions }

    /// <summary>
    /// Accessory test (Mrk 01/10: choose between a hat worn on COghe's crest and small things floating inside its body).
    /// Presentation only: nothing here touches particles, forces or puzzle state. It reads where the skin was drawn after
    /// every rebuild (<see cref="VenomSurface.DrawnParticles"/>, <see cref="VenomSurface.SkinTop"/>), so acts, poses and
    /// splits carry the accessory along.
    /// </summary>
    public sealed class COgheAccessories : MonoBehaviour
    {
        private const string Ivory = "#e8e0cf", Teal = "#356d6d", Coral = "#bc7770", Amber = "#dfba70", Rose = "#e3a0a6", Leaf = "#6f9a6a";
        private VenomCampaign game;
        private VenomSurface surface;
        private COgheHomeMaterials materials;
        private Material lit, glow, translucent, skinMaterial;
        private COgheAccessory built = COgheAccessory.None;
        public COgheAccessory Style { get; set; }
        /// <summary>Everything worn or floating hangs under this (no colliders, no rigidbodies).</summary>
        public Transform Holder { get; private set; }

        // the hat: a damped spring onto the crest of the biggest piece
        private Transform hat; private float sink;
        private Vector3 hatPosition, hatTarget, jiggle, jiggleVelocity; private Quaternion hatRotation = Quaternion.identity;
        private float hatShown; private bool hatPlaced;
        public bool HatWorn => hat != null && hatShown > .5f;
        public Vector3 HatPosition => hatPosition;

        // inclusions: each rides a blend of three particles of one piece, re-anchoring now and then
        private sealed class Floater
        {
            public Transform T; public bool Fish; public int A, B, C; public float WA, WB, Phase, Next, Spin;
            public Vector3 Position, Velocity, Heading = Vector3.right; public bool Placed;
        }
        private readonly List<Floater> floaters = new List<Floater>();
        private readonly System.Random rnd = new System.Random(901);
        public int FloaterCount => floaters.Count;
        public Vector3 FloaterPosition(int i) => floaters[i].Position;

        public static COgheAccessories Attach(VenomCampaign game, COgheAccessory style)
        {
            var surface = game.Matter.GetComponent<VenomSurface>();
            var a = surface.GetComponent<COgheAccessories>() ?? surface.gameObject.AddComponent<COgheAccessories>();
            a.game = game; a.surface = surface; a.Style = style;
            surface.Rebuilt -= a.OnRebuilt; surface.Rebuilt += a.OnRebuilt;
            return a;
        }

        private void OnDestroy()
        {
            if (surface != null) { surface.Rebuilt -= OnRebuilt; if (skinMaterial != null) surface.SkinRenderer.sharedMaterial = skinMaterial; }
            Clear(); materials?.Dispose();
            if (lit != null) Destroy(lit); if (glow != null) Destroy(glow); if (translucent != null) Destroy(translucent);
        }

        private void OnRebuilt(VenomSurface s)
        {
            if (built != Style) Build();
            if (Style == COgheAccessory.None) return;
            float dt = Mathf.Min(Time.deltaTime, .05f);
            if (Style == COgheAccessory.Inclusions) StepFloaters(dt); else StepHat(dt);
        }

        // ---- building ---------------------------------------------------------------------------------------------------------
        private void Clear()
        {
            if (hat != null) Destroy(hat.gameObject); hat = null; hatPlaced = false; hatShown = 0;
            foreach (var f in floaters) if (f.T != null) Destroy(f.T.gameObject);
            floaters.Clear();
        }

        private void Build()
        {
            Clear(); built = Style;
            if (Holder == null) { Holder = new GameObject("COghe accessories").transform; Holder.SetParent(transform, false); }
            if (lit == null)
            {
                lit = new Material(game.Matter.Profile.Skin) { name = "Accessory lit" };
                lit.SetFloat("_Metallic", 0); lit.SetFloat("_Smoothness", .35f);
                glow = new Material(lit) { name = "Accessory glow" }; glow.EnableKeyword("_EMISSION");
                materials = new COgheHomeMaterials(lit, lit);
            }
            SetTranslucent(Style == COgheAccessory.Inclusions);
            if (Style == COgheAccessory.None) return;
            if (Style == COgheAccessory.Inclusions) { BuildFloaters(); return; }
            var k = new COgheLowPoly();
            switch (Style)
            {
                case COgheAccessory.Beanie:
                    k.Paint(Teal).Lathe(new[] { new Vector2(.026f, .004f), new Vector2(.025f, .011f), new Vector2(.022f, .018f), new Vector2(.015f, .024f), new Vector2(.007f, .027f), new Vector2(0, .028f) }, 14);
                    k.Paint(Coral).Cylinder(.0275f, .0275f, -.002f, .007f, 14);
                    k.Paint(Ivory).Ico(new Vector3(0, .032f, 0), Vector3.one * .007f, 1);
                    sink = .012f;
                    break;
                case COgheAccessory.PartyHat:
                    k.Paint(Amber).Lathe(new[] { new Vector2(.019f, 0), new Vector2(.013f, .015f) }, 12);
                    k.Paint(Coral).Lathe(new[] { new Vector2(.013f, .015f), new Vector2(.0065f, .03f) }, 12);
                    k.Paint(Amber).Lathe(new[] { new Vector2(.0065f, .03f), new Vector2(0, .046f) }, 12);
                    k.Paint(Teal).Cylinder(.0195f, .0195f, -.001f, .003f, 12);
                    k.Paint(Ivory).Ico(new Vector3(0, .047f, 0), Vector3.one * .006f, 1);
                    sink = .005f;
                    break;
                case COgheAccessory.FlowerCrown:
                    for (int i = 0; i < 7; i++)
                    {
                        float a = i * Mathf.PI * 2 / 7; var c = new Vector3(Mathf.Cos(a) * .024f, .004f, Mathf.Sin(a) * .024f);
                        k.Paint(i % 3 == 0 ? Coral : i % 3 == 1 ? Rose : Ivory);
                        for (int p = 0; p < 5; p++) { float b = p * Mathf.PI * 2 / 5; k.Ico(c + new Vector3(Mathf.Cos(b), .3f, Mathf.Sin(b)) * .0042f, Vector3.one * .0036f, 0); }
                        k.Paint(Amber).Ico(c + Vector3.up * .002f, Vector3.one * .0025f, 0);
                        var n = new Vector3(Mathf.Cos(a + Mathf.PI / 7) * .025f, .001f, Mathf.Sin(a + Mathf.PI / 7) * .025f);
                        k.Paint(Leaf).Ico(n, new Vector3(.005f, .0015f, .0028f), 0);
                    }
                    sink = .008f;
                    break;
            }
            hat = k.Bake("COghe " + Style, Holder, materials).transform;
            hat.gameObject.SetActive(false);
        }

        private void BuildFloaters()
        {
            // two little fish and a handful of star bits (they glow a little, so they read through the tinted liquid)
            // star bits glow through the liquid: drawn additively after the clear skin
            glow.SetFloat("_Surface", 1); glow.SetOverrideTag("RenderType", "Transparent"); glow.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            glow.SetFloat("_SrcBlend", (float)BlendMode.One); glow.SetFloat("_DstBlend", (float)BlendMode.One); glow.SetFloat("_ZWrite", 0);
            glow.renderQueue = (int)RenderQueue.Transparent + 10;
            glow.SetColor("_BaseColor", Color.black); glow.SetColor("_EmissionColor", new Color(1f, .85f, .5f) * 3f);
            for (int i = 0; i < 9; i++)
            {
                bool fish = i < 2; var k = new COgheLowPoly(); GameObject go;
                if (fish)
                {
                    k.Paint(i == 0 ? Coral : Amber).Ico(Vector3.zero, new Vector3(.011f, .006f, .0045f), 1);
                    k.Tri(new Vector3(-.008f, 0, 0), new Vector3(-.017f, .006f, 0), new Vector3(-.017f, -.006f, 0));
                    k.Tri(new Vector3(-.008f, 0, 0), new Vector3(-.017f, -.006f, 0), new Vector3(-.017f, .006f, 0));
                    k.Paint(Ivory).Ico(new Vector3(.0045f, .0012f, .0026f), Vector3.one * .0011f, 0);
                    k.Ico(new Vector3(.0045f, .0012f, -.0026f), Vector3.one * .0011f, 0);
                    go = k.Bake("Inclusion fish", Holder, materials);
                }
                else
                {
                    // a four-point star, both faces
                    k.Paint(Amber);
                    for (int p = 0; p < 4; p++)
                    {
                        float a = p * Mathf.PI / 2; Vector3 tip = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * .0075f;
                        Vector3 l = new Vector3(Mathf.Cos(a + .8f), Mathf.Sin(a + .8f), 0) * .0022f, r = new Vector3(Mathf.Cos(a - .8f), Mathf.Sin(a - .8f), 0) * .0022f;
                        k.Tri(Vector3.zero, r, tip); k.Tri(Vector3.zero, tip, r); k.Tri(Vector3.zero, tip, l); k.Tri(Vector3.zero, l, tip);
                    }
                    go = k.Bake("Inclusion star", Holder, materials);
                    foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = glow;
                }
                foreach (var r in go.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
                var f = new Floater { T = go.transform, Fish = fish, Phase = (float)rnd.NextDouble() * 10, Spin = (float)(rnd.NextDouble() * 2 - 1) * 90 };
                floaters.Add(f);
            }
        }

        private void SetTranslucent(bool on)
        {
            if (skinMaterial == null) skinMaterial = surface.SkinRenderer.sharedMaterial;
            if (on && translucent == null) MakeTranslucent();
            // the skin and every renderer drawn with it (tendrils, limbs)
            foreach (var r in surface.GetComponentsInChildren<Renderer>(true))
                if (r.sharedMaterial == (on ? skinMaterial : translucent) && r.sharedMaterial != null) r.sharedMaterial = on ? translucent : skinMaterial;
        }

        private void MakeTranslucent()
        {
            if (translucent == null)
            {
                // a test tint ("ocean blue, clear") until the ink shader exists: URP Lit switched to alpha blending
                translucent = new Material(skinMaterial) { name = "COghe clear blue (test)" };
                translucent.SetFloat("_Surface", 1); translucent.SetFloat("_Blend", 0); translucent.SetOverrideTag("RenderType", "Transparent");
                translucent.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); translucent.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                translucent.SetFloat("_SrcBlendAlpha", (float)BlendMode.One); translucent.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
                translucent.SetFloat("_ZWrite", 0); translucent.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); translucent.renderQueue = (int)RenderQueue.Transparent;
                translucent.SetColor("_BaseColor", new Color(.1f, .42f, .8f, .45f)); translucent.SetFloat("_Smoothness", .82f); translucent.SetFloat("_Metallic", 0);
                translucent.EnableKeyword("_EMISSION"); translucent.SetColor("_EmissionColor", new Color(.01f, .05f, .12f));
            }
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
            if (hat == null || !Biggest(out int group, out int count, out bool leaving)) return;
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
        }

        // ---- inclusions -------------------------------------------------------------------------------------------------------
        private void StepFloaters(float dt)
        {
            var m = game.Matter; float now = Time.time;
            foreach (var f in floaters)
            {
                int g = m.Groups[f.A];
                bool split = m.Groups[f.B] != g || m.Groups[f.C] != g;
                if (!f.Placed || split || now > f.Next) Anchor(f, g, !f.Placed);
                Vector3 target = surface.DrawnParticles[f.A] * f.WA + surface.DrawnParticles[f.B] * f.WB + surface.DrawnParticles[f.C] * (1 - f.WA - f.WB);
                target += new Vector3(Mathf.Sin(now * .9f + f.Phase), Mathf.Sin(now * 1.3f + f.Phase * 1.7f), Mathf.Cos(now * .7f + f.Phase)) * .0035f;
                if (!f.Placed) { f.Position = target; f.Placed = true; }
                // carried by the liquid: close behind their anchors, never left outside the skin
                Vector3 before = f.Position;
                f.Position = Vector3.Lerp(f.Position, target, 1 - Mathf.Exp(-dt * 18));
                f.Position = target + Vector3.ClampMagnitude(f.Position - target, .005f);
                f.Velocity = (f.Position - before) / Mathf.Max(1e-4f, dt);
                if (f.Velocity.sqrMagnitude > 1e-6f) f.Heading = Vector3.Slerp(f.Heading, f.Velocity.normalized, 1 - Mathf.Exp(-dt * 4));
                f.T.position = f.Position;
                if (f.Fish)
                {
                    var look = Quaternion.LookRotation(Vector3.Cross(f.Heading, Vector3.up).sqrMagnitude > 1e-4f ? Vector3.Cross(f.Heading, Vector3.up) : Vector3.forward, Vector3.up);
                    f.T.rotation = look * Quaternion.Euler(0, Mathf.Sin(now * 9 + f.Phase) * 18, 0);   // tail wiggle
                }
                else
                {
                    f.T.rotation = Quaternion.Euler(0, now * f.Spin, now * f.Spin * .6f);
                    f.T.localScale = Vector3.one * (.75f + .35f * Mathf.Abs(Mathf.Sin(now * 2.3f + f.Phase)));   // twinkle
                }
            }
        }

        private void Anchor(Floater f, int group, bool first)
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
