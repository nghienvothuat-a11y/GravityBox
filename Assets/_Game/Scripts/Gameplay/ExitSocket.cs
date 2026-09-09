using System;
using System.Collections.Generic;
using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    // Local +Z points out of the box. Geometry and this contract share the same aperture.
    public sealed partial class ExitSocket : MonoBehaviour, IResettable
    {
        [Min(0.001f)] public float ApertureRadius = 0.023f;
        [Min(0.0001f)] public float WallHalfDepth = 0.003f;
        public string RequiredChannel;
        private sealed class Passage
        {
            public BallController Ball;
            public Vector3 Previous, AssistAcceleration;
            public bool Traversing, Clearing, Exited, AssistActive;
        }
        private readonly List<Passage> passages = new List<Passage>();
        private BallController ball => passages.Count == 0 ? null : passages[0].Ball;
        private MechanismSignals signals;

        public bool Accepting { get; set; } = true;
        public int EscapedCount { get; private set; }
        public int BallCount => passages.Count;
        public bool HasExited => BallCount > 0 && EscapedCount == BallCount;
        public bool HasBallExited(BallController target) => Find(target)?.Exited ?? false;
        public event Action<BallController> BallExited;
        public BallController LastEscapedBall { get; private set; }
        private Passage Find(BallController target)
        { foreach (Passage p in passages) if (p.Ball == target) return p; return null; }
        public float CompletionMargin => ball == null ? 0 : ball.Profile.Radius * 0.035f;
        public float ClearanceTolerance => ball == null ? 0 : ball.Profile.Radius * 0.01f;
        public bool IsUnlocked => string.IsNullOrEmpty(RequiredChannel) || (signals != null && signals.Read(RequiredChannel));
        public event Action Exited;

        public void Bind(BallController target, MechanismSignals bus) => Bind(new[] { target }, bus);
        public void Bind(IReadOnlyList<BallController> targets, MechanismSignals bus)
        {
            passages.Clear();
            foreach (BallController target in targets)
                if (target != null && Find(target) == null) passages.Add(new Passage { Ball = target });
            signals = bus;
            assistRoot = GetComponentInParent<Rigidbody>();
            ResetState();
            BeginTracking();
        }
        public void CaptureInitialState() { }
        public void ResetState()
        {
            EscapedCount = 0; Accepting = true; LastEscapedBall = null;
            foreach (Passage p in passages) p.Exited = p.Traversing = p.Clearing = false;
            ResetAssist();
        }
        public void BeginTracking()
        {
            foreach (Passage p in passages)
                p.Previous = transform.InverseTransformPoint(p.Ball.Body.position);
        }

        // Also called after manual Physics.Simulate in tests and before out-of-bounds checks.
        // Sweeping between samples prevents a fast ball from skipping the opening.
        public void EvaluateTraversal()
        {
            foreach (Passage p in passages) EvaluateTraversal(p);
        }

        private void EvaluateTraversal(Passage p)
        {
            BallController ball = p.Ball;
            if (ball == null) return;
            Vector3 current = transform.InverseTransformPoint(ball.Body.position);
            Vector3 from = p.Previous;
            p.Previous = current;
            if (!Accepting || p.Exited || ball.IsCaptured || !IsUnlocked)
            {
                p.Traversing = p.Clearing = false;
                return;
            }
            float radius = ball.Profile.Radius;
            float inner = -WallHalfDepth;
            float outer = WallHalfDepth;
            if (current.z <= inner) { p.Traversing = p.Clearing = false; return; }
            if (from.z <= inner && current.z > inner)
                p.Traversing = Fits(AtDepth(from, current, inner), radius);
            if (p.Traversing)
            {
                Vector3 end = current.z > outer ? AtDepth(from, current, outer) : current;
                if (!Fits(end, radius)) { p.Traversing = p.Clearing = false; return; }
                if (current.z > outer) { p.Traversing = false; p.Clearing = true; }
            }
            if (p.Clearing && current.z < outer) { p.Clearing = false; p.Traversing = Fits(current, radius); }
            if (!p.Clearing || current.z < outer + radius + CompletionMargin) return;
            p.Exited = true;
            p.AssistActive = false; p.AssistAcceleration = Vector3.zero;
            EscapedCount++; LastEscapedBall = ball;
            BallExited?.Invoke(ball);
            // Keep the Rigidbody dynamic, at its actual position and velocity, for the escape payoff.
            if (HasExited) Exited?.Invoke();
        }

        private bool Fits(Vector3 point, float radius)
        {
            float clearance = Mathf.Max(0, ApertureRadius - radius + ClearanceTolerance);
            return point.x * point.x + point.y * point.y <= clearance * clearance;
        }
        private static Vector3 AtDepth(Vector3 a, Vector3 b, float z) => Vector3.LerpUnclamped(a, b, (z - a.z) / (b.z - a.z));
        private void FixedUpdate() => EvaluateTraversal();
    }
}
