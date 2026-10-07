# EV-023 — Canonical Hybrid Composition + Visible Wave/Gate HUD

Status: PASS for the bounded Windows canonical-hybrid scope. Q3 overall remains PARTIAL / IN_PROGRESS.

## Scope

This evidence converts the EV-022 defense-wave seam from test-only/runtime-foundation evidence into the default saved canonical gameplay path.

Founder direction remains:
- idle RPG + simple defense hybrid;
- preserve Necromancer / Raise / five-slot Formation / automatic combat / Obsidian Soul presentation;
- Guard remains one archetype;
- five Formation slots remain repeated instances of that same Raised Guard;
- launch species/content count remains UNDECIDED.

The representative slice uses two simultaneous threats and gate integrity 10 only as slice inputs. They are not launch-content or final-balance decisions.

## AWU-1 — canonical hybrid gameplay wiring

Implemented in the saved canonical scene/prefab and FirstPlayableGameplayComposition:
- FirstPlayableDefenseWaveRuntimeController is now owned by the normal composition;
- both player and Raised-ally auto-combat use defense-wave truth;
- wave 1 starts with two tracked Guard threats;
- first threat defeat leaves Battle and Defense Running;
- the remaining active threat becomes the combat target;
- the first defeated threat remains the Raise candidate;
- Raise activates Formation slot 0 and the Raised Guard can contribute against the remaining threat inside the same wave;
- final threat defeat clears the wave and resolves Battle;
- StartNextEncounter starts the next sequential wave only after Cleared;
- a canonical gate-breach orchestration seam verifies Running -> Failed truth;
- saved scene and prefab contain both defense runtime and defense HUD components.

Fresh saved-artifact readback:
- CANONICAL_REOPEN_PASS components=21
- CANONICAL_HYBRID_COMPONENTS_ENSURED

## AWU-2 — visible wave/gate HUD + representative Windows evidence

New runtime component:
Assets/Necrom/FirstPlayable/Runtime/FirstPlayableDefenseWaveHudRuntimeBinding.cs

Direct design input:
- Figma file eXqKU1qHXsn52SJfIGltZo
- node 46:309
- semantic states RUNNING / CLEARED / FAILED
- wave number, gate integrity/max, active/remaining threats, gate integrity bar.

Runtime mapping preserves the existing combat HUD overlay and production art. It adds no second Canvas. The panel is positioned immediately above ProtectedCombatReadabilityZoneMirror and is refreshed from FirstPlayableDefenseWaveHudState.

Bounded design mapping:
- RUNNING accent #087f5b
- CLEARED accent #31d49b
- FAILED accent #c92a2a
- panel #202532
- detail #c1c7d0
- gate track #343b4b
- 11 / 16 / 12 semantic typography roles.

This is a semantic/runtime mapping claim, not a pixel-perfect Figma certification.

## Fresh Unity regression

Final canonical-targeted:
Artifacts/EV023-canonical-targeted3.xml
- 7/7 PASS

Final integrated-loop targeted:
Artifacts/EV023-integrated-targeted.xml
- 4/4 PASS

Final production visual targeted:
Artifacts/EV023-q3visual-targeted.xml
- 10/10 PASS

Final full PlayMode after the code changes:
Artifacts/EV023-final-full-playmode.xml
- 137/137 PASS
- failed 0
- skipped 0

The new canonical gate test verifies:
- gate 10 -> 6: Defense Running, one threat remains;
- gate 6 -> 0: Defense Failed, Battle Defeat;
- visible defense HUD state is FAILED and includes 0 / 10.

Important boundary: this gate failure is actual Unity canonical orchestration evidence, but ordinary automatic player gameplay does not yet autonomously move a threat into the gate.

## Fresh Windows build

Artifacts/EV023-build.log
- CANONICAL_REOPEN_PASS components=21
- VISUAL_PLAYER_BUILD Succeeded errors=0
- Unity batch exit 0

Build:
Artifacts/VisualPlayer/NecromVisual.exe
- size 667136
- executable SHA256 B9FC30BB05CA93A042831974149A681D656963E3E612ECAFD9AE826759D739AD
- buildGUID c7000ae4a8ce4a4daf1ce688abcb4a5d

