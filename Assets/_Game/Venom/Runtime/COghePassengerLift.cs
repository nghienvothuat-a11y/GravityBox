using UnityEngine;
namespace GravityBox.Venom
{
    /// <summary>A call panel, boarding check and finite-force motor. Tissue is carried by contact, never positioned.</summary>
    public sealed class COghePassengerLift : COgheMechanism
    {
        /// <summary>The tray button's own state (Mrk, 05/10/2026: a button must read as a button). It sinks when COghe
        /// presses it, stays down while the tray travels and pops back up on arrival, ready to send the tray the other way.</summary>
        public enum ButtonState { Ready, Armed, Pressing, Latched, Releasing }
        public const float PressSeconds=.25f, ReleaseSeconds=.2f;
        public COgheRailSlider Rail;
        public VenomSurfacePatch Deck;
        public Transform Panel;
        public COgheRailSlider RequiredRail;
        public Vector2 DeckSize=new Vector2(.24f,.22f);
        public float DeckHeight=.018f, Speed=.075f, MaximumForce=3;
        // Optional fixed call panels at the landings (0 = lower stop, 1 = upper stop; null = none) and props carried as cargo.
        public Transform[] CallPanels=System.Array.Empty<Transform>();
        public bool CarriesProps;
        // Dead-man enable: while RequiredRail is not at its end the tray may not rise, and a raised tray returns to its
        // lower landing under the same finite, damped motor (nothing is snapped; tissue on the deck rides down).
        public bool ReturnWhenDisabled;
        public Transform BoardPoint;
        public bool Moving {get;private set;}
        public bool Boarding {get;private set;}
        public int Trips {get;private set;}
        public ButtonState Button {get;private set;}
        /// <summary>The call panel that brought the empty tray, held down until the tray arrives; -1 when none.</summary>
        public int CalledPanel {get;private set;}=-1;
        public int Actor=>actor;
        public bool Powered=>RequiredRail==null||RequiredRail.AtEnd;
        /// <summary>+1 when the next trip goes up, -1 when it goes down.</summary>
        public int NextDirection=>Rail.Position>Rail.Travel*.5f?-1:1;
        /// <summary>The button cap COghe presses with a tendril while the press lasts (presentation reads it).</summary>
        public Vector3 PressPoint=>Panel.TransformPoint(Vector3.up);
        private VenomCampaign game;
        private int actor=-1;private float target,stable,buttonClock,callClock,travelIntegral;
        private bool callReleasing;
        private VenomCampaignMotion.Order boardingOrder;
        private Vector3 panelRest;
        private Vector3[] callRest=System.Array.Empty<Vector3>();
        private float panelTravel,callTravel;
        private LineRenderer[] rings=System.Array.Empty<LineRenderer>(),chevrons=System.Array.Empty<LineRenderer>();

