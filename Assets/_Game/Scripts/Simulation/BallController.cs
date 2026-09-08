using System;
using GravityBox.Foundation;
using UnityEngine;

namespace GravityBox.Simulation
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class BallController : MonoBehaviour, IResettable, IPhysicsAffectable
    {
        [SerializeField] private BallPhysicsProfile profile;
        private Rigidbody body;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 initialVelocity;
        private TrailRenderer trail;
        public Rigidbody Body => body != null ? body : body = GetComponent<Rigidbody>();
        public BallPhysicsProfile Profile => profile;
        public bool IsCaptured { get; private set; }
        public bool HasContact { get; private set; }
        public event Action<float> Impact;

        public void Configure(BallPhysicsProfile configuration, Vector3 velocity, bool showTrail)
        {
            profile = configuration;
            Body.mass = profile.Mass;
            Body.useGravity = false;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            Body.collisionDetectionMode = profile.CollisionDetection;
            Body.maxDepenetrationVelocity = profile.MaxDepenetrationSpeed;
            var sphere = GetComponent<SphereCollider>();
            sphere.radius = profile.Radius;
            sphere.sharedMaterial = profile.ContactMaterial;
            initialVelocity = velocity;
            trail = GetComponent<TrailRenderer>();
            if (trail != null) trail.emitting = showTrail;
            Body.linearVelocity = velocity;
        }

        public void CaptureInitialState()
        {
            initialPosition = Body.position;
            initialRotation = Body.rotation;
        }

        public void ResetState()
        {
            Body.isKinematic = false;
            Body.position = initialPosition;
            Body.rotation = initialRotation;
            Body.linearVelocity = initialVelocity;
            Body.angularVelocity = Vector3.zero;
            IsCaptured = false;
            HasContact = false;
            if (trail != null) trail.Clear();
            Body.WakeUp();
        }

        public void Capture(Vector3 position)
        {
            if (IsCaptured) return;
            IsCaptured = true;
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
            Body.isKinematic = true;
            Body.position = position;
            if (trail != null) trail.Clear();
        }

        private void OnCollisionEnter(Collision collision)
        {
            HasContact = true;
            Impact?.Invoke(collision.impulse.magnitude);
        }
        private void OnCollisionStay(Collision collision) => HasContact = true;
        private void OnCollisionExit(Collision collision) => HasContact = false;
    }
}
