using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Visibility graph on the floor. Ports with puzzle effects are obstacles
    /// unless that port was explicitly assigned. Rebuilt only as geometry changes.</summary>
    public sealed class VenomJourneyRoute
    {
        private readonly Vector2[] nodes=new Vector2[50];
        private readonly float[] costs=new float[50];
        private readonly int[] parents=new int[50];
        private readonly bool[] closed=new bool[50];
        public bool Find(Vector3 start,Vector3 goal,List<Rect> obstacles,out Vector3 waypoint)
        {
            Vector2 a=new Vector2(start.x,start.z),b=new Vector2(goal.x,goal.z);
            waypoint=goal;
            // A previously assigned station becomes an obstacle after cancellation.
            // Permit leaving its clearance region, without crossing its centre.
            foreach(var r in obstacles)if(r.Contains(a))
            {
                Vector2 q=a;float best=float.PositiveInfinity;
                foreach(var p in new[]{new Vector2(r.xMin-.012f,a.y),new Vector2(r.xMax+.012f,a.y),new Vector2(a.x,r.yMin-.012f),new Vector2(a.x,r.yMax+.012f)})
                {
                    float d=(p-a).sqrMagnitude+(p-b).sqrMagnitude*.15f;
                    if(Mathf.Abs(p.x)<.245f&&Mathf.Abs(p.y)<.245f&&d<best){best=d;q=p;}
                }
                waypoint=new Vector3(q.x,goal.y,q.y);return best<float.PositiveInfinity;
            }
            nodes[0]=a;nodes[1]=b;int count=2;
            foreach(var r in obstacles)
                foreach(var p in new[]{new Vector2(r.xMin-.008f,r.yMin-.008f),new Vector2(r.xMax+.008f,r.yMin-.008f),new Vector2(r.xMax+.008f,r.yMax+.008f),new Vector2(r.xMin-.008f,r.yMax+.008f)})
                    if(count<nodes.Length&&Mathf.Abs(p.x)<.245f&&Mathf.Abs(p.y)<.245f)nodes[count++]=p;
            for(int i=0;i<count;i++){costs[i]=float.PositiveInfinity;parents[i]=-1;closed[i]=false;}costs[0]=0;
            for(int k=0;k<count;k++)
            {
                int current=-1;float best=float.PositiveInfinity;
                for(int i=0;i<count;i++)if(!closed[i]&&costs[i]<best){best=costs[i];current=i;}
                if(current<0||current==1)break;closed[current]=true;
                for(int i=0;i<count;i++)
                {
                    if(closed[i]||!Clear(nodes[current],nodes[i],obstacles))continue;
                    float cost=best+Vector2.Distance(nodes[current],nodes[i]);
                    if(cost<costs[i]){costs[i]=cost;parents[i]=current;}
                }
            }
            if(parents[1]<0){waypoint=start;return false;}
            int next=1;while(parents[next]>0)next=parents[next];
            waypoint=new Vector3(nodes[next].x,goal.y,nodes[next].y);return true;
        }
        private static bool Clear(Vector2 a,Vector2 b,List<Rect> obstacles)
        {
            foreach(var r in obstacles)
            {
                Vector2 d=b-a;float enter=0,leave=1;bool outside=false;
                for(int axis=0;axis<2;axis++)
                {
                    float min=axis==0?r.xMin:r.yMin,max=axis==0?r.xMax:r.yMax;
                    if(Mathf.Abs(d[axis])<.000001f){if(a[axis]<min||a[axis]>max){outside=true;break;}continue;}
                    float u=(min-a[axis])/d[axis],v=(max-a[axis])/d[axis];if(u>v){float t=u;u=v;v=t;}
                    enter=Mathf.Max(enter,u);leave=Mathf.Min(leave,v);if(enter>leave){outside=true;break;}
                }
                if(!outside)return false;
            }
            return true;
        }
    }
}
