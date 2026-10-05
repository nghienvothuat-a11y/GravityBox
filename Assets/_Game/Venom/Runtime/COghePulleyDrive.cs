using UnityEngine;
namespace GravityBox.Venom
{
    /// <summary>Tension-only elastic cable between two constrained carriages, with a terminal pawl.</summary>
    public sealed class COghePulleyDrive : COgheMechanism
    {
        public COgheRailSlider Input, Output;
        public COgheTapRail Command;
        public Transform[] Guides;
        public Transform Drum,DrumAnchor;
        public Transform[] Wheels;
        public LineRenderer Cable;
        public float Stiffness=45, Damping=.12f, MaximumTension=1.2f;
        public float Tension { get; private set; }
        public override void ResetMechanism(VenomCampaign game){Tension=0;}
        public override void StepMechanism(VenomCampaign game,float dt)
        {
            bool reversing=Command!=null&&Command.Busy&&Command.RequestedPosition<Input.Position-.01f;
            if(Input.AtEnd)Input.Locked=true;
            if(reversing){Input.Locked=false;Output.LatchAtEnd=false;Output.ReleaseLatch();}
            else Output.LatchAtEnd=true;
            float a=Vector3.Dot(Input.Body.linearVelocity,Input.WorldAxis),b=Vector3.Dot(Output.Body.linearVelocity,Output.WorldAxis);
            Tension=Mathf.Clamp((Input.Position-Output.Position)*Stiffness+(a-b)*Damping,0,MaximumTension);
            Input.ApplyEffort(-Input.WorldAxis*Tension);Output.ApplyEffort(Output.WorldAxis*Tension);
        }
        private void LateUpdate()
        {
            if(Cable==null||Input==null||Output==null)return;
            Cable.positionCount=Guides.Length+2;Cable.SetPosition(0,DrumAnchor!=null?DrumAnchor.position:Input.Body.transform.position+Vector3.up*.035f);
            if(Drum!=null)Drum.localRotation=Quaternion.Euler(90,Input.ShownPosition/.024f*Mathf.Rad2Deg,0);
            if(Wheels!=null)foreach(var wheel in Wheels)wheel.localRotation=Quaternion.Euler(90,Output.ShownPosition/.027f*Mathf.Rad2Deg,0);
            for(int i=0;i<Guides.Length;i++)Cable.SetPosition(i+1,Guides[i].position);
            Cable.SetPosition(Guides.Length+1,Output.Body.transform.position+Vector3.up*.022f);
        }
    }
}
