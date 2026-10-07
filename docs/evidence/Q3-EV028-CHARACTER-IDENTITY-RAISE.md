# Q3 EV-028 — CHARACTER IDENTITY THROUGH RAISE + REPRESENTATIVE PRODUCTION STATES

Status: IMPLEMENTED / BATCH_VERIFICATION_PENDING

This commit preserves Forest Friend character identity through the existing gameplay path without redefining Raise, Formation, auto combat, wave, gate pressure or balance.

Implemented:
- canonical representative archetypes forest.chr001 / forest.chr003 / forest.chr006
- character visual bindings: ArchetypeId + Art + VFX identity + Accent
- enemy visual selection by archetype identity
- defeated archetype identity preserved through existing Raise command/domain path
- Formation ally visual uses the same character art identity when a binding exists
- CHR-001 VFX identity = LeafBarrier
- CHR-003 VFX identity = StarArrow
- CHR-006 VFX identity = DewHeal
- HUD target/defeat/Raise names derive from archetype display name
- runtime capture metadata records enemy/ally archetype, art and VFX identity
- Figma production-state board = 65:309 with ATTACK / HIT / DEFEAT / RAISE for CHR-001 / 003 / 006

Evidence truth at commit time:
- latest post-final-edit Q3 visual targeted = 12/12 PASS
- earlier same-turn canonical 8/8, full PlayMode 140/140, Windows build success/errors0/reopen22 and runtime capture were executed before the final HUD-name + capture-metadata edits
- therefore this commit is NOT promoted to final full-regression PASS
- under QDEC-017 it remains IMPLEMENTED / BATCH_VERIFICATION_PENDING until the scheduled ~10-AWU batch gate

Known environment note:
Remote batch Unity requires process-local HOME, TMP, PROGRAMDATA and ALLUSERSPROFILE to be restored so UPM can start reliably.

Evidence boundaries:
- 120 finished production characters: NOT DONE
- final Founder creative approval: NOT RUN
- final rights/legal acceptance: NOT RUN
- physical iOS/Android/SafeArea/audio/A11Y: NOT RUN
- real-user fun/retention: NOT RUN
