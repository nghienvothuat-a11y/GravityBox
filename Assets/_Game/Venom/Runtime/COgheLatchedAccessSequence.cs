using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// Latches the two deliberate physical actions in level 13. The lever latch
    /// reads real hinge travel and the inlet latch reads measured tissue load.
    /// Powered shutters remain joint-constrained rigidbodies driven by forces.
    /// </summary>
    public sealed class COgheLatchedAccessSequence : COgheMechanism
    {
        public Rigidbody Lever;
        public HingeJoint LeverJoint;
        public VenomPressurePlate Button;
        public Rigidbody RoomDoor,TubeLid;
        public Vector3 RoomDoorRest,TubeLidRest;
        public Vector3 DoorAxis=Vector3.up,LidAxis=Vector3.up;
        public float DoorTravel=.145f,LidTravel=.105f,LeverLatchAngle=31f;
        public bool DoorLatched { get; private set; }
        public bool TubeLatched { get; private set; }
        public float DoorOpening { get; private set; }
        public float LidOpening { get; private set; }
        public override bool ControlsExit => true;
        public override bool ExitUnlocked => TubeLatched&&LidOpening>=LidTravel*.82f;
        public override string Activity => !DoorLatched ? "Kéo cần A" : !TubeLatched ? "Nhấn nút B" : "Ống đã mở";
        private Quaternion leverRest;
        private bool captured;

        public override void InitializeMechanism(VenomCampaign game)
        {leverRest=Quaternion.Inverse(game.Root.rotation)*Lever.rotation;captured=true;}

        public override void ResetMechanism(VenomCampaign game)
        {
            if(!captured)InitializeMechanism(game);
            DoorLatched=false;TubeLatched=false;DoorOpening=LidOpening=0;
            Button?.ResetPlate(game.Root);
        }

        public override void StepMechanism(VenomCampaign game,float dt)
        {
            if(!captured)InitializeMechanism(game);
            Quaternion leverNow=Quaternion.Inverse(game.Root.rotation)*Lever.rotation;
            if(!DoorLatched&&Quaternion.Angle(leverRest,leverNow)>=LeverLatchAngle)DoorLatched=true;
            if(DoorLatched&&Button!=null)
            {
                Button.Step(game.Matter,game.Root);
                if(Button.Pressed)TubeLatched=true;
            }
            Drive(game,RoomDoor,RoomDoorRest,DoorAxis,DoorLatched?DoorTravel:0,out float door);
            Drive(game,TubeLid,TubeLidRest,LidAxis,TubeLatched?LidTravel:0,out float lid);
            DoorOpening=door;LidOpening=lid;
        }

        private static void Drive(VenomCampaign game,Rigidbody body,Vector3 rest,Vector3 localAxis,float target,out float displacement)
        {
            Vector3 axis=game.Root.TransformDirection(localAxis.normalized);
            Vector3 restWorld=game.Root.TransformPoint(rest);
            displacement=Vector3.Dot(body.position-restWorld,axis);
            float relativeSpeed=Vector3.Dot(body.linearVelocity-game.Root.GetComponent<Rigidbody>().GetPointVelocity(body.position),axis);
            // Campaign gravity is applied explicitly to every prop. Counter
            // only its component along this powered rail; the joint still owns
            // every other degree of freedom and both physical end stops.
            float gravityAlong=Vector3.Dot(Vector3.down*9.81f,axis);
            float acceleration=Mathf.Clamp((target-displacement)*145f-relativeSpeed*19f-gravityAlong,-32f,32f);
            body.AddForce(axis*acceleration,ForceMode.Acceleration);
        }
    }
}
