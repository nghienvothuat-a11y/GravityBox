using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Presentation
{
    // A cutaway affects renderer materials only. Every deck stays physically present.
    public sealed class MazeLayerView : MonoBehaviour
    {
        private sealed class Surface
        {
            public Renderer Renderer;
            public Material[] Original, Faded;
            public int Layer;
        }
        private readonly List<Surface> surfaces = new List<Surface>();
        private readonly Dictionary<Material, Material> fadedMaterials = new Dictionary<Material, Material>();
        private LayeredMaze maze;
        private BallController ball;
        public bool Overview { get; private set; }
        public int ActiveLayer { get; private set; } = -1;
        public int LayerCount => maze != null ? maze.Decks.Length : 0;

        public void Initialize(LayeredMaze layout, BallController sphere)
        {
            maze = layout; ball = sphere;
            for (int layer = 0; layer < maze.Decks.Length; layer++)
            foreach (Renderer renderer in maze.Decks[layer].Renderers)
            {
                if (renderer == null) continue;
                Material[] original = renderer.sharedMaterials;
                var faded = new Material[original.Length];
                for (int i = 0; i < original.Length; i++) faded[i] = Fade(original[i]);
                surfaces.Add(new Surface { Renderer = renderer, Original = original, Faded = faded, Layer = layer });
            }
            Refresh();
        }

        private Material Fade(Material source)
        {
            if (source == null) return null;
            if (fadedMaterials.TryGetValue(source, out Material material)) return material;
            material = new Material(source) { name = source.name + " (inactive floor)" };
            material.SetFloat("_Surface", 1); material.SetFloat("_Blend", 0);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0); material.SetFloat("_Cull", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetShaderPassEnabled("ShadowCaster", false);
            Color color = source.GetColor("_BaseColor");
            color.a = color.a < .5f ? .008f : .045f;
            material.SetColor("_BaseColor", color);
            if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", Color.black);
            fadedMaterials.Add(source, material);
            return material;
        }

        public void SetOverview(bool overview)
        {
            Overview = overview;
            Apply();
        }

        public void Refresh()
        {
            if (maze == null || ball == null) return;
            ActiveLayer = maze.GetLayerIndex(ball.Body.position);
            Apply();
        }

        private void LateUpdate()
        {
            if (maze == null || ball == null) return;
            int layer = maze.GetLayerIndex(ball.Body.position);
            if (layer == ActiveLayer) return;
            ActiveLayer = layer;
            Apply();
        }

        private void Apply()
        {
            foreach (Surface surface in surfaces)
                if (surface.Renderer != null)
                    surface.Renderer.sharedMaterials = Overview || surface.Layer == ActiveLayer ? surface.Original : surface.Faded;
        }

        private void OnDestroy()
        {
            foreach (Surface surface in surfaces)
                if (surface.Renderer != null) surface.Renderer.sharedMaterials = surface.Original;
            foreach (Material material in fadedMaterials.Values)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) { DestroyImmediate(material); continue; }
#endif
                Destroy(material);
            }
            surfaces.Clear(); fadedMaterials.Clear();
        }
    }
}
