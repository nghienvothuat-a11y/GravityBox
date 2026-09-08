using UnityEngine;

namespace GravityBox.Simulation
{
    [CreateAssetMenu(menuName = "Gravity Box/Ball Physics Profile")]
    public sealed class BallPhysicsProfile : ScriptableObject
    {
        [Min(0.01f)] public float Mass = 1f;
        [Min(0.1f)] public float Radius = 0.27f;
        [Min(0.1f)] public float MaxDepenetrationSpeed = 3f;
        public CollisionDetectionMode CollisionDetection = CollisionDetectionMode.ContinuousDynamic;
        public PhysicsMaterial ContactMaterial;
    }
}
