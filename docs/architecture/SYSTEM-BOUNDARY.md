# System Boundary — Client / Server

Status: DESIGN_CONTRACT
Lifecycle: 06-05
Baseline: ADR-0001 / CP-NECRO-114

## Trust zones

### Unity client (untrusted for authority)
May own presentation, input, animation, local first-playable simulation, cache and offline-first local persistence.

The client MUST NOT be the production authority for:
- secrets or privileged credentials,
- authentication/session issuance,
- authorization/RLS decisions,
- cloud canonical progression writes,
- payment/entitlement decisions,
- privileged admin/ops mutation,
- webhook signing/verification,
- security-sensitive remote configuration.

Current first-playable local state is a development/product capability, not evidence that production online authority belongs on-device.

### Server / trusted services (provider unselected)
When online/account/cloud features are activated, the trusted side owns identity verification, authorization, canonical remote writes, conflict policy, privileged integrations, secret material, audit records and webhook verification.

No backend vendor is selected by this contract.

### External providers
Auth, analytics, crash, payment, notification and other vendors remain behind adapters. Provider SDK objects MUST NOT become gameplay-domain truth.

## Boundary crossings

Every future remote mutation must carry an authenticated actor/session, authorization decision, version/schema identifier, validation result and observable error outcome. Retriable writes require an idempotency strategy before production activation.

## Figma quality-preservation invariant

Figma deferral changes sequencing only. It MUST NOT lower the final design-quality target.

Until paid Figma work resumes:
- temporary Unity UI values/assets MUST NOT become design canon,
- no Figma variable value or codeSyntax may be inferred,
- layout/presentation code should consume replaceable tokens/components rather than hard-coding a parallel design system,
- gameplay/domain/server contracts must not encode visual dimensions, colors, typography, icon geometry, animation timing or component state styling as domain truth,
- the canonical Figma file remains the future source for verified visual token/component mapping,
- after Figma resumes, verified tokens/components/states must be mapped, diffed and revalidated before visual implementation is considered complete.

Any pre-Figma engineering choice that would make later canonical Figma fidelity materially harder is a blocker, not an accepted shortcut.

## Current evidence boundary
Backend/auth/RLS/API/cloud sync/webhook provider: UNSELECTED.
Online production authority: NOT IMPLEMENTED.
Unity runnable build: NOT ESTABLISHED.
Physical device / AT / runtime performance: NOT RUN.
