namespace GravityBox.Venom
{
    /// <summary>How the player answers COghe in a bonus round.</summary>
    public enum COgheBonusAnswer { Touch, Feed, Item }

    /// <summary>One word COghe "says": the shape it makes, the answer that means "I understood you", and how many times
    /// (a number is shown first, as that many raised tendrils: "two" + mushroom = feed it exactly two).</summary>
    public readonly struct COgheBonusAsk
    {
        public readonly COgheShape Shape; public readonly COgheBonusAnswer Answer; public readonly string Item; public readonly int Count;
        public COgheBonusAsk(COgheShape shape, COgheBonusAnswer answer, string item = null, int count = 1)
        { Shape = shape; Answer = answer; Item = item; Count = count < 1 ? 1 : count; }
        public bool Accepts(COgheBonusAnswer answer, string item) => answer == Answer && (Answer != COgheBonusAnswer.Item || item == Item);
        /// <summary>The number shape before the word (none for one).</summary>
        public COgheShape? NumberShape => Count >= 2 && Count <= 3 ? COgheShape.One + (Count - 1) : (COgheShape?)null;
    }

    /// <summary>One question: a sentence of words, done in its order or in any order.</summary>
    public sealed class COgheBonusRound
    {
        public readonly COgheBonusAsk[] Words; public readonly bool Ordered;
        public COgheBonusRound(bool ordered, params COgheBonusAsk[] words) { Ordered = ordered; Words = words; }
    }

    /// <summary>
    /// The bonus "Hiểu ra" (Mrk 07/10/2026): after each chapter's boss, a skippable round in Home where COghe "speaks" only in
    /// simple body shapes and the player answers by doing what it asks. Each right answer pays at once, with COghe's joy, and
    /// more each question; understanding the whole bonus pays the most, after COghe's thank-you. A wrong answer only gets a
    /// gentle shake. Nothing here touches the puzzles: the bonus lives in Home and is optional (Skip goes on to the next
    /// chapter; a skipped bonus waits in Home).
    /// The answers only use what every player has (a touch, Feed, the gift ball), so no bonus depends on what was bought; the
    /// chapters grow by putting words together: one word, two in any order, an order, a number, all of them.
    /// </summary>
    public static class COgheBonus
    {
        private static COgheBonusAsk Cuddle(int n = 1) => new COgheBonusAsk(COgheShape.Heart, COgheBonusAnswer.Touch, null, n);
        private static COgheBonusAsk Food(int n = 1) => new COgheBonusAsk(COgheShape.Mushroom, COgheBonusAnswer.Feed, null, n);
        private static COgheBonusAsk Play() => new COgheBonusAsk(COgheShape.Ball, COgheBonusAnswer.Item, "BALL");
        private static COgheBonusRound One(COgheBonusAsk word) => new COgheBonusRound(true, word);
        private static COgheBonusRound Both(COgheBonusAsk a, COgheBonusAsk b) => new COgheBonusRound(false, a, b);
        private static COgheBonusRound InOrder(params COgheBonusAsk[] words) => new COgheBonusRound(true, words);

        private static readonly COgheBonusRound[][] Chapters =
        {
            // 1 (Home has just opened): one word each
            new[] { One(Cuddle()), One(Play()), One(Food()) },
            // 2: two things at once, in any order
            new[] { Both(Cuddle(), Food()), Both(Play(), Cuddle()), Both(Food(), Play()) },
            // 3: in COghe's order
            new[] { InOrder(Food(), Cuddle()), InOrder(Cuddle(), Play(), Food()), InOrder(Play(), Food(), Cuddle()) },
            // 4: how many (exactly: one too many and it counts again)
            new[] { One(Food(2)), One(Cuddle(3)), One(Food(3)) },
            // 5: all of it
            new[] { InOrder(Cuddle(2), Play()), InOrder(Food(), Cuddle(3)), InOrder(Food(2), Cuddle(), Play()) },
        };

        /// <summary>The questions of chapter <paramref name="chapter"/>'s bonus (1-based), or null if it has none.</summary>
        public static COgheBonusRound[] Rounds(int chapter) => chapter >= 1 && chapter <= Chapters.Length ? Chapters[chapter - 1] : null;
        public static string Key(int chapter) => "bonus:" + chapter;
        /// <summary>Done once its reward is paid (the shop records paid keys, so it pays once).</summary>
        public static bool Done(int chapter) => COgheShop.Paid(Key(chapter));
        /// <summary>The prize for understanding every question: the largest.</summary>
        public static int Reward => COgheEconomy.BonusDrops * COgheEntitlements.DropsMultiplier;
        /// <summary>Paid the moment question <paramref name="round"/> (1-based) is understood, growing question by question (10, 20, 30).</summary>
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
