using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// A deck on a vertical axle, turned by a gear train one step (a quarter turn) toward its stop. It turns only while
    /// the train is powered and nothing stands on the deck, and catches at the stop for good. Leaving the motor pad
    /// halfway leaves the deck where it is. The deck is a kinematic body: tissue on it is carried by contact, never set.
    /// </summary>
    public sealed class COgheTurntable : COgheMechanism
    {
        public Rigidbody Deck;
        public COgheGearTrain Train;
        public COgheTissueClearance Clearance;
        public float StepDegrees = 90, DegreesPerSecond = 40;
        public bool Caught { get; private set; }
        public float Angle { get; private set; }
        public override string Activity => !Caught && Train != null && Train.Powered && Clearance != null && Clearance.Blocked ? "Có mô trên bàn xoay — bàn chờ" : null;
        private Quaternion restLocal;
        private Vector3 restPosition;
        public override void InitializeMechanism(VenomCampaign game)
        {
            restLocal = Quaternion.Inverse(Deck.transform.parent.rotation) * Deck.rotation; restPosition = Deck.position;
        }
        public override void ResetMechanism(VenomCampaign game)
        {
            Caught = false; Angle = 0; Deck.position = restPosition; Deck.rotation = Deck.transform.parent.rotation * restLocal;
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            if (Caught) return;
            if (Train == null || !Train.Powered || Clearance != null && Clearance.Blocked) return;
            Angle = Mathf.MoveTowards(Angle, StepDegrees, DegreesPerSecond * dt);
            Deck.MoveRotation(Deck.transform.parent.rotation * Quaternion.Euler(0, Angle, 0) * restLocal);
            if (Angle >= StepDegrees - .01f) { Caught = true; game.Motion.BuildGraph(true); }
        }
    }
}
