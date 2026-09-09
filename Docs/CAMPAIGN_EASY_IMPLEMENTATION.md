# Easy campaign implementation provenance

Date: 2026-09-09

Authoring model: **GPT-5.6 Sol**, assigned by the user for the 55 easy campaign levels.

Implementation: `Assets/_Game/Editor/Campaign/EasyCampaignBuilder.cs` and
`EasyCampaignGeometry.cs`. The builder owns C01–09, C11–14, C19, C21–22, C24,
C26, C29, C31–32, C34, C39, C41–44, C49, C51–55, C59, C61–66, C69, C71–76,
C79, C81–84, C89, C94, and C99.

`Assets/_Game/Tests/PlayMode/EasyCampaignRouteTests.cs` provides authored-spawn,
tilt-only route fixtures for the dry introduction set and the dry/water comparison plus
static-baffle water levels. These fixtures never assign a ball pose or velocity.

Each level makes a physical geometry or mechanism change described by the approved matrix:

- C01–09 use separately authored shells, voids, impact obstacles, guides, braking courts,
  and widened alignment spaces. They are not color or spawn-only copies.
- C11–19 keep the trusted passive slider but physically change holding pockets, approaches,
  post-gate turns, route length, and recovery arcs.
- C21–29 use broad real decks, transfer drops, a near-stop cage start, and short reversible
  multi-face routes. C26 is a reduced three-face network rather than the complete P12 maze.
  C24 moves the hinge limit near the neutral pose instead of presetting the Rigidbody, preserving
  the prototype joint frame and connected anchor.
- C31–39 add real second spawn points and use lane walls, stopping pockets, widened lever
  trays, cage mouths, and catch courts. Completion remains the existing all-balls exit rule.
- C41–49 preserve the physical rack/cam/ratchet and vary the required notch count, return
  court, branch order, direct lane, and post-cam arc. Reduced one/two-tooth cams use physical
  opening thresholds of 26/56 degrees so mechanism state matches their authored slot angle.
- C51–59 preserve physical pendulum and ballistic motion while widening the opening/receiver,
  separating approach axes, adding an observation pocket, and shortening recovery routes.
- C61–79 use one ball and static physical baffles only. C61/C62 and C71/C72 are matched
  dry/water and water/mercury comparisons. All other liquid routes alter fixed topology or
  supporting height while reusing the existing `WaterVolume` profiles.
- C81–89 return to dry physics with relief-shaped shells, perpendicular decks, route choices,
  broad transfer courts, and a shallow star landing around the independently hinged cage.
- C94 and C99 use two physical balls. C94 provides separate rack waiting pockets around one
  retained cam notch; C99 widens both lever lanes and their two short exit landings.

The campaign builder does not set ball velocity or pose during play, apply direct ball force,
disable colliders, or add waypoint completion. It preserves the shared 30 mm / approximately
111 g steel ball, Earth environment, physical exit, and 40 mm exit assist through the campaign
catalog architecture.

## Verification still required

This file records authoring truth, not a claim of completed playtest. Unity generation,
compilation, collider inspection, route-solving tests, and device playtest are owned by the
campaign integration pass. In particular, the reduced multi-deck paths (C21, C22, C26, C29,
C66, C76, C82, C83), modified joint clearances (C11–14, C19, C24, C34, C39, C41–55, C59,
C94, C99), fluid volume fit, and every exit approach need runtime route verification before
release. Any shortcut found by real physics is acceptable unless it removes the level's sole
teaching decision; no hidden route or waypoint rule should be added to suppress it.
