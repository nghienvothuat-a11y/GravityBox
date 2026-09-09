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
        private readonly System.Collections.Generic.Dictionary<BallController, float> readyAt = new System.Collections.Generic.Dictionary<BallController, float>();
        private System.Collections.Generic.IReadOnlyList<BallController> balls;
        public bool IsActive { get; private set; } = true;
        public event Action Fired;

        public void Bind(BallController target) => Bind(new[] { target });
        public void Bind(System.Collections.Generic.IReadOnlyList<BallController> targets) { balls = targets; readyAt.Clear(); }
        public void CaptureInitialState() { }
        public void ResetState() { readyAt.Clear(); IsActive = true; }
        public void SetActive(bool active) => IsActive = active;

        public bool TryFire(BallController target)
        {
            if (!IsActive || target == null || target.IsCaptured || (readyAt.TryGetValue(target, out float time) && Time.time < time)) return false;
            readyAt[target] = Time.time + Cooldown;
            Vector3 direction = transform.TransformDirection(LocalDirection).normalized;
            float currentAlongNormal = Vector3.Dot(target.Body.linearVelocity, direction);
            float desiredSpeed = Spring ? Mathf.Max(Speed, -currentAlongNormal * 0.95f) : Speed;
            target.Body.AddForce(direction * Mathf.Max(0, desiredSpeed - currentAlongNormal), ForceMode.VelocityChange);
            Fired?.Invoke();
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (balls == null) return;
            foreach (BallController ball in balls)
                if (ball != null && other.attachedRigidbody == ball.Body) { TryFire(ball); break; }
        }
    }
}
