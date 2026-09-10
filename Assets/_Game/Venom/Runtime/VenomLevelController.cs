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
        public VenomFollowCamera FollowView { get; private set; }
        public bool WallCrawl => ControlMode == VenomControlMode.SurfaceCrawl || ControlMode == VenomControlMode.SplitVault || ControlMode == VenomControlMode.TouchSurface;
        public VenomGuidance Guidance;
        public VenomJourney Journey;
        public static readonly int[] Experiments = {1,2,3,4,5,7,8};
        public VenomSplitVault SplitVault;
        public float NavigationY=>SplitVault!=null?SplitVault.NavigationY:FloorBoundary.Top+.025f;
        public Vector3 NavigationUp=>SplitVault!=null?Rotation.transform.up:Vector3.up;
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
            var floor = WallCrawl ? (MeshCollider)CrawlFaces[0] : Rotation.GetComponentInChildren<MeshCollider>();
            FloorBoundary = floor.gameObject.AddComponent<VenomFloorBoundary>();
            FloorBoundary.Initialize(floor,Outlet,ApertureRadius,!WallCrawl);
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
            SplitVault?.Initialize(this);
            if (DirectControl) Locomotion = new VenomLocomotion(this);
            Guidance?.Initialize(this);
            Journey?.Initialize(this);
            if(WallCrawl)FollowView=new VenomFollowCamera(this);
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
            // A gravity drop from the raised latch can cover about 12 mm in one
            // tick. Activate the limit early enough to catch that arrival.
            float contactDistance = body == Blade && !DirectControl ? .015f : .0005f;
            joint.linearLimit=new SoftJointLimit{limit=(upper-lower)*.5f,bounciness=0,contactDistance=contactDistance};
        }

        private void FixedUpdate() { if (!Paused && !Lost) Step(Time.fixedDeltaTime); }
        public void Step(float dt)
        {
            if (Paused || Lost || dt <= 0) return;
            Organism.Step(dt);
            if(WallCrawl)
            {
                SplitVault?.Cut();Guidance?.StepMechanisms(dt);Locomotion.Step(dt);EvaluateEscape(dt);Guidance?.ObserveCompletion();CheckLost();return;
            }
            if (!BladeReleased)
            {
                Vector3 centre = Vector3.zero;
                foreach (var body in Organism.Bodies) centre += Rotation.transform.InverseTransformPoint(body.position)/32;
                // In the tilt experiment, let the soft body slide beneath a
                // raised knife before releasing its weight. A knife already on
                // the floor mostly deflects an off-centre body around its tip.
                float approach = DirectControl ? .018f : .045f;
                float alignment = DirectControl ? .035f : .04f;
                BladeReleased = Mathf.Abs(centre.z-BladeRest.z) < approach && Mathf.Abs(centre.x-BladeRest.x) < alignment;
                if (!BladeReleased)
                {
                    Vector3 parked = Rotation.transform.TransformPoint(BladeRest+Vector3.up*.064f);
                    Blade.AddForce(Rotation.transform.up*Mathf.Clamp(Vector3.Dot(parked-Blade.position,Rotation.transform.up)*65-Vector3.Dot(Blade.linearVelocity,Rotation.transform.up)*2,-2,2));
                }
            }
            if (BladeReleased)
            {
                bladeCycle += dt;
                Blade.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            }
            if (DirectControl && BladeReleased)
            {
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
            }
            // Re-arm after a missed pass or a reunited body has moved clear.
            // The raised position is reached by the same physical return spring.
            if (BladeReleased && bladeCycle > 1.8f && !GateLatched && Organism.FragmentCount == 1)
            {
                float z = 0;
                foreach (var body in Organism.Bodies) z += Rotation.transform.InverseTransformPoint(body.position).z/32;
                if (Mathf.Abs(z-BladeRest.z) > .075f) { BladeReleased = false; bladeCycle = 0; }
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
            if(SplitVault!=null)return SplitVault.NavigationFree(world,clearance,allowOutlet);
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

        public bool ExitAssistClear(Vector3 position)
        {
            Vector3 toward=Outlet.TransformPoint(new Vector3(0,0,.012f))-position;
            return !SegmentBlocked(position,position+toward.normalized*Mathf.Min(toward.magnitude,.016f));
        }
        private void EvaluateEscape(float dt)
        {
            float radius = MatterProfile.ParticleRadius;
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
            {
                Rigidbody body = Organism.Bodies[i]; Vector3 local = Outlet.InverseTransformPoint(body.position);
                Vector3 old = previous[i]; previous[i] = local;
                if (Organism.Escaped[i])
                {
                    if(WallCrawl)
                    {
                        // Keep the escaped tissue clear of an upward-facing exit
                        // while the rest crawls through; collect one visible body outside.
                        Vector3 target=Outlet.TransformPoint(new Vector3(0,0,.10f));
                        Vector3 desired=Vector3.ClampMagnitude((target-body.position)*5,.18f);
                        Vector3 relative=body.linearVelocity-Rotation.GetComponent<Rigidbody>().GetPointVelocity(body.position);
                        body.AddForce(Vector3.up*9.81f+Vector3.ClampMagnitude((desired-relative)*24,4.5f),ForceMode.Acceleration);
                    }
                    continue;
                }
                float radial = new Vector2(local.x,local.y).magnitude;
                // Support acts only at the final, unobstructed aperture; particles keep their colliders.
                if (GateLatched && local.z > -.06f && local.z < .025f && radial < .055f)
                {
                    if (ExitAssistClear(body.position))
                    {
                        Vector3 desired = Outlet.TransformDirection(new Vector3(-local.x * 9, -local.y * 9, WallCrawl ? .22f : .14f));
                        Vector3 relative = body.linearVelocity - Rotation.GetComponent<Rigidbody>().GetPointVelocity(body.position);
                        Vector3 assist=Vector3.ClampMagnitude((desired-relative)*(WallCrawl?24:7),4.5f);
                        if(WallCrawl)assist+=Vector3.up*9.81f;
                        body.AddForce(assist,ForceMode.Acceleration);
                    }
                }
                if (old.z < -.003f && local.z >= -.003f)
                {
                    Vector3 crossing = Vector3.Lerp(old, local, (-.003f-old.z)/(local.z-old.z));
                    // Use the same contact tolerance as the in-bore check below.
                    // PhysX can clear the rim within this tolerance; rejecting
                    // that first crossing leaves real escaped tissue at 31/32.
                    enteredBore[i] = new Vector2(crossing.x,crossing.y).magnitude < ApertureRadius - radius + .001f;
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
                ResetBody(Blade,BladeRest+Vector3.up*.064f); ResetBody(Gate,GateRest);
                LeftPad.ResetPlate(Rotation.transform); RightPad.ResetPlate(Rotation.transform);
            }
            else GateLatched=Guidance==null || !Guidance.RequiresButton;
            Organism.ResetMatter();
            Climbing?.Reset();
            SplitVault?.ResetState();
            FollowView?.Reset();
            Locomotion?.Reset();
            Guidance?.ResetState();
            Journey?.ResetState();
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
        public void ToggleZoom()
        {
            if(!WallCrawl)return;
            GetComponent<VenomInput>().CancelGesture();FollowView.Toggle();
        }
        public void LoadExperiment(int number)
        {
            if(Journey!=null){Journey.Load(number);return;}
            if (System.Array.IndexOf(Experiments,number)<0) return;
            Time.timeScale = 1;
            SceneManager.LoadScene($"Venom{number:00}");
        }
        internal void LatchGuidedGate()=>GateLatched=true;
        private void OnApplicationPause(bool pause) { if (pause && !Paused) TogglePause(); }
        private void OnDestroy() { Time.timeScale = 1; }
    }
}
