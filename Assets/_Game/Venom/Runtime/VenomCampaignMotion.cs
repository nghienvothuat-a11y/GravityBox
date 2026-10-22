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
            public int CommandId;
            public bool Holding, Exit, AwaitingContact, AvoidSlippery;
            /// <summary>Back at a corner it cut and got stuck on: this waypoint is walked all the way to.</summary>
            public bool Rounding;
        }
        private readonly VenomCampaign game;
        private readonly List<Vector3> nodes=new List<Vector3>();
        private readonly List<VenomSurfacePatch> nodeSurfaces=new List<VenomSurfacePatch>();
        // How high a node stands above the foot of its surface, for a near-vertical surface (0 for a floor or top).
        private readonly List<float> nodeRise=new List<float>();
        private readonly List<List<int>> links=new List<List<int>>();
        private readonly List<List<int>> linkStorage=new List<List<int>>();
        private readonly List<Vector3> worldNodes=new List<Vector3>();
        private readonly VenomNavigationSnapshot navigation=new VenomNavigationSnapshot();
        private readonly VenomNavigationRevision graphRevision=new VenomNavigationRevision();
        private float[] costs=System.Array.Empty<float>();
        private int[] parents=System.Array.Empty<int>(),heap=System.Array.Empty<int>(),heapPositions=System.Array.Empty<int>();
        private bool[] done=System.Array.Empty<bool>();
        private int heapCount;
        public int GraphBuildCount {get;private set;}
        private readonly Dictionary<Vector3Int,List<int>> cells=new Dictionary<Vector3Int,List<int>>();
        private readonly List<List<int>> cellStorage=new List<List<int>>();
        private readonly List<int> neighbours=new List<int>(128);
        private readonly HashSet<int> seenGroups=new HashSet<int>();
        private VenomSurfaceSnapshot[] contactWindows=System.Array.Empty<VenomSurfaceSnapshot>();
        private static Vector3Int Cell(Vector3 p)=>new Vector3Int(Mathf.FloorToInt(p.x/.1f),Mathf.FloorToInt(p.y/.1f),Mathf.FloorToInt(p.z/.1f));
        private readonly Order[] orders=new Order[32];
        private readonly VenomSurfacePatch[] support=new VenomSurfacePatch[32];
        private readonly Vector3[] contact=new Vector3[32], intent=new Vector3[32];
        // The contact samples approximate the fraction of the footprint that
        // still adheres. Internal / airborne particles are not missing feet.
        // Available traction is finite (N = kg * m/s² * adhering fraction).
        // Sustained excess load peels the footprint before it can reattach.
        private const float GripAccelerationLimit=36f;
        private const float BaseMoveSpeed=.1638f; // m/s; +30% from the previous .126 m/s.
        /// <summary>Walking speed multiplier in levels (the Agility ability: +10% a level; a hungry COghe walks slower,
        /// COgheEnergy); Home keeps the base pace.</summary>
        public float SpeedScale=1;
        private float MoveSpeed=>game.Home?BaseMoveSpeed:BaseMoveSpeed*SpeedScale;
        private readonly float[] gripStrain=new float[32], detachedUntil=new float[32];
        private readonly HashSet<VenomSurfacePatch>[] detachedSurfaces=new HashSet<VenomSurfacePatch>[32];
        private sealed class RingCatch
        {
            public VenomSurfacePatch Surface;
            public Vector3 LocalPoint,LocalTarget;
            public float Until;
        }
        private readonly RingCatch[] ringCatches=new RingCatch[32];
        public int Selected;
        private int nextCommandId;
        public int RouteCount=>nodes.Count;
        public VenomCampaignMotion(VenomCampaign owner){game=owner;System.Array.Fill(stallSince,-1f);System.Array.Fill(cornerSince,-1f);}
        public void Reset(){System.Array.Clear(orders,0,32);System.Array.Clear(support,0,32);System.Array.Clear(gripStrain,0,32);System.Array.Clear(detachedUntil,0,32);System.Array.Clear(detachedSurfaces,0,32);System.Array.Clear(ringCatches,0,32);System.Array.Fill(stallSince,-1f);System.Array.Fill(cornerSince,-1f);Selected=0;nextCommandId=0;BuildGraph(true);}
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
        /// <summary>The last <see cref="Move"/> found a route on the surfaces (false: no route; it heads straight for it).</summary>
        public bool LastMoveReachable {get;private set;}=true;
        public int MoveCount {get;private set;}
        private VenomSurfacePatch[] glassCheckedFor;private bool climbableGlass;
        /// <summary>The level's glass is climbable, with slick tape where it is not (<see cref="VenomSurfacePatch.SlipRegions"/>).</summary>
        private bool ClimbableGlass()
        {
            if(glassCheckedFor!=game.Surfaces)
            {
                glassCheckedFor=game.Surfaces;climbableGlass=false;
                if(game.Surfaces!=null)foreach(var s in game.Surfaces)if(s!=null&&s.ExteriorGlass&&!s.Slippery){climbableGlass=true;break;}
            }
            return climbableGlass;
        }
        public void ReconcileAfterFusion()
        {
            // Several fragments may now share a body. Keep the newest command,
            // not whichever old pad/route happens to have the lowest particle ID.
            for(int a=0;a<32;a++)
            {
                var winner=orders[a];if(winner==null)continue;
                int count=0;
                for(int i=0;i<32;i++)
                    if(orders[i]!=null&&game.Matter.Groups[i]==game.Matter.Groups[a])
                    {count++;if(orders[i].CommandId>winner.CommandId)winner=orders[i];}
                if(count<2)continue;
                Cancel(a);orders[winner.Anchor]=winner;winner.Cursor=0;
                FindPath(Centre(winner.Anchor),game.Root.TransformPoint(winner.Target),winner.Path,winner.AvoidSlippery);
                SetCatch(winner.Anchor,null);
            }
        }
        public bool Busy(int anchor)=>Get(anchor)?.Holding??false;
        // A grip overload peeled this group off its surface in the last .45 s. A peel drops the group's order on its
        // own; mechanisms walking a body to a handle use this to tell it from the player giving a new command.
        public bool Peeled(int anchor)
        {int group=game.Matter.Groups[anchor];for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&detachedUntil[i]>game.Matter.SimulationTime)return true;return false;}
        public void BraceAgainstManipulation(int anchor,Vector3 reaction)
        {
            int group=game.Matter.Groups[anchor],feet=0;
            float mass=0,contactMass=0,gripMass=0;
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&!game.Matter.Escaped[i])
            {
                float m=game.Matter.Bodies[i].mass;mass+=m;
                if(support[i]!=null)contactMass+=m;
                if(HasGrip(i)){feet++;gripMass+=m;}
            }
            if(feet<2)return;
            float available=Mathf.Max(0,mass*GripAccelerationLimit*gripMass/Mathf.Max(contactMass,.0001f)-mass*9.81f);
            Vector3 bracing=Vector3.ClampMagnitude(reaction,available)/feet;
            // The hand still receives the full equal/opposite object reaction.
            // Real planted feet carry its load into their actual supporting surface.
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&HasGrip(i))
            {
                game.Matter.Bodies[i].AddForce(bracing);
                var floor=support[i].Shape.attachedRigidbody;
                if(floor!=null&&!floor.isKinematic)floor.AddForceAtPosition(-bracing,contact[i]);
            }
        }
        public Vector3 Intent(int particle)=>intent[particle];
        public bool Support(int particle,out Collider collider,out Vector3 point,out Vector3 normal)
        {
            var patch=support[particle];collider=patch!=null?patch.Shape:null;point=contact[particle];normal=patch!=null?patch.NormalAt(point):Vector3.up;
            return patch!=null && !game.Matter.Escaped[particle];
        }
        // A hard landing can push a node through one thin face of a hollow block. Trapped inside, it tethers the
        // whole body: COghe has a route but cannot move (level 51 / Spatial 26, a fall off the rope, 18/10/2026).
        // Once a body with a plain walking order has stayed within 3 cm for a second, a node enclosed behind a block face
        // is put back in front of it. Through the face: the body's centre is in front of it, the node's centre lies
        // over the face and behind its plane (in contact a node's centre stays in front; half-way through, the thin
        // face holds it and it jitters), and a ray from the node to the face meets that face first (skin curled under
        // a shelf meets the shelf's underside; skin hanging over an edge is not over the face).
        // A node can also snag on the lip of an opening (the stored tray's slot in 51's middle platform): held far from
        // the still body, it shakes the whole second. That node is put back inside the body.
        private readonly float[] stallSince=new float[32],stallSeen=new float[32],shake=new float[32];private readonly int[] shakeTicks=new int[32];
        private readonly Vector3[] stallAt=new Vector3[32];
        private readonly RaycastHit[] tunnelHits=new RaycastHit[16];private readonly Collider[] tunnelRoom=new Collider[16];
        public int FreedNodes {get;private set;}
        private void FreeTunnelledNodes(int anchor,Vector3 centre)
        {
            float now=game.Matter.SimulationTime;int group=game.Matter.Groups[anchor];
            bool fresh=stallSince[anchor]<0||now-stallSeen[anchor]>.05f||Vector3.Distance(centre,stallAt[anchor])>.03f;   // a strained body bobs 1–2 cm in place
            stallSeen[anchor]=now;
            if(fresh){stallAt[anchor]=centre;stallSince[anchor]=now;ClearShake(anchor,group);return;}
            shakeTicks[anchor]++;for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group)shake[i]+=game.Matter.Bodies[i].linearVelocity.magnitude;
            if(now-stallSince[anchor]<1)return;
            stallSince[anchor]=now;float r=game.Matter.Profile.ParticleRadius;
            var span=new Bounds(centre,Vector3.zero);
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&!game.Matter.Escaped[i])span.Encapsulate(game.Matter.Bodies[i].position);
            span.Expand(4*r);
            bool backfaces=Physics.queriesHitBackfaces;Physics.queriesHitBackfaces=true;
            try
            {
                foreach(var patch in game.Surfaces)
                {
                    if(patch==null||!patch.isActiveAndEnabled||patch.ExteriorGlass||patch.Hole||patch.IsCurved||patch.Shape==null||!patch.Shape.bounds.Intersects(span))continue;
                    if(patch.transform.InverseTransformPoint(centre).z<=0)continue;
                    for(int i=0;i<32;i++)
                    {
                        if(game.Matter.Groups[i]!=group||game.Matter.Escaped[i])continue;
                        var body=game.Matter.Bodies[i];Vector3 p=patch.transform.InverseTransformPoint(body.position);
                        if(p.z>=-.002f||Mathf.Abs(p.x)>patch.Size.x*.5f-r||Mathf.Abs(p.y)>patch.Size.y*.5f-r)continue;
                        if(FirstSolid(body.position,patch.Normal,.002f-p.z)!=patch.Shape)continue;
                        Vector3 free=patch.transform.TransformPoint(new Vector3(p.x,p.y,r*1.2f));
                        if(!Room(free,r*.9f,group))continue;
                        body.position=free;body.linearVelocity=Vector3.zero;support[i]=null;FreedNodes++;
                    }
                }
            }
            finally{Physics.queriesHitBackfaces=backfaces;}
            for(int i=0;i<32;i++)
            {
                if(game.Matter.Groups[i]!=group||game.Matter.Escaped[i])continue;
                var body=game.Matter.Bodies[i];
                if(shake[i]/Mathf.Max(1,shakeTicks[anchor])<.5f||Vector3.Distance(body.position,centre)<.045f)continue;
                Vector3 spot=Room(centre,r*.9f,group)?centre:centre+Vector3.up*(2*r);
                if(!Room(spot,r*.9f,group))continue;
                body.position=spot;body.linearVelocity=Vector3.zero;support[i]=null;FreedNodes++;
            }
            ClearShake(anchor,group);
        }
        private void ClearShake(int anchor,int group){shakeTicks[anchor]=0;for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group)shake[i]=0;}
        private static bool IsMatter(Collider c)=>c.TryGetComponent<VenomContact>(out _);
        private Collider FirstSolid(Vector3 from,Vector3 direction,float length)
        {
            int n=Physics.RaycastNonAlloc(from,direction,tunnelHits,length,~0,QueryTriggerInteraction.Ignore);Collider first=null;float best=float.PositiveInfinity;
            for(int k=0;k<n;k++)if(!IsMatter(tunnelHits[k].collider)&&tunnelHits[k].distance<best){best=tunnelHits[k].distance;first=tunnelHits[k].collider;}
            return first;
        }
        // Free space for a freed node: no geometry and no node of another part (that would fuse them).
        private bool Room(Vector3 point,float radius,int group)
        {
            int n=Physics.OverlapSphereNonAlloc(point,radius,tunnelRoom,~0,QueryTriggerInteraction.Ignore);
            for(int k=0;k<n;k++){var rb=tunnelRoom[k].attachedRigidbody;int node=rb!=null?System.Array.IndexOf(game.Matter.Bodies,rb):-1;if(node<0||game.Matter.Groups[node]!=group)return false;}
            return true;
        }
        public bool HasGrip(int particle)=>support[particle]!=null&&support[particle].Grip(contact[particle])&&
            (game.Matter.SimulationTime>=detachedUntil[particle]||detachedSurfaces[particle]==null||!detachedSurfaces[particle].Contains(support[particle]));
        public bool TryCatchPoint(int anchor,out Vector3 point)
        {
            var caught=ringCatches[anchor];point=Vector3.zero;
            if(caught==null||game.InTube||game.Matter.SimulationTime>=caught.Until)return false;
            point=caught.Surface.transform.TransformPoint(caught.LocalPoint);return true;
        }
        private void SetCatch(int anchor,RingCatch caught)
        {for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[anchor])ringCatches[i]=caught;}
        private RingCatch CatchRing(int anchor,Vector3 centre,Vector3 velocity)
        {
            var caught=ringCatches[anchor];var ring=game.Tube!=null?game.Tube.Entrance:null;
            if(caught!=null&&(game.InTube||game.Matter.SimulationTime>=caught.Until||
                !caught.Surface.isActiveAndEnabled||Vector3.Distance(centre,caught.Surface.transform.TransformPoint(caught.LocalPoint))>.15f))
            {SetCatch(anchor,null);caught=null;}
            if(caught!=null||game.InTube||ring==null||Vector3.Dot(velocity,Vector3.down)<.25f)return caught;
            int count=0;Vector3 point=Vector3.zero;
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[anchor]&&HasGrip(i)&&support[i]==ring)
            {count++;point+=contact[i];}
            // Only two real skin contacts can start this short active grasp.
            // The stored anchor has a finite reach and never catches a remote fall.
            if(count<2)return null;
            Vector3 local=ring.transform.InverseTransformPoint(ring.Closest(point/count));
            Vector2 radial=new Vector2(local.x,local.y)-ring.HoleCentre;
            float radius=Mathf.Lerp(ring.HoleRadius,ring.GripRadius,.5f);
            Vector2 footprint=ring.HoleCentre+(radial.sqrMagnitude>.000001f?radial.normalized:Vector2.up)*radius;
            caught=new RingCatch{Surface=ring,LocalPoint=local,LocalTarget=new Vector3(footprint.x,footprint.y,.022f),Until=game.Matter.SimulationTime+.55f};
            SetCatch(anchor,caught);Cancel(anchor);return caught;
        }
        public void BuildGraph(bool force=false)
        {
            if(!force&&graphRevision.Matches(game))return;
            COgheMobileMetrics.Begin(2);GraphBuildCount++;
            Physics.SyncTransforms();
            navigation.Capture(game);
            nodes.Clear();worldNodes.Clear();nodeSurfaces.Clear();nodeRise.Clear();links.Clear();cells.Clear();int cellCount=0;
            Matrix4x4 toRoot=game.Root.worldToLocalMatrix;
            foreach(var s in game.Surfaces)
            {
                if(!s.isActiveAndEnabled||s.IsCurved)continue;
                Matrix4x4 toWorld=s.transform.localToWorldMatrix;
                bool wall=Mathf.Abs(Vector3.Dot(s.Normal,game.Root.up))<.5f;float foot=float.PositiveInfinity;
                if(wall)foreach(var corner in new[]{new Vector3(-1,-1),new Vector3(1,-1),new Vector3(1,1),new Vector3(-1,1)})
                    foot=Mathf.Min(foot,Vector3.Dot(toWorld.MultiplyPoint3x4(new Vector3(corner.x*s.Size.x*.5f,corner.y*s.Size.y*.5f,0)),game.Root.up));
                int nx=Mathf.Max(1,Mathf.CeilToInt(s.Size.x/.065f)),ny=Mathf.Max(1,Mathf.CeilToInt(s.Size.y/.065f));
                for(int x=0;x<=nx;x++)for(int y=0;y<=ny;y++)
                {
                    float ix=Mathf.Min(.017f,s.Size.x*.25f),iy=Mathf.Min(.017f,s.Size.y*.25f);
                    var p=new Vector3(Mathf.Lerp(-s.Size.x*.5f+ix,s.Size.x*.5f-ix,x/(float)nx),Mathf.Lerp(-s.Size.y*.5f+iy,s.Size.y*.5f-iy,y/(float)ny),.021f);
                    Vector3 world=toWorld.MultiplyPoint3x4(p);
                    if(!s.Contains(p)||navigation.Occupied(world))continue;
                    int index=nodes.Count;Vector3 local=toRoot.MultiplyPoint3x4(world);
                    nodes.Add(local);worldNodes.Add(world);nodeSurfaces.Add(s);nodeRise.Add(wall?Vector3.Dot(world,game.Root.up)-foot:0);
                    if(linkStorage.Count<=index)linkStorage.Add(new List<int>(8));
                    var connections=linkStorage[index];connections.Clear();links.Add(connections);
                    Vector3Int cell=Cell(local);
                    if(!cells.TryGetValue(cell,out var bucket))
                    {
                        if(cellStorage.Count<=cellCount)cellStorage.Add(new List<int>(16));
                        bucket=cellStorage[cellCount++];bucket.Clear();cells.Add(cell,bucket);
                    }
                    bucket.Add(index);
                }
            }
            for(int i=0;i<nodes.Count;i++)
            {
                neighbours.Clear();var cell=Cell(nodes[i]);
                for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)for(int z=-1;z<=1;z++)
                    if(cells.TryGetValue(cell+new Vector3Int(x,y,z),out var bucket))
                        foreach(int j in bucket)if(j>i&&(nodes[i]-nodes[j]).sqrMagnitude<=.10f*.10f)neighbours.Add(j);
                // Preserve the original deterministic edge order for equal-cost routes.
                neighbours.Sort();
                foreach(int j in neighbours)if(navigation.Clear(worldNodes[i],worldNodes[j],.006f))
                {links[i].Add(j);links[j].Add(i);}
            }
            graphRevision.Capture(game);COgheMobileMetrics.End(2);
        }
        public void Move(int anchor,Vector3 world,bool hold=false,bool exit=false)
        {
            if(game.Props.Length>0)BuildGraph();
            SetCatch(anchor,null);
            Cancel(anchor);
            bool tubeDrop=game.Tube!=null&&game.Tube.Entrance!=null&&
                Vector3.Distance(game.Tube.Entrance.Closest(world),world)<.05f;
            var o=new Order{Anchor=anchor,Target=game.Root.InverseTransformPoint(world),Holding=hold,Exit=exit,
                AvoidSlippery=!exit&&!tubeDrop,CommandId=++nextCommandId};
            int grips=0;bool recovering=false;
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[anchor])
            {
                if(HasGrip(i))grips++;
                recovering|=detachedUntil[i]>game.Matter.SimulationTime;
            }
            // A fresh instruction given while the body is still falling from a peel must
            // survive the transient contacts made on the way down. It will be replanned
            // from the first stable footprint instead of being discarded by a second peel.
            o.AwaitingContact=grips<2||recovering;
            LastMoveReachable=true;MoveCount++;
            if(game.Definition.Passive&&!game.Home)o.Path.Add(o.Target);
            else LastMoveReachable=FindPath(Centre(anchor),world,o.Path,o.AvoidSlippery);
            orders[anchor]=o;
        }
        public bool FindPath(Vector3 start,Vector3 goal,List<Vector3> path,bool avoidSlippery=false,bool quiet=false)
        {
            COgheMobileMetrics.Begin(3);
            path.Clear();int n=nodes.Count;
            if(n==0){path.Add(game.Root.InverseTransformPoint(goal));COgheMobileMetrics.End(3);return false;}
            // The graph is cached in box coordinates and survives rotation.
            // Picking a destination and evaluating traction must use its CURRENT
            // world pose, not the positions captured when the graph was built.
            Matrix4x4 pose=game.Root.localToWorldMatrix;
            for(int i=0;i<n;i++)worldNodes[i]=pose.MultiplyPoint3x4(nodes[i]);
            // A normal waypoint may snap to an adjacent dry sample so a slightly
            // imprecise tap does not strand the body on a coating. A direct exit
            // command from another face keeps the physically nearest sample, which
            // lets a coating teach itself. Once the player has deliberately reached
            // a dry point on the outlet face, preserve that safe final approach.
            int a=Nearest(start),nearestGoal=Nearest(goal);
            bool dryOutletApproach=nodeSurfaces[a]==nodeSurfaces[nearestGoal]&&nodeSurfaces[a].Grip(worldNodes[a]);
            int b=avoidSlippery||dryOutletApproach?NearestDestination(goal):nearestGoal;
            bool grippyGoal=avoidSlippery&&nodeSurfaces[b].Grip(worldNodes[b]);
            if(costs.Length<n)
            {
                int capacity=Mathf.NextPowerOfTwo(n);costs=new float[capacity];parents=new int[capacity];
                done=new bool[capacity];heap=new int[capacity];heapPositions=new int[capacity];
            }
            for(int i=0;i<n;i++){costs[i]=float.PositiveInfinity;parents[i]=-1;done[i]=false;heapPositions[i]=-1;}
            heapCount=0;costs[a]=0;Queue(a);
            while(heapCount>0)
            {
                int current=Pop();if(current==b)break;done[current]=true;
                foreach(int next in links[current])
                {
                    if(done[next])continue;
                    // On glass roads (NoSlickClimb) a slick wall more than a small step above its foot is entered only going
                    // down (a slide): a far tap once planned a short slick climb (it slipped back) over the long road that was
                    // the way (19/10/2026). Other levels keep the slick climb a far tap may try, which the Origin lessons teach.
                    if(nodeSurfaces[next].NoSlickClimb&&nodeRise[next]>.06f&&!nodeSurfaces[next].Grip(worldNodes[next])&&
                       Vector3.Dot(worldNodes[next]-worldNodes[current],game.Root.up)>-.005f)continue;
                    float tractionCost=grippyGoal&&!nodeSurfaces[next].Grip(worldNodes[next])?20f:1f;
                    float d=costs[current]+Vector3.Distance(nodes[current],nodes[next])*tractionCost;
                    if(d<costs[next]){costs[next]=d;parents[next]=current;Queue(next);}
                }
            }
            if(float.IsPositiveInfinity(costs[b])){if(!quiet)Debug.LogWarning($"Disconnected surface route {game.Definition.Order}: {nodes[a]} -> {nodes[b]}, {nodes.Count} nodes");path.Add(game.Root.InverseTransformPoint(goal));COgheMobileMetrics.End(3);return false;}
            for(int at=b;at>=0;at=parents[at])
            {
                path.Add(nodes[at]);if(at==a)break;
                if(TryInsideCorner(parents[at],at,out var corner))path.Add(corner);
            }
            path.Reverse();path.Add(game.Root.InverseTransformPoint(goal));COgheMobileMetrics.End(3);return true;
        }
        // Dijkstra with a decrease-key heap. Equal costs retain the old scan's
        // lowest-node-index tie break, so this optimization does not reroute puzzles.
        private bool Before(int a,int b)=>costs[a]<costs[b]||(costs[a]==costs[b]&&a<b);
        private void Swap(int a,int b)
        {int node=heap[a];heap[a]=heap[b];heap[b]=node;heapPositions[heap[a]]=a;heapPositions[heap[b]]=b;}
        private void Queue(int node)
        {
            int at=heapPositions[node];if(at<0){at=heapCount++;heap[at]=node;heapPositions[node]=at;}
            while(at>0){int parent=(at-1)/2;if(!Before(heap[at],heap[parent]))break;Swap(at,parent);at=parent;}
        }
        private int Pop()
        {
            int node=heap[0];heapPositions[node]=-1;heapCount--;if(heapCount==0)return node;
            heap[0]=heap[heapCount];heapPositions[heap[0]]=0;int at=0;
            while(at*2+1<heapCount)
            {
                int child=at*2+1;if(child+1<heapCount&&Before(heap[child+1],heap[child]))child++;
                if(!Before(heap[child],heap[at]))break;Swap(child,at);at=child;
            }
            return node;
        }
        private bool TryInsideCorner(int from,int to,out Vector3 corner)
        {
            corner=Vector3.zero;
            var first=nodeSurfaces[from];var second=nodeSurfaces[to];
            if(first==second||Mathf.Abs(Vector3.Dot(first.Normal,second.Normal))>.001f)return false;
            Vector3 a=game.Root.TransformPoint(nodes[from]),b=game.Root.TransformPoint(nodes[to]);
            // A short diagonal across an inside corner can leave a small fragment
            // out of reach of BOTH panes. Travel into the corner before turning.
            // Convex divider crests still use EdgeTarget's body-clearance route.
            if(first.DistanceInside(b)<.02f||second.DistanceInside(a)<.02f)return false;
            Vector3 p=(a+b)*.5f;
            p+=first.Normal*(.021f-first.DistanceInside(p));
            p+=second.Normal*(.021f-second.DistanceInside(p));
            if(!first.Contains(first.transform.InverseTransformPoint(p))||
                !second.Contains(second.transform.InverseTransformPoint(p))||
                !game.Clear(a,p,.006f)||!game.Clear(p,b,.006f))return false;
            corner=game.Root.InverseTransformPoint(p);return true;
        }
        private int Nearest(Vector3 p)
        {
            int best=0;float distance=float.PositiveInfinity;
            for(int i=0;i<nodes.Count;i++)
            {float d=(game.Root.TransformPoint(nodes[i])-p).sqrMagnitude;if(d<distance){distance=d;best=i;}}
            return best;
        }
        private int NearestDestination(Vector3 p)
        {
            int nearest=Nearest(p);
            if(nodeSurfaces[nearest].Grip(worldNodes[nearest]))return nearest;
            int grippy=-1;float grippyDistance=float.PositiveInfinity;
            for(int i=0;i<nodes.Count;i++)
            {
                if(!nodeSurfaces[i].Grip(worldNodes[i]))continue;
                float d=(worldNodes[i]-p).sqrMagnitude;
                if(d<grippyDistance){grippyDistance=d;grippy=i;}
            }
            // A sampled node can land just inside a coating even when the tap or
            // aperture centre is immediately beside it. Prefer the stable rim only
            // inside one footprint; taps clearly inside a slick area remain slick.
            float nearestDistance=Vector3.Distance(worldNodes[nearest],p);
            return grippy>=0&&Mathf.Sqrt(grippyDistance)<=nearestDistance+.025f?grippy:nearest;
        }
        // Hunger (COgheEnergy) and the Agility card change the pace only while COghe cruises along its route. A climb or a step
        // within reach, and the last stretch to where it was sent (a tube mouth, a handle's stand point, the other half, a
        // landing), are taken at the base pace, so the moments that decide a level play out as every level was solved. A
        // 30-speed sweep (21/10/2026) showed each other pace tipping a different marginal spot: the balcony tube of 41 and 53
        // (it slid round the carriage at x1, stuck against it at x0.98 and x1.02), merges, a landing beside the exit.
        private const float CruiseLookahead=.08f,StepHeight=.012f;
        private float PaceFor(Order o,Vector3 centre)
        {
            if(MoveSpeed==BaseMoveSpeed||o.Path.Count==0)return MoveSpeed;
            Vector3 from=centre;float ahead=0;
            for(int k=Mathf.Min(o.Cursor,o.Path.Count-1);k<o.Path.Count;k++)
            {
                Vector3 p=game.Root.TransformPoint(o.Path[k]);
                if(Mathf.Abs(p.y-from.y)>StepHeight)return BaseMoveSpeed;   // a climb or a step down ahead
                ahead+=Vector3.Distance(from,p);from=p;
                if(ahead>CruiseLookahead)return MoveSpeed;
            }
            return BaseMoveSpeed;   // the last stretch
        }
        // Cutting a corner (the next waypoint is taken from 2.2 cm away) can press the body into what the corner went round:
        // level 17's half stuck on Q's casing for good at x0.85, and freed itself only by chance at x1. Stalled 0.8 s on the
        // flat with that corner's waypoint just behind, it goes back to the waypoint, all the way this time, then on; stalled
        // again there, it tries the next one again.
        private readonly float[] cornerSince=new float[32];private readonly Vector3[] cornerAt=new Vector3[32];
        private void RoundStalledCorner(int a,Order o,Vector3 centre)
        {
            float now=game.Matter.SimulationTime;
            if(o.Cursor>=o.Path.Count-1){cornerSince[a]=-1;return;}   // at its last waypoint: arrived, or holding there
            if(cornerSince[a]<0||now<cornerSince[a]||Vector3.Distance(centre,cornerAt[a])>.006f){cornerSince[a]=now;cornerAt[a]=centre;return;}
            if(now-cornerSince[a]<.8f)return;
            cornerSince[a]=now;
            if(o.Rounding){o.Rounding=false;if(o.Cursor<o.Path.Count-1)o.Cursor++;return;}
            if(o.Cursor<=0)return;
            Vector3 corner=game.Root.TransformPoint(o.Path[o.Cursor-1]);
            if(Vector3.Distance(centre,corner)>.04f||Mathf.Abs(corner.y-centre.y)>StepHeight)return;
            o.Cursor--;o.Rounding=true;
        }
        private Vector3 EdgeTarget(Order order,int anchor,Vector3 centre,Vector3 target)
        {
            // Inflate a convex turn by the living body's current footprint.
            // The point graph alone clears the head, but can leave the tail on
            // the other side of a thin panel when the next waypoint descends.
            foreach(var patch in game.Surfaces)
            {
                if(!patch.isActiveAndEnabled||(patch.Hole&&!patch.NavigationHoleBlocked)||game.IsHeldSurface(patch))continue;
                Vector3 goal=patch.transform.InverseTransformPoint(target);
                if(goal.z>=0)continue;
                bool blocked=false;
                for(int i=0;i<32&&!blocked;i++)
                {
                    if(game.Matter.Groups[i]!=game.Matter.Groups[anchor]||game.Matter.Escaped[i])continue;
                    Vector3 p=patch.transform.InverseTransformPoint(game.Matter.Bodies[i].position);
                    if(p.z<=0)continue;
                    Vector3 crossing=Vector3.Lerp(p,goal,p.z/(p.z-goal.z));
                    blocked=patch.ContainsForNavigation(crossing,game.Matter.Profile.ParticleRadius);
                }
                if(!blocked)continue;
                int crest=-1,axis=0;float sign=1,best=float.PositiveInfinity;Vector3 turn=Vector3.zero;
                for(int k=0;k<order.Path.Count;k++)
                {
                    Vector3 p=patch.transform.InverseTransformPoint(game.Root.TransformPoint(order.Path[k]));
                    if(Mathf.Abs(p.z)>.10f)continue;
                    for(int d=0;d<2;d++)
                    {
                        if(Mathf.Abs(p[d])<=patch.Size[d]*.5f+.002f)continue;
                        float cost=Mathf.Abs(k-order.Cursor)+Mathf.Abs(p[d])*.1f;
                        if(cost>=best)continue;
                        best=cost;crest=k;axis=d;sign=Mathf.Sign(p[d]);turn=p;
                    }
                }
                if(crest<0)continue;
                Vector3 localCentre=patch.transform.InverseTransformPoint(centre);
                float extent=0,depth=0;
                for(int i=0;i<32;i++)
                {
                    if(game.Matter.Groups[i]!=game.Matter.Groups[anchor]||game.Matter.Escaped[i])continue;
                    Vector3 p=patch.transform.InverseTransformPoint(game.Matter.Bodies[i].position)-localCentre;
                    extent=Mathf.Max(extent,-p[axis]*sign);depth=Mathf.Max(depth,p.z);
                }
                turn[axis]=sign*(patch.Size[axis]*.5f+extent+game.Matter.Profile.ParticleRadius+.008f);
                turn.z=-depth-game.Matter.Profile.ParticleRadius-.016f;
                // Going round the side of a carriage or block it is still in front of, the body first slides along the
                // face past its edge: aimed straight at the turn behind it, it pushed into the corner and stayed there (the
                // balcony carriage of levels 41 and 53, at every pace but x1, 21/10/2026). Fixed walls and panels, and
                // crests (over the top), keep the straight turn.
                var faceBody=patch.Shape!=null?patch.Shape.attachedRigidbody:null;
                Vector3 along=patch.transform.TransformDirection(axis==0?Vector3.right:Vector3.up);
                if(faceBody!=null&&faceBody!=game.Owner.Rotation.GetComponent<Rigidbody>()&&localCentre.z>0&&
                   Mathf.Abs(Vector3.Dot(patch.Normal,game.Root.up))<.3f&&Mathf.Abs(Vector3.Dot(along,game.Root.up))<.3f&&
                   localCentre[axis]*sign<patch.Size[axis]*.5f+extent+game.Matter.Profile.ParticleRadius&&
                   Mathf.Abs(goal[1-axis])<=patch.Size[1-axis]*.5f)   // round it, not up onto it
                {Vector3 slide=turn;slide.z=localCentre.z;slide[1-axis]=localCentre[1-axis];return patch.transform.TransformPoint(slide);}
                order.Cursor=Mathf.Min(order.Path.Count-1,Mathf.Max(order.Cursor,crest+1));
                return patch.transform.TransformPoint(turn);
            }
            return target;
        }
        private bool BodyCanReach(int anchor,Vector3 target)
        {
            for(int i=0;i<32;i++)
                if(game.Matter.Groups[i]==game.Matter.Groups[anchor]&&!game.Matter.Escaped[i]&&
                    !game.Clear(game.Matter.Bodies[i].position,target,game.Matter.Profile.ParticleRadius))return false;
            return true;
        }
        public void Step(float dt)
        {
            // Scene children are not enabled yet during the owner's Awake.
            // Build after they join the physics scene, not against an empty graph.
            if(nodes.Count==0&&(!game.Definition.Passive||game.Home))BuildGraph();
            if(contactWindows.Length!=game.Surfaces.Length)contactWindows=new VenomSurfaceSnapshot[game.Surfaces.Length];
            for(int s=0;s<contactWindows.Length;s++)
            {contactWindows[s].CaptureContact(game.Surfaces[s]);contactWindows[s].Active&=!game.IsHeldSurface(game.Surfaces[s]);}
            var chamberBody=game.Owner.Rotation.GetComponent<Rigidbody>();bool exitAvailable=game.FinalExitAvailable;
            // Establish local contact first. No force can be anchored to a remote surface.
            for(int i=0;i<32;i++)
            {
                support[i]=null;intent[i]=Vector3.zero;
                var p=game.Matter.Bodies[i].position;float best=.034f;
                for(int s=0;s<contactWindows.Length;s++)
                {
                    ref var window=ref contactWindows[s];
                    if(!window.Active||p.x<window.SkinMin.x||p.x>window.SkinMax.x||p.y<window.SkinMin.y||p.y>window.SkinMax.y||p.z<window.SkinMin.z||p.z>window.SkinMax.z)continue;
                    var patch=window.Patch;
                    var local=patch.transform.InverseTransformPoint(p);
                    float inside=patch.Curved!=null?patch.Curved.DistanceInside(local):patch.SphereRadius>0?patch.SphereRadius-local.magnitude:local.z;
                    if(inside<-.002f||inside>.036f||!patch.Contains(local,.001f))continue;
                    Vector3 q=patch.Closest(p);float d=Vector3.Distance(q,p);
                    // The deformable skin extends beyond each particle centre.
                    // A catch is local to its contact envelope, never a reach from the ceiling.
                    float limit=patch.RingGrip&&patch.Grip(q)?.021f:.034f;
                    if(d>=best||d>limit)continue;
                    best=d;support[i]=patch;contact[i]=q;
                }
            }
            var seen=seenGroups;seen.Clear();
            for(int a=0;a<32;a++)
            {
                if(game.Matter.Escaped[a]||!seen.Add(game.Matter.Groups[a]))continue;
                Vector3 centre=Centre(a);Order o=Get(a);Vector3 target=centre;
                int grips=0;float mass=0,contactMass=0,gripMass=0,strain=0;Vector3 momentum=Vector3.zero;
                for(int i=0;i<32;i++)
                {
                    if(game.Matter.Groups[i]!=game.Matter.Groups[a]||game.Matter.Escaped[i])continue;
                    mass+=game.Matter.Bodies[i].mass;strain=Mathf.Max(strain,gripStrain[i]);
                    momentum+=game.Matter.Bodies[i].linearVelocity*game.Matter.Bodies[i].mass;
                    if(support[i]!=null)contactMass+=game.Matter.Bodies[i].mass;
                    if(HasGrip(i)){grips++;gripMass+=game.Matter.Bodies[i].mass;}
                }
                float capacity=mass*GripAccelerationLimit*gripMass/Mathf.Max(contactMass,.0001f);
                var caught=CatchRing(a,centre,momentum/Mathf.Max(mass,.0001f));o=Get(a);
                if(caught!=null){capacity=mass*GripAccelerationLimit;strain=0;}
                float weight=mass*9.81f;
                strain=grips>=2&&capacity<weight?strain+dt*8*(weight-capacity)/weight:Mathf.Max(0,strain-dt*.25f);
                bool suppressed=game.MechanismSuppressesMotion(a)||game.IsFlowing(a);
                if(o!=null&&!o.Holding&&!o.Exit&&caught==null&&!suppressed&&!game.Attached&&!game.InTube)FreeTunnelledNodes(a,centre);else stallSince[a]=-1;
                bool peeled=strain>.14f&&!suppressed;
                HashSet<VenomSurfacePatch> released=null;
                if(peeled)
                {
                    released=new HashSet<VenomSurfacePatch>();
                    for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[a]&&HasGrip(i))released.Add(support[i]);
                }
                for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[a])
                {
                    gripStrain[i]=peeled?0:strain;
                    if(peeled){detachedUntil[i]=game.Matter.SimulationTime+.45f;detachedSurfaces[i]=released;}
                }
                if(peeled)
                {
                    // On glass COghe may climb (glass-crawl levels, 12/10/2026) a peel while the next waypoint lies below
                    // the body is a fall toward the goal: the order waits for the landing and is replanned there. Elsewhere,
                    // and on the way up, a peel ends the order (a climb that gave up). A floor bears the body, so it does
                    // not peel there.
                    bool downhill=o!=null&&ClimbableGlass()&&o.Path.Count>0&&game.Root.TransformPoint(o.Path[Mathf.Min(o.Cursor,o.Path.Count-1)]).y<centre.y-.01f;
                    if(o==null||!o.AwaitingContact&&!downhill){Cancel(a);o=null;}
                    else {o.Cursor=0;o.AwaitingContact=true;}
                    grips=0;capacity=0;
                }
                if(o!=null)
                {
                    if(grips<2)o.AwaitingContact=true;
                    else if(o.AwaitingContact){FindPath(centre,game.Root.TransformPoint(o.Target),o.Path,o.AvoidSlippery);o.Cursor=0;o.AwaitingContact=false;}
                }
                if(o!=null&&grips>=2&&caught==null&&!suppressed&&!game.InTube&&!game.Attached)RoundStalledCorner(a,o,centre);else cornerSince[a]=-1;
                if(o!=null)
                {
                    while(o.Cursor<o.Path.Count-1&&Vector3.Distance(centre,game.Root.TransformPoint(o.Path[o.Cursor]))<(o.Rounding?.008f:.022f)){o.Cursor++;o.Rounding=false;}
                    target=game.Root.TransformPoint(o.Path[o.Cursor]);
                    if(Vector3.Distance(centre,game.Root.TransformPoint(o.Target))<.023f&&!o.Holding&&!o.Exit&&
                        BodyCanReach(a,game.Root.TransformPoint(o.Target))){Cancel(a);o=null;}
                }
                bool anchored=(grips>=2||caught!=null)&&(!game.Definition.Passive||game.Home)&&!suppressed;
                if(o!=null&&caught==null&&anchored)target=EdgeTarget(o,a,centre,target);
                Vector3 delta=target-centre;
                float pace=o!=null?PaceFor(o,centre):MoveSpeed;
                Vector3 desired=o!=null?delta.normalized*pace:Vector3.zero;
                // the last stretch eases in, so COghe stops exactly where it is sent
                if(o!=null&&o.Cursor==o.Path.Count-1)desired=Vector3.ClampMagnitude(delta*4,pace);
                if(caught!=null)desired=Vector3.ClampMagnitude((caught.Surface.transform.TransformPoint(caught.LocalTarget)-centre)*6,.35f);
                bool railManipulation=game.TryRailManipulationIntent(a,out var handleTarget,out var handleVelocity);
                if(railManipulation){delta=handleTarget-centre;desired=handleVelocity;}
                Vector3 passiveIntent=Vector3.zero;
                if(o!=null&&!game.Home&&(game.Definition.Passive||(!suppressed&&!anchored&&contactMass>0)))
                {
                    Vector3 normal=Vector3.zero;
                    for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[a]&&support[i]!=null)
                        normal+=support[i].NormalAt(contact[i]);
                    normal=normal.sqrMagnitude>.000001f?normal.normalized:Vector3.up;
                    foreach(var s in game.Surfaces)if(s.IsCurved){normal=s.NormalAt(centre);break;}
                    // A skidding body cannot follow navigation waypoints. Its
                    // visual effort faces the player's chosen point even when
                    // inertia has carried it past an unreached route node.
                    Vector3 commanded=game.Root.TransformPoint(o.Target)-centre;
                    passiveIntent=Vector3.ProjectOnPlane(commanded,normal);
                    passiveIntent.Normalize();
                }
                for(int i=0;i<32;i++)
                {
                    if(game.Matter.Groups[i]!=game.Matter.Groups[a]||game.Matter.Escaped[i])continue;
                    Rigidbody body=game.Matter.Bodies[i];var patch=support[i];
                    bool atExit=game.ExitAssisting(i,exitAvailable);
                    game.Matter.SetFlow(i,game.IsFlowing(i)||atExit?1:.12f);
                    // Visual effort is independent of traction. Slick contact
                    // receives no drive, adhesion or gravity cancellation.
                    intent[i]=atExit?Vector3.zero:passiveIntent;
                    if(!anchored||atExit)continue;
                    // Distribute the finite force transmitted by planted feet
                    // through the connected body. Gravity keeps acting on all
                    // tissue, including the unsupported head over a slick patch.
                    Vector3 relative=body.linearVelocity-(patch!=null&&patch.MotionFrame!=null?patch.MotionFrame:chamberBody).GetPointVelocity(body.position);
                    Vector3 acceleration=Vector3.up*9.81f;
                    bool manipulating=game.Attached&&game.Matter.Groups[a]==game.Matter.Groups[Selected];
                    // a slower (hungry) COghe walks slower but pushes as hard when something stops it: a step, a tray lip,
                    // a block to climb (the drive was proportional to the pace, and x0.85 could not climb level 42's tray
                    // or level 58's block); at the usual pace or faster nothing changes
                    float drive=caught!=null?35:26*Mathf.Max(1,BaseMoveSpeed/pace);
                    if(!manipulating)acceleration+=Vector3.ClampMagnitude((desired-relative)*drive,caught!=null?18:5);
                    acceleration=Vector3.ClampMagnitude(acceleration,capacity/Mathf.Max(mass,.0001f));
                    intent[i]=desired/pace;
                    if(HasGrip(i))
                    {
                        Vector3 n=patch.Normal;
                        float distance=Vector3.Dot(body.position-contact[i],n);
                        float grip=(o!=null||railManipulation)&&Vector3.Dot(delta,n)>.018f?0:1;
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
