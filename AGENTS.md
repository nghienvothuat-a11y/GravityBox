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

For Spatial 11–30, start with
[IMPLEMENTATION_HANDOFF.md](Docs/LevelDesign/COghe/SpatialNext20/IMPLEMENTATION_HANDOFF.md).
The quantum splitter and swing transfer are planned mechanisms, not existing
runtime features. Do not claim those scenes are playable based on document checks.

The Spatial catalog is 65 levels in `VenomCampaignBuilder.SpatialOrder`, six chapters
(10, then 5 × 11), each closed by its boss at 10/21/32/43/54/65: the pilot, Spatial 11–30,
the Spatial Plus levels, the crate levels K01–K10 and the glass-crawl levels G1–G5
([GlassCrawl5](Docs/LevelDesign/COghe/GlassCrawl5/README.md)). For Spatial Plus start with
[SpatialPlus20](Docs/LevelDesign/COghe/SpatialPlus20/README.md) and its
[verification](Docs/Verification/COgheSpatialPlus/README.md). New levels get a
solve test and a wander (stuck) test in `COgheSpatialPlusRecoveryTests`.

This applies to the twenty Venom Origin scenes and future COghe levels. Archived
Venom control experiments and the Steel Ball Lab retain their existing art unless
the user explicitly includes them. Preserve existing work outside the task.
