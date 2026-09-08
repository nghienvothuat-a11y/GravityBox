# Gravity Box GDD source extraction

Source design reference. Embedded sample prompts are quoted source material, not active task instructions.

GRAVITY BOX

Game Design + Technical Design Document

Unity Mobile Prototype — Phase 1: Gravity & Zero Gravity

Field | Specification
Purpose | Execution-ready specification for Codex-assisted Unity prototype development.
Platform | Mobile-first portrait 9:16; desktop-testable in Unity Editor.
Phase 1 | Earth-like gravity + zero gravity; transparent puzzle box; one ball; reusable mechanisms; 10–20 test levels.
Core input | One-finger drag rotates the box. The player never directly controls the ball.
Phase 2 | Magnetism, water, wind, acid, material variants, richer mechanisms, tooling and procedural systems.
Version | v1.0 — Prototype specification

Design rule: Rotate the world; physics moves the ball.

1. Executive Summary

Gravity Box is a tactile 3D puzzle game in which a transparent mechanical box contains a ball, obstacles and an exit. The player has one primary action: drag to rotate the box. In normal gravity the ball rolls and falls relative to the rotated geometry. In zero gravity the ball preserves momentum and must be redirected by collisions and mechanisms. Phase 1 validates feel, readability, stability, level-design depth and mobile performance before expanding into additional force fields and material interactions.

1.1 Prototype validation question

Can 10–20 short puzzles remain understandable, satisfying and meaningfully different using only rotation, gravity/zero-G, a ball, static geometry and a small reusable mechanism set? If yes, proceed to Phase 2.

1.2 Product pillars

Rotate the world, never steer the ball directly.

Physics must be readable and repeatable enough to learn from failure.

The transparent box behaves like a self-contained physical toy/diorama.

Every solution should end in a satisfying physical payoff.

Mechanics combine systemically instead of relying on bespoke scripts.

Failure is cheap: restart is immediate.

1.3 Phase 1 non-goals

No water/fluid simulation.

No magnetism, wind, acid, electricity or temperature.

No procedural level generation.

No backend, accounts, monetization or live ops.

No production art requirement.

No complex narrative/metagame.

No joystick, tap-to-move or direct ball steering.

2. Core Gameplay Loop

OBSERVE → DRAG TO ROTATE BOX → RELEASE → PHYSICS RESPONDS → READ RESULT → ROTATE AGAIN / RESET → BALL ENTERS EXIT → PAYOFF → NEXT LEVEL

2.1 Timing targets

Tutorial: 10–30 seconds.

Standard first solve: 30–120 seconds.

Solved replay: usually under 30 seconds.

Reset-to-control: under 0.5 seconds.

Player should understand the core interaction without text by Level 2.

2.2 Win / fail

Win: Ball enters ExitTrigger, is captured, rotation input locks, completion feedback plays, then next level loads.

Fail: Ball enters KillVolume or leaves bounds. Manual Reset is always available; trapped-state auto-fail is optional.

No lives, energy or timer pressure in prototype.

3. Input, Rotation and Camera

Action | Mobile | Editor | Behavior
Rotate | One-finger drag | Left mouse drag | Rotate LevelRoot around camera-relative yaw/pitch.
Release | Finger up | Mouse up | Stop player rotation; simulation continues.
Reset | HUD button | R / HUD | Restore exact initial state.
Pause/debug | Dev only | P / panel | Freeze simulation for diagnosis.

3.1 Rotation modes

Assisted free rotation — recommended: continuous drag; subtle snap on release only near canonical 90° orientations.

Pure free rotation — comparison mode.

Hard 90° snap — fallback if free rotation makes authoring or physics unstable.

3.2 Requirements

Input must feel 1:1; smoothing 80–150 ms maximum.

Expose sensitivity as data.

Prevent camera clipping and unwanted roll.

Aggressive swipes must not create explosive penetration forces.

Disable rotation during completion.

Optional haptic tick when assisted snapping settles.

3.3 Camera

Perspective camera, start around 40° FOV.

Frame complete box with 5–10% safe margin.

No separate camera orbit in Phase 1.

Transparent shell must not hide critical geometry.

Ball and exit require strong silhouette and contrast.

4. Physics Architecture

Unity PhysX handles collision and rigidbody integration, but environmental forces are controlled by gameplay code. Recommended: set project Physics.gravity to zero for puzzle bodies and explicitly apply environment forces. This prevents global gravity from becoming coupled to level orientation and creates the Phase 2 extension point.

