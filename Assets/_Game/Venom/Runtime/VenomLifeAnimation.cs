using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>
    /// Interprets physical motion as a flowing, probing creature. The central mass
    /// leads the performance; transient filaments follow actual surface contacts.
    /// Presentation only: never writes particles, colliders, bonds or puzzle state.
    /// </summary>
    public sealed class VenomLifeAnimation : MonoBehaviour
    {
        private const int Arms = 5, Segments = 12, Sides = 7;
        private sealed class Foot
        {
            public Collider Surface;
            public Vector3 LocalPoint, LocalNormal, LocalDirection;
            public float Age, Reach, Hold, Release, WaitUntil, ReleaseAt, Thickness;
            public int PlantId, Particle, Attempts, Seed;
        }
        private sealed class Fragment
        {
            public readonly Foot[] Feet = new Foot[Arms];
            public bool Seen;
            public int Count, Peeks;
            public float IdleTime, NextPeek, PeekStart = -100, PeekLength = 3, Head, Moving, Speed, Flow;
            public Vector3 Up = Vector3.up, Forward = Vector3.forward, Probe = Vector3.forward, Lag, LagRate;
            public Fragment(int key, float time)
            {
                NextPeek = 1.8f + Noise(key)*1.2f;
                for (int i = 0; i < Arms; i++) Feet[i] = new Foot { WaitUntil = time+Noise(key+i*19)*.7f };
            }
        }
        public readonly struct Plant
        {
            public readonly int Fragment, Limb, Id;
            public readonly Collider Surface;
            public readonly Vector3 LocalPoint, Tip;
            public Plant(int fragment, int limb, int id, Collider surface, Vector3 localPoint, Vector3 tip)
            { Fragment = fragment; Limb = limb; Id = id; Surface = surface; LocalPoint = localPoint; Tip = tip; }
        }
        private CohesiveOrganism organism;
        private VenomLevelController level;
        private readonly Fragment[] fragments = new Fragment[CohesiveOrganism.ParticleCount];
        private readonly Vector3[] deformation = new Vector3[CohesiveOrganism.ParticleCount];
        private readonly List<Vector3> vertices = new List<Vector3>(10000), normals = new List<Vector3>(10000);
        private readonly List<int> triangles = new List<int>(18000);
        private readonly List<Plant> plants = new List<Plant>(48);
        private Mesh tendrils;
        private float clock, dt, previousSimulationTime = -1;
        public float HeadAmount { get; private set; }
        public float HeadHeight { get; private set; }
        public int TendrilCount { get; private set; }
        public float CrawlAmount { get; private set; }
        public IReadOnlyList<Plant> PlantedFeet => plants;

        public void Initialize(CohesiveOrganism source, VenomLevelController owner)
        {
            organism = source; level = owner;
            var go = new GameObject("Transient contact filaments", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(transform, false);
            tendrils = new Mesh { name = "Transient contact filaments", indexFormat = IndexFormat.UInt32 };
            tendrils.MarkDynamic(); go.GetComponent<MeshFilter>().sharedMesh = tendrils;
            go.GetComponent<MeshRenderer>().sharedMaterial = source.Profile.Skin;
        }
        public void BeginFrame()
        {
            float time = organism.SimulationTime;
            bool reset = time < previousSimulationTime;
            if (reset) { Array.Clear(fragments,0,fragments.Length); clock = 0; }
            dt = previousSimulationTime < 0 || reset ? 0 : Mathf.Min(.05f,time-previousSimulationTime)*organism.Profile.AnimationSpeed;
            previousSimulationTime = time;
            clock += dt;
            vertices.Clear(); normals.Clear(); triangles.Clear(); plants.Clear();
            HeadAmount = HeadHeight = CrawlAmount = 0; TendrilCount = 0;
            foreach (var fragment in fragments) if (fragment != null) fragment.Seen = false;
        }
        // Up to six extra field sources form a crest, not another simulated body.
        public int Decorate(Vector3[] points, float[] supports, float[] weights, int[] ids, int count)
        {
            int key = ids[0];
            var state = fragments[key] ?? (fragments[key] = new Fragment(key,clock));
            state.Seen = true;
            if (state.Count != count)
            {
                state.Head = state.IdleTime = 0; state.PeekStart = -100;
                state.Lag = state.LagRate = Vector3.zero;
                foreach (var foot in state.Feet) foot.Surface = null;
            }
            state.Count = count;
            Vector3 centre = Vector3.zero, velocity = Vector3.zero, up = Vector3.zero, floorPoint = Vector3.zero;
            int contacts = 0; bool leaving = false;
            Collider support = null;
            Rigidbody box = level.Rotation.GetComponent<Rigidbody>();
            for (int i = 0; i < count; i++)
            {
                centre += transform.TransformPoint(points[i]);
                Rigidbody body = organism.Bodies[ids[i]];
                Vector3 relative = body.linearVelocity;
                leaving |= organism.Escaped[ids[i]];
                if (organism.TryGetSupport(ids[i],out var collider,out var point,out var normal))
                {
                    up += normal; floorPoint += point; contacts++; support = collider;
                    if (collider.attachedRigidbody != null) relative -= collider.attachedRigidbody.GetPointVelocity(body.position);
                }
                else relative -= box.GetPointVelocity(body.position);
                velocity += relative;
            }
            centre /= count; velocity /= count;
            Vector3 exit = level.Outlet.InverseTransformPoint(centre);
            leaving |= level.GateLatched && exit.z > -.06f && new Vector2(exit.x,exit.y).magnitude < .06f;
            bool grounded = contacts > 0 && count >= 3 && !leaving;
            if (grounded)
            {
                up.Normalize(); floorPoint /= contacts;
                state.Up = Vector3.Slerp(state.Up,up,1-Mathf.Exp(-dt*14));
            }
            up = state.Up;
            Vector3 tangentVelocity = Vector3.ProjectOnPlane(velocity,up);
            state.Speed = Mathf.Lerp(state.Speed,tangentVelocity.magnitude,1-Mathf.Exp(-dt*10));
            float moving = grounded ? Smooth(.010f,.065f,state.Speed) : 0;
            state.Moving = Mathf.MoveTowards(state.Moving,moving,dt*(grounded ? 4 : 12));
            // The intended direction follows flow/downhill, never the game camera.
            Vector3 intent = tangentVelocity + Vector3.ProjectOnPlane(Vector3.down,up)*.018f;
            if (grounded && intent.sqrMagnitude > .000025f)
                state.Forward = Vector3.Slerp(state.Forward,intent.normalized,1-Mathf.Exp(-dt*5));
            state.Forward = Vector3.ProjectOnPlane(state.Forward,up).normalized;
            if (state.Forward.sqrMagnitude < .1f) state.Forward = Vector3.ProjectOnPlane(box.transform.forward,up).normalized;
            state.Flow += dt*(.12f + state.Speed*8);
            if (dt > 0)
                state.Lag = Vector3.SmoothDamp(state.Lag,Vector3.ClampMagnitude(-tangentVelocity*.055f,.009f),
                    ref state.LagRate,.19f,Mathf.Infinity,dt);
            state.IdleTime = grounded && state.Speed < .024f ? state.IdleTime+dt : 0;
            if (grounded && count >= 8 && state.IdleTime > state.NextPeek)
            {
                state.PeekStart = clock; state.IdleTime = 0; state.Peeks++;
                state.PeekLength = 2.7f + Noise(key+state.Peeks*31)*1.1f;
                state.NextPeek = 4.5f + Noise(key+state.Peeks*17)*4;
                state.Probe = FindProbeDirection(centre,up,state.Forward,key+state.Peeks*37);
            }
            float peek = (clock-state.PeekStart)/state.PeekLength;
            float envelope = Smooth(0,.28f,peek)*(1-Smooth(.58f,1,peek));
            float targetHead = grounded && state.Speed < .024f && count >= 8 ? envelope : 0;
            state.Head = Mathf.MoveTowards(state.Head,targetHead,dt*3.5f);
            if (!grounded) foreach (var foot in state.Feet) foot.Surface = null;

            Vector3 localCentre = transform.InverseTransformPoint(centre), localUp = transform.InverseTransformDirection(up);
            float top = 0, bottom = 0;
            for (int i = 0; i < count; i++)
            {
                float h = Vector3.Dot(points[i]-localCentre,localUp);
                top = Mathf.Max(top,h); bottom = Mathf.Min(bottom,h);
            }
            if (grounded)
            {
                FlowBody(state,key,points,count,centre,up,top,bottom,floorPoint);
                DrawFeet(state,key,points,ids,count,centre,up,floorPoint,support);
            }
            if (state.Head < .008f) return count;
            // An off-centre, broad fold gathers upward, hesitates, then rolls back.
            // Its asymmetric bend conveys curiosity without a permanent face/neck.
            Vector3 forward = Vector3.ProjectOnPlane(state.Probe,up).normalized;
            forward = Quaternion.AngleAxis((Smooth(.28f,.48f,peek)-Smooth(.57f,.86f,peek))*26,up)*forward;
            Vector3 anchor = centre + up*(top*.45f) + forward*.006f;
            float lift = organism.Profile.CuriousHeadLift*state.Head;
            Vector3 direction = (up+forward*.42f).normalized;
            Vector3 side = Vector3.Cross(up,forward).normalized;
            float clearance = lift+.03f;
            for (int p = -1; p <= 1; p++)
                if (level.RaycastBoundary(anchor+side*(p*.014f),direction,clearance,out var hit)) clearance = hit.distance;
            lift = Mathf.Min(lift,Mathf.Max(0,clearance-.026f));
            float head = Mathf.Min(state.Head,lift/Mathf.Max(.001f,organism.Profile.CuriousHeadLift));
            HeadAmount = Mathf.Max(HeadAmount,head); HeadHeight = Mathf.Max(HeadHeight,lift);
            if (head < .02f) return count;
            for (int j = 0; j < 6; j++)
            {
                float t = j/5f;
                Vector3 source = anchor+up*(lift*t)+forward*(lift*.48f*t*t);
                points[count] = transform.InverseTransformPoint(source);
                supports[count] = Mathf.Lerp(.029f,.018f,t);
                weights[count++] = head*Mathf.Lerp(.8f,.62f,t);
            }
            return count;
        }

        private Vector3 FindProbeDirection(Vector3 centre, Vector3 up, Vector3 forward, int seed)
        {
            Vector3 result = forward; float best = float.NegativeInfinity;
            // Prefer a nearby boundary to inspect, but require room to grow a crest.
            for (int i = 0; i < 7; i++)
            {
                Vector3 direction = Quaternion.AngleAxis(i*51.43f+Noise(seed)*85,up)*forward;
                float distance = level.RaycastBoundary(centre,direction,.16f,out var hit) ? hit.distance : .16f;
                if (distance < .036f) continue;
                float score = .16f-distance+Noise(seed+i*13)*.045f;
                if (score > best) { best = score; result = direction; }
            }
            return result;
        }
        private void FlowBody(Fragment state, int key, Vector3[] points, int count,
            Vector3 centre, Vector3 up, float top, float bottom, Vector3 floor)
        {
            Vector3 forward = state.Head > .1f ? Vector3.ProjectOnPlane(state.Probe,up).normalized : state.Forward;
            Vector3 side = Vector3.Cross(up,forward).normalized;
            Vector3 average = Vector3.zero;
            // A broad lobe travels across the upper tissue. Quieter uneven folds
            // continue at rest; the underside remains supported by physical nodes.
            float travel = Mathf.Repeat(state.Flow+Noise(key),1);
            float front = Mathf.Lerp(-.025f,.027f,travel);
            float wave = Mathf.Sin(travel*Mathf.PI);
            float lateral = (Mathf.PerlinNoise(key+.7f,clock*.27f)-.5f)*.038f;
            float activity = .5f + Mathf.PerlinNoise(key+8.3f,clock*.43f)*.5f;
            for (int i = 0; i < count; i++)
            {
                Vector3 world = transform.TransformPoint(points[i]), offset = world-centre;
                float height = Vector3.Dot(offset,up), x = Vector3.Dot(offset,forward), z = Vector3.Dot(offset,side);
                float upper = Smooth(bottom-.003f,top+.003f,height);
                float lobe = Mathf.Exp(-((x-front)*(x-front)/.0003f+(z-lateral)*(z-lateral)/.00045f))*wave;
                float fold = organism.Profile.IdleBulge*activity*(lobe*2.8f-.7f)*upper;
                float stretch = 1+state.Moving*.28f;
                Vector3 shape = forward*(x*(stretch-1)) + (side*z+up*height)*(1/Mathf.Sqrt(stretch)-1);
                Vector3 shift = state.Lag*upper + up*fold - forward*(lobe*.003f*upper);
                // A crest borrows its shoulder shape from the body rather than
                // looking like a sphere on a separately inflated stalk.
                shift += forward*(state.Head*.006f*upper)-up*(state.Head*.002f*(1-upper));
                deformation[i] = Vector3.ClampMagnitude(shape+shift,.014f);
                average += deformation[i];
            }
            average /= count;
            for (int i = 0; i < count; i++)
            {
                Vector3 world = transform.TransformPoint(points[i]);
                Vector3 delta = deformation[i]-average;
                // Remove common translation before constraining visual
                // displacement against nearby apparatus. Never cross a thin wall.
                float length = delta.magnitude;
                if (length > .00001f && level.RaycastBoundary(world,delta/length,length+.014f,out var hit))
                    delta *= Mathf.Clamp01((hit.distance-.014f)/length);
                Vector3 target = world+delta;
                target += up*Mathf.Max(0,organism.Profile.ParticleRadius-Vector3.Dot(target-floor,up));
                points[i] = transform.InverseTransformPoint(target);
            }
        }

        private void DrawFeet(Fragment state, int key, Vector3[] points, int[] ids, int count,
            Vector3 centre, Vector3 up, Vector3 floor, Collider support)
        {
            CrawlAmount = Mathf.Max(CrawlAmount,state.Moving);
            int limbCount = count < 8 ? 3 : Arms;
            for (int arm = 0; arm < limbCount; arm++)
            {
                Foot foot = state.Feet[arm];
                int seed = foot.Seed;
                if (foot.Surface == null)
                {
                    if (dt <= 0 || clock < foot.WaitUntil || state.Moving < .18f) continue;
                    seed = foot.Seed = key*97+arm*23+foot.Attempts++*71;
                    // Independent, flow-biased sprouts. Slots are a budget, not a
                    // radial skeleton: each new attachment chooses a fresh angle.
                    float angle = (Noise(seed)*2-1)*78;
                    if (Noise(seed+11) > .72f) angle += 180;
                    Vector3 direction = Quaternion.AngleAxis(angle,up)*state.Forward;
                    int rootId = 0; float best = float.NegativeInfinity;
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 offset = transform.TransformPoint(points[i])-centre;
                        float score = Vector3.Dot(offset,direction)-Mathf.Abs(Vector3.Dot(offset,up)+.012f)*.9f;
                        if (score > best) { rootId = i; best = score; }
                    }
                    Vector3 root = transform.TransformPoint(points[rootId])+direction*.003f;
                    Vector3 goal = root+direction*organism.Profile.TendrilReach*Mathf.Lerp(.65f,1.25f,Noise(seed+7));
                    goal -= up*Vector3.Dot(goal-floor,up);
                    foot.WaitUntil = clock+.18f+Noise(seed+3)*.43f;
                    if (!level.RaycastBoundary(goal+up*.025f,-up,.04f,out var hit) || Vector3.Dot(hit.normal,up)<.75f) continue;
                    if (level.SegmentBlocked(root,hit.point+up*.001f)) continue;
                    foot.Surface = hit.collider; foot.LocalPoint = hit.collider.transform.InverseTransformPoint(hit.point);
                    foot.LocalNormal = hit.collider.transform.InverseTransformDirection(hit.normal);
                    foot.LocalDirection = hit.collider.transform.InverseTransformDirection(direction);
                    foot.Particle = ids[rootId]; foot.Age = 0; foot.PlantId++;
                    foot.Reach = .16f+Noise(seed+17)*.18f;
                    foot.Hold = .24f+Noise(seed+29)*.55f;
                    foot.Release = .3f+Noise(seed+43)*.3f;
                    foot.ReleaseAt = foot.Reach+foot.Hold;
                    foot.Thickness = .65f+Noise(seed+59)*.5f;
                }
                int index = Array.IndexOf(ids,foot.Particle,0,count);
                if (index < 0 || !foot.Surface.enabled) { foot.Surface = null; continue; }
                Vector3 normal = foot.Surface.transform.TransformDirection(foot.LocalNormal).normalized;
                Vector3 anchor = foot.Surface.transform.TransformPoint(foot.LocalPoint);
                Vector3 dir = foot.Surface.transform.TransformDirection(foot.LocalDirection).normalized;
                Vector3 origin = transform.TransformPoint(points[index])+dir*.003f;
                float distance = Vector3.Distance(origin,anchor);
                if (distance > .095f || level.SegmentBlocked(origin,anchor+normal*.001f))
                { foot.Surface = null; continue; }
                foot.Age += dt;
                if ((distance > .055f || distance < .009f || state.Moving < .12f) && foot.Age > foot.Reach)
                    foot.ReleaseAt = Mathf.Min(foot.ReleaseAt,foot.Age);
                float reach = Smooth(0,foot.Reach,foot.Age);
                float release = Smooth(foot.ReleaseAt,foot.ReleaseAt+foot.Release,foot.Age);
                if (release >= .999f)
                { foot.Surface = null; foot.WaitUntil = clock+.13f+Noise(seed+67)*.55f; continue; }
                Vector3 end = Vector3.Lerp(origin,anchor+normal*.0007f,reach*(1-release));
                end += normal*(Mathf.Sin(release*Mathf.PI)*.005f);
                if (reach >= .999f && release == 0)
                    plants.Add(new Plant(key,arm,foot.PlantId,foot.Surface,foot.LocalPoint,end));
                float tension = Mathf.InverseLerp(.025f,.075f,distance);
                Vector3 bend = Vector3.Cross(normal,dir)*(.0035f*(Noise(seed+2)*2-1)*(1-tension));
                Vector3 p1 = origin+dir*.008f+normal*.0015f;
                Vector3 p2 = Vector3.Lerp(origin,end,.72f)+bend+normal*(.001f+release*.006f);
                Tube(origin,p1,p2,end,normal,anchor,foot.Thickness*Mathf.Lerp(1,.48f,tension)*(1-release*.5f));
                TendrilCount++;
            }
        }
        private void Tube(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 up, Vector3 floor, float thickness)
        {
            int first = vertices.Count;
            for (int ring = 0; ring <= Segments; ring++)
            {
                float t = ring/(float)Segments, u = 1-t;
                Vector3 p = u*u*u*p0 + 3*u*u*t*p1 + 3*u*t*t*p2 + t*t*t*p3;
                Vector3 tangent = (3*u*u*(p1-p0)+6*u*t*(p2-p1)+3*t*t*(p3-p2)).normalized;
                Vector3 right = Vector3.Cross(tangent,up).normalized;
                if (right.sqrMagnitude < .1f) right = Vector3.Cross(tangent,Vector3.right).normalized;
                Vector3 binormal = Vector3.Cross(right,tangent).normalized;
                float radius = Mathf.Lerp(.006f,.00055f,Mathf.Pow(t,.55f))*thickness;
                p += up*Mathf.Max(0,radius-Vector3.Dot(p-floor,up));
                for (int side = 0; side < Sides; side++)
                {
                    float angle = side*Mathf.PI*2/Sides;
                    Vector3 n = (right*Mathf.Cos(angle)+binormal*(Mathf.Sin(angle)/Mathf.Lerp(.55f,1,t))).normalized;
                    vertices.Add(transform.InverseTransformPoint(p + right*(Mathf.Cos(angle)*radius) + binormal*(Mathf.Sin(angle)*radius*Mathf.Lerp(.55f,1,t))));
                    normals.Add(transform.InverseTransformDirection(n));
                    if (ring == Segments) continue;
                    int a = first+ring*Sides+side, b = first+ring*Sides+(side+1)%Sides;
                    triangles.Add(a);triangles.Add(a+Sides);triangles.Add(b);
                    triangles.Add(b);triangles.Add(a+Sides);triangles.Add(b+Sides);
                }
            }
        }


        public void EndFrame()
        {
            for (int i = 0; i < fragments.Length; i++) if (fragments[i] != null && !fragments[i].Seen) fragments[i] = null;
            tendrils.Clear(); tendrils.SetVertices(vertices); tendrils.SetNormals(normals); tendrils.SetTriangles(triangles,0); tendrils.RecalculateBounds();
        }
        private static float Smooth(float min, float max, float value) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(min,max,value));
        private static float Noise(int key) => Mathf.Repeat(Mathf.Sin(key*127.1f+19.7f)*43758.5453f,1);
        private void OnDestroy() { if (tendrils != null) Destroy(tendrils); }
    }
}
