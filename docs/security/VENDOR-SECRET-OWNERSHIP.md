# Vendor & Secret Ownership Register — Lifecycle 06-16

Status: PARTIAL / NO SECRET MATERIAL.

This register records only evidenced or intentionally unselected ownership. It contains no tokens, passwords, signing keys, or secret values.

| Capability | Vendor/account state | Owner | Secret storage/rotation |
|---|---|---|---|
| Source repository | GitHub sdfklasdf/Necrom — connector read/write evidenced | Founder | connector/OAuth managed externally; secret values not stored here |
| Project canon | Google Drive project board — read/write evidenced | Founder | connector/OAuth managed externally |
| Game engine/editor | Unity 6.3 LTS family selected; actual editor account/toolchain patch NOT ESTABLISHED | Founder / Engineering | TBD after actual toolchain/account readback |
| Backend/auth/database | UNSELECTED / NOT IMPLEMENTED | Founder approval required | TBD; client storage forbidden |
| Analytics/crash | UNSELECTED | Founder approval required | TBD |
| Push/search/CDN/media | NOT REQUIRED for current first playable / UNSELECTED | Founder approval required | TBD |
| iOS/Android store signing | NOT RUN / account and signing ownership not evidenced here | Founder | TBD; private signing material never repository |
| Figma | canonical file exists; current Starter MCP blocker | Founder / Design | connector-managed; no secret values in repo |

## Ownership rules
- Founder approves purchase/provider lock where required by project policy.
- Operational secret injection/rotation owner must be named before a provider is production-enabled.
- Client receives only public/client-safe identifiers; privileged keys remain trusted-side.
- Rotation must support overlap/revocation without requiring gameplay-domain rewrite.
- Suspected exposure => revoke/rotate first, then investigate; never preserve a compromised credential for compatibility.

## Figma quality impact
PASS: vendor/secret ownership is isolated from presentation. No visual implementation or token value is locked here.

## Evidence boundary
Because 00-10 remains PARTIAL, this Lifecycle 06-16 artifact is PARTIAL. It is not evidence of real vendor accounts, secret stores, key rotation, store signing, backend, or production environment separation.
