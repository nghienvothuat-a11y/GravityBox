using UnityEngine;

namespace GravityBox.Venom
{
    [DefaultExecutionOrder(100)]
    public sealed class COgheViewPresentation:MonoBehaviour
    {
        public VenomSurfacePatch[] Panes;
        public Transform[] PaneVisuals;
        private Renderer[][] renderers;
        private VenomCampaign game;
        private void Awake()
        {
            game=GetComponent<VenomCampaign>();renderers=new Renderer[Panes.Length][];
            for(int i=0;i<Panes.Length;i++)renderers[i]=PaneVisuals!=null&&i<PaneVisuals.Length?PaneVisuals[i].GetComponentsInChildren<Renderer>():new[]{Panes[i].GetComponent<Renderer>()};
        }
        private void LateUpdate()
        {
            if(game.Owner==null||game.Home||game.Owner.Completed)return;
            for(int i=0;i<Panes.Length;i++)
                foreach(var renderer in renderers[i])renderer.enabled=Vector3.Dot(game.Owner.View.transform.forward,Panes[i].Normal)<-.001f;
        }
    }
}
