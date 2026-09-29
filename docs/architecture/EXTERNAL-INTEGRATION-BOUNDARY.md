# External Integration Boundary — Lifecycle 06-13

Status: DESIGN CONTRACT / applicability review.

Current approved first playable has no required search service, remote media service, or notification service. Backend/provider remains UNSELECTED.

## Search
External search: NOT APPLICABLE to current first playable.
If future content search is approved: adapter boundary, bounded timeout, explicit unavailable state, no gameplay truth stored in search index.

## Media
Remote media delivery: NOT REQUIRED for current first playable. Production assets are NOT CLEARED and no CDN/provider is selected.
If introduced: media failure falls back to semantic placeholder/state; asset delivery cannot mutate gameplay truth.

## Notifications
Push notifications: NOT APPLICABLE to current first playable.
If later approved: notification is advisory only; opening a notification re-reads authoritative state. Delivery retry cannot duplicate rewards or state transitions.

## Shared external-service rules
- no secrets in client.
- timeout/retry policy belongs to adapter/config, not presentation.
- mutation retries follow 06-12 idempotency.
- external outage degrades the integration, not canonical gameplay ownership.
- provider SDK types cannot enter Domain.

## Failure counterexamples
- search unavailable: core combat/raise continues; no fake empty canonical result.
- media unavailable: gameplay state remains usable; presentation exposes missing-media semantics.
- duplicate notification: opening twice cannot duplicate domain mutation.

## Figma quality impact
PASS: failure/loading/empty semantics are preserved for later high-fidelity Figma treatment; no provisional visual styling is canonized.

## Evidence boundary
No external search/media/push provider, endpoint, credential, timeout measurement, or runtime fallback has been implemented or tested.
