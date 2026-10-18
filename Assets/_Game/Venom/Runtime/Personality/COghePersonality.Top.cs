using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// The spinning top game, chapter 5's last question ("Hiểu ra" toys, Mrk 17/10/2026). COghe winds a tendril round the
    /// top's handle and pulls: it spins. Soon it slows and starts to wobble, more and more; a tap on the top while it wobbles
    /// spins it up again (no clock to beat: it wobbles for a good few seconds). Three rescues: COghe twirls beside it like a
    /// top itself, faster with every tap of the player, and the top is the player's. Let it wobble too long and it falls
    /// with a clatter: COghe gasps, tries spinning itself and gets dizzy (a gag), stands the top up and spins it again.
    /// At Home a tap on the top sends COghe to spin it and twirl beside it.
    /// </summary>
    public sealed partial class COghePersonality
    {
        public const int TopRescues = 3;
        public const float TopIntroLength = 2.4f, TopSteady = 1.4f, TopWobbleTime = 4.2f, TopSpinUp = .7f;
        /// <summary>Later chapters: one rescue more per level (up to 5) and a quicker wobble (not under 2.8 s).</summary>
        private int TopRescuesNow => TopRescues + Mathf.Min(Mathf.Max(0, toyLevel), 2);
        private float TopWobbleNow => Mathf.Max(2.8f, TopWobbleTime - .4f * Mathf.Max(0, toyLevel));
        // the top's state, set by COghe's poses and drawn every Home frame (StepTop)
        private float topAngle, topSpeed, topWobble, topFallen, topPrecess, topRescuedAt = -1;
        private bool topNeedsPull;   // after a fall: stand it up and pull it again before it spins
        private bool IsTopGame => toy != null && toy.Id == COgheToys.Top;

        /// <summary>Every Home frame: the top turns at its speed, leans by its wobble (round and round), lies by its fall.</summary>
        private void StepTop(float dt)
        {
            var top = room.Find(COgheToys.Top);
            if (top == null || !top.Root.activeSelf) return;
            var spin = top.Part("Spin"); if (spin == null) return;
            // not in its game or its play at Home (a word's tap gave it a whizz): it winds down
            if (!(InToyGame && IsTopGame) && (playing == null || playing.Id != COgheToys.Top)) topSpeed = Mathf.MoveTowards(topSpeed, 0, dt * 300);
            topAngle = Mathf.Repeat(topAngle + topSpeed * dt, 360);
            topPrecess = Mathf.Repeat(topPrecess + (120 + 300 * topWobble) * dt, 360);
            Vector3 axis = Quaternion.AngleAxis(topPrecess, Vector3.up) * Vector3.right;
            spin.localRotation = Quaternion.AngleAxis(24 * topWobble * (1 - topFallen), axis) * Quaternion.AngleAxis(80 * topFallen, Vector3.forward)
                * Quaternion.AngleAxis(topAngle, Vector3.up);
        }
        private void ResetTop(COgheHomeItem top) { topSpeed = topWobble = topFallen = 0; var spin = top.Part("Spin"); if (spin != null) spin.localRotation = Quaternion.identity; }

        /// <summary>A tap on the top: while it wobbles, a rescue; spinning steadily, just a little extra whizz.</summary>
        private void TopTapped()
        {
            if (bonusPhase == BonusPhase.ToyJam) { jamBoostAt = bonusTime; topSpeed = Mathf.Min(1600, topSpeed + 200); return; }
            if (bonusPhase == BonusPhase.ToyShow) { topSpeed = Mathf.Min(1400, topSpeed + 150); return; }
            if (bonusPhase != BonusPhase.ToyListen || topRescuedAt >= 0) return;
            if (topWobble < .06f) { topSpeed = Mathf.Min(1400, topSpeed + 150); return; }   // not wobbling yet: no harm, a whizz
            topRescuedAt = bonusTime; toyTaps = 1;
            COgheAudio.Instance?.Play("top_whirr", .55f, 0, .05f); COgheAudio.Instance?.Play("creature_whoosh", .3f, 0, .05f);
            COgheAnalytics.Log("bonus_toy_tap", "rescue", toyPattern + 1);
        }

        private bool UpdateTopGame(float dt)
        {
            switch (bonusPhase)
            {
                case BonusPhase.ToyIntro:
                    Time += dt; TopPullPose(toy, bonusTime, TopIntroLength, 1.2f);
                    if (bonusTime >= TopIntroLength) StartToyShow();
                    return true;
                case BonusPhase.ToyShow:
                {
                    // spinning steadily (after a fall: stood up and pulled again first)
                    Time += dt; float length = topNeedsPull ? 1.2f + TopSteady : TopSteady;
                    if (topNeedsPull && bonusTime < 1.2f) TopPullPose(toy, bonusTime, 1.2f, .7f); else TopWatchPose(bonusTime);
                    topWobble = Mathf.MoveTowards(topWobble, 0, dt * 2);
                    if (bonusTime >= length) { topNeedsPull = false; StartToyListen(); topRescuedAt = -1; }
                    return true;
                }
                case BonusPhase.ToyListen:
                    Time += dt;
                    if (topRescuedAt < 0)
                    {   // it slows and wobbles more and more; fully over, it falls
                        topWobble = Mathf.Clamp01((bonusTime - .3f) / TopWobbleNow); topSpeed = Mathf.Lerp(900, 300, topWobble);
                        TopWatchPose(bonusTime);
                        if (topWobble >= 1) { BonusMisses++; StartGag(COgheGag.TopFall); }
                    }
                    else
                    {   // rescued: it straightens up and whizzes; COghe cheers
                        float k = Mathf.Clamp01((bonusTime - topRescuedAt) / TopSpinUp);
                        topWobble = Mathf.Lerp(topWobble, 0, k); topSpeed = Mathf.Lerp(topSpeed, 1100, k);
                        HopPose(Mathf.Min(bonusTime - topRescuedAt, .79f));
                        if (k >= 1)
                        {
                            COgheAnalytics.Log("bonus_toy_beat", "rescue", toyPattern + 1); topRescuedAt = -1;
                            toyPattern++;
                            if (toyPattern < TopRescuesNow) StartToyShow();
                            else { bonusPhase = BonusPhase.ToyJam; bonusTime = 0; jamBoostAt = -100; Begin(COgheAct.Home, 1e6f); COgheAudio.Instance?.Play("creature_happy", .5f, 0, .1f); }
                        }
                    }
                    return true;
                case BonusPhase.ToyJam:
                    Time += dt; TopTwirlPose(bonusTime, JamLength, true);
                    if (bonusTime >= JamLength) { topSpeed = 0; WinToyGame(); }
                    return true;
            }
            return false;
        }

        private Vector3 TopHandle(COgheHomeItem top) => top.World(new Vector3(0, .62f, 0));
        private Vector3 TowardTop(COgheHomeItem top) { Vector3 d = Vector3.ProjectOnPlane(room.BoundsOf(top).center - centre, up); return d.sqrMagnitude > 1e-6f ? d.normalized : CameraFlat(); }

        /// <summary>A tendril winds round the handle and pulls: the top spins up; COghe staggers back from the pull. From a
        /// fall the top is stood up first.</summary>
        private void TopPullPose(COgheHomeItem top, float t, float length, float pullAt)
        {
            const float s = .036f;
            var p = StillPose(); if (top == null) { Pose = p; return; }
            Vector3 toward = TowardTop(top);
            float reach = Smooth01(.1f, .45f, t) * (1 - Smooth01(pullAt, pullAt + .2f, t));
            if (topFallen > 0) topFallen = Mathf.Clamp01(1 - Smooth01(.1f, pullAt - .15f, t));
            float wind = Mathf.Sin(t * 14) * reach * (t > .45f ? 1 : 0);
            if (t < pullAt) topSpeed = Mathf.MoveTowards(topSpeed, 0, lastDt * 800);
            else if (topSpeed < 900) { topSpeed = 900; COgheAudio.Instance?.Play("top_whirr", .55f, 0, .05f); }
            float stagger = t > pullAt ? Mathf.Sin(Mathf.Clamp01((t - pullAt) / .5f) * Mathf.PI) : 0;
            p.Centre = centre + toward * (s * .25f * reach) - toward * (s * .45f * stagger);
            p.Turn = Lean(toward, .1f * reach - .12f * stagger); p.Squash = 1 - .1f * stagger;
            if (reach > .03f) { p.Tendrils = 1; p.TipA = Vector3.Lerp(centre + up * (s * .6f), TopHandle(top) + Vector3.Cross(up, toward) * (.006f * wind), reach); }
            if (Crossed(pullAt + .45f, 100, t) && length > 2) COgheAudio.Instance?.Play("creature_happy", .45f, 0, .1f);
            Pose = p;
        }

        private void TopWatchPose(float t)
        {
            // eyes on the top, swaying with its wobble, a little anxious as it leans more
            const float s = .036f;
            var p = StillPose();
            p.Turn = Quaternion.AngleAxis(10 * topWobble * Mathf.Sin(topPrecess * Mathf.Deg2Rad), CameraFlat());
            p.Centre = centre + up * (s * .06f * Mathf.Sin(t * 3)) ; p.Squash = 1 - .06f * topWobble;
            Pose = p;
        }

        /// <summary>COghe twirls like a top beside the spinning top, two tendrils out; every tap of the player in the game
        /// makes it hop higher. The top slows to a stop at the end.</summary>
        private void TopTwirlPose(float t, float length, bool taps)
        {
            const float s = .036f;
            var p = StillPose(); var toCam = CameraFlat(); var side = Vector3.Cross(up, toCam);
            float on = Smooth01(0, .4f, t) * (1 - Smooth01(length - .7f, length - .1f, t));
            float boost = taps && jamBoostAt >= 0 && t >= jamBoostAt ? Mathf.Sin(Mathf.Clamp01((t - jamBoostAt) / .45f) * Mathf.PI) : 0;
            topSpeed = Mathf.Lerp(0, taps ? Mathf.Max(topSpeed, 1000) : 1000, on); topWobble = 0;
            float spin = 360 * 1.4f * t * on;
            p.Turn = Quaternion.AngleAxis(spin, up);
            p.Squash = 1 + .12f * on; p.Centre = centre + up * (s * (.15f * Mathf.Abs(Mathf.Sin(t * 6)) * on + .8f * boost));
            if (on > .05f)
            {
                Vector3 a = Quaternion.AngleAxis(spin, up) * side;
                p.Tendrils = 2; p.TipA = centre + up * (s * .7f) + a * (.06f * on); p.TipB = centre + up * (s * .7f) - a * (.06f * on);
            }
            if (Crossed(.2f, 1.1f, t) && on > .5f) COgheAudio.Instance?.Play("top_whirr", .35f, 0, .1f);
            Pose = p;
        }

        /// <summary>At Home: it pulls the top into a spin, twirls beside it, the top winds down, and a heart.</summary>
        private void TopPlayPose(COgheHomeItem top, float t, ref COgheSkinPose p)
        {
            const float s = .036f;
            if (t < 1.2f) TopPullPose(top, t, 1.2f, .7f); else TopTwirlPose(t - 1.2f, 3.4f, false);
            var pose = Pose;
            p.Centre = pose.Centre; p.Turn = pose.Turn; p.Squash = pose.Squash; p.Tendrils = pose.Tendrils; p.TipA = pose.TipA; p.TipB = pose.TipB;
            if (t > 4.6f) topSpeed = 0;
            float hop = Mathf.Sin(Smooth01(4.6f, 5.0f, t) * Mathf.PI);
            p.Centre += up * (s * .8f * hop);
            p.Morph = Smooth01(4.75f, 5.0f, t) * (1 - Smooth01(5.35f, 5.6f, t)); p.Shape = COgheShape.Heart;
            if (Crossed(4.7f, 100, t)) COgheAudio.Instance?.Play("creature_tada", .45f, 0, .2f);
        }
    }
}
