# Mystery gift art handoff — 21 October 2026

Owner: Codex asset + visual timing; Claude runtime integration + gameplay validation.
Source request: Buzz event 3de6538d7fc3f4c5be3c4354c4c80263f6c9cdbaf34de02c23ed17e16541a115.
Integration contract: Claude event b832c4992d7949df839cb8b09cec0546dcbb63532bcb039b92fd2013e2482695.

## Asset

`Assets/_Game/Venom/Resources/COgheMysteryGift/COghe_Gift.fbx` (moved there by Claude; `build_gift.py` exports to it) is loaded at runtime by `COgheMysteryBox`.
Editable Blender scene and reproducible `build_gift.py` accompany it. No Tripo credits spent.
Budget superseded by Mrk event 6fae4f41ce79543f5956ab804179d29a67e572649222e84e9a43dbde06d170f5: **total box under 500 triangles**, including body/lid/bow.
Body 5.2 cm wide (5.41 cm including ribbon thickness), cap 5.7 cm, bow peak approximately 7 cm above base.
FBX exported -Z forward / Y up, metres, body pivot at bottom centre;
lid pivot at its bottom centre (3.85 cm above body origin). Bow child follows lid.
Ribbons use two separated thin surfaces, intentionally open edges; do not add Solidify/subdivision during import.
Three opaque material names: Gift_Coral, Gift_Peach, Gift_Champagne. No textures.
Import to Unity with material remaps to URP/Lit; do not assume Blender materials import identically.
Suggested smoothness: coral .58, peach .62, champagne .65, gold metallic .25.
Use the game's existing key/fill/reflection setup. No new lights or postprocessing.
`mesh_audit.json` records triangle counts; `fbx_roundtrip_audit.json` verifies exported mesh counts, finite/nondegenerate triangles, dimensions and bow parent. Run `verify_gift.py` with Blender; inspect its output because Blender can exit 0 on a Python assertion failure. Preview studio lights are NOT runtime lights.
Body is hollow and has a floor; ribbon bands stop at the mouth. Cap can detach cleanly.

The previous screenshot shows hard-edged box faces, filled oval bow lobes and a solid
brownish floor disk. This version replaces them with broad bevels, ribbon loops with
openings, draped ends and three coordinated colours. Remove old opaque halo entirely.
A very faint mesh glint at the cap seam can suggest hidden contents; no floor disk.

## Motion storyboard (unscaled seconds, independent of gameplay pause)

Use a cosmetic pivot under the existing placement root; preserve the normal-aligned
root for wall/raised placements. Dimensions below are multiples of body width W=.052m.
Keep all transforms relative to captured resting pose; never accumulate deltas.

| Phase | Time | Action and easing |
| --- | --- | --- |
| Drop | 0–.48 | Fall from +3W along support normal, ease-in quadratic; yaw from -18° to 0°. Bow follows lid. |
| Land | .48–.60 | Pivot scale (1.10,.87,1.10), then rebound toward (.96,1.07,.96). Body base stays at support; no negative penetration. |
| Settle | .60–.86 | Damped ease to (1,1,1), cap secondary tilt 4° → -2° → 0°. |
| Idle | 3.2 s cycle | First 2.35 s quiet. Then tilt +3°/−3° over .30 s; cap rises .018W and settles over .35 s. Last .20 s quiet. Never expose reward. Two small glints only during the wiggle. |
| Approach focus | .65 s | Smoothstep camera into a 3/4 view, gift 25–30% viewport height, COghe beside it. Wait .15 s at rest before offer. Frame bow and body, not just cap. |
| Open anticipation | 0–.24 | Gift compresses to (1.06,.92,1.06); cap shivers ±3°, seam glint ramps. This begins only after reward success. |
| Release | .24–.55 | Cap/bow rises .85W, sideways .20W away from creature; cubic ease-out, max 18° tilt. Body returns to unit scale with one soft overshoot. No tumbling. |
| Reveal | .40–.90 | Reward icon emerges from mouth .10W → 1.25W, scale .25 → 1.08 → 1.0. Use real 256px item/drop icon and a restrained ivory backing. No mystery content shown before success. |
| Celebrate | .50–1.15 | Two short side bursts, 10–14 bounded mesh confetti total, warm gold/coral/ivory; gentle outward arcs, no full-screen flash. COghe small happy bounce/look-up, silhouette remains readable. |
| Hold | .90–1.70 | Icon steady; actual item name or drop amount shown. Cap remains suspended offset, body stays visible. Let player see what they earned. |
| Receipt | 1.70–2.00 | Transition reward icon into existing Surprise popup with ease-out; gift fades/scales away after receipt takes over. Never pay again at animation completion. |
| Skip | .45 s | Whole gift shrinks gently and lifts 1W, no confetti or reward; restore camera/input via existing flow. |

Use sine-out for short reveal overshoot; cubic-out x=1-(1-t)^3 for release;
smoothstep x=t*t*(3-2*t) for camera and settling. New timing needs the UI coroutine
adjusted: old box lifetime is .9s. Do not let old timeout remove this asset mid-reveal.
A billboard reward icon gives both currency and furniture equally clear presentation.
Do not invent a 3D reward that differs from the actual granted item.

## UI composition

Offer below gift and face, with one clear Watch ad to open button and visible Skip.
Do not stack the modal over the bow. Preserve ivory panel/teal action/Manrope typography.
No rarity labels, fake jackpot colours, countdowns or promised unknown contents.
The close-up should keep foreground frame rails out of the box/face silhouette through
existing presentation cutaway; never delete their colliders or alter level paths.

## Integration acceptance

1. Import in Unity 6000.3.19f1, URP 17.3.0, linear colour; inspect metres, normals,
   child transforms, material slots and cap seam in the actual camera.
2. Capture closed, anticipation, cap released, reward hold and receipt in portrait.
   Compare 3/8/13 at identical camera states; no rail across face or bow.
3. Record real 30fps portrait clip: land → approach → offer → reward → next control.
4. Check Skip, cancellation, unavailable-ad fallback, replay/opened box, and reward
   once-only persistence using existing owner tests. Verify physics unchanged.
5. Whole affected test package plus placement checks remain Claude's responsibility.
   Blender renders and mesh counts do not establish Unity appearance or phone FPS.

## Final source validation

Final asset: **468 triangles** = body/ribbons 132 + lid 44 + bow/lid ribbons 292.
Blender FBX round-trip preserves these counts, the three mesh objects, three unique materials and bow-to-lid parent. Finite-vertex and positive triangle-area assertions pass. Final closed/open studio renders visually inspected. Unity import and runtime validation remain pending.
