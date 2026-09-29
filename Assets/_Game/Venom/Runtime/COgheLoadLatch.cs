using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// Every input (a measured pad load or a held handle) must be active together to drive one output under finite
    /// force. A pawl catches the output only after it rests at the end of travel; from then on the holders may leave.
    /// Losing an input before the catch lets the output return. No fragment identity or level number is read.
    /// </summary>
    public sealed class COgheLoadLatch : COgheMechanism
    {
        public COgheTissueSensor[] Inputs = System.Array.Empty<COgheTissueSensor>();
        public COgheTapRail[] Holds = System.Array.Empty<COgheTapRail>();
        public COgheRailSlider[] Rails = System.Array.Empty<COgheRailSlider>();
        public COgheRailSlider Output;
        public Transform[] Pins = System.Array.Empty<Transform>();
        public Transform Pawl;
        public float Force = .45f;
        public bool Final;
        // Any: one active input suffices (OR). Retain=false: no pawl, the output follows its inputs (a held door).
        public bool Any, Retain = true;
        public COgheTissueClearance Clearance;
        public VenomSurfacePatch Aperture;
        public bool Caught { get; private set; }
        public override bool ControlsExit => Final;
        public override bool ExitUnlocked => Caught && Output.AtEnd;
        public override string Activity => Clearance != null && Clearance.Blocked && !Powered && Output.Position > Output.CatchTolerance ? "Có mô ở cửa — cửa chờ mở" :
            !Any && !Caught && ActiveCount > 0 && !AllActive ? $"Khoá · {ActiveCount}/{InputCount} có tải" : null;
        public int InputCount => Inputs.Length + Holds.Length + Rails.Length;
        public int ActiveCount
        {
            get
            {
                int n = 0;
                foreach (var s in Inputs) if (s != null && s.Active) n++;
                foreach (var h in Holds) if (h != null && h.Holding) n++;
                foreach (var r in Rails) if (r != null && r.AtEnd) n++;
                return n;
            }
        }
        public bool AllActive => ActiveCount == InputCount;
        public bool Powered => Any ? ActiveCount > 0 : AllActive;
        private float stable;
        private Vector3[] pinRest;
        private Vector3 pawlRest;
        public override void InitializeMechanism(VenomCampaign game)
        {
            pinRest = new Vector3[Pins.Length];
            for (int i = 0; i < Pins.Length; i++) if (Pins[i] != null) pinRest[i] = Pins[i].localPosition;
            if (Pawl != null) pawlRest = Pawl.localPosition;
        }
        public override void ResetMechanism(VenomCampaign game)
        {
            Caught = false; stable = 0; Output.Locked = false; Output.ReleaseLatch(); Show(game, true);
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            bool all = Powered;
            float speed = Vector3.Dot(Output.Body.linearVelocity - Output.Frame.GetComponent<Rigidbody>().GetPointVelocity(Output.Body.position), Output.WorldAxis);
            // Anti-pinch: while tissue is in the doorway the output never closes on it; it waits open.
            bool holdOpen = Clearance != null && Clearance.Blocked && Output.Position > Output.CatchTolerance;
            if (!Caught)
            {
                float target = all || holdOpen ? Output.Travel : 0;
                Output.ApplyEffort(Output.WorldAxis * Mathf.Clamp((target - Output.Position) * 6 - speed * .9f, -Force, Force));
                stable = Retain && all && Output.AtEnd && Mathf.Abs(speed) < .02f ? stable + dt : 0;
                if (stable >= .1f) { Caught = true; Output.Locked = true; if (game.Motion != null) game.Motion.BuildGraph(true); }
            }
            Show(game, false);
        }
        private bool open;
        private void Show(VenomCampaign game, bool force)
        {
            for (int i = 0; i < Pins.Length && i < pinRest.Length; i++)
            {
                if (Pins[i] == null) continue;
                int h = i - Inputs.Length, r = h - Holds.Length;
                bool active = Caught || (i < Inputs.Length ? Inputs[i] != null && Inputs[i].Active : h < Holds.Length ? Holds[h].Holding : r < Rails.Length && Rails[r].AtEnd);
                Pins[i].localPosition = pinRest[i] + (active ? Vector3.up * .012f : Vector3.zero);
            }
            if (Pawl != null) Pawl.localPosition = pawlRest + (Caught ? Vector3.down * .006f : Vector3.zero);
            bool next = ExitUnlocked;
            if (Aperture != null) Aperture.NavigationHoleBlocked = Final && !next;
            if ((next != open || force) && game != null && game.Motion != null) { open = next; if (!force) game.Motion.BuildGraph(); }
        }
    }
}
