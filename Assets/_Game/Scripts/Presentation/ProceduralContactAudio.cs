using UnityEngine;

namespace GravityBox.Presentation
{
    /// <summary>
    /// Small, deterministic placeholder material recordings. Synthesis supplies
    /// timbre only; GameFeedback derives their level and timing from contact data.
    /// Replace these with recorded ball/surface samples for final material authoring.
    /// </summary>
    internal static class ProceduralContactAudio
    {
        private const int SampleRate = 44100;

        public static AudioClip CreateImpact()
        {
            var samples = new float[(int)(SampleRate * 0.24f)];
            var random = new System.Random(1703);
            float lowNoise = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                float time = (float)i / SampleRate;
                float noise = (float)random.NextDouble() * 2 - 1;
                lowNoise += (noise - lowNoise) * 0.24f;
                float attack = 1 - Mathf.Exp(-time * 6000);
                float body = Mathf.Sin(2 * Mathf.PI * 245 * time) * Mathf.Exp(-time * 38) * 0.34f;
                body += Mathf.Sin(2 * Mathf.PI * 593 * time) * Mathf.Exp(-time * 45) * 0.20f;
                float metal = Mathf.Sin(2 * Mathf.PI * 1823 * time) * Mathf.Exp(-time * 27) * 0.11f;
                metal += Mathf.Sin(2 * Mathf.PI * 3167 * time) * Mathf.Exp(-time * 41) * 0.055f;
                float contact = lowNoise * Mathf.Exp(-time * 220) * 0.9f;
                samples[i] = (body + metal + contact) * attack;
            }
            return Clip("Steel bearing contact (procedural)", samples);
        }

        public static AudioClip CreateRolling()
        {
            const int seconds = 3;
            const int overlap = 4096;
            int length = SampleRate * seconds;
            var noise = new float[length + overlap];
            var filtered = new float[noise.Length];
            var samples = new float[length];
            var random = new System.Random(5907);
            for (int i = 0; i < noise.Length; i++) noise[i] = (float)random.NextDouble() * 2 - 1;
            float body = 0, texture = 0;
            for (int i = 0; i < filtered.Length; i++)
            {
                // Two gentle low passes retain a heavy bearing rumble and remove
                // the sandpaper-like high band from the earlier placeholder.
                body += (noise[i] - body) * .010f;
                texture += (noise[i] - texture) * .052f;
                filtered[i] = body * 1.45f + (texture - body) * .12f;
            }
            System.Array.Copy(filtered, samples, length);
            // Fold the continuation into the beginning. The sample immediately
            // after the loop end now flows into sample zero without a periodic click.
            for (int i = 0; i < overlap; i++)
            {
                float t = Mathf.SmoothStep(0, 1, (float)i / (overlap - 1));
                samples[i] = Mathf.Lerp(filtered[length + i], filtered[i], t) * .72f;
            }
            for (int i = overlap; i < samples.Length; i++) samples[i] *= .72f;
            return Clip("Steel bearing rolling (procedural)", samples);
        }

        private static AudioClip Clip(string name, float[] samples)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
