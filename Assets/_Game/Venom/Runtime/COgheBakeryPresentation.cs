using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// Bakery review level look at run time (presentation only): the studio table and the camera clear take the backdrop
    /// colour. The studio is shared by every level, so it is tinted per renderer here instead of changing its material.
    /// </summary>
    public sealed class COgheBakeryPresentation : MonoBehaviour
    {
        public Color Backdrop=new Color(.95f,.71f,.61f);
        private void Start()
        {
            var owner=GetComponent<VenomLevelController>();
            if(owner!=null&&owner.View!=null){owner.View.clearFlags=CameraClearFlags.SolidColor;owner.View.backgroundColor=Backdrop;}
            var studio=transform.Find("Day Lab studio");if(studio==null)return;
            var block=new MaterialPropertyBlock();
            foreach(var r in studio.GetComponentsInChildren<Renderer>(true))
            {
                if(r.name.Contains("shadow")||r.name.Contains("Shadow")){r.enabled=false;continue;}
                r.GetPropertyBlock(block);block.SetColor("_BaseColor",Backdrop*.93f);block.SetColor("_Color",Backdrop*.93f);r.SetPropertyBlock(block);
            }
        }
    }
}
