using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Two held-load outputs with real end catches and a retracting selector stop.</summary>
    public sealed class COgheTwoStageWinch : COgheMechanism
    {
        public COgheTissueSensor Input, Output;
        public COgheRailSlider Handle, SelectorStop, SecondOutput, FinalCap;
        public COgheRailSlider PreparationCarriage, PreparationPin;
        public COgheGearTrain PreparationTransmission;
        public COgheRailSlider[] ReunionDoors;
        public Transform FirstDrum, SecondDrum;
        public float HandleForce = .050f, DoorSpeed = .10f, DriveForce = .8f;
        public float SelectorSpring = .25f, SelectorBand = .018f;
        public bool ReunionComplete { get; private set; }
        public bool Complete { get; private set; }
        public bool Engaged { get; private set; }
        public int ActiveOutput { get; private set; }
        public bool Prepared => PreparationCarriage == null ||
            PreparationCarriage.AtEnd && PreparationPin != null && PreparationPin.AtEnd;
        public bool StageTwoEnabled => ReunionComplete && SelectorStop != null && SelectorStop.AtEnd;
        public override bool ControlsExit => true;
        public override bool ExitUnlocked => Complete && (FinalCap == null || FinalCap.AtEnd);

        public override void ResetMechanism(VenomCampaign game)
        {
            ReunionComplete = Complete = Engaged = false;
            ActiveOutput = 0;
            foreach (var door in ReunionDoors) door.Locked = true;
            SelectorStop.Locked = SecondOutput.Locked = true;
            Handle.Locked = false;
            if (FinalCap != null) FinalCap.Locked = true;
            Show();
        }

        public override void StepMechanism(VenomCampaign game, float dt)
        {
            bool powered = Prepared && Input != null && Input.Active && Output != null && Output.Active &&
                (PreparationTransmission == null || PreparationTransmission.Powered);
            bool first = powered && !ReunionComplete && Handle.Position <= SelectorBand && Handle.Effort <= -HandleForce;
            bool second = powered && StageTwoEnabled && !Complete &&
                Handle.Position >= Handle.Travel - SelectorBand && Handle.Effort >= HandleForce;
            ActiveOutput = first ? 1 : second ? 2 : 0;
            Engaged = ActiveOutput != 0;
            bool allOpen = ReunionDoors.Length > 0;
            foreach (var door in ReunionDoors)
            {
                Drive(door, first, DoorSpeed);
                allOpen &= door.AtEnd;
            }
            if (allOpen) ReunionComplete = true;
            // The stop rises only after BOTH measured door end catches. It is
            // a colliding rail, so an early pull cannot enter output II.
            Drive(SelectorStop, ReunionComplete, DoorSpeed);
            Drive(SecondOutput, second, DoorSpeed);
            if (SecondOutput.AtEnd) Complete = true;
            if (FinalCap != null) FinalCap.Locked = !Complete;
            if (PreparationTransmission != null)
            {
                float loadSpeed = 0;
                if (first) foreach (var door in ReunionDoors)
                    loadSpeed += Vector3.Dot(door.Body.linearVelocity, door.WorldAxis) / ReunionDoors.Length;
                if (second) loadSpeed = Vector3.Dot(SecondOutput.Body.linearVelocity, SecondOutput.WorldAxis);
                PreparationTransmission.HasOutputLoad = true;
                PreparationTransmission.OutputSpeed = -loadSpeed / PreparationTransmission.PitchRadii[1];
            }
            // Unattended C returns toward neutral. The spring's maximum force
            // is below HandleForce; its own effort cannot operate either load.
            float restoring = Mathf.Clamp((Handle.Travel * .5f - Handle.Position) * SelectorSpring, -HandleForce * .4f, HandleForce * .4f);
            Handle.ApplyEffort(Handle.WorldAxis * restoring);
            Show();
        }

        private void Drive(COgheRailSlider rail, bool powered, float speedTarget)
        {
            rail.Locked = !powered || rail.AtEnd;
            if (!powered || rail.AtEnd) return;
            float speed = Vector3.Dot(rail.Body.linearVelocity, rail.WorldAxis);
            float gravity = rail.Gravity ? Mathf.Max(0, -Vector3.Dot(Vector3.down * 9.81f * rail.Body.mass, rail.WorldAxis)) : 0;
            rail.ApplyEffort(rail.WorldAxis * Mathf.Clamp((speedTarget - speed) * 8 + gravity + rail.Resistance, 0, DriveForce));
        }

        private void Show()
        {
            if (FirstDrum != null && ReunionDoors.Length > 0)
                FirstDrum.localRotation = Quaternion.Euler(0, 0, ReunionDoors[0].Position / .02f * Mathf.Rad2Deg);
            if (SecondDrum != null && SecondOutput != null)
                SecondDrum.localRotation = Quaternion.Euler(0, 0, SecondOutput.Position / .02f * Mathf.Rad2Deg);
        }
    }
}
