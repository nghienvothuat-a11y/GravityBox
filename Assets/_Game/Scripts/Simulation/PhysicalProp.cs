using GravityBox.Foundation;
using UnityEngine;

namespace GravityBox.Simulation
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PhysicalProp : MonoBehaviour, IResettable, IPhysicsAffectable
    {
        private Rigidbody body;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        public Rigidbody Body => body != null ? body : body = GetComponent<Rigidbody>();

        public void Initialize()
        {
            // Author relative to the level, then simulate independently in world space.
            transform.SetParent(null, true);
            Body.isKinematic = false;
            Body.useGravity = false;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            Body.maxDepenetrationVelocity = 3;
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
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
            Body.WakeUp();
        }
    }
}
