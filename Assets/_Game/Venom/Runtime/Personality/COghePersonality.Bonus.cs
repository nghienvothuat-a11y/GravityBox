using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// The bonus "Hiểu ra" in Home (<see cref="COgheBonus"/>): COghe walks to the middle of the room and holds a shape that
    /// says what it wants. The right answer gets a happy hop and then the thing itself (it plays with the ball, eats the
    /// food); a wrong one only a gentle shake before it asks again. After the last round it thanks the player: it presses
    /// against the screen, becomes a big heart (the UI sends hearts flying from it) and bounces.
    /// While the bonus runs, COghe does not wander or obey floor taps; touches, items and Feed are its answers.
    /// </summary>
    public sealed partial class COghePersonality
    {
        private enum BonusPhase { None, Arrive, Ask, No, Yes, Answer, Front, Finale, Done }
        public const float JoyLength = 1.5f, JoyRewardAt = .3f;
        public const float FinaleLength = 6.2f, FinaleHeartsAt = 2.3f, FinaleConfettiAt = 3.55f, FinalePrizeAt = 4.75f;
        private BonusPhase bonusPhase;
        private COgheBonusAsk[] bonusRounds;
        private int bonusRound;
        private float bonusTime;
        private bool heartsDue, confettiDue, prizeDue;
        private int joyDue;

        public bool InBonus => bonusPhase != BonusPhase.None;
        public int BonusRound => bonusRound;
        public int BonusRounds => bonusRounds?.Length ?? 0;
        /// <summary>Wrong answers in the current round (the UI offers a hint after two).</summary>
        public int BonusMisses { get; private set; }
        public COgheBonusAsk? BonusAsk => InBonus && bonusRound < BonusRounds ? bonusRounds[bonusRound] : null;
        /// <summary>COghe is asking right now (holding its shape and waiting for an answer).</summary>
        public bool BonusAsking => bonusPhase == BonusPhase.Ask;
        public bool BonusThanking => bonusPhase == BonusPhase.Front || bonusPhase == BonusPhase.Finale;
        /// <summary>Lent to the room after a right answer (playing with the ball, eating): the camera follows it.</summary>
        public bool BonusBusy => bonusPhase == BonusPhase.Answer;
        /// <summary>Its thank-you itself (and after): the camera comes close.</summary>
        public bool BonusCloseUp => bonusPhase == BonusPhase.Finale || bonusPhase == BonusPhase.Done;
        public bool BonusFinished => bonusPhase == BonusPhase.Done;

        /// <summary>True once, when the finale's heart is fully formed: the UI bursts hearts from it.</summary>
        public bool TakeBonusHearts() { bool due = heartsDue; heartsDue = false; return due; }
        /// <summary>Once per right answer, at the top of COghe's jump of joy: the round (1-based) whose reward the UI pays and shows.</summary>
        public int TakeBonusJoy() { int round = joyDue; joyDue = 0; return round; }
        /// <summary>True once, when the finale's star forms: confetti.</summary>
        public bool TakeBonusConfetti() { bool due = confettiDue; confettiDue = false; return due; }
        /// <summary>True once, as COghe starts its happy dance: the prize flies out of it into the Drops.</summary>
        public bool TakeBonusPrize() { bool due = prizeDue; prizeDue = false; return due; }

        public void BeginBonus(COgheBonusAsk[] rounds)
        {
            if (room == null || rounds == null || rounds.Length == 0) return;
            StopPlaying(); FinishReveals(); EndAct(); home = HomeState.Rest; game.Motion.StopAll();
            bonusRounds = rounds; bonusRound = 0; BonusMisses = 0; heartsDue = confettiDue = prizeDue = false; joyDue = 0;
            WalkToBonusSpot();
        }

        public void EndBonus()
        {
            if (bonusPhase == BonusPhase.None) return;
            bonusPhase = BonusPhase.None; bonusRounds = null; heartsDue = confettiDue = prizeDue = false; joyDue = 0;
            if (room != null) { StopPlaying(); EndAct(); home = HomeState.Rest; homeTimer = 0; homeNext = 1.5f; }
        }

        /// <summary>A tap on the screen near the shape COghe is holding (it stands taller than the body's particles).</summary>
        public bool BonusHit(Ray ray)
        {
            if (!InBonus) return false;
            Vector3 at = SkinCentre + up * (.036f * 1.2f);
            return Vector3.Cross(at - ray.origin, ray.direction).magnitude < .06f;
        }

        /// <summary>Feed pressed during the bonus: an answer, not a meal on demand.</summary>
        public void BonusFeed() => BonusHeard(COgheBonusAnswer.Feed, null);

        /// <summary>The player answered: a touch on COghe, Feed, or a tap on an item.</summary>
        private void BonusHeard(COgheBonusAnswer answer, string item)
        {
            if (bonusPhase != BonusPhase.Ask) return;   // busy (walking back, reacting): the tap is not an answer
            var ask = bonusRounds[bonusRound];
            bool right = ask.Accepts(answer, item);
            COgheAnalytics.Log("bonus_answer", "round", bonusRound + 1, "answer", answer == COgheBonusAnswer.Item ? item : answer.ToString(), "correct", right);
            if (!right)
            {
                BonusMisses++; bonusPhase = BonusPhase.No; bonusTime = 0; Begin(COgheAct.Home, .9f);
                COgheAudio.Instance?.Play("creature_hm", .45f, 0, .15f);
                return;
            }
            if (answer == COgheBonusAnswer.Feed) game.HomeFeedBalls?.Feed(1, centre + CameraFlat() * .1f);   // one ball, tossed close: it eats it after its hop (a wrong Feed throws none)
            // joy at once (Mrk: "COghe phải thể hiện cảm xúc ngay lập tức"), whatever the answer was
            bonusPhase = BonusPhase.Yes; bonusTime = 0; Begin(COgheAct.Home, JoyLength);
            COgheAudio.Instance?.Play("creature_happy", .55f, 0, .15f);
        }

        /// <summary>Runs the bonus; false while COghe is lent to the room (walking to the ball and playing, eating).</summary>
        private bool UpdateBonus(float dt)
        {
            bonusTime += dt;
            switch (bonusPhase)
            {
                case BonusPhase.Arrive:
                case BonusPhase.Front:
                {
                    bool front = bonusPhase == BonusPhase.Front;
                    if (game.Motion.Get(0) != null && bonusTime < (front ? 4 : 7)) return true;
                    if (front) { game.Motion.StopAll(); bonusPhase = BonusPhase.Finale; bonusTime = 0; Begin(COgheAct.Home, FinaleLength); return true; }
                    game.Motion.StopAll(); bonusPhase = BonusPhase.Ask; bonusTime = 0; Begin(COgheAct.Home, 1e6f);
                    COgheAudio.Instance?.Play("creature_curious_1", .4f, 0, .15f);
                    return true;
                }
                case BonusPhase.Ask:
                    Time += dt; AskPose(bonusRounds[bonusRound].Shape, bonusTime);
                    // ask again now and then, so a player who looked away hears it
                    if (Crossed(4.5f, 4.5f, bonusTime)) COgheAudio.Instance?.Play(rnd.Next(2) == 0 ? "creature_curious_1" : "creature_curious_2", .3f, 0, .15f);
                    return true;
                case BonusPhase.No:
                    Time += dt; NoPose(bonusTime);
                    if (bonusTime >= .9f) { bonusPhase = BonusPhase.Ask; bonusTime = 0; Begin(COgheAct.Home, 1e6f); }
                    return true;
                case BonusPhase.Yes:
                    Time += dt; JoyPose(bonusTime);
                    if (bonusTime >= JoyRewardAt && bonusTime - dt < JoyRewardAt) joyDue = bonusRound + 1;
                    if (bonusTime < JoyLength) return true;
                    EndAct();
                    var ask = bonusRounds[bonusRound];
                    if (ask.Answer == COgheBonusAnswer.Touch) { NextBonusRound(); return true; }
                    bonusPhase = BonusPhase.Answer; bonusTime = 0;
                    if (ask.Answer == COgheBonusAnswer.Item) GoTo(room.Find(ask.Item));
                    else { home = HomeState.Eat; stateTime = 0; meal = null; }
                    return false;
                case BonusPhase.Answer:
                    // the room's own states play it out; when COghe is free again, the round is done
                    if (home == HomeState.Rest && playing == null && !eating && bonusTime > .3f) { NextBonusRound(); return true; }
                    if (bonusTime > 20) { StopPlaying(); home = HomeState.Rest; NextBonusRound(); return true; }   // could not get there
                    return false;
                case BonusPhase.Finale:
                    Time += dt; FinalePose(bonusTime);
                    if (bonusTime >= FinaleHeartsAt && bonusTime - dt < FinaleHeartsAt) heartsDue = true;
                    if (bonusTime >= FinaleConfettiAt && bonusTime - dt < FinaleConfettiAt) confettiDue = true;
                    if (bonusTime >= FinalePrizeAt && bonusTime - dt < FinalePrizeAt) prizeDue = true;
                    if (bonusTime >= FinaleLength) { bonusPhase = BonusPhase.Done; bonusTime = 0; EndAct(); }
                    return true;
                case BonusPhase.Done:
                    return true;   // it rests, content, until the player continues
            }
            return false;
        }

        private void NextBonusRound()
        {
            bonusRound++; BonusMisses = 0;
            if (bonusRound < BonusRounds) { WalkToBonusSpot(); return; }
            // all understood: out into the open (clear of the furniture), where the camera comes close for its thank-you
            bonusPhase = BonusPhase.Front; bonusTime = 0; home = HomeState.Rest;
            game.Motion.Move(0, room.StageSpot());
        }

        private void WalkToBonusSpot()
        {
            bonusPhase = BonusPhase.Arrive; bonusTime = 0; home = HomeState.Rest;
            game.Motion.Move(0, room.Root.TransformPoint(new Vector3(0, COgheHomeRoom.BodyHeight, -.06f)));
        }

        private COgheSkinPose StillPose() => new COgheSkinPose { Blend = 1, Centre = centre, Turn = Quaternion.identity, Axis = up, Squash = 1, Bubble = -1 };

        private void AskPose(COgheShape shape, float t)
        {
            const float s = .036f;
            var p = StillPose(); float form = Smooth01(0, .45f, t);
            p.Morph = form; p.Shape = shape;
            if (shape == COgheShape.Ball)
            {
                // a cartoon ball bouncing on the spot: high, stretched as it flies, squashed flat as it lands
                float u = Mathf.Repeat(t / .7f, 1), height = 4 * u * (1 - u), speed = Mathf.Abs(1 - 2 * u);
                p.Centre = centre + up * (s * 2.2f * height * form);
                float land = Mathf.Exp(-Mathf.Min(u, 1 - u) * 18);
                p.Squash = (1 + .14f * speed * (1 - land) - .3f * land) * form + (1 - form);
                if (t > .45f && Crossed(.7f, .7f, t)) COgheAudio.Instance?.Play("creature_land", .2f, 0, .1f);
            }
            else
            {
                // held up to the player, swaying a little; every few seconds it lifts it again, asking
                float beat = Mathf.Repeat(t - .5f, 2.8f), lift = t > .5f ? Mathf.Sin(Mathf.Clamp01(beat / .5f) * Mathf.PI) : 0;
                p.Centre = centre + up * (s * .3f * lift);
                p.Turn = Quaternion.AngleAxis(5 * Mathf.Sin(t * 1.9f) * form, CameraFlat());
                p.Squash = 1 + .05f * Mathf.Sin(t * 2.3f);
            }
            Pose = p;
        }

        private void NoPose(float t)
        {
            // the shape melts back and the body shakes side to side: "no, not that"
            const float s = .036f;
            var p = StillPose(); var side = Vector3.Cross(up, CameraFlat());
            p.Morph = 1 - Smooth01(0, .18f, t); p.Shape = bonusRounds[bonusRound].Shape;
            float shake = Mathf.Sin(t * 21) * (1 - Smooth01(.45f, .85f, t)) * Smooth01(.08f, .2f, t);
            p.Centre = centre + side * (s * .32f * shake);
            p.Turn = Quaternion.AngleAxis(-9 * shake, CameraFlat());
            p.Squash = 1 - .06f * Mathf.Abs(shake);
            Pose = p;
        }

        private void JoyPose(float t)
        {
            // the moment it is understood: a crouch, a jump with a whole turn, then a heart that beats twice, and down again
            const float s = .036f;
            var p = StillPose(); var toCam = CameraFlat();
            float crouch = t < .12f ? Mathf.Sin(t / .12f * Mathf.PI) : 0;
            float jump = t < .64f ? Mathf.Sin(Smooth01(.1f, .64f, t) * Mathf.PI) : 0;
            float land = t > .62f ? Mathf.Sin(Mathf.Clamp01((t - .62f) / .14f) * Mathf.PI) : 0;
            p.Centre = centre + up * (s * 2.3f * jump);
            p.Turn = Quaternion.AngleAxis(360 * Smooth01(.14f, .6f, t), toCam);
            p.Squash = (1 - .28f * crouch) * (1 + .12f * jump) * (1 - .2f * land);
            float heart = Smooth01(.66f, .86f, t) * (1 - Smooth01(1.3f, JoyLength, t));
            float beat = t > .9f ? Mathf.Exp(-Mathf.Repeat(t - .9f, .3f) * 14) : 0;
            p.Morph = heart; p.Shape = COgheShape.Heart;
            p.Centre += up * (s * .28f * beat * heart);
            Pose = p;
            if (Crossed(.68f, 100, t)) COgheAudio.Instance?.Play("creature_tada", .5f, 0, .1f);
        }

        private void FinalePose(float t)
        {
            // the thank-you, the biggest moment (Mrk: "phải làm thật vui"): it squashes flat against the screen and wobbles
            // there, pops back in a high jump with a whole turn, becomes a big beating heart while hearts fly, then a star
            // with confetti, and dances from side to side as the prize flies into the Drops
            const float s = .036f;
            var p = StillPose(); var toCam = CameraFlat(); var side = Vector3.Cross(up, toCam);
            float press = Smooth01(.15f, .55f, t) * (1 - Smooth01(1.45f, 1.75f, t));
            float wobble = Mathf.Sin((t - .55f) * 17) * Mathf.Exp(-Mathf.Max(0, t - .55f) * 2.2f) * press;
            p.Centre = centre + toCam * (s * 1.5f * press) + up * (s * .35f * press);
            p.Axis = toCam; p.Squash = 1 - .45f * press + .06f * wobble;
            float pop = Mathf.Sin(Smooth01(1.7f, 2.2f, t) * Mathf.PI);
            p.Centre += up * (s * 1.8f * pop);
            if (t > 1.7f && t < 2.25f) p.Turn = Quaternion.AngleAxis(360 * Smooth01(1.72f, 2.18f, t), toCam);
            float heart = Smooth01(2.05f, 2.35f, t) * (1 - Smooth01(3.2f, 3.4f, t));
            float star = Smooth01(3.45f, 3.7f, t) * (1 - Smooth01(4.55f, 4.75f, t));
            if (heart > 0)
            {
                float beat = t > 2.4f ? Mathf.Exp(-Mathf.Repeat(t - 2.4f, .42f) * 12) : 0;
                p.Morph = heart; p.Shape = COgheShape.Heart;
                p.Centre += up * (s * .3f * beat * heart); p.Turn = Quaternion.AngleAxis(5 * Mathf.Sin(t * 5) * heart, toCam);
            }
            else if (star > 0)
            {
                p.Morph = star; p.Shape = COgheShape.Star;
                p.Centre += up * (s * .4f * star); p.Turn = Quaternion.AngleAxis(22 * Mathf.Sin((t - 3.45f) * 6) * star, toCam);
            }
            if (t > 4.75f)
            {
                float d = t - 4.75f, hop = Mathf.Abs(Mathf.Sin(d * 7f)) * (1 - Smooth01(5.7f, FinaleLength, t));
                p.Centre += up * (s * .9f * hop) + side * (s * .5f * Mathf.Sin(d * 3.5f) * (1 - Smooth01(5.7f, FinaleLength, t)));
                p.Turn = Quaternion.AngleAxis(10 * Mathf.Sin(d * 7f) * (1 - Smooth01(5.7f, FinaleLength, t)), toCam);
                p.Squash = 1 + .1f * hop;
            }
            Pose = p;
            if (Crossed(1.72f, 100, t)) COgheAudio.Instance?.Play("creature_whoosh", .4f, 0, .1f);
            if (Crossed(3.5f, 100, t)) COgheAudio.Instance?.Play("creature_tada", .5f, 0, .1f);
            if (Crossed(4.8f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .55f, 0, .1f);
            if (Crossed(.5f, 100, t)) COgheAudio.Instance?.Play("creature_splat", .35f, 0, .1f);
            if (Crossed(2.0f, 100, t)) COgheAudio.Instance?.Play("creature_tada", .55f, 0, .1f);
        }
    }
}
