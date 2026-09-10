using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravityBox.Venom
{
    public enum VenomTask { Wait, Move, HoldA, HoldB, Cut, Exit }

    /// <summary>Five authored lessons using one input grammar. Orders belong to stable
    /// material points; physics/contact observations, never arrival animations, open gates.</summary>
    public sealed class VenomJourney : MonoBehaviour
    {
        public sealed class Order
        {
            public int Anchor, Face;
            public VenomTask Task;
            public Vector3 Target;
            public bool Arrived, Blocked;
            public string Feedback;
        }
        public VenomPressurePlate PadA, PadB;
        public Rigidbody ExitCover, ButtonCover, Knife;
        public Vector3 PadALocal=new Vector3(-.135f,-.25f,.105f),PadBLocal=new Vector3(.135f,-.25f,.105f);
        public Vector3 KnifeLocal=new Vector3(0,-.19f,-.115f);
        public Vector3 ExitCoverRest=new Vector3(0,.247f,0),ButtonCoverRest=new Vector3(.135f,-.241f,.105f);
        public bool AutoAdvance=true;
        public VenomJourneyProgress Progress {get;private set;}
        public int Chapter=>level.LevelNumber;
        public int ExitFace=>Chapter==1?0:5;
        public float CutBias {get;private set;}
        public float HoldProgress {get;private set;}
        public float MassA {get;private set;}
        public float MassB {get;private set;}
        public float RequiredA=>Chapter==5?.024f:.012f;
        public float RequiredB=>Chapter==5?.060f:.012f;
        public bool AActive {get;private set;}
        public bool BActive {get;private set;}
        public bool Solved {get;private set;}
        public bool Cutting=>cutAt>=0;
        public int CommandCount {get;private set;}
        public string Status {get;private set;}
        public string Learning {get;private set;}
        public float ExpressionUntil {get;private set;}
        public string Title=>new[]{"","01   Chào bạn nhỏ","02   Một thế giới sáu mặt","03   Giữ thêm một chút","04   Chờ nhau qua cửa","05   Mỗi phần một nhiệm vụ"}[Chapter];
        public string Hint=>new[]{"","Chạm một điểm để làm quen.\nDẫn bạn nhỏ tới lỗ tròn trên sàn.","Kéo xoay hộp để nhìn mặt khác.\nChỉ đường bò lên lỗ tròn giữa trần.","Chạm nút A. Bạn nhỏ sẽ ở lại giữ nút.\nGiữ đủ vòng sáng, rồi chỉ lỗ thoát.","Chạm dao để chia đôi, rồi chọn từng phần.\nMột phần giữ A, phần kia vào B chốt cửa.","Thử chia đều hoặc lệch ở dao.\nA cần 24 g, B cần 60 g. Giao đúng phần."}[Chapter];
        private VenomLevelController level;
        private readonly Order[] orders=new Order[32];
        private readonly float[] cueUntil=new float[32];
        private readonly Vector3[] cueTarget=new Vector3[32];
        private readonly List<Rect> obstacles=new List<Rect>(8);
        private readonly VenomJourneyRoute floorRoute=new VenomJourneyRoute();
        private readonly VenomSurfaceRoute ceilingRoute=new VenomSurfaceRoute();
        private readonly List<LineRenderer> markers=new List<LineRenderer>();
        private float cutAt=-1,splitUntil,finishedAt=-1,stationLessonHold;
        private int cutAnchor=-1;
        private bool cutPerformed,recorded;

        public void Initialize(VenomLevelController owner)
        {
            level=owner;Progress=VenomJourneyProgress.Load();
            foreach(var body in new[]{ExitCover,ButtonCover,Knife,PadA?.Body,PadB?.Body})if(body!=null)body.transform.SetParent(level.Apparatus,true);
            for(int i=0;i<4;i++)
            {
                var line=new GameObject("Assigned task "+i,typeof(LineRenderer)).GetComponent<LineRenderer>();
                line.transform.SetParent(owner.Rotation.transform,false);line.useWorldSpace=false;line.loop=true;
                line.positionCount=40;line.widthMultiplier=.0016f;line.sharedMaterial=owner.IndicatorMaterial;markers.Add(line);
            }
        }
        public void ResetState()
        {
            System.Array.Clear(orders,0,32);System.Array.Clear(cueUntil,0,32);
            cutAt=finishedAt=-1;splitUntil=0;cutAnchor=-1;cutPerformed=recorded=false;
            Solved=Chapter<=2;AActive=BActive=false;HoldProgress=MassA=MassB=stationLessonHold=0;CommandCount=0;
            Status="CHẠM ĐỂ HƯỚNG DẪN BẠN NHỎ";Learning=Progress.Familiarity>0?"Bạn nhỏ nhận ra người bạn quen.":"Một người bạn mới đang làm quen với bạn.";
            PadA?.ResetPlate(level.Rotation.transform);PadB?.ResetPlate(level.Rotation.transform);
            ResetBody(ExitCover,ExitCoverRest);ResetBody(ButtonCover,ButtonCoverRest);ResetBody(Knife,KnifeLocal+Vector3.up*.09f);
            foreach(var line in markers)line.enabled=false;
        }
        private void ResetBody(Rigidbody body,Vector3 local)
        {
            if(body==null)return;body.position=World(local);body.rotation=level.Rotation.transform.rotation;
            if(!body.isKinematic)body.linearVelocity=body.angularVelocity=Vector3.zero;
        }
        private Vector3 World(Vector3 p)=>level.Rotation.transform.TransformPoint(p);
        private Vector3 Local(Vector3 p)=>level.Rotation.transform.InverseTransformPoint(p);
        public Order TaskFor(int particle)
        {
            int group=level.Organism.Groups[particle];
            // The owning material point may cross the outlet before the tail.
            // Keep the order for its connected tissue until that tissue also exits.
            foreach(var order in orders)if(order!=null&&level.Organism.Groups[order.Anchor]==group)return order;
            return null;
        }
        public bool Select(int anchor)
        {
            if(!level.Locomotion.Select(anchor))return false;
            Cue(anchor,level.View.transform.position,.7f);Status="ĐÃ CHỌN · CHẠM NƠI MUỐN GIAO VIỆC";return true;
        }
        public bool Touch(Vector2 screen)
        {
            if(!level.CanControl)return false;
            // Fragment selection wins only near its actual projected body, not a large invisible hit disk.
            float best=18*VenomCameraFraming.UiScale(level,Screen.width,Screen.height);int selected=-1;
            foreach(var f in level.Locomotion.Fragments)
            {
                var p=level.View.WorldToScreenPoint(f.Centre);float distance=Vector2.Distance(screen,p);
                if(p.z>0&&distance<best){best=distance;selected=f.Anchor;}
            }
            if(selected>=0&&level.Organism.FragmentCount>1)return Select(selected);
            Ray ray=level.View.ScreenPointToRay(screen);float nearest=float.PositiveInfinity;int port=-1;
            var bodies=new[]{Knife,PadA?.Body,PadB?.Body,ExitCover,ButtonCover};
            for(int i=0;i<bodies.Length;i++)if(bodies[i]!=null)
                foreach(var shape in bodies[i].GetComponentsInChildren<Collider>())
                    if(shape.enabled&&shape.Raycast(ray,out var hit,nearest)){nearest=hit.distance;port=i;}
            // Touching the raised blade or cover uses its real projected geometry;
            // projecting that tap all the way to the floor can select a distant point.
            if(port==0)return RequestCut(CutBias);
            if(port==1)return Command(0,PadALocal);
            if(port==2||port==4)return Command(0,PadBLocal);
            if(port==3)return Command(ExitFace,Local(level.Outlet.position));
            if(!level.Guidance.PickSurface(ray,out int face,out var point))return false;
            return Command(face,point);
        }
        public bool Command(int face,Vector3 point)
        {
            if(!level.CanControl||level.Locomotion.Selected==null||face<0||face>5||float.IsNaN(point.sqrMagnitude)||float.IsInfinity(point.sqrMagnitude))return false;
            var chosen=level.Locomotion.Selected;var task=VenomTask.Move;
            if(face==0)
            {
                if(PadA!=null&&XZ(point-PadALocal)<.071f){task=VenomTask.HoldA;point=PadALocal;}
                else if(PadB!=null&&XZ(point-PadBLocal)<.071f)
                {
                    if(!AActive&&!Solved){Reject(chosen.Anchor,"B CÒN BỊ CHE · GIỮ A",PadBLocal);Status="B CÒN BỊ CHE · GIAO MỘT PHẦN GIỮ A";return false;}
                    task=VenomTask.HoldB;point=PadBLocal;
                }
                else if(Knife!=null&&XZ(point-KnifeLocal)<.077f)return RequestCut(CutBias);
            }
            Vector3 exit=Local(level.Outlet.position);
            if(face==ExitFace&&Vector3.ProjectOnPlane(point-exit,VenomWallClimb.Normals[face]).magnitude<.085f)
            {
                if(!level.GateLatched){Reject(chosen.Anchor,"LỖ CHƯA MỞ · BẠN NHỎ CHỜ HƯỚNG DẪN",exit);return false;}
                task=VenomTask.Exit;point=exit;
            }
            if(Cutting){Status="DAO ĐANG HẠ · CHỜ HAI PHẦN TÁCH RA";return false;}
            Assign(chosen.Anchor,task,face,point);CommandCount++;
            Status=task==VenomTask.HoldA||task==VenomTask.HoldB?(Progress.Knows(VenomSkill.Hold)?"NHỚ RỒI · MÌNH SẼ Ở LẠI GIỮ NÚT":"ĐẾN THỬ NÚT · BẠN NHỎ ĐANG QUAN SÁT"):"ĐÃ HIỂU · ĐANG TỚI ĐIỂM BẠN CHỈ";
            Cue(chosen.Anchor,World(point),.75f);return true;
        }
        public void SetCutBias(float bias){if(!Cutting)CutBias=Chapter==5?Mathf.Clamp(bias,0,.018f):0;}
        public bool RequestCut(float bias)
        {
            if(!level.CanControl||Knife==null||Cutting)return false;
            var f=level.Locomotion.Selected;
            if(f==null||f.Count<30||level.Organism.FragmentCount!=1)
            {Status="GỌI HAI PHẦN VỀ HỢP THỂ TRƯỚC KHI CẮT LẠI";return false;}
            SetCutBias(bias);Assign(f.Anchor,VenomTask.Cut,0,new Vector3(CutBias,-.25f,KnifeLocal.z));CommandCount++;
            Status=CutBias>0?"ĐƯA THÂN LỆCH LƯỠI DAO ĐỂ CHIA KHÔNG ĐỀU":"ĐƯA GIỮA THÂN TỚI DAO ĐỂ CHIA ĐÔI";
            Cue(f.Anchor,World(KnifeLocal),.8f);return true;
        }
        public void CallTogether()
        {
            if(!level.CanControl||Cutting)return;
            // A neutral meeting point keeps the reunion away from the sharp blade and station covers.
            Vector3 meeting=new Vector3(-.13f,-.25f,-.14f);
            foreach(var f in level.Locomotion.Fragments)Assign(f.Anchor,VenomTask.Move,0,meeting);
            Status="GỌI VỀ · CÁC PHẦN RỜI NHIỆM VỤ ĐỂ HỢP THỂ";
        }
        public void GuideAllOut()
        {
            if(!level.CanControl||!level.GateLatched)return;
            foreach(var f in level.Locomotion.Fragments)Assign(f.Anchor,VenomTask.Exit,ExitFace,Local(level.Outlet.position));
            Status="CÙNG RA NGOÀI · BẠN NHỎ TỰ CĂN QUA LỖ";
        }
        private void Assign(int anchor,VenomTask task,int face,Vector3 target)
        {
            int group=level.Organism.Groups[anchor];
            for(int i=0;i<32;i++)if(orders[i]!=null&&level.Organism.Groups[orders[i].Anchor]==group)orders[i]=null;
            Vector3 normal=VenomWallClimb.Normals[face];
            target=new Vector3(Mathf.Clamp(target.x,-.215f,.215f),Mathf.Clamp(target.y,-.215f,.215f),Mathf.Clamp(target.z,-.215f,.215f));
            target=Vector3.ProjectOnPlane(target,normal)-normal*.25f;
            orders[anchor]=new Order{Anchor=anchor,Task=task,Face=face,Target=target,Feedback="ĐANG TỚI"};
        }
        private void Reject(int anchor,string message,Vector3 local){Status=message;Cue(anchor,World(local),1.1f);}
        private void Cue(int anchor,Vector3 world,float seconds){cueTarget[anchor]=world;cueUntil[anchor]=level.Organism.SimulationTime+seconds;ExpressionUntil=cueUntil[anchor];}
        public float Attention(int particle,out Vector3 target)
        {
            target=Vector3.zero;float remaining=0;int group=level.Organism.Groups[particle];
            for(int i=0;i<32;i++)if(level.Organism.Groups[i]==group&&cueUntil[i]-level.Organism.SimulationTime>remaining)
            {remaining=cueUntil[i]-level.Organism.SimulationTime;target=cueTarget[i];}
            return Mathf.Clamp01(remaining*3)*(.8f+Progress.Familiarity*.04f);
        }
        public bool Busy(int particle)
        {
            var task=TaskFor(particle);return task!=null&&(task.Task==VenomTask.HoldA||task.Task==VenomTask.HoldB||task.Task==VenomTask.Cut);
        }
        public bool CanFuse(int a,int b)
        {
            if(level.Organism.Groups[a]==level.Organism.Groups[b])return true;
            if(level.Organism.SimulationTime<splitUntil||Cutting||Busy(a)||Busy(b))return false;
            var x=TaskFor(a);var y=TaskFor(b);
            return x==null&&y==null || x!=null&&y!=null&&x.Face==y.Face&&Vector3.Distance(x.Target,y.Target)<.065f;
        }
        public void Step(float dt)
        {
            PadA?.Step(level.Organism,level.Rotation.transform);PadB?.Step(level.Organism,level.Rotation.transform);
            MassA=StationMass(PadA,PadALocal);MassB=StationMass(PadB,PadBLocal);
            AActive=MassA+.00001f>=RequiredA;BActive=MassB+.00001f>=RequiredB;
            stationLessonHold=AActive||BActive?stationLessonHold+dt:0;
            if(stationLessonHold>=.65f)Learn(VenomSkill.Hold);
            bool valid=Chapter==3?AActive:Chapter>=4&&AActive&&BActive&&PadA.Group!=PadB.Group&&level.Organism.CutCount>0;
            if(!Solved)
            {
                HoldProgress=valid?HoldProgress+dt:Mathf.Max(0,HoldProgress-dt*3);
                if(HoldProgress>=(Chapter==3?1.8f:.65f))
                {
                    Solved=true;Learn(Chapter==3?VenomSkill.Hold:Chapter==4?VenomSkill.Cooperate:VenomSkill.Allocate);
                    Status="ĐÃ CHỐT CỬA · CHỈ LỖ HOẶC GỌI TẤT CẢ RA NGOÀI";
                    if(level.Locomotion.Selected!=null)Cue(level.Locomotion.Selected.Anchor,level.Outlet.position,1.5f);
                }
            }
            MoveCover(ExitCover,ExitCoverRest,Vector3.right,Solved?.13f:0,dt);
            MoveCover(ButtonCover,ButtonCoverRest,Vector3.up,AActive||Solved||MassB>.001f?.14f:0,dt);
            if(Solved&&(ExitCover==null||Local(ExitCover.position).x-ExitCoverRest.x>.105f))level.LatchGuidedGate();
            StepKnife(dt);
            if(level.Climbing.VisitedCount>=2)Learn(VenomSkill.Climb);
            if(level.Completed&&!recorded){recorded=true;Progress.Learn(VenomSkill.Arrive);Progress.Finish(Chapter);finishedAt=level.Organism.SimulationTime;Learning="Chúng mình làm được rồi!";}
        }
        private float StationMass(VenomPressurePlate pad,Vector3 target)
        {
            if(pad==null||pad.Load<.006f||pad.Group<0)return 0;
            // Contact validates occupancy. Count only connected tissue physically over
            // this station, not the mass of a distant fragment with a single touching tip.
            float mass=0;
            for(int i=0;i<32;i++)if(!level.Organism.Escaped[i]&&level.Organism.Groups[i]==pad.Group)
            {
                Vector3 p=Local(level.Organism.Bodies[i].position)-target;
                if(Mathf.Abs(p.x)<.066f&&Mathf.Abs(p.z)<.066f&&p.y>-.008f&&p.y<.075f)mass+=level.MatterProfile.ParticleMass;
            }
            return mass;
        }
        private void MoveCover(Rigidbody body,Vector3 rest,Vector3 axis,float travel,float dt)
        {
            if(body==null)return;
            Vector3 p=Local(body.position);float current=Vector3.Dot(p-rest,axis);
            float next=Mathf.MoveTowards(current,travel,dt*.11f);
            body.MovePosition(World(rest+axis*next));body.MoveRotation(level.Rotation.transform.rotation);
        }
        private void StepKnife(float dt)
        {
            if(Knife==null)return;
            if(cutAt<0)
            {
                ResetMovingKnife(KnifeLocal+Vector3.up*.09f);
                foreach(var f in level.Locomotion.Fragments)
                {
                    var order=TaskFor(f.Anchor);
                    if(order!=null&&order.Task==VenomTask.Cut&&XZ(Local(f.Centre)-order.Target)<.009f&&Vector3.ProjectOnPlane(f.Velocity-level.Rotation.GetComponent<Rigidbody>().GetPointVelocity(f.Centre),level.Rotation.transform.up).magnitude<.055f)
                    {
                        // Centre a material cross-section, not the visual blob's COM.
                        // The body keeps its real shape after a reunion, so a fixed
                        // offset alone does not reliably produce a different split.
                        var xs=new List<float>(32);
                        for(int i=0;i<32;i++)if(!level.Organism.Escaped[i]&&level.Organism.Groups[i]==f.Group)xs.Add(Local(level.Organism.Bodies[i].position).x);
                        xs.Sort();int division=CutBias>0?10:xs.Count/2;
                        float plane=(xs[division-1]+xs[division])*.5f;
                        if(Mathf.Abs(plane)>.0025f)
                        {order.Target.x=Mathf.Clamp(Local(f.Centre).x-plane,-.045f,.045f);order.Arrived=false;continue;}
                        cutAt=level.Organism.SimulationTime;cutAnchor=f.Anchor;cutPerformed=false;Status="BÁM YÊN · DAO ĐANG CHIA CƠ THỂ";break;
                    }
                }
                return;
            }
            float age=level.Organism.SimulationTime-cutAt;
            float y=Mathf.Lerp(.09f,-.014f,Mathf.Clamp01(age/.55f));
            ResetMovingKnife(KnifeLocal+Vector3.up*y);
            if(age>.28f&&!cutPerformed)
            {
                int before=level.Organism.FragmentCount;
                level.Organism.Cut(Knife.transform,new Vector3(.002f,.046f,.065f));
                if(level.Organism.FragmentCount>before)
                {
                    cutPerformed=true;splitUntil=level.Organism.SimulationTime+2.8f;
                    System.Array.Clear(orders,0,32);
                    var seen=new HashSet<int>();
                    for(int i=0;i<32;i++)if(!level.Organism.Escaped[i]&&seen.Add(level.Organism.Groups[i]))
                    {
                        float mean=0;int n=0;
                        for(int j=0;j<32;j++)if(level.Organism.Groups[j]==level.Organism.Groups[i]){mean+=Local(level.Organism.Bodies[j].position).x;n++;}
                        Assign(i,VenomTask.Move,0,new Vector3(mean/n<0?-.09f:.09f,-.25f,KnifeLocal.z));
                    }
                    Learn(VenomSkill.Divide);Status="HAI PHẦN · CHỌN MỘT PHẦN RỒI GIAO VIỆC";
                }
            }
            if(age>1.1f){cutAt=-1;if(!cutPerformed){Assign(cutAnchor,VenomTask.Move,0,new Vector3(-.10f,-.25f,-.14f));Status="CHƯA CẮT ĐƯỢC · ĐƯA THÂN TỚI DAO LẠI";}}
        }
        private void ResetMovingKnife(Vector3 p){Knife.MovePosition(World(p));Knife.MoveRotation(level.Rotation.transform.rotation);}
        private void Learn(VenomSkill skill)
        {
            if(!Progress.Learn(skill))return;
            Learning=skill==VenomSkill.Arrive?"Đã học: đi tới nơi bạn chỉ.":skill==VenomSkill.Climb?"Đã học: bò qua mép sang mặt khác.":skill==VenomSkill.Hold?"Đã học: ở lại giữ nút.":skill==VenomSkill.Divide?"Đã học: chia cơ thể và chờ nhau.":skill==VenomSkill.Cooperate?"Đã học: một phần giữ, một phần mở đường.":"Đã học: mỗi phần có sức nặng khác nhau.";
        }
        public Vector3 Steer(VenomLocomotion.Fragment fragment)
        {
            var order=TaskFor(fragment.Anchor);if(order==null)return Vector3.zero;
            if(Cutting&&!cutPerformed)return Vector3.zero;
            int face=level.Climbing.FaceFor(fragment.Anchor);Vector3 local=Local(fragment.Centre),normal=VenomWallClimb.Normals[face];
            bool exit=order.Task==VenomTask.Exit;
            Vector3 target=order.Target;
            if(face!=order.Face)target=VenomSurfaceRoute.Edge(face,VenomSurfaceRoute.NextFace(face,order.Face,local,target),local,target);
            Vector3 waypoint=target;
            if(face==5)
            {
                Rect? cover=ExitCover==null?null:(Rect?)RectAt(Local(ExitCover.position),.085f);
                waypoint=ceilingRoute.Detour(local,target,!exit,cover);
            }
            if(face==0&&Chapter==1)waypoint=ceilingRoute.Detour(local,target,!exit,null);
            if(face==0&&Chapter>=3)
            {
                obstacles.Clear();
                if(Knife!=null&&order.Task!=VenomTask.Cut&&level.Organism.SimulationTime>=splitUntil)obstacles.Add(new Rect(-.043f,KnifeLocal.z-.067f,.086f,.134f));
                if(PadA!=null&&order.Task!=VenomTask.HoldA)obstacles.Add(RectAt(PadALocal,.067f));
                if(PadB!=null&&order.Task!=VenomTask.HoldB)obstacles.Add(RectAt(PadBLocal,.067f));
                if(ButtonCover!=null&&Local(ButtonCover.position).y<ButtonCoverRest.y+.10f)obstacles.Add(RectAt(Local(ButtonCover.position),.084f));
                bool wasBlocked=order.Blocked;
                order.Blocked=!floorRoute.Find(local,target,obstacles,out waypoint);
                if(order.Blocked)
                {
                    if(!wasBlocked)Cue(fragment.Anchor,level.View.transform.position,1.2f);
                    order.Feedback="CHỜ LỐI MỞ";return Vector3.zero;
                }
            }
            bool arrived=face==order.Face&&Vector3.ProjectOnPlane(order.Target-local,normal).magnitude<(order.Task==VenomTask.Cut?.0015f:.014f);
            if(arrived&&!exit)
            {
                if(!order.Arrived)
                {
                    order.Arrived=true;Learn(VenomSkill.Arrive);Cue(fragment.Anchor,level.View.transform.position,.7f);
                    if(fragment.Selected&&order.Task==VenomTask.Move)Status="ĐÃ TỚI · BẠN NHỎ CHỜ CHỈ DẪN TIẾP";
                }
                if(order.Task==VenomTask.HoldA||order.Task==VenomTask.HoldB)
                {
                    bool a=order.Task==VenomTask.HoldA;float mass=a?MassA:MassB,need=a?RequiredA:RequiredB;
                    order.Feedback=mass+.00001f>=need?"ĐANG GIỮ "+(a?"A":"B"):"CHƯA ĐỦ TẢI";
                    if(fragment.Count*level.MatterProfile.ParticleMass+.00001f<need)Status="PHẦN NÀY QUÁ NHẸ · GỌI VỀ ĐỂ CHIA LỆCH HOẶC ĐỔI PHẦN";
                }
                else order.Feedback=order.Task==VenomTask.Cut?"CHỜ DAO":"ĐÃ TỚI · CHỜ BẠN";
                return Vector3.zero;
            }
            order.Feedback=exit?"TỰ THOÁT":"ĐANG TỚI";
            Vector3 delta=Vector3.ProjectOnPlane(waypoint-local,normal);
            bool intermediate=Vector3.Distance(waypoint,order.Target)>.02f;
            return level.Rotation.transform.TransformDirection(intermediate?delta.normalized:Vector3.ClampMagnitude(delta/.042f,1));
        }
        private static Rect RectAt(Vector3 p,float half)=>new Rect(p.x-half,p.z-half,half*2,half*2);
        private static float XZ(Vector3 p)=>new Vector2(p.x,p.z).magnitude;
        private void LateUpdate()
        {
            if(level==null)return;
            int index=0;
            foreach(var f in level.Locomotion.Fragments)
            {
                var order=TaskFor(f.Anchor);if(order==null||index>=markers.Count)continue;
                var line=markers[index++];line.enabled=!level.Completed;
                Vector3 n=VenomWallClimb.Normals[order.Face],u=Vector3.Cross(n,Mathf.Abs(n.y)>.5f?Vector3.right:Vector3.up).normalized,v=Vector3.Cross(n,u);
                float radius=order.Task==VenomTask.HoldA||order.Task==VenomTask.HoldB?.06f:.016f;
                for(int i=0;i<40;i++)line.SetPosition(i,order.Target+n*.002f+(u*Mathf.Cos(i*Mathf.PI/20)+v*Mathf.Sin(i*Mathf.PI/20))*radius);
            }
            for(;index<markers.Count;index++)markers[index].enabled=false;
            if(AutoAdvance&&finishedAt>=0&&Chapter<5&&level.Organism.SimulationTime-finishedAt>3)Load(Chapter+1);
        }
        public void Load(int chapter)
        {
            if(chapter<1||chapter>5)return;Time.timeScale=1;SceneManager.LoadScene($"VenomJourney{chapter:00}");
        }
    }
}
