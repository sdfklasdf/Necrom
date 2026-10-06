# Q3 Hybrid Runtime Vertical Seam — EV-022

Date: 2026-10-06 KST
Project: Necromancer / QUALITY_GAME
Input baseline: CP-NECRO-V2-016 / EV-021 / main 11e166449051ba9c8afe3acf94fba4db4e7ddcf3
Milestone: Q3 VERTICAL SLICE IN_PROGRESS / HYBRID RESTRUCTURE PARTIAL
Verdict: PASS for UPM/test-path recovery and the bounded hybrid runtime seam. Q3 overall remains PARTIAL / IN_PROGRESS.

## Evidence boundary

EV-022 proves a Unity-executed hybrid runtime seam for:
- two simultaneous defense threats in one wave;
- active-threat targeting independent from the defeated Raise candidate;
- first enemy defeat does not resolve the whole wave;
- existing Raise and Formation ownership preserved inside the wave;
- an actually raised ally can damage the remaining threat;
- the existing exact allied-contribution receipt records that actual unit;
- gate breaches drive integrity loss and wave failure;
- IdleDefenseEncounter remains the owner of wave/gate terminal truth;
- existing legacy single-duel behavior remains regression-compatible when the defense-wave bridge is not configured.

EV-022 does not yet prove:
- canonical FirstPlayableGameplayComposition uses the hybrid wave in the shipping/review scene;
- visible runtime wave/gate HUD overlay matches Figma;
- a representative Windows player actually plays the hybrid loop;
- physical mobile/device acceptance;
- final creative, legal, rights or font-release acceptance;
- real-user fun/retention;
- launch monster/content count.

Those remain PARTIAL / NOT RUN / UNKNOWN / UNDECIDED as applicable.

## AWU-1 — Unity UPM / test execution recovery

Observed previous failure:
- Unity Package Manager IPC timed out before valid EV-021 Unity testing.
- -noUpm was invalid because TMP/UI/NUnit package references disappeared.

Root cause isolated in the connected Windows execution environment:
- process PROGRAMDATA / ALLUSERSPROFILE / HOME / TMP values were absent;
- direct UnityPackageManager server startup exited from getLocalConfigFolder with an undefined path;
- earlier recovery commands also combined -runTests with -quit even though this repository's verified Test Runner contract explicitly avoids that combination.

Recovery:
- restore process-local:
  - HOME=C:\Users\user
  - USERPROFILE=C:\Users\user
  - TEMP/TMP=C:\Users\user\AppData\Local\Temp
  - PROGRAMDATA=C:\ProgramData
  - ALLUSERSPROFILE=C:\ProgramData
- launch Unity Test Runner without -quit.
- UPM readback: IPC server started and project:list-packages returned 200.

Fresh EditMode:
Artifacts/EV022-idledefense-editmode.xml
- IdleDefenseEncounterTests
- 4 / 4 PASS
- failed 0
- skipped 0
- Unity process exit 0

AWU-1 verdict: PASS.
The prior UPM blocker is resolved for the current connected environment.

## AWU-2 — hybrid runtime vertical seam

### Enemy runtime ownership

EnemySpawnController now:
- tracks multiple active threats;
- preserves CurrentTarget as a compatibility / current interaction target;
- exposes TryGetFirstActiveTarget for combat targeting;
- removes defeated threats from active defense truth while preserving the defeated object as the Raise candidate;
- removes/deactivates gate-breaching threats;
- preserves legacy next-encounter behavior by promoting a fresh spawn to CurrentTarget when the previous current is no longer active.

This separation is intentional:
- combat target may be enemy B;
- Raise candidate may still be defeated enemy A.

### Targeting

FirstPlayableTargetingController now selects the first valid active tracked threat rather than assuming CurrentTarget is always the live combat target.

### Wave/runtime bridge

New:
Assets/Necrom/FirstPlayable/Runtime/FirstPlayableDefenseWaveRuntimeController.cs

Responsibilities:
- bridge IdleDefenseEncounter to actual Unity EnemyRuntimeEntity objects;
- register actual spawned threats;
- record actual defeated threats;
- record actual gate breaches;
- resolve the existing application Battle only when defense truth reaches Cleared or Failed;
- expose a small FirstPlayableDefenseWaveHudState projection:
  - phase
  - wave number
  - gate integrity/max
  - remaining queued threats
  - active threat count.

It does not own:
- damage math;
- target selection;
- Raise;
- Formation;
- ally ownership;
- animation/audio;
- final HUD rendering.

### Auto combat compatibility

FirstPlayableAutoCombatLoop and FirstPlayableAlliedAutoCombatLoop now support an optional defense-wave bridge.

When not configured:
- historical single-duel resolve behavior remains unchanged.

When configured:
- lethal damage records the defeated threat into defense-wave truth;
- the first kill does not automatically resolve the Battle;
- Battle resolution occurs only when IdleDefenseEncounter is Cleared/Failed.

### Existing Raise / exact ally proof preserved

FirstPlayableCombatHudProjector:
- uses the actual active threat for combat target display when one exists;
- keeps Raise availability based on CurrentTarget, allowing a defeated Raise candidate to coexist with another active threat.

