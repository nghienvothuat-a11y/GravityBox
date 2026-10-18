using System.Collections.Generic;

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

    /// <summary>One question: a sentence of words, done in its order or in any order; or (the last one of a chapter, Mrk
    /// 17/10/2026) a game played together with a toy (<see cref="Toy"/>), which the player keeps.</summary>
    public sealed class COgheBonusRound
    {
        public readonly COgheBonusAsk[] Words; public readonly bool Ordered;
        /// <summary>The toy of a game round (<see cref="COgheToys"/>), or null for a sentence.</summary>
        public readonly string Toy;
        /// <summary>How hard a game is: 0 in chapters 2–6, more later (longer beats and tunes, a faster wobble).</summary>
        public readonly int Level;
        public bool IsGame => Toy != null;
        public COgheBonusRound(bool ordered, params COgheBonusAsk[] words) { Ordered = ordered; Words = words; }
        private COgheBonusRound(string toy, int level) { Toy = toy; Level = level; Words = new COgheBonusAsk[0]; Ordered = true; }
        /// <summary>A game with <paramref name="toy"/>: COghe plays a beat on it, the player plays it back.</summary>
        public static COgheBonusRound Game(string toy, int level = 0) => new COgheBonusRound(toy, level);
    }

    /// <summary>
    /// The bonus "Hiểu ra" (Mrk 07/10/2026): after each chapter's boss, a required chapter interlude in Home where COghe "speaks" only in
    /// simple body shapes and the player answers by doing what it asks. Each right answer pays at once, with COghe's joy, and
    /// more each question; understanding the whole bonus pays the most, after COghe's thank-you. A wrong answer only gets a
    /// gentle shake. The interlude lives in Home. Its answers are required before the next chapter; leaving returns to the menu
    /// and completed questions resume without paying twice. Existing completed chapters and purchases are preserved.
    /// The answers only use what every player has (a touch, Feed, the gift ball), so no bonus depends on what was bought; the
    /// chapters grow by putting words together: one word, two in any order, an order, a number, all of them, and (the sixth
    /// chapter, Mrk 12/10/2026) a number on most words.
    /// A wrong answer is a little comedy of what COghe got instead (it spits the food out, giggles at a touch, the ball bonks
    /// it), then it asks again (Mrk 17/10/2026). Chapters 2 to 6 end with a game on a new toy (the drum, the horn,
    /// the balloons, the spinning top, the music box); the drum
    /// (<see cref="COgheToys"/>) is a word from chapter 3 on. A ball game (<c>Game("BALL")</c>) exists for later
    /// chapters; chapter 1 stays as it was (Mrk 17/10/2026).
    /// From chapter 7 the questions are made from the chapter number (<see cref="Generate"/>; Mrk 18/10/2026: up to 300
    /// levels), so every later boss has its "Hiểu ra" and its ability card.
    /// </summary>
    public static class COgheBonus
    {
        private static COgheBonusAsk Cuddle(int n = 1) => new COgheBonusAsk(COgheShape.Heart, COgheBonusAnswer.Touch, null, n);
        private static COgheBonusAsk Food(int n = 1) => new COgheBonusAsk(COgheShape.Mushroom, COgheBonusAnswer.Feed, null, n);
        private static COgheBonusAsk Play() => new COgheBonusAsk(COgheShape.Ball, COgheBonusAnswer.Item, "BALL");
        /// <summary>The drum, a word from chapter 3 on (chapter 2's game gives it to every player).</summary>
        private static COgheBonusAsk Drum(int n = 1) => new COgheBonusAsk(COgheShape.Drum, COgheBonusAnswer.Item, COgheToys.Drum, n);
        /// <summary>A toy as a word (the drum from chapter 3, the others from chapter 7): tap it, or tap it that many times.</summary>
        public static COgheBonusAsk ToyWord(string toy, int n = 1) => new COgheBonusAsk(ToyShape(toy), COgheBonusAnswer.Item, toy, n);
        public static COgheShape ToyShape(string toy) => toy == COgheToys.Horn ? COgheShape.Horn : toy == COgheToys.Balloon ? COgheShape.Balloon
            : toy == COgheToys.Top ? COgheShape.Top : toy == COgheToys.MusicBox ? COgheShape.MusicBox : COgheShape.Drum;
        private static COgheBonusRound One(COgheBonusAsk word) => new COgheBonusRound(true, word);
        private static COgheBonusRound Both(COgheBonusAsk a, COgheBonusAsk b) => new COgheBonusRound(false, a, b);
        private static COgheBonusRound InOrder(params COgheBonusAsk[] words) => new COgheBonusRound(true, words);

        private static readonly COgheBonusRound[][] Chapters =
        {
            // 1 (Home has just opened): one word each (Mrk 17/10/2026: chapter 1 stays this easy)
            new[] { One(Cuddle()), One(Play()), One(Food()) },
            // 2: two things at once, in any order; then the drum, played together (Mrk 17/10/2026: the last question is a
            // game with a new toy, which the player keeps)
            new[] { Both(Cuddle(), Food()), Both(Play(), Cuddle()), COgheBonusRound.Game(COgheToys.Drum) },
            // 3: in COghe's order (the drum is a word now); then the horn: COghe's tune played back in order
            new[] { InOrder(Food(), Cuddle()), InOrder(Cuddle(), Drum(), Food()), COgheBonusRound.Game(COgheToys.Horn) },
            // 4: how many (exactly: one too many and it counts again); then the balloons: just as many puffs
            new[] { One(Food(2)), One(Drum(3)), COgheBonusRound.Game(COgheToys.Balloon) },
            // 5: all of it
            new[] { InOrder(Cuddle(2), Play()), InOrder(Drum(2), Food(), Cuddle()), COgheBonusRound.Game(COgheToys.Top) },
            // 6: every word counts
            new[] { InOrder(Cuddle(2), Food(2)), InOrder(Play(), Drum(3), Food()), COgheBonusRound.Game(COgheToys.MusicBox) },
        };

        /// <summary>Chapters with a "Hiểu ra" at most (300 levels at 11 a chapter is 28; room to grow).</summary>
        public const int MaxChapters = 60;
        /// <summary>The questions of chapter <paramref name="chapter"/>'s bonus (1-based), or null if it has none.</summary>
        public static COgheBonusRound[] Rounds(int chapter)
        {
            if (chapter >= 1 && chapter <= Chapters.Length) return Chapters[chapter - 1];
            if (chapter < 1 || chapter > MaxChapters) return null;
            if (!generated.TryGetValue(chapter, out var rounds)) generated[chapter] = rounds = Generate(chapter);
            return rounds;
        }
        private static readonly Dictionary<int, COgheBonusRound[]> generated = new Dictionary<int, COgheBonusRound[]>();
        /// <summary>The toy games, one after another from chapter 7.</summary>
        public static readonly string[] GameCycle = { COgheToys.Drum, COgheToys.Horn, COgheToys.Balloon, COgheToys.Top, COgheToys.MusicBox };

        /// <summary>
        /// Chapter 7 on (Mrk 18/10/2026), made from the chapter number so it is the same every time (a player who leaves and
        /// comes back gets the same questions): a sentence of three words in COghe's order with a number on one, then four
        /// words with a number on one (two from chapter 10), then a toy game, the five in turn, harder every five chapters.
        /// Words are what every player has by then: a cuddle, food, the gift ball and the five toys of chapters 2–6; no word
        /// twice in a row, numbers 2 or 3.
        /// </summary>
        private static COgheBonusRound[] Generate(int chapter)
        {
            var rnd = new System.Random(chapter * 7919 + 13);
            string[] words = { "touch", "feed", "BALL", COgheToys.Drum, COgheToys.Horn, COgheToys.Balloon, COgheToys.Top, COgheToys.MusicBox };
            COgheBonusAsk Word(string w, int n) => w == "touch" ? Cuddle(n) : w == "feed" ? Food(n) : w == "BALL" ? new COgheBonusAsk(COgheShape.Ball, COgheBonusAnswer.Item, "BALL", n) : ToyWord(w, n);
            COgheBonusRound Sentence(int length, int numbers)
            {
                var picked = new List<string>(length);
                while (picked.Count < length)
                {
                    string w = words[rnd.Next(words.Length)];
                    if (picked.Contains(w)) continue;   // each word once: a sentence stays easy to read
                    picked.Add(w);
                }
                var counts = new int[length]; for (int i = 0; i < length; i++) counts[i] = 1;
                for (int k = 0; k < numbers; k++) { int i; do i = rnd.Next(length); while (counts[i] > 1); counts[i] = 2 + rnd.Next(2); }
                var asks = new COgheBonusAsk[length]; for (int i = 0; i < length; i++) asks[i] = Word(picked[i], counts[i]);
                return InOrder(asks);
            }
            int level = 1 + (chapter - 7) / 5;
            return new[] { Sentence(3, 1), Sentence(4, chapter >= 10 ? 2 : 1), COgheBonusRound.Game(GameCycle[(chapter - 7) % GameCycle.Length], level) };
        }
        public static string Key(int chapter) => "bonus:" + chapter;
        /// <summary>Completion checkpoint; legacy paid rewards also count as completed.</summary>
        public static bool Done(int chapter) => COgheShop.Understood(Key(chapter));
        public static int CompletedRounds(int chapter)
        {
            int count = 0; var rounds = Rounds(chapter);
            while (rounds != null && count < rounds.Length && COgheShop.Understood(RoundKey(chapter, count + 1))) count++;
            return count;
        }
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

        /// <summary>The earliest bonus whose boss is beaten but which is not done (including unfinished interludes from older saves), or 0.</summary>
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
