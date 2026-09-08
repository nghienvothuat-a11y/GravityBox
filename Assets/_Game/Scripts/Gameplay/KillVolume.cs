using System;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    public sealed class KillVolume : MonoBehaviour
    {
        private BallController ball;
        public event Action Hit;
        public void Bind(BallController target) => ball = target;
        private void OnTriggerEnter(Collider other)
        {
            if (ball != null && !ball.IsCaptured && other.attachedRigidbody == ball.Body) Hit?.Invoke();
        }
    }
}
