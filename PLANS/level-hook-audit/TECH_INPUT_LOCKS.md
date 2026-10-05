# COghe: tube taps, lock/gate parts and the black button at position 13

- **Repo:** GravityBox, branch `NewGraphic`, HEAD `dee018ca` (2026-10-04). The working tree was clean.
- **Audit:** read-only, 2026-10-05.
- **Method:** I read the code, docs and images. I also used Python scripts to scan the serialized `.unity` YAML for component GUIDs, material GUIDs and hierarchy order. I did not run Unity and did not run any tests. Nothing below has been checked at runtime.

**Labels:**
- **[C]** Confirmed by reading code, a scene or an asset.
- **[H]** Hypothesis or inference, not run.
- **[D]** Design proposal.

**Path shorthand** (all paths are relative to the repo root):
- `Runtime/` = `Assets/_Game/Venom/Runtime/`
- `Editor/` = `Assets/_Game/Editor/`
- `Tests/` = `Assets/_Game/Tests/PlayMode/`
- `Scenes/` = `Assets/_Game/Venom/SpatialCampaign/`

---

## 0. Play position and content key

`SpatialOrder` is defined at `Editor/COgheSpatialCampaignBuilder.cs:22-27`. Position = index + 1 (`:30`). Numeric keys map to the scene `COgheSpatialNN`; E and B keys map to `COgheSpatialPlus<KEY>` (`:28`).

| Positions | Keys |
|---|---|
| 1–10 | 01 02 03 04 05 06 07 08 09 10 |
| 11–20 | 11 12 **E01** 14 E02 15 16 E03 17 20 |
| 21–30 | E04 18 E05 19 21 E06 22 13 E07 B1 |
| 31–40 | E08 23 E09 24 25 E10 E11 26 27 30 |
| 41–50 | E12 E13 28 E14 29 E15 E16 E17 E18 B2 |

**Builders:**
- 01–10: `BuildSpatial` in `Editor/COgheSpatialCampaignBuilder.cs:69-117`.
- 11–30: `Next<NN>` in `Editor/COgheSpatialNextBuilder.cs`.
- E and B levels: `Plus<KEY>` in `Editor/COgheSpatialPlusLevels.cs`.

All 50 levels set `ViewOnly=true`: the box stays fixed and dragging orbits the camera (`COgheSpatialCampaignBuilder.cs:77`, `COgheSpatialNextBuilder.cs:46`, `COgheSpatialPlusBuilder.cs:81`).

---

## 1. Tube tapping

Producer request: "In tube levels, tapping anywhere on a tube should make COghe enter it, not only the tube mouth. Where tubes branch in several directions, tapping the branch on one side should send COghe into that branch."

### 1.1 How a tap becomes a command today [C]

1. **ViewOnly input path.** Touch and mouse input go through `ConsumeViewTouches` / `ConsumeMouseInput` (`Runtime/VenomCampaign.cs:579-585`, `Runtime/VenomCampaignInput.cs:52-103`). A release with less than 10 px of movement (scaled to a 540 px width) counts as a tap and calls `TouchPoint` (`VenomCampaign.cs:557,563,601`).

2. **`TouchPoint`** (`VenomCampaign.cs:601-699`) runs these steps in order:
   1. **Tissue pick.** It finds the nearest particle within 4.2 cm of the ray (`:606-611`). In ViewOnly it switches fragment only when there are several fragments and that particle is in front of `obstruction + 4.2 cm` (`:649-653`).
   2. **Obstruction.** It raycasts all colliders (`:627`). `obstruction` is the distance of the first hit that is not creature tissue (`VenomContact`) and is pickable (`:643-648`). Any collider without a `VenomSurfacePatch` counts as opaque (`CanPick(null)` returns true, `:637`).
   3. **Mechanisms.** It calls `TouchMechanism(ray, obstruction)` (`:663`). This loops over `Mechanisms` in hierarchy order and **the first `TryTouch` that returns true wins** (`Runtime/VenomCampaignExpansion.cs:80-85`). The order comes from `GetComponentsInChildren`; only `COgheCooperativeDrive` is moved to the end (`VenomCampaignExpansion.cs:18-24`). **No mechanism compares distances with another.**
   4. **Surface pass.** Only after that does `PrepareTapCommand` cancel the old rail task (`:664`, `Runtime/VenomCampaignTapCommands.cs:9-18`) and the surface pass run (`:672-695`):
      - A surface patch hit becomes `MoveTo` (`:689-693`).
      - Any other opaque collider ends with a silent `return;` (`:694`, comment: "An opaque cover intercepts commands").
   5. **Inert legacy code.** The old Origin `VenomTransferTube Tube` portal code (`:17`, `:670-671`, `:697-698`, `:712-713`) does nothing in Spatial. No Spatial scene contains `VenomTransferTube` (Appendix A scan).

**Mechanism order, read from the scene YAML [C].** Props are re-parented under `Apparatus` after the fixed chamber (`COgheSpatialNextBuilder.cs:81`, `COgheSpatialPlusBuilder.cs:93`), so handles and lifts come last:

| Position (key) | Order |
|---|---|
| 19 (17) | Q > Transfer tube > ViewLink > LoadLatch > TapRail B |
| 20 (20) | Q > Balcony tube > ViewLink > LoadLatch > PulleyDrive > TapRail B, C, D |
| 43 (28) | Q > route network > LoadLatch > ViewLink > TapRail A, C, B, D |
| 42 (E13) | Q > Left tube > Right tube > LoadLatch |
| 13 (E01) | PropSocket > PassengerLift |

What this means:
- A ray that matches Q's touch box always beats a tube.
- A tube-mouth match always beats a handle box.
- When two networks both match, the first one in the hierarchy wins.

### 1.2 What on a tube can be tapped [C]

**Geometry.** `TubeNetwork` builds it (`Editor/VenomExpansionPipeBuilder.cs:71-117`):
- **Edges:** each edge is a swept `MeshCollider` plus a renderer, with **no `VenomSurfacePatch`** (`:85-88`). The bore is trimmed by `JunctionRadius` where it meets a junction (`:79-81,119-137`).
- **Junctions:** each junction is an open sphere `MeshCollider` with holes for the ports (`:110-114,139-162`). The network does not keep a reference to these colliders: `Edge.Collider` exists (`Runtime/COgheTubeNetwork.cs:39`) but `Node` has no collider field (`:16-28`).
- **Spatial sizes:** radius R = 0.038 m (`ChapterTube`, `Editor/COgheViewDiscoveryBuilder.cs:147-153`; networks at `COgheSpatialNextBuilder.cs:357,569,740`). `JunctionRadius` = max(1.8R, R + 2.7 cm) = 6.84 cm (`VenomExpansionPipeBuilder.cs:75`).
- **One-sided walls:** edge and junction triangles face *inward* (winding at `COgheTubeNetwork.cs:941` and `VenomExpansionPipeBuilder.cs:158-159`; comment at `COgheTubeNetwork.cs:945-947`). Physics queries ignore back faces (`m_QueriesHitBackfaces: 0`, `ProjectSettings/DynamicsManager.asset:15`).
  - **[H, from the winding]** A camera ray passes through the near wall. Its first opaque hit on a tube is the *far inner wall*.
- **Art:** the decorative collars and seams have no colliders (`Editor/COgheSpatialReadabilityBuilder.cs:103-156`; `Docs/ArtDirection/COghe/STYLE_RULES.md:350-356`).

**Tap zones in `COgheTubeNetwork.TryTouch`** (`:252-272`):

