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
        public float Load { get; private set; }
        public bool Active => Load >= Threshold;
        private MaterialPropertyBlock properties;
        public override void ResetMechanism(VenomCampaign game) { Load = 0; Show(); }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            Load = 0;
            if (game != null && game.Matter != null)
                for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                {
                    if (game.Matter.Escaped[i]) continue;
                    Vector3 p = transform.InverseTransformPoint(game.Matter.Bodies[i].position);
                    // Particle radius plus a small contact skin. Tissue high over the pad supplies no load.
                    if (Mathf.Abs(p.x) < Size.x * .5f && Mathf.Abs(p.z) < Size.y * .5f && p.y >= -.003f && p.y <= game.Matter.Profile.ParticleRadius + .009f)
                        Load += game.Matter.Profile.ParticleMass * Mathf.Clamp01(Vector3.Dot(transform.up, Vector3.up));
                }
            Show();
        }
        private void Show()
        {
            if (Cap != null) Cap.localPosition = Vector3.down * (Active ? .006f : 0);
            if (Pin != null) Pin.localPosition = Vector3.right * (Active ? .018f : 0);
            if (Indicator == null) return;
            if (properties == null) properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", Active ? new Color(.32f,.70f,.59f) : new Color(.65f,.74f,.77f));
            properties.SetColor("_EmissionColor", Active ? new Color(.10f,.27f,.19f) : Color.black);
            Indicator.SetPropertyBlock(properties);
        }
    }
}
