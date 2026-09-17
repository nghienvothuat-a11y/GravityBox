using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// Observes the real slide, free flight and wall contact in level 12. It
    /// never moves tissue; the trough, world gravity and collision response own
    /// the trajectory.
    /// </summary>
    public sealed class COgheSlideLaunchMonitor : COgheMechanism
    {
        public VenomSurfacePatch[] SlideSurfaces;
        public VenomSurfacePatch CatchSurface;
        public Transform LaunchPlane;
        public bool EnteredSlide { get; private set; }
        public bool Launched { get; private set; }
        public bool Caught { get; private set; }
        public float PeakAirSpeed { get; private set; }
        public float CurrentSpeed { get; private set; }
        public float PeakSlideSpeed { get; private set; }
        public int SlideContactCount { get; private set; }
        public int SupportContactCount { get; private set; }
        public int ActualContactCount { get; private set; }
        public int AirborneParticleCount { get; private set; }
        public int AirborneFrames { get; private set; }
        public override string Activity => Caught ? "Bám được rồi" : Launched ? "Đang bay" : EnteredSlide ? "Trượt lấy đà" : null;
        private readonly bool[] observedAirborne=new bool[CohesiveOrganism.ParticleCount];
        private readonly Collider[] overlapBuffer=new Collider[64];

        public override void ResetMechanism(VenomCampaign game)
        {
            EnteredSlide=false;Launched=false;Caught=false;PeakAirSpeed=CurrentSpeed=PeakSlideSpeed=0;
            SlideContactCount=SupportContactCount=ActualContactCount=AirborneParticleCount=AirborneFrames=0;
            System.Array.Clear(observedAirborne,0,observedAirborne.Length);
        }

        public override void StepMechanism(VenomCampaign game,float dt)
        {
            bool onCatch=false;float speed=0;int slideContacts=0,supportContacts=0,actualContacts=0,particles=0;
            for(int i=0;i<CohesiveOrganism.ParticleCount;i++)
            {
                if(game.Matter.Escaped[i])continue;
                particles++;
                speed=Mathf.Max(speed,game.Matter.Bodies[i].linearVelocity.magnitude);
                bool physicalContact=HasPhysicalContact(game,i);
                if(physicalContact)actualContacts++;
                if(EnteredSlide&&LaunchPlane!=null&&!physicalContact&&LaunchPlane.InverseTransformPoint(game.Matter.Bodies[i].position).z>.002f)
                    observedAirborne[i]=true;
                if(!game.Motion.Support(i,out var shape,out _,out _))continue;
                supportContacts++;
                if(CatchSurface!=null&&shape==CatchSurface.Shape&&game.Motion.HasGrip(i))onCatch=true;
                if(SlideSurfaces!=null)foreach(var slide in SlideSurfaces)
                    if(slide!=null&&shape==slide.Shape){slideContacts++;break;}
            }
            bool onSlide=slideContacts>0;
            int airborne=0;for(int i=0;i<observedAirborne.Length;i++)if(observedAirborne[i])airborne++;
            CurrentSpeed=speed;SlideContactCount=slideContacts;SupportContactCount=supportContacts;
            ActualContactCount=actualContacts;AirborneParticleCount=airborne;
            EnteredSlide|=onSlide;
            if(onSlide)PeakSlideSpeed=Mathf.Max(PeakSlideSpeed,speed);
            AirborneFrames=EnteredSlide&&actualContacts<particles?AirborneFrames+1:0;
            if(EnteredSlide&&airborne>0)PeakAirSpeed=Mathf.Max(PeakAirSpeed,speed);
            if(EnteredSlide&&particles>0&&airborne>=particles)Launched=true;
            Caught|=Launched&&onCatch;
        }

        private bool HasPhysicalContact(VenomCampaign game,int particle)
        {
            Rigidbody body=game.Matter.Bodies[particle];float radius=game.Matter.Profile.ParticleRadius+.001f;
            int count=Physics.OverlapSphereNonAlloc(body.position,radius,overlapBuffer,~0,QueryTriggerInteraction.Ignore);
            int group=game.Matter.Groups[particle];
            for(int h=0;h<count;h++)
            {
                Rigidbody hitBody=overlapBuffer[h].attachedRigidbody;bool tissue=false;
                for(int i=0;i<CohesiveOrganism.ParticleCount;i++)if(game.Matter.Groups[i]==group&&game.Matter.Bodies[i]==hitBody){tissue=true;break;}
                if(!tissue)return true;
            }
            return false;
        }
    }
}
