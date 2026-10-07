# Q3 FOREST FRIENDS FAMILY 01 — PRODUCTION PIPELINE

Date: 2026-10-07
Baseline: CP-NECRO-V2-021 / EV-026
Scope: 2 AWU max. Character-content production only; existing battle/Raise/Formation/wave/gate mechanics are preserved.

## AWU-1 — 10-character family kit
PASS for design-kit scope.
- Figma board: 61:309.
- Ten distinct original chibi characters:
  - CHR-001 도토리 방패병 — Tank / COMMON / acorn shield / leaf barrier.
  - CHR-002 솔방울 검객 — Melee / RARE / twig sword / star slash.
  - CHR-003 민들레 궁수 — Ranged / RARE / dandelion bow / star arrow.
  - CHR-004 클로버 우편부 — Support / EPIC / mailbag / clover wind.
  - CHR-005 밤톨 마법사 — Mage / EPIC / chestnut wand / violet star magic.
  - CHR-006 이슬 치료사 — Heal / RARE / dew bottle / dew heal.
  - CHR-007 딸기 폭탄꾼 — Burst / EPIC / berry bomb / berry explosion.
  - CHR-008 나무껍질 수호자 — Defense / LEGEND / bark shield / ring wall.
  - CHR-009 씨앗 소환사 — Summon / EPIC / seed pods / seed summon.
  - CHR-010 반딧불 길잡이 — Utility / LEGEND / firefly lantern / firefly path.
- Shared rig is allowed only as a production accelerator; silhouette/head accessory, face, prop, palette and VFX identity are distinct.
- 120-roster canonical JSON updated for CHR-001..010.
- The remaining 110 slots remain planned, not falsely marked complete.

## AWU-2 — representative asset pipeline
PASS for representative Windows runtime scope.
- Figma export nodes:
  - CHR-001: 61:313 → forest_chr_001.png
  - CHR-003: 61:366 → forest_chr_003.png
  - CHR-006: 61:445 → forest_chr_006.png
- SHA256:
  - CHR-001: eee634e1afd85ed034346a0a1eab7741afc19823d95180ace7ae0f7b8bea72ea
  - CHR-003: 787d6a35a3d8d6352751e27e10131256b635a9eadde8cfa572b364a11d85a711
  - CHR-006: d553944421c455d722084cacbe39352e4fda73289634a5dda72ebc8bdf2840fb
- Runtime now binds a three-texture ForestFriendEnemyArts family set.
- Canonical enemy ordinal selects the family texture deterministically without changing enemy domain truth.
- Actual visual path verified:
  - canonical threat 1 → forest_chr_001 / 도토리 방패병
  - canonical threat 2 → forest_chr_003 / 민들레 궁수
- Raised ally still uses the existing verified generic spirit-friend art because current domain data does not yet persist a character-archetype identity through Raise. No false per-character Raise mapping was fabricated.

## Verification
- Q3 visual targeted: 11/11 PASS.
- Full PlayMode: 139/139 PASS.
- Windows canonical build: Success / errors=0 / reopen components=22.
- Actual Windows capture run: 20261007T134809769-1ee6eba87d0541a7915df306fa8f2846.
- requested=actual:
  - 390x844
  - 768x1024
  - 360x640
- capture audit: overflow=True NONE / =FAIL NONE.
- 390x844 readback visibly shows CHR-001 first, then CHR-003 as the next active threat.

## Scale boundary
This does NOT mean character production is almost done.
The project target is 120 characters. Each character still needs later production decomposition for final art, animation states, hit/defeat/Raise representation, skill VFX, data/balance, responsive/device readability and final creative acceptance.
The whole game's visual/content workload therefore remains a 1000+ unit class project and can expand into several thousand reviewable detail units. A 20–40 unit count applies to bounded screens/character packages, not the whole game.

## Evidence boundary
- 120 finished production characters: NOT DONE.
- Per-character Raised identity persistence: NOT IMPLEMENTED.
- Founder final creative acceptance: NOT RUN.
- Final rights/legal acceptance: NOT RUN.
- Physical iOS/Android/SafeArea/audio/A11Y: NOT RUN.
- Real-user fun/retention: NOT RUN.
