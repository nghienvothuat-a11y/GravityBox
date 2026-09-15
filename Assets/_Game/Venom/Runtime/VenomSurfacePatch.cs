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
        public Vector3 Normal=>transform.forward;
        public bool Contains(Vector3 local,float margin=0)
        {
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
            p.x=Mathf.Clamp(p.x,-Size.x*.5f,Size.x*.5f);p.y=Mathf.Clamp(p.y,-Size.y*.5f,Size.y*.5f);p.z=0;
            Vector2 d=new Vector2(p.x,p.y)-HoleCentre;
            if(Hole&&d.magnitude<HoleRadius)
            {d=(d.sqrMagnitude>.000001f?d.normalized:Vector2.up)*HoleRadius;p.x=HoleCentre.x+d.x;p.y=HoleCentre.y+d.y;}
            return transform.TransformPoint(p);
        }
    }
}
