# A2-2B1 AWU-4 — canonical runtime composition and visual QA
Date: 2026-10-03 UTC. Baseline main a0665cd63c5cd177bbb58af4e03673aaa93fce69.
Verdict: PASS for bounded Q2 review composition / simulated SafeArea / desktop development-player visual QA. A2-2B1 overall and FP-29 remain IN_PROGRESS; AWU-5 NOT RUN.

## Independent work units
|Unit|Input / work|Output / done condition|Evidence / readback|Failure return / downstream|
|---|---|---|---|---|
|4A|Existing production battle, raise, roster, HUD; create composition and save|FirstPlayable.unity + FirstPlayable.prefab, serialized font/runtime hierarchy|Unity import + reopen, saved artifacts|Return to composition; visual QA depends on this|
|4B|Actual domain/runtime dependencies; execute combat, raise, ally contribution and lifecycle|Normal binding; one overlay; disable/re-entry/destroy clean|Six canonical-scene tests, no test-only state adapter|Return to wiring; state/visual QA blocked if invalid|
|4C|Canonical composition; configure portrait sizes + simulated SafeArea|Five geometry zones, no HUD/protected overlap|12 player geometry readbacks; 390x844/768x1024/360x640, simulated 4% vertical insets; test also simulates1080x2400|Return to layout; physical-device remains separate|
|4D|Fresh Figma11:89 + CTA7:26 contract; compare actual PNGs|Material renderer mismatches repaired; unresolved production assets explicit|12 actual PNGs; representative frames visually inspected; typography/color/CTA/semantic mapping|Return to renderer, design unchanged; final assets Q3 carry|
|4E|Final source/artifacts|Regression, build, fresh capture, source equivalence|6/6 targeted;120/120 full PlayMode; Windows Development build succeeded; main reopen succeeded;10 source/artifact hashes match recovery|Return to failing unit; AWU-5 integration next|

## Runtime and evidence
Scene: Assets/Necrom/FirstPlayable/Scenes/FirstPlayable.unity.
Prefab: Assets/Necrom/FirstPlayable/Prefabs/FirstPlayable.prefab.
Composition creates real Formation/Application/Battle/EnemySpawn/Roster/SoulBridge/HudSession and production projector/lifecycle/runtime binding. Automatic player/allied loops run by default; normal Raise/Next buttons call domain paths. Capture uses manual advancement of those same production loops for deterministic observations; it does not inject a HUD state adapter.
The opt-in capture collector runs only with --necro-visual-qa. Each run gets a fresh directory and manifest/buildGUID, with completion only after success. Evidence committed here came from run20261003T164226412-d25330414e0f4e9192331f978c1004e9, buildGUID6f8f886c7cfd4138b38f71c074a5e9c1. See build-hash.txt.
Saved scene reimport/reopen in main succeeded; validation/build ran in recovery with matching changed source and scene/prefab hashes.
XML results are retained, not inferred from process exit. Relevant whole PlayMode:120 passed,0failed,0skipped. Test TMP fixture materials now use the actual TMP shader because importing real TMP resources exposed invalid UI/Default fixture material _CullMode errors.
Each captured frame has exactly one HUD overlay and no TMP text overflow; geometry verifier checks all5 zones and HUD maximumY below protected minimumY.

## Figma comparison
Fresh canonical eXqKU1qHXsn52SJfIGltZo node11:89 is a semantic review board, not a portrait gameplay screen. Its342x112 cards establish state/component intent. Runtime stacks target/raise/army into portrait zones; Raise176high reserves CTA while retaining shared padding/type. No Figma modifications made.
|Semantic set|Figma ↔ runtime contract / executed capture coverage|
|---|---|
|Target3|None(gray),Active(info),Defeated(soul). Captures Active/Defeated; None supported by reviewed renderer/projector contract, not captured in canonical play|
|Raise7|NoTarget(gray),TargetNotReady(danger),SourceUnavailableOrConsumed(danger),InsufficientSoul(danger),Eligible(soul),CommittedAwaitingProof(info),ProofObserved(soul). Captures TargetNotReady/Eligible/Committed/Proof; remaining3 not captured in canonical play|
|Army4|Empty(gray),Owned(info),ProofPending(info),ProofObserved(soul). Captures Empty/Pending/Observed; Owned not captured in canonical play|

Verified theme comparison: panel #202532, white text, secondary #4d5566, info #1864ab, soul #087f5b,danger #c92a2a; type12/20/14, padding16,gap8. Card radius18, CTA radius12/M48. Info/secondary values additionally confirmed in fresh node/CTA reads. Disabled CTA gray with group opacity0.55; eligible soul/opacity1.
Material fixes: rounded panels and CTA; state accent dot/color; insufficientSoul remains dark panel with danger accent; disabled CTA gray/opacity; correct overlay scale on viewport change; configured SafeArea retained through encounter restart. Tests were observed failing before corresponding fixes and passing afterward.
Final PNG comparisons show consistent semantic colors/type hierarchy, no clipped text, and separate protected combat/HUD regions. The reviewfont is LiberationSans SDF from imported TMP Essentials, English diagnostic copy, and simple colored combat markers. These are review assets, not approved production font/copy/art.

## Evidence limits and carry-forward
- 1080x2400 actual pixels: NOT RUN. Windows clamped an earlier requested1080x2400 to1080x1080; those images are excluded from committed final evidence.1080x2400 geometry simulation passed, which is not actualpixel screenshot evidence.768x1024 provides a genuinely executed alternate portrait ratio;360x640 a short portrait.
- SafeArea: simulated insets validated; physical-device notch/readability/gameplay NOT RUN.
- Windows development build + launch/capture EXECUTED. Mobile build/install/launch/store NOT RUN.
- Production final font/copy/icon/sprite/provenance/assetrights UNKNOWN/NOT RUN. No asset clearance inferred from TMP import.
- All14 semantic states mapped;9distinct states actually captured via canonical domain paths. Complete canonical state-path integration is AWU-5 work.
- Soul defeat-grant policy is configured but canonical auto loop does not yet invoke ApplyDefeatGrant. Initial review balance funds3raises. Economy continuity / repeated encounter inputs and grant idempotency must be resolved or explicitly scoped in AWU-5; no claim of complete economy loop.
- Real user fun, retention, production performance/crash evidence NOT RUN.
- AWU-5 entry satisfied for integrated normal-input evidence/regression, with Soul grant and missing canonical state paths carried forward. Do not mark A2-2B1/FP-29 PASS before that closure.

## Reproduction
scripts/verify-first-playable.ps1 restores HOME/TMP/PROGRAMDATA only in the launched process. -runTests omits -quit. Use -Filter ALL for full regression.
Editor methods in Necrom.EditorTools.FirstPlayableCanonicalScene: Generate, Readback, BuildVisualPlayer; ImportReviewFont asynchronously imports TMP essentials (omit -quit). Generation saves actual prefab and scene; generated assets are committed.
