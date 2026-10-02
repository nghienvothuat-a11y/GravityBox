using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Where the monster's face is this frame (world space): read by <see cref="COgheMonsterFace"/> and the menu camera.</summary>
    public struct COgheMonsterRig
    {
        /// <summary>0: no monster; 1: fully formed with its face. Body: how far the liquid has risen into it.</summary>
        public float Amount, Body;
        /// <summary>Head centre and axes (Forward points out of the face, toward the camera); Size: metres per shape unit.</summary>
        public Vector3 Head, Right, Up, Forward; public float Size;
        /// <summary>The jaw's frame: the head's middle carried with the jaw, and its axes (it drops and tips as it opens);
        /// <see cref="Jaw"/>: how open, 0..1.</summary>
        public Vector3 JawHead, JawUp, JawForward; public float Jaw;
        public float Squint, Tongue, Sway, Time;
        /// <summary>The menu camera: the middle and height of the whole monster, how far to lean in on the face, a shake.</summary>
        public Vector3 Focus; public float Height, Zoom, Shake;
    }

    /// <summary>
    /// The monster (Mrk 02/10: the main menu's attention-grabber, ~6.6 s). The body rumbles and spikes, rises into a hulking
    /// liquid monster with clawed arms and whipping tendrils, opens its jaw, lunges at the viewer with a roar, then leans back,
    /// hugs its belly and cackles, and melts back into COghe. Like every act it is the skin only: the particles stay where
    /// they are. Its eyes, teeth, mouth and tongue are <see cref="COgheMonsterFace"/>, set on the skin through the field kept here.
    /// </summary>
    public sealed partial class VenomLifeAnimation
    {
        public const float MonsterJawDegrees = 16, MonsterJawDrop = .46f;
        public COgheMonsterRig Monster;
        private COgheMonsterFace monsterFace;
        // this frame's field (surface-local), kept so the face can sit exactly on the skin
        private readonly Vector3[] fieldPoint = new Vector3[MaxSources];
        private readonly float[] fieldSupport = new float[MaxSources], fieldWeight = new float[MaxSources];
        private int fieldCount;

        private struct MonsterPose
        {
            public float Rumble, Column, Reach, HeadRoll, Morph, Face, Lean, HeadPitch, HeadScale, Jaw, Squint, Tongue, Raise, Belly, Bob, Shiver, Tendrils, Zoom, Shake;
        }

        // A key of the timeline: eases from 0 at a to 1 at b.
        private static float Ease(float a, float b, float t) => Smooth(a, b, t);

        private static MonsterPose MonsterPoseAt(float t, float length)
        {
            var m = new MonsterPose();
            m.Rumble = Ease(0, .25f, t) * (1 - Ease(.85f, 1.3f, t));
            m.Column = Ease(.75f, 1.15f, t) * (1 - Ease(1.15f, 1.7f, t));   // it surges up as a column before it takes shape
            m.Morph = Ease(.85f, 1.65f, t) * (1 - Ease(length - .85f, length - .1f, t));
            m.Face = Ease(1.3f, 1.65f, t) * (1 - Ease(length - 1.05f, length - .6f, t));
            float windUp = Ease(1.85f, 2.35f, t) * (1 - Ease(2.35f, 2.55f, t));
            float roar = Ease(2.35f, 2.6f, t) * (1 - Ease(3.25f, 3.75f, t));
            float laugh = Ease(3.8f, 4.15f, t) * (1 - Ease(5.45f, 5.95f, t));
            // laughing: shoulders bounce on each "khặc", about 4.6 a second, slowing at the end
            float beat = Mathf.Max(0, Mathf.Sin((t - 3.85f) * Mathf.PI * 2 * Mathf.Lerp(4.6f, 3.4f, Ease(4.6f, 5.5f, t))));
            float chatter = Mathf.Pow(beat, .7f) * laugh;
            m.Jaw = .08f + .47f * windUp + .92f * roar * (1 + .04f * Mathf.Sin(t * 31)) + .5f * chatter + .1f * laugh;
            m.Jaw = Mathf.Min(1, m.Jaw) * m.Face;
            m.Lean = -10 * windUp + 18 * roar - 8 * laugh + 6 * chatter;
            m.HeadPitch = -14 * windUp - 22 * roar - 4 * laugh - 13 * chatter;   // a nod back on every "khặc"
            m.HeadRoll = 11 * laugh * Mathf.Sin((t - 3.8f) * 2.4f);              // and a smug sway of the head
            m.Reach = .7f * roar;   // the neck stretches: the head comes at the viewer   // face to the viewer in the roar: undo the lean, tip up
            m.HeadScale = 1.3f + .5f * roar;   // the head swells as it lunges: it comes at the viewer
            m.Squint = .5f * laugh - .2f * roar;
            m.Tongue = Mathf.Max(.55f * windUp + roar, .35f * laugh);
            m.Raise = Ease(2.25f, 2.6f, t) * (1 - Ease(3.3f, 3.85f, t));
            m.Belly = Ease(3.75f, 4.15f, t) * (1 - Ease(5.5f, 6.05f, t));
            m.Bob = .3f * chatter;
            m.Shiver = laugh * .05f;
            m.Tendrils = Mathf.Max(.55f, roar);
            m.Zoom = Ease(1.9f, 2.55f, t) * (1 - Ease(3.3f, 4.0f, t)) + .5f * Ease(3.6f, 4.1f, t) * (1 - Ease(5.5f, 6.2f, t));
            m.Shake = .0045f * roar * Mathf.Exp(-Mathf.Max(0, t - 2.45f) * 2.2f) + .0012f * chatter;
            return m;
        }

        // Rest positions (body radii; x right, y up, z toward the camera) of the parts that turn: the waist the upper body
        // leans about, the neck the head turns about, the jaw hinge, the head's middle.
        private static readonly Vector3 Waist = new Vector3(0, .95f, 0), Neck = new Vector3(0, 2.42f, -.12f), Hinge = new Vector3(0, 2.88f, -.12f), HeadMid = new Vector3(0, 3.04f, .16f);
        private Quaternion monsterLean, monsterHead; private float monsterScale = 1, monsterBob, monsterShiver, monsterReach;

        private Vector3 UpperBody(Vector3 q) => Waist + monsterLean * (q - Waist) + new Vector3(monsterShiver, monsterBob, 0);
        private Vector3 HeadPart(Vector3 q) => UpperBody(Neck) + monsterLean * (monsterHead * ((q - Neck) * monsterScale) + Vector3.forward * monsterReach);
        // the jaw drops down and a little forward, tipping open: a gaping mouth seen from the front
        private static Quaternion JawTurn(float jaw) => Quaternion.AngleAxis(jaw * MonsterJawDegrees, Vector3.right);
        private Vector3 JawPart(Vector3 q, float jaw) => HeadPart(Hinge + JawTurn(jaw) * (q - Hinge) + new Vector3(0, -MonsterJawDrop, .1f) * jaw);

        private void BuildMonster(in MonsterPose m, float t)
        {
            shapeCount = limbCount = 0;
            monsterLean = Quaternion.AngleAxis(m.Lean, Vector3.right);
            monsterHead = Quaternion.AngleAxis(m.HeadPitch, Vector3.right) * Quaternion.AngleAxis(m.HeadRoll, Vector3.forward);
            monsterScale = m.HeadScale; monsterBob = m.Bob; monsterShiver = Mathf.Sin(t * 47) * m.Shiver; monsterReach = m.Reach;
            // a smooth pool of liquid it rises out of, a waist, a hulking chest and shoulders
            Src(0, .3f, 0, 1.0f);
            for (int i = 0; i < 7; i++) { float a = i * Mathf.PI * 2 / 7; Src(Mathf.Cos(a) * .78f, .14f, Mathf.Sin(a) * .78f, .72f); }
            Src(Waist.x, Waist.y, Waist.z, .92f);
            Part(UpperBody(new Vector3(0, 1.42f, .14f)), .96f);
            Part(UpperBody(new Vector3(0, 1.92f, .02f)), .96f);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(UpperBody(new Vector3(side * .46f, 1.97f, .18f)), .64f);
                Part(UpperBody(new Vector3(side * 1.0f, 2.2f, -.05f)), .66f);
            }
            Part(UpperBody(Neck), .72f);
            // a big head, the skull drawn back; the jaw drops about its hinge
            float grown = monsterScale;   // a swelling head keeps its blobs touching
            Part(HeadPart(new Vector3(0, 3.2f, -.05f)), 1.02f * grown);
            Part(HeadPart(new Vector3(0, 3.42f, -.36f)), .66f * grown);
            // the upper jaw: one arc across the face, so it stays whole when the jaw drops
            Part(HeadPart(new Vector3(0, 2.96f, .56f)), .62f * grown);
            for (int side = -1; side <= 1; side += 2) Part(HeadPart(new Vector3(side * .34f, 3.0f, .42f)), .6f * grown);
            // a wide, flat chin
            Part(JawPart(new Vector3(0, 2.62f, .46f), m.Jaw), .5f * grown);
            for (int side = -1; side <= 1; side += 2) Part(JawPart(new Vector3(side * .42f, 2.65f, .32f), m.Jaw), .47f * grown);
            // cheeks: goo joining the jaw to the head at the corners of the mouth
            for (int side = -1; side <= 1; side += 2)
            { var c = new Vector3(side * .62f, 2.76f, .16f); Part(Vector3.Lerp(HeadPart(c), JawPart(c, m.Jaw), .5f), .5f * grown); }

            // arms: hanging, then thrown up and out for the roar, then hugging the belly for the laugh
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 shoulder = UpperBody(new Vector3(side * 1.0f, 2.2f, -.05f));
                float hang = Mathf.Max(0, 1 - m.Raise - m.Belly);
                Vector3 elbow = new Vector3(side * 1.36f, 1.55f, .12f) * hang + new Vector3(side * 1.55f, 2.3f, .75f) * m.Raise + new Vector3(side * 1.26f, 1.62f, .58f) * m.Belly;
                Vector3 wrist = new Vector3(side * 1.42f, .95f, .3f) * hang + new Vector3(side * 1.55f, 2.75f, 1.55f) * m.Raise + new Vector3(side * .44f, 1.38f, 1.02f) * m.Belly;
                elbow = UpperBody(elbow); wrist = UpperBody(wrist);
                // the arm is liquid too: a chain of blobs from the shoulder to the hand
                Part(Vector3.Lerp(shoulder, elbow, .5f), .56f); Part(elbow, .5f); Part(Vector3.Lerp(elbow, wrist, .5f), .46f); Part(wrist, .46f);
                // three claws: down and in at rest, toward the viewer in the roar, round the belly in the laugh
                Vector3 along = (wrist - elbow).normalized;
                Vector3 palm = (new Vector3(-side * .7f, 0, .7f) * hang + new Vector3(0, -.25f, 1) * m.Raise + new Vector3(0, 0, -1) * m.Belly).normalized;
                Vector3 across = Vector3.Cross(along, palm).normalized;
                float curl = .5f + .5f * m.Raise + .4f * m.Belly;
                for (int c = -1; c <= 1; c++)
                {
                    Vector3 dir = (along + across * (c * .5f)).normalized, root = wrist + along * .16f + across * (c * .1f);
                    Vector3 tip = root + dir * .62f + palm * (.3f * curl);
                    LimbQ(root, root + dir * .26f, root + dir * .5f + palm * (.12f * curl), tip, .15f, .045f);
                }
            }
            // tendrils lashing from the back
            for (int k = 0; k < 3; k++)
            {
                float side = k == 0 ? -1 : k == 1 ? 1 : 0;
                Vector3 root = UpperBody(new Vector3(side * .48f, 2.45f + (k == 2 ? .3f : 0), -.48f));
                Vector3 dir = new Vector3(side * .85f, 1, -.55f).normalized;
                float sway = Mathf.Sin(t * (3.1f + k) + k * 2.1f), whip = Mathf.Sin(t * (5.3f + k * .7f) + k);
                Vector3 bend = new Vector3(sway * .5f, 0, whip * .3f);
                float reach = 1.15f * m.Tendrils;
                LimbQ(root, root + dir * (.45f * reach), root + dir * (.9f * reach) + bend * .6f, root + dir * (1.35f * reach) + bend + Vector3.up * (.2f * whip), .18f, .035f);
            }
        }

        private void Part(Vector3 q, float support) => Src(q.x, q.y, q.z, support);

        private int MonsterAct(COghePersonality p, Vector3[] points, float[] supports, float[] weights, int count, Vector3 centre, Vector3 ground, Vector3 up, Vector3 floor, float t)
        {
            float s = BodyRadius;
            var m = MonsterPoseAt(t, p.Length);
            BuildMonster(m, t);
            float e = m.Morph * p.Fade;
            for (int i = 0; i < count; i++)
            {
                Vector3 world = transform.TransformPoint(points[i]);
                // the rumble: the liquid shivers and swells before it rises
                if (m.Column > 0)
                {
                    Vector3 d = world - ground; float h = Vector3.Dot(d, actUp);
                    world = ground + (d - actUp * h) * (1 - .3f * m.Column) + actUp * (h * (1 + 1.5f * m.Column));
                }
                if (m.Rumble > 0)
                {
                    float swell = 1 + .14f * m.Rumble * (.5f + .5f * Mathf.Sin(t * 9 + i * 1.3f));
                    world = ground + (world - ground) * swell + (actRight * Mathf.Sin(t * 173 + i * 1.7f) + actToCamera * Mathf.Cos(t * 151 + i * 2.3f) + actUp * Mathf.Sin(t * 131 + i)) * (.0024f * m.Rumble);
                }
                int k = slotOf[i]; bool absorbed = k < 0; if (absorbed) k = -k - 1;
                points[i] = transform.InverseTransformPoint(Vector3.Lerp(world, ToWorld(shapePoint[k], ground, s), e));
                // swollen while it stretches and reshapes, so the liquid never tears apart on the way
                supports[i] = Mathf.Lerp(supports[i], shapeSupport[k] * s, e) * (1 + .5f * m.Column + .3f * Mathf.Sin(Mathf.PI * e));
                weights[i] = Mathf.Lerp(1, absorbed ? 0 : shapeWeight[k], e);
            }
            int n = count;
            for (int k = 0; k < shapeCount && n < MaxSources; k++)
            {
                if (slotUsed[k]) continue;
                points[n] = transform.InverseTransformPoint(ToWorld(shapePoint[k], ground, s));
                supports[n] = shapeSupport[k] * s; weights[n++] = shapeWeight[k] * e;
            }
            float grow = Smooth(.45f, 1, e);
            for (int l = 0; l < limbCount; l++)
            {
                Vector3 root = limbPoint[l * 4];
                Vector3 Q(int j) => ToWorld(root + (limbPoint[l * 4 + j] - root) * grow, ground, s);
                Limb(Q(0), Q(1), Q(2), Q(3), limbRadius[l * 2] * s * grow, limbRadius[l * 2 + 1] * s * grow, floor, up, limbCapped[l]);
                TendrilCount++;
            }
            // spikes jab out of the shivering body before it rises
            for (int k = 0; k < 5 && m.Rumble > .05f; k++)
            {
                float start = .3f + k * .11f, jab = Mathf.Sin(Mathf.Clamp01((t - start) / .28f) * Mathf.PI) * m.Rumble;
                if (jab <= .02f) continue;
                float angle = k * 2.4f + .6f;
                Vector3 dir = new Vector3(Mathf.Cos(angle) * .75f, 1, Mathf.Sin(angle) * .55f).normalized;
                Vector3 root = new Vector3(0, .55f, 0) + dir * .4f, tip = root + dir * (1.1f * jab);
                Limb(ToWorld(root, ground, s), ToWorld(Vector3.Lerp(root, tip, .33f), ground, s), ToWorld(Vector3.Lerp(root, tip, .66f), ground, s), ToWorld(tip, ground, s), .2f * s * jab, .02f * s, floor, up);
                TendrilCount++;
            }
            // the face's frame, for COgheMonsterFace and the camera
            Vector3 Dir(Vector3 q) => (ToWorld(q, ground, s) - ToWorld(Vector3.zero, ground, s)) / s;
            Quaternion head = monsterLean * monsterHead;
            Monster.Amount = m.Face * p.Fade; Monster.Body = e; Monster.Time = t;
            Monster.Head = ToWorld(HeadPart(HeadMid), ground, s);
            Monster.Right = Dir(head * Vector3.right).normalized; Monster.Up = Dir(head * Vector3.up).normalized; Monster.Forward = Dir(head * Vector3.forward).normalized;
            Monster.Size = s * monsterScale; Monster.Jaw = m.Jaw;
            Monster.JawHead = ToWorld(JawPart(HeadMid, m.Jaw), ground, s);
            Monster.JawUp = Dir(head * (JawTurn(m.Jaw) * Vector3.up)).normalized; Monster.JawForward = Dir(head * (JawTurn(m.Jaw) * Vector3.forward)).normalized;
            Monster.Squint = m.Squint; Monster.Tongue = m.Tongue * m.Face; Monster.Sway = Mathf.Sin(t * 7.3f) * .7f + Mathf.Sin(t * 12.1f) * .3f;
            Monster.Focus = ToWorld(new Vector3(0, 1.75f, 0), ground, s); Monster.Height = 3.8f * s * Mathf.Max(e, .4f);
            Monster.Zoom = m.Zoom * p.Fade; Monster.Shake = m.Shake * p.Fade;
            // keep the field: the face finds the skin in it
            fieldCount = n;
            for (int i = 0; i < n; i++) { fieldPoint[i] = points[i]; fieldSupport[i] = supports[i]; fieldWeight[i] = weights[i]; }
            if (monsterFace == null) { monsterFace = GetComponent<COgheMonsterFace>(); if (monsterFace == null) { monsterFace = gameObject.AddComponent<COgheMonsterFace>(); monsterFace.Initialize(this, organism.Profile.Skin); } }
            DanceAmount = Mathf.Max(DanceAmount, e); HeadAmount = Mathf.Max(HeadAmount, e);
            return n;
        }

        /// <summary>The skin field at a surface-local point (≥ the skin threshold inside).</summary>
        private float Field(Vector3 local)
        {
            float f = 0;
            for (int i = 0; i < fieldCount; i++)
            {
                float h = fieldSupport[i], d2 = (local - fieldPoint[i]).sqrMagnitude, q = 1 - d2 / (h * h);
                if (q > 0) f += q * q * q * fieldWeight[i];
            }
            return f;
        }

        /// <summary>Where a ray from inside the monster (world <paramref name="origin"/>, <paramref name="direction"/>) leaves the
        /// skin; <paramref name="reach"/> limits the search. Returns the origin side's last point inside when it never leaves.</summary>
        public Vector3 SkinAlong(Vector3 origin, Vector3 direction, float reach)
        {
            float threshold = organism.Profile.SkinThreshold;
            Vector3 o = transform.InverseTransformPoint(origin), d = transform.InverseTransformDirection(direction).normalized;
            float step = reach / 24, inside = 0, outside = -1;
            if (Field(o) < threshold)
            {
                // starting outside: come back in along the ray first (the head may be smaller than expected)
                for (int k = 1; k <= 24; k++) { if (Field(o - d * (step * k)) >= threshold) { o -= d * (step * k); break; } }
            }
            for (int k = 1; k <= 24; k++) { if (Field(o + d * (step * k)) < threshold) { outside = step * k; inside = step * (k - 1); break; } }
            if (outside < 0) return transform.TransformPoint(o + d * reach);
            for (int k = 0; k < 6; k++) { float mid = (inside + outside) * .5f; if (Field(o + d * mid) >= threshold) inside = mid; else outside = mid; }
            return transform.TransformPoint(o + d * ((inside + outside) * .5f));
        }

        /// <summary>The skin's outward normal near a world point.</summary>
        public Vector3 SkinNormal(Vector3 world)
        {
            Vector3 p = transform.InverseTransformPoint(world); float e = .0015f;
            var g = new Vector3(Field(p + Vector3.right * e) - Field(p - Vector3.right * e), Field(p + Vector3.up * e) - Field(p - Vector3.up * e), Field(p + Vector3.forward * e) - Field(p - Vector3.forward * e));
            return g.sqrMagnitude < 1e-12f ? Vector3.up : -transform.TransformDirection(g).normalized;
        }
        /// <summary>Is a world point inside the skin?</summary>
        public bool InsideSkin(Vector3 world) => Field(transform.InverseTransformPoint(world)) >= organism.Profile.SkinThreshold;
    }
}
