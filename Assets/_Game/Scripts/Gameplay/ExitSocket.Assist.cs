using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    // Deliberate end-of-level assistance. The ordinary full-sphere exit contract
    // remains in ExitSocket.cs; this motor only adds force to a dynamic Rigidbody.
    public sealed partial class ExitSocket : IForceProvider, IForceStepProvider
    {
        public bool AssistEnabled = true;
        [Min(.001f)] public float AssistRadius = .04f;
        [Min(.01f)] public float AssistSpeed = .24f;
        [Min(.01f)] public float AssistEjectionSpeed = .65f;
        [Min(1)] public float AssistMaxAcceleration = 45;
        public bool AssistActive { get; private set; }
        public Vector3 AssistAcceleration { get; private set; }
        private Rigidbody assistRoot;
        private readonly RaycastHit[] assistHits = new RaycastHit[16];

        public void PrepareStep(float dt)
        {
            AssistAcceleration = Vector3.zero;
            if (dt <= 0 || !CanAssist()) { AssistActive = false; return; }
            float radius = ball.Profile.Radius;
            float clearance = ApertureRadius - radius - ClearanceTolerance;
            if (clearance <= 0) { AssistActive = false; return; }
            Vector3 p = transform.InverseTransformPoint(ball.Body.position);
            Vector3 align = new Vector3(0, 0, -WallHalfDepth - radius - .001f);
            Vector3 outside = new Vector3(0, 0, WallHalfDepth + radius + CompletionMargin + .02f);
            if (!AssistActive)
            {
                // Start from inside/partly in the aperture, never recapture a ball
                // arriving from outside. Sweeps prevent attraction through a maze wall
                // or a physical shutter; triggers never form an invisible blocker.
                if (p.sqrMagnitude > AssistRadius * AssistRadius || p.z > WallHalfDepth
                    || !AssistPathClear(ball.Body.position, transform.TransformPoint(align), radius)
                    || !AssistPathClear(transform.TransformPoint(align), transform.TransformPoint(outside), radius)) return;
                AssistActive = true;
            }
            if (p.sqrMagnitude > AssistRadius * AssistRadius * 6.25f)
            { ResetAssist(); return; }
            float radial = new Vector2(p.x, p.y).magnitude;
            Vector3 goal = radial > clearance * .6f && p.z < -WallHalfDepth ? align : outside;
            Vector3 relative = ball.Body.linearVelocity
                - (assistRoot != null ? assistRoot.GetPointVelocity(ball.Body.position) : Vector3.zero);
            Vector3 desired = transform.TransformDirection(Vector3.ClampMagnitude((goal - p) * 60,
                goal == align ? AssistSpeed : AssistEjectionSpeed));
            if (p.z > WallHalfDepth + radius * .5f)
            {
                // A small outward lateral departure keeps an upward-facing opening
                // from dropping the released ball straight back into the same bore.
                Vector3 fromCentre = transform.position - (assistRoot != null ? assistRoot.position : Vector3.zero);
                Vector3 departure = Vector3.ProjectOnPlane(fromCentre, transform.forward).normalized;
                if (departure.sqrMagnitude < .1f) departure = transform.right;
                desired += departure * .35f;
            }
            // Exponential velocity response keeps the force bounded and timestep-aware.
            // Gravity, buoyancy, drag and contacts still run; no velocity/pose assignment.
            float response = (1 - Mathf.Exp(-180 * dt)) / dt;
            AssistAcceleration = Vector3.ClampMagnitude((desired - relative) * response, AssistMaxAcceleration);
        }

        public Vector3 GetAcceleration(IPhysicsAffectable target, EnvironmentProfile environment)
            => ReferenceEquals(target, ball) && CanAssist() ? AssistAcceleration : Vector3.zero;

        private bool CanAssist() => AssistEnabled && isActiveAndEnabled && Accepting && !HasExited
            && IsUnlocked && ball != null && !ball.IsCaptured && !ball.Body.isKinematic;

        private bool AssistPathClear(Vector3 from, Vector3 to, float radius)
        {
            Vector3 delta = to - from;
            if (delta.sqrMagnitude < 1e-10f) return true;
            int count = UnityEngine.Physics.SphereCastNonAlloc(from, radius * .97f, delta.normalized,
                assistHits, delta.magnitude, ~0, QueryTriggerInteraction.Ignore);
            if (count == assistHits.Length) return false;
            for (int i = 0; i < count; i++)
                if (assistHits[i].rigidbody != ball.Body) return false;
            return true;
        }

        private void ResetAssist() { AssistActive = false; AssistAcceleration = Vector3.zero; }
        private void OnDisable() => ResetAssist();
    }
}
