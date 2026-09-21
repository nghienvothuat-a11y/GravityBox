using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>A held source and one travelling gear first open access, then power a manually operated winch.</summary>
    public sealed class COgheCooperativeDockTransmission : COgheMechanism
    {
        public COgheTissueSensor Input;
        public COgheRailSlider Carriage, AccessDoor, Handle, ExitDoor;
        public Transform CarriageWheel;
        public Transform[] StationIWheels, StationIIWheels;
        public float PitchRadius = .04f, MeshTolerance = .003f;
        public float Speed = .09f, MaxForce = .8f, HandleForce = .05f;
        public bool AccessCaught { get; private set; }
        public bool Complete { get; private set; }
        // Radial pitch tolerance alone accepts a broad approach interval when
        // the carriage travels perpendicular to the wheel centres' line. The
        // bearing must also reach its actual dock before transferring power.
        public bool AtI => Carriage != null && Carriage.Position <= Carriage.CatchTolerance && Contact(StationIWheels);
        public bool AtII => Carriage != null && Carriage.AtEnd && Contact(StationIIWheels);
        public bool Engaged { get; private set; }
        public override bool ControlsExit => true;
        public override bool ExitUnlocked => Complete;
        private Quaternion wheelRest;
        private Quaternion[] restI, restII;

        public override void InitializeMechanism(VenomCampaign game)
        {
            wheelRest = CarriageWheel.localRotation;
            restI = Capture(StationIWheels); restII = Capture(StationIIWheels);
        }
        private static Quaternion[] Capture(Transform[] wheels)
        {
            var result = new Quaternion[wheels.Length];
            for (int i = 0; i < wheels.Length; i++) result[i] = wheels[i].localRotation;
            return result;
        }
        public override void ResetMechanism(VenomCampaign game)
        {
            if (restI == null) InitializeMechanism(game);
            CarriageWheel.localRotation = wheelRest;
            for (int i = 0; i < restI.Length; i++) StationIWheels[i].localRotation = restI[i];
            for (int i = 0; i < restII.Length; i++) StationIIWheels[i].localRotation = restII[i];
            AccessCaught = Complete = Engaged = false;
            AccessDoor.Locked = Handle.Locked = ExitDoor.Locked = true;
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            bool powered = Input != null && Input.Active;
            bool first = powered && AtI && !AccessCaught;
            AccessCaught = Drive(AccessDoor, first, AccessCaught);
            bool second = powered && AtII;
            Handle.Locked = Complete || !second;
            Engaged = !Complete && second && Handle.Position > .008f && Handle.Effort >= HandleForce;
            Complete = Drive(ExitDoor, Engaged, Complete);
            var load = first ? AccessDoor : Engaged ? ExitDoor : null;
            float angle = load == null ? 0 : Vector3.Dot(load.Body.linearVelocity, load.WorldAxis) / PitchRadius * Mathf.Rad2Deg * dt;
            CarriageWheel.Rotate(Vector3.forward, -angle, Space.Self);
            Mesh(StationIWheels); Mesh(StationIIWheels);
        }
        private bool Contact(Transform[] wheels)
        {
            if (CarriageWheel == null || wheels == null || wheels.Length != 2) return false;
            foreach (var wheel in wheels)
                if (wheel == null || Vector3.Dot(wheel.forward, CarriageWheel.forward) < .999f || !COgheGearTrain.PitchContact(CarriageWheel.position, wheel.position,
                    CarriageWheel.forward, PitchRadius, PitchRadius, MeshTolerance)) return false;
            return true;
        }
        private bool Drive(COgheRailSlider door, bool powered, bool caught)
        {
            caught |= door.AtEnd;
            door.Locked = caught || !powered;
            if (!door.Locked)
            {
                float speed = Vector3.Dot(door.Body.linearVelocity, door.WorldAxis);
                float gravity = door.Gravity ? -Vector3.Dot(Vector3.down * (door.Body.mass * 9.81f), door.WorldAxis) : 0;
                door.ApplyEffort(door.WorldAxis * Mathf.Clamp((Speed - speed) * 8 + gravity + door.Resistance, 0, MaxForce));
            }
            return caught;
        }
        private void Mesh(Transform[] wheels)
        {
            const float toothPitch = 20f; // authored equal 18-tooth wheels
            foreach (var wheel in wheels)
            {
                Vector3 line = CarriageWheel.InverseTransformPoint(wheel.position);
                if (Mathf.Abs(line.z) > MeshTolerance || line.magnitude > PitchRadius * 2.23f) continue;
                float phi = Mathf.Atan2(line.y, line.x) * Mathf.Rad2Deg;
                float current = (Quaternion.Inverse(CarriageWheel.rotation) * wheel.rotation).eulerAngles.z;
                float ideal = 2 * phi + 180 - toothPitch * .5f;
                wheel.Rotate(Vector3.forward, Mathf.Repeat(ideal - current + toothPitch * .5f, toothPitch) - toothPitch * .5f, Space.Self);
            }
        }
    }
}
