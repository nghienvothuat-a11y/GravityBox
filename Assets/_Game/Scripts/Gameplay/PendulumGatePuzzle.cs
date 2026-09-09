using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    /// <summary>The gate is a native jointed pendulum. These references never drive its position.</summary>
    public sealed class PendulumGatePuzzle : MonoBehaviour
    {
        public PhysicalHinge Pendulum;
        public Collider Bob;
        public float ApertureWidth = .080f;
        public float ArmLength = .190f;
        public float PassagePlaneZ;
    }
}
