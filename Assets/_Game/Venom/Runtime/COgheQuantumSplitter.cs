using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// Quantum machine Q. A whole fragment that has entered the chamber is gathered and drawn into
    /// two lobes of exactly equal particle count by finite forces. A real septum then closes between
    /// the lobes and only afterwards does the topology change. Each lobe leaves to its own tray.
    /// One split per entry: tissue must leave the chamber before it can be split again.
    /// </summary>
    public sealed class COgheQuantumSplitter : COgheMechanism
    {
        public enum SplitPhase { Idle, Gathering, Separating, Sealing, Clearing }
        // Local frame: x from the left tray (-) to the right tray (+), y up, z from the entrance (-) to the back (+).
        public Transform Chamber;
        public Vector3 ReceiveHalfSize = new Vector3(.062f, .055f, .050f);
        public Vector3 GatherLocal = new Vector3(0, .03f, .005f);
        public float LobeOffset = .045f;
        public COgheRailSlider Septum;
        // Tray lane gates: closed except while Q delivers lobes, so the chamber has a single way in (the front).
        public COgheRailSlider LeftGate, RightGate;
        public Vector3[] LeftLane = System.Array.Empty<Vector3>(), RightLane = System.Array.Empty<Vector3>();
        public Transform LeftTray, RightTray;
        public Vector2 TrayHalfSize = new Vector2(.055f, .055f);
        public Vector3 TouchCentre = new Vector3(0, .09f, .01f), TouchHalfSize = new Vector3(.10f, .06f, .06f);
        public int MinimumParticles = 16;
        public float Gap = .0115f, Acceleration = 16f, SeptumForce = .35f;
        public Transform ScanSheet;
        public Renderer[] Lamps = System.Array.Empty<Renderer>();
        public Material LampIdle, LampActive;

        public SplitPhase Phase { get; private set; }
        public int Splits { get; private set; }
        public int LastLeft { get; private set; } = -1;
        public int LastRight { get; private set; } = -1;
        public string LastMessage { get; private set; }
        public int Requested => requested;
        public int ArmedInside { get { int n = 0; if (owner != null) for (int i = 0; i < 32; i++) if (armed[i] && Inside(i)) n++; return n; } }
        public override bool TransportsTissue => true;
        public override string Activity => Phase == SplitPhase.Gathering ? "Q · Đang gom" : Phase == SplitPhase.Separating ? "Q · Tạo hai thuỳ" :
            Phase == SplitPhase.Sealing ? "Q · Vách phân luồng khép" : Phase == SplitPhase.Clearing ? "Q · Đưa hai phần ra khay" :
            owner != null && owner.Matter.SimulationTime < messageUntil ? LastMessage : null;

        private VenomCampaign owner;
        private readonly bool[] armed = new bool[CohesiveOrganism.ParticleCount];
        private readonly bool[] member = new bool[CohesiveOrganism.ParticleCount];
        private readonly bool[] left = new bool[CohesiveOrganism.ParticleCount];
        private readonly int[] order = new int[CohesiveOrganism.ParticleCount];
        private readonly int[] lane = new int[2];
        private int anchor = -1, count;
        private float clock, messageUntil;
        private bool selectLeft;
        // Q processes the part the player sent into it (tap on Q); a part merely routed past the lanes is not split.
        private int requested = -1;
        private VenomCampaignMotion.Order requestOrder;

        public override void InitializeMechanism(VenomCampaign game) { owner = game; }
        public override void ResetMechanism(VenomCampaign game)
        {
            owner = game; Phase = SplitPhase.Idle; Splits = 0; LastLeft = LastRight = -1; LastMessage = null; messageUntil = 0;
            anchor = -1; count = 0; clock = 0; lane[0] = lane[1] = 0; requested = -1; requestOrder = null;
            System.Array.Clear(member, 0, member.Length); System.Array.Clear(left, 0, left.Length);
            for (int i = 0; i < armed.Length; i++) armed[i] = true;
            if (Septum != null) Septum.Locked = true;
            Show();
        }
        public override bool SuppressesMotion(int particle) => Phase != SplitPhase.Idle && particle >= 0 && particle < member.Length && member[particle];
        // While lobes form and leave, tissue flows (soft bonds, neighbour exchange) as it does in a tube;
        // the machine carries its weight so it does not spread into a sheet.
        public override bool IsFlowing(int particle) => SuppressesMotion(particle) && Phase != SplitPhase.Gathering;
        public bool Inside(int particle)
        {
            Vector3 p = Chamber.InverseTransformPoint(owner.Matter.Bodies[particle].position) - new Vector3(0, ReceiveHalfSize.y, GatherLocal.z);
            return Mathf.Abs(p.x) <= ReceiveHalfSize.x && Mathf.Abs(p.y) <= ReceiveHalfSize.y && Mathf.Abs(p.z) <= ReceiveHalfSize.z;
        }
        private bool OnTray(Transform tray, int particle)
        {
            Vector3 p = tray.InverseTransformPoint(owner.Matter.Bodies[particle].position);
            return Mathf.Abs(p.x) <= TrayHalfSize.x && Mathf.Abs(p.z) <= TrayHalfSize.y && p.y > -.02f && p.y < .08f;
        }
        public Vector3 EntryPoint => Chamber.TransformPoint(GatherLocal + Vector3.up * .008f);

        public override bool TryTouch(VenomCampaign game, Ray ray, float nearestSolidDistance)
        {
            var local = new Ray(Chamber.InverseTransformPoint(ray.origin), Chamber.InverseTransformDirection(ray.direction));
            // Only the casing itself (above the floor band) accepts the tap; floor points around the machine stay floor commands.
            if (!new Bounds(TouchCentre, TouchHalfSize * 2).IntersectRay(local, out float distance) ||
                distance > nearestSolidDistance + .03f) return false;
            int selected = game.Motion.Selected;
            if (Phase != SplitPhase.Idle && member[selected]) return true;
            if (!game.PrepareTapCommand(selected)) return true;
            game.Motion.Move(selected, EntryPoint, true);
            requested = selected; requestOrder = game.Motion.Get(selected);
            game.Feedback.ShowCommand(EntryPoint, Chamber.up, Chamber);
            return true;
        }

        private void Message(string text) { LastMessage = text; messageUntil = owner.Matter.SimulationTime + 2.5f; }

        public override void StepMechanism(VenomCampaign game, float dt)
        {
            owner = game;
            var matter = game.Matter;
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++) if (matter.Escaped[i] || !Inside(i)) armed[i] = true;
            if (game.Owner.Lost || game.Home) { if (Phase != SplitPhase.Idle) Release(false); return; }
            clock += dt;
            DriveGates(game, Phase == SplitPhase.Sealing || Phase == SplitPhase.Clearing);
            if (Phase == SplitPhase.Idle) { StepIdle(game); Show(); return; }
            // A new command before the topology changes withdraws the fragment; afterwards Q finishes delivering both lobes.
            if ((Phase == SplitPhase.Gathering || Phase == SplitPhase.Separating) && game.Motion.Get(anchor) != null)
            { Release(false); Message("Q đã dừng — phần này rời máy"); Show(); return; }
            if (Phase == SplitPhase.Gathering)
            {
                Vector3 centre = Vector3.zero; for (int i = 0; i < 32; i++) if (member[i]) centre += matter.Bodies[i].position; centre /= count;
                for (int i = 0; i < 32; i++) if (member[i]) Pull(i, Chamber.TransformPoint(GatherLocal), dt);
                if (clock > .45f && Vector3.Distance(centre, Chamber.TransformPoint(GatherLocal)) < .018f) BeginSeparation();
                else if (clock > 5) { Release(false); Message("Q không gom được toàn thân — thử lại"); }
            }
            else if (Phase == SplitPhase.Separating || Phase == SplitPhase.Sealing)
            {
                bool apart = true;
                for (int i = 0; i < 32; i++)
                {
                    if (!member[i]) continue;
                    Pull(i, Chamber.TransformPoint(GatherLocal + Vector3.right * (left[i] ? -LobeOffset : LobeOffset)), dt);
                    float x = Chamber.InverseTransformPoint(matter.Bodies[i].position).x;
                    if (left[i] ? x > -Gap : x < Gap) apart = false;
                }
                if (Phase == SplitPhase.Separating)
                {
                    if (apart && clock > .3f) { Phase = SplitPhase.Sealing; clock = 0; Septum.Locked = false; }
                    else if (clock > 6) { Release(false); Message("Q chưa tách được hai thuỳ — thử lại"); }
                }
                else
                {
                    DriveSeptum(1);
                    if (!apart && Septum.Position < Septum.Travel * .5f) { Phase = SplitPhase.Separating; clock = 0; DriveSeptum(-1); }
                    else if (Septum.AtEnd) Split(game);
                    else if (clock > 4) { Release(false); Message("Vách phân luồng bị cản — thử lại"); }
                }
            }
            else if (Phase == SplitPhase.Clearing)
            {
                DriveSeptum(1);
                bool delivered = true;
                for (int side = 0; side < 2; side++)
                {
                    var path = side == 0 ? LeftLane : RightLane; var tray = side == 0 ? LeftTray : RightTray;
                    Vector3 centre = Vector3.zero; int n = 0;
                    for (int i = 0; i < 32; i++) if (member[i] && left[i] == (side == 0)) { centre += matter.Bodies[i].position; n++; }
                    centre /= Mathf.Max(1, n);
                    Vector3 target = lane[side] < path.Length ? Chamber.TransformPoint(path[lane[side]]) : tray.position + tray.up * .025f;
                    if (lane[side] < path.Length && Vector3.Distance(centre, target) < .03f) lane[side]++;
                    for (int i = 0; i < 32; i++)
                    {
                        if (!member[i] || left[i] != (side == 0)) continue;
                        Pull(i, target, dt);
                        if (!OnTray(tray, i)) delivered = false;
                    }
                }
                if (delivered && clock > .4f || clock > 9) Release(true);
            }
            Show();
        }

        private void StepIdle(VenomCampaign game)
        {
            var matter = game.Matter;
            if (requested >= 0 && !ReferenceEquals(game.Motion.Get(requested), requestOrder)) { requested = -1; requestOrder = null; }
            // The septum slides along its own plane, so withdrawing it can never pinch tissue: always withdraw.
            if (Septum != null && Septum.Position > Septum.CatchTolerance) { DriveSeptum(-1); return; }
            if (Septum != null) Septum.Locked = true;
            for (int i = 0; i < 32; i++)
            {
                if (matter.Escaped[i] || !armed[i] || !Inside(i) || requested < 0 || matter.Groups[i] != matter.Groups[requested]) continue;
                int group = matter.Groups[i], n = 0; bool whole = true;
                for (int j = 0; j < 32; j++)
                {
                    if (matter.Groups[j] != group || matter.Escaped[j]) continue;
                    n++; if (!armed[j] || !Inside(j)) { whole = false; break; }
                }
                if (!whole) continue;
                if (n < MinimumParticles || n % 2 != 0)
                {
                    for (int j = 0; j < 32; j++) if (matter.Groups[j] == group) armed[j] = false;
                    Message(n < MinimumParticles ? "Phần này đã nhỏ nhất — Q không tách thêm" : "Q chỉ chia được phần có khối lượng chẵn");
                    return;
                }
                for (int j = 0; j < 32; j++)
                    if (matter.Groups[j] != group && !matter.Escaped[j] && (OnTray(LeftTray, j) || OnTray(RightTray, j) || Inside(j)))
                    { Message("Khay ra đang có mô — dẫn phần đó rời khay"); return; }
                Begin(game, i, group, n);
                return;
            }
        }

        private void Begin(VenomCampaign game, int particle, int group, int n)
        {
            anchor = particle; count = n; clock = 0; lane[0] = lane[1] = 0;
            selectLeft = game.Matter.Groups[game.Motion.Selected] == group;
            for (int i = 0; i < 32; i++) { member[i] = game.Matter.Groups[i] == group && !game.Matter.Escaped[i]; left[i] = false; if (member[i]) armed[i] = false; }
            game.PrepareTapCommand(anchor);
            game.Motion.Cancel(anchor);
            requested = -1; requestOrder = null;
            Phase = SplitPhase.Gathering;
        }

        private void BeginSeparation()
        {
            // Rank the lobes by position across the septum plane; ties keep the stable particle index.
            int n = 0; for (int i = 0; i < 32; i++) if (member[i]) order[n++] = i;
            System.Array.Sort(order, 0, n, System.Collections.Generic.Comparer<int>.Create((a, b) =>
            {
                float xa = Chamber.InverseTransformPoint(owner.Matter.Bodies[a].position).x, xb = Chamber.InverseTransformPoint(owner.Matter.Bodies[b].position).x;
                return xa != xb ? xa.CompareTo(xb) : a.CompareTo(b);
            }));
            for (int k = 0; k < n; k++) left[order[k]] = k < n / 2;
            Phase = SplitPhase.Separating; clock = 0;
        }

        private void Split(VenomCampaign game)
        {
            var matter = game.Matter;
            matter.Partition(matter.Groups[anchor], left);
            int a = -1, b = -1, na = 0, nb = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!member[i]) continue;
                if (left[i]) { if (a < 0) a = i; na++; } else { if (b < 0) b = i; nb++; }
            }
            bool exact = a >= 0 && b >= 0 && na == nb && matter.Groups[a] != matter.Groups[b];
            for (int i = 0; i < 32 && exact; i++) if (member[i] && matter.Groups[i] != matter.Groups[left[i] ? a : b]) exact = false;
            if (!exact) Debug.LogError($"Q partition was not exact: {na}/{nb}");
            LastLeft = a; LastRight = b; Splits++;
            game.Motion.Cancel(a); game.Motion.Cancel(b);
            Phase = SplitPhase.Clearing; clock = 0; lane[0] = lane[1] = 0;
        }

        private void Release(bool delivered)
        {
            if (delivered && selectLeft && LastLeft >= 0) owner.SelectFragment(LastLeft);
            System.Array.Clear(member, 0, member.Length);
            Phase = SplitPhase.Idle; anchor = -1; clock = 0;
            owner.Motion.BuildGraph();
        }

        private void DriveGates(VenomCampaign game, bool open)
        {
            foreach (var gate in new[] { LeftGate, RightGate })
            {
                if (gate == null) continue;
                bool occupied = false;
                if (!open)
                    for (int i = 0; i < 32 && !occupied; i++)
                    {
                        if (game.Matter.Escaped[i]) continue;
                        Vector3 p = gate.Body.transform.InverseTransformPoint(game.Matter.Bodies[i].position);
                        // Anti-pinch: never lower a gate onto tissue standing in its lane.
                        occupied = Mathf.Abs(p.x) < .025f && Mathf.Abs(p.z) < .035f;
                    }
                bool raise = open || occupied && gate.Position > gate.CatchTolerance;
                gate.Locked = false;
                float target = raise ? gate.Travel : 0, speed = Vector3.Dot(gate.Body.linearVelocity, gate.WorldAxis);
                gate.ApplyEffort(gate.WorldAxis * Mathf.Clamp((target - gate.Position) * 10 - speed * 1.2f, -SeptumForce, SeptumForce));
            }
        }
        private void DriveSeptum(float direction)
        {
            if (Septum == null) return;
            Septum.Locked = false;
            float target = direction > 0 ? Septum.Travel : 0;
            float speed = Vector3.Dot(Septum.Body.linearVelocity, Septum.WorldAxis);
            Septum.ApplyEffort(Septum.WorldAxis * Mathf.Clamp((target - Septum.Position) * 10 - speed * 1.2f, -SeptumForce, SeptumForce));
        }

        // Finite, mass-proportional forces: the machine carries the tissue's weight while it draws it to a point
        // (as the creature's own feet do when it crawls). Contacts and bonds still act on every particle.
        private void Pull(int i, Vector3 target, float dt)
        {
            var body = owner.Matter.Bodies[i];
            Vector3 relative = body.linearVelocity - Chamber.GetComponentInParent<Rigidbody>().GetPointVelocity(body.position);
            Vector3 desired = Vector3.ClampMagnitude((target - body.position) * 5, .15f);
            Vector3 acceleration = Vector3.ClampMagnitude((desired - relative) * 25, Acceleration) + Vector3.up * 9.81f;
            body.AddForce(acceleration * body.mass);
        }

        private void Show()
        {
            bool active = Phase != SplitPhase.Idle;
            foreach (var lamp in Lamps)
                if (lamp != null && LampActive != null && LampIdle != null && lamp.sharedMaterial != (active ? LampActive : LampIdle))
                    lamp.sharedMaterial = active ? LampActive : LampIdle;
            if (ScanSheet != null) ScanSheet.gameObject.SetActive(Phase == SplitPhase.Separating || Phase == SplitPhase.Sealing);
        }
    }
}
