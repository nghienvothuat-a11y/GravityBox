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
        private Text title, number, environment, hint, state, progress, stats, dragHint, pauseLabel, debugText;
        private Image environmentDot, progressFill;
        private GameObject levelModal, pauseOverlay, debugPanel, finishOverlay;
        private RectTransform finger;
        private bool touched;
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
            Label("Edition", safe, "PHYSICS LAB   /   01", 19, Muted, 60, 104, 760, 30);
            Label("Prototype", safe, "PROTOTYPE", 19, Muted, -275, 57, 215, 34, true, TextAnchor.MiddleRight);
            Line("Header rule", safe, 60, 155, -60, new Color(0.2f, 0.29f, 0.33f));
            number = Label("Level number", safe, "01", 88, Ink, 55, 194, 180, 108);
            Label("Level label", safe, "EXPERIMENT", 20, Muted, 62, 307, 220, 30);
            var badge = Panel("Environment pill", safe, Surface, -390, 213, 330, 68, true);
            environmentDot = Panel("Signal", badge, Accent, 24, 26, 12, 12).GetComponent<Image>();
            environment = Label("Environment", badge, "GRAVITY", 24, Accent, 53, 12, 250, 43);
            title = Label("Puzzle name", safe, "First principles", 51, Ink, 60, 361, 940, 76);
            state = Label("Status", safe, "ROTATE THE WORLD. FIND THE EXIT.", 21, Muted, 62, 446, 945, 40);

            var bottom = Rect("Controls", safe, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 410), new Vector2(0, 410));
            bottom.pivot = new Vector2(0.5f, 1);
            progress = Label("Progress", bottom, "01 / 16", 23, Muted, 60, 0, 300, 36);
            stats = Label("Attempts", bottom, "0 RESETS", 21, Muted, -340, 0, 280, 36, true, TextAnchor.MiddleRight);
            var track = Panel("Progress track", bottom, new Color(0.15f, 0.22f, 0.26f), 60, 54, 960, 3);
            progressFill = Panel("Progress fill", track, Accent, 0, 0, 60, 3).GetComponent<Image>();
            hint = Label("Teaching hint", bottom, "Tilt the ramp. Let gravity do the rest.", 27, Ink, 60, 83, 930, 84);
            Button("Reset", bottom, "RESET", 60, 190, 440, 100, Accent, new Color(0.05f, 0.10f, 0.10f), () => levels.ResetLevel());
            Button("Experiments", bottom, "LEVELS", 522, 190, 254, 100, Surface, Ink, ToggleLevels);
            pauseLabel = Button("Pause", bottom, "II", 798, 190, 222, 100, Surface, Ink, () => levels.TogglePause());
            dragHint = Label("Input hint", bottom, "DRAG TO ROTATE     /     RELEASE TO OBSERVE", 19, Muted, 60, 324, 960, 34, false, TextAnchor.MiddleCenter);
            finger = Panel("Ghost finger", safe, new Color(0.86f, 0.97f, 0.87f, 0.4f), 0, 0, 20, 20);
            finger.anchorMin = finger.anchorMax = new Vector2(0.5f, 0.37f);
            finger.pivot = Vector2.one * 0.5f;

            BuildLevelModal();
            pauseOverlay = Overlay("Pause overlay", "TAKE A BREATH", "The experiment will wait.", "RESUME", () => levels.TogglePause());
            finishOverlay = Overlay("Finish overlay", "EXPERIMENTS COMPLETE", "16 small worlds. One simple rule.", "PLAY AGAIN", () => levels.Load(0));
            BuildDiagnostics();
            ApplySafeArea();
        }

        private void BuildLevelModal()
        {
            levelModal = Panel("Experiment selector", safe, new Color(0.037f, 0.065f, 0.086f, 0.99f), 38, 174, 1004, 1320).gameObject;
            levelModal.GetComponent<Image>().raycastTarget = true;
            Label("Selector title", levelModal.transform, "Choose an experiment", 43, Ink, 40, 40, 860, 80);
            Label("Chapter 1", levelModal.transform, "01—10   /   GRAVITY", 23, Accent, 42, 140, 860, 48);
            for (int i = 0; i < levels.Catalog.Levels.Length; i++)
            {
                int index = i;
                LevelDefinition definition = levels.Catalog.Levels[i];
                int row = i < 10 ? i / 3 : (i - 10) / 3;
                int col = i < 10 ? i % 3 : (i - 10) % 3;
                float top = i < 10 ? 207 + row * 142 : 847 + row * 142;
                Button("Select " + definition.Id, levelModal.transform, $"{i + 1:00}\n{definition.DisplayName}", 42 + col * 306, top,
                    286, 118, Surface, Ink, () => { levelModal.SetActive(false); levels.Load(index); }, 24);
            }
            Label("Chapter 2", levelModal.transform, "11—16   /   ZERO-G", 23, new Color(0.43f, 0.82f, 1), 42, 784, 860, 40);
            Button("Close selector", levelModal.transform, "BACK TO BOX", 42, 1138, 900, 92, Accent, Surface, ToggleLevels);
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
        public void NotifyDrag() { touched = true; finger.gameObject.SetActive(false); }
        public bool BlocksRotation(Vector2 position)
        {
            float normalized = (position.y - Screen.safeArea.yMin) / Mathf.Max(1, Screen.safeArea.height);
            return ModalOpen || normalized < 0.23f || normalized > 0.76f || (debugPanel.activeSelf && normalized > 0.52f);
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
            environment.text = definition.Environment.DisplayName;
            environment.color = environmentDot.color = definition.Environment.Accent;
            hint.text = definition.TeachingHint;
            progress.text = $"{definition.DisplayIndex:00} / {levels.Catalog.Levels.Length:00}";
            progressFill.color = definition.Environment.Accent;
            progressFill.rectTransform.sizeDelta = new Vector2(960f * definition.DisplayIndex / levels.Catalog.Levels.Length, 3);
            touched = !definition.Tutorial;
            finger.gameObject.SetActive(!touched);
            finishOverlay.SetActive(false);
        }

        private void OnStateChanged(SessionState session)
        {
            state.text = session == SessionState.Completing ? "LOCKED IN. BEAUTIFULLY DONE."
                : session == SessionState.Failed ? "TRY A DIFFERENT ANGLE. RESETTING…"
                : session == SessionState.Paused ? "SIMULATION PAUSED"
                : session == SessionState.Finished ? "ALL EXPERIMENTS COMPLETE"
                : "ROTATE THE WORLD. FIND THE EXIT.";
            state.color = session == SessionState.Completing ? Accent : Muted;
            pauseOverlay.SetActive(session == SessionState.Paused && !ModalOpen);
            finishOverlay.SetActive(session == SessionState.Finished);
            pauseLabel.text = session == SessionState.Paused ? ">" : "II";
        }

        private void Update()
        {
            if (levels == null) return;
            if (Screen.safeArea != lastSafe) ApplySafeArea();
            if (!touched && finger.gameObject.activeSelf)
            {
                float phase = Time.unscaledTime * 1.5f;
                finger.anchoredPosition = new Vector2(Mathf.Sin(phase) * 110, Mathf.Cos(phase) * 16);
            }
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.25f;
            stats.text = $"{levels.ResetCount} RESETS";
            if (debugPanel.activeSelf && levels.Ball != null)
            {
                Rigidbody rb = levels.Ball.Body;
                debugText.text = $"{levels.Definition.Id}  ·  {levels.Session.State}  ·  {levels.Definition.Environment.Id}\n"
                    + $"v {rb.linearVelocity:F2}   speed {rb.linearVelocity.magnitude:F2} m/s\n"
                    + $"ω {rb.angularVelocity:F2}   sleeping {rb.IsSleeping()}\n"
                    + $"root {levels.Current.Rotation.Orientation.eulerAngles:F1}\n"
                    + $"fixed {Time.fixedDeltaTime:F4}s  ·  {Time.timeScale:F2}x  ·  resets {levels.ResetCount}\n"
                    + $"pointer {UnityEngine.InputSystem.Mouse.current?.position.ReadValue()} / {Screen.width}x{Screen.height} · drags {levels.DragCount}";
                Debug.DrawRay(levels.Ball.transform.position, levels.Definition.Environment.Acceleration * 0.15f, Color.green, 0.26f);
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
