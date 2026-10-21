using UnityEngine;

namespace GravityBox.Venom
{
    public enum COgheAct { None, Tantrum, Wave, Shape, Melt, GlassTap, Doze, Home, Monster, Trick, Read }
    /// <summary>Shapes COghe can make of itself, in the order they unlock (see <see cref="COghePersonality.ShapeUnlocks"/>).
    /// Shapes after the unlock list (Ball, One–Three: that many tendrils raised like fingers, and the toys' words such as the
    /// Drum) are words of the bonus "Hiểu ra" only, never picked at random.</summary>
    public enum COgheShape { Heart, Star, Question, Mushroom, Snowman, ThumbsUp, Rocket, Umbrella, Ball, One, Two, Three, Drum, Horn, Balloon, Top, MusicBox, Apple }

    /// <summary>
    /// COghe's character in the Spatial levels: curious, cheerful, a little comic. It decides *when* the creature performs a
    /// short act (waving a hand at the camera, turning into a funny shape, pretending to melt, tapping the glass, dozing off,
    /// or a tantrum when the player taps non-stop); <see cref="VenomLifeAnimation"/> draws it. Acts are presentation only:
    /// the skin leaves the simulated body and comes back to it, nothing is written to particles, forces or puzzle state.
    /// The tantrum is the one act that takes the controls away (Mrk: the player cannot steer it while it sulks); it starts
    /// only once the body is at rest and gives the controls back where the body still is.
    /// Acts only play when the body is whole, at rest on a floor, outside mechanisms, and there is room for them.
    /// </summary>
    public sealed partial class COghePersonality : MonoBehaviour
    {
        /// <summary>Tests and proof runs can switch the character off entirely.</summary>
        public static bool Enabled = true;
        public const float TantrumLength = 3.1f, IdleBeforeFirstAct = 6.5f, IdleBeforeDoze = 45f;
        /// <summary>A head rises from a liquid puddle, grows a face, unfurls its tongue and melts back (Mrk 15/10).</summary>
        public const float MonsterLength = 9.6f;
        private const int TapsForTantrum = 6;
        private const float TapWindow = 3f, TantrumCooldown = 25f, TantrumWaitLimit = 4f, FadeOut = .16f;
        /// <summary>Catalog position from which each shape can appear (hidden unlocks: it simply starts showing up).</summary>
        public static readonly int[] ShapeUnlocks = { 2, 4, 6, 8, 12, 16, 22, 28 };

        public COgheAct Act { get; private set; }
        public COgheShape Shape { get; private set; }
        /// <summary>The Emotion trick being played (Act == Trick).</summary>
        public COgheTrick Trick { get; private set; }
        /// <summary>Seconds into the current act (simulation time: pauses with the game).</summary>
        public float Time { get; private set; }
        public float Length { get; private set; }
        /// <summary>1 while an act plays; falls to 0 when an idle act is interrupted by the player.</summary>
        public float Fade { get; private set; } = 1;
        /// <summary>Act counter: the renderer rebuilds its per-act state when this changes.</summary>
        public int Serial { get; private set; }
        public bool HasWall { get; private set; }
        public Vector3 WallPoint { get; private set; }
        public Vector3 WallNormal { get; private set; }
        public bool LocksInput => Act == COgheAct.Tantrum || tantrumPending;
        public int Taps { get; private set; }

        private VenomCampaign game;
        private VenomLifeAnimation life;
        private readonly float[] taps = new float[TapsForTantrum];
        private bool tantrumPending, releasing, sawWave;
        private float lastSim = -1, idle, nextActAt, tantrumAllowedAt, pendingSince, releaseStart, tiredActAt;
        private int lastCommands, lastAct = -1, lastShape = -1;
        private System.Random rnd;
        // what the renderer saw of the whole body this frame
        private bool grounded; private Vector3 up = Vector3.up, centre, floor; private float speed;

        public static bool Applies(VenomCampaign g) => g != null && g.Definition != null && g.Definition.ProgressKey == "coghe.spatial.pilot";

        public void Initialize(VenomCampaign owner)
        {
            game = owner;
            rnd = new System.Random(owner.Definition.Order * 7919 + 17);
            game.MechanismTouched -= OnMechanismTouched; game.MechanismTouched += OnMechanismTouched;
            ResetState();
        }