        public override string Activity=>Boarding&&Button==ButtonState.Pressing?"Bấm nút thang":Boarding?"Lên khay nâng":Moving?"Thang đang di chuyển":null;
        public override void InitializeMechanism(VenomCampaign owner)
        {
            game=owner;panelRest=Panel.localPosition;
            // Sink by most of the cap that stands proud of the deck, never into it (a unit cylinder is two units tall).
            panelTravel=Mathf.Clamp((panelRest.y+Panel.localScale.y-DeckHeight)*.6f,.003f,.009f);
            callRest=new Vector3[CallPanels.Length];for(int i=0;i<CallPanels.Length;i++)if(CallPanels[i]!=null)callRest[i]=CallPanels[i].localPosition;
            callTravel=.006f;
            BuildIndicators();
        }
        public override void ResetMechanism(VenomCampaign owner)
        {game=owner;Moving=Boarding=false;actor=-1;Trips=0;stable=0;Rail.Locked=true;boardingOrder=null;Button=ButtonState.Ready;CalledPanel=-1;callReleasing=false;buttonClock=callClock=0;}
        public override bool TryTouch(VenomCampaign owner,Ray ray,float obstruction)
        {
            int call=HitCallPanel(ray,obstruction);
            if(call>=0)
            {
                float station=call==0?0:Rail.Travel;
                if(Moving){Refuse(owner,Refusal.Busy,CallPanels[call].position);return true;}
                if(!Powered){Refuse(owner,Refusal.NoPower,CallPanels[call].position);return true;}
                // A call panel brings the empty tray to its landing; at that landing it acts as the tray's own button.
                if(Mathf.Abs(Rail.Position-station)>Rail.CatchTolerance*2)
                {actor=owner.Motion.Selected;BeginTravel(station,false,call);owner.Feedback.ShowCommand(CallPanels[call].position,Vector3.up,CallPanels[call]);return true;}
                return Board(owner,CallPanels[call]);
            }
            if(!HitsPanel(ray,obstruction))return false;
            if(Moving){Refuse(owner,Refusal.Busy,Panel.position);return true;}
            if(!Powered){Refuse(owner,Refusal.NoPower,Panel.position);return true;}
            return Board(owner,Panel);
        }
        /// <summary>A held crate is let go for a tap on a lift button (Mrk: the button near COghe looked dead).</summary>
        public override bool ClaimsTapWhileHolding(Ray ray,float obstruction)=>HitsPanel(ray,obstruction)||HitCallPanel(ray,obstruction)>=0;
        private bool HitsPanel(Ray ray,float obstruction)=>new Bounds(Panel.position,new Vector3(.09f,.065f,.085f)).IntersectRay(ray,out float d)&&d<=obstruction+.006f;
        private int HitCallPanel(Ray ray,float obstruction)
        {
            for(int i=0;i<CallPanels.Length;i++)
                if(CallPanels[i]!=null&&new Bounds(CallPanels[i].position,new Vector3(.07f,.07f,.07f)).IntersectRay(ray,out float c)&&c<=obstruction+.006f)return i;
            return -1;
        }
        private bool Board(VenomCampaign owner,Transform pressed)
        {
            actor=owner.Motion.Selected;
            if(!owner.PrepareTapCommand(actor))return true;
            // From below, the button calls the empty tray down; from the landing beside it, COghe boards and rides.
            bool below=owner.Motion.Centre(actor).y<Deck.transform.position.y-.06f;
            if(NextDirection<0&&!OnDeck(actor)&&CallPanels.Length==0&&below){BeginTravel(0,true,-1);return true;}
            Boarding=true;Button=ButtonState.Armed;buttonClock=0;
            owner.Motion.Move(actor,BoardPoint!=null?BoardPoint.position:Deck.Closest(Rail.Body.position+Vector3.up*DeckHeight)+Vector3.up*.018f);
            boardingOrder=owner.Motion.Get(actor);
            owner.Feedback.ShowCommand(pressed.position,Vector3.up,pressed);return true;
        }
        private bool OnDeck(int index)=>OnDeckPoint(game.Matter.Bodies[index].position,.009f);
        private bool OnDeckPoint(Vector3 world,float margin)
        {
            var p=Rail.Body.transform.InverseTransformPoint(world);
            return Mathf.Abs(p.x)<DeckSize.x*.5f-margin&&Mathf.Abs(p.z)<DeckSize.y*.5f-margin&&p.y>DeckHeight-.008f&&p.y<DeckHeight+.12f;
        }
        private void BeginTravel(float destination,bool carrying,int call)
        {
            target=destination;Moving=true;Boarding=false;stable=0;travelIntegral=0;Rail.Locked=false;Rail.ReleaseLatch();
            if(call>=0){CalledPanel=call;callReleasing=false;}else if(carrying){Button=ButtonState.Latched;buttonClock=0;}
            if(carrying)game.Motion.Cancel(actor);
        }
        public override void StepMechanism(VenomCampaign owner,float dt)
        {
            if(Button==ButtonState.Releasing&&(buttonClock+=dt)>=ReleaseSeconds)Button=ButtonState.Ready;
            if(callReleasing&&(callClock+=dt)>=ReleaseSeconds){callReleasing=false;CalledPanel=-1;}
            if(owner.Owner.Lost||owner.Home){Moving=Boarding=false;Rail.Locked=true;if(Button!=ButtonState.Releasing)Button=ButtonState.Ready;return;}
            if(Boarding)
            {
                var current=owner.Motion.Get(actor);
                if(current!=null&&!ReferenceEquals(current,boardingOrder)){Boarding=false;Button=ButtonState.Ready;return;}
                bool all=true;for(int i=0;i<32;i++)if(owner.Matter.Groups[i]==owner.Matter.Groups[actor]&&!OnDeck(i)){all=false;break;}
                stable=all?stable+dt:0;
                if(stable>.2f&&(!ReturnWhenDisabled||Powered))
                {
                    // Fully aboard: COghe presses the button, then the tray leaves.
                    if(Button!=ButtonState.Pressing){Button=ButtonState.Pressing;buttonClock=0;}
                    else if((buttonClock+=dt)>=PressSeconds)BeginTravel(NextDirection<0?0:Rail.Travel,true,-1);   // the far stop, even if the tray sagged a little under its rider
                }
                else if(Button==ButtonState.Pressing)Button=ButtonState.Armed;
            }
            if(ReturnWhenDisabled&&!Powered&&Rail.Position>Rail.CatchTolerance*2&&!(Moving&&target<=0)){Boarding=false;Button=ButtonState.Ready;BeginTravel(0,false,-1);}
            if(!Moving)return;
            float error=target-Rail.Position,speed=Vector3.Dot(Rail.Body.linearVelocity,Rail.WorldAxis);
            float desired=Mathf.Clamp(error*4,-Speed,Speed);
            float mass=Rail.Body.mass;for(int i=0;i<32;i++)if(OnDeck(i))mass+=owner.Matter.Bodies[i].mass;
            if(CarriesProps)foreach(var prop in owner.Props)if(prop.Body!=Rail.Body&&OnDeckPoint(prop.Body.worldCenterOfMass,0))mass+=prop.Body.mass;
            // The weight feed-forward assumes the rider rests its whole weight on the deck; a rider partly holding itself
            // up left a tray stuck at its top stop (the chapter-1 boss, 05/10/2026). A slow integral of the speed error
            // makes up whatever the estimate misses.
            travelIntegral=Mathf.Clamp(travelIntegral+(desired-speed)*8f*dt,-1,1);
            Rail.ApplyEffort(Rail.WorldAxis*Mathf.Clamp(mass*9.81f+(desired-speed)*1.0f+travelIntegral,-MaximumForce,MaximumForce));
            stable=Mathf.Abs(error)<Rail.CatchTolerance&&Mathf.Abs(speed)<.02f?stable+dt:0;
            if(stable>.15f)
            {
                Moving=false;Rail.Locked=true;Trips++;owner.Motion.BuildGraph(true);
                if(Button==ButtonState.Latched){Button=ButtonState.Releasing;buttonClock=0;}
                if(CalledPanel>=0){callReleasing=true;callClock=0;}
            }
        }