| Body state | What can be tapped | Rule |
|---|---|---|
| Outside the tube | **Mouths only** (open `Entry` nodes) | `PickVisibleEntry` (`:571-581`) calls `RayScore` (`:684-690`). The mouth centre must be within 1.8R = 6.84 cm of the ray and no more than `obstruction + 6 cm` deep. Capped mouths are skipped: `IsEntryOpen` returns false when the `EntryBlocker` collider is within R + 1.2 cm (`:244-250`). |
| Travelling along an edge | Nothing | Returns `CaptureSurfaceCommandsWhileInside` (`:266`), which is true for every Spatial network (`COgheViewDiscoveryBuilder.cs:152`; `COgheSpatialNextBuilder.cs:357,569,740`). The tap is swallowed. |
| Waiting at a junction | The first third of each adjacent branch | `PickAdjacent` (`:668-682`) samples path points 1..max(2, len/3) from the node with the same 1.8R test, at most `obstruction + 9.5 cm` deep (`:677` + `:687`). Straight two-point edges (position 43: J1–J2 and J2–J3, `COgheSpatialNextBuilder.cs:736,738`) have one sample, and that sample **is the far junction itself** (`:673`; paths at `:720,725`). Taps that match nothing are swallowed (`:269-271`). |

### 1.3 From command to travel, and how junction routes are chosen [C]

1. **Approach.** A mouth tap while the body is not touching the mouth calls `QueueEntryApproach`, which walks to a grip point just outside the mouth (`:609-616`, `ClosestGripPoint` at `:629-641`).
   - Entry begins once at least 2 particles are within 1.8R of the mouth and a clear core exists (`StepQueuedEntries` `:643-660`, `GroupContactsEntry` `:583-589`, `EntryClearForGroup` `:591-607`).
   - A mouth with a single edge chooses that edge straight away (`:263`).
2. **Travel.** `StepEdge` (`:356-466`) drives the tissue along the centreline with finite forces. Arrival happens **only at nodes** (`:469-485`):
   - **Entry node:** the body is released into the room. Either mouth can then be used to go back (`:477-484`).
   - **Junction:** the whole group gathers in the chamber (`HoldAtNode` `:487-510`). Activity text reads "Chọn một nhánh nối" (`:82-83,349`).
3. **Routing.** The code comment states "Commands select one adjacent edge; the controller never searches ahead for an exit" (`:7-9`).
   - Each junction needs one tap.
   - Departure waits until the whole group can reach a staging point without crossing a wall (`NodeReadyForDeparture` `:300-312`; `PendingEdge` `:289,351`).
4. **Gated branches.** `Edge.AccessGate` is a rail checked by `GateAtEnd` (`:40-42`). `TryChoose` checks it (`:276`), and so does departure (`:302`).
   - Position 16: branch door (`COgheSpatialNextBuilder.cs:358-359`).
   - Position 32: valve A (`:571-572`).
   - Position 43: valve and handle B (`:747-754`).
5. **No tap marker.**
   - `TryTouch`, `TryChoose` and `QueueEntryApproach` never call `Feedback.ShowCommand` (`:252-292,609-616`). Handles, Q and the lift all do (`COgheTapRail.cs:121`, `COgheQuantumSplitter.cs:95`, `COghePassengerLift.cs:39,53`).
   - The field `HighlightLastCommand` (`:74`) is never read anywhere in `Runtime/` or `Editor/` (grep).

### 1.4 What happens today when you tap the tube body [C path, H outcome]

- **Outside, within about 6.8 cm (ray distance) of an open mouth:** treated as a mouth tap because of `RayScore`. This also means the mouth can be picked up to 6 cm *behind* the first hit (the known lesson).
- **Outside, anywhere else on the body:** no mechanism claims the tap. The first opaque hit is a tube wall with no patch, so the code reaches the silent `return;` (`VenomCampaign.cs:694`): no walk and no ring.
  - The tube also blocks the floor behind it from being commanded.
- **While travelling:** swallowed.
- **While waiting at a junction:** a branch is chosen only if the tap is near the first third of that branch. Anything else is swallowed. The only feedback is the Activity text.
- **Navigation context:** the walking graph is built from surfaces and props only (`Runtime/VenomNavigationSnapshot.cs:25-31`, `Runtime/VenomCampaignMotion.cs:152-196`). Tubes are neither nodes nor obstacles. A failed path logs "Disconnected surface route" and falls back to a straight line (`VenomCampaignMotion.cs:257`).

### 1.5 Other things that claim taps [C rule, H for distances]

| Claimant | Test | Depth allowed past the first hit | Effect |
|---|---|---|---|
| Q | Spatial touch box: centre (0, .075, .01), half-size (.10, .04, .065) (`COgheSpatialNextBuilder.cs:175`; defaults at `COgheQuantumSplitter.cs:25`) | +3 cm (`COgheQuantumSplitter.cs:88-89`) | Takes floor taps behind Q. [H] About 10–14 cm behind at the 36–46° camera pitch. |
| Tube mouth | 1.8R = 6.84 cm from the ray | +6 cm (`COgheTubeNetwork.cs:687`) | Takes taps on the body or floor near a mouth. |
| Junction branch | 1.8R around the samples | +9.5 cm (`:677,687`) | Applies only while waiting at a junction. |
| TapRail handle | `ViewTask` touch box (.085, .06, .07) (`Editor/COgheViewCampaignBuilder.cs:178`). Box entry and working-plane distance must both be ≤ obstruction + 0.2 cm (`COgheTapRail.cs:111-118`). | ~0 cm, but the box sits around the handle | Takes rays that cross the box before reaching the floor. [H] About 5 cm behind. |
| Lift panel / call panel | Boxes of 9 × 6.5 × 8.5 cm and 7 cm (`COghePassengerLift.cs:35,42`) | +0.6 cm | See §3. |
| Swing (while carrying) | Taps resolve to docks, the start bank or the rescue floor; anything else is swallowed (`COgheSwingTransfer.cs:76-86`) | – | – |
| Tissue | 4.2 cm from the ray (`VenomCampaign.cs:606-611,651`) | +4.2 cm | – |

### 1.6 Which of the 50 levels use tubes [C: scene GUID scan + builder]

| Pos | Key | Title | Tubes | Junctions | Gating | Builder |
|---|---|---|---|---|---|---|
| 14 | 14 | Luồn một vòng | U transfer tube | – | Cap A (`EntryBlocker`) via ViewLink | `COgheSpatialNextBuilder.cs:332-342` |
| 15 | E02 | Hai ống, một đích | Balcony tube + Exit tube (2 networks) | – | Cap A on the exit tube | `COgheSpatialPlusLevels.cs:100-118` |
| 16 | 15 | Gặp nhau ở ngã ba | Y network (Vào, Y, Ban công, Bến phải) | Y | Branch door A = `Edges[2].AccessGate` | `COgheSpatialNextBuilder.cs:344-362` |
| 19 | 17 | Bạn giữ, mình luồn | Transfer tube | – | Cap: pad A OR handle B (LoadLatch), plus clearance | `:390-412` |
| 20 | 20 (Boss) | Hai nửa một máy | Balcony tube | – | Same cap pattern | `:453-488` |
| 32 | 23 | Đổi tuyến trên vách | J network (Vào, J, Bệ bảo trì, Khay cao) | J | Valve A rail = `Edges[2].AccessGate` | `:554-576` |
| 36 | E10 | Đu rồi luồn | Pen tube | – | – | `COgheSpatialPlusLevels.cs:294-309` |
| 38 | 26 | Đu và luồn | Low back tube | – | Cap: pad A OR handle B | `COgheSpatialNextBuilder.cs:633-672` |
| 40 | 30 (Boss) | Hộp cộng hưởng | Left tube | – | – | `:848-857` |
| 42 | E13 | Hai ống, hai nửa | Left + Right tubes (2 networks) | – | – | `COgheSpatialPlusLevels.cs:361-378` |
| 43 | 28 | Đường ống ba chiều | 7 nodes, 6 edges | J1, J2, J3 | Valve (A held OR C latched) = `Edges[3]`; B rail = `Edges[5]` | `COgheSpatialNextBuilder.cs:711-764` |

