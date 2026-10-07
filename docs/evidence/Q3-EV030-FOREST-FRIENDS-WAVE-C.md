# EV-030 — FOREST FRIENDS PRODUCTION WAVE C

Status: IMPLEMENTED / VERIFICATION_PENDING
Batch verification counter after this work: 6 / 10 AWU

Scope:
- AWU-5: CHR-005 밤톨 마법사 production package
- AWU-6: CHR-007 딸기 폭탄꾼 production package

Figma:
- Family01 base board: 61:309
- CHR-005 base art: 61:414
- CHR-007 base art: 61:467
- Production states wave C: 67:309
- CHR-005 production section: 67:312
- CHR-007 production section: 67:433
- Both characters have ATTACK / HIT / DEFEAT / RAISE states.
- CHR-005 unique VFX identity: ChestnutStar
- CHR-007 unique VFX identity: BerryBurst

Unity implementation:
- forest_chr_005.png and generated Unity meta added.
- forest_chr_007.png and generated Unity meta added.
- forest.chr005 binding added with ChestnutStar VFX identity.
- forest.chr007 binding added with BerryBurst VFX identity.
- HUD display-name mapping added for 밤톨 마법사 / 딸기 폭탄꾼.
- canonical scene/prefab now serialize seven Forest Friend visual bindings:
  CHR-001 / 002 / 003 / 004 / 005 / 006 / 007
- existing mechanics, balance, Raise, Formation, auto combat, wave and gate truth were not redesigned.

Lightweight verification:
- QDEC-017/QDEC-018 cadence applies.
- Unity 6000.3.25f1 batch Apply/import completed for each AWU materialization.
- Unity script compilation succeeded.
- canonical scene/prefab reopen readback succeeded with components=22.
- forest.chr005 and forest.chr007 are serialized in both scene and prefab.
- unrelated pre-existing PNG importer reserialization noise was reverted before commit.
- No targeted full regression, Windows build, runtime capture, responsive capture, physical-device test, A11Y test or real-user test was run.

Scale and evidence boundaries:
- Whole-game visual/content production remains a 1000+ to several-thousand reviewable-unit system.
- 20–40 unit ranges remain bounded screen/character-package decomposition only.
- 120 final production characters: NOT DONE.
- CHR-005/007 batch verification: PENDING.
- final Founder creative approval: NOT RUN.
- final rights/legal acceptance: NOT RUN.
- physical device/A11Y/real-user evidence: NOT RUN.
- Next scheduled full verification remains AWU-10 unless an exception condition arises.
