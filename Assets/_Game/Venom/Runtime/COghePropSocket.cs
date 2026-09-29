using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>A socket pawl for a loose block. It catches only a block that really rests inside its tolerance and
    /// is no longer held; grasping the block again releases it. The block is never moved into place.</summary>
    public sealed class COghePropSocket : COgheMechanism
    {
        public VenomMovableProp Prop;
        public Transform Socket;
        public Vector3 Tolerance = new Vector3(.016f, .012f, .016f);
        public Transform Pawl;
        public bool Seated { get; private set; }
        public override string Activity => null;
        private Vector3 pawlRest;
        private RigidbodyConstraints free;
        public override void InitializeMechanism(VenomCampaign game) { free = Prop.Body.constraints; if (Pawl != null) pawlRest = Pawl.localPosition; }
        public override void ResetMechanism(VenomCampaign game) { Seat(game, false); }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            Vector3 p = Socket.InverseTransformPoint(Prop.Body.position);
            bool inside = Mathf.Abs(p.x) <= Tolerance.x && Mathf.Abs(p.y) <= Tolerance.y && Mathf.Abs(p.z) <= Tolerance.z;
            bool held = game.HeldProp == Prop;
            if (!Seated && inside && !held && Prop.Body.linearVelocity.magnitude < .02f) Seat(game, true);
            else if (Seated && held) Seat(game, false);
        }
        private void Seat(VenomCampaign game, bool value)
        {
            if (Seated == value && Prop.Body.constraints == (value ? RigidbodyConstraints.FreezeAll : free)) return;
            Seated = value; Prop.Body.constraints = value ? RigidbodyConstraints.FreezeAll : free;
            if (Pawl != null) Pawl.localPosition = pawlRest + (value ? Vector3.up * .006f : Vector3.zero);
            if (game != null && game.Motion != null) game.Motion.BuildGraph(true);
        }
    }
}
