namespace GravityBox.Venom
{
    /// <summary>The 20 Emotion tricks (ability cards, Mrk 16/10/2026), in unlock order: each Emotion level opens the next
    /// two. Approved list: PLANS/COGHE_ABILITY_CARDS.md (Mrk 16/10/2026). Built five at a time for review.</summary>
    public enum COgheTrick
    {
        Groove, Wink, Raspberry, Clap, PuffUp,                 // Lv 1–3 (batch 1)
        Sneeze, Peekaboo, BellyDrum, BackTurnSulk, Moonwalk,   // Lv 3–5
        TailChase, YuckFace, Flex, BubblePop, Skater,          // Lv 6–8 (tail chase replaces the screen lick, Mrk 17/10)
        Selfie, Juggle, Mimic, RockPaperScissors, BreathHeart  // Lv 8–10
    }

    /// <summary>What each trick is and when it may play. Tricks are presentation only (skin, eyes, face, tendrils): they
    /// never move a particle, a device or the puzzle.</summary>
    public static class COgheTricks
    {
        public const int Count = 20;
        /// <summary>Tricks built so far (the review batches); later ones never play until they exist.</summary>
        public const int Built = 15;
        public static float Length(COgheTrick t)
        {
            switch (t)
            {
                case COgheTrick.Groove: return 3.6f;
                case COgheTrick.Wink: return 2.2f;
                case COgheTrick.Raspberry: return 2.8f;
                case COgheTrick.Clap: return 2.4f;
                case COgheTrick.PuffUp: return 2.8f;
                case COgheTrick.Sneeze: return 3.2f;
                case COgheTrick.Peekaboo: return 3.4f;
                case COgheTrick.BellyDrum: return 3.4f;
                case COgheTrick.BackTurnSulk: return 3.6f;
                case COgheTrick.Moonwalk: return 4.0f;
                case COgheTrick.TailChase: return 3.8f;
                case COgheTrick.YuckFace: return 2.9f;   // a Home meal pulled face, then gulped down anyway
                case COgheTrick.Flex: return 3.2f;
                case COgheTrick.BubblePop: return 3.4f;
                case COgheTrick.Skater: return 2.6f;
                default: return 3f;
            }
        }
        /// <summary>The trick draws COghe's monster face (fangs, tongue) instead of its eyes.</summary>
        public static bool UsesFace(COgheTrick t) => t == COgheTrick.Wink || t == COgheTrick.Raspberry || t == COgheTrick.Peekaboo;
        /// <summary>A heavy job: a crate pushed, or a handle that pulls a load (a flex instead of a clap).</summary>
        public static bool Heavy(COgheTapRail rail) => rail.CrateFaces || rail.CompensateLoad || rail.LoadShare > 0;
        /// <summary>Unlocked by the player's Emotion level (two a level) and already built.</summary>
        public static bool Unlocked(COgheTrick t) => (int)t < Built && (int)t < COgheAbilities.MoodsUnlocked;
        /// <summary>Tests and review reels: every built trick, whatever the Emotion level.</summary>
        public static bool AllForTests;
        public static bool Available(COgheTrick t) => (int)t < Built && (AllForTests || Unlocked(t));
    }
}