        public void ResetState()
        {
            Act = COgheAct.None; Fade = 1; tantrumPending = releasing = false; idle = 0; lastSim = -1;
            nextActAt = IdleBeforeFirstAct; cameraMovedAt = -1; for (int i = 0; i < taps.Length; i++) taps[i] = -100;
            pendingTrick = null; journeysSeen = -1; airborne = 0; lastCentreAt = -1; for (int i = 0; i < reactedAt.Length; i++) reactedAt[i] = -100;
            lastCommands = game != null && game.Feedback != null ? game.Feedback.CommandCount : 0;
            ResetSigh();
        }

        /// <summary>A real tap from the player (touch or mouse, not a script).</summary>
        public void NoteTap()
        {
            if (!Enabled || game == null || game.Matter == null) return;
            Taps++;
            for (int i = taps.Length - 1; i > 0; i--) taps[i] = taps[i - 1];
            taps[0] = game.Matter.SimulationTime;
        }

        /// <summary>Called by the renderer while it draws the whole body.</summary>
        internal void ObserveBody(VenomLifeAnimation source, Vector3 bodyCentre, Vector3 bodyUp, Vector3 floorPoint, bool onGround, float bodySpeed)
        {
            life = source; centre = bodyCentre; up = bodyUp; floor = floorPoint; grounded = onGround; speed = bodySpeed;
        }

        private void Update()
        {
            if (game == null || game.Owner == null || game.Matter == null) return;
            EnsureEyes();
            if (!Enabled || (game.Home && room == null)) { if (Act != COgheAct.None || tantrumPending) EndAct(); return; }
            float now = game.Matter.SimulationTime, dt = lastSim < 0 ? 0 : now - lastSim;
            if (dt < -.05f)                                              // Retry rewinds the simulation clock
            {
                ResetState(); lastSim = now;
                return;
            }
            lastSim = now;
            if (dt <= 0) return;                                         // paused
            lastDt = dt;
            TrickOverlay();
            if (game.Home) { HomeTrickEvents(now); UpdateHome(dt); return; }
            TiredSounds(dt);

            bool commanded = game.Feedback != null && game.Feedback.CommandCount != lastCommands;
            if (game.Feedback != null) lastCommands = game.Feedback.CommandCount;
            bool whole = game.Matter.TotalFragmentCount == 1;
            bool moving = game.Motion.Get(game.Motion.Selected) != null || speed > .03f;
            bool over = game.Owner.Completed || game.Owner.Lost;
            idle = commanded || moving || !grounded ? 0 : idle + dt;

            if (Act != COgheAct.None)
            {
                float before = Time;
                Time += dt;
                if (!releasing) Cues(before, Time);
                bool interrupted = Act != COgheAct.Tantrum && (commanded || moving || !whole);
                if (over || (Act == COgheAct.Tantrum && !whole)) { EndAct(); return; }
                if (interrupted && !releasing) { releasing = true; releaseStart = Time; }
                if (releasing) Fade = 1 - Mathf.Clamp01((Time - releaseStart) / FadeOut);
                if (Time >= Length || (releasing && Fade <= 0)) EndAct();
                return;
            }

            if (over) retries = 0;
            if (sulkAfterRetry && now > 1.5f) { sulkAfterRetry = false; QueueTrick(COgheTrick.BackTurnSulk, now); }
            if (TrickEvents(now, dt)) return;

            // A burst of taps: take the controls, let the body finish its move, then throw a fit.
            if (!tantrumPending && now >= tantrumAllowedAt && whole && !over && now - taps[taps.Length - 1] <= TapWindow)
            { tantrumPending = true; pendingSince = now; }
            if (tantrumPending)
            {
                if (over || !whole || now - pendingSince > TantrumWaitLimit) { tantrumPending = false; tantrumAllowedAt = now + TantrumCooldown; return; }
                if (Ready(false)) { FindWall(.30f, true); Begin(COgheAct.Tantrum, TantrumLength); tantrumAllowedAt = now + TantrumCooldown; }
                return;
            }

            // hungry (COgheEnergy): soon after each stop it shows it, a flop, an apple of itself for food, a doze
            if (COgheEnergy.TiredIn(game))
            {
                if (idle <= 0) tiredActAt = COgheEnergy.State == COgheHunger.Exhausted ? 1.2f : 3f;
                else if (idle >= tiredActAt && Ready(true)) { TiredAct(); return; }
            }
            if (idle < nextActAt || !Ready(true)) return;
            ChooseIdleAct();
        }

