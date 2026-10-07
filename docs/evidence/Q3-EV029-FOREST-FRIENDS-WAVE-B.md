# EV-029 — FOREST FRIENDS PRODUCTION WAVE B

Status: IMPLEMENTED / VERIFICATION_PENDING
Batch verification counter after this work: 4 / 10 AWU

Scope:
- AWU-3: CHR-002 솔방울 검객 production package
- AWU-4: CHR-004 클로버 우편부 production package

Figma:
- Family01 base board: 61:309
- CHR-002 base art: 61:337
- CHR-004 base art: 61:394
- Production states wave B: 66:309
- CHR-002 states: ATTACK / HIT / DEFEAT / RAISE
- CHR-004 states: ATTACK / HIT / DEFEAT / RAISE

Unity implementation:
- forest_chr_002.png added
- forest_chr_004.png added
- forest.chr002 binding added with PineSlash VFX identity
- forest.chr004 binding added with CloverWind VFX identity
- HUD display-name mapping added for 솔방울 검객 / 클로버 우편부
- canonical scene/prefab now serialize five Forest Friend visual bindings:
  CHR-001 / 002 / 003 / 004 / 006
- existing mechanics, balance, Raise, Formation, auto combat, wave and gate truth were not redesigned

Verification policy:
- QDEC-017/QDEC-018 cadence applies.
- No full targeted→full regression→Windows build/runtime capture suite was run for AWU-3/4.
- A lightweight Unity Apply/import/readback was executed because new PNG assets and serialized scene/prefab bindings had to be materialized.
- Unity script compilation succeeded during Apply.
- UPM connected normally after the known process-local environment variables were restored.
- Therefore this work is IMPLEMENTED / VERIFICATION_PENDING, not final PASS.

Scale boundary:
- Whole-game visual/content production remains a 1000+ to several-thousand reviewable-unit system.
- This two-AWU wave does not reduce or redefine that scale.
- 20–40 unit ranges remain bounded screen/character-package decomposition only.

Evidence boundaries:
- 120 final characters: NOT DONE
- CHR-002/004 final batch verification: PENDING
- final Founder creative approval: NOT RUN
- final rights/legal acceptance: NOT RUN
- physical device/A11Y/real-user evidence: NOT RUN
