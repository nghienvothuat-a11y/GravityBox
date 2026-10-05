# COghe: control-feel audit (rail jitter, lift button, force feedback)

- **Repo:** GravityBox, branch `NewGraphic`, HEAD `dee018ca`, clean tree.
- **Date:** 2026-10-05.
- **Method:** read-only. I didn't edit the repo or run Unity. File paths are relative to the repo root.

Evidence labels used throughout:

- **[C] Confirmed by reading** the code, scene YAML or committed data.
- **[M] Offline model.** A 1-D Python model that re-implements the update equations of `COgheTapRail` and `COgheRailSlider` at 1/120 s, using the masses and gains from the scene files. It isn't a Unity measurement. See Appendix A.
- **[H] Hypothesis.** Needs measurement.

---

## 0. Summary

### 1. The main jitter is a control-loop bug, not rendering [C+M]

Every Spatial handle is a `COgheTapRail` driving a `COgheRailSlider`. The handle's force is a velocity servo, and it has two problems:

- **The force arrives one physics tick late.** On each handle GameObject, the slider component comes before the TapRail component, so the slider applies the force the TapRail computed on the previous tick.
- **The gain is too high for these carriages.** The gain uses a 0.18 kg mass floor, but the carriages weigh 30–35 g.

The loop gain per tick is K ≈ 1.07, which is above 1. So:

- the handle oscillates at about 20 Hz;
- its effort slams between ±0.672 N;
- the equal-and-opposite reaction shakes COghe's particles.

**Scope:** 43 of the 59 tap rails in the 50-level catalog are in this unstable regime, including levels 4, 5, 6, 7, 9 and 10. The other 16 are stable but ring after disturbances.

**Fixes:**

- **One line:** make the gain proportional to the carriage's real mass and add friction feed-forward. This removes the oscillation in the model.
- **Structural:** step the rails after every mechanism that applies force to them.

### 2. Mechanisms are not interpolated, but COghe is [C+M]

Handles, doors, decks and the lift are all non-interpolated Rigidbodies. COghe's particles are interpolated.

Physics runs at 120 Hz. On OPPO, frame times are quantised to a mix of 16.6 ms and 33.2 ms. So each displayed frame advances the mechanisms by a different number of ticks, and they judder next to a smoothly moving creature.

### 3. The navigation graph is rebuilt about every 0.35 s while a rail face moves [C]

Each rebuild cost 7–19 ms on a Mac M5 in the pilot mechanism levels. That produces frame hitches, followed by multi-tick catch-up frames.

### 4. Lift in level 8 [C]

How it works today:

- The player taps a blue puck on the deck.
- COghe walks to the deck centre.
- After COghe has been fully on the deck for 0.2 s, the motor runs.
- The puck sinks 4 mm while the lift moves, then rises again on arrival.

What's missing:

- COghe never presses the button.
- The button sits about 4 cm from where COghe stands, and nothing stops the body from covering it.
- There is no state light and no press sound.

**The lift can already go back down:** tap the puck again. A PlayMode test covers this.

The proposal is an explicit button state machine, moving the button, a press tendril, a status ring and sound hooks.

### 5. A part that is too weak fails silently in the shipping UI [C]

Today:

- Strength is 7 m/s² × tissue mass, so the whole body produces 0.672 N.
- Nothing is checked when the player taps. The part walks over, grabs and pulls, and the visuals are identical to a successful pull.
- Static friction holds the object completely still.
- After the stall timeout (3–6 s), the task cancels with a Vietnamese message. The shipping Product UI never shows that message.

The proposal is to give this case its own Strain / TooHeavy state, based on real effort saturation:

- ramp the effort up to full strength;
- show taut tentacles and a tremble;
- let the object give a few millimetres, then spring back;
- show a weight badge with the share of the body it needs;
- play sounds.

Exact hook points are listed in §3.4.

---

## 1. Jitter when pulling mechanisms

### 1.1 How a pull is simulated today [C]

**Timing**

- **Fixed step is 1/120 s.**
  - Set in `ProjectSettings/TimeManager.asset:7-11` (1176000 / 141120000).
  - Also set by `PhysicsTiming.Apply()` in `Assets/_Game/Scripts/Simulation/PhysicsTiming.cs:7-11`. The same call sets `maximumDeltaTime = 0.1` and Unity gravity to 0.
  - Applied when the campaign starts, together with `Application.targetFrameRate = 60` (`Assets/_Game/Venom/Runtime/VenomLevelController.cs:349`).
- **Android and iOS** default to quality level "Medium" (`ProjectSettings/QualitySettings.asset:328-346`), which has `vSyncCount: 1` (`:138`).

**Order of work inside one tick** (runtime files are in `Assets/_Game/Venom/Runtime/`)

1. `VenomLevelController.FixedUpdate` calls `Step` (`VenomLevelController.cs:113-117`), which calls `VenomCampaign.Step` (`VenomCampaign.cs:164-189`).
2. `VenomCampaign.Step` runs, in order:
   - prop gravity (`:168-172`);
   - `Matter.Step` (`:173`);
   - `Motion.Step`, which applies the creature's forces (`:176`);
   - `StepProp` through `StepMechanisms` (`:177`).
3. PhysX simulates after FixedUpdate.

`StepMechanisms` steps the `Mechanisms[]` array in `GetComponentsInChildren` order. The only reordering is that `COgheCooperativeDrive` is moved to the end (`VenomCampaignExpansion.cs:16-31`, `:38-40`).

**Level 4 scene** (`Assets/_Game/Venom/SpatialCampaign/COgheSpatial04.unity`)

How the builder creates it (editor files are in `Assets/_Game/Editor/`):

- `COgheSpatialCampaignBuilder.cs:87-94` calls `ViewTask`.
- `ViewTask` is at `COgheViewCampaignBuilder.cs:176-178` and calls `TapRail`.
- `TapRail` (`COgheTapCampaignBuilder.cs:128-139`) builds a 35 g carriage with Resistance 0.010 N and latches at both ends.
- `ViewGate` (`COgheViewCampaignBuilder.cs:180-195`) builds a 22 g shutter.
- `ViewLink` (`:196-201`) connects them with a `COgheViewMechanism`.

