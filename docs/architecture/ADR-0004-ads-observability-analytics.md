# ADR-0004 — Ads, crash observability and product analytics

Status: SELECTED_FOR_Q5 / NOT IMPLEMENTED
Date: 2026-10-03
Mode: QUALITY_GAME

## Decision
The following providers are selected for Q5 Monetization & Telemetry:
- Google AdMob — mobile ads.
- Sentry — technical error/crash source of truth.
- PostHog — product analytics source of truth.

Mixpanel is not selected.

## Separation
AdMob owns ad delivery only. Placement, frequency and reward value remain game-economy decisions.
Sentry owns crash/exception/release-linked technical reliability evidence.
PostHog owns gameplay/product events, funnels, retention, cohorts and experiments.

Do not maintain two canonical crash systems. PostHog error features, if enabled later, are secondary to Sentry.

## Sequencing
These SDKs are not Q2 First Playable blockers.
Install them in Q5 after Q4 backend/online foundation is selected and minimally operational.
Before public release, verify test/production separation and actual runtime ingestion/transmission.

## Reward safety
A rewarded-ad completion that changes gameplay value must pass through a provider-neutral exactly-once reward boundary. Q4 trusted backend may be used for server-side verification when the ad design requires it.

## Evidence boundary
SELECTED_FOR_Q5:
- AdMob.
- Sentry.
- PostHog.

NOT IMPLEMENTED / NOT RUN:
- vendor accounts/projects.
- Unity packages.
- ad units.
- Sentry test exception/crash ingestion.
- PostHog event ingestion/dashboard.
- rewarded-ad runtime/SSV.
- physical-device validation.
- measured eCPM, retention, conversion or crash rate.
