# ADR-0003 — Mobile monetization and payment routing

Status: APPROVED_BY_FOUNDER
Date: 2026-10-03
Scope: Necromancer mobile game monetization architecture

## Decision
Keep global launch capability open.

For mobile digital purchases:
- Unity remains the game client.
- RevenueCat is the selected purchase/entitlement integration layer.
- iOS purchase rail is Apple In-App Purchase / StoreKit.
- Android purchase rail is Google Play Billing.
- Store-specific purchase/receipt objects stay behind a purchase adapter and MUST NOT become gameplay-domain truth.

For web checkout:
- Toss Payments is the selected preferred web PG.
- Web checkout implementation remains DEFERRED until a web checkout surface is separately approved.

For Korea-specific mobile alternative billing:
- Toss-based external/alternative mobile billing is DEFERRED_OPTIMIZATION.
- It is not part of the baseline global mobile purchase path.
- It requires a separate future decision gate before implementation.

## Domain boundary
Gameplay may consume provider-neutral outcomes only, such as:
- purchase requested,
- purchase succeeded,
- purchase failed,
- purchase restored,
- entitlement active,
- entitlement revoked.

No Apple receipt type, Google billing object, RevenueCat SDK object, Toss SDK object, store transaction object, or vendor webhook payload may become Battle/Raise/Soul/Formation/domain truth.

## Trusted-side boundary
Privileged receipt validation, webhook verification, entitlement reconciliation, refund/revocation reconciliation, secret material and canonical remote grant/revoke decisions belong on trusted services when those services are implemented.

The current project has not selected a backend provider. This ADR selects monetization vendors and boundaries only; it does not prove backend/account/webhook implementation.

## Idempotency and money safety
Purchase grant, purchase replay, restore, refund/revocation reconciliation and provider webhook effects require idempotent handling before production activation. Unknown transport outcome must never be converted into a second grant by issuing a fresh effect identity blindly.

## Evidence boundary
SELECTED:
- RevenueCat for mobile purchase/entitlement integration.
- Apple IAP / StoreKit for iOS store billing.
- Google Play Billing for Android store billing.
- Toss Payments as preferred web PG.

NOT IMPLEMENTED / NOT RUN:
- RevenueCat project/account integration.
- Apple/Google production store products.
- Toss web checkout.
- payment backend/webhook/ledger.
- sandbox purchase/restore/refund tests.
- Korea-specific Toss mobile alternative billing.
- production monetary reconciliation.

## Rollback / replacement
Vendor adapters must remain replaceable. A future Founder-approved ADR may supersede RevenueCat or Toss without changing gameplay-domain contracts. Only direct vendor consumers become stale.
