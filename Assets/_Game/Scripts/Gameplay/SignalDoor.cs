using GravityBox.Foundation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    public sealed class SignalDoor : MonoBehaviour, IResettable, IMechanism
    {
        public string Channel = "gate-a";
        public Collider Blocker;
        public Transform Visual;
        public Vector3 OpenOffset = new Vector3(0, 1.2f, 0);
        private Vector3 closedPosition;
        private MechanismSignals signals;
        public bool IsActive { get; private set; }

        public void Bind(MechanismSignals bus)
        {
            if (signals != null) signals.Changed -= OnSignal;
            signals = bus;
            signals.Changed += OnSignal;
        }
        public void CaptureInitialState() { if (Visual != null) closedPosition = Visual.localPosition; }
        public void ResetState()
        {
            IsActive = false;
            if (Blocker != null) Blocker.enabled = true;
            if (Visual != null) Visual.localPosition = closedPosition;
        }
        public void SetActive(bool active)
        {
            IsActive = active;
            if (Blocker != null) Blocker.enabled = !active;
        }
        private void Update()
        {
            if (Visual == null) return;
            Vector3 target = closedPosition + (IsActive ? OpenOffset : Vector3.zero);
            Visual.localPosition = Vector3.MoveTowards(Visual.localPosition, target, Time.deltaTime * 5f);
        }
        private void OnSignal(string channel, bool value) { if (channel == Channel) SetActive(value); }
        private void OnDestroy() { if (signals != null) signals.Changed -= OnSignal; }
    }
}
