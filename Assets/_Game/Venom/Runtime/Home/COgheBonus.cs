namespace GravityBox.Venom
{
    /// <summary>How the player answers COghe in a bonus round.</summary>
    public enum COgheBonusAnswer { Touch, Feed, Item }

    /// <summary>One thing COghe asks for: the shape it makes and the answer that means "I understood you".</summary>
    public readonly struct COgheBonusAsk
    {
        public readonly COgheShape Shape; public readonly COgheBonusAnswer Answer; public readonly string Item;
        public COgheBonusAsk(COgheShape shape, COgheBonusAnswer answer, string item = null) { Shape = shape; Answer = answer; Item = item; }
        public bool Accepts(COgheBonusAnswer answer, string item) => answer == Answer && (Answer != COgheBonusAnswer.Item || item == Item);
    }

    /// <summary>
    /// The bonus "Hiểu ra" (Mrk 07/10/2026): after each chapter's boss, a skippable round in Home where COghe "speaks" only in
    /// simple body shapes and the player answers by doing what it asks. Understanding it pays a large reward once and COghe
    /// thanks the player (it presses against the screen, makes a heart, hearts fly). Each right answer pays at once, with
    /// COghe's joy, and more each round; the prize for the whole bonus is the largest. A wrong answer only gets a gentle shake.
    /// Nothing here touches the puzzles: the bonus lives in Home and is optional (Skip goes on to the next chapter; a skipped
    /// bonus waits in Home).
    /// </summary>
    public static class COgheBonus
    {
        // Chapter 1 (Home has just opened): one shape, one answer, using what every player has at that point: a touch, food
        // and the gift ball. Later chapters add two shapes, an order, a number (planned; not defined yet).
        private static readonly COgheBonusAsk[][] Chapters =
        {
            new[]
            {
                new COgheBonusAsk(COgheShape.Heart, COgheBonusAnswer.Touch),
                new COgheBonusAsk(COgheShape.Ball, COgheBonusAnswer.Item, "BALL"),
                new COgheBonusAsk(COgheShape.Mushroom, COgheBonusAnswer.Feed),
            },
        };

        /// <summary>The rounds of chapter <paramref name="chapter"/>'s bonus (1-based), or null if it has none yet.</summary>
        public static COgheBonusAsk[] Rounds(int chapter) => chapter >= 1 && chapter <= Chapters.Length ? Chapters[chapter - 1] : null;
        public static string Key(int chapter) => "bonus:" + chapter;
        /// <summary>Done once its reward is paid (the shop records paid keys, so it pays once).</summary>
        public static bool Done(int chapter) => COgheShop.Paid(Key(chapter));
        /// <summary>The prize for understanding every round: the largest.</summary>
        public static int Reward => COgheEconomy.BonusDrops * COgheEntitlements.DropsMultiplier;
        /// <summary>Paid the moment round <paramref name="round"/> (1-based) is understood, growing round by round (10, 20, 30).</summary>
        public static int RoundReward(int round) => COgheEconomy.BonusRoundDrops * round * COgheEntitlements.DropsMultiplier;
        public static string RoundKey(int chapter, int round) => Key(chapter) + ":" + round;

        /// <summary>The chapter whose boss is catalog level <paramref name="order"/> (1-based), or 0 if it is not a boss.</summary>
        public static int ChapterOfBoss(COgheProductCatalog catalog, int order)
        {
            if (catalog == null || order < 1 || order > catalog.Levels.Length || catalog.Levels[order - 1] == null || !catalog.Levels[order - 1].Boss) return 0;
            int chapter = 0;
            for (int i = 0; i < order; i++) if (catalog.Levels[i] != null && catalog.Levels[i].Boss) chapter++;
            return chapter;
        }

        /// <summary>A bonus to offer after beating catalog level <paramref name="order"/>: its chapter, or 0.</summary>
        public static int After(COgheProductCatalog catalog, int order)
        {
            int chapter = ChapterOfBoss(catalog, order);
            return chapter > 0 && Rounds(chapter) != null && !Done(chapter) ? chapter : 0;
        }

        /// <summary>The earliest bonus whose boss is beaten but which is not done (skipped earlier), or 0.</summary>
        public static int Waiting(COgheProductCatalog catalog, VenomCampaignSave save)
        {
            if (catalog == null || save == null) return 0;
            int chapter = 0;
            for (int i = 0; i < catalog.Levels.Length; i++)
            {
                var level = catalog.Levels[i]; if (level == null || !level.Boss) continue;
                chapter++;
                if (save.Completed.Contains(level.Id) && Rounds(chapter) != null && !Done(chapter)) return chapter;
            }
            return 0;
        }
    }
}
