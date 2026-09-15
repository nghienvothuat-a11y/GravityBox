using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    // Small surface graph: route only the command given, never solve mechanism dependencies.
    public sealed class VenomCampaignMotion
    {
        public sealed class Order
        {
            public int Anchor;
            public Vector3 Target;
            public readonly List<Vector3> Path=new List<Vector3>();
            public int Cursor;
            public bool Holding, Exit, AwaitingContact;
        }
        private readonly VenomCampaign game;
        private readonly List<Vector3> nodes=new List<Vector3>();
        private readonly List<List<int>> links=new List<List<int>>();
        private readonly Order[] orders=new Order[32];
        private readonly VenomSurfacePatch[] support=new VenomSurfacePatch[32];
        private readonly Vector3[] contact=new Vector3[32], intent=new Vector3[32];
        public int Selected;
        public int RouteCount=>nodes.Count;
        public VenomCampaignMotion(VenomCampaign owner){game=owner;}
        public void Reset(){System.Array.Clear(orders,0,32);System.Array.Clear(support,0,32);Selected=0;BuildGraph();}
        public Vector3 Centre(int anchor)
        {
            Vector3 c=Vector3.zero;int count=0;int group=game.Matter.Groups[anchor];
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&!game.Matter.Escaped[i]){c+=game.Matter.Bodies[i].position;count++;}
            if(count==0)for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group){c+=game.Matter.Bodies[i].position;count++;}
            return c/Mathf.Max(1,count);
        }
        public Order Get(int anchor)
        {for(int i=0;i<32;i++)if(orders[i]!=null&&game.Matter.Groups[i]==game.Matter.Groups[anchor])return orders[i];return null;}
        public void Cancel(int anchor)
        {for(int i=0;i<32;i++)if(orders[i]!=null&&game.Matter.Groups[i]==game.Matter.Groups[anchor])orders[i]=null;}
        public void StopAll(){System.Array.Clear(orders,0,32);}
        public bool Busy(int anchor)=>Get(anchor)?.Holding??false;
        public Vector3 Intent(int particle)=>intent[particle];
        public bool Support(int particle,out Collider collider,out Vector3 point,out Vector3 normal)
        {
            var patch=support[particle];collider=patch!=null?patch.Shape:null;point=contact[particle];normal=patch!=null?patch.Normal:Vector3.up;
            return patch!=null && !game.Matter.Escaped[particle];
        }
        public bool HasGrip(int particle)=>support[particle]!=null&&support[particle].Grip(contact[particle]);
        public void BuildGraph()
        {
            Physics.SyncTransforms();
            nodes.Clear();links.Clear();
            foreach(var s in game.Surfaces)
            {
                if(!s.isActiveAndEnabled)continue;
                int nx=Mathf.Max(1,Mathf.CeilToInt(s.Size.x/.065f)),ny=Mathf.Max(1,Mathf.CeilToInt(s.Size.y/.065f));
                for(int x=0;x<=nx;x++)for(int y=0;y<=ny;y++)
                {
                    float ix=Mathf.Min(.017f,s.Size.x*.25f),iy=Mathf.Min(.017f,s.Size.y*.25f);
                    var p=new Vector3(Mathf.Lerp(-s.Size.x*.5f+ix,s.Size.x*.5f-ix,x/(float)nx),Mathf.Lerp(-s.Size.y*.5f+iy,s.Size.y*.5f-iy,y/(float)ny),.021f);
                    if(!s.Contains(p)||game.Occupied(s.transform.TransformPoint(p)))continue;
                    nodes.Add(game.Root.InverseTransformPoint(s.transform.TransformPoint(p)));links.Add(new List<int>(8));
                }
            }
            for(int i=0;i<nodes.Count;i++)for(int j=i+1;j<nodes.Count;j++)
            {
                if((nodes[i]-nodes[j]).sqrMagnitude>.10f*.10f)continue;
                if(game.Clear(game.Root.TransformPoint(nodes[i]),game.Root.TransformPoint(nodes[j]),.006f))
                {links[i].Add(j);links[j].Add(i);}
            }
        }
        public void Move(int anchor,Vector3 world,bool hold=false,bool exit=false)
        {
            if(game.Props.Length>0)BuildGraph();
            Cancel(anchor);
            var o=new Order{Anchor=anchor,Target=game.Root.InverseTransformPoint(world),Holding=hold,Exit=exit};
            int grips=0;for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[anchor]&&HasGrip(i))grips++;
            o.AwaitingContact=grips<2;
            FindPath(Centre(anchor),world,o.Path);orders[anchor]=o;
        }
        public bool FindPath(Vector3 start,Vector3 goal,List<Vector3> path)
        {
            path.Clear();int n=nodes.Count;
            if(n==0){path.Add(game.Root.InverseTransformPoint(goal));return false;}
            int a=Nearest(start),b=Nearest(goal);
            var costs=new float[n];var parents=new int[n];var done=new bool[n];
            for(int i=0;i<n;i++){costs[i]=float.PositiveInfinity;parents[i]=-1;}costs[a]=0;
            for(int k=0;k<n;k++)
            {
                int current=-1;float best=float.PositiveInfinity;
                for(int i=0;i<n;i++)if(!done[i]&&costs[i]<best){best=costs[i];current=i;}
                if(current<0||current==b)break;done[current]=true;
                foreach(int next in links[current])
                {float d=costs[current]+Vector3.Distance(nodes[current],nodes[next]);if(d<costs[next]){costs[next]=d;parents[next]=current;}}
            }
            if(float.IsPositiveInfinity(costs[b])){Debug.LogWarning($"Disconnected surface route {game.Definition.Order}: {nodes[a]} -> {nodes[b]}, {nodes.Count} nodes");path.Add(game.Root.InverseTransformPoint(goal));return false;}
            for(int at=b;at>=0;at=parents[at]){path.Add(nodes[at]);if(at==a)break;}
            path.Reverse();path.Add(game.Root.InverseTransformPoint(goal));return true;
        }
        private int Nearest(Vector3 p)
        {
            int best=0;float distance=float.PositiveInfinity;
            for(int i=0;i<nodes.Count;i++)
            {float d=(game.Root.TransformPoint(nodes[i])-p).sqrMagnitude;if(d<distance){distance=d;best=i;}}
            return best;
        }
        public void Step(float dt)
        {
            // Scene children are not enabled yet during the owner's Awake.
            // Build after they join the physics scene, not against an empty graph.
            if(nodes.Count==0&&(!game.Definition.Passive||game.Home))BuildGraph();
            // Establish local contact first. No force can be anchored to a remote surface.
            for(int i=0;i<32;i++)
            {
                support[i]=null;intent[i]=Vector3.zero;
                var p=game.Matter.Bodies[i].position;float best=.034f;
                foreach(var patch in game.Surfaces)
                {
                    if(!patch.isActiveAndEnabled||game.IsHeldSurface(patch))continue;
                    var local=patch.transform.InverseTransformPoint(p);
                    if(local.z<-.002f||local.z>.036f||!patch.Contains(local,.001f))continue;
                    Vector3 q=patch.Closest(p);float d=Vector3.Distance(q,p);
                    // The deformable skin extends beyond each particle centre.
                    // A catch is local to its contact envelope, never a reach from the ceiling.
                    float limit=patch.RingGrip&&patch.Grip(q)?.021f:.034f;
                    if(d>=best||d>limit)continue;
                    best=d;support[i]=patch;contact[i]=q;
                }
            }
            var seen=new HashSet<int>();
            for(int a=0;a<32;a++)
            {
                if(game.Matter.Escaped[a]||!seen.Add(game.Matter.Groups[a]))continue;
                Vector3 centre=Centre(a);Order o=Get(a);Vector3 target=centre;
                int grips=0;
                for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[a]&&HasGrip(i))grips++;
                if(o!=null)
                {
                    if(grips<2)o.AwaitingContact=true;
                    else if(o.AwaitingContact){FindPath(centre,game.Root.TransformPoint(o.Target),o.Path);o.Cursor=0;o.AwaitingContact=false;}
                }
                if(o!=null)
                {
                    while(o.Cursor<o.Path.Count-1&&Vector3.Distance(centre,game.Root.TransformPoint(o.Path[o.Cursor]))<.022f)o.Cursor++;
                    target=game.Root.TransformPoint(o.Path[o.Cursor]);
                    if(Vector3.Distance(centre,game.Root.TransformPoint(o.Target))<.023f&&!o.Holding&&!o.Exit){Cancel(a);o=null;}
                }
                bool anchored=grips>=2&&(!game.Definition.Passive||game.Home)&&!game.InTube;
                Vector3 delta=target-centre;
                Vector3 desired=o!=null?delta.normalized*.105f:Vector3.zero;
                if(o!=null&&o.Cursor==o.Path.Count-1)desired=Vector3.ClampMagnitude(delta*4,.105f);
                for(int i=0;i<32;i++)
                {
                    if(game.Matter.Groups[i]!=game.Matter.Groups[a]||game.Matter.Escaped[i])continue;
                    Rigidbody body=game.Matter.Bodies[i];var patch=support[i];
                    bool atExit=game.ExitAssisting(i);
                    game.Matter.SetFlow(i,game.InTube||atExit?1:.12f);
                    if(!anchored||atExit)continue;
                    // Muscular tension carries the complete connected body from
                    // its planted feet. Unsupported tissue keeps its deformable
                    // bonds, rather than pulling a climbing body off the wall.
                    Vector3 relative=body.linearVelocity-game.Owner.Rotation.GetComponent<Rigidbody>().GetPointVelocity(body.position);
                    Vector3 acceleration=Vector3.up*9.81f;
                    bool manipulating=game.Attached&&game.Matter.Groups[a]==game.Matter.Groups[Selected];
                    if(!manipulating)acceleration+=Vector3.ClampMagnitude((desired-relative)*26,5);
                    intent[i]=desired/.105f;
                    if(patch!=null&&patch.Grip(contact[i]))
                    {
                        Vector3 n=patch.Normal;
                        float distance=Vector3.Dot(body.position-contact[i],n);
                        float grip=o!=null&&Vector3.Dot(delta,n)>.018f?0:1;
                        acceleration+=n*((.012f-distance)*700-Vector3.Dot(relative,n)*22)*grip;
                        var prop=patch.Shape.attachedRigidbody;
                        if(prop!=null&&!prop.isKinematic)prop.AddForceAtPosition(-Vector3.ClampMagnitude(acceleration,28)*body.mass,contact[i]);
                    }
                    body.AddForce(Vector3.ClampMagnitude(acceleration,28)*body.mass);
                }
            }
        }
    }
}
