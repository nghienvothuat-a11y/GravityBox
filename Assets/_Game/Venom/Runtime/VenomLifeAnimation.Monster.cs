using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Where the monster's face is this frame (world space): read by <see cref="COgheMonsterFace"/> and the menu camera.</summary>
    public struct COgheMonsterRig
    {
        /// <summary>0: no monster; 1: fully formed with its face. Body: how far the liquid has risen into it.</summary>
        public float Amount, Body;
        /// <summary>Head centre and axes (Forward points out of the face); Size: metres per shape unit.</summary>
        public Vector3 Head, Right, Up, Forward; public float Size;
        /// <summary>The jaw's frame: the head's middle carried with the jaw, and its axes; <see cref="Jaw"/>: how open, 0..1.</summary>
        public Vector3 JawHead, JawUp, JawForward; public float Jaw;
        /// <summary>Squint and blink (0 open .. 1 shut), how far the tongue is out (0..1), seconds into the act.</summary>
        public float Squint, Blink, Tongue, Time;
        /// <summary>The menu camera: the middle and height of the whole monster, how far to lean in on the face, a shake.</summary>
        public Vector3 Focus; public float Height, Zoom, Shake;
    }

    /// <summary>
    /// The monster (Mrk 02/10: the main menu's attention-grabber, ~7 s, soft and liquid, no scare, no voice). COghe flows up
    /// into a Venom-like liquid creature, turns to show its profile, opens its mouth while its whole body sways like a
    /// liquid, slides out a long writhing tongue, takes it back and looks about, then turns to the camera, blinks, tilts its
    /// head, and melts back. Like every act it is the skin only: the particles stay where they are. Its eyes, teeth, mouth
    /// and tongue are <see cref="COgheMonsterFace"/>, set on the skin through the field kept here.
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
            public float Gather, Column, Morph, Face, Yaw, HeadYaw, HeadPitch, HeadRoll, Lean, Writhe, Phase, Jaw, Tongue, Blink, HeadScale, Zoom;
        }

        // A key of the timeline: eases from 0 at a to 1 at b.
        private static float Ease(float a, float b, float t) => Smooth(a, b, t);

        private static MonsterPose MonsterPoseAt(float t, float length)
        {
            var m = new MonsterPose();
            m.Gather = Ease(0, .45f, t) * (1 - Ease(.6f, 1.1f, t));                   // the liquid gathers itself
            m.Column = Ease(.35f, .85f, t) * (1 - Ease(.85f, 1.5f, t));               // flows up as a column
            m.Morph = Ease(.45f, 1.55f, t) * (1 - Ease(length - .95f, length - .1f, t));
            m.Face = Ease(1.0f, 1.5f, t) * (1 - Ease(length - 1.15f, length - .65f, t));
            // it turns to show its profile, then back to the camera
            m.Yaw = 78 * Ease(1.3f, 2.3f, t) * (1 - Ease(4.5f, 5.4f, t));
            // the whole body sways like a liquid, most while its mouth opens
            m.Writhe = .08f + .36f * Ease(1.6f, 2.5f, t) * (1 - Ease(4.0f, 4.9f, t));
            m.Phase = t * 4.2f;
            float open = Ease(2.1f, 3.0f, t) * (1 - Ease(3.9f, 4.5f, t));
            m.Jaw = (.08f + .78f * open + .08f * open * Mathf.Sin(t * 5.3f)) * m.Face;
            m.Tongue = Ease(2.55f, 3.45f, t) * (1 - Ease(3.85f, 4.5f, t));
            float look = Ease(3.95f, 4.35f, t) * (1 - Ease(4.45f, 4.85f, t));     // and it looks: up and further away
            m.HeadPitch = -8 * open - 14 * look;
            m.HeadYaw = 22 * look;
            float tilt = Ease(5.0f, 5.6f, t) * (1 - Ease(6.15f, 6.6f, t));       // to the camera, a curious tilt of the head
            m.HeadRoll = 22 * tilt + 3 * tilt * Mathf.Sin((t - 5.6f) * 3.1f);
            m.Lean = 7 * open;
            m.Blink = Mathf.Max(0, 1 - Mathf.Abs(t - 5.3f) / .1f);                  // a blink as it turns to look at you
            m.HeadScale = 1.3f + .06f * tilt;
            m.Zoom = .32f * Ease(1.8f, 2.8f, t) * (1 - Ease(6.2f, 6.9f, t));
            return m;
        }

        // Rest positions (body radii; x right, y up, z toward the camera) of the parts that turn: the waist the upper body
        // bends from, the neck the head turns about, the jaw hinge, the head's middle.
        private static readonly Vector3 Waist = new Vector3(0, .95f, 0), Neck = new Vector3(0, 2.42f, -.12f), Hinge = new Vector3(0, 2.88f, -.12f), HeadMid = new Vector3(0, 3.04f, .16f);
        private const float Spine = 2.3f;   // waist to head, along which the turn and the sway grow
        private Quaternion monsterHead; private float monsterScale = 1, monsterYaw, monsterLean, monsterWrithe, monsterPhase;

        /// <summary>How far up the spine (0 at the waist, 1 at the head): the body bends and turns more the higher it is.</summary>
        private static float Height01(Vector3 q) => Mathf.Clamp01((q.y - Waist.y) / Spine);
        private Quaternion BodyTurn(float h) => Quaternion.AngleAxis(monsterYaw * Mathf.Lerp(.45f, 1, h), Vector3.up) * Quaternion.AngleAxis(monsterLean * h, Vector3.right);
        // the liquid sway: an S travelling up the body, side to side and a little front to back
        private Vector3 Sway(float h) => new Vector3(Mathf.Sin(monsterPhase - h * 2.4f), 0, .45f * Mathf.Sin(monsterPhase * .7f - h * 1.9f)) * (monsterWrithe * h);
        private Vector3 UpperBody(Vector3 q) { float h = Height01(q); return Waist + BodyTurn(h) * (q - Waist) + Sway(h); }
        private Vector3 HeadPart(Vector3 q) => UpperBody(Neck) + monsterHead * ((q - Neck) * monsterScale);
        // the jaw drops down and a little forward, tipping open
        private static Quaternion JawTurn(float jaw) => Quaternion.AngleAxis(jaw * MonsterJawDegrees, Vector3.right);
        private Vector3 JawPart(Vector3 q, float jaw) => HeadPart(Hinge + JawTurn(jaw) * (q - Hinge) + new Vector3(0, -MonsterJawDrop, .1f) * jaw);

        private void BuildMonster(in MonsterPose m, float t)
        {
            shapeCount = limbCount = 0;
            monsterYaw = m.Yaw; monsterLean = m.Lean; monsterWrithe = m.Writhe; monsterPhase = m.Phase; monsterScale = m.HeadScale;
            // the head rides the top of the sway, rolling with it, and turns, nods and tilts on its own
            float swayRoll = -m.Writhe * 55 * Mathf.Cos(m.Phase - 2.4f);
            monsterHead = BodyTurn(1) * Quaternion.AngleAxis(m.HeadYaw, Vector3.up) * Quaternion.AngleAxis(m.HeadPitch, Vector3.right)
                          * Quaternion.AngleAxis(m.HeadRoll + swayRoll, Vector3.forward);
            // a smooth pool of liquid it rises out of, a waist, chest and shoulders
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
            float grown = monsterScale;
            Part(HeadPart(new Vector3(0, 3.2f, -.05f)), 1.02f * grown);
            Part(HeadPart(new Vector3(0, 3.42f, -.36f)), .66f * grown);
            Part(HeadPart(new Vector3(0, 2.96f, .56f)), .62f * grown);   // the upper jaw, one arc across the face
            for (int side = -1; side <= 1; side += 2) Part(HeadPart(new Vector3(side * .34f, 3.0f, .42f)), .6f * grown);
            Part(JawPart(new Vector3(0, 2.62f, .46f), m.Jaw), .5f * grown);   // a wide, flat chin
            for (int side = -1; side <= 1; side += 2) Part(JawPart(new Vector3(side * .42f, 2.65f, .32f), m.Jaw), .47f * grown);
            for (int side = -1; side <= 1; side += 2)
            { var c = new Vector3(side * .62f, 2.76f, .16f); Part(Vector3.Lerp(HeadPart(c), JawPart(c, m.Jaw), .5f), .5f * grown); }   // cheeks

            // arms: liquid, hanging loose and swinging with the body; soft claws
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 shoulder = UpperBody(new Vector3(side * 1.0f, 2.2f, -.05f));
                float swing = Mathf.Sin(m.Phase * .8f + side * 1.3f);
                Vector3 elbow = UpperBody(new Vector3(side * 1.34f, 1.58f, .12f + .1f * swing));
                Vector3 wrist = UpperBody(new Vector3(side * (1.36f + .08f * swing), 1.0f, .3f + .22f * swing));
                Part(Vector3.Lerp(shoulder, elbow, .5f), .56f); Part(elbow, .5f); Part(Vector3.Lerp(elbow, wrist, .5f), .46f); Part(wrist, .46f);
                Vector3 along = (wrist - elbow).normalized, palm = new Vector3(-side * .7f, 0, .7f).normalized, across = Vector3.Cross(along, palm).normalized;
                for (int c = -1; c <= 1; c++)
                {
                    float wave = .12f * Mathf.Sin(m.Phase * 1.3f + c + side);
                    Vector3 dir = (along + across * (c * .45f)).normalized, root = wrist + along * .16f + across * (c * .1f);
                    LimbQ(root, root + dir * .22f, root + dir * .42f + palm * (.1f + wave), root + dir * .52f + palm * (.22f + wave), .14f, .05f);
                }
            }
            // tendrils flowing from the back
            for (int k = 0; k < 3; k++)
            {
                float side = k == 0 ? -1 : k == 1 ? 1 : 0;
                Vector3 root = UpperBody(new Vector3(side * .48f, 2.45f + (k == 2 ? .3f : 0), -.48f));
                Vector3 dir = BodyTurn(1) * new Vector3(side * .85f, 1, -.55f).normalized;
                float a = Mathf.Sin(t * (1.6f + .3f * k) + k * 2.1f), b = Mathf.Sin(t * (2.3f + .4f * k) + k);
                Vector3 bend = new Vector3(a * .45f, .1f * b, b * .35f);
                LimbQ(root, root + dir * .35f, root + dir * .7f + bend * .5f, root + dir * 1.0f + bend, .17f, .035f);
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
                // the liquid gathers (a slow swell), then flows up as a column before it takes shape
                if (m.Gather > 0) world = ground + (world - ground) * (1 + .1f * m.Gather * (.6f + .4f * Mathf.Sin(t * 5 + i * 1.3f)));
                if (m.Column > 0)
                {
                    Vector3 d = world - ground; float h = Vector3.Dot(d, actUp);
                    world = ground + (d - actUp * h) * (1 - .3f * m.Column) + actUp * (h * (1 + 1.5f * m.Column));
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
            // the face's frame, for COgheMonsterFace and the camera
            Vector3 Dir(Vector3 q) => (ToWorld(q, ground, s) - ToWorld(Vector3.zero, ground, s)) / s;
            Quaternion head = monsterHead;
            Monster.Amount = m.Face * p.Fade; Monster.Body = e; Monster.Time = t;
            Monster.Head = ToWorld(HeadPart(HeadMid), ground, s);
            Monster.Right = Dir(head * Vector3.right).normalized; Monster.Up = Dir(head * Vector3.up).normalized; Monster.Forward = Dir(head * Vector3.forward).normalized;
            Monster.Size = s * monsterScale; Monster.Jaw = m.Jaw;
            Monster.JawHead = ToWorld(JawPart(HeadMid, m.Jaw), ground, s);
            Monster.JawUp = Dir(head * (JawTurn(m.Jaw) * Vector3.up)).normalized; Monster.JawForward = Dir(head * (JawTurn(m.Jaw) * Vector3.forward)).normalized;
            Monster.Squint = 0; Monster.Blink = m.Blink; Monster.Tongue = m.Tongue * m.Face;
            Monster.Focus = ToWorld(new Vector3(0, 1.75f, 0), ground, s); Monster.Height = 3.8f * s * Mathf.Max(e, .4f);
            Monster.Zoom = m.Zoom * p.Fade; Monster.Shake = 0;
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
