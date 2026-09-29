# Data Map — Lifecycle 07-01

Status: DESIGN_CONTRACT
Baseline: CP-NECRO-117 / 03-08 / 03-15 / 06-05 / 06-07

## Current first-playable flow

| Data class | Collection / creation | Current processing | Current storage | Readers | Deletion / end-of-life | Evidence boundary |
|---|---|---|---|---|---|---|
| Local player identity key | created locally for first playable | ownership key for local state | local persistence adapter, implementation not yet established | gameplay application services | reset/delete semantics TBD | no account/session server exists |
| Progression / region state | gameplay outcomes | Progression service | local persistence target | UI, combat setup, persistence/events | reset/recovery TBD | no cloud canonical store |
| Enemy / combat state | runtime gameplay | Combat service | combat session; persistence only if explicitly snapshotted later | combat UI/events | encounter resolution | runtime implementation NOT ESTABLISHED |
| RaiseSource | enemy defeat | Raise service | domain state / optional save snapshot | raise UI | consumed/expired; expiry TBD | design state only |
| Owned undead / formation | raise + army commands | Army/Raise services | local persistence target | UI/combat/persistence | removal TBD | no remote ownership store |
| Save snapshot | persistence request | persistence application service | provider-neutral local adapter target | recovery/load | reset/recovery TBD | actual serializer/store NOT IMPLEMENTED |
| Analytics candidate events | domain emits through event seam | provider-neutral event sink | no analytics provider selected | none in production | retention TBD | UNSELECTED / NOT RUN |
| Account/session | not collected now | future trusted identity service only | future trusted service | client identity adapter | policy TBD | NOT IMPLEMENTED |
| Cloud save/profile | not collected now | future trusted persistence only | future provider | client sync adapter | policy TBD | NOT IMPLEMENTED |
| Payment/entitlement | not in current free-validation MVP | future trusted service only | future provider | entitlement adapter | policy TBD | NOT IMPLEMENTED |

## Collection-to-deletion continuity
For current MVP scope, gameplay data begins at local gameplay creation, flows through explicit domain services, and terminates in session resolution or the provider-neutral persistence boundary. Destructive reset/delete/recovery is intentionally unresolved upstream and is therefore not invented here.

Future account, cloud, analytics and payment data are conditional lanes, not current collection. They require a new provider decision, privacy/legal revalidation and explicit deletion/retention rules before activation.

## Trust and ownership
Presentation does not own or mutate canonical gameplay truth. Future remote authority follows SYSTEM-BOUNDARY and TRUST-MODEL. Provider SDK objects cannot become domain entities.

## DEC-037 Figma quality impact
PASS. This map introduces no visual value, token, asset, component geometry, motion or accessibility shortcut. UI remains a replaceable reader of domain/application state.
