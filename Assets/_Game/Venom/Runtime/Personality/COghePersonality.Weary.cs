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

        /// <summary>How far the body sags, 0–1: a sigh, a hungry COghe's droop, or its panting at a stop for breath (the
        /// renderer squashes the skin by it).</summary>
        public float Sag => game == null || game.Matter == null ? 0 : Mathf.Max(SighSag, Mathf.Max(Droop, RestSag));
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

        // Hungry or exhausted in a level, the body droops and heaves with slow, heavy breaths, walking or standing (skin only,
        // VenomLifeAnimation.Slump), and it stops on the way to catch its breath (VenomCampaignMotion.RestWalk, COgheEnergy):
        // at each stop it sinks and pants, an exhausted one flat to the floor, then rises and walks on at its usual pace.
        /// <summary>A hungry COghe's droop, and an exhausted one's, each plus a breath's heave (fractions of a sigh's sag).</summary>
        public const float HungryDroop = .16f, HungryHeave = .08f, ExhaustedDroop = .32f, ExhaustedHeave = .14f;
        /// <summary>Breaths a second while COghe pants at a stop.</summary>
        public const float PantRate = 2.6f;
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
        /// <summary>The panting at a stop for breath, <see cref="Sag"/>'s share: the body sinks, heaves quickly a few times
        /// and rises as the walk goes on; hungry it sinks halfway, exhausted it goes flat.</summary>
        public float RestSag
        {
            get
            {
                if (game == null || game.Matter == null || game.Home) return 0;
                var motion = game.Motion;
                float r = motion != null ? motion.RestingFor(motion.Selected) : -1, length = motion != null ? motion.RestPause : 0;
                if (r < 0 || length <= 0) return 0;
                float envelope = Mathf.SmoothStep(0, 1, r / .18f) * (1 - Mathf.SmoothStep(0, 1, (r - (length - .15f)) / .15f));
                float heave = Mathf.Sin(r * PantRate * 2 * Mathf.PI);
                return envelope * (COgheEnergy.State == COgheHunger.Exhausted ? .85f + .15f * heave : .5f + .25f * heave);
            }
        }
        private int restsHeard;
        /// <summary>Pants heard since the level started (tests, the review film).</summary>
        public int Pants { get; private set; }
        // the pant is heard once at the start of each stop
        private void TiredSounds(float dt)
        {
            var motion = game.Motion;
            if (motion == null || motion.Rests == restsHeard) return;
            if (motion.Rests > restsHeard && COgheEnergy.TiredIn(game))
            {
                Pants++;
                COgheAudio.Instance?.Play("creature_pant", COgheEnergy.State == COgheHunger.Exhausted ? .45f : .32f, 0, .3f);
            }
            restsHeard = motion.Rests;
        }

        private void ResetSigh() { sighAt = -100; Sighs = 0; restsHeard = 0; Pants = 0; }

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
