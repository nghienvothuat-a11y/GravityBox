# COghe V2 — concept fidelity correction

Requested by Mrk on 28 September 2026 after review of the first V2 player images.
This pass makes the ten scenes closer to the supplied Day Lab concepts. It does
not assign a measured visual-similarity percentage or claim phone qualification.

## Visible changes

- Thicker pale-blue walls, with rounded edges extending outside the playable
  chamber, and a deeper porcelain base. Real apertures remain open.
- Warm tile joints and subtle fixed shading at wall/floor edges, using shared
  512×512 floor and 128×128 wall textures with mipmaps.
- Circular amber thumb grips, porcelain bearings, paired satin-metal guides and
  metal transmission shafts attached to the actual moving mechanism.
- Neutral cream studio, reduced yellow cast, and a lower left-front camera
  (37° pitch / −16° yaw). Lesson 03 keeps its authored observation angle.

The static geometry is combined by material; moving grips remain separate. One
key light casts shadows, fill casts none, reflections use the existing static
studio cubemap. No new runtime simulation, realtime reflection or screen-space
post effect is introduced. The chamber, 32-particle body, 120Hz tick, touch rules,
mechanism travel, save IDs and authored solutions retain their existing behavior.

## Before and after (actual Unity player)

[Before — level 05](../COgheViewV2/Images/05-start.png) ·
[After — level 05](Images/05-start.png) ·
[Before — Boss 10](../COgheViewV2/Images/10-start.png) ·
[After — Boss 10](Images/10-start.png)

All ten start frames are in [Images](Images). These are unedited game captures,
not concept art. The Mac window rendered at **720×1022**, despite a request for
720×1280; exact taller framing is also exercised by the existing automated suite.

## Verification

Base source: `678d0dda3ee6ccd4c8b85e8442ff68d1223d5410`
plus this art change. Unity **6000.3.19f1**, URP **17.3.0**, macOS Metal.
The [source manifest](SourceManifest.json) records the exact tested files.
Unity temporary test fixtures are excluded; their names remain in the manifest.
The [Mac manifest](MacManifest.json) identifies the built player.

- [Compared **24,447 serialized configuration lines**](PhysicsComparison.json) across all ten scenes:
  colliders, rigidbodies, joints, surface patches, rail sliders and tap tasks,
  including local transforms. Data matches the original V2 before this art pass.
  Transient native load IDs are normalized to hierarchy/asset references.
  Original physics mesh files are unchanged in Git.
- Mac development build succeeded with the ten V2 scenes.
- [Accelerated touch replay](FastPlayerRun.json): **10/10 passed**, each with
  32 escaped particles and one fragment; no captured Error/Exception/Assert.
  This run is for solution and screenshot inspection, **not FPS measurement**.
- [Full PlayMode](FullPlay.xml): **441/441 passed**, zero failures/skips,
  including all 20 V2 cases.
- [Full EditMode](FullEdit.xml): **8/8 passed**, zero failures/skips.
- [Real-time touch replay](RealtimePlayerRun.json): **10/10 passed**, all 32
  particles escaped in one body each time; zero captured runtime errors. Mac
  Mac16,10, Apple M4, 720×1022, development player.
  Sampled gameplay: **75.74s** (screenshots excluded);
  per-level FPS **59.26–59.99**,
  maximum per-level p95 **17.24ms**, p99 **32.57ms**,
  slowest sampled frame **33.31ms**, **0** frames over
  33.33ms and **0** over 50ms. These are desktop observations, not an
  Android performance acceptance result. Editor test processes had finished before
  this replay began.

The first verification-tool attempt compared native load IDs and stopped before
building; comparing stable references resolved that false difference. Its raw
snapshots are retained under local `Artifacts/COgheViewArt`.

## Reproduce

Pull the updated source and open the existing V2 scenes; regeneration is not
needed to play. Standard Mac/Android builder menus still select the ten V2 scenes.
The updated local Mac app is `Builds/Venom/macOS/Venom.app`.

To regenerate: **Gravity Box → COghe → V2 → Rebuild concept art and verify physics**.
This regenerates the ten scenes, checks the physical/input configuration and
builds the Mac preview. It does not modify archived campaign scenes.

## Limits

The concept's photographic soft illumination, glass and creature silhouette are
not reproduced exactly. COghe remains the existing deforming simulation; some
level layouts retain physics-tested adaptations from the first V2 delivery.
No new human/novice playtest was performed. The earlier claim of a possible
80–90% aesthetic match was an estimate, not a measured acceptance score for this pass. ADB inventory was empty, so Android
GPU cost, FPS, memory pressure and 15–20 minute thermal behavior remain unmeasured.
The prior Android APK was not rebuilt by this art-only pass; rebuild from this
source to see the new art on a phone.
