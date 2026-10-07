# Q3 Idle RPG + Light Defense Hybrid Restructure

Date: 2026-10-06 KST
Founder decision: RESTRUCTURE the game as an idle RPG + simple defense hybrid.
Status: APPROVED DIRECTION / IMPLEMENTATION PARTIAL.

## Product contract

The player is still a Necromancer building a raised army, but the playable pressure changes from a sequence of isolated duels to continuous defensive waves.

Minimum representative loop:
1. enemies enter a defense wave;
2. Necromancer and the current Raised formation auto-attack;
3. enemies threaten a cemetery/gate integrity objective;
4. defeated enemies can produce the existing Raise opportunity;
5. the player chooses Raise when eligible and fills the existing five-slot Formation;
6. Raised allies contribute automatically;
7. the wave clears only after all queued/active threats are resolved;
8. surviving the wave leads to the next defense wave.

This keeps idle-RPG readability and automation while adding a simple active pressure system.

## Scope limits for the first hybrid slice

Included:
- on-screen automatic combat;
- multiple enemies per wave as domain truth;
- one defended gate/integrity objective;
- wave clear/fail state;
- existing Raise + five-slot Formation;
- existing exact allied contribution feedback;
- one simple target-priority path in later runtime integration.

Not yet approved/implemented:
- tower placement;
- multi-lane pathfinding;
- complex maze building;
- offline earnings/progression numbers;
- final economy/balance values;
- launch monster/species count;
- boss systems;
- final wave/content quantity.

No balance constants are fixed by the new domain seam. Gate integrity, wave number, enemy count and damage are supplied by callers/tests.

## Existing work that remains reusable

Keep without re-implementing:
- Obsidian Soul art candidate set;
- Noto Regular/Medium/Bold runtime package;
- Combat/Soul/Army semantic icon language;
- "援곕떒 理쒕?" full-formation copy;
- Attack / Hit / Defeat / Raise / exact allied-contribution motion candidates;
- five production SFX and their provenance;
- Formation capacity and Raised ally ownership;
- Raise source/consumption logic;
- Necromancer and Raised ally automatic behavior concepts;
- damage/death pipeline;
- HUD lifecycle/presentation infrastructure;
- responsive and protected-art rules.

Guard remains one archetype. The five Formation slots still represent five instances of the same Raised Guard in the current evidence. This does not decide launch species count.

## Current architecture impact

Reusable:
- Formation
- RaiseService / RaiseSource
- RaisedAllyRuntimeEntity / roster
- basic auto-behavior specs
- damage/death events
- production presentation assets

Must change in later runtime work:
- EnemySpawnController currently owns only CurrentTarget and rejects a second active enemy;
- Targeting currently assumes a single current target;
- FirstPlayableAutoCombatLoop resolves the whole Battle on the first enemy defeat;
- FirstPlayableAlliedAutoCombatLoop also resolves the whole Battle on the first enemy defeat;
- BattleStateMachine models one duel lifecycle rather than a wave;
- GameStateSnapshot currently has no gate/wave state;
- HUD has no wave/gate integrity presentation.

Therefore the hybrid must be integrated as a new encounter/wave orchestration layer rather than by pretending the old single-target battle is already a defense game.

## EV-021 domain foundation implemented

New:
Assets/Necrom/Core/Domain/IdleDefenseEncounter.cs

It owns:
- DefenseWavePhase: Ready / Running / Cleared / Failed;
- GateMaxIntegrity / GateIntegrity;
- sequential WaveNumber;
- RemainingEnemiesToSpawn;
- ActiveEnemyCount;
- revision-checked mutations;
- EnemySpawned;
- EnemyDefeated;
- EnemyReachedGate;
- PrepareNextWave.

Properties:
- wave does not clear while queued or active enemies remain;
- a gate breach consumes integrity;
- zero integrity fails the wave;
- clearing preserves remaining gate integrity for the next wave;
- stale revisions and invalid transitions reject without mutation.

Tests authored:
Assets/Necrom/Tests/EditMode/IdleDefenseEncounterTests.cs

Unity package-backed test execution is currently blocked by a local UPM IPC startup failure, so those NUnit tests are NOT RUN in Unity yet.

Independent package-free execution:
Artifacts/EV021-domain-harness.txt
- EV021_DOMAIN_HARNESS_PASS
- verifies wave clear, gate damage/failure, sequential next wave and revision behavior.

