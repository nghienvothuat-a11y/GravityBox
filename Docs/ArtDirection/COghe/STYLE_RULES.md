# COghe — Day Lab design rules

Status: **approved by the user, 16/09/2026**. Version 1.3 (control feedback and reinforced lid, 16/09/2026).

The visual reference is the Unity level 07 shipped at commit `8718849`, with
[runtime images](Runtime/README.md). The Day Lab concept is supporting inspiration;
its photographic lighting and proportions are not a requirement to change gameplay.

## 1. Identity and hierarchy

- A bright, warm, calm space research laboratory. The player guides a curious
  creature and builds a relationship with it. Keep equipment clean and readable.
- Write the name **COghe** exactly. The legacy app/bundle/save identifiers may remain
  Venom; changing branding must not silently reset progress.
- First read: COghe. Second: relevant mechanism, slippery surface and exit. Third:
  mounts, labels and the laboratory setting. Decoration must not hide actions.
- Retain the existing dark, asymmetric liquid body, tendrils, transient lobes and
  gravity response. Do not add eyes, teeth, ears, costumes or a permanent head.
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