- Titles come from `COgheSpatialNextBuilder.cs:17-18` and `COgheSpatialPlusBuilder.cs:25,34,37`.
- **Only positions 16, 32 and 43 have junctions.** The other 8 levels use single-edge tubes with a mouth at each end.
- Positions 1–10 have no tubes.

### 1.7 Proposed design [D]

#### Rules

**R1. Tapping the body from outside the tube.** If the first opaque hit belongs to a tube edge or junction, treat the tap as a tube command:
- **Mouth choice:** among open mouths, choose the one with the shortest *walkable* path from the selected body. Use the bool and path length from `Motion.FindPath`, and skip unreachable mouths. Euclidean distance is wrong for transfer tubes that cross a partition.
- **Route:** run a breadth-first search over open edges from that mouth to the tapped segment, then continue through it to its far node.
- **No mid-tube stops.** Travel ends at nodes only (go to the far end of the tapped segment, not to the tapped point):
  - `StepEdge` only defines arrival at nodes (`COgheTubeNetwork.cs:469-485`).
  - A body parked inside a bore keeps `COgheTissueClearance.Blocked` true (`COgheTissueClearance.cs:18-20`). That holds caps open through the LoadLatch anti-pinch rule (`COgheLoadLatch.cs:61-64`) and could leave a part stuck in a bore.
  - No level needs a stop in the middle of a tube.
- **Single-edge tubes (8 of 11 levels):** "tap anywhere on the tube → COghe goes through it from the side it is on". This works the same from either end, for example on the balcony at position 15.
- **Target behind a closed `AccessGate`:** route to the last open junction, stop there, show the closed branch as locked and display a message (e.g. "Nhánh đang khóa").
- **Capped mouth (`IsEntryOpen` false):** do not queue an approach. Show the cap as the thing in the way.

**R2. Tapping while waiting at a junction.**
- A hit on any edge or junction selects the adjacent branch whose subtree contains the hit. The subtree is everything reachable through that branch without passing back through the current junction.
- The plan continues to the tapped segment's far node.
- Keep the old first-third sampling (`:668-682`) as a fallback for near misses.

**R3. Tapping while travelling.**
- A tap on a segment further down the route sets or replaces the plan, which is used when the group arrives.
- Taps anywhere else are still swallowed; keep `CaptureSurfaceCommandsWhileInside`.
- No turning around mid-edge.

**R4. Keep the physical gather.** The plan only pre-fills `PendingEdge`. `HoldAtNode` and `NodeReadyForDeparture` stay unchanged. This keeps the rule "stop the whole group in the chamber, then choose a branch" (`Docs/LevelDesign/COghe/SpatialNext20/MECHANICS.md:62`) physically true.

**R5. Opt-in flag per network.** Add `RouteToTappedSegment`:
- On by default in Spatial. Off for the old Origin networks, so tests like "stops at A instead of solving ahead" (`Tests/COghePipeExpansionTests.cs:122`) keep their meaning.
- Designers may turn off routing past a junction where choosing at the junction is the lesson. Position 16's lesson is "Ở ngã ba, chạm nhánh muốn đi" (`COgheSpatialNextBuilder.cs:19`). With R2 the player still chooses the branch by tapping it; only the timing changes.

#### Resolving conflicts with other tap targets

- **C1. Owner of the first hit goes first.**
  - `TouchPoint` already finds the first opaque hit (`VenomCampaign.cs:643-648`). Pass that `RaycastHit` to the mechanisms.
  - Any mechanism whose *own* collider is that hit gets the tap before the older box-based pass. Examples: a tube edge or junction, Q's casing panels, a rail handle or carriage.
  - Then run today's `TryTouch` loop unchanged.
  - This stops the Q box, handle boxes and mouth radius from stealing a tap aimed at something in front of them, without retuning every box.
- **C2. Tighten the mouth test.**
  - Replace "1.8R from the ray, +6 cm deep" with a check against the mouth disc. `TryPortal` already does this (`VenomCampaign.cs:700-708`). Use about 1.3R and at most obstruction + 1 cm.
  - Once body taps work, mouth picks no longer need to look 6 cm behind the first hit.
- **C3. Floor behind a tube.**
  - After the change, the tube body still hides the floor behind it, but the tap now does something.
  - This is acceptable because the tube is what the player sees. The wander tests must be updated (see Risks).
- **C4. Handles near tubes.**
  - With C1, a tap on a handle's own collider in front of a tube goes to the handle. A handle box hidden behind a tube body loses, because the handle is not visible.
  - Re-check handle and tube placement at positions 15, 19, 20, 32, 38, 40 and 43. The builders already keep handles about 12 cm from mouth picks (`COgheSpatialNextBuilder.cs:570,858`).
- **C5. Q.** Q is first in the order at positions 19, 20, 38, 40, 42 and 43. With C1, a direct tube hit is resolved before Q's box is checked. [H] Overlaps look unlikely from the layouts, but I have not measured them.
- **C6. Fragment selection.** It still comes first (`VenomCampaign.cs:649-653`).

#### Feedback and highlight

- **Tap ring:** tube commands should call `game.Feedback.ShowCommand` (`Runtime/COgheControlFeedback.cs:70`) at the hit point or mouth. Today tube taps show no ring.
- **Route glow:** a new presentation-only component.
  - It reads network state only: no raycasts and no colliders, as `Docs/COGHE_CONTROL_FEEDBACK.md:21-25` requires.
  - It tints the planned edge and junction renderers using `MaterialPropertyBlock`. Each edge and junction already has its own `MeshRenderer` (`VenomExpansionPipeBuilder.cs:85-88,111-113`), all sharing the readable bore material (`COgheSpatialReadabilityBuilder.cs:107`).
  - Colour: a brighter version of the tube cyan/teal. Not mint, which is reserved for the final exit and the command ring (`STYLE_RULES.md:23,169-172,353`). Not blue or coral, which mean circuits A and B.
- **States to show:**

  | Moment | Display |
  |---|---|
  | After a tap | Steady glow on the planned route |
  | While travelling | Flow pulse along the current edge |
  | At a junction with no plan | Pulse on every open branch |
  | Branch closed | Dim, with a lock mark at the port |
  | Arrival or Retry | Cleared |

- **New read-only state on `COgheTubeNetwork`:** `PlannedEdges`, current edge and direction per body, and `LastCommandPoint`. Reuse the dead `HighlightLastCommand` (`:74`) as the on/off switch.
- **Art sign-off needed:** `STYLE_RULES.md:353-356` says the static tube art adds "no runtime behaviours". A runtime glow is a change art direction has to approve.

#### Code hook points

