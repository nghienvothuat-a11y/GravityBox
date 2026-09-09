using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    /// <summary>
    /// An ideal retaining pawl: a designated rigid stop must actually contact its
    /// moving member near the seating angle. Engagement removes one degree of
    /// freedom; it never supplies motion, teleports a part, or controls a ball.
    /// </summary>
    [RequireComponent(typeof(PhysicalHinge))]
    public sealed class ContactSeatLatch : MonoBehaviour, IResettable
    {
        public PhysicalHinge Hinge;
        public Collider Receiver;
        public float SeatingAngle;
        [Min(.1f)] public float SeatingTolerance = 3;
        public bool Armed = true;
        public bool Latched { get; private set; }
        public float EngagedAngle { get; private set; }
        private JointLimits initialLimits;
        private bool captured;
        private bool initialArmed;

        public void Configure(PhysicalHinge hinge, Collider receiver, float angle, float tolerance = 3)
        {
            Hinge = hinge; Receiver = receiver; SeatingAngle = angle; SeatingTolerance = tolerance;
            CaptureInitialState();
        }

        public void CaptureInitialState()
        {
            if (captured || Hinge == null || Hinge.Joint == null) return;
            initialLimits = Hinge.Joint.limits; initialArmed = Armed; captured = true;
        }

        public void ResetState()
        {
            Latched = false; EngagedAngle = 0;
            if (captured) Armed = initialArmed;
            if (captured && Hinge != null && Hinge.Joint != null)
            { Hinge.Joint.limits = initialLimits; Hinge.Joint.useLimits = true; }
        }

        private void OnCollisionEnter(Collision collision) => TryEngage(collision);
        private void OnCollisionStay(Collision collision) => TryEngage(collision);

        private void TryEngage(Collision collision)
        {
            if (!Armed || Latched || Receiver == null || collision.collider != Receiver || Hinge == null) return;
            float angle = Hinge.Angle;
            if (Mathf.Abs(Mathf.DeltaAngle(angle, SeatingAngle)) > SeatingTolerance) return;
            // Continuous speculative collision can report a future contact while
            // the member is still visibly above its seat. A retaining pawl only
            // catches once real contact has dissipated the closing motion.
            if (Mathf.Abs(Hinge.AngularSpeed) > 12) return;
            bool seated=false;
            for(int i=0;i<collision.contactCount;i++)
                if(collision.GetContact(i).separation<=.0006f){seated=true;break;}
            if(!seated)return;
            CaptureInitialState();
            // Keep the contact-resolved pose. A small angular clearance avoids an
            // overconstrained stop, as a real pawl has finite manufacturing play.
            var locked = initialLimits;
            locked.min = angle - .1f; locked.max = angle + .1f;
            locked.bounciness = 0; locked.contactDistance = 0;
            Hinge.Joint.limits = locked; Hinge.Joint.useLimits = true;
            EngagedAngle = angle; Latched = true;
        }
    }
}