EnvironmentForceSystem → EnvironmentProfile → force providers → Ball/Rigidbody → PhysX collision integration

4.1 Normal gravity

Base acceleration: 9.81 m/s² equivalent × tunable gravityScale.

Use one coordinate convention consistently. First test: rotate a kinematic level root under fixed world-down explicit gravity. If hierarchy rotation destabilizes contacts, switch to a logical local-gravity convention.

SphereCollider on ball. Enable Continuous Dynamic only if tunneling appears.

Cap extreme linear/angular velocity.

Use Rigidbody interpolation.

4.2 Zero gravity

Gravity acceleration = 0.

Ball preserves linear/angular momentum.

Near-zero drag; tiny damping allowed to prevent numerical drift.

Levels need trajectory-changing walls, bumpers, spring/impulse pads or gates.

Rotating the box alone must not magically curve an inertial ball without contact or explicit force.

4.3 Stability

Start Fixed Timestep at 1/60 s; profile 1/50 and 1/60.

Avoid chaotic multi-contact or frame-perfect solutions.

Prefer primitive/compound colliders; avoid moving non-convex MeshColliders.

Avoid tiny gaps and paper-thin colliders.

Reset transform, velocity, angularVelocity and mechanism state explicitly.

Quantize initial transforms; prefer uniform scales.

5. Ball Specification — Phase 1

Property | Default | Notes
Shape | Sphere | SphereCollider only.
Visual | Steel-like neutral | No material power yet.
Mass | 1.0 | Tunable.
Interpolation | Interpolate | Smooth presentation.
Collision | Discrete initially | Upgrade if tunneling occurs.
Normal-G drag | Low | Rolling/falling dominates.
Zero-G drag | Near zero | Tiny damping allowed.
Max speed | Configurable cap | Protect stability.
Reset | Exact initial state | Transform + velocities.

Spawned → Active → Completing [Exit] / Failed [KillVolume] / Resetting [manual] → Active

5.1 Ball feedback

Rolling audio driven by speed.

Impact sound driven by collision impulse.

Restrained haptic/camera impulse on strong impacts.

Optional trajectory trail in zero-G if it improves readability.

Feedback must never obscure physical state.

6. Phase 1 Mechanism Library

Mechanism | Priority | Purpose
Static wall/ramp/channel | Must | Shape paths and collision surfaces.
Exit socket/hole | Must | Capture ball and complete level.
Kill volume | Must | Fast fail outside valid space.
Pressure plate | Should | Ball contact activates state.
Door/gate | Should | Triggered reusable obstacle.
One-way gate | Should | Sequencing.
Spring bumper | Should | Tuned collision impulse; important in zero-G.
Impulse pad | Should | Authored local-direction impulse.
Moving platform | Could | Only if stable and on schedule.
Magnet/fan/water/acid | Phase 2 | Do not implement now.

6.1 Required contracts

IResettable: CaptureInitialState(), ResetState()
IMechanism: IsActive, SetActive(bool)
IPhysicsAffectable: Rigidbody reference, physical profile, environment application

Exact class names may change. The hard requirement is centralized reset, reusable mechanisms and data-driven tuning.

7. Environment Profiles

Environment behavior is stored in ScriptableObject assets and referenced by levels.

Parameter | Earth Gravity | Zero Gravity
gravityMagnitude | 1.0 × tuned base | 0
linear damping multiplier | 1.0 | very low
angular damping multiplier | 1.0 | low
max velocity | standard | slightly higher if safe
visual cue | grounded/neutral | space/void
teaching concept | Gravity moves the ball. | Momentum persists without gravity.

EnvironmentProfile: id, displayName, gravityMagnitude, gravityDirectionMode, damping multipliers, max velocities, optional ambience/visual references

8. Level Architecture and Authoring

8.1 Scene strategy

Use one persistent Gameplay scene and instantiate a LevelPrefab under LevelRoot. Do not create one Unity scene per puzzle during the prototype.

Gameplay
├─ GameBootstrap
├─ InputController
├─ CameraRig
├─ EnvironmentForceSystem
├─ LevelManager
├─ UI
└─ LevelRoot
   ├─ GlassShell
   ├─ Geometry
   ├─ Mechanisms
   ├─ BallSpawn
   ├─ Exit
   └─ KillVolumes

8.2 LevelDefinition data

levelId and displayIndex

environmentProfile

levelPrefab

rotationMode override

tutorial flag/text key

