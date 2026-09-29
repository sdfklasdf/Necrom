# Figma Deferral Quality Contract

Status: FOUNDER_CONSTRAINT
Applies from: CP-NECRO-115
Canonical Figma file: eXqKU1qHXsn52SJfIGltZo

## Non-negotiable invariant
Deferring Figma work is a sequencing optimization only. It may not reduce the final product's design quality, fidelity, accessibility, component completeness, motion/state quality or maintainability after paid Figma work resumes.

## Pre-Figma engineering acceptance gate
A pre-Figma change is acceptable only if all applicable statements are true:
- it does not invent Figma values or codeSyntax;
- it does not make temporary visual values/assets canonical;
- it keeps presentation replaceable through tokens/components/view models/adapters;
- it keeps gameplay/domain/data/network truth independent of visual styling;
- it preserves all semantic UI states needed for later high-fidelity design;
- it records any temporary visual implementation as disposable/provisional;
- it can accept canonical Figma tokens/components without rewriting core gameplay/domain architecture.

If a change fails this gate, it is BLOCKED until the relevant Figma work is available or the change is redesigned.

## Resume gate after Figma payment
Before visual implementation is considered complete:
1. fresh-read the canonical Figma file and component/token versions;
2. finish/verify the required component families and states;
3. establish actual Figma-to-code token mapping from verified sources;
4. diff provisional code/UI against canonical Figma;
5. replace provisional values/assets/components rather than preserving them for convenience;
6. revalidate responsive states, accessibility, focus, reduced motion and semantic states;
7. run supported runtime/device validation when a runnable build exists;
8. clear production font/asset rights before production use.

## Current evidence
Primary component 7:26 / 12 variants is prior CP-109 evidence only.
05-21 mapping remains PARTIAL.
05-22 remains blocked by Figma Starter MCP limit.
No final visual-quality PASS is claimed by this contract.
