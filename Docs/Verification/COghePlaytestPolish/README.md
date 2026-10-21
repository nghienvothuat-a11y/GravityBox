# COghe — playtest polish, 20/10/2026

Scope: the eight requests following the iPad playtest, on NewGraphic. No progression reorder or Boss 10 layout change.

| Area | Contract |
| --- | --- |
| Intro → 01 | Pull the hands shot back to the live creature’s screen position and size, then crossfade into the actual level camera/creature; the final shot contains no painted substitute box. Skip still hands over immediately. |
| 02 pedestal | Render skin has 1.5 mm clearance from coincident glass/floor planes. Colliders, routes and step sizes stay unchanged. Applied to saved scene and the authoring pass. |
| Victory Drops ad | Keep the offer until Next. Loading disables it in place; cancelled/failed ads retain it. A successful reward leaves a disabled “Claimed” receipt at the same position. Reward ledger still pays once. |
| Sliding crates | Allow the available part of a blocked lane. A second tap reverses if still blocked, or continues if the blocker was removed. No position teleport at partial stops. The red crate must still clear the floor hole physically. |
| Understand / Feed | Apple silhouette with two shoulders, stem and leaf; Mushroom remains available as an unrelated idle shape. Existing enum IDs unchanged. |
| Ability accents | Playful: slow floating coral hearts. Speedy: pearl/mint glints from actual support contact while crawling. Clever: small cyan arithmetic symbols. One pooled mesh, 12 motes maximum, ~2 s bursts with 14–20 s between; gameplay only. |
| First ink | Mint is free and is the guided syringe. Back/Escape cannot leave the lesson. Done completes it after real injection. Old players retain Ocean and completed lessons remain completed. |
| Passenger lift | Tray controls require boarding and pressing; no implicit remote call from below. Stop a passenger trip if its rider leaves. Explicit landing call panels and authored deadman return remain distinct. |

## Verification

- Full PlayMode run: **941 passed, 0 failed, 45 explicitly skipped / 986 total** (3,216 seconds), `Artifacts/Tests/playtest-polish-full.xml` and `.log`.
- This includes every Spatial solve/wander group, the full direct-exit bypass sweep, crate reversals and partial motion followed by the K01 solve, Boss 10's complete passenger sequence, driver-leaves-mid-ride recovery, first-launch/skip, Mint save/reload and ad cancellation.
- Level 2 repair command compared the scene's physics fingerprint before/after: unchanged. Saved scene and the common authoring art pass both use the separated render skin.
- Final UI-only refinements: retain the claimed reward receipt, preserve ability cooldown across pause/resume and prioritize the newest card. **16/16 final targeted regressions passed** (49 seconds), including the new actual-contact Speedy test. Physics code is unchanged since the full run. See [machine-readable test summary](tests.json). The final pullback/intro and Apple refinements passed another **5/5 regressions** (38 seconds), including the complete food-request lesson and immediate Skip. Final native Mac review completed successfully (exit 0). Captures show the aligned intro handover, connected apple stem/leaf, three sparse ability accents, Mint injection through the Done step, and two of eight rotated pedestal views. See [visual review](review.html).
- Intro's final frame equals the live level render pixel-for-pixel at 540×1170 and 768×1024.

The tests' time values are suite duration, not frame-rate measurements. Editor/Mac verification is not a claim of mobile-device FPS. Existing player saves are disabled in the native proof fixture.

## Native build and reproducibility

- Unity 6000.3.19f1, `GravityBox.Editor.COgheProductUIBuilder.BuildMac`. Development player: `Builds/COgheProduct/macOS/COghe.app`. Build succeeded.
- Run the executable `Contents/MacOS/Gravity Box` with `-coghe-playtest-review /absolute/output/directory`. Final proof reports `POLISH REVIEW DONE: Mint injected, mandatory Done=True`. The screenshots exercise presentation; gameplay solve evidence comes from the PlayMode suite.
- Logs: `Artifacts/COghePlaytestPolish/build-mac-verified.log` and `native-verified.log`. Source/image hashes are in [build manifest](manifest.json). No iPad deployment or mobile performance measurement was performed in this task.
- Existing development-build proof generator logs `Expected exactly one full solution for each campaign level 30–40`. Its unchanged test-name matcher finds 31–40 but misses the existing Boss 30 method. Unity still completes the build successfully. This independent generated ChapterProof issue is not evidence that those player-proof scenarios ran; this review uses its own fixture and the separate full PlayMode suite.
