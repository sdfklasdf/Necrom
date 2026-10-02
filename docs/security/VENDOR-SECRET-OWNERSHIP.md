# Vendor & Secret Ownership Register — Lifecycle 06-16

Status: PARTIAL / PROVIDERS SELECTED / NO SECRET MATERIAL.

This register records provider decisions and evidenced ownership boundaries. It contains no tokens, passwords, signing keys or secret values.

| Capability | Vendor/account state | Owner | Secret storage/rotation |
|---|---|---|---|
| Source repository | GitHub sdfklasdf/Necrom — connector read/write evidenced | Founder | connector/OAuth managed externally; secret values not stored here |
| Project canon | Google Drive project board — read/write evidenced | Founder | connector/OAuth managed externally |
| Game engine/editor | Unity 6.3 LTS family selected | Founder | actual privileged account/signing material remains outside repository |
| Backend/auth/database | UNSELECTED / NOT IMPLEMENTED | Founder | TBD; client-side privileged storage forbidden |
| Mobile IAP integration | RevenueCat SELECTED; account/project not established here | Founder | client-safe SDK configuration may ship when required; privileged/admin credentials remain trusted-side and rotatable |
| iOS billing/store | Apple IAP / StoreKit SELECTED; products/signing/account not established here | Founder | signing keys, App Store Connect credentials and privileged store credentials never enter repository/client source |
| Android billing/store | Google Play Billing SELECTED; products/signing/account not established here | Founder | signing/private service credentials never enter repository/client source |
| Web PG | Toss Payments SELECTED_PREFERRED; integration deferred | Founder | client-safe values may be exposed only as documented; secret/payment-server credentials remain trusted-side and rotatable |
| Mobile advertising | Google AdMob REQUIRED; account/app/ad units not established here | Founder | client-safe app/ad-unit identifiers may ship; privileged account credentials remain external |
| Technical error/crash | Sentry REQUIRED; project/runtime ingestion not established here | Founder | client-safe SDK configuration may ship where appropriate; org/admin tokens remain privileged and external |
| Product analytics | PostHog REQUIRED; project/runtime ingestion not established here | Founder | client-safe project configuration may ship where appropriate; admin/personal credentials remain privileged and external |
| Mixpanel | NOT SELECTED | Founder | no credential path |
| Push/search/CDN/media | NOT REQUIRED current first playable / UNSELECTED | Founder | TBD if introduced |
| Figma | canonical file exists; current access evidence tracked separately | Founder | connector-managed; no secret values in repo |

## Ownership rules
- The Founder is the current executive, development and operations owner for project-level provider decisions and actions.
- Lifecycle role labels remain responsibility categories and do not imply a separate person must be contacted.
- Operational secret injection/rotation scope must be explicit before a provider is production-enabled.
- Client receives only client-safe identifiers/configuration; privileged keys remain trusted-side.
- Rotation must support overlap/revocation without requiring gameplay-domain rewrite.
- Suspected exposure => revoke/rotate first, then investigate; never preserve a compromised credential for compatibility.

## Provider-specific authority boundary
- RevenueCat/store state may inform entitlement reconciliation but SDK/vendor objects do not become gameplay-domain truth.
- Toss payment-server secrets never belong in Unity client code.
- AdMob callbacks that lead to rewards pass through a provider-neutral exactly-once reward boundary.
- Sentry is technical error/crash source of truth.
- PostHog is product-analytics source of truth; it is not a second crash source of truth.

## Figma quality impact
PASS: vendor/secret ownership is isolated from presentation. No visual implementation or token value is locked here.

## Evidence boundary
Lifecycle 06-16 remains PARTIAL. Provider selection is real, but this file is not evidence of vendor account creation, store product creation, secret-store configuration, key rotation, SDK installation, runtime transmission, sandbox payment success or production release readiness.