        private void TiredAct()
        {
            const float body = .036f;
            bool exhausted = COgheEnergy.State == COgheHunger.Exhausted;
            bool open = life.Clearance(centre, up, .15f) && life.Clearance(centre + up * .05f, Right(), .06f) && life.Clearance(centre + up * .05f, -Right(), .06f);
            if (exhausted && idle > 9f) Begin(COgheAct.Doze, 1e6f);                       // nods off where it stopped
            else if (!exhausted && open && rnd.NextDouble() < .5) { Shape = COgheShape.Apple; Begin(COgheAct.Shape, 2.9f); COgheAudio.Instance?.Play("creature_grumble", .4f, 0, .1f); }
            else if (life.Clearance(centre, Right(), body * 1.2f)) Begin(COgheAct.Melt, 2.3f);   // flops down, worn out
            tiredActAt = idle + (exhausted ? 4f : 7f) + (float)rnd.NextDouble() * 3f;
            nextActAt = Mathf.Max(nextActAt, tiredActAt + 2);   // the usual acts wait their turn
        }

        /// <summary>Starts an act now, whatever the state (tests, previews, a debug menu).</summary>
        public void Force(COgheAct act, COgheShape shape = COgheShape.Heart)
        {
            if (act == COgheAct.None) { EndAct(); return; }
            Shape = shape;
            if (act == COgheAct.Tantrum) FindWall(.30f, true);
            if (act == COgheAct.GlassTap) FindWall(.12f, false);
            Begin(act, act == COgheAct.Tantrum ? TantrumLength : act == COgheAct.Monster ? MonsterLength : act == COgheAct.Read ? 5.2f : act == COgheAct.Doze ? 1e6f : act == COgheAct.Melt ? 2.3f : act == COgheAct.GlassTap ? 2.1f : 2.9f);
        }

        // Sounds on the beats of each act (clips in Resources/COgheAudio; a missing clip is simply silent).
        private void Cues(float from, float to)
        {
            switch (Act)
            {
                case COgheAct.Tantrum:
                    At(from, to, 0, "creature_grumble", .6f); At(from, to, .45f, "creature_whoosh", .45f); At(from, to, 1.05f, "creature_splat", .7f);
                    At(from, to, 1.3f, "creature_slide", .35f); At(from, to, 2.12f, "creature_whoosh", .25f); At(from, to, 2.6f, "creature_hmph", .55f); break;
                case COgheAct.Wave: At(from, to, .55f, "creature_hi", .55f); break;
                case COgheAct.Shape: At(from, to, .42f, "creature_tada", .5f); break;
                case COgheAct.Melt: At(from, to, .05f, "creature_melt", .45f); At(from, to, 1.55f, "creature_pop", .5f); break;
                case COgheAct.GlassTap: for (int k = 0; k < 3; k++) At(from, to, .7f + k * .32f, "glass_tok", .4f); break;
                case COgheAct.Monster:
                    // liquid sounds only, no voice (Mrk)
                    At(from, to, .35f, "monster_rise", .5f); At(from, to, 4.65f, "monster_slurp", .45f); At(from, to, MonsterLength - .75f, "creature_pop", .45f); break;
                case COgheAct.Trick: TrickCues(from, to); break;
                case COgheAct.Doze: if (to > 1.5f && Mathf.Floor((to - 1.5f) / 3.4f) != Mathf.Floor((from - 1.5f) / 3.4f)) COgheAudio.Instance?.Play("creature_snore", .3f, 0, .5f); break;
            }
        }

