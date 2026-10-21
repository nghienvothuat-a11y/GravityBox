using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>How well fed COghe is (<see cref="COgheEnergy"/>).</summary>
    public enum COgheHunger { Fed, Hungry, Exhausted }

    /// <summary>
    /// COghe's energy (Mrk 21/10/2026: "mỗi loại thức ăn sẽ có chỉ số năng lượng … nếu không cho ăn nó sẽ đói, mệt"):
    /// 0–100, each level won uses <see cref="PerLevel"/>, food fills it back. A free steel ball gives the least and only
    /// fills COghe up to <see cref="BallCap"/>; snacks give more (<see cref="COgheSnacks"/>). Hungry, COghe will not play
    /// with its toys at Home and asks for food; in levels it looks tired (eyes, a drooping, heaving walk, a sigh at a
    /// device) and stops on the way to catch its breath, more so exhausted. A slower walk was tried first and dropped (Mrk
    /// 21/10/2026, it tipped marginal spots of the levels): COghe walks at its usual pace between the stops, and only stops
    /// on still, level floor, so grip, climbs, pushes, devices, tubes and swings play as usual and no level is blocked
    /// (Mrk 21/10/2026: "cứ vài giây nó lại dừng lại thở dốc … Đói thì dừng ngắn và thưa. Mệt lả thì dừng lâu và dày").
    /// Nothing is used
    /// before Home opens (Boss 10), since COghe cannot be fed yet. Stored with the shop state as the energy used, so an
    /// old save starts full. Mrk's decision replaces the "never weaker for not visiting Home" line of
    /// Docs/VENOM_PURE_PUZZLE_AND_HOME.md §5.
    /// </summary>
    public static class COgheEnergy
    {
        public const int Max = 100, PerLevel = 10, PerBall = 4, BallCap = 60, HungryBelow = 50, ExhaustedBelow = 25;
        /// <summary>Stops for breath in levels (<see cref="VenomCampaignMotion.RestWalk"/>, RestPause): seconds of walking
        /// between stops, and of each stop. Hungry: short and sparse; exhausted: long and dense.</summary>
        public const float HungryWalk = 2.4f, HungryPause = .5f, ExhaustedWalk = 1.4f, ExhaustedPause = .85f;

        public static int Value => Mathf.Clamp(Max - COgheShop.Current.EnergyUsed, 0, Max);
        public static COgheHunger State => Value < ExhaustedBelow ? COgheHunger.Exhausted : Value < HungryBelow ? COgheHunger.Hungry : COgheHunger.Fed;
        public static bool Hungry => State != COgheHunger.Fed;
        /// <summary>Hungry or exhausted in this game: the product's levels only (the lab and the tests' plain campaign keep the
        /// usual COghe).</summary>
        public static bool TiredIn(VenomCampaign game) => Hungry && game != null && COgheProductMode.Applies(game);
        /// <summary>Sets <paramref name="motion"/>'s stops for breath for <paramref name="state"/> (none when fed).</summary>
        public static void ApplyRests(VenomCampaignMotion motion, COgheHunger state)
        {
            if (motion == null) return;
            motion.RestWalk = state == COgheHunger.Exhausted ? ExhaustedWalk : state == COgheHunger.Hungry ? HungryWalk : 0;
            motion.RestPause = state == COgheHunger.Exhausted ? ExhaustedPause : state == COgheHunger.Hungry ? HungryPause : 0;
        }

        /// <summary>A level was won with Home open: it used some energy.</summary>
        public static void UseForLevel(string levelId) => Set(Value - PerLevel, "level", levelId);
        /// <summary>A steel ball eaten: the energy it gave (none once COghe is at <see cref="BallCap"/>).</summary>
        public static int EatBall()
        {
            int before = Value;
            if (before < BallCap) Set(Mathf.Min(BallCap, before + PerBall), "ball", "steel_ball");
            return Value - before;
        }
        /// <summary>A snack eaten: the energy it gave.</summary>
        public static int Eat(COgheSnacks.Snack snack)
        {
            int before = Value;
            if (snack != null) Set(before + snack.Energy, "snack", snack.Id);
            return Value - before;
        }

        private static void Set(int value, string reason, string id)
        {
            value = Mathf.Clamp(value, 0, Max);
            if (value == Value) return;
            COgheShop.SetEnergyUsed(Max - value);
            COgheAnalytics.Log("energy", "reason", reason, "id", id, "value", value);
        }
    }
}
