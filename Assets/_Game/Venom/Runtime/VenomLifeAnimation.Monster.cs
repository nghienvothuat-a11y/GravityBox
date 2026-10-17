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
        public float Squint, Blink, Mouth, Fangs, Tongue, Time;
        /// <summary>One eye shut (a wink, 0..1) and a fast side-to-side waggle of the tongue (a raspberry, 0..1).</summary>
        public float Wink, Waggle;
        /// <summary>The menu camera: the middle and height of the whole monster, how far to lean in on the face, a shake.</summary>
        public Vector3 Focus; public float Height, Zoom, Shake;
    }

    /// <summary>
    /// A temporary liquid head rising out of COghe's puddle (15/10/2026). No torso, shoulders or hands.
    /// The mound rises before the face forms, then smiles, bares its fangs and slowly unfurls its tongue.
    /// The same act is used at Home and in the menu. All deformation is skin-only; physics is untouched.
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
        private Matrix4x4 fieldLocal, fieldWorld;

        private struct MonsterPose
        {
            public float Gather, Rise, Morph, Face, Mouth, Fangs, Yaw, HeadYaw, HeadPitch, HeadRoll, Lean, Writhe, Phase, Jaw, Tongue, Blink, HeadScale, Zoom;
            public float Squint, Wink, Waggle;   // Emotion tricks
        }

        private static float Ease(float a, float b, float t) => Smooth(a, b, t);

        private static MonsterPose MonsterPoseAt(float t, float length)
        {
            var m = new MonsterPose();
            m.Gather = Ease(0, .65f, t) * (1 - Ease(.8f, 1.7f, t));
            m.Morph = Ease(.15f, 1.6f, t) * (1 - Ease(length - 1.6f, length, t));
            m.Rise = Ease(.55f, 3.0f, t) * (1 - Ease(length - 1.8f, length - .15f, t));
            m.Face = Ease(2.1f, 3.05f, t) * (1 - Ease(length - 1.9f, length - 1.1f, t));
            m.Mouth = Ease(2.9f, 3.65f, t) * (1 - Ease(length - 2.0f, length - 1.2f, t));
            m.Fangs = Ease(3.45f, 4.35f, t) * (1 - Ease(length - 2.15f, length - 1.4f, t));
            // Stay in three-quarter view: both eyes and the growing jaw remain readable at Home.
            m.Yaw = 28 * Ease(3.1f, 4.5f, t) * (1 - Ease(6.6f, 7.7f, t));
            m.Writhe = (.025f + .035f * Ease(2.5f, 4, t)) * m.Rise;
            m.Phase = t * 2.0f;
            float open = Ease(3.65f, 4.85f, t) * (1 - Ease(6.75f, 7.55f, t));
            m.Jaw = (.08f + .8f * open) * m.Mouth;
            m.Tongue = Ease(4.65f, 5.8f, t) * (1 - Ease(6.35f, 7.15f, t));
            m.HeadPitch = -6 * open;
            m.HeadYaw = -8 * Ease(6.5f, 7.2f, t) * (1 - Ease(7.4f, 8.2f, t));
            float tilt = Ease(7.0f, 7.55f, t) * (1 - Ease(8.05f, 8.6f, t));
            m.HeadRoll = 13 * tilt;
            m.Lean = 3 * open;
            m.Blink = Ease(7.45f, 7.55f, t) * (1 - Ease(7.6f, 7.74f, t));
            m.HeadScale = 1.16f + .018f * Mathf.Sin(t * 2.3f) * m.Rise;
            m.Zoom = .22f * Ease(1.8f, 3.3f, t) * (1 - Ease(7.8f, 9.3f, t));
            return m;
        }

        // A broad puddle and a short flowing connection, not a human spine/waist/neck.
        private static readonly Vector3 Neck = new Vector3(0, 1.22f, -.12f),
            Hinge = new Vector3(0, 1.84f, -.12f), HeadMid = new Vector3(0, 2.0f, .16f);
        private Quaternion monsterHead;
        private float monsterScale = 1;
        private Vector3 monsterCrown;
        private Vector3 HeadPart(Vector3 q) => monsterCrown + monsterHead * ((q - Neck) * monsterScale);
        private static Quaternion JawTurn(float jaw) => Quaternion.AngleAxis(jaw * MonsterJawDegrees, Vector3.right);
        private Vector3 JawPart(Vector3 q, float jaw) => HeadPart(Hinge + JawTurn(jaw) * (q - Hinge) + new Vector3(0, -MonsterJawDrop, .1f) * jaw);

        private void BuildMonster(in MonsterPose m, float t)
        {
            shapeCount = limbCount = 0;
            monsterScale = m.HeadScale * Mathf.Lerp(.24f, 1, m.Rise);
            monsterHead = Quaternion.Euler(m.HeadPitch + m.Lean, m.Yaw + m.HeadYaw, m.HeadRoll);
            monsterCrown = new Vector3(Mathf.Sin(m.Phase) * m.Writhe,
                Mathf.Lerp(.12f, .91f, m.Rise), Neck.z * m.Rise);
            // A soft asymmetric skirt stays in contact with the ground throughout the transformation.
            Src(0, .18f, 0, .96f);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2 / 8, pulse = .025f * Mathf.Sin(t * 2.4f - i * .75f);
                float radius = .69f + pulse;
                Src(Mathf.Cos(a) * radius, .105f + pulse * .2f, Mathf.Sin(a) * radius * .85f, .66f);
            }
            // Overlapping lobes carry a travelling swell from the puddle to the head, with no shoulder outline.
            for (int i = 1; i <= 3; i++)
            {
                float u = i / 3f;
                Vector3 q = Vector3.Lerp(new Vector3(0, .22f, 0), monsterCrown, u);
                q.x += .025f * Mathf.Sin(t * 2.2f - u * 3) * m.Rise;
                Part(q, Mathf.Lerp(.85f, .78f, u) * Mathf.Lerp(.6f, 1, m.Rise));
            }
            float r = monsterScale;
            Part(HeadPart(new Vector3(0, 2.16f, -.05f)), .99f * r);
            Part(HeadPart(new Vector3(0, 2.36f, -.28f)), .70f * r);
            Part(HeadPart(new Vector3(0, 1.92f, .52f)), .61f * r);
            for (int side = -1; side <= 1; side += 2)
                Part(HeadPart(new Vector3(side * .34f, 1.96f, .38f)), .60f * r);
            Part(JawPart(new Vector3(0, 1.58f, .44f), m.Jaw), .44f * r);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(JawPart(new Vector3(side * .40f, 1.61f, .30f), m.Jaw), .43f * r);
                var cheek = new Vector3(side * .56f, 1.72f, .13f);
                Part(Vector3.Lerp(HeadPart(cheek), JawPart(cheek, m.Jaw), .5f), .46f * r);
            }
        }

        private void Part(Vector3 q, float support) => Src(q.x, q.y, q.z, support);

        private int MonsterAct(COghePersonality p, Vector3[] points, float[] supports, float[] weights, int count, Vector3 centre, Vector3 ground, Vector3 up, Vector3 floor, float t)
            => MonsterBody(p, MonsterPoseAt(t, p.Length), points, supports, weights, count, centre, ground, up, floor, t);

        /// <summary>The monster (or a small one, for a trick) posed as <paramref name="m"/>.</summary>
        private int MonsterBody(COghePersonality p, in MonsterPose m, Vector3[] points, float[] supports, float[] weights, int count, Vector3 centre, Vector3 ground, Vector3 up, Vector3 floor, float t)
        {
            float s = BodyRadius;
            BuildMonster(m, t);
            float e = m.Morph * p.Fade;
            for (int i = 0; i < count; i++)
            {
                Vector3 world = transform.TransformPoint(points[i]);
                // the liquid gathers (a slow swell), then flows up as a column before it takes shape
                if (m.Gather > 0) world = ground + (world - ground) * (1 + .1f * m.Gather * (.6f + .4f * Mathf.Sin(t * 5 + i * 1.3f)));
                int k = slotOf[i]; bool absorbed = k < 0; if (absorbed) k = -k - 1;
                points[i] = transform.InverseTransformPoint(Vector3.Lerp(world, ToWorld(shapePoint[k], ground, s), e));
                // swollen while it stretches and reshapes, so the liquid never tears apart on the way
                supports[i] = Mathf.Lerp(supports[i], shapeSupport[k] * s, e) * (1 + .12f * Mathf.Sin(Mathf.PI * e));
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
            Monster.Squint = m.Squint - .06f * m.Jaw; Monster.Blink = m.Blink; Monster.Wink = m.Wink * p.Fade; Monster.Waggle = m.Waggle * p.Fade;
            Monster.Mouth = m.Mouth * p.Fade; Monster.Fangs = m.Fangs * p.Fade; Monster.Tongue = m.Tongue * m.Face * p.Fade;
            Monster.Focus = ToWorld(new Vector3(0, 1.25f * m.Rise, 0), ground, s); Monster.Height = 3.1f * s * Mathf.Max(e, .4f);
            Monster.Zoom = m.Zoom * p.Fade; Monster.Shake = 0;
            // keep the field: the face finds the skin in it
            fieldCount = 0; fieldLocal = transform.worldToLocalMatrix; fieldWorld = transform.localToWorldMatrix;
            for (int i = 0; i < n; i++)
            {
                if (weights[i] < .00001f) continue;
                fieldPoint[fieldCount] = points[i]; fieldSupport[fieldCount] = supports[i]; fieldWeight[fieldCount++] = weights[i];
            }
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
            Vector3 o = fieldLocal.MultiplyPoint3x4(origin), d = fieldLocal.MultiplyVector(direction).normalized;
            float step = reach / 24, inside = 0, outside = -1;
            if (Field(o) < threshold)
            {
                // starting outside: come back in along the ray first (the head may be smaller than expected)
                for (int k = 1; k <= 24; k++) { if (Field(o - d * (step * k)) >= threshold) { o -= d * (step * k); break; } }
            }
            for (int k = 1; k <= 24; k++) { if (Field(o + d * (step * k)) < threshold) { outside = step * k; inside = step * (k - 1); break; } }
            if (outside < 0) return fieldWorld.MultiplyPoint3x4(o + d * reach);
            for (int k = 0; k < 6; k++) { float mid = (inside + outside) * .5f; if (Field(o + d * mid) >= threshold) inside = mid; else outside = mid; }
            return fieldWorld.MultiplyPoint3x4(o + d * ((inside + outside) * .5f));
        }

        /// <summary>The skin's outward normal near a world point.</summary>
        public Vector3 SkinNormal(Vector3 world)
        {
            Vector3 p = fieldLocal.MultiplyPoint3x4(world), gradient = Vector3.zero;
            for (int i = 0; i < fieldCount; i++)
            {
                float h2 = fieldSupport[i] * fieldSupport[i];
                Vector3 d = p - fieldPoint[i]; float q = 1 - d.sqrMagnitude / h2;
                if (q > 0) gradient += d * (6 * q * q * fieldWeight[i] / h2);
            }
            return gradient.sqrMagnitude < 1e-12f ? Vector3.up : fieldWorld.MultiplyVector(gradient).normalized;
        }
        /// <summary>Is a world point inside the skin?</summary>
        public bool InsideSkin(Vector3 world) => Field(fieldLocal.MultiplyPoint3x4(world)) >= organism.Profile.SkinThreshold;
    }
}
