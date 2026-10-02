# Environment & Configuration Contract — Lifecycle 06-15

Status: PARTIAL / DESIGN CONTRACT ONLY.

## Dependency status
Provider architecture is now selected for mobile IAP, web PG, ads, crash reporting and product analytics, but actual vendor accounts/projects and production environment separation are not established by this document.

Backend/auth/database remains unselected.

## Logical environments
| Environment | Purpose | Credentials | External endpoints | Data |
|---|---|---|---|---|
| local-dev | developer/local first playable | no production secrets | local/fake/test adapters by default | disposable local data |
| test | automated/controlled verification | dedicated test-only injection | sandbox/test endpoints and test identifiers | disposable fixtures |
| staging | future integration validation | staging-only injected secrets/config | staging/sandbox vendor projects or explicitly isolated configuration | non-production |
| production | released service | production secret manager/injection only | production vendor endpoints/projects | production authority |

## Provider configuration separation
### RevenueCat / Apple / Google billing
- Development/sandbox purchase configuration MUST NOT silently fall back to production products.
- Production product identifiers and entitlement mapping require explicit environment/release configuration.
- Privileged store/vendor credentials never live in Unity source or committed config.

### Toss Payments
- Test and live payment credentials/configuration must be separated.
- Web checkout remains deferred; no live payment default may exist before that scope is approved.

### Google AdMob
- Development/test must use test-safe ad configuration.
- Production ad unit configuration must be an explicit release configuration, not an implicit fallback.

### Sentry
- Environment, release/build and project configuration must distinguish development/test/staging/production evidence.
- A test error in a non-production environment must not be reported as production reliability evidence.

### PostHog
- Development/test events must be separable from production analytics.
- Test fixtures and synthetic analytics must not pollute production metrics or be reported as real user behavior.

## General rules
- Environment selection is configuration, never a visual/Figma concern.
- Secrets, signing keys, service-role credentials and webhook verification secrets never live in client source, repository, Figma or committed config.
- Provider URLs/limits/features are adapter configuration and must not enter Domain.
- No production default may silently fall back to test/local.
- No test/local default may silently write to production.
- Feature flags that change domain semantics require version/compatibility review.
- Provider outage/degraded behavior must not mutate gameplay truth except through an explicitly designed provider-neutral rule.
- Exact account/project IDs, keys, limits and production configuration remain UNSET until actual provider setup/readback.

## Failure behavior
Missing required secret/config => fail closed with explicit configuration error.
Unknown environment => startup/integration blocked; never assume production.
Staging/production credential mix => security failure; no fallback.
Unavailable optional integration => apply its documented degraded behavior without mutating gameplay truth.

## Figma quality impact
PASS: configuration boundary is presentation-independent and preserves canonical Figma freedom.

## Evidence boundary
Logical separation is specified. Actual vendor accounts/projects, secret stores, staging/production projects, purchase products, ad units, analytics projects, crash projects and environment-isolation tests are NOT RUN / NOT ESTABLISHED by this document.
