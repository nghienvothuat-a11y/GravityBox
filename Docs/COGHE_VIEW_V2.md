# COghe V2 — 30 levels with view-only controls

Authorized by Mrk on 28 September 2026 (Buzz event `09cb747e27dc1fbdfb6763a0a443dba3db980f474115b1d5e2612518ef279825`). The opening ten-level chapter replaces physical box rotation. The current catalog adds levels 11–30 and the follow-up camera/level04 revisions described below. Archived content retains its own controls and remains regression coverage.

Unity 6000.3.19f1, URP 17.3.0, Input System 1.17.0. The initial ten-level implementation began at `317fdae`. Current source/build evidence is recorded in the [expansion report](Verification/COgheViewExpansion/README.md); historical checks do not certify subsequent changes.

## Player contract

Tap a visible surface or handle. The organism navigates, braces and supplies finite effort to a real rail. Drag left/right orbits the camera about world Y with fixed pitch. The chamber and gravity stay unchanged. Pinch zoom changes the view; all contacts from a pinch must end before another tap. Overview restores the overview. Retry resets the puzzle and camera. No Follow control in V2.

Exterior faces looking toward the camera are omitted from picking and rendering, retaining collision/adhesion. Interior walls, shutters and lids still obstruct the ray. A hidden handle must not activate through a cover. The cutaway is presentation only. Camera framing uses authored static bounds, not moving-object bounds.

## Content

| Slot | Stable ID | Lesson | Author solution |
| --- | --- | --- | --- |
| 1 | coghe.view.v2.01 | Tap | Tap final aperture |
| 2 | coghe.view.v2.02 | Climb | Broad ramp, then aperture |
| 3 | coghe.view.v2.03 | Observe | Orbit around opaque L wall, tap visible aperture |
| 4 | coghe.view.v2.04 | Zoom | Inspect fine floor grooves, alternate end gaps around two low baffles, floor exit |
| 5 | coghe.view.v2.05 | One tap | A drives door; exit |
| 6 | coghe.view.v2.06 | Practice | Mirrored A/door; exit |
| 7 | coghe.view.v2.07 | Reverse | Open entrance, enter chamber, return A, exit |
| 8 | coghe.view.v2.08 | Bridge | Slide bridge into its dock, cross, exit |
| 9 | coghe.view.v2.09 | Dependency | A uncovers B; B opens door |
| 10 | coghe.view.v2.10 | Boss | A uncovers B; B docks bridge; cross and operate C; exit |

Boss has no solution hint and retains Home unlock. All ten use one body and preserve 32 particles and 120 Hz physics. No knives, timers, purchased abilities or permanent upgrades.

## Reproducible authoring and build

`GravityBox.Editor.VenomCampaignBuilder.GenerateViewCampaign` authors new scenes, definitions, meshes and art through Editor APIs under `Assets/_Game/Venom/ViewCampaign`. Its output is versioned. Existing scene files are not regenerated.

`CampaignScenePaths()` selects the complete V2 catalog before archived catalogs. Standard `Tools/build-venom.sh`, `Tools/build-venom-android.sh`, Android menu and iOS builder consume that catalog. Explicit historical `--tap`/`--onboarding` variants remain available.

IDs are new because puzzles changed. Progress uses existing `venom.origin.v2`: old completed IDs remain in the list and HomeUnlocked remains true. There is no destructive migration or fabricated completion for new puzzles. The existing selector permits replay/navigation to all entries.

## Mobile budget and evidence

Use shared materials, precomputed studio reflections, one shadow-casting key and batched decorative meshes. No realtime reflection capture, SSAO, depth of field or extra physics particles. Cosmetic detail never adds collision/navigation.

Target from the approved proposal: 60 FPS; frame time p95 <=20 ms and p99 <=33.3 ms; record hitches >50 ms, memory/GC, CPU/GPU when available and a 15–20 minute sustained session. These are targets, not achieved results.

Initial ADB inventory was empty. Author replay/Editor checks do not prove Android FPS, thermal stability, touch feel or novice comprehension. Device, OS, build hash, backend, resolution, quality, warm-up, run duration and repeated samples are required for a mobile result.

## Validation

Full PlayMode and EditMode suites, ten author solutions through screen picking, orbit/pinch cancellation, opaque-cover occlusion, physical gate and bridge behavior, retry, ID/save preservation, default build catalog, Mac player replay, Android ARM64 IL2CPP build. Raw artifacts: `Artifacts/COgheViewV2`. Verification status is recorded separately; this document does not assert a pass before results exist.

