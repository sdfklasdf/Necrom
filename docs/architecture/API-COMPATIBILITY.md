# API Compatibility Policy

Status: DESIGN_CONTRACT
Lifecycle: 06-10
Depends on: ADR-0001, SYSTEM-BOUNDARY, API-CONTRACT

## Versioning
Public remote contracts use a major path version (currently design target /v1) plus schemaVersion in envelopes. No deployed v1 is claimed.

## Compatibility rules
1. Additive optional fields are preferred within a major version.
2. Clients must ignore unknown response fields unless a security invariant requires rejection.
3. Existing required request fields are not removed or reinterpreted within a supported major version.
4. New required behavior ships through capability/version negotiation or a new major contract.
5. Deprecated fields have a documented replacement and observation window before removal.
6. Error codes remain machine-stable; new codes may be added, but existing codes are not silently repurposed.
7. Persisted payload/schema changes use explicit migration/version handling; old data is never assumed compatible without a migration path.

## Client support boundary
Exact minimum supported client version and minimum OS are UNSET until release/toolchain evidence exists. A future server must be able to reject unsupported clients explicitly rather than producing undefined behavior.

## Expand -> migrate -> contract
For breaking data/API evolution:
1. expand server/schema to accept old + new,
2. migrate/observe,
3. move clients,
4. contract only after supported old clients/data are outside the compatibility window and rollback conditions are satisfied.

## Figma/component compatibility
Figma component/token versions evolve independently from network API versions. Deferring Figma MUST NOT freeze temporary UI as compatibility surface. When paid Figma work resumes, visual token/component changes may be applied without an API major bump unless the semantic state/behavior contract itself changes.
