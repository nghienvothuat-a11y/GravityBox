using GravityBox.Foundation;
using UnityEngine;

namespace GravityBox.Simulation
{
    // Springs only return the rack/pawl. Ball-to-rack and pawl-to-wheel contacts
    // supply all forward work. A quantised unilateral hinge stop models the
    // retaining pawl: it permits backlash but cannot rotate the wheel forwards.
    public sealed class MemoryRatchetAssembly : MonoBehaviour, IForceProvider, IForceStepProvider, IResettable
    {
        public GravitySliderGuide Rack;
        public PhysicalHinge Cam;
        public PhysicalHinge DrivePawl;
        public float ToothDegrees=30f;
        public float ReturnSpring=12f;
        public float ReturnDamping=.18f;
        [Range(1,3)] public int MaximumTeeth=3;
        [Range(0,90)] public float PassageOpenDegrees=86;
        public int RetainedTeeth { get; private set; }
        public bool PassageAligned => Cam!=null && Cam.Angle>=PassageOpenDegrees;
        private JointLimits initialLimits;
        private int permittedTooth=1;

        public void CaptureInitialState() { initialLimits=Cam.Joint.limits; }
        public void ResetState()
        { RetainedTeeth=0;permittedTooth=1;Cam.Joint.limits=initialLimits; }
        public void PrepareStep(float dt)
        {
            if(dt<=0 || !isActiveAndEnabled || Rack==null || Cam==null || Rack.Body.isKinematic) return;
            Rigidbody box=Rack.Joint.connectedBody;
            Vector3 axis=box.rotation*Rack.SlideAxisInBox;
            float extension=Mathf.Max(0,Rack.Displacement);
            Rack.Body.AddForce(axis*(-ReturnSpring*extension-ReturnDamping*Rack.TravelSpeed),ForceMode.Force);
            // Drive finger can fold on its return stroke and rests against a real
            // hinge limit on its power stroke. This torsional spring stores energy.
            if(DrivePawl!=null && !DrivePawl.Body.isKinematic)
            {
                Vector3 pawlAxis=DrivePawl.Body.rotation*DrivePawl.Joint.axis;
                float speed=Vector3.Dot(DrivePawl.Body.angularVelocity-Rack.Body.angularVelocity,pawlAxis);
                DrivePawl.Body.AddTorque(pawlAxis*(-.004f*DrivePawl.Angle*Mathf.Deg2Rad-.00006f*speed),ForceMode.Force);
            }
            JointLimits limits=Cam.Joint.limits;
            int maximum=Mathf.Clamp(MaximumTeeth,1,3);
            int reached=Mathf.Clamp(Mathf.FloorToInt((Cam.Angle-.10f)/ToothDegrees),0,maximum);
            bool changed=false;
            if(reached>RetainedTeeth)
            {
                RetainedTeeth=reached;
                limits.min=RetainedTeeth*ToothDegrees-.8f; // Clearance behind an already passed tooth.
                changed=true;
            }
            // A two-pawl escapement arrests the NEXT tooth even after a hard
            // impact. Its keeper can withdraw only after the physical rack has
            // returned. Releasing this limit adds no torque or angular impulse.
            if(RetainedTeeth==permittedTooth && permittedTooth<maximum && Rack.Displacement<.0035f)
            {
                permittedTooth++;
                limits.max=permittedTooth*ToothDegrees+1.5f;
                changed=true;
            }
            if(changed) Cam.Joint.limits=limits;
        }
        public Vector3 GetAcceleration(IPhysicsAffectable target,EnvironmentProfile environment)=>Vector3.zero;

    }
}
