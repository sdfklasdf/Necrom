# Q3 presentation quality closure — EV-016

Date: 2026-10-05 KST
Baseline: origin/main 1415f195808e2ca5ea4bd065204730d5ddf44233 (EV-015 implementation checkpoint)
Milestone: Q3 VERTICAL SLICE IN_PROGRESS
Verdict: PASS for the bounded current-head desktop review validation. Q3 overall remains PARTIAL / IN_PROGRESS because final commercial direction, production typography/copy/rights, production sound, physical mobile device, accessibility and real-user fun/retention are still open.

## Routing

Selected DEV: DEV-02-05/06/07/08/13, DEV-04-09/10 and Q3 final-quality exit.
Existing AWU-1~5, FP-29, EV-014 representative-art wiring and EV-015 authored review presentation were inputs and were not reimplemented.
Guard remains one archetype repeated in five Formation slots. Five slots are not five monster species. Three regular + one boss remains an unapproved proposal.

## Actual Work Units

### EV016-A — current-head targeted + full regression

Input:
- main/origin/local 1415f195808e2ca5ea4bd065204730d5ddf44233.
- Unity 6000.3.25f1.
- valid Unity Personal entitlement restored.
- validation project C:\Dev\Necrom-playmode-recovery at the same Git HEAD, clean.

Work:
- fresh FirstPlayableQ3VisualTests targeted execution.
- fresh entire relevant PlayMode execution.
- process-local HOME=USERPROFILE, TMP=TEMP, PROGRAMDATA=C:\ProgramData.
- -runTests was used without -quit.

Result:
- targeted: 7/7 PASS, failed0, skipped0, duration 11.9268454 s.
- full relevant PlayMode: 131/131 PASS, failed0, skipped0, duration 19.4142807 s.
- Previous EV-014 129/129 was not reused as new evidence.

Artifacts:
- docs/evidence/artifacts/Q3/EV015-current/targeted.xml
- docs/evidence/artifacts/Q3/EV015-current/full.xml
- docs/evidence/artifacts/Q3/EV015-current/raw-logs.zip (targeted/full raw logs preserved)

Verdict: PASS.

### EV016-B — fresh Windows development build + responsive playback

Work:
- Necrom.EditorTools.FirstPlayableCanonicalScene.BuildVisualPlayer executed from current validation project.
- Canonical scene reopened before build.
- Fresh development-player responsive run executed at 390x844, 768x1024 and 360x640.

Build result:
- CANONICAL_REOPEN_PASS components=19.
- VISUAL_PLAYER_BUILD Succeeded errors=0.
- fresh build GUID: 81e37441d4564baabe2f62bb167a86f4.

Responsive run:
- run id: 20261005T132118670-2d80deeef4ef4671a6baa62220640048.
- 3 requested ratios executed at exact actual pixel sizes.
- 4 captures per ratio: active / eligible / raised / proof.
- q3ArtLoaded=4.
- authoredMotion=True.
- audioClips=5.
- audioListenerCount=1.
- overlayCount=1 on every capture.
- q3ProtectedUnitArt=PASS on every capture.
- text overflow True count=0 on every capture.
- review SFX playback increases through actual combat/Raise progression.

360x640 raised geometry:
- RaisedGuardSlot0 = 30.24 x 45.36 px.
- unit art remained inside ProtectedCombatReadabilityZone.
- Figma 31:158 uses the same 30.24 x 45.36 px raised-unit review floor for its 360x640 full-Formation candidate.

Artifacts:
- docs/evidence/artifacts/Q3/EV015-current/raw-logs.zip (build/responsive-player raw logs preserved)
- docs/evidence/artifacts/Q3/EV015-current/responsive/

Verdict: PASS within desktop simulated-SafeArea review scope.

### EV016-C — normal-path native OS input + authored motion/audio playback

Initial attempt:
- canonical scripts/verify-first-playable-native-input.ps1 launched the current player but Windows rejected SetForegroundWindow.
- This was a desktop foreground-focus issue before the first requested gameplay click, not a gameplay failure.

Recovery:
- no production code, scene, prefab or Figma changes.
- an evidence-only temporary driver copied the canonical native-input script and added robust AppActivate + ShowWindow + BringWindowToTop + SetForegroundWindow retry.
- gameplay logic, input-request protocol, client-pixel checks, OS cursor positioning and mouse_event clicks were unchanged.
- evidence driver is preserved at docs/evidence/artifacts/Q3/EV015-current/native-input-focusfix.ps1.

