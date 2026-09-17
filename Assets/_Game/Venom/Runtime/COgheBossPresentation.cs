using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Mechanism feedback reads the simulation; it never controls cuts or latches.</summary>
    public sealed class COgheBossPresentation : MonoBehaviour
    {
        public Transform[] PadCaps;
        public Renderer[] PadLights, WarningLights;
        public Renderer SensorBoundary;
        private VenomCampaign game;
        private MaterialPropertyBlock block;
        private static readonly Color Rest=new Color(.32f,.43f,.49f);
        private static readonly Color Active=new Color(.25f,.83f,.57f);
        private static readonly Color Warning=new Color(1f,.50f,.12f);
        private void Awake(){game=GetComponent<VenomCampaign>();block=new MaterialPropertyBlock();}
        private void LateUpdate()
        {
            if(game==null||game.Owner==null)return;
            for(int i=0;i<PadCaps.Length;i++)
            {
                float mass=i==0?game.MassA:game.MassB;
                float down=Mathf.Clamp01(mass/.012f);
                var p=PadCaps[i].localPosition;
                p.z=Mathf.MoveTowards(p.z,Mathf.Lerp(.007f,.001f,down),Time.deltaTime*.035f);
                PadCaps[i].localPosition=p;
                Glow(PadLights[i],mass>=.012f?Active:Rest,mass>=.012f?.35f:.02f);
            }
            bool warning=game.KnifePhase==VenomCampaign.BladePhase.Warning;
            bool falling=game.KnifePhase==VenomCampaign.BladePhase.Falling;
            for(int i=0;i<WarningLights.Length;i++)
            {
                bool on=falling||warning&&game.KnifeWarningProgress>=(float)i/WarningLights.Length;
                Glow(WarningLights[i],on?Warning:Rest,on?.5f:.02f);
            }
            Glow(SensorBoundary,warning||falling?Warning:Rest,warning||falling?.25f:0);
        }
        private void Glow(Renderer r,Color color,float emission)
        {
            if(r==null)return;
            r.GetPropertyBlock(block);block.SetColor("_BaseColor",color);
            block.SetColor("_EmissionColor",color*emission);r.SetPropertyBlock(block);
        }
    }
}
