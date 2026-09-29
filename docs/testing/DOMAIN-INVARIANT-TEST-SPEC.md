# FIRST PLAYABLE Domain Invariant Test Specification

Status: specification only. No test runner or Unity toolchain is established.

## Evidence boundary
- This document is not a test execution result.
- Compile, typecheck, unit test, Unity test and build remain NOT RUN until an executable toolchain exists.
- The cases below become executable acceptance tests when a C# test harness is established.

## Invariants

### Combatant
1. Positive damage reduces health.
2. Lethal damage sets health to zero and life state to Defeated.
3. Damage after Defeated has no second effect.
4. Non-positive damage is rejected.

### Battle state
1. Ready -> Running -> Victory|Defeat -> Resolved is allowed.
2. Any stale expectedRevision is rejected.
3. Start outside Ready is rejected.
4. Resolve outside Running is rejected.
5. Finalize outside Victory|Defeat is rejected.

### Raise source
1. Only a Defeated combatant can create a RaiseSource.
2. Available -> Consumed occurs exactly once.
3. Duplicate consume and stale revision are rejected.

### Raise + formation
1. Raised unit preserves source archetype and becomes Player faction.
2. One unit cannot occupy two formation slots.
3. Occupied target slot rejects another unit.
4. Stale formation revision is rejected.
5. RaiseIntoFormation preflights source and formation before consuming the source.
6. With serialized local commands, a successful raise produces a unit discoverable by IsReadyForNextCombat.
7. A rejected preflight leaves the RaiseSource Available.

### Remote-authority boundary
If remote authority is introduced, source consumption and formation mutation require one authoritative transaction. The local serialized-command proof is not server atomicity evidence.

## DEC-037
All assertions target domain semantics only. No visual value, Figma variable, asset, layout, focus style or motion behavior is canonized here.