What the scene contains:

| Item | Value | Scene line(s) |
|---|---|---|
| Order of the Apparatus children | `Fixed chamber` (contains "Mechanical linkage"), `A carriage`, `A blue shutter` | `:4117-4119` |
| `A carriage` component order | Transform, Rigidbody, VenomMovableProp, **COgheRailSlider**, ConfigurableJoint, **COgheTapRail** | `:1422-1427` |
| `A carriage` mass | 0.035 | `:1651` |
| `A carriage` kinematic? | no | `:1666` |
| `A carriage` interpolation | **None** | `:1667` |
| Shutter mass | 0.022 | `:2823` |
| Shutter interpolation | **None** | `:2839` |
| TapRail settings | Speed 0.09, StallSeconds 3, CompensateLoad off, TrackStandPoint on | `:1475-1494` |
| Slider settings | Travel 0.16, Resistance 0.01, Damping 0.08, CatchTolerance 0.003, latches at both ends | `:1614-1622` |

**The rail itself** (`COgheRailSlider.cs`)

- It is one degree of freedom on a ConfigurableJoint.
- The brake works by setting the joint's `xMotion` to Locked at the current position (`:36-42`).
- End latches re-arm on every tick while the rail is inside CatchTolerance (`:45-46`). They release only when the effort exceeds Resistance + 4 mN in the opposite direction (`:49`).
- The force each tick is: pending effort, plus gravity, minus Coulomb friction, minus Damping × velocity (`:48-58`). The pending effort is then cleared (`:59`).

**How the handle is driven** (`COgheTapRail.StepMechanism`, `:182-246`)

1. Target speed: `desired = clamp(3·(target − pos), ±Speed)` (`:231`).
2. Gain: `gain = max(m_rail, 0.18) · 25` (`:234`). For a 35 g carriage that is 4.5 N·s/m.
3. Optional integral term (`:235`).
4. The effort is clamped to ±7 m/s² × the tissue mass doing the pulling (`:236-237`). For the whole body that is ±0.672 N.
5. The effort goes to `Rail.ApplyEffort` (`:242`), which only queues it for the slider's next `StepMechanism`.
6. The reaction on COghe:
   - `BraceAgainstManipulation` pushes the gripping feet with +force (`:243`, `VenomCampaignMotion.cs:94-115`);
   - every particle receives −force / actorCount (`:244-245`).

**How COghe is coupled to the handle**

- There is no joint or spring between them.
- While the task is Operating, `Motion.Step` uses the target and velocity from `TryIntent` (`VenomCampaignMotion.cs:470-471`, `COgheTapRail.cs:92-104`):
  - a feed-forward along the rail, using the same 3·err law;
  - plus a 4/s pull toward `StandPoint`, which is derived from `Handle.position` (`:66-67`).
- Each particle is a velocity servo with gain 26/s, capped at 5 m/s² (`VenomCampaignMotion.cs:503-504`).
- The task cancels if fewer than 2 feet grip, or the body centre is more than 0.145 m from the handle (`COgheTapRail.cs:226-227`).
- The tentacles are presentation-only tubes drawn from the body centre to `HandPoint` (`VenomLifeAnimation.cs:282-315`).

**Link from handle to door**

- `COgheViewMechanism` applies a PD force to the dynamic shutter on every tick: 5 N/m, 0.7 N·s/m, capped at ±0.45 N (`COgheViewMechanism.cs:19-26`).
- It doesn't use `MovePosition` and nothing runs in `Update`.
- With a 22 g shutter this is overdamped (ζ ≈ 1.2), so the door low-passes handle noise [C/M].

**Rendering**

- The skin is rebuilt in every `LateUpdate` from the **interpolated** particle transforms (`VenomSurface.cs:72-99`). The particles use `Interpolate` (`CohesiveOrganism.cs:60`).
- The shell is deliberately non-interpolated (`VenomLevelController.cs:351-353`).
- Every prop is built non-interpolated (`Assets/_Game/Editor/VenomCampaignBuilder.cs:220`).
- Across all Spatial scenes, 234 of 241 Rigidbodies have `m_Interpolate: 0` (count from a scene scan).

**Camera**

