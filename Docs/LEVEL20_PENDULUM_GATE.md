# Level 20 — Cổng con lắc

## Intended experiment

A pendulum blocks the only passage between the waiting chamber and the exit court. Sideways rotation moves its centre of mass, creating a real swing. Forward tilt rolls the steel ball through while the aperture is clear. A player who discovers a stable open orientation has also solved the physical puzzle correctly.

This is the introductory member of the pendulum family. It deliberately permits a held-open pose; beating a narrowly timed rhythm is not a prerequisite for testing the core mechanic.

## Authored geometry

- Outer interior: 600 × 600 × 280 mm.
- Full-depth partition at Z = 0, with a single low 80 × 84 mm passage.
- Waiting guides constrain a parked ball laterally while the player tilts to excite the pendulum.
- Bearing: `(0, 100, −25)` mm, axis Z; native travel −68° to +68°.
- Pendulum mass: 280 g; bob centre 190 mm below the bearing and COM 175 mm below it.
- Bob: 100 × 78 × 14 mm. Its bottom is 8 mm above the supporting floor in the closed pose. The full lower-corner sweep retains more than 2 mm floor clearance; a 30 mm ball cannot pass underneath it.
- The bob swings behind the partition, separated from its back face by 12 mm. This permits a free swing alongside the partition without allowing a 30 mm ball to slip through the wall-to-bob gap.
- Circular shared exit: `(205, −140, −230)` mm, well inside the receiving chamber.

Native joints, gravity and contacts provide all movement. A small viscous bearing torque damps the pendulum. There is no timer, position animation or collider disabling. Both chambers have solid catch floors, so a failed crossing is recoverable.

## Intended route

1. Keep the ball inside the waiting guides.
2. Rotate sideways until the bob swings clear of the aperture.
3. Add forward slope so the ball rolls through the opening.
4. Bring the exit court toward level, then roll the ball to its round exit.

The automated route uses only player rotation targets, including a held pose near Euler `(−25°, 0°, −40°)` for the first crossing. Its result is recorded in the current Unity test run. Tests also drive the ball into the closed bob and verify that sideways gravity subsequently permits traversal.

## What to assess by hand

Is the crossing window easy to read? Can the player deliberately generate or damp an oscillation? Do the bob’s weight and contacts feel plausible? Future variations can add a second pendulum or a moving receiving bridge after the introductory behaviour has been tested.
