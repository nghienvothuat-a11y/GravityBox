using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>A piece of COghe's wardrobe: a hat worn on its crest, or a little thing floating inside its body.</summary>
    public sealed class COgheWear
    {
        public string Id, Name; public int Level; public bool Inside;
    }

    /// <summary>The wardrobe (Mrk 01/10: hats and things floating inside, both). Icons: Resources/COgheStyle/Icons/&lt;Id&gt;.</summary>
    public static class COgheWardrobe
    {
        public const int MaxInside = 2;
        public static readonly COgheWear[] Hats =
        {
            Hat("HAT_BEANIE", "Beanie", 13), Hat("HAT_PARTY", "Party hat", 18), Hat("HAT_FLOWER", "Flower crown", 20), Hat("HAT_CAP", "Cap", 25),
            Hat("HAT_STRAW", "Straw hat", 29), Hat("HAT_PROPELLER", "Propeller cap", 35), Hat("HAT_CROWN", "Crown", 42), Hat("HAT_ASTRO", "Space helmet", 54),
        };
        public static readonly COgheWear[] InsideItems =
        {
            In("FLOAT_STARS", "Star bits", 30), In("FLOAT_FISH", "Little fish", 32), In("FLOAT_BUBBLES", "Bubbles", 37),
            In("FLOAT_JELLY", "Baby jellyfish", 47), In("FLOAT_PEARLS", "Tiny pearls", 48), In("FLOAT_PLANET", "Tiny planet", 56),
        };
        public static COgheWear Find(string id)
        {
            foreach (var w in Hats) if (w.Id == id) return w;
            foreach (var w in InsideItems) if (w.Id == id) return w;
            return null;
        }
        private static COgheWear Hat(string id, string name, int level) => new COgheWear { Id = id, Name = name, Level = level };
        private static COgheWear In(string id, string name, int level) => new COgheWear { Id = id, Name = name, Level = level, Inside = true };
    }

    /// <summary>
    /// COghe's look, saved on the device: up to four inks and every particle's share of them, the marbling seed, the hat and
    /// up to two inside decorations. Applied wherever the live COghe appears (Home, main menu, every level).
    /// </summary>
    [Serializable]
    public sealed class COgheStyle
    {
        private const string Key = "coghe.style.v1";
        public string[] Inks = new string[COgheInking.Slots];
        public Vector4[] Amount = new Vector4[CohesiveOrganism.ParticleCount];
        public int Seed;
        public string Hat = "";
        public List<string> Inside = new List<string>(COgheWardrobe.MaxInside);

        private static COgheStyle current;
        /// <summary>The saved look (black and bare until the player changes it).</summary>
        public static COgheStyle Current
        {
            get
            {
                if (current != null) return current;
                current = new COgheStyle { Seed = Environment.TickCount & 0xffff };
                if (VenomCampaignSave.PersistenceEnabled)
                {
                    try { var loaded = JsonUtility.FromJson<COgheStyle>(PlayerPrefs.GetString(Key, "")); if (loaded != null) current = loaded.Valid(); }
                    catch (Exception e) { Debug.LogWarning("COghe style unreadable, starting black: " + e.Message); }
                }
                return current;
            }
        }
        /// <summary>Tests: forget the cached look (and use <paramref name="style"/> instead, if given).</summary>
        public static void ResetForTests(COgheStyle style = null) { current = style; }

        public bool HasInk { get { foreach (var a in Amount) if (a.x + a.y + a.z + a.w > .002f) return true; return false; } }
        public bool HasWear => !string.IsNullOrEmpty(Hat) || Inside.Count > 0;

        public void Save()
        {
            if (!VenomCampaignSave.PersistenceEnabled) return;
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(this)); PlayerPrefs.Save();
        }

        public COgheStyle Clone()
        {
            var c = new COgheStyle { Seed = Seed, Hat = Hat, Inside = new List<string>(Inside) };
            Array.Copy(Inks, c.Inks, Inks.Length); Array.Copy(Amount, c.Amount, Amount.Length);
            return c;
        }
        public void CopyFrom(COgheStyle s)
        {
            Seed = s.Seed; Hat = s.Hat; Inside.Clear(); Inside.AddRange(s.Inside);
            Array.Copy(s.Inks, Inks, Inks.Length); Array.Copy(s.Amount, Amount, Amount.Length);
        }

        private COgheStyle Valid()
        {
            if (Inks == null || Inks.Length != COgheInking.Slots) Inks = new string[COgheInking.Slots];
            if (Amount == null || Amount.Length != CohesiveOrganism.ParticleCount) Amount = new Vector4[CohesiveOrganism.ParticleCount];
            // an empty, unknown or repeated ink leaves its place empty, and its amounts with it
            for (int s = 0; s < Inks.Length; s++)
            {
                bool keep = !string.IsNullOrEmpty(Inks[s]) && COgheInks.Find(Inks[s]) != null && System.Array.IndexOf(Inks, Inks[s]) == s;
                if (keep) continue;
                Inks[s] = null; for (int i = 0; i < Amount.Length; i++) Amount[i][s] = 0;
            }
            if (Inside == null) Inside = new List<string>();
            var inside = new List<string>(COgheWardrobe.MaxInside);
            foreach (var id in Inside) { var w = COgheWardrobe.Find(id); if (w != null && w.Inside && !inside.Contains(id) && inside.Count < COgheWardrobe.MaxInside) inside.Add(id); }
            Inside = inside;
            var hat = COgheWardrobe.Find(Hat ?? ""); if (hat == null || hat.Inside) Hat = "";
            return this;
        }

        /// <summary>Dress the live COghe of <paramref name="game"/> in this look (inks, hat, inside decorations).</summary>
        public void ApplyTo(VenomCampaign game)
        {
            if (game == null || game.Matter == null) return;
            var surface = game.Matter.GetComponent<VenomSurface>(); if (surface == null) return;
            var inking = surface.GetComponent<COgheInking>();
            if (HasInk) { if (inking == null) inking = COgheInking.Attach(game, Seed); inking.Load(this); }
            else if (inking != null) { inking.Load(this); UnityEngine.Object.Destroy(inking); }   // empty at once, gone at the frame's end
            var wear = surface.GetComponent<COgheAccessories>();
            if (HasWear) { if (wear == null) wear = COgheAccessories.Attach(game); wear.Dress(Hat, Inside); }
            else if (wear != null) UnityEngine.Object.Destroy(wear);
        }
    }
}