        // Emotion tricks (ability cards, Mrk 16/10/2026) --------------------------------------------------------------------
        // the same reaction (a tap, a fall, a device done) is not repeated within 8 s; different ones may follow each other
        private COgheTrick? pendingTrick; private float pendingTrickAt, airborne, lastCentreAt = -1; private int journeysSeen = -1;
        private readonly float[] reactedAt = new float[COgheTricks.Count];
        private int retries; private bool sulkAfterRetry;
        private Vector3 lastCentre;
        private int heavySeen = -1; private float slick;
        /// <summary>Seconds of the current slide on a slick coating (tests).</summary>
        public float SlideSeconds => slick;
        /// <summary>Each trick's plays this level (tests and the review reel).</summary>
        public readonly int[] TrickPlays = new int[COgheTricks.Count];
        /// <summary>Plays <paramref name="trick"/> now (its trigger, a test or a review reel).</summary>
        public void PlayTrick(COgheTrick trick)
        {
            Trick = trick; TrickPlays[(int)trick]++; flashedAt = -1;
            if (trick == COgheTrick.RockPaperScissors) TrickVariant = TrickPlays[(int)trick] % 3;   // who plays what, and who wins
            if (trick == COgheTrick.Mimic && !mimicSet) DefaultMimic();
            mimicSet = false;
            Begin(COgheAct.Trick, COgheTricks.Length(trick));
        }
        /// <summary>The player tapped COghe itself (whole, in a level): a wink.</summary>
        public void NoteBodyTap() { if (game != null && game.Matter != null) QueueTrick(COgheTrick.Wink, game.Matter.SimulationTime); }
        /// <summary>The player sent COghe somewhere it cannot go (slick glass, no route): a raspberry.</summary>
        public void NoteUnreachable() { if (game != null && game.Matter != null) QueueTrick(COgheTrick.Raspberry, game.Matter.SimulationTime); }
        /// <summary>The player restarted the level (Restart, or Try again after a failure): the third in a row and COghe
        /// turns its back and sulks once the level runs again (Emotion). A win clears the count.</summary>
        public void NoteRetry() { if (++retries >= 3) { retries = 0; sulkAfterRetry = true; } }
        /// <summary>The player turned or zoomed the camera: once they stop, COghe strikes a pose for it (Emotion).</summary>
        public void NoteCameraMove() { if (game != null && game.Matter != null && COgheTricks.Available(COgheTrick.Selfie)) cameraMovedAt = game.Matter.SimulationTime; }
        private float cameraMovedAt = -1, flashedAt = -1;
        /// <summary>Rock–paper–scissors: which hands are shown (0..2).</summary>
        public int TrickVariant { get; private set; }
        /// <summary>Mimic: what it copies (a box, else a handle) and where that is, for its eyes.</summary>
        public bool MimicBox { get; private set; } = true;
        public Vector3 MimicTarget { get; private set; }
        private bool mimicSet;
        // a crate or a handle within reach of the body: the nearest
        private bool FindMimicTarget(out Vector3 at, out bool box)
        {
            at = default; box = true; float best = float.MaxValue;
            foreach (var m in game.Mechanisms)
            {
                if (!(m is COgheTapRail rail) || rail.Rail == null) continue;
                Vector3 p = rail.CrateFaces ? rail.Rail.Body.position : rail.HandPoint;
                float reach = rail.CrateFaces ? rail.CrateSize.magnitude * .5f + .1f : .14f, d = Vector3.ProjectOnPlane(p - centre, up).magnitude;
                if (d > reach || d >= best) continue;
                best = d; at = p; box = rail.CrateFaces;
            }
            return best < float.MaxValue;
        }
        private void DefaultMimic()
        {
            MimicBox = true; MimicTarget = centre + CameraFlat() * .2f;
            if (room != null)
            {
                float best = float.MaxValue;
                foreach (var item in room.Items)
                {
                    if (item == null || !item.Root.activeInHierarchy) continue;
                    var c = room.BoundsOf(item).center; float d = Vector3.Distance(c, centre);
                    if (d < best) { best = d; MimicTarget = c; }
                }
            }
            else if (FindMimicTarget(out var at, out bool box)) { MimicTarget = at; MimicBox = box; }
        }
        // At Home: the breath heart for a 2 s hold on COghe, and a pose for the camera once the player has turned or zoomed it.
        private void HomeTrickEvents(float now)
        {
            if (room == null || bonusPhase != BonusPhase.None) return;
            bool page = game.ProductUI == null || game.ProductUI.Page == COgheProductPage.Home;
            bool free = (home == HomeState.Rest || home == HomeState.User || home == HomeState.Walk || home == HomeState.Play) && (Act == COgheAct.None || Act == COgheAct.Home && home == HomeState.Play);
            if (page && free && game.BodyHeld >= 2f && COgheTricks.Available(COgheTrick.BreathHeart))
            {
                game.ConsumePointer(); StopPlaying(); game.Motion.Cancel(0); home = HomeState.Rest; homeTimer = 0; homeNext = 3;
                PlayTrick(COgheTrick.BreathHeart); return;
            }
            if (cameraMovedAt < 0 || now - cameraMovedAt < .6f) return;
            cameraMovedAt = -1;
            if (page && free && Act == COgheAct.None && game.Motion.Get(0) == null && now - reactedAt[(int)COgheTrick.Selfie] > 20)
            { reactedAt[(int)COgheTrick.Selfie] = now; PlayTrick(COgheTrick.Selfie); }
        }
        // What some tricks put on the screen: the selfie's flash, the fog and heart of the breath heart.
        private COgheTrickOverlay overlay;
        /// <summary>The flash and the breath on the glass (tests).</summary>
        public COgheTrickOverlay OverlayForTests => overlay;
        private void TrickOverlay()
        {
            if (Act != COgheAct.Trick || life == null || game.Owner.View == null) return;
            if (overlay == null && (Trick == COgheTrick.Selfie || Trick == COgheTrick.BreathHeart)) overlay = COgheTrickOverlay.Create(transform);
            if (Trick == COgheTrick.Selfie && Time >= .9f && flashedAt < 0) { flashedAt = Time; overlay.Flash(); }
            if (Trick == COgheTrick.BreathHeart && Time > .8f)
            {
                var view = game.Owner.View; Vector3 at = view.WorldToScreenPoint(life.TrickAnchor); if (at.z <= 0) return;
                float radius = 1.5f * Vector2.Distance(at, view.WorldToScreenPoint(life.TrickAnchor + view.transform.right * life.TrickSpan));
                overlay.Breath(at, radius, Smooth01(.8f, 1.5f, Time), Mathf.Clamp01((Time - 1.7f) / 2.3f));
            }
        }
        private void QueueTrick(COgheTrick trick, float now)
        { if (!Enabled || !COgheTricks.Available(trick) || now - reactedAt[(int)trick] < 8) return; pendingTrick = trick; pendingTrickAt = now; }
        // a device finished (a clap), a fall (puffing up), and whatever the player set off, once the body is at rest
        private bool TrickEvents(float now, float dt)
        {
            int journeys = 0, heavy = 0;
            foreach (var m in game.Mechanisms) if (m is COgheTapRail rail) { journeys += rail.CompletedJourneys; if (COgheTricks.Heavy(rail)) heavy += rail.CompletedJourneys; }
            // a heavy job done (a crate pushed, a load hauled): a flex once COghe knows it, else the usual clap
            if (journeysSeen >= 0 && journeys > journeysSeen)
                QueueTrick(heavy > heavySeen && COgheTricks.Available(COgheTrick.Flex) ? COgheTrick.Flex : COgheTrick.Clap, now);
            journeysSeen = journeys; heavySeen = heavy;
            // a fall: dropping fast (a crawl down glass is slower) for a moment, then landing
            Vector3 body = game.Motion.Centre(game.Motion.Selected);   // the simulated body (the skin is not drawn whole mid-fall)
            float vertical = dt > 0 && lastCentreAt >= 0 ? Vector3.Dot(body - lastCentre, game.Root.up) / dt : 0; lastCentre = body; lastCentreAt = now;
            if (vertical < -.35f && !game.InTube && !game.Attached) airborne += dt;
            else if (airborne > .08f && vertical > -.08f && slick <= .25f) QueueTrick(COgheTrick.PuffUp, now);
            // a slide on a slick coating (down a taped wall, along a slick face; not a fall brushing past one): a skater's
            // landing instead of puffing up
            int slipping = 0, gripping = 0;
            for (int i = 0; i < 32; i++)
            {
                if (game.Matter.Groups[i] != game.Matter.Groups[game.Motion.Selected] || !game.Motion.Support(i, out _, out _, out _)) continue;
                if (game.Motion.HasGrip(i)) gripping++; else slipping++;
            }
            float moving = dt > 0 && lastCentreAt >= 0 ? speed : 0;
            if (slipping >= 3 && slipping > gripping && (moving > .08f || vertical < -.15f) && vertical > -.35f && !game.InTube && !game.Attached) slick += dt;
            else if (slick > 0 && vertical > -.08f && speed < .05f)
            { if (slick > .25f && COgheTricks.Available(COgheTrick.Skater)) QueueTrick(COgheTrick.Skater, now); slick = 0; }
            if (vertical > -.08f) airborne = 0;
            // the player turned or zoomed the camera and has let go: a selfie, not more than every 20 s
            if (cameraMovedAt >= 0 && now - cameraMovedAt > .6f)
            { cameraMovedAt = -1; if (now - reactedAt[(int)COgheTrick.Selfie] > 20) QueueTrick(COgheTrick.Selfie, now); }
            if (pendingTrick.HasValue && now - pendingTrickAt > 5) pendingTrick = null;
            if (!pendingTrick.HasValue || game.Owner.Completed || game.Owner.Lost || !Ready(true)) return false;
            reactedAt[(int)pendingTrick.Value] = now; PlayTrick(pendingTrick.Value); pendingTrick = null; return true;
        }
        private void TrickCues(float from, float to)
        {
            switch (Trick)
            {
                case COgheTrick.Groove: At(from, to, .15f, "creature_happy", .35f); break;
                case COgheTrick.Wink: At(from, to, .72f, "creature_ack_2", .45f); At(from, to, 1.0f, "creature_pop", .3f); break;
                case COgheTrick.Raspberry: At(from, to, .82f, "creature_slide", .5f); At(from, to, 1.3f, "creature_grumble", .3f); At(from, to, 1.9f, "creature_hm", .35f); break;
                case COgheTrick.Clap: for (int k = 0; k < 3; k++) At(from, to, .58f + k * .35f, "creature_pop", .38f); At(from, to, 1.75f, "creature_happy", .4f); break;
                case COgheTrick.PuffUp: At(from, to, .2f, "creature_grumble", .45f); At(from, to, .78f, "creature_land", .5f); At(from, to, 1.18f, "creature_land", .5f); At(from, to, 1.8f, "creature_hmph", .55f); break;
                case COgheTrick.Sneeze: At(from, to, .3f, "creature_hm", .35f); At(from, to, .72f, "creature_hm", .4f); At(from, to, 1.08f, "creature_splat", .55f); At(from, to, 2.55f, "creature_merge", .35f); break;
                case COgheTrick.Peekaboo: At(from, to, .05f, "creature_melt", .45f); At(from, to, .8f, "creature_curious_1", .4f); At(from, to, 1.38f, "creature_pop", .6f); At(from, to, 1.45f, "monster_rise", .35f); break;
                case COgheTrick.BellyDrum: for (int k = 0; k < 8; k++) At(from, to, .45f + k * .25f, "lab_far_thud", .45f); At(from, to, 2.65f, "creature_gulp", .5f); break;
                case COgheTrick.BackTurnSulk: At(from, to, .45f, "creature_hmph", .55f); At(from, to, 2.05f, "creature_hm", .35f); break;
                case COgheTrick.Moonwalk: At(from, to, .2f, "creature_slide", .4f); At(from, to, 1.6f, "creature_slide", .35f); At(from, to, 3.3f, "creature_tada", .45f); break;
                case COgheTrick.TailChase: At(from, to, .3f, "creature_curious_1", .4f); At(from, to, 1.2f, "creature_slide", .35f); At(from, to, 2.4f, "creature_pop", .55f); At(from, to, 2.75f, "creature_hm", .35f); At(from, to, 3.45f, "creature_happy", .4f); break;
                case COgheTrick.Flex: At(from, to, .3f, "creature_grumble", .3f); At(from, to, 1.05f, "creature_tada", .45f); At(from, to, 1.85f, "creature_ack_2", .4f); break;
                case COgheTrick.BubblePop: At(from, to, .35f, "creature_hm", .35f); At(from, to, 2.0f, "creature_pop", .65f); At(from, to, 2.35f, "creature_ack_2", .35f); break;
                case COgheTrick.Skater: At(from, to, .1f, "creature_slide", .45f); At(from, to, 1.45f, "creature_curious_1", .4f); At(from, to, 2.0f, "creature_hm", .35f); break;
                case COgheTrick.Selfie: At(from, to, .15f, "creature_hi", .4f); At(from, to, .9f, "mech_latch", .55f); At(from, to, 1.3f, "creature_happy", .35f); break;
                case COgheTrick.Juggle: At(from, to, .35f, "creature_split", .35f); for (int k = 0; k < 7; k++) At(from, to, 1.05f + k * .45f, "xylo_" + (k % 3 * 2), .3f); At(from, to, 4.0f, "creature_merge", .35f); break;
                case COgheTrick.Mimic: At(from, to, .2f, "creature_curious_2", .4f); At(from, to, .7f, "creature_pop", .45f); At(from, to, 1.4f, "creature_ack_3", .35f); At(from, to, 2.6f, "creature_merge", .3f); break;
                case COgheTrick.RockPaperScissors: At(from, to, .25f, "creature_split", .45f); for (int k = 0; k < 3; k++) At(from, to, 1.0f + k * .4f, "xylo_" + (k * 2), .4f); At(from, to, 2.1f, "creature_pop", .55f); At(from, to, 2.45f, "creature_tada", .4f); At(from, to, 2.7f, "creature_hmph", .35f); At(from, to, 4.1f, "creature_merge_full", .4f); break;
                case COgheTrick.BreathHeart: At(from, to, .3f, "creature_curious_1", .35f); At(from, to, .85f, "creature_whoosh", .5f); At(from, to, 1.75f, "creature_slide", .3f); At(from, to, 4.05f, "xylo_5", .45f); At(from, to, 4.3f, "creature_happy", .45f); break;
            }
        }

