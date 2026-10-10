# I18n UI binding acceptance — 2026-10-11
Scope: approved AWU 2. External ko/en JSON supports 18 trait names and three main-menu captions plus account EXP. This is a foundation, not a complete translated build.

## Implemented
- Stable trait localizationKey; LocalizedContentNames resolves all 7 Origins / 7 Classes / 4 Jokers. Legacy definitions without keys remain resolvable.
- Monster-name resolver uses monster.MON_*.name if supplied, otherwise existing name. No 120-monster translations supplied.
- Unity Resources adapter and event-driven Text binding with subscribe/unsubscribe lifecycle.
- Main buttons: Summon / Formation / Skills. EXP formatted through external localized template.
- Existing enum values, IDs, battle formulas, UI anchors and protected font assets unchanged.

## Actual verification
- Core red 4 missing-implementation tests, then complete 162/162 pass, zero skipped. Logs in ../I18n-Binding-20261011.
- Native Unity 6000.3.25f1 FirstPlayable PlayMode, fixed 390x844, ko -> en -> ko.
- Actual label readback, 18 trait resolver checks, screenshot capture and visual inspection of Korean and English. Korean glyphs readable.
- Lifecycle checks: disabled binding / switch / enable; inactive attachment; new-key rebinding; destroyed subscriber then switch.
- Screenshots: ko-390x844.png, en-390x844.png, ko-return-390x844.png.
- Initial probe failed before language checks because static reflection flags were omitted. A retry was issued before refreshed assembly loaded and also failed. Flags corrected, compilation refreshed, subsequent native runs succeeded. First failure retained; not a game compile/runtime defect.
- Independent read-only review found no critical/important code defects. Minor additional malformed-table/custom-key test coverage deferred.

## Boundaries and observations
Language switched through API/test probe; no player language-selection screen, persistence, device autodetection, RTL/plural support or mobile build/device test. Remaining menus/popup/HUD text intentionally still Korean. Existing upper-right menu buttons cover part of top HUD at 390x844; this order did not change that layout. EXP and selected labels remain readable. Purchased assets were not imported; no all-120 or skill translation, combat synergy calculation, mileage UI or save migration.

Acceptance is limited to the manager and selected integration. Game remains in asset payment/download/Import waiting state.
