using UnityEngine;

namespace GravityBox.Venom
{
    // Local equipment indicators. This component never changes a puzzle state,
    // issues a movement command, or tells the player the next solution step.
    public sealed class COgheCooperativeBossPresentation : MonoBehaviour
    {
        public COgheCooperativeWinch Winch;
        public Renderer[] Lamps;
        public TextMesh GearState, WinchState, CoverState;
        private MaterialPropertyBlock block;
        private int previous=-1;

        private void LateUpdate()
        {
            if(Winch==null)return;
            bool a=Winch.Input.Active,g=Winch.GearCarriage.AtEnd,b=Winch.Output.Active;
            bool ready=a&&b&&Winch.Transmission.Powered;
            int state=(a?1:0)|(g?2:0)|(b?4:0)|(ready||Winch.Complete?8:0)|
                (Winch.Complete?16:0)|(Winch.Engaged?32:0)|(Winch.FinalCap.AtEnd?64:0);
            if(state==previous)return;
            previous=state;
            if(block==null)block=new MaterialPropertyBlock();
            for(int i=0;i<Lamps.Length;i++)
            {
                if(Lamps[i]==null)continue;
                bool active=(state&(1<<i))!=0;
                block.SetColor("_BaseColor",active?new Color(.32f,.70f,.59f):new Color(.65f,.74f,.77f));
                block.SetColor("_EmissionColor",active?new Color(.10f,.27f,.19f):Color.black);
                Lamps[i].SetPropertyBlock(block);
            }
            if(GearState!=null)GearState.text=g?"DA KHOP":a?"MO KHOA":"KHOA";
            if(WinchState!=null)WinchState.text=Winch.Complete?"DA MO":Winch.Engaged?"DANG MO":ready?"SAN SANG":"KHOA";
            if(CoverState!=null)CoverState.text=Winch.FinalCap.AtEnd?"DA MO":Winch.Complete?"MO KHOA":"KHOA";
        }
    }
}
