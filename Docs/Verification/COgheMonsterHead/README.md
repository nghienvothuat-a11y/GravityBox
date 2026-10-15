# COghe — Liquid monster head at Home

Date: 15 October 2026 · Branch: `NewGraphic`

[Watch the runtime review](review.html). This replaces the humanoid monster act with a temporary head growing out of COghe’s liquid body. The same act is used by Home and Main Menu.

## Approved request and implementation

The user requested a gradual transformation: a head rises from slime, becomes a monster, bares fangs and extends its tongue. The implementation removes the torso, shoulders, arms and hands. A broad liquid pool supports overlapping lobes and the head in the existing implicit skin field.

The act lasts 9.6 seconds:

| Time | Visible action |
| --- | --- |
| 0–1.6 s | The body gathers and swells into a mound. |
| 0.55–3.05 s | The head rises; the eyes gradually appear. |
| 2.9–4.85 s | The mouth forms, the curved fangs emerge and the jaw opens. |
| 4.65–6.35 s | The tongue unfurls; the head holds a gentle three-quarter pose. |
| 6.35–7.75 s | The tongue retracts, jaw closes and the head tilts/blinks. |
| 7.7–9.6 s | Facial features disappear and the head melts back into COghe. |

Surface normals are sampled analytically. A bounded edge correction smooths the head while retaining the existing skin grid density. The face has smooth eye contours, curved fangs and an elliptical tongue. Geometry buffers are reused. Home’s existing zoom camera follows the rising head without automatically changing the user’s zoom selection.

Follow-up on 15 October: the tongue is **twice as wide**, with its previous thickness. Mrk found the first widened version too rigid, so the final version restores the earlier travelling-wave rhythm through the middle and tip. Only the short section at the mouth is damped; gravity adds a small droop. Its 29 cross-sections and 16 sides are unchanged. Both gallery videos show this final waveform.

The mobile optimization reuses corrected skin intersections/normals/ink across adjacent grid cells during the monster act, and reuses mouth samples and eye seam/centre samples. It does not lower the skin density, animation update rate, particle count or simulation rate. The ordinary gameplay skin path is unchanged.

Only presentation changes: particle positions, adhesion, splitting, merging, colliders, level state, progression and Plus access remain unchanged. Review command flags bypass access only in an in-memory fixture with saving disabled.

## Verification

- The original head redesign passed seven distinct Play Mode checks; historical results remain in [tests.json](tests.json).
- The iPhone optimization follow-up covered eight distinct checks: face lifecycle, physics independence, Home interruption/width, menu startup, cached/uncached mesh equivalence (including ink), two accessory checks and ink transport. After the final waveform revision, Home, menu and mesh equivalence were rerun: **3/3 passed**. See [all runs and the corrected initial test expectation](iPhone/tests.json).
- The Home test verifies that the body remains one fragment, no particles escape, cancelling the act removes the face, and returning to play leaves progress unchanged.
- Final native macOS build succeeded. Home and Main Menu frames were exported from that build and visually inspected at the mound, formed-head, fangs/tongue and return phases.
- No scene, material, profile or physics asset files were modified. `git diff --check` passed.

### Measured performance and limits

On the connected **iPhone 13 Pro Max / A15 / iOS 18.3.2**, at **1284 × 2778**, the optimized skin/face ran 90 repeated Home transformations over **918.14 seconds**: **59.995 FPS**, **16.81 ms p95**, **16.95 ms p99**, worst **32.41 ms**, and **zero frames over 33.334 ms**. All 90 cycles have complete sample coverage. This soak uses the first widened tongue motion; the subsequent flowing-wave revision has a separate final-device measurement in the linked report.

The same iPhone already ran the short pre-optimization baseline at 60 FPS. The benefit is reduced CPU work, not a claimed jump from 45 to 60 FPS on iPhone. Two earlier short after-runs were interrupted by application background/foreground transitions and are excluded as complete-run comparisons; the user confirmed the phone was untouched. Their cause remains unconfirmed. See the [device report, final waveform measurement, raw samples and build hashes](iPhone/README.md).

Historical Mac measurement before this optimization: 44.63 FPS over 10.2 seconds at 472 × 1022 ([raw data](timing-mac.json)). A later optimized Mac run before the waveform follow-up measured 59.54 FPS over 30.63 seconds, with one 39.43 ms frame ([data](iPhone/mac-optimized-before-wave.json)); the scenarios differ in duration and warm-up, so this is not a controlled A/B result. Neither Mac measurement substitutes for native mobile evidence.

The silent videos and stills use a fixed 30 FPS capture clock to inspect choreography; they are not performance evidence. The device tests warm up one full transformation before sampling. Cold startup, independent GPU timing and OS process RSS were not measured. OPPO and iPad were not profiled in this task.

## Deliverables and reproduction

- `home.mp4`, `menu.mp4`: final native runtime sequences.
- `home-*.png`, `menu-fangs.png`: unedited runtime frames.
- Local test application: `Builds/SpatialLab/macOS/COghe.app`.
- Local detailed test/build logs: `Artifacts/COgheMonster20261015/` (ignored build artifacts).
- Follow-up iPhone builds, raw captures, source snapshots and test XML: `Artifacts/COgheMonsterIPhone20261015/` (ignored).

Launch a test build with `-coghe-monster-reel /absolute/output/path -coghe-monster-home` to capture Home. Omit `-coghe-monster-home` for Main Menu. Add `-coghe-monster-realtime -coghe-monster-cycles 20` to measure the normal player loop without frame capture. The fixture is compiled only for Editor, development or benchmark builds. On iOS, where IL2CPP did not forward the launch flags to `Environment`, put the same argument array in `Documents/COgheMonsterRequest.json` as `{"args":[...]}`. The fixture consumes/deletes the explicit request and disables saving. A normal launch has no request and retains the player's existing progress.

## Main changed files

- `VenomLifeAnimation.Monster.cs`: liquid silhouette, pose phases and face anchors.
- `COgheMonsterFace.cs`: face geometry, fangs, tongue and materials.
- `VenomSurface.cs`: bounded implicit-surface correction during the monster act only.
- `COghePersonality.cs`: act duration and tongue sound cue.
- `COgheProductUI.Items.cs`: Home zoom framing.
- `COghePersonalityTests.cs`, `COgheMonsterHomeTests.cs`: lifecycle and Home regression checks.

This record supersedes the historical humanoid monster presentation in `Docs/Personality/COghe`.
