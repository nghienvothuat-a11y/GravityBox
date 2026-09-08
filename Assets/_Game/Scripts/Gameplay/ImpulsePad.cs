using System;
using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    public sealed class ImpulsePad : MonoBehaviour, IResettable, IMechanism
    {
        public Vector3 LocalDirection = Vector3.up;
        [Min(0)] public float Speed = 5f;
        [Min(0)] public float Cooldown = 0.3f;
        public bool Spring;
        private float readyAt;
        private BallController ball;
        public bool IsActive { get; private set; } = true;
        public event Action Fired;

        public void Bind(BallController target) => ball = target;
        public void CaptureInitialState() { }
        public void ResetState() { readyAt = 0; IsActive = true; }
        public void SetActive(bool active) => IsActive = active;

        public bool TryFire(BallController target)
        {
            if (!IsActive || target == null || target.IsCaptured || Time.time < readyAt) return false;
            readyAt = Time.time + Cooldown;
            Vector3 direction = transform.TransformDirection(LocalDirection).normalized;
            float currentAlongNormal = Vector3.Dot(target.Body.linearVelocity, direction);
            float desiredSpeed = Spring ? Mathf.Max(Speed, -currentAlongNormal * 0.95f) : Speed;
            target.Body.AddForce(direction * Mathf.Max(0, desiredSpeed - currentAlongNormal), ForceMode.VelocityChange);
            Fired?.Invoke();
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ball != null && other.attachedRigidbody == ball.Body) TryFire(ball);
        }
    }
}
