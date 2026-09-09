# Campaign boss whiteboxes and proof requirements

Implemented 2026-09-09 by the requested GPT-6 Astra boss task. These are new authored physics whiteboxes, not renamed lab prefabs. The design reference remains `CHILL_CAMPAIGN_DESIGN.md`. This document does **not** certify complete route solvability, casual difficulty, timing tolerance, mobile readability, or release readiness.

`BossCampaignBuilder.Build(spec, context)` dispatches only C10/20/30/40/50/60/70/80/90/100. BossGeometry attaches transient meshes for the campaign generator to persist; no prototype asset is mutated. Shared HardCampaignModules supplies gravity sliders, contact-seated hinges, retained rack cams, hanging cages and the flight trough. Lab runtime ball, gravity, material, rotation, exit assist and 120 Hz timing remain shared.

| Boss | New physical construction | Recovery / retained state | Evidence still required |
| --- | --- | --- | --- |
| C10 Lion | Smooth 16-facet mask with broad brow and cheeks; full-depth rounded muzzle island; either cheek connects to the mouth bore | Both cheeks loop to the mouth; no narrow mane pockets, locks or hidden conditions | Both authored-spawn physics routes; perturb steering; new-player first look |
| C20 Garden | Three raised courts, two opposed 120 mm sliders, folding bridge with contact receiver, final graded court | Separate parking pockets; bridge pawl holds after actual seat contact; missed crossing stays near stage three | Both slider operations from spawn; bridge seating/contact under live gravity; sill and recovery bypass audit |
| C30 Constellation | Independently suspended initial cage, 14 cube rooms with a short series of 3D turns, separate sphere shell | 116 mm clear room sections contain misses at adjoining nodes; reverse portals remain open | Sphere clearance fixture is geometric, not a dynamic full-route proof; cage-mouth recovery and optical readability need actual steering |
| C40 Heart | New enclosed unequal-arm load module, 65 mm A recess, 90 mm B cradle and 96 mm B window, physical securing plunger; roof-fed cage; broad passive catch and physical funnel | A holds load while B reaches plunger; an ideal retaining pawl arms but never drives the lever; both remain dynamic | Recalculate/measure loaded and released torque in multiple poses, full A→B→A route, inlet sweep/contact, no bypass that invalidates cooperation |
| C50 Lotus | New lobed shell around first full rack cam, contact-seated bridge and opposite-facing final full cam leading to the central bore | Each cam retains reached teeth; three refuge courts; bridge gap has a lower local floor | Physical cam→bridge→cam route and native rack contact; retaining states through inversion; stage boundaries; final bore through both decks |
| C60 Orbit | Pendulum waiting court, separate preparation path to real launch gap, contained receiving tower, down-transfer to lower cage, graded exit approach | Launch miss basin and direct return slope; waiting pocket before flight; cage has local catch apron | Full sequential route, geometric fit at receiver→cage elbow, angle/time perturbations, recovery under 20 s; no timing claim from flight duration |
| C70 Aquarium | One sinking steel ball; three connected regions around static stepped crescent walls in the original 320×84×320 mm liquid domain | Wide reversible corners; no submerged dynamic props | Dynamic water route, wall model behavior at obstacle joins, all orientations near bore |
| C80 Silver Moon | Same exact fluid domain; one floating ball; stepped crescent support surfaces and explicit optical-cutaway label | Connected upper support faces and broad return regions | Dynamic buoyancy route and exit equilibrium; explain optical cutaway at phone size |
| C90 Kaleidoscope | Cage and spatial room course with two real short loops that reconnect to nearby nodes; circle/diamond/fork landmarks | Wrong branches reconnect rather than long dead ends; no reflected fake paths | Full 3D route and local recovery. Current outer sphere is an inspection enclosure: the branched room network supplies the unusual topology. A more sculpted outer enclosure remains art work |
| C100 Universe | Compressed sphere with load/plunger module, separate retained cam court, roof-fed cage, independent preparation court, launch trough, contained receiver and final funnel | Mechanical load/cam progress persists; two balls can pause before being launched separately; misses stay above a perforated recovery basin | End-to-end two-ball route; contact/escape of both; native stage transfer fit; perturbation and recovery playtest |

## Physical assumptions

A's authored long-arm load at 0.25 m gives approximately `0.111 × 9.81 × 0.25 = 0.272 N m` at the level pose. B at 0.11 m opposes it with about `0.120 N m`. These figures are a static sanity check only; actual contact positions, the inclined cradle, lever inertia, world orientation and friction must be measured in the solver. The lever is trimmed about its pivot rather than made artificially weightless. No ball pose or velocity is rewritten by a boss mechanic.

The bridge and cam use the existing ideal contact/retaining-pawl constraints. They retain an already reached physical state; they do not add opening torque. The securing plunger enables the retaining pawl after genuine contact pressure. These are idealized mechanical constraints, not detailed flexible tooth material simulation.

The cage roof inlets, stationary transfer sleeves, flight tower and their adjacent courts are new geometry. Clearance and containment at moving interfaces require particular attention. A physically valid alternate route remains a valid solution: there is no invisible stage gate in ExitSocket. If a route invalidates the intended puzzle, repair the physical housing and retest rather than adding a waypoint requirement.

