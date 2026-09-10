using UnityEngine;

namespace GravityBox.Venom
{
    [CreateAssetMenu(menuName = "Gravity Box/Venom Matter")]
    public sealed class VenomProfile : ScriptableObject
    {
        public float ParticleRadius = .009f;
        public float ParticleMass = .003f;
        public float Spacing = .0185f;
        public float BondReach = .027f;
        public float Stiffness = 2.6f;
        public float Viscosity = .009f;
        public float Plasticity = .65f;
        public float CutHealingDelay = 1.5f;
        public float FusionSeconds = .65f;
        public float SkinSupport = .026f;
        public float SkinThreshold = .48f;
        public float MeshCell = .0065f;
        [Header("Squeezing tissue (direct control)")]
        [Range(.02f,1)] public float FlowStiffness = .16f;
        [Min(.1f)] public float FlowPlasticity = 12f;
        [Min(.1f)] public float TissuePressure = 8f;
        [Min(0)] public float TissueDamping = .06f;
        [Header("Living surface (visual only)")]
        [Range(.25f,3f)] public float AnimationSpeed = 1.5f;
        public float IdleBulge = .0036f;
        public float CuriousHeadLift = .041f;
        public float TendrilReach = .034f;
        public float RaisedTendrilReach = .052f;
        public float DanceLift = .064f;
        [Min(2)] public float DanceInterval = 10f;
        public PhysicsMaterial Contact;
        public Material Skin;
    }
}
