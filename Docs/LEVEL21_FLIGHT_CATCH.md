# Level 21 — Catch me

The steel ball gains speed on a continuous curved trough, leaves its physical end,
and travels freely under world gravity. The receiving court is offset sideways.
Turning the box changes where that court will be when the ball arrives; no force
steers the airborne ball.

## Geometry and scale

- Shell half-size: `(0.35, 0.29, 0.27)` metres.
- Spawn: `(-0.297, 0.231, -0.075)`.
- Trough: a short flat holding pocket followed by a 32-segment continuous cubic
  surface, from `(-0.285, 0.210, -0.075)` to `(-0.020, 0.075, -0.075)`.
- Clear trough width: 64 mm; steel-ball diameter remains 30 mm.
- Receiving mouth: approximately `x=[0.075,0.305]`, `z=[-0.015,0.207]`, at
  `y=-0.042`. Its walls extend to the enclosure floor, so a miss cannot simply
  roll into the receiving court from below.
- Real round exit: `(0.225,-0.290,0.120)`, with the shared 23 mm radius and 40 mm
  unobstructed approach assistance.

## Playing route

1. Tilt gently towards the low end of the takeoff trough.
2. Begin rotating about the vertical axis while the ball travels down the curve,
   bringing the offset receiver underneath its flight.
3. Once the ball lands in the receiver, tilt it along the floor to the round exit.

The verified automated route starts with local gravity proportional to
`(1.2,-9.81,0)`, begins a 60-degree yaw when the ball passes local `x=-0.200`, then
uses small tilts towards the exit. It only requests box orientation through the
shipped acceleration/speed limits; it never writes ball pose or velocity.

A miss stays in the closed outer enclosure. The intended recovery route uses the
clear cover: invert the box, move around the ramp to the cover, enter the upper
holding pocket above the start, then roll the box back through +90 degrees about
Z while holding the ball against the pocket's back wall. Finish with local gravity
proportional to `(-1.2,-9.81,0)` until ready to launch again. The back wall and pocket
cheeks physically reach the cover. The tested recovered ball position is
`(-0.30600,0.22501,-0.07354)`.

## Verification and scope

`FlightMemoryTests.cs` checks a real unsupported span, measured world-gravity
acceleration during flight, containment of a miss, a complete from-spawn catch
and escape, and recovery to the launch pocket. All three checks passed in
`Artifacts/mechanical-sixth.xml` using the shipped generated geometry.

This is a generous first physical prototype. Future variants can change the
landing offset, curve/drop height, receiver orientation, or add a second catch.
The ball physics and receiving-mouth clearance should stay readable when adding
difficulty.
