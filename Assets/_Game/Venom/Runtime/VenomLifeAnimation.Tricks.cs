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
