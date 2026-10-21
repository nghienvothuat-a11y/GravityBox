# COghe repository rules

Before designing, adding or changing COghe levels, mechanisms, navigation or
performance-sensitive gameplay, read and follow
[COGHE_LEVEL_DESIGN_RULES.md](Docs/COGHE_LEVEL_DESIGN_RULES.md).
Use [LEVEL_TEMPLATE.md](Docs/LevelDesign/COghe/LEVEL_TEMPLATE.md) for each new level
dossier. Preserve existing approved designs and record changes and verification.
The workflow does not require reapproval of work already authorized by the user.
Its current Origin gameplay rules supersede historical Journey/lab proposals,
including old merge cooldowns and task-based merge restrictions.

Before changing COghe models, materials, shaders, lighting, camera presentation,
HUD or level decoration, read `Docs/ArtDirection/COghe/STYLE_RULES.md` and follow it.
For Spatial Pilot 01–10 and new Spatial 11–30, the approved reference is Glass C
with printed circuits and quiet satin guides, documented at the top of STYLE_RULES
and in Docs/Verification/COgheSpatialCircuits. Original Unity Day Lab level 07
remains the historical Origin baseline. Later scoped rules take precedence over
older V2/Blender proposals; concept paintings do not override runtime contracts.
Keep presentation separate from puzzle physics and state.

Spatial glass rule (14/10/2026): bare glass always grips, including walls and roof.
Only visible slick coatings remove adhesion; their meshes must match `SlipRegions`.
This supersedes the 09/10 non-climbable-glass proposal. Opening display levels 1–5
have wall exits; display 6 (K01, user correction 15/10/2026) has a floor exit
under the red crate, with no switches, buttons or exit shutter. Opening 1–6 have
no floor perimeter tape or auxiliary grip boards; display 7 uses
bare glass at its switch and coatings to prevent bypassing the pulley bridge.
Keep authoring and saved scenes consistent; verify solves and wall/roof shortcuts.
See `Docs/Verification/COgheGlassRule/README.md`.

For Spatial 11–30, start with
[IMPLEMENTATION_HANDOFF.md](Docs/LevelDesign/COghe/SpatialNext20/IMPLEMENTATION_HANDOFF.md).
The quantum splitter and swing transfer are planned mechanisms, not existing
runtime features. Do not claim those scenes are playable based on document checks.

The main Spatial campaign has 65 levels in `VenomCampaignBuilder.SpatialOrder`, six chapters
(10, then 5 × 11), each closed by its boss at 10/21/32/43/54/65: the pilot, Spatial 11–30,
the Spatial Plus levels, the crate levels K01–K10, the glass-crawl levels G1–G5
([GlassCrawl5](Docs/LevelDesign/COghe/GlassCrawl5/README.md)) and the glass-road levels G6–G11
([GlassRoads](Docs/LevelDesign/COghe/GlassRoads/README.md)), which replaced 08, G1, 07, N41, K08 and K10 at
8/11/25/27/59/63 on 19/10/2026 (those stay in code and saves, out of the order). For Spatial Plus start with
[SpatialPlus20](Docs/LevelDesign/COghe/SpatialPlus20/README.md) and its
[verification](Docs/Verification/COgheSpatialPlus/README.md). The catalog also contains two post-campaign prototypes, F2/F3 (67 entries total).
On 21/10/2026, F1 (old display 66) replaced N29 at display 15; N29 remains archived
by stable ID, outside the order. See `Docs/Verification/COghePlaytestRefinement`.
New levels get a
solve test and a wander (stuck) test in `COgheSpatialPlusRecoveryTests`.

This applies to the twenty Venom Origin scenes and future COghe levels. Archived
Venom control experiments and the Steel Ball Lab retain their existing art unless
the user explicitly includes them. Preserve existing work outside the task.
