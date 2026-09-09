# Level 19 — Lồng trong lồng

## Intended experiment

The outer box is held by the player. A weighted inner cage hangs on a horizontal bearing and tries to align its centre of mass with Earth gravity. Its floor therefore responds differently to the outer shell. Once the outer box rotates beyond the available hinge travel, a physical joint stop makes the inner floor tilt and releases the ball through the cage mouth.

This is a single suspended cage inside the outer enclosure. It tests the basic physical interaction before adding multiple nested axes.

## Authored geometry

- Outer interior: 640 × 600 × 360 mm; shared 30 mm, approximately 111 g steel ball.
- Cage: 266 × 266 mm footprint, floor and roof 100 mm apart, 6 mm skins.
- Cage pivot: local `(0, 25, 80)` mm, hinge along X, limits −32° to +32°.
- Cage mass: 340 g, centre of mass 45 mm below the bearing; visible keel under the floor.
- One front opening: 64 mm wide, approximately 94 mm high. The roof and other walls remain collidable on inversion.
- Mouth transfers to the lower enclosure floor. The circular shared exit sits at `(235, −180, −235)` mm.

All moving cage parts belong to one detached world-space Rigidbody. A native HingeJoint carries its load; passive bearing damping adds torque proportional to relative angular velocity. No motor, orientation servo, ball force, teleport, timed door or hidden collider switch is used.

## Intended route

1. Tilt gently about X to observe the inner floor resisting the outer rotation.
2. Continue in the direction that lowers the cage mouth. The ±32° stop eventually forces the inner floor to slope.
3. Keep the ball lined up with the mouth, then let it fall into the lower catch area.
4. Restore a comfortable view and guide it along the enclosure floor to the circular exit.

The automated route uses only `BoxRotationController.SetTargetOrientation`: −76° about X until mouth traversal, then gravity-based feedback to the floor exit. Its result is recorded in the current Unity test run; this document does not claim a user difficulty rating.

## What to assess by hand

Can the player understand the independent centre of gravity and the stop without a long explanation? Does the visible keel convey weight? Is the release readable through the two clear skins? A route that finds a different stable orientation is a valid physical solution.
