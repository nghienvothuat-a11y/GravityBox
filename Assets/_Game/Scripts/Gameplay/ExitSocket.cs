using System;
using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    public sealed class ExitSocket : MonoBehaviour, IResettable
    {
        public Transform CapturePoint;
        public string RequiredChannel;
        private BallController ball;
        private MechanismSignals signals;
        public bool Accepting { get; set; } = true;
        public bool HasCaptured { get; private set; }
        public event Action Captured;

        public void Bind(BallController target, MechanismSignals bus) { ball = target; signals = bus; }
        public void CaptureInitialState() { }
        public void ResetState() { HasCaptured = false; Accepting = true; }
        public bool TryCapture(BallController candidate)
        {
            if (!Accepting || HasCaptured || candidate != ball || candidate.IsCaptured) return false;
            if (!string.IsNullOrEmpty(RequiredChannel) && (signals == null || !signals.Read(RequiredChannel))) return false;
            HasCaptured = true;
            candidate.Capture(CapturePoint != null ? CapturePoint.position : transform.position);
            Captured?.Invoke();
            return true;
        }
        private void OnTriggerEnter(Collider other) { if (ball != null && other.attachedRigidbody == ball.Body) TryCapture(ball); }
        private void OnTriggerStay(Collider other) { if (ball != null && other.attachedRigidbody == ball.Body) TryCapture(ball); }
    }
}
