using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// Hearts that fly up from COghe when the player has understood it (the bonus "Hiểu ra", Mrk 07/10/2026): ~18 small filled
    /// hearts in the product's warm palette pop out of the big heart COghe makes, drift up swaying and fade. UI images
    /// animated in code like <see cref="COgheConfetti"/>; the heart sprite is drawn once in code (the icon atlas heart is
    /// an outline). Presentation only.
    /// </summary>
    public sealed class COgheHearts : MonoBehaviour
    {
        private int Count = 22;
        private const float Life = 2.6f;
        private static readonly Color[] Palette =
        {
            new Color(.86f, .36f, .3f), new Color(.737f, .467f, .439f), new Color(.93f, .55f, .52f), new Color(.875f, .729f, .439f),
        };
        private static Sprite sprite;
        private RectTransform[] hearts;
        private Image[] images;
        private Vector2[] position, velocity;
        private float[] size, delay, sway, swayRate;
        private float clock;
        private System.Random rnd;
        private float scale = 1;

        /// <summary>Hearts over <paramref name="parent"/> (a full-screen rect, logical units) from <paramref name="origin"/>
        /// (logical units from its lower-left corner).</summary>
        public static COgheHearts Burst(RectTransform parent, Vector2 origin, int seed, int count = 22, float scale = 1)
        {
            var go = new GameObject("Bonus hearts", typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var burst = go.AddComponent<COgheHearts>(); burst.Count = count; burst.scale = scale; burst.Build(origin, seed);
            return burst;
        }

        /// <summary>The filled heart (also on the bonus reward popup).</summary>
        public static Sprite Heart => HeartSprite();
        private static Sprite HeartSprite()
        {
            if (sprite != null) return sprite;
            const int n = 64; var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Heart" };
            var pixels = new Color32[n * n];
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                // the heart curve (x² + y² − 1)³ − x²y³ ≤ 0, softened over about a pixel at its edge
                float coverage = 0;
                for (int sy = 0; sy < 3; sy++) for (int sx = 0; sx < 3; sx++)
                {
                    float u = ((x + (sx + .5f) / 3) / n - .5f) * 2.6f, v = ((y + (sy + .5f) / 3) / n - .45f) * 2.6f;
                    float a = u * u + v * v - 1;
                    if (a * a * a - u * u * v * v * v <= 0) coverage += 1f / 9;
                }
                pixels[y * n + x] = new Color32(255, 255, 255, (byte)(coverage * 255));
            }
            tex.SetPixels32(pixels); tex.Apply(false, true);
            sprite = Sprite.Create(tex, new Rect(0, 0, n, n), Vector2.one * .5f, 100);
            return sprite;
        }

        private void Build(Vector2 origin, int seed)
        {
            rnd = new System.Random(seed); var heart = HeartSprite();
            hearts = new RectTransform[Count]; images = new Image[Count]; position = new Vector2[Count]; velocity = new Vector2[Count];
            size = new float[Count]; delay = new float[Count]; sway = new float[Count]; swayRate = new float[Count];
            for (int i = 0; i < Count; i++)
            {
                var go = new GameObject("Heart", typeof(RectTransform), typeof(Image));
                var r = go.GetComponent<RectTransform>(); r.SetParent(transform, false);
                r.anchorMin = r.anchorMax = Vector2.zero; r.pivot = Vector2.one * .5f;
                var im = go.GetComponent<Image>(); im.sprite = heart; im.raycastTarget = false; im.color = Palette[rnd.Next(Palette.Length)];
                hearts[i] = r; images[i] = im;
                position[i] = origin + new Vector2(R(-10, 10), R(-6, 6));
                float angle = R(20, 160) * Mathf.Deg2Rad, speed = R(150, 310) * scale;
                velocity[i] = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                size[i] = R(16, 34) * Mathf.Lerp(1, scale, .5f); delay[i] = i < 6 ? R(0, .08f) : R(.1f, .7f);
                sway[i] = R(0, 6.28f); swayRate[i] = R(2.4f, 4.2f);
                go.SetActive(false);
            }
        }
        private float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);

        private void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 1f / 20);   // game time, like the confetti (a pause freezes them)
            clock += dt;
            bool alive = false;
            for (int i = 0; i < Count; i++)
            {
                float age = clock - delay[i];
                if (age < 0) { alive = true; continue; }
                if (age > Life) { if (hearts[i].gameObject.activeSelf) hearts[i].gameObject.SetActive(false); continue; }
                alive = true;
                if (!hearts[i].gameObject.activeSelf) hearts[i].gameObject.SetActive(true);
                // out fast, then floating up slowly, swaying like a balloon
                velocity[i] *= Mathf.Exp(-2.2f * dt);
                velocity[i].y += 26 * dt;
                position[i] += velocity[i] * dt + new Vector2(Mathf.Sin(sway[i] + age * swayRate[i]) * 14 * dt, 0);
                float pop = age < .25f ? Mathf.Sin(age / .25f * Mathf.PI * .5f) * (1 + .3f * Mathf.Sin(age / .25f * Mathf.PI)) : 1;
                hearts[i].anchoredPosition = position[i];
                hearts[i].sizeDelta = Vector2.one * (size[i] * pop);
                hearts[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(sway[i] + age * swayRate[i]) * 12);
                var c = images[i].color; c.a = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(Life * .55f, Life, age)); images[i].color = c;
            }
            if (!alive) Destroy(gameObject);
        }
    }
}
