# COghe ability cards (Mrk, 16/10/2026)

After each "Understand COghe" (the bonus that follows each Boss) the player takes **exactly one** of three cards: Emotion,
Agility or Intelligence. Each pick adds one level to that ability, up to 10.

Mrk approved "clear direction, hidden details":
- cards say what grows, not by how much;
- Emotion's tricks are discovered at Home;
- Intelligence explains itself once;
- the Home popup keeps the bars and "Lv n/10".

Part 1 (this document): the cards, the Home Ability button and popup, Agility and Intelligence. Part 2: the 20
Emotion animations (list in the design reply to Mrk, 16/10).

## Rules and data

- **State:** `COgheShop.State`: `Emotion`, `Agility`, `Intelligence` and `CardsPicked`, the chapters whose card is
  taken.
  - `COgheShop.PickAbility(chapter, ability)` takes one card per chapter and refuses a full ability.
  - `COgheAbilities.Pending()` is the earliest chapter whose Understand COghe is done but whose card is not taken. A
    player who leaves during the choice keeps the card; it waits with a red dot on Home's Ability button.
- **Agility:** `VenomCampaignMotion.SpeedScale` = 1 + 0.1 × level, set at each level reset in the product. It covers
  walking, climbing and crawling.
  - Home keeps the base pace.
  - Devices (rails, lifts, gears, Q) keep their own speeds, so puzzle logic does not change.
  - The final approach of every move keeps the base pace, so a faster COghe still stops exactly where it is sent.
- **Intelligence:** `COgheIntelligence`, product levels only.
  - After `IdleDelay` (5 s) without input, COghe looks around for a second and walks to the next step of the level's
    solution: beside a rail's handle, next to a weight pad (never onto it), or up to another device.
  - It never works a device itself. The player's operation of that device advances the step.
  - Level N remembers N steps. Past them, it shows the question-mark shape once per pause.
  - Boss levels are included.
- **Steps:** `Resources/COgheUI/SolutionSteps.json`, one ordered list per `Definition.Id`, recorded by the Explicit
  PlayMode test `RecordSolutionSteps` from the same reference solutions the solve tests play.
  - A rail counts when a journey completes, a pad when it switches on, and any other device when it takes a tap it does
    not refuse.
  - Re-record after changing a level's devices or solution.

## UI

- **Home:** "Ability" (Lucide wallet-cards) replaces "Play". Play only cuddled COghe, which a tap on COghe does.
- **`AbilityStats` popup:** three rows with icon, name, a 10-segment bar, "Lv n/10" and a short line with no numbers.
  "Choose your card" appears when a card waits.
- **`AbilityPick` popup:** opens from `FinishBonus` when a card waits.
  - Three cards: art, name, "Lv n → n+1", 10 pips, and what grows.
  - Tap a card, then "Choose X". The confirmation shows the chosen card, COghe's shape reaction and, for the first
    Intelligence level, the 5-second tip. Continue then goes on to the next level, as after a bonus.
- **Art:**
  - Icons Smile, Zap, Brain and Cards in the four free cells of `Resources/COgheUI/Icons.png`.
  - Card art in `Resources/COgheUI/Abilities/` (renders of the in-game COghe).
- **Review reel:** `COgheAbilityReel` (development builds), run with `-coghe-ability-reel <dir>`.

## Verification (16/10/2026)

- **Tests:** `COgheAbilityTests` and `COgheAbilityUITests`.
  - One card per chapter, up to 10, nothing waits once all are full.
  - In the same 1.5 s, COghe covers 0.23 m at Agility 0 and 0.46 m at Agility 10.
  - G2 at Intelligence 1: COghe walks beside A without pulling it, wonders after A, and goes for B at Intelligence 2.
  - The UI flow after the bonus and from Home.
- **Agility 10 sweep** (`SolveEveryLevelAtTopAgility`, Explicit): 63/65 reference solutions solve at 200%.
  - N18 and N33 stop at a merge: the scripted tap lands on a part and selects it.
  - Both halves fuse when called together directly at 200% (checked in the same run).
- **Native reel** (Mac development build): Home → Ability → choose Intelligence → tip → G2 → beside A → tap A → "?".
