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
        /// <summary>Where the Intelligence ability has COghe look while it thinks (null: its own choice).</summary>
        public Vector3? GuideLook;
        public COgheMood EyeMood(int group, out Vector3? look)
        {
            look = null;
            if (game == null) return COgheMood.Normal;
            if (room == null && game.Owner.Completed) return COgheMood.Happy;   // the win, and the victory dance after it
            if (Act == COgheAct.Doze) return COgheMood.Sleepy;
            if (Act == COgheAct.Trick)
            {
                if (COgheTricks.UsesFace(Trick)) return COgheMood.Hidden;          // the monster face has its own eyes
                switch (Trick)
                {
                    case COgheTrick.PuffUp: return COgheMood.Sulky;
                    case COgheTrick.Sneeze: return Time < 1.05f ? COgheMood.Sleepy : Time < 2.1f ? COgheMood.Surprised : COgheMood.Happy;
                    case COgheTrick.BackTurnSulk: if (Time < 2.0f) return COgheMood.Hidden; look = null; return COgheMood.Sulky;   // its back, then a sulky look round
                    case COgheTrick.TailChase:   // eyes on the tail, then rolling round dizzily
                        if (life != null) look = life.TrickFocus;
                        return Time < 2.4f ? COgheMood.Focus : Time < 3.35f ? COgheMood.Surprised : COgheMood.Happy;
                    case COgheTrick.Juggle: if (life != null && Time > .5f && Time < 4.0f) look = life.TrickFocus; return Time < 4.0f ? COgheMood.Focus : COgheMood.Happy;
                    case COgheTrick.Mimic: if (Time < .6f) { look = MimicTarget; return COgheMood.Focus; } return COgheMood.Happy;   // a look at it, then "ta-da"
                    case COgheTrick.RockPaperScissors: return Time > .3f && Time < 4.3f ? COgheMood.Hidden : COgheMood.Happy;   // two minis, no single face
                    case COgheTrick.BreathHeart: if (life != null && Time > 1.6f && Time < 4.1f) look = life.TrickFocus; return Time < 1.6f ? COgheMood.Focus : COgheMood.Love;
                    case COgheTrick.BubblePop: return Time < 2.0f ? COgheMood.Focus : Time < 2.6f ? COgheMood.Surprised : COgheMood.Happy;
                    case COgheTrick.Skater: return Time > 1.4f && Time < 2.1f ? COgheMood.Surprised : COgheMood.Happy;   // "oops"
                    default: return COgheMood.Happy;
                }
            }
            if (Act != COgheAct.None && Act != COgheAct.Home) return COgheMood.Hidden;   // a shape, a wave, the monster, a melt, a tantrum
            if (Act == COgheAct.Home && Pose.Morph > .15f) return COgheMood.Hidden;      // a heart, a bonus word
            if (room != null) return HomeMood(out look);
            if (GuideLook.HasValue) { look = GuideLook; return COgheMood.Focus; }   // Intelligence: looking around for the next device
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
                    case BonusPhase.Gag: { look = GagLook(out var mood); return mood; }
                    case BonusPhase.ToyIntro:
                        if (toy != null && bonusTime < .85f) { look = room.BoundsOf(toy).center; return COgheMood.Focus; }
                        return bonusTime > 1.42f && bonusTime < 1.8f ? COgheMood.Surprised : COgheMood.Happy;   // startled by its own BOOM
                    case BonusPhase.ToyShow: if (toy != null) look = room.BoundsOf(toy).center; return COgheMood.Focus;
                    case BonusPhase.ToyListen:
                        // its eyes go from the drum to the player and back, and to the drum at every strike
                        if (toy != null && (bonusTime - toyLastHit < .5f || Mathf.Repeat(bonusTime, 2.4f) < 1.0f)) { look = room.BoundsOf(toy).center; return COgheMood.Focus; }
                        return COgheMood.Happy;
                    case BonusPhase.ToyGood: case BonusPhase.ToyJam: return COgheMood.Happy;
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
                    if (yuck && eating && Time > .62f) return Time < 2.15f ? COgheMood.Sulky : Time < 2.55f ? COgheMood.Surprised : COgheMood.Normal;
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
