using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>A held clutch opens a real spring-return door; a far-side handle catches it at its upper stop.</summary>
    public sealed class COgheSpringAccessDoor : COgheMechanism
    {
        public COgheTissueSensor Input;
        public COgheRailSlider Door, LatchHandle, FinalCover;
        public Vector3 DoorSize = new Vector3(.020f, .16f, .22f);
        public float OpenSpeed = .12f, ReturnSpeed = .15f, MotorForce = .8f;
        public Transform CatchPin, Spring;
        public bool Caught { get; private set; }
        public bool Obstructed { get; private set; }
        public override bool ControlsExit => FinalCover != null;
        public override bool ExitUnlocked => Caught && FinalCover != null && FinalCover.AtEnd;

        public override void ResetMechanism(VenomCampaign game)
        {
            Caught = Obstructed = false;
            Door.Locked = true;
            LatchHandle.Locked = true;
            if (FinalCover != null) FinalCover.Locked = true;
            Show();
        }

        public override void StepMechanism(VenomCampaign game, float dt)
        {
            // Both real stops must meet. Pulling L early cannot remotely finish D.
            if (Door.AtEnd && LatchHandle.AtEnd) Caught = true;
            bool powered = Input != null && Input.Active;
            Obstructed = !Caught && !powered && TissueBelowDoor(game);
            LatchHandle.Locked = !Caught && !Door.AtEnd;
            Door.Locked = Caught || Obstructed || (powered ? Door.AtEnd : Door.Position <= .001f);
            if (!Door.Locked)
            {
                float speed = Vector3.Dot(Door.Body.linearVelocity, Door.WorldAxis);
                float target = powered ? OpenSpeed : -ReturnSpeed;
                float compensation = Door.Gravity ? -Vector3.Dot(Vector3.down * (9.81f * Door.Body.mass), Door.WorldAxis) : 0;
                float force = (target - speed) * 8f + compensation + Mathf.Sign(target) * Door.Resistance;
                Door.ApplyEffort(Door.WorldAxis * Mathf.Clamp(force, -MotorForce, MotorForce));
            }
            if (FinalCover != null) FinalCover.Locked = !Caught;
            Show();
        }

        private bool TissueBelowDoor(VenomCampaign game)
        {
            if (game == null || game.Matter == null) return false;
            float radius = game.Matter.Profile.ParticleRadius + .012f;
            Vector3 centre = Door.Frame.InverseTransformPoint(Door.Body.position);
            float bottom = centre.y - DoorSize.y * .5f;
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
            {
                if (game.Matter.Escaped[i]) continue;
                Vector3 p = Door.Frame.InverseTransformPoint(game.Matter.Bodies[i].position);
                // A narrow real safety edge protects the swept opening, not
                // the entire room; leaving A alone cannot hold the door open.
                if (Mathf.Abs(p.x - centre.x) < DoorSize.x * .5f + radius &&
                    Mathf.Abs(p.z - centre.z) < DoorSize.z * .5f + radius &&
                    p.y > Door.Start.y - DoorSize.y * .5f - radius && p.y < bottom + radius)
                    return true;
            }
            return false;
        }

        private void Show()
        {
            if (CatchPin != null) CatchPin.localPosition = Vector3.right * (Caught ? .014f : 0);
            if (Spring != null) Spring.localScale = new Vector3(1, 1 + Door.Fraction * .7f, 1);
        }
    }
}
