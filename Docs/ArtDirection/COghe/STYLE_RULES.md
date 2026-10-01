# COghe — Day Lab design rules

Status: **approved direction, consolidated 29/09/2026**. Version 2.0 — Glass C / Spatial.

## Current authority and scope

For **Spatial Pilot 01–10 and the planned Spatial 11–30**, the approved baseline is
**Glass C with printed circuits and quiet satin guides**. Use the actual Unity/Mac
[ten-level circuit review](../../Verification/COgheSpatialCircuits/README.md)
and its [before/after gallery](../../Verification/COgheSpatialCircuits/review.html)
as the presentation reference. The later Glass C and Spatial sections below take
precedence over the original Day Lab palette and the opaque V2/Blender proposals.
The 20 new illustrations describe layouts; they do not replace the material,
lighting or physical rules with whatever an image generator happened to draw.

| Element | Binding direction for Spatial |
| --- | --- |
| Scene and creature | Clear glass enclosure, thin aluminium frame, calm warm lab background; glossy dark liquid COghe with its existing silhouette and tendrils. No Blender dependency. |
| Number plate | Small ivory plastic plaque adhered directly to the glass, below the upper front-left rail; two-digit level number. No floating sign or bracket on the frame. |
| Controls and outputs | Muted blue A and muted coral B on handles, labels and narrow receiving bands; matching letter/glyph plus matching colour show the relationship. At most two circuit colours per visible work area; A1/A2 reuse A. |
| Bodies and supports | Warm pearl/ivory equipment casings; low-contrast satin blue-grey rails and supports. Do not colour an entire door or mechanical frame as a signal. |
| Connections | Thin printed traces with chamfered corners and small terminals, flush to a real floor, work surface or glass pane. These are presentation only. Actual load-bearing pulley cables remain visible mechanical cables. |
| Surface semantics | Lavender satin means no active grip; muted cyan distinguishes transfer tubes; a thin flush mint ring identifies the final exit. Keep these separate from A/B circuit colours. |
| Light and depth | Glass C lighting/background and baked reflection assets; one shadow-casting key, restrained fill. No per-mechanism realtime lights, realtime reflection capture or heavy screen-space effects. |
| Animation | Read real contact, force and mechanism state. Keep control targets legible during movement; decorative motion never opens a gate or changes the navigation graph. |

Material assets and the Spatial art builder are the source for exact tuning;
do not guess colours from screenshots. Presentation changes must pass the physics
snapshot comparison and portrait visual checks described in the circuit review.
This approval fixes the visual direction, not a claim that the next 20 levels have
been built or that their mobile performance has passed.

The original sections below remain the baseline for archived Origin scenes unless
that catalog is explicitly included in a migration. In new Spatial scenes, the
quantum machine Q replaces the historical blade design; see the
[design handoff and constraints](../../LevelDesign/COghe/SpatialNext20/IMPLEMENTATION_HANDOFF.md).

The original Day Lab visual reference is the Unity level 07 shipped at commit `8718849`, with
[runtime images](Runtime/README.md). The Day Lab concept is supporting inspiration;
its photographic lighting and proportions are not a requirement to change gameplay.

## Product interface — approved 30/09/2026

Applies to the Spatial 50 product flow. See the [approved mockups](ProductUI/review.html)
and [native Unity verification](../../Verification/COgheProductUI/README.md).

- Main Menu features the existing **live 3D COghe**, using its normal surface and
  life animation. No portrait sprite substitute or redesigned character.
- English is the default. Gameplay uses short icons; labels remain where they
  help explain pause settings or the three main-menu destinations.
- Warm ivory panels, dark blue-grey Manrope type and restrained teal actions.
  Use one shared Lucide outline icon atlas; touch targets at least 48 logical px.
  Respect device safe areas and the actual occupied control rectangles.
- No level selection in Product. Play resumes the first incomplete stable level
  ID. Keep existing saves and Home unlock; do not unlock Home to render the menu.
- Restart/Home/Menu/Help/Music/Sound belong to Pause. Confirm before discarding
  the current puzzle attempt. Pause blocks all world input and physics. Music
  and effects are separate persisted preferences.
