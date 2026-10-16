using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Short, non-interactive UI accents. One mesh, shared UI material, no lights,
    /// particles, textures or per-frame allocations. Rewards are paid by the shop, never by VFX.</summary>
    public sealed class COgheRewardFx : MaskableGraphic
    {
        public const float FireworkLife = 2.6f, ShineLife = 1.35f;
        private bool fireworks;
        private float began;
        private static readonly Color[] Palette = {
            new Color(.32f,.70f,.59f), new Color(.875f,.729f,.439f),
            new Color(.737f,.467f,.439f), new Color(.686f,.796f,.835f)
        };

        public static COgheRewardFx Fireworks(RectTransform parent, float began)
            => Create(parent, began, true);
        public static COgheRewardFx Shine(RectTransform parent, float began)
            => Create(parent, began, false);

        private static COgheRewardFx Create(RectTransform parent, float start, bool burst)
        {
            if (Time.unscaledTime - start >= (burst ? FireworkLife : ShineLife)) return null;
            var go = new GameObject(burst ? "Unlock fireworks" : "Drop shine", typeof(RectTransform));
            var r = go.GetComponent<RectTransform>(); r.SetParent(parent, false);
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            var fx = go.AddComponent<COgheRewardFx>();
            fx.began = start; fx.fireworks = burst; fx.raycastTarget = false;
            return fx;
        }

        private void Update()
        {
            if (Time.unscaledTime - began > (fireworks ? FireworkLife : ShineLife)) Destroy(gameObject);
            else SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); float time = Time.unscaledTime - began;
            if (time < 0) return;
            Rect r = rectTransform.rect;
            if (!fireworks)
            {
                // A single soft glint sweeps across the drop, then two small warm sparkles fade out.
                float t = Mathf.Clamp01(time / ShineLife);
                float a = Mathf.Sin(t * Mathf.PI);
                var p = r.center + new Vector2(Mathf.Lerp(-.25f,.27f,t) * r.width,
                    Mathf.Lerp(-.18f,.25f,t) * r.height);
                Star(vh, p, r.width * .34f * a, new Color(1,.98f,.84f,a));
                Star(vh, r.center + new Vector2(r.width*.43f,r.height*.34f), 5*a, new Color(.875f,.729f,.439f,a));
                Star(vh, r.center - new Vector2(r.width*.35f,r.height*.22f), 3*a, new Color(.55f,.79f,.71f,a));
                return;
            }
            // Two small fireworks above the sides of the item card, leaving its title/icon readable.
            for (int pop = 0; pop < 2; pop++)
            {
                float age = time - .12f - pop * .32f;
                if (age < 0 || age > 1.85f) continue;
                float t = age / 1.85f, fade = Mathf.SmoothStep(1,0,Mathf.InverseLerp(.35f,1,t));
                Vector2 centre = new Vector2(pop == 0 ? r.xMin + 44 : r.xMax - 44, r.yMax - 35);
                float spread = 64 * (1 - Mathf.Exp(-age * 3));
                for (int i = 0; i < 14; i++)
                {
                    float angle = (i * 360f / 14 + pop * 19) * Mathf.Deg2Rad;
                    var dir = new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                    var c = Palette[(i + pop) % Palette.Length]; c.a = fade;
                    Vector2 p = centre + dir * spread - Vector2.up * (age * age * 17);
                    Star(vh,p,2.8f*fade,c);
                    c.a *= .35f;
                    Segment(vh,p-dir*9,p-dir*2,1.3f,c);
                }
            }
        }

        private static void Star(VertexHelper vh, Vector2 p, float radius, Color c)
        {
            if (radius <= .01f) return;
            Diamond(vh,p,Vector2.right*radius,Vector2.up*radius*.22f,c);
            Diamond(vh,p,Vector2.up*radius,Vector2.right*radius*.22f,c);
        }
        private static void Diamond(VertexHelper vh, Vector2 p, Vector2 a, Vector2 b, Color c)
        {
            int n = vh.currentVertCount;
            vh.AddVert(p-a,c,Vector2.zero); vh.AddVert(p-b,c,Vector2.zero);
            vh.AddVert(p+a,c,Vector2.zero); vh.AddVert(p+b,c,Vector2.zero);
            vh.AddTriangle(n,n+1,n+2); vh.AddTriangle(n,n+2,n+3);
        }
        private static void Segment(VertexHelper vh, Vector2 a, Vector2 b, float width, Color c)
        {
            var d = b-a; var side = new Vector2(-d.y,d.x).normalized * width*.5f;
            int n = vh.currentVertCount;
            vh.AddVert(a-side,c,Vector2.zero); vh.AddVert(a+side,c,Vector2.zero);
            vh.AddVert(b+side,c,Vector2.zero); vh.AddVert(b-side,c,Vector2.zero);
            vh.AddTriangle(n,n+1,n+2); vh.AddTriangle(n,n+2,n+3);
        }
    }
}
