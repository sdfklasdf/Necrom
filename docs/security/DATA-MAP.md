# Data Map — Current + Future Lanes

Status: CURRENT Q2 MAP / REFRESH_BEFORE_Q4-Q5

## Current Q2 First Playable
Current gameplay data is local/runtime:
- local player key / local ownership context,
- progression and stage state,
- combat and RaiseSource state,
- owned undead / formation,
- local save snapshot where implemented.

No Supabase cloud authority, account/session server, payment, ads or production telemetry is active in Q2.

## Selected future lanes
Q4 Supabase:
- account/session identity,
- cloud save/profile/progression,
- trusted mutation/reconciliation records.
Exact fields, retention and deletion policy are defined when Q4 is implemented.

Q5 PostHog:
- selected analytics provider.
- exact event/property/PII schema is defined before SDK activation.
- no production event transmission exists now.

Q5 Sentry:
- selected crash/error provider.
- exact breadcrumbs/tags/user identifiers are minimized and reviewed before activation.

Q5 RevenueCat / Apple / Google:
- selected mobile purchase/entitlement path.
- exact transaction/entitlement data flow is mapped before sandbox integration.

Q5 AdMob:
- selected ads provider.
- consent/advertising identifiers and rewarded-ad verification flow are mapped before public use.

Toss Payments:
- OUT OF CURRENT SCOPE.

## Refresh rule
This map is not STALE merely because providers were selected.
Refresh it immediately before Q4/Q5 implementation when actual fields, endpoints, regions, retention and consent behavior can be grounded.

## Evidence boundary
No Q4/Q5 external data transmission is claimed. Actual privacy/legal review remains NOT RUN until the relevant implementation/release window.
