# FIRST PLAYABLE Persistence Contract

Status: provider-neutral local foundation. Compile/test/runtime evidence is not established.

## Save
1. Validate the complete snapshot before serialization.
2. Serialization failure aborts before store mutation.
3. Store failure propagates; callers must not display or record save success.
4. This contract does not claim durable storage, fsync, cloud durability, transactions, retries, or remote idempotency.

## Load
1. Missing local state may return no state.
2. Decode failure is an error, not a fresh-game success.
3. Validate decoded state before migration.
4. Migration must follow a registered path and validate every produced snapshot.
5. Validate again through hydration before exposing domain state.
6. Unknown schema, invalid enum, negative revision, duplicate slot, duplicate unit, capacity overflow, or malformed identity is rejected.

## Atomicity boundary
The in-memory adapter is serialized local FIRST PLAYABLE infrastructure only. It is not evidence for database transactions, distributed locking, cloud conflict resolution, or server authority. If remote authority is introduced, compound effects such as RaiseSource consumption + Formation mutation require one authoritative transaction under the 06-11 contract.

## Versioning
The migration registry contains no fabricated migrations. A migration must be explicitly registered and must change the declared schema version exactly as registered.

## DEC-037
Persistence/domain contracts contain no visual tokens, assets, layout, component styling, Figma variables or codeSyntax. Presentation remains replaceable.
