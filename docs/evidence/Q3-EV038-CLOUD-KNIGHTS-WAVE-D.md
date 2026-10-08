# EV-038 — CLOUD KNIGHTS CHR-016 + CHR-017 PRODUCTION WAVE D

Status: BOUNDED TARGETED VERIFIED / Q3 OVERALL PARTIAL
Date: 2026-10-08
Verification class: FLEXIBLE_VERIFICATION × 2

Scope:
- CHR-016 새벽안개 치유사 production package.
- CHR-017 우박 공병 production package.
- Reuse the verified CharacterVisuals multi-family seam.
- No gameplay mechanics, balance, save/load, domain state or serialization architecture redesign.

Figma:
- Family02 canonical board: 72:309.
- CHR-016 base export: 72:445.
- CHR-017 base export: 72:467.
- Production state Wave D: 76:309.
- CHR-016 section: 76:312.
- CHR-017 section: 76:439.
- Both characters have ATTACK / HIT / DEFEAT / RAISE.
- CHR-016 VFX identity: MistMend.
- CHR-017 VFX identity: HailPop.

Unity:
- cloud_chr_016.png / cloud.chr016 / MistMend.
- cloud_chr_017.png / cloud.chr017 / HailPop.
- Korean display names bound.
- CharacterVisuals reused without another serialized migration.
- scene/prefab character binding count: 17.
- CHR-016 and CHR-017 alphaIsTransparency=1.
- canonical reopen PASS components=22.
- roster JSON 120 entries valid.

AWU sequencing:
1. CHR-016 materialized first.
   - First Apply attempt hit UPM IPC environment failure before serialization change.
   - That failed run was NOT counted as success.
   - Retry with explicit HOME/TEMP/TMP/PROGRAMDATA/ALLUSERSPROFILE succeeded.
   - scene/prefab bindings=16.
   - alpha transparency=1.
   - judgment: IMPLEMENTED / VERIFICATION_PENDING.
2. CHR-017 materialized second.
   - scene/prefab bindings=17.
   - alpha transparency=1.
   - roster runtime identity readback valid.

Functional-closure validation:
- FirstPlayableQ3VisualTests: 12/12 PASS.
- Exact binding set now forest.chr001~010 + cloud.chr011~017.
- CHR-016 and CHR-017 promoted to UNITY_PACKAGE_TARGETED_VERIFIED.
- Full PlayMode was not repeated because both AWUs were content-only additions on the EV-035 full-regression-verified CharacterVisuals seam.

Evidence boundary:
- Family02 runtime production coverage: CHR-011~017 verified, CHR-018~020 NOT DONE.
- 120 final production characters: NOT DONE.
- Long-term meta progression/retention design required by QDEC-020: NOT YET EXECUTED.
- Final game name required by QDEC-021: NOT SELECTED; current working title remains internal only.
- Founder final creative acceptance: NOT RUN.
- Final rights/legal acceptance: NOT RUN.
- Physical device / A11Y / real-user evidence: NOT RUN.
