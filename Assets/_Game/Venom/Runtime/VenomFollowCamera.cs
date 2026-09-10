using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Composes a smooth close-follow view over the existing overview.
    /// Heading stays fixed, so zooming cannot reverse screen-relative controls.</summary>
    public sealed class VenomFollowCamera
    {
        private readonly VenomLevelController level;
        private Vector3 focus;
        private bool hasFocus;
        private float blend;
        public bool Zoomed {get;private set;}
        public float Blend=>blend;
        public VenomFollowCamera(VenomLevelController owner){level=owner;}
        public void Toggle(){Zoomed=!Zoomed;}
        public void Reset(){Zoomed=false;blend=0;hasFocus=false;}
        public void Frame(int width,int height,float dt)
        {
            // VenomCameraFraming.Frame has already set the exact overview.
            var camera=level.View;
            Vector3 centre=Vector3.zero;int count=0;
            var selected=level.Locomotion.Selected;
            for(int i=0;i<CohesiveOrganism.ParticleCount;i++)
            {
                if(selected!=null && (level.Organism.Escaped[i] || level.Organism.Groups[i]!=selected.Group))continue;
                centre+=level.Organism.Bodies[i].position;count++;
            }
            if(count==0)return;
            centre/=count;
            dt=Mathf.Clamp(dt,0,.1f);
            if(!hasFocus){focus=centre;hasFocus=true;}
            else focus=Vector3.Lerp(focus,centre,1-Mathf.Exp(-dt*18));
            blend=Mathf.MoveTowards(blend,Zoomed?1:0,dt/.45f);
            float eased=Mathf.SmoothStep(0,1,blend);
            float scale=VenomCameraFraming.UiScale(level,width,height);
            float bottom=282*scale,top=height-205*scale;
            // Keep a useful close view on both portrait and resized Mac windows.
            float size=Mathf.Max(.12f*height/Mathf.Max(80*scale,top-bottom),.12f*height/(width*.92f));
            Vector3 position=focus-camera.transform.forward*.42f-camera.transform.up*(((bottom+top)/height-1)*size);
            camera.transform.position=Vector3.Lerp(camera.transform.position,position,eased);
            camera.orthographicSize=Mathf.Lerp(camera.orthographicSize,size,eased);
        }
    }
}
