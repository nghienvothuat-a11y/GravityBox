using System.Collections.Generic;
using GravityBox.Gameplay;
using UnityEngine;

namespace GravityBox.Editor
{
    // A continuous gravity-fed trough ends in real empty space. The receiving
    // court is offset sideways, so a level, stationary box misses the landing.
    internal static class FlightCatchBuilder
    {
        public static readonly Vector3 Spawn = new Vector3(-.297f, .231f, -.075f);
        public static LevelRuntime Build(Material glass, Material frame, Material rim, PhysicsMaterial contact)
        {
            var a = new MechanicalAuthoring("Flight catch", new Vector3(.35f,.29f,.27f),
                new Vector2(.225f,.120f), Spawn, glass, frame, rim, contact);
            const int segments = 32;
            var path = new List<Vector2> { new Vector2(-.321f,.210f), new Vector2(-.285f,.210f) };
            for (int i=1;i<=segments;i++)
            {
                float t=i/(float)segments, t2=t*t, t3=t2*t;
                path.Add((2*t3-3*t2+1)*new Vector2(-.285f,.210f)
                    +(t3-2*t2+t)*new Vector2(.265f,-.230f)
                    +(-2*t3+3*t2)*new Vector2(-.020f,.075f)
                    +(t3-t2)*new Vector2(.265f,.018f));
            }
            a.MeshObject("Continuous curved takeoff trough", Ribbon(path,-.107f,-.043f,0,-.006f),frame);
            a.MeshObject("Takeoff left clear rail",Ribbon(path,-.115f,-.107f,.035f,-.006f),glass);
            a.MeshObject("Takeoff right clear rail",Ribbon(path,-.043f,-.035f,.035f,-.006f),glass);
            a.Block("Launch pocket back",new Vector3(-.324f,.2485f,-.075f),new Vector3(.006f,.077f,.080f),glass);
            a.Block("Recovery cup left cheek",new Vector3(-.304f,.264f,-.111f),new Vector3(.040f,.046f,.008f),glass);
            a.Block("Recovery cup right cheek",new Vector3(-.304f,.264f,-.039f),new Vector3(.040f,.046f,.008f),glass);
            // Every entrance from the recovery floor is physically closed. Entry
            // is over a 245 mm wall, through the broad unobstructed mouth above it.
            a.Block("Receiver front wall",new Vector3(.190f,-.166f,-.019f),new Vector3(.246f,.248f,.008f),glass);
            a.Block("Receiver left wall",new Vector3(.071f,-.166f,.096f),new Vector3(.008f,.248f,.238f),glass);
            a.Block("Receiver back wall",new Vector3(.190f,-.166f,.211f),new Vector3(.246f,.248f,.008f),glass);
            a.Block("Receiver right wall",new Vector3(.309f,-.166f,.096f),new Vector3(.008f,.248f,.238f),glass);
            a.Block("Receiver front lip",new Vector3(.190f,-.041f,-.019f),new Vector3(.246f,.002f,.009f),rim,null,false);
            a.Block("Receiver left lip",new Vector3(.071f,-.041f,.096f),new Vector3(.009f,.002f,.238f),rim,null,false);
            a.Block("Receiver back lip",new Vector3(.190f,-.041f,.211f),new Vector3(.246f,.002f,.009f),rim,null,false);
            a.Block("Receiver right lip",new Vector3(.309f,-.041f,.096f),new Vector3(.009f,.002f,.238f),rim,null,false);
            a.Label("ROLL",new Vector3(-.284f,.213f,-.075f),.006f);
            a.Label("CATCH",new Vector3(.190f,-.285f,.017f),.009f);
            a.Label("RECOVER",new Vector3(-.160f,-.285f,.160f),.010f);
            return a.Level;
        }

        private static Mesh Ribbon(List<Vector2> path,float z0,float z1,float top,float bottom)
        {
            var v=new List<Vector3>();var t=new List<int>();
            for(int i=1;i<path.Count;i++)
            {
                Vector2 p=path[i-1],q=path[i];
                Quad(P(p,z0,top),P(q,z0,top),P(q,z1,top),P(p,z1,top));
                Quad(P(p,z1,bottom),P(q,z1,bottom),P(q,z0,bottom),P(p,z0,bottom));
                Quad(P(p,z0,bottom),P(q,z0,bottom),P(q,z0,top),P(p,z0,top));
                Quad(P(p,z1,top),P(q,z1,top),P(q,z1,bottom),P(p,z1,bottom));
            }
            Vector2 first=path[0],last=path[path.Count-1];
            Quad(P(first,z0,top),P(first,z1,top),P(first,z1,bottom),P(first,z0,bottom));
            Quad(P(last,z0,bottom),P(last,z1,bottom),P(last,z1,top),P(last,z0,top));
            var mesh=new Mesh{name="Continuous takeoff ribbon"};mesh.SetVertices(v);mesh.SetTriangles(t,0);
            mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
            void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d)
            {int n=v.Count;v.AddRange(new[]{a,b,c,d});t.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});}
            Vector3 P(Vector2 p,float z,float y)=>new Vector3(p.x,p.y+y,z);
        }
    }
}
