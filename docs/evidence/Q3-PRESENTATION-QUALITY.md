# Q3 presentation quality closure — partial evidence

Date: 2026-10-04 KST
Baseline: origin/main 514f341f28115b776ddb42530e63659ae49a9446 (EV-014)
Milestone: Q3 VERTICAL SLICE IN_PROGRESS
Verdict: PARTIAL. Small-screen readability and authored review presentation are implemented, but the final fresh PlayMode/full regression/build/native-input pass is BLOCKED by the current Unity license state.

## Routing

Selected DEV: DEV-02-05/06/07/08/13, DEV-04-09/10 plus the Q3 final-quality exit.
Existing AWU-1~5, FP-29 and EV-014 representative art wiring were consumed as input and were not reimplemented.
Guard remains one archetype repeated in five Formation slots. Five slots are not five monster species. Three regular + one boss remains an unapproved proposal.

## Actual Work Units

### Q3-P1 — short-portrait readability

Input:
- EV-014 runtime composition and Figma frames 18:72 / 18:78 / 18:84.
- Prior 360x640 evidence showed RaisedGuardSlot0 at about 19.57 x 29.36 physical px.

Work:
- Runtime ally presentation now applies a width-aware readability floor while retaining the protected-combat constraint.
- At 360x640 the intended raised-ally box is about 30.24 x 45.36 px.
- Figma canonical added frame 31:158: Q3 / 360x640 · full formation readability · review candidate.
- Full-Formation capacity treatment remains the same RaiseEligible capacity guard, not a fifteenth semantic state.

Output:
- Assets/Necrom/FirstPlayable/Runtime/FirstPlayableVisualPresentation.cs
- Figma 31:158 and review note 31:174.

Verification:
- Existing current-runtime targeted run AWU4-Q3-presentation-targeted.xml: 7/7 PASS, including ShortPortraitRaisedAllyHasReadableFloorAndRemainsProtected.
- The runtime/profile files tested by that XML are byte-identical to the current main working tree.
- Figma 31:158 screenshot was visually inspected: five allies are readable, do not overlap, stay inside the battle band, and the HUD/capacity CTA has no clipping.
- Current final fresh re-run after the later AudioListener scene addition: BLOCKED by Unity licensing, see Q3-P3.

Verdict: PASS for the bounded readability implementation; final current-head regression is not yet closed.

### Q3-P2 — authored review motion and audio

Input:
- EV-014 procedural lunge/tint/collapse/pulse event hooks.
- Actual production damage, defeat, Raise, roster and exact-contributor events.

Work:
- Added FirstPlayablePresentationProfile with five authored AnimationCurve timing contracts:
  attack 180 ms, hit 120 ms, defeat 340 ms, Raise 420 ms, allied contribution 240 ms.
- Generated five deterministic PCM review SFX with scripts/generate-q3-review-sfx.ps1.
- No third-party samples are used.
- Added AudioSource runtime playback and one canonical scene AudioListener.
- Actual event hooks remain the trigger; no test-state adapter was introduced.

Audio artifact readback:
- 5/5 WAV files parse as RIFF/WAVE PCM mono 22050 Hz 16-bit.
- Durations: attack 0.14 s, hit 0.10 s, defeat 0.30 s, Raise 0.36 s, ally contribution 0.12 s.
- Production sound design / mix / loudness / physical-device playback remain NOT RUN.

Unity readback:
- Artifacts/AWU4-Q3-presentation-apply9.log exited 0 after the listener addition.
- Canonical scene/prefab reopen passed with art 4/4, authored motion 5/5, audio clips 5/5.
- No compiler-error signature exists in the apply log.

Targeted evidence:
- AWU4-Q3-presentation-targeted.xml: 7/7 PASS before the final AudioListener assertion was added.
- It verifies actual damage attack/hit/defeat cue requests, Raise art/cue, exact allied contribution, authored profile/audio references, short portrait readability, copy fit and lifecycle re-entry.
- The later listener addition compiled/imported/reopened successfully but has not received a new PlayMode execution because Unity licensing is currently unavailable.

Verdict: IMPLEMENTED / PARTIAL VALIDATION. Audible normal-path playback on the current final scene still needs one fresh licensed run.

### Q3-P3 — final regression/build/input/visual closure

Attempt:
- Fresh targeted command executed with process-local HOME=USERPROFILE, TMP=TEMP, PROGRAMDATA=C:\ProgramData.
- -runTests was used without -quit.

Result:
- Unity terminated before test execution with exit code 198.
- Log: C:\Dev\Necrom-playmode-recovery\Artifacts\AWU4-Q3-motion-audio-targeted-fresh.log
- Cause: no current Unity entitlement/token/ULF was available to the Licensing Client.
- No XML was produced. This is BLOCKED, not FAIL and not PASS.

Consequences:
- Current-head full PlayMode is NOT RUN.
- Current-head Windows rebuild/launch/native-input/audio playback is NOT RUN.
- EV-014 129/129 and old build/input evidence are historical only and are not reused as new results.

## Figma readback

Canonical file: eXqKU1qHXsn52SJfIGltZo
- HUD semantic board: 11:89 (Target3 / Raise7 / Army4 = 14 semantic states)
- Q3 battle: 18:72
- Q3 raised: 18:78
- Q3 full Formation: 18:84
- Capacity component: 19:90
- 360x640 full-Formation readability: 31:158
- Motion/audio review note: 31:174

State evidence boundary remains:
- 9 canonical captures: TargetActive/Defeated; RaiseTargetNotReady/Eligible/CommittedAwaitingProof/ProofObserved; ArmyEmpty/ProofPending/ProofObserved.
- 5 component-boundary states: TargetNone; RaiseNoTarget/SourceUnavailableOrConsumed/InsufficientSoul; ArmyOwned.
- Full capacity is not a fifteenth semantic state.

## Evidence boundaries still open

UNKNOWN / NOT RUN:
- final commercial visual direction
- final product font/copy/icon and full asset-rights review
- production sound design and mix
- physical mobile device install / actual SafeArea
- actual 1080x2400 pixel run
- accessibility acceptance
- real-user fun / retention
- approved launch monster count

## Failure return

Re-activate/sign into Unity Hub so Unity 6000.3.25f1 has a valid editor entitlement.
Then sync the current main working-tree source to C:\Dev\Necrom-playmode-recovery and run:
1. FirstPlayableQ3VisualTests targeted suite.
2. Entire relevant PlayMode suite.
3. Windows development build.
4. Normal native-input playback and responsive capture, including current audio/motion counters and 360x640 art geometry.
Only after those pass may this presentation slice be promoted from PARTIAL to validated current-head evidence.
