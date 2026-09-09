using UnityEngine;

namespace GravityBox.Simulation
{
    [CreateAssetMenu(menuName = "Gravity Box/Water Profile")]
    public sealed class WaterProfile : ScriptableObject
    {
        [Min(1)] public float Density = 998.2f;
        [Min(.000001f)] public float DynamicViscosity = .001002f;
        [Min(.00000003f), Tooltip("Assumed combined surface roughness in metres; not measured for this virtual material.")]
        public float EffectiveRoughness = .000003f;
        [Min(.01f), Tooltip("Approximate bulk-flow response to the held box; not a resolved fluid solver.")]
        public float EntrainmentSeconds = .4f;
    }
}
