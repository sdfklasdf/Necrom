# EV-033 — FOREST FRIENDS PRODUCTION WAVE E

Status: BOUNDED TARGETED VERIFIED / FAMILY01 PACKAGE COVERAGE 10/10 / Q3 OVERALL PARTIAL
Verification class: FLEXIBLE_VERIFICATION × 2 (QDEC-019)
Date: 2026-10-08

Scope:
- CHR-009 씨앗 소환사 production package.
- CHR-010 반딧불 길잡이 production package.
- No gameplay mechanics, balance, save/load, global state, inventory, payment or persistence redesign.

Figma:
- Family01 base art CHR-009: 61:520
- Family01 base art CHR-010: 61:543
- Production state board Wave E: 70:309
- CHR-009 production section: 70:312
- CHR-010 production section: 70:421
- Both characters: ATTACK / HIT / DEFEAT / RAISE authored.
- CHR-009 original VFX identity: SeedlingCall.
- CHR-010 original VFX identity: FireflyTrail.
- Transparent exports retained from canonical Family01 art.

Unity implementation:
- forest_chr_009.png / forest.chr009 / SeedlingCall.
- forest_chr_010.png / forest.chr010 / FireflyTrail.
- Korean display names bound.
- scene/prefab canonical Forest Friend binding count: 10.
- roster canonical data updated.
- existing mechanics/balance unchanged.

AWU sequencing:
1. CHR-009 materialized first.
   - Unity Apply/import/compile/reopen: PASS.
   - canonical reopen components=22.
   - scene/prefab bindings=9.
   - alphaIsTransparency=1.
   - roster JSON valid.
   - Judgment before AWU2: IMPLEMENTED / VERIFICATION_PENDING.
2. CHR-010 materialized second.
   - Unity Apply/import/compile/reopen: PASS.
   - canonical reopen components=22.
   - scene/prefab bindings=10.
   - CHR-009/010 alphaIsTransparency=1.

Functional-closure targeted verification:
- FirstPlayableQ3VisualTests: 12/12 PASS.
- Test harness asserts exact canonical set forest.chr001 through forest.chr010.
- Roster JSON parses with 120 entries.
- CHR-009 and CHR-010 promoted to UNITY_PACKAGE_TARGETED_VERIFIED.

QDEC-019 verification decision:
- Both changes were limited-scope character visual/content packages, so FLEXIBLE_VERIFICATION applied.
- Relevant targeted verification ran at closure of the second flexible package.
- Full 140-test regression and Windows build/runtime were NOT repeated because these packages did not alter core gameplay/state architecture and EV-031 recently passed full PlayMode140/140 + Windows build + runtime Raise evidence.
- Unverified flexible-work accumulation after EV-033 = 0.

Evidence boundaries:
- Family01 current bounded production-package coverage: 10/10.
- This does NOT mean final release character production is fully complete: later animation polish, balance, device readability, A11Y and final creative/legal acceptance remain.
- Families02-12 / CHR-011 through CHR-120 production packages: NOT DONE.
- 120 final production characters: NOT DONE.
- Founder final creative acceptance: NOT RUN.
- Final rights/legal acceptance: NOT RUN.
- Physical device / A11Y / real-user evidence: NOT RUN.
