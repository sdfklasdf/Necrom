# EV-036 — CLOUD KNIGHTS CHR-012 + CHR-013 PRODUCTION WAVE B

Status: BOUNDED TARGETED VERIFIED / Q3 OVERALL PARTIAL
Date: 2026-10-08
Verification class: FLEXIBLE_VERIFICATION × 2

Scope:
- CHR-012 번개깃 창병 production package.
- CHR-013 빗방울 석궁수 production package.
- Reuse the verified CharacterVisuals multi-family seam.
- No gameplay mechanics, balance, save/load, domain state or serialization architecture redesign.

Figma:
- Family02 canonical board: 72:309.
- CHR-012 base export: 72:337.
- CHR-013 base export: 72:366.
- Production state Wave B: 74:309.
- CHR-012 section: 74:312.
- CHR-013 section: 74:415.
- Both characters have ATTACK / HIT / DEFEAT / RAISE.
- CHR-012 VFX identity: BoltLance.
- CHR-013 VFX identity: Rainbolt.

Unity:
- cloud_chr_012.png / cloud.chr012 / BoltLance.
- cloud_chr_013.png / cloud.chr013 / Rainbolt.
- Korean display names bound.
- CharacterVisuals reused without another serialized migration.
- scene/prefab character binding count: 13.
- CHR-012 and CHR-013 alphaIsTransparency=1.
- canonical reopen PASS components=22.
- roster JSON 120 entries valid.

AWU sequencing:
1. CHR-012 materialized first.
   - scene/prefab bindings=12.
   - alpha transparency=1.
   - roster runtime identity readback valid.
   - judgment: IMPLEMENTED / VERIFICATION_PENDING.
2. CHR-013 materialized second.
   - scene/prefab bindings=13.
   - alpha transparency=1.
   - roster runtime identity readback valid.

Functional-closure validation:
- FirstPlayableQ3VisualTests: 12/12 PASS.
- Exact binding set now forest.chr001~010 + cloud.chr011~013.
- CHR-012 and CHR-013 promoted to UNITY_PACKAGE_TARGETED_VERIFIED.
- Full PlayMode was not repeated because both AWUs were content-only additions on the already full-regression-verified CharacterVisuals seam from EV-035.

Evidence boundary:
- Family02 runtime production coverage: CHR-011~013 verified, CHR-014~020 NOT DONE.
- 120 final production characters: NOT DONE.
- Founder final creative acceptance: NOT RUN.
- Final rights/legal acceptance: NOT RUN.
- Physical device / A11Y / real-user evidence: NOT RUN.
