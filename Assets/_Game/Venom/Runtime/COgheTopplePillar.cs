using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// A pillar on a hinge at its foot (Mrk 19/10/2026: physics props). It stands until a trigger body (a steel ball) touches
    /// it, then falls about the hinge until it rests at FallAngle as a ramp or a bridge. The fall is integrated here as a
    /// rigid rod turning about its foot, not left to a joint, so it lands the same way every time. COghe pushing it does
    /// nothing: only the trigger knocks it over.
    /// </summary>
    public sealed class COgheTopplePillar : COgheMechanism
    {
        public Rigidbody Body;
        public Vector3 HingeLocal;
        // Root-local axis; a positive turn about it tips the pillar toward its landing.
        public Vector3 AxisLocal = Vector3.forward;
        public float Length = .22f, FallAngle = 66f, KnockSpeed = 1.2f, TriggerReach = .006f;
        public Rigidbody[] Triggers = System.Array.Empty<Rigidbody>();
        // Slick while the pillar stands (it is no ladder), walkable once it is down.
        public VenomSurfacePatch[] WalkWhenDown = System.Array.Empty<VenomSurfacePatch>();
        public Material StandingMaterial, DownMaterial;
        public bool Down => state == 2;
        public bool Falling => state == 1;
        public float Angle { get; private set; }
        private int state;
        private float speed;
        private Vector3 restPosition;
        private Quaternion restRotation;
        private Collider[] shapes;
        private Transform root;

        public override void InitializeMechanism(VenomCampaign game)
        {
            root = Body.transform.parent; restPosition = Body.transform.localPosition; restRotation = Body.transform.localRotation;
            shapes = Body.GetComponentsInChildren<Collider>(true);
        }
        public override void ResetMechanism(VenomCampaign game)
        {
            if (shapes == null) InitializeMechanism(game);
            state = 0; speed = 0; Angle = 0;
            Body.transform.localPosition = restPosition; Body.transform.localRotation = restRotation;
            Walkable(false);
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            if (state == 0) { if (Knocked()) { state = 1; speed = KnockSpeed; } return; }
            if (state != 1) return;
            // A rod turning about its foot: angular acceleration 3g sin(theta) / 2L.
            speed += 1.5f * 9.81f / Length * Mathf.Sin(Angle * Mathf.Deg2Rad) * dt;
            Angle = Mathf.Min(FallAngle, Angle + speed * Mathf.Rad2Deg * dt);
            Quaternion turn = Quaternion.AngleAxis(Angle, AxisLocal);
            Vector3 local = HingeLocal + turn * (restPosition - HingeLocal);
            Body.MovePosition(root.TransformPoint(local)); Body.MoveRotation(root.rotation * (turn * restRotation));
            if (Angle >= FallAngle)
            {
                state = 2; Body.transform.localPosition = local; Body.transform.localRotation = turn * restRotation;
                Walkable(true); game.Motion.BuildGraph(true);
            }
        }
        private bool Knocked()
        {
            foreach (var trigger in Triggers)
            {
                if (trigger == null) continue;
                var ball = trigger.GetComponent<SphereCollider>();
                float reach = (ball != null ? ball.radius * trigger.transform.lossyScale.x : 0) + TriggerReach;
                Vector3 centre = trigger.worldCenterOfMass;
                foreach (var shape in shapes) if (shape != null && shape.enabled && (shape.ClosestPoint(centre) - centre).magnitude <= reach) return true;
            }
            return false;
        }
        private void Walkable(bool down)
        {
            foreach (var face in WalkWhenDown)
            {
                if (face == null) continue;
                face.Slippery = !down;
                var look = face.GetComponent<Renderer>();
                var material = down ? DownMaterial : StandingMaterial;
                if (look != null && material != null) look.sharedMaterial = material;
            }
        }
    }
}
