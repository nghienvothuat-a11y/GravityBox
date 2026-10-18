using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// The music box game, chapter 6's last question ("Hiểu ra" toys, Mrk 17/10/2026), the last chapter's celebration.
    /// COghe lifts the lid (a little dancer rises inside) and turns the crank: the tune's first two notes. Then it is the
    /// player's turn: each tap on the box turns the crank a quarter and plays the next two notes, the dancer twirls and COghe
    /// dances a step. Three taps finish the tune; three tunes, then the box plays on by itself while they dance, faster with
    /// every tap of the player, and the box is the player's. Nothing here can be wrong but a tap that is not on the box (the
    /// usual gags). At Home a tap on the box sends COghe to wind it and dance to a tune.
    /// </summary>
    public sealed partial class COghePersonality
    {
        private static readonly int[][] BoxTunes = { new[] { 0, 0, 3, 3, 4, 4, 3, 3 }, new[] { 2, 1, 0, 1, 2, 2, 2, 1 }, new[] { 0, 2, 3, 5, 7, 5, 3, 0 } };
        public const int BoxTurns = 3;   // taps of the player for each tune (COghe plays its first two notes)
        public const float BoxIntroLength = 2.4f, BoxShowLength = 1.5f, BoxStepLength = .55f, BoxFlourish = 1.3f;
        // the box's state, set by COghe's poses and drawn every Home frame (StepMusicBox)
        private float boxLid, boxCrank, boxCrankTarget, boxDancer, boxStepAt = -100;
        private int boxStep;   // dance steps so far (alternating sway and hop)
        private bool IsBoxGame => toy != null && toy.Id == COgheToys.MusicBox;
        private int[] BoxTune => BoxTunes[Mathf.Clamp(toyPattern, 0, BoxTunes.Length - 1)];

        /// <summary>Every Home frame: the lid, the crank turning to where it was wound, the dancer rising with the lid and
        /// twirling.</summary>
        private void StepMusicBox(float dt)
        {
            var box = room.Find(COgheToys.MusicBox);
            if (box == null || !box.Root.activeSelf) return;
            boxCrank = Mathf.MoveTowards(boxCrank, boxCrankTarget, dt * 420);
            var lid = box.Part("Lid"); if (lid != null) lid.localRotation = Quaternion.Euler(100 * boxLid, 0, 0);
            var crank = box.Part("Crank"); if (crank != null) crank.localRotation = Quaternion.Euler(boxCrank, 0, 0);
            var dancer = box.Part("Dancer");
            if (dancer != null) { float up01 = Smooth01(.4f, 1, boxLid); dancer.localScale = Vector3.one * up01; dancer.localRotation = Quaternion.Euler(0, boxDancer, 0); }
        }
        private void ResetMusicBox(COgheHomeItem box)
        {
            boxLid = boxCrank = boxCrankTarget = boxDancer = 0; boxStep = 0;
            var lid = box.Part("Lid"); if (lid != null) lid.localRotation = Quaternion.identity;
            var crank = box.Part("Crank"); if (crank != null) crank.localRotation = Quaternion.identity;
            var dancer = box.Part("Dancer"); if (dancer != null) { dancer.localScale = Vector3.zero; dancer.localRotation = Quaternion.identity; }
        }
        /// <summary>A quarter turn of the crank: two notes of the tune (from note <paramref name="from"/>), the dancer twirls.</summary>
        private void BoxTurn(int[] tune, int from, float volume)
        {
            boxCrankTarget += 90; boxDancer += 90;
            if (from < tune.Length) COgheAudio.Instance?.Play("mbox_" + tune[from], volume, 0, .02f);
            pendingNote = from + 1 < tune.Length ? tune[from + 1] : -1; pendingNoteAt = bonusTime + .26f; pendingVolume = volume;
        }
        private int pendingNote = -1; private float pendingNoteAt, pendingVolume;
        private void PlayPendingNote(float t)
        {
            if (pendingNote < 0 || t < pendingNoteAt) return;
            COgheAudio.Instance?.Play("mbox_" + pendingNote, pendingVolume, 0, .02f); pendingNote = -1;
        }

        /// <summary>A tap on the box: the crank turns a quarter, the next two notes (during the dance, a hop).</summary>
        private void BoxTapped()
        {
            if (bonusPhase == BonusPhase.ToyJam) { jamBoostAt = bonusTime; return; }
            if (bonusPhase != BonusPhase.ToyListen || toyTaps >= BoxTurns) return;
            toyTaps++; toyLastHit = bonusTime; boxStepAt = bonusTime; boxStep++;
            BoxTurn(BoxTune, 2 * toyTaps, .6f);
            COgheAnalytics.Log("bonus_toy_tap", "tune", toyPattern + 1, "turn", toyTaps);
            if (toyTaps == BoxTurns) toyConfirmAt = bonusTime + BoxStepLength;
        }

        private bool UpdateBoxGame(float dt)
        {
            switch (bonusPhase)
            {
                case BonusPhase.ToyIntro:
                    Time += dt; BoxOpenPose(bonusTime);
                    if (bonusTime >= BoxIntroLength) StartToyShow();
                    return true;
                case BonusPhase.ToyShow:
                    Time += dt; BoxCrankPose(bonusTime);
                    if (Crossed(.5f, 100, bonusTime)) BoxTurn(BoxTune, 0, .55f);
                    PlayPendingNote(bonusTime);
                    if (bonusTime >= BoxShowLength) StartToyListen();
                    return true;
                case BonusPhase.ToyListen:
                    Time += dt; PlayPendingNote(bonusTime); BoxDancePose(bonusTime);
                    if (toyConfirmAt >= 0 && bonusTime >= toyConfirmAt)
                    {   // the whole tune: a flourish while the dancer twirls
                        COgheAnalytics.Log("bonus_toy_beat", "tune", toyPattern + 1);
                        bonusPhase = BonusPhase.ToyGood; bonusTime = 0; Begin(COgheAct.Home, BoxFlourish);
                        COgheAudio.Instance?.Play("creature_happy", .45f, 0, .1f);
                    }
                    return true;
                case BonusPhase.ToyGood:
                    Time += dt; BoxFlourishPose(bonusTime);
                    if (bonusTime < BoxFlourish) return true;
                    toyPattern++;
                    if (toyPattern < BoxTunes.Length) StartToyShow();
                    else { bonusPhase = BonusPhase.ToyJam; bonusTime = 0; jamBoostAt = -100; Begin(COgheAct.Home, 1e6f); }
                    return true;
                case BonusPhase.ToyJam:
                    Time += dt; BoxJamPose(bonusTime, JamLength + .6f, true);
                    if (bonusTime >= JamLength + .6f) WinToyGame();
                    return true;
            }
            return false;
        }

        private Vector3 BoxCrankKnob(COgheHomeItem box) { var crank = box.Part("Crank"); return crank != null ? crank.TransformPoint(new Vector3(.08f, .14f, 0) / 1f) : box.World(new Vector3(.4f, .3f, 0)); }
        private Vector3 TowardBox(COgheHomeItem box) { Vector3 d = Vector3.ProjectOnPlane(room.BoundsOf(box).center - centre, up); return d.sqrMagnitude > 1e-6f ? d.normalized : CameraFlat(); }

        private void BoxOpenPose(float t)
        {
            // it peeks: lifts the lid with a tendril, the dancer rises; it beams, then points at the crank for the player
            const float s = .036f;
            var p = StillPose(); if (toy == null) { Pose = p; return; }
            Vector3 toward = TowardBox(toy);
            float reach = Smooth01(.4f, .7f, t) * (1 - Smooth01(1.3f, 1.5f, t));
            boxLid = Smooth01(.7f, 1.3f, t);
            float peek = Smooth01(1.2f, 1.5f, t) * (1 - Smooth01(1.9f, 2.2f, t));
            p.Centre = centre + toward * (s * (.2f * reach + .35f * peek)); p.Turn = Lean(toward, .12f * reach + .16f * peek);
            if (reach > .03f) { p.Tendrils = 1; p.TipA = Vector3.Lerp(centre + up * (s * .6f), toy.World(new Vector3(0, .5f + .3f * boxLid, -.2f)), reach); }
            if (Crossed(.9f, 100, t)) COgheAudio.Instance?.Play("mbox_5", .45f, 0, .05f);
            if (Crossed(1.5f, 100, t)) COgheAudio.Instance?.Play("creature_curious_1", .4f, 0, .1f);
            Pose = p;
        }

        private void BoxCrankPose(float t)
        {
            // it turns the crank once, a tendril on the knob going round with it
            const float s = .036f;
            var p = StillPose(); if (toy == null) { Pose = p; return; }
            Vector3 toward = TowardBox(toy);
            float on = Smooth01(.1f, .4f, t) * (1 - Smooth01(1.1f, 1.4f, t));
            p.Centre = centre + toward * (s * .2f * on); p.Turn = Lean(toward, .1f * on);
            if (on > .03f) { p.Tendrils = 1; p.TipA = Vector3.Lerp(centre + up * (s * .6f), BoxCrankKnob(toy), on); }
            Pose = p;
        }

        private void BoxDancePose(float t)
        {
            // the player's turn: a step of the dance with every quarter turn, swaying in between
            const float s = .036f;
            var p = StillPose(); var toCam = CameraFlat(); var side = Vector3.Cross(up, toCam);
            float step = boxStepAt >= 0 && t >= boxStepAt ? Mathf.Sin(Mathf.Clamp01((t - boxStepAt) / BoxStepLength) * Mathf.PI) : 0;
            bool hop = (boxStep & 1) == 0;
            p.Centre = centre + up * (s * (.08f * Mathf.Max(0, Mathf.Sin(t * 3)) + (hop ? .7f : .2f) * step)) + side * (s * (hop ? 0 : .35f * step));
            p.Turn = Quaternion.AngleAxis((hop ? 0 : 14) * step + 4 * Mathf.Sin(t * 2), toCam);
            p.Squash = 1 + .1f * step;
            Pose = p;
        }

        private void BoxFlourishPose(float t)
        {
            // the tune is complete: a whole turn on the spot, both tendrils up, while the dancer twirls
            const float s = .036f;
            var p = StillPose(); var toCam = CameraFlat(); var side = Vector3.Cross(up, toCam);
            float turn = Smooth01(.05f, .9f, t), arms = Mathf.Sin(Mathf.Clamp01(t / BoxFlourish) * Mathf.PI);
            boxDancer += lastDt * 600;
            p.Turn = Quaternion.AngleAxis(360 * turn, up); p.Centre = centre + up * (s * .5f * arms); p.Squash = 1 + .1f * arms;
            if (arms > .05f) { p.Tendrils = 2; p.TipA = centre + up * (s * .7f + .05f * arms) + side * (.05f * arms); p.TipB = centre + up * (s * .7f + .05f * arms) - side * (.05f * arms); }
            Pose = p;
        }

        /// <summary>The box plays on by itself (all three tunes' notes in turn), crank turning, dancer twirling; COghe dances,
        /// higher with every tap of the player. The lid closes at the end.</summary>
        private void BoxJamPose(float t, float length, bool taps)
        {
            const float s = .036f;
            var p = StillPose(); var toCam = CameraFlat(); var side = Vector3.Cross(up, toCam);
            float on = Smooth01(0, .3f, t) * (1 - Smooth01(length - .8f, length - .3f, t));
            const float beat = .3f; int k = Mathf.FloorToInt(t / beat);
            if (on > .3f && Crossed(0, beat, t)) { var tune = BoxTunes[(k / 8) % BoxTunes.Length]; COgheAudio.Instance?.Play("mbox_" + tune[k % 8], .45f, 0, .02f); boxCrankTarget += 45; boxDancer += 45; }
            if (t > length - .8f) boxLid = 1 - Smooth01(length - .8f, length - .3f, t);
            float boost = taps && jamBoostAt >= 0 && t >= jamBoostAt ? Mathf.Sin(Mathf.Clamp01((t - jamBoostAt) / .45f) * Mathf.PI) : 0;
            float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI / beat * .5f));
            p.Centre = centre + up * (s * (.25f * bounce * on + .8f * boost)) + side * (s * .3f * Mathf.Sin(t * 2.6f) * on);
            p.Turn = Quaternion.AngleAxis(12 * Mathf.Sin(t * 2.6f) * on, toCam) * Quaternion.AngleAxis(25 * Mathf.Sin(t * 1.3f) * on, up);
            p.Squash = 1 + .1f * bounce * on;
            if (on > .05f) { p.Tendrils = 2; p.TipA = centre + up * (s * .7f + .04f * on) + side * (.05f * on); p.TipB = centre + up * (s * .7f + .04f * (1 - bounce) * on) - side * (.05f * on); }
            Pose = p;
        }

        /// <summary>At Home: it opens the box, winds it, dances to a tune, closes it, and a heart.</summary>
        private void BoxPlayPose(COgheHomeItem box, float t, ref COgheSkinPose p)
        {
            const float s = .036f;
            boxLid = Smooth01(.2f, .7f, t) * (1 - Smooth01(4.6f, 5.0f, t));
            var tune = BoxTunes[0];
            if (t > .8f && t < 4.4f && Crossed(.8f, .45f, t)) { int k = Mathf.Clamp(Mathf.FloorToInt((t - .8f) / .45f), 0, 7); COgheAudio.Instance?.Play("mbox_" + tune[k], .45f, 0, .02f); boxCrankTarget += 45; boxDancer += 45; }
            float dance = Smooth01(.8f, 1.1f, t) * (1 - Smooth01(4.2f, 4.5f, t)), bounce = Mathf.Abs(Mathf.Sin(t * 3.5f));
            Vector3 side = Vector3.Cross(up, CameraFlat());
            p.Centre = centre + up * (s * .25f * bounce * dance) + side * (s * .3f * Mathf.Sin(t * 1.75f) * dance);
            p.Turn = Quaternion.AngleAxis(12 * Mathf.Sin(t * 1.75f) * dance, CameraFlat());
            float reach = Smooth01(.1f, .3f, t) * (1 - Smooth01(.7f, .9f, t));
            if (reach > .03f) { p.Tendrils = 1; p.TipA = Vector3.Lerp(centre + up * (s * .6f), box.World(new Vector3(0, .5f + .3f * boxLid, -.2f)), reach); }
            float hop = Mathf.Sin(Smooth01(4.9f, 5.3f, t) * Mathf.PI);
            p.Centre += up * (s * .8f * hop);
            p.Morph = Smooth01(5.05f, 5.3f, t) * (1 - Smooth01(5.65f, 5.9f, t)); p.Shape = COgheShape.Heart;
            if (Crossed(5.0f, 100, t)) COgheAudio.Instance?.Play("creature_tada", .45f, 0, .2f);
        }
    }
}
