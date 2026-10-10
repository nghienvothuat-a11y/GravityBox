# Bakery 49 authored asset kit

Authorized by Mrk in Buzz event `c406ae7fff60e7da1ddc89c011115557ef893c35a2fd67276a5c4ca0bb4d5490`.
Integration contract from Claude: `cd7c612d9bfbdc1a5cabb8863481923c50ddf599c6757a43105ba0a2c997c594`.
This is an isolated bakery sample, superseding the historical glass art direction only for the approved sample.

Original meshes authored locally by Codex using Blender 5.2.0; no downloaded assets, AI provider outputs or external texture licenses.

## Integration

Runtime source files live in `Assets/_Game/Venom/Art/Bakery/`. FBX is triangulated, metres, Y-up, with UV0 and authored normals. Object transforms are applied. Let Unity create and retain importer `.meta` files; no collider generation, animation, cameras or lights. Use shared URP Lit materials from `materials.json`, metallic zero, opaque rendering. Names of FBX material slots match the manifest. Colors are intended as Unity material inputs, not measured screenshot colors.

| File | Dimensions in metres (X/Y/Z) | Triangles | Placement |
| --- | --- | ---: | --- |
| Cherry | .0294/.0446/.0273 | 1000 | Base at origin; sole win target visual |
| Plate | .9715/.075/.7733 | 1152 | Bottom at origin; inner floor Y=.008; rim crest Y=.075 |
| PressurePad | .100/.0115/.100 | 576 | Bottom at origin; flat top, no baked activation state |
| CandyHandle | .020/.0412/.0069 | 1364 | Stick base at origin, disc faces local ±Z |
| GummyLamp | .030/.018/.030 | 192 | Bottom at origin; neutral geometry, state color set by gameplay |
| CreamDrip | .100/.022/.005 | 260 | Exception: anchor is below strip, top Y=.025, bottoms Y=.003–.012; faces −Z |

One of each totals 4,544 triangles. Actual scene cost depends on instance counts and material batches. These counts are not an FPS benchmark.

Plate is a rounded rectangular dish to leave more room at gameplay corners than an ellipse. Its visible rim is continuous, rising 67 mm above the inner floor. The asset does not prevent climbing: Claude owns the physical barrier, slippery surface, navigation and corner-clearance checks. Align the plate inner floor with the actual floor; do not lay two opaque floors at the same height. Do not infer physical clearance from its outside dimensions.

Keep the pressure-pad collision and real measured load activation with the existing mechanism. Attach visual moving parts to their actual owners. Match control colors to circuit/output identities when assigning materials; the strawberry palette is a default art swatch. Do not scatter cherry duplicates. CreamDrip has matching end profiles and should only skirt non-walkable edges, never cover the purple surface boundary.

## Reproduction and evidence

Run Blender in background with `--python Tools/Bakery/build_bakery_props.py`, then `--python Tools/Bakery/verify_bakery_fbx.py`.

`FBX_VERIFICATION.json` records a fresh FBX reimport checking dimensions, triangles, UV0, finite vertices, closed manifold edges, outward signed volumes and nondegenerate faces. `mesh_audit.json` records source-space bounds after Y-up conversion. All six passed at base `c7aa1d22` plus these asset files.

`PROP_PREVIEW.png` is a Blender asset inspection render, with small props enlarged independently for legibility. It is not a Unity frame, relative scale reference, lighting bake or proof of mobile performance. The generated `.blend` is a disposable inspection scene; source authority is the script and shipped FBX. Unity import/material assignment and live level evidence belong to the integration worktree, controlled by Claude.

## Biscuit material textures

Run `build_bakery_textures.py` in Blender after the mesh generator. It adds six 512×512 PNGs and texture paths to `materials.json`. Mesh regeneration rewrites the base manifest, so always run textures second.

BaseColor is a neutral pore/grain modulation multiplied by the manifest color; import sRGB. Normal is tangent-space OpenGL (+Y), import as Normal Map without green-channel inversion, suggested bump scale .35. Mask is linear data, metallic in R (zero) and smoothness in A; preserve source alpha. Use Repeat, mipmaps and a maximum of 512. With a mask assigned, URP `_Smoothness` must be 1 because the texture already contains the final smoothness; without the mask, use manifest smoothness. Enable the normal and metallic-map shader keywords when assigning maps by script.

Use the Biscuit material on the existing gear mesh; preserve its teeth, rotation and collision. Current color maps intentionally keep pores subtle at gameplay distance. These are synthesized surface detail, not a lightmap or baked scene shadow. Final texture filtering/compression and material response require checking in the Unity player. No new shader or render pipeline feature is required.

## Rim toppings, second pass

Run `build_bakery_toppings.py` after the other generators. It adds `Sprinkle.fbx` (168 triangles, 9 × 2.6 × 2.6 mm), `ChocolateChip.fbx` (160 triangles, approximately 14 × 10 × 14 mm), and `MintLeaf.fbx` (176 triangles, 27 × 2.3 × 12 mm). Combined source budget is 504 triangles. A composition with 18 sprinkles, 6 chips and 3 leaves adds 4,512 triangles; scene cost depends on instance counts. Batch static instances by material. Scale is metres/Y-up as with the first batch; the thin leaf anchor is on the support plane with a 0.6 mm air gap at its tips. No colliders.

Keep these outside the playable floor and away from the cherry goal and input handles. Use sparse clusters on the outer rim; none should resemble a reachable collectible. Sprinkles use the shared Strawberry slot (Cream or Wafer can be substituted for restrained variation), chips use Chocolate, leaves use MintLeaf. `TOPPINGS_VERIFICATION.json` records successful closed-mesh/UV/triangle FBX round trips. `TOPPINGS_PREVIEW.png` is a Blender inspection image at uniform 20× magnification, not a Unity frame.

The manifest also adds optional `Grape jelly` with color (.58,.43,.78), smoothness .76. It aims for a less saturated lavender with broader highlights than the initial .9 gloss. Use it as an art adjustment subject to the integrated lighting comparison, not a measured fidelity improvement. The original six assets are unchanged by this second-pass generator.
