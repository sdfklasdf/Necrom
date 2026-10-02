# Idempotency Contract — Lifecycle 06-12

Status: DESIGN CONTRACT / provider-neutral.

## Key model
Mutation requests that can create irreversible or duplicate effects carry a stable idempotency/effect identity plus operation scope. A future authoritative service stores or reconciles the first terminal result and returns the same logical effect on safe replay.

Key identity = actor-scope + operation + idempotencyKey/effectIdentity.
Payload fingerprint mismatch for an existing key => IDEMPOTENCY_CONFLICT.

Vendor transaction/event IDs may participate in dedupe/reconciliation but do not replace the project-level operation boundary automatically.

## Required targets
- raise/consume source
- combat result commit and reward/progression application
- stage/boss clear commit
- save/sync mutation when sent remotely
- mobile purchase grant / entitlement activation
- purchase retry or replay after unknown transport outcome
- purchase restore reconciliation
- refund / revocation / chargeback reconciliation when applicable
- payment/provider webhook redelivery
- Toss web-payment effect when that deferred scope is later implemented
- rewarded-ad completion -> in-game reward grant
- destructive reset/delete if later specified

Pure reads do not require an idempotency key.

Analytics-event deduplication is a data-quality concern unless the event itself triggers a gameplay/money/reward effect. Sentry crash/error ingestion is observational and must not create domain effects.

## Retention
Exact time retention is UNSET until operation retry/offline windows and backend/provider behavior are selected and evidenced. Implementations must not invent a TTL. A provider/trusted service must prove its retention/reconciliation behavior covers the accepted replay window before production use.

## Replay and concurrency
- same key + same fingerprint + completed: return original logical result, no new effect.
- same key + same fingerprint + in progress: wait/poll or explicit IN_PROGRESS; do not execute a second mutation.
- same key + different fingerprint: IDEMPOTENCY_CONFLICT.
- different keys racing for one single-use resource: state/revision constraint permits one winner; loser receives CONFLICT.
- transport timeout is UNKNOWN outcome until replay/query resolves it; client must not assume failure and create a fresh effect key.
- duplicate purchase/provider webhook delivery must not grant the entitlement twice.
- duplicate rewarded-ad completion callbacks must not grant the gameplay reward twice.
- refund/revocation replay must not double-remove or corrupt an already reconciled entitlement/reward state.

## Counterexamples
1. Raise button is tapped twice with same key: one undead only.
2. Client times out after server committed combat reward then retries: original result returned, no second reward.
3. Two different keys attempt same raise source: one state commit wins, other conflicts.
4. Reused key with modified formation payload: conflict, never silently treated as replay.
5. Store/RevenueCat webhook redelivery: one entitlement/domain effect for one logical provider transaction/event.
6. Rewarded ad emits completion twice: one reward grant.
7. Purchase succeeds but client times out: reconciliation/restore resolves the existing purchase; a blind second grant is forbidden.
8. Refund/revocation redelivery: one reconciliation effect.

## Provider boundary
RevenueCat, Apple, Google, Toss and AdMob remain behind adapters. Provider SDK classes, callback objects and payloads do not become domain models.

## Figma quality impact
PASS: idempotency is below presentation. UI remains free to express pending/retry/conflict states with canonical components/tokens.

## Evidence boundary
The contract now includes approved payment/ad-reward targets, but no payment backend, idempotency store, provider webhook, sandbox purchase, restore/refund replay, rewarded-ad integration or exactly-once runtime test has run.
