# FIRST PLAYABLE Snapshot Codec Compatibility Contract

Status: CONTRACT_ONLY — codec implementation is intentionally deferred until the actual Unity project/editor compatibility surface is verified.

## Required semantics
- Round-trip every field in `GameStateSnapshot` without changing schema version, battle phase/revision, formation revision, slot index, or unit identity.
- Reject malformed payloads; malformed or incompatible saves must never silently become a fresh game.
- Preserve unknown-schema failure until an explicit registered migration path exists.
- Serialization failure must occur before store mutation.
- Deserialization must not mutate domain state; validation and migration happen before hydration.
- Encoding must be deterministic enough for diagnostics, but byte-for-byte canonical serialization is not yet required.

## Compatibility gate before choosing an implementation
1. Read the actual Unity project metadata and exact editor patch.
2. Verify the serializer/API is supported by that Unity runtime/API compatibility level and target platforms.
3. Run the .NET Core regression suite.
4. Run Unity EditMode tests against the same fixtures.
5. Run at least one Android and one iOS save/load round trip before production persistence PASS.

## Explicit non-decisions
- No serializer library/provider is selected here.
- No JSON/Binary/MessagePack format is canon.
- No backend/database/cloud provider is selected.
- No remote-authoritative storage, transaction, locking, sync, or idempotency claim is made.

## DEC-037
The codec transports presentation-independent state only. It contains no Figma variables, codeSyntax, visual tokens, assets, layout, animation, focus, or accessibility shortcuts. A later canonical Figma implementation must not require persistence/domain rewrites.
