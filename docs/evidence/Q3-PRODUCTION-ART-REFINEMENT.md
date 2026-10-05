# Q3 Obsidian Soul Production-Art Refinement — EV-018

Date: 2026-10-05 KST
Project: Necromancer / QUALITY_GAME
Input baseline: EV-017 / main 37d2338fa5d872fe3693d4a41b3b2925a52e5595
Milestone: Q3 VERTICAL SLICE IN_PROGRESS
Verdict: PASS for the bounded production-art refinement/integration scope. Q3 overall remains PARTIAL / IN_PROGRESS.

## Founder approvals consumed

Explicit Founder approval in the current conversation:
- commercial direction: A — OBSIDIAN SOUL
- functional UI baseline: Noto Sans KR / Noto Sans CJK KR
- production-art path: image-generation refinement with per-file provenance

This approval does not approve final legal rights, final copy/icon, production audio, physical-device quality, accessibility or real-user fun.

## Actual Work Unit — PA1 production candidate pack

Created/imported four production-candidate images:
- Necromancer: Assets/Necrom/FirstPlayable/Art/Q3/ProductionCandidate/player.png
  - 1024x1536 RGBA
  - SHA-256 a7f6fe76ceb0021858f596c230c02b24274d303110cf03a79f27b6faa6df7fbc
- Hostile Guard: .../guard.png
  - 1024x1536 RGBA
  - SHA-256 387d3070f00abb0db2350ae6acac57b4f8a7fb24828e61e609b0893cf4eed591
- Raised Guard: .../raised.png
  - 1024x1536 RGBA
  - SHA-256 afca05119e17010e62663379ab1c686e9a2d9faec0e84598aff5f5155a2d23ee
- Cemetery background: .../background.png
  - 1448x1086 RGB
  - SHA-256 4447edd20acdd581eaa519a24d3b41fb83d513ca30ae071fa54d1d1f6817c101

Direction contract:
- obsidian/slate base
- emerald = Soul / Raised
- crimson = hostile danger
- violet = secondary Necromancer / Arcane accent
- environment saturation below foreground-unit emphasis

Guard remains one archetype. Raised Guard is the raised version of the same Guard.
Formation five slots remain repetitions of this one archetype and are not five monster species.
Launch monster count remains UNDECIDED.

Per-file image-generation IDs, hashes and evidence boundaries are recorded in:
Assets/Necrom/FirstPlayable/Art/Q3/ProductionCandidate/provenance.json

Rights boundary:
- generation provenance: RECORDED
- final release-rights/legal clearance: UNKNOWN / NOT RUN

PA1 verdict: PASS as production candidate creation + provenance, not final legal clearance.

## Actual Work Unit — PA2 Figma canonical production candidate

Existing approval/design artifacts:
- commercial direction board: 33:177
- production presentation contract: 33:274

Founder approval state was written into these review artifacts.

Production-candidate Figma frames:
- Battle: 35:234
- Raised: 35:245
- Full Formation: 35:256
- 360x640 Full Formation: 36:290
- Provenance note: 36:305

The new raster assets were uploaded directly into their corresponding Background / Necromancer / Guard / RaisedGuard fills.
Upload result: 24/24 target placements returned HTTP 200 across the three 390x844 frames and one 360x640 frame.

Visual readback:
- 390x844 Battle: Necromancer and hostile Guard silhouettes remain distinct on the new cemetery.
- 390x844 Raised: one emerald Raised Guard remains readable after conversion.
- 390x844 Full Formation: five repeated Raised slots remain visible without changing species count.
- 360x640 Full Formation: five Raised slots remain separated inside the protected combat band; CTA/HUD remains unclipped.

Figma frame existence is not counted as runtime evidence.

PA2 verdict: PASS for canonical design candidate integration.

## Actual Work Unit — PA3 Unity canonical integration

Changed canonical art binding:
- FirstPlayableQ3Art.ArtPath now targets:
  Assets/Necrom/FirstPlayable/Art/Q3/ProductionCandidate/
- scene and prefab were rebound to the four production-candidate textures.
- Q3 art Readback now rejects any canonical texture reference outside the approved ProductionCandidate path.
- PlayMode test ProductionCandidateTexturesAreBoundToApprovedPath was added.

