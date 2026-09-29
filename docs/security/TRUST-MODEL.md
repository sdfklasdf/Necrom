# Authentication / Authorization Trust Model

Status: DESIGN_CONTRACT
Lifecycle: 06-08

## Current state
Account/login/RLS/server authorization is NOT IMPLEMENTED. The first playable may use a local player identity only as a local ownership key.

## Production-capable trust rules
1. The Unity client is untrusted for authorization decisions.
2. A client-provided player/account/resource owner id is a claim, never proof.
3. When remote state exists, the trusted service resolves actor identity from a verified session and derives authorization server-side.
4. Row/resource ownership checks occur on the trusted side. RLS or equivalent policy is conditional on the selected backend, not assumed.
5. Admin/ops capabilities require a distinct privileged role and audited path; Founder governance does not automatically become a runtime superuser.
6. Secrets, service credentials, signing keys and webhook secrets never ship in the client.
7. Provider SDK success does not by itself mean a domain mutation is authorized.
8. Authorization failure is explicit and must not fall back to local success for canonical remote state.

## Role-bypass review
- PLAYER cannot mutate another player's canonical remote state by changing an id in a request.
- PLAYER cannot issue Founder/project approvals through gameplay APIs.
- OPS cannot bypass Founder-only governance decisions.
- External provider callbacks cannot mutate canonical state until signature/authenticity and replay/idempotency rules pass.
- Analytics/event ingestion cannot mutate gameplay truth.
- Local development/debug identities cannot be accepted as production authority.

## Figma quality isolation
Authentication state UI (signed-out, loading, denied, conflict, offline) remains a presentation contract to be completed against canonical Figma after paid work resumes. This trust model intentionally contains no visual values, so later high-fidelity Figma work is not constrained by temporary engineering UI.
