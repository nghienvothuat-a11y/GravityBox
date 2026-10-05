using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravityBox.Venom
{
    /// <summary>
    /// COghe sound, first version: one background track across every level and a small set of creature and mechanism
    /// effects. Mechanisms are heard while they move (a motor for things that lift, a slide for blocks on the floor) and
    /// when they arrive at either end of their travel. It only reads game state (like COgheControlFeedback): it never changes physics,
    /// input, navigation or mechanisms. Clips load by name from Resources/COgheAudio, so any clip can be replaced by a
    /// file with the same name (Docs/Audio/COghe/README.md). Event timing uses simulation time, so scripted tests see
    /// the same events as live play.
    /// </summary>
    public sealed class COgheAudio : MonoBehaviour
    {
        public static COgheAudio Instance { get; private set; }
        private const string ClipFolder = "COgheAudio/";
        private const string CatalogKey = "coghe.spatial.pilot";

        // Mix (0–1). First pass; tune by ear on the phone.
        public const float MusicVolume = .65f, MotorVolume = .40f, SlideVolume = .45f, ArriveVolume = .35f;

        public static bool MusicOn
        {
            get => PlayerPrefs.GetInt("coghe.audio.music", 1) == 1;
            set { PlayerPrefs.SetInt("coghe.audio.music", value ? 1 : 0); PlayerPrefs.Save(); }
        }
        public static bool EffectsOn
        {
            get => PlayerPrefs.GetInt("coghe.audio.effects", 1) == 1;
            set { PlayerPrefs.SetInt("coghe.audio.effects", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        /// <summary>How many times each sound played (tests and tuning).</summary>
        public readonly Dictionary<string, int> Played = new Dictionary<string, int>();
        public AudioSource Music { get; private set; }
        public AudioSource Motor { get; private set; }
        public AudioSource Slide { get; private set; }
        /// <summary>0–1: how much of the motor / floor-slide loop is playing (smoothed).</summary>
        public float MotorLevel => motorLevel;
        public float SlideLevel => slideLevel;
        public VenomCampaign Game => game;

        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, float> lastPlayed = new Dictionary<string, float>();
        private readonly AudioSource[] voices = new AudioSource[10];
        private int nextVoice;

        private VenomCampaign game;
        private bool bound;
        private COgheRailSlider[] rails = new COgheRailSlider[0];
        private COgheTissueSensor[] pads = new COgheTissueSensor[0];
        private COgheGearTrain[] trains = new COgheGearTrain[0];
        private COghePassengerLift[] lifts = new COghePassengerLift[0];
        private COgheTurntable[] tables = new COgheTurntable[0];
        private COgheSwingTransfer[] swings = new COgheSwingTransfer[0];
        private COgheTubeNetwork[] tubes = new COgheTubeNetwork[0];
        private COgheTapRail[] tasks = new COgheTapRail[0];
        private VenomMovableProp[] loose = new VenomMovableProp[0];
        private readonly HashSet<COgheRailSlider> liftRails = new HashSet<COgheRailSlider>();
        private bool[] atEnd, atStart, active, meshed, caught, travelling, operating;
        private float[] railPosition, railTravel, tableAngle;
        private Vector3[] loosePosition;
        private int[] trips, landings, journeys;
        private COghePassengerLift.ButtonState[] liftButtons = new COghePassengerLift.ButtonState[0];
        private int refusals;
        private bool[] overloaded = new bool[0];
        private COgheSwingTransfer.SwingPhase[] swingPhase;
        private string[] failures;
        private int fragments, commands, escaped;
        private bool attached, completed, lost, falling;
        private float lastSim, quietUntil, idleSince, nextIdle, nextFar, fallSpeed, motorLevel, slideLevel, crawlLevel, duckUntil, motorPan, slidePan;
        private Vector3 lastCentre;
        private bool haveCentre;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("COghe audio");
            DontDestroyOnLoad(go);
            go.AddComponent<COgheAudio>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Music = Loop("music_lab_loop");
            Motor = Loop("mech_motor_loop");
            Slide = Loop("block_slide_loop");
            for (int i = 0; i < voices.Length; i++) { voices[i] = gameObject.AddComponent<AudioSource>(); voices[i].playOnAwake = false; }
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this) { Instance = null; SceneManager.sceneLoaded -= OnSceneLoaded; }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { game = null; bound = false; }

        private AudioSource Loop(string name)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = Clip(name); source.loop = true; source.playOnAwake = false; source.volume = 0;
            return source;
        }

        private AudioClip Clip(string name)
        {
            if (!clips.TryGetValue(name, out var clip)) { clip = Resources.Load<AudioClip>(ClipFolder + name); clips[name] = clip; }
            return clip;
        }

        private void Update()
        {
            if (game == null) { game = FindFirstObjectByType<VenomCampaign>(); bound = false; }
            bool ours = Ours();
            if (ours && !bound) Bind();      // a level found before its definition is ready binds on a later frame
            if (ours && !COgheIntro.Playing) Observe();   // the intro has its own score; the level waits under it
            Mix(ours);
        }

        private bool Ours() => game != null && game.Definition != null && game.Definition.ProgressKey == CatalogKey && game.Matter != null && game.Owner != null;

        // ---- binding ----------------------------------------------------------------------------------------------------
        private void Bind()
        {
            if (!Ours()) return;
            lastPlayed.Clear(); // Cooldowns belong to this level's simulation clock.
            var root = game.Owner.Apparatus;
            var allRails = root.GetComponentsInChildren<COgheRailSlider>();
            var list = new List<COgheRailSlider>();
            foreach (var r in allRails) if (!r.name.StartsWith("Q ")) list.Add(r); // Q's own gates are part of its split
            rails = list.ToArray();
            pads = root.GetComponentsInChildren<COgheTissueSensor>();
            trains = root.GetComponentsInChildren<COgheGearTrain>();
            lifts = root.GetComponentsInChildren<COghePassengerLift>();
            liftRails.Clear(); foreach (var l in lifts) if (l.Rail != null) liftRails.Add(l.Rail); // a lift arrives with its own ding
            tables = root.GetComponentsInChildren<COgheTurntable>();
            swings = root.GetComponentsInChildren<COgheSwingTransfer>();
            tubes = root.GetComponentsInChildren<COgheTubeNetwork>();
            tasks = root.GetComponentsInChildren<COgheTapRail>();
            var props = new List<VenomMovableProp>();
            foreach (var prop in game.Props) if (prop != null && prop.Body != null && prop.GetComponent<COgheRailSlider>() == null) props.Add(prop);
            loose = props.ToArray();
            Snapshot(); bound = true;
            idleSince = Now; nextIdle = Now + Random.Range(14f, 24f); nextFar = Now + Random.Range(20f, 45f);
            quietUntil = Now + .6f;
            if (!COgheIntro.Playing) Play("game_level_start", .45f);
        }

        private float Now => game != null && game.Matter != null ? game.Matter.SimulationTime : Time.time;

        private void Snapshot()
        {
            atEnd = new bool[rails.Length]; atStart = new bool[rails.Length]; railPosition = new float[rails.Length]; railTravel = new float[rails.Length];
            for (int i = 0; i < rails.Length; i++) { atEnd[i] = rails[i].AtEnd; atStart[i] = rails[i].Position <= rails[i].CatchTolerance; railPosition[i] = rails[i].Position; }
            loosePosition = new Vector3[loose.Length]; for (int i = 0; i < loose.Length; i++) loosePosition[i] = loose[i].Body.position;
            active = new bool[pads.Length]; for (int i = 0; i < pads.Length; i++) active[i] = pads[i].Active;
            meshed = new bool[trains.Length]; for (int i = 0; i < trains.Length; i++) meshed[i] = trains[i].Meshed;
            trips = new int[lifts.Length]; for (int i = 0; i < lifts.Length; i++) trips[i] = lifts[i].Trips;
            liftButtons = new COghePassengerLift.ButtonState[lifts.Length]; for (int i = 0; i < lifts.Length; i++) liftButtons[i] = lifts[i].Button;
            refusals = Refusals();
            overloaded = new bool[tasks.Length];
            caught = new bool[tables.Length]; tableAngle = new float[tables.Length];
            for (int i = 0; i < tables.Length; i++) { caught[i] = tables[i].Caught; tableAngle[i] = tables[i].Angle; }
            swingPhase = new COgheSwingTransfer.SwingPhase[swings.Length]; landings = new int[swings.Length];
            for (int i = 0; i < swings.Length; i++) { swingPhase[i] = swings[i].Phase; landings[i] = swings[i].Landings; }
            travelling = new bool[tubes.Length]; for (int i = 0; i < tubes.Length; i++) travelling[i] = tubes[i].AnyTravelling;
            operating = new bool[tasks.Length]; journeys = new int[tasks.Length]; failures = new string[tasks.Length];
            for (int i = 0; i < tasks.Length; i++) { operating[i] = tasks[i].Phase == COgheTapRail.TaskPhase.Operating; journeys[i] = tasks[i].CompletedJourneys; failures[i] = tasks[i].LastFailure; }
            fragments = game.Matter.TotalFragmentCount; escaped = game.Matter.EscapedCount;
            commands = game.Feedback != null ? game.Feedback.CommandCount : 0;
            attached = game.Attached; completed = game.Owner.Completed; lost = game.Owner.Lost;
            falling = false; haveCentre = false; lastSim = Now; motorLevel = slideLevel = crawlLevel = 0;
        }

        // ---- events (called every frame live; scripted tests may call it after every physics tick) ----------------------
        public void Observe()
        {
            if (!bound || !Ours()) return;
            float now = Now, dt = now - lastSim;
            if (dt < -.05f) { lastPlayed.Clear(); Snapshot(); quietUntil = now + .5f; Play("game_retry", .5f); return; } // Retry
            if (dt <= 0) return;
            lastSim = now;
            bool quiet = now < quietUntil;

            // creature
            int commandCount = game.Feedback != null ? game.Feedback.CommandCount : 0;
            if (commandCount != commands) { commands = commandCount; idleSince = now; if (!quiet) PlayAny(.55f, BodyPan(), "creature_ack_1", "creature_ack_2", "creature_ack_3"); }
            int frag = game.Matter.TotalFragmentCount;
            if (frag != fragments)
            {
                if (!quiet) { if (frag > fragments) Play("creature_split", .7f, BodyPan()); else Play(frag == 1 ? "creature_merge_full" : "creature_merge", .7f, BodyPan()); }
                fragments = frag;
            }
            if (game.Attached && !attached && !quiet) Play("creature_grab", .5f, BodyPan());
            attached = game.Attached;
            int out_ = game.Matter.EscapedCount;
            if (out_ > escaped && escaped == 0 && !quiet) Play("creature_exit", .8f, 0);
            escaped = out_;

            // body falling and landing, from the selected body's centre
            Vector3 centre = game.Root.InverseTransformPoint(game.Motion.Centre(game.Motion.Selected));
            bool valid = !(float.IsNaN(centre.x) || float.IsNaN(centre.y) || float.IsNaN(centre.z)); // NaN once everything is out
            if (valid && haveCentre)
            {
                Vector3 v = (centre - lastCentre) / dt;
                bool carried = AnyTubeTravelling() || AnySwinging() || AnyLiftMoving();
                if (!carried && v.y < -.30f) { falling = true; fallSpeed = Mathf.Max(fallSpeed, -v.y); }
                else if (falling && v.y > -.05f)
                {
                    falling = false;
                    if (!quiet && !carried) Play("creature_land", Mathf.Clamp01(.25f + fallSpeed * .6f) * .6f, BodyPan());
                    fallSpeed = 0;
                }
                float speed = carried ? 0 : new Vector2(v.x, v.z).magnitude + Mathf.Max(0, v.y) * .5f;
                crawlLevel = Mathf.Lerp(crawlLevel, Mathf.Clamp01((speed - .008f) / .05f), Mathf.Clamp01(dt * 8));
            }
            haveCentre = valid; if (valid) lastCentre = centre;

            // mechanisms: what moves, and what arrives. A rail that has travelled at least 1 cm since it last arrived
            // sounds "arrived" when it reaches either end. Horizontal rails and loose props slide on the floor; vertical
            // rails, powered gear trains and turning tables run the motor.
            float motor = 0, slide = 0; Vector3 motorAt = Vector3.zero, slideAt = Vector3.zero;
            for (int i = 0; i < rails.Length; i++)
            {
                var r = rails[i]; float p = r.Position, step = Mathf.Abs(p - railPosition[i]), speed = step / dt;
                railPosition[i] = p; railTravel[i] += step;
                if (Mathf.Abs(r.WorldAxis.y) < .5f) { slide += speed; slideAt += r.Body.position * speed; }
                else { motor += speed; motorAt += r.Body.position * speed; }
                bool end = r.AtEnd, start = p <= r.CatchTolerance;
                if (((end && !atEnd[i]) || (start && !atStart[i])) && railTravel[i] >= .01f && !liftRails.Contains(r))
                {
                    if (!quiet) Play("mech_arrive", ArriveVolume, Pan(r.Body.position), .12f);
                    railTravel[i] = 0;
                }
                atEnd[i] = end; atStart[i] = start;
            }
            for (int i = 0; i < loose.Length; i++)
            {
                Vector3 at = loose[i].Body.position, d = at - loosePosition[i]; loosePosition[i] = at;
                float speed = new Vector2(d.x, d.z).magnitude / dt;
                if (speed > .004f) { slide += speed; slideAt += at * speed; }
            }
            for (int i = 0; i < tables.Length; i++)
            {
                float turn = Mathf.Abs(tables[i].Angle - tableAngle[i]) / dt * .002f; tableAngle[i] = tables[i].Angle;
                motor += turn; motorAt += tables[i].transform.position * turn;
            }
            for (int i = 0; i < tasks.Length; i++)
            {
                bool op = tasks[i].Phase == COgheTapRail.TaskPhase.Operating;
                if (op && !operating[i] && !quiet) Play("creature_grab", .45f, BodyPan());
                operating[i] = op;
                string f = tasks[i].LastFailure;
                if (!string.IsNullOrEmpty(f) && f != failures[i] && !quiet && Refusals() == refusals) Play("creature_hm", .5f, BodyPan(), 1.2f); // a refusal has its own sound
                failures[i] = f;
            }
            int refused = Refusals();
            if (refused > refusals && !quiet) Play(LastRefusal() == COgheMechanism.Refusal.TooHeavy ? "creature_hmph" : "glass_tok", .5f, BodyPan(), .2f);
            refusals = refused;
            for (int i = 0; i < tasks.Length && i < overloaded.Length; i++)
            {
                bool o = tasks[i].Overloaded;
                if (o && !overloaded[i] && !quiet) Play("creature_grumble", .5f, BodyPan(), .5f);
                overloaded[i] = o;
            }
            for (int i = 0; i < pads.Length; i++)
            {
                bool a = pads[i].Active;
                if (a != active[i] && !quiet) Play(a ? "mech_pad_on" : "mech_pad_off", a ? .5f : .35f, Pan(pads[i].transform.position));
                active[i] = a;
            }
            for (int i = 0; i < trains.Length; i++)
            {
                bool m = trains[i].Meshed;
                if (m && !meshed[i] && !quiet) Play("mech_gear_mesh", .6f, 0);
                meshed[i] = m;
                if (trains[i].Powered) { motor += .012f; motorAt += trains[i].transform.position * .012f; } // running, even with its output at rest
            }
            for (int i = 0; i < lifts.Length; i++)
            {
                if (lifts[i].Trips != trips[i] && !quiet) Play("mech_lift_ding", .5f, Pan(lifts[i].transform.position));
                trips[i] = lifts[i].Trips;
                // The tray button clicks down when pressed and pops back up on arrival.
                var button = lifts[i].Button;
                if (button != liftButtons[i] && !quiet)
                {
                    if (button == COghePassengerLift.ButtonState.Pressing) Play("mech_latch", .45f, Pan(lifts[i].Panel.position));
                    else if (button == COghePassengerLift.ButtonState.Releasing) Play("metal_clink", .35f, Pan(lifts[i].Panel.position));
                }
                liftButtons[i] = button;
            }
            for (int i = 0; i < tables.Length; i++)
            {
                if (tables[i].Caught && !caught[i] && !quiet) Play("mech_latch", .6f, Pan(tables[i].transform.position));
                caught[i] = tables[i].Caught;
            }
            for (int i = 0; i < swings.Length; i++)
            {
                var ph = swings[i].Phase;
                if (ph == COgheSwingTransfer.SwingPhase.Swinging && swingPhase[i] != ph && !quiet) Play("creature_swing", .6f, BodyPan());
                swingPhase[i] = ph;
                if (swings[i].Landings != landings[i] && !quiet) Play("creature_land", .5f, BodyPan());
                landings[i] = swings[i].Landings;
            }
            for (int i = 0; i < tubes.Length; i++)
            {
                bool t = tubes[i].AnyTravelling;
                if (t != travelling[i] && !quiet) Play(t ? "tube_in" : "tube_out", .6f, BodyPan());
                travelling[i] = t;
            }
            motorLevel = Mathf.Lerp(motorLevel, Mathf.Clamp01(motor / .05f), Mathf.Clamp01(dt * 6));
            slideLevel = Mathf.Lerp(slideLevel, Mathf.Clamp01((slide - .004f) / .06f), Mathf.Clamp01(dt * 8));
            if (motor > 1e-4f) motorPan = Mathf.Lerp(motorPan, Pan(motorAt / motor), Mathf.Clamp01(dt * 4));
            if (slide > 1e-4f) slidePan = Mathf.Lerp(slidePan, Pan(slideAt / slide), Mathf.Clamp01(dt * 4));

            // game flow
            if (game.Owner.Completed && !completed) { Play("game_win", .9f, 0); duckUntil = Time.unscaledTime + 4.5f; }
            completed = game.Owner.Completed;
            if (game.Owner.Lost && !lost) Play("game_fail", .6f, 0);
            lost = game.Owner.Lost;

            // a curious sound now and then when left alone; far-off lab sounds, rarely
            bool resting = !completed && !lost && !game.Owner.Paused && crawlLevel < .05f && motorLevel < .05f && slideLevel < .05f;
            if (!resting) idleSince = now;
            if (resting && now - idleSince > 1 && now > nextIdle) { PlayAny(.4f, BodyPan(), "creature_curious_1", "creature_curious_2"); nextIdle = now + Random.Range(16f, 28f); }
            if (now > nextFar) { PlayAny(.22f, Random.Range(-.7f, .7f), "lab_far_beep", "lab_far_clink", "lab_far_thud"); nextFar = now + Random.Range(25f, 55f); }
        }

        private bool AnyTubeTravelling() { foreach (var t in tubes) if (t.AnyTravelling) return true; return false; }
        private bool AnySwinging() { foreach (var s in swings) if (s.Phase == COgheSwingTransfer.SwingPhase.Swinging) return true; return false; }
        private COgheMechanism.Refusal LastRefusal()
        {
            COgheMechanism last = null; foreach (var m in game.Mechanisms) if (m != null && m.Refusals > 0 && (last == null || m.RefusedAt > last.RefusedAt)) last = m;
            return last != null ? last.LastRefusal : COgheMechanism.Refusal.None;
        }
        private int Refusals() { int n = 0; if (game != null) foreach (var m in game.Mechanisms) if (m != null) n += m.Refusals; return n; }
        private bool AnyLiftMoving() { foreach (var l in lifts) if (l.Moving) return true; return false; }

        // ---- mixing -----------------------------------------------------------------------------------------------------
        private void Mix(bool ours)
        {
            float fade = Mathf.Clamp01(Time.unscaledDeltaTime * 2f);
            bool paused = ours && game.Owner.Paused;
            foreach(var voice in voices)voice.mute=!EffectsOn;
            float music = MusicOn && !COgheIntro.Playing && (ours || Music.isPlaying) ? MusicVolume * (paused ? .45f : 1) * (Time.unscaledTime < duckUntil ? .35f : 1) : 0;
            Drive(Music, music, fade);
            bool moving = ours && EffectsOn && !paused && !game.Owner.Completed;
            Drive(Motor, moving ? MotorVolume * motorLevel : 0, Mathf.Clamp01(Time.unscaledDeltaTime * 8));
            Motor.pitch = .9f + motorLevel * .2f; Motor.panStereo = Mathf.Clamp(motorPan, -.6f, .6f);
            Drive(Slide, moving ? SlideVolume * slideLevel : 0, Mathf.Clamp01(Time.unscaledDeltaTime * 10));
            Slide.pitch = .9f + slideLevel * .25f; Slide.panStereo = Mathf.Clamp(slidePan, -.6f, .6f);
        }

        private static void Drive(AudioSource source, float target, float rate)
        {
            if (source.clip == null) return;
            source.volume = Mathf.MoveTowards(source.volume, target, Mathf.Max(rate, .001f));
            if (target > 0 && !source.isPlaying) source.Play();
            else if (target <= 0 && source.volume <= .001f && source.isPlaying) source.Pause();
        }

        // ---- one-shots --------------------------------------------------------------------------------------------------
        /// <summary>Every one-shot that passes the repeat guard (previews mix a reel's sound from this).</summary>
        public static System.Action<string, float> Heard;
        public void Play(string name, float volume, float pan = 0, float minGap = .06f)
        {
            float now = Now;
            if (lastPlayed.TryGetValue(name, out float last) && now - last < minGap && now >= last) return;
            lastPlayed[name] = now;
            Played[name] = Played.TryGetValue(name, out int n) ? n + 1 : 1;
            Heard?.Invoke(name, volume);
            if (!EffectsOn) return;
            var clip = Clip(name); if (clip == null) return;
            var voice = voices[nextVoice]; nextVoice = (nextVoice + 1) % voices.Length;
            voice.clip = clip; voice.volume = volume * Random.Range(.9f, 1f); voice.pitch = Random.Range(.97f, 1.03f);
            voice.panStereo = Mathf.Clamp(pan, -.6f, .6f); voice.Play();
        }

        private void PlayAny(float volume, float pan, params string[] names) => Play(names[Random.Range(0, names.Length)], volume, pan, .12f);

        private float BodyPan() => game != null ? Pan(game.Motion.Centre(game.Motion.Selected)) : 0;
        private float Pan(Vector3 world)
        {
            var view = game != null && game.Owner != null ? game.Owner.View : null;
            if (view == null) return 0;
            var p = view.WorldToViewportPoint(world); return (p.x - .5f) * 1.2f;
        }

        /// <summary>HUD buttons.</summary>
        public static void UiTap() { if (Instance != null) Instance.PlayUi("ui_tap", .5f); }
        public static void Happy() { if (Instance != null) Instance.PlayUi("creature_happy", .6f); }
        private void PlayUi(string name, float volume)
        {
            Played[name] = Played.TryGetValue(name, out int n) ? n + 1 : 1;
            if (!EffectsOn) return;
            var clip = Clip(name); if (clip == null) return;
            var voice = voices[nextVoice]; nextVoice = (nextVoice + 1) % voices.Length;
            voice.clip = clip; voice.volume = volume; voice.pitch = 1; voice.panStereo = 0; voice.Play();
        }
    }
}
