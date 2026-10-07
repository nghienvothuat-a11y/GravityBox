using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>What COghe's eyes show (prototype, Mrk 07/10/2026: "Nếu COghe có mắt để diễn tả cảm xúc thì có được không?").</summary>
    public enum COgheMood { Normal, Happy, Love, Sad, Sulky, Surprised, Sleepy, Focus, Hidden }

    /// <summary>
    /// The mood the eyes show, read from what COghe is already doing: a touch reaction, a sulk, a meal, a game, the bonus,
    /// a refused tap, a mechanism it works, a win. Shapes, the monster and the other acts hide the eyes (the shape is the
    /// message). Presentation only: nothing here changes what COghe does.
    /// </summary>
    public sealed partial class COghePersonality
    {
        private COgheEyes eyes;
        /// <summary>The eyes, once they are switched on (COgheEyes.Enabled), on the skin that exists by then.</summary>
        private void EnsureEyes() { if (COgheEyes.Enabled && eyes == null && game != null && game.Matter != null && game.Matter.GetComponent<VenomSurface>() != null) eyes = COgheEyes.Attach(game); }
        /// <summary>The mood for the eyes of fragment <paramref name="group"/>, and where they look (null: at the player).</summary>
        public COgheMood EyeMood(int group, out Vector3? look)
        {
            look = null;
            if (game == null) return COgheMood.Normal;
            if (room == null && game.Owner.Completed) return COgheMood.Happy;   // the win, and the victory dance after it
            if (Act == COgheAct.Doze) return COgheMood.Sleepy;
            if (Act != COgheAct.None && Act != COgheAct.Home) return COgheMood.Hidden;   // a shape, a wave, the monster, a melt, a tantrum
            if (Act == COgheAct.Home && Pose.Morph > .15f) return COgheMood.Hidden;      // a heart, a bonus word
            if (room != null) return HomeMood(out look);
            float now = game.Matter.SimulationTime;
            foreach (var m in game.Mechanisms)
            {
                if (m == null) continue;
                if (m.Refusals > 0 && now - m.RefusedAt < 1.3f) { look = m.RefusalPoint; return COgheMood.Sad; }
                if (m is COgheTapRail t && t.Busy && t.Actor >= 0 && game.Matter.Groups[t.Actor] == group) { look = t.HandPoint; return COgheMood.Focus; }
            }
            return COgheMood.Normal;
        }

        private COgheMood HomeMood(out Vector3? look)
        {
            look = null;
            if (InBonus)
            {
                switch (bonusPhase)
                {
                    case BonusPhase.No: return COgheMood.Sad;
                    case BonusPhase.Joy: case BonusPhase.Hop: case BonusPhase.Done: return COgheMood.Happy;
                    case BonusPhase.Finale: return COgheMood.Love;
                    case BonusPhase.Gulp: if (meal != null) look = meal.Position; return COgheMood.Focus;
                }
            }
            switch (home)
            {
                case HomeState.Sulk: return COgheMood.Sulky;
                case HomeState.React:
                    switch (reaction)
                    {
                        case Reaction.Hug: return COgheMood.Love;
                        case Reaction.Flatten: case Reaction.Dodge: return COgheMood.Surprised;
                        case Reaction.Roll: return COgheMood.Hidden;
                        default: return COgheMood.Happy;
                    }
                case HomeState.Eat:
                    if (meal != null) { look = eating ? mealFrom : meal.Position; return COgheMood.Focus; }
                    return COgheMood.Happy;
                case HomeState.Play:
                    if (playing == null) return COgheMood.Happy;
                    switch (playing.Id)
                    {
                        case "BED": case "HAMMOCK": return COgheMood.Sleepy;
                        case "BALL": { var ball = playing.Part("Ball"); if (ball != null) look = ball.position; return COgheMood.Focus; }
                        case "TV": case "AQUARIUM": look = room.BoundsOf(playing).center; return COgheMood.Focus;
                        default: return COgheMood.Happy;
                    }
                case HomeState.Walk:
                    if (target != null) look = room.BoundsOf(target).center;
                    return COgheMood.Normal;
            }
            return COgheMood.Normal;
        }
    }
}