        private static void At(float from, float to, float beat, string clip, float volume)
        { if (from < beat && to >= beat || from == 0 && beat == 0) COgheAudio.Instance?.Play(clip, volume, 0, .2f); }

        /// <summary>Cosmetic reading, only when idle and clear of mechanisms. A player command cancels it normally.</summary>
        public bool TryRead()
        {
            if(!Enabled||game==null||game.Home||Act!=COgheAct.None||idle<.7f||!Ready(true)||game.Owner.Completed||game.Owner.Lost)return false;
            if(!life.Clearance(centre,up,.11f)||!life.Clearance(centre+up*.035f,Right(),.065f)||!life.Clearance(centre+up*.035f,-Right(),.065f))return false;
            Begin(COgheAct.Read,5.2f);nextActAt=idle+12;return true;
        }

        private bool Ready(bool quietOnly)
        {
            if (!grounded || speed > .03f || game.Motion.Get(game.Motion.Selected) != null) return false;
            if (game.Attached || game.Cutting || game.InTube || game.Owner.Paused) return false;
            if (Vector3.Dot(up, game.Root.up) < .8f) return false;                // on a wall or a slope: no acts
            return life != null && (!quietOnly || game.Matter.TotalFragmentCount == 1);
        }

        private void ChooseIdleAct()
        {
            float body = .036f;
            bool room = life.Clearance(centre, up, .15f) && life.Clearance(centre + up * .05f, Right(), .06f) && life.Clearance(centre + up * .05f, -Right(), .06f);
            bool nearWall = FindWall(.12f, false);
            // beside a crate or a handle, now and then it copies its shape (Emotion; no random draw unless it can)
            if (idle < IdleBeforeDoze && COgheTricks.Available(COgheTrick.Mimic) && FindMimicTarget(out var thing, out bool box) && rnd.NextDouble() < .5)
            { MimicTarget = thing; MimicBox = box; mimicSet = true; PlayTrick(COgheTrick.Mimic); nextActAt = idle + 8f + (float)rnd.NextDouble() * 6f; return; }
            // an idle trick (groove, sneeze) instead of the usual act, once COghe knows one
            int idleTricks = (COgheTricks.Available(COgheTrick.Groove) ? 1 : 0) + (COgheTricks.Available(COgheTrick.Sneeze) ? 1 : 0);
            if (idle < IdleBeforeDoze && room && idleTricks > 0 && rnd.NextDouble() < .35)
            {
                var pick = !COgheTricks.Available(COgheTrick.Sneeze) ? COgheTrick.Groove : !COgheTricks.Available(COgheTrick.Groove) ? COgheTrick.Sneeze
                    : rnd.Next(2) == 0 ? COgheTrick.Groove : COgheTrick.Sneeze;
                PlayTrick(pick); nextActAt = idle + 8f + (float)rnd.NextDouble() * 6f; return;
            }
            COgheAct act;
            if (idle >= IdleBeforeDoze) act = COgheAct.Doze;
            else if (!sawWave && room) act = COgheAct.Wave;
            else
            {
                // weighted, never the same act twice in a row
                float r = (float)rnd.NextDouble();
                act = r < .45f ? COgheAct.Shape : r < .62f ? COgheAct.Wave : r < .80f ? COgheAct.Melt : COgheAct.GlassTap;
                if ((int)act == lastAct) act = act == COgheAct.Shape ? COgheAct.Melt : COgheAct.Shape;
                if ((act == COgheAct.Shape || act == COgheAct.Wave) && !room) act = COgheAct.Melt;
                if (act == COgheAct.GlassTap && !nearWall) act = room ? COgheAct.Shape : COgheAct.Melt;
            }
            if (act == COgheAct.Melt && !life.Clearance(centre, Right(), body * 1.2f)) { nextActAt = idle + 4; return; }
            if (act == COgheAct.Shape) Shape = PickShape();
            if (act == COgheAct.Wave) sawWave = true;
            Begin(act, act == COgheAct.Doze ? 1e6f : act == COgheAct.Wave ? 2.9f : act == COgheAct.Shape ? 2.9f : act == COgheAct.Melt ? 2.3f : 2.1f);
            nextActAt = idle + 8f + (float)rnd.NextDouble() * 6f;
        }

