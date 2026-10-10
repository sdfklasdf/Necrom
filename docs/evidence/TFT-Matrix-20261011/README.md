# AWU-TFT-MATRIX-01 — 2026-10-11 KST
Approved: 7 Origins / 7 Classes / 4 Jokers; schema only, no combat effects or asset Import.
Baseline main 2fbf267c4d924f3c177e8b7911dfac12c7d6fdf8. Protected dirty: three NotoSansKR SDF fonts + untracked Tests/bin.
- TftTaxonomy now supplies exactly 18 canonical string IDs; explicit typed enums support future consumers without changing serialized IDs.
- Ghoul remains a legacy Plague alias. Existing effect plans preserved; Abyss/Elemental/Bruiser/Mage/Supporter effects are EMPTY (unspecified), all tier numbers remain unapproved/empty.
- Validation rejects unknown IDs and category mismatches. Legacy canonical subsets remain readable; no existing catalog/assets were rewritten.
- RED: three new regression tests failed for missing matrix/traits/canonical validation (red.txt).
- GREEN: complete Core suite 131 passed, 0 failed/skipped (green.txt).
- Test project targets net8.0, PC has .NET10 only. Initial testhost abort retained in runtime-block.txt. Actual tests used process-local DOTNET_ROLL_FORWARD=Major; project/install unchanged.
- Native Unity 6000.3.25f1 compiled code; temporary ScriptableObject ReadDraft + JsonUtility roundtrip passed for 120 original MON IDs with in-memory TEST assignments covering all 49 Origin/Class combinations.
- schema-roundtrip.json is a TEST fixture, never runtime content or an approved character assignment table. Existing runtime synergy table is NOT TFT; test fixture uses empty synergy rules. Initial mismatched fixture error retained in probe-fixture-failure.txt, corrected and rerun PASS.
- An old compiled probe consumed the trigger before refresh; only its generated old evidence files were restored to their exact HEAD bytes. Existing user dirty never restored/reset.
- Unity validation output and schema JSON were read back. No real character assets, battle formulas, UI, or PlayerPrefs changes.
- Native scene remains FirstPlayable; no PlayMode gameplay or device/build claim for this data AWU.
