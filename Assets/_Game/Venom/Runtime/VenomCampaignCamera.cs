using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Presentation only. Cache authored chamber geometry once; project
    /// eight corners per frame. Never query renderers or touch simulation state.</summary>
    public sealed class VenomCampaignCamera
    {
        private readonly VenomCampaign game;
        private readonly COgheStudioBackdrop studioBackdrop;
        private readonly bool spherical;
        private Vector3 focus;
        private bool initialized;
        public Bounds OverviewBounds {get;private set;}
        public int Zone {get;private set;}=-1;
        public int ZoneCount=>game.Definition.CameraZones?.Length??0;
        public bool Inspecting=>game.Zoom||Zone>=0;
        public bool ShowZones=>ZoneCount>0&&!game.Home&&!game.Owner.Completed&&!game.Owner.Lost;

        public VenomCampaignCamera(VenomCampaign game)
        {
            this.game=game;
            studioBackdrop=new COgheStudioBackdrop(game.transform);
            bool found=false;Bounds bounds=default;
            foreach(var surface in game.Surfaces)
            {
                if(surface==null)continue;
                // Dynamic props are already contained by the shell. Ignore their
                // current pose so reset, pushing and opening a door cannot zoom.
                if(surface.Shape!=null&&surface.Shape.attachedRigidbody!=null&&
                    surface.Shape.attachedRigidbody.transform!=game.Root)continue;
                for(int i=0;i<8;i++)
                {
                    Vector3 point=surface.SphereRadius>0?Corner(new Bounds(Vector3.zero,Vector3.one*surface.SphereRadius*2),i):
                        new Vector3((i&1)==0?-surface.Size.x*.5f:surface.Size.x*.5f,
                            (i&2)==0?-surface.Size.y*.5f:surface.Size.y*.5f,0);
                    point=game.Root.InverseTransformPoint(surface.transform.TransformPoint(point));
                    if(!found){bounds=new Bounds(point,Vector3.zero);found=true;}else bounds.Encapsulate(point);
                }
            }
            if(!found)bounds=new Bounds(Vector3.zero,Vector3.one*game.Definition.ViewRadius*1.3f);
            bounds.Encapsulate(game.Root.InverseTransformPoint(game.Owner.Outlet.position));
            bounds.Expand(.036f); // Glass thickness, frame rails and corner sockets.
            OverviewBounds=bounds;
            spherical=game.Surfaces.Length==1&&game.Surfaces[0].SphereRadius>0;
            Reset();
        }
        public void Reset()
        {
            int initial=game.Definition.InitialCameraZone;
            Zone=initial>=0&&initial<ZoneCount?initial:-1;initialized=false;
        }
        public void SelectZone(int zone)
        {
            if(zone< -1||zone>=ZoneCount||game.Home)return;
            Zone=zone;game.Zoom=false;
        }
        public void ToggleFollow()
        {
            if(game.Zoom){game.Zoom=false;Zone=-1;}
            else {game.Zoom=true;Zone=-1;}
        }
        public static Vector3 Corner(Bounds bounds,int index)=>bounds.center+Vector3.Scale(bounds.extents,
            new Vector3((index&1)==0?-1:1,(index&2)==0?-1:1,(index&4)==0?-1:1));

        // Pixel rect, bottom-left origin. Keep the chamber below the title/view
        // selectors and above the rotation legend/fragment/action controls.
        public Rect UsableRect(int width,int height,Rect safeArea)
        {
            float scale=Mathf.Min(width/540f,height/960f);
            float left=Mathf.Max(width*.035f,safeArea.xMin+8*scale);
            float right=Mathf.Min(width*.965f,safeArea.xMax-8*scale);
            float bottom=Mathf.Max(236*scale,safeArea.yMin+12*scale);
            float top=Mathf.Min(height-(ShowZones?249:207)*scale,safeArea.yMax-12*scale);
            return Rect.MinMaxRect(left,bottom,Mathf.Max(left+1,right),Mathf.Max(bottom+1,top));
        }
        public bool AllowsPointer(Vector2 p,int width,int height)
        {
            float scale=Mathf.Min(width/540f,height/960f);
            return p.y>165*scale&&p.y<height-(ShowZones?244:195)*scale;
        }
        public void Frame(int width,int height,float dt,bool snap=false,Rect? safeArea=null)
        {
            width=Mathf.Max(1,width);height=Mathf.Max(1,height);
            var camera=game.Owner.View;
            camera.orthographic=true;camera.aspect=(float)width/height;
            Vector3 heading=Zone>=0&&game.Definition.CameraZones[Zone].OverrideCameraEuler
                ?game.Definition.CameraZones[Zone].CameraEuler:game.Definition.CameraEuler;
            camera.transform.rotation=Quaternion.Euler(heading);
            Rect usable=UsableRect(width,height,safeArea??new Rect(0,0,width,height));
            Vector3 right=camera.transform.right,up=camera.transform.up;
            Vector3 target;float halfWidth,halfHeight;
            if(game.Zoom)
            {
                target=game.Motion.Centre(game.Motion.Selected);
                halfWidth=.145f;halfHeight=.18f;
            }
            else
            {
                Bounds bounds=Zone>=0?game.Definition.CameraZones[Zone].LocalBounds:OverviewBounds;
                target=game.Root.TransformPoint(bounds.center);
                halfWidth=halfHeight=0;
                if(spherical&&Zone<0)halfWidth=halfHeight=bounds.extents.x;
                else for(int i=0;i<8;i++)
                {
                    Vector3 delta=game.Root.TransformPoint(Corner(bounds,i))-target;
                    halfWidth=Mathf.Max(halfWidth,Mathf.Abs(Vector3.Dot(delta,right)));
                    halfHeight=Mathf.Max(halfHeight,Mathf.Abs(Vector3.Dot(delta,up)));
                }
            }
            float targetSize=Mathf.Max(halfWidth/(camera.aspect*usable.width/width),halfHeight/(usable.height/height));
            float blend=1-Mathf.Exp(-Mathf.Max(0,dt)*10);
            if(snap||!initialized){focus=target;camera.orthographicSize=targetSize;initialized=true;}
            else
            {
                focus=Vector3.Lerp(focus,target,blend);
                camera.orthographicSize=Mathf.Lerp(camera.orthographicSize,targetSize,blend);
                // A rotating overview must never crop the real corners while
                // catching up with its wider silhouette. Inspection transitions
                // still blend in both directions.
                if(!game.Zoom&&Zone<0)camera.orthographicSize=Mathf.Max(camera.orthographicSize,targetSize);
            }
            Vector2 offset=usable.center/new Vector2(width,height)-Vector2.one*.5f;
            Vector3 framingOffset=right*(offset.x*2*camera.orthographicSize*camera.aspect)+up*(offset.y*2*camera.orthographicSize);
            studioBackdrop.Fit(camera,focus-framingOffset);
        }
    }
}
