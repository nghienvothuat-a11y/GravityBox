using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Seam-free support exists only directly over the physical deck at either terminal.</summary>
    public sealed class COgheTwoDockDeck : COgheMechanism
    {
        public COgheRailSlider Rail;
        public VenomSurfacePatch MovingTop, StartTop, EndTop;
        private int dock = -2;
        public override void ResetMechanism(VenomCampaign game) { dock=-2; Refresh(game); }
        public override void StepMechanism(VenomCampaign game,float dt) { Refresh(game); }
        private void Refresh(VenomCampaign game)
        {
            // Release the seam cover before leaving a dock. Waiting for the
            // carriage to move would trap its side faces in the static cover.
            int next=Rail.Position<=Rail.CatchTolerance && Rail.Effort<=Rail.Resistance+.004f?0:
                Rail.AtEnd && Rail.Effort>=-Rail.Resistance-.004f?1:-1;
            if(next==dock)return;
            dock=next;
            MovingTop.gameObject.SetActive(next<0);
            StartTop.gameObject.SetActive(next==0);
            EndTop.gameObject.SetActive(next==1);
            Physics.SyncTransforms();
            if(game.Motion!=null)game.Motion.BuildGraph(true);
        }
    }
}
