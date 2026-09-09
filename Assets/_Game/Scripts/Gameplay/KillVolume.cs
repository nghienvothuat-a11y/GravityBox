using System;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    public sealed class KillVolume : MonoBehaviour
    {
        private System.Collections.Generic.IReadOnlyList<BallController> balls;
        public event Action Hit;
        public void Bind(BallController target) => Bind(new[] { target });
        public void Bind(System.Collections.Generic.IReadOnlyList<BallController> targets) => balls = targets;
        private void OnTriggerEnter(Collider other)
        {
            if (balls == null) return;
            foreach (BallController ball in balls)
                if (ball != null && !ball.IsCaptured && other.attachedRigidbody == ball.Body) { Hit?.Invoke(); break; }
        }
    }
}
