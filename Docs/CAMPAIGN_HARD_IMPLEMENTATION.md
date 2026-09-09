# Hard campaign implementation — 35 non-boss levels

Authored 2026-09-09. These are implementation records, not measured difficulty or a claim that all intended routes have passed playtest. Unity generation and physics execution are owned by the campaign integration task; do not infer solvability from a successful build.

## Shared physical contract

All dimensions are metres. The catalog supplies the same 30 mm, 111 g ball, world gravity, 120 Hz timing, rotation input and 23 mm radius real final bore. Internal transfer bores have no assist or completion component. Only the final `ExitSocket` counts escape, and all balls must leave. No runtime waypoint, ball pose assignment, motor, collider toggle, teleport or hidden route requirement was added by these builders.

`HardCampaignModules` creates primitive compound rigidbodies and transient convex meshes. Root authoring persists them in the campaign folder. `MechanicalAuthoring.MeshObject` is deliberately not used: generating hard campaign content must not overwrite a prototype mesh in the Lab.

- **Bridge:** native hinge, 140 mm plank, 100 mm clear deck, 90 mm resting pocket, real receiving collider and existing contact-seat latch. Its approach abutment and sill close the under-bridge bypass. Deep housings add full-depth extensions. A local east ramp returns a miss to the current landing.
- **Cam:** native P22 rack, return spring, folding drive pawl and physical gear teeth, with a 72 mm slot. The rotor is authored to open at one, two or three 30-degree teeth. `MaximumTeeth` prevents subsequent strokes closing the route. `PassageOpenDegrees` reports that level's physical opening. Defaults remain three teeth and 86 degrees for the Lab.
- **Shared-state cam:** C45/46 use two shutter rotors with one angular state, transmitted through a native angular-only constraint. A visible timing-belt trace explains the shared state. First rotor has a convex clipped swept slot that stays open from tooth one through tooth two; the second becomes traversable at tooth two. Each court exposes only its next rack. This realizes the matrix's intermediate-route dependency using a coupled two-rotor assembly rather than a single large rotor.
- **Cage:** passive weighted hinge with ±32-degree stops, 80 mm mouths and broad catch aprons. C86/98 add a close circular swept housing. C93/97 reuse the boss roof-fed cage with a 120 × 130 mm inlet, 90 mm outlet and ±24-degree stops.
- **Flight:** 92 mm takeoff trough, 90 mm unsupported gap, 220 mm receiving width and receiver 70 mm below the lip. Hard flight-training levels offset the receiver by 130 mm, requiring approximately 35 mm of lateral catch adjustment; the broad C98 alternative accepts a prepared orientation. A 40 mm radius internal bore drains the receiver; its walls extend to the local floor so a miss cannot simply roll below the tray to the bore. The 302 mm rise of the nearby return apron replaces P21's route around the whole lid.
- **3D graph:** sealed unions of 120 mm room cells, 4 mm walls, genuinely open shared faces and an actual outlet. A missed turn stays at an adjacent node; reverse faces remain open. These are new graph topologies, not a rotated P12 prefab.
- **Liquids:** exactly one ball and static axis-aligned obstacles. The retained rectangular domain is `(0.37, 0.357, 0.37)` half-size, matching the 720 mm deep shell's inner floor and cover faces. Water/mercury profiles and visuals are copied by reference from their existing Lab profiles; no material or fluid tuning is changed. The built-in rectangular bulk-flow approximation remains the model, not an arbitrary-shape fluid simulation.

## Per-level physical changes

