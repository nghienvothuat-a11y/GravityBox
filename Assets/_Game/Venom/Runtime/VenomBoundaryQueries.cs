using System;
using UnityEngine;

namespace GravityBox.Venom
{
    // Animation performs many short feeler rays against the same physics pose.
    // Capture once for that batch; exact collider raycasts still decide contact.
    internal sealed class VenomBoundaryQueries
    {
        private Collider[] shapes=Array.Empty<Collider>();
        private Bounds[] bounds=Array.Empty<Bounds>();
        private bool[] active=Array.Empty<bool>();
        public void Capture(Collider[] source)
        {
            shapes=source;
            if(bounds.Length!=source.Length){bounds=new Bounds[source.Length];active=new bool[source.Length];}
            for(int i=0;i<source.Length;i++)
            {
                active[i]=source[i]!=null&&source[i].enabled;
                if(!active[i])continue;
                bounds[i]=source[i].bounds;bounds[i].Expand(.02f);
            }
        }
        public bool Raycast(Vector3 origin,Vector3 direction,float distance,out RaycastHit nearest)
        {
            nearest=default;bool found=false;var ray=new Ray(origin,direction);
            Vector3 d=ray.direction,inverse=new Vector3(1/d.x,1/d.y,1/d.z);
            for(int i=0;i<shapes.Length;i++)
                if(active[i]&&VenomNavigationSnapshot.Intersects(bounds[i],origin,inverse,distance)&&shapes[i].Raycast(ray,out var hit,distance))
                {nearest=hit;distance=hit.distance;found=true;}
            return found;
        }
        public bool SegmentBlocked(Vector3 a,Vector3 b)
        {
            Vector3 delta=b-a;var ray=new Ray(a,delta.normalized);float distance=delta.magnitude;
            Vector3 d=ray.direction,inverse=new Vector3(1/d.x,1/d.y,1/d.z);
            for(int i=0;i<shapes.Length;i++)
                if(active[i]&&VenomNavigationSnapshot.Intersects(bounds[i],a,inverse,distance)&&shapes[i].Raycast(ray,out _,distance))return true;
            return false;
        }
    }
}
