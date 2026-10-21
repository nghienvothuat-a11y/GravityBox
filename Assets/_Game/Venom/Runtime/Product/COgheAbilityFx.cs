using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Sparse card accents, projected from the creature and its real contact surface.
    /// One pooled UI mesh, at most twelve motes; no lights, colliders, physics writes or runtime textures.</summary>
    public sealed class COgheAbilityFx : MaskableGraphic
    {
        private struct Mote
        {
            public Transform Frame;
            public Vector3 Point, Normal, Along;
            public float Age, Life;
            public int Kind, Glyph;
        }
        private readonly Mote[] motes = new Mote[12];
        private COgheProductUI ui;
        private float untilBurst = 2, emission, remaining;
        private int cursor, nextKind = -1, kind, serial, lastChoice = int.MinValue;
        private Vector3 previousCentre;
        private float simulationAt=-1;
        private bool wasLive;
        private COgheWaterTrail water;
        public int WaterSegments => water != null ? water.Segments : 0;
        public int LiveCount { get; private set; }
        public int Bursts { get; private set; }
        public bool TrailActive => remaining > 0 && kind == 1;

        public static COgheAbilityFx Create(COgheProductUI owner)
        {
            var go = new GameObject("Ability accents", typeof(RectTransform));
            var r = go.GetComponent<RectTransform>(); r.SetParent(owner.SafeRoot, false);
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            var fx = go.AddComponent<COgheAbilityFx>(); fx.ui = owner; fx.raycastTarget = false;
            return fx;
        }
        private bool Live => ui != null && ui.Page == COgheProductPage.Game &&
            ui.Popup == COgheProductPopup.None && ui.Game.Owner.CanControl && !ui.Game.Home;

        private void Update()
        {
            if (!Live)
            {
                if (LiveCount > 0) { System.Array.Clear(motes, 0, motes.Length); LiveCount = 0; SetVerticesDirty(); }
                water?.Clear(); remaining = 0; wasLive = false; return;
            }
            var game = ui.Game;
            if(game.Matter.SimulationTime<simulationAt-.05f)
            {water?.Clear();System.Array.Clear(motes,0,motes.Length);LiveCount=0;remaining=0;wasLive=false;SetVerticesDirty();}
            simulationAt=game.Matter.SimulationTime;
            int actor = game.Motion.Selected;
            Vector3 centre = game.Motion.Centre(actor);
            float dt = Mathf.Min(Time.deltaTime, .05f);
            if (!wasLive) { previousCentre = centre; wasLive = true; }
            if (lastChoice != COgheShop.Current.LastAbility)
            {
                lastChoice = COgheShop.Current.LastAbility;
                nextKind = Mathf.Clamp(lastChoice, 0, 2); untilBurst = 2; remaining = 0;
            }
            Vector3 delta = centre - previousCentre; previousCentre = centre;
            bool crawling = delta.magnitude > dt * .012f && delta.magnitude < .06f &&
                game.Motion.Intent(actor).sqrMagnitude > .00002f;
            if (COgheAbilities.Level(COgheAbility.Agility)>0 && crawling && !game.InTube && !game.Attached &&
                game.Motion.Support(actor,out var support,out var foot,out var footNormal) && support!=null)
            {
                if(water==null){var w=new GameObject("Speedy water · presentation only");w.transform.SetParent(game.Root,false);water=w.AddComponent<COgheWaterTrail>();water.Initialize();}
                water.Contact(support.transform,foot,footNormal);
            }
            else water?.Break();
            water?.Step(dt);
            untilBurst -= dt;
            if (remaining <= 0 && untilBurst <= 0)
            {
                if (nextKind < 0) nextKind = Mathf.Clamp(COgheShop.Current.LastAbility, 0, 2);
                for (int i = 0; i < 3; i++)
                {
                    int candidate = (nextKind + i) % 3;
                    if (COgheAbilities.Level((COgheAbility)candidate) == 0) continue;
                    // Speedy waits for walking; resting or falling must not paint a fake glass trail.
                    if (candidate == 1 && !crawling) continue;
                    if(candidate==2 && (game.Personality==null || !game.Personality.TryRead()))continue;
                    kind = candidate; nextKind = (candidate + 1) % 3;
                    remaining = candidate==2 ? 4.2f : 1.8f; emission = 0; untilBurst = 14 + (Bursts % 3) * 3; Bursts++; break;
                }
            }
            if (remaining > 0)
            {
                remaining -= dt; emission -= dt;
                if(kind==2 && game.Personality.Act!=COgheAct.Read)remaining=0;
                if (remaining>0 && emission <= 0 && (kind != 1 || crawling))
                {
                    emission = kind == 1 ? .18f : .48f;
                    Emit(actor, centre, delta);
                }
            }
            bool dirty = LiveCount > 0;
            LiveCount = 0;
            for (int i = 0; i < motes.Length; i++)
            {
                if (motes[i].Life <= 0) continue;
                motes[i].Age += dt;
                if (motes[i].Age >= motes[i].Life) motes[i].Life = 0; else LiveCount++;
            }
            if (dirty || LiveCount > 0) SetVerticesDirty();
        }
        private void Emit(int actor, Vector3 centre, Vector3 direction)
        {
            var game = ui.Game;
            Transform frame = game.Root; Vector3 point = centre, normal = Vector3.up, along = direction.normalized;
            if (kind == 1)
            {
                if (!game.Motion.Support(actor, out var shape, out point, out normal) || shape == null) return;
                frame = shape.transform;
                point += normal * .0015f;
                along = Vector3.ProjectOnPlane(direction, normal).normalized;
                if (along.sqrMagnitude < .5f) return;
            }
            motes[cursor] = new Mote { Frame = frame, Point = frame.InverseTransformPoint(point),
                Normal = frame.InverseTransformDirection(normal), Along = frame.InverseTransformDirection(along),
                Age = 0, Life = kind == 1 ? 2.1f : 2.8f, Kind = kind, Glyph = serial++ % 6 };
            cursor = (cursor + 1) % motes.Length;
        }
        private bool Project(Vector3 world, out Vector2 point)
        {
            var screen = ui.Game.Owner.View.WorldToScreenPoint(world);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screen, null, out point);
            return screen.z > 0;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); if (!Live) return;
            var cam = ui.Game.Owner.View;
            foreach (var m in motes)
            {
                if (m.Life <= 0 || m.Frame == null) continue;
                float t = m.Age / m.Life;
                float alpha = Mathf.SmoothStep(0, 1, t / .12f) * Mathf.SmoothStep(1, 0, Mathf.InverseLerp(.45f, 1, t));
                Vector3 world = m.Frame.TransformPoint(m.Point);
                if (m.Kind == 1)
                {
                    Vector3 n = m.Frame.TransformDirection(m.Normal), a = m.Frame.TransformDirection(m.Along);
                    Vector3 side = Vector3.Cross(n, a).normalized;
                    if (!Project(world, out var p) || !Project(world + side * .009f, out var sx) ||
                        !Project(world + a * .018f, out var sy)) continue;
                    // Soft pearl-mint footprint flush to glass; a tiny glint lifts from the same contact.
                    // The water itself is a depth-tested world ribbon; only its tiny rising stars use this UI mesh.
                    if (Project(world + n * (.006f + m.Age * .013f), out var glow))
                        Star(vh, glow, 3.1f + Mathf.Sin(m.Age*5+m.Glyph)*.7f, new Color(.88f, .98f, .88f, alpha * .8f));
                    continue;
                }
                world += cam.transform.up * (.035f + m.Age * .024f) +
                    cam.transform.right * (Mathf.Sin(m.Glyph * 2.1f + m.Age * 2) * .012f);
                if (!Project(world, out var centre)) continue;
                if (m.Kind == 0) Heart(vh, centre, 6.5f, new Color(.80f, .43f, .42f, alpha * .8f));
                else MathGlyph(vh, centre, m.Glyph, new Color(.39f, .61f, .65f, alpha * .85f));
            }
        }
        private static void Heart(VertexHelper vh, Vector2 p, float size, Color c)
        {
            int start = vh.currentVertCount; vh.AddVert(p, c, Vector2.zero);
            for (int i = 0; i <= 32; i++)
            {
                float a = i * Mathf.PI * 2 / 32, sn = Mathf.Sin(a);
                vh.AddVert(p + new Vector2(sn * sn * sn, (13 * Mathf.Cos(a) - 5 * Mathf.Cos(2*a) -
                    2 * Mathf.Cos(3*a) - Mathf.Cos(4*a)) / 16) * size, c, Vector2.zero);
                if (i > 0) vh.AddTriangle(start, start + i, start + i + 1);
            }
        }
        private static void Ellipse(VertexHelper vh, Vector2 p, Vector2 a, Vector2 b, Color c)
        {
            int start = vh.currentVertCount; vh.AddVert(p, c, Vector2.zero); c.a = 0;
            for (int i = 0; i <= 16; i++)
            {
                float t = i * Mathf.PI * 2 / 16;
                vh.AddVert(p + a * Mathf.Cos(t) + b * Mathf.Sin(t), c, Vector2.zero);
                if (i > 0) vh.AddTriangle(start, start + i, start + i + 1);
            }
        }
        protected override void OnDestroy(){if(water!=null)Destroy(water.gameObject);base.OnDestroy();}
        private static void Star(VertexHelper vh, Vector2 p, float r, Color c)
        {
            int n=vh.currentVertCount;vh.AddVert(p,c,Vector2.zero);
            for(int i=0;i<=8;i++){float a=i*Mathf.PI/4;float radius=(i%2==0?r:r*.22f);vh.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,c,Vector2.zero);if(i>0)vh.AddTriangle(n,n+i,n+i+1);}
        }
        private static void Spark(VertexHelper vh, Vector2 p, float r, Color c)
        { Line(vh, p - Vector2.up*r, p + Vector2.up*r, 1, c); Line(vh, p - Vector2.right*r, p + Vector2.right*r, 1, c); }
        private static void MathGlyph(VertexHelper vh, Vector2 p, int glyph, Color c)
        {
            const float w=1.4f;
            if(glyph==0){Line(vh,p+Vector2.left*5,p+Vector2.right*5,w,c);Line(vh,p+Vector2.down*5,p+Vector2.up*5,w,c);return;}
            if(glyph==1){Line(vh,p+new Vector2(-5,2),p+new Vector2(5,2),w,c);Line(vh,p+new Vector2(-5,-2),p+new Vector2(5,-2),w,c);return;}
            if(glyph==2){Line(vh,p+new Vector2(-4,-4),p+new Vector2(4,4),w,c);Line(vh,p+new Vector2(-4,4),p+new Vector2(4,-4),w,c);return;}
            if(glyph==3){Line(vh,p+new Vector2(-5,4),p+new Vector2(5,4),w,c);Line(vh,p+new Vector2(-2,4),p+new Vector2(-3,-5),w,c);Line(vh,p+new Vector2(3,4),p+new Vector2(3,-5),w,c);return;}
            if(glyph==5)
            {
                Curve(vh,p,new Vector2(-4,3),new Vector2(-4,8),new Vector2(5,8),new Vector2(4,2),w,c);
                Curve(vh,p,new Vector2(4,2),new Vector2(3,0),new Vector2(-3,-3),new Vector2(-4,-5),w,c);
                Line(vh,p+new Vector2(-4,-5),p+new Vector2(5,-5),w,c);return;
            }
            Vector2 previous=Vector2.zero;
            for(int i=0;i<=32;i++)
            {
                float a=i/32f*Mathf.PI*2;Vector2 q=new Vector2(6*Mathf.Cos(a),3*Mathf.Sin(2*a));
                if(i>0)Line(vh,p+previous,p+q,w,c);previous=q;
            }
        }
        private static void Curve(VertexHelper vh,Vector2 origin,Vector2 a,Vector2 b,Vector2 c,Vector2 d,float width,Color color)
        {
            Vector2 previous=a;
            for(int i=1;i<=12;i++){float t=i/12f,u=1-t;var q=u*u*u*a+3*u*u*t*b+3*u*t*t*c+t*t*t*d;Line(vh,origin+previous,origin+q,width,color);previous=q;}
        }
        private static void Line(VertexHelper vh, Vector2 a, Vector2 b, float width, Color c)
        {
            Vector2 d = b-a, side = new Vector2(-d.y,d.x).normalized * width * .5f; int n = vh.currentVertCount;
            vh.AddVert(a-side,c,Vector2.zero); vh.AddVert(a+side,c,Vector2.zero);
            vh.AddVert(b+side,c,Vector2.zero); vh.AddVert(b-side,c,Vector2.zero);
            vh.AddTriangle(n,n+1,n+2); vh.AddTriangle(n,n+2,n+3);
        }
    }
}
