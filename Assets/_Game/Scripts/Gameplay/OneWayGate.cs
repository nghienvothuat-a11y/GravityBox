using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    [DefaultExecutionOrder(-20)]
    public sealed class OneWayGate : MonoBehaviour, IResettable, IMechanism
    {
        public Collider Blocker;
        public Vector3 LocalPassDirection = Vector3.right;
        public float Clearance = 0.4f;
        private BallController ball;
        private Collider ballCollider;
        public bool IsActive { get; private set; }

        public void Bind(BallController target)
        {
            ball = target;
            ballCollider = target.GetComponent<Collider>();
        }
        public void CaptureInitialState() { }
        public void ResetState() => SetActive(false);
        public void SetActive(bool active)
        {
            IsActive = active;
            if (ballCollider != null && Blocker != null) UnityEngine.Physics.IgnoreCollision(ballCollider, Blocker, active);
        }
        private void FixedUpdate() => Step();

        public void Step()
        {
            if (ball == null || ball.IsCaptured) return;
            float side = Vector3.Dot(ball.Body.position - transform.position, transform.TransformDirection(LocalPassDirection).normalized);
            // Hysteresis lets the whole sphere clear the plane before restoring the barrier.
            if (side < -Clearance) SetActive(true);
            else if (side > Clearance) SetActive(false);
        }
        private void OnDestroy() => SetActive(false);
    }
}
