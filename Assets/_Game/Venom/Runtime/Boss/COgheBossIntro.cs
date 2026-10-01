using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// Boss levels open with a short tour (Mrk): the camera glides into the glass box, close past its mechanisms like a
    /// walk through a room, ends at the exit, pulls back into the game's own view (a dolly-zoom from perspective into the
    /// orthographic framing, so the hand-over is seamless), then a hazard banner sweeps across: "WARNING · VERY HARD LEVEL".
    /// The level waits paused underneath; a tap skips to the banner. Presentation only.
    /// </summary>
    public sealed class COgheBossIntro : MonoBehaviour
    {
        public static COgheBossIntro Current { get; private set; }
        /// <summary>While true the campaign camera rig leaves the view alone.</summary>
        public static bool OwnsCamera => Current != null && Current.camera != null && !Current.handedBack;
        public float Speed = 1;
        /// <summary>Previews: frame for this target size instead of the screen, and draw the banner through the camera.</summary>
        public static Vector2Int? PreviewSize;
        public float Clock { get; private set; }
        public bool Finished { get; private set; }
        public float TourLength { get; private set; }
        public const float DiveIn = 1.5f, PullBack = 1.6f, Warning = 2.7f;

        private VenomCampaign game;
        private Action done;
        private new Camera camera;
        private bool pausedByUs, handedBack, started;
        private readonly List<Vector3> eyes = new List<Vector3>(8), looks = new List<Vector3>(8);
        private Vector3 restPosition, restLook; private Quaternion restRotation; private float restSize, restNear, restFar;
        private float tourFov = 50;
        // banner
        private GameObject canvasGo;
        private RectTransform band, title, subtitle;
        private RawImage stripesTop, stripesBottom; private Image vignette, flash;
        private Texture2D stripeTexture, vignetteTexture;
        private bool sounded;

        public static COgheBossIntro Play(VenomCampaign game, Action onDone)
        {
            if (Current != null) return Current;
            var intro = new GameObject("COghe boss intro").AddComponent<COgheBossIntro>();
            intro.game = game; intro.done = onDone; Current = intro;
            return intro;
        }

        private void Start()
        {
            if (game == null || game.Owner == null) { Finish(); return; }
            camera = game.Owner.View;
            if (!game.Owner.Paused) { game.Owner.TogglePause(); pausedByUs = true; }
            // the game's own view, where the tour must end
            game.CameraRig.Reset();
            int w = PreviewSize?.x ?? Screen.width, h = PreviewSize?.y ?? Screen.height;
            game.CameraRig.Frame(w, h, 0, true, PreviewSize.HasValue ? new Rect(0, 0, w, h) : Screen.safeArea);
            restPosition = camera.transform.position; restRotation = camera.transform.rotation; restSize = camera.orthographicSize;
            restNear = camera.nearClipPlane; restFar = camera.farClipPlane;
            Vector3 boxCentre = game.Root.TransformPoint(game.CameraRig.OverviewBounds.center);
            restLook = restPosition + camera.transform.forward * Vector3.Dot(boxCentre - restPosition, camera.transform.forward);
            BuildTour(boxCentre);
            TourLength = DiveIn + 1.8f * Mathf.Max(1, eyes.Count - 1);
            camera.orthographic = false; camera.nearClipPlane = .01f; camera.farClipPlane = 30; camera.fieldOfView = tourFov;
            BuildBanner();
            started = true;
        }

        /// <summary>The camera's stops inside the box: close to up to four prominent mechanisms in a short walk, then the
        /// exit. The tour dives in from the game's own view and pulls back out to it.</summary>
        private void BuildTour(Vector3 boxCentre)
        {
            var bounds = game.CameraRig.OverviewBounds;   // the shell, in the box's own space
            Vector3 forward = camera.transform.forward, flatToCamera = Vector3.ProjectOnPlane(-forward, game.Root.up).normalized;
            var targets = new List<(Vector3 centre, float size)>();
            foreach (var mechanism in game.Mechanisms)
            {
                if (mechanism == null || !mechanism.isActiveAndEnabled) continue;
                var renderers = mechanism.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) continue;
                var b = renderers[0].bounds; foreach (var r in renderers) b.Encapsulate(r.bounds);
                float size = b.size.magnitude;
                if (size < .03f || size > 1.2f) continue;
                bool near = false;
                for (int i = 0; i < targets.Count; i++) if (Vector3.Distance(targets[i].centre, b.center) < .1f) { near = true; if (size > targets[i].size) targets[i] = (b.center, size); }
                if (!near) targets.Add((b.center, size));
            }
            targets.Sort((a, b) => b.size.CompareTo(a.size));
            if (targets.Count > 4) targets.RemoveRange(4, targets.Count - 4);
            var sizes = new List<(Vector3 centre, float size)>(targets);
            // a short walk: start from the creature, always the nearest next stop
            var ordered = new List<Vector3>(); Vector3 from = game.Motion.Centre(0);
            while (targets.Count > 0)
            {
                int best = 0; for (int i = 1; i < targets.Count; i++) if (Vector3.Distance(from, targets[i].centre) < Vector3.Distance(from, targets[best].centre)) best = i;
                ordered.Add(targets[best].centre); from = targets[best].centre; targets.RemoveAt(best);
            }
            ordered.Add(game.Owner.Outlet.position);
            eyes.Clear(); looks.Clear();
            foreach (var target in ordered)
            {
                Vector3 toCentre = Vector3.ProjectOnPlane(boxCentre - target, game.Root.up);
                Vector3 back = (toCentre.normalized + flatToCamera).normalized;
                if (back.sqrMagnitude < .1f) back = flatToCamera;
                float size = target == game.Owner.Outlet.position ? .3f : .05f;   // the exit: step back so its cover fits
                foreach (var t in sizes) if (Vector3.Distance(t.centre, target) < .001f) size = t.size;
                Vector3 eye = target + back * Mathf.Clamp(.14f + size * .55f, .17f, .32f) + game.Root.up * .09f;
                // stay inside the glass
                Vector3 local = game.Root.InverseTransformPoint(eye), min = bounds.min + Vector3.one * .03f, max = bounds.max - Vector3.one * .03f;
                local = new Vector3(Mathf.Clamp(local.x, min.x, max.x), Mathf.Clamp(local.y, min.y + .04f, max.y), Mathf.Clamp(local.z, min.z, max.z));
                eyes.Add(game.Root.TransformPoint(local)); looks.Add(target);
            }
        }

        private void Update()
        {
            if (!started || Finished) return;
            if (game == null || game.Owner == null) { Finish(); return; }
            float dt = Mathf.Min(Time.unscaledDeltaTime, 1f / 20) * Speed;
            bool tap = (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
            if (tap && Clock < TourLength + PullBack) { Clock = TourLength + PullBack; HandBack(); }
            Clock += dt;
            if (Clock < DiveIn) Between(eyes[0], looks[0], 1 - Mathf.SmoothStep(0, 1, Clock / DiveIn), true);
            else if (Clock < TourLength) Tour((Clock - DiveIn) / (TourLength - DiveIn));
            else if (Clock < TourLength + PullBack) Between(eyes[eyes.Count - 1], looks[looks.Count - 1], Mathf.SmoothStep(0, 1, (Clock - TourLength) / PullBack), false);
            else
            {
                HandBack();
                Banner((Clock - TourLength - PullBack) / Warning);
                if (Clock >= TourLength + PullBack + Warning) Finish();
            }
        }

        private void Tour(float u)
        {
            if (eyes.Count == 1) { camera.transform.SetPositionAndRotation(eyes[0], Quaternion.LookRotation(looks[0] - eyes[0], game.Root.up)); camera.fieldOfView = tourFov; return; }
            float s = Mathf.Clamp01(u) * (eyes.Count - 1);
            int i = Mathf.Min(Mathf.FloorToInt(s), eyes.Count - 2); float f = Mathf.SmoothStep(0, 1, s - i) * .35f + (s - i) * .65f;
            Vector3 eye = CatmullRom(eyes, i, f), look = CatmullRom(looks, i, f);
            camera.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(look - eye, game.Root.up));
            camera.fieldOfView = tourFov;
        }

        /// <summary>Between a stop (k = 0) and the game's view (k = 1): rotation and look point blend while the camera
        /// backs away and narrows its lens, until a 6 m, ~6° perspective matches the orthographic framing; at k = 1 the
        /// real orthographic camera takes over with no visible jump.</summary>
        private void Between(Vector3 stopEye, Vector3 stopLook, float k, bool diving)
        {
            if (diving && k >= .999f) { camera.orthographic = true; camera.transform.SetPositionAndRotation(restPosition, restRotation); camera.orthographicSize = restSize; return; }
            camera.orthographic = false;
            float stopDistance = Vector3.Distance(stopEye, stopLook), stopHalf = stopDistance * Mathf.Tan(tourFov * .5f * Mathf.Deg2Rad);
            Quaternion stopRotation = Quaternion.LookRotation(stopLook - stopEye, game.Root.up);
            Quaternion rotation = Quaternion.Slerp(stopRotation, restRotation, k);
            Vector3 look = Vector3.Lerp(stopLook, restLook, k);
            float distance = Mathf.Lerp(stopDistance, 6f, k * k), half = Mathf.Lerp(stopHalf, restSize, k);
            camera.fieldOfView = 2 * Mathf.Atan(half / distance) * Mathf.Rad2Deg;
            camera.transform.SetPositionAndRotation(look - rotation * Vector3.forward * distance, rotation);
        }

        private void HandBack()
        {
            if (handedBack || camera == null) return;
            handedBack = true;
            camera.orthographic = true; camera.orthographicSize = restSize; camera.nearClipPlane = restNear; camera.farClipPlane = restFar;
            camera.transform.SetPositionAndRotation(restPosition, restRotation);
        }

        private static Vector3 CatmullRom(List<Vector3> p, int i, float t)
        {
            Vector3 p0 = p[Mathf.Max(0, i - 1)], p1 = p[i], p2 = p[Mathf.Min(p.Count - 1, i + 1)], p3 = p[Mathf.Min(p.Count - 1, i + 2)];
            float t2 = t * t, t3 = t2 * t;
            return .5f * (2 * p1 + (p2 - p0) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + (3 * p1 - p0 - 3 * p2 + p3) * t3);
        }

        // The warning banner ---------------------------------------------------------------------------------------------------
        private void BuildBanner()
        {
            canvasGo = new GameObject("Boss warning", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 300;
            if (PreviewSize.HasValue) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = .5f; }
            var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(360, 640); scaler.matchWidthOrHeight = .5f;
            // hazard stripes and a red edge glow, generated once
            stripeTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
            var px = new Color32[32 * 32];
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++) px[y * 32 + x] = ((x + y) % 32) < 16 ? new Color32(236, 178, 52, 255) : new Color32(26, 30, 33, 255);
            stripeTexture.SetPixels32(px); stripeTexture.Apply();
            vignetteTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var vp = new Color32[64 * 64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float dx = (x - 31.5f) / 32, dy = (y - 31.5f) / 32, d = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy) * .9f);
                vp[y * 64 + x] = new Color32(205, 38, 34, (byte)(255 * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.55f, 1f, d))));
            }
            vignetteTexture.SetPixels32(vp); vignetteTexture.Apply();
            vignette = Full<Image>("Red edge"); vignette.sprite = Sprite.Create(vignetteTexture, new Rect(0, 0, 64, 64), Vector2.one * .5f); vignette.color = new Color(1, 1, 1, 0);
            flash = Full<Image>("Flash"); flash.color = new Color(.85f, .1f, .08f, 0);
            band = Rect("Band", canvasGo.transform, new Vector2(0, .5f), new Vector2(1, .5f), new Vector2(0, 118));
            var dark = band.gameObject.AddComponent<Image>(); dark.color = new Color(.07f, .08f, .09f, .88f); dark.raycastTarget = false;
            stripesTop = Stripes("Stripes top", new Vector2(0, 1), 18); stripesBottom = Stripes("Stripes bottom", new Vector2(0, 0), 18);
            var font = Resources.Load<Font>("COgheUI/ManropeBold") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title = Text("WARNING", font, 40, new Color(.96f, .27f, .2f), 12);
            subtitle = Text("VERY HARD LEVEL", font, 15, new Color(.98f, .95f, .88f), -26);
            canvasGo.SetActive(false);
        }

        private T Full<T>(string name) where T : Graphic
        {
            var r = Rect(name, canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero);
            var g = r.gameObject.AddComponent<T>(); g.raycastTarget = false; return g;
        }
        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 size)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
            r.anchorMin = min; r.anchorMax = max; r.sizeDelta = size; r.anchoredPosition = Vector2.zero; return r;
        }
        private RawImage Stripes(string name, Vector2 edge, float height)
        {
            var r = Rect(name, band, new Vector2(0, edge.y), new Vector2(1, edge.y), new Vector2(0, height));
            r.pivot = new Vector2(.5f, edge.y); r.anchoredPosition = Vector2.zero;
            var raw = r.gameObject.AddComponent<RawImage>(); raw.texture = stripeTexture; raw.raycastTarget = false; return raw;
        }
        private RectTransform Text(string value, Font font, int size, Color color, float y)
        {
            var r = Rect(value, band, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(420, size * 1.4f)); r.anchoredPosition = new Vector2(0, y);
            var t = r.gameObject.AddComponent<Text>(); t.font = font; t.fontSize = size; t.color = color; t.alignment = TextAnchor.MiddleCenter;
            t.text = string.Join(" ", value.ToCharArray()); t.raycastTarget = false; t.horizontalOverflow = HorizontalWrapMode.Overflow;
            var outline = r.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(0, 0, 0, .6f); outline.effectDistance = new Vector2(1.5f, -1.5f);
            return r;
        }

        private void Banner(float u)
        {
            if (canvasGo == null) return;
            if (!canvasGo.activeSelf) canvasGo.SetActive(true);
            if (!sounded) { sounded = true; COgheAudio.Instance?.Play("boss_warning", .7f, 0, .5f); }
            float width = ((RectTransform)canvasGo.transform).rect.width;
            // the band sweeps in from the right, the words race across it, then everything leaves to the left
            float enter = 1 - Mathf.Pow(1 - Mathf.Clamp01(u / .16f), 3), leave = Mathf.Pow(Mathf.Clamp01((u - .84f) / .16f), 2);
            band.anchoredPosition = new Vector2(width * (1 - enter) - width * leave, 0);
            band.localScale = new Vector3(1, Mathf.Lerp(.2f, 1, enter) * (1 - .6f * leave), 1);
            float race = Mathf.Lerp(width * .7f, -width * .7f, u);
            float hold = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.12f, .3f, u)) * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.7f, .88f, u)));
            title.anchoredPosition = new Vector2(Mathf.Lerp(race, 0, hold), 12);
            subtitle.anchoredPosition = new Vector2(Mathf.Lerp(-race * .8f, 0, hold), -26);
            float pulse = Mathf.Pow(Mathf.Max(0, Mathf.Sin(u * Mathf.PI * 6)), 3);
            title.localScale = Vector3.one * (1 + .07f * pulse * hold);
            stripesTop.uvRect = new Rect(-u * 6, 0, width / 18f, 18 / 18f); stripesBottom.uvRect = new Rect(u * 6, 0, width / 18f, 1);
            vignette.color = new Color(1, 1, 1, (.25f + .55f * pulse) * (1 - leave) * enter);
            flash.color = new Color(.85f, .1f, .08f, .22f * (1 - Mathf.Clamp01(u / .08f)));
        }

        private void Finish()
        {
            if (Finished) return;
            Finished = true; HandBack();
            if (pausedByUs && game != null && game.Owner != null && game.Owner.Paused) game.Owner.TogglePause();
            if (Current == this) Current = null;
            var callback = done; done = null; callback?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (!Finished) { HandBack(); if (pausedByUs && game != null && game.Owner != null && game.Owner.Paused) game.Owner.TogglePause(); }
            if (Current == this) Current = null;
            if (stripeTexture != null) Destroy(stripeTexture);
            if (vignetteTexture != null) Destroy(vignetteTexture);
            if (vignette != null && vignette.sprite != null) Destroy(vignette.sprite);
        }

        /// <summary>Previews and tests: jump to a moment of the intro.</summary>
        public void Seek(float seconds) { Clock = seconds; Update(); }
    }
}
