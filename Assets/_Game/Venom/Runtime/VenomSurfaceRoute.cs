using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Routes on an empty six-face shell. The ceiling visibility graph
    /// avoids the aperture until an exit is intended; no chord crosses the interior.</summary>
    public sealed class VenomSurfaceRoute
    {
        private readonly Vector2[] nodes=new Vector2[10];
        private readonly float[] cost=new float[10];
        private readonly int[] parent=new int[10];
        private readonly bool[] closed=new bool[10];
        public static Vector3 Edge(int from,int to,Vector3 a,Vector3 b)
        {
            var n=VenomWallClimb.Normals;Vector3 along=Vector3.Cross(n[from],n[to]);
            return -(n[from]+n[to])*.25f+along*Mathf.Clamp(Vector3.Dot((a+b)*.5f,along),-.16f,.16f);
        }
        public static int NextFace(int from,int to,Vector3 a,Vector3 b)
        {
            var n=VenomWallClimb.Normals;
            if(Vector3.Dot(n[from],n[to])>-.5f)return to;
            float best=float.PositiveInfinity;int result=1;
            for(int f=0;f<6;f++)
            {
                if(Mathf.Abs(Vector3.Dot(n[from],n[f]))>.1f)continue;
                Vector3 entry=Edge(from,f,a,b),exit=Edge(f,to,a,b);
                float length=Vector3.Distance(a,entry)+Vector3.Distance(entry,exit)+Vector3.Distance(exit,b);
                if(length<best){best=length;result=f;}
            }
            return result;
        }
        private static bool Clear(Vector2 a,Vector2 b,bool avoidHole,Rect? obstacle)
        {
            Vector2 d=b-a;
            if(avoidHole)
            {
                float t=d.sqrMagnitude<.000001f?0:Mathf.Clamp01(-Vector2.Dot(a,d)/d.sqrMagnitude);
                if((a+d*t).sqrMagnitude<.115f*.115f)return false;
            }
            if(obstacle.HasValue)
            {
                Rect r=obstacle.Value;float enter=0,leave=1;
                for(int axis=0;axis<2;axis++)
                {
                    float min=axis==0?r.xMin:r.yMin,max=axis==0?r.xMax:r.yMax;
                    if(Mathf.Abs(d[axis])<.000001f){if(a[axis]<min || a[axis]>max)return true;continue;}
                    float first=(min-a[axis])/d[axis],last=(max-a[axis])/d[axis];
                    if(first>last){float swap=first;first=last;last=swap;}
                    enter=Mathf.Max(enter,first);leave=Mathf.Min(leave,last);
                    if(enter>leave)return true;
                }
                return false;
            }
            return true;
        }
        public Vector3 Detour(Vector3 from,Vector3 to,bool avoidHole,Rect? obstacle)
        {
            Vector2 a=new Vector2(from.x,from.z),b=new Vector2(to.x,to.z);
            if(Clear(a,b,avoidHole,obstacle))return to;
            if(obstacle.HasValue && obstacle.Value.Contains(a))
            {
                // The COM may be inside body-clearance inflation while the
                // physical tissue rests against the door edge. Slide along that
                // edge first, rather than asking it to cross the parked cover.
                Rect r=obstacle.Value;Vector2 offset=a-r.center,escape=a;
                if(Mathf.Abs(offset.x)>Mathf.Abs(offset.y))escape.y=offset.y<0?r.yMin-.012f:r.yMax+.012f;
                else escape.x=offset.x<0?r.xMin-.012f:r.xMax+.012f;
                return new Vector3(Mathf.Clamp(escape.x,-.218f,.218f),to.y,Mathf.Clamp(escape.y,-.218f,.218f));
            }
            if(avoidHole && a.magnitude<.116f)
            {
                Vector2 outward=(a.sqrMagnitude>.0001f?a.normalized:Vector2.up)*.15f;
                return new Vector3(outward.x,to.y,outward.y);
            }
            nodes[0]=a;nodes[1]=b;
            nodes[2]=new Vector2(-.15f,-.15f);nodes[3]=new Vector2(.15f,-.15f);
            nodes[4]=new Vector2(.15f,.15f);nodes[5]=new Vector2(-.15f,.15f);
            int count=6;
            if(obstacle.HasValue)
            {
                Rect r=obstacle.Value;
                nodes[6]=new Vector2(r.xMin-.012f,r.yMin-.012f);nodes[7]=new Vector2(r.xMax+.012f,r.yMin-.012f);
                nodes[8]=new Vector2(r.xMax+.012f,r.yMax+.012f);nodes[9]=new Vector2(r.xMin-.012f,r.yMax+.012f);count=10;
            }
            for(int i=0;i<count;i++){cost[i]=float.PositiveInfinity;parent[i]=-1;closed[i]=false;}cost[0]=0;
            for(int visit=0;visit<count;visit++)
            {
                int current=-1;float best=float.PositiveInfinity;
                for(int i=0;i<count;i++)if(!closed[i]&&cost[i]<best){best=cost[i];current=i;}
                if(current<0 || current==1)break;closed[current]=true;
                for(int i=0;i<count;i++)
                {
                    if(closed[i] || i>1 && Mathf.Max(Mathf.Abs(nodes[i].x),Mathf.Abs(nodes[i].y))>.235f || !Clear(nodes[current],nodes[i],avoidHole,obstacle))continue;
                    float length=cost[current]+Vector2.Distance(nodes[current],nodes[i]);
                    if(length<cost[i]){cost[i]=length;parent[i]=current;}
                }
            }
            if(parent[1]<0)return from;
            int next=1;while(parent[next]>0)next=parent[next];
            Vector2 waypoint=nodes[next];
            if(obstacle.HasValue)
            {
                Rect r=obstacle.Value;
                if(a.x>r.xMax && waypoint.x>a.x)waypoint.x=a.x;
                if(a.x<r.xMin && waypoint.x<a.x)waypoint.x=a.x;
            }
            return new Vector3(waypoint.x,to.y,waypoint.y);
        }
    }
}
