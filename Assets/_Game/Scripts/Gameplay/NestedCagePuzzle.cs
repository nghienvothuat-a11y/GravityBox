using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    /// <summary>Authored references for inspection and physical route verification; contains no ball control.</summary>
    public sealed class NestedCagePuzzle : MonoBehaviour
    {
        public PhysicalHinge Cage;
        public Transform Mouth;
        public Vector3 CageHalfSize = new Vector3(.13f, .05f, .13f);
        public float MouthWidth = .064f;
        public float StopAngle = 32;
    }
}
