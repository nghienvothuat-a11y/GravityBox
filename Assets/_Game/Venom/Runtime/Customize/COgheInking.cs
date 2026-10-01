using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>
    /// Inks injected into COghe (Mrk 01/10: hold a syringe on the body, the colour spreads through the liquid; release and
    /// COghe keeps it). Each of the 32 particles carries its share of up to four inks, so the colour travels with the
    /// liquid: it swirls as COghe moves and splits with it. Holding longer injects more and reaches wider; afterwards the
    /// inks keep diffusing between neighbouring particles for a moment, then settle. Presentation only: no forces.
    /// </summary>
    public sealed class COgheInking : MonoBehaviour
    {
        public const int Slots = 4;
        /// <summary>The ink in each slot (null: empty).</summary>
        public readonly string[] Inks = new string[Slots];
        /// <summary>Each particle's share of each slot's ink (sum ≤ 1; the rest is COghe's own dark liquid).</summary>
        public readonly Vector4[] Amount = new Vector4[CohesiveOrganism.ParticleCount];
        private VenomCampaign game;
        private VenomSurface surface;
        private Material original, skin, filaments;
        private float active, rinse;
        private bool dirty = true;
        public bool Transparent { get; private set; }

        public static COgheInking Attach(VenomCampaign game, int seed = 1)
        {
            var surface = game.Matter.GetComponent<VenomSurface>();
            var a = surface.GetComponent<COgheInking>() ?? surface.gameObject.AddComponent<COgheInking>();
            a.game = game; a.surface = surface; surface.ParticleInk = a.Amount;
            a.Build(seed);
            return a;
        }

        private void Build(int seed)
        {
            if (skin != null) return;
            var shader = Resources.Load<Shader>("COgheInk/InkSkin");
            original = surface.SkinRenderer.sharedMaterial;
            skin = new Material(shader) { name = "COghe ink skin" };
            if (original.HasProperty("_BaseColor")) skin.SetColor("_BaseColor", original.GetColor("_BaseColor"));
            if (original.HasProperty("_Metallic")) skin.SetFloat("_BaseMetallic", original.GetFloat("_Metallic"));
            if (original.HasProperty("_Smoothness")) skin.SetFloat("_BaseSmoothness", original.GetFloat("_Smoothness"));
            skin.SetVector("_BodyCentre", new Vector4(0, 0, 0, seed * 13.37f % 97));
            filaments = new Material(skin) { name = "COghe ink filaments" }; filaments.SetFloat("_UseMeanInk", 1);
            foreach (var r in surface.GetComponentsInChildren<Renderer>(true))
                if (r.sharedMaterial == original) r.sharedMaterial = r == surface.SkinRenderer ? skin : filaments;
        }

        private void OnDestroy()
        {
            if (surface != null)
            {
                surface.ParticleInk = null;
                foreach (var r in surface.GetComponentsInChildren<Renderer>(true)) if (r.sharedMaterial == skin || r.sharedMaterial == filaments) r.sharedMaterial = original;
            }
            if (skin != null) Destroy(skin); if (filaments != null) Destroy(filaments);
        }

        /// <summary>The slot holding this ink, taking a free one (or <paramref name="replace"/>, when all four are used).</summary>
        public int SlotFor(string ink, int replace = -1)
        {
            for (int s = 0; s < Slots; s++) if (Inks[s] == ink) return s;
            for (int s = 0; s < Slots; s++) if (Inks[s] == null) { Inks[s] = ink; dirty = true; return s; }
            if (replace < 0) return -1;   // the player chooses which colour goes (the UI asks)
            for (int i = 0; i < Amount.Length; i++) Amount[i][replace] = 0;
            Inks[replace] = ink; dirty = true; return replace;
        }

        /// <summary>One frame of holding the syringe at <paramref name="world"/>; <paramref name="held"/>: seconds so far.</summary>
        public bool Inject(Vector3 world, string ink, float dt, float held)
        {
            int slot = SlotFor(ink); if (slot < 0) return false;
            float reach = .016f + .022f * Mathf.Clamp01(held / 2.5f);   // holding longer reaches wider
            for (int i = 0; i < Amount.Length; i++)
            {
                float d = (surface.DrawnParticles[i] - world).magnitude, w = Mathf.Exp(-(d / reach) * (d / reach));
                if (w < .01f) continue;
                var a = Amount[i]; float mine = Mathf.Min(1, a[slot] + 1.3f * w * dt);
                float others = a.x + a.y + a.z + a.w - a[slot], room = 1 - mine;
                if (others > room && others > 1e-5f) a *= room / others;   // the new ink displaces the others (and the dark liquid first)
                a[slot] = mine; Amount[i] = a;
            }
            active = 1.6f; rinse = 0;
            return true;
        }

        /// <summary>Wash every ink out (over a short moment); accessories are untouched.</summary>
        public void Rinse() { rinse = 1; active = 0; }

        private void LateUpdate()
        {
            if (game == null || surface == null) return;
            float dt = Mathf.Min(Time.deltaTime, .05f);
            if (rinse > 0)
            {
                float k = Mathf.Exp(-dt * 5);
                for (int i = 0; i < Amount.Length; i++) Amount[i] *= k;
                rinse -= dt * 1.2f;
                if (rinse <= 0) { System.Array.Clear(Amount, 0, Amount.Length); for (int s = 0; s < Slots; s++) Inks[s] = null; dirty = true; }
            }
            Diffuse(dt);
            active = Mathf.Max(0, active - dt);
            if (dirty) Apply();
            // the marbling's frame of reference rides with the body; it stirs faster while ink flows in
            Vector3 centre = Vector3.zero; Vector4 inks = Vector4.zero;
            for (int i = 0; i < Amount.Length; i++) { centre += surface.DrawnParticles[i]; inks += Amount[i]; }
            centre /= Amount.Length; inks /= Amount.Length;
            var c = skin.GetVector("_BodyCentre"); c.x = centre.x; c.y = centre.y; c.z = centre.z;
            skin.SetVector("_BodyCentre", c); filaments.SetVector("_BodyCentre", c);
            filaments.SetVector("_MeanInk", inks);
            float stir = active > 0 ? .55f : .12f; skin.SetFloat("_Stir", stir); filaments.SetFloat("_Stir", stir);
            SetTransparent(ClearShare(inks) > .02f);
        }

        // neighbouring particles share their inks (fast just after an injection, then almost still)
        private readonly Vector4[] delta = new Vector4[CohesiveOrganism.ParticleCount];
        private void Diffuse(float dt)
        {
            float rate = active > 0 ? .9f : .05f;
            var bodies = game.Matter.Bodies;
            for (int i = 0; i < Amount.Length; i++)
            {
                Vector4 sum = Vector4.zero; float weight = 0;
                for (int j = 0; j < Amount.Length; j++)
                {
                    if (i == j) continue;
                    float d = (bodies[i].position - bodies[j].position).magnitude; if (d > .045f) continue;
                    float k = Mathf.Exp(-(d / .024f) * (d / .024f)); sum += (Amount[j] - Amount[i]) * k; weight += k;
                }
                delta[i] = weight > 0 ? sum / weight * Mathf.Min(1, rate * dt) * .5f : Vector4.zero;
            }
            for (int i = 0; i < Amount.Length; i++) Amount[i] += delta[i];
        }

        private float ClearShare(Vector4 mean)
        {
            float clear = 0;
            for (int s = 0; s < Slots; s++) { var ink = COgheInks.Find(Inks[s]); if (ink != null) clear += mean[s] * ink.Surface.w; }
            return clear;
        }

        private void Apply()
        {
            dirty = false;
            for (int s = 0; s < Slots; s++)
            {
                var ink = COgheInks.Find(Inks[s]);
                foreach (var m in new[] { skin, filaments })
                {
                    m.SetColor("_InkColor" + s, ink != null ? ink.Color : Color.black);
                    m.SetColor("_InkAccent" + s, ink != null ? ink.Accent : Color.black);
                    m.SetVector("_InkSurface" + s, ink != null ? ink.Surface : new Vector4(0, .7f, 0, 0));
                    m.SetVector("_InkFx" + s, ink != null ? ink.Fx : Vector4.zero);
                }
            }
        }

        private void SetTransparent(bool clear)
        {
            if (clear == Transparent && skin.renderQueue == (clear ? (int)RenderQueue.Transparent : (int)RenderQueue.Geometry)) return;
            Transparent = clear;
            foreach (var m in new[] { skin, filaments })
            {
                // premultiplied alpha keeps the wet highlights bright on a clear body
                m.SetFloat("_SrcBlend", (float)BlendMode.One); m.SetFloat("_DstBlend", clear ? (float)BlendMode.OneMinusSrcAlpha : (float)BlendMode.Zero);
                m.SetFloat("_ZWrite", 1);
                if (clear) m.EnableKeyword("_ALPHAPREMULTIPLY_ON"); else m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                m.renderQueue = clear ? (int)RenderQueue.Transparent : (int)RenderQueue.Geometry;
                m.SetOverrideTag("RenderType", clear ? "Transparent" : "Opaque");
            }
        }
    }
}
