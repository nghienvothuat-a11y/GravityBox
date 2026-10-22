# Clever at display 25 — 22/10/2026

Reported behavior: Clever Lv 2 did not approach either the first mechanism or the
next one after the player operated the first. Display 25 is G8, stable ID
`coghe.spatial.plus.g8`.

## Cause and correction

G8 had no entry in `SolutionSteps.json`. Clever therefore had no targets, even
with saved Intelligence = 2. The same omission affected the other five glass-road
replacements and the two post-campaign prototypes. Completed reference solves
now provide lessons for displays 8/11/25/27/59/63 and prototypes 66/67. Existing
lessons, archived IDs and player saves are preserved.

G8's lesson is **A carriage → B carriage → C carriage**. Clever Lv 2 approaches A,
waits for the player to operate it, then climbs to B. It waits for the player
again and does not guide to C at Lv 2. The existing idle rule remains 5 seconds
without input followed by a 1-second look, when COghe is not busy.

Retry also retained the previous attempt's input timestamp after resetting the
simulation clock to zero. A reproduced 17-second attempt added an unwanted
17-second delay to the next idle wait. The input timestamp is now reset after
the clock, and Clever's lesson state is reset after the mechanisms. This also
applies when returning from Home through the same reset path.

The solution recorder excludes a pad crossed incidentally while approaching a
rail. G11's corrected lesson is A → P → swing B, matching its reference solve.
No level layout, navigation forces, ability balance or presentation was changed.

## Verification

| Run | Result |
| --- | --- |
| Baseline reproduction | Both expected failures reproduced: G8 had 0 steps; Retry retained `LastInputTime = 16.9998` |
| Record missing lessons | Eight completed reference solves; recorder passed |
| Correct G11 recording | Completed reference solve; A → P → B recorded |
| Focused campaign regression | 6 passed, 0 failed |
| Product UI regression | 2 passed, 0 failed |
| Full PlayMode regression | **989 passed, 0 failed, 54 opt-in tests skipped**, 1043 discovered; 4034 seconds |

`COgheCleverRegressionTests.cs` checks all 67 catalog entries and resolves every
recorded target. G8 is exercised at 30/60 Hz frame cadence with saved Lv 2,
physical approaches to A and B, single-tap player operation, the two-step limit,
and Retry. A separate product test enables personality/reading, uses real
InputSystem pointer events, and tests the product Restart popup. The full suite
includes the new G11 ordering guard and the existing immediate-input test during
Clever reading.

The [verification manifest](verification.json) contains exact test names,
results, source hashes and raw XML hashes. Raw XML/logs are under
`Artifacts/Tests/clever-*`; a local copy of the final XML is under
`Artifacts/COgheClever20261022/`.

Reproduce the full regression:

```sh
bash Tools/run-coghe-tests.sh clever-full-regression
```

Run the focused behavior checks:

```sh
bash Tools/run-coghe-tests.sh clever-focused 'CleverBookCoversCurrentCatalogAndResolvesEveryStep|CleverRetryRestartsItsIdleClock|CleverLevelTwoGuidesLevel25|CleverLevel25WorksWithSavedCardAndProductPresentation|CleverGlassRoadLessonStartsWithTheGearBeforeThePad|CleverReadsButPlayerCommandRemainsImmediate'
```

The iPhone reconnected after regression finished. Its saved Intelligence was
confirmed as Lv 2. A fresh Release ARM64 iOS build containing all 67 scenes and
the G8 lesson was installed over the existing app. Clever Lv 2, ability cards,
owned items, Drops and all 27 completed-level entries were preserved. iOS
initially blocked launching because the phone was locked. After the user
unlocked it, launch succeeded and the COghe process was confirmed running. Local build logs and binary hashes are under
`Artifacts/COgheClever20261022/ios/`.

The 30/60 Hz cases are simulation/frame-cadence checks, not measured phone FPS.
Catalog coverage proves lesson presence and target resolution; automated A/B
behavior was tested in Unity, not replayed on the physical iPhone.
