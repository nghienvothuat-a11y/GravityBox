using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    internal static class BossGeometry
    {
        internal static Vector2[] Rectangle(float x,float z)=>new[]{new Vector2(-x,-z),new Vector2(x,-z),new Vector2(x,z),new Vector2(-x,z)};
        internal static Vector2[] Oval(float x,float z,int count=64)
        {var p=new Vector2[count];for(int i=0;i<count;i++){float t=i*Mathf.PI*2/count;p[i]=new Vector2(Mathf.Cos(t)*x,Mathf.Sin(t)*z);}return p;}
        internal static MechanicalAuthoring Author(LevelRuntime l,CampaignBuildContext c)=>new MechanicalAuthoring(l,c.Glass,c.Frame,c.Rim,c.Contact);
        internal static GameObject Mesh(MechanicalAuthoring a,string name,UnityEngine.Mesh mesh,Material material,bool collide=true,Transform parent=null)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent!=null?parent:a.Root,false);
            go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=material;
            go.GetComponent<Renderer>().shadowCastingMode=material==a.Glass?ShadowCastingMode.Off:ShadowCastingMode.On;
            if(collide){var col=go.AddComponent<MeshCollider>();col.sharedMesh=mesh;col.sharedMaterial=a.Contact;col.contactOffset=.0005f;}
            return go;
        }
        internal static LevelRuntime Sphere(string name,float radius,Vector3 spawn,CampaignBuildContext c)
        {
            var root=new GameObject(name,typeof(Rigidbody),typeof(BoxRotationController),typeof(LevelRuntime));
            var rb=root.GetComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;
            var l=root.GetComponent<LevelRuntime>();l.Rotation=root.GetComponent<BoxRotationController>();
            l.InteriorDepth=2*(radius+.006f);l.BoundsHalfExtent=radius+.085f;l.Footprint=Oval(radius,radius);
            var s=new GameObject("BallSpawn");s.transform.SetParent(root.transform,false);s.transform.localPosition=spawn;l.BallSpawn=s.transform;
            var a=Author(l,c);var shell=root.AddComponent<SphericalEnclosure>();shell.InnerRadius=radius;shell.ShellThickness=.006f;
            shell.ShellCollider=Mesh(a,"Continuous inspection sphere",SphereMazeGeometry.Shell(radius,.006f,.023f,name+" shell"),c.Glass).GetComponent<MeshCollider>();
            float inside=Mathf.Sqrt(radius*radius-.023f*.023f),outside=Mathf.Sqrt((radius+.006f)*(radius+.006f)-.023f*.023f);
            var e=new GameObject("Real final bore",typeof(ExitSocket));e.transform.SetParent(root.transform,false);
            e.transform.localPosition=Vector3.down*((inside+outside)*.5f);e.transform.localRotation=Quaternion.LookRotation(Vector3.down,Vector3.forward);
            l.Exit=e.GetComponent<ExitSocket>();l.Exit.ApertureRadius=.023f;l.Exit.WallHalfDepth=(outside-inside)*.5f;
            Mesh(a,"Exit light inlay",PhysicsLabGeometry.Inlay(.023f,l.Exit.WallHalfDepth),c.Rim,false,e.transform);
            return l;
        }
        internal static Renderer Line(MechanicalAuthoring a,string name,Vector3[] points,Material material,float width=.0013f)
        {
            var g=new GameObject(name,typeof(LineRenderer));g.transform.SetParent(a.Root,false);var r=g.GetComponent<LineRenderer>();
            r.useWorldSpace=false;r.sharedMaterial=material;r.positionCount=points.Length;r.SetPositions(points);
            r.startWidth=r.endWidth=width;r.numCornerVertices=2;r.shadowCastingMode=ShadowCastingMode.Off;return r;
        }
        internal static Renderer Arc(MechanicalAuthoring a,string name,Vector3 center,float rx,float rz,Material material,float start=0,float sweep=360)
        {
            var p=new Vector3[49];for(int i=0;i<p.Length;i++){float t=(start+sweep*i/(p.Length-1))*Mathf.Deg2Rad;p[i]=center+new Vector3(Mathf.Cos(t)*rx,0,Mathf.Sin(t)*rz);}return Line(a,name,p,material);
        }
        internal static BossPresentation Present(MechanicalAuthoring a,params BossPresentation.Milestone[] milestones)
        {var p=a.Level.gameObject.AddComponent<BossPresentation>();p.Exit=a.Level.Exit;p.Milestones=milestones;return p;}
        internal static BossPresentation.Milestone Region(string name,Vector3 center,Vector3 size,params Renderer[] lines)
            =>new BossPresentation.Milestone{Name=name,Region=new Bounds(center,size),Inlays=lines};
        internal static void Pocket(MechanicalAuthoring a,string name,Vector3 center,float width=.09f,float length=.10f)
        {
            a.Block(name+" left cheek",center+new Vector3(-width*.5f,0,0),new Vector3(.006f,.054f,length),a.Glass);
            a.Block(name+" right cheek",center+new Vector3(width*.5f,0,0),new Vector3(.006f,.054f,length),a.Glass);
            a.Block(name+" back",center+new Vector3(0,0,length*.5f),new Vector3(width+.006f,.054f,.006f),a.Glass);
            Arc(a,name+" mark",center+Vector3.down*.025f,width*.3f,length*.3f,a.Rim);
        }
        // A union of cube rooms: open faces join adjacent rooms; all other faces
        // are actual panels. This makes recovery remain at the adjacent node.
        internal static void Rooms(MechanicalAuthoring a,IEnumerable<Vector3Int> cells,float pitch,Vector3 offset,
            Vector3Int inlet,Vector3Int outlet,Vector3Int inletNormal,Vector3Int outletNormal)
        {
            var set=new HashSet<Vector3Int>(cells);
            var dirs=new[]{Vector3Int.right,Vector3Int.left,Vector3Int.up,Vector3Int.down,new Vector3Int(0,0,1),new Vector3Int(0,0,-1)};
            foreach(var cell in set) foreach(var d in dirs)
            {
                if(set.Contains(cell+d)||(cell==inlet&&d==inletNormal)||(cell==outlet&&d==outletNormal))continue;
                Vector3 size=Vector3.one*(pitch+.004f);if(d.x!=0)size.x=.004f;if(d.y!=0)size.y=.004f;if(d.z!=0)size.z=.004f;
                Vector3 at=offset+(Vector3)cell*pitch+(Vector3)d*(pitch*.5f);
                a.Block("Route node "+cell+" face "+d,at,size,a.Glass);
                if(d==Vector3Int.down) Line(a,"Rest node edge "+cell,new[]{at+new Vector3(-pitch*.35f,.0025f,0),at+new Vector3(pitch*.35f,.0025f,0)},a.Rim);
            }
        }
    }
}
