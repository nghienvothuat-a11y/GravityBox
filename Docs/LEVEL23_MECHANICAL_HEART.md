# Level 23 — Trái tim cơ khí

## Experiment

This boss combines the two-ball balance, a contact-operated retaining pawl, a gravity-suspended transfer cage and a free-flight catcher inside a glass sphere. Both steel balls remain physical and both must clear the final round bore.

The design tests whether these mechanisms remain understandable when they share one box rotation. It is a prototype of a difficult combination, not a measured claim about player difficulty or completion rate.

## Physical sequence

1. Roll A along the long arm into its far recess. The equal-mass balls then create unequal moments and lift B's shorter arm.
2. Tilt to make B climb out of its inclined cradle. A remains in a real recessed pocket, keeping the long arm loaded during the opposite tilt. B uses the 46 mm aisle and goes around the front of the release cup to reach its contact face.
3. B pushes a spring-loaded contact in its landing court. That contact arms the retaining pawl. The lever must actually touch its receiving stop near its seated angle before it can remain held.
4. Tip steeply to retrieve A from its recess. Move both balls through the upper module's transfer bore. This bore has no exit assist and does not count toward victory.
5. The balls drop through the suspended cage's roof inlet. Turn the sphere beyond the cage's ±24° stop to tip its floor toward the side mouth. They can pass through separately: rotations used to guide B can already deliver A through the lower mechanisms.
6. Both balls leave the mouth in unsupported flight, land in the catch tray and roll into a sealed funnel leading to the final round exit. A may escape before B reaches the cage; the session remains active until B also exits.

## Dimensions and checks

- Glass sphere: 500 mm inner radius, 6 mm shell.
- Upper balance enclosure: 640 × 460 × 320 mm.
- A's recessed pocket: carrier X = −226 to −184 mm; depth 8 mm. The 42 mm length accommodates a 30 mm ball while retaining a useful moment arm under B's crossing tilt.
- Boss retaining receiver: carrier-side position X = −234 mm, on the solid outer lip rather than below the recessed floor.
- B release plunger: 20 g, 8 mm guide travel; activation requires real ball contact and 3 mm spring compression.
- Transfer bore: `(0, −160, −184)` mm, radius 23 mm.
- Cage bearing: `(0, −285, −130)` mm, X axis, ±24° travel, 200 g carrier with a low centre of mass.
- Cage inlet: 70 ×115 mm; it is wider than the module bore to accommodate the carrier's angular travel.
- Cage side mouth: 66 mm clear width. The cage roof, walls and floor remain collidable.
- Catch tray: 280 ×320 mm, surface at Y = −399 mm. Its four 50 mm cheeks make a failed landing recoverable within the tray.
- Funnel: 53 mm inlet radius tapering to the shared 23 mm exit radius. The outer funnel wall meets the sphere around the exit so a ball outside the funnel cannot simply slide along the sphere into the final bore.

The steel balls retain the shared 30 mm diameter, approximately 111 g mass and Earth gravity. No runtime code assigns ball poses or velocities. All ball control in the solution test is expressed as an outer-box rotation target.

## Validation and limits

The focused run `Artifacts/mechanical-eighth.xml` passed both `MechanicalHeartTests` on 2026-09-09. It verifies the unarmed pawl/reset behaviour and completes the full route from both authored spawns using rotation targets alone.

The full-route test records these events continuously, including while the other ball is still being moved through the upper mechanism:

| Physical event | Ball A | Ball B |
| --- | ---: | ---: |
| Centre enters cage interior | 12.992 s | 16.292 s |
| Centre crosses the actual side-mouth plane | 15.458 s | 32.000 s |
| Whole sphere clears mouth, with no nearby solid contact | 15.500 s | 32.050 s |
| Complete escape from final round bore | Passed | Passed |

Times are elapsed simulated time in this authored test route, not a player target or difficulty estimate. Mouth-centre crossings were `(11.24, −32.01, 113.00)` mm for A and `(13.79, −32.02, 113.00)` mm for B in cage coordinates. Each sphere then fully cleared the mouth and had a measured interval without solid contact before reaching the catcher. The test checks the centre crossing separately from whole-sphere clearance because the centre has already fallen below the floor edge by the latter event.

The pawl is an ideal contact latch: engagement constrains the joint at the actual contact-resolved angle; individual ratchet teeth and latch deformation are not simulated. The release plunger supplies the state change, and does not drive the lever.

The intended bore-to-cage drop is geometrically aligned. The space between them is open; an expert who changes the rotation during that drop may find a way around the cage and onto the receiving tray. Enforcing a single mandatory route would require an additional physical transfer housing. This version keeps alternate physical routes legal, while the automated intended route explicitly requires cage traversal.
