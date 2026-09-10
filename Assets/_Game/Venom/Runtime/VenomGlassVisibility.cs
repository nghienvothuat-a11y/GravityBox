using UnityEngine;

namespace GravityBox.Venom
{
    public sealed class VenomGlassVisibility : MonoBehaviour
    {
        public VenomLevelController Level;
        private MaterialPropertyBlock block;
        private void LateUpdate()=>Refresh();
        public void Refresh()
        {
            if(Level==null || Level.CrawlFaces==null)return;
            if(block==null)block=new MaterialPropertyBlock();
            for(int i=0;i<Level.CrawlFaces.Length;i++)
            {
                var shape=Level.CrawlFaces[i];var renderer=shape.GetComponent<Renderer>();
                Vector3 outward=-Level.Rotation.transform.TransformDirection(VenomWallClimb.Normals[i]);
                float facing=Vector3.Dot(outward,(Level.View.transform.position-shape.bounds.center).normalized);
                // The near pane becomes faint; far panes retain enough tint to
                // read the volume. Every face keeps its physical collider.
                Color color=new Color(.35f,.62f,.64f,Mathf.Lerp(.15f,.025f,Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.15f,.3f,facing))));
                block.SetColor("_BaseColor",color);renderer.SetPropertyBlock(block);
            }
        }
    }
}
