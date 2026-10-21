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

        /// <summary>How far the body sags, 0–1: a sigh, or a hungry COghe's droop (the renderer squashes the skin by it).</summary>
        public float Sag => game == null || game.Matter == null ? 0 : Mathf.Max(SighSag, Droop);
        /// <summary>The sigh's share of <see cref="Sag"/>.</summary>
        public float SighSag
        {
            get
            {
                if (game == null || game.Matter == null) return 0;
                float t = game.Matter.SimulationTime - sighAt;
                if (t < 0 || t > SighLength) return 0;
                return Mathf.SmoothStep(0, 1, t / .3f) * (1 - Mathf.SmoothStep(0, 1, (t - .6f) / (SighLength - .6f)));
            }
        }

        // Hungry or exhausted in a level (Mrk 21/10/2026: "chỉ thể hiện bằng animation", nothing of the level changes), the
        // body droops and heaves with slow, heavy breaths, walking or standing, and an exhausted one pants now and then on the
        // way. Skin only (VenomLifeAnimation.Slump): the walk, its speed and every force stay as a fed COghe's.
        /// <summary>A hungry COghe's droop, and an exhausted one's, each plus a breath's heave (fractions of a sigh's sag).</summary>
        public const float HungryDroop = .16f, HungryHeave = .08f, ExhaustedDroop = .32f, ExhaustedHeave = .14f;
        /// <summary>Seconds of walking between an exhausted COghe's pants.</summary>
        public const float PantEvery = 6.5f;
        /// <summary>The droop's share of <see cref="Sag"/>.</summary>
        public float Droop
        {
            get
            {
                if (game == null || game.Matter == null || game.Home || !COgheEnergy.TiredIn(game)) return 0;
                bool exhausted = COgheEnergy.State == COgheHunger.Exhausted;
                float breath = .5f + .5f * Mathf.Sin(game.Matter.SimulationTime * (exhausted ? 1.3f : 1f) * 2 * Mathf.PI);
                return exhausted ? ExhaustedDroop + ExhaustedHeave * breath : HungryDroop + HungryHeave * breath;
            }
        }
        private float walkedSincePant;
        /// <summary>Pants since the level started (tests).</summary>
        public int Pants { get; private set; }
        private void TiredSounds(float dt)
        {
            if (!COgheEnergy.TiredIn(game) || COgheEnergy.State != COgheHunger.Exhausted || !grounded || speed < .03f) return;
            walkedSincePant += dt;
            if (walkedSincePant < PantEvery) return;
            walkedSincePant = 0; Pants++;
            COgheAudio.Instance?.Play("creature_pant", .4f, 0, .3f);
        }

        private void ResetSigh() { sighAt = -100; Sighs = 0; walkedSincePant = PantEvery * .6f; Pants = 0; }

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
