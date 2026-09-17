using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>A single physical translational degree of freedom with a brake and dry resistance.</summary>
    public sealed class COgheRailSlider : COgheMechanism
    {
        public Rigidbody Body;
        public Transform Frame;
        public ConfigurableJoint Joint;
        public Vector3 Start, Axis = Vector3.up;
        public float Travel = .12f, InitialTravel, Resistance = .015f, Damping = .08f;
        public bool Gravity = true, Locked, LatchAtEnd, LatchAtStart;
        public bool Latched { get; private set; }
        public float Effort { get; private set; }
        public float Position => Mathf.Clamp(Vector3.Dot(Frame.InverseTransformPoint(Body.position) - Start, Axis.normalized), 0, Travel);
        public float Fraction => Travel > 0 ? Position / Travel : 0;
        public Vector3 WorldAxis => Frame.TransformDirection(Axis.normalized);
        public bool AtEnd => Position >= Travel - .0018f;
        private Vector3 pendingEffort;
        private bool brakeSet;
        private float brakePosition;
        private bool lowerCatch;

        public void ApplyEffort(Vector3 force) { pendingEffort += Vector3.Project(force, WorldAxis); }
        public void ReleaseLatch() { Latched = false; }
        public override void ResetMechanism(VenomCampaign game)
        {
            pendingEffort = Vector3.zero; Effort = 0; Latched = false; brakeSet = false; lowerCatch = false;
            Body.position = Frame.TransformPoint(Start + Axis.normalized * InitialTravel);
            Body.rotation = Frame.rotation;
            Body.linearVelocity = Body.angularVelocity = Vector3.zero;
            SetBrake(Locked);
        }
        private void SetBrake(bool enabled)
        {
            if (enabled && !brakeSet) brakePosition = Position;
            Joint.connectedAnchor = Start + Axis.normalized * (enabled ? brakePosition : Travel * .5f);
            Joint.xMotion = enabled ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Limited;
            brakeSet = enabled;
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            if (LatchAtEnd && AtEnd) { Latched = true; lowerCatch = false; }
            if (LatchAtStart && Position <= .0018f) { Latched = true; lowerCatch = true; }
            // A deliberate reverse pull releases a terminal catch. Gates keep their separate brake.
            Effort = Vector3.Dot(pendingEffort, WorldAxis);
            if (Latched && (lowerCatch ? Effort > Resistance + .004f : Effort < -Resistance - .004f)) Latched = false;
            SetBrake(Locked || Latched);
            Vector3 relative = Body.linearVelocity - Frame.GetComponent<Rigidbody>().GetPointVelocity(Body.position);
            float speed = Vector3.Dot(relative, WorldAxis);
            float gravity = Gravity ? Vector3.Dot(Vector3.down * 9.81f * Body.mass, WorldAxis) : 0;
            // Campaign applies gravity to props. Cancel it here and apply exactly one axial force.
            if (GetComponent<VenomMovableProp>() != null) Body.AddForce(Vector3.up * 9.81f, ForceMode.Acceleration);
            float drive = Effort + gravity;
            float friction = Mathf.Abs(speed) < .002f ? Mathf.Clamp(drive, -Resistance, Resistance) : Mathf.Sign(speed) * Resistance;
            if (!Locked && !Latched) Body.AddForce(WorldAxis * (drive - friction - speed * Damping));
            pendingEffort = Vector3.zero;
        }
    }
}