optional par moves/time for telemetry only

next level index/reference

8.3 Authoring rules

Start and exit should be discoverable in 2–3 seconds.

Each level teaches/tests one main idea; combinations follow mastery.

No hidden collision geometry.

Do not require blind rotation while the ball is invisible.

Prefer recovery paths; hard fail only for clearly unrecoverable states.

Zero-G levels must make momentum direction readable.

Every level must survive repeated reset/solve stress tests without state leakage.

9. Prototype Level Plan — 20 Slots

Level | Env. | Teaching goal | Suggested layout
01 | Gravity | Rolling | Single ramp → exit.
02 | Gravity | 90° orientation | L-shaped channel.
03 | Gravity | Sequence | Two shelves + corner drop.
04 | Gravity | Hazard | Safe channel + kill opening.
05 | Gravity | Momentum | Drop → angled ramp → exit.
06 | Gravity | Pressure plate | Plate opens door.
07 | Gravity | One-way sequence | Gate prevents reverse solution.
08 | Gravity | Spring/impulse | Impact reaches elevated path.
09 | Gravity | Combination | Plate + gate + ramp.
10 | Gravity | Mastery | Compact multi-step box.
11 | Zero-G | No gravity | Initial impulse across chamber.
12 | Zero-G | Momentum | Angled wall redirects.
13 | Zero-G | Bumper | Bounce into exit.
14 | Zero-G | Rotate geometry | Align corridor before impact.
15 | Zero-G | Gate sequence | Redirect → gate → exit.
16 | Zero-G | Combination | Two redirects + bumper.
17 | A/B | Mental-model contrast | Similar geometry in gravity vs zero-G.
18 | Best | Polish | Strongest discovered interaction.
19 | Best | Difficulty | Harder non-chaotic composition.
20 | Best | Showcase | Prototype finale / trailer-ready level.

10. UX, Tutorial and Feedback

10.1 HUD

Level number.

Reset button always visible.

Environment badge: GRAVITY / ZERO-G during prototype.

No joystick or directional buttons.

Development build may expose pause, slow motion, trajectory and physics diagnostics.

10.2 Tutorial

L01: ghost-finger drag; avoid text if possible.

L02: teach multi-axis rotation.

L04: introduce hazard with unmistakable visual language.

L06: visually show pressure plate → door causality.

L11: clear ZERO-G transition; ball visibly floats/persists.

L12: teach collision redirection before adding bumpers.

10.3 Completion

Exit captures ball instead of allowing it to bounce out.

Short 0.5–1.0 s mechanical/audio payoff.

Optional micro slow-motion at final capture only if it feels better.

Next level transition should be fast; no long results screen.

11. Visual Direction — Prototype

Transparent glass/acrylic cube shell with readable edges.

Internal mechanisms use simple clean primitives with distinct silhouettes.

Neutral ball, high-contrast exit, clear hazards.

Gravity world: grounded workshop/lab feel. Zero-G world: darker void/space backdrop or chamber lighting.

Do not spend prototype time on expensive shaders before physics feel is validated.

Target portrait 9:16 composition but keep safe areas for common phone aspect ratios.

11.1 Performance-friendly rendering

Avoid multiple layers of expensive transparent surfaces.

Prefer one shell material and limited transparent sorting complexity.

Bake/static lighting where practical; use few realtime lights.

Keep post-processing restrained.

Pool repeatable VFX/audio objects if necessary.

12. Audio and Haptics

Event | Audio | Haptic
Drag/snap | Subtle mechanical movement/tick | Light tick on snap.
Roll | Loop volume/pitch from speed | None.
Impact | Impulse-scaled hit | Light/medium by impulse.
Plate/door | Mechanical click/slide | Light.
Bumper | Spring/metal ping | Medium.
Exit | Distinct lock-in + success sting | Success pulse.
Fail | Short muted cue | Optional light.

Audio is functional feedback first. It should communicate mass, contact and mechanism state.

13. Technical Project Structure

Assets/
  _Game/
    Scripts/
      Core/
      Input/
      Physics/
      Environments/
      Mechanisms/
      Levels/
      UI/
      Debug/
    ScriptableObjects/
      Environments/
      Physics/
      Levels/
    Prefabs/
      Core/
      Ball/
      Mechanisms/
      Levels/
    Scenes/
      Bootstrap.unity
      Gameplay.unity
    Materials/
    Audio/
    VFX/
    Tests/

13.1 Assembly boundaries

