# Technology Account Inventory — Lifecycle 00-10 Revalidation

Checkpoint target: CP-NECRO-117
Date: 2026-09-30
Status: PARTIAL — verified services are separated from unverified/unselected services.
No secret/token/password material is stored here.

| Capability | Project identifier / scope | Owner | Actual access evidence | Status |
|---|---|---|---|---|
| Source control | GitHub `sdfklasdf/Necrom`, branch `main` | Founder | connector branch read succeeded at `6f00e7d6f1c0cfc3a45197427ab48789f46e8c77`; prior and current project writes evidenced | VERIFIED_READ_WRITE |
| Canon / project board | Google Drive spreadsheet `1zLhQQ3pTgjLAkuNMSDNLQtFYu2hS3Nsz5O5O2kPGYnw` | Founder | metadata/cell reads and project-board writes/readbacks succeed | VERIFIED_READ_WRITE |
| Design | Figma canonical file `eXqKU1qHXsn52SJfIGltZo` | Founder / Design | current MCP request returned Starter-plan MCP call-limit paywall; CP-109 remains prior evidence only | ACCESS_BLOCKED_RATE_LIMIT / NOT_FRESH_READ |
| Engine/editor | Unity 6.3 LTS family + C# approved architecture | Founder / Engineering | no connected Unity account/editor/toolchain readback available in current environment | UNVERIFIED; exact patch UNSET |
| Backend/auth/database | none selected for Necrom | Founder approval required | no provider selected; no project/account readback applicable | UNSELECTED |
| Analytics/crash | none selected for Necrom | Founder approval required | no provider/project evidence | UNSELECTED / UNVERIFIED |
| Android distribution | Google Play / signing | Founder | no project-specific account/console/signing evidence read | UNVERIFIED / NOT RUN |
| iOS distribution | Apple Developer/App Store Connect/signing | Founder | no project-specific account/console/signing evidence read | UNVERIFIED / NOT RUN |
| Search/push/CDN/media | not required for current first playable | Founder approval if introduced | no provider selected | NOT REQUIRED CURRENT FP / UNSELECTED |

## 00-10 decision
Lifecycle 00-10 remains PARTIAL. Its completion condition requires owner and access level for the actually available technology accounts. GitHub and Drive satisfy that requirement for their scopes; Figma fresh access is blocked; Unity/editor and distribution accounts are not evidenced; backend and analytics are not selected.

## Downstream revalidation
- 06-15 remains PARTIAL: logical environment contract exists, but actual dev/test/staging/production service-account separation is not established.
- 06-16 remains PARTIAL: ownership rules exist, but actual key stores/rotation and unverified vendor accounts are absent.
- 06-17 remains PARTIAL: design plan exists, but no real persistence/backend migration, backup, restore, or rollback rehearsal exists.
- 06-18 is NOT READY: 06-15~17 are not fully evidenced and no backend/provider usage model is selected. Existing quoted prices elsewhere are not equivalent to a project-specific infrastructure cost ceiling.

## DEC-037 Figma quality impact
PASS. This inventory fixes no visual value, asset, token, codeSyntax, component geometry, motion, focus behavior, or accessibility shortcut. Figma remains canonical for later visual implementation.
