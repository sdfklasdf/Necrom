# Q3 Production UI Fidelity — EV-019

Date: 2026-10-06 KST
Project: Necromancer / QUALITY_GAME
Input baseline: EV-018 / main f3f8ad65b86c4a7f4501dd471897056f8bf29450
Milestone: Q3 VERTICAL SLICE IN_PROGRESS
Verdict: PASS for the bounded production UI typography / semantic-icon / copy fidelity scope. Q3 overall remains PARTIAL / IN_PROGRESS.

## Founder approvals consumed

Already approved:
- commercial direction: A — OBSIDIAN SOUL
- functional UI font baseline: Noto Sans KR / Noto Sans CJK KR
- 3-weight strategy: Regular / Medium / Bold
- production-art path: image-generation refinement with per-file provenance

This checkpoint does not approve final legal rights, production-final audio/motion, physical-device acceptance, accessibility acceptance, user fun/retention or launch content count.

## Actual Work Unit — UI1 production typography + semantic icon runtime

Implemented:
- production OTF sources added under Assets/Necrom/FirstPlayable/Fonts/Production/
  - NotoSansKR-Regular.otf
    SHA-256 69975A0AC8472717870AEFEAB0A4D52739308D90856B9955313B2AD5E0148D68
  - NotoSansKR-Medium.otf
    SHA-256 B46988EF13E8BAC08F3933AF686EAF770972994F9B6D335BE0184D60169B5431
  - NotoSansKR-Bold.otf
    SHA-256 5A6CEB287ED2FC6CFC6213144EBEA68CBD94B20FC9EB873D8486493BF02D9BDA
- Unity-generated TMP SDF assets persisted for all three weights.
- canonical scene/prefab now serialize Regular / Medium / Bold.
- runtime HUD hierarchy:
  - State Key = Medium
  - Primary Text = Bold
  - Secondary Text = Regular
  - Primary CTA = Medium
  - next-encounter production label = Medium
- semantic state marker changed from dot-only treatment to explicit generated icon sprites:
  - Target family = Combat
  - Raise family = Soul
  - Army family = Army
- state still uses icon + label + semantic color, so color is not the only state carrier.
- full-capacity disabled CTA production copy = "군단 최대".

Public compatibility:
- existing one-argument FirstPlayableCombatHudFontProviderAdapter constructor remains the single public constructor.
- weighted production runtime uses WithWeights(...) factory.
- this preserves pre-existing reflection/test fixture contracts.

Readback:
- FirstPlayableQ3Art.Apply exit 0.
- CANONICAL_REOPEN_PASS components=19.
- Q3_PRESENTATION_REOPEN_PASS art=4 motion=5 audio=5.
- Regular/Medium/Bold source fonts are all required to resolve from the approved Production folder.

UI1 verdict: PASS.

## Regression history

First targeted run after implementation:
- 9/9 PASS.

First full run:
- 122/133 PASS, 11 FAIL.
- failure was not accepted.
- all 11 failures were existing HUD foundation/lifecycle/runtime-binding fixtures.
- root cause: adding a second public FontProviderAdapter constructor broke tests that intentionally require a single public constructor via reflection.

Recovery:
- restored one public compatibility constructor.
- moved 3-weight construction behind public static WithWeights(...).
- no existing test fixture contract was weakened.

Fresh post-fix targeted:
- FirstPlayableQ3VisualTests
- 9/9 PASS
- failed 0
- skipped 0
- duration 1.9539314 s

Fresh post-fix full relevant PlayMode:
- 133/133 PASS
- failed 0
- skipped 0
- duration 6.8859531 s

Artifacts:
- docs/evidence/artifacts/Q3/EV019-production-ui/targeted.xml
- docs/evidence/artifacts/Q3/EV019-production-ui/full.xml
- docs/evidence/artifacts/Q3/EV019-production-ui/raw-logs.zip

## Windows development build + responsive runtime