Core: bootstrap, state machine, service references.

Physics: environment force application and ball physical behavior.

Gameplay/Mechanisms: resettable reusable puzzle objects.

Levels: loading, definitions and completion.

UI: HUD/tutorial only.

Tests: edit-mode and play-mode tests.

14. Core Runtime Responsibilities

System | Responsibility | Must not do
GameBootstrap | Initialize services/config | Contain level-specific logic.
LevelManager | Load/reset/advance level | Implement physics.
InputController | Translate touch/mouse to rotation intent | Move ball.
BoxRotationController | Apply safe LevelRoot rotation | Know puzzle solution.
EnvironmentForceSystem | Apply active environment rules | Hard-code level IDs.
BallController | Ball state/feedback/exit/fail hooks | Own global progression.
Mechanisms | Reusable local behavior | Reference unrelated level objects directly.
ResetSystem | Restore registered state | Reload scene unless fallback.
DebugPanel | Expose diagnostics | Ship enabled in release by default.

15. Codex Implementation Plan

Codex should work in small verifiable milestones. Each milestone must compile, run, and include a short verification note before proceeding. Do not ask Codex to build the whole game in one prompt.

Milestone | Deliverable | Exit criterion
M0 — Project baseline | Unity project settings, folders, Git hygiene, portrait target, test scene | Clean compile; scene runs.
M1 — Box interaction | Camera + transparent box + drag rotation + reset | Stable mouse/touch rotation; no physics yet.
M2 — Gravity ball | Rigidbody ball + explicit gravity + static ramps + exit | Ball rolls/falls predictably and completes level.
M3 — Reset/state | Central reset registry + kill volume | 100 resets without state drift.
M4 — Level framework | LevelDefinition + prefab loading + next level | 3 gravity levels load sequentially.
M5 — Mechanisms | Plate, door, one-way gate, bumper/impulse pad | Each mechanism resettable and reusable.
M6 — Zero-G | Environment profile switching + zero gravity + momentum | 3 zero-G levels demonstrably different.
M7 — Content | 10–20 prototype levels | All levels solvable and ordered.
M8 — Mobile polish | Touch tuning, haptics, audio, performance | Target device stable; no critical input/physics bugs.
M9 — Evaluation build | APK/iOS dev build + debug toggles | Ready for structured playtest.

15.1 Codex working rules

Before coding, inspect the existing repository and summarize relevant files/classes.

Change the smallest coherent surface area per task.

Never rewrite unrelated systems to solve a local issue.

After every change: compile, run relevant tests, report changed files and known risks.

Use serialized fields/config assets for tuning values; avoid scattered magic numbers.

Do not add Phase 2 systems unless explicitly requested.

Do not claim success from code inspection alone; verify in Unity or through automated tests where possible.

Maintain a DEVELOPMENT_LOG.md with milestone, changes, tests, open issues and next step.

15.2 Recommended first Codex prompt

You are implementing Milestone M1 of Gravity Box in an existing Unity project.
First inspect the repository and report the current scene/project structure. Then implement only:
1) a LevelRoot that can be rotated with one-finger drag and mouse drag,
2) a fixed perspective CameraRig framing the complete cube,
3) configurable rotation sensitivity,
4) an optional assisted snap near 90-degree orientations,
5) a Reset action restoring LevelRoot orientation.
Do not implement ball physics, levels, Phase 2 mechanics, monetization or production art.
Keep code modular, mobile-safe and testable. After implementation, compile and verify behavior, then list changed files, test results, tuning fields and remaining risks.

15.3 Subsequent prompt pattern

TASK: <one milestone or one bug>
CONTEXT: Read GDD sections <X–Y>.
CONSTRAINTS: Do not modify unrelated systems. Do not implement Phase 2.
ACCEPTANCE CRITERIA:
- <observable behavior 1>
- <observable behavior 2>
- clean compile
- reset remains deterministic
VERIFY:
- run tests / Unity play check
- report files changed
- report any deviation from GDD
STOP after acceptance criteria are met.

16. Debug and Developer Tools

Toggle gravity vector visualization.

Display ball linear velocity, angular velocity, speed and sleep state.

Display current EnvironmentProfile.

Slow motion: 0.25× / 0.5× / 1× for diagnosis only.

Pause/step physics if practical.

Draw exit/kill trigger gizmos.

Show LevelRoot Euler/quaternion debug values.

Button: Reset Level; Reload Level; Next Level.

Optional trajectory line in zero-G based on current velocity; debug-only initially.

