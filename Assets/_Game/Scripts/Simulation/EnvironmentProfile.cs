using UnityEngine;

namespace GravityBox.Simulation
{
    [CreateAssetMenu(menuName = "Gravity Box/Environment Profile")]
    public sealed class EnvironmentProfile : ScriptableObject
    {
        public string Id = "earth";
        public string DisplayName = "GRAVITY";
        [Min(0)] public float GravityScale = 1f;
        public Vector3 WorldGravityDirection = Vector3.down;
        [Min(0)] public float LinearDamping = 0.035f;
        [Min(0)] public float AngularDamping = 0.04f;
        [Min(1)] public float MaxLinearSpeed = 12f;
        [Min(1)] public float MaxAngularSpeed = 35f;
        public Color Accent = new Color(0.65f, 0.94f, 0.55f);
        public Color Background = new Color(0.037f, 0.061f, 0.08f);
        public bool IsZeroGravity => GravityScale <= 0.0001f;
        public Vector3 Acceleration => WorldGravityDirection.normalized * (9.81f * GravityScale);

        private void OnValidate()
        {
            GravityScale = Mathf.Max(0, GravityScale);
            LinearDamping = Mathf.Max(0, LinearDamping);
            AngularDamping = Mathf.Max(0, AngularDamping);
            MaxLinearSpeed = Mathf.Max(1, MaxLinearSpeed);
            MaxAngularSpeed = Mathf.Max(1, MaxAngularSpeed);
        }
    }
}
