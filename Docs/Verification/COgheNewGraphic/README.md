# NewGraphic — verification and visual revision

Status: **visual direction rejected; experiment stopped**. User requested a return to glass on 29 September; see [level 01 glass pilot](../COgheGlassPreview/README.md). On 29 September the user reported that the installed Blender profile did not match the approved concept. Gameplay verification below is not visual acceptance.

## Installed first pass

- APK SHA-256: `3ec804d377908894266961ed4cff439773b570db7efba679b4af7d0200bd72ff`.
- OPPO CPH2591, Mali-G52 MC2. Normal player loop, real InputSystem touch replay; screenshots excluded from frame samples.
- One uninterrupted Blender replay completed 10/10, each with all 32 particles escaped as one fragment. Approximately 168 seconds, **not a 15-minute thermal test**.
- The preceding Current replay changed profile during execution and had destroyed-object references. It is invalid as an A/B baseline; no comparative performance claim is made.
- Accelerated Mac replay passed 10/10. It is not performance evidence. PlayMode suite passed 100/100 before the visual revision.

| Level | Average FPS | p95 ms | p99 ms |
|---|---:|---:|---:|
| 01 | 58.4 | 16.6 | 33.2 |
| 02 | 58.4 | 16.6 | 33.1 |
| 03 | 53.4 | 33.1 | 33.2 |
| 04 | 55.8 | 33.1 | 33.2 |
| 05 | 55.0 | 33.1 | 33.2 |
| 06 | 54.5 | 33.1 | 33.2 |
| 07 | 52.0 | 33.1 | 33.2 |
| 08 | 54.8 | 33.1 | 33.2 |
| 09 | 51.3 | 33.1 | 33.2 |
| 10 | 49.5 | 33.2 | 33.2 |

Raw report: [OPPO first pass](oppo-first-pass.json). [Visual comparison](review.html) shows the concept, first pass and revised native Mac render. The A/B APK includes both asset sets; memory is not an isolated shipping-build comparison. GPU frame timings were not captured.

## Fidelity gaps found in actual renders

- Separate narrow rails read as a scaffold instead of the concept's continuous rounded equipment enclosure.
- Flat internal partitions and shutters lack manufactured face detail.
- Floor texture/joints and panel contact shading are not visible enough; investigate surface rendering before adding post-processing.
- Concept laboratory context is absent.
- Gameplay layout remains the approved V2 layout; decorative props in the concept are not a new puzzle specification.

## Revised presentation — 29 September

- Replaced the narrow joined rails with a continuous rounded Blender enclosure. Pale-blue panels, inset porcelain shutter faces and small fasteners give the mechanisms a clearer equipment form.
- Fixed procedural material texture interpolation that had flattened floor joints and panel grooves. Thin surfaces render on both sides; the exit annulus no longer duplicates coplanar faces or overlaps the old outline.
- Adjusted key/fill light and added a distant laboratory background rendered from the editable Blender scene. The background is a 768×320 texture, not runtime depth of field.
- Added camera framing allowance for the wider casing. Gameplay geometry, masses, routes and collider snapshot remain unchanged across all 30 V2 scenes: `0f57d8866c1e3cd37b62dcf06fdd61cc4c5cb821e0c758e1fb4bfa533a495356`.
- Evaluated Blender kit: 66 modules, 47,060 triangles across the unique exported library. This is not a per-frame triangle or draw-call measurement.

### Verification scope

- 24/24 PlayMode input/camera/route tests passed after the framing and profile changes. Final material/annulus corrections followed those tests.
- Final native Mac author replay passed 10/10, all 32 particles escaped in one fragment. It uses accelerated simulation and supplies no FPS evidence. Report: [Mac replay](mac-author-replay.json).
- The original 100/100 suite passed before this visual revision; it is not presented as a full-suite rerun of the final files.
- All ten final Mac screenshots were visually inspected. The comparison page uses real player captures, not generated mockups.

### Remaining visual differences

The revision is awaiting user review. Concept illustration proportions, soft contact shading and small manufactured details are not fully reproduced. Some rear-wall/floor contact edges still look jagged on the mobile shadow settings. The authored V2 layout and creature size are preserved rather than copying the illustration's unrelated LAB 07 arrangement.

### Interrupted benchmark runs

