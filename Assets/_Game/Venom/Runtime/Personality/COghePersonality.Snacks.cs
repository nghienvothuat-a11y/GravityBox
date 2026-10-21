using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// Snacks at Home (Mrk 21/10/2026): the bought snack drops just in front of COghe, a tendril reaches, wraps it and reels
    /// it in, a gulp, then the snack's own reaction: yummy (lip-smacking, happy hops, hearts), spicy ("huh?", mad hopping,
    /// flames and steam), icy (stiff, shivering under falling snow, a warming shake), sour (scrunched tight, trembling,
    /// springing back) or lovely (heart eyes, a double spin, confetti, hearts). Its energy goes in with the gulp.
    /// </summary>
    public sealed partial class COghePersonality
    {
        private COgheSnacks.Snack snack; private GameObject snackModel; private COgheSnackFx snackFx; private Vector3 snackAt;
        /// <summary>Seconds from the drop to the gulp; the reaction follows.</summary>
        public const float SnackGulp = 1.08f;
        /// <summary>The snack COghe is eating or reacting to, else null.</summary>
        public COgheSnacks.Snack Snack => home == HomeState.Snack ? snack : null;
        public int SnacksEaten { get; private set; }
        public COgheSnackFx SnackFx => snackFx;
        /// <summary>Free for a snack: at Home, not in the bonus, the wardrobe, the menu or another snack.</summary>
        public bool CanEatSnack => room != null && !InBonus && !OnStage && !Showcase && home != HomeState.Snack;

        public static float ReactionLength(COgheTaste taste) =>
            taste == COgheTaste.Yummy ? 2f : taste == COgheTaste.Spicy ? 2.6f : taste == COgheTaste.Cold ? 2.4f : taste == COgheTaste.Sour ? 2.2f : 2.8f;

        /// <summary>Eat <paramref name="s"/> now (it was paid for): false, and nothing happens, if COghe cannot.</summary>
        public bool EatSnack(COgheSnacks.Snack s)
        {
            if (s == null || !CanEatSnack) return false;
            StopPlaying(); StopEating(); EndAct(); game.Motion.StopAll();
            snack = s; home = HomeState.Snack; stateTime = 0;
            var toCam = CameraFlat(); Vector3 at = centre + toCam * .075f;
            snackAt = at - up * Vector3.Dot(at - floor, up);
            if (snackModel != null) Destroy(snackModel);
            snackModel = COgheSnacks.Build(s, room.Root, room.Materials);
            snackModel.transform.SetPositionAndRotation(snackAt + up * .12f, Quaternion.LookRotation(toCam, up));
            if (snackFx == null) snackFx = COgheSnackFx.Create(room.Root, room.Materials, () => game.Matter.SimulationTime);
            Begin(COgheAct.Home, SnackGulp + ReactionLength(s.Kind));
            Pose = default; Pose.Turn = Quaternion.identity; Pose.Squash = 1; Pose.Axis = up; Pose.Bubble = -1;
            COgheAudio.Instance?.Play("creature_whoosh", .25f, 0, .05f);
            return true;
        }

        private void UpdateSnack(float dt)
        {
            game.Motion.StopAll();   // a short show: taps wait until it is over
            Time += dt; SnackPose(Time);
            if (Time >= Length) EndSnack();
        }

        private void EndSnack() { DropSnack(); EndAct(); home = HomeState.Rest; homeTimer = 0; homeNext = 2; }
        /// <summary>The snack is gone (eaten, or the show cut short by the wardrobe or the bonus).</summary>
        private void DropSnack() { if (snackModel != null) { Destroy(snackModel); snackModel = null; } snack = null; }

        private void SnackPose(float t)
        {
            const float s = .036f;
            var p = new COgheSkinPose { Blend = 1, Centre = centre, Turn = Quaternion.identity, Axis = up, Squash = 1, Bubble = -1 };
            Vector3 dir = Vector3.ProjectOnPlane(snackAt - centre, up); dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : CameraFlat();
            if (t < SnackGulp)
            {
                // it drops in front with a little bounce; a tendril reaches, wraps it, reels it in; it disappears with a gulp
                Vector3 mouth = centre + dir * (s * .7f) + up * (s * .35f);
                float fall = Mathf.Clamp01(t / .28f), bounce = t < .28f ? 0 : Mathf.Sin(Mathf.Clamp01((t - .28f) / .14f) * Mathf.PI) * .012f;
                float reach = Smooth01(.3f, .52f, t), pull = Smooth01(.52f, .86f, t), swallow = Smooth01(.86f, 1f, t);
                Vector3 rest = snackAt + up * ((1 - fall * fall) * .12f + bounce);
                Vector3 at = Vector3.Lerp(Vector3.Lerp(rest, mouth, pull), centre + up * (s * .2f), swallow);
                if (snackModel != null)
                {
                    snackModel.transform.position = at; snackModel.transform.localScale = Vector3.one * (Mathf.Lerp(1, .65f, pull) * (1 - swallow));
                    if (t >= 1f) { Destroy(snackModel); snackModel = null; SnacksEaten++; COgheEnergy.Eat(snack); game.GreetHomeQuietly(); }
                }
                p.Tendrils = t > .3f && t < .9f ? 1 : 0; p.TipA = t < .52f ? Vector3.Lerp(centre + dir * s, rest, reach) : at;
                p.Centre = centre + dir * (s * .25f * reach * (1 - swallow));
                p.Axis = t < .9f ? dir : up;
                float gulp = t < .92f ? 0 : Mathf.Sin(Mathf.Clamp01((t - .92f) / .3f) * Mathf.PI * 2) * .6f;
                p.Squash = t < .9f ? 1 - .1f * reach * (1 - pull) : 1 + .14f * gulp;
                if (Crossed(.5f, 100, t)) COgheAudio.Instance?.Play("creature_grab", .35f, 0, .1f);
                if (Crossed(.95f, 100, t)) COgheAudio.Instance?.Play("creature_gulp", .55f, 0, .1f);
            }
            else ReactionPose(ref p, t - SnackGulp);
            Pose = p;
        }

        private void ReactionPose(ref COgheSkinPose p, float r)
        {
            const float s = .036f;
            Vector3 toCam = CameraFlat(), side = Vector3.Cross(up, toCam).normalized;
            float L = ReactionLength(snack.Kind), fade = 1 - Smooth01(L - .25f, L, r);
            switch (snack.Kind)
            {
                case COgheTaste.Yummy:
                {   // it smacks its lips (a quick chew), then two happy hops; hearts float up
                    float chew = r < .6f ? Mathf.Sin(r * 9 * Mathf.PI * 2) * .06f * (1 - r / .6f) : 0;
                    float hops = Hop(r, .6f, .42f) + Hop(r, 1.1f, .42f);
                    p.Centre = centre + up * (s * 1.1f * hops);
                    p.Squash = 1 + chew + .12f * hops - .18f * (Hop(r, 1.02f, .12f) + Hop(r, 1.52f, .12f));
                    if (Crossed(.05f, 100, r)) COgheAudio.Instance?.Play("monster_slurp", .35f, 0, .1f);
                    if (Crossed(.65f, 100, r)) { affectionHearts = true; COgheAudio.Instance?.Play("creature_happy", .5f, 0, .1f); }
                    break;
                }
                case COgheTaste.Spicy:
                {   // a beat of "huh?", then it hops about madly, puffing flames and steam
                    float huh = Smooth01(0, .15f, r) * (1 - Smooth01(.25f, .35f, r));
                    float frenzy = Smooth01(.3f, .45f, r) * fade;
                    float hop = Mathf.Abs(Mathf.Sin((r - .3f) * Mathf.PI * 3.6f)) * frenzy;
                    p.Centre = centre + up * (s * .9f * hop) + side * (s * .45f * Mathf.Sin((r - .3f) * 9) * frenzy);
                    p.Turn = Quaternion.AngleAxis(16 * Mathf.Sin(r * 23) * frenzy, up);
                    p.Squash = 1 + .14f * huh + .1f * Mathf.Sin(r * 31) * frenzy;
                    foreach (float at in new[] { .35f, .9f, 1.45f, 2f })
                        if (Crossed(at, 100, r))
                        {
                            snackFx?.Flames(p.Centre + toCam * (s * .9f) + up * (s * .15f), toCam, up);
                            COgheAudio.Instance?.Play("balloon_puff", .4f, 0, .05f);
                        }
                    if (Crossed(.05f, 100, r)) COgheAudio.Instance?.Play("creature_curious_1", .4f, 0, .1f);
                    break;
                }
                case COgheTaste.Cold:
                {   // it stiffens, then shivers all over, huddled small, under falling snow; a last shake warms it up
                    float freeze = Smooth01(0, .2f, r);
                    float shiver = Smooth01(.15f, .3f, r) * (1 - Smooth01(L - .55f, L - .4f, r));
                    float shake = Smooth01(L - .5f, L - .4f, r) * (1 - Smooth01(L - .15f, L, r));
                    p.Turn = Quaternion.AngleAxis(7 * Mathf.Sin(r * 60) * shiver + 22 * Mathf.Sin(r * 26) * shake, up);
                    p.Centre = centre + side * (s * .07f * Mathf.Sin(r * 53) * shiver);
                    p.Squash = 1 - .1f * freeze * (1 - shake) + .04f * Mathf.Sin(r * 47) * shiver;
                    if (Crossed(.1f, 100, r)) { snackFx?.Snow(centre, up, side, 12); COgheAudio.Instance?.Play("creature_hm", .4f, 0, .1f); }
                    if (Crossed(L - .5f, 100, r)) COgheAudio.Instance?.Play("creature_ack_1", .4f, 0, .1f);
                    break;
                }
                case COgheTaste.Sour:
                {   // it scrunches up tight and trembles, then springs back taller with a wobble
                    float scrunch = Smooth01(0, .18f, r) * (1 - Smooth01(1.05f, 1.2f, r));
                    float spring = r < 1.2f ? 0 : Mathf.Sin((r - 1.2f) * 14) * Mathf.Exp(-(r - 1.2f) * 3.2f);
                    p.Squash = 1 - .36f * scrunch + .03f * Mathf.Sin(r * 70) * scrunch + .3f * spring;
                    p.Turn = Quaternion.AngleAxis(8 * Mathf.Sin(r * 40) * scrunch, toCam);
                    p.Centre = centre - up * (s * .2f * scrunch) + up * (s * .4f * Mathf.Max(0, spring));
                    if (Crossed(.05f, 100, r)) COgheAudio.Instance?.Play("creature_hmph", .45f, 0, .1f);
                    if (Crossed(1.2f, 100, r)) COgheAudio.Instance?.Play("creature_pop", .5f, 0, .1f);
                    break;
                }
                default:
                {   // heart eyes; it spins round twice with a hop, confetti pops, and hearts float up
                    float spin = Smooth01(.25f, 1.55f, r), hop = Mathf.Sin(spin * Mathf.PI);
                    p.Turn = Quaternion.AngleAxis(720 * spin, up);
                    p.Centre = centre + up * (s * 1.2f * hop);
                    p.Squash = 1 + .1f * hop - .15f * Hop(r, 1.55f, .14f);
                    if (Crossed(.3f, 100, r)) { snackFx?.Confetti(centre, up); COgheAudio.Instance?.Play("creature_tada", .5f, 0, .1f); }
                    if (Crossed(1.7f, 100, r)) { affectionHearts = true; snackFx?.Confetti(centre, up); COgheAudio.Instance?.Play("creature_happy", .5f, 0, .1f); }
                    break;
                }
            }
        }

        private static float Hop(float t, float start, float length) { float u = (t - start) / length; return u <= 0 || u >= 1 ? 0 : Mathf.Sin(u * Mathf.PI); }

        /// <summary>The eyes while it eats a snack: on the snack, then the taste's own (happy, startled, sad and cold, squinting
        /// then startled, heart eyes).</summary>
        private COgheMood SnackMood(out Vector3? look)
        {
            look = null;
            if (snack == null) return COgheMood.Happy;
            if (Time < SnackGulp) { if (snackModel != null) look = snackModel.transform.position; return Time < .25f ? COgheMood.Surprised : COgheMood.Focus; }
            float r = Time - SnackGulp, L = ReactionLength(snack.Kind);
            switch (snack.Kind)
            {
                case COgheTaste.Yummy: return COgheMood.Happy;
                case COgheTaste.Spicy: return COgheMood.Surprised;
                case COgheTaste.Cold: return r < .25f ? COgheMood.Surprised : r < L - .45f ? COgheMood.Sad : COgheMood.Happy;
                case COgheTaste.Sour: return r < 1.2f ? COgheMood.Sulky : r < 1.5f ? COgheMood.Surprised : COgheMood.Happy;
                default: return COgheMood.Love;
            }
        }
    }
}
