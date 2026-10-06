# Q3 Release-Readiness Evidence Audit ??EV-021

Date: 2026-10-06 KST
Project: Necromancer / QUALITY_GAME
Input: CP-NECRO-V2-015 / EV-018 / EV-019 / EV-020 / main e469eacad56ed7683ebaa36cb0ed08e401841c77
Milestone: Q3 VERTICAL SLICE IN_PROGRESS
Verdict: PASS for evidence-integrity audit and human/device acceptance preparation. Q3 overall remains PARTIAL / IN_PROGRESS.

## Evidence boundary

This audit proves that the checked production-candidate files still match their recorded provenance and that the human/device acceptance procedure is prepared.

It does not prove:
- final release-rights or legal clearance;
- Founder/human creative acceptance;
- representative physical-device install, SafeArea, audio mix or accessibility;
- real-user fun, play or retention;
- launch monster/content count.

Those remain UNKNOWN / NOT RUN / UNDECIDED as applicable.

The Founder has also changed the gameplay direction to an idle-RPG + light-defense hybrid. EV-018/019/020 remain valid for their bounded art/UI/motion/audio evidence, but the previous single-duel runtime is no longer sufficient by itself to close the representative Q3 gameplay exit.

## Fresh baseline readback

- GitHub repository: sdfklasdf/Necrom
- local HEAD = origin/main = remote main = e469eacad56ed7683ebaa36cb0ed08e401841c77 before EV-021 work
- initial working tree: clean
- Unity project version: 6000.3.25f1
- Figma production canonical exists:
  - 35:234 Battle
  - 35:245 Raised
  - 35:256 Full Formation
  - 36:290 360x640 Full Formation
  - 36:305 provenance note
- Formation five slots still repeat one Raised Guard archetype. Five slots are not five species.

## Per-file provenance / SHA audit

Art ??all actual SHA-256 values match Assets/Necrom/FirstPlayable/Art/Q3/ProductionCandidate/provenance.json:
- player.png ??A7F6FE76CEB0021858F596C230C02B24274D303110CF03A79F27B6FAA6DF7FBC ??generation id present ??PASS
- guard.png ??387D3070F00ABB0DB2350AE6ACAC57B4F8A7FB24828E61E609B0893CF4EED591 ??generation id present ??PASS
- raised.png ??AFCA05119E17010E62663379AB1C686E9A2D9FAEC0E84598AFF5F5155A2D23EE ??generation id present ??PASS
- background.png ??4447EDD20ACDD581EAA519A24D3B41FB83D513CA30AE071FA54D1D1F6817C101 ??generation id present ??PASS

Audio ??all actual SHA-256 values match Assets/Necrom/FirstPlayable/Audio/Q3ProductionCandidate/PROVENANCE.json; each entry also has a generator task id and source prompt:
- attack-production.mp3 ??9438FF79E2F749EA422A1199974ADB71DB5605B027863E216B30A3E4FAB260C0 ??PASS
- hit-production.mp3 ??B3D3A86D82E118149C756B5569999F02A9E7A2B613464B49B138A080C377D694 ??PASS
- defeat-production.mp3 ??1F261212A342AFB1504E33BE05CE79C3DB16FDA451FD541E3C915C93CDA4294B ??PASS
- raise-production.mp3 ??D517EF7A2399BF1FA7F3B40E1537F515443D5FC2859092F18CF54315D626B3A7 ??PASS
- ally-contribution-production.mp3 ??1B86FE91A132B5058C18E1DA6D53D47069D956D115BAEAD5587C72F806CAFAB9 ??PASS

Fonts ??actual SHA-256 matches EV-019:
- NotoSansKR-Regular.otf ??69975A0AC8472717870AEFEAB0A4D52739308D90856B9955313B2AD5E0148D68 ??PASS
- NotoSansKR-Medium.otf ??B46988EF13E8BAC08F3933AF686EAF770972994F9B6D335BE0184D60169B5431 ??PASS
- NotoSansKR-Bold.otf ??5A6CEB287ED2FC6CFC6213144EBEA68CBD94B20FC9EB873D8486493BF02D9BDA ??PASS
- Regular/Medium/Bold TMP SDF assets exist ??PASS
- Assets/Necrom/FirstPlayable/Fonts/OFL-CJK.txt exists and identifies SIL Open Font License 1.1 ??PASS as license-evidence presence
- final font release packaging/attribution acceptance ??NOT RUN

Automated audit:
- scripts/q3-release-readiness-audit.ps1
- output: docs/evidence/artifacts/Q3/EV021-release-readiness/windows-audit.txt
- result: PASS_EVIDENCE_INTEGRITY_ONLY
- legal approval asserted by script: NO

## EV-018 / EV-019 / EV-020 vs Q3 exit

