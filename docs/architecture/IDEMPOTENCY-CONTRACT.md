# Idempotency Contract

Status: CORE SAFETY PRINCIPLE / Q4 IMPLEMENTATION DEFERRED

## Principle
Any retriable mutation that could duplicate durable value must have a stable effect identity and an authoritative dedupe/reconciliation path.

Likely targets:
- cloud save/progression commit,
- combat/progression reward if moved to trusted authority,
- purchase/entitlement grant,
- purchase webhook replay,
- restore/refund/revocation reconciliation,
- rewarded-ad reward when server-verified,
- destructive reset/delete.

Rules:
- same identity + same payload after completion => same logical result, no second effect.
- same identity + different payload => conflict.
- timeout is UNKNOWN until reconciled; never blindly create a second grant.
- duplicate store/webhook/ad completion must not duplicate value.

## Q4 implementation
Supabase is selected as the trusted backend where these rules will be implemented if the corresponding feature exists.
Exact table schema, transaction pattern, unique constraints, retention TTL and retry windows are intentionally NOT fixed yet.

## Current Q2 boundary
Local First Playable gameplay may use deterministic local guards/tests, but this document is not evidence of a server-side idempotency store.

Evidence boundary:
No Supabase idempotency table/function, payment webhook, sandbox purchase, refund replay or AdMob SSV has been executed.
