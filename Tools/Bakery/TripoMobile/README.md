# Tripo bakery reduced mesh comparison

These are local comparison candidates for Claude's N41 integration, requested in Buzz event `91f18f9e61644dcc9896f106c0745062da8bc9f329ef80afe9f0e8b7bae1c446`. They are not campaign-ready modular assets. Source FBXs remain unchanged. No additional Tripo generation or conversion was purchased by this cleanup.

## Files and measurements

Runtime inputs: `Assets/_Game/Venom/Art/Bakery/TripoMobile/` in the `GravityBox-bakery-assets` worktree. Copy this directory only, preserving existing destination meta files if updating it. These files are uncommitted; no integration branch was changed.

| Mesh | Source triangles | Exported triangles | Width × height × depth in Unity metres |
| --- | ---: | ---: | --- |
| CreamBlock.fbx | 13563 | 2956 | .100013 × .081212 × .104527 |
| GlazeBlock.fbx | 13615 | 2712 | .100015 × .051099 × .087861 |

Combined exported geometry is 5668 triangles, approximately 79.1% below the two source meshes. Dimensions preserve source proportions at approximately 10 cm width. FBX uses metres, Y up, bottom-centre pivot, one mesh, one material slot, UV0 and no collider. Blender round-trip uses Z up and confirms bottom height within floating-point tolerance; Unity import remains Claude's responsibility.

Every delivered PNG is 1024 × 1024. For Unity URP Lit use the basecolor, normal and Mask maps; separate roughness and metallic maps are retained for comparison/rebaking, not additional required runtime maps.

- Basecolor: sRGB, opaque material, base colour white. Ignore texture alpha for transparency.
- Normal: tangent-space OpenGL, import as Normal Map. Check highlights in Unity after tangent generation.
- Mask: linear, R metallic, G 1, B 1, A smoothness = 1 − roughness. Set smoothness multiplier to 1 and source to metallic alpha.
- Share the two materials and textures across instances. Choose mipmaps and platform compression in Unity; source PNG dimensions are not a measured runtime-memory figure.

## What changed and what remains

The script reduces geometry, caps boundary loops, removes degenerate geometry, rebuilds UVs, bakes base colour/tangent normal/roughness/metallic from the original, pads atlas edges and exports FBX. Whole-object voxel remeshing was rejected because the source openings produced thin shells and invalid faces. Earlier trials are under `.scratch/tripo/codex_trials/` and are not integration inputs.

CreamBlock now has zero boundary and nonmanifold edges after FBX round-trip. GlazeBlock still has 4 boundary and 10 total nonmanifold edges after export, including source-derived topology that simple capping does not resolve. Neither exported mesh contains zero-area triangles at the audit threshold. The report records any FBX triangle-count change explicitly. Do not call both meshes watertight or fully repaired.

The four PNG previews are Blender renders with the same light/camera setup before and after cleanup, not Unity captures. They show retained broad silhouettes and removed large open gaps in the cream body. Small dark seams remain around toppings and the inset cream surface; review these at gameplay distance. The glaze candidate retains residual topology issues and is for comparison, not a production sign-off.

CreamBlock's height includes raised toppings. It is not a flat walkable tile: fitting its entire bounds to floor thickness puts the cream surface below the collider. Keep contact height separate from decorations. GlazeBlock's inset cream crown also needs a contact-alignment check. Neither rendering cleanup nor keeping colliders unchanged proves visible contact remains correct.

No corner/edge/centre module kit is delivered. The asymmetrical toppings, crown and unique atlas need deliberately matching cuts, end profiles and new UVs before modular use. Stretching either mesh into the .92 m perimeter bar will distort bevels, icing and texel density. Use designed-size variants or authored modular geometry; these atlases are not seamless textures for arbitrary procedural boxes.

The text-generated bar and pedestal are audited but excluded from cleanup because their shapes/content differ from the requested assets. This describes these samples, not all text-to-3D outputs.

## Verification and rerun

`audit.json` contains source SHA-256 hashes, conversion and generation task IDs, source topology, cleaned topology, FBX round-trip bounds, UVs, referenced texture sizes and remaining defects. Saved conversion task metadata records only the conversion cost; the 220-credit total for all four generations is Claude's reported experiment total, not independently reconstructed here.

Run from the worktree with Blender 5.2:

```sh
/Applications/Blender.app/Contents/MacOS/Blender -b --factory-startup --python-exit-code 1 --python Tools/Bakery/clean_tripo_models.py
```

The script reads the four `.scratch/tripo/<name>/tripo-out/*/model.fbx` inputs. Archive existing output/evidence directories before rerunning: it refuses to overwrite them. Keep the original source files for repeatability; `.scratch` alone is not durable source storage.

No Unity scene, collider, physics code or gameplay tests changed here. Claude owns the same-camera Unity comparison, import verification and full regression. Mobile performance is unmeasured; triangle counts alone cannot establish whether a phone runs the scene acceptably.