See per-level dossiers in `LevelDesign/COghe/ViewV2`.


## Visual changes after physical validation

Level2 uses a broad45° ramp to the raised platform. The original level4 reused it; the later user-authorized redesign replaces it with the floor inspection course described in its dossier. Level7 uses a floor aperture in the far compartment and a return shutter; its two-sided A handle remains available from inside. Levels8/10 slide a bridge along the aisle to align with the opposite bank; the banks and docked deck share a height. Boss C stands on a fixed bank outside the moving floor shutter's sweep. These retain the concepts' lessons while replacing geometry that caused collisions or ambiguous contact.

Spawn clearance is validated for all32 sphere colliders before the first physics tick. The centre is y=-.250m above the floor at-.30m. This avoids initial slab penetration; the earlier centre at-.275m put lower particles on the underside of a thin floor.

## Optional instrumented Android replay

The ordinary APK has no automatic replay. Build with `COGHE_BENCHMARK=1 bash Tools/build-venom-android.sh` to include opt-in diagnostics in an IL2CPP Release APK, then preserve it under a distinct filename. Without the launch extra it plays normally. The instrumented run uses the same touch input, physics and authored solutions, disables persistent progress writes, and restores the real save after finishing.

```bash
adb install -r Builds/Venom/Android/COghe-V2-benchmark.apk
adb shell am force-stop com.gravityboxlab.venom
adb shell monkey -p com.gravityboxlab.venom 1
# Use the resolved activity from the device; do not assume an old UnityPlayerActivity.
adb shell cmd package resolve-activity --brief com.gravityboxlab.venom
adb shell am force-stop com.gravityboxlab.venom
adb shell am start -n <resolved-component> --ez coghe_view_proof true
adb pull /sdcard/Android/data/com.gravityboxlab.venom/files/view-proof Artifacts/COgheViewV2/Android
```

`run.json` records the actual device/API/resolution, buildGUID, all ten results, frame samples, memory and instrumented CPU work. Scene loads, warm-up, screenshots and victory animation are outside sampled gameplay frames. Collection counts cover the replay including screenshots; they are not a zero-allocation gameplay assertion. This short author replay must be supplemented by a sustained15–20 minute session, manual gestures/HUD checks and new-player trials. The historical `coghe_benchmark` flag exercises old levels11–20; use `coghe_view_proof` for this catalog.

## Verified prototype handoff

[Final test report, raw XML/player results and Unity gallery](Verification/COgheViewV2/README.md). Unity checks passed at source commit `bd0ca04`; mobile hardware and novice acceptance remain open.


## Extension 11–30 (28 September 2026)

[V2 levels 11–30: implementation and verification](COGHE_VIEW_V2_EXPANSION.md). Twenty additional scenes implement the approved concept. Full PlayMode 513/513 and EditMode 8/8 passed; both native replay modes completed all 30 scenes. See that report and the per-level dossiers for evidence and device limits.

## Orbit and zoom feedback revision (28 September 2026)

Mrk requested gradual near-edge appearance during orbit and a distinct level4 zoom lesson. Complete outer panes and their metal trim now interpolate opacity over a shallow angle band and settle using unscaled time, including after reversal or pause. Authored transparent URP variants are referenced by scenes for build stripping; full opacity restores original opaque depth and materials. Colliders, picking rules and physical state stay independent. Level4 now has a floor route around two alternating low satin baffles with fine guide grooves, replacing the duplicated ramp. Zoom enlarges the route without gating victory. The final expansion verification report covers these changes.


## Simultaneous cooperation revision (28 September 2026)

Mrk authorized rebuilding23–30 with shared two-input mechanisms and wall exits. Twin spring pulls, held brake/bridge drive and paired pressure lifts now require overlapping work. Terminal catches permit reunion before the exit; the Boss combines two roles at a time. Boundary grip overlays fade with their wall while keeping real adhesion/collision and visible handles. See [current design and implementation](COGHE_VIEW_V2_SIMULTANEOUS.md), [versioned dossiers](LevelDesign/COghe/ViewV2/SimultaneousR2) and [exact verification](Verification/COgheSimultaneous/README.md). Scene/content/save identities and the30-level catalog remain stable.