The new PlayMode test executes the existing RaiseActionController, existing roster activation, existing allied auto-combat and existing HUD session contribution receipt.

## Failure/recovery history

Initial hybrid targeted:
Artifacts/EV022-hybrid-runtime-targeted.xml
- 1 / 2 PASS
- one test errored because its reflection helper only read public properties while FirstPlayableCombatHudSession.ObservedContribution is internal.
- product state was not weakened; the helper was corrected to read the existing internal evidence seam.

Second targeted:
- 2 / 2 PASS.

Initial full PlayMode after multi-threat change:
Artifacts/EV022-hybrid-runtime-full.xml
- 128 / 136 PASS
- 8 FAIL.
- cause: historical next-encounter tests expected a new spawn to become CurrentTarget when the prior target was defeated.
- compatibility rule restored without removing multi-threat support.

Final targeted:
Artifacts/EV022-hybrid-runtime-targeted3.xml
- 2 / 2 PASS
- failed 0
- skipped 0.

Final full PlayMode:
Artifacts/EV022-hybrid-runtime-full2.xml
- 136 / 136 PASS
- failed 0
- skipped 0.
- includes the historical 134-test surface plus the two hybrid runtime tests.

## Executed hybrid behaviors

Path A — defeat / Raise / exact ally / clear:
1. two active threats are registered to wave 1;
2. enemy A is lethally resolved through the actual damage/death pipeline;
3. Battle stays Running;
4. active threat count becomes 1;
5. targeting selects enemy B;
6. CurrentTarget remains defeated enemy A as the Raise candidate;
7. existing RaiseActionController raises enemy A into Formation slot 0;
8. actual Raised ally auto-combat damages enemy B;
9. exact allied contribution receipt contains that raised ally UnitId;
10. enemy B defeat closes defense truth to Cleared;
11. application Battle resolves Victory.

Path B — gate pressure / failure:
1. two active threats are registered;
2. enemy A reaches gate: integrity 10 -> 6, wave remains Running;
3. targeting continues with enemy B;
4. enemy B reaches gate: integrity 6 -> 0;
5. defense truth becomes Failed;
6. application Battle resolves Defeat.

The gate value 10 and per-test damage are test inputs only, not approved game balance.

## Figma hybrid HUD contract

Existing production canonical frames 35:234 / 35:245 / 35:256 / 36:290 / 36:305 were not modified.

New semantic contract frame:
- file eXqKU1qHXsn52SJfIGltZo
- node 46:309
- name: Q3 / Hybrid Defense HUD Contract v1
- 390 x 485
- states:
  - RUNNING
  - CLEARED
  - FAILED
- displays wave, gate integrity, threat count and a gate-integrity bar.
- running bar readback: 306 / 306.
- cleared example readback: 214.2 / 306 (70%).
- failed state has zero fill.

This is design/state-contract evidence only.
Figma frame existence is not runtime HUD implementation evidence.

## Windows build / canonical readback

Fresh build after EV-022 code:
Artifacts/EV022-hybrid-build.log

- Unity exit 0
- CANONICAL_REOPEN_PASS components=19
- VISUAL_PLAYER_BUILD Succeeded errors=0
- executable: Artifacts/VisualPlayer/NecromVisual.exe
- executable size: 667136 bytes
- executable SHA-256:
  B9FC30BB05CA93A042831974149A681D656963E3E612ECAFD9AE826759D739AD

Boundary:
The current canonical scene still runs the historical single-duel composition.
This build proves compile/build compatibility only; it is not representative hybrid gameplay evidence.

## Q3 status after EV-022

PASS:
- EV-018 production art bounded scope
- EV-019 production UI bounded scope
- EV-020 production motion/audio bounded scope
- EV-021 release-readiness evidence integrity
- EV-021 pure-domain idle-defense foundation
- EV-022 valid Unity EditMode 4/4
- EV-022 hybrid targeted PlayMode 2/2
- EV-022 full PlayMode 136/136
- EV-022 build compile/readback
- Figma hybrid wave/gate semantic contract node 46:309

PARTIAL / next direct consumer:
- canonical FirstPlayableGameplayComposition still needs the defense wave bridge wired into the actual scene;
- visible wave/gate runtime HUD must consume FirstPlayableDefenseWaveHudState;
- representative Windows runtime evidence must demonstrate the hybrid loop itself;
- responsive/fidelity evidence must then be refreshed for the changed visible HUD.

NOT RUN / UNKNOWN:
- Founder final creative audition/acceptance
- final release-rights/legal clearance
- final font release/attribution acceptance
- representative physical mobile install / actual SafeArea
- physical speaker/headphone mix
- physical accessibility acceptance
- real-user fun/play/retention

UNDECIDED:
- launch monster/content count
- offline progression/economy numbers
- later defense complexity such as tower placement or multi-lane pathfinding

## Next recommended execution

CANONICAL HYBRID COMPOSITION + VISIBLE WAVE/GATE HUD.

Wire the already tested defense-wave runtime into FirstPlayableGameplayComposition and the saved canonical scene/prefab, map FirstPlayableDefenseWaveHudState to the new Figma 46:309 semantic contract, then execute fresh targeted/full regression + Windows hybrid runtime/native-input visual evidence. Do not expand content quantity before that representative slice exists.
