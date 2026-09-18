namespace GravityBox.Venom
{
    /// <summary>Exposes an exit only while a visible, physical rail has reached its authored stop.</summary>
    public sealed class COgheExitRailLock : COgheMechanism
    {
        public COgheRailSlider Rail;
        public bool RequireEnd = true;
        public override bool ControlsExit => true;
        public override bool ExitUnlocked => Rail != null && (RequireEnd ? Rail.AtEnd : Rail.Position <= .0018f);
        public override string Activity => ExitUnlocked ? "Lối ra đã mở" : null;
        public override void ResetMechanism(VenomCampaign game) { }
        public override void StepMechanism(VenomCampaign game, float dt) { }
    }
}
