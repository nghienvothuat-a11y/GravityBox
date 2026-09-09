# Level 22 — The box remembers

The ball pushes a spring-return rack. A separate hinged finger on that rack
contacts a twelve-tooth wheel, physically rotating a slotted cam. A retaining
catch remembers each reached tooth; an escapement permits one tooth per stroke.
After three pushes the cam's real slot connects the entrance and exit courts.
The same ball must leave the rack, cross the opened passage and physically exit.

## Mechanical model

| Component | Authored values |
| --- | --- |
| Shell half-size | `(0.36,0.060,0.34)` m |
| Cam pivot/axis | `(0,0,0.045)` m; local `-Y` |
| Cam | 65 g; two convex half-discs, radius 94 mm, 64 mm clear slot |
| Teeth | 12 at 30-degree pitch; 99 mm centre radius |
| Rack | 18 g; 62 mm native-joint stroke along `+X` |
| Rack return spring | 12 N/m; damping 0.18 N·s/m |
| Hinged drive finger | 4 g; native angular range 0–85 degrees |
| Finger return spring | 0.004 N·m/rad; damping 0.00006 N·m·s/rad |
| Cam bearing friction | 0.00008 N·m·s/rad |
| Exit | `(0.22,-0.060,0.25)` m; shared round bore/approach assistance |

The rack contact face is transparent with metal rims so the player can see the
ball loading it. The visible coil follows the rack's measured position. The cam,
rack and finger are world-space dynamic rigidbodies, joined to the box or rack.
Contacts provide all forward work. Only passive return-spring and bearing forces
are added by the mechanism.

The retaining/escapement pair uses **ideal unilateral native hinge limits** rather
than solving two additional tiny pawl-tooth contacts. After the measured cam angle
passes 30/60/90 degrees, its reverse limit moves behind the already-passed tooth
with 0.8 degrees of clearance. The forward stop starts at 31.5 degrees. It permits
61.5 and then 91.5 degrees only after the real rack returns below 3.5 mm. Releasing
a stop applies no torque or angular impulse. No script assigns a cam angle,
changes a ball's pose/velocity, disables a passage collider, or signals an invisible
door to open.

A fixed transparent sleeve around the cam has only north/south mouths. This is
essential: without it, the first oblique slot orientation would already allow a
diagonal shortcut. The sleeve's radial clearance is smaller than the ball's
diameter, while the physical gear retains clearance from its fixed housing.

## Playing route

1. From `(-0.275,-0.038,-0.245)`, bring the ball to the left of the rack contact
   face, near `(-0.172,-0.038,-0.100)`.
2. Tilt right to push; tilt left until the rack and finger return. Repeat three
   times. The cam remains at its reached state while the ball moves away.
3. Go around the south end of the rack: approximately `(-0.24,-0.20)` then
   `(0,-0.20)` in the floor's XZ plane.
4. Cross the aligned cam to `(0,0.18)`, then reach the exit `(0.22,0.25)`.

The from-spawn integration test uses the normal box rotation controller, including
speed and acceleration limits. It verifies exactly one retained tooth per press,
spring return, retained cam angle, traversal by the operating ball, physical exit,
and reset. With the final fixed sleeve, the three observed cam angles were
31.47, 61.50 and 91.50 degrees. Both mechanism tests, including the complete ball
route, passed in `Artifacts/mechanical-sixth.xml`.

Future variants can change the cam profile and port placement, use a second cam,
or require a route through one intermediate state before advancing again. Keep
the spring load and escapement visible, and preserve a way to retrieve every
required ball.
