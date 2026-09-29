# Data Minimization Table — Lifecycle 07-02

Status: DESIGN_CONTRACT
Baseline: DATA-MAP / current Founder-approved MVP scope

| Purpose | Minimum data needed now | Explicitly not required now | Activation gate |
|---|---|---|---|
| First-playable ownership | local opaque player key | real name, email, phone, social identity, contacts, precise location | account feature decision |
| Progression continuity | stage/progression identifiers and values required by gameplay | demographic profile, advertising identifiers | none beyond persistence implementation |
| Undead army continuity | undead instance/type/state and formation references needed to restore army | social graph, device contacts, unrelated device data | none beyond persistence implementation |
| Combat continuity | only state explicitly required by save/checkpoint contract | raw sensor data, media library, microphone, camera | explicit feature decision |
| Diagnostics | no provider data currently required | device advertising id, account PII | analytics/crash provider decision + privacy review |
| Cloud sync | none currently | account credentials or remote profile | backend/auth/provider decision + 07 privacy/security revalidation |
| Monetization | none in current free-validation MVP | payment instrument data | monetization/provider decision + legal/security gates |

## Rule
A field is not collected merely because a future provider can collect it. New fields require a named processing purpose, owner, retention/deletion rule and a revalidation of downstream privacy/security consumers.

Unknown future fields remain absent, not placeholders populated with fabricated values.

## DEC-037 Figma quality impact
PASS. Data minimization is independent of visual design. Future consent/account/diagnostic UI must still be implemented from canonical Figma states without weakening accessibility, focus, responsive or motion requirements.