        // ---- presentation: reads the state above, never changes it ---------------------------------------------------
        private static readonly Color Mint=new Color(.10f,.57f,.45f),Amber=new Color(.87f,.49f,.12f),Coral=new Color(.78f,.30f,.24f),Idle=new Color(.55f,.58f,.60f);
        private void BuildIndicators()
        {
            var material=game!=null&&game.Feedback!=null?game.Feedback.MarkerMaterial:null;
            if(material==null)return;
            int count=1+CallPanels.Length;rings=new LineRenderer[count];chevrons=new LineRenderer[count];
            for(int i=0;i<count;i++)
            {
                var button=i==0?Panel:CallPanels[i-1];if(button==null)continue;
                rings[i]=Line(button.name+" · status ring",49,.0022f,material);
                chevrons[i]=Line(button.name+" · direction",3,.0024f,material);
            }
        }
        private LineRenderer Line(string name,int points,float width,Material material)
        {
            var line=new GameObject(name+" · presentation only").AddComponent<LineRenderer>();line.transform.SetParent(transform,false);
            line.sharedMaterial=material;line.useWorldSpace=true;line.positionCount=points;line.startWidth=line.endWidth=width;
            line.numCapVertices=2;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;return line;
        }
        /// <summary>How far the tray button is pressed, 0 (up) to 1 (down), from the simulated button state.</summary>
        public float PanelDepth
        {
            get
            {
                switch(Button)
                {
                    case ButtonState.Armed: return .12f;
                    case ButtonState.Pressing: return Mathf.SmoothStep(.12f,1,Mathf.Clamp01(buttonClock/(PressSeconds*.5f)));
                    case ButtonState.Latched: return 1;
                    case ButtonState.Releasing:
                    {
                        // A damped spring back up, a little past rest.
                        float t=Mathf.Clamp01(buttonClock/ReleaseSeconds);
                        return Mathf.Exp(-5*t)*Mathf.Cos(t*Mathf.PI*1.2f)*(1-t);
                    }
                    default: return 0;
                }
            }
        }
        private float CallDepth(int i)=>CalledPanel!=i?0:callReleasing?1-Mathf.SmoothStep(0,1,Mathf.Clamp01(callClock/ReleaseSeconds)):1;
        private void LateUpdate()
        {
            if(Panel==null)return;
            Panel.localPosition=panelRest+Vector3.down*panelTravel*PanelDepth;
            for(int i=0;i<CallPanels.Length;i++)if(CallPanels[i]!=null)CallPanels[i].localPosition=callRest[i]+Vector3.down*callTravel*CallDepth(i);
            if(game==null||game.Owner==null||game.Owner.View==null||rings.Length==0)return;
            bool live=!game.Home&&!game.Owner.Completed&&!game.Owner.Lost;
            float now=Time.unscaledTime,refused=game.Matter!=null?game.Matter.SimulationTime-RefusedAt:9;
            for(int i=0;i<rings.Length;i++)
            {
                if(rings[i]==null)continue;
                var button=i==0?Panel:CallPanels[i-1];
                bool pressed=i==0?Button==ButtonState.Pressing||Button==ButtonState.Latched:CalledPanel==i-1&&!callReleasing;
                bool arriving=i==0?Button==ButtonState.Releasing:CalledPanel==i-1&&callReleasing;
                Color color=!Powered?Idle:pressed||Moving?Amber:Mint;
                float alpha=pressed||arriving?1:Moving?.35f:.55f+.3f*Mathf.Sin(now*3);
                if(arriving)color=Color.Lerp(Color.white,Mint,.5f);
                if(refused<.7f&&Vector3.Distance(RefusalPoint,button.position)<.01f){color=Coral;alpha=1;}
                float radius=button.lossyScale.x*.5f+.006f;
                // The ring sits on the bezel (the button's rest height), so a pressed cap visibly drops inside it.
                Vector3 centre=button.parent.TransformPoint((i==0?panelRest:callRest[i-1])+Vector3.up*button.localScale.y*.6f);
                Vector3 up=button.parent.up;
                rings[i].enabled=chevrons[i].enabled=live;if(!live)continue;
                Circle(rings[i],centre,up,radius);
                color.a=alpha;rings[i].startColor=rings[i].endColor=color;
                // The direction the next press sends the tray: ▲ up, ▼ down.
                int direction=i==0?NextDirection:(i==1?-1:1);
                Vector3 side=Vector3.ProjectOnPlane(game.Owner.View.transform.right,up).normalized,tip=centre+side*(radius+.012f)+up*.010f*direction;
                Vector3 baseCentre=tip-up*.010f*direction;
                chevrons[i].SetPosition(0,baseCentre-side*.007f);chevrons[i].SetPosition(1,tip);chevrons[i].SetPosition(2,baseCentre+side*.007f);
                var chevron=color;chevron.a=Moving&&!pressed?.25f:.9f;chevrons[i].startColor=chevrons[i].endColor=chevron;
            }
        }
        private static void Circle(LineRenderer line,Vector3 centre,Vector3 normal,float radius)
        {
            Vector3 x=Vector3.Cross(normal,Vector3.forward).normalized;if(x.sqrMagnitude<.1f)x=Vector3.Cross(normal,Vector3.right).normalized;
            Vector3 y=Vector3.Cross(normal,x);
            for(int i=0;i<49;i++){float a=i*Mathf.PI*2/48;line.SetPosition(i,centre+(x*Mathf.Cos(a)+y*Mathf.Sin(a))*radius);}
        }
    }
}
