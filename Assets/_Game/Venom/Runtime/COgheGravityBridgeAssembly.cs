using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Read-only state for level 14's passive hinge and gravity rail.</summary>
    public sealed class COgheGravityBridgeAssembly : COgheMechanism
    {
        public Rigidbody Bridge,ExitGate;
        public Vector3 GateClosedLocal;
        public Vector3 GateAxis=Vector3.forward;
        public float GateTravel=.14f,GateClearance=.105f;
        public float BridgeTravel { get; private set; }
        public float GateDisplacement { get; private set; }
        public bool BridgeSeated=>BridgeTravel>=86f;
        public bool GateClear=>GateDisplacement<=-GateClearance;
        public override bool ControlsExit => true;
        public override bool ExitUnlocked => GateClear;
        public override string Activity => GateClear ? "Cửa B đã trượt mở" : BridgeSeated ? "Cầu A đã tới bệ" : null;
        private Quaternion bridgeRest;
        private bool captured;

        public override void InitializeMechanism(VenomCampaign game)
        {bridgeRest=Quaternion.Inverse(game.Root.rotation)*Bridge.rotation;captured=true;}

        public override void ResetMechanism(VenomCampaign game)
        {if(!captured)InitializeMechanism(game);BridgeTravel=GateDisplacement=0;}

        public override void StepMechanism(VenomCampaign game,float dt)
        {
            if(!captured)InitializeMechanism(game);
            Quaternion current=Quaternion.Inverse(game.Root.rotation)*Bridge.rotation;
            BridgeTravel=Quaternion.Angle(bridgeRest,current);
            GateDisplacement=Vector3.Dot(game.Root.InverseTransformPoint(ExitGate.position)-GateClosedLocal,GateAxis.normalized);
        }
    }
}
