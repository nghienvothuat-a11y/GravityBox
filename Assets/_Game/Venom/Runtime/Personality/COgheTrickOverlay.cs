using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// What some Emotion tricks put on the screen itself (ability cards, Mrk 16/10/2026): the camera flash of a selfie, and
    /// the fog COghe breathes onto the glass with the heart it draws in it. An overlay canvas above everything; no
    /// raycasts, nothing in the world, and everything fades away by itself.
    /// </summary>
    public sealed class COgheTrickOverlay : MonoBehaviour
    {
        public const float FlashSeconds = .3f, FogFade = 2.6f, FogAlpha = .6f;
        private RectTransform layer;
        private Image flash, fog, heart;
        private float flashAt = -10, breathAt = -10, fogAmount, heartAlpha;
        private static Sprite softDisc, heartLine;

        /// <summary>How thick the fog is and how much of the heart is drawn and showing (tests).</summary>
        public float Fog => fog != null && fog.enabled ? fog.color.a : 0;
        public float Heart => heart != null && heart.enabled ? heart.fillAmount : 0;
        public bool Flashing => flash != null && flash.enabled;

        public static COgheTrickOverlay Create(Transform owner)
        {
            var go = new GameObject("COghe trick overlay", typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(owner, false);
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 400;
            var overlay = go.AddComponent<COgheTrickOverlay>(); overlay.layer = (RectTransform)go.transform;
            overlay.flash = overlay.Make("Flash", null); var r = overlay.flash.rectTransform;
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            overlay.fog = overlay.Make("Breath fog", SoftDisc());
            overlay.heart = overlay.Make("Heart in the fog", HeartLine());
            overlay.heart.type = Image.Type.Filled; overlay.heart.fillMethod = Image.FillMethod.Radial360;
            overlay.heart.fillOrigin = (int)Image.Origin360.Top; overlay.heart.fillClockwise = true;
            return overlay;
        }

        private Image Make(string name, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var image = go.GetComponent<Image>(); ((RectTransform)go.transform).SetParent(layer, false);
            image.sprite = sprite; image.raycastTarget = false; image.enabled = false;
            var rect = image.rectTransform; rect.anchorMin = rect.anchorMax = Vector2.zero; rect.pivot = new Vector2(.5f, .5f);
            return image;
        }

        /// <summary>The camera's flash: a white blink over the whole screen.</summary>
        public void Flash() => flashAt = UnityEngine.Time.unscaledTime;

        /// <summary>This frame's breath on the glass at <paramref name="at"/> (screen pixels): a fog patch of
        /// <paramref name="radius"/> (0..1 grown) and the heart drawn in it so far (0..1).</summary>
        public void Breath(Vector2 at, float radius, float amount, float drawn)
        {
            breathAt = UnityEngine.Time.unscaledTime; fogAmount = amount; heartAlpha = drawn > 0 ? 1 : 0;
            float scale = layer.lossyScale.x > 0 ? 1 / layer.lossyScale.x : 1;
            Place(fog, at * scale, Vector2.one * radius * 2 * scale);
            Place(heart, at * scale, Vector2.one * radius * 1.15f * scale);
            heart.fillAmount = Mathf.Clamp01(drawn);
        }

        private static void Place(Image image, Vector2 at, Vector2 size)
        { image.rectTransform.anchoredPosition = at; image.rectTransform.sizeDelta = size; }

        private void Update()
        {
            float now = UnityEngine.Time.unscaledTime;
            float f = 1 - Mathf.Clamp01((now - flashAt) / FlashSeconds);
            flash.enabled = f > 0; flash.color = new Color(1, 1, 1, .7f * f * f);
            // once COghe stops breathing on it, the fog clears and the heart goes with it
            float clear = 1 - Mathf.Clamp01((now - breathAt - .1f) / FogFade);
            float fogA = FogAlpha * fogAmount * clear, heartA = heartAlpha * Mathf.Clamp01(clear * 1.4f);
            fog.enabled = fogA > .003f; fog.color = new Color(.95f, .97f, 1, fogA);
            heart.enabled = heartA > .003f && heart.fillAmount > 0; heart.color = new Color(.36f, .42f, .5f, .75f * heartA);
        }

        /// <summary>Tests: let <paramref name="seconds"/> pass for the fading.</summary>
        public void AgeForTests(float seconds) { flashAt -= seconds; breathAt -= seconds; Update(); }

        // A misty disc, soft all the way to its edge.
        private static Sprite SoftDisc()
        {
            if (softDisc != null) return softDisc;
            const int n = 64; var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                float a = Mathf.SmoothStep(1, 0, Mathf.InverseLerp(.35f, 1, d));
                px[y * n + x] = new Color32(255, 255, 255, (byte)(255 * a));
            }
            tex.SetPixels32(px); tex.Apply(false, true);
            return softDisc = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 100);
        }

        // A heart traced as a soft line, the way a fingertip draws it in fog.
        private static Sprite HeartLine()
        {
            if (heartLine != null) return heartLine;
            const int n = 128; var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var curve = new Vector2[240];
            for (int k = 0; k < curve.Length; k++)
            {
                float a = k / (float)curve.Length * Mathf.PI * 2, sn = Mathf.Sin(a);
                var h = new Vector2(16 * sn * sn * sn, 13 * Mathf.Cos(a) - 5 * Mathf.Cos(2 * a) - 2 * Mathf.Cos(3 * a) - Mathf.Cos(4 * a)) / 17f;
                curve[k] = new Vector2(.5f + .44f * h.x, .55f + .44f * h.y) * n;
            }
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                var p = new Vector2(x + .5f, y + .5f); float best = float.MaxValue;
                foreach (var c in curve) best = Mathf.Min(best, (c - p).sqrMagnitude);
                float a = Mathf.SmoothStep(1, 0, Mathf.InverseLerp(2.2f, 4.5f, Mathf.Sqrt(best)));
                px[y * n + x] = new Color32(255, 255, 255, (byte)(255 * a));
            }
            tex.SetPixels32(px); tex.Apply(false, true);
            return heartLine = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 100);
        }
    }
}
