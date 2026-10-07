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
        public COgheRailSlider PowerRail;
        public bool ReturnWhenDisconnected;
        public bool LatchOutput;
        public float ReturnForce=.18f;
        public float MeshTolerance = .002f, MotorSpeed = 1.25f, MotorTorque = .04f;
        public bool HasOutputLoad;
        public float OutputSpeed;
        // Wheels[i] and Wheels[i+1] share one vertical shaft (a compound gear joining two layers) when i is listed here:
        // they turn together instead of meshing. Empty keeps every link a meshing pair, as in the older levels.
        public int[] ShaftLinks = System.Array.Empty<int>();
        // Further motor pads that must ALL be loaded too (a heavy output that needs two motors). Empty: InputClutch only.
        public COgheTissueSensor[] ExtraClutches = System.Array.Empty<COgheTissueSensor>();
        // False for a rack that is not the exit (a step, a lift, a bridge): the final exit then does not wait for it.
        public bool GatesExit = true;
        // Chapter 5: the rack runs the way the last wheel turns, toward its end when that is ForwardSign and back to its start
        // otherwise, so an idler (one more meshing pair) reverses it. Several reversible trains may share one rack: only a
        // meshed train drives it (two meshed at once push against each other: a jam). Off: the older one-way racks.
        public bool Reversible;
        public float ForwardSign = 1;
        // Chapter 5 ("Ai chạy máy"): above zero, the motor's pull at the rack follows the weight on its pads (N per kg of tissue,
        // summed over every clutch: two motors add), and RackSpring (N/m) holds the rack back toward its start, so a light
        // driver lifts a load only part of the way.
        public float ForcePerLoad, RackSpring;
        // A constant pull back to the start on top of RackSpring (N): a light driver does not lift the load at all and a
        // middling one stalls part way ("Hai động cơ một cửa"). Zero: the spring alone.
        public float RackPreload;
        // A lamp that lights while every link of the train meshes (chapter 5: "đèn khớp"). Null: no lamp.
        public Renderer MeshLamp;
        public Material MeshLampOn, MeshLampOff;
        // Several trains through the same wheels (the boss gearbox: two outputs, each through the straight gear or the idler;
        // "Hai tầng trục": a lift and the upper layer): where SharedWheels[i] is set, Wheels[i] is this train's own hidden copy
        // of that visible wheel, and the visible wheel follows the train that is meshed and turning. Empty: Wheels are drawn.
        public Transform[] SharedWheels = System.Array.Empty<Transform>();
        private Quaternion[] sharedRest;
        private Transform Shared(int i) => SharedWheels != null && i < SharedWheels.Length ? SharedWheels[i] : null;
        public bool Meshed { get; private set; }
        public bool ClutchesActive
        {
            get
            {
                if (InputClutch != null && !InputClutch.Active) return false;
                if (ExtraClutches != null) foreach (var c in ExtraClutches) if (c != null && !c.Active) return false;
                return true;
            }
        }
        public bool Powered => Meshed && ClutchesActive && (PowerRail == null || PowerRail.AtEnd);
        public float[] AngularSpeeds { get; private set; }
        public override bool ControlsExit => Rack != null && GatesExit;
        public override bool ExitUnlocked => Rack == null || Rack.AtEnd;
        private Quaternion[] initialRotations;
        private bool outputLatched;
        public override void InitializeMechanism(VenomCampaign game)
        {
            initialRotations = new Quaternion[Wheels.Length];
            for (int i = 0; i < Wheels.Length; i++) initialRotations[i] = Wheels[i].localRotation;
            sharedRest = new Quaternion[Wheels.Length];
            for (int i = 0; i < Wheels.Length; i++) if (Shared(i) != null) sharedRest[i] = Shared(i).localRotation;
        }
        public override void ResetMechanism(VenomCampaign game)
        {
            if (initialRotations == null) InitializeMechanism(game);
            AngularSpeeds = new float[Wheels.Length]; Meshed = false; HasOutputLoad = false; OutputSpeed = 0;
            outputLatched=false;
            for (int i = 0; i < Wheels.Length; i++) Wheels[i].localRotation = initialRotations[i];
            for (int i = 0; i < Wheels.Length; i++) if (Shared(i) != null) Shared(i).localRotation = sharedRest[i];
            ShowMeshLamp();
        }
        private void ShowMeshLamp()
        {
            if (MeshLamp == null) return;
            var m = Meshed ? MeshLampOn : MeshLampOff;
            if (m != null && MeshLamp.sharedMaterial != m) MeshLamp.sharedMaterial = m;
        }
        public static bool PitchContact(Vector3 a, Vector3 b, Vector3 axle, float ra, float rb, float tolerance)
        {
            Vector3 delta = b - a;
            return Mathf.Abs(Vector3.Dot(delta, axle.normalized)) <= tolerance &&
                Mathf.Abs(Vector3.ProjectOnPlane(delta, axle).magnitude - ra - rb) <= tolerance;
        }
        // Link i joins Wheels[i-1] and Wheels[i]: a shared shaft (coaxial, same speed) or a meshing pair (pitch contact).
        private bool Shaft(int i) => ShaftLinks != null && System.Array.IndexOf(ShaftLinks, i - 1) >= 0;
        private bool Linked(int i) => Shaft(i)
            ? Vector3.ProjectOnPlane(Wheels[i].position - Wheels[i - 1].position, Wheels[0].forward).magnitude <= MeshTolerance
            : PitchContact(Wheels[i - 1].position, Wheels[i].position, Wheels[0].forward, PitchRadii[i - 1], PitchRadii[i], MeshTolerance);
        // Angular speed of wheel i per unit speed of wheel i-1 across link i.
        private float Ratio(int i) => Shaft(i) ? 1 : -PitchRadii[i - 1] / PitchRadii[i];
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            if (AngularSpeeds == null) ResetMechanism(game);
            for (int i = 0; i < Wheels.Length; i++) if (Shared(i) != null) Wheels[i].localRotation = Shared(i).localRotation;
            bool powered = ClutchesActive && (PowerRail == null || PowerRail.AtEnd);
            float direction = 1;
            if (Reversible) { direction = -ForwardSign; for (int i = 1; i < Wheels.Length; i++) direction *= Mathf.Sign(Ratio(i)); }
            bool stopped = Rack != null && (direction > 0 ? Rack.AtEnd : Rack.Position <= Rack.CatchTolerance);
            AngularSpeeds[0] = powered && !stopped ? -MotorSpeed : 0;
            Meshed = true;
            for (int i = 1; i < Wheels.Length; i++)
            {
                bool contact = Linked(i);
                Meshed &= contact;
                AngularSpeeds[i] = contact ? AngularSpeeds[i - 1] * Ratio(i) : 0;
            }
            ShowMeshLamp();
            if (Rack != null && (!Reversible || Meshed))
            {
                if(LatchOutput&&Rack.AtEnd)outputLatched=true;
                Rack.Locked = !Powered || stopped;
                if(ReturnWhenDisconnected&&!Powered&&!outputLatched)
                {
                    Rack.Locked=false;Rack.ApplyEffort(-Rack.WorldAxis*ReturnForce);
                    // The return spring back-drives the output bearing and only the wheels still touching it.
                    int last=Wheels.Length-1;
                    AngularSpeeds[last]=Vector3.Dot(Rack.Body.linearVelocity,Rack.WorldAxis)/PitchRadii[last];
                    for(int i=last-1;i>=0;i--)
                    {
                        if(!Linked(i+1))break;
                        AngularSpeeds[i]=AngularSpeeds[i+1]/Ratio(i+1);
                    }
                }
                if(outputLatched)Rack.Locked=true;
                if((RackSpring>0||RackPreload>0)&&!Rack.Locked)Rack.ApplyEffort(-Rack.WorldAxis*(RackSpring*Rack.Position+RackPreload));
                if (Powered && !stopped)
                {
                    float radius = PitchRadii[PitchRadii.Length - 1];
                    float speed = Vector3.Dot(Rack.Body.linearVelocity, Rack.WorldAxis);
                    float desired = Mathf.Abs(AngularSpeeds[AngularSpeeds.Length - 1]) * radius * direction;
                    float cap = MotorTorque / radius;
                    if (ForcePerLoad > 0)
                    {
                        float load = InputClutch != null ? InputClutch.Load : 0;
                        foreach (var extra in ExtraClutches) if (extra != null) load += extra.Load;
                        cap = ForcePerLoad * load;
                    }
                    // A load-driven motor reaches its full pull at a stall (the usual gain of 8 alone capped it near 1 N).
                    float gain = ForcePerLoad > 0 ? 40 : 8;
                    Rack.ApplyEffort(Rack.WorldAxis * Mathf.Clamp((desired - speed) * gain, direction > 0 ? 0 : -cap, direction > 0 ? cap : 0));
                    // Loaded train phase follows the measured rack travel. A blocked rack stalls all connected wheels.
                    int end = Wheels.Length - 1;
                    AngularSpeeds[end] = speed / PitchRadii[end];
                    for (int i = end - 1; i >= 0; i--) AngularSpeeds[i] = AngularSpeeds[i + 1] / Ratio(i + 1);
                }
            }
            else if (HasOutputLoad && Powered)
            {
                int last = Wheels.Length - 1;
                AngularSpeeds[last] = OutputSpeed;
                for (int i = last - 1; i >= 0; i--) AngularSpeeds[i] = AngularSpeeds[i + 1] / Ratio(i + 1);
            }
            for (int i = 0; i < Wheels.Length; i++)
            {
                Wheels[i].Rotate(Vector3.forward, AngularSpeeds[i] * Mathf.Rad2Deg * dt, Space.Self);
                if (i == 0 || AngularSpeeds[i] == 0 || Shaft(i)) continue;
                if(!PitchContact(Wheels[i-1].position,Wheels[i].position,Wheels[0].forward,PitchRadii[i-1],PitchRadii[i],MeshTolerance))continue;
                // Project the free bearing onto the ideal tooth-phase constraint when contact engages.
                // The correction is at most half one tooth pitch, never a carriage displacement.
                Quaternion basis = (Wheels[0].parent!=null?Wheels[0].parent.rotation:Quaternion.identity) * initialRotations[0];
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
            if (Meshed && AngularSpeeds[0] != 0)
                for (int i = 0; i < Wheels.Length; i++) if (Shared(i) != null) Shared(i).localRotation = Wheels[i].localRotation;
        }
    }
}
