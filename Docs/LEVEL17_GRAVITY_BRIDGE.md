# Level 17 — Fold the bridge

The player parks the steel ball in the western three-sided recess, tilts the box east to let a hinged bridge fall into its receiver, returns the ball to the doorway and crosses the bridge. The eastern landing ends above a shallow exit well, so the final descent and complete exit remain visible.

## Physical construction

- Outer box: 600 × 360 × 280 mm. Steel ball: unchanged 30 mm / approximately 111 g. The extra headroom clears the bridge rail's full swept arc.
- The western floor is a solid abutment with its top at Y = −19 mm. A partition across X = 0 leaves one 110 mm crossing. Its lower sill prevents passing beneath the raised bridge.
- Bridge: 140 × 100 mm, 6 mm thick, mass 80 g, two glass side rails. Its horizontal Z hinge is at (0, −22, 0) mm. The bridge starts upright and falls east through 90 degrees.
- The receiver beneath the far end is a real collider. Actual contact within 0.8° of the seat, after closing motion has slowed, engages an ideal retaining pawl, represented by limiting the existing native hinge to its current angle ±0.1°. Speculative collision predictions cannot engage it early. No position, velocity, ball force, animation or collider disabling drives this transition. This is an ideal rigid latch abstraction, not a detailed flexible metal pawl simulation.
- The landing starts 5 mm beyond the bridge end and has a matching top elevation. It does not obstruct the bridge's receiving contact.
- Standard Earth gravity acts on the ball and bridge. The only added mechanical force is small viscous bearing friction. The shared 40 mm exit assist remains enabled.

## Intended route

1. From (−215, −95) mm in the XZ plane, roll north into the marked parking recess near (−215, +80).
2. Tilt east approximately 20–30°. The recess's eastern cheek holds the ball while the bridge lowers and seats.
3. Roll south out of the recess, then approach the opening at (−75, 0).
4. Roll east across the seated bridge and landing. Drop into the well, then approach the real circular outlet at (+245, 0).

The test suite includes an isolated gravity/latch/reset experiment and a complete spawn-to-exit route controlled only through box orientation. The route test is not a runtime autopilot. Difficulty and feel still need human playtesting.

Full three-axis rotation may reveal alternative physical routes after the opening is cleared. Such routes should be assessed as emergent solutions, rather than silently prevented with invisible walls or artificial ball control.
