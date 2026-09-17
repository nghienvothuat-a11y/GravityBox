using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Interlocked finite-force winch. End catches are determined independently for both real doors.</summary>
    public sealed class COgheCooperativeWinch : COgheMechanism
    {
        public COgheTissueSensor Input, Output;
        public COgheGearTrain Transmission;
        public COgheRailSlider GearCarriage, Handle, FinalCap;
        public COgheRailSlider[] Doors;
        public Transform LockPin, Drum;
        public float HandleForce = .065f, WinchForce = .8f, DoorSpeed = .025f, DrumRadius = .02f;
        public bool Complete { get; private set; }
        public bool Engaged { get; private set; }
        public override bool ControlsExit => true;
        public override bool ExitUnlocked => Complete && (FinalCap == null || FinalCap.AtEnd);
        public override void ResetMechanism(VenomCampaign game)
        {
            Complete = Engaged = false;
            if (FinalCap != null) FinalCap.Locked = true;
            if (GearCarriage != null) GearCarriage.Locked = true;
            foreach (var door in Doors) door.Locked = true;
            Handle.Locked = true;
            Show();
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            bool input = Input != null && Input.Active;
            if (GearCarriage != null) GearCarriage.Locked = !input;
            bool power = input && (Output == null || Output.Active) && (Transmission == null || Transmission.Powered);
            Handle.Locked = Complete || !power;
            // The measured handle effort comes from a nearby attached creature, never from a remote tap.
            Engaged = !Complete && power && Handle.Position > .008f && Handle.Effort >= HandleForce;
            bool allOpen = true;
            foreach (var door in Doors)
            {
                allOpen &= door.AtEnd;
                door.Locked = Complete || !Engaged || door.AtEnd;
                if (Engaged && !door.AtEnd)
                {
                    float speed = Vector3.Dot(door.Body.linearVelocity, door.WorldAxis);
                    float gravity = Mathf.Max(0, -Vector3.Dot(Vector3.down * 9.81f * door.Body.mass, door.WorldAxis));
                    door.ApplyEffort(door.WorldAxis * Mathf.Clamp((DoorSpeed - speed) * 8 + gravity + door.Resistance, 0, WinchForce));
                }
            }
            if (allOpen) Complete = true;
            if (Transmission != null)
            {
                Transmission.HasOutputLoad = Engaged || Complete;
                float speed = 0;
                foreach (var door in Doors) speed += Vector3.Dot(door.Body.linearVelocity,door.WorldAxis) / Doors.Length;
                Transmission.OutputSpeed = Complete ? 0 : -speed / DrumRadius;
            }
            if (FinalCap != null) FinalCap.Locked = !Complete;
            Show();
        }
        private void Show()
        {
            if (LockPin != null) LockPin.localPosition = Vector3.right * (Complete ? .02f : 0);
            if (Drum != null && Doors.Length > 0) Drum.localRotation = Quaternion.Euler(0, 0, Doors[0].Position / .02f * Mathf.Rad2Deg);
        }
    }
}
