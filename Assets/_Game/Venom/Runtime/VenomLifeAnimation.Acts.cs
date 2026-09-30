using System;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// The acts of <see cref="COghePersonality"/>, drawn on the skin: the 32 field sources leave the simulated particles for a
    /// target shape (a hand, a heart, a star…) and come back; thin parts (fingers, star points, the hook of a question mark)
    /// are tubes like the tendrils. Shapes are authored in body radii (x right, y up, z toward the camera) and keep the mesh
    /// density of the live skin. Nothing here touches a particle, a force or the puzzle.
    /// </summary>
    public sealed partial class VenomLifeAnimation
    {
        private const int MaxSources = CohesiveOrganism.ParticleCount + 6, MaxLimbs = 8;
        private const float BodyRadius = .036f;
        private int actSerial = -1, shapeCount, limbCount;
        private readonly Vector3[] shapePoint = new Vector3[MaxSources];
        private readonly float[] shapeSupport = new float[MaxSources], shapeWeight = new float[MaxSources];
        private readonly Vector3[] limbPoint = new Vector3[MaxLimbs * 4];
        private readonly float[] limbRadius = new float[MaxLimbs * 2];
        private readonly bool[] limbCapped = new bool[MaxLimbs];
        private readonly int[] slotOf = new int[CohesiveOrganism.ParticleCount];
        private readonly bool[] slotUsed = new bool[MaxSources], pointUsed = new bool[CohesiveOrganism.ParticleCount];
        private readonly float[] pairDistance = new float[CohesiveOrganism.ParticleCount * MaxSources];
        private readonly int[] pairIndex = new int[CohesiveOrganism.ParticleCount * MaxSources];
        private Vector3 actRight = Vector3.right, actUp = Vector3.up, actToCamera = Vector3.back, actRest;
        private float actTilt;

        /// <summary>No apparatus between <paramref name="from"/> and <paramref name="distance"/> along <paramref name="direction"/>.</summary>
        internal bool Clearance(Vector3 from, Vector3 direction, float distance)
            => direction.sqrMagnitude < 1e-8f || !boundaryQueries.Raycast(from, direction.normalized, distance, out _);

        internal bool Probe(Vector3 from, Vector3 direction, float distance, out Vector3 point, out Vector3 normal)
        {
            point = normal = Vector3.zero;
            if (!boundaryQueries.Raycast(from, direction.normalized, distance, out var hit)) return false;
            point = hit.point; normal = hit.normal; return true;
        }

        private int PerformAct(COghePersonality p, Vector3[] points, float[] supports, float[] weights, int count, Vector3 centre, Vector3 up, Vector3 floor)
        {
            float s = BodyRadius, t = p.Time, length = p.Length;
            Vector3 ground = centre - up * Vector3.Dot(centre - floor, up);
            if (p.Serial != actSerial) BeginAct(p, points, count, ground, up);
            DanceAmount = Mathf.Max(DanceAmount, p.Fade);
            switch (p.Act)
            {
                case COgheAct.Tantrum: return Tantrum(p, points, supports, count, centre, ground, t);
                case COgheAct.Melt: return Melt(points, supports, count, centre, ground, t, p.Fade);
                case COgheAct.Doze: return Doze(points, supports, weights, count, centre, ground, t, p.Fade);
                case COgheAct.GlassTap: return GlassTap(p, points, count, centre, ground, t);
            }
            // Wave and Shape: morph into the template and back
            float e = Smooth(0, .5f, t) * (1 - Smooth(length - .5f, length, t)) * p.Fade;
            BuildShape(p.Act, p.Shape, t, length);
            float boing = p.Act == COgheAct.Shape ? 1 + .06f * Mathf.Sin(Mathf.Clamp01((t - .5f) / .6f) * Mathf.PI * 3) * Smooth(.45f, .6f, t) * (1 - Smooth(1.2f, 1.6f, t)) : 1;
            for (int i = 0; i < count; i++)
            {
                int k = slotOf[i]; bool absorbed = k < 0; if (absorbed) k = -k - 1;
                Vector3 world = transform.TransformPoint(points[i]);
                Vector3 target = ToWorld(shapePoint[k] * boing, ground, s);
                points[i] = transform.InverseTransformPoint(Vector3.Lerp(world, target, e));
                supports[i] = Mathf.Lerp(supports[i], shapeSupport[k] * s, e);
                weights[i] = Mathf.Lerp(1, absorbed ? 0 : shapeWeight[k], e);
            }
            int n = count;
            for (int k = 0; k < shapeCount && n < MaxSources; k++)
            {
                if (slotUsed[k]) continue;   // extra sources beyond the body's 32 grow in with the morph
                points[n] = transform.InverseTransformPoint(ToWorld(shapePoint[k] * boing, ground, s));
                supports[n] = shapeSupport[k] * s; weights[n++] = shapeWeight[k] * e;
            }
            float grow = Smooth(.5f, 1, e);   // thin parts sprout from the shape once the body has mostly become it
            for (int l = 0; l < limbCount; l++)
            {
                Vector3 root = limbPoint[l * 4];
                Vector3 Q(int j) => ToWorld((root + (limbPoint[l * 4 + j] - root) * grow) * boing, ground, s);
                Limb(Q(0), Q(1), Q(2), Q(3), limbRadius[l * 2] * s * grow, limbRadius[l * 2 + 1] * s * grow, floor, up, limbCapped[l]);
            }
            HeadAmount = Mathf.Max(HeadAmount, e);
            return n;
        }

        private void BeginAct(COghePersonality p, Vector3[] points, int count, Vector3 ground, Vector3 up)
        {
            actSerial = p.Serial; actUp = up; actRest = ground;
            var view = level.View;
            Vector3 right = Vector3.ProjectOnPlane(view != null ? view.transform.right : Vector3.right, up);
            actRight = right.sqrMagnitude > 1e-6f ? right.normalized : Vector3.right;
            actToCamera = Vector3.Cross(up, actRight);
            float pitch = view != null ? Mathf.Asin(Mathf.Clamp(-Vector3.Dot(view.transform.forward, up), -1, 1)) : 0;
            actTilt = Mathf.Clamp(pitch * .6f, 0, 35 * Mathf.Deg2Rad);   // lean the flat shapes back to face a camera looking down
            if (p.Act != COgheAct.Wave && p.Act != COgheAct.Shape) return;
            // match each particle to the nearest free source of the shape at its peak; left-over particles melt into it
            BuildShape(p.Act, p.Shape, p.Length * .5f, p.Length);
            int pairs = 0;
            for (int i = 0; i < count; i++)
            {
                Vector3 world = transform.TransformPoint(points[i]);
                for (int k = 0; k < shapeCount; k++)
                { pairDistance[pairs] = (world - ToWorld(shapePoint[k], ground, BodyRadius)).sqrMagnitude; pairIndex[pairs++] = i * MaxSources + k; }
            }
            Array.Sort(pairDistance, pairIndex, 0, pairs);
            Array.Clear(slotUsed, 0, slotUsed.Length); Array.Clear(pointUsed, 0, pointUsed.Length);
            for (int q = 0; q < pairs; q++)
            {
                int i = pairIndex[q] / MaxSources, k = pairIndex[q] % MaxSources;
                if (pointUsed[i] || slotUsed[k]) continue;
                pointUsed[i] = slotUsed[k] = true; slotOf[i] = k;
            }
            for (int i = 0; i < count; i++)
            {
                if (pointUsed[i]) continue;
                Vector3 world = transform.TransformPoint(points[i]); float best = float.MaxValue;
                for (int k = 0; k < shapeCount; k++)
                {
                    float d = (world - ToWorld(shapePoint[k], ground, BodyRadius)).sqrMagnitude;
                    if (d < best) { best = d; slotOf[i] = -k - 1; }
                }
            }
        }

        /// <summary>Shape space (body radii; x right, y up, z toward the camera, leaned back to face it) to world.</summary>
        private Vector3 ToWorld(Vector3 q, Vector3 ground, float s)
        {
            float c = Mathf.Cos(actTilt), n = Mathf.Sin(actTilt);
            float y = q.y * c + q.z * n, z = q.z * c - q.y * n;
            return ground + actRight * (q.x * s) + actUp * (y * s) + actToCamera * (z * s);
        }

        // Shapes -----------------------------------------------------------------------------------------------------------
        private void Src(float x, float y, float z, float support, float weight = 1)
        {
            if (shapeCount >= MaxSources) return;
            shapePoint[shapeCount] = new Vector3(x, y, z); shapeSupport[shapeCount] = support; shapeWeight[shapeCount++] = weight;
        }

        private void LimbQ(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float r0, float r1, bool capped = true)
        {
            if (limbCount >= MaxLimbs) return;
            limbCapped[limbCount] = capped;
            limbPoint[limbCount * 4] = a; limbPoint[limbCount * 4 + 1] = b; limbPoint[limbCount * 4 + 2] = c; limbPoint[limbCount * 4 + 3] = d;
            limbRadius[limbCount * 2] = r0; limbRadius[limbCount * 2 + 1] = r1; limbCount++;
        }

        private void Straight(Vector3 a, Vector3 d, float r0, float r1) => LimbQ(a, Vector3.Lerp(a, d, .33f), Vector3.Lerp(a, d, .66f), d, r0, r1);

        private static Vector3 Spin(Vector3 q, Vector3 pivot, float degrees)
            => pivot + Quaternion.AngleAxis(degrees, Vector3.forward) * (q - pivot);

        private void Puddle(float radius, float height, int ring, float support)
        {
            Src(0, height, 0, support * 1.1f);
            for (int i = 0; i < ring; i++)
            {
                float a = i * Mathf.PI * 2 / ring;
                Src(Mathf.Cos(a) * radius, height * .8f, Mathf.Sin(a) * radius, support);
            }
        }

        private void BuildShape(COgheAct act, COgheShape shape, float t, float length)
        {
            shapeCount = limbCount = 0;
            if (act == COgheAct.Wave)
            {
                // a hand rises out of the body, palm to the camera, and waves from the wrist
                Puddle(.55f, .3f, 5, .72f);
                Src(0, .75f, 0, .66f); Src(0, 1.1f, 0, .64f);
                float wave = Mathf.Sin((t - .55f) * Mathf.PI * 2 * 2.1f) * 22 * Smooth(.5f, .8f, t) * (1 - Smooth(length - .75f, length - .45f, t));
                var wrist = new Vector3(0, 1.3f, 0);
                for (int r = 0; r < 3; r++) for (int c = -1; c <= 1; c++)
                {
                    var q = Spin(new Vector3(c * .4f, 1.6f + r * .32f, 0), wrist, wave);
                    Src(q.x, q.y, q.z, .6f);
                }
                float[] fx = { -.44f, -.15f, .15f, .44f }, fl = { .72f, .88f, .9f, .74f }, fs = { -9, -3, 3, 9 };
                for (int f = 0; f < 4; f++)
                {
                    var root = new Vector3(fx[f], 2.2f, 0);
                    var tip = root + Quaternion.AngleAxis(-fs[f], Vector3.forward) * Vector3.up * fl[f];
                    Straight(Spin(root, wrist, wave), Spin(tip, wrist, wave), .19f, .15f);
                }
                Straight(Spin(new Vector3(.52f, 1.72f, 0), wrist, wave), Spin(new Vector3(1.0f, 2.2f, .05f), wrist, wave), .2f, .16f);
                return;
            }
            switch (shape)
            {
                case COgheShape.Heart:
                    Src(-.6f, 2.12f, 0, .98f); Src(.6f, 2.12f, 0, .98f);
                    Src(-.95f, 1.85f, 0, .72f); Src(.95f, 1.85f, 0, .72f);
                    Src(-.55f, 1.45f, 0, .85f); Src(.55f, 1.45f, 0, .85f); Src(0, 1.55f, 0, .8f);
                    Src(-.3f, 1.02f, 0, .72f); Src(.3f, 1.02f, 0, .72f); Src(0, .72f, 0, .58f); Src(0, .44f, 0, .42f);
                    break;
                case COgheShape.Star:
                {
                    var c = new Vector3(0, 1.15f, 0);
                    Src(c.x, c.y, 0, 1.15f); Src(c.x, c.y + .22f, 0, .78f); Src(c.x - .22f, c.y - .14f, 0, .78f); Src(c.x + .22f, c.y - .14f, 0, .78f);
                    for (int a = 0; a < 5; a++)
                    {
                        float angle = (90 + a * 72) * Mathf.Deg2Rad;
                        var dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                        Straight(c + dir * .22f, c + dir * 1.18f, .5f, .1f);
                    }
                    break;
                }
                case COgheShape.Question:
                    Puddle(.35f, .38f, 4, .72f);   // the dot is the body
                    LimbQ(new Vector3(0, 1.08f, 0), new Vector3(0, 1.5f, 0), new Vector3(.62f, 1.65f, 0), new Vector3(.62f, 2.15f, 0), .2f, .2f, false);
                    LimbQ(new Vector3(.62f, 2.15f, 0), new Vector3(.62f, 2.8f, 0), new Vector3(-.6f, 2.85f, 0), new Vector3(-.58f, 2.22f, 0), .2f, .19f);
                    Src(0, 1.0f, 0, .34f);
                    break;
                case COgheShape.Mushroom:
                    for (int i = 0; i < 4; i++) Src(0, .3f + i * .36f, 0, .62f);
                    for (int i = 0; i < 9; i++)
                    {
                        float a = i * Mathf.PI * 2 / 9;
                        Src(Mathf.Cos(a) * .8f, 1.74f, Mathf.Sin(a) * .8f, .84f);
                    }
                    Src(0, 2.05f, 0, 1.0f); Src(0, 1.85f, 0, .85f);
                    break;
                case COgheShape.Snowman:
                    // spaced so the field keeps a waist between balls (solved for necks ~40% of the ball above them)
                    Src(0, .52f, 0, .95f, 1.3f); Src(0, 1.56f, 0, .72f, 1.3f); Src(0, 2.3f, 0, .52f, 1.3f);
                    Straight(new Vector3(-.34f, 1.6f, 0), new Vector3(-1.1f, 2.02f, 0), .12f, .07f);
                    Straight(new Vector3(.34f, 1.6f, 0), new Vector3(1.1f, 2.02f, 0), .12f, .07f);
                    Straight(new Vector3(-.9f, 1.92f, 0), new Vector3(-1.05f, 2.32f, 0), .08f, .05f);
                    Straight(new Vector3(.9f, 1.92f, 0), new Vector3(1.05f, 2.32f, 0), .08f, .05f);
                    break;
                case COgheShape.ThumbsUp:
                    Puddle(.5f, .28f, 4, .66f);
                    Src(-.25f, 1.05f, 0, .74f); Src(.2f, 1.05f, 0, .74f); Src(-.25f, 1.45f, 0, .72f); Src(.2f, 1.45f, 0, .72f); Src(0, .75f, 0, .66f);
                    LimbQ(new Vector3(.12f, 1.62f, .05f), new Vector3(.14f, 1.95f, .05f), new Vector3(.12f, 2.2f, .05f), new Vector3(.1f, 2.45f, .05f), .24f, .2f);
                    break;
                case COgheShape.Rocket:
                    for (int i = 0; i < 5; i++) Src(0, .55f + i * .42f, 0, .72f);
                    Src(0, 2.72f, 0, .5f); Src(0, 2.95f, 0, .32f);
                    Straight(new Vector3(-.3f, .85f, 0), new Vector3(-.8f, .22f, 0), .18f, .07f);
                    Straight(new Vector3(.3f, .85f, 0), new Vector3(.8f, .22f, 0), .18f, .07f);
                    Straight(new Vector3(0, .85f, .3f), new Vector3(0, .22f, .78f), .18f, .07f);
                    break;
                case COgheShape.Umbrella:
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * Mathf.PI * 2 / 8;
                        Src(Mathf.Cos(a) * .92f, 2.12f, Mathf.Sin(a) * .92f, .62f);
                    }
                    Src(0, 2.42f, 0, .88f); Src(0, 2.25f, 0, .8f);
                    LimbQ(new Vector3(0, 2.15f, 0), new Vector3(0, 1.4f, 0), new Vector3(0, .9f, 0), new Vector3(0, .42f, 0), .13f, .13f);
                    LimbQ(new Vector3(0, .42f, 0), new Vector3(0, .05f, 0), new Vector3(.42f, .02f, 0), new Vector3(.42f, .32f, 0), .13f, .12f);
                    break;
            }
        }

        // Acts drawn from the body itself ---------------------------------------------------------------------------------
        private int Melt(Vector3[] points, float[] supports, int count, Vector3 centre, Vector3 ground, float t, float fade)
        {
            // it sags into a puddle, ripples, then pops back up a little too tall and settles
            float melt = Smooth(0, .8f, t) * (1 - Smooth(1.45f, 1.7f, t));
            float pop = Mathf.Sin(Mathf.Clamp01((t - 1.55f) / .6f) * Mathf.PI * 2.5f) * Mathf.Exp(-(t - 1.55f) * 3) * (t > 1.55f ? 1 : 0);
            float ripple = Mathf.Sin(t * 14) * .08f * melt;
            for (int i = 0; i < count; i++)
            {
                Vector3 w = transform.TransformPoint(points[i]), d = w - ground;
                float h = Vector3.Dot(d, actUp); Vector3 flat = d - actUp * h;
                float spread = 1 + (.42f + ripple) * melt - .12f * pop;
                float height = h * (1 - .82f * melt) * (1 + .3f * pop);
                Vector3 target = ground + flat * spread + actUp * height;
                points[i] = transform.InverseTransformPoint(Vector3.Lerp(w, target, fade));
                supports[i] *= 1 + .16f * melt * fade;
            }
            return count;
        }

        private int Doze(Vector3[] points, float[] supports, float[] weights, int count, Vector3 centre, Vector3 ground, float t, float fade)
        {
            // it settles low and breathes slowly; now and then a bubble rises off it and pops
            float settle = Smooth(0, 1.2f, t) * fade, breath = Mathf.Sin(t * Mathf.PI * 2 / 3.4f);
            float squash = 1 - .26f * settle + .05f * breath * settle;
            float top = 0;
            for (int i = 0; i < count; i++)
            {
                Vector3 w = transform.TransformPoint(points[i]), d = w - ground;
                float h = Vector3.Dot(d, actUp); Vector3 flat = d - actUp * h;
                top = Mathf.Max(top, h * squash);
                points[i] = transform.InverseTransformPoint(ground + flat * (1 + (1 - squash) * .5f) + actUp * (h * squash));
            }
            float cycle = Mathf.Repeat(t - 1.5f, 3.4f) / 1.6f;
            if (t > 1.5f && cycle < 1 && settle > .5f)
            {
                // the bubble swells on top of the body, lifts off and pops
                float swell = Smooth(0, .55f, cycle), lift = Smooth(.5f, 1, cycle), size = (.35f + .4f * swell) * (cycle < .92f ? 1 : 0) * settle;
                Vector3 bubble = ground + actUp * (top + .004f + swell * .006f + lift * .03f) + actRight * (.01f + .006f * lift);
                points[count] = transform.InverseTransformPoint(bubble); supports[count] = BodyRadius * size; weights[count++] = size > .05f ? 1.6f : 0;
            }
            return count;
        }

        private int GlassTap(COghePersonality p, Vector3[] points, int count, Vector3 centre, Vector3 ground, float t)
        {
            // it leans toward the nearest glass and knocks on it three times with a tendril
            float e = Smooth(0, .35f, t) * (1 - Smooth(p.Length - .35f, p.Length, t)) * p.Fade;
            Vector3 toWall = p.HasWall ? Vector3.ProjectOnPlane(p.WallPoint - centre, actUp) : actRight * .06f;
            Vector3 dir = toWall.normalized;
            float top = 0;
            for (int i = 0; i < count; i++)
            {
                Vector3 w = transform.TransformPoint(points[i]); float h = Vector3.Dot(w - ground, actUp);
                top = Mathf.Max(top, h);
                points[i] = transform.InverseTransformPoint(w + dir * (Mathf.Clamp01(h / .05f) * .008f * e));
            }
            float knock = 0;
            for (int k = 0; k < 3; k++) knock = Mathf.Max(knock, Mathf.Sin(Mathf.Clamp01((t - .55f - k * .32f) / .3f) * Mathf.PI));
            Vector3 root = ground + actUp * (top * .8f) + dir * .012f;
            Vector3 wall = p.HasWall ? p.WallPoint + actUp * (top * .9f) : root + dir * .06f;
            Vector3 tip = Vector3.Lerp(wall - dir * .03f, wall - dir * .004f, knock);
            Limb(root, root + actUp * .02f + dir * .01f, tip - dir * .018f + actUp * .012f, tip, .0075f * e, .0055f * e, ground, actUp);
            TendrilCount++;
            return count;
        }

        private int Tantrum(COghePersonality p, Vector3[] points, float[] supports, int count, Vector3 centre, Vector3 ground, float t)
        {
            // fumes, jumps with a flip, splats on the glass, slides down, hops back where it was and shakes it off
            float s = BodyRadius;
            Vector3 up = actUp, rest = centre;
            bool wall = p.HasWall;
            Vector3 normal = wall ? p.WallNormal : up;
            Vector3 splat = wall ? p.WallPoint + normal * (s * .45f) + up * (s * 1.1f) : ground + up * (s * .35f);
            Vector3 slid = wall ? p.WallPoint + normal * (s * .5f) + up * (s * .55f) : splat;
            Vector3 flipAxis = wall ? Vector3.Cross(up, -normal).normalized : actRight;
            Vector3 position = rest; Quaternion turn = Quaternion.identity;
            Vector3 squashAxis = up; float squash = 1, jitter = 0, splatting = 0;
            if (t < .45f)
            {
                squash = 1 - .2f * Smooth(0, .2f, t); jitter = .0022f * Smooth(0, .15f, t);
            }
            else if (t < 1.05f)
            {
                float u = (t - .45f) / .6f;
                Vector3 to = wall ? splat : rest;
                position = Vector3.Lerp(rest, to, Smooth(0, 1, u)) + up * (Mathf.Sin(u * Mathf.PI) * (wall ? s * 1.6f : s * 2.8f));
                turn = Quaternion.AngleAxis(360 * Smooth(0, 1, u), flipAxis);
                squash = 1 + .18f * Mathf.Sin(u * Mathf.PI);
            }
            else if (t < 2.1f)
            {
                float u = Mathf.Clamp01((t - 1.05f) / 1.05f);
                position = Vector3.Lerp(splat, slid, Smooth(.2f, 1, u));
                squashAxis = normal; squash = Mathf.Lerp(.3f, .48f, Smooth(.3f, 1, u)) + .1f * Mathf.Sin(u * 20) * (1 - u);
                splatting = 1 - Smooth(.7f, 1, u);
            }
            else if (t < 2.6f)
            {
                float u = (t - 2.1f) / .5f;
                position = Vector3.Lerp(wall ? slid : splat, rest, Smooth(0, 1, u)) + up * (Mathf.Sin(u * Mathf.PI) * s * 1.1f);
                squash = 1 + .12f * Mathf.Sin(u * Mathf.PI);
            }
            else
            {
                float u = t - 2.6f;
                squash = 1 - .22f * Mathf.Cos(u * 24) * Mathf.Exp(-u * 7);
                turn = Quaternion.AngleAxis(Mathf.Sin(u * 30) * 14 * Mathf.Exp(-u * 6), up);
            }
            for (int i = 0; i < count; i++)
            {
                Vector3 d = transform.TransformPoint(points[i]) - rest;
                float along = Vector3.Dot(d, squashAxis);
                Vector3 shaped = (d - squashAxis * along) / Mathf.Sqrt(squash) + squashAxis * (along * squash);
                shaped = turn * shaped;
                if (jitter > 0) shaped += actRight * (Mathf.Sin(t * 190 + i) * jitter) + actToCamera * (Mathf.Cos(t * 170 + i * 2) * jitter);
                points[i] = transform.InverseTransformPoint(position + shaped);
                supports[i] *= 1 + .22f * splatting;   // a flattened body stays one sheet
            }
            DanceAmount = 1;
            return count;
        }

        /// <summary>A round-tipped tube (finger, star point, hook): radius <paramref name="r0"/> at the root to
        /// <paramref name="r1"/>, closing over the last sixth.</summary>
        private void Limb(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float r0, float r1, Vector3 floor, Vector3 up, bool capped = true)
        {
            if (r0 < .0004f) return;
            int first = vertices.Count;
            for (int ring = 0; ring <= Segments; ring++)
            {
                float t = ring / (float)Segments, u = 1 - t;
                Vector3 p = u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
                Vector3 tangent = (3 * u * u * (p1 - p0) + 6 * u * t * (p2 - p1) + 3 * t * t * (p3 - p2));
                if (tangent.sqrMagnitude < 1e-12f) tangent = p3 - p0;
                tangent.Normalize();
                Vector3 right = Vector3.Cross(tangent, up);
                if (right.sqrMagnitude < 1e-4f) right = Vector3.Cross(tangent, Mathf.Abs(tangent.x) < .8f ? Vector3.right : Vector3.forward);
                right.Normalize();
                Vector3 binormal = Vector3.Cross(right, tangent).normalized;
                float cap = t < .84f || !capped ? 1 : Mathf.Sqrt(Mathf.Max(0, 1 - Mathf.Pow((t - .84f) / .16f, 2)));
                float radius = Mathf.Lerp(r0, r1, t) * Mathf.Max(cap, .08f);
                p += up * Mathf.Max(0, radius - Vector3.Dot(p - floor, up));
                for (int side = 0; side < Sides; side++)
                {
                    float angle = side * Mathf.PI * 2 / Sides;
                    Vector3 n = right * Mathf.Cos(angle) + binormal * Mathf.Sin(angle);
                    vertices.Add(transform.InverseTransformPoint(p + n * radius));
                    normals.Add(transform.InverseTransformDirection(Vector3.Lerp(n, tangent, 1 - cap).normalized));
                    if (ring == Segments) continue;
                    int a = first + ring * Sides + side, b = first + ring * Sides + (side + 1) % Sides;
                    triangles.Add(a); triangles.Add(a + Sides); triangles.Add(b);
                    triangles.Add(b); triangles.Add(a + Sides); triangles.Add(b + Sides);
                }
            }
        }
    }
}
