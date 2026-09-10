using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravityBox.Venom
{
    [DefaultExecutionOrder(-100)]
    public sealed class VenomLevelController : MonoBehaviour
    {
        public VenomProfile MatterProfile;
        public int LevelNumber = 1;
        public VenomControlMode ControlMode;
        public VenomLocomotionProfile LocomotionProfile;
        public VenomLocomotion Locomotion { get; private set; }
        public VenomWallClimb Climbing { get; private set; }
        public bool WallCrawl => ControlMode == VenomControlMode.SurfaceCrawl;
        public Collider[] CrawlFaces;
        public Material IndicatorMaterial;
        public bool DirectControl => ControlMode != VenomControlMode.TiltBox;
        public bool CanControl => !Paused && !Completed && !Lost;
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
        public bool BladeReleased { get; private set; }
        public float PairedHold { get; private set; }
        public float GateOpening => Gate == null ? 0 : Vector3.Dot(Gate.position - Rotation.transform.TransformPoint(GateRest), Rotation.transform.up);
        private readonly Vector3[] previous = new Vector3[CohesiveOrganism.ParticleCount];
        private readonly bool[] enteredBore = new bool[CohesiveOrganism.ParticleCount];
        private Collider[] boundaries;
        private Collider[] navigationObstacles;
        private Vector3 initialRootPosition;
        private float bladeCycle;

        private void Awake()
        {
            PhysicsTiming.Apply(); Application.targetFrameRate = 60;
            Rotation.Configure(RotationProfile, RotationMode.Free); Rotation.CaptureInitialState();
            initialRootPosition = Rotation.transform.position;
            var floor = Rotation.GetComponentInChildren<MeshCollider>();
            FloorBoundary = floor.gameObject.AddComponent<VenomFloorBoundary>();
            FloorBoundary.Initialize(floor,Outlet,ApertureRadius);
            boundaries = Rotation.GetComponentsInChildren<Collider>();
            navigationObstacles = System.Array.FindAll(boundaries,c => c.gameObject != floor.gameObject && c.GetComponentInParent<VenomPressurePlate>() == null && !c.isTrigger);
            if(!WallCrawl)
            {
                ConfigureFloorStop(Blade,BladeRest); ConfigureFloorStop(Gate,GateRest);
                Blade.transform.SetParent(Apparatus, true); Gate.transform.SetParent(Apparatus, true);
                LeftPad.transform.SetParent(Apparatus, true); RightPad.transform.SetParent(Apparatus, true);
            }
            var matter = new GameObject("Living matter — world space"); matter.transform.SetParent(transform, false);
            Organism = matter.AddComponent<CohesiveOrganism>(); Organism.Initialize(MatterProfile, Spawn, this);
            if(WallCrawl)Climbing=new VenomWallClimb(this);
            if (DirectControl) Locomotion = new VenomLocomotion(this);
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
            if (Paused || Lost || dt <= 0) return;
            Organism.Step(dt);
            if(WallCrawl)
            {
                Locomotion.Step(dt);EvaluateEscape(dt);CheckLost();return;
            }
            if (DirectControl && !BladeReleased)
            {
                Vector3 centre = Vector3.zero;
                foreach (var body in Organism.Bodies) centre += Rotation.transform.InverseTransformPoint(body.position)/32;
                BladeReleased = Mathf.Abs(centre.z-BladeRest.z) < .018f && Mathf.Abs(centre.x-BladeRest.x) < .035f;
                if (!BladeReleased)
                {
                    Vector3 parked = Rotation.transform.TransformPoint(BladeRest+Vector3.up*.064f);
                    Blade.AddForce(Rotation.transform.up*Mathf.Clamp(Vector3.Dot(parked-Blade.position,Rotation.transform.up)*65-Vector3.Dot(Blade.linearVelocity,Rotation.transform.up)*2,-2,2));
                }
            }
            if (!DirectControl || BladeReleased) Blade.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            if (DirectControl && BladeReleased)
            {
                bladeCycle += dt;
                // A powered press is necessary in a stationary chamber: gravity
                // alone can leave the blade supported on top of the soft body.
                float downSpeed = Vector3.Dot(Blade.linearVelocity,Rotation.transform.up);
                float drive = (-.22f-downSpeed)*4;
                if (bladeCycle > 1.2f)
                {
                    float height = Vector3.Dot(Rotation.transform.TransformPoint(BladeRest+Vector3.up*.064f)-Blade.position,Rotation.transform.up);
                    drive = height*65-downSpeed*2+Blade.mass*9.81f;
                }
                Blade.AddForce(Rotation.transform.up*Mathf.Clamp(drive,-1.5f,1.5f));
                if (bladeCycle > 1.8f && !GateLatched && Organism.FragmentCount == 1)
                {
                    float z = 0;
                    foreach (var body in Organism.Bodies) z += Rotation.transform.InverseTransformPoint(body.position).z/32;
                    if (Mathf.Abs(z-BladeRest.z) > .075f) { BladeReleased = false; bladeCycle = 0; }
                }
            }
            Organism.Cut(Blade.transform, BladeHalfSize);
            Locomotion?.Step(dt);
            if (ControlMode != VenomControlMode.FollowLargest)
            { LeftPad.Step(Organism, Rotation.transform); RightPad.Step(Organism, Rotation.transform); }
            bool pair = ControlMode == VenomControlMode.FollowLargest
                ? Organism.CutCount > 0 && Organism.MergeCount > 0 && Organism.FragmentCount == 1
                : LeftPad.Pressed && RightPad.Pressed && LeftPad.Group != RightPad.Group && Organism.CutCount > 0;
            PairedHold = pair ? PairedHold + dt : 0;
            if (PairedHold >= .22f) GateLatched = true;
            Vector3 target = Rotation.transform.TransformPoint(GateRest + Vector3.up * (GateLatched ? .145f : 0));
            Vector3 axis = Rotation.transform.up;
            float error = Vector3.Dot(target - Gate.position, axis);
            float speed = Vector3.Dot(Gate.linearVelocity - Rotation.GetComponent<Rigidbody>().GetPointVelocity(Gate.position), axis);
            Gate.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            Gate.AddForce(axis * Mathf.Clamp(error * 45 - speed * 1.6f, -2, 2));
            EvaluateEscape(dt);
            CheckLost();
        }
        private void CheckLost()
        {
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

        public bool NavigationFree(Vector3 world, float clearance, bool allowOutlet)
        {
            Vector3 p = Rotation.transform.InverseTransformPoint(world);
            if (Mathf.Abs(p.x) > .25f-clearance || Mathf.Abs(p.z) > .32f-clearance) return false;
            Vector3 hole = Rotation.transform.InverseTransformPoint(Outlet.position);
            if (!allowOutlet && new Vector2(p.x-hole.x,p.z-hole.z).magnitude < ApertureRadius+clearance) return false;
            p.y = FloorBoundary.Top+.025f; world = Rotation.transform.TransformPoint(p);
            foreach (var collider in navigationObstacles)
            {
                if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy) continue;
                // These experiments have a stationary horizontal floor. Covers
                // and a raised gate above the crawling envelope are traversable.
                Bounds bounds = collider.bounds;
                if (bounds.min.y > world.y+.02f || bounds.max.y < world.y-.016f || bounds.SqrDistance(world) >= clearance*clearance) continue;
                if ((collider.ClosestPoint(world)-world).sqrMagnitude < clearance*clearance) return false;
            }
            return true;
        }

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
            Time.timeScale = 1; Paused = Completed = GateLatched = Lost = BladeReleased = false; PairedHold = bladeCycle = 0;
            Rotation.GetComponent<Rigidbody>().position = initialRootPosition; Rotation.ResetState();
            if(!WallCrawl)
            {
                ResetBody(Blade, BladeRest); ResetBody(Gate, GateRest);
                if (DirectControl) ResetBody(Blade,BladeRest+Vector3.up*.064f);
                LeftPad.ResetPlate(Rotation.transform); RightPad.ResetPlate(Rotation.transform);
            }
            else GateLatched=true;
            Organism.ResetMatter();
            Climbing?.Reset();
            Locomotion?.Reset();
            Rotation.InputEnabled = !DirectControl || WallCrawl;
            for (int i = 0; i < previous.Length; i++)
            { previous[i] = Outlet.InverseTransformPoint(Organism.Bodies[i].position); enteredBore[i] = false; }
            Physics.SyncTransforms();
        }
        private void ResetBody(Rigidbody body, Vector3 local)
        {
            body.position = Rotation.transform.TransformPoint(local); body.rotation = Rotation.transform.rotation;
            body.linearVelocity = body.angularVelocity = Vector3.zero;
        }
        public void TogglePause()
        {
            Paused = !Paused; Time.timeScale = Paused ? 0 : 1;
            Rotation.InputEnabled = CanControl && (!DirectControl || WallCrawl); Locomotion?.SetInput(Vector3.zero);
        }
        public void LoadExperiment(int number)
        {
            if (number < 1 || number > 4) return;
            Time.timeScale = 1;
            SceneManager.LoadScene($"Venom{number:00}");
        }
        private void OnApplicationPause(bool pause) { if (pause && !Paused) TogglePause(); }
        private void OnDestroy() { Time.timeScale = 1; }
    }
}
