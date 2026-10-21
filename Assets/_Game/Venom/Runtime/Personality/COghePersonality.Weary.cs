using UnityEngine;

namespace GravityBox.Venom
{
    // Exhausted (COgheEnergy, Mrk 21/10/2026: "khi mệt lả mà bấm cơ quan, COghe thở dài, xệ người một cái rồi vẫn đi làm"):
    // a device the player taps gets a sigh and a sag of the body, and the order is carried out at once. The sag is drawn on
    // the skin over the walk (VenomLifeAnimation), so nothing waits for it and no level gets harder.
    public sealed partial class COghePersonality
    {
        /// <summary>Seconds of a sigh: the body slumps, stays low a moment, then rises again.</summary>
        public const float SighLength = 1.1f;
        /// <summary>Taps in a row get one sigh: the next one waits this long (simulation seconds).</summary>
        public const float SighGap = 6f;
        private float sighAt = -100;
        /// <summary>Sighs since the level started (tests).</summary>
        public int Sighs { get; private set; }

        /// <summary>How far the body sags for a sigh, 0–1 (the renderer squashes the skin by it).</summary>
        public float Sag
        {
            get
            {
                if (game == null || game.Matter == null) return 0;
                float t = game.Matter.SimulationTime - sighAt;
                if (t < 0 || t > SighLength) return 0;
                return Mathf.SmoothStep(0, 1, t / .3f) * (1 - Mathf.SmoothStep(0, 1, (t - .6f) / (SighLength - .6f)));
            }
        }

        private void ResetSigh() { sighAt = -100; Sighs = 0; }

        private void OnMechanismTouched(COgheMechanism touched)
        {
            if (!Enabled || game == null || game.Matter == null || game.Home || touched == null) return;
            if (!COgheEnergy.TiredIn(game) || COgheEnergy.State != COgheHunger.Exhausted) return;
            float now = game.Matter.SimulationTime;
            if (touched.Refusals > 0 && now - touched.RefusedAt < .05f) return;   // refused: COghe is not going anyway
            if (now - sighAt >= 0 && now - sighAt < SighGap) return;
            sighAt = now; Sighs++;
            COgheAudio.Instance?.Play("creature_sigh", .5f);
        }

        private void OnDestroy() { if (game != null) game.MechanismTouched -= OnMechanismTouched; }
    }
}
