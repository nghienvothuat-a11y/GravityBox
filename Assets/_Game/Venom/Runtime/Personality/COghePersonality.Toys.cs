using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>What COghe does about a wrong answer in "Hiểu ra" (Mrk 17/10/2026: a comedy, not a telling-off).</summary>
    public enum COgheGag { None, Spit, GiggleRumble, GigglePoint, Bonk, Hiccup, GiggleRoll, CoverEars, Nope, Startle, Blooper, Pop, TopFall }

    /// <summary>
    /// "Hiểu ra" toys and gags (Mrk 17/10/2026).
    /// - The game with a toy (<see cref="COgheToys"/>), the last question of a chapter: COghe brings the drum out and plays a
    ///   beat on it (2 strikes, then 3, then 4); the player plays it back by tapping the drum. Only the number of strikes
    ///   counts, never the timing, and nothing is timed. One strike too many and COghe covers its ears, then plays the beat
    ///   again. Three beats played back: they jam together for a few seconds (each tap of the player gets a strike and a
    ///   bounce), then COghe's joy and the reward, and the drum is the player's. At Home a tap on it sends COghe to play.
    /// - A wrong answer is a little comedy about what COghe got instead: it spits the food out, giggles at a touch and then
    ///   shows what it wanted (a rumbling belly, a tendril pointing), the ball bonks it on the head, too much food gives it
    ///   hiccups, too many touches a giggling roll. Then it asks again.
    /// Presentation only: the skin, a served food ball, and the ball's and the drum's own parts.
    /// </summary>
    public sealed partial class COghePersonality
    {
        // the beats of the drum game (seconds from the first strike): two, three, then four ("ta ta, ta ta")
        private static readonly float[][] DrumBeats = { new[] { 0f, .5f }, new[] { 0f, .42f, .84f }, new[] { 0f, .36f, .96f, 1.32f } };
        /// <summary>How hard the game being played is (<see cref="COgheBonusRound.Level"/>): 0 in chapters 2–6.</summary>
        private int toyLevel;
        /// <summary>The drum's beat <paramref name="k"/> (seconds from the first strike): the approved three at level 0; later one
        /// strike more per level (up to 6), in pairs with a short rest between.</summary>
        private float[] DrumPattern(int k)
        {
            k = Mathf.Clamp(k, 0, DrumBeats.Length - 1);
            if (toyLevel <= 0) return DrumBeats[k];
            int n = Mathf.Min(6, DrumBeats[k].Length + toyLevel);
            var times = new float[n]; float at = 0;
            for (int i = 0; i < n; i++) { times[i] = at; at += i % 2 == 0 ? .34f : .56f; }
            return times;
        }
        public const float ToyIntroLength = 2.3f, ToyGoodLength = .8f, JamLength = 5.4f, JamBeat = .5f, ToyConfirm = .7f, ToyReplayAfter = 7f;
        private const float ShowLead = .35f, DrumHeadY = .45f;
        private COgheHomeItem toy; private int toyPattern, toyTaps; private float toyLastHit = -100, toyConfirmAt = -1, jamBoostAt = -100, drumHitAt = -100, toyPop = -1;
        private COgheGag gag, pendingGag; private float gagLength; private COgheHomeItem gagItem; private COgheFeedBall gagBall; private Vector3 gagFrom, gagLook;

        /// <summary>The gag playing now (None outside one).</summary>
        public COgheGag Gag => bonusPhase == BonusPhase.Gag ? gag : COgheGag.None;
        /// <summary>The last gag played and how many so far (tests, the review reel).</summary>
        public COgheGag LastGag { get; private set; }
        public int GagsPlayed { get; private set; }
        /// <summary>The current question is a game with a toy.</summary>
        public bool InToyGame => InBonus && bonusRounds != null && bonusRound < BonusRounds && bonusRounds[bonusRound].IsGame;
        public COgheHomeItem ToyItem => InToyGame ? toy : null;
        /// <summary>Which beat of the game (0-based) and how many strikes it has.</summary>
        public int ToyPattern => toyPattern;
        public int ToyBeats => IsBallGame ? BallPasses : IsHornGame ? HornTune.Length : IsBalloonGame ? BalloonNeeds
            : IsTopGame ? TopRescuesNow : IsBoxGame ? BoxTurns
            : toyPattern < DrumBeats.Length ? DrumPattern(toyPattern).Length : 0;
        /// <summary>The toy of the game being played (its id), or null.</summary>
        public string ToyId => InToyGame ? bonusRounds[bonusRound].Toy : null;
        /// <summary>The horn's tune to play back (tests), and where to tap the toy now.</summary>
        public int[] ToyTune => IsHornGame ? HornTune : null;
        public Vector3 ToyTapPoint => toy != null ? ToyPoint : centre;
        /// <summary>The dots the UI shows while it is the player's turn: strikes of this beat, or passes of the ball game.</summary>
        public int ToyDots => ToyBeats;
        public int ToyDotsFilled => IsBallGame || IsTopGame ? toyPattern : toyTaps;
        /// <summary>The spinning top leans enough to need a tap (tests, the review reel).</summary>
        public bool ToyWobbling => IsTopGame && topWobble > .2f;
        public bool ToyIsBall => InToyGame && bonusRounds[bonusRound].Toy == "BALL";
        /// <summary>The game waits for the player: the drum's turn, or the ball resting at the player's spot.</summary>
        public bool ToyWaitingForPlayer => ToyListening && (!IsBallGame || ballAtPlayer && ballReturnAt < 0);
        /// <summary>How close the camera frames the game (the ball game spans the aisle).</summary>
        public float ToyFrameSize => IsBallGame ? .36f : IsBalloonGame ? .34f : IsHornGame ? .34f : IsTopGame || IsBoxGame ? .3f : .28f;   // the horn is long
        /// <summary>Strikes the player has played back so far.</summary>
        public int ToyTaps => toyTaps;
        public bool ToyListening => bonusPhase == BonusPhase.ToyListen;
        public bool ToyJamming => bonusPhase == BonusPhase.ToyJam;
        public bool ToyShowing => bonusPhase == BonusPhase.ToyShow || bonusPhase == BonusPhase.ToyIntro;
        /// <summary>Changes whenever the game's on-screen state does (the UI rebuilds its line and beat dots).</summary>
        public int ToyStateKey => InToyGame ? (((int)bonusPhase * 8 + toyPattern) * 8 + toyTaps) + 1 : 0;
        /// <summary>The camera frames COghe and the toy, close, while they play.</summary>
        public Vector3? ToyFocus
        {
            get
            {
                if (!InToyGame || toy == null || !toy.Root.activeInHierarchy || bonusPhase == BonusPhase.Arrive || bonusPhase == BonusPhase.Joy) return null;
                return Vector3.Lerp(SkinCentre, ToyPoint, .5f) + up * .012f;
            }
        }
        /// <summary>The camera comes closer for a gag (the joke reads at a glance); for the ball's bonk it takes in the ball's way
        /// too, for the spat food the way it flies.</summary>
        public bool GagFrame(out Vector3 target, out float size)
        {
            target = SkinCentre + up * .01f; size = .27f;
            if (bonusPhase != BonusPhase.Gag || gag == COgheGag.Nope) return false;
            if (InToyGame && toy != null) { target = Vector3.Lerp(SkinCentre, ToyPoint, .5f) + up * .012f; size = ToyFrameSize + .01f; return true; }   // the toy stays in view
            if ((gag == COgheGag.Bonk || gag == COgheGag.GigglePoint || gag == COgheGag.Startle) && gagItem != null) { target = Vector3.Lerp(target, ItemPoint(gagItem), .4f); size = .34f; }   // and what it is about
            if (gag == COgheGag.Spit) { target += CameraFlat() * .04f; size = .3f; }
            return true;
        }
        /// <summary>Where the toy of the game is now (the ball moves), and where any item is.</summary>
        private Vector3 ToyPoint => IsBallGame ? ballAt : IsBalloonGame ? BalloonPoint : room.BoundsOf(toy).center;
        private Vector3 ItemPoint(COgheHomeItem item) { if (item == toy && IsBalloonGame) return BalloonPoint; var ball = item.Id == "BALL" ? item.Part("Ball") : null; return ball != null ? ball.position : room.BoundsOf(item).center; }
        private bool CurrentIsGame => bonusRounds != null && bonusRound < BonusRounds && bonusRounds[bonusRound].IsGame;

        // The drum game ---------------------------------------------------------------------------------------------------
        private void StartToyGame()
        {
            toy = room.Find(bonusRounds[bonusRound].Toy); toyLevel = bonusRounds[bonusRound].Level; toyPattern = 0; toyTaps = 0; toyConfirmAt = -1; toyLastHit = -100;
            if (IsBallGame) { StartBallGame(); skinTop = SkinTopOver(floor); }
            else if (toy != null && !toy.Root.activeSelf)
            {   // brought out for the game: it pops up beside COghe
                room.ShowToy(toy, true); toy.Root.transform.localScale = Vector3.zero; toyPop = 0;
            }
            if (IsBalloonGame) StartBalloonGame();
            if (IsTopGame) { topSpeed = topWobble = topFallen = 0; topNeedsPull = false; topRescuedAt = -1; }
            if (IsBoxGame) { boxLid = boxCrank = boxCrankTarget = boxDancer = 0; boxStep = 0; pendingNote = -1; }
            bonusPhase = BonusPhase.ToyIntro; bonusTime = 0; Begin(COgheAct.Home, 1e6f);
            COgheAudio.Instance?.Play("creature_curious_1", .4f, 0, .15f);
            COgheAnalytics.Log("bonus_toy_start", "toy", toy != null ? toy.Id : "none");
        }
        private void StartToyShow() { bonusPhase = BonusPhase.ToyShow; bonusTime = 0; toyTaps = 0; toyConfirmAt = -1; Begin(COgheAct.Home, 1e6f); }
        private void StartToyListen()
        {
            bonusPhase = BonusPhase.ToyListen; bonusTime = 0; toyTaps = 0; toyConfirmAt = -1; toyLastHit = -100; ballReturnAt = -1;
            hornPlayed.Clear(); balloonPuffs = 0; balloonPuffAt = -100; Begin(COgheAct.Home, 1e6f);
        }

        /// <summary>A tap on the screen during the game: the drum (generously around it) comes before COghe itself.</summary>
        public bool ToyTouch(Ray ray)
        {
            if (!InToyGame || toy == null || !toy.Root.activeInHierarchy) return false;
            if (IsBallGame)
            {   // the ball where it is now, generously
                if (Vector3.Cross(ballAt - ray.origin, ray.direction).magnitude > BallRadius + .03f) return false;
            }
            else if (IsBalloonGame)
            {   // the balloon being blown up (in the flight: the balloons over COghe), generously
                float reach = bonusPhase == BonusPhase.ToyJam ? .08f : .04f * balloonSize + .03f;
                if (Vector3.Cross(BalloonPoint - ray.origin, ray.direction).magnitude > reach) return false;
            }
            else
            {
                var b = room.BoundsOf(toy); b.Expand(.035f); if (!b.IntersectRay(ray)) return false;
                if (IsHornGame) hornTapKey = NearestHornKey(ray);   // the horn: whichever button is nearest the tap
            }
            BonusHeard(COgheBonusAnswer.Item, toy.Id);
            return true;
        }

        /// <summary>An answer during the game: a strike on the drum, or a gag for anything else.</summary>
        private void ToyHeard(COgheBonusAnswer answer, string item)
        {
            if (toy != null && answer == COgheBonusAnswer.Item && item == toy.Id)
            {
                if (IsBallGame) { BallTapped(); return; }
                if (IsHornGame) { HornTapped(Mathf.Max(0, hornTapKey)); return; }
                if (IsBalloonGame) { BalloonTapped(); return; }
                if (IsTopGame) { TopTapped(); return; }
                if (IsBoxGame) { BoxTapped(); return; }
                DrumStrike(true);
                if (bonusPhase == BonusPhase.ToyJam) { jamBoostAt = bonusTime; return; }
                if (bonusPhase != BonusPhase.ToyListen) return;   // while COghe plays, the drum sounds but does not count
                toyTaps++; toyLastHit = bonusTime;
                COgheAnalytics.Log("bonus_toy_tap", "beat", toyPattern + 1, "taps", toyTaps);
                if (toyTaps > ToyBeats) { BonusMisses++; StartGag(COgheGag.CoverEars); return; }   // one too many: too loud!
                if (toyTaps == ToyBeats) toyConfirmAt = bonusTime + ToyConfirm;                    // a moment to see no more come
                return;
            }
            if (bonusPhase == BonusPhase.ToyJam) { if (answer == COgheBonusAnswer.Touch) jamBoostAt = bonusTime; return; }   // all fun now
            if (bonusPhase != BonusPhase.ToyShow && bonusPhase != BonusPhase.ToyListen && bonusPhase != BonusPhase.ToyIntro) return;
            if (IsBallGame && (bonusPhase != BonusPhase.ToyListen || ballReturnAt >= 0)) return;   // the ball is on its way: not now
            BonusMisses++;
            StartGag(ChooseGag(answer, item, false, null));
        }

        /// <summary>Runs the game's phases; false if the phase is not one of them.</summary>
        private bool UpdateToy(float dt)
        {
            if (bonusPhase != BonusPhase.Gag)
            {
                if (IsBallGame && UpdateBallGame(dt)) return true;
                if (IsHornGame && UpdateHornGame(dt)) return true;
                if (IsBalloonGame && UpdateBalloonGame(dt)) return true;
                if (IsTopGame && UpdateTopGame(dt)) return true;
                if (IsBoxGame && UpdateBoxGame(dt)) return true;
            }
            switch (bonusPhase)
            {
                case BonusPhase.ToyIntro:
                    Time += dt; ToyIntroPose(bonusTime);
                    if (bonusTime >= ToyIntroLength) StartToyShow();
                    return true;
                case BonusPhase.ToyShow:
                {
                    Time += dt; var beats = DrumPattern(toyPattern);
                    ToyShowPose(bonusTime, beats);
                    if (bonusTime >= ShowLead + beats[beats.Length - 1] + .6f) StartToyListen();
                    return true;
                }
                case BonusPhase.ToyListen:
                    Time += dt; ToyListenPose(bonusTime);
                    if (toyConfirmAt >= 0 && bonusTime >= toyConfirmAt)
                    {   // played back: a hop and a thumbs-up, and on to the next beat
                        COgheAnalytics.Log("bonus_toy_beat", "beat", toyPattern + 1);
                        bonusPhase = BonusPhase.ToyGood; bonusTime = 0; Begin(COgheAct.Home, ToyGoodLength);
                        COgheAudio.Instance?.Play("creature_hi", .45f, 0, .1f);
                    }
                    else if (toyTaps == 0 && bonusTime > ToyReplayAfter) StartToyShow();   // nothing yet: once more
                    return true;
                case BonusPhase.ToyGood:
                    Time += dt; HopPose(bonusTime);
                    if (bonusTime < ToyGoodLength) return true;
                    toyPattern++;
                    if (toyPattern < DrumBeats.Length) StartToyShow();
                    else { bonusPhase = BonusPhase.ToyJam; bonusTime = 0; jamBoostAt = -100; Begin(COgheAct.Home, 1e6f); COgheAudio.Instance?.Play("creature_happy", .5f, 0, .1f); }
                    return true;
                case BonusPhase.ToyJam:
                    Time += dt; ToyJamPose(bonusTime);
                    if (bonusTime >= JamLength) WinToyGame();
                    return true;
                case BonusPhase.Gag:
                    Time += dt; GagPose(bonusTime);
                    if (bonusTime >= gagLength) EndGag();
                    return true;
            }
            return false;
        }

        /// <summary>The game is won: the toy is the player's, and the question is paid with COghe's joy.</summary>
        private void WinToyGame()
        {
            if (toy != null && COgheToys.Is(toy.Id)) { COgheToys.Give(toy.Id); room.MarkSeen(toy); room.ShowToy(toy, true); ResetToy(); }
            COgheAnalytics.Log("bonus_toy_won", "toy", toy != null ? toy.Id : "none");
            lastWord = default; roundDone = true;
            bonusPhase = BonusPhase.Joy; bonusTime = 0; Begin(COgheAct.Home, JoyLength);
            COgheAudio.Instance?.Play("creature_happy", .55f, 0, .15f);
        }

        /// <summary>The bonus ends (finished or left): a toy that is not the player's yet goes away again.</summary>
        private void EndToyGame()
        {
            if (gagBall != null) { game.HomeFeedBalls?.Consume(gagBall); gagBall = null; }
            if (gag == COgheGag.Bonk || IsBallGame) ResetBall();
            if (toy != null && room != null && COgheToys.Is(toy.Id)) { ResetToy(); room.ShowToy(toy, false); }
            toy = null; toyPop = -1; gag = COgheGag.None;
        }

        /// <summary>A toy tapped during a sentence (an answer or not): it answers with its own sound and a little move.</summary>
        private void ToyPoke(string id)
        {
            switch (id)
            {
                case COgheToys.Drum: DrumStrike(true); break;
                case COgheToys.Horn: HornKeyPress(rnd.Next(3), true); break;
                case COgheToys.Balloon: COgheAudio.Instance?.Play("balloon_puff", .5f, 0, .05f); break;
                case COgheToys.Top: topSpeed = Mathf.Max(topSpeed, 700); COgheAudio.Instance?.Play("top_whirr", .45f, 0, .1f); break;
                case COgheToys.MusicBox: boxCrankTarget += 45; boxDancer += 45; COgheAudio.Instance?.Play("mbox_" + rnd.Next(8), .5f, 0, .05f); break;
            }
        }

        private void DrumStrike(bool player)
        {
            drumHitAt = game.Matter.SimulationTime;
            COgheAudio.Instance?.Play(player ? "drum_hi" : "drum_lo", player ? .7f : .6f, 0, .03f);
        }

        /// <summary>Every Home frame: the drum's skin dips when struck, and a toy brought out pops up.</summary>
        private void StepToys(float dt)
        {
            var drum = room.Find(COgheToys.Drum);
            if (drum != null && drum.Root.activeSelf)
            {
                var head = drum.Part("Head");
                float since = game.Matter.SimulationTime - drumHitAt, impact = since >= 0 ? Mathf.Exp(-since * 20) : 0;   // the clock restarts with each visit
                // a shallow dip (the skin is .03 W thick: deeper and the dark inside of the shell would show)
                if (head != null) { head.localPosition = new Vector3(0, DrumHeadY - .012f * impact, 0); head.localScale = new Vector3(1 + .05f * impact, 1, 1 + .05f * impact); }
            }
            StepHorn(); StepTop(dt); StepMusicBox(dt);
            if (toyPop >= 0 && toy != null)
            {
                toyPop += dt; float k = Mathf.Clamp01(toyPop / .6f), pop = 1 + Mathf.Sin(k * Mathf.PI) * .25f * (1 - k);
                if (toyPop - dt <= 0) COgheAudio.Instance?.Play("creature_tada", .45f, 0, .1f);
                toy.Root.transform.localScale = Vector3.one * (COgheHomeItems.W * Smooth01(0, 1, k) * pop);
                if (k >= 1) toyPop = -1;
            }
        }
        private void ResetToy()
        {
            if (toy == null) return;
            toy.Root.transform.localScale = Vector3.one * COgheHomeItems.W; toyPop = -1;
            if (toy.Id == COgheToys.Drum) { var head = toy.Part("Head"); if (head != null) { head.localPosition = new Vector3(0, DrumHeadY, 0); head.localScale = Vector3.one; } }
            if (toy.Id == COgheToys.Horn) ResetHorn(toy);
            if (toy.Id == COgheToys.Balloon) ResetBalloons(toy);
            if (toy.Id == COgheToys.Top) ResetTop(toy);
            if (toy.Id == COgheToys.MusicBox) ResetMusicBox(toy);
        }

        // where a strike lands: the drum's skin, a little toward COghe; and where the tendril waits above it
        private Vector3 DrumSpot(COgheHomeItem drum, float across)
        {
            var head = drum.Part("Head"); Vector3 at = head != null ? head.position : drum.World(new Vector3(0, DrumHeadY, 0));
            Vector3 toward = Vector3.ProjectOnPlane(centre - at, up); toward = toward.sqrMagnitude > 1e-6f ? toward.normalized : CameraFlat();
            return at + up * .0035f + toward * .006f + Vector3.Cross(up, toward) * across;
        }
        private static float Strike(float t, float beat)
        {
            if (t < beat - .16f || t > beat + .24f) return 0;
            return t < beat ? Smooth01(beat - .16f, beat, t) : 1 - Smooth01(beat + .05f, beat + .24f, t);
        }
        private Vector3 TowardToy() { Vector3 d = Vector3.ProjectOnPlane(room.BoundsOf(toy).center - centre, up); return d.sqrMagnitude > 1e-6f ? d.normalized : Vector3.Cross(up, CameraFlat()); }
        private Quaternion Lean(Vector3 toward, float amount) => Quaternion.FromToRotation(up, Vector3.Slerp(up, toward, amount).normalized);

        private void ToyIntroPose(float t)
        {
            // "look what I've got!": it wiggles with excitement at the drum, raises a tendril high and gives it one big BOOM,
            // jumps at its own noise, then beams
            const float s = .036f;
            var p = StillPose(); if (toy == null) { Pose = p; return; }
            Vector3 toward = TowardToy();
            if (t < .85f) { p.Centre = centre + up * (s * .22f * Mathf.Abs(Mathf.Sin(t * 13))); p.Squash = 1 + .07f * Mathf.Sin(t * 26); }
            float raise = Smooth01(.85f, 1.2f, t) * (1 - Smooth01(1.3f, 1.4f, t)), arm = Smooth01(.8f, 1f, t) * (1 - Smooth01(1.75f, 1.95f, t));
            if (arm > .02f) { p.Tendrils = 1; p.TipA = Vector3.Lerp(centre + up * (s * .6f), DrumSpot(toy, 0) + up * (.004f + .05f * raise), arm); }
            float jump = Mathf.Sin(Smooth01(1.42f, 1.8f, t) * Mathf.PI);
            p.Centre += up * (s * 1.1f * jump); p.Squash *= 1 + .12f * jump;
            p.Turn = Lean(toward, .1f * Smooth01(.8f, 1.1f, t) * (1 - Smooth01(1.4f, 1.6f, t)));
            if (t > 1.8f) { float w = t - 1.8f; p.Turn = Quaternion.AngleAxis(8 * Mathf.Sin(w * 9) * Mathf.Exp(-w * 3), CameraFlat()); }
            if (Crossed(1.4f, 100, t)) { DrumStrike(false); COgheAudio.Instance?.Play("drum_lo", .4f, 0, 0); }
            if (Crossed(1.95f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .45f, 0, .1f);
            Pose = p;
        }

        private void ToyShowPose(float t, float[] beats)
        {
            // it leans over the drum and plays the beat with one tendril, bobbing on each strike
            const float s = .036f;
            var p = StillPose(); if (toy == null) { Pose = p; return; }
            Vector3 toward = TowardToy(); float lean = Smooth01(0, .25f, t), hit = 0;
            for (int k = 0; k < beats.Length; k++)
            {
                hit = Mathf.Max(hit, Strike(t, ShowLead + beats[k]));
                if (Crossed(ShowLead + beats[k], 100, t)) DrumStrike(false);
            }
            p.Centre = centre + toward * (s * .16f * lean) - up * (s * .08f * hit);
            p.Turn = Lean(toward, .1f * lean); p.Squash = 1 - .06f * hit;
            p.Tendrils = 1; p.TipA = Vector3.Lerp(centre + up * (s * .6f), DrumSpot(toy, 0) + up * (.03f * (1 - hit)), lean);
            Pose = p;
        }

        private void ToyListenPose(float t)
        {
            // "your turn!": it points at the drum, then sits back facing the player, swaying, and nods at every strike
            const float s = .036f;
            var p = StillPose(); if (toy == null) { Pose = p; return; }
            float point = Smooth01(.05f, .35f, t) * (1 - Smooth01(.9f, 1.2f, t));
            float nod = toyLastHit >= 0 ? Mathf.Exp(-(t - toyLastHit) * 9) * (t >= toyLastHit ? 1 : 0) : 0;
            p.Centre = centre + up * (s * .07f * Mathf.Max(0, Mathf.Sin(t * 3.1f))) - up * (s * .22f * nod);
            p.Turn = Quaternion.AngleAxis(4 * Mathf.Sin(t * 2.2f), CameraFlat()) * Lean(TowardToy(), .08f * nod);
            p.Squash = 1 - .08f * nod;
            if (point > .03f) { p.Tendrils = 1; p.TipA = Vector3.Lerp(centre + up * (s * .6f), DrumSpot(toy, 0) + up * .03f, point); }
            Pose = p;
        }

        private void ToyJamPose(float t)
        {
            // together: it grooves and drums on two spots in turn, two strikes a second; every tap of the player makes it jump
            const float s = .036f;
            var p = StillPose(); if (toy == null) { Pose = p; return; }
            Vector3 toward = TowardToy(); float hitA = 0, hitB = 0;
            float end = Smooth01(JamLength - .45f, JamLength, t);
            for (int k = 0; .3f + k * JamBeat < JamLength - .4f; k++)
            {
                float b = .3f + k * JamBeat, h = Strike(t, b);
                if ((k & 1) == 0) hitA = Mathf.Max(hitA, h); else hitB = Mathf.Max(hitB, h);
                if (Crossed(b, 100, t)) DrumStrike(false);
            }
            float bounce = .5f + .5f * Mathf.Cos(t / JamBeat * Mathf.PI * 2), boost = jamBoostAt >= 0 && t >= jamBoostAt ? Mathf.Sin(Mathf.Clamp01((t - jamBoostAt) / .4f) * Mathf.PI) : 0;
            p.Centre = centre + toward * (s * .14f) + up * (s * (.12f * bounce + .7f * boost));
            p.Turn = Quaternion.AngleAxis(9 * Mathf.Sin(t / JamBeat * Mathf.PI), CameraFlat()) * Lean(toward, .08f);
            p.Squash = 1 - .06f * (1 - bounce) + .1f * boost;
            float arms = 1 - end;
            if (arms > .03f)
            {
                p.Tendrils = 2;
                p.TipA = Vector3.Lerp(centre + up * (s * .6f), DrumSpot(toy, .011f) + up * (.028f * (1 - hitA)), arms);
                p.TipB = Vector3.Lerp(centre + up * (s * .6f), DrumSpot(toy, -.011f) + up * (.028f * (1 - hitB)), arms);
            }
            Pose = p;
        }

        /// <summary>At Home: a little groove on the drum with two tendrils, a big double strike, and a heart.</summary>
        private void DrumPlayPose(COgheHomeItem drum, float t, ref COgheSkinPose p)
        {
            const float s = .036f;
            float[] beats = { .5f, .75f, 1.0f, 1.5f, 1.75f, 2.0f, 2.5f, 2.75f, 3.0f, 3.25f, 3.7f, 3.85f };
            float hitA = 0, hitB = 0;
            for (int k = 0; k < beats.Length; k++)
            {
                float h = Strike(t, beats[k]);
                if ((k & 1) == 0 || k >= 10) hitA = Mathf.Max(hitA, h); if ((k & 1) == 1 || k >= 10) hitB = Mathf.Max(hitB, h);
                if (Crossed(beats[k], 100, t)) { drumHitAt = game.Matter.SimulationTime; COgheAudio.Instance?.Play("drum_lo", .5f, 0, .03f); }
            }
            float arms = Smooth01(.15f, .4f, t) * (1 - Smooth01(4.0f, 4.3f, t));
            Vector3 toward = Vector3.ProjectOnPlane(room.BoundsOf(drum).center - centre, up).normalized;
            float bounce = .5f + .5f * Mathf.Cos(t * Mathf.PI * 4);
            p.Centre = centre + toward * (s * .12f * arms) + up * (s * .1f * bounce * arms);
            p.Turn = Quaternion.AngleAxis(7 * Mathf.Sin(t * Mathf.PI * 2) * arms, CameraFlat());
            if (arms > .03f)
            {
                p.Tendrils = 2;
                p.TipA = Vector3.Lerp(centre + up * (s * .6f), DrumSpot(drum, .011f) + up * (.028f * (1 - hitA)), arms);
                p.TipB = Vector3.Lerp(centre + up * (s * .6f), DrumSpot(drum, -.011f) + up * (.028f * (1 - hitB)), arms);
            }
            float hop = Mathf.Sin(Smooth01(4.2f, 4.6f, t) * Mathf.PI);
            p.Centre += up * (s * .8f * hop);
            p.Morph = Smooth01(4.35f, 4.6f, t) * (1 - Smooth01(4.95f, 5.2f, t)); p.Shape = COgheShape.Heart;
            if (Crossed(4.3f, 100, t)) COgheAudio.Instance?.Play("creature_tada", .45f, 0, .2f);
        }

        // Gags ------------------------------------------------------------------------------------------------------------
        /// <summary>The comedy for an answer COghe did not want: about what it got (food, a touch, the ball), and for a touch,
        /// what it wanted instead.</summary>
        private COgheGag ChooseGag(COgheBonusAnswer answer, string item, bool tooMany, COgheBonusAsk? expected)
        {
            gagItem = null;
            if (answer == COgheBonusAnswer.Feed) return tooMany ? COgheGag.Hiccup : COgheGag.Spit;
            if (answer == COgheBonusAnswer.Touch)
            {
                if (tooMany) return COgheGag.GiggleRoll;
                if (InToyGame && toy != null) { gagItem = toy; return COgheGag.GigglePoint; }
                if (expected.HasValue && expected.Value.Answer == COgheBonusAnswer.Feed) return COgheGag.GiggleRumble;
                if (expected.HasValue && expected.Value.Answer == COgheBonusAnswer.Item) { gagItem = room.Find(expected.Value.Item); if (gagItem != null) return COgheGag.GigglePoint; }
                return COgheGag.GiggleRoll;
            }
            if (COgheToys.Is(item) && room.Find(item) != null) { gagItem = room.Find(item); return COgheGag.Startle; }   // a toy out of turn
            if (item == "BALL" && room.Find("BALL") != null) { gagItem = room.Find("BALL"); return COgheGag.Bonk; }
            return COgheGag.Nope;
        }

        public static float GagLength(COgheGag g)
        {
            switch (g)
            {
                case COgheGag.Spit: return 2.6f;
                case COgheGag.GiggleRumble: return 2.5f;
                case COgheGag.GigglePoint: return 2.3f;
                case COgheGag.Bonk: return 2.5f;
                case COgheGag.Hiccup: return 2.5f;
                case COgheGag.GiggleRoll: return 2.2f;
                case COgheGag.CoverEars: return 2.3f;
                case COgheGag.Startle: return 2.1f;
                case COgheGag.Blooper: return 1.9f;
                case COgheGag.Pop: return 2.0f;
                case COgheGag.TopFall: return 2.4f;
                default: return .9f;
            }
        }

        private void StartGag(COgheGag g)
        {
            gag = g; LastGag = g; GagsPlayed++; gagLength = GagLength(g);
            bonusPhase = BonusPhase.Gag; bonusTime = 0; Begin(COgheAct.Home, gagLength);
            COgheAnalytics.Log("bonus_gag", "gag", g.ToString(), "round", bonusRound + 1);
            if (g == COgheGag.Spit)
            {   // a ball drops in front of it, as for a right answer: it catches it… and spits it out
                var food = game.HomeFeedBalls; var toCam = CameraFlat(); Vector3 at = centre + toCam * .075f;
                gagFrom = OnFloor(at); gagBall = food?.Serve(gagFrom + up * .09f);
            }
            if (g == COgheGag.Bonk) { var ball = gagItem?.Part("Ball"); gagFrom = ball != null ? ball.position : centre; skinTop = SkinTopOver(OnFloor(centre)); }
            if (g == COgheGag.Nope) COgheAudio.Instance?.Play("creature_hm", .45f, 0, .15f);
            if (g == COgheGag.Blooper) COgheAudio.Instance?.Play("horn_honk", .7f, 0, .05f);
            if (g == COgheGag.TopFall) { topNeedsPull = true; topRescuedAt = -1; COgheAudio.Instance?.Play("top_clack", .6f, 0, .05f); }
            if (g == COgheGag.Pop)
            {   // the balloon bursts in its face
                var b = toy != null ? BalloonPart(toy, Mathf.Min(toyPattern, 2)) : null; if (b != null) b.localScale = Vector3.zero;
                COgheAudio.Instance?.Play("balloon_pop", .8f, 0, .05f);
            }
        }

        private void EndGag()
        {
            if (gagBall != null) { game.HomeFeedBalls?.Consume(gagBall); gagBall = null; }
            if (gag == COgheGag.Bonk) ResetBall();
            gag = COgheGag.None; EndAct();
            if (CurrentIsGame && IsBallGame && ballAtPlayer) StartToyListen();   // the ball still waits at the player's spot
            else if (CurrentIsGame) StartToyShow(); else Ask();
        }
        private void ResetBall()
        {
            var ball = room?.Find("BALL")?.Part("Ball");
            if (ball != null) { ball.localPosition = new Vector3(0, .45f, 0); ball.localRotation = Quaternion.identity; }
        }
        private Vector3 OnFloor(Vector3 p) => p - up * Vector3.Dot(p - floor, up) + up * COgheFeedBall.Radius;

        /// <summary>Where the eyes look in a gag (null: at the player).</summary>
        private Vector3? GagLook(out COgheMood mood)
        {
            float t = bonusTime;
            switch (gag)
            {
                case COgheGag.Spit:
                    mood = t < .62f ? COgheMood.Focus : t < 2.1f ? COgheMood.Sulky : COgheMood.Normal;
                    return t < .62f && gagBall != null ? gagBall.Position : (Vector3?)null;
                case COgheGag.GiggleRumble: mood = t < 1.0f ? COgheMood.Happy : COgheMood.Sad; return null;
                case COgheGag.GigglePoint:
                    mood = t < .9f ? COgheMood.Happy : COgheMood.Focus;
                    return t >= .9f && gagItem != null ? ItemPoint(gagItem) : (Vector3?)null;
                case COgheGag.Blooper: mood = t < .9f ? COgheMood.Surprised : COgheMood.Happy; return null;
                case COgheGag.Pop: mood = t < 1.0f ? COgheMood.Surprised : COgheMood.Happy; return null;
                case COgheGag.TopFall:
                    mood = t < .8f ? COgheMood.Surprised : t < 1.8f ? COgheMood.Hidden : COgheMood.Happy;
                    return t < .8f && toy != null ? room.BoundsOf(toy).center : (Vector3?)null;
                case COgheGag.Startle:
                    mood = t < .65f ? COgheMood.Surprised : t < 1.45f ? COgheMood.Sulky : COgheMood.Normal;
                    return t >= .65f && t < 1.45f && gagItem != null ? room.BoundsOf(gagItem).center : (Vector3?)null;
                case COgheGag.Bonk:
                    if (t < .62f) { mood = COgheMood.Focus; var ball = gagItem?.Part("Ball"); return ball != null ? ball.position : (Vector3?)null; }
                    mood = t < 1.9f ? COgheMood.Surprised : COgheMood.Happy;
                    return t < 1.9f ? SkinCentre + up * .02f + Quaternion.AngleAxis(t * 600, up) * CameraFlat() * .05f : (Vector3?)null;   // seeing stars
                case COgheGag.Hiccup: mood = t < 1.55f ? COgheMood.Surprised : COgheMood.Happy; return null;
                case COgheGag.GiggleRoll: mood = t > .45f && t < 1.45f ? COgheMood.Hidden : COgheMood.Happy; return null;
                case COgheGag.CoverEars: mood = t < 1.3f ? COgheMood.Sulky : COgheMood.Happy; return null;
                default: mood = COgheMood.Sad; return null;
            }
        }

        private void GagPose(float t)
        {
            const float s = .036f;
            var p = StillPose(); var toCam = CameraFlat(); var side = Vector3.Cross(up, toCam);
            float Giggle(float from, float to) { float e = Smooth01(from, from + .12f, t) * (1 - Smooth01(to - .15f, to, t)); return e; }
            switch (gag)
            {
                case COgheGag.Spit:
                {
                    // it catches the ball and pulls it in, then puffs its cheeks, chews… "ptoo!": out it flies, bouncing away;
                    // a shake of the head, "bleh"
                    Vector3 mouth = centre + toCam * (s * .7f) + up * (s * .35f), cheek = centre + toCam * (s * .45f) + up * (s * .25f);
                    Vector3 dir = (toCam * .85f + side * .5f).normalized;
                    Vector3 land1 = OnFloor(centre + dir * .15f), land2 = OnFloor(centre + dir * .2f), stop = OnFloor(centre + dir * .235f);
                    float pull = Smooth01(.3f, .62f, t), puff = Smooth01(.62f, .8f, t) * (1 - Smooth01(1.1f, 1.18f, t));
                    float recoil = t > 1.15f ? Mathf.Sin(Mathf.Clamp01((t - 1.15f) / .3f) * Mathf.PI) : 0;
                    Vector3 ballAt; float size = 1;
                    if (t < .22f) ballAt = Vector3.Lerp(gagFrom + up * .09f, gagFrom, (t / .22f) * (t / .22f));
                    else if (t < .62f) { ballAt = Vector3.Lerp(gagFrom, mouth, pull); size = Mathf.Lerp(1, .7f, pull); }
                    else if (t < 1.15f) { ballAt = cheek; size = .6f; }
                    else if (t < 1.5f) { float u = (t - 1.15f) / .35f; ballAt = Vector3.Lerp(mouth, land1, u) + up * (Mathf.Sin(u * Mathf.PI) * .05f); }
                    else if (t < 1.75f) { float u = (t - 1.5f) / .25f; ballAt = Vector3.Lerp(land1, land2, u) + up * (Mathf.Sin(u * Mathf.PI) * .014f); }
                    else { float u = Smooth01(1.75f, 2.2f, t); ballAt = Vector3.Lerp(land2, stop, 1 - (1 - u) * (1 - u)); size = 1 - Smooth01(2.2f, 2.5f, t); }
                    if (gagBall != null) gagBall.Place(ballAt, size);
                    p.Tendrils = t < .62f ? 1 : 0; p.TipA = t < .3f ? Vector3.Lerp(centre + toCam * s, gagFrom, Smooth01(.05f, .3f, t)) : ballAt;
                    float chew = t > .8f && t < 1.12f ? Mathf.Sin((t - .8f) * 30) : 0;
                    p.Centre = centre - toCam * (s * .3f * recoil);
                    p.Axis = Vector3.Slerp(up, toCam, .5f * puff).normalized;   // the cheeks bulge toward the player
                    p.Squash = 1 - .2f * puff + .04f * chew + .12f * recoil;
                    float shake = Smooth01(1.3f, 1.4f, t) * (1 - Smooth01(1.85f, 2.0f, t));
                    p.Turn = Quaternion.AngleAxis(16 * Mathf.Sin((t - 1.3f) * 21) * shake, up);
                    if (Crossed(.32f, 100, t)) COgheAudio.Instance?.Play("creature_grab", .35f, 0, .1f);
                    if (Crossed(.75f, 100, t)) COgheAudio.Instance?.Play("creature_hm", .4f, 0, .1f);
                    if (Crossed(1.15f, 100, t)) COgheAudio.Instance?.Play("creature_splat", .5f, 0, .1f);
                    if (Crossed(1.5f, 100, t) || Crossed(1.75f, 100, t)) COgheAudio.Instance?.Play("metal_clink", .35f, 0, .05f);
                    if (Crossed(1.4f, 100, t)) COgheAudio.Instance?.Play("creature_hmph", .45f, 0, .1f);
                    break;
                }
                case COgheGag.GiggleRumble:
                {
                    // ticklish! it giggles and wriggles… then its belly rumbles, and a tendril pats it: "food, please"
                    float g = Giggle(0, 1.05f), droop = Smooth01(1.0f, 1.3f, t) * (1 - Smooth01(2.2f, 2.5f, t));
                    float rumble = Smooth01(1.1f, 1.25f, t) * (1 - Smooth01(1.75f, 1.95f, t));
                    p.Squash = 1 + .15f * Mathf.Sin(t * 38) * g - .1f * droop;
                    p.Turn = Quaternion.AngleAxis(Mathf.Sin(t * 29) * 11 * g, toCam);
                    p.Centre = centre + up * (s * .15f * Mathf.Abs(Mathf.Sin(t * 13)) * g) + toCam * (s * .1f * Mathf.Sin(t * 26) * rumble);
                    float pat = Smooth01(1.25f, 1.45f, t) * (1 - Smooth01(2.15f, 2.4f, t));
                    if (pat > .03f)
                    {   // out in front of its belly (the body is ~5 cm round at Home), tapping it
                        Vector3 belly = centre + toCam * .06f + side * .018f - up * .004f;
                        p.Tendrils = 1; p.TipA = Vector3.Lerp(centre + up * (s * .5f), belly + toCam * (.012f * Mathf.Max(0, Mathf.Sin((t - 1.45f) * 14))), pat);
                    }
                    if (Crossed(.05f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .5f, 0, .1f);
                    if (Crossed(1.1f, 100, t)) COgheAudio.Instance?.Play("creature_grumble", .55f, 0, .1f);
                    break;
                }
                case COgheGag.GigglePoint:
                {
                    // ticklish! it giggles… then turns to what it wants and points at it, bouncing: "that one!"
                    float g = Giggle(0, .95f), point = Smooth01(.95f, 1.25f, t) * (1 - Smooth01(2.0f, 2.3f, t));
                    Vector3 at = gagItem != null ? ItemPoint(gagItem) : centre + toCam * .1f;
                    Vector3 toward = Vector3.ProjectOnPlane(at - centre, up); toward = toward.sqrMagnitude > 1e-6f ? toward.normalized : toCam;
                    float bob = Mathf.Max(0, Mathf.Sin((t - 1.3f) * 9)) * point;
                    p.Squash = 1 + .15f * Mathf.Sin(t * 38) * g + .06f * bob;
                    p.Turn = Quaternion.AngleAxis(Mathf.Sin(t * 29) * 11 * g, toCam) * Lean(toward, .12f * point);
                    p.Centre = centre + up * (s * (.15f * Mathf.Abs(Mathf.Sin(t * 13)) * g + .3f * bob)) + toward * (s * .2f * point);
                    if (point > .03f)
                    {
                        Vector3 from = centre + up * (s * .6f), aim = (at - from).normalized;
                        p.Tendrils = 1; p.TipA = from + aim * Mathf.Lerp(.01f, Mathf.Min(.075f, Vector3.Distance(from, at) * .7f), point) + up * (.004f * Mathf.Sin(t * 12));
                    }
                    if (Crossed(.05f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .5f, 0, .1f);
                    if (Crossed(1.0f, 100, t)) COgheAudio.Instance?.Play("creature_curious_1", .45f, 0, .1f);
                    if (Crossed(1.45f, 100, t)) COgheAudio.Instance?.Play("creature_ack_2", .4f, 0, .1f);
                    break;
                }
                case COgheGag.Bonk:
                {
                    // the ball hops over and bonks it on the head: it squashes flat, sees stars, wobbles; the ball rolls home
                    var ball = gagItem?.Part("Ball");
                    // the ball's centre a radius over the floor beside COghe, where it lands after the bonk
                    Vector3 head = centre + up * (skinTop * .96f), aside = OnFloor(centre + side * .075f + toCam * .02f) + up * (.45f * COgheHomeItems.W - COgheFeedBall.Radius);
                    Vector3 at = gagFrom; Vector3 rest = gagItem != null ? gagItem.World(new Vector3(0, .45f, 0)) : gagFrom;
                    if (t < .15f) at = rest + up * (.006f * Mathf.Abs(Mathf.Sin(t * 40)));
                    else if (t < .6f) { float u = (t - .15f) / .45f; at = Vector3.Lerp(rest, head, u) + up * (Mathf.Sin(u * Mathf.PI) * .07f); }
                    else if (t < 1.0f) { float u = (t - .6f) / .4f; at = Vector3.Lerp(head, aside, u) + up * (Mathf.Sin(u * Mathf.PI) * .04f); }
                    else { float u = Smooth01(1.15f, 2.2f, t); at = Vector3.Lerp(aside, rest, u); }
                    if (ball != null) { ball.position = at; ball.localRotation = Quaternion.AngleAxis(720 * Smooth01(.15f, 2.2f, t), side); }   // two whole turns: home as it was
                    float flat = t > .6f ? Mathf.Exp(-(t - .6f) * 5) : 0;
                    float dizzy = Smooth01(.8f, 1.0f, t) * (1 - Smooth01(1.7f, 1.95f, t));
                    Vector3 tilt = Quaternion.AngleAxis(t * 520, up) * side;
                    p.Squash = 1 - .45f * flat + .05f * Mathf.Sin((t - .6f) * 25) * flat;
                    p.Turn = Quaternion.AngleAxis(10 * dizzy, tilt);
                    if (t > 1.95f) { float w = t - 1.95f; p.Turn = Quaternion.AngleAxis(14 * Mathf.Sin(w * 30) * Mathf.Exp(-w * 6), up); }
                    if (Crossed(.15f, 100, t)) COgheAudio.Instance?.Play("creature_pop", .4f, 0, .1f);
                    if (Crossed(.6f, 100, t)) COgheAudio.Instance?.Play("lab_far_thud", .55f, 0, .1f);
                    if (Crossed(.65f, 100, t)) COgheAudio.Instance?.Play("creature_splat", .35f, 0, .1f);
                    if (Crossed(1.05f, 100, t)) COgheAudio.Instance?.Play("creature_hm", .4f, 0, .1f);
                    break;
                }
                case COgheGag.Hiccup:
                {
                    // too full: round and heavy, two hiccups that jolt it up, then a burp bubble that pops; relief
                    float full = Smooth01(0, .25f, t) * (1 - Smooth01(2.1f, 2.5f, t));
                    float hic = Mathf.Max(Mathf.Sin(Mathf.Clamp01((t - .3f) / .22f) * Mathf.PI), Mathf.Sin(Mathf.Clamp01((t - 1.0f) / .22f) * Mathf.PI));
                    p.Squash = 1 - .16f * full + .2f * hic;
                    p.Centre = centre + up * (s * .8f * hic);
                    p.Bubble = t > 1.5f && t < 2.32f ? (t - 1.5f) / .9f : -1;
                    if (Crossed(.3f, 100, t) || Crossed(1.0f, 100, t)) COgheAudio.Instance?.Play("creature_pop", .55f, 0, .05f);
                    if (Crossed(1.5f, 100, t)) COgheAudio.Instance?.Play("creature_gulp", .5f, 0, .1f);
                    if (Crossed(2.25f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .4f, 0, .1f);
                    break;
                }
                case COgheGag.GiggleRoll:
                {
                    // tickled too much: it giggles, rolls right over laughing, and giggles some more
                    float g = Mathf.Max(Giggle(0, .5f), Giggle(1.4f, 2.2f));
                    float roll = Smooth01(.45f, 1.45f, t), e = Mathf.Sin(Mathf.Clamp01((t - .45f) / 1f) * Mathf.PI);
                    p.Centre = centre + up * (s * .5f * e) + side * (s * .6f * e) + up * (s * .12f * Mathf.Abs(Mathf.Sin(t * 13)) * g);
                    p.Turn = Quaternion.AngleAxis(360 * roll, toCam) * Quaternion.AngleAxis(Mathf.Sin(t * 29) * 10 * g, toCam);
                    p.Squash = 1 + .15f * Mathf.Sin(t * 38) * g;
                    if (Crossed(.05f, 100, t) || Crossed(1.45f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .5f, 0, .1f);
                    if (Crossed(.5f, 100, t)) COgheAudio.Instance?.Play("creature_whoosh", .3f, 0, .1f);
                    break;
                }
                case COgheGag.CoverEars:
                {
                    // one strike too many: "too loud!" it crouches with both tendrils over its sides, shivering; then shakes it
                    // off and giggles
                    float cover = Smooth01(.05f, .25f, t) * (1 - Smooth01(1.2f, 1.45f, t));
                    float shiver = t > .2f && t < 1.25f ? Mathf.Sin(t * 60) : 0, off = t > 1.4f ? Mathf.Sin((t - 1.4f) * 30) * Mathf.Exp(-(t - 1.4f) * 5) : 0;
                    p.Squash = 1 - .2f * cover + .1f * off;
                    p.Centre = centre + side * (s * .04f * shiver * cover);
                    p.Turn = Quaternion.AngleAxis(12 * off, up);
                    if (cover > .03f)
                    {   // over its "ears": just outside the body on both sides, high up (the body is ~5 cm round at Home)
                        Vector3 top = centre + up * (s * .55f), ear = centre + up * .03f;
                        p.Tendrils = 2;
                        p.TipA = Vector3.Lerp(top, ear + side * .066f, cover);
                        p.TipB = Vector3.Lerp(top, ear - side * .066f, cover);
                    }
                    if (Crossed(.1f, 100, t)) COgheAudio.Instance?.Play("creature_hmph", .5f, 0, .1f);
                    if (Crossed(1.45f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .45f, 0, .1f);
                    break;
                }
                case COgheGag.Blooper:
                {
                    // the wrong button: the horn honks like a goose; COghe jumps back from it, squashed and wide-eyed, wobbles,
                    // then giggles
                    Vector3 toward = toy != null ? TowardHorn(toy) : toCam;
                    float jolt = Mathf.Sin(Smooth01(0, .3f, t) * Mathf.PI), flat = t > .25f ? Mathf.Exp(-(t - .25f) * 5) : 0;
                    float giggle = Giggle(1.0f, 1.9f);
                    p.Centre = centre - toward * (s * .5f * jolt) + up * (s * .6f * jolt);
                    p.Squash = 1 + .2f * jolt - .3f * flat + .06f * Mathf.Sin((t - .25f) * 28) * flat + .12f * Mathf.Sin(t * 38) * giggle;
                    p.Turn = Quaternion.AngleAxis(Mathf.Sin(t * 29) * 10 * giggle, toCam);
                    if (Crossed(1.05f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .45f, 0, .1f);
                    break;
                }
                case COgheGag.TopFall:
                {
                    // the top topples with a clatter; COghe gasps, tries spinning itself like a top, and wobbles dizzily
                    topFallen = Smooth01(0, .35f, t); topSpeed = Mathf.MoveTowards(topSpeed, 0, lastDt * 2500); topWobble = 1 - topFallen;
                    float gasp = Mathf.Sin(Mathf.Clamp01((t - .3f) / .5f) * Mathf.PI), twirl = Smooth01(.8f, 1.8f, t);
                    float dizzy = Smooth01(1.75f, 1.9f, t) * (1 - Smooth01(2.15f, 2.4f, t));
                    p.Squash = 1 + .18f * gasp + .12f * Mathf.Sin(twirl * Mathf.PI);
                    p.Turn = Quaternion.AngleAxis(720 * twirl, up) * Quaternion.AngleAxis(10 * dizzy, Quaternion.AngleAxis(t * 600, up) * side);
                    p.Centre = centre + up * (s * .3f * gasp);
                    if (Crossed(.35f, 100, t)) COgheAudio.Instance?.Play("creature_hm", .45f, 0, .1f);
                    if (Crossed(.85f, 100, t)) COgheAudio.Instance?.Play("creature_whoosh", .4f, 0, .1f);
                    if (Crossed(1.85f, 100, t)) COgheAudio.Instance?.Play("creature_hm", .35f, 0, .1f);
                    break;
                }
                case COgheGag.Pop:
                {
                    // one puff too many: BANG, the balloon bursts in its face; it is blown flat and back, blinks, then laughs
                    float blast = t < .5f ? Mathf.Exp(-t * 7) : 0, laugh = Giggle(.9f, 2.0f);
                    p.Centre = centre - toCam * (s * .4f * blast) + up * (s * .15f * Mathf.Abs(Mathf.Sin(t * 13)) * laugh);
                    p.Axis = Vector3.Slerp(up, toCam, .6f * blast).normalized; p.Squash = 1 - .4f * blast + .14f * Mathf.Sin(t * 38) * laugh;
                    p.Turn = Quaternion.AngleAxis(Mathf.Sin(t * 29) * 10 * laugh, toCam);
                    if (Crossed(.9f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .5f, 0, .1f);
                    break;
                }
                case COgheGag.Startle:
                {
                    // the drum goes BOOM when it did not ask for it: it leaps up in fright, lands flat, glares at the drum
                    // ("hmph"), then shrugs with both tendrils up
                    float leap = Mathf.Sin(Smooth01(0, .45f, t) * Mathf.PI), land = t > .45f ? Mathf.Exp(-(t - .45f) * 7) : 0;
                    float glare = Smooth01(.7f, .85f, t) * (1 - Smooth01(1.35f, 1.45f, t)), shrug = Smooth01(1.45f, 1.65f, t) * (1 - Smooth01(1.85f, 2.1f, t));
                    Vector3 toward = gagItem != null ? Vector3.ProjectOnPlane(room.BoundsOf(gagItem).center - centre, up).normalized : side;
                    p.Centre = centre + up * (s * 1.5f * leap) - toward * (s * .3f * leap);
                    p.Squash = 1 + .25f * leap - .35f * land + .05f * glare * Mathf.Sin(t * 30);
                    p.Turn = Lean(toward, .1f * glare);
                    if (shrug > .03f)
                    {
                        Vector3 top = centre + up * (s * .55f);
                        p.Tendrils = 2; p.TipA = Vector3.Lerp(top, top + side * .06f + up * .025f, shrug); p.TipB = Vector3.Lerp(top, top - side * .06f + up * .025f, shrug);
                        p.Centre += up * (s * .15f * shrug);
                    }
                    if (Crossed(.02f, 100, t)) COgheAudio.Instance?.Play("creature_whoosh", .4f, 0, .1f);
                    if (Crossed(.45f, 100, t)) COgheAudio.Instance?.Play("creature_land", .45f, 0, .1f);
                    if (Crossed(.8f, 100, t)) COgheAudio.Instance?.Play("creature_hmph", .45f, 0, .1f);
                    if (Crossed(1.5f, 100, t)) COgheAudio.Instance?.Play("creature_hm", .4f, 0, .1f);
                    break;
                }
                default:
                    NoPose(t); return;
            }
            Pose = p;
        }

        // The ball game (chapter 1's last question): passed back and forth --------------------------------------------------
        // COghe shows the ball word, the ball rolls over to it, and three times it nudges the ball down the aisle to the
        // player's spot; a tap on the ball sends it back, and COghe traps it. Then a little header show, and the ball rolls home.
        public const int BallPasses = 3;
        public const float BallIntroLength = 3.2f, BallPassLength = 1.6f, BallReturnLength = .9f, BallJamLength = 4.6f;
        private bool ballAtPlayer; private float ballReturnAt = -1; private Vector3 ballAt, ballFrom;
        private bool IsBallGame => toy != null && toy.Id == "BALL";
        private float BallRadius => .45f * COgheHomeItems.W;
        private Vector3 BallOnFloor(Vector3 p) => p - up * Vector3.Dot(p - floor, up) + up * BallRadius;
        private Vector3 BallRest => toy.World(new Vector3(0, .45f, 0));
        /// <summary>Where the player sends it back from: down the aisle toward the camera, clear of the dumbbell.</summary>
        private Vector3 BallPlayerSpot => BallOnFloor(room.Root.TransformPoint(new Vector3(-.17f, 0, -.3f)));
        /// <summary>Where COghe traps it: right in front of it, on the player's side.</summary>
        private Vector3 BallCatchSpot
        {
            get { Vector3 d = Vector3.ProjectOnPlane(BallPlayerSpot - centre, up); d = d.sqrMagnitude > 1e-6f ? d.normalized : CameraFlat(); return BallOnFloor(centre + d * .12f); }
        }
        private Transform BallPart => toy != null ? toy.Part("Ball") : null;
        /// <summary>Moves the ball; on the floor it rolls without slipping, in the air it spins.</summary>
        private void PlaceBall(Vector3 at, bool rolling)
        {
            var ball = BallPart; if (ball == null) return;
            Vector3 flat = Vector3.ProjectOnPlane(at - ballPrevious, up);
            if (rolling && flat.sqrMagnitude > 1e-10f) ballSpin = Quaternion.AngleAxis(flat.magnitude / BallRadius * Mathf.Rad2Deg, Vector3.Cross(up, flat).normalized) * ballSpin;
            else if (!rolling) ballSpin = Quaternion.AngleAxis(lastDt * 560, Vector3.Cross(up, CameraFlat())) * ballSpin;
            ballPrevious = at; ballAt = at;
            ball.position = at; ball.rotation = ballSpin;
        }
        private void StartBallGame()
        {
            var ball = BallPart; ballAt = ballPrevious = ballFrom = ball != null ? ball.position : BallRest; ballSpin = ball != null ? ball.rotation : Quaternion.identity;
            ballAtPlayer = false; ballReturnAt = -1;
        }
        private void BallTapped()
        {
            if (bonusPhase == BonusPhase.ToyJam) { jamBoostAt = bonusTime; return; }
            if (bonusPhase != BonusPhase.ToyListen || !ballAtPlayer || ballReturnAt >= 0) return;
            ballReturnAt = bonusTime; toyTaps = 1; ballFrom = ballAt;
            COgheAudio.Instance?.Play("creature_pop", .45f, 0, .05f);
            COgheAnalytics.Log("bonus_toy_tap", "pass", toyPattern + 1);
        }
        /// <summary>The ball game's phases (the drum's are in UpdateToy).</summary>
        private bool UpdateBallGame(float dt)
        {
            switch (bonusPhase)
            {
                case BonusPhase.ToyIntro:
                    Time += dt; BallIntroPose(bonusTime);
                    if (bonusTime >= BallIntroLength) StartToyShow();
                    return true;
                case BonusPhase.ToyShow:
                    Time += dt; BallPassPose(bonusTime);
                    if (bonusTime >= BallPassLength) { ballAtPlayer = true; ballReturnAt = -1; StartToyListen(); }
                    return true;
                case BonusPhase.ToyListen:
                    Time += dt; BallListenPose(bonusTime);
                    if (ballReturnAt >= 0 && bonusTime >= ballReturnAt + BallReturnLength)
                    {   // trapped: a hop and a thumbs-up, and the next pass
                        ballAtPlayer = false; ballReturnAt = -1;
                        COgheAnalytics.Log("bonus_toy_beat", "pass", toyPattern + 1);
                        bonusPhase = BonusPhase.ToyGood; bonusTime = 0; Begin(COgheAct.Home, ToyGoodLength);
                        COgheAudio.Instance?.Play("creature_hi", .45f, 0, .1f);
                    }
                    return true;
                case BonusPhase.ToyGood:
                    Time += dt; HopPose(bonusTime); PlaceBall(BallCatchSpot, true);
                    if (bonusTime < ToyGoodLength) return true;
                    toyPattern++;
                    if (toyPattern < BallPasses) StartToyShow();
                    else { bonusPhase = BonusPhase.ToyJam; bonusTime = 0; jamBoostAt = -100; ballFrom = ballAt; Begin(COgheAct.Home, 1e6f); COgheAudio.Instance?.Play("creature_happy", .5f, 0, .1f); }
                    return true;
                case BonusPhase.ToyJam:
                    Time += dt; BallJamPose(bonusTime);
                    if (bonusTime >= BallJamLength) { PlaceBall(BallRest, true); ResetBall(); WinToyGame(); }
                    return true;
            }
            return false;
        }

        private void BallIntroPose(float t)
        {
            // first the word: it makes the bouncing ball shape; then the real ball rolls round to it, and it hops for joy
            const float s = .036f;
            var p = StillPose();
            float form = Smooth01(0, .3f, t) * (1 - Smooth01(1.2f, 1.45f, t));
            if (form > 0)
            {
                float u = Mathf.Repeat(t / .7f, 1), height = 4 * u * (1 - u), land = Mathf.Exp(-Mathf.Min(u, 1 - u) * 18);
                p.Morph = form; p.Shape = COgheShape.Ball;
                p.Centre = centre + up * (s * 2.2f * height * form);
                p.Squash = (1 + .14f * Mathf.Abs(1 - 2 * u) * (1 - land) - .3f * land) * form + (1 - form);
                if (Crossed(.7f, .7f, t) && t < 1.3f) COgheAudio.Instance?.Play("creature_land", .2f, 0, .1f);
            }
            // round COghe's side (not through it) to the spot in front of it
            float k = Smooth01(1.5f, 2.6f, t);
            Vector3 side = BallOnFloor(centre + Vector3.Cross(up, CameraFlat()) * .13f + (BallRest - centre) * .25f);
            Vector3 a = Vector3.Lerp(ballFrom, side, k), b = Vector3.Lerp(side, BallCatchSpot, k);
            if (t >= 1.5f) PlaceBall(BallOnFloor(Vector3.Lerp(a, b, k)), true);
            float hop = Mathf.Sin(Smooth01(2.65f, 3.1f, t) * Mathf.PI);
            p.Centre += up * (s * .9f * hop); p.Squash *= 1 + .1f * hop;
            if (Crossed(1.5f, 100, t)) COgheAudio.Instance?.Play("creature_curious_1", .4f, 0, .1f);
            if (Crossed(2.65f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .45f, 0, .1f);
            Pose = p;
        }

        private void BallPassPose(float t)
        {
            // a wind-up, a nudge (the third time a header in an arc), and the ball rolls down to the player's spot
            const float s = .036f;
            var p = StillPose(); Vector3 to = BallPlayerSpot, from = BallCatchSpot;
            Vector3 dir = Vector3.ProjectOnPlane(to - from, up).normalized;
            float wind = Smooth01(0, .3f, t) * (1 - Smooth01(.3f, .42f, t)), push = Mathf.Sin(Smooth01(.32f, .6f, t) * Mathf.PI);
            p.Centre = centre - dir * (s * .25f * wind) + dir * (s * .35f * push);
            p.Squash = 1 - .15f * wind + .08f * push;
            p.Turn = Lean(dir, .1f * push - .06f * wind);
            float k = Mathf.Clamp01((t - .45f) / .95f), ease = 1 - (1 - k) * (1 - k);
            if (toyPattern == 2)
            {   // a header: up and over in an arc, one bounce
                Vector3 at = Vector3.Lerp(from, to, ease) + up * (Mathf.Sin(Mathf.Clamp01(k / .75f) * Mathf.PI) * .07f + (k > .75f ? Mathf.Sin((k - .75f) / .25f * Mathf.PI) * .012f : 0));
                p.Centre += up * (s * .5f * push);
                PlaceBall(t < .45f ? from : at, t < .45f || k >= .75f);
            }
            else
            {
                float bounce = toyPattern == 1 ? Mathf.Abs(Mathf.Sin(k * Mathf.PI * 3)) * .02f * (1 - k) : 0;   // the second pass hops along
                PlaceBall(t < .45f ? from : Vector3.Lerp(from, to, ease) + up * bounce, t < .45f || bounce < .002f);
            }
            if (Crossed(.45f, 100, t)) COgheAudio.Instance?.Play("creature_pop", .4f, 0, .1f);
            if (Crossed(1.4f, 100, t)) COgheAudio.Instance?.Play("creature_land", .25f, 0, .1f);
            Pose = p;
        }

        private void BallListenPose(float t)
        {
            // "your turn!": it bounces on the spot, eager; when the ball comes back it crouches and traps it
            const float s = .036f;
            var p = StillPose();
            if (ballReturnAt < 0)
            {
                float eager = Mathf.Abs(Mathf.Sin(t * 5)) * (Mathf.Repeat(t, 3f) < 1.2f ? 1 : .3f);
                p.Centre = centre + up * (s * .25f * eager); p.Squash = 1 + .05f * Mathf.Sin(t * 10);
                PlaceBall(BallPlayerSpot + up * (.004f * Mathf.Abs(Mathf.Sin(t * 3))), false);
                if (Crossed(4.5f, 4.5f, t)) COgheAudio.Instance?.Play("creature_curious_2", .3f, 0, .2f);
            }
            else
            {
                float k = Mathf.Clamp01((t - ballReturnAt) / BallReturnLength), ease = 1 - (1 - k) * (1 - k);
                PlaceBall(Vector3.Lerp(ballFrom, BallCatchSpot, ease), true);
                float trap = Mathf.Sin(Smooth01(.7f, 1f, k) * Mathf.PI);
                p.Squash = 1 - .2f * trap; p.Centre = centre - up * (s * .1f * trap);
                if (Crossed(ballReturnAt + BallReturnLength * .85f, 100, t)) COgheAudio.Instance?.Play("creature_grab", .4f, 0, .05f);
            }
            Pose = p;
        }

        private void BallJamPose(float t)
        {
            // the show: it flicks the ball up and heads it again and again (higher for every tap of the player), then pops it
            // back to its spot
            const float s = .036f;
            var p = StillPose(); float top = skinTop * .96f;
            Vector3 head = centre + up * top, rest = BallRest;
            const float period = .55f; float end = BallJamLength - .8f;
            if (t < .35f) { PlaceBall(Vector3.Lerp(ballFrom, head, Smooth01(0, .35f, t)) + up * (Mathf.Sin(t / .35f * Mathf.PI) * .03f), false); p.Squash = 1 - .15f * Smooth01(0, .3f, t); }
            else if (t < end)
            {
                float local = Mathf.Repeat(t - .35f, period), u = local / period;
                float boost = jamBoostAt >= 0 && t >= jamBoostAt ? Mathf.Exp(-(t - jamBoostAt) * 2.5f) : 0;
                PlaceBall(head + up * (4 * (.05f + .04f * boost) * u * (1 - u)), false);
                float hit = Mathf.Exp(-local * 14);
                p.Centre = centre - up * (s * .25f * hit) + up * (s * .5f * boost * Mathf.Sin(u * Mathf.PI)); p.Squash = 1 - .2f * hit;
                p.Turn = Quaternion.AngleAxis(Mathf.Sin(t * 9) * 6, CameraFlat());
                if (Crossed(.35f, period, t)) COgheAudio.Instance?.Play("creature_pop", .35f, 0, .05f);
            }
            else
            {
                float u = Mathf.Clamp01((t - end) / .7f);
                PlaceBall(Vector3.Lerp(head, rest, u) + up * (Mathf.Sin(u * Mathf.PI) * .08f), u >= 1);
                float hop = Mathf.Sin(Smooth01(end + .3f, end + .75f, t) * Mathf.PI); p.Centre = centre + up * (s * .8f * hop);
                if (Crossed(end, 100, t)) COgheAudio.Instance?.Play("creature_whoosh", .35f, 0, .1f);
            }
            Pose = p;
        }

    }
}
