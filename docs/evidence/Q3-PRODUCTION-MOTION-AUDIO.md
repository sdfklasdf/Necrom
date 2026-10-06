# Q3 Production Motion / Audio Convergence — EV-020

Date: 2026-10-06 KST
Project: Necromancer / QUALITY_GAME
Input baseline: CP-NECRO-V2-014 / EV-019 / main 05b1051b3dbd3d6c8a446261246470eb6a886e78
Milestone: Q3 VERTICAL SLICE IN_PROGRESS
Verdict: PASS for the bounded production-candidate motion/audio convergence scope. Q3 overall remains PARTIAL / IN_PROGRESS.

## Evidence boundary

This checkpoint proves that production-candidate motion and SFX were actually authored/generated, imported, bound, executed and regression-tested.

It does NOT prove:
- final human creative approval of the sounds or motion
- final release-rights/legal clearance
- representative physical-mobile speaker/headphone mix
- physical-device SafeArea or accessibility acceptance
- real-user fun, retention or commercial performance
- approved launch monster/content count

Those remain UNKNOWN / NOT RUN / UNDECIDED as applicable.

Guard remains one archetype. Five Formation slots remain repeated Raised Guard instances, not five monster species.

## AWU-M1 — Production-candidate motion authoring and runtime integration

Input:
- approved Obsidian Soul commercial direction
- existing event-driven FirstPlayableVisualPresentation
- existing review curves used only as wiring evidence
- attack / hit / defeat / Raise / exact allied-contribution event boundaries

Implemented production-candidate timing:
- attack: 0.24 s
- hit: 0.16 s
- defeat: 0.50 s
- Raise: 0.66 s
- allied contribution: 0.20 s

Curves:
- attack: readable anticipation at t=.12 (-.18), decisive forward snap at t=.42 (1.12), then settle
- hit: tight local response, no whole-screen shake
- defeat: five-key weighted vertical collapse with a low point before the final dead settle
- Raise: spectral pull / emergence into 1.24 overshoot reform, then settle
- allied contribution: concise local 1.12 pulse that identifies the exact contributing ally

Runtime presentation:
- Necromancer attack lunge increased to 11 px scale-relative travel.
- hit adds only a small local Guard kick; no screen shake was added.
- Raise applies local vertical soul-pull motion plus emerald translucent emergence.
- exact allied contribution is driven by the actual actor EntityId and applies local lift / scale / green pulse only to that ally.
- gameplay state, damage, formation and resource truth remain owned by existing domain/runtime systems.

Native execution evidence:
- attack/hit frame: attack cue=1, hit cue=1; Guard shows local red hit response.
- defeat motion frame: Guard scale=(1.000, 0.774, 1.000), defeat cue=1.
- Raise motion frame: RaisedGuardSlot0 is shifted ~6.7 px from settled position and rendered with emerald translucent color RGBA(0.341,1.000,0.661,0.551).
- exact ally contribution frame: lastContributionUnitId=ally:11; RaisedGuardSlot0 scale=(1.053,1.053,1.053), RGBA(0.580,1.000,0.720,1.000).
- all captured unit art remained inside the protected combat readability zone.

AWU-M1 verdict: PASS for technical production-candidate motion implementation/playback. Final human creative acceptance NOT RUN.

## AWU-A1 — Production-candidate SFX creation, provenance, import and mix

A distinct runtime folder was created:
Assets/Necrom/FirstPlayable/Audio/Q3ProductionCandidate/

The old deterministic Q3Review WAVs were not renamed or promoted.

Five new SFX were generated through the connected sound-effect generation tool and stored with per-file provenance:

1. attack-production.mp3
- task id: e4c6e981-b3ec-429c-b4c7-3de2a7bb3c9d
- requested: 0.8 s
- SHA-256: 9438FF79E2F749EA422A1199974ADB71DB5605B027863E216B30A3E4FAB260C0

2. hit-production.mp3
- task id: c5c96fbd-98f6-406e-92be-c7477c042a3b
- requested: 0.6 s
- SHA-256: B3D3A86D82E118149C756B5569999F02A9E7A2B613464B49B138A080C377D694

3. defeat-production.mp3
- task id: b9bdc8dc-394b-4041-9fc1-c9597a41581f
- requested: 1.2 s
- SHA-256: 1F261212A342AFB1504E33BE05CE79C3DB16FDA451FD541E3C915C93CDA4294B

4. raise-production.mp3
- task id: e6b77144-c231-4877-8d6b-158b3e113bba
- requested: 2.0 s
- SHA-256: D517EF7A2399BF1FA7F3B40E1537F515443D5FC2859092F18CF54315D626B3A7

5. ally-contribution-production.mp3
- task id: a96f5e05-b7bf-4409-b682-4ab4b44833fa
- requested: 0.7 s
- generator-reported: 0.680249 s
- SHA-256: 1B86FE91A132B5058C18E1DA6D53D47069D956D115BAEAD5587C72F806CAFAB9

Per-file prompts and provenance live in:
Assets/Necrom/FirstPlayable/Audio/Q3ProductionCandidate/PROVENANCE.json

Unity import contract:
- force mono
- DecompressOnLoad
- PCM
- preserve source sample rate
- preload audio data

