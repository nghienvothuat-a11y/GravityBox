using UnityEngine;

namespace GravityBox.Venom
{
    public enum COgheAct { None, Tantrum, Wave, Shape, Melt, GlassTap, Doze, Home, Monster }
    /// <summary>Shapes COghe can make of itself, in the order they unlock (see <see cref="COghePersonality.ShapeUnlocks"/>).
    /// Shapes after the unlock list (Ball, and One–Three: that many tendrils raised like fingers) are words of the bonus
    /// "Hiểu ra" only, never picked at random.</summary>
    public enum COgheShape { Heart, Star, Question, Mushroom, Snowman, ThumbsUp, Rocket, Umbrella, Ball, One, Two, Three }

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
        /// <summary>The main menu's monster: flows up, turns, sways, slides out its tongue, looks at you, melts back (Mrk 02/10).</summary>
        public const float MonsterLength = 7.2f;
        private const int TapsForTantrum = 6;
        private const float TapWindow = 3f, TantrumCooldown = 25f, TantrumWaitLimit = 4f, FadeOut = .16f;
        /// <summary>Catalog position from which each shape can appear (hidden unlocks: it simply starts showing up).</summary>
        public static readonly int[] ShapeUnlocks = { 2, 4, 6, 8, 12, 16, 22, 28 };

        public COgheAct Act { get; private set; }
        public COgheShape Shape { get; private set; }
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
        private float lastSim = -1, idle, nextActAt, tantrumAllowedAt, pendingSince, releaseStart;
        private int lastCommands, lastAct = -1, lastShape = -1;
        private System.Random rnd;
        // what the renderer saw of the whole body this frame
        private bool grounded; private Vector3 up = Vector3.up, centre, floor; private float speed;

        public static bool Applies(VenomCampaign g) => g != null && g.Definition != null && g.Definition.ProgressKey == "coghe.spatial.pilot";

        public void Initialize(VenomCampaign owner)
        {
            game = owner;
            rnd = new System.Random(owner.Definition.Order * 7919 + 17);
            ResetState();
        }

        public void ResetState()
        {
            Act = COgheAct.None; Fade = 1; tantrumPending = releasing = false; idle = 0; lastSim = -1;
            nextActAt = IdleBeforeFirstAct; for (int i = 0; i < taps.Length; i++) taps[i] = -100;
            lastCommands = game != null && game.Feedback != null ? game.Feedback.CommandCount : 0;
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
            if (!Enabled || (game.Home && room == null)) { if (Act != COgheAct.None || tantrumPending) EndAct(); return; }
            float now = game.Matter.SimulationTime, dt = lastSim < 0 ? 0 : now - lastSim;
            if (dt < -.05f) { ResetState(); lastSim = now; return; }   // Retry rewinds the simulation clock
            lastSim = now;
            if (dt <= 0) return;                                         // paused
            lastDt = dt;
            if (game.Home) { UpdateHome(dt); return; }

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

            // A burst of taps: take the controls, let the body finish its move, then throw a fit.
            if (!tantrumPending && now >= tantrumAllowedAt && whole && !over && now - taps[taps.Length - 1] <= TapWindow)
            { tantrumPending = true; pendingSince = now; }
            if (tantrumPending)
            {
                if (over || !whole || now - pendingSince > TantrumWaitLimit) { tantrumPending = false; tantrumAllowedAt = now + TantrumCooldown; return; }
                if (Ready(false)) { FindWall(.30f, true); Begin(COgheAct.Tantrum, TantrumLength); tantrumAllowedAt = now + TantrumCooldown; }
                return;
            }

            if (idle < nextActAt || !Ready(true)) return;
            ChooseIdleAct();
        }

        /// <summary>Starts an act now, whatever the state (tests, previews, a debug menu).</summary>
        public void Force(COgheAct act, COgheShape shape = COgheShape.Heart)
        {
            if (act == COgheAct.None) { EndAct(); return; }
            Shape = shape;
            if (act == COgheAct.Tantrum) FindWall(.30f, true);
            if (act == COgheAct.GlassTap) FindWall(.12f, false);
            Begin(act, act == COgheAct.Tantrum ? TantrumLength : act == COgheAct.Monster ? MonsterLength : act == COgheAct.Doze ? 1e6f : act == COgheAct.Melt ? 2.3f : act == COgheAct.GlassTap ? 2.1f : 2.9f);
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
                    At(from, to, .35f, "monster_rise", .5f); At(from, to, 2.55f, "monster_slurp", .45f); At(from, to, MonsterLength - .75f, "creature_pop", .45f); break;
                case COgheAct.Doze: if (to > 1.5f && Mathf.Floor((to - 1.5f) / 3.4f) != Mathf.Floor((from - 1.5f) / 3.4f)) COgheAudio.Instance?.Play("creature_snore", .3f, 0, .5f); break;
            }
        }

        private static void At(float from, float to, float beat, string clip, float volume)
        { if (from < beat && to >= beat || from == 0 && beat == 0) COgheAudio.Instance?.Play(clip, volume, 0, .2f); }

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
