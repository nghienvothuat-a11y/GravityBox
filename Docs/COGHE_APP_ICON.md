# Approved COghe app icon

Mrk approved this icon on 7 October 2026 (Buzz event `1c1fc7af29af812b4ce5558fff75eefe8624d4c0d8ba568f45adc1fbb6ad6e8e`).

Source: `Assets/_Game/Venom/Art/COgheAppIcon.png`, a 1024 × 1024 opaque RGB PNG generated with imagegen. SHA-256: `f9f9bb18feccb6b23608a1b9ecea761b2c6a38c6f0a947aa0c6654287accfed9` (identical to the approved Buzz attachment).

`ProjectSettings/ProjectSettings.asset` assigns this texture as the default icon and to all 19 existing iPhone/iPad/App Store slots. Unity's default slot is serialized as 128 × 128; the source texture remains 1024 × 1024, uncompressed, without mipmaps or alpha. The iOS App Store slot explicitly requests 1024 × 1024. The existing build entry points use these Player Settings.

Validation on 7 October 2026, base `7ce3b074` plus this icon-only change:

- Unity 6000.3.19f1 imported the exact asset and copied Player Settings in an isolated minimal project. `PlayerSettings.GetIcons(NamedBuildTarget.Unknown, IconKind.Any)` resolved the approved texture; dimensions, no mipmaps and uncompressed import were verified. Unity exited 0 and logged `COGHE_ICON_VALIDATED`.
- Checked all 19 iOS texture references, PNG dimensions/opacity, identical source checksum and `git diff --check`.
- This was asset/settings validation, not a game build or gameplay test run. The local Editor has only MacStandaloneSupport; iOS export, Xcode archive and device icon verification remain unperformed.
- The runner reported failure despite Unity exit 0 because the Editor log includes licensing handshake/access-token warnings. The validation method completed successfully; this does not establish iOS build availability.

The square source intentionally has no baked corner mask. See [Apple app icon guidance](https://developer.apple.com/design/human-interface-guidelines/app-icons/) and [Unity PlayerSettings icon API](https://docs.unity.com/ja-jp/engine/6000.3/script-reference/unityeditor/playersettings/seticons).
