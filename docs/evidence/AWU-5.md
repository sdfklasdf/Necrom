# AWU-5 — final integrated evidence closure

Date: 2026-10-03 UTC. Baseline main: 696b07751ebd46fbd8a6155f25e7f4b2b58b206f.
Mode QUALITY_GAME; milestone Q2 FIRST PLAYABLE.
Verdict: **AWU-5 PASS; A2-2B1 PASS within the Q2 review-runtime boundary. FP-29 NOT RUN; Q2 milestone remains IN_PROGRESS pending its acceptance review.**
DEV-04-03/04/09/10/12 and DEV-06-03. Existing AWU-1–4 were not rebuilt as new work.

## Independent actual work units

| Unit | Input | Work | Output | Done condition | Evidence boundary | Readback/verification | Failure return | Downstream impact |
|---|---|---|---|---|---|---|---|---|
| AWU-5A resource and repeat combat | Saved canonical scene, shared damage/death pipeline, Soul bridge, AWU-4 carry-forward | Wire one real lethal-result notification to defeat grant; prioritize newly eligible source over prior proof; prevent full-Formation CTA | Production runtime changes and saved-scene integration tests | Player and allied defeat grants exactly once; five raises and six encounters; capacity guard | Review balance 10, grant 4, cost 3; no persistence/final economy claim | RED failures reproduced; targeted green; duplicate/no-op advances preserve balance/revision | Return to grant transaction, presenter or capacity guard according to failed assertion | Repeated conversion now remains actionable and truthful |
| AWU-5B input, semantic and lifecycle integration | Normal canonical composition, Figma 11:89 14-state contract | Real EventSystem raycast tests; automatic Update battle; lifecycle disable/re-entry; render all 14 keys; classify five non-default paths | Four integration tests, expanded renderer assertion, normal-path visual observer | Actual hit-tested CTA, no disabled-button mutation, one overlay, no grant replay, exact-unit contribution | Component/seam states are not canonical gameplay screenshots | 35/35 targeted; automatic battle and genuine ownership/contribution assertions | Return to input routing, lifecycle binding or state projection | Supports final E2E acceptance without a test-only gameplay adapter |
| AWU-5C build and evidence closure | Final matching main/recovery sources and saved artifacts | Reimport/reopen main; build Windows player; OS mouse input; resolution/SafeArea capture; regression; independent code review | XML/logs, 23 PNGs + geometry, 10 OS-click records, build/source hashes | Fresh completion markers, pixel dimensions match, protected zone clear, regression passes | Windows Development execution; simulated SafeArea; production/device excluded | 124/124 full regression; final verification.txt; saved hierarchy readback; review no Critical/Important issues | Build/import/input/capture failure stays FAIL until corrected and rerun | A2-2B1 bounded closure; FP-29 acceptance is the direct consumer |

## Implementation and execution

The common damage/death pipeline emits its actual lethal result once after creating the source and defeat event. Canonical composition calls the existing Soul bridge using the current account revision. Player and allied attacks share this pipeline. Existing duplicate/retry/idempotency transaction tests remain in the fresh full regression.

The new source's Eligible/InsufficientSoul availability outranks an old Raise proof receipt. Army proof remains independent, so observing a prior ally no longer hides a new actionable Raise. Formation capacity disables the CTA before an invalid normal click; it does not falsely label an eligible source as consumed.

Failure progression was executed: resource RED 1/3; grant green/repeat RED 2/3; presenter green/capacity RED 2/3; final unit green 3/3. The first broad run found two null-slot renderer regressions; a null-safe slot predicate fixed those, followed by 123/123 and final 124/124 fresh PASS. An initial native-input run failed on Windows 125% DPI coordinates; thread-local DPI awareness corrected client-pixel conversion. Atomic immutable input request files avoid reading a partially written command. Failed runs are not completion evidence.

Final tests:
- [targeted XML](awu-5/AWU4-U5-closure-targeted.xml): 35/35, zero failed/skipped, 2026-10-03 17:22:50–55Z.
- [full PlayMode XML](awu-5/AWU4-U5-closure-regression.xml): 124/124, zero failed/skipped, 17:24:07–12Z.
- Unmodified matching test/build/reimport logs are preserved in unity-logs.zip alongside the XMLs (archive preserves Unity-emitted whitespace). Unity 6000.3.25f1. Known-good process-local HOME/USERPROFILE and TMP/TEMP recovery was retained; Test Runner did not combine -runTests with -quit.
- Main Unity reimport/reopen readback and saved hierarchy are included. Five changed runtime files and two test files match recovery SHA256 in [verification](awu-5/verification.txt).
- Windows Development build succeeded. Build GUID 48db96ec38a94ca5b0ad55c08339b24b; executable SHA256 B9FC30BB05CA93A042831974149A681D656963E3E612ECAFD9AE826759D739AD.
- Reproducible actual Windows input driver: [script](../../scripts/verify-first-playable-native-input.ps1). It launches its own player, converts Unity coordinates to DPI-aware client pixels, clicks only that window, and requires exit 0 plus exactly 10 clicks and a fresh completion marker.
- Native run 20261003T172043846-6c4fdaddc223462983df28af041e304c: automatic Update combat, five real Raise clicks and five Next clicks, six encounters. No manual combat Advance or test-state adapter in this branch. Balances: first defeat 14, first Raise 11, second allied defeat 15; fifth Raise 15, sixth defeat 19. Five actual owned units; capacity CTA disabled, Next still available.
- Exact first-raised-unit contribution is observed. The newest fifth unit can truthfully remain ProofPending when earlier allies defeat the next target first. Five owned units does not mean five proven contributors.
- [native second eligible capture](awu-5/native/390x844-native-2-eligible.png) and [full army capture](awu-5/native/390x844-native-full-army.png).

