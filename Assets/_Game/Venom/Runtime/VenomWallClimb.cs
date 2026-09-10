using System;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Adhesive locomotion on the six real inner faces of experiment 04.
    /// All movement is force-driven in the moving surface's reference frame.</summary>
    public sealed class VenomWallClimb
    {
        public static readonly Vector3[] Normals={Vector3.up,Vector3.right,Vector3.left,Vector3.forward,Vector3.back,Vector3.down};
        private static readonly string[] Names={"SÀN","TƯỜNG TRÁI","TƯỜNG PHẢI","TƯỜNG TRƯỚC","TƯỜNG SAU","TRẦN"};
        private sealed class State
        {
            public int Face;
            public readonly int[] Contacts=new int[6],EdgeContacts=new int[6];
            public bool Gripping,Carrying;
            public Vector3 GripLocal,RawAtTurn,Carried;
        }
        private readonly VenomLevelController level;
        private readonly State[] states=new State[32];
        private readonly RaycastHit[,] hits=new RaycastHit[32,6];
        private readonly bool[,] near=new bool[32,6];
        private readonly Collider[] supports=new Collider[32];
        private readonly Vector3[] points=new Vector3[32],normals=new Vector3[32];
        private readonly float[] times=new float[32];
        private int selectedFace;
        private Vector2 heldScreen;
        private Vector3 heldLocal;
        private bool holdingDirection;
        public Vector3 Normal=>level.Rotation.transform.TransformDirection(Normals[selectedFace]);
        public string SurfaceName=>Names[selectedFace];
        public int FaceFor(int anchor)=>states[anchor]?.Face??0;
        public int VisitedMask {get;private set;}
        public int VisitedCount {get{int n=0;for(int i=0;i<6;i++)if((VisitedMask&(1<<i))!=0)n++;return n;}}
        public VenomWallClimb(VenomLevelController owner){level=owner;Reset();}
        public void Reset(){Array.Clear(states,0,32);Array.Clear(supports,0,32);selectedFace=0;VisitedMask=0;holdingDirection=false;}

        public Vector3 ScreenDirection(Vector2 direction)
        {
            if(direction.sqrMagnitude<.0001f){holdingDirection=false;return Vector3.zero;}
            var root=level.Rotation.transform;
            // Keep a held gesture continuous around an edge. Releasing or
            // steering by more than five degrees remaps against the new face.
            if(holdingDirection && Vector2.Dot(direction.normalized,heldScreen)>.9962f)
                return root.TransformDirection(heldLocal)*Mathf.Clamp01(direction.magnitude);
            var camera=level.View.transform;Vector3 normal=Normal;
            Vector3 ray=camera.right*direction.x+camera.up*direction.y;
            float facing=Vector3.Dot(camera.forward,normal);
            // Invert the surface projection; soften the singularity of an
            // edge-on face, where rotating the box gives a clearer view.
            float denominator=Mathf.Sign(facing==0?1:facing)*Mathf.Max(.18f,Mathf.Abs(facing));
            Vector3 tangent=Vector3.ProjectOnPlane(ray-camera.forward*(Vector3.Dot(ray,normal)/denominator),normal);
            heldScreen=direction.normalized;heldLocal=root.InverseTransformDirection(tangent.normalized);holdingDirection=true;
            return tangent.normalized*Mathf.Clamp01(direction.magnitude);
        }

        public Vector3 Step(VenomLocomotion.Fragment fragment,Vector3 command,float dt)
        {
            var matter=level.Organism;var root=level.Rotation.transform;var config=level.LocomotionProfile;
            var box=level.Rotation.GetComponent<Rigidbody>();
            State state=states[fragment.Anchor]??(states[fragment.Anchor]=new State());
            Vector3 localCommand=root.InverseTransformDirection(command);
            float strength=command.magnitude;
            bool moving=strength>.03f;
            if(!moving || Vector3.Dot(localCommand.normalized,state.RawAtTurn)<.96f)state.Carrying=false;
            Vector3 heading=state.Carrying?state.Carried:Vector3.ProjectOnPlane(localCommand,Normals[state.Face]).normalized;
            int[] contacts=state.Contacts,edges=state.EdgeContacts;Array.Clear(contacts,0,6);Array.Clear(edges,0,6);
            for(int i=0;i<32;i++)
            {
                if(matter.Escaped[i] || matter.Groups[i]!=fragment.Group)continue;
                supports[i]=null;
                for(int face=0;face<6;face++)
                {
                    Collider shape=level.CrawlFaces[face];
                    Vector3 normal=root.TransformDirection(Normals[face]);
                    near[i,face]=shape!=null && shape.enabled && shape.Raycast(new Ray(matter.Bodies[i].position,-normal),out hits[i,face],config.AdhesionReach);
                    if(near[i,face]){contacts[face]++;if(level.SplitVault==null || hits[i,face].distance<.025f)edges[face]++;}
                }
            }
            if(moving && heading.sqrMagnitude>.1f)
            {
                int next=state.Face;float best=-.45f;
                for(int face=0;face<6;face++)
                {
                    if(face==state.Face || edges[face]<3)continue;
                    float approach=Vector3.Dot(heading,Normals[face]);
                    if(approach<best){best=approach;next=face;}
                }
                if(next!=state.Face)
                {
                    heading=Quaternion.FromToRotation(Normals[state.Face],Normals[next])*heading;
                    state.Face=next;state.Carried=heading;state.RawAtTurn=localCommand.normalized;state.Carrying=true;state.Gripping=false;
                }
            }
            if(contacts[state.Face]==0)
            {
                // Reacquire a surface after a real fall; never attract matter
                // across the open volume of the box.
                for(int face=0;face<6;face++)if(contacts[face]>contacts[state.Face]){state.Face=face;state.Gripping=false;}
            }
            if(fragment.Selected)selectedFace=state.Face;
            if(contacts[state.Face]>=Mathf.Min(8,fragment.Count))VisitedMask|=1<<state.Face;
            Vector3 preferred=root.TransformDirection(Normals[state.Face]);
            Vector3 travel=root.TransformDirection(heading)*strength;
            if(level.SplitVault!=null)
            {
                // A cut exposes smaller, yielding lobes; an intact body keeps
                // its cohesive skin and cannot actively thread this narrow door.
                bool canSqueeze=fragment.Count<32 && matter.CutCount>0 && state.Face==5;
                fragment.Squeeze.Step(level,fragment.Group,fragment.Centre,canSqueeze?travel:Vector3.zero,dt);
                for(int i=0;i<32;i++)if(matter.Groups[i]==fragment.Group)matter.SetFlow(i,fragment.Squeeze.Amount);
            }
            int supported=0;
            for(int i=0;i<32;i++)
            {
                if(matter.Escaped[i] || matter.Groups[i]!=fragment.Group)continue;
                int face=near[i,state.Face]?state.Face:-1;float distance=float.PositiveInfinity;
                if(face<0)for(int f=0;f<6;f++)if(near[i,f] && hits[i,f].distance<distance){face=f;distance=hits[i,f].distance;}
                if(face<0)continue;
                RaycastHit hit=hits[i,face];supports[i]=hit.collider;
                points[i]=hit.collider.transform.InverseTransformPoint(hit.point);
                normals[i]=hit.collider.transform.InverseTransformDirection(root.TransformDirection(Normals[face]));
                times[i]=matter.SimulationTime;supported++;
            }
            if(supported==0){state.Gripping=false;return travel;}
            if(moving)state.Gripping=false;
            else if(!state.Gripping){state.GripLocal=root.InverseTransformPoint(fragment.Centre);state.Gripping=true;}
            Vector3 grip=Vector3.ProjectOnPlane(root.TransformPoint(state.GripLocal)-fragment.Centre,preferred);
            float weight=Mathf.Min(fragment.Count/(float)supported,level.SplitVault!=null?4f:2.5f);
            if(level.Journey!=null && moving)
            {
                // Leading contacts lift the trailing tissue around a corner.
                // Counting floor contacts as wall traction underestimates the load
                // carried by a small split body's leading tendrils and stalls it.
                int leading=0;
                for(int i=0;i<32;i++)if(!matter.Escaped[i]&&matter.Groups[i]==fragment.Group&&near[i,state.Face])leading++;
                if(leading>0)weight=Mathf.Min(fragment.Count/(float)leading,2.5f);
            }
            Vector3 gravity=Vector3.down*9.81f;
            for(int i=0;i<32;i++)
            {
                if(matter.Escaped[i] || matter.Groups[i]!=fragment.Group || supports[i]==null)continue;
                Vector3 outlet=level.Outlet.InverseTransformPoint(matter.Bodies[i].position);
                if(outlet.z>-.06f && outlet.z<.025f && new Vector2(outlet.x,outlet.y).magnitude<.055f && level.ExitAssistClear(matter.Bodies[i].position))continue;
                TryGetSupport(i,out var surface,out var point,out var normal);
                Vector3 relative=matter.Bodies[i].linearVelocity-box.GetPointVelocity(matter.Bodies[i].position);
                Vector3 desired;
                if(moving)
                {
                    // Trailing tissue still on the previous face feeds towards
                    // the corner while the front climbs onto the new face.
                    Vector3 direction=Vector3.Dot(normal,preferred)<.5f?-preferred:travel;
                    float speed=fragment.Following?config.FollowSpeed:config.CrawlSpeed;
                    desired=Vector3.ProjectOnPlane(direction,normal).normalized*(speed*strength);
                    if(level.SplitVault!=null && state.Face==5 && fragment.Squeeze.Amount>0)
                        desired=fragment.Squeeze.Velocity(level,matter.Bodies[i].position,travel,speed);
                }
                else desired=Vector3.ClampMagnitude(Vector3.ProjectOnPlane(grip,normal)*5,config.CrawlSpeed);
                float response=level.Journey!=null?32:config.VelocityResponse;
                Vector3 tangent=(desired-Vector3.ProjectOnPlane(relative,normal))*response-Vector3.ProjectOnPlane(gravity,normal);
                if(moving)tangent+=desired.normalized*config.FrictionCompensation;
                tangent=Vector3.ClampMagnitude(tangent,config.ClimbAcceleration);
                float gap=Vector3.Dot(matter.Bodies[i].position-point,normal);
                float adhesion=(matter.Profile.ParticleRadius+.0004f-gap)*900-Vector3.Dot(relative,normal)*35-Mathf.Max(0,Vector3.Dot(gravity,normal));
                adhesion=Mathf.Clamp(adhesion,-config.AdhesionAcceleration,config.AdhesionAcceleration*.3f);
                Vector3 force=(tangent+normal*adhesion)*matter.Bodies[i].mass*weight;
                matter.Bodies[i].AddForce(force);
                if(surface.attachedRigidbody!=null && !surface.attachedRigidbody.isKinematic)surface.attachedRigidbody.AddForceAtPosition(-force,point);
            }
            return travel;
        }

        public bool TryGetSupport(int i,out Collider collider,out Vector3 point,out Vector3 normal)
        {
            collider=supports[i];point=normal=Vector3.zero;
            if(level.Organism.Escaped[i] || collider==null || !collider.enabled || level.Organism.SimulationTime-times[i]>.05f)return false;
            point=collider.transform.TransformPoint(points[i]);normal=collider.transform.TransformDirection(normals[i]).normalized;return true;
        }
    }
}
