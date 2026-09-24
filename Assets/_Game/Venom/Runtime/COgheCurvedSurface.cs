using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Local-space proximity to an authored inward-facing hollow mesh.
    /// Queries never move tissue or add forces. The matching MeshCollider owns physics.</summary>
    public sealed class COgheCurvedSurface : MonoBehaviour
    {
        public Mesh Interior;
        private Mesh cached;
        private Vector3[] vertices;
        private int[] triangles, indices;
        private Bounds[] triangleBounds;
        private Node[] nodes;
        private int used;
        private struct Node { public Bounds Bounds; public int Left, Right, Start, Count; }
        private sealed class AxisComparer : IComparer<int>
        {
            public Bounds[] Bounds; public int Axis;
            public int Compare(int a, int b) => Bounds[a].center[Axis].CompareTo(Bounds[b].center[Axis]);
        }
        public Bounds LocalBounds => Interior.bounds;
        private void Ensure()
        {
            if(cached == Interior && nodes != null) return;
            if(Interior == null) throw new InvalidOperationException("Curved shell requires its interior mesh.");
            vertices=Interior.vertices; triangles=Interior.triangles;
            int count=triangles.Length/3;
            if(count==0) throw new InvalidOperationException("Curved shell has no triangles.");
            indices=new int[count]; triangleBounds=new Bounds[count]; nodes=new Node[count*2]; used=0;
            for(int i=0;i<count;i++)
            {
                indices[i]=i;var bounds=new Bounds(vertices[triangles[i*3]],Vector3.zero);
                bounds.Encapsulate(vertices[triangles[i*3+1]]);bounds.Encapsulate(vertices[triangles[i*3+2]]);triangleBounds[i]=bounds;
            }
            Build(0,count);cached=Interior;
        }
        private int Build(int start,int count)
        {
            int id=used++;var bounds=triangleBounds[indices[start]];
            for(int i=start+1;i<start+count;i++)bounds.Encapsulate(triangleBounds[indices[i]]);
            nodes[id]=new Node{Bounds=bounds,Start=start,Count=count,Left=-1,Right=-1};
            if(count<=8)return id;
            var size=bounds.size;int axis=size.x>size.y?(size.x>size.z?0:2):(size.y>size.z?1:2);
            Array.Sort(indices,start,count,new AxisComparer{Bounds=triangleBounds,Axis=axis});
            int half=count/2;int left=Build(start,half),right=Build(start+half,count-half);
            nodes[id].Left=left;nodes[id].Right=right;nodes[id].Count=0;return id;
        }
        public void Nearest(Vector3 point,out Vector3 closest,out Vector3 inward)
        {
            Ensure();float best=float.PositiveInfinity;closest=Vector3.zero;inward=Vector3.up;
            Search(0,point,ref best,ref closest,ref inward);
        }
        private void Search(int id,Vector3 p,ref float best,ref Vector3 closest,ref Vector3 inward)
        {
            ref var node=ref nodes[id];if(node.Bounds.SqrDistance(p)>best)return;
            if(node.Count>0)
            {
                for(int i=node.Start;i<node.Start+node.Count;i++)
                {
                    int t=indices[i]*3;var a=vertices[triangles[t]];var b=vertices[triangles[t+1]];var c=vertices[triangles[t+2]];
                    var q=ClosestTriangle(p,a,b,c);float distance=(q-p).sqrMagnitude;
                    if(distance>=best)continue;
                    best=distance;closest=q;inward=Vector3.Cross(b-a,c-a).normalized;
                }
                return;
            }
            int first=node.Left,second=node.Right;
            if(nodes[first].Bounds.SqrDistance(p)>nodes[second].Bounds.SqrDistance(p)){int swap=first;first=second;second=swap;}
            Search(first,p,ref best,ref closest,ref inward);Search(second,p,ref best,ref closest,ref inward);
        }
        public bool Contains(Vector3 local,float margin)
        {
            Nearest(local,out var q,out var n);var delta=local-q;float d=Vector3.Dot(delta,n);
            return delta.sqrMagnitude-d*d<=Mathf.Pow(.002f+Mathf.Max(0,margin),2);
        }
        public float DistanceInside(Vector3 local)
        {Nearest(local,out var q,out var n);return Vector3.Dot(local-q,n);}
        public bool Constrain(ref Vector3 local,out Vector3 inward)
        {
            Nearest(local,out var q,out inward);
            float distance=Vector3.Dot(local-q,inward);
            if(distance>=0||distance<-.026f||(local-q).sqrMagnitude>.026f*.026f)return false;
            local=q+inward*.0006f;return true;
        }
        // Voronoi-region closest point, including edges and vertices. No allocation per query.
        public static Vector3 ClosestTriangle(Vector3 p,Vector3 a,Vector3 b,Vector3 c)
        {
            var ab=b-a;var ac=c-a;var ap=p-a;float d1=Vector3.Dot(ab,ap),d2=Vector3.Dot(ac,ap);
            if(d1<=0&&d2<=0)return a;
            var bp=p-b;float d3=Vector3.Dot(ab,bp),d4=Vector3.Dot(ac,bp);if(d3>=0&&d4<=d3)return b;
            float vc=d1*d4-d3*d2;if(vc<=0&&d1>=0&&d3<=0)return a+ab*(d1/(d1-d3));
            var cp=p-c;float d5=Vector3.Dot(ab,cp),d6=Vector3.Dot(ac,cp);if(d6>=0&&d5<=d6)return c;
            float vb=d5*d2-d1*d6;if(vb<=0&&d2>=0&&d6<=0)return a+ac*(d2/(d2-d6));
            float va=d3*d6-d5*d4;if(va<=0&&d4-d3>=0&&d5-d6>=0)return b+(c-b)*((d4-d3)/((d4-d3)+(d5-d6)));
            float denominator=1/(va+vb+vc);return a+ab*(vb*denominator)+ac*(vc*denominator);
        }
    }
}
