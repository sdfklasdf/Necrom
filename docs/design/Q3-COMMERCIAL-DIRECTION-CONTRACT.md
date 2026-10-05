# Q3 Commercial Direction Contract — EV-017

Date: 2026-10-05 KST
Project: Necromancer / QUALITY_GAME
Original design baseline: EV-016 / main 50a3d2851b365ff76d6c5d1c87bc70f44caee2f3
Implementation follow-up: EV-018 integrates the approved Obsidian Soul production-candidate pack and revalidates runtime.
Milestone: Q3 VERTICAL SLICE IN_PROGRESS
Verdict: FOUNDER_APPROVED. Direction A — OBSIDIAN SOUL, Noto functional UI baseline, and imagegen refinement production-art path were explicitly approved by the Founder on 2026-10-05.

## Scope

EV-017 itself was a design-only narrowing step. After Founder approval, EV-018 changes art bindings, scene/prefab serialized texture references, the Q3 art-applier path, and one PlayMode assertion that verifies the approved ProductionCandidate path. Gameplay state, balance, semantic-state count and content count are unchanged.
EV-018 therefore runs fresh targeted/full PlayMode, Windows build, responsive captures and normal-path native input instead of reusing EV-016 as current-head evidence.

Guard remains one archetype repeated across five Formation slots.
Five Formation slots are not five monster species.
Three regular plus one boss remains only a proposal; launch content count stays UNDECIDED.

## Figma canonical additions

File: eXqKU1qHXsn52SJfIGltZo

- Commercial direction review board: 33:177
- Production presentation contract: 33:274
- Existing full-Formation reference: 18:84
- Existing 360x640 readability frame: 31:158
- Existing motion/audio review note: 31:174

The two new frames are review/decision artifacts, not runtime evidence and not final shipped art.

## Direction convergence

### A — OBSIDIAN SOUL — RECOMMENDED

Intent:
- preserve the validated gothic cemetery identity;
- reserve emerald for Soul / Raised / positive necromancy power;
- reserve crimson for hostile / defeat danger;
- keep violet as secondary Necromancer / Arcane identity;
- lower environment saturation so unit silhouettes and state feedback dominate;
- make the growing undead Formation the recognizable brand signature rather than generic dark-fantasy ornament.

Heuristic assessment:
- current asset fit: HIGH
- mobile semantic separation: HIGH
- implementation risk: LOW
- differentiation potential: MEDIUM-HIGH

Primary risk:
If the Soul emerald and army silhouette language are not consistently reinforced, the product can still read as generic dark fantasy.

### B — BONE RELIQUARY — ALTERNATIVE

Intent:
- bone / ivory / ash material emphasis;
- restrained magic;
- stronger physical relic and armor language.

Heuristic assessment:
- current asset fit: MEDIUM
- mobile semantic separation: HIGH
- implementation risk: MEDIUM
- differentiation potential: MEDIUM

Primary risk:
May weaken the current emerald Raise identity and drift toward familiar dark-RPG visual language.

### C — ARCANE VIOLET — ALTERNATIVE

Intent:
- stronger violet magical lighting and fog;
- more explicit arcane glow and spell fantasy.

Heuristic assessment:
- current asset fit: MEDIUM-HIGH
- mobile semantic separation: MEDIUM
- implementation risk: MEDIUM
- differentiation potential: MEDIUM-HIGH

Primary risk:
Violet overuse can collide with Soul emerald and make UI/gameplay semantics less immediately readable.

## Recommendation

Direction A — OBSIDIAN SOUL is Founder-approved.

Reason:
It preserves the most already-validated work while giving the project a clearer product identity around collecting Souls and growing an undead Formation. It also minimizes semantic conflict between Soul, hostile and Arcane feedback.

The original narrowing was an AI design recommendation, not user research. The Founder subsequently approved Direction A explicitly; this still does not constitute real-user preference evidence.

## Production typography contract — functional baseline approved

Functional UI baseline:
- Noto Sans KR / Noto Sans CJK KR family.
- Functional weights: Regular / Medium / Bold.
- Current repository contains the SIL OFL 1.1 license text for the bundled Noto CJK source.
- No custom display face should be introduced until rights, Korean legibility and small-screen performance are verified.

Current status:
- Noto functional family/3-weight strategy: FOUNDER_APPROVED
- license source text exists: VERIFIED_ARTIFACT
- current runtime still uses the existing Regular review SDF; Medium/Bold runtime packaging: NOT IMPLEMENTED
- final packaging / attribution review: NOT RUN

## Production copy contract — proposed

Tone:
- short, direct dark-fantasy Korean;
- state label first, result/action second, explanatory line last;
- no diagnostic copy, English placeholders or implementation terminology in player-facing UI.

Recommended full-capacity wording:
- Primary: 군단이 가득 찼습니다
- Secondary: 다음 전투에서 군단의 힘을 확인하세요
- Disabled CTA: 군단 최대

