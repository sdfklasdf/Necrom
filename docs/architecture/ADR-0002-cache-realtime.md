# ADR-0002 — Cache and Realtime Necessity — Lifecycle 06-14

Status: DESIGN DECISION.

## Decision
Do not introduce distributed cache or realtime network infrastructure for the current local-first first playable. Preserve seams for future online authority.

## Basis
03-14 has measurement-ready metrics but numeric thresholds remain UNSET_PENDING_BASELINE. 06-02/06-11 define local-first gameplay and explicit state revisions. 06-13 has no required external realtime integration.

## Path decisions
- combat simulation/state: process-local authoritative state for current first playable; no network cache.
- formation/army/progression: local domain state + persistence snapshot; invalidated by successful domain mutation/revision.
- presentation projections: may cache derived view state only; invalidated from domain revision/event, never treated as truth.
- remote account/cloud state: NOT IMPLEMENTED; no cache/TTL invented.
- realtime multiplayer/chat/guild: OUTSIDE current MVP; no realtime transport selected.

TTL for authoritative gameplay data: NOT APPLICABLE in current design. If remote caches are introduced later, TTL, invalidation owner, per-user isolation, stale-read tolerance and outage fallback become mandatory before implementation.

## Failure cases
- stale presentation projection after domain revision: discard/rebuild projection.
- future remote cache unavailable: authoritative path must be explicit; never promote stale cache to ownership truth.
- cross-player cache key collision: prohibited; future remote cache requires actor/tenant isolation.

## Figma quality impact
PASS: presentation caching contains semantics only and cannot constrain future Figma visual fidelity.

## Evidence boundary
No measured latency, cache hit rate, realtime transport, backend, load test, or runtime performance result exists.
