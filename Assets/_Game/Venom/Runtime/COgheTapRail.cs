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
        public VenomSurfacePatch WorkingSurface;
        public Vector3 StandOffset = new Vector3(0, 0, -.067f);
        public Vector3 TouchSize = new Vector3(.108f, .075f, .18f);
        public COgheTissueSensor RequiredLoad;
        public COgheRailSlider RequiredRail;
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
        public bool InterlockOpen => (RequiredLoad == null || RequiredLoad.Active) &&
            (RequiredRail == null || (RequiredPosition >= 0 ? Mathf.Abs(RequiredRail.Position - RequiredPosition) <= RequiredRail.CatchTolerance :
                RequiredEnd ? RequiredRail.AtEnd : RequiredRail.Position <= RequiredRail.CatchTolerance));
        public override string Activity => Busy ? Label + (Phase == TaskPhase.Approaching ? " · Đang tới" : " · Đang chuyển") :
            owner != null && owner.Matter.SimulationTime < messageUntil ? LastFailure : null;
        public Vector3 HandPoint => Handle != null ? Handle.position : Rail.Body.position;
        public Vector3 StandPoint => WorkingSurface.Closest(HandPoint + Rail.Frame.TransformDirection(StandOffset)) + WorkingSurface.Normal * .022f;
        public bool Pulling => Vector3.Dot(Rail.WorldAxis * (target - Rail.Position), StandPoint - HandPoint) > 0;
        private VenomCampaign owner;
        private VenomCampaignMotion.Order order;
        private readonly List<Vector3> route = new List<Vector3>();
        private float target, lastProgressAt, bestDistance, stableTime, messageUntil;
        private int actorCount;
        private int targetStop;
        private Vector3 pinRest;

        public override void InitializeMechanism(VenomCampaign game)
        { owner = game; if (InterlockPin != null) pinRest = InterlockPin.localPosition; }
        public override void ResetMechanism(VenomCampaign game)
        {
            owner = game; Phase = TaskPhase.Idle; Actor = -1; order = null;
            CompletedJourneys = 0; LastFailure = null; messageUntil = 0; stableTime = 0;
            targetStop = 0; target = 0;
            Rail.Locked = false;
        }
        public bool Owns(int anchor) => Busy && Actor >= 0 && owner.Matter.Groups[anchor] == owner.Matter.Groups[Actor];
        public bool TryIntent(int anchor, out Vector3 point, out Vector3 velocity)
        {
            point = velocity = Vector3.zero;
            if (Phase != TaskPhase.Operating || !Owns(anchor)) return false;
            point = StandPoint;
            velocity = Rail.WorldAxis * Mathf.Clamp((target - Rail.Position) * 3, -Speed, Speed);
            return true;
        }
        public override bool TryTouch(VenomCampaign game, Ray ray, float nearestSolidDistance)
        {
            // Handle meshes have non-unit scale. Pick an authored metric envelope around the entire visible carriage.
            var frame = Rail.Frame;
            Vector3 centre = Rail.Body.position + WorkingSurface.Normal * .023f;
            Quaternion inverse = Quaternion.Inverse(frame.rotation);
            var local = new Ray(inverse * (ray.origin - centre), inverse * ray.direction);
            if (!new Bounds(Vector3.zero, TouchSize).IntersectRay(local, out float distance)) return false;
            if (distance > nearestSolidDistance + .002f) return false;
            if(!new Plane(WorkingSurface.Normal,Rail.Body.position+WorkingSurface.Normal*.037f).Raycast(ray,out float faceDistance) ||
                faceDistance>nearestSolidDistance+.002f)return false;
            Request(game.Motion.Selected);
            game.Feedback.ShowCommand(HandPoint, WorkingSurface.Normal, Rail.transform);
            return true;
        }
        public bool Request(int anchor)
        {
            if (!owner.Owner.CanControl || owner.Home || anchor < 0 || anchor >= CohesiveOrganism.ParticleCount || owner.Matter.Escaped[anchor]) return false;
            if (Busy) { Message("Cơ quan đang thực hiện"); return false; }
            if (!InterlockOpen) { Message(RequiredLoad != null ? "Cần một phần giữ bàn đạp" : "Chốt đang khóa"); return false; }
            if (!owner.PrepareTapCommand(anchor)) return false;
            owner.Motion.BuildGraph();
            if (!owner.Motion.FindPath(owner.Motion.Centre(anchor), StandPoint, route, true))
            { Message("Đường tới cơ quan đang bị chặn"); return false; }
            Actor = anchor;
            actorCount = CountActor();
            if (HasStops)
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
        public void CancelTask(string reason = null)
        {
            if (Actor >= 0 && ReferenceEquals(owner.Motion.Get(Actor), order)) owner.Motion.Cancel(Actor);
            Phase = TaskPhase.Idle; Actor = -1; order = null;
            if (reason != null) Message(reason);
        }
        private void OnDisable()
        {if(owner!=null&&owner.Motion!=null)CancelTask();}
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            // The detent holds the measured position, including a cancelled partial journey.
            // Reissuing a partial journey resumes its target; only physically reaching a stop advances the cycle.
            Rail.Locked = !InterlockOpen || (HasStops && !Busy);
            if (InterlockPin != null) InterlockPin.localPosition = pinRest + (InterlockOpen ? Vector3.up * .025f : Vector3.zero);
            if (!Busy) return;
            // A merge keeps Motion's newest real command; a cut never leaves a force owned by the old body.
            if (game.Owner.Lost || game.Home || !ReferenceEquals(game.Motion.Get(Actor), order) || CountActor() < actorCount)
            { CancelTask("Lệnh đã thay đổi"); return; }
            actorCount = CountActor();
            if (!InterlockOpen) { CancelTask("Chốt đã khóa — giữ bàn đạp rồi thử lại"); return; }
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
            if (stableTime >= .10f)
            { CompletedJourneys++; CancelTask(); game.Motion.BuildGraph(); return; }
            if (feet < 2 || Vector3.Distance(centre, HandPoint) > .145f)
            { CancelTask("Mất điểm bám — chạm lại để tiếp tục"); return; }
            if (now - lastProgressAt > StallSeconds)
            { CancelTask("Cơ quan bị kẹt hoặc phần này chưa đủ lực"); return; }
            float desired = Mathf.Clamp((target - Rail.Position) * 3, -Speed, Speed);
            float mass = actorCount * game.Matter.Profile.ParticleMass;
            // Finite hand effort, equal/opposite tissue reaction and actual planted-foot support.
            float effort = Mathf.Clamp((desired - velocity) * Mathf.Max(Rail.Body.mass, .18f) * 25,
                -mass * 7, mass * 7);
            if (reached) effort = 0;
            Vector3 force = axis * effort;
            Rail.ApplyEffort(force);
            game.Motion.BraceAgainstManipulation(Actor, force);
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                if (game.Matter.Groups[i] == game.Matter.Groups[Actor]) game.Matter.Bodies[i].AddForce(-force / actorCount);
        }
    }
}
