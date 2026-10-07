using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// The bonus "Hiểu ra" in Home (<see cref="COgheBonus"/>): COghe walks to the middle of the room and "says" what it wants
    /// in body shapes, a sentence of words played one after another (a number first, as raised tendrils, when it wants it
    /// several times). The player answers by doing it: a touch, Feed, the ball.
    /// - A word done (not the last): a quick thumbs-up; a counted answer flashes the count so far on its tendrils; reaching
    ///   the number, it waits a moment to check, and one too many gets a shake and the count starts again.
    /// - The whole question done: joy at once (a jump with a whole turn, a beating heart; the UI pays it at the top of the
    ///   jump), then the thing itself (it plays with the ball, catches and gulps the food), then the next question.
    /// - A wrong answer (or the wrong order): only a gentle shake, then it asks again.
    /// After the last question it thanks the player (<see cref="FinalePose"/>). While the bonus runs, COghe does not wander or
    /// obey floor taps; touches, items and Feed are its answers.
    /// </summary>
    public sealed partial class COghePersonality
    {
        private enum BonusPhase { None, Arrive, Ask, No, Hop, Count, Confirm, Gulp, Joy, Action, Front, Finale, Done }
        public const float JoyLength = 1.5f, JoyRewardAt = .3f, GulpLength = 1.57f, ConfirmLength = 1.3f;
        public const float FinaleLength = 6.2f, FinaleHeartsAt = 2.3f, FinaleConfettiAt = 3.55f, FinalePrizeAt = 4.75f;
        private const float WordSlot = 1.25f, SentenceRest = .7f;
        private BonusPhase bonusPhase;
        private COgheBonusRound[] bonusRounds;
        private int bonusRound, activeWord = -1, gulpsQueued;
        private int[] wordCount = new int[0]; private bool[] wordDone = new bool[0];
        private bool roundDone;          // the question is understood: after its action, the next question
        private bool gulpForWord, shakeAfterGulp;   // the gulp is a word's food (then on) / a count broke off mid-gulp (then a shake)
        private COgheBonusAsk lastWord;  // the word just completed (its action follows the hop or the joy)
        private readonly List<COgheShape> sentence = new List<COgheShape>(6);
        private COgheShape shownShape; private float shownMorph;
        private float bonusTime;
        private bool heartsDue, confettiDue, prizeDue;
        private int joyDue;

        public bool InBonus => bonusPhase != BonusPhase.None;
        public int BonusRound => bonusRound;
        public int BonusRounds => bonusRounds?.Length ?? 0;
        /// <summary>Wrong answers in the current question (the UI offers a hint after two).</summary>
        public int BonusMisses { get; private set; }
        /// <summary>The word to do next (the one being counted, or the first not done).</summary>
        public COgheBonusAsk? BonusAsk
        {
            get
            {
                if (!InBonus || bonusRound >= BonusRounds) return null;
                var words = bonusRounds[bonusRound].Words;
                if (activeWord >= 0) return words[activeWord];
                for (int i = 0; i < words.Length; i++) if (!wordDone[i]) return words[i];
                return null;
            }
        }
        /// <summary>How many times the word being counted has been done so far (0 when none is being counted).</summary>
        public int BonusCounted => activeWord >= 0 ? wordCount[activeWord] : 0;
        /// <summary>Words of the current question done so far.</summary>
        public int BonusWordsDone { get { int n = 0; foreach (bool d in wordDone) if (d) n++; return n; } }
        /// <summary>COghe is asking right now (playing its sentence and waiting for an answer).</summary>
        public bool BonusAsking => bonusPhase == BonusPhase.Ask;
        public bool BonusThanking => bonusPhase == BonusPhase.Front || bonusPhase == BonusPhase.Finale;
        /// <summary>Lent to the room after a right answer (playing with the ball): the camera follows it.</summary>
        public bool BonusBusy => bonusPhase == BonusPhase.Action;
        /// <summary>Its thank-you itself (and after): the camera comes close.</summary>
        public bool BonusCloseUp => bonusPhase == BonusPhase.Finale || bonusPhase == BonusPhase.Done;
        public bool BonusFinished => bonusPhase == BonusPhase.Done;

        /// <summary>True once, when the finale's heart is fully formed: the UI bursts hearts from it.</summary>
        public bool TakeBonusHearts() { bool due = heartsDue; heartsDue = false; return due; }
        /// <summary>Once per question understood, at the top of COghe's jump of joy: the question (1-based) the UI pays and shows.</summary>
        public int TakeBonusJoy() { int round = joyDue; joyDue = 0; return round; }
        /// <summary>True once, when the finale's star forms: confetti.</summary>
        public bool TakeBonusConfetti() { bool due = confettiDue; confettiDue = false; return due; }
        /// <summary>True once, as COghe starts its happy dance: the prize flies out of it into the Drops.</summary>
        public bool TakeBonusPrize() { bool due = prizeDue; prizeDue = false; return due; }

        public void BeginBonus(COgheBonusRound[] rounds)
        {
            if (room == null || rounds == null || rounds.Length == 0) return;
            StopPlaying(); FinishReveals(); EndAct(); home = HomeState.Rest; game.Motion.StopAll();
            bonusRounds = rounds; bonusRound = 0; heartsDue = confettiDue = prizeDue = false; joyDue = 0;
            StartQuestion(); WalkToBonusSpot();
        }

        public void EndBonus()
        {
            if (bonusPhase == BonusPhase.None) return;
            bonusPhase = BonusPhase.None; bonusRounds = null; heartsDue = confettiDue = prizeDue = false; joyDue = 0; activeWord = -1; gulpsQueued = 0;
            gulpForWord = shakeAfterGulp = false;
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

        private void StartQuestion()
        {
            int n = bonusRounds[bonusRound].Words.Length;
            wordCount = new int[n]; wordDone = new bool[n]; activeWord = -1; gulpsQueued = 0; roundDone = gulpForWord = shakeAfterGulp = false; BonusMisses = 0;
        }

        /// <summary>The player answered: a touch on COghe, Feed, or a tap on an item.</summary>
        private void BonusHeard(COgheBonusAnswer answer, string item)
        {
            var words = bonusRounds != null && bonusRound < BonusRounds ? bonusRounds[bonusRound].Words : null;
            if (words == null) return;
            string said = answer == COgheBonusAnswer.Item ? item : answer.ToString();
            // a number being counted: more of the same (one too many once it is reached), or something else breaks it off
            if (activeWord >= 0 && (bonusPhase == BonusPhase.Ask || bonusPhase == BonusPhase.Count || bonusPhase == BonusPhase.Gulp || bonusPhase == BonusPhase.Confirm))
            {
                var counted = words[activeWord];
                if (!counted.Accepts(answer, item)) { Wrong(said); return; }
                if (wordCount[activeWord] >= counted.Count) { COgheAnalytics.Log("bonus_answer", "round", bonusRound + 1, "answer", said, "correct", false, "too_many", true); Wrong(said); return; }
                CountOne(answer, said); return;
            }
            if (bonusPhase != BonusPhase.Ask) return;   // busy (walking back, reacting): the tap is not an answer
            int first = -1; for (int i = 0; i < words.Length; i++) if (!wordDone[i]) { first = i; break; }
            int match = -1;
            for (int i = 0; i < words.Length; i++)
                if (!wordDone[i] && words[i].Accepts(answer, item) && (!bonusRounds[bonusRound].Ordered || i == first)) { match = i; break; }
            if (match < 0) { COgheAnalytics.Log("bonus_answer", "round", bonusRound + 1, "answer", said, "correct", false); Wrong(said); return; }
            if (words[match].Count > 1) { activeWord = match; CountOne(answer, said); return; }
            COgheAnalytics.Log("bonus_answer", "round", bonusRound + 1, "answer", said, "correct", true);
            CompleteWord(match);
        }

        private void Wrong(string said)
        {
            BonusMisses++;
            if (activeWord >= 0) { wordCount[activeWord] = 0; activeWord = -1; }   // a count broken off (or one too many): count again
            if (bonusPhase == BonusPhase.Gulp) { shakeAfterGulp = true; return; }   // it finishes its mouthful; the shake follows
            bonusPhase = BonusPhase.No; bonusTime = 0; Begin(COgheAct.Home, .9f);
            COgheAudio.Instance?.Play("creature_hm", .45f, 0, .15f);
        }

        private void CountOne(COgheBonusAnswer answer, string said)
        {
            wordCount[activeWord]++;
            COgheAnalytics.Log("bonus_answer", "round", bonusRound + 1, "answer", said, "correct", true, "count", wordCount[activeWord]);
            if (answer == COgheBonusAnswer.Feed) { gulpsQueued++; if (bonusPhase != BonusPhase.Gulp) StartGulp(); return; }
            bonusPhase = BonusPhase.Count; bonusTime = 0; Begin(COgheAct.Home, .7f);
            COgheAudio.Instance?.Play("creature_pop", .4f, 0, .05f);
        }

        /// <summary>A counted answer is over (its flash or its gulp): the number reached waits to be checked; else it asks on.</summary>
        private void AfterCounted()
        {
            if (activeWord >= 0 && wordCount[activeWord] >= bonusRounds[bonusRound].Words[activeWord].Count)
            { bonusPhase = BonusPhase.Confirm; bonusTime = 0; Begin(COgheAct.Home, ConfirmLength); return; }
            Ask();
        }

        private void CompleteWord(int i)
        {
            wordDone[i] = true; lastWord = bonusRounds[bonusRound].Words[i];
            if (activeWord == i) activeWord = -1;
            roundDone = true; foreach (bool d in wordDone) roundDone &= d;
            if (roundDone)
            {   // joy at once (Mrk: "COghe phải thể hiện cảm xúc ngay lập tức"); the question is paid at the top of its jump
                bonusPhase = BonusPhase.Joy; bonusTime = 0; Begin(COgheAct.Home, JoyLength);
                COgheAudio.Instance?.Play("creature_happy", .55f, 0, .15f);
            }
            else
            {
                bonusPhase = BonusPhase.Hop; bonusTime = 0; Begin(COgheAct.Home, .8f);
                COgheAudio.Instance?.Play("creature_hi", .45f, 0, .1f);
            }
        }

        /// <summary>After the hop or the joy: the word's thing (counted words had theirs already), then on.</summary>
        private bool AfterWord()
        {
            EndAct();
            if (lastWord.Count == 1 && lastWord.Answer == COgheBonusAnswer.Feed) { gulpsQueued = 1; gulpForWord = true; StartGulp(); return true; }
            if (lastWord.Count == 1 && lastWord.Answer == COgheBonusAnswer.Item)
            {
                bonusPhase = BonusPhase.Action; bonusTime = 0; GoTo(room.Find(lastWord.Item));
                return false;
            }
            if (roundDone) NextBonusRound(); else Ask();
            return true;
        }

        private void Ask()
        {
            bonusPhase = BonusPhase.Ask; bonusTime = 0; Begin(COgheAct.Home, 1e6f);
            // the sentence: the number being counted, or what is left to do (in order: from the next word on)
            sentence.Clear();
            var round = bonusRounds[bonusRound];
            for (int i = 0; i < round.Words.Length; i++)
            {
                if (wordDone[i] || activeWord >= 0 && i != activeWord) continue;
                var w = round.Words[i];
                if (w.NumberShape.HasValue) sentence.Add(w.NumberShape.Value);
                sentence.Add(w.Shape);
            }
        }

        private void StartGulp()
        {
            var food = game.HomeFeedBalls;
            if (food == null || gulpsQueued <= 0) { gulpsQueued = 0; EndGulp(); return; }
            gulpsQueued--;
            // a ball drops right in front of it, and it catches and gulps it where it stands (no walking: a count stays quick)
            var toCam = CameraFlat(); Vector3 at = centre + toCam * .075f;
            mealFrom = at - up * Vector3.Dot(at - floor, up) + up * COgheFeedBall.Radius;
            meal = food.Serve(mealFrom + up * .09f); lastMeal = false; eating = true;
            bonusPhase = BonusPhase.Gulp; bonusTime = 0; Begin(COgheAct.Home, GulpLength);
            COgheAudio.Instance?.Play("creature_whoosh", .2f, 0, .05f);
        }

        /// <summary>The last ball is eaten: a shake if the count broke off meanwhile, on after a word's food, else the count goes on.</summary>
        private void EndGulp()
        {
            if (shakeAfterGulp) { shakeAfterGulp = gulpForWord = false; bonusPhase = BonusPhase.No; bonusTime = 0; Begin(COgheAct.Home, .9f); COgheAudio.Instance?.Play("creature_hm", .45f, 0, .15f); return; }
            if (gulpForWord) { gulpForWord = false; if (roundDone) NextBonusRound(); else Ask(); return; }
            AfterCounted();
        }

        /// <summary>Runs the bonus; false while COghe is lent to the room (walking to the ball and playing).</summary>
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
                    game.Motion.StopAll();
                    if (front) { bonusPhase = BonusPhase.Finale; bonusTime = 0; Begin(COgheAct.Home, FinaleLength); return true; }
                    Ask(); COgheAudio.Instance?.Play("creature_curious_1", .4f, 0, .15f);
                    return true;
                }
                case BonusPhase.Ask:
                    Time += dt; AskPose(bonusTime);
                    // ask again now and then, so a player who looked away hears it
                    if (Crossed(4.5f, 4.5f, bonusTime)) COgheAudio.Instance?.Play(rnd.Next(2) == 0 ? "creature_curious_1" : "creature_curious_2", .3f, 0, .15f);
                    return true;
                case BonusPhase.No:
                    Time += dt; NoPose(bonusTime);
                    if (bonusTime >= .9f) Ask();
                    return true;
                case BonusPhase.Count:
                    Time += dt; NumberPose(bonusTime, .7f, wordCount[activeWord], true);
                    if (bonusTime >= .7f) AfterCounted();
                    return true;
                case BonusPhase.Confirm:
                    Time += dt; NumberPose(bonusTime, ConfirmLength, wordCount[activeWord], false);
                    if (bonusTime >= ConfirmLength) CompleteWord(activeWord);
                    return true;
                case BonusPhase.Gulp:
                    Time += dt; GulpPose(bonusTime);
                    if (bonusTime < GulpLength) return true;
                    eating = false; meal = null;
                    if (gulpsQueued > 0) StartGulp(); else EndGulp();
                    return true;
                case BonusPhase.Hop:
                    Time += dt; HopPose(bonusTime);
                    if (bonusTime < .8f) return true;
                    return AfterWord();
                case BonusPhase.Joy:
                    Time += dt; JoyPose(bonusTime);
                    if (bonusTime >= JoyRewardAt && bonusTime - dt < JoyRewardAt) joyDue = bonusRound + 1;
                    if (bonusTime < JoyLength) return true;
                    return AfterWord();
                case BonusPhase.Action:
                    // the room's own states play it out (walking to the ball, its game); when COghe is free again, on
                    if (home == HomeState.Rest && playing == null && bonusTime > .3f) { if (roundDone) NextBonusRound(); else WalkToBonusSpot(); return true; }
                    if (bonusTime > 20) { StopPlaying(); home = HomeState.Rest; if (roundDone) NextBonusRound(); else WalkToBonusSpot(); return true; }   // could not get there
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
            bonusRound++;
            if (bonusRound < BonusRounds) { StartQuestion(); WalkToBonusSpot(); return; }
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

        private void AskPose(float t)
        {
            // one word: held up; several: played one after another, each formed then let go, a rest, then again
            const float s = .036f;
            var p = StillPose(); int n = sentence.Count;
            COgheShape shape; float form, local;
            if (n <= 1) { shape = n == 1 ? sentence[0] : COgheShape.Question; form = Smooth01(0, .45f, t); local = t; }
            else
            {
                float period = n * WordSlot + SentenceRest, at = Mathf.Repeat(t, period); int i = Mathf.FloorToInt(at / WordSlot);
                if (i >= n) { shape = sentence[n - 1]; form = 0; local = 0; }
                else
                {
                    shape = sentence[i]; local = at - i * WordSlot;
                    form = Smooth01(0, .28f, local) * (1 - Smooth01(WordSlot - .28f, WordSlot, local));
                    if (Crossed(i * WordSlot + .05f, period, t)) COgheAudio.Instance?.Play("creature_pop", .2f, 0, .1f);
                }
            }
            p.Morph = form; p.Shape = shape; shownShape = shape; shownMorph = form;
            if (shape == COgheShape.Ball)
            {
                // a cartoon ball bouncing on the spot: high, stretched as it flies, squashed flat as it lands
                float u = Mathf.Repeat(local / .7f, 1), height = 4 * u * (1 - u), speed = Mathf.Abs(1 - 2 * u);
                p.Centre = centre + up * (s * 2.2f * height * form);
                float land = Mathf.Exp(-Mathf.Min(u, 1 - u) * 18);
                p.Squash = (1 + .14f * speed * (1 - land) - .3f * land) * form + (1 - form);
                if (n <= 1 && t > .45f && Crossed(.7f, .7f, t)) COgheAudio.Instance?.Play("creature_land", .2f, 0, .1f);
            }
            else
            {
                // held up to the player, swaying a little; every few seconds it lifts it again, asking
                float beat = Mathf.Repeat(t - .5f, 2.8f), lift = n <= 1 && t > .5f ? Mathf.Sin(Mathf.Clamp01(beat / .5f) * Mathf.PI) : 0;
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
            p.Morph = shownMorph * (1 - Smooth01(0, .18f, t)); p.Shape = shownShape;
            float shake = Mathf.Sin(t * 21) * (1 - Smooth01(.45f, .85f, t)) * Smooth01(.08f, .2f, t);
            p.Centre = centre + side * (s * .32f * shake);
            p.Turn = Quaternion.AngleAxis(-9 * shake, CameraFlat());
            p.Squash = 1 - .06f * Mathf.Abs(shake);
            Pose = p;
        }

        private void NumberPose(float t, float length, int count, bool hop)
        {
            // the count so far on its tendrils (a little hop with each one); reaching the number, it holds it, leaning this way
            // and that: "that's it?"
            const float s = .036f;
            var p = StillPose(); var toCam = CameraFlat();
            p.Morph = Smooth01(0, .15f, t) * (1 - Smooth01(length - .15f, length, t)); p.Shape = COgheShape.One + (Mathf.Clamp(count, 1, 3) - 1);
            shownShape = p.Shape; shownMorph = p.Morph;
            if (hop) { float h = Mathf.Sin(Smooth01(0, .4f, t) * Mathf.PI); p.Centre = centre + up * (s * .8f * h); }
            else { p.Turn = Quaternion.AngleAxis(9 * Mathf.Sin(t * 4.2f), toCam); p.Centre = centre + up * (s * .15f * Mathf.Sin(t * 8.4f)); }
            Pose = p;
        }

        private void HopPose(float t)
        {
            // one word of several understood: a hop and a thumbs-up, "good, and the rest?"
            const float s = .036f;
            var p = StillPose();
            float hop = Mathf.Sin(Smooth01(0, .45f, t) * Mathf.PI);
            p.Centre = centre + up * (s * 1.1f * hop); p.Squash = 1 + .1f * hop;
            p.Morph = Smooth01(.1f, .3f, t) * (1 - Smooth01(.62f, .8f, t)); p.Shape = COgheShape.ThumbsUp;
            Pose = p;
        }

        private void GulpPose(float t)
        {
            // the ball drops in front of it; a tendril catches it and pulls it in, gulp
            if (t < .22f)
            {
                float k = t / .22f;
                if (meal != null) meal.Place(Vector3.Lerp(mealFrom + up * .09f, mealFrom, k * k), 1);
                var p = StillPose(); p.Squash = 1 + .05f * k; Pose = p;
                if (Crossed(.2f, 100, t)) COgheAudio.Instance?.Play("metal_clink", .3f, 0, .05f);
                return;
            }
            EatPose(t - .22f);
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
