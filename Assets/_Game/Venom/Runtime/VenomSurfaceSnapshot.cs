using UnityEngine;

namespace GravityBox.Venom
{
    // Immutable for one query batch. Native Transform/Collider reads happen once
    // per surface, not once per skin vertex or candidate navigation edge.
    internal struct VenomSurfaceSnapshot
    {
        public VenomSurfacePatch Patch;
        public Matrix4x4 ToLocal,ToWorld;
        public Quaternion Rotation;
        public Bounds SkinBounds,ColliderBounds;
        public Vector3 SkinMin,SkinMax;
        public Vector3 ClipMin,ClipMax;
        public Vector3 NavigationMin,NavigationMax,MarginExpansion;
        public bool HasCollider;
        public bool Active;
        public void CaptureContact(VenomSurfacePatch patch)
        {
            Patch=patch;Active=patch.isActiveAndEnabled;ToWorld=patch.transform.localToWorldMatrix;
            Vector3 centre=patch.SphereRadius>0?Vector3.zero:new Vector3(0,0,.017f);
            Vector3 e=patch.SphereRadius>0?Vector3.one*(patch.SphereRadius+.0021f):new Vector3(patch.Size.x*.5f+.0011f,patch.Size.y*.5f+.0011f,.0191f);
            SetBounds(centre,e);
        }
        public void Capture(VenomSurfacePatch patch)
        {
            Patch=patch;ToLocal=patch.transform.worldToLocalMatrix;ToWorld=patch.transform.localToWorldMatrix;
            Rotation=patch.transform.rotation;Active=patch.isActiveAndEnabled;
            HasCollider=patch.Shape!=null;
            ColliderBounds=HasCollider?patch.Shape.bounds:default;
            // Compound boxes and cooked thin panes can report contacts just
            // outside their render-space bound after a physics pose update.
            // This conservative 1 cm pad is rejection-only, never a new wall.
            ColliderBounds.Expand(.02f);
            Vector3 localCentre=patch.SphereRadius>0?Vector3.zero:new Vector3(0,0,-.013f);
            Vector3 e=patch.SphereRadius>0?Vector3.one*(patch.SphereRadius+.0261f):new Vector3(patch.Size.x*.5f,patch.Size.y*.5f,.0131f);
            SetBounds(localCentre,e);
            Vector3 shift=ToWorld.MultiplyVector(Vector3.forward*.0266f);
            if(patch.SphereRadius>0)
            {
                float scale=Mathf.Max(ToWorld.MultiplyVector(Vector3.right).magnitude,Mathf.Max(ToWorld.MultiplyVector(Vector3.up).magnitude,ToWorld.MultiplyVector(Vector3.forward).magnitude));
                ClipMax=Vector3.one*(.0266f*scale);ClipMin=-ClipMax;
            }
            else {ClipMin=Vector3.Min(shift,Vector3.zero);ClipMax=Vector3.Max(shift,Vector3.zero);}
            Vector3 right=ToWorld.MultiplyVector(Vector3.right),up=ToWorld.MultiplyVector(Vector3.up),forward=ToWorld.MultiplyVector(Vector3.forward);
            right=new Vector3(Mathf.Abs(right.x),Mathf.Abs(right.y),Mathf.Abs(right.z));
            up=new Vector3(Mathf.Abs(up.x),Mathf.Abs(up.y),Mathf.Abs(up.z));
            forward=new Vector3(Mathf.Abs(forward.x),Mathf.Abs(forward.y),Mathf.Abs(forward.z));
            MarginExpansion=right+up;
            Vector3 navExtent=patch.SphereRadius>0?(right+up+forward)*(patch.SphereRadius+.0081f):right*(patch.Size.x*.5f)+up*(patch.Size.y*.5f)+forward*.0081f;
            navExtent+=Vector3.one*.00002f;
            Vector3 navCentre=ToWorld.MultiplyPoint3x4(Vector3.zero);NavigationMin=navCentre-navExtent;NavigationMax=navCentre+navExtent;
        }
        private void SetBounds(Vector3 localCentre,Vector3 e)
        {
            Vector3 x=ToWorld.MultiplyVector(new Vector3(e.x,0,0)),y=ToWorld.MultiplyVector(new Vector3(0,e.y,0)),z=ToWorld.MultiplyVector(new Vector3(0,0,e.z));
            SkinBounds=new Bounds(ToWorld.MultiplyPoint3x4(localCentre),new Vector3(Mathf.Abs(x.x)+Mathf.Abs(y.x)+Mathf.Abs(z.x),Mathf.Abs(x.y)+Mathf.Abs(y.y)+Mathf.Abs(z.y),Mathf.Abs(x.z)+Mathf.Abs(y.z)+Mathf.Abs(z.z))*2+Vector3.one*.00002f);
            SkinMin=SkinBounds.min;SkinMax=SkinBounds.max;
        }
        public bool RayBlocked(Ray ray,Vector3 inverseDirection,float length)
        {
            // Bounds is only a rejection test. The original collider remains
            // authoritative, including thickness, holes and disabled shapes.
            return HasCollider&&VenomNavigationSnapshot.Intersects(ColliderBounds,ray.origin,inverseDirection,length)&&Patch.Shape.Raycast(ray,out _,length);
        }
    }
}
