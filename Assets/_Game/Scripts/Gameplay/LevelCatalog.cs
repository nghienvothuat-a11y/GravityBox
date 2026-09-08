using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    [CreateAssetMenu(menuName = "Gravity Box/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        public LevelDefinition[] Levels;
        public BallController BallPrefab;
        public BallPhysicsProfile BallProfile;
        public RotationSettings Rotation;
        [Min(0.1f)] public float CompletionDelay = 1.8f;
        [Min(0.1f)] public float FailureDelay = 0.7f;
    }
}
