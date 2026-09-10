using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// Read-only surface limits shared by the slider end stops and rendered skin.
    /// The authored mesh remains the physical floor, including its circular bore.
    /// </summary>
    public sealed class VenomFloorBoundary : MonoBehaviour
    {
        private Bounds slab;
        private Vector2 hole;
        private float apertureSquared;
        private bool hasOpening;
        public float Top => slab.max.y;
        public float Bottom => slab.min.y;

        public void Initialize(MeshCollider source,Transform outlet,float radius,bool opening=true)
        {
            slab=source.sharedMesh.bounds;hasOpening=opening;
            Vector3 localOutlet=transform.InverseTransformPoint(outlet.position);
            hole=new Vector2(localOutlet.x,localOutlet.z);
            // The rendered circle is inscribed; keep the cap just inside its rim.
            float inset=radius*.9994f-.0002f;apertureSquared=inset*inset;
        }
        public bool OverSolid(Vector3 local)
        {
            if(local.x<slab.min.x || local.x>slab.max.x || local.z<slab.min.z || local.z>slab.max.z) return false;
            return !hasOpening || new Vector2(local.x-hole.x,local.z-hole.y).sqrMagnitude>apertureSquared;
        }
    }
}
