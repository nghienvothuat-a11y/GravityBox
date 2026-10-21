using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// The mystery gift box seen in a level (Mrk 21/10/2026): Codex's gift, 468 triangles (Mrk: under 500) in a body, a
    /// lid and a bow riding on the lid (Resources/COgheMysteryGift; source, storyboard and timings in
    /// Tools/MysteryGift/HANDOFF.md). It drops near the exit, wiggles now and then until COghe reaches it, then opens
    /// (the lid lifts away from COghe, two small confetti bursts, the prize card rises out of it) or flies away.
    /// Renderers only: no collider, rigidbody or light, so taps, paths and the puzzle never see it. Every animation runs
    /// on unscaled time because the offer popup pauses the level.
    /// </summary>
    public sealed class COgheMysteryBox : MonoBehaviour
    {
        public enum State { Falling, Idle, Opening, Leaving, Gone }
        public State Phase { get; private set; }
        /// <summary>Where it rests (world): the bottom centre of the box.</summary>
        public Vector3 Rest { get; private set; }
        /// <summary>Body width, the storyboard's W (the lid is 5.7 cm wide, the bow peaks 7 cm up).</summary>
        public const float Size = .052f;
        public const string Model = "COgheMysteryGift/COghe_Gift";
        /// <summary>Seconds after Open: the prize has risen and held (the reveal popup takes over), the box is gone.</summary>
        public const float ShowTime = 1.7f, OpenTime = 2f;
        const float DropHeight = 3 * Size, FallTime = .48f, Wiggle = 3.2f, Settled = .38f;
        // Codex's three materials (his linear Blender colours as sRGB) on the level's own lit material.
        static readonly (string name, string colour, float smoothness, float metallic)[] Paints =
        {
            ("Gift_Coral", "#E75C73", .58f, 0), ("Gift_Peach", "#FE959D", .62f, 0), ("Gift_Champagne", "#FDCF70", .65f, .25f),
        };

        COgheHomeMaterials materials; Material lit;
        readonly Dictionary<string, Material> paints = new Dictionary<string, Material>();
        Transform pivot, lid, glints;
        readonly List<Transform> confetti = new List<Transform>();
        Vector3 lidRest, away = Vector3.right; Quaternion lidRestRotation;
        float clock;
        // The prize: a world-space card over the box facing the camera (Codex's billboard), so it sits right in any view.
        const float CardWidth = .056f, CardHeight = .068f, Units = 10000;   // canvas units of 0.1 mm
        RectTransform prize; CanvasGroup prizeAlpha, prizeName; Image prizePicture; Camera view;

        /// <summary>The box mouth (world), where the prize comes out.</summary>
        public Vector3 Mouth => Rest + transform.up * .04f;

        /// <param name="toCamera">The way the close-up camera looks from (world): the bow's tails face it, three-quarter on.</param>
        public void Initialize(Material source, Vector3 restWorld, Vector3 surfaceNormal, Vector3 toCamera)
        {
            Rest = restWorld; Vector3 up = surfaceNormal.sqrMagnitude > .5f ? surfaceNormal.normalized : Vector3.up;
            Vector3 front = Vector3.ProjectOnPlane(toCamera, up); if (front.sqrMagnitude < 1e-6f) front = Vector3.ProjectOnPlane(Vector3.back, up);
            transform.SetPositionAndRotation(restWorld, Quaternion.LookRotation(front.normalized, up) * Quaternion.Euler(0, 30, 0));
            lit = new Material(source); materials = new COgheHomeMaterials(lit, lit);
            pivot = new GameObject("Box pivot").transform; pivot.SetParent(transform, false);
            var gift = Instantiate(Resources.Load<GameObject>(Model), pivot, false).transform; gift.name = "Gift";
            lid = gift.Find("Gift_Lid"); lid.SetParent(pivot, true); lidRest = lid.localPosition; lidRestRotation = lid.localRotation;
            foreach (var r in pivot.GetComponentsInChildren<MeshRenderer>(true))
            {
                var slots = r.sharedMaterials; for (int i = 0; i < slots.Length; i++) slots[i] = Paint(slots[i]); r.sharedMaterials = slots;
            }
            // Two small glints at the lid seam: the wiggle and the moment before it opens.
            glints = new GameObject("Glints").transform; glints.SetParent(pivot, false);
            foreach (var at in new[] { new Vector3(.027f, .039f, .02f), new Vector3(-.022f, .04f, .027f) })
            {
                var g = new COgheLowPoly(); g.Paint("#FFF6C8").Ico(Vector3.zero, new Vector3(.0042f, .0042f, .0042f), 0);
                g.Bake("Glint", glints, materials).transform.localPosition = at;
            }
            // Twelve confetti flakes in two side bursts (gold, coral, ivory), hidden until it opens.
            var colours = new[] { "#FDCF70", "#E75C73", "#FFF4DE" };
            for (int i = 0; i < 12; i++)
            {
                var k = new COgheLowPoly(); k.Paint(colours[i % 3]).Box(Vector3.zero, new Vector3(.0046f, .0006f, .0032f), 0);
                var flake = k.Bake("Confetti " + i, transform, materials).transform; flake.gameObject.SetActive(false); confetti.Add(flake);
            }
            foreach (var r in GetComponentsInChildren<Renderer>(true)) { r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false; }
            Phase = State.Falling; clock = 0; Pose(0);
        }

        Material Paint(Material imported)
        {
            string name = imported != null ? imported.name : "";
            var paint = Paints[0];
            foreach (var p in Paints) if (name.StartsWith(p.name)) paint = p;
            if (paints.TryGetValue(paint.name, out var m)) return m;
            ColorUtility.TryParseHtmlString(paint.colour, out var colour);
            m = new Material(lit) { name = paint.name };
            m.SetColor("_BaseColor", colour); m.SetFloat("_Smoothness", paint.smoothness); m.SetFloat("_Metallic", paint.metallic);
            m.SetColor("_EmissionColor", Color.black); m.DisableKeyword("_EMISSION");
            return paints[paint.name] = m;
        }

        /// <summary>The lid lifts away from <paramref name="creature"/> (world), so COghe never hides the opening.</summary>
        public void LiftAwayFrom(Vector3 creature)
        {
            Vector3 d = transform.InverseTransformDirection(Rest - creature); d.y = 0;
            if (d.sqrMagnitude > 1e-8f) away = d.normalized;
        }
        /// <summary>Opens, then shows the prize rising out of it: its real icon and name (or Drops) on an ivory card.</summary>
        public void Open(Sprite icon, string label, bool drops, COgheUIArt art, Camera camera)
        {
            if (Phase == State.Opening || Phase == State.Gone) return;
            Phase = State.Opening; clock = 0; view = camera;
            if (art != null) BuildPrize(icon, label, drops, art);
        }

        void BuildPrize(Sprite icon, string label, bool drops, COgheUIArt art)
        {
            var go = new GameObject("Prize", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            prize = (RectTransform)go.transform; prize.SetParent(transform, false);
            go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            prize.sizeDelta = new Vector2(CardWidth, CardHeight) * Units; prize.pivot = new Vector2(.5f, 0); prize.localScale = Vector3.zero;
            prizeAlpha = go.GetComponent<CanvasGroup>(); prizeAlpha.blocksRaycasts = false; prizeAlpha.interactable = false;
            float w = CardWidth * Units, h = CardHeight * Units;
            art.Box(prize, "Card", new Rect(0, 0, w, h), COgheUIArt.Paper).pixelsPerUnitMultiplier = .27f;   // ~1.2 cm corners
            var picture = art.Rect(prize, "Icon", drops ? new Rect(w * .2f, w * .14f, w * .6f, w * .6f) : new Rect(w * .1f, w * .06f, w * .8f, w * .8f)).gameObject.AddComponent<Image>();
            prizePicture = picture; picture.sprite = icon; picture.preserveAspect = true; picture.raycastTarget = false; if (icon == null) picture.color = COgheUIArt.Ink;
            var name = art.Label(prize, "Name", label, new Rect(w * .04f, w * .88f, w * .92f, h - w * .9f), 66, drops ? COgheUIArt.Teal : COgheUIArt.Ink);
            name.font = art.BoldFont; prizeName = name.gameObject.AddComponent<CanvasGroup>();
        }
        public void FlyAway() { if (Phase == State.Leaving || Phase == State.Gone) return; Phase = State.Leaving; clock = 0; }
        public bool Landed => Phase != State.Falling;

        void Update()
        {
            clock += Mathf.Min(Time.unscaledDeltaTime, .05f);
            Pose(clock);
            if (Phase == State.Gone) Destroy(gameObject);
        }

        static float Smooth(float t) { t = Mathf.Clamp01(t); return t * t * (3 - 2 * t); }
        static float CubicOut(float t) { t = 1 - Mathf.Clamp01(t); return 1 - t * t * t; }
        static float SoftBack(float t) { t = Mathf.Clamp01(t) - 1; return 1 + 2 * t * t * t + t * t; }   // one ~4% overshoot

        void Pose(float t)
        {
            if (pivot == null) return;
            Vector3 one = Vector3.one, squash = new Vector3(1.06f, .92f, 1.06f);
            pivot.localPosition = Vector3.zero; pivot.localRotation = Quaternion.identity; pivot.localScale = one;
            Vector3 lidOffset = Vector3.zero; Quaternion lidTilt = Quaternion.identity; float glint = 0;
            switch (Phase)
            {
                case State.Falling:
                {
                    float u = Mathf.Clamp01(t / FallTime);
                    pivot.localPosition = Vector3.up * DropHeight * (1 - u * u);   // ease-in fall, base along the normal
                    pivot.localRotation = Quaternion.Euler(0, -18 * (1 - u), 0);
                    if (u >= 1) { Phase = State.Idle; clock = 0; }
                    break;
                }
                case State.Idle:
                {
                    // Land: squash (1.10, .87) then rebound (.96, 1.07) in .12 s, settle by .38 s with the lid tilting
                    // 4° → -2° → 0. Then every 3.2 s: 2.35 s still, a ±3° rock on the bottom edge, the lid lifts a little.
                    if (t < .06f) pivot.localScale = Vector3.Lerp(one, new Vector3(1.10f, .87f, 1.10f), Smooth(t / .06f));
                    else if (t < .12f) pivot.localScale = Vector3.Lerp(new Vector3(1.10f, .87f, 1.10f), new Vector3(.96f, 1.07f, .96f), Smooth((t - .06f) / .06f));
                    else if (t < Settled) pivot.localScale = Vector3.Lerp(new Vector3(.96f, 1.07f, .96f), one, Smooth((t - .12f) / .26f));
                    float settleTilt = t < .12f ? 4 * Smooth(t / .12f) : t < .25f ? Mathf.Lerp(4, -2, Smooth((t - .12f) / .13f)) : Mathf.Lerp(-2, 0, Smooth((t - .25f) / .13f));
                    lidTilt = Quaternion.AngleAxis(settleTilt, Vector3.right);
                    if (t < Settled) break;
                    float cycle = Mathf.Repeat(t - Settled, Wiggle);
                    if (cycle >= 2.35f && cycle < 2.65f)
                    {
                        float a = 3 * Mathf.Sin((cycle - 2.35f) / .30f * Mathf.PI * 2);
                        var rock = Quaternion.AngleAxis(a, Vector3.forward);
                        var edge = new Vector3(-Mathf.Sign(a) * Size * .5f, 0, 0);   // rocks on the edge that stays down
                        pivot.localRotation = rock; pivot.localPosition = edge - rock * edge;
                    }
                    if (cycle >= 2.65f && cycle < 3f) lidOffset = Vector3.up * .018f * Size * Mathf.Sin((cycle - 2.65f) / .35f * Mathf.PI);
                    if (cycle >= 2.35f && cycle < 3f) glint = Mathf.Sin((cycle - 2.35f) / .65f * Mathf.PI);
                    break;
                }
                case State.Opening:
                {
                    // Anticipation .24 s (squash, lid shiver, seam glint), release .24–.55 (lid .85 W up and .2 W away,
                    // up to 18° tilt, cubic out; body back to size with one soft overshoot), confetti .50–1.15, hold,
                    // then shrink away 1.7–2.0 once the reveal popup has taken over.
                    Vector3 lift = Vector3.up * .85f * Size + away * .20f * Size; Vector3 tiltAxis = Vector3.Cross(Vector3.up, away);
                    if (t < .24f)
                    {
                        float k = Smooth(t / .24f);
                        pivot.localScale = Vector3.Lerp(one, squash, k);
                        lidTilt = Quaternion.AngleAxis(3 * k * Mathf.Sin(t * Mathf.PI * 2 * 9), Vector3.forward); glint = k;
                    }
                    else
                    {
                        float r = CubicOut((t - .24f) / .31f);
                        pivot.localScale = Vector3.LerpUnclamped(squash, one, SoftBack((t - .24f) / .31f));
                        lidOffset = lift * r + (t > .55f ? Vector3.up * .002f * Mathf.Sin((t - .55f) * 4) : Vector3.zero);
                        lidTilt = Quaternion.AngleAxis(18 * r, tiltAxis);
                        glint = Mathf.Clamp01(1 - (t - .24f) / .2f);
                    }
                    if (t > ShowTime) pivot.localScale *= 1 - Smooth((t - ShowTime) / (OpenTime - ShowTime));
                    PoseConfetti(t - .5f); PosePrize(t);
                    if (t >= OpenTime) Phase = State.Gone;
                    break;
                }
                case State.Leaving:
                {
                    // Skip: shrinks gently while lifting one W, no confetti and no prize.
                    float s = Smooth(t / .45f);
                    pivot.localPosition = Vector3.up * Size * s; pivot.localScale = one * (1 - s);
                    if (t >= .45f) Phase = State.Gone;
                    break;
                }
            }
            lid.localPosition = lidRest + lidOffset; lid.localRotation = lidTilt * lidRestRotation;
            glints.gameObject.SetActive(glint > .01f);
            if (glint > .01f) for (int i = 0; i < glints.childCount; i++)
                glints.GetChild(i).localScale = Vector3.one * glint * (.7f + .3f * Mathf.Sin(Time.unscaledTime * 9 + i * 2));
        }

        /// <summary>The prize card's picture while it shows (world centre and width): where the reveal panel's picture flies from.</summary>
        public bool PrizePicture(out Vector3 centre, out float width)
        {
            centre = default; width = 0;
            if (prizePicture == null) return false;
            var r = prizePicture.rectTransform; centre = r.TransformPoint(r.rect.center); width = r.rect.width * r.lossyScale.x;
            return true;
        }

        /// <summary>Codex's timing: hidden to .40 s; rises .40–.90 from .1 W to 1.25 W over the mouth, scale .25 → 1.08 → 1;
        /// the name from .90 s; holds to ShowTime, when the reveal panel opens and the picture flies into it. In front
        /// of the lifted lid (towards the camera), facing it.</summary>
        void PosePrize(float t)
        {
            if (prize == null) return;
            float scale, alpha = 1, rise;
            if (t < .4f) { scale = 0; rise = .1f; }
            else if (t < .9f)
            {
                float u = (t - .4f) / .5f; rise = Mathf.Lerp(.1f, 1.25f, CubicOut(u));
                scale = u < .7f ? Mathf.Lerp(.25f, 1.08f, Mathf.Sin(u / .7f * Mathf.PI * .5f)) : Mathf.Lerp(1.08f, 1, Smooth((u - .7f) / .3f));
            }
            else { rise = 1.25f; scale = 1; alpha = 1 - Mathf.Clamp01((t - ShowTime) / .12f); }   // its picture flies into the panel
            Vector3 toward = view != null ? -view.transform.forward : Vector3.zero;
            prize.position = Mouth + transform.up * rise * Size + toward * .06f;
            if (view != null) prize.rotation = view.transform.rotation;
            prize.localScale = Vector3.one * scale / Units; prizeAlpha.alpha = alpha;
            prizeName.alpha = Mathf.Clamp01((t - .9f) / .15f);
        }

        /// <summary>Two bursts of six flakes from the sides of the mouth, arcing out and falling; gone by .65 s.</summary>
        void PoseConfetti(float t)
        {
            for (int i = 0; i < confetti.Count; i++)
            {
                float age = t - (i < 6 ? 0 : .08f); var flake = confetti[i];
                bool live = age >= 0 && age < .65f; flake.gameObject.SetActive(live); if (!live) continue;
                float side = i < 6 ? -1 : 1, spread = (i % 6) / 5f;
                Vector3 v = new Vector3(side * (.10f + .08f * spread), .24f + .10f * ((i * 7) % 5) / 4f, (spread - .5f) * .10f);
                flake.localPosition = new Vector3(side * .02f, .045f, 0) + v * age + Vector3.down * .45f * age * age;
                flake.localRotation = Quaternion.Euler(age * 720 + i * 40, age * 540 + i * 25, i * 30);
                flake.localScale = Vector3.one * (age > .45f ? 1 - (age - .45f) / .2f : 1);
            }
        }

        void OnDestroy()
        {
            materials?.Dispose(); foreach (var m in paints.Values) Destroy(m); paints.Clear();
            if (lit != null) Destroy(lit);
        }
    }
}
