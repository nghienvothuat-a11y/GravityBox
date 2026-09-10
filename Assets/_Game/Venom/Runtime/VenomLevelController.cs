using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Venom
{
    [DefaultExecutionOrder(-100)]
    public sealed class VenomLevelController : MonoBehaviour
    {
        public VenomProfile MatterProfile;
        public RotationSettings RotationProfile;
        public BoxRotationController Rotation;
        public Transform Spawn, Outlet;
        public Rigidbody Blade, Gate;
        public Vector3 BladeRest, GateRest;
        public VenomPressurePlate LeftPad, RightPad;
        public Camera View;
        public Transform Apparatus;
        public float ApertureRadius = .035f;
        public Vector3 BladeHalfSize = new Vector3(.003f,.06f,.055f);
        public CohesiveOrganism Organism { get; private set; }
        public VenomFloorBoundary FloorBoundary { get; private set; }
        public bool GateLatched { get; private set; }
        public bool Completed { get; private set; }
        public bool Paused { get; private set; }
        public bool Lost { get; private set; }
        public float PairedHold { get; private set; }
        public float GateOpening => Vector3.Dot(Gate.position - Rotation.transform.TransformPoint(GateRest), Rotation.transform.up);
        private readonly Vector3[] previous = new Vector3[CohesiveOrganism.ParticleCount];
        private readonly bool[] enteredBore = new bool[CohesiveOrganism.ParticleCount];
        private Collider[] boundaries;
        private Vector3 initialRootPosition;

        private void Awake()
        {
            PhysicsTiming.Apply(); Application.targetFrameRate = 60;
            Rotation.Configure(RotationProfile, RotationMode.Free); Rotation.CaptureInitialState();
            initialRootPosition = Rotation.transform.position;
            var floor = Rotation.GetComponentInChildren<MeshCollider>();
            FloorBoundary = floor.gameObject.AddComponent<VenomFloorBoundary>();
            FloorBoundary.Initialize(floor,Outlet,ApertureRadius);
            boundaries = Rotation.GetComponentsInChildren<Collider>();
            ConfigureFloorStop(Blade,BladeRest);
            ConfigureFloorStop(Gate,GateRest);
            Blade.transform.SetParent(Apparatus, true); Gate.transform.SetParent(Apparatus, true);
            LeftPad.transform.SetParent(Apparatus, true); RightPad.transform.SetParent(Apparatus, true);
            var matter = new GameObject("Living matter — world space"); matter.transform.SetParent(transform, false);
            Organism = matter.AddComponent<CohesiveOrganism>(); Organism.Initialize(MatterProfile, Spawn, this);
            matter.AddComponent<VenomSurface>().Initialize(Organism, this);
            gameObject.AddComponent<VenomInput>().Initialize(this);
            gameObject.AddComponent<VenomHud>().Initialize(this);
            ResetExperiment();
        }

        private void ConfigureFloorStop(Rigidbody body,Vector3 rest)
        {
            // These sliders disable collision with their connected chamber, so
            // the joint itself must stop the blade/gate before it enters the floor.
            // Keep the authored upper end; centre a symmetric joint limit between
            // that end and the actual lower face of the mechanism.
            var shape=body.GetComponentInChildren<BoxCollider>();
            float bottom=float.PositiveInfinity;
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
            {
                Vector3 corner=shape.center+Vector3.Scale(shape.size,new Vector3(x,y,z))*.5f;
                bottom=Mathf.Min(bottom,body.transform.InverseTransformPoint(shape.transform.TransformPoint(corner)).y);
            }
            var joint=body.GetComponent<ConfigurableJoint>();
            float upper=rest.y+joint.linearLimit.limit;
            float lower=FloorBoundary.Top-bottom+.0005f;
            joint.connectedAnchor=new Vector3(rest.x,(upper+lower)*.5f,rest.z);
            joint.linearLimit=new SoftJointLimit{limit=(upper-lower)*.5f,bounciness=0,contactDistance=.0005f};
        }

        private void FixedUpdate() { if (!Paused && !Lost) Step(Time.fixedDeltaTime); }
        public void Step(float dt)
        {
            Organism.Step(dt);
            Blade.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            Organism.Cut(Blade.transform, BladeHalfSize);
            LeftPad.Step(Organism, Rotation.transform); RightPad.Step(Organism, Rotation.transform);
            bool pair = LeftPad.Pressed && RightPad.Pressed && LeftPad.Group != RightPad.Group && Organism.CutCount > 0;
            PairedHold = pair ? PairedHold + dt : 0;
            if (PairedHold >= .22f) GateLatched = true;
            Vector3 target = Rotation.transform.TransformPoint(GateRest + Vector3.up * (GateLatched ? .145f : 0));
            Vector3 axis = Rotation.transform.up;
            float error = Vector3.Dot(target - Gate.position, axis);
            float speed = Vector3.Dot(Gate.linearVelocity - Rotation.GetComponent<Rigidbody>().GetPointVelocity(Gate.position), axis);
            Gate.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            Gate.AddForce(axis * Mathf.Clamp(error * 45 - speed * 1.6f, -2, 2));
            EvaluateEscape(dt);
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                if (!Organism.Escaped[i] && Rotation.transform.InverseTransformPoint(Organism.Bodies[i].position).magnitude > 1.6f)
                { Lost = true; Rotation.InputEnabled = false; }
        }

        public bool SegmentBlocked(Vector3 a, Vector3 b)
        {
            var ray = new Ray(a, (b-a).normalized); float distance = Vector3.Distance(a,b);
            foreach (Collider collider in boundaries)
                if (collider != null && collider.enabled && collider.Raycast(ray, out _, distance)) return true;
            return false;
        }

        public bool IsBoundary(Collider collider) => System.Array.IndexOf(boundaries, collider) >= 0;

        public bool RaycastBoundary(Vector3 origin, Vector3 direction, float distance, out RaycastHit nearest)
        {
            nearest = default; bool found = false;
            var ray = new Ray(origin, direction);
            foreach (Collider collider in boundaries)
                if (collider != null && collider.enabled && collider.Raycast(ray, out var hit, distance))
                { nearest = hit; distance = hit.distance; found = true; }
            return found;
        }

        private void EvaluateEscape(float dt)
        {
            float radius = MatterProfile.ParticleRadius;
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
            {
                Rigidbody body = Organism.Bodies[i]; Vector3 local = Outlet.InverseTransformPoint(body.position);
                Vector3 old = previous[i]; previous[i] = local;
                if (Organism.Escaped[i]) continue;
                float radial = new Vector2(local.x,local.y).magnitude;
                // Support acts only at the final, unobstructed aperture; particles keep their colliders.
                if (GateLatched && local.z > -.06f && local.z < .025f && radial < .055f)
                {
                    Vector3 toward = Outlet.TransformPoint(new Vector3(0,0,.012f)) - body.position;
                    if (!SegmentBlocked(body.position, body.position + toward.normalized * Mathf.Min(toward.magnitude, .016f)))
                    {
                        Vector3 desired = Outlet.TransformDirection(new Vector3(-local.x * 9, -local.y * 9, .14f));
                        Vector3 relative = body.linearVelocity - Rotation.GetComponent<Rigidbody>().GetPointVelocity(body.position);
                        body.AddForce(Vector3.ClampMagnitude((desired-relative)*7, 4.5f), ForceMode.Acceleration);
                    }
                }
                if (old.z < -.003f && local.z >= -.003f)
                {
                    Vector3 crossing = Vector3.Lerp(old, local, (-.003f-old.z)/(local.z-old.z));
                    enteredBore[i] = new Vector2(crossing.x,crossing.y).magnitude < ApertureRadius - radius;
                }
                if (local.z < -.012f) enteredBore[i] = false;
                if (local.z >= -.003f && local.z <= .003f && radial > ApertureRadius-radius+.001f) enteredBore[i] = false;
                float completionPlane=.003f+radius;
                if (GateLatched && enteredBore[i] && old.z < completionPlane && local.z >= completionPlane)
                {
                    // Once the centre has traversed the bore, the trailing hemisphere can
                    // clear the underside while moving sideways. Do not test a new cylinder
                    // below the physical opening: it incorrectly rejects legitimate exits.
                    Organism.RecordEscape(i);
                }
            }
            if (Organism.EscapedCount == CohesiveOrganism.ParticleCount)
            { Completed = true; Rotation.InputEnabled = false; }
        }

        public void ResetExperiment()
        {
            Time.timeScale = 1; Paused = Completed = GateLatched = Lost = false; PairedHold = 0;
            Rotation.GetComponent<Rigidbody>().position = initialRootPosition; Rotation.ResetState();
            ResetBody(Blade, BladeRest); ResetBody(Gate, GateRest);
            LeftPad.ResetPlate(Rotation.transform); RightPad.ResetPlate(Rotation.transform);
            Organism.ResetMatter();
            for (int i = 0; i < previous.Length; i++)
            { previous[i] = Outlet.InverseTransformPoint(Organism.Bodies[i].position); enteredBore[i] = false; }
            Physics.SyncTransforms();
        }
        private void ResetBody(Rigidbody body, Vector3 local)
        {
            body.position = Rotation.transform.TransformPoint(local); body.rotation = Rotation.transform.rotation;
            body.linearVelocity = body.angularVelocity = Vector3.zero;
        }
        public void TogglePause() { Paused = !Paused; Time.timeScale = Paused ? 0 : 1; Rotation.InputEnabled = !Paused && !Completed && !Lost; }
        private void OnApplicationPause(bool pause) { if (pause && !Paused) TogglePause(); }
        private void OnDestroy() { Time.timeScale = 1; }
    }
}
