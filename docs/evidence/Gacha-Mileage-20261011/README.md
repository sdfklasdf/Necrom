# AWU-GACHA-MILEAGE-01 — 2026-10-11 KST
Approved: refactor 200-draw guarantee foundation to Origin OR Class selection; manager/data only.
Baseline AWU1 commit61e09db900e86569449b4157cae47cd0e831342b; original order baseline2fbf267.
## Implemented / provisional policy
- MonsterGachaManager supports explicit SelectionMileage mode. Existing constructors keep LegacyRandomPity for untouched UI/PlayerPrefs compatibility. This is NOT live UI rollout.
- Each successful paid random draw adds one cumulative mileage count; every 200 adds one ticket. Random Legendary never erases cumulative mileage. Failed payment adds nothing. Ordinary Legendary random band remains 9800..9999/10000 (2%).
- Canonical Origin OR Class filter only; Joker/alias/invalid selectors rejected. Candidate IDs come only from matching Legendary catalog entries. Redeem exact chosen ID; unmatched/empty pool or no ticket yields null without consuming.
- Successful selection grants the existing PermanentMonsterRoster; duplicate becomes a fragment. Redemption consumes one ticket, not diamonds; does not count as a new random draw.
- This accrual/rarity/exact-ID policy is a technical draft for the approved foundation, not final economic design. Production 120-unit TFT assignments have NOT been authored.
- Version1 GachaMileageState snapshot: successfulDraws + redeemedTickets. Available tickets derived = floor(draws/200)-redeemed, progress=draws%200. Invalid/negative/unknown-version/overspent states rejected before overwrite; defensive copies.
- Overflow preflight before payment, catalog deep copy for selection eligibility, defensive reward copies. Single-owner in-memory manager; no cross-process/monetized authority claim.
## Actual verification
- RED:13 new tests failed because mileage mode absent (red.txt).
- GREEN: full Core suite149 passed, zero failed/skipped (final-green.txt; initial green.txt=144). net8.0 testhost on installed.NET10 using process-local DOTNET_ROLL_FORWARD=Major.
- Native Unity6000.3.25f1 compiled new data/manager/probe. Isolated QA catalog/roster: failed payment unchanged; 198+10=208 -> ticket1/progress8, one payment; JsonUtility roundtrip; exact Abyss Legendary selected; repeated redemption refused (unity-validation.txt).
- snapshot.json readback = schemaVersion1/successfulDraws208/redeemedTickets1.
## Boundaries
No actual asset Import, battle/synergy damage, UI wiring, PlayerPrefs/save writes/migration, mobile build/device test.
Legacy pity cannot silently migrate to mileage because consecutive pity counts do not encode historical draw totals. Durable atomic diamond/roster/mileage save + replay/conflict protection remain a separate approved AWU before activation.
Existing user font3 dirty + Tests/bin untouched/excluded; source catalogs/dummy/QuarterView3D/Packages and GachaUIController/PermanentRosterSaveManager unchanged.

Code review: no important defects under single-thread ownership. Found callback reentrancy edge; RED2 failed preguard, then mutation/restore guards implemented. Full suite149 passed, including guard release, redemption overflow and reward mutation isolation. Review of final guard found no important flaws. Wallet callback must not directly mutate roster. No durable transaction claim.
