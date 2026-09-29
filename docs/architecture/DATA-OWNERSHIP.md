# Data Ownership Table

Status: DESIGN_CONTRACT
Lifecycle: 06-07

| Entity | Canonical writer in current domain | Readers | Delete/reset authority | Truth boundary |
|---|---|---|---|---|
| Player progression | Progression service | UI, persistence, events | reset TBD | local domain now; remote canonical store conditional |
| Enemy instance | Combat service | combat UI, Raise service | combat resolution | combat session |
| RaiseSource | Raise service | raise UI | consume/expire by Raise rules; expiry TBD | Raise service |
| Undead | Army/Raise services with non-overlapping commands | UI, combat, persistence | removal TBD | owned-army domain |
| Formation | Army service | UI, combat, persistence | player remove/swap; delete semantics TBD | Army service |
| Combat | Combat service | UI, events, progression | resolved by Combat | combat session |
| Skill state | Combat service | UI | session lifecycle | Combat service |
| Region/Stage | Progression service | UI, combat setup, persistence | not player-deletable | Progression service |
| Boss encounter | Combat + Progression through explicit result handoff | UI, events | encounter resolution | Combat result then Progression consumes result |
| Save snapshot | Persistence application service | recovery/load | reset/recovery TBD | persistence adapter; not gameplay rule source |
| Account/Session | future trusted identity service | client identity adapter | provider/policy TBD | NOT IMPLEMENTED |
| Cloud save/profile | future trusted persistence service | client sync adapter | policy TBD | NOT IMPLEMENTED |

## Write rule
One command has one authoritative writer. Cross-domain changes happen through explicit results/events/use cases, never by a UI or analytics adapter mutating another service's state.

## Design-system separation
Figma tokens, component variants and visual assets own presentation semantics only. They never own gameplay/entity data. A later Figma redesign therefore cannot require migration of gameplay truth merely to reach final visual quality.

## Unresolved lifecycle
Undead removal, save reset/delete/recovery and account deletion remain TBD/NOT IMPLEMENTED exactly as upstream evidence states.
