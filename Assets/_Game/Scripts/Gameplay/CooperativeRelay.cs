using System.Collections.Generic;
using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    // Pressure-operated powered shutters: physical contacts compress the plungers;
    // bounded motor forces move collidable gates. The second switch retains both gates.
    public sealed class CooperativeRelay : MonoBehaviour, IBallMechanism, IResettable, IForceProvider, IForceStepProvider
    {
        public PressurePlunger HoldSwitch, ReleaseSwitch;
        public GravitySliderGuide LeftGate, RightGate;
        public bool Released { get; private set; }
        public bool Holding => HoldSwitch.Pressed;
        public bool LeftClear => LeftGate.IsPassageClear;
        public bool RightClear => RightGate.IsPassageClear;
        public BallController FirstOperator { get; private set; }
        public BallController SecondOperator { get; private set; }
        public void Bind(IReadOnlyList<BallController> balls)
        {
            HoldSwitch.Bind(balls); ReleaseSwitch.Bind(balls);
        }
        public void CaptureInitialState() { }
        public void ResetState()
        {
            Released = false; FirstOperator = SecondOperator = null;
            HoldSwitch.ResetState(); ReleaseSwitch.ResetState();
        }
        public void PrepareStep(float dt)
        {
            if (dt <= 0 || !isActiveAndEnabled) return;
            HoldSwitch.StepSpring(); ReleaseSwitch.StepSpring();
            if (Holding && !Released) FirstOperator = HoldSwitch.Operator;
            // The two isolated entrance chambers enforce the hand-off geometrically.
            // Record the operators for diagnostics; no identity-specific control force.
            if (ReleaseSwitch.Pressed)
            {
                SecondOperator = ReleaseSwitch.Operator;
                Released = true;
            }
            Drive(LeftGate, Released);
            Drive(RightGate, Released || Holding);
        }
        private static void Drive(GravitySliderGuide guide, bool open)
        {
            float target = open ? guide.Travel : 0;
            float force = Mathf.Clamp((target - guide.Displacement) * 100 - guide.TravelSpeed * 4.5f, -2, 2);
            guide.Body.AddForce(guide.Joint.connectedBody.rotation * guide.SlideAxisInBox * force, ForceMode.Force);
        }
        public Vector3 GetAcceleration(IPhysicsAffectable target, EnvironmentProfile environment) => Vector3.zero;
    }
}
