using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// A physical, bidirectional graph of hollow tubes.  Commands select one
    /// adjacent edge; the controller never searches ahead for an exit.
    /// </summary>
    public sealed class COgheTubeNetwork : COgheMechanism
    {
        public enum TerminalKind { Junction, Entry, Closed, Exit }

        [Serializable]
        public sealed class Node
        {
            public string Name;
            public Vector3 LocalPosition;
            public Vector3 LocalOutward;
            public TerminalKind Terminal;

            public Node(string name, Vector3 position, TerminalKind terminal=TerminalKind.Junction)
            { Name=name;LocalPosition=position;Terminal=terminal;LocalOutward=Vector3.zero; }

            public Node(string name, Vector3 position, TerminalKind terminal, Vector3 outward)
            { Name=name;LocalPosition=position;Terminal=terminal;LocalOutward=outward; }
        }

        [Serializable]
        public sealed class Edge
        {
            public string Name;
            public int A,B;
            public bool Flexible, FlexibleTip;
            public Vector3[] ControlPoints;
            public Transform Geometry;
            public MeshFilter Filter;
            public MeshCollider Collider;

            [NonSerialized] internal Vector3[] Path;
            [NonSerialized] internal float[] Distance;
            [NonSerialized] internal Vector3[] Velocity;
            [NonSerialized] internal float Length;
            [NonSerialized] internal Bounds LocalBounds;
            [NonSerialized] internal Mesh RuntimeMesh;

            public Edge(string name,int a,int b,params Vector3[] controlPoints)
            {Name=name;A=a;B=b;ControlPoints=controlPoints;}

            public Edge FlexibleFromRoot(bool freeTip=true)
            {Flexible=true;FlexibleTip=freeTip;return this;}
        }

        private sealed class Travel
        {
            public int Anchor,Node=-1,Edge=-1,Direction,CommandId,PendingEdge=-1,ArrivalEdge=-1,ArrivalDirection;
        }

        public Node[] Nodes=Array.Empty<Node>();
        public Edge[] Edges=Array.Empty<Edge>();
        public float Radius=.035f;
        public float JunctionRadius=.052f;
        public Collider EntryBlocker;
        public float EntryClearance=.012f;
        public bool GateOpenOverride=true;
        public bool AutoCaptureEntries;
        [Tooltip("While inside this network, taps can only choose connected pipe branches; chamber surfaces do not receive movement commands.")]
        public bool CaptureSurfaceCommandsWhileInside;
        public bool SolidExterior;
        public bool HighlightLastCommand=true;
        public int FlexibleGeometryInterval=3;

        public int LastChosenEdge {get;private set;}=-1;
        public int LastReachedNode {get;private set;}=-1;
        public bool ExitReached {get;private set;}
        public override bool ControlsExit=>HasExit;
        public override bool ExitUnlocked=>!ControlsExit||ExitReached;
        public override string Activity=>AnyTravelling?"Đang luồn qua ống":AnyApproaching?"Đang vào ống":
            AnyChoice?"Chọn một nhánh nối":AnyWaiting?"Chạm ống để quay lại":null;
        public bool HasExit {get;private set;}
        public bool AnyTravelling {get;private set;}
        public bool AnyWaiting {get;private set;}
        public bool AnyApproaching {get;private set;}
        public bool AnyChoice {get;private set;}

        private readonly Travel[] travelByParticle=new Travel[CohesiveOrganism.ParticleCount];
        private readonly int[] blockedAutoEntry=new int[CohesiveOrganism.ParticleCount];
        private readonly int[] queuedEntry=new int[CohesiveOrganism.ParticleCount];
        private readonly int[] queuedEdge=new int[CohesiveOrganism.ParticleCount];
        private readonly Vector3[] queuedApproach=new Vector3[CohesiveOrganism.ParticleCount];
        private readonly List<int>[] adjacency=new List<int>[64];
        private VenomCampaign game;
        private Transform root;
        private int flexibleTick;
        private int nextTravelCommand;
        private Collider[] solidColliders=Array.Empty<Collider>();
        private Matrix4x4 skinToLocal,skinToWorld;
        private Quaternion skinRotation;
        private int skinFrame=-1;
        private int skinEntryCount;
        private float skinShellRadius;
        public override bool HasSkinConstraint=>true;
        public override bool TransportsTissue=>true;
        public override bool SeparatesTissue=>true;
        public override void PrepareSkinFrame()
        {
            if(root==null)return;
            skinToLocal=root.worldToLocalMatrix;skinToWorld=root.localToWorldMatrix;skinRotation=root.rotation;skinFrame=Time.frameCount;
            skinEntryCount=0;skinShellRadius=0;
            foreach(Node node in Nodes)if(node.Terminal==TerminalKind.Entry){skinShellRadius+=node.LocalPosition.magnitude;skinEntryCount++;}
            if(skinEntryCount>0)skinShellRadius/=skinEntryCount;
        }

        public void Configure(Node[] nodes,Edge[] edges,float radius,float junctionRadius=.052f)
        {Nodes=nodes??Array.Empty<Node>();Edges=edges??Array.Empty<Edge>();Radius=radius;JunctionRadius=junctionRadius;PrepareGraph();}

        public override void InitializeMechanism(VenomCampaign owner)
        {
            game=owner;root=owner.Root;PrepareGraph();
            for(int i=0;i<queuedEntry.Length;i++){queuedEntry[i]=-1;queuedEdge[i]=-1;}
            solidColliders=GetComponentsInChildren<Collider>(true);
            SyncExitDetector();
        }

        public override void ResetMechanism(VenomCampaign owner)
        {
            game=owner;root=owner.Root;Array.Clear(travelByParticle,0,travelByParticle.Length);
            for(int i=0;i<blockedAutoEntry.Length;i++){blockedAutoEntry[i]=-1;queuedEntry[i]=queuedEdge[i]=-1;}
            LastChosenEdge=LastReachedNode=-1;ExitReached=false;AnyTravelling=AnyWaiting=AnyApproaching=AnyChoice=false;flexibleTick=nextTravelCommand=0;
            PrepareGraph(true);solidColliders=GetComponentsInChildren<Collider>(true);SyncExitDetector();
        }

        public override bool SuppressesMotion(int particle)
        {return particle>=0&&particle<travelByParticle.Length&&travelByParticle[particle]!=null;}

        public override bool IsFlowing(int particle)=>SuppressesMotion(particle);

        public override bool MayConstrainSkin(Bounds worldBounds)
        {
            if(root==null)return false;
            // The legacy multi-mouth sphere also supplies shell normals away
            // from the tube bodies, so retain its exact per-vertex query.
            if(skinEntryCount>=4)return true;
            Vector3 centre=skinToLocal.MultiplyPoint3x4(worldBounds.center),e=worldBounds.extents;
            Vector3 x=skinToLocal.MultiplyVector(Vector3.right*e.x),y=skinToLocal.MultiplyVector(Vector3.up*e.y),z=skinToLocal.MultiplyVector(Vector3.forward*e.z);
            Vector3 extent=new Vector3(Mathf.Abs(x.x)+Mathf.Abs(y.x)+Mathf.Abs(z.x),Mathf.Abs(x.y)+Mathf.Abs(y.y)+Mathf.Abs(z.y),Mathf.Abs(x.z)+Mathf.Abs(y.z)+Mathf.Abs(z.z));
            var fragment=new Bounds(centre,(extent+Vector3.one*(Radius+.0301f))*2);
            foreach(var edge in Edges)
                if(edge.Path!=null&&edge.Path.Length>=2&&fragment.Intersects(edge.LocalBounds))return true;
            return false;
        }

        public override bool ConstrainSkin(ref Vector3 world,out Vector3 normal)
        {
            normal=Vector3.up;if(root==null)return false;
            if(skinFrame!=Time.frameCount)PrepareSkinFrame();
            Vector3 localWorld=skinToLocal.MultiplyPoint3x4(world);float best=float.PositiveInfinity;
            Vector3 bestCentre=Vector3.zero;
            foreach(Edge edge in Edges)
            {
                if(edge.Path==null||edge.Path.Length<2)continue;
                float reach=Radius+.03f;
                Vector3 min=edge.LocalBounds.min,max=edge.LocalBounds.max;
                float dx=Mathf.Max(Mathf.Max(min.x-localWorld.x,0),localWorld.x-max.x),dy=Mathf.Max(Mathf.Max(min.y-localWorld.y,0),localWorld.y-max.y),dz=Mathf.Max(Mathf.Max(min.z-localWorld.z,0),localWorld.z-max.z);
                if(dx*dx+dy*dy+dz*dz>reach*reach)continue;
                Closest(edge,localWorld,out Vector3 centre,out _,out _);
                float distance=Vector3.Distance(localWorld,centre);
                if(distance<best){best=distance;bestCentre=centre;}
            }
            if(best>Radius+.03f)return IsApertureSkin(localWorld,ref normal);
            Vector3 radial=localWorld-bestCentre;
            if(radial.sqrMagnitude<.000001f)radial=Vector3.up;
            normal=skinRotation*radial.normalized;
            if(best>Radius-.0005f)world=skinToWorld.MultiplyPoint3x4(bestCentre+radial.normalized*(Radius-.0005f));
            return true;
        }

        private bool IsApertureSkin(Vector3 localWorld,ref Vector3 normal)
        {
            if(skinEntryCount<4)return false;float shellRadius=skinShellRadius;
            if(shellRadius<.1f||Mathf.Abs(localWorld.magnitude-shellRadius)>.032f)return false;
            Vector3 direction=localWorld.normalized;
            foreach(Node node in Nodes)
            {
                if(node.Terminal!=TerminalKind.Entry||node.LocalPosition.sqrMagnitude<.01f)continue;
                if(Vector3.Cross(direction,node.LocalPosition.normalized).magnitude>Radius/shellRadius*1.35f)continue;
                normal=root.TransformDirection(direction);return true;
            }
            return false;
        }

        public override bool BlocksFusion(int a,int b)
        {
            if(game==null||a<0||b<0||a>=32||b>=32)return false;
            return SolidSeparates(game.Matter.Bodies[a].position,game.Matter.Bodies[b].position);
        }

        public override bool AllowsExitAssist(int particle,Vector3 capturePoint)
        {
            if(game==null||particle<0||particle>=travelByParticle.Length)return true;
            Travel travel=travelByParticle[particle];
            if(travel==null||travel.Edge<0)return true;
            Edge edge=Edges[travel.Edge];int target=travel.Direction>0?edge.B:edge.A;
            if(Nodes[target].Terminal!=TerminalKind.Exit)return true;
            if(SolidExterior)
            {
                Closest(edge,root.InverseTransformPoint(game.Matter.Bodies[particle].position),out _,out _,out float along);
                float remaining=travel.Direction>0?edge.Length-along:along;
                if(remaining>Radius*.5f)return false;
            }
            // Campaign recovery assistance pulls in a straight line. Keep the
            // tube's centreline servo in control until that line lies wholly
            // inside the real final collar; otherwise it cuts across a curved
            // wall and strands the tail against its collider.
            return !SolidSeparates(game.Matter.Bodies[particle].position,capturePoint);
        }

        /// <summary>True only when the segment crosses real tube or cap geometry.</summary>
        public bool SolidSeparates(Vector3 first,Vector3 second)
        {return SolidSeparates(first,second,out _);}

        private bool SolidSeparates(Vector3 first,Vector3 second,out Collider blocker)
        {
            blocker=null;
            Vector3 delta=second-first;float distance=delta.magnitude;
            if(distance<.00001f)return false;
            Ray forward=new Ray(first,delta/distance),reverse=new Ray(second,-delta/distance);
            foreach(Collider solid in solidColliders)
            {
                if(solid==null||!solid.enabled||!solid.gameObject.activeInHierarchy||solid.isTrigger)continue;
                if(solid.Raycast(forward,out _,distance)||solid.Raycast(reverse,out _,distance)){blocker=solid;return true;}
            }
            return false;
        }

        public bool IsParticleInside(int particle)=>SuppressesMotion(particle);

        public bool IsEntryOpen(int node)
        {
            if(!GateOpenOverride||node<0||node>=Nodes.Length||Nodes[node].Terminal!=TerminalKind.Entry)return false;
            if(EntryBlocker==null||!EntryBlocker.enabled||!EntryBlocker.gameObject.activeInHierarchy)return true;
            Vector3 mouth=NodeWorld(node),closest=EntryBlocker.ClosestPoint(mouth);
            return Vector3.Distance(closest,mouth)>Radius+EntryClearance;
        }

        public override bool TryTouch(VenomCampaign owner,Ray ray,float nearestSolidDistance)
        {
            if(game==null)InitializeMechanism(owner);
            int anchor=owner.Motion.Selected;Travel travel=TravelFor(anchor);
            if(travel==null)
            {
                int entry=PickVisibleEntry(ray,nearestSolidDistance);
                if(entry<0)return false;
                if(!GroupContactsEntry(anchor,entry)||!EntryClearForGroup(anchor,entry))
                {QueueEntryApproach(anchor,entry,adjacency[entry].Count==1?adjacency[entry][0]:-1);return true;}
                ClearQueuedEntry(anchor);travel=BeginAtNode(anchor,entry);
                if(adjacency[entry].Count==1)return TryChoose(anchor,adjacency[entry][0]);
                return true;
            }
            if(travel.Edge>=0)return CaptureSurfaceCommandsWhileInside;
            int chosen=PickAdjacent(travel.Node,ray,nearestSolidDistance);
            if(chosen>=0&&TryChoose(anchor,chosen))return true;
            // Consume invalid taps as well, so the campaign cannot fall through
            // to a surface destination or show a misleading touch marker.
            return CaptureSurfaceCommandsWhileInside;
        }

        public bool TryChoose(int anchor,int edgeIndex)
        {
            if(game==null||edgeIndex<0||edgeIndex>=Edges.Length)return false;
            Travel travel=TravelFor(anchor);
            if(travel==null)
            {
                int node=EntryNearGroup(anchor);
                if(node<0)return false;
                if(!EntryClearForGroup(anchor,node)){QueueEntryApproach(anchor,node,edgeIndex);return true;}
                travel=BeginAtNode(anchor,node);
            }
            if(travel.Edge>=0||travel.Node<0||!adjacency[travel.Node].Contains(edgeIndex))return false;
            travel.CommandId=++nextTravelCommand;
            LastChosenEdge=edgeIndex;
            game.Motion.Cancel(anchor);
            if(!NodeReadyForDeparture(travel,edgeIndex)){travel.PendingEdge=edgeIndex;return true;}
            StartEdge(travel,edgeIndex);
            return true;
        }

        private void StartEdge(Travel travel,int edgeIndex)
        {
            Edge edge=Edges[edgeIndex];
            travel.Edge=edgeIndex;travel.Direction=edge.A==travel.Node?1:-1;travel.Node=-1;travel.PendingEdge=-1;
        }

        private bool NodeReadyForDeparture(Travel travel,int edgeIndex)
        {
            if(travel.Node<0||Nodes[travel.Node].Terminal!=TerminalKind.Junction)return true;
            Edge edge=Edges[edgeIndex];bool forward=edge.A==travel.Node;
            Vector3 localNode=Nodes[travel.Node].LocalPosition;
            Vector3 tangent=(forward?edge.Path[1]-edge.Path[0]:edge.Path[edge.Path.Length-2]-edge.Path[edge.Path.Length-1]).normalized;
            Vector3 staging=root.TransformPoint(localNode+tangent*(JunctionRadius*.45f));
            int group=game.Matter.Groups[travel.Anchor];
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&!game.Matter.Escaped[i]&&
                SolidSeparates(game.Matter.Bodies[i].position,staging))return false;
            return true;
        }

        private bool JunctionContainsCentre(int node,Vector3 world)
        {
            Vector3 offset=root.InverseTransformPoint(world)-Nodes[node].LocalPosition;
            float particleRadius=game.Matter.Profile.ParticleRadius;
            if(offset.magnitude<=JunctionRadius-particleRadius)return true;
            foreach(int edgeIndex in adjacency[node])
            {
                Edge edge=Edges[edgeIndex];Vector3 direction=edge.A==node?
                    (edge.Path[1]-edge.Path[0]).normalized:
                    (edge.Path[edge.Path.Length-2]-edge.Path[edge.Path.Length-1]).normalized;
                float axial=Vector3.Dot(offset,direction);
                if(axial<0||axial>JunctionRadius)continue;
                if(Vector3.ProjectOnPlane(offset,direction).magnitude<=Radius-particleRadius)return true;
            }
            return false;
        }

        public override void StepMechanism(VenomCampaign owner,float dt)
        {
            if(game==null)InitializeMechanism(owner);
            StepFlexible(dt);
            StepQueuedEntries();
            AutoCaptureContactingGroups();
            ReconcileGroups();
            AnyTravelling=AnyWaiting=AnyChoice=false;AnyApproaching=false;
            for(int i=0;i<queuedEntry.Length;i++)if(queuedEntry[i]>=0){AnyApproaching=true;break;}
            var seen=new HashSet<Travel>();
            for(int i=0;i<travelByParticle.Length;i++)
            {
                Travel travel=travelByParticle[i];
                if(travel==null||!seen.Add(travel))continue;
                if(travel.Edge>=0){AnyTravelling=true;StepEdge(travel,dt);}
                else
                {
                    AnyWaiting=true;AnyApproaching|=travel.PendingEdge>=0;
                    AnyChoice|=travel.PendingEdge<0&&travel.Node>=0&&adjacency[travel.Node].Count>1;
                    HoldAtNode(travel,dt);
                    if(travel.PendingEdge>=0&&NodeReadyForDeparture(travel,travel.PendingEdge))StartEdge(travel,travel.PendingEdge);
                }
            }
        }

        private void StepEdge(Travel travel,float dt)
        {
            Edge edge=Edges[travel.Edge];
            if(edge.Path==null||edge.Path.Length<2)return;
            float nearestEnd=float.PositiveInfinity,beyondSum=0;bool allArrived=true,allCrossed=true;int particleCount=0;
            int target=travel.Direction>0?edge.B:edge.A;
            TerminalKind terminal=Nodes[target].Terminal;bool openTerminal=terminal==TerminalKind.Entry||terminal==TerminalKind.Exit;
            const float openLead=.065f;
            Vector3 endpoint=NodeWorld(target);Vector3 localEndpoint=root.InverseTransformPoint(endpoint);
            Vector3 terminalTangent=travel.Direction>0?
                (edge.Path[edge.Path.Length-1]-edge.Path[edge.Path.Length-2]).normalized:
                (edge.Path[0]-edge.Path[1]).normalized;
            Rigidbody frame=root.GetComponent<Rigidbody>();
            for(int i=0;i<32;i++)
            {
                if(game.Matter.Groups[i]!=game.Matter.Groups[travel.Anchor]||game.Matter.Escaped[i])continue;
                var body=game.Matter.Bodies[i];Vector3 localBody=root.InverseTransformPoint(body.position);
                particleCount++;
                Closest(edge,localBody,out Vector3 local,out Vector3 tangent,out float along);
                if(travel.Direction<0)tangent=-tangent;
                float beyond=Vector3.Dot(localBody-localEndpoint,terminalTangent);
                beyondSum+=beyond;
                if(terminal==TerminalKind.Exit&&beyond>=-.004f&&
                    Vector3.ProjectOnPlane(localBody-localEndpoint,terminalTangent).magnitude<game.Owner.ApertureRadius)
                    ExitReached=true;
                // Continue the centreline only through a short receiving lead.
                // Past that point the longitudinal error brakes the head while
                // the tail physically clears the actual terminal plane.
                if(openTerminal&&beyond>0){tangent=terminalTangent;local=localEndpoint+tangent*Mathf.Min(beyond,openLead);}
                Vector3 centre=root.TransformPoint(local),worldTangent=root.TransformDirection(tangent).normalized;
                Vector3 radial=centre-body.position;
                Vector3 relative=body.linearVelocity-(frame!=null?frame.GetPointVelocity(body.position):Vector3.zero);
                float remaining=travel.Direction>0?edge.Length-along:along;
                nearestEnd=Mathf.Min(nearestEnd,remaining);
                // Decelerate to zero at the chamber centre. A non-zero minimum
                // speed makes the head cross the junction while the tail is
                // still approaching, so the whole body can never be resident
                // in the chamber at once and branch selection never unlocks.
                float speed=openTerminal?.11f*Mathf.Clamp01((openLead-beyond)/.035f):Mathf.Min(.13f,Mathf.Max(0,remaining)*3f);
                // Tissue that has crossed an open mouth can spread into the
                // receiving room. Keeping full radial centering there packs a
                // large fragment into a one-particle-wide plug and strands its
                // tail inside an otherwise clear bore.
                float radialDrive=openTerminal&&beyond>0?Mathf.Clamp01(1-beyond/openLead):1;
                Vector3 desired=worldTangent*speed+Vector3.ClampMagnitude(radial*8*radialDrive,.12f);
                int source=travel.Direction>0?edge.A:edge.B;
                if(SolidExterior&&Nodes[source].Terminal==TerminalKind.Entry)
                {
                    Vector3 mouth=NodeWorld(source),axis=EntryInwardWorld(source);
                    Vector3 offset=body.position-mouth;
                    float depth=Vector3.Dot(offset,axis);
                    float skin=game.Matter.Profile.ParticleRadius;
                    float offAxis=Vector3.ProjectOnPlane(offset,axis).magnitude;
                    float fromEntry=travel.Direction>0?along:edge.Length-along;
                    // Gather the tail in front of the actual mouth before
                    // advancing it. Pulling a spread tail towards the nearest
                    // curve point would press it against a solid outer wall.
                    if(fromEntry<Radius&&depth<Radius*1.5f&&offAxis>Radius-skin-.003f)
                        desired=Vector3.ClampMagnitude((mouth-axis*(skin+.012f)-body.position)*6,.14f);
                }
                Vector3 acceleration=Vector3.up*9.81f+Vector3.ClampMagnitude((desired-relative)*24,7);
                // Once the real exit detector captures a particle, the shared
                // campaign exit controller owns its force. Applying both
                // gravity compensation servos makes them fight after rotation.
                if(!(terminal==TerminalKind.Exit&&game.ExitAssisting(i)))body.AddForce(acceleration,ForceMode.Acceleration);
                game.Matter.SetFlow(i,1);
                float endpointDistance=Vector3.Distance(body.position,endpoint);
                // The real chamber is the union of its sphere and connected
                // bore mouths. Count a centre only where its particle skin fits
                // that physical envelope; averages never discard route history.
                if(terminal==TerminalKind.Junction&&!JunctionContainsCentre(target,body.position))allArrived=false;
                float crossingDepth=game.Matter.Profile.ParticleRadius+.002f;
                if(beyond<crossingDepth)allCrossed=false;
            }
            float averageBeyond=particleCount>0?beyondSum/particleCount:float.NegativeInfinity;
            float requiredLead=openLead*.48f;
            if(openTerminal?(!allCrossed||averageBeyond<requiredLead):terminal==TerminalKind.Closed?nearestEnd>Radius*.5f:!allArrived||nearestEnd>JunctionRadius*.72f)return;
            travel.ArrivalEdge=travel.Edge;travel.ArrivalDirection=travel.Direction;
            travel.Node=target;travel.Edge=-1;travel.PendingEdge=-1;LastReachedNode=target;
            if(Nodes[target].Terminal==TerminalKind.Exit)
            {
                ExitReached=true;
                for(int i=0;i<32;i++)if(travelByParticle[i]==travel)travelByParticle[i]=null;
            }
            else if(Nodes[target].Terminal==TerminalKind.Entry)
            {
                // A room-to-room transfer has an open mouth at both ends. Once
                // the trailing tissue clears the distal mouth, ordinary room
                // locomotion resumes and either mouth can be used in reverse.
                BlockAutoCapture(travel,target);
                for(int i=0;i<32;i++)if(travelByParticle[i]==travel)travelByParticle[i]=null;
            }
        }

        private void HoldAtNode(Travel travel,float dt)
        {
            Vector3 target=NodeWorld(travel.Node);Rigidbody frame=root.GetComponent<Rigidbody>();
            for(int i=0;i<32;i++)
            {
                if(game.Matter.Groups[i]!=game.Matter.Groups[travel.Anchor]||game.Matter.Escaped[i])continue;
                Rigidbody body=game.Matter.Bodies[i];
                Vector3 relative=body.linearVelocity-(frame!=null?frame.GetPointVelocity(body.position):Vector3.zero);
                if(travel.PendingEdge>=0&&travel.ArrivalEdge>=0&&Vector3.Distance(body.position,target)>JunctionRadius*.82f)
                {
                    Edge incoming=Edges[travel.ArrivalEdge];
                    Closest(incoming,root.InverseTransformPoint(body.position),out Vector3 local,out Vector3 tangent,out float along);
                    if(travel.ArrivalDirection<0)tangent=-tangent;
                    float remaining=travel.ArrivalDirection>0?incoming.Length-along:along;
                    Vector3 centre=root.TransformPoint(local),worldTangent=root.TransformDirection(tangent).normalized;
                    Vector3 routedDesired=worldTangent*Mathf.Min(.10f,Mathf.Max(0,remaining)*3f)+Vector3.ClampMagnitude((centre-body.position)*8,.12f);
                    body.AddForce(Vector3.up*9.81f+Vector3.ClampMagnitude((routedDesired-relative)*24,7),ForceMode.Acceleration);
                    game.Matter.SetFlow(i,.8f);continue;
                }
                Vector3 desired=Vector3.ClampMagnitude((target-body.position)*3,.065f);
                body.AddForce(Vector3.up*9.81f+Vector3.ClampMagnitude((desired-relative)*18,5),ForceMode.Acceleration);
                game.Matter.SetFlow(i,.55f);
            }
        }

        private Travel BeginAtNode(int anchor,int node)
        {
            ClearQueuedEntry(anchor);
            var travel=new Travel{Anchor=anchor,Node=node,CommandId=++nextTravelCommand};int group=game.Matter.Groups[anchor];
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group)travelByParticle[i]=travel;
            game.Motion.Cancel(anchor);LastReachedNode=node;return travel;
        }

        private void AutoCaptureContactingGroups()
        {
            if(!AutoCaptureEntries)return;
            var groups=new HashSet<int>();
            for(int anchor=0;anchor<32;anchor++)
            {
                int group=game.Matter.Groups[anchor];if(game.Matter.Escaped[anchor]||!groups.Add(group)||TravelFor(anchor)!=null)continue;
                int blocked=blockedAutoEntry[anchor];
                if(blocked>=0)
                {
                    bool clear=true;Vector3 oldMouth=NodeWorld(blocked);
                    for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&!game.Matter.Escaped[i]&&
                        Vector3.Distance(game.Matter.Bodies[i].position,oldMouth)<Radius*2f){clear=false;break;}
                    if(clear)for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group)blockedAutoEntry[i]=-1;
                }
                for(int node=0;node<Nodes.Length;node++)
                {
                    if(node==blocked)continue;
                    if(Nodes[node].Terminal!=TerminalKind.Entry||!IsEntryOpen(node)||adjacency[node].Count!=1)continue;
                    Vector3 mouth=NodeWorld(node);int contacts=0;
                    for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&!game.Matter.Escaped[i]&&
                        Vector3.Distance(game.Matter.Bodies[i].position,mouth)<Radius*1.5f)contacts++;
                    if(contacts<2||!EntryClearForGroup(anchor,node))continue;
                    BeginAtNode(anchor,node);TryChoose(anchor,adjacency[node][0]);break;
                }
            }
        }

        private void BlockAutoCapture(Travel travel,int entry)
        {for(int i=0;i<32;i++)if(travelByParticle[i]==travel)blockedAutoEntry[i]=entry;}

        private Travel TravelFor(int anchor)
        {
            if(anchor<0||anchor>=32)return null;
            Travel direct=travelByParticle[anchor];if(direct!=null)return direct;
            int group=game.Matter.Groups[anchor];
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&travelByParticle[i]!=null)return travelByParticle[i];
            return null;
        }

        private int EntryNearGroup(int anchor)
        {
            Vector3 centre=game.Motion.Centre(anchor);int best=-1;float distance=.11f;
            for(int n=0;n<Nodes.Length;n++)
            {
                if(Nodes[n].Terminal!=TerminalKind.Entry||!IsEntryOpen(n))continue;
                float d=Vector3.Distance(centre,NodeWorld(n));if(d<distance){distance=d;best=n;}
            }
            return best;
        }

        private int PickVisibleEntry(Ray ray,float limit)
        {
            int best=-1;float score=float.PositiveInfinity;
            for(int node=0;node<Nodes.Length;node++)
            {
                if(Nodes[node].Terminal!=TerminalKind.Entry||!IsEntryOpen(node))continue;
                if(!RayScore(ray,NodeWorld(node),limit,out float candidate)||candidate>=score)continue;
                best=node;score=candidate;
            }
            return best;
        }

        private bool GroupContactsEntry(int anchor,int node)
        {
            int group=game.Matter.Groups[anchor],contacts=0;Vector3 mouth=NodeWorld(node);
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&!game.Matter.Escaped[i]&&
                Vector3.Distance(game.Matter.Bodies[i].position,mouth)<Radius*1.8f)contacts++;
            return contacts>=2;
        }

        private bool EntryClearForGroup(int anchor,int node)
        {
            int group=game.Matter.Groups[anchor];Vector3 mouth=NodeWorld(node),axis=EntryInwardWorld(node);
            float particleRadius=game.Matter.Profile.ParticleRadius;int core=0;
            Vector3 approach=mouth-axis*.025f;
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&!game.Matter.Escaped[i])
            {
                if(!game.Clear(game.Matter.Bodies[i].position,approach,particleRadius))return false;
                Vector3 offset=game.Matter.Bodies[i].position-mouth;float depth=Vector3.Dot(offset,axis);
                float radial=Vector3.ProjectOnPlane(offset,axis).magnitude;
                if(Mathf.Abs(depth)<Radius*1.8f&&radial<Radius-particleRadius*.2f)core++;
            }
            // A clear leading core can deform into the bore from the real
            // gripping annulus. Requiring every spread particle to pre-fit the
            // bore prevents physical intake from ever starting.
            return core>=2;
        }

        private void QueueEntryApproach(int anchor,int node,int edge)
        {
            Vector3 mouth=NodeWorld(node),inward=EntryInwardWorld(node);
            Vector3 approach=ClosestGripPoint(mouth-inward*(Radius+.008f));
            int group=game.Matter.Groups[anchor];Vector3 local=root.InverseTransformPoint(approach);
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group){queuedEntry[i]=node;queuedEdge[i]=edge;queuedApproach[i]=local;}
            game.Motion.Move(anchor,approach,true);
        }

        private Vector3 EntryInwardWorld(int node)
        {
            foreach(int index in adjacency[node])
            {
                Edge edge=Edges[index];Vector3 tangent=edge.A==node?edge.Path[1]-edge.Path[0]:edge.Path[edge.Path.Length-2]-edge.Path[edge.Path.Length-1];
                if(tangent.sqrMagnitude>.000001f)return root.TransformDirection(tangent.normalized);
            }
            Vector3 outward=Nodes[node].LocalOutward.sqrMagnitude>.000001f?Nodes[node].LocalOutward:Vector3.forward;
            return root.TransformDirection(outward.normalized);
        }

        private Vector3 ClosestGripPoint(Vector3 desired)
        {
            VenomSurfacePatch best=null;Vector3 point=desired;float distance=float.PositiveInfinity;
            foreach(VenomSurfacePatch patch in game.Surfaces)
            {
                if(patch==null||!patch.isActiveAndEnabled)continue;
                if(patch.Shape!=null&&patch.Shape.attachedRigidbody!=null&&!patch.Shape.attachedRigidbody.isKinematic)continue;
                Vector3 candidate=patch.Closest(desired);float d=Vector3.Distance(candidate,desired);
                if(d>=distance||(!game.Definition.Passive&&!patch.Grip(candidate)))continue;
                best=patch;point=candidate;distance=d;
            }
            return best!=null?point+best.NormalAt(point)*.019f:desired;
        }

        private void StepQueuedEntries()
        {
            var groups=new HashSet<int>();
            for(int anchor=0;anchor<32;anchor++)
            {
                int group=game.Matter.Groups[anchor];if(!groups.Add(group)||travelByParticle[anchor]!=null)continue;
                int node=queuedEntry[anchor];if(node<0)continue;
                VenomCampaignMotion.Order order=game.Motion.Get(anchor);
                if(order!=null&&Vector3.Distance(order.Target,queuedApproach[anchor])>.006f){ClearQueuedEntry(anchor);continue;}
                if(GroupContactsEntry(anchor,node)&&EntryClearForGroup(anchor,node)&&IsEntryOpen(node))
                {
                    int edge=queuedEdge[anchor];BeginAtNode(anchor,node);
                    if(edge>=0)TryChoose(anchor,edge);
                    continue;
                }
                if(order==null)ClearQueuedEntry(anchor);
            }
        }

        private void ClearQueuedEntry(int anchor)
        {
            if(game==null||anchor<0||anchor>=32)return;int group=game.Matter.Groups[anchor];
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group)queuedEntry[i]=queuedEdge[i]=-1;
        }

        private int PickAdjacent(int node,Ray ray,float limit)
        {
            int best=-1;float score=float.PositiveInfinity;
            foreach(int index in adjacency[node])
            {
                Edge edge=Edges[index];bool forward=edge.A==node;int count=Mathf.Min(edge.Path.Length-1,Mathf.Max(2,edge.Path.Length/3));
                for(int j=1;j<=count;j++)
                {
                    int sample=forward?j:edge.Path.Length-1-j;
                    if(!RayScore(ray,root.TransformPoint(edge.Path[sample]),limit+.035f,out float d)||d>=score)continue;
                    score=d;best=index;
                }
            }
            return best;
        }

        private bool RayScore(Ray ray,Vector3 point,float limit,out float score)
        {
            float depth=Vector3.Dot(point-ray.origin,ray.direction);score=float.PositiveInfinity;
            if(depth<0||depth>limit+.06f)return false;
            score=Vector3.Cross(point-ray.origin,ray.direction).magnitude;
            return score<Radius*1.8f;
        }

        private void ReconcileGroups()
        {
            var reconciled=new HashSet<int>();
            for(int anchor=0;anchor<32;anchor++)
            {
                int group=game.Matter.Groups[anchor];if(!reconciled.Add(group))continue;
                Travel winner=null;
                for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group)
                {
                    Travel candidate=travelByParticle[i];
                    if(candidate!=null&&(winner==null||candidate.CommandId>winner.CommandId))winner=candidate;
                }
                if(winner==null)continue;
                winner.Anchor=anchor;
                for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group)travelByParticle[i]=winner;
            }
        }

        private void PrepareGraph(bool resetFlexible=false)
        {
            HasExit=false;
            for(int i=0;i<adjacency.Length;i++){if(adjacency[i]==null)adjacency[i]=new List<int>();else adjacency[i].Clear();}
            for(int n=0;n<Nodes.Length;n++)HasExit|=Nodes[n].Terminal==TerminalKind.Exit;
            for(int i=0;i<Edges.Length;i++)
            {
                Edge edge=Edges[i];
                if(edge.A<0||edge.B<0||edge.A>=Nodes.Length||edge.B>=Nodes.Length)continue;
                adjacency[edge.A].Add(i);adjacency[edge.B].Add(i);
                if(edge.ControlPoints==null||edge.ControlPoints.Length<2)edge.ControlPoints=new[]{Nodes[edge.A].LocalPosition,Nodes[edge.B].LocalPosition};
                if(edge.Flexible&&(resetFlexible||edge.Path==null))
                {
                    edge.Path=(Vector3[])edge.ControlPoints.Clone();edge.Velocity=new Vector3[edge.Path.Length];
                }
                else if(!edge.Flexible)edge.Path=SampleCurve(edge.ControlPoints,6);
                RecalculateDistance(edge);
            }
        }

        private void StepFlexible(float dt)
        {
            bool rebuild=(++flexibleTick%Mathf.Max(1,FlexibleGeometryInterval))==0;
            Vector3 gravity=root.InverseTransformDirection(Vector3.down);
            foreach(Edge edge in Edges)
            {
                if(!edge.Flexible||edge.Path==null||edge.Path.Length<2)continue;
                int last=edge.Path.Length-1;
                edge.Path[0]=edge.ControlPoints[0];edge.Velocity[0]=Vector3.zero;
                if(last>0){edge.Path[1]=edge.ControlPoints[1];edge.Velocity[1]=Vector3.zero;}
                for(int i=2;i<=last;i++)
                {
                    if(i==last&&!edge.FlexibleTip){edge.Path[i]=edge.ControlPoints[i];edge.Velocity[i]=Vector3.zero;continue;}
                    Vector3 spring=(edge.ControlPoints[i]-edge.Path[i])*3.2f;
                    edge.Velocity[i]=(edge.Velocity[i]+(gravity*1.35f+spring)*dt)*Mathf.Exp(-dt*2.4f);
                    edge.Path[i]+=edge.Velocity[i]*dt;
                }
                for(int pass=0;pass<4;pass++)for(int i=1;i<last;i++)
                {
                    float rest=Vector3.Distance(edge.ControlPoints[i],edge.ControlPoints[i+1]);
                    Vector3 delta=edge.Path[i+1]-edge.Path[i];float length=delta.magnitude;if(length<.00001f)continue;
                    Vector3 correction=delta*(1-rest/length);
                    bool pinA=i==1;bool pinB=i+1==last&&!edge.FlexibleTip;
                    if(pinA)edge.Path[i+1]-=correction;else if(pinB)edge.Path[i]+=correction;else{edge.Path[i]+=correction*.5f;edge.Path[i+1]-=correction*.5f;}
                }
                Nodes[edge.A].LocalPosition=edge.Path[0];Nodes[edge.B].LocalPosition=edge.Path[last];
                RecalculateDistance(edge);
                if(rebuild)RebuildFlexibleGeometry(edge);
            }
            SyncExitDetector();
        }

        private void SyncExitDetector()
        {
            if(game==null||game.Owner==null||game.Owner.Outlet==null)return;
            for(int n=0;n<Nodes.Length;n++)
            {
                if(Nodes[n].Terminal!=TerminalKind.Exit)continue;
                Vector3 outward=Nodes[n].LocalOutward;
                foreach(Edge edge in Edges)if(edge.A==n||edge.B==n)
                {
                    if(edge.Path==null||edge.Path.Length<2)continue;
                    outward=edge.A==n?edge.Path[0]-edge.Path[1]:edge.Path[edge.Path.Length-1]-edge.Path[edge.Path.Length-2];
                    break;
                }
                if(outward.sqrMagnitude<.0001f)outward=Vector3.forward;outward.Normalize();
                game.Owner.Outlet.position=root.TransformPoint(Nodes[n].LocalPosition);
                Vector3 up=Mathf.Abs(Vector3.Dot(outward,Vector3.up))>.9f?Vector3.forward:Vector3.up;
                game.Owner.Outlet.rotation=root.rotation*Quaternion.LookRotation(outward,up);
                return;
            }
        }

        private void RebuildFlexibleGeometry(Edge edge)
        {
            if(edge.Filter==null||edge.Collider==null)return;
            if(edge.RuntimeMesh==null)
            {
                edge.RuntimeMesh=new Mesh{name=edge.Name+" flexible bore"};edge.RuntimeMesh.MarkDynamic();
                edge.Filter.sharedMesh=edge.RuntimeMesh;edge.Collider.sharedMesh=null;edge.Collider.sharedMesh=edge.RuntimeMesh;
            }
            BuildSweptMesh(edge.Path,Radius,Nodes[edge.B].Terminal==TerminalKind.Closed,edge.RuntimeMesh);
            edge.Collider.sharedMesh=null;edge.Collider.sharedMesh=edge.RuntimeMesh;
        }

        private Vector3 NodeWorld(int node)=>root.TransformPoint(Nodes[node].LocalPosition);

        public string DebugState(int anchor)
        {
            if(game==null)return "uninitialized";Travel travel=TravelFor(anchor);
            if(travel==null)return $"outside centre={game.Motion.Centre(anchor):F4} exit={ExitReached} {DebugOutsideState(anchor)}";
            if(travel.Edge<0)
            {
                string blockers="";
                if(travel.PendingEdge>=0)
                {
                    Edge pending=Edges[travel.PendingEdge];bool forward=pending.A==travel.Node;
                    Vector3 tangent=(forward?pending.Path[1]-pending.Path[0]:pending.Path[pending.Path.Length-2]-pending.Path[pending.Path.Length-1]).normalized;
                    Vector3 staging=root.TransformPoint(Nodes[travel.Node].LocalPosition+tangent*(JunctionRadius*.45f));int pendingGroup=game.Matter.Groups[anchor];
                    var blocked=new List<string>();for(int i=0;i<32;i++)if(game.Matter.Groups[i]==pendingGroup&&!game.Matter.Escaped[i]&&SolidSeparates(game.Matter.Bodies[i].position,staging,out Collider hit))blocked.Add($"p{i}{game.Matter.Bodies[i].position:F3}@{hit.name}");
                    blockers=$" pending={travel.PendingEdge} staging={staging:F3} solid=[{string.Join(";",blocked)}]";
                }
                return $"waiting node={travel.Node}{blockers} centre={game.Motion.Centre(anchor):F4}";
            }
            Edge edge=Edges[travel.Edge];float min=float.PositiveInfinity,max=0,radial=0,minBeyond=float.PositiveInfinity,maxBeyond=float.NegativeInfinity,sumBeyond=0;int count=0,behind=0,worst=-1;
            var tail=new List<string>();
            Vector3 endpoint=NodeWorld(travel.Direction>0?edge.B:edge.A);
            Vector3 terminalTangent=root.TransformDirection(travel.Direction>0?
                edge.Path[edge.Path.Length-1]-edge.Path[edge.Path.Length-2]:edge.Path[0]-edge.Path[1]).normalized;
            int group=game.Matter.Groups[anchor];
            for(int i=0;i<32;i++)
            {
                if(game.Matter.Groups[i]!=group||game.Matter.Escaped[i])continue;
                Closest(edge,root.InverseTransformPoint(game.Matter.Bodies[i].position),out Vector3 centre,out _,out float along);
                float remaining=travel.Direction>0?edge.Length-along:along;
                min=Mathf.Min(min,remaining);max=Mathf.Max(max,Vector3.Distance(game.Matter.Bodies[i].position,endpoint));
                float particleRadial=Vector3.Distance(root.InverseTransformPoint(game.Matter.Bodies[i].position),centre);
                if(particleRadial>radial){radial=particleRadial;worst=i;}
                float beyond=Vector3.Dot(game.Matter.Bodies[i].position-endpoint,terminalTangent);
                minBeyond=Mathf.Min(minBeyond,beyond);maxBeyond=Mathf.Max(maxBeyond,beyond);sumBeyond+=beyond;
                if(beyond<game.Matter.Profile.ParticleRadius+.002f)
                {
                    behind++;
                    tail.Add($"p{i}{game.Matter.Bodies[i].position:F3}/v{game.Matter.Bodies[i].linearVelocity:F3}/r{particleRadial:F3}");
                }
                count++;
            }
            float averageBeyond=count>0?sumBeyond/count:0;
            return $"edge={travel.Edge}:{edge.Name} dir={travel.Direction} count={count} minRemain={min:F4} maxEnd={max:F4} maxRadial={radial:F4}@p{worst} plane={minBeyond:F4}..{maxBeyond:F4} avg={averageBeyond:F4} behind={behind}[{string.Join(";",tail)}] centre={game.Motion.Centre(anchor):F4}";
        }

        public string DebugOutsideState(int anchor)
        {
            if(game==null||anchor<0||anchor>=32)return "outside-unavailable";int group=game.Matter.Groups[anchor];
            float min=float.PositiveInfinity,max=0;var particles=new List<string>();
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&!game.Matter.Escaped[i])
            {
                Vector3 local=root.InverseTransformPoint(game.Matter.Bodies[i].position);float radius=local.magnitude;
                min=Mathf.Min(min,radius);max=Mathf.Max(max,radius);
                particles.Add($"p{i}L{local:F3}/W{game.Matter.Bodies[i].position:F3}/v{game.Matter.Bodies[i].linearVelocity:F3}");
            }
            return $"localRadius={min:F3}..{max:F3}[{string.Join(";",particles)}]";
        }

        public string DebugExitRoster()
        {
            if(game==null||game.Owner==null||game.Owner.Outlet==null)return "exit-unavailable";
            var live=new List<string>();
            for(int i=0;i<32;i++)if(!game.Matter.Escaped[i])
            {
                Vector3 local=game.Owner.Outlet.InverseTransformPoint(game.Matter.Bodies[i].position);
                live.Add($"p{i}{local:F3}/v{game.Matter.Bodies[i].linearVelocity:F3}");
            }
            return $"escaped={game.Matter.EscapedCount}/32 lost={game.Owner.Lost} failure={game.Failure} live=[{string.Join(";",live)}]";
        }

        public string DebugEntryState(int anchor,int node)
        {
            if(game==null)return "uninitialized";
            if(anchor<0||anchor>=32||node<0||node>=Nodes.Length)return "invalid entry diagnostic";
            Vector3 mouth=NodeWorld(node),axis=EntryInwardWorld(node),approach=mouth-axis*.025f;
            float particleRadius=game.Matter.Profile.ParticleRadius;int group=game.Matter.Groups[anchor],contacts=0,core=0,rim=0;
            var failures=new List<string>();
            for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&!game.Matter.Escaped[i])
            {
                Vector3 position=game.Matter.Bodies[i].position,offset=position-mouth;
                float depth=Vector3.Dot(offset,axis),radial=Vector3.ProjectOnPlane(offset,axis).magnitude;
                bool contact=Vector3.Distance(position,mouth)<Radius*1.8f;
                bool clear=game.Clear(position,approach,particleRadius);
                bool outsideRim=depth>-particleRadius&&radial>Radius-particleRadius*.35f;
                if(Mathf.Abs(depth)<Radius*1.8f&&radial<Radius-particleRadius*.2f)core++;
                if(outsideRim)rim++;
                if(contact)contacts++;
                if(!clear)failures.Add($"p{i}{position:F3}:sweep-blocked");
            }
            VenomCampaignMotion.Order order=game.Motion.Get(anchor);
            return $"entry={node}:{Nodes[node].Name} open={IsEntryOpen(node)} contacts={contacts} core={core} rim={rim} ready={EntryClearForGroup(anchor,node)} mouth={mouth:F3} approach={approach:F3} order={(order==null?"none":root.TransformPoint(order.Target).ToString("F3"))} failures=[{string.Join(";",failures)}]";
        }

        private static void RecalculateDistance(Edge edge)
        {
            edge.Distance=new float[edge.Path.Length];edge.Length=0;
            edge.LocalBounds=new Bounds(edge.Path[0],Vector3.zero);
            for(int i=1;i<edge.Path.Length;i++)
            {
                edge.Length+=Vector3.Distance(edge.Path[i-1],edge.Path[i]);edge.Distance[i]=edge.Length;
                edge.LocalBounds.Encapsulate(edge.Path[i]);
            }
        }

        private static void Closest(Edge edge,Vector3 point,out Vector3 centre,out Vector3 tangent,out float along)
        {
            centre=edge.Path[0];tangent=(edge.Path[1]-edge.Path[0]).normalized;along=0;float best=float.PositiveInfinity;
            for(int i=0;i<edge.Path.Length-1;i++)
            {
                Vector3 a=edge.Path[i],delta=edge.Path[i+1]-a;float sq=delta.sqrMagnitude;if(sq<.000001f)continue;
                float t=Mathf.Clamp01(Vector3.Dot(point-a,delta)/sq);Vector3 q=a+delta*t;float d=(q-point).sqrMagnitude;
                if(d>=best)continue;best=d;centre=q;tangent=delta.normalized;along=edge.Distance[i]+Mathf.Sqrt(sq)*t;
            }
        }

        public static Vector3[] SampleCurve(Vector3[] controls,int stepsPerSpan=6)
        {
            if(controls==null||controls.Length<2)return controls??Array.Empty<Vector3>();
            if(controls.Length==2)return new[]{controls[0],controls[1]};
            var result=new List<Vector3>((controls.Length-1)*stepsPerSpan+1){controls[0]};
            for(int span=0;span<controls.Length-1;span++)
            {
                Vector3 p0=controls[Mathf.Max(0,span-1)],p1=controls[span],p2=controls[span+1],p3=controls[Mathf.Min(controls.Length-1,span+2)];
                for(int s=1;s<=stepsPerSpan;s++)
                {
                    float t=s/(float)stepsPerSpan,t2=t*t,t3=t2*t;
                    result.Add(.5f*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t2+(-p0+3*p1-3*p2+p3)*t3));
                }
            }
            return result.ToArray();
        }

        public static void BuildSweptMesh(Vector3[] path,float radius,bool capEnd,Mesh mesh,int sides=16)
        {
            var vertices=new List<Vector3>(path.Length*sides+(capEnd?sides+1:0));var triangles=new List<int>((path.Length-1)*sides*6+sides*3);
            Vector3 previousNormal=Vector3.zero;
            for(int i=0;i<path.Length;i++)
            {
                Vector3 tangent=(path[Mathf.Min(path.Length-1,i+1)]-path[Mathf.Max(0,i-1)]).normalized;
                Vector3 normal=i==0?Vector3.Cross(tangent,Mathf.Abs(Vector3.Dot(tangent,Vector3.up))>.85f?Vector3.right:Vector3.up).normalized:Vector3.ProjectOnPlane(previousNormal,tangent).normalized;
                if(normal.sqrMagnitude<.5f)normal=Vector3.Cross(tangent,Vector3.right).normalized;
                previousNormal=normal;Vector3 binormal=Vector3.Cross(tangent,normal).normalized;
                for(int s=0;s<sides;s++){float a=s*Mathf.PI*2/sides;vertices.Add(path[i]+(normal*Mathf.Cos(a)+binormal*Mathf.Sin(a))*radius);}
            }
            for(int i=0;i<path.Length-1;i++)for(int s=0;s<sides;s++)
            {int n=(s+1)%sides,a=i*sides+s,b=i*sides+n,c=(i+1)*sides+s,d=(i+1)*sides+n;triangles.Add(a);triangles.Add(c);triangles.Add(b);triangles.Add(b);triangles.Add(c);triangles.Add(d);}
            if(capEnd)
            {
                int centre=vertices.Count;vertices.Add(path[path.Length-1]);int ring=(path.Length-1)*sides;
                // Face the cap into the bore. MeshCollider triangles are
                // one-sided, so the incoming tissue must meet the front face.
                for(int s=0;s<sides;s++){triangles.Add(centre);triangles.Add(ring+(s+1)%sides);triangles.Add(ring+s);}
            }
            mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        }
    }
}
