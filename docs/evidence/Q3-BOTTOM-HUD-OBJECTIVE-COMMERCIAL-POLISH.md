# Q3 BOTTOM HUD COMMERCIAL POLISH + DEFENSE OBJECTIVE VISUAL REFINEMENT

Date: 2026-10-07
Baseline: CP-NECRO-V2-019 / EV-024
Parent main: 50be41852645100fd5fea4b2fedd83f98b170487
Scope: max 2 AWU, visual/runtime polish only. Existing Raise / Formation / auto combat / wave / gate-pressure mechanics were not reimplemented.

## AWU-1 — Bottom HUD commercial polish

Figma canonical working file:
- file: eXqKU1qHXsn52SJfIGltZo
- RUNNING: 52:309
- CLEARED: 52:378
- FAILED: 52:447

Design change:
- Replaced the three equal-weight stacked proof cards with one compact command dock.
- Combat/target summary is primary information, army count + five formation pips is compact secondary information, and Raise remains the only prominent action.
- RUNNING / CLEARED / FAILED defense truth is surfaced in a slim defense rail.
- Existing semantic content keys and canonical Raise CTA path remain intact.
- Runtime polish is additive over the verified HUD projection and does not own gameplay truth.

Runtime implementation:
- FirstPlayableCommercialHudRuntimePolish.cs
- FirstPlayableGameplayComposition.cs
- FirstPlayableDefenseWaveHudRuntimeBinding.cs

Failure return:
- First final-form canonical targeted run initially returned 6/8 because compact gate copy changed "0 / 10" to "0/10".
- No mechanics failure was present.
- Existing semantic copy contract was restored; rerun returned 8/8 PASS.

## AWU-2 — Defense objective visual refinement

Design/runtime direction:
- Replaced the green proof-only marker with an Obsidian Soul cemetery/soul-gate silhouette.
- Visual structure: stone pylons, lintel, barred opening, threshold, soul seal + halo.
- Running/cleared uses Soul accent; FAILED switches objective accent to Danger.
- Prior defended-objective center and enemy gate-pressure endpoint were preserved.
- enemyStartX / enemyGateX and gate-pressure mechanics were not changed.

Runtime implementation:
- FirstPlayableVisualPresentation.cs

## Verification

Targeted:
- EV025-final-targeted.xml: 8/8 PASS.
- EV025-targeted-q3visual.xml: 10/10 PASS.
- EV025-targeted-integrated.xml: 4/4 PASS.

Full relevant regression:
- EV025-full-playmode.xml: 138/138 PASS.

Windows build:
- EV025-final-build.log
- CANONICAL_REOPEN_PASS components=22.
- VISUAL_PLAYER_BUILD Succeeded errors=0.

Actual Windows player — commercial visual evidence:
- success run: 20261007T094726443-c8ed2cbcef214342920f7bb046abac70
- actual 390x844 = requested 390x844
- actual 768x1024 = requested 768x1024
- actual 360x640 = requested 360x640
- all capture metadata: overflow=True = 0
- all capture metadata: =FAIL = 0
- generated states per size: active, first-defeat, raised, ally-contribution, cleared.

Normal gate-pressure failure:
- failure run: 20261007T094833345-aa2b28b5dc714e8e81ea611cc799e9d6
- TargetNone
- RaiseNoTarget
- defensePhase=Failed
- gateIntegrity=0/10
- FAILED rail + red soul-gate objective + failed-state commercial copy visible.
- physical device NOT RUN.

Native OS Raise input:
- native run: 20261007T094907459-922e1be6aca14bda89edf9c0076c2f07
- one real OS mouse click injected into the Windows player.
- complete.txt: NATIVE INPUT + AUTOMATIC UPDATE CAPTURE COMPLETE.
- native-input-driver: request=1, client=300,115, pixels=390x844.
- player/driver exit path completed successfully.
- physical device NOT RUN.

## Evidence boundary / gate

PASS in this evidence:
- Figma commercial-state design and readback.
- Unity runtime application.
- semantic/mechanics regression.
- Windows build.
- Windows player captures at the three requested resolutions.
- normal automatic failure surface.
- OS-native Raise input path.

Still NOT RUN / not self-PASSed:
- Founder final creative approval of this commercial polish.
- final rights/legal clearance.
- physical iOS/Android device evidence.
- full accessibility / assistive-technology evidence.
- real-user usability/fun evidence.
- production audio final approval.

Q3 remains IN_PROGRESS / overall PARTIAL. This work upgrades the HUD/objective from proof surfaces to a stronger production candidate; it does not close the remaining Q3 evidence gates.
