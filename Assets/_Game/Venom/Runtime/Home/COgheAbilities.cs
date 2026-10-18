using UnityEngine;

namespace GravityBox.Venom
{
    public enum COgheAbility { Emotion, Agility, Intelligence }

    /// <summary>
    /// Ability cards (Mrk 16/10/2026): after each chapter's "Hiểu ra" the player takes exactly one card, and that ability
    /// rises by one, to <see cref="Max"/>. Emotion unlocks two new moods per level, Agility adds 10% walking speed per
    /// level, Intelligence is how many steps of a puzzle COghe remembers and walks to by itself (the player still works
    /// every device). Stored in the shop state with the other progression flags.
    /// </summary>
    public static class COgheAbilities
    {
        public const int Max = 10;
        public static int Level(COgheAbility ability)
        {
            var s = COgheShop.Current;
            int level = ability == COgheAbility.Emotion ? s.Emotion : ability == COgheAbility.Agility ? s.Agility : s.Intelligence;
            return Mathf.Clamp(level, 0, Max);
        }
        public static bool Full(COgheAbility ability) => Level(ability) >= Max;
        public static bool AllFull => Full(COgheAbility.Emotion) && Full(COgheAbility.Agility) && Full(COgheAbility.Intelligence);
        /// <summary>Walking speed multiplier in levels: +10% per Agility level.</summary>
        public static float SpeedScale => 1 + .1f * Level(COgheAbility.Agility);
        /// <summary>How many puzzle steps COghe remembers.</summary>
        public static int StepsRemembered => Level(COgheAbility.Intelligence);
        /// <summary>Emotion moods unlocked (two per level).</summary>
        public static int MoodsUnlocked => 2 * Level(COgheAbility.Emotion);
        public static bool Picked(int chapter) => COgheShop.Current.CardsPicked.Contains(chapter);

        /// <summary>The earliest chapter whose "Hiểu ra" is done but whose card is not taken yet (a player who left during
        /// the pick keeps it), or 0. Nothing waits once every ability is full.</summary>
        public static int Pending()
        {
            if (AllFull) return 0;
            for (int chapter = 1; chapter <= COgheBonus.MaxChapters && COgheBonus.Rounds(chapter) != null; chapter++)
                if (COgheBonus.Done(chapter) && !Picked(chapter)) return chapter;
            return 0;
        }
    }
}
