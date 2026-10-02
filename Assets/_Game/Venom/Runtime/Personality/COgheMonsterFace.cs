using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>
    /// The monster's face (<see cref="VenomLifeAnimation.Monster"/>): two big white slanted eyes (they blink), a mouth from
    /// cheek to cheek with rows of sharp teeth, and a long writhing tongue. Every point is put on the skin itself (found in the skin's field), so the
    /// face rides the head as it lunges, laughs and melts. Only while the act plays; presentation only (no colliders).
    /// The eyes, teeth and tongue appear only in this act (Mrk 02/10, an exception to the "no eyes or teeth" rule).
    /// </summary>
    public sealed class COgheMonsterFace : MonoBehaviour
    {
        private const int Lip = 18, Rows = 5, TeethPerRow = 13;
        private const float MouthWidth = 1.15f;   // radians either side of the face's middle
        private VenomLifeAnimation life;
        private VenomSurface surface;
        private Mesh eyes, mouth, throat, teeth, tongue;
        private Material white, red, dark, ivory, pink;
        private readonly List<GameObject> parts = new List<GameObject>();
        private readonly List<Vector3> v = new List<Vector3>(1024), n = new List<Vector3>(1024);
        private readonly List<int> tri = new List<int>(2048);
        private readonly Vector3[] upper = new Vector3[Lip + 1], lower = new Vector3[Lip + 1];
        private bool shown;

        // eye outline (radians across and up from its middle): round on the inside, a long point up and out
        private static readonly Vector2[] EyeShape =
        {
            new Vector2(-.2f, -.02f), new Vector2(-.15f, -.1f), new Vector2(-.03f, -.15f), new Vector2(.11f, -.13f), new Vector2(.23f, -.06f),
            new Vector2(.33f, .07f), new Vector2(.43f, .25f), new Vector2(.28f, .18f), new Vector2(.12f, .11f), new Vector2(-.03f, .07f), new Vector2(-.15f, .04f),
        };

        public void Initialize(VenomLifeAnimation owner, Material lit)
        {
            life = owner; surface = GetComponent<VenomSurface>();
            white = Fx(Color.white);
            red = Lit(lit, new Color(.58f, .06f, .1f), .6f); dark = Lit(lit, new Color(.16f, .01f, .03f), .35f);
            ivory = Lit(lit, new Color(.93f, .9f, .82f), .7f); pink = Lit(lit, new Color(.72f, .2f, .27f), .78f);
            eyes = Part("Monster eyes", white); mouth = Part("Monster mouth", red); throat = Part("Monster throat", dark);
            teeth = Part("Monster teeth", ivory); tongue = Part("Monster tongue", pink);
            if (surface != null) surface.Rebuilt += OnRebuilt;
            Show(false);
        }

        private static Material Lit(Material template, Color color, float smoothness)
        {
            var m = new Material(template) { name = "Monster " + color };
            m.SetColor("_BaseColor", color); m.SetFloat("_Metallic", 0); m.SetFloat("_Smoothness", smoothness);
            return m;
        }
        private static Material Fx(Color color)
        {
            var m = new Material(Resources.Load<Shader>("COgheInk/AccessoryFx")) { name = "Monster eyes" };
            m.SetColor("_Color", color); m.SetFloat("_Rim", .12f); m.SetFloat("_RimAlpha", 0);
            m.SetFloat("_SrcBlend", (float)BlendMode.One); m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.renderQueue = (int)RenderQueue.Transparent + 20;   // over the skin, even a clear inked one
            return m;
        }
        private Mesh Part(string name, Material material)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(transform, false);
            var mesh = new Mesh { name = name }; mesh.MarkDynamic();
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.GetComponent<MeshRenderer>(); r.sharedMaterial = material; r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            parts.Add(go); return mesh;
        }
        private void Show(bool on) { if (shown == on) return; shown = on; foreach (var p in parts) p.SetActive(on); }

        private void OnDestroy()
        {
            if (surface != null) surface.Rebuilt -= OnRebuilt;
            foreach (var m in new Object[] { eyes, mouth, throat, teeth, tongue, white, red, dark, ivory, pink }) if (m != null) Destroy(m);
        }

        // Head frame --------------------------------------------------------------------------------------------------------
        private COgheMonsterRig rig;
        private Vector3 jawUp, jawForward, jawHead;
        /// <summary>A direction from the head's middle: <paramref name="across"/> radians toward its right, <paramref name="rise"/> up.</summary>
        private Vector3 Dir(float across, float rise, bool jaw = false)
        {
            float c = Mathf.Cos(rise);
            Vector3 up = jaw ? jawUp : rig.Up, forward = jaw ? jawForward : rig.Forward;
            return (rig.Right * (Mathf.Sin(across) * c) + up * Mathf.Sin(rise) + forward * (Mathf.Cos(across) * c)).normalized;
        }
        private Vector3 OnSkin(float across, float rise, bool jaw = false) => life.SkinAlong(jaw ? jawHead : rig.Head, Dir(across, rise, jaw), rig.Size * 2.2f);

        private void OnRebuilt(VenomSurface s)
        {
            rig = life.Monster;
            if (rig.Amount < .02f) { Show(false); return; }
            Show(true);
            jawUp = rig.JawUp; jawForward = rig.JawForward; jawHead = rig.JawHead;   // the jaw's own frame
            float show = Mathf.SmoothStep(0, 1, rig.Amount);
            BuildMouth(show);
            BuildTeeth(show);
            BuildEyes(show);
            BuildTongue(show);
        }

        // The mouth: a grin from cheek to cheek; the jaw's lip follows the jaw ------------------------------------------------
        private static float UpperLip(float x) => -.16f + .36f * x * x;                               // corners up: a grin
        private static float LowerLip(float x) => UpperLip(x) - (.03f + .09f * (1 - x * x));         // a dark line when shut

        private void BuildMouth(float show)
        {
            float width = MouthWidth * Mathf.Lerp(.6f, 1, show);
            for (int i = 0; i <= Lip; i++)
            {
                float x = i / (float)Lip * 2 - 1;
                upper[i] = OnSkin(x * width, UpperLip(x));
                // the lower lip slides down the skin as the jaw opens (the skin of head, cheeks and chin is one piece of
                // liquid; the opening is drawn here): widest in the middle, closing at the corners
                lower[i] = OnSkin(x * width, LowerLip(x) - rig.Jaw * .7f * Mathf.Sqrt(Mathf.Max(0, 1 - x * x)));
            }
            // the skin's mesh is built on a ~6 mm grid: stand clear of it, or it pokes through
            Vector3 lift = rig.Forward * Mathf.Max(rig.Size * .1f, .0045f);
            // the mouth: rows between the lips, kept just outside the skin
            Begin();
            for (int r = 0; r <= Rows; r++)
            for (int i = 0; i <= Lip; i++)
            {
                Vector3 p = Vector3.Lerp(upper[i], lower[i], r / (float)Rows);
                if (r > 0 && r < Rows) p = OutsideSkin(p);
                v.Add(p + lift); n.Add(rig.Forward);
            }
            Grid(Rows, Lip);
            Commit(mouth);
            // the throat: the darker middle of the opening, a little in front of the mouth
            Begin();
            for (int r = 0; r <= Rows; r++)
            for (int i = 0; i <= Lip; i++)
            {
                float x = i / (float)Lip * 2 - 1, w = Mathf.Lerp(.3f, .78f, r / (float)Rows);
                float u = Mathf.Lerp(.5f, i / (float)Lip, .72f);   // the deep middle of the mouth
                int j = Mathf.Clamp(Mathf.RoundToInt(u * Lip), 0, Lip);
                Vector3 p = OutsideSkin(Vector3.Lerp(upper[j], lower[j], w));
                v.Add(p + lift * 1.4f); n.Add(rig.Forward);
            }
            Grid(Rows, Lip);
            Commit(throat);
        }

        /// <summary>A point pushed out of the skin along the face's forward direction.</summary>
        private Vector3 OutsideSkin(Vector3 p)
        {
            for (int k = 0; k < 12 && life.InsideSkin(p); k++) p += rig.Forward * (rig.Size * .06f);
            return p;
        }

        private void BuildTeeth(float show)
        {
            Begin();
            for (int row = 0; row < 2; row++)
            {
                bool top = row == 0;
                for (int k = 0; k < TeethPerRow; k++)
                {
                    float x = ((k + .5f) / TeethPerRow * 2 - 1) * .93f, at = (x + 1) * .5f * Lip;
                    int i = Mathf.Clamp(Mathf.FloorToInt(at), 0, Lip - 1); float f = at - i;
                    Vector3 lipUpper = Vector3.Lerp(upper[i], upper[i + 1], f), lipLower = Vector3.Lerp(lower[i], lower[i + 1], f);
                    Vector3 root = top ? lipUpper : lipLower;
                    // toward the other lip, a little out toward the viewer
                    Vector3 across = top ? -rig.Up : rig.Up, toward = (top ? lipLower - lipUpper : lipUpper - lipLower);
                    Vector3 dir = (across * .8f + rig.Forward * .25f + (toward.sqrMagnitude > 1e-8f ? toward.normalized * .4f : Vector3.zero)).normalized;
                    float fang = Mathf.Exp(-Mathf.Pow((Mathf.Abs(x) - .52f) / .12f, 2));   // long canines either side
                    float length = rig.Size * (top ? .3f + .3f * fang : .3f + .26f * fang) * (1 - .35f * x * x) * show;
                    float width = rig.Size * .095f * (1 + .35f * fang) * Mathf.Lerp(.5f, 1, show);
                    Vector3 side = Vector3.Cross(dir, rig.Forward).normalized;
                    Cone(root + rig.Forward * Mathf.Max(rig.Size * .16f, .006f) - dir * (width * .3f), dir, side, width, length);   // in front of the mouth
                }
            }
            Commit(teeth);
        }

        private void BuildEyes(float show)
        {
            Begin();
            float squint = Mathf.Clamp(rig.Squint, -.3f, .8f), size = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.15f, .75f, show)) * (1 + .15f * Mathf.Max(0, -squint));   // wide in the roar
            if (size < .01f) { Commit(eyes); return; }
            for (int side = -1; side <= 1; side += 2)
            {
                int first = v.Count;
                Vector2 middle = new Vector2(side * .46f, .3f);
                Vector3 centre = OnSkin(middle.x, middle.y);
                v.Add(centre + life.SkinNormal(centre) * Mathf.Max(rig.Size * .08f, .0042f)); n.Add(rig.Forward);
                foreach (var q0 in EyeShape)
                {
                    // a squint narrows it and drops the inner corner: a smug, wicked look
                    float open = 1 - .92f * Mathf.Clamp01(rig.Blink);   // a blink closes it to a slit
                    Vector2 q = new Vector2(q0.x * side, (q0.y * (1 - .55f * Mathf.Max(0, squint)) + q0.x * .28f * Mathf.Max(0, squint)) * open) * (size * 1.45f);
                    Vector3 p = OnSkin(middle.x + q.x, middle.y + q.y);
                    v.Add(p + life.SkinNormal(p) * Mathf.Max(rig.Size * .08f, .0042f)); n.Add(rig.Forward);
                }
                int count = EyeShape.Length;
                for (int k = 0; k < count; k++) Face(first, first + 1 + k, first + 1 + (k + 1) % count);
            }
            Commit(eyes);
        }

        private readonly Vector3[] tonguePath = new Vector3[21];
        /// <summary>A long tongue sliding out and writhing: a wave travels down it, mostly up and down so it reads in profile.</summary>
        private void BuildTongue(float show)
        {
            Begin();
            float out_ = rig.Tongue;
            if (out_ < .02f) { Commit(tongue); return; }
            int mid = Lip / 2;
            Vector3 root = OutsideSkin(Vector3.Lerp(upper[mid], lower[mid], .7f)) - rig.Forward * (rig.Size * .06f);
            float length = rig.Size * 2.6f * out_, t = rig.Time;
            for (int k = 0; k < tonguePath.Length; k++)
            {
                float u = k / (float)(tonguePath.Length - 1);
                float lift = .27f * Mathf.Sin(t * 5.2f - u * 7.5f) * u + .25f * u * u * u * u;   // the tip curls up
                float side = .09f * Mathf.Sin(t * 3.7f - u * 5f) * u;
                tonguePath[k] = root + rig.Forward * (length * u) + rig.Up * (length * (lift - .2f * u * u)) + rig.Right * (length * side);
            }
            TubePath(tonguePath, rig.Size * .17f, rig.Size * .055f);
            Commit(tongue);
        }

        // Mesh helpers ------------------------------------------------------------------------------------------------------
        private void Begin() { v.Clear(); n.Clear(); tri.Clear(); }
        private void Commit(Mesh mesh)
        {
            mesh.Clear();
            if (v.Count == 0) return;
            var local = transform.worldToLocalMatrix;
            for (int i = 0; i < v.Count; i++) { v[i] = local.MultiplyPoint3x4(v[i]); n[i] = local.MultiplyVector(n[i]).normalized; }
            mesh.SetVertices(v); mesh.SetNormals(n); mesh.SetTriangles(tri, 0, false);
            if (mesh != eyes) mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
        /// <summary>A triangle facing <see cref="COgheMonsterRig.Forward"/> (out of the face), whatever order it is given in.</summary>
        private void Face(int a, int b, int c)
        {
            Vector3 normal = Vector3.Cross(v[b] - v[a], v[c] - v[a]);
            if (Vector3.Dot(normal, rig.Forward) >= 0) { tri.Add(a); tri.Add(b); tri.Add(c); } else { tri.Add(a); tri.Add(c); tri.Add(b); }
        }
        private void Grid(int rows, int columns)
        {
            int stride = columns + 1, start = v.Count - (rows + 1) * stride;
            for (int r = 0; r < rows; r++)
            for (int i = 0; i < columns; i++)
            {
                int a = start + r * stride + i, b = a + 1, c = a + stride, d = c + 1;
                Face(a, b, d); Face(a, d, c);
            }
        }
        private void Cone(Vector3 root, Vector3 dir, Vector3 side, float width, float length)
        {
            if (length < 1e-5f) return;
            const int sides = 5;
            Vector3 other = Vector3.Cross(dir, side).normalized;
            int first = v.Count;
            for (int k = 0; k < sides; k++)
            {
                float angle = k * Mathf.PI * 2 / sides;
                Vector3 radial = side * Mathf.Cos(angle) + other * Mathf.Sin(angle) * .7f;
                v.Add(root + radial * (width * .5f)); n.Add(radial);
            }
            v.Add(root + dir * length); n.Add(dir);
            int tip = v.Count - 1;
            for (int k = 0; k < sides; k++)
            {
                int a = first + k, b = first + (k + 1) % sides;
                // outward from the cone's axis
                Vector3 normal = Vector3.Cross(v[b] - v[a], v[tip] - v[a]);
                Vector3 outward = (v[a] + v[b]) * .5f - root;
                if (Vector3.Dot(normal, outward) >= 0) { tri.Add(a); tri.Add(b); tri.Add(tip); } else { tri.Add(a); tri.Add(tip); tri.Add(b); }
            }
        }
        private void TubePath(Vector3[] path, float r0, float r1)
        {
            const int sides = 8;
            int first = v.Count, segments = path.Length - 1;
            for (int ring = 0; ring <= segments; ring++)
            {
                float t = ring / (float)segments;
                Vector3 p = path[ring], tangent = (path[Mathf.Min(segments, ring + 1)] - path[Mathf.Max(0, ring - 1)]).normalized;
                Vector3 right = Vector3.Cross(tangent, rig.Up); if (right.sqrMagnitude < 1e-6f) right = rig.Right; right.Normalize();
                Vector3 normal = Vector3.Cross(right, tangent).normalized;
                float cap = t < .85f ? 1 : Mathf.Sqrt(Mathf.Max(0, 1 - Mathf.Pow((t - .85f) / .15f, 2)));
                float radius = Mathf.Lerp(r0, r1, t) * Mathf.Max(cap, .1f);
                for (int k = 0; k < sides; k++)
                {
                    float angle = k * Mathf.PI * 2 / sides;
                    Vector3 dir = right * Mathf.Cos(angle) * 1.35f + normal * Mathf.Sin(angle) * .55f;   // a flat tongue
                    v.Add(p + dir * radius); n.Add(dir.normalized);
                }
            }
            for (int ring = 0; ring < segments; ring++)
            for (int k = 0; k < sides; k++)
            {
                int a = first + ring * sides + k, b = first + ring * sides + (k + 1) % sides, c = a + sides, d = b + sides;
                Vector3 outward = v[a] - Vector3.Lerp(v[first + ring * sides], v[first + ring * sides + sides / 2], .5f);
                Vector3 normal = Vector3.Cross(v[b] - v[a], v[c] - v[a]);
                if (Vector3.Dot(normal, outward) >= 0) { tri.Add(a); tri.Add(b); tri.Add(c); tri.Add(b); tri.Add(d); tri.Add(c); }
                else { tri.Add(a); tri.Add(c); tri.Add(b); tri.Add(b); tri.Add(c); tri.Add(d); }
            }
        }
    }
}
