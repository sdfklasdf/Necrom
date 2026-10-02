# System Boundary — Client / Trusted Backend

Status: CURRENT DESIGN CONTRACT
Mode: QUALITY_GAME
Baseline: ADR-0001 / ADR-0003 / ADR-0004 / ADR-0005

## Unity client
The Unity client may own:
- presentation and input,
- animation,
- current local First Playable simulation,
- local cache/offline data needed before Q4.

The client is not trusted for:
- privileged secrets,
- canonical remote account authority,
- payment/entitlement grants,
- server-verified ad rewards,
- privileged cloud progression writes,
- webhook verification,
- admin operations.

## Q4 trusted backend
Supabase is SELECTED_FOR_Q4 but NOT IMPLEMENTED.

Q4 is responsible for introducing the minimum trusted path needed for:
- Auth/session,
- cloud save and version/conflict metadata,
- privileged progression/economy mutations when needed,
- purchase webhook/entitlement reconciliation,
- AdMob SSV when the selected reward design requires it,
- audit/reconciliation records.

Exact schema, TTL and concurrency mechanisms are deferred until Q4 implementation.

## External providers
Q5 selections:
- RevenueCat + Apple IAP / Google Play Billing.
- Google AdMob.
- Sentry.
- PostHog.

Toss Payments is OUT OF CURRENT SCOPE.

All provider SDKs remain behind adapters and cannot become gameplay-domain truth.

## Figma / presentation
Final visual quality remains high, but Q2 playability is the current priority.
Presentation must remain replaceable so Q3 Vertical Slice can perform final-quality Figma/runtime alignment without rewriting gameplay truth.

## Evidence boundary
Current First Playable local/runtime evidence remains separate from future Q4/Q5 integrations.
Supabase and all Q5 vendor integrations are selected only; actual accounts/runtime integration are NOT IMPLEMENTED.
