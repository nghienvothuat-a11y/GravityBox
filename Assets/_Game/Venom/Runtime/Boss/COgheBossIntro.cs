using System;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// Boss levels show the existing warning at the normal gameplay angle. No opening camera tour or zoom.
    /// The level waits paused underneath the short banner; presentation never changes the puzzle state.
    /// </summary>
    public sealed class COgheBossIntro : MonoBehaviour
    {
        public static COgheBossIntro Current { get; private set; }
        public float Speed = 1;
        /// <summary>Previews: frame for this target size instead of the screen, and draw the banner through the camera.</summary>
        public static Vector2Int? PreviewSize;
        public float Clock { get; private set; }
        public bool Finished { get; private set; }
        public const float Warning = 2.7f;

        private VenomCampaign game;
        private Action done;
        private new Camera camera;
        private bool pausedByUs, started;
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
            // Establish the authored gameplay view once; the campaign rig keeps ownership throughout.
            game.CameraRig.Reset();
            int w = PreviewSize?.x ?? Screen.width, h = PreviewSize?.y ?? Screen.height;
            game.CameraRig.Frame(w, h, 0, true, PreviewSize.HasValue ? new Rect(0, 0, w, h) : Screen.safeArea);
            BuildBanner();
            started = true;
        }

        private void Update()
        {
            if (!started || Finished) return;
            if (game == null || game.Owner == null) { Finish(); return; }
            Clock += Mathf.Min(Time.unscaledDeltaTime, 1f / 20) * Speed;
            Banner(Mathf.Clamp01(Clock / Warning));
            if (Clock >= Warning) Finish();
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
            Finished = true;
            if (pausedByUs && game != null && game.Owner != null && game.Owner.Paused) game.Owner.TogglePause();
            if (Current == this) Current = null;
            var callback = done; done = null; callback?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (!Finished && pausedByUs && game != null && game.Owner != null && game.Owner.Paused) game.Owner.TogglePause();
            if (Current == this) Current = null;
            if (stripeTexture != null) Destroy(stripeTexture);
            if (vignetteTexture != null) Destroy(vignetteTexture);
            if (vignette != null && vignette.sprite != null) Destroy(vignette.sprite);
        }

        /// <summary>Previews and tests: jump to a moment of the intro.</summary>
        public void Seek(float seconds) { Clock = seconds; Update(); }
    }
}