        private COgheShape PickShape()
        {
            int level = Mathf.Max(game.Definition.Order, game.Progress != null ? game.Progress.Completed.Count + 1 : 1);
            int available = 0;
            while (available < ShapeUnlocks.Length && ShapeUnlocks[available] <= level) available++;
            if (available == 0) return COgheShape.Heart;
            // a shape unlocked at exactly this level shows first; then any, not the last one again
            for (int i = 0; i < available; i++) if (ShapeUnlocks[i] == game.Definition.Order && lastShape != i) { lastShape = i; return (COgheShape)i; }
            int pick = rnd.Next(available);
            if (available > 1 && pick == lastShape) pick = (pick + 1) % available;
            lastShape = pick; return (COgheShape)pick;
        }

        private void Begin(COgheAct act, float length)
        {
            Act = act; Time = 0; Length = length; Fade = 1; releasing = false; tantrumPending = false; Serial++;
            lastAct = (int)act;
            if (act == COgheAct.Tantrum) game.CancelPointer();
        }

        private void EndAct()
        {
            Act = COgheAct.None; Fade = 1; releasing = false; tantrumPending = false; idle = 0;
        }

        private Vector3 Right()
        {
            var view = game.Owner.View;
            Vector3 r = Vector3.ProjectOnPlane(view != null ? view.transform.right : Vector3.right, up);
            return r.sqrMagnitude > 1e-6f ? r.normalized : Vector3.right;
        }

