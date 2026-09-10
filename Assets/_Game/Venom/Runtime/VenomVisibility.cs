using UnityEngine;

namespace GravityBox.Venom
{
    // Looking through the underside must not hide the organism. Only opacity changes.
    public sealed class VenomVisibility : MonoBehaviour
    {
        public Renderer Floor;
        public Camera View;
        private MaterialPropertyBlock block;
        private Color color;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            color = Floor.sharedMaterial.GetColor("_BaseColor");
        }

        private void LateUpdate()
        {
            float facing = Vector3.Dot(Floor.transform.up,
                (View.transform.position - Floor.transform.position).normalized);
            color.a = Mathf.Lerp(.10f, 1, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-.1f, .35f, facing)));
            block.SetColor("_BaseColor", color);
            Floor.SetPropertyBlock(block);
        }
    }
}
