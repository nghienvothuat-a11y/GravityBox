# COghe V2 — levels 11–30

> Current revision: levels23–30 now use [simultaneous cooperation R2](COGHE_VIEW_V2_SIMULTANEOUS.md). The layout table and513/513 evidence below describe the original R1 expansion and remain as history. See [R2 verification](Verification/COgheSimultaneous/README.md) for current results.

Authorized by Mrk on 28 September 2026 after the [20-level concept](LevelDesign/COghe/ViewV2/ExpansionConcept/DESIGN.md). This is an extension of the view-only V2 campaign, with stable IDs `coghe.view.v2.11` through `.30`. The catalog lists all thirty V2 scenes. Follow-up requests authorize a distinct zoom lesson in level 04 and smooth outer-pane/trim visibility in all thirty. Original levels other than 04 preserve their physical layout; serialized presentation references are updated.

**Verified Mac prototype:** full PlayMode 513/513, EditMode 8/8, native touch replay 30/30 in accelerated and realtime modes. [Final evidence, images and limits](Verification/COgheViewExpansion/README.md). Individual dossiers are in [ViewV2](LevelDesign/COghe/ViewV2). Diagnostic artifacts are under `Artifacts/COgheViewExpansion`; failed diagnostic runs are not release evidence.

## Layout and mechanism intent

| Levels | Distinct dependency |
| --- | --- |
| 11 / 13 / 21 | A bridge's old parking area hides a lower exit pocket. A continuous slippery separator prevents dropping into it from the departure bank; the receiving bank supplies the descent. 13 adds a handle cover; 21 drives a separate exit cover. |
| 12 | A reversible lid reveals a broad descent to a lower corridor. |
| 14 / 15 | A real rail engages visible gears. 14 returns when disengaged; 15 moves one shared transmission between two outputs whose terminal catches persist. |
| 16 | The second bridge physically contacts the first bridge's sequencing dog until the first moves. No action-count check determines order. |
| 17 | An open transfer tube reaches an elevated rear platform, with a return route. |
| 18 | A Y tube has two actual mouth shutters. The wrong branch has a safe landing and return. Occupied tubing blocks the selector; the auxiliary branch catches the final release. |
| 19 | A selector alternates two room doors. A separately caught bridge persists when the player closes the side route and reopens the main one. |
| 20 | A cover reveals B; B powers the pipe inlet, the pipe reaches sealed bay C, C docks a bridge, and D opens the final cover at the front end of the U-shaped route. |
| 22 / 23 | A real knife makes unequal fragments. Selection preserves independent commands; proximity fusion follows existing physical rules. |
| 24 / 25 | A measured pad opens a spring-return door. 24 requires the worker to return; 25 supplies a far-side hold-open catch. |
| 26 | Pad load releases the bridge task. Losing load stops a partial journey; reholding allows the player to resume it. |
| 27 | One part crosses a pipe to operate the remote rail while the other holds the pad. The remote output catches the direct return door. |
| 28 | A held selector can revisit a dead-end alcove and switch to the final output without losing completed catches. |
| 29 | The outbound pipe reaches a service strip from which the worker docks the return bridge and catches its door for the holder. |
| 30 | The worker uses P and the same selector for the access door and reunion bridge, then C catches the return door. Both parts reunite before the final exit. |

The authorized art language is retained: cream porcelain and tile joints, thick pale-blue cutaway panes, amber handles and moving parts, cyan transfer tubes with open collars, silver blades and measured mint status lights. Decorative geometry has no colliders or navigation surfaces. The builder compares serialized collision, Rigidbody, joint, surface, tube and tap-rail data before and after applying art and fails on a difference.

## Runtime boundaries

- `COgheViewTransmission` applies bounded force to real output rails. Gear contact and real input positions select the output; settling at its terminal catch retains it. Lamps read measured state.
- `COgheTissueClearance` observes actual tissue and tube occupancy. It prevents unsafe selector movement without storing a puzzle solution.
- Optional tube-edge gates leave existing ungated networks unchanged. Optional spring-door latch handles allow the temporary door in 24 to use the same safety edge as latched doors.
- A curved transfer only applies its receiving lead and terminal-plane test once the tissue reaches the end along the actual path. A distant portion of a bent tube can already lie beyond that infinite plane; it must not count as arrival. This shared correction is covered by the full legacy suite and a dedicated Boss 30 case.
- The nine new knives hold the lowered physical blade until tissue clears the sensor. Players can choose their fragments without a timing race; the blade then retracts under the existing finite force.
- Body mass, cut topology, 32 particles, 120 Hz, fusion and exit rules remain shared. There is no cut cooldown, task-based fusion protection, forced equal split or third required role.
- Acceptance routes live only in `ChapterProof` author/development code. Runtime gameplay and Boss hints never consult them.

## Rebuild and test

Use Unity **6000.3.19f1** and the checked-in package lock.

- Generate the new scenes: **Gravity Box → COghe → V2 → Generate levels 11–30**. The saved scenes are ready to open without generation.
- Build a Mac player: `bash Tools/build-venom.sh`.
- Full PlayMode: `bash Tools/verify-coghe-expansion.sh` (graphics enabled; it discovers the new suites without a filter).
- Editor batch author/build entry point: `GravityBox.Editor.VenomCampaignBuilder.GenerateViewExpansionAndBuildMac`.
- Development player replay: `-coghe-view-proof <absolute-output-directory> -coghe-view-first 11 -coghe-view-last 30 -coghe-proof-quit`. The optional `-coghe-view-fast` uses scripted 120 Hz physics for diagnosis and must not be presented as performance evidence.

`COgheViewExpansionTests` has a complete solution, recovery/portrait checks and a distinct adverse scenario for every new level, plus cross-cutting transit reset, unequal cut, safety-edge and shortcut checks. The player injects InputSystem touch events for world interactions and drag; fragment selection uses the same public command as the HUD chips. This is author replay, not a new-player study or a substitute for phone input testing.

The Computer Use service could not initialize in this session (`process is not defined` twice), so no manual GUI playthrough is claimed. Android performance, temperature and new-player usability require separate evidence on the actual device; desktop captures and tests do not establish those results.

## Follow-up: smooth cutaway and distinct zoom lesson

`COgheViewPresentation` fades the complete near-facing pane and its trim using camera angle plus unscaled-time smoothing. Reversing a drag reverses the current opacity without resetting it. Original opaque materials/depth are restored at full visibility. `COgheViewFadeBuilder` serializes transparent material references into all thirty scenes, compares physical/input data before and after, and builds the Mac preview. Its menu is **Gravity Box → COghe → V2 → Update zoom lesson and smooth cutaway**. This command intentionally regenerates level 04 first, then applies presentation only.

[Level 04](LevelDesign/COghe/ViewV2/Level04.md) now teaches inspection of fine floor grooves and alternating end gaps around two low slippery baffles. Level 02 retains its ramp. All locomotion follows real surface picking, and a player who already sees the route can solve without zooming. Native `-coghe-view-fade-proof` records slow actual touch drags, intermediate opacity samples and screenshots in an author-only diagnostic.
