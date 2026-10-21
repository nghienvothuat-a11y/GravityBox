using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>
    /// The mystery gift box seen in a level (Mrk 21/10/2026): a small bright box that drops near the exit, hops and
    /// glints until COghe reaches it, then opens or flies away. Built from the low-poly kit like the reading book:
    /// renderers only, no collider, rigidbody or light, so taps, paths and the puzzle never see it. Every animation runs
    /// on unscaled time because the offer popup pauses the level.
    /// </summary>
    public sealed class COgheMysteryBox : MonoBehaviour
    {
        public enum State { Falling, Idle, Opening, Leaving, Gone }
        public State Phase { get; private set; }
        /// <summary>Where it rests (world): the bottom centre of the box.</summary>
        public Vector3 Rest { get; private set; }
        public const float Size = .04f;
        const float DropHeight = .26f, FallTime = .55f;

        COgheHomeMaterials materials; Material lit;
        Transform pivot, body, lid, glints, halo;
        Vector3 up = Vector3.up;
        float clock;

        public void Initialize(Material source, Vector3 restWorld, Vector3 surfaceNormal)
        {
            Rest = restWorld; up = surfaceNormal.sqrMagnitude > .5f ? surfaceNormal.normalized : Vector3.up;
            lit = new Material(source); materials = new COgheHomeMaterials(lit, lit);
            transform.SetPositionAndRotation(restWorld, Quaternion.FromToRotation(Vector3.up, up));
            pivot = new GameObject("Box pivot").transform; pivot.SetParent(transform, false);
            float s = Size, h = s * .5f;
            // Body: magenta with teal dots and a yellow ribbon round it.
            var k = new COgheLowPoly();
            k.Paint("#FF4FA3").Box(new Vector3(0, h * .9f, 0), new Vector3(s, s * .9f, s), .003f);
            k.Paint("#FFD23F").Box(new Vector3(0, h * .9f, 0), new Vector3(s * .22f, s * .905f, s * 1.01f), 0);
            k.Box(new Vector3(0, h * .9f, 0), new Vector3(s * 1.01f, s * .905f, s * .22f), 0);
            k.Paint("#3FD0C9");
            foreach (var d in new[] { new Vector2(-.28f, .3f), new Vector2(.3f, .62f), new Vector2(-.3f, .7f), new Vector2(.28f, .26f) })
                foreach (float side in new[] { -1f, 1f })
                {
                    k.Ico(new Vector3(d.x * s, d.y * s, side * s * .505f), new Vector3(.0032f, .0032f, .0008f), 0);
                    k.Ico(new Vector3(side * s * .505f, d.y * s, d.x * s), new Vector3(.0008f, .0032f, .0032f), 0);
                }
            body = k.Bake("Box body", pivot, materials).transform;
            // Lid with the bow: two loops and a knot.
            var l = new COgheLowPoly();
            l.Paint("#FF7DBE").Box(new Vector3(0, .004f, 0), new Vector3(s * 1.1f, .008f, s * 1.1f), .002f);
            l.Paint("#FFD23F").Box(new Vector3(0, .004f, 0), new Vector3(s * .24f, .0085f, s * 1.12f), 0);
            l.Box(new Vector3(0, .004f, 0), new Vector3(s * 1.12f, .0085f, s * .24f), 0);
            l.Ico(new Vector3(-.0075f, .013f, 0), new Vector3(.0085f, .0055f, .0045f), 1);
            l.Ico(new Vector3(.0075f, .013f, 0), new Vector3(.0085f, .0055f, .0045f), 1);
            l.Paint("#FFB000").Ico(new Vector3(0, .012f, 0), Vector3.one * .0035f, 1);
            lid = l.Bake("Box lid", pivot, materials).transform; lid.localPosition = new Vector3(0, s * .9f, 0);
            // Gold glints that circle the box, and a soft warm spot under it.
            glints = new GameObject("Glints").transform; glints.SetParent(transform, false);
            for (int i = 0; i < 6; i++)
            {
                var g = new COgheLowPoly(); g.Paint(i % 2 == 0 ? "#FFF3A0" : "#FFFFFF").Ico(Vector3.zero, Vector3.one * .0034f, 0);
                var glint = g.Bake("Glint " + i, glints, materials).transform;
                float a = i * Mathf.PI * 2 / 6; glint.localPosition = new Vector3(Mathf.Cos(a) * .042f, .02f + (i % 3) * .012f, Mathf.Sin(a) * .042f);
            }
            var spot = new COgheLowPoly(); spot.Paint("#FFE9A8").At(new Vector3(0, .0015f, 0), Quaternion.Euler(90, 0, 0)).Ellipse(Vector3.zero, .036f, .036f, 20, true);
            halo = spot.Bake("Warm spot", transform, materials).transform;
            foreach (var r in GetComponentsInChildren<Renderer>()) { r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false; }
            Phase = State.Falling; clock = 0; Pose(0);
        }

        public void Open() { if (Phase == State.Opening || Phase == State.Gone) return; Phase = State.Opening; clock = 0; }
        public void FlyAway() { if (Phase == State.Leaving || Phase == State.Gone) return; Phase = State.Leaving; clock = 0; }
        public bool Landed => Phase != State.Falling;

        void Update()
        {
            clock += Mathf.Min(Time.unscaledDeltaTime, .05f);
            Pose(clock);
            if (Phase == State.Gone) Destroy(gameObject);
        }

        void Pose(float t)
        {
            if (pivot == null) return;
            float spin = Time.unscaledTime;
            glints.localRotation = Quaternion.Euler(0, spin * 70, 0);
            for (int i = 0; i < glints.childCount; i++)
                glints.GetChild(i).localScale = Vector3.one * (.6f + .5f * Mathf.Abs(Mathf.Sin(spin * 3.1f + i * 1.3f)));
            halo.localScale = Vector3.one * (1 + .12f * Mathf.Sin(spin * 4));
            switch (Phase)
            {
                case State.Falling:
                {
                    float u = Mathf.Clamp01(t / FallTime);
                    pivot.localPosition = Vector3.up * DropHeight * (1 - u * u);
                    pivot.localRotation = Quaternion.Euler(0, 200 * (1 - u), 0);
                    glints.gameObject.SetActive(u >= 1); halo.gameObject.SetActive(u >= 1);
                    if (u >= 1) { Phase = State.Idle; clock = 0; }
                    break;
                }
                case State.Idle:
                {
                    // A landing squash first, then every 1.6 s a little hop with a shake: "open me".
                    float land = Mathf.Clamp01(t / .35f), squash = (1 - land) * Mathf.Sin(land * Mathf.PI * 3) * .18f;
                    float cycle = Mathf.Repeat(t, 1.6f), hop = cycle < .45f ? Mathf.Sin(cycle / .45f * Mathf.PI) : 0;
                    pivot.localPosition = Vector3.up * hop * .012f;
                    pivot.localScale = new Vector3(1 + squash, 1 - squash, 1 + squash);
                    pivot.localRotation = Quaternion.Euler(0, 0, hop * 9 * Mathf.Sin(cycle * 40));
                    lid.localPosition = new Vector3(0, Size * .9f + hop * .004f, 0); lid.localRotation = Quaternion.identity;
                    break;
                }
                case State.Opening:
                {
                    // The lid pops up and tumbles away, the box swells and vanishes into the reward.
                    float u = Mathf.Clamp01(t / .9f);
                    lid.localPosition = new Vector3(u * .03f, Size * .9f + Mathf.Sin(Mathf.Min(u * 1.4f, 1) * Mathf.PI * .5f) * .07f, 0);
                    lid.localRotation = Quaternion.Euler(0, u * 300, u * 120);
                    float swell = u < .5f ? 1 + u * .5f : Mathf.Lerp(1.25f, 0, (u - .5f) * 2);
                    body.localScale = Vector3.one * swell; lid.localScale = Vector3.one * (u < .7f ? 1 : Mathf.Lerp(1, 0, (u - .7f) / .3f));
                    if (u >= 1) Phase = State.Gone;
                    break;
                }
                case State.Leaving:
                {
                    float u = Mathf.Clamp01(t / .8f);
                    pivot.localPosition = Vector3.up * u * u * .35f; pivot.localRotation = Quaternion.Euler(0, u * 540, 0);
                    pivot.localScale = Vector3.one * (1 - u * .8f); halo.localScale = Vector3.one * (1 - u);
                    if (u >= 1) Phase = State.Gone;
                    break;
                }
            }
        }

        void OnDestroy() { materials?.Dispose(); if (lit != null) Destroy(lit); }
    }
}
