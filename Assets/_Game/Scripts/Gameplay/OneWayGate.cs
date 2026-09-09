using System.Collections.Generic;
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
        private sealed class Passage { public BallController Ball; public Collider Collider; public bool Open; }
        private readonly List<Passage> passages = new List<Passage>();
        public bool IsActive { get { foreach (Passage p in passages) if (p.Open) return true; return false; } }
        public void Bind(BallController target) => Bind(new[] { target });
        public void Bind(IReadOnlyList<BallController> targets)
        {
            SetActive(false); passages.Clear();
            foreach (BallController target in targets)
                passages.Add(new Passage { Ball = target, Collider = target.GetComponent<Collider>() });
        }
        public void CaptureInitialState() { }
        public void ResetState() => SetActive(false);
        public void SetActive(bool active) { foreach (Passage p in passages) SetActive(p, active); }
        private void SetActive(Passage p, bool active)
        {
            p.Open = active;
            if (p.Collider != null && Blocker != null) UnityEngine.Physics.IgnoreCollision(p.Collider, Blocker, active);
        }
        private void FixedUpdate() => Step();
        public void Step()
        {
            foreach (Passage p in passages)
            {
                if (p.Ball == null || p.Ball.IsCaptured) continue;
                float side = Vector3.Dot(p.Ball.Body.position - transform.position, transform.TransformDirection(LocalPassDirection).normalized);
                // Each sphere retains its own clearance; one cannot open collision for its partner.
                if (side < -Clearance) SetActive(p, true);
                else if (side > Clearance) SetActive(p, false);
            }
        }
        private void OnDestroy() => SetActive(false);
    }
}
