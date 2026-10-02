# API Resource and Error Contract

Status: DESIGN_DRAFT_FOR_Q4
Backend selection: Supabase SELECTED_FOR_Q4 / NOT IMPLEMENTED

This remains a provider-neutral application contract. No endpoint is currently deployed.

Potential Q4 resource families:
- profile
- progression/cloud-save
- owned army/formation projection
- sync/reconciliation
- trusted economy/reward mutations when required

Common mutation concerns:
- authenticated actor/session,
- request/effect identity,
- schema/client version,
- expected revision when optimistic concurrency is used,
- explicit validation and conflict outcomes.

Core error semantics:
UNAUTHENTICATED, FORBIDDEN, VALIDATION_FAILED, CONFLICT, IDEMPOTENCY_CONFLICT, RATE_LIMITED, TEMPORARY_UNAVAILABLE, PERMANENT_FAILURE.

Implementation note:
Supabase transport may use database/RLS, RPC or trusted functions as appropriate in Q4. The domain must not depend on a specific transport shape.

Evidence boundary:
No Q4 API/RPC/function is claimed deployed by this draft.
