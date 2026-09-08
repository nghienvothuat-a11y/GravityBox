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
        private BallController ball;
        public bool IsActive { get; private set; }
        public event Action Activated;

        public void Bind(MechanismSignals bus, BallController target) { signals = bus; ball = target; }
        public void CaptureInitialState() { if (Visual != null) initialScale = Visual.localScale; }
        public void ResetState()
        {
            IsActive = false;
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
        private void OnTriggerEnter(Collider other) { if (IsBall(other)) SetActive(true); }
        private void OnTriggerStay(Collider other) { if (IsBall(other)) SetActive(true); }
        private void OnTriggerExit(Collider other) { if (!Latch && IsBall(other)) SetActive(false); }
        private bool IsBall(Collider other) => ball != null && other.attachedRigidbody == ball.Body && !ball.IsCaptured;
    }
}
