using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>One bounded, depth-tested mesh anchored to the real supporting surfaces. Cosmetic only.</summary>
    public sealed class COgheWaterTrail : MonoBehaviour
    {
        const int Capacity = 80, Across = 5, Vertices = Across * 2;
        const float Life = 2.4f;
        struct Drop { public Transform Frame; public Vector3 A, B, Side; public float Age; public bool Used; }
        readonly Drop[] drops = new Drop[Capacity];
        readonly Vector3[] vertices = new Vector3[Capacity * Vertices];
        readonly Color[] colors = new Color[Capacity * Vertices];
        readonly int[] triangles = new int[Capacity * (Across - 1) * 6];
        Mesh mesh; Material material; MeshRenderer drawing;
        Transform lastFrame; Vector3 lastPoint; int cursor; bool connected;
        public int Segments { get; private set; }
        public void Initialize()
        {
            mesh = new Mesh { name = "Speedy water ribbon (pooled)" }; mesh.MarkDynamic();
            material = new Material(Resources.Load<Shader>("COgheInk/WaterTrail"));
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            drawing = gameObject.AddComponent<MeshRenderer>(); drawing.sharedMaterial = material;
            drawing.shadowCastingMode = ShadowCastingMode.Off; drawing.receiveShadows = false;
            for (int i = 0; i < Capacity; i++) for (int j = 0; j < Across - 1; j++)
            {
                int t = (i * (Across - 1) + j) * 6, a = i * Vertices + j, b = a + Across;
                triangles[t]=a; triangles[t+1]=b; triangles[t+2]=a+1;
                triangles[t+3]=a+1; triangles[t+4]=b; triangles[t+5]=b+1;
            }
            mesh.vertices = vertices; mesh.colors = colors; mesh.triangles = triangles;
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 5);
        }
        public void Break() { connected = false; }
        public void Clear()
        { System.Array.Clear(drops, 0, Capacity); Segments = 0; connected = false; if(drawing!=null)drawing.enabled=false; }
        public void Contact(Transform frame, Vector3 point, Vector3 normal)
        {
            point += normal * .0008f;
            Vector3 local = frame.InverseTransformPoint(point);
            if (connected && lastFrame == frame)
            {
                Vector3 delta = point - frame.TransformPoint(lastPoint);
                if (delta.sqrMagnitude < .006f * .006f) return;
                if (delta.sqrMagnitude < .055f * .055f)
                {
                    drops[cursor] = new Drop { Frame=frame, A=lastPoint, B=local,
                        Side=frame.InverseTransformDirection(Vector3.Cross(normal,delta).normalized), Used=true };
                    cursor=(cursor+1)%Capacity;
                }
            }
            lastFrame=frame;lastPoint=local;connected=true;
        }
        public void Step(float dt)
        {
            Segments=0;
            for(int i=0;i<Capacity;i++)
            {
                ref var d=ref drops[i]; if(d.Used)d.Age+=dt;
                bool live=d.Used&&d.Frame!=null&&d.Age<Life;
                if(!live){d.Used=false;for(int j=0;j<Vertices;j++)colors[i*Vertices+j]=Color.clear;continue;}
                Segments++;
                float fade=Mathf.SmoothStep(1,0,d.Age/Life);
                Vector3 side=d.Frame.TransformDirection(d.Side)*(.013f+.002f*d.Age);
                for(int end=0;end<2;end++)for(int j=0;j<Across;j++)
                {
                    int n=i*Vertices+end*Across+j;
                    Vector3 centre=d.Frame.TransformPoint(end==0?d.A:d.B);
                    vertices[n]=transform.InverseTransformPoint(centre+side*(j*.5f-1));
                    // Clear feathered edges, translucent mint water and a narrow pearly reflection.
                    float opacity=j==0||j==4?0:j==2?.28f:.12f;
                    colors[n]=new Color(j==2?.69f:.39f,.88f,.83f,opacity*fade);
                }
            }
            drawing.enabled=Segments>0;
            if(Segments==0)return;
            mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.RecalculateBounds();
        }
        void OnDestroy(){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
    }
}
