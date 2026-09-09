using System.Collections.Generic;
using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    // A real spring-loaded contact surface. Proximity alone cannot press it.
    [RequireComponent(typeof(GravitySliderGuide))]
    public sealed class PressurePlunger : MonoBehaviour, IResettable
    {
        public float Spring = 18;
        public float Damper = 1.2f;
        public float ActivationTravel = .003f;
        public GravitySliderGuide Guide;
        private readonly HashSet<BallController> contacts = new HashSet<BallController>();
        private IReadOnlyList<BallController> balls;
        public BallController Operator
        {
            get
            {
                if (Guide.Displacement < ActivationTravel) return null;
                foreach (BallController ball in contacts)
                    if (ball != null && !ball.IsCaptured && ball.gameObject.activeInHierarchy) return ball;
                return null;
            }
        }
        public bool Pressed => Operator != null;
        public void Bind(IReadOnlyList<BallController> targets) { balls = targets; contacts.Clear(); }
        public void StepSpring()
        {
            Vector3 axis = Guide.Joint.connectedBody.rotation * Guide.SlideAxisInBox;
            float force = -Spring * Guide.Displacement - Damper * Guide.TravelSpeed;
            Guide.Body.AddForce(axis * force, ForceMode.Force);
        }
        public void CaptureInitialState() { }
        public void ResetState() => contacts.Clear();
        private void OnCollisionEnter(Collision collision) => Observe(collision);
        private void OnCollisionStay(Collision collision) => Observe(collision);
        private void Observe(Collision collision)
        {
            if (balls == null) return;
            foreach (BallController ball in balls)
                if (ball != null && collision.rigidbody == ball.Body) { contacts.Add(ball); return; }
        }
        private void OnCollisionExit(Collision collision)
        {
            if (balls == null) return;
            foreach (BallController ball in balls)
                if (ball != null && collision.rigidbody == ball.Body) contacts.Remove(ball);
        }
        private void OnDisable() => contacts.Clear();
    }
}
