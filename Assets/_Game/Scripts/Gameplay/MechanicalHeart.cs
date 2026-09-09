using System.Collections.Generic;
using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    // The spring plunger releases an ideal retaining pawl. It does not drive the
    // bridge: a loaded lever must still meet its receiver before it can latch.
    public sealed class MechanicalHeart : MonoBehaviour, IBallMechanism, IResettable, IForceProvider, IForceStepProvider
    {
        public ContactSeatLatch BridgeCatch;
        public PressurePlunger ReleasePlunger;
        public PhysicalHinge Cage;
        public Transform TransferBore;
        public Transform CageInlet;
        public Transform CageMouth;
        public bool Released { get; private set; }
        public BallController ReleaseOperator { get; private set; }
        public void Bind(IReadOnlyList<BallController> balls) => ReleasePlunger.Bind(balls);
        public void CaptureInitialState() { }
        public void ResetState()
        { Released=false; ReleaseOperator=null; BridgeCatch.Armed=false; ReleasePlunger.ResetState(); }
        public void PrepareStep(float dt)
        {
            if(dt<=0 || !isActiveAndEnabled) return;
            ReleasePlunger.StepSpring();
            if(ReleasePlunger.Pressed) { Released=true; ReleaseOperator=ReleasePlunger.Operator; }
            BridgeCatch.Armed=Released;
        }
        public Vector3 GetAcceleration(IPhysicsAffectable target, EnvironmentProfile environment) => Vector3.zero;
    }
}