The Windows launcher executable hash can remain stable while Unity data changes; data identity for this build also includes:
- globalgamemanagers SHA256 B333C7F2B4AD719B7E36D0B479EF59741B974E7D0079354EFF6BA7A0AE6B6D02
- level0 SHA256 185CC4031211D6F2A4985FC1F0808811943E761714F6622354D24A46629011B4
- resources.assets SHA256 9594474C90D7E9840DF8FBE7164F3DAFA18B80F0FF4A8FD4D3A7C93A429296EF
- sharedassets0.assets SHA256 71C33A5783E36802FF8D1A0DBBBBBB41F6DE2F663D90C504B2A4702B5643C9E2

## Representative responsive Windows runtime

Canonical run:
Artifacts/VisualPlayer/VisualEvidence/20261007T080129029-78c9d2efdc02497d977c1c3900bada87

Actual requested/executed portrait sizes:
- 390x844
- 768x1024
- 360x640

At all three sizes:
- defenseHudAboveProtectedCombat=PASS;
- existing q3ProtectedUnitArt=PASS;
- one Combat HUD overlay Canvas remains;
- defense state/title/detail text did not overflow;
- RUNNING and CLEARED state copy was visibly rendered;
- wave/gate panel remained above protected combat art.

The actual PNG captures were visually inspected, not inferred only from metadata. The compact defense panel remained readable and did not cover protected combat art or existing Target/Raise/Army cards.

Representative 390x844 success loop readback:
1. RUNNING, wave 1, gate 10/10, active 2.
2. First defeat: Battle remains Running, active 1, TargetActive points at the remaining Guard, RaiseEligible refers to the defeated candidate.
3. Raise: soul 14 -> 11, alliedCount 1.
4. Same-wave Raised ally contribution: ArmyProofObserved / RaiseProofObserved and actual UnitId observed.
5. Final threat defeat: CLEARED, active 0, gate 10/10, Battle resolved.

Responsive SafeArea evidence is simulated 4% vertical insets. It is not physical-device SafeArea evidence.

## Normal Windows OS input

Final driver result:
Artifacts/EV023-native-driver-console.txt

Result:
- NATIVE_INPUT_PASS=1 real OS click
- PLAYER_EXIT=0
- canonical run: Artifacts/VisualPlayer/VisualEvidence/20261007T080521627-d0fffb0d6cff4d7891cfe4d0f909b1ca

Driver evidence:
- DPI 120
- virtual client 312x675
- DPI-aware physical client 390x844
- one actual OS cursor/mouse click hit the Raise CTA.

The native branch keeps automatic Update enabled. It does not manually call AdvanceCombat or inject a HUD-state adapter.

Observed flow:
- automatic first threat defeat -> Defense Running / active 1;
- actual OS Raise click -> alliedCount 1;
- actual Raised ally contribution -> exact contribution proof;
- automatic final threat defeat -> Defense Cleared / Battle Resolved.

This is Windows desktop native-input evidence, not physical-mobile evidence.

## Evidence boundaries / remaining Q3 work

PASS here means only:
- saved canonical hybrid success loop is now real;
- visible runtime wave/gate HUD is mapped from the Figma semantic contract;
- responsive Windows representative captures exist;
- normal Windows OS Raise input works in the representative hybrid loop;
- canonical gate failure truth and visible FAILED state are executable in Unity tests.

Still NOT PASS / NOT RUN:
- ordinary automatic gameplay has no autonomous visible enemy-advance -> gate-breach pressure yet;
- GameStateSnapshot still does not persist wave/gate state;
- Founder creative acceptance;
- final rights/legal/font release approval;
- representative physical mobile SafeArea / speaker / earphone / accessibility;
- real-user fun/retention;
- launch monster/content quantity.

## Next direct consumer

VISIBLE GATE PRESSURE + NORMAL-PLAY FAILURE PATH.

Keep it bounded:
- make an active threat visibly progress toward the single defended gate/objective;
- actual normal gameplay can call RecordGateBreach without a test-only hook;
- gate HUD reacts to damage;
- failure can occur in the normal canonical player;
- preserve Raise/Formation/auto-combat/Obsidian Soul layout;
- do not add tower placement, multi-lane, launch content expansion, or final economy.
