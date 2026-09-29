# State Transition & Concurrency Contract — Lifecycle 06-11

Status: DESIGN CONTRACT / no runtime implementation claimed.

## Inputs
03-08 domain state model; 06-07 data ownership; 06-09 API contract; 06-10 compatibility policy.

## Global rules
- A command has one canonical writer.
- Every mutation evaluates preconditions against one observed revision and commits all-or-nothing.
- Repeated or concurrent commands must not create duplicate gameplay effects.
- Presentation may request a command, but it never owns domain truth.
- Local-first execution may serialize commands in one application command queue; future remote authority must use expectedRevision plus idempotency at the authoritative writer.
- Conflict is explicit; last-write-wins is forbidden for ownership, progression, formation, raise, rewards, or destructive state.

## State transitions
| Aggregate | Allowed transition | Atomic effect | Conflict rule |
|---|---|---|---|
| Enemy | SPAWNED -> ENGAGED -> DEFEATED -> RAISE_ELIGIBLE or RESOLVED | defeat produces at most one raise source | second defeat/result for same resolved enemy is no-op/conflict |
| Raise source | AVAILABLE -> SELECTED -> CONSUMED or EXPIRED | consume source + create one undead + ownership record | only one consume wins |
| Undead | ACQUIRED -> ACTIVE/RESERVE -> GROWING -> EVOLVED | state + progression revision | stale revision rejected |
| Formation | EMPTY/PARTIAL/READY <-> edited; READY -> IN_COMBAT | slot set changes as one revision | concurrent edits require expectedRevision |
| Combat | READY -> RUNNING -> VICTORY/DEFEAT -> RESOLVED | result committed once | duplicate result cannot double reward/progression |
| Skill | READY -> CAST/ACTIVE -> COOLDOWN -> READY | resource/cooldown transition atomic | duplicate input during non-READY has no second effect |
| Region/Stage | LOCKED -> AVAILABLE -> IN_PROGRESS -> CLEARED | clear/progression unlock once | stale/duplicate clear has no extra reward |
| Save | NONE -> CREATED -> CURRENT -> UPDATED | snapshot replace only after complete valid write | failed write cannot become CURRENT |

Unspecified reset/delete/removal/recovery transitions remain TBD and MUST NOT be invented by implementation.

## Counterexamples required by Lifecycle
1. Two Raise commands for one AVAILABLE source arrive together: exactly one may consume it and create one undead; the other receives replay/conflict and creates nothing.
2. Double tap skill while first cast is committing: one transition to CAST/COOLDOWN; second command cannot spend resource twice.
3. Formation edit A and B both start from revision 8: first commit creates revision 9; second must conflict/reload rather than overwrite revision 9.
4. Combat result retry after timeout: same logical result cannot grant reward, stage clear, or progression twice.
5. Save write interrupted before verification: previous CURRENT snapshot remains authoritative; partial snapshot is never reported as success.
6. Old client sends removed/changed semantics: compatibility layer rejects explicitly; it cannot reinterpret into a different mutation.

## Figma quality impact
PASS: contract exposes semantic states/conflict/loading/error outcomes without prescribing colors, spacing, typography, motion, assets, component geometry, or token values. Canonical Figma can later represent every state without domain rewrite.

## Evidence boundary
No lock implementation, database transaction, runtime queue, test, build, server, or device behavior is claimed.
