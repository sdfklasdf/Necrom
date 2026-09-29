# Authentication / Session Risk Review — Lifecycle 07-05

Status: DESIGN_CONTRACT_WITH_DEFERRED_RUNTIME
Baseline: 06-08 TRUST-MODEL / current account-auth state NOT IMPLEMENTED

## Current applicability
The current first playable has no production account, login, remote session or auth provider. Therefore there is no implemented session token to penetration-test or revoke today. Future online capability remains open.

## Threat paths and required controls

| Threat path | Required control before activation | Current evidence |
|---|---|---|
| stolen access/session token | short-lived scoped credential strategy, secure platform storage, server-side expiry/revocation policy | NOT IMPLEMENTED |
| replayed request | idempotency/replay controls for effectful commands; nonce/signature where provider protocol requires | design contracts only |
| client-forged owner/player id | trusted service derives actor from verified session; never authorize from client owner id | TRUST-MODEL contract |
| debug identity accepted in production | environment-gated debug identity; production rejection | design contract only |
| session fixation / account switch state bleed | explicit session replacement, cache partition/clear, canonical actor re-resolution | NOT IMPLEMENTED |
| logout without revocation | server/provider revocation where supported plus local credential/cache clear | NOT IMPLEMENTED |
| provider callback spoof | signature/authenticity verification before domain command | integration/idempotency contracts only |

## Recovery semantics
Authentication cancellation, denial, expiry, offline state and conflict must be explicit application outcomes. They may not silently fall back to privileged/local success when canonical remote state exists.

## Runtime boundary
This review does not claim auth/session security has been executed. Provider selection, implementation, token storage inspection, revocation tests and replay tests are NOT RUN.

## DEC-037 Figma quality impact
PASS. Auth states are semantic outputs only. No temporary auth UI values or components become visual canon; canonical Figma retains full control of state variants, focus, reduced motion, responsive and accessibility treatment.
