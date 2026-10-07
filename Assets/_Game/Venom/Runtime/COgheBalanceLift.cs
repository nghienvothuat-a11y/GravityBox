using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// A balance lift (chapter 4, "Bập bênh nâng bạn"): two trays on one rope over a beam. The side carrying more tissue goes
    /// down and the other comes up; within Margin of each other they stay where they are (balanced: 25 with 25, 50 with 50).
    /// Both loads are weighed whole by column sensors, the rising one riding its tray. Only the rising tray moves; the beam
    /// and ropes are drawn from its travel.
    /// </summary>
    public sealed class COgheBalanceLift : COgheMechanism
    {
        public COgheRailSlider Rising;
        public COgheTissueSensor RisingLoad, CounterLoad;
        public Transform Beam;
        public float BeamTilt = 16f, Margin = .012f, Force = 1.5f;
        public LineRenderer Rope;
        public Transform CounterAnchor, RisingAnchor, CounterEnd, RisingEnd;
        public float Difference => CounterLoad.Load - RisingLoad.Load;
        public bool Balanced => Mathf.Abs(Difference) <= Margin && (CounterLoad.Load > 0 || RisingLoad.Load > 0);
        public override string Activity => Balanced ? "Hai bên cân nhau" : null;
        public float Speed = .08f;
        public bool Moving { get; private set; }
        private float integral, stable;
        public override void ResetMechanism(VenomCampaign game) { Moving = false; integral = stable = 0; Show(); }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            float diff = Difference;
            // Heavier counter: up; heavier rider: down; balanced: stay; both empty: back to rest (a tray that rose empty comes
            // down again once the counterweight steps off).
            float target = diff > Margin ? Rising.Travel : diff < -Margin ? 0 : CounterLoad.Load + RisingLoad.Load < Margin ? 0 : Rising.Position;
            float error = target - Rising.Position;
            if (!Moving && Mathf.Abs(error) > Rising.CatchTolerance * 2)
            {
                // Setting off: like the passenger lift, riders drop their walk orders (an order to the deck's old spot
                // held a quarter, and with it the tray, half way up).
                Moving = true; integral = stable = 0; Rising.Locked = false; Rising.ReleaseLatch();
                if (game != null && game.Matter != null && game.Motion != null)
                    for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                        if (!game.Matter.Escaped[i] && RisingLoad.Holds(game.Matter.Bodies[i].position)) game.Motion.Cancel(i);
            }
            if (!Moving) { Show(); return; }
            float speed = Vector3.Dot(Rising.Body.linearVelocity - Rising.Frame.GetComponent<Rigidbody>().GetPointVelocity(Rising.Body.position), Rising.WorldAxis);
            float desired = Mathf.Clamp(error * 4, -Speed, Speed);
            // The rider's weight is carried; a slow integral makes up what the estimate misses.
            integral = Mathf.Clamp(integral + (desired - speed) * 8 * dt, -1, 1);
            Rising.ApplyEffort(Rising.WorldAxis * Mathf.Clamp(RisingLoad.Load * Physics.gravity.magnitude + (desired - speed) + integral, -Force, Force));
            stable = Mathf.Abs(error) < Rising.CatchTolerance && Mathf.Abs(speed) < .02f ? stable + dt : 0;
            if (stable > .15f) { Moving = false; Rising.Locked = true; if (game != null && game.Motion != null) game.Motion.BuildGraph(true); }
            Show();
        }
        private void Show()
        {
            if (Rising == null || Rising.Travel <= 0) return;
            float t = Mathf.Clamp01(Rising.Position / Rising.Travel);
            if (Beam != null) Beam.localRotation = Quaternion.Euler(0, 0, BeamTilt * t);
            if (Rope != null && CounterAnchor != null && RisingAnchor != null && CounterEnd != null && RisingEnd != null)
            {
                Rope.positionCount = 4;
                Rope.SetPosition(0, CounterAnchor.position); Rope.SetPosition(1, CounterEnd.position);
                Rope.SetPosition(2, RisingEnd.position); Rope.SetPosition(3, RisingAnchor.position);
            }
        }
    }
}
