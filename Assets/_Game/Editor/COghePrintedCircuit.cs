using System;
using System.Collections.Generic;
using System.Linq;
using GravityBox.Venom;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
 // Editor-only routing. Ink is baked onto fixed physical faces, never a bridge across empty space.
 // Moving controls keep their coloured badge; their fixed wiring goes on the stationary enclosure.
 internal sealed class COghePrintedCircuit
 {
  internal const float Offset=.0012f,Width=.003f;
  internal sealed class Face
  {
   public Transform T;public Vector3 Centre,X,Y,N;public Vector2 Half;public string Name;
   public bool Hole;public Vector2 HoleCentre;public float HoleRadius;
   public Vector2 UV(Vector3 p)=>new Vector2(Vector3.Dot(p-Centre,X),Vector3.Dot(p-Centre,Y));
   public Vector3 At(Vector2 p)=>Centre+X*p.x+Y*p.y;
   public Vector3 Closest(Vector3 p,float margin=0)
   {
    var q=UV(p);q.x=Mathf.Clamp(q.x,-Half.x+margin,Half.x-margin);q.y=Mathf.Clamp(q.y,-Half.y+margin,Half.y-margin);
    if(Hole){var d=q-HoleCentre;float r=HoleRadius+Mathf.Max(.002f,margin);if(d.magnitude<r)q=HoleCentre+(d.sqrMagnitude>1e-10f?d.normalized:Vector2.up)*r;}
    return At(q);
   }
   public Vector3[] Corners=>new[]{At(new Vector2(-Half.x,-Half.y)),At(new Vector2(Half.x,-Half.y)),At(new Vector2(Half.x,Half.y)),At(new Vector2(-Half.x,Half.y))};
   public bool Clear(Vector3 a,Vector3 b)
   {
    var p=UV(a);var q=UV(b);
    if(Mathf.Abs(p.x)>Half.x+.0002f||Mathf.Abs(q.x)>Half.x+.0002f||Mathf.Abs(p.y)>Half.y+.0002f||Mathf.Abs(q.y)>Half.y+.0002f)return false;
    if(!Hole)return true;
    var d=q-p;float t=d.sqrMagnitude>1e-10f?Mathf.Clamp01(Vector2.Dot(HoleCentre-p,d)/d.sqrMagnitude):0;
    return Vector2.Distance(p+d*t,HoleCentre)>=HoleRadius+.0015f;
   }
  }
  readonly List<Face> faces=new List<Face>();
  readonly List<(int a,int b,Vector3 p,Vector3 q)> joins=new List<(int,int,Vector3,Vector3)>();
  readonly HashSet<Face> enclosure=new HashSet<Face>();
  internal IReadOnlyList<Face> Faces=>faces;
  public COghePrintedCircuit(Transform root)
  {
   bool Fixed(Transform t)=>t.GetComponentInParent<Rigidbody>()==null||t.GetComponentInParent<Rigidbody>().transform==root;
   var conditional=new HashSet<VenomSurfacePatch>();
   foreach(var deck in root.GetComponentsInChildren<COgheDockedBridgeDeck>(true))
   {if(deck.DockedTop!=null)conditional.Add(deck.DockedTop);if(deck.DockedSurfaces!=null)foreach(var p in deck.DockedSurfaces)conditional.Add(p);}
   foreach(var deck in root.GetComponentsInChildren<COgheTwoDockDeck>(true)){conditional.Add(deck.StartTop);conditional.Add(deck.EndTop);}
   foreach(var p in root.GetComponentsInChildren<VenomSurfacePatch>(true))
   {
    if(!p.isActiveAndEnabled||p.IsCurved||p.MotionFrame!=null||conditional.Contains(p)||!Fixed(p.transform))continue;
    faces.Add(new Face{T=p.transform,Centre=p.transform.position,X=p.transform.right,Y=p.transform.up,N=p.Normal,
     Half=p.Size*.5f,Name=p.name,Hole=p.Hole,HoleCentre=p.HoleCentre,HoleRadius=p.HoleRadius});
   }
   // Plinth sides and solid casings can carry a trace even if that side is not a navigation patch.
   foreach(var b in root.GetComponentsInChildren<BoxCollider>(true))
   {
    if(!b.enabled||!b.gameObject.activeInHierarchy||b.isTrigger||!Fixed(b.transform)||b.GetComponent<VenomSurfacePatch>()!=null)continue;
    var r=b.GetComponent<MeshRenderer>();if(r==null||!r.enabled)continue;
    var size=Vector3.Scale(b.size,b.transform.lossyScale);var centre=b.transform.TransformPoint(b.center);
    var axes=new[]{b.transform.right,b.transform.up,b.transform.forward};
    for(int axis=0;axis<3;axis++)for(int sign=-1;sign<=1;sign+=2)
     faces.Add(new Face{T=b.transform,Name=b.name,Centre=centre+axes[axis]*size[axis]*.5f*sign,
      N=axes[axis]*sign,X=axes[(axis+1)%3],Y=axes[(axis+2)%3],Half=new Vector2(size[(axis+1)%3],size[(axis+2)%3])*.5f});
   }
   for(int a=0;a<faces.Count;a++)for(int b=a+1;b<faces.Count;b++)
   {
    var hits=new List<Vector3>();var x=faces[a];var y=faces[b];
    foreach(var seed in x.Corners.Concat(y.Corners).Concat(new[]{x.Centre,y.Centre}))
    {
     Vector3 p=seed,q=seed;
     for(int k=0;k<8;k++){p=x.Closest(q);q=y.Closest(p);}
     if(Vector3.Distance(p,q)<.0008f&&!hits.Any(h=>Vector3.Distance(h,p)<.001f))hits.Add((p+q)*.5f);
    }
    if(hits.Count==0)continue;
    var first=hits[0];var last=first;float length=0;
    foreach(var p in hits)foreach(var q in hits)if((p-q).sqrMagnitude>length){first=p;last=q;length=(p-q).sqrMagnitude;}
    joins.Add((a,b,first,last));
   }
   // A button cap or a floating carrier is not a route to the enclosure. Start on the main connected shell;
   // its real plinth sides, walls and floor remain available, while isolated contact plates keep their badge.
   if(faces.Count>0)
   {
    int start=0;for(int i=1;i<faces.Count;i++)if(faces[i].Half.x*faces[i].Half.y>faces[start].Half.x*faces[start].Half.y)start=i;
    var queue=new Queue<int>();queue.Enqueue(start);enclosure.Add(faces[start]);
    while(queue.Count>0){int f=queue.Dequeue();foreach(var j in joins){int n=j.a==f?j.b:j.b==f?j.a:-1;if(n>=0&&enclosure.Add(faces[n]))queue.Enqueue(n);}}
   }
  }
  internal (Face face,Vector3 point) Anchor(Vector3 p,float margin=0,bool connectedOnly=false)
  {
   Face best=null;Vector3 point=p;float distance=float.PositiveInfinity;
   foreach(var f in faces){if(connectedOnly&&!enclosure.Contains(f)||Mathf.Min(f.Half.x,f.Half.y)<=margin)continue;var q=f.Closest(p,margin);float d=(q-p).sqrMagnitude;if(d>=distance)continue;distance=d;point=q;best=f;}
   if(best==null)throw new InvalidOperationException("Printed circuit has no fixed physical support");
   return(best,point);
  }
  public void Draw(Transform parent,Vector3[] localPoints,Material material,string name="Printed conductor")
  {
   var path=new List<Vector3>{localPoints[0]};
   for(int i=1;i+1<localPoints.Length;i++)
   {
    var a=Anchor(parent.TransformPoint(localPoints[i-1]),0,true);var b=Anchor(parent.TransformPoint(localPoints[i]),0,true);var c=Anchor(parent.TransformPoint(localPoints[i+1]),0,true);
    if(a.face==b.face&&b.face==c.face)
    {float bevel=Mathf.Min(.008f,Mathf.Min(Vector3.Distance(a.point,b.point),Vector3.Distance(b.point,c.point))*.25f);path.Add(parent.InverseTransformPoint(b.point+(a.point-b.point).normalized*bevel));path.Add(parent.InverseTransformPoint(b.point+(c.point-b.point).normalized*bevel));}
    else path.Add(localPoints[i]);
   }
   path.Add(localPoints[localPoints.Length-1]);
   for(int i=1;i<path.Count;i++)Route(parent,parent.TransformPoint(path[i-1]),parent.TransformPoint(path[i]),material,name);
  }
  void Route(Transform parent,Vector3 start,Vector3 end,Material material,string name)
  {
   var a=Anchor(start,0,true);var b=Anchor(end,0,true);int source=faces.IndexOf(a.face),target=faces.IndexOf(b.face);
   var distance=Enumerable.Repeat(float.PositiveInfinity,faces.Count).ToArray();var previous=Enumerable.Repeat(-1,faces.Count).ToArray();
   var reached=new bool[faces.Count];var points=new Vector3[faces.Count];distance[source]=0;points[source]=a.point;
   for(int k=0;k<faces.Count;k++)
   {
    int f=-1;for(int i=0;i<faces.Count;i++)if(!reached[i]&&(f<0||distance[i]<distance[f]))f=i;
    if(f<0||float.IsPositiveInfinity(distance[f]))break;if(f==target)break;reached[f]=true;
    foreach(var j in joins)
    {
     int n=j.a==f?j.b:j.b==f?j.a:-1;if(n<0||reached[n])continue;
     var d=j.q-j.p;var mid=(start+end)*.5f;float t=d.sqrMagnitude>1e-10f?Mathf.Clamp01(Vector3.Dot(mid-j.p,d)/d.sqrMagnitude):0;
     var p=j.p+d*t;float cost=distance[f]+Vector3.Distance(points[f],p)+.002f;
     if(cost>=distance[n])continue;distance[n]=cost;previous[n]=f;points[n]=p;
    }
   }
   if(float.IsPositiveInfinity(distance[target]))
   {
    throw new InvalidOperationException($"Disconnected printed circuit: {a.face.Name} -> {b.face.Name}");
   }
   var route=new List<int>();for(int f=target;f>=0;f=previous[f])route.Add(f);route.Reverse();
   for(int i=0;i<route.Count;i++)
   {
    int f=route[i];var p=faces[f].Closest(points[f]);var q=faces[f].Closest(i+1<route.Count?points[route[i+1]]:b.point);
    OnFace(parent,faces[f],p,q,material,name);
   }
  }
  void OnFace(Transform parent,Face face,Vector3 start,Vector3 end,Material material,string name)
  {
   if(face.Clear(start,end)){Strip(parent,face,start,end,material,name);return;}
   // Visibility graph around a real aperture: never print across the open exit or pipe mouth.
   var points=new List<Vector3>{start,end};
   if(face.Hole)for(int i=0;i<24;i++)
   {
    float a=i*Mathf.PI*2/24;var uv=face.HoleCentre+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(face.HoleRadius+.0035f)/Mathf.Cos(Mathf.PI/24);
    if(Mathf.Abs(uv.x)<=face.Half.x&&Mathf.Abs(uv.y)<=face.Half.y)points.Add(face.At(uv));
   }
   var costs=Enumerable.Repeat(float.PositiveInfinity,points.Count).ToArray();var prev=Enumerable.Repeat(-1,points.Count).ToArray();var visited=new bool[points.Count];costs[0]=0;
   for(int k=0;k<points.Count;k++)
   {
    int n=-1;for(int i=0;i<points.Count;i++)if(!visited[i]&&(n<0||costs[i]<costs[n]))n=i;
    if(n<0||float.IsPositiveInfinity(costs[n])||n==1)break;visited[n]=true;
    for(int i=0;i<points.Count;i++)if(!visited[i]&&face.Clear(points[n],points[i]))
    {float cost=costs[n]+Vector3.Distance(points[n],points[i]);if(cost<costs[i]){costs[i]=cost;prev[i]=n;}}
   }
   if(float.IsPositiveInfinity(costs[1]))throw new InvalidOperationException("Cannot print circuit on "+face.Name);
   for(int i=1;prev[i]>=0;i=prev[i])Strip(parent,face,points[prev[i]],points[i],material,name);
  }
  static void Strip(Transform parent,Face face,Vector3 a,Vector3 b,Material material,string name)
  {
   Vector3 d=b-a;if(d.magnitude<.0004f)return;
   var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);
   go.transform.SetPositionAndRotation((a+b)*.5f+face.N*Offset,Quaternion.LookRotation(d,face.N));
   go.transform.localScale=new Vector3(Width,.0005f,d.magnitude);
   Object.DestroyImmediate(go.GetComponent<Collider>());var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;
  }
 }
}