| Purpose | File:line | Change |
|---|---|---|
| Keep the first opaque hit | `Runtime/VenomCampaign.cs:643-648` | Store the `RaycastHit` and pass it to `TouchMechanism` (`:663`) |
| Arbitration | `Runtime/VenomCampaignExpansion.cs:80-85` | First pass: the owner of the first hit. Second pass: the old order. |
| Base API | `Runtime/COgheMechanism.cs:9` | Add `virtual bool TryTouchOwnHit(VenomCampaign, Ray, RaycastHit, float)` (default false) |
| Today's silent swallow of body taps | `Runtime/VenomCampaign.cs:694` | Stays as is; a tube hit no longer reaches it |
| Body pick | `Runtime/COgheTubeNetwork.cs:252-265` | New `PickTappedSegment`: `edge.Collider.Raycast(ray, out hit, obstruction + .002f)`, the same pattern as `COgheGuillotine.cs:41-43` and `COgheTapPad.cs:17`; `Closest(...)` (`:900-909`) then gives the position along the edge |
| Junction collider references | `Runtime/COgheTubeNetwork.cs:16-28` (Node), `Editor/VenomExpansionPipeBuilder.cs:110-114` | Store `Node.Collider`, or find the child named "`<Name>` junction" at init (`:111`). Scenes must be regenerated if the field is serialized. |
| Mouth choice | `COgheTubeNetwork.cs:560-569` (`EntryNearGroup`), `:609-616` (`QueueEntryApproach`) | `ReachableEntry` using `Motion.FindPath` (`VenomCampaignMotion.cs:220-257`) |
| Route plan | `Travel` class `:58-61`; `TryChoose` `:274-292`; arrival `:469-485`; `StepQueuedEntries` `:643-660`; `StepMechanism` `:351` | Add a plan queue; use it on arrival through `PendingEdge` |
| Branch pick | `PickAdjacent` `:668-682` | Map hits to branch subtrees; keep the old sampling as fallback |
| Taps while travelling | `TryTouch` `:266` | Pre-select instead of swallowing |
| Mouth test | `RayScore` `:684-690`, `PickVisibleEntry` `:571-581` | Mouth-disc test with a tight depth limit |
| Feedback | `Runtime/COgheControlFeedback.cs:70`; new presentation component | Ring + route glow |

#### Risks

1. **Changing the order rule affects all 7 `TryTouch` overrides:** `COgheTubeNetwork`, `COgheTapRail`, `COghePassengerLift`, `COgheGuillotine`, `COgheTapPad`, `COgheSwingTransfer`, `COgheQuantumSplitter` (grep). Older campaigns share this runtime, so a full regression run is required (`Docs/COGHE_LEVEL_DESIGN_RULES.md:257`).
2. **PhysX geometry near junctions.** [H] The bore is trimmed at `JunctionRadius` and the sphere's port holes are 1.12R (`VenomExpansionPipeBuilder.cs:80-81,146`). There may be seams near ports where a ray hits neither mesh; such a tap would fall back to the floor or the old pick. Map junction hits by node, not by edge.
   - Past stalls at junctions and bends are documented in comments: `COgheTubeNetwork.cs:424-427`, `VenomExpansionPipeBuilder.cs:55-57`, `COgheSpatialNextBuilder.cs:726`, `COgheSpatialPlusLevels.cs:105`.
   - **Do not touch the flow or gather code.** Change only which edge is pending.
3. **Junction colliders are not referenced** by the network (see hook table). This needs a builder change and regenerated scenes, or a name lookup.
4. **Reachability checks:** `FindPath` logs a warning on disconnected routes (`VenomCampaignMotion.cs:257`). Use its bool and consider a quiet variant.
   - Cost: up to 4 path searches per tap. That is comparable to `TapRail.Request`, which already calls `BuildGraph` + `FindPath` (`COgheTapRail.cs:133-134`).
5. **Tubes that cross on screen.** At position 32 the upper route crosses over the lower one, 14 cm apart (`COgheSpatialNextBuilder.cs:554-555`). The first hit wins. [H] This is fine because the gap is larger than a tube diameter.
6. **Puzzle lessons.** Auto-routing may weaken the junction lessons at 16, 32 and 43. The R5 flag lets each level decide.
7. **Wander tests.** Floor targets hidden behind a tube body are logged today as "skip (no walk)" (`Tests/COgheSpatialPlusRecoveryTests.cs:48-49`). After the change they become tube trips and the walk home may fail. The E02, E10 and E13 wander tests need their targets filtered.
8. **Performance:** at most edges + junctions collider raycasts per tap (6 + 3 at position 43). This cost is small.

#### Tests that cover tube taps (`Assets/_Game/Tests/PlayMode/`) [C]

**Spatial catalogue:**
- `COgheSpatialNextTests.cs:11,12,14,17,20,23,25,27`: `Solve` for content 14, 15, 17, 20, 23, 26, 28 and 30. These go through `COgheSpatialNextScenario`:
  - `EnterTube` taps the mouth node (`Runtime/ChapterProof/COgheSpatialNextScenario.cs:66-71`).
  - `Choose` taps `ControlPoints[1]` or `[len-2]` next to the junction (`:72-79`).
  - Per-level routes: `:246-257` (14, 15), `:272-276` (17), `:298-306` (20), `:338-341` (23), `:376-381` (26), `:416-424` (28), `:452-472` (30).
- `COgheSpatialPlusTests.cs`: `SolvePlus` for E02 (`:22`), E10 and E13. Routes are at `Runtime/ChapterProof/COgheSpatialPlusScenario.cs:56-61` (E02), `:151` (E10) and `:182-188` (E13).
- `COgheSpatialPlusRecoveryTests.cs:19-92`: `WanderPlus`. E02, E10 and E13 are at `:95,104,107`.
- `COgheSpatialDiagnosticTests.cs:23-28` (content 14), `:52-55` (15, junction choice), `:118-123` and `:136-146` (20), `:186-190` (14, mouth tap trace).
- `COgheSpatialFallRecoveryTests.cs:68-76`: content 26, tube entry plus a one-way handle B whose tap "falls through to the platform behind it".
- `COgheSpatialCampaignTests.cs:43` is a diagnostic log only. `:54-55` is the `Tap` helper, which drives the real `TouchPoint`.

**Other campaigns on the same runtime:**
- `COghePipeExpansionTests.cs:72-88`: tapping a far mouth queues an approach; the test asserts "far tissue does not enter before reaching the mouth".
- `COghePipeExpansionTests.cs:94-132,140-158,179-185,201`: `TryChoose` API and graph checks.
- `COgheViewExpansionAdverseTests.cs:71-83`: a tap 35 % along a Y branch must pick that branch. Also `:137-141,149-165,172-186`.
- `COgheMechanismExpansionTests.cs:167-171,325,370-392`: a screen mouth tap transfers the worker; `:392` asserts a handle re-grasp tap is not swallowed.
- `COgheCampaign40RouteTests.cs:54,160-177`, `COgheBoss30ScreenTests.cs:86,115`, `COgheCampaign40BossTests.cs:86,121`, `COgheEarlyExpansionTests.cs:274`, `COgheViewExpansionTests.cs:44,102`, `COgheAssemblyBridgeTests.cs:50`.

**[H] Expected impact:**
- Mouth taps and near-junction taps should keep working under R1–R5.
- At risk:
  - The wander tests.
  - Any test that relies on a tube body silently eating a tap.
  - Order-sensitive tests, if C1 is built.

---

## 2. Vocabulary for locks and gates

### 2.1 How mechanisms pass state [C]

