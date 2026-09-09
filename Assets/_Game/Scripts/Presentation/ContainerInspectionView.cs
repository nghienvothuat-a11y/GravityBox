using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Presentation
{
    // Optical underside inspection. The floor collider always remains present.
    public sealed class ContainerInspectionView : MonoBehaviour
    {
        private Renderer floor;
        private Material original, inspection;
        private Camera view;
        private MaterialPropertyBlock properties;
        private Color color;
        public void Initialize(Renderer renderer, Camera camera)
        {
            // Native rendering objects must be created on the main thread after
            // AddComponent, never in a MonoBehaviour constructor/field initializer.
            properties = new MaterialPropertyBlock();
            floor = renderer; view = camera;
            if (floor == null || floor.sharedMaterial == null) return;
            original = floor.sharedMaterial;
            inspection = new Material(original) { name = original.name + " inspection instance" };
            color = inspection.GetColor("_BaseColor");
            inspection.SetFloat("_Surface", 1); inspection.SetFloat("_Blend", 0);
            inspection.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            inspection.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            inspection.SetFloat("_ZWrite", 0); inspection.SetFloat("_Cull", (float)CullMode.Back);
            inspection.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            inspection.renderQueue = (int)RenderQueue.Transparent - 30;
            inspection.SetShaderPassEnabled("ShadowCaster", false);
        }
        private void LateUpdate()
        {
            if (floor == null || inspection == null) return;
            if (view == null) view = Camera.main;
            if (view == null) return;
            float facing = Vector3.Dot(-transform.up, (view.transform.position - floor.bounds.center).normalized);
            float alpha = Mathf.Lerp(1, .12f, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-.2f, .45f, facing)));
            // The panel is a closed mesh. Keep its ordinary depth-writing material
            // while opaque, and cull the far face while inspecting through it.
            floor.sharedMaterial = alpha >= .999f ? original : inspection;
            floor.GetPropertyBlock(properties); Color shown = color; shown.a = alpha;
            properties.SetColor("_BaseColor", shown); floor.SetPropertyBlock(properties);
        }
        private void OnDestroy()
        {
            if (floor != null && floor.sharedMaterial == inspection) floor.sharedMaterial = original;
            if (inspection != null) Destroy(inspection);
        }
    }
}
