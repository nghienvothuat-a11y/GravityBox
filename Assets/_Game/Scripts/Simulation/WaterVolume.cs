using GravityBox.Foundation;
using UnityEngine;

namespace GravityBox.Simulation
{
    /// <summary>
    /// Retained, completely filled water. Archimedes buoyancy and sphere drag in SI units.
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
        public BallController Ball => ball;
        public float SubmergedFraction { get; private set; }
        public Vector3 BuoyancyForce { get; private set; }
        public Vector3 DragForce { get; private set; }
        public Vector3 RelativeVelocity { get; private set; }
        public Vector3 FluidAngularVelocity => fluidSpin;
        public Vector3 BoxAngularVelocity => boxSpin;

        public void Bind(BallController target, EnvironmentProfile world)
        {
            container = GetComponent<Rigidbody>(); ball = target; environment = world;
            ResetState();
        }

        public void PrepareStep(float dt)
        {
            if (container == null || ball == null || Profile == null || dt <= 0) return;
            Quaternion change = container.rotation * Quaternion.Inverse(previousRotation);
            change.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            boxSpin = Mathf.Abs(angle) > .00001f ? axis.normalized * (angle * Mathf.Deg2Rad / dt) : Vector3.zero;
            boxVelocity = (container.position - previousPosition) / dt;
            previousRotation = container.rotation; previousPosition = container.position;
            fluidSpin = Vector3.Lerp(fluidSpin, boxSpin, 1 - Mathf.Exp(-dt / Profile.EntrainmentSeconds));
            float radius = ball.Profile.Radius;
            SubmergedFraction = Immersion(ball.Body.position, radius);
            float volume = 4f / 3f * Mathf.PI * radius * radius * radius * SubmergedFraction;
            BuoyancyForce = -environment.Acceleration * (Profile.Density * volume);
            RelativeVelocity = ball.Body.linearVelocity - VelocityAt(ball.Body.position);
            DragForce = SphereDrag(RelativeVelocity, radius, Profile.Density, Profile.DynamicViscosity) * SubmergedFraction;
            // A dissipative impulse cannot cross zero relative speed within one tick.
            DragForce = Vector3.ClampMagnitude(DragForce, ball.Body.mass * RelativeVelocity.magnitude / dt);
            acceleration = (BuoyancyForce + DragForce) / ball.Body.mass;
            if (SubmergedFraction <= 0 || ball.Body.isKinematic) return;
            // Low-Re rotational viscous drag on a sphere, not arbitrary angular damping.
            Vector3 relativeSpin = ball.Body.angularVelocity - fluidSpin;
            Vector3 torque = -relativeSpin * (8 * Mathf.PI * Profile.DynamicViscosity * radius * radius * radius * SubmergedFraction);
            torque = Vector3.ClampMagnitude(torque, ball.Profile.SolidSphereInertia * relativeSpin.magnitude / dt);
            ball.Body.AddTorque(torque, ForceMode.Force);
        }

        public Vector3 GetAcceleration(IPhysicsAffectable target, EnvironmentProfile world)
            => ReferenceEquals(target, ball) ? acceleration : Vector3.zero;

        public static Vector3 SphereDrag(Vector3 relativeVelocity, float radius, float density, float viscosity)
        {
            float speed = relativeVelocity.magnitude;
            if (speed < .0000001f) return Vector3.zero;
            float reynolds = density * speed * (2 * radius) / viscosity;
            // Schiller–Naumann below Re=1000; Newton regime above (outside drag crisis).
            float magnitude = reynolds < 1000
                ? 6 * Mathf.PI * viscosity * radius * speed * (1 + .15f * Mathf.Pow(reynolds, .687f))
                : .5f * density * .44f * Mathf.PI * radius * radius * speed * speed;
            return -relativeVelocity * (magnitude / speed);
        }

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
            Quaternion rotation = container != null ? container.rotation : transform.rotation;
            Vector3 p = LocalPoint(worldPosition);
            Vector3 relative = Quaternion.Inverse(rotation) * Vector3.Cross(fluidSpin - boxSpin, rotation * p);
            // Enforce zero relative normal flow at each closed box face. This bounded
            // interpolation is a bulk-flow cue, not an incompressible pressure solve.
            for (int axis = 0; axis < 3; axis++)
                relative[axis] *= Mathf.Clamp01(1 - p[axis] * p[axis] / (HalfSize[axis] * HalfSize[axis]));
            return boxVelocity + Vector3.Cross(boxSpin, rotation * p) + rotation * relative;
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
            BuoyancyForce = DragForce = RelativeVelocity = Vector3.zero;
            SubmergedFraction = ball != null ? Immersion(ball.Body.position, ball.Profile.Radius) : 0;
        }
    }
}
