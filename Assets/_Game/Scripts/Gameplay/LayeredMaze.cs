using System;
using UnityEngine;

namespace GravityBox.Gameplay
{
    [Serializable]
    public sealed class LayeredMazeDeck
    {
        public string Name;
        public float FloorHeight;
        public Collider FloorCollider;
        public Renderer[] Renderers = Array.Empty<Renderer>();
        [Tooltip("Authored centreline in box-local metres; useful for validation, never applied to the ball.")]
        public Vector3[] RouteLocalPoints = Array.Empty<Vector3>();
    }

    /// <summary>
    /// Geometry metadata for stacked, physically connected mazes. Layer changes
    /// are read from the ball position; this component never moves any object.
    /// </summary>
    public sealed class LayeredMaze : MonoBehaviour
    {
        public LayeredMazeDeck[] Decks = Array.Empty<LayeredMazeDeck>();
        public float[] FloorHeights = Array.Empty<float>();
        public Transform[] TransferPorts = Array.Empty<Transform>();
        public float TransferRadius = 0.038f;
        public int LayerCount => Decks.Length;

        // Indices follow the authored box, top to bottom, even when it is inverted.
        public int GetLayerIndex(Vector3 worldBall)
        {
            if (Decks.Length == 0) return -1;
            float localHeight = transform.InverseTransformPoint(worldBall).y;
            for (int i = 0; i < Decks.Length - 1; i++)
                if (localHeight >= Decks[i].FloorHeight) return i;
            return Decks.Length - 1;
        }
    }
}
