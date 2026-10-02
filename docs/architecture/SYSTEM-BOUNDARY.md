# System Boundary — Client / Server

Status: DESIGN_CONTRACT
Lifecycle: 06-05
Baseline: ADR-0001 / ADR-0002 / ADR-0003

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

For monetization, the trusted side must ultimately own privileged receipt/webhook validation, entitlement reconciliation and authoritative remote grant/revoke decisions when those paths are implemented.

No backend vendor is selected by this contract.

### External providers
External providers remain behind adapters. Provider SDK objects MUST NOT become gameplay-domain truth.

Selected project roles:
- RevenueCat: mobile purchase/entitlement integration layer over store billing.
- Apple IAP / StoreKit: iOS store purchase rail.
- Google Play Billing: Android store purchase rail.
- Toss Payments: preferred web PG; web checkout is not yet implemented.
- Google AdMob: required mobile advertising provider.
- Sentry: required technical error/crash source of truth.
- PostHog: required product-analytics source of truth.

Korea-specific Toss mobile alternative billing is deferred and is not part of the baseline global mobile purchase path.

## Boundary crossings
Every future remote mutation must carry an authenticated actor/session where applicable, authorization decision, version/schema identifier, validation result and observable error outcome. Retriable writes require an idempotency strategy before production activation.

Purchase grant, restore, refund/revocation, webhook replay and rewarded-ad reward application are duplicate-effect-sensitive and must follow the idempotency contract.

Analytics, crash and advertising telemetry are not domain authority. Their outage or retry behavior must not silently create gameplay effects.

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
Vendor architecture selection: APPROVED for RevenueCat, Apple/Google store billing, Toss web PG, AdMob, Sentry and PostHog.
Vendor accounts/projects/SDK integrations: NOT ESTABLISHED / NOT IMPLEMENTED unless separately evidenced elsewhere.
Backend/auth/RLS/API/cloud sync/webhook provider: UNSELECTED.
Online production authority: NOT IMPLEMENTED.
Physical device / AT / vendor runtime transmission evidence: NOT RUN.
