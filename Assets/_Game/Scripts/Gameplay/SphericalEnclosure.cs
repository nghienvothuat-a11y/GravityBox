using UnityEngine;

namespace GravityBox.Gameplay
{
    // Geometry metadata shared by spherical mechanisms without a maze graph.
    public sealed class SphericalEnclosure : MonoBehaviour
    {
        public float InnerRadius;
        public float ShellThickness;
        public MeshCollider ShellCollider;
    }
}