17. Testing Strategy

17.1 Automated tests

Test | Type | Pass condition
EnvironmentProfile Earth | Edit mode | Gravity magnitude/direction resolves correctly.
EnvironmentProfile Zero-G | Edit mode | No gravity force is applied.
Reset ball | Play mode | Transform and velocities equal captured initial state.
Reset mechanism | Play mode | Door/plate/bumper returns to initial state.
Exit completion | Play mode | Exactly one completion event fires.
Kill volume | Play mode | Fail/reset path is valid.
Level load | Play mode | Definition spawns correct prefab/environment.
Repeated reset | Play mode | No duplicate listeners or state accumulation.

17.2 Manual physics matrix

Scenario | What to test
Slow rotation | Ball contact remains stable.
Fast rotation | No explosive tunneling/penetration.
Corner contact | No permanent jitter.
High-speed ball | No exit/wall tunneling.
Reset during motion | Exact restoration.
Reset during mechanism animation | No orphaned coroutine/tween/state.
Zero-G long drift | No unwanted acceleration.
Device frame drop | Physics remains playable and does not catastrophically diverge.

17.3 Device matrix

At least one mid-range Android device.

At least one iPhone representative of target audience.

Unity Editor mouse simulation.

Test 30/60 FPS presentation if frame cap changes; physics timestep must remain explicit.

18. Performance Budget — Prototype

Area | Target / rule
Dynamic rigidbodies | Prefer 1–5; avoid unnecessary dynamic mechanisms.
Mechanisms | Typically <30 active components per level.
Physics timestep | 1/60 baseline; profile before increasing solver cost.
Colliders | Primitive/compound preferred.
Transparent shell | Minimal overlapping transparency.
Draw calls/materials | Keep prototype simple; batch where practical.
GC allocations | No recurring allocations in FixedUpdate/Update hot paths.
Frame target | 60 FPS preferred; 30 FPS acceptable only as fallback, not as excuse for unstable physics.
Load/reset | No visible loading on reset; next-level transition should feel immediate.

Physics should be profiled on real devices before optimizing. Do not prematurely replace PhysX with a custom solver unless measurements prove it necessary.

19. Analytics for Internal Playtest

level_start / level_complete / level_reset / level_fail

time_to_complete

reset_count

rotation_drag_count

total_drag_distance or approximate rotation amount

environment_id

completion rate per level

optional: number of assisted snaps

Phase 1 analytics may be local logs/CSV only. The purpose is to identify confusing levels and whether zero-G is understandable, not to build production telemetry.

20. Prototype Acceptance Criteria

Area | Pass
Control | New player can rotate box immediately; touch and mouse both work.
Physics | Ball motion is stable, legible and broadly repeatable.
Gravity | At least 8–10 gravity puzzles are meaningfully distinct.
Zero-G | At least 5–6 zero-G puzzles require momentum reasoning, not merely cosmetic gravity removal.
Reset | Reliable under repeated use; no state leakage.
Performance | Target mobile devices remain responsive with no major physics spikes.
Readability | Ball, exit, hazards and active mechanisms are visually clear.
Fun signal | Playtesters voluntarily retry and can describe why they failed.
Architecture | Adding a future force provider does not require rewriting LevelManager/Input/ball state machine.

21. Go / No-Go Gate Before Phase 2

GO if

Rotation itself feels satisfying.

Players understand cause/effect without lengthy tutorial.

Gravity levels show enough design space for short-session mobile play.

Zero-G creates a genuinely different mental model.

Physics bugs are rare enough that players blame their decision, not the game.

Content can be authored quickly from reusable pieces.

NO-GO / redesign if

Most solutions reduce to random spinning.

Free rotation creates too much chaotic physics.

Zero-G is confusing or boring without too many extra controls.

Level authoring requires bespoke code per puzzle.

Reset/replay is inconsistent.

Transparent 3D readability is poor on phone screens.

22. Phase 2 Roadmap — Planned, Not Implemented

