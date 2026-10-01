using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>One injectable ink: its colour and the material it brings into COghe's liquid.</summary>
    public sealed class COgheInk
    {
        public string Id, Name; public int Level;
        /// <summary>What it does to the body, in words (the Style screen: "Clear", "Metal", "Glow · semi-clear"…).</summary>
        public string Material;
        public Color Color, Accent;
        /// <summary>x metallic, y smoothness, z glow, w clarity (how see-through it makes the body).</summary>
        public Vector4 Surface;
        /// <summary>x stars, y nebula clouds, z iridescence, w glowing veins.</summary>
        public Vector4 Fx;
        public bool Clear => Surface.w > .15f;   // see-through enough for things floating inside
    }

    /// <summary>The twelve syringes (Mrk 01/10, kept as planned), unlocked through the campaign.</summary>
    public static class COgheInks
    {
        public static readonly COgheInk[] Catalog =
        {
            Ink("INK_OCEAN", "Ocean", 10, new Color(.04f, .32f, .78f), new Color(.55f, .85f, 1f), new Vector4(0, .82f, 0, .3f), new Vector4(0, .25f, 0, 0), "Clear"),
            Ink("INK_MINT", "Mint", 10, new Color(.3f, .82f, .62f), new Color(.8f, 1f, .92f), new Vector4(0, .7f, 0, 0), Vector4.zero, "Solid"),
            Ink("INK_CORAL", "Coral", 13, new Color(.95f, .38f, .3f), new Color(1f, .72f, .55f), new Vector4(0, .65f, 0, 0), new Vector4(0, .2f, 0, 0), "Solid"),
            Ink("INK_FIREFLY", "Firefly", 16, new Color(.25f, .9f, .25f), new Color(.85f, 1f, .45f), new Vector4(0, .78f, .9f, .2f), new Vector4(.25f, .3f, 0, 0), "Glow · semi-clear"),
            Ink("INK_GOLD", "Gold", 19, new Color(1f, .78f, .32f), new Color(1f, .92f, .6f), new Vector4(1, .82f, 0, 0), new Vector4(0, 0, .08f, 0), "Metal"),
            Ink("INK_SAKURA", "Sakura", 22, new Color(.95f, .5f, .66f), new Color(1f, .85f, .9f), new Vector4(0, .72f, .05f, 0), new Vector4(0, .3f, .15f, 0), "Soft shimmer"),
            Ink("INK_STARDUST", "Stardust", 25, new Color(.1f, .16f, .45f), new Color(1f, .92f, .7f), new Vector4(.05f, .68f, .08f, 0), new Vector4(1, .35f, 0, 0), "Stars"),
            Ink("INK_AURORA", "Aurora", 28, new Color(.15f, .75f, .68f), new Color(.7f, .3f, 1f), new Vector4(0, .85f, .25f, .2f), new Vector4(0, .6f, .8f, 0), "Iridescent · semi-clear"),
            Ink("INK_LAVA", "Lava", 32, new Color(.85f, .18f, .05f), new Color(1f, .55f, .1f), new Vector4(0, .5f, .15f, 0), new Vector4(0, .3f, 0, 1), "Glowing veins"),
            Ink("INK_PEARL", "Pearl", 36, new Color(.9f, .88f, .84f), new Color(.8f, .9f, 1f), new Vector4(.3f, .9f, 0, 0), new Vector4(0, 0, .45f, 0), "Pearlescent"),
            Ink("INK_GALAXY", "Galaxy", 42, new Color(.18f, .06f, .42f), new Color(.95f, .4f, .85f), new Vector4(0, .8f, .25f, .22f), new Vector4(.9f, 1, 0, 0), "Clear · nebula"),
            Ink("INK_PRISM", "Prism", 50, new Color(.85f, .85f, .9f), new Color(1f, 1f, 1f), new Vector4(0, .9f, .1f, .2f), new Vector4(0, 0, 1, 0), "Rainbow · semi-clear"),
        };

        public static COgheInk Find(string id) { foreach (var ink in Catalog) if (ink.Id == id) return ink; return null; }

        private static COgheInk Ink(string id, string name, int level, Color color, Color accent, Vector4 surface, Vector4 fx, string material)
            => new COgheInk { Id = id, Name = name, Level = level, Color = color, Accent = accent, Surface = surface, Fx = fx, Material = material };
    }
}
