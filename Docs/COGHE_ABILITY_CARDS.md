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

## UI — child-friendly redesign, 17/10/2026

Audience requested by Mrk: ages 4–15. Illustrations carry the primary meaning;
short English labels remain for readers. The intended age range is a design goal,
not a claim of tested comprehension by children.

| Display name | Saved ability (unchanged) | Illustration | Main benefit |
| --- | --- | --- | --- |
| Playful | Emotion | Happy liquid COghe, coral hearts | Learn new tricks |
| Speedy | Agility | Low crawling silhouette, amber motion streaks | Move faster |
| Clever | Intelligence | Professor COghe with round glasses, teal academic cap and book (Mrk's follow-up) | Remember more |

- Mrk's layout revision: three equally sized cards share **one horizontal row**,
  with an 8-logical-pixel gap and 286-logical-pixel height. All three stay visible
  together on phones and tablets; no scrolling or preselected/featured choice.
  Entire card is tappable, including the picture and copy.
- Previewing a card never consumes it. Selection uses a stable outline **and a
  check**, then a large explicit “Choose [name]” button. The hit target does not
  jump on selection. All cards receive equal visual emphasis.
- Each card keeps the short benefit and next level. Its supporting explanation
  appears in a shared full-width area below the row when selected, so narrow
  cards do not need tiny copy. The 10-segment progress bars remain in Home's
  vertical powers summary, which is not a choice screen.
- MAX shows “All learned!” and cannot be selected. Other cards remain usable.
- First Clever confirmation explains both rules: wait 5 seconds to approach the
  next device, then **the player taps to use it**. It does not promise auto-solving.
- Keep ivory surfaces, blue-grey readable text, soft coral/amber/teal illustration
  wells, familiar shared icons and large confirmation/continue controls.
- Artwork is transparent pre-rendered UI, capped at 512 px on import, bilinear,
  alpha-aware, no mipmaps/CPU readback. Sprites are cached. No new live 3D renders,
  lights, shaders, per-frame animations, physics or ability balance changes.
- The professor outfit is **card art only**, not a change to the gameplay model or
  the user's wardrobe. Existing saves and internal enum names remain compatible.
- Source assets: `Assets/_Game/Venom/Resources/COgheUI/Abilities/`.
- [Art prompts](ArtDirection/COghe/AbilityCards/2026-10-17/prompts.json),
  [professor revision](ArtDirection/COghe/AbilityCards/2026-10-17/professor-prompt.txt).
  Made with built-in image_gen; images are in the repo, no generator needed to build.
- [Native UI review](Verification/COgheAbilityCards/review.html).
  Reproduce with a Mac test build and `-coghe-card-review <output-directory>`.
  The opt-in fixture uses in-memory progress and writes no player save.
- Existing development reel `-coghe-ability-reel <dir>` retains stable control
  identifiers even though visible names have changed.

## Verification — 17/10/2026 redesign

- **5/5 targeted PlayMode tests passed**, including real pointer selection,
  changing the preview without spending, one confirmed reward, MAX exclusion,
  Home's pending-card flow, and layout on 640×1136, 720×1280, 1080×2340 and
  1536×2048 viewports. Labels are measured against their boxes and artwork;
  equal card dimensions, one horizontal row, no preselection, unchanged hit areas
  after selection, no overlap and no off-screen controls are checked.
- Spatial macOS build succeeded. The native screenshot fixture covers the picker,
  each selection, first Clever confirmation, progress summary and MAX state.
- All changes are UI/assets/docs; scene files, definitions, physics and saved
  ability values were not changed. No new full campaign solve or phone benchmark
  is claimed for this presentation change.
- Next validation is observation with actual children in the intended age range:
  can they identify what each picture offers and change/confirm their choice?

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