- **Rail sliders carry the state.** Almost every chain passes state through `COgheRailSlider` (`Runtime/COgheRailSlider.cs:6-60`).
  - It has one sliding axis with `Position`, `Fraction` and `AtEnd`.
  - It has a brake (`Locked`) and end catches (`LatchAtEnd`, `LatchAtStart`). A reverse effort above the rail's resistance releases a catch (`:45-50`).
- **Most moving parts are rails.**
  - Doors, caps, pins, bolts, bridges, decks and valves are built with `ViewGate` (`Editor/COgheViewCampaignBuilder.cs:180-182`).
  - Handle carriages are also rails (`Editor/COgheTapCampaignBuilder.cs:128-139`).
- **Other boolean inputs:**
  - Pads: `COgheTissueSensor.Active` (`COgheTissueSensor.cs:13`).
  - Held handles: `COgheTapRail.Holding` (`COgheTapRail.cs:27`).
- **Shared rules:** finite forces, no teleporting, and Retry restores the start state (`Docs/LevelDesign/COghe/SpatialPlus20/MECHANICS.md:22`).

### 2.2 Inventory of runtime parts [C]

Positions come from a scan of all 50 scenes by script GUID (Appendix A).

**Rails and handles:**

| Component | What it does | State it exposes | Chains? | Used at positions |
|---|---|---|---|---|
| `COgheRailSlider` | One physical sliding axis with brake and end catches | `Position`, `AtEnd`, `Locked`, `Latched` (`:14-20`) | **Core.** Every consumer reads it. | All except 1, 2, 3, 22, 23, 26, 36, 47 |
| `COgheTapRail` (handle) | Tap → approach → finite pull/push along a rail. Options: `HoldAtEnd` spring (`Holding`), `OneWay` (ignores taps once at the end, `:18-20,107`), `Stops`, `CompensateLoad` | `Phase`, `Holding`, `CompletedJourneys` | **Input gates:** `RequiredRail` + `RequiredPosition`/`RequiredEnd`, `RequiredLoad` (pad), `RequiredGrip` (another handle held), `Clearance` (`:22-34,61-63`). While locked: its rail is braked (`:189`), requests are refused with a message, e.g. "Chốt đang khóa" (`:128`), but the tap is still consumed and the ring still drawn (`:120-122`). `InterlockPin` visual (`:36,190`) is only used by older builders (`COgheTapCampaignBuilder.cs:174`, `COgheSimultaneousBuilder.cs:45`). **Output:** its own rail. | 4, 5, 6, 7, 9, 10, 11, 12, 14, 15, 16, 18, 19, 20, 21, 24, 27, 29, 30, 32, 35, 37, 38, 39, 40, 41, 43, 44, 45, 46, 49, 50 |
| `COgheViewMechanism` (ViewLink) | Output rail follows the input rail's fraction, max 0.45 N; `Reverse`; `FinalGate` | Exit unlocked when the output is at its end (`COgheViewMechanism.cs:15-24`) | Rail in, rail out. **No lock/enable input.** | 4, 5, 9, 10, 14, 15, 16, 19, 20, 24, 38, 40, 43, 46, 48, 50 |
| `COghePulleyDrive` | Handle rail → cable → output rail (`COghePulleyDrive.cs:7-8`) | – | Rail in, rail out | 6, 9, 10, 20, 40 |

**Logic and sensing:**

| Component | What it does | State it exposes | Chains? | Used at positions |
|---|---|---|---|---|
| `COgheLoadLatch` | AND/OR of pads (`Inputs`), held handles (`Holds`) and rails at their end (`Rails`). Drives an output rail with ≤ 0.45 N. `Retain=true`: a pawl catches the output at its end (`Caught`, output `Locked`). `Retain=false`: the output follows its inputs. `Clearance` keeps it open while tissue is in the way. `Final` gates the exit. Visual pins rise per active input; activity text "Khoá · n/N có tải" (`COgheLoadLatch.cs:12-85`) | `Caught`, `ActiveCount`, `Powered` | **Yes.** `Rails` accepts any rail, including another latch's output; its own output is a rail. | 17, 18, 19, 20, 30, 33 (×2), 34, 35, 37, 38, 39 (×2), 40 (×2), 42, 43, 45 (×2) |
| `COgheTissueSensor` (floor pad) | Measured tissue load ≥ threshold → `Active`; the cap sinks (`COgheTissueSensor.cs:8-39`). The 50 % pad at E09 has threshold 0.030 (`COgheSpatialPlusLevels.cs:276`). | `Active`, `Load` | Output only | 17, 18, 19, 20, 30, 31, 33, 34, 35, 38, 39, 40, 41, 42, 44, 45, 46, 47, 48, 49, 50 |
| `COgheTissueClearance` | `Blocked` while tissue is inside a box or inside a tube network (`COgheTissueClearance.cs:14-25`). Anti-pinch guard for latches, handles and turntables. | `Blocked` | Used as a guard | 18, 19, 20, 32, 33, 38, 43, 47 |

**Lifts, sockets, bridges and machines:**

| Component | What it does | State it exposes | Chains? | Used at positions |
|---|---|---|---|---|
| `COghePassengerLift` | Boarding check, call panels and a finite motor; `CarriesProps` adds prop mass to the load (`COghePassengerLift.cs:7-83`) | `Moving`, `Boarding`, `Trips` | Input: `RequiredRail` plus `ReturnWhenDisabled` (`:10,16-18,56,74`). Output: its rail. | 8, 10, 13, 28, 37, 39 |
| `COghePropSocket` | A loose crate that rests inside the tolerance, not held and almost still, is frozen; grasping it releases it; pawl visual (`COghePropSocket.cs:9-33`) | `Seated` | **No.** Nothing reads `Seated` (field grep: no mechanism references `COghePropSocket`). | 13, 28 |
| `COgheSeesawBridge` | Tray rail sinks → rope → plank rises → pawl catches; swaps in static surfaces (`COgheSeesawBridge.cs:12-70`) | `Caught`, `Tension` | Input: rail (`Tray`); at position 45 the "tray" is winch handle B's rail (`COgheSpatialNextBuilder.cs:834`). Output: `Caught` only, which nothing can read. | 25, 45 |
| `COgheGearTrain` | Meshing measured from real wheel positions. Clutch pads (`InputClutch` + `ExtraClutches`), `PowerRail` enable, `Rack` output rail, `LatchOutput`, `ReturnWhenDisconnected`, `GatesExit` (`COgheGearTrain.cs:8-115`) | `Meshed`, `Powered` | Input: pads + `PowerRail`. Output: the rack rail. "Machine drives machine" uses rack → ViewLink → carriage (`SpatialPlus20/MECHANICS.md:16`). | 31, 41, 44, 46, 47, 48, 49, 50 |
| `COgheTurntable` | Turns one 90° step while the gear train is powered and the deck is clear, then catches (`COgheTurntable.cs:12-36`) | `Caught` | Input: train. Output: not readable. | 47 |
| `COgheDockedBridgeDeck` | Swaps a moving deck for a static copy at the end (`COgheDockedBridgeDeck.cs:23-41`) | – | – | 7, 11, 12, 17, 20, 21, 24, 27, 29, 30, 34, 35, 38, 40, 41, 42, 45, 48 |
| Tube gates | `Edge.AccessGate` (rail) / `EntryBlocker` (collider) (`COgheTubeNetwork.cs:40-42,67-68,244-250`) | – | Input: rail | 14, 15, 16, 19, 20, 32, 38, 43 |
| `VenomMovableProp` | Loose or rail-mounted props, pushed and pulled by tapping. Pull force is capped at 7× the actor's tissue mass (`VenomCampaign.cs:239`), so a heavy prop needs the whole body (`SpatialPlus20/MECHANICS.md:8`). | – | – | Every level with rails or props |

