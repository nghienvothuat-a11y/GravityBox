using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Paired lamps describe actual latches; visuals never actuate a mechanism.</summary>
    public sealed class COgheAccessSequencePresentation : MonoBehaviour
    {
        public COgheLatchedAccessSequence Sequence;
        public Renderer[] DoorLamps, TubeLamps;
        private MaterialPropertyBlock block;

        private void LateUpdate()
        {
            if(Sequence==null)return;
            if(block==null)block=new MaterialPropertyBlock();
            Paint(DoorLamps,Sequence.DoorLatched);Paint(TubeLamps,Sequence.TubeLatched);
            if(Sequence.Button!=null&&Sequence.Button.Light!=null)
            {Colour(Sequence.Button.Pressed);Sequence.Button.Light.SetPropertyBlock(block);}
        }

        private void Paint(Renderer[] lamps,bool active)
        {
            if(lamps==null)return;
            Colour(active);
            foreach(var lamp in lamps)if(lamp!=null)lamp.SetPropertyBlock(block);
        }

        private void Colour(bool active)
        {
            Color colour=active?new Color(.32f,.70f,.59f):new Color(.84f,.61f,.32f);
            block.SetColor("_BaseColor",colour);block.SetColor("_EmissionColor",colour*(active?.30f:.02f));
        }
    }
}
