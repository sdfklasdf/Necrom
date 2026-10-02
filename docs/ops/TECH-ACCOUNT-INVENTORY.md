# Technology Account Inventory — Lifecycle 00-10 Revalidation

Checkpoint target: next project checkpoint after CP-NECRO-164
Date: 2026-10-03
Status: PARTIAL — provider selection is separated from actual account/access evidence.
No secret/token/password material is stored here.

| Capability | Project identifier / scope | Owner | Actual access evidence | Status |
|---|---|---|---|---|
| Source control | GitHub `sdfklasdf/Necrom`, branch `main` | Founder | connected GitHub read/write evidenced in project history | VERIFIED_READ_WRITE |
| Canon / project board | Google Drive spreadsheet `1zLhQQ3pTgjLAkuNMSDNLQtFYu2hS3Nsz5O5O2kPGYnw` | Founder | metadata/cell reads and project-board writes/readbacks evidenced | VERIFIED_READ_WRITE |
| Design | Figma canonical file `eXqKU1qHXsn52SJfIGltZo` | Founder | separate project evidence governs current access state | SELECTED_CANON / ACCOUNT_STATE_SEPARATE |
| Engine/editor | Unity 6.3 LTS family + C# approved architecture | Founder | separate project evidence governs installed editor/toolchain state | SELECTED |
| Backend/auth/database | none selected for Necrom | Founder | no provider selected | UNSELECTED / NOT IMPLEMENTED |
| Mobile IAP integration | RevenueCat | Founder | provider selected by ADR-0003; no account/project readback performed by this update | SELECTED / ACCOUNT_NOT_ESTABLISHED_HERE |
| iOS billing/distribution | Apple IAP / StoreKit + App Store Connect | Founder | purchase rail selected; store account/products/signing not read back by this update | SELECTED_RAIL / ACCOUNT_NOT_ESTABLISHED_HERE |
| Android billing/distribution | Google Play Billing + Google Play | Founder | purchase rail selected; store account/products/signing not read back by this update | SELECTED_RAIL / ACCOUNT_NOT_ESTABLISHED_HERE |
| Web PG | Toss Payments | Founder | preferred web PG selected; no web checkout/account readback performed by this update | SELECTED_PREFERRED / INTEGRATION_DEFERRED |
| Mobile advertising | Google AdMob | Founder | required provider selected; no account/app/ad-unit readback performed by this update | SELECTED_REQUIRED / ACCOUNT_NOT_ESTABLISHED_HERE |
| Technical error/crash | Sentry | Founder | required provider selected; no project/DSN/runtime-ingestion readback performed by this update | SELECTED_REQUIRED / ACCOUNT_NOT_ESTABLISHED_HERE |
| Product analytics | PostHog | Founder | required provider selected; no project/key/event-ingestion readback performed by this update | SELECTED_REQUIRED / ACCOUNT_NOT_ESTABLISHED_HERE |
| Mixpanel | not selected | Founder | no project usage intended while PostHog is analytics source of truth | NOT_SELECTED |
| Search/push/CDN/media | not required for current first playable / unselected | Founder | no provider selected | NOT_REQUIRED_CURRENT_FP / UNSELECTED |

## Single-person operating context
The Founder currently performs executive, development and operational responsibilities for this project. Lifecycle role labels remain responsibility categories; they are not instructions to hand work to a separate person.

## 00-10 decision
Lifecycle 00-10 remains PARTIAL. Provider selection does not prove account ownership, access, key storage, test/staging/production separation, store products or runtime integration.

## Downstream revalidation
- 06-15 remains PARTIAL until actual environment separation for selected vendors is established and read back.
- 06-16 remains PARTIAL until actual vendor accounts, privileged secret storage and rotation/revocation paths are evidenced.
- 06-18 remains NOT READY for measured project cost ceilings until real pricing/usage assumptions are grounded.
- 08 analytics design can now use PostHog as its selected implementation target but event transmission remains NOT RUN.
- 10-23 third-party SDK transmission verification becomes APPLICABLE once the SDKs are actually in a build.
- 11 payment/integration track becomes APPLICABLE for the approved monetization scope; account/sandbox evidence remains pending.
- 13-09 technical crash/log collection has Sentry as the selected implementation target but ingestion remains NOT RUN.

## DEC-037 Figma quality impact
PASS. Vendor choices do not fix visual values, assets, tokens, component geometry, motion, focus behavior or accessibility shortcuts.
