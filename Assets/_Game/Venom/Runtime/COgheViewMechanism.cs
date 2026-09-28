using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>A visible mechanical linkage drives a separate, physical shutter.
    /// No pose or win state is assigned by a tap or by presentation.</summary>
    public sealed class COgheViewMechanism : COgheMechanism
    {
        public COgheRailSlider Input, Output;
        public bool Reverse, FinalGate;
        public VenomSurfacePatch Aperture;
        public Renderer Lamp;
        public Material Waiting, Ready;
        public override bool ControlsExit=>FinalGate;
        public override bool ExitUnlocked=>Output!=null&&(Reverse?Output.Position<=Output.CatchTolerance:Output.AtEnd);
        private bool open;
        public override void ResetMechanism(VenomCampaign game)
        {open=false;UpdateAperture(game,true);}
        public override void StepMechanism(VenomCampaign game,float dt)
        {
            float fraction=Input.AtEnd?1:Input.Position<=Input.CatchTolerance?0:Input.Fraction;
            float target=(Reverse?1-fraction:fraction)*Output.Travel;
            float speed=Vector3.Dot(Output.Body.linearVelocity,Output.WorldAxis);
            Output.ApplyEffort(Output.WorldAxis*Mathf.Clamp((target-Output.Position)*5-speed*.7f,-.45f,.45f));
            UpdateAperture(game,false);
        }
        private void UpdateAperture(VenomCampaign game,bool force)
        {
            bool next=ExitUnlocked;
            if(Lamp!=null)Lamp.sharedMaterial=next?Ready:Waiting;
            if(Aperture!=null)Aperture.NavigationHoleBlocked=!next;
            if(next!=open||force){open=next;if(game.Motion!=null)game.Motion.BuildGraph();}
        }
    }
}
