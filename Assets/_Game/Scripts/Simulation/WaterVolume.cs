using GravityBox.Foundation;
using UnityEngine;

namespace GravityBox.Simulation
{
    /// <summary>
    /// Retained, completely filled Newtonian liquid. Legacy Water name preserves assets.
    /// Water and mercury use the same SI-unit force path, driven only by their profile.
    /// Rotation-driven bulk flow is a bounded approximation, not Navier–Stokes/SPH.
    /// The real exit remains open to the ball; water retention is an authored game rule.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class WaterVolume : MonoBehaviour, IForceProvider, IForceStepProvider, IResettable
    {
        public WaterProfile Profile;
        public Vector3 HalfSize = new Vector3(.16f, .042f, .16f);
        public Bounds Obstacle = new Bounds(new Vector3(0, -.010f, 0), Vector3.one * .064f);
        private Rigidbody container;
        private BallController ball;
        private EnvironmentProfile environment;
        private Quaternion previousRotation;
        private Vector3 previousPosition, boxSpin, boxVelocity, fluidSpin, acceleration;
        private Collider[] surfaces = System.Array.Empty<Collider>();
        private FlowState flow, oldFlow;
        private struct FlowState
        {
            public Vector3 Position, Linear, BoxSpin, FluidSpin;
            public Quaternion Rotation;
            public Vector3 Velocity(Vector3 world, Vector3 halfSize)
            {
                Vector3 p = Quaternion.Inverse(Rotation) * (world - Position);
                Vector3 relative = Quaternion.Inverse(Rotation) * Vector3.Cross(FluidSpin - BoxSpin, Rotation * p);
                for (int axis = 0; axis < 3; axis++)
                    relative[axis] *= Mathf.Clamp01(1 - p[axis] * p[axis] / (halfSize[axis] * halfSize[axis]));
                return Linear + Vector3.Cross(BoxSpin, Rotation * p) + Rotation * relative;
            }
        }
        public BallController Ball => ball;
        public float SubmergedFraction { get; private set; }
        public Vector3 BuoyancyForce { get; private set; }
        public Vector3 DragForce { get; private set; }
        public Vector3 WallDragForce { get; private set; }
        public Vector3 FluidAccelerationForce { get; private set; }
        public float AddedMass { get; private set; }
        public float WallRollingWeight { get; private set; }
        public Vector3 RelativeVelocity { get; private set; }
        public Vector3 FluidAngularVelocity => fluidSpin;
        public Vector3 BoxAngularVelocity => boxSpin;

        public void Bind(BallController target, EnvironmentProfile world)
        {
            container = GetComponent<Rigidbody>(); ball = target; environment = world;
            surfaces = GetComponentsInChildren<Collider>();
            ResetState();
        }

        public void PrepareStep(float dt)
        {
            if (!isActiveAndEnabled || container == null || ball == null || Profile == null || dt <= 0) return;
            oldFlow = flow;
            Quaternion change = container.rotation * Quaternion.Inverse(previousRotation);
            change.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            boxSpin = Mathf.Abs(angle) > .00001f ? axis.normalized * (angle * Mathf.Deg2Rad / dt) : Vector3.zero;
            boxVelocity = (container.position - previousPosition) / dt;
            previousRotation = container.rotation; previousPosition = container.position;
            fluidSpin = Vector3.Lerp(fluidSpin, boxSpin, 1 - Mathf.Exp(-dt / Profile.EntrainmentSeconds));
            flow = new FlowState { Position = container.position, Rotation = container.rotation,
                Linear = boxVelocity, BoxSpin = boxSpin, FluidSpin = fluidSpin };
            float radius = ball.Profile.Radius;
            SubmergedFraction = Immersion(ball.Body.position, radius);
            float volume = 4f / 3f * Mathf.PI * radius * radius * radius * SubmergedFraction;
            float displacedMass = Profile.Density * volume;
            SetAddedMass(.5f * displacedMass);
            BuoyancyForce = -environment.Acceleration * (Profile.Density * volume);
            RelativeVelocity = ball.Body.linearVelocity - VelocityAt(ball.Body.position);
            Vector3 freeDrag = SphereDrag(RelativeVelocity, radius, Profile.Density, Profile.DynamicViscosity);
            WallDragForce = RollingWallCorrection(radius, out float wallWeight) * SubmergedFraction;
            WallRollingWeight = wallWeight;
            DragForce = freeDrag * SubmergedFraction + WallDragForce;
            // Backward drag coefficient integration prevents reversal at coarse timesteps.
            // No velocity assignment; the native contact solver receives a force impulse.
            float freeRate = freeDrag.magnitude * SubmergedFraction / Mathf.Max(RelativeVelocity.magnitude, 1e-9f);
            float wallSpeed = Vector3.Project(RelativeVelocity, WallDragForce).magnitude;
            float wallRate = WallDragForce.magnitude / Mathf.Max(wallSpeed, 1e-9f);
            float freeDenominator = 1 + freeRate * dt / ball.Body.mass;
            WallDragForce /= freeDenominator * (1 + (freeRate + wallRate) * dt / ball.Body.mass);
            DragForce = freeDrag * SubmergedFraction / freeDenominator + WallDragForce;
            // Material derivative of the SAME approximate ambient flow used for drag.
            // Backtrace a fluid parcel, not the ball, so ball motion cannot create a
            // fictitious pressure impulse. Includes local and convective acceleration.
            Vector3 oldVelocity = oldFlow.Velocity(ball.Body.position, HalfSize);
            Vector3 fluidAcceleration = (VelocityAt(ball.Body.position)
                - oldFlow.Velocity(ball.Body.position - oldVelocity * dt, HalfSize)) / dt;
            FluidAccelerationForce = (displacedMass + AddedMass) * fluidAcceleration;
            // Rigidbody.mass is translational inertia (steel + entrained water).
            // Cancel added water's gravity: steel weight remains m_steel*g, not m_eff*g.
            acceleration = (BuoyancyForce + DragForce + FluidAccelerationForce
                - AddedMass * environment.Acceleration) / ball.Body.mass;
            if (SubmergedFraction <= 0 || ball.Body.isKinematic) return;
            // Low-Re rotational viscous drag on a sphere, not arbitrary angular damping.
            Vector3 relativeSpin = ball.Body.angularVelocity - fluidSpin;
            Vector3 torque = -relativeSpin * (8 * Mathf.PI * Profile.DynamicViscosity * radius * radius * radius
                * SubmergedFraction * (1 - wallWeight));
            torque = Vector3.ClampMagnitude(torque, ball.Profile.SolidSphereInertia * relativeSpin.magnitude / dt);
            ball.Body.AddTorque(torque, ForceMode.Force);
        }

        public Vector3 GetAcceleration(IPhysicsAffectable target, EnvironmentProfile world)
            => isActiveAndEnabled && ReferenceEquals(target, ball) ? acceleration : Vector3.zero;

        public static Vector3 SphereDrag(Vector3 relativeVelocity, float radius, float density, float viscosity)
            => WaterHydrodynamics.FreeSphereDrag(relativeVelocity, radius, density, viscosity);

        private Vector3 RollingWallCorrection(float radius, out float weight)
        {
            weight = 0; Vector3 correction = Vector3.zero;
            if (SubmergedFraction <= 0) return correction;
            Vector3 centre = ball.Body.position;
            Vector3 wallVelocity = boxVelocity + Vector3.Cross(boxSpin, centre - container.position);
            Vector3 v = ball.Body.linearVelocity - wallVelocity;
            Vector3 spin = ball.Body.angularVelocity - boxSpin;
            // L13/L14 have axis-aligned flat panels and a cube. Query the ACTUAL collider
            // faces, including the cut floor, not the water bounds: never seal the exit.
            // Use the strongest rolling face, not six stacked copies of the drag law.
            for (int axis = 0; axis < 3; axis++)
            for (int sign = -1; sign <= 1; sign += 2)
            {
                Vector3 localDirection = Vector3.zero; localDirection[axis] = sign;
                Vector3 direction = container.rotation * localDirection;
                Ray ray = new Ray(centre, direction);
                float distance = radius * 1.25f; Vector3 normal = Vector3.zero;
                foreach (Collider surface in surfaces)
                {
                    if (surface == null || !surface.enabled || surface.isTrigger || surface.attachedRigidbody != container) continue;
                    if (surface.Raycast(ray, out RaycastHit hit, distance) && Vector3.Dot(hit.normal, direction) < -.99f)
                    { distance = hit.distance; normal = hit.normal; }
                }
                if (normal == Vector3.zero) continue;
                float gap = Mathf.Max(0, distance - radius);
                Vector3 tangent = Vector3.ProjectOnPlane(v, normal);
                Vector3 slip = tangent - Vector3.Cross(spin, normal) * radius;
                float roll = tangent.magnitude < .0001f ? 0 : Mathf.Clamp01(1 - slip.magnitude / tangent.magnitude);
                // This transition away from a wall is a model interpolation, not a
                // measured finite-gap correlation. Sliding/flight retains sphere drag.
                float candidate = roll * (1 - Mathf.SmoothStep(0, 1, gap / (radius * .25f)));
                if (candidate <= weight) continue;
                Vector3 relativeTangent = Vector3.ProjectOnPlane(RelativeVelocity, normal);
                float speed = relativeTangent.magnitude;
                float rolling = WaterHydrodynamics.RollingSphereResistance(speed, radius, Profile.Density,
                    Profile.DynamicViscosity, Mathf.Max(gap, Profile.EffectiveRoughness));
                float free = SphereDrag(relativeTangent, radius, Profile.Density, Profile.DynamicViscosity).magnitude;
                correction = -relativeTangent.normalized * Mathf.Max(0, rolling - free) * candidate;
                weight = candidate;
            }
            return correction;
        }

        private void SetAddedMass(float value)
        {
            AddedMass = value;
            if (ball == null || ball.Profile == null) return;
            float effectiveMass = ball.Profile.Mass + value;
            if (Mathf.Abs(ball.Body.mass - effectiveMass) < 1e-8f) return;
            ball.Body.mass = effectiveMass;
            // A sphere's inviscid rotational added inertia is zero. Keep solid steel I.
            ball.Body.inertiaTensor = Vector3.one * ball.Profile.SolidSphereInertia;
        }

        private void OnDisable() { SetAddedMass(0); acceleration = Vector3.zero; }
        private void OnEnable() { if (container != null) ResetState(); }

        public float Immersion(Vector3 worldPosition, float radius)
        {
            Vector3 p = LocalPoint(worldPosition);
            float inside = Mathf.Min(HalfSize.x - Mathf.Abs(p.x), HalfSize.y - Mathf.Abs(p.y), HalfSize.z - Mathf.Abs(p.z));
            float h = Mathf.Clamp(radius + inside, 0, 2 * radius);
            // Exact spherical cap at the isolated exit plane. At inaccessible outer
            // corners this nearest-plane approximation is not a volume intersection.
            return h * h * (3 * radius - h) / (4 * radius * radius * radius);
        }

        public Vector3 LocalPoint(Vector3 world) => Quaternion.Inverse(container != null ? container.rotation : transform.rotation)
            * (world - (container != null ? container.position : transform.position));

        public Vector3 VelocityAt(Vector3 worldPosition)
        {
            if (container == null) return Vector3.zero;
            // Rendering/tests may sample after PhysX has completed MoveRotation.
            // Re-express this step's flow in the latest rigid pose at that boundary.
            FlowState current = flow;
            current.Position = container.position; current.Rotation = container.rotation;
            return current.Velocity(worldPosition, HalfSize);
        }

        public Vector3 LocalFlowAt(Vector3 localPosition)
        {
            Quaternion rotation = container != null ? container.rotation : transform.rotation;
            Vector3 position = container != null ? container.position : transform.position;
            Vector3 relative = VelocityAt(position + rotation * localPosition) - boxVelocity - Vector3.Cross(boxSpin, rotation * localPosition);
            return Quaternion.Inverse(rotation) * relative;
        }

        public bool ContainsVisual(Vector3 local, float margin = 0)
        {
            if (Mathf.Abs(local.x) > HalfSize.x - margin || Mathf.Abs(local.y) > HalfSize.y - margin || Mathf.Abs(local.z) > HalfSize.z - margin) return false;
            Bounds blocked = Obstacle; blocked.Expand(margin * 2);
            return !blocked.Contains(local);
        }

        public void CaptureInitialState() { }
        public void ResetState()
        {
            if (container == null) container = GetComponent<Rigidbody>();
            previousRotation = container.rotation; previousPosition = container.position;
            boxSpin = boxVelocity = fluidSpin = acceleration = Vector3.zero;
            flow = oldFlow = new FlowState { Position = container.position, Rotation = container.rotation };
            BuoyancyForce = DragForce = WallDragForce = FluidAccelerationForce = RelativeVelocity = Vector3.zero;
            WallRollingWeight = 0; SetAddedMass(0);
            SubmergedFraction = ball != null ? Immersion(ball.Body.position, ball.Profile.Radius) : 0;
        }
    }
}
