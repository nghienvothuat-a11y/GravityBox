# COghe V2 — Simultaneous cooperation, levels 23–30

Authorized by Mrk on 28 September 2026, Buzz event `6e0465c806f74f6bb00658db11fd75fcfc8138f1dff6a95c489967d236738474`. Replaces the earlier independent handles / held-pad arrangements in these eight scenes. Scene GUIDs, content IDs, progress keys and the 30-level catalog stay stable. The original dossiers retain their historical verification; current dossiers are in [SimultaneousR2](LevelDesign/COghe/ViewV2/SimultaneousR2).

Implemented and verified on Mac: **521/521 PlayMode, 8/8 EditMode, 30/30 accelerated native routes and 8/8 normal-loop routes for levels 23–30**. Every route finishes with all 32 particles in one body. See the [final evidence and limits](Verification/COgheSimultaneous/README.md).

| Level | Cooperative action | Retained result / reunion |
| --- | --- | --- |
| 23 | A and B continuously pull opposite wall grips | Shared wall shutter rises and catches; reunite on the floor |
| 24 | A releases wall brake while B drives the bridge from the departure bank | Bridge catches; reunite and cross |
| 25 | B uses the two-way tube to reach the far-bank bridge handle while A holds the brake | B can return over the caught bridge |
| 26 | A and B supply a paired piston lift from rear and side walls | Raised deck joins fixed ramp to the wall exit landing |
| 27 | Two grips around a corner pull the same shutter | Mechanical catch permits both to leave |
| 28 | Paired inputs lift the departure landing, then T selects the exit-span lift | First output remains caught while the second is raised |
| 29 | Brake and bridge first, then C/D pull the wall shutter on the receiving side | Bridge lets the former brake operator join the far pair |
| 30 | Brake and bridge, then upper-left C and right-wall D raise the upper span | Worker returns on the low bridge in front of the lift; reunite on the upper departure landing |

## Continuous ownership and physical output

`COgheTapRail.HoldAtEnd` adds an optional spring-held task. A real command owns one actor; the task checks planted feet on its configured surface, hand reach, unchanged command and live tissue count. It applies capped hand effort and the opposite reaction to tissue. The spring returns when the actor leaves. Selecting the other actor preserves the first task; issuing a new destination cancels that actor's task. Ordinary non-held rail tasks keep their existing rules.

`COgheCooperativeDrive` reads both current inputs after the tasks have processed contact, cuts and merged commands. Twin pulls and paired valves need both spring grips to be supported at their ends. The brake bridge needs A's continuous hold and B's real drive task. Output motion uses bounded rail force. A settled end stop catches only while both roles overlap; one body visiting A and then B cannot store a half-completed input. Catches release the held commands. No minimum timer changes the 32-particle topology or prevents merging.

Before a catch, missing pressure returns the door/lift; a conservative tissue sweep can stop its return without opening the exit or granting a catch. Missing brake release stops the bridge and leaves the worker waiting; reholding resumes its command. The full final aperture stays navigation-blocked until the output and its real shutter clear. A separate fragment exiting still loses under the existing reunion rule.

## Readability and authoring

Amber grips, visible pulley/pressure branches, two input lamps and an actual moving catch pin expose the shared result. Twin pulls tilt the balance for unequal input, brakes have separating jaws, and piston rods follow measured deck displacement. The final catch plays an original short synthesized click, stored with its import settings. T and Boss C/D have visible pins and linkages that retract only when the earlier rail has reached its catch. Decorative links have no collision or input surfaces. The builder checks the physics/input fingerprint before and after Day Lab art.

`COgheSimultaneousBuilder.cs` authors only 23–30 through Unity APIs. Geometry uses real wall apertures and grippy approach strips. The pressure route uses a closed ramp wedge; receiving lips match the 26 mm dock thickness, and adjacent pistons have clearance while one output is already caught. Held handles remain visible independently of fading outer panes. Boundary grip/exit overlays use the same cutaway and outside-ray picking convention as their wall; their collision and adhesion stay active while hidden. `COgheSimultaneousScenario` is an author-only touch route; gameplay and Boss HUD never read its solution.

## Verification scope

Full PlayMode and EditMode, native Mac touch routes at normal speed for23–30, full30-level accelerated replay, portrait layouts, pause/retry, one-body sequential attempts, reversed inputs, interrupted pressure/brake, selector retention, safety obstruction, unequal cuts, held-body merging and early wall exit. Each final result must reference the candidate's source and binary manifests. Mac author replay does not certify phone performance or first-time player comprehension.
