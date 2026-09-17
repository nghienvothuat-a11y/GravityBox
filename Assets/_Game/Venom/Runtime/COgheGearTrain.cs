using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Ideal involute gear constraint with finite motor torque and measured pitch-circle contact.</summary>
    public sealed class COgheGearTrain : COgheMechanism
    {
        public Transform[] Wheels;
        public float[] PitchRadii;
        public int[] ToothCounts;
        public COgheRailSlider Rack;
        public COgheTissueSensor InputClutch;
        public float MeshTolerance = .002f, MotorSpeed = 1.25f, MotorTorque = .04f;
        public bool HasOutputLoad;
        public float OutputSpeed;
        public bool Meshed { get; private set; }
        public bool Powered => Meshed && (InputClutch == null || InputClutch.Active);
        public float[] AngularSpeeds { get; private set; }
        public override bool ControlsExit => Rack != null;
        public override bool ExitUnlocked => Rack == null || Rack.AtEnd;
        private Quaternion[] initialRotations;
        public override void InitializeMechanism(VenomCampaign game)
        {
            initialRotations = new Quaternion[Wheels.Length];
            for (int i = 0; i < Wheels.Length; i++) initialRotations[i] = Wheels[i].localRotation;
        }
        public override void ResetMechanism(VenomCampaign game)
        {
            if (initialRotations == null) InitializeMechanism(game);
            AngularSpeeds = new float[Wheels.Length]; Meshed = false; HasOutputLoad = false; OutputSpeed = 0;
            for (int i = 0; i < Wheels.Length; i++) Wheels[i].localRotation = initialRotations[i];
        }
        public static bool PitchContact(Vector3 a, Vector3 b, Vector3 axle, float ra, float rb, float tolerance)
        {
            Vector3 delta = b - a;
            return Mathf.Abs(Vector3.Dot(delta, axle.normalized)) <= tolerance &&
                Mathf.Abs(Vector3.ProjectOnPlane(delta, axle).magnitude - ra - rb) <= tolerance;
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            if (AngularSpeeds == null) ResetMechanism(game);
            bool powered = InputClutch == null || InputClutch.Active;
            bool stopped = Rack != null && Rack.AtEnd;
            AngularSpeeds[0] = powered && !stopped ? -MotorSpeed : 0;
            Meshed = true;
            for (int i = 1; i < Wheels.Length; i++)
            {
                bool contact = PitchContact(Wheels[i - 1].position, Wheels[i].position, Wheels[0].forward,
                    PitchRadii[i - 1], PitchRadii[i], MeshTolerance);
                Meshed &= contact;
                AngularSpeeds[i] = contact ? -AngularSpeeds[i - 1] * PitchRadii[i - 1] / PitchRadii[i] : 0;
            }
            if (Rack != null)
            {
                Rack.Locked = !Powered || stopped;
                if (Powered && !stopped)
                {
                    float radius = PitchRadii[PitchRadii.Length - 1];
                    float speed = Vector3.Dot(Rack.Body.linearVelocity, Rack.WorldAxis);
                    float desired = Mathf.Abs(AngularSpeeds[AngularSpeeds.Length - 1]) * radius;
                    Rack.ApplyEffort(Rack.WorldAxis * Mathf.Clamp((desired - speed) * 8, 0, MotorTorque / radius));
                    // Loaded train phase follows the measured rack travel. A blocked rack stalls all connected wheels.
                    for (int i = Wheels.Length - 1; i >= 0; i--)
                        AngularSpeeds[i] = speed / PitchRadii[i] * ((Wheels.Length - 1 - i) % 2 == 0 ? 1 : -1);
                }
            }
            else if (HasOutputLoad && Powered)
            {
                int last = Wheels.Length - 1;
                for (int i = last; i >= 0; i--)
                    AngularSpeeds[i] = OutputSpeed * PitchRadii[last] / PitchRadii[i] * ((last - i) % 2 == 0 ? 1 : -1);
            }
            for (int i = 0; i < Wheels.Length; i++)
            {
                Wheels[i].Rotate(Vector3.forward, AngularSpeeds[i] * Mathf.Rad2Deg * dt, Space.Self);
                if (i == 0 || AngularSpeeds[i] == 0) continue;
                // Project the free bearing onto the ideal tooth-phase constraint when contact engages.
                // The correction is at most half one tooth pitch, never a carriage displacement.
                Quaternion basis = Wheels[0].parent.rotation;
                Vector3 line = Quaternion.Inverse(basis) * (Wheels[i].position - Wheels[i - 1].position);
                float phi = Mathf.Atan2(line.y, line.x) * Mathf.Rad2Deg;
                float previous = (Quaternion.Inverse(basis) * Wheels[i - 1].rotation).eulerAngles.z;
                float current = (Quaternion.Inverse(basis) * Wheels[i].rotation).eulerAngles.z;
                float ratio = PitchRadii[i - 1] / PitchRadii[i];
                int teeth = ToothCounts != null && ToothCounts.Length > i ? ToothCounts[i] : 20;
                float pitch = 360f / teeth;
                float ideal = (1 + ratio) * phi + 180 - previous * ratio - pitch * .5f;
                float correction = Mathf.Repeat(ideal - current + pitch * .5f, pitch) - pitch * .5f;
                Wheels[i].Rotate(Vector3.forward, correction, Space.Self);
            }
        }
    }
}
