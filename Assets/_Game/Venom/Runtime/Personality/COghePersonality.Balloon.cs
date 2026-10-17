using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// The balloon game, chapter 4's last question ("Hiểu ra" toys, Mrk 17/10/2026; chapter 4 teaches counting). COghe holds
    /// up a limp balloon and shows on its tendrils how many puffs it needs (2, then 3, then 3); each tap of the player on the
    /// balloon is a puff, and it grows. Exactly that many and a moment's wait: COghe ties it and lets it float over to the
    /// bunch. One too many and it pops in COghe's face (a gag); a new one comes, the same count. Three balloons: COghe takes
    /// their strings and floats up with them, higher with every tap of the player; the bunch is the player's. At Home a tap
    /// on it sends COghe to float with the balloons.
    /// </summary>
    public sealed partial class COghePersonality
    {
        private static readonly int[] BalloonPuffs = { 2, 3, 3 };
        public const float BalloonIntroLength = 2.0f, BalloonShowLength = 1.6f, BalloonTieLength = 1.3f;
        private int balloonPuffs; private float balloonPuffAt = -100, balloonSize = .3f;
        private bool IsBalloonGame => toy != null && toy.Id == COgheToys.Balloon;
        private int BalloonNeeds => BalloonPuffs[Mathf.Clamp(toyPattern, 0, BalloonPuffs.Length - 1)];
        private static Transform BalloonPart(COgheHomeItem bunch, int i) => bunch.Part("Balloon" + i);
        private static Transform StringPart(COgheHomeItem bunch, int i) => bunch.Part("String" + i);
        /// <summary>Where COghe holds a balloon to blow it up: in front of its mouth, toward the player.</summary>
        private Vector3 BalloonMouth => centre + CameraFlat() * .105f + up * .055f;   // out in front, clear of its dark body
        /// <summary>The balloon being blown up (or the bunch, in the flight at the end).</summary>
        private Vector3 BalloonPoint
        {
            get { var b = toy != null ? BalloonPart(toy, Mathf.Min(toyPattern, 2)) : null; return b != null ? b.position : centre; }
        }
        /// <summary>A balloon's size for a number of puffs: limp, then fuller with each, full at the count (and too big after).</summary>
        private float BalloonSizeFor(int puffs) => puffs <= 0 ? .3f : .3f + .7f * Mathf.Min(puffs, BalloonNeeds + 1) / BalloonNeeds;

        private void StartBalloonGame()
        {
            // the bunch starts empty: only the weight pops up; each balloon comes out limp in COghe's hold
            for (int i = 0; i < 3; i++) { var b = BalloonPart(toy, i); if (b != null) b.localScale = Vector3.zero; var s = StringPart(toy, i); if (s != null) s.gameObject.SetActive(false); }
            balloonPuffs = 0; balloonSize = .3f;
        }
        private void ResetBalloons(COgheHomeItem bunch)
        {
            for (int i = 0; i < 3; i++)
            {
                var b = BalloonPart(bunch, i); if (b != null) { b.localPosition = COgheHomeItems.BalloonSpots[i]; b.localScale = Vector3.one; b.localRotation = Quaternion.identity; }
                var s = StringPart(bunch, i); if (s != null) s.gameObject.SetActive(true);
            }
        }
        /// <summary>A tap on the balloon: a puff.</summary>
        private void BalloonTapped()
        {
            if (bonusPhase == BonusPhase.ToyJam) { jamBoostAt = bonusTime; COgheAudio.Instance?.Play("creature_hi", .35f, 0, .1f); return; }
            if (bonusPhase != BonusPhase.ToyListen) return;
            balloonPuffs++; toyTaps = balloonPuffs; toyLastHit = bonusTime; balloonPuffAt = bonusTime;
            COgheAudio.Instance?.Play("balloon_puff", .6f, 0, .03f);
            COgheAnalytics.Log("bonus_toy_tap", "balloon", toyPattern + 1, "puffs", balloonPuffs);
            if (balloonPuffs > BalloonNeeds) { BonusMisses++; StartGag(COgheGag.Pop); return; }   // one too many: pop!
            if (balloonPuffs == BalloonNeeds) toyConfirmAt = bonusTime + ToyConfirm;
        }

        private bool UpdateBalloonGame(float dt)
        {
            switch (bonusPhase)
            {
                case BonusPhase.ToyIntro:
                    Time += dt; BalloonIntroPose(bonusTime);
                    if (bonusTime >= BalloonIntroLength) StartToyShow();
                    return true;
                case BonusPhase.ToyShow:
                    Time += dt; BalloonShowPose(bonusTime);
                    if (bonusTime >= BalloonShowLength) StartToyListen();
                    return true;
                case BonusPhase.ToyListen:
                    Time += dt; BalloonListenPose(bonusTime);
                    if (toyConfirmAt >= 0 && bonusTime >= toyConfirmAt)
                    {   // the right size: it ties the balloon and lets it float over to the bunch
                        COgheAnalytics.Log("bonus_toy_beat", "balloon", toyPattern + 1);
                        bonusPhase = BonusPhase.ToyGood; bonusTime = 0; Begin(COgheAct.Home, BalloonTieLength);
                        COgheAudio.Instance?.Play("creature_grab", .45f, 0, .1f);
                    }
                    return true;
                case BonusPhase.ToyGood:
                    Time += dt; BalloonTiePose(bonusTime);
                    if (bonusTime < BalloonTieLength) return true;
                    toyPattern++;
                    if (toyPattern < BalloonPuffs.Length) StartToyShow();
                    else { bonusPhase = BonusPhase.ToyJam; bonusTime = 0; jamBoostAt = -100; Begin(COgheAct.Home, 1e6f); COgheAudio.Instance?.Play("creature_happy", .5f, 0, .1f); }
                    return true;
                case BonusPhase.ToyJam:
                    Time += dt; Pose = BalloonFlight(bonusTime, toy, JamLength, true);
                    if (bonusTime >= JamLength) { ResetBalloons(toy); WinToyGame(); }
                    return true;
            }
            return false;
        }

        // the balloon in COghe's hold: at its mouth, swaying a little, at the given size
        private void HoldBalloon(float size, float sway)
        {
            var b = BalloonPart(toy, Mathf.Min(toyPattern, 2)); if (b == null) return;
            b.position = BalloonMouth + up * (.03f * (size - .3f)) + Vector3.Cross(up, CameraFlat()) * (.004f * sway);
            b.localScale = Vector3.one * size; b.localRotation = Quaternion.AngleAxis(8 * sway, CameraFlat());
            balloonSize = size;
        }

        private void BalloonIntroPose(float t)
        {
            // the bunch's weight pops up; COghe pulls out a limp balloon and shows it off, wiggling
            const float s = .036f;
            var p = StillPose(); if (toy == null) { Pose = p; return; }
            float wiggle = Mathf.Sin(t * 12) * Smooth01(.4f, .7f, t) * (1 - Smooth01(1.6f, 1.9f, t));
            p.Centre = centre + up * (s * .15f * Mathf.Abs(wiggle)); p.Squash = 1 + .05f * wiggle;
            if (t > .5f) HoldBalloon(.3f * Smooth01(.5f, .8f, t), wiggle);
            p.Tendrils = t > .5f ? 1 : 0; p.TipA = BalloonMouth - up * .03f;
            if (Crossed(.55f, 100, t)) COgheAudio.Instance?.Play("creature_pop", .4f, 0, .1f);
            Pose = p;
        }

        private void BalloonShowPose(float t)
        {
            // "this many puffs!": the count on its tendrils, the limp balloon held up in front
            const float s = .036f;
            var p = StillPose();
            balloonPuffs = 0; HoldBalloon(.3f, Mathf.Sin(t * 3));
            p.Morph = Smooth01(.1f, .3f, t) * (1 - Smooth01(BalloonShowLength - .3f, BalloonShowLength, t));
            p.Shape = COgheShape.One + (Mathf.Clamp(BalloonNeeds, 1, 3) - 1);
            p.Centre = centre + up * (s * .25f * Mathf.Sin(Smooth01(0, .4f, t) * Mathf.PI));
            if (Crossed(.15f, 100, t)) COgheAudio.Instance?.Play("creature_pop", .4f, 0, .1f);
            Pose = p;
        }

        private void BalloonListenPose(float t)
        {
            // each tap of the player: COghe breathes in and blows, cheeks out toward the balloon, and it grows
            const float s = .036f;
            var p = StillPose(); var toCam = CameraFlat();
            float since = t - balloonPuffAt, blow = balloonPuffAt >= 0 && since >= 0 ? Mathf.Sin(Mathf.Clamp01(since / .45f) * Mathf.PI) : 0;
            float target = BalloonSizeFor(balloonPuffs), grow = Mathf.Lerp(balloonSize, target, 1 - Mathf.Exp(-lastDt * 9));
            HoldBalloon(grow, Mathf.Sin(t * 2.5f) * .5f);
            p.Axis = Vector3.Slerp(up, toCam, .5f * blow).normalized; p.Squash = 1 - .15f * blow;
            p.Centre = centre + toCam * (s * .1f * blow) + up * (s * .06f * Mathf.Max(0, Mathf.Sin(t * 3)));
            p.Tendrils = 1; p.TipA = BalloonMouth - up * .03f;
            Pose = p;
        }

        private void BalloonTiePose(float t)
        {
            // a twist of the knot, a hop, and the balloon floats off to its place in the bunch; its string shows as it lands
            const float s = .036f;
            var p = StillPose(); int i = Mathf.Min(toyPattern, 2); var b = BalloonPart(toy, i);
            float twist = Mathf.Sin(Smooth01(0, .35f, t) * Mathf.PI * 2), fly = Smooth01(.35f, 1.15f, t);
            if (b != null)
            {
                Vector3 rest = toy.Root.transform.TransformPoint(COgheHomeItems.BalloonSpots[i]), from = BalloonMouth + up * (.03f * (balloonSize - .3f));
                b.position = Vector3.Lerp(from, rest, fly) + up * (Mathf.Sin(fly * Mathf.PI) * .04f);
                b.localScale = Vector3.one * Mathf.Lerp(balloonSize, 1, fly); b.localRotation = Quaternion.AngleAxis(25 * twist, CameraFlat());
                var str = StringPart(toy, i); if (str != null) str.gameObject.SetActive(fly >= 1);
            }
            p.Turn = Quaternion.AngleAxis(12 * twist, up);
            float hop = Mathf.Sin(Smooth01(.35f, .75f, t) * Mathf.PI); p.Centre = centre + up * (s * .9f * hop); p.Squash = 1 + .1f * hop;
            p.Tendrils = t < .35f ? 1 : 0; p.TipA = BalloonMouth - up * .02f;
            if (Crossed(.4f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .45f, 0, .1f);
            Pose = p;
        }

        /// <summary>Flying with the balloons (the end of the game, and the game at Home): their strings in two tendrils, COghe
        /// floats up and bobs; in the game every tap of the player lifts it higher. Then they go back to the bunch.</summary>
        private COgheSkinPose BalloonFlight(float t, COgheHomeItem bunch, float length, bool taps)
        {
            const float s = .036f;
            var p = StillPose(); var side = Vector3.Cross(up, CameraFlat());
            float take = Smooth01(.1f, .7f, t) * (1 - Smooth01(length - .8f, length - .2f, t));
            float boost = taps && jamBoostAt >= 0 && t >= jamBoostAt ? Mathf.Exp(-(t - jamBoostAt) * 2.2f) * Smooth01(jamBoostAt, jamBoostAt + .2f, t) : 0;
            float lift = take * (.03f + .012f * Mathf.Sin(t * 2.4f)) + .04f * boost;
            Vector3 over = centre + up * (lift + .1f);
            p.Centre = centre + up * lift; p.Squash = 1 + .06f * Mathf.Sin(t * 2.4f) * take;
            p.Turn = Quaternion.AngleAxis(6 * Mathf.Sin(t * 1.7f) * take, CameraFlat());
            for (int i = 0; i < 3; i++)
            {
                var b = BalloonPart(bunch, i); if (b == null) continue;
                Vector3 rest = bunch.Root.transform.TransformPoint(COgheHomeItems.BalloonSpots[i]);
                Vector3 held = over + side * ((i - 1) * .04f) + up * (i == 1 ? .02f : 0) + side * (.006f * Mathf.Sin(t * 2 + i));
                b.position = Vector3.Lerp(rest, held, take); b.localScale = Vector3.one; b.localRotation = Quaternion.AngleAxis(6 * Mathf.Sin(t * 1.5f + i), CameraFlat());
                var str = StringPart(bunch, i); if (str != null) str.gameObject.SetActive(take < .05f);
            }
            if (take > .05f)
            {
                p.Tendrils = 2;
                p.TipA = Vector3.Lerp(centre + up * (s * .6f), over - side * .04f - up * .04f, take);
                p.TipB = Vector3.Lerp(centre + up * (s * .6f), over + side * .04f - up * .04f, take);
            }
            if (Crossed(.3f, 100, t)) COgheAudio.Instance?.Play("creature_whoosh", .35f, 0, .1f);
            return p;
        }

        /// <summary>At Home: up and away with the bunch, a bob, back down, and a heart.</summary>
        private void BalloonPlayPose(COgheHomeItem bunch, float t, ref COgheSkinPose p)
        {
            const float s = .036f;
            var flight = BalloonFlight(t, bunch, 4.6f, false);
            p.Centre = flight.Centre; p.Squash = flight.Squash; p.Turn = flight.Turn; p.Tendrils = flight.Tendrils; p.TipA = flight.TipA; p.TipB = flight.TipB;
            float hop = Mathf.Sin(Smooth01(4.6f, 5.0f, t) * Mathf.PI);
            p.Centre += up * (s * .8f * hop);
            p.Morph = Smooth01(4.75f, 5.0f, t) * (1 - Smooth01(5.35f, 5.6f, t)); p.Shape = COgheShape.Heart;
            if (Crossed(4.7f, 100, t)) COgheAudio.Instance?.Play("creature_tada", .45f, 0, .2f);
        }
    }
}
