# API Resource and Error Contract

Status: DESIGN_DRAFT
Lifecycle: 06-09
Provider: UNSELECTED

This is a provider-neutral contract. No endpoint is claimed deployed.

## Resource families
- /v1/profile — remote player/profile projection when accounts are activated.
- /v1/progression — canonical remote progression snapshot when cloud authority is activated.
- /v1/army — owned undead and formation commands/projection.
- /v1/combat/results — trusted result submission/validation boundary if combat authority or anti-abuse requirements later require it.
- /v1/events — append-only accepted event envelope; never gameplay truth.
- /v1/sync — versioned reconciliation boundary for local/offline to remote transition.

## Common request envelope
- schemaVersion
- clientVersion
- requestId
- idempotencyKey for retriable mutations
- expectedRevision for optimistic concurrency where applicable
- payload

Actor identity is derived from the trusted session when online auth exists; it is not accepted from payload as authority.

## Common success envelope
- requestId
- schemaVersion
- revision when a canonical resource changes
- payload

## Error contract
| Code | Meaning | Client behavior |
|---|---|---|
| UNAUTHENTICATED | no valid trusted session | enter auth/recovery state; do not commit remote success locally |
| FORBIDDEN | actor lacks resource/action permission | show bounded denial; no retry loop |
| VALIDATION_FAILED | payload violates contract | preserve safe local state; surface actionable field/domain error |
| CONFLICT | expected revision is stale | fetch/reconcile; never blind overwrite |
| IDEMPOTENCY_CONFLICT | key reused with different mutation | stop and reconcile |
| RATE_LIMITED | trusted service throttled | bounded backoff using server guidance |
| TEMPORARY_UNAVAILABLE | retryable dependency failure | bounded retry/offline path |
| PERMANENT_FAILURE | non-retryable service failure | fail explicitly; retain recovery evidence |

## Figma handoff invariant
API error codes are semantic states, not visual designs. Final loading/error/conflict/offline presentation remains governed by the canonical Figma component/state system after Figma resumes. Engineering must expose all semantic states without fixing their final visual treatment now.
