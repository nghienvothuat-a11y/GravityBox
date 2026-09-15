using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    public sealed class VenomSurfacePatch : MonoBehaviour
    {
        public Vector2 Size=Vector2.one*.6f;
        public bool Hole, Slippery, Selectable=true, RingGrip;
        public bool InterceptExterior;
        public Vector2 HoleCentre;
        public float HoleRadius=.039f, GripRadius=.08f;
        public bool HasSlipRegion;
        public Rect SlipRegion;
        public Collider Shape;
        public float SphereRadius;
        public Vector3 Normal=>transform.forward;
        public Vector3 NormalAt(Vector3 world)=>SphereRadius>0?(transform.position-world).normalized:Normal;
        public float DistanceInside(Vector3 world)
        {var p=transform.InverseTransformPoint(world);return SphereRadius>0?SphereRadius-p.magnitude:p.z;}
        public bool Contains(Vector3 local,float margin=0)
        {
            if(SphereRadius>0)return !Hole||local.y<=0||new Vector2(local.x,local.z).magnitude>=HoleRadius-margin;
            return Mathf.Abs(local.x)<=Size.x*.5f+margin && Mathf.Abs(local.y)<=Size.y*.5f+margin &&
                (!Hole || Vector2.Distance(new Vector2(local.x,local.y),HoleCentre)>=HoleRadius-margin);
        }
        public bool Grip(Vector3 world)
        {
            Vector3 p=transform.InverseTransformPoint(world);
            bool slick=Slippery || HasSlipRegion&&SlipRegion.Contains(new Vector2(p.x,p.y));
            return !slick || RingGrip&&Vector2.Distance(new Vector2(p.x,p.y),HoleCentre)<GripRadius;
        }
        public Vector3 Closest(Vector3 world)
        {
            Vector3 p=transform.InverseTransformPoint(world);
            if(SphereRadius>0)
            {
                p=(p.sqrMagnitude>0?p.normalized:Vector3.down)*SphereRadius;
                if(!Contains(p))
                {
                    Vector2 rim=new Vector2(p.x,p.z);rim=(rim.sqrMagnitude>0?rim.normalized:Vector2.right)*HoleRadius;
                    p=new Vector3(rim.x,Mathf.Sqrt(SphereRadius*SphereRadius-HoleRadius*HoleRadius),rim.y);
                }
                return transform.TransformPoint(p);
            }
            p.x=Mathf.Clamp(p.x,-Size.x*.5f,Size.x*.5f);p.y=Mathf.Clamp(p.y,-Size.y*.5f,Size.y*.5f);p.z=0;
            Vector2 d=new Vector2(p.x,p.y)-HoleCentre;
            if(Hole&&d.magnitude<HoleRadius)
            {d=(d.sqrMagnitude>.000001f?d.normalized:Vector2.up)*HoleRadius;p.x=HoleCentre.x+d.x;p.y=HoleCentre.y+d.y;}
            return transform.TransformPoint(p);
        }
        public bool PickSphere(Ray ray,out Vector3 point)
        {
            point=Vector3.zero;if(SphereRadius<=0)return false;
            Vector3 origin=transform.InverseTransformPoint(ray.origin),direction=transform.InverseTransformDirection(ray.direction).normalized;
            float projection=Vector3.Dot(origin,direction),discriminant=projection*projection-origin.sqrMagnitude+SphereRadius*SphereRadius;
            if(discriminant<0)return false;
            float distance=-projection-Mathf.Sqrt(discriminant);if(distance<0)return false;
            Vector3 p=origin+direction*distance;if(!Contains(p))return false;
            point=transform.TransformPoint(p);return true;
        }
    }
}
