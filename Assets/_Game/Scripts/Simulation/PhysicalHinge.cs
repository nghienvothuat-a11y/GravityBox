using GravityBox.Foundation;
using UnityEngine;

namespace GravityBox.Simulation
{
    // The native joint carries the load. Only viscous bearing friction is added;
    // angle, angular velocity and the connected body are never animated here.
    [RequireComponent(typeof(PhysicalProp), typeof(HingeJoint))]
    public sealed class PhysicalHinge : MonoBehaviour, IResettable, IForceProvider, IForceStepProvider
    {
        [SerializeField] private HingeJoint hinge;
        [SerializeField, Min(0)] private float bearingDamping = .00002f;
        private Rigidbody body;
        private JointLimits initialLimits;
        private bool initialUseLimits;
        private Quaternion referenceRotation;
        private bool captured;
        public Rigidbody Body => body != null ? body : body = GetComponent<Rigidbody>();
        public HingeJoint Joint => hinge != null ? hinge : hinge = GetComponent<HingeJoint>();
        public float Angle
        {
            get
            {
                if(!captured || Joint.connectedBody==null) return 0;
                Quaternion delta=Quaternion.Inverse(referenceRotation)*Quaternion.Inverse(Joint.connectedBody.rotation)*Body.rotation;
                float sine=Vector3.Dot(new Vector3(delta.x,delta.y,delta.z),Joint.axis.normalized);
                return Mathf.DeltaAngle(0,2*Mathf.Atan2(sine,delta.w)*Mathf.Rad2Deg);
            }
        }
        public float AngularSpeed => Joint.connectedBody==null ? 0 : Vector3.Dot(
            Body.angularVelocity-Joint.connectedBody.angularVelocity,Body.rotation*Joint.axis)*Mathf.Rad2Deg;
        public void Configure(HingeJoint joint, float damping = .00002f)
        { hinge = joint; bearingDamping = Mathf.Max(0, damping); }
        public void CaptureInitialState()
        {
            initialLimits = Joint.limits; initialUseLimits = Joint.useLimits;
            referenceRotation=Joint.connectedBody!=null ? Quaternion.Inverse(Joint.connectedBody.rotation)*Body.rotation : Body.rotation;
            captured=true;
        }
        public void ResetState()
        { Joint.limits = initialLimits; Joint.useLimits = initialUseLimits; }
        public void PrepareStep(float dt)
        {
            if (dt <= 0 || !isActiveAndEnabled || Body.isKinematic || Joint.connectedBody == null) return;
            Vector3 axis = Body.rotation * Joint.axis;
            float speed = Vector3.Dot(Body.angularVelocity - Joint.connectedBody.angularVelocity, axis);
            Body.AddTorque(-axis * speed * bearingDamping, ForceMode.Force);
        }
        public Vector3 GetAcceleration(IPhysicsAffectable target, EnvironmentProfile environment) => Vector3.zero;
    }
}
