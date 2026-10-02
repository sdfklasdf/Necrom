# External Integration Boundary

Status: CURRENT APPLICABILITY SUMMARY

Current Q2 First Playable does not require external search, push notifications, remote media, payment, ads, crash telemetry or product analytics to complete its core gameplay loop.

Q4:
- Supabase is selected for backend/Auth/cloud-save/trusted integration foundation.

Q5:
- RevenueCat + Apple/Google store billing.
- AdMob.
- Sentry.
- PostHog.

Search, push, CDN/media and other providers remain unselected until a real product requirement appears.

Shared rules:
- no privileged secrets in client.
- provider objects do not enter gameplay Domain.
- outages degrade the integration rather than silently changing gameplay truth.
- duplicate-value mutations use idempotent/trusted handling when implemented.

Evidence boundary:
Provider selection is not runtime integration. Q4/Q5 integrations are NOT RUN.
