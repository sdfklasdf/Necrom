# FIRST PLAYABLE Domain Invariant Test Specification

Status: executable Unity-independent Core coverage established for the local FIRST PLAYABLE scope. Unity/editor/device and remote-authority behavior remain outside this evidence.

## Evidence boundary
- The executable harness is `Tests/Necrom.Core.Tests/Necrom.Core.Tests.csproj` on .NET 8 + NUnit in GitHub Actions.
- Latest pre-closeout execution baseline: run 36684460541, Failed 0 / Passed 46 / Skipped 0 / Total 46.
- This Core harness is not Unity compile, Unity Test Framework, Unity build, device/runtime, server transaction, distributed concurrency, or remote idempotency evidence.
- Exact Unity editor patch remains UNSET and runnable Unity build remains NOT ESTABLISHED.

## Executable coverage map
- Combatant invariants: `CombatantInvariantTests.cs`.
- Battle transition/revision invariants: `BattleStateInvariantTests.cs`.
- Raise-source lifecycle/revision invariants: `RaiseSourceInvariantTests.cs`.
- Formation slot/duplicate/revision invariants: `FormationInvariantTests.cs`.
- Raise + formation preflight/local atomic-boundary invariants: `RaiseFormationInvariantTests.cs`.
- Local duplicate/retry and event-metadata-before-mutation invariants: `CommandReplayInvariantTests.cs`.
- Snapshot/hydration/migration invariants: `SnapshotContractTests.cs`.
- Persistence recovery invariants: `PersistenceRecoveryTests.cs`.
- Bootstrap/checkpoint persistence edge invariants: `CheckpointBootstrapEdgeTests.cs`.
- Existing bootstrap/checkpoint happy paths remain in `FirstPlayableBootstrapTests.cs` and `FirstPlayableCheckpointTests.cs`.

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

### Persistence/bootstrap/checkpoint
1. Invalid snapshots are rejected before codec/store mutation.
2. Null codec serialization cannot reach the store.
3. Save failures and cancellation propagate and are not reported as success.
4. Corrupt/incompatible saves do not silently become a fresh game.
5. Invalid target schema rejects before store load.
6. A missing save may create a new state, but a null new-state factory result is rejected.

### Remote-authority boundary
If remote authority is introduced, source consumption and formation mutation require one authoritative transaction. The local serialized-command proof is not server atomicity evidence. Local stale-revision/replay tests are not a remote idempotency store, TTL, lock, database transaction, or distributed concurrency proof.

## DEC-037
All assertions target domain/application/persistence semantics only. No visual value, Figma variable, codeSyntax, asset, layout, focus style or motion behavior is canonized here. Canonical Figma can later represent semantic states without core domain/persistence rewrite.