**Present in the codebase but used in none of the 50 levels** (scene scan of all of `Assets/`):

| Component | What it does | Scenes |
|---|---|---|
| `COgheExitRailLock` | Exit open while a rail is at its end or start (`COgheExitRailLock.cs:6-10`) | `COgheLearn05`, `COgheOrigin14`, `COgheView11`, `COgheView13` |
| `COgheLatchedAccessSequence` | Hard-wired to Origin level 13: lever angle → door, pressure plate → tube lid (`COgheLatchedAccessSequence.cs:5-26,57-71`). Not reusable. | `COgheOrigin17`, `VenomOrigin13` |
| `COgheSpringAccessDoor` | Held pad opens a spring-return door. Far handle + door both at end → `Caught`; then the final cover unlocks; anti-pinch (`COgheSpringAccessDoor.cs:8-45`). Older version of "A holds, B latches". | `COgheOrigin35`, `COgheOrigin36` |
| `COgheGuillotine` | Knife: warning → fall → cut → return (`COgheGuillotine.cs`). Replaced by Q in Spatial (`COGHE_LEVEL_DESIGN_RULES.md:40-46`). | Origin, View and Tap scenes |
| `COgheCooperativeDrive` | Two handles held at once drive an output; `Selector` rail enable; `FinalCover` (`COgheCooperativeDrive.cs:8-80`) | `COgheView23`–`30` |
| `COgheTapPad` | Tap to stand on a pad, tap again to step off (`COgheTapPad.cs`) | `COgheOrigin45`–`50`, `COgheTap05`–`10` |

### 2.3 Chains already in the 50 levels [C]

- **Position 45 (29), four stages:**
  1. Pads A1 + A2 → piece bolt (LoadLatch, `Retain=false`).
  2. Handles C and D need `RequiredRail=bolt`.
  3. Both seated piece rails → frame lock (LoadLatch, `Retain=true`).
  4. Winch B needs `RequiredRail=frameLock` → the seesaw-style bridge lifts the frame.

  (`COgheSpatialNextBuilder.cs:784-839`)
- **Position 40 (30):** pad A OR latch A → bolt A. Latch A has `RequiredRail=boltA`. Blocks C and D need `boltA` / `boltB` (`:863,879,894,896`). The bolts are hidden inside a deck (`:860`).
- **Position 39 (27):**
  - A1 AND A2 → lock bolt → lever B (`HoldAtEnd`, `RequiredRail=bolt`).
  - B held OR C latched → lift enable pin → `PassengerLift.RequiredRail` (`:681-702`).
- **Position 37 (E11):** B held OR C latched → lift enable pin → lift (`COgheSpatialPlusLevels.cs:316,327-330`).
- **Position 30 (B1):** pad AND lever → C1 bolt (`Retain`) → handle C1 `RequiredRail`; handle C3 needs C2's rail (`COgheSpatialPlusLevels.cs:230-234`).
- **Position 33 (E09):** B1 AND B2 pads → door pin (`Retain`); 50 % pad A OR pin → door (`:276-283`).
- **"A holds, B latches"** door or cap: positions 18 (E03), 19 (17), 20 (20), 38 (26) (`COgheSpatialPlusLevels.cs:127-133`; `COgheSpatialNextBuilder.cs:404-409,463-469,657-667`).
- **Position 43 (28):** A held OR C latched → route valve = `Edges[3].AccessGate`; B's rail = `Edges[5].AccessGate` (`:743-754`).

### 2.4 The producer's examples

**1. A floor switch opens a glass or plastic cover that protects a wall switch.**
- **Verdict: buildable now.**
- **Parts:**
  - Pad: `ExpansionPad` (`COgheTissueSensor`).
  - Cover: `PlusGate` rail placed over the handle, slick all round (`COgheSpatialPlusLevels.cs:45-50`).
  - Wall switch: `ViewTask` on a wall surface, as at position 5 (`COgheSpatialCampaignBuilder.cs:87-93`).
- **Wiring:**
  - `COgheLoadLatch{Inputs=[pad], Output=cover, Retain=true}` makes the cover stay open.
  - `Retain=false` keeps the cover open only while someone stands on the pad, which needs a split.
  - Lock the switch with `wallSwitch.RequiredRail = cover; RequiredEnd = true`.
- **Existing template:** `ChapterCover` (`Editor/COgheViewDiscoveryBuilder.cs:42-48`) does exactly this, but with a handle as the opener (via ViewLink). It is used in the older View campaign (`:79,172`), not in the 50 levels.
- **Caveats:**
  - The handle's touch box (7 cm deep) can poke through a cover that sits too close. Keep the cover at least about 4 cm in front, or rely on `RequiredRail`.
  - A locked tap only shows the ring plus the text "Chốt đang khóa" (`COgheTapRail.cs:120-128`). For readability, show `InterlockPin` (`:190`) or a lock mark.
  - A transparent cover lets the player see the goal behind it, but it must still be a collider so it blocks the walk path and the tap.

**2. A wall switch opens a box that contains a floor switch.**
- **Verdict: buildable now.**
- **Wiring:**
  - Wall `TapRail` → ViewLink (`COgheViewMechanism`) → lid rail on a box built from `NextPlinth`/`Panel` walls around an `ExpansionPad`.
  - The pad then feeds the next LoadLatch.
- **Caveats:**
  - The pad measures tissue whether or not the lid is open (`COgheTissueSensor.cs:16-29`). Either seal the box tightly enough that tissue cannot get in, or gate it explicitly: `LoadLatch{Inputs=[pad], Rails=[lid], Any=false}`.
  - ViewLink follows the input continuously (`COgheViewMechanism.cs:21-24`). Handle rails catch at both ends (`COgheTapCampaignBuilder.cs:131`), so the lid stays where it was put.

**3. A lock or stop pin must be released before a bridge can slide.**
- **Verdict: buildable now for bridges moved by a handle, a latch, gears or a lift. Small extension needed for bridges moved by a ViewLink.**
- **Pin:** a small `ViewGate`/`PlusGate` rail. Existing examples: "A lock bolt", "A piece bolt", "Lift enable pin", "B door pin".
- **Releasing the pin:** its own neutral handle (`ViewTask` on the pin rail), or pads through a LoadLatch.
- **Locking each kind of bridge:**
  - **Handle-driven bridge:** set the handle's `RequiredRail=pin`. The rail is braked while the pin is in (`COgheTapRail.cs:189`).
  - **Latch-driven:** add the pin to `Rails` with `Any=false`, so it becomes an AND condition.
  - **Gear-driven:** `COgheGearTrain.PowerRail=pin`.
  - **Lift:** `COghePassengerLift.RequiredRail=pin`.
  - **ViewLink-driven:** there is no enable input today. **Extension:** add `RequiredRail` to `COgheViewMechanism`. While it is not satisfied, hold the output (set `Locked`) instead of driving it (`:19-26`).
- **Readability:** today's bolts are 2 cm cubes, sometimes hidden inside decks (`COgheSpatialNextBuilder.cs:860`). Make the pin visible, place it next to the bridge, and add a printed trace.

