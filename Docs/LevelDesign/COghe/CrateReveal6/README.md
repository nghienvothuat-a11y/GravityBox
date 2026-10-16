# Display 06 — exit beneath the red crate

Revision: 15/10/2026, corrected to the user's latest instruction. Stable ID
`coghe.spatial.plus.k01`, scene `COgheSpatialPlusK01`, same campaign position/saves.
This supersedes the rejected switch → guard → button proposal and the earlier
wall exit for display 6. All other opening levels retain their approved layouts.

## Experience

Move an obstruction and discover the way out. This level practices pushing and
pulling crates, with no new switch or button sequence. Keep the small, legible
crate arrangement and tap-any-face control. No timing challenge.

## Layout and solution

The red crate starts over a real circular floor opening. A tall crate initially
blocks its rail. The third crate remains movable. All crates retain their existing
slick coating and finite-force rails; bare enclosure glass grips normally.

```text
move blocking crate → move red crate → reveal floor hole → escape downward
```

1. Move the tall crate out of the red crate's travel path.
2. Move the red crate aside; the mint-rimmed floor exit is now visible.
3. Guide COghe through the hole, down out of the box as one complete body.

There are no switches, pressure buttons, hinged lids, recesses, gate shutters or
printed control connections. Camera orbit does not change gravity. Glass C
lighting, clean glass, specimen plaque, quiet rails and existing crate colours
remain. The hole mesh, collision and final outlet must coincide, with no opaque
artwork underneath the opening.

## Recovery and invariants

Crates stay reversible; a blocked move is refused immediately. Moving the red
crate back covers the same physical hole. Tapping the hidden hole cannot escape
through the crate. Retry restores the covering position and all 32 particles.
No extra condition is attached to the win: the complete body passes the outlet.
No changes to global movement, adhesion, merge rules or Boss 10.

## Implementation and verification

Reuse `COgheTapRail`, `COgheRailSlider` and the existing crate floor shell. Authoring
and saved scene must match. Check the physical covered/open exit, actual tap solve,
reset, wander/recovery, blocked and reversible moves, neighbouring solves and
Boss 10. Record fresh native-player evidence in
[COgheCrateReveal6](../../../Verification/COgheCrateReveal6/README.md).

Kết quả: 9/9 PlayMode đạt; Mac giải bằng 3 lần chạm, đủ 32/32 hạt và một cơ thể.
