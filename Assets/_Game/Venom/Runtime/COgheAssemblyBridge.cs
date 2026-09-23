using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Observes real seating; never moves pieces or gates an invisible exit.</summary>
    public sealed class COgheAssemblyBridge : COgheMechanism
    {
        public COgheRailSlider[] Rails;
        public int SeatedCount {get;private set;}
        public bool Ready=>Rails!=null&&Rails.Length>0&&SeatedCount==Rails.Length;
        public override void ResetMechanism(VenomCampaign game){SeatedCount=0;}
        public override void StepMechanism(VenomCampaign game,float dt)
        {
            SeatedCount=0;
            foreach(var rail in Rails)if(rail.AtEnd&&rail.Latched)SeatedCount++;
        }
    }
}
