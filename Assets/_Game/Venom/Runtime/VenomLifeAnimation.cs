using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>
    /// Read-only interpretation of matter motion. Decorative field sources form the
    /// curious head; tapered tubes reach, plant and peel off actual contacted surfaces.
    /// Nothing here writes a Rigidbody, collider, bond, switch or escape state.
    /// </summary>
    public sealed class VenomLifeAnimation : MonoBehaviour
    {
        private const int Arms = 6, Segments = 10, Sides = 7;
        private sealed class Foot
        {
            public Collider Surface;
            public Vector3 LocalPoint, LocalNormal;
            public float Phase;
            public int PlantId;
            public bool Planted;
        }
        private sealed class Fragment
        {
            public readonly Foot[] Feet = new Foot[Arms];
            public bool Seen;
            public int Count, Peeks;
            public float IdleTime, NextPeek, PeekStart = -100, Head, Moving, Speed;
            public Vector3 Up = Vector3.up, Forward = Vector3.forward, Centre, HeadTip;
            public Fragment(int key)
            {
                NextPeek = 1.6f + Noise(key) * 1.8f;
                for (int i = 0; i < Arms; i++) Feet[i] = new Foot { Phase = Mathf.Repeat(i * .381966f + Noise(key), 1) };
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
        private readonly List<Vector3> vertices = new List<Vector3>(10000), normals = new List<Vector3>(10000);
        private readonly List<int> triangles = new List<int>(18000);
        private readonly List<Vector3> eyeVertices = new List<Vector3>(1000), eyeNormals = new List<Vector3>(1000);
        private readonly List<int> eyeTriangles = new List<int>(2000);
        private readonly List<Plant> plants = new List<Plant>(48);
        private Mesh tendrils, eyes;
        private Material eyeMaterial;
        private float clock = -1, dt;
        public float HeadAmount { get; private set; }
        public float HeadHeight { get; private set; }
        public int TendrilCount { get; private set; }
        public float CrawlAmount { get; private set; }
        public IReadOnlyList<Plant> PlantedFeet => plants;

        public void Initialize(CohesiveOrganism source, VenomLevelController owner)
        {
            organism = source; level = owner;
            tendrils = CreateMesh("Contact tendrils", source.Profile.Skin);
            eyeMaterial = new Material(source.Profile.Skin) { name = "Curious eye glints" };
            eyeMaterial.SetColor("_BaseColor", new Color(.66f,.85f,.76f));
            eyeMaterial.SetFloat("_Metallic", .1f);
            eyeMaterial.SetColor("_EmissionColor", new Color(.10f,.22f,.16f));
            eyes = CreateMesh("Curious eyes", eyeMaterial);
        }

        private Mesh CreateMesh(string label, Material material)
        {
            var go = new GameObject(label, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(transform, false);
            var result = new Mesh { name = label, indexFormat = IndexFormat.UInt32 }; result.MarkDynamic();
            go.GetComponent<MeshFilter>().sharedMesh = result;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return result;
        }

        public void BeginFrame()
        {
            float time = organism.SimulationTime;
            if (time < clock) Array.Clear(fragments, 0, fragments.Length);
            dt = clock < 0 || time < clock ? 0 : Mathf.Min(.05f, time-clock);
            clock = time;
            vertices.Clear(); normals.Clear(); triangles.Clear(); plants.Clear();
            eyeVertices.Clear(); eyeNormals.Clear(); eyeTriangles.Clear();
            HeadAmount = HeadHeight = CrawlAmount = 0; TendrilCount = 0;
            foreach (var fragment in fragments) if (fragment != null) fragment.Seen = false;
        }

        // Positions are local to the same identity-space renderer as the physical skin.
        // Extra points are visual metaballs; they never join the simulation's 32 nodes.
        public int Decorate(Vector3[] points, float[] supports, float[] weights, int[] ids, int count)
        {
            int key = ids[0];
            var state = fragments[key] ?? (fragments[key] = new Fragment(key));
            state.Seen = true;
            if (state.Count != count)
            {
                state.Head = state.IdleTime = 0; state.PeekStart = -100;
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
                if (organism.TryGetSupport(ids[i], out var collider, out var point, out var normal))
                {
                    up += normal; floorPoint += point; contacts++; support = collider;
                    if (collider.attachedRigidbody != null) relative -= collider.attachedRigidbody.GetPointVelocity(body.position);
                }
                else relative -= box.GetPointVelocity(body.position);
                velocity += relative;
            }
            centre /= count; velocity /= count; state.Centre = centre;
            Vector3 exit = level.Outlet.InverseTransformPoint(centre);
            leaving |= level.GateLatched && exit.z > -.06f && new Vector2(exit.x,exit.y).magnitude < .06f;
            bool grounded = contacts > 0 && count >= 3 && !leaving;
            if (grounded)
            {
                up.Normalize(); floorPoint /= contacts;
                state.Up = Vector3.Slerp(state.Up, up, 1-Mathf.Exp(-dt*14));
            }
            up = state.Up;
            Vector3 tangentVelocity = Vector3.ProjectOnPlane(velocity, up);
            state.Speed = Mathf.Lerp(state.Speed, tangentVelocity.magnitude, 1-Mathf.Exp(-dt*10));
            float moving = grounded ? Mathf.SmoothStep(0,1,Mathf.InverseLerp(.010f,.065f,state.Speed)) : 0;
            state.Moving = Mathf.MoveTowards(state.Moving, moving, dt * (grounded ? 5 : 12));
            state.IdleTime = grounded && state.Speed < .024f ? state.IdleTime+dt : 0;
            if (grounded && count >= 8 && state.IdleTime > state.NextPeek)
            {
                state.PeekStart = clock; state.IdleTime = 0;
                state.NextPeek = 5.2f + Noise(key + ++state.Peeks*17)*3.4f;
            }
            float peek = (clock-state.PeekStart)/3.0f;
            float envelope = Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,.25f,peek))
                           * (1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.72f,1,peek)));
            float targetHead = grounded && state.Speed < .024f && count >= 8 ? envelope : 0;
            state.Head = Mathf.MoveTowards(state.Head,targetHead,dt*3.5f);
            // No feet remain visually attached to the aperture or while airborne.
            if (!grounded) foreach (var foot in state.Feet) { foot.Surface = null; foot.Planted = false; }

            float idle = grounded ? 1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.015f,.07f,state.Speed)) : 0;
            Vector3 localCentre = transform.InverseTransformPoint(centre), localUp = transform.InverseTransformDirection(up);
            float top = 0, bottom = 0;
            for (int i = 0; i < count; i++)
            {
                float height = Vector3.Dot(points[i]-localCentre,localUp);
                top = Mathf.Max(top,height); bottom = Mathf.Min(bottom,height);
            }
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = points[i]-localCentre;
                float upper = Mathf.InverseLerp(bottom-.001f,top+.001f,Vector3.Dot(offset,localUp));
                float pulse = Mathf.Sin(clock*2.2f + offset.x*90 + offset.z*65 + key*.37f);
                Vector3 bulge = localUp * (pulse * organism.Profile.IdleBulge * upper * upper)
                    + Vector3.ProjectOnPlane(offset,localUp) * (.045f * Mathf.Sin(clock*1.7f + key) * upper);
                points[i] += bulge * idle;
            }

            if (grounded && state.Moving > .12f)
                DrawFeet(state,key,points,ids,count,centre,up,floorPoint,tangentVelocity,support);

            if (state.Head < .008f) return count;
            Vector3 forward = Vector3.ProjectOnPlane(level.View.transform.position-centre,up).normalized;
            if (forward.sqrMagnitude < .1f) forward = Vector3.ProjectOnPlane(level.Rotation.transform.forward,up).normalized;
            float glance = Mathf.Sin(Mathf.Clamp01(peek)*Mathf.PI*2-.8f) * 48;
            forward = Quaternion.AngleAxis(glance,up)*forward;
            state.Forward = forward;
            Vector3 anchor = centre + up*(top*.7f);
            float lift = organism.Profile.CuriousHeadLift * state.Head;
            Vector3 headDirection = (up + forward*.27f).normalized;
            // Probe head clearance at the crown and both shoulders before extruding.
            Vector3 side = Vector3.Cross(up,forward).normalized;
            float clearance = lift + .025f;
            for (int p = -1; p <= 1; p++)
                if (level.RaycastBoundary(anchor + side*(p*.013f),headDirection,clearance,out var hit))
                    clearance = hit.distance;
            lift = Mathf.Min(lift,Mathf.Max(0,clearance-.023f));
            float head = Mathf.Min(state.Head,lift/Mathf.Max(.001f,organism.Profile.CuriousHeadLift));
            HeadAmount = Mathf.Max(HeadAmount,head); HeadHeight = Mathf.Max(HeadHeight,lift);
            if (head < .02f) return count;
            int original = count;
            for (int j = 0; j < 6; j++)
            {
                float t = j/5f;
                Vector3 source = anchor + up*(lift*t) + forward*(lift*.33f*t*t);
                points[count] = transform.InverseTransformPoint(source);
                supports[count] = j==0 ? .022f : j<4 ? .0165f : j==4 ? .021f : .0255f;
                weights[count++] = head * (j==0 ? .8f : j<4 ? .52f : j==4 ? .85f : 1.15f);
            }
            state.HeadTip = transform.TransformPoint(points[count-1]);
            if (head > .45f)
            {
                float show = Mathf.SmoothStep(0,1,Mathf.InverseLerp(.45f,.9f,head));
                // Small forward-facing glints make the left/right glance readable.
                for (int eye = -1; eye <= 1; eye += 2)
                    Eye(state.HeadTip + forward*.016f + side*(eye*.0054f) + up*.002f,
                        side,up,forward,new Vector3(.0031f,.00165f,.0014f)*show);
            }
            return original+6;
        }

        private void DrawFeet(Fragment state, int key, Vector3[] points, int[] ids, int count,
            Vector3 centre, Vector3 up, Vector3 floorPoint, Vector3 velocity, Collider support)
        {
            Vector3 along = Vector3.ProjectOnPlane(support.transform.forward,up).normalized;
            if (along.sqrMagnitude < .1f) along = Vector3.ProjectOnPlane(support.transform.right,up).normalized;
            Vector3 across = Vector3.Cross(up,along).normalized;
            CrawlAmount = Mathf.Max(CrawlAmount,state.Moving);
            int limbCount = count < 8 ? 4 : Arms;
            for (int arm = 0; arm < limbCount; arm++)
            {
                Foot foot = state.Feet[arm];
                Vector3 direction = along*Mathf.Cos(arm*Mathf.PI*2/limbCount) + across*Mathf.Sin(arm*Mathf.PI*2/limbCount);
                int rootId = 0; float best = float.NegativeInfinity;
                for (int i = 0; i < count; i++)
                {
                    Vector3 offset = transform.TransformPoint(points[i])-centre;
                    float score = Vector3.Dot(offset,direction)-Mathf.Abs(Vector3.Dot(offset,up)+.012f)*.8f;
                    if (score > best) { rootId = i; best = score; }
                }
                Vector3 root = transform.TransformPoint(points[rootId])+direction*.005f;
                float phase = foot.Phase + dt*Mathf.Lerp(1.1f,3.2f,Mathf.Clamp01(state.Speed/.35f));
                if (phase >= 1) { phase %= 1; foot.Surface = null; }
                foot.Phase = phase; foot.Planted = false;
                if (foot.Surface == null)
                {
                    Vector3 goal = root+direction*organism.Profile.TendrilReach + velocity.normalized*.006f;
                    float height = Vector3.Dot(goal-floorPoint,up);
                    goal -= up*height;
                    if (!level.RaycastBoundary(goal+up*.025f,-up,.04f,out var hit) || Vector3.Dot(hit.normal,up)<.75f) continue;
                    if (level.SegmentBlocked(root,hit.point+up*.0012f)) continue;
                    foot.Surface = hit.collider; foot.LocalPoint = hit.collider.transform.InverseTransformPoint(hit.point);
                    foot.LocalNormal = hit.collider.transform.InverseTransformDirection(hit.normal);
                    foot.PlantId++;
                }
                Vector3 normal = foot.Surface.transform.TransformDirection(foot.LocalNormal).normalized;
                Vector3 anchor = foot.Surface.transform.TransformPoint(foot.LocalPoint);
                float distance = Vector3.Distance(root,anchor);
                if (distance > .070f || !foot.Surface.enabled || level.SegmentBlocked(root,anchor+normal*.0012f))
                { foot.Surface = null; continue; }
                float reach = Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,.23f,phase));
                float release = Mathf.SmoothStep(0,1,Mathf.InverseLerp(.67f,1,phase));
                float growth = Mathf.SmoothStep(0,1,Mathf.InverseLerp(.12f,.4f,state.Moving));
                Vector3 end = Vector3.Lerp(root,anchor+normal*.0008f,reach*(1-release)*growth);
                end += up*(Mathf.Sin(release*Mathf.PI)*.009f + Mathf.Sin(reach*Mathf.PI)*.004f);
                foot.Planted = phase >= .23f && phase < .67f && growth >= .999f;
                if (foot.Planted) plants.Add(new Plant(key,arm,foot.PlantId,foot.Surface,foot.LocalPoint,end));
                Vector3 p1 = root + direction*.006f + up*.005f;
                Vector3 p2 = Vector3.Lerp(root,end,.68f) + up*(.004f+release*.008f)
                    + Vector3.Cross(up,direction)*(.003f*Mathf.Sin(phase*Mathf.PI*2+arm));
                Tube(root,p1,p2,end,up,anchor,state.Moving);
                TendrilCount++;
            }
        }

        private void Tube(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 up, Vector3 floor, float amount)
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
                float radius = Mathf.Lerp(.0044f,.00065f,Mathf.Pow(t,.62f))*Mathf.Lerp(.35f,1,amount);
                p += up*Mathf.Max(0,radius-Vector3.Dot(p-floor,up));
                for (int side = 0; side < Sides; side++)
                {
                    float angle = side*Mathf.PI*2/Sides;
                    Vector3 n = right*Mathf.Cos(angle)+binormal*Mathf.Sin(angle);
                    vertices.Add(transform.InverseTransformPoint(p+n*radius));
                    normals.Add(transform.InverseTransformDirection(n));
                    if (ring == Segments) continue;
                    int a = first+ring*Sides+side, b = first+ring*Sides+(side+1)%Sides;
                    triangles.Add(a);triangles.Add(a+Sides);triangles.Add(b);
                    triangles.Add(b);triangles.Add(a+Sides);triangles.Add(b+Sides);
                }
            }
        }

        private void Eye(Vector3 centre, Vector3 right, Vector3 up, Vector3 forward, Vector3 scale)
        {
            int first = eyeVertices.Count; const int longitude = 10, latitude = 6;
            for (int y = 0; y <= latitude; y++) for (int x = 0; x <= longitude; x++)
            {
                float a = x*Mathf.PI*2/longitude, b = y*Mathf.PI/latitude;
                Vector3 unit = new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a));
                Vector3 n = (right*(unit.x/Mathf.Max(scale.x,.00001f))+up*(unit.y/Mathf.Max(scale.y,.00001f))+forward*(unit.z/Mathf.Max(scale.z,.00001f))).normalized;
                eyeVertices.Add(transform.InverseTransformPoint(centre+right*(unit.x*scale.x)+up*(unit.y*scale.y)+forward*(unit.z*scale.z)));
                eyeNormals.Add(transform.InverseTransformDirection(n));
                if (y==latitude || x==longitude) continue;
                int i=first+y*(longitude+1)+x;
                eyeTriangles.Add(i);eyeTriangles.Add(i+1);eyeTriangles.Add(i+longitude+1);
                eyeTriangles.Add(i+1);eyeTriangles.Add(i+longitude+2);eyeTriangles.Add(i+longitude+1);
            }
        }

        public void EndFrame()
        {
            for (int i = 0; i < fragments.Length; i++) if (fragments[i] != null && !fragments[i].Seen) fragments[i] = null;
            tendrils.Clear();tendrils.SetVertices(vertices);tendrils.SetNormals(normals);tendrils.SetTriangles(triangles,0);tendrils.RecalculateBounds();
            eyes.Clear();eyes.SetVertices(eyeVertices);eyes.SetNormals(eyeNormals);eyes.SetTriangles(eyeTriangles,0);eyes.RecalculateBounds();
        }
        private static float Noise(int key) => Mathf.Repeat(Mathf.Sin(key*127.1f+19.7f)*43758.5453f,1);
        private void OnDestroy()
        {
            if (tendrils != null) Destroy(tendrils);
            if (eyes != null) Destroy(eyes);
            if (eyeMaterial != null) Destroy(eyeMaterial);
        }
    }
}
