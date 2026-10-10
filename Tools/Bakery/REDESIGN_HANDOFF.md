# Bakery redesign additions — 10 October 2026

Requested by Mrk in Buzz event `0b7f275d2b7bfb6cbd7f2ee13c96a4de3055ec8928bad325cbba0ab2a82de13f`; asset list from Claude in `d099ddd825287c167ee9d3ac3516d08bbdea0f5832fd8ccc1bc6e4eb6d6a2f12`.
This scoped bakery revision supersedes the historical lab palette for this sample only. Keep presentation separate from physics.

## Files and integration

All runtime inputs are under `Assets/_Game/Venom/Art/Bakery/`. Existing nine FBXs and six textures are unchanged. `materials.json` adds three entries while preserving the existing entries.

| Asset | Dimensions in Unity metres | Pivot | Triangles |
| --- | --- | --- | --- |
| GlazeDrip.fbx | X .100, Y .031, Z .007 | Centre of top edge, top Y=0, drip down to Y=-.031 | 1364 |
| PipedCream.fbx | X .100, Y .01174, Z .01356 | Centre of ground plane; actual bottom Y=.000128 | 1460 |

Both are closed meshes, Y up, X along edge, UV0, triangulated, no colliders. Ends have matching cross-sections for straight 10 cm repeats. End caps remain closed; tuck joins into the coating and do not stretch straight strips around corners. Use separate corner geometry or stop behind a rounded corner. Avoid long-distance nonuniform scaling that distorts drip thickness.

- `Glaze lilac`: RGB (.62,.43,.79), metallic 0, smoothness .82, opaque URP Lit. Body coating and drip must share this material. It is blueberry mirror glaze over cake, not transparent jelly. Cover the entire slippery contact face continuously; the drip alone is not a complete surface coating.
- `Piped cream`: RGB (1,.91,.76), metallic 0, smoothness .32. Keep its raised geometry outside walking/contact lanes and mechanism sweeps. This decorative edge does not replace the broad rounded cream cap.
- `Sponge`: RGB (.94,.66,.32), three 512px repeat textures. BaseColor is a neutral multiplier in sRGB; Normal is OpenGL/import as Normal Map, scale .65; Mask is linear, R=metallic 0 and A=smoothness. Set material smoothness multiplier to 1 when using the Mask. Start with one UV repeat per .10–.15 m; tune at gameplay camera rather than magnifying pores to craters.

## Shape targets for the rebuild

These are proposed art dimensions, not measurements of an implemented scene:

- Floor: approximately 4 cm cake thickness extending below the existing contact height; continuous playable top with shallow portion seams. The outer cut face shows sponge and cream filling. No exposed deep gaps under COghe.
- Cake bodies: visible roundness at normal portrait framing; start with 8–12 mm corner radii for 10–20 cm bodies, then constrain the bevel to the collision/contact envelope. Avoid large invisible collider corners or decorative overhangs inside routes.
- Cream cap: rounded 8–12 mm visual thickness on a sufficiently thick cake, integrated with the silhouette. For a fixed top contact height, put its thickness below that height. Avoid a thin square slab resting on a purple block.
- Slippery cake: continuous lilac glazed side, rounded crown, inset cream top only where grip exists. Sponge layers are internal, visible on a separately labelled cutaway, or on outer nonplayable cake faces. Never imply climbability with a naked sponge band on a slippery wall.
- Retain the actual gear teeth and sweeps. Biscuit colour/icing may decorate them, but the concept does not authorize a different gear mechanism.

## Evidence and limits

Run `Blender -b --python Tools/Bakery/build_bakery_redesign.py` from any directory with the script path resolved. Source generation is deterministic and regenerates only these additions plus their manifest entries and report/preview.

`REDESIGN_VERIFICATION.json`: both FBXs pass Blender export/reimport checks for bounds, UV0, triangle count, closed manifold, outward volume and nondegenerate triangles. Combined new geometry: 2824 triangles. `REDESIGN_PROPS_PREVIEW.png` is a Blender prop preview, not Unity or gameplay evidence.

Unity importer checks, materials, GUID creation for new assets, portrait renders, contact alignment, full regression and phone profiling remain integration work. No runtime code or scenes changed in this delivery. Do not transfer earlier Unity test results to this revision.
