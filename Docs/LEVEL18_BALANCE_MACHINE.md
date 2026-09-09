# Level 18 — Two balls, one balance

Two identical steel balls occupy separate lanes attached to one trimmed balance beam. Initially A sits near the pivot, while B sits farther along the short arm; B's moment holds its end down. The player tilts left to move A toward the far end of the long arm. A then lowers its own end and lifts B until the beam contacts a retaining pawl. Both balls can leave through the aligned side windows and reach the same circular outlet.

## Physical construction

- Outer box: 640 × 460 × 320 mm. Both steel balls retain the shared 30 mm diameter, density and approximately 111 g mass.
- Beam mass: 60 g. A visible trim weight represents static balancing of the unloaded beam; its authored centre of mass is the hinge origin. Its single native hinge has no motor and no spring.
- Initial carrier pose: −25° about Z, baked into a child frame while the native hinge body begins at identity. Native hinge range: −1…27°, with the seat at +25°. A starts at carrier-local X = −38 mm; B's cradle is centred at +95 mm. Move A to the marked load position around −180 mm and counter-tilt while the beam rises.
- A's clear lane extends X = −240…+40 mm at Z = −55 mm. B has a 76 mm long cradle at Z = +55 mm, inclined +25° relative to the carrier so that its floor starts level in the world. This prevents B from rolling off the carrier end and wedging against the fixed sill during the opening seconds. Clear lane height is 42 mm, with transparent sides and roofs.
- B's inner cap retains it against the leftward slope needed to move A. After the lift, tilt right far enough to climb the now-inclined cradle. The two required balls remain ordinary independently simulated rigid bodies.
- Fixed exit walls at X = −249 and +152 mm contain windows 51 mm and 66 mm high respectively. The displaced carriers cannot meet these windows until B has been raised and the main carrier is approximately horizontal.
- A real receiving collider at (−218, −11, −55) mm engages the same contact-actuated ideal retaining pawl used by level 17. The hinge is retained at its contact-resolved pose. This ideal lock is a deliberate rigid mechanical abstraction; a flexible pawl tooth is not simulated.
- The left and right landing courts lead south to a common lower exit court. No ball has to remain behind on a pressure plate after the latch engages.

## Intended route

1. Tilt left roughly 30–40°. This overcomes the initial uphill slope of A's lane and rolls A toward its marked load position. Keep A on the mark rather than forcing it into the distant end gap.
2. Let the far-arm load lower A's end, lift B and seat the pawl. The motion and two different lever arms should make the cause visible.
3. Let A roll through the left window into the western side court, then south into the lower exit court and through the outlet at (0, −184) mm in XZ.
4. Tilt right approximately 35° to retrieve B up its cradle and through the eastern window. Bring it south, then to the same outlet. Either escape order is allowed by the global all-balls-out rule.

The tests include equal mass/no motor checks, a component experiment comparing load positions, latch retention/reset, and a complete orientation-only route from both spawns. Human testing is still required to judge how legible the lever moment is and whether the initial rotation is comfortable on the device.