- Contextual fragment/zone/release controls appear only when needed. UI controls
  must not also issue a command to the glass underneath them.
- Victory uses the existing close-up and dance, then proceeds automatically;
  the last level ends on the completion screen. No forced Next tap.
- Main Menu/Home presentation reuses the creature, background and lighting.
  Keep menu room colliders separate from the authored puzzle; restore the puzzle
  on Play. Never use decorative movement to alter puzzle state.
- Full furniture shopping is a later Home feature; current Home retains Feed and
  Play. It must not show a working purchase flow without the corresponding system.

## 1. Identity and hierarchy

- A bright, warm, calm space research laboratory. The player guides a curious
  creature and builds a relationship with it. Keep equipment clean and readable.
- Write the name **COghe** exactly. The legacy app/bundle/save identifiers may remain
  Venom; changing branding must not silently reset progress.
- First read: COghe. Second: relevant mechanism, slippery surface and exit. Third:
  mounts, labels and the laboratory setting. Decoration must not hide actions.
- Retain the existing dark, asymmetric liquid body, tendrils, transient lobes and
  gravity response. Do not add eyes, teeth, ears or a permanent head.
- Style (approved by Mrk 01/10/2026, which amends the earlier "dark body, no
  costumes" rule): the player may inject up to four inks into the liquid and
  dress COghe in one hat plus up to two little things floating inside. The default
  stays the dark body. Inks travel with the particles and change only the skin's
  material; wardrobe is presentation only (no colliders, forces or puzzle state)
  and must never hide a mechanism, the exit or the creature's silhouette. Item
  icons follow the Home item icon style (Codex, `Resources/COgheStyle/Icons`).
- Build assets in Unity for this phase. Do not introduce Blender or purchased asset
  dependencies without a new task requiring them.

## 2. Material language

Values below are Unity material Base Color inputs (RGB, 0–1), not final screenshot
pixel colors. Keep shared materials in `Assets/_Game/Venom/Art/DayLab`.

| Role | Base color | Metallic / smoothness | Visual rule |
| --- | --- | --- | --- |
| Equipment casing | .91, .88, .81 | .08 / .42 | Warm porcelain, small rounded edges |
| Structural trim | .65, .74, .77 | .60 / .61 | Narrow aluminium strips; no mirror-chrome cage |
| Sockets/feet | .065, .10, .12 | .55 / .56 | Dark, small, recessed accents |
| Pressure sensor housing | .69, .77, .79 | .25 / .48 | Pale blue-grey beneath the porcelain bezel; no broad black plate |
| Cutting blade | .88, .91, .94 | .58 / .78 | Silver steel, flat face normals, subtle 128×128 brushed grain |
| Honed edge | .95, .97, 1 | .66 / .88 | Narrow polished silver edge, distinct from the dark clamp |
| Movable resin | .84, .61, .32 | .03 / .52 | Amber; warm, solid, softly rounded |
| Tray | .77, .83, .84 | .08 / .38 | Pearl blue; reveal the creature when viewed from below |
| Active indicator | .32, .70, .59 | .10 / .60 | Quiet mint; emission .10, .27, .19 |
| Labels | .10, .17, .21 | 0 / .30 | Readable blue-grey, few words |
| COghe | .022, .030, .035 | .32 / .80 | Dark wet body with broad restrained highlights |
| Chamber glass | .64, .78, .82 | Custom glass shader | Clear and quiet |
| Fixed rigid plastic divider | .91, .56, .20 | Transparent, alpha .36 | Amber panel, visible thickness and amber edges |
| Slippery coating | .43, .37, .76 | Custom glass satin shader | Lavender, distinct from the blue floor and amber plastic |
| Transfer pipe | .16, .55, .67 | Transparent, alpha .32 | Cyan bore, porcelain collars and blue metal gaskets; keep flow visible |

- Slippery areas need a visible boundary and satin grain, not only a color change.
  The clear grip island must match the real `HoleCentre` and `GripRadius`.
- Every final round exit uses the shared quiet-mint material on a **3.2 mm flush
  outline**. It follows the authored aperture exactly, casts no shadow and adds no
  collider, light, raised lip or input surface. Keep the physical aluminium lip
  secondary so the exit remains readable without looking like a neon portal.
- Movable lids must read as separate objects. Transparent lids use a thin amber
  edge; do not make a solid plug where the simulation uses an open-bottom cap.
- Pressure pads use amber circular caps, a pale blue-grey socket, porcelain bezel and A/B
  labels. Caps depress 6 mm under measured tissue load and rise when released.
  The mint status light follows the real 12 g activation threshold. Visual travel
  does not change the authored sensing area or collision surface.
- A visual light must never imply a latch or button is active before simulation says so.
- Blades use brushed steel and a distinct honed cutting edge; covers/rails use
  the same porcelain/amber language. Avoid horror effects.
- World-space labels must depth-test against opaque equipment; text on a sensor
  cannot show through its closed cover. Keep font-atlas updates in presentation.

## 3. Geometry and physical truth

- One Unity unit is one metre. Preserve original collider geometry, contact
  materials, mass, transforms, articulation, route metadata and apertures.
- Decorative meshes have **no collider**, no surface-navigation registration and
  no independent input interception. Their parents follow the real mechanism.
- Small edge bevels are visual only. Do not shrink a usable gap, add a raised exit
  rim or place an opaque chassis beneath a real floor opening.
- Rotatable boxes use open frame rails. Keep the floor visually clear when it
  faces the camera from outside; raycasts still hit the original surfaces.
- A stationary table must sit outside every rotation of the box. Do not attach a
  world-direction shadow to a rotating face. Contact shadow approximations are
  allowed on the fixed level 07 floor; rotating mechanisms use real light shadows.
- Pipe collars, sphere seams and mounts stay outside the playable volume and must
  leave the actual bores visible. Animate flow with the existing moving tissue.

## 4. Light, camera and interface

- Use the approved warm key (1.10 intensity, RGB 1/.97/.91) and cool fill (.45,
  RGB .81/.90/1). Only the key casts shadows; use subtle strength .20.
- Use the shared precomputed 128-pixel-per-face studio cubemap. No realtime
  reflection capture, compulsory bloom, refraction, SSAO or depth-of-field.
- Keep camera/control choices appropriate to each level. Add framing clearance
  when needed; do not change the physical pivot to make an image prettier.
- Hide decorative trim that blocks close inspection. Victory keeps the established
  close camera, hides scenery and uses the three existing celebration animations.
- Bright HUD, dark blue-grey text, muted teal selected state. Keep titles and
  controls out of the playable chamber. Preserve retry, pause, zoom, fragment
  selection, Collection access and its food/play actions.
- Boss 10 uses a more elaborate version of this same lab kit. It has **no tutorial
  or solution hints**. Preserve the Collection unlock and the merged-body win rule.
- Command feedback uses quiet mint: an expanding touch ring, a brief outline of
  the actual picked face and a smaller destination ring attached to that surface.
  Inset the outline from opaque frame trim. A short, faint wash can identify an
  unperforated pane; never paint a quad across an actual opening.
  Fragment selection uses one mint arrow above the selected body. Amber arrows
  identify the first exit, the level 07 crate and the level 08 roof departure.
  All cues disappear during victory, failure and Home. A locked-rotation icon is
  control feedback, not a Boss solution hint.
- The rotation legend occupies the strip below the chamber. Overview framing from
  level 03 reserves space for it; zoom still follows the selected creature.

## 5. Architecture and cost

- One editor art builder applies the kit to existing scenes. It is safe to rerun,
  and the campaign generator calls it so regenerating levels keeps the art.
- Reuse shared materials/cubemap. Store generated per-level mesh batches separately
  so rebuilding one scene cannot overwrite another scene's mesh references.
- Combine static meshes by material; keep moving lids/blades/props separate.
- Store per-surface appearance data separately from physics. Visibility code may
  modify renderers/property blocks; it must not move physics bodies or change wins.
- Keep the existing 32 particles, mesh density, animation and simulation timing.
  Quality settings must not change puzzle physics between devices.
- Validate the look in real Unity renders, not concept images. Measure mobile
  performance on hardware before making FPS claims.

## 6. Boss 10 mechanism revision

User-authorized on 16/09/2026: lock box rotation for the entire Boss. The metal
blade parks high, starts a four-lamp warning when actual tissue enters the marked
sensor area, waits one simulation second, then drops under world gravity. Players
can change the creature’s position during the warning. Cut actual intersected
bonds at impact; do not centre the body or force equal fragments. A missed strike
is valid. Lift the blade back, then require the sensor area to clear before rearming.
Camera 37° pitch / 15° yaw keeps the A pad visible beside the blade.

This revision intentionally changes only Boss rotation, framing and blade rail
configuration. Presentation-only geometry rules continue to apply to levels 01–09.

Additional user-authorized level 09 fix: the loose lid has 14 mm walls, a mass of
180 g, non-bouncy contact, more solver iterations and speculative continuous
collision detection. Its amber resin shell shows the real wall thickness and an
open bottom. Contact recovery against the rotating chamber prevents a missed wall
collision from ejecting this oversized lid; do not apply this constraint to props
which are designed to fit through an opening. The lid remains a free dynamic body
and must fall away under gravity when the chamber is inverted.

## 7. Required checks for art changes

1. Compare collider/Rigidbody/joint data and gameplay definitions before/after.
2. Inspect all affected levels in portrait view, including the rotating floor,
   clear grip ring, tube flow, falling cap and Boss mechanisms.
3. Run the existing relevant PlayMode solutions. For a campaign-wide change, run
   the complete Origin suite, including cut/merge failure and Collection unlock.
4. Inspect zoom, retry, victory framing and fragment/Home UI. Keep saved before/after
   images and an honest record of checks and limits.
5. Build the macOS prototype so the user can assess the result. Build APK only when
   the user asks for it. Update README links when the visual workflow changes.

## 8. Level 15 pipe maze readability

Use teal transparent bores, thin copper longitudinal ribs and porcelain end
couplings to distinguish the maze from the blue-gray chamber glass. Keep junction
windows clear enough to see COghe and the available branches. Ribs follow the
actual trimmed bore mesh; never invent a visual connection between crossing pipes.
Decorative geometry has no colliders or input targets and is batched by material.
Rebuild through `Gravity Box → COghe → Rebuild Day Lab · Pipe Maze 15`; the
expansion art generator also applies this treatment automatically.

## 9. Assembly bridge (replacement level 16)

User-authorized redesign on 17/09/2026 replaces the sphere and hoses with three
manually assembled bridge modules. The old geometry is not a constraint for this
replacement. Use amber gripping decks, lavender transparent slippery sidewalls,
low metal handles and porcelain socket corners. Show only one usable handle per
module; a tap on the deck remains a locomotion command. Socket lamps read the
real end catch and must extinguish after a reverse pull. The art never moves a
bridge, provides a hidden walking surface, or gates the exit.

## 10. Campaign camera framing (17/09/2026)

Fit the chamber's authored static bounds to the portrait area between title and
rotation/fragment controls, including safe-area clearance. Preserve each level's
camera heading and the physical rotation pivot. Do not fit to every renderer each
frame or allow a moving prop to resize the overview. Sphere framing uses its true
radius. Follow mode tracks the selected creature smoothly; selecting an authored
camera zone inspects that compartment without changing puzzle state. Zones are
optional data in the campaign definition, currently used by 08, 19 and 20. Boss
zones are viewpoint selectors only, never solution hints. Keep a visible overview
return and prevent selector taps from issuing movement/rotation commands. Use the
production camera path for portrait verification captures. See
[framing and verification](../../COGHE_CAMERA.md).

The studio table must cover the entire viewport throughout rotation and follow,
including tall mobile aspect ratios. Keep its authored height and shadow space
fixed; expand only the collider-free backdrop mesh. Orthographic camera depth and
far clipping may adapt without changing the chamber's screen projection. Verify
all four corner rays reach the tabletop within its edges and clipping range.

## 11. Authorized view-only V2 (28 September 2026)

For the ten new V2 scenes, Mrk's supplied concept direction supersedes the archived rotating cage layout: opaque pale-blue far panes, warm porcelain floor/base, rounded amber handles and mint mechanism state. Incoming exterior panes and their trim form a camera cutaway; their collision and adhesion remain intact. Interior opaque walls/lids still obstruct picking. Drag orbits worldY at fixed pitch, pinch zooms about the two-finger midpoint, and Overview resets the view. V2 has no creature-follow button.

`COgheViewArtBuilder` and `COgheViewPresentation` own this presentation. Decorative geometry is batched by material; no extra colliders, realtime reflections or heavy post-processing. HUD uses the device safe area. Archived scenes retain their existing art/control contract. The concept is a direction, not an asserted visual-similarity score.

### V2 concept fidelity pass (28 September 2026)

Mrk requested a closer match to the supplied first-ten-level concepts after
reviewing the V2 player screenshots. `COgheViewStudioBuilder` now adds outward
blue wall thickness and rounded edges, a deeper porcelain tray, softly shaded
tile joints, circular amber thumb grips, paired satin-metal guides and a neutral
cream studio. Most V2 scenes use a 37° pitch / −16° yaw; lesson 03 retains its
hidden-target viewpoint. These are presentation changes, not permission to
reshape the validated puzzle or move its colliders.

The V2 kit uses its own materials and two shared generated textures (512² floor,
128² wall, both with mipmaps). The studio backdrop is unlit with the existing
soft contact decal; equipment uses the existing one-shadow-key/fill/cubemap path.
No realtime reflection, SSAO, bloom or additional runtime simulation is added.
Use **Gravity Box → COghe → V2 → Rebuild concept art and verify physics** to
regenerate and compare serialized physical/input data before building the Mac
preview. Camera/material differences are intentional. Review actual player
captures, and measure on the target phone before claiming mobile performance.

## V2 cutaway feedback (28 September 2026)

User-requested: near outer walls and their trim must emerge gradually while the camera orbits, without a hard visibility switch. Fade by view angle with temporal smoothing that reverses from the current opacity. Keep physics, navigation and ray-picking independent. Restore the original opaque render state when fully visible; serialize transparent material variants so player builds retain the required URP shader variants. Validate slow drag, fast reversal, fixed intermediate angle and pause in both tests and the native player.


## Return to glass — level 01 pilot (29 September 2026)

The user rejected the Blender presentation after device testing and requested the creature inside a glass enclosure again. This supersedes the opaque V2/Blender casing for the level 01 pilot. Restore transparent optical panes, slender aluminium rails, small porcelain corner mounts, a pale-blue inspection floor and a flush mint exit. Keep the dark liquid body, existing warm/cool Day Lab lights and quiet cream background. The box must visibly read as a closed glass volume, with the existing glass shader keeping foreground panes subtle.

Scope now: V2 01 only. Retain its current puzzle, dimensions, collider aperture, tap/orbit/pinch controls and save ID. Drag remains a camera orbit, not a physical box rotation. Other levels return to the pre-Blender profile by default; converting their opaque V2 walls to glass is not part of this pilot. Blender source assets and verification stay archived for history; they are no longer the default direction. `COgheGlassPreviewBuilder` rebuilds the pilot, and the V2 art generator calls its level-01 adapter so regeneration preserves it. Validate in the native Mac player before reporting the look.

### Glass depth study — level 01 only (29 September 2026)

User authorized testing greater depth with lighting, materials and background. This is an **experiment awaiting visual selection**, not a new approved campaign style. Level 01 exposes Original / A lighting / B materials / C lab-background controls. C is the pilot default. Switching reloads the same level; all geometry, inputs and puzzle rules remain unchanged.

Use one existing shadow-casting key and the existing static studio cubemap. A adds floor shadow reception and surface-local tissue contact shading; B adds restrained cubemap reflections, rougher porcelain and softer wet-body highlights; C adds low-contrast procedural lab silhouettes and a cool-to-warm background. These silhouettes are screen-space background art, not movable lab props. No new physics queries, realtime reflection capture, SSAO, bloom or depth-of-field. Contact shading is a proximity-based approximation that reads tissue positions and clears on escape; it is not simulated light transport. It stays attached to the actual support-plane coordinates and cannot fill a hole.

Keep study materials/shaders under `Art/GlassDepth`, never edit shared Day Lab assets for this trial. `COgheGlassDepthStudy` owns runtime presentation; `COgheGlassDepthBuilder` configures it through the existing glass builder. [Actual captures, tests and performance limits](../../Verification/COgheGlassDepth/README.md).

### Selected C and specimen numbers (29 September 2026)

The user selected **C**. C is the approved glass direction, replacing the pending-selection status above. The current implemented glass pilot remains V2 01; rolling glass geometry out to other layouts is separate from adding identification tags. Hide the comparison controls in normal play; explicit benchmark selection remains available for historical comparisons.

User correction: the first rail-clip prototype was rejected. Match the original `Concepts/01-day-lab.png`: a small ivory-plastic plaque adhered directly to the outside of the front glass, just below its upper-left rail. Rounded corners, dark blue-grey two-digit number (01–30) and a short printed underline. No clips, raised sign above the rim or billboard. Use depth-tested world-space text, no collider or click target; keep clear of the exit. The plaque follows the chamber and is hidden during victory.

`COgheSpecimenPlateBuilder` owns the repeatable tag build; the V2 art generator and glass pilot rebuild call it. Shared plastic material and font atlas, batched plaque/underline geometry; three renderers per enclosure. Preserve existing campaign numbering, camera framing and gameplay. [Verification](../../Verification/COgheSpecimenPlates/README.md).

### Spatial pilot — printed circuits and quieter guides (29 September 2026)

User requested more refined mechanism connections and less prominent sliders across
all ten Spatial Pilot scenes. Replace decorative diagonal linkage rods with thin
printed circuit paths on the floor, raised work surface and rear glass. Use chamfered
planar elbows and small terminal pads. These identify a control relationship; they
are not walkable bridges, physics cables or additional touch targets.

Keep blue A and muted coral B on the actual handles, identity badges and narrow
shutter bands. Large machine panels are pearl/ivory; guide rails are slender satin
blue-grey. Lavender still identifies slippery surfaces and mint still identifies
the exit. Pulley cables remain visible physical ropes, with quiet grey bearings and
small blue hubs, because they explain the real lifting mechanism.

`COgheSpatialArtBuilder` is the shared, repeatable presentation pass called by the
Spatial campaign generator. **Spatial pilot → Refine circuits and verify physics**
updates saved scenes without rebuilding gameplay. Static decoration is combined
by shared material; moving badges stay on their actual bodies. No new runtime
behaviours, lights, colliders, input targets or navigation patches. Preserve Glass C,
the specimen tag, camera, puzzles and all other campaign catalogs.

### Spatial mobile surfaces and tube mouths (30 September 2026)

Closed mechanism bodies assembled from thick contact panels must not render a
panel's narrow closing face on top of an adjacent panel's front face. This creates
ivory/lavender depth fighting, especially visible while orbiting on OPPO. Keep the
physics slabs intact; remove only fully covered closing triangles from separate
render meshes. Do not hide the problem with depth bias, enlarged colliders or
changes to slippery materials. Fixed surfaces, moving bodies and parked replicas
must all be checked. Share identical repaired meshes between levels.

Spatial tubes use a restrained transparent cyan bore, two thin teal longitudinal
seams, and porcelain mouth collars with a narrow teal gasket. Preserve a clearly
open centre: collar inner radii remain outside the physical bore. Mark actual entry
nodes, not closed ends or nonexistent connections. Mint remains the final exit;
blue A/coral B remain control circuits. Two batched opaque art meshes per static
network, shared materials, no new lights, runtime behaviours, colliders or pick
targets. Flexible tubes need deformation-aware decoration and are excluded from
static collars/seams.

The Spatial generator calls `RefineSpatialReadability`; for existing scenes use
**Spatial pilot → Repair surface rendering and clarify tubes**. It compares all
50 scenes' physics and definitions before/after and writes an affected-level audit.
Run visual tests with a graphics device (not `-nographics` when the test captures
`Camera.Render`). [Verification](../../Verification/COgheSurfaceReadability/README.md).
