using System;
using UnityEngine;

namespace GravityBox.Simulation
{
    // Telemetry for an ideal mechanical rail. The Rigidbody and ConfigurableJoint perform all motion.
    // PhysicalProp owns detachment, registration with world gravity, and reset of the moving body.
    [RequireComponent(typeof(PhysicalProp), typeof(ConfigurableJoint))]
    public sealed class GravitySliderGuide : MonoBehaviour
    {
        [SerializeField] private ConfigurableJoint sliderJoint;
        [SerializeField] private Vector3 closedAnchorInBox = Vector3.zero;
        [SerializeField] private Vector3 slideAxisInBox = Vector3.forward;
        [SerializeField, Min(0.001f)] private float travel = 0.120f;
        [SerializeField, Min(0)] private float clearanceDisplacement = 0.078f;
        private Rigidbody body;

        public Rigidbody Body => body != null ? body : body = GetComponent<Rigidbody>();
        public ConfigurableJoint Joint => sliderJoint != null ? sliderJoint : sliderJoint = GetComponent<ConfigurableJoint>();
        public float Travel => travel;
        public float ClearanceDisplacement => clearanceDisplacement;
        public Vector3 SlideAxisInBox => slideAxisInBox;

        public float Displacement
        {
            get
            {
                ConfigurableJoint joint = Joint;
                Rigidbody box = joint != null ? joint.connectedBody : null;
                if (box == null) return 0;
                Vector3 anchor = Body.position + Body.rotation * joint.anchor;
                Vector3 closed = box.position + box.rotation * closedAnchorInBox;
                return Vector3.Dot(anchor - closed, box.rotation * slideAxisInBox);
            }
        }

        public float OpenRatio => Mathf.Clamp01(Displacement / travel);
        // This reports actual geometric clearance; it never enables or disables collision.
        public bool IsPassageClear => Joint != null && Joint.connectedBody != null && Displacement >= clearanceDisplacement;

        public float TravelSpeed
        {
            get
            {
                ConfigurableJoint joint = Joint;
                Rigidbody box = joint != null ? joint.connectedBody : null;
                if (box == null) return 0;
                Vector3 anchor = Body.position + Body.rotation * joint.anchor;
                Vector3 relativeVelocity = Body.GetPointVelocity(anchor) - box.GetPointVelocity(anchor);
                return Vector3.Dot(relativeVelocity, box.rotation * slideAxisInBox);
            }
        }

        // Authoring metadata only. There is deliberately no Update/FixedUpdate, motor, unlock or reset state here.
        public void Configure(ConfigurableJoint joint, Vector3 closedAnchor, Vector3 axis, float stroke, float clearance)
        {
            if (joint == null || joint.connectedBody == null) throw new ArgumentException("A slider requires a joint connected to its box.", nameof(joint));
            if (axis.sqrMagnitude < 0.0001f || stroke <= 0 || clearance < 0 || clearance > stroke)
                throw new ArgumentException("Slider axis, stroke and passage clearance must describe a valid rail.");
            sliderJoint = joint;
            closedAnchorInBox = closedAnchor;
            slideAxisInBox = axis.normalized;
            travel = stroke;
            clearanceDisplacement = clearance;
        }
    }
}
