using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Validity of the retained surface graph. A moving door or a changed
    /// aperture invalidates it; repeated commands against identical geometry do not.</summary>
    internal sealed class VenomNavigationRevision
    {
        private struct SurfaceState
        {
            public VenomSurfacePatch patch;
            public Vector3 position,scale;
            public Quaternion rotation;
            public Vector2 size,holeCentre;
            public float radius,sphere;
            public bool active,hole,blocked,colliderEnabled;
        }
        private struct PropState {public Rigidbody body;public Vector3 position;public Quaternion rotation;}
        private SurfaceState[] surfaces;
        private PropState[] props;
        private bool valid;
        public bool Matches(VenomCampaign game)
        {
            if(!valid||surfaces.Length!=game.Surfaces.Length||props.Length!=game.Props.Length)return false;
            var inverse=game.Root.worldToLocalMatrix;var rotation=Quaternion.Inverse(game.Root.rotation);
            for(int i=0;i<surfaces.Length;i++)
            {
                var p=game.Surfaces[i];var s=surfaces[i];
                if(s.patch!=p||s.active!=p.isActiveAndEnabled||s.blocked!=p.NavigationHoleBlocked||s.hole!=p.Hole||s.size!=p.Size||s.holeCentre!=p.HoleCentre||s.radius!=p.HoleRadius||s.sphere!=p.SphereRadius||s.colliderEnabled!=(p.Shape!=null&&p.Shape.enabled)||s.scale!=p.transform.lossyScale)return false;
                if((s.position-inverse.MultiplyPoint3x4(p.transform.position)).sqrMagnitude>0.00005f*0.00005f||Quaternion.Angle(s.rotation,rotation*p.transform.rotation)>.01f)return false;
            }
            for(int i=0;i<props.Length;i++)
            {
                var body=game.Props[i].Body;var s=props[i];
                if(s.body!=body||(s.position-inverse.MultiplyPoint3x4(body.position)).sqrMagnitude>0.00005f*0.00005f||Quaternion.Angle(s.rotation,rotation*body.rotation)>.01f)return false;
            }
            return true;
        }
        public void Capture(VenomCampaign game)
        {
            if(surfaces==null||surfaces.Length!=game.Surfaces.Length)surfaces=new SurfaceState[game.Surfaces.Length];
            if(props==null||props.Length!=game.Props.Length)props=new PropState[game.Props.Length];
            var inverse=game.Root.worldToLocalMatrix;var rotation=Quaternion.Inverse(game.Root.rotation);
            for(int i=0;i<surfaces.Length;i++)
            {
                var p=game.Surfaces[i];surfaces[i]=new SurfaceState{patch=p,position=inverse.MultiplyPoint3x4(p.transform.position),rotation=rotation*p.transform.rotation,scale=p.transform.lossyScale,size=p.Size,holeCentre=p.HoleCentre,radius=p.HoleRadius,sphere=p.SphereRadius,active=p.isActiveAndEnabled,hole=p.Hole,blocked=p.NavigationHoleBlocked,colliderEnabled=p.Shape!=null&&p.Shape.enabled};
            }
            for(int i=0;i<props.Length;i++)
            {var body=game.Props[i].Body;props[i]=new PropState{body=body,position=inverse.MultiplyPoint3x4(body.position),rotation=rotation*body.rotation};}
            valid=true;
        }
    }
}
