using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// The opening: a 20 s wordless motion comic over level 1 that ends in an ink dissolve revealing the live glass box.
    /// Layers load by name from Resources/COgheIntro, prepared by Tools/intro/prepare_intro_layers.py (layout.txt gives
    /// each trimmed texture's rect in the canvas it was painted on, and where COghe sits in the layers that hold it).
    /// It plays once, the first time a player opens position 1 of the Spatial catalog, with the level paused underneath;
    /// never in tests or the native proof run. Drawing is immediate mode (Graphics.DrawTexture): no canvas, no extra
    /// camera, nothing added to the scenes. A missing layer is skipped, so art can be swapped one file at a time.
    /// </summary>
    public sealed class COgheIntro : MonoBehaviour
    {
        public const float Length = 20f;
        /// <summary>Where a skip lands: COghe already seated in the drawn box, just before the dissolve.</summary>
        public const float SkipTo = 17.3f;
        private const float Seat = 16.3f, DissolveStart = 18.0f, DissolveEnd = 19.6f;
        private const string SeenKey = "coghe.intro.seen", Folder = "COgheIntro/", Catalog = "coghe.spatial.pilot";
        private static readonly Vector2 Panel = new Vector2(1080, 2340);
        // Level 1's box floor on the 1080×2340 reference frame (REFERENCES/level01_match.json) and in the box's own space.
        // The lab plate was registered onto that frame, so fitting these corners to the live camera lands it on the real box.
        private static readonly Vector2[] RefFloor = { new Vector2(266, 1549), new Vector2(1019, 1388), new Vector2(61, 1218), new Vector2(814, 1057) };
        private static readonly Vector3[] LocalFloor = { new Vector3(-.40f, -.30f, -.30f), new Vector3(.40f, -.30f, -.30f), new Vector3(-.40f, -.30f, .30f), new Vector3(.40f, -.30f, .30f) };
        private static readonly Vector2 RefSeat = new Vector2(360, 1437);
        private const float RefCogheWidth = 92;    // COghe's body on the reference frame (the painted one has tendrils besides)
        // Canvas each layer was painted on (LAYER_MANIFEST.md), in panel pixels; backgrounds are the whole panel.
        private static readonly Dictionary<string, Vector2> Canvases = new Dictionary<string, Vector2>
        {
            { "S1_FX_METEOR", new Vector2(1024, 1024) }, { "S2_FX_BLAST", new Vector2(1024, 1024) },
            { "S3_SOLDIERS", new Vector2(1080, 900) }, { "S3_SCIENTIST", new Vector2(800, 1600) },
            { "S4_SPHERE_CLOSED", new Vector2(900, 900) }, { "S4_SPHERE_OPEN", new Vector2(900, 900) }, { "S5_COGHE_RISE", new Vector2(900, 900) },
            { "S5_SOLDIERS_REACT", new Vector2(1080, 900) }, { "S6_SCIENTIST_KNEEL", new Vector2(1000, 1800) },
            { "S7_HANDS_PLACE", new Vector2(1080, 1300) }, { "S7_COGHE_SEATED", new Vector2(400, 400) },
        };
        // Points read off the paintings (canvas 0..1): the meteor's head, the blast's core, the sphere's lower pole and
        // cradle (Codex handoff: 625,1114 and 625,820 of 1254), her open palm while kneeling; and on the panel, the
        // crater centre.
        private static readonly Vector2 MeteorHead = new Vector2(.204f, .785f), BlastCore = new Vector2(.507f, .59f);
        private static readonly Vector2 SpherePole = new Vector2(.498f, .888f), SphereCradle = new Vector2(.498f, .654f);
        private static readonly Vector2 KneelPalm = new Vector2(.60f, .476f);
        private static readonly Vector2 CraterCentre = new Vector2(530, 1180);
        private static readonly Vector2 SpherePlace = new Vector2(535, 1560);   // the lower pole, on the floor patch
        private const float SphereWidth = 1100, RiseScale = .58f;               // S5_COGHE_RISE against the sphere canvas (handoff)

        /// <summary>Every layer the shots draw (Resources/COgheIntro).</summary>
        public static readonly string[] LayerNames =
        {
            "S1_BG_SKY", "S1_FX_METEOR", "S2_BG_CRATER", "S2_FX_BLAST", "S3_BG_RIM", "S3_SOLDIERS", "S3_SCIENTIST",
            "S4_BG_FLOOR", "S4_SPHERE_CLOSED", "S4_SPHERE_OPEN", "S5_COGHE_RISE", "S5_SOLDIERS_REACT", "S6_BG_SITE",
            "S6_SCIENTIST_KNEEL", "S6_CLOSEUP", "S7_BG_LAB_MATCH", "S7_HANDS_PLACE", "S7_COGHE_SEATED",
        };

        private static COgheIntro current;
        public static bool Playing => current != null;
        public static bool Seen
        {
            get => PlayerPrefs.GetInt(SeenKey, 0) == 1;
            set { PlayerPrefs.SetInt(SeenKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }
        /// <summary>The intro ends on level 1's box, so it can only play (or be replayed) there.</summary>
        public static bool Fits(VenomCampaign g) => g != null && g.Definition != null && g.Definition.ProgressKey == Catalog && g.Definition.Order == 1 && !g.Home;

        /// <summary>Playback speed (tests run it faster).</summary>
        public float Speed = 1;
        public float Clock { get; set; }
        public bool Finished { get; private set; }
        public bool Started => mat != null;
        private bool running;
        private Action completed;
        public bool Replay {get;private set;}

        private VenomCampaign game;
        private Material mat;
        private Texture2D noise, streaks, burst, star, white, eyeWhite, pupil, smile;
        private readonly List<Texture2D> generated = new List<Texture2D>();
        private readonly Dictionary<string, Art> art = new Dictionary<string, Art>();
        private Dictionary<string, string[]> layout;
        private AudioSource score;
        private GUIStyle skipStyle;
        private COgheUIArt productArt;
        private float W, H, s0; private Vector2 o0;   // target size and the cover-fit of the panel on it

        private sealed class Art { public Texture Tex; public Rect Trim = new Rect(0, 0, 1, 1); public Vector2 Canvas = Panel; public Vector4 Coghe; }
        private struct Cam
        {
            public float Zoom; public Vector2 Focus, Pan, Shake;
            public Cam(float zoom, Vector2 focus, Vector2 pan = default, Vector2 shake = default) { Zoom = zoom; Focus = focus; Pan = pan; Shake = shake; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => TryAutoPlay();
            TryAutoPlay();
        }

        private static void TryAutoPlay()
        {
            if (current != null || Seen || !VenomCampaignSave.PersistenceEnabled || Application.isBatchMode) return;
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-coghe-view-proof") >= 0) return;
            var g = FindFirstObjectByType<VenomCampaign>();
            if (Fits(g)&&!COgheProductMode.Applies(g)) Play(g);
        }

        /// <summary>Plays the intro over this level (level 1: the last panel matches its glass box).</summary>
        public static COgheIntro Play(VenomCampaign g, Action onComplete=null, bool replay=false)
        {
            if (current != null) return current;
            var intro = new GameObject("COghe intro").AddComponent<COgheIntro>();
            intro.game = g; intro.completed=onComplete;intro.Replay=replay;current = intro;
            return intro;
        }

        private IEnumerator Start()
        {
            for (float waited = 0; game != null && (game.Owner == null || game.Matter == null) && waited < 5; waited += Time.unscaledDeltaTime)
                yield return null;   // the level is still starting
            if (game == null || game.Owner == null || game.Matter == null) { Finish(); yield break; }
            // everything is loaded before the level is paused and the clock runs: no layer arrives mid-shot, and a
            // failure leaves the level playable instead of paused under nothing
            try
            {
                var shader = Resources.Load<Shader>(Folder + "IntroLayer");
                var material = new Material(shader != null ? shader : Shader.Find("Unlit/Transparent"));
                MakeTextures();
                LoadLayout();
                foreach (var name in LayerNames) Get(name);
                mat = material;
                if(game.ProductUI!=null)productArt=new COgheUIArt();
            }
            catch (Exception e) { Debug.LogException(e); Finish(); yield break; }
            if (!game.Owner.Paused) game.Owner.TogglePause();
            var clip = Resources.Load<AudioClip>("COgheAudio/intro_score");
            if (clip != null && COgheAudio.MusicOn)
            {
                score = gameObject.AddComponent<AudioSource>();
                score.clip = clip; score.volume = .9f; score.ignoreListenerPause = true; score.playOnAwake = false;
            }
        }

        private void Update()
        {
            if (game == null || game.Owner == null) { Finish(); return; }   // the level was left
            if (!Started || Finished) return;
            if (!game.Owner.Paused)
            {
                // Back / Escape toggled the pause: take it as a skip, and keep the level waiting under the comic
                Skip();
                if (!game.Owner.Completed && !game.Owner.Lost) game.Owner.TogglePause();
            }
            if (!running) { running = true; if (score != null) score.Play(); return; }   // picture and score start together
            Clock += Mathf.Min(Time.unscaledDeltaTime, 1f / 20) * Speed;               // a slow frame cannot eat a shot
            if (score != null) score.pitch = Mathf.Clamp(Speed, .1f, 3);
            if (Clock >= Length) Finish();
        }

        /// <summary>Jumps to COghe seated in the drawn box: a skip still ends on the dissolve into the game.</summary>
        public void Skip()
        {
            if (Clock >= SkipTo) return;
            Clock = SkipTo;
            if (score != null && score.clip != null) score.time = Mathf.Min(SkipTo, score.clip.length - .01f);
        }

        private void Finish()
        {
            if (Finished) return;
            Finished = true;
            // the intro hands over to a running level (also when it was replayed from the pause screen)
            if (game != null && game.Owner != null && game.Owner.Paused && !game.Owner.Completed && !game.Owner.Lost) game.Owner.TogglePause();
            if (VenomCampaignSave.PersistenceEnabled) Seen = true;
            if (current == this) current = null;
            var callback=completed;completed=null;callback?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            // destroyed without finishing (not by a scene change, which resets the time scale itself): never leave the level paused
            if (!Finished && Started && game != null && game.Owner != null && game.Owner.Paused) game.Owner.TogglePause();
            if (current == this) current = null;
            foreach (var t in generated) if (t != null) Destroy(t);
            foreach (var a in art.Values) if (a.Tex != null) Resources.UnloadAsset(a.Tex);   // the art only lives while it plays
            if (mat != null) Destroy(mat);
            productArt?.Dispose();
        }

        // ---- drawing ----------------------------------------------------------------------------------------------------
        private void OnGUI()
        {
            if (!Started || Finished) return;
            GUI.depth = -1000; GUI.matrix = Matrix4x4.identity;
            if (Event.current.type == EventType.Repaint) Draw(Screen.width, Screen.height);
            if(productArt!=null)
            {
                float unit=Mathf.Min(Screen.safeArea.width/360f,Screen.safeArea.height/640f);var safe=Screen.safeArea;
                if(Replay&&productArt.GuiButton(new Rect(safe.xMin+24*unit,Screen.height-safe.yMax+25*unit,52*unit,52*unit),COgheIcon.Back)){COgheAudio.UiTap();Finish();return;}
                // Mrk: low and centred, where a thumb rests when the phone is held in one hand
                if(Clock>1f&&Clock<SkipTo-.3f&&productArt.GuiPill(SkipButton(unit,safe),COgheIcon.Skip,"Skip",unit)){COgheAudio.UiTap();Skip();}
                return;
            }
            if (Clock > 1f && Clock < SkipTo - .3f)
            {
                float u = Mathf.Min(Screen.width / 540f, Screen.height / 960f);
                if (skipStyle == null) skipStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(20 * u), fontStyle = FontStyle.Bold };
                var safe = Screen.safeArea;
                if (GUI.Button(new Rect(safe.center.x - 32 * u, Screen.height - safe.yMin - 70 * u, 64 * u, 42 * u), "››", skipStyle)) { COgheAudio.UiTap(); Skip(); }
            }
        }

        /// <summary>The skip button in GUI coordinates (top-left origin): a pill centred at the bottom of the safe area.</summary>
        public static Rect SkipButton(float unit, Rect safe) => new Rect(safe.center.x - 62 * unit, Screen.height - safe.yMin - 76 * unit, 124 * unit, 48 * unit);

        /// <summary>Draws the frame at the current clock over the active target: the screen in OnGUI, or a RenderTexture
        /// under a top-left pixel matrix (GL.LoadPixelMatrix(0, w, h, 0)).</summary>
        public void Draw(float width, float height)
        {
            if (!Started) return;
            W = width; H = height;
            s0 = Mathf.Max(W / Panel.x, H / Panel.y); o0 = new Vector2((W - Panel.x * s0) * .5f, (H - Panel.y * s0) * .5f);
            mat.SetVector("_TargetSize", new Vector4(W, H, 0, 0));
            float t = Clock;
            if (t < 1.8f) Fall(t);
            else if (t < 3.0f) Impact(t);
            else if (t < 5.6f) Arrival(t);
            else if (t < 7.6f) Sphere(t);
            else if (t < 9.6f) Emerge(t);
            else if (t < 11.4f) Kneel(t);
            else if (t < 14.6f) CloseUp(t);
            else if (t < Seat) Place(t);
            else Seated(t);
            // no printed-dot overlay: the redrawn comic is clean ink and smooth colour (Mrk, 01/10: the grain read as AI)
        }

        // Shots ----------------------------------------------------------------------------------------------------------
        private void Fall(float t)
        {
            float u = Seg(t, 0, 1.8f);
            var cam = new Cam(1.10f - .08f * Smooth(u), Panel * .5f, new Vector2(0, -30 * u));
            Background("S1_BG_SKY", cam, .4f);
            // the head travels against its own trail, accelerating toward the horizon on the left
            var end = new Vector2(360, 1540); var dir = new Vector2(.72f, -.70f).normalized;
            float m = .15f + .85f * Mathf.Pow(u, 1.7f);
            Put("S1_FX_METEOR", MeteorHead, end + dir * 1500 * (1 - m), 700 + 320 * m, cam);
            FullScreen(streaks, .28f * Smooth(Seg(u, .25f, .8f)));
            FullScreen(white, Seg(t, 1.62f, 1.8f));
        }

        private void Impact(float t)
        {
            float since = t - 1.8f, u = Seg(t, 1.8f, 3.0f);
            var cam = new Cam(1.18f - .15f * EaseOut(u), CraterCentre, default, Shake(30 * (1 - u) * (1 - u), t));
            float invert = since > .05f && since < .16f ? 1 : 0;   // the comic impact frame: ink flips for a beat
            Background("S2_BG_CRATER", cam, .5f, invert);
            Put("S2_FX_BLAST", BlastCore, CraterCentre, 1024 * (.55f + .9f * EaseOut(Seg(since, 0, .5f))), cam, 1, 1 - Seg(u, .55f, 1f), invert);
            FullScreen(burst, .6f * (1 - u));
            FullScreen(white, since < .05f ? 1 : 1 - Seg(since, .05f, .25f));
        }

        private void Arrival(float t)
        {
            float u = Seg(t, 3.0f, 5.6f);
            var cam = new Cam(1.06f - .06f * Smooth(u), new Vector2(540, 1400), new Vector2(0, 30 * (1 - u)));
            Background("S3_BG_RIM", cam, .35f);
            float step = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 1.7f));
            Put("S3_SOLDIERS", new Vector2(.5f, 1), new Vector2(400, 2010 - 10 * step), 980 * (.9f + .1f * Smooth(u)), cam);
            float her = EaseOut(Seg(t, 3.25f, 4.3f));
            Put("S3_SCIENTIST", new Vector2(.5f, 1), new Vector2(Mathf.Lerp(1180, 770, her), 2290 - 12 * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 1.5f))), 820, cam, 1.15f);
        }

        private void Sphere(float t)
        {
            float u = Seg(t, 5.6f, 7.6f);
            var cam = new Cam(1f + .10f * Smooth(u), new Vector2(535, 1300));
            Background("S4_BG_FLOOR", cam, .5f);
            bool open = t >= 7.0f;
            float pop = open ? 1 + .06f * (1 - EaseOut(Seg(t, 7.0f, 7.3f))) : 1;
            float glow = open ? 1 : 1 + .12f * (.5f + .5f * Mathf.Sin(t * 8));   // the seams breathe before it opens
            Put(open ? "S4_SPHERE_OPEN" : "S4_SPHERE_CLOSED", SpherePole, SpherePlace, SphereWidth * pop, cam, 1, 1, 0, new Color(glow, glow, glow * .95f, 1));
            if (open) FullScreen(white, .85f * (1 - Seg(t, 7.0f, 7.14f)));
        }

        private void Emerge(float t)
        {
            float u = Seg(t, 7.6f, 9.6f), react = EaseOut(Seg(t, 7.8f, 8.15f));
            var cam = new Cam(1.10f + .06f * Smooth(u), new Vector2(535, 1300), default, Shake(10 * (react > 0 && react < 1 ? 1 - react : 0), t));
            Background("S4_BG_FLOOR", cam, .5f);
            Put("S4_SPHERE_OPEN", SpherePole, SpherePlace, SphereWidth, cam);
            // COghe blooms up out of the cradle: its body foot pinned to the cradle, a liquid overshoot, then a wobble
            var a = Get("S5_COGHE_RISE");
            if (a != null)
            {
                Vector2 cradle = SpherePlace + (SphereCradle - SpherePole) * SphereWidth;
                var foot = a.Coghe != Vector4.zero ? new Vector2(a.Coghe.x, a.Coghe.w) : new Vector2(.5f, .84f);
                float grow = EaseOutBack(Seg(t, 7.62f, 7.95f)), wob = Wobble(t - 7.95f);
                Put("S5_COGHE_RISE", foot, cradle, SphereWidth * RiseScale * grow, cam, 1, 1, 0, Color.white, 1 - .10f * wob, 1 + .14f * wob);
                // it opens its eyes wide on the world, glances at each soldier flinching back, blinks
                if (a.Coghe != Vector4.zero && t > 7.9f)
                {
                    var size = new Vector2(SphereWidth * RiseScale * grow * (1 - .10f * wob), SphereWidth * RiseScale * grow * a.Canvas.y / a.Canvas.x * (1 + .14f * wob)) * Scale(cam, 1);
                    var topLeft = ToScreen(cradle, cam, 1) - Vector2.Scale(foot, size);
                    var look = Glance(t, 8.3f, Vector2.zero, new Vector2(-1, .1f)); look = Glance(t, 8.7f, look, new Vector2(1, .1f)); look = Glance(t, 9.3f, look, new Vector2(0, -.3f));
                    Eyes(topLeft + new Vector2(a.Coghe.x * size.x, (a.Coghe.y - .05f * a.Coghe.z) * size.y), a.Coghe.z * size.x, look,
                        Blink(t, 9.15f), 0, 1.18f - .18f * Smooth(Seg(t, 8.3f, 8.7f)), 0, Seg(t, 7.9f, 8.0f), 1 + .14f * wob);
                }
            }
            // the two soldiers at the frame edges flinch back: each half slides out through its own edge
            float wide = W * 1.04f * (1 + .05f * react);
            var at = new Vector2(W * .5f, H * 1.02f + 40 * s0 * react);
            PutScreen("S5_SOLDIERS_REACT", new Vector2(.5f, 1), at + new Vector2(-70 * s0 * react, 0), wide, new Rect(0, 0, .5f, 1));
            PutScreen("S5_SOLDIERS_REACT", new Vector2(.5f, 1), at + new Vector2(70 * s0 * react, 0), wide, new Rect(.5f, 0, .5f, 1));
        }

        private void Kneel(float t)
        {
            float u = Seg(t, 9.6f, 11.4f);
            var cam = new Cam(1f + .05f * Smooth(u), new Vector2(540, 1300));
            Background("S6_BG_SITE", cam, .4f);
            // she is held to the right edge: the rifle barrel she lowers enters from outside the frame
            float tall = H * .86f, wide = tall * 1000 / 1800f;
            var corner = new Vector2(W + 6 * s0, H * 1.02f - (1 - EaseOut(Seg(t, 9.6f, 10.1f))) * 30 * s0);
            var topLeft = corner - new Vector2(wide, tall);
            PutScreen("S6_SCIENTIST_KNEEL", new Vector2(1, 1), corner, wide);
            // COghe hops out of the crater onto her open palm
            float body = wide * .10f, k = Seg(t, 10.1f, 10.6f);
            if (k <= 0) return;
            Vector2 rest = topLeft + Vector2.Scale(KneelPalm, new Vector2(wide, tall)) - new Vector2(0, body * .45f);
            var from = new Vector2(W * 1.05f, H * .92f);
            var p = Vector2.Lerp(from, rest, k) - new Vector2(0, H * .14f * 4 * k * (1 - k));
            float wob = k >= 1 ? Wobble(t - 10.6f) : 0;
            Creature(p, body, new Color(1, .92f, .82f, 1), 1 + .12f * wob, 1 - .16f * wob);
            // on the way it looks where it is jumping; landed, a happy squint, then up at her face
            var toward = k < 1 ? (rest - p).normalized : new Vector2(.55f, -.8f);
            SeatedEyes(p, body, 1 + .12f * wob, 1 - .16f * wob, toward, k >= 1 && t < 11.0f ? 1 : 0, 1, 0);
        }

        private void CloseUp(float t)
        {
            float u = Seg(t, 11.4f, 14.6f);
            var a = Get("S6_CLOSEUP");
            var focus = a != null && a.Coghe != Vector4.zero ? new Vector2(a.Coghe.x * Panel.x, a.Coghe.y * Panel.y) : new Vector2(670, 1243);
            float zoom = 1f + .22f * Smooth(u);
            var cam = new Cam(zoom, focus);
            Background("S6_CLOSEUP", cam, 1);
            // on her palm: it looks up at her, blinks, and smiles
            if (a != null && a.Coghe != Vector4.zero)
                Eyes(ToScreen(focus - new Vector2(0, .06f * a.Coghe.z * Panel.x), cam, 1), a.Coghe.z * Panel.x * Scale(cam, 1),
                    new Vector2(-.55f, -.85f), Blink(t, 12.25f), Smooth(Seg(t, 13.15f, 13.3f)));
            // a few glints around the creature on her palm
            Glint(focus + new Vector2(-120, -150), 12.0f, t, cam, zoom);
            Glint(focus + new Vector2(135, -95), 12.55f, t, cam, zoom);
            Glint(focus + new Vector2(30, -215), 13.2f, t, cam, zoom);
        }

        private void Place(float t)
        {
            // a close panel slides over the close-up: her hands lower COghe onto the glass floor
            float slide = 1 - EaseOut(Seg(t, 14.6f, 14.85f));
            if (slide > 0) CloseUp(14.6f - .001f);
            var hands = Get("S7_HANDS_PLACE");
            Vector2 anchor = hands != null && hands.Coghe != Vector4.zero ? new Vector2(hands.Coghe.x, hands.Coghe.y) : new Vector2(.44f, .78f);
            float bodyShare = hands != null && hands.Coghe.z > 0 ? hands.Coghe.z : .186f;
            var target = new Vector2(W * .46f, H * .5f);
            // big enough that the forearms' cut edge stays above the screen, and the right cut edge beyond it
            float wide = Mathf.Max(W * 1.02f, (target.y + H * .03f) / (anchor.y * 1300 / 1080f));
            wide = Mathf.Max(wide, (W - target.x + 12 * s0) / (1 - anchor.x));
            float down = EaseOut(Seg(t, 14.7f, 15.9f)), settle = Mathf.Sin(Seg(t, 15.9f, 16.25f) * Mathf.PI) * 12 * s0;
            float shift = slide * W;
            var at = target + new Vector2(shift, -H * .45f * (1 - down) + settle);
            // the plate behind, zoomed so its seat sits under the creature at the hands' scale, softened like depth of field
            float body = bodyShare * wide, zoom = body / (RefCogheWidth * s0);
            var cam = new Cam(zoom, RefSeat, (target + new Vector2(shift, body * .35f) - (o0 + RefSeat * s0)) / s0);
            Background("S7_BG_LAB_MATCH", cam, 1, 0, 1.2f);
            PutScreen("S7_HANDS_PLACE", anchor, at, wide, new Rect(0, 0, 1, 1), new Color(.97f, .98f, 1, 1));
            // lowered into the box, it looks down at the glass floor coming up, then around
            if (hands != null && hands.Coghe != Vector4.zero)
                Eyes(at - new Vector2(0, .06f * body), body, Glance(t, 15.85f, new Vector2(0, 1), new Vector2(.3f, -.2f)), Blink(t, 16.05f));
        }

        private void Seated(float t)
        {
            float dissolve = Mathf.Pow(Seg(t, DissolveStart, DissolveEnd), 1.4f) * 1.12f;   // opens slowly around COghe, then spreads
            Align(out float s, out Vector2 o);
            Vector2 seat = LiveSeat(s, o);
            // the wide panel settles onto the real camera's framing, then everything dissolves into the live level
            float z = 1 + .035f * (1 - EaseOut(Seg(t, Seat, Seat + .3f)));
            s *= z; o = seat + (o - seat) * z;
            mat.SetVector("_DissolveCentre", new Vector4(seat.x / H, seat.y / H, 0, 0));   // target heights from the top-left
            Quad(white, new Rect(0, 0, W, H), new Color(.93f, .94f, .92f, 1), 1, 0, 0, dissolve);
            var plate = Get("S7_BG_LAB_MATCH");
            if (plate != null) Quad(plate.Tex, new Rect(o.x, o.y, Panel.x * s, Panel.y * s), Color.white, 1, 0, 0, dissolve);
            // COghe hops out of her palm into the box, lands, looks around
            float drop = 1 - EaseIn(Seg(t, Seat, Seat + .16f)), wob = Wobble(t - Seat - .16f);
            float idle = t > Seat + 1.2f ? Mathf.Sin((t - Seat) * 7) * .025f : 0;
            Creature(seat - new Vector2(0, 70 * s * drop), RefCogheWidth * s, Color.white, 1 + .12f * wob + idle, 1 - .16f * wob - idle, dissolve);
            // a squint as it lands, then a look around its new box (left, right, at the player), a blink
            var look = Glance(t, Seat + .5f, Vector2.zero, new Vector2(-1, .15f)); look = Glance(t, Seat + 1.05f, look, new Vector2(1, .15f)); look = Glance(t, Seat + 1.6f, look, Vector2.zero);
            SeatedEyes(seat - new Vector2(0, 70 * s * drop), RefCogheWidth * s, 1 + .12f * wob + idle, 1 - .16f * wob - idle, look,
                t < Seat + .35f && t > Seat + .1f ? 1 : 0, Blink(t, Seat + 2.05f), dissolve);
        }

        // Layers ---------------------------------------------------------------------------------------------------------
        private void Background(string name, Cam cam, float parallax, float invert = 0, float blur = 0)
        {
            var a = Get(name);
            if (a == null) { Quad(white, new Rect(0, 0, W, H), new Color(.05f, .07f, .13f, 1), 1, 0, 0, 0); return; }
            Draw(a, ToScreen(Vector2.zero, cam, parallax), Panel * Scale(cam, parallax), new Rect(0, 0, 1, 1), Color.white, 1, invert, blur, 0);
        }

        /// <summary>A layer whose canvas point <paramref name="pivot"/> (0..1) lands on the panel point <paramref name="at"/>,
        /// its canvas <paramref name="width"/> panel pixels wide, seen through the shot's camera.</summary>
        private void Put(string name, Vector2 pivot, Vector2 at, float width, Cam cam, float parallax = 1, float alpha = 1, float invert = 0,
            Color? tint = null, float sx = 1, float sy = 1)
        {
            var a = Get(name); if (a == null || width <= 0) return;
            Vector2 size = new Vector2(width * sx, width * a.Canvas.y / a.Canvas.x * sy) * Scale(cam, parallax);
            Draw(a, ToScreen(at, cam, parallax) - Vector2.Scale(pivot, size), size, new Rect(0, 0, 1, 1), tint ?? Color.white, alpha, invert, 0, 0);
        }

        /// <summary>The same in screen pixels, for layers held to the frame edges (their painted crops stay outside).</summary>
        private void PutScreen(string name, Vector2 pivot, Vector2 at, float width, Rect crop, Color? tint = null)
        {
            var a = Get(name); if (a == null) return;
            Vector2 size = new Vector2(width, width * a.Canvas.y / a.Canvas.x);
            Draw(a, at - Vector2.Scale(pivot, size), size, crop, tint ?? Color.white, 1, 0, 0, 0);
        }

        private void PutScreen(string name, Vector2 pivot, Vector2 at, float width) => PutScreen(name, pivot, at, width, new Rect(0, 0, 1, 1));

        /// <summary>COghe alone (the seated sprite): body centred on <paramref name="centre"/>, <paramref name="body"/> wide,
        /// squashing from its foot.</summary>
        private void Creature(Vector2 centre, float body, Color tint, float sx, float sy, float dissolve = 0)
        {
            var a = Get("S7_COGHE_SEATED"); if (a == null) return;
            bool known = a.Coghe != Vector4.zero;
            float wide = body / (known ? a.Coghe.z : .48f);
            var foot = known ? new Vector2(a.Coghe.x, a.Coghe.w) : new Vector2(.48f, .78f);
            var ground = centre + new Vector2(0, ((known ? a.Coghe.w - a.Coghe.y : .18f)) * wide);
            var size = new Vector2(wide * sx, wide * sy);
            Draw(a, ground - Vector2.Scale(foot, size), size, new Rect(0, 0, 1, 1), tint, 1, 0, 0, dissolve);
        }

        // COghe's eyes (Mrk 07/10/2026: "tạo lại intro với COghe có mắt"): the same eyes as in the game (white, a dark rim,
        // a pupil with a shine, smiling arcs), drawn over the painted COghe of each shot. Axis-aligned like the paintings.
        /// <summary>A pair of eyes on a painted COghe whose body is <paramref name="body"/> wide, the pair centred on
        /// <paramref name="face"/>. <paramref name="look"/> is a screen direction (y down, length ≤ 1); <paramref name="open"/>
        /// 1 open, 0 shut; <paramref name="happy"/> 1 for smiling arcs.</summary>
        private void Eyes(Vector2 face, float body, Vector2 look, float open = 1, float happy = 0, float big = 1, float dissolve = 0, float alpha = 1, float squash = 1)
        {
            if (body <= 0 || alpha <= .01f || eyeWhite == null) return;
            float w = body * .19f * big, h = body * .245f * big * squash, gap = body * .145f;
            for (int side = -1; side <= 1; side += 2)
            {
                var c = face + new Vector2(side * gap, 0);
                if (happy > .5f) { Quad(smile, new Rect(c.x - w * .62f, c.y - h * .42f, w * 1.24f, h * .62f), Color.white, alpha, 0, 0, dissolve); continue; }
                float hh = h * Mathf.Max(.1f, open);
                Quad(eyeWhite, new Rect(c.x - w * .5f, c.y - hh * .5f, w, hh), Color.white, alpha, 0, 0, dissolve);
                if (open < .4f) continue;   // mid-blink: the lid line only
                float p = w * .5f / big;
                var at = c + new Vector2(look.x * w * .2f, look.y * hh * .17f);
                Quad(pupil, new Rect(at.x - p * .5f, at.y - p * .5f, p, p), Color.white, alpha, 0, 0, dissolve);
            }
        }
        /// <summary>Eyes on the seated sprite, placed like <see cref="Creature"/> (its body centre, squash from its foot).</summary>
        private void SeatedEyes(Vector2 centre, float body, float sx, float sy, Vector2 look, float happy, float open, float dissolve)
        {
            var a = Get("S7_COGHE_SEATED"); if (a == null) return;
            bool known = a.Coghe != Vector4.zero;
            float wide = body / (known ? a.Coghe.z : .48f), below = (known ? a.Coghe.w - a.Coghe.y : .18f) * wide;
            // the body squashes from its foot: its middle moves with it
            var face = centre + new Vector2(0, below) - new Vector2(0, (below + .04f * body) * sy);
            Eyes(face, body * sx, look, open, happy, 1, dissolve, 1, sy);
        }
        private static Vector2 Glance(float t, float at, Vector2 from, Vector2 to) => Vector2.Lerp(from, to, Smooth(Seg(t, at, at + .14f)));
        private static float Blink(float t, float at) => 1 - Mathf.Sin(Seg(t, at, at + .16f) * Mathf.PI);

        private void Glint(Vector2 panelAt, float start, float t, Cam cam, float zoom)
        {
            float k = Seg(t, start, start + .4f);
            if (k <= 0 || k >= 1) return;
            float size = 70 * Mathf.Sin(k * Mathf.PI) * s0 * zoom;
            var p = ToScreen(panelAt, cam, 1);
            Quad(star, new Rect(p.x - size * .5f, p.y - size * .5f, size, size), new Color(1, .97f, .85f, 1), 1, 0, 0, 0);
        }

        private void Draw(Art a, Vector2 topLeft, Vector2 size, Rect crop, Color tint, float alpha, float invert, float blur, float dissolve)
        {
            Rect t = a.Trim;
            float x0 = Mathf.Max(t.xMin, crop.xMin), x1 = Mathf.Min(t.xMax, crop.xMax), y0 = Mathf.Max(t.yMin, crop.yMin), y1 = Mathf.Min(t.yMax, crop.yMax);
            if (x1 <= x0 || y1 <= y0) return;
            var screen = new Rect(topLeft.x + x0 * size.x, topLeft.y + y0 * size.y, (x1 - x0) * size.x, (y1 - y0) * size.y);
            float u0 = (x0 - t.x) / t.width, u1 = (x1 - t.x) / t.width, vTop = (y0 - t.y) / t.height, vBottom = (y1 - t.y) / t.height;
            var source = new Rect(u0, 1 - vBottom, u1 - u0, vBottom - vTop);   // texture space runs bottom-up
            Quad(a.Tex, screen, tint, alpha, invert, blur, dissolve, source);
        }

        private void FullScreen(Texture tex, float alpha) => Quad(tex, new Rect(0, 0, W, H), Color.white, alpha, 0, 0, 0);

        private void Quad(Texture tex, Rect r, Color tint, float alpha, float invert, float blur, float dissolve, Rect? source = null)
        {
            if (tex == null || alpha <= .001f || dissolve >= 1.12f) return;
            mat.SetFloat("_Alpha", alpha); mat.SetFloat("_Invert", invert); mat.SetFloat("_Blur", blur); mat.SetFloat("_Dissolve", dissolve);
            mat.SetColor("_Tint", tint); mat.SetTexture("_NoiseTex", noise);
            mat.SetVector("_QuadRect", new Vector4(r.x, r.y, r.width, r.height));
            Graphics.DrawTexture(r, tex, source ?? new Rect(0, 0, 1, 1), 0, 0, 0, 0, mat);
        }

        // Geometry -------------------------------------------------------------------------------------------------------
        private Vector2 ToScreen(Vector2 p, Cam cam, float parallax)
        {
            float z = 1 + (cam.Zoom - 1) * parallax;
            return o0 + cam.Focus * s0 + (p - cam.Focus) * s0 * z + cam.Pan * s0 * parallax + cam.Shake;
        }

        private float Scale(Cam cam, float parallax) => s0 * (1 + (cam.Zoom - 1) * parallax);

        /// <summary>Screen placement of the 1080×2340 lab panel that puts its drawn box on the live box: a least-squares
        /// scale and offset over the four projected floor corners.</summary>
        private void Align(out float s, out Vector2 o)
        {
            var view = game != null && game.Owner != null ? game.Owner.View : null;
            if (view == null) { s = s0; o = o0; return; }
            var p = new Vector2[4]; Vector2 pm = Vector2.zero, rm = Vector2.zero;
            for (int i = 0; i < 4; i++) { p[i] = Project(view, game.Root.TransformPoint(LocalFloor[i])); pm += p[i] * .25f; rm += RefFloor[i] * .25f; }
            float num = 0, den = 0;
            for (int i = 0; i < 4; i++) { num += Vector2.Dot(RefFloor[i] - rm, p[i] - pm); den += (RefFloor[i] - rm).sqrMagnitude; }
            s = den > 0 ? num / den : s0; o = pm - rm * s;
        }

        /// <summary>Where COghe really is on screen (it may have moved when the intro is replayed from the pause screen).</summary>
        private Vector2 LiveSeat(float s, Vector2 o)
        {
            var view = game != null && game.Owner != null ? game.Owner.View : null;
            if (view == null || game.Motion == null) return o + RefSeat * s;
            var p = Project(view, game.Motion.Centre(game.Motion.Selected));
            return float.IsNaN(p.x) ? o + RefSeat * s : p;
        }

        private Vector2 Project(Camera view, Vector3 world)
        {
            var p = view.WorldToScreenPoint(world);
            return new Vector2(p.x / view.pixelWidth * W, (1 - p.y / view.pixelHeight) * H);
        }

        private Vector2 Shake(float amount, float t)
            => amount <= 0 ? Vector2.zero : new Vector2(Mathf.Sin(t * 61) + Mathf.Sin(t * 37) * .5f, Mathf.Sin(t * 53 + 1) + Mathf.Sin(t * 29) * .5f) * amount * s0;

        private static float Seg(float t, float a, float b) => Mathf.Clamp01((t - a) / (b - a));
        private static float Smooth(float x) => x * x * (3 - 2 * x);
        private static float EaseOut(float x) { x = 1 - x; return 1 - x * x * x; }
        private static float EaseIn(float x) => x * x * x;
        private static float EaseOutBack(float x) { const float c = 1.9f; x -= 1; return x <= -1 ? 0 : 1 + (c + 1) * x * x * x + c * x * x; }
        /// <summary>A landing squash that rings out like liquid: 1 at contact, decaying.</summary>
        private static float Wobble(float since) => since < 0 ? 0 : Mathf.Cos(since * 22) * Mathf.Exp(-since * 6);

        // ---- art ----------------------------------------------------------------------------------------------------------
        private Art Get(string name)
        {
            if (art.TryGetValue(name, out var a)) return a.Tex != null ? a : null;
            a = new Art { Tex = Resources.Load<Texture2D>(Folder + name) };
            if (Canvases.TryGetValue(name, out var canvas)) a.Canvas = canvas;
            if (layout != null && layout.TryGetValue(name, out var f))
            {
                a.Trim = new Rect(F(f[1]), F(f[2]), F(f[3]), F(f[4]));
                if (f.Length >= 9) a.Coghe = new Vector4(F(f[5]), F(f[6]), F(f[7]), F(f[8]));
            }
            art[name] = a;
            return a.Tex != null ? a : null;
        }

        private void LoadLayout()
        {
            layout = new Dictionary<string, string[]>();
            var text = Resources.Load<TextAsset>(Folder + "layout");
            if (text == null) return;
            foreach (var line in text.text.Split('\n'))
            {
                var f = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (f.Length >= 5 && !f[0].StartsWith("#")) layout[f[0]] = f;
            }
            Resources.UnloadAsset(text);
        }

        private static float F(string s) => float.Parse(s, CultureInfo.InvariantCulture);

        private static float[,] Grid(System.Random rnd, int n)
        {
            var g = new float[n, n]; for (int y = 0; y < n; y++) for (int x = 0; x < n; x++) g[x, y] = (float)rnd.NextDouble();
            return g;
        }

        private static float Octave(float[,] g, int n, float fx, float fy)
        {
            int ix = (int)fx, iy = (int)fy; float ux = Smooth(fx - ix), uy = Smooth(fy - iy);
            return Mathf.Lerp(Mathf.Lerp(g[ix % n, iy % n], g[(ix + 1) % n, iy % n], ux), Mathf.Lerp(g[ix % n, (iy + 1) % n], g[(ix + 1) % n, (iy + 1) % n], ux), uy);
        }

        private Texture2D NewTexture(int w, int h, TextureWrapMode wrap = TextureWrapMode.Clamp)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = wrap, filterMode = FilterMode.Bilinear };
            generated.Add(t); return t;
        }

        private void MakeTextures()
        {
            var rnd = new System.Random(7);
            white = NewTexture(4, 4); var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255); white.SetPixels32(px); white.Apply();
            // smooth value noise for the ink dissolve (two octaves, tiles): soft blotches, no grain on their edges
            noise = NewTexture(128, 128, TextureWrapMode.Repeat);
            var coarse = Grid(rnd, 8); var fine = Grid(rnd, 16);
            var np = new Color32[128 * 128];
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
            {
                float v = .68f * Octave(coarse, 8, x / 16f, y / 16f) + .32f * Octave(fine, 16, x / 8f, y / 8f);
                byte b = (byte)(Mathf.Clamp01(v) * 255); np[y * 128 + x] = new Color32(b, b, b, 255);
            }
            noise.SetPixels32(np); noise.Apply();
            // speed lines along the meteor's fall, and the radial burst of the impact frame
            streaks = NewTexture(256, 512); var sp = new Color32[256 * 512];
            for (int i = 0; i < 70; i++)
            {
                float x0 = (float)rnd.NextDouble() * 400 - 70, y0 = (float)rnd.NextDouble() * 512, len = 60 + (float)rnd.NextDouble() * 180;
                for (int k = 0; k < len; k++) { int x = (int)(x0 + k * .5f), y = (int)(y0 - k * .48f); if (x >= 0 && x < 256 && y >= 0 && y < 512) sp[y * 256 + x] = new Color32(255, 240, 210, (byte)(190 * Mathf.Sin(k / len * Mathf.PI))); }
            }
            streaks.SetPixels32(sp); streaks.Apply();
            burst = NewTexture(256, 512); var bp = new Color32[256 * 512];
            for (int y = 0; y < 512; y++) for (int x = 0; x < 256; x++)
            {
                float dx = (x - 128) / 128f, dy = (y - 256) / 256f * 2, a = Mathf.Atan2(dy, dx), r = Mathf.Sqrt(dx * dx + dy * dy);
                float ray = Mathf.Repeat(a * 24 / (2 * Mathf.PI) + Mathf.Sin(a * 7) * .3f, 1f) < .16f ? 1 : 0;
                bp[y * 256 + x] = new Color32(255, 250, 235, (byte)(255 * ray * Mathf.Clamp01((r - .45f) * 2)));
            }
            burst.SetPixels32(bp); burst.Apply();
            // a four-point glint
            star = NewTexture(64, 64); var st = new Color32[64 * 64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float dx = Mathf.Abs(x - 31.5f) / 32, dy = Mathf.Abs(y - 31.5f) / 32;
                float a = Mathf.Exp(-dx * 16) * Mathf.Exp(-dy * 2.5f) + Mathf.Exp(-dy * 16) * Mathf.Exp(-dx * 2.5f) + Mathf.Exp(-(dx * dx + dy * dy) * 60);
                st[y * 64 + x] = new Color32(255, 255, 255, (byte)(255 * Mathf.Clamp01(a)));
            }
            star.SetPixels32(st); star.Apply();
            // COghe's eyes: an ellipse (white, dark rim), a pupil with its shine, a smiling arc (white stroke, dark rim)
            var ink = new Color(.07f, .08f, .1f, 1);
            eyeWhite = Shape(96, 120, (x, y) => { float r = Mathf.Sqrt(x * x + y * y); return (Edge(r, 1), Color.Lerp(ink, Color.white, Edge(r, .76f))); });
            pupil = Shape(64, 64, (x, y) => { float r = Mathf.Sqrt(x * x + y * y), g = Mathf.Sqrt((x + .32f) * (x + .32f) + (y - .34f) * (y - .34f)); return (Edge(r, 1), Color.Lerp(ink, Color.white, Edge(g, .3f))); });
            smile = Shape(128, 64, (x, y) =>
            {
                // the upper half of a ring (y from -1 at the bottom of the texture), with round ends
                float yy = (y + 1) * .5f * 1.15f - .12f, r = Mathf.Sqrt(x * x / (.8f * .8f) + yy * yy / (.95f * .95f));
                float d = yy >= 0 ? Mathf.Abs(r - 1) * .8f : Mathf.Min(Vector2.Distance(new Vector2(x, yy), new Vector2(-.8f, 0)), Vector2.Distance(new Vector2(x, yy), new Vector2(.8f, 0)));
                return (Edge(d, .2f), Color.Lerp(ink, Color.white, Edge(d, .11f)));
            });
        }
        private static float Edge(float distance, float radius) => Mathf.Clamp01((radius - distance) * 24 + .5f);
        /// <summary>A texture from a shape function over -1..1 (y up): coverage and colour per pixel.</summary>
        private Texture2D Shape(int w, int h, Func<float, float, (float cover, Color colour)> shape)
        {
            var t = NewTexture(w, h); var px = new Color32[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {
                var (cover, colour) = shape((x + .5f) / w * 2 - 1, (y + .5f) / h * 2 - 1);
                colour.a = cover; px[y * w + x] = colour;
            }
            t.SetPixels32(px); t.Apply();
            return t;
        }
    }
}
