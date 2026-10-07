# EV-032 — FOREST FRIEND CHR-008 PRODUCTION PACKAGE

Status: BOUNDED TARGETED VERIFIED / Q3 OVERALL PARTIAL
Verification class: FLEXIBLE_VERIFICATION (QDEC-019)
Date: 2026-10-08

Scope:
- CHR-008 나무껍질 수호자 production package only.
- No gameplay mechanics, balance, save/load, global state, inventory, payment or persistence redesign.

Figma:
- Family01 base art: 61:496
- Production state board Wave D: 69:309
- CHR-008 production section: 69:312
- ATTACK / HIT / DEFEAT / RAISE authored.
- Original VFX identity: GrowthRingBarrier.
- Visual motif: bark shield + amber growth-ring barrier + mint spirit reform.
- Transparent export source retained from canonical Family01 art.

Unity implementation:
- forest_chr_008.png added with transparent import.
- archetype: forest.chr008
- display name: 나무껍질 수호자
- VFX enum/binding: GrowthRingBarrier
- scene/prefab canonical Forest Friend binding count: 8
- roster canonical data updated.
- existing mechanics/balance unchanged.

Acceptance / targeted verification:
- Unity Apply/import: SUCCESS.
- Script compile: SUCCESS.
- Canonical scene/prefab reopen: PASS, components=22.
- scene bindings: 8.
- prefab bindings: 8.
- forest_chr_008 alphaIsTransparency=1.
- Roster JSON parse: PASS.
- FirstPlayableQ3VisualTests: 12/12 PASS.
- Test harness now asserts exact canonical eight-character binding set.

QDEC-019 verification decision:
- This is limited-scope character visual/content work, so FLEXIBLE_VERIFICATION applies.
- Targeted Q3 visual validation was run at functional closure.
- Full 140-test regression and Windows build/runtime were NOT repeated after CHR-008 because the immediately preceding EV-031 catch-up verification already passed full PlayMode140/140 + Windows build + runtime Raise evidence, and CHR-008 did not alter core gameplay/state architecture.
- Unverified flexible-work accumulation after EV-032 = 0.

Evidence boundaries:
- CHR-008 production package bounded scope: VERIFIED.
- CHR-009 / CHR-010 production packages: NOT DONE.
- 120 final production characters: NOT DONE.
- Founder final creative acceptance: NOT RUN.
- Final rights/legal acceptance: NOT RUN.
- Physical device / A11Y / real-user evidence: NOT RUN.