System | Core rule | Example puzzle value | Implementation approach
Material profiles | Ball/object material changes interactions | Steel vs glass vs rubber | ScriptableObject physical/material coefficients.
Magnetism | Magnetic force acts on compatible materials | Attract/repel steel ball | Localized force field; gameplay curve, not full EM simulation.
Water | Buoyancy + drag + current | Sink/float/current routing | Volume + buoyancy/drag; visual shader, no full fluid sim.
Wind | Directional/turbulent force | Light objects drift; heavy objects resist | Force volume + tuned noise.
Acid | Corrosion/state change | Dissolve blockers or destroy vulnerable ball | Trigger/volume + timed state effect; visual dissolve.
Electricity | Conductive state / powered mechanisms | Steel ball bridges circuit | State graph, not circuit simulator unless needed.
Multiple balls | Coupled objectives | Move one while holding another | Same physics interface + goal conditions.
Advanced mechanisms | Gears, pulleys, counterweights, portals | Rube-Goldberg chains | Reusable components with reset contracts.
Procedural tooling | Generate/validate puzzle candidates | Large content scale | Constraint generator + automated simulation/solver later.

22.1 Recommended Phase 2 order

P2.1 MaterialProfile foundation.

P2.2 Magnetism.

P2.3 Wind.

P2.4 Water buoyancy/current.

P2.5 Acid/state transformation.

P2.6 Multiple balls and material combinations.

P2.7 Level-editor productivity tools.

P2.8 Procedural generation/solver research only after authored rules are proven.

23. Phase 2 Combination Matrix

Environment / Ball | Steel | Glass | Rubber | Magnetic
Gravity | Heavy predictable baseline | Fragile impact puzzle | High bounce | Baseline + magnetic future.
Zero-G | Momentum baseline | Fragile collision routing | Elastic rebound | Magnetic trajectory control.
Magnetic field | Strong attraction | No attraction | Usually no attraction | Attract/repel depending polarity.
Water | Sink | Density-tuned | Often float | Sink + magnet interactions.
Wind | Low response | Medium | High | Mass-dependent.
Acid | Material-specific corrosion | Potential etch/break | Potential dissolve | Material-specific.

The matrix is a design roadmap, not a promise that every combination must ship. Each interaction must earn its complexity through clear puzzle value.

24. Risks and Mitigations

Risk | Impact | Mitigation
Rotating geometry destabilizes contacts | High | Prototype both physical-root rotation and logical gravity-vector rotation early.
Random spinning solves levels | High | Use gates, sequencing, hazards, narrow affordances and assisted orientation.
Zero-G lacks agency | High | Provide bumpers/impulse mechanisms and readable trajectory planning.
Physics differs by frame/device | Medium | Fixed timestep, velocity caps, forgiving geometry, practical repeatability.
Transparent box becomes visually noisy | High | Minimal shell, strong silhouettes, limited overlapping transparency.
Scope explosion | High | Hard Phase 1 exclusion list; milestone gates.
Codex over-refactors | Medium | Small prompts, acceptance criteria, repository inspection, changed-file review.
Level production becomes slow | High | Prefab library, LevelDefinition data, no bespoke scripts, later editor tools.

25. Definition of Done for the Prototype

Unity project opens with zero compile errors.

Gameplay scene loads Level 01 and can advance through the prototype set.

Touch/mouse box rotation is responsive and configurable.

Gravity and Zero-G are data-driven EnvironmentProfiles.

Ball/physics state resets reliably.

Required mechanisms are reusable and resettable.

At least 15 polished test levels exist, with a target of 20 slots.

Development diagnostics can explain physics failures.

Build runs on at least one Android and one iOS test device or equivalent available target devices.

Known issues are documented in DEVELOPMENT_LOG.md.

Phase 2 code is not prematurely mixed into the prototype.

Appendix A — Suggested Initial Unity Configuration

Setting | Starting value | Reason
Orientation | Portrait | Primary mobile framing.
Target FPS | 60 | Tactile rotation/physics.
Fixed Timestep | 0.0166667 | 60 Hz physics baseline.
Physics.gravity | 0,0,0 for controlled architecture | Environment system applies force.
Solver iterations | Unity default initially | Increase only after evidence.
Input | Unity Input System or existing project standard | Touch + mouse parity.
Color space | Project standard; Linear preferred if supported | Visual consistency.
Quality | Simple prototype tier | Physics feel before graphics.

Appendix B — Repository Handoff Checklist for Codex

Place this GDD at project root or /Docs/GRAVITY_BOX_GDD.docx; optionally export a Markdown companion for easier agent parsing.

Create /Docs/DEVELOPMENT_LOG.md.

Create /Docs/DECISIONS.md for architectural decisions such as rotation convention.

Codex must read the relevant GDD section before each milestone.

Every milestone ends with: compile status, test status, changed files, tuning values, known issues, next task.

Commit after stable milestones so physics experiments can be rolled back cleanly.
