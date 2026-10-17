using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// COghe's Emotion tricks (ability cards, Mrk 16/10/2026), drawn on the skin like the other acts: the body's sources are
    /// squashed, swayed and swelled about the spot where COghe stands, tendrils are tubes, and the tricks with a face use a
    /// small version of the monster head (its own eyes, fangs and red tongue). Nothing here touches a particle, a force or the
    /// puzzle; the skin returns to the body as each trick ends.
    /// </summary>
    public sealed partial class VenomLifeAnimation
    {
        private int TrickAct(COghePersonality p, Vector3[] points, float[] supports, float[] weights, int count, Vector3 centre, Vector3 ground, Vector3 up, Vector3 floor, float t)
        {
            if (COgheTricks.UsesFace(p.Trick)) return MonsterBody(p, TrickFacePose(p.Trick, t, p.Length), points, supports, weights, count, centre, ground, up, floor, t);
            float e = Smooth(0, .3f, t) * (1 - Smooth(p.Length - .35f, p.Length, t)) * p.Fade;
            DanceAmount = Mathf.Max(DanceAmount, e);
            switch (p.Trick)
            {
                case COgheTrick.Groove: return Groove(points, count, centre, ground, floor, t, e);
                case COgheTrick.Clap: return Clap(points, count, centre, ground, floor, t, e);
                case COgheTrick.PuffUp: return PuffUp(points, supports, weights, count, centre, ground, floor, t, e);
                case COgheTrick.Sneeze: return Sneeze(points, supports, weights, count, ground, t, e);
                case COgheTrick.BellyDrum: return BellyDrum(points, supports, weights, count, ground, floor, t, e);
                case COgheTrick.BackTurnSulk: return BackTurnSulk(points, count, ground, floor, t, e);
                case COgheTrick.Moonwalk: return Moonwalk(points, count, ground, floor, t, e, p.Length);
                case COgheTrick.Flex: return Flex(points, supports, weights, count, ground, floor, t, e);
                case COgheTrick.TailChase: return TailChase(points, supports, weights, count, centre, ground, floor, t, e);
                case COgheTrick.Selfie: return Selfie(points, count, ground, floor, t, e);
                case COgheTrick.Juggle: return Juggle(points, count, ground, floor, t, e);
                case COgheTrick.Mimic: return Mimic(p, points, count, ground, floor, t, e);
                case COgheTrick.RockPaperScissors: return RockPaperScissors(p, points, supports, weights, count, ground, floor, t, e);
                case COgheTrick.BreathHeart: return BreathHeart(points, count, ground, floor, t, e);
                case COgheTrick.BubblePop: return BubblePop(points, supports, weights, count, ground, t, e);
                case COgheTrick.Skater: return Skater(points, count, ground, floor, t, e);
            }
            return count;
        }

        /// <summary>The body squashed (volume kept) along up by <paramref name="squash"/>, turned about the ground point, shifted
        /// by <paramref name="offset"/> and scaled by <paramref name="scale"/>, blended in by <paramref name="e"/>. Returns the
        /// posed body's top above the ground.</summary>
        /// <summary>The body's half width across (metres) before posing: arms are sized to it (wide at Home, small in a level).</summary>
        private float BodyHalfWidth(Vector3[] points, int count, Vector3 ground)
        {
            float r = 0;
            for (int i = 0; i < count; i++) r = Mathf.Max(r, Vector3.ProjectOnPlane(transform.TransformPoint(points[i]) - ground, actUp).magnitude);
            return Mathf.Max(r, BodyRadius * .8f);
        }

        private float PoseBody(Vector3[] points, int count, Vector3 ground, float squash, Quaternion turn, Vector3 offset, float scale, float e)
        {
            float top = 0;
            for (int i = 0; i < count; i++)
            {
                Vector3 w = transform.TransformPoint(points[i]), d = (w - ground) * scale;
                float along = Vector3.Dot(d, actUp);
                Vector3 shaped = turn * ((d - actUp * along) / Mathf.Sqrt(squash) + actUp * (along * squash));
                Vector3 posed = ground + offset + shaped;
                top = Mathf.Max(top, Vector3.Dot(posed - ground, actUp));
                points[i] = transform.InverseTransformPoint(Vector3.Lerp(w, posed, e));
            }
            return top;
        }

        // Lv 1 · Groove: it bobs to the beat and sways, two little arms up in turn.
        private int Groove(Vector3[] points, int count, Vector3 centre, Vector3 ground, Vector3 floor, float t, float e)
        {
            const float beat = 2.2f; float s = BodyHalfWidth(points, count, ground);
            float phase = t * beat * Mathf.PI * 2, bounce = .5f + .5f * Mathf.Cos(phase);
            float squash = 1 - .09f * (1 - bounce);
            var turn = Quaternion.AngleAxis(10 * Mathf.Sin(phase * .5f), actToCamera);
            float top = PoseBody(points, count, ground, squash, turn, actUp * (s * .12f * bounce), 1, e);
            for (int side = -1; side <= 1; side += 2)
            {
                float lift = Mathf.Max(0, Mathf.Sin(phase + (side < 0 ? 0 : Mathf.PI)));
                Vector3 root = ground + turn * (actUp * (top * .7f) + actRight * (side * s * .62f));
                Vector3 tip = root + actUp * (s * (.45f + .55f * lift)) + actRight * (side * s * (.45f - .15f * lift)) + actToCamera * (s * .25f);
                Limb(root, root + actRight * (side * s * .25f), tip - actUp * (s * .25f), tip, s * .2f * e, s * .15f * e, floor, actUp);
                TendrilCount++;
            }
            return count;
        }

        // Lv 2 · Clap: two tendrils clap three times in front of it when a device it worked has finished.
        private int Clap(Vector3[] points, int count, Vector3 centre, Vector3 ground, Vector3 floor, float t, float e)
        {
            float s = BodyHalfWidth(points, count, ground), clap = 0;
            for (int k = 0; k < 3; k++) clap = Mathf.Max(clap, Mathf.Clamp01(1 - Mathf.Abs(t - (.58f + k * .35f)) / .14f));
            float squash = 1 - .06f * clap, arms = Smooth(.15f, .45f, t) * (1 - Smooth(1.9f, 2.2f, t)) * e;
            float top = PoseBody(points, count, ground, squash, Quaternion.AngleAxis(-6 * e, actRight), actToCamera * (s * .1f), 1, e);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 root = ground + actUp * (top * .58f) + actRight * (side * s * .62f);
                Vector3 open = root + actRight * (side * s * .55f) + actToCamera * (s * .75f) + actUp * (s * .35f);
                Vector3 shut = ground + actUp * (top * .72f) + actToCamera * (s * 1.15f) + actRight * (side * s * .07f);
                Vector3 tip = Vector3.Lerp(open, shut, Smooth(0, 1, clap));
                Limb(root, root + actRight * (side * s * .3f) + actToCamera * (s * .2f), tip - actToCamera * (s * .25f), tip, s * .2f * arms, s * .17f * arms, floor, actUp);
                TendrilCount++;
            }
            return count;
        }

        // Lv 3 · Puff up: after a fall it swells with anger, stamps twice, lets off two puffs of steam, then sighs it out.
        private int PuffUp(Vector3[] points, float[] supports, float[] weights, int count, Vector3 centre, Vector3 ground, Vector3 floor, float t, float e)
        {
            float s = BodyRadius;
            float swell = Smooth(0, .5f, t) * (1 - Smooth(1.7f, 2.2f, t));
            float Bump(float at) => Mathf.Clamp01(1 - Mathf.Abs(t - at) / .12f);
            float hop = Mathf.Max(Mathf.Sin(Mathf.Clamp01((t - .55f) / .2f) * Mathf.PI), Mathf.Sin(Mathf.Clamp01((t - .95f) / .2f) * Mathf.PI));
            float squash = 1 - .17f * Mathf.Max(Bump(.78f), Bump(1.18f)) + .05f * hop;
            float wobble = t > 1.7f ? .09f * Mathf.Sin((t - 1.7f) * 30) * Mathf.Exp(-(t - 1.7f) * 5) : 0;
            float top = PoseBody(points, count, ground, squash + wobble, Quaternion.identity, actUp * (s * .35f * hop * (1 - Mathf.Max(Bump(.78f), Bump(1.18f)))), 1 + .22f * swell, e);
            for (int i = 0; i < count; i++) supports[i] *= 1 + .18f * swell * e;
            int n = count;
            for (int k = 0; k < 2 && n < MaxSources; k++)
            {
                float u = Mathf.Clamp01((t - (.95f + k * .32f)) / .6f);
                if (u <= 0 || u >= 1) continue;
                float side = k == 0 ? -1 : 1, size = .3f * Mathf.Sin(u * Mathf.PI);
                points[n] = transform.InverseTransformPoint(ground + actUp * (top + s * (.15f + 1.1f * u)) + actRight * (side * s * (.35f + .25f * u)));
                supports[n] = s * size; weights[n++] = size > .05f ? 1.4f * e : 0;
            }
            return n;
        }

        // Lv 3 · Sneeze: "ah… ah…" (it stretches up, head back) then "choo!": it lunges, a droplet flies out, lands, and slides
        // back into it.
        private int Sneeze(Vector3[] points, float[] supports, float[] weights, int count, Vector3 ground, float t, float e)
        {
            float s = BodyHalfWidth(points, count, ground);
            float ah = Mathf.Max(Smooth(.15f, .45f, t) * (1 - Smooth(.5f, .62f, t)) * .6f, Smooth(.6f, .98f, t)) * (1 - Smooth(1.02f, 1.12f, t));
            float choo = Mathf.Sin(Mathf.Clamp01((t - 1.02f) / .4f) * Mathf.PI);
            float squash = 1 + .16f * ah - .2f * choo;
            var turn = Quaternion.AngleAxis(-12 * ah + 16 * choo, actRight);
            PoseBody(points, count, ground, squash, turn, actToCamera * (s * .25f * choo), 1, e);
            int n = count;
            if (t > 1.05f && t < 2.6f && n < MaxSources)
            {
                // flies out toward the camera and to the side, bounces once, then slides home along the floor
                Vector3 away = (actToCamera * .8f + actRight * .6f).normalized * (s * 2.1f);
                float fly = Smooth(1.05f, 1.55f, t), home = Smooth(1.9f, 2.6f, t);
                Vector3 at = ground + away * fly * (1 - home) + actUp * (s * (.35f * Mathf.Sin(Mathf.Clamp01((t - 1.05f) / .5f) * Mathf.PI) + .12f));
                float size = .32f * (1 - Smooth(2.4f, 2.6f, t));
                points[n] = transform.InverseTransformPoint(at); supports[n] = BodyRadius * size; weights[n++] = size > .04f ? 1.5f * e : 0;
            }
            return n;
        }

        // Lv 4 · Belly drum (Home): two tendrils drum on its belly in turn, the belly jiggles, then a burp bubble pops.
        private int BellyDrum(Vector3[] points, float[] supports, float[] weights, int count, Vector3 ground, Vector3 floor, float t, float e)
        {
            float s = BodyHalfWidth(points, count, ground), hit = 0;
            for (int k = 0; k < 8; k++) hit = Mathf.Max(hit, Mathf.Clamp01(1 - Mathf.Abs(t - (.45f + k * .25f)) / .09f));
            float burp = Mathf.Sin(Mathf.Clamp01((t - 2.55f) / .35f) * Mathf.PI);
            float top = PoseBody(points, count, ground, 1 - .05f * hit + .06f * burp, Quaternion.AngleAxis(-5 * e, actRight), Vector3.zero, 1 + .05f * burp, e);
            float arms = Smooth(.2f, .4f, t) * (1 - Smooth(2.45f, 2.7f, t)) * e;
            for (int side = -1; side <= 1; side += 2)
            {
                int beat = Mathf.FloorToInt((t - .45f) / .25f + .5f);
                bool mine = (beat & 1) == (side < 0 ? 0 : 1);
                float down = mine ? Mathf.Clamp01(1 - Mathf.Abs(t - (.45f + beat * .25f)) / .12f) : 0;
                Vector3 root = ground + actUp * (top * .62f) + actRight * (side * s * .7f);
                Vector3 belly = ground + actUp * (top * .42f) + actToCamera * (s * .78f) + actRight * (side * s * .28f);
                Vector3 tip = Vector3.Lerp(belly + actToCamera * (s * .35f) + actUp * (s * .25f), belly, down);
                Limb(root, root + actRight * (side * s * .25f) + actToCamera * (s * .2f), tip + actUp * (s * .2f), tip, s * .18f * arms, s * .16f * arms, floor, actUp);
                TendrilCount++;
            }
            int n = count;
            if (t > 2.6f && n < MaxSources)
            {
                float u = Mathf.Clamp01((t - 2.6f) / .6f), size = .38f * Smooth(0, .5f, u) * (u < .92f ? 1 : 0);
                points[n] = transform.InverseTransformPoint(ground + actUp * (top + s * (.1f + .5f * u)) + actToCamera * (s * .3f));
                supports[n] = BodyRadius * size; weights[n++] = size > .04f ? 1.4f * e : 0;
            }
            return n;
        }

        // Lv 5 · Back-turn sulk: it slumps away from the player and will not look (no eyes), "hmph", then peeks back sulkily.
        private int BackTurnSulk(Vector3[] points, int count, Vector3 ground, Vector3 floor, float t, float e)
        {
            float away = Smooth(.1f, .5f, t) * (1 - Smooth(2.0f, 2.5f, t) * .6f), shiver = t > .45f && t < .9f ? Mathf.Sin(t * 70) * .015f : 0;
            var turn = Quaternion.AngleAxis(16 * away, actRight) * Quaternion.AngleAxis(shiver * 60, actUp);
            PoseBody(points, count, ground, 1 - .14f * away, turn, -actToCamera * (BodyRadius * .3f * away), 1, e);
            return count;
        }

        // Lv 5 · Moonwalk (Home; the win dance has its own): it stands up on two little feet, glides backward with them
        // sliding in turn, then spins back to where it stood (the body itself never moves) and poses.
        private int Moonwalk(Vector3[] points, int count, Vector3 ground, Vector3 floor, float t, float e, float length)
        {
            float s = BodyHalfWidth(points, count, ground), glide = Smooth(.3f, 2.9f, t) * (1 - Smooth(2.9f, 3.5f, t)), spin = Smooth(3.0f, 3.5f, t);
            Vector3 travel = -actRight * (s * .4f * glide);   // close up at Home it still stays in frame
            var turn = Quaternion.AngleAxis(360 * spin, actUp) * Quaternion.AngleAxis(-8 * Smooth(.2f, .5f, t) * (1 - spin), actToCamera);
            float stand = s * .45f * Smooth(.05f, .35f, t) * (1 - Smooth(length - .55f, length - .15f, t));
            float top = PoseBody(points, count, ground, 1.12f + .04f * Mathf.Sin(t * 12), turn, travel + actUp * stand, .88f, e);
            Vector3 foot = ground + travel;
            for (int side = -1; side <= 1; side += 2)
            {
                // at the front edge of the raised body, where the camera (from above) sees under it
                float slide = Mathf.Sin(t * 6.5f + (side < 0 ? 0 : Mathf.PI)) * (1 - spin);
                Vector3 root = foot + actUp * (stand * .9f) + actRight * (side * s * .35f) + actToCamera * (s * .7f);
                Vector3 tip = foot + actUp * (BodyRadius * .1f) + actRight * (side * s * .35f + s * .45f * slide) + actToCamera * (s * .85f);
                Limb(root, root - actUp * (stand * .4f), tip + actUp * (stand * .3f), tip, s * .17f * e, s * .15f * e, floor, actUp);
                TendrilCount++;
            }
            if (spin > .5f)
            {
                // the pose: one arm up at the end
                Vector3 shoulder = foot + actUp * (top * .7f) + actRight * (s * .5f), tip = shoulder + actUp * (s * 1.1f) + actRight * (s * .3f);
                Limb(shoulder, shoulder + actUp * (s * .4f), tip - actUp * (s * .3f), tip, s * .17f * e * spin, s * .13f * e * spin, floor, actUp);
                TendrilCount++;
            }
            return count;
        }

        /// <summary>Where a trick wants the eyes to look (world): the tail in a tail chase, a dizzy circle after it.</summary>
        public Vector3 TrickFocus { get; private set; }

        private static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        { float u = 1 - t; return u * u * u * a + 3 * u * u * t * b + 3 * u * t * t * c + t * t * t * d; }

        // Lv 6 · Tail chase (Home): a little tail grows at its side and it spins after it, faster and faster, eyes on the
        // tail; it catches it with a pounce, the tail melts back in, and it wobbles round dizzily before shaking it off.
        private int TailChase(Vector3[] points, float[] supports, float[] weights, int count, Vector3 centre, Vector3 ground, Vector3 floor, float t, float e)
        {
            float s = BodyHalfWidth(points, count, ground);
            float run = Mathf.Clamp01((t - .35f) / 2.0f), angle = 720 * Mathf.Pow(run, 1.5f);
            float tail = Smooth(.1f, .45f, t) * (1 - Smooth(2.4f, 2.75f, t)) * e;
            float pounce = t > 2.3f ? Mathf.Sin(Mathf.Clamp01((t - 2.3f) / .3f) * Mathf.PI) : 0;
            float dizzy = Smooth(2.45f, 2.6f, t) * (1 - Smooth(3.1f, 3.4f, t)), shake = t > 3.35f ? Mathf.Sin((t - 3.35f) * 40) * Mathf.Exp(-(t - 3.35f) * 7) : 0;
            Vector3 dir = Quaternion.AngleAxis(angle, actUp) * actRight;
            Vector3 lean = Quaternion.AngleAxis(t * 520, actUp) * actRight;
            var turn = Quaternion.AngleAxis(angle * .35f, actUp) * Quaternion.AngleAxis(9 * dizzy, lean) * Quaternion.AngleAxis(14 * shake, actUp);
            Vector3 circle = Quaternion.AngleAxis(angle - 90, actUp) * actRight * (s * .1f * Smooth(.4f, .8f, t) * (1 - pounce));
            float top = PoseBody(points, count, ground, 1 - .14f * pounce + .03f * Mathf.Sin(angle * Mathf.Deg2Rad * 2) * tail, turn, circle, 1, e);
            Vector3 at = ground + circle;
            // the tail: out of the side the spin is leaving behind, curling up at the tip and wagging
            Vector3 root = at + actUp * (top * .3f) + dir * (s * .75f);
            Vector3 sideways = Vector3.Cross(actUp, dir);
            Vector3 tip = root + dir * (s * 1.0f * tail) + actUp * (s * .7f * tail) + sideways * (s * .18f * Mathf.Sin(t * 14) * tail);
            Limb(root - dir * (s * .2f), root + dir * (s * .5f * tail), tip - actUp * (s * .3f * tail), tip, s * .22f * tail, s * .1f * tail, floor, actUp);
            TendrilCount++;
            int n = count;
            if (n < MaxSources)
            { points[n] = transform.InverseTransformPoint(tip); supports[n] = s * .16f * tail; weights[n++] = tail > .05f ? 1.4f : 0; }   // a round tuft
            TrickFocus = t < 2.45f ? tip : centre + actUp * (s * .6f) + Quaternion.AngleAxis(t * 700, actUp) * actRight * (s * .5f);
            return n;
        }

        // Lv 7 · Flex: after a heavy job it stands tall; two smooth arms swell out of its sides and curl up into biceps,
        // pumped twice with a proud little bounce, then melt back in.
        private int Flex(Vector3[] points, float[] supports, float[] weights, int count, Vector3 ground, Vector3 floor, float t, float e)
        {
            float s = BodyHalfWidth(points, count, ground);
            float grow = Smooth(.1f, .7f, t) * (1 - Smooth(2.6f, 3.15f, t)) * e;
            float curl = Smooth(.45f, .95f, t) * (1 - Smooth(2.45f, 2.85f, t));
            float Pulse(float a, float b) => Smooth(a, a + .3f, t) * (1 - Smooth(b - .3f, b, t));
            float pump = Mathf.Max(Pulse(.95f, 1.6f), Pulse(1.7f, 2.4f));
            float top = PoseBody(points, count, ground, 1 + .08f * grow + .03f * pump, Quaternion.AngleAxis(-5 * grow, actRight), actUp * (s * .04f * pump), 1, e);
            int n = count;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 outward = actRight * side;
                Vector3 shoulder = ground + actUp * (top * .58f) + outward * (s * .55f);
                // straight out at first, then the forearm curls up: one smooth curve from the shoulder to the fist
                Vector3 elbow = shoulder + outward * (s * .8f * grow) + actToCamera * (s * .1f);
                Vector3 straight = elbow + outward * (s * .45f * grow);
                Vector3 curled = elbow + actUp * (s * (.62f + .08f * pump) * grow) - outward * (s * .12f) + actToCamera * (s * .05f);
                Vector3 fist = Vector3.Lerp(straight, curled, curl);
                Vector3 c1 = shoulder + outward * (s * .5f * grow), c2 = elbow + outward * (s * .12f * grow) + actUp * (s * .02f);
                Limb(shoulder, c1, c2, fist, s * .24f * grow, s * .17f * grow, floor, actUp, false);
                TendrilCount++;
                // the fist and the biceps are part of the liquid skin: round, and the biceps swells on each pump
                if (n + 1 < MaxSources)
                {
                    points[n] = transform.InverseTransformPoint(fist); supports[n] = s * .24f * grow; weights[n++] = grow > .05f ? 1.4f : 0;
                    Vector3 biceps = Bezier(shoulder, c1, c2, fist, .4f) + actUp * (s * (.05f + .1f * curl));
                    points[n] = transform.InverseTransformPoint(biceps); supports[n] = s * (.12f + .08f * curl + .12f * pump) * grow; weights[n++] = grow > .05f ? 1.4f : 0;
                }
            }
            return n;
        }

        // Lv 7 · Bubble pop (Home): it blows a glossy bubble that swells and wobbles in front of its face, then pops in its
        // face, a splash of droplets raining back onto it.
        private int BubblePop(Vector3[] points, float[] supports, float[] weights, int count, Vector3 ground, float t, float e)
        {
            float s = BodyHalfWidth(points, count, ground);
            float blow = Smooth(.3f, 1.9f, t), popped = t >= 2.0f ? 1 : 0;
            float flinch = t > 2.0f ? Mathf.Sin(Mathf.Clamp01((t - 2.0f) / .35f) * Mathf.PI) : 0;
            float top = PoseBody(points, count, ground, 1 + .05f * Mathf.Sin(t * 9) * blow * (1 - popped) - .12f * flinch,
                Quaternion.AngleAxis(-8 * blow * (1 - popped) + 10 * flinch, actRight), Vector3.zero, 1, e);
            int n = count;
            Vector3 mouth = ground + actUp * (top * .55f) + actToCamera * (s * .85f);
            if (popped == 0 && blow > .02f && n < MaxSources)
            {
                float r = BodyRadius * (.15f + 1.0f * blow) * (1 + .08f * Mathf.Sin(t * 14) * blow);
                points[n] = transform.InverseTransformPoint(mouth + actToCamera * (r * .8f) + actUp * (r * .15f));
                supports[n] = r; weights[n++] = 1.5f * e;
            }
            else if (popped > 0 && t < 2.9f)
            {
                float u = Mathf.Clamp01((t - 2.0f) / .9f);
                for (int k = 0; k < 5 && n < MaxSources; k++)
                {
                    float a = k / 5f * Mathf.PI * 2 + .4f;
                    Vector3 outward = (actRight * Mathf.Cos(a) + actUp * (Mathf.Sin(a) * .7f) + actToCamera * .4f).normalized;
                    Vector3 burst = mouth + actToCamera * (BodyRadius * .8f) + outward * (BodyRadius * (.3f + 1.4f * Smooth(0, .4f, u)));
                    Vector3 back = ground + actUp * (top * .7f) + actRight * (Mathf.Cos(a) * s * .5f);
                    float size = .2f * (1 - Smooth(.75f, 1f, u));
                    points[n] = transform.InverseTransformPoint(Vector3.Lerp(burst, back, Smooth(.35f, 1f, u)));
                    supports[n] = BodyRadius * size; weights[n++] = size > .03f ? 1.4f * e : 0;
                }
            }
            return n;
        }

        // Lv 8 · Skater: landing from a slide on a slick coating it glides on like a figure skater, arms out wide, wobbles
        // ("oops!") and steadies itself where it stands.
        private int Skater(Vector3[] points, int count, Vector3 ground, Vector3 floor, float t, float e)
        {
            float s = BodyHalfWidth(points, count, ground);
            float glide = Smooth(0, 1.3f, t) * (1 - Smooth(1.9f, 2.5f, t));
            float oops = t > 1.4f && t < 2.2f ? Mathf.Sin((t - 1.4f) * 22) * (1 - Mathf.InverseLerp(1.4f, 2.2f, t)) : 0;
            float pose = Smooth(.05f, .35f, t) * (1 - Smooth(1.45f, 1.7f, t)) * e;
            var turn = Quaternion.AngleAxis(-10 * pose + 22 * oops, actToCamera) * Quaternion.AngleAxis(25 * Mathf.Sin(t * 2.4f) * pose, actUp);
            Vector3 travel = actRight * (s * .35f * glide);
            float top = PoseBody(points, count, ground, 1.08f, turn, travel, .92f, e);
            Vector3 at = ground + travel;
            float arms = Mathf.Max(pose, Mathf.Abs(oops) * e);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 root = at + turn * (actUp * (top * .6f) + actRight * (side * s * .6f));
                Vector3 tip = root + turn * (actRight * (side * s * (1.0f + .1f * Mathf.Sin(t * 5 + side)))) + actUp * (s * ((side < 0 ? .25f : -.05f) + .3f * oops * side));
                Limb(root, root + turn * (actRight * (side * s * .35f)), tip - turn * (actRight * (side * s * .25f)), tip, s * .17f * arms, s * .12f * arms, floor, actUp);
                TendrilCount++;
            }
            return count;
        }

        /// <summary>The breath heart's fog patch on the glass (world) and its half size: the overlay is placed there.</summary>
        public Vector3 TrickAnchor { get; private set; }
        public float TrickSpan { get; private set; }

        // Lv 8 · Selfie: the player turned or zoomed the camera; COghe strikes a pose for it, head on one side and a tendril up
        // in a V beside its face; click, flash; it holds the pose a moment.
        private int Selfie(Vector3[] points, int count, Vector3 ground, Vector3 floor, float t, float e)
        {
            float s = BodyHalfWidth(points, count, ground);
            float pose = Smooth(.05f, .4f, t) * (1 - Smooth(2.1f, 2.5f, t)) * e;
            float snap = t > .85f ? Mathf.Sin(Mathf.Clamp01((t - .85f) / .25f) * Mathf.PI) : 0;
            float top = PoseBody(points, count, ground, 1.06f + .04f * snap, Quaternion.AngleAxis(12 * pose, actToCamera) * Quaternion.AngleAxis(-6 * pose, actRight), actUp * (s * .04f * pose), 1, e);
            Vector3 root = ground + actUp * (top * .55f) + actRight * (s * .6f);
            Vector3 wrist = root + actRight * (s * .3f) + actUp * (s * (.55f + .06f * snap)) + actToCamera * (s * .25f);
            Limb(root, root + actRight * (s * .25f), wrist - actUp * (s * .22f), wrist, s * .19f * pose, s * .14f * pose, floor, actUp, false);
            // the V: two little fingers spread apart
            for (int f = -1; f <= 1; f += 2)
            {
                Vector3 tip = wrist + (actUp * .85f + actRight * (.12f + .32f * f)) * (s * .58f * pose);
                Limb(wrist, Vector3.Lerp(wrist, tip, .35f), Vector3.Lerp(wrist, tip, .7f), tip, s * .14f * pose, s * .1f * pose, floor, actUp);
                TendrilCount++;
            }
            TendrilCount++;
            return count;
        }

        // Lv 9 · Juggle (Home): it pinches three droplets off its top and juggles them in a cascade over its head, two tendril
        // hands throwing and catching, eyes on the balls; then catches them back in, one by one.
        private int Juggle(Vector3[] points, int count, Vector3 ground, Vector3 floor, float t, float e)
        {
            float s = BodyHalfWidth(points, count, ground);
            float hands = Smooth(.2f, .6f, t) * (1 - Smooth(3.9f, 4.4f, t)) * e;
            float top = PoseBody(points, count, ground, 1 - .04f * hands, Quaternion.identity, Vector3.zero, 1, e);
            Vector3 home = ground + actUp * (top * .85f), over = ground + actUp * (top + s * .25f) + actToCamera * (s * .3f);
            float w = s * .85f, h = s * 1.1f;
            Vector3 left = over - actRight * w, right = over + actRight * w;
            const float period = .9f;
            float catchL = 0, catchR = 0;
            for (int i = 0; i < 3; i++)
            {
                float born = .3f + i * .22f, back = 3.6f + i * .2f;
                if (t < born || t > back + .4f) continue;
                float phase = Mathf.Max(0, t - .7f) / period + i / 3f, f = phase - Mathf.Floor(phase);
                bool toRight = f < .5f; float u = (toRight ? f : f - .5f) * 2;
                Vector3 from = toRight ? left : right, to = toRight ? right : left;
                Vector3 at = Vector3.Lerp(from, to, u) + actUp * (h * 4 * u * (1 - u));
                if (toRight) catchR = Mathf.Max(catchR, Smooth(.85f, 1, u)); else catchL = Mathf.Max(catchL, Smooth(.85f, 1, u));
                at = Vector3.Lerp(home, at, Smooth(born, born + .4f, t));       // pinched off the top
                float gone = Smooth(back, back + .4f, t);
                at = Vector3.Lerp(at, home, gone);                                // and taken back in
                if (i == 0) TrickFocus = at;
                // a lone skin source this far from the body is too faint to draw: the droplet is a little closed capsule
                float r = s * .13f * (1 - .5f * gone) * e;
                for (int end = -1; end <= 1; end += 2)
                {
                    Vector3 axis = actUp * (end * r * .8f);
                    Limb(at, at + axis * .33f, at + axis * .66f, at + axis, r, r, floor, actUp);
                }
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 shoulder = ground + actUp * (top * .55f) + actRight * (side * s * .7f);
                Vector3 hand = (side < 0 ? left : right) - actUp * (s * (.25f + .12f * (side < 0 ? catchL : catchR)));
                Limb(shoulder, shoulder + actRight * (side * s * .25f), hand - actUp * (s * .25f), hand, s * .17f * hands, s * .12f * hands, floor, actUp);
                TendrilCount++;
            }
            return count;
        }

        // Lv 9 · Mimic: beside a crate it turns into a little box, beside a handle into a lever; it glances at the real thing,
        // becomes it with a wiggle, then flows back.
        private int Mimic(COghePersonality p, Vector3[] points, int count, Vector3 ground, Vector3 floor, float t, float e)
        {
            float s = BodyHalfWidth(points, count, ground);
            float morph = Smooth(.55f, .95f, t) * (1 - Smooth(2.4f, 2.9f, t)) * e, wiggle = morph * Mathf.Sin(t * 9);
            TrickFocus = p.MimicTarget;
            if (p.MimicBox)
            {
                // each source moved into a box of its own size (axes: right, up, toward the camera), out toward its faces so
                // the skin is a block with rounded edges; a little hop as it becomes one
                float hx = s * .78f, hy = s * .72f, hop = Mathf.Sin(Mathf.Clamp01((t - .7f) / .35f) * Mathf.PI);
                Vector3 c = ground + actUp * (hy + s * .12f * hop);
                var turn = Quaternion.AngleAxis(7 * wiggle, actUp);
                float far = 1e-5f;
                for (int i = 0; i < count; i++) far = Mathf.Max(far, Vector3.Distance(transform.TransformPoint(points[i]), c));
                for (int i = 0; i < count; i++)
                {
                    Vector3 w = transform.TransformPoint(points[i]), d = w - c;
                    float x = Vector3.Dot(d, actRight), y = Vector3.Dot(d, actUp), z = Vector3.Dot(d, actToCamera);
                    float m = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z));
                    if (m < 1e-5f) continue;
                    float k = Mathf.Lerp(.55f, 1f, d.magnitude / far) * .82f / m;
                    Vector3 boxed = c + turn * (actRight * (x * k * hx) + actUp * (y * k * hy) + actToCamera * (z * k * hx));
                    points[i] = transform.InverseTransformPoint(Vector3.Lerp(w, boxed, morph));
                }
                return count;
            }
            // a handle: a low base, a post and a grip bar across its top
            float top = PoseBody(points, count, ground, 1 - .35f * morph, Quaternion.AngleAxis(8 * wiggle, actToCamera), Vector3.zero, 1, e);
            Vector3 post = ground + actUp * (top * .8f), head = post + actUp * (s * .75f * morph) + actRight * (s * .06f * wiggle);
            Limb(post, post + actUp * (s * .25f * morph), head - actUp * (s * .2f * morph), head, s * .13f * morph, s * .11f * morph, floor, actUp, false);
            Vector3 bar = (actRight * .9f + actToCamera * .2f).normalized * (s * .4f * morph);
            Limb(head - bar, head - bar * .4f, head + bar * .4f, head + bar, s * .11f * morph, s * .11f * morph, floor, actUp);
            TendrilCount += 2;
            return count;
        }

        // Lv 10 · Rock–paper–scissors (Home): it splits into two minis that shake their fists three times ("oẳn, tù, tì!"),
        // then show their hands; the winner bounces, the loser droops; then they flow back together.
        private int RockPaperScissors(COghePersonality p, Vector3[] points, float[] supports, float[] weights, int count, Vector3 ground, Vector3 floor, float t, float e)
        {
            float s = BodyHalfWidth(points, count, ground);
            float split = Smooth(.2f, .7f, t) * (1 - Smooth(3.8f, 4.5f, t)) * e;
            float Bump(float at) => Mathf.Clamp01(1 - Mathf.Abs(t - at) / .14f);
            float shake = Mathf.Max(Bump(1.0f), Mathf.Max(Bump(1.4f), Bump(1.8f))) * (t < 2.0f ? 1 : 0), shown = Smooth(2.05f, 2.2f, t);
            // hands (0 rock, 1 paper, 2 scissors) for the left and the right mini, and who wins
            int left = p.TrickVariant == 0 ? 2 : p.TrickVariant == 1 ? 1 : 2, right = p.TrickVariant == 0 ? 0 : p.TrickVariant == 1 ? 0 : 1;
            bool leftWins = (left == 0 && right == 2) || (left == 1 && right == 0) || (left == 2 && right == 1);
            float after = Smooth(2.3f, 2.6f, t) * (1 - Smooth(3.5f, 3.8f, t));
            Vector3 cl = Vector3.zero, cr = Vector3.zero; int nl = 0, nr = 0;
            for (int i = 0; i < count; i++) { Vector3 w = transform.TransformPoint(points[i]); if (Vector3.Dot(w - ground, actRight) < 0) { cl += w; nl++; } else { cr += w; nr++; } }
            cl /= Mathf.Max(1, nl); cr /= Mathf.Max(1, nr);
            float apart = s * .62f * split;
            for (int i = 0; i < count; i++)
            {
                Vector3 w = transform.TransformPoint(points[i]); bool isLeft = Vector3.Dot(w - ground, actRight) < 0;
                Vector3 own = isLeft ? cl : cr; int side = isLeft ? -1 : 1; bool wins = isLeft == leftWins;
                float bounce = wins ? Mathf.Abs(Mathf.Sin((t - 2.3f) * 9)) * after : 0, droop = wins ? 0 : after;
                Vector3 d = (w - own) * (1 - .12f * split);
                d = Vector3.ProjectOnPlane(d, actUp) * (1 + .15f * droop) + actUp * (Vector3.Dot(d, actUp) * (1 - .3f * droop));
                Vector3 moved = own + d + actRight * (side * apart) + actUp * (s * (.12f * shake + .25f * bounce));
                points[i] = transform.InverseTransformPoint(Vector3.Lerp(w, moved, split));
            }
            int n = count;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 c = ground + actRight * (side * (apart + s * .3f * split));
                Vector3 root = c + actUp * (s * .6f) - actRight * (side * s * .2f), wrist = root - actRight * (side * s * .45f) + actUp * (s * (.35f + .25f * shake)) + actToCamera * (s * .3f);
                float r = s * .17f * split;
                Limb(root, root + actUp * (s * .15f), wrist - actUp * (s * .1f), wrist, r, r * .85f, floor, actUp, false); TendrilCount++;
                int hand = side < 0 ? left : right; Vector3 toward = -actRight * side;
                if (shown < .5f || hand == 0)
                { if (n < MaxSources) { points[n] = transform.InverseTransformPoint(wrist); supports[n] = s * .23f * split; weights[n++] = split > .05f ? 1.4f : 0; } }
                else
                {
                    int fingers = hand == 1 ? 4 : 2;
                    for (int f = 0; f < fingers; f++)
                    {
                        float spread = fingers == 2 ? (f == 0 ? -.35f : .35f) : -.6f + f * .4f;
                        Vector3 dir = (toward * .7f + actUp * .7f + actToCamera * .1f).normalized;
                        Vector3 tip = wrist + (Quaternion.AngleAxis(spread * 60, actToCamera) * dir) * (s * .5f * shown);
                        Limb(wrist, Vector3.Lerp(wrist, tip, .35f), Vector3.Lerp(wrist, tip, .7f), tip, s * .11f * split, s * .08f * split, floor, actUp); TendrilCount++;
                    }
                }
            }
            return n;
        }

        // Lv 10 · Breath heart (Home, a finger held on COghe for 2 s): it rises up to the glass, breathes a patch of fog onto it
        // and draws a heart in the fog with the tip of a tendril, then sinks back, pleased. The fog itself is on the screen
        // (COgheTrickOverlay), placed at TrickAnchor.
        private int BreathHeart(Vector3[] points, int count, Vector3 ground, Vector3 floor, float t, float e)
        {
            float s = BodyHalfWidth(points, count, ground);
            float rise = Smooth(.1f, .8f, t) * (1 - Smooth(4.6f, 5.5f, t)) * e, breath = Smooth(.8f, 1.05f, t) * (1 - Smooth(1.35f, 1.7f, t));
            float top = PoseBody(points, count, ground, 1 + .1f * rise + .08f * breath, Quaternion.AngleAxis(-8 * breath, actRight),
                actToCamera * (s * .3f * rise) + actUp * (s * .25f * rise), 1, e);
            Vector3 centre = ground + actUp * (top * .95f + s * .45f) + actToCamera * (s * 1.1f);
            TrickAnchor = centre; TrickSpan = s * .55f;
            float drawn = Mathf.Clamp01((t - 1.7f) / 2.3f), arm = Smooth(1.5f, 1.8f, t) * (1 - Smooth(4.1f, 4.5f, t)) * e;
            float a = drawn * Mathf.PI * 2, sn = Mathf.Sin(a);
            var heart = new Vector2(16 * sn * sn * sn, 13 * Mathf.Cos(a) - 5 * Mathf.Cos(2 * a) - 2 * Mathf.Cos(3 * a) - Mathf.Cos(4 * a)) / 17f;
            Vector3 tip = centre + actRight * (heart.x * s * .5f) + actUp * (heart.y * s * .5f + s * .05f);
            TrickFocus = tip;
            Vector3 root = ground + actUp * (top * .6f) + actRight * (s * .45f) + actToCamera * (s * .5f);
            Limb(root, root + actUp * (s * .3f) + actToCamera * (s * .2f), tip - actToCamera * (s * .25f), tip, s * .15f * arm, s * .08f * arm, floor, actUp);
            TendrilCount++;
            return count;
        }

        /// <summary>The face tricks: a small monster head (lower than the Home monster, so it fits a level) that smiles and
        /// winks with a flick of the tongue, or blows a raspberry with the tongue out and waggling.</summary>
        private static MonsterPose TrickFacePose(COgheTrick trick, float t, float length)
        {
            var m = new MonsterPose();
            m.Morph = Ease(0, .32f, t) * (1 - Ease(length - .45f, length, t));
            m.Rise = .58f * m.Morph; m.HeadScale = 1.05f;
            m.Face = Ease(.22f, .55f, t) * (1 - Ease(length - .6f, length - .25f, t));
            m.Writhe = .015f * m.Rise; m.Phase = t * 2f;
            if (trick == COgheTrick.Peekaboo)
            {
                // melted to a puddle, eyes peek out and look around; then "boo!": it shoots up as a grinning fanged head
                float pop = Ease(1.35f, 1.6f, t) * (1 - Ease(length - .8f, length - .2f, t));
                m.Rise = Mathf.Lerp(.06f, .95f, pop) * m.Morph; m.Gather = .6f * Ease(0, .4f, t) * (1 - pop);
                m.HeadScale = Mathf.Lerp(.8f, 1.22f, pop);
                m.Face = Ease(.55f, .85f, t) * (1 - Ease(length - .6f, length - .25f, t));
                float peek = (1 - pop) * m.Face; m.Squint = .35f * peek; m.HeadYaw = 18 * Mathf.Sin(t * 3.5f) * peek;
                m.Mouth = pop; m.Fangs = pop; m.Jaw = .1f + .6f * pop; m.Tongue = .35f * pop * Ease(1.6f, 2.0f, t);
                m.Zoom = .1f * pop; m.HeadPitch = -6 * pop;
                return m;
            }
            if (trick == COgheTrick.Wink)
            {
                m.Mouth = .7f * m.Face; m.Fangs = .45f * m.Face;
                float flick = Ease(.95f, 1.12f, t) * (1 - Ease(1.32f, 1.6f, t));
                m.Jaw = .08f + .25f * flick; m.Tongue = .5f * flick;
                m.Wink = Ease(.68f, .8f, t) * (1 - Ease(1.35f, 1.5f, t));
                m.HeadRoll = 12 * m.Wink; m.Yaw = 12 * m.Face; m.HeadPitch = -4 * m.Face;
            }
            else
            {
                m.Mouth = m.Face; m.Fangs = .3f * m.Face; m.Squint = .35f * m.Face;
                float open = Ease(.55f, .8f, t) * (1 - Ease(length - .9f, length - .55f, t));
                m.Jaw = .1f + .4f * open; m.Tongue = .95f * open;
                m.Waggle = Ease(.82f, 1f, t) * (1 - Ease(2.0f, 2.2f, t));
                m.HeadYaw = 9 * Mathf.Sin(t * 17) * m.Waggle; m.HeadPitch = -5 * open;
            }
            return m;
        }
    }
}
