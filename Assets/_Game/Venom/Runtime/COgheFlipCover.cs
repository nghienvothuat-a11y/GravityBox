using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// A five-sided box over a switch, hinged on one edge like the flip cover of a wall socket (Mrk, 05/10/2026).
    /// Presentation only: the switch under it is locked by real rails (its RequiredRail/AlsoRequired). The lid follows
    /// those rails, and each latch clip slides off as its own rail opens; with clips, the lid waits for the last one.
    /// Nothing here changes physics, input or navigation.
    /// </summary>
    public sealed class COgheFlipCover : MonoBehaviour
    {
        public COgheRailSlider[] Locks = System.Array.Empty<COgheRailSlider>();
        public Transform Lid;
        /// <summary>Hinge axis in the lid's parent frame; a positive angle swings the box off its switch.</summary>
        public Vector3 HingeAxis = Vector3.right;
        public float OpenAngle = 95;
        public Transform[] Clips = System.Array.Empty<Transform>();
        public Vector3[] ClipTravel = System.Array.Empty<Vector3>();
        private Quaternion lidRest;
        private Vector3[] clipRest;
        private bool ready;

        private void Awake() => Prepare();

        private void Prepare()
        {
            if (ready) return;
            ready = true;
            if (Lid != null) lidRest = Lid.localRotation;
            clipRest = new Vector3[Clips.Length];
            for (int i = 0; i < Clips.Length; i++) if (Clips[i] != null) clipRest[i] = Clips[i].localPosition;
        }

        /// <summary>0 closed … 1 fully open, from the rails' shown (interpolated) positions.</summary>
        public float Openness
        {
            get
            {
                float open = 1;
                foreach (var rail in Locks) if (rail != null) open = Mathf.Min(open, Fraction(rail));
                // A latched box stays shut until its clips are nearly clear.
                return Clips.Length > 0 ? Mathf.Clamp01((open - .35f) / .65f) : open;
            }
        }

        private static float Fraction(COgheRailSlider rail) => rail.Travel > 0 ? Mathf.Clamp01(rail.ShownPosition / rail.Travel) : 1;

        private void LateUpdate()
        {
            Prepare();
            for (int i = 0; i < Clips.Length && i < Locks.Length; i++)
                if (Clips[i] != null && Locks[i] != null && i < ClipTravel.Length)
                    Clips[i].localPosition = clipRest[i] + ClipTravel[i] * Mathf.SmoothStep(0, 1, Mathf.Clamp01(Fraction(Locks[i]) * 1.6f));
            if (Lid != null) Lid.localRotation = Quaternion.AngleAxis(OpenAngle * Mathf.SmoothStep(0, 1, Openness), HingeAxis) * lidRest;
        }
    }
}
