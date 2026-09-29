# Authorization Matrix — Lifecycle 07-06

Status: DESIGN_CONTRACT
Baseline: 03-07 permission requirements / 06-08 TRUST-MODEL

| Actor | Resource / action | Current local rule | Future remote enforcement point | Cross-user rule |
|---|---|---|---|---|
| PLAYER | own progression read/write through gameplay use cases | allowed through domain/application services | trusted API + server-side ownership policy/RLS-equivalent | deny other-player canonical state |
| PLAYER | own undead/formation commands | allowed through Army/Raise application boundary | trusted API derives actor from verified session | resource owner id supplied by client is claim only |
| PLAYER | combat actions | allowed for active local session | authoritative service if online mode later requires it | cannot target another player's canonical progression |
| PLAYER | project/Founder approval | never allowed | no gameplay API route | deny |
| FOUNDER | project governance decisions | outside game runtime | separate governance system, not implicit game admin | not mapped to runtime superuser |
| OPS | operational mutation | NOT IMPLEMENTED | distinct privileged audited path if introduced | cannot inherit Founder authority implicitly |
| DEBUG/DEV | local development identity | development only | prohibited as production authority | deny in production |
| PROVIDER CALLBACK | domain mutation | N/A now | signature/authenticity + replay/idempotency verification before command | no direct canonical mutation |
| ANALYTICS | gameplay truth mutation | prohibited | ingestion-only adapter | deny |

## Server/RLS verification points
A selected backend must implement equivalent trusted-side checks for: authenticated actor resolution, ownership/resource authorization, privileged-role separation, canonical write validation, idempotency/replay handling, and auditability. RLS is not assumed until a provider is selected.

## Evidence boundary
No server, RLS policy, auth provider, account system or remote database is implemented. This is the required authorization contract, not runtime verification.

## DEC-037 Figma quality impact
PASS. Authorization semantics expose denied/loading/conflict/offline outcomes without prescribing their visual form. Canonical Figma can later define all variants, focus, accessibility, responsive and motion behavior without core rewrite.
