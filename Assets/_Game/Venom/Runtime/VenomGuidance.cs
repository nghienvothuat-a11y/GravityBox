using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Surface destinations and observable, local demonstration memory.
    /// Decisions produce tangential intent; adhesion and PhysX move the organism.</summary>
    public sealed class VenomGuidance : MonoBehaviour
    {
        public bool RequiresButton;
        public VenomPressurePlate Button;
        public Rigidbody Door;
        public Vector3 ButtonLocal=new Vector3(.13f,-.25f,-.11f);
        public Vector3 DoorRest=new Vector3(0,.247f,0);
        public VenomGuidanceMemory Memory {get;private set;}
        public int TargetFace {get;private set;}=-1;
        public Vector3 TargetLocal {get;private set;}
        public bool HasTarget=>TargetFace>=0;
        public bool Exiting {get;private set;}
        public bool Replaying {get;private set;}
        public bool DoorUnlocked {get;private set;}
        public bool Arrived {get;private set;}
        public int CommandCount {get;private set;}
        public int ArrivalCount {get;private set;}
        public string Status {get;private set;}="CHẠM MỘT ĐIỂM TRÊN MẶT TRONG HỘP";
        public Vector3 TargetWorld=>level.Rotation.transform.TransformPoint(TargetLocal);
        public float DoorTravel=>Door==null?0:level.Rotation.transform.InverseTransformPoint(Door.position).x-DoorRest.x;
        private VenomLevelController level;
        private Rect? DoorFootprint
        {
            get
            {
                if(Door==null)return null;
                Vector3 centre=level.Rotation.transform.InverseTransformPoint(Door.position);
                return new Rect(centre.x-.087f,centre.z-.087f,.174f,.174f);
            }
        }
        private LineRenderer marker;
        private float hold;
        private int replayIndex;
        private bool taught, exitRemembered;
        private readonly VenomSurfaceRoute route=new VenomSurfaceRoute();

        public void Initialize(VenomLevelController owner)
        {
            level=owner;Memory=VenomGuidanceMemory.Load(level.LevelNumber);
            if(Door!=null)Door.transform.SetParent(owner.Apparatus,true);
            if(Button!=null)Button.transform.SetParent(owner.Apparatus,true);
            marker=new GameObject("Remembered surface destination",typeof(LineRenderer)).GetComponent<LineRenderer>();
            marker.transform.SetParent(owner.Rotation.transform,false);marker.useWorldSpace=false;
            marker.loop=true;marker.positionCount=48;marker.widthMultiplier=.0018f;marker.sharedMaterial=owner.IndicatorMaterial;
            ResetState();
        }
        public void ResetState()
        {
            TargetFace=-1;Exiting=Replaying=DoorUnlocked=Arrived=false;
            hold=0;replayIndex=CommandCount=ArrivalCount=0;taught=exitRemembered=false;
            if(Door!=null)
            {
                Door.position=level.Rotation.transform.TransformPoint(DoorRest);Door.rotation=level.Rotation.transform.rotation;
                Door.linearVelocity=Door.angularVelocity=Vector3.zero;
                Button.ResetPlate(level.Rotation.transform);
            }
            Status=RequiresButton?"CHỈ SINH VẬT TỚI NÚT SÁNG ĐỂ MỞ CỬA":"CHẠM MỘT ĐIỂM TRÊN MẶT TRONG HỘP";
            if(marker!=null)marker.enabled=false;
        }
        public bool PickSurface(Ray worldRay,out int face,out Vector3 local)
        {
            // Select the visible inner face through the near transparent pane.
            // This agrees with the glass rendering and avoids a hidden front-wall target.
            var root=level.Rotation.transform;
            Vector3 origin=root.InverseTransformPoint(worldRay.origin),direction=root.InverseTransformDirection(worldRay.direction);
            face=-1;local=Vector3.zero;float best=float.PositiveInfinity;
            for(int f=0;f<6;f++)
            {
                Vector3 normal=VenomWallClimb.Normals[f];float denominator=Vector3.Dot(direction,normal);
                if(denominator>=-.0001f)continue;
                float distance=Vector3.Dot(-normal*.25f-origin,normal)/denominator;
                if(distance<0 || distance>=best)continue;
                Vector3 point=origin+direction*distance;
                if(Mathf.Max(Mathf.Abs(point.x),Mathf.Abs(point.y),Mathf.Abs(point.z))>.2501f)continue;
                face=f;local=point;best=distance;
            }
            return face>=0;
        }
        public bool Touch(Vector2 screen)
        {
            return PickSurface(level.View.ScreenPointToRay(screen),out int face,out var point) && Command(face,point);
        }
        public bool Command(int face,Vector3 local)
        {
            if(!level.CanControl || face<0 || face>=6 || !Finite(local))return false;
            if(RequiresButton && !DoorUnlocked && face==5 && new Vector2(local.x,local.z).magnitude<.085f)
            {Status="LỖ ĐANG ĐÓNG · CHẠM NÚT SÁNG TRƯỚC";return false;}
            if(face==5 && DoorFootprint.HasValue && DoorFootprint.Value.Contains(new Vector2(local.x,local.z)))
            {Status="ĐIỂM NÀY BỊ CỬA CHE · CHỌN CHỖ KHÁC";return false;}
            Replaying=Exiting=false;CommandCount++;SetTarget(face,local,true);return true;
        }
        private static bool Finite(Vector3 p)=>!float.IsNaN(p.x)&&!float.IsNaN(p.y)&&!float.IsNaN(p.z)&&!float.IsInfinity(p.sqrMagnitude);
        private void SetTarget(int face,Vector3 local,bool remember)
        {
            local=new Vector3(Mathf.Clamp(local.x,-.205f,.205f),Mathf.Clamp(local.y,-.205f,.205f),Mathf.Clamp(local.z,-.205f,.205f));
            var normal=VenomWallClimb.Normals[face];
            TargetLocal=Vector3.ProjectOnPlane(local,normal)-normal*.25f;
            TargetFace=face;Arrived=false;taught=remember;
            Status=Replaying?"ĐANG LÀM LẠI ĐIỀU ĐÃ HỌC":"ĐANG TỚI ĐIỂM ĐƯỢC CHỈ";
        }
        public bool Replay()
        {
            if(!level.CanControl || Memory.Visits.Count==0 && !Memory.KnowsSwitch)return false;
            Replaying=true;Exiting=false;replayIndex=0;
            if(RequiresButton && Memory.KnowsSwitch)
            {if(level.GateLatched)BeginExit();else SetTarget(0,ButtonLocal,false);}
            else SetTarget(Memory.Visits[0].Face,Memory.Visits[0].Point,false);
            return true;
        }
        public void Forget()
        {
            Memory=new VenomGuidanceMemory();Memory.Save(level.LevelNumber);
            Replaying=false;HasStopped();Status="ĐÃ QUÊN · CHẠM ĐỂ HƯỚNG DẪN LẠI";
        }
        private void HasStopped(){TargetFace=-1;Exiting=false;Arrived=false;taught=false;}
        public void StepMechanisms(float dt)
        {
            if(!RequiresButton)return;
            Button.Step(level.Organism,level.Rotation.transform);
            hold=Button.Pressed?hold+dt:0;
            if(!DoorUnlocked && hold>=.25f)
            {
                DoorUnlocked=true;Memory.KnowsSwitch=true;
                Memory.Remember(0,ButtonLocal);Memory.Save(level.LevelNumber);
                Status="ĐÃ HỌC: NÚT MỞ CỬA · ĐỢI CỬA TRƯỢT";
            }
            var root=level.Rotation.transform;Vector3 axis=root.right;
            Vector3 target=root.TransformPoint(DoorRest+Vector3.right*(DoorUnlocked ? .13f : 0));
            float error=Vector3.Dot(target-Door.position,axis);
            float velocity=Vector3.Dot(Door.linearVelocity-level.Rotation.GetComponent<Rigidbody>().GetPointVelocity(Door.position),axis);
            Door.AddForce(Vector3.down*9.81f,ForceMode.Acceleration);
            Door.AddForce(axis*Mathf.Clamp(error*65-velocity*2-Vector3.Dot(Vector3.down*9.81f,axis)*Door.mass,-2,2));
            if(DoorUnlocked && DoorTravel>.105f && !level.GateLatched)
            {level.LatchGuidedGate();BeginExit();}
        }
        private void BeginExit()
        {
            SetTarget(5,new Vector3(0,.25f,0),false);Exiting=true;Replaying=false;
            Status="ĐÃ THẤY LỐI RA · ĐANG TỰ THOÁT";
        }
        public void ObserveCompletion()
        {
            if(!level.Completed || !Exiting || exitRemembered || RequiresButton)return;
            exitRemembered=true;Memory.Remember(5,new Vector3(0,.25f,0));Memory.Save(level.LevelNumber);
        }
        public Vector3 Steer(VenomLocomotion.Fragment fragment)
        {
            var root=level.Rotation.transform;
            Vector3 local=root.InverseTransformPoint(fragment.Centre);
            int face=level.Climbing.FaceFor(fragment.Anchor);
            bool wantsExit=level.GateLatched && TargetFace==5 && new Vector2(TargetLocal.x,TargetLocal.z).magnitude<.105f;
            if(wantsExit && !Exiting && face==5 && Vector3.Distance(local,new Vector3(0,.225f,0))<.105f)BeginExit();
            if(!HasTarget)return Vector3.zero;
            Vector3 normal=VenomWallClimb.Normals[face],delta;
            if(face!=TargetFace)
            {
                int next=VenomSurfaceRoute.NextFace(face,TargetFace,local,TargetLocal);
                Vector3 edge=VenomSurfaceRoute.Edge(face,next,local,TargetLocal);
                if(face==5)edge=route.Detour(local,edge,!Exiting,DoorFootprint);
                delta=Vector3.ProjectOnPlane(edge-local,normal);
                return root.TransformDirection(delta.normalized);
            }
            Vector3 waypoint=face==5?route.Detour(local,TargetLocal,!wantsExit,DoorFootprint):TargetLocal;
            delta=Vector3.ProjectOnPlane(waypoint-local,normal);
            if(!Exiting && Vector3.ProjectOnPlane(TargetLocal-local,normal).magnitude<.018f)
            {
                if(!Arrived)
                {
                    Arrived=true;ArrivalCount++;
                    if(taught){Memory.Remember(TargetFace,TargetLocal);Memory.Save(level.LevelNumber);taught=false;}
                    Status=RequiresButton && !DoorUnlocked && Vector3.Distance(TargetLocal,ButtonLocal)<.055f?"ĐANG BÁM NÚT · CHỜ MỞ CỬA":"ĐÃ TỚI · ĐÃ GHI NHỚ ĐIỂM NÀY";
                    if(Replaying && !(RequiresButton && Memory.KnowsSwitch))
                    {
                        replayIndex++;
                        if(replayIndex<Memory.Visits.Count)SetTarget(Memory.Visits[replayIndex].Face,Memory.Visits[replayIndex].Point,false);
                        else{Replaying=false;Status="ĐÃ LÀM XONG ĐIỀU ĐÃ HỌC";}
                    }
                }
                return Vector3.zero;
            }
            // Brake at the actual destination, not at a temporary obstacle corner.
            bool intermediate=(waypoint-TargetLocal).sqrMagnitude>.0001f;
            return root.TransformDirection(intermediate?delta.normalized:Vector3.ClampMagnitude(delta/.055f,1));
        }
        private void LateUpdate()=>RefreshMarker();
        public void RefreshMarker()
        {
            if(marker==null)return;marker.enabled=HasTarget&&!level.Completed;
            if(!marker.enabled)return;
            Vector3 normal=VenomWallClimb.Normals[TargetFace];
            Vector3 axis=Vector3.Cross(normal,Mathf.Abs(normal.y)>.5f?Vector3.right:Vector3.up).normalized;
            Vector3 other=Vector3.Cross(normal,axis);
            float radius=.014f+(Arrived?0:.002f*Mathf.Sin(level.Organism.SimulationTime*6));
            for(int i=0;i<48;i++)marker.SetPosition(i,TargetLocal+normal*.001f+(axis*Mathf.Cos(i*Mathf.PI/24)+other*Mathf.Sin(i*Mathf.PI/24))*radius);
        }
    }
}
