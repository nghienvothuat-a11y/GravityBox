using System.Collections.Generic;
using GravityBox.Venom;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        private static bool BuildExpansion15To16(int number,ExpansionContext c)
        {
            if(number==15){BuildPipeMaze(c);return true;}
            if(number==16){BuildAssemblyBridge(c);return true;}
            return false;
        }

        private static void BuildPipeMaze(ExpansionContext c)
        {
            c.Definition.Title="15 · Tìm lối ra!";
            c.Definition.Lesson="Chọn từng nhánh nối. Đầu cụt luôn cho phép quay lại.";
            c.Definition.CanRotate=true;c.Definition.Passive=false;
            c.Definition.CameraEuler=new Vector3(25,-31,0);c.Definition.ViewRadius=.57f;
            c.Spawn=new Vector3(-.23f,-.255f,-.20f);c.Exit=new Vector3(.3f,.08f,.12f);c.Outward=Vector3.right;
            c.Owner.ApertureRadius=.041f;
            Cube(c.Root,Vector3.zero,.3f,c.Exit,c.Outward,c.Owner.ApertureRadius,c.Surfaces);

            var n=new[]
            {
                new COgheTubeNetwork.Node("Vào",new Vector3(-.235f,-.225f,-.18f),COgheTubeNetwork.TerminalKind.Entry,new Vector3(-.25f,-.96f,-.12f)),
                new COgheTubeNetwork.Node("A",new Vector3(-.14f,-.12f,-.10f)),
                new COgheTubeNetwork.Node("B",new Vector3(-.07f,.075f,-.145f)),
                new COgheTubeNetwork.Node("C",new Vector3(-.015f,-.075f,.12f)),
                new COgheTubeNetwork.Node("D",new Vector3(.095f,.08f,.025f)),
                new COgheTubeNetwork.Node("E",new Vector3(.215f,.08f,.12f)),
                new COgheTubeNetwork.Node("Cụt 1",new Vector3(-.19f,.195f,.11f),COgheTubeNetwork.TerminalKind.Closed),
                new COgheTubeNetwork.Node("Cụt 2",new Vector3(.03f,-.215f,.18f),COgheTubeNetwork.TerminalKind.Closed),
                new COgheTubeNetwork.Node("Cụt 3",new Vector3(.205f,.195f,-.10f),COgheTubeNetwork.TerminalKind.Closed),
                new COgheTubeNetwork.Node("Ra",c.Exit,COgheTubeNetwork.TerminalKind.Exit,c.Outward)
            };
            var e=new[]
            {
                Edge("Vào–A",0,1,n,new Vector3(-.22f,-.19f,-.17f),new Vector3(-.18f,-.16f,-.14f)),
                Edge("A–B",1,2,n,new Vector3(-.16f,-.025f,-.16f),new Vector3(-.12f,.045f,-.18f)),
                Edge("A–C",1,3,n,new Vector3(-.11f,-.15f,.015f),new Vector3(-.06f,-.12f,.095f)),
                // B–D passes behind C–D in projection. Their bores remain over
                // two diameters apart, so the apparent crossing is not a node.
                Edge("B–D",2,4,n,new Vector3(.01f,.075f,-.145f),new Vector3(.045f,.13f,-.045f)),
                Edge("C–D",3,4,n,new Vector3(.065f,-.075f,.12f),new Vector3(.045f,.03f,.095f)),
                Edge("B–Cụt 1",2,6,n,new Vector3(-.07f,.105f,-.065f),new Vector3(-.16f,.17f,.06f)),
                Edge("C–Cụt 2",3,7,n,new Vector3(-.015f,-.075f,.23f),new Vector3(-.005f,-.09f,.265f),
                    new Vector3(.02f,-.15f,.245f),new Vector3(.03f,-.19f,.20f)),
                Edge("D–E",4,5,n,new Vector3(.13f,.08f,.025f),new Vector3(.15f,.08f,.065f),new Vector3(.18f,.08f,.12f)),
                Edge("E–Cụt 3",5,8,n,new Vector3(.215f,.16f,.12f),new Vector3(.24f,.20f,.02f)),
                // Meet the +X wall bore squarely. An oblique final segment drove
                // the trailing tissue into the rim even though the graph route
                // itself was correct.
                Edge("E–Ra",5,9,n,new Vector3(.26f,.08f,.12f))
            };
            TubeNetwork(c.Root,"Interwoven pipe network",n,e,.034f,glass,false).CaptureSurfaceCommandsWhileInside=true;
        }

        private static COgheTubeNetwork.Edge Edge(string name,int a,int b,COgheTubeNetwork.Node[] nodes,params Vector3[] middle)
        {
            var points=new Vector3[middle.Length+2];points[0]=nodes[a].LocalPosition;
            for(int i=0;i<middle.Length;i++)points[i+1]=middle[i];points[points.Length-1]=nodes[b].LocalPosition;
            return new COgheTubeNetwork.Edge(name,a,b,points);
        }

        /// <summary>Shared authoring API used by expansion levels 13 and 20.</summary>
        private static COgheTubeNetwork TubeNetwork(Transform root,string name,COgheTubeNetwork.Node[] nodes,
            COgheTubeNetwork.Edge[] edges,float radius,Material wall,bool flexible=false)
        {
            var go=new GameObject(name,typeof(COgheTubeNetwork));go.transform.SetParent(root,false);
            var network=go.GetComponent<COgheTubeNetwork>();network.Configure(nodes,edges,radius,Mathf.Max(radius*1.8f,radius+.027f));
            for(int i=0;i<edges.Length;i++)
            {
                var edge=edges[i];edge.Flexible|=flexible;
                Vector3[] path=edge.Flexible?(Vector3[])edge.ControlPoints.Clone():COgheTubeNetwork.SampleCurve(edge.ControlPoints,6);
                if(!edge.Flexible)path=TrimJunctionEnds(path,nodes[edge.A].Terminal==COgheTubeNetwork.TerminalKind.Junction,
                    nodes[edge.B].Terminal==COgheTubeNetwork.TerminalKind.Junction,network.JunctionRadius);
                var mesh=new Mesh{name=edge.Name+" bore"};
                COgheTubeNetwork.BuildSweptMesh(path,radius,nodes[edge.B].Terminal==COgheTubeNetwork.TerminalKind.Closed,mesh);
                mesh=Save(mesh);
                var part=new GameObject(edge.Name,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));part.transform.SetParent(go.transform,false);
                var filter=part.GetComponent<MeshFilter>();var collider=part.GetComponent<MeshCollider>();
                filter.sharedMesh=mesh;part.GetComponent<MeshRenderer>().sharedMaterial=wall;collider.sharedMesh=mesh;collider.sharedMaterial=contact;
                edge.Geometry=part.transform;edge.Filter=filter;edge.Collider=collider;
            }
            for(int node=0;node<nodes.Length;node++)
            {
                if(nodes[node].Terminal!=COgheTubeNetwork.TerminalKind.Junction)continue;
                var ports=new List<Vector3[]>();
                foreach(var edge in edges)if(edge.A==node||edge.B==node)
                {
                    Vector3[] sampled=COgheTubeNetwork.SampleCurve(edge.ControlPoints,12);
                    var centreline=new List<Vector3>();
                    if(edge.A==node)
                    {
                        for(int i=0;i<sampled.Length;i++)
                        {centreline.Add(sampled[i]-nodes[node].LocalPosition);if(centreline[centreline.Count-1].magnitude>network.JunctionRadius+radius*1.5f)break;}
                    }
                    else
                    {
                        for(int i=sampled.Length-1;i>=0;i--)
                        {centreline.Add(sampled[i]-nodes[node].LocalPosition);if(centreline[centreline.Count-1].magnitude>network.JunctionRadius+radius*1.5f)break;}
                    }
                    ports.Add(centreline.ToArray());
                }
                var mesh=Save(JunctionMesh(network.JunctionRadius,radius,ports));
                var joint=new GameObject(nodes[node].Name+" junction",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));joint.transform.SetParent(go.transform,false);joint.transform.localPosition=nodes[node].LocalPosition;
                joint.GetComponent<MeshFilter>().sharedMesh=mesh;joint.GetComponent<MeshRenderer>().sharedMaterial=wall;
                joint.GetComponent<MeshCollider>().sharedMesh=mesh;joint.GetComponent<MeshCollider>().sharedMaterial=contact;
                Ring(joint.transform,Vector2.zero,network.JunctionRadius*.72f,.0015f,mint);
            }
            return network;
        }

        private static Vector3[] TrimJunctionEnds(Vector3[] path,bool trimStart,bool trimEnd,float distance)
        {
            var result=new List<Vector3>(path);
            if(trimStart)
            {
                Vector3 origin=result[0];int keep=1;
                while(keep<result.Count-1&&Vector3.Distance(origin,result[keep])<distance)keep++;
                Vector3 near=result[keep-1],far=result[keep];float a=Vector3.Distance(origin,near),b=Vector3.Distance(origin,far);
                Vector3 clipped=Vector3.Lerp(near,far,Mathf.InverseLerp(a,b,distance));result.RemoveRange(0,keep);result.Insert(0,clipped);
            }
            if(trimEnd)
            {
                Vector3 origin=result[result.Count-1];int keep=result.Count-2;
                while(keep>0&&Vector3.Distance(origin,result[keep])<distance)keep--;
                Vector3 near=result[keep+1],far=result[keep];float a=Vector3.Distance(origin,near),b=Vector3.Distance(origin,far);
                Vector3 clipped=Vector3.Lerp(near,far,Mathf.InverseLerp(a,b,distance));result.RemoveRange(keep+1,result.Count-keep-1);result.Add(clipped);
            }
            return result.ToArray();
        }

        private static Mesh JunctionMesh(float sphereRadius,float boreRadius,List<Vector3[]> ports)
        {
            const int rings=20,sectors=40;var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int y=0;y<=rings;y++)for(int x=0;x<=sectors;x++)
            {float v=y/(float)rings*Mathf.PI,u=x/(float)sectors*Mathf.PI*2;vertices.Add(new Vector3(Mathf.Sin(v)*Mathf.Cos(u),Mathf.Cos(v),Mathf.Sin(v)*Mathf.Sin(u))*sphereRadius);}
            bool Open(Vector3 point)
            {
                float opening=boreRadius*1.12f,limit=opening*opening;
                foreach(Vector3[] port in ports)for(int i=0;i<port.Length-1;i++)
                {
                    Vector3 delta=port[i+1]-port[i];float length=delta.sqrMagnitude;
                    float t=length>.000001f?Mathf.Clamp01(Vector3.Dot(point-port[i],delta)/length):0;
                    if((point-(port[i]+delta*t)).sqrMagnitude<limit)return true;
                }
                return false;
            }
            for(int y=0;y<rings;y++)for(int x=0;x<sectors;x++)
            {
                int a=y*(sectors+1)+x,b=a+1,c=a+sectors+1,d=c+1;
                if(!Open((vertices[a]+vertices[c]+vertices[b])/3)){triangles.Add(a);triangles.Add(c);triangles.Add(b);}
                if(!Open((vertices[b]+vertices[c]+vertices[d])/3)){triangles.Add(b);triangles.Add(c);triangles.Add(d);}
            }
            var mesh=new Mesh{name="Open branch chamber"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }

    }
}
