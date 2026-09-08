using UnityEngine;

namespace GravityBox.Simulation
{
    [CreateAssetMenu(menuName = "Gravity Box/Ball Physics Profile")]
    public sealed class BallPhysicsProfile : ScriptableObject
    {
        [Tooltip("Kilograms. A solid steel sphere with a 15 mm radius weighs about 111 g.")]
        [Min(0.001f)] public float Mass = 0.111f;
        [Tooltip("Metres; the Rigidbody and SphereCollider must have unit world scale.")]
        [Min(0.001f)] public float Radius = 0.015f;
        [Tooltip("Dimensionless rolling moment coefficient: resisting torque = coefficient × normal load × radius. Applied only at a real contact.")]
        [Range(0, 0.05f)] public float RollingResistanceCoefficient = 0.008f;
        [Min(0.00001f)] public float ContactOffset = 0.0005f;
        [Min(1)] public float MinimumAngularSpeedLimit = 400f;
        [Min(0.1f)] public float MaxDepenetrationSpeed = 1f;
        [Range(6, 32)] public int SolverIterations = 16;
        [Range(1, 16)] public int SolverVelocityIterations = 8;
        public CollisionDetectionMode CollisionDetection = CollisionDetectionMode.ContinuousDynamic;
        public PhysicsMaterial ContactMaterial;

        public float SolidSphereInertia => 0.4f * Mass * Radius * Radius;
    }
}
