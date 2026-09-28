using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>A reusable two-station clutch. Each output catches only after its real rail settles.</summary>
    public sealed class COgheViewTransmission : COgheMechanism
    {
        public COgheRailSlider Power, Selector, First, Second;
        public COgheTissueSensor InputLoad;
        public bool FirstIsFinal, SecondIsFinal, RetainOutputs = true;
        public VenomSurfacePatch FirstAperture, SecondAperture;
        public Transform[] FirstWheels, SecondWheels;
        public Transform FirstPin, SecondPin;
        public Renderer FirstLamp, SecondLamp;
        public Material Waiting, Ready;
        public float WheelRadius = .025f, Force = .45f;
        public bool FirstCaught { get; private set; }
        public bool SecondCaught { get; private set; }
        public bool FirstEngaged => (InputLoad == null || InputLoad.Active) && (Power == null || Power.AtEnd) &&
            (Selector == null || Selector.Position <= Selector.CatchTolerance) && Meshed(FirstWheels);
        public bool SecondEngaged => (InputLoad == null || InputLoad.Active) && (Power == null || Power.AtEnd) && Selector != null && Selector.AtEnd && Meshed(SecondWheels);
        public override bool ControlsExit => FirstIsFinal || SecondIsFinal;
        public override bool ExitUnlocked => (!FirstIsFinal || First != null && First.AtEnd) &&
            (!SecondIsFinal || Second != null && Second.AtEnd);
        private float firstStable, secondStable;
        private bool firstOpen, secondOpen;
        private Quaternion[] firstRest, secondRest;
        private Vector3 firstPinScale, secondPinScale;
        private bool Meshed(Transform[] wheels)
        {
            if (wheels == null || wheels.Length < 2) return false;
            for (int i = 1; i < wheels.Length; i++)
                if (!COgheGearTrain.PitchContact(wheels[i-1].position, wheels[i].position,
                    wheels[0].forward, WheelRadius, WheelRadius, .004f)) return false;
            return true;
        }

        public override void InitializeMechanism(VenomCampaign game)
        {
            firstRest = Rest(FirstWheels); secondRest = Rest(SecondWheels);
            if (FirstPin != null) firstPinScale = FirstPin.localScale;
            if (SecondPin != null) secondPinScale = SecondPin.localScale;
        }
        private static Quaternion[] Rest(Transform[] wheels)
        {
            var rotations = new Quaternion[wheels == null ? 0 : wheels.Length];
            for (int i = 0; i < rotations.Length; i++) rotations[i] = wheels[i].localRotation;
            return rotations;
        }
        public override void ResetMechanism(VenomCampaign game)
        {
            FirstCaught = SecondCaught = firstOpen = secondOpen = false;
            firstStable = secondStable = 0;
            if (First != null) First.Locked = false;
            if (Second != null) Second.Locked = false;
            Restore(FirstWheels, firstRest); Restore(SecondWheels, secondRest);
            UpdateApertures(game);
        }
        private static void Restore(Transform[] wheels, Quaternion[] rotations)
        { if (rotations != null) for (int i = 0; i < rotations.Length; i++) wheels[i].localRotation = rotations[i]; }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            bool first = FirstCaught, second = SecondCaught;
            Drive(First, FirstEngaged, ref first, ref firstStable, FirstWheels, dt);
            Drive(Second, SecondEngaged, ref second, ref secondStable, SecondWheels, dt);
            FirstCaught = first; SecondCaught = second;
            if (FirstPin != null) FirstPin.localScale = Vector3.Scale(firstPinScale, new Vector3(first ? 1 : .3f, 1, 1));
            if (SecondPin != null) SecondPin.localScale = Vector3.Scale(secondPinScale, new Vector3(second ? 1 : .3f, 1, 1));
            UpdateApertures(game);
        }
        private void Drive(COgheRailSlider rail, bool engaged, ref bool caught, ref float stable, Transform[] wheels, float dt)
        {
            if (rail == null) return;
            float speed = Vector3.Dot(rail.Body.linearVelocity, rail.WorldAxis);
            stable = engaged && rail.AtEnd && Mathf.Abs(speed) < .025f ? stable + dt : 0;
            if (RetainOutputs && stable >= .1f) caught = true;
            rail.Locked = caught;
            if (!caught)
            {
                float target = engaged ? rail.Travel : 0;
                rail.ApplyEffort(rail.WorldAxis * Mathf.Clamp((target - rail.Position) * 6 - speed * .9f, -Force, Force));
            }
            if (engaged && wheels != null)
                for (int i = 0; i < wheels.Length; i++)
                    wheels[i].Rotate(Vector3.forward, speed / WheelRadius * Mathf.Rad2Deg * dt * (i % 2 == 0 ? 1 : -1), Space.Self);
        }
        private void ShowLamp(Renderer lamp, bool on)
        {
            if (lamp != null && lamp.sharedMaterial != (on ? Ready : Waiting)) lamp.sharedMaterial = on ? Ready : Waiting;
        }
        private void UpdateApertures(VenomCampaign game)
        {
            bool a = First != null && First.AtEnd, b = Second != null && Second.AtEnd;
            ShowLamp(FirstLamp, RetainOutputs ? FirstCaught : a);
            ShowLamp(SecondLamp, RetainOutputs ? SecondCaught : b);
            if (FirstAperture != null) FirstAperture.NavigationHoleBlocked = !a;
            if (SecondAperture != null) SecondAperture.NavigationHoleBlocked = !b;
            if ((a != firstOpen || b != secondOpen) && game.Motion != null) game.Motion.BuildGraph();
            firstOpen = a; secondOpen = b;
        }
    }
}
