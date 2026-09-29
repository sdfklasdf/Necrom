# ADR-0001 — Client technology stack

Status: APPROVED_BY_FOUNDER
Lifecycle: 06-04
Date: 2026-09-30

## Decision
Use Unity 6.3 LTS family with C# as the client/gameplay foundation for the mobile-first first playable and MVP.

The exact Unity editor patch is intentionally not pinned until the editor/toolchain is installed and read back. No runnable build is claimed by this ADR.

## Product constraints
- iOS and Android first.
- Portrait, one-hand play.
- Army-command idle/growth RPG with real-time combat state, entity lifecycle, animation/effects and scalable content.
- First playable must preserve kill -> raise -> army ownership -> next-combat participation -> persistence.
- 100k+ users is a target, not measured demand. Persistence, identity, network/server and event seams remain open.
- PvP, guild, chat, realtime co-op, marketplace and full live-ops remain outside current MVP unless separately approved.

## Alternatives considered
- Current repository state (README only): reversible but cannot implement the game.
- Expo / React Native: strong mobile application UI/tooling, but gameplay/render loop would rely on additional graphics/game abstractions.
- Godot 4: capable game engine and mobile export; C# mobile support remains a risk area compared with the selected path.
- Unity: dedicated game engine, mature mobile export/runtime, 2D/3D/gameplay tooling and C# domain separation.

## Boundaries
- No Figma variable values or codeSyntax are inferred.
- Figma token mapping remains partial until an actual code-token source is compared with Figma.
- Backend/auth/RLS/API/cloud sync/webhook providers are not selected by this ADR.
- Physical device, accessibility technology, runtime performance and store builds remain NOT RUN.
- Production font/asset rights remain NOT CLEARED.

## Rollback
Before engine-specific gameplay implementation becomes material, this decision can be superseded by a new Founder-approved ADR. Only actual consumers of this ADR become stale.
