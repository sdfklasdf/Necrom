# Service Responsibility Map

Status: DESIGN_CONTRACT
Lifecycle: 06-06

Dependency direction:
Presentation/Input -> Application Use Cases -> Domain Services -> Ports -> Adapters.
Domain code does not depend on Unity UI, Figma, backend vendors, analytics SDKs or persistence implementations.

| Service | Single responsibility | Owns state changes | External dependency |
|---|---|---|---|
| Combat | Resolve combat session rules/results | Combat | none directly |
| Raise | Validate and execute raise eligibility/result | RaiseSource, Undead acquisition | persistence/event ports only through application layer |
| Army | Formation and owned-undead progression rules | Undead, Formation | persistence port |
| Progression | Region/stage/boss progression | Region/Stage, Boss, Progression | persistence port |
| Persistence application service | Coordinate save/load/version/recovery | Save snapshot lifecycle | IGameStateStore |
| Identity application service | Resolve current player context | no gameplay state | IPlayerIdentity |
| Event application service | Emit versioned domain/application events | no gameplay truth | IGameEventSink |
| Operations recovery | Controlled recovery workflow only | only explicitly recoverable state | audit/ops adapter when implemented |

## No duplicate truth
Presentation mirrors state but is not authoritative. Analytics mirrors events but is not authoritative. Persistence serializes authoritative domain state but does not invent gameplay transitions. Future server adapters enforce remote trust/authorization without duplicating gameplay rules unless an explicit server-authoritative migration is approved.

## Figma isolation
Visual components/tokens may change after Figma resumes without changing domain service contracts. UI view models may adapt, but design values never become domain/service constants.

## Current non-claims
No service above is proven implemented or runnable by this document. No live ops staff, backend provider or server runtime is established.
