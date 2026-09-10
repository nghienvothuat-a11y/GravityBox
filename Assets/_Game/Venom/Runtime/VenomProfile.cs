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
        [Header("Living surface (visual only, metres)")]
        public float IdleBulge = .0028f;
        public float CuriousHeadLift = .034f;
        public float TendrilReach = .028f;
        public PhysicsMaterial Contact;
        public Material Skin;
    }
}
