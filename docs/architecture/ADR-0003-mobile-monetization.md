# ADR-0003 — Mobile monetization

Status: SELECTED_FOR_Q5 / NOT IMPLEMENTED
Date: 2026-10-03
Mode: QUALITY_GAME

## Decision
Global mobile launch remains the baseline.

Mobile digital purchases:
- RevenueCat is the selected purchase/entitlement integration layer for Q5 Monetization & Telemetry.
- iOS uses Apple In-App Purchase / StoreKit.
- Android uses Google Play Billing.
- Unity consumes provider-neutral purchase/entitlement outcomes through an adapter.

Toss Payments and Korea-specific alternative billing are OUT OF CURRENT SCOPE.
They are not part of the initial mobile launch architecture. A future web-commerce or regional-billing project may evaluate them through a separate decision.

## Sequencing
Do not integrate mobile payments during the current Q2 First Playable.
Q4 Backend / Online Foundation comes first so trusted webhook, entitlement reconciliation, ledger/idempotency and cloud authority have an actual home.
Q5 then installs and verifies RevenueCat/store integrations.

## Domain boundary
Apple receipts, Google billing objects, RevenueCat SDK objects and webhook payloads MUST NOT become Battle/Raise/Soul/Formation domain models.

## Evidence boundary
SELECTED:
- RevenueCat for Q5.
- Apple IAP / StoreKit as iOS purchase rail.
- Google Play Billing as Android purchase rail.

NOT IMPLEMENTED / NOT RUN:
- RevenueCat account/project.
- store products.
- Unity RevenueCat SDK.
- Q4 trusted payment webhook/ledger.
- sandbox purchase/restore/refund/revocation.
- production transactions.

Vendor selection is not release readiness.
