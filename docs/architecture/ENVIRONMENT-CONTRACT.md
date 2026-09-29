# Environment & Configuration Contract — Lifecycle 06-15

Status: PARTIAL / DESIGN CONTRACT ONLY.

## Dependency status
00-10 technology account inventory is PARTIAL. GitHub and Google Drive access are evidenced; backend, store, analytics and other production service accounts are not established. Therefore this document defines separation rules but MUST NOT be treated as proof that real dev/test/staging/prod environments exist.

## Logical environments
| Environment | Purpose | Credentials | External endpoints | Data |
|---|---|---|---|---|
| local-dev | developer/local first playable | no production secrets | local/fake adapters by default | disposable local data |
| test | automated/controlled verification when toolchain exists | dedicated test-only injection | fake/test endpoints | disposable fixtures |
| staging | future integration validation | staging-only injected secrets | staging services only | non-production |
| production | released service | production secret manager/injection only | production services | production authority |

## Rules
- Environment selection is configuration, never a visual/Figma concern.
- Secrets, signing keys, service-role credentials and webhook verification secrets never live in client source, repository, Figma, or committed config.
- Provider URLs/limits/features are adapter configuration and must not enter Domain.
- No production default may silently fall back to test/local.
- Feature flags that change domain semantics require version/compatibility review; visual-only flags cannot become a substitute for canonical Figma.
- Backend/provider is UNSELECTED, so concrete connection strings, URLs, project IDs and limits remain UNSET.
- Exact Unity editor patch remains UNSET until actual toolchain readback.

## Failure behavior
Missing required secret/config => fail closed with explicit configuration error.
Unknown environment => startup/integration blocked; never assume production.
Staging/production credential mix => security failure; no fallback.
Unavailable optional integration => apply its documented degraded behavior without mutating gameplay truth.

## Figma quality impact
PASS: configuration boundary is presentation-independent and preserves canonical Figma freedom.

## Evidence boundary
Logical separation is specified. Actual service accounts, secret stores, staging/prod projects, Unity build configurations and environment isolation tests are NOT RUN / NOT ESTABLISHED.
