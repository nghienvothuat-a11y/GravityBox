# COghe — Chapter 31–40 implementation

Implementation worktree: `codex/venom-macos-preview`, baseline `5fb2609f0407e0871e5d973aa41a7b65af7f3b05` plus the chapter diff. Unity6000.3.19f1; no package/Editor upgrade. Verification completed on 2026-09-21; evidence and limits are recorded below.

## Scope and authoring

The approved [dossiers and illustrations](Sketches31-40/README.md) define the mechanical relationships and intended solution order. Scenes `Assets/_Game/Venom/Campaign30/COgheOrigin31.unity` through `COgheOrigin40.unity` append stable IDs `venom.origin.31`–`venom.origin.40`; scenes01–30 are preserved. The existing campaign selector now has four pages.

Run `GravityBox.Editor.VenomCampaignBuilder.GenerateCampaign40` in the pinned Editor to regenerate only this chapter. New partial builders are `VenomCampaign40Builder`, `VenomCampaign40CooperationBuilder`, and `VenomCampaign40BossBuilder` under `Assets/_Game/Editor/`. Geometry, joints, components and scene references are created with Unity APIs.

The shared rail/force, gear, knife, tissue-sensor, tube and winch systems remain the basis. New components are `COgheTwoDockDeck`, `COgheSpringAccessDoor`, `COgheCooperativeDockTransmission`, and `COgheTwoStageWinch`; none branches on campaign number. Docked bridge surfaces now release when measured effort requests reverse travel, before a static seam cover can trap the moving side faces.

## Physical integration decisions

| Level | Authored behavior and clearance work |
|---|---|
|31|G masks P; retract G, withdraw the actual rail stop, then mesh G to raise E. P stroke clears the entire G envelope.|
|32|One captive bridge serves A then B. Its handle sits beside the walking lane. The moving plate covers the unused portal; dock seams retain physical support.|
|33|T moves paired solid shutters through a16mm sleeve slot. Both pipes are bidirectional; upper elbows deliver onto banks away from the floor bores so tissue can land and return. The slot remains narrower than an18mm particle.|
|34|B and X physically compete for the same bay; X obstructs G. Bank-side grips retain a real supporting surface throughout both strokes. The withdrawn plate still covers the exit-island aperture.|
|35|Held A opens spring-return D; far-side L catches it, freeing the holder. Reunite and pull E.|
|36|L catches D and physically uncovers B. Worker holds B while the former A holder operates C.|
|37|The same G meshes at I to catch D, then at II to power C/E while A stays loaded. Station labels and fixed conduits identify the outputs.|
|38|First split opens D, reunion supplies force to Q, second split operates B/C. Q uses actual mass/resistance; no fragment-count force gate.|
|39|Three independent pieces hold A/B and operate C. I catches both reunion doors before a physical stop permits II. Release C removes motor effort.|
|40|Whole-body Q/P preparation precedes the three-role sequence. A is outside the preparation alcove and away from the waiting worker. II retracts H’s real pin; the reunited body must still pull H.|

The illustrations are not scale drawings. Clearance and handle/camera positions were refined from failed physical and picking tests while preserving the intended relationships. Existing roof picking and natural fusion rules are preserved.

## Verification contract

Full EditMode and PlayMode suites must finish with nonzero counts and no failures. New solution tests are `COgheCampaign40RouteTests`, `COgheCampaign40CooperationTests`, and `COgheCampaign40BossTests`; they issue ordinary movement/grasp/push/pull/pipe commands and step real physics at120Hz. They never assign tissue poses, gate-open state or victory. Win requires one reunited fragment and all32 particles escaped. They also check reset after actual wins.

`COgheCampaign40InputTests` verifies every authored handle and pressure pad in the opening state at720×1280 and720×1612, using actual screen rays and overview or authored room views. Deliberately concealed P and B controls must remain blocked. Picking assertions do not move mechanisms or tissue.

For genuine Development-player framebuffer evidence, run `GravityBox.Editor.COgheChapterProofGenerator.Generate`, allow import, then build with `GravityBox.Editor.VenomCampaignBuilder.BuildMac`. The generator stamps source hashes and rejects stale replay sources at build time. Run the player with `-coghe-chapter-proof-directory <absolute-path>` and optionally `-coghe-chapter-proof-quit`. Each level writes an overview before the real exit command, a victory/HUD image after earned completion, JSON state and SHA256 hashes. Persistence is isolated for the whole explicit replay session. The replay is absent from release builds.

These are automated physical solution and input checks plus Mac player evidence. New-player difficulty, human touch-only playthroughs, mobile frame time/thermal behavior and every possible alternate arrangement remain separate measurements. No test suite proves absence of every possible physics defect.

## Completed verification — 2026-09-21

The tested state is baseline `5fb2609f0407e0871e5d973aa41a7b65af7f3b05` plus the chapter working-tree diff, identified by `Artifacts/CHAPTER40_FINAL_SOURCE_MANIFEST.json`. Its recorded source hashes still matched after the build and player run.

| Gate | Result | Evidence relative to repository |
|---|---|---|
|Full PlayMode|341/341 passed; 0 failed, 0 skipped|`Artifacts/chapter40-playmode-07/TestResults.xml`|
|Full EditMode|8/8 passed; 0 failed|`Artifacts/chapter40-editmode-01/TestResults.xml`|
|Mac Development build|Success; arm64 + x86_64, 40 campaign scenes|`Artifacts/chapter40-build-macos-01/Editor.log`, `Builds/Venom/macOS/Venom.app`|
|Actual Mac player solutions|31–40: 10/10 passed, no captured Error/Exception/Assert|`Artifacts/chapter40-player-proof/20260921T135858138Z/run.json`|
|Per-level real frames|10 before-exit overviews + 10 earned victories, all manually inspected|Same directory: `level-N-open.png`, `level-N-won.png`, `level-N.json`|

The app ran on Mac16,10 / Apple M4, Unity6000.3.19f1. Actual window framebuffer was720×1022 (the desktop constrained the requested720×1280 window). Separate screen-ray contracts cover720×1280 and720×1612. Before exit each level has one reunited fragment, zero escaped particles, an available exit and no loss; every victory has one fragment,32 escaped particles and completion true. Reset-after-win checks also passed. Replay disables save writes for the entire run.

The command replay uses ordinary control APIs with real120Hz physics. It verifies the intended solutions and checked failure/recovery cases; it is not a human touch-only mobile playtest or a mobile performance measurement. Earlier failing runs are retained in `Artifacts/chapter40-playmode-01` through `-06`; the geometry corrections are described above. Runner summary heuristics flagged licensing diagnostics, but actual Unity processes returned0 and the XML/build results above succeeded.

A shareable copy with20 PNGs, per-level state, XML results, source/build SHA256 manifests, logs and an HTML gallery is in `/Users/tommynguyen/.buzz/OUTBOX/COGHE_LEVELS_31_40_PLAYTEST_2026_09_21/20260921T135858138Z/`.

## Đợt rà đường đi 32–40 — 21/09/2026

Các solution test 32–40 hiện đi qua production screen picker, gồm 20 kịch bản
giải/phục hồi ở portrait. Đã bổ sung camera khoang32–35, mặt nhận hướng kéo cho L33
và chốt đầu ray cho G34. Xem [báo cáo mới](../../Verification/COgheCampaign30/LEVELS_32_40_ROUTE_AUDIT_2026_09_21.md)
để lấy kết quả kiểm chứng của bản sửa; các số liệu ở phần trước là lịch sử bản cũ.
