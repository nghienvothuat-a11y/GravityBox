# COghe NewGraphic — Blender source

Editable kit for V2 01–10, authored with Blender 5.2.1 LTS. Gameplay remains in Unity.

- `COghe_Lab_Kit.blend`: bevel modifiers, materials and module collections; laid out as a library, not a playable level.
- `COghe_Lab_Kit.fbx`: portable evaluated mesh library for an artist; its library spacing is intentional.
- `layout.json`: exact local dimensions and sibling-index anchors (duplicate surface names are supported) exported from the approved Unity scenes.
- `build_kit.py`: reproducible Blender authoring/export script.
- `Laboratory_Backdrop.blend` / `build_backdrop.py`: editable distant laboratory and offline blur render. Output is a 768×320 texture, not runtime depth of field.
- `meshpack.json`: evaluated geometry, normals, UVs and material groups in Unity local coordinates.
- `manifest.json`: actual module and triangle counts.

## Rebuild

1. In Unity call `GravityBox.Editor.COgheBlenderExport.Export` using `-executeMethod` to refresh the layout. This opens scenes; save unrelated work first.
2. Run from repository root:

```sh
/Applications/Blender.app/Contents/MacOS/Blender -b --python-exit-code 1 \
  --python ArtSource/COghe/NewGraphic/build_kit.py -- "$PWD"
```

Run `build_backdrop.py` with the same Blender command to regenerate the distant lab PNG.

3. Unity menu **Gravity Box → COghe → NewGraphic → Apply Blender kit to V2 01–10** creates shared Mesh assets and URP materials. It replaces its own visual roots and checks serialized physics before/after.
4. Build **NewGraphic → Build Android A-B test APK**. The test build includes the existing 30-level catalog; only 01–10 have the new profile. The on-screen graphics button reloads the current puzzle and changes presentation. It does not preserve a half-completed mechanism state.

The actual Unity import route is **Blender evaluated mesh → meshpack → Unity Mesh assets**, not Unity's automatic `.blend` import or direct FBX import. This avoids Blender/FBX axis dependencies on build machines. An artist editing the `.blend` manually must update/export the corresponding evaluated meshpack; rerunning `build_kit.py` recreates the library and overwrites manual source edits. Copy an edited source before regeneration.

One Unity unit is one metre. New mesh roots have no collider, Rigidbody or input component. Moving art is parented to the existing mechanism transform. Individual chamber faces retain camera cutaway/fade. Materials and lighting are applied only by `COgheGraphicProfile`, before scene presentation caches its renderers. The creature still uses the original tissue solver and mesh animation.

No realtime reflections, fluid simulation, SSAO, bloom or extra shadow light are introduced. Textures are small procedural material maps. The A/B APK contains both visual sets; its memory footprint is not an isolated old-vs-new shipping APK comparison.
