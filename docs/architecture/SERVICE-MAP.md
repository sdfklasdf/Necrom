# Service Responsibility Map

Status: CURRENT DESIGN CONTRACT

Dependency direction:
Presentation/Input -> Application Use Cases -> Domain Services -> Ports -> Adapters.

Domain code does not depend on Unity UI, Figma, Supabase, RevenueCat, AdMob, Sentry, PostHog or persistence implementations.

Core responsibilities:
- Combat: combat rules/results.
- Raise: raise eligibility/result.
- Army: owned-undead and formation rules.
- Progression: region/stage/boss progression.
- Persistence application service: save/load/recovery coordination.
- Identity application service: player context through a port.
- Event application service: versioned events through a port.

Q4 adapter target:
Supabase may implement identity, remote persistence, trusted functions and reconciliation ports. It does not become the domain model.

Q5 adapter targets:
RevenueCat, AdMob, Sentry and PostHog remain integration adapters.

No duplicate truth:
Presentation and analytics are mirrors, not authority. Remote persistence records canonical remote state only after Q4 authority rules are actually implemented.

Evidence boundary:
Core gameplay has separate runtime evidence. Supabase/Q5 provider services are selected but NOT IMPLEMENTED.
