using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// Win-screen confetti (Mrk: cheerful but quiet, not showy): two small party-popper bursts from the lower corners, paper
    /// pieces that flip and flutter down and fade. ~50 UI images in the product palette, animated in code, gone in 3.5 s.
    /// </summary>
    public sealed class COgheConfetti : MonoBehaviour
    {
        private const int Count = 52;
        private const float Gravity = 560, Drag = 1.9f, Life = 3.6f;
        private static readonly Color[] Palette =
        {
            new Color(.737f, .467f, .439f), new Color(.875f, .729f, .439f), new Color(.208f, .427f, .427f),
            new Color(.686f, .796f, .835f), new Color(.737f, .467f, .439f), new Color(.875f, .729f, .439f), new Color(.91f, .878f, .812f),
        };
        private RectTransform[] pieces;
        private Image[] images;
        private Vector2[] position, velocity, size;
        private float[] spin, spinRate, sway, swayRate, delay;
        private float clock, width, height;
        private System.Random rnd;

        /// <summary>Starts the confetti over <paramref name="parent"/> (a full-screen rect, logical units).</summary>
        public static COgheConfetti Burst(RectTransform parent, float width, float height, int seed)
        {
            var go = new GameObject("Win confetti", typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var confetti = go.AddComponent<COgheConfetti>(); confetti.Build(width, height, seed);
            COgheAudio.Instance?.Play("confetti_pop", .45f, 0, .5f);
            return confetti;
        }

        private void Build(float w, float h, int seed)
        {
            width = w; height = h; rnd = new System.Random(seed);
            pieces = new RectTransform[Count]; images = new Image[Count]; position = new Vector2[Count]; velocity = new Vector2[Count]; size = new Vector2[Count];
            spin = new float[Count]; spinRate = new float[Count]; sway = new float[Count]; swayRate = new float[Count]; delay = new float[Count];
            for (int i = 0; i < Count; i++)
            {
                bool left = i % 2 == 0;
                var go = new GameObject("Paper", typeof(RectTransform), typeof(Image));
                var r = go.GetComponent<RectTransform>(); r.SetParent(transform, false);
                r.anchorMin = r.anchorMax = new Vector2(0, 0); r.pivot = Vector2.one * .5f;
                var im = go.GetComponent<Image>(); im.raycastTarget = false; im.color = Palette[rnd.Next(Palette.Length)];
                pieces[i] = r; images[i] = im;
                // a popper in each lower corner, aimed up and inward
                position[i] = new Vector2(left ? -6 : w + 6, h * .18f);
                float angle = R(58, 80) * Mathf.Deg2Rad, speed = R(430, 700) * Mathf.Sqrt(h / 640f);
                velocity[i] = new Vector2((left ? 1 : -1) * Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                bool ribbon = rnd.NextDouble() < .25;
                size[i] = ribbon ? new Vector2(R(2.5f, 3.5f), R(11, 15)) : new Vector2(R(5, 8), R(3.5f, 5.5f));
                spin[i] = R(0, 6.28f); spinRate[i] = R(5, 11) * (rnd.NextDouble() < .5 ? -1 : 1);
                sway[i] = R(0, 6.28f); swayRate[i] = R(2.2f, 4f);
                delay[i] = left ? R(0, .08f) : .12f + R(0, .08f);
                r.sizeDelta = size[i]; go.SetActive(false);
            }
        }
        private float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);

        private void Update()
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 1f / 20);
            clock += dt;
            for (int i = 0; i < Count; i++)
            {
                float age = clock - delay[i];
                if (age < 0) continue;
                if (!pieces[i].gameObject.activeSelf) pieces[i].gameObject.SetActive(true);
                // air drag slows the burst; then the paper falls slowly, swaying
                velocity[i] *= Mathf.Exp(-Drag * dt);
                velocity[i].y -= Gravity * dt * Mathf.Lerp(.35f, 1, Mathf.Clamp01(velocity[i].y / 200f + .5f));
                velocity[i].y = Mathf.Max(velocity[i].y, -95);
                position[i] += velocity[i] * dt + new Vector2(Mathf.Sin(age * swayRate[i] + sway[i]) * 26 * dt, 0);
                spin[i] += spinRate[i] * dt;
                var r = pieces[i];
                r.anchoredPosition = position[i];
                r.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(age * 1.7f + sway[i]) * 40 + spin[i] * 12);
                r.localScale = new Vector3(Mathf.Abs(Mathf.Cos(spin[i])) * .85f + .15f, 1, 1);   // flipping paper
                float fade = 1 - Mathf.Clamp01((age - (Life - .7f)) / .7f);
                var c = images[i].color; c.a = fade; images[i].color = c;
            }
            if (clock > Life + .3f) Destroy(gameObject);
        }
    }
}
