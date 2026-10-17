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

        /// <summary>The face tricks: a small monster head (lower than the Home monster, so it fits a level) that smiles and
        /// winks with a flick of the tongue, or blows a raspberry with the tongue out and waggling.</summary>
        private static MonsterPose TrickFacePose(COgheTrick trick, float t, float length)
        {
            var m = new MonsterPose();
            m.Morph = Ease(0, .32f, t) * (1 - Ease(length - .45f, length, t));
            m.Rise = .58f * m.Morph; m.HeadScale = 1.05f;
            m.Face = Ease(.22f, .55f, t) * (1 - Ease(length - .6f, length - .25f, t));
            m.Writhe = .015f * m.Rise; m.Phase = t * 2f;
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
