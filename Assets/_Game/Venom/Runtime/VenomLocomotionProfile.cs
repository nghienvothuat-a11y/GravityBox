using UnityEngine;

namespace GravityBox.Venom
{
    public enum VenomControlMode { TiltBox, SelectFragment, FollowLargest }

    [CreateAssetMenu(menuName = "Gravity Box/Venom Locomotion")]
    public sealed class VenomLocomotionProfile : ScriptableObject
    {
        [Min(.01f)] public float CrawlSpeed = .14f;
        [Min(.01f)] public float FollowSpeed = .16f;
        [Min(0)] public float FollowDelay = 3f;
        [Min(.1f)] public float VelocityResponse = 16f;
        [Min(.1f)] public float MaxAcceleration = 5f;
        [Min(.1f)] public float GripAcceleration = 3f;
        [Min(0)] public float FrictionCompensation = 1.3f;
        [Min(.1f)] public float RepathSeconds = .35f;
        [Min(.009f)] public float NavigationClearance = .029f;
    }
}