## Visual feedback

`BossPresentation` is a prefab component implementing the existing ball-binding/reset contracts. It reads roster positions, native hinge/cam/seat state and real `ExitSocket.HasExited`. Thin inlays respond using per-renderer material property blocks. Two-ball final illumination waits for both actual escapes. Reset clears observed progress. It never changes physics, collision, exit acceptance, camera orientation, time scale or body transforms. Observation regions are presentation only.

Visuals are deliberately sparse whitebox art: engraved lion face, leaf/lotus inlays, orbital rings and differentiated room landmarks. They are not a claim that the planned final art/audio/replay package is complete. A full-box view of later bosses still renders a 30 mm ball small on a 360 px display; C40 radius was reduced to 0.73 m and C100 to 0.82 m, but phone-size acceptance remains unresolved. No new cinematic camera or opaque VFX conceals this issue.

## Runnable focused fixtures

`Assets/_Game/Tests/PlayMode/CampaignBossRouteTests.cs` extends the existing physics fixture:

- `CampaignBossLion_FromSpawnEitherCheekEscapesWithSharedTilt`: two real physical routes using only ordinary root target orientation from the authored spawn; full exit and reset assertions.
- `CampaignBossGarden_OpposedDoorsAndBridgeWorkUnderRealTiltWithLiveBall`: opposite gate travel, actual bridge seating and retention under reverse tilt, with the live ball remaining dynamic from its authored spawn.
- `CampaignBossLotus_FirstCamThreeContactStrokesAndPassageFromAuthoredSpawn`: three physical ball/rack strokes and spring returns, retained teeth, then the same operating ball traverses the opened first cam.
- `CampaignBossConstellation_CentrelineHasSphereClearanceAndClosedWrongFaces`: 30 mm swept-sphere clearance through all planned room portals, reverse recovery clearance and a real closed wrong face. This proves a geometric corridor, not gravity steering ability.
- `CampaignBossCooperation_HasBroadPhysicalRefugeAndContactRetainedProgress`: broad refuge/roster/contact references and reset after actual simulation. This is a mechanism contract fixture, not a complete cooperative route.
- `CampaignBossLiquid_UsesExactRectangularDomainAndOnlyStaticObstacles`: actual retained-fluid domain and static collider/roster/immersion checks.

Generate campaign assets before running these fixtures. Root task owns Unity invocation, generated content and consolidated audit. No Unity run was made from the boss-authoring task. Failed fixtures or geometry audits are blockers for the specific affected claim; they must not be replaced by count checks.

## Acceptance ledger

Source authoring for all ten bosses: implemented. Structural/proof fixtures: authored. C10 dynamic whole-route verification: both cheek routes passed. C20 native gate/bridge operation and C50 first cam act passed, but neither is a whole-boss route proof. Other dynamic whole routes remain pending. All-100 spawn/static bore audit: passed generation 6. Touch/mobile readability and user timing/recovery budgets: pending. Root implemented isolated inspection-glass/brass/graphite materials and a 15-second actual-pose replay; their complete product verification belongs to the consolidated report. Contact sound tuning and final authored art remain polish work.


## Recorded runner evidence

The parent task ran Unity 6000.3.19f1; the boss task did not launch a second editor.

- [`geometry-audit.json`](Verification/Campaign100/geometry-audit.json), generation 6: 100 levels checked, no spawn or static exit errors. This does not test complete routes.
- [`campaign-playmode.xml`](Verification/Campaign100/campaign-playmode.xml), first campaign run: C10 both from-spawn cheek routes, C30/C90 swept room portals and wrong face, C40/C100 physical simulation/reset and wide refuges, C70/C80 retained liquid domains all passed. C20 and C50 failures were retained as evidence and fixed rather than weakening assertions.
- [`playmode-results.xml`](Verification/Campaign100/playmode-results.xml), 367/369 full PlayMode run: C50's actual first rack completed three strokes and returns, and the same operating ball crossed into the next court. No ball pose/velocity assignments or capture were used. C20's opposite sliders passed after a 3 mm per-side vertical lintel clearance fix; its bridge still failed at about -80.225 degrees.
- [`bridge-focused.xml`](Verification/Campaign100/bridge-focused.xml), 2/2 focused run: C20 and the earlier chapter bridge passed after physical mating clearances were corrected. The fixed landing previously started at x=138 mm, exactly where the 140 mm moving rail reached at about 80 degrees, with zero lateral rail clearance. Moving the fixed landing start to 145 mm and providing 4 mm lateral clearance allowed actual seat contact and reverse-gravity retention.

C50's first failure exposed a controller that held return tilt for three seconds after the rack had returned: the live ball drifted away from the actuator. Its return now brakes through a physical feedback-steered approach and reapproaches the contact face for each stroke. The south separator was also extended to the actual shell, closing an unintended shortcut around the first court. The cam still receives all opening work from native ball/rack/finger contact.

Final consolidated PlayMode results after the shared fixes are recorded by the parent task in the verification README. These records must not be described as full route proofs for all ten bosses.
