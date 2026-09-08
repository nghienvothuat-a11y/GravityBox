using UnityEngine;

namespace GravityBox.Simulation
{
    [CreateAssetMenu(menuName = "Gravity Box/Rotation Settings")]
    public sealed class RotationSettings : ScriptableObject
    {
        [Min(1)] public float DegreesPerScreen = 200f;
        [Range(30, 240)] public float MaxDegreesPerSecond = 85f;
        [Range(30, 720)] public float MaxDegreesPerSecondSquared = 360f;
        [Range(0.01f, 0.3f)] public float SmoothingSeconds = 0.11f;
        [Range(0, 20)] public float AssistedSnapAngle = 10f;
        [Range(1, 45)] public float MaxQueuedAngle = 14f;
    }
}
