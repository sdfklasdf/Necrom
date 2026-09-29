# Migration & Rollback Plan — Lifecycle 06-17

Status: PARTIAL / DESIGN PLAN. No migration has been executed.

## Inputs
06-04 approved Unity/C# ADR; 06-07 data ownership; 06-09 API draft; 06-10 compatibility; 06-16 vendor/secret register.

## Universal sequence
1. EXPAND: introduce additive schema/contract capability that old readers tolerate.
2. MIGRATE: copy/transform with explicit source/target versions and verification.
3. MOVE CLIENTS: only compatible clients write/read the new representation.
4. CONTRACT: remove legacy path only after rollback window and compatibility evidence.

## Current first-playable persistence
- Existing implemented persistence schema: NOT ESTABLISHED.
- Therefore no destructive migration is authorized.
- Future save format must carry schemaVersion and preserve a readable previous known-good snapshot during migration.
- Failed validation leaves the previous snapshot CURRENT.
- Irreversible reset/delete remains TBD and cannot be introduced by migration code.

## Future remote/API migration
- additive fields before required semantic changes.
- dual-read/dual-write is allowed only with one canonical writer and reconciliation evidence.
- API major/version policy follows API-COMPATIBILITY.md.
- vendor migration must keep Domain free of vendor SDK types.

## Rollback points
- before new writes: rollback configuration/adapter.
- during expand/migrate: stop new writer; restore verified prior snapshot/backup.
- after client move but before contract: route compatible clients back if old representation remains valid.
- after irreversible contract/destructive data transformation: rollback may be impossible; requires explicit Founder/risk approval and backup/restore evidence before execution.

## Observation required before contract
migration counts, validation failures, conflict/idempotency errors, restore verification and compatibility outcomes. Numeric thresholds remain UNSET until actual baseline/operational context exists.

## Figma quality impact
PASS: data/API migration does not freeze presentation. Visual/token/component migration remains governed separately by DEC-037 and the Figma resume gate.

## Evidence boundary
No database, backend, backup, restore, save migration, client migration, vendor migration, or rollback rehearsal has run. 06-16 is PARTIAL, so this plan cannot be promoted to execution-ready.