## Q3 impact

EV-018, EV-019 and EV-020 remain PASS for their bounded production art/UI/motion/audio scopes.

However, the product direction changed materially. The historical EV-020 desktop build represents the old single-duel flow, so it must not be used as final evidence that the new Q3 gameplay slice is representative.

Q3 overall remains PARTIAL.

Before Q3 can close, the next runtime integration should minimally demonstrate:
- wave controller connected to the new domain;
- multiple active/queued enemy runtime representation;
- target selection among active threats;
- first enemy defeat does not end the wave;
- Raise remains correct inside the wave;
- Raised allies keep contributing;
- gate damage/failure is visible;
- wave clear transition is visible;
- existing Obsidian Soul production presentation remains readable;
- targeted + full regressions are fresh;
- representative physical-device acceptance is then performed against the hybrid slice.

## Direct DEV routing

Primary:
- DEV-04-01 core domain/function ??EV-021 foundation started
- DEV-04-02 core playable/workflow ??NEXT
- DEV-04-04 state transition ??NEXT
- DEV-04-12 milestone integration regression ??after runtime wiring
- DEV-06-03 E2E/PlayMode core flow ??after runtime wiring

UI/fidelity consumers:
- DEV-02-02 information/screen/HUD scope ??wave/gate additions
- DEV-02-03 state matrix ??Ready/Running/Cleared/Failed + wave/gate states
- DEV-02-08 component v0 ??wave/gate HUD component
- DEV-02-13 Figma?뭝mplementation mapping
- DEV-04-03 runtime HUD
- DEV-04-09/10 Figma/runtime + responsive
- DEV-06-08/09/10 physical device/fidelity/accessibility later

## Next implementation boundary

Do not jump directly to full content production.

The next independently reviewable work should be:
HYBRID RUNTIME VERTICAL SEAM ??wire IdleDefenseEncounter into a minimal wave runtime while preserving existing Formation/Raise/auto-combat ownership.

Done condition:
- at least two threats can belong to one wave;
- defeating the first threat does not resolve the wave;
- remaining threat can pressure the gate;
- wave clear/fail truth comes from IdleDefenseEncounter;
- existing Raise and Raised ally contribution still work;
- fresh valid Unity tests are required before PASS.


## EV-022 runtime-seam update — supersedes the EV-021 pending-runtime notes above

The bounded hybrid runtime seam is now implemented and validly executed in Unity.

Completed:
- Unity UPM/Test Runner path recovered with process-local HOME/TMP/PROGRAMDATA values and the repository's no-`-quit` Test Runner contract;
- IdleDefenseEncounter EditMode tests 4/4 PASS;
- EnemySpawnController supports multiple active threats while preserving CurrentTarget compatibility for Raise/legacy encounter behavior;
- FirstPlayableTargetingController selects an actual active tracked threat;
- FirstPlayableDefenseWaveRuntimeController bridges wave/gate domain truth to Unity threat objects;
- player and Raised-ally auto-combat optionally route lethal outcomes through defense-wave truth instead of resolving Battle on the first kill;
- first enemy defeat can coexist with a second active combat target while the defeated first enemy remains the Raise candidate;
- existing Raise -> Formation -> actual Raised ally contribution is executed inside one defense wave;
- gate breach Running/Failed truth is executed;
- final hybrid targeted PlayMode 2/2 PASS;
- final full PlayMode 136/136 PASS;
- Windows canonical build compiles/reopens with errors=0;
- Figma hybrid HUD semantic contract exists at node 46:309 with RUNNING / CLEARED / FAILED and wave/gate/threat information.

Still not complete:
- FirstPlayableGameplayComposition and saved canonical scene/prefab do not yet use the defense-wave bridge as their default gameplay loop;
- the new wave/gate HUD state projection is not yet a visible runtime overlay;
- the Windows player build is therefore compile-compatibility evidence, not representative hybrid-play evidence;
- GameStateSnapshot still does not persist wave/gate state;
- physical-device and real-user evidence remain NOT RUN.

### Updated direct DEV routing

Completed for the bounded seam:
- DEV-04-01 core domain/function
- DEV-04-02 hybrid workflow foundation
- DEV-04-04 hybrid state transition foundation
- DEV-06-03 Unity E2E/PlayMode seam regression

