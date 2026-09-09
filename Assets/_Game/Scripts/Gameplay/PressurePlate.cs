using System;
using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    public sealed class PressurePlate : MonoBehaviour, IResettable, IMechanism
    {
        public string Channel = "gate-a";
        public bool Latch = true;
        public Transform Visual;
        private Vector3 initialScale;
        private MechanismSignals signals;
        private System.Collections.Generic.IReadOnlyList<BallController> balls;
        private readonly System.Collections.Generic.HashSet<Rigidbody> occupants = new System.Collections.Generic.HashSet<Rigidbody>();
        public bool IsActive { get; private set; }
        public event Action Activated;

        public void Bind(MechanismSignals bus, BallController target) => Bind(bus, new[] { target });
        public void Bind(MechanismSignals bus, System.Collections.Generic.IReadOnlyList<BallController> targets)
        { signals = bus; balls = targets; occupants.Clear(); }
        public void CaptureInitialState() { if (Visual != null) initialScale = Visual.localScale; }
        public void ResetState()
        {
            IsActive = false; occupants.Clear();
            signals?.Set(Channel, false);
            if (Visual != null) Visual.localScale = initialScale;
        }
        public void SetActive(bool active)
        {
            if (active == IsActive) return;
            IsActive = active;
            signals?.Set(Channel, active);
            if (Visual != null) Visual.localScale = active ? initialScale * 0.8f : initialScale;
            if (active) Activated?.Invoke();
        }
        private void OnTriggerEnter(Collider other) => OnTriggerStay(other);
        private void OnTriggerStay(Collider other) { if (IsBall(other)) { occupants.Add(other.attachedRigidbody); SetActive(true); } }
        private void OnTriggerExit(Collider other) { occupants.Remove(other.attachedRigidbody); if (!Latch && occupants.Count == 0) SetActive(false); }
        private bool IsBall(Collider other)
        {
            if (balls == null) return false;
            foreach (BallController ball in balls)
                if (ball != null && other.attachedRigidbody == ball.Body && !ball.IsCaptured) return true;
            return false;
        }
    }
}