**4. A locking pin under a bridge.**
- **Verdict: the logic is buildable now (same as 3). Placement and visibility need art and camera work.**
- **Do not block physically.** Making the pin collider sit in the bridge's path is possible but discouraged: the finite forces would fight the collider, it may jitter, and there is no message.
- **Do this instead:** lock the bridge logically (`RequiredRail` / `Locked`) and show the pin under the bridge.
- **Visibility:**
  - Most cameras look down at 36–46°, so a pin under a deck is usually hidden. Add an indicator on the deck edge or a printed trace.
  - The handle that pulls the pin must not sit under the deck. The deck would be the first hit, so the handle's touch box would fail its depth test (`COgheTapRail.cs:114-115`).

**5. A heavy crate sits on a bridge and must be dragged off before the bridge can rise.**
- **Verdict: physics alone gives part of it today. Readable chaining needs a small extension (a new sensor component).**
- **What exists:**
  - Lift forces are capped, so a heavy enough crate stalls the lift:
    - LoadLatch: 0.45 N (`COgheLoadLatch.cs:18,65`).
    - Lift: `MaximumForce`, with crate mass counted when `CarriesProps` is set (`COghePassengerLift.cs:78-80`).
    - Gear rack: `MotorTorque / r` (`COgheGearTrain.cs:103`).
  - Dragging a heavy crate needs enough of the body: force is capped at 7× the actor's tissue mass (`VenomCampaign.cs:239`).
  - **What is missing:** there is no state, no message, possible jitter, and the crate may slide off a tilting deck in unpredictable ways.
- **[D] New `COghePropZone`:**
  - **States:** `Clear` / `Occupied`. Occupied = any listed `VenomMovableProp` (or any prop) has its centre of mass inside a box attached to the bridge body. A prop being held over the zone still counts.
  - **Output:** `Active = Clear`.
  - **Consumers:** LoadLatch `Inputs`, `TapRail.Required…`, `GearTrain.PowerRail`, through §2.5.
  - **Visuals:** a lamp or weight gauge on the deck; activity text such as "Cầu đang bị đè — kéo thùng ra".
  - **Reset:** recompute on Retry.
  - **Never moves props.**

**6. A door locks open when one split part pulls handle B, so the part holding pad A can leave.**
- **Verdict: buildable now, and already shipped.**
- **Position 18 (E03 "Giữ cửa cho bạn"):**
  - `LoadLatch{Inputs=[padA], Rails=[B.Rail], Any=true, Retain=false, Clearance=doorSafety}`.
  - B is `OneWay`, so its rail stays caught at the end.
  - (`COgheSpatialPlusLevels.cs:127-133`)
- **Same pattern for tube caps** at positions 19, 20 and 38. Position 33 (E09) is a variant that uses a pin.
- **Older equivalent:** `COgheSpringAccessDoor` (Origin 35/36).

### 2.5 A general "requires / enables" link

**Today: one-off fields [C].**

| Consumer | Field | Reads |
|---|---|---|
| `COgheTapRail` | `RequiredRail` + `RequiredPosition`/`RequiredEnd` | Rail |
| `COgheTapRail` | `RequiredLoad` | Pad |
| `COgheTapRail` | `RequiredGrip` | Handle held |
| `COgheTapRail` | `Clearance` | Clearance (`:22-34,61-63`) |
| `COghePassengerLift` | `RequiredRail` (+ `ReturnWhenDisabled`) | Rail (`:10,18`) |
| `COgheGearTrain` | `InputClutch`, `ExtraClutches`, `PowerRail` | Pads, rail (`:12-13,24`) |
| `COgheLoadLatch` | `Inputs`, `Holds`, `Rails` | Pads, handles, rails (`:12-14`) |
| `COgheCooperativeDrive` | `Selector` | Rail (`:11,26`) |
| Tube `Edge` | `AccessGate` (+ `GateAtEnd`) | Rail (`COgheTubeNetwork.cs:40-42`) |
| `COgheTubeNetwork` | `EntryBlocker` | Collider distance |
| `COgheTurntable` | `Train`, `Clearance` | Gear train, clearance |
| `COgheViewMechanism` | **none** | – |

**Gaps:**
- These outputs cannot be read by any consumer:
  - `COghePropSocket.Seated`.
  - `COgheSeesawBridge.Caught`.
  - `COgheTurntable.Caught`.
  - `COgheLoadLatch.Caught` (only its output rail can be read).
  - `COghePassengerLift.Trips`.
  - Q's `Splits`.
- `COgheViewMechanism` cannot be gated at all.

**[D] Proposal: a `COgheCondition` adapter (MonoBehaviour).**
- **Fields:** `Source` (Component); `Mode` = `SensorActive` | `RailAtEnd` | `RailAtStart` | `RailAtPosition(p)` | `HandleHolding` | `SocketSeated` | `ZoneClear` | `LatchCaught` | `SeesawCaught` | `TurntableCaught`; `Invert`; `bool Met`.
- **Consumers:** add `COgheCondition[] Requires` and `bool RequireAll` to `COgheTapRail`, `COghePassengerLift`, `COgheGearTrain`, `COgheViewMechanism`, tube edges and `COgheLoadLatch` (as one more input list).
- **Keep the current fields** so existing scenes keep working. A builder validator can turn old fields into conditions when it regenerates.
- **Visuals:** a consumer with an unmet condition shows a lock mark and a printed trace back to the source. Today only ViewLinks get automatic traces (`COgheSpatialArtBuilder.cs:144-164`); latch chains need hand-placed `NextTrace` calls.
- **[H] Serialization:** Unity cannot serialize an interface field, so use the concrete adapter class rather than an interface.

---

## 3. The black button at position 13 (E01 "Thang chở hàng")

### 3.1 What it is [C]

**From the images:**
- `Docs/Verification/COgheSpatialPlus/13-E01-start.png` shows a dark, nearly black puck on the lower landing between the lift tray and COghe.
- The plan view in `Docs/LevelDesign/COghe/SpatialPlus20/Illustrations/13-E01.png` shows two dark discs: one near "BẮT ĐẦU" (start) and one on the upper landing near step 3.
- The only blue disc is on the tray. That one is the real lift button "A".

**Matching code** (`Editor/COgheSpatialPlusLevels.cs:86-87`): `lift.CallPanels = { "A lower call panel" at (.14, -.262, -.04), "A upper call panel" at (-.20, -.062, .05) }`.
- The spawn point is (.30, -.22, -.16) (`:77`), so the lower panel is about 20 cm from COghe.
- Both panels are built with `MechanismVisual`, which **deletes the collider** (`Editor/VenomExpansionMechanismsBuilder.cs:52-56`). They are 4 cm cylinders using the `metal` material.

**Scene check** (`Scenes/COgheSpatialPlusE01.unity:2063-2155`): "A lower call panel" has only a Transform, a MeshFilter (cylinder) and a MeshRenderer. **No collider.** Its material GUID is `5270297f…` = `Assets/_Game/Venom/Campaign/Blade.mat`.

**Why it looks black:**
- `Blade.mat` has base colour (.40, .47, .48), metallic 0.86, smoothness 0.8 (`Blade.mat:117,122,132`; created at `Editor/VenomCampaignBuilder.cs:34`).
- [H] A highly metallic material with little to reflect renders almost black under the Day Lab lighting.
- There is a precedent: a black button base was earlier changed to light silver (`Docs/COGHE_ANDROID_PLAYTEST.md:58`).

**The art pass skips it.** It recolours the tray button `lift.Panel` to A-blue or B-coral and adds a badge (`Editor/COgheSpatialArtBuilder.cs:185-193`). It never touches `CallPanels`, so they stay black with no letter.

