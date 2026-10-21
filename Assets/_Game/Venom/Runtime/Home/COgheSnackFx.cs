using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>
    /// The little bits a snack reaction throws around COghe at Home: puffs of flame and steam (spicy), falling snowflakes
    /// (icy), confetti (lovely). Small low-poly meshes moved by hand, no particle system, light or collider
    /// (Docs/ArtDirection/COghe/STYLE_RULES.md); on the room's simulation clock, so they hold while the room is paused.
    /// </summary>
    public sealed class COgheSnackFx : MonoBehaviour
    {
        public enum Bit { Flame, Steam, Snow, Confetti }
        struct Piece { public Transform T; public Vector3 From, Velocity, Spin; public float Born, Life, Size; public Bit Kind; }
        readonly List<Piece> live = new List<Piece>(48);
        readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
        COgheHomeMaterials materials; System.Func<float> now; int seed = 1;
        float clock => now != null ? now() : Time.time;

        /// <summary>The bits' holder under <paramref name="parent"/>; <paramref name="clock"/> is the room's simulation time.</summary>
        public static COgheSnackFx Create(Transform parent, COgheHomeMaterials materials, System.Func<float> clock)
        {
            var fx = new GameObject("Snack fx").AddComponent<COgheSnackFx>();
            fx.transform.SetParent(parent, false); fx.materials = materials; fx.now = clock; return fx;
        }
        public int Live => live.Count;

        /// <summary>A burst of flame from <paramref name="mouth"/> along <paramref name="toward"/>, with steam rising.</summary>
        public void Flames(Vector3 mouth, Vector3 toward, Vector3 up)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 spread = Random3() * .25f;
                Add(Bit.Flame, i % 3 == 0 ? "#FFD54F" : i % 3 == 1 ? "#FF7043" : "#E53935", mouth, (toward + up * .35f + spread).normalized * (.10f + .03f * Rand()), .45f + .1f * Rand(), .006f + .002f * Rand());
            }
            for (int i = 0; i < 3; i++) Add(Bit.Steam, "#ECEFF1", mouth + up * .02f, (up + Random3() * .3f).normalized * .06f, .9f, .007f);
        }
        /// <summary>Snowflakes falling round <paramref name="centre"/> from above it, for <paramref name="seconds"/>.</summary>
        public void Snow(Vector3 centre, Vector3 up, Vector3 across, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 side = Quaternion.AngleAxis(360f * i / count + 20 * Rand(), up) * across;
                Vector3 from = centre + up * (.07f + .03f * Rand()) + side * (.03f + .04f * Rand());
                var p = Add(Bit.Snow, "#BFE2FF", from, -up * (.035f + .015f * Rand()) + side * .008f, 1.6f + .5f * Rand(), .0045f);
                p.Born += .12f * i; Replace(p);   // they come one after another
            }
        }
        /// <summary>Confetti popping up and out round <paramref name="centre"/>.</summary>
        public void Confetti(Vector3 centre, Vector3 up)
        {
            var colours = new[] { "#FDCF70", "#E75C73", "#FFF4DE", "#7FD3C6" };
            for (int i = 0; i < 14; i++)
            {
                Vector3 side = Quaternion.AngleAxis(360f * i / 14 + 15 * Rand(), up) * Vector3.ProjectOnPlane(Vector3.right, up).normalized;
                Add(Bit.Confetti, colours[i % colours.Length], centre + up * .04f, side * (.07f + .04f * Rand()) + up * (.20f + .08f * Rand()), .9f + .2f * Rand(), .0045f);
            }
        }

        Piece Add(Bit kind, string colour, Vector3 at, Vector3 velocity, float life, float size)
        {
            var go = new GameObject(kind.ToString(), typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(transform, false); go.transform.position = at; go.transform.localScale = Vector3.zero;
            go.GetComponent<MeshFilter>().sharedMesh = MeshOf(kind);
            var r = go.GetComponent<MeshRenderer>(); r.sharedMaterial = materials.For(COgheLowPoly.Hex(colour), false);
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            var p = new Piece { T = go.transform, From = at, Velocity = velocity, Spin = Random3() * 540, Born = clock, Life = life, Size = size, Kind = kind };
            live.Add(p); return p;
        }
        void Replace(Piece p) { for (int i = 0; i < live.Count; i++) if (live[i].T == p.T) { live[i] = p; return; } }

        Mesh MeshOf(Bit kind)
        {
            string key = kind == Bit.Snow ? "snow" : kind == Bit.Confetti ? "flake" : "puff";
            if (meshes.TryGetValue(key, out var m)) return m;
            var k = new COgheLowPoly(); k.Paint(Color.white);
            if (key == "snow")
            {
                // a six-pointed flake: three crossed bars
                for (int i = 0; i < 3; i++) k.At(Vector3.zero, Quaternion.Euler(0, 0, 60 * i)).Box(Vector3.zero, new Vector3(1.6f, .28f, .18f));
            }
            else if (key == "flake") k.Box(Vector3.zero, new Vector3(1f, .14f, .7f));
            else k.Ico(Vector3.zero, Vector3.one, 1);
            var tmp = k.Bake("fx " + key, transform, materials); m = tmp.GetComponent<MeshFilter>().sharedMesh; Destroy(tmp);
            meshes[key] = m; return m;
        }

        void Update()
        {
            float time = clock;   // the room's clock: a paused room holds them
            for (int i = live.Count - 1; i >= 0; i--)
            {
                var p = live[i]; float age = time - p.Born;
                if (p.T == null) { live.RemoveAt(i); continue; }
                if (age < 0) { p.T.localScale = Vector3.zero; continue; }
                float u = age / p.Life;
                if (u >= 1) { Destroy(p.T.gameObject); live.RemoveAt(i); continue; }
                Vector3 at = p.From + p.Velocity * age; float size = p.Size;
                switch (p.Kind)
                {
                    case Bit.Flame: size *= Mathf.Sin(Mathf.Min(1, u * 2.2f) * Mathf.PI * .5f) * (1 - u * u) * 1.6f; at += Vector3.up * .03f * age * age; break;
                    case Bit.Steam: size *= .6f + 1.4f * u; size *= 1 - Mathf.Clamp01((u - .7f) / .3f); break;
                    case Bit.Snow: at += Vector3.right * .006f * Mathf.Sin(age * 5 + p.Born * 3); size *= Mathf.Min(1, u * 6) * (1 - Mathf.Clamp01((u - .8f) / .2f)); break;
                    case Bit.Confetti: at += Vector3.down * .45f * age * age; size *= 1 - Mathf.Clamp01((u - .75f) / .25f); break;
                }
                p.T.position = at; p.T.localScale = Vector3.one * Mathf.Max(0, size);
                p.T.localRotation = Quaternion.Euler(p.Spin * age);
            }
        }

        float Rand() { seed = seed * 1103515245 + 12345; return ((seed >> 8) & 0xFFFF) / 65535f; }
        Vector3 Random3() => new Vector3(Rand() - .5f, Rand() - .5f, Rand() - .5f) * 2;

        void OnDestroy() { foreach (var p in live) if (p.T != null) Destroy(p.T.gameObject); live.Clear(); }
    }
}
