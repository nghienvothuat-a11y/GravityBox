# Five snack meshes — 21 October 2026

Requested by Claude in Buzz event `63c73d9196303b6a6b71a553428a39b1656417588799a8b912bbf2411be67e65`, following Mrk's five-snack prototype scope. Built from c5b7f108 on codex/coghe-snacks-art. Codex supplies meshes; Claude owns runtime reactions, economy, tray and Unity icons.

## Runtime contract

Each ID below is one FBX with one mesh and multiple material slots at `Assets/_Game/Venom/Resources/COgheSnacks/<id>.fbx`. Load path omits Resources and extension. No extra FX meshes, textures, animation, collider or lights. All opaque. Materials follow `Snack_<Label>_<hex sRGB>`; parse final six hex digits as sRGB with the existing game material system. Blender shader RGB is the corresponding linear value. Use existing game lighting; studio preview lights are not exported.

| ID | Triangles | Bounds in cm (X / depth / height) | Materials |
| --- | ---: | --- | ---: |
| SNACK_APPLE_PIE | 174 | 2.8 / 3.4 / 2.2 | 5 |
| SNACK_CHILLI_CANDY | 184 | 3.4 / 1.62 / 1.62 | 3 |
| SNACK_ICE_CREAM | 208 | 3.0 / 2.85 / 3.8 | 3 |
| SNACK_LEMON_DROP | 138 | 3.2 / 2.13 / 1.6 | 3 |
| SNACK_STRAWBERRY_CAKE | 194 | 2.8 / 3.4 / 3.3 | 4 |

Units metres, FBX exported -Z forward / Y up; pivot is horizontal bounding-box centre at bottom, all mesh geometry above base. Bounds table uses Blender source X/Y/Z (height Z); Unity conversion puts height on Y. Set runtime root scale to one. Cone is balanced on its tip for a clear familiar silhouette; this is a cosmetic asset, not a physical object.

Distinct silhouettes: pastry slice with lattice and apple garnish; red bonbon with flared green wrapper ends; pink scoop and golden cone; flattened yellow lozenge with leaf; layered cream/jam/sponge slice topped with strawberry. Deliberate low-poly facets. Mesh is joined for easy whole-food scaling/eating; reactions remain code-owned.

## Source and verification

Original procedural geometry authored for this task; no downloaded assets, textures, third-party model license or provider credits.

- `Tools/Snacks/build_snacks.py`: reproducible generation, export, FBX reimport assertions and studio rendering. Run `blender -b --python Tools/Snacks/build_snacks.py` from repo root. Blender may return exit 0 on assertion failure: require `SNACKS_VALIDATED` in output and inspect audit files.
- `ArtSource/COghe/Snacks/COghe_Snacks.blend`: editable models arranged side-by-side for inspection. These layout offsets are only in the source preview; exported FBXs are individually at zero.
- `mesh_audit.json`, `fbx_roundtrip_audit.json`: all five <300 triangles, bounds preserved, pivot zero, finite vertices, nondegenerate triangles, material names preserved. No modifiers requiring Unity evaluation.
- Individual PNGs and `SNACKS_PREVIEW.jpg`: Blender studio previews inspected, not in-game evidence or final icons. Sheet order matches the table above.
- `.meta` files derive from the existing gift importer with unique GUIDs and animation/camera/light/blendshape import disabled. Preserve on merge. Actual Unity importer results remain to be checked.

## Claude integration checks

Import with Unity 6000.3.19f1 / current URP. Verify each runtime loader ID, 1x physical scale, Y-up/base pivot and colour parsing (including any importer name suffix); inspect game lighting and all five 256px icons. Check five eating reactions and no clipping against COghe/room. Source audit does not establish Unity render appearance, batching, mobile FPS or gameplay test results. Unity was not opened and no runtime code or gameplay tests were changed in this worktree.
