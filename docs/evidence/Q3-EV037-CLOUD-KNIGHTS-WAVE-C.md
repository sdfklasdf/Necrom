# EV-037 — CLOUD KNIGHTS CHR-014 + CHR-015 PRODUCTION WAVE C

Status: BOUNDED TARGETED VERIFIED / Q3 OVERALL PARTIAL
Date: 2026-10-08
Verification class: FLEXIBLE_VERIFICATION × 2

Scope:
- CHR-014 바람종 전령 production package.
- CHR-015 노을 마도기사 production package.
- Reuse the verified CharacterVisuals multi-family seam.
- No gameplay mechanics, balance, save/load, domain state or serialization architecture redesign.

Figma:
- Family02 canonical board: 72:309.
- CHR-014 base export: 72:394.
- CHR-015 base export: 72:414.
- Production state Wave C: 75:309.
- CHR-014 section: 75:312.
- CHR-015 section: 75:441.
- Both characters have ATTACK / HIT / DEFEAT / RAISE.
- CHR-014 VFX identity: BellGust.
- CHR-015 VFX identity: SunsetSigil.

Unity:
- cloud_chr_014.png / cloud.chr014 / BellGust.
- cloud_chr_015.png / cloud.chr015 / SunsetSigil.
- Korean display names bound.
- CharacterVisuals reused without another serialized migration.
- scene/prefab character binding count: 15.
- CHR-014 and CHR-015 alphaIsTransparency=1.
- canonical reopen PASS components=22.
- roster JSON 120 entries valid.

AWU sequencing:
1. CHR-014 materialized first.
   - scene/prefab bindings=14.
   - alpha transparency=1.
   - roster runtime identity readback valid.
   - judgment: IMPLEMENTED / VERIFICATION_PENDING.
2. CHR-015 materialized second.
   - scene/prefab bindings=15.
   - alpha transparency=1.
   - roster runtime identity readback valid.

Functional-closure validation:
- FirstPlayableQ3VisualTests: 12/12 PASS.
- Exact binding set now forest.chr001~010 + cloud.chr011~015.
- CHR-014 and CHR-015 promoted to UNITY_PACKAGE_TARGETED_VERIFIED.
- Full PlayMode was not repeated because both AWUs were content-only additions on the already full-regression-verified CharacterVisuals seam from EV-035.

Evidence boundary:
- Family02 runtime production coverage: CHR-011~015 verified, CHR-016~020 NOT DONE.
- 120 final production characters: NOT DONE.
- Long-term meta progression/retention design required by QDEC-020: NOT YET EXECUTED.
- Final game name required by QDEC-021: NOT SELECTED; '네크로맨서 키우기' is working title only.
- Founder final creative acceptance: NOT RUN.
- Final rights/legal acceptance: NOT RUN.
- Physical device / A11Y / real-user evidence: NOT RUN.