The earlier manual A/B interruptions and the subsequent `AndroidFidelityClean/01-current` run are excluded from a complete A/B comparison. The latter changed profile after level 7 and lost replay scene references (6/10 completed), even after the user agreed to leave the device alone. The cause of that later switch is not established; it must not be attributed to user input without evidence. The benchmark now disables HUD actions while it owns the scene and restores them when it ends. Normal play retains all controls.

The revised art APK `6d7087e6d05313b53482bf89e6618ea6f17943817c358de813e82e395306e2c3` completed a separate uninterrupted Blender replay: 10/10 passed, no logged exceptions, 53.0–58.4 average FPS per level. See [revised art device replay](oppo-fidelity-revised.json). The next APK only adds the benchmark HUD guard; visual assets are unchanged.

Final paired device measurements are recorded below after the protected replay completes.


## Final protected OPPO A/B — completed

Installed APK SHA-256: `3a5045ab847f6b3c22cc369e2b69b46aafa85e62f183f41d07c847a283356e07`.
Unity 6000.3.19f1, IL2CPP ARM64 Release with benchmark instrumentation; OPPO CPH2591, Android 15, Mali-G52 MC2/Vulkan, 720×1612. Same APK, Current then Blender. Each replay uses the normal player loop and actual InputSystem touch events; no accelerated simulation. HUD actions are disabled only during the automated replay. Screenshots are excluded from frame samples.

**Both profiles passed 10/10, with no logged errors; each level escaped all 32 particles in one fragment.** Raw reports: [Current](oppo-current.json), [Blender](oppo-blender.json).

| Level | Current FPS | Blender FPS | Current p95 ms | Blender p95 ms | Blender p99 ms | Blender frames >50 ms |
|---|---:|---:|---:|---:|---:|---:|
| 01 | 57.2 | 57.7 | 16.6 | 16.7 | 33.2 | 0 |
| 02 | 58.4 | 58.6 | 16.6 | 16.6 | 33.1 | 0 |
| 03 | 56.2 | 55.2 | 16.6 | 33.1 | 33.2 | 1 |
| 04 | 57.5 | 57.6 | 16.6 | 16.6 | 33.2 | 1 |
| 05 | 57.5 | 57.9 | 16.7 | 16.6 | 33.2 | 0 |
| 06 | 58.2 | 57.8 | 16.6 | 16.7 | 33.2 | 0 |
| 07 | 56.7 | 52.9 | 33.1 | 33.1 | 33.2 | 0 |
| 08 | 57.3 | 56.6 | 16.7 | 33.1 | 33.2 | 0 |
| 09 | 57.0 | 55.5 | 33.1 | 33.1 | 33.2 | 0 |
| 10 | 54.4 | 52.4 | 33.1 | 33.1 | 33.2 | 0 |

Across captured gameplay samples, weighted FPS is 56.80 Current and 55.75 Blender (about 1.9% lower). Combined p95 is 33.12 vs 33.14 ms. Both profiles fall short of the proposed sustained-60/p95≤20ms target; in particular Blender levels 03 and 08 cross from a ~16.6ms to ~33.1ms per-level p95. Do not call this a performance acceptance solely because the overall average is close.

Current has 4 frames >50ms, maximum 198.88ms; Blender has 2, maximum 198.93ms. These spikes remain reported, not removed as outliers. CPU/render/GPU timing attribution was not collected, so no claim is made about their cause.

PSS sampled every ~30 seconds peaks at 352.4 MiB Current and 369.8 MiB Blender (~4.9% higher). This is a sampled maximum, not a continuous memory peak. Both asset sets ship in the same APK. USB power was connected, battery 100%; battery temperature was 41→41°C for Current and 41→39°C for Blender. This is battery temperature, not a SoC thermal sensor.

Scope: one protected A/B pair, approximately 178+177 seconds after repeated earlier runs. Three controlled pairs, a dedicated 15–20-minute soak, GPU/draw-call profiling and novice-user testing remain outstanding. Final Mac screenshots predate only the benchmark HUD guard; their art assets are identical to this APK. The 24-test suite and Mac replay are scoped above, not relabelled as reruns after the guard.

Normal launch after benchmarking was verified on OPPO at level 01. The A/B button was tapped in both directions and remained usable after the guard was released. The device was left on Blender, level 01. [Installed screen](oppo-ready.png).
