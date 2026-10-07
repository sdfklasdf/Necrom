# EV-024 — Visible Gate Pressure + Normal-Play Failure Path

Status: PASS for the bounded Windows normal-play gate-pressure scope. Q3 overall remains PARTIAL / IN_PROGRESS.

## Scope

This work closes the direct EV-023 gameplay gap: gate failure is no longer only a test/orchestration hook.

The canonical hybrid still preserves:
- Necromancer automatic combat;
- Raise;
- five-slot Formation;
- actual Raised ally contribution;
- Obsidian Soul production-candidate art / UI / motion / audio;
- one Guard archetype repeated as threats / Raised Guards.

The pressure values below are representative vertical-slice inputs, not final balance:
- wave threats: 2;
- gate integrity: 10;
- threat travel time: 4.8 seconds;
- normal breach damage: 10.

Tower placement, multi-lane, final economy, offline-progression numbers, launch monster/content count remain UNDECIDED.

## AWU-1 — Normal runtime gate pressure

Added:
- Assets/Necrom/FirstPlayable/Runtime/FirstPlayableGatePressureController.cs

The controller:
- owns only time-to-objective pressure for registered active threats;
- advances from normal Update;
- rejects invalid/stale/non-active threats;
- calls FirstPlayableDefenseWaveRuntimeController.RecordGateBreach without test-only state injection;
- stops once defense leaves Running;
- exposes read-only progress for visual/runtime evidence.

Canonical composition now:
- registers both wave threats with gate pressure;
- resets pressure per wave;
- keeps pressure enabled/disabled with automatic combat;
- manual AdvanceCombat uses the same pressure system for deterministic replay/debugging.

Race-safety boundary:
- defeated threats are removed from active defense truth and are ignored by pressure;
- escaped threats are removed by EnemySpawnController;
- a threat cannot both resolve as defeated and later breach the gate through the pressure controller.

## Visible pressure

FirstPlayableVisualPresentation now:
- renders a bounded defended-objective marker inside the protected combat zone;
- visually moves the current active Guard from its starting position toward that objective according to actual gate-pressure progress;
- preserves the existing defeat animation before switching visual focus to the next active threat;
- hides stale defeated interaction art after a Failed defense result.

The gate marker is functional vertical-slice evidence, not final commercial gate artwork.

Final normal-failure capture:
Artifacts/VisualPlayer/VisualEvidence/20261007T091526791-32bac2bd99f44877948fcd3c224181b5

Advanced pressure frame:
- gatePressureProgress=0.745
- target=enemy:canonical:2
- Guard left edge x=186.38
- defended marker is at approximately x=146
- Defense phase still Running
- gate 10/10
- active threat count 1

This is actual Windows player motion state, not a test-state adapter.

## Normal no-input failure

Development player was launched with automatic gameplay and no Raise click/state injection.

Observed:
- first threat is defeated by automatic combat;
- second threat continues advancing;
- no Raise input is supplied;
- second threat reaches the defended objective;
- RecordGateBreach executes from normal runtime pressure;
- gate 10 -> 0;
- Defense Running -> Failed;
- Battle resolves as failure;
- visible defense HUD becomes FAILED / 묘지가 무너졌습니다 / 묘지 0 / 10.

Final failure semantic cleanup:
- Target becomes TargetNone;
- Raise becomes RaiseNoTarget;
- stale defeated target art is hidden;
- failed state no longer presents an actionable Raise CTA.

Player exit: 0.

## Timely Raise success path remains valid

Current-head native input run:
Artifacts/VisualPlayer/VisualEvidence/20261007T091556848-a8f06f28191f4c028cbf62701f280c1a

External driver result:
- NATIVE_INPUT_PASS=1 real OS click
- PLAYER_EXIT=0
- first defeat exposes Raise while Defense remains Running
- actual OS Raise click activates one Raised Guard
- exact Raised ally contribution is observed (lastContributionUnitId=ally:10)
- wave reaches Cleared
- gate remains 10/10

Therefore the current representative loop has a real decision window:
- ignore Raise pressure -> gate failure;
- timely Raise -> allied contribution helps clear before breach.

This is a vertical-slice timing proof, not final balance approval.

## Fresh regression

Current-head final targeted canonical:
Artifacts/EV024-final-canonical.xml
- 8/8 PASS
- failed 0
- skipped 0

Current-head final full PlayMode:
Artifacts/EV024-final-full-playmode.xml
- 138/138 PASS
- failed 0
- skipped 0

The new canonical normal-pressure test verifies:
- active progress exceeds 70%;
- normal gameplay can fail without ResolveFirstThreatAtGate;
- gate reaches 0;
- Battle resolves;
- failed HUD is non-actionable.

## Fresh Windows build

Final build:
- CANONICAL_REOPEN_PASS components=22
- VISUAL_PLAYER_BUILD Succeeded errors=0
- buildGUID bbfd3abe92724b978d5d4b20c3617c02

Artifacts/VisualPlayer/NecromVisual.exe:
- size 667136 bytes
- SHA256 B9FC30BB05CA93A042831974149A681D656963E3E612ECAFD9AE826759D739AD

The launcher executable hash can remain stable while Unity data changes; buildGUID identifies this build execution.

## UI / art boundary

The current lower Target / Raise / Army cards are NOT declared final commercial UI.

They are retained here because they are:
- functionally wired;
- semantically validated;
- responsive enough for Q3 mechanics evidence;
- useful for input/state verification.

They still require a dedicated commercial-polish pass covering:
- information hierarchy;
- card density;
- button prominence;
- spacing and rhythm;
- iconography;
- combat-first visual focus;
- final state composition.

Likewise, the current defended-objective marker is a functional pressure visualization, not final environment/gate art.

Do not use EV-024 as evidence that the bottom HUD or gate art is final-quality.

## Remaining Q3 boundaries

Still NOT PASS / NOT RUN:
- final lower-HUD commercial polish / Founder visual acceptance;
- final defended-objective/gate art treatment;
- GameStateSnapshot wave/gate persistence;
- final rights/legal/font release acceptance;
- representative physical mobile SafeArea / speaker / earphone / accessibility;
- real-user fun / retention;
- launch monster/content quantity.

## Next direct consumer

BOTTOM HUD COMMERCIAL POLISH + DEFENSE OBJECTIVE VISUAL REFINEMENT.

Keep mechanics fixed while refining Figma/runtime visual hierarchy. Do not reopen wave logic, Raise truth, Formation truth, or final balance unless evidence exposes a real defect.
