using UnityEngine;
namespace GravityBox.Venom
{
    public sealed class COgheViewReturnExit:COgheMechanism
    {
        public COgheRailSlider Rail,EntryRail;
        public VenomSurfacePatch Aperture,EntryReverse;
        public override bool ControlsExit=>true;
        public override bool ExitUnlocked=>Rail.Position<=Rail.CatchTolerance;
        public override void ResetMechanism(VenomCampaign game)=>Refresh();
        public override void StepMechanism(VenomCampaign game,float dt)=>Refresh();
        private void Refresh(){Aperture.NavigationHoleBlocked=!ExitUnlocked;EntryReverse.NavigationHoleBlocked=!EntryRail.AtEnd;}
    }
}