| Level | Authored physical change | Recovery / review focus |
| --- | --- | --- |
| C15 | Short one-bridge court with north parking refuge, full bridge sill, direct east exit court. | Local east return ramp; verify seating and novice understanding before tuning. |
| C16 | South-facing refuge and north approach cheek reverse the parking direction while preserving hinge behavior. | Early departure returns to the same bridge court; latch retains a seated bridge. |
| C17 | Bridge landing feeds a real north return bend, with a small final refuge. | Brake after the bridge before the exit bend; no long upstream recovery. |
| C18 | Gravity slide, raised entrance deck, independent middle pocket and bridge in series. | Middle deck is outside the slide sweep. The lower sill and upper portal remain physical. |
| C23 | Broad single passive cage with closed entry and one 80 mm side mouth. | Large external floor and nearby catch aprons receive an imperfect release. |
| C25 | Cage release feeds an L-shaped receiving court, with an inside corner and separate braking apron. | The next turn is visible after leaving the cage; floor catches overshoot. |
| C27 | New six-turn 3D route with horizontal, vertical and depth transfers; sealed start and actual output. | All route faces are bidirectional; local cell walls contain misses. |
| C28 | Longer eight-turn 3D route plus a short elevated visible dead-end branch. | The branch rejoins at its entry; no unseen corridor beyond its end cap. |
| C33 | P18 long arm shortened by 60 mm, corresponding A exit wall and receiver shifted inward, receiving deck extended. | Joint mass/axes remain unchanged; native load and contact must still be validated after geometry change. |
| C35 | Full-length lever with 64 mm recessed A holding pocket, central start and B braking cheek. | A must reach the far recess; B's receiving direction differs from C33. |
| C36 | Broad loaded lever starts A in its refuge; a single physical B plunger arms the retaining seat. | This is a passive P18/P23 realization of the single hold/latch relationship; no P16 powered shutter is added. |
| C37 | Two distinct receiving pockets on opposite sides of the lever, joined by a south U route around a splitter. | Either ball may leave first after the contact latch; no permanent load is required. |
| C38 | Far load recess, B contact-release pocket and short lower S-shaped retrieval route. | A holds while B reaches the latch, then both can be retrieved; identity does not control forces. |
| C45 | Coupled 30/60-degree shutter assembly in two serial courts, first passage swept open through the second state. | The second rack lies beyond the first shutter; returning to the first court remains geometrically possible. |
| C46 | Same shared mechanical state in an L-shaped enclosure: second actuator is approached around a corner from a new side. | A central braking island gives a visible stop between the two actuator faces. |
| C47 | Cam working chamber, graded transfer, retained bridge and an isolated north return lane ending behind the cam. | Cam depth closures and south partition remove the deep-box bypass. The return lane is entered only after the bridge. |
| C48 | Independent cams in serial courts: one tooth first, two teeth second; intermediate approach fork and refuge. | Both states retain independently; no contradictory simultaneous input is required. |
| C56 | Offset flight receiver with actual bore, floor-tight receiver tower and nearby return ramp. | A missed catch remains within the launch region; full tilt-only recovery needs execution. |
| C57 | Receiver drains into a contained lower L route, with a physical turn baffle before the final bore. | Walls catch landing momentum; local lower court cannot expose the launch area to the final aperture. |
| C58 | Stable pendulum waiting court precedes a separate preparation landing and graded ascent to the launch pocket. | A miss after launch returns locally without reopening the pendulum. Check the narrow outer swing envelope. |
| C67 | Static water U-route on an upper support, then a lower receiving route on another depth line. | Actual cell transfers and reverse adjacency; same water profile throughout. |
| C68 | Two short water routes with the lower layer offset in depth from the upper projection. | A receiving node is immediately below the transfer; no wrong hole leading to the start. |
| C77 | Mercury route through a wide vertical neck into a higher support chamber, then a visible turn. | 76 mm alignment collar, broad cell on both sides, no minimum speed or launcher. |
| C78 | Two mercury paths joined upwards through two short support changes. | Each transfer lands against a nearby high support; no whole-box recovery sequence. |
| C85 | Dry two-chamber route with a central neck and orthogonal support transitions. | Three clear resting regions; actual 76 mm collar at the middle transfer. |
| C86 | Two separate passive cages with different hinge/support axes and an intermediate refuge, both with fixed swept housings. | Intermediate court is outside the rotating cages; audit all tilted mouth clearances and possible outer-shell bypasses. |
| C87 | Two legitimate graph alternatives: short route through a 60 mm collar or long 116 mm-clear route across depth/height. | Both rejoin a common node; neither branch seals after entry. |
| C88 | Three short 3D regions plus a visible return loop linking the upper and lower decisions. | Loop uses real open room faces; wrong turns remain reversible locally. |
| C91 | Two separate start refuges feed one one-tooth retained cam and one shared exit court. | Either ball may push the rack and either may leave first; retained opening remains. |
| C92 | Enlarged load module drains to a sealed rack court. Actual rack compression arms the loaded lever's latch, then opens the common output. | The pressure observer adds no second return spring; the existing rack spring supplies it once. A remains retrievable after B works the cam. |
| C93 | Two-ball upper cam court, retained two-tooth opening, enclosed transfer into roof-fed cage, broad local catch pan and final bore tube. | 170 mm forward transfer position fits the enclosure; misses land in the local catch pan. |
| C95 | Two side-by-side workshops with independent one-tooth cams and two initial balls. Common north court is beyond both shutters. | Order is chosen by position; opening either cam does not close the other. |
| C96 | Two preparation refuges, one shared flight trough, one broad receiver with an additional resting pocket. | One ball can wait while the other flies; receiver has a retained physical lower outlet. |
| C97 | Enlarged A/B contact-latched load module, real transfer tube, roof-fed cage and separate receiving pan. | Same cooperative state survives passage through the cage; either ball can leave first. |
| C98 | Longitudinal cam, bridge and common choice court. North branch is a passive cage; south branch a genuine flight/catch. Separate final-boundary apertures admit the cage at receiving height or the flight through its enclosed lower outlet. | Both branches meet one final court. High-risk integration case: validate route choice, two-ball staging and all lower/upper bypass closures before acceptance. |