Fresh build:
- FirstPlayableCanonicalScene.BuildVisualPlayer
- BUILD_EXIT=0
- VISUAL_PLAYER_BUILD Succeeded errors=0
- buildGUID=9df8ef86d88c491ebbaa3b34e278b3e5

Fresh responsive run:
- run=20261005T161653919-9c02137f0cff45c3916fccd11f5b3eb4
- player exit 0
- 390x844 / 768x1024 / 360x640
- active / eligible / raised / proof captures completed
- overlayCount=1
- q3ProtectedUnitArt=PASS
- no HUD text overflow in the fresh 360x640 readback
- physicalDevice=NOT RUN

Artifacts:
- docs/evidence/artifacts/Q3/EV019-production-ui/responsive/

## Native normal-input runtime

Fresh native run:
- run=20261005T161705372-1ba2ebaade8646fab8b885202d0e6627
- existing evidence-only foreground focus-retry driver used; product input logic unchanged
- NATIVE_INPUT_PASS=10 real OS clicks
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
- all HUD text groups reported overflow=False
- full-capacity CTA readback content = "군단 최대" (PowerShell console mojibake does not alter the UTF-8 capture file)

Artifacts:
- docs/evidence/artifacts/Q3/EV019-production-ui/native/

## Figma/runtime synchronization

Figma file:
eXqKU1qHXsn52SJfIGltZo

Production canonical frames retained:
- Battle 35:234
- Raised 35:245
- Full Formation 35:256
- 360x640 Full Formation 36:290
- provenance note 36:305

EV-019 synchronization:
- dot-only state accents in the four production frames were visually replaced with shape-redundant semantic treatment:
  - Combat
  - Soul
  - Army
- full-capacity CTA override changed from "군단 가득 참" to "군단 최대".
- existing component instances were not detached.
- current Figma font family remains Noto Sans KR.
- Figma direct font-style API inspection was blocked by tool safety checks, so no unsupported claim is made that every Figma text node is now separately bound to Medium/Bold font files.
- runtime 3-weight hierarchy is independently executed and PlayMode-verified.

Visual readback:
- Full Formation 390x844 screenshot preserved at artifacts/Q3/EV019-production-ui/figma/full-390x844.png.
- Full Formation 360x640 screenshot preserved at artifacts/Q3/EV019-production-ui/figma/full-360x640.png.
- both show semantic shape markers, "군단 최대", five repeated Raised Guard slots, and unclipped HUD.

## Evidence boundary

PASS in this checkpoint:
- Noto 3-weight runtime packaging/binding
- semantic icon runtime generation and family mapping
- production full-capacity CTA copy
- canonical scene/prefab readback
- targeted 9/9
- full PlayMode 133/133
- Windows development build
- responsive development-player capture
- native OS input 10/10
- Figma production-frame semantic-icon/copy sync

Still UNKNOWN / NOT RUN:
- final font release packaging/attribution audit
- final production icon art source if generated-runtime glyphs are later replaced by authored vector assets
- final release-rights/legal clearance for production art
- production-final motion asset polish
- production-final sound design/mix/loudness
- representative physical mobile install
- actual mobile SafeArea
- physical-device accessibility acceptance
- real-user fun/play/retention evidence
- approved launch monster/content count

Guard remains one archetype.
Formation five slots remain repeated Raised Guard instances, not five monster species.
Launch monster count remains UNDECIDED.

## Current verdict

EV-019 bounded scope: PASS.
Q3 overall: PARTIAL / IN_PROGRESS.

## Next recommended execution

Close the remaining presentation-system production gap before physical-device acceptance:
- author/approve production-final motion and audio assets/mix contract for attack, hit, defeat, Raise and exact allied contribution;
- keep current deterministic review SFX as wiring evidence only;
- after runtime media changes, run affected targeted → relevant full regression → build/runtime visual;
- then move to representative physical mobile/SafeArea/A11Y acceptance.