These are candidate production strings; final copy approval is NOT RUN.

## Icon and semantic-color contract — proposed

Icon:
- 20 / 24 px functional sizes;
- single-color silhouette plus one state accent;
- avoid ornamental detail below 16 px;
- no decorative skull/bone repetition without semantic purpose.

Semantic color:
- Soul / Raised / positive resource: emerald #087F5B
- Arcane / Necromancer identity: violet #6741D9
- Hostile / defeat danger: crimson #C92A2A
- Army / neutral info: blue #1864AB
- core surfaces: #0B0E14 / #202532

State must never rely on color alone; label/icon/shape redundancy is required.

## Production motion contract — proposed

Target timing ranges:
- attack anticipation + strike: 140–180 ms
- hit confirmation: 90–120 ms
- defeat settle: 280–360 ms
- Raise transformation: 380–480 ms
- allied contribution: 180–240 ms

Motion language:
- attack = forward snap + fast settle
- hit = local impact; minimize whole-screen shake
- defeat = weighted collapse; no gore dependency
- Raise = upward Soul pull → emerald reform
- allied contribution = exact-unit pulse; must not read as the Necromancer's own attack

The current five authored review curves remain implementation evidence, not final rig/frame animation.

## Production audio contract — proposed

Palette:
- dry bone / stone / cloth transient;
- low spectral Soul layer;
- player attack and hit cues short and non-melodic;
- Raise gets the strongest recognizable Soul signature;
- allied-contribution cue stays small and positional to avoid fatigue with five allies.

Priority:
1. Raise / defeat confirmation
2. player hit feedback
3. allied contribution
4. ambient / UI texture

Mix constraints:
- combat readability over spectacle;
- repeated allies need polyphony control;
- mobile-speaker, earphone and loudness acceptance are NOT RUN.

The current deterministic five review WAV files remain event-wiring evidence only and are not production audio.

## Rights and provenance boundary

Verified artifacts:
- Noto CJK OFL 1.1 license text is present in the repository.
- Q3 review SFX are deterministic local synthesis with no third-party samples.
- Q3 review art has OpenAI image-generation provenance and original prompts recorded.

Not yet cleared:
- review art release-rights final clearance: UNKNOWN
- final font packaging / attribution review: NOT RUN
- production icon source / license: NOT PRODUCED
- production audio source / license: NOT PRODUCED
- final per-file release asset manifest: NOT RUN

No legal-clearance PASS is claimed.

## Founder decision gate — resolved 2026-10-05

Founder explicitly approved:
1. Direction A — OBSIDIAN SOUL.
2. Noto Sans KR / Noto Sans CJK KR as the functional UI baseline.
3. Image-generation refinement with per-file provenance as the production-art path.

Current status:
- COMMERCIAL DIRECTION = FOUNDER_APPROVED
- FUNCTIONAL FONT BASELINE = FOUNDER_APPROVED
- PRODUCTION ART PATH = FOUNDER_APPROVED
- production-candidate asset integration = IMPLEMENTED / VALIDATED in EV-018
- final copy/icon/rights/production audio/physical-device/user evidence remain open
- Q3 overall = PARTIAL / IN_PROGRESS

## Q3 exit boundary unchanged

Still required before Q3 PASS:
- Founder-approved commercial visual direction
- final production font/copy/icon
- complete rights/provenance acceptance
- production motion/audio asset and mix acceptance
- representative physical mobile install
- actual mobile SafeArea
- accessibility acceptance
- actual user fun/play evidence

## Actual Work Units

### Q3-FD1 — Commercial direction narrowing
Input: EV-016 validated review candidate, current HUD/state semantics, existing art provenance.
Work: compare three bounded directions and create Figma review board.
Output: Figma 33:177.
Done: one recommended option with explicit alternatives/risks and no fake user evidence.
Evidence boundary: DESIGNED / DOCUMENTED, Founder approval NOT RUN.
Verdict: PASS.

### Q3-FD2 — Production presentation contract
Input: Direction A recommendation, bundled Noto license text, current review motion/audio and semantic-color system.
Work: define typography/copy/icon/color/motion/audio/rights contract and Founder gate.
Output: Figma 33:274 and this document.
Done: production rules and open rights/device/user evidence are explicit.
Evidence boundary: DESIGNED / DOCUMENTED, production assets and final legal/device acceptance NOT RUN.
Verdict: PASS.

## Next action after Founder decision

If Direction A is approved:
- produce a bounded production-art refinement pack for Necromancer + Guard + Raised Guard + cemetery only;
- do not expand monster count yet;
- update Figma canonical final-direction frames;
- integrate only approved assets;
- then run affected targeted → relevant full regression → build/visual/device validation.

If Direction A is not approved:
- revise only the selected direction before generating production assets.