Fresh native run:
- run id: 20261005T132347996-8eeee4182cd444c197380a5dfae7ea66.
- PLAYER_EXIT=0.
- 10/10 actual OS mouse clicks completed.
- 6 encounters resolved and five Raises executed through normal gameplay.
- final soulBalance=19.
- alliedCount=5.
- Raise CTA was verified non-interactable at full army before final capture.
- final visibleAllyArt=5.
- overlayCount=1.
- q3ProtectedUnitArt=PASS.
- text overflow True count=0.
- authoredMotion=True.
- audioClips=5.
- audioListenerCount=1.
- final reviewSfxPlaybackCount=51.
- playerAttackCues=7.
- hitCues=20.
- defeatCues=6.
- raiseCues=5.
- alliedContributionCues=13.
- last actual contributor=ally:35.

Actual event/audio evidence:
- native-attack-hit: reviewSfxPlaybackCount=2 and lastReviewSfx=hit-review.
- each successful Raise increments raiseCues and ends with lastReviewSfx=raise-review.
- later encounters increment alliedContributionCues from actual allied attacks.
- defeat captures end with lastReviewSfx=defeat-review.
- test-only semantic adapters were not used for this native run.

Artifacts:
- docs/evidence/artifacts/Q3/EV015-current/native-input/
- docs/evidence/artifacts/Q3/EV015-current/raw-logs.zip (native-player raw log preserved)
- docs/evidence/artifacts/Q3/EV015-current/native-input-focusfix.ps1

Verdict: PASS for current-head desktop normal-input review playback.

## Figma/runtime comparison

Canonical file: eXqKU1qHXsn52SJfIGltZo
- HUD semantic board: 11:89.
- Q3 battle: 18:72.
- Q3 raised: 18:78.
- Q3 full Formation: 18:84.
- Capacity component: 19:90.
- 360x640 full-Formation readability: 31:158.
- Motion/audio review note: 31:174.

Fresh metadata readback of 31:158:
- frame = 360x640.
- CombatViewport = 360 x 160.49.
- RaisedGuard and slots 1-4 = 30.24 x 45.36 each.
- capacity CTA remains explicit.
- Runtime 360x640 RaisedGuardSlot0 = 30.24 x 45.36, matching the candidate readability floor.
- Runtime full army was additionally proven at 390x844 with five visible slots, 42 x 63 each and CTA disabled by capacity guard.
- No claim of pixel-identical final fidelity; this is a bounded review-candidate comparison.

## State evidence boundary

14 renderer semantic states remain:
- Target3.
- Raise7.
- Army4.

9 canonical captures remain:
- TargetActive / TargetDefeated.
- RaiseTargetNotReady / RaiseEligible / RaiseCommittedAwaitingProof / RaiseProofObserved.
- ArmyEmpty / ArmyProofPending / ArmyProofObserved.

5 component-boundary states remain:
- TargetNone.
- RaiseNoTarget.
- RaiseSourceUnavailableOrConsumed.
- RaiseInsufficientSoul.
- ArmyOwned.

Full capacity is CTA treatment under existing semantics, not a fifteenth semantic state.

## Current verdict

PASS:
- current-head targeted 7/7.
- current-head full PlayMode 131/131.
- Windows development build, errors0.
- fresh responsive launch/capture at 390x844 / 768x1024 / 360x640.
- protected unit geometry and overlay1.
- no text overflow in fresh captures.
- normal gameplay native OS input 10/10.
- actual attack/hit/defeat/Raise/exact allied contribution event feedback.
- authored review motion profile and 5 review SFX are actually present and played on the normal desktop path.
- Figma 360x640 readability floor matches runtime raised-unit geometry.

Q3 overall remains PARTIAL / IN_PROGRESS.

## Evidence boundaries still open

UNKNOWN / NOT RUN:
- final commercial visual direction approval.
- final product font/copy/icon approval.
- complete asset-rights/legal review.
- production sound design, mix, loudness and final commercial audio approval.
- physical mobile device install.
- actual mobile SafeArea.
- actual 1080x2400 physical pixel run.
- device accessibility acceptance.
- real-user fun, retention and performance acceptance.
- approved launch monster count.

Desktop SafeArea evidence remains simulated 4% vertical insets and must not be called physical-device validation.

## Next priority

Move from review-candidate presentation into Q3 final-direction convergence.
Before broad content expansion, resolve final commercial visual direction / production typography-copy / asset-rights and production motion-audio direction, then validate on a representative physical mobile device.
