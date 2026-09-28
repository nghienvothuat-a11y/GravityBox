using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    [DefaultExecutionOrder(100)]
    public sealed class COgheViewPresentation : MonoBehaviour
    {
        public VenomSurfacePatch[] Panes;
        public Transform[] PaneVisuals;
        public Material[] FadeSources, FadeVariants;
        [Min(.01f)] public float FadeSeconds = .18f;
        [Range(.02f, .5f)] public float AngleBand = .24f;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private sealed class PaneRenderer
        {
            public Renderer Renderer;
            public Material[] Solid, Faded;
            public Color[] Colors;
            public MaterialPropertyBlock[] OriginalBlocks, Blocks;
            public ShadowCastingMode Shadows;
            public bool Enabled;
        }
        private PaneRenderer[][] renderers;
        private float[] opacity;
        private bool initialized;
        private VenomCampaign game;

        private void Awake()
        {
            game = GetComponent<VenomCampaign>();
            renderers = new PaneRenderer[Panes.Length][]; opacity = new float[Panes.Length];
            for (int i = 0; i < Panes.Length; i++)
            {
                var list = PaneVisuals != null && i < PaneVisuals.Length && PaneVisuals[i] != null
                    ? PaneVisuals[i].GetComponentsInChildren<Renderer>() : new[] { Panes[i].GetComponent<Renderer>() };
                renderers[i] = new PaneRenderer[list.Length];
                for (int j = 0; j < list.Length; j++)
                {
                    var r = list[j]; var materials = r.sharedMaterials;
                    var entry = new PaneRenderer { Renderer = r, Solid = materials, Faded = (Material[])materials.Clone(),
                        Shadows = r.shadowCastingMode, Enabled = r.enabled, Colors = new Color[materials.Length],
                        OriginalBlocks = new MaterialPropertyBlock[materials.Length], Blocks = new MaterialPropertyBlock[materials.Length] };
                    for (int k = 0; k < materials.Length; k++)
                    {
                        entry.Colors[k] = materials[k].GetColor(BaseColor);
                        entry.OriginalBlocks[k] = new MaterialPropertyBlock(); entry.Blocks[k] = new MaterialPropertyBlock();
                        r.GetPropertyBlock(entry.OriginalBlocks[k], k); r.GetPropertyBlock(entry.Blocks[k], k);
                        if (FadeSources != null && FadeVariants != null)
                            for (int f = 0; f < FadeSources.Length && f < FadeVariants.Length; f++)
                                if (FadeSources[f] == materials[k] && FadeVariants[f] != null) entry.Faded[k] = FadeVariants[f];
                    }
                    renderers[i][j] = entry;
                }
            }
        }
        private void LateUpdate() => Refresh(Time.unscaledDeltaTime);

        // Only presentation state is written. Ray picking and physical pane data remain independent.
        public void Refresh(float deltaTime)
        {
            if (game.Owner == null || game.Home || game.Owner.Completed || renderers == null) return;
            for (int i = 0; i < Panes.Length; i++)
            {
                float facing = Vector3.Dot(game.Owner.View.transform.forward, Panes[i].Normal);
                float target = Mathf.SmoothStep(0, 1, Mathf.Clamp01(-facing / Mathf.Max(.02f, AngleBand)));
                float value = initialized ? Mathf.MoveTowards(opacity[i], target, Mathf.Max(0, deltaTime) / Mathf.Max(.01f, FadeSeconds)) : target;
                if (initialized && Mathf.Abs(value - opacity[i]) < .00001f) continue;
                opacity[i] = value;
                foreach (var entry in renderers[i]) Apply(entry, value);
            }
            initialized = true;
        }
        private static void Apply(PaneRenderer entry, float value)
        {
            var r = entry.Renderer;
            r.enabled = value > .0001f;
            bool solid = value >= .9999f;
            r.sharedMaterials = solid ? entry.Solid : entry.Faded;
            r.shadowCastingMode = solid ? entry.Shadows : ShadowCastingMode.Off;
            for (int k = 0; k < entry.Solid.Length; k++)
            {
                if (solid) r.SetPropertyBlock(entry.OriginalBlocks[k], k);
                else
                {
                    Color color = entry.Colors[k]; color.a *= value;
                    entry.Blocks[k].SetColor(BaseColor, color); r.SetPropertyBlock(entry.Blocks[k], k);
                }
            }
        }
        private void OnDisable()
        {
            initialized = false;
            if (renderers == null) return;
            foreach (var pane in renderers) foreach (var entry in pane)
            {
                if (entry.Renderer == null) continue;
                entry.Renderer.sharedMaterials = entry.Solid; entry.Renderer.shadowCastingMode = entry.Shadows;
                entry.Renderer.enabled = entry.Enabled;
                for (int k = 0; k < entry.Solid.Length; k++) entry.Renderer.SetPropertyBlock(entry.OriginalBlocks[k], k);
            }
        }
    }
}
