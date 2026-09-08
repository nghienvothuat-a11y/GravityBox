using System;
using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    // Local +Z points out of the box. Geometry and this contract share the same aperture.
    public sealed class ExitSocket : MonoBehaviour, IResettable
    {
        [Min(0.1f)] public float ApertureRadius = 0.78f;
        [Min(0.01f)] public float WallHalfDepth = 0.09f;
        public string RequiredChannel;
        private BallController ball;
        private MechanismSignals signals;
        private Vector3 previous;
        private bool traversing, clearing;
        public bool Accepting { get; set; } = true;
        public bool HasExited { get; private set; }
        public bool IsUnlocked => string.IsNullOrEmpty(RequiredChannel) || (signals != null && signals.Read(RequiredChannel));
        public event Action Exited;

        public void Bind(BallController target, MechanismSignals bus)
        {
            ball = target;
            signals = bus;
            BeginTracking();
        }
        public void CaptureInitialState() { }
        public void ResetState() { HasExited = false; Accepting = true; traversing = clearing = false; }
        public void BeginTracking() { if (ball != null) previous = transform.InverseTransformPoint(ball.Body.position); }

        // Also called after manual Physics.Simulate in tests and before out-of-bounds checks.
        // Sweeping between samples prevents a fast ball from skipping the opening.
        public void EvaluateTraversal()
        {
            if (ball == null) return;
            Vector3 current = transform.InverseTransformPoint(ball.Body.position);
            Vector3 from = previous;
            previous = current;
            if (!Accepting || HasExited || ball.IsCaptured || !IsUnlocked)
            {
                traversing = clearing = false;
                return;
            }
            float radius = ball.Profile.Radius;
            float inner = -WallHalfDepth;
            float outer = WallHalfDepth;
            if (current.z <= inner) { traversing = clearing = false; return; }
            if (from.z <= inner && current.z > inner)
                traversing = Fits(AtDepth(from, current, inner), radius);
            if (traversing)
            {
                Vector3 end = current.z > outer ? AtDepth(from, current, outer) : current;
                if (!Fits(end, radius)) { traversing = clearing = false; return; }
                if (current.z > outer) { traversing = false; clearing = true; }
            }
            if (clearing && current.z < outer) { clearing = false; traversing = Fits(current, radius); }
            if (!clearing || current.z < outer + radius + 0.02f) return;
            HasExited = true;
            // Keep the Rigidbody dynamic, at its actual position and velocity, for the escape payoff.
            Exited?.Invoke();
        }

        private bool Fits(Vector3 point, float radius)
        {
            float clearance = Mathf.Max(0, ApertureRadius - radius + 0.015f);
            return point.x * point.x + point.y * point.y <= clearance * clearance;
        }
        private static Vector3 AtDepth(Vector3 a, Vector3 b, float z) => Vector3.LerpUnclamped(a, b, (z - a.z) / (b.z - a.z));
        private void FixedUpdate() => EvaluateTraversal();
    }
}
