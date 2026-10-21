# COghe — Playtest refinement · 21/10/2026

User scope: Speedy water and stars; Clever reading; Flower crown colour; move F1 (old 66) into 15; preserve rounded crates at docking; remove redundant lower call button at 19; simplify Boss 21; clarify stairs at 31.

## Authoring and behavior

- **15** becomes `coghe.spatial.plus.f1` (Bi giữ cửa). `N29` is retired from the campaign, retained as an archived scene and stable save ID. There are 67 catalog entries, with bosses still at 10/21/32/43/54/65. A third pacing migration captures the old 68 positions before evaluating earned unlocks.
- **17 and shared docked blocks**: the solver hands a moving crate to a fixed collision replica at its stop. Both copies now have matching rounded skins. The renderer hand-off follows the contact hand-off and reverses on pulling back/Retry. The art repair leaves collider geometry and forces unchanged. A separate contact fix retains an already docked surface while its pawl remains latched within twice the entry tolerance; a deliberate reverse pull still releases it immediately. This avoids losing the rider’s grip from tiny settling under the stacked cargo load.
- **19 / content 13**: no lower call button beside spawn. The tray still requires boarding to go up. When empty and raised, tapping its visible panel from the nearby lower landing calls it down; this optional cargo-station behavior does not apply to Boss 10. The upper call panel remains useful for retrieving the tray from upstairs.
- **21 / B1**: retain Q, pad A, shelf lever B, the C1 lock, three nested cargo blocks and the exit tower. Remove the tiny overhang trap, which punished a reversible block order without a clear visual cause. C2 and C3 are freely reversible, without a hidden order lock; C1 still requires the whole body after the pad/lever latch. The exit landing is 6 cm deep at the rear (z = 24–30 cm): leaving C3 at the front leaves a visible 12 cm gap; C2 is needed for horizontal reach and C3 for rear alignment. Q’s delivery trays reach 18 cm to either side, clearing the trailing lobe from its casing before the next climb. Camera pitch is lower to expose all three block fronts. No new tutorial or automatic puzzle action.
- **31 / content 22**: clear top elevations above the floor of 3 cm (A), 6 cm (B), 9.1 cm (C span), 12 cm (exit landing). Two bearers support the span. Its handle stays at floor reach; the camera reveals the risers. Same B-away → C → B → A solution, tested again after geometry changes.

## Presentation budget

Speedy uses one depth-tested vertex-colour mesh, at most 80 short contact segments (640 triangles), fading over 2.4 s. Samples break when support changes, on jumps/teleports and while operating a prop/tube; coordinates follow their real support. Stars remain a sparse burst from those contacts, not continuous confetti.

Clever reads only while whole, stationary, supported and outside mechanisms. A 5.2 s act retrieves a small bound book, opens it, turns a page and returns it to the body. Two existing cosmetic tendrils hold the book. Player commands cancel through the same .16 s fade as other idle acts. Math glyphs rise during reading. No input lock, collider, extra skin source or force.

Flower crown uses coral, lilac, butter yellow, sky blue and fresh green leaves, with rounded petals and warm centres. Same wardrobe slot and ownership. Its tiny triangle cross-products were below `Vector3.Normalize`’s epsilon, producing zero normals and black petals despite correct material colours. The mesh kit now divides by the measured nonzero length; flower icospheres additionally opt into outward winding. Other legacy winding stays unchanged.

## Verification

Full PlayMode regression: **961 passed, 0 failed, 45 explicit tests skipped** (1006 discovered; 3446 s), recorded in `Artifacts/Tests/refinement-full-final.xml`. The shared runtime was unchanged during this run. Final B1 rear-landing / Q-tray geometry and N13 proof-route adjustments were checked separately: **13/13 focused solve, recovery and shortcut tests**, including 30/60 Hz simulation cases (`Artifacts/Tests/refinement-layout-final2.xml`). All five changed levels then passed normal-loop native Mac touch replays with 32 particles reunited and escaped; see the table below and `native-validation.json`.

Static curriculum validates 67 unique IDs, six fixed bosses and 12 learning chains. The rounded-art audit covers 31 docked decks across 22 scenes; before/after collider and force signatures match. After syncing the final layout back into the main project, Unity imported and prepared the catalog successfully: 67 stable IDs, no compilation errors (`Artifacts/COghePlaytestRefinement/final-main-import.log`). Asset GUID checks found no duplicates or dangling references to removed generated assets. Native images and videos are in `media/`. Device FPS is not inferred from simulation cadence or these images; this request does not include a new iPad installation.

Boss 21 correction: B is now a held spring lever (`HoldAtEnd`, no end latch); the C1 latch uses `Holds`, not merely `Rails.AtEnd`. A and B must be actively loaded together. After its pawl catches, both may leave. This makes Q useful and removes the former solo B-then-A bypass. The reference solve uses the real Hold command.

Solution memory was re-recorded from successful solves for displays 15, 17, 19, 21 and 31. Clever now recognizes an actively held spring lever as an operated step. The recorder excludes pads activated by props alone: on new 15, the useful cues are D (release ball) then C (crate), not walking to the pad already operated by the ball.

Recovery evidence: the Boss 21 test first hauls C1 before aligning C3, tries the exit (must stay blocked), walks back down, slides C3, then exits with all 32 particles. That exposed sub-millimetre carrier settling outside `AtEnd`; the bounded latched hand-off fix above resolves it. The test also checks releasing the dock with a real reverse effort. No creature/prop teleport is used in the solve/recovery path.

Native normal-loop touch replays, all 32 particles reunited and escaped:

| Display | Content | Taps | Result |
| --- | --- | ---: | --- |
| 15 | F1, formerly 66 | 6 | Pass |
| 17 | N13 | 12 | Pass |
| 19 | 13 | 9 | Pass |
| 21 | B1 final rear landing | 16 | Pass |
| 31 | 22 | 9 | Pass |

Replay correction: a rapid sequence of author taps can trigger the existing short tantrum. The native replay now waits for that normal input lock to clear before its next tap; it does not disable the personality or change gameplay. The trial change to N13’s final ledge height was reverted. The final 17 solve uses its original 9 cm island. Screen projection is recalculated immediately before each touch, after screenshot capture.
