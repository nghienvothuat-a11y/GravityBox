using System;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>
    /// A reward you can see arrive (the bonus "Hiểu ra", Mrk 07/10/2026: "xong một câu thì cho phần thưởng luôn"): Drops burst
    /// out of COghe, then fly in an arc into the Drops counter, each one adding its share as it lands, while "+N" rises
    /// where they came from. UI images animated in code on game time, like <see cref="COgheConfetti"/>. Presentation only:
    /// the reward is already paid when this starts; it only shows it arriving.
    /// </summary>
    public sealed class COgheDropsFly : MonoBehaviour
    {
        private RectTransform[] drops;
        private Vector2[] start, burst;
        private float[] delay, travel;
        private int[] share;
        private bool[] landed;
        private Func<Vector2> target;
        private Action<int> arrived;
        private RectTransform label; private Text labelText;
        private Vector2 origin;
        private float clock;

        /// <summary>Starts it over <paramref name="parent"/> (a full-screen rect; positions in logical units from its lower-left
        /// corner): <paramref name="icons"/> drops carrying <paramref name="amount"/> from <paramref name="from"/> to wherever
        /// <paramref name="to"/> says the counter is now; <paramref name="arrived"/> gets each drop's share as it lands.</summary>
        public static COgheDropsFly Launch(RectTransform parent, Sprite drop, Font font, Vector2 from, Func<Vector2> to, int icons, int amount, Action<int> arrived, int seed)
        {
            var go = new GameObject("Drops fly", typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var fly = go.AddComponent<COgheDropsFly>(); fly.Build(drop, font, from, to, Mathf.Max(1, icons), amount, arrived, seed);
            return fly;
        }

        private void Build(Sprite sprite, Font font, Vector2 from, Func<Vector2> to, int icons, int amount, Action<int> onArrive, int seed)
        {
            var rnd = new System.Random(seed); float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
            target = to; arrived = onArrive; origin = from;
            drops = new RectTransform[icons]; start = new Vector2[icons]; burst = new Vector2[icons]; delay = new float[icons]; travel = new float[icons];
            share = new int[icons]; landed = new bool[icons];
            for (int i = 0; i < icons; i++)
            {
                var im = new GameObject("Drop", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                im.sprite = sprite; im.preserveAspect = true; im.raycastTarget = false; if (sprite == null) im.color = COgheUIArt.Teal;
                var r = im.rectTransform; r.SetParent(transform, false); r.anchorMin = r.anchorMax = Vector2.zero; r.pivot = Vector2.one * .5f;
                r.sizeDelta = Vector2.one * R(20, 28); r.anchoredPosition = from; r.gameObject.SetActive(false);
                drops[i] = r; start[i] = from;
                float angle = R(30, 150) * Mathf.Deg2Rad; burst[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * R(40, 90);
                delay[i] = i * .045f; travel[i] = R(.55f, .75f);
                share[i] = amount / icons + (i < amount % icons ? 1 : 0);
            }
            if (amount > 0)
            {
                labelText = new GameObject("Plus drops", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
                labelText.font = font; labelText.fontSize = 26; labelText.alignment = TextAnchor.MiddleCenter; labelText.raycastTarget = false;
                labelText.color = COgheUIArt.Teal; labelText.text = "+" + amount; labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
                label = labelText.rectTransform; label.SetParent(transform, false); label.anchorMin = label.anchorMax = Vector2.zero;
                label.sizeDelta = new Vector2(120, 36); label.anchoredPosition = from + Vector2.up * 30;
            }
        }

        private void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 1f / 20);
            clock += dt;
            bool alive = false;
            for (int i = 0; i < drops.Length; i++)
            {
                if (landed[i]) continue;
                alive = true;
                float age = clock - delay[i];
                if (age < 0) continue;
                if (!drops[i].gameObject.activeSelf) drops[i].gameObject.SetActive(true);
                // out in a little burst (.22 s), then an arc into the counter, faster as it goes
                const float pop = .22f;
                Vector2 popped = start[i] + burst[i];
                if (age < pop) { float k = 1 - (1 - age / pop) * (1 - age / pop); drops[i].anchoredPosition = Vector2.Lerp(start[i], popped, k); drops[i].localScale = Vector3.one * (.4f + .6f * k); continue; }
                float u = Mathf.Clamp01((age - pop) / travel[i]), e = u * u * (3 - 2 * u) * .4f + u * u * .6f;
                Vector2 end = target(), control = popped + burst[i] * .8f + Vector2.up * 40;
                drops[i].anchoredPosition = (1 - e) * (1 - e) * popped + 2 * (1 - e) * e * control + e * e * end;
                drops[i].localScale = Vector3.one * Mathf.Lerp(1, .7f, e);
                if (u >= 1) { landed[i] = true; drops[i].gameObject.SetActive(false); arrived?.Invoke(share[i]); }
            }
            if (label != null)
            {
                float k = Mathf.Clamp01(clock / 1.3f), pop2 = clock < .18f ? Mathf.Sin(clock / .18f * Mathf.PI * .5f) * (1 + .35f * Mathf.Sin(clock / .18f * Mathf.PI)) : 1;
                label.anchoredPosition = origin + Vector2.up * (30 + 46 * Mathf.Sqrt(k));
                label.localScale = Vector3.one * pop2;
                var c = labelText.color; c.a = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.75f, 1.3f, clock)); labelText.color = c;
                if (k < 1) alive = true;
            }
            if (!alive) Destroy(gameObject);
        }
    }

    /// <summary>A reward amount counted up from zero on a label ("+0 Drops" … "+100 Drops"), quick then easing, on game time.</summary>
    public sealed class COgheCountUp : MonoBehaviour
    {
        private Text text; private int amount; private string prefix, suffix; private float clock; private int shown = -1;
        public static void Run(Text label, int amount, string prefix, string suffix)
        {
            var c = label.gameObject.AddComponent<COgheCountUp>(); c.text = label; c.amount = amount; c.prefix = prefix; c.suffix = suffix;
            label.text = prefix + "0" + suffix;
        }
        private void Update()
        {
            clock += Mathf.Min(Time.deltaTime, 1f / 20);
            float u = Mathf.Clamp01((clock - .15f) / .9f);
            int now = Mathf.RoundToInt(amount * (1 - (1 - u) * (1 - u) * (1 - u)));
            if (now != shown)
            {
                if (shown >= 0 && now / 10 != shown / 10) COgheAudio.Instance?.Play("metal_clink", .12f, 0, .03f);
                shown = now; text.text = prefix + now + suffix;
            }
            if (u >= 1) { COgheAudio.Happy(); Destroy(this); }
        }
    }
}
