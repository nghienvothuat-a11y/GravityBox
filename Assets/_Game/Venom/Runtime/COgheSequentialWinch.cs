using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>A physical access cover followed by a measured handle and latched shutter.</summary>
    public sealed class COgheSequentialWinch : COgheMechanism
    {
        public COgheRailSlider Access, Handle, Door;
        public Transform Drum, LockPin;
        public float HandleForce = .050f, DoorSpeed = .055f, WinchForce = .8f;
        public bool Complete { get; private set; }
        public bool AccessOpen => Access == null || Access.AtEnd;
        public bool Engaged { get; private set; }
        public override bool ControlsExit => true;
        public override bool ExitUnlocked => Complete;
        public override string Activity => Engaged ? "Đang kéo cửa" : AccessOpen && !Complete ? "Tay kéo đã lộ" : null;

        public override void ResetMechanism(VenomCampaign game)
        {
            Complete = Engaged = false;
            if (Handle != null) Handle.Locked = !AccessOpen;
            if (Door != null) Door.Locked = true;
            Show();
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            if (Handle == null || Door == null) return;
            Handle.Locked = Complete || !AccessOpen;
            Engaged = !Complete && AccessOpen && Handle.Position > .006f && Handle.Effort >= HandleForce;
            Door.Locked = Complete || !Engaged || Door.AtEnd;
            if (Engaged && !Door.AtEnd)
            {
                float speed = Vector3.Dot(Door.Body.linearVelocity, Door.WorldAxis);
                float gravity = Mathf.Max(0, -Vector3.Dot(Vector3.down * 9.81f * Door.Body.mass, Door.WorldAxis));
                Door.ApplyEffort(Door.WorldAxis * Mathf.Clamp((DoorSpeed - speed) * 8 + gravity + Door.Resistance, 0, WinchForce));
            }
            if (Door.AtEnd) Complete = true;
            Show();
        }
        private void Show()
        {
            if (LockPin != null) LockPin.localPosition = Vector3.right * (Complete ? .020f : 0);
            if (Drum != null && Door != null) Drum.localRotation = Quaternion.Euler(0, 0, Door.Position / .020f * Mathf.Rad2Deg);
        }
    }
}