        /// <summary>The nearest wall within reach, preferring side glass the camera sees the act against.</summary>
        private bool FindWall(float reach, bool needClearPath)
        {
            HasWall = false;
            if (life == null) return false;
            Vector3 from = centre + up * .02f, toCamera = game.Owner.View != null ? -game.Owner.View.transform.forward : -Vector3.forward;
            float best = float.MaxValue;
            Vector3 f = Vector3.ProjectOnPlane(Right(), up).normalized;
            for (int i = 0; i < 12; i++)
            {
                Vector3 d = Quaternion.AngleAxis(i * 30, up) * f;
                if (!life.Probe(from, d, reach, out var point, out var normal)) continue;
                if (Mathf.Abs(Vector3.Dot(normal, up)) > .4f) continue;                         // a wall, not a ledge
                // side glass reads best from the camera (a splat or a knock seen in profile); never the pane facing it
                float facing = Vector3.Dot(normal, toCamera);
                if (facing < -.5f) continue;
                float score = Vector3.Distance(from, point) + Mathf.Abs(facing) * .06f;
                if (score < best) { best = score; HasWall = true; WallPoint = point; WallNormal = normal; }
            }
            if (HasWall && needClearPath && !life.Clearance(from, (WallPoint - from).normalized, Vector3.Distance(from, WallPoint) - .02f)) HasWall = false;
            return HasWall;
        }
    }
}
