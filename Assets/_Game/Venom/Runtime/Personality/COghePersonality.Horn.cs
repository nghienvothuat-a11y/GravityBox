using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// The horn game, chapter 3's last question ("Hiểu ra" toys, Mrk 17/10/2026; chapter 3 teaches order). COghe plays a
    /// little tune on the toy horn's three coloured buttons and the player plays the same buttons back in the same order: two
    /// notes, then three, then three in a new order. A wrong button blows a comic honk (COghe jumps back, wide-eyed), and
    /// COghe plays the tune again. All three played back: a fanfare together, each tap a note and a bounce; the horn is the
    /// player's. At Home a tap on the horn sends COghe to play its fanfare.
    /// </summary>
    public sealed partial class COghePersonality
    {
        private static readonly int[][] HornTunes = { new[] { 0, 2 }, new[] { 2, 0, 1 }, new[] { 1, 2, 0 } };
        private static readonly int[] HornFanfare = { 0, 1, 2, 2, 1, 2 };
        public const float HornIntroLength = 2.6f, HornNote = .55f;
        private readonly List<int> hornPlayed = new List<int>(4);
        private int hornKeyDown = -1, hornTapKey = -1; private float hornKeyAt = -100;
        private bool IsHornGame => toy != null && toy.Id == COgheToys.Horn;
        private int[] HornTune => HornTunes[Mathf.Clamp(toyPattern, 0, HornTunes.Length - 1)];

        private static Transform HornKey(COgheHomeItem horn, int i) => horn.Part("Key" + i);
        /// <summary>The button nearest a tap that landed on the horn: its whole area is shared out among the three.</summary>
        private int NearestHornKey(Ray ray)
        {
            int best = 0; float nearest = float.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                var key = HornKey(toy, i); if (key == null) continue;
                float d = Vector3.Cross(key.position - ray.origin, ray.direction).magnitude;
                if (d < nearest) { nearest = d; best = i; }
            }
            return best;
        }
        private void HornKeyPress(int key, bool player)
        {
            hornKeyDown = key; hornKeyAt = game.Matter.SimulationTime;
            COgheAudio.Instance?.Play("horn_" + key, player ? .65f : .55f, 0, .03f);
        }
        /// <summary>Every Home frame: the button pressed last sinks and comes back up.</summary>
        private void StepHorn()
        {
            var horn = room.Find(COgheToys.Horn);
            if (horn == null || !horn.Root.activeSelf) return;
            float since = game.Matter.SimulationTime - hornKeyAt;
            for (int i = 0; i < 3; i++)
            {
                var key = HornKey(horn, i); if (key == null) continue;
                float down = i == hornKeyDown && since >= 0 ? Mathf.Exp(-since * 9) : 0;
                key.localPosition = new Vector3(COgheHomeItems.HornKeyX[i], COgheHomeItems.HornKeyY - .05f * down, 0);
            }
        }
        private void ResetHorn(COgheHomeItem horn)
        {
            for (int i = 0; i < 3; i++) { var key = HornKey(horn, i); if (key != null) key.localPosition = new Vector3(COgheHomeItems.HornKeyX[i], COgheHomeItems.HornKeyY, 0); }
            hornKeyDown = -1;
        }

        /// <summary>A button the player pressed (or, during the fanfare, any tap on the horn).</summary>
        private void HornTapped(int key)
        {
            HornKeyPress(key, true);
            if (bonusPhase == BonusPhase.ToyJam) { jamBoostAt = bonusTime; return; }
            if (bonusPhase != BonusPhase.ToyListen || toyConfirmAt >= 0) return;   // while COghe plays, the horn sounds but does not count
            var tune = HornTune;
            if (key != tune[hornPlayed.Count]) { BonusMisses++; COgheAnalytics.Log("bonus_toy_tap", "tune", toyPattern + 1, "key", key, "correct", false); StartGag(COgheGag.Blooper); return; }
            hornPlayed.Add(key); toyTaps = hornPlayed.Count; toyLastHit = bonusTime;
            COgheAnalytics.Log("bonus_toy_tap", "tune", toyPattern + 1, "key", key, "correct", true);
            if (hornPlayed.Count == tune.Length) toyConfirmAt = bonusTime + .45f;   // the whole tune: a breath, then joy
        }

        private bool UpdateHornGame(float dt)
        {
            switch (bonusPhase)
            {
                case BonusPhase.ToyIntro:
                    Time += dt; HornIntroPose(bonusTime);
                    if (bonusTime >= HornIntroLength) StartToyShow();
                    return true;
                case BonusPhase.ToyShow:
                {
                    Time += dt; var tune = HornTune; HornShowPose(bonusTime, tune);
                    if (bonusTime >= ShowLead + tune.Length * HornNote + .35f) StartToyListen();
                    return true;
                }
                case BonusPhase.ToyListen:
                    Time += dt; HornListenPose(bonusTime);
                    if (toyConfirmAt >= 0 && bonusTime >= toyConfirmAt)
                    {
                        COgheAnalytics.Log("bonus_toy_beat", "tune", toyPattern + 1);
                        bonusPhase = BonusPhase.ToyGood; bonusTime = 0; Begin(COgheAct.Home, ToyGoodLength);
                        COgheAudio.Instance?.Play("creature_hi", .45f, 0, .1f);
                    }
                    else if (hornPlayed.Count == 0 && bonusTime > ToyReplayAfter) StartToyShow();   // nothing yet: the tune once more
                    return true;
                case BonusPhase.ToyGood:
                    Time += dt; HopPose(bonusTime);
                    if (bonusTime < ToyGoodLength) return true;
                    toyPattern++;
                    if (toyPattern < HornTunes.Length) StartToyShow();
                    else { bonusPhase = BonusPhase.ToyJam; bonusTime = 0; jamBoostAt = -100; Begin(COgheAct.Home, 1e6f); COgheAudio.Instance?.Play("creature_happy", .5f, 0, .1f); }
                    return true;
                case BonusPhase.ToyJam:
                    Time += dt; HornJamPose(bonusTime);
                    if (bonusTime >= JamLength) WinToyGame();
                    return true;
            }
            return false;
        }

        // the tendril over a button: just above it, pressing down by "hit"
        private Vector3 HornTip(COgheHomeItem horn, int key, float hit)
        {
            var k = HornKey(horn, key); Vector3 at = k != null ? k.position : room.BoundsOf(horn).center;
            return at + up * (.004f + .022f * (1 - hit));
        }
        private Vector3 TowardHorn(COgheHomeItem horn) { Vector3 d = Vector3.ProjectOnPlane(room.BoundsOf(horn).center - centre, up); return d.sqrMagnitude > 1e-6f ? d.normalized : CameraFlat(); }

        private void HornIntroPose(float t)
        {
            // "look!": it wiggles at the horn that popped up, leans over and tries its three buttons, low to high
            const float s = .036f;
            var p = StillPose(); if (toy == null) { Pose = p; return; }
            Vector3 toward = TowardHorn(toy);
            if (t < .9f) { p.Centre = centre + up * (s * .22f * Mathf.Abs(Mathf.Sin(t * 13))); p.Squash = 1 + .07f * Mathf.Sin(t * 26); }
            float lean = Smooth01(.85f, 1.1f, t) * (1 - Smooth01(2.2f, 2.5f, t)), hit = 0; int key = 0;
            for (int k = 0; k < 3; k++)
            {
                float b = 1.2f + k * .35f, h = Strike(t, b); if (h > hit) { hit = h; key = k; }
                if (t >= b - .17f) key = Mathf.Max(key, k);
                if (Crossed(b, 100, t)) HornKeyPress(k, false);
            }
            p.Centre += toward * (s * .3f * lean) - up * (s * .06f * hit); p.Turn = Lean(toward, .14f * lean);
            if (lean > .03f) { p.Tendrils = 1; p.TipA = Vector3.Lerp(centre + up * (s * .6f), HornTip(toy, key, hit), lean); }
            if (Crossed(2.3f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .45f, 0, .1f);
            Pose = p;
        }

        private void HornShowPose(float t, int[] tune)
        {
            // it leans over the horn and plays the tune, one button a beat, bobbing with each note
            const float s = .036f;
            var p = StillPose(); if (toy == null) { Pose = p; return; }
            Vector3 toward = TowardHorn(toy); float lean = Smooth01(0, .25f, t), hit = 0; int key = tune[0];
            for (int k = 0; k < tune.Length; k++)
            {
                float b = ShowLead + k * HornNote, h = Strike(t, b);
                if (t >= b - .17f) key = tune[k];
                hit = Mathf.Max(hit, h);
                if (Crossed(b, 100, t)) HornKeyPress(tune[k], false);
            }
            p.Centre = centre + toward * (s * .3f * lean) - up * (s * .07f * hit);
            p.Turn = Lean(toward, .14f * lean); p.Squash = 1 - .05f * hit;
            p.Tendrils = 1; p.TipA = Vector3.Lerp(centre + up * (s * .6f), HornTip(toy, key, hit), lean);
            Pose = p;
        }

        private void HornListenPose(float t)
        {
            // "your turn!": it points at the horn, then sits back facing the player and nods at every right note
            const float s = .036f;
            var p = StillPose(); if (toy == null) { Pose = p; return; }
            float point = Smooth01(.05f, .3f, t) * (1 - Smooth01(.8f, 1.1f, t));
            float nod = toyLastHit >= 0 && t >= toyLastHit ? Mathf.Exp(-(t - toyLastHit) * 9) : 0;
            p.Centre = centre + up * (s * .07f * Mathf.Max(0, Mathf.Sin(t * 3.1f))) - up * (s * .22f * nod);
            p.Turn = Quaternion.AngleAxis(4 * Mathf.Sin(t * 2.2f), CameraFlat());
            p.Squash = 1 - .08f * nod;
            if (point > .03f) { p.Tendrils = 1; p.TipA = Vector3.Lerp(centre + up * (s * .6f), HornTip(toy, 1, 0) + up * .01f, point); }
            Pose = p;
        }

        private void HornJamPose(float t)
        {
            // the fanfare, together: two tendrils play it while COghe bounces; every tap of the player makes it jump
            const float s = .036f;
            var p = StillPose(); if (toy == null) { Pose = p; return; }
            Vector3 toward = TowardHorn(toy); float hitA = 0, hitB = 0; int keyA = 0, keyB = 1;
            float end = Smooth01(JamLength - .45f, JamLength, t);
            for (int k = 0; .4f + k * .5f < JamLength - .5f; k++)
            {
                float b = .4f + k * .5f, h = Strike(t, b); int key = HornFanfare[k % HornFanfare.Length];
                if ((k & 1) == 0) { if (t >= b - .17f) keyA = key; hitA = Mathf.Max(hitA, h); } else { if (t >= b - .17f) keyB = key; hitB = Mathf.Max(hitB, h); }
                if (Crossed(b, 100, t)) HornKeyPress(key, false);
            }
            float bounce = .5f + .5f * Mathf.Cos(t / .5f * Mathf.PI * 2), boost = jamBoostAt >= 0 && t >= jamBoostAt ? Mathf.Sin(Mathf.Clamp01((t - jamBoostAt) / .4f) * Mathf.PI) : 0;
            p.Centre = centre + toward * (s * .25f) + up * (s * (.1f * bounce + .7f * boost));
            p.Turn = Quaternion.AngleAxis(8 * Mathf.Sin(t / .5f * Mathf.PI), CameraFlat()) * Lean(toward, .1f);
            p.Squash = 1 - .05f * (1 - bounce) + .1f * boost;
            float arms = 1 - end;
            if (arms > .03f)
            {
                p.Tendrils = 2;
                p.TipA = Vector3.Lerp(centre + up * (s * .6f), HornTip(toy, keyA, hitA), arms);
                p.TipB = Vector3.Lerp(centre + up * (s * .6f), HornTip(toy, keyB, hitB), arms);
            }
            Pose = p;
        }

        /// <summary>At Home: the fanfare on the horn, then a heart.</summary>
        private void HornPlayPose(COgheHomeItem horn, float t, ref COgheSkinPose p)
        {
            const float s = .036f;
            float arms = Smooth01(.15f, .4f, t) * (1 - Smooth01(3.6f, 3.9f, t)), hit = 0; int key = 0;
            for (int k = 0; k < HornFanfare.Length; k++)
            {
                float b = .5f + k * .5f, h = Strike(t, b);
                if (t >= b - .17f) key = HornFanfare[k];
                hit = Mathf.Max(hit, h);
                if (Crossed(b, 100, t)) { hornKeyDown = HornFanfare[k]; hornKeyAt = game.Matter.SimulationTime; COgheAudio.Instance?.Play("horn_" + HornFanfare[k], .5f, 0, .03f); }
            }
            Vector3 toward = TowardHorn(horn);
            p.Centre = centre + toward * (s * .25f * arms) - up * (s * .06f * hit);
            p.Turn = Lean(toward, .12f * arms);
            if (arms > .03f) { p.Tendrils = 1; p.TipA = Vector3.Lerp(centre + up * (s * .6f), HornTip(horn, key, hit), arms); }
            float hop = Mathf.Sin(Smooth01(3.8f, 4.2f, t) * Mathf.PI);
            p.Centre += up * (s * .8f * hop);
            p.Morph = Smooth01(3.95f, 4.2f, t) * (1 - Smooth01(4.55f, 4.8f, t)); p.Shape = COgheShape.Heart;
            if (Crossed(3.9f, 100, t)) COgheAudio.Instance?.Play("creature_tada", .45f, 0, .2f);
        }
    }
}
