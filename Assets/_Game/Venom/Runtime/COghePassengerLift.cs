using UnityEngine;
namespace GravityBox.Venom
{
    /// <summary>A call panel, boarding check and finite-force motor. Tissue is carried by contact, never positioned.</summary>
    public sealed class COghePassengerLift : COgheMechanism
    {
        public COgheRailSlider Rail;
        public VenomSurfacePatch Deck;
        public Transform Panel;
        public COgheRailSlider RequiredRail;
        public Vector2 DeckSize=new Vector2(.24f,.22f);
        public float DeckHeight=.018f, Speed=.075f, MaximumForce=3;
        // Optional fixed call panels at the landings (0 = lower stop, 1 = upper stop) and props carried as cargo.
        public Transform[] CallPanels=System.Array.Empty<Transform>();
        public bool CarriesProps;
        public Transform BoardPoint;
        public bool Moving {get;private set;}
        public bool Boarding {get;private set;}
        public int Trips {get;private set;}
        private VenomCampaign game;
        private int actor=-1;private float target,stable;
        private VenomCampaignMotion.Order boardingOrder;
        private Vector3 panelRest;

        public override string Activity=>Boarding?"Lên khay nâng":Moving?"Thang đang di chuyển":null;
        public override void InitializeMechanism(VenomCampaign owner){game=owner;panelRest=Panel.localPosition;}
        public override void ResetMechanism(VenomCampaign owner){game=owner;Moving=Boarding=false;actor=-1;Trips=0;stable=0;Rail.Locked=true;boardingOrder=null;}
        public override bool TryTouch(VenomCampaign owner,Ray ray,float obstruction)
        {
            for(int i=0;i<CallPanels.Length;i++)
            {
                if(CallPanels[i]==null||!new Bounds(CallPanels[i].position,new Vector3(.07f,.07f,.07f)).IntersectRay(ray,out float c)||c>obstruction+.006f)continue;
                float station=i==0?0:Rail.Travel;
                if(Moving||RequiredRail!=null&&!RequiredRail.AtEnd)return true;
                // A call panel brings the empty tray to its landing; at that landing it acts as the tray's own button.
                if(Mathf.Abs(Rail.Position-station)>Rail.CatchTolerance*2){actor=owner.Motion.Selected;BeginTravel(station,false);owner.Feedback.ShowCommand(CallPanels[i].position,Vector3.up,CallPanels[i]);return true;}
                return Board(owner,CallPanels[i]);
            }
            if(!new Bounds(Panel.position,new Vector3(.09f,.065f,.085f)).IntersectRay(ray,out float d)||d>obstruction+.006f)return false;
            if(Moving||RequiredRail!=null&&!RequiredRail.AtEnd)return true;
            return Board(owner,Panel);
        }
        private bool Board(VenomCampaign owner,Transform pressed)
        {
            actor=owner.Motion.Selected;
            if(!owner.PrepareTapCommand(actor))return true;
            if(Rail.AtEnd&&!OnDeck(actor)&&CallPanels.Length==0) {BeginTravel(0);return true;}
            Boarding=true;owner.Motion.Move(actor,BoardPoint!=null?BoardPoint.position:Deck.Closest(Rail.Body.position+Vector3.up*DeckHeight)+Vector3.up*.018f);
            boardingOrder=owner.Motion.Get(actor);
            owner.Feedback.ShowCommand(pressed.position,Vector3.up,pressed);return true;
        }
        private void LateUpdate(){if(Panel!=null)Panel.localPosition=Vector3.Lerp(Panel.localPosition,panelRest+Vector3.down*(Moving?.004f:0),1-Mathf.Exp(-Time.deltaTime*12));}
        private bool OnDeck(int index)=>OnDeckPoint(game.Matter.Bodies[index].position,.009f);
        private bool OnDeckPoint(Vector3 world,float margin)
        {
            var p=Rail.Body.transform.InverseTransformPoint(world);
            return Mathf.Abs(p.x)<DeckSize.x*.5f-margin&&Mathf.Abs(p.z)<DeckSize.y*.5f-margin&&p.y>DeckHeight-.008f&&p.y<DeckHeight+.12f;
        }
        private void BeginTravel(float destination,bool carrying=true){target=destination;Moving=true;Boarding=false;stable=0;Rail.Locked=false;Rail.ReleaseLatch();if(carrying)game.Motion.Cancel(actor);}
        public override void StepMechanism(VenomCampaign owner,float dt)
        {
            if(owner.Owner.Lost||owner.Home){Moving=Boarding=false;Rail.Locked=true;return;}
            if(Boarding)
            {
                var current=owner.Motion.Get(actor);
                if(current!=null&&!ReferenceEquals(current,boardingOrder)){Boarding=false;return;}
                bool all=true;for(int i=0;i<32;i++)if(owner.Matter.Groups[i]==owner.Matter.Groups[actor]&&!OnDeck(i)){all=false;break;}
                stable=all?stable+dt:0;if(stable>.2f)BeginTravel(Rail.AtEnd?0:Rail.Travel);
            }
            if(!Moving)return;
            float error=target-Rail.Position,speed=Vector3.Dot(Rail.Body.linearVelocity,Rail.WorldAxis);
            float desired=Mathf.Clamp(error*4,-Speed,Speed);
            float mass=Rail.Body.mass;for(int i=0;i<32;i++)if(OnDeck(i))mass+=owner.Matter.Bodies[i].mass;
            if(CarriesProps)foreach(var prop in owner.Props)if(prop.Body!=Rail.Body&&OnDeckPoint(prop.Body.worldCenterOfMass,0))mass+=prop.Body.mass;
            Rail.ApplyEffort(Rail.WorldAxis*Mathf.Clamp(mass*9.81f+(desired-speed)*1.0f,-MaximumForce,MaximumForce));
            stable=Mathf.Abs(error)<Rail.CatchTolerance&&Mathf.Abs(speed)<.02f?stable+dt:0;
            if(stable>.15f){Moving=false;Rail.Locked=true;Trips++;owner.Motion.BuildGraph(true);}
        }
    }
}