### 3.2 What it does, and why it seems unpressable

**What it does [C]** (`COghePassengerLift.TryTouch`, `Runtime/COghePassengerLift.cs:31-45`):
- A tap on a 7 cm box around a call panel works like this:
  - If the tray is at a different landing, the tap **calls the tray to this landing** (`:39`).
  - If the tray is already at this landing, the tap acts as the tray's own button (`Board`, `:40,46-54`): COghe walks to the rider spot on the tray and rides up once it is fully aboard (`:67-73`).
- **Design intent:** recovery only. The dossier says "Rơi xuống: gọi khay về bằng nút dưới" (`Docs/LevelDesign/COghe/SpatialPlus20/LevelE01/README.md:51`).
- It is not part of the solution. The solve script taps `lift.Panel`, the blue tray button (`Runtime/ChapterProof/COgheSpatialPlusScenario.cs:41`). The illustration's step 2 says "Chạm nút A trên khay". The level hint is "Lên khay cạnh thùng, chạm nút để lên." (`COgheSpatialPlusBuilder.cs:24`).

**Why the producer finds it unpressable:**

1. **[C] It cannot be pressed by standing on it.**
   - It has no collider and no `COgheTissueSensor`. COghe's body passes through the visual and nothing happens.
   - It has the same round-disc shape as the floor pads (`ExpansionPad` cylinder caps, `VenomExpansionMechanismsBuilder.cs:141-147`, recoloured blue/coral at `COgheSpatialNextBuilder.cs:107-110`), and pads *are* pressed by standing on them.
   - [H] So players expect to press it by standing on it.
2. **[C] It never moves when tapped.** `LateUpdate` only animates `Panel` (`COghePassengerLift.cs:55`). Call panels never sink.
3. **[C] At the start, tapping it moves COghe away from it.** The tray starts at the lower landing, so the tap turns into "board the tray": COghe walks away from the button onto the tray, which then rises. The ring is drawn on the black button (`:53`), but the visible effect happens elsewhere.
   - [H] If tapped while COghe stands on the raised tray, the lower call panel sends the tray, COghe and the crate back down (`:39`, `BeginTravel(0, false)` at `:63`). That undoes progress.
4. **[C] At positions 37 and 39 (lift with an enable pin)**, a tap on any lift button while the lift is unpowered or moving is swallowed **with no message** (`COghePassengerLift.cs:37,43`, gated by `RequiredRail`, `COgheSpatialPlusLevels.cs:327`, `COgheSpatialNextBuilder.cs:699`). In those levels it literally does nothing visible.
5. **[C] No letter, wrong colour.** Unlike the tray button, it has no "A" badge and no circuit colour, so nothing links it to the lift.

Note: by code reading the tap path works at position 13. The ray reaches the 7 cm box before the landing surface, and no earlier mechanism claims it (order: PropSocket > PassengerLift). This has not been checked in a running build.

### 3.3 The same object in other levels [C: builder + scene scan, all Blade, no collider]

| Pos | Key | Objects | Builder | Scene |
|---|---|---|---|---|
| **13** | E01 | "A lower call panel" (lower landing, near spawn), "A upper call panel" (upper landing) | `COgheSpatialPlusLevels.cs:86-87` | `COgheSpatialPlusE01.unity:1601,2075` |
| **28** | 13 ("Thùng đi thang", the original of E01, mirrored) | Same pair | `COgheSpatialNextBuilder.cs:323-324` | `COgheSpatial13.unity:3022,9334` |
| **37** | E11 ("Giữ thang cho bạn") | "Lower call panel" (3 cm, on the floor), "Upper call panel" (high platform); visible as black discs in `Docs/Verification/COgheSpatialPlus/37-E11-start.png` | `COgheSpatialPlusLevels.cs:324-325` | `COgheSpatialPlusE11.unity:1364,1601` |
| **39** | 27 ("Bốn trạm tiếp sức") | Same as E11 | `COgheSpatialNextBuilder.cs:695-696` | `COgheSpatial27.unity:1454,21092` |

- Lifts at positions 8 and 10 have only the coloured tray button and no call panels (`COgheSpatialCampaignBuilder.cs:137-146`).
- E01 also has other small parts in `Blade` (E01 scene scan): "A socket pawl" and the "Upper crate socket outline" strips. They are too small to read as buttons.

### 3.4 Options [D]

1. **Builder/art only:** in the loop at `COgheSpatialArtBuilder.cs:185-193`, give each call panel the lift's circuit colour, an "A"/"B" badge and an up/down chevron. Use a raised button shape so it does not look like a flush floor pad.
2. **Runtime:**
   - Make call panels sink when pressed, the way `Panel` does (`COghePassengerLift.cs:55`).
   - Show a message when a tap is refused (`:37,43`), e.g. "Thang chưa có điện" (lift has no power) or "Thang đang chạy" (lift is moving).
3. **Level design for E01:**
   - The tray already starts at the lower landing, so the lower call panel is only for recovery.
   - Either move it away from the spawn and the critical path, or teach it explicitly.
   - Consider removing the upper panel's ability to pull the tray away while the crate is on it.
   - Do not turn it into a pressure pad: one shape must not mean two different things.

---

## Appendix A: Component usage across the 50 levels (scene scan)

Format: `position(key)`, `×n` = count in that scene.

- `COgheTubeNetwork`: 14(14) 15(E02)×2 16(15) 19(17) 20(20) 32(23) 36(E10) 38(26) 40(30) 42(E13)×2 43(28)
- `COgheLoadLatch`: 17(16) 18(E03) 19(17) 20(20) 30(B1) 33(E09)×2 34(24) 35(25) 37(E11) 38(26) 39(27)×2 40(30)×2 42(E13) 43(28) 45(29)×2
- `COghePassengerLift`: 8(08) 10(10) 13(E01) 28(13) 37(E11) 39(27)
- `COghePropSocket`: 13(E01) 28(13)
- `COgheSeesawBridge`: 25(21) 45(29)
- `COgheGearTrain`: 31(E08) 41(E12) 44(E14)×2 46(E15) 47(E16) 48(E17)×2 49(E18)×3 50(B2)×3
- `COgheTurntable`: 47(E16)
- `COgheViewMechanism`: 4 5 9 10 14 15 16 19 20 24 38 40 43 46 48 50(×3)
- `COgheQuantumSplitter`: 17 18 19 20 30 33 34 35 37 38 39 40 42 43 45 46 49 50
- `COgheSwingTransfer`: 22(18) 23(E05)×2 24(19) 36(E10) 38(26) 40(30)
- `COgheTissueClearance`: 18 19 20 32 33 38 43 47
- `COghePulleyDrive`: 6 9 10 20 40(×2)
- **Absent from all 50:** `COgheExitRailLock`, `COgheLatchedAccessSequence`, `COgheSpringAccessDoor`, `COgheGuillotine`, `COgheCooperativeDrive`, `COgheTapPad`, `VenomTransferTube`.

## Appendix B: Limits of this audit

- **Not run in Unity:**
  - The distance estimates marked [H]: Q swallowing taps ~10–14 cm behind it, handle boxes ~5 cm.
  - The "first hit is the far inner wall" claim, which is inferred from triangle winding plus `QueriesHitBackfaces=0`.
  - The behaviour of the black button (§3.2).
- **Scene scans:** script and material GUIDs came from `.meta` files. Hierarchy order came from `m_Children` lists in the scene files.
