using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Read-only lessons in the isolated onboarding pilot. Nothing here issues a gameplay command.</summary>
    [DefaultExecutionOrder(130)]
    public sealed class COgheOnboarding : MonoBehaviour
    {
        public enum LessonKind { Exit, Climb, Tap, Crate, Rail, Bridge, Sequence, Boss }
        public enum CueKind { None, Touch, Destination, Release }
        public LessonKind Lesson;
        public VenomMovableProp Primary, Secondary;
        public COgheRailSlider PrimaryRail, SecondaryRail;
        public COgheTapRail TapA, TapB;
        public COgheSequentialWinch Sequence;
        public COgheDockedBridgeDeck Bridge;
        public Vector3 LocalDestination;
        public string Hint {get;private set;}="";
        public string Stage {get;private set;}="";
        public CueKind Cue {get;private set;}
        public Vector3 CuePoint {get;private set;}
        public bool Showing=>game!=null&&!hidden&&(!completedBefore||replay)&&!game.Definition.Boss;
        public bool CueVisible=>Showing&&Cue!=CueKind.None&&game.Owner.CanControl&&!game.Home;
        private VenomCampaign game;
        private bool hidden,completedBefore,replay,climbed;
        private float stageAt;
        private GUIStyle cueLabel;
        private static readonly Color Ink=new Color(.16f,.32f,.35f);
        private static readonly Color Amber=new Color(.87f,.49f,.12f);
        private static readonly Vector2[] Finger={new Vector2(4,17),new Vector2(-8,1),new Vector2(-6,-3),new Vector2(-2,-2),new Vector2(4,4),
            new Vector2(-4,-14),new Vector2(-1,-17),new Vector2(2,-16),new Vector2(9,-2),new Vector2(16,0),new Vector2(19,8),new Vector2(14,18)};

        private void Awake(){game=GetComponent<VenomCampaign>();}
        public void ResetObservation()
        {
            if(game==null)game=GetComponent<VenomCampaign>();
            completedBefore=game.Progress.Completed.Contains(game.Definition.Id);
            if(completedBefore)replay=false;
            climbed=false;Stage="";Hint="";Cue=CueKind.None;
            Refresh();
        }
        public void Toggle()
        {
            if(Showing)hidden=true;
            else {hidden=false;replay=true;}
            Refresh();
        }
        private void LateUpdate()=>Refresh();
        public void Refresh()
        {
            if(game==null||game.Owner==null||game.Motion==null)return;
            Hint="";Cue=CueKind.None;
            if(!Showing||game.Home||game.Owner.Completed||game.Owner.Lost||game.Owner.Paused)return;
            if(Lesson==LessonKind.Climb)
            {
                // A successful climb, including an alternative direct route, is the evidence.
                climbed|=game.Root.InverseTransformPoint(game.Motion.Centre(game.Motion.Selected)).y>LocalDestination.y-.03f;
                if(!climbed)
                {
                    Set("climb","Chạm mặt kính để COghe leo lên.",CueKind.Touch,game.Root.TransformPoint(LocalDestination));
                    var order=game.Motion.Get(game.Motion.Selected);
                    if(order!=null&&Vector3.Distance(game.Root.TransformPoint(order.Target),CuePoint)<.05f)Cue=CueKind.None;
                    return;
                }
            }
            if(Lesson==LessonKind.Exit||Lesson==LessonKind.Climb){ExitCue();return;}
            if(Lesson==LessonKind.Tap)
            {
                if(TapA.Busy||TapB!=null&&TapB.Busy)
                {Set("tap-moving","COghe đang thao tác. Đợi cơ quan tới chốt.");return;}
                if(game.FinalExitAvailable){ExitCue();return;}
                if(TapB==null)
                {
                    if(TapA.AtEnd)Set("gate-rising","Đã khớp! Cửa đang nâng.");
                    else Set("tap-a","Chạm A một lần. COghe đưa nó tới chốt.",CueKind.Touch,TapA.HandPoint);
                }
                else if(!TapA.AtEnd&&!TapB.AtEnd)Set("tap-a","Chạm A để nhả chốt B.",CueKind.Touch,TapA.HandPoint);
                else if(!TapB.AtEnd)Set("tap-b","B đã được nhả. Chạm B một lần.",CueKind.Touch,TapB.HandPoint);
                else if(TapA.AtEnd)Set("tap-return","Chạm A lần nữa để đưa về.",CueKind.Touch,TapA.HandPoint);
                else Set("gate-rising","Đã nối nguồn. Cửa đang nâng.");
                return;
            }
            if(Lesson==LessonKind.Crate)
            {
                if(game.Root.InverseTransformPoint(Primary.Body.position).x>.15f&&!game.Attached)
                {ExitCue();return;}
                Manipulate(Primary,null,"crate",LocalDestination,"Chạm thùng để COghe bám vào.");return;
            }
            if(Lesson==LessonKind.Sequence)
            {
                if(!Sequence.AccessOpen){Manipulate(Primary,PrimaryRail,"open-a",LocalDestination);return;}
                if(game.Attached&&!Sequence.Complete)
                {
                    // Switching handle is a separate instruction; do not imply B is active while holding A.
                    if(game.HeldProp==Primary)
                    {Set("release-a","A đã mở. Chạm “Buông vật” để rời tay nắm.",CueKind.Release);return;}
                }
                if(!Sequence.Complete){Manipulate(Secondary,SecondaryRail,"operate-b",Vector3.zero);return;}
            }
            else if(PrimaryRail!=null&&!PrimaryRail.AtEnd)
            {Manipulate(Primary,PrimaryRail,"move-rail",LocalDestination);return;}
            if(game.Attached){Set("release","Đã tới chốt. Chạm “Buông vật”.",CueKind.Release);return;}
            if(Lesson==LessonKind.Bridge&&Bridge!=null)
            {
                Vector3 deck=Bridge.DockedTop.transform.position;
                if(game.Root.InverseTransformPoint(game.Motion.Centre(game.Motion.Selected)).x<game.Root.InverseTransformPoint(deck).x-.025f)
                {Set("cross-bridge","Chạm mặt cầu để bò qua.",CueKind.Touch,deck);return;}
            }
            if(game.FinalExitAvailable)ExitCue();
            else Set("gate-rising","Đã khớp! Quan sát cửa đang nâng.");
        }
        private void Manipulate(VenomMovableProp prop,COgheRailSlider rail,string stage,Vector3 localDestination,string invite="Chạm tay nắm để COghe bám vào.")
        {
            if(!game.Attached)
            {
                Set(stage+"-grasp",invite,CueKind.Touch,prop.ManipulationGrip!=null?prop.ManipulationGrip.position:prop.Body.position);
                return;
            }
            Vector3 point=rail!=null?rail.Frame.TransformPoint(rail.Start+rail.Axis*(rail.Travel+.10f)):game.Root.TransformPoint(localDestination);
            Set(stage+"-target","Đã bám. Chạm vùng đích để đẩy / kéo.",CueKind.Destination,point);
            // A commanded move must make progress; grasp jitter and stalled operations keep the cue.
            bool moving=rail!=null
                ? rail.Position-rail.InitialTravel>.006f&&Vector3.Dot(rail.Body.linearVelocity,rail.WorldAxis)>.005f
                : Vector3.ProjectOnPlane(prop.Body.position-game.Root.TransformPoint(prop.InitialPosition),Vector3.up).magnitude>.008f&&prop.Body.linearVelocity.magnitude>.015f;
            if(game.HasPropTarget&&moving)
            {Hint="COghe đang đẩy / kéo tới đích.";Cue=CueKind.None;}
        }
        private void ExitCue()
        {
            if(game.Motion.Get(game.Motion.Selected)?.Exit==true)
            {Set("exit-moving","COghe đang tới lỗ sáng.");return;}
            Set("exit","Chạm lỗ sáng để ra ngoài.",CueKind.Touch,game.Owner.Outlet.position);
        }
        private void Set(string stage,string text,CueKind cue=CueKind.None,Vector3 point=default)
        {
            if(Stage!=stage){Stage=stage;stageAt=Time.unscaledTime;}
            Hint=text;Cue=cue;CuePoint=point;
        }

        // Lightweight screen illustration. Draw-only IMGUI does not consume pointer events.
        // The target centre remains empty so the real handle/aperture stays visible.
        private static void Stroke(Vector2 a,Vector2 b,float width,Color color)
        {
            var matrix=GUI.matrix;var previous=GUI.color;
            // Compose in logical HUD coordinates; RotateAroundPivot mixes the scaled canvas
            // with screen-space pivots and separates the strokes on a resized player window.
            GUI.color=color;GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(a.x,a.y,0),
                Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg),Vector3.one);
            GUI.DrawTexture(new Rect(0,-width*.5f,Vector2.Distance(a,b),width),Texture2D.whiteTexture);
            GUI.matrix=matrix;GUI.color=previous;
        }
        private static void Circle(Vector2 centre,float radius,Color color)
        {
            Vector2 previous=centre+Vector2.right*radius;
            for(int i=1;i<=32;i++)
            {float angle=i*Mathf.PI/16;var next=centre+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;Stroke(previous,next,1.8f,color);previous=next;}
        }
        private void OnGUI()
        {
            if(!CueVisible)return;
            float scale=Mathf.Min(Screen.width/540f,Screen.height/960f),h=Screen.height/scale;
            Vector3 screen=game.Owner.View.WorldToScreenPoint(CuePoint);
            Vector2 p=Cue==CueKind.Release?new Vector2(432,h-140):new Vector2((screen.x-(Screen.width-540*scale)*.5f)/scale,(Screen.height-screen.y)/scale);
            if(Cue!=CueKind.Release&&(screen.z<=0||p.x<20||p.x>520||p.y<210||p.y>h-235))return;
            var old=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-540*scale)*.5f,0,0),Quaternion.identity,Vector3.one*scale);
            // Initial quiet demonstration; after ten seconds without progress it pulses again.
            float age=Time.unscaledTime-stageAt;
            float pulse=age<3||age>10?Mathf.Sin(Time.unscaledTime*3)*2:0;
            Circle(p,18+pulse,Amber);
            if(Cue==CueKind.Destination)
            {
                Stroke(p+new Vector2(-7,5),p+new Vector2(0,-4),2,Ink);
                Stroke(p+new Vector2(0,-4),p+new Vector2(7,5),2,Ink);
            }
            // A small outlined index finger outside the target, independent of device glyph support.
            Vector2 finger=p+new Vector2(26+pulse,25+pulse);
            for(int i=1;i<Finger.Length;i++)Stroke(finger+Finger[i-1],finger+Finger[i],2,Ink);
            if(cueLabel==null)cueLabel=new GUIStyle(GUI.skin.label){fontSize=12,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,normal={textColor=Ink}};
            string label=Cue==CueKind.Destination?"ĐÍCH":Cue==CueKind.Release?"BUÔNG":Stage=="exit"?"LỐI RA":
                Lesson==LessonKind.Tap?"1 CHẠM":Lesson==LessonKind.Crate?"THÙNG":Stage.EndsWith("-grasp",System.StringComparison.Ordinal)?"TAY NẮM":"CHẠM";
            GUI.Label(new Rect(Mathf.Clamp(p.x-55,20,410),p.y+48,110,20),label,cueLabel);
            GUI.matrix=old;
        }
    }
}
