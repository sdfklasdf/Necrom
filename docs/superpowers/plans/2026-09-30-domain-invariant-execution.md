# FIRST PLAYABLE Core Invariant Execution Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans inline. Apply superpowers:test-driven-development to new behavior/bugfixes and superpowers:verification-before-completion before PASS claims.

**Goal:** Convert the remaining WP-193 FIRST PLAYABLE domain invariants and persistence edge cases into executable Unity-independent .NET Core regression evidence without changing Unity/Figma/provider boundaries.

**Architecture:** Keep tests in the existing net8.0 NUnit harness. Domain/application/persistence production code is changed only if an executable test exposes a real defect. Local serialized execution remains explicitly non-authoritative for remote concurrency/idempotency.

**Tech Stack:** C#, .NET 8 test harness, NUnit, GitHub Actions. Unity 6.3 LTS family is direction only; exact Unity editor patch remains UNSET.

**Spec:** docs/testing/DOMAIN-INVARIANT-TEST-SPEC.md plus Lifecycle 06-11/06-12 and DEC-037.

## Global Constraints
- Maximum seven independently reviewable actual work units.
- No Unity compile/test/build claim from Core .NET evidence.
- No backend/provider/serializer selection.
- No remote transaction/idempotency claim from local serialized execution.
- No visual values, Figma variables/codeSyntax, assets, layout, focus, motion, or presentation-domain coupling.
- Breaking changes only reopen actual direct/transitive consumers.

## Review Focus
- Stale revision must reject without mutation.
- Rejected raise/formation preflight must leave source Available.
- Duplicate/retry commands must not create a second gameplay effect locally.
- Persistence failures/cancellation must propagate rather than report success.
- Existing successful transitions and FIRST PLAYABLE readiness must remain intact.

## Work Units
- [ ] WP-213 Combatant executable invariants.
- [ ] WP-214 Battle state transition/revision executable invariants.
- [ ] WP-215 RaiseSource lifecycle executable invariants.
- [ ] WP-216 Formation revision/slot/duplicate executable invariants.
- [ ] WP-217 RaiseIntoFormation preflight/atomic-boundary executable invariants.
- [ ] WP-218 Local duplicate/retry + event-metadata mutation-order executable invariants.
- [ ] WP-219 Checkpoint/bootstrap persistence edge executable invariants.

Each unit: read direct dependencies -> DEC-037 impact check -> add test -> GitHub actual readback -> GitHub Actions full suite -> PASS/PARTIAL/BLOCKED/FAIL -> downstream review.