- production-candidate art creation/provenance/integration ??PASS (EV-018)
- Noto 3-weight runtime packaging + semantic icons + "援곕떒 理쒕?" ??PASS (EV-019)
- production-candidate motion/audio generation, provenance, import, runtime playback ??PASS (EV-020)
- EV-020 targeted PlayMode 10/10 ??PASS, fresh artifact still present and parseable
- EV-020 full PlayMode 134/134 ??PASS, fresh artifact still present and parseable
- prior Windows build / responsive / native desktop input evidence ??PASS for EV-020 bounded desktop scope
- Founder final creative audition ??NOT RUN
- final visual/audio/font release-rights/legal clearance ??UNKNOWN / NOT RUN
- representative physical mobile install / SafeArea ??NOT RUN
- speaker/headphone physical mix ??NOT RUN
- physical accessibility acceptance ??NOT RUN
- real-user fun/play/retention ??NOT RUN
- launch monster/content count ??UNDECIDED
- representative idle-defense hybrid runtime ??PARTIAL: domain foundation exists in EV-021; runtime integration not yet implemented

## Founder production motion/audio audition checklist

Use the current Windows build only to judge the existing production-candidate media. Reconfirm final pacing after the idle-defense runtime is integrated.

1. Attack: readable anticipation and snap; not floaty or over-cinematic.
2. Hit: local feedback is clear without whole-screen shake; repeated hits are not tiring.
3. Defeat: collapse reads as weighted defeat and does not look like a scale glitch.
4. Raise: emerald soul-pull/reform is the strongest signature moment and is understandable without text.
5. Exact allied contribution: the actual contributing Raised Guard is identifiable; pulse is local and not noisy.
6. Audio identity: Attack / Hit / Defeat / Raise / Ally are distinguishable.
7. Mix: Raise is signature-forward; Hit and Ally remain low-fatigue under repetition.
8. Obsidian Soul fit: motion/audio support the dark obsidian/emerald/crimson/violet direction.
9. Decision record: ACCEPT / REJECT / ACCEPT WITH CHANGES plus one-line reason per rejected item.

Final Founder creative acceptance remains NOT RUN until the Founder actually performs this audition.

## Representative physical-device acceptance checklist

Record device model, OS version, build/commit, orientation and evidence timestamp before testing.

- install succeeds from the actual mobile artifact;
- cold launch and relaunch succeed;
- portrait orientation is correct;
- actual SafeArea avoids notch/cutout/home-indicator overlap;
- 360-class and representative device text remains readable;
- Attack motion is readable at device scale;
- Hit motion is visible but not excessive;
- Defeat collapse remains legible;
- Raise spectral emerald pull/reform remains legible;
- exact ally contribution identifies only the real contributing ally;
- speaker mix: five cue families are distinguishable, Raise signature is present, repeated Hit/Ally is not fatiguing;
- earphone/headphone mix: no painful peak, imbalance or excessive fatigue;
- polyphony under full formation does not turn into noise;
- accessibility: contrast, text, touch target and reduced-motion needs are actually reviewed;
- normal touch input succeeds without evidence-only desktop helpers;
- no visible hitch/stall during normal combat/raise flow;
- Figma/runtime fidelity is compared on the same representative state;
- idle-defense hybrid-specific wave/gate HUD is included once that runtime exists.

No physical-device item may be marked PASS from desktop evidence.

## Evidence storage contract

Root:
docs/evidence/artifacts/Q3/EV021-release-readiness/

Recommended evidence names:
- founder-audition/decision.md
- founder-audition/attack.mp4
- founder-audition/hit.mp4
- founder-audition/defeat.mp4
- founder-audition/raise.mp4
- founder-audition/ally-contribution.mp4
- physical-device/<device-model>/device.txt
- physical-device/<device-model>/install-launch.mp4
- physical-device/<device-model>/safearea.png
- physical-device/<device-model>/combat-motion.mp4
- physical-device/<device-model>/speaker-notes.md
- physical-device/<device-model>/earphone-notes.md
- physical-device/<device-model>/accessibility.md
- physical-device/<device-model>/input-performance.md
- physical-device/<device-model>/figma-runtime.md

## Windows/Unity release-readiness execution

Evidence-integrity audit executed successfully with failure count 0.

A fresh Unity EditMode run was attempted after the hybrid domain file was added:
- normal package path: INVALID RUN / BLOCKED because Unity Package Manager IPC failed to start after 30 s;
- -noUpm recovery: INVALID RUN because disabling UPM removes existing TMP/UI/NUnit package references, causing unrelated existing compile errors;
- therefore neither run is counted as product PASS or FAIL.

A package-independent .NET 10 harness compiled and executed the new pure-domain seam:
- result: EV021_DOMAIN_HARNESS_PASS
- wave clear, gate damage/failure, next-wave sequencing and revision behavior executed.
- this is evidence for the pure C# domain seam only; Unity integration remains NOT RUN until a valid Unity package/test run succeeds.

## Q3 verdict after EV-021

EV-021 release-readiness evidence integrity/preparation: PASS.
EV-021 idle-defense domain foundation: PASS at pure-domain level; Unity integration PARTIAL / NOT RUN.
Q3 overall: PARTIAL / IN_PROGRESS.

The new Founder gameplay direction increases the Q3 gameplay exit requirement: the final representative slice must demonstrate the idle-RPG + light-defense loop rather than only the historical single-enemy duel.