Actual Unity PCM readback after import:
- attack-production: length .862 s / mono / 44.1 kHz / peak 1.0000 / RMS .1212
- hit-production: length .653 s / mono / 44.1 kHz / peak 1.0000 / RMS .0787
- defeat-production: length 1.254 s / mono / 44.1 kHz / peak .9999 / RMS .0545
- raise-production: length 2.064 s / mono / 44.1 kHz / peak 1.0000 / RMS .1895
- ally-contribution-production: length .758 s / mono / 44.1 kHz / peak 1.0000 / RMS .1640

Runtime gain contract:
- master .34
- attack .72
- hit .48
- defeat .82
- Raise 1.00
- allied contribution .38

The mix intentionally gives Raise the strongest signature gain and reduces repetitive hit/allied contribution gain to reduce polyphony fatigue.

The historical properties ReviewSfxPlaybackCount and LastReviewSfxName remain only as backward-compatible evidence hooks. Their names do not mean the bound assets are still review WAVs.

AWU-A1 verdict: PASS for generated production-candidate asset creation, provenance, Unity import, PCM readback and runtime binding. Final release legal/device/creative approval NOT RUN.

## TDD / implementation recovery

RED:
Artifacts/EV020-red.xml
- total 1
- passed 0
- failed 1
- expected failure: PlayerAttackSfx still pointed to Q3Review/attack-review.wav

During initial implementation Unity 6.3 rejected the obsolete AudioImporter.preloadAudioData API.
Root cause:
- Unity 6.3 moved preloadAudioData to AudioImporterSampleSettings.

Recovery:
- importer.defaultSampleSettings.preloadAudioData is now used.
- second Apply succeeded.
- no obsolete API workaround or suppressed compiler error was used.

GREEN:
Artifacts/EV020-green.xml
- total 1
- passed 1
- failed 0

## Final fresh regression evidence

Final targeted:
Artifacts/EV020-final2-targeted.xml
- FirstPlayableQ3VisualTests
- 10 / 10 PASS
- failed 0
- skipped 0
- duration 3.1309022 s

Final full PlayMode:
Artifacts/EV020-final2-full.xml
- 134 / 134 PASS
- failed 0
- skipped 0
- duration 9.1795397 s

These are fresh EV-020 results and do not reuse EV-019 9/9 or 133/133.

## Final fresh build / responsive runtime

Windows development build:
- FirstPlayableCanonicalScene.BuildVisualPlayer
- BUILD_EXIT=0
- VISUAL_PLAYER_BUILD Succeeded errors=0

Final responsive run:
20261006T021502740-aca28d191b1b4251a258ce6ae2c16f85

Build GUID:
c0d135821d294e40a69b4322965a8658

Executed:
- 390x844
- 768x1024
- 360x640
- active / eligible / raised / proof

360x640 fresh readback:
- actual=360x640
- overlayCount=1
- q3ProtectedUnitArt=PASS
- motionDurations=attack:.24, hit:.16, defeat:.50, Raise:.66, ally:.20
- production SFX names resolve to the five new production candidates
- runtime mix resolves to .34/.72/.48/.82/1.00/.38
- no text row reported overflow=True
- physicalDevice=NOT RUN

## Final fresh native normal-input runtime

Final native run:
20261006T021515798-046a6507cd104c93bdd10d69ae2f42e7

Input:
- NATIVE_INPUT_PASS=10 real OS clicks
- PLAYER_EXIT=0
- existing evidence-only foreground focus-retry driver
- product input logic unchanged

Final full-army runtime:
- soulBalance=19
- alliedCount=5
- visibleAllyArt=5
- overlayCount=1
- authoredMotion=True
- production audio clips=5
- runtime SFX playback count=51
- AudioListener count=1
- player attack cues=7
- hit cues=20
- defeat cues=6
- Raise cues=5
- allied contribution cues=13
- last contribution unit=ally:35
- q3ProtectedUnitArt=PASS
- no HUD row reported overflow=True

Dedicated motion frames:
- attack/hit: actual event cues and local Guard hit feedback captured.
- defeat: actual Guard scaleY=.774 mid-collapse.
- Raise: actual spectral emerald pull frame plus position delta from settled state.
- exact allied contribution: actor ally:11, only RaisedGuardSlot0 local scale=1.053 and emerald pulse captured.

Physical device remains NOT RUN.

## Figma

Static Q3 production canonical remains:
- 35:234 Battle
- 35:245 Raised
- 35:256 Full Formation
- 36:290 short Full Formation
- 36:305 provenance note

EV-020 did not change static screen composition, tokens, copy or layout, so no Figma write was required. EV-019 static Figma/runtime visual contract remains applicable.

## Final verdict

EV-020 bounded technical production-candidate motion/audio convergence: PASS.

Q3 overall: PARTIAL / IN_PROGRESS.

Still open before Q3 exit:
- Founder/human creative audition and acceptance of production-candidate motion/audio
- final rights/legal clearance
- final font release packaging/attribution audit
- representative physical mobile install
- actual mobile SafeArea
- physical-device audio mix
- physical-device accessibility acceptance
- real-user fun/play/retention evidence
- launch monster/content count

## Next recommended execution

Perform Q3 release-readiness evidence audit and prepare representative physical-mobile acceptance:
- audit per-file art/audio/font provenance and licensing evidence without pretending legal approval
- prepare a concise physical-device checklist for SafeArea, readability, motion, audio mix and accessibility
- keep final legal/creative/device approval as explicit human/user gates
- do not mark Q3 PASS until those gates and required user evidence exist.