## Figma semantic evidence boundary

Figma canonical eXqKU1qHXsn52SJfIGltZo node 11:89 was freshly read and visually inspected before comparison. Target 3 + Raise 7 + Army 4 = 14 semantic keys. The renderer now actually renders each key in the executed component test. This does not imply all keys are reachable or captured in the default canonical loop.

| Semantic key | Canonical condition/evidence |
|---|---|
| TargetActive | Live enemy; canonical active PNG |
| TargetDefeated | Real lethal result; canonical eligible/raised/proof PNG |
| TargetNone | No enemy snapshot; executed projector/renderer boundary. Default composition synchronously spawns before binding: no observable normal-input capture |
| RaiseTargetNotReady | Live target; canonical active PNG and disabled input test |
| RaiseEligible | Defeated unconsumed source and sufficient Soul; actual OS-click captures |
| RaiseCommittedAwaitingProof | Successful actual conversion before that exact unit contributes; canonical raised PNG |
| RaiseProofObserved | Actual committed UnitId contribution; canonical proof PNG |
| RaiseNoTarget | No enemy snapshot; executed projector/renderer boundary, not default canonical capture |
| RaiseSourceUnavailableOrConsumed | No usable source/consumed source without a valid retained proof receipt; executed projector/renderer boundary. Default receipt normally yields committed/proof |
| RaiseInsufficientSoul | Defeated usable source with insufficient account balance; executed real-Soul-policy seam/component boundary. Default review 10 + 4 - 3 loop does not reach it |
| ArmyEmpty | No owned roster; canonical active/eligible PNG |
| ArmyProofPending | Actual owned latest unit lacking exact proof; canonical raised and full-army PNG |
| ArmyProofObserved | Actual exact-unit contribution; canonical proof/second-eligible PNG |
| ArmyOwned | Occupied Formation without valid receipt, such as fresh/lost HUD session; executed projector/renderer boundary. Default initial empty roster with retained session does not reach it |

Five absent default canonical states are deliberately distinguished from nine captured normal domain states. No production adapter was added to manufacture them.

## Visual, geometry and SafeArea readback

Final responsive run 20261003T172130982-3d65b347c5684b3989f239ad55ae3d1e produces 12 actual PNGs: 390×844, 768×1024, 360×640, each active/eligible/raised/proof. Native automatic input adds 11 actual 390×844 PNGs. All 23 PNG dimensions were read from image bytes and matched their requested actual dimensions. Every metadata file reports one overlay, no TMP overflow and all five world-corner rectangles:
TargetStatusReadabilityZone, RaiseActionStatusReadabilityZone, ArmyStatusReadabilityZone, ProtectedCombatReadabilityZone and CombatViewport. All HUD-zone maximum Y values are below the protected-zone minimum Y.

SafeArea evidence is explicitly simulated 4% vertical insets. Fresh saved-scene lifecycle/restart/layout tests include 1080×2400 geometry; 1080×2400 actual screenshots remain NOT RUN because the Windows player clamps that size. 768×1024 supplies another portrait ratio with actual pixels; this is not a phone-device result.

Figma verified contract retained: card radius 18, padding 16/gap 8, typography 12/20/14, dark #202532, white foreground, secondary #4d5566, info #1864ab, Soul #087f5b and danger #c92a2a. Actual fresh captures were inspected alongside the canonical state board. AWU-4 rounding, scaling, state accents, disabled CTA and protected-zone layout fixes remain verified by new captures/regression. Soul copy now shows the actual balance/cost. The material stale-proof/next-Raise mismatch was corrected.

Explicit Q3 carry-forward: full-Formation disabled “ARMY FULL” is a composed capacity guard beyond the canonical 14 source/state cards; obtain a canonical design extension rather than mislabeling it SourceConsumed. Full final font/copy/art direction and production visual convergence remain separate. Review fonts, literal state keys and block graphics are review artifacts.

## Limits and downstream status

- AWU-5: PASS, IMPLEMENTED / EXECUTED / VALIDATED within this review-runtime scope.
- A2-2B1: PASS within its integrated HUD review boundary, combining EV-008–012; no new claim for higher evidence levels.
- FP-29: NOT RUN as a separate acceptance work item. Its exact current acceptance contract must be recovered before completing it. Q2 remains IN_PROGRESS; next action is FP-29 first-value-loop acceptance and Q2 exit review.
- Physical device, mobile build/install/launch: NOT RUN.
- Final production font/copy/icon and asset rights: UNKNOWN / NOT RUN.
- Real user fun, retention, monetization, crash-in-production and persistence: NOT RUN.
- Minor QA-tool limitation: latest-run pointer is a direct write and can cause a false failure under a concurrent read; no false PASS. Immutable click requests and unique-run completion checks are enforced.
- No extra plugin required. Gemini can independently review the evidence and visual comprehensibility without editing the active canonical repository.

User can open C:\Dev\Necrom in Unity 6000.3.25f1, open Assets/Necrom/FirstPlayable/Scenes/FirstPlayable.unity, press Play, wait for defeat, click RAISE and NEXT ENCOUNTER. This is a playable review build, not final art or mobile validation.
