using System;
using System.Collections.Generic;
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
        private readonly Dictionary<int, SurfaceContact> contacts = new Dictionary<int, SurfaceContact>(8);
        private readonly Dictionary<int, SurfaceContact> collisionContacts = new Dictionary<int, SurfaceContact>(8);
        private readonly List<int> invalidContacts = new List<int>(8);
        private int contactStep;
        private struct SurfaceContact
        {
            public Collider Collider;
            public Rigidbody Body;
            public Vector3 Normal, Point;
            public float NormalImpulse;
            public int PointCount, LastSeenStep;
        }
        public Rigidbody Body => body != null ? body : body = GetComponent<Rigidbody>();
        public BallPhysicsProfile Profile => profile;
        public bool IsCaptured { get; private set; }
        public bool HasContact => contacts.Count > 0;
        public int ContactCount => contacts.Count;
        public float ContactSpeed { get; private set; }
        public float ContactSlipSpeed { get; private set; }
        public float ContactLoad { get; private set; }
        public Vector3 ContactNormal { get; private set; }
        public Vector3 ContactPoint { get; private set; }
        // SI units, N·s. Presentation scales this measured impulse for sound, never for motion.
        public event Action<float> Impact;

        public void Configure(BallPhysicsProfile configuration, Vector3 velocity, bool showTrail)
        {
            profile = configuration;
            Body.mass = profile.Mass;
            Body.useGravity = false;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            Body.collisionDetectionMode = profile.CollisionDetection;
            Body.maxDepenetrationVelocity = profile.MaxDepenetrationSpeed;
            Body.solverIterations = profile.SolverIterations;
            Body.solverVelocityIterations = profile.SolverVelocityIterations;
            Body.maxAngularVelocity = profile.MinimumAngularSpeedLimit;
            // Tiny marbles must not sleep before their slow roll is visibly finished.
            Body.sleepThreshold = 0.00001f;
            var sphere = GetComponent<SphereCollider>();
            sphere.radius = profile.Radius;
            sphere.center = Vector3.zero;
            sphere.contactOffset = profile.ContactOffset;
            sphere.sharedMaterial = profile.ContactMaterial;
            Body.centerOfMass = Vector3.zero;
            Body.inertiaTensorRotation = Quaternion.identity;
            Body.inertiaTensor = Vector3.one * profile.SolidSphereInertia;
            initialVelocity = velocity;
            trail = GetComponent<TrailRenderer>();
            if (trail != null) trail.emitting = showTrail;
            Body.linearVelocity = velocity;
            ClearContacts();
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
            ClearContacts();
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
            ClearContacts();
            if (trail != null) trail.Clear();
        }

        public void StepContactResistance(float dt)
        {
            if (dt <= 0 || IsCaptured || Body.isKinematic) return;
            contactStep++;
            PruneContacts();
            UpdateContactReadout(dt);
            if (profile == null || profile.RollingResistanceCoefficient <= 0 || Body.IsSleeping()) return;
            foreach (SurfaceContact contact in contacts.Values)
            {
                Vector3 spherePoint = Body.worldCenterOfMass - contact.Normal * profile.Radius;
                Vector3 surfaceVelocity = contact.Body != null ? contact.Body.GetPointVelocity(spherePoint) : Vector3.zero;
                // An impact can already be separating by the time callbacks arrive; do not damp its airborne spin.
                // A sphere's spin cannot separate it along its contact normal. The cached callback point
                // can lag the integrated centre by v*dt, so angular velocity at that old point is misleading.
                if (Vector3.Dot(Body.linearVelocity - surfaceVelocity, contact.Normal) > 0.001f) continue;
                Vector3 surfaceSpin = contact.Body != null ? contact.Body.angularVelocity : Vector3.zero;
                Vector3 rollingSpin = Vector3.ProjectOnPlane(Body.angularVelocity - surfaceSpin, contact.Normal);
                float speed = rollingSpin.magnitude;
                if (speed < 0.0001f) continue;
                float load = contact.NormalImpulse / dt;
                float rollingMoment = profile.RollingResistanceCoefficient * load * profile.Radius;
                // Bound the dissipative impulse at zero relative spin; resistance cannot reverse spin or add energy.
                Vector3 spinAxis = rollingSpin / speed;
                float inverseInertia = 1f / profile.SolidSphereInertia;
                if (contact.Body != null && !contact.Body.isKinematic)
                {
                    Vector3 localAxis = Quaternion.Inverse(contact.Body.rotation * contact.Body.inertiaTensorRotation) * spinAxis;
                    Vector3 tensor = contact.Body.inertiaTensor;
                    inverseInertia += localAxis.x * localAxis.x / Mathf.Max(tensor.x, 0.00000001f)
                                    + localAxis.y * localAxis.y / Mathf.Max(tensor.y, 0.00000001f)
                                    + localAxis.z * localAxis.z / Mathf.Max(tensor.z, 0.00000001f);
                }
                float stopMoment = speed / (inverseInertia * dt * contacts.Count);
                Vector3 torque = -rollingSpin / speed * Mathf.Min(rollingMoment, stopMoment);
                Body.AddTorque(torque, ForceMode.Force);
                if (contact.Body != null && !contact.Body.isKinematic) contact.Body.AddTorque(-torque, ForceMode.Force);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            RecordContact(collision);
            Impact?.Invoke(collision.impulse.magnitude);
        }
        private void OnCollisionStay(Collision collision) => RecordContact(collision);
        private void OnCollisionExit(Collision collision)
        {
            contacts.Remove(collision.collider.GetInstanceID());
            UpdateContactReadout(Time.fixedDeltaTime);
        }

        private void RecordContact(Collision collision)
        {
            if (IsCaptured || collision.contactCount == 0) return;
            collisionContacts.Clear();
            for (int i = 0; i < collision.contactCount; i++)
            {
                UnityEngine.ContactPoint point = collision.GetContact(i);
                Collider surface = point.otherCollider;
                Vector3 normal = point.normal;
                if (surface.attachedRigidbody == Body) { surface = point.thisCollider; normal = -normal; }
                int key = surface.GetInstanceID();
                collisionContacts.TryGetValue(key, out SurfaceContact contact);
                contact.Collider = surface;
                contact.Body = surface.attachedRigidbody;
                contact.Normal += normal;
                contact.Point += point.point;
                contact.NormalImpulse += Mathf.Abs(Vector3.Dot(point.impulse, normal));
                contact.PointCount++;
                contact.LastSeenStep = contactStep;
                collisionContacts[key] = contact;
            }
            // One collision may contain floor and wall contacts from the same compound Rigidbody.
            foreach (var pair in collisionContacts)
            {
                SurfaceContact contact = pair.Value;
                contact.Normal.Normalize();
                contact.Point /= contact.PointCount;
                contacts[pair.Key] = contact;
            }
            UpdateContactReadout(Time.fixedDeltaTime);
        }

        private void PruneContacts()
        {
            invalidContacts.Clear();
            foreach (var pair in contacts)
                if (pair.Value.Collider == null || !pair.Value.Collider.enabled || !pair.Value.Collider.gameObject.activeInHierarchy
                    || (!Body.IsSleeping() && pair.Value.LastSeenStep < contactStep - 1))
                    invalidContacts.Add(pair.Key);
            foreach (int key in invalidContacts) contacts.Remove(key);
        }

        private void UpdateContactReadout(float dt)
        {
            ContactSpeed = ContactSlipSpeed = ContactLoad = 0;
            ContactNormal = ContactPoint = Vector3.zero;
            float largestLoad = -1;
            foreach (SurfaceContact contact in contacts.Values)
            {
                if (contact.Collider == null || !contact.Collider.enabled || !contact.Collider.gameObject.activeInHierarchy) continue;
                float load = contact.NormalImpulse / Mathf.Max(dt, 0.00001f);
                ContactLoad += load;
                if (load < largestLoad) continue;
                largestLoad = load;
                Vector3 spherePoint = Body.worldCenterOfMass - contact.Normal * (profile != null ? profile.Radius : 0);
                Vector3 surfaceVelocity = contact.Body != null ? contact.Body.GetPointVelocity(spherePoint) : Vector3.zero;
                ContactSpeed = Vector3.ProjectOnPlane(Body.linearVelocity - surfaceVelocity, contact.Normal).magnitude;
                ContactSlipSpeed = Vector3.ProjectOnPlane(Body.GetPointVelocity(spherePoint) - surfaceVelocity, contact.Normal).magnitude;
                ContactNormal = contact.Normal;
                ContactPoint = contact.Point;
            }
        }

        private void ClearContacts()
        {
            contacts.Clear();
            collisionContacts.Clear();
            contactStep = 0;
            ContactSpeed = ContactSlipSpeed = ContactLoad = 0;
            ContactNormal = ContactPoint = Vector3.zero;
        }

        private void OnDisable() => ClearContacts();
    }
}
