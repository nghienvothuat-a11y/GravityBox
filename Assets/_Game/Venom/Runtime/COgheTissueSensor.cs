using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Measures actual tissue supported over a pad; neither selection nor fragment identity latches it.</summary>
    public sealed class COgheTissueSensor : COgheMechanism
    {
        public Vector2 Size = new Vector2(.075f, .075f);
        public float Threshold = .009f;
        public Transform Cap, Pin;
        public Renderer Indicator;
        /// <summary>Load gauge (chapter 4): one tile per quarter body the pad needs. Tile i lights once the load reaches
        /// (i+1)/n of the threshold, so a quarter on a two-tile (half-body) pad lights one tile of two.</summary>
        public Renderer[] GaugeTiles;
        /// <summary>Above zero the gauge measures up to this load instead of the threshold (a motor pad that any part powers
        /// but whose strength follows its weight: four tiles, one per quarter).</summary>
        public float GaugeFull;
        /// <summary>A scale pan (Boss 40, the balance lift): above zero, any part with tissue over the pan up to this height
        /// is weighed whole (a quarter .024, three quarters .072). Counting only the tissue over the pan let a part standing
        /// half off read light: three quarters off-centre missed the window, a whole body half off fell into it.</summary>
        public float Column;
        /// <summary>A scale accepts a window: above MaxLoad it is too heavy and inactive (Overload lights).</summary>
        public float MaxLoad;
        public Renderer Overload;
        /// <summary>The scale's beam: counterweight end down while light, pan end down while too heavy, level when right.</summary>
        public Transform Beam;
        public float BeamTilt = 14f;
        public float Load { get; private set; }
        public bool TooHeavy => MaxLoad > 0 && Load > MaxLoad;
        public bool Active => Load >= Threshold && !TooHeavy;
        /// <summary>Whether a world point lies in the weighed volume (the pad's column, or its contact layer).</summary>
        public bool Holds(Vector3 world)
        {
            Vector3 p = transform.InverseTransformPoint(world);
            return Mathf.Abs(p.x) < Size.x * .5f && Mathf.Abs(p.z) < Size.y * .5f && p.y >= -.003f && p.y <= (Column > 0 ? Column : .02f);
        }
        private MaterialPropertyBlock properties;
        public override void ResetMechanism(VenomCampaign game) { Load = 0; Show(); }
        private static readonly int[] weighed = new int[CohesiveOrganism.ParticleCount];
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            Load = 0;
            if (Column > 0 && game != null && game.Matter != null)
            {
                int parts = 0;
                for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                {
                    if (game.Matter.Escaped[i] || !Holds(game.Matter.Bodies[i].position)) continue;
                    int g = game.Matter.Groups[i];bool seen = false;
                    for (int k = 0; k < parts; k++) if (weighed[k] == g) { seen = true; break; }
                    if (!seen) weighed[parts++] = g;
                }
                for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                {
                    if (game.Matter.Escaped[i]) continue;
                    for (int k = 0; k < parts; k++) if (weighed[k] == game.Matter.Groups[i]) { Load += game.Matter.Profile.ParticleMass; break; }
                }
                Show(); return;
            }
            if (game != null && game.Matter != null)
                for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                {
                    if (game.Matter.Escaped[i]) continue;
                    Vector3 p = transform.InverseTransformPoint(game.Matter.Bodies[i].position);
                    // Particle radius plus a small contact skin. Tissue high over the pad supplies no load.
                    float top = Column > 0 ? Column : game.Matter.Profile.ParticleRadius + .009f;
                    if (Mathf.Abs(p.x) < Size.x * .5f && Mathf.Abs(p.z) < Size.y * .5f && p.y >= -.003f && p.y <= top)
                        Load += game.Matter.Profile.ParticleMass * Mathf.Clamp01(Vector3.Dot(transform.up, Vector3.up));
                }
            Show();
        }
        private void Show()
        {
            if (Cap != null) Cap.localPosition = Vector3.down * (Active ? .006f : 0);
            if (Pin != null) Pin.localPosition = Vector3.right * (Active ? .018f : 0);
            if (GaugeTiles != null)
                for (int i = 0; i < GaugeTiles.Length; i++)
                    if (GaugeTiles[i] != null) GaugeTiles[i].enabled = Load >= (GaugeFull > 0 ? GaugeFull : Threshold) * (i + 1) / GaugeTiles.Length - 1e-5f;
            if (Overload != null) Overload.enabled = TooHeavy;
            if (Beam != null)
                Beam.localRotation = Quaternion.Euler(0, 0, Active ? 0 : TooHeavy ? BeamTilt : -BeamTilt * (1 - .5f * Mathf.Clamp01(Load / Threshold)));
            if (Indicator == null) return;
            if (properties == null) properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", Active ? new Color(.32f,.70f,.59f) : new Color(.65f,.74f,.77f));
            properties.SetColor("_EmissionColor", Active ? new Color(.10f,.27f,.19f) : Color.black);
            Indicator.SetPropertyBlock(properties);
        }
    }
}
