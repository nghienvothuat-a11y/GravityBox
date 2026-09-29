using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// A plank on a real hinge axle. A tension-only rope runs from a load tray over pulleys to the plank's anchor:
    /// as the loaded tray sinks, the rope lifts that end. When the plank rests on both bearers (level) a pawl catches
    /// the axle; the latched deck is carried by static surfaces so walking on it cannot rock it.
    /// </summary>
    public sealed class COgheSeesawBridge : COgheMechanism
    {
        public Rigidbody Plank;
        public HingeJoint Hinge;
        public Transform Anchor;
        public COgheRailSlider Tray;
        public Transform[] Guides = System.Array.Empty<Transform>();
        public float Stiffness = 60, Damping = .2f, MaximumTension = 1.5f, CatchAngle = 1.2f, Slack = .008f;
        public Quaternion LevelLocalRotation = Quaternion.identity;
        public VenomSurfacePatch[] MovingSurfaces = System.Array.Empty<VenomSurfacePatch>();
        public VenomSurfacePatch[] DockedSurfaces = System.Array.Empty<VenomSurfacePatch>();
        public LineRenderer Rope;
        public Transform Pawl;
        public bool Caught { get; private set; }
        public float Tension { get; private set; }
        public override string Activity => !Caught && Tension > .05f ? "Đối trọng đang kéo cầu" : null;
        public float AngleToLevel => Quaternion.Angle(Quaternion.Inverse(Plank.transform.parent.rotation) * Plank.rotation, LevelLocalRotation);
        private Vector3 anchorRest, pawlRest;
        private float trayRest, stable;
        private Quaternion plankRest;
        private Vector3 plankRestPosition;

        public override void InitializeMechanism(VenomCampaign game)
        {
            anchorRest = Plank.transform.parent.InverseTransformPoint(Anchor.position);
            plankRest = Plank.rotation; plankRestPosition = Plank.position;
            if (Pawl != null) pawlRest = Pawl.localPosition;
        }
        public override void ResetMechanism(VenomCampaign game)
        {
            Caught = false; stable = 0; Tension = 0;
            Plank.isKinematic = false; Plank.position = plankRestPosition; Plank.rotation = plankRest;
            Plank.linearVelocity = Plank.angularVelocity = Vector3.zero;
            Show(game, false, true);
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            if (Caught) { Tension = 0; return; }
            // Weight comes from the campaign, which accelerates every prop (the plank is one) — never add it twice.
            // Rope length is fixed: the tray's descent must equal the anchor's rise. Tension only pulls.
            Vector3 anchorNow = Plank.transform.parent.InverseTransformPoint(Anchor.position);
            float rise = anchorNow.y - anchorRest.y, descent = Tray.Position;
            float riseSpeed = Vector3.Dot(Plank.GetPointVelocity(Anchor.position), Vector3.up);
            float traySpeed = Vector3.Dot(Tray.Body.linearVelocity, Tray.WorldAxis);
            // A little slack at rest: the rope only tightens once the loaded tray has really started to sink.
            float stretch = descent - rise - Slack;
            Tension = stretch > 0 ? Mathf.Clamp(stretch * Stiffness + (traySpeed - riseSpeed) * Damping, 0, MaximumTension) : 0;
            Tray.ApplyEffort(-Tray.WorldAxis * Tension);
            Plank.AddForceAtPosition(Vector3.up * Tension, Anchor.position);
            float angle = Quaternion.Angle(Quaternion.Inverse(Plank.transform.parent.rotation) * Plank.rotation, LevelLocalRotation);
            stable = angle <= CatchAngle && Plank.angularVelocity.magnitude < .15f && Tension > .05f ? stable + dt : 0;
            if (stable >= .12f) { Caught = true; Plank.isKinematic = true; Show(game, true, false); }
        }
        private void Show(VenomCampaign game, bool docked, bool force)
        {
            foreach (var s in MovingSurfaces) if (s != null) s.gameObject.SetActive(!docked);
            foreach (var s in DockedSurfaces) if (s != null) s.gameObject.SetActive(docked);
            if (Pawl != null) Pawl.localPosition = pawlRest + (docked ? Vector3.down * .008f : Vector3.zero);
            Physics.SyncTransforms();
            if (game != null && game.Motion != null) game.Motion.BuildGraph(true);
        }
        private void LateUpdate()
        {
            if (Rope == null || Anchor == null || Tray == null) return;
            Rope.positionCount = Guides.Length + 2; Rope.SetPosition(0, Tray.Body.position + Vector3.up * .02f);
            for (int i = 0; i < Guides.Length; i++) Rope.SetPosition(i + 1, Guides[i].position);
            Rope.SetPosition(Guides.Length + 1, Anchor.position);
        }
    }
}
