using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>
    /// COghe's eyes (Mrk 07/10/2026: "Nếu COghe có mắt để diễn tả cảm xúc thì có được không?", reviewed side by side,
    /// then "tao chốt phương án có mắt"). On by default; -coghe-no-eyes on the command line (or <see cref="Enabled"/>)
    /// switches them off.
    /// Each piece of COghe (a whole body, or each part after a split, smaller for a smaller part) gets two cartoon eyes on the
    /// side of its skin facing the player, found on the real skin every frame so they ride every squash, hop and squeeze.
    /// White with a dark rim, so they read on the black body and on the light inks alike. They blink, look at what COghe
    /// is busy with (a handle, the food, the ball) and show its mood (<see cref="COghePersonality.EyeMood"/>): happy arcs,
    /// hearts when it hugs the player, drooping when sad or sulking, wide when surprised, closed asleep. They fade out
    /// while COghe makes a shape (the shape is the message), in a tube, stretched thin, or leaving through the exit.
    /// Presentation only: meshes without colliders, drawn over the skin; nothing is written to the simulation.
    /// </summary>
    public sealed class COgheEyes : MonoBehaviour
    {
        public static bool Enabled = Array.IndexOf(Environment.GetCommandLineArgs(), "-coghe-no-eyes") < 0;
        /// <summary>Recorders and reviews: keep <see cref="LastState"/> each frame (off in play: no text built per frame).</summary>
        public static bool Trace;
        private const int MaxPieces = 4, Ring = 22;
        private VenomCampaign game;
        private VenomSurface surface;
        private Mesh mesh; private GameObject view;
        private Material dark, white, pink;
        private readonly List<Vector3> v = new List<Vector3>(1024);
        private readonly List<int> gone = new List<int>(4);
        private readonly List<int>[] tri = { new List<int>(512), new List<int>(512), new List<int>(512), new List<int>(256), new List<int>(256) };
        private sealed class Pair
        {
            public float Shown, NextBlink, Blink, Size = -1, Odd, HiddenFor; public Vector2 Look; public COgheMood Current; public bool Seen;
            public readonly Steady[] Eye = new Steady[2]; public readonly Vector3[] Normal = new Vector3[2];
            public void Forget() { Eye[0] = Eye[1] = default; Normal[0] = Normal[1] = Vector3.zero; }
        }
        /// <summary>A one-euro filter: still, it smooths hard (the skin's ripple does not shake the eyes); moving fast, it
        /// follows closely (a squash or a hop does not leave them behind).</summary>
        private struct Steady
        {
            public Vector3 X, Speed; public bool Set;
            public Vector3 Step(Vector3 x, float dt)
            {
                if (!Set) { X = x; Speed = Vector3.zero; Set = true; return X; }
                if (dt <= 0) return X;
                Speed = Vector3.Lerp(Speed, (x - X) / dt, Alpha(dt, 1f));
                X = Vector3.Lerp(X, x, Alpha(dt, 1.2f + 30 * Speed.magnitude));
                return X;
            }
            private static float Alpha(float dt, float cutoff) => 1 / (1 + 1 / (2 * Mathf.PI * cutoff * dt));
        }
        private readonly Dictionary<int, Pair> pairs = new Dictionary<int, Pair>();
        private readonly System.Random rnd = new System.Random(77);
        private readonly int[] groups = new int[32], counts = new int[32];
        private readonly Vector3[] bodies = new Vector3[32];
        private float clock = -1;
        /// <summary>Last frame, per piece: group, mood, shown, half-sizes, particles, and the left eye's offset from the body
        /// as found and as drawn (mm). Only kept while <see cref="Trace"/>.</summary>
        public string LastState { get; private set; } = "";
        private readonly System.Text.StringBuilder state = new System.Text.StringBuilder();   // the simulation's clock: the eyes keep COghe's time (paused with it, steady in a recording)

        public static COgheEyes Attach(VenomCampaign game)
        {
            var s = game.Matter.GetComponent<VenomSurface>(); if (s == null) return null;
            var e = s.GetComponent<COgheEyes>(); if (e == null) e = s.gameObject.AddComponent<COgheEyes>();
            e.game = game; e.surface = s; s.Rebuilt -= e.OnRebuilt; s.Rebuilt += e.OnRebuilt;
            return e;
        }

        private static Material Fx(Color color, string name)
        {
            var m = new Material(Resources.Load<Shader>("COgheInk/AccessoryFx")) { name = name };
            m.SetColor("_Color", color); m.SetFloat("_Rim", 0); m.SetFloat("_RimAlpha", 0);
            m.SetFloat("_SrcBlend", (float)BlendMode.One); m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.renderQueue = (int)RenderQueue.Transparent + 22;   // over the skin and the worn things
            return m;
        }
        private void Build()
        {
            view = new GameObject("COghe eyes", typeof(MeshFilter), typeof(MeshRenderer)); view.transform.SetParent(transform, false);
            mesh = new Mesh { name = "COghe eyes" }; mesh.MarkDynamic(); mesh.subMeshCount = tri.Length; view.GetComponent<MeshFilter>().sharedMesh = mesh;
            dark = Fx(new Color(.07f, .08f, .1f), "Eye ink"); white = Fx(new Color(.98f, .98f, .96f), "Eye white"); pink = Fx(new Color(.93f, .38f, .45f), "Eye heart");
            var r = view.GetComponent<MeshRenderer>(); r.sharedMaterials = new[] { dark, white, dark, white, pink };
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
        }
        private void OnDestroy()
        {
            if (surface != null) surface.Rebuilt -= OnRebuilt;
            foreach (var o in new UnityEngine.Object[] { mesh, dark, white, pink }) if (o != null) Destroy(o);
            if (view != null) Destroy(view);
        }

        private void OnRebuilt(VenomSurface s)
        {
            if (!Enabled || game == null || game.Owner == null || game.Owner.View == null) { if (view != null) view.SetActive(false); return; }
            if (view == null) Build();
            view.SetActive(true);
            float now = game.Matter.SimulationTime, dt = clock < 0 ? 0 : Mathf.Clamp(now - clock, 0, .05f);
            if (now < clock - .05f) pairs.Clear();   // Retry rewinds the clock: start the eyes again
            clock = now;
            v.Clear(); foreach (var t in tri) t.Clear();
            // the pieces of COghe still in the box (a small drip gets no eyes), and the middle of each piece's body; after
            // a win, the whole of COghe out at the outlet celebrating
            var m = game.Matter; int pieces = 0;
            bool celebrating = game.Owner.Celebration != null && game.Owner.Celebration.Active;
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
            {
                int g = m.Groups[i]; int k = Array.IndexOf(groups, g, 0, pieces);
                if (k < 0) { if (pieces >= groups.Length) continue; k = pieces++; groups[k] = g; counts[k] = 0; bodies[k] = Vector3.zero; }
                bool left = m.Escaped[i] && !celebrating;
                counts[k] += left ? -100 : 1; bodies[k] += m.Bodies[i].transform.position;
            }
            foreach (var p in pairs.Values) p.Seen = false;
            state.Clear();
            var cam = game.Owner.View.transform;
            for (int k = 0, drawn = 0; k < pieces && drawn < MaxPieces; k++)
            {
                int g = groups[k], count = counts[k]; Vector3 body = bodies[k] / Mathf.Max(1, count);
                if (!pairs.TryGetValue(g, out var pair)) { pair = new Pair { NextBlink = now + 1 + (float)rnd.NextDouble() * 3, Shown = 0 }; pairs[g] = pair; }
                pair.Seen = true;
                if (!surface.FragmentShape(g, out var centre, out var half)) continue;
                Vector3? look = null; var mood = COgheMood.Normal;
                if (game.Personality != null) mood = game.Personality.EyeMood(g, out look);
                // stretched thin (a sausage squeezing through a gap or reaching far), in a tube, too small or leaving: no
                // eyes. Flat is fine (lying in bed, spread on the floor): only the longest side against the middle one counts,
                // and only once it has lasted a moment, so a quick stretch does not make them flicker.
                float longest = Mathf.Max(half.x, half.y, half.z), middle = Mathf.Max(.002f, half.x + half.y + half.z - longest - Mathf.Min(half.x, Mathf.Min(half.y, half.z)));
                bool odd = !celebrating && (longest / middle > 2.6f || longest > .1f);   // the victory dance waves its arms
                pair.Odd = odd ? pair.Odd + dt : 0;
                // a shape hides them, but not a frame's flicker of one
                pair.HiddenFor = mood == COgheMood.Hidden ? pair.HiddenFor + dt : 0;
                bool show = pair.HiddenFor < .08f && count >= 4 && !game.InTube && pair.Odd < .2f;
                pair.Shown = Mathf.MoveTowards(pair.Shown, show ? 1 : 0, dt * (show ? 9 : 10));
                if (Trace) state.Append($"{g}:{mood}:{pair.Shown:F2}:{half.x:F3},{half.y:F3},{half.z:F3}:{count} ");
                if (pair.Shown <= .01f) { pair.Forget(); continue; }
                if (mood != COgheMood.Hidden) pair.Current = mood;
                // blinks: quick, every few seconds (not while the eyes are already shut or arched)
                if (now > pair.NextBlink) { pair.Blink = .14f; pair.NextBlink = now + 2.2f + (float)rnd.NextDouble() * 3.5f; }
                pair.Blink = Mathf.Max(0, pair.Blink - dt);
                float blink = pair.Blink > 0 ? Mathf.Sin(pair.Blink / .14f * Mathf.PI) : 0;
                // the size follows the piece's width as the player sees it (a smaller part, smaller eyes), smoothed so a
                // squash or a reach does not make them pulse
                Vector3 across = surface.transform.InverseTransformDirection(cam.right);
                float width = Mathf.Abs(across.x) * half.x + Mathf.Abs(across.y) * half.y + Mathf.Abs(across.z) * half.z;
                float size = Mathf.Clamp(width / .04f, .5f, 1.3f);
                pair.Size = pair.Size < 0 ? size : Mathf.Lerp(pair.Size, size, 1 - Mathf.Exp(-dt * 3));
                float scale = pair.Size * Mathf.SmoothStep(0, 1, pair.Shown) * (celebrating ? 1.15f : 1);
                DrawPair(g, centre, body, cam, look, pair, blink, scale, dt, now);
                drawn++;
            }
            if (Trace) LastState = state.ToString();
            gone.Clear(); foreach (var kv in pairs) if (!kv.Value.Seen) gone.Add(kv.Key); foreach (var g in gone) pairs.Remove(g);
            mesh.Clear(); mesh.subMeshCount = tri.Length;
            var local = transform.worldToLocalMatrix; for (int i = 0; i < v.Count; i++) v[i] = local.MultiplyPoint3x4(v[i]);
            // two-sided: whichever way a piece's skin faces, the eyes show
            foreach (var t in tri) { int n = t.Count; for (int i = 0; i < n; i += 3) { t.Add(t[i]); t.Add(t[i + 2]); t.Add(t[i + 1]); } }
            mesh.SetVertices(v); for (int t = 0; t < tri.Length; t++) mesh.SetTriangles(tri[t], t, false); mesh.RecalculateBounds();
        }

        private void DrawPair(int group, Vector3 centre, Vector3 body, Transform cam, Vector3? look, Pair pair, float blink, float scale, float dt, float now)
        {
            Vector3 up = game.Root != null ? game.Root.up : Vector3.up;
            Vector3 toCam = (cam.position - centre).normalized;
            Vector3 face = (toCam + up * .3f).normalized;
            if (!surface.SkinToward(group, centre, face, out var p0, out var n0)) return;
            Vector3 right = Vector3.ProjectOnPlane(cam.right, n0).normalized;
            float w = .0071f * scale, h = .0092f * scale, spacing = .0108f * scale;
            // where the eyes look: at the player, or at what COghe is busy with (a little, inside the eye)
            Vector2 want = Vector2.zero;
            if (look.HasValue)
            {
                // full aside for something away from it, less for something close (a hand on a handle): no darting about
                Vector3 d = look.Value - p0; Vector2 onScreen = new Vector2(Vector3.Dot(d, cam.right), Vector3.Dot(d, cam.up));
                want = onScreen / Mathf.Max(onScreen.magnitude, .04f) * .8f;
            }
            else want = new Vector2(Mathf.Sin(now * .7f + group) * .25f, Mathf.Sin(now * .45f + group * 2) * .15f);   // a living look about
            pair.Look = Vector2.Lerp(pair.Look, want, 1 - Mathf.Exp(-dt * 6));
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 aim = (p0 + right * (side * spacing) - centre).normalized;
                if (!surface.SkinToward(group, centre, aim, out var p, out var n)) continue;
                // steadied where they sit on the body (so moving with COghe costs no lag), and lifted toward the camera
                // (unseen in its straight-on view) so a ripple of the skin never covers them
                int e = side < 0 ? 0 : 1; Vector3 raw = p - body;
                p = body + pair.Eye[e].Step(raw, dt) + toCam * .003f;
                if (Trace && e == 0) { var f = pair.Eye[0].X; state.Append($"e{raw.x * 1000:F2},{raw.y * 1000:F2},{raw.z * 1000:F2}|{f.x * 1000:F2},{f.y * 1000:F2},{f.z * 1000:F2} "); }
                pair.Normal[e] = pair.Normal[e] == Vector3.zero ? n : Vector3.Slerp(pair.Normal[e], n, 1 - Mathf.Exp(-dt * 10));
                n = pair.Normal[e];
                Vector3 normal = (n * .45f + toCam * .55f).normalized;
                Vector3 r = Vector3.ProjectOnPlane(cam.right, normal).normalized, u = Vector3.Cross(normal, r).normalized;
                if (Vector3.Dot(u, cam.up) < 0) u = -u;
                Vector3 at = p + normal * .0016f;
                Eye(at, normal, r, u, side, w, h, pair, blink);
            }
        }

        // one eye in its own plane: at, right r, up u; side −1 left, +1 right (outer corner is +side)
        private void Eye(Vector3 at, Vector3 normal, Vector3 r, Vector3 u, int side, float w, float h, Pair pair, float blink)
        {
            Vector3 P(float x, float y, float lift) => at + r * x + u * y + normal * lift;
            var mood = pair.Current;
            switch (mood)
            {
                case COgheMood.Happy:   // ∩ arcs: smiling eyes
                    Arc(P, w * 1.05f, h * .55f, -h * .1f, true, w * .62f);
                    return;
                case COgheMood.Sleepy:  // ‿ closed
                    Arc(P, w * 1.0f, h * .35f, h * .05f, false, w * .55f);
                    return;
                case COgheMood.Love:    // heart eyes
                    Heart(P, w * 1.25f, 0);
                    return;
            }
            float open = 1, tilt = 0, big = 1, pupil = .52f;
            switch (mood)
            {
                case COgheMood.Sad: open = .78f; tilt = -.35f; break;          // the outer corner droops
                case COgheMood.Sulky: open = .5f; tilt = .12f; break;          // half shut, flat
                case COgheMood.Surprised: big = 1.22f; pupil = .36f; break;    // wide open
                case COgheMood.Focus: open = .86f; break;
            }
            open *= 1 - blink;
            if (open < .12f) { Arc(P, w, h * .25f, 0, false, w * .34f); return; }   // mid-blink: a line
            float ew = w * big, eh = h * big;
            // the lid: the top of the eye cut by a line (lower at the outer corner when tilt < 0)
            float Lid(float x) => eh * (2 * open - 1) + tilt * eh * (x * side / ew);
            // rim (dark, a little bigger), then the white, then the pupil and its shine
            Ellipse(P, ew * 1.2f, eh * 1.14f, x => Lid(x) + eh * .14f, 0, 0);
            Ellipse(P, ew, eh, Lid, 1, .0002f);
            Vector2 c = new Vector2(pair.Look.x * ew * .42f, pair.Look.y * eh * .4f - eh * .06f);
            float pr = ew * pupil;
            Circle(P, c, pr, 2, .0004f, y => y <= Lid(c.x) + .0001f);
            Circle(P, c + new Vector2(-pr * .35f, pr * .35f), pr * .3f, 3, .0006f, y => y <= Lid(c.x));
        }

        private void Ellipse(Func<float, float, float, Vector3> P, float ew, float eh, Func<float, float> lid, int sub, float lift)
        {
            int first = v.Count; v.Add(P(0, Mathf.Min(0, lid(0)) * .5f, lift));
            for (int i = 0; i < Ring; i++)
            {
                float a = i * Mathf.PI * 2 / Ring, x = Mathf.Cos(a) * ew, y = Mathf.Sin(a) * eh;
                v.Add(P(x, Mathf.Min(y, lid(x)), lift));
            }
            for (int i = 0; i < Ring; i++) { tri[sub].Add(first); tri[sub].Add(first + 1 + (i + 1) % Ring); tri[sub].Add(first + 1 + i); }
        }
        private void Circle(Func<float, float, float, Vector3> P, Vector2 c, float radius, int sub, float lift, Func<float, bool> keep)
        {
            int first = v.Count; v.Add(P(c.x, c.y, lift));
            for (int i = 0; i < 14; i++)
            {
                float a = i * Mathf.PI * 2 / 14; float y = c.y + Mathf.Sin(a) * radius;
                v.Add(P(c.x + Mathf.Cos(a) * radius, keep(y) ? y : c.y + (y - c.y) * .2f, lift));
            }
            for (int i = 0; i < 14; i++) { tri[sub].Add(first); tri[sub].Add(first + 1 + (i + 1) % 14); tri[sub].Add(first + 1 + i); }
        }
        // a thick curved stroke (white with a dark rim): ∩ when up, ‿ when not
        private void Arc(Func<float, float, float, Vector3> P, float ew, float eh, float y0, bool upward, float thick)
        {
            for (int layer = 0; layer < 2; layer++)
            {
                int sub = layer == 0 ? 0 : 1; float t = layer == 0 ? thick : thick * .55f, lift = layer * .0002f; int first = v.Count;
                for (int i = 0; i <= 12; i++)
                {
                    float a = Mathf.PI * i / 12, x = -Mathf.Cos(a) * ew, y = y0 + (upward ? 1 : -1) * Mathf.Sin(a) * eh;
                    Vector2 n = new Vector2(-Mathf.Cos(a) * eh, (upward ? 1 : -1) * Mathf.Sin(a) * ew).normalized * (t * .5f);
                    v.Add(P(x + n.x, y + n.y, lift)); v.Add(P(x - n.x, y - n.y, lift));
                }
                for (int i = 0; i < 12; i++)
                {
                    int a = first + i * 2;
                    tri[sub].Add(a); tri[sub].Add(a + 2); tri[sub].Add(a + 1);
                    tri[sub].Add(a + 1); tri[sub].Add(a + 2); tri[sub].Add(a + 3);
                }
            }
        }
        private void Heart(Func<float, float, float, Vector3> P, float size, float y0)
        {
            for (int layer = 0; layer < 2; layer++)
            {
                int sub = layer == 0 ? 0 : 4; float s = size * (layer == 0 ? 1.2f : 1f), lift = layer * .0003f; int first = v.Count;
                v.Add(P(0, y0, lift));
                for (int i = 0; i < 24; i++)
                {
                    float t = i * Mathf.PI * 2 / 24;
                    float x = 16 * Mathf.Pow(Mathf.Sin(t), 3), y = 13 * Mathf.Cos(t) - 5 * Mathf.Cos(2 * t) - 2 * Mathf.Cos(3 * t) - Mathf.Cos(4 * t);
                    v.Add(P(x / 17 * s, y0 + (y / 17 + .1f) * s, lift));
                }
                for (int i = 0; i < 24; i++) { tri[sub].Add(first); tri[sub].Add(first + 1 + (i + 1) % 24); tri[sub].Add(first + 1 + i); }
            }
        }
    }
}
