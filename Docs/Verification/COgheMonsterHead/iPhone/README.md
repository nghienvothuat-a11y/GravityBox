# iPhone — monster animation and wider flowing tongue

15 October 2026 · `NewGraphic`, base commit `eb14e234` plus the uncommitted liquid-head changes. [Build/source hashes](build-manifest.json) identify the three native binaries. The game was installed on the connected iPhone without clearing the player's save.

## What changed

- Monster skin intersections, normals and ink are reused across adjacent cells. An equivalence test compares the cached and uncached mesh at four phases.
- Mouth samples and eye centre/seam samples are reused. No skin-density, particle-count, physics-frequency or animation-frequency reduction.
- Tongue width is exactly doubled; thickness stays the same. The first widened motion was too restrained for Mrk, so the final build restores the earlier travelling-wave rhythm, with an anchored root, gentle gravitational droop and a following tip. The final Home/Menu videos in the parent gallery show this revision.

## Conditions and results

iPhone 13 Pro Max (`iPhone14,3`), Apple A15 GPU, iOS 18.3.2, Unity 6000.3.19f1. Release IL2CPP / Metal, **1284 × 2778**, 60 FPS target, unchanged saved quality settings. `COGHE_MOBILE_BENCHMARK` enables explicit instrumentation; no Unity Profiler was attached. Native 3D Home is zoomed in with its normal UI. One complete act warms buffers/shaders before measurement; screenshot work is excluded.

| Metric | Before optimization | Optimized soak, first wider motion | Final flowing tongue |
| --- | ---: | ---: | ---: |
| Repeated acts | 3 | 90 | 20 |
| Sampled duration | 30.62 s | 918.14 s | 204.05 s |
| Frames | 1,837 | 55,084 | 12,242 |
| Mean FPS | 60.00 | 60.00 | 60.00 |
| p95 frame | 17.11 ms | 16.81 ms | 16.96 ms |
| p99 frame | 17.22 ms | 16.95 ms | 17.17 ms |
| Worst frame | 17.63 ms | 32.41 ms | 25.22 ms |
| Frames >33.334 / >50 ms | 0 / 0 | 0 / 0 | 0 / 0 |
| Mean skin CPU | 3.646 ms | 2.874 ms | 2.880 ms |
| Mean face CPU | 0.799 ms | 0.623 ms | 0.636 ms |
| GC collections | 5 | 127 | 33 |
| Unity tracked memory, start → end | 79.69 → 79.96 MB | 79.69 → 82.05 MB | 79.89 → 82.42 MB |

All 90 soak cycles and all 20 final cycles have complete time coverage. The final console contains no background/foreground transition after the initial launch, no runtime error and no memory warning. The phone was charging; battery went from 75% to 80% during the soak and stayed at 80% in the final run. The baseline and final console reported thermal state **Fair** at startup. No temperature-in-degrees or continuous thermal trace was collected.

For the same first three complete measured cycles, the final build reduced mean skin CPU from **3.646 to 2.900 ms (20.4%)**, and face CPU from **0.799 to 0.632 ms (21.0%)**. These timers are separate: the skin timer ends before the face callback. This is one comparison on one phone, not a multi-device statistical study. The baseline already reached the 60 FPS cap.

## Interrupted runs and limits

Two earlier short after-runs had native background/foreground callbacks during their third cycle. One lost about 2.2 seconds of sampling; the other recorded a 1.665-second interruption. Mrk confirmed that the phone was untouched. Their cause is **not established**; they are not counted as uninterrupted performance passes or blamed on the animation. The later soak has complete coverage with no multi-second gaps; the final run also has a clean native lifecycle log. This does not prove that the earlier system/app interruption can never recur.

The long soak's console did not forward Unity messages, so its continuity was checked from all per-cycle samples, not from an asserted absence of lifecycle callbacks. A short, filtered system-log read occurred roughly midway through that soak; it is included in the recorded samples. The final 20-cycle run used no concurrent device queries or system-log streaming.

Cold launch and the first un-warmed act, independent GPU time, OS RSS, all gameplay levels and other devices were not profiled here. A startup memory warning occurred before the baseline warm measurement. Tracked Unity memory endpoints are not proof of zero leaks or an OS memory budget. The videos/stills use a fixed 30 FPS capture clock and are visual evidence only.

## Evidence

- [Before](before.json), [15-minute soak](soak.json), [final waveform](final.json), [matched-cycle comparison and final coverage](comparison.json).
- Compressed raw frame CSV: [before](before-frames.csv.gz), [soak](soak-frames.csv.gz), [final](final-frames.csv.gz).
- Excluded interrupted runs: [first](interrupted-after-1.json), [second](interrupted-after-2.json), with their compressed CSVs in this directory.
- [Play Mode results](tests.json): eight distinct checks passed across the optimization work. Three relevant checks were rerun after the final waveform change and passed. The initial incorrect tongue-width assertion and its corrected expectation are recorded rather than hidden.
- Native iPhone captures from the final binary: [mound](iphone-mound.png), [head](iphone-head.png), [tongue](iphone-tongue.png), [return](iphone-return.png). Resolution is 1284 × 2778; inspected separately from the timing run. The first screenshot attempt failed because Unity prefixed the sandbox path twice. Creating the corresponding temporary output directory allowed a clean capture retry with the same binary; the measurement files and player save were unaffected. Future use of this fixture's screenshot path on mobile needs that accommodation or a relative screenshot path.
- Native build, installation, console and raw-capture logs: `Artifacts/COgheMonsterIPhone20261015/` (ignored). Detailed device identifiers remain there, outside the published report.

The explicit one-shot measurement request is consumed on launch, with saving disabled. After the final captures, COghe was successfully launched normally on the iPhone at 14:03 local time. See the parent [reproduction instructions](../README.md).
