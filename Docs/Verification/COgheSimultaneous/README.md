# COghe V2 simultaneous cooperation — levels 23–30

28 September 2026. Rebuilt eight scenes with continuous two-body inputs, visible shared outputs, wall handles and real wall exits. The terminal catches let both fragments leave their jobs and reunite inside before exiting. See [mechanism/layout notes](../../COGHE_VIEW_V2_SIMULTANEOUS.md), [eight versioned dossiers](../../LevelDesign/COghe/ViewV2/SimultaneousR2) and the [actual Unity screenshot gallery](Gallery.md). R1 evidence remains in its original directory.

## Final results

| Check | Result | Evidence |
| --- | --- | --- |
| Full PlayMode package | **521/521**, zero failures or skips | [FullPlay.xml](FullPlay.xml), run17, 431.742 seconds |
| Full EditMode package | **8/8**, zero failures or skips | [FullEdit.xml](FullEdit.xml), run17 |
| Native Mac accelerated author route | **30/30**, zero reported runtime errors | [FastPlayerRun.json](FastPlayerRun.json), build16 |
| Native Mac normal player loop, changed levels 23–30 | **8/8**, zero reported runtime errors | [RealtimePlayerRun.json](RealtimePlayerRun.json), same build16 |
| Native live cooperation captures | **11 outputs** across all eight changed scenes | [Gallery.md](Gallery.md), JSON cooperation records |
| Delivered source/binary comparison | **All gameplay, scenes and assets match built inputs**; only one test fixture differs | [SourceManifest.json](SourceManifest.json), [BuildSource.json](BuildSource.json), [MacManifest.json](MacManifest.json) |

Every native route finishes with **32 escaped particles and one connected body**. Every cooperation record has two distinct tissue groups, two fragments and positive measured overlap. All 11 accelerated capture records also have positive instantaneous effort on both inputs. Normal-loop brake workers can have zero instantaneous effort on a sampled physics tick while still in their active motor task; these samples are not described as continuous positive-force measurements.

The 521 tests comprise the prior 513, minus nine superseded R1 cooperation cases, plus 17 new R2 cases. Retained full-route and reset tests exercise the replacement scenes. Earlier failures and their fixes are recorded in [DiagnosticHistory.json](DiagnosticHistory.json); they are not counted as passing final runs.

## Behavior exercised

- Whole-body A→B and B→A cannot catch the shared shutter. Two independent fragments can operate in reverse order.
- Selecting a partner preserves the first held command; a new destination releases the commanded actor. A held body remains eligible for normal proximity merging.
- Missing pressure returns an uncaught shutter/lift. A tissue obstruction stops its return without granting a catch or exit. Missing brake release stops B, which resumes its same task when A returns.
- A physically unequal cut can supply both roles. An early independent wall exit loses with the existing reunion message. Retry clears catches, task ownership and pipe transit; pause stops physics.
- Level28 retains its first caught output while T selects the second. Late C/D inputs stay locked until the preceding mechanism reaches its real catch.
- Near-wall grip strips fade with their wall and permit the first touch through the cutaway while retaining their physical support. Real held feet, orbit restoration and unchanged collision geometry are covered by the new regression case.
- All changed scenes are exercised through their complete routes, including tubes, return bridges, ramps, lifts, reunion and wall exits.

The blade, topology, 120Hz timestep, adhesion and win/loss rules are unchanged. New inputs use bounded hand effort and real planted feet. Decorative links, pulleys and pressure rods have no collisions or input ownership; the builder checks physics/input state around art generation.

## Exact source and player state

Base commit `2c76105f68d1ce1636f0177b192564e8f5cd2cbc` plus the changed inputs in this report. Unity **6000.3.19f1**, URP **17.3.0**, Input System **1.17.0**, Test Framework **1.6.0**. Native build GUID **`de4a91fe0f5e417a9e1cd71ff3617a6e`**, actual player window **720×1022**. The development bundle is `Builds/Venom/macOS/Venom.app` in the preview worktree. All 340 bundle file hashes were checked after both replays and remain identical to [MacManifest.json](MacManifest.json).

[BuildSource.json](BuildSource.json) records 12,347 actual inputs after build16 and before its full suite. [SourceManifest.json](SourceManifest.json) records 12,345 delivered inputs after final tests and restoration of known test side effects. Two pre-existing local files, `ProjectSettings/PackageManagerSettings.asset` and `ProjectSettings/URPProjectSettings.asset`, are recorded in the actual build snapshot but excluded from this change. Temporary Unity InitTestScene files and documentation are outside these input manifests.

No gameplay, scene, art-source or bundle edits occurred during the final native runs. Full PlayMode17 changes only the setup of the held-body merge test: it issues a public surface command because touching the now-visible partner correctly selects that actor. No tissue teleport or forced topology is used in that case. [TestSourceDuringRun.json](TestSourceDuringRun.json) records source immediately after that test launch; the prelaunch comparison identified exactly this fixture and a previous test side effect removing the mint material emission keyword. The prelaunch snapshot failed its assertion and was not written; this file is explicitly an in-run snapshot.

Known Unity test side effects changed the three archived PhysicsLab glass materials' source blend and Quiet mint light's emission keyword. Their exact diffs were inspected and restored to the build baseline after Full PlayMode17, before Full EditMode17. The final hash comparison confirms the test fixture is the sole difference from the native build inputs. The built Mac materials were never changed. This audit distinguishes the actual test input state from the delivered state.

## Reproduction

Run from a checkout with the exact Editor installed; use one Editor per project. Generate the eight scenes and Mac development player with:

```sh
/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath "$PWD" -executeMethod GravityBox.Editor.VenomCampaignBuilder.GenerateViewExpansionAndBuildMac -coghe-view-levels 23,24,25,26,27,28,29,30 -quit -logFile Artifacts/COgheSimultaneous/build.log
```

Run each complete test platform sequentially. PlayMode requires graphics:

```sh
/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath "$PWD" -runTests -testPlatform PlayMode -testResults Artifacts/COgheSimultaneous/FullPlay.xml -logFile Artifacts/COgheSimultaneous/FullPlay.log
/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode -testResults Artifacts/COgheSimultaneous/FullEdit.xml -logFile Artifacts/COgheSimultaneous/FullEdit.log
Builds/Venom/macOS/Venom.app/Contents/MacOS/Venom -coghe-view-proof Artifacts/COgheSimultaneous/Fast -coghe-view-fast -coghe-view-first 1 -coghe-view-last 30 -coghe-proof-quit
Builds/Venom/macOS/Venom.app/Contents/MacOS/Venom -coghe-view-proof Artifacts/COgheSimultaneous/Realtime -coghe-view-first 23 -coghe-view-last 30 -coghe-proof-quit
```

Inspect the XML and JSON results, not only process exit codes. Native proof actions use Input System touch events for world actions and orbit/pinch; fragment selection uses the public selection API. The route harness is author-only and gameplay does not read its solution.

## Limits

The normal-speed 23–30 run overlapped the Editor regression run. It is completion/input evidence; its frame samples are **not an isolated performance benchmark**. Full normal-speed replay of unchanged 01–22 was not repeated in this revision; the 30-level accelerated route and complete package tests cover the retained content. Automated portrait tests cover 720×1280 and 720×1612; the native JSON records actual window dimensions.

Mac author routes do not certify phone performance, thermal stability, physical touchscreen comfort or a first-time player's understanding. No APK/device installation was requested or performed. The finite scripted drive is a gameplay mechanism, not a fluid-pressure solver.
