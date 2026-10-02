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
        private enum HomeState { Rest, Walk, Play, React, Sulk, User, Eat }
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
        /// <summary>Going for, or eating, the food balls.</summary>
        public bool Eating => home == HomeState.Eat;
        public int BallsEaten { get; private set; }
        /// <summary>Where COghe's skin is drawn (its body, or where a Home act carries it): the zoomed Home camera follows it.</summary>
        public Vector3 SkinCentre => room != null && Act == COgheAct.Home && Pose.Blend > 0 ? Vector3.Lerp(centre, Pose.Centre, Pose.Blend) : game.Motion.Centre(0);
        /// <summary>Main menu: the room is hidden, so COghe stays put and only performs small acts on the spot.</summary>
        internal bool Showcase { get; set; }
        private int showcaseActs;
        /// <summary>The Style screen: COghe holds still on its spot (no wandering, games, feeding or acts) while it is dressed.</summary>
        public bool OnStage { get; private set; }
        public void TakeStage(Vector3 spot)
        {
            StopPlaying(); EndAct(); home = HomeState.Rest; game.Motion.StopAll();
            if (room != null) TeleportBody(spot);
            OnStage = true;
        }
        public void LeaveStage() { OnStage = false; home = HomeState.Rest; homeTimer = 0; homeNext = 1.5f; }

        internal void EnterHome(COgheHomeRoom homeRoom)
        {
            room = homeRoom; home = HomeState.Rest; homeTimer = 0; homeNext = 1.2f; showcaseActs = 0; playing = target = null; eating = false; meal = null; EndAct();
            reveals.Clear(); revealClock = -.6f; revealVisit = false; revealPending = true;
            for (int i = 0; i < touches.Length; i++) touches[i] = -100;
        }
        internal void LeaveHome() { room = null; home = HomeState.Rest; playing = target = null; eating = false; meal = null; EndAct(); }

        /// <summary>The player tapped COghe itself.</summary>
        public void TouchedInHome(Vector3 point)
        {
            if (room == null || Act == COgheAct.Monster) return;   // the monster finishes its show
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
            if (room == null || item == null || !room.Present(item)) return;
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
            if (OnStage) { room.Step(); return; }
            room.Step();
            Reveal(dt);
            stateTime += dt;
            bool commanded = game.Feedback != null && game.Feedback.CommandCount != lastCommands;
            if (game.Feedback != null) lastCommands = game.Feedback.CommandCount;
            if (commanded && home != HomeState.User) { StopPlaying(); home = HomeState.User; stateTime = 0; }   // the player steers: step aside
            else if (game.HomeFeeding && !Showcase && (home == HomeState.Rest || home == HomeState.Walk || home == HomeState.Play))
            { StopPlaying(); home = HomeState.Eat; stateTime = 0; meal = null; }                                          // food! off to eat it
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
                    if (stateTime > 6 && game.Motion.Get(0) == null) { home = HomeState.Rest; homeTimer = 0; homeNext = 2; }
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
                case HomeState.Eat:
                    UpdateEat(dt);
                    break;
            }
        }

        private void ChooseHomeActivity()
        {
            homeTimer = 0; homeNext = 2f + (float)rnd.NextDouble() * 3f;
            if (Showcase)
            {
                homeNext = 5f + (float)rnd.NextDouble() * 4f;
                // the monster first, to catch the eye as the menu opens (Mrk), then every third act
                if (showcaseActs++ % 3 == 0) { Begin(COgheAct.Monster, MonsterLength); homeNext = 6f + (float)rnd.NextDouble() * 3f; return; }
                COgheAct small = rnd.Next(3) == 0 ? COgheAct.Wave : rnd.Next(2) == 0 ? COgheAct.Shape : COgheAct.Melt;
                if (small == COgheAct.Shape) Shape = PickShape();
                Begin(small, small == COgheAct.Melt ? 2.3f : 2.9f);
                return;
            }
            choices.Clear();
            foreach (var item in room.Items) if (room.Present(item) && item != lastPlayed) choices.Add(item);
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
                "BED" => 7f, "DUMBBELL" => 5.2f, "BALL" => 6.8f, "MIRROR" => 5.8f, "SWING" => 6.5f, "SLIDE" => 4.4f, "TV" => 6f,
                "XYLOPHONE" => 5f, "TRAMPOLINE" => 5.5f, "HAMMOCK" => 7f, "WHEEL" => 6f, "AQUARIUM" => 5.5f, "SHADOW_LAMP" => 6f, _ => 4.8f,
            };
            if (item.Id == "MIRROR" || item.Id == "SHADOW_LAMP") Shape = PickShape();
            if (item.Id == "BALL")
            {
                var ball = item.Part("Ball"); ballSide = rnd.Next(2) == 0 ? -1 : 1;
                if (ball != null) { ballPrevious = ball.position; ballSpin = ball.rotation; skinTop = SkinTopOver(ball.position - up * (.45f * COgheHomeItems.W)); }
            }
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
            StopEating();   // a touch, a toy or a preview mid-meal: the half-swallowed ball is finished first
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
                    BallGame(item, t, ref p);
                    break;
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

        // The ball (Mrk: really fun, roll it and toss it): COghe wiggles with excitement, dribbles the ball round a loop
        // and back, grabs it with two tendrils and tosses it high, heads it twice as it comes down, pops it away behind, and
        // hops into a heart while the ball rolls back to its spot. The ball rolls without slipping (spun by how far it went).
        private float ballSide = 1, skinTop = .045f; private Vector3 ballPrevious; private Quaternion ballSpin = Quaternion.identity;
        private Renderer skinRenderer;
        /// <summary>How high COghe's skin reaches over the floor right now (the ball lands on that).</summary>
        private float SkinTopOver(Vector3 floorPoint)
        {
            if (skinRenderer == null) foreach (var r in game.Matter.GetComponentsInChildren<MeshRenderer>()) if (r.name == "Continuous wet skin") { skinRenderer = r; break; }
            return skinRenderer != null ? Mathf.Clamp(Vector3.Dot(skinRenderer.bounds.max - floorPoint, up), .025f, .08f) : .045f;
        }
        private void BallGame(COgheHomeItem item, float t, ref COgheSkinPose p)
        {
            var ball = item.Part("Ball"); if (ball == null) return;
            const float s = .036f, loop = .08f;
            float rb = .45f * COgheHomeItems.W;
            Vector3 rest = item.World(new Vector3(0, .45f, 0));
            Vector3 fwd = Vector3.ProjectOnPlane(rest - centre, up); fwd = fwd.sqrMagnitude > 1e-6f ? fwd.normalized : Vector3.forward;
            Vector3 side = Vector3.Cross(up, fwd) * ballSide;
            float height = Vector3.Dot(centre - rest, up) + rb;                      // the skin centre over the floor
            Vector3 Floor(Vector3 at, float h) => at - up * (Vector3.Dot(at - rest, up) + rb) + up * h;
            Vector3 Loop(float a) => rest + side * (loop * (1 - Mathf.Cos(a))) + fwd * (loop * Mathf.Sin(a));
            Vector3 Tangent(float a) => (side * Mathf.Sin(a) + fwd * Mathf.Cos(a)).normalized;
            Vector3 Pusher(float a) => Floor(Loop(a) - Tangent(a) * (rb + s * .9f), height);
            Vector3 Arc(Vector3 from, Vector3 to, float k, float h) => Vector3.Lerp(from, to, k) + up * (Mathf.Sin(k * Mathf.PI) * h);
            Vector3 under = Floor(rest, height), behind = rest + fwd * .1f;
            float onHead = skinTop * .96f;                                           // the ball resting on COghe's head
            Vector3 at = rest; bool rolling = true;
            p.Centre = centre;
            if (t < .6f)
            {   // can't wait
                p.Centre = centre + up * (s * .22f * Mathf.Abs(Mathf.Sin(t * 13))); p.Squash = 1 + .07f * Mathf.Sin(t * 26);
            }
            else if (t < .95f) p.Centre = Arc(centre, Pusher(0), Smooth01(.6f, .95f, t), s * .5f);
            else if (t < 3f)
            {   // dribble: nudging the ball round a loop, leaning into it
                float a = Mathf.PI * 2 * Smooth01(.95f, 3f, t);
                at = Loop(a);
                p.Centre = Pusher(a) + up * (s * .2f * Mathf.Abs(Mathf.Sin(a * 2)));
                p.Turn = Quaternion.AngleAxis(12, Vector3.Cross(up, Tangent(a)));
                p.Squash = 1 + .06f * Mathf.Sin(a * 4);
                if (Crossed(1.3f, .55f, t)) COgheAudio.Instance?.Play("creature_pop", .22f, 0, .2f);
            }
            else if (t < 3.35f)
            {   // two tendrils take hold, it crouches
                float k = Smooth01(3f, 3.35f, t);
                p.Centre = Pusher(Mathf.PI * 2); p.Squash = 1 - .18f * k; p.Turn = Quaternion.AngleAxis(8 * k, Vector3.Cross(up, fwd));
                p.Tendrils = 2; p.TipA = rest + side * (rb * .95f); p.TipB = rest - side * (rb * .95f);
                if (Crossed(3.05f, 100, t)) COgheAudio.Instance?.Play("creature_grab", .4f, 0, .2f);
            }
            else if (t < 4.25f)
            {   // the toss: high, spinning; COghe hops underneath and looks up
                float k = (t - 3.35f) / .9f;
                at = rest + up * (onHead * k + 4 * .2f * k * (1 - k)); rolling = false;
                if (t < 3.5f)
                {
                    p.Centre = Pusher(Mathf.PI * 2) + up * (s * .25f * Smooth01(3.35f, 3.5f, t)); p.Squash = 1.15f;
                    if (t < 3.45f) { p.Tendrils = 2; p.TipA = at + side * (rb * .95f); p.TipB = at - side * (rb * .95f); }
                }
                else if (t < 3.95f) p.Centre = Arc(Pusher(Mathf.PI * 2), under, Smooth01(3.5f, 3.95f, t), s * .5f);
                else { p.Centre = under; p.Squash = 1.08f + .03f * Mathf.Sin(t * 20); }
                if (Crossed(3.36f, 100, t)) COgheAudio.Instance?.Play("creature_whoosh", .35f, 0, .2f);
            }
            else if (t < 4.92f)
            {   // two headers
                float h = t < 4.62f ? .045f : .03f, u = t < 4.62f ? (t - 4.25f) / .37f : (t - 4.62f) / .3f;
                at = rest + up * (onHead + 4 * h * u * (1 - u)); rolling = false;
                float hit = Mathf.Exp(-(t - (t < 4.62f ? 4.25f : 4.62f)) * 16);
                p.Centre = under - up * (s * .25f * hit); p.Squash = 1 - .22f * hit;
                p.Turn = Quaternion.AngleAxis(Mathf.Sin(t * 18) * 6, Vector3.Cross(up, fwd));
                if (Crossed(4.25f, 100, t) || Crossed(4.62f, 100, t)) COgheAudio.Instance?.Play("creature_pop", .4f, 0, .1f);
            }
            else if (t < 5.45f)
            {   // the last header pops it away behind; COghe hops back to where it stood
                float u = (t - 4.92f) / .53f;
                at = Vector3.Lerp(rest + up * onHead, behind, u) + up * (4 * .08f * u * (1 - u)); rolling = false;
                float hit = Mathf.Exp(-(t - 4.92f) * 16);
                p.Centre = Arc(under, centre, Smooth01(4.97f, 5.45f, t), s * .6f) - up * (s * .25f * hit); p.Squash = 1 - .22f * hit;
                if (Crossed(4.92f, 100, t)) COgheAudio.Instance?.Play("creature_pop", .45f, 0, .1f);
            }
            else
            {   // a little bounce, the ball rolls home; COghe hops into a heart
                float u = Mathf.Clamp01((t - 5.45f) / .2f);
                at = t < 5.65f ? behind + up * (4 * .015f * u * (1 - u)) : Vector3.Lerp(behind, rest, 1 - Mathf.Pow(1 - Smooth01(5.65f, 6.5f, t), 2));
                float hop = Mathf.Sin(Smooth01(5.5f, 5.95f, t) * Mathf.PI);
                p.Centre = centre + up * (s * 1f * hop); p.Squash = 1 + .12f * hop;
                p.Morph = Smooth01(5.7f, 6f, t) * (1 - Smooth01(6.35f, 6.65f, t)); p.Shape = COgheShape.Heart;
                if (Crossed(5.72f, 100, t)) COgheAudio.Instance?.Play("creature_tada", .45f, 0, .2f);
            }
            // roll without slipping on the floor; spin freely in the air
            Vector3 flat = Vector3.ProjectOnPlane(at - ballPrevious, up);
            if (rolling && flat.sqrMagnitude > 1e-10f) ballSpin = Quaternion.AngleAxis(flat.magnitude / rb * Mathf.Rad2Deg, Vector3.Cross(up, flat).normalized) * ballSpin;
            else if (!rolling) ballSpin = Quaternion.AngleAxis(lastDt * 560, side) * ballSpin;
            ballPrevious = at;
            ball.position = at; ball.rotation = ballSpin;
        }

        // Eating (Mrk: Feed throws steel balls; COghe goes to where each one stops and eats it) -----------------------------
        private COgheFeedBall meal; private int mealTries; private bool eating, lastMeal; private Vector3 mealFrom;
        private void StopEating()
        {
            if (eating && meal != null) game.HomeFeedBalls?.Consume(meal);   // half swallowed: finish it
            if (eating) EndAct();
            eating = false; meal = null;
        }
        private void UpdateEat(float dt)
        {
            var food = game.HomeFeedBalls;
            if (eating)
            {
                Time += dt; EatPose(Time);
                if (Time >= Length) { EndAct(); eating = false; meal = null; }
                return;
            }
            if (food == null || !game.HomeFeeding) { home = HomeState.Rest; homeTimer = 0; homeNext = 2; return; }
            var body = game.Motion.Centre(0);
            if (meal == null || meal.Taken)
            {
                meal = food.NearestResting(body); mealTries = 0;
                if (meal == null) return;   // still bouncing: wait for one to stop
                WalkToMeal(body);
            }
            float reach = Vector3.ProjectOnPlane(meal.Position - body, up).magnitude;
            bool stopped = game.Motion.Get(0) == null;
            if (stopped && reach < .11f) StartMeal(food);
            else if (stopped || stateTime > 9)
            {
                if (++mealTries <= 2 && stateTime <= 9) WalkToMeal(body);
                else if (reach < .2f) StartMeal(food);                               // close enough to reach for it
                else { food.Consume(meal); meal = null; stateTime = 0; }            // out of reach: let it go
            }
        }
        private void WalkToMeal(Vector3 body)
        {
            Vector3 to = Vector3.ProjectOnPlane(meal.Position - body, up);
            Vector3 dir = to.sqrMagnitude > 1e-6f ? to.normalized : CameraFlat();
            var local = room.Root.InverseTransformPoint(meal.Position - dir * (COgheFeedBall.Radius + .055f));
            local.x = Mathf.Clamp(local.x, -COgheHomeRoom.HalfWidth + .06f, COgheHomeRoom.HalfWidth - .06f);
            local.z = Mathf.Clamp(local.z, -COgheHomeRoom.HalfDepth + .06f, COgheHomeRoom.HalfDepth - .06f);
            local.y = COgheHomeRoom.BodyHeight;
            game.Motion.Move(0, room.Root.TransformPoint(local)); stateTime = 0;
        }
        private void StartMeal(COgheFeedBalls food)
        {
            food.Take(meal); mealFrom = meal.Position; eating = true;
            lastMeal = food.Balls.Count <= 1;
            Begin(COgheAct.Home, lastMeal ? 1.75f : 1.35f);
            Pose = default; Pose.Turn = Quaternion.identity; Pose.Squash = 1; Pose.Axis = up; Pose.Bubble = -1;
        }
        private void EatPose(float t)
        {
            const float s = .036f;
            var p = new COgheSkinPose { Blend = 1, Centre = centre, Turn = Quaternion.identity, Axis = up, Squash = 1, Bubble = -1 };
            Vector3 dir = Vector3.ProjectOnPlane(mealFrom - centre, up); dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : CameraFlat();
            Vector3 mouth = centre + dir * (s * .7f) + up * (s * .35f);
            float reach = Smooth01(.05f, .3f, t), pull = Smooth01(.3f, .68f, t), swallow = Smooth01(.68f, .86f, t);
            // a tendril reaches out, wraps the ball and reels it in; it disappears into the body with a gulp
            Vector3 ballAt = Vector3.Lerp(Vector3.Lerp(mealFrom, mouth, pull), centre + up * (s * .2f), swallow);
            if (meal != null)
            {
                if (t < .86f) meal.Place(ballAt, Mathf.Lerp(1, .6f, pull) * (1 - swallow));
                else { game.HomeFeedBalls?.Consume(meal); meal = null; BallsEaten++; game.GreetHomeQuietly(); }
            }
            p.Tendrils = t < .7f ? 1 : 0; p.TipA = t < .3f ? Vector3.Lerp(centre + dir * s, mealFrom, reach) : ballAt;
            p.Centre = centre + dir * (s * .25f * reach * (1 - swallow));
            p.Axis = t < .8f ? dir : up;
            float gulp = t < .8f ? 0 : Mathf.Sin(Mathf.Clamp01((t - .8f) / .5f) * Mathf.PI * 2) * (1 - Smooth01(.8f, 1.3f, t));
            p.Squash = t < .8f ? 1 - .1f * reach * (1 - pull) : 1 + .14f * gulp;
            if (lastMeal) { float hop = Mathf.Sin(Smooth01(1.25f, 1.65f, t) * Mathf.PI); p.Centre += up * (s * .8f * hop); }
            if (Crossed(.32f, 100, t)) COgheAudio.Instance?.Play("creature_grab", .35f, 0, .1f);
            if (Crossed(.8f, 100, t)) COgheAudio.Instance?.Play("creature_gulp", .55f, 0, .1f);
            if (lastMeal && Crossed(1.25f, 100, t)) COgheAudio.Instance?.Play("creature_happy", .45f, 0, .2f);
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

        /// <summary>Something was just bought for the room: pop it in now and go to it.</summary>
        public void RevealNew()
        {
            if (room == null) return;
            room.Refresh(); revealPending = true; revealClock = -.25f; reveals.Clear();
            foreach (var item in room.Newly()) item.Root.transform.localScale = Vector3.zero;   // hidden until its pop
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
