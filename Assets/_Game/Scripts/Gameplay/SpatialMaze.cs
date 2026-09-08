using System;
using UnityEngine;

namespace GravityBox.Gameplay
{
    [Serializable]
    public sealed class SpatialMazeEdge
    {
        public int A, B, Axis;
        public MeshCollider Collider;
    }

    [Serializable]
    public sealed class SpatialMazePlank
    {
        public Vector3 CentreLocal, Size;
        public Quaternion RotationLocal = Quaternion.identity;
        public MeshCollider SectionCollider;
    }

    /// <summary>
    /// Geometry references for a maze assembled from small physical planks.
    /// The graph documents the connected empty space; it never moves the ball.
    /// </summary>
    public sealed class SpatialMaze : MonoBehaviour
    {
        public float InnerRadius = .36f;
        public float ShellThickness = .006f;
        public MeshCollider ShellCollider;
        public float ClearWidth = .050f;
        public float PlankThickness = .003f;
        public float PlankWidth = .010f;
        public float SightGap = .015f;
        public Vector3[] NodesLocal = Array.Empty<Vector3>();
        public SpatialMazeEdge[] Edges = Array.Empty<SpatialMazeEdge>();
        public int[] MainPath = Array.Empty<int>();
        public MeshCollider[] JunctionColliders = Array.Empty<MeshCollider>();
        public SpatialMazePlank[] Planks = Array.Empty<SpatialMazePlank>();
        public int SpawnNode = 20;
        [Tooltip("The final target is outside the real circular exit, beyond the shell.")]
        public int ExitNode = 27;
    }
}
