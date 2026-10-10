# AWU-I18N-CORE-01 — 2026-10-11 KST
Baseline main3d17f6a0ef428286256bd24df6b64c101d44196d; three existing font dirty assets and Tests/bin preserved.
Approved global one-build foundation, external data ko/en only; no120-unit full translation or asset Import.
- Engine-independent LocalizationManager consumes serializable table DTO, validates schema/languages/keys/default translation and duplicates; copies data into private lookup dictionaries.
- Default ko; explicit runtime language switch, regional en-US -> en; unsupported language leaves prior choice; same selection does not fire redundant event.
- Missing translation -> default ko; missing key -> visible [key]. Format uses selected culture and caller-supplied values.
- External Resources/Localization/necro-localization.json contains22 keys:18 trait test names+4 main UI test strings. These are provisional copy, not approved final translations.
- RED9 feature-missing tests -> full Core suite158 pass/0fail/0skip. Actual net8 testhost uses process-local DOTNET_ROLL_FORWARD=Major on installed.NET10; project/install unchanged.
- Native Unity6000.3.25f1 Resources JSON parsing / Core switching / JsonUtility roundtrip / all18 trait keys verified by explicit Editor probe.
- UI attachment belongs to next AWU; no gameplay/save/currency/actual character content mutations.
- No language settings persistence, automatic device-language choice, RTL, pluralization, translator workflow, platform build/device/global-release claim.