## Verification ownership and limits

`HardCampaignMechanismTests.cs` provides:

1. A **tilt-only** bridge seating trial from C15's authored spawn, followed by reset. It does not claim complete exit-route verification.
2. One/two/three tooth stop/extra-return/reset checks, including the unchanged Lab default and correct `PassageAligned` telemetry.
3. Geometric full-ball clearance at the retained C91 and C93 cam positions. These deliberately position the mechanism for geometry inspection; they are not route solutions.
4. Full 30 mm sphere sweeps in both directions along the main 3D room paths for C27/28/67/68/77/78/85/87/88. These prove connector geometry, not input timing or perceived difficulty.
5. Single-ball, static geometry, exact rectangular-domain and final-bore contracts for all four hard fluid levels.

Root's campaign tests additionally check all spawns, finite containment, reset/registration and whole-ball final traversal. Keep build, geometry fixture, mechanism trial and complete tilt-only route results separate in the verification report. Complete routes and recovery times for the new composites, especially C45/46, C58, C86 and C92–98, require execution and should remain unaccepted until it succeeds. Touch-device readability and measured difficulty remain playtest work; target D is not a measured result.

### Integration results and corrections

The integration task's initial campaign run passed all 100 spawn/lifecycle tests and all 100 isolated final-aperture tests. Twenty of the first 21 hard fixtures passed, including the shared cam's intermediate/final clearances and all nine bidirectional room-route clearances. The bridge fixture first needed to look up its detached prop through `LevelRuntime.Props`; subsequent actual simulation exposed a real 80-degree mechanical jam.

The shared bridge's old static rails began at X=138 mm and exactly met the moving rail tips at Z=52 mm. Static landing and rails now start at X=145 mm, 5 mm beyond the moving plank, and the fixed rail centres are Z=±59 mm. The landing top aligns with the plank at Y=3 mm. Root's generation-six tilt-only C15 and C20 seating trials both passed after this correction; joint limits, contact-seat tolerance and input forces were not relaxed.

C85's neck was corrected to the actual X transfer plane; an additional fixture checks centre clearance, off-centre contact and closed lower/depth faces. The bridge return now enters the open end of the landing, with the toe authored at each caller's actual recovery floor. The C17 turning wall ends before that diagonal return. Eight additional fixtures cover the recovery join and shell clearance for every shared-bridge caller: C15/16/17/18/20/47/50/98. The integration task will record their final generation-seven results in its consolidated verification report. These checks still do not establish full player routes or recovery times for every composite.

Generation seven passed the corrected C85 neck fixture and all eight shared-bridge recovery-join/shell-clearance fixtures. Its full PlayMode result was **377/378 passed**. The single remaining failure was C17's final bore: the new return-ramp toe occupied the former northern exit location. C17 now finishes at `(0.31, -0.11)` in XZ, down the eastern side of its U return, while the north court remains the recovery refuge. That coordinate drives the actual `Planar` floor cut, socket and inlay together. Root is verifying the regenerated C17 and final full run; no hidden assist relocation or collision change was used to mask the blockage.
