using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Continuous, supported inputs drive one real output. Only its settled end stop retains progress.</summary>
    public sealed class COgheCooperativeDrive : COgheMechanism
    {
        public enum MechanismKind { TwinPull, BrakeBridge, PairedValves }
        public MechanismKind Kind;
        public COgheTapRail A, B;
        public COgheRailSlider Output, Selector, FinalCover;
        public bool SelectorEnd, FinalGate;
        public VenomSurfacePatch Aperture;
        public Vector3 OutputSize;
        public float Force = .15f, Speed = .065f;
        public Transform CatchPin, Balance, Brake;
        public Renderer LampA, LampB;
        public Material Waiting, Ready;
        public LineRenderer BranchA, BranchB;
        public Transform Junction;
        public Transform[] PistonRods = System.Array.Empty<Transform>();
        public Transform[] Pulleys = System.Array.Empty<Transform>();
        public bool Caught { get; private set; }
        public bool Obstructed { get; private set; }
        public float OverlapSeconds { get; private set; }
        public bool Selected => Selector == null || (SelectorEnd ? Selector.AtEnd : Selector.Position <= Selector.CatchTolerance);
        public bool BothActive => Selected && A != null && A.Holding && B != null &&
            (Kind == MechanismKind.BrakeBridge ? B.Phase == COgheTapRail.TaskPhase.Operating : B.Holding);
        public override bool ControlsExit => FinalGate;
        public override bool ExitUnlocked => !FinalGate || Caught && Output.AtEnd && (FinalCover == null || FinalCover.AtEnd);
        public override string Activity => Caught ? "Chốt đã gài" : Obstructed ? "Đang chờ vùng hạ trống" : null;
        private float stable;
        private Vector3 catchRest, brakeRest;
        private Quaternion balanceRest;
        private AudioSource click;
        private bool apertureOpen;
        private Vector3[] rodPositions;

        public override void InitializeMechanism(VenomCampaign game)
        {
            if (CatchPin != null) catchRest = CatchPin.localPosition;
            if (Brake != null) brakeRest = Brake.localPosition;
            if (Balance != null) balanceRest = Balance.localRotation;
            click = GetComponent<AudioSource>();
            rodPositions = new Vector3[PistonRods.Length];
            for (int i = 0; i < PistonRods.Length; i++) rodPositions[i] = PistonRods[i].localPosition;
        }
        public override void ResetMechanism(VenomCampaign game)
        {
            Caught = Obstructed = false; stable = OverlapSeconds = 0;
            apertureOpen = false;
            if (click != null) click.Stop();
            Output.Locked = Kind == MechanismKind.BrakeBridge;
            if (FinalCover != null) FinalCover.Locked = true;
            if (Aperture != null) Aperture.NavigationHoleBlocked = true;
            Show();
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            bool active = BothActive;
            float speed = Vector3.Dot(Output.Body.linearVelocity - Output.Frame.GetComponent<Rigidbody>().GetPointVelocity(Output.Body.position), Output.WorldAxis);
            if (active && !Caught) OverlapSeconds += dt;
            stable = active && Output.AtEnd && Mathf.Abs(speed) < .025f ? stable + dt : 0;
            if (!Caught && stable >= .075f)
            {
                Caught = true; Output.Locked = true;
                A.CompleteHold(); B.CompleteHold();
                game.Motion.BuildGraph();
                if (click != null && click.clip != null) click.Play();
            }
            Obstructed = !Caught && !active && Kind != MechanismKind.BrakeBridge && TissueInReturnSweep(game);
            Output.Locked = Caught || Obstructed || Kind == MechanismKind.BrakeBridge && !active;
            if (!Output.Locked && Kind != MechanismKind.BrakeBridge)
            {
                float target = active ? Output.Travel : 0;
                float desired = Mathf.Clamp((target - Output.Position) * 3, -Speed, Speed);
                // Two engaged spring inputs feed a finite geared drive; neither can store pressure for the other.
                float available = active ? Mathf.Min(Force, (A.AppliedEffort + B.AppliedEffort) * 3) : Force;
                Output.ApplyEffort(Output.WorldAxis * Mathf.Clamp((desired - speed) * .8f + Mathf.Sign(desired) * Output.Resistance, -available, available));
            }
            if (FinalCover != null)
            {
                FinalCover.Locked = !Caught || FinalCover.AtEnd;
                if (!FinalCover.Locked)
                {
                    float coverSpeed = Vector3.Dot(FinalCover.Body.linearVelocity, FinalCover.WorldAxis);
                    FinalCover.ApplyEffort(FinalCover.WorldAxis * Mathf.Clamp((FinalCover.Travel - FinalCover.Position) * 5 - coverSpeed, -.2f, .2f));
                }
            }
            bool open = Caught && Output.AtEnd && (FinalCover == null || FinalCover.AtEnd);
            if (Aperture != null) Aperture.NavigationHoleBlocked = !open;
            if (open != apertureOpen) { apertureOpen = open; game.Motion.BuildGraph(); }
            Show();
        }
        private bool TissueInReturnSweep(VenomCampaign game)
        {
            if (Output.Position < .002f) return false;
            Vector3 axis = Output.Axis.normalized;
            Vector3 displacement = axis * Output.Position;
            Vector3 centre = Output.Start + displacement * .5f;
            Vector3 size = OutputSize + new Vector3(Mathf.Abs(displacement.x), Mathf.Abs(displacement.y), Mathf.Abs(displacement.z));
            var bounds = new Bounds(centre, size + Vector3.one * (game.Matter.Profile.ParticleRadius * 2 + .010f));
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                if (!game.Matter.Escaped[i] && bounds.Contains(Output.Frame.InverseTransformPoint(game.Matter.Bodies[i].position))) return true;
            return false;
        }
        private void Show()
        {
            if (CatchPin != null) CatchPin.localPosition = catchRest + Vector3.right * (Caught ? .024f : 0);
            if (Brake != null) Brake.localPosition = brakeRest + Vector3.up * (A.Holding || Caught ? .018f : 0);
            if (Balance != null) Balance.localRotation = balanceRest * Quaternion.Euler(0, 0, (A.Rail.Fraction - B.Rail.Fraction) * 22);
            for (int i = 0; i < PistonRods.Length; i++)
            {
                PistonRods[i].localPosition = rodPositions[i] + Vector3.up * Output.Position * .5f;
                PistonRods[i].localScale = new Vector3(.010f, .013f + Output.Position, .010f);
            }
            Lamp(LampA, A.Holding); Lamp(LampB, Kind == MechanismKind.BrakeBridge ? B.Phase == COgheTapRail.TaskPhase.Operating && A.Holding : B.Holding);
            Branch(BranchA, A, A.Holding); Branch(BranchB, B, Kind == MechanismKind.BrakeBridge ? BothActive : B.Holding);
        }
        private void Lamp(Renderer lamp, bool active)
        { if (lamp != null) lamp.sharedMaterial = active ? Ready : Waiting; }
        private void Branch(LineRenderer line, COgheTapRail input, bool active)
        {
            if (line == null || Junction == null) return;
            Vector3 start = input.HandPoint, end = Junction.position;
            line.SetPosition(0, start);
            if (Pulleys.Length == 2)
            {
                Vector3 pulley = Pulleys[input == A ? 0 : 1].position;
                line.SetPosition(1, pulley); line.SetPosition(2, (pulley + end) * .5f + Vector3.down * (active ? 0 : .015f)); line.SetPosition(3, end);
            }
            else { line.SetPosition(1, (start + end) * .5f + Vector3.down * (active || Kind == MechanismKind.PairedValves ? 0 : .025f)); line.SetPosition(2, end); }
        }
    }
}