Unity apply/reopen:
- command: Necrom.EditorTools.FirstPlayableQ3Art.Apply
- log: Artifacts/EV018-production-art-apply.log
- result:
  - canonical scene reopen PASS
  - Q3 presentation reopen PASS
  - art references 4/4
  - authored motion 5
  - audio clips 5/5
  - batchmode exit 0

PA3 verdict: PASS.

## Actual Work Unit — PA4 fresh regression/build/runtime evidence

Validation project was reset to origin/main 37d2338 and only EV-018 changes were overlaid.

Fresh targeted PlayMode:
- FirstPlayableQ3VisualTests
- 8/8 PASS
- failed 0
- skipped 0
- duration 1.3842251 s
- includes approved ProductionCandidate asset-path assertion

Fresh full relevant PlayMode:
- 132/132 PASS
- failed 0
- skipped 0
- duration 6.2512041 s

Windows development build:
- CANONICAL_REOPEN_PASS components=19
- VISUAL_PLAYER_BUILD Succeeded errors=0
- Unity batchmode successful exit
- refreshed player data:
  - NecromVisual_Data/sharedassets0.assets
  - size 16,640,456 bytes
  - timestamp 2026-10-05T23:17:55.2073810+09:00
  - SHA-256 5DE095D758E9E82F1B3622A4F3D3832249F21703CC04C3973B7E4D6E5BEFB8EB

Fresh responsive runtime:
run = 20261005T141956627-d1487aacc3244c2c80bf21eff94f5468
- 390x844 / 768x1024 / 360x640
- active / eligible / raised / proof = 12 captures
- every capture:
  - q3ArtLoaded=4
  - overlayCount=1
  - q3ProtectedUnitArt=PASS
  - authoredMotion=True
  - audioClips=5
  - text overflow True count=0
- 360x640 Raised slot geometry remains 30.24 x 45.36 px.
- direct PNG visual readback confirmed the new production candidate art is present in the actual Windows player.

Native normal-path runtime:
A first monitored attempt was invalidated by foreground-window interference from concurrent diagnostics and timed out after request 3. It is not counted as product failure or PASS.
The run was repeated without foreground interference using the existing evidence-only focus-retry driver.

Successful native run:
run = 20261005T142359405-25dc335bc1ef483a83e33e4f9db8dfb4
- actual OS mouse input: 10/10 requests
- player exit 0
- soulBalance=19
- alliedCount=5
- visibleAllyArt=5
- overlayCount=1
- q3ProtectedUnitArt=PASS
- authoredMotion=True
- audioClips=5
- audioListenerCount=1
- reviewSfxPlaybackCount=51
- playerAttackCues=7
- hitCues=20
- defeatCues=6
- raiseCues=5
- alliedContributionCues=13
- lastContributionUnitId=ally:35
- all three HUD text groups report overflow=False
- direct full-army PNG readback confirmed all five repeated Raised Guard slots render with the new art.

PA4 verdict: PASS for current-head Windows desktop review/runtime scope.

## Noto functional baseline boundary

Founder approved Noto Sans KR / Noto Sans CJK KR as the functional UI baseline and the 3-weight strategy Regular / Medium / Bold.

Current implementation:
- bundled/current runtime review SDF remains NotoSansCJKkr-Regular.
- repository includes SIL OFL 1.1 license text.
- Medium/Bold runtime font asset packaging is NOT IMPLEMENTED in EV-018.
- final release packaging/attribution review is NOT RUN.

No false claim of completed 3-weight runtime implementation is made.

## Q3 status

Completed in this checkpoint:
- Founder commercial direction approval
- Founder Noto functional baseline approval
- Founder imagegen refinement path approval
- four production-candidate visual assets with provenance
- Figma production-candidate frames including 360x640
- Unity scene/prefab production-candidate binding
- fresh targeted 8/8
- fresh full PlayMode 132/132
- successful Windows development build
- fresh responsive 12 captures
- fresh native 10/10 input / full Formation runtime evidence

Still open before Q3 overall PASS:
- final release-rights/legal clearance for production art
- final production copy/icon assets
- Noto Medium/Bold runtime packaging + final attribution/release check
- production-final motion assets/animation polish
- production-final sound design/mix/loudness
- representative physical mobile install
- actual mobile SafeArea
- accessibility acceptance
- actual user fun/play evidence
- approved launch content/monster count

Q3 overall verdict: PARTIAL / IN_PROGRESS.
