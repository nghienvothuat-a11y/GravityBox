using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;
using Touch=UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace GravityBox.Venom
{
    [DefaultExecutionOrder(-110)]
    public sealed partial class VenomCampaign : MonoBehaviour
    {
        public VenomCampaignDefinition Definition;
        public VenomSurfacePatch[] Surfaces;
        public VenomMovableProp[] Props;
        public VenomTransferTube Tube;
        public Rigidbody Knife, ExitCover, ButtonCover;
        public Transform PadA,PadB;
        public bool AutoAdvance=true;
        public VenomLevelController Owner {get;private set;}
        public CohesiveOrganism Matter=>Owner.Organism;
        public Transform Root=>Owner.Rotation.transform;
        public VenomCampaignMotion Motion {get;private set;}
        public VenomCampaignSave Progress {get;private set;}
        public bool Zoom;
        public VenomCampaignCamera CameraRig {get;private set;}
        public bool InTube {get;private set;}
        public enum BladePhase { Ready, Warning, Falling, Returning, AwaitClear }
        public BladePhase KnifePhase {get;private set;}
        public const float KnifeWarningSeconds=1f;
        private static readonly Vector3 KnifeCutHalfSize=new Vector3(.006f,.065f,.075f);
        public float KnifeWarningProgress=>KnifePhase==BladePhase.Warning?Mathf.Clamp01(cutClock/KnifeWarningSeconds):0;
        public bool Cutting=>KnifePhase==BladePhase.Warning||KnifePhase==BladePhase.Falling;
        public bool GateOpen {get;private set;}
        public string Failure {get;private set;}
        public string Activity {get;private set;}="Idle";
        public bool Attached=>heldProp!=null;
        public bool IsHeldSurface(VenomSurfacePatch p)=>heldProp!=null&&p.Shape.attachedRigidbody==heldProp.Body;
        public bool IsPulling {get;private set;}
        public Vector3 PropContact {get;private set;}
        public float Impact {get;private set;}
        public float CatchPulse {get;private set;}
        public bool Home {get;private set;}
        public const string MergeFailure="bạn phải hợp thể trước khi chui ra";
        private readonly Vector3[] previous=new Vector3[32];
        private readonly bool[] inBore=new bool[32];
        private readonly int[] knifeGroupsBeforeCut=new int[CohesiveOrganism.ParticleCount];
        private VenomMovableProp heldProp,approachProp;
        private VenomMovableProp climbingStep;
        private Vector3 stepTop;
        private bool tubeIntent;
        private Vector3 propTarget,gripLocal,gripNormalLocal,knifeRest,exitRest,buttonRest;
        private float lastPropInput,cutClock=-1,holdTime,advanceAt=-1,lastGraph;
        private bool cutDone,hadContact,hasExited;
        private int tubeAnchor;
        private Vector3 lastVelocity;
        private bool pointerDown,pointerMoved;
        private Vector2 pointerStart,pointerPrevious;
        private int touchFinger=-1;
        public COgheControlFeedback Feedback {get;private set;}
        private GUIStyle title,body,button,small;
        private VenomHabitat habitat;
        private COgheDayLabPresentation dayLab;

        public void Initialize(VenomLevelController owner)
        {
            dayLab=GetComponent<COgheDayLabPresentation>();
            Owner=owner;Progress=VenomCampaignSave.Read();
            owner.InitializeCampaignMatter();
            Motion=new VenomCampaignMotion(this);
            Matter.Fused+=Motion.ReconcileAfterFusion;
            foreach(var p in Props){p.Body.useGravity=false;p.Capture(Root);p.GetComponent<VenomPropContainment>()?.Initialize(Root);}
            if(Knife!=null)knifeRest=Root.InverseTransformPoint(Knife.position);
            if(ExitCover!=null)exitRest=Root.InverseTransformPoint(ExitCover.position);
            if(ButtonCover!=null)buttonRest=Root.InverseTransformPoint(ButtonCover.position);
            Feedback=GetComponent<COgheControlFeedback>();
            if(Feedback==null)Feedback=gameObject.AddComponent<COgheControlFeedback>();
            Feedback.Initialize(this);
            InitializeMechanisms();CameraRig=new VenomCampaignCamera(this);EnhancedTouchSupport.Enable();ResetLevel();
        }
        public void ResetLevel()
        {
            ResetPointerInput();
            if(Owner==null||Matter==null)return;
            habitat?.Leave();Owner.Rotation.ResetState();Owner.ResetCampaignState();
            foreach(var p in Props)p.ResetTo(Root);
            ResetBody(Knife,knifeRest);ResetBody(ExitCover,exitRest);ResetBody(ButtonCover,buttonRest);
            heldProp=approachProp=climbingStep=null;tubeIntent=false;InTube=false;cutClock=-1;holdTime=0;Failure=null;Activity="Idle";
            hasExited=false;advanceAt=-1;GateOpen=PadA==null;Zoom=false;Home=false;CameraRig?.Reset();
            KnifePhase=BladePhase.Ready;cutDone=false;MassA=MassB=0;
            Owner.Rotation.InputEnabled=Definition.CanRotate;
            if(GateOpen)Owner.LatchGuidedGate();
            for(int i=0;i<32;i++){previous[i]=Owner.Outlet.InverseTransformPoint(Matter.Bodies[i].position);inBore[i]=false;}
            ResetMechanisms();Physics.SyncTransforms();Motion.Reset();Feedback.ResetFeedback();
        }
        private void ResetBody(Rigidbody body,Vector3 position)
        {if(body==null)return;body.position=Root.TransformPoint(position);body.rotation=Root.rotation;if(!body.isKinematic)body.linearVelocity=body.angularVelocity=Vector3.zero;}
        public bool CanFuse(int a,int b)
        {
            if(Matter.Groups[a]==Matter.Groups[b])return true;
            // Proximity is the only gameplay condition. The real cutting edge
            // still separates tissue while it physically lies between the parts.
            // Use the same edge volume as Cut (the collision proxy is shorter
            // to avoid ejecting soft tissue during the strike).
            return !MechanismBlocksFusion(a,b)&&
                (Mechanisms.Length==0||Clear(Matter.Bodies[a].position,Matter.Bodies[b].position))&&
                (Knife==null||!Matter.CrossesBlade(Knife.transform,KnifeCutHalfSize,a,b));
        }
        public bool Clear(Vector3 a,Vector3 b,float radius=0)
        {
            var delta=b-a;if(delta.sqrMagnitude<.000001f)return true;
            // Only authored scenery can obstruct a route; other pieces are simulated, not walls.
            foreach(var patch in Surfaces)
            {
                if(!patch.isActiveAndEnabled)continue;
                Vector3 x=patch.transform.InverseTransformPoint(a),y=patch.transform.InverseTransformPoint(b);
                if(patch.SphereRadius<=0&&Mathf.Abs(x.z-y.z)>.00001f)
                {
                    float t=x.z/(x.z-y.z);
                    if(t>=0&&t<=1&&patch.ContainsForNavigation(Vector3.Lerp(x,y,t),radius))return false;
                }
                if(patch.Shape!=null&&patch.Shape.Raycast(new Ray(a,delta.normalized),out var hit,delta.magnitude-.002f))return false;
            }
            foreach(var p in Props)
                foreach(var collider in p.CollisionShapes)
                    if(collider.Raycast(new Ray(a,delta.normalized),out _,delta.magnitude-.002f))return false;
            return true;
        }
        public bool Occupied(Vector3 world)
        {
            foreach(var s in Surfaces)
            {
                if(!s.isActiveAndEnabled)continue;
                Vector3 p=s.transform.InverseTransformPoint(world);
                float distance=s.DistanceInside(world);
                if(distance>-.008f&&distance<.008f&&s.Contains(p))return true;
            }
            return false;
        }
        public void Step(float dt)
        {
            // PhysicsTiming deliberately disables Unity's global gravity. Every
            // free prop needs the same explicit world acceleration as the tissue.
            if(!Home)foreach(var prop in Props)
            {
                prop.GetComponent<VenomPropContainment>()?.ResolveContacts();
                prop.Body.AddForce(Vector3.down*9.81f,ForceMode.Acceleration);
            }
            Matter.Step(dt);
            if(Owner.Completed){Owner.Celebration.Step();return;}
            Impact=Mathf.MoveTowards(Impact,0,dt*3);CatchPulse=Mathf.MoveTowards(CatchPulse,0,dt*2);
            Motion.Step(dt);
            if(!Home){StepProp(dt);StepKnife(dt);StepPads(dt);StepTube(dt);StepMechanisms(dt);}
            int contacts=0;Vector3 velocity=Vector3.zero;
            for(int i=0;i<32;i++){if(Motion.HasGrip(i))contacts++;velocity+=Matter.Bodies[i].linearVelocity/32;}
            if(!hadContact&&contacts>4){Impact=Mathf.Clamp01((lastVelocity-velocity).magnitude/.5f);CatchPulse=1;}
            hadContact=contacts>4;lastVelocity=velocity;
            Activity=InTube?"Chảy qua ống":Motion.TryCatchPoint(Motion.Selected,out _)?"Bám vành ống":KnifePhase==BladePhase.Warning?"Cảnh giác":KnifePhase==BladePhase.Falling?"Phân tách":heldProp!=null?(IsPulling?"Kéo":"Đẩy"):
                (Definition.Passive||contacts<2)&&Motion.Get(Motion.Selected)!=null&&Motion.Intent(Motion.Selected).sqrMagnitude>.001f?"Cố bò / trượt":
                contacts<3?(velocity.magnitude>.06f?"Rơi / trượt":"Trượt"):Motion.Busy(Motion.Selected)?"Giữ":
                Motion.Get(Motion.Selected)!=null?"Bò / leo":"Idle";
            if(!Home){Activity=MechanismActivity()??Activity;EvaluateExit();}else habitat?.Step(dt);
            if(!Owner.Completed&&!Owner.Lost)for(int i=0;i<32;i++)
                if(Matter.Bodies[i].position.magnitude>2.2f){Fail("Sinh vật đã ra ngoài vỏ hộp. Thử lại.");break;}
        }
        private void StepProp(float dt)
        {
            if(climbingStep!=null)
            {
                foreach(var face in climbingStep.GetComponentsInChildren<VenomSurfacePatch>())
                    if(Vector3.Dot(face.Normal,Vector3.up)>.9f)
                    {
                        Vector3 fresh=face.Closest(climbingStep.Body.position+Vector3.up)+face.Normal*.024f;
                        if(Vector3.Distance(stepTop,fresh)>.018f){stepTop=fresh;Motion.Move(Motion.Selected,stepTop);}
                        break;
                    }
                if(Vector3.Distance(Motion.Centre(Motion.Selected),stepTop)<.035f)
                {climbingStep=null;Motion.Move(Motion.Selected,Owner.Outlet.position-Owner.Outlet.forward*.024f,false,true);}
                return;
            }
            if(approachProp!=null&&heldProp==null)
            {
                Vector3 c=Motion.Centre(Motion.Selected);Vector3 p=PropSideContact(approachProp,c);
                if(Vector3.Distance(c,p)<.060f)
                {heldProp=approachProp;approachProp=null;gripLocal=Quaternion.Inverse(heldProp.Body.rotation)*(p-heldProp.Body.position);gripNormalLocal=Quaternion.Inverse(heldProp.Body.rotation)*(heldProp.ManipulationGrip!=null?heldProp.ManipulationGrip.forward:Vector3.ProjectOnPlane(p-heldProp.Body.position,Vector3.up).normalized);PropContact=p;lastPropInput=Matter.SimulationTime;propTarget=heldProp.Body.position;Motion.Cancel(Motion.Selected);}
            }
            if(heldProp==null)return;
            if(Matter.SimulationTime-lastPropInput>=3){ReleaseProp();return;}
            var rb=heldProp.Body;Vector3 c2=Motion.Centre(Motion.Selected);var rail=heldProp.GetComponent<COgheRailSlider>();Vector3 desired=rail!=null?Vector3.Project(propTarget-rb.position,rail.WorldAxis):Vector3.ProjectOnPlane(propTarget-rb.position,Vector3.up);
            Vector3 fromExit=Owner.Outlet.InverseTransformPoint(rb.position);
            if(heldProp.ProvidesStep&&Mathf.Abs(fromExit.x)<.028f&&rb.linearVelocity.magnitude<.035f)
            {
                var step=heldProp;VenomSurfacePatch top=null;
                foreach(var p in step.GetComponentsInChildren<VenomSurfacePatch>())if(Vector3.Dot(p.Normal,Vector3.up)>.95f){top=p;break;}
                // A yawed crate touches the wall with its corner before its
                // centre reaches the threshold of an axis-aligned .18 m box.
                // Use the real top edge and preserve the original 15 mm reach.
                float leadingEdge=float.NegativeInfinity;
                if(top!=null)
                    for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)
                    {
                        Vector3 corner=top.transform.TransformPoint(new Vector3(x*top.Size.x*.5f,y*top.Size.y*.5f,0));
                        leadingEdge=Mathf.Max(leadingEdge,Owner.Outlet.InverseTransformPoint(corner).z);
                    }
                if(top!=null&&leadingEdge>-.015f)
                {ReleaseProp();climbingStep=step;stepTop=top.Closest(rb.position+Vector3.up)+Vector3.up*.024f;Motion.Move(Motion.Selected,stepTop);return;}
            }
            Vector3 velocity=Vector3.ClampMagnitude(desired*2,.085f);
            PropContact=rb.position+rb.rotation*gripLocal;
            Vector3 fromProp=rb.rotation*gripNormalLocal;
            IsPulling=Vector3.Dot(desired,fromProp)>0;
            int count=0;for(int i=0;i<32;i++)if(Matter.Groups[i]==Matter.Groups[Motion.Selected])count++;
            Vector3 relativeVelocity=rb.linearVelocity-Root.GetComponent<Rigidbody>().GetPointVelocity(rb.position);
            Vector3 correction=rail!=null?Vector3.Project(velocity-relativeVelocity,rail.WorldAxis):Vector3.ProjectOnPlane(velocity-rb.linearVelocity,Vector3.up);
            Vector3 force=Vector3.ClampMagnitude(correction*(rail!=null?Mathf.Max(rb.mass,.25f):rb.mass)*36,count*Matter.Profile.ParticleMass*7);
            if(Vector3.Distance(c2,PropContact)>.13f){ReleaseProp();return;}
            // Contact/tether acts equally on the object and material. Feet provide the reaction.
            if(rail!=null)rail.ApplyEffort(force);else rb.AddForceAtPosition(force,PropContact);
            if(rail!=null)Motion.BraceAgainstManipulation(Motion.Selected,force);
            Vector3 bodyTarget=PropContact+fromProp*.045f;
            for(int i=0;i<32;i++)if(Matter.Groups[i]==Matter.Groups[Motion.Selected])
            {
                var b=Matter.Bodies[i];Vector3 drive=Vector3.ClampMagnitude((bodyTarget-c2)*12+velocity,.18f);
                b.AddForce(-force/count+(rail!=null?drive-b.linearVelocity:Vector3.ProjectOnPlane(drive-b.linearVelocity,Vector3.up))*b.mass*36);
            }
            if(Matter.SimulationTime-lastGraph>.6f&&rb.linearVelocity.sqrMagnitude<.0001f){Motion.BuildGraph();lastGraph=Matter.SimulationTime;}
        }
        public void ReleaseProp(){heldProp=approachProp=null;Motion.Cancel(Motion.Selected);Motion.BuildGraph();Activity="Idle";}
        public void SetPropTarget(Vector3 world){if(heldProp==null)return;propTarget=world;if(heldProp.GetComponent<COgheRailSlider>()==null)propTarget.y=heldProp.Body.position.y;lastPropInput=Matter.SimulationTime;}
        public void SelectProp(VenomMovableProp prop)
        {
            climbingStep=null;ReleaseProp();approachProp=prop;
            Vector3 c=Motion.Centre(Motion.Selected),p=PropSideContact(prop,c);
            Vector3 d=prop.ManipulationGrip!=null?prop.ManipulationGrip.forward:Vector3.ProjectOnPlane(c-prop.Body.position,Vector3.up).normalized;
            Motion.Move(Motion.Selected,p+d*.045f);
        }
        private Vector3 PropSideContact(VenomMovableProp prop,Vector3 centre)
        {
            if(prop.ManipulationGrip!=null)return prop.ManipulationGrip.position;
            Vector3 nearest=prop.Body.position;float best=float.PositiveInfinity;
            foreach(var face in prop.GetComponentsInChildren<VenomSurfacePatch>())
            {
                if(Mathf.Abs(Vector3.Dot(face.Normal,Vector3.up))>.5f)continue;
                Vector3 reach=centre;reach.y=prop.Body.worldCenterOfMass.y;
                Vector3 p=face.Closest(reach);float d=(p-centre).sqrMagnitude;
                if(d<best){best=d;nearest=p;}
            }
            return nearest;
        }
        public void RequestCut()
        {
            if(Knife==null)return;
            // A tap on the machine is a movement instruction. Only actual tissue
            // entering its sensor starts the cycle; the player can still move away.
            Motion.Move(Motion.Selected,Root.TransformPoint(new Vector3(knifeRest.x,-.275f,knifeRest.z)),true);
        }
        private bool KnifeZoneOccupied()
        {
            for(int i=0;i<32;i++)
            {
                if(Matter.Escaped[i])continue;
                Vector3 p=Root.InverseTransformPoint(Matter.Bodies[i].position);
                if(Mathf.Abs(p.x-knifeRest.x)<.025f&&Mathf.Abs(p.z-knifeRest.z)<.068f&&p.y<-.235f&&p.y>-.31f)return true;
            }
            return false;
        }
        private void StepKnife(float dt)
        {
            if(Knife==null||Home)return;
            bool occupied=KnifeZoneOccupied();
            if(KnifePhase==BladePhase.Ready&&occupied)
            {KnifePhase=BladePhase.Warning;cutClock=0;cutDone=false;}
            else if(KnifePhase==BladePhase.Warning)
            {
                cutClock+=dt;
                if(cutClock>=KnifeWarningSeconds)
                {KnifePhase=BladePhase.Falling;cutClock=0;}
            }
            if(KnifePhase==BladePhase.Falling)
            {
                Knife.AddForce(Vector3.down*9.81f,ForceMode.Acceleration);
                cutClock+=dt;
                if(!cutDone&&Root.InverseTransformPoint(Knife.position).y<-.195f)
                {
                    // Sever real bonds crossed by the descending metal blade.
                    // No teleport, centring or prescribed fragment mass ratio.
                    int before=Matter.TotalFragmentCount;
                    System.Array.Copy(Matter.Groups,knifeGroupsBeforeCut,knifeGroupsBeforeCut.Length);
                    Matter.Cut(Knife.transform,KnifeCutHalfSize);
                    if(Matter.TotalFragmentCount>before){cutDone=true;NotifyMechanismCut(Knife.transform,knifeGroupsBeforeCut);}
                }
                if(cutClock>=.42f){KnifePhase=BladePhase.Returning;cutClock=0;}
            }
            else
            {
                Vector3 target=Root.TransformPoint(knifeRest);
                Knife.AddForce(Vector3.ClampMagnitude((target-Knife.position)*180-Knife.linearVelocity*22,30),ForceMode.Acceleration);
                if(KnifePhase==BladePhase.Returning&&Vector3.Distance(Knife.position,target)<.003f&&Knife.linearVelocity.magnitude<.025f)
                    KnifePhase=BladePhase.AwaitClear;
                if(KnifePhase==BladePhase.AwaitClear&&!occupied)KnifePhase=BladePhase.Ready;
            }
            Owner.Rotation.InputEnabled=Definition.CanRotate&&!Cutting&&Owner.CanControl;
        }
        private int PadGroup(Transform pad,out float mass)
        {
            mass=0;int group=-1;float largest=0;
            for(int a=0;a<32;a++)
            {
                float m=0;
                for(int i=0;i<32;i++)if(Matter.Groups[i]==Matter.Groups[a])
                {
                    Vector3 p=pad.InverseTransformPoint(Matter.Bodies[i].position);
                    if(Mathf.Abs(p.x)<.044f&&Mathf.Abs(p.y)<.044f&&p.z>=0&&p.z<.032f&&Motion.HasGrip(i))m+=Matter.Profile.ParticleMass;
                }
                if(m>largest){largest=m;group=Matter.Groups[a];}
            }
            mass=largest;return group;
        }
        public float MassA {get;private set;}
        public float MassB {get;private set;}
        private void StepPads(float dt)
        {
            if(PadA==null)return;
            int a=PadGroup(PadA,out float ma),b=PadGroup(PadB,out float mb);MassA=ma;MassB=mb;
            if(!GateOpen)
            {
                holdTime=ma>=.012f&&mb>=.012f&&a!=b&&Matter.CutCount>0?holdTime+dt:0;
                if(holdTime>=.65f){GateOpen=true;Owner.LatchGuidedGate();}
            }
            MoveCover(ButtonCover,buttonRest+(ma>=.012f||GateOpen?Vector3.up*.14f:Vector3.zero),dt);
            MoveCover(ExitCover,exitRest+(GateOpen?Vector3.right*.14f:Vector3.zero),dt);
            SetGlow(PadA,ma>=.012f);SetGlow(PadB,mb>=.012f);
        }
        private void SetGlow(Transform t,bool active)
        {
            var r=t.GetComponent<Renderer>();if(r==null||!r.enabled)return;var block=new MaterialPropertyBlock();
            block.SetColor("_EmissionColor",active?new Color(.3f,1,.65f):new Color(.08f,.1f,.08f));r.SetPropertyBlock(block);
        }
        private void MoveCover(Rigidbody rb,Vector3 local,float dt)
        {if(rb!=null){rb.MovePosition(Vector3.MoveTowards(rb.position,Root.TransformPoint(local),dt*.18f));rb.MoveRotation(Root.rotation);}}
        public bool EnterTube()
        {
            if(InTube)return true;
            if(Tube==null)return false;
            tubeIntent=true;
            int contactCount=0;
            for(int i=0;i<32;i++)if(Matter.Groups[i]==Matter.Groups[Motion.Selected]&&Motion.HasGrip(i)&&
                Motion.Support(i,out var shape,out _,out _)&&shape==Tube.Entrance.Shape&&
                Vector3.Distance(Matter.Bodies[i].position,Tube.transform.position)<.11f)contactCount++;
            if(contactCount<2)return false;
            // Finish braking against the new contact before the gentler tube
            // flow takes over. Keep the queued instruction during the grasp.
            if(Motion.TryCatchPoint(Motion.Selected,out _))
            {
                Vector3 velocity=Vector3.zero;int count=0;
                for(int i=0;i<32;i++)if(Matter.Groups[i]==Matter.Groups[Motion.Selected]){velocity+=Matter.Bodies[i].linearVelocity;count++;}
                if((velocity/Mathf.Max(1,count)).sqrMagnitude>.25f*.25f)return false;
            }
            InTube=true;tubeIntent=false;tubeAnchor=Motion.Selected;Motion.Cancel(tubeAnchor);return true;
        }
        private void StepTube(float dt)
        {
            // Auto-entry still requires real grip contacts and braking in EnterTube;
            // the roof, slippery wall and missed falls cannot start the flow.
            if(!InTube&&(tubeIntent||Tube!=null&&Tube.AutoEnterOnContact))EnterTube();
            if(!InTube||Tube==null)return;
            bool through=true;
            for(int i=0;i<32;i++)if(Matter.Groups[i]==Matter.Groups[tubeAnchor])
            {
                var b=Matter.Bodies[i];Vector3 p=Tube.transform.InverseTransformPoint(b.position);
                if(p.z<Tube.Length+.035f)through=false;
                Vector3 desired=new Vector3(-p.x*9,-p.y*9,.11f);
                if(p.z<-.015f&&new Vector2(p.x,p.y).magnitude>Tube.Radius-.011f)desired.z=.01f;
                if(p.z>Tube.Length+.04f)desired.z=.025f;
                Vector3 v=Tube.transform.TransformDirection(Vector3.ClampMagnitude(desired,.15f));
                b.AddForce(Vector3.up*9.81f+Vector3.ClampMagnitude((v-b.linearVelocity)*24,7),ForceMode.Acceleration);
            }
            if(through){InTube=false;Motion.Cancel(tubeAnchor);Motion.BuildGraph();}
        }
        public void EvaluateExit()
        {
            if(Owner.Lost||Owner.Completed||!FinalExitAvailable)return;
            // Full roster connectivity, never just the number of pieces remaining in the box.
            if(hasExited&&Matter.TotalFragmentCount!=1){Fail(MergeFailure);return;}
            float r=Matter.Profile.ParticleRadius;
            for(int i=0;i<32;i++)
            {
                var b=Matter.Bodies[i];Vector3 p=Owner.Outlet.InverseTransformPoint(b.position);var old=previous[i];previous[i]=p;
                if(Matter.Escaped[i])
                {b.AddForce(Vector3.up*9.81f+Vector3.ClampMagnitude((Owner.Outlet.TransformPoint(Vector3.forward*.12f)-b.position)*18-b.linearVelocity*8,5),ForceMode.Acceleration);continue;}
                float radial=new Vector2(p.x,p.y).magnitude;
                if(ExitAssisting(i))
                {
                    Vector3 desired=Owner.Outlet.TransformDirection(Vector3.ClampMagnitude(new Vector3(-p.x*8,-p.y*8,.14f),.18f));
                    b.AddForce(Vector3.up*9.81f+Vector3.ClampMagnitude((desired-b.linearVelocity)*20,5),ForceMode.Acceleration);
                }
                if(old.z<-.004f&&p.z>=-.004f)
                {var cross=Vector3.Lerp(old,p,(-.004f-old.z)/(p.z-old.z));inBore[i]=new Vector2(cross.x,cross.y).magnitude<Owner.ApertureRadius;}
                if(p.z<-.018f)inBore[i]=false;
                if(p.z>=-.004f&&p.z<=.004f&&radial>Owner.ApertureRadius)inBore[i]=false;
                // A rotating circular rim can expose tissue already straddling
                // its entrance plane. The far plane still requires full clearance.
                bool clearedMouth=p.z>=.004f+r&&p.z<.085f&&radial<Owner.ApertureRadius;
                if(clearedMouth||inBore[i]&&old.z<.004f+r&&p.z>=.004f+r)
                {
                    if(Matter.TotalFragmentCount!=1){Fail(MergeFailure);return;}
                    hasExited=true;Matter.RecordEscape(i);
                }
            }
            if(Matter.EscapedCount==32)
            {Progress.Win(Definition);Owner.SetCampaignOutcome(true);advanceAt=Matter.SimulationTime+VenomCelebration.Duration;}
        }
        public bool ExitAssisting(int particle)
            =>ExitAssisting(particle,FinalExitAvailable);
        internal bool ExitAssisting(int particle,bool exitAvailable)
        {
            // Mouth capture is physical assistance, not permission to win.
            // Small fragments otherwise lose their last ceiling contact over
            // the bore and fall forever before EvaluateExit can reject them.
            // The complete roster must still be fused when tissue crosses out.
            if(Home||!exitAvailable||Matter.Escaped[particle])return false;
            Vector3 p=Owner.Outlet.InverseTransformPoint(Matter.Bodies[particle].position);
            Vector3 capture=Owner.Outlet.TransformPoint(Vector3.back*.022f);
            bool eligible=hasExited&&p.magnitude<.25f&&Clear(Matter.Bodies[particle].position,capture)||
                p.z>-.050f&&p.z<.03f&&new Vector2(p.x,p.y).magnitude<Owner.ApertureRadius+.017f&&Owner.ExitAssistClear(Matter.Bodies[particle].position);
            if(!eligible)return false;
            foreach(var mechanism in transportMechanisms)
                if(mechanism.isActiveAndEnabled&&!mechanism.AllowsExitAssist(particle,capture))return false;
            return true;
        }
        public void Fail(string reason)
        {Failure=reason;Motion.StopAll();heldProp=approachProp=null;InTube=false;Owner.SetCampaignOutcome(false);}
        public void ConstrainSkin(ref Vector3 local,ref Vector3 normal,Transform skin)
        {
            Vector3 world=skin.TransformPoint(local);
            foreach(var mechanism in Mechanisms)
                if(mechanism.isActiveAndEnabled&&mechanism.ConstrainSkin(ref world,out Vector3 worldNormal))
                {local=skin.InverseTransformPoint(world);normal=skin.InverseTransformDirection(worldNormal);return;}
            if(InTube&&Tube!=null)
            {
                Vector3 p=Tube.transform.InverseTransformPoint(world);float radial=new Vector2(p.x,p.y).magnitude;
                if(p.z>0&&p.z<Tube.Length&&radial>Tube.Radius-.0005f&&radial<Tube.Radius+.03f)
                {
                    Vector3 outward=new Vector3(p.x,p.y,0).normalized;float scale=(Tube.Radius-.0005f)/radial;p.x*=scale;p.y*=scale;
                    world=Tube.transform.TransformPoint(p);normal=skin.InverseTransformDirection(Tube.transform.TransformDirection(outward));
                }
            }
            foreach(var patch in Surfaces)
            {
                Vector3 p=patch.transform.InverseTransformPoint(world);
                if(patch.SphereRadius>0)
                {
                    if(p.magnitude>patch.SphereRadius&&p.magnitude<patch.SphereRadius+.026f&&patch.Contains(p))
                    {world=patch.transform.TransformPoint(p.normalized*(patch.SphereRadius-.0006f));normal=skin.InverseTransformDirection(patch.NormalAt(world));}
                    continue;
                }
                if(p.z>=0||p.z<-.026f||!patch.Contains(p))continue;
                p.z=.0006f;world=patch.transform.TransformPoint(p);normal=skin.InverseTransformDirection(patch.Normal);
            }
            local=skin.InverseTransformPoint(world);
        }
        public void Load(int number)
        {if(number<1||number>LevelCount)return;Time.timeScale=1;SceneManager.LoadScene("VenomOrigin"+number.ToString("00"));}
        private bool PlayArea(Vector2 p)
        {return CameraRig.AllowsPointer(p,Screen.width,Screen.height);}
        public void BeginPointer(Vector2 p){if(!Owner.CanControl||!PlayArea(p))return;pointerDown=true;pointerMoved=false;pointerStart=pointerPrevious=p;}
        public void MovePointer(Vector2 p)
        {
            if(!pointerDown)return;
            if(!pointerMoved&&(p-pointerStart).magnitude>10*Screen.width/540f){pointerMoved=true;if(Definition.CanRotate&&!Cutting)Owner.Rotation.BeginDrag();}
            if(pointerMoved&&Definition.CanRotate&&!Cutting)Owner.Rotation.Drag((p-pointerPrevious)/Mathf.Min(Screen.width,Screen.height),Owner.View.transform.up,Owner.View.transform.right);
            pointerPrevious=p;
        }
        public void EndPointer(Vector2 p){MovePointer(p);if(pointerDown&&!pointerMoved)TouchPoint(p);pointerDown=false;Owner.Rotation.EndDrag();}
        private void Update()
        {
            if(Owner==null)return;
            var k=Keyboard.current;
            if(k!=null)
            {
                var ks=new[]{k.digit1Key,k.digit2Key,k.digit3Key,k.digit4Key,k.digit5Key,k.digit6Key,k.digit7Key,k.digit8Key,k.digit9Key,k.digit0Key};
                for(int i=0;i<10;i++)if(ks[i].wasPressedThisFrame){Load(i+1+(k.leftShiftKey.isPressed||k.rightShiftKey.isPressed?10:0));return;}
                if(k.rKey.wasPressedThisFrame)ResetLevel();if(k.pKey.wasPressedThisFrame||k.escapeKey.wasPressedThisFrame)Owner.TogglePause();
                if(k.zKey.wasPressedThisFrame)CameraRig.ToggleFollow();
            }
            if(AutoAdvance&&Owner.Completed&&Matter.SimulationTime>=advanceAt&&!Definition.Boss&&Definition.Order<LevelCount){Load(Definition.Order+1);return;}
            if(!Owner.CanControl){ResetPointerInput();return;}
            if(Touch.activeTouches.Count>0||touchFinger>=0)
            {
                DiscardMouseInputForTouch();
                foreach(var t in Touch.activeTouches)
                {
                    if(touchFinger<0&&t.phase==UnityEngine.InputSystem.TouchPhase.Began){touchFinger=t.finger.index;BeginPointer(t.screenPosition);}
                    if(t.finger.index!=touchFinger)continue;
                    if(t.phase==UnityEngine.InputSystem.TouchPhase.Ended){EndPointer(t.screenPosition);touchFinger=-1;}
                    else if(t.phase==UnityEngine.InputSystem.TouchPhase.Canceled){pointerDown=false;touchFinger=-1;Owner.Rotation.EndDrag();}
                    else MovePointer(t.screenPosition);
                    break;
                }
            }
            else ConsumeMouseInput();
        }
        public void TouchPoint(Vector2 screen)
        {
            if(!Owner.CanControl)return;
            climbingStep=null;tubeIntent=false;
            Ray ray=Owner.View.ScreenPointToRay(screen);
            int chosen=-1;float selection=.042f;
            for(int i=0;i<32;i++)
            {
                Vector3 p=Matter.Bodies[i].position;float d=Vector3.Cross(p-ray.origin,ray.direction).magnitude;
                if(d<selection){chosen=i;selection=d;}
            }
            if(Home&&chosen>=0){habitat?.Greet();return;}
            if(chosen>=0&&Matter.TotalFragmentCount>1){SelectFragment(chosen);return;}
            // The passive sphere still acknowledges the nearest shell point.
            // Its inward-facing collision mesh alone would select the far wall.
            var hits=Physics.RaycastAll(ray,5);System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            float obstruction=5;
            VenomSurfacePatch PickPatch(RaycastHit hit)
            {
                var face=hit.collider.GetComponent<VenomSurfacePatch>();
                var proxy=hit.collider.GetComponent<COgheSurfacePickProxy>();
                return face!=null?face:proxy!=null?proxy.Resolve(hit.triangleIndex):null;
            }
            bool CanPick(VenomSurfacePatch face)
            {
                if(face==null)return true;
                // Interior glass is transparent to an entering ray. Roofs also
                // accept exterior taps; lesson 03 exposes its near front pane.
                return face.Selectable&&(face.SphereRadius>0||face.InterceptExterior||Vector3.Dot(ray.direction,face.Normal)<0);
            }
            foreach(var hit in hits)
            {
                if(hit.collider.GetComponent<VenomContact>()!=null)continue;
                var face=PickPatch(hit);if(!CanPick(face))continue;
                obstruction=hit.distance;break;
            }
            // Assembly handles aim on their visible deck plane. A tall moving
            // front face must not swallow every command toward its socket.
            if(heldProp!=null&&heldProp.ManipulationPlane!=null)
            {
                var plane=heldProp.ManipulationPlane;
                if(new Plane(plane.up,plane.position).Raycast(ray,out float distance))
                {var target=ray.GetPoint(distance);SetPropTarget(target);ShowMarker(target,plane.up,plane);}
                return;
            }
            if(TouchMechanism(ray,obstruction))return;
            if(!Home&&Definition.Passive)
                foreach(var surface in Surfaces)
                    if(surface.Selectable&&surface.PickSphere(ray,out var point)){MoveTo(point,surface);return;}
            bool PortalVisible(Transform p)
            {return new Plane(p.forward,p.position).Raycast(ray,out float distance)&&distance<obstruction+.0015f;}
            if(!Home&&heldProp==null&&PortalVisible(Owner.Outlet)&&TryPortal(ray,Owner.Outlet,Owner.ApertureRadius+.007f,true))return;
            if(!Home&&heldProp==null&&Tube!=null&&PortalVisible(Tube.transform)&&TryPortal(ray,Tube.transform,Tube.Radius+.008f,false))return;
            foreach(var hit in hits)
            {
                if(hit.collider.GetComponent<VenomContact>()!=null)continue;
                var patch=PickPatch(hit);
                // Glass facing away from the inspection side can be seen through;
                // explicitly selectable front panes still intercept hidden destinations (03).
                if(!CanPick(patch))continue;
                var prop=hit.collider.GetComponentInParent<VenomMovableProp>();
                if(heldProp!=null){SetPropTarget(hit.point);ShowMarker(new Vector3(propTarget.x,-.299f,propTarget.z),Vector3.up,Root);return;}
                if(prop!=null&&prop.Manipulable&&(!prop.ManipulationHandleOnly||prop.ManipulationGrip!=null&&hit.collider.transform.IsChildOf(prop.ManipulationGrip))){SelectProp(prop);ShowMarker(hit.point,hit.normal,prop.transform,patch);return;}
                if(Knife!=null&&hit.collider.attachedRigidbody==Knife){RequestCut();ShowMarker(Root.TransformPoint(new Vector3(knifeRest.x,-.299f,knifeRest.z)),Root.up,Root);return;}
                if(PadA!=null&&(hit.collider.transform==PadA||hit.collider.transform==PadB))
                {Motion.Move(Motion.Selected,hit.point+Root.up*.018f,true);ShowMarker(hit.point,Root.up,hit.collider.transform,patch);return;}
                if(patch!=null)
                {
                    var proxy=hit.collider.GetComponent<COgheSurfacePickProxy>();
                    MoveTo(proxy!=null?proxy.CommandPoint(patch,hit.point):hit.point,patch);return;
                }
                return; // An opaque cover intercepts commands to the mechanism behind it.
            }
            // A bore is empty; pick its disk only if no nearer selectable surface occludes it.
            TryPortal(ray,Owner.Outlet,Owner.ApertureRadius+.015f,true);
            if(Tube!=null)TryPortal(ray,Tube.transform,Tube.Radius+.045f,false);
        }
        private bool TryPortal(Ray ray,Transform portal,float radius,bool exit)
        {
            Plane plane=new Plane(portal.forward,portal.position);
            if(!plane.Raycast(ray,out float t))return false;Vector3 p=ray.GetPoint(t);
            if(Vector3.Distance(p,portal.position)>radius)return false;
            if(exit)Motion.Move(Motion.Selected,portal.position-portal.forward*.024f,false,true);
            else if(!EnterTube())Motion.Move(Motion.Selected,portal.position-portal.forward*.025f);
            ShowMarker(portal.position,-portal.forward,portal);return true;
        }
        public void MoveTo(Vector3 world,VenomSurfacePatch surface)
        {
            if(heldProp!=null){SetPropTarget(world);ShowMarker(new Vector3(propTarget.x,-.299f,propTarget.z),Vector3.up,Root);return;}
            if(!Home&&Tube!=null&&Vector3.Distance(world,Tube.transform.position)<.085f&&EnterTube())
            {ShowMarker(Tube.transform.position,-Tube.transform.forward,Tube.transform);return;}
            Vector3 normal=surface.NormalAt(world);
            Motion.Move(Motion.Selected,world+normal*.019f);ShowMarker(world,normal,surface.transform,surface);
        }
        private void ShowMarker(Vector3 p,Vector3 n,Transform follows,VenomSurfacePatch surface=null)
        {Feedback.ShowCommand(p,n,follows,surface);}
        private void LateUpdate()
        {
            if(Owner==null)return;
            if(Owner.Completed){Owner.Celebration.Frame(Screen.width,Screen.height);return;}
            CameraRig.Frame(Screen.width,Screen.height,Time.unscaledDeltaTime,false,Screen.safeArea);
        }
        private void OnGUI()
        {
            if(Owner==null)return;
            if(dayLab!=null&&dayLab.enabled)return;
            float s=Mathf.Min(Screen.width/540f,Screen.height/960f),h=Screen.height/s;
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-540*s)*.5f,0,0),Quaternion.identity,Vector3.one*s);
            if(title==null)
            {
                title=new GUIStyle(GUI.skin.label){fontSize=25,alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold,normal={textColor=new Color(.83f,.96f,.90f)}};
                body=new GUIStyle(title){fontSize=16,fontStyle=FontStyle.Normal,wordWrap=true};small=new GUIStyle(body){fontSize=13};button=new GUIStyle(GUI.skin.button){fontSize=15};
            }
            GUI.Label(new Rect(15,20,510,38),Home?"NHÀ CỦA SINH VẬT":Definition.Title,title);
            for(int i=1;i<=LevelCount;i++)if(GUI.Button(new Rect(20+(i-1)%10*51,67+(i-1)/10*34,47,29),(i%10==0?"B"+i:i.ToString("00")),button))Load(i);
            if(!Definition.Boss&&!Owner.Completed&&!Home)GUI.Label(new Rect(24,143,492,54),Definition.Lesson,small);
            if(CameraRig.ShowZones)
                for(int i=-1;i<CameraRig.ZoneCount;i++)
                {
                    float width=500f/(CameraRig.ZoneCount+1);
                    if(GUI.Button(new Rect(20+(i+1)*width,202,width-5,36),i<0?"Toàn cảnh":Definition.CameraZones[i].Label,button))CameraRig.SelectZone(i);
                }
            if(Owner.Lost)
            {GUI.Box(new Rect(25,h*.40f,490,145),"");GUI.Label(new Rect(43,h*.40f+12,454,80),Failure,body);if(GUI.Button(new Rect(150,h*.40f+94,240,38),"THỬ LẠI",button))ResetLevel();}
            else if(Owner.Completed)
            {
                GUI.Label(new Rect(20,h-218,500,44),"CHÚNG MÌNH LÀM ĐƯỢC RỒI!",title);
                if(Definition.Boss&&Owner.Celebration.ReadyForNext)
                {GUI.Label(new Rect(20,h-174,500,30),"ĐÃ MỞ COLLECTION",body);if(GUI.Button(new Rect(80,h-131,380,45),"NHÀ CỦA SINH VẬT",button))EnterHome();}
            }
            else
            {
                GUI.Label(new Rect(20,h-167,500,30),Owner.Paused?"TẠM DỪNG":Activity,body);
                var groups=new HashSet<int>();int slot=0;
                for(int i=0;i<32;i++)if(groups.Add(Matter.Groups[i]))
                {int anchor=i;if(Matter.TotalFragmentCount>1&&GUI.Button(new Rect(115+slot*160,h-124,150,34),(Matter.Groups[Motion.Selected]==Matter.Groups[i]?"● ":"")+"Phần "+(slot+1),button))SelectFragment(anchor);slot++;}
                if(Attached&&GUI.Button(new Rect(170,h-124,200,34),"BUÔNG HỘP",button))ReleaseProp();
            }
            if(GUI.Button(new Rect(20,h-72,120,36),"THỬ LẠI",button))ResetLevel();
            if(GUI.Button(new Rect(150,h-72,110,36),Owner.Paused?"TIẾP":"DỪNG",button))Owner.TogglePause();
            if(GUI.Button(new Rect(270,h-72,110,36),Zoom?"TOÀN CẢNH":"THEO COghe",button))CameraRig.ToggleFollow();
            if(Progress.HomeUnlocked&&GUI.Button(new Rect(390,h-72,130,36),Home?"CHÀO BẠN":"COLLECTION",button)){if(Home)habitat?.Greet();else EnterHome();}
            if(Home)
            {if(GUI.Button(new Rect(60,h-124,200,36),"CHO ĂN",button))habitat?.Feed();if(GUI.Button(new Rect(280,h-124,200,36),"CHƠI CÙNG",button))habitat?.Greet();}
        }
        public float Greeting=>Home?(habitat?.Greeting??0):0;
        public void FeedHome(){if(Home)habitat?.Feed();}
        public void GreetHome(){if(Home)habitat?.Greet();}
        public void EnterHome()
        {
            if(!Progress.HomeUnlocked)return;
            ResetLevel();Home=true;Progress.RevealHome=false;Progress.Write();Owner.Rotation.InputEnabled=false;
            habitat=habitat??new VenomHabitat(this);habitat.Enter();Zoom=true;
        }
        private void OnDestroy(){habitat?.Dispose();if(Owner!=null&&Owner.Rotation!=null)Owner.Rotation.EndDrag();EnhancedTouchSupport.Disable();}
    }
}
