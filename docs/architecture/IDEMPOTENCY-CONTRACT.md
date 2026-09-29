# Idempotency Contract — Lifecycle 06-12

Status: DESIGN CONTRACT / provider-neutral.

## Key model
Mutation requests that can create irreversible or duplicate effects carry a caller-generated idempotencyKey plus operation scope. Future authoritative service stores the first terminal result for the key and returns that same logical result on safe replay.

Key identity = actor-scope + operation + idempotencyKey.
Payload fingerprint mismatch for an existing key => IDEMPOTENCY_CONFLICT.

## Required targets
- raise/consume source
- combat result commit and reward/progression application
- stage/boss clear commit
- save/sync mutation when sent remotely
- future purchase/reservation/webhook mutations if those features are later approved
- destructive reset/delete if later specified

Pure reads do not require an idempotency key.

## Retention
Exact time retention is UNSET until operation retry/offline windows and backend are selected. Implementations must not invent a TTL. A provider must prove its retention covers the maximum accepted replay window before production use.

## Replay and concurrency
- same key + same fingerprint + completed: return original logical result, no new effect.
- same key + same fingerprint + in progress: wait/poll or explicit IN_PROGRESS; do not execute a second mutation.
- same key + different fingerprint: IDEMPOTENCY_CONFLICT.
- different keys racing for one single-use resource: state/revision constraint permits one winner; loser receives CONFLICT.
- transport timeout is UNKNOWN outcome until replay/query resolves it; client must not assume failure and create a fresh effect key.

## Counterexamples
1. Raise button is tapped twice with same key: one undead only.
2. Client times out after server committed combat reward then retries: original result returned, no second reward.
3. Two different keys attempt same raise source: one state commit wins, other conflicts.
4. Reused key with modified formation payload: conflict, never silently treated as replay.
5. Future webhook redelivery: one domain mutation for one provider event identity.

## Figma quality impact
PASS: idempotency is below presentation. UI remains free to express pending/retry/conflict states with future canonical components/tokens.

## Evidence boundary
No backend, idempotency store, TTL, payment, webhook, or runtime test exists yet.
