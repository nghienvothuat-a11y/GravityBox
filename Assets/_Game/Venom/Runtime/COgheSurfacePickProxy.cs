using UnityEngine;

namespace GravityBox.Venom
{
    /// Maps a continuous collision mesh to its authored navigation patches.
    /// Physics keeps a seamless mesh; input and feedback use the actual hit segment.
    public sealed class COgheSurfacePickProxy : MonoBehaviour
    {
        public VenomSurfacePatch[] Surfaces;
        public int TrianglesPerSurface=6;
        public bool CentreCommands=true;

        public Vector3 CommandPoint(VenomSurfacePatch patch,Vector3 hit)
        {
            Vector3 point=patch.Closest(hit);
            if(!CentreCommands)return point;
            // A tap on either guard means enter the trough, not crawl over its
            // edge. Preserve the hit's lengthwise position, centre across width.
            Vector3 local=patch.transform.InverseTransformPoint(point);local.x=0;
            return patch.transform.TransformPoint(local);
        }

        public VenomSurfacePatch Resolve(int triangleIndex)
        {
            if(triangleIndex<0||TrianglesPerSurface<1||Surfaces==null)return null;
            int index=triangleIndex/TrianglesPerSurface;
            return index<Surfaces.Length?Surfaces[index]:null;
        }
    }
}
