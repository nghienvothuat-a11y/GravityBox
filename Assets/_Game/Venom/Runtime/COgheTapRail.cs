using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>One command owns one real, reversible rail journey. The body supplies finite effort.</summary>
    public sealed class COgheTapRail : COgheMechanism
    {
        public enum TaskPhase { Idle, Approaching, Operating }
        public COgheRailSlider Rail;
        public Transform Handle;
        public Transform AlternateHandle;
        public VenomSurfacePatch WorkingSurface;
        public Vector3 StandOffset = new Vector3(0, 0, -.067f);
        public Vector3 TouchSize = new Vector3(.108f, .075f, .18f);
        public bool PickHandleOnly;
        public bool TwoSided;
        public bool TrackStandPoint;
        public COgheTissueSensor RequiredLoad;
        // Optional spring-held inputs use the same approach, planted feet and finite muscle force.
        public bool HoldAtEnd;
        public float ReturnForce = .026f;
        public COgheTapRail RequiredGrip;
        public bool Holding { get; private set; }
        public float AppliedEffort { get; private set; }
        public bool CanInterrupt => HoldAtEnd || RequiredGrip != null;
        public COgheRailSlider RequiredRail;
        public COgheTissueClearance Clearance;
        public bool RequiredEnd = true;
        // Negative retains the original terminal interlock. Otherwise a real cam at this rail position releases it.
        public float RequiredPosition = -1;
        public float[] Stops = System.Array.Empty<float>();
        public Transform InterlockPin;
        public string Label = "Cơ quan";
        public float Speed = .09f;
        public float StallSeconds = 3;
        public TaskPhase Phase { get; private set; }
        public bool Busy => Phase != TaskPhase.Idle;
        public bool AtEnd => Rail.AtEnd;
        public int Actor { get; private set; } = -1;
        public int CompletedJourneys { get; private set; }
        public string LastFailure { get; private set; }
        public bool HasStops => Stops != null && Stops.Length > 1;
        public int CurrentStop
        {
            get
            {
                if (!HasStops) return Rail.AtEnd ? 1 : Rail.Position <= Rail.CatchTolerance ? 0 : -1;
                for (int i = 0; i < Stops.Length; i++)
                    if (Mathf.Abs(Rail.Position - Stops[i]) <= Rail.CatchTolerance) return i;
                return -1;
            }
        }
        public int NextStop => HasStops && !Busy && CurrentStop >= 0 ? (CurrentStop + 1) % Stops.Length : targetStop;
        public bool InterlockOpen => (RequiredGrip == null || RequiredGrip.Holding) && (Clearance == null || !Clearance.Blocked) && (RequiredLoad == null || RequiredLoad.Active) &&
            (RequiredRail == null || (RequiredPosition >= 0 ? Mathf.Abs(RequiredRail.Position - RequiredPosition) <= RequiredRail.CatchTolerance :
                RequiredEnd ? RequiredRail.AtEnd : RequiredRail.Position <= RequiredRail.CatchTolerance));
        public override string Activity => Busy ? Label + (Phase == TaskPhase.Approaching ? " · Đang tới" : Holding ? " · Đang giữ" : RequiredGrip != null && !InterlockOpen ? " · Chờ nhả phanh" : " · Đang chuyển") :
            owner != null && owner.Matter.SimulationTime < messageUntil ? LastFailure : null;
        public Vector3 HandPoint => backSide&&AlternateHandle!=null?AlternateHandle.position:Handle != null ? Handle.position : Rail.Body.position;
        public Vector3 StandPoint => WorkingSurface.Closest(HandPoint + Rail.Frame.TransformDirection(stance)) + WorkingSurface.Normal * .022f;
        public bool Pulling => Vector3.Dot(Rail.WorldAxis * (target - Rail.Position), StandPoint - HandPoint) > 0;
        private VenomCampaign owner;
        private VenomCampaignMotion.Order order;
        private readonly List<Vector3> route = new List<Vector3>();
        private float target, lastProgressAt, bestDistance, stableTime, messageUntil;
        private int actorCount;
        private int targetStop;
        private Vector3 pinRest;
        private Vector3 stance;
        private bool backSide;

        public override void InitializeMechanism(VenomCampaign game)
        { owner = game; stance=StandOffset; if (InterlockPin != null) pinRest = InterlockPin.localPosition; }
        public override void ResetMechanism(VenomCampaign game)
        {
            owner = game; Phase = TaskPhase.Idle; Actor = -1; order = null;
            Holding = false; AppliedEffort = 0;
            CompletedJourneys = 0; LastFailure = null; messageUntil = 0; stableTime = 0;
            targetStop = 0; target = 0;
            stance=StandOffset;
            backSide=false;
            Rail.Locked = false;
        }
        public bool Owns(int anchor) => Busy && Actor >= 0 && owner.Matter.Groups[anchor] == owner.Matter.Groups[Actor];
        public bool TryIntent(int anchor, out Vector3 point, out Vector3 velocity)
        {
            point = velocity = Vector3.zero;
            if (Phase != TaskPhase.Operating || !Owns(anchor)) return false;
            point = StandPoint;
            velocity = Rail.WorldAxis * (RequiredGrip != null && !InterlockOpen ? 0 : Mathf.Clamp((target - Rail.Position) * 3, -Speed, Speed));
            if(TrackStandPoint)velocity+=Vector3.ClampMagnitude(Vector3.ProjectOnPlane(point-owner.Motion.Centre(anchor),WorkingSurface.Normal)*4,Speed);
            return true;
        }
        public override bool TryTouch(VenomCampaign game, Ray ray, float nearestSolidDistance)
        {
            // Handle meshes have non-unit scale. Pick an authored metric envelope around the entire visible carriage.
            var frame = Rail.Frame;
            Quaternion inverse = Quaternion.Inverse(frame.rotation);
            bool Visible(Vector3 centre,Vector3 face)
            {
                var local=new Ray(inverse*(ray.origin-centre),inverse*ray.direction);
                return new Bounds(Vector3.zero,TouchSize).IntersectRay(local,out float distance)&&distance<=nearestSolidDistance+.002f&&
                    new Plane(WorkingSurface.Normal,face).Raycast(ray,out float faceDistance)&&faceDistance<=nearestSolidDistance+.002f;
            }
            Vector3 primary=Handle!=null?Handle.position:Rail.Body.position;
            bool hit=Visible(PickHandleOnly?primary:Rail.Body.position+WorkingSurface.Normal*.023f,PickHandleOnly?primary:Rail.Body.position+WorkingSurface.Normal*.037f);
            if(!hit&&(AlternateHandle==null||!Visible(AlternateHandle.position,AlternateHandle.position)))return false;
            Request(game.Motion.Selected);
            game.Feedback.ShowCommand(HandPoint, WorkingSurface.Normal, Rail.transform);
            return true;
        }
        public bool Request(int anchor)
        {
            if (!owner.Owner.CanControl || owner.Home || anchor < 0 || anchor >= CohesiveOrganism.ParticleCount || owner.Matter.Escaped[anchor]) return false;
            if (Busy) { Message("Cơ quan đang thực hiện"); return false; }
            if (!InterlockOpen && RequiredGrip == null) { Message(Clearance != null && Clearance.Blocked ? "Có mô trong vùng chuyển — đưa về bệ an toàn" : RequiredLoad != null ? "Cần một phần giữ bàn đạp" : "Chốt đang khóa"); return false; }
            if (!owner.PrepareTapCommand(anchor)) return false;
            stance=StandOffset;
            backSide=TwoSided&&Vector3.Dot(owner.Motion.Centre(anchor)-Rail.Body.position,Rail.Frame.TransformDirection(StandOffset))<0;
            if(backSide)stance=-StandOffset;
            owner.Motion.BuildGraph();
            if (!owner.Motion.FindPath(owner.Motion.Centre(anchor), StandPoint, route, true))
            { Message("Đường tới cơ quan đang bị chặn"); return false; }
            Actor = anchor;
            actorCount = CountActor();
            if (HoldAtEnd) target = Rail.Travel;
            else if (HasStops)
            {
                int current = CurrentStop;
                if (current >= 0) targetStop = (current + 1) % Stops.Length;
                target = Stops[targetStop];
            }
            else if (Rail.AtEnd) target = 0;
            else if (Rail.Position <= Rail.CatchTolerance) target = Rail.Travel;
            owner.Motion.Move(anchor, StandPoint, true);
            order = owner.Motion.Get(anchor);
            Phase = TaskPhase.Approaching; LastFailure = null;
            lastProgressAt = owner.Matter.SimulationTime;
            bestDistance = Vector3.Distance(owner.Motion.Centre(anchor), StandPoint); stableTime = 0;
            return true;
        }
        private int CountActor()
        {
            int count = 0;
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                if (!owner.Matter.Escaped[i] && owner.Matter.Groups[i] == owner.Matter.Groups[Actor]) count++;
            return count;
        }
        private void Message(string message)
        { LastFailure = message; messageUntil = owner.Matter.SimulationTime + 2.5f; }
        public void CompleteHold()
        {
            if (!HoldAtEnd || !Busy) return;
            CompletedJourneys++; CancelTask(); Message("Đã chốt — có thể rời tay nắm");
        }
        public void CancelTask(string reason = null)
        {
            if (Actor >= 0 && ReferenceEquals(owner.Motion.Get(Actor), order)) owner.Motion.Cancel(Actor);
            Phase = TaskPhase.Idle; Actor = -1; order = null;
            Holding = false; AppliedEffort = 0;
            if (reason != null) Message(reason);
        }
        private void OnDisable()
        {if(owner!=null&&owner.Motion!=null)CancelTask();}
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            Holding = false; AppliedEffort = 0;
            if (HoldAtEnd && Rail.Position > .0002f)
                Rail.ApplyEffort(-Rail.WorldAxis * ReturnForce);
            // The detent holds the measured position, including a cancelled partial journey.
            // Reissuing a partial journey resumes its target; only physically reaching a stop advances the cycle.
            Rail.Locked = !InterlockOpen || (HasStops && !Busy);
            if (InterlockPin != null) InterlockPin.localPosition = pinRest + (InterlockOpen ? Vector3.up * .025f : Vector3.zero);
            if (!Busy) return;
            // A merge keeps Motion's newest real command; a cut never leaves a force owned by the old body.
            if (game.Owner.Lost || game.Home || !ReferenceEquals(game.Motion.Get(Actor), order) || CountActor() < actorCount)
            { CancelTask("Lệnh đã thay đổi"); return; }
            actorCount = CountActor();
            if (!InterlockOpen && RequiredGrip == null) { CancelTask("Chốt đã khóa — giữ bàn đạp rồi thử lại"); return; }
            float now = game.Matter.SimulationTime;
            Vector3 centre = game.Motion.Centre(Actor);
            int feet = 0;
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                if (game.Matter.Groups[i] == game.Matter.Groups[Actor] && game.Motion.HasGrip(i) &&
                    game.Motion.Support(i, out var collider, out _, out _) && collider == WorkingSurface.Shape) feet++;
            if (Phase == TaskPhase.Approaching)
            {
                float distance = Vector3.Distance(centre, StandPoint);
                if (distance < bestDistance - .006f) { bestDistance = distance; lastProgressAt = now; }
                if (distance < .039f && feet >= 2 && game.Clear(centre, HandPoint, 0, Rail.Body))
                { Phase = TaskPhase.Operating; bestDistance = Mathf.Abs(target - Rail.Position); lastProgressAt = now; }
                else if (now - lastProgressAt > StallSeconds + 3) CancelTask("Không tới được tay nắm — thử đường khác");
                return;
            }
            float remaining = Mathf.Abs(target - Rail.Position);
            if (remaining < bestDistance - .002f) { bestDistance = remaining; lastProgressAt = now; }
            Vector3 axis = Rail.WorldAxis;
            float velocity = Vector3.Dot(Rail.Body.linearVelocity - Rail.Frame.GetComponent<Rigidbody>().GetPointVelocity(Rail.Body.position), axis);
            bool reached = remaining <= Rail.CatchTolerance;
            stableTime = reached && Mathf.Abs(velocity) < .025f ? stableTime + dt : 0;
            if (!HoldAtEnd && stableTime >= .10f)
            { CompletedJourneys++; CancelTask(); game.Motion.BuildGraph(); return; }
            if (feet < 2 || Vector3.Distance(centre, HandPoint) > .145f)
            { CancelTask("Mất điểm bám — chạm lại để tiếp tục"); return; }
            if (RequiredGrip != null && !InterlockOpen) { lastProgressAt = now; return; }
            if (!(HoldAtEnd && reached) && now - lastProgressAt > StallSeconds)
            { CancelTask("Cơ quan bị kẹt hoặc phần này chưa đủ lực"); return; }
            float desired = Mathf.Clamp((target - Rail.Position) * 3, -Speed, Speed);
            float mass = actorCount * game.Matter.Profile.ParticleMass;
            // Finite hand effort, equal/opposite tissue reaction and actual planted-foot support.
            float gain = HoldAtEnd ? Rail.Body.mass * 20 : Mathf.Max(Rail.Body.mass, .18f) * 25;
            float effort = Mathf.Clamp((desired - velocity) * gain + (HoldAtEnd ? ReturnForce + Rail.Resistance : 0),
                -mass * 7, mass * 7);
            if (reached && !HoldAtEnd) effort = 0;
            AppliedEffort = Mathf.Max(0, effort);
            Holding = HoldAtEnd && reached && effort >= ReturnForce * .9f;
            Vector3 force = axis * effort;
            Rail.ApplyEffort(force);
            game.Motion.BraceAgainstManipulation(Actor, force);
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                if (game.Matter.Groups[i] == game.Matter.Groups[Actor]) game.Matter.Bodies[i].AddForce(-force / actorCount);
        }
    }
}