- Spatial levels are `ViewOnly`, so "follow" just maps to Overview (`VenomCampaignCamera.cs:77-82`).
- Framing targets static bounds with exponential smoothing on unscaled time (`:133-157`).
- **The camera is not a jitter source in the Spatial levels.** (The Origin follow camera is a separate case; see §1.2 #6.)

### 1.2 Ranked root causes

#### #1: The handle servo is unstable (one-tick force delay × gain too high)

**Confidence and priority:** HIGH. The component order, masses and gains are confirmed by reading [C]. The size of the effect comes from the model [M].

**Evidence**

1. **The force is delayed one tick.**
   - On every handle GameObject, `COgheRailSlider` comes before `COgheTapRail` (scene `:1422-1427`).
   - The builders add the slider in `ExpansionRail` (`Assets/_Game/Editor/VenomExpansionMechanismsBuilder.cs:62`) and only afterwards add the TapRail (`COgheTapCampaignBuilder.cs:133`).
   - So on tick n the slider applies the effort the TapRail computed on tick n−1. The TapRail's new effort, computed from vₙ, waits until tick n+1.
   - A scan of all 50 catalog scenes found this same order on **all 59** tap rails.
2. **The gain is too high.**
   - Loop gain per tick: `K = max(m,0.18)·25·dt / m`.
   - 35 g gives K = 1.07. 30 g gives 1.25. 40 g gives 0.94. 60 g gives 0.63.
   - The 0.18 kg floor matches the default prop mass (`VenomCampaignBuilder.cs:220`). It has been there since commit `0f4a48e4` (found with `git log -L`) and was never rescaled for the 35 g carriages.
   - `HoldAtEnd` already uses `20·m` instead (`COgheTapRail.cs:234`).
3. **What that does to the loop.**
   - With one tick of delay, the loop's characteristic equation is z² − (1−c)z + K = 0, where c is the slider's damping term per tick. So |z| = √K.
   - When K > 1 the oscillation grows by about 61° per tick (about 20 Hz) until the effort clamp saturates. The result is bang-bang control at ±0.672 N.
   - **43 of 59** tap rails have K > 1. These are all the 30–35 g carriages: Spatial 04, 05, 06, 07 (the bridge), 09, 10, 14, 15, 17, 19, 20, 23, 26–30, and Plus B1, B2, E02, E03, E11, E12, E14, E15, E18.
   - The other 16 (40–80 g, plus the 220 g crate C1) are stable but ring. A 40 g carriage has |z| = 0.97, so it rings at about 20 Hz for about 0.3 s after each disturbance.

**Model results [M]**

Level 4 parameters, whole body, handle displacement per displayed frame while pulling:

| Variant | 60 fps step (mm): mean / sd / min…max | 45 fps step sd (mm) | Effort range |
|---|---|---|---|
| **Current** (slider before TapRail, 0.18 kg floor) | 1.39 / **1.90** / **−0.25…4.09** | 2.13 | **−0.672…+0.672 N** |
| Rails stepped after the TapRail | 1.42 / 0.08 / 1.01…1.44 | 0.34 | 0.008…0.017 N |
| Gain = 25·m_rail, delay kept | 1.20 / 0.00 | 0.28 | 0.016 N |
| Gain = 25·m_rail + friction feed-forward + old integral rate, with a 0.22 N load | 1.50 / 0.03 | n/a | n/a |
| Current, half body | 1.37 / 0.93 / −0.19…2.07 | 1.09 | ±0.336 N |
| Current + CompensateLoad (pulley input in levels 6, 9, 10) | 1.44 / 1.93 / −1.79…3.17 | 2.12 | ±0.672 N |

What the model shows:

- In the current setup the handle's velocity swings between −0.09 and +0.25 m/s around its 0.09 m/s target. On some frames it moves backwards.
- The **mean** speed is unchanged, so the pull finishes at the normal time.
- That explains why the solve tests miss it: they only assert end states (for example `RailTrip` in `Assets/_Game/Tests/PlayMode/COgheSpatialRecoveryTests.cs:11-15`).

**Why COghe shakes too**

- The reaction (`COgheTapRail.cs:244-245`) applies ±0.021 N to each 3 g particle, about ±7 m/s², at about 20 Hz.
- That is more than the 5 m/s² cap on the body's own servo (`VenomCampaignMotion.cs:503`), so the body can't cancel it.
- At the same time, bracing pushes the planted feet the opposite way (`VenomCampaignMotion.cs:106-111`). The feet and the rest of the body oscillate against each other [M for the size; H that it's visible, roughly 0.5–1.3 mm].

**Possibly related symptoms [H]**

- The author route script expects pulls to "lose footing" partway and re-taps the handle (`ChapterProof/COgheSpatialNextScenario.cs:42-47`).
- The slide-loop audio volume follows the rail's frame-to-frame speed (`Audio/COgheAudio.cs:225-228`, `:294-295`), so it will warble along with the oscillation.

**Fix.** Do both. A is the minimal patch.

- **A. Gain from the real carriage mass** (`COgheTapRail.cs:234`):
  1. Change the proportional gain to `Rail.Body.mass * 25` (keep `* 20` for `HoldAtEnd`).
  2. Add a friction feed-forward of `Mathf.Sign(desired) * Rail.Resistance` while |desired| > 1e-4.
  3. Keep `Mathf.Max(Rail.Body.mass, .18f) * 25` only inside the CompensateLoad integral on `:235`.

  The model stays smooth with this change and still lifts a 0.22 N load. Rails of 0.18 kg or more are unaffected.
- **B. Structural: step rails after the mechanisms that drive them.**
  - In `VenomCampaignExpansion.InitializeMechanisms` (`:19-24`), order the array as: other mechanisms → `COgheCooperativeDrive` → `COgheRailSlider`.
  - That removes the one-tick delay for every producer: TapRail, PassengerLift, and anything that sets `Locked`.
  - **Risk:** several mechanisms read `Rail.Effort` and would then see the previous tick's sum:
    - `COgheDockedBridgeDeck.cs:25`
    - `COgheTwoDockDeck.cs:17-18`
    - `COgheSequentialWinch.cs:29`
    - `COgheCooperativeWinch.cs:34`
    - `COgheCooperativeDockTransmission.cs:54`
    - `COgheTwoStageWinch.cs:41-43`

    Run the full PlayMode regression and the 50 native replays after this change.
- **Not a fix:** more solver iterations or pinning poses. The rules forbid that as a way to hide a structural problem (`Docs/COGHE_LEVEL_DESIGN_RULES.md:221-222`).

**How to verify**

- **Trace every tick** through the existing harness hook (`trace` in `Assets/_Game/Tests/PlayMode/COgheSpatialCampaignTests.cs:36`, called from `Tick()`). Log: `Rail.Position`, axial velocity, `Rail.Effort`, `AppliedEffort`, actor centre, and feet count.
- **Pass criteria** while 0.02 m < position < Travel − 0.02 m:

  | Metric | Pass if | Model value today |
  |---|---|---|
  | Sign changes of axial acceleration per 0.5 s | ≤ 2 | about 20 |
  | Max \|`Rail.Effort`\| in level 4 | ≤ 0.1 N | 0.672 N |
  | sd of 2-tick handle displacement | ≤ 0.2 mm | 1.9 mm |
  | RMS axial acceleration of the creature's centre | drops at least 5× | n/a |

- **New test `TapRailsPullSmoothly`:** operate every `COgheTapRail` in the 50 levels (reuse the scenario `Operate` helpers) and apply the criteria above.
- **New static test `MechanismOrderPutsProducersBeforeRails`:** assert that every slider is stepped after every mechanism that calls `ApplyEffort` on it or sets its `Locked` flag.

#### #2: Mechanisms aren't interpolated, but COghe is

**Confidence and priority:** HIGH on device. The settings are confirmed [C], the aliasing comes from the model [M], and the size on a real device is a hypothesis [H].

**Evidence**

- Rail bodies use interpolation None (`VenomCampaignBuilder.cs:220`; 234 of 241 Spatial bodies). Particles use Interpolate (`CohesiveOrganism.cs:60`). The GDD asks for interpolation (`Docs/GDD_REFERENCE.md:141`, `:175`).
- At 120 Hz physics, a 60 fps frame normally contains 2 ticks, but any frame-pacing jitter turns that into 1 or 3.
- OPPO data (`Docs/Verification/COgheMobile/optimized.jsonl`, levels 11–20) shows p50 16.6 ms and p95 33.2 ms in every phase. That means frames alternate between 2 and 4 ticks.
- So non-interpolated handles, doors and decks move twice as far on some frames as on others, while the interpolated creature glides. This shows up as stutter, and as COghe slipping relative to the handle or lift deck.
- The Spatial levels haven't been measured on Android at all (`Docs/LevelDesign/COghe/SpatialPilot/Level04/README.md:41`).
- In the model, even after fix #1, the per-frame handle step at 45 fps varies from 1.18 to 2.16 mm (sd 0.34). That is purely tick aliasing.

**Fix**

1. **Check one thing first.** The shell was left non-interpolated on purpose, so that routing and contact queries read the 120 Hz pose (`VenomLevelController.cs:351-353`). Simulation code reads moving faces through their transforms:
   - `VenomCampaignMotion.cs:403`
   - `BuildGraph` at `:163`
   - `VenomNavigationRevision.cs:29-30`
   - `COgheTapRail.HandPoint` at `:66`

   So first add a PlayMode assertion that checks, inside FixedUpdate in Unity 6000.3, that an interpolated rail satisfies `rail.transform.position == rail.Body.position`.
2. **If the assertion holds:** set `interpolation = Interpolate` on every rail and prop body, in the builder `Prop()` and/or at runtime in `COgheRailSlider`.
3. **If it doesn't hold**, choose one:
   - compute `HandPoint` / `StandPoint` from `Body.position` and rotation, and build the face matrices from the Rigidbody pose; or
   - keep the bodies non-interpolated and drive a separate visual root in `LateUpdate` (manual interpolation).
4. **Switch presentation code** that samples the physics pose in `LateUpdate` over to the interpolated transform. Examples:
   - `COghePulleyDrive.LateUpdate`: the cable end uses `Output.Body.position`, and the drum and wheels use `Rail.Position` (`COghePulleyDrive.cs:26-34`);
   - `COgheTapGatePresentation.cs:20-26`.
5. **[H] Consider** `Time.maximumDeltaTime` ≈ 1/30 on mobile (`PhysicsTiming.cs:11`) to cap catch-up bursts.

**How to verify**

- **Player-loop PlayMode test** (not Script mode):
  - set `Time.captureDeltaTime` alternating between 1/60 and 1/30 s, or random between 14 and 20 ms;
  - record the handle's transform on every rendered frame during a pull;
  - rendered speed = Δpos / Δt. Pass if its coefficient of variation is below 10%. This fails today.
- **Same check on OPPO**, using a dev-only trace that records per frame:
  - `unscaledDeltaTime`;
  - ticks that ran in the frame;
  - the handle transform;
  - the creature's rendered centre.

#### #3: Navigation graph rebuilds while mechanisms move

**Confidence and priority:** MEDIUM-HIGH. This matches "sometimes it stutters". The trigger is confirmed [C], the cost on a Mac is confirmed [C], and the device cost is a hypothesis [H].

**Evidence**

- **What triggers a rebuild.** Every 0.35 s, `StepMechanisms` calls `Motion.BuildGraph()` if any surface has moved more than 1 cm (`VenomCampaignExpansion.cs:41-46`). The cached graph counts as stale once any prop moves more than 0.05 mm (`VenomNavigationRevision.cs:22-38`).
- **How often that happens.** A pull at 0.09 m/s moves about 3 cm per 0.35 s window. So there is a full rebuild, including `Physics.SyncTransforms` (`VenomCampaignMotion.cs:152-196`), roughly every 0.35 s. There are extra rebuilds:
  - when an aperture opens (`COgheViewMechanism.cs:32`);
  - when a task starts (`COgheTapRail.cs:133`);
  - when a task completes (`:225`).
- **What a rebuild costs on a Mac M5** (`Docs/Verification/COgheSpatialPilot/mac-native-run.json`, field `maxGraphMs`):

  | Level | Max single graph build |
  |---|---|
  | 1–3 | 0 ms |
  | 4 | 7.3 ms |
  | 5 | 14.6 ms |
  | 6 | 11.4 ms |
  | 9 | 14.2 ms |
  | 10 | 19.4 ms |

  The worst frame in those levels was 23–33 ms. A mid-range Android phone will be several times slower. The result is one long frame followed by a 3–4-tick catch-up, which #2 makes visible.

**Fix**

- Skip the periodic rebuild while the only faces moving belong to a rail with an active task.
- Rebuild once when the rail settles (completion already does this), or update the graph locally. The rules already name local or spread-out updates as the next step (`Docs/COGHE_LEVEL_DESIGN_RULES.md:209-211`).

**How to verify:** use the `COgheMobileMetrics` graph counters during a pull, and correlate frames over 20 ms with graph builds.

#### #4: Skin and tentacles mix interpolated and physics poses

**Confidence and priority:** MEDIUM. The code is confirmed [C]; whether it is visible is a hypothesis [H].

**Evidence**

- **Tentacles.** Each tendril ends at `HandPoint`, which is `Handle.position` at the latest physics pose. It starts from the body centre, which comes from interpolated particle transforms (`VenomLifeAnimation.cs:123`, `:282-312`). So tendril length and angle flicker by up to one tick of motion (about 0.75 mm at 0.09 m/s), and far more while #1 is active.
- **Skin clipping.** The skin is clipped against planes taken from moving faces at their physics pose (snapshot in `VenomCampaignSkin.cs:16-24`, applied in `VenomCampaign.cs:507-540`), while the particles are interpolated. On the level 8 lift deck, moving at 0.075 m/s, the skin's bottom is clamped to a deck plane that can be one tick ahead. COghe may appear to sink or bob relative to the deck.

**Fix:** this mostly resolves with #2, because rails and particles will then be posed consistently in `LateUpdate`. If you choose the manual-interpolation route, feed the same interpolated poses to the skin and tendril code.

#### #5: Pops at grab and release, and an animation clock that advances in ticks

**Confidence and priority:** LOW-MEDIUM. The code is confirmed [C]; whether it is visible is a hypothesis [H].

**Evidence**

- **Pops.** On the frame the task becomes Operating, the body stretch jumps from 1 to 1.16 when pulling (or 0.88 when pushing), and it jumps back on release (`VenomLifeAnimation.cs:285`). The two tendrils appear and disappear within one frame (`:305-315`).
- **Clock.** The life-animation clock advances by `SimulationTime` deltas (`VenomLifeAnimation.cs:87-93`), so it moves by 1–4 ticks per frame while positions are interpolated. Wobble and tendril phases therefore step unevenly.

**Fix**

- Smooth an attach weight (`SmoothDamp`, about 0.12 s) and grow or retract the tendrils instead of popping them.
- Drive the presentation clock from a clamped `Time.deltaTime`.

#### #6: Minor items [C]

- **Door kick at the ends.** The door's target snaps by 2.6 mm when the handle enters or leaves the 3 mm end zones (`COgheViewMechanism.cs:21`). That gives a small kick at the start and end of the pull. Use `Input.Fraction` directly.
- **Start latch toggling.** The start latch re-arms on every tick (`COgheRailSlider.cs:45-49`). With #1 active it toggles about 3 times at the start of a pull (model), and each toggle reconfigures the joint. Fixed by #1.
- **Pulley cable.** Stiffness 45 and Damping 0.12 (`COghePulleyDrive.cs:13`; scene `COgheSpatial06.unity:8162-8164`) are lightly damped in isolation. In a 2-body model, though, the deck follows the carriage without going backwards once #1 is fixed (frame step sd 0.37 mm). It is not a primary cause [M].
- **Origin follow camera (non-Spatial levels).** It targets the raw Rigidbody centre (`VenomCampaignCamera.cs:128`, `VenomCampaignMotion.cs:60-66`), so the creature jitters on screen in follow mode. It should use the interpolated transforms. This doesn't apply to the ViewOnly Spatial levels.
- **Visual-only transforms written in FixedUpdate.** These step at the tick rate, so they suffer the same aliasing as #2:
  - the interlock pin (`COgheTapRail.cs:190`);
  - the transmission wheels (`COgheViewTransmission.cs:83-85`);
  - pad caps (`COgheTissueSensor.cs:32`).

### 1.3 Verification plan

1. **Instrument the build.** Add a dev-only `COgheFeelTrace`, gated the same way as the benchmark / `COgheMobileMetrics`. It writes a CSV to `persistentDataPath`:
   - **per tick:** t, rail position, velocity, Effort, AppliedEffort, feet, body centre;
   - **per frame:** `unscaledDeltaTime`, ticks in the frame, handle transform, rendered creature centre, graph builds.
2. **Compute these metrics:**
   - rate of acceleration sign changes;
   - max |Effort|;
   - coefficient of variation of rendered speed;
   - per-frame handle-vs-creature displacement sd;
   - frames over 20 ms, split by whether they contain a graph build.
3. **PlayMode tests:**
   - script mode: `TapRailsPullSmoothly` over all 59 rails;
   - the static ordering test;
   - the player-loop interpolation test;
   - then all existing suites.
4. **Device:** OPPO, Spatial 04/06/07/08/09, before and after. Record results in the format of `Docs/COGHE_LEVEL_DESIGN_RULES.md:264-274`: date, device, commit, build SHA, scenario, p95/p99, and frames over 33 ms.

---

## 2. Level 8: a lift button that reads as a button

### 2.1 How `COghePassengerLift` works today [C]

**How it's built**

- `SpatialLift` (`COgheSpatialCampaignBuilder.cs:137-146`) creates a 40 g "Passenger elevator" rail with 0.30 m of vertical travel.
- The deck faces move with it (`MotionFrame` = the rail body, `:142`).
- The **Panel** is a metal cylinder:
  - 50 mm across × 24 mm tall (scale 0.05 / 0.012 / 0.05);
  - at deck-local (0, 0.023, −0.04): it sticks up 17 mm above the deck, 4 cm from the deck centre;
  - with **no collider** (`MechanismVisual` destroys it, `VenomExpansionMechanismsBuilder.cs:52-56`);
  - coloured blue (`:114`). The art pass keeps "its depression motion" (`COgheSpatialArtBuilder.cs:185-192`).
- Scene file: the panel is at `COgheSpatial08.unity:973-989` and the lift fields at `:4884-4893`. There are no CallPanels, no BoardPoint, and `ReturnWhenDisabled` is off.

**What a tap does**

1. The tap ray is tested against a 9 × 6.5 × 8.5 cm box around the Panel (`COghePassengerLift.cs:42`).
2. While the lift is moving (or its required rail isn't at its end), the tap is swallowed (`:43`).
3. Otherwise `Board` runs (`:46-54`):
   - if the lift is up and COghe isn't on it, the empty lift is sent down (`:50`);
   - otherwise COghe walks to the deck centre, because BoardPoint is null, and the command ring appears on the panel (`:51-53`).

**Boarding and travel**

- Once every particle of that body has been on the deck for 0.2 s, `BeginTravel` sends the lift to the other end (`:67-73`). It cancels the walk and unlocks the rail (`:63`).
- **Motor:**
  - effort = clamp(weight of deck + tissue on deck (+ cargo) + 1.0·(desired − v), ±3 N);
  - desired = clamp(4·err, ±0.075 m/s) (`:75-80`).
- **Arrival:** once |err| < CatchTolerance and |v| < 0.02 m/s for 0.15 s, the motor stops, the brake engages, Trips increases by one, and the graph is rebuilt (`:81-82`).

**What the button does visually**

- `LateUpdate` lerps the Panel 4 mm down while the lift is moving and back up when it stops (`:55`).
- So the button only goes down after boarding plus 0.2 s.
- It moves 4 mm, which is about 3 px across the 720 px overview.
- Nothing actually presses it.

**Sound**

- An acknowledgement on any accepted command (`COgheAudio.cs:186-187`).
- The motor loop plays because the rail is vertical (`:227-228`).
- `mech_lift_ding` plays when Trips changes (`:270-274`).
- There is no click on press or release.

**COghe's animation**

- Nothing lift-specific.
- COghe walks to the deck centre, which is over or next to the button. The puck has no collider, so the skin can overlap it.

### 2.2 Can the lift go back down? Yes [C]

- **COghe rides it back down:** tap the panel again while riding at the top. COghe boards again, holds for 0.2 s, and `BeginTravel(0)` runs (`COghePassengerLift.cs:72`).
  - Covered by the test `ElevatorRoundTripCarriesAllTissue` (`Assets/_Game/Tests/PlayMode/COgheSpatialRecoveryTests.cs:48-55`): up gives Trips = 1, down gives Trips = 2, and all 32 particles are carried.
- **The empty lift comes down on its own:** if COghe has already left the deck at the top, a tap sends the empty lift down (`:50`).
- **Nothing on screen tells the player either option exists.** The lesson text only mentions going up ("Chạm nút trên khay để lên thang", "Tap the button on the tray to ride up", `COgheSpatialCampaignBuilder.cs:17`).

### 2.3 Proposed change

**Constraints:**

- Presentation reads real state and never gates motion on an animation (`Docs/COGHE_LEVEL_DESIGN_RULES.md:138-139`).
- Circuit identity colours stay (A blue, B coral); status lights are secondary (`Docs/LevelDesign/COghe/SpatialPilot/Level08/README.md:29`).
- Read `Docs/ArtDirection/COghe/STYLE_RULES.md` before any art change (`AGENTS.md`).

**A. Simulation state** (in `COghePassengerLift`; deterministic, no change to the motor or forces)

- Add `enum ButtonState { Ready, Armed, Pressing, Latched, Releasing }`.
- Add `NextDirection`: +1 at the bottom landing, −1 at the top.
- Add `PressPoint`: the top of the button, in world space.
- Transitions:

  | From | When | To |
  |---|---|---|
  | Ready | tap accepted | Armed (today's Boarding) |
  | Armed | all tissue on the deck for 0.2 s (existing `:72`) | Pressing, for a fixed 0.25 s of simulation time, not tied to an animation |
  | Pressing | 0.25 s elapsed; `BeginTravel` runs | Latched (= Moving). Taps are swallowed as today (`:43`) but acknowledged with a "locked" tick. |
  | Latched | arrival (`:82`) | Releasing, for 0.2 s |
  | Releasing | 0.2 s elapsed | Ready, with NextDirection flipped |

- The meaning of `Trips` doesn't change, so the existing tests still apply.

**B. Placement**

- Move the button off the spot where COghe stands. For example, put it on a short pedestal at the camera-facing front-left corner of the deck, around deck-local (−0.09, 0.035, −0.09).
- Set `BoardPoint` in the opposite half, for example (+0.04, …, +0.04).
- That way the body never covers the button, and a tentacle with about 6–8 cm of reach can press it.
- Where to change it: builder `COgheSpatialCampaignBuilder.cs:141-143`, art `COgheSpatialArtBuilder.cs:185-198`.
- Check the result at 720×1280 and 720×1612, as rule 3.6 requires.

**C. New `COgheLiftButtonPresentation`** (presentation only, runs in `LateUpdate`)

- **Cap travel:** 8–10 mm, about 40% of the cap height (at least 6–8 px in the overview), moving against a fixed bezel ring so the depression is easy to read.
- **Motion per state:**

  | State | Cap motion |
  |---|---|
  | Armed | 1–2 mm dip to acknowledge the tap, and a brighter rim |
  | Pressing | eases fully down in 0.12 s, in sync with the tentacle tip |
  | Latched | held fully down; optionally a 1 mm pulse on the motor beat |
  | Releasing | critically damped spring back up with a small overshoot (about 0.18 s) |

- **Status ring / LED** (a secondary colour; the cap stays circuit blue):
  - Ready: soft breathing white/mint, with an engraved ▲ or ▼ for NextDirection;
  - Latched: amber;
  - arrival: a brief mint flash.

**D. COghe presses the button**

- Add `virtual bool TryPress(int anchor, out Vector3 point, out float amount)` to `COgheMechanism` (`COgheMechanism.cs:5-27`).
- Query it next to `TryManipulationContact` (`VenomCampaignTapCommands.cs:19-26`).
- In `VenomLifeAnimation.CampaignPerformance` (`VenomLifeAnimation.cs:278-315`), draw one tendril that reaches out, pushes (the body leans about 3 mm) and retracts, timed by the Pressing and Releasing states.
- A real tissue pad (`COgheTissueSensor`) would not fit here. COghe stands on the deck for the whole ride, so a pad can't pop back up on arrival.

**E. Sound** (in `COgheAudio.Observe`, next to the lift loop at `:270-274`; add snapshot arrays near `:161`)

| Moment | Clip | Note |
|---|---|---|
| Pressing starts | `mech_latch` | exists; volume about 0.5 |
| Tap while Latched | `ui_tap` or `glass_tok` | quiet |
| Arrival | `mech_lift_ding` | keep as today |
| Releasing | `metal_clink` | exists; for the button popping up |

The clips are in `Resources/COgheAudio` (`Docs/Audio/COghe/README.md`).

**F. Tests:** extend `ElevatorRoundTripCarriesAllTissue` to assert:

- the state sequence Ready → Armed → Pressing → Latched → Releasing → Ready;
- `NextDirection` is −1 at the top;
- the presentation depth is at least 0.95 while Moving, and at most 0.05 within 0.3 s after Trips changes;
- `Played["mech_latch"]` increments;
- every particle stays at least 1 cm away from the button footprint.

---

## 3. Force feedback when a part is too weak

### 3.1 How strength is modelled today [C]

- **Active force = 7 m/s² × tissue mass** of the body doing the work.
  - TapRail effort clamp: `COgheTapRail.cs:232-237`.
  - Held props use the same rule: `VenomCampaign.cs:239`.
  - The 96 g whole body therefore produces 0.672 N; 50% gives 0.336 N; 25% gives 0.168 N.
  - Design source: `Docs/VENOM_CREATURE_SKILLS.md:38-48` (F = a_tissue · mᵢ).
- **Bracing is finite:** 36 m/s² × the gripping fraction (`VenomCampaignMotion.cs:44`, `:94-115`).
- **Loads come from physics.** There is no "required mass" field. A load is made of:
  - rail Resistance (dry friction);
  - gravity along the rail axis;
  - latches (`COgheRailSlider.cs:43-58`);
  - linked loads such as pulley tension (`COghePulleyDrive.cs:23-24`).

  Examples:
  - Boss B1, crate C1: 0.22 kg with 0.45 N of resistance, so it needs more than 67% of the body. It uses CompensateLoad (`COgheSpatialPlusLevels.cs:221-222`).
  - Unit test: one third of the body fails and two thirds lifts the load (`COgheMechanismExpansionTests.cs:40-47`).
- **Pads** use a threshold in kg (`COgheTissueSensor.cs:9-26`). Level E09 prints "50%" and "25%" labels next to its heavy and light pads (`COgheSpatialPlusLevels.cs:276-278`). That is the only weight cue anywhere on screen.
- **Rails without CompensateLoad never reach full strength.** At a stall their effort is gain × desired (for example 4.5 × 0.09 = 0.405 N), not the tissue cap. A stuck rail with light gain never pulls at full strength.

### 3.2 What happens today when a part is too weak [C]

1. **The tap is accepted.** `Request` only checks control, busy state, interlocks and the route (`COgheTapRail.cs:124-150`). There is no mass check.
2. **COghe walks over and grabs.** The phase goes from Approaching to Operating (`:209-216`) and the `creature_grab` sound plays (`COgheAudio.cs:250-251`). The tendrils and the 1.16 stretch look exactly like a successful pull (`VenomLifeAnimation.cs:282-315`).
3. **COghe pulls, but the object doesn't move at all.** Static friction exactly cancels any drive below Resistance (`COgheRailSlider.cs:57`). [M/H] In pulley levels the input carriage stretches the cable a few millimetres before it stalls.
4. **After a stall timeout, COghe lets go.** If the rail hasn't made 2 mm of progress within StallSeconds, the task ends:
   - StallSeconds is 3 s by default, 5 s for crates (`COgheSpatialNextBuilder.cs:236`) and 6 s for pulleys (`COgheSpatialCampaignBuilder.cs:130`);
   - the task calls `CancelTask("Cơ quan bị kẹt hoặc phần này chưa đủ lực")` ("The mechanism is stuck or this part isn't strong enough") at `COgheTapRail.cs:229-230`.
5. **The player never sees that message in the shipping game.**
   - It is shown only by the developer / DayLab IMGUI HUD (`COgheDayLabPresentation.cs:175`, `VenomCampaign.cs:757`).
   - The shipping Product UI applies to this catalog (key `coghe.spatial.pilot`, `Product/COgheProductCatalog.cs:9`, `:27-33`). Under the Product UI the DayLab HUD returns early (`COgheDayLabPresentation.cs:111`).
   - Nothing in `Product/` reads `Activity` or `LastFailure`. `COgheProductUI.Notify` exists but isn't wired to this (`COgheProductUI.cs:237-244`).
   - **What the player actually sees:** COghe holds the handle motionless for 3–6 s, lets go, and makes a "hm" sound (`COgheAudio.cs:253-254`). A jammed mechanism and a too-heavy one look and sound the same.
6. **Loose props have no stall logic.** The body keeps pushing until 3 s pass without a new command, then lets go (`VenomCampaign.cs:212`).

### 3.3 Proposed design

**Principles**

- Don't refuse the tap. The skills contract says to try, then report ("thiếu lực thì tì thử rồi báo", `Docs/VENOM_CREATURE_SKILLS.md:20`), and that tentacles go taut under load ("xúc tu căng theo lực", `:21`).
- Visuals read the real effort (`Docs/COGHE_LEVEL_DESIGN_RULES.md:138-139`).
- Mass stays a resource inside the level, not a permanent stat.

**A. Simulation: ramp up to full strength, then a Strain / TooHeavy outcome** (in `COgheTapRail.StepMechanism`)

1. When the rail is stuck (|v| < 4 mm/s with remaining > CatchTolerance), ramp the effort up to the full tissue cap over about 0.6 s. This generalises CompensateLoad, so that "fail" really means "full strength wasn't enough".
2. Define `Strain = AppliedEffort / (7·mass)`, smoothed.
3. Treat the rail as overloaded while the effort is at the cap and the rail is stuck.
4. After about 1.5 s overloaded (a new `StrainSeconds`, shorter than StallSeconds), end the task with `FailureKind.TooHeavy`.
5. Keep the existing stall path for `FailureKind.Jammed` (interlock or obstacle).
6. Expose `Strain`, `FailureKind` and `RequiredShare`.

**How to set `RequiredShare`**

- Author it on each heavy rail in the builder (for example `LoadShare = .5f`), following E09's printed labels.
- Add a test that checks it against physics: a group just below that share fails, and a group at that share succeeds.
- Optional runtime estimate: (Resistance + |gravity·axis| + opposing external effort) / (7·M_total), rounded to 25, 50, 75 or 100%.

**B. A few millimetres of give, then spring back**

- **Option 1 (recommended first; presentation only, no physics risk):**
  - offset only the handle grip's visual by 2–4 mm along the pull axis, in proportion to Strain;
  - add an 8–12 Hz tremble;
  - spring it back (critically damped, 0.15 s) when COghe gives up.
- **Option 2 (physical):** a `Give` compliance in `COgheRailSlider` (`:43-58`).
  - While static friction holds, a stiff spring lets the body deflect up to 3 mm in proportion to drive / breakaway force, and returns it when the drive drops.
  - Either keep the deflection under the 2 mm progress threshold (`COgheTapRail.cs:219`), or base too-heavy detection on saturation instead of distance.
- **Pulleys** may already give and return through the cable stretch [H, verify].

**C. COghe's strain animation**

Drive it from `Strain`, exposed through an extended `TryManipulationContact` (`VenomCampaignTapCommands.cs:19-26`). Implement it in `VenomLifeAnimation.CampaignPerformance` (`:278-315`).

- **Tentacles go taut:**
  - bend goes to 0;
  - thickness goes from 1 to 0.65 (the `Tube` thickness parameter, `VenomLifeAnimation.cs:655`);
  - add a third tendril.
- **Body:**
  - stretch goes from 1.16 up to 1.3 along the pull axis;
  - leans back up to 6 mm (the existing clamp at `:301`);
  - flattens a little;
  - feet stay planted.
- **Rhythm:**
  - 2–3 "heaves" about 0.5 s apart, in phase with the handle's give;
  - an 8–12 Hz tremor in proportion to Strain.
- **Giving up:**
  - tendrils snap back over 0.2 s;
  - a small slump and head shake (a `COghePersonality` act such as "hmph").

**D. "Too heavy" marker** (presentation only; either lines created once in the style of `COgheControlFeedback`, `Docs/COGHE_CONTROL_FEEDBACK.md:21-25`, or a printed label like E09's)

- An ivory badge on the handle or object showing:
  - a weight glyph;
  - the needed share (for example "50%");
  - a 2-segment meter, with the current part's share filled against the needed share.
- It fades in when Strain > 0.5 and stays for 3 s after COghe gives up.
- Design option: also show it whenever a split part is selected near that object.
- Product UI: map `FailureKind` to an English toast through `COgheProductUI.Notify`, for example "Too heavy for this part — needs half of COghe. Merge and try again." (the Product UI is in English; see `COgheProductUI.View.cs:211`).

**E. Sound** (in the `COgheAudio.Observe` tasks loop, `:248-256`)

| Moment | Sound | Note |
|---|---|---|
| Strain starts, and each heave | `creature_grumble` | exists |
| Object under load | a low creak loop scaled by Strain | new clip, or `block_slide_loop` at low pitch with tremolo |
| Give-up | `creature_hmph`, then `metal_clink` as the handle springs back | both exist |
| Jammed | `creature_hm` | keep this for Jammed only |

**F. Loose props:** compute saturation at `VenomCampaign.cs:239`, show the same strain feedback, and give up after `StrainSeconds` instead of relying on the 3 s idle release at `:212`.

**G. Tests** (following the `COgheSpatialPlusRecoveryTests` pattern, e.g. `SpatialPlusE09QuarterTooLightForHeavyPad`, `:139`)

- **Half body on B1's crate C1:**
  - Strain > 0.5 within 1 s;
  - the handle's visual offset stays at or under 4 mm, and the Rigidbody moves less than CatchTolerance;
  - the task ends as TooHeavy within StrainSeconds + 0.5 s;
  - the handle returns to within 0.5 mm of rest;
  - `Played["creature_hmph"]` increments.
- **Whole body:** still succeeds, unchanged.
- **Interlocked rail:** still reports Jammed.

### 3.4 Hook point index

| Need | File:line |
|---|---|
| Effort, gain and clamp; compute Strain and saturation; ramp to the cap | `Assets/_Game/Venom/Runtime/COgheTapRail.cs:231-245` |
| Stall cancel, to split into TooHeavy vs Jammed | `COgheTapRail.cs:229-230` |
| Tap acceptance (keep accepting) | `COgheTapRail.cs:124-150` |
| Status text and Activity | `COgheTapRail.cs:64-65` |
| Optional physical "give" compliance | `COgheRailSlider.cs:43-60` |
| Held-prop force clamp and idle release | `VenomCampaign.cs:239`, `:212` |
| Contact query used by animation (add strain / press) | `VenomCampaignTapCommands.cs:19-26` |
| Tentacles, stretch and tube thickness | `VenomLifeAnimation.cs:278-315`, `:655` |
| Mechanism API for a press hook | `COgheMechanism.cs:5-27` |
| Lift state, timing and panel visual | `COghePassengerLift.cs:42-55`, `:63-82` |
| Lift layout (button, BoardPoint) | `Assets/_Game/Editor/COgheSpatialCampaignBuilder.cs:137-146`; `COgheSpatialArtBuilder.cs:185-198` |
| Sound events | `Audio/COgheAudio.cs:161`, `:248-256`, `:270-274` |
| Player-facing message in the shipping UI | `Product/COgheProductUI.cs:237-244` (Notify) |
| Precedent for printed share labels | `COgheSpatialPlusLevels.cs:276-278` |

---

## Appendix A: the offline model [M]

**State:** x and v of one carriage, moving along one axis.

**What runs each tick, in the scene's order:**

1. **Slider:**
   - re-arms the end latches inside 3 mm;
   - releases a latch when Effort > R + 4 mN (or below −(R + 4 mN) at the far end);
   - when latched, the joint is locked at the current x;
   - otherwise F = E − friction − 0.08·v, where friction = clamp(E, ±R) if |v| < 2 mm/s, else sign(v)·R;
   - then the pending effort is cleared.
2. **TapRail:**
   - d = clamp(3·(0.16 − x), ±0.09);
   - effort = clamp((d − v)·max(m, 0.18)·25 (+ integral), ±cap), or 0 once within 3 mm of the end;
   - the effort is added to pending.
3. **Integration:** v += F/m·dt, x += v·dt, with dt = 1/120 s and the joint limits applied.

**Parameters:** m = 0.035 kg, R = 0.010 N, cap = 0.672 N (whole body), from `COgheSpatial04.unity` and `Matter.asset` (ParticleMass 0.003).

**Displayed frames:** sampled every 2 ticks for 60 fps, and in a 3, 3, 2 tick pattern for 45 fps.

**Not modelled:**

- PhysX contacts and joint compliance;
- creature coupling, other than the reaction-force arithmetic;
- the cable model, except in a separate 2-body pulley run.

The Python ran inline from the shell and was not saved. The equations above are enough to reproduce it.

## Appendix B: open items

- **Not run in Unity or on a device.** All magnitudes need the traces in §1.3.
- **Unverified:** whether, in Unity 6000.3, the transform of an interpolated Rigidbody equals its physics pose inside FixedUpdate. This decides how fix #2 is implemented.
- **No Android data for Spatial levels.** They have only been measured on a Mac M5 (`mac-native-run.json`).
- **Component order relies on observed behaviour.** `GetComponentsInChildren` returning components in component-list order is observed Unity behaviour, not documented. Fix #1B makes the order explicit so it no longer depends on this.
