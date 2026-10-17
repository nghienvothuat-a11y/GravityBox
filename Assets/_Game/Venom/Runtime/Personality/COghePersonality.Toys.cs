using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>What COghe does about a wrong answer in "Hiểu ra" (Mrk 17/10/2026: a comedy, not a telling-off).</summary>
    public enum COgheGag { None, Spit, GiggleRumble, GigglePoint, Bonk, Hiccup, GiggleRoll, CoverEars, Nope }

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
        public int ToyBeats => toyPattern < DrumBeats.Length ? DrumBeats[toyPattern].Length : 0;
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
                return Vector3.Lerp(SkinCentre, room.BoundsOf(toy).center, .5f) + up * .012f;
            }
        }
        /// <summary>The camera comes closer for a gag (the joke reads at a glance); for the ball's bonk it takes in the ball's way
        /// too, for the spat food the way it flies.</summary>
        public bool GagFrame(out Vector3 target, out float size)
        {
            target = SkinCentre + up * .01f; size = .27f;
            if (bonusPhase != BonusPhase.Gag || gag == COgheGag.Nope) return false;
            if (InToyGame && toy != null) { target = Vector3.Lerp(SkinCentre, room.BoundsOf(toy).center, .5f) + up * .012f; size = .29f; return true; }   // the drum stays in view
            if ((gag == COgheGag.Bonk || gag == COgheGag.GigglePoint) && gagItem != null) { target = Vector3.Lerp(target, room.BoundsOf(gagItem).center, .4f); size = .34f; }   // and what it is about
            if (gag == COgheGag.Spit) { target += CameraFlat() * .04f; size = .3f; }
            return true;
        }
        private bool CurrentIsGame => bonusRounds != null && bonusRound < BonusRounds && bonusRounds[bonusRound].IsGame;

        // The drum game ---------------------------------------------------------------------------------------------------
        private void StartToyGame()
        {
            toy = room.Find(bonusRounds[bonusRound].Toy); toyPattern = 0; toyTaps = 0; toyConfirmAt = -1; toyLastHit = -100;
            if (toy != null && !toy.Root.activeSelf)
            {   // brought out for the game: it pops up beside COghe
                room.ShowToy(toy, true); toy.Root.transform.localScale = Vector3.zero; toyPop = 0;
            }
            bonusPhase = BonusPhase.ToyIntro; bonusTime = 0; Begin(COgheAct.Home, 1e6f);
            COgheAudio.Instance?.Play("creature_curious_1", .4f, 0, .15f);
            COgheAnalytics.Log("bonus_toy_start", "toy", toy != null ? toy.Id : "none");
        }
        private void StartToyShow() { bonusPhase = BonusPhase.ToyShow; bonusTime = 0; toyTaps = 0; toyConfirmAt = -1; Begin(COgheAct.Home, 1e6f); }
        private void StartToyListen() { bonusPhase = BonusPhase.ToyListen; bonusTime = 0; toyTaps = 0; toyConfirmAt = -1; toyLastHit = -100; Begin(COgheAct.Home, 1e6f); }

        /// <summary>A tap on the screen during the game: the drum (generously around it) comes before COghe itself.</summary>
        public bool ToyTouch(Ray ray)
        {
            if (!InToyGame || toy == null || !toy.Root.activeInHierarchy) return false;
            var b = room.BoundsOf(toy); b.Expand(.035f);
            if (!b.IntersectRay(ray)) return false;
            BonusHeard(COgheBonusAnswer.Item, toy.Id);
            return true;
        }

        /// <summary>An answer during the game: a strike on the drum, or a gag for anything else.</summary>
        private void ToyHeard(COgheBonusAnswer answer, string item)
        {
            if (toy != null && answer == COgheBonusAnswer.Item && item == toy.Id)
            {
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
            BonusMisses++;
            StartGag(ChooseGag(answer, item, false, null));
        }

        /// <summary>Runs the game's phases; false if the phase is not one of them.</summary>
        private bool UpdateToy(float dt)
        {
            switch (bonusPhase)
            {
                case BonusPhase.ToyIntro:
                    Time += dt; ToyIntroPose(bonusTime);
                    if (bonusTime >= ToyIntroLength) StartToyShow();
                    return true;
                case BonusPhase.ToyShow:
                {
                    Time += dt; var beats = DrumBeats[toyPattern];
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
            if (toy != null) { COgheToys.Give(toy.Id); room.MarkSeen(toy); room.ShowToy(toy, true); ResetToy(); }
            COgheAnalytics.Log("bonus_toy_won", "toy", toy != null ? toy.Id : "none");
            lastWord = default; roundDone = true;
            bonusPhase = BonusPhase.Joy; bonusTime = 0; Begin(COgheAct.Home, JoyLength);
            COgheAudio.Instance?.Play("creature_happy", .55f, 0, .15f);
        }

        /// <summary>The bonus ends (finished or left): a toy that is not the player's yet goes away again.</summary>
        private void EndToyGame()
        {
            if (gagBall != null) { game.HomeFeedBalls?.Consume(gagBall); gagBall = null; }
            if (gag == COgheGag.Bonk) ResetBall();
            if (toy != null && room != null) { ResetToy(); room.ShowToy(toy, false); }
            toy = null; toyPop = -1; gag = COgheGag.None;
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
            if (drum == null || !drum.Root.activeSelf) return;
            var head = drum.Part("Head");
            float impact = Mathf.Exp(-(game.Matter.SimulationTime - drumHitAt) * 20);
            if (head != null) { head.localPosition = new Vector3(0, DrumHeadY - .035f * impact, 0); head.localScale = new Vector3(1 + .06f * impact, 1, 1 + .06f * impact); }
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
            var head = toy.Part("Head"); if (head != null) { head.localPosition = new Vector3(0, DrumHeadY, 0); head.localScale = Vector3.one; }
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
        }

        private void EndGag()
        {
            if (gagBall != null) { game.HomeFeedBalls?.Consume(gagBall); gagBall = null; }
            if (gag == COgheGag.Bonk) ResetBall();
            gag = COgheGag.None; EndAct();
            if (CurrentIsGame) StartToyShow(); else Ask();
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
                    return t >= .9f && gagItem != null ? room.BoundsOf(gagItem).center : (Vector3?)null;
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
                    Vector3 at = gagItem != null ? room.BoundsOf(gagItem).center : centre + toCam * .1f;
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
                default:
                    NoPose(t); return;
            }
            Pose = p;
        }
    }
}
