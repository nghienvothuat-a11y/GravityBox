namespace GravityBox.Venom
{
    /// <summary>
    /// COghe's toys (Mrk 17/10/2026): a line of their own, apart from the Home furniture sold for Drops. Each "Hiểu ra" ends
    /// with a game the player plays together with COghe using a new toy; the toy is then the player's for free, stays in
    /// the room, and COghe plays with it when it is tapped. Toys are never sold, never listed with the furniture, and never
    /// depend on a level reached: owning one is what puts it in the room.
    /// </summary>
    public static class COgheToys
    {
        public const string Drum = "TOY_DRUM", Horn = "TOY_HORN", Balloon = "TOY_BALLOON", Top = "TOY_TOP", MusicBox = "TOY_MUSICBOX";
        /// <summary>Each toy and the chapter whose "Hiểu ra" game gives it.</summary>
        public static readonly (string id, string name, int chapter)[] Catalog =
        {
            (Drum, "Drum", 2), (Horn, "Horn", 3), (Balloon, "Balloons", 4), (Top, "Spinning top", 5), (MusicBox, "Music box", 6),
        };

        public static bool Is(string id)
        {
            foreach (var t in Catalog) if (t.id == id) return true;
            return false;
        }
        public static string Name(string id)
        {
            foreach (var t in Catalog) if (t.id == id) return t.name;
            return id;
        }
        /// <summary>The toy of chapter <paramref name="chapter"/>'s game, or null.</summary>
        public static string OfChapter(int chapter)
        {
            foreach (var t in Catalog) if (t.chapter == chapter) return t.id;
            return null;
        }
        /// <summary>Won in its game (never a gift, a purchase or a test's "everything unlocked").</summary>
        public static bool Owns(string id) => COgheShop.Current.Owned.Contains(id);
        /// <summary>The game with it is won: it is the player's (once).</summary>
        public static void Give(string id) => COgheShop.Grant(id, "bonus_toy");
        /// <summary>Saves that finished a chapter's "Hiểu ra" before its toy existed get the toy: later questions use it as a
        /// word, and every player who reached them has earned it.</summary>
        public static void GrantEarned()
        {
            foreach (var t in Catalog) if (!Owns(t.id) && COgheBonus.Done(t.chapter)) COgheShop.Grant(t.id, "bonus_toy_earned");
        }
    }
}
