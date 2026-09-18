using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>One gear carriage powers two visible end stations in sequence.</summary>
    public sealed class COgheDualDockTransmission : COgheMechanism
    {
        public COgheRailSlider Carriage, AccessGate, ExitGate;
        public Transform CarriageWheel;
        public Transform[] StationAWheels, StationBWheels;
        public float GateSpeed = .055f, DriveForce = .8f, DockTolerance = .003f, WheelSpeed = 95f;
        public float PitchRadius=.045f,MeshTolerance=.002f;
        public int ToothCount=18;
        private Quaternion carriageRest;
        private Quaternion[] stationARest,stationBRest;
        public bool StageAComplete { get; private set; }
        public bool StageBComplete { get; private set; }
        public bool AtA => Carriage != null && Carriage.Position <= DockTolerance && StationContact(StationAWheels);
        public bool AtB => Carriage != null && Carriage.Position >= Carriage.Travel - DockTolerance && StationContact(StationBWheels);
        public override bool ControlsExit => true;
        public override bool ExitUnlocked => StageAComplete && StageBComplete;
        public override string Activity => AtA && !StageAComplete ? "Trạm A đang mở đường" : AtB && StageAComplete && !StageBComplete ? "Trạm B đang mở cửa" : null;

        public override void InitializeMechanism(VenomCampaign game)
        {
            carriageRest=CarriageWheel.localRotation;
            stationARest=Capture(StationAWheels);stationBRest=Capture(StationBWheels);
        }
        private static Quaternion[] Capture(Transform[] wheels)
        {
            var result=new Quaternion[wheels.Length];
            for(int i=0;i<wheels.Length;i++)result[i]=wheels[i].localRotation;
            return result;
        }
        private static void Restore(Transform[] wheels,Quaternion[] rotations)
        {for(int i=0;i<wheels.Length;i++)wheels[i].localRotation=rotations[i];}
        public override void ResetMechanism(VenomCampaign game)
        {
            if(stationARest==null)InitializeMechanism(game);
            CarriageWheel.localRotation=carriageRest;
            Restore(StationAWheels,stationARest);Restore(StationBWheels,stationBRest);
            StageAComplete = StageBComplete = false;
            if (AccessGate != null) AccessGate.Locked = true;
            if (ExitGate != null) ExitGate.Locked = true;
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            if (Carriage == null || AccessGate == null || ExitGate == null) return;
            bool drivingA = AtA && !StageAComplete;
            bool drivingB = StageAComplete && AtB && !StageBComplete;
            StageAComplete = Drive(AccessGate, drivingA, StageAComplete);
            StageBComplete = Drive(ExitGate, drivingB, StageBComplete);
            var load=drivingA?AccessGate:drivingB?ExitGate:null;
            // A blocked rack stalls its train. Rotation follows measured load
            // travel; it cannot visually keep turning through a jammed gate.
            float spin=load!=null?Mathf.Clamp(Vector3.Dot(load.Body.linearVelocity,load.WorldAxis)/PitchRadius*Mathf.Rad2Deg,-WheelSpeed,WheelSpeed)*dt:0;
            if (CarriageWheel != null) CarriageWheel.Rotate(Vector3.forward, -spin, Space.Self);
            MeshStation(StationAWheels);MeshStation(StationBWheels);
        }
        private bool StationContact(Transform[] wheels)
        {
            if(CarriageWheel==null||wheels==null||wheels.Length!=2)return false;
            foreach(var wheel in wheels)
                if(wheel==null||Vector3.Dot(wheel.forward,CarriageWheel.forward)<.999f||
                    !COgheGearTrain.PitchContact(CarriageWheel.position,wheel.position,CarriageWheel.forward,PitchRadius,PitchRadius,MeshTolerance))return false;
            return true;
        }
        private void MeshStation(Transform[] wheels)
        {
            if(CarriageWheel==null||wheels==null)return;
            float toothPitch=360f/ToothCount,tipDistance=PitchRadius*2+4*PitchRadius/ToothCount;
            foreach(var wheel in wheels)
            {
                if(wheel==null)continue;
                Vector3 line=CarriageWheel.InverseTransformPoint(wheel.position);
                if(Mathf.Abs(line.z)>MeshTolerance||line.sqrMagnitude>tipDistance*tipDistance)continue;
                // Both fixed wheels mesh directly with G, so both rotate in
                // the opposite direction. Keep the free bearing within half
                // a tooth of the correct phase as G slides into engagement.
                float phi=Mathf.Atan2(line.y,line.x)*Mathf.Rad2Deg;
                float current=(Quaternion.Inverse(CarriageWheel.rotation)*wheel.rotation).eulerAngles.z;
                float ideal=2*phi+180-toothPitch*.5f;
                wheel.Rotate(Vector3.forward,Mathf.Repeat(ideal-current+toothPitch*.5f,toothPitch)-toothPitch*.5f,Space.Self);
            }
        }
        private bool Drive(COgheRailSlider gate, bool powered, bool complete)
        {
            if (complete) { gate.Locked = true; return true; }
            gate.Locked = !powered || gate.AtEnd;
            if (powered && !gate.AtEnd)
            {
                float speed = Vector3.Dot(gate.Body.linearVelocity, gate.WorldAxis);
                // A counterbalanced rail already cancels world gravity. Adding
                // compensation again accelerates the load beyond motor speed.
                float gravity = gate.Gravity?Mathf.Max(0, -Vector3.Dot(Vector3.down * 9.81f * gate.Body.mass, gate.WorldAxis)):0;
                gate.ApplyEffort(gate.WorldAxis * Mathf.Clamp((GateSpeed - speed) * 8 + gravity + gate.Resistance, 0, DriveForce));
            }
            return gate.AtEnd;
        }
    }
}
