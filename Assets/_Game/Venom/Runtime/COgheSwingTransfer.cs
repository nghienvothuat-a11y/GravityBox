using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// A rope swing: one fixed anchor, a finite-length rope constraint and a grip ring. A winch parks the
    /// ring on its start hook. The body grips the ring with finite forces, the player chooses a landing
    /// bank, the hook releases and gravity alone swings the pendulum. The body lets go only when its own
    /// tissue is inside the chosen bank's real contact envelope; a miss swings back to the hook.
    /// </summary>
    public sealed class COgheSwingTransfer : COgheMechanism
    {
        public enum SwingPhase { Idle, Approaching, Gripping, Ready, Swinging, Recovering }
        public Transform Pivot;
        public Rigidbody Ring;
        public float RopeLength = .26f;
        public Transform StartHook;
        public Transform StandPoint;
        public VenomSurfacePatch StartBank;
        public VenomSurfacePatch[] Docks = System.Array.Empty<VenomSurfacePatch>();
        public Transform[] DockTargets = System.Array.Empty<Transform>();
        public VenomSurfacePatch RescueFloor;
        public float HangDepth = .035f, TouchRadius = .045f, WinchForce = .9f;
        public int LandingContacts = 8;
        public LineRenderer Rope;
        public Transform Drum, RingVisual;
        // Normal of the swing plane (presentation only: the drum turns about it).
        public Vector3 PlaneNormal = Vector3.forward;
        public string Label = "A";

        public SwingPhase Phase { get; private set; }
        public int Actor { get; private set; } = -1;
        public int Landings { get; private set; }
        public int Misses { get; private set; }
        public int ChosenDock { get; private set; } = -1;
        public string LastMessage { get; private set; }
        public bool Parked => parked;
        public override bool TransportsTissue => true;
        public override string Activity => Phase == SwingPhase.Approaching ? "Tới vòng " + Label : Phase == SwingPhase.Gripping ? "Ôm vòng " + Label :
            Phase == SwingPhase.Ready ? "Đang bám dây — chạm bến muốn tới" : Phase == SwingPhase.Swinging ? "Đang đu" :
            Phase == SwingPhase.Recovering ? "Tời đưa dây về bến" : owner != null && owner.Matter.SimulationTime < messageUntil ? LastMessage : null;

        private VenomCampaign owner;
        private VenomCampaignMotion.Order order;
        private int replans;
        private readonly bool[] member = new bool[CohesiveOrganism.ParticleCount];
        private readonly Vector3[] offset = new Vector3[CohesiveOrganism.ParticleCount];
        private bool parked, carrying, passedLowest;
        private float clock, messageUntil, lastSide;
        private Quaternion drumRest;

        public override void InitializeMechanism(VenomCampaign game) { owner = game; if (Drum != null) drumRest = Drum.localRotation; }
        public override void ResetMechanism(VenomCampaign game)
        {
            owner = game; Phase = SwingPhase.Idle; Actor = -1; order = null; Landings = Misses = 0; ChosenDock = -1; LastMessage = null; messageUntil = 0;
            carrying = passedLowest = false; clock = 0; System.Array.Clear(member, 0, member.Length);
            Park();
        }
        public override bool SuppressesMotion(int particle) => carrying && particle >= 0 && particle < member.Length && member[particle];
        public override bool IsFlowing(int particle) => false;
        public override bool AllowsExitAssist(int particle, Vector3 capturePoint) => !SuppressesMotion(particle);

        private void Park()
        {
            Ring.isKinematic = true; Ring.position = StartHook.position; Ring.rotation = StartHook.rotation;
            Ring.linearVelocity = Ring.angularVelocity = Vector3.zero; parked = true;
        }
        private void Message(string text) { LastMessage = text; messageUntil = owner.Matter.SimulationTime + 2.5f; }
        private Vector3 Hang => Ring.position + Vector3.down * HangDepth;
        private bool Hits(VenomSurfacePatch patch, Ray ray, float limit) => patch != null && patch.Shape != null && patch.Shape.Raycast(ray, out _, limit + .002f);

        public override bool TryTouch(VenomCampaign game, Ray ray, float nearestSolidDistance)
        {
            int selected = game.Motion.Selected;
            bool mine = Actor >= 0 && game.Matter.Groups[selected] == game.Matter.Groups[Actor];
            if (carrying && mine)
            {
                for (int d = 0; d < Docks.Length; d++)
                    if (Docks[d] != null && Docks[d].isActiveAndEnabled && Hits(Docks[d], ray, nearestSolidDistance))
                    {
                        if (Phase != SwingPhase.Ready) { Message("Đang đu — chờ dây ổn định"); return true; }
                        ChosenDock = d; Launch(); game.Feedback.ShowCommand(DockTargets[d].position, Vector3.up, DockTargets[d]); return true;
                    }
                if (Phase == SwingPhase.Ready && Hits(StartBank, ray, nearestSolidDistance)) { LetGo(StandPoint.position, "Đã buông dây ở bến xuất phát"); return true; }
                if (Hits(RescueFloor, ray, nearestSolidDistance)) { LetGo(null, "Buông dây xuống sàn cứu hộ"); return true; }
                Message("Chạm bến muốn tới"); return true;
            }
            if (Phase != SwingPhase.Idle || !parked) return false;
            if (new Plane(-ray.direction, Ring.position).Raycast(ray, out float distance) && distance <= nearestSolidDistance + .03f &&
                Vector3.Distance(ray.GetPoint(distance), Ring.position) <= TouchRadius)
            {
                if (!game.PrepareTapCommand(selected)) return true;
                Actor = selected; game.Motion.Move(Actor, StandPoint.position, true); order = game.Motion.Get(Actor);
                Phase = SwingPhase.Approaching; clock = 0; replans = 0;
                game.Feedback.ShowCommand(Ring.position, Vector3.up, Ring.transform);
                return true;
            }
            return false;
        }

        private void Launch()
        {
            Ring.isKinematic = false; parked = false; Phase = SwingPhase.Swinging; clock = 0; passedLowest = false;
            lastSide = Side(Ring.position);
        }
        // Signed horizontal offset in the swing plane: positive on the start side, zero under the anchor.
        private float Side(Vector3 p) { Vector3 start = Vector3.ProjectOnPlane(StartHook.position - Pivot.position, Vector3.up); return Vector3.Dot(p - Pivot.position, start.normalized); }

        private void LetGo(Vector3? destination, string text)
        {
            int anchor = Actor;
            carrying = false; System.Array.Clear(member, 0, member.Length);
            Phase = parked ? SwingPhase.Idle : SwingPhase.Recovering; clock = 0; Actor = -1; order = null;
            if (destination.HasValue && anchor >= 0) owner.Motion.Move(anchor, destination.Value);
            owner.Motion.BuildGraph();
            if (text != null) Message(text);
        }

        public override void StepMechanism(VenomCampaign game, float dt)
        {
            owner = game; clock += dt;
            if (game.Owner.Lost || game.Home) { if (carrying) LetGo(null, null); return; }
            if (!Ring.isKinematic) Ring.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            if (Phase == SwingPhase.Approaching)
            {
                if (!ReferenceEquals(game.Motion.Get(Actor), order))
                {
                    // A peel on the climb to the ring dropped the walk, not the player: head for the ring again.
                    if (game.Motion.Get(Actor) == null && game.Motion.Peeled(Actor) && replans < 3)
                    { replans++; game.Motion.Move(Actor, StandPoint.position, true); order = game.Motion.Get(Actor); return; }
                    Phase = SwingPhase.Idle; Actor = -1; order = null; return;
                }
                Vector3 centre = game.Motion.Centre(Actor);
                if (Vector3.Distance(centre, StandPoint.position) < .05f && Vector3.Distance(centre, Hang) < .11f)
                {
                    int group = game.Matter.Groups[Actor];
                    for (int i = 0; i < 32; i++)
                    {
                        member[i] = game.Matter.Groups[i] == group && !game.Matter.Escaped[i];
                        if (member[i]) offset[i] = Vector3.ClampMagnitude((game.Matter.Bodies[i].position - centre) * .7f, .03f);
                    }
                    game.Motion.Cancel(Actor); order = null; carrying = true; Phase = SwingPhase.Gripping; clock = 0;
                }
                else if (clock > 25) { Phase = SwingPhase.Idle; Actor = -1; Message("Không tới được vòng " + Label); }
                return;
            }
            if (carrying) Carry(game);
            if (Phase == SwingPhase.Gripping)
            {
                bool held = true;
                for (int i = 0; i < 32; i++) if (member[i] && Vector3.Distance(game.Matter.Bodies[i].position, Hang + offset[i]) > .03f) held = false;
                if (held && clock > .5f) { Phase = SwingPhase.Ready; Message("Đã bám vòng " + Label + " — chạm bến muốn tới"); }
                else if (clock > 6) LetGo(StandPoint.position, "Chưa ôm được vòng — thử lại");
            }
            else if (Phase == SwingPhase.Swinging)
            {
                int landed = ChosenDock >= 0 ? Landed(game) : -1;
                if (landed >= 0)
                {
                    Landings++; LetGo(DockTargets[landed].position, "Đã sang bến"); return;
                }
                float side = Side(Ring.position);
                if (side < 0) passedLowest = true;
                // Past the far apex without a landing: the rope now carries the body back to the winch.
                if (passedLowest && side > lastSide && clock > .25f) { Misses++; Phase = SwingPhase.Recovering; clock = 0; Message("Chưa tới bến — tời đưa dây về"); }
                lastSide = side;
            }
            else if (Phase == SwingPhase.Recovering)
            {
                Vector3 toward = StartHook.position - Ring.position;
                Vector3 pull = Vector3.ClampMagnitude(toward * 40 - Ring.linearVelocity * 6, 1) * WinchForce;
                float load = Ring.mass; if (carrying) for (int i = 0; i < 32; i++) if (member[i]) load += game.Matter.Bodies[i].mass;
                if (!Ring.isKinematic) Ring.AddForce(pull * load * 9.81f + Vector3.up * 9.81f * load * .9f);
                if (toward.magnitude < .006f && Ring.linearVelocity.magnitude < .08f)
                {
                    Park();
                    if (carrying) Phase = SwingPhase.Ready; else { Phase = SwingPhase.Idle; }
                }
                else if (clock > 12) { Park(); Phase = carrying ? SwingPhase.Ready : SwingPhase.Idle; }
            }
        }

        // The authored docks form one landing (a docked replica, its moving deck and the bank beside it): the body
        // has landed once enough of the carried tissue rests on their real contact envelopes together.
        private int Landed(VenomCampaign game)
        {
            int best = -1, bestContacts = 0, total = 0, carried = 0;
            for (int i = 0; i < 32; i++) if (member[i]) carried++;
            // "Enough tissue" scales with what the rope carries: 3/8 of it, at least 3, never more than LandingContacts.
            int need = Mathf.Min(LandingContacts, Mathf.Max(3, carried * 3 / 8));
            for (int d = 0; d < Docks.Length; d++)
            {
                if (Docks[d] == null || !Docks[d].isActiveAndEnabled) continue;
                int contacts = 0;
                for (int i = 0; i < 32; i++)
                    if (member[i] && game.Motion.Support(i, out var collider, out _, out _) && collider == Docks[d].Shape) contacts++;
                total += contacts;
                if (contacts > bestContacts) { best = d; bestContacts = contacts; }
            }
            return total >= need ? best : -1;
        }

        // Finite spring-damper grip between each particle and the ring. Every reaction loads the rope.
        private void Carry(VenomCampaign game)
        {
            Vector3 ringVelocity = Ring.isKinematic ? Vector3.zero : Ring.linearVelocity, reaction = Vector3.zero;
            for (int i = 0; i < 32; i++)
            {
                if (!member[i]) continue;
                var body = game.Matter.Bodies[i];
                Vector3 acceleration = (Hang + offset[i] - body.position) * 600 + (ringVelocity - body.linearVelocity) * 12;
                if (Phase == SwingPhase.Gripping) acceleration = Vector3.ClampMagnitude(acceleration, 24) + Vector3.up * 9.81f * Mathf.Clamp01(clock * 2);
                Vector3 force = Vector3.ClampMagnitude(acceleration, 60) * body.mass;
                body.AddForce(force); reaction -= force;
            }
            if (!Ring.isKinematic) Ring.AddForce(reaction);
        }

        private void LateUpdate()
        {
            if (Pivot == null || Ring == null) return;
            if (Rope != null) { Rope.positionCount = 2; Rope.SetPosition(0, Pivot.position); Rope.SetPosition(1, Ring.position); }
            Vector3 rope = Ring.position - Pivot.position;
            if (RingVisual != null && rope.sqrMagnitude > .0001f) RingVisual.rotation = Quaternion.FromToRotation(Vector3.down, rope.normalized) * StartHook.rotation;
            if (Drum != null) Drum.localRotation = drumRest * Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.down, rope, PlaneNormal) * 3);
        }
    }
}
