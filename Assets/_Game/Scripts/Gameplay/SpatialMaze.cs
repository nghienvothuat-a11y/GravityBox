using UnityEngine;

namespace GravityBox.Gameplay
{
    /// <summary>
    /// Authoring references for separate physical planks in a glass sphere.
    /// The open air between them is playable space; there is no prescribed path.
    /// </summary>
    public sealed class SpatialMaze : MonoBehaviour
    {
        public float InnerRadius = .36f;
        public float ShellThickness = .006f;
        public MeshCollider ShellCollider;
        public BoxCollider[] Planks = System.Array.Empty<BoxCollider>();
        public int SpawnPlankIndex;
        public int CatchPlankIndex = 1;
        public int AuthoringSeed = 12;
        public BoxCollider SpawnPlank => Planks != null && SpawnPlankIndex >= 0 && SpawnPlankIndex < Planks.Length ? Planks[SpawnPlankIndex] : null;
        public BoxCollider CatchPlank => Planks != null && CatchPlankIndex >= 0 && CatchPlankIndex < Planks.Length ? Planks[CatchPlankIndex] : null;
    }
}
