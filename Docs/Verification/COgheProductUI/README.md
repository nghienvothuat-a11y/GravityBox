# COghe Product UI — Unity implementation

Latest scoped update: [OPPO victory layout and fullscreen inspection, 3 October 2026](../COgheMobileUI/README.md).

## Manual victory continuation — 4 October 2026

User revision: the victory screen now waits for a tap on the existing **→ Level N**
text. Its appearance and centre stay unchanged, with an invisible 48 px touch
area and no new filled button. The close-up,
dance and first-win reward remain; the reward-offer timer and rewarded-ad closure
cannot load another scene. Next Level handles unlock notices and eligible
interstitials, ignores duplicate taps, and continues after ad closure/failure.
The final scene offers Menu. Product UI ignores the legacy `AutoAdvance` flag;
archived control experiments are unchanged.

Verification: **37/37 Product UI tests passed**, followed by **5/5 targeted tests**
after retaining the original text presentation. The real level-1 solve waits eight
seconds beyond the dance checks, then pixel-clicks the text to load level 2.
Separate UI fixtures verify reward completion keeps Victory open, duplicate taps
are ignored, ad failure releases an already-requested transition, and unlocks
wait for the tap. XML: `Artifacts/COgheManualVictory/product-ui.xml` and
`text-tap.xml`. Native Mac proof also passed: real level-1 escape, hold at
Victory, then explicit continuation to level 2. Captures/result:
`Artifacts/COgheManualVictory/native/`. Mac and Android builds succeeded; the
updated APK was **not installed** because OPPO was no longer listed by ADB.
Build hash/status: `Artifacts/COgheManualVictory/build-result.json`.
The automatic-progression evidence in the original September report is historical.

30 September 2026 · NewGraphic · Spatial 50 · Unity 6000.3.19f1.

The approved [UI mockup](../../ArtDirection/COghe/ProductUI/review.html) is implemented
as retained Unity UI. [Native screenshot gallery](review.html).

## Player flow

- Main Menu: live 3D COghe, Play, Home, replay Intro. Touch COghe to greet it.
- Play resumes the **first incomplete stable level ID**, even when a development
  save contains completions out of order. No product level picker or number-key skip.
- English UI, icon gameplay controls, safe-area layout and 48+ logical-pixel targets.
- Pause: Restart, Home, Menu, Help, independent Music and Sound settings. Restart
  and leaving an unfinished puzzle require confirmation. Pause stops simulation
  and every popup consumes input before it reaches the glass.
- Contextual buttons select fragments by mass, release a held task, inspect camera
  zones or return to the overview. Gesture controls remain unchanged.
- First-run Intro hands over to level 1; replay from Main Menu returns to Main Menu.
- Existing victory dance and close-up remain until the player chooses Next Level,
  including Boss levels. Level 50 remains on campaign completion with Menu.
  Home unlocks after the first Boss, at campaign position 10.
- Home retains the existing Feed/Play interactions. Collection explains that more
  furnishings are coming; this change does not implement a furniture shop.

## Implementation boundaries

`COgheProductUI` owns canvas, safe areas, modals and input exclusion.
`COgheUIArt` shares one icon atlas, sliced panel and fixed-weight Manrope fonts.
`COgheProductCatalog` references all 50 existing definitions in `SpatialOrder`.
No duplicated level IDs, new progress key, physics dimensions or scene regeneration.

Main Menu uses the real existing organism and `VenomLifeAnimation` in the existing
habitat. It neither grants Home nor writes completion. Returning to gameplay resets
and restores the authored apparatus. Menu/Home framing is presentation only.

Font assets are fixed 500/750-weight instances: Unity's legacy dynamic Font uses
weight 200 when given the original variable font. Regenerate with
`Tools/generate-coghe-ui-fonts.py` (fonttools required); OFL/Lucide licenses ship
beside the assets. Runtime requires no font-generation dependency.

Audio debounce timestamps are now cleared when the simulation clock restarts or
binds another scene; otherwise an earlier retry at time zero could suppress the
next retry sound. Turning Sound off also mutes an already-playing effect.

## Verification

- **50 Spatial solve tests passed** with all tissue escaping; this includes the
  complete current catalog and its existing mechanism/merge requirements.
- **50 Origin regression tests passed**, covering shared input, physics, split,
  fusion, slippery surfaces, tube transfer and exit behavior.
- **23 final UI / audio / Intro tests passed**: 10 Product UI, 8 audio and 5 Intro.
  The UI suite visits all 50 scene entries, clicks Pause/Resume, verifies safe
  targets and prevents commands through UI. It also tests real pixel clicks and synthetic touchscreen Pause/Resume,
  physical level-1 completion and automatic loading of level 2, fragment selection,
  save IDs, Home lock, confirmations and Intro handover.
- The initial combined run was 121/122: retry audio failed due to stale debounce
  time. After the clock-reset fix, all 22 affected UI/audio/Intro tests passed.
- Native Mac proof solves level 1 without teleporting, captures its celebration,
  verifies automatic advance to level 2, and renders menu/popup/Home/Intro at
  720×1280 and 720×1612. `native-result.json` records completion.
- The fragments screenshot uses a partitioned UI fixture in level 1. It tests
  selector presentation; it is **not** evidence of a splitter in that level.
- Manual native-window check: Play resumed the existing save at level 4; clicking
  Pause opened the new popup, and Menu → Confirm returned to Main Menu. No level
  was completed or progress reset during that check.
- Verification sessions disable campaign persistence. Normal gameplay keeps the
  existing company/product/bundle identity and save key.

Raw XML and logs: `Artifacts/COgheProductUI/`. A compact record of all 123 unique
cases and the latest results is in `verification.json`.

Not yet verified in this change: OPPO installation, physical notch/inset behavior,
Android touch feel or device frame rate. Mac captures do not establish mobile FPS.

## Build and review

Unity menu: **Gravity Box → COghe → Product UI → Build Mac**.
Output: `Builds/COgheProduct/macOS/COghe.app`.

The regular Spatial Android build also includes Product UI when rebuilt. Older
V2/Origin builds retain their earlier UI. Use `-coghe-developer-ui` for a Spatial
level picker; existing proof/benchmark launches keep their diagnostic UI.

Opt-in native verification (development build only; does not write campaign saves):

```sh
'Builds/COgheProduct/macOS/COghe.app/Contents/MacOS/Gravity Box' \
  -coghe-product-proof \
  -coghe-product-proof-output /Users/mrk/GravityBox/Docs/Verification/COgheProductUI \
  -screen-width 720 -screen-height 1280 -screen-fullscreen 0 \
  -logFile /Users/mrk/GravityBox/Artifacts/COgheProductUI/native-final.log
```
