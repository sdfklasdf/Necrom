# 120-character content schema draft — AWU-ROSTER-SCHEMA-02
Decision: QDEC-026, Founder order 2026-10-09. Working title NECRO; final release name undecided.
Scope: schema and validation only. No character art import, no catalog overwrite, no FirstPlayable combat change.

## Existing implementation is the baseline
Assets/Resources/monster_catalog.json already has 120 MON_* entries with hp/attack/rarity/gachaWeight/element and 2–3 traits each.
SkillSynergyV3.cs already supports 1–3 distinct traits, six unique permanent-deck character IDs, and threshold-based effects.
synergy_rules_v2.json already has 20 rules; special tiers are partly SPEC_ONLY. StatCalculator and AutoDeckRecommender exist. This AWU does not claim to implement them or approve current coefficients.
Temporary Raise Formation and permanent six-unit deck are different systems. This draft does not change either capacity.

## Data contract
| Group | Source / fields | Rule |
| --- | --- | --- |
| Version | schemaVersion=1, expectedCharacterCount=120, contentRevision, balanceStatus | DRAFT status; explicit revision |
| Character / base stats | Existing MonsterCatalogData / MonsterEntry | Preserve MON_* ID, hp, attack, rarity, gachaWeight, element, traits; existing values provisional |
| Trait registry | traitIds | Stable IDs; each unit references 1–3 distinct known traits |
| Synergy | Existing SynergyCatalog/SynergyRule | Reuse trait, effectKey, ordered thresholds, values, statKeys; preserve original full source JSON |
| Asset / supplemental stats | CharacterContentProfile | characterId FK, familyId (optional), assetKey, UNASSIGNED/MAPPED, defense, attacksPerSecond, affinityId |
| Affinity | affinityIds + DirectedAffinityRule[] | Directed attacker/defender pair, finite nonnegative multiplier, no duplicate pair |
| Unity authoring | CharacterCatalogDraftAsset | TextAsset references + editable profile/registry arrays, ReadDraft validation view |

MonsterEntry remains the single source of base hp/attack/traits. Profiles do not duplicate those fields. Asset keys remain logical strings until the new asset inventory and license are provided; no Addressables package is installed.
MAPPED means a logical mapping exists, never imported, licensed or release verified.
No matchup entry means unspecified; this draft does not invent a neutral default or implement a combat multiplier.

## Bulk management / migration path (design only)
1. Map each incoming asset filename/key to an existing MON_* stable ID. Never renumber ownership saves.
2. Edit one profile row per character; a CSV export is supplied for 120-row review.
3. Validate duplicate IDs, foreign references, trait counts, positive stats/speed, affinity pairs and ordered synergy tiers.
4. Explicitly approve rules and coefficients, then implement a reviewed importer or ScriptableObject asset creation.
5. Keep source JSON and schemaVersion during migration; prove save compatibility before any production switch.
The current ScriptableObject wrapper reads existing TextAssets and preserves their full JSON contents. Its reduced validation view must NOT be serialized over the source catalogs, as extra SPEC_ONLY descriptions would be lost.

## Validation and status
- Test-first acceptance failed because the schema was missing.
- .NET suite: 120 pass / 0 fail / 0 skipped = existing 105 + 15 schema cases, under .NET10 Major rollforward. Native .NET8 unavailable.
- Unity compile, ScriptableObject.ReadDraft with real catalogs, JsonUtility roundtrip and bad-FK rejection are verified separately in unity-validation.txt.
- existing-catalog-draft.json / profile-review.csv are DRAFT exports. Supplemental defense=0/speed=1 are fixture placeholders, not approved game balance; asset mapping count zero.
- No new visual asset files, effects, matchup table, importer, runtime hookup, device/build or final balance validation.
- Local draft remains uncommitted after EXP commit, so it can be reviewed separately.

Required next inputs: new asset location/inventory/license, character-to-MON mapping, intended trait group labels/thresholds/effects, and directional matchup policy. Founder+Gemini decide those rules.