Next direct consumer:
- DEV-02-13 Figma -> runtime mapping for node 46:309
- DEV-04-03 visible runtime HUD integration
- DEV-04-12 canonical milestone integration regression
- DEV-04-09/10 Figma/runtime + responsive comparison
- then DEV-06-08/09/10 representative device/fidelity/accessibility.

### Updated next implementation boundary

CANONICAL HYBRID COMPOSITION + VISIBLE WAVE/GATE HUD.

Done condition:
- the saved canonical composition actually starts/owns a defense wave;
- the default playable scene demonstrates more than one threat within a wave;
- the first defeat does not end the wave;
- Raise and Raised-ally contribution remain normal gameplay paths;
- gate integrity and wave state are visibly rendered from FirstPlayableDefenseWaveHudState;
- runtime mapping is directly compared with Figma node 46:309;
- fresh targeted/full regressions pass;
- a fresh Windows player executes and captures the hybrid loop through normal input;
- no claim of physical-mobile or real-user acceptance is made before those tests actually occur.


## EV-023 canonical hybrid composition update

EV-023 converts the tested EV-022 seam into the saved canonical/default player path.

Completed:
- FirstPlayableGameplayComposition now owns FirstPlayableDefenseWaveRuntimeController;
- saved FirstPlayable scene/prefab contain the defense runtime and defense HUD components;
- the representative wave starts with two Guard threats without deciding launch content quantity;
- first defeat keeps Battle/Defense Running and preserves the defeated Raise candidate while targeting the remaining active Guard;
- Raise -> Formation -> actual Raised ally contribution now occurs inside the same canonical wave;
- final threat defeat clears the wave;
- sequential StartNextEncounter starts the next wave;
- canonical gate orchestration verifies 10 -> 6 Running -> 0 Failed/Defeat;
- visible FirstPlayableDefenseWaveHudRuntimeBinding maps Figma 46:309 semantics into the existing runtime overlay;
- RUNNING / CLEARED / FAILED, wave, gate integrity/max, active/remaining threat and gate bar are runtime-visible;
- final canonical targeted 7/7, integrated 4/4, production visual 10/10, full PlayMode 137/137 PASS;
- fresh Windows build PASS / errors0;
- representative Windows captures at 390x844 / 768x1024 / 360x640 keep the new HUD above protected combat art and preserve production readability;
- normal Windows OS-input evidence executes automatic first defeat -> real Raise click -> actual Raised ally contribution -> Cleared.

Evidence boundary:
- responsive SafeArea is simulated, not physical-device evidence;
- gate failure is currently canonical orchestration/test evidence, not an autonomous ordinary-player gate-pressure loop;
- GameStateSnapshot wave/gate persistence is still absent;
- Founder creative, legal/font release, physical mobile, accessibility and real-user evidence remain NOT RUN;
- two threats and gate integrity 10 are representative slice inputs only, not launch/final balance decisions.

Updated direct DEV:
- DEV-02-13 bounded Figma -> runtime mapping: PASS for node 46:309 semantic contract
- DEV-04-03 visible runtime defense HUD: PASS bounded
- DEV-04-12 canonical integration regression: PASS bounded
- DEV-04-09/10 responsive Windows runtime mapping: PASS bounded
- DEV-06-03 canonical hybrid PlayMode regression: PASS bounded
- next: normal-play gate pressure/failure path, then human/device gates.

Next implementation boundary:
VISIBLE GATE PRESSURE + NORMAL-PLAY FAILURE PATH.

Do not add tower placement, multi-lane, final economy or launch content expansion in that work.


## EV-024 superseding runtime update — normal gate pressure

Canonical normal gameplay now contains visible time-to-objective pressure:
- two representative Guard threats are pressure-registered;
- active Guard art advances toward a single defended-objective marker;
- no-input normal Update can reach gate breach and Failed truth without a test-only hook;
- timely Raise preserves the success path and exact Raised ally contribution;
- final current-head canonical targeted 8/8 and full PlayMode 138/138 PASS;
- Windows failure capture and native OS-click success capture both exist.

Evidence: docs/evidence/Q3-GATE-PRESSURE-NORMAL-FAILURE.md

Important visual boundary:
- the lower Target / Raise / Army HUD is still a functional production candidate, not final commercial polish;
- the defended-objective marker is also a functional pressure proof, not final gate/environment art;
- next visual work should refine those surfaces without reopening validated hybrid mechanics.
