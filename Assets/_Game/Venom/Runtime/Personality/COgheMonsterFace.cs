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
        private const int Lip = 28, Rows = 5, TeethPerRow = 11;
        private const float MouthWidth = 1.15f;   // radians either side of the face's middle
        private VenomLifeAnimation life;
        private VenomSurface surface;
        private Mesh eyes, mouth, throat, teeth, tongue;
        private Material white, red, dark, ivory, pink;
        private readonly List<GameObject> parts = new List<GameObject>();
        private readonly List<Vector3> v = new List<Vector3>(1024), n = new List<Vector3>(1024);
        private readonly List<int> tri = new List<int>(2048);
        private readonly Vector3[] upper = new Vector3[Lip + 1], lower = new Vector3[Lip + 1];
        private readonly Vector3[] lipPoints=new Vector3[(Lip+1)*(Rows+1)], lipNormals=new Vector3[(Lip+1)*(Rows+1)];
        public const float TongueWidthScale = 2.7f; // exactly twice the previous 1.35; thickness is unchanged
        private bool shown;
        public double LastBuildMilliseconds { get; private set; }

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
            red = Lit(lit, new Color(.24f, .025f, .045f), .38f); dark = Lit(lit, new Color(.026f, .008f, .016f), .22f);
            ivory = Lit(lit, new Color(.93f, .9f, .82f), .7f); pink = Lit(lit, new Color(.64f, .16f, .22f), .62f);
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
            long started = COgheMobileMetrics.Enabled ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;
            LastBuildMilliseconds = 0;
            rig = life.Monster;
            if (rig.Amount < .02f) { Show(false); return; }
            Show(true);
            jawUp = rig.JawUp; jawForward = rig.JawForward; jawHead = rig.JawHead;   // the jaw's own frame
            float show = Mathf.SmoothStep(0, 1, rig.Amount);
            BuildMouth(rig.Mouth);
            BuildTeeth(rig.Fangs);
            BuildEyes(show);
            BuildTongue(show);
            if (started != 0) LastBuildMilliseconds = (System.Diagnostics.Stopwatch.GetTimestamp() - started) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
        }

        // The mouth: a grin from cheek to cheek; the jaw's lip follows the jaw ------------------------------------------------
        private static float UpperLip(float x) => -.16f + .36f * x * x;                               // corners up: a grin
        private static float LowerLip(float x) => UpperLip(x) - (.03f + .09f * (1 - x * x));         // a dark line when shut

        private float LipAngle(float x, float across) => Mathf.Lerp(UpperLip(x),
            LowerLip(x) - rig.Jaw * .7f * Mathf.Sqrt(Mathf.Max(0, 1 - x * x)), across);

        private Vector3 MouthPoint(float x, float across, float width, float offset, out Vector3 normal)
        {
            Vector3 p = OnSkin(x * width, LipAngle(x, across));
            normal = life.SkinNormal(p);
            return p + normal * offset;
        }

        private void BuildMouth(float show)
        {
            Begin();
            if (show < .001f) { Commit(mouth); Commit(throat); return; }
            float width = MouthWidth * Mathf.SmoothStep(0, 1, show);
            float lift = .0016f;
            // Smoothly sampled surface patches; no rounded-to-nearest columns or stepped push-out.
            for (int row = 0; row <= Rows; row++)
            for (int i = 0; i <= Lip; i++)
            {
                Vector3 p = MouthPoint(i / (float)Lip * 2 - 1, row / (float)Rows, width, lift, out var normal);
                int at=row*(Lip+1)+i;lipPoints[at]=p;lipNormals[at]=normal;
                if(row==0)upper[i]=p;if(row==Rows)lower[i]=p;
                v.Add(p); n.Add(normal);
            }
            Grid(Rows, Lip); Commit(mouth);
            Begin();
            for (int row = 0; row <= Rows; row++)
            for (int i = 0; i <= Lip; i++)
            {
                float x = (i / (float)Lip * 2 - 1) * .95f;
                float u=(x+1)*.5f*Lip, w=Mathf.Lerp(.12f,.88f,row/(float)Rows)*Rows;
                int column=Mathf.Min((int)u,Lip-1),r=Mathf.Min((int)w,Rows-1);u-=column;w-=r;
                int at=r*(Lip+1)+column;
                Vector3 normal=Vector3.Lerp(Vector3.Lerp(lipNormals[at],lipNormals[at+1],u),Vector3.Lerp(lipNormals[at+Lip+1],lipNormals[at+Lip+2],u),w).normalized;
                Vector3 p=Vector3.Lerp(Vector3.Lerp(lipPoints[at],lipPoints[at+1],u),Vector3.Lerp(lipPoints[at+Lip+1],lipPoints[at+Lip+2],u),w)+normal*.00045f;
                v.Add(p); n.Add(normal);
            }
            Grid(Rows, Lip); Commit(throat);
        }

        private void BuildTeeth(float show)
        {
            Begin();
            if (show < .001f || rig.Mouth < .1f) { Commit(teeth); return; }
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
                    Vector3 normal = life.SkinNormal(root);
                    Vector3 tangent = Vector3.ProjectOnPlane(toward.sqrMagnitude > 1e-8f ? toward.normalized : across, normal).normalized;
                    Vector3 dir = (tangent + normal * .5f).normalized;
                    float fang = Mathf.Exp(-Mathf.Pow((Mathf.Abs(x) - .52f) / .12f, 2));   // long canines either side
                    float length = rig.Size * (top ? .3f + .3f * fang : .3f + .26f * fang) * (1 - .35f * x * x) * show;
                    length = Mathf.Min(length, (lipUpper - lipLower).magnitude * .48f);
                    float width = rig.Size * .085f * (1 + .35f * fang) * Mathf.SmoothStep(0, 1, show);
                    Vector3 side = Vector3.Cross(dir, rig.Forward).normalized;
                    Cone(root + rig.Forward * .0013f - dir * (width * .3f), dir, side, width, length);   // in front of the mouth
                }
            }
            Commit(teeth);
        }

        private void BuildEyes(float show)
        {
            Begin();
            float squint = Mathf.Clamp(rig.Squint, -.3f, .8f), size = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.15f, .75f, show)) * (1 + .15f * Mathf.Max(0, -squint));   // wide in the roar
            if (size < .01f) { Commit(eyes); return; }
            const int steps = 44, rings = 3;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 middle = new Vector2(side * .46f, .3f);
                Vector3 centre=OnSkin(middle.x,middle.y), centreNormal=life.SkinNormal(centre);
                for (int ring = 0; ring <= rings; ring++)
                for (int k = 0; k <= steps; k++)
                {
                    if(ring==0){v.Add(centre+centreNormal*.0017f);n.Add(centreNormal);continue;}
                    if(k==steps){v.Add(v[v.Count-steps]);n.Add(n[n.Count-steps]);continue;}
                    float at = k / (float)steps * EyeShape.Length;
                    int j = Mathf.FloorToInt(at) % EyeShape.Length; float u = at - Mathf.Floor(at);
                    Vector2 a = EyeShape[(j + EyeShape.Length - 1) % EyeShape.Length], b = EyeShape[j],
                        c = EyeShape[(j + 1) % EyeShape.Length], d = EyeShape[(j + 2) % EyeShape.Length];
                    Vector2 q0 = .5f * ((2 * b) + (-a + c) * u + (2 * a - 5 * b + 4 * c - d) * u * u + (-a + 3 * b - 3 * c + d) * u * u * u);
                    float open = 1 - .94f * Mathf.Clamp01(rig.Blink);
                    Vector2 q = new Vector2(q0.x * side, q0.y * (1 - .55f * Mathf.Max(0, squint)) * open) * (size * 1.35f * ring / rings);
                    Vector3 p = OnSkin(middle.x + q.x, middle.y + q.y), normal = life.SkinNormal(p);
                    v.Add(p + normal * .0017f); n.Add(normal);
                }
                Grid(rings, steps);
            }
            Commit(eyes);
        }

        private readonly Vector3[] tonguePath = new Vector3[29];
        /// <summary>A long tongue sliding out and writhing: a wave travels down it, mostly up and down so it reads in profile.</summary>
        private void BuildTongue(float show)
        {
            Begin();
            float out_ = rig.Tongue;
            if (out_ < .02f) { Commit(tongue); return; }
            Vector3 root = MouthPoint(0, .7f, MouthWidth, .0025f, out _);
            float length = rig.Size * 1.85f * out_, t = rig.Time;
            for (int k = 0; k < tonguePath.Length; k++)
            {
                float u = k / (float)(tonguePath.Length - 1);
                // Keep the old travelling-wave rhythm across the whole tongue. Only
                // the first short section eases in, so the root stays anchored while
                // the middle bends and the tip follows instead of holding a fixed curl.
                float envelope=u*Mathf.SmoothStep(0,1,u/.18f), curl=.25f*u*u*u*u;
                float lift=.27f*Mathf.Sin(t*3.2f-u*5.5f)*envelope+curl;
                float side=.09f*Mathf.Sin(t*2.4f-u*4f)*envelope;
                tonguePath[k]=root+rig.Forward*(length*u)+Vector3.down*(length*.20f*u*u)
                    +rig.Up*(length*lift)+rig.Right*(length*side);
            }
            TubePath(tonguePath, rig.Size * .17f * Mathf.SmoothStep(0, .3f, out_), rig.Size * .04f * out_);
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
            const int sides = 10, rings = 4;
            Vector3 other = Vector3.Cross(dir, side).normalized;
            int first = v.Count;
            for (int ring = 0; ring <= rings; ring++)
            {
                float u = ring / (float)rings, radius = width * .5f * Mathf.Pow(1 - u, .8f);
                for (int k = 0; k < sides; k++)
                {
                    float angle = k * Mathf.PI * 2 / sides;
                    Vector3 radial = side * Mathf.Cos(angle) + other * Mathf.Sin(angle);
                    v.Add(root + dir * (length * u) + rig.Forward * (length * .16f * u * u) + radial * radius);
                    n.Add((radial + dir * (width / Mathf.Max(length, .0001f) * .5f)).normalized);
                }
            }
            for (int ring = 0; ring < rings; ring++)
            for (int k = 0; k < sides; k++)
            {
                int a = first + ring * sides + k, b = first + ring * sides + (k + 1) % sides, c = a + sides, d = b + sides;
                tri.Add(a); tri.Add(b); tri.Add(c); tri.Add(b); tri.Add(d); tri.Add(c);
            }
        }
        private void TubePath(Vector3[] path, float r0, float r1)
        {
            const int sides = 16;
            int first = v.Count, segments = path.Length - 1;
            for (int ring = 0; ring <= segments; ring++)
            {
                float t = ring / (float)segments;
                Vector3 p = path[ring], tangent = (path[Mathf.Min(segments, ring + 1)] - path[Mathf.Max(0, ring - 1)]).normalized;
                Vector3 right=Vector3.ProjectOnPlane(rig.Right,tangent).normalized;
                if(right.sqrMagnitude<1e-6f)right=Vector3.Cross(tangent,rig.Up).normalized;
                right=Quaternion.AngleAxis(8*Mathf.Sin(rig.Time*2.4f-t*4f)*t*rig.Tongue,tangent)*right;
                Vector3 normal = Vector3.Cross(right, tangent).normalized;
                float cap = t < .85f ? 1 : Mathf.Sqrt(Mathf.Max(0, 1 - Mathf.Pow((t - .85f) / .15f, 2)));
                float radius = Mathf.Lerp(r0, r1, t) * Mathf.Max(cap, .1f);
                for (int k = 0; k < sides; k++)
                {
                    float angle = k * Mathf.PI * 2 / sides;
                    Vector3 dir = right * Mathf.Cos(angle) * TongueWidthScale + normal * Mathf.Sin(angle) * .55f;   // a flat tongue
                    v.Add(p + dir * radius); n.Add((right * (Mathf.Cos(angle) / TongueWidthScale) + normal * (Mathf.Sin(angle) / .55f)).normalized);
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
