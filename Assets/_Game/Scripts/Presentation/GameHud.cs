using System;
using System.Collections;
using GravityBox.Foundation;
using GravityBox.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GravityBox.Presentation
{
    public sealed class GameHud : MonoBehaviour
    {
        private static readonly Color Ink = new Color(0.90f, 0.94f, 0.95f);
        private static readonly Color Muted = new Color(0.46f, 0.57f, 0.62f);
        private static readonly Color Surface = new Color(0.072f, 0.105f, 0.13f);
        private static readonly Color Accent = new Color(0.70f, 0.96f, 0.52f);
        private Font font;
        private LevelManager levels;
        private RectTransform safe;
        private Rect lastSafe;
        private Text title, number, environment, hint, state, progress, stats, pauseLabel, debugText;
        private GameObject levelModal, pauseOverlay, debugPanel;
        private float nextRefresh;
        public bool ModalOpen => levelModal != null && levelModal.activeSelf;

        public void Initialize(LevelManager manager)
        {
            levels = manager;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Build();
            levels.Loaded += OnLoaded;
            levels.Session.Changed += OnStateChanged;
            OnLoaded(levels.Definition);
            OnStateChanged(levels.Session.State);
        }

        private void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            // Keep the authored 1080-unit width on taller phones; extra height becomes
            // breathing room between the top and bottom anchors, never horizontal clipping.
            scaler.matchWidthOrHeight = 0f;
            gameObject.AddComponent<GraphicRaycaster>();
            safe = Rect("Safe area", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            if (EventSystem.current == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                es.transform.SetParent(transform, false);
            }

            Label("Brand", safe, "G R A V I T Y  /  B O X", 30, Ink, 60, 52, 760, 45);
            Label("Edition", safe, "STEEL BALL   /   PHYSICS STUDY", 19, Muted, 60, 104, 760, 30);
            Label("Prototype", safe, "PROTOTYPE", 19, Muted, -275, 57, 215, 34, true, TextAnchor.MiddleRight);
            Line("Header rule", safe, 60, 155, -60, new Color(0.2f, 0.29f, 0.33f));
            number = Label("Level number", safe, "01", 64, Muted, 58, 191, 130, 82);
            title = Label("Container name", safe, "Circle", 57, Ink, 204, 191, 804, 82);
            environment = Label("Ball specification", safe, "STEEL · 111 g · Ø 30 mm", 22, Muted, 62, 296, 650, 38);
            Label("Environment", safe, "EARTH GRAVITY", 20, Muted, -365, 296, 305, 38, true, TextAnchor.MiddleRight);
            state = Label("Status", safe, "ROLL THROUGH THE GREEN OPENING.", 21, Muted, 62, 354, 945, 40);

            var bottom = Rect("Controls", safe, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 346), new Vector2(0, 346));
            bottom.pivot = new Vector2(0.5f, 1);
            progress = Label("Progress", bottom, "01 / 03", 23, Muted, 60, 0, 300, 36);
            stats = Label("Speed", bottom, "0.00 m/s", 23, Ink, -340, 0, 280, 36, true, TextAnchor.MiddleRight);
            Line("Controls rule", bottom, 60, 49, -60, new Color(0.15f, 0.22f, 0.26f));
            hint = Label("Teaching hint", bottom, "Tilt gently. Watch the ball gather speed.", 27, Ink, 60, 67, 960, 80);
            Button("Reset", bottom, "RESET", 60, 167, 290, 100, Accent, new Color(0.05f, 0.10f, 0.10f), () => levels.ResetLevel());
            Button("Next experiment", bottom, "NEXT BOX", 372, 167, 290, 100, Surface, Ink, () => levels.Load((levels.Index + 1) % levels.Catalog.Levels.Length));
            Button("Experiments", bottom, "SHAPES", 684, 167, 210, 100, Surface, Ink, ToggleLevels);
            pauseLabel = Button("Pause", bottom, "II", 916, 167, 104, 100, Surface, Ink, () => levels.TogglePause());
            Label("Input hint", bottom, "DRAG TO TILT     /     RELEASE TO OBSERVE", 19, Muted, 60, 295, 960, 34, false, TextAnchor.MiddleCenter);

            BuildLevelModal();
            pauseOverlay = Overlay("Pause overlay", "TAKE A BREATH", "The experiment will wait.", "RESUME", () => levels.TogglePause());
            BuildDiagnostics();
            ApplySafeArea();
        }

        private void BuildLevelModal()
        {
            float height = 306 + levels.Catalog.Levels.Length * 178;
            levelModal = Panel("Experiment selector", safe, new Color(0.037f, 0.065f, 0.086f, 0.99f), 38, 407, 1004, height).gameObject;
            levelModal.GetComponent<Image>().raycastTarget = true;
            Label("Selector title", levelModal.transform, "Choose a box", 43, Ink, 40, 36, 860, 80);
            Label("Selector caption", levelModal.transform, "Same steel ball. Three box shapes.", 24, Muted, 42, 122, 900, 46);
            for (int i = 0; i < levels.Catalog.Levels.Length; i++)
            {
                int index = i;
                LevelDefinition definition = levels.Catalog.Levels[i];
                Button("Select " + definition.Id, levelModal.transform, $"{i + 1:00}   /   {definition.DisplayName}", 42, 196 + i * 178,
                    920, 142, Surface, Ink, () => { levelModal.SetActive(false); levels.Load(index); }, 34);
            }
            Button("Close selector", levelModal.transform, "BACK TO BOX", 42, height - 112, 920, 72, Accent, Surface, ToggleLevels);
            levelModal.SetActive(false);
        }

        private GameObject Overlay(string name, string heading, string caption, string action, Action callback)
        {
            RectTransform panel = Panel(name, safe, new Color(0.045f, 0.075f, 0.09f, 0.98f), 60, 655, 960, 490);
            panel.GetComponent<Image>().raycastTarget = true;
            Label("Heading", panel, heading, 41, Ink, 40, 70, 880, 80, false, TextAnchor.MiddleCenter);
            Label("Caption", panel, caption, 26, Muted, 40, 170, 880, 60, false, TextAnchor.MiddleCenter);
            Button("Action", panel, action, 120, 294, 720, 104, Accent, Surface, callback);
            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        private void BuildDiagnostics()
        {
            debugPanel = Panel("Development diagnostics", safe, new Color(0.025f, 0.04f, 0.055f, 0.97f), 60, 506, 960, 378).gameObject;
            debugPanel.GetComponent<Image>().raycastTarget = true;
            debugText = Label("Physics readings", debugPanel.transform, "", 23, Ink, 24, 15, 912, 250);
            Button("Previous", debugPanel.transform, "PREV", 24, 280, 200, 65, Surface, Ink, () => levels.Load(Mathf.Max(0, levels.Index - 1)), 21);
            Button("Next", debugPanel.transform, "NEXT", 252, 280, 200, 65, Surface, Ink, () => levels.Next(), 21);
            Button("Slow", debugPanel.transform, "0.25x / 1x", 480, 280, 210, 65, Surface, Ink, () => { if (levels.Session.State == SessionState.Active) Time.timeScale = Time.timeScale < 1 ? 1 : 0.25f; }, 21);
            Button("Step", debugPanel.transform, "STEP", 717, 280, 219, 65, Surface, Ink, () => { if (levels.Session.State == SessionState.Paused) StartCoroutine(StepOnce()); }, 21);
            debugPanel.SetActive(false);
        }

        private IEnumerator StepOnce()
        {
            Time.timeScale = 1;
            yield return new WaitForFixedUpdate();
            if (levels.Session.State == SessionState.Paused) Time.timeScale = 0;
        }

        public void ToggleDiagnostics() { if (Debug.isDebugBuild) debugPanel.SetActive(!debugPanel.activeSelf); }
        // Retained for the input adapter; this study needs no animated gesture cue.
        public void NotifyDrag() { }
        public bool BlocksRotation(Vector2 position)
        {
            float normalized = (position.y - Screen.safeArea.yMin) / Mathf.Max(1, Screen.safeArea.height);
            return ModalOpen || normalized < 0.19f || normalized > 0.80f || (debugPanel.activeSelf && normalized > 0.52f);
        }

        private void ToggleLevels()
        {
            bool open = !levelModal.activeSelf;
            levelModal.SetActive(open);
            if (open && levels.Session.State == SessionState.Active) levels.TogglePause();
            else if (!open && levels.Session.State == SessionState.Paused) levels.TogglePause();
            pauseOverlay.SetActive(false);
        }

        private void OnLoaded(LevelDefinition definition)
        {
            number.text = definition.DisplayIndex.ToString("00");
            title.text = definition.DisplayName;
            environment.text = $"STEEL · {levels.Ball.Body.mass * 1000:0} g · Ø {levels.Ball.Profile.Radius * 2000:0} mm";
            hint.text = definition.TeachingHint;
            progress.text = $"{definition.DisplayIndex:00} / {levels.Catalog.Levels.Length:00}";
        }

        private void OnStateChanged(SessionState session)
        {
            state.text = session == SessionState.Completing ? "BALL OUTSIDE. RESET OR CHOOSE THE NEXT BOX."
                : session == SessionState.Failed ? "TRY A DIFFERENT ANGLE. RESETTING…"
                : session == SessionState.Paused ? "SIMULATION PAUSED"
                : session == SessionState.Finished ? "CHOOSE A BOX TO CONTINUE."
                : "ROLL THROUGH THE GREEN OPENING.";
            state.color = session == SessionState.Completing ? Accent : Muted;
            pauseOverlay.SetActive(session == SessionState.Paused && !ModalOpen);
            pauseLabel.text = session == SessionState.Paused ? ">" : "II";
        }

        private void Update()
        {
            if (levels == null) return;
            if (Screen.safeArea != lastSafe) ApplySafeArea();
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.25f;
            if (levels.Ball != null) stats.text = $"{levels.Ball.Body.linearVelocity.magnitude:0.00} m/s";
            if (debugPanel.activeSelf && levels.Ball != null)
            {
                Rigidbody rb = levels.Ball.Body;
                debugText.text = $"{levels.Definition.Id}  ·  {levels.Session.State}  ·  {levels.Definition.Environment.Id}\n"
                    + $"v {rb.linearVelocity:F2}   speed {rb.linearVelocity.magnitude:F2} m/s\n"
                    + $"ω {rb.angularVelocity:F2}   sleeping {rb.IsSleeping()}\n"
                    + $"root {levels.Current.Rotation.Orientation.eulerAngles:F1}\n"
                    + $"fixed {Time.fixedDeltaTime:F4}s  ·  {Time.timeScale:F2}x  ·  resets {levels.ResetCount}\n"
                    + $"contact {levels.Ball.ContactLoad:F2} N · slip {levels.Ball.ContactSlipSpeed:F3} m/s · drags {levels.DragCount}";
                Debug.DrawRay(levels.Ball.transform.position,
                    levels.Definition.Environment.Acceleration.normalized * levels.Current.BoundsHalfExtent * 0.5f, Color.green, 0.26f);
            }
        }

        private void ApplySafeArea()
        {
            lastSafe = Screen.safeArea;
            safe.anchorMin = new Vector2(lastSafe.xMin / Screen.width, lastSafe.yMin / Screen.height);
            safe.anchorMax = new Vector2(lastSafe.xMax / Screen.width, lastSafe.yMax / Screen.height);
            safe.offsetMin = safe.offsetMax = Vector2.zero;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, false);
            r.anchorMin = min; r.anchorMax = max;
            r.anchoredPosition = position; r.sizeDelta = size;
            return r;
        }

        private RectTransform Panel(string name, Transform parent, Color color, float x, float y, float w, float h, bool right = false)
        {
            var r = Rect(name, parent, new Vector2(right ? 1 : 0, 1), new Vector2(right ? 1 : 0, 1), new Vector2(x, -y), new Vector2(w, h));
            r.pivot = new Vector2(0, 1);
            Image img = r.gameObject.AddComponent<Image>(); img.color = color; img.raycastTarget = false;
            return r;
        }

        private Text Label(string name, Transform parent, string value, int size, Color color, float x, float y, float w, float h, bool right = false, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var r = Rect(name, parent, new Vector2(right ? 1 : 0, 1), new Vector2(right ? 1 : 0, 1), new Vector2(x, -y), new Vector2(w, h));
            r.pivot = new Vector2(0, 1);
            Text text = r.gameObject.AddComponent<Text>();
            text.font = font; text.text = value; text.fontSize = size; text.color = color;
            text.alignment = alignment; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private Text Button(string name, Transform parent, string value, float x, float y, float w, float h, Color color, Color textColor, Action click, int size = 26)
        {
            RectTransform r = Panel(name, parent, color, x, y, w, h);
            r.GetComponent<Image>().raycastTarget = true;
            var button = r.gameObject.AddComponent<Button>();
            button.targetGraphic = r.GetComponent<Image>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.88f, 0.98f, 0.96f);
            colors.pressedColor = new Color(0.65f, 0.8f, 0.78f);
            button.colors = colors;
            button.onClick.AddListener(() => click());
            return Label(name + " label", r, value, size, textColor, 12, 8, w - 24, h - 16, false, TextAnchor.MiddleCenter);
        }

        private void Line(string name, Transform parent, float left, float top, float right, Color color)
        {
            RectTransform r = Panel(name, parent, color, left, top, 0, 1);
            r.anchorMax = new Vector2(1, 1);
            r.sizeDelta = new Vector2(right - left, 1);
        }

        private void OnDestroy()
        {
            if (levels == null) return;
            levels.Loaded -= OnLoaded;
            levels.Session.Changed -= OnStateChanged;
        }
    }
}
