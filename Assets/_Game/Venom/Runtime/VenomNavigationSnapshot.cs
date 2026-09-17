using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    internal sealed class VenomNavigationSnapshot
    {
        private VenomSurfaceSnapshot[] surfaces=Array.Empty<VenomSurfaceSnapshot>();
        private readonly List<Collider> props=new List<Collider>();
        private readonly List<Bounds> propBounds=new List<Bounds>();
        private struct Node {public Vector3 min,max;public int left,right,item;}
        private Node[] tree=Array.Empty<Node>();
        private Bounds[] bounds=Array.Empty<Bounds>();
        private int[] indices=Array.Empty<int>(),stack=Array.Empty<int>();
        private int root=-1,nextNode;
        private const float TreeRadius=.01f;
        private sealed class AxisComparer : IComparer<int>
        {
            public Bounds[] bounds;public int axis;
            public int Compare(int a,int b)
            {int order=bounds[a].center[axis].CompareTo(bounds[b].center[axis]);return order!=0?order:a.CompareTo(b);}
        }
        private readonly AxisComparer comparer=new AxisComparer();
        public void Capture(VenomCampaign game)
        {
            if(surfaces.Length!=game.Surfaces.Length)surfaces=new VenomSurfaceSnapshot[game.Surfaces.Length];
            for(int i=0;i<surfaces.Length;i++)surfaces[i].Capture(game.Surfaces[i]);
            props.Clear();propBounds.Clear();
            foreach(var prop in game.Props)foreach(var shape in prop.CollisionShapes)
            {props.Add(shape);var b=shape.bounds;b.Expand(.02f);propBounds.Add(b);}
            int count=surfaces.Length+props.Count;
            if(bounds.Length<count)
            {
                int capacity=Mathf.NextPowerOfTwo(Mathf.Max(1,count));bounds=new Bounds[capacity];indices=new int[capacity];tree=new Node[capacity*2];stack=new int[capacity*2];
            }
            for(int i=0;i<count;i++)
            {
                indices[i]=i;
                if(i>=surfaces.Length)bounds[i]=propBounds[i-surfaces.Length];
                else
                {
                    ref var surface=ref surfaces[i];var b=new Bounds();
                    b.SetMinMax(surface.NavigationMin-surface.MarginExpansion*TreeRadius,surface.NavigationMax+surface.MarginExpansion*TreeRadius);
                    if(surface.HasCollider)b.Encapsulate(surface.ColliderBounds);bounds[i]=b;
                }
            }
            comparer.bounds=bounds;nextNode=0;root=count>0?Build(0,count):-1;
        }
        private int Build(int start,int count)
        {
            int node=nextNode++;Bounds b=bounds[indices[start]];
            for(int i=start+1;i<start+count;i++)b.Encapsulate(bounds[indices[i]]);
            tree[node]=new Node{min=b.min,max=b.max,item=count==1?indices[start]:-1,left=-1,right=-1};
            if(count==1)return node;
            Vector3 size=b.size;comparer.axis=size.x>=size.y&&size.x>=size.z?0:size.y>=size.z?1:2;
            Array.Sort(indices,start,count,comparer);int half=count/2;
            int left=Build(start,half),right=Build(start+half,count-half);
            tree[node].left=left;tree[node].right=right;return node;
        }
        private static bool Overlap(Vector3 min,Vector3 max,Vector3 a,Vector3 b)
            =>b.x>=min.x&&a.x<=max.x&&b.y>=min.y&&a.y<=max.y&&b.z>=min.z&&a.z<=max.z;
        public bool Occupied(Vector3 world)
        {
            if(root<0)return false;int count=0;stack[count++]=root;
            while(count>0)
            {
                ref var node=ref tree[stack[--count]];
                if(!Overlap(node.min,node.max,world,world))continue;
                if(node.item<0){stack[count++]=node.left;stack[count++]=node.right;continue;}
                if(node.item>=surfaces.Length)continue;
                ref var s=ref surfaces[node.item];if(!s.Active)continue;
                if(world.x<s.NavigationMin.x||world.x>s.NavigationMax.x||world.y<s.NavigationMin.y||world.y>s.NavigationMax.y||world.z<s.NavigationMin.z||world.z>s.NavigationMax.z)continue;
                Vector3 p=s.ToLocal.MultiplyPoint3x4(world);
                float distance=s.Patch.SphereRadius>0?s.Patch.SphereRadius-p.magnitude:p.z;
                if(distance>-.008f&&distance<.008f&&s.Patch.Contains(p))return true;
            }
            return false;
        }
        public bool Clear(Vector3 a,Vector3 b,float radius)
        {
            Vector3 delta=b-a;if(delta.sqrMagnitude<.000001f)return true;
            float length=delta.magnitude-.002f;var ray=new Ray(a,delta.normalized);
            Vector3 direction=ray.direction,inverse=new Vector3(1/direction.x,1/direction.y,1/direction.z);
            Vector3 segmentMin=Vector3.Min(a,b),segmentMax=Vector3.Max(a,b);
            if(radius>TreeRadius)
            {
                // Larger future clearance requests retain the exact full scan.
                for(int i=0;i<surfaces.Length+props.Count;i++)
                    if(Blocked(i,a,b,ray,inverse,length,segmentMin,segmentMax,radius))return false;
                return true;
            }
            if(root<0)return true;int count=0;stack[count++]=root;
            while(count>0)
            {
                ref var node=ref tree[stack[--count]];
                if(!Overlap(node.min,node.max,segmentMin,segmentMax))continue;
                if(node.item<0){stack[count++]=node.left;stack[count++]=node.right;continue;}
                if(Blocked(node.item,a,b,ray,inverse,length,segmentMin,segmentMax,radius))return false;
            }
            return true;
        }
        private bool Blocked(int item,Vector3 a,Vector3 b,Ray ray,Vector3 inverse,float length,Vector3 segmentMin,Vector3 segmentMax,float radius)
        {
            if(item>=surfaces.Length)
            {int i=item-surfaces.Length;return Intersects(propBounds[i],a,inverse,length)&&props[i].Raycast(ray,out _,length);}
            ref var s=ref surfaces[item];if(!s.Active)return false;
            Vector3 margin=s.MarginExpansion*radius,min=s.NavigationMin-margin,max=s.NavigationMax+margin;
            if(s.Patch.SphereRadius<=0&&Overlap(min,max,segmentMin,segmentMax))
            {
                Vector3 x=s.ToLocal.MultiplyPoint3x4(a),y=s.ToLocal.MultiplyPoint3x4(b);
                if(Mathf.Abs(x.z)<.000001f)x=s.Patch.transform.InverseTransformPoint(a);
                if(Mathf.Abs(y.z)<.000001f)y=s.Patch.transform.InverseTransformPoint(b);
                if(Mathf.Abs(x.z-y.z)>.00001f)
                {
                    float t=x.z/(x.z-y.z);
                    if(t>=0&&t<=1&&s.Patch.ContainsForNavigation(Vector3.Lerp(x,y,t),radius))return true;
                }
            }
            return s.RayBlocked(ray,inverse,length);
        }
        internal static bool Intersects(Bounds bounds,Vector3 origin,Vector3 inverse,float length)
        {
            Vector3 min=bounds.min,max=bounds.max;float enter=0,exit=length;
            return Axis(origin.x,inverse.x,min.x,max.x,ref enter,ref exit)&&
                Axis(origin.y,inverse.y,min.y,max.y,ref enter,ref exit)&&
                Axis(origin.z,inverse.z,min.z,max.z,ref enter,ref exit);
        }
        private static bool Axis(float origin,float inverse,float min,float max,ref float enter,ref float exit)
        {
            if(float.IsInfinity(inverse))return origin>=min&&origin<=max;
            float a=(min-origin)*inverse,b=(max-origin)*inverse;
            if(a>b){float temp=a;a=b;b=temp;}
            enter=Mathf.Max(enter,a);exit=Mathf.Min(exit,b);return enter<=exit;
        }
    }
}
