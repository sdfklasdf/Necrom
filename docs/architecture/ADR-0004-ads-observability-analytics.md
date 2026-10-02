# ADR-0004 — Ads, crash observability and product analytics

Status: APPROVED_BY_FOUNDER
Date: 2026-10-03
Scope: Necromancer mobile game vendor architecture

## Decision
The following providers are required for the commercial/public mobile product:

- Google AdMob = REQUIRED mobile advertising provider.
- Sentry = REQUIRED technical error/crash observability provider.
- PostHog = REQUIRED product analytics provider.

Mixpanel is not selected. PostHog is the single product-analytics source of truth unless a later ADR supersedes it.

## Separation of responsibilities
### Google AdMob
Owns advertising delivery/integration only.
Ad placement, ad frequency, rewarded-ad economy value and final monetization balance remain separate product/economy decisions.

### Sentry
Is the source of truth for technical errors, crashes, stack traces, release/build-linked reliability evidence and diagnostic breadcrumbs.

### PostHog
Is the source of truth for product analytics such as gameplay events, funnels, retention, cohorts, feature flags and experiments when those capabilities are actually implemented.

PostHog error-tracking capability, if available, MUST NOT become a second canonical error source while Sentry is the project error source of truth.

## Gameplay/domain boundary
AdMob, Sentry and PostHog SDK objects MUST NOT become gameplay-domain truth.

Analytics failure, crash-reporting failure or ad-provider failure must not mutate Battle/Raise/Soul/Formation truth except through an explicitly designed provider-neutral gameplay rule.

A rewarded-ad callback that grants an in-game reward is a money/economy-adjacent mutation and requires exactly-once/idempotent reward application.

## Analytics boundary
Existing event candidates remain design inputs, not proof of telemetry.

Event names, properties, identity rules, funnels and retention metrics must be finalized through the Lifecycle analytics steps before production measurement claims.

No synthetic event count, retention, eCPM, crash rate or monetization metric may be presented as measured production evidence.

## Privacy and third-party SDK boundary
Before production release, vendor data flows must be reconciled with the project data map, notice/consent/retention rules and actual build/network evidence where required by Lifecycle.

Client-safe configuration may ship in the client where the provider model requires it. Privileged/admin secrets must not be stored in source, committed config or gameplay code.

## Evidence boundary
SELECTED / REQUIRED:
- Google AdMob.
- Sentry.
- PostHog.

NOT IMPLEMENTED / NOT RUN:
- vendor accounts/projects readback.
- Unity SDK/package installation.
- production ad units.
- production analytics event transmission.
- Sentry test exception/crash ingestion.
- PostHog event ingestion/dashboard verification.
- physical-device ad/crash/analytics validation.
- measured eCPM, retention, conversion or crash-rate evidence.
