using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>How the skin is posed for a Home act (world space). The physical body stays where it walked to.</summary>
    public struct COgheSkinPose
    {
        public float Blend;            // 0: skin on the body, 1: fully posed
        public Vector3 Centre;         // where the skin's centre goes
        public Quaternion Turn;        // rotation of the skin about its centre
        public Vector3 Axis; public float Squash;   // volume-preserving squash along Axis (1: none)
        public float Morph; public COgheShape Shape; public bool Hand;
        public int Tendrils; public Vector3 TipA, TipB;
        public float Bubble;           // snore bubble cycle 0..1, negative: none
    }

    /// <summary>
    /// COghe at Home (Mrk: it lives freely there, the player plays with it). It wanders the room, visits the furniture it
    /// has earned and plays with each piece, reacts when touched (and sulks in a corner when poked too much), and runs to
    /// a new item the first time it appears. Walking is real (the Home has no puzzle); each game with an item is a pose of
    /// the skin plus the item's own moving parts.
    /// </summary>
    public sealed partial class COghePersonality
    {
        private enum HomeState { Rest, Walk, Play, React, Sulk, User }
        private enum Reaction { Tickle, Hop, Heart, Roll, Flatten, Dodge }
        private HomeState home;
        private COgheHomeRoom room;
        private COgheHomeItem target, playing;
        private bool secondApproach;
        private float homeTimer, homeNext, stateTime, playLength, walkLimit, reactionUntil;
        private Reaction reaction; private Vector3 touchDirection;
        private readonly float[] touches = new float[4];
        private readonly List<COgheHomeItem> reveals = new List<COgheHomeItem>(16);
        private float revealClock; private bool revealVisit, revealPending;
        private readonly List<COgheHomeItem> choices = new List<COgheHomeItem>(16);
        private COgheHomeItem lastPlayed;
        private Vector3 sulkCorner;
        public COgheSkinPose Pose;
        public COgheHomeItem Playing => playing;
        public bool Sulking => home == HomeState.Sulk;
        /// <summary>Main menu: the room is hidden, so COghe stays put and only performs small acts on the spot.</summary>
        internal bool Showcase { get; set; }

        internal void EnterHome(COgheHomeRoom homeRoom)
        {
            room = homeRoom; home = HomeState.Rest; homeTimer = 0; homeNext = 1.2f; playing = target = null; EndAct();
            reveals.Clear(); revealClock = -.6f; revealVisit = false; revealPending = true;
            for (int i = 0; i < touches.Length; i++) touches[i] = -100;
        }
        internal void LeaveHome() { room = null; home = HomeState.Rest; playing = target = null; EndAct(); }

        /// <summary>The player tapped COghe itself.</summary>
        public void TouchedInHome(Vector3 point)
        {
            if (room == null) return;
            if (home == HomeState.Sulk) { COgheAudio.Instance?.Play("creature_hmph", .45f, 0, .6f); return; }   // it is not talking to you
            float now = game.Matter.SimulationTime;
            for (int i = touches.Length - 1; i > 0; i--) touches[i] = touches[i - 1];
            touches[0] = now;
            StopPlaying();
            if (now - touches[touches.Length - 1] < 3f) { StartSulk(); return; }
            touchDirection = Vector3.ProjectOnPlane(centre - point, up).normalized;
            reaction = (Reaction)rnd.Next(6);
            home = HomeState.React; stateTime = 0; Begin(COgheAct.Home, reaction == Reaction.Heart ? 1.5f : 1.1f);
            COgheAudio.Instance?.Play(reaction == Reaction.Tickle ? "creature_happy" : reaction == Reaction.Heart ? "creature_tada" : reaction == Reaction.Flatten ? "creature_pop" : "creature_hi", .5f, 0, .15f);
        }
        /// <summary>The player tapped a piece of furniture, or chose it in the item menu: go and play with it.</summary>
        public void PlayWith(COgheHomeItem item)
        {
            if (room == null || item == null || !room.Unlocked(item)) return;
            StopPlaying(); GoTo(item);
        }

        /// <summary>Previews and tests: put COghe at the item and start its game now.</summary>
        public void PlayNow(COgheHomeItem item)
        {
            if (room == null || item == null) return;
            StopPlaying(); FinishReveals(); TeleportBody(room.ApproachPoint(item)); StartPlaying(item);
        }

        private void UpdateHome(float dt)
        {
            room.Step();
            Reveal(dt);
            stateTime += dt;
            bool commanded = game.Feedback != null && game.Feedback.CommandCount != lastCommands;
            if (game.Feedback != null) lastCommands = game.Feedback.CommandCount;
            if ((commanded || game.HomeFeeding) && home != HomeState.User) { StopPlaying(); home = HomeState.User; stateTime = 0; }   // the player steers or feeds: step aside
            if (Act != COgheAct.None && Act != COgheAct.Home)
            {
                // a level act (wave, shape, melt) while resting
                float before = Time; Time += dt; if (!releasing) Cues(before, Time);
                if (commanded || Time >= Length) EndAct();
                return;
            }
            switch (home)
            {
                case HomeState.User:
                    if (stateTime > 6 && game.Motion.Get(0) == null && !game.HomeFeeding) { home = HomeState.Rest; homeTimer = 0; homeNext = 2; }
                    break;
                case HomeState.Rest:
                    homeTimer += dt;
                    if (revealVisit && !Showcase) { revealVisit = false; var newest = reveals[reveals.Count - 1]; reveals.Clear(); GoTo(newest); break; }
                    if (homeTimer < homeNext || !grounded) break;
                    ChooseHomeActivity();
                    break;
                case HomeState.Walk:
                    walkLimit -= dt;
                    Vector3 goal = room.ApproachPoint(target, secondApproach);
                    bool arrived = game.Motion.Get(0) == null && Vector3.ProjectOnPlane(game.Motion.Centre(0) - goal, up).magnitude < .035f;
                    if (arrived) StartPlaying(target);
                    else if (walkLimit <= 0 || game.Motion.Get(0) == null) { home = HomeState.Rest; homeTimer = 0; homeNext = 1.5f; }   // could not get there
                    break;
                case HomeState.Play:
                    Time += dt;
                    PlayPose(playing, Time);
                    if (Time >= Length) FinishPlaying();
                    break;
                case HomeState.React:
                    Time += dt; ReactPose(Time);
                    if (Time >= Length) { EndAct(); home = HomeState.Rest; homeTimer = 0; homeNext = 2.5f; }
                    break;
                case HomeState.Sulk:
                    SulkPose(dt);
                    break;
            }
        }

        private void ChooseHomeActivity()
        {
            homeTimer = 0; homeNext = 2f + (float)rnd.NextDouble() * 3f;
            if (Showcase)
            {
                homeNext = 5f + (float)rnd.NextDouble() * 4f;
                COgheAct small = rnd.Next(3) == 0 ? COgheAct.Wave : rnd.Next(2) == 0 ? COgheAct.Shape : COgheAct.Melt;
                if (small == COgheAct.Shape) Shape = PickShape();
                Begin(small, small == COgheAct.Melt ? 2.3f : 2.9f);
                return;
            }
            choices.Clear();
            foreach (var item in room.Items) if (room.Unlocked(item) && item != lastPlayed) choices.Add(item);
            float r = (float)rnd.NextDouble();
            if (choices.Count > 0 && r < .55f) { GoTo(choices[rnd.Next(choices.Count)]); return; }
            if (r < .8f)
            {
                game.Motion.Move(0, room.WanderPoint(rnd));
                return;
            }
            // a little show on the spot
            COgheAct act = rnd.Next(3) == 0 ? COgheAct.Wave : rnd.Next(2) == 0 ? COgheAct.Shape : COgheAct.Melt;
            if (act == COgheAct.Shape) Shape = PickShape();
            Begin(act, act == COgheAct.Melt ? 2.3f : 2.9f);
        }

        private void GoTo(COgheHomeItem item)
        {
            target = item; secondApproach = false; home = HomeState.Walk; walkLimit = 9;
            game.Motion.Move(0, room.ApproachPoint(item));
        }

        private void StartPlaying(COgheHomeItem item)
        {
            playing = item; lastPlayed = item; home = HomeState.Play;
            float length = item.Id switch
            {
                "BED" => 7f, "DUMBBELL" => 5.2f, "BALL" => 4.2f, "MIRROR" => 5.8f, "SWING" => 6.5f, "SLIDE" => 4.4f, "TV" => 6f,
                "XYLOPHONE" => 5f, "TRAMPOLINE" => 5.5f, "HAMMOCK" => 7f, "WHEEL" => 6f, "AQUARIUM" => 5.5f, "SHADOW_LAMP" => 6f, _ => 4.8f,
            };
            if (item.Id == "MIRROR" || item.Id == "SHADOW_LAMP") Shape = PickShape();
            Begin(COgheAct.Home, length);
            Pose = default; Pose.Turn = Quaternion.identity; Pose.Squash = 1; Pose.Axis = up; Pose.Bubble = -1;
        }

        private void FinishPlaying()
        {
            if (playing != null) ResetParts(playing);
            if (playing != null && playing.Id == "SLIDE") TeleportBody(room.ApproachPoint(playing, true));   // it ends at the bottom of the chute
            playing = null; EndAct(); home = HomeState.Rest; homeTimer = 0; homeNext = 1.5f + (float)rnd.NextDouble() * 2;
        }

        private void StopPlaying()
        {
            if (playing != null) ResetParts(playing);
            playing = null; target = null;
            if (Act != COgheAct.None) EndAct();
            if (home == HomeState.Walk || home == HomeState.Play) home = HomeState.Rest;
        }

        private void StartSulk()
        {
            home = HomeState.Sulk; stateTime = 0;
            // off to the nearest corner of the aisle
            float best = float.MaxValue;
            for (int i = 0; i < 4; i++) { var c = room.Corner(i); float d = (c - centre).sqrMagnitude; if (d < best) { best = d; sulkCorner = c; } }
            game.Motion.Move(0, sulkCorner);
            COgheAudio.Instance?.Play("creature_grumble", .55f, 0, .2f);
        }

        private void SulkPose(float dt)
        {
            bool there = game.Motion.Get(0) == null;
            if (there && Act != COgheAct.Home) { Begin(COgheAct.Home, 5.5f); }
            if (Act == COgheAct.Home)
            {
                Time += dt;
                float e = Smooth01(0, .5f, Time) * (1 - Smooth01(Length - .5f, Length, Time));
                var toCam = CameraFlat();
                // slumped, leaning away from the player, huffing now and then (a faceless body cannot turn its back)
                Pose.Blend = 1; Pose.Centre = centre - toCam * (.012f * e); Pose.Turn = Quaternion.AngleAxis((18 + 4 * Mathf.Sin(Time * 1.7f)) * e, Vector3.Cross(toCam, up));
                Pose.Axis = up; Pose.Squash = 1 - .22f * e + .04f * Mathf.Max(0, Mathf.Sin(Time * 2.4f)) * e;
                Pose.Morph = 0; Pose.Tendrils = 0; Pose.Bubble = -1;
                if (Time >= Length) { COgheAudio.Instance?.Play("creature_hmph", .5f, 0, .2f); EndAct(); home = HomeState.Rest; homeTimer = 0; homeNext = 2; }
            }
            else if (stateTime > 8) { home = HomeState.Rest; homeTimer = 0; }
        }

        // Touch reactions -------------------------------------------------------------------------------------------------------
        private void ReactPose(float t)
        {
            float u = Mathf.Clamp01(t / Length), e = Mathf.Sin(u * Mathf.PI);
            Pose.Blend = 1; Pose.Centre = centre; Pose.Turn = Quaternion.identity; Pose.Axis = up; Pose.Squash = 1; Pose.Morph = 0; Pose.Tendrils = 0; Pose.Bubble = -1;
            float s = .036f;
            switch (reaction)
            {
                case Reaction.Tickle: Pose.Squash = 1 + .18f * Mathf.Sin(t * 38) * e; Pose.Turn = Quaternion.AngleAxis(Mathf.Sin(t * 29) * 12 * e, CameraFlat()); break;
                case Reaction.Hop: Pose.Centre = centre + up * (s * 1.5f * Mathf.Sin(u * Mathf.PI)); Pose.Squash = u < .15f ? 1 - .25f * Mathf.Sin(u / .15f * Mathf.PI) : 1 + .15f * e; break;
                case Reaction.Heart: Pose.Morph = Smooth01(0, .3f, u) * (1 - Smooth01(.75f, 1, u)); Pose.Shape = COgheShape.Heart; break;
                case Reaction.Roll: Pose.Centre = centre + up * (s * .5f * e) + Vector3.Cross(up, CameraFlat()) * (s * .6f * e); Pose.Turn = Quaternion.AngleAxis(360 * Smooth01(0, 1, u), CameraFlat()); break;
                case Reaction.Flatten: Pose.Squash = u < .5f ? 1 - .55f * Smooth01(0, .25f, u) : .45f + .75f * Smooth01(.5f, .75f, u) - .2f * Smooth01(.75f, 1, u); break;
                case Reaction.Dodge: Pose.Centre = centre + (touchDirection.sqrMagnitude > .5f ? touchDirection : Vector3.Cross(up, CameraFlat())) * (s * 1.3f * e) + up * (s * .4f * e); Pose.Squash = 1 + .1f * e; break;
            }
        }

        // Playing with the furniture ------------------------------------------------------------------------------------------
        private void PlayPose(COgheHomeItem item, float t)
        {
            float s = .036f, L = Length;
            var p = new COgheSkinPose { Blend = 1, Centre = centre, Turn = Quaternion.identity, Axis = up, Squash = 1, Bubble = -1 };
            float hopIn = Smooth01(0, .5f, t), hopOut = Smooth01(L - .5f, L, t);
            Vector3 Hop(Vector3 from, Vector3 to, float k, float height) => Vector3.Lerp(from, to, k) + up * (Mathf.Sin(k * Mathf.PI) * height);
            Vector3 Onto(Vector3 anchor, float height) => hopOut > 0 ? Hop(anchor, centre, hopOut, height) : Hop(centre, anchor, hopIn, height);
            switch (item.Id)
            {
                case "BED":
                {
                    var cushion = item.Part("Cushion");
                    Vector3 bed = item.World(new Vector3(0, .25f, 0)) + up * (s * .55f);
                    p.Centre = Onto(bed, s * 1.2f);
                    float asleep = Smooth01(.6f, 1.2f, t) * (1 - Smooth01(L - 1f, L - .5f, t)), breath = Mathf.Sin(t * Mathf.PI * 2 / 3.2f);
                    p.Squash = 1 - .3f * asleep + .05f * breath * asleep;
                    p.Bubble = asleep > .6f ? Mathf.Repeat(t - 1.4f, 3.2f) / 1.6f : -1;
                    if (cushion != null) cushion.localScale = new Vector3(1, 1 - .04f * asleep * (.5f + .5f * breath), 1);
                    if (Crossed(1.4f, 3.2f, t)) COgheAudio.Instance?.Play("creature_snore", .3f, 0, .5f);
                    break;
                }
                case "DUMBBELL":
                {
                    var weights = item.Part("Weights");
                    float grip = Smooth01(.2f, .6f, t) * (1 - Smooth01(L - .6f, L - .2f, t));
                    float lift = t < .8f || t > L - .8f ? 0 : Mathf.Max(0, Mathf.Sin((t - .8f) / (L - 1.6f) * Mathf.PI * 3));
                    if (weights != null) weights.localPosition = new Vector3(0, .3f + .85f * lift, 0);
                    p.Squash = 1 - .12f * lift * grip + .03f * Mathf.Sin(t * 30) * lift;   // it strains a little at the top
                    p.Centre = centre + up * (s * .1f * lift);
                    p.Tendrils = grip > .05f ? 2 : 0;
                    p.TipA = item.World(new Vector3(-.25f, .3f + .85f * lift, 0)); p.TipB = item.World(new Vector3(.25f, .3f + .85f * lift, 0));
                    p.Blend = grip;
                    if (Crossed(.9f, (L - 1.6f) / 3, t)) COgheAudio.Instance?.Play("creature_grab", .45f, 0, .3f);
                    break;
                }
                case "BALL":
                {
                    var ball = item.Part("Ball");
                    Vector3 away = Vector3.ProjectOnPlane(item.World(new Vector3(0, .45f, 0)) - centre, up).normalized;
                    float bump = Mathf.Sin(Mathf.Clamp01((t - .3f) / .5f) * Mathf.PI);                   // it hops into the ball
                    float roll = Mathf.Sin(Mathf.Clamp01((t - .7f) / 3f) * Mathf.PI);                   // the ball rolls off and comes back
                    p.Centre = centre + away * (s * .9f * bump) + up * (s * .8f * bump);
                    p.Squash = 1 + .15f * bump;
                    if (ball != null)
                    {
                        ball.localPosition = new Vector3(0, .45f, 0) + item.Root.transform.InverseTransformDirection(away) * (1.8f * roll / .07f * .07f);
                        ball.localRotation = Quaternion.AngleAxis(roll * 300, item.Root.transform.InverseTransformDirection(Vector3.Cross(up, away)));
                    }
                    if (Crossed(.55f, 100, t)) COgheAudio.Instance?.Play("creature_pop", .45f, 0, .2f);
                    if (Crossed(1.2f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .45f, 0, .2f);
                    break;
                }
                case "MIRROR":
                {
                    var glass = item.Part("Mirror");
                    if (glass != null) glass.localRotation = Quaternion.Euler(Mathf.Sin(t * 1.3f) * 4, 0, 0);
                    // it poses in front of the mirror: one shape, back, another
                    float first = Smooth01(.8f, 1.3f, t) * (1 - Smooth01(2.6f, 3.0f, t)), second = Smooth01(3.1f, 3.6f, t) * (1 - Smooth01(L - .8f, L - .3f, t));
                    p.Morph = Mathf.Max(first, second); p.Shape = first > 0 ? Shape : (COgheShape)(((int)Shape + 3) % ShapeUnlocks.Length);
                    p.Turn = Quaternion.AngleAxis(Mathf.Sin(t * 2) * 8, up);
                    if (Crossed(.9f, 100, t) || Crossed(3.2f, 100, t)) COgheAudio.Instance?.Play("creature_tada", .45f, 0, .3f);
                    break;
                }
                case "SWING":
                {
                    var seat = item.Part("Seat");
                    float ride = Smooth01(.6f, 1.2f, t) * (1 - Smooth01(L - 1.1f, L - .6f, t));
                    float angle = Mathf.Sin((t - .6f) * 2.6f) * 18 * ride;
                    if (seat != null) seat.localRotation = Quaternion.Euler(angle, 0, 0);
                    Vector3 onSeat = seat != null ? seat.TransformPoint(new Vector3(0, -2.08f + .12f, 0)) + up * (s * .5f) : centre;
                    p.Centre = Onto(onSeat, s * 1.4f);
                    p.Turn = Quaternion.AngleAxis(angle * .6f, Vector3.Cross(up, CameraFlat()));
                    if (Crossed(.7f, 1.2f, t) && ride > .5f) COgheAudio.Instance?.Play("creature_swing", .35f, 0, .5f);
                    break;
                }
                case "SLIDE":
                {
                    // up the ladder, over the top, down the chute toward the player
                    Vector3 Path(float k)
                    {
                        if (k < .35f) { float u = k / .35f; return item.World(new Vector3(0, .05f + 2.35f * u, 1.62f + .35f * (1 - u))); }
                        if (k < .45f) { float u = (k - .35f) / .1f; return item.World(Vector3.Lerp(new Vector3(0, 2.5f, 1.55f), new Vector3(0, 2.45f, 1.0f), u)); }
                        float v = (k - .45f) / .55f;
                        return item.World(Vector3.Lerp(new Vector3(0, 2.4f, 1.0f), new Vector3(0, .25f, -2.3f), v) + new Vector3(0, Mathf.Sin(v * Mathf.PI) * -.25f, 0));
                    }
                    float kk = Smooth01(0, 1, Mathf.Clamp01(t / (L - .3f)));
                    p.Centre = Path(kk) + up * (s * .5f);
                    p.Axis = kk > .45f ? Vector3.Normalize(item.World(new Vector3(0, .25f, -2.3f)) - item.World(new Vector3(0, 2.4f, 1f))) : up;
                    p.Squash = kk > .45f && kk < .98f ? 1.35f : 1;
                    p.Blend = Smooth01(0, .12f, t);
                    if (Crossed(1.6f, 100, t)) COgheAudio.Instance?.Play("creature_whoosh", .5f, 0, .2f);
                    if (Crossed(L - .5f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .5f, 0, .2f);
                    break;
                }
                case "TV":
                {
                    var a = item.Part("ShapeA"); var b = item.Part("ShapeB");
                    if (a != null) { a.localPosition = new Vector3(-.3f + Mathf.Sin(t * 1.7f) * .25f, 1.1f + Mathf.Sin(t * 2.3f) * .12f, -.265f); a.localScale = Vector3.one * (1 + .25f * Mathf.Sin(t * 3)); }
                    if (b != null) { b.localPosition = new Vector3(.38f + Mathf.Sin(t * 2.1f + 1) * .2f, .88f + Mathf.Cos(t * 2.7f) * .1f, -.266f); }
                    float jump = Mathf.Sin(Mathf.Clamp01((t - 3f) / .6f) * Mathf.PI);
                    p.Centre = centre + up * (s * 1.2f * jump); p.Squash = 1 + .2f * jump - .06f * Mathf.Sin(t * 5) * (1 - jump);
                    p.Turn = Quaternion.AngleAxis(Mathf.Sin(t * 2.2f) * 10, up);
                    if (Crossed(3f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .5f, 0, .2f);
                    break;
                }
                case "XYLOPHONE":
                {
                    // two tendrils play a little tune, one key at a time
                    int[] tune = Tune; float[] xs = KeyX;
                    float beat = (t - .5f) / .45f; int note = Mathf.Clamp(Mathf.FloorToInt(beat), 0, tune.Length - 1);
                    float strike = t < .5f || beat >= tune.Length ? 0 : Mathf.Sin(Mathf.Clamp01(beat - Mathf.Floor(beat)) * Mathf.PI);
                    int key = tune[note];
                    for (int i = 0; i < 6; i++) { var k = item.Part(KeyNames[i]); if (k != null) k.localPosition = new Vector3(xs[i], .24f - (i == key ? .03f * strike : 0), 0); }
                    Vector3 hit = item.World(new Vector3(xs[key], .3f + .3f * (1 - strike), 0));
                    p.Tendrils = 1; p.TipA = hit; p.Blend = Smooth01(0, .4f, t) * (1 - Smooth01(L - .4f, L, t));
                    p.Turn = Quaternion.AngleAxis(Mathf.Sin(t * 4.4f) * 6, up);
                    if (t >= .5f && beat < tune.Length && Crossed(.5f + .45f * .5f, .45f, t)) COgheAudio.Instance?.Play(NoteNames[tune[Mathf.Clamp(Mathf.FloorToInt((t - .5f - .2f) / .45f), 0, tune.Length - 1)]], .55f, 0, .1f);
                    break;
                }
                case "TRAMPOLINE":
                {
                    var mat = item.Part("Mat");
                    Vector3 top = item.World(new Vector3(0, .56f, 0)) + up * (s * .5f);
                    float bounceT = Mathf.Clamp01((t - .5f) / (L - 1f));
                    float n = bounceT * 4, phase = n - Mathf.Floor(n), height = (Mathf.Floor(n) + 1) * .6f;
                    float air = bounceT >= 1 || t < .5f ? 0 : Mathf.Sin(phase * Mathf.PI);
                    float sag = t < .5f || bounceT >= 1 ? 0 : Mathf.Max(0, 1 - phase * 6) + Mathf.Max(0, phase * 6 - 5);
                    p.Centre = Onto(top + up * (s * height * air - s * .3f * sag), s * 1.3f);
                    p.Squash = 1 - .3f * sag + .15f * air;
                    if (Mathf.Floor(n) >= 3 && bounceT < 1) p.Turn = Quaternion.AngleAxis(360 * phase, Vector3.Cross(up, CameraFlat()));
                    if (mat != null) mat.localScale = new Vector3(1, 1 + 13 * sag, 1);
                    if (t > .5f && bounceT < 1 && Crossed(.5f, (L - 1f) / 4, t)) COgheAudio.Instance?.Play("creature_pop", .45f, 0, .15f);
                    break;
                }
                case "HAMMOCK":
                {
                    var sling = item.Part("Sling");
                    float rest = Smooth01(.6f, 1.2f, t) * (1 - Smooth01(L - 1f, L - .5f, t));
                    float rock = Mathf.Sin(t * 1.6f) * 8 * rest;
                    if (sling != null) sling.localRotation = Quaternion.Euler(rock, 0, 0);
                    Vector3 bed = sling != null ? sling.TransformPoint(new Vector3(0, .68f - 1.38f + .05f, 0)) + up * (s * .35f) : centre;
                    p.Centre = Onto(bed, s * 1.3f);
                    p.Squash = 1 - .35f * rest + .04f * Mathf.Sin(t * 2) * rest;
                    p.Bubble = rest > .6f ? Mathf.Repeat(t - 1.6f, 3.2f) / 1.6f : -1;
                    if (Crossed(1.6f, 3.2f, t)) COgheAudio.Instance?.Play("creature_snore", .28f, 0, .5f);
                    break;
                }
                case "WHEEL":
                {
                    var drum = item.Part("Drum");
                    float run = Smooth01(.6f, 1.1f, t) * (1 - Smooth01(L - 1f, L - .6f, t));
                    if (drum != null) drum.localRotation = Quaternion.Euler(0, 0, (t - .6f) * -260 * run);
                    Vector3 inside = item.World(new Vector3(0, .41f, -.1f)) + up * (s * .45f);
                    p.Centre = Onto(inside, s * 1.4f) + up * (Mathf.Abs(Mathf.Sin(t * 16)) * s * .12f * run);
                    p.Squash = 1 + .1f * Mathf.Sin(t * 32) * run;
                    p.Turn = Quaternion.AngleAxis(-8 * run, Vector3.Cross(up, CameraFlat()));
                    if (Crossed(.8f, .9f, t) && run > .5f) COgheAudio.Instance?.Play("creature_slide", .2f, 0, .3f);
                    break;
                }
                case "AQUARIUM":
                {
                    var fish = item.Part("Fish");
                    float swim = t * 1.4f;
                    if (fish != null) { fish.localPosition = new Vector3(Mathf.Sin(swim) * .55f, .88f + Mathf.Sin(swim * 2.2f) * .12f, Mathf.Cos(swim) * .2f); fish.localRotation = Quaternion.Euler(0, Mathf.Cos(swim) > 0 ? 180 : 0, 0); }
                    Vector3 glassFront = item.World(new Vector3(0, .9f, -.7f));
                    Vector3 press = Vector3.Lerp(centre, new Vector3(glassFront.x, centre.y + s * .6f, glassFront.z) - Vector3.ProjectOnPlane(item.Root.transform.forward, up).normalized * (-s * .3f), .7f);
                    float stick = Smooth01(.3f, .8f, t) * (1 - Smooth01(L - .7f, L - .2f, t));
                    p.Centre = Vector3.Lerp(centre, press, stick);
                    p.Axis = -Vector3.ProjectOnPlane(item.Root.transform.forward, up).normalized; p.Squash = 1 - .35f * stick;
                    if (Crossed(.8f, 100, t)) COgheAudio.Instance?.Play("glass_tok", .35f, 0, .2f);
                    break;
                }
                case "SHADOW_LAMP":
                {
                    var head = item.Part("Head");
                    if (head != null) head.localRotation = Quaternion.Euler(Mathf.Sin(t * .8f) * 4, 0, 0);
                    float first = Smooth01(.7f, 1.2f, t) * (1 - Smooth01(2.6f, 3f, t)), second = Smooth01(3.1f, 3.6f, t) * (1 - Smooth01(L - .8f, L - .3f, t));
                    p.Morph = Mathf.Max(first, second); p.Shape = first > 0 ? Shape : (COgheShape)(((int)Shape + 5) % ShapeUnlocks.Length);
                    if (Crossed(.8f, 100, t) || Crossed(3.2f, 100, t)) COgheAudio.Instance?.Play("creature_tada", .4f, 0, .3f);
                    break;
                }
                case "TROPHY":
                {
                    float beat = t * Mathf.PI * 3.2f, hop = Mathf.Max(0, Mathf.Sin(beat));
                    p.Centre = centre + up * (s * .9f * hop) + Vector3.Cross(up, CameraFlat()) * (Mathf.Sin(beat * .5f) * s * .5f);
                    p.Squash = 1 + .18f * hop; p.Turn = Quaternion.AngleAxis(Mathf.Sin(beat * .5f) * 15, CameraFlat());
                    p.Blend = Smooth01(0, .3f, t) * (1 - Smooth01(L - .3f, L, t));
                    if (Crossed(.3f, 100, t)) COgheAudio.Instance?.Play("game_win", .35f, 0, .5f);
                    break;
                }
            }
            if (item.Id != "DUMBBELL" && item.Id != "XYLOPHONE" && item.Id != "SLIDE" && item.Id != "TROPHY") p.Blend = 1;
            Pose = p;
        }

        private void ResetParts(COgheHomeItem item)
        {
            foreach (var kv in item.Parts)
            {
                kv.Value.localRotation = Quaternion.identity; kv.Value.localScale = Vector3.one;
            }
            var weights = item.Part("Weights"); if (weights != null) weights.localPosition = new Vector3(0, .3f, 0);
            var ball = item.Part("Ball"); if (ball != null) ball.localPosition = new Vector3(0, .45f, 0);
            var fish = item.Part("Fish"); if (fish != null) fish.localPosition = new Vector3(0, .88f, 0);
        }

        private void Reveal(float dt)
        {
            if (revealPending && !Showcase)
            {
                // furniture earned since the last visit pops into place, one after another; COghe then runs to the newest
                // (not in the main menu, where the room is hidden: the reveal waits for the real visit)
                revealPending = false;
                foreach (var item in room.Newly()) { reveals.Add(item); item.Root.transform.localScale = Vector3.zero; room.MarkSeen(item); }
            }
            if (reveals.Count == 0) return;
            float before = revealClock; revealClock += dt;
            bool done = true;
            for (int i = 0; i < reveals.Count; i++)
            {
                float start = i * .35f, k = Mathf.Clamp01((revealClock - start) / .6f), pop = 1 + Mathf.Sin(k * Mathf.PI) * .25f * (1 - k);
                if (before < start && revealClock >= start) COgheAudio.Instance?.Play("creature_tada", .45f, 0, .1f);
                reveals[i].Root.transform.localScale = Vector3.one * (COgheHomeItems.W * Smooth01(0, 1, k) * pop);
                done &= k >= 1;
            }
            if (done) { revealVisit = home == HomeState.Rest; FinishRevealsKeepVisit(); }
        }
        private void FinishRevealsKeepVisit()
        {
            foreach (var item in reveals) item.Root.transform.localScale = Vector3.one * COgheHomeItems.W;
            if (!revealVisit) reveals.Clear();
        }
        private void FinishReveals() { revealVisit = false; FinishRevealsKeepVisit(); reveals.Clear(); }

        private void TeleportBody(Vector3 to)
        {
            Vector3 shift = Vector3.ProjectOnPlane(to - game.Motion.Centre(0), up);
            foreach (var b in game.Matter.Bodies) { b.position += shift; b.linearVelocity = b.angularVelocity = Vector3.zero; }
            Physics.SyncTransforms(); game.Motion.Reset();
        }

        private bool Crossed(float first, float period, float t)
        {
            float prev = t - lastDt;
            if (t < first) return false;
            float a = Mathf.Floor((prev - first) / period), b = Mathf.Floor((t - first) / period);
            return prev < first || b != a;
        }
        private float lastDt;
        private static readonly string[] KeyNames = { "Key0", "Key1", "Key2", "Key3", "Key4", "Key5" };
        private static readonly int[] Tune = { 0, 2, 4, 5, 4, 2, 0, 3, 5 };
        private static readonly float[] KeyX = { -.875f, -.525f, -.175f, .175f, .525f, .875f };
        private static readonly string[] NoteNames = { "xylo_0", "xylo_1", "xylo_2", "xylo_3", "xylo_4", "xylo_5" };
        private Vector3 CameraFlat()
        {
            var view = game.Owner.View;
            Vector3 f = Vector3.ProjectOnPlane(view != null ? -view.transform.forward : Vector3.back, up);
            return f.sqrMagnitude > 1e-6f ? f.normalized : Vector3.back;
        }
        private static float Smooth01(float a, float b, float x) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(a, b, x));
    }
}
